using System.Reflection;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-setup", SetupAsync),
        ("03-create", CreateAsync),
        ("04-source", SourceAsync),
        ("05-view", ViewAsync),
        ("06-types", TypesAsync),
        ("07-path", PathAsync),
        ("08-part", PartAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    // Creates the three seed articles, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        var articles = new (string Title, string Body)[]
        {
            ("Spring Planting Calendar", "Sow lettuce and peas in March, and start tomatoes indoors in April."),
            ("Five Herbs for a Windowsill", "Basil, chives, mint, parsley, and thyme grow well in pots by a bright window."),
            ("Composting Basics", "Mix kitchen scraps with dry leaves, and turn the pile every week."),
        };

        foreach (var (title, body) in articles)
        {
            await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Article/Create", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await Task.Delay(500);
            Console.WriteLine($"Seeding {title}");
            await page.Locator(".trumbowyg-editor").PressSequentiallyAsync(body, new LocatorPressSequentiallyOptions { Delay = 5 });
            await Task.Delay(500);
            await page.Locator("button.publish").First.ClickAsync();
            await page.WaitForURLAsync(url => !url.Contains("/Create", StringComparison.OrdinalIgnoreCase));
            await Task.Delay(800);
        }

        await context.CloseAsync();
    }

    // The recorder's cursor overlay adds a style element that an XML page (the sitemap) shows as plain text: remove it there.
    private const string XmlCleanScript = """
        (() => {
          const clean = (n) => {
            if (n.nodeType === 1 && !document.contentType.includes('html')
              && (n.id === 'demo-cursor' || n.localName === 'style' || (n.classList && n.classList.contains('demo-ripple')))) { n.remove(); }
          };
          new MutationObserver((records) => records.forEach((r) => r.addedNodes.forEach(clean))).observe(document, { childList: true, subtree: true });
        })();
        """;

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.Context.AddInitScriptAsync(XmlCleanScript);

        return scene;
    }

    // The scene clock (seconds of video so far), which the scene keeps private.
    private static double Now(Scene scene)
        => (double)typeof(Scene).GetProperty("Now", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(scene)!;

    // Holds until a step has this many seconds of narration left, so that a toast shown by the next click is still on screen at the end.
    private static async Task LeftAsync(Scene scene, double stepStart, IReadOnlyDictionary<string, double> durations, string id, double secondsLeft)
    {
        var wait = stepStart + durations[id] - secondsLeft - Now(scene);

        if (wait > 0)
        {
            await scene.HoldAsync(wait);
        }
    }

    // Clicks something that loads a page, and cuts the load out of the clip.
    private static async Task GoAsync(Scene scene, ILocator locator, int lead = 150)
    {
        await scene.MoveToAsync(locator);
        await Task.Delay(150);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), lead);
    }

    private static ILocator Group(IPage page, string title)
        => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator MenuLink(IPage page, string group, string text)
        => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{group}\"] a.item-label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*" + System.Text.RegularExpressions.Regex.Escape(text) + @"\s*$") });

    private static ILocator PagesRow(IPage page)
        => page.Locator("li.list-group-item, tr, .list-group-item").Filter(new() { HasText = "Pages" }).Last;

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        var sitemapsRow = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("code").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*OrchardCore\.Sitemaps\s*$") }) });

        await scene.StepAsync("setup-1", async () =>
        {
            var t0 = Now(scene);
            await scene.ClickAsync(Group(page, "Tools"));
            await GoAsync(scene, MenuLink(page, "Tools", "Features"));
            await page.Locator("#search-box").WaitForAsync();
            await scene.PauseAsync(600);
            await scene.TypeAsync(page.Locator("#search-box"), "Sitemaps", 4);
            await scene.PauseAsync(500);

            if (await page.Locator("#search-box").InputValueAsync() != "Sitemaps")
            {
                await page.Locator("#search-box").FillAsync("Sitemaps");
                await scene.PauseAsync(500);
            }

            await scene.HighlightAsync(sitemapsRow);
            await LeftAsync(scene, t0, durations, "setup-1", 3.2);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, sitemapsRow.Locator("#btn-enable-OrchardCore_Sitemaps"), 0);
            await page.Locator("#search-box").WaitForAsync();
            await scene.PauseAsync(300);
            await page.Locator("#search-box").FillAsync("Sitemaps");
            await scene.PauseAsync(500);
            await scene.HighlightAsync(sitemapsRow.Locator("a:has-text(\"Disable\")"));
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            var seo = page.Locator("#left-nav ul.nav-group-items[data-title=\"Tools\"] button.item-label:has(span.title:text-is(\"Search Engine Optimization\"))");

            if (!await seo.IsVisibleAsync())
            {
                await scene.ClickAsync(Group(page, "Tools"));
            }

            await scene.ClickAsync(seo);
            var items = page.Locator("#left-nav ul[data-title=\"Search Engine Optimization\"] a.item-label");
            await items.Last.WaitForAsync();
            var count = await items.CountAsync();

            for (var i = 0; i < count; i++)
            {
                await scene.HighlightAsync(items.Nth(i), padding: 3);
            }

            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-tools-search_engine_optimization-sitemaps"));
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.HighlightAsync(page.Locator(".alert.alert-warning").First);
        });

        await scene.StepAsync("setup-4", async () =>
        {
            var t0 = Now(scene);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Group(page, "Settings"));
            var general = MenuLink(page, "Settings", "General");

            if (await general.CountAsync() > 0)
            {
                await GoAsync(scene, general);
            }
            else
            {
                await scene.OffCameraAsync(() => scene.NavigateAsync("/Admin/Settings/general"), 150);
            }

            var field = page.Locator("input[name=\"ISite.BaseUrl\"]");
            await field.WaitForAsync();
            await scene.PauseAsync(500);
            await scene.TypeAsync(field, "https://www.example.com", 3);
            await scene.PauseAsync(300);

            if (await field.InputValueAsync() != "https://www.example.com")
            {
                await field.FillAsync("https://www.example.com");
            }

            await page.Keyboard.PressAsync("Tab");
            await LeftAsync(scene, t0, durations, "setup-4", 3.0);
            await GoAsync(scene, page.Locator("button.save"), 0);
            await page.Locator(".message-success, .toast, .alert-success").First.WaitForAsync();
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Sitemaps/List");
        await page.Locator("a.create").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create"));
            await page.Locator("#Name").WaitForAsync();
            await scene.PauseAsync(400);
            await scene.TypeAsync(page.Locator("#Name"), "Pages", 4);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("div.ocat-wrapper:has(#Path)").First);
        });

        await scene.StepAsync("create-3", async () =>
        {
            var t0 = Now(scene);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("div.mb-3:has(#Enabled), div.form-check:has(#Enabled)").Last);
            await LeftAsync(scene, t0, durations, "create-3", 2.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.create"), 0);
        });

        await scene.StepAsync("create-4", async () =>
        {
            var row = PagesRow(page);
            await row.WaitForAsync();
            await scene.HighlightAsync(row.Locator(".badge:has-text(\"Enabled\")").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            var manage = row.Locator("a:has-text(\"Manage Sources\")").First;
            var view = row.Locator("a:has-text(\"View\")").First;
            var edit = row.Locator("a:has-text(\"Edit\")").First;
            var delete = row.Locator("a:has-text(\"Delete\")").First;
            await scene.HighlightAsync(manage, padding: 3);
            await scene.HighlightAsync(view, "/pages.xml", padding: 3);
            await scene.HighlightAsync(edit, padding: 3);
            await scene.HighlightAsync(delete, padding: 3);
        });
    }

    private static async Task SourceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Sitemaps/List");
        await PagesRow(page).WaitForAsync();
        await scene.StartCaptureAsync();

        var addSource = page.Locator("button[data-bs-target=\"#modalSources\"]");

        await scene.StepAsync("source-1", async () =>
        {
            await GoAsync(scene, PagesRow(page).Locator("a:has-text(\"Manage Sources\")").First);
            await addSource.WaitForAsync();
            await scene.PauseAsync(500);
            await scene.ClickAsync(addSource);
        });

        await scene.StepAsync("source-2", async () =>
        {
            await page.Locator("#modalSources.show").WaitForAsync();
            await scene.PauseAsync(500);
            var cards = page.Locator("#modalSources .card");
            var types = cards.Filter(new() { HasText = "Sitemap Content Types" }).First;
            var custom = cards.Filter(new() { HasText = "Sitemap Custom Path" }).First;
            await scene.HighlightAsync(types);
            await scene.HighlightAsync(custom);
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, types.Locator("a:has-text(\"Add\")").First);
        });

        await scene.StepAsync("source-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("input.content-types-sitemap-index-all"));
        });

        await scene.StepAsync("source-4", async () =>
        {
            var t0 = Now(scene);
            await scene.ClearHighlightsAsync();
            var selects = page.Locator("#index-all-row select");
            await scene.HighlightAsync(selects.Nth(0));
            await scene.HighlightAsync(selects.Nth(1));
            await LeftAsync(scene, t0, durations, "source-4", 2.8);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.create"), 0);
        });

        await scene.StepAsync("source-5", async () =>
        {
            var item = page.Locator("li.list-group-item, .list-group-item").Filter(new() { HasText = "Sitemap Content Types" }).Last;
            await item.WaitForAsync();
            await scene.HighlightAsync(item.Locator(".badge:has-text(\"All\")").First);
            await scene.HighlightAsync(item.Locator(":text(\"Sitemap Content Types\")").First);
        });
    }

    // Opens the sitemap page of a sitemap (outside the capture) on its Manage Sources page.
    private static async Task OpenSourcesAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/Sitemaps/List");
        var manage = PagesRow(scene.Page).Locator("a:has-text(\"Manage Sources\")").First;
        await manage.ClickAsync();
        await scene.Page.Locator("button[data-bs-target=\"#modalSources\"]").WaitForAsync();
        await scene.PauseAsync(600);
    }

    // Clicks View, which opens the sitemap in another tab, and captures that tab from now on.
    private static async Task OpenViewAsync(Scene scene)
    {
        var view = scene.Page.Locator("a.btn-success:has-text(\"View\")").First;
        var popup = await scene.Page.RunAndWaitForPopupAsync(() => scene.ClickAsync(view, pauseAfter: 200));
        await scene.OffCameraAsync(async () =>
        {
            await scene.SwitchToAsync(popup);

            // The browser's own zoom, so that the tree is readable in the video.
            await popup.EvaluateAsync("() => { document.documentElement.style.zoom = '1.6'; window.scrollTo(0, 0); }");
            await Task.Delay(500);
        }, 100);

        // The screencast sends a frame only when the page changes: a tiny zoom change after the cut makes the XML appear in the clip.
        await popup.EvaluateAsync("() => { document.documentElement.style.zoom = '1.601'; }");
        await scene.PauseAsync(300);
    }

    // Scrolls the Edit Content Type page so that its list of parts and the Add Parts button are on screen.
    private static async Task ScrollToPartsAsync(Scene scene)
    {
        var add = scene.Page.Locator("a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Add Parts\s*$") }).First;
        var box = await add.BoundingBoxAsync();

        if (box is not null && box.Y > 560)
        {
            await scene.ScrollAsync(0, box.Y - 560, 20);
        }
    }

    private static ILocator SourceItem(IPage page)
        => page.Locator("li.list-group-item, .list-group-item").Filter(new() { HasText = "Sitemap Content Types" }).Last;

    private static async Task ViewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenSourcesAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("view-1", async () =>
        {
            await OpenViewAsync(scene);
        });

        await scene.StepAsync("view-2", async () =>
        {
            await scene.HoldAsync(1.0);
            await scene.ScrollAsync(0, 300);
            await scene.HoldAsync(2.5);
            await scene.ScrollAsync(0, 300);
        });
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenSourcesAsync(scene);
        var page = scene.Page;
        await scene.StartCaptureAsync();

        ILocator TypeRow(string name) => page.Locator("#index-selected-row li").Filter(new() { Has = page.Locator("label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*" + System.Text.RegularExpressions.Regex.Escape(name) + @"\s*$") }) }).First;

        await scene.StepAsync("types-1", async () =>
        {
            await GoAsync(scene, SourceItem(page).Locator("a.btn-primary:has-text(\"Edit\")").First);
            await page.Locator("input.content-types-sitemap-index-all").WaitForAsync();
            await scene.PauseAsync(400);
            await scene.ClickAsync(page.Locator("input.content-types-sitemap-index-all"));
            await page.Locator("#index-selected-row").WaitForAsync();
        });

        await scene.StepAsync("types-2", async () =>
        {
            // The first rows are already on screen after the box is unchecked: ring their names without scrolling.
            foreach (var name in new[] { "Article", "Blog", "Blog Post" })
            {
                var label = page.Locator("#index-selected-row li label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*" + System.Text.RegularExpressions.Regex.Escape(name) + @"\s*$") }).First;
                await scene.HighlightAsync(label, padding: 5);
            }

            await scene.HoldAsync(2.2);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(TypeRow("Article").Locator("input[type=checkbox]"));
            await scene.ClickAsync(TypeRow("Blog Post").Locator("input[type=checkbox]"));
        });

        await scene.StepAsync("types-3", async () =>
        {
            var t0 = Now(scene);
            var post = TypeRow("Blog Post");
            var article = TypeRow("Article");
            await scene.SelectAsync(post.Locator("select").Nth(0), "Weekly");
            await scene.SelectAsync(post.Locator("select").Nth(1), "0.8");
            await scene.SelectAsync(article.Locator("select").Nth(0), "Monthly");
            await LeftAsync(scene, t0, durations, "types-3", 2.8);
            await GoAsync(scene, page.Locator("button.save"), 0);
        });

        await scene.StepAsync("types-4", async () =>
        {
            var item = SourceItem(page);
            await item.WaitForAsync();
            await scene.HighlightAsync(item.Locator(".badge:has-text(\"Article\")").First);
            await scene.HighlightAsync(item.Locator(".badge:has-text(\"Blog Post\")").First);
            await scene.HighlightAsync(item.Locator(":text(\"Sitemap Content Types\")").First);
            await scene.HoldAsync(1.8);
            await scene.ClearHighlightsAsync();
            await OpenViewAsync(scene);
            await scene.HoldAsync(0.8);
            await scene.ScrollAsync(0, 300);
        });
    }

    private static async Task PathAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenSourcesAsync(scene);
        var page = scene.Page;
        await scene.StartCaptureAsync();

        await scene.StepAsync("path-1", async () =>
        {
            await scene.HoldAsync(1.5);
            await scene.ClickAsync(page.Locator("button[data-bs-target=\"#modalSources\"]"));
            await page.Locator("#modalSources.show").WaitForAsync();
            await scene.PauseAsync(500);
            var custom = page.Locator("#modalSources .card").Filter(new() { HasText = "Sitemap Custom Path" }).First;
            await GoAsync(scene, custom.Locator("a:has-text(\"Add\")").First);
            await page.Locator("#CustomPathSitemapSource_Path").WaitForAsync();
        });

        await scene.StepAsync("path-2", async () =>
        {
            var field = page.Locator("#CustomPathSitemapSource_Path");
            await scene.TypeAsync(field, "tags", 4);
            await scene.HighlightAsync(page.Locator("div.ocat-wrapper:has(#CustomPathSitemapSource_Path)").First);
        });

        await scene.StepAsync("path-3", async () =>
        {
            var t0 = Now(scene);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(page.Locator("#CustomPathSitemapSource_ChangeFrequency"), "Monthly");
            await LeftAsync(scene, t0, durations, "path-3", 3.2);
            await GoAsync(scene, page.Locator("button.create"), 0);
            var item = page.Locator("li.list-group-item, .list-group-item").Filter(new() { HasText = "Sitemap Custom Path" }).Last;
            await item.WaitForAsync();
            await scene.HighlightAsync(item);
        });

        await scene.StepAsync("path-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenViewAsync(scene);
            await scene.ScrollAsync(0, 600);
        });
    }

    private static async Task PartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("part-1", async () =>
        {
            await scene.ClickAsync(Group(page, "Design"));
            await scene.ClickAsync(page.Locator("#left-nav ul.nav-group-items[data-title=\"Design\"] button.item-label:has(span.title:text-is(\"Content Definition\"))"));
            var types = page.Locator("#left-nav ul[data-title=\"Content Definition\"] a.item-label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Content Types\s*$") });
            await GoAsync(scene, types);
            await GoAsync(scene, page.Locator("a[role=\"btn-edit-Article\"]"));
            await page.Locator("a:has-text(\"Add Parts\")").First.WaitForAsync();
            await scene.PauseAsync(500);
            await ScrollToPartsAsync(scene);
        });

        await scene.StepAsync("part-2", async () =>
        {
            var t0 = Now(scene);
            var addParts = page.Locator("a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Add Parts\s*$") }).First;
            await GoAsync(scene, addParts);
            var row = page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Sitemap\s*$") }) }).First;
            await row.WaitForAsync();
            await scene.ClickAsync(row.Locator("input[type=checkbox]"));
            await scene.HighlightAsync(row.Locator(":text(\"Provides an optional part\")").First);
            await LeftAsync(scene, t0, durations, "part-2", 3.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#add-selected-parts-button"), 0);
            await page.Locator("a:has-text(\"Add Parts\")").First.WaitForAsync();
            await scene.PauseAsync(400);
            await ScrollToPartsAsync(scene);
        });

        await scene.StepAsync("part-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Group(page, "Content"));
            await GoAsync(scene, page.Locator("#left-nav a.oc-navigation-admin-content-content_items"));
            var row = page.Locator("li.list-group-item, .list-group-item").Filter(new() { HasText = "About" }).Filter(new() { Has = page.Locator("a.edit") }).Last;
            await row.WaitForAsync();
            await GoAsync(scene, row.Locator("a.edit").First);
            var seo = page.Locator("a, button").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*SEO\s*$") }).First;
            await seo.WaitForAsync();
            await scene.PauseAsync(400);
            await scene.ClickAsync(seo);
            await scene.ClickAsync(page.Locator("input[data-sitemap-override-toggle]"));
        });

        await scene.StepAsync("part-4", async () =>
        {
            var t0 = Now(scene);
            var exclude = page.Locator("input[type=checkbox][name$=\"Exclude\"]").First;
            await exclude.WaitForAsync();
            await scene.HighlightAsync(page.Locator("div:has(> input[type=checkbox][name$=\"Exclude\"]), .form-check:has(input[name$=\"Exclude\"])").Last);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("select[name$=\"ChangeFrequency\"]").First);
            await scene.HighlightAsync(page.Locator("select[name$=\"Priority\"]").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(exclude);
            await LeftAsync(scene, t0, durations, "part-4", 2.8);
            await GoAsync(scene, page.Locator("button.publish").First, 0);
        });

        await scene.StepAsync("part-5", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/pages.xml");
                await page.EvaluateAsync("() => { document.documentElement.style.zoom = '1.6'; window.scrollTo(0, 0); }");
                await Task.Delay(500);
            }, 150);
            await page.EvaluateAsync("() => { document.documentElement.style.zoom = '1.601'; }");
            await scene.PauseAsync(300);
            await scene.HoldAsync(1.0);
            await scene.ScrollAsync(0, 300);
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Sitemap Indexes", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-create", CreateAsync),
        ("03-view", ViewAsync),
        ("05-cache", CacheAsync),
        ("06-cleanup", CleanupAsync),
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

    private static async Task WaitSiteAsync()
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        for (var i = 0; i < 60; i++)
        {
            try
            {
                var response = await client.GetAsync(BaseUrl + "/");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(2000);
        }
    }

    private static async Task EnableFeatureAsync(IPage page, string id)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#btn-enable-" + id.Replace('.', '_')).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitSiteAsync();
        await Task.Delay(5000);
    }

    private static async Task AddSitemapAsync(IPage page, string name, string contentType)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Sitemaps/Create");
        await page.Locator("#Name").FillAsync(name);
        await page.Locator("button.create").ClickAsync();
        await page.WaitForURLAsync(u => u.EndsWith("/Admin/Sitemaps/List", StringComparison.OrdinalIgnoreCase));
        var row = page.Locator(".list-group-item").Filter(new() { HasTextString = name }).First;
        await row.Locator("a:has-text('Manage Sources')").First.ClickAsync();
        await page.Locator("button[data-bs-target='#modalSources']").ClickAsync();
        await page.Locator("#modalSources.show").WaitForAsync();
        await page.Locator("#modalSources .card").Filter(new() { HasTextString = "Sitemap Content Types" }).First.Locator("a:has-text('Add')").First.ClickAsync();
        await page.WaitForURLAsync(u => u.Contains("SitemapSource/Create"));
        await page.Locator("label[for='ContentTypesSitemapSource_IndexAll']").ClickAsync();
        var box = page.Locator("div.form-check").Filter(new() { Has = page.Locator("input.content-type-checkbox") }).Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^\\s*" + contentType + "\\s*$") }).First;
        await box.Locator("label").First.ClickAsync();
        await page.Locator("button.create").ClickAsync();
        await page.WaitForURLAsync(u => u.Contains("/Admin/Sitemaps/Display"));
    }

    private static async Task<string> GetAsync(string path)
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        return await client.GetStringAsync(BaseUrl + path);
    }

    /// <summary>Provisioning, outside the capture: the features, the Base url, the Robots setting, the sitemaps Posts and Articles, and a Posts cache file that no sitemap uses any more.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        foreach (var id in new[] { "OrchardCore.Sitemaps", "OrchardCore.Seo", "OrchardCore.BackgroundTasks" })
        {
            await EnableFeatureAsync(page, id);
        }

        await page.GotoAsync(BaseUrl + "/Admin/Settings/general");
        await page.Locator("input[name='ISite.BaseUrl']").FillAsync("https://www.example.com");
        await page.Locator("button.save").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync(BaseUrl + "/Admin/Settings/robotsSettings");
        await page.Locator("button.save").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await AddSitemapAsync(page, "Posts", "Blog Post");
        await AddSitemapAsync(page, "Articles", "Article");

        await GetAsync("/posts.xml");
        await GetAsync("/articles.xml");

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems");
        var list = await page.Locator("a:has-text('List BlogPost Items')").First.GetAttributeAsync("href");
        await page.GotoAsync(BaseUrl + list);
        var edit = await page.Locator(".list-group-item").Filter(new() { HasTextString = "Man must explore" }).First.Locator("a[href*='/Edit']").First.GetAttributeAsync("href");
        await page.GotoAsync(BaseUrl + edit);
        await page.Locator(".CodeMirror").First.EvaluateAsync("e => { const cm = e.CodeMirror; cm.setValue(cm.getValue() + '\\n\\nFake edit.'); }");
        await page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);
        await GetAsync("/posts.xml");

        foreach (var path in new[] { "/Admin/SitemapIndexes/List", "/Admin/SitemapCache/List", "/Admin/BackgroundTasks", "/Admin/Features" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    // The recorder's cursor overlay adds a style element that an XML page shows as plain text: remove it there.
    private const string XmlCleanScript = """
        (() => {
          const clean = (n) => {
            if (n.nodeType === 1 && !document.contentType.includes('html')
              && (n.id === 'demo-cursor' || n.localName === 'style' || (n.classList && n.classList.contains('demo-ripple')))) { n.remove(); }
          };
          new MutationObserver((records) => records.forEach((r) => r.addedNodes.forEach(clean))).observe(document, { childList: true, subtree: true });
        })();
        """;

    private static async Task<Scene> NewXmlSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.Context.AddInitScriptAsync(XmlCleanScript);
        return scene;
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<Scene, Task> ready = null)
    {
        var scene = await NewXmlSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        await scene.Page.Locator("#left-nav").WaitForAsync();
        await ExpandSeoAsync(scene.Page);
        if (ready != null)
        {
            await ready(scene);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static ILocator Group(IPage page, string title)
        => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator MenuLink(IPage page, string group, string text)
        => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{group}\"] a.item-label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*" + System.Text.RegularExpressions.Regex.Escape(text) + @"\s*$") });

    // Opens Tools and Search Engine Optimization in the admin menu, so that the three entries are visible.
    private static async Task ExpandSeoAsync(IPage page)
    {
        var sitemaps = page.Locator("a.oc-navigation-admin-tools-search_engine_optimization-sitemaps").First;
        var seo = page.Locator("#left-nav ul.nav-group-items[data-title=\"Tools\"] button.item-label:has(span.title:text-is(\"Search Engine Optimization\"))");
        if (await sitemaps.IsVisibleAsync())
        {
            return;
        }

        if (!await seo.IsVisibleAsync())
        {
            await Group(page, "Tools").ClickAsync();
            await Task.Delay(500);
        }

        await seo.ClickAsync();
        await Task.Delay(500);
    }

    // Shows an XML page zoomed so that it reads in the video (inside OffCameraAsync), then, on camera, a tiny zoom change makes the screencast send a frame.
    private static async Task ZoomXmlAsync(IPage page)
    {
        await page.EvaluateAsync("() => { document.documentElement.style.zoom = '1.6'; window.scrollTo(0, 0); }");
        await Task.Delay(500);
    }

    private static async Task NudgeXmlAsync(Scene scene, IPage page)
    {
        await page.EvaluateAsync("() => { document.documentElement.style.zoom = '1.601'; }");
        await scene.PauseAsync(300);
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task WaitTextAsync(IPage page, string text, int timeout = 60_000)
        => await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = timeout });

    // Clicks a link or button that loads a new page; the load is cut out of the video (see Scene.OffCameraAsync).
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
    }

    // Opens a page during the capture, with the load cut out of the video.
    private static Task VisitAsync(Scene scene, string path)
        => scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(BaseUrl + path)), leadMilliseconds: 250);

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Row(IPage page, string text) => page.Locator(".list-group-item, tr").Filter(new() { HasTextString = text }).First;

    private static async Task OpenAdminGroupAsync(IPage page, string name)
    {
        var group = page.Locator("#left-nav").GetByText(name, new() { Exact = true }).First;
        await group.ClickAsync();
        await Task.Delay(500);
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Sitemaps/List", s => s.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Articles" }).First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".list-group-item").Filter(new() { HasTextString = "Posts" }).First.Locator("a").First);
            await scene.HighlightAsync(page.Locator(".list-group-item").Filter(new() { HasTextString = "Articles" }).First.Locator("a").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("create-2", async () =>
        {
            await GoAsync(scene, MenuLink(page, "Tools", "Sitemap Indexes").First);
            await WaitTextAsync(page, "There are no sitemap indexes");
            await GoAsync(scene, page.Locator("a.create").First);
            await page.Locator("#Name").WaitForAsync();
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.TypeAsync(page.Locator("#Name"), "Main", delay: 40);
            await scene.TypeAsync(page.Locator("#Path"), "sitemap.xml", delay: 30);
        });

        await scene.StepAsync("create-4", async () =>
        {
            if (!await page.Locator("#Enabled").IsCheckedAsync())
            {
                await scene.ClickAsync(page.Locator("label[for='Enabled']"), pauseAfter: 300);
            }

            await scene.ClickAsync(page.Locator("label.form-check-label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Articles\s*$") }).First, pauseAfter: 300);
            await scene.ClickAsync(page.Locator("label.form-check-label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Posts\s*$") }).First, pauseAfter: 300);
        });

        await scene.StepAsync("create-5", async () =>
        {
            await GoAsync(scene, page.Locator("button.save").First);
            await page.Locator(".list-group-item").Filter(new() { HasTextString = "Main" }).First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".list-group-item").Filter(new() { HasTextString = "Main" }).First.Locator(".badge").First);
        });
    }

    private static async Task ViewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/SitemapIndexes/List", s => s.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Main" }).First.WaitForAsync());
        var main = scene.Page;
        IPage popup = null;

        await scene.StepAsync("view-1", async () =>
        {
            var view = main.Locator(".list-group-item").Filter(new() { HasTextString = "Main" }).First.Locator("a:has-text('View')").First;
            popup = await main.RunAndWaitForPopupAsync(() => scene.ClickAsync(view, pauseAfter: 200));
            await scene.OffCameraAsync(async () =>
            {
                await popup.WaitForLoadStateAsync();
                await scene.SwitchToAsync(popup);
                await ZoomXmlAsync(popup);
            }, 100);
            await NudgeXmlAsync(scene, popup);
            await scene.HoldAsync(1.0);
        });

        await scene.StepAsync("view-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await popup.GotoAsync(BaseUrl + "/posts.xml");
                await ZoomXmlAsync(popup);
            }, 100);
            await NudgeXmlAsync(scene, popup);
            await scene.HoldAsync(1.0);
        });

        await scene.StepAsync("view-3", async () =>
        {
            await scene.SwitchToAsync(main);
            await main.BringToFrontAsync();
            await scene.ClickAsync(Group(main, "Settings"), pauseAfter: 400);
            var search = main.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] button.item-label:has(span.title:text-is(\"Search\"))").First;
            await scene.ClickAsync(search, pauseAfter: 400);
            var seo = main.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] ul[data-title=\"Search\"] button.item-label:has(span.title:text-is(\"Search Engine Optimization\"))").First;
            await scene.ClickAsync(seo, pauseAfter: 400);
            await GoAsync(scene, main.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] a.item-label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Robots\s*$") }).First);
            var box = main.Locator("label:has-text('Include all sitemaps')").First;
            await box.WaitForAsync();
            await ShowAsync(scene, box, 1500);
        });

        await scene.StepAsync("view-4", async () =>
        {
            await scene.OffCameraAsync(async () => { await main.GotoAsync(BaseUrl + "/robots.txt"); await ZoomXmlAsync(main); }, 100);
            await NudgeXmlAsync(scene, main);
            await scene.HoldAsync(1.0);
        });
    }

    private static async Task CacheAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/SitemapIndexes/List", s => s.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Main" }).First.WaitForAsync());
        var page = scene.Page;
        string unused = null;

        await scene.StepAsync("cache-1", async () =>
        {
            await GoAsync(scene, MenuLink(page, "Tools", "Sitemaps Cache").First);
            await page.Locator(".list-group-item").Nth(3).WaitForAsync();
            await scene.HighlightAsync(page.Locator("a:has-text('Purge All')").First);
        });

        await scene.StepAsync("cache-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var posts = page.Locator(".list-group-item").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"Posts_") });
            await scene.HighlightAsync(posts.Nth(0));
            await scene.HoldAsync(1.5);
            await scene.HighlightAsync(posts.Nth(1));
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            var names = await posts.EvaluateAllAsync<string[]>("es => es.map(e => (e.innerText.match(/Posts_[A-Za-z0-9]+\\.xml/) || [''])[0])");
            unused = await FindUnusedAsync(names);
        });

        await scene.StepAsync("cache-3", async () =>
        {
            var row = page.Locator(".list-group-item").Filter(new() { HasTextString = unused }).First;
            await scene.ClickAsync(row.Locator("a:has-text('Purge')").First, pauseAfter: 300);
            await page.Locator("#modalOkButton").WaitForAsync();
            await scene.HoldAsync(1.2);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Locator("#modalOkButton").ClickAsync()), 200);
            await page.Locator(".list-group-item").Nth(2).WaitForAsync();
        });

        await scene.StepAsync("cache-4", async () =>
        {
            await scene.ClickAsync(page.Locator("a:has-text('Purge All')").First, pauseAfter: 300);
            await page.Locator("#modalOkButton").WaitForAsync();
            await scene.HoldAsync(1.2);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Locator("#modalOkButton").ClickAsync()), 200);
            await WaitTextAsync(page, "no sitemaps cached");
        });
    }

    // The unused Posts file is the older one: the current Posts file is the one the Posts sitemap serves now.
    private static async Task<string> FindUnusedAsync(string[] names)
    {
        var folder = "C:/Code/oc-video-site/src/OrchardCore.Cms.Web/wwwroot/Default/sm-cache";
        var infos = names.Select(n => new FileInfo(Path.Combine(folder, n))).Where(f => f.Exists).OrderBy(f => f.LastWriteTimeUtc).ToList();
        return infos.Count > 0 ? infos[0].Name : names[0];
    }

    private static async Task CleanupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Features", s => s.Page.Locator("#search-box").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("cleanup-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "sitemaps", delay: 40);
            var row = page.Locator("[data-filter-value]").Filter(new() { HasTextString = "OrchardCore.Sitemaps.Cleanup" }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, row.Locator("#btn-enable-OrchardCore_Sitemaps_Cleanup"));
            await WaitSiteAsync();
        });

        await scene.StepAsync("cleanup-2", async () =>
        {
            await GoAsync(scene, MenuLink(page, "Tools", "Background Tasks").First);
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 24; i++)
                {
                    if (await page.Locator(".list-group-item").Filter(new() { HasTextString = "Sitemap Cache Cleaner" }).First.Locator("[data-next-run]").CountAsync() > 0)
                    {
                        return;
                    }

                    await Task.Delay(8000);
                    await page.GotoAsync(BaseUrl + "/Admin/BackgroundTasks");
                }
            }, 100);
            await scene.HighlightAsync(page.Locator(".list-group-item").Filter(new() { HasTextString = "Sitemap Cache Cleaner" }).First);
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the editors.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Feeds", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-find", FindAsync),
        ("04-control", ControlAsync),
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

    private static async Task SetCodeAsync(IPage page, string text)
    {
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("t => document.querySelector('.CodeMirror').CodeMirror.setValue(t)", text);
    }

    private static async Task PublishAsync(IPage page)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static ILocator BlogRow(IPage page)
        => page.Locator(".list-group-item").Filter(new() { Has = page.Locator("a[href*='/Display']:text-is('Blog')") }).First;

    private static async Task<string> BlogEditUrlAsync(IPage page)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems");
        var href = await BlogRow(page).Locator("a[href*='/Edit']").First.GetAttributeAsync("href");
        var id = href.Split('/')[4];
        return BaseUrl + "/Admin/Contents/ContentItems/" + id + "/Edit";
    }

    /// <summary>Provisioning, outside the capture: the bakery content of the plan, then Feeds disabled.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        var blog = await BlogEditUrlAsync(page);
        await page.GotoAsync(blog);
        await SetCodeAsync(page, "News and notes from Example Bakery.");
        await PublishAsync(page);

        await page.GotoAsync(blog);
        await page.Locator("a:has-text('List Blog Post Items')").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator(".list-group-item").Filter(new() { HasTextString = "Man must explore" }).First.Locator("a[href*='/Edit']").First.ClickAsync();
        await page.Locator("#TitlePart_Title").FillAsync("Opening Week Notes");
        await SetCodeAsync(page, "We opened our doors last week and sold out of rye twice.");
        await PublishAsync(page);

        var posts = new (string Title, string Body, bool Draft)[]
        {
            ("Weekend Market Hours", "We are at the Saturday market from eight until noon.", false),
            ("Spring Menu Preview", "Rhubarb tarts and lemon scones join the counter this month.", false),
            ("Sourdough Class Dates", "Our sourdough class meets every second Sunday.", false),
            ("Holiday Opening Times", "We close at three on the last day before the holiday.", false),
            ("Autumn Menu Ideas", "Pumpkin loaf and apple galettes are being tested.", true),
        };

        foreach (var (title, body, draft) in posts)
        {
            await Task.Delay(3000);
            await page.GotoAsync(blog);
            await page.Locator("a:has-text('Create Blog Post')").First.ClickAsync();
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await SetCodeAsync(page, body);
            if (draft)
            {
                await page.GetByRole(AriaRole.Button, new() { Name = "Save Draft" }).First.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
            else
            {
                await PublishAsync(page);
            }
        }

        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#search-box").FillAsync("Feeds");
        await page.Locator("#btn-disable-OrchardCore_Feeds").ClickAsync();
        await page.Locator(".modal.show").GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitSiteAsync();
        await Task.Delay(5000);

        foreach (var path in new[] { "/Admin", "/Admin/Features", "/Admin/Contents/ContentItems" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Seeded.");
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

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<Scene, Task> ready = null)
    {
        var scene = await NewXmlSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
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

    // Zooms an XML page so that it reads in the video (inside OffCameraAsync); on camera, a tiny zoom change makes the screencast send a frame.
    private static async Task ZoomXmlAsync(IPage page, string zoom = "1.6")
    {
        await page.EvaluateAsync("z => { if (document.documentElement.style) { document.documentElement.style.zoom = z; } window.scrollTo(0, 0); }", zoom);
        await Task.Delay(500);
    }

    private static async Task NudgeXmlAsync(Scene scene, IPage page, string zoom = "1.601")
    {
        await page.EvaluateAsync("z => { if (document.documentElement.style) { document.documentElement.style.zoom = z; } }", zoom);
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

    private static ILocator FeedRow(IPage page, string id) => page.Locator("[data-filter-value]").Filter(new() { HasTextString = id }).First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", s => s.Page.Locator("#left-nav").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(Group(page, "Tools"), pauseAfter: 300);
            await GoAsync(scene, MenuLink(page, "Tools", "Features").First);
            await page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "Feeds", delay: 40);
            await FeedRow(page, "OrchardCore.Feeds").WaitForAsync();
            await scene.HighlightAsync(FeedRow(page, "OrchardCore.Feeds").Locator("#btn-enable-OrchardCore_Feeds"));
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, FeedRow(page, "OrchardCore.Feeds").Locator("#btn-enable-OrchardCore_Feeds"));
            await WaitSiteAsync();
            await page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "Feeds", delay: 20);
            await FeedRow(page, "OrchardCore.Feeds").Locator("#btn-disable-OrchardCore_Feeds").WaitForAsync();
            await scene.HighlightAsync(FeedRow(page, "OrchardCore.Feeds").Locator("#btn-disable-OrchardCore_Feeds"));
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("Lists", new() { Delay = 40 });
            var row = FeedRow(page, "OrchardCore.Lists");
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });
    }

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", s => s.Page.Locator("#left-nav").WaitForAsync());
        var page = scene.Page;
        IPage feed = null;

        await scene.StepAsync("find-1", async () =>
        {
            await scene.ClickAsync(Group(page, "Content"), pauseAfter: 300);
            await GoAsync(scene, MenuLink(page, "Content", "Content Items").First);
            var title = BlogRow(page).Locator("a[href*='/Edit']").First;
            await title.WaitForAsync();
            await GoAsync(scene, title);
            await page.Locator("#TitlePart_Title").WaitForAsync();
        });

        await scene.StepAsync("find-2", async () =>
        {
            var label = page.Locator("label[for$='DisableRssFeed']").First;
            await label.ScrollToMiddleAsync();
            await scene.HighlightAsync(label);
        });

        await scene.StepAsync("find-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var link = page.Locator("span.hint:has-text('The current feed is available at') a").First;
            await link.ScrollToMiddleAsync();
            await scene.HighlightAsync(link);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            feed = await page.RunAndWaitForPopupAsync(() => scene.ClickAsync(link, pauseAfter: 200));
            await scene.OffCameraAsync(async () =>
            {
                await feed.WaitForLoadStateAsync();
                await scene.SwitchToAsync(feed);
                await ZoomXmlAsync(feed, "1.5");
            }, 100);
            await NudgeXmlAsync(scene, feed, "1.501");
        });

        await scene.StepAsync("find-4", async () =>
        {
            await scene.HoldAsync(1.0);
        });

        await scene.StepAsync("find-5", async () =>
        {
            await scene.ScrollAsync(0, 260, 20);
            await scene.HoldAsync(1.0);
            await scene.ScrollAsync(0, 200, 20);
        });
    }

    private static async Task ControlAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        IPage feed = null;
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentItems", async s =>
        {
            var url = await BlogEditUrlAsync(s.Page);
            await s.Page.GotoAsync(url);
            var link = s.Page.Locator("span.hint:has-text('The current feed is available at') a").First;
            await link.ScrollIntoViewIfNeededAsync();
            feed = await s.Page.RunAndWaitForPopupAsync(() => link.ClickAsync());
            await feed.WaitForLoadStateAsync();
            await s.Page.BringToFrontAsync();
            await s.Page.Locator("input[name$='FeedItemsCount']").First.ScrollToMiddleAsync();
        });
        var page = scene.Page;

        await scene.StepAsync("control-1", async () =>
        {
            var count = page.Locator("input[name$='FeedItemsCount']").First;
            await scene.HighlightAsync(count);
            await scene.HoldAsync(1.5);
            await count.FillAsync("");
            await scene.TypeAsync(count, "3", delay: 80);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("control-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").First);
            await page.Locator("input[name$='FeedItemsCount']").First.WaitForAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.SwitchToAsync(feed);
                await scene.WaitForNavigationAsync(() => feed.ReloadAsync());
                await ZoomXmlAsync(feed, "1.5");
            }, 100);
            await NudgeXmlAsync(scene, feed, "1.501");
            await scene.HoldAsync(1.0);
        });

        await scene.StepAsync("control-3", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.SwitchToAsync(page);
                await page.BringToFrontAsync();
                await page.Locator("input[name$='FeedProxyUrl']").First.ScrollToMiddleAsync();
            }, 100);
            var proxy = page.Locator("input[name$='FeedProxyUrl']").First;
            await scene.HighlightAsync(proxy);
            await scene.TypeAsync(proxy, "https://example.com/blog/feed", delay: 25);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("control-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var box = page.Locator("input[type='checkbox'][name$='DisableRssFeed']").First;
            await scene.ClickAsync(page.Locator("label[for$='DisableRssFeed']").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").First);
            await page.Locator("input[name$='FeedItemsCount']").First.WaitForAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.SwitchToAsync(feed);
                await scene.WaitForNavigationAsync(() => feed.ReloadAsync());
            }, 100);
            await NudgeXmlAsync(scene, feed, "1.001");
            await scene.HoldAsync(1.0);
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

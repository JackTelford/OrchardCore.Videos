using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "URL rewriting", recorded in order on one fresh site.</summary>
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
        ("03-redirect", RedirectAsync),
        ("03-redirect-test", RedirectTestAsync),
        ("04-rewrite", RewriteAsync),
        ("04-rewrite-test", RewriteTestAsync),
        ("05-manage", ManageAsync),
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

    /// <summary>Nothing to seed. Warms the pages up once, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Features", "/about", "/" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Pages warmed up.");
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

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<IPage, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(90_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene.Page);
        }

        await scene.StartCaptureAsync();
        return scene;
    }

    private static ILocator RulesItem(IPage page) => page.Locator("a[href$='/Admin/UrlRewriting/Index']").First;

    private static async Task OpenRulesMenuAsync(IPage page)
    {
        if (!await RulesItem(page).IsVisibleAsync())
        {
            await OpenAdminGroupAsync(page, "Tools");
        }
    }

    private static ILocator RuleRow(IPage page, string name) => page.Locator("#rewrite-rules-sortable-list .item").Filter(new() { HasTextString = name }).First;

    private static ILocator Save(IPage page) => page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First;

    private static async Task GoOffAsync(Scene scene, ILocator locator, int lead = 250)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: lead);
    }

    private static async Task FieldAsync(Scene scene, string label, string text)
    {
        await scene.TypeAsync(scene.Page.GetByLabel(label, new() { Exact = true }).First, text, delay: 40);
        await scene.Page.Keyboard.PressAsync("Tab");
    }

    private static string Address(Scene scene) => new Uri(scene.Page.Url).PathAndQuery;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, page.Locator("a[href$='/Admin/Features']").First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("URL Rewriting", new() { Delay = 45 });
            var row = page.Locator("[data-filter-value*='OrchardCore.UrlRewriting']").First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("#btn-enable-OrchardCore_UrlRewriting");
            await scene.MoveToAsync(enable);
            await GoOffAsync(scene, enable);
            await WaitTextAsync(page, "The feature URL Rewriting was enabled.", 120_000);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await OpenRulesMenuAsync(page);
            await scene.HighlightAsync(RulesItem(page));
        });
    }

    private static async Task RedirectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("redirect-1", async () =>
        {
            await OpenRulesMenuAsync(page);
            await GoAsync(scene, RulesItem(page));
            await page.GetByText("Nothing here!").First.WaitForAsync();
        });

        await scene.StepAsync("redirect-2", async () =>
        {
            await scene.ClickAsync(page.Locator("button.create").First, pauseAfter: 300);
            await page.Locator("#modalAddRule.show").WaitForAsync();
            await scene.PauseAsync(2000);
            await GoAsync(scene, page.Locator("#modalAddRule .card:has(h4:text-is('Redirect')) a.btn").First);
            await page.GetByLabel("Display Name", new() { Exact = true }).First.WaitForAsync();
        });

        await scene.StepAsync("redirect-3", async () =>
        {
            await FieldAsync(scene, "Display Name", "Redirect about-us to about");
            await FieldAsync(scene, "Match URL Pattern", "^/about-us$");
        });

        await scene.StepAsync("redirect-4", async () =>
        {
            await scene.ClickAsync(page.GetByLabel("Case Insensitive").First, pauseAfter: 400);
        });

        await scene.StepAsync("redirect-5", async () =>
        {
            await scene.HighlightAsync(page.GetByLabel("Query String Policy").First);
        });

        await scene.StepAsync("redirect-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, page.GetByLabel("Redirect Type").First, 2500);
            await scene.SelectAsync(page.GetByLabel("Redirect Type").First, "Moved Permanently (301)");
        });

        await scene.StepAsync("redirect-7", async () =>
        {
            await FieldAsync(scene, "The Substitution URL Pattern", "/about");
            await scene.MoveToAsync(Save(page));
        });

        await scene.StepAsync("redirect-8", async () =>
        {
            await GoOffAsync(scene, Save(page));
            await WaitTextAsync(page, "Rule created successfully.", 120_000);
            await scene.HighlightAsync(RuleRow(page, "Redirect about-us to about"));
        });
    }

    private static async Task RedirectTestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/");
        var page = scene.Page;

        await scene.StepAsync("redirect-test-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("h1").First, "Request: /about-us?ref=newsletter");
        });

        await scene.StepAsync("redirect-test-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/about-us?ref=newsletter");
            await page.WaitForURLAsync(u => u.EndsWith("/about?ref=newsletter"));
            await scene.HighlightAsync(page.Locator("h1").First, "Result: " + Address(scene));
        });
    }

    private static async Task RewriteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/UrlRewriting/Index");
        var page = scene.Page;

        await scene.StepAsync("rewrite-1", async () =>
        {
            await scene.ClickAsync(page.Locator("button.create").First, pauseAfter: 300);
            await page.Locator("#modalAddRule.show").WaitForAsync();
            await scene.PauseAsync(1500);
            await GoAsync(scene, page.Locator("#modalAddRule .card:has(h4:text-is('Rewrite')) a.btn").First);
            await page.GetByLabel("Display Name", new() { Exact = true }).First.WaitForAsync();
        });

        await scene.StepAsync("rewrite-2", async () =>
        {
            await FieldAsync(scene, "Display Name", "Serve media from img");
            await FieldAsync(scene, "Match URL Pattern", "^/img/(.*)$");
        });

        await scene.StepAsync("rewrite-3", async () =>
        {
            await FieldAsync(scene, "The Substitution URL Pattern", "/media/$1");
        });

        await scene.StepAsync("rewrite-4", async () =>
        {
            await scene.ClickAsync(page.GetByLabel("Case Insensitive").First, pauseAfter: 400);
            await scene.SelectAsync(page.GetByLabel("Query String Policy").First, "Exclude Query String from Original Request");
        });

        await scene.StepAsync("rewrite-5", async () =>
        {
            await scene.ClickAsync(page.GetByLabel("Stop Evaluation of Subsequent Rules").First, pauseAfter: 400);
            await scene.MoveToAsync(Save(page));
        });

        await scene.StepAsync("rewrite-6", async () =>
        {
            await GoOffAsync(scene, Save(page));
            await WaitTextAsync(page, "Rule created successfully.", 120_000);
            await scene.HighlightAsync(page.Locator("#rewrite-rules-sortable-list"));
        });
    }

    private static async Task RewriteTestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/");
        var page = scene.Page;

        await scene.StepAsync("rewrite-test-1", async () =>
        {
            await scene.NavigateAsync("/img/about-bg.jpg");
            await page.Locator("img").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("img").First, "Address: " + Address(scene));
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/UrlRewriting/Index");
        var page = scene.Page;

        await scene.StepAsync("manage-1", async () =>
        {
            await scene.HighlightAsync(RuleRow(page, "Redirect about-us to about"));
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, RuleRow(page, "Redirect about-us to about").Locator("a.btn-primary").First);
            await scene.PauseAsync(2500);
            await GoAsync(scene, page.Locator("a.cancel").First);
            await RuleRow(page, "Serve media from img").WaitForAsync();
        });

        await scene.StepAsync("manage-3", async () =>
        {
            var handle = RuleRow(page, "Serve media from img").Locator(".ui-sortable-handle").First;
            var first = RuleRow(page, "Redirect about-us to about");
            await page.RunAndWaitForResponseAsync(() => scene.DragToAsync(handle, first, 0, -22), r => r.Url.Contains("/url-rewriting/resort"), new() { Timeout = 30_000 });
            await page.WaitForFunctionAsync("() => document.querySelector('#rewrite-rules-sortable-list .item')?.innerText.includes('Serve media from img')");
        });

        await scene.StepAsync("manage-4", async () =>
        {
            await scene.ClickAsync(RuleRow(page, "Serve media from img").Locator("a.btn-danger").First, pauseAfter: 300);
            await page.Locator("#confirmRemoveModal").WaitForAsync();
            await scene.PauseAsync(2000);
            await GoOffAsync(scene, page.Locator("#modalOkButton"));
            await WaitTextAsync(page, "Rule deleted successfully.", 120_000);
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

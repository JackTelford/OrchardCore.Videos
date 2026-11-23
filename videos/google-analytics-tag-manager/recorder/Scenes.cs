using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Fake IDs: the placeholders of the documentation.
    private const string TrackingId = "G-XXXXXXXXXX";
    private const string ContainerId = "GTM-XXXXXXX";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-analytics", AnalyticsAsync),
        ("04-tagmanager", TagManagerAsync),
        ("05-site", SiteAsync),
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

    // Warms up the first visit of the pages, outside the capture. Nothing is changed.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();

        foreach (var path in new[] { "/", "/about", "/Admin", "/Admin/Features" })
        {
            await page.GotoAsync(BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await Task.Delay(800);
        }

        await context.CloseAsync();
    }

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

        // The scripts of the video are added for the visitor's browser only: keep the recording machine from calling Google.
        await scene.Context.RouteAsync(new Regex(@"googletagmanager\.com|google-analytics\.com|doubleclick\.net"), route => route.AbortAsync());

        return scene;
    }

    // Opens the page with the browser's storage cleared, so every take starts with the menu groups collapsed.
    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    // Moves to the link on camera, then clicks it and waits for the page off camera.
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 300);
    }

    private static ILocator Group(IPage page, string title) => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator GroupLink(IPage page, string group, string text) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{group}\"] a.item-label:has-text(\"{text}\")");

    // Settings, Integrations, then the given settings page.
    private static async Task OpenIntegrationAsync(Scene scene, string text)
    {
        var page = scene.Page;
        await scene.ClickAsync(Group(page, "Settings"));
        var integrations = page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] button.item-label:has(span.title:text-is(\"Integrations\"))");
        await scene.ClickAsync(integrations);
        var link = page.Locator($"#left-nav ul.nav-group-items[data-title=\"Settings\"] ul[data-title=\"Integrations\"] a.item-label:has-text(\"{text}\")");
        await link.WaitForAsync();
        await GoAsync(scene, link);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        var search = page.Locator("#search-box");

        async Task TypeSearchAsync()
        {
            await search.WaitForAsync();
            await scene.PauseAsync(600);
            await scene.TypeAsync(search, "Google", 3);
            await scene.PauseAsync(500);

            if (await search.InputValueAsync() != "Google")
            {
                await search.FillAsync("Google");
                await scene.PauseAsync(500);
            }
        }

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(Group(page, "Tools"));
            await GroupLink(page, "Tools", "Features").WaitForAsync();
            await GoAsync(scene, GroupLink(page, "Tools", "Features"));
            await TypeSearchAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            var rows = page.Locator("[data-filter-value]:visible");
            var title = page.Locator("h3:visible").Filter(new() { HasText = "Google" }).First;
            await scene.HighlightAsync(title);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(rows.Filter(new() { HasText = "Google Analytics" }).First.Locator("span.fw-semibold").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(rows.Filter(new() { HasText = "Google Authentication" }).First.Locator("span.fw-semibold").First);
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(rows.Filter(new() { HasText = "Google Tag Manager" }).First.Locator("span.fw-semibold").First);
            await scene.HoldAsync(0.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(rows.Filter(new() { HasText = "Google Authentication" }).First.Locator("span.fw-semibold").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("input[name='featureIds'][value='OrchardCore.Google.Analytics']"), pauseAfter: 200);
            await scene.ClickAsync(page.Locator("input[name='featureIds'][value='OrchardCore.Google.TagManager']"), pauseAfter: 200);
            await scene.MoveToAsync(page.Locator("#btn-enable-OrchardCore_Google_Analytics"));
        });

        await scene.StepAsync("enable-4", async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
            var enable = page.Locator("a[data-action='Enable']:visible").First;
            await enable.WaitForAsync();
            await scene.MoveToAsync(enable);

            // The click lands at the end of the step, so the message is still on screen in the next one.
            var wait = durations["enable-4"] - 0.45 -stopwatch.Elapsed.TotalSeconds;

            if (wait > 0)
            {
                await scene.HoldAsync(wait);
            }

            var (x, y) = await scene.CenterOfAsync(enable);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 300);
        });

        await scene.StepAsync("enable-5", async () =>
        {
            await TypeSearchAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AnalyticsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        var field = page.Locator("input[id$='TrackingID']");

        await scene.StepAsync("analytics-1", async () =>
        {
            await OpenIntegrationAsync(scene, "Google Analytics");
            await field.WaitForAsync();
            await scene.HighlightAsync(page.Locator("a.seedoc"));
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("h3:has-text(\"Google Analytics Settings\")"));
        });

        await scene.StepAsync("analytics-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("label:has-text(\"Tracking ID\")").First);
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("span.hint:has-text(\"tracking ID defined\"), .form-text:has-text(\"tracking ID defined\"), small:has-text(\"tracking ID defined\")").First);
        });

        await scene.StepAsync("analytics-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(field);
            await scene.TypeAsync(field, TrackingId, 3);
        });

        await scene.StepAsync("analytics-4", async () =>
        {
            await GoAsync(scene, page.Locator("button.save:visible").First);
            await page.Locator("input[id$='TrackingID']").WaitForAsync();
        });

        await scene.StepAsync("analytics-5");

        await scene.ClearHighlightsAsync();
    }

    private static async Task TagManagerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        var field = page.Locator("input[id$='ContainerID']");

        await scene.StepAsync("tagmanager-1", async () =>
        {
            await OpenIntegrationAsync(scene, "Google Tag Manager");
            await field.WaitForAsync();
        });

        await scene.StepAsync("tagmanager-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("label:has-text(\"Container ID\")").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("span.hint:has-text(\"container ID defined\"), .form-text:has-text(\"container ID defined\"), small:has-text(\"container ID defined\")").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(field);
            await scene.TypeAsync(field, ContainerId, 3);
        });

        await scene.StepAsync("tagmanager-3", async () =>
        {
            await GoAsync(scene, page.Locator("button.save:visible").First);
            await page.Locator("input[id$='ContainerID']").WaitForAsync();
        });

        await scene.StepAsync("tagmanager-4");

        await scene.ClearHighlightsAsync();
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Settings/OrchardCore.Google.Analytics");
        await page.Locator("input[id$='TrackingID']").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            await scene.OffCameraAsync(() => scene.NavigateAsync("/"), 300);
            await scene.ScrollAsync(0, 400);
            await scene.HoldAsync(1.0);
            await scene.ScrollAsync(0, -400);
        });

        await scene.StepAsync("site-2", async () =>
        {
            await GoAsync(scene, page.Locator("#mainNav a:text-is(\"About\")").First);
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.OffCameraAsync(() => scene.NavigateAsync("/Admin/Settings/OrchardCore.Google.Analytics"), 300);
            await page.Locator("h3:has-text(\"Google Analytics Settings\")").WaitForAsync();
            await scene.HighlightAsync(page.Locator("h3:has-text(\"Google Analytics Settings\")"));
        });

        await scene.ClearHighlightsAsync();
    }
}

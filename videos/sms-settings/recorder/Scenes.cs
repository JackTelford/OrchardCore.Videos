using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "SMS settings", recorded in order on one fresh site.</summary>
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
        ("03-default", DefaultAsync),
        ("04-test", TestAsync),
        ("05-twilio", TwilioAsync),
        ("06-azure", AzureAsync),
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

    /// <summary>Nothing to seed. Warms the admin pages up once, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Features", "/" })
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

    private static async Task<Scene> StartAdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, Func<IPage, Task> before = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        if (before != null)
        {
            await before(scene.Page);
        }

        await scene.StartCaptureAsync();
        return scene;
    }

    private static ILocator Nav(IPage page, string href) => page.Locator($"#left-nav a[href$='{href}']").First;

    private static async Task OpenSmsMenuAsync(IPage page)
    {
        await OpenAdminGroupAsync(page, "Settings");
        await OpenAdminGroupAsync(page, "Communication");
    }

    private static async Task SaveAsync(Scene scene)
    {
        await GoAsync(scene, scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First);
        await WaitTextAsync(scene.Page, "Site settings updated successfully.");
    }

    private static ILocator Pane(IPage page) => page.Locator(".tab-pane.active").First;

    private static async Task TabAsync(Scene scene, string name)
        => await scene.ClickAsync(scene.Page.Locator("a.nav-link, button.nav-link").Filter(new() { HasTextString = name }).First, pauseAfter: 400);

    private static async Task SearchFeatureAsync(Scene scene, string text)
    {
        await scene.ClickAsync(scene.Page.Locator("#search-box"), pauseAfter: 200);
        await scene.Page.Keyboard.TypeAsync(text, new() { Delay = 50 });
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, Nav(page, "/Admin/Features"));
            await SearchFeatureAsync(scene, "SMS");
            var enable = page.Locator("#btn-enable-OrchardCore_Sms");
            await enable.WaitForAsync();
            await scene.HighlightAsync(enable);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_Sms"));
            await WaitTextAsync(page, "was enabled", 90_000);
            await scene.MoveToAsync(900, 600);
            await OpenSmsMenuAsync(page);
            await scene.HighlightAsync(Nav(page, "/Admin/Settings/sms"));
        });
    }

    private static async Task DefaultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("default-1", async () =>
        {
            await OpenSmsMenuAsync(page);
            await GoAsync(scene, Nav(page, "/Admin/Settings/sms"));
            await page.GetByLabel("Default SMS provider").WaitForAsync();
            await scene.HighlightAsync(page.GetByLabel("Default SMS provider"), "Default SMS provider");
        });

        await scene.StepAsync("default-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(page.GetByLabel("Default SMS provider"), "Log");
        });

        await scene.StepAsync("default-3", async () =>
        {
            await SaveAsync(scene);
        });
    }

    private static async Task TestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("test-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await OpenAdminGroupAsync(page, "Testing");
            await GoAsync(scene, Nav(page, "/Admin/sms/test"));
            await page.GetByLabel("Provider", new() { Exact = true }).WaitForAsync();
            await ShowAsync(scene, page.GetByLabel("Provider", new() { Exact = true }), 1500);
            await ShowAsync(scene, page.Locator("input[type='tel']"), 1500);
        });

        await scene.StepAsync("test-2", async () =>
        {
            await scene.SelectAsync(page.GetByLabel("Provider", new() { Exact = true }), "Log");
            await scene.TypeAsync(page.Locator("input[type='tel']"), "+12015550123", delay: 60);
        });

        await scene.StepAsync("test-3", async () =>
        {
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Send", Exact = true }));
            await WaitTextAsync(page, "The test SMS message has been successfully sent.");
        });
    }

    private static async Task TwilioAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations, OpenSmsMenuAsync);
        var page = scene.Page;
        page.SetDefaultNavigationTimeout(120_000);

        await scene.StepAsync("twilio-1", async () =>
        {
            await GoAsync(scene, Nav(page, "/Admin/Settings/sms"));
            await TabAsync(scene, "Twilio");
            await scene.ClickAsync(page.Locator("label").Filter(new() { HasTextString = "Enable Twilio Provider" }).First, pauseAfter: 500);
            await Pane(page).GetByLabel("Account SID").WaitForAsync();
        });

        await scene.StepAsync("twilio-2", async () =>
        {
            await scene.TypeAsync(Pane(page).GetByLabel("Phone Number"), "+14255550123", delay: 50);
            await scene.TypeAsync(Pane(page).GetByLabel("Account SID"), "AC00000000000000000000000000000000", delay: 30);
            await scene.TypeAsync(Pane(page).GetByLabel("Auth Token"), "fake-auth-token", delay: 50);
        });

        await scene.StepAsync("twilio-3", async () =>
        {
            await SaveAsync(scene);
            await TabAsync(scene, "Twilio");
            await scene.HighlightAsync(Pane(page).GetByLabel("Auth Token"));
        });

        await scene.StepAsync("twilio-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/sms/test");
            await scene.SelectAsync(page.GetByLabel("Provider", new() { Exact = true }), "Twilio");
            await scene.TypeAsync(page.Locator("input[type='tel']"), "+12015550123", delay: 60);
            var send = page.GetByRole(AriaRole.Button, new() { Name = "Send", Exact = true });
            await scene.MoveToAsync(send);
            var (x, y) = await scene.CenterOfAsync(send);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
            await WaitTextAsync(page, "The test SMS message failed to send.");
        });
    }

    private static async Task AzureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("azure-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, Nav(page, "/Admin/Features"));
            await SearchFeatureAsync(scene, "SMS");
            var enable = page.Locator("#btn-enable-OrchardCore_Sms_Azure");
            await enable.WaitForAsync();
            await GoAsync(scene, enable);
            await WaitTextAsync(page, "was enabled", 90_000);
            await scene.MoveToAsync(900, 600);
        });

        await scene.StepAsync("azure-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/sms");
            await TabAsync(scene, "Azure Communication Services");
            await scene.ClickAsync(Pane(page).Locator("label").Filter(new() { HasTextString = "Enable" }).First, pauseAfter: 500);
            await scene.TypeAsync(Pane(page).GetByLabel("Phone Number"), "+12065550123", delay: 35);
            await scene.TypeAsync(Pane(page).GetByLabel("Connection String"), "endpoint=https://example.com/;accesskey=fake-key", delay: 12);
            await SaveAsync(scene);
            await TabAsync(scene, "Azure Communication Services");
        });

        await scene.StepAsync("azure-3", async () =>
        {
            await TabAsync(scene, "Providers");
            await scene.SelectAsync(page.GetByLabel("Default SMS provider"), "Azure");
            await SaveAsync(scene);
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

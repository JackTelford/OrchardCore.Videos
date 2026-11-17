using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Azure email", recorded in order on one fresh site.</summary>
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
        ("03-configure", ConfigureAsync),
        ("04-test", TestAsync),
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

        foreach (var path in new[] { "/Admin", "/Admin/Features" })
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

    private static async Task<Scene> StartAdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(90_000);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static ILocator Nav(IPage page, string href) => page.Locator($"#left-nav a[href$='{href}']").First;

    private static async Task GoOffAsync(Scene scene, ILocator locator, int lead = 250)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: lead);
    }

    private static async Task TabAsync(Scene scene, string name)
        => await scene.ClickAsync(scene.Page.Locator("ul.nav-tabs a.nav-link, a.nav-link, button.nav-link").Filter(new() { HasTextString = name }).First, pauseAfter: 400);

    private static ILocator Pane(IPage page) => page.Locator(".tab-pane.active").First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, Nav(page, "/Admin/Features"));
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("Azure Communication Services Email", new() { Delay = 30 });
            await page.Locator("#btn-enable-OrchardCore_Email_Azure").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.PauseAsync(2000);
            await GoOffAsync(scene, page.Locator("#btn-enable-OrchardCore_Email_Azure"));
            await WaitTextAsync(page, "was enabled", 120_000);
            await scene.MoveToAsync(900, 650);
        });
    }

    private static async Task ConfigureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("configure-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Settings");
            await OpenAdminGroupAsync(page, "Communication");
            await GoAsync(scene, Nav(page, "/Admin/Settings/email"));
            await scene.MoveToAsync(page.Locator("a.nav-link").Filter(new() { HasTextString = "Providers" }).First);
            await scene.MoveToAsync(page.Locator("a.nav-link").Filter(new() { HasTextString = "Azure Communication Services" }).First);
            await ShowAsync(scene, page.Locator(".alert").Filter(new() { HasTextString = "No email providers" }).First, 1500);
        });

        await scene.StepAsync("configure-2", async () =>
        {
            await TabAsync(scene, "Azure Communication Services");
            await scene.PauseAsync(3000);
            await scene.ClickAsync(page.Locator("label").Filter(new() { HasTextString = "Enable Azure Provider" }).First, pauseAfter: 400);
            await Pane(page).GetByLabel("Sender email address").WaitForAsync();
        });

        await scene.StepAsync("configure-3", async () =>
        {
            var hint = page.GetByText("The default email address to use as a sender").First;
            await scene.MoveToAsync(hint);
            await ShowAsync(scene, hint, 3000);
            await scene.TypeAsync(Pane(page).GetByLabel("Sender email address"), "noreply@example.com", delay: 30);
        });

        await scene.StepAsync("configure-4", async () =>
        {
            await scene.TypeAsync(Pane(page).GetByLabel("Connection String"), "endpoint=https://example-resource.communication.example.com/;accesskey=FAKE-KEY-NOT-A-REAL-KEY", delay: 14);
        });

        await scene.StepAsync("configure-5", async () =>
        {
            await scene.PauseAsync(2500);
            await GoOffAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First);
            await WaitTextAsync(page, "Site settings updated successfully.", 120_000);
            await scene.MoveToAsync(900, 650);
        });

        await scene.StepAsync("configure-6", async () =>
        {
            await TabAsync(scene, "Azure Communication Services");
            var field = Pane(page).GetByLabel("Connection String");
            await scene.MoveToAsync(field);
            await scene.HighlightAsync(field);
        });

        await scene.StepAsync("configure-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TabAsync(scene, "Providers");
            await scene.HighlightAsync(page.GetByLabel("Default provider"));
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
            await GoAsync(scene, Nav(page, "/Admin/Email/Test"));
            await page.GetByLabel("Provider", new() { Exact = true }).WaitForAsync();
            await scene.HighlightAsync(page.GetByLabel("Provider", new() { Exact = true }));
        });

        await scene.StepAsync("test-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#To"), "jamie.chen@example.com", delay: 25);
            await scene.TypeAsync(page.Locator("#Subject"), "Test message", delay: 30);
            await scene.TypeAsync(page.Locator("#Body"), "Hello from the test page.", delay: 30);
        });

        await scene.StepAsync("test-3", async () =>
        {
            foreach (var id in new[] { "#From", "#Cc", "#Bcc", "#ReplyTo" })
            {
                await scene.MoveToAsync(page.Locator(id));
                await scene.PauseAsync(500);
            }

            await scene.HighlightAsync(page.Locator("#From"));
        });

        await scene.StepAsync("test-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
            var send = page.GetByRole(AriaRole.Button, new() { Name = "Send", Exact = true }).First;
            await scene.MoveToAsync(send);
            await scene.HighlightAsync(send);
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

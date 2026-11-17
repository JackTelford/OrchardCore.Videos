using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Email: Configure SMTP and Send a Test Email", recorded in order on one fresh Blog site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The test mail server (smtp4dev in Docker): its web page on this computer.
    public static string MailUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_MAIL_URL") ?? "http://localhost:3000";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-configure", ConfigureAsync),
        ("04-test", TestAsync),
        ("05-inbox", InboxAsync),
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

    /// <summary>Warms up the pages the clips open. Nothing is changed.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Features" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Site warmed up.");
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

    private static ILocator Group(IPage page, string key) => page.Locator($"button.nav-group-label.oc-navigation-admin-{key}");

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Row(IPage page, string feature) => page.Locator("li.list-group-item, tr").Filter(new() { Has = page.Locator($"span.fw-semibold:text-is('{feature}')") }).Last;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(Group(page, "tools"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Features'] >> visible=true").First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "smtp", delay: 40);
            await page.Locator("#btn-enable-OrchardCore_Email_Smtp").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await ShowAsync(scene, Row(page, "SMTP Email Provider"), 2000);
            await scene.MoveToAsync(Row(page, "SMTP Email Provider").Locator(".badge:text-is('Email')").First);
            await scene.HighlightAsync(Row(page, "SMTP Email Provider").Locator(".badge:text-is('Email')").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_Email_Smtp"));
            await WaitTextAsync(page, "was enabled.");
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "email", delay: 40);
            await page.Locator("#list-summary").WaitForAsync();
            await ShowAsync(scene, Row(page, "Email"), 1500);
            await scene.HighlightAsync(Row(page, "SMTP Email Provider"));
        });
    }

    private static async Task ConfigureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("config-1", async () =>
        {
            await scene.ClickAsync(Group(page, "settings"), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("li.oc-navigation-admin-settings-communication > figure span.title, button:has-text('Communication')").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Settings/email'] >> visible=true").First);
            await page.Locator("ul.nav-tabs").WaitForAsync();
            await ShowAsync(scene, page.Locator(".alert.alert-warning").First, 2000);
        });

        await scene.StepAsync("config-2", async () =>
        {
            await scene.ClickAsync(page.Locator("ul.nav-tabs a.nav-link:text-is('SMTP')"), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("input[type='checkbox'][name='ISite.SmtpSettings.IsEnabled']"), pauseAfter: 600);
        });

        await scene.StepAsync("config-3", async () =>
        {
            await scene.TypeAsync(page.Locator("[name='ISite.SmtpSettings.DefaultSender']"), "noreply@example.com", delay: 25);
        });

        await scene.StepAsync("config-4", async () =>
        {
            await scene.HighlightAsync(page.Locator("[name='ISite.SmtpSettings.DeliveryMethod']"));
        });

        await scene.StepAsync("config-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var host = page.Locator("[name='ISite.SmtpSettings.Host']");
            await host.ScrollToMiddleAsync();
            await host.FillAsync(string.Empty);
            await scene.TypeAsync(host, "localhost", delay: 30);
            var port = page.Locator("[name='ISite.SmtpSettings.Port']");
            await port.FillAsync(string.Empty);
            await scene.TypeAsync(port, "2525", delay: 40);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("config-6", async () =>
        {
            await page.Locator("[name='ISite.SmtpSettings.EncryptionMethod']").ScrollToMiddleAsync();
            await scene.SelectAsync(page.Locator("[name='ISite.SmtpSettings.EncryptionMethod']"), "None", pauseAfter: 400);
            await scene.HighlightAsync(page.Locator("input[type='checkbox'][name='ISite.SmtpSettings.AutoSelectEncryption']"));
        });

        await scene.StepAsync("config-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            var credentials = page.Locator("input[type='checkbox'][name='ISite.SmtpSettings.RequireCredentials']");
            await credentials.ScrollToMiddleAsync();
            await scene.ClickAsync(credentials, pauseAfter: 600);

            foreach (var label in new[] { "Use Windows authentication", "User name", "Password" })
            {
                var text = page.GetByText(label, new() { Exact = true }).First;

                if (await text.IsVisibleAsync())
                {
                    await scene.MoveToAsync(text);
                    await scene.PauseAsync(1200);
                }
            }

            await scene.ClickAsync(credentials, pauseAfter: 600);
        });

        await scene.StepAsync("config-8", async () =>
        {
            await GoAsync(scene, page.Locator("button.primaryAction.save").First);
            await scene.ScrollAsync(0, -2000);
        });

        await scene.StepAsync("config-9", async () =>
        {
            await scene.HighlightAsync(page.Locator("[name='ISite.EmailSettings.DefaultProvider']"));
        });
    }

    private static async Task TestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("test-1", async () =>
        {
            await scene.ClickAsync(Group(page, "tools"), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("li.oc-navigation-admin-tools-testing > figure span.title, button:has-text('Testing')").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Email/Test'] >> visible=true").First);
            await page.Locator("#Provider").WaitForAsync();
            await scene.HighlightAsync(page.Locator("#Provider"));
        });

        await scene.StepAsync("test-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#To"), "jamie.chen@example.com", delay: 25);
            await scene.HighlightAsync(page.Locator("#From"));
        });

        await scene.StepAsync("test-3", async () =>
        {
            await scene.ClearHighlightsAsync();

            foreach (var id in new[] { "Cc", "Bcc", "ReplyTo" })
            {
                await scene.MoveToAsync(page.Locator($"#{id}"));
                await scene.PauseAsync(500);
            }

            await scene.ClickAsync(page.Locator("#Subject"), pauseAfter: 100);
            await page.Locator("#Subject").FillAsync("Test message");
            await scene.ClickAsync(page.Locator("#Body"), pauseAfter: 100);
            await page.Locator("#Body").FillAsync("Hello from Orchard Core.");
        });

        await scene.StepAsync("test-4", async () =>
        {
            await GoAsync(scene, page.Locator("button:text-is('Send')"));
            await WaitTextAsync(page, "Message sent successfully.");
        });
    }

    private static async Task InboxAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        var inbox = await scene.Context.NewPageAsync();
        await inbox.GotoAsync(MailUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await scene.OpenAsync("/Admin/Email/Test");
        await inbox.GetByText("Test message").First.WaitForAsync();
        await page.BringToFrontAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("inbox-1", async () =>
        {
            await scene.SwitchToAsync(inbox);
            await scene.HighlightAsync(inbox.GetByText("Test message").First);
        });

        await scene.StepAsync("inbox-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(inbox.GetByText("Test message").First, pauseAfter: 300);
            await inbox.FrameLocator("iframe").GetByText("Hello from Orchard Core.").First.WaitForAsync();
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the forms.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Secrets: Store Passwords and API Keys Safely". Every secret is invented; typed values show as dots.
/// Record the clips one after the other, with the seed between 04-types and 05-review (see recording-plan.md):
/// 02-enable, 03-create, 04-types, (SM_MODE=seed Recorder prepare), 05-review, 06-update, 07-delete, 08-settings.
/// </summary>
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
        ("03-create", CreateAsync),
        ("04-types", TypesAsync),
        ("05-review", ReviewAsync),
        ("06-update", UpdateAsync),
        ("07-delete", DeleteAsync),
        ("08-settings", SettingsAsync),
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

    private static string Day(int offset) => DateTime.UtcNow.Date.AddDays(offset).ToString("yyyy-MM-dd");

    // SM_MODE=seed creates the four *.ApiKey secrets (fake names and values), outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        if (Environment.GetEnvironmentVariable("SM_MODE") == "seed")
        {
            await PostSecretAsync(page, "Payments.ApiKey", "fake-payments-value-1", "API key of the payment service", Day(12));
            await PostSecretAsync(page, "Analytics.ApiKey", "fake-analytics-value-1", "API key of the analytics service", Day(-20));
            await PostSecretAsync(page, "Newsletter.ApiKey", "fake-newsletter-value-1", "API key of the newsletter service", null);
            await PostSecretAsync(page, "Survey.ApiKey", "fake-survey-value-1", "API key of the survey service", null);

            await page.GotoAsync(BaseUrl + "/Admin/Secrets/Index", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            Console.WriteLine(await page.Locator("ul.list-group").InnerTextAsync());
        }

        await context.CloseAsync();
    }

    private static async Task PostSecretAsync(IPage page, string name, string value, string description, string expires)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Secrets/Create?type=TextSecret", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var status = await page.EvaluateAsync<int>(
            """
            async (a) => {
                const form = document.querySelector('form[action*="Secrets/Create"]');
                const body = new URLSearchParams();
                body.set('__RequestVerificationToken', form.querySelector('input[name="__RequestVerificationToken"]').value);
                body.set('SecretType', 'TextSecret');
                body.set('Name', a.name);
                body.set('Store', 'Database');
                body.set('TextValue', a.value);
                body.set('Description', a.description);
                if (a.expires) { body.set('ExpiresUtc', a.expires); }
                const response = await fetch(form.action, { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body });
                return response.status;
            }
            """,
            new { name, value, description, expires });
        Console.WriteLine($"Seed {name}: HTTP {status}");
    }

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.SetDefaultTimeout(120_000);
        return scene;
    }

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    // Moves to a link or button, then clicks it with the page load cut off camera.
    private static async Task GoAsync(Scene scene, ILocator locator, int lead = 300)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), lead);
    }

    private static async Task OpenToolsAsync(Scene scene)
    {
        var tools = scene.Page.Locator("#left-nav button.nav-group-label").Filter(new() { HasTextRegex = Exact("Tools") }).First;

        if (await tools.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(tools, pauseAfter: 200);
        }
    }

    private static async Task OpenAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.Locator("#left-nav").First.WaitForAsync();
        await Task.Delay(400);
    }

    private static ILocator Row(Scene scene, string name) => scene.Page.Locator($"li.list-group-item:has(a:text-is(\"{name}\"))").First;

    private static async Task OpenAddDialogAsync(Scene scene)
    {
        await scene.ClickAsync(scene.Page.Locator("button.create").First, pauseAfter: 200);
        await scene.Page.Locator("#modalSecretTypes .card").First.WaitForAsync();
        await Task.Delay(500);
    }

    private static ILocator Card(Scene scene, string title) => scene.Page.Locator($"#modalSecretTypes .card:has(h5:text-is(\"{title}\"))").First;

    // Clicks Save with the page load cut off camera; the toast then is on screen when the narration ends.
    private static async Task SaveAsync(Scene scene)
    {
        var save = scene.Page.Locator("button.save").First;
        await GoAsync(scene, save, 200);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAsync(scene, "/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.PauseAsync(300);
            await OpenToolsAsync(scene);
            await GoAsync(scene, scene.Page.Locator("#left-nav a.item-label:has(span.title:text-is('Features'))").First);
            await scene.Page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Secrets", delay: 10);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var enable = scene.Page.Locator("#btn-enable-OrchardCore_Secrets").First;
            await scene.HighlightAsync(enable);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, enable, 200);
            await scene.Page.Locator(".toast").First.WaitForAsync();
            await scene.MoveToAsync(1000, 340, steps: 8);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await OpenToolsAsync(scene);
            var security = scene.Page.Locator("#left-nav button.oc-navigation-admin-tools-security").First;

            if (await security.GetAttributeAsync("aria-expanded") == "false")
            {
                await scene.ClickAsync(security, pauseAfter: 300);
            }

            var group = scene.Page.Locator("#left-nav li.oc-navigation-admin-tools-security ul").First;
            await scene.HighlightAsync(group);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, scene.Page.Locator("#left-nav a.item-label:has(span.title:text-is('Secrets'))").First);
            await scene.Page.Locator("button.create").First.WaitForAsync();
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button.create").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAsync(scene, "/Admin/Secrets/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.PauseAsync(300);
            await OpenAddDialogAsync(scene);
            await scene.MoveToAsync(Card(scene, "Text Secret").Locator("h5").First);
            await scene.MoveToAsync(Card(scene, "RSA Key").Locator("h5").First);
            await scene.MoveToAsync(Card(scene, "X.509 Certificate").Locator("h5").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.HighlightAsync(Card(scene, "Text Secret"));
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, Card(scene, "Text Secret").Locator("a.btn").First);
            await scene.Page.Locator("#Name").WaitForAsync();
            await Task.Delay(600);
            await scene.TypeAsync(scene.Page.Locator("#Name").First, "Smtp.Password", delay: 10);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#store-display").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#TextValue").First, "fake-mail-value-1", delay: 10);
            await scene.HighlightAsync(scene.Page.Locator("#TextValue").First);
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#Description").First, "Password of the mail account that sends the site email", delay: 8);
            await scene.Page.Locator("#ExpiresUtc").FillAsync(Day(365));
            await scene.HighlightAsync(scene.Page.Locator("#ExpiresUtc").First);
        });

        await scene.StepAsync("create-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HoldAsync(Math.Max(0.5, durations["create-6"] - 6.0));
            await SaveAsync(scene);
            await scene.Page.Locator("li.list-group-item:has(a:text-is(\"Smtp.Password\"))").First.WaitForAsync();
        });
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAsync(scene, "/Admin/Secrets/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("types-1", async () =>
        {
            await scene.PauseAsync(300);
            await OpenAddDialogAsync(scene);
            await GoAsync(scene, Card(scene, "RSA Key").Locator("a.btn").First);
            await scene.Page.Locator("#Name").WaitForAsync();
            await Task.Delay(600);
            await scene.TypeAsync(scene.Page.Locator("#Name").First, "Signing.Key", delay: 10);
            await scene.HighlightAsync(scene.Page.Locator("#RsaKeySize").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#Description").First, "Example RSA key pair", delay: 8);
            await scene.HoldAsync(Math.Max(0.3, durations["types-1"] * 0.6 - 7.5));
            await SaveAsync(scene);
            await scene.Page.Locator("li.list-group-item:has(a:text-is(\"Signing.Key\"))").First.WaitForAsync();
        });

        await scene.StepAsync("types-2", async () =>
        {
            await GoAsync(scene, Row(scene, "Signing.Key").Locator("a.btn-primary").First);
            await scene.Page.Locator(".alert-info").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".alert-info").First);
        });

        await scene.StepAsync("types-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, scene.Page.Locator("a.cancel").First);
            await scene.Page.Locator("button.create").First.WaitForAsync();
            await OpenAddDialogAsync(scene);
            await GoAsync(scene, Card(scene, "X.509 Certificate").Locator("a.btn").First);
            await scene.Page.Locator("#X509Source").WaitForAsync();
            await Task.Delay(600);
            await scene.HighlightAsync(scene.Page.Locator("#X509Source").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#X509Source").First, "PEM-encoded certificate");
        });
    }

    private static async Task ReviewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAsync(scene, "/Admin/Secrets/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("review-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".secrets-expiration-warning").First);
        });

        await scene.StepAsync("review-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Analytics.ApiKey").Locator("span.text-bg-danger").First);
            await scene.PauseAsync((int)Math.Min(4500, durations["review-2"] * 450));
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Payments.ApiKey").Locator("span.text-bg-warning").First);
        });

        await scene.StepAsync("review-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "payment", delay: 10);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter")), 200);
        });

        await scene.StepAsync("review-4", async () =>
        {
            await scene.OffCameraAsync(() => scene.Page.GotoAsync(BaseUrl + "/Admin/Secrets/Index", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }), 100);
            await scene.ClickAsync(scene.Page.Locator("div.filter button.dropdown-toggle:has-text(\"Expiration\")").First, pauseAfter: 300);
            var expired = scene.Page.Locator(".dropdown-menu.show a:has(span.text:text-is(\"Expired\"))").First;
            await scene.MoveToAsync(expired);
            var (x, y) = await scene.CenterOfAsync(expired);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 200);
            await scene.HoldAsync(1.0);
            await scene.OffCameraAsync(() => scene.Page.GotoAsync(BaseUrl + "/Admin/Secrets/Index", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }), 100);
            await scene.ClickAsync(scene.Page.Locator("div.filter button.dropdown-toggle:has-text(\"Type\")").First, pauseAfter: 300);
            var rsa = scene.Page.Locator(".dropdown-menu.show a:has(span.text:text-is(\"RSA Key\"))").First;
            await scene.MoveToAsync(rsa);
            var (rx, ry) = await scene.CenterOfAsync(rsa);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)rx, (float)ry)), 200);
        });

        await scene.StepAsync("review-5", async () =>
        {
            await scene.OffCameraAsync(() => scene.Page.GotoAsync(BaseUrl + "/Admin", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }), 100);
            await scene.Page.Locator(".secrets-expiration-warning").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".secrets-expiration-warning").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task UpdateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAsync(scene, "/Admin/Secrets/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("update-1", async () =>
        {
            await scene.PauseAsync(300);
            await GoAsync(scene, Row(scene, "Payments.ApiKey").Locator("a.btn-primary").First);
            await scene.Page.Locator("#TextValue").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#TextValue").First);
        });

        await scene.StepAsync("update-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#TextValue").First, "fake-payments-value-2", delay: 10);
            await scene.Page.Locator("#ExpiresUtc").FillAsync(Day(365));
            await scene.HoldAsync(Math.Max(0.3, durations["update-2"] - 5.5));
            await SaveAsync(scene);
            await scene.Page.Locator("li.list-group-item:has(a:text-is(\"Payments.ApiKey\"))").First.WaitForAsync();
        });
    }

    private static async Task DeleteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAsync(scene, "/Admin/Secrets/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("delete-1", async () =>
        {
            await scene.PauseAsync(300);
            await scene.ClickAsync(Row(scene, "Analytics.ApiKey").Locator("a.btn-danger").First, pauseAfter: 200);
            await scene.Page.Locator("#modalOkButton").First.WaitForAsync();
            await scene.HoldAsync(Math.Max(1.5, durations["delete-1"] - 6.0));
            var ok = scene.Page.Locator("#modalOkButton").First;
            await scene.MoveToAsync(ok);
            var (x, y) = await scene.CenterOfAsync(ok);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 200);
        });

        await scene.StepAsync("delete-2", async () =>
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            await QuickClickAsync(scene, scene.Page.Locator("input[value=\"Database:Newsletter.ApiKey\"]").First);
            await QuickClickAsync(scene, scene.Page.Locator("input[value=\"Database:Survey.ApiKey\"]").First);
            await QuickClickAsync(scene, scene.Page.Locator("#bulk-action-menu-button").First);
            await QuickClickAsync(scene, scene.Page.Locator(".dropdown-item[data-action=\"Remove\"]").First);
            await scene.Page.Locator("#modalOkButton").First.WaitForAsync();
            await scene.HoldAsync(Math.Max(0.3, durations["delete-2"] - 3.8 - clock.Elapsed.TotalSeconds));
            var ok = scene.Page.Locator("#modalOkButton").First;
            await scene.MoveToAsync(ok);
            var (x, y) = await scene.CenterOfAsync(ok);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 200);
        });
    }

    // A click with a short pointer move, for steps that must finish early on a loaded machine.
    private static async Task QuickClickAsync(Scene scene, ILocator locator)
    {
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, steps: 8);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await Task.Delay(150);
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAsync(scene, "/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await scene.PauseAsync(300);
            await scene.ClickAsync(scene.Page.Locator("#left-nav button.nav-group-label").Filter(new() { HasTextRegex = Exact("Settings") }).First, pauseAfter: 200);
            await scene.ClickAsync(scene.Page.Locator("#left-nav button.item-label:has(span.title:text-is('Communication'))").First, pauseAfter: 200);
            await GoAsync(scene, scene.Page.Locator("#left-nav a.item-label:has(span.title:text-is('Email'))").First);
            await scene.Page.Locator("a.nav-link:text-is(\"SMTP\")").First.WaitForAsync();
            await scene.ClickAsync(scene.Page.Locator("a.nav-link:text-is(\"SMTP\")").First, pauseAfter: 300);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label:text-is(\"Enable SMTP Provider\")").First, pauseAfter: 150);
            await scene.TypeAsync(scene.Page.Locator("[id$='_DefaultSender']").First, "noreply@example.com", delay: 8);
            await scene.TypeAsync(scene.Page.Locator("[id$='_Host']").First, "smtp.example.com", delay: 8);
            await scene.ClickAsync(scene.Page.Locator("label:text-is(\"Require credentials\")").First, pauseAfter: 150);
            await scene.TypeAsync(scene.Page.Locator("[id$='_UserName']").First, "mailer@example.com", delay: 8);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("select[data-secret-input-source]").First);
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("select[data-secret-input-source]").First, "Use a secret");
            await scene.SelectAsync(scene.Page.Locator("[data-secret-input-secrets] select").First, "Smtp.Password");
            await scene.HighlightAsync(scene.Page.Locator("a:has-text('New secret')").First);
            await scene.HighlightAsync(scene.Page.Locator("button[data-secret-input-reload]").First);
        });

        await scene.StepAsync("settings-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HoldAsync(Math.Max(0.3, durations["settings-5"] - 4.4));
            await SaveAsync(scene);
            await scene.Page.Locator("select[data-secret-input-source]").First.WaitForAsync(new() { State = WaitForSelectorState.Attached });
        });

        await scene.StepAsync("settings-6", async () =>
        {
            await scene.PauseAsync(300);
            await QuickClickAsync(scene, scene.Page.Locator("a.nav-link:text-is(\"SMTP\")").First);
            await scene.Page.Locator("select[data-secret-input-source]").First.WaitForAsync();
            await scene.ScrollAsync(0, 700, steps: 10);
            await scene.PauseAsync(500);
        });
    }
}

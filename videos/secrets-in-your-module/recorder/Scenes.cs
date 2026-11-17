using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Secrets in Your Module". PrepareAsync enables Email and SMTP Email Provider and saves the SMTP settings (no password).
/// Record 01-without; then run "record x-secrets" (not a clip: it enables Secrets and creates the secret Smtp.Password off camera); then record 01-with.
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
        ("01-without", WithoutAsync),
        ("x-secrets", SecretsAsync),
        ("01-with", WithAsync),
    ];

    private static async Task EnableFeatureAsync(IPage page, string search, string rowName, string id)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Features", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#search-box").FillAsync(search);

        if (await page.Locator($"#btn-enable-{id}").CountAsync() > 0)
        {
            await page.Locator($"#btn-enable-{id}").ClickAsync();
            await page.GetByText("was enabled.").First.WaitForAsync();
        }

        Console.WriteLine("Enabled " + rowName);
    }

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90000);

        await EnableFeatureAsync(page, "Email", "Email", "OrchardCore_Email");
        await EnableFeatureAsync(page, "SMTP", "SMTP Email Provider", "OrchardCore_Email_Smtp");

        await page.GotoAsync(BaseUrl + "/Admin/Settings/email", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("a[role='tab']:text-is('SMTP')").ClickAsync();
        await page.GetByLabel("Enable SMTP Provider", new() { Exact = true }).CheckAsync();
        await page.GetByLabel("Sender email address", new() { Exact = true }).FillAsync("noreply@example.com");
        await page.GetByLabel("Delivery method", new() { Exact = true }).SelectOptionAsync(new SelectOptionValue { Label = "Network" });
        await page.GetByLabel("Host name", new() { Exact = true }).FillAsync("smtp.example.com");
        await page.GetByLabel("Port number", new() { Exact = true }).FillAsync("25");
        await page.GetByLabel("Require credentials", new() { Exact = true }).CheckAsync();
        await page.GetByLabel("User name", new() { Exact = true }).FillAsync("orchard-mailer");
        await page.Locator("button.save").First.ClickAsync();
        await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        Console.WriteLine("Saved the SMTP settings.");
        await context.CloseAsync();
    }

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips.Where(clip => clip.Name != "x-secrets").ToArray() : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    // Enables Secrets and creates the secret Smtp.Password. Not a clip: nothing is recorded.
    private static async Task SecretsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90000);

        await EnableFeatureAsync(page, "Secrets", "Secrets", "OrchardCore_Secrets");
        await page.GotoAsync(BaseUrl + "/Admin/Secrets/Index", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("button.create").First.ClickAsync();
        await page.Locator("#modalSecretTypes.show").WaitForAsync();
        await page.Locator("#modalSecretTypes .card:has(h5:text-is('Text Secret')) a").First.ClickAsync();
        await page.Locator("#Name").FillAsync("Smtp.Password");
        await page.Locator("#TextValue").First.FillAsync("sample-secret-value");
        await page.Locator("#Description").FillAsync("SMTP password of the sample site");
        await page.Locator("button.save").First.ClickAsync();
        await page.GetByText("Smtp.Password").First.WaitForAsync();
        Console.WriteLine("Created the secret Smtp.Password.");
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task ScrollUntilVisibleAsync(Scene scene, ILocator label)
    {
        await label.EvaluateAsync("e => e.scrollIntoView({ behavior: 'instant', block: 'center' })");
        await scene.PauseAsync(300);
    }

    private static async Task OpenSmtpAsync(Scene scene)
    {
        var tab = scene.Page.Locator("a[role='tab']:text-is('SMTP')").First;

        if (!(await tab.GetAttributeAsync("class") ?? string.Empty).Contains("active"))
        {
            await scene.ClickAsync(tab, pauseAfter: 400);
        }

        var label = scene.Page.Locator("label[for$='_Password']").First;
        await label.WaitForAsync();
        await scene.PauseAsync(300);
        await ScrollUntilVisibleAsync(scene, label);
    }

    private static async Task SaveAsync(Scene scene)
    {
        var save = scene.Page.Locator("button.save").First;
        await scene.MoveToAsync(save);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(() => save.ClickAsync());
            await scene.Page.GetByText("Site settings updated successfully.").First.WaitForAsync(new() { Timeout = 60000 });
        }, leadMilliseconds: 150);
    }

    private static async Task WithoutAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/email");
        await scene.Page.Locator("a[role='tab']:text-is('SMTP')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("editor-1", async () =>
        {
            await OpenSmtpAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("input[type='password'][id$='_Password']").First);
        });

        await scene.StepAsync("editor-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("input[type='password'][id$='_Password']").First, "not-a-real-password", delay: 30);
            await SaveAsync(scene);
            await OpenSmtpAsync(scene);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WithAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/email");
        await scene.Page.Locator("a[role='tab']:text-is('SMTP')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        var source = scene.Page.Locator("select[data-secret-input-source]").First;
        var secrets = scene.Page.Locator("[data-secret-input-secrets] select").First;

        await scene.StepAsync("editor-3", async () =>
        {
            await OpenSmtpAsync(scene);
            await scene.HighlightAsync(source);
        });

        await scene.StepAsync("editor-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(source, "Use a secret");
            await secrets.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("button[data-secret-input-reload]").First);
            await scene.PauseAsync(700);
            await scene.HighlightAsync(scene.Page.Locator("a:has-text('New secret')").First);
        });

        await scene.StepAsync("editor-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(secrets, "Smtp.Password");
            await SaveAsync(scene);
            await OpenSmtpAsync(scene);
            await scene.NudgeAsync();
            await scene.PauseAsync(800);
        });

        await scene.ClearHighlightsAsync();
    }
}

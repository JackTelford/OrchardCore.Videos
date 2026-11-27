using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("02-email", EmailAsync),
        ("03-allow", AllowAsync),
        ("04-request", RequestAsync),
        ("05-mail", MailAsync),
        ("06-reset", ResetAsync),
    ];

    private const string FirstPassword = "Start#1234";
    private const string NewPassword = "Fresh#5678";

    // A local smtp4dev container: web page on 3015, SMTP on 2535.
    private const string MailUrl = "http://localhost:3015";

    // Enables Email and SMTP, points SMTP at the local catcher, and creates Jamie (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await EnableFeaturesAsync(page, "OrchardCore.Email", "OrchardCore.Email.Smtp");
        await Task.Delay(3000);
        await page.GotoAsync(BaseUrl + "/Admin/Settings/email", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("a.nav-link:text-is(\"SMTP\")").ClickAsync();
        await page.Locator("#ISite_SmtpSettings_IsEnabled").CheckAsync();
        await page.Locator("#ISite_SmtpSettings_DefaultSender").FillAsync("noreply@example.com");
        await page.Locator("#ISite_SmtpSettings_Host").FillAsync("localhost");
        await page.Locator("#ISite_SmtpSettings_Port").FillAsync("2535");
        await page.Locator("#ISite_SmtpSettings_AutoSelectEncryption").UncheckAsync();
        await page.Locator("#ISite_SmtpSettings_RequireCredentials").UncheckAsync();
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.save").ClickAsync());
        await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"command\",\"Commands\":[\"createUser /UserName:jamie.chen /Password:" + FirstPassword + " /Email:jamie.chen@example.com\"]}]}");
        Console.WriteLine("Seeded.");
        await context.CloseAsync();
    }

    private static async Task<Scene> NewVisitorSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "anonymous.json"), Path.GetDirectoryName(directory), durations);

    private static ILocator FeatureRow(IPage page)
        => page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_Users_ResetPassword, #btn-disable-OrchardCore_Users_ResetPassword") }).First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "reset password", 40);
            await page.Locator("#btn-enable-OrchardCore_Users_ResetPassword").WaitForAsync();
            await scene.HighlightAsync(FeatureRow(page));
            await scene.MoveToAsync(FeatureRow(page).Locator(".direct-dependencies"));
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("#btn-enable-OrchardCore_Users_ResetPassword");
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(enable));
            await page.GetByText("The feature Users Reset Password was enabled.").First.WaitForAsync(new() { Timeout = 30000 });
        });
    }

    private static async Task EmailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/email");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("email-1", async () =>
            await scene.HighlightAsync(page.GetByLabel("Default provider"), "Default provider"));

        await scene.StepAsync("email-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("a.nav-link:text-is(\"SMTP\")"));
            foreach (var id in new[] { "DefaultSender", "Host", "Port" })
            {
                await scene.HighlightAsync(page.Locator("#ISite_SmtpSettings_" + id));
                await scene.PauseAsync(900);
            }
        });
    }

    private static async Task AllowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/userResetPassword");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var allow = page.GetByLabel("Allow the users to reset their password");
        var theme = page.GetByLabel("Use site theme for password reset page");

        await scene.StepAsync("allow-1", async () =>
        {
            var entry = page.Locator("a[href$='/Admin/Settings/userResetPassword']").First;
            if (await entry.IsVisibleAsync())
            {
                await scene.HighlightAsync(entry, "Settings, Security, Reset Password");
                await scene.PauseAsync(1500);
            }

            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(allow);
            await scene.HighlightAsync(theme);
        });

        await scene.StepAsync("allow-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, allow);
        });

        await scene.StepAsync("allow-3", async () =>
        {
            await scene.HighlightAsync(theme);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        });
    }

    private static async Task RequestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewVisitorSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Login");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var forgot = page.GetByRole(AriaRole.Link, new() { Name = "Forgot your password?" });

        await scene.StepAsync("request-1", async () => await scene.HighlightAsync(forgot));

        await scene.StepAsync("request-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, forgot);
            await FastTypeAsync(scene, page.GetByLabel("Username or email address"), "jamie.chen@example.com", 10);
        });

        await scene.StepAsync("request-3", async () =>
        {
            await FastGoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Submit" }));
            await page.GetByText("Please check your email to reset your password.").First.WaitForAsync();
        });
    }

    private static async Task MailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewVisitorSceneAsync(browser, directory, durations);
        await scene.Page.GotoAsync(MailUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var message = page.Locator(".messagelist .el-table__body .el-table__row").First;

        await scene.StepAsync("mail-1", async () => await scene.HighlightAsync(message));

        await scene.StepAsync("mail-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, message);
            var link = page.FrameLocator("iframe").GetByRole(AriaRole.Link, new() { Name = "Reset Password" });
            await link.WaitForAsync();
            await scene.MoveToAsync(500, 480, 20);
        });
    }

    private static async Task ResetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewVisitorSceneAsync(browser, directory, durations);
        var page = scene.Page;

        // The link of the newest message is read off camera from the catcher's API and opened by address.
        using var http = new HttpClient();
        var list = await http.GetStringAsync(MailUrl + "/api/messages");
        var id = Regex.Match(list, "\"id\":\"([0-9a-f-]+)\"").Groups[1].Value;
        var html = await http.GetStringAsync($"{MailUrl}/api/messages/{id}/html");
        var link = System.Net.WebUtility.HtmlDecode(Regex.Match(html, "href=\"([^\"]*ResetPassword[^\"]*)\"").Groups[1].Value);
        await page.GotoAsync(link, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await scene.StartCaptureAsync();

        await scene.StepAsync("reset-1", async () =>
        {
            foreach (var locator in new[] { page.GetByLabel("Username or email address"), page.GetByLabel("New password", new() { Exact = true }), page.GetByLabel("New password confirmation") })
            {
                await scene.HighlightAsync(locator);
                await scene.PauseAsync(700);
            }
        });

        await scene.StepAsync("reset-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, page.GetByLabel("Username or email address"), "jamie.chen@example.com", 6);
            await FastTypeAsync(scene, page.GetByLabel("New password", new() { Exact = true }), NewPassword, fill: true);
            await FastTypeAsync(scene, page.GetByLabel("New password confirmation"), NewPassword, fill: true);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("reset-3", async () =>
        {
            await FastGoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Reset password" }));
            await page.GetByText("Your password has been reset.").First.WaitForAsync();
        });

        await scene.StepAsync("reset-4", async () =>
        {
            await FastGoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Click here to log in" }));
            await FastTypeAsync(scene, page.GetByLabel("Username or email address"), "jamie.chen@example.com", 6);
            await FastTypeAsync(scene, page.GetByLabel("Password", new() { Exact = true }), NewPassword, fill: true);
            await FastGoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Log in" }));
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-settings", SettingsAsync),
        ("04-register", RegisterAsync),
        ("05-verify-settings", VerifySettingsAsync),
        ("05-verify-visitor", VerifyVisitorAsync),
        ("06-approve-settings", ApproveSettingsAsync),
        ("06-approve-visitor", ApproveVisitorAsync),
        ("06-approve-admin", ApproveAdminAsync),
    ];

    private const string VisitorPassword = "Visitor#2468";

    // A local smtp4dev container (web page on 3005, SMTP on 2525) catches the confirmation message.
    private const string InboxUrl = "http://localhost:3005";

    // Enables the SMTP provider and points it at the local inbox, outside the capture.
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
        await page.Locator("#ISite_SmtpSettings_Port").FillAsync("2525");
        await page.Locator("#ISite_SmtpSettings_AutoSelectEncryption").UncheckAsync();
        await page.Locator("#ISite_SmtpSettings_RequireCredentials").UncheckAsync();
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.save").ClickAsync());
        Console.WriteLine("Seeded.");
        await context.CloseAsync();
    }

    private static Task<Scene> NewAnonymousSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "anonymous.json"), Path.GetDirectoryName(directory), durations);

    private static ILocator Reg(IPage page, string name) => page.Locator($"input[type=checkbox][name$=\"{name}\"]");

    private static ILocator RegBox(IPage page, string name) => Reg(page, name).Locator("xpath=..");

    private static ILocator Row(IPage page, string name) => page.Locator($"li.list-group-item:has-text(\"{name}@example.com\")");

    private static async Task FillRegisterAsync(Scene scene, string user)
    {
        var page = scene.Page;
        await FastTypeAsync(scene, page.Locator("input[name$=\"UserName\"]"), user);
        await FastTypeAsync(scene, page.Locator("input[name$=\"Email\"]"), user + "@example.com");
        await FastTypeAsync(scene, page.Locator("input[name$=\"Password\"]:not([name$=\"ConfirmPassword\"])"), VisitorPassword, fill: true);
        await FastTypeAsync(scene, page.Locator("input[name$=\"ConfirmPassword\"]"), VisitorPassword, fill: true);
    }

    // Opens the groups of the admin menu in turn, then follows the last link.
    private static async Task MenuAsync(Scene scene, params string[] path)
    {
        for (var i = 0; i < path.Length - 1; i++)
        {
            var group = scene.Page.Locator($"#left-nav button.item-label:has(span.title:text-is(\"{path[i]}\"))");
            if (await group.GetAttributeAsync("aria-expanded") != "true")
            {
                await FastClickAsync(scene, group, 350);
            }
        }

        await FastGoAsync(scene, scene.Page.Locator($"#left-nav a.item-label:has(span.title:text-is(\"{path[^1]}\"))"));
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await MenuAsync(scene, "Tools", "Features");
            await scene.TypeAsync(page.Locator("#search-box"), "Users Registration", 30);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await FastGoAsync(scene, page.Locator("#btn-enable-OrchardCore_Users_Registration"));
            await scene.TypeAsync(page.Locator("#search-box"), "Users Registration", 20);
            await scene.HighlightAsync(page.Locator(".list-group-item:has(#btn-disable-OrchardCore_Users_Registration) .direct-dependencies"));
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await MenuAsync(scene, "Settings", "Communication", "Email");
            var providers = page.Locator("a.nav-link:text-is(\"Providers\")");
            if (await providers.CountAsync() > 0 && !(await providers.First.GetAttributeAsync("class") ?? "").Contains("active"))
            {
                await FastClickAsync(scene, providers.First);
            }

            await PointAsync(scene, page.Locator("select[name$=\"DefaultProvider\"]"));
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("settings-1", async () =>
        {
            await MenuAsync(scene, "Settings", "Security", "Registration");
            foreach (var name in new[] { "UsersMustValidateEmail", "UsersAreModerated", "UseSiteTheme" })
            {
                await scene.HighlightAsync(RegBox(page, name));
            }
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, Reg(page, "UsersMustValidateEmail"));
            await scene.HighlightAsync(RegBox(page, "UsersMustValidateEmail"));
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, Reg(page, "UsersMustValidateEmail"));
            await scene.HighlightAsync(RegBox(page, "UsersAreModerated"));
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RegBox(page, "UseSiteTheme").Locator("xpath=.."));
        });
    }

    private static async Task RegisterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewAnonymousSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Login");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("register-1", async () =>
        {
            var link = page.Locator("a:has-text(\"Register as a new user\")");
            await PointAsync(scene, link);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, link);
        });

        await scene.StepAsync("register-2", async () => await FillRegisterAsync(scene, "jamie.chen"));

        await scene.StepAsync("register-3", async () =>
            await FastGoAsync(scene, page.Locator("button[type=submit]:text-is(\"Register\")")));
    }

    private static async Task VerifySettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/userRegistration");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("verify-1", async () =>
        {
            await FastClickAsync(scene, Reg(page, "UsersMustValidateEmail"), 700);
            await scene.PauseAsync(4200);
            await FastGoAsync(scene, page.Locator("button.save"));
        });
    }

    private static async Task VerifyVisitorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewAnonymousSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Register");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        IPage inbox = null;

        await scene.StepAsync("verify-2", async () =>
        {
            await FillRegisterAsync(scene, "morgan.lee");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button[type=submit]:text-is(\"Register\")"));
        });

        await scene.StepAsync("verify-3", async () =>
        {
            inbox = await scene.Page.Context.NewPageAsync();
            await inbox.GotoAsync(InboxUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await scene.SwitchToAsync(inbox);
            await FastClickAsync(scene, inbox.Locator(".messagelist .el-table__body .el-table__row").First);
        });

        await scene.StepAsync("verify-4", async () =>
        {
            var link = inbox.FrameLocator("iframe").Locator("a:has-text(\"click here\")");
            var href = await link.GetAttributeAsync("href");
            await scene.WaitForNavigationAsync(async () => await inbox.GotoAsync(href));
        });
    }

    private static async Task ApproveSettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/userRegistration");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("approve-1", async () =>
        {
            await FastClickAsync(scene, Reg(page, "UsersMustValidateEmail"));
            await FastClickAsync(scene, Reg(page, "UsersAreModerated"), 700);
            await scene.PauseAsync(4000);
            await FastGoAsync(scene, page.Locator("button.save"));
        });
    }

    private static async Task ApproveVisitorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewAnonymousSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Register");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("approve-2", async () =>
        {
            await FillRegisterAsync(scene, "riley.park");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button[type=submit]:text-is(\"Register\")"));
        });
    }

    private static async Task ApproveAdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("approve-3", async () =>
        {
            await MenuAsync(scene, "Access Control", "Users");
            var row = Row(page, "riley.park");
            await PointAsync(scene, row.Locator(".badge:has-text(\"Disabled\")"));
            await scene.PauseAsync(700);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, row.Locator("button.actions"));
            await PointAsync(scene, row.Locator(".actions-menu a.dropdown-item:text-is(\"Enable\")"));
        });

        await scene.StepAsync("approve-4", async () =>
        {
            var row = Row(page, "riley.park");
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, row.Locator(".actions-menu a.dropdown-item:text-is(\"Enable\")"));
            await scene.PauseAsync(900);
            await FastGoAsync(scene, page.Locator("#modalOkButton"));
        });
    }
}

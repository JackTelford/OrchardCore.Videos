using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-open", OpenAsync),
        ("03-remember", RememberAsync),
        ("04-theme", ThemeAsync),
        ("05-local", LocalAsync),
        ("06-details", DetailsAsync),
        ("07-lockout", LockoutAsync),
    ];

    private const string UserPassword = "Password1!";

    // Seeds the user jamie.chen (a confirmed email), outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"command\",\"Commands\":[\"createUser /UserName:jamie.chen /Password:" + UserPassword + " /Email:jamie.chen@example.com\"]}]}");
        Console.WriteLine("Seeded.");
        await context.CloseAsync();
    }

    private static ILocator Cb(IPage page, string name) => page.Locator("#ISite_LoginSettings_" + name);

    // The checkbox with its label and hint.
    private static ILocator Box(IPage page, string name) => Cb(page, name).Locator("xpath=../..");

    private static ILocator Row(IPage page, string name) => page.Locator($"li.list-group-item:has-text(\"{name}@example.com\")");

    private static ILocator LogInButton(IPage page) => page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true });

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Login");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("open-1", async () =>
        {
            foreach (var locator in new[] { page.Locator("#LoginForm_UserName"), page.Locator("#LoginForm_Password"), page.Locator("#LoginForm_RememberMe"), LogInButton(page) })
            {
                await PointAsync(scene, locator);
                await scene.PauseAsync(900);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("open-2", async () =>
        {
            await scene.NavigateAsync("/Admin");
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Settings\"))"), 400);
            await FastClickAsync(scene, page.Locator("#left-nav button.item-label:has(span.title:text-is(\"Security\"))"), 400);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"User Login\"))"));
        });

        await scene.StepAsync("open-3", async () =>
        {
            await PointAsync(scene, Box(page, "AllowRememberMe"));
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await PointAsync(scene, Box(page, "AllowChangingPhoneNumber"));
        });
    }

    private static async Task RememberAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/userLogin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("remember-1", async () => await PointAsync(scene, Box(page, "AllowRememberMe")));

        await scene.StepAsync("remember-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await PointAsync(scene, Box(page, "UsePersistentAuthenticationCookie"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("label[for=\"ISite_LoginSettings_UsePersistentAuthenticationCookie\"]"));
            await FastGoAsync(scene, page.Locator("button.save"));
        });

        await scene.StepAsync("remember-3", async () =>
        {
            await scene.NavigateAsync("/Login");
            await PointAsync(scene, page.Locator("#LoginForm_RememberMe"));
        });

        await scene.StepAsync("remember-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Settings/userLogin");
            await FastClickAsync(scene, page.Locator("label[for=\"ISite_LoginSettings_AllowRememberMe\"]"), 150);
            await FastGoAsync(scene, page.Locator("button.save"));
            await scene.NavigateAsync("/Login");
        });

        await scene.StepAsync("remember-5", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/userLogin");
            await PointAsync(scene, Box(page, "UsePersistentAuthenticationCookie"));
        });
    }

    private static async Task ThemeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/userLogin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("theme-1", async () =>
        {
            await PointAsync(scene, Box(page, "UseSiteTheme"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("label[for=\"ISite_LoginSettings_UseSiteTheme\"]"));
            await FastGoAsync(scene, page.Locator("button.save"));
        });

        await scene.StepAsync("theme-2", async () =>
        {
            await scene.NavigateAsync("/Login");
            await PointAsync(scene, LogInButton(page));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await PointAsync(scene, page.Locator("footer").First);
        });
    }

    private static async Task LocalAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/userLogin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("local-1", async () => await PointAsync(scene, Box(page, "DisableLocalLogin")));

        await scene.StepAsync("local-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Box(page, "DisableLocalLogin"), "Leave this option off");
        });
    }

    private static async Task DetailsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Users/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        string editPath = null;

        await scene.StepAsync("details-1", async () =>
        {
            await FastGoAsync(scene, Row(page, "jamie.chen").Locator("a.btn:text-is(\"Edit\")"));
            editPath = new Uri(page.Url).PathAndQuery;
            foreach (var field in new[] { "#User_UserName", "#User_Email", "#User_PhoneNumber" })
            {
                await PointAsync(scene, page.Locator(field));
                await scene.PauseAsync(1200);
                await scene.ClearHighlightsAsync();
            }

            await scene.HighlightAsync(page.Locator("#User_PhoneNumber"));
        });

        await scene.StepAsync("details-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Settings/userLogin");
            await FastClickAsync(scene, page.Locator("label[for=\"ISite_LoginSettings_AllowChangingUsername\"]"), 150);
            await FastClickAsync(scene, page.Locator("label[for=\"ISite_LoginSettings_AllowChangingEmail\"]"), 150);
            await FastGoAsync(scene, page.Locator("button.save"));
        });

        await scene.StepAsync("details-3", async () =>
        {
            await scene.NavigateAsync(editPath);
            await PointAsync(scene, page.Locator("#User_UserName"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await PointAsync(scene, page.Locator("#User_Email"));
        });
    }

    private static async Task LockoutAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Login");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var wrong = UserPassword + "x";

        await scene.StepAsync("lockout-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#LoginForm_UserName"), "jamie.chen");
            await scene.TypeAsync(page.Locator("#LoginForm_Password"), wrong);
            await scene.ClickAndWaitForNavigationAsync(LogInButton(page));
        });

        await scene.StepAsync("lockout-2", async () =>
        {
            // The failed attempts that lead to the lock-out are not shown; the last one is.
            await scene.OffCameraAsync(async () =>
            {
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    if (await page.Locator("#LoginForm_UserName").CountAsync() == 0)
                    {
                        break;
                    }

                    if (await page.Locator("#LoginForm_UserName").InputValueAsync() == "")
                    {
                        await page.Locator("#LoginForm_UserName").FillAsync("jamie.chen");
                    }

                    await page.Locator("#LoginForm_Password").FillAsync(wrong);
                    await page.RunAndWaitForNavigationAsync(async () => await LogInButton(page).ClickAsync());
                }
            });

            for (var attempt = 0; attempt < 10 && await page.GetByText("The account is locked out").CountAsync() == 0; attempt++)
            {
                if (await page.Locator("#LoginForm_UserName").InputValueAsync() == "")
                {
                    await page.Locator("#LoginForm_UserName").FillAsync("jamie.chen");
                }

                await page.Locator("#LoginForm_Password").FillAsync(wrong);
                await scene.ClickAndWaitForNavigationAsync(LogInButton(page));
            }
        });

        await scene.StepAsync("lockout-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Users/Index");
            await PointAsync(scene, Row(page, "jamie.chen").Locator(".badge:has-text(\"Locked\")"));
        });

        await scene.StepAsync("lockout-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = Row(page, "jamie.chen");
            await FastClickAsync(scene, row.Locator("button.actions"));
            await FastClickAsync(scene, row.Locator(".actions-menu a.dropdown-item:text-is(\"Unlock\")"));
            await scene.PauseAsync(2600);
            await FastGoAsync(scene, page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("lockout-5", async () =>
        {
            await PointAsync(scene, page.GetByText("User unlocked successfully.").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await PointAsync(scene, Row(page, "jamie.chen"));
        });
    }
}

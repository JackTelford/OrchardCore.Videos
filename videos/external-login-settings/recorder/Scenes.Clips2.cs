using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static async Task OpenGroupAsync(Scene scene, string group, int pause = 250)
    {
        var button = scene.Page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\")), #left-nav button.item-label:has(span.title:text-is(\"{group}\"))").First;
        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await FastClickAsync(scene, button, pause);
        }
    }

    private static ILocator VisibilityButton(IPage page) => page.Locator("button[data-id=\"visibility-filter\"], button:has-text(\"Visibility\")").First;

    private static async Task ToggleHideOnDemandAsync(Scene scene)
    {
        var page = scene.Page;
        await FastClickAsync(scene, VisibilityButton(page), 300);
        await FastClickAsync(scene, page.Locator("a.dropdown-item:has-text(\"Hide on demand\"), .dropdown-menu.show :text-is(\"Hide on demand\")").First, 300);
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForTimeoutAsync(300);
    }

    // Signs the visitor in at the provider tenant (off camera) and opens the given page on the site.
    private static async Task VisitorAsync(Scene scene, string openPath)
    {
        var page = scene.Page;
        await scene.Context.ClearCookiesAsync();
        await scene.OpenAsync("/idp/Login");
        await page.Locator("#LoginForm_UserName").FillAsync("taylor.brooks");
        await page.Locator("#LoginForm_Password").FillAsync(Password);
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button[type=submit]").First.ClickAsync());
        await scene.OpenAsync(openPath);
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        var page = scene.Page;
        await page.Locator("#search-box").WaitForAsync();
        await OpenGroupAsync(scene, "Tools", 100);
        await page.WaitForTimeoutAsync(400);
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))").First);
            await FastTypeAsync(scene, page.Locator("#search-box"), "External Authentication", 60);
            await scene.HighlightAsync(VisibilityButton(page), "Hide on demand");
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ToggleHideOnDemandAsync(scene);
            var row = page.Locator(".list-group-item[data-filter-value*=\"OrchardCore.Users.ExternalAuthentication\"]").First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row.Locator("a.btn, button.btn, span[data-bs-toggle=\"tooltip\"]").First);
        });

        await scene.StepAsync("features-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.Locator("#search-box").FillAsync("");
            await FastTypeAsync(scene, page.Locator("#search-box"), "OpenID Connect Client", 40);
            var row = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_OpenId_Client") }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("#btn-enable-OrchardCore_OpenId_Client")));
            await page.GetByText("enabled").First.WaitForAsync();
        });

        await scene.StepAsync("features-4", async () =>
        {
            await ToggleHideOnDemandAsync(scene);
            await FastTypeAsync(scene, page.Locator("#search-box"), "External Authentication", 40);
            var row = page.Locator(".list-group-item[data-filter-value*=\"OrchardCore.Users.ExternalAuthentication\"]").First;
            await row.WaitForAsync();
            var disable = row.Locator("span[data-bs-toggle=\"tooltip\"], .btn-danger, a.btn").Last;
            await scene.MoveToAsync(disable);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task LoginAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await VisitorAsync(scene, "/Login");
        var page = scene.Page;
        var provider = page.Locator("button[name=\"provider\"][value=\"OpenIdConnect\"]").First;
        await provider.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("login-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("h1:has-text(\"Use another service to log in\")").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(provider, "Example Accounts");
        });

        await scene.StepAsync("login-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(provider));
            await page.GetByText("New registrations are disabled for this site.").First.WaitForAsync();
        });

        await scene.StepAsync("login-3", async () =>
            await scene.HighlightAsync(page.GetByText("New registrations are disabled for this site.").First));
    }

    private static async Task RegistrationAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/userRegistration");
        var page = scene.Page;
        await page.GetByLabel("Disable new external registration").WaitForAsync();
        await OpenGroupAsync(scene, "Settings", 100);
        await OpenGroupAsync(scene, "Security", 100);
        await page.WaitForTimeoutAsync(400);
        await scene.StartCaptureAsync();

        await scene.StepAsync("reg-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("#left-nav a:has(span.title:text-is(\"Registration\"))").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByRole(AriaRole.Heading, new() { Name = "Configuring External Authentication for User Registration with Third-Party Providers" }).First);
        });

        await scene.StepAsync("reg-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Disable new external registration").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("reg-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.GetByLabel("Disable new external registration").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 300);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First));
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        });

        await scene.StepAsync("reg-4", async () =>
        {
            foreach (var label in new[] { "Do not ask username", "Do not ask email address", "Do not create local password" })
            {
                await scene.HighlightAsync(page.GetByLabel(label).Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
            }
        });

        await scene.StepAsync("reg-5", async () =>
        {
            await scene.MoveToAsync(page.GetByText("Do not ask email address").First);
            await scene.PauseAsync(2500);
            await scene.MoveToAsync(page.GetByText("Do not create local password").First);
        });

        await scene.StepAsync("reg-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Use a script to generate userName based on external provider claims").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });
    }

    private static async Task RegisterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await VisitorAsync(scene, "/Login");
        var page = scene.Page;
        await page.Locator("button[name=\"provider\"][value=\"OpenIdConnect\"]").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("new-1", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("button[name=\"provider\"][value=\"OpenIdConnect\"]").First));
            await page.Locator("h4:has-text(\"Create a new account.\")").First.WaitForAsync();
        });

        await scene.StepAsync("new-2", async () =>
        {
            foreach (var label in new[] { "Username", "Email", "Password", "Confirm password" })
            {
                var field = page.GetByLabel(label, new() { Exact = true }).First;
                if (await field.CountAsync() > 0)
                {
                    await scene.HighlightAsync(field);
                    await scene.PauseAsync(900);
                    await scene.ClearHighlightsAsync();
                }
            }
        });

        await scene.StepAsync("new-3", async () =>
        {
            var email = page.GetByLabel("Email", new() { Exact = true }).First;
            if (string.IsNullOrEmpty(await email.InputValueAsync()))
            {
                await FastTypeAsync(scene, email, "taylor.brooks@example.com", 8);
            }

            await FastTypeAsync(scene, page.GetByLabel("Password", new() { Exact = true }).First, Password, 5, fill: true);
            await FastTypeAsync(scene, page.GetByLabel("Confirm password", new() { Exact = true }).First, Password, 5, fill: true);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("button:has-text(\"Register\")").First));
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/OrchardCore.Users/ExternalAuthentications/ExternalLogins"));
            await page.GetByText("Registered Logins").First.WaitForAsync();
        });
    }

    private static async Task LoginSettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/userLogin");
        var page = scene.Page;
        await OpenGroupAsync(scene, "Settings", 100);
        await OpenGroupAsync(scene, "Security", 100);
        await page.WaitForTimeoutAsync(400);
        await scene.StartCaptureAsync();

        await scene.StepAsync("loginset-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("#left-nav a:has(span.title:text-is(\"User Login\"))").First);
            await scene.PauseAsync(1500);
            await FastClickAsync(scene, page.Locator("a.nav-link:has-text(\"External Login\"), button.nav-link:has-text(\"External Login\")").First, 300);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Use external provider for login.").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("loginset-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Use a script to set user roles, user claims and user properties based on external provider claims").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("loginset-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.GetByLabel("Use external provider for login.").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 300);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First);
        });
    }

    private static async Task StraightAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await VisitorAsync(scene, "/");
        var page = scene.Page;
        await scene.StartCaptureAsync();

        await scene.StepAsync("straight-1", async () =>
        {
            await scene.NavigateAsync("/Login");
            await page.Locator(".post-preview").First.WaitForAsync();
            await scene.PauseAsync(1500);
            await scene.NavigateAsync("/OrchardCore.Users/ExternalAuthentications/ExternalLogins");
            await page.GetByText("Registered Logins").First.WaitForAsync();
        });
    }
}

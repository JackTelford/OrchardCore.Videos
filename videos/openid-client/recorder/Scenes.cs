using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "OpenID Connect Client". The site has two tenants: Default (the client) and idp (the provider, jamie.chen).
/// PrepareAsync seeds the provider (Authorization Server, an application). Record in one take, in order: 02-enable, 03-connect,
/// 04-registration, 05-signin, 06-account.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // A client secret made up for the recording; it is created on the provider and typed in the client settings.
    private const string ClientSecret = "example-client-secret-0123456789";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-connect", ConnectAsync),
        ("04-registration", RegistrationAsync),
        ("05-signin", SigninAsync),
        ("06-account", AccountAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);

        await page.GotoAsync(BaseUrl + "/idp/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[name='LoginForm.UserName']").FillAsync("jamie.chen");
        await page.Locator("input[name='LoginForm.Password']").FillAsync(Password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await page.WaitForURLAsync(url => !url.Contains("/Login"));

        // 1. Enable the Authorization Server.
        await page.GotoAsync(BaseUrl + "/idp/Admin/Features", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#search-box").FillAsync("OpenID Connect Authorization Server");

        if (await page.Locator("#btn-enable-OrchardCore_OpenId_Server").CountAsync() > 0)
        {
            await page.Locator("#btn-enable-OrchardCore_OpenId_Server").ClickAsync();
            await page.GetByText("was enabled.").First.WaitForAsync();
        }

        Console.WriteLine("Enabled the Authorization Server.");

        // 2. Its settings.
        await page.GotoAsync(BaseUrl + "/idp/Admin/OpenId/ServerConfiguration", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        foreach (var id in new[] { "EnableAuthorizationEndpoint", "EnableTokenEndpoint", "EnableUserInfoEndpoint" })
        {
            await page.Locator($"#OpenIdServerSettings_{id}").CheckAsync();
        }

        await page.Locator("#OpenIdServerSettings_AllowAuthorizationCodeFlow").CheckAsync();
        await page.Locator("button.save, button:has-text('Save')").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(3000);
        await page.GotoAsync(BaseUrl + "/idp/Admin/OpenId/ServerConfiguration", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Saved the Authorization Server settings: token endpoint " + await page.Locator("#OpenIdServerSettings_EnableTokenEndpoint").IsCheckedAsync()
            + ", code flow " + await page.Locator("#OpenIdServerSettings_AllowAuthorizationCodeFlow").IsCheckedAsync());

        // 3. The application.
        await page.GotoAsync(BaseUrl + "/idp/Admin/OpenId/Application/Create", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#DisplayName").FillAsync("Example Site");
        await page.Locator("#Type").SelectOptionAsync(new SelectOptionValue { Label = "Confidential client" });
        await page.Locator("#ClientId").FillAsync("example-site");
        await page.Locator("#ClientSecret").FillAsync(ClientSecret);
        await page.Locator("#AllowAuthorizationCodeFlow").CheckAsync();
        await page.Locator("#RedirectUris").FillAsync("http://localhost:5012/signin-oidc");
        await page.Locator("#ConsentType").SelectOptionAsync(new SelectOptionValue { Label = "Implicit consent" });
        await page.Locator("#ScopeEntries_0__Selected").CheckAsync();
        await page.Locator("#ScopeEntries_1__Selected").CheckAsync();
        await page.Locator("button.save, button:has-text('Save')").First.ClickAsync();
        await page.WaitForURLAsync(url => !url.Contains("/Application/Create"));
        Console.WriteLine("Created the application example-site.");
        await context.CloseAsync();
    }

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static ILocator Group(Scene scene, string name)
        => scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static async Task OpenGroupAsync(Scene scene, string name)
    {
        var group = Group(scene, name);

        if (await group.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(group, pauseAfter: 300);
        }
    }

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    // Clicks something that loads another page, with the wait cut out of the clip.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);
    }

    private static ILocator Field(Scene scene, string label) => scene.Page.Locator($".ocat-wrapper:has(> label:text-is(\"{label}\")) input").First;

    private static ILocator Check(Scene scene, string label) => scene.Page.Locator($".form-check:has(label:text-is(\"{label}\")) input").First;

    private static async Task OpenSettingsPageAsync(Scene scene, string[] groups, string href)
    {
        foreach (var group in groups)
        {
            await OpenGroupAsync(scene, group);
        }

        await NavAsync(scene, scene.Page.Locator($"#left-nav a[href$='{href}']").First);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        var row = scene.Page.Locator(".list-group-item:has(#btn-enable-OrchardCore_OpenId_Client), .list-group-item:has(#btn-disable-OrchardCore_OpenId_Client)").First;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "OpenID Connect", delay: 40);
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("#btn-enable-OrchardCore_OpenId_Client").First);
            await scene.Page.GetByText("The feature OpenID Connect Client Integration was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            await scene.MoveToAsync(scene.Page.GetByText("The feature OpenID Connect Client Integration was enabled.").First);
            await scene.PauseAsync(2500);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "OpenID Connect", delay: 6);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ConnectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("connect-1", async () =>
        {
            await OpenSettingsPageAsync(scene, ["Settings", "OpenID Connect"], "/Admin/Settings/OrchardCore.OpenId.Client");
            await scene.TypeAsync(Field(scene, "Display Name"), "Example Identity", delay: 12);
        });

        await scene.StepAsync("connect-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "Authority"), "http://localhost:5012/idp", delay: 6);
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.TypeAsync(Field(scene, "ClientId"), "example-site", delay: 10);
            await scene.Page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("connect-3", async () =>
        {
            await scene.TypeAsync(Field(scene, "CallbackPath"), "/signin-oidc", delay: 12);
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(Field(scene, "CallbackPath"));
        });

        await scene.StepAsync("connect-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await Field(scene, "Scopes").ScrollIntoViewIfNeededAsync();
            await scene.TypeAsync(Field(scene, "Scopes"), "email", delay: 40);
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(Field(scene, "Scopes"));
        });

        await scene.StepAsync("connect-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var signedOutCallback = Field(scene, "SignedOut CallbackPath");
            await signedOutCallback.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(signedOutCallback);
            await scene.PauseAsync(800);
            await scene.MoveToAsync(Field(scene, "SignedOut Redirect Uri"));
            await scene.PauseAsync(800);
            var store = Check(scene, "Store tokens?");
            await store.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(store);
            await scene.HighlightAsync(scene.Page.Locator(".form-check:has(label:text-is('Store tokens?'))").First);
        });

        await scene.StepAsync("connect-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.GetByText("Code Authentication Flow").First.ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(scene.Page.Locator("input.openid-use-code-id-token-flow").First, pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator("input.openid-use-code-flow").First, pauseAfter: 500);
        });

        await scene.StepAsync("connect-7", async () =>
        {
            await scene.Page.Locator(".openid-client-secret.show").First.WaitForAsync();
            await scene.TypeAsync(scene.Page.Locator(".ocat-wrapper:has(> label:text-is('Client Secret')) input[type=password]").First, ClientSecret, delay: 15);
            await scene.MoveToAsync(scene.Page.Locator("select").Filter(new() { Has = scene.Page.Locator("option[value='form_post']") }).First);
        });

        await scene.StepAsync("connect-8", async () =>
        {
            var save = scene.Page.Locator("button.save").First;
            await save.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText("Site settings updated successfully.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("Site settings updated successfully.").First);
            await scene.PauseAsync(2500);
            await scene.Page.Locator(".ocat-wrapper:has(> label:text-is('Client Secret')) input[type=password]").First.ScrollIntoViewIfNeededAsync();
        });
    }

    private static async Task RegistrationAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("reg-1", async () =>
        {
            await OpenSettingsPageAsync(scene, ["Settings", "Security"], "/Admin/Settings/userRegistration");
            await scene.Page.GetByText("Configuring External Authentication").First.WaitForAsync();
        });

        await scene.StepAsync("reg-2", async () =>
        {
            foreach (var label in new[] { "Disable new external registration", "Do not ask username", "Do not ask email address", "Do not create local password" })
            {
                var row = scene.Page.Locator($".form-check:has(label:text-is('{label}'))").First;
                await scene.MoveToAsync(row);
                await scene.HighlightAsync(row);
                await scene.HoldAsync(1.6);

                if (label != "Do not create local password")
                {
                    await scene.ClearHighlightsAsync();
                }
            }
        });

        await scene.StepAsync("reg-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Check(scene, "Disable new external registration"), pauseAfter: 300);
            await scene.ClickAsync(Check(scene, "Do not ask email address"), pauseAfter: 400);
            await scene.ClickAsync(Check(scene, "Do not create local password"), pauseAfter: 400);
            var save = scene.Page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText("Site settings updated successfully.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("Site settings updated successfully.").First);
        });
    }

    private static async Task SigninAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.Context.ClearCookiesAsync();
        await scene.OpenAsync("/Login");
        var provider = scene.Page.Locator("button[name='provider'][value='OpenIdConnect']").First;
        await provider.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("signin-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Use another service to log in").First);
            await scene.HoldAsync(2);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(provider);
        });

        await scene.StepAsync("signin-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(provider);
            await scene.OffCameraAsync(async () =>
            {
                await provider.ClickAsync();
                await scene.Page.WaitForURLAsync(url => url.Contains("/idp/Login"), new() { Timeout = 60000 });
                await scene.Page.Locator("input[name='LoginForm.UserName']").WaitForAsync();
            }, leadMilliseconds: 150);
        });

        await scene.StepAsync("signin-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[name='LoginForm.UserName']").First, "jamie.chen", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("input[name='LoginForm.Password']").First, Password, delay: 30);
            var login = scene.Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).First;
            await scene.MoveToAsync(login);
            await scene.OffCameraAsync(async () =>
            {
                await login.ClickAsync();
                await scene.Page.WaitForURLAsync(url => url.Contains("ExternalLoginCallback"), new() { Timeout = 60000 });
                await scene.Page.GetByText("Create a new account.").First.WaitForAsync();
            }, leadMilliseconds: 150);
        });

        await scene.StepAsync("signin-4", async () =>
        {
            var username = scene.Page.Locator("input[name$='UserName'], input[name$='Username']").First;
            await scene.ClickAsync(username, pauseAfter: 200);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await scene.Page.Keyboard.PressAsync("Delete");
            await scene.Page.Keyboard.TypeAsync("jamie.chen", new() { Delay = 40 });
            await scene.HighlightAsync(username);
        });

        await scene.StepAsync("signin-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var register = scene.Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).First;
            await scene.MoveToAsync(register);
            await scene.OffCameraAsync(async () =>
            {
                await register.ClickAsync();
                await scene.Page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/", new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
        });
    }

    private static async Task AccountAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("account-1", async () =>
        {
            await OpenGroupAsync(scene, "Access Control");
            await NavAsync(scene, scene.Page.Locator("#left-nav a[href*='/Admin/Users']").First);
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item:has-text('jamie.chen')").First);
        });

        await scene.StepAsync("account-2", async () =>
        {
            await scene.MoveToAsync(scene.Page.Locator(".list-group-item:has-text('jamie.chen')").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

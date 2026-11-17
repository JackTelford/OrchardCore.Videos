using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-login", LoginAsync),
        ("04-registration", RegistrationAsync),
        ("05-register", RegisterAsync),
        ("06-settings", LoginSettingsAsync),
        ("06-straight", StraightAsync),
    ];

    private static readonly string IdpRecipe = "{\"steps\":[{\"name\":\"feature\",\"enable\":[\"OrchardCore.OpenId\",\"OrchardCore.OpenId.Management\",\"OrchardCore.OpenId.Server\"]},"
        + "{\"name\":\"OpenIdServerSettings\",\"Authority\":\"" + "BASE/idp" + "\",\"EnableAuthorizationEndpoint\":true,\"EnableTokenEndpoint\":true,\"EnableUserInfoEndpoint\":true,\"EnableLogoutEndpoint\":true,\"AllowAuthorizationCodeFlow\":true,\"AllowRefreshTokenFlow\":true},"
        + "{\"name\":\"OpenIdApplication\",\"ClientId\":\"example-site\",\"DisplayName\":\"Example site\",\"Type\":\"public\",\"ConsentType\":\"implicit\",\"RedirectUris\":\"BASE/signin-oidc\",\"PostLogoutRedirectUris\":\"BASE/signout-callback-oidc\",\"AllowAuthorizationCodeFlow\":true,\"AllowRefreshTokenFlow\":true,\"AllowLogoutEndpoint\":true,\"ScopeEntries\":[{\"Name\":\"email\"},{\"Name\":\"profile\"}]}]}";

    private static readonly string ClientRecipe = "{\"steps\":[{\"name\":\"OpenIdClientSettings\",\"DisplayName\":\"Example Accounts\",\"Authority\":\"BASE/idp\",\"ClientId\":\"example-site\",\"CallbackPath\":\"/signin-oidc\",\"SignedOutCallbackPath\":\"/signout-callback-oidc\",\"Scopes\":\"openid profile email\",\"ResponseType\":\"code\",\"ResponseMode\":\"form_post\"}]}";

    private static async Task<IPage> IdpAdminAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/idp/Login", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#LoginForm_UserName").FillAsync("idpadmin");
        await page.Locator("#LoginForm_Password").FillAsync(Password);
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button[type=submit]").First.ClickAsync());
        return page;
    }

    // OCV_SEED=A: the idp tenant (server, application, user taylor.brooks). OCV_SEED=B: the client settings on the Default tenant.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var seed = Environment.GetEnvironmentVariable("OCV_SEED") ?? "A";
        if (seed == "A")
        {
            var page = await IdpAdminAsync(browser);
            Console.WriteLine("idp admin url: " + page.Url);
            await page.GotoAsync(BaseUrl + "/idp/Admin/DeploymentPlan/Import/Json", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror'); cm.CodeMirror.setValue(json); cm.CodeMirror.save(); }", IdpRecipe.Replace("BASE", BaseUrl));
            await page.Locator("button.import").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Console.WriteLine("Import: " + (await page.Locator("body").InnerTextAsync()).Contains("imported successfully"));
            await Task.Delay(5000);
            await page.GotoAsync(BaseUrl + "/idp/Admin/Users/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("input[name='User.UserName']").FillAsync("taylor.brooks");
            await page.Locator("input[name='User.Email']").FillAsync("taylor.brooks@example.com");
            await page.Locator("input[name='User.Password']").FillAsync(Password);
            await page.Locator("input[name='User.PasswordConfirmation']").FillAsync(Password);
            await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.save").First.ClickAsync());
            Console.WriteLine("User created: " + page.Url);
            await page.Context.CloseAsync();
            return;
        }

        var (context, p) = await SeedPageAsync(browser);
        await ImportRecipeAsync(p, ClientRecipe.Replace("BASE", BaseUrl));
        await context.CloseAsync();
    }
}

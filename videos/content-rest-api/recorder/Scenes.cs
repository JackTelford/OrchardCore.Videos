using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Content REST API", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-role", RoleAsync),
        ("07-admin", AdminAsync),
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

    private const string FeaturesJson = """
        {"steps":[{"name":"feature","enable":["OrchardCore.OpenId","OrchardCore.OpenId.Server","OrchardCore.OpenId.Validation","OrchardCore.OpenId.Management"],"disable":[]}]}
        """;

    private const string ServerJson = """
        {"steps":[
          {"name":"OpenIdServerSettings","AccessTokenFormat":0,"TokenEndpointPath":"/connect/token","AuthorizationEndpointPath":"/connect/authorize","LogoutEndpointPath":"/connect/logout","UserinfoEndpointPath":"/connect/userinfo","AllowClientCredentialsFlow":true,"AllowAuthorizationCodeFlow":true,"AllowRefreshTokenFlow":true,"AllowPasswordFlow":false,"AllowImplicitFlow":false,"RequireProofKeyForCodeExchange":false,"UseReferenceAccessTokens":false,"DisableAccessTokenEncryption":false},
          {"name":"OpenIdValidationSettings","Authority":null,"Audience":"tenant"}
        ]}
        """;

    private const string AppJson = """
        {"steps":[
          {"name":"OpenIdApplication","ClientId":"content-api-client","DisplayName":"Content API client","Type":"confidential","ClientSecret":"Secret-for-the-demo-only-1","AllowClientCredentialsFlow":true,"RoleEntries":[{"Name":"ApiClient","Selected":true}],"ScopeEntries":[],"AllowAuthorizationCodeFlow":false,"AllowRefreshTokenFlow":false,"AllowPasswordFlow":false,"AllowImplicitFlow":false,"AllowLogoutEndpoint":false,"AllowIntrospectionEndpoint":false,"AllowRevocationEndpoint":false}
        ]}
        """;

    private static async Task ImportJsonAsync(IPage page, string json)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!");
    }

    private static async Task WaitSiteAsync()
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        for (var i = 0; i < 60; i++)
        {
            try
            {
                var response = await client.GetAsync(BaseUrl + "/");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(2000);
        }
    }

    /// <summary>The OpenID server, the ApiClient role and application, then the seed calls of the plan (section 2). Outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await ImportJsonAsync(page, FeaturesJson);
        await WaitSiteAsync();
        await Task.Delay(3000);

        await page.GotoAsync(BaseUrl + "/Admin/Roles/Index");
        await page.Locator("a, button").Filter(new() { HasTextString = "Add Role" }).First.ClickAsync();
        await page.GetByLabel("Name").First.FillAsync("ApiClient");
        await page.GetByLabel("Description").First.FillAsync("Calls the Content API");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save & Configure" }).ClickAsync();
        await page.Locator("#search-box").WaitForAsync();
        foreach (var permission in new[] { "Access content via the api", "Publish or unpublish content for others", "Delete content for others" })
        {
            await page.Locator("#search-box").FillAsync(permission);
            var row = page.Locator($"tr[data-text='{permission}']");
            await row.WaitForAsync();
            await row.Locator("input[type='checkbox']").First.CheckAsync();
        }

        await page.Locator("button.save").First.ClickAsync();
        await WaitTextAsync(page, "Role updated successfully.");

        await page.GotoAsync(BaseUrl + "/Admin/OpenId/ServerConfiguration");
        await page.Locator("input[type='checkbox'][name='OpenIdServerSettings.EnableTokenEndpoint']").CheckAsync(new() { Force = true });
        await page.Locator("input[type='checkbox'][name='OpenIdServerSettings.AllowClientCredentialsFlow']").CheckAsync(new() { Force = true });
        await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First.ClickAsync();
        await WaitTextAsync(page, "successfully updated");
        await WaitSiteAsync();
        await Task.Delay(3000);
        await page.GotoAsync(BaseUrl + "/Admin/OpenId/ValidationConfiguration");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First.ClickAsync();
        await WaitTextAsync(page, "successfully updated");
        await WaitSiteAsync();
        await Task.Delay(3000);
        await ImportJsonAsync(page, AppJson);
        await WaitSiteAsync();

        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        var tokenResponse = await client.PostAsync(BaseUrl + "/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "content-api-client",
            ["client_secret"] = "Secret-for-the-demo-only-1",
        }));
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        Console.WriteLine("Token request: " + (int)tokenResponse.StatusCode);
        var token = System.Text.Json.JsonDocument.Parse(tokenJson).RootElement.GetProperty("access_token").GetString();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        async Task<string> SendAsync(HttpMethod method, string path, string body = null)
        {
            var request = new HttpRequestMessage(method, BaseUrl + path);
            if (body != null)
            {
                request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{method} {path} -> {(int)response.StatusCode}");
            return text;
        }

        string IdOf(string json) => System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("ContentItemId").GetString();

        var a = IdOf(await SendAsync(HttpMethod.Post, "/api/content", """{"ContentType":"Article","TitlePart":{"Title":"Planting notes"},"HtmlBodyPart":{"Html":"<p>Seedlings go out in May.</p>"}}"""));
        await SendAsync(HttpMethod.Post, "/api/content?draft=true", """{"ContentType":"Article","TitlePart":{"Title":"Greenhouse log"}}""");
        await SendAsync(HttpMethod.Get, "/api/content/" + a);
        await SendAsync(HttpMethod.Post, "/api/content", "{\"ContentItemId\":\"" + a + "\",\"TitlePart\":{\"Title\":\"Spring planting notes\"}}");
        await SendAsync(HttpMethod.Post, "/api/content?draft=true", "{\"ContentItemId\":\"" + a + "\",\"Article\":{\"Subtitle\":{\"Text\":\"Revised in the greenhouse\"}}}");
        var c = IdOf(await SendAsync(HttpMethod.Post, "/api/content", """{"ContentType":"Article","TitlePart":{"Title":"Old notes"}}"""));
        await SendAsync(HttpMethod.Delete, "/api/content/" + c);

        foreach (var path in new[] { "/Admin/Roles/Index", "/Admin/Contents/ContentItems" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Prepared.");
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

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<IPage, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene.Page);
        }

        await scene.StartCaptureAsync();
        return scene;
    }

    private static ILocator Perm(IPage page, string name) => page.Locator($"tr[data-text='{name}']").First;

    private static async Task RoleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Roles/Index", p => p.Locator("a[href$='/Roles/Edit/ApiClient']").First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("perm-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[href$='/Roles/Edit/ApiClient']").First);
            await page.Locator("#search-box").WaitForAsync();
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("via the api", new() { Delay = 50 });
            await Perm(page, "Access content via the api").WaitForAsync();
            await page.Locator("tr[data-text='Clone content']").First.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await scene.HighlightAsync(Perm(page, "Access content via the api"), "Access content via the api");
        });

        await scene.StepAsync("perm-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.PressAsync("Escape");
            await page.Keyboard.TypeAsync("content for others", new() { Delay = 50 });
            await Perm(page, "Publish or unpublish content for others").WaitForAsync();
            await Perm(page, "Edit content for others").WaitForAsync();
            await Perm(page, "Delete content for others").WaitForAsync();
            await scene.HighlightAsync(Perm(page, "Publish or unpublish content for others"), "Allow");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Perm(page, "Edit content for others"), "Effective");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Perm(page, "Delete content for others"), "Allow");
        });
    }

    private static async Task AdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentItems", p => p.GetByRole(AriaRole.Link, new() { Name = "Greenhouse log", Exact = true }).First.WaitForAsync());
        var page = scene.Page;
        ILocator RowOf(string title) => page.Locator("li.list-group-item, tr").Filter(new() { Has = page.GetByRole(AriaRole.Link, new() { Name = title, Exact = true }) }).First;

        await scene.StepAsync("admin-1", async () =>
        {
            await scene.HighlightAsync(RowOf("Greenhouse log").Locator(".badge", new() { HasTextString = "Draft" }).First, "Draft");
        });

        await scene.StepAsync("admin-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf("Spring planting notes").Locator(".badge", new() { HasTextString = "Published" }).First, "Published");
            await scene.HighlightAsync(RowOf("Spring planting notes").Locator(".badge", new() { HasTextString = "Draft" }).First, "Draft");
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

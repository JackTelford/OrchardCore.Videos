using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string ServerPage = "/Admin/OpenId/ServerConfiguration";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-endpoints", EndpointsAsync),
        ("04-flows", FlowsAsync),
        ("05-global", GlobalAsync),
        ("06-advanced", AdvancedAsync),
        ("07-save", SaveAsync),
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

    // Warms the admin pages once, outside the capture, so that the first visit is not slow on camera. It saves nothing.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        foreach (var path in new[] { "/Admin", "/Admin/Features" })
        {
            await page.GotoAsync(BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 120000 });
            await Task.Delay(1500);
        }

        await context.CloseAsync();
    }

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.SetDefaultTimeout(120000);
        return scene;
    }

    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    private static ILocator Group(IPage page, string title) => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator Items(IPage page, string title) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{title}\"]");

    private static ILocator Field(IPage page, string name) => page.Locator($"[data-openid-field=\"{name}\"]");

    // The row of a checkbox: the checkbox, its label and its hint.
    private static ILocator RowOf(ILocator input) => input.Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper')][1]");

    private static ILocator Check(IPage page, string label) => page.GetByLabel(label, new() { Exact = true });

    // A short pointer move and a click, for steps that must fit a short narration on a loaded machine.
    private static async Task QuickClickAsync(Scene scene, ILocator locator)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await scene.PauseAsync(300);
    }

    private static double D(IReadOnlyDictionary<string, double> durations, string id) => durations.TryGetValue(id, out var seconds) ? seconds : 4;

    // Opens the server page outside the capture and waits for the flows to be expanded.
    private static async Task OpenServerPageAsync(Scene scene)
    {
        await OpenCleanAsync(scene, ServerPage);
        await Field(scene.Page, "AllowClientCredentialsFlow").WaitForAsync();
        await RowOf(Field(scene.Page, "AllowClientCredentialsFlow")).WaitForAsync();
        await Task.Delay(800);
        await scene.Page.EvaluateAsync("() => window.scrollTo(0, 0)");
    }

    private static async Task ScrollToAsync(Scene scene, ILocator target, double top = 160, bool smooth = false)
    {
        await target.WaitForAsync();
        var delta = await target.EvaluateAsync<double>("(e, top) => { const y = Math.max(0, Math.min(e.getBoundingClientRect().top + window.scrollY - top, document.documentElement.scrollHeight - window.innerHeight)); return y - window.scrollY; }", top);
        if (smooth && Math.Abs(delta) > 4)
        {
            await scene.ScrollAsync(0, delta, 8);
        }
        else
        {
            await scene.Page.EvaluateAsync("(d) => window.scrollBy(0, d)", delta);
            await scene.PauseAsync(300);
        }
    }

    private static async Task ScrollToEndAsync(Scene scene)
    {
        await scene.Page.EvaluateAsync("() => window.scrollTo(0, document.body.scrollHeight)");
        await scene.PauseAsync(300);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("#left-nav button.nav-group-label").First.WaitForAsync();
        var tools = Group(page, "Tools");
        if (await Items(page, "Tools").Locator("a.item-label:visible").CountAsync() == 0)
        {
            await tools.ClickAsync();
            await Task.Delay(500);
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            var link = Items(page, "Tools").Locator("a.item-label:has-text(\"Features\")").First;
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), 300);
            var box = page.Locator("#search-box");
            await box.WaitForAsync();
            await scene.PauseAsync(600);
            await scene.TypeAsync(box, "OpenID", 3);
            await scene.PauseAsync(500);
            if (await box.InputValueAsync() != "OpenID")
            {
                await box.FillAsync("OpenID");
                await scene.PauseAsync(500);
            }
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var enable = page.Locator("#btn-enable-OrchardCore_OpenId_Server");
            await enable.WaitForAsync();
            await scene.MoveToAsync(enable);
            await scene.HighlightAsync(page.Locator("[data-filter-value*=\"OrchardCore.OpenId.Server\"]:visible").First);
            await scene.HoldAsync(Math.Max(0.5, D(durations, "enable-2") - 4.2));
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(enable);
                await page.Locator("text=The feature OpenID Connect Authorization Server was enabled.").First.WaitForAsync(new() { Timeout = 120000 });
                await page.Locator("#search-box").FillAsync("OpenID");
                await page.Locator("[data-filter-value*=\"OrchardCore.OpenId.Management\"]:visible").First.WaitForAsync();
            }, 300);
            await scene.HighlightAsync(page.Locator("[data-filter-value*=\"OrchardCore.OpenId.Management\"]:visible").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await QuickClickAsync(scene, Group(page, "Settings"));
            var settingsSub = Items(page, "Settings").Locator("button.item-label:has(span.title:text-is(\"OpenID Connect\"))").First;
            await QuickClickAsync(scene, settingsSub);
            var server = Items(page, "Settings").Locator("ul[data-title=\"OpenID Connect\"] a.item-label:has-text(\"Authorization Server\")").First;
            await server.WaitForAsync();
            await scene.HighlightAsync(server);
            await scene.HoldAsync(1.8);
            await scene.ClearHighlightsAsync();
            await QuickClickAsync(scene, Group(page, "Access Control"));
            var accessSub = Items(page, "Access Control").Locator("button.item-label:has(span.title:text-is(\"OpenID Connect\"))").First;
            await QuickClickAsync(scene, accessSub);
            var apps = Items(page, "Access Control").Locator("ul[data-title=\"OpenID Connect\"] a.item-label:has-text(\"Applications\")").First;
            var scopes = Items(page, "Access Control").Locator("ul[data-title=\"OpenID Connect\"] a.item-label:has-text(\"Scopes\")").First;
            await apps.WaitForAsync();
            await scene.HighlightAsync(apps);
            await scene.HoldAsync(0.6);
            await scene.HighlightAsync(scopes);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EndpointsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("#left-nav button.nav-group-label").First.WaitForAsync();
        await Group(page, "Settings").ClickAsync();
        await Items(page, "Settings").Locator("button.item-label:has(span.title:text-is(\"OpenID Connect\"))").First.ClickAsync();
        var server = Items(page, "Settings").Locator("ul[data-title=\"OpenID Connect\"] a.item-label:has-text(\"Authorization Server\")").First;
        await server.WaitForAsync();
        await Task.Delay(500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("endpoints-1", async () =>
        {
            await scene.MoveToAsync(server);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(server), 300);
            await Field(page, "AllowClientCredentialsFlow").WaitForAsync();
            await RowOf(Field(page, "AllowClientCredentialsFlow")).WaitForAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(page.Locator(".alert.alert-warning:visible").First);
        });

        await scene.StepAsync("endpoints-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, page.Locator("h3:has-text(\"Endpoints\")").First, 80, true);
            await scene.HighlightAsync(RowOf(Field(page, "EnableTokenEndpoint")));
            await scene.HoldAsync(1.0);
            await scene.HighlightAsync(RowOf(Field(page, "AllowAuthorizationCodeFlow")));
            await scene.HighlightAsync(RowOf(Field(page, "AllowRefreshTokenFlow")));
        });

        await scene.StepAsync("endpoints-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(Field(page, "EnableTokenEndpoint").Locator("xpath=following::code[1]"));
            await scene.HighlightAsync(RowOf(Field(page, "EnableTokenEndpoint")));
        });

        await scene.StepAsync("endpoints-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Field(page, "EnableAuthorizationEndpoint")));
            await scene.HoldAsync(4.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Check(page, "Enable Pushed Authorization Endpoint")));
        });

        await scene.StepAsync("endpoints-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Check(page, "Enable End Session Endpoint")));
            await scene.HoldAsync(3.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Check(page, "Require logout confirmation")));
        });

        await scene.StepAsync("endpoints-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Check(page, "Enable User Info Endpoint")));
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Check(page, "Enable Introspection Endpoint")));
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Check(page, "Enable Revocation Endpoint")));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FlowsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenServerPageAsync(scene);
        await ScrollToAsync(scene, RowOf(Field(page, "EnableTokenEndpoint")), 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("flows-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("h3:text-is(\"Flows\")").First);
        });

        await scene.StepAsync("flows-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Field(page, "EnableTokenEndpoint"));
            await RowOf(Field(page, "AllowPasswordFlow")).WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
            await scene.HighlightAsync(RowOf(Field(page, "AllowImplicitFlow")));
        });

        await scene.StepAsync("flows-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync(ServerPage);
                await Field(page, "AllowClientCredentialsFlow").WaitForAsync();
                await RowOf(Field(page, "AllowClientCredentialsFlow")).WaitForAsync();
                await scene.PauseAsync(600);
                await ScrollToAsync(scene, RowOf(Field(page, "EnableTokenEndpoint")), 120);
            }, 300);
            await scene.HighlightAsync(RowOf(Field(page, "AllowAuthorizationCodeFlow")));
        });

        await scene.StepAsync("flows-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Field(page, "AllowImplicitFlow")));
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Field(page, "AllowHybridFlow")));
        });

        await scene.StepAsync("flows-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Field(page, "AllowPasswordFlow")));
            await scene.HoldAsync(3.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Field(page, "AllowRefreshTokenFlow")));
        });

        await scene.StepAsync("flows-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Field(page, "AllowClientCredentialsFlow")));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task GlobalAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenServerPageAsync(scene);
        await ScrollToAsync(scene, page.Locator("h3:has-text(\"Proof Key\")").First, 100);
        await scene.StartCaptureAsync();

        await scene.StepAsync("global-1", async () =>
        {
            await scene.HighlightAsync(RowOf(Check(page, "Require Proof Key for Code Exchange")));
        });

        await scene.StepAsync("global-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Check(page, "Require Pushed Authorization Requests"));
            await ScrollToAsync(scene, Check(page, "Enable Pushed Authorization Endpoint"), 300, true);
            await scene.ClickAsync(Check(page, "Enable Pushed Authorization Endpoint"));
            var save = page.Locator(".action-bar-bottom button.btn-primary:visible").First;
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(save), 300);
        });

        await scene.StepAsync("global-3", async () =>
        {
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.PauseAsync(300);
            await scene.HighlightAsync(page.Locator(".validation-summary-errors").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AdvancedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenServerPageAsync(scene);
        await ScrollToEndAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("advanced-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("h3:has-text(\"Advanced Options\")").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Check(page, "Authority")));
        });

        await scene.StepAsync("advanced-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(page, "EncryptionCertificateStoreLocation"));
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(page, "SigningCertificateStoreLocation"));
        });

        await scene.StepAsync("advanced-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Field(page, "EncryptionCertificateStoreLocation"), "CurrentUser");
            var name = Field(page, "EncryptionCertificateStoreName");
            await name.WaitForAsync();
            await scene.HighlightAsync(name);
        });

        await scene.StepAsync("advanced-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Check(page, "Disable Rolling Refresh Tokens")));
        });

        await scene.StepAsync("advanced-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(page, "AccessTokenFormat"));
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Field(page, "AccessTokenFormat"), "JsonWebToken");
        });

        await scene.StepAsync("advanced-6", async () =>
        {
            await scene.HighlightAsync(Field(page, "DisableAccessTokenEncryption"));
            await scene.HoldAsync(4.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(RowOf(Check(page, "Use Reference Access Tokens")));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SaveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenServerPageAsync(scene);
        await ScrollToAsync(scene, RowOf(Field(page, "AllowClientCredentialsFlow")), 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("save-1", async () =>
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            await scene.ClickAsync(Field(page, "AllowClientCredentialsFlow"));
            await ScrollToAsync(scene, Check(page, "Require Proof Key for Code Exchange"), 300);
            await scene.ClickAsync(Check(page, "Require Proof Key for Code Exchange"));
            var wait = D(durations, "save-1") - 2.2 - clock.Elapsed.TotalSeconds;
            if (wait > 0)
            {
                await scene.HoldAsync(wait);
            }

            var save = page.Locator(".action-bar-bottom button.btn-primary:visible").First;
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(save);
                await page.Locator("text=OpenID server configuration successfully updated.").First.WaitForAsync(new() { Timeout = 120000 });
            }, 300);
        });

        await scene.StepAsync("save-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("text=OpenID server configuration successfully updated.").First);
            await scene.HoldAsync(1.8);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, RowOf(Field(page, "AllowClientCredentialsFlow")), 120);
            await scene.HighlightAsync(RowOf(Field(page, "AllowClientCredentialsFlow")));
        });

        await scene.ClearHighlightsAsync();
    }
}

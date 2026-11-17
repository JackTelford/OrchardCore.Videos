using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-service", ServiceAsync),
        ("03-web", WebAsync),
        ("04-manage", ManageAsync),
        ("05-scopes", ScopesAsync),
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

    // Seeds the site outside the capture: enables the server, saves its settings with the flows the video needs, and adds one application.
    // It is safe to run twice. Fake data only.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120000);
        await page.GotoAsync(BaseUrl + "/Admin/Features", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 120000 });
        await page.Locator("#search-box").FillAsync("OpenID");
        if (await page.Locator("#btn-enable-OrchardCore_OpenId_Server").CountAsync() > 0)
        {
            await page.Locator("#btn-enable-OrchardCore_OpenId_Server").ClickAsync();
            await page.Locator("text=The feature OpenID Connect Authorization Server was enabled.").First.WaitForAsync();
        }

        Console.WriteLine("feature enabled");
        await page.GotoAsync(BaseUrl + "/Admin/OpenId/ServerConfiguration", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 120000 });
        var flow = page.Locator("[data-openid-field=\"AllowClientCredentialsFlow\"]");
        await flow.WaitForAsync();
        await Task.Delay(800);
        await flow.CheckAsync();
        await page.Locator(".action-bar-bottom button.btn-primary:visible").First.ClickAsync();
        await page.Locator("text=OpenID server configuration successfully updated.").First.WaitForAsync();
        Console.WriteLine("server saved");
        await page.GotoAsync(BaseUrl + "/Admin/OpenId/Application", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 120000 });
        if (await page.Locator("text=Legacy Prototype").CountAsync() == 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/OpenId/Application/Create", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 120000 });
            await page.Locator("#DisplayName").WaitForAsync();
            await Task.Delay(800);
            await page.Locator("#DisplayName").FillAsync("Legacy Prototype");
            await page.Locator("#Type").SelectOptionAsync("Public client");
            await page.Locator("#ClientId").FillAsync("legacy-prototype");
            await page.Locator("#AllowAuthorizationCodeFlow").CheckAsync();
            await page.Locator("#RedirectUris").WaitForAsync();
            await page.Locator("#RedirectUris").FillAsync("https://legacy.example.com/signin-oidc");
            await page.Locator("button.save").ClickAsync();
            await page.Locator("text=Legacy Prototype").First.WaitForAsync();
            Console.WriteLine("legacy seeded");
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

    private static double D(IReadOnlyDictionary<string, double> durations, string id) => durations.TryGetValue(id, out var seconds) ? seconds : 4;

    private static ILocator Group(IPage page, string title) => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator Items(IPage page, string title) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{title}\"]");

    // A short pointer move and a click, for steps that must fit a short narration on a loaded machine.
    private static async Task QuickClickAsync(Scene scene, ILocator locator)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await scene.PauseAsync(300);
    }

    // Clicks a link whose page is slow to load: the pointer moves on camera, the load is cut out.
    private static async Task GoAsync(Scene scene, ILocator link)
    {
        await link.WaitForAsync();
        await scene.MoveToAsync(link);
        await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), 300);
    }

    // Opens Access Control > OpenID Connect > <leaf> in the admin menu, expanding what is collapsed, and loads the page off camera.
    private static async Task OpenMenuAsync(Scene scene, string leaf)
    {
        var page = scene.Page;
        var items = Items(page, "Access Control");
        if (await items.Locator("button.item-label:visible").CountAsync() == 0 && await items.Locator("a.item-label:visible").CountAsync() == 0)
        {
            await QuickClickAsync(scene, Group(page, "Access Control"));
        }

        var sub = items.Locator("button.item-label:has(span.title:text-is(\"OpenID Connect\"))").First;
        var link = items.Locator($"ul[data-title=\"OpenID Connect\"] a.item-label:has-text(\"{leaf}\")").First;
        if (!await link.IsVisibleAsync())
        {
            await QuickClickAsync(scene, sub);
        }

        await GoAsync(scene, link);
    }

    // Types text and checks that it arrived (typing right after a load can be lost).
    private static async Task TypeVerifiedAsync(Scene scene, ILocator field, string text)
    {
        await field.WaitForAsync();
        await scene.TypeAsync(field, text, 2);
        if (await field.InputValueAsync() != text)
        {
            await field.FillAsync(text);
            await scene.PauseAsync(300);
        }
    }

    private static async Task ScrollToAsync(Scene scene, ILocator target, double top = 160, bool smooth = true)
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
        var delta = await scene.Page.EvaluateAsync<double>("() => document.documentElement.scrollHeight - window.innerHeight - window.scrollY");
        if (delta > 4)
        {
            await scene.ScrollAsync(0, delta, 8);
        }
    }

    private static ILocator RowOf(IPage page, string name) => page.Locator($"li.list-group-item:has(a[href*=\"/Edit/\"]:text-is(\"{name}\"))");

    private static async Task SaveAsync(Scene scene)
    {
        var save = scene.Page.Locator("button.save").First;
        await scene.MoveToAsync(save);
        await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(save), 300);
    }

    // The Applications / Scopes list is shown with the rows it should have.
    private static async Task WaitRowsAsync(IPage page, params string[] names)
    {
        foreach (var name in names)
        {
            await page.Locator($"li.list-group-item:has(a:text-is(\"{name}\"))").First.WaitForAsync();
        }
    }

    private static async Task ServiceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("#left-nav button.nav-group-label").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("service-1", async () =>
        {
            await OpenMenuAsync(scene, "Applications");
            await WaitRowsAsync(page, "Legacy Prototype");
            await scene.MoveToAsync(page.Locator("a.create").First);
        });

        await scene.StepAsync("service-2", async () =>
        {
            await scene.ClickAsync(page.Locator("a.create").First, pauseAfter: 100);
            await page.Locator("#DisplayName").WaitForAsync();
            await scene.PauseAsync(600);
            await TypeVerifiedAsync(scene, page.Locator("#DisplayName"), "Inventory Sync");
            await scene.MoveToAsync(page.Locator("#Type"));
        });

        await scene.StepAsync("service-3", async () =>
        {
            await TypeVerifiedAsync(scene, page.Locator("#ClientId"), "inventory-sync");
            await scene.ClickAsync(page.Locator("#generateClientSecret"));
        });

        await scene.StepAsync("service-4", async () =>
        {
            await ScrollToAsync(scene, page.Locator("h3:has-text(\"Flows\"), h4:has-text(\"Flows\"), h5:has-text(\"Flows\")").First, 130);
            await scene.ClickAsync(page.Locator("#AllowClientCredentialsFlow"));
        });

        await scene.StepAsync("service-5", async () =>
        {
            await ScrollToEndAsync(scene);
            await scene.ClickAsync(page.Locator("#RoleGroup label:text-is(\"Editor\")").First);
        });

        await scene.StepAsync("service-6", async () =>
        {
            await scene.HighlightAsync(page.Locator("#ScopeGroup"));
            await scene.HoldAsync(2);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene);
            await WaitRowsAsync(page, "Inventory Sync", "Legacy Prototype");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WebAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/OpenId/Application");
        await WaitRowsAsync(page, "Inventory Sync", "Legacy Prototype");
        await scene.StartCaptureAsync();

        await scene.StepAsync("web-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First);
            await page.Locator("#DisplayName").WaitForAsync();
            await scene.PauseAsync(600);
            await TypeVerifiedAsync(scene, page.Locator("#DisplayName"), "Storefront Web");
            await scene.SelectAsync(page.Locator("#Type"), "Public client");
            await page.Locator("#ClientSecretWrapper").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await scene.PauseAsync(500);
            await ScrollToAsync(scene, page.Locator("#Type"), 110);
        });

        await scene.StepAsync("web-2", async () =>
        {
            await ScrollToAsync(scene, page.Locator("#ClientId"), 110);
            await TypeVerifiedAsync(scene, page.Locator("#ClientId"), "storefront-web");
            await scene.ClickAsync(page.Locator("#AllowAuthorizationCodeFlow"));
        });

        await scene.StepAsync("web-3", async () =>
        {
            await ScrollToAsync(scene, page.Locator("#RedirectUris"), 200);
            await TypeVerifiedAsync(scene, page.Locator("#RedirectUris"), "https://storefront.example.com/signin-oidc");
            await scene.ClickAsync(page.Locator("#AllowLogoutEndpoint"));
            await page.Locator("#PostLogoutRedirectUris").WaitForAsync();
            await TypeVerifiedAsync(scene, page.Locator("#PostLogoutRedirectUris"), "https://storefront.example.com/signout-callback-oidc");
        });

        await scene.StepAsync("web-4", async () =>
        {
            await ScrollToAsync(scene, page.Locator("#RequireProofKeyForCodeExchange"), 200);
            await scene.ClickAsync(page.Locator("#RequireProofKeyForCodeExchange"));
        });

        await scene.StepAsync("web-5", async () =>
        {
            await ScrollToAsync(scene, page.Locator("#ConsentType"), 200);
            await scene.MoveToAsync(page.Locator("#ConsentType"));
            await scene.HighlightAsync(page.Locator("#ConsentType"));
        });

        await scene.StepAsync("web-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, page.Locator("#ScopeGroup"), 250);
            await scene.ClickAsync(page.Locator("#ScopeGroup label:text-is(\"email\")").First);
            await scene.ClickAsync(page.Locator("#ScopeGroup label:text-is(\"profile\")").First);
            await scene.HoldAsync(1.5);
            await SaveAsync(scene);
            await WaitRowsAsync(page, "Inventory Sync", "Legacy Prototype", "Storefront Web");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/OpenId/Application");
        await WaitRowsAsync(page, "Inventory Sync", "Legacy Prototype", "Storefront Web");
        await scene.StartCaptureAsync();

        await scene.StepAsync("manage-1", async () =>
        {
            await TypeVerifiedAsync(scene, page.Locator("#search-text"), "Legacy");
            var search = page.Locator("button[type=submit][title=\"Search\"]").First;
            await scene.MoveToAsync(search);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(search), 300);
            await WaitRowsAsync(page, "Legacy Prototype");
            await scene.HoldAsync(2);
            await GoAsync(scene, page.Locator("a:text-is(\"Clear\")").First);
            await WaitRowsAsync(page, "Inventory Sync", "Legacy Prototype", "Storefront Web");
            await scene.MoveToAsync(page.Locator("#search-text"));
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await GoAsync(scene, RowOf(page, "Inventory Sync").Locator("a.btn:text-is(\"Edit\")"));
            await page.Locator("#ClientSecret").WaitForAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(page.Locator("#ClientSecret"));
        });

        await scene.StepAsync("manage-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.cancel").First);
            await WaitRowsAsync(page, "Inventory Sync", "Legacy Prototype", "Storefront Web");
            await scene.ClickAsync(RowOf(page, "Legacy Prototype").Locator("a.delete"));
            await page.Locator("#confirmRemoveModal").WaitForAsync();
            await scene.HoldAsync(1.5);
            var ok = page.Locator("#modalOkButton");
            await scene.MoveToAsync(ok);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(ok), 300);
            await WaitRowsAsync(page, "Inventory Sync", "Storefront Web");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ScopesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/OpenId/Application");
        await WaitRowsAsync(page, "Inventory Sync", "Storefront Web");
        await scene.StartCaptureAsync();

        await scene.StepAsync("scopes-1", async () =>
        {
            await OpenMenuAsync(scene, "Scopes");
            await WaitRowsAsync(page, "Email", "Phone", "Profile", "Roles");
            await scene.MoveToAsync(page.Locator("a.create").First);
        });

        await scene.StepAsync("scopes-2", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First);
            await page.Locator("#DisplayName").WaitForAsync();
            await scene.PauseAsync(600);
            await TypeVerifiedAsync(scene, page.Locator("#DisplayName"), "Inventory");
            await TypeVerifiedAsync(scene, page.Locator("#Name"), "inventory");
            await TypeVerifiedAsync(scene, page.Locator("#Description"), "Reads and updates the stock levels of the shop.");
        });

        await scene.StepAsync("scopes-3", async () =>
        {
            await TypeVerifiedAsync(scene, page.Locator("#Resources"), "inventory-api");
            await scene.HighlightAsync(page.Locator("span.hint").First);
        });

        await scene.StepAsync("scopes-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene);
            await WaitRowsAsync(page, "Email", "Inventory", "Phone", "Profile", "Roles");
            await scene.MoveToAsync(RowOf2(page, "Inventory"));
        });

        await scene.StepAsync("scopes-5", async () =>
        {
            await OpenMenuAsync(scene, "Applications");
            await WaitRowsAsync(page, "Inventory Sync", "Storefront Web");
            await GoAsync(scene, RowOf(page, "Inventory Sync").Locator("a.btn:text-is(\"Edit\")"));
            await page.Locator("#ScopeGroup").WaitForAsync();
            await ScrollToAsync(scene, page.Locator("#ScopeGroup"), 250);
            await scene.ClickAsync(page.Locator("#ScopeGroup label:text-is(\"inventory\")").First);
        });

        await scene.StepAsync("scopes-6", async () =>
        {
            await SaveAsync(scene);
            await WaitRowsAsync(page, "Inventory Sync", "Storefront Web");
        });

        await scene.ClearHighlightsAsync();
    }

    private static ILocator RowOf2(IPage page, string name) => page.Locator($"li.list-group-item:has(a:text-is(\"{name}\"))").First;
}

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
        ("03-features", FeaturesAsync),
        ("04-enable", EnableAsync),
        ("05-tour", TourAsync),
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

    // Warms the admin pages once, outside the capture, so that the first visit is not slow on camera.
    // It reads only and saves nothing (the OpenID Connect pages answer 404 until the features are enabled).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        foreach (var path in new[] { "/Admin", "/Admin/Features", "/Admin/OpenId/ServerConfiguration", "/Admin/OpenId/Application", "/Admin/OpenId/Scope", "/Admin/Features" })
        {
            await page.GotoAsync(BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 120000 });
            await Task.Delay(1500);
        }

        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    private static ILocator Group(IPage page, string title) => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator Row(IPage page, string id) => page.Locator($"[data-filter-value*=\"{id}\"]:visible").First;

    // Types in the search box, checks the value and retries when the page lost the text.
    private static async Task SearchAsync(Scene scene, string text)
    {
        var box = scene.Page.Locator("#search-box");
        await box.WaitForAsync();
        await scene.PauseAsync(600);
        await scene.TypeAsync(box, text, 3);
        await scene.PauseAsync(500);
        if (await box.InputValueAsync() != text)
        {
            await box.FillAsync(text);
            await scene.PauseAsync(500);
        }
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await page.Locator("#left-nav button.nav-group-label").First.WaitForAsync();
        await page.EvaluateAsync("() => document.querySelector('#left-nav button.nav-group-label.oc-navigation-admin-tools, #left-nav button.nav-group-label')?.focus()");
        var tools = Group(page, "Tools");
        if (await page.Locator("#left-nav ul.nav-group-items[data-title=\"Tools\"] a.item-label:visible").CountAsync() == 0)
        {
            await tools.ClickAsync();
            await Task.Delay(500);
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await scene.HoldAsync(1.0);
            var link = page.Locator("#left-nav ul.nav-group-items[data-title=\"Tools\"] a.item-label:has-text(\"Features\")").First;
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), 300);
            await SearchAsync(scene, "OpenID");
        });

        await scene.StepAsync("features-2", async () =>
        {
            var heading = page.Locator(".feature-group h3:has-text(\"OpenID Connect\")").First;
            await heading.WaitForAsync();
            await scene.HighlightAsync(heading);
        });

        await scene.StepAsync("features-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "OrchardCore.OpenId.Server"));
        });

        await scene.StepAsync("features-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "OrchardCore.OpenId.Client"));
        });

        await scene.StepAsync("features-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "OrchardCore.OpenId.Management"));
        });

        await scene.StepAsync("features-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "OrchardCore.OpenId.Secrets"));
        });

        await scene.StepAsync("features-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "OrchardCore.OpenId.Validation"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Features");
        var box = page.Locator("#search-box");
        await box.WaitForAsync();
        await scene.PauseAsync(800);
        await box.FillAsync("OpenID");
        await scene.PauseAsync(600);
        if (await box.InputValueAsync() != "OpenID")
        {
            await box.FillAsync("OpenID");
            await scene.PauseAsync(600);
        }

        await Row(page, "OrchardCore.OpenId.Server").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("[data-filter-value*=\"OrchardCore.OpenId.Server\"]:visible .direct-dependencies").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = Row(page, "OrchardCore.OpenId.Server").Locator("a:has-text(\"Enable\"), button:has-text(\"Enable\")").First;
            await scene.MoveToAsync(enable);
            await scene.HoldAsync(2.2);
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(enable);
                await page.Locator("text=The feature OpenID Connect Authorization Server was enabled.").First.WaitForAsync(new() { Timeout = 120000 });
            }, 300);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await SearchAsync(scene, "OpenID");
            await scene.HighlightAsync(page.Locator("[data-filter-value*=\"OrchardCore.OpenId.Server\"]:visible .metadata .badge").First);
            await scene.HoldAsync(0.8);
            await scene.HighlightAsync(page.Locator("[data-filter-value*=\"OrchardCore.OpenId.Management\"]:visible .metadata .badge").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TourAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Features");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        ILocator Items(string title) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{title}\"]");
        ILocator Sub(string parent) => Items(parent).Locator("button.item-label:has(span.title:text-is(\"OpenID Connect\"))").First;

        await scene.StepAsync("tour-1", async () =>
        {
            await scene.ClickAsync(Group(page, "Settings"));
            await scene.ClickAsync(Sub("Settings"));
            var link = Items("Settings").Locator("ul[data-title=\"OpenID Connect\"] a.item-label:has-text(\"Authorization Server\")").First;
            await link.WaitForAsync();
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), 300);
        });

        await scene.StepAsync("tour-2", async () =>
        {
            var warning = page.Locator("form p.alert-warning").First;
            await warning.WaitForAsync();
            await scene.HighlightAsync(warning);
            await scene.HoldAsync(3.5);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 150);
        });

        await scene.StepAsync("tour-3", async () =>
        {
            await scene.ScrollAsync(0, 180);
            await scene.HighlightAsync(page.Locator("label:has-text(\"Allow Authorization Code Flow\")").First);
            await scene.HighlightAsync(page.Locator("label:has-text(\"Allow Refresh Token Flow\")").First);
        });

        await scene.StepAsync("tour-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HoldAsync(2.0);
            await scene.ScrollAsync(0, 120);
            await page.EvaluateAsync("() => { if (window.scrollY > 452) window.scrollTo(0, 450); }");
            Console.WriteLine("tour-4 scrollY " + await page.EvaluateAsync<double>("() => window.scrollY"));
        });

        await scene.StepAsync("tour-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.ClickAsync(Group(page, "Settings"));
            await scene.ClickAsync(Group(page, "Access Control"));
            await scene.ClickAsync(Sub("Access Control"));
            var link = Items("Access Control").Locator("ul[data-title=\"OpenID Connect\"] a.item-label:has-text(\"Applications\")").First;
            await link.WaitForAsync();
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), 300);
        });

        await scene.StepAsync("tour-6", async () =>
        {
            var link = Items("Access Control").Locator("ul[data-title=\"OpenID Connect\"] a.item-label:has-text(\"Scopes\")").First;
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), 300);
            await page.Locator("text=Roles").First.WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }
}

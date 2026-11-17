using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "OpenApi docs", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("02b-provision", ProvisionAsync),
        ("04-swagger", SwaggerAsync),
        ("05-explorers", ExplorersAsync),
        ("06-schema", SchemaAsync),
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

    /// <summary>Nothing to seed before the first clip. Warms the pages up once, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Features", "/Admin/Contents/ContentItems" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Pages warmed up.");
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

    /// <summary>Provisioning after 02-enable: the recipe "OpenAPI Documentation - Bearer/PKCE", then the redirect address of its application. No frames.</summary>
    private static async Task ProvisionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        await page.GotoAsync(BaseUrl + "/Admin/Recipes");
        await page.Locator("#search-box").FillAsync("OpenAPI Documentation");
        var run = page.Locator("[data-filter-value]").Filter(new() { HasTextString = "OpenAPI Documentation" }).Filter(new() { HasTextString = "PKCE" }).First.Locator("a, button").Filter(new() { HasTextString = "Run" }).First;
        await run.ClickAsync();
        await page.Locator(".modal.show").GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await WaitTextAsync(page, "has been run successfully", 180_000);
        await WaitSiteAsync();
        await Task.Delay(4000);

        await page.GotoAsync(BaseUrl + "/Admin/OpenId/Application");
        await page.GetByRole(AriaRole.Link, new() { Name = "Edit" }).First.ClickAsync();
        await page.GetByLabel("Redirect Uris", new() { Exact = true }).FillAsync(BaseUrl + "/OrchardCore.OpenApi/openapi-oidc-silent.html");
        await page.GetByLabel("Post-Logout Redirect Uris").FillAsync(BaseUrl + "/");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitSiteAsync();
        await Task.Delay(3000);

        foreach (var path in new[] { "/swagger", "/redoc", "/scalar/v1", "/Admin/Settings/openapi" })
        {
            await page.GotoAsync(BaseUrl + path);
            await Task.Delay(3000);
        }

        await context.CloseAsync();
        Console.WriteLine("Provisioned.");
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
        scene.Page.SetDefaultNavigationTimeout(90_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene.Page);
        }

        await scene.StartCaptureAsync();
        return scene;
    }

    private static ILocator Box(IPage page, string id) => page.Locator($"input[id='{id}']");

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Features", p => p.Locator("#search-box").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("OpenApi", new() { Delay = 50 });
            await page.WaitForFunctionAsync("() => /Showing 4 of/.test(document.querySelector('#list-summary')?.innerText || '')");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            foreach (var id in new[] { "OrchardCore.OpenApi.ReDocUI", "OrchardCore.OpenApi.ScalarUI", "OrchardCore.OpenApi.SwaggerUI" })
            {
                await scene.ClickAsync(Box(page, id), pauseAfter: 300);
            }

            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
            await GoOffAsync(scene, page.Locator("a.dropdown-item[data-action='Enable']").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            try
            {
                await WaitTextAsync(page, "The following features were enabled", 4_000);
            }
            catch (TimeoutException)
            {
                Console.WriteLine("(no notification on screen)");
            }

            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("OpenApi", new() { Delay = 50 });
            await page.WaitForFunctionAsync("() => /Showing 4 of/.test(document.querySelector('#list-summary')?.innerText || '')");
        });
    }

    private static async Task GoOffAsync(Scene scene, ILocator locator, int lead = 250)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: lead);
    }

    private static async Task SwaggerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        string blogId = null;
        await using var scene = await StartAtAsync(browser, directory, durations, "/swagger", async p =>
        {
            await p.Locator(".opblock-tag").First.WaitForAsync();
            try
            {
                await p.Locator("button.authorization__btn.locked").First.WaitForAsync(new() { Timeout = 15_000 });
            }
            catch (TimeoutException)
            {
                await Task.Delay(5000);
            }

            var other = await p.Context.NewPageAsync();
            await other.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems");
            var href = await other.Locator("#left-nav a").Filter(new() { HasTextString = "Blog" }).First.GetAttributeAsync("href");
            blogId = href.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipWhile(s => s != "ContentItems").Skip(1).First();
            await other.CloseAsync();
            await p.BringToFrontAsync();
        });
        var page = scene.Page;
        var block = page.Locator("div.opblock-get").Filter(new() { Has = page.Locator("span.opblock-summary-path[data-path='/api/content/{contentItemId}']") }).First;

        await scene.StepAsync("swagger-1", async () =>
        {
            await scene.PauseAsync(1500);
            await ShowAsync(scene, page.Locator(".opblock-tag").First, 1500);
        });

        await scene.StepAsync("swagger-2", async () =>
        {
            await block.ScrollToMiddleAsync();
            await scene.ClickAsync(block.Locator(".opblock-summary-control").First, pauseAfter: 500);
            await block.Locator("button.try-out__btn").WaitForAsync();
        });

        await scene.StepAsync("swagger-3", async () =>
        {
            await scene.ClickAsync(block.Locator("button.try-out__btn"), pauseAfter: 400);
            var input = block.Locator("input[type='text']").First;
            await scene.TypeAsync(input, blogId, delay: 35);
            await scene.ClickAsync(block.Locator("button.execute"), pauseAfter: 400);
            await block.Locator(".live-responses-table .response-col_status").Filter(new() { HasTextString = "200" }).First.WaitForAsync();
            await block.Locator(".request-url").First.EvaluateAsync("e => e.scrollIntoView({ block: 'start', behavior: 'instant' })");
        });
    }

    private static async Task ExplorersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/redoc", p => p.Locator("h1, h2").First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("redoc-1", async () =>
        {
            await scene.PauseAsync(1200);
            await scene.ScrollAsync(0, 500);
        });

        await scene.StepAsync("scalar-1", async () =>
        {
            await scene.NavigateAsync("/scalar/v1");
            await page.GetByText("OpenApi V1").First.WaitForAsync();
            await scene.PauseAsync(1500);
        });
    }

    private static async Task SchemaAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Settings/openapi", p => p.GetByLabel("Allow anonymous access to the API schema").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("schema-1", async () =>
        {
            await ShowAsync(scene, page.GetByRole(AriaRole.Heading, new() { Name = "API Documentation UIs" }).First, 2000);
        });

        await scene.StepAsync("schema-2", async () =>
        {
            var box = page.GetByLabel("Allow anonymous access to the API schema");
            await box.ScrollToMiddleAsync();
            await scene.ClickAsync(box, pauseAfter: 400);
            await ShowAsync(scene, page.GetByText("When enabled, the JSON schema endpoints").First, 1500);
        });

        await scene.StepAsync("schema-3", async () =>
        {
            await GoOffAsync(scene, page.Locator("button.save, button.primaryAction").Filter(new() { HasTextString = "Save" }).First);
            await WaitTextAsync(page, "Site settings updated successfully.", 120_000);
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

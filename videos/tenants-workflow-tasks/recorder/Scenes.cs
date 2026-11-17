using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Tenant workflow tasks", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-workflow", WorkflowAsync),
        ("04-create", CreateAsync),
        ("05-setup", SetupAsync),
        ("06-run", RunAsync),
        ("07-result", ResultAsync),
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

    /// <summary>Nothing to seed: every record of the video is created by the clips.</summary>
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private const string WorkflowName = "Create and set up a tenant";

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<Scene, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(180_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static async Task GoOffAsync(Scene scene, ILocator locator, int lead = 250)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: lead);
    }

    private static async Task OpenDesignerAsync(Scene scene, bool pin)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await page.GetByRole(AriaRole.Link, new() { Name = WorkflowName, Exact = true }).First.ClickAsync();
        await page.Locator("[data-cy='workflow-designer'], .wfd-node").First.WaitForAsync();
        await page.Locator("[data-cy='panel-tab-issues']").First.ClickAsync();
        await page.Locator("button.leftbar-compactor").First.ClickAsync();
        await Task.Delay(600);
        if (pin)
        {
            try
            {
                await page.Locator("button[aria-label*='Keep the activity panel'], button[title*='Keep the activity panel'], [data-cy='panel-pin']").First.ClickAsync(new() { Timeout = 4000 });
            }
            catch (TimeoutException)
            {
                Console.WriteLine("(no pin button)");
            }
        }
    }

    private static ILocator Node(IPage page, string text) => page.Locator(".wfd-node").Filter(new() { HasTextString = text }).First;

    private static async Task TypeFieldAsync(Scene scene, string suffix, string text)
    {
        var field = scene.Page.Locator($"input[name$='{suffix}']").First;
        await field.ScrollToMiddleAsync();
        await scene.TypeAsync(field, text, delay: 35);
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

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Features", async s => await s.Page.Locator("#search-box").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("features-1", async () =>
        {
            await scene.MoveToAsync(page.Locator("#left-nav a[href$='/Admin/Features']").First);
            await scene.HighlightAsync(page.Locator("#left-nav a[href$='/Admin/Features']").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "Tenants", delay: 50);
            var enable = page.Locator("#btn-enable-OrchardCore_Tenants");
            await enable.WaitForAsync();
            await GoOffAsync(scene, enable);
            await WaitTextAsync(page, "The feature Tenants was enabled.", 120_000);
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("Workflows", new() { Delay = 50 });
            var enable = page.Locator("#btn-enable-OrchardCore_Workflows");
            await enable.WaitForAsync();
            await GoOffAsync(scene, enable);
            await WaitTextAsync(page, "was enabled", 120_000);
            await scene.MoveToAsync(900, 650);
            await OpenAdminGroupAsync(page, "Multi-Tenancy");
            await OpenAdminGroupAsync(page, "Design");
            await scene.HighlightAsync(page.Locator("#left-nav a").Filter(new() { HasTextString = "Tenants" }).First);
            await scene.HighlightAsync(page.Locator("#left-nav a").Filter(new() { HasTextString = "Workflows" }).First);
        });
    }

    private static async Task WorkflowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", async s => await OpenAdminGroupAsync(s.Page, "Design"));
        var page = scene.Page;

        await scene.StepAsync("workflow-1", async () =>
        {
            await GoAsync(scene, page.Locator("#left-nav a[href$='/Admin/Workflows/Types']").First);
            await GoAsync(scene, page.Locator("a.btn.create").First);
            await scene.TypeAsync(page.Locator("input[name='Name']"), WorkflowName, delay: 35);
        });

        await scene.StepAsync("workflow-2", async () =>
        {
            await scene.ClickAsync(page.Locator("summary").Filter(new() { HasTextString = "More settings" }).First, pauseAfter: 400);
            var box = page.Locator("input[type='checkbox'][name='RecordActivityData']");
            await scene.HighlightAsync(box);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(box, pauseAfter: 400);
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Create and open the designer" }));
            await page.Locator("[data-cy='start-picker']").WaitForAsync();
        });

        await scene.StepAsync("workflow-3", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-search']"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Started", new() { Delay = 50 });
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-activity-StartedByWorkflowEvent']"), pauseAfter: 300);
            await page.Locator("[data-cy='start-badge']").WaitForAsync();
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Workflows/Types", s => OpenDesignerAsync(s, pin: true));
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            var done = Node(page, "Started By Workflow").Locator(".wfd-port[data-outcome='Done']");
            await scene.MoveToAsync(done);
            await scene.ClickAsync(done, pauseAfter: 150);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "Create Tenant", delay: 40);
            await scene.ClickAsync(page.Locator("[data-cy='quick-add-CreateTenantTask']"), pauseAfter: 300);
            await page.Locator("input[name$='TenantNameExpression']").First.WaitForAsync();
        });

        await scene.StepAsync("create-2", async () =>
        {
            await TypeFieldAsync(scene, "TenantNameExpression", "Bluebird");
            await TypeFieldAsync(scene, "DescriptionExpression", "Bluebird");
            await TypeFieldAsync(scene, "RequestUrlPrefixExpression", "bluebird");
        });

        await scene.StepAsync("create-3", async () =>
        {
            await TypeFieldAsync(scene, "DatabaseProviderExpression", "Sqlite");
            await TypeFieldAsync(scene, "RecipeNameExpression", "Blog");
            await page.Keyboard.PressAsync("Tab");
            await scene.ClickAsync(page.Locator("[data-cy='panel-apply']"), pauseAfter: 300);
            await page.Locator("[data-cy='panel-apply']").Filter(new() { HasTextString = "Saved" }).WaitForAsync();
        });
    }

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Workflows/Types", s => OpenDesignerAsync(s, pin: true));
        var page = scene.Page;

        await scene.StepAsync("setup-1", async () =>
        {
            var done = Node(page, "Create Tenant Task").Locator(".wfd-port[data-outcome='Done']");
            await scene.MoveToAsync(done);
            await scene.ClickAsync(done, pauseAfter: 150);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "Setup Tenant", delay: 40);
            await scene.ClickAsync(page.Locator("[data-cy='quick-add-SetupTenantTask']"), pauseAfter: 300);
            await page.Locator("input[name$='SiteNameExpression']").First.WaitForAsync();
            await TypeFieldAsync(scene, "TenantNameExpression", "Bluebird");
            await TypeFieldAsync(scene, "SiteNameExpression", "Bluebird Books");
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await TypeFieldAsync(scene, "AdminUsernameExpression", "admin");
            await TypeFieldAsync(scene, "AdminEmailExpression", "owner@example.com");
            await TypeFieldAsync(scene, "AdminPasswordExpression", "Bluebird-demo-2");
        });

        await scene.StepAsync("setup-3", async () =>
        {
            var recipe = page.Locator("input[name$='RecipeNameExpression']").First;
            await recipe.ScrollToMiddleAsync();
            await scene.PauseAsync(1500);
            await page.Keyboard.PressAsync("Tab");
            await scene.ClickAsync(page.Locator("[data-cy='panel-apply']"), pauseAfter: 300);
            await page.Locator("[data-cy='panel-apply']").Filter(new() { HasTextString = "Saved" }).WaitForAsync();
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Workflows/Types", s => OpenDesignerAsync(s, pin: false));
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            await page.WaitForFunctionAsync("() => document.body.innerText.includes('Saved')");
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-publish']"));
            await WaitTextAsync(page, "The workflow was published as version");
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-run']"), pauseAfter: 500);
            await scene.ClickAsync(page.Locator("[data-cy='run-start']"), pauseAfter: 300);
            await scene.OffCameraAsync(async () => await page.Locator("[data-cy='run-status']").Filter(new() { HasTextString = "Finished" }).WaitForAsync(new() { Timeout = 240_000 }), leadMilliseconds: 250);
            await scene.MoveToAsync(900, 650);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await GoAsync(scene, page.Locator("[data-cy='run-open-instance']"));
            await page.Locator("#workflow-designer, [data-cy='workflow-designer']").First.WaitForAsync();
        });
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Tenants", s => s.Page.Locator("a.text-break").Filter(new() { HasTextString = "Bluebird" }).First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("result-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "Bluebird" }).First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("result-2", async () =>
        {
            var link = page.Locator("a.text-break").Filter(new() { HasTextString = "Bluebird" }).First;
            await scene.MoveToAsync(link);
            var (x, y) = await scene.CenterOfAsync(link);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y));
                await page.WaitForFunctionAsync("() => document.title.includes('Bluebird')", null, new() { Timeout = 240_000 });
            }, leadMilliseconds: 250);
            await scene.MoveToAsync(900, 650);
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

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Session activities", recorded in order on one fresh site.</summary>
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
        ("02b-import", ImportAsync),
        ("02-activity", ActivityAsync),
        ("03-workflow", WorkflowAsync),
        ("03-run", RunAsync),
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
        {"steps":[{"name":"feature","enable":["OrchardCore.Workflows"],"disable":[]}]}
        """;

    private const string WorkflowJson = """
        {"steps":[{"name":"WorkflowType","data":[{"WorkflowTypeId":"orderhandoff","Name":"Order handoff","IsEnabled":true,"IsSingleton":false,"DeleteFinishedWorkflows":false,"Variables":[],
        "Activities":[
         {"ActivityId":"start","Name":"StartedByWorkflowEvent","X":40,"Y":120,"IsStart":true,"Properties":{}},
         {"ActivityId":"fork","Name":"ForkTask","X":310,"Y":120,"IsStart":false,"Properties":{"Forks":["Billing","Shipping"]}},
         {"ActivityId":"logbilling","Name":"LogTask","X":580,"Y":20,"IsStart":false,"Properties":{"ActivityMetadata":{"Title":"Prepare billing"},"Text":{"Expression":"Billing prepared for order 1001","Syntax":"Liquid"}}},
         {"ActivityId":"logshipping","Name":"LogTask","X":580,"Y":220,"IsStart":false,"Properties":{"ActivityMetadata":{"Title":"Prepare shipping"},"Text":{"Expression":"Shipping prepared for order 1001","Syntax":"Liquid"}}},
         {"ActivityId":"join","Name":"JoinTask","X":850,"Y":120,"IsStart":false,"Properties":{}},
         {"ActivityId":"commit","Name":"CommitTransactionTask","X":1120,"Y":120,"IsStart":false,"Properties":{}},
         {"ActivityId":"logdone","Name":"LogTask","X":1120,"Y":320,"IsStart":false,"Properties":{"ActivityMetadata":{"Title":"Report the result"},"Text":{"Expression":"Order 1001 committed","Syntax":"Liquid"}}}],
        "Transitions":[
         {"SourceActivityId":"start","SourceOutcomeName":"Done","DestinationActivityId":"fork"},
         {"SourceActivityId":"fork","SourceOutcomeName":"Billing","DestinationActivityId":"logbilling"},
         {"SourceActivityId":"fork","SourceOutcomeName":"Shipping","DestinationActivityId":"logshipping"},
         {"SourceActivityId":"logbilling","SourceOutcomeName":"Done","DestinationActivityId":"join"},
         {"SourceActivityId":"logshipping","SourceOutcomeName":"Done","DestinationActivityId":"join"},
         {"SourceActivityId":"join","SourceOutcomeName":"Joined","DestinationActivityId":"commit"},
         {"SourceActivityId":"commit","SourceOutcomeName":"Valid","DestinationActivityId":"logdone"}]}]}]}
        """;

    private static async Task ImportJsonAsync(IPage page, string json)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!");
    }

    /// <summary>Enables the Workflows feature (snapshot A), outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        await ImportJsonAsync(page, FeaturesJson);
        await Task.Delay(4000);
        foreach (var path in new[] { "/Admin/Features", "/Admin/Workflows/Types", "/" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Workflows enabled.");
    }

    /// <summary>Imports the workflow "Order handoff" after 02-enable (snapshot B). No frames.</summary>
    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);
        await ImportJsonAsync(page, WorkflowJson);
        await page.GotoAsync(BaseUrl + "/Admin/Workflows/Types");
        await page.GetByRole(AriaRole.Link, new() { Name = "Order handoff", Exact = true }).First.ClickAsync();
        await page.Locator("[data-cy='activity-commit']").WaitForAsync();
        await context.CloseAsync();
        Console.WriteLine("Imported.");
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

    private static async Task<Scene> StartFeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(90_000);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static async Task ClickIfAsync(ILocator locator)
    {
        try
        {
            if (await locator.CountAsync() > 0 && await locator.First.IsVisibleAsync())
            {
                await locator.First.ClickAsync(new() { Timeout = 3000 });
            }
        }
        catch (PlaywrightException)
        {
        }
    }

    // Opens the designer outside the capture, then sets the panes: the activities pane open or collapsed, the workflow panel collapsed.
    private static async Task<Scene> StartDesignerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, bool collapseToolbox)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        scene.Page.SetDefaultNavigationTimeout(90_000);
        await scene.OpenAsync("/Admin/Workflows/Types");
        await page.GetByRole(AriaRole.Link, new() { Name = "Order handoff", Exact = true }).First.ClickAsync();
        await page.Locator("[data-cy='activity-commit']").WaitForAsync();
        await ClickIfAsync(page.GetByRole(AriaRole.Button, new() { Name = "Collapse the panel" }));
        if (collapseToolbox)
        {
            await ClickIfAsync(page.GetByRole(AriaRole.Button, new() { Name = "Hide the activities" }));
        }

        await Task.Delay(500);
        await ClickIfAsync(page.GetByRole(AriaRole.Button, new() { Name = "Fit to content" }));
        await Task.Delay(800);
        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static ILocator Node(IPage page, string id) => page.Locator($"[data-cy='activity-{id}']");

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartFeaturesAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("session", new() { Delay = 50 });
            var row = page.Locator("[data-filter-value*='OrchardCore.Workflows.Session']").First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row, "Session Workflows Activities");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("#btn-enable-OrchardCore_Workflows_Session");
            await scene.MoveToAsync(enable);
            var (x, y) = await scene.CenterOfAsync(enable);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
            await scene.MoveToAsync(900, 650);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("session", new() { Delay = 50 });
        });
    }

    private static async Task ActivityAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartDesignerAsync(browser, directory, durations, collapseToolbox: false);
        var page = scene.Page;

        await scene.StepAsync("activity-1", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-search']"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("commit", new() { Delay = 50 });
            var card = page.Locator("[data-cy^='toolbox-activity-']").First;
            await card.WaitForAsync();
            await scene.HighlightAsync(card);
        });

        await scene.StepAsync("activity-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-search']"), pauseAfter: 100);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.PressAsync("Delete");
            await scene.HighlightAsync(Node(page, "commit"));
            await scene.PauseAsync(1500);
            await scene.DoubleClickAsync(Node(page, "commit"), pauseAfter: 400);
            await page.Locator("[data-cy='activity-panel']").WaitForAsync();
        });
    }

    private static async Task WorkflowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartDesignerAsync(browser, directory, durations, collapseToolbox: true);
        var page = scene.Page;

        await scene.StepAsync("workflow-1", async () =>
        {
            foreach (var id in new[] { "start", "fork" })
            {
                await ShowAsync(scene, Node(page, id), 1500);
            }

            await scene.HighlightAsync(Node(page, "logbilling"));
            await scene.HighlightAsync(Node(page, "logshipping"));
        });

        await scene.StepAsync("workflow-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            foreach (var id in new[] { "join", "commit" })
            {
                await ShowAsync(scene, Node(page, id), 1500);
            }

            await scene.HighlightAsync(Node(page, "logdone"));
        });

        await scene.StepAsync("workflow-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.DoubleClickAsync(Node(page, "fork"), pauseAfter: 400);
            var forks = page.GetByLabel("Forks");
            await forks.WaitForAsync();
            await ShowAsync(scene, forks, 1800);
            await page.Keyboard.PressAsync("Escape");
            await scene.DoubleClickAsync(Node(page, "join"), pauseAfter: 400);
            var mode = page.GetByLabel("Mode");
            await mode.WaitForAsync();
            await scene.HighlightAsync(mode);
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartDesignerAsync(browser, directory, durations, collapseToolbox: true);
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-run']"), pauseAfter: 500);
            await scene.ClickAsync(page.Locator("[data-cy='run-start']"), pauseAfter: 300);
            await page.Locator("[data-cy='run-status']").Filter(new() { HasTextString = "Finished" }).WaitForAsync();
            await scene.MoveToAsync(900, 650);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await GoAsync(scene, page.Locator("[data-cy='run-open-instance']"));
            await page.Locator("#workflow-designer").WaitForAsync();
            await page.Locator("#workflow-designer").ScrollToMiddleAsync();
            var rail = page.Locator("[data-cy='panel-rail-journal']");
            if (await rail.CountAsync() > 0 && await rail.First.IsVisibleAsync())
            {
                await scene.ClickAsync(rail.First, pauseAfter: 400);
            }
            else
            {
                await scene.ClickAsync(page.Locator("[data-cy='panel-tab-journal']"), pauseAfter: 400);
            }

            await page.Locator("[data-cy='journal-record-6']").WaitForAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='journal-record-4']"));
            await scene.HighlightAsync(page.Locator("[data-cy='journal-record-6']"));
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='journal-record-7']"), pauseAfter: 500);
            await scene.HighlightAsync(page.Locator("[data-cy='journal-record-7'] [data-cy='journal-outcomes']"));
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

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Workflows in depth: expressions, variables and loops", on a Blog site seeded with one workflow.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string WorkflowName = "Sample: count the coffees";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-designer", DesignerAsync),
        ("02-declare", DeclareAsync),
        ("03-canvas", CanvasAsync),
        ("04-run", RunAsync),
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

    /// <summary>Enables Workflows and imports the sample workflow, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#btn-enable-OrchardCore_Workflows").ClickAsync();
        await WaitTextAsync(page, "was enabled");

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", Seed.Json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!");
        await page.GotoAsync(BaseUrl + "/Admin/Workflows/Types");

        await context.CloseAsync();
        Console.WriteLine("Sample workflow imported.");
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

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1500, string caption = null)
    {
        await scene.HighlightAsync(locator, caption);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Node(IPage page, string id) => page.Locator($"[data-cy='activity-{id}']");

    // Opens the designer of the sample workflow, outside the capture, with the activities pane hidden and the canvas fitted.
    private static async Task OpenDesignerAsync(Scene scene)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await page.GetByRole(AriaRole.Link, new() { Name = WorkflowName }).First.ClickAsync();
        await page.Locator("[data-cy='designer-toolbar']").WaitForAsync();
        await Node(page, "writesummary").WaitForAsync();
        await page.Locator("button.leftbar-compactor").ClickAsync();
        var collapse = page.Locator("[data-cy='toolbox-collapse']");

        if (await collapse.IsVisibleAsync())
        {
            await collapse.ClickAsync();
        }

        await page.Locator("[data-cy='zoom-fit']").ClickAsync();
        await Task.Delay(800);
    }

    private static async Task ClosePanelAsync(Scene scene)
        => await scene.ClickAsync(scene.Page.Locator("button[aria-label='Close the activity panel']"), pauseAfter: 300);

    private static async Task DesignerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("designer-1", async () =>
        {
            await scene.DoubleClickAsync(Node(page, "eachitem"), pauseAfter: 500);
            await page.Locator("select[aria-label='Syntax of Items']").WaitForAsync();
            await scene.HighlightAsync(page.Locator("select[aria-label='Syntax of Items']"), "Literal");
        });

        await scene.StepAsync("designer-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ClosePanelAsync(scene);
            await scene.DoubleClickAsync(Node(page, "checkitem"), pauseAfter: 500);
            await page.Locator("select[aria-label='Syntax of Condition']").WaitForAsync();
            await scene.HighlightAsync(page.Locator("select[aria-label='Syntax of Condition']"), "JavaScript");
        });

        await scene.StepAsync("designer-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ClosePanelAsync(scene);
            await scene.DoubleClickAsync(Node(page, "writesummary"), pauseAfter: 500);
            await page.Locator("select[aria-label='Syntax of Value']").WaitForAsync();
            await scene.HighlightAsync(page.Locator("select[aria-label='Syntax of Value']"), "Liquid");
        });

        await scene.StepAsync("designer-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='activity-tab-data']"), pauseAfter: 500);
            await page.Locator("[data-cy='available-data']").WaitForAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='available-data']"));
        });
    }

    private static async Task DeclareAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("declare-1", async () =>
        {
            await ShowAsync(scene, page.Locator("[data-cy='variables-tab']"), 3000);
            await scene.HighlightAsync(page.Locator("[data-cy='variable']").Nth(1).Locator("[data-cy='variable-is-output']"), "Output");
        });
    }

    private static async Task CanvasAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("canvas-1", async () =>
        {
            await ShowAsync(scene, page.Locator("[data-cy='port-checkitem-True']"), 3000, "True");
            await scene.HighlightAsync(page.Locator("[data-cy='port-checkitem-False']"), "False");
        });

        await scene.StepAsync("canvas-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='panel-tab-workflow']"), pauseAfter: 400);
            var select = page.GetByLabel("Outcomes with several transitions");
            await select.WaitForAsync(new() { State = WaitForSelectorState.Attached });
            await select.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(select);
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await ShowAsync(scene, page.Locator("[data-cy='port-eachitem-Iterate']"), 2500, "Iterate");
            await ShowAsync(scene, Node(page, "checkitem"), 2500);
            await ShowAsync(scene, Node(page, "countcoffee"), 3000);
            await scene.HighlightAsync(page.Locator("[data-cy='port-eachitem-Done']"), "Done");
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-run']"), pauseAfter: 300);
            await page.Locator(".modal.show").WaitForAsync();
            await scene.ClickAsync(page.Locator("[data-cy='run-start']"), pauseAfter: 200);
            await page.WaitForFunctionAsync("() => document.querySelector(\"[data-cy='run-status']\")?.innerText.includes('Finished')");
            await page.Locator("[data-cy='run-output-summary']").WaitForAsync();
        });

        await scene.StepAsync("run-3", async () =>
        {
            await GoAsync(scene, page.Locator("[data-cy='run-open-instance']"));
            await Node(page, "eachitem").WaitForAsync();
            await page.Locator("[data-cy='executed-badge']").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='activity-eachitem'] [data-cy='executed-badge']").First);
        });

        await scene.StepAsync("run-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, page.Locator("[data-cy='variables-tab']"), 3000);
            await scene.HighlightAsync(page.Locator("[data-cy='variable-row-summary']"));
        });
    }
}

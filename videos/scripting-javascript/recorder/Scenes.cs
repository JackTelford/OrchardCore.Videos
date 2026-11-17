using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Scripting: JavaScript in Workflows and Recipes": the sample workflow "Sample: order total" of the
/// Workflow Variables Samples recipe, which PrepareAsync runs outside the capture. Restore the snapshot before a
/// new take (the designer saves a draft as it goes).
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-workflow", WorkflowAsync),
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

    // Runs the Workflow Variables Samples recipe (it enables Workflows) outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        await page.GotoAsync($"{BaseUrl}/Admin/Recipes", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#search-box").FillAsync("Workflow Variables");
        var row = page.Locator(".recipe-group .list-group-item", new() { HasText = "Workflow Variables Samples" }).First;
        await row.GetByText("Run", new() { Exact = true }).ClickAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.Locator(".toast.message-success").First.WaitForAsync();
        await Task.Delay(4000);
        await page.GotoAsync($"{BaseUrl}/Admin/Workflows/Types", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine(await page.Locator("body").InnerTextAsync() is { } text && text.Contains("Sample: order total") ? "Seeded." : "NOT seeded.");
        await context.CloseAsync();
    }

    private static async Task WorkflowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        var link = scene.Page.GetByRole(AriaRole.Link, new() { Name = "Sample: order total", Exact = true });

        // Not recorded: grow the activity panel once, so the whole script is visible (the designer remembers the height).
        await scene.OpenAsync("/Admin/Workflows/Types");
        await link.ClickAsync();
        await scene.Page.Locator("[data-cy='activity-readquery']").WaitForAsync();
        await scene.Page.Locator("[data-cy='activity-readquery']").DblClickAsync();
        await scene.Page.Locator("[data-cy='activity-panel']").WaitForAsync();
        var resizer = scene.Page.Locator("[data-cy='activity-panel-resizer']");
        await resizer.FocusAsync();

        for (var i = 0; i < 6; i++)
        {
            await scene.Page.Keyboard.PressAsync("Shift+ArrowUp");
        }

        var box = await resizer.BoundingBoxAsync();
        Console.WriteLine($"  resizer at {box?.Y}");
        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("workflow-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(link);
            await scene.Page.Locator("[data-cy='activity-readquery']").WaitForAsync();
            await scene.Page.Locator("[data-cy='activity-computetotal']").WaitForAsync();
        });

        await scene.StepAsync("workflow-2", async () =>
        {
            await scene.DoubleClickAsync(scene.Page.Locator("[data-cy='activity-readquery']"));
            await scene.Page.Locator("[data-cy='activity-panel']").WaitForAsync();
            await scene.Page.Locator("[data-cy='panel-activity-form'] .monaco-editor").First.WaitForAsync();
        });

        await scene.StepAsync("workflow-3", async () =>
        {
            // The open panel covers the middle of the node: double-click on its header, which stays visible.
            var (x, y) = await scene.CenterOfAsync(scene.Page.Locator("[data-cy='activity-computetotal']"));
            await scene.MoveToAsync(x, y - 26);
            await scene.Page.Mouse.DblClickAsync((float)x, (float)(y - 26));
            await scene.Page.Locator("[data-cy='panel-activity-title']", new() { HasTextString = "Compute the total" }).WaitForAsync();
            await scene.HighlightAsync(scene.Page.GetByLabel("Syntax of Value"));
        });

        await scene.StepAsync("workflow-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("[data-cy='activity-tab-data']"), pauseAfter: 300);
            await scene.Page.Locator("[data-cy='available-data']").WaitForAsync();
            await scene.ClickAsync(scene.Page.Locator("[data-cy='available-group-functions']"), pauseAfter: 300);
        });
    }
}

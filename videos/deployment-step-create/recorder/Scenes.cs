using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Write a Deployment Step": 06-plan creates the plan "Release package", adds the All Features step to it
/// and opens the list of deployment targets (without selecting one, so nothing is downloaded).
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
        ("06-plan", PlanAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task FastAsync(ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 8 });
    }

    private static async Task PlanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/DeploymentPlan/Index");

        // Opens Tools and Deployments in the admin menu before the capture, so that Plans is visible.
        var plans = page.GetByRole(AriaRole.Link, new() { Name = "Plans", Exact = true }).First;

        if (!await plans.IsVisibleAsync())
        {
            await page.Locator("#left-nav button.item-label", new() { HasTextRegex = new(@"^\s*Tools\s*$") }).First.ClickAsync();
            await Task.Delay(600);
            await page.Locator("#left-nav button.item-label", new() { HasTextRegex = new(@"^\s*Deployments\s*$") }).First.ClickAsync();
            await Task.Delay(600);
        }

        await scene.StartCaptureAsync();
        var card = page.Locator("#modalSteps .deployment-step-item").Filter(new() { HasText = "All Features" }).First;
        var ignore = page.GetByLabel("Ignore disabled features");

        await scene.StepAsync("plan-1", async () =>
        {
            await scene.HighlightAsync(plans, "Plans");
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a.create", new() { HasText = "Add Deployment Plan" }).First);
            await FastAsync(page.Locator("#Name"), "Release package");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.create", new() { HasText = "Create" }).First);
        });

        await scene.StepAsync("plan-2", async () =>
        {
            await scene.ClickAsync(page.Locator("button", new() { HasTextRegex = new(@"^\s*Add Step\s*$") }).First);
            await page.Locator("#modalSteps").WaitForAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(page.Locator("#deployment-step-categories .nav-link", new() { HasText = "Infrastructure" }).First, "Category");
        });

        await scene.StepAsync("plan-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#deployment-step-categories .nav-link", new() { HasText = "Infrastructure" }).First);
            await card.WaitForAsync();
            await scene.HighlightAsync(card, "Thumbnail view");
        });

        await scene.StepAsync("plan-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(card.GetByRole(AriaRole.Link, new() { Name = "Add" }).First);
            await page.Locator(".oc-breadcrumb-title").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".oc-breadcrumb-title").First, "Title");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(ignore, "Edit view");
        });

        await scene.StepAsync("plan-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(ignore);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.create", new() { HasText = "Create" }).First);
            await page.Locator("#stepOrder").WaitForAsync();
            await scene.HighlightAsync(page.Locator("#stepOrder .list-group-item").First, "Summary view");
        });

        await scene.StepAsync("plan-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("button, a", new() { HasTextRegex = new(@"^\s*Execute\s*$") }).First);
            await page.Locator("#modalTargets").WaitForAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(page.Locator("#modalTargets .card").Filter(new() { HasText = "File Download" }).First, "Target");
        });
    }
}

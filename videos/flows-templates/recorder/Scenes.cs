using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Templates for Flow and Bag Parts". The site is made with the Agency recipe. 03-display only reads; 06-debug saves
/// the debugging setting. No PrepareAsync. Never open the front end, the contents list or the dashboard (Agency sample content).
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
        ("03-display", DisplayAsync),
        ("06-debug", DebugAsync),
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

    private static async Task DisplayAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/LandingPage");
        await scene.Page.Locator("#parts").WaitForAsync();
        await scene.Page.EvaluateAsync("document.querySelector('#parts').scrollIntoView({ block: 'center' })");
        await scene.PauseAsync(500);
        await scene.StartCaptureAsync();

        var services = scene.Page.Locator("#parts li").Filter(new() { HasText = "Services" }).First;

        await scene.StepAsync("display-1", async () =>
        {
            await scene.HighlightAsync(services);
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("display-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var edit = services.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true }).First;
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(edit), leadMilliseconds: 150);
            var field = scene.Page.GetByLabel("Display Type", new() { Exact = true }).First;
            await field.WaitForAsync();
            await field.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await scene.PauseAsync(400);
            await scene.HighlightAsync(field);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DebugAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/debugging");
        var option = scene.Page.GetByLabel("Write shape debug information").First;
        await option.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("debug-1", async () =>
        {
            await scene.HighlightAsync(option);
        });

        await scene.StepAsync("debug-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(option, pauseAfter: 300);
            var save = scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText("Site settings updated successfully.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("Site settings updated successfully.").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

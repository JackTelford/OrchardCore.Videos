using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

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

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-before", BeforeAsync),
        ("03-after", AfterAsync),
        ("04-after", ItemAfterAsync),
        ("05-debug", DebugAsync),
    ];

    private const string Subheading = "header.masthead .site-heading .subheading";

    private static async Task BeforeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/events/harvest-festival");
        await page.Locator("header.masthead").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("before-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("header.masthead .site-heading").First, "Title only");
            await scene.PauseAsync(1200);
        });

        await scene.StepAsync("before-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/events/night-market");
            await scene.PauseAsync(900);
            await scene.NavigateAsync("/events/garden-tour");
        });
    }

    private static async Task AfterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("type-3", async () =>
        {
            await scene.NavigateAsync("/events/harvest-festival");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(page.Locator(Subheading).First, "Event");
        });

        await scene.StepAsync("type-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/events/night-market");
            await scene.PauseAsync(900);
            await scene.NavigateAsync("/events/garden-tour");
        });
    }

    private static async Task ItemAfterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/events/harvest-festival");
        await scene.StartCaptureAsync();

        await scene.StepAsync("item-3", async () =>
        {
            await scene.NavigateAsync("/events/night-market");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(page.Locator(Subheading).First, "Featured event");
        });

        await scene.StepAsync("item-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/events/harvest-festival");
        });
    }

    private static async Task DebugAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Settings/debugging");
        var label = page.Locator("label", new() { HasTextString = "Write shape debug information" }).First;
        await label.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("debug-1", async () =>
        {
            await scene.HighlightAsync(label, "Off by default");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(label);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First);
        });

        await scene.StepAsync("debug-2", async () =>
        {
            await scene.NavigateAsync("/events/night-market");
        });
    }
}

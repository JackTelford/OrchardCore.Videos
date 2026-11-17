using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Shapes: how Orchard Core renders a page". 09-debug saves the Debugging settings (it changes the site).
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
        ("02-page", PageAsync),
        ("09-debug", DebugAsync),
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

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator(".post-preview").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("page-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#navbarResponsive").First, "Menu shape");
        });

        await scene.StepAsync("page-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("header.masthead").First, "Header zone");
        });

        await scene.StepAsync("page-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator(".post-preview").First, "Summary display type");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DebugAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("debug-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav button.nav-group-label:has(.title:text-is('Settings'))").First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.item-label:has(.title:text-is('Debugging'))").First);
            await scene.ClickAsync(scene.Page.Locator("label:has-text('Write shape debug information')").First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
        });

        await scene.StepAsync("debug-2", async () =>
        {
            await scene.NavigateAsync("/");
            await scene.Page.Locator("header.masthead").WaitForAsync();
        });
    }
}

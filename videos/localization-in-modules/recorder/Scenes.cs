using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clip of the localization-in-modules video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("04-plural-screen", PluralAsync),
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

    private static async Task PluralAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var summary = page.Locator("label#items");

        await scene.StepAsync("items-1", async () =>
        {
            await scene.PauseAsync(600);
            await scene.HighlightAsync(summary, "Plural");
        });

        await scene.StepAsync("items-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var search = page.Locator(".action-bar input[type='search']").First;
            await scene.ClickAsync(search, pauseAfter: 100);
            await page.Keyboard.TypeAsync("About", new() { Delay = 60 });
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await page.Keyboard.PressAsync("Enter"));
                await Assertions.Expect(summary).ToContainTextAsync("1 item", new() { Timeout = 60000 });
            });
        });

        await scene.StepAsync("items-3", async () =>
        {
            await scene.HighlightAsync(summary, "Singular");
        });
    }
}

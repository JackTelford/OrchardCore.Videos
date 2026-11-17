using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Add Site Settings to Your Module": the Debugging group of the Settings module on a stock site.
/// The clip saves the group with the box ticked: restore the snapshot before a new take.
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
        ("05-try", TryAsync),
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

    private static async Task TryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Admin");
        var settings = scene.Page.Locator("#left-nav button.item-label").Filter(new() { HasTextRegex = new(@"^\s*Settings\s*$") }).First;

        if (await settings.GetAttributeAsync("aria-expanded") == "true")
        {
            await settings.ClickAsync();
            await Task.Delay(600);
        }

        await scene.StartCaptureAsync();
        var debugging = scene.Page.Locator("#left-nav a.item-label[href$='/Admin/Settings/debugging']").First;

        await scene.StepAsync("try-1", async () =>
        {
            await scene.ClickAsync(settings, pauseAfter: 400);
            await debugging.WaitForAsync();
            await scene.HighlightAsync(debugging, "Debugging");
        });

        await scene.StepAsync("try-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(debugging);
            var box = scene.Page.GetByLabel("Write shape debug information");
            await box.WaitForAsync();
            await scene.HighlightAsync(box, "Write shape debug information");
        });

        await scene.StepAsync("try-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByLabel("Write shape debug information"), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("button.save").First, "Save");
        });

        await scene.StepAsync("try-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
            var toast = scene.Page.Locator(".toast.message-success").First;
            await toast.WaitForAsync();
            await scene.HighlightAsync(toast);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByLabel("Write shape debug information"));
        });
    }
}

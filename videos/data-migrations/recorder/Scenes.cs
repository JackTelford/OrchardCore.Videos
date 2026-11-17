using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Data Migrations in Orchard Core": enabling the Forms feature runs its migration, which creates the
/// Form content type. The clip changes the site: restore the snapshot before a new take.
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

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Form", 40);
            await scene.Page.Locator("#list-alert").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#list-alert"));
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Features");
            await scene.Page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Forms", 40);
            await scene.Page.Locator("#btn-enable-OrchardCore_Forms").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#btn-enable-OrchardCore_Forms"));
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = scene.Page.Locator("#btn-enable-OrchardCore_Forms");
            await scene.MoveToAsync(enable);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await enable.ClickAsync();
                await scene.Page.GetByText("The feature Forms was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            }));
        });

        await scene.StepAsync("run-4", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/List");
            await scene.Page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Form", 40);
            var row = scene.Page.Locator("[data-type-name='Form']");
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });
    }
}

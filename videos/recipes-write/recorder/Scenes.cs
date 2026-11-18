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
        ("06-run", RunAsync),
    ];

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.OpenAsync("/Admin/Recipes");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "Bakery", delay: 90);
            await page.Locator("h4", new() { HasTextString = "Bakery Starter" }).First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("h4", new() { HasTextString = "Bakery Starter" }).First);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(page.Locator("#btn-run-BakeryPages"), pauseAfter: 900);
            await page.Locator("#confirmRemoveModal").WaitForAsync();
            await scene.PauseAsync((int)(Math.Max(1.0, durations["run-2"] - 3.5) * 1000));
            await scene.ClickAsync(page.Locator("#modalOkButton"), pauseAfter: 0);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.OffCameraAsync(async () => await page.GetByText("has been run successfully").First.WaitForAsync(new() { Timeout = 120000 }));
            await scene.HighlightAsync(page.Locator(".toast-body").Filter(new() { HasTextString = "has been run successfully" }).First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("run-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await page.GotoAsync(Scenes.BaseUrl + "/");
                await page.GetByText("Welcome to Greenfield Bakery").First.WaitForAsync(new() { Timeout = 120000 });
            });
        });
    }
}

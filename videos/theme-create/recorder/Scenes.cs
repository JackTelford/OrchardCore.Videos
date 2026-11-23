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
        ("05-apply", ApplyAsync),
    ];

    private static async Task ApplyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;

        ILocator Card() => page.Locator(".theme-card").Filter(new() { Has = page.Locator(".card-title", new() { HasTextString = "MyTheme" }) }).First;

        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("apply-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#left-nav button.nav-group-label").Filter(new() { HasTextString = "Design" }).First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav a[href$='/Admin/Themes']").First);
            await page.Locator(".theme-card").First.WaitForAsync();
        });

        await scene.StepAsync("apply-2", async () =>
        {
            await Card().ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(Card(), "MyTheme");
        });

        await scene.StepAsync("apply-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card().Locator("button", new() { HasTextString = "Enable" }).Last, "Enable");
            await scene.PauseAsync((int)(Math.Max(3.0, durations["apply-3"] - 6.0) * 1000));
            await scene.ClearHighlightsAsync();
            var make = Card().Locator("button", new() { HasTextString = "Make Current" }).First;
            await scene.HighlightAsync(make, "Make Current");
            await scene.MoveToAsync(make);
        });

        await scene.StepAsync("apply-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1300);
            await scene.ClickAndWaitForNavigationAsync(Card().Locator("button", new() { HasTextString = "Make Current" }).First);
            await page.Locator(".toast-body").First.WaitForAsync(new() { Timeout = 60000 });
            await scene.PauseAsync(5500);
            await Card().ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(Card().Locator(".card-footer").First);
        });

        await scene.StepAsync("apply-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        });
    }
}

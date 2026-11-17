using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Create a Module": the Greetings module (a manifest, a Startup class with the route Home/Index, a
/// controller and a view) is built into the host that the site runs from, outside the repository. 05-enable enables
/// its feature, 06-run opens its page. 05-enable changes the site: restore the snapshot before a new take.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string FeatureRow = "[data-filter-value*='Greetings.OrchardCore']";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("05-enable", EnableAsync),
        ("06-run", RunAsync),
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
        await field.PressSequentiallyAsync(text, new() { Delay = 40 });
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            var tools = scene.Page.Locator("#left-nav button.item-label").Filter(new() { HasTextRegex = new("^\\s*Tools\\s*$") }).First;

            if (await tools.GetAttributeAsync("aria-expanded") != "true")
            {
                await scene.ClickAsync(tools, pauseAfter: 500);
            }

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new("^\\s*Features\\s*$") }).First);
            await scene.Page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Greetings", 70);
            await scene.Page.Locator(FeatureRow).First.WaitForAsync();
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h3, h2", new() { HasTextString = "Samples" }).First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(FeatureRow).First);
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = scene.Page.Locator(FeatureRow).First.Locator("a[id^='btn-enable-']");
            await scene.MoveToAsync(enable);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await enable.ClickAsync();
                await scene.Page.GetByText("The feature Greetings was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            }));
        });

        await scene.StepAsync("enable-5", async () =>
        {
            await FastAsync(scene.Page.Locator("#search-box"), "Greetings");
        });

        await scene.StepAsync("enable-6", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(FeatureRow).First.Locator(".badge, [title]").Filter(new() { HasTextString = "Enabled" }).First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.PauseAsync(6500);
            await scene.NavigateAsync("/Home/Index");
        });

        await scene.StepAsync("run-2", async () =>
        {
            var heading = scene.Page.Locator("h1", new() { HasTextString = "Hello from the Greetings module" }).First;
            await heading.WaitForAsync();
            await scene.HighlightAsync(heading);
        });
    }
}

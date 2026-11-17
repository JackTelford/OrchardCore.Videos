using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "The Orchard Core Health Checks module". 02-enable and 05-compact are recorded in one run (the
/// second enables Redis, which has no configuration, so its check is unhealthy); 05-detailed needs the site
/// restarted with OrchardCore__HealthChecks__ShowDetails=true.
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
        ("02-enable", EnableAsync),
        ("05-compact", CompactAsync),
        ("05-detailed", DetailedAsync),
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

    private static async Task FastAsync(Scene scene, ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 3 });
    }

    // Enlarges the plain text of the endpoint, so it reads at video size, and rings it with the route as its caption.
    private static async Task ShowEndpointAsync(Scene scene, string waitFunction, string fontSize, string margin, string caption)
    {
        await scene.NavigateAsync("/health/live");
        await scene.Page.WaitForFunctionAsync(waitFunction, null, new() { Timeout = 60000 });
        await scene.Page.EvaluateAsync($"() => {{ const pre = document.querySelector('pre') ?? document.body; pre.style.fontSize = '{fontSize}'; pre.style.margin = '{margin}'; }}");
        await scene.HighlightAsync(scene.Page.Locator("pre, body").First, caption);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await FastAsync(scene, scene.Page.Locator("#search-box"), "Health Checks");
            await scene.Page.Locator("#btn-enable-OrchardCore_HealthChecks").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("[data-filter-value^='Health Checks OrchardCore.HealthChecks ']").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            var enable = scene.Page.Locator("#btn-enable-OrchardCore_HealthChecks");
            await scene.MoveToAsync(enable);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await enable.ClickAsync();
                await scene.Page.GetByText("The feature Health Checks was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            }));
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await ShowEndpointAsync(scene, "() => document.body.innerText.trim() === 'Healthy'", "48px", "64px", "/health/live");
        });
    }

    private static async Task CompactAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("fail-1", async () =>
        {
            await FastAsync(scene, scene.Page.Locator("#search-box"), "Redis");
            await scene.Page.Locator("#btn-enable-OrchardCore_Redis").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("[data-filter-value^='Redis OrchardCore.Redis ']").First, "Redis");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            var enable = scene.Page.Locator("#btn-enable-OrchardCore_Redis");
            await scene.MoveToAsync(enable);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await enable.ClickAsync();
                await scene.Page.GetByText("The feature Redis was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            }));
        });

        await scene.StepAsync("fail-2", async () =>
        {
            await ShowEndpointAsync(scene, "() => document.body.innerText.trim() === 'Unhealthy'", "48px", "64px", "/health/live");
        });
    }

    private static async Task DetailedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("detail-1", async () =>
        {
            await ShowEndpointAsync(scene, "() => /\"Status\":\\s*\"Unhealthy\"/.test(document.body.innerText)", "34px", "48px", "ShowDetails: true");
        });
    }
}

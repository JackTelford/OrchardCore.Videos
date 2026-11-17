using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Redis". The site is started with OrchardCore__Redis__Configuration pointing at a Redis in Docker, and with
/// OrchardCore__HealthChecks__ShowDetails. Record in one take, in order: 03-enable, then 04-health. No PrepareAsync.
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
        ("03-enable", EnableAsync),
        ("04-health", HealthAsync),
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

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static ILocator Row(Scene scene, string id) => scene.Page.Locator($"[data-filter-value*='{id}']").First;

    // Clicks something that reloads the page, with the wait cut out of the clip.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);
    }

    private static async Task SearchAsync(Scene scene, string text, string summary)
    {
        await scene.TypeAsync(scene.Page.Locator("#search-box").First, text, delay: 6);
        await scene.Page.Locator("#list-summary").GetByText(summary).First.WaitForAsync();
    }

    private static async Task OpenFeaturesAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        var tools = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact("Tools") }).First;

        if (await tools.GetAttributeAsync("aria-expanded") == "false")
        {
            await tools.ClickAsync();
        }
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenFeaturesAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await SearchAsync(scene, "OrchardCore.Redis", "Showing 5 of");
            await scene.HighlightAsync(scene.Page.Locator(".feature-group:not(.d-none) h3:text-is('Distributed')").First, "Distributed");
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".feature-group:not(.d-none) h3:text-is('Security')").First, "Security");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "OrchardCore.Redis.Cache").Locator(".direct-dependencies").First, "Depends on Redis");
            await scene.HoldAsync(2);
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync((int)Math.Max(0, (durations["enable-2"] - 5.5) * 1000));
            await NavAsync(scene, scene.Page.Locator("#btn-enable-OrchardCore_Redis_Cache").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await SearchAsync(scene, "OrchardCore.Redis", "Showing 5 of");
            await scene.PauseAsync(2500);
            await scene.ClickAsync(scene.Page.Locator("input[name='featureIds'][value='OrchardCore.Redis.Bus']").First, pauseAfter: 150);
            await scene.ClickAsync(scene.Page.Locator("input[name='featureIds'][value='OrchardCore.Redis.Lock']").First, pauseAfter: 150);
            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button").First, pauseAfter: 200);
            await NavAsync(scene, scene.Page.Locator("a[data-action='Enable']").First);
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await SearchAsync(scene, "OrchardCore.Redis", "Showing 5 of");
            await scene.HighlightAsync(Row(scene, "OrchardCore.Redis.DataProtection"), "Disabled");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task HealthAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenFeaturesAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("health-1", async () =>
        {
            await SearchAsync(scene, "OrchardCore.HealthChecks", "Showing 1 of");
            await NavAsync(scene, scene.Page.Locator("#btn-enable-OrchardCore_HealthChecks").First);
        });

        await scene.StepAsync("health-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/health/live");
                await scene.Page.WaitForFunctionAsync("() => document.body.innerText.includes('Redis Health Check')");
            }, leadMilliseconds: 100);
        });

        await scene.StepAsync("health-3");
    }
}

using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Mini Profiler". Set VIDEO_INIT_SCRIPT to a script outside the repository that hides ".mp-machine-name" (see
/// recording-plan.md). Record 02-enable first (it enables the feature), then 03-roles, 04-profile and 06-admin. No PrepareAsync.
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
        ("03-roles", RolesAsync),
        ("04-profile", ProfileAsync),
        ("06-admin", AdminAsync),
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

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    // Clicks something that loads another page, with the wait cut out of the clip.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/Admin/Features");
                await scene.Page.Locator("#search-box").WaitForAsync();
            }, leadMilliseconds: 100);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Mini Profiler", delay: 12);
            await scene.Page.Locator("#list-summary").GetByText("Showing 1 of").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".feature-group:not(.d-none) h3:text-is('Performance')").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("a[href*='/Features/OrchardCore.MiniProfiler/Enable']").First);
            await scene.Page.GetByText("The feature Mini Profiler was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            await scene.MoveToAsync(scene.Page.GetByText("The feature Mini Profiler was enabled.").First);
        });
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.Page.Locator("a.edit[href$='/Roles/Edit/Editor']").First.WaitForAsync();

        // The widget is docked over the right edge of admin pages; tuck it away so it does not cover the Allow column.
        var toggle = scene.Page.Locator("#oc-mp-toggle").First;
        await toggle.WaitForAsync(new() { Timeout = 30000 });

        if (await toggle.GetAttributeAsync("title") == "Hide MiniProfiler")
        {
            await toggle.ClickAsync();
            await scene.Page.WaitForTimeoutAsync(600);
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("roles-1", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("a.edit[href$='/Roles/Edit/Editor']").First);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Mini Profiler", delay: 30);
            await scene.Page.Locator("tr[data-text]:visible").Nth(1).WaitForAsync();
        });

        await scene.StepAsync("roles-2", async () =>
        {
            var front = scene.Page.Locator("tr[data-text='View Mini Profiler widget on front end pages']").First;
            var back = scene.Page.Locator("tr[data-text='View Mini Profiler widget on back end pages']").First;
            await ShowAsync(scene, front, 4.5);
            await scene.HighlightAsync(back);
            await scene.PauseAsync(4500);
            await scene.HighlightAsync(front);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProfileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.OpenAsync("/");
        var button = scene.Page.Locator("body > .mp-results .mp-button").First;
        await button.WaitForAsync(new() { Timeout = 30000 });
        await scene.StartCaptureAsync();

        var popup = scene.Page.Locator(".mp-results .mp-result").First.Locator(".mp-popup").First;

        await scene.StepAsync("profile-1", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/");
                await button.WaitForAsync(new() { Timeout = 30000 });
            }, leadMilliseconds: 100);
            await scene.HighlightAsync(button);
        });

        await scene.StepAsync("profile-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(button, pauseAfter: 400);
            await popup.WaitForAsync();
            await scene.HighlightAsync(popup.Locator("th").Filter(new() { HasText = "duration (ms)" }).First);
        });

        await scene.StepAsync("profile-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(popup.Locator("a.mp-toggle-trivial").Filter(new() { HasText = "show trivial" }).First, pauseAfter: 300);
            var shape = scene.Page.Locator("tr.oc-mp-shape").First;
            await shape.WaitForAsync();
            await shape.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(shape.Locator(".mp-label").First);
        });

        await scene.StepAsync("profile-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(popup.Locator("a.mp-queries-show").First, pauseAfter: 300);
            await scene.Page.Locator(".mp-overlay .mp-queries tr.oc-mp-query").First.WaitForAsync();
            await scene.Page.Locator(".mp-overlay .mp-queries").First.EvaluateAsync("e => e.scrollTop = 0");
            await scene.HighlightAsync(scene.Page.Locator(".query pre:visible").First);
        });
    }

    private static async Task AdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.EvaluateAsync("localStorage.removeItem('OrchardCore.MiniProfiler.Collapsed')");
        await scene.OpenAsync("/Admin/Features");
        var toggle = scene.Page.Locator("#oc-mp-toggle").First;
        await toggle.WaitForAsync(new() { Timeout = 30000 });
        await scene.StartCaptureAsync();

        await scene.StepAsync("admin-1", async () =>
        {
            await scene.MoveToAsync(toggle);
            await scene.ClickAsync(toggle, pauseAfter: 900);
            await scene.ClickAsync(toggle, pauseAfter: 500);
        });
    }
}

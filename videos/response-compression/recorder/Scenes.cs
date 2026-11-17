using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Response Compression". Only 02-enable is recorded; it enables the feature. No PrepareAsync.
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

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        var row = scene.Page.Locator("[data-filter-value*='OrchardCore.ResponseCompression']").First;

        await scene.StepAsync("enable-1", async () =>
        {
            var tools = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact("Tools") }).First;

            if (await tools.GetAttributeAsync("aria-expanded") == "false")
            {
                await scene.ClickAsync(tools, pauseAfter: 700);
            }

            var features = scene.Page.Locator("a[href$='/Admin/Features']").First;
            await scene.MoveToAsync(features);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(features), leadMilliseconds: 100);
            await scene.Page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Response Compression", delay: 12);
            await scene.Page.Locator("#list-summary").GetByText("Showing 1 of").First.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(row.Locator("code").First);
            await scene.HoldAsync(1.2);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator(".metadata .badge").First);
            await scene.HoldAsync(1.2);
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(800);
            var enable = row.Locator("a.btn-primary").First;
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => enable.ClickAsync()), leadMilliseconds: 150);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            var toast = scene.Page.Locator(".oc-notify-toast-container .toast").Filter(new() { HasText = "The feature Response Compression was enabled." });
            await toast.First.WaitForAsync(new() { Timeout = 60000 });
            await scene.HighlightAsync(toast.First);
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Response Compression", delay: 12);
            await scene.Page.Locator("#list-summary").GetByText("Showing 1 of").First.WaitForAsync();
            await scene.HighlightAsync(row.Locator(".metadata .badge").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("a.btn-danger").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

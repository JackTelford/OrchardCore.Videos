using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Data Protection Keys". Only 03-enable is recorded; it enables Distributed Data Protection (Azure Storage) on a site
/// with no Azure or Redis configuration. No PrepareAsync.
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

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        var azure = scene.Page.Locator("[data-filter-value*='OrchardCore.DataProtection.Azure']").First;
        var redis = scene.Page.Locator("[data-filter-value*='OrchardCore.Redis.DataProtection']").First;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "data protection", delay: 30);
            await scene.Page.Locator("#list-summary").GetByText("Showing 2 of").First.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(azure.Locator("code").First, "Identifier");
            await scene.HoldAsync(2);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(redis.Locator(".direct-dependencies .badge").First, "Depends on Redis");
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = scene.Page.Locator("#btn-enable-OrchardCore_DataProtection_Azure").First;
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => enable.ClickAsync()), leadMilliseconds: 150);
            var message = scene.Page.GetByText("The feature Distributed Data Protection (Azure Storage) was enabled.").First;
            await message.WaitForAsync(new() { Timeout = 60000 });
            await scene.MoveToAsync(message);
        });

        await scene.ClearHighlightsAsync();
    }
}

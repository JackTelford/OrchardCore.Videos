using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clip of the configuration-appsettings video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("04-list", ListAsync),
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

    // Moves to a link or button, clicks it and cuts the page load out of the video.
    private static async Task GoAsync(Scene scene, ILocator link, ILocator ready = null)
    {
        await scene.MoveToAsync(link);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(async () => await link.ClickAsync());

            if (ready is not null)
            {
                await ready.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Visible });
            }
        }, 450);
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        var link = page.Locator("#left-nav a[href$='/Admin/Tenants']").First;

        if (await link.IsVisibleAsync())
        {
            await page.Locator("#left-nav #multitenancy button").First.ClickAsync();
            await Task.Delay(500);
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            if (!await link.IsVisibleAsync())
            {
                await scene.ClickAsync(page.Locator("#left-nav #multitenancy button").First, pauseAfter: 250);
            }

            var row = page.Locator(".list-group-item").Filter(new() { HasText = "MyTenant" }).First;
            await GoAsync(scene, link, row);
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a#btn-setup-MyTenant"), page.Locator("#SubmitButton"));
            await scene.HoldAsync(Math.Max(1.0, durations["list-2"] * 0.30));
            await scene.SelectAsync(page.Locator("#SiteTimeZone"), "America/Los_Angeles");
            await scene.ScrollAsync(0, 300);
        });
    }
}

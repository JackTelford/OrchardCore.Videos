using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Reverse Proxy Settings". Record in one take, in order: 02-enable (enables Reverse Proxy Configuration),
/// then 03-settings (saves the three headers). No PrepareAsync.
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
        ("03-settings", SettingsAsync),
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

    private static async Task<Scene> OpenDashboardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();

        foreach (var name in new[] { "Tools", "Settings" })
        {
            var group = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = new Regex($@"^\s*{name}\s*$") }).First;

            if (await group.GetAttributeAsync("aria-expanded") == "true")
            {
                await group.EvaluateAsync("e => e.click()");
                await scene.Page.WaitForTimeoutAsync(400);
            }
        }

        await scene.StartCaptureAsync();
        return scene;
    }

    // Clicks something that loads another page, with the wait cut out of the clip.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);
    }

    private static ILocator Group(Scene scene, string name) => scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = new Regex($@"^\s*{name}\s*$") }).First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations);
        var enable = scene.Page.Locator("a[href*='OrchardCore.ReverseProxy/Enable']").First;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.PauseAsync(1800);
            await scene.ClickAsync(Group(scene, "Tools"), pauseAfter: 700);
            var features = scene.Page.Locator("#left-nav a.item-label[href$='/Admin/Features']").First;
            await scene.MoveToAsync(features);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(features), leadMilliseconds: 100);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "reverse", delay: 60);
            await scene.Page.Locator("#list-summary").GetByText("Showing 1 of").First.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h3:text-is('Infrastructure')").First);
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(enable);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => enable.ClickAsync()), leadMilliseconds: 150);
            await scene.Page.Locator(".toast.message-success:has-text('The feature Reverse Proxy Configuration was enabled.')").First.WaitForAsync(new() { Timeout = 60000 });
            await scene.PauseAsync(1500);
            await scene.ClickAsync(Group(scene, "Settings"), pauseAfter: 600);
            await scene.HighlightAsync(scene.Page.Locator("#left-nav a.item-label:has-text('Reverse Proxy')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations);

        ILocator Option(string header) => scene.Page.Locator($".form-check:has(input[name$='EnableXForwarded{header}'])").First;
        ILocator Box(string header) => scene.Page.Locator($"input[type='checkbox'][name$='EnableXForwarded{header}']").First;
        ILocator Value(int index) => scene.Page.Locator(".hint:has-text('Current request value')").Nth(index);

        await scene.StepAsync("settings-1", async () =>
        {
            await scene.ClickAsync(Group(scene, "Settings"), pauseAfter: 600);
            var link = scene.Page.Locator("#left-nav a.item-label:has-text('Reverse Proxy')").First;
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), leadMilliseconds: 100);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator(".alert.alert-warning").First);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.ClearHighlightsAsync();

            foreach (var header in new[] { "For", "Proto", "Host" })
            {
                await scene.HighlightAsync(Option(header));
                await scene.PauseAsync(1400);
                await scene.ClearHighlightsAsync();
            }

            await scene.HighlightAsync(Option("For"));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("settings-3", async () =>
        {
            for (var i = 0; i < 3; i++)
            {
                await scene.HighlightAsync(Value(i));
                await scene.PauseAsync(3000);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("settings-4", async () =>
        {
            foreach (var header in new[] { "For", "Proto", "Host" })
            {
                await scene.ClickAsync(Box(header), pauseAfter: 600);
            }
        });

        await scene.StepAsync("settings-5", async () =>
        {
            var save = scene.Page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => save.ClickAsync()), leadMilliseconds: 150);
            await scene.Page.Locator(".toast.message-success:has-text('Site settings updated successfully.')").First.WaitForAsync(new() { Timeout = 60000 });
        });
    }
}

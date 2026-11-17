using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "SignalR". The site is started with OrchardCore__Redis__Configuration pointing at a Redis in Docker.
/// PrepareAsync enables Media SignalR (and SignalR with it). Record 02-features, then 03-live.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string Connected = "SignalR connected to MediaHub";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-live", LiveAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/Features", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#search-box").FillAsync("media signalr");
        await page.Locator("#btn-enable-OrchardCore_Media_SignalR").ClickAsync();
        await page.GetByText("The feature Media SignalR was enabled.").First.WaitForAsync(new() { Timeout = 90000 });
        Console.WriteLine("Enabled Media SignalR.");
        await context.CloseAsync();
    }

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

    private static ILocator Row(Scene scene, string featureId)
        => scene.Page.Locator(".list-group-item")
            .Filter(new() { Has = scene.Page.Locator($"#btn-enable-{featureId}, #btn-disable-{featureId}") }).First;

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "signalr", delay: 60);
            await scene.Page.Locator("#list-summary").GetByText("Showing 4 of").First.WaitForAsync();

            foreach (var id in new[] { "OrchardCore_SignalR", "OrchardCore_SignalR_Redis", "OrchardCore_SignalR_Azure", "OrchardCore_Media_SignalR" })
            {
                await scene.MoveToAsync(Row(scene, id));
                await scene.PauseAsync(700);
            }
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.HighlightAsync(Row(scene, "OrchardCore_SignalR"), "No pages of its own");
            await scene.HoldAsync(3);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "OrchardCore_SignalR_Azure").Locator(".direct-dependencies").First, "Depends on SignalR");
            await scene.HoldAsync(3);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "OrchardCore_SignalR_Redis").Locator(".direct-dependencies").First, "Depends on Redis and SignalR");
        });

        await scene.StepAsync("features-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = scene.Page.Locator("#btn-enable-OrchardCore_SignalR_Redis").First;
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => enable.ClickAsync()), leadMilliseconds: 150);
            await scene.Page.GetByText("The feature SignalR Redis Backplane was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "redis", delay: 30);
            await scene.HighlightAsync(Row(scene, "OrchardCore_Redis"), "Enabled with it");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var first = scene.Page;

        await first.RunAndWaitForConsoleMessageAsync(
            () => scene.OpenAsync("/Admin/Media"),
            new() { Predicate = message => message.Text.Contains(Connected), Timeout = 20_000 });
        await first.Locator(".file-item").First.WaitForAsync();

        var second = await scene.Context.NewPageAsync();
        await second.RunAndWaitForConsoleMessageAsync(
            () => second.GotoAsync(scene.BaseUrl + "/Admin/Media", new() { WaitUntil = WaitUntilState.NetworkIdle }),
            new() { Predicate = message => message.Text.Contains(Connected), Timeout = 20_000 });
        await second.Locator(".file-item").First.WaitForAsync();
        await scene.PauseAsync(1000);

        // The tab that is captured must be the first one when the capture starts.
        await first.BringToFrontAsync();
        await scene.StartCaptureAsync();

        ILocator Post(IPage page) => page.Locator(".file-item").Filter(new() { HasText = "post-bg.jpg" });

        await scene.StepAsync("live-1", async () =>
        {
            await scene.HighlightAsync(Post(first), "First tab");
            await scene.HoldAsync(2);
            await scene.ClearHighlightsAsync();
            await scene.SwitchToAsync(second);
            await scene.HighlightAsync(Post(second), "Second tab");
        });

        await scene.StepAsync("live-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Post(second).Locator("a.action-button").First, pauseAfter: 400);
            await scene.ClickAsync(scene.Page.Locator("#overlay_file_menu").GetByText("Rename", new() { Exact = true }).First, pauseAfter: 500);
            var field = scene.Page.Locator("input[name=rename]").First;
            await field.WaitForAsync();
            await field.FillAsync("");
            await scene.TypeAsync(field, "post-header.jpg", delay: 40);
            await scene.ClickAsync(scene.Page.Locator("#btn-submit").First, pauseAfter: 300);
            await second.Locator(".file-item").Filter(new() { HasText = "post-header.jpg" }).First.WaitForAsync(new() { Timeout = 10000 });
        });

        await scene.StepAsync("live-3", async () =>
        {
            await scene.SwitchToAsync(first);
            var renamed = first.Locator(".file-item").Filter(new() { HasText = "post-header.jpg" });
            await renamed.First.WaitForAsync(new() { Timeout = 10000 });
            await scene.HighlightAsync(renamed.First, "Updated without a reload");
        });
    }
}

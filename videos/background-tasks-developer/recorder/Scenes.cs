using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recording of "Background tasks for developers". PrepareAsync is not needed: the site is prepared by enabling Background Tasks
/// before the capture (the tool script does it with a JSON import). Single clip: 05-admin.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string TaskName = "OrchardCore.Media.Services.ResizedMediaCacheBackgroundTask";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("05-admin", AdminAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var json = JsonSerializer.Serialize(new { steps = new object[] { new { name = "feature", enable = new[] { "OrchardCore.BackgroundTasks" } } } });
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();

        for (var i = 0; i < 24; i++)
        {
            await page.GotoAsync(BaseUrl + "/Admin/BackgroundTasks", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            if (await page.Locator($"[data-task-name='{TaskName}'] [data-next-run]").CountAsync() > 0)
            {
                Console.WriteLine("Background Tasks enabled and loaded.");
                break;
            }

            await Task_Delay(15000);
        }

        await context.CloseAsync();
    }

    private static Task Task_Delay(int ms) => Task.Delay(ms);

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task AdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/BackgroundTasks");
        var row = scene.Page.Locator($"[data-task-name='{TaskName}']").First;
        await row.WaitForAsync();
        var tools = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact("Tools") }).First;

        if (await tools.GetAttributeAsync("aria-expanded") == "false")
        {
            await tools.ClickAsync();
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("admin-1", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("a[href$='/Admin/BackgroundTasks']").First, 1.5);
            await ShowAsync(scene, row.Locator("a").Filter(new() { HasTextString = "Resized Media Cache Cleaner" }).First, 1.5);
            await ShowAsync(scene, row.Locator(".admin-list-secondary").First, 1.5);
            await scene.HighlightAsync(row.Locator("[data-next-run]").First);
        });

        await scene.StepAsync("admin-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var link = row.Locator("a").Filter(new() { HasTextString = "Resized Media Cache Cleaner" }).First;
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), leadMilliseconds: 150);
            var settings = scene.Page.Locator(".card:has(.card-header:text-is('Settings'))").First;
            await ShowAsync(scene, settings.Locator("dt:text-is('Technical name') + dd").First, 1.5);
            await scene.HighlightAsync(settings.Locator("dt:text-is('Schedule') + dd").First);
        });

        await scene.StepAsync("admin-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button[data-task-actions]").First, pauseAfter: 400);
            var run = scene.Page.Locator("a[data-run-task]").First;
            await scene.MoveToAsync(run);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => run.ClickAsync()), leadMilliseconds: 150);
            await scene.Page.Locator("dd[data-last-run] [data-run-result='Succeeded']").First.WaitForAsync(new() { Timeout = 20000 });
            await ShowAsync(scene, scene.Page.Locator("dd[data-last-run]").First, 1.2);
            await ShowAsync(scene, scene.Page.Locator("dt:text-is('Last run duration') + dd").First, 1.2);
            await scene.HighlightAsync(scene.Page.Locator("dt:text-is('Last run triggered by') + dd").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

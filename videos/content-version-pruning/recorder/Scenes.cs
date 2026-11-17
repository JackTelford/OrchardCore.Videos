using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Content Version Pruning". The clips depend on each other and change the site: 02-features enables
/// two features, 03-settings saves the pruning settings, 04-task enables the task and runs it once. Before 04-task
/// the background service must have loaded the task (WaitForTaskAsync, off camera).
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string TaskName = "OrchardCore.Contents.VersionPruning.Tasks.ContentVersionPruningBackgroundTask";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-settings", SettingsAsync),
        ("04-task", TaskAsync),
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
        await field.PressSequentiallyAsync(text, new() { Delay = 3 });
    }

    private static async Task OpenGroupAsync(Scene scene, string group)
    {
        var button = scene.Page.Locator("#left-nav button.item-label").Filter(new() { HasTextRegex = new($"^\\s*{group}\\s*$") }).First;

        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(button, pauseAfter: 400);
        }
    }

    private static async Task GoAsync(Scene scene, string group, string link)
    {
        await OpenGroupAsync(scene, group);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new($"^\\s*{link}\\s*$") }).First);
    }

    // Enables a feature found by its search text; the wait for the tenant to restart is cut off camera.
    private static async Task EnableFeatureAsync(Scene scene, ILocator enable, string message, string search)
    {
        await scene.MoveToAsync(enable);
        await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
        {
            await enable.ClickAsync();
            await scene.Page.GetByText(message).First.WaitForAsync(new() { Timeout = 60000 });
        }));
        await FastAsync(scene.Page.Locator("#search-box"), search);
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await GoAsync(scene, "Tools", "Features");
            await scene.Page.Locator("#search-box").WaitForAsync();
            await FastAsync(scene.Page.Locator("#search-box"), "pruning");
        });

        await scene.StepAsync("features-2", async () =>
        {
            await EnableFeatureAsync(scene, scene.Page.Locator("a[id^='btn-enable-']:visible").First, "The feature Content Version Pruning was enabled.", "pruning");
        });

        await scene.StepAsync("features-3", async () =>
        {
            await scene.Page.Locator("#search-box").FillAsync("");
            await FastAsync(scene.Page.Locator("#search-box"), "Background Tasks");
            var row = scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.GetByText("Background Tasks", new() { Exact = true }) }).First;
            await EnableFeatureAsync(scene, row.Locator("a[id^='btn-enable-']").First, "The feature Background Tasks was enabled.", "Background Tasks");
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await GoAsync(scene, "Settings", "Content Version Pruning");
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByLabel("Disable", new() { Exact = true }));
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByLabel("Retention period (days)"));
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByLabel("Archived versions to keep per item"));
        });

        await scene.StepAsync("settings-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("input[type=checkbox][value='BlogPost']"), pauseAfter: 400);
            await scene.ClickAsync(scene.Page.Locator("input[type=checkbox][value='Page']"), pauseAfter: 400);
        });

        await scene.StepAsync("settings-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
        });
    }

    private static async Task TaskAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // Off camera: the background service must have loaded the task before the clip starts.
        await scene.OpenAsync("/Admin/BackgroundTasks");

        for (var i = 0; i < 16; i++)
        {
            if (await scene.Page.Locator($"[data-task-name='{TaskName}'][data-task-status='Idle']").CountAsync() > 0)
            {
                break;
            }

            await Task.Delay(15_000);
            await scene.Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var row = scene.Page.Locator($"[data-task-name='{TaskName}']");

        await scene.StepAsync("task-1", async () =>
        {
            await GoAsync(scene, "Tools", "Background Tasks");
            await row.WaitForAsync();
            await scene.HighlightAsync(row.Locator(".badge").Filter(new() { HasText = "Disabled" }).First);
        });

        await scene.StepAsync("task-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(row.GetByRole(AriaRole.Link, new() { Name = "Enable", Exact = true }));
            await scene.HighlightAsync(row.Locator("[data-next-run]").First);
            await scene.HighlightAsync(row.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }));
        });

        await scene.StepAsync("task-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(row.GetByRole(AriaRole.Button, new() { Name = "Actions", Exact = true }), pauseAfter: 400);
            var runNow = row.GetByRole(AriaRole.Link, new() { Name = "Run now", Exact = true });
            await scene.HighlightAsync(runNow);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(runNow);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await runNow.ClickAsync();
                await row.Locator("[data-last-run][data-run-result='Succeeded']").First.WaitForAsync(new() { Timeout = 120000 });
            }));
            await scene.HighlightAsync(row.Locator("[data-last-run]").First);
        });

        await scene.StepAsync("task-4");
    }
}

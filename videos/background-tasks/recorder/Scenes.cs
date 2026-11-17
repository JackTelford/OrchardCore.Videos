using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Background tasks". PrepareAsync enables Publish Later and Content Version Pruning (outside the capture).
/// Record in one take, in order, after the site has run at least 5 minutes since its first request: 02-enable first, then wait.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string Chunk = "OrchardCore.Media.Services.ChunkFileUploadBackgroundTask";
    private const string Pruning = "OrchardCore.Contents.VersionPruning.Tasks.ContentVersionPruningBackgroundTask";
    private const string Remote = "OrchardCore.Media.Services.RemoteMediaCacheBackgroundTask";
    private const string Resized = "OrchardCore.Media.Services.ResizedMediaCacheBackgroundTask";
    private const string Publisher = "OrchardCore.PublishLater.Services.ScheduledPublishingBackgroundTask";
    private const string Toast = ".oc-notify-toast-container .toast.message-success";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-list", ListAsync),
        ("04-details", DetailsAsync),
        ("05-schedule", ScheduleAsync),
        ("06-run", RunAsync),
        ("07-toggle", ToggleAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var json = JsonSerializer.Serialize(new { steps = new object[] { new { name = "feature", enable = new[] { "OrchardCore.PublishLater", "OrchardCore.Contents.VersionPruning" } } } });
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Enabled Publish Later and Content Version Pruning.");
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

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static ILocator Row(Scene scene, string name) => scene.Page.Locator($"[data-task-name='{name}']").First;

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, string caption = null, bool clear = true)
    {
        await scene.HighlightAsync(target, caption);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    // A click that loads a page, with the cursor going to the target on camera; the toast, when there is one, keeps the cursor on it.
    private static async Task NavAsync(Scene scene, ILocator target, bool keepToast = false)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);

        if (keepToast)
        {
            await scene.Page.Locator(Toast).First.WaitForAsync();
            await scene.MoveToAsync(scene.Page.Locator(Toast).First);
        }
    }

    private static async Task OpenListAsync(Scene scene, string layout = "List")
    {
        await scene.OpenAsync($"/Admin/BackgroundTasks?layout={layout}");
        await Row(scene, Publisher).WaitForAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        var tools = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact("Tools") }).First;

        if (await tools.GetAttributeAsync("aria-expanded") == "false")
        {
            await tools.ClickAsync();
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = Exact("Features") }).First);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Background Tasks", delay: 30);
            var heading = scene.Page.Locator(".feature-group h3:text-is('Tasks')").First;
            await heading.WaitForAsync();
            await ShowAsync(scene, heading, 1, "Tasks");
            await NavAsync(scene, scene.Page.Locator("a#btn-enable-OrchardCore_BackgroundTasks").First, keepToast: true);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator(Toast).First, 1.5, "Feature enabled");
            var link = scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = Exact("Background Tasks") }).First;
            await ShowAsync(scene, link, 1, "Tools, Background Tasks", clear: false);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, link);
            await Row(scene, Publisher).WaitForAsync();
        });
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenListAsync(scene);

        // Start just after a run of the Publisher, so its row is not replaced during the highlights.
        var first = await Row(scene, Publisher).Locator("[data-last-run]").First.GetAttributeAsync("title");

        for (var i = 0; i < 40; i++)
        {
            await Task.Delay(2000);
            await scene.Page.ReloadAsync();
            await Row(scene, Publisher).WaitForAsync();

            if (await Row(scene, Publisher).Locator("[data-last-run]").First.GetAttributeAsync("title") != first)
            {
                break;
            }
        }

        await scene.StartCaptureAsync();
        var publisher = Row(scene, Publisher);

        await scene.StepAsync("list-1", async () =>
        {
            await ShowAsync(scene, publisher.Locator("a:text-is('Scheduled Content Items Publisher')").First, 1.2, "Title");
            await ShowAsync(scene, publisher.Locator(".admin-list-secondary").First, 1.2, "Description");
            await ShowAsync(scene, publisher.Locator(".badge:has-text('Enabled')").First, 1.2, "Enabled");
            await ShowAsync(scene, publisher.Locator("[data-last-run]").First, 1.2, "Last run");
            await scene.HighlightAsync(publisher.Locator("[data-next-run]").First, "Next run");
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, Row(scene, Publisher).Locator("[data-last-run]").First, 2);
            await scene.HighlightAsync(Row(scene, Chunk).Locator("[data-last-run]").First, "never");
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, scene.Page.Locator(".admin-list-layout-selector").First, 1.5, "List and Grid");
            await NavAsync(scene, scene.Page.Locator(".admin-list-layout-selector a[title='Grid']").First);
            await ShowAsync(scene, scene.Page.Locator("[role='columnheader']:text-is('Last run')").First, 1);
            await scene.HighlightAsync(scene.Page.Locator("[role='columnheader']:text-is('Next run')").First);
        });

        await scene.StepAsync("list-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "cache", delay: 40);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter")), leadMilliseconds: 100);
            var status = scene.Page.Locator(".filter .bootstrap-select button").First;
            await scene.ClickAsync(status);
            await scene.HoldAsync(2.5);
            await scene.ClickAsync(scene.Page.Locator(".bootstrap-select .dropdown-menu.show a").Filter(new() { HasTextString = "Enabled" }).First, pauseAfter: 300);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DetailsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenListAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("details-1", async () =>
        {
            await NavAsync(scene, Row(scene, Publisher).Locator("a:text-is('Scheduled Content Items Publisher')").First);
            await scene.HighlightAsync(scene.Page.Locator(".card:has(.card-header:text-is('Execution'))").First, "Execution");
        });

        await scene.StepAsync("details-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, scene.Page.Locator(".card:has(.card-header:text-is('Settings'))").First, 2, "Settings");
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(scene.Page.Locator(".card:has(.card-header:text-is('Recent runs'))").First, "Recent runs");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ScheduleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenListAsync(scene);
        var nextBefore = await Row(scene, Resized).Locator("[data-next-run]").First.GetAttributeAsync("title");
        await scene.StartCaptureAsync();

        await scene.StepAsync("schedule-1", async () =>
        {
            await NavAsync(scene, Row(scene, Resized).Locator("a.btn:text-is('Edit')").First);
            await ShowAsync(scene, scene.Page.Locator("#Title").First, 1, "Read-only");
            await ShowAsync(scene, scene.Page.Locator("#Description").First, 1);
            await scene.HighlightAsync(scene.Page.Locator("#Schedule").First, "Schedule");
        });

        await scene.StepAsync("schedule-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var schedule = scene.Page.Locator("#Schedule").First;
            await schedule.FillAsync(string.Empty);
            await scene.TypeAsync(schedule, "every night", delay: 25);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Locator("button.save").First.ClickAsync()), leadMilliseconds: 150);
            await scene.HighlightAsync(scene.Page.Locator("span[data-valmsg-for='Schedule']").First, "Rejected");
        });

        await scene.StepAsync("schedule-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#use-default-schedule").First);
            await scene.HoldAsync(1);
            var schedule = scene.Page.Locator("#Schedule").First;
            await schedule.FillAsync(string.Empty);
            await scene.TypeAsync(schedule, "*/10 * * * *", delay: 25);
            await NavAsync(scene, scene.Page.Locator("button.save").First, keepToast: true);
        });

        await scene.StepAsync("schedule-4", async () =>
        {
            await scene.HighlightAsync(Row(scene, Resized).Locator("[data-next-run]").First, "Next run");
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 40; i++)
                {
                    await scene.Page.GotoAsync(BaseUrl + "/Admin/BackgroundTasks");
                    await Row(scene, Resized).WaitForAsync();

                    if (await Row(scene, Resized).Locator("[data-next-run]").First.GetAttributeAsync("title") != nextBefore)
                    {
                        break;
                    }

                    await Task.Delay(5000);
                }
            });
            await scene.HighlightAsync(Row(scene, Resized).Locator("[data-next-run]").First, "Next run");
        });

        await scene.StepAsync("schedule-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, Row(scene, Resized).Locator("a.btn:text-is('Edit')").First);
            await ShowAsync(scene, scene.Page.Locator("label[for='UsePipeline']").First, 1.5, "Use Tenant Pipeline");
            await scene.ClickAsync(scene.Page.Locator("#advanced-tab").First);
            await scene.HighlightAsync(scene.Page.Locator("#LockTimeout").First, "Lock timeout (ms)");
            await scene.HighlightAsync(scene.Page.Locator("#LockExpiration").First, "Lock expiration (ms)");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenListAsync(scene);

        // Start just after a run of the Publisher, so the click on Run now comes less than 10 seconds after the last check.
        var first = await Row(scene, Publisher).Locator("[data-last-run]").First.GetAttributeAsync("title");

        for (var i = 0; i < 40; i++)
        {
            await Task.Delay(2000);
            await scene.Page.ReloadAsync();
            await Row(scene, Publisher).WaitForAsync();

            if (await Row(scene, Publisher).Locator("[data-last-run]").First.GetAttributeAsync("title") != first)
            {
                break;
            }
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            var row = Row(scene, Chunk);
            await scene.ClickAsync(row.Locator("button.actions").First, pauseAfter: 200);
            await ShowAsync(scene, row.Locator("a[data-run-task]").First, 0.5, "Run now", clear: false);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, row.Locator("a[data-run-task]").First, keepToast: true);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await Row(scene, Chunk).Locator("[data-last-run][data-run-result='Succeeded']").First.WaitForAsync(new() { Timeout = 20000 });
            await scene.HighlightAsync(Row(scene, Chunk).Locator("[data-last-run]").First, "Last run");
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = Row(scene, Pruning);
            await scene.ClickAsync(row.Locator("button.actions").First, pauseAfter: 200);
            await ShowAsync(scene, row.Locator("a[data-run-task]").First, 1, "Run now", clear: false);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, row.Locator("a[data-run-task]").First, keepToast: true);
            await Row(scene, Pruning).Locator("[data-last-run][data-run-result='Succeeded']").First.WaitForAsync(new() { Timeout = 30000 });
            await ShowAsync(scene, Row(scene, Pruning).Locator(".badge:has-text('Disabled')").First, 1, "Disabled");
            await scene.HighlightAsync(Row(scene, Pruning).Locator("[data-last-run]").First, "Last run");
        });

        await scene.StepAsync("run-4", async () =>
        {
            await scene.ClearHighlightsAsync();

            foreach (var name in new[] { Remote, Resized })
            {
                await scene.ClickAsync(scene.Page.Locator($"input[name='taskNames'][value='{name}']").First, pauseAfter: 150);
            }

            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button").First, pauseAfter: 200);
            await ShowAsync(scene, scene.Page.Locator("a.dropdown-item[data-action='Run']").First, 1, "Run now", clear: false);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("a.dropdown-item[data-action='Run']").First);
            await scene.Page.Locator("#modalOkButton").WaitForAsync();
            await scene.HoldAsync(1.2);
            await NavAsync(scene, scene.Page.Locator("#modalOkButton").First, keepToast: true);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ToggleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenListAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("toggle-1", async () =>
        {
            var disable = Row(scene, Remote).Locator("a.btn-danger:text-is('Disable')").First;
            await ShowAsync(scene, disable, 0.8, "Disable");
            await NavAsync(scene, disable, keepToast: true);
            await ShowAsync(scene, Row(scene, Remote).Locator(".badge:has-text('Disabled')").First, 1, "Disabled");
            await scene.HighlightAsync(Row(scene, Remote).Locator("[data-next-run]").First, "Next run");
        });

        await scene.StepAsync("toggle-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, Row(scene, Remote).Locator("a.btn-success:text-is('Enable')").First, keepToast: true);
            await ShowAsync(scene, Row(scene, Remote).Locator(".badge:has-text('Enabled')").First, 1);
            await scene.HighlightAsync(Row(scene, Remote).Locator("[data-next-run]").First, "Next run");
        });

        await scene.ClearHighlightsAsync();
    }
}

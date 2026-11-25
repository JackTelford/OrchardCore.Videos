using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Workflows: timers", recorded in order on one fresh Blog site that has the Workflows feature enabled.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string WorkflowName = "Scheduled check";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-create", CreateAsync),
        ("04-run", RunAsync),
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

    /// <summary>Enables the Workflows feature, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#btn-enable-OrchardCore_Workflows").ClickAsync();
        await WaitTextAsync(page, "was enabled");

        foreach (var path in new[] { "/Admin/Features", "/Admin/Workflows/Types", "/Admin/Workflows/Types/Create" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Workflows enabled.");
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task WaitTextAsync(IPage page, string text, int timeout = 60_000)
        => await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = timeout });

    // Clicks a link or button that loads a new page; the load is cut out of the video (see Scene.OffCameraAsync).
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
    }

    private static ILocator Group(IPage page, string key) => page.Locator($"button.nav-group-label.oc-navigation-admin-{key}");

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1500)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static async Task OpenDesignerAsync(Scene scene)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await page.GetByRole(AriaRole.Link, new() { Name = WorkflowName }).First.ClickAsync();
        await page.Locator("[data-cy='start-picker'], .wfd-node").First.WaitForAsync();
        await page.Locator("[data-cy='panel-tab-issues']").ClickAsync();
        await page.Locator("button.leftbar-compactor").ClickAsync();
        await Task.Delay(600);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(Group(page, "tools"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Features'] >> visible=true").First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "timer", delay: 40);
            await page.Locator("#btn-enable-OrchardCore_Workflows_Timers").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("#btn-enable-OrchardCore_Workflows_Timers"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_Workflows_Timers"));
            await WaitTextAsync(page, "was enabled");
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(Group(page, "design"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Workflows/Types'] >> visible=true").First);
            await scene.MoveToAsync(page.Locator("a.btn.create").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await GoAsync(scene, page.Locator("a.btn.create").First);
            await scene.TypeAsync(page.Locator("input#Name"), WorkflowName, delay: 25);
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Create and open the designer" }));
            await page.Locator("[data-cy='start-picker']").WaitForAsync();
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='start-picker'] button").Filter(new() { HasTextString = "Timer Event" }).First);
            await page.Locator("[data-cy='start-badge']").WaitForAsync();
        });

        await scene.StepAsync("create-4", async () =>
        {
            var cron = page.GetByLabel("CRON Expression");
            await cron.WaitForAsync();
            await scene.ClickAsync(cron, pauseAfter: 100);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.PressAsync("Delete");
            await scene.TypeAsync(cron, "* * * * *", delay: 80);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("create-5", async () =>
        {
            var timeZone = page.GetByLabel("Use site time zone");
            await scene.ClickAsync(timeZone, pauseAfter: 300);
            await scene.HighlightAsync(timeZone);
        });

        await scene.StepAsync("create-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.Locator("button[aria-label='Close the activity panel']").First.ClickIfVisibleAsync();
            var done = page.Locator(".wfd-port:not(.is-connected)[data-cy$='-Done']").First;
            await scene.MoveToAsync(done);
            await scene.PauseAsync(600);
            await scene.ClickAsync(done, pauseAfter: 150);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "Log", delay: 40);
            await scene.ClickAsync(page.Locator("[data-cy='quick-add-LogTask']"), pauseAfter: 200);
            await page.Locator("[data-cy='panel-activity-form']").WaitForAsync();
        });

        await scene.StepAsync("create-7", async () =>
        {
            var text = page.GetByLabel("Text", new() { Exact = true });
            await scene.ClickAsync(text, pauseAfter: 100);
            await scene.TypeAsync(text, "The scheduled check ran.", delay: 25);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("create-8", async () =>
        {
            await page.WaitForFunctionAsync("() => document.body.innerText.includes('Saved')");
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-publish']"));
            await WaitTextAsync(page, "The workflow was published as version");
        });
    }

    private static async Task<int> FinishedCountAsync(IPage page)
    {
        var text = await page.Locator("[data-cy='instance-count-Finished']").InnerTextAsync();
        return int.Parse(new string(text.Where(char.IsDigit).ToArray()));
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // The timers are checked once a minute: wait, outside the capture, until the workflow has run at least once.
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var probe = await context.NewPageAsync();
        probe.SetDefaultTimeout(30_000);
        var deadline = DateTime.UtcNow.AddMinutes(6);

        while (DateTime.UtcNow < deadline)
        {
            await probe.GotoAsync(BaseUrl + "/Admin/Workflows/Instances");

            if (await probe.Locator("[data-cy='instance-count-Finished']").CountAsync() > 0 && await FinishedCountAsync(probe) >= 1)
            {
                break;
            }

            await Task.Delay(5000);
        }

        await context.CloseAsync();

        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("a.badge:has-text('Instances')").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.badge:has-text('Instances')").First);
            await scene.HighlightAsync(page.Locator("[data-cy='instance-count-Halted']"));
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var before = await FinishedCountAsync(page);
            await scene.HighlightAsync(page.Locator("[data-cy='instance-count-Finished']"));

            // The next run comes due within a minute: reload, off camera, until the Finished count has gone up.
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 40; i++)
                {
                    await scene.WaitForNavigationAsync(() => page.ReloadAsync());

                    if (await FinishedCountAsync(page) > before)
                    {
                        break;
                    }

                    await Task.Delay(4000);
                }
            }, leadMilliseconds: 1500);

            await scene.HighlightAsync(page.Locator("[data-cy='instance-count-Finished']"));
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("[data-cy='instance-count-Finished']"));
            await GoAsync(scene, page.Locator("a[href*='/Workflow/Details/']").First);
            await page.Locator(".wfd-node.is-executed").Nth(1).WaitForAsync();
            await scene.ScrollAsync(0, 400);
        });
    }
}

internal static class LocatorExtensions
{
    public static async Task ClickIfVisibleAsync(this ILocator locator)
    {
        if (await locator.IsVisibleAsync())
        {
            await locator.ClickAsync();
        }
    }
}

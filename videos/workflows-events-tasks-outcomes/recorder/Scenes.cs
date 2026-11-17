using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Workflows: events, tasks and outcomes", recorded in order on one fresh Blog site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string WorkflowName = "Announce published pages";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-event", EventAsync),
        ("03-outcomes", OutcomesAsync),
        ("04-tasks", TasksAsync),
        ("05-run", RunAsync),
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

    /// <summary>Enables the Workflows feature, outside the capture, and warms up the pages.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#btn-enable-OrchardCore_Workflows").ClickAsync();
        await WaitTextAsync(page, "was enabled");

        foreach (var path in new[] { "/Admin/Workflows/Types", "/Admin/Workflows/Types/Create", "/Admin/Contents/ContentItems" })
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

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    // Opens the designer of the workflow, outside the capture, with the whole width for it.
    private static async Task OpenDesignerAsync(Scene scene)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await page.GetByRole(AriaRole.Link, new() { Name = WorkflowName }).First.ClickAsync();
        await page.Locator("[data-cy='start-picker'], .wfd-node").First.WaitForAsync();
        await page.Locator("[data-cy='panel-tab-issues']").ClickAsync();
        await page.Locator("button.leftbar-compactor").ClickAsync();
        await Task.Delay(600);
        await page.Locator("button[aria-label='Close the activity panel']").First.ClickIfVisibleAsync();
    }

    private static ILocator ForkNode(IPage page) => page.Locator(".wfd-node.is-task").Filter(new() { HasTextString = "Fork" }).First;

    private static async Task EventAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("event-1", async () =>
        {
            await scene.ClickAsync(Group(page, "design"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Workflows/Types'] >> visible=true").First);
            await GoAsync(scene, page.Locator("a.btn.create").First);
        });

        await scene.StepAsync("event-2", async () =>
        {
            await scene.TypeAsync(page.Locator("input#Name"), WorkflowName, delay: 25);
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Create and open the designer" }));
            await page.Locator("[data-cy='start-picker']").WaitForAsync();
        });

        await scene.StepAsync("event-3", async () =>
        {
            await ShowAsync(scene, page.Locator("[data-cy='start-picker']"), 2500);
            await scene.ClickAsync(page.Locator("[data-cy='start-picker-ContentPublishedEvent']"));
        });

        await scene.StepAsync("event-4", async () =>
        {
            await page.Locator("[data-cy='start-badge']").WaitForAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='start-badge']"));
        });

        await scene.StepAsync("event-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("button.dropdown-toggle[data-id='ContentPublishedEvent_SelectedContentTypeNames']"), pauseAfter: 300);
            await page.Locator(".multiselect--active, .dropdown-menu.show").First.WaitForAsync();
            await scene.TypeAsync(page.GetByPlaceholder("Search content types"), "Page", delay: 40);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show ul.inner li a").Filter(new() { HasTextString = "Page" }).First);
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForFunctionAsync("() => document.querySelector('.wfd-node.is-start .wfd-node-body em')?.innerText.includes('Page')");
        });
    }

    private static async Task OutcomesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("outcomes-1", async () =>
        {
            var done = page.Locator(".wfd-node.is-start [data-outcome='Done']");
            await scene.MoveToAsync(done);
            await scene.PauseAsync(1500);
            await scene.ClickAsync(done, pauseAfter: 150);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
        });

        await scene.StepAsync("outcomes-2", async () =>
        {
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "fork", delay: 40);
            await scene.ClickAsync(page.Locator("[data-cy='quick-add-ForkTask']"), pauseAfter: 200);
            await page.Locator(".wfd-node.is-task").First.WaitForAsync();
        });

        await scene.StepAsync("outcomes-3", async () =>
        {
            await scene.HighlightAsync(ForkNode(page));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("outcomes-4", async () =>
        {
            var forks = page.GetByLabel("Forks");
            await scene.ClickAsync(forks, pauseAfter: 100);
            await scene.TypeAsync(forks, "Editors, Audit", delay: 40);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("outcomes-5", async () =>
        {
            await page.Locator("button[aria-label='Close the activity panel']").First.ClickIfVisibleAsync();
            await ShowAsync(scene, ForkNode(page).Locator("[data-outcome='Editors']"), 1800);
            await scene.HighlightAsync(ForkNode(page).Locator("[data-outcome='Audit']"));
        });
    }

    private static async Task TasksAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("tasks-1", async () =>
        {
            var editors = ForkNode(page).Locator("[data-outcome='Editors']");
            await scene.MoveToAsync(editors);
            await scene.PauseAsync(600);
            await scene.ClickAsync(editors, pauseAfter: 150);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "notify", delay: 40);
            await scene.ClickAsync(page.Locator("[data-cy='quick-add-NotifyTask']"), pauseAfter: 200);
        });

        await scene.StepAsync("tasks-2", async () =>
        {
            var message = page.GetByLabel("Message");
            await message.WaitForAsync();
            await scene.ClickAsync(message, pauseAfter: 100);
            await scene.TypeAsync(message, "Announced by the workflow, a page was published.", delay: 20);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("tasks-3", async () =>
        {
            await page.Locator("button[aria-label='Close the activity panel']").First.ClickIfVisibleAsync();
            await scene.TypeAsync(page.Locator("input[placeholder='Search activities']"), "log", delay: 40);
            await scene.ClickAsync(page.Locator("label[for='wfd-kind-tasks']"), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-activity-LogTask']"), pauseAfter: 200);
            var log = page.Locator(".wfd-node.is-task").Filter(new() { HasTextString = "Log" }).First;
            await log.WaitForAsync();

            // A new activity lands in the middle of the canvas, on top of the Fork Task: move it below.
            var (lx, ly) = await scene.CenterOfAsync(log);
            await scene.DragAsync(log, lx + 120, ly + 190);
        });

        await scene.StepAsync("tasks-4", async () =>
        {
            var message = page.GetByLabel("Text", new() { Exact = true });

            if (!await message.IsVisibleAsync())
            {
                await scene.DoubleClickAsync(page.Locator(".wfd-node.is-task").Filter(new() { HasTextString = "Log" }).First, pauseAfter: 300);
            }

            await message.WaitForAsync();
            await scene.ClickAsync(message, pauseAfter: 100);
            await scene.TypeAsync(message, "Audit, a page was published.", delay: 25);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("tasks-5", async () =>
        {
            await page.Locator("button[aria-label='Close the activity panel']").First.ClickIfVisibleAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='issue-badge']").First);
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("tasks-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var log = page.Locator(".wfd-node.is-task").Filter(new() { HasTextString = "Log" }).First;
            await scene.DragToAsync(ForkNode(page).Locator("[data-outcome='Audit']"), log);
            await page.Locator("[data-cy='issue-badge']").WaitForAsync(new() { State = WaitForSelectorState.Detached });
        });

        await scene.StepAsync("tasks-7", async () =>
        {
            await scene.MoveToAsync(page.Locator(".wfd-node.is-task").Filter(new() { HasTextString = "Notify" }).First.Locator("[data-outcome='Done']"));
            await scene.PauseAsync(2000);
            await scene.MoveToAsync(page.Locator(".wfd-node.is-task").Filter(new() { HasTextString = "Log" }).First.Locator("[data-outcome='Done']"));
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await page.WaitForFunctionAsync("() => document.body.innerText.includes('Saved')");
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-publish']"));
            await WaitTextAsync(page, "The workflow was published as version");
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.GotoAsync(BaseUrl + "/Admin")), leadMilliseconds: 250);
            await scene.ClickAsync(Group(page, "content"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-content-content_items"));
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Page" }).First);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Opening hours", delay: 30);
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").Filter(new() { Visible = true }).First);
        });

        await scene.StepAsync("run-4", async () =>
        {
            await WaitTextAsync(page, "Announced by the workflow, a page was published.");
        });

        await scene.StepAsync("run-5", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.GotoAsync(BaseUrl + "/Admin/Workflows/Types")), leadMilliseconds: 250);
            await GoAsync(scene, page.Locator("a.btn:has-text('Instances')").First);
            await scene.HighlightAsync(page.Locator(".badge:has-text('Finished'), [data-cy='instance-count-Finished']").First);
        });

        await scene.StepAsync("run-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a[href*='/Admin/Workflows/Instances/Details'], a[href*='Details']").First);
            await page.Locator(".wfd-node.is-executed").Nth(2).WaitForAsync();
            await scene.ScrollAsync(0, 420);
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

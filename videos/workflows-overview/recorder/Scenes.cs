using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the Workflows overview, recorded in order on one fresh Blog site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string WorkflowName = "Notify the editors when an article is published";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-enable", EnableAsync),
        ("04-create", CreateAsync),
        ("05-designer", DesignerAsync),
        ("06-build", BuildAsync),
        ("07-run", RunAsync),
        ("08-instances", InstancesAsync),
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

    private static async Task WaitTextAsync(IPage page, string text, int timeout = 60_000)
        => await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = timeout });

    // Clicks a link or button that loads a new page; the load is cut out of the video (see Scene.OffCameraAsync).
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
    }

    private static async Task SettledSummaryAsync(IPage page)
    {
        var last = "";
        var same = 0;

        while (same < 4)
        {
            var now = await page.Locator("#list-summary").InnerTextAsync();
            same = now == last ? same + 1 : 0;
            last = now;
            await Task.Delay(150);
        }
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(page.Locator("button.nav-group-label.oc-navigation-admin-tools"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Features']").First);
            await scene.TypeAsync(page.Locator("#search-box"), "workflows", delay: 30);
            await SettledSummaryAsync(page);
            await scene.ScrollAsync(0, 170);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("div.feature-group").Filter(new() { Has = page.Locator("h3", new() { HasTextString = "Workflows" }) }).Last);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_Workflows"));
            await WaitTextAsync(page, "The feature Workflows was enabled.");
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await page.Locator("#search-box").FillAsync("workflows");
            await SettledSummaryAsync(page);
            await scene.ScrollAsync(0, 170);
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
            await scene.ClickAsync(page.Locator("button.nav-group-label.oc-navigation-admin-design"));
            await GoAsync(scene, page.Locator("a[href$='/Admin/Workflows/Types']").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await GoAsync(scene, page.Locator("a.btn.create").First);
            await scene.TypeAsync(page.Locator("input#Name"), WorkflowName, delay: 35);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClickAsync(page.Locator("details[data-cy='create-more-settings'] summary"));
            await scene.HighlightAsync(page.Locator("input#IsEnabled"));
            await scene.HighlightAsync(page.Locator("select[data-cy='instance-limit']"));
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var button = page.GetByRole(AriaRole.Button, new() { Name = "Create and open the designer" });
            await button.ScrollIntoViewIfNeededAsync();
            await GoAsync(scene, button);
            await page.Locator("[data-cy='start-picker']").WaitForAsync();
        });
    }

    private static async Task OpenDesignerAsync(Scene scene)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await page.GetByRole(AriaRole.Link, new() { Name = WorkflowName }).First.ClickAsync();
        await page.Locator("[data-cy='start-picker']").WaitForAsync();
        await page.Locator("[data-cy='panel-tab-issues']").ClickAsync();
        await page.Locator("button.leftbar-compactor").ClickAsync();
        await Task.Delay(600);
    }

    private static async Task DesignerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("designer-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("[data-cy='designer-toolbar']"));
            await scene.MoveToAsync(page.Locator("[data-cy='toolbar-list']"));
        });

        await scene.StepAsync("designer-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='designer-toolbox']"));
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-category-Content']"));
            await scene.ClickAsync(page.Locator("label[for='wfd-kind-events']"));
        });

        await scene.StepAsync("designer-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("label[for='wfd-kind-all']"));
            await scene.HighlightAsync(page.Locator("[data-cy='designer-canvas']"));
            await scene.MoveToAsync(page.Locator("[data-cy='start-picker']"));
        });

        await scene.StepAsync("designer-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("ul.wfd-panel-tabs:has([data-cy='panel-tab-workflow'])"));
            var tab = page.Locator("[data-cy='panel-tab-workflow']");
            var box = await tab.BoundingBoxAsync();
            await scene.MoveToAsync(box.X + box.Width / 2, box.Y + 200);
            await scene.ClickAsync(tab);
            await scene.PauseAsync(1500);
            await scene.ClickAsync(page.Locator("[data-cy='panel-tab-issues']"));
        });
    }

    private static async Task BuildAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("build-1", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='start-picker-ContentPublishedEvent']"));
            await page.Locator(".wfd-node.is-start").WaitForAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='start-badge']"));
        });

        await scene.StepAsync("build-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("button.dropdown-toggle[data-id='ContentPublishedEvent_SelectedContentTypeNames']"));
            await scene.TypeAsync(page.GetByPlaceholder("Search content types"), "Article", delay: 70);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show ul.inner li a").Filter(new() { HasTextString = "Article" }).First);
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForFunctionAsync("() => document.querySelector('.wfd-node.is-start .wfd-node-body em')?.innerText.includes('Article')");
        });

        await scene.StepAsync("build-3", async () =>
        {
            await scene.ClickAsync(page.Locator("button[aria-label='Close the activity panel']"), pauseAfter: 150);
            await scene.MoveToAsync(page.Locator(".wfd-node.is-start [data-outcome='Done']"));
            await scene.PauseAsync(700);
            await scene.ClickAsync(page.Locator(".wfd-node.is-start [data-outcome='Done']"), pauseAfter: 150);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "Notify", delay: 45);
        });

        await scene.StepAsync("build-4", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='quick-add-NotifyTask']"));
            await page.Locator(".wfd-node.is-task").WaitForAsync();
        });

        await scene.StepAsync("build-5", async () =>
        {
            await scene.HighlightAsync(page.Locator("[data-cy='issue-badge']").First);
            await scene.HighlightAsync(page.Locator("[data-cy='issues-count']"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.GetByLabel("Message"), "An article was published.", delay: 40);
            await page.Keyboard.PressAsync("Tab");
            await page.Locator("[data-cy='issue-badge']").WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await page.GetByLabel("Message").ScrollIntoViewIfNeededAsync();
        });

        await scene.StepAsync("build-6", async () =>
        {
            var saved = page.Locator("[data-cy='designer-toolbar']").GetByText("Saved", new() { Exact = true });
            await saved.WaitForAsync();
            await scene.HighlightAsync(saved);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-publish']"));
            await WaitTextAsync(page, "The workflow was published as version 2.");
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAsync(page.Locator("button.nav-group-label.oc-navigation-admin-content"));
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-content-content_items").First);
            var row = page.Locator("li, tr").Filter(new() { HasTextString = "About" }).Filter(new() { Has = page.Locator("a.btn.edit") }).Last;
            await GoAsync(scene, row.Locator("a.btn.edit").First);
            await scene.HighlightAsync(page.Locator("button.publish").Filter(new() { Visible = true }).First);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "An article was published.");
            await WaitTextAsync(page, "Your Article has been published.");
        });
    }

    private static async Task InstancesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("instances-1", async () =>
        {
            await scene.ClickAsync(page.Locator("button.nav-group-label.oc-navigation-admin-design"));
            await GoAsync(scene, page.Locator("a[href$='/Admin/Workflows/Types']").First);
            await scene.HighlightAsync(page.Locator("a.badge:has-text('Instances')").First);
        });

        await scene.StepAsync("instances-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.badge:has-text('Instances')").First);
            await scene.HighlightAsync(page.Locator("[data-cy='instance-count-Finished']"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a[href*='/Admin/Workflows/Instances/Details'], a[href*='Details']").First);
            await page.Locator(".wfd-node.is-executed").Nth(1).WaitForAsync();
            await scene.ScrollAsync(0, 420);
        });
    }
}

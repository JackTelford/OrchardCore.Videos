using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Workflows: create, update and publish content", recorded in order on one fresh Blog site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string WorkflowName = "Announce published articles";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-event", EventAsync),
        ("03-create", CreateAsync),
        ("04-update", UpdateAsync),
        ("05-publish", PublishAsync),
        ("06-run", RunAsync),
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

    /// <summary>Enables the Workflows feature and creates the Announcement content type, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#btn-enable-OrchardCore_Workflows").ClickAsync();
        await WaitTextAsync(page, "was enabled");

        // The Announcement content type (Title and HtmlBody parts).
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/List");
        await page.Locator("#btnCreate").ClickAsync();
        await page.Locator("#DisplayName").PressSequentiallyAsync("Announcement", new() { Delay = 30 });
        await page.Locator("button.create").First.ClickAsync();

        foreach (var part in new[] { "Title", "HtmlBody" })
        {
            var row = page.Locator("li.list-group-item").Filter(new() { Has = page.GetByText(part, new() { Exact = true }) }).First;
            await row.Locator("input[type='checkbox']").First.ClickAsync();
        }

        await page.Locator("#add-selected-parts-button").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        foreach (var path in new[] { "/Admin/Workflows/Types", "/Admin/Workflows/Types/Create", "/Admin/Contents/ContentItems" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Site prepared.");
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

    private static ILocator FreeDone(IPage page) => page.Locator(".wfd-port:not(.is-connected)[data-cy$='-Done']").First;

    private static async Task AddAfterDoneAsync(Scene scene, string search, string activity)
    {
        var page = scene.Page;
        var done = FreeDone(page);
        await scene.MoveToAsync(done);
        await scene.PauseAsync(600);
        await scene.ClickAsync(done, pauseAfter: 150);
        await page.Locator("[data-cy='quick-add']").WaitForAsync();
        await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), search, delay: 25);
        await scene.ClickAsync(page.Locator($"[data-cy='quick-add-{activity}']"), pauseAfter: 200);
        await page.Locator("[data-cy='panel-activity-form']").WaitForAsync();
    }

    // Replaces the content of the Content Properties editor (a CodeMirror editor) with a one-line JSON text.
    private static async Task SetPropertiesAsync(Scene scene, string json)
    {
        var page = scene.Page;
        var editor = page.Locator("[data-task-editor='content'] .CodeMirror").First;
        await editor.ScrollToMiddleAsync();
        await scene.ClickAsync(editor, pauseAfter: 150);
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.InsertTextAsync(json);
        await scene.ClickAsync(page.Locator("[data-cy='panel-apply']"), pauseAfter: 100);
        await page.GetByText("Saved!").First.WaitForAsync();
    }

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
            await scene.TypeAsync(page.Locator("input#Name"), WorkflowName, delay: 15);
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Create and open the designer" }));
            await page.Locator("[data-cy='start-picker']").WaitForAsync();
        });

        await scene.StepAsync("event-2", async () =>
        {
            await ShowAsync(scene, page.Locator("[data-cy='start-picker']"), 2500);
            await scene.ClickAsync(page.Locator("[data-cy='start-picker-ContentPublishedEvent']"));
            await page.Locator("[data-cy='start-badge']").WaitForAsync();
        });

        await scene.StepAsync("event-3", async () =>
        {
            await scene.ClickAsync(page.Locator("button.dropdown-toggle[data-id='ContentPublishedEvent_SelectedContentTypeNames']"), pauseAfter: 300);
            await page.Locator(".dropdown-menu.show").First.WaitForAsync();
            await scene.TypeAsync(page.GetByPlaceholder("Search content types"), "Article", delay: 40);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show ul.inner li a").Filter(new() { HasTextString = "Article" }).First);
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForFunctionAsync("() => document.querySelector('.wfd-node.is-start .wfd-node-body em')?.innerText.includes('Article')");
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await AddAfterDoneAsync(scene, "Create Content", "CreateContentTask");
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.SelectAsync(page.GetByLabel("Content Type", new() { Exact = true }), "Announcement", pauseAfter: 400);
            var publish = page.GetByLabel("Publish", new() { Exact = true }).First;
            await scene.MoveToAsync(publish);
            await scene.HighlightAsync(publish);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await SetPropertiesAsync(scene, "{ \"DisplayText\": \"Announcement: {{ Workflow.Input.ContentItem.DisplayText }}\", \"TitlePart\": { \"Title\": \"Announcement: {{ Workflow.Input.ContentItem.DisplayText }}\" } }");
        });
    }

    private static async Task UpdateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("update-1", async () =>
        {
            await AddAfterDoneAsync(scene, "Update Content", "UpdateContentTask");
        });

        await scene.StepAsync("update-2", async () =>
        {
            var id = page.GetByLabel("Content Item ID", new() { Exact = true });
            await scene.ClickAsync(id, pauseAfter: 100);
            await scene.TypeAsync(id, "property(\"ContentItem\").ContentItemId", delay: 15);
            await page.Keyboard.PressAsync("Tab");
            await scene.ClickAsync(page.Locator("[data-cy='activity-tab-data']"), pauseAfter: 400);
        });

        await scene.StepAsync("update-3", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='activity-tab-settings']"), pauseAfter: 400);
            await SetPropertiesAsync(scene, "{ \"HtmlBodyPart\": { \"Html\": \"<p>The {{ Workflow.Input.ContentEvent.ContentType }} {{ Workflow.Input.ContentItem.DisplayText }} was published.</p>\" } }");
        });
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("publish-1", async () =>
        {
            await AddAfterDoneAsync(scene, "Publish Content", "PublishContentTask");
        });

        await scene.StepAsync("publish-2", async () =>
        {
            var expression = page.GetByLabel("Expression", new() { Exact = true });
            await scene.ClickAsync(expression, pauseAfter: 100);
            await scene.TypeAsync(expression, "property(\"ContentItem\")", delay: 25);
            await page.Keyboard.PressAsync("Tab");
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
            await GoAsync(scene, page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Article" }).First);
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Seed Starting Guide", delay: 25);
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").Filter(new() { Visible = true }).First);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await WaitTextAsync(page, "Your Article has been published.");
            var row = page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "Announcement: Seed Starting Guide" }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, row.Locator("a.edit").First);
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

    // Scrolls an element to the middle of the window.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

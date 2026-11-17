using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Notifications", recorded in order on one fresh site.</summary>
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
        ("02b-seed", SeedAsync),
        ("03-workflow", WorkflowAsync),
        ("04-bell", BellAsync),
        ("06-preferences", PreferencesAsync),
        ("05-center", CenterAsync),
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

    private const string FeaturesJson = """
        {"steps":[{"name":"feature","enable":["OrchardCore.Workflows","OrchardCore.Email","OrchardCore.Email.Smtp","OrchardCore.Sms"],"disable":[]}]}
        """;

    private const string WorkflowsJson = """
        {"steps":[{"name":"WorkflowType","data":[
        {"WorkflowTypeId":"notifyeditorspublished","Name":"Notify the editors when a page is published","IsEnabled":true,"IsSingleton":false,"DeleteFinishedWorkflows":false,
         "Activities":[
          {"ActivityId":"pagepublished","Name":"ContentPublishedEvent","X":40,"Y":40,"IsStart":true,"Properties":{"ContentTypeFilter":["Page"]}},
          {"ActivityId":"notifyeditors","Name":"NotifyUserTask","X":380,"Y":40,"IsStart":false,"Properties":{"UserNames":{"Expression":"ADMIN, JAMIE.CHEN"},"Subject":{"Expression":"A page was published"},"Summary":{"Expression":"Check the page and share it with your readers."},"TextBody":{"Expression":"The page is live on the site."},"HtmlBody":{"Expression":""},"IsHtmlPreferred":false,"Url":{"Expression":""}}}],
         "Transitions":[{"SourceActivityId":"pagepublished","SourceOutcomeName":"Done","DestinationActivityId":"notifyeditors"}]},
        {"WorkflowTypeId":"notifyeditorsunpublished","Name":"Notify the editors when a page is unpublished","IsEnabled":true,"IsSingleton":false,"DeleteFinishedWorkflows":false,
         "Activities":[
          {"ActivityId":"pageunpublished","Name":"ContentUnpublishedEvent","X":40,"Y":40,"IsStart":true,"Properties":{"ContentTypeFilter":["Page"]}},
          {"ActivityId":"notifyeditors","Name":"NotifyUserTask","X":380,"Y":40,"IsStart":false,"Properties":{"UserNames":{"Expression":"ADMIN, JAMIE.CHEN"},"Subject":{"Expression":"A page was unpublished"},"Summary":{"Expression":"Readers no longer see the page."},"TextBody":{"Expression":"The page is no longer visible on the site."},"HtmlBody":{"Expression":""},"IsHtmlPreferred":false,"Url":{"Expression":""}}}],
         "Transitions":[{"SourceActivityId":"pageunpublished","SourceOutcomeName":"Done","DestinationActivityId":"notifyeditors"}]}
        ]}]}
        """;

    private static async Task ImportJsonAsync(IPage page, string json)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!");
    }

    private static async Task<(IBrowserContext Context, IPage Page)> OpenAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        return (context, page);
    }

    private static async Task CreatePageAsync(IPage page, string title, string button)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Page/Create");
        await page.GetByLabel("Title", new() { Exact = true }).First.FillAsync(title);
        await page.Locator(button).First.ClickAsync();
        await page.WaitForFunctionAsync("() => !document.location.pathname.endsWith('/Create')");
    }

    /// <summary>Starting state S0: the features, the second user, the phone numbers and the draft page. Outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await OpenAsync(browser);

        await ImportJsonAsync(page, FeaturesJson);
        await Task.Delay(3000);

        await page.GotoAsync(BaseUrl + "/Admin/Settings/userLogin");
        await page.GetByLabel("Allow user to change their phone number").CheckAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First.ClickAsync();
        await WaitTextAsync(page, "Site settings updated successfully.");

        await page.GotoAsync(BaseUrl + "/Admin/Users/Index");
        await page.Locator("a[href*='/Admin/Users/Edit/']").First.ClickAsync();
        await page.Locator("input[type='tel']").FillAsync("+12015550123");
        await page.Locator("button.save").First.ClickAsync();
        await WaitTextAsync(page, "User updated successfully.");

        await page.GotoAsync(BaseUrl + "/Admin/Users/Create");
        await page.GetByLabel("User name").FillAsync("jamie.chen");
        await page.Locator("input[type='email']").FillAsync("jamie.chen@example.com");
        await page.Locator("input[type='tel']").FillAsync("+12015550124");
        await page.Locator("input[type='password']").First.FillAsync("Password1!");
        await page.Locator("input[type='password']").Nth(1).FillAsync("Password1!");
        await page.Locator("label").Filter(new() { HasTextString = "Editor" }).First.ClickAsync();
        await page.Locator("button.save, button.btn-success").First.ClickAsync();
        await WaitTextAsync(page, "User created successfully.");

        await CreatePageAsync(page, "Contact us", "button.save-draft, button:has-text('Save Draft')");

        foreach (var path in new[] { "/Admin/Features", "/Admin/Workflows/Types", "/Admin/Contents/ContentItems", "/Admin/notifications", "/Admin/Users/Index" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Prepared.");
    }

    /// <summary>Seed S1 (after clip 02-enable): the two workflows and the two pages. Outside the capture.</summary>
    private static async Task SeedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var (context, page) = await OpenAsync(browser);
        await ImportJsonAsync(page, WorkflowsJson);
        await CreatePageAsync(page, "About the market", "button.publish");
        await CreatePageAsync(page, "Old market map", "button.publish");

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems");
        var row = page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "Old market map" }).First;
        await row.GetByRole(AriaRole.Button, new() { Name = "Actions" }).ClickAsync();
        await row.GetByText("Unpublish", new() { Exact = true }).ClickAsync();
        await page.Locator(".modal.show").GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await WaitTextAsync(page, "unpublished");

        await page.GotoAsync(BaseUrl + "/Admin/notifications");
        await WaitTextAsync(page, "A page was unpublished");
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
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

    // Opens a page during the capture, with the load cut out of the video.
    private static Task VisitAsync(Scene scene, string path)
        => scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(BaseUrl + path)), leadMilliseconds: 250);

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Row(IPage page, string text) => page.Locator(".list-group-item, tr").Filter(new() { HasTextString = text }).First;

    private static async Task OpenAdminGroupAsync(IPage page, string name)
    {
        var group = page.Locator("#left-nav").GetByText(name, new() { Exact = true }).First;
        await group.ClickAsync();
        await Task.Delay(500);
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<IPage, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene.Page);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static ILocator Nav(IPage page, string href) => page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Bell(IPage page) => page.Locator(".notification-navbar-container > button").First;

    private static ILocator Toggle(IPage page, string value) => page.Locator($"input[name='featureIds'][value='{value}']");

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, Nav(page, "/Admin/Features"));
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("notifications", new() { Delay = 45 });
            await Toggle(page, "OrchardCore.Notifications.Sms").WaitForAsync(new() { State = WaitForSelectorState.Attached });
            await page.Locator("h3:text-is('Notifications')").First.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            foreach (var id in new[] { "OrchardCore.Notifications", "OrchardCore.Notifications.Email", "OrchardCore.Notifications.Sms" })
            {
                var row = page.Locator(".list-group-item").Filter(new() { Has = Toggle(page, id) }).First;
                await ShowAsync(scene, row, 1800);
            }

            await scene.HighlightAsync(page.Locator(".list-group-item").Filter(new() { Has = Toggle(page, "OrchardCore.Notifications.Sms") }).First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            foreach (var id in new[] { "OrchardCore.Notifications", "OrchardCore.Notifications.Email", "OrchardCore.Notifications.Sms" })
            {
                await scene.ClickAsync(Toggle(page, id), pauseAfter: 300);
            }

            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("a[data-action='Enable']").First);
            await WaitTextAsync(page, "were enabled", 90_000);
            await Bell(page).WaitForAsync();
            await scene.MoveToAsync(900, 650);
            await scene.HighlightAsync(Bell(page));
        });
    }

    private static async Task WorkflowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;
        page.SetDefaultTimeout(180_000);

        await scene.StepAsync("workflow-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Design");
            await GoAsync(scene, Nav(page, "/Admin/Workflows/Types"));
            await GoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Notify the editors when a page is published", Exact = true }).First);
            await page.Locator("[data-cy='activity-notifyeditors']").WaitForAsync();
            await scene.MoveToAsync(900, 650);
        });

        await scene.StepAsync("workflow-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("[data-cy='activity-pagepublished']"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='activity-notifyeditors']"));
        });

        await scene.StepAsync("workflow-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.DoubleClickAsync(page.Locator("[data-cy='activity-notifyeditors']"));
            await page.Locator("[data-cy='activity-panel']").WaitForAsync();
            var panel = page.Locator("[data-cy='activity-panel']");
            var users = panel.GetByLabel("User names");
            await users.ScrollToMiddleAsync();
            await ShowAsync(scene, users, 1500);
            await scene.HighlightAsync(panel.GetByLabel("Subject", new() { Exact = true }));
        });

        await scene.StepAsync("workflow-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var panel = page.Locator("[data-cy='activity-panel']");
            foreach (var label in new[] { "Summary", "URL", "Type", "Text Body", "HTML Body" })
            {
                var field = panel.GetByLabel(label, new() { Exact = true }).First;
                await field.ScrollToMiddleAsync();
                await scene.PauseAsync(1500);
            }

            await panel.GetByText("Use HTML Body when possible").First.ScrollToMiddleAsync();
        });

        await scene.StepAsync("workflow-5", async () =>
        {
            var panel = page.Locator("[data-cy='activity-panel']");
            await panel.GetByLabel("Text Body", new() { Exact = true }).First.ScrollToMiddleAsync();
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-search']"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("notifications", new() { Delay = 45 });
            var first = page.Locator(".wfd-toolbox-list li").Filter(new() { HasTextString = "Notify Specific Users" }).First;
            await first.WaitForAsync();
            await ShowAsync(scene, first, 1500);
            await scene.HighlightAsync(page.Locator(".wfd-toolbox-list li").Filter(new() { HasTextString = "Notify Content Owner" }).First);
        });

        await scene.StepAsync("workflow-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenAdminGroupAsync(page, "Content");
            await GoAsync(scene, page.Locator("#left-nav a").Filter(new() { HasTextString = "Content Items" }).First);
            await scene.ClickAsync(page.Locator("input[type='search'], input[placeholder='Search']").First, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Contact us", new() { Delay = 45 });
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Keyboard.PressAsync("Enter")), leadMilliseconds: 250);
            var row = Row(page, "Contact us");
            await row.WaitForAsync();
            await scene.ClickAsync(row.GetByRole(AriaRole.Button, new() { Name = "Actions" }), pauseAfter: 300);
            var publish = row.GetByText("Publish Draft", new() { Exact = true }).First;
            await scene.MoveToAsync(publish);
            var (x, y) = await scene.CenterOfAsync(publish);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
            await scene.MoveToAsync(900, 650);
            await WaitTextAsync(page, "published");
        });
    }

    private static async Task BellAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", p => Bell(p).WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("bell-1", async () =>
        {
            await scene.MoveToAsync(Bell(page));
            await scene.HighlightAsync(Bell(page));
        });

        await scene.StepAsync("bell-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Bell(page), pauseAfter: 600);
            await page.Locator(".notification-navbar").First.WaitForAsync();
        });

        await scene.StepAsync("bell-3", async () =>
        {
            var menu = page.Locator(".notification-navbar").First;
            var link = page.Locator(".notification-navbar a").Filter(new() { HasTextString = "Notification Center" }).First;
            await link.ScrollIntoViewIfNeededAsync();
            var box = await menu.BoundingBoxAsync();
            var bellBox = await Bell(page).BoundingBoxAsync();
            var outsideX = box.X + box.Width + 40;
            await scene.MoveToAsync(outsideX, bellBox.Y + bellBox.Height / 2);
            var (x, y) = await scene.CenterOfAsync(link);
            await scene.MoveToAsync(outsideX, y);
            await scene.MoveToAsync(x + box.Width / 2 - 20, y);
            var (cx, cy) = (x + box.Width / 2 - 20, y);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)cx, (float)cy)), leadMilliseconds: 250);
            await scene.MoveToAsync(900, 650);
        });
    }

    private static async Task CenterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/notifications", p => p.Locator("input[name='itemIds']").First.WaitForAsync(new() { State = WaitForSelectorState.Attached }));
        var page = scene.Page;
        ILocator Rows() => page.Locator("li.notification-container-item");

        await scene.StepAsync("center-1");

        await scene.StepAsync("center-2", async () =>
        {
            await ShowAsync(scene, Rows().First.Locator("p.fw-bold").First, 1500);
            await ShowAsync(scene, Rows().First.Locator(".badge.ta-badge").First, 1500);
            var last = Rows().Last;
            await GoAsync(scene, last.GetByRole(AriaRole.Link, new() { Name = "Mark as read" }).First);
            await scene.MoveToAsync(900, 650);
        });

        await scene.StepAsync("center-3", async () =>
        {
            await scene.ClickAsync(page.Locator("input[placeholder='Search']").First, pauseAfter: 200);
            await page.Keyboard.TypeAsync("share", new() { Delay = 50 });
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Keyboard.PressAsync("Enter")), leadMilliseconds: 250);
            await scene.ClickAsync(page.Locator(".bootstrap-select button[title='Show']").First, pauseAfter: 400);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Locator(".bootstrap-select .dropdown-menu.show a, .bootstrap-select .dropdown-menu.show li").Filter(new() { HasTextString = "Unread" }).First.ClickAsync()), leadMilliseconds: 250);
            await scene.MoveToAsync(page.Locator("button[title='Sort']").First);
            await scene.HighlightAsync(page.Locator("button[title='Sort']").First);
        });

        await scene.StepAsync("center-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = Rows().First;
            await scene.ClickAsync(row.Locator("button.dropdown-toggle.actions").First, pauseAfter: 300);
            await GoAsync(scene, row.GetByText("View", new() { Exact = true }).First);
            await page.GetByText("The page is live on the site.").First.WaitForAsync();
            await scene.MoveToAsync(900, 650);
            await ShowAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Mark as unread" }).Or(page.GetByRole(AriaRole.Link, new() { Name = "Mark as unread" })).First, 1200);
            await scene.HighlightAsync(page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).Or(page.GetByRole(AriaRole.Link, new() { Name = "Delete" })).First);
        });

        await scene.StepAsync("center-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator(".breadcrumb a").Filter(new() { HasTextString = "Notification Center" }).First);
            await page.Locator("input[name='itemIds']").First.WaitForAsync(new() { State = WaitForSelectorState.Attached });
            var checks = page.Locator("li.notification-is-unread input[name='itemIds']");
            await scene.ClickAsync(checks.Nth(0), pauseAfter: 300);
            await scene.ClickAsync(checks.Nth(1), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("a.dropdown-item[data-action='Read']").First, pauseAfter: 500);
            var ok = page.Locator("#modalOkButton");
            await ok.WaitForAsync();
            await scene.MoveToAsync(ok);
            var (x, y) = await scene.CenterOfAsync(ok);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
            await scene.MoveToAsync(900, 650);
        });

        await scene.StepAsync("center-6", async () =>
        {
            await scene.HighlightAsync(page.Locator("a.btn").Filter(new() { HasTextString = "Mark All as Read" }).First);
        });
    }

    private static async Task PreferencesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;
        ILocator Methods() => page.Locator("ul[id$='Methods']").First;

        await scene.StepAsync("pref-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#navbarDropdown"), pauseAfter: 400);
            await GoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Profile" }).First);
            await page.Locator("input[type='tel']").WaitForAsync();
        });

        await scene.StepAsync("pref-2", async () =>
        {
            await Methods().ScrollToMiddleAsync();
            await scene.HighlightAsync(Methods());
        });

        await scene.StepAsync("pref-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Methods().Locator("input[type='checkbox'][value='SMS']"), pauseAfter: 500);
            await GoAsync(scene, page.Locator("button.save").First);
            await WaitTextAsync(page, "User updated successfully.");
            await Methods().ScrollToMiddleAsync();
        });

        await scene.StepAsync("pref-4", async () =>
        {
            await page.Locator("input[type='email']").First.ScrollToMiddleAsync();
            await ShowAsync(scene, page.Locator("input[type='email']").First, 1500);
            await scene.HighlightAsync(page.Locator("input[type='tel']").First);
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the editors.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "User events", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-build", BuildAsync),
        ("03-create", CreateAsync),
        ("04-roles", RolesAsync),
        ("05-signin", SignInAsync),
        ("06-instances", InstancesAsync),
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
        {"steps":[{"name":"feature","enable":["OrchardCore.Workflows"],"disable":[]}]}
        """;

    /// <summary>Enables the Workflows feature (S0), outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", FeaturesJson);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!");
        await Task.Delay(4000);

        foreach (var path in new[] { "/Admin/Workflows/Types", "/Admin/Users/Index", "/Admin/Roles/Index" })
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
        scene.Page.SetDefaultNavigationTimeout(90_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene.Page);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static ILocator Group(IPage page, string key) => page.Locator($"button.nav-group-label.oc-navigation-admin-{key}");

    private static ILocator Saved(IPage page) => page.Locator("[data-cy='toolbar-publish']");

    private static ILocator Toast(IPage page) => page.Locator(".toast.message-information");

    private static async Task WaitSavedAsync(IPage page)
        => await page.WaitForFunctionAsync("() => document.body.innerText.includes('Saved')");

    private static async Task ClosePanelAsync(IPage page)
        => await page.Locator("button[aria-label='Close the activity panel'], [data-cy='activity-panel-close']").First.ClickIfVisibleAsyncSafe();

    private static async Task CreateWorkflowAsync(Scene scene, string name)
    {
        var page = scene.Page;
        await GoAsync(scene, page.Locator("a[href$='/Admin/Workflows/Types'] >> visible=true").First);
        await GoAsync(scene, page.Locator("a.btn.create").First);
        await scene.TypeAsync(page.Locator("input#Name"), name, delay: 30);
        await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Create and open the designer" }));
        await page.Locator("[data-cy='start-picker']").WaitForAsync();
    }

    private static async Task AddNotifyAsync(Scene scene, string message)
    {
        var page = scene.Page;
        await ClosePanelAsync(page);
        var done = page.Locator(".wfd-node.is-start [data-outcome='Done']");
        await scene.MoveToAsync(done);
        await scene.ClickAsync(done, pauseAfter: 150);
        await page.Locator("[data-cy='quick-add']").WaitForAsync();
        await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "notify", delay: 35);
        await scene.ClickAsync(page.Locator("[data-cy='quick-add-NotifyTask']"), pauseAfter: 200);
        var text = page.GetByLabel("Message", new() { Exact = true });
        await text.WaitForAsync();
        await scene.SelectAsync(page.GetByLabel("Notification Type"), "Information", pauseAfter: 300);
        await scene.ClickAsync(text, pauseAfter: 100);
        await scene.TypeAsync(text, message, delay: 25);
        await page.Keyboard.PressAsync("Tab");
        await WaitSavedAsync(page);
        await text.ScrollToMiddleAsync();
    }

    private static async Task PublishAsync(Scene scene)
    {
        await scene.ClickAsync(scene.Page.Locator("[data-cy='toolbar-publish']"));
        await WaitTextAsync(scene.Page, "The workflow was published as version");
    }

    private static async Task BuildAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", p => Task.CompletedTask);
        var page = scene.Page;

        await scene.StepAsync("build-1", async () =>
        {
            await scene.ClickAsync(Group(page, "design"), pauseAfter: 250);
            await ShowAsync(scene, page.Locator("a[href$='/Admin/Workflows/Types'] >> visible=true").First, 1500);
            await CreateWorkflowAsync(scene, "Account created");
        });

        await scene.StepAsync("build-2", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='start-picker-UserCreatedEvent']"));
            await page.Locator("[data-cy='start-badge']").WaitForAsync();
        });

        await scene.StepAsync("build-3", async () =>
        {
            await ClosePanelAsync(page);
            var done = page.Locator(".wfd-node.is-start [data-outcome='Done']");
            await scene.MoveToAsync(done);
            await scene.ClickAsync(done, pauseAfter: 150);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "notify", delay: 35);
            await scene.ClickAsync(page.Locator("[data-cy='quick-add-NotifyTask']"), pauseAfter: 200);
            await page.Locator(".wfd-node.is-task").First.WaitForAsync();
        });

        await scene.StepAsync("build-4", async () =>
        {
            var text = page.GetByLabel("Message", new() { Exact = true });
            await text.WaitForAsync();
            await scene.SelectAsync(page.GetByLabel("Notification Type"), "Information", pauseAfter: 300);
            await scene.ClickAsync(text, pauseAfter: 100);
            await scene.TypeAsync(text, "A user account was created.", delay: 25);
            await page.Keyboard.PressAsync("Tab");
            await WaitSavedAsync(page);
            await text.ScrollToMiddleAsync();
        });

        await scene.StepAsync("build-5", async () =>
        {
            await PublishAsync(scene);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Workflows/Types");
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(Group(page, "access_control"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("#left-nav a[href$='/Admin/Users/Index']").First);
            await GoAsync(scene, page.Locator("a.btn").Filter(new() { HasTextString = "Add User" }).First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("User name", new() { Exact = true }).First, "jamie.chen", delay: 40);
            await scene.TypeAsync(page.GetByLabel("Email", new() { Exact = true }).First, "jamie.chen@example.com", delay: 30);
            await scene.ClickAsync(page.GetByText("Generate password").First, pauseAfter: 400);
            await GoOffAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await Toast(page).First.WaitForAsync();
            await scene.HighlightAsync(Toast(page).First);
        });
    }

    private static async Task GoOffAsync(Scene scene, ILocator locator, int lead = 250)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: lead);
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Users/Index", async p =>
        {
            if (!await p.Locator("a[href$='/Admin/Workflows/Types'] >> visible=true").First.IsVisibleAsync())
            {
                await Group(p, "design").ClickAsync();
            }
        });
        var page = scene.Page;

        await scene.StepAsync("roles-1", async () =>
        {
            await CreateWorkflowAsync(scene, "Role changed");
        });

        await scene.StepAsync("roles-2", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-search']"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("user updated", new() { Delay = 40 });
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-activity-UserUpdatedEvent']"), pauseAfter: 300);
            await page.Locator("[data-cy='start-badge']").WaitForAsync();
        });

        await scene.StepAsync("roles-3", async () =>
        {
            await AddNotifyAsync(scene, "A user account was updated.");
        });

        await scene.StepAsync("roles-4", async () =>
        {
            await PublishAsync(scene);
        });

        await scene.StepAsync("roles-5", async () =>
        {
            await OffAdminAsync(scene);
            await scene.ClickAsync(Group(page, "access_control"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("#left-nav a[href$='/Admin/Users/Index']").First);
            var row = page.Locator("li.list-group-item, tr").Filter(new() { HasTextString = "jamie.chen" }).First;
            await GoAsync(scene, row.Locator("a").Filter(new() { HasTextString = "Edit" }).First);
            await scene.ClickAsync(page.Locator("label").Filter(new() { HasTextString = "Editor" }).First, pauseAfter: 400);
            await GoOffAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First);
        });

        await scene.StepAsync("roles-6", async () =>
        {
            await Toast(page).First.WaitForAsync();
            await scene.HighlightAsync(Toast(page).First);
        });
    }

    private static async Task OffAdminAsync(Scene scene)
    {
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(BaseUrl + "/Admin/Users/Index")), leadMilliseconds: 250);
        await scene.MoveToAsync(900, 650);
    }

    private static async Task SignInAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Users/Index", async p =>
        {
            if (!await p.Locator("a[href$='/Admin/Workflows/Types'] >> visible=true").First.IsVisibleAsync())
            {
                await Group(p, "design").ClickAsync();
            }
        });
        var page = scene.Page;

        await scene.StepAsync("signin-1", async () =>
        {
            await CreateWorkflowAsync(scene, "Welcome back");
        });

        await scene.StepAsync("signin-2", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-search']"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("loggedin", new() { Delay = 40 });
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-activity-UserLoggedInEvent']"), pauseAfter: 300);
            await page.Locator("[data-cy='start-badge']").WaitForAsync();
        });

        await scene.StepAsync("signin-3", async () =>
        {
            await AddNotifyAsync(scene, "Welcome back");
        });

        await scene.StepAsync("signin-4", async () =>
        {
            await PublishAsync(scene);
            await scene.ClickAsync(page.Locator("#navbarDropdown"), pauseAfter: 400);
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Log off" }).Or(page.GetByRole(AriaRole.Link, new() { Name = "Log off" })).First);
        });

        await scene.StepAsync("signin-5", async () =>
        {
            await scene.NavigateAsync("/Login?ReturnUrl=%2FAdmin%2FWorkflows%2FTypes");
            await scene.TypeAsync(page.Locator("#LoginForm_UserName"), "admin", delay: 40);
            await scene.TypeAsync(page.Locator("#LoginForm_Password"), Password, delay: 40);
            await GoOffAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).First);
            await WaitTextAsync(page, "Welcome back");
        });
    }

    private static async Task InstancesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Workflows/Types");
        var page = scene.Page;

        await scene.StepAsync("instances-1", async () =>
        {
            await GoAsync(scene, page.Locator("[data-cy='all-instances']"));
        });

        await scene.StepAsync("instances-2", async () =>
        {
            var row = page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("a[data-cy='instance-workflow']") }).Filter(new() { HasTextString = "Account created" }).First;
            await GoAsync(scene, row.Locator("a[href*='/Workflow/Details/']").First);
            await page.Locator("#workflow-designer").WaitForAsync();
            await ShowAsync(scene, page.GetByText("Correlation ID").First, 2000);
        });
    }
}

internal static class SafeLocatorExtensions
{
    public static async Task ClickIfVisibleAsyncSafe(this ILocator locator)
    {
        try
        {
            if (await locator.IsVisibleAsync())
            {
                await locator.ClickAsync(new() { Timeout = 3000 });
            }
        }
        catch (PlaywrightException)
        {
        }
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

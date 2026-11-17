using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Tenants: Run Many Sites From One Application". The site has the Default, Bakery and Studio tenants
/// (Auto Setup); 03-enable turns the Tenants feature on, then "prepare" seeds the tenants list, then the other clips run.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-enable", EnableAsync),
        ("04-list", ListAsync),
        ("05-actions", ActionsAsync),
        ("06-add", AddAsync),
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

    /// <summary>Seeds the tenants list, outside the capture; run it after 03-enable.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task EditAsync(string tenant, string category, string description, string prefix, string host)
        {
            await page.GotoAsync(BaseUrl + "/Admin/Tenants");
            var row = page.Locator("li").Filter(new() { Has = page.Locator($"a.text-break:text-is('{tenant}')") }).Last;
            await row.Locator("a.btn:text-is('Edit')").First.ClickAsync();
            await page.GetByLabel("Category").First.FillAsync(category);
            await page.GetByLabel("Description").First.FillAsync(description);

            if (prefix is not null)
            {
                await page.GetByLabel("Url Prefix").First.FillAsync(prefix);
            }

            if (host is not null)
            {
                await page.GetByLabel("Hostname").First.FillAsync(host);
            }

            await page.Locator("button.btn-primary, button.save").Filter(new() { HasTextString = "Save" }).First.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await EditAsync("Bakery", "Customers", "Website of a corner bakery.", null, null);
        await EditAsync("Studio", "Customers", "Portfolio of a photography studio.", string.Empty, "studio.example.com");

        await page.GotoAsync(BaseUrl + "/Admin/Tenants");
        var studio = page.Locator("li").Filter(new() { Has = page.Locator("a.text-break:text-is('Studio')") }).Last;
        await studio.Locator("a.btn:text-is('Disable')").First.ClickAsync();
        await page.Locator(".modal.show button.btn-danger, .modal.show button:has-text('Ok'), .modal.show button:has-text('OK')").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync(BaseUrl + "/Admin/Tenants/Create");
        await page.GetByLabel("Name", new() { Exact = true }).First.FillAsync("Workshop");
        await page.GetByLabel("Category").First.FillAsync("Internal");
        await page.GetByLabel("Url Prefix").First.FillAsync("workshop");
        await page.GetByLabel("Description").First.FillAsync("Practice site for the editors.");
        await page.GetByLabel("Recipe").First.SelectOptionAsync(new SelectOptionValue { Label = "Blank site" });
        await page.Locator("button.btn-primary, button.save").Filter(new() { HasTextString = "Create" }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Warms up the Bakery site, outside the capture.
        await page.GotoAsync(BaseUrl + "/bakery");
        await page.GotoAsync(BaseUrl + "/Admin/Tenants");

        await context.CloseAsync();
        Console.WriteLine("Tenants prepared.");
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

    private static ILocator Row(IPage page, string tenant) => page.Locator("li").Filter(new() { Has = page.Locator($"a.text-break:text-is('{tenant}')") }).Last;

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
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
            await scene.TypeAsync(page.Locator("#search-box"), "tenants", delay: 30);
            await page.Locator("#btn-enable-OrchardCore_Tenants").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.MoveToAsync(page.Locator("h3").Filter(new() { HasTextString = "Infrastructure" }).First);
            await scene.PauseAsync(500);
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_Tenants"));
            await WaitTextAsync(page, "The feature Tenants was enabled.");
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClickAsync(Group(page, "multi_tenancy"), pauseAfter: 400);
        });
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.ClickAsync(Group(page, "multi_tenancy"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Tenants'] >> visible=true").First);
            await scene.MoveToAsync(page.Locator("a.text-break:text-is('Bakery')"));
            await scene.PauseAsync(1500);
            await scene.MoveToAsync(Row(page, "Bakery").Locator(".badge").First);
            await scene.PauseAsync(1500);
            await scene.MoveToAsync(Row(page, "Studio").GetByText("studio.example.com").First);
        });

        await scene.StepAsync("list-2", async () =>
        {
            await ShowAsync(scene, page.Locator("#search-box"), 1800);
            await ShowAsync(scene, page.Locator("div.filter").First, 2500);
            await scene.HighlightAsync(page.Locator("div.filter").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, Row(page, "Studio").Locator("a[href*='Options.Status=Disabled']").First);
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.GotoAsync(BaseUrl + "/Admin/Tenants")), leadMilliseconds: 250);
            await scene.ClickAsync(page.Locator("#tenantNames-Bakery"), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("#tenantNames-Studio"), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
        });
    }

    private static async Task ActionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await scene.StartCaptureAsync();

        await scene.StepAsync("actions-1", async () =>
        {
            await scene.HighlightAsync(Row(page, "Default").Locator("a.btn").First);
            await scene.PauseAsync(500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "Default").Locator("div.btn-group, .actions, td:last-child, div:has(> a.btn)").Last);
        });

        await scene.StepAsync("actions-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, Row(page, "Bakery").Locator("a.btn:text-is('Disable'), button:text-is('Disable')").First, 1500);
            await ShowAsync(scene, Row(page, "Bakery").Locator("a.btn:text-is('Features')").First, 1500);
            var view = Row(page, "Bakery").Locator("a.btn:text-is('View')").First;
            await scene.MoveToAsync(view);
            var popup = await scene.Context.RunAndWaitForPageAsync(async () => await scene.ClickAsync(view, pauseAfter: 200));
            await popup.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await scene.SwitchToAsync(popup);
        });

        await scene.StepAsync("actions-3", async () =>
        {
            await scene.SwitchToAsync(page);
            await ShowAsync(scene, Row(page, "Studio").Locator("a.btn:text-is('Enable'), button:text-is('Enable')").First, 2500);
            await scene.HighlightAsync(Row(page, "Workshop").Locator("a.btn:text-is('Setup')").First);
        });
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.btn:has-text('Add Tenant')").First);
            await scene.TypeAsync(page.GetByLabel("Name", new() { Exact = true }).First, "Gallery", delay: 20);
            await scene.TypeAsync(page.GetByLabel("Category").First, "Customers", delay: 20);
            await scene.TypeAsync(page.GetByLabel("Description").First, "Website of an art gallery.", delay: 12);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Url Prefix").First, "gallery", delay: 20);
            await scene.HighlightAsync(page.GetByLabel("Url Prefix").First);
            await scene.HighlightAsync(page.GetByLabel("Hostname").First);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(page.GetByLabel("Recipe").First, "Blank site", pauseAfter: 400);
            var heading = page.GetByText("Database Presets").First;
            await heading.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(2500);
            await scene.ScrollAsync(0, -2000);
            await scene.HighlightAsync(page.Locator("button:has-text('Create'), a:has-text('Create')").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("button:has-text('Create & Setup'), a:has-text('Create & Setup')").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.cancel").First);
        });
    }
}

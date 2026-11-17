using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Tenants: edit, disable, reload and remove", recorded in order on a site with six tenants.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-list", ListAsync),
        ("03-edit", EditAsync),
        ("04-disable", DisableAsync),
        ("05-reload", ReloadAsync),
        ("06-bulk", BulkAsync),
        ("07-remove", RemoveAsync),
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

    /// <summary>Warms up the pages, outside the capture. Nothing is changed.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Tenants", "/Admin/Tenants/Edit/Pilot" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Site warmed up.");
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

    private static ILocator Row(IPage page, string tenant) => page.Locator("li.list-group-item").Filter(new() { Has = page.Locator($"a.text-break:text-is('{tenant}')") }).Last;

    private static ILocator Button(IPage page, string tenant, string text) => Row(page, tenant).Locator($"a.btn:text-is('{text}'), button:text-is('{text}')").First;

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1500)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static async Task ConfirmAsync(Scene scene)
    {
        var page = scene.Page;
        await page.Locator("#modalOkButton").WaitForAsync();
        await GoAsync(scene, page.Locator("#modalOkButton"));
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.ClickAsync(page.Locator("button.nav-group-label.oc-navigation-admin-multi_tenancy"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Tenants'] >> visible=true").First);
            await scene.MoveToAsync(Row(page, "Acme").Locator("a.text-break").First);
            await scene.PauseAsync(1500);
            await scene.MoveToAsync(Row(page, "Acme").Locator(".badge").First);
            await scene.PauseAsync(1500);
            await scene.MoveToAsync(Row(page, "Harbor").Locator(".badge").Last);
        });

        await scene.StepAsync("list-2", async () =>
        {
            await ShowAsync(scene, Row(page, "Acme").Locator("div.btn-group, td:last-child, div:has(> a.btn)").Last, 3000);
            await ShowAsync(scene, Row(page, "Default").Locator("div.btn-group, td:last-child, div:has(> a.btn)").Last, 3500);
            await scene.HighlightAsync(Row(page, "Pilot").Locator("div.btn-group, td:last-child, div:has(> a.btn)").Last);
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "harbor", delay: 60);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Keyboard.PressAsync("Enter")), leadMilliseconds: 250);
        });

        await scene.StepAsync("list-4", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.GotoAsync(BaseUrl + "/Admin/Tenants")), leadMilliseconds: 250);
            await scene.ClickAsync(page.Locator("div.filter button:has-text('State')").First, pauseAfter: 400);
            await GoAsync(scene, page.Locator(".dropdown-menu.show a:has-text('Uninitialized')").First);
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await page.Locator("button.nav-group-label.oc-navigation-admin-multi_tenancy").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("edit-1", async () =>
        {
            await GoAsync(scene, Button(page, "Acme", "Edit"));
            await scene.HighlightAsync(page.Locator("#Name"));
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, page.Locator("#RequestUrlPrefix"), 2500);
            await scene.HighlightAsync(page.Locator("#RequestUrlHost"));
        });

        await scene.StepAsync("edit-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#Category"), "Customers", delay: 25);
            await scene.TypeAsync(page.Locator("#Description"), "Acme Bakery storefront", delay: 12);
        });

        await scene.StepAsync("edit-4", async () =>
        {
            await GoAsync(scene, page.Locator("button.btn-primary:has-text('Save'), button.save").First);
            await Row(page, "Acme").WaitForAsync();
        });

        await scene.StepAsync("edit-5", async () =>
        {
            await GoAsync(scene, Button(page, "Pilot", "Edit"));
            await page.Locator("#DatabaseProvider").ScrollToMiddleAsync();
            await scene.HighlightAsync(page.Locator("#DatabaseProvider"));
        });
    }

    private static async Task DisableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await page.Locator("button.nav-group-label.oc-navigation-admin-multi_tenancy").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("disable-1", async () =>
        {
            await scene.ClickAsync(Button(page, "Harbor", "Disable"), pauseAfter: 300);
            await page.Locator("#modalOkButton").WaitForAsync();
        });

        await scene.StepAsync("disable-2", async () =>
        {
            await ConfirmAsync(scene);
            await Row(page, "Harbor").Locator(".badge:has-text('Disabled')").WaitForAsync();
        });

        await scene.StepAsync("disable-3", async () =>
        {
            await scene.HighlightAsync(Row(page, "Harbor").Locator("div.btn-group, div:has(> a.btn)").Last);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(Button(page, "Harbor", "Remove"));
        });

        await scene.StepAsync("disable-4", async () =>
        {
            await GoAsync(scene, Button(page, "Harbor", "Enable"));
            await Row(page, "Harbor").Locator(".badge:has-text('Running')").WaitForAsync();
        });
    }

    private static async Task ReloadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await page.Locator("button.nav-group-label.oc-navigation-admin-multi_tenancy").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("reload-1", async () =>
        {
            await scene.HighlightAsync(Button(page, "Harbor", "Reload"));
        });

        await scene.StepAsync("reload-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, Button(page, "Harbor", "Reload"));
        });

        await scene.StepAsync("reload-3", async () =>
        {
            await scene.MoveToAsync(Row(page, "Harbor").Locator("a.text-break").First);
        });
    }

    private static async Task BulkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await page.Locator("button.nav-group-label.oc-navigation-admin-multi_tenancy").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("bulk-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#tenantNames-Juniper"), pauseAfter: 400);
            await scene.ClickAsync(page.Locator("#tenantNames-Maple"), pauseAfter: 400);
            await scene.HighlightAsync(page.Locator("#bulk-action-menu-button"));
        });

        await scene.StepAsync("bulk-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 400);
            await scene.ClickAsync(page.Locator("a.dropdown-item[data-action='Disable']"), pauseAfter: 300);
            await ConfirmAsync(scene);
        });

        await scene.StepAsync("bulk-3", async () =>
        {
            await scene.HighlightAsync(Row(page, "Juniper"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "Maple"));
        });
    }

    private static async Task RemoveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // Maple must no longer be in use when Remove runs.
        await Task.Delay(15_000);
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await page.Locator("button.nav-group-label.oc-navigation-admin-multi_tenancy").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("remove-1", async () =>
        {
            await scene.ClickAsync(Button(page, "Maple", "Remove"), pauseAfter: 300);
            await page.Locator("#modalOkButton").WaitForAsync();
        });

        await scene.StepAsync("remove-2", async () =>
        {
            await ConfirmAsync(scene);
            await WaitTextAsync(page, "was removed");
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the forms.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

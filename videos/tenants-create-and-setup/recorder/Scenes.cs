using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Tenants: Create a Tenant and Set It Up", recorded in order on one fresh SaaS site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-create", CreateAsync),
        ("03-list", ListAsync),
        ("04-setup", SetupAsync),
        ("05-open", OpenAsync),
        ("06-shortcut", ShortcutAsync),
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

    /// <summary>Warms up the pages the clips open. Nothing is changed.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Tenants", "/Admin/Tenants/Create" })
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

    // Opens a Setup screen. The screen opens on the time zone of the server; the recording machine is not on Pacific time,
    // so the zone is set to America/Los_Angeles while the page loads, off camera (the screen shows no time zone of this machine).
    private static async Task GoToSetupAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y));
            await scene.Page.Locator("#SiteTimeZone").SelectOptionAsync(new SelectOptionValue { Value = "America/Los_Angeles" });
        }, leadMilliseconds: 250);
    }

    private static ILocator Group(IPage page, string key) => page.Locator($"button.nav-group-label.oc-navigation-admin-{key}");

    private static ILocator Row(IPage page, string tenant) => page.Locator("li.list-group-item").Filter(new() { Has = page.Locator($"a.text-break:text-is('{tenant}')") }).Last;

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(Group(page, "multi_tenancy"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Tenants'] >> visible=true").First);
            await GoAsync(scene, page.Locator("a.btn.create").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(page.Locator("#Name"), "Maplewood", delay: 30);
            await scene.HighlightAsync(page.Locator("#Name"));
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#Category"), "Customers", delay: 25);
            await scene.TypeAsync(page.Locator("#Description"), "Website of Maplewood Bakery", delay: 12);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.TypeAsync(page.Locator("#RequestUrlPrefix"), "maplewood", delay: 25);
            await scene.HighlightAsync(page.Locator("#RequestUrlPrefix"));
            await scene.HighlightAsync(page.Locator("#RequestUrlHost"));
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.HighlightAsync(page.Locator("#RequestUrlHost"), "Prefix, hostname, or both");
        });

        await scene.StepAsync("create-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(page.Locator("#RecipeName"), "Blog");
        });

        await scene.StepAsync("create-7", async () =>
        {
            await page.GetByText("Database Presets").First.ScrollToMiddleAsync();
            await scene.SelectAsync(page.Locator("#DatabaseProvider"), "SqlConnection");
        });

        await scene.StepAsync("create-8", async () =>
        {
            await scene.SelectAsync(page.Locator("#DatabaseProvider"), "Sqlite");
        });

        await scene.StepAsync("create-9", async () =>
        {
            var create = page.Locator("button.create").First;
            await create.ScrollToMiddleAsync();
            await scene.HighlightAsync(page.Locator("button[value='createAndSetup']").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, create);
        });
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.HighlightAsync(Row(page, "Maplewood"));

            foreach (var badge in new[] { "Customers", "Sqlite", "Blog", "Uninitialized" })
            {
                await scene.MoveToAsync(Row(page, "Maplewood").Locator(".badge").Filter(new() { HasTextString = badge }).First);
                await scene.PauseAsync(1200);
            }

            await scene.HighlightAsync(Row(page, "Maplewood").Locator(".badge").Filter(new() { HasTextString = "Uninitialized" }).First);
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, Row(page, "Maplewood").Locator("a.text-break").First, 2000);
            await scene.MoveToAsync(page.Locator("#btn-setup-Maplewood"));
            await scene.HighlightAsync(page.Locator("#btn-setup-Maplewood"));
        });
    }

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await GoToSetupAsync(scene, page.Locator("#btn-setup-Maplewood"));
            await scene.TypeAsync(page.Locator("#SiteName"), "Maplewood Bakery", delay: 25);
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.MoveToAsync(page.Locator("#SiteName"));
            await scene.MoveToAsync(page.GetByText("Super User").First);
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.SelectAsync(page.Locator("#SiteTimeZone"), "Europe/Amsterdam");
            await scene.HighlightAsync(page.Locator("#SiteTimeZone"));
        });

        await scene.StepAsync("setup-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.Locator("#UserName").ScrollToMiddleAsync();
            await scene.TypeAsync(page.Locator("#UserName"), "admin", delay: 20);
            await scene.TypeAsync(page.Locator("#Email"), "admin@example.com", delay: 12);
            await scene.TypeAsync(page.Locator("#Password"), Password, delay: 12);
            await scene.TypeAsync(page.Locator("#PasswordConfirmation"), Password, delay: 12);
        });

        await scene.StepAsync("setup-5", async () =>
        {
            await page.Locator("#SubmitButton").ScrollToMiddleAsync();
            await scene.HighlightAsync(page.Locator("#SubmitButton"));
        });

        await scene.StepAsync("setup-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(page.Locator("#SubmitButton"));
            page.SetDefaultTimeout(180_000);
            await scene.OffCameraAsync(async () =>
            {
                var (x, y) = await scene.CenterOfAsync(page.Locator("#SubmitButton"));
                await scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y));
                await page.Locator("a.navbar-brand").Filter(new() { HasTextString = "Maplewood Bakery" }).WaitForAsync();
            });
            page.SetDefaultTimeout(20_000);
        });
    }

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            await scene.HighlightAsync(Row(page, "Maplewood"));

            foreach (var button in new[] { "View", "Disable", "Reload", "Features" })
            {
                await scene.MoveToAsync(Row(page, "Maplewood").Locator($"a.btn:text-is('{button}'), button:text-is('{button}')").First);
                await scene.PauseAsync(1200);
            }
        });

        IPage popup = null;

        await scene.StepAsync("open-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var view = Row(page, "Maplewood").Locator("a.view, a.btn:text-is('View')").First;
            await scene.MoveToAsync(view);
            popup = await scene.Context.RunAndWaitForPageAsync(async () => await scene.ClickAsync(view, pauseAfter: 200));
            await popup.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await scene.SwitchToAsync(popup);
        });

        await scene.StepAsync("open-3", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => popup.GotoAsync(BaseUrl + "/maplewood/Admin")), leadMilliseconds: 250);
        });
    }

    private static async Task ShortcutAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants/Create");
        await scene.StartCaptureAsync();

        await scene.StepAsync("shortcut-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#Name"), "Riverside", delay: 25);
            await scene.TypeAsync(page.Locator("#RequestUrlPrefix"), "riverside", delay: 25);
            await scene.SelectAsync(page.Locator("#RecipeName"), "Blog", pauseAfter: 300);
            await page.Locator("#DatabaseProvider").ScrollToMiddleAsync();
            await scene.SelectAsync(page.Locator("#DatabaseProvider"), "Sqlite", pauseAfter: 300);
        });

        await scene.StepAsync("shortcut-2", async () =>
        {
            var button = page.Locator("button[value='createAndSetup']").First;
            await button.ScrollToMiddleAsync();
            await scene.MoveToAsync(button);
            var (x, y) = await scene.CenterOfAsync(button);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y));
                await page.Locator("#SiteTimeZone").SelectOptionAsync(new SelectOptionValue { Value = "America/Los_Angeles" });
            }, leadMilliseconds: 250);
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

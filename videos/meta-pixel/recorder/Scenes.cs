using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Meta Pixel", recorded in order on one fresh site.</summary>
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
        ("03-pixel", PixelAsync),
        ("05-capi", CapiAsync),
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

    /// <summary>Nothing to seed. Warms the pages up once, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Features", "/Admin/Contents/ContentItems" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Pages warmed up.");
    }

    private static async Task WaitSiteAsync()
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        for (var i = 0; i < 60; i++)
        {
            try
            {
                var response = await client.GetAsync(BaseUrl + "/");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(2000);
        }
    }

    // The recorder's browser never lets a page of the site reach Meta.
    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<Scene, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.Context.RouteAsync("**/*.facebook.net/**", route => route.AbortAsync());
        await scene.Context.RouteAsync("**/*.facebook.com/**", route => route.AbortAsync());
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static ILocator Group(IPage page, string title)
        => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator MenuLink(IPage page, string group, string text)
        => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{group}\"] a.item-label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*" + System.Text.RegularExpressions.Regex.Escape(text) + @"\s*$") });

    private static ILocator Integrations(IPage page)
        => page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] button.item-label:has(span.title:text-is(\"Integrations\"))").First;

    private static async Task OpenPixelMenuAsync(Scene scene)
    {
        await scene.ClickAsync(Group(scene.Page, "Settings"), pauseAfter: 400);
        await scene.ClickAsync(Integrations(scene.Page), pauseAfter: 400);
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

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentItems", s => s.Page.Locator("#left-nav").WaitForAsync());
        var page = scene.Page;
        var row = page.Locator("[data-filter-value]").Filter(new() { HasTextString = "OrchardCore.Facebook.Pixel" }).First;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(Group(page, "Tools"), pauseAfter: 300);
            await GoAsync(scene, MenuLink(page, "Tools", "Features").First);
            await page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "Meta Pixel", delay: 40);
            await row.WaitForAsync();
            await scene.HighlightAsync(page.GetByRole(AriaRole.Heading, new() { Name = "Meta", Exact = true }).First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.GetByText("Provides a way to enable Meta Pixel tracking for your site.").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, row.Locator("#btn-enable-OrchardCore_Facebook_Pixel"));
            await WaitSiteAsync();
            await page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "Meta Pixel", delay: 20);
            await row.Locator("#btn-disable-OrchardCore_Facebook_Pixel").WaitForAsync();
            await scene.HighlightAsync(row.Locator("#btn-disable-OrchardCore_Facebook_Pixel"));
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenPixelMenuAsync(scene);
            await scene.HighlightAsync(page.GetByRole(AriaRole.Link, new() { Name = "Meta Pixel", Exact = true }).First);
        });
    }

    private static async Task PixelAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentItems", s => s.Page.Locator("#left-nav").WaitForAsync());
        var page = scene.Page;
        var field = page.GetByLabel("Pixel Identifier", new() { Exact = true });

        await scene.StepAsync("pixel-1", async () =>
        {
            await OpenPixelMenuAsync(scene);
            await GoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Meta Pixel", Exact = true }).First);
            await field.WaitForAsync();
            await scene.HighlightAsync(field);
            await scene.HighlightAsync(page.GetByText("The Pixel ID found on the Meta Data Sources portal.").First);
        });

        await scene.StepAsync("pixel-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(field, "123456789012345", delay: 70);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("pixel-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First);
            await WaitTextAsync(page, "Site settings updated successfully.");
            await scene.HoldAsync(1.0);
        });
    }

    private static async Task CapiAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Settings/facebook-pixel", s => s.Page.GetByLabel("Pixel Identifier", new() { Exact = true }).WaitForAsync());
        var page = scene.Page;
        var token = page.GetByLabel("Conversions API Access Token", new() { Exact = true });
        var test = page.GetByLabel("Test Event Code", new() { Exact = true });

        await scene.StepAsync("capi-1", async () =>
        {
            var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Conversions API", Exact = true }).First;
            await heading.ScrollToMiddleAsync();
            await scene.HighlightAsync(heading);
        });

        await scene.StepAsync("capi-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(token, "example-access-token-0000", delay: 40);
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(token);
        });

        await scene.StepAsync("capi-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(test, "TEST12345", delay: 70);
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(test);
        });

        await scene.StepAsync("capi-4", async () =>
        {
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First);
            await WaitTextAsync(page, "Site settings updated successfully.");
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

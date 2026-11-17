using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Static files per tenant", on a site with the Default tenant (SaaS recipe) and the Maple (/bakery) and
/// Cedar (/garden) tenants, installed by Auto Setup. PrepareAsync writes the robots.txt files and enables the feature on Cedar.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The App_Data folder of the site (the tenant folders are App_Data/Sites/<Tenant>/wwwroot).
    public static string AppData { get; } = Environment.GetEnvironmentVariable("VIDEO_APPDATA") ?? throw new InvalidOperationException("Set VIDEO_APPDATA to the App_Data folder of the site.");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("04-serve", ServeAsync),
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

    /// <summary>Writes the seed files and enables the Static File Provider on Cedar, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        Directory.CreateDirectory(Path.Combine(AppData, "Sites", "Maple", "wwwroot"));
        Directory.CreateDirectory(Path.Combine(AppData, "Sites", "Cedar", "wwwroot"));
        await File.WriteAllTextAsync(Path.Combine(AppData, "Sites", "Maple", "wwwroot", "robots.txt"), "# robots.txt of Maple Bakery\nUser-agent: *\nDisallow: /drafts\n");
        await File.WriteAllTextAsync(Path.Combine(AppData, "Sites", "Cedar", "wwwroot", "robots.txt"), "# robots.txt of Cedar Garden\nUser-agent: *\nDisallow: /\n");

        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features/Cedar");
        await page.Locator("#btn-enable-OrchardCore_Tenants_FileProvider").ClickAsync();
        await page.WaitForFunctionAsync("() => document.body.innerText.includes('The feature Static File Provider was enabled.')");

        foreach (var path in new[] { "/Admin/Tenants", "/Admin/Features/Maple" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Tenant folders seeded.");
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

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(page.Locator("button.nav-group-label.oc-navigation-admin-multi_tenancy"), pauseAfter: 400);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Tenants'] >> visible=true").First);
            await page.Locator("a.text-break:text-is('Maple')").WaitForAsync();
            await scene.HighlightAsync(page.Locator("a[href*='/Admin/Features/Maple']").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a[href*='/Admin/Features/Maple']").First);
            await scene.HighlightAsync(page.Locator("#search-box"), "Features of the Maple tenant");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "static", delay: 40);
            await page.Locator("#btn-enable-OrchardCore_Tenants_FileProvider").WaitForAsync();
            await scene.HighlightAsync(page.Locator("[data-filter-value]:not(.d-none)").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_Tenants_FileProvider"));
            await WaitTextAsync(page, "The feature Static File Provider was enabled.");
            await scene.TypeAsync(page.Locator("#search-box"), "static", delay: 30);
        });
    }

    private static async Task ServeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await page.AddInitScriptAsync("document.addEventListener('DOMContentLoaded', () => { if (document.contentType === 'text/plain') document.body.style.zoom = '2.4'; });");
        await scene.OpenAsync("/bakery/robots.txt");
        await scene.StartCaptureAsync();

        await scene.StepAsync("serve-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("pre"), "/bakery/robots.txt");
        });

        await scene.StepAsync("serve-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/garden/robots.txt");
            await scene.HighlightAsync(page.Locator("pre"), "/garden/robots.txt");
        });
    }
}

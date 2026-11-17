using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Shells and configuration": the Tenants page with the Acme tenant that appsettings.json prepared.
/// The site is started with App_Data/appsettings.json already holding the Acme section (see the README).
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    /// <summary>The global tenant configuration file of the slides, to put in the App_Data folder before the first start.</summary>
    public const string AppSettings = """
        {
          "OrchardCore": {
            "Acme": {
              "State": "Uninitialized",
              "RequestUrlPrefix": "acme",
              "Category": "Demo",
              "Description": "The Acme sample site.",
              "DatabaseProvider": "Sqlite"
            }
          }
        }
        """;

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-tenants", TenantsAsync),
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

    /// <summary>Enables the Tenants feature, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#btn-enable-OrchardCore_Tenants").ClickAsync();
        await page.WaitForFunctionAsync("() => document.body.innerText.includes('was enabled')");
        await page.GotoAsync(BaseUrl + "/Admin/Tenants");
        await context.CloseAsync();
        Console.WriteLine("Tenants enabled.");
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // Clicks a link or button that loads a new page; the load is cut out of the video (see Scene.OffCameraAsync).
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
    }

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms, string caption = null)
    {
        await scene.HighlightAsync(locator, caption);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static async Task TenantsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await page.Locator("button.nav-group-label.oc-navigation-admin-multi_tenancy").ClickAsync();
        await scene.StartCaptureAsync();

        var row = page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("a.text-break:text-is('Acme')") }).Last;

        await scene.StepAsync("tenants-1", async () =>
        {
            await scene.MoveToAsync(row.Locator("a.text-break").First);
            await scene.PauseAsync(1000);
            await scene.HighlightAsync(row.Locator("span.badge", new() { HasTextString = "Demo" }).First, "Demo");
            await scene.HighlightAsync(row.Locator("span.badge", new() { HasTextString = "Sqlite" }).First, "Sqlite");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, row.Locator("span.badge", new() { HasTextString = "Uninitialized" }).First, 1500, "Uninitialized");
            await scene.HighlightAsync(page.Locator("#btn-setup-Acme"));
        });

        await scene.StepAsync("tenants-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, row.Locator("a.edit").First);
            await ShowAsync(scene, page.Locator("#RequestUrlPrefix"), 1500, "Url Prefix");
            await ShowAsync(scene, page.Locator("#Description"), 1500);
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(page.Locator("#DatabaseProvider"), "Database provider");
        });
    }
}

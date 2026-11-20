using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Settings, General". Record in one take, in order: the clips save settings (site name, time zone, page size)
/// and the next ones start from that state. PrepareAsync seeds ten fake articles so the Content Items list has two pages.
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
        ("02-open", OpenAsync),
        ("03-site", SiteAsync),
        ("04-zone", ZoneAsync),
        ("05-size", SizeAsync),
        ("06-resources", ResourcesAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var items = Enumerable.Range(1, 10).Select(n => new Dictionary<string, object>
        {
            ["ContentType"] = "Article",
            ["ContentItemId"] = Guid.NewGuid().ToString("N")[..26],
            ["ContentItemVersionId"] = Guid.NewGuid().ToString("N")[..26],
            ["DisplayText"] = $"Harbor Lane Note {n}",
            ["Latest"] = true,
            ["Published"] = true,
            ["Author"] = "admin",
            ["TitlePart"] = new Dictionary<string, string> { ["Title"] = $"Harbor Lane Note {n}" },
        }).ToList();
        var json = JsonSerializer.Serialize(new { steps = new[] { new { name = "content", data = items } } });

        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Seeded ten articles.");
        await context.CloseAsync();
    }

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

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task SaveAsync(Scene scene)
    {
        var save = scene.Page.Locator("button.save").First;
        await scene.MoveToAsync(save);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(() => save.ClickAsync());
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        });
    }

    private static async Task GeneralPageAsync(Scene scene, string path = "/Admin/Settings/general")
    {
        await scene.OpenAsync(path);
        await scene.Page.Locator("input[name$='SiteName']").WaitForAsync();
    }

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            var settings = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact("Settings") }).First;

            if (await settings.GetAttributeAsync("aria-expanded") == "false")
            {
                await scene.ClickAsync(settings, pauseAfter: 700);
            }

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = Exact("General") }).First);
            await scene.Page.Locator("input[name$='SiteName']").WaitForAsync();
        });

        await scene.StepAsync("open-2", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("h1").First, 1.5);
            await scene.HighlightAsync(scene.Page.Locator("p.alert.alert-warning").First);
        });

        await scene.StepAsync("open-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, scene.Page.Locator("a.nav-link[role='tab']").First.Locator("xpath=ancestor::ul[1]"), 1.5);
            await scene.HighlightAsync(scene.Page.Locator("button.save").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await GeneralPageAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            var field = scene.Page.Locator("input[name$='SiteName']").First;
            await scene.ClickAsync(field);
            await field.FillAsync(string.Empty);
            await scene.TypeAsync(field, "Harbor Lane Journal");
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("label:text-is('Page title format')").First);
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label:text-is('Base url')").First);
        });

        await scene.StepAsync("site-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("a.ta-navbar-brand").First);
        });

        await scene.StepAsync("site-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var visit = scene.Page.Locator(".user-top-navbar a[aria-label='Visit Site']").First;
            await scene.MoveToAsync(visit);
            await scene.HighlightAsync(visit, "Visit Site");
            await scene.HoldAsync(1);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(visit);
            await scene.HighlightAsync(scene.Page.Locator("a.navbar-brand").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ZoneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await GeneralPageAsync(scene);
        await scene.StartCaptureAsync();

        var zone = scene.Page.Locator("select[name$='TimeZone']").First;

        await scene.StepAsync("zone-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("label:text-is('Default Time Zone')").First);
            await scene.HighlightAsync(zone.Locator("xpath=following::*[contains(@class,'hint')][1]"));
        });

        await scene.StepAsync("zone-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(zone);
            await scene.PauseAsync(8000);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(zone, "Local to server");
        });

        await scene.StepAsync("zone-3", async () =>
        {
            await zone.SelectOptionAsync("Europe/London");
            await scene.HoldAsync(0.8);
            await SaveAsync(scene);
        });

        await scene.StepAsync("zone-4", async () =>
        {
            await scene.HighlightAsync(zone);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SizeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.Page.Locator("h1.oc-breadcrumb-title").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("size-1", async () =>
        {
            await scene.HoldAsync(3);
            await scene.ScrollAsync(0, 1200);
        });

        await scene.StepAsync("size-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/general");
            var field = scene.Page.Locator("input[name$='PageSize']").First;
            await scene.ClickAsync(field);
            await field.FillAsync(string.Empty);
            await scene.TypeAsync(field, "5");
            await SaveAsync(scene);
        });

        await scene.StepAsync("size-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems");
            await scene.Page.Locator("h1.oc-breadcrumb-title").WaitForAsync();
            await scene.HoldAsync(0.5);
            await scene.ScrollAsync(0, 1200);
        });

        await scene.StepAsync("size-4", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/general");
            await scene.PauseAsync(3000);
            await scene.ClickAsync(scene.Page.Locator("input[name$='AllowPageSizeSelection'][type='checkbox']").First);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='PageSizeOptions']").First);
        });

        await scene.StepAsync("size-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var field = scene.Page.Locator("input[name$='PageSizeOptions']").First;
            await scene.ClickAsync(field);
            await field.FillAsync(string.Empty);
            await scene.TypeAsync(field, "25, 5, 10");
            await SaveAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='PageSizeOptions']").First);
        });

        await scene.StepAsync("size-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Contents/ContentItems");
            await scene.Page.Locator("div.pager-page-size select").First.WaitForAsync();
            await scene.ScrollAsync(0, 1200);
            var select = scene.Page.Locator("div.pager-page-size select").First;
            await scene.HighlightAsync(select);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => select.SelectOptionAsync(new SelectOptionValue { Label = "10" }));
            await scene.Page.Locator("div.pager-page-size select").First.WaitForAsync();
            await scene.ScrollAsync(0, 1200);
        });
    }

    private static async Task ResourcesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await GeneralPageAsync(scene);
        await scene.StartCaptureAsync();

        ILocator Tab(string name) => scene.Page.Locator("a.nav-link[role='tab']").Filter(new() { HasTextRegex = Exact(name) }).First;

        await scene.StepAsync("res-1", async () =>
        {
            await scene.ClickAsync(Tab("Resources"));
            await ShowAsync(scene, scene.Page.GetByText("Settings for scripts and stylesheets").First, 1.5);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='AppendVersion'][type='checkbox']").First);
        });

        await scene.StepAsync("res-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label:has-text('Use framework CDN')").First);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='UseCdn'][type='checkbox']").First);
        });

        await scene.StepAsync("res-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label:text-is('Site CDN (Content Delivery Network) base url')").First);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='CdnBaseUrl']").First);
        });

        await scene.StepAsync("res-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("select[name$='ResourceDebugMode']").First);
        });

        await scene.StepAsync("res-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Tab("Cache"));
            await ShowAsync(scene, scene.Page.GetByText("Settings for dynamic caches").First, 1.5);
            await scene.HighlightAsync(scene.Page.Locator("select[name$='CacheMode']").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

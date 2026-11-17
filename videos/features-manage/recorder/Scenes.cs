using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Features, enable and disable what your site uses". Record in one take, in order: 04-enable enables Background
/// Tasks, 05-disable disables it again, 06-bulk enables the Google Analytics and Tag Manager features.
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
        ("02-page", PageAsync),
        ("03-filter", FilterAsync),
        ("04-enable", EnableAsync),
        ("05-disable", DisableAsync),
        ("06-bulk", BulkAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static Task CenterAsync(ILocator locator) => locator.EvaluateAsync("e => e.scrollIntoView({block: 'center'})");

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static ILocator Row(Scene scene, string id)
        => scene.Page.Locator($"li[data-filter-value]:has(code:text-is(\"{id}\"))").First;

    private static async Task OpenFeaturesAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
    }

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("page-1", async () =>
        {
            await scene.PauseAsync(500);
            var tools = scene.Page.Locator("#left-nav button.nav-group-label").Filter(new() { HasTextRegex = Exact("Tools") }).First;

            if (await tools.GetAttributeAsync("aria-expanded") == "false")
            {
                await scene.ClickAsync(tools, pauseAfter: 250);
            }

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.item-label:has(span.title:text-is('Features'))").First);
            await scene.Page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("page-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".feature-group:not(.d-none) h3").First);
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#list-summary").First);
        });

        await scene.StepAsync("page-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = scene.Page.Locator("li[data-filter-value]:not(.d-none):has(.direct-dependencies .badge)").First;
            await CenterAsync(row);
            await scene.HoldAsync(0.4);
            await ShowAsync(scene, row.Locator("span.fw-semibold").First, 2.6);
            await ShowAsync(scene, row.Locator("code").First, 2.0);
            await ShowAsync(scene, row.Locator(".direct-dependencies").First, 2.6);
            await ShowAsync(scene, row.Locator(".metadata .ta-badge").First, 2.4);
            await scene.HighlightAsync(row.Locator(".admin-list-secondary").First);
        });

        await scene.StepAsync("page-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = scene.Page.Locator("li[data-is-enabled='false']:not(.d-none) a.btn-primary").First;
            await CenterAsync(enable);
            await scene.HighlightAsync(enable);
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            var disable = scene.Page.Locator("li[data-is-enabled='true']:not(.d-none) a.btn-danger").First;
            await CenterAsync(disable);
            await scene.HighlightAsync(disable);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FilterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenFeaturesAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#search-box").First);
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "background tasks", delay: 90);
        });

        await scene.StepAsync("find-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#list-summary").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#search-box").First, pauseAfter: 300);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("find-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[data-id='status-filter']").First);
            await scene.PauseAsync(3800);
            await scene.ClickAsync(scene.Page.Locator(".bootstrap-select:has(button[data-id='status-filter']) span.text:text-is('Enabled only')").First);
        });

        await scene.StepAsync("find-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[data-id='visibility-filter']").First);
            await scene.PauseAsync(3000);
            await scene.HighlightAsync(scene.Page.Locator(".bootstrap-select:has(button[data-id='visibility-filter']) span.text:text-is('Hide on demand')").First);
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".bootstrap-select:has(button[data-id='visibility-filter']) span.text:text-is('Hide always enabled')").First);
        });

        await scene.StepAsync("find-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(600);
            await scene.ClickAsync(scene.Page.Locator(".bootstrap-select:has(button[data-id='visibility-filter']) span.text:text-is('Hide always enabled')").First, pauseAfter: 300);
            await scene.ClickAsync(scene.Page.Locator("#list-summary").First, pauseAfter: 300);
            var row = Row(scene, "OrchardCore.Features");
            await CenterAsync(row);
            await scene.HoldAsync(0.5);
            var wrapper = row.Locator("span.d-inline-block[data-bs-toggle='tooltip']").First;
            await scene.HighlightAsync(wrapper);
            await scene.PauseAsync(2500);
            await scene.MoveToAsync(wrapper);
            await scene.Page.Locator(".tooltip.show").First.WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenFeaturesAsync(scene);
        await scene.StartCaptureAsync();

        const string Enable = "#btn-enable-OrchardCore_BackgroundTasks";

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "background tasks", delay: 80);
            var row = Row(scene, "OrchardCore.BackgroundTasks");
            await row.WaitForAsync();
            await scene.HighlightAsync(row.Locator(".metadata .ta-badge").First);
            await scene.PauseAsync(1600);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(Enable).First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(Enable).First);
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        });

        await scene.StepAsync("enable-3", async () =>
        {
            var entry = scene.Page.Locator("#left-nav a.item-label:has(span.title:text-is('Background Tasks'))").First;
            await entry.WaitForAsync();
            await scene.HighlightAsync(entry);
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "background tasks", delay: 80);
            await scene.HighlightAsync(scene.Page.Locator("#btn-disable-OrchardCore_BackgroundTasks").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DisableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenFeaturesAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("disable-1", async () =>
        {
            await scene.PauseAsync(300);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "background tasks", delay: 30);
            await scene.ClickAsync(scene.Page.Locator("#btn-disable-OrchardCore_BackgroundTasks").First, pauseAfter: 400);
            await scene.Page.Locator("#confirmRemoveModal .modal-title").First.WaitForAsync();
        });

        await scene.StepAsync("disable-2", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("#confirmRemoveModal .modal-title").First, 3.2);
            await ShowAsync(scene, scene.Page.Locator("#modalOkButton").First, 1.6);
            await scene.HighlightAsync(scene.Page.Locator("#modalCancelButton").First);
        });

        await scene.StepAsync("disable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton").First);
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#left-nav li.nav-group:has(button.nav-group-label:has-text('Tools')) ul.nav-group-items").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BulkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenFeaturesAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("bulk-1", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("li[data-filter-value]:not(.d-none) input[name='featureIds']").First, 4.0);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "google", delay: 90);
            await Row(scene, "OrchardCore.Google.Analytics").WaitForAsync();
            await scene.PauseAsync(800);
            await scene.ClickAsync(scene.Page.Locator("input[name='featureIds'][value='OrchardCore.Google.Analytics']").First, pauseAfter: 400);
            await scene.PauseAsync(500);
            await scene.ClickAsync(scene.Page.Locator("input[name='featureIds'][value='OrchardCore.Google.TagManager']").First, pauseAfter: 400);
        });

        await scene.StepAsync("bulk-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button").First, pauseAfter: 200);
            await ShowAsync(scene, scene.Page.Locator("a[data-action='Disable']").First, 1.1);
            await ShowAsync(scene, scene.Page.Locator("a[data-action='Enable']").First, 1.6);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[data-action='Enable']").First);
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        });

        await scene.StepAsync("bulk-3", async () =>
        {
            await scene.PauseAsync(3000);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "google", delay: 90);

            foreach (var id in new[] { "OrchardCore.Google.Analytics", "OrchardCore.Google.TagManager", "OrchardCore.Google.GoogleAuthentication" })
            {
                await ShowAsync(scene, Row(scene, id).Locator(".metadata .ta-badge").First, 1.8, clear: id != "OrchardCore.Google.GoogleAuthentication");
            }
        });

        await scene.ClearHighlightsAsync();
    }
}

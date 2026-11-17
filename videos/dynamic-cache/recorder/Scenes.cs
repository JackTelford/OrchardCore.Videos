using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Dynamic Cache". The site runs in the Development environment. PrepareAsync imports the template
/// Content__Article (seed-template.json, next to this file in the repository). Record 02-mode, then 04-watch on the site it leaves.
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
        ("02-mode", ModeAsync),
        ("04-watch", WatchAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var json = File.ReadAllText(Path.Combine(Root, "recorder", "seed-template.json"));
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Imported the template.");
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

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task ShowCacheLinesAsync(Scene scene)
    {
        var rendered = scene.Page.Locator("p:has-text('Rendered at')").First;
        await rendered.WaitForAsync();
        await rendered.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
        await scene.HoldAsync(0.3);
        await scene.HighlightAsync(rendered);
        await scene.HighlightAsync(scene.Page.Locator("p:has-text('Cached at')").First);
    }

    private static async Task OpenCacheTabAsync(Scene scene)
    {
        await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Tab, new() { Name = "Cache", Exact = true }).First);
        await scene.Page.EvaluateAsync("() => { const label = [...document.querySelectorAll('label')].find(l => l.textContent.trim() === 'Cache Mode'); document.getElementById(label.htmlFor).size = 4; }");
        await scene.HighlightAsync(scene.Page.GetByLabel("Cache Mode").First);
    }

    private static async Task ModeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("mode-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Dynamic Cache", delay: 30);
            await scene.Page.Locator("li[data-filter-value]:has(span.fw-semibold:text-is('Dynamic Cache'))").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".feature-group:visible h3").First);
            await scene.HighlightAsync(scene.Page.Locator("li[data-filter-value]:has(span.fw-semibold:text-is('Dynamic Cache'))").First);
        });

        await scene.StepAsync("mode-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.NavigateAsync("/Admin/Settings/general"), leadMilliseconds: 100);
            await OpenCacheTabAsync(scene);
        });

        await scene.StepAsync("mode-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.GetByLabel("Cache Mode").First, "Enabled");
            var save = scene.Page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.Locator("input[name$='SiteName']").First.WaitForAsync();
            }, leadMilliseconds: 150);
            await OpenCacheTabAsync(scene);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WatchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/about");
        await scene.Page.Locator("p:has-text('Cached at')").First.WaitForAsync();
        await scene.Page.Locator("p:has-text('Rendered at')").First.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
        await scene.StartCaptureAsync();

        await scene.StepAsync("watch-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("p:has-text('Rendered at')").First);
            await scene.HighlightAsync(scene.Page.Locator("p:has-text('Cached at')").First);
        });

        await scene.StepAsync("watch-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(2000);
            await scene.OffCameraAsync(() => scene.NavigateAsync("/about"), leadMilliseconds: 100);
            await ShowCacheLinesAsync(scene);
        });

        await scene.StepAsync("watch-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.NavigateAsync("/Admin/Contents/ContentItems"), leadMilliseconds: 100);
            var edit = scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "About" }).First.Locator("a.edit").First;
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => edit.ClickAsync()), leadMilliseconds: 100);
            var publish = scene.Page.Locator("button.publish").First;
            await scene.MoveToAsync(publish);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => publish.ClickAsync()), leadMilliseconds: 100);
        });

        await scene.StepAsync("watch-4", async () =>
        {
            await scene.OffCameraAsync(() => scene.NavigateAsync("/about"), leadMilliseconds: 100);
            await ShowCacheLinesAsync(scene);
        });

        await scene.ClearHighlightsAsync();
    }
}

using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

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
        ("03-default", DefaultAsync),
        ("04-settings", SettingsAsync),
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

    // A short cursor path: on a loaded machine every pointer move is a slow round trip, and the clicks must land on their sentence.
    private static async Task ClickAsync(Scene scene, ILocator locator, int pauseAfter = 400)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 12);
        await Task.Delay(100);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await Task.Delay(pauseAfter);
    }

    // Clicks something that navigates; the slow page load is cut out of the clip.
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 12);
        await Task.Delay(100);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 400);
        await Task.Delay(200);
    }

    // Holds until the given second of the step, so a click lands on its sentence of the narration.
    private static Task AtAsync(Scene scene, Stopwatch step, double seconds)
        => scene.HoldAsync(Math.Max(0, seconds - step.Elapsed.TotalSeconds));

    private static ILocator Group(IPage page, string title) => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator MenuLink(IPage page, string group, string text) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{group}\"] a.item-label:has-text(\"{text}\")");

    private static ILocator Notice(IPage page, string text) => page.Locator(".message, .alert, .toast").Filter(new() { HasText = text }).Last;

    private static async Task TypeIntoAsync(Scene scene, ILocator field, string text, int delay = 3)
    {
        await field.WaitForAsync();
        await ClickAsync(scene, field, 400);
        await field.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = delay });
        await Task.Delay(300);

        if (await field.InputValueAsync() != text)
        {
            await field.FillAsync(text);
            await Task.Delay(300);
        }
    }

    // The plain-text page is drawn small: draw its lines larger so they read in the 1080p frame (the file is unchanged).
    private static async Task ZoomAsync(IPage page)
    {
        await page.EvaluateAsync("() => { const pre = document.querySelector('pre'); pre.style.fontSize = '36px'; pre.style.display = 'inline-block'; pre.style.margin = '40px'; pre.style.padding = '10px 24px'; }");
        await Task.Delay(300);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        var row = page.Locator("[data-filter-value^=\"SEO OrchardCore.Seo \"]");

        await scene.StepAsync("enable-1", async () =>
        {
            await ClickAsync(scene, Group(page, "Tools"), 300);
            await GoAsync(scene, MenuLink(page, "Tools", "Features"));
            var search = page.Locator("#search-box");
            await search.WaitForAsync();
            await scene.PauseAsync(600);
            await TypeIntoAsync(scene, search, "SEO", 40);
            await row.WaitForAsync();
            await scene.PauseAsync(500);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var step = Stopwatch.StartNew();
            var link = row.GetByRole(AriaRole.Link, new() { Name = "Enable", Exact = true });
            await scene.HighlightAsync(row);
            await AtAsync(scene, step, 2.5);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(link);
            await AtAsync(scene, step, 8.0);
            await GoAsync(scene, link);
            await scene.OffCameraAsync(() => page.WaitForSelectorAsync("text=was enabled"), 300);
            var notice = Notice(page, "was enabled");
            await scene.MoveToAsync(notice);
            await scene.HighlightAsync(notice);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DefaultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("default-1", async () =>
        {
            await scene.HoldAsync(3.5);
            await scene.OffCameraAsync(() => scene.NavigateAsync("/robots.txt"), 300);
            await page.WaitForSelectorAsync("text=User-agent");
            await ZoomAsync(page);
        });

        await scene.StepAsync("default-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("pre"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        ILocator Check(string text) => page.Locator(".form-check").Filter(new() { HasText = text }).First;

        await scene.StepAsync("settings-1", async () =>
        {
            await ClickAsync(scene, Group(page, "Settings"), 300);
            var search = page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] button.item-label:has(span.title:text-is(\"Search\"))");
            await ClickAsync(scene, search, 300);
            var seo = page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] ul[data-title=\"Search\"] button.item-label:has(span.title:text-is(\"Search Engine Optimization\"))");
            await ClickAsync(scene, seo, 300);
            var robots = page.Locator("#left-nav ul[data-title=\"Search Engine Optimization\"] a.item-label:has(span.title:text-is(\"Robots\"))");
            await GoAsync(scene, robots);
            await page.Locator("#robotsSettings_AllowAll, .form-check").First.WaitForAsync();
            await scene.PauseAsync(500);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            var box = Check("Add a rule to allow all web robots.");
            await box.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(box);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Check("Add a rule to disallow web robots to the admin routes."));
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var area = page.GetByLabel("Additional rules");
            await TypeIntoAsync(scene, area, "Disallow: /drafts/", 3);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("settings-5", async () =>
        {
            var step = Stopwatch.StartNew();
            var save = page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await AtAsync(scene, step, 1.5);
            await GoAsync(scene, save);
            await scene.OffCameraAsync(() => page.WaitForSelectorAsync("text=Site settings updated successfully."), 300);
            var notice = Notice(page, "Site settings updated successfully.");
            await scene.MoveToAsync(notice);
            await scene.HighlightAsync(notice);
        });

        await scene.StepAsync("settings-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1500);
            await scene.OffCameraAsync(() => scene.NavigateAsync("/robots.txt"), 300);
            await page.WaitForSelectorAsync("text=drafts");
            await ZoomAsync(page);
            await scene.HighlightAsync(page.Locator("pre"));
        });

        await scene.ClearHighlightsAsync();
    }
}

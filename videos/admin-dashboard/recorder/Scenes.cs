using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Admin dashboard". Order: 02-enable (enables the feature), 03-add (creates "Welcome, editors"), then
/// "Recorder prepare" (three more fake widgets, outside the capture), then 04-arrange.
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
        ("02-enable", EnableAsync),
        ("03-add", AddAsync),
        ("04-arrange", ArrangeAsync),
    ];

    // The three seed widgets, created through a JSON import (off camera).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var widgets = new[]
        {
            ("Style reminders", "Use sentence case in headlines. Add alt text to every image.", 1, 2, 1, true),
            ("Office hours", "The editorial desk answers questions on weekdays from 9:00 to 17:00.", 2, 1, 1, true),
            ("Holiday schedule", "Share your days off with the desk by the end of the month.", 3, 1, 1, false),
        }.Select(w => new Dictionary<string, object>
        {
            ["ContentType"] = "HtmlDashboardWidget",
            ["ContentItemId"] = Guid.NewGuid().ToString("N")[..26],
            ["ContentItemVersionId"] = Guid.NewGuid().ToString("N")[..26],
            ["DisplayText"] = w.Item1,
            ["Latest"] = true,
            ["Published"] = w.Item6,
            ["Author"] = "admin",
            ["TitlePart"] = new Dictionary<string, string> { ["Title"] = w.Item1 },
            ["HtmlBodyPart"] = new Dictionary<string, string> { ["Html"] = $"<p>{w.Item2}</p>" },
            ["DashboardPart"] = new Dictionary<string, int> { ["Position"] = w.Item3, ["Width"] = w.Item4, ["Height"] = w.Item5 },
        }).ToList();
        var json = JsonSerializer.Serialize(new { steps = new[] { new { name = "content", data = widgets } } });
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Seeded three widgets.");
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

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await ShowAsync(scene, scene.Page.GetByText("Welcome to Orchard Core").First, 1.2);

            foreach (var title in new[] { "Create content", "Customize theme", "Manage users", "Learn more" })
            {
                await scene.MoveToAsync(scene.Page.Locator("h5.card-title").Filter(new() { HasTextString = title }).First);
                await scene.HoldAsync(0.6);
            }
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var tools = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact("Tools") }).First;

            if (await tools.GetAttributeAsync("aria-expanded") == "false")
            {
                await scene.ClickAsync(tools, pauseAfter: 400);
            }

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a[href$='/Admin/Features']").First);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Admin Dashboard", delay: 55);
            await scene.Page.Locator("a[id^='btn-enable-'][href*='OrchardCore.AdminDashboard']").First.WaitForAsync();
        });

        await scene.StepAsync("enable-3", async () =>
        {
            var enable = scene.Page.Locator("a[id^='btn-enable-'][href*='OrchardCore.AdminDashboard']").First;
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(enable);
                await scene.Page.GetByText("was enabled").First.WaitForAsync();
            }, leadMilliseconds: 300);
        });

        await scene.StepAsync("enable-4", async () =>
        {
            var brand = scene.Page.Locator("a.ta-navbar-brand").First;
            await scene.MoveToAsync(brand);
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(brand);
                await scene.Page.Locator("h1").Filter(new() { HasTextString = "Dashboard" }).First.WaitForAsync();
            }, leadMilliseconds: 300);
        });
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("h1").Filter(new() { HasTextString = "Dashboard" }).First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn.btn-primary.btn-sm").Filter(new() { HasTextString = "Manage Dashboard" }).First);
            await ShowAsync(scene, scene.Page.Locator("a.btn.btn-success").First, 1.5);
            await scene.HighlightAsync(scene.Page.Locator("button#new-dropdown").First);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button#new-dropdown").First);
            var item = scene.Page.Locator("a.add-list-widget").First;
            await item.WaitForAsync();
            await ShowAsync(scene, item, 1.2);
            await scene.ClickAndWaitForNavigationAsync(item);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.TypeAsync(scene.Page.GetByLabel("Title", new() { Exact = true }).First, "Welcome, editors", delay: 40);
            var editor = scene.Page.Locator("div.trumbowyg-editor").First;
            await editor.WaitForAsync();
            await scene.TypeAsync(editor, "Welcome to the editorial desk. Draft in your own section, and ask a colleague to review before you publish.", delay: 7);
        });

        await scene.StepAsync("add-4", async () =>
        {
            var position = scene.Page.GetByLabel("Position", new() { Exact = true }).First;
            await position.ScrollIntoViewIfNeededAsync();

            foreach (var (label, value) in new[] { ("Position", "0"), ("Width", "2"), ("Height", "1") })
            {
                var field = scene.Page.GetByLabel(label, new() { Exact = true }).First;
                await scene.ClickAsync(field, pauseAfter: 150);
                await scene.Page.Keyboard.PressAsync("Control+A");
                await field.PressSequentiallyAsync(value, new() { Delay = 80 });
                await scene.Page.Keyboard.PressAsync("Tab");
            }

            await scene.HighlightAsync(scene.Page.GetByLabel("Position", new() { Exact = true }).First);
            await scene.HighlightAsync(scene.Page.GetByLabel("Width", new() { Exact = true }).First);
            await scene.HighlightAsync(scene.Page.GetByLabel("Height", new() { Exact = true }).First);
            await scene.HoldAsync(1);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("add-5", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("button.draft").First, 1.2);
            var publish = scene.Page.Locator("button.publish").First;
            await scene.MoveToAsync(publish);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(publish), leadMilliseconds: 150);
            await scene.Page.Locator(".dashboard-wrapper").First.WaitForAsync();
        });
    }

    private static ILocator Widget(Scene scene, string title)
        => scene.Page.Locator(".dashboard-wrapper").Filter(new() { HasTextString = title }).First;

    private static async Task ArrangeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/dashboard/manage");
        await Widget(scene, "Office hours").WaitForAsync();
        await scene.Page.EvaluateAsync("() => window.dispatchEvent(new Event('resize'))");
        await scene.StartCaptureAsync();

        await scene.StepAsync("arrange-1", async () =>
        {
            await scene.DragToAsync(Widget(scene, "Office hours").Locator(".dashboard-handle").First, Widget(scene, "Welcome, editors"), -150, -40);
        });

        await scene.StepAsync("arrange-2", async () =>
        {
            await scene.Page.Locator("#dashboard-undo-message:not(.d-none)").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#dashboard-undo-message a").First);
        });

        await scene.StepAsync("arrange-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var handle = Widget(scene, "Style reminders").Locator(".dashboard-resize-handle-se").First;
            var (x, y) = await scene.CenterOfAsync(handle);
            await scene.DragAsync(handle, x + 60, y + 120);
        });

        await scene.StepAsync("arrange-4", async () =>
        {
            await scene.HoldAsync(1);
        });

        await scene.StepAsync("arrange-5", async () =>
        {
            await scene.ClickAsync(Widget(scene, "Welcome, editors").Locator("div.btn-group[title='Actions'] > button").First);
        });

        await scene.StepAsync("arrange-6", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClearHighlightsAsync();
            var holiday = Widget(scene, "Holiday schedule");
            await holiday.ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(holiday.Locator("div.btn-group[title='Actions'] > button").First);
        });

        await scene.StepAsync("arrange-7", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ScrollAsync(0, -2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn.btn-success").First);
        });
    }
}

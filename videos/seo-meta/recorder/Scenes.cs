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
        ("03-defaults", DefaultsAsync),
        ("04-part", PartAsync),
        ("05-page", PageAsync),
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

    // Scrolls so the element's top sits at the given distance from the top of the window.
    private static async Task ScrollToAsync(ILocator locator, int top)
    {
        await locator.EvaluateAsync("(e, top) => { const r = e.getBoundingClientRect(); window.scrollBy(0, r.top - top); }", top);
        await Task.Delay(300);
    }

    private static ILocator Group(IPage page, string title) => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator MenuLink(IPage page, string group, string text) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{group}\"] a.item-label:has-text(\"{text}\")");

    private static ILocator Notice(IPage page, string text) => page.Locator(".message, .alert, .toast").Filter(new() { HasText = text }).Last;

    private static async Task TypeIntoAsync(Scene scene, ILocator field, string text, int delay = 30)
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

    private static ILocator Wrapper(IPage page, string label) => page.Locator("div.field-wrapper").Filter(new() { HasText = label }).Last;

    // Picks a picture in the Select Media dialog of the media field.
    private static async Task PickPictureAsync(Scene scene, ILocator wrapper, string file)
    {
        var page = scene.Page;
        await ClickAsync(scene, wrapper.Locator("button:has-text(\"Add media\")"), 300);
        await page.Locator(".mf-picker-shell").WaitForAsync();
        await scene.OffCameraAsync(() => page.Locator($".mf-picker-shell div.table-row.file-item:has-text(\"{file}\")").WaitForAsync(), 300);
        await ClickAsync(scene, page.Locator($".mf-picker-shell div.table-row.file-item:has-text(\"{file}\") input[name=\"select-file\"]"), 400);
        await ClickAsync(scene, page.Locator(".mf-picker-shell .mf-modal-footer .mf-btn-primary"), 300);
        await page.Locator(".mf-picker-shell").WaitForAsync(new() { State = WaitForSelectorState.Detached });
        await wrapper.Locator("li.mf-thumb-item img").First.WaitForAsync();
    }

    private static async Task MediaTextAsync(Scene scene, ILocator wrapper, string text)
    {
        var page = scene.Page;
        var button = wrapper.Locator(".media-field-app").GetByRole(AriaRole.Button, new() { Name = "Media text" });

        if (!await button.IsVisibleAsync())
        {
            await ClickAsync(scene, wrapper.Locator("li.mf-thumb-item").First, 400);
        }

        await ClickAsync(scene, button, 300);
        await page.Locator(".mf-form-textarea").WaitForAsync();
        await TypeIntoAsync(scene, page.Locator(".mf-form-textarea"), text);
        await ClickAsync(scene, page.Locator(".mf-modal .mf-modal-footer .mf-btn-primary"), 300);
        await page.Locator(".mf-form-textarea").WaitForAsync(new() { State = WaitForSelectorState.Detached });
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await ClickAsync(scene, Group(page, "Tools"), 300);
            await GoAsync(scene, MenuLink(page, "Tools", "Features"));
            var search = page.Locator("#search-box");
            await search.WaitForAsync();
            await scene.PauseAsync(600);
            await TypeIntoAsync(scene, search, "SEO", 40);
            await page.Locator("[data-filter-value^=\"SEO OrchardCore.Seo \"]").WaitForAsync();
            await scene.PauseAsync(500);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var step = Stopwatch.StartNew();
            var link = page.Locator("[data-filter-value^=\"SEO OrchardCore.Seo \"]").GetByRole(AriaRole.Link, new() { Name = "Enable", Exact = true });
            await scene.MoveToAsync(link);
            await AtAsync(scene, step, 4.0);
            await GoAsync(scene, link);
            await scene.OffCameraAsync(() => page.WaitForSelectorAsync("text=was enabled"), 300);
            var notice = Notice(page, "was enabled");
            await scene.MoveToAsync(notice);
            await scene.HighlightAsync(notice);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DefaultsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("defaults-1", async () =>
        {
            await ClickAsync(scene, Group(page, "Settings"), 300);
            await GoAsync(scene, MenuLink(page, "Settings", "Social Meta Settings"));
            await page.Locator("div.field-wrapper").Filter(new() { HasText = "Default Meta Description" }).Locator(".CodeMirror").WaitForAsync();
        });

        await scene.StepAsync("defaults-2", async () =>
        {
            var wrapper = Wrapper(page, "Default Meta Description");
            await ClickAsync(scene, wrapper.Locator(".CodeMirror"), 300);
            await page.Keyboard.TypeAsync("A demo blog built with Orchard Core.", new() { Delay = 30 });
        });

        await scene.StepAsync("defaults-3", async () =>
        {
            var step = Stopwatch.StartNew();
            await AtAsync(scene, step, 1.5);
            await scene.ScrollAsync(0, 180, 20);
        });

        await scene.StepAsync("defaults-4", async () =>
        {
            var step = Stopwatch.StartNew();
            var image = Wrapper(page, "Default social image");
            await ScrollToAsync(page.Locator("h5:text-is(\"Social\")").First, 100);
            await scene.HighlightAsync(page.Locator("h5:text-is(\"Social\")").First);
            await AtAsync(scene, step, 5.0);
            await scene.ClearHighlightsAsync();
            await PickPictureAsync(scene, image, "home-bg.jpg");
        });

        await scene.StepAsync("defaults-5", async () =>
        {
            var step = Stopwatch.StartNew();
            await MediaTextAsync(scene, Wrapper(page, "Default social image"), "Banner picture of the demo blog");
            await AtAsync(scene, step, 7.5);
            await ClickAsync(scene, page.Locator("button.save").First, 100);
            await scene.OffCameraAsync(() => page.WaitForSelectorAsync("text=Site settings updated successfully"), 300);
            await scene.HighlightAsync(Notice(page, "Site settings updated successfully"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("part-1", async () =>
        {
            await ClickAsync(scene, Group(page, "Design"), 300);
            var definition = page.Locator("#left-nav ul.nav-group-items[data-title=\"Design\"] button.item-label:has(span.title:text-is(\"Content Definition\"))");
            await ClickAsync(scene, definition, 300);
            await GoAsync(scene, page.Locator("#left-nav ul.nav-group-items[data-title=\"Design\"] a.item-label:has-text(\"Content Types\")"));
            await page.Locator("[role='btn-edit-Article']").WaitForAsync();
            await GoAsync(scene, page.Locator("[role='btn-edit-Article']"));
            await page.Locator("a.btn:has-text(\"Add Parts\")").WaitForAsync();
            await scene.ScrollAsync(0, 1400, 24);
        });

        await scene.StepAsync("part-2", async () =>
        {
            var step = Stopwatch.StartNew();
            await GoAsync(scene, page.Locator("a.btn:has-text(\"Add Parts\")").First);
            var row = page.Locator("li.list-group-item").Filter(new() { HasText = "SeoMeta" });
            await row.First.WaitForAsync();
            await scene.HighlightAsync(row.First);
            await AtAsync(scene, step, 6.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, row.First.GetByRole(AriaRole.Button, new() { Name = "Add & Configure" }));
            await scene.MoveToAsync(900, 400, 8);
        });

        await scene.StepAsync("part-3", async () =>
        {
            var step = Stopwatch.StartNew();
            await AtAsync(scene, step, 5.2);
            foreach (var label in new[] { "Display Keywords", "Display Custom Meta Tags", "Display Open Graph", "Display Twitter", "Display Google Schema" })
            {
                await scene.HighlightAsync(page.GetByText(label, new() { Exact = true }).First);
                await scene.PauseAsync(1050);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("part-4", async () =>
        {
            var step = Stopwatch.StartNew();
            await ClickAsync(scene, page.GetByLabel("Display Open Graph"), 300);
            await ClickAsync(scene, page.GetByLabel("Display Twitter"), 300);
            await AtAsync(scene, step, 4.5);
            await GoAsync(scene, page.Locator("button.save").First);
            await page.Locator("text=part settings have been saved").WaitForAsync();
            await scene.MoveToAsync(900, 400, 8);
            await scene.ScrollAsync(0, 1400, 20);
            await scene.HighlightAsync(Notice(page, "part settings have been saved"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("page-1", async () =>
        {
            await ClickAsync(scene, Group(page, "Content"), 300);
            await GoAsync(scene, page.Locator("#left-nav a.oc-navigation-admin-content-content_items"));
            await GoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true }).First);
            var tab = page.GetByRole(AriaRole.Tab, new() { Name = "SEO" });
            await ClickAsync(scene, tab, 300);
            await page.GetByLabel("Page Title").WaitForAsync();
            Console.WriteLine("SEO tab aria-selected=" + await tab.GetAttributeAsync("aria-selected") + " class=" + await tab.GetAttributeAsync("class"));
        });

        await scene.StepAsync("page-2", async () =>
        {
            var render = page.GetByLabel("Render", new() { Exact = true });
            await scene.HighlightAsync(render);
            await scene.HighlightAsync(page.GetByText("Whether to render metadata for this page."));
        });

        await scene.StepAsync("page-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TypeIntoAsync(scene, page.GetByLabel("Page Title"), "About This Demo Blog", 40);
            await scene.HighlightAsync(page.GetByText("Override the default generated web browser page title"));
        });

        await scene.StepAsync("page-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TypeIntoAsync(scene, page.Locator("textarea[name$='MetaDescription']"), "A short introduction to this demo blog and what it covers.", 20);
        });

        await scene.StepAsync("page-5", async () =>
        {
            var step = Stopwatch.StartNew();
            await scene.HighlightAsync(page.GetByLabel("Canonical"));
            await scene.HighlightAsync(page.GetByText("Override the default canonical url."));
            await AtAsync(scene, step, 7.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Meta Robots"));
            await scene.HighlightAsync(page.GetByText("The robots meta to render for this page."));
        });

        await scene.StepAsync("page-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var image = Wrapper(page, "Default social image");
            await ScrollToAsync(page.Locator("h5:text-is(\"Social\")").First, 100);
            await PickPictureAsync(scene, image, "about-bg.jpg");
        });

        await scene.StepAsync("page-7", async () =>
        {
            await MediaTextAsync(scene, Wrapper(page, "Default social image"), "Banner picture of the About page");
        });

        await scene.StepAsync("page-8", async () =>
        {
            var step = Stopwatch.StartNew();
            var og = page.Locator("h5:text-is(\"Open Graph\")").First;
            await ScrollToAsync(og, 100);
            await scene.HighlightAsync(og);
            await AtAsync(scene, step, 4.5);
            await scene.ClearHighlightsAsync();
            var tw = page.Locator("h5:text-is(\"Twitter\")").First;
            await ScrollToAsync(tw, 100);
            await scene.HighlightAsync(tw);
        });

        await scene.StepAsync("page-9", async () =>
        {
            var step = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var publish = page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }).First;
            await scene.MoveToAsync(publish);
            await scene.HighlightAsync(publish);
            await AtAsync(scene, step, 7.6);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, publish);
            await page.Locator("text=has been published").WaitForAsync();
            await scene.HighlightAsync(Notice(page, "has been published"));
        });

        await scene.ClearHighlightsAsync();
    }
}

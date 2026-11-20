using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static IReadOnlyDictionary<string, double> s_durations = new Dictionary<string, double>();

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-link", LinkAsync),
        ("03-html", HtmlAsync),
        ("04-markdown", MarkdownAsync),
        ("05-youtube", YoutubeAsync),
        ("06-fill", FillAsync),
        ("07-site", SiteAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        s_durations = durations;
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    // Seeds the content type Workshop with the Title part and nothing else, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 }, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/List", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("a[href$='/Admin/ContentTypes/Create']").First.ClickAsync();
        await page.GetByLabel("Display Name", new() { Exact = true }).FillAsync("Workshop");
        await page.GetByLabel("Display Name", new() { Exact = true }).PressAsync("Tab");
        Console.WriteLine("technical name: " + await page.GetByLabel("Technical Name", new() { Exact = true }).InputValueAsync());
        await page.GetByRole(AriaRole.Button, new() { Name = "Create", Exact = true }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.GetByLabel("Title", new() { Exact = true }).CheckAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Add Selected Parts" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("now on " + page.Url + " : " + await page.TitleAsync());
        await context.CloseAsync();
    }

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.SetDefaultTimeout(120_000);

        return scene;
    }

    // The clock of a step: its time without the off-camera waits, so a click lands on its sentence of the narration.
    private sealed class Clock(Scene scene, string id)
    {
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        private double _skipped;

        public double Duration { get; } = s_durations.TryGetValue(id, out var d) ? d : 2;

        public double Elapsed => _watch.Elapsed.TotalSeconds - _skipped;

        public async Task UntilAsync(double seconds)
        {
            Console.WriteLine($"    at {Elapsed:F1}s (target {seconds:F1}s of {Duration:F1}s)");
            var remaining = seconds - Elapsed;

            if (remaining > 0)
            {
                await scene.HoldAsync(remaining);
            }
        }

        // Holds until this fraction of the narration has been spoken.
        public Task FractionAsync(double fraction) => UntilAsync(Duration * fraction);

        // Holds until this many seconds before the end of the narration.
        public Task BeforeEndAsync(double seconds) => UntilAsync(Duration - seconds);

        public async Task OffAsync(Func<Task> wait, int lead = 400)
        {
            await scene.OffCameraAsync(async () =>
            {
                var t = Stopwatch.StartNew();
                await wait();
                _skipped += t.Elapsed.TotalSeconds + 0.4;
            }, lead);
        }
    }

    private static Task StepAsync(Scene scene, string id, Func<Clock, Task> actions)
        => scene.StepAsync(id, async () =>
        {
            var clock = new Clock(scene, id);
            await actions(clock);
            Console.WriteLine($"    done at {clock.Elapsed:F1}s of {clock.Duration:F1}s");
        });

    // A short cursor path: on a loaded machine every pointer move is a slow round trip.
    private static async Task ClickAsync(Scene scene, ILocator locator, int pauseAfter = 300)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await Task.Delay(100);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await Task.Delay(pauseAfter);
    }

    // Clicks something that loads a page; the load is cut out of the clip.
    private static async Task GoAsync(Scene scene, Clock clock, ILocator locator, int lead = 500)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await Task.Delay(100);
        await clock.OffAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), lead);
        await Task.Delay(150);
    }

    private static async Task TypeAsync(Scene scene, ILocator field, string text, int delay = 6)
    {
        await ClickAsync(scene, field, 150);
        await field.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = delay });
        await Task.Delay(200);

        if (await field.InputValueAsync() != text)
        {
            await field.FillAsync(text);
        }
    }

    private static ILocator Label(IPage page, string name) => page.GetByLabel(name, new() { Exact = true });

    private static ILocator Heading(IPage page, string name) => page.GetByRole(AriaRole.Heading, new() { Name = name, Exact = true });

    private static ILocator Button(IPage page, string name) => page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private static async Task ScrollToAsync(Scene scene, ILocator element, double top)
    {
        var box = await element.BoundingBoxAsync();
        await scene.ScrollAsync(0, box!.Y - top, 6);
    }

    private static async Task ScrollTopAsync(Scene scene)
    {
        if (await scene.Page.EvaluateAsync<double>("() => window.scrollY") > 0)
        {
            await scene.ScrollAsync(0, -3000, 6);
        }
    }

    private static async Task BringIntoViewAsync(Scene scene, ILocator element, double top = 400)
    {
        var box = await element.BoundingBoxAsync();

        if (box!.Y < 100 || box.Y + box.Height > 760)
        {
            await ScrollToAsync(scene, element, top);
        }
    }

    private static async Task RingAsync(Scene scene, ILocator element, double seconds, string caption = null)
    {
        await scene.HighlightAsync(element, caption);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator FieldTypeLabel(IPage page, string type)
        => page.Locator("form[action*='AddFieldsTo'] ul.list-group label").Filter(new() { HasTextRegex = new Regex($@"^\s*{Regex.Escape(type)}\s*$") }).First;

    // The page of a content type, scrolled to the Fields heading so Add Field is in view.
    private static async Task OpenWorkshopAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Workshop");
        await ScrollToAsync(scene, Heading(scene.Page, "Fields"), 120);
    }

    private static async Task AddFieldAsync(Scene scene, Clock clock)
    {
        await GoAsync(scene, clock, scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Field", Exact = true }));
        await Label(scene.Page, "Display Name").WaitForAsync();
        await scene.PauseAsync(300);
    }

    private static async Task ChooseTypeAsync(Scene scene, string type)
    {
        var label = FieldTypeLabel(scene.Page, type);
        await label.WaitForAsync();
        await BringIntoViewAsync(scene, label);
        await ClickAsync(scene, label, 300);

        if (!await Label(scene.Page, type).IsCheckedAsync())
        {
            await Label(scene.Page, type).CheckAsync();
        }
    }

    private static Task SaveConfigureAsync(Scene scene, Clock clock) => GoAsync(scene, clock, Button(scene.Page, "Save & Configure"));

    private static Task SaveAsync(Scene scene, Clock clock) => GoAsync(scene, clock, Button(scene.Page, "Save"));

    // Scrolls so that the Fields of the type are in view after a save.
    private static async Task ShowFieldsAsync(Scene scene, double top = 120)
    {
        await Heading(scene.Page, "Fields").WaitForAsync();
        await ScrollToAsync(scene, Heading(scene.Page, "Fields"), top);
        await scene.MoveToAsync(760, 420, 8);
    }

    private static ILocator CheckRow(IPage page, string nameEnd)
        => page.Locator("div.form-check, div.mb-3").Filter(new() { Has = page.Locator($"input[name$='{nameEnd}']") }).Last;

    // 02-link: the Link Field.
    private static async Task LinkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/List");
        var edit = page.Locator("a[role='btn-edit-Workshop']").First;
        await edit.WaitForAsync();
        await ScrollToAsync(scene, edit, 400);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "link-1", async clock =>
        {
            var row = page.Locator("li.list-group-item").Filter(new() { Has = edit }).Last;
            await scene.HighlightAsync(row);
            await clock.FractionAsync(0.3);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, clock, edit);
            await Heading(page, "Fields").WaitForAsync();
            await ScrollToAsync(scene, Heading(page, "Fields"), 120);
            await clock.FractionAsync(0.55);
            await scene.HighlightAsync(Heading(page, "Fields"));
            await clock.FractionAsync(0.75);
            await scene.ClearHighlightsAsync();
            await AddFieldAsync(scene, clock);
        });

        await StepAsync(scene, "link-2", async clock =>
        {
            await TypeAsync(scene, Label(page, "Display Name"), "Registration Link");
            await scene.PauseAsync(300);
            await scene.HighlightAsync(Label(page, "Technical Name"));
            await clock.FractionAsync(0.5);
            await scene.ClearHighlightsAsync();
            await ChooseTypeAsync(scene, "Link Field");
        });

        await StepAsync(scene, "link-3", async clock =>
        {
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Required").WaitForAsync();
        });

        await StepAsync(scene, "link-4", async clock =>
        {
            await ClickAsync(scene, Label(page, "Required"), 200);
            await scene.HighlightAsync(page.Locator("select[name$='LinkTextMode']"));
            await clock.FractionAsync(0.4);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(page.Locator("select[name$='LinkTextMode']"), "Required");
            await scene.HighlightAsync(page.Locator("select[name$='LinkTextMode']"));
        });

        await StepAsync(scene, "link-5", async clock =>
        {
            await scene.ClearHighlightsAsync();
            var target = page.Locator("input[name$='DefaultTarget']");
            await ScrollToAsync(scene, target, 300);
            await scene.HighlightAsync(target);
            await clock.FractionAsync(0.35);
            await scene.ClearHighlightsAsync();
            await TypeAsync(scene, target, "_blank");
            await page.Keyboard.PressAsync("Tab");
            await clock.BeforeEndAsync(4.4);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene);
        });
    }

    // 03-html: the Html Field with the Wysiwyg editor.
    private static async Task HtmlAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenWorkshopAsync(scene);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "html-1", async clock =>
        {
            await AddFieldAsync(scene, clock);
            await TypeAsync(scene, Label(page, "Display Name"), "Schedule");
            await ChooseTypeAsync(scene, "Html Field");
            await clock.BeforeEndAsync(3.2);
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Hint").WaitForAsync();
        });

        await StepAsync(scene, "html-2", async clock =>
        {
            await TypeAsync(scene, page.Locator("textarea[name$='Hint']"), "Days, times and room.");
            await clock.FractionAsync(0.4);
            await scene.HighlightAsync(CheckRow(page, "SanitizeHtml"));
        });

        await StepAsync(scene, "html-3", async clock =>
        {
            await scene.ClearHighlightsAsync();
            var editor = page.Locator("select[data-field-editor-select]");
            await ScrollToAsync(scene, editor, 300);
            await scene.HighlightAsync(editor);
            await clock.FractionAsync(0.4);
            await scene.ClearHighlightsAsync();
            var (ex, ey) = await scene.CenterOfAsync(editor);
            await scene.MoveToAsync(ex, ey, 8);
            await editor.SelectOptionAsync("Wysiwyg");
            await scene.PauseAsync(300);
            await clock.BeforeEndAsync(5.5);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene);
        });
    }

    // 04-markdown: the Markdown Field.
    private static async Task MarkdownAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenWorkshopAsync(scene);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "markdown-1", async clock =>
        {
            await AddFieldAsync(scene, clock);
            await TypeAsync(scene, Label(page, "Display Name"), "What to Bring");
            await ChooseTypeAsync(scene, "Markdown Field");
            await clock.BeforeEndAsync(3.2);
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Hint").WaitForAsync();
        });

        await StepAsync(scene, "markdown-2", async clock =>
        {
            await TypeAsync(scene, page.Locator("textarea[name$='Hint']"), "One item per line.");
            await scene.HighlightAsync(page.Locator("textarea[name$='Hint']"));
            await clock.FractionAsync(0.2);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(CheckRow(page, "SanitizeHtml"));
            await clock.FractionAsync(0.4);
            await scene.ClearHighlightsAsync();
            var editor = page.Locator("select[data-field-editor-select]");
            await ScrollToAsync(scene, editor, 300);
            await scene.HighlightAsync(editor);
            await clock.FractionAsync(0.75);
            await scene.ClearHighlightsAsync();
            await clock.BeforeEndAsync(4.4);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene);
        });
    }

    // 05-youtube: the Youtube Field.
    private static async Task YoutubeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenWorkshopAsync(scene);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "youtube-1", async clock =>
        {
            await AddFieldAsync(scene, clock);
            await TypeAsync(scene, Label(page, "Display Name"), "Teaser Video");
            await ChooseTypeAsync(scene, "Youtube Field");
            await clock.BeforeEndAsync(3.2);
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Placeholder").WaitForAsync();
        });

        await StepAsync(scene, "youtube-2", async clock =>
        {
            await TypeAsync(scene, page.Locator("input[name$='Placeholder']"), "Paste the address of the video page");
            await TypeAsync(scene, page.Locator("textarea[name$='Hint']"), "Use the address of the video page.");
            await clock.FractionAsync(0.4);
            var width = page.Locator("input[name$='Width']");
            await ScrollToAsync(scene, width, 300);
            await scene.HighlightAsync(width);
            await scene.HighlightAsync(page.Locator("input[name$='Height']"));
            await clock.FractionAsync(0.75);
            await scene.ClearHighlightsAsync();
            await clock.BeforeEndAsync(4.4);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene);
        });

        await StepAsync(scene, "youtube-3", async clock =>
        {
            await ScrollToAsync(scene, Heading(page, "Fields"), 100);
            await scene.HighlightAsync(page.Locator("ul#fields"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static ILocator Wrapper(IPage page, string name) => page.Locator($".field-wrapper-workshop-{name}");

    // 06-fill: create and publish a workshop.
    private static async Task FillAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/List");
        var create = page.Locator("a#btn-create-Workshop");
        await create.WaitForAsync();
        await ScrollToAsync(scene, create, 400);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "fill-1", async clock =>
        {
            await clock.FractionAsync(0.1);
            await GoAsync(scene, clock, create);
            await Label(page, "Title").WaitForAsync();
            await Wrapper(page, "teaser-video").WaitForAsync();
            await clock.FractionAsync(0.3);
            await ScrollTopAsync(scene);
            await clock.FractionAsync(0.6);
            await scene.ScrollAsync(0, 700, 6);
        });

        await StepAsync(scene, "fill-2", async clock =>
        {
            await ScrollTopAsync(scene);
            await TypeAsync(scene, Label(page, "Title"), "Pottery Basics");
            var link = Wrapper(page, "registration-link");
            await ScrollToAsync(scene, link, 200);
            await TypeAsync(scene, link.Locator("input[name$='.Url']"), "https://example.com/workshops/pottery-basics");
            await TypeAsync(scene, link.Locator("input[name$='.Text']"), "Register for Pottery Basics");
            await clock.BeforeEndAsync(3.2);
            await scene.HighlightAsync(link.Locator("input[name$='.Target']"));
            await scene.HoldAsync(0.5);
            await scene.ClearHighlightsAsync();
            var schedule = Wrapper(page, "schedule");
            await ScrollToAsync(scene, schedule, 150);
            await ClickAsync(scene, schedule.Locator(".trumbowyg-editor"), 100);
        });

        await StepAsync(scene, "fill-3", async clock =>
        {
            var schedule = Wrapper(page, "schedule");
            var bold = schedule.Locator(".trumbowyg-bold-button");
            await page.Keyboard.TypeAsync("Saturdays from ", new() { Delay = 5 });
            await ClickAsync(scene, bold, 100);
            await page.Keyboard.TypeAsync("10:00 to 12:00", new() { Delay = 5 });
            await ClickAsync(scene, bold, 100);
            await page.Keyboard.TypeAsync(".", new() { Delay = 10 });
            await page.Keyboard.PressAsync("Enter");
            await page.Keyboard.TypeAsync("Doors open at 9:30.", new() { Delay = 5 });
        });

        await StepAsync(scene, "fill-4", async clock =>
        {
            var area = Wrapper(page, "whatto-bring").Locator("textarea");
            await ScrollToAsync(scene, Wrapper(page, "whatto-bring"), 150);
            await ClickAsync(scene, area, 200);
            var lines = new[] { "## Bring with you", "- An apron", "- A towel", "- A notebook" };

            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    await page.Keyboard.PressAsync("Enter");
                }

                await page.Keyboard.TypeAsync(lines[i], new() { Delay = 10 });
            }
        });

        await StepAsync(scene, "fill-5", async clock =>
        {
            var wrapper = Wrapper(page, "teaser-video");
            await ScrollToAsync(scene, wrapper, 200);
            await TypeAsync(scene, wrapper.Locator("input[name$='.RawAddress']"), "https://www.example.com/watch?v=pottery-teaser");
        });

        await StepAsync(scene, "fill-6", async clock =>
        {
            await clock.BeforeEndAsync(4.0);
            await GoAsync(scene, clock, page.Locator("button.publish[name='submit.Publish']"));
        });
    }

    // 07-site: the item on the site.
    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/List");
        var list = page.Locator("a[role='btn-list-Workshop']").First;
        await list.WaitForAsync();
        await ScrollToAsync(scene, list, 400);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "site-1", async clock =>
        {
            var row = page.Locator("li.list-group-item").Filter(new() { Has = list }).Last;
            await scene.HighlightAsync(row);
            await clock.FractionAsync(0.15);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, clock, list);
            var view = page.Locator("li.list-group-item").Filter(new() { HasText = "Pottery Basics" }).First.Locator("a.view");
            await view.WaitForAsync();
            await clock.FractionAsync(0.6);
            var (x, y) = await scene.CenterOfAsync(view);
            await scene.MoveToAsync(x, y, 8);
            IPage tab = null;
            await clock.OffAsync(async () =>
            {
                tab = await page.Context.RunAndWaitForPageAsync(() => page.Mouse.ClickAsync((float)x, (float)y));
                await tab.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await scene.SwitchToAsync(tab);
            }, 400);
            page = scene.Page;
            await page.Locator(".field-name-workshop-registration-link, .field-type-linkfield").First.WaitForAsync();
        });

        await StepAsync(scene, "site-2", async clock =>
        {
            var link = scene.Page.Locator("div.field-type-linkfield a").First;
            await ScrollToAsync(scene, link, 300);
            await scene.HighlightAsync(link);
        });

        await StepAsync(scene, "site-3", async clock =>
        {
            await scene.ClearHighlightsAsync();
            var html = scene.Page.Locator("div.field-type-htmlfield").First;
            await BringIntoViewAsync(scene, html, 300);
            await scene.HighlightAsync(html);
        });

        await StepAsync(scene, "site-4", async clock =>
        {
            await scene.ClearHighlightsAsync();
            var md = scene.Page.Locator("div.field-type-markdownfield").First;
            await BringIntoViewAsync(scene, md, 300);
            await scene.HighlightAsync(md);
            await scene.Page.Locator("iframe[src*='/embed/pottery-teaser']").First.WaitForAsync();
        });

        await StepAsync(scene, "site-5", async clock =>
        {
            await scene.ClearHighlightsAsync();
            var frame = scene.Page.Locator("iframe[src*='/embed/pottery-teaser']").First;
            await clock.OffAsync(async () =>
            {
                await BringIntoViewAsync(scene, frame, 200);
                await scene.Page.FrameLocator("iframe[src*='/embed/pottery-teaser']").Locator("body").WaitForAsync();
                await Task.Delay(1500);
            });
            await scene.HighlightAsync(frame);
        });

        await scene.ClearHighlightsAsync();
    }
}

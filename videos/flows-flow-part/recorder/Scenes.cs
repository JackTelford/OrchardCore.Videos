using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the flows-flow-part video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-part", PartAsync),
        ("03-build", BuildAsync),
        ("04-site", SiteAsync),
        ("05-blocks", BlocksAsync),
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

    private static ILocator Label(Scene scene, string text) => scene.Page.GetByLabel(text, new() { Exact = true });

    private static async Task AtAsync(Stopwatch watch, int milliseconds)
    {
        var remaining = milliseconds - (int)watch.ElapsedMilliseconds;

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }
    }

    // Scrolls with the wheel until the element is comfortably in view.
    private static async Task RevealAsync(Scene scene, ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();

        if (box is not null && (box.Y < 130 || box.Y + box.Height > 700))
        {
            await scene.ScrollAsync(0, box.Y - 300);
        }
    }

    // Opens a page through the left menu: the groups to expand, then the link.
    private static async Task MenuAsync(Scene scene, string fallbackPath, params string[] path)
    {
        var page = scene.Page;
        var groups = path[..^1];
        var container = page.Locator($"#left-nav ul[data-title='{groups[^1]}']");

        foreach (var group in groups)
        {
            var list = page.Locator($"#left-nav ul[data-title='{group}']");

            if (await list.CountAsync() == 0)
            {
                await scene.NavigateAsync(fallbackPath);

                return;
            }

            if (!await list.IsVisibleAsync())
            {
                await scene.ClickAsync(page.Locator($"#left-nav figcaption:has(span.title:text-is('{group}'))").First, pauseAfter: 250);
            }
        }

        await scene.ClickAndWaitForNavigationAsync(container.GetByRole(AriaRole.Link, new() { Name = path[^1], Exact = true }).First);
    }

    // The block of a form that holds a label: its closest wrapping div, or the label itself.
    private static async Task<ILocator> BlockAsync(Scene scene, string text)
    {
        var label = scene.Page.GetByText(text, new() { Exact = true }).First;
        var wrapper = label.Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'form-group')][1]");

        return await wrapper.CountAsync() > 0 ? wrapper : label;
    }

    private static ILocator Widget(Scene scene, string type) => scene.Page.Locator($".widget-editor[data-content-type='{type}']").Last;

    private static async Task HoverHeaderAsync(Scene scene, string type)
    {
        var header = Widget(scene, type).Locator(".widget-editor-header").First;
        await scene.MoveToAsync(header);
        await scene.PauseAsync(300);
    }

    private static async Task PartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav figcaption:has(span.title:text-is('Design'))").First.ClickAsync();
        await scene.Page.Locator("#left-nav figcaption:has(span.title:text-is('Content Definition'))").First.ClickAsync();
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("part-1", async () =>
        {
            await scene.PauseAsync(500);
            await MenuAsync(scene, "/Admin/ContentTypes/List", "Design", "Content Definition", "Content Types");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a[href$='/Admin/ContentTypes/Edit/Article']").First);
        });

        await scene.StepAsync("part-2", async () =>
        {
            var add = page.Locator("a[href$='/Admin/ContentTypes/AddPartsTo/Article']").First;
            await RevealAsync(scene, add);
            await scene.ClickAndWaitForNavigationAsync(add);
            var row = page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("label.form-check-label:text-is('Flow')") }).First;
            await row.WaitForAsync();
            await RevealAsync(scene, row);
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("part-3", async () =>
        {
            var row = page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("label.form-check-label:text-is('Flow')") }).First;
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("a, button").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Add & Configure") }).First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/ContentTypes/Edit/Page");
            var flow = page.Locator("#parts li").Filter(new() { HasText = "Flow" }).First;
            await flow.WaitForAsync();
            await RevealAsync(scene, flow);
            await scene.HighlightAsync(flow);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(flow.Locator("a", new() { HasTextString = "Edit" }).First);
        });

        await scene.StepAsync("sett-1", async () =>
        {
            await scene.HighlightAsync(await BlockAsync(scene, "Restrict Content Types"));
        });

        await scene.StepAsync("sett-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var box = page.Locator(".form-check").Filter(new() { HasText = "Collapse Contained Items" }).Last;
            await RevealAsync(scene, box);
            await scene.HighlightAsync(box);
        });

        await scene.StepAsync("sett-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var select = page.GetByLabel("What default alignment should be used?");
            await RevealAsync(scene, select);
            await scene.HighlightAsync(select);
        });

        await scene.StepAsync("sett-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var select = page.GetByLabel("What type of editor should be used?");
            await RevealAsync(scene, select);
            await scene.HighlightAsync(select);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BuildAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav figcaption:has(span.title:text-is('Content'))").First.ClickAsync();
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("build-1", async () =>
        {
            var items = page.Locator("#left-nav ul[data-title='Content']").GetByRole(AriaRole.Link, new() { Name = "Content Items", Exact = true }).First;
            await scene.MoveToAsync(items);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await items.ClickAsync());
                await page.Locator("#new-dropdown").WaitForAsync(new() { Timeout = 60000 });
            });
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 900);
            var item = page.Locator("#new-dropdown ~ .dropdown-menu a.dropdown-item").GetByText("Page", new() { Exact = true }).First;
            await scene.MoveToAsync(item);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await item.ClickAsync());
                await Label(scene, "Title").WaitForAsync(new() { Timeout = 60000 });
            });
        });

        await scene.StepAsync("build-2", async () =>
        {
            var title = Label(scene, "Title");
            await title.WaitForAsync();
            await scene.HighlightAsync(title);
            await scene.PauseAsync(700);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByText("Permalink", new() { Exact = true }).First);
            await scene.PauseAsync(700);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByText("Flow", new() { Exact = true }).First);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(title, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Hillside Bakery", new() { Delay = 6 });
        });

        await scene.StepAsync("build-3", async () =>
        {
            var plus = page.Locator(".btn-widget-add-below button.dropdown-toggle").First;
            await RevealAsync(scene, plus);
            await scene.ClickAsync(plus, pauseAfter: 900);
            await scene.ClickAsync(page.Locator("a.add-widget[data-widget-type='Paragraph']").First, pauseAfter: 100);
            await page.Locator(".widget-editor[data-content-type='Paragraph']").First.WaitForAsync();
            var editor = page.Locator(".widget-editor[data-content-type='Paragraph'] .trumbowyg-editor").First;
            await scene.ClickAsync(editor, pauseAfter: 100);
            await editor.FillAsync("Fresh bread and pastries, baked every morning.");
        });

        await scene.StepAsync("build-4", async () =>
        {
            var plus = page.Locator(".btn-widget-add-below button.dropdown-toggle").Last;
            await scene.ClickAsync(plus, pauseAfter: 200);
            await scene.ClickAsync(page.Locator("a.add-widget[data-widget-type='Blockquote']").Last, pauseAfter: 100);
            await page.Locator(".widget-editor[data-content-type='Blockquote']").First.WaitForAsync();
            var text = page.Locator(".widget-editor[data-content-type='Blockquote'] textarea").First;
            await scene.ClickAsync(text, pauseAfter: 100);
            await text.FillAsync("I walk to the shop every Friday for a loaf.");
        });

        await scene.StepAsync("build-5", async () =>
        {
            foreach (var type in new[] { "Paragraph", "Blockquote" })
            {
                await HoverHeaderAsync(scene, type);
                var widget = Widget(scene, type);
                await scene.ClickAsync(widget.Locator(".btn-widget-metadata button").Filter(new() { HasText = "100%" }).First, pauseAfter: 700);
                await scene.ClickAsync(widget.Locator("label.dropdown-item").Filter(new() { HasText = "50%" }).First, pauseAfter: 300);
            }
        });

        await scene.StepAsync("build-6", async () =>
        {
            await HoverHeaderAsync(scene, "Blockquote");
            var widget = Widget(scene, "Blockquote");
            await scene.ClickAsync(widget.Locator(".btn-widget-metadata button.dropdown-toggle:has(i.fa-align-justify)").First, pauseAfter: 1000);
            await scene.ClickAsync(widget.Locator("label.dropdown-item").Filter(new() { HasText = "Center" }).First, pauseAfter: 300);
        });

        await scene.StepAsync("build-7", async () =>
        {
            await HoverHeaderAsync(scene, "Paragraph");
            var widget = Widget(scene, "Paragraph");
            await scene.ClickAsync(widget.Locator(".widget-editor-btn-collapse").First, pauseAfter: 900);
            await scene.ClickAsync(widget.Locator(".widget-editor-btn-expand").First, pauseAfter: 300);
        });

        await scene.StepAsync("build-8", async () =>
        {
            await HoverHeaderAsync(scene, "Blockquote");
            var handle = Widget(scene, "Blockquote").Locator(".widget-editor-handle").First;
            var target = Widget(scene, "Paragraph").Locator(".widget-editor-header").First;
            await scene.DragToAsync(handle, target, -150, 0);
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("build-9", async () =>
        {
            await HoverHeaderAsync(scene, "Blockquote");
            var widget = Widget(scene, "Blockquote");
            await scene.ClickAsync(widget.Locator(".btn-widget-metadata button[title='Insert Widget']").First, pauseAfter: 1000);
            await scene.ClickAsync(widget.Locator("a.insert-widget[data-widget-type='RawHtml']").First, pauseAfter: 300);
            await page.Locator(".widget-editor[data-content-type='RawHtml']").First.WaitForAsync();
        });

        await scene.StepAsync("build-10", async () =>
        {
            await HoverHeaderAsync(scene, "RawHtml");
            await scene.ClickAsync(Widget(scene, "RawHtml").Locator(".widget-delete").First, pauseAfter: 500);
            await page.Locator("#modalOkButton").WaitForAsync();
            await scene.PauseAsync(1000);
            await scene.ClickAsync(page.Locator("#modalOkButton"), pauseAfter: 300);
            await page.Locator(".widget-editor[data-content-type='RawHtml']").WaitForAsync(new() { State = WaitForSelectorState.Detached });
        });

        await scene.StepAsync("build-11", async () =>
        {
            await scene.ScrollAsync(0, 1500);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.publish").First);
            await page.Locator("li.list-group-item").Filter(new() { HasText = "Hillside Bakery" }).First.WaitForAsync();
        });
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/hillside-bakery");
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            await scene.PauseAsync(1000);
            await scene.ScrollAsync(0, 400);
        });
    }

    private static async Task BlocksAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Page/ContentParts/FlowPart/Edit");
        var page = scene.Page;
        await page.Locator("select[data-type-part-editor-select]").ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("blocks-1", async () =>
        {
            await scene.SelectAsync(page.Locator("select[data-type-part-editor-select]"), "Blocks");
            await scene.PauseAsync(1000);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First);
        });

        await scene.StepAsync("blocks-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems");
            var edit = page.Locator("li.list-group-item").Filter(new() { HasText = "Hillside Bakery" }).Locator("a.btn.edit").First;
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await edit.ClickAsync());
                await page.Locator("button.flowpart-blocks-picker-button").First.WaitForAsync(new() { Timeout = 60000 });
            });

            var plus = page.Locator("button.flowpart-blocks-picker-button").Last;
            await RevealAsync(scene, plus);
            await scene.ClickAsync(plus, pauseAfter: 600);
            await page.Locator("#contentTypePickerModal").WaitForAsync();
        });

        await scene.StepAsync("blocks-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.HighlightAsync(page.Locator("#contentTypePickerApp ul.content-type-picker-categories").First);
            await AtAsync(watch, 3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#contentTypePickerApp .content-type-picker-grid").First);
        });

        await scene.StepAsync("blocks-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var search = page.Locator("#contentTypePickerApp input[type=search]").First;
            await scene.ClickAsync(search, pauseAfter: 200);
            await page.Keyboard.TypeAsync("quote", new() { Delay = 40 });
            await scene.PauseAsync(1000);
            var card = page.Locator(".content-type-card").Filter(new() { HasText = "Blockquote" }).First;
            await scene.ClickAsync(card.GetByRole(AriaRole.Button, new() { Name = "Add" }), pauseAfter: 300);
            await Assertions.Expect(page.Locator(".widget-editor[data-content-type='Blockquote']")).ToHaveCountAsync(2, new() { Timeout = 15000 });
            await Assertions.Expect(page.Locator("#contentTypePickerModal")).ToBeHiddenAsync(new() { Timeout = 15000 });
        });
    }
}

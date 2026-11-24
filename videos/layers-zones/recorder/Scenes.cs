using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Layers: show widgets on the right pages". Record in one take, in order: 03-zones adds the zone Header,
/// 04-layer adds the layer Posts, 05-widget adds the widget "Follow the blog"; 06 to 08 do not change the site in the end.
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
        ("03-zones", ZonesAsync),
        ("04-layer", LayerAsync),
        ("05-widget", WidgetAsync),
        ("06-site", SiteAsync),
        ("07-move", MoveAsync),
        ("08-manage", ManageAsync),
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

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, string caption = null, bool clear = true)
    {
        await scene.HighlightAsync(target, caption);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    // Waits until the given number of seconds have passed since the step started.
    private static async Task PauseUntilAsync(Stopwatch started, double seconds)
    {
        var remaining = seconds - started.Elapsed.TotalSeconds;

        if (remaining > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(remaining));
        }
    }

    private static async Task OpenWidgetsAsync(Scene scene)
    {
        var design = scene.Page.Locator("button.nav-group-label:has-text('Design')").First;

        if (await design.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(design, pauseAfter: 500);
        }

        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/Layers']").First);
    }

    private static async Task OpenZonesAsync(Scene scene)
    {
        var settings = scene.Page.Locator("button.nav-group-label:has-text('Settings')").First;

        if (await settings.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(settings, pauseAfter: 500);
        }

        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/Settings/zones']").First);
    }

    private static ILocator Zone(Scene scene, string name) => scene.Page.Locator($"div.layer-zone[data-zone='{name}']").First;

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("page-1", async () =>
        {
            await OpenWidgetsAsync(scene);
            await scene.Page.Locator("h3:has-text('Zones')").First.WaitForAsync();
            await ShowAsync(scene, scene.Page.Locator("h3:has-text('Zones')").First, 1);
            await scene.HighlightAsync(scene.Page.Locator("h3:has-text('Layers')").First);
        });

        await scene.StepAsync("page-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Zone(scene, "Footer"));
            await scene.HighlightAsync(Zone(scene, "Footer").Locator("span.badge:has-text('Always')").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Zone(scene, "Content"));
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("page-3", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("li.list-group-item:has(#Always)").First, 2);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item:has(#Homepage)").First);
        });

        await scene.StepAsync("page-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#Always").First);
            await scene.Page.Locator("div.layer-zone[data-zone='Footer'] li.list-group-item-success").First.WaitForAsync();
            await scene.PauseAsync(2000);
            await scene.ClickAsync(scene.Page.Locator("#Always").First);
            await scene.Page.Locator("li.list-group-item-success").First.WaitForAsync(new() { State = WaitForSelectorState.Detached });
        });
    }

    private static async Task ZonesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Layers");
        await scene.Page.Locator("h3:has-text('Zones')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        var input = scene.Page.Locator("input[name$='Zones']").First;

        await scene.StepAsync("zones-1", async () =>
        {
            await OpenZonesAsync(scene);
            await scene.Page.GetByText("Available zones for Layers").First.WaitForAsync();
            await scene.HighlightAsync(input);
        });

        await scene.StepAsync("zones-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, scene.Page.Locator("span.hint:has-text('A comma separated list of values')").First, 2);
            await scene.ClickAsync(input);
            await scene.Page.Keyboard.PressAsync("End");
            await input.PressSequentiallyAsync(", Header", new() { Delay = 55 });
            await scene.Page.WaitForFunctionAsync("() => document.querySelector(\"input[name$='Zones']\").value === 'Content, Footer, Header'");
        });

        await scene.StepAsync("zones-3", async () =>
        {
            var started = Stopwatch.StartNew();
            await PauseUntilAsync(started, durations["zones-3"] - 5.5);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        });

        await scene.StepAsync("zones-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenWidgetsAsync(scene);
            await Zone(scene, "Header").WaitForAsync();
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(Zone(scene, "Header").Locator("button:has-text('Add Widget')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LayerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Layers");
        await scene.Page.Locator("h3:has-text('Layers')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("layer-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn[href$='/Admin/Layers/Create']").First);
            await scene.Page.GetByText("Name").First.WaitForAsync();
        });

        await scene.StepAsync("layer-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input#Name").First, "Posts", delay: 50);
            await scene.TypeAsync(scene.Page.Locator("textarea#Description").First, "Pages that show a blog post.", delay: 30);
            await scene.MoveToAsync(scene.Page.Locator("input[type='submit'][value='Create']").First);
        });

        await scene.StepAsync("layer-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("input[type='submit'][value='Create']").First);
            var warning = scene.Page.Locator("p.alert-warning").First;
            await warning.WaitForAsync();
            await scene.HighlightAsync(warning);
        });

        await scene.StepAsync("layer-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button[data-bs-target='#modalConditions']").First);
            await scene.Page.Locator("#modalConditions.show").WaitForAsync();
            await scene.PauseAsync(3500);
            await scene.HighlightAsync(scene.Page.Locator("#search-box").First);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "url", delay: 80);
            await scene.Page.Locator("#modalConditions .item[data-filter-value='UrlCondition']").First.WaitForAsync();
        });

        await scene.StepAsync("layer-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalConditions .item[data-filter-value='UrlCondition'] a.condition-thumbnail").First);
            await scene.Page.GetByText("Operation").First.WaitForAsync();
            await scene.SelectAsync(scene.Page.Locator("select[name$='SelectedOperation']").First, "Starts with");
            await scene.TypeAsync(scene.Page.Locator("input[name$='Value']").First, "/blog/", delay: 50);
            await scene.Page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("layer-6", async () =>
        {
            var started = Stopwatch.StartNew();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.create").First);
            var row = scene.Page.Locator("li.list-group-item:has-text('Starts with')").First;
            await row.WaitForAsync();
            await ShowAsync(scene, row, 3);
            await PauseUntilAsync(started, durations["layer-6"] - 5.5);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("input[type='submit'][value='Save']").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WidgetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Layers");
        await Zone(scene, "Content").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("widget-1", async () =>
        {
            await scene.ClickAsync(Zone(scene, "Content").Locator("button:has-text('Add Widget')").First);
            await ShowAsync(scene, Zone(scene, "Content").Locator("ul.dropdown-menu").First, 0.5, clear: false);
            await scene.MoveToAsync(Zone(scene, "Content").Locator("a.add-list-widget[data-widget-type='Paragraph']").First);
        });

        await scene.StepAsync("widget-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Zone(scene, "Content").Locator("a.add-list-widget[data-widget-type='Paragraph']").First);
            var title = scene.Page.Locator("input[name='LayerMetadata.Title']").First;
            await title.WaitForAsync();
            await scene.TypeAsync(title, "Follow the blog", delay: 45);
            await scene.HighlightAsync(scene.Page.Locator("input[type='checkbox'][name$='RenderTitle']").First);
        });

        await scene.StepAsync("widget-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("select[name$='.Layer']").First, "Posts");
            await ShowAsync(scene, scene.Page.GetByText("The layer controls the visibility of this widget.").First, 2);
            var editor = scene.Page.Locator(".trumbowyg-editor").First;
            await editor.WaitForAsync();
            await scene.TypeAsync(editor, "Read more stories on the blog home page.", delay: 25);
        });

        await scene.StepAsync("widget-4", async () =>
        {
            var started = Stopwatch.StartNew();
            var publish = scene.Page.Locator("button.publish[name='submit.Publish']").First;
            await publish.ScrollIntoViewIfNeededAsync();
            await PauseUntilAsync(started, durations["widget-4"] - 6.5);
            await scene.ClickAndWaitForNavigationAsync(publish);
            await scene.Page.Locator("div.layer-zone[data-zone='Content'] li.list-group-item").First.WaitForAsync();
            await scene.HighlightAsync(Zone(scene, "Content").Locator("li.list-group-item").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("header.masthead").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            await scene.NavigateAsync("/blog/post-1");
            var paragraph = scene.Page.Locator("article.widget-paragraph").First;
            await paragraph.WaitForAsync();
            await paragraph.ScrollIntoViewIfNeededAsync();
            await scene.HoldAsync(0.5);
            await scene.HighlightAsync(paragraph);
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/about");
            await scene.HoldAsync(3);
            await scene.ScrollAsync(0, 3000);
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.NavigateAsync("/");
            await scene.HoldAsync(1);
            await scene.ScrollAsync(0, 300);
            await scene.HoldAsync(1);
            await scene.ScrollAsync(0, 300);
        });
    }

    private static async Task MoveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Layers");
        await Zone(scene, "Content").Locator("li.list-group-item").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("move-1", async () =>
        {
            await scene.DragToAsync(
                Zone(scene, "Content").Locator("li.list-group-item .properties").First,
                Zone(scene, "Footer").Locator("li.list-group-item .properties").First,
                0,
                -10);
            await scene.Page.Locator("#layer-undo-message").WaitForAsync();
            await scene.Page.EvaluateAsync("window.scrollTo(0, 0)");
        });

        await scene.StepAsync("move-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#layer-undo-message a").First);
            await scene.HoldAsync(1);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#layer-undo-message a").First);
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Layers");
        await scene.Page.Locator("li.list-group-item:has(#Posts)").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("manage-1", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("li.list-group-item:has(#Posts)").First, 1.5);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item:has(#Posts) a.btn:has-text('Edit')").First);
            await scene.HighlightAsync(scene.Page.Locator("input[readonly]").First, "Read-only");
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, scene.Page.Locator("textarea, input[name$='Description']").First, 1.5);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item:has-text('Starts with')").First);
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Layers");
            await scene.ClickAsync(scene.Page.Locator("li.list-group-item:has(#Posts) a.btn-danger:has-text('Delete')").First);
            await scene.Page.Locator("#confirmRemoveModal").WaitForAsync();
            await scene.PauseAsync(2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton").First);
        });
    }
}

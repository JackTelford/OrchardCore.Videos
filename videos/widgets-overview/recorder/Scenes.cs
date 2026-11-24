using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Widgets overview". Record in one take, in order: 04-allow adds the WidgetsList part to Article, 05-add saves two
/// widgets in the About article, 06-manage works on that article without saving.
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
        ("02-feature", FeatureAsync),
        ("03-types", TypesAsync),
        ("04-allow", AllowAsync),
        ("05-add", AddAsync),
        ("06-manage", ManageAsync),
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

    private static ILocator Group(Scene scene, string name)
        => scene.Page.Locator("#left-nav button, #left-nav a").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static ILocator Entry(Scene scene, string name)
        => scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static async Task OpenGroupAsync(Scene scene, string name, int pause = 500)
    {
        var group = Group(scene, name);

        if (await group.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(group, pauseAfter: pause);
        }
    }

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, string caption = null, bool clear = true)
    {
        await scene.HighlightAsync(target, caption);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static ILocator TypeRow(Scene scene, string technicalName)
        => scene.Page.Locator($"li.list-group-item:has(a[role='btn-edit-{technicalName}'])").First;

    private static async Task FeatureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        var row = scene.Page.Locator("li[data-filter-value]:has(code:text-is('OrchardCore.Widgets'))").First;

        await scene.StepAsync("feature-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await scene.ClickAndWaitForNavigationAsync(Entry(scene, "Features"));
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Widgets", delay: 70);
            await row.WaitForAsync();
            await scene.HoldAsync(0.5);
            await scene.HighlightAsync(row.Locator(".admin-list-secondary").First);
        });

        await scene.StepAsync("feature-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#btn-disable-OrchardCore_Widgets").First, "Disable");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("types-1", async () =>
        {
            await OpenGroupAsync(scene, "Design", 400);
            await OpenGroupAsync(scene, "Content Definition", 400);
            await scene.ClickAndWaitForNavigationAsync(Entry(scene, "Content Types"));
            await scene.Page.Locator("a[role='btn-edit-Article']").First.WaitForAsync();
        });

        await scene.StepAsync("types-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Widget", delay: 70);
            await scene.HoldAsync(0.6);
            var first = true;

            foreach (var type in new[] { "Blockquote", "Container", "Image", "Paragraph", "RawHtml" })
            {
                var badge = TypeRow(scene, type).Locator(".ta-badge").Filter(new() { HasTextString = "Widget" }).First;
                await scene.HighlightAsync(badge, first ? "Stereotype" : null);
                first = false;
            }

            await scene.HoldAsync(1);
        });

        await scene.StepAsync("types-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[role='btn-edit-Paragraph']").First);
            var field = scene.Page.Locator("input[name$='Stereotype']").First;
            await field.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(field, "Stereotype");
        });

        await scene.StepAsync("types-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var fields = scene.Page.Locator("ul#fields > li").First;
            await fields.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(fields);
        });

        await scene.StepAsync("types-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var cancel = scene.Page.Locator("a.cancel, a:text-is('Cancel')").First;
            await cancel.ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(cancel);
            await scene.HighlightAsync(scene.Page.Locator("a:text-is('Create new type'), a.create:has-text('Create new type')").First, "Create new type");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AllowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.Page.Locator("a[role='btn-edit-Article']").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("allow-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Article", delay: 50);
            await scene.HoldAsync(0.4);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[role='btn-edit-Article']").First);
            var add = scene.Page.Locator("a:text-is('Add Parts')").First;
            await add.ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(add);
        });

        await scene.StepAsync("allow-2", async () =>
        {
            var row = scene.Page.Locator("li.list-group-item, .list-group-item").Filter(new() { Has = scene.Page.Locator("button[name='submit.AddAndConfigure']") }).Filter(new() { HasTextString = "WidgetsList" }).First;
            await row.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(row);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(row.Locator("button[name='submit.AddAndConfigure']").First);
        });

        await scene.StepAsync("allow-3", async () =>
        {
            var field = scene.Page.GetByLabel("Available widgets zones");
            await scene.HighlightAsync(field, "Available widgets zones");
            await scene.TypeAsync(field, "Content, Footer", delay: 45);
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
        });

        await scene.StepAsync("allow-4", async () =>
        {
            await scene.MoveToAsync(900, 300);
            var parts = scene.Page.Locator("ul#parts > li").Filter(new() { HasTextString = "WidgetsList" }).First;
            await parts.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(parts);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        var content = Group(scene, "Content");

        if (await content.GetAttributeAsync("aria-expanded") == "false")
        {
            await content.ClickAsync();
        }

        await Entry(scene, "Content Items").ClickAsync();
        await scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "About" }).First.WaitForAsync();
        await scene.StartCaptureAsync();

        var footer = scene.Page.Locator(".widget-list-zone").Filter(new() { Has = scene.Page.Locator(".card-header:text-is('Footer')") }).First;
        var contentZone = scene.Page.Locator(".widget-list-zone").Filter(new() { Has = scene.Page.Locator(".card-header:text-is('Content')") }).First;

        await scene.StepAsync("add-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "About" }).First.Locator("a.edit").First);
            await footer.WaitForAsync();
            await footer.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(contentZone.Locator(".card-header").First);
            await scene.HighlightAsync(footer.Locator(".card-header").First);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var plus = footer.Locator("button[title='Add Widget']").First;
            await scene.HighlightAsync(plus, "Add Widget");
            await scene.HoldAsync(1);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(plus);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.ClickAsync(footer.Locator(".dropdown-item.add-list-widget").Filter(new() { HasTextRegex = Exact("Paragraph") }).First);
            var card = footer.Locator(".widget-editor").First;
            var editor = card.Locator(".trumbowyg-editor").First;
            await scene.OffCameraAsync(async () =>
            {
                await card.WaitForAsync();
                var expand = card.Locator(".widget-editor-btn-expand").First;

                if (await expand.IsVisibleAsync())
                {
                    await expand.ClickAsync();
                }

                await editor.WaitForAsync();
            }, leadMilliseconds: 100);
            await scene.TypeAsync(editor, "Open every weekday from nine to five.", delay: 10);
            await contentZone.Locator("button[title='Add Widget']").First.ScrollIntoViewIfNeededAsync();
        });

        await scene.StepAsync("add-4", async () =>
        {
            var plus = contentZone.Locator("button[title='Add Widget']").First;
            await scene.ClickAsync(plus, pauseAfter: 100);
            await scene.ClickAsync(contentZone.Locator(".dropdown-item.add-list-widget").Filter(new() { HasTextRegex = Exact("Blockquote") }).First, pauseAfter: 100);
            var card = contentZone.Locator(".widget-editor").First;
            var quote = card.Locator("textarea").First;
            await scene.OffCameraAsync(async () =>
            {
                await card.WaitForAsync();
                var expand = card.Locator(".widget-editor-btn-expand").First;

                if (await expand.IsVisibleAsync())
                {
                    await expand.ClickAsync();
                }

                await quote.WaitForAsync();
            }, leadMilliseconds: 100);
            await scene.TypeAsync(quote, "Every garden begins with a single seed.", delay: 8);
        });

        await scene.StepAsync("add-5", async () =>
        {
            var publish = scene.Page.Locator("button.publish").First;
            await scene.HighlightAsync(publish, "Publish");
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(publish);
        });

        await scene.StepAsync("add-6", async () =>
        {
            await scene.NavigateAsync("/about");
            var quote = scene.Page.Locator("article.widget-blockquote").First;
            await quote.WaitForAsync();
            await quote.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(quote);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            var paragraph = scene.Page.Locator("article.widget-paragraph").First;
            await paragraph.ScrollIntoViewIfNeededAsync();
            await scene.HoldAsync(0.5);
            await scene.HighlightAsync(paragraph);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "About" }).First.WaitForAsync();
        await scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "About" }).First.Locator("a.edit").First.ClickAsync();
        var footer = scene.Page.Locator(".widget-list-zone").Filter(new() { Has = scene.Page.Locator(".card-header:text-is('Footer')") }).First;
        await footer.WaitForAsync();
        var contentZone = scene.Page.Locator(".widget-list-zone").Filter(new() { Has = scene.Page.Locator(".card-header:text-is('Content')") }).First;
        await contentZone.EvaluateAsync("e => e.scrollIntoView({ block: 'start' })");
        await scene.Page.EvaluateAsync("window.scrollBy(0, -90)");
        await scene.StartCaptureAsync();

        // Every card holds the "Insert Widget" list of all the widget types, so the cards are told apart by their editor.
        var blockquote = scene.Page.Locator(".widget-editor:not(:has(.trumbowyg-editor))").First;
        var paragraph = scene.Page.Locator(".widget-editor:has(.trumbowyg-editor)").First;

        await scene.StepAsync("manage-1", async () =>
        {
            var handle = blockquote.Locator(".widget-editor-handle").First;
            await scene.HighlightAsync(handle);
            await scene.HoldAsync(1);
            await scene.ClearHighlightsAsync();
            await scene.DragToAsync(handle, paragraph.Locator(".card-header, .widget-editor-handle").First);
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await scene.ClickAsync(paragraph.Locator(".widget-editor-btn-collapse").First, pauseAfter: 1000);
            await scene.ClickAsync(paragraph.Locator(".widget-editor-btn-expand").First);
        });

        await scene.StepAsync("manage-3", async () =>
        {
            var header = paragraph.Locator(".card-header").First;
            await scene.MoveToAsync(header);
            await scene.HoldAsync(0.5);
            await scene.HighlightAsync(paragraph.Locator(".btn-widget-metadata .insert-list-widget-toggle, .btn-widget-metadata button, .btn-widget-metadata").First, "Insert Widget");
        });

        await scene.StepAsync("manage-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(blockquote.Locator(".widget-list-delete").First);
            await scene.Page.Locator("#modalOkButton").WaitForAsync();
            await scene.HoldAsync(0.8);
            await scene.ClickAsync(scene.Page.Locator("#modalOkButton").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

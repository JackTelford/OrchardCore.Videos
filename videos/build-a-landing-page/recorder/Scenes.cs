using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Landing page", recorded in order on one fresh site.</summary>
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
        ("03-layer", LayerAsync),
        ("04-zones", ZonesAsync),
        ("05-widget", WidgetAsync),
        ("06-result", ResultAsync),
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

    /// <summary>Nothing to seed. Warms the pages up once, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Contents/ContentItems", "/Admin/Contents/ContentTypes/Page/Create", "/Admin/Layers", "/Admin/Settings/zones", "/" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Pages warmed up.");
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, string group)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(90_000);
        await scene.OpenAsync(path);
        if (group != null)
        {
            await scene.Page.Locator($"button.nav-group-label.oc-navigation-admin-{group}").First.ClickAsync();
            await Task.Delay(500);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static ILocator AddWidget(IPage page) => page.Locator("button[title='Add Widget']").First;

    private static ILocator Editor(IPage page, string type) => page.Locator($".widget-editor[data-content-type='{type}']").First;

    // The size, alignment and insert buttons of a widget card are shown while the pointer is over its header.
    private static async Task HoverMetadataAsync(Scene scene, ILocator card)
    {
        await card.Locator(".widget-editor-header").First.ScrollToMiddleAsync();
        await scene.MoveToAsync(card.Locator(".widget-editor-header").First, 200, 0);
        await scene.PauseAsync(500);
    }

    private static async Task SizeAsync(Scene scene, ILocator card, string size)
    {
        await HoverMetadataAsync(scene, card);
        var toggle = card.Locator("button.dropdown-toggle:visible").Filter(new() { HasTextString = "100%" }).First;
        await scene.ClickAsync(toggle, pauseAfter: 300);
        await scene.ClickAsync(card.Locator("label.dropdown-item").Filter(new() { HasTextString = size }).First, pauseAfter: 300);
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task WaitTextAsync(IPage page, string text, int timeout = 60_000)
        => await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = timeout });

    // Clicks a link or button that loads a new page; the load is cut out of the video (see Scene.OffCameraAsync).
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
    }

    // Opens a page during the capture, with the load cut out of the video.
    private static Task VisitAsync(Scene scene, string path)
        => scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(BaseUrl + path)), leadMilliseconds: 250);

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Row(IPage page, string text) => page.Locator(".list-group-item, tr").Filter(new() { HasTextString = text }).First;

    private static async Task OpenAdminGroupAsync(IPage page, string name)
    {
        var group = page.Locator("#left-nav").GetByText(name, new() { Exact = true }).First;
        await group.ClickAsync();
        await Task.Delay(500);
    }

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", "content");
        var page = scene.Page;

        await scene.StepAsync("page-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[href$='/Contents/ContentItems']").First);
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Page" }).First);
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Spring Workshop", delay: 40);
            await scene.TypeAsync(page.GetByLabel("Permalink").First, "spring-workshop", delay: 40);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("page-2", async () =>
        {
            await scene.ScrollAsync(0, 350);
            await ShowAsync(scene, AddWidget(page), 1000);
            await scene.ClickAsync(AddWidget(page), pauseAfter: 400);
            await page.Locator("div.dropdown-menu.show").First.WaitForAsync();
        });

        await scene.StepAsync("page-3", async () =>
        {
            await scene.ClickAsync(page.Locator("a.dropdown-item.add-widget[data-widget-type='Paragraph']").First, pauseAfter: 400);
            await Editor(page, "Paragraph").WaitForAsync();
            var editor = Editor(page, "Paragraph").Locator(".field-wrapper-paragraph-content .trumbowyg-editor").First;
            await editor.WaitForAsync();
            await scene.ClickAsync(editor, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Join our free two-hour workshop and learn to publish your first website.", new() { Delay = 40 });
        });

        await scene.StepAsync("page-4", async () =>
        {
            await SizeAsync(scene, Editor(page, "Paragraph"), "50%");
        });

        await scene.StepAsync("page-5", async () =>
        {
            await scene.ClickAsync(AddWidget(page), pauseAfter: 400);
            await scene.ClickAsync(page.Locator("a.dropdown-item.add-widget[data-widget-type='Blockquote']").First, pauseAfter: 400);
            var card = Editor(page, "Blockquote");
            await card.WaitForAsync();
            await scene.TypeAsync(card.Locator(".field-wrapper-blockquote-quote textarea").First, "Clear, practical and full of examples. Maya Ortiz", delay: 35);
            await SizeAsync(scene, card, "50%");
            await HoverMetadataAsync(scene, card);
            await scene.ClickAsync(card.Locator(".btn-widget-metadata button.dropdown-toggle:visible").First, pauseAfter: 300);
            await scene.ClickAsync(card.Locator("label.dropdown-item").Filter(new() { HasTextString = "Center" }).First, pauseAfter: 300);
        });

        await scene.StepAsync("page-6", async () =>
        {
            var handle = Editor(page, "Blockquote").Locator(".widget-editor-handle").First;
            await scene.DragToAsync(handle, Editor(page, "Paragraph"), -40, 0);
        });

        await scene.StepAsync("page-7", async () =>
        {
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Page has been published.");
        });

        await scene.StepAsync("page-8", async () =>
        {
            await scene.NavigateAsync("/spring-workshop");
            await page.Locator("section.flow").First.WaitForAsync();
        });
    }

    private static async Task LayerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", "design");
        var page = scene.Page;

        await scene.StepAsync("layer-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[href$='/Layers']").First);
            await ShowAsync(scene, page.GetByRole(AriaRole.Heading, new() { Name = "Zones" }).First, 1200);
            await ShowAsync(scene, page.GetByRole(AriaRole.Heading, new() { Name = "Layers" }).First, 1200);
            await ShowAsync(scene, page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#Always") }).First, 1000);
            await ShowAsync(scene, page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#Homepage") }).First, 1000);
        });

        await scene.StepAsync("layer-2", async () =>
        {
            var row = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#Always") }).First;
            await GoAsync(scene, row.GetByRole(AriaRole.Link, new() { Name = "Edit" }).First);
            var rule = page.Locator("ol.condition-group li").First;
            await rule.WaitForAsync();
            await scene.HighlightAsync(rule);
        });

        await scene.StepAsync("layer-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator(".breadcrumb a").Filter(new() { HasTextString = "Widgets and Layers" }).First);
            await GoAsync(scene, page.Locator("a.btn.btn-primary.btn-sm").Filter(new() { HasTextString = "Add" }).Last);
            await scene.TypeAsync(page.GetByLabel("Name", new() { Exact = true }).First, "Workshop", delay: 40);
            await scene.TypeAsync(page.GetByLabel("Description", new() { Exact = true }).First, "Widgets for the workshop page", delay: 35);
            await GoAsync(scene, page.Locator("input[type='submit'][value='Create']").First);
            await page.GetByText("No conditions found").First.WaitForAsync();
        });

        await scene.StepAsync("layer-4", async () =>
        {
            await ShowAsync(scene, page.GetByText("No conditions found").First, 1500);
            await scene.ClickAsync(page.Locator("button[data-bs-target='#modalConditions']").First, pauseAfter: 400);
            await page.Locator("#modalConditions.show").WaitForAsync();
        });

        await scene.StepAsync("layer-5", async () =>
        {
            await scene.HighlightAsync(page.Locator("#search-box").Last);
            await scene.ScrollAsync(0, 300);
            await scene.PauseAsync(1000);
            await scene.ScrollAsync(0, -300);
        });

        await scene.StepAsync("layer-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#search-box").Last, "Url", delay: 60);
            await GoAsync(scene, page.Locator(".item:visible a.condition-thumbnail").First);
            await scene.TypeAsync(page.GetByLabel("Value", new() { Exact = true }).First, "/spring-workshop", delay: 40);
            await GoAsync(scene, page.Locator("button.create").First);
            await WaitTextAsync(page, "Condition added successfully.");
        });

        await scene.StepAsync("layer-7", async () =>
        {
            await scene.HighlightAsync(page.Locator("ol.condition-group li").First);
        });
    }

    private static async Task ZonesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", "settings");
        var page = scene.Page;

        await scene.StepAsync("zones-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[href$='/Settings/zones']").First);
            await scene.HighlightAsync(page.GetByLabel("Available zones for Layers"));
        });

        await scene.StepAsync("zones-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("span.hint").First);
        });
    }

    private static async Task WidgetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", "design");
        var page = scene.Page;
        var footer = page.Locator(".layer-zone[data-zone='Footer']").First;

        await scene.StepAsync("widget-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[href$='/Layers']").First);
            await footer.WaitForAsync();
            await ShowAsync(scene, footer, 1800);
            await scene.ClickAsync(footer.GetByRole(AriaRole.Button, new() { Name = "Add Widget" }).First, pauseAfter: 400);
            await GoAsync(scene, page.Locator("a.add-list-widget[data-widget-type='Paragraph'][data-zone='Footer']").First);
            await page.GetByLabel("Title", new() { Exact = true }).First.WaitForAsync();
        });

        await scene.StepAsync("widget-2", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Questions about the workshop?", delay: 40);
            await scene.ClickAsync(page.GetByLabel("Render title"), pauseAfter: 300);
            await scene.SelectAsync(page.GetByLabel("The layer"), "Workshop");
        });

        await scene.StepAsync("widget-3", async () =>
        {
            var editor = page.Locator(".field-wrapper-paragraph-content .trumbowyg-editor").First;
            await scene.ClickAsync(editor, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Write to workshop@example.com and we will reply within one day.", new() { Delay = 40 });
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Paragraph has been published.");
        });

        await scene.StepAsync("widget-4", async () =>
        {
            var row = footer.Locator(".list-group-item").Filter(new() { HasTextString = "Questions about the workshop?" }).First;
            await ShowAsync(scene, row, 1500);
            await scene.ClickAsync(page.Locator("#Workshop"), pauseAfter: 400);
        });

        await scene.StepAsync("widget-5", async () =>
        {
            var row = footer.Locator(".list-group-item").Filter(new() { HasTextString = "Questions about the workshop?" }).First;
            var other = footer.Locator(".list-group-item").Filter(new() { HasNotTextString = "Questions about the workshop?" }).First;
            await scene.DragToAsync(row.Locator(".properties").First, other, 0, -10);
            await page.WaitForFunctionAsync("() => !document.querySelector('#layer-undo-message')?.classList.contains('d-none')");
        });
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Layers", null);
        var page = scene.Page;

        await scene.StepAsync("result-1", async () =>
        {
            await scene.NavigateAsync("/spring-workshop");
            await scene.ScrollAsync(0, 4000);
            await scene.HighlightAsync(page.Locator("footer").First);
        });

        await scene.StepAsync("result-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/");
            await scene.ScrollAsync(0, 4000);
            await scene.HighlightAsync(page.Locator("footer").First);
        });

        await scene.StepAsync("result-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Layers");
            await scene.ClickAsync(page.Locator("#Workshop"), pauseAfter: 400);
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the editors.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

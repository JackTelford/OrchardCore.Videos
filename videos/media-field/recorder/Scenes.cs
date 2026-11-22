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

    // The four invented files, made with Pillow outside the repository.
    private static string Seed(string name) => Path.Combine(Environment.GetEnvironmentVariable("MF_SEED") ?? "C:/ocv/s3/mf-seed", name);

    private const string EditQuery = "?returnUrl=%2FAdmin%2FContents%2FContentItems";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-add", AddAsync),
        ("03-settings", SettingsAsync),
        ("04-attach", AttachAsync),
        ("05-gallery", GalleryAsync),
        ("06-attached", AttachedAsync),
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

    // Seeds the library (outside the capture): the folder open-house with the three pictures and the PDF.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1600, Height = 900 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.GotoAsync(BaseUrl + "/Admin/Media", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("div.table-row.file-item").First.WaitForAsync();

        var root = page.Locator("#folder-tree .treeroot");
        await root.HoverAsync();
        await root.Locator(".folder-actions a.action-button").ClickAsync();
        await page.Locator("text='Create a subfolder'").ClickAsync();
        await page.Locator("input[name='create-folder']").FillAsync("open-house");
        await page.Locator("#btn-submit").ClickAsync();
        await page.Locator("#folder-tree li.folder:has-text('open-house') .folder-name").WaitForAsync();
        await Task.Delay(800);
        await page.Locator("#folder-tree li.folder:has-text('open-house') .folder-name").ClickAsync();
        await Task.Delay(1500);

        await page.Locator("#fileupload").SetInputFilesAsync([Seed("spring-orchard.jpg"), Seed("greenhouse.jpg"), Seed("workshop.jpg"), Seed("visitor-guide.pdf")]);
        await page.Locator(".file-items-table .table-row.file-item:has-text(\"visitor-guide.pdf\")").First.WaitForAsync();
        await Task.Delay(6000);
        Console.WriteLine("rows in open-house: " + await page.Locator(".file-items-table .table-row.file-item").CountAsync());
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // Opens the page with the browser's storage cleared, so every take starts from the defaults.
    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    // A shorter cursor path than Scene.ClickAsync: on a loaded machine every pointer move is a slow round trip, and the clicks must land on their sentence.
    private static async Task ClickAsync(Scene scene, ILocator locator, int pauseAfter = 400)
    {
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 12);
        await Task.Delay(100);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await Task.Delay(pauseAfter);
    }

    private static async Task ClickAndNavigateAsync(Scene scene, ILocator locator)
    {
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 12);
        await Task.Delay(100);
        await scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y));
        await Task.Delay(300);
    }

    private static async Task TypeIntoAsync(Scene scene, ILocator field, string text)
    {
        await field.WaitForAsync();
        await ClickAsync(scene, field, 600);
        await field.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = 55 });
        await Task.Delay(300);

        if (await field.InputValueAsync() != text)
        {
            await field.FillAsync(text);
            await Task.Delay(300);
        }
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

    // The edit address of the Page "Spring Open House", read from the Content Items list outside the capture.
    private static async Task<string> PageEditPathAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        var href = await scene.Page.Locator("a[href*='/Admin/Contents/ContentItems/'][href*='/Edit']").Filter(new() { HasText = "Spring Open House" }).First.GetAttributeAsync("href");

        if (href is null)
        {
            var row = scene.Page.Locator("li, tr, .list-group-item").Filter(new() { HasText = "Spring Open House" }).Last;
            href = await row.Locator("a[href*='/Edit']").First.GetAttributeAsync("href");
        }

        var id = href!.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipWhile(part => part != "ContentItems").Skip(1).First();
        Console.WriteLine("page id " + id);

        return $"/Admin/Contents/ContentItems/{id}/Edit{EditQuery}";
    }

    private static ILocator Nav(IPage page, string text) => page.Locator($"#left-nav :is(button, a).item-label:has(span.title:text-is(\"{text}\"))");

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            var step = Stopwatch.StartNew();
            await AtAsync(scene, step, 2.0);
            await ClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Design\"))"), 300);
            var definition = page.Locator("#left-nav ul.nav-group-items[data-title=\"Design\"] button.item-label:has(span.title:text-is(\"Content Definition\"))");
            await AtAsync(scene, step, 3.3);
            await ClickAsync(scene, definition, 300);
            var types = page.Locator("#left-nav ul.nav-group-items[data-title=\"Design\"] a.item-label:has(span.title:text-is(\"Content Types\"))");
            await types.WaitForAsync();
            await AtAsync(scene, step, 4.8);
            await ClickAndNavigateAsync(scene, types);
            await page.Locator("[role=\"btn-edit-Page\"]").WaitForAsync();
        });

        await scene.StepAsync("add-2", async () =>
        {
            var step = Stopwatch.StartNew();
            await AtAsync(scene, step, 0.8);
            await ClickAndNavigateAsync(scene, page.Locator("[role=\"btn-edit-Page\"]"));
            var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Fields", Exact = true }).First;
            await heading.WaitForAsync();
            await AtAsync(scene, step, 3.6);
            await heading.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(heading);
            await AtAsync(scene, step, 5.2);
            await scene.ClearHighlightsAsync();
            await ClickAndNavigateAsync(scene, page.Locator("a:has-text(\"Add Field\")").First);
            await page.Locator("input[name=\"DisplayName\"]").WaitForAsync();
        });

        await scene.StepAsync("add-3", async () =>
        {
            var step = Stopwatch.StartNew();
            var name = page.Locator("input[name=\"DisplayName\"]");
            await TypeIntoAsync(scene, name, "Photos");
            await page.Keyboard.PressAsync("Tab");
            await Task.Delay(500);
            var radio = page.Locator("input[name=\"FieldTypeName\"][value=\"MediaField\"]");
            var label = page.Locator("label[for=\"" + await radio.GetAttributeAsync("id") + "\"]");
            await AtAsync(scene, step, 5.4);
            await ClickAsync(scene, label, 500);
            await AtAsync(scene, step, 8.0);
            await ClickAndNavigateAsync(scene, page.Locator("button[value=\"SaveAndConfigure\"]"));
            await page.GetByText("The field \"Photos\" has been added.").WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/ContentParts/Page/Fields/Photos/Edit");
        await page.Locator("input[name=\"DisplayName\"]").WaitForAsync();
        await scene.StartCaptureAsync();

        ILocator Box(string label) => page.GetByLabel(label, new() { Exact = true }).Locator("xpath=ancestor::div[contains(@class,'form-check')][1]");

        await scene.StepAsync("settings-1", async () =>
        {
            var step = Stopwatch.StartNew();
            await AtAsync(scene, step, 2.0);
            await scene.HighlightAsync(Box("Required"));
            await AtAsync(scene, step, 5.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Box("Multiple"));
            await AtAsync(scene, step, 8.0);
            await scene.ClearHighlightsAsync();
            await AtAsync(scene, step, 9.3);
            await TypeIntoAsync(scene, page.GetByLabel("Hint", new() { Exact = true }), "Choose the photos for this page.");
        });

        await scene.StepAsync("settings-2", async () =>
        {
            var step = Stopwatch.StartNew();
            await scene.HighlightAsync(Box("Allow media text"));
            await AtAsync(scene, step, 3.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Box("Allow anchors"));
            await AtAsync(scene, step, 7.5);
            await ClickAsync(scene, page.GetByLabel("Allow anchors", new() { Exact = true }).Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 300);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            var step = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var all = page.GetByLabel("Allow all default media types", new() { Exact = true });
            await all.ScrollIntoViewIfNeededAsync();
            await AtAsync(scene, step, 4.0);
            await ClickAsync(scene, all.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 300);
            await page.Locator("#MediaExtensionWrapper").WaitForAsync();
            await AtAsync(scene, step, 8.0);
            var image = page.GetByLabel("image types", new() { Exact = true });
            await ClickAsync(scene, image.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 300);
            await ScrollToAsync(all, 150);
            await scene.HighlightAsync(image.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("settings-4", async () =>
        {
            var step = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var editor = page.Locator("select[name*=\"Editor\"]").First;
            await editor.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(editor);
            await scene.MoveToAsync(editor);
            await AtAsync(scene, step, 5.0);
            await scene.ClearHighlightsAsync();
            await ClickAndNavigateAsync(scene, page.Locator("button.save").First);
            await page.GetByText("field settings have been saved").WaitForAsync();
            await ScrollToAsync(page.GetByRole(AriaRole.Heading, new() { Name = "Fields", Exact = true }).First, 110);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AttachAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Contents/ContentItems");
        await page.Locator("#new-dropdown").WaitForAsync();
        var contentItems = page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Content Items\"))").First;

        if (!await contentItems.IsVisibleAsync())
        {
            await page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Content\"))").ClickAsync();
        }

        await contentItems.WaitForAsync();
        await scene.StartCaptureAsync();

        var app = page.Locator(".field-wrapper-page-photos .media-field-app, .media-field-app").First;

        await scene.StepAsync("attach-1", async () =>
        {
            var step = Stopwatch.StartNew();
            var items = page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Content Items\"))").First;
            await scene.HighlightAsync(items);
            await AtAsync(scene, step, 2.0);
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, page.Locator("#new-dropdown"), 300);
            var link = page.Locator("#new-dropdown + .dropdown-menu").GetByRole(AriaRole.Link, new() { Name = "Page", Exact = true });
            await link.WaitForAsync();
            await scene.HighlightAsync(link);
            await AtAsync(scene, step, 5.0);
            await scene.ClearHighlightsAsync();
            await ClickAndNavigateAsync(scene, link);
            await page.Locator("input[name=\"TitlePart.Title\"]").WaitForAsync();
        });

        await scene.StepAsync("attach-2", async () =>
        {
            var step = Stopwatch.StartNew();
            await TypeIntoAsync(scene, page.Locator("input[name=\"TitlePart.Title\"]"), "Spring Open House");
            var wrapper = page.Locator(".field-wrapper-page-photos");
            await wrapper.ScrollIntoViewIfNeededAsync();
            await page.EvaluateAsync("() => { const f = document.querySelector('.field-wrapper-page-photos'); const r = f.getBoundingClientRect(); window.scrollBy(0, r.top - 120); }");
            await Task.Delay(400);
            var hint = wrapper.Locator("text=Choose the photos for this page.");
            await scene.HighlightAsync(hint);
            await scene.HighlightAsync(wrapper.GetByText("No Files"));
            await AtAsync(scene, step, 5.0);
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, wrapper.Locator("button.mf-btn-primary"), 300);
            await page.Locator(".mf-picker-shell").WaitForAsync();
            await scene.OffCameraAsync(() => page.Locator(".mf-picker-shell div.table-row.file-item").First.WaitForAsync());
        });

        await scene.StepAsync("attach-3", async () =>
        {
            var step = Stopwatch.StartNew();
            await ClickAsync(scene, page.Locator(".mf-picker-shell #folder-tree li.folder:has-text(\"open-house\") .folder-name").First, 300);
            await scene.OffCameraAsync(() => page.Locator(".mf-picker-shell div.table-row.file-item:has-text(\"workshop.jpg\")").WaitForAsync(), 300);
            await scene.HighlightAsync(page.Locator(".mf-picker-shell #file-filter-input"));
            await scene.HighlightAsync(page.Locator(".mf-picker-shell label.upload-button"));
            await AtAsync(scene, step, 7.0);
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, page.Locator(".mf-picker-shell div.table-row.file-item:has-text(\"spring-orchard.jpg\") input[name=\"select-file\"]"), 300);
            await ClickAsync(scene, page.Locator(".mf-picker-shell div.table-row.file-item:has-text(\"greenhouse.jpg\") input[name=\"select-file\"]"), 300);
        });

        await scene.StepAsync("attach-4", async () =>
        {
            var step = Stopwatch.StartNew();
            await scene.HighlightAsync(page.Locator(".mf-picker-shell .mf-modal-footer").GetByText("2 selected"));
            await AtAsync(scene, step, 3.0);
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, page.Locator(".mf-picker-shell .mf-modal-footer .mf-btn-primary"), 300);
            await page.Locator(".mf-picker-shell").WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await page.Locator("li.mf-thumb-item img").Nth(1).WaitForAsync();
        });

        await scene.StepAsync("attach-5", async () =>
        {
            var step = Stopwatch.StartNew();
            await ClickAsync(scene, page.Locator("li.mf-thumb-item:has-text(\"spring-orchard.jpg\")").First, 400);
            var toolbar = page.Locator(".media-field-app");
            await AtAsync(scene, step, 2.5);
            await scene.HighlightAsync(toolbar.Locator("button:has-text(\"Remove\")").First);
            await AtAsync(scene, step, 4.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(toolbar.GetByRole(AriaRole.Button, new() { Name = "Media text" }));
            await AtAsync(scene, step, 6.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(toolbar.GetByRole(AriaRole.Button, new() { Name = "Anchor" }));
            await AtAsync(scene, step, 8.0);
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, toolbar.GetByRole(AriaRole.Button, new() { Name = "Media text" }), 300);
            await page.Locator(".mf-form-textarea").WaitForAsync();
        });

        await scene.StepAsync("attach-6", async () =>
        {
            var step = Stopwatch.StartNew();
            await TypeIntoAsync(scene, page.Locator(".mf-form-textarea"), "Spring orchard");
            await AtAsync(scene, step, 3.5);
            await ClickAsync(scene, page.Locator(".mf-modal .mf-modal-footer .mf-btn-primary"), 300);
            await page.Locator(".mf-form-textarea").WaitForAsync(new() { State = WaitForSelectorState.Detached });
        });

        await scene.StepAsync("attach-7", async () =>
        {
            var step = Stopwatch.StartNew();
            await ClickAsync(scene, page.Locator(".media-field-app").GetByRole(AriaRole.Button, new() { Name = "Anchor" }), 400);
            var picture = page.Locator(".mf-modal img").First;
            await picture.WaitForAsync();
            await scene.OffCameraAsync(() => page.WaitForFunctionAsync("() => { const i = document.querySelector('.mf-modal img'); return i && i.complete && i.naturalWidth > 0; }"), 300);
            var box = await picture.BoundingBoxAsync();
            await AtAsync(scene, step, 3.5);
            await scene.MoveToAsync(box!.X + box.Width * 0.7, box.Y + box.Height * 0.35, 14);
            await Task.Delay(100);
            await page.Mouse.ClickAsync((float)(box.X + box.Width * 0.7), (float)(box.Y + box.Height * 0.35));
            await AtAsync(scene, step, 6.5);
            var reset = page.Locator(".mf-modal").GetByRole(AriaRole.Button, new() { Name = "Reset Anchor" });
            await scene.HighlightAsync(reset);
            await AtAsync(scene, step, 9.0);
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, page.Locator(".mf-modal .mf-modal-footer .mf-btn-primary"), 300);
        });

        await scene.StepAsync("attach-8", async () =>
        {
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await ClickAndNavigateAsync(scene, page.Locator("button.publish").First);
            await page.GetByText("Your Page has been published.").WaitForAsync();
        });

        await scene.StepAsync("attach-9", async () =>
        {
            await scene.NavigateAsync("/spring-open-house");
            await page.WaitForFunctionAsync("() => { const i = [...document.querySelectorAll('.field-name-page-photos img')]; return i.length >= 2 && i.every(x => x.complete && x.naturalWidth > 0); }");
            await page.EvaluateAsync("() => { document.body.style.zoom = '0.34'; }");
            await Task.Delay(300);
            await page.EvaluateAsync("() => { const f = document.querySelector('.field-name-page-photos'); const r = f.getBoundingClientRect(); window.scrollBy(0, r.top - 24); }");
            await Task.Delay(500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task GalleryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        var edit = await PageEditPathAsync(scene);
        await OpenCleanAsync(scene, "/Admin/ContentParts/Page/Fields/Photos/Edit");
        await page.Locator("input[name=\"DisplayName\"]").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("gallery-1", async () =>
        {
            var step = Stopwatch.StartNew();
            var editor = page.Locator("select[name*=\"Editor\"]").First;
            await editor.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(editor);
            await AtAsync(scene, step, 4.0);
            await scene.SelectAsync(editor, "Gallery");
            await AtAsync(scene, step, 9.4);
            await scene.ClearHighlightsAsync();
            await ClickAndNavigateAsync(scene, page.Locator("button.save").First);
            await page.GetByText("field settings have been saved").WaitForAsync();
            await ScrollToAsync(page.GetByRole(AriaRole.Heading, new() { Name = "Fields", Exact = true }).First, 110);
        });

        await scene.StepAsync("gallery-2", async () =>
        {
            var step = Stopwatch.StartNew();
            await scene.NavigateAsync(edit);
            var card = page.Locator(".field-wrapper-page-photos .mf-gallery-add-card");
            await card.WaitForAsync();
            await page.WaitForFunctionAsync("() => [...document.querySelectorAll('.field-wrapper-page-photos img')].every(x => x.complete)");
            await page.EvaluateAsync("() => { const f = document.querySelector('.field-wrapper-page-photos'); const r = f.getBoundingClientRect(); window.scrollBy(0, r.top - 120); }");
            await Task.Delay(400);
            var first = page.Locator(".field-wrapper-page-photos .mf-gallery-card, .field-wrapper-page-photos ol.mf-gallery-cards > li").First;
            var buttons = first.Locator("button");
            var count = await buttons.CountAsync();

            for (var i = 0; i < count; i++)
            {
                await scene.HighlightAsync(buttons.Nth(i));
                await Task.Delay(1000);
                await scene.ClearHighlightsAsync();
            }

            await scene.HighlightAsync(card);
            await Task.Delay(600);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("gallery-3", async () =>
        {
            var step = Stopwatch.StartNew();
            await ClickAsync(scene, page.Locator("button[title=\"List view\"]").First, 200);
            await page.Locator(".field-wrapper-page-photos ol.mf-gallery-list").WaitForAsync();
            await AtAsync(scene, step, 1.0);
            await ClickAsync(scene, page.Locator("button[title=\"Grid view\"]").First, 300);
            await page.Locator(".field-wrapper-page-photos ol.mf-gallery-cards").WaitForAsync();
            var cards = page.Locator(".field-wrapper-page-photos ol.mf-gallery-cards > li");
            var (fx, fy) = await scene.CenterOfAsync(cards.Nth(1));
            var (tx, ty) = await scene.CenterOfAsync(cards.Nth(0));
            await scene.MoveToAsync(fx, fy, 10);
            await page.Mouse.DownAsync();
            await scene.MoveToAsync(tx, ty, 18);
            await Task.Delay(100);
            await page.Mouse.UpAsync();
            await Task.Delay(300);
            await ClickAndNavigateAsync(scene, page.Locator("button.publish").First);
            await page.GetByText("Your Page has been published.").WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AttachedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        var edit = await PageEditPathAsync(scene);
        await OpenCleanAsync(scene, "/Admin/ContentTypes/Edit/Page");
        await page.GetByRole(AriaRole.Heading, new() { Name = "Fields", Exact = true }).First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("attached-1", async () =>
        {
            var step = Stopwatch.StartNew();
            await ClickAndNavigateAsync(scene, page.Locator("a:has-text(\"Add Field\")").First);
            await TypeIntoAsync(scene, page.Locator("input[name=\"DisplayName\"]"), "Visitor Guide");
            await page.Keyboard.PressAsync("Tab");
            await Task.Delay(400);
            var radio = page.Locator("input[name=\"FieldTypeName\"][value=\"MediaField\"]");
            await ClickAsync(scene, page.Locator("label[for=\"" + await radio.GetAttributeAsync("id") + "\"]"), 400);
            await AtAsync(scene, step, 7.0);
            await ClickAndNavigateAsync(scene, page.Locator("button[value=\"SaveAndConfigure\"]"));
            await page.GetByText("The field \"Visitor Guide\" has been added.").WaitForAsync();
        });

        await scene.StepAsync("attached-2", async () =>
        {
            var step = Stopwatch.StartNew();
            ILocator Label(string text) => page.GetByLabel(text, new() { Exact = true }).Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label");
            await ClickAsync(scene, Label("Multiple"), 300);
            await AtAsync(scene, step, 3.0);
            await ClickAsync(scene, Label("Allow all default media types"), 300);
            await page.Locator("#MediaExtensionWrapper").WaitForAsync();
            await AtAsync(scene, step, 5.5);
            await ClickAsync(scene, Label(".pdf"), 300);
            await AtAsync(scene, step, 8.0);
            var editor = page.Locator("select[name*=\"Editor\"]").First;
            await editor.ScrollIntoViewIfNeededAsync();
            await scene.SelectAsync(editor, "Attached");
            await AtAsync(scene, step, 10.5);
            await ClickAndNavigateAsync(scene, page.Locator("button.save").First);
            await page.GetByText("field settings have been saved").WaitForAsync();
            await ScrollToAsync(page.GetByRole(AriaRole.Heading, new() { Name = "Fields", Exact = true }).First, 110);
        });

        await scene.StepAsync("attached-3", async () =>
        {
            var step = Stopwatch.StartNew();
            await scene.NavigateAsync(edit);
            var wrapper = page.Locator(".field-wrapper-page-visitor-guide");
            await wrapper.WaitForAsync();
            await page.EvaluateAsync("() => { const f = document.querySelector('.field-wrapper-page-visitor-guide'); const r = f.getBoundingClientRect(); window.scrollBy(0, r.top - 200); }");
            await Task.Delay(400);
            var add = wrapper.Locator("label:has-text(\"Add media\")").First;
            await scene.HighlightAsync(add);
            await AtAsync(scene, step, 3.0);
            // The Upload endpoint of this build reads the folder from the query string only, but the field posts it in the form:
            // the file would land in the library root and be refused when the page is saved. Put the folder in the query, as the library does.
            var tempFolder = await wrapper.Locator(".media-field-app").GetAttributeAsync("data-temp-upload-folder");
            await page.RouteAsync("**/api/media/Upload", route => route.ContinueAsync(new() { Url = route.Request.Url.Split('?')[0] + "?path=" + Uri.EscapeDataString(tempFolder!) }));
            await wrapper.Locator("input[type=file]").SetInputFilesAsync(Seed("visitor-guide.pdf"));
            await scene.ClearHighlightsAsync();
            await wrapper.GetByText("visitor-guide.pdf").First.WaitForAsync();
            await Task.Delay(800);
            await ScrollToAsync(wrapper, 300);
        });

        await scene.StepAsync("attached-4", async () =>
        {
            await ClickAndNavigateAsync(scene, page.Locator("button.publish").First);
            await page.GetByText("Your Page has been published.").WaitForAsync();
        });

        await scene.StepAsync("attached-5", async () =>
        {
            await scene.NavigateAsync("/spring-open-house");
            var link = page.Locator(".field-name-page-visitor-guide a");
            await link.WaitForAsync();
            await page.EvaluateAsync("() => { document.body.style.zoom = '0.4'; }");
            await Task.Delay(300);
            await ScrollToAsync(page.Locator(".field-name-page-photos"), 10);
            await scene.HighlightAsync(link);
        });

        await scene.ClearHighlightsAsync();
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Multilingual site", recorded in order on one fresh site.</summary>
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
        ("03-cultures", CulturesAsync),
        ("04-part", PartAsync),
        ("05-translate", TranslateAsync),
        ("06-picker", PickerAsync),
        ("07-try", TryAsync),
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

    /// <summary>Replaces the Latin text of the About page with English text (the seed), outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems");
        var row = page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "About" }).First;
        await row.Locator("a.edit").First.ClickAsync();
        var editor = page.Locator(".trumbowyg-editor").First;
        await editor.WaitForAsync();
        await editor.ClickAsync();
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.TypeAsync("We run a small garden café. The menu follows the seasons, and every dish is made with local produce.");
        await page.Locator("button.publish").First.ClickAsync();
        await WaitTextAsync(page, "has been published");

        foreach (var path in new[] { "/Admin/Features", "/about", "/" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Seed done.");
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static async Task GoOffAsync(Scene scene, ILocator locator, int lead = 250)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: lead);
    }

    private static ILocator Nav(IPage page, string href) => page.Locator($"#left-nav a[href$='{href}']").First;

    private static async Task SearchRowsAsync(Scene scene, string text)
    {
        await scene.ClickAsync(scene.Page.Locator("#search-box"), pauseAfter: 150);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await scene.Page.Keyboard.TypeAsync(text, new() { Delay = 50 });
        await scene.Page.WaitForFunctionAsync("() => { const rows = [...document.querySelectorAll('[data-filter-value]:not(.d-none)')]; return rows.length > 0 && rows.every(r => r.dataset.filterValue.toLowerCase().includes('localiza')); }");
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

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, Nav(page, "/Admin/Features"));
            await SearchRowsAsync(scene, "localization");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var row = page.Locator("[data-filter-value*='ContentCulturePicker']").First;
            await ShowAsync(scene, row, 1500);
            await scene.HighlightAsync(row.Locator(".badge").Filter(new() { HasTextString = "Content Localization" }).First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await GoOffAsync(scene, row.Locator("a.btn-primary").Filter(new() { HasTextString = "Enable" }).First);
            await WaitTextAsync(page, "was enabled", 120_000);
            await scene.MoveToAsync(900, 650);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await SearchRowsAsync(scene, "localization");
            var shown = page.Locator("[data-filter-value]:not(.d-none)");
            var count = await shown.CountAsync();
            for (var i = 0; i < count; i++)
            {
                await scene.HighlightAsync(shown.Nth(i).Locator(".badge").Filter(new() { HasTextString = "Enabled" }).First);
                await scene.PauseAsync(800);
            }
        });
    }

    private static async Task CulturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;
        var rows = page.Locator(".localization-settings-wrapper table tbody tr");

        await scene.StepAsync("cultures-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Settings");
            await OpenAdminGroupAsync(page, "Localization");
            await GoAsync(scene, Nav(page, "/Admin/Settings/localization"));
            await rows.First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".localization-settings-wrapper table").First);
        });

        await scene.StepAsync("cultures-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(page.Locator(".localization-settings-wrapper select").First, "fr-FR");
            await scene.ClickAsync(page.Locator(".localization-settings-wrapper a.btn-success").First, pauseAfter: 400);
            await page.WaitForFunctionAsync("() => document.querySelectorAll('.localization-settings-wrapper table tbody tr').length === 2");
        });

        await scene.StepAsync("cultures-3", async () =>
        {
            await ShowAsync(scene, page.Locator(".localization-settings-wrapper th").Filter(new() { HasTextString = "Default culture" }).First, 1500);
            await scene.HighlightAsync(page.Locator("#customRadio_0"));
        });

        await scene.StepAsync("cultures-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.Locator("button.save").First.ScrollToMiddleAsync();
            await GoOffAsync(scene, page.Locator("button.save").First);
            await WaitTextAsync(page, "Site settings updated successfully.", 120_000);
            await scene.MoveToAsync(900, 650);
        });
    }

    private static async Task PartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("part-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Design");
            await OpenAdminGroupAsync(page, "Content Definition");
            await GoAsync(scene, Nav(page, "/Admin/ContentTypes/List"));
            await page.Locator("[role='btn-edit-Article']").First.ScrollToMiddleAsync();
            await GoAsync(scene, page.Locator("[role='btn-edit-Article']").First);
            await page.Locator("h3").Filter(new() { HasTextString = "Parts" }).First.WaitForAsync();
        });

        await scene.StepAsync("part-2", async () =>
        {
            var add = page.Locator("a").Filter(new() { HasTextString = "Add Parts" }).First;
            await add.ScrollToMiddleAsync();
            await GoAsync(scene, add);
            var row = page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("label.form-check-label:text-is('Localization')") }).First;
            await row.ScrollToMiddleAsync();
            await scene.ClickAsync(row.Locator("label.form-check-label").First, pauseAfter: 400);
            await GoAsync(scene, page.Locator("#add-selected-parts-button"));
        });

        await scene.StepAsync("part-3", async () =>
        {
            await ShowAsync(scene, page.GetByText("part has been added").First, 1500);
            var parts = page.Locator("#parts");
            await parts.ScrollToMiddleAsync();
            await scene.HighlightAsync(parts.GetByText("Localization").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await page.Locator("button.save").First.ScrollToMiddleAsync();
            await GoAsync(scene, page.Locator("button.save").First);
            await WaitTextAsync(page, "Content type updated successfully.");
        });
    }

    private static async Task TranslateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;
        ILocator AboutRow() => page.Locator(".list-group-item, tr").Filter(new() { Has = page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true }) }).First;

        await scene.StepAsync("translate-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Content");
            await GoAsync(scene, page.Locator("#left-nav a").Filter(new() { HasTextString = "Content Items" }).First);
            await scene.HighlightAsync(AboutRow());
        });

        await scene.StepAsync("translate-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, AboutRow().Locator("a.edit").First);
            var panel = page.Locator("#contentLocalizationToggle");
            await panel.ScrollToMiddleAsync();
            await scene.HighlightAsync(panel);
        });

        await scene.StepAsync("translate-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#contentLocalizationToggle button").First, pauseAfter: 500);
            await scene.PauseAsync(1500);
            await page.Locator("button.publish").First.ScrollToMiddleAsync();
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Article has been published.");
        });

        await scene.StepAsync("translate-4", async () =>
        {
            var row = AboutRow();
            await scene.HighlightAsync(row.Locator(".badge").Filter(new() { HasTextString = "en-US" }).First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("button.localizations").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(row.Locator("button.localizations").First, pauseAfter: 500);
            await scene.HighlightAsync(page.Locator(".dropdown-menu.show a.dropdown-item").Filter(new() { HasTextString = "fr-FR" }).First);
        });

        await scene.StepAsync("translate-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator(".dropdown-menu.show a.dropdown-item").Filter(new() { HasTextString = "fr-FR" }).First);
            var panel = page.Locator("#contentLocalizationToggle");
            await panel.ScrollToMiddleAsync();
            await scene.ClickAsync(panel.Locator("button").First, pauseAfter: 500);
            await scene.HighlightAsync(panel);
        });

        await scene.StepAsync("translate-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var title = page.GetByLabel("Title", new() { Exact = true }).First;
            await title.ScrollToMiddleAsync();
            await scene.ClickAsync(title, pauseAfter: 150);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("À propos", new() { Delay = 40 });
            var link = page.GetByLabel("Permalink").First;
            await scene.ClickAsync(link, pauseAfter: 150);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("a-propos", new() { Delay = 40 });
            var body = page.Locator(".trumbowyg-editor").First;
            await scene.ClickAsync(body, pauseAfter: 150);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("Nous tenons un petit café-jardin. Notre carte suit les saisons.", new() { Delay = 25 });
        });

        await scene.StepAsync("translate-7", async () =>
        {
            await page.Locator("button.publish").First.ScrollToMiddleAsync();
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Article has been published.");
            await scene.HighlightAsync(page.Locator(".badge").Filter(new() { HasTextString = "fr-FR" }).First);
            await scene.HighlightAsync(page.Locator(".badge").Filter(new() { HasTextString = "en-US" }).First);
        });

        await scene.StepAsync("translate-8", async () =>
        {
            await scene.ClearHighlightsAsync();
            var picker = page.Locator("button[title='Culture'], .bootstrap-select.culture-selectpicker button.dropdown-toggle").First;
            await scene.ClickAsync(picker, pauseAfter: 500);
            var entry = page.Locator(".dropdown-menu.show li a, .dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "fr-FR" }).First;
            await scene.MoveToAsync(entry);
            var (x, y) = await scene.CenterOfAsync(entry);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
        });
    }

    private static async Task PickerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("picker-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Design");
            await GoAsync(scene, Nav(page, "/Admin/Themes"));
            await ShowAsync(scene, page.Locator(".theme-card").Filter(new() { HasTextString = "The Blog Theme" }).First, 1500);
            await ShowAsync(scene, page.Locator(".theme-card").Filter(new() { HasTextString = "The Default Theme" }).First, 1500);
            var make = page.Locator(".theme-card").Filter(new() { HasTextString = "The Default Theme" }).First.GetByRole(AriaRole.Link, new() { Name = "Make Current" }).Or(page.Locator(".theme-card").Filter(new() { HasTextString = "The Default Theme" }).First.GetByRole(AriaRole.Button, new() { Name = "Make Current" })).First;
            await GoAsync(scene, make);
        });

        await scene.StepAsync("picker-2", async () =>
        {
            await OpenAdminGroupAsync(page, "Settings");
            await OpenAdminGroupAsync(page, "Localization");
            await GoAsync(scene, Nav(page, "/Admin/Settings/ContentCulturePicker"));
            await ShowAsync(scene, page.GetByLabel("Set Cookie"), 1500);
            await scene.HighlightAsync(page.GetByLabel("Homepage redirect"));
        });

        await scene.StepAsync("picker-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#left-nav a").Filter(new() { HasTextString = "Content Culture" }).Last);
            await scene.HighlightAsync(page.GetByLabel("Set Cookie"));
        });
    }

    private static async Task TryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/");
        var page = scene.Page;
        var picker = page.Locator("#oc-culture-picker").First;

        await scene.StepAsync("try-1", async () =>
        {
            await GoAsync(scene, page.Locator("nav a").Filter(new() { HasTextString = "About" }).First);
            await picker.WaitForAsync();
            await scene.HighlightAsync(picker);
        });

        await scene.StepAsync("try-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(picker, pauseAfter: 400);
            var entry = page.Locator(".dropdown-menu.show a, .dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "français" }).First;
            await ShowAsync(scene, entry, 1500);
            await GoAsync(scene, entry);
        });

        await scene.StepAsync("try-3", async () =>
        {
            await WaitTextAsync(page, "Notre carte suit les saisons.");
            await scene.HighlightAsync(picker);
        });

        await scene.StepAsync("try-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(picker, pauseAfter: 400);
            await GoAsync(scene, page.Locator(".dropdown-menu.show a, .dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "English" }).First);
        });

        await scene.StepAsync("try-5", async () =>
        {
            await GoAsync(scene, page.Locator("nav a").Filter(new() { HasTextString = "Home" }).First);
            await scene.ClickAsync(picker, pauseAfter: 400);
            await GoAsync(scene, page.Locator(".dropdown-menu.show a, .dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "français" }).First);
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

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Alias part", recorded in order on one fresh Blog site.</summary>
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
        ("03-add", AddAsync),
        ("04-generate", GenerateAsync),
        ("05-unique", UniqueAsync),
        ("06-disabled", DisabledAsync),
        ("08-use", UseAsync),
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

    /// <summary>Nothing to seed: the Blog recipe is the data. Warms the pages up once, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Features", "/Admin/ContentTypes/List", "/Admin/ContentTypes/Edit/Article", "/Admin/Contents/ContentItems", "/Admin/Contents/ContentTypes/Article/Create", "/" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Pages warmed up.");
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

    private static ILocator AliasField(IPage page) => page.Locator("input[name$='AliasPart.Alias']").First;

    private static ILocator TitleField(IPage page) => page.GetByLabel("Title", new() { Exact = true }).First;

    private static ILocator OptionsSelect(IPage page) => page.Locator("select[id$='Options']").First;

    private static async Task ShowAliasAsync(Scene scene)
    {
        var field = AliasField(scene.Page);
        await field.ScrollToMiddleAsync();
        await scene.HighlightAsync(field);
    }

    private static async Task SaveSettingsAsync(Scene scene)
    {
        await GoAsync(scene, scene.Page.Locator("button.save[name='submit.Save']").First);
        await WaitTextAsync(scene.Page, "part settings have been saved");
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<IPage, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene.Page);
        }

        await scene.StartCaptureAsync();
        return scene;
    }

    private static async Task FeatureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("feature-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, page.Locator("a[href$='/Admin/Features']").First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("alias", new() { Delay = 50 });
            var row = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("span.fw-semibold", new() { HasTextString = "Alias" }) }).Filter(new() { HasTextString = "OrchardCore.Alias" }).First;
            await row.WaitForAsync();
            await ShowAsync(scene, page.Locator("h3:text-is('Content Management')").First, 1500);
            await scene.HighlightAsync(row.Locator(".badge", new() { HasTextString = "Enabled" }).First);
        });
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("add-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Design");
            await OpenAdminGroupAsync(page, "Content Definition");
            await GoAsync(scene, page.Locator("a[href$='/Admin/ContentTypes/List']").First);
            await GoAsync(scene, page.Locator("a[role='btn-edit-Article']"));
            await page.Locator("h3").Filter(new() { HasTextString = "Parts" }).First.WaitForAsync();
            var parts = page.Locator("h3").Filter(new() { HasTextString = "Parts" }).First;
            await parts.ScrollToMiddleAsync();
            await scene.MoveToAsync(page.Locator("a[href*='/ContentParts/'][href$='/Edit']").First);
            await scene.MoveToAsync(page.Locator("a[href$='/ContentTypes/AddPartsTo/Article']"), -150, 0);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await GoAsync(scene, page.Locator("a[href$='/ContentTypes/AddPartsTo/Article']"));
            var row = page.Locator("li.list-group-item:has(label.form-check-label:text-is('Alias'))").First;
            await row.ScrollToMiddleAsync();
            await ShowAsync(scene, row, 1500);
            await GoAsync(scene, row.Locator("button[name='submit.AddAndConfigure']").First);
            await OptionsSelect(page).WaitForAsync();
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.HighlightAsync(OptionsSelect(page));
            await scene.SelectAsync(OptionsSelect(page), "GeneratedDisabled");
            await scene.PauseAsync(1200);
            await scene.SelectAsync(OptionsSelect(page), "Editable");
        });

        await scene.StepAsync("add-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, page.Locator("label").Filter(new() { HasTextString = "Pattern" }).First, 1500);
            await SaveSettingsAsync(scene);
        });
    }

    private static async Task GenerateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("generate-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Content");
            await GoAsync(scene, page.Locator("a[href$='/Admin/Contents/ContentItems']").First);
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Article" }).First);
            await TitleField(page).WaitForAsync();
            await scene.TypeAsync(TitleField(page), "Contact Us", delay: 45);
            await ShowAliasAsync(scene);
        });

        await scene.StepAsync("generate-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Article has been published.");
            await GoAsync(scene, Row(page, "Contact Us").Locator("a.edit").First);
            await page.WaitForFunctionAsync("() => document.querySelector('input[name$=\"AliasPart.Alias\"]')?.value === 'contact-us'");
            await ShowAliasAsync(scene);
        });

        await scene.StepAsync("generate-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TitleField(page).ScrollToMiddleAsync();
            await scene.ClickAsync(TitleField(page), pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("Contact", new() { Delay = 50 });
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Article has been published.");
            await GoAsync(scene, page.Locator(".list-group-item, tr").Filter(new() { Has = page.GetByRole(AriaRole.Link, new() { Name = "Contact", Exact = true }) }).First.Locator("a.edit").First);
            await page.WaitForFunctionAsync("() => document.querySelector('input[name$=\"AliasPart.Alias\"]')?.value === 'contact-us'");
            await ShowAliasAsync(scene);
        });

        await scene.StepAsync("generate-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(AliasField(page), pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.PressAsync("Delete");
            await scene.PauseAsync(800);
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Article has been published.");
            await GoAsync(scene, page.Locator(".list-group-item, tr").Filter(new() { Has = page.GetByRole(AriaRole.Link, new() { Name = "Contact", Exact = true }) }).First.Locator("a.edit").First);
            await page.WaitForFunctionAsync("() => document.querySelector('input[name$=\"AliasPart.Alias\"]')?.value === 'contact'");
            await ShowAliasAsync(scene);
        });
    }

    private static async Task UniqueAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentItems", p => p.Locator("#new-dropdown").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("unique-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Article" }).First);
            await TitleField(page).WaitForAsync();
            await scene.TypeAsync(TitleField(page), "Help Center", delay: 45);
            await AliasField(page).ScrollToMiddleAsync();
            await scene.TypeAsync(AliasField(page), "main-menu", delay: 45);
            await GoAsync(scene, page.Locator("button.publish").First);
            var message = page.Locator(".field-validation-error").Filter(new() { HasTextString = "already in use" }).First;
            await message.WaitForAsync();
            await ShowAliasAsync(scene);
        });
    }

    private static async Task DisabledAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/ContentTypes/Edit/Article", p => p.Locator("a[href*='/ContentParts/AliasPart/Edit']").First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("disabled-1", async () =>
        {
            var edit = page.Locator("a[href*='/ContentParts/AliasPart/Edit']").First;
            await edit.ScrollToMiddleAsync();
            await GoAsync(scene, edit);
            await OptionsSelect(page).WaitForAsync();
            await scene.SelectAsync(OptionsSelect(page), "GeneratedDisabled");
            await SaveSettingsAsync(scene);
        });

        await scene.StepAsync("disabled-2", async () =>
        {
            await VisitAsync(scene, "/Admin/Contents/ContentItems");
            await GoAsync(scene, Row(page, "About").Locator("a.edit").First);
            await AliasField(page).WaitForAsync();
            await ShowAliasAsync(scene);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Article has been published.");
            await GoAsync(scene, Row(page, "About").Locator("a.edit").First);
            await page.WaitForFunctionAsync("() => document.querySelector('input[name$=\"AliasPart.Alias\"]')?.value === 'about'");
            await ShowAliasAsync(scene);
        });
    }

    private static async Task UseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("use-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Content");
            await GoAsync(scene, page.Locator("#left-nav a").Filter(new() { HasTextString = "Menus" }).First);
            await GoAsync(scene, Row(page, "Main Menu").Locator("a.edit").First);
            await AliasField(page).WaitForAsync();
            await ShowAliasAsync(scene);
        });

        await scene.StepAsync("use-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await VisitAsync(scene, "/");
            await page.Locator("#mainNav").WaitForAsync();
            await ShowAsync(scene, page.Locator("#mainNav a:text-is('Home')").First, 1500);
            await scene.HighlightAsync(page.Locator("#mainNav a:text-is('About')").First);
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

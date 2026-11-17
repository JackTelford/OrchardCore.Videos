using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Home Page", recorded in order on one fresh Blog site that has one extra article.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string ArticleTitle = "Welcome to Juniper Lane Studio";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-allow", AllowAsync),
        ("04-set", SetAsync),
        ("05-change", ChangeAsync),
        ("06-permission", PermissionAsync),
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

    /// <summary>Creates the article "Welcome to Juniper Lane Studio", outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Article/Create");
        await page.GetByLabel("Title", new() { Exact = true }).First.FillAsync(ArticleTitle);
        await page.GetByLabel("Subtitle").First.FillAsync("Handmade pottery and weekend workshops");
        var editor = page.Locator(".trumbowyg-editor, .ck-editor__editable, .mce-content-body, .wysiwyg-editor").First;
        await editor.ClickAsync();
        await page.Keyboard.TypeAsync("Juniper Lane Studio is a small pottery workshop. Visit the shop, join a weekend class, or book a private lesson.");
        await page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }).First.ClickAsync();
        await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", "has been published");

        foreach (var path in new[] { "/Admin/Features", "/Admin/ContentTypes/List", "/Admin/Contents/ContentItems", "/Admin/Roles", "/" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Article created.");
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

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await OpenAdminGroupAsync(page, "Tools");
        await page.Locator("a[href$='/Admin/Features']").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[href$='/Admin/Features']").First);
            await scene.TypeAsync(page.Locator("#search-box"), "Home Route", delay: 40);
            var row = page.Locator("[data-filter-value]", new() { HasTextString = "OrchardCore.HomeRoute" }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("Autoroute", new() { Delay = 40 });
            var row = page.Locator("[data-filter-value]", new() { HasTextString = "OrchardCore.Autoroute" }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });
    }

    private static async Task AllowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await OpenAdminGroupAsync(page, "Design");
        await OpenAdminGroupAsync(page, "Content Definition");
        await page.Locator("a[href$='/Admin/ContentTypes/List']").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("allow-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[href$='/Admin/ContentTypes/List']").First);
            await GoAsync(scene, page.Locator("a[role='btn-edit-Article']"));
            await page.Locator("h3").Filter(new() { HasTextString = "Parts" }).First.WaitForAsync();
        });

        await scene.StepAsync("allow-2", async () =>
        {
            var edit = page.Locator("a[href*='/ContentTypes/Article/ContentParts/AutoroutePart/Edit']").First;
            await edit.ScrollToMiddleAsync();
            await ShowAsync(scene, edit, 1500);
            await GoAsync(scene, edit);
            await page.GetByLabel("Show homepage options").WaitForAsync();
        });

        await scene.StepAsync("allow-3", async () =>
        {
            var row = page.Locator(".form-check").Filter(new() { Has = page.Locator("input[name$='ShowHomepageOption']") }).First;
            await row.ScrollToMiddleAsync();
            await scene.HighlightAsync(row);
        });
    }

    private static async Task SetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.HoldAsync(2);
            await scene.ScrollAsync(0, 300);
        });

        await scene.StepAsync("set-2", async () =>
        {
            await VisitAsync(scene, "/Admin/Contents/ContentItems");
            await Row(page, ArticleTitle).WaitForAsync();
            await GoAsync(scene, Row(page, ArticleTitle).Locator("a.edit").First);
            await page.GetByLabel("Permalink").WaitForAsync();
        });

        await scene.StepAsync("set-3", async () =>
        {
            await scene.HighlightAsync(page.GetByLabel("Permalink"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            var row = page.Locator(".form-check").Filter(new() { Has = page.Locator("input[name$='SetHomepage']") }).First;
            await row.ScrollToMiddleAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("set-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("input[type='checkbox'][name$='SetHomepage']"), pauseAfter: 300);
            await scene.PauseAsync(1500);
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "has been published");
        });

        await scene.StepAsync("set-5", async () =>
        {
            await VisitAsync(scene, "/");
            await WaitTextAsync(page, ArticleTitle);
        });

        await scene.StepAsync("set-6", async () =>
        {
            await VisitAsync(scene, "/Admin/Contents/ContentItems");
            await GoAsync(scene, Row(page, ArticleTitle).Locator("a.edit").First);
            var note = page.Locator(".alert.alert-info").Filter(new() { HasTextString = "This content item is the current homepage" }).First;
            await note.WaitForAsync();
            await note.ScrollToMiddleAsync();
            await scene.HighlightAsync(note);
        });
    }

    private static async Task ChangeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await Row(page, ArticleTitle).WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("change-1", async () =>
        {
            var blogRow = page.Locator(".list-group-item, tr").Filter(new() { Has = page.GetByRole(AriaRole.Link, new() { Name = "Blog", Exact = true }) }).Filter(new() { HasNotText = "Welcome to" }).Last;
            await GoAsync(scene, blogRow.Locator("a.edit").First);
            await scene.ClickAsync(page.Locator("input[type='checkbox'][name$='SetHomepage']"), pauseAfter: 300);
            await scene.PauseAsync(1200);
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "has been published");
        });

        await scene.StepAsync("change-2", async () =>
        {
            await VisitAsync(scene, "/");
            await page.Locator("article, .blog-post, h1").First.WaitForAsync();
            await scene.HoldAsync(3);
        });

        await scene.StepAsync("change-3", async () =>
        {
            await VisitAsync(scene, "/Admin/Contents/ContentItems");
            await GoAsync(scene, Row(page, ArticleTitle).Locator("a.edit").First);
            var row = page.Locator(".form-check").Filter(new() { Has = page.Locator("input[name$='SetHomepage']") }).First;
            await row.ScrollToMiddleAsync();
            await scene.HighlightAsync(row);
        });
    }

    private static async Task PermissionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await OpenAdminGroupAsync(page, "Access Control");
        await page.Locator("a[href*='/Admin/Roles']").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("permission-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[href*='/Admin/Roles']").First);
            await Row(page, "Administrator").WaitForAsync();
            await scene.PauseAsync(3000);
            await scene.HighlightAsync(Row(page, "Administrator"));
        });

        await scene.StepAsync("permission-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, Row(page, "Editor").Locator("a.edit").First);
            await scene.TypeAsync(page.Locator("#search-box"), "homepage", delay: 45);
            var row = page.Locator("tr[data-text='Set homepage.']").First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
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

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Build a Blog", recorded in order on one fresh Blog site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-tour", TourAsync),
        ("03-category", CategoryAsync),
        ("04-post", PostAsync),
        ("05-tags", TagsAsync),
        ("06-menu", MenuAsync),
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

    /// <summary>Warms up the pages the clips open, so their first load is not slow. Nothing is changed.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/", "/Admin", "/Admin/Contents/ContentItems/Taxonomy", "/Admin/Layers", "/Admin/Media", "/Admin/Contents/ContentItems/Menu" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Site warmed up.");
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

    private static ILocator Group(IPage page, string key) => page.Locator($"button.nav-group-label.oc-navigation-admin-{key}");

    private static async Task TourAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("tour-1", async () =>
        {
            await scene.PauseAsync(3000);
            await scene.HighlightAsync(page.Locator("header.masthead"));
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".post-preview").First);
        });

        await scene.StepAsync("tour-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#mainNav .navbar-nav"));
            await scene.MoveToAsync(page.Locator("#mainNav .navbar-nav a").First);
            await scene.PauseAsync(800);
            await scene.MoveToAsync(page.Locator("#mainNav .navbar-nav a").Nth(1));
        });

        await scene.StepAsync("tour-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator(".post-preview a").First);
            await scene.PauseAsync(2500);
            await page.Locator(".term-badge").First.ScrollToMiddleAsync();
            await scene.ScrollAsync(0, 250);
        });

        await scene.StepAsync("tour-4", async () =>
        {
            await VisitAsync(scene, "/Admin");
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-blog").First);
            await page.GetByText("Create Blog Post").First.WaitForAsync();
        });

        await scene.StepAsync("tour-5", async () =>
        {
            await GoAsync(scene, page.Locator("a:has-text('Edit Blog')").First);
            var note = page.GetByText("current homepage").First;
            await note.ScrollToMiddleAsync();
            await scene.HighlightAsync(note);
        });

        await scene.StepAsync("tour-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, -3000);
            await scene.ClickAsync(Group(page, "design"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Layers']").First);
            await scene.HighlightAsync(page.Locator(".card:has-text('Footer'), .zone:has-text('Footer')").Last);
        });
    }

    private static async Task CategoryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("category-1", async () =>
        {
            await scene.ClickAsync(Group(page, "content"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-content-taxonomy").First);
            var row = page.Locator("li, tr").Filter(new() { HasTextString = "Categories" }).Filter(new() { Has = page.Locator("a.edit") }).Last;
            await GoAsync(scene, row.Locator("a.edit").First);
        });

        await scene.StepAsync("category-2", async () =>
        {
            await page.Locator("#menu").ScrollToMiddleAsync();
            await scene.PauseAsync(2000);
            await scene.HighlightAsync(page.Locator("#menu"));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("a:has-text('Add Category')").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#menu :is(a,button):has-text('Add')").First);
        });

        await scene.StepAsync("category-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a:has-text('Add Category')").First);
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }), "Photography", delay: 25);
            await scene.TypeAsync(page.GetByLabel("Permalink"), "photography", delay: 25);
        });

        await scene.StepAsync("category-4", async () =>
        {
            await scene.ClickAsync(page.Locator(".icon-picker-toggle").First, pauseAfter: 300);
            await scene.TypeAsync(page.Locator("input.iconpicker-search"), "camera", delay: 20);
            await scene.PauseAsync(500);
            await scene.ClickAsync(page.Locator(".iconpicker-item:has(i.fa-camera) >> visible=true").First, pauseAfter: 600);
            await scene.PauseAsync(1200);
            await GoAsync(scene, page.Locator("button[value='submit.Publish']").First);
            await scene.MoveToAsync(900, 250);
        });

        await scene.StepAsync("category-5", async () =>
        {
            await GoAsync(scene, page.Locator("button[value='submit.Publish']").First);
            await WaitTextAsync(page, "Your Taxonomy has been published.");
            await scene.MoveToAsync(900, 250);
        });
    }

    private static async Task PostAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("a.oc-navigation-admin-blog").First.ClickAsync();
        await page.GetByText("Create Blog Post").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("post-1", async () =>
        {
            await GoAsync(scene, page.Locator("a:has-text('Create Blog Post')").First);
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }), "Chasing Golden Hour in Lisbon", delay: 25);
        });

        await scene.StepAsync("post-2", async () =>
        {
            await scene.ClickAsync(page.Locator(".markdown-body-part-wysiwyg-editor .CodeMirror").First, pauseAfter: 200);
            await page.Keyboard.TypeAsync("The light over the river turns gold for twenty minutes every evening.", new() { Delay = 25 });
        });

        await scene.StepAsync("post-3", async () =>
        {
            var subtitle = page.GetByLabel("Subtitle");
            await subtitle.ScrollToMiddleAsync();
            await scene.TypeAsync(subtitle, "Light worth waiting for", delay: 25);
            await scene.ClickAsync(page.GetByText("Add media").First, pauseAfter: 600);
            await page.GetByText("post-bg.jpg").First.WaitForAsync();
            await scene.ClickAsync(page.Locator(".file-item").Filter(new() { HasTextString = "post-bg.jpg" }).Locator("input.ma-check-input"), pauseAfter: 300);
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "OK" }), pauseAfter: 500);
        });

        await scene.StepAsync("post-4", async () =>
        {
            var tags = page.Locator(".multiselect").First;
            await tags.ScrollToMiddleAsync();
            await scene.PauseAsync(500);
            await scene.ClickAsync(tags, pauseAfter: 200);
            await page.Locator(".multiselect--active").First.WaitForAsync();
            await page.Keyboard.TypeAsync("Explo", new() { Delay = 40 });
            await page.Locator(".multiselect__option:has-text('Exploration')").First.WaitForAsync();
            await scene.ClickAsync(page.Locator(".multiselect__option:has-text('Exploration')").First, pauseAfter: 300);
        });

        await scene.StepAsync("post-5", async () =>
        {
            await scene.ClickAsync(page.Locator(".multiselect").First, pauseAfter: 200);
            await page.Locator(".multiselect--active").First.WaitForAsync();
            await page.Keyboard.TypeAsync("Lisbon", new() { Delay = 40 });
            await scene.PauseAsync(1800);
            await page.Keyboard.PressAsync("Enter");
            await page.Locator(".multiselect__tag:has-text('Lisbon')").WaitForAsync();
        });

        await scene.StepAsync("post-6", async () =>
        {
            var photography = page.Locator("label.form-check-label:has-text('Photography')").First;
            await photography.ScrollToMiddleAsync();
            await scene.ClickAsync(photography, pauseAfter: 300);
        });

        await scene.StepAsync("post-7", async () =>
        {
            await GoAsync(scene, page.Locator("button[value='submit.Publish']").First);
            await WaitTextAsync(page, "Your Blog Post has been published.");
        });

        await scene.StepAsync("post-8", async () =>
        {
            await VisitAsync(scene, "/");
            await page.Locator(".post-preview:has-text('Chasing Golden Hour in Lisbon')").First.WaitForAsync();
            await GoAsync(scene, page.Locator(".post-preview a:has-text('Chasing Golden Hour in Lisbon')").First);
            await scene.ScrollAsync(0, 300);
        });
    }

    private static async Task TagsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/blog/chasing-golden-hour-in-lisbon");
        await page.Locator(".term-badge").First.ScrollToMiddleAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("tags-1", async () =>
        {
            await GoAsync(scene, page.Locator(".term-badge a:has-text('Lisbon')").First);
        });

        await scene.StepAsync("tags-2", async () =>
        {
            await VisitAsync(scene, "/Admin");
            await scene.ClickAsync(Group(page, "content"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-content-taxonomy").First);
            var row = page.Locator("li, tr").Filter(new() { HasTextString = "Tags" }).Filter(new() { Has = page.Locator("a.edit") }).Last;
            await GoAsync(scene, row.Locator("a.edit").First);
            await page.Locator("#menu").ScrollToMiddleAsync();
        });

        await scene.StepAsync("tags-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("a:has-text('Add Tag')").First);
        });
    }

    private static async Task MenuAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("menu-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-main_menu").First);
            await scene.HighlightAsync(page.GetByLabel("Alias"));
        });

        await scene.StepAsync("menu-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.GetByText("Add Menu Item").First.ScrollToMiddleAsync();
            await scene.PauseAsync(3000);
            await scene.ClickAsync(page.GetByText("Add Menu Item").First);
            await page.GetByText("Available Menu Items").First.WaitForAsync();
        });

        await scene.StepAsync("menu-3", async () =>
        {
            foreach (var name in new[] { "Link Menu Item", "Content Menu Item", "Html Menu Item" })
            {
                await scene.MoveToAsync(page.Locator(".modal.show .card").Filter(new() { HasTextString = name }).First);
                await scene.PauseAsync(1800);
            }
        });

        await scene.StepAsync("menu-4", async () =>
        {
            var card = page.Locator(".modal.show .card").Filter(new() { HasTextString = "Link Menu Item" }).First;
            await GoAsync(scene, card.Locator("a.thumbnail-link-create").First);
            await scene.TypeAsync(page.GetByLabel("Name"), "Photography", delay: 25);
            await scene.TypeAsync(page.GetByLabel("Url"), "~/categories/photography", delay: 25);
        });

        await scene.StepAsync("menu-5", async () =>
        {
            await GoAsync(scene, page.Locator("button[value='submit.Publish']").First);
            await page.Locator("#menu li").Nth(2).WaitForAsync();
        });

        await scene.StepAsync("menu-6", async () =>
        {
            await GoAsync(scene, page.Locator("button[value='submit.Publish']").Last);
            await WaitTextAsync(page, "Your Menu has been published.");
            await scene.HighlightAsync(page.Locator("#menu li:has-text('Photography') button:has-text('Add')").First);
        });

        await scene.StepAsync("menu-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await VisitAsync(scene, "/");
            await GoAsync(scene, page.Locator("#mainNav a:has-text('Photography')").First);
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

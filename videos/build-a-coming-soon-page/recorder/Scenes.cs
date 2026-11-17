using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Coming soon", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-recipe", RecipeAsync),
        ("02b-setup", SetupAsync),
        ("03-visit", VisitAsync2),
        ("05-recaptcha", RecaptchaAsync),
        ("06-theme", ThemeAsync),
        ("04-edit", EditAsync),
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

    /// <summary>Nothing to seed.</summary>
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    /// <summary>Finishes the setup off camera (the plan's table of answers). No frames.</summary>
    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);
        await page.GotoAsync(BaseUrl + "/");
        await page.Locator("#SiteName").FillAsync("Example Garden Supply");
        await page.Locator("#recipeButton").ClickAsync();
        await page.Locator("a.dropdown-item[data-recipe-name='ComingSoon']").ClickAsync();
        await page.Locator("#SiteTimeZone").SelectOptionAsync(new SelectOptionValue { Value = "America/Los_Angeles" });
        await page.Locator("#UserName").FillAsync("admin");
        await page.Locator("#Email").FillAsync("admin@example.com");
        await page.Locator("#Password").FillAsync(Password);
        await page.Locator("#PasswordConfirmation").FillAsync(Password);
        await page.Locator("#SubmitButton").ClickAsync();
        await page.WaitForFunctionAsync("() => !document.location.pathname.toLowerCase().includes('setup') && !!document.querySelector('h1')", null, new() { Timeout = 180_000 });
        await Task.Delay(5000);
        await context.CloseAsync();
        Console.WriteLine("Setup finished.");
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

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<IPage, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(90_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene.Page);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 700);
        return scene;
    }

    private static async Task RecipeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/", async p =>
        {
            await p.Locator("#SiteName").WaitForAsync();
            await p.Locator("#SiteTimeZone").SelectOptionAsync(new SelectOptionValue { Value = "America/Los_Angeles" });
        });
        var page = scene.Page;

        await scene.StepAsync("recipe-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#SiteName"), "Example Garden Supply", delay: 40);
            await scene.ClickAsync(page.Locator("#recipeButton"), pauseAfter: 300);
            var item = page.Locator("a.dropdown-item[data-recipe-name='ComingSoon']");
            await item.WaitForAsync();
            await scene.PauseAsync(700);
            await scene.ClickAsync(item, pauseAfter: 300);
        });

        await scene.StepAsync("recipe-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("#recipeButton"), "Coming Soon");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 600);
            await scene.HighlightAsync(page.Locator("#SubmitButton"), "Finish Setup");
        });
    }

    private static async Task VisitAsync2(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/", p => p.Locator("input[type='email']").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("visit-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("h1").First, "Coming Soon");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".masthead-content p").First);
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("input[type='email']").First);
        });

        await scene.StepAsync("visit-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#submit-button").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".social-icons").First);
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".alert-warning").First);
        });
    }

    private static async Task ThemeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Themes", p => p.GetByText("The Coming Soon Theme").First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("launch-2", async () =>
        {
            var card = page.Locator(".card, .list-group-item, li").Filter(new() { HasTextString = "The Coming Soon Theme" }).Filter(new() { HasTextString = "current Site theme" }).Last;
            await scene.HighlightAsync(card);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            var defaultCard = page.Locator(".card, .list-group-item, li").Filter(new() { HasTextString = "The Default Theme" }).Last;
            var make = defaultCard.GetByRole(AriaRole.Link, new() { Name = "Make Current" }).Or(defaultCard.GetByRole(AriaRole.Button, new() { Name = "Make Current" })).First;
            await make.ScrollToMiddleAsync();
            await scene.MoveToAsync(make);
            await scene.HighlightAsync(make, "Make Current");
        });
    }

    private static async Task RecaptchaAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("signup-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/recaptcha");
            await page.GetByLabel("Site Key").WaitForAsync();
            await ShowAsync(scene, page.GetByLabel("Site Key"), 3000);
            await scene.HighlightAsync(page.GetByLabel("Secret Key"), "Secret Key");
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentItems", p => p.Locator("a.btn.edit").First.WaitForAsync());
        var page = scene.Page;
        var message = "We are building an online shop for garden tools. Leave your email address and we will tell you when it opens.";

        await scene.StepAsync("edit-1", async () =>
        {
            await ShowAsync(scene, page.Locator("a[href$='/Admin/Contents/ContentItems']").First, 2000);
            await GoAsync(scene, page.Locator("a.btn.edit").First);
            await page.GetByLabel("Title", new() { Exact = true }).First.WaitForAsync();
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await ShowAsync(scene, page.GetByLabel("Title", new() { Exact = true }).First, 2000);
            var note = page.Locator(".alert-info").Filter(new() { HasTextString = "current homepage" }).First;
            await note.ScrollToMiddleAsync();
            await ShowAsync(scene, note, 3000);
            var action = page.GetByLabel("Action", new() { Exact = true }).First;
            await action.ScrollToMiddleAsync();
            await scene.HighlightAsync(action);
        });

        await scene.StepAsync("edit-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var card = page.Locator(".widget-editor, .card").Filter(new() { HasTextString = "InputFieldName" }).Last;
            await card.ScrollToMiddleAsync();
            var editor = card.Locator(".trumbowyg-editor").First;
            await editor.ScrollToMiddleAsync();
            await scene.TripleClickAsyncSafe(editor);
            await page.Keyboard.TypeAsync(message, new() { Delay = 20 });
            var button = card.GetByLabel("Button", new() { Exact = true }).First;
            await scene.TripleClickAsyncSafe(button);
            await page.Keyboard.TypeAsync("Keep Me Posted", new() { Delay = 25 });
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(card.GetByLabel("InputFieldName").First);
        });

        await scene.StepAsync("edit-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var icon = page.Locator("label:text-is('Icon Class')").First.Locator("xpath=following::input[1]");
            await icon.ScrollToMiddleAsync();
            await ShowAsync(scene, icon, 2000);
            var url = page.Locator("input[name='SocialIcons-0.SocialIcon.Url.Text']");
            await scene.TripleClickAsyncSafe(url);
            await page.Keyboard.TypeAsync("https://example.com/garden-supply", new() { Delay = 25 });
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("edit-5", async () =>
        {
            var publish = page.Locator("button.publish").First;
            await GoAsync(scene, publish);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.GotoAsync(BaseUrl + "/")), leadMilliseconds: 250);
            await page.WaitForFunctionAsync("() => document.querySelector('.masthead-content')?.innerText.includes('garden tools')");
        });
    }
}

internal static class ClickExtensions
{
    public static async Task TripleClickAsyncSafe(this Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        await locator.ClickAsync(new() { ClickCount = 3 });
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

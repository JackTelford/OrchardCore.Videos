using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Build a Company Website With the Agency Theme", recorded in order on one fresh Agency site.</summary>
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
        ("03-edit", EditAsync),
        ("04-enable", EnableAsync),
        ("04-form", FormAsync),
        ("05-menu", MenuAsync),
        ("06-finish", FinishAsync),
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

    /// <summary>
    /// Seeds the site, outside the capture: three invented team members, no clients (the recipe's logos are real companies),
    /// and a neutral hint on the Twitter field of a team member (the recipe's hint names a real person).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        // The hint of the Twitter field first, so the editor of the landing page shows it.
        await page.GotoAsync(BaseUrl + "/Admin/ContentParts/TeamMember/Fields/Twitter/Edit");
        await page.GetByLabel("Hint").FillAsync("Twitter handle");
        await page.Locator("button.save, button[type='submit']").Filter(new() { HasTextString = "Save" }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems");
        var row = page.Locator("li, tr").Filter(new() { HasTextString = "LandingPage" }).Filter(new() { Has = page.Locator("a.edit") }).Last;
        await row.Locator("a.edit").First.ClickAsync();
        await page.Locator(".content-part-wrapper-team").First.WaitForAsync();

        var names = new[] { "Jamie Chen", "Morgan Ellis", "Riley Okafor" };
        var cards = page.Locator(".content-part-wrapper-team .widget-editor");

        for (var i = 0; i < names.Length; i++)
        {
            await cards.Nth(i).GetByLabel("Title", new() { Exact = true }).First.FillAsync(names[i]);
        }

        var clients = page.Locator(".content-part-wrapper-clients .widget-editor");

        while (await clients.CountAsync() > 0)
        {
            await clients.First.Locator("button.widget-delete").ClickAsync();
            await page.Locator(".modal.show button.btn-danger, .modal.show button:has-text('Ok'), .modal.show button:has-text('OK'), .modal.show button:has-text('Yes')").First.ClickAsync();
            await Task.Delay(500);
        }

        await page.Locator("button[name='submit.Publish']").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Warms up the pages the clips open.
        foreach (var path in new[] { "/", "/Admin/Features", "/Admin/Contents/ContentItems", "/Admin/Contents/ContentItems/Menu" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Site prepared.");
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

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static async Task TourAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("tour-1", async () =>
        {
            await ShowAsync(scene, page.Locator("header.masthead .masthead-heading"), 1800);
            await ShowAsync(scene, page.GetByText("Tell Me More").First, 1800);
            await ShowAsync(scene, page.Locator("#mainNav"), 1800);
        });

        await scene.StepAsync("tour-2", async () =>
        {
            foreach (var name in new[] { "Services", "Portfolio", "About" })
            {
                await scene.ClickAsync(page.Locator("#mainNav a.nav-link").Filter(new() { HasTextString = name }).First, pauseAfter: 1500);
            }
        });

        await scene.StepAsync("tour-3", async () =>
        {
            foreach (var name in new[] { "Services", "Portfolio", "About", "Team", "Contact" })
            {
                await ShowAsync(scene, page.Locator("#mainNav a.nav-link").Filter(new() { HasTextString = name }).First, 700);
            }

            await scene.ClickAsync(page.Locator("#mainNav a.nav-link").Filter(new() { HasTextString = "Team" }).First, pauseAfter: 1500);
            await scene.ClickAsync(page.Locator("#mainNav a.nav-link").Filter(new() { HasTextString = "Contact" }).First, pauseAfter: 800);
        });

        await scene.StepAsync("tour-4", async () =>
        {
            await ShowAsync(scene, page.Locator("#contact .section-heading"), 1800);
            await ShowAsync(scene, page.Locator("#contactForm"), 2500);
            await scene.PauseAsync(3000);
            await scene.ClickAsync(page.Locator("a.navbar-brand").First, pauseAfter: 500);
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("edit-1", async () =>
        {
            await scene.ClickAsync(Group(page, "content"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-content-content_items"));
            await scene.HighlightAsync(page.Locator("li, tr").Filter(new() { HasTextString = "LandingPage" }).Last);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            var row = page.Locator("li, tr").Filter(new() { HasTextString = "LandingPage" }).Filter(new() { Has = page.Locator("a.edit") }).Last;
            await GoAsync(scene, row.Locator("a.edit").First);
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.PauseAsync(1000);

            foreach (var name in new[] { "Services", "Portfolio", "About", "Team", "Clients" })
            {
                var label = page.Locator(".content-part-wrapper .ocat-label").Filter(new() { HasTextString = name }).First;
                await label.ScrollToMiddleAsync();
                await ShowAsync(scene, label, 700);
            }

            await page.Locator(".content-part-wrapper-services").First.ScrollToMiddleAsync();
            await scene.ClickAsync(page.Locator(".content-part-wrapper-services button.widget-editor-btn-collapse").First, pauseAfter: 800);
            await scene.ClickAsync(page.Locator(".content-part-wrapper-services button.widget-editor-btn-expand").First, pauseAfter: 400);
        });

        await scene.StepAsync("edit-3", async () =>
        {
            var team = page.Locator(".content-part-wrapper-team .widget-editor").First;
            await team.ScrollToMiddleAsync();

            foreach (var label in new[] { "Title", "Occupation", "Picture", "Twitter", "Facebook", "LinkedIn" })
            {
                var field = team.GetByText(label, new() { Exact = true }).First;

                if (await field.IsVisibleAsync())
                {
                    await ShowAsync(scene, field, 500);
                }
            }

            var occupation = team.GetByLabel("Occupation");
            await scene.ClickAsync(occupation, pauseAfter: 200);
            await occupation.FillAsync(string.Empty);
            await scene.TypeAsync(occupation, "Design Director", delay: 20);
        });

        await scene.StepAsync("edit-4", async () =>
        {
            var services = page.Locator(".content-part-wrapper-services");
            await services.First.ScrollToMiddleAsync();
            var collapse = services.Locator("button.widget-editor-btn-collapse");
            var count = await collapse.CountAsync();

            for (var i = 0; i < count; i++)
            {
                await scene.ClickAsync(collapse.Nth(i), pauseAfter: 600);
            }

            var add = services.Locator("a.add-widget").First;
            await scene.HighlightAsync(add);
            await scene.PauseAsync(700);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(add, pauseAfter: 300);
            await page.WaitForFunctionAsync("() => document.querySelectorAll('.content-part-wrapper-services .widget-editor').length >= 4");
            var card = services.Locator(".widget-editor").Last;
            await scene.TypeAsync(card.GetByLabel("Title", new() { Exact = true }).First, "Brand Identity", delay: 25);
        });

        await scene.StepAsync("edit-5", async () =>
        {
            var card = page.Locator(".content-part-wrapper-services .widget-editor").Last;
            var editor = card.Locator(".CodeMirror").First;
            await editor.ScrollToMiddleAsync();
            await scene.ClickAsync(editor, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Logos, color palettes and style guides for growing companies.", new() { Delay = 6 });
            var icon = card.GetByLabel("Icon Class");
            await scene.ClickAsync(icon, pauseAfter: 200);
            await scene.TypeAsync(icon, "fa-palette", delay: 25);
            await scene.HighlightAsync(icon);
        });

        await scene.StepAsync("edit-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var services = page.Locator(".content-part-wrapper-services");
            var cards = services.Locator(".widget-editor");
            await services.First.ScrollToMiddleAsync();
            var handle = cards.Last.Locator(".widget-editor-handle").First;
            await ShowAsync(scene, handle, 1000);
            await ShowAsync(scene, cards.Last.Locator("button.widget-delete").First, 1000);
            await scene.DragToAsync(handle, cards.First.Locator(".widget-editor-handle").First, offsetY: -8);
        });

        await scene.StepAsync("edit-7", async () =>
        {
            await scene.HighlightAsync(page.Locator("button[name='submit.Publish']").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").First);
            await WaitTextAsync(page, "Your Landing Page has been published.");
            await scene.PauseAsync(2000);
            await VisitAsync(scene, "/#services");
            await scene.PauseAsync(1500);
        });
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(Group(page, "tools"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Features'] >> visible=true").First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "Forms", delay: 40);
            await page.Locator("#btn-enable-OrchardCore_Forms").WaitForAsync();
            await scene.HighlightAsync(page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_Forms") }).First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_Forms"));
            await WaitTextAsync(page, "The feature Forms was enabled.");
        });
    }

    private static async Task FormAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("form-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("#new-dropdown, button:has-text('New')").First);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#new-dropdown, button:has-text('New')").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("a.dropdown-item:text-is('Page')").First);
        });

        await scene.StepAsync("form-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("button[title='Add Widget']").First);
        });

        await scene.StepAsync("form-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.GetByLabel("Title", new() { Exact = true }).First, pauseAfter: 100);
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Start a project", delay: 25);
            await ShowAsync(scene, page.GetByLabel("Permalink"), 1500);
            await scene.ClickAsync(page.Locator("button[title='Add Widget']").First, pauseAfter: 400);
            await scene.ClickAsync(page.Locator("a.add-widget:text-is('Form')").First, pauseAfter: 200);
            await page.Locator(".widget-editor[data-content-type='Form']").WaitForAsync();
        });

        var form = page.Locator(".widget-editor[data-content-type='Form']");

        await scene.StepAsync("form-4", async () =>
        {
            await scene.ClickAsync(form.GetByLabel("Title", new() { Exact = true }).First, pauseAfter: 100);
            await scene.TypeAsync(form.GetByLabel("Title", new() { Exact = true }).First, "Project inquiry", delay: 25);
            await ShowAsync(scene, form.GetByLabel("Action").First, 2000);
        });

        await scene.StepAsync("form-5", async () =>
        {
            await ShowAsync(scene, form.GetByLabel("Method").First, 1800);
            await ShowAsync(scene, form.GetByLabel("Anti Forgery Token").First, 1800);
        });

        await scene.StepAsync("form-6", async () =>
        {
            await AddWidgetAsync(scene, form, "Input");
            var input = form.Locator(".widget-editor[data-content-type='Input']").First;
            await FillInputAsync(scene, input, "name", null, "Standard", "Your name");
        });

        await scene.StepAsync("form-7", async () =>
        {
            await scene.HighlightAsync(form.Locator(".widget-editor[data-content-type='Input']").First.GetByLabel("Label option").First, "None, Standard, Screen Readers");
        });

        await scene.StepAsync("form-8", async () =>
        {
            await scene.ClearHighlightsAsync();
            await AddWidgetAsync(scene, form, "Input");
            var input = form.Locator(".widget-editor[data-content-type='Input']").Nth(1);
            await FillInputAsync(scene, input, "email", "email", "Standard", "Your email", fast: true);
        });

        await scene.StepAsync("form-9", async () =>
        {
            await AddWidgetAsync(scene, form, "Text Area");
            var area = form.Locator(".widget-editor[data-content-type='TextArea']").First;
            await FillInputAsync(scene, area, "message", null, "Standard", "Project details", fast: true);
            await AddWidgetAsync(scene, form, "Button");
            var button = form.Locator(".widget-editor[data-content-type='Button']").First;
            await EnterAsync(scene, button.GetByLabel("HTML Name").First, "send", true);
            await EnterAsync(scene, button.GetByLabel("Text", new() { Exact = true }).First, "Send", true);
            await scene.SelectAsync(button.GetByLabel("Type", new() { Exact = true }).First, "submit", pauseAfter: 150);
        });

        await scene.StepAsync("form-10", async () =>
        {
            var publish = page.Locator("button[name='submit.Publish']").First;
            await publish.ScrollToMiddleAsync();
            await scene.HighlightAsync(publish);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, publish);
            await WaitTextAsync(page, "Your Page has been published.");
        });
    }

    private static async Task AddWidgetAsync(Scene scene, ILocator form, string type)
    {
        var contentType = type.Replace(" ", string.Empty);
        var before = await form.Locator($".widget-editor[data-content-type='{contentType}']").CountAsync();
        var plus = form.Locator("button[title='Add Widget']").Last;
        await plus.ScrollToMiddleAsync();
        await scene.ClickAsync(plus, pauseAfter: 300);
        await scene.MoveToAsync(form.Locator($"a.add-widget:text-is('{type}')").First);
        var (x, y) = await scene.CenterOfAsync(form.Locator($"a.add-widget:text-is('{type}')").First);

        // The card is loaded by a request: the wait is cut out of the video.
        await scene.OffCameraAsync(() => Task.WhenAll(
            form.Page.Mouse.ClickAsync((float)x, (float)y),
            form.Page.WaitForFunctionAsync("([t, n]) => document.querySelectorAll(`.widget-editor[data-content-type='${t}']`).length > n", new object[] { contentType, before })), leadMilliseconds: 150);
    }

    // Types into a field, or (fast) clicks it and sets the text at once: typing is slow when the machine is busy.
    private static async Task EnterAsync(Scene scene, ILocator field, string text, bool fast)
    {
        if (fast)
        {
            await scene.ClickAsync(field, pauseAfter: 100);
            await field.FillAsync(text);
        }
        else
        {
            await scene.TypeAsync(field, text, delay: 10);
        }
    }

    private static async Task FillInputAsync(Scene scene, ILocator card, string name, string type, string label, string text, bool fast = false)
    {
        await card.WaitForAsync();
        await card.ScrollToMiddleAsync();
        await EnterAsync(scene, card.GetByLabel("HTML Name").First, name, fast);

        if (type is not null)
        {
            await scene.SelectAsync(card.GetByLabel("Type", new() { Exact = true }).First, type, pauseAfter: 150);
        }

        await scene.SelectAsync(card.GetByLabel("Label option").First, label, pauseAfter: 150);
        await EnterAsync(scene, card.GetByLabel("Label Text").First, text, fast);
    }

    private static async Task MenuAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("menu-1", async () =>
        {
            await scene.ClickAsync(Group(page, "content"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-content-menus, a[href*='ContentItems/Menu']").First);
            var row = page.Locator("li, tr").Filter(new() { HasTextString = "Main Menu" }).Filter(new() { Has = page.Locator("a.edit") }).Last;
            await GoAsync(scene, row.Locator("a.edit").First);
            await scene.HighlightAsync(page.Locator("#menu"));
        });

        await scene.StepAsync("menu-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.GetByText("Add Menu Item").First, pauseAfter: 500);
            await page.Locator("#modalMenuItems .card").First.WaitForAsync();

            foreach (var name in new[] { "Link Menu Item", "Content Menu Item", "Html Menu Item" })
            {
                await ShowAsync(scene, page.Locator("#modalMenuItems .card").Filter(new() { HasTextString = name }).First, 1500);
            }

            var card = page.Locator("#modalMenuItems .card").Filter(new() { HasTextString = "Content Menu Item" }).First;
            await GoAsync(scene, card.Locator("a.thumbnail-link-create").First);
        });

        await scene.StepAsync("menu-3", async () =>
        {
            await scene.ClickAsync(page.GetByLabel("Name").First, pauseAfter: 100);
            await scene.TypeAsync(page.GetByLabel("Name").First, "Start a project", delay: 25);
            var picker = page.Locator(".multiselect").First;
            await picker.ScrollToMiddleAsync();
            await scene.ClickAsync(picker, pauseAfter: 300);
            await page.Locator(".multiselect--active").First.WaitForAsync();
            await page.Keyboard.TypeAsync("Start", new() { Delay = 40 });
            var option = page.Locator(".multiselect__option:has-text('Start a project')").First;
            await option.WaitForAsync();
            await scene.ClickAsync(option, pauseAfter: 300);
        });

        await scene.StepAsync("menu-4", async () =>
        {
            var check = page.GetByLabel("Check content permissions").First;
            await check.ScrollToMiddleAsync();
            await scene.HighlightAsync(check);
        });

        await scene.StepAsync("menu-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").First);
            await page.Locator("#menu li").Nth(5).WaitForAsync();
            await ShowAsync(scene, page.Locator("#menu li").Last, 1000);
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").First);
            await WaitTextAsync(page, "Your Menu has been published.");
            await scene.HighlightAsync(page.Locator("#menu"));
        });
    }

    private static async Task FinishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("finish-1", async () =>
        {
            var link = page.Locator("#mainNav a.nav-link").Filter(new() { HasTextString = "Start a project" }).First;
            await ShowAsync(scene, link, 1500);
            await GoAsync(scene, link);
        });

        await scene.StepAsync("finish-2", async () =>
        {
            await scene.ScrollAsync(0, 500);

            foreach (var label in new[] { "Your name", "Your email", "Project details", "Send" })
            {
                await ShowAsync(scene, page.GetByText(label, new() { Exact = true }).First, 900);
            }
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

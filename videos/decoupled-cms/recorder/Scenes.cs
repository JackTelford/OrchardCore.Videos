using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Decoupled CMS with Razor Pages", recorded in order on a throwaway Razor Pages host that is not set up yet.
/// "setup" (the Recorder command) signs in after 03-setup, then the other clips run in the order of the list.
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
        ("03-setup", SetupAsync),
        ("04-type", TypeAsync),
        ("05-post", PostAsync),
        ("06-page", PageAsync),
        ("07-alias-part", AliasPartAsync),
        ("07-alias-page", AliasPageAsync),
        ("07-alias-pattern", AliasPatternAsync),
        ("08-preview", PreviewAsync),
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

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000, string caption = null)
    {
        await scene.HighlightAsync(locator, caption);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    // The identifier of the blog post, read from its Edit link in the Content Items list (outside the capture).
    private static async Task<string> PostIdAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems");
        var href = await page.Locator("a.edit").First.GetAttributeAsync("href");
        await context.CloseAsync();
        return Regex.Match(href, "/ContentItems/([^/?]+)/Edit").Groups[1].Value;
    }

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/");

        // The Setup screen opens on the time zone of the server, which is this computer's: start from Europe/London instead, off camera.
        await page.Locator("#SiteTimeZone").SelectOptionAsync(new SelectOptionValue { Value = "Europe/London" });
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#SiteName"), "Orchard Notes", delay: 30);
            await scene.HighlightAsync(page.Locator("#SiteName"), "Default title of your pages");
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#recipeButton"), pauseAfter: 400);
            await scene.ClickAsync(page.Locator("a.dropdown-item[data-recipe-name='Blank']"), pauseAfter: 400);
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.SelectAsync(page.Locator("#SiteTimeZone"), "America/Los_Angeles", pauseAfter: 300);
            await ShowAsync(scene, page.Locator("#DatabaseProvider"), 1500);
            await page.Locator("#UserName").ScrollToMiddleAsync();
            await scene.TypeAsync(page.Locator("#UserName"), "admin", delay: 20);
            await scene.TypeAsync(page.Locator("#Email"), "admin@example.com", delay: 12);
            await scene.TypeAsync(page.Locator("#Password"), Password, delay: 12);
            await scene.TypeAsync(page.Locator("#PasswordConfirmation"), Password, delay: 12);
        });

        await scene.StepAsync("setup-4", async () =>
        {
            page.SetDefaultTimeout(180_000);
            await scene.MoveToAsync(page.Locator("#SubmitButton"));
            var (x, y) = await scene.CenterOfAsync(page.Locator("#SubmitButton"));
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y));
                await page.GetByRole(AriaRole.Heading, new() { Name = "Welcome" }).WaitForAsync();
            });
            page.SetDefaultTimeout(20_000);
        });
    }

    private static async Task TypeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("type-1", async () =>
        {
            await scene.ClickAsync(Group(page, "design"), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("li.oc-navigation-admin-design-content_definition > figure span.title").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("a[href$='/Admin/ContentTypes/List'] >> visible=true").First);
        });

        await scene.StepAsync("type-2", async () =>
        {
            await GoAsync(scene, page.Locator("#btnCreate"));
            await scene.TypeAsync(page.Locator("#DisplayName"), "Blog Post", delay: 30);
            await page.WaitForFunctionAsync("() => document.querySelector('#Name').value === 'BlogPost'");
            await GoAsync(scene, page.Locator("button.create").First);
        });

        await scene.StepAsync("type-3", async () =>
        {
            foreach (var part in new[] { "Title", "MarkdownBody" })
            {
                var row = page.Locator("li.list-group-item").Filter(new() { Has = page.GetByText(part, new() { Exact = true }) }).First;
                await scene.ClickAsync(row.Locator("input[type='checkbox']").First, pauseAfter: 300);
            }

            await GoAsync(scene, page.Locator("#add-selected-parts-button"));
        });

        await scene.StepAsync("type-4", async () =>
        {
            var parts = page.Locator("#parts");
            await parts.ScrollToMiddleAsync();
            await ShowAsync(scene, parts, 2500, "Title, MarkdownBody");
        });
    }

    private static async Task PostAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("post-1", async () =>
        {
            await scene.ClickAsync(Group(page, "content"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-content-content_items"));
            await GoAsync(scene, page.Locator("a.btn:has-text('New Blog Post')").First);
        });

        await scene.StepAsync("post-2", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Winter Pruning Notes", delay: 20);
            await scene.ClickAsync(page.GetByLabel("MarkdownBody").First, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Prune apple trees while they are dormant.", new() { Delay = 12 });
            await GoAsync(scene, page.Locator("button[name='submit.Publish'], button.publish").Filter(new() { Visible = true }).First);
            await WaitTextAsync(page, "Your Blog Post has been published.");
        });
    }

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var id = await PostIdAsync(browser);
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("render-5", async () =>
        {
            await VisitAsync(scene, "/blogpost/" + id);
            await page.Locator("main h1").Filter(new() { HasTextString = "Winter Pruning Notes" }).WaitForAsync();
            await ShowAsync(scene, page.Locator("main h1"), 1000);
            await scene.HighlightAsync(page.Locator("main p:has-text('Prune apple trees while they are dormant.')"));
        });
    }

    private static async Task AliasPartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var id = await PostIdAsync(browser);
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/Edit/BlogPost");
        await scene.StartCaptureAsync();

        await scene.StepAsync("alias-1", async () =>
        {
            await GoAsync(scene, page.GetByText("Add Parts").First);
            var row = page.Locator("li.list-group-item").Filter(new() { Has = page.GetByText("Alias", new() { Exact = true }) }).First;
            await scene.ClickAsync(row.Locator("input[type='checkbox']").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("#add-selected-parts-button"));
            await page.Locator("#parts").ScrollToMiddleAsync();
        });

        await scene.StepAsync("alias-2", async () =>
        {
            await VisitAsync(scene, $"/Admin/Contents/ContentItems/{id}/Edit");
            var alias = page.GetByLabel("Alias").First;
            await alias.ScrollToMiddleAsync();
            await scene.TypeAsync(alias, "pruning", delay: 30);
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").First);
            await WaitTextAsync(page, "Your Blog Post has been published.");
        });
    }

    private static async Task AliasPageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("alias-4", async () =>
        {
            await VisitAsync(scene, "/blogpost/pruning");
            await page.Locator("main h1").Filter(new() { HasTextString = "Winter Pruning Notes" }).WaitForAsync();
            await scene.HighlightAsync(page.Locator("main h1"));
        });
    }

    private static async Task AliasPatternAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var id = await PostIdAsync(browser);
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/Edit/BlogPost");
        await scene.StartCaptureAsync();

        await scene.StepAsync("alias-5", async () =>
        {
            var row = page.Locator("#parts li").Filter(new() { HasTextString = "Alias" }).First;
            await row.ScrollToMiddleAsync();
            await GoAsync(scene, row.Locator("a:has-text('Edit'), button:has-text('Edit')").First);
            await page.Locator(".CodeMirror").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".CodeMirror").First);
        });

        await scene.StepAsync("alias-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await VisitAsync(scene, $"/Admin/Contents/ContentItems/{id}/Edit");
            var alias = page.GetByLabel("Alias").First;
            await alias.ScrollToMiddleAsync();
            await scene.ClickAsync(alias, pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.PressAsync("Backspace");
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").First);
            await WaitTextAsync(page, "Your Blog Post has been published.");
            await page.GetByText("Leave empty to re-generate it.").First.ScrollToMiddleAsync();
            await scene.HighlightAsync(page.GetByText("Leave empty to re-generate it.").First);
        });
    }

    private static async Task PreviewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var id = await PostIdAsync(browser);
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var editor = scene.Page;
        await scene.OpenAsync($"/Admin/Contents/ContentItems/{id}/Edit");
        await scene.StartCaptureAsync();

        IPage preview = null;

        async Task<IPage> OpenPreviewAsync(string text)
        {
            var popup = await scene.Context.RunAndWaitForPageAsync(async () => await scene.ClickAsync(editor.Locator("#previewButton"), pauseAfter: 200));
            await scene.SwitchToAsync(popup);
            await popup.WaitForFunctionAsync(
                "t => { const f = [...document.querySelectorAll('iframe')].find(i => i.style.zIndex === '999990'); return !!f && !!f.contentDocument && f.contentDocument.body.innerText.includes(t); }",
                text, new PageWaitForFunctionOptions { Timeout = 60_000 });
            return popup;
        }

        await scene.StepAsync("preview-1", async () =>
        {
            preview = await OpenPreviewAsync("Prune apple trees while they are dormant.");
        });

        await scene.StepAsync("preview-2", async () =>
        {
            await scene.SwitchToAsync(editor);
            await VisitAsync(scene, "/Admin/ContentTypes/AddPartsTo/BlogPost");
            var row = editor.Locator("li.list-group-item").Filter(new() { Has = editor.GetByText("Preview", new() { Exact = true }) }).First;
            await scene.ClickAsync(row.Locator("input[type='checkbox']").First, pauseAfter: 300);
            await GoAsync(scene, editor.Locator("#add-selected-parts-button"));
            var partRow = editor.Locator("#parts li").Filter(new() { HasTextString = "Preview" }).First;
            await partRow.ScrollToMiddleAsync();
            await GoAsync(scene, partRow.Locator("a:has-text('Edit'), button:has-text('Edit')").First);
            var cm = editor.Locator(".CodeMirror").First;
            await scene.ClickAsync(cm, pauseAfter: 200);
            await editor.Keyboard.PressAsync("Control+A");
            await editor.Keyboard.TypeAsync("/blogpost/{{ ContentItem.Content.AliasPart.Alias }}", new() { Delay = 10 });
            var value = await editor.EvaluateAsync<string>("() => document.querySelector('.CodeMirror').CodeMirror.getValue()");

            if (value != "/blogpost/{{ ContentItem.Content.AliasPart.Alias }}")
            {
                await editor.EvaluateAsync("() => document.querySelector('.CodeMirror').CodeMirror.setValue('/blogpost/{{ ContentItem.Content.AliasPart.Alias }}')");
            }

            await scene.PauseAsync(1500);
            await GoAsync(scene, editor.Locator("button.save, button:has-text('Save')").First);
        });

        await scene.StepAsync("preview-3", async () =>
        {
            await preview.CloseAsync();
            await scene.SwitchToAsync(editor);
            await VisitAsync(scene, $"/Admin/Contents/ContentItems/{id}/Edit");
            IPage second = null;

            await scene.OffCameraAsync(async () =>
            {
                second = await scene.Context.RunAndWaitForPageAsync(async () => await editor.Locator("#previewButton").ClickAsync());
                await second.WaitForFunctionAsync(
                    "() => { const f = [...document.querySelectorAll('iframe')].find(i => i.style.zIndex === '999990'); return !!f && !!f.contentDocument && !!f.contentDocument.querySelector('h1') && f.contentDocument.querySelector('h1').innerText.includes('Winter Pruning Notes'); }",
                    null, new PageWaitForFunctionOptions { Timeout = 60_000 });
            });

            await scene.SwitchToAsync(second);
            await scene.SwitchToAsync(editor);
            var title = editor.Locator("input.content-preview-text").First;
            await scene.ClickAsync(title, pauseAfter: 100);
            await editor.Keyboard.PressAsync("End");
            await editor.Keyboard.TypeAsync(" Revised", new() { Delay = 30 });
            await scene.SwitchToAsync(second);

            await scene.OffCameraAsync(() => second.WaitForFunctionAsync(
                "() => { const f = [...document.querySelectorAll('iframe')].find(i => i.style.zIndex === '999990'); return !!f && !!f.contentDocument && !!f.contentDocument.querySelector('h1') && f.contentDocument.querySelector('h1').innerText.includes('Winter Pruning Notes Revised'); }",
                null, new PageWaitForFunctionOptions { Timeout = 60_000 }));
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

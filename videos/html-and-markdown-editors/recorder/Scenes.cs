using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Html and Markdown: Write the Body of Your Content". PrepareAsync imports two content types
/// (Announcement without a body, Help Topic with MarkdownBody) with the JSON import, outside the capture.
/// 02-add attaches HtmlBody to Announcement and 03-html uses it; 05-markdown is independent.
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
        ("02-add", AddAsync),
        ("03-html", HtmlAsync),
        ("04-standard", StandardAsync),
        ("05-markdown", MarkdownAsync),
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

    // Imports the seed content types (the JSON is in VIDEO_SEED_JSON, outside the repository).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var json = File.ReadAllText(Environment.GetEnvironmentVariable("VIDEO_SEED_JSON") ?? throw new InvalidOperationException("Set VIDEO_SEED_JSON."));
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        await page.GotoAsync($"{BaseUrl}/Admin/DeploymentPlan/Import/Json", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator(".CodeMirror").First.WaitForAsync();
        await page.EvaluateAsync("text => document.querySelector('.CodeMirror').CodeMirror.setValue(text)", json);
        await page.Locator("button.import").ClickAsync();
        await page.Locator(".toast.message-success").First.WaitForAsync();
        await Task.Delay(3000);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(ILocator field, string text, int delay = 8)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
    }

    private static async Task OpenGroupAsync(Scene scene, string group)
    {
        var button = scene.Page.Locator("#left-nav button.item-label").Filter(new() { HasTextRegex = new($"^\\s*{group}\\s*$") }).First;

        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(button, pauseAfter: 150);
        }
    }

    private static ILocator Row(Scene scene, string title)
        => scene.Page.Locator("li.list-group-item").Filter(new() { HasText = title });

    private static async Task<IPage> ViewAsync(Scene scene, string title)
    {
        var popup = await scene.Page.RunAndWaitForPopupAsync(async () =>
            await scene.ClickAsync(Row(scene, title).Locator("a.view").First, pauseAfter: 200));
        await popup.WaitForLoadStateAsync();
        await scene.SwitchToAsync(popup);
        return popup;
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new("^\\s*Features\\s*$") }).First.ClickAsync());
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "html", 25);
            await scene.PauseAsync(1200);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await scene.Page.Keyboard.TypeAsync("markdown", new() { Delay = 25 });
            await scene.PauseAsync(1200);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await OpenGroupAsync(scene, "Design");
            await OpenGroupAsync(scene, "Content Definition");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new("^\\s*Content Types\\s*$") }).First.ClickAsync());
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("a[role='btn-edit-Announcement']").ClickAsync());
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("a.btn:text-is('Add Parts')").First.ClickAsync());

            foreach (var part in new[] { "HtmlBody", "MarkdownBody", "HtmlBody" })
            {
                var row = scene.Page.Locator("li.list-group-item").Filter(new() { HasText = part }).First;
                await scene.HighlightAsync(row);
                await scene.PauseAsync(800);
                await scene.ClearHighlightsAsync();
            }

            await scene.WaitForNavigationAsync(() => scene.Page.Locator("button[name='submit.AddAndConfigure'][value='HtmlBodyPart']").ClickAsync());
        });

        await scene.StepAsync("add-4", async () =>
        {
            var select = scene.Page.Locator("select[data-type-part-editor-select]").First;
            await scene.HighlightAsync(select);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(select, "Wysiwyg");
        });

        await scene.StepAsync("add-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".form-check", new() { HasText = "Sanitize Html" }).First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
        });

        await scene.StepAsync("add-6", async () =>
        {
            await scene.Page.Locator("ul#parts").ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.Locator("ul#parts li").Filter(new() { HasText = "HtmlBody" }).Locator(".badge").First);
        });
    }

    private static async Task HtmlAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("html-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 400);
            await scene.WaitForNavigationAsync(() => scene.Page.Locator(".dropdown-menu a.dropdown-item", new() { HasTextString = "Announcement" }).First.ClickAsync());
        });

        await scene.StepAsync("html-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("HtmlBody", new() { Exact = true }).First);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await FastAsync(scene.Page.Locator("input.content-caption-text").First, "Garden Tour This Saturday");
            var editor = scene.Page.Locator(".html-body-part-wysiwyg-editor .trumbowyg-editor").First;
            await editor.FocusAsync();
            await editor.PressSequentiallyAsync("Join us for a guided walk through the community garden.", new() { Delay = 8 });
        });

        await scene.StepAsync("html-3", async () =>
        {
            var editor = scene.Page.Locator(".html-body-part-wysiwyg-editor .trumbowyg-editor").First;
            await editor.FocusAsync();
            await scene.Page.Keyboard.PressAsync("End");
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.ClickAsync(scene.Page.Locator(".trumbowyg-bold-button").First, pauseAfter: 150);
            await editor.PressSequentiallyAsync("Doors open at ten o'clock.", new() { Delay = 6 });
            await scene.ClickAsync(scene.Page.Locator(".trumbowyg-bold-button").First, pauseAfter: 150);
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.ClickAsync(scene.Page.Locator(".trumbowyg-unorderedList-button").First, pauseAfter: 150);
            await editor.PressSequentiallyAsync("Bring a sun hat", new() { Delay = 6 });
            await scene.Page.Keyboard.PressAsync("Enter");
            await editor.PressSequentiallyAsync("Wear closed shoes", new() { Delay = 6 });
        });

        await scene.StepAsync("html-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish").First);
            await ViewAsync(scene, "Garden Tour This Saturday");
        });
    }

    private static async Task StandardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/Blog");
        await scene.StartCaptureAsync();

        await scene.StepAsync("std-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.edit").First);
            await scene.Page.Locator(".CodeMirror").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".CodeMirror").First);
        });

        await scene.StepAsync("std-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".CodeMirror").First, pauseAfter: 300);
            await scene.PauseAsync(2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.cancel").First);
        });
    }

    private static async Task MarkdownAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var toolbar = scene.Page.Locator(".markdown-body-part-wysiwyg-editor .editor-toolbar");

        await scene.StepAsync("md-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 400);
            await scene.WaitForNavigationAsync(() => scene.Page.Locator(".dropdown-menu a.dropdown-item", new() { HasTextString = "Help Topic" }).First.ClickAsync());
            await FastAsync(scene.Page.Locator("input.content-caption-text").First, "Borrow a Library Book");
        });

        await scene.StepAsync("md-2", async () =>
        {
            var area = scene.Page.Locator(".markdown-body-part-wysiwyg-editor .CodeMirror").First;
            await scene.ClickAsync(area, pauseAfter: 200);
            await scene.Page.Keyboard.TypeAsync("## How borrowing works", new() { Delay = 8 });
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.Page.Keyboard.TypeAsync("Show your library card at the front desk to borrow a book.", new() { Delay = 8 });
        });

        await scene.StepAsync("md-3", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Home");

            for (var i = 0; i < 3; i++)
            {
                await scene.Page.Keyboard.PressAsync("Control+ArrowRight");
            }

            await scene.Page.Keyboard.PressAsync("Control+Shift+ArrowLeft");
            await scene.HighlightAsync(scene.Page.Locator(".editor-toolbar button[aria-label='Bold']").First);
            await scene.PauseAsync(500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".editor-toolbar button[aria-label='Bold']").First, pauseAfter: 100);
            await scene.Page.Keyboard.PressAsync("End");
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.HighlightAsync(scene.Page.Locator(".editor-toolbar button[aria-label='Generic List']").First);
            await scene.PauseAsync(500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".editor-toolbar button[aria-label='Generic List']").First, pauseAfter: 100);
            await scene.Page.Keyboard.TypeAsync("Loans last three weeks", new() { Delay = 8 });
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.Page.Keyboard.TypeAsync("You can renew a book twice", new() { Delay = 8 });
        });

        await scene.StepAsync("md-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".editor-toolbar button[aria-label='Toggle Preview']").First, pauseAfter: 2000);
            await scene.ClickAsync(scene.Page.Locator(".editor-toolbar button[aria-label='Toggle Preview']").First, pauseAfter: 300);
            await scene.ClickAsync(scene.Page.Locator(".editor-toolbar button[aria-label='Toggle Side by Side']").First, pauseAfter: 2000);
            await scene.ClickAsync(scene.Page.Locator(".editor-toolbar button[aria-label='Toggle Side by Side']").First, pauseAfter: 300);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.Page.Locator("button.publish").First.WaitForAsync();
        });

        await scene.StepAsync("md-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish").First);
            await ViewAsync(scene, "Borrow a Library Book");
        });
    }
}

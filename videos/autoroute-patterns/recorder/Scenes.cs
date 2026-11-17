using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Autoroute patterns", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-edit", EditAsync),
        ("04-result", ResultAsync),
        ("05-container", ContainerAsync),
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

    private const string SeedJson = """
        {"steps":[{"name":"ContentDefinition","ContentTypes":[{"Name":"Product","DisplayName":"Product","Settings":{"ContentTypeSettings":{"Creatable":true,"Draftable":true,"Versionable":true,"Listable":true}},"ContentTypePartDefinitionRecords":[{"PartName":"TitlePart","Name":"TitlePart","Settings":{"ContentTypePartSettings":{"Position":"0"}}},{"PartName":"AutoroutePart","Name":"AutoroutePart","Settings":{"ContentTypePartSettings":{"Position":"1"},"AutoroutePartSettings":{"AllowCustomPath":false,"AllowUpdatePath":true,"Pattern":"{{ ContentItem | display_text | slugify }}"}}},{"PartName":"Product","Name":"Product","Settings":{"ContentTypePartSettings":{"Position":"2"}}}]}],"ContentParts":[{"Name":"Product","Settings":{},"ContentPartFieldDefinitionRecords":[{"FieldName":"TextField","Name":"Color","Settings":{"ContentPartFieldSettings":{"DisplayName":"Color","Position":"0"}}}]}]}]}
        """;

    /// <summary>Creates the Product type (Title, Autoroute with a fixed pattern, a Color text field), outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", SeedJson);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!");

        foreach (var path in new[] { "/Admin/ContentTypes/List", "/Admin/Contents/ContentTypes/Product/Create", "/Admin/ContentTypes/BlogPost/ContentParts/AutoroutePart/Edit", "/Admin/Contents/ContentItems" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Product type created.");
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
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene.Page);
        }

        await scene.StartCaptureAsync();
        return scene;
    }

    private static ILocator Editor(IPage page) => page.Locator(".CodeMirror").First;

    private static ILocator Permalink(IPage page) => page.GetByLabel("Permalink").First;

    private static ILocator Publish(IPage page) => page.Locator("button.publish").First;

    private static async Task FillProductAsync(Scene scene, string color)
    {
        var page = scene.Page;
        await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Trail Mug", delay: 45);
        await scene.TypeAsync(page.GetByLabel("Color", new() { Exact = true }).First, color, delay: 45);
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/ContentTypes/List", async p =>
        {
            await p.Locator("a[role='btn-edit-Product']").ScrollToMiddleAsync();
        });
        var page = scene.Page;

        await scene.StepAsync("edit-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[role='btn-edit-Product']"));
            await page.Locator("h3").Filter(new() { HasTextString = "Parts" }).First.WaitForAsync();
            var edit = page.Locator("a[href$='/ContentTypes/Product/ContentParts/AutoroutePart/Edit']").First;
            await edit.ScrollToMiddleAsync();
            await GoAsync(scene, edit);
            await Editor(page).WaitForAsync();
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.HighlightAsync(Editor(page), "Pattern");
        });

        await scene.StepAsync("edit-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator(".CodeMirror-line").First, pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+End");
            await page.Keyboard.PressAsync("Enter");
            await page.Keyboard.TypeAsync("-{{ ContentItem.Content.Product.Color.Text | slugify }}", new() { Delay = 40 });
        });

        await scene.StepAsync("edit-4", async () =>
        {
            await page.Keyboard.PressAsync("Control+End");
            await page.Keyboard.PressAsync("Backspace");
            await page.Keyboard.PressAsync("Backspace");
            await GoAsync(scene, page.Locator("button.save").First);
            var error = page.GetByText("Pattern doesn't contain a valid Liquid expression").First;
            await error.WaitForAsync();
            Console.WriteLine("Error: " + (await error.InnerTextAsync()).Replace("\n", " "));
        });

        await scene.StepAsync("edit-5", async () =>
        {
            await scene.ClickAsync(page.Locator(".CodeMirror-line").Last, pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+End");
            await page.Keyboard.TypeAsync("}}", new() { Delay = 80 });
            var value = await page.EvalOnSelectorAsync<string>(".CodeMirror", "e => e.CodeMirror.getValue()");
            Console.WriteLine("Pattern: " + value.Replace("\n", " | "));
            await scene.PauseAsync(2500);
            await GoAsync(scene, page.Locator("button.save").First);
            await WaitTextAsync(page, "part settings have been saved");
        });
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentTypes/Product/Create", p => p.GetByLabel("Title", new() { Exact = true }).First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("result-1", async () =>
        {
            await FillProductAsync(scene, "Forest Green");
            await ShowAsync(scene, Permalink(page), 1200);
            await GoAsync(scene, Publish(page));
            await WaitTextAsync(page, "Your Product has been published.");
        });

        await scene.StepAsync("result-2", async () =>
        {
            await scene.HighlightAsync(Permalink(page));
        });

        await scene.StepAsync("result-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Contents/ContentTypes/Product/Create");
            await FillProductAsync(scene, "Forest Green");
            await GoAsync(scene, Publish(page));
            await WaitTextAsync(page, "Your Product has been published.");
            await scene.HighlightAsync(Permalink(page));
        });

        await scene.StepAsync("result-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var color = page.GetByLabel("Color", new() { Exact = true }).First;
            await color.FillAsync("");
            await scene.TypeAsync(color, "Slate Blue", delay: 45);
            await scene.ClickAsync(page.GetByLabel("Refresh the path"), pauseAfter: 400);
            await GoAsync(scene, Publish(page));
            await WaitTextAsync(page, "Your Product has been published.");
            await scene.HighlightAsync(Permalink(page));
        });
    }

    private static async Task ContainerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var blogHref = "";
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/ContentTypes/BlogPost/ContentParts/AutoroutePart/Edit", async p =>
        {
            await Editor(p).WaitForAsync();
            blogHref = await p.Locator("#left-nav a").Filter(new() { HasTextString = "Blog" }).First.GetAttributeAsync("href");
        });
        var page = scene.Page;

        await scene.StepAsync("container-1", async () =>
        {
            await scene.HighlightAsync(Editor(page), "Pattern");
        });

        await scene.StepAsync("container-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#left-nav a").Filter(new() { HasTextString = "Blog" }).First);
            await GoAsync(scene, page.Locator("a:has-text('Create Blog Post')").First);
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Notes from the Orchard", delay: 45);
            await scene.ClickAsync(page.Locator("button.publish + button.dropdown-toggle").First, pauseAfter: 400);
            await GoAsync(scene, page.Locator("button.publish-continue").First);
            await WaitTextAsync(page, "Your Blog Post has been published.");
        });

        await scene.StepAsync("container-3", async () =>
        {
            await scene.HighlightAsync(Permalink(page));
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

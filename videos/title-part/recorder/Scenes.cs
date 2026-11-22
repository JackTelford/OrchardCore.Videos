using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Title part", recorded in order on one fresh Blog site that has a Product type without a Title part.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-attach", AttachAsync),
        ("03-typed", TypedAsync),
        ("04-generate", GenerateAsync),
        ("05-hidden", HiddenAsync),
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
        {"steps":[{"name":"ContentDefinition","ContentTypes":[{"Name":"Product","DisplayName":"Product","Settings":{"ContentTypeSettings":{"Creatable":true,"Listable":true,"Draftable":true,"Versionable":true}},"ContentTypePartDefinitionRecords":[{"PartName":"Product","Name":"Product","Settings":{"ContentTypePartSettings":{"Position":"0"}}}]}],"ContentParts":[{"Name":"Product","Settings":{},"ContentPartFieldDefinitionRecords":[{"FieldName":"TextField","Name":"Brand","Settings":{"ContentPartFieldSettings":{"DisplayName":"Brand","Position":"0"}}},{"FieldName":"TextField","Name":"Name","Settings":{"ContentPartFieldSettings":{"DisplayName":"Name","Position":"1"}}}]}]}]}
        """;

    /// <summary>Creates the Product type (two Text fields, no Title part), outside the capture.</summary>
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

        foreach (var path in new[] { "/Admin/ContentTypes/List", "/Admin/ContentTypes/Edit/Product", "/Admin/Contents/ContentItems" })
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

    private static ILocator ProductRow(IPage page) => page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("a[role='btn-edit-Product']") }).First;

    private static async Task ProductInViewAsync(IPage page)
        => await page.Locator("a[role='btn-edit-Product']").ScrollToMiddleAsync();

    private static ILocator Options(IPage page) => page.Locator("select[id$='Options']");

    private static ILocator PartEdit(IPage page) => page.Locator("a[href$='/ContentTypes/Product/ContentParts/TitlePart/Edit']").First;

    private static async Task SaveAsync(Scene scene)
    {
        await GoAsync(scene, scene.Page.Locator("button.save[name='submit.Save']").First);
        await WaitTextAsync(scene.Page, "part settings have been saved");
    }

    private static async Task PublishAsync(Scene scene)
    {
        await GoAsync(scene, scene.Page.Locator("button.publish").First);
    }

    private static async Task<Scene> StartListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await ProductInViewAsync(scene.Page);
        await scene.StartCaptureAsync();
        return scene;
    }

    private static async Task AttachAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartListAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("attach-1", async () =>
        {
            await ShowAsync(scene, ProductRow(page), 1200);
            await GoAsync(scene, page.Locator("a[role='btn-edit-Product']"));
            await page.Locator("a[href$='/ContentTypes/AddPartsTo/Product']").WaitForAsync();
        });

        await scene.StepAsync("attach-2", async () =>
        {
            await GoAsync(scene, page.Locator("a[href$='/ContentTypes/AddPartsTo/Product']"));
            var row = page.Locator("li.list-group-item:has(label.form-check-label:text-is('Title'))").First;
            await row.ScrollToMiddleAsync();
            await ShowAsync(scene, row, 1500);
            await GoAsync(scene, page.Locator("button[name='submit.AddAndConfigure'][value='TitlePart']"));
            await Options(page).WaitForAsync();
        });

        await scene.StepAsync("attach-3", async () =>
        {
            await scene.HighlightAsync(Options(page), "Behavior option");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("input[id$='RenderTitle']"), "Render Title");
        });

        await scene.StepAsync("attach-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("input[id$='Placeholder']"), "Product title", delay: 45);
            await scene.SelectAsync(Options(page), "EditableRequired");
        });

        await scene.StepAsync("attach-5", async () =>
        {
            await SaveAsync(scene);
        });
    }

    private static async Task TypedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartListAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("typed-1", async () =>
        {
            await GoAsync(scene, page.Locator("#btn-create-Product"));
            var title = page.GetByLabel("Title", new() { Exact = true }).First;
            await title.WaitForAsync();
            await scene.HighlightAsync(title, "Title");
        });

        await scene.StepAsync("typed-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await PublishAsync(scene);
            var message = page.Locator(".validation-summary-errors, .field-validation-error").Filter(new() { HasTextString = "A value is required for Title" }).First;
            await message.WaitForAsync();
            await message.ScrollToMiddleAsync();
            await scene.HighlightAsync(message);
        });

        await scene.StepAsync("typed-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Brightwater Camp Stove", delay: 40);
            await PublishAsync(scene);
            await WaitTextAsync(page, "Your Product has been published.");
            await ProductInViewAsync(page);
        });

        await scene.StepAsync("typed-4", async () =>
        {
            await GoAsync(scene, page.Locator("a[role='btn-list-Product']"));
            var link = Row(page, "Brightwater Camp Stove");
            await link.WaitForAsync();
            await scene.HighlightAsync(link);
        });
    }

    private static async Task GenerateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartListAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("gen-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[role='btn-edit-Product']"));
            await PartEdit(page).ScrollToMiddleAsync();
            await GoAsync(scene, PartEdit(page));
            await scene.SelectAsync(Options(page), "GeneratedDisabled");
            await page.Locator("#patternEditor .CodeMirror").WaitForAsync();
        });

        await scene.StepAsync("gen-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("#patternEditor"), "Pattern");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#patternEditor .CodeMirror"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("{{ ContentItem.Content.Product.Brand.Text }} {{ ContentItem.Content.Product.Name.Text }}", new() { Delay = 30 });
            var value = await page.EvalOnSelectorAsync<string>("#patternEditor .CodeMirror", "e => e.CodeMirror.getValue()");
            Console.WriteLine("Pattern: " + value);
        });

        await scene.StepAsync("gen-3", async () =>
        {
            await scene.MoveToAsync(page.Locator("input[id$='Placeholder']"));
            await page.Locator("input[id$='Placeholder']").FillAsync("");
            await SaveAsync(scene);
        });

        await scene.StepAsync("gen-4", async () =>
        {
            await VisitAsync(scene, "/Admin/ContentTypes/List");
            await ProductInViewAsync(page);
            await GoAsync(scene, page.Locator("#btn-create-Product"));
            var hint = page.GetByText("The title will be auto-generated using the pattern.").First;
            await hint.WaitForAsync();
            await scene.HighlightAsync(hint);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.GetByLabel("Brand", new() { Exact = true }), "Brightwater", delay: 40);
            await scene.TypeAsync(page.GetByLabel("Name", new() { Exact = true }), "Trail Backpack", delay: 40);
            await PublishAsync(scene);
            await WaitTextAsync(page, "Your Product has been published.");
            await ProductInViewAsync(page);
        });

        await scene.StepAsync("gen-5", async () =>
        {
            await GoAsync(scene, page.Locator("a[role='btn-list-Product']"));
            var link = Row(page, "Brightwater Trail Backpack");
            await link.WaitForAsync();
            await scene.HighlightAsync(link);
        });

        await scene.StepAsync("gen-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, Row(page, "Brightwater Trail Backpack").Locator("a.edit").First);
            await page.GetByLabel("Brand", new() { Exact = true }).WaitForAsync();
        });

        await scene.StepAsync("gen-7", async () =>
        {
            await GoAsync(scene, page.Locator("a.cancel").First);
            var view = Row(page, "Brightwater Trail Backpack").Locator("a.view").First;
            var popup = await scene.Context.RunAndWaitForPageAsync(async () => await scene.ClickAsync(view, pauseAfter: 200));
            await scene.SwitchToAsync(popup);
            await popup.Locator("h1").Filter(new() { HasTextString = "Brightwater Trail Backpack" }).First.WaitForAsync();
        });
    }

    private static async Task HiddenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartListAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("hidden-1", async () =>
        {
            await GoAsync(scene, page.Locator("a[role='btn-edit-Product']"));
            await PartEdit(page).ScrollToMiddleAsync();
            await GoAsync(scene, PartEdit(page));
            await scene.SelectAsync(Options(page), "GeneratedHidden");
            await page.Locator("#patternEditor .CodeMirror").WaitForAsync();
            await scene.HighlightAsync(page.Locator("#patternEditor"), "Pattern");
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene);
        });

        await scene.StepAsync("hidden-2", async () =>
        {
            await VisitAsync(scene, "/Admin/ContentTypes/List");
            await ProductInViewAsync(page);
            await GoAsync(scene, page.Locator("#btn-create-Product"));
            await page.GetByLabel("Brand", new() { Exact = true }).WaitForAsync();
            await scene.TypeAsync(page.GetByLabel("Brand", new() { Exact = true }), "Brightwater", delay: 40);
            await scene.TypeAsync(page.GetByLabel("Name", new() { Exact = true }), "Water Bottle", delay: 40);
            await PublishAsync(scene);
            await WaitTextAsync(page, "Your Product has been published.");
        });

        await scene.StepAsync("hidden-3", async () =>
        {
            await ProductInViewAsync(page);
            await GoAsync(scene, page.Locator("a[role='btn-list-Product']"));
            var link = Row(page, "Brightwater Water Bottle");
            await link.WaitForAsync();
            await scene.HighlightAsync(link);
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

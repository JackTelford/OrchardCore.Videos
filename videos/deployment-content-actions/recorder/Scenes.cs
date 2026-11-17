using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Export Content From the List". PrepareAsync creates the plan Orchard Harvest Content and three
/// published articles off camera. The clips depend on each other (see recording-plan.md): 02-features enables the
/// features, 04-add and 04-many fill the plan, 05-settings picks the export plan, 06-* export.
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
        ("02-features", FeaturesAsync),
        ("03-download", DownloadAsync),
        ("04-add", AddAsync),
        ("04-many", ManyAsync),
        ("05-settings", SettingsAsync),
        ("06-export", ExportAsync),
        ("06-many", ExportManyAsync),
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

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task GoAsync(string path) => await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.NetworkIdle });

        await GoAsync("/Admin/DeploymentPlan/Create");
        await page.Locator("input[name='Name']").FillAsync("Orchard Harvest Content");
        await page.Locator("button.create").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1000);

        foreach (var title in new[] { "Cider Press Rota", "Pruning Guide", "Apple Harvest Schedule" })
        {
            await GoAsync("/Admin/Contents/ContentTypes/Article/Create");
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator("button.publish").ClickAsync();
            await page.WaitForURLAsync(url => !url.Contains("/Create"));
            await Task.Delay(1500);
        }

        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 4 });
    }

    private static async Task OpenGroupAsync(Scene scene, string group)
    {
        var button = scene.Page.Locator("#left-nav button.item-label").Filter(new() { HasTextRegex = new($"^\\s*{group}\\s*$") }).First;

        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(button, pauseAfter: 150);
        }
    }

    private static async Task GoMenuAsync(Scene scene, string link)
        => await scene.WaitForNavigationAsync(() => scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new($"^\\s*{link}\\s*$") }).First.ClickAsync());

    private static ILocator Row(Scene scene, string title) => scene.Page.Locator($"li.list-group-item:has(a:text-is('{title}'))").First;

    private static async Task OpenActionsAsync(Scene scene, string title)
        => await scene.ClickAsync(Row(scene, title).Locator("button.actions"), pauseAfter: 400);

    private static ILocator MenuItem(Scene scene, string title, string text)
        => Row(scene, title).Locator(".actions-menu a.dropdown-item, .actions-menu button.dropdown-item, .actions-menu button").Filter(new() { HasTextRegex = new($"^\\s*{text}\\s*$") }).First;

    private static async Task SelectRowAsync(Scene scene, string title)
        => await scene.ClickAsync(Row(scene, title).Locator("input[name='itemIds']"), pauseAfter: 200);

    private static ILocator BulkItem(Scene scene, string text)
        => scene.Page.Locator("#actions .dropdown-item, .bulk-actions .dropdown-item").Filter(new() { HasTextRegex = new($"^\\s*{text}\\s*$") }).First;

    private static async Task ConfirmAsync(Scene scene)
    {
        await scene.Page.Locator("#confirmRemoveModal.show").WaitForAsync();
        await scene.PauseAsync(800);
        await scene.WaitForNavigationAsync(async () => await scene.ClickAsync(scene.Page.Locator("#modalOkButton"), pauseAfter: 100));
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await GoMenuAsync(scene, "Features");
            await FastAsync(scene.Page.Locator("#search-box"), "content items list");
        });

        await scene.StepAsync("features-2", async () =>
        {
            foreach (var box in await scene.Page.Locator("li.list-group-item:visible input.form-check-input").AllAsync())
            {
                await scene.MoveToAsync(box);
                await box.CheckAsync();
                await scene.PauseAsync(200);
            }

            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button"), pauseAfter: 500);
            var enable = scene.Page.Locator("a.dropdown-item[data-action='Enable']").First;
            await enable.WaitForAsync();
            await scene.MoveToAsync(enable);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await enable.ClickAsync();
                await Task.Delay(8000);
            }));
        });

        await scene.StepAsync("features-3", async () =>
        {
            await FastAsync(scene.Page.Locator("#search-box"), "content items list");
        });
    }

    private static async Task DownloadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("download-1", async () =>
        {
            await OpenActionsAsync(scene, "Apple Harvest Schedule");
            await scene.HighlightAsync(MenuItem(scene, "Apple Harvest Schedule", "View as JSON"));
            await scene.HighlightAsync(MenuItem(scene, "Apple Harvest Schedule", "Download as JSON"));
        });

        await scene.StepAsync("download-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => MenuItem(scene, "Apple Harvest Schedule", "View as JSON").ClickAsync());
            await scene.Page.Locator(".CodeMirror, .monaco-editor").First.WaitForAsync();
        });

        await scene.StepAsync("download-3", async () =>
        {
            var download = await scene.Page.RunAndWaitForDownloadAsync(async () =>
                await scene.ClickAsync(scene.Page.Locator("button:has-text('Download'), a:has-text('Download')").First, pauseAfter: 200));
            await download.DeleteAsync();
            await scene.PauseAsync(1500);
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("a.cancel, a:has-text('Cancel')").First.ClickAsync());
        });

        await scene.StepAsync("download-4", async () =>
        {
            await OpenActionsAsync(scene, "Apple Harvest Schedule");
            var download = await scene.Page.RunAndWaitForDownloadAsync(async () =>
                await scene.ClickAsync(MenuItem(scene, "Apple Harvest Schedule", "Download as JSON"), pauseAfter: 200));
            await download.DeleteAsync();
        });
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await OpenActionsAsync(scene, "Apple Harvest Schedule");
            await scene.ClickAsync(MenuItem(scene, "Apple Harvest Schedule", "Add to Deployment Plan"), pauseAfter: 300);
            await scene.Page.Locator(".modal.show").First.WaitForAsync();
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .card", new() { HasText = "Orchard Harvest Content" }).First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var select = scene.Page.Locator(".modal.show .card", new() { HasText = "Orchard Harvest Content" }).First.Locator("a, button").Filter(new() { HasTextRegex = new("^\\s*Select\\s*$") }).First;
            await scene.WaitForNavigationAsync(async () => await scene.ClickAsync(select, pauseAfter: 100));
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        });

        await scene.StepAsync("add-4", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await OpenGroupAsync(scene, "Deployments");
            await GoMenuAsync(scene, "Plans");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("li.list-group-item", new() { HasText = "Orchard Harvest Content" }).First.Locator("a", new() { HasTextString = "Manage Steps" }).First.ClickAsync());
        });

        await scene.StepAsync("add-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("ul#stepOrder li").First);
        });
    }

    private static async Task ManyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("addmany-1", async () =>
        {
            await SelectRowAsync(scene, "Pruning Guide");
            await SelectRowAsync(scene, "Cider Press Rota");
            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button"), pauseAfter: 400);
            await scene.ClickAsync(BulkItem(scene, "Add to deployment plan"), pauseAfter: 300);
        });

        await scene.StepAsync("addmany-2", async () =>
        {
            await scene.Page.Locator("#confirmRemoveModal.show").WaitForAsync();
            await scene.PauseAsync(800);
            await scene.ClickAsync(scene.Page.Locator("#modalOkButton"), pauseAfter: 200);
            await scene.Page.Locator(".modal.show .card", new() { HasText = "Orchard Harvest Content" }).First.WaitForAsync();
            var select = scene.Page.Locator(".modal.show .card", new() { HasText = "Orchard Harvest Content" }).First.Locator("a, button").Filter(new() { HasTextRegex = new("^\\s*Select\\s*$") }).First;
            await scene.WaitForNavigationAsync(async () => await scene.ClickAsync(select, pauseAfter: 100));
        });

        await scene.StepAsync("addmany-3", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await OpenGroupAsync(scene, "Deployments");
            await GoMenuAsync(scene, "Plans");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("li.list-group-item", new() { HasText = "Orchard Harvest Content" }).First.Locator("a", new() { HasTextString = "Manage Steps" }).First.ClickAsync());
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await OpenGroupAsync(scene, "Deployments");
            await GoMenuAsync(scene, "Plans");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("li.list-group-item", new() { HasText = "Export content to deployment plan target" }).First.Locator("a", new() { HasTextString = "Manage Steps" }).First.ClickAsync());
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("ul#stepOrder li").First);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenGroupAsync(scene, "Settings");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new("^\\s*Deployment Targets\\s*$") }).First.ClickAsync());
            var plan = scene.Page.Locator("select").First;
            await scene.SelectAsync(plan, "Export content to deployment plan target");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
        });
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("export-1", async () =>
        {
            await OpenActionsAsync(scene, "Apple Harvest Schedule");
            await scene.ClickAsync(MenuItem(scene, "Apple Harvest Schedule", "Export to Deployment Target"), pauseAfter: 300);
            await scene.Page.Locator(".modal.show").First.WaitForAsync();
        });

        await scene.StepAsync("export-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .card").First);
        });

        await scene.StepAsync("export-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var select = scene.Page.Locator(".modal.show .card", new() { HasText = "File Download" }).First.Locator("a, button").Filter(new() { HasTextRegex = new("^\\s*Select\\s*$") }).First;
            var download = await scene.Page.RunAndWaitForDownloadAsync(async () => await scene.ClickAsync(select, pauseAfter: 200));
            await download.DeleteAsync();
        });
    }

    private static async Task ExportManyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("exportmany-1", async () =>
        {
            await SelectRowAsync(scene, "Pruning Guide");
            await SelectRowAsync(scene, "Cider Press Rota");
            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button"), pauseAfter: 400);
            await scene.ClickAsync(BulkItem(scene, "Export to deployment target"), pauseAfter: 300);
        });

        await scene.StepAsync("exportmany-2", async () =>
        {
            await scene.Page.Locator("#confirmRemoveModal.show").WaitForAsync();
            await scene.PauseAsync(800);
            await scene.ClickAsync(scene.Page.Locator("#modalOkButton"), pauseAfter: 200);
            await scene.Page.Locator(".modal.show .card", new() { HasText = "File Download" }).First.WaitForAsync();
            var select = scene.Page.Locator(".modal.show .card", new() { HasText = "File Download" }).First.Locator("a, button").Filter(new() { HasTextRegex = new("^\\s*Select\\s*$") }).First;
            var download = await scene.Page.RunAndWaitForDownloadAsync(async () => await scene.ClickAsync(select, pauseAfter: 200));
            await download.DeleteAsync();
        });
    }
}

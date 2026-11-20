using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Content Definition Stores". PrepareAsync builds the starting state outside the capture: the file
/// store is enabled, holds the Blog definitions (imported from a package exported from the database) and the type
/// Event. The three clips are one story: 03-export, then 03-switch, then 03-import, without restoring in between.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static string Downloads { get; } = Environment.GetEnvironmentVariable("VIDEO_DOWNLOADS") ?? Path.Combine(Path.GetTempPath(), "ocv-cds-downloads");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-export", ExportAsync),
        ("03-switch", SwitchAsync),
        ("03-import", ImportAsync),
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
        Directory.CreateDirectory(Downloads);
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height }, AcceptDownloads = true });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task GoAsync(string path) => await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // 2. Export the Blog definitions from the database, as Seed.zip.
        await GoAsync("/Admin/DeploymentPlan/Index");
        await page.Locator("a.btn:has-text('Add Deployment Plan')").First.ClickAsync();
        await page.Locator("input[name='Name']").FillAsync("Seed");
        await page.Locator("button.create").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("button:has-text('Add Step')").First.ClickAsync();
        await page.Locator("#modalSteps.show").WaitForAsync();
        await page.Locator("#modalSteps .deployment-step-item:has-text('Update Content Definitions') a:has-text('Add')").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("label:has-text('Include all content types and parts definitions.')").First.ClickAsync();
        await page.Locator("button.create").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("button:has-text('Execute')").First.ClickAsync();
        await page.Locator("#modalTargets.show").WaitForAsync();
        var download = await page.RunAndWaitForDownloadAsync(async () =>
            await page.Locator("#modalTargets .card:has-text('File Download') a:has-text('Select')").First.ClickAsync());
        await download.SaveAsAsync(Path.Combine(Downloads, "Seed.zip"));

        // 3. Delete the plan.
        await GoAsync("/Admin/DeploymentPlan/Index");
        var row = page.Locator("li.list-group-item:has-text('Seed')").First;
        var delete = row.Locator("a:has-text('Delete')").First;

        if (!await delete.IsVisibleAsync())
        {
            await row.Locator("button.dropdown-toggle").First.ClickAsync();
        }

        await delete.ClickAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);

        // 4. Enable the file store.
        await GoAsync("/Admin/Features");
        await page.Locator("#search-box").FillAsync("File Content");
        await page.Locator("#btn-enable-OrchardCore_Contents_FileContentDefinition").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(5000);

        // 5. Put the Blog definitions in the file.
        await GoAsync("/Admin/DeploymentPlan/Import/Index");
        await page.Locator("#file").SetInputFilesAsync(Path.Combine(Downloads, "Seed.zip"));
        await page.Locator("button.import").ClickAsync();
        await page.Locator(".toast.message-success").First.WaitForAsync();
        await Task.Delay(3000);

        // 6. Create the type Event in the file.
        await GoAsync("/Admin/ContentTypes/Create");
        await page.Locator("#DisplayName").FillAsync("Event");
        await page.Keyboard.PressAsync("Tab");
        await page.Locator("button.create").ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("AddPartsTo"));
        await Task.Delay(1500);

        if (File.Exists(Path.Combine(Downloads, "ContentDefinitions.zip")))
        {
            File.Delete(Path.Combine(Downloads, "ContentDefinitions.zip"));
        }

        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(Scene scene, ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 2 });
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        Directory.CreateDirectory(Downloads);
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("export-1", async () =>
        {
            await scene.PauseAsync(4500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn:has-text('Add Deployment Plan')").First);
            await FastAsync(scene, scene.Page.Locator("input[name='Name']"), "Content Definitions");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("button.create").First.ClickAsync());
        });

        await scene.StepAsync("export-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Step')").First, pauseAfter: 300);
            await scene.Page.Locator("#modalSteps.show").WaitForAsync();
            await FastAsync(scene, scene.Page.Locator("#modalSteps #search-box"), "Update Content");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("#modalSteps .deployment-step-item:has-text('Update Content Definitions') a:has-text('Add')").First.ClickAsync());
            await scene.ClickAsync(scene.Page.Locator("label:has-text('Include all content types and parts definitions.')").First, pauseAfter: 300);
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("button.create").First.ClickAsync());
        });

        await scene.StepAsync("export-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Execute')").First, pauseAfter: 300);
            await scene.Page.Locator("#modalTargets.show").WaitForAsync();
            var card = scene.Page.Locator("#modalTargets .card:has-text('File Download')").First;
            await scene.PauseAsync(3000);
            var download = await scene.Page.RunAndWaitForDownloadAsync(async () =>
                await scene.ClickAsync(card.Locator("a:has-text('Select')").First, pauseAfter: 200));
            await download.SaveAsAsync(Path.Combine(Downloads, "ContentDefinitions.zip"));
            await scene.HighlightAsync(card, "ContentDefinitions.zip");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SwitchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("switch-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "File Content", 10);
            await scene.ClickAsync(scene.Page.Locator("#btn-disable-OrchardCore_Contents_FileContentDefinition"), pauseAfter: 1500);
            await scene.Page.Locator("#modalOkButton").WaitForAsync();
            var yes = scene.Page.Locator("#modalOkButton");
            await scene.MoveToAsync(yes);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await yes.ClickAsync();
                await scene.Page.Locator(".toast.message-success").First.WaitForAsync(new() { Timeout = 60000 });
            }));
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "File Content", 10);
        });

        await scene.StepAsync("switch-2", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/List");
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Event", 30);
        });
    }

    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("import-1", async () =>
        {
            await scene.NavigateAsync("/Admin/DeploymentPlan/Import/Index");
            await scene.MoveToAsync(scene.Page.Locator("#file"));
            await scene.Page.Locator("#file").SetInputFilesAsync(Path.Combine(Downloads, "ContentDefinitions.zip"));
            await scene.MoveToAsync(scene.Page.Locator("button.import"));
        });

        await scene.StepAsync("import-2", async () =>
        {
            // The import is slow: the wait is cut off camera, so the video jumps to the page with its message.
            var import = scene.Page.Locator("button.import");
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await import.ClickAsync();
                await scene.Page.Locator(".toast.message-success").First.WaitForAsync(new() { Timeout = 60000 });
            }));
            await scene.PauseAsync(3500);
            await scene.NavigateAsync("/Admin/ContentTypes/List");
            await FastAsync(scene, scene.Page.Locator("#search-box"), "Event");
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Deployment overview": a plan on the source site (the Default tenant) and an import on the
/// Staging tenant (URL prefix "staging"), which has a sign-in of its own, state-staging.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static string StagingStatePath => Path.Combine(Root, "state-staging.json");

    // Where the plan's zip is saved: outside the repository.
    private static string PackageDirectory { get; } = Environment.GetEnvironmentVariable("VIDEO_PACKAGE_DIR") ?? Path.Combine(Path.GetTempPath(), "ocv-package");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-find", FindAsync),
        ("03-plan", PlanAsync),
        ("04-import", ImportAsync),
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

    // Seeds the Page size of both tenants outside the capture: 25 on the source site, 10 on the Staging site.
    public static async Task PrepareAsync(IBrowser browser)
    {
        async Task SetPageSizeAsync(string baseUrl, string state, string value)
        {
            var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = state, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(30_000);
            await page.GotoAsync($"{baseUrl}/Admin/Settings/general", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#ISite_PageSize").FillAsync(value);
            await page.Locator("button.save").First.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(2500);
            await context.CloseAsync();
        }

        await SetPageSizeAsync(BaseUrl, StatePath, "25");
        await SetPageSizeAsync(BaseUrl + "/staging", StagingStatePath, "10");
        Console.WriteLine("Seeded.");
    }

    private static ILocator PageSizeField(IPage page) => page.Locator("#ISite_PageSize");

    private static async Task CloseGroupAsync(IPage page, string group)
    {
        var button = page.Locator("button.item-label", new() { HasTextString = group }).First;

        if (await button.IsVisibleAsync() && await button.GetAttributeAsync("aria-expanded") == "true")
        {
            await button.ClickAsync();
            await Task.Delay(500);
        }
    }

    private static async Task OpenGroupAsync(Scene scene, string group)
    {
        var button = scene.Page.Locator("button.item-label").Filter(new() { HasTextRegex = new($"^\\s*{group}\\s*$") }).First;

        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(button, pauseAfter: 500);
        }
    }

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await CloseGroupAsync(scene.Page, "Deployments");
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Deployment", 40);
            await scene.Page.Locator("#btn-disable-OrchardCore_Deployment").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-disable-OrchardCore_Deployment") }));
        });

        await scene.StepAsync("find-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenGroupAsync(scene, "Tools");
            await OpenGroupAsync(scene, "Deployments");
            await scene.Page.Locator("a.item-label", new() { HasTextString = "Plans" }).First.WaitForAsync();

            foreach (var entry in new[] { "Plans", "Package Import", "JSON Import", "Remote Instances", "Remote Clients" })
            {
                await scene.ClearHighlightsAsync();
                await scene.HighlightAsync(scene.Page.Locator("a.item-label").Filter(new() { HasTextRegex = new($"^\\s*{entry}\\s*$") }).First);
                await scene.PauseAsync(1000);
            }
        });
    }

    private static async Task PlanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        Directory.CreateDirectory(PackageDirectory);
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/general");
        await scene.StartCaptureAsync();

        await scene.StepAsync("plan-1", async () =>
        {
            await scene.HighlightAsync(PageSizeField(scene.Page));
        });

        await scene.StepAsync("plan-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/DeploymentPlan/Index");
            await scene.PauseAsync(5000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create", new() { HasTextString = "Add Deployment Plan" }).First);
        });

        await scene.StepAsync("plan-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Staging Setup", 30);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.create", new() { HasTextString = "Create" }).First);
        });

        await scene.StepAsync("plan-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a", new() { HasTextString = "Add Step" }).First, pauseAfter: 300);
            await scene.Page.Locator("#modalSteps.show").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#deployment-step-categories"));
        });

        await scene.StepAsync("plan-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#deployment-step-categories .nav-link", new() { HasTextString = "Configuration" }).First, pauseAfter: 500);
            var card = scene.Page.Locator(".deployment-step-item").Filter(new() { Has = scene.Page.Locator("h4.card-title", new() { HasTextString = "Site Settings" }) }).First;
            await card.WaitForAsync();
            await scene.HighlightAsync(card);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(card.Locator("a", new() { HasTextRegex = new("^\\s*Add\\s*$") }).First);
        });

        await scene.StepAsync("plan-6", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("input[type=checkbox][value='PageSize']"), pauseAfter: 600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.create", new() { HasTextString = "Create" }).First);
        });

        await scene.StepAsync("plan-7", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a", new() { HasTextRegex = new("^\\s*Execute\\s*$") }).First, pauseAfter: 300);
            await scene.Page.Locator("#modalTargets.show").WaitForAsync();
            var card = scene.Page.Locator("#modalTargets .card, #modalTargets .deployment-target-item").Filter(new() { Has = scene.Page.Locator("h4.card-title", new() { HasTextString = "File Download" }) }).First;
            await scene.HighlightAsync(card);
            await scene.PauseAsync(5500);
            var download = await scene.Page.RunAndWaitForDownloadAsync(async () =>
                await scene.ClickAsync(card.Locator("a", new() { HasTextRegex = new("^\\s*Select\\s*$") }).First, pauseAfter: 200));
            await download.SaveAsAsync(Path.Combine(PackageDirectory, "StagingSetup.zip"));
            Console.WriteLine($"  downloaded {download.SuggestedFilename}");
        });

        await scene.StepAsync("plan-8", async () =>
        {
            await scene.ClearHighlightsAsync();
            var card = scene.Page.Locator("#modalTargets .card, #modalTargets .deployment-target-item").Filter(new() { Has = scene.Page.Locator("h4.card-title", new() { HasTextString = "File Download" }) }).First;
            await scene.HighlightAsync(card, "StagingSetup.zip");
        });
    }

    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl + "/staging", StagingStatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Admin/Settings/general");
        await CloseGroupAsync(scene.Page, "Deployments");
        await CloseGroupAsync(scene.Page, "Tools");
        await scene.StartCaptureAsync();

        await scene.StepAsync("import-1", async () =>
        {
            await scene.HighlightAsync(PageSizeField(scene.Page));
        });

        await scene.StepAsync("import-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenGroupAsync(scene, "Tools");
            await OpenGroupAsync(scene, "Deployments");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.item-label").Filter(new() { HasTextRegex = new("^\\s*Package Import\\s*$") }).First);
            await scene.Page.Locator("#file").SetInputFilesAsync(Path.Combine(PackageDirectory, "StagingSetup.zip"));
        });

        await scene.StepAsync("import-3", async () =>
        {
            var wait = Math.Max(0, durations["import-3"] - 5.5);
            await scene.PauseAsync((int)(wait * 1000));
            await scene.WaitForNavigationAsync(async () => await scene.ClickAsync(scene.Page.Locator("button.import, button[type=submit]", new() { HasTextString = "Import" }).First, pauseAfter: 100));
        });

        await scene.StepAsync("import-4", async () =>
        {
            await OpenGroupAsync(scene, "Settings");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.item-label").Filter(new() { HasTextRegex = new("^\\s*General\\s*$") }).First);
            await scene.HighlightAsync(PageSizeField(scene.Page));
        });
    }
}

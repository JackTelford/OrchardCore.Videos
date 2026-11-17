using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Orchard Core deployment plans". PrepareAsync creates two empty plans; each clip starts from the
/// state the previous one left (02-create adds Spring Launch, 03 to 05 add its steps, 07-manage renames a plan).
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
        ("02-create", CreateAsync),
        ("03-steps", StepsAsync),
        ("04-order", OrderAsync),
        ("05-general", GeneralAsync),
        ("06-execute", ExecuteAsync),
        ("07-manage", ManageAsync),
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
        page.SetDefaultTimeout(30_000);

        foreach (var name in new[] { "Archive Old Posts", "Weekly Settings Backup" })
        {
            await page.GotoAsync($"{BaseUrl}/Admin/DeploymentPlan/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("input[name='Name']").FillAsync(name);
            await page.Locator("button.create").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(1000);
        }

        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(ILocator field, string text, int delay = 6)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
    }

    private static async Task OpenGroupAsync(Scene scene, string group)
    {
        var button = scene.Page.Locator("#left-nav button.item-label").Filter(new() { HasTextRegex = new($"^\\s*{group}\\s*$") }).First;

        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(button, pauseAfter: 250);
        }
    }

    private static ILocator Modal(Scene scene) => scene.Page.Locator("#modalSteps");

    private static ILocator Category(Scene scene, string name)
        => scene.Page.Locator("#deployment-step-categories .nav-link").Filter(new() { HasTextRegex = new($"^\\s*{name}\\s*$") }).First;

    private static ILocator Card(Scene scene, string title)
        => Modal(scene).Locator(".deployment-step-item").Filter(new() { Has = scene.Page.Locator("h4.card-title", new() { HasTextRegex = new($"^\\s*{title}\\s*$") }) }).First;

    private static async Task OpenStepsDialogAsync(Scene scene)
    {
        await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Step')").First, pauseAfter: 200);
        await Modal(scene).Locator(".deployment-step-item").First.WaitForAsync();
    }

    private static async Task AddCardAsync(Scene scene, string title)
        => await scene.WaitForNavigationAsync(() => Card(scene, title).Locator("a", new() { HasTextRegex = new("^\\s*Add\\s*$") }).First.ClickAsync());

    private static ILocator CreateButton(Scene scene) => scene.Page.Locator("button.create").First;

    private static async Task OpenPlanAsync(Scene scene, string name)
    {
        await scene.Page.Locator("li.list-group-item", new() { HasText = name }).First.Locator("a", new() { HasTextString = "Manage Steps" }).First.ClickAsync();
        await scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static ILocator Step(Scene scene, string title)
        => scene.Page.Locator("#stepOrder > li").Filter(new() { Has = scene.Page.GetByText(title, new() { Exact = true }) }).First;

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await OpenGroupAsync(scene, "Deployments");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new("^\\s*Plans\\s*$") }).First);
            await scene.HighlightAsync(scene.Page.Locator("a.create").First);
            await scene.PauseAsync(600);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await FastAsync(scene.Page.Locator("input[name='Name']"), "Spring Launch");
            await scene.WaitForNavigationAsync(() => CreateButton(scene).ClickAsync());
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button:has-text('Execute')").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button:has-text('Add Step')").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".alert-info").First);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task StepsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await OpenPlanAsync(scene, "Spring Launch");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await OpenStepsDialogAsync(scene);
            await scene.HighlightAsync(Modal(scene).Locator("input[type=search]").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#deployment-step-categories"));
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Category(scene, "Security"), pauseAfter: 400);
            await scene.HighlightAsync(Card(scene, "All Roles"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card(scene, "All Users"));
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Category(scene, "All"), pauseAfter: 300);
            await FastAsync(Modal(scene).Locator("input[type=search]").First, "content");
            await scene.PauseAsync(400);
            await AddCardAsync(scene, "Content");
        });

        await scene.StepAsync("add-4", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByLabel("Blog Post", new() { Exact = true }), pauseAfter: 300);
            await scene.HighlightAsync(scene.Page.GetByText("Export As Setup recipe").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => CreateButton(scene).ClickAsync());
        });

        await scene.StepAsync("add-5", async () =>
        {
            await OpenStepsDialogAsync(scene);
            await FastAsync(Modal(scene).Locator("input[type=search]").First, "update content");
            await scene.PauseAsync(400);
            await AddCardAsync(scene, "Update Content Definitions");
        });

        await scene.StepAsync("add-6", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Include all content types and parts definitions.").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByLabel("Blog Post", new() { Exact = true }), pauseAfter: 300);
            await scene.WaitForNavigationAsync(() => CreateButton(scene).ClickAsync());
        });
    }

    private static async Task OrderAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await OpenPlanAsync(scene, "Spring Launch");
        var planUrl = scene.Page.Url.Replace(BaseUrl, string.Empty);
        await scene.StartCaptureAsync();

        await scene.StepAsync("order-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Content Definitions should be placed before any content steps.").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.DragToAsync(scene.Page.Locator("#stepOrder > li:nth-child(2) .ui-sortable-handle").First, scene.Page.Locator("#stepOrder > li:nth-child(1)").First, 0, -10);
        });

        await scene.StepAsync("order-2", async () =>
        {
            await scene.PauseAsync(Math.Max(0, (int)((durations["order-2"] - 3.5) * 1000)));
            await scene.NavigateAsync(planUrl);
        });

        await scene.StepAsync("order-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Step(scene, "Content").Locator("a", new() { HasTextRegex = new("^\\s*Edit\\s*$") }).First);
            await scene.ClickAsync(scene.Page.GetByLabel("Page", new() { Exact = true }), pauseAfter: 300);
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("button.save, button:has-text('Save')").First.ClickAsync());
        });

        await scene.StepAsync("order-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#stepOrder > li:nth-child(1) a:text-is('Delete'), #stepOrder > li:nth-child(1) button:text-is('Delete')").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#stepOrder > li:nth-child(2) a:text-is('Delete'), #stepOrder > li:nth-child(2) button:text-is('Delete')").First);
        });
    }

    private static async Task GeneralAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await OpenPlanAsync(scene, "Spring Launch");
        await scene.StartCaptureAsync();

        await scene.StepAsync("general-1", async () =>
        {
            await OpenStepsDialogAsync(scene);
            await scene.ClickAsync(Category(scene, "Deployment"), pauseAfter: 400);
        });

        await scene.StepAsync("general-2", async () =>
        {
            foreach (var title in new[] { "Custom File", "Deployment Plans", "JSON Recipe" })
            {
                await scene.ClearHighlightsAsync();
                await scene.HighlightAsync(Card(scene, title).Locator("h4.card-title"));
                await scene.PauseAsync(1700);
            }
        });

        await scene.StepAsync("general-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await AddCardAsync(scene, "Custom File");
            await FastAsync(scene.Page.GetByLabel("File Name", new() { Exact = true }), "notes/readme.txt");
            await FastAsync(scene.Page.GetByLabel("File Content", new() { Exact = true }), "Prepared for the spring launch.");
            await scene.WaitForNavigationAsync(() => CreateButton(scene).ClickAsync());
        });

        await scene.StepAsync("general-4", async () =>
        {
            await OpenStepsDialogAsync(scene);
            await FastAsync(Modal(scene).Locator("input[type=search]").First, "recipe file");
            await scene.PauseAsync(400);
            await AddCardAsync(scene, "Recipe File");
            await FastAsync(scene.Page.GetByLabel("Recipe Name", new() { Exact = true }), "spring-launch");
            await FastAsync(scene.Page.GetByLabel("Display Name", new() { Exact = true }), "Spring Launch");
            await scene.WaitForNavigationAsync(() => CreateButton(scene).ClickAsync());
        });
    }

    private static async Task ExecuteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await OpenPlanAsync(scene, "Spring Launch");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Execute')").First, pauseAfter: 300);
            await scene.Page.Locator("#modalTargets.show").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#modalTargets .card").Filter(new() { HasText = "File Download" }).First);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var select = scene.Page.Locator("#modalTargets .card").Filter(new() { HasText = "File Download" }).First.Locator("a:has-text('Select')").First;
            var download = await scene.Page.RunAndWaitForDownloadAsync(async () => await scene.ClickAsync(select, pauseAfter: 200));
            Console.WriteLine($"  downloaded {download.SuggestedFilename}");
            await download.DeleteAsync();
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#modalTargets button:has-text('Cancel')").First, pauseAfter: 300);
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("manage-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Weekly", 40);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            var row = scene.Page.Locator("li.list-group-item", new() { HasText = "Weekly Settings Backup" }).First;

            foreach (var label in new[] { "Manage Steps", "Edit", "Delete" })
            {
                await scene.ClearHighlightsAsync();
                await scene.HighlightAsync(row.Locator("a, button").Filter(new() { HasTextRegex = new($"^\\s*{label}\\s*$") }).First);
                await scene.PauseAsync(900);
            }
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item", new() { HasText = "Weekly Settings Backup" }).First.Locator("a", new() { HasTextRegex = new("^\\s*Edit\\s*$") }).First);
            await scene.Page.Locator("input[name='Name']").FillAsync(string.Empty);
            await FastAsync(scene.Page.Locator("input[name='Name']"), "Weekly Settings Export");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("button.save, button:has-text('Save')").First.ClickAsync());
        });
    }
}

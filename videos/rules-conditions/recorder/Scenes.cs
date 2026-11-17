using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Rules: Conditions That Decide When a Layer Shows". PrepareAsync imports seed.json (the layer Welcome with
/// one widget and an empty rule). Record in one take, in order: 03-open, 04-group, 05-conditions, 06-site, 07-visitor.
/// Each clip leaves the site in the state the next one starts from.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string EditLayer = "/Admin/Layers/Edit?name=Welcome";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-open", OpenAsync),
        ("04-group", GroupAsync),
        ("05-conditions", ConditionsAsync),
        ("06-site", SiteAsync),
        ("07-visitor", VisitorAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var json = File.ReadAllText(Path.Combine(Root, "recorder", "seed.json"));
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Imported the layer Welcome and its widget.");
        await context.CloseAsync();
    }

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

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    // Clicks something that loads another page, with the wait cut out of the clip.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);
    }

    private static ILocator RuleAdd(Scene scene) => scene.Page.Locator("button[data-bs-target='#modalConditions']").First;

    private static ILocator GroupAdd(Scene scene) => scene.Page.Locator("li.list-group-item button[data-bs-target='#modalConditions']").First;

    private static ILocator Filter(Scene scene) => scene.Page.Locator("#modalConditions #search-box").First;

    private static ILocator Card(Scene scene, string title)
        => scene.Page.Locator("#modalConditions .item:visible").Filter(new() { HasText = title }).First;

    private static async Task OpenDialogAsync(Scene scene, ILocator add)
    {
        await scene.ClickAsync(add, pauseAfter: 100);
        await scene.Page.Locator("#modalConditions.show").WaitForAsync();
        await scene.PauseAsync(250);
    }

    private static async Task AddCardAsync(Scene scene, string filter, string title)
    {
        await scene.TypeAsync(Filter(scene), filter, delay: 4);
        await Card(scene, title).WaitForAsync();
        await scene.PauseAsync(300);
        await NavAsync(scene, Card(scene, title).Locator("a.condition-thumbnail").First);
    }

    private static async Task CreateAsync(Scene scene)
    {
        await NavAsync(scene, scene.Page.Locator("button.create").First);
    }

    private static async Task OpenEditPageAsync(Scene scene)
    {
        await scene.OpenAsync(EditLayer);
        await scene.Page.GetByText("Edit Layer - Welcome").First.WaitForAsync();
    }

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            var design = scene.Page.Locator("button.nav-group-label:has-text('Design')").First;

            if (await design.GetAttributeAsync("aria-expanded") == "false")
            {
                await scene.ClickAsync(design, pauseAfter: 400);
            }

            var widgets = scene.Page.Locator("a[href$='/Admin/Layers']").First;
            await scene.MoveToAsync(widgets);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(widgets), leadMilliseconds: 150);
            await scene.Page.Locator("li.list-group-item:has(#Welcome)").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item:has(#Welcome)").First);
        });

        await scene.StepAsync("open-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("div.col-md-4 a[href*='Layers/Edit?name=Welcome']").First);
            await scene.HighlightAsync(scene.Page.GetByText("No conditions found").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task GroupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenEditPageAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("group-1", async () =>
        {
            await OpenDialogAsync(scene, RuleAdd(scene));
            await scene.HoldAsync(1.5);
            await scene.TypeAsync(Filter(scene), "any", delay: 40);
            await Card(scene, "Any condition").WaitForAsync();
            await scene.HighlightAsync(Card(scene, "Any condition"));
        });

        await scene.StepAsync("group-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, Card(scene, "Any condition").Locator("a.condition-thumbnail").First);
            await scene.TypeAsync(scene.Page.Locator("input[name='AnyConditionGroup.DisplayText']").First, "Home or About", delay: 40);
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(400);
            await CreateAsync(scene);
            await scene.Page.GetByText("Condition added successfully").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item:has-text('Home or About')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ConditionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenEditPageAsync(scene);
        await GroupAdd(scene).WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("cond-1", async () =>
        {
            await scene.HighlightAsync(GroupAdd(scene));
            await scene.HoldAsync(1);
            await scene.ClearHighlightsAsync();
            await OpenDialogAsync(scene, GroupAdd(scene));
            await AddCardAsync(scene, "homepage", "Homepage condition");
        });

        await scene.StepAsync("cond-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("div.form-check").First);
            await scene.HighlightAsync(scene.Page.Locator("div.form-check").Nth(1));
            await scene.HoldAsync(1);
            await scene.ClearHighlightsAsync();
            await CreateAsync(scene);
            await scene.Page.GetByText("Condition added successfully").First.WaitForAsync();
        });

        await scene.StepAsync("cond-3", async () =>
        {
            await OpenDialogAsync(scene, GroupAdd(scene));
            await AddCardAsync(scene, "url", "Url condition");
        });

        await scene.StepAsync("cond-4", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("select[name='UrlCondition.SelectedOperation']").First, "Starts with");
            await scene.TypeAsync(scene.Page.Locator("input[name='UrlCondition.Value']").First, "/about", delay: 50);
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(400);
            await CreateAsync(scene);
            await scene.Page.GetByText("Starts with /about").First.WaitForAsync();
        });

        await scene.StepAsync("cond-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item:has-text('Home or About')").First);
            await scene.HoldAsync(0.9);
            await scene.ClearHighlightsAsync();
            var row = scene.Page.Locator("li.list-group-item:has-text('Starts with /about')").Last;
            await scene.HighlightAsync(row.Locator("a:has-text('Edit'), button:has-text('Edit')").First);
            await scene.HoldAsync(0.9);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("a:has-text('Delete'), button:has-text('Delete')").First);
            await scene.HoldAsync(0.9);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenEditPageAsync(scene);
        await scene.StartCaptureAsync();

        var widget = scene.Page.Locator("div.widget-container").Filter(new() { HasText = "Welcome note" }).First;

        await scene.StepAsync("site-1", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/");
                await widget.WaitForAsync();
            }, leadMilliseconds: 100);
            await scene.HighlightAsync(widget);
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var about = scene.Page.Locator("#mainNav a").Filter(new() { HasText = "About" }).First;
            await scene.MoveToAsync(about);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(about), leadMilliseconds: 150);
            await widget.WaitForAsync();
            await scene.HighlightAsync(widget);
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/blog/post-1");
                await scene.Page.Locator("h1").First.WaitForAsync();
            }, leadMilliseconds: 100);
            await scene.HighlightAsync(scene.Page.Locator("h1").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task VisitorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenEditPageAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("visitor-1", async () =>
        {
            await OpenDialogAsync(scene, RuleAdd(scene));
            await AddCardAsync(scene, "authenticated", "Is authenticated condition");
        });

        await scene.StepAsync("visitor-2", async () =>
        {
            await CreateAsync(scene);
            await scene.Page.GetByText("Condition added successfully").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item:has-text('Is authenticated')").Last);
        });

        await scene.StepAsync("visitor-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, scene.Page.Locator("li.list-group-item:has-text('Home or About')").First, 4);
            await ShowAsync(scene, scene.Page.Locator("li.list-group-item:has-text('Is authenticated')").Last, 4);
        });

        await scene.ClearHighlightsAsync();
    }
}

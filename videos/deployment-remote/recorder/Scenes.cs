using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Remote Deployment". Two tenants of one host: the source (Default tenant, "Orchard Core") and the
/// destination (Staging tenant, "Staging Site", URL prefix staging) with a sign-in of its own, state-staging.json.
/// PrepareAsync seeds the source off camera: two published pages and the plan Publish Pages.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static string StagingStatePath => Path.Combine(Root, "state-staging.json");

    private static string ApiKey { get; } = Environment.GetEnvironmentVariable("VIDEO_REMOTE_API_KEY") ?? "kd83-Vq2xN7pLw5Rb0TzYm9Ce4HsUa6J";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-client", ClientAsync),
        ("03-instance", InstanceAsync),
        ("04-send", SendAsync),
        ("05-check", CheckAsync),
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

    private static Task<Scene> SourceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static Task<Scene> DestinationAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl + "/staging", StagingStatePath, Path.GetDirectoryName(directory), durations);

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);

        async Task GoAsync(string path) => await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.NetworkIdle });

        foreach (var title in new[] { "Spring Planting Workshop", "Seed Swap Weekend" })
        {
            await GoAsync("/Admin/Contents/ContentTypes/Page/Create");
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator("button.publish").ClickAsync();
            await page.WaitForURLAsync(url => !url.Contains("/Create"));
            await Task.Delay(1500);
        }

        await GoAsync("/Admin/DeploymentPlan/Index");
        await page.Locator("a.btn:has-text('Add Deployment Plan')").First.ClickAsync();
        await page.Locator("input[name='Name']").FillAsync("Publish Pages");
        await page.Locator("button.create").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("button:has-text('Add Step')").First.ClickAsync();
        await page.Locator("#modalSteps.show").WaitForAsync();
        await page.Locator("#modalSteps #search-box").FillAsync("Content");
        await page.Locator("#modalSteps .deployment-step-item").Filter(new() { Has = page.Locator("h4.card-title", new() { HasTextRegex = new("^\\s*Content\\s*$") }) }).First.Locator("a:has-text('Add')").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("label:has-text('Page')").Filter(new() { HasTextRegex = new("^\\s*Page\\s*$") }).First.ClickAsync();
        await page.Locator("button.create").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(ILocator field, string text, int delay = 3)
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

    private static async Task OpenToolsAsync(Scene scene)
    {
        await OpenGroupAsync(scene, "Tools");
        await OpenGroupAsync(scene, "Deployments");
    }

    private static async Task GoAsync(Scene scene, string link)
        => await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new($"^\\s*{link}\\s*$") }).First);

    private static async Task ClientAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await DestinationAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("client-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".navbar-brand, a:has-text('Staging Site')").First);
            await scene.PauseAsync(500);
            await scene.ClearHighlightsAsync();
            await OpenToolsAsync(scene);
            await GoAsync(scene, "Remote Clients");
        });

        await scene.StepAsync("client-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn:has-text('Add Remote Client')").First);
        });

        await scene.StepAsync("client-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ClientName"), "Main Site", 30);
        });

        await scene.StepAsync("client-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ApiKey"), ApiKey, 3);
            await scene.HighlightAsync(scene.Page.Locator("#ApiKey"));
        });

        await scene.StepAsync("client-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.create, button:has-text('Create')").First);
            await scene.Page.Locator("a", new() { HasTextString = "Main Site" }).First.WaitForAsync();
        });
    }

    private static async Task InstanceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await SourceAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await OpenToolsAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("inst-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".navbar-brand, a:has-text('Orchard Core')").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "Remote Instances");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn:has-text('Add Remote Instance')").First);
        });

        await scene.StepAsync("inst-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Staging", 30);
        });

        await scene.StepAsync("inst-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Url"), BaseUrl + "/staging/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import", 8);
            await scene.HighlightAsync(scene.Page.Locator("#Url"));
        });

        await scene.StepAsync("inst-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#ClientName"), "Main Site", 8);
            await scene.TypeAsync(scene.Page.Locator("#ApiKey"), ApiKey, 3);
        });

        await scene.StepAsync("inst-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.create, button:has-text('Create')").First);
            await scene.Page.Locator("a", new() { HasTextString = "Staging" }).First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".remote-instance-url").First);
        });
    }

    private static async Task SendAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await SourceAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await OpenToolsAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("send-1", async () =>
        {
            await GoAsync(scene, "Plans");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item:has-text('Publish Pages') a:has-text('Manage Steps')").First);
            await scene.Page.Locator("ul#stepOrder li").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("ul#stepOrder li").First);
        });

        await scene.StepAsync("send-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.btn-success:has-text('Execute'), button:has-text('Execute')").First, pauseAfter: 300);
            await scene.Page.Locator("#modalTargets.show").WaitForAsync();
            var cards = scene.Page.Locator("#modalTargets .card");
            await scene.HighlightAsync(cards.Nth(0));
            await scene.HighlightAsync(cards.Nth(1));
        });

        await scene.StepAsync("send-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var select = scene.Page.Locator("#modalTargets .card").Filter(new() { Has = scene.Page.Locator("h4", new() { HasTextRegex = new("^\\s*Staging\\s*$") }) }).First.Locator("a:has-text('Select')").First;
            await scene.MoveToAsync(select);
            await scene.HighlightAsync(select);
        });

        await scene.StepAsync("send-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var select = scene.Page.Locator("#modalTargets .card").Filter(new() { Has = scene.Page.Locator("h4", new() { HasTextRegex = new("^\\s*Staging\\s*$") }) }).First.Locator("a:has-text('Select')").First;
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await select.ClickAsync();
                await scene.Page.GetByText("Deployment executed successfully.").First.WaitForAsync(new() { Timeout = 60000 });
            }));
        });
    }

    private static async Task CheckAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await DestinationAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("check-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".navbar-brand, a:has-text('Staging Site')").First);
            await scene.PauseAsync(700);
            await scene.ClearHighlightsAsync();
            await OpenGroupAsync(scene, "Content");
            await GoAsync(scene, "Content Items");
            await scene.Page.Locator("a", new() { HasTextString = "Spring Planting Workshop" }).First.WaitForAsync();
        });

        await scene.StepAsync("check-2", async () =>
        {
            foreach (var title in new[] { "Spring Planting Workshop", "Seed Swap Weekend" })
            {
                await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasText = title }).First);
            }
        });
    }
}

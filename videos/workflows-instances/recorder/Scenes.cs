using System.Net;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Workflow Instances", recorded in order on a Blog site seeded with two workflows and seven instances.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-list", ListAsync),
        ("03-waiting", WaitingAsync),
        ("04-faulted", FaultedAsync),
        ("05-bulk", BulkAsync),
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

    // A stand-in for the stock service of the "Order follow-up" workflow: answers 200 on 127.0.0.1:5099 while it runs.
    private sealed class StandIn : IDisposable
    {
        private readonly HttpListener _listener = new();

        public StandIn()
        {
            _listener.Prefixes.Add("http://127.0.0.1:5099/");
            _listener.Start();
            _ = Task.Run(async () =>
            {
                while (_listener.IsListening)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        var bytes = System.Text.Encoding.UTF8.GetBytes("12 units");
                        context.Response.StatusCode = 200;
                        await context.Response.OutputStream.WriteAsync(bytes);
                        context.Response.Close();
                    }
                    catch (Exception) when (!_listener.IsListening)
                    {
                    }
                }
            });
        }

        public void Dispose() => _listener.Close();
    }

    /// <summary>Enables the features, imports the two workflows and starts the seven instances, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var feature in new[] { "OrchardCore_Workflows", "OrchardCore_Workflows_Http" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            await page.Locator($"#btn-enable-{feature}").ClickAsync();
            await WaitTextAsync(page, "was enabled");
        }

        var json = await File.ReadAllTextAsync(Path.Combine(Root, "seed.json"));
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!");

        async Task RunAsync(string workflow, string order, string status)
        {
            await page.GotoAsync(BaseUrl + "/Admin/Workflows/Types");
            await page.GetByRole(AriaRole.Link, new() { Name = workflow }).First.ClickAsync();
            await page.Locator("[data-cy='toolbar-run']").ClickAsync();
            await page.Locator("[id^='wfd-run-'][id$='-orderNumber']").FillAsync(order);
            await page.Locator("[data-cy='run-start']").ClickAsync();
            await page.WaitForFunctionAsync("s => document.querySelector('.modal.show')?.innerText.includes(s)", status);
            await page.Locator("[data-cy='run-close']").ClickAsync();
        }

        // Four runs with the stock service up, three with it down.
        using (new StandIn())
        {
            await RunAsync("Order follow-up", "A-1001", "Finished");
            await RunAsync("Order follow-up", "A-1002", "Finished");
            await RunAsync("Payment follow-up", "A-1003", "Halted");
            await RunAsync("Payment follow-up", "A-1004", "Halted");
        }

        await RunAsync("Order follow-up", "A-1005", "Faulted");
        await RunAsync("Order follow-up", "A-1006", "Faulted");
        await RunAsync("Order follow-up", "A-1007", "Faulted");

        await context.CloseAsync();
        Console.WriteLine("Workflows imported.");
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

    private static ILocator Group(IPage page, string key) => page.Locator($"button.nav-group-label.oc-navigation-admin-{key}");

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1500)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.ClickAsync(Group(page, "design"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Workflows/Types'] >> visible=true").First);
            await ShowAsync(scene, page.Locator("a.badge:has-text('Instances')").First, 2000);
            await scene.HighlightAsync(page.Locator("[data-cy='all-instances']"));
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("[data-cy='all-instances']"));

            foreach (var selector in new[] { "[data-cy='instance-workflow']", "a[href*='/Workflow/Details/']", "[data-cy='instance-status']", "[data-cy='instance-version']" })
            {
                await ShowAsync(scene, page.Locator(selector).First, 1700);
            }

            await scene.HighlightAsync(page.Locator(".metadata .badge").First);
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, page.Locator("[data-cy='instance-counts']"), 3000);
            await GoAsync(scene, page.Locator("[data-cy='instance-count-Faulted']"));
            await scene.HighlightAsync(page.Locator(".btn-group.filter").First);
        });
    }

    private static async Task WaitingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Instances");
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            await GoAsync(scene, page.Locator("[data-cy='instance-count-Halted']"));
            await GoAsync(scene, page.Locator("a[href*='/Workflow/Details/']").First);
            await scene.HighlightAsync(page.Locator(".workflow-details").First);
        });

        await scene.StepAsync("open-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(page.Locator("[data-cy='instance-version']").First);
        });

        await scene.StepAsync("open-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, page.Locator("[data-cy='blocking-badge']").First, 3000);
            await scene.HighlightAsync(page.Locator("[data-cy='viewer-legend']").First);
        });

        await scene.StepAsync("open-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, page.Locator("[data-cy='panel-tab-variables']"), 2000);
            await scene.HighlightAsync(page.Locator("tr:has-text('orderNumber'), li:has-text('orderNumber')").First);
        });

        await scene.StepAsync("open-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='panel-tab-journal']"), pauseAfter: 400);
            await scene.ClickAsync(page.Locator("[data-cy='journal-record-2']"), pauseAfter: 600);
            await scene.ClickAsync(page.Locator("[data-cy='activity-tab-details']"), pauseAfter: 400);
        });
    }

    private static async Task FaultedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        using var standIn = new StandIn();
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Instances");
        await scene.StartCaptureAsync();

        await scene.StepAsync("fault-1", async () =>
        {
            await GoAsync(scene, page.Locator("[data-cy='instance-count-Faulted']"));
            await GoAsync(scene, page.Locator("a[href*='/Workflow/Details/']").First);
            await scene.HighlightAsync(page.Locator(".ocat-wrapper").Filter(new() { Has = page.Locator("label:text-is('Fault Message')") }).First);
        });

        await scene.StepAsync("fault-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(page.Locator("[data-cy='activity-checkstock']"));
        });

        await scene.StepAsync("fault-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='panel-tab-journal']"), pauseAfter: 400);
            await scene.HighlightAsync(page.Locator("[data-cy='journal-record-2'] [data-cy='journal-error']"));
        });

        await scene.StepAsync("fault-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='journal-record-2']"), pauseAfter: 600);
        });

        await scene.StepAsync("fault-5", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='retry-button']"), pauseAfter: 300);
            await page.Locator("#modalOkButton").WaitForAsync();
        });

        await scene.StepAsync("fault-6", async () =>
        {
            await scene.ClickAsync(page.Locator("#modalOkButton"), pauseAfter: 200);
            await page.Locator("text=The instance ran again: Finished.").WaitForAsync();
            await scene.PauseAsync(4000);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.ReloadAsync()), leadMilliseconds: 250);
            await scene.ScrollAsync(0, 600);
            await scene.HighlightAsync(page.Locator("a.btn-warning:has-text('Restart')").First);
        });
    }

    private static async Task BulkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        using var standIn = new StandIn();
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Instances");
        await scene.StartCaptureAsync();

        await scene.StepAsync("bulk-1", async () =>
        {
            await GoAsync(scene, page.Locator("[data-cy='instance-count-Faulted']"));
            await scene.ClickAsync(page.Locator("input[name='itemIds']").Nth(0), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("input[name='itemIds']").Nth(1), pauseAfter: 300);
            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
        });

        await scene.StepAsync("bulk-2", async () =>
        {
            await scene.ClickAsync(page.Locator("a.dropdown-item[data-action='Retry']"), pauseAfter: 300);
            await page.Locator("#modalOkButton").WaitForAsync();
            await GoAsync(scene, page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("bulk-3", async () =>
        {
            await GoAsync(scene, page.Locator("[data-cy='instance-count-Halted']"));
            await scene.ClickAsync(page.Locator("#select-all"), pauseAfter: 250);
            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 250);
            await scene.ClickAsync(page.Locator("a.dropdown-item[data-action='Cancel']"), pauseAfter: 250);
            await page.Locator("#modalOkButton").WaitForAsync();
            await GoAsync(scene, page.Locator("#modalOkButton"));
        });
    }
}

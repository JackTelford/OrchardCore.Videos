using System.Net;
using System.Text;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Workflows: HTTP activities", recorded on a Blog site seeded with the "Stock check" workflow.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string WorkflowName = "Stock check";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-activities", ActivitiesAsync),
        ("03-receive", ReceiveAsync),
        ("05-call", CallAsync),
        ("06-run", RunAsync),
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

    // The stock API: answers 200 with the product on /A-100.json, and 404 for anything else, on 127.0.0.1:5080.
    private sealed class StockApi : IDisposable
    {
        private readonly HttpListener _listener = new();

        public StockApi()
        {
            _listener.Prefixes.Add("http://127.0.0.1:5080/");
            _listener.Prefixes.Add("http://localhost:5080/");
            _listener.Start();
            _ = Task.Run(async () =>
            {
                while (_listener.IsListening)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        var found = context.Request.Url.AbsolutePath == "/A-100.json";
                        var bytes = Encoding.UTF8.GetBytes(found ? "{ \"sku\": \"A-100\", \"name\": \"Garden trowel\", \"inStock\": 12 }" : "Not found");
                        context.Response.StatusCode = found ? 200 : 404;
                        context.Response.ContentType = found ? "application/json" : "text/plain";
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

    /// <summary>Enables the features and imports the Stock check workflow, outside the capture.</summary>
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

        var json = Seed.Json;
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!");

        foreach (var path in new[] { "/Admin/Workflows/Types" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Stock check imported.");
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task WaitTextAsync(IPage page, string text, int timeout = 60_000)
        => await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = timeout });

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1500)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    // Opens the designer of Stock check, outside the capture. The activities pane is hidden unless the clip shows it.
    private static async Task OpenDesignerAsync(Scene scene, bool hidePane = true)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await page.GetByRole(AriaRole.Link, new() { Name = WorkflowName }).First.ClickAsync();
        await page.Locator("[data-cy='designer-toolbar']").WaitForAsync();
        await page.Locator(".wfd-node").Nth(4).WaitForAsync();
        await page.Locator("button.leftbar-compactor").ClickAsync();

        if (hidePane)
        {
            await page.Locator("[data-cy='toolbox-collapse']").ClickAsync();
        }

        await Task.Delay(800);
    }

    private static ILocator Node(IPage page, string id) => page.Locator($"[data-cy='activity-{id}']");

    // Opens the settings of an activity: a double-click, or its settings button when the double-click is missed.
    private static async Task OpenNodeAsync(Scene scene, string id)
    {
        var page = scene.Page;
        await scene.DoubleClickAsync(Node(page, id), pauseAfter: 500);

        try
        {
            await page.Locator("[data-cy='activity-panel-close']").WaitForAsync(new() { Timeout = 2500 });
        }
        catch (TimeoutException)
        {
            await scene.ClickAsync(Node(page, id).Locator("[data-cy='node-edit']"), pauseAfter: 500);
            await page.Locator("[data-cy='activity-panel-close']").WaitForAsync();
        }
    }

    private static async Task ClosePanelAsync(Scene scene)
    {
        await scene.ClickAsync(scene.Page.Locator("[data-cy='activity-panel-close']"), pauseAfter: 300);
    }

    private static async Task ActivitiesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene, hidePane: false);
        await scene.StartCaptureAsync();

        await scene.StepAsync("acts-1", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbox-search']"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("[data-cy='toolbox-search']"), "HTTP", delay: 60);
            await scene.HighlightAsync(page.Locator("[data-cy='toolbox-category-HTTP']"));
        });

        await scene.StepAsync("acts-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("label[for='wfd-kind-events']"), pauseAfter: 400);

            foreach (var card in new[] { "HttpRequestEvent", "HttpRequestFilterEvent", "SignalEvent" })
            {
                await ShowAsync(scene, page.Locator($"[data-cy='toolbox-activity-{card}']"), 1600);
            }
        });

        await scene.StepAsync("acts-3", async () =>
        {
            await scene.ClickAsync(page.Locator("label[for='wfd-kind-tasks']"), pauseAfter: 400);

            foreach (var card in new[] { "HttpRedirectTask", "HttpRequestTask", "HttpResponseTask" })
            {
                await ShowAsync(scene, page.Locator($"[data-cy='toolbox-activity-{card}']"), 1400);
            }
        });
    }

    private static async Task ReceiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("recv-1", async () =>
        {
            foreach (var id in new[] { "stockrequested", "readproduct", "callstockapi" })
            {
                await ShowAsync(scene, Node(page, id), 2800);
            }

            await scene.HighlightAsync(Node(page, "replyfound"));
            await scene.HighlightAsync(Node(page, "replynotfound"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("recv-2", async () =>
        {
            await OpenNodeAsync(scene, "stockrequested");
            await page.WaitForFunctionAsync("() => (document.querySelector('[data-workflow-url-text]')?.value || '').length > 10");
            await ShowAsync(scene, page.GetByLabel("HTTP Method"), 1800);
            await ShowAsync(scene, page.Locator("[data-workflow-url-text]").First, 2200);
            await ShowAsync(scene, page.Locator("#generate-url-button"), 1800);
            await scene.HighlightAsync(page.GetByText("Anyone that invokes this URL will trigger this workflow. Keep it in a safe place.").First);
        });

        await scene.StepAsync("recv-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.GetByLabel("Validate Antiforgery Token").ScrollToMiddleAsync();
            await ShowAsync(scene, page.GetByLabel("Validate Antiforgery Token"), 2500);
            await ShowAsync(scene, page.GetByLabel("Token Life Span"), 2500);
            await scene.HighlightAsync(page.GetByText("Number of days to expire the token. 0 for the token to never expire").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await ClosePanelAsync(scene);
        });
    }

    private static async Task CallAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("call-1", async () =>
        {
            await OpenNodeAsync(scene, "callstockapi");
            await ShowAsync(scene, page.GetByLabel("URL", new() { Exact = true }).First, 2500);
            await ShowAsync(scene, page.GetByLabel("HTTP Method").First, 1800);
            await ShowAsync(scene, page.GetByLabel("Content Type").First, 1800);
            await page.GetByLabel("Headers").First.ScrollToMiddleAsync();
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("call-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var codes = page.GetByLabel("HTTP Response Codes");
            await codes.ScrollToMiddleAsync();
            await ShowAsync(scene, codes, 3000);
            await ClosePanelAsync(scene);

            foreach (var outcome in new[] { "200", "404", "UnhandledHttpStatus" })
            {
                await ShowAsync(scene, Node(page, "callstockapi").Locator($"[data-outcome='{outcome}']"), 1700);
            }
        });

        await scene.StepAsync("call-3", async () =>
        {
            await OpenNodeAsync(scene, "callstockapi");
            await scene.ClickAsync(page.Locator("[data-cy='activity-tab-outputs']"), pauseAfter: 400);

            foreach (var output in new[] { "Body", "StatusCode", "Response" })
            {
                await ShowAsync(scene, page.Locator($"[data-cy='output-{output}']"), 2500);
            }

            await ClosePanelAsync(scene);
        });

        await scene.StepAsync("call-4", async () =>
        {
            await OpenNodeAsync(scene, "replyfound");
            await ShowAsync(scene, page.GetByLabel("HTTP Status Code").First, 1500);
            await ShowAsync(scene, page.GetByLabel("Content Type").First, 1500);
            await ShowAsync(scene, page.GetByLabel("Content", new() { Exact = true }).First, 1800);
            await ClosePanelAsync(scene);
            await OpenNodeAsync(scene, "replynotfound");
            await ShowAsync(scene, page.GetByLabel("HTTP Status Code").First, 1500);
            await ShowAsync(scene, page.GetByLabel("Content Type").First, 1500);
            await scene.HighlightAsync(page.GetByLabel("Content", new() { Exact = true }).First);
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        using var stockApi = new StockApi();
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-run']"), pauseAfter: 300);
            await page.Locator(".modal.show").WaitForAsync();
            await scene.PauseAsync(2500);
            await scene.ClickAsync(page.Locator("[data-cy='run-body']"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("[data-cy='run-body']"), "{ \"sku\": \"A-100\" }", delay: 40);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='run-start']"), pauseAfter: 200);
            await page.WaitForFunctionAsync("() => document.querySelector(\"[data-cy='run-response-status']\")?.innerText.includes('200')");
            await ShowAsync(scene, page.Locator("[data-cy='run-response-status']"), 2000);
            await scene.HighlightAsync(page.Locator("[data-cy='run-open-instance']"));
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.Locator("[data-cy='run-body']").FillAsync(string.Empty);
            await scene.TypeAsync(page.Locator("[data-cy='run-body']"), "{ \"sku\": \"Z-999\" }", delay: 30);
            await scene.ClickAsync(page.Locator("[data-cy='run-start']"), pauseAfter: 200);
            await page.WaitForFunctionAsync("() => document.querySelector(\"[data-cy='run-response-status']\")?.innerText.includes('404')");
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

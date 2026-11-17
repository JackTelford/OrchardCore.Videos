using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Write a Workflow Activity": 06-designer adds the Notify Task to the draft of the workflow "Notify
/// editors" and sets its settings. PrepareAsync (outside the capture) enables Workflows and creates the workflow with
/// two JSON Imports: a Feature step, then a WorkflowType step with one Content Published Event.
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
        ("06-designer", DesignerAsync),
    ];

    private const string EnableWorkflows = """
        { "steps": [ { "name": "feature", "enable": [ "OrchardCore.Workflows" ], "disable": [] } ] }
        """;

    private const string NotifyEditors = """
        {
          "steps": [
            {
              "name": "WorkflowType",
              "data": [
                {
                  "WorkflowTypeId": "4fa1b2c3d4e5f60718293a4b5c6d7e8f",
                  "Name": "Notify editors",
                  "IsEnabled": true,
                  "IsSingleInstance": false,
                  "DeleteFinishedWorkflows": false,
                  "Activities": [
                    {
                      "ActivityId": "9a8b7c6d5e4f30211203f4e5d6c7b8a9",
                      "Name": "ContentPublishedEvent",
                      "X": 40,
                      "Y": 60,
                      "IsStart": true,
                      "Properties": { "ContentTypeFilter": [] }
                    }
                  ],
                  "Transitions": []
                }
              ]
            }
          ]
        }
        """;

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

    // The page's code editor replaces the value of the textarea on submit, so the form is posted by script.
    private static async Task<string> ImportAsync(IPage page, string json)
    {
        await page.GotoAsync($"{BaseUrl}/Admin/DeploymentPlan/Import/Json", new() { WaitUntil = WaitUntilState.NetworkIdle });
        return await page.EvaluateAsync<string>("""
            async json => {
              const form = document.querySelector('form:has(#Json)');
              document.getElementById('Json').value = json;
              const response = await fetch(form.action, { method: 'POST', body: new FormData(form) });
              return (await response.text()).includes('Recipe imported successfully') ? 'ok' : 'failed';
            }
            """, json);
    }

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        Console.WriteLine($"Features: {await ImportAsync(page, EnableWorkflows)}");
        await Task.Delay(8000);
        await page.GotoAsync($"{BaseUrl}/Admin/Workflows/Types", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine($"Workflow: {await ImportAsync(page, NotifyEditors)}");
        await page.GotoAsync($"{BaseUrl}/Admin/Workflows/Types", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.GetByText("Notify editors").First.WaitForAsync();
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 8 });
    }

    private static async Task DesignerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("designer-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a", new() { HasTextRegex = new(@"^\s*Notify editors\s*$") }).First);
            await page.Locator("[data-cy='designer-canvas'] .wfd-node", new() { HasText = "Content Published Event" }).First.WaitForAsync();
        });

        await scene.StepAsync("designer-2", async () =>
        {
            await FastAsync(page.Locator("[data-cy='toolbox-search']"), "notify");
            var card = page.Locator("[data-cy='toolbox-activity-NotifyTask']").First;
            await card.WaitForAsync();
            await scene.MoveToAsync(card);
            await scene.HighlightAsync(card, "Thumbnail view");
        });

        await scene.StepAsync("designer-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var port = page.Locator(".wfd-node", new() { HasText = "Content Published Event" }).Locator("[data-outcome='Done']").First;
            await scene.ClickAsync(port);
            await FastAsync(page.Locator("[data-cy='quick-add-search']"), "notify");
            await scene.ClickAsync(page.Locator("[data-cy='quick-add-NotifyTask']").First);
            await page.Locator(".wfd-node", new() { HasText = "Notify Task" }).First.WaitForAsync();
        });

        await scene.StepAsync("designer-4", async () =>
        {
            var node = page.Locator(".wfd-node", new() { HasText = "Notify Task" }).First;
            var body = node.Locator(".wfd-node-body").First;
            await scene.MoveToAsync(body);
            await scene.HighlightAsync(body, "Design view");
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(node.Locator("[data-cy='issue-badge']").First);
        });

        await scene.StepAsync("designer-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var form = page.Locator("[data-cy='panel-activity-form']");
            await form.GetByText("Notification Type").First.WaitForAsync();

            foreach (var row in new[]
            {
                form.Locator(".ocat-wrapper:has(label:has-text('Title'))").First,
                form.Locator(".ocat-wrapper:has(label:has-text('Notification Type'))").First,
                form.Locator(".ocat-wrapper:has(label:has-text('Message'))").First,
                form.Locator(".ocat-wrapper:has(h5:has-text('When it fails'))").First,
            })
            {
                await scene.HighlightAsync(row);
                await scene.PauseAsync(1200);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("designer-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var form = page.Locator("[data-cy='panel-activity-form']");
            await scene.SelectAsync(form.Locator("select[name$='NotificationType']").First, "Information");
            await Task.Delay(1200);
            await FastAsync(form.Locator("input[name$='Message']").First, "A page was published.");
            await scene.ClickAsync(form.GetByText("The message to show").First);
            await form.Locator("input[name$='Message']").First.ScrollIntoViewIfNeededAsync();
            var body = page.Locator(".wfd-node", new() { HasText = "Notify Task" }).First.Locator(".wfd-node-body").First;
            await body.GetByText("Information Notification").First.WaitForAsync();
            await body.GetByText("A page was published.").First.WaitForAsync();
            await scene.HighlightAsync(body);
        });
    }
}

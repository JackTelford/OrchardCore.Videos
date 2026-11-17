using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the data-pipelines-schedule-history video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-run", RunAsync),
        ("03-schedule", ScheduleAsync),
        ("04-list", ListAsync),
        ("04-details", DetailsAsync),
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

    // Moves to a link or button, clicks it and cuts the page load out of the video.
    private static async Task GoAsync(Scene scene, ILocator link, ILocator ready = null)
    {
        await scene.MoveToAsync(link);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(async () => await link.ClickAsync());

            if (ready is not null)
            {
                await ready.WaitForAsync(new() { Timeout = 120000, State = WaitForSelectorState.Visible });
            }
        }, 450);
    }

    private static ILocator Cy(Scene scene, string name) => scene.Page.Locator($"[data-cy='{name}']");

    private static async Task RingAsync(Scene scene, ILocator target, double seconds)
    {
        await target.ScrollIntoViewIfNeededAsync();
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator PipelineRow(IPage page, string name) => page.Locator("li[data-pipeline-id]").Filter(new() { HasText = name }).First;

    // ---------- seed (outside the capture) ----------

    private const string FeaturesRecipe = """
{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.DataPipelines", "OrchardCore.Workflows", "OrchardCore.Workflows.Timers" ] } ] }
""";

    private static string PipelineJson(string name, string description, string fileName, bool csv)
    {
        var type = csv ? "ReadMediaFile" : "DataSource";
        var settings = csv
            ? "\"ReadMediaFileStepSettings\": {\"Path\": \"imports/subscribers.csv\"}"
            : "\"DataSourceStepSettings\": {\"Source\": \"Contents\", \"DataSet\": \"BlogPost\"}";

        return "{ \"Name\": \"" + name + "\", \"Description\": \"" + description + "\", \"IsEnabled\": true, \"Definition\": { \"Steps\": ["
            + "{\"StepId\": \"source\", \"Type\": \"" + type + "\", \"X\": 180, \"Y\": 320, \"Properties\": {" + settings + "}},"
            + "{\"StepId\": \"file\", \"Type\": \"CreateFile\", \"X\": 480, \"Y\": 320, \"Properties\": {\"CreateFileStepSettings\": {\"Format\": \"csv\", \"FileName\": \"" + fileName + "\"}}},"
            + "{\"StepId\": \"save\", \"Type\": \"SaveToMedia\", \"X\": 780, \"Y\": 320, \"Properties\": {\"SaveToMediaStepSettings\": {\"Folder\": \"exports\", \"Overwrite\": true}}}"
            + "], \"Connections\": ["
            + "{\"SourceStepId\": \"source\", \"SourcePort\": \"Output\", \"TargetStepId\": \"file\", \"TargetPort\": \"Input\"},"
            + "{\"SourceStepId\": \"file\", \"SourcePort\": \"Output\", \"TargetStepId\": \"save\", \"TargetPort\": \"Input\"}"
            + "] } }";
    }

    private static string PipelinesRecipe() => "{ \"steps\": [ { \"name\": \"DataPipelines\", \"Pipelines\": [ "
        + PipelineJson("Blog posts export", "Exports the blog posts to a file of the media library.", "blog-posts", false) + ", "
        + PipelineJson("Subscriber list check", "Reads the subscriber file of the media library and saves a checked copy.", "subscribers-checked", true) + ", "
        + PipelineJson("Newsletter export", "Exports the blog posts for the newsletter.", "newsletter", false)
        + " ] } ] }";

    private static async Task ImportAsync(IPage page, string recipe)
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            try
            {
                await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
                await page.Locator(".CodeMirror").First.WaitForAsync(new() { Timeout = 5000 });
                break;
            }
            catch (Exception)
            {
                await Task.Delay(3000);
            }
        }

        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", recipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(4000);
    }

    private static async Task OpenDesignerAsync(IPage page, string pipeline)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DataPipelines");
        var edit = PipelineRow(page, pipeline).Locator("a.btn-primary").First;
        await page.GotoAsync(BaseUrl + await edit.GetAttributeAsync("href"));
        await page.Locator("[data-cy='data-pipeline-designer']").WaitForAsync(new() { Timeout = 60000 });
        await page.Locator("[data-node-id]").First.WaitForAsync(new() { Timeout = 60000 });
        await Task.Delay(1500);
    }

    private static async Task PublishPipelineAsync(IPage page, string pipeline)
    {
        await OpenDesignerAsync(page, pipeline);
        await page.Locator("[data-cy='toolbar-publish']").ClickAsync();
        var anyway = page.GetByRole(AriaRole.Button, new() { Name = "Publish anyway" });

        try
        {
            await anyway.WaitForAsync(new() { Timeout = 3000 });
            await anyway.ClickAsync();
        }
        catch (Exception)
        {
        }

        await page.Locator("[data-cy='published-version']").WaitForAsync(new() { Timeout = 60000 });
    }

    // Starts a run from the Actions menu of the pipeline and waits until it ends.
    private static async Task RunNowAsync(IPage page, string pipeline, string expected)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DataPipelines");
        var row = PipelineRow(page, pipeline);
        await row.Locator("button.actions").ClickAsync();
        await row.Locator("a.dropdown-item:has-text('Run now')").ClickAsync();

        try
        {
            var run = page.Locator(".modal.show .btn-primary").First;
            await run.WaitForAsync(new() { Timeout = 2500 });
            await run.ClickAsync();
        }
        catch (Exception)
        {
        }

        await page.WaitForURLAsync("**/Admin/DataPipelines/Run/**", new() { Timeout = 60000 });

        for (var i = 0; i < 40; i++)
        {
            await Task.Delay(1500);
            await page.ReloadAsync();

            if (await page.Locator($".badge:has-text('{expected}')").CountAsync() > 0)
            {
                return;
            }
        }

        throw new InvalidOperationException($"The run of {pipeline} did not end as {expected}.");
    }

    // The workflow of sched-1 to sched-6, without capture: used to seed runs started by a workflow.
    private static async Task BuildWorkflowAsync(IPage page, string name, string cron, bool siteTimeZone)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Workflows/Types");
        await page.Locator("a.create").First.ClickAsync();
        await page.Locator("input[name='Name']").FillAsync(name);
        await page.Locator("button.save").First.ClickAsync();
        await page.Locator("[data-cy='start-picker']").WaitForAsync(new() { Timeout = 60000 });
        await page.Locator("[data-cy='start-picker-TimerEvent']").ClickAsync();
        var box = page.GetByLabel("CRON Expression");
        await box.WaitForAsync(new() { Timeout = 30000 });
        await box.ClickAsync();
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.TypeAsync(cron);
        await page.Keyboard.PressAsync("Tab");
        await page.GetByText("Saved!").First.WaitForAsync(new() { Timeout = 30000 });

        if (siteTimeZone)
        {
            await page.GetByText("Use site time zone").First.ClickAsync();
            await Task.Delay(1500);
        }

        var port = page.Locator("[aria-label^='Outcome Done']").First;
        await port.HoverAsync();
        await port.ClickAsync();
        await page.Locator("[data-cy='quick-add-search']").FillAsync("Run Data");
        await page.Locator("[data-cy='quick-add-RunDataPipelineTask']").ClickAsync();
        await page.Locator("#RunDataPipelineTask_PipelineId").WaitForAsync(new() { Timeout = 30000 });
        await page.Locator("#RunDataPipelineTask_PipelineId").SelectOptionAsync(new SelectOptionValue { Label = "Blog posts export" });
        await page.Keyboard.PressAsync("Tab");
        await page.GetByText("Saved!").First.WaitForAsync(new() { Timeout = 30000 });
        await page.GetByLabel("Skip when a run is already queued or running").ScrollIntoViewIfNeededAsync();
        await page.GetByText("Skip when a run is already queued or running").First.ClickAsync();
        await Task.Delay(1500);
        await page.Locator("[data-cy='toolbar-publish']").ClickAsync();

        try
        {
            var anyway = page.GetByRole(AriaRole.Button, new() { Name = "Publish anyway" });
            await anyway.WaitForAsync(new() { Timeout = 4000 });
            await anyway.ClickAsync();
        }
        catch (Exception)
        {
        }

        await Task.Delay(3000);
    }

    // Seeds the pipelines, the runs and the history (outside the capture). The CSV file is in the media folder beforehand,
    // and SEED_FILE (its path) is removed before the failing run.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

        await ImportAsync(page, FeaturesRecipe);
        await Task.Delay(10000);
        await ImportAsync(page, PipelinesRecipe());
        Console.WriteLine("Imported the pipelines.");

        await PublishPipelineAsync(page, "Blog posts export");
        await PublishPipelineAsync(page, "Subscriber list check");
        Console.WriteLine("Published two pipelines.");

        await RunNowAsync(page, "Blog posts export", "Succeeded");
        await RunNowAsync(page, "Blog posts export", "Succeeded");
        await RunNowAsync(page, "Subscriber list check", "Succeeded");
        Console.WriteLine("Seeded four runs.");

        var file = Environment.GetEnvironmentVariable("SEED_FILE");

        if (!string.IsNullOrEmpty(file) && File.Exists(file))
        {
            File.Delete(file);
        }

        await RunNowAsync(page, "Subscriber list check", "Failed");
        Console.WriteLine("Seeded the failed run.");

        await BuildWorkflowAsync(page, "Seed runs", "* * * * *", false);
        Console.WriteLine("Published the Seed runs workflow, waiting for three runs.");

        for (var i = 0; i < 120; i++)
        {
            await Task.Delay(5000);
            await page.GotoAsync(BaseUrl + "/Admin/DataPipelines/Runs");
            var count = await page.Locator(".badge:has-text('Workflow')").CountAsync();

            if (count >= 3)
            {
                break;
            }
        }

        await page.GotoAsync(BaseUrl + "/Admin/Workflows/Types");
        var row = page.Locator("li, tr").Filter(new() { HasText = "Seed runs" }).First;
        await row.Locator("button.actions, .dropdown-toggle").First.ClickAsync();
        await row.GetByText("Delete").First.ClickAsync();

        try
        {
            var confirm = page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").First;
            await confirm.WaitForAsync(new() { Timeout = 3000 });
            await confirm.ClickAsync();
        }
        catch (Exception)
        {
        }

        await Task.Delay(3000);
        await page.GotoAsync(BaseUrl + "/Admin/DataPipelines/Runs");
        Console.WriteLine("Runs listed: " + await page.Locator("li[data-run-id], .list-group-item").CountAsync());
        await context.CloseAsync();
    }

    // ---------- clips ----------

    // Drags the top edge of the activity panel up so that its form fits.
    private static async Task GrowPanelAsync(IPage page, int pixels)
    {
        var handle = page.Locator("[data-cy='activity-panel-resizer']").First;
        var box = await handle.BoundingBoxAsync();

        if (box is null)
        {
            return;
        }

        var x = box.X + box.Width / 2;
        var y = box.Y + box.Height / 2;
        await page.Mouse.MoveAsync(x, y);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(x, y - pixels, new() { Steps = 8 });
        await page.Mouse.UpAsync();
        await Task.Delay(300);
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DataPipelines");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            var blog = PipelineRow(page, "Blog posts export").Locator(".badge");
            await RingAsync(scene, blog.Nth(0), 1.4);
            await RingAsync(scene, blog.Nth(1), 1.6);
            var subscribers = PipelineRow(page, "Subscriber list check").Locator(".badge");
            await RingAsync(scene, subscribers.Nth(0), 1.2);
            await RingAsync(scene, subscribers.Nth(1), 1.6);
            await RingAsync(scene, PipelineRow(page, "Newsletter export").Locator(".badge:has-text('Draft')").First, 2.0);
        });

        await scene.StepAsync("run-2", async () =>
        {
            var row = PipelineRow(page, "Blog posts export");
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 300);
            await scene.MoveToAsync(row.Locator("a.dropdown-item:has-text('Run history')"));
            await scene.HoldAsync(1.0);
            await scene.MoveToAsync(row.Locator("a.dropdown-item:has-text('Run now')"));
            await scene.OffCameraAsync(async () => { }, 100);
            await scene.WaitForNavigationAsync(async () => await row.Locator("a.dropdown-item:has-text('Run now')").ClickAsync());
            await page.Locator("h1, h2").First.WaitForAsync();
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 40; i++)
                {
                    if (await page.Locator(".badge:has-text('Succeeded')").CountAsync() > 0)
                    {
                        break;
                    }

                    await Task.Delay(1500);
                    await page.ReloadAsync();
                }
            }, 400);

            await scene.NavigateAsync(page.Url.Replace(BaseUrl, string.Empty));
            await RingAsync(scene, page.Locator(".badge:has-text('Succeeded')").First, 1.5);
            await RingAsync(scene, page.GetByText("Version 1").First, 1.2);

            foreach (var label in new[] { "Queued", "Started", "Ended" })
            {
                await RingAsync(scene, page.GetByText(label, new() { Exact = true }).First, 1.0);
            }
        });
    }

    private static async Task ScheduleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("sched-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First, page.Locator("input[name='Name']"));
            await scene.ClickAsync(page.Locator("input[name='Name']"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Nightly blog posts export", new() { Delay = 35 });
            await GoAsync(scene, page.Locator("button.save").First, Cy(scene, "start-picker"));
        });

        await scene.StepAsync("sched-2", async () =>
        {
            await scene.ClickAsync(Cy(scene, "start-picker-TimerEvent"), pauseAfter: 300);
            await page.GetByLabel("CRON Expression").WaitForAsync(new() { Timeout = 30000 });
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 170), 200);
        });

        await scene.StepAsync("sched-3", async () =>
        {
            var box = page.GetByLabel("CRON Expression");
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("0 2 * * *", new() { Delay = 60 });
            await page.Keyboard.PressAsync("Tab");
            await page.GetByText("Saved!").First.WaitForAsync(new() { Timeout = 30000 });
            await scene.ClickAsync(page.GetByText("Use site time zone").First, pauseAfter: 300);
            await Task.Delay(1500);
        });

        await scene.StepAsync("sched-4", async () =>
        {
            var port = page.Locator("[aria-label^='Outcome Done']").First;
            await scene.MoveToAsync(port);
            await scene.HoldAsync(0.8);
            await port.ClickAsync();
            await scene.ClickAsync(Cy(scene, "quick-add-search"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Run Data", new() { Delay = 50 });
            await scene.ClickAsync(Cy(scene, "quick-add-RunDataPipelineTask"), pauseAfter: 300);
            await page.Locator("#RunDataPipelineTask_PipelineId").WaitForAsync(new() { Timeout = 30000 });
        });

        await scene.StepAsync("sched-5", async () =>
        {
            await scene.SelectAsync(page.Locator("#RunDataPipelineTask_PipelineId"), "Blog posts export");
            await page.Keyboard.PressAsync("Tab");
            await page.GetByText("Saved!").First.WaitForAsync(new() { Timeout = 30000 });
            var skip = page.GetByText("Skip when a run is already queued or running").First;
            await skip.ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(skip, pauseAfter: 300);
            await Task.Delay(1000);
        });

        await scene.StepAsync("sched-6", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbar-publish"), pauseAfter: 300);
            var anyway = page.GetByRole(AriaRole.Button, new() { Name = "Publish anyway" });

            try
            {
                await anyway.WaitForAsync(new() { Timeout = 4000 });
                await scene.HoldAsync(1.5);
                await scene.ClickAsync(anyway, pauseAfter: 200);
            }
            catch (Exception)
            {
            }

            await page.GetByText("was published").First.WaitForAsync(new() { Timeout = 30000 });
        });

        await Task.Delay(3000);
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DataPipelines/Runs");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("hist-1", async () =>
        {
            var first = page.Locator("li.list-group-item:not(.text-bg-theme)").First;
            await RingAsync(scene, first, 2.0);
            var badges = first.Locator(".badge");
            var count = await badges.CountAsync();

            for (var i = 0; i < count; i++)
            {
                await RingAsync(scene, badges.Nth(i), 1.2);
            }
        });

        await scene.StepAsync("hist-2", async () =>
        {
            await RingAsync(scene, page.Locator(".badge:has-text('Manual')").First, 2.5);
            await RingAsync(scene, page.Locator(".badge:has-text('Workflow')").First, 2.5);
        });

        await scene.StepAsync("hist-3", async () =>
        {
            await RingAsync(scene, page.Locator("input[type='search'], input[type='text']").First, 1.5);
            await scene.ClickAsync(page.Locator("[data-dp-status-filter]").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator(".dropdown-menu.show a:has-text('Failed'), .dropdown-menu a:has-text('Failed')").First, page.Locator(".text-danger, .alert-danger").First);
        });
    }

    private static async Task DetailsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DataPipelines/Runs?status=Failed");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("hist-4", async () =>
        {
            await GoAsync(scene, page.Locator("a:has-text('Details')").First, page.Locator(".badge:has-text('Failed')").First);
            await RingAsync(scene, page.Locator(".badge:has-text('Failed')").First, 1.5);

            foreach (var label in new[] { "Queued", "Started", "Ended", "Error" })
            {
                await RingAsync(scene, page.GetByText(label, new() { Exact = true }).First, 1.0);
            }
        });

        await scene.StepAsync("hist-5", async () =>
        {
            var steps = page.Locator("h5:has-text('Steps')").First;
            await RingAsync(scene, steps.Locator("xpath=following::table[1]"), 3.0);
            await page.Locator("h5:has-text('Log')").First.ScrollIntoViewIfNeededAsync();
            await RingAsync(scene, page.Locator("h5:has-text('Log')").First.Locator("xpath=following::*[contains(@class,'list-group')][1]"), 3.0);
        });

        await scene.StepAsync("hist-6", async () =>
        {
            await scene.NavigateAsync("/Admin/DataPipelines/Runs");
            await GoAsync(scene, page.Locator("a:has-text('Details')").First, page.Locator("h5:has-text('Steps')").First);
            await RingAsync(scene, page.Locator("h5:has-text('Steps')").First.Locator("xpath=following::table[1]"), 3.0);
            await RingAsync(scene, page.Locator("h5:has-text('Delivered')").First.Locator("xpath=following::li[1]"), 3.5);
        });
    }
}

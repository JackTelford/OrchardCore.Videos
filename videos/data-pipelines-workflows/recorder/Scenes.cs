using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the data-pipelines-workflows video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-build", BuildAsync),
        ("03-run", RunAsync),
        ("04-react", ReactAsync),
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
        try
        {
            await target.EvaluateAsync("e => e.scrollIntoView({ block: 'center', inline: 'center' })", null, new() { Timeout = 4000 });
            await Task.Delay(300);
        }
        catch (Exception)
        {
            Console.WriteLine("  (not visible, skipped: " + target + ")");
            await scene.HoldAsync(seconds);
            return;
        }

        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Port(IPage page, string outcome) => page.Locator($"[aria-label^='Outcome {outcome}']").First;

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

    // ---------- seed (outside the capture) ----------

    private const string FeaturesRecipe = """
{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.Workflows", "OrchardCore.DataPipelines", "OrchardCore.Email" ] } ] }
""";

    private const string PipelinesRecipe = """
{
  "steps": [
    {
      "name": "DataPipelines",
      "Pipelines": [
        {
          "PipelineId": "blogpostsexport",
          "Name": "Blog posts export",
          "Description": "Saves the blog posts as a CSV file in the media library.",
          "IsEnabled": true,
          "Definition": {
            "Steps": [
              { "StepId": "source", "Type": "DataSource", "X": 120, "Y": 200, "Properties": { "DataSourceStepSettings": { "Source": "Contents", "DataSet": "BlogPost", "Fields": [ "DisplayText", "Author", "PublishedUtc" ] } } },
              { "StepId": "file", "Type": "CreateFile", "X": 440, "Y": 200, "Properties": { "CreateFileStepSettings": { "Format": "csv", "FileName": "blog-posts" } } },
              { "StepId": "media", "Type": "SaveToMedia", "X": 760, "Y": 200, "Properties": { "SaveToMediaStepSettings": { "Folder": "", "Overwrite": true } } }
            ],
            "Connections": [
              { "SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input" },
              { "SourceStepId": "file", "SourcePort": "Output", "TargetStepId": "media", "TargetPort": "Input" }
            ]
          }
        },
        {
          "PipelineId": "blogpostsbyemail",
          "Name": "Blog posts by email",
          "Description": "Sends the blog posts by email to the editors.",
          "IsEnabled": true,
          "Definition": {
            "Steps": [
              { "StepId": "source", "Type": "DataSource", "X": 120, "Y": 200, "Properties": { "DataSourceStepSettings": { "Source": "Contents", "DataSet": "BlogPost", "Fields": [ "DisplayText", "Author", "PublishedUtc" ] } } },
              { "StepId": "file", "Type": "CreateFile", "X": 440, "Y": 200, "Properties": { "CreateFileStepSettings": { "Format": "csv", "FileName": "blog-posts" } } },
              { "StepId": "mail", "Type": "SendByEmail", "X": 760, "Y": 200, "Properties": { "SendByEmailStepSettings": { "To": "editors@example.com", "Subject": "Blog posts", "Body": "The blog posts are attached." } } }
            ],
            "Connections": [
              { "SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input" },
              { "SourceStepId": "file", "SourcePort": "Output", "TargetStepId": "mail", "TargetPort": "Input" }
            ]
          }
        }
      ]
    }
  ]
}
""";

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

    private static async Task PublishPipelineAsync(IPage page, string id)
    {
        await page.GotoAsync(BaseUrl + $"/Admin/DataPipelines/{id}/Edit");
        await page.Locator("[data-cy='data-pipeline-designer']").WaitForAsync(new() { Timeout = 60000 });
        await page.Locator("[data-node-id]").First.WaitForAsync(new() { Timeout = 60000 });
        await Task.Delay(1500);
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


    private const string PostRecipe = """
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "Man must explore, and this is exploration at its greatest", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Man must explore, and this is exploration at its greatest" },
          "MarkdownBodyPart": { "Markdown": "Problems look mighty small from 150 miles up." },
          "AutoroutePart": { "Path": "blog/man-must-explore" }
        }
      ]
    }
  ]
}
""";

    // Adds the sample blog post of the video (the Blog recipe of this build has none), outside the capture.
    public static async Task SeedPostAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await ImportAsync(page, PostRecipe);
        await context.CloseAsync();
    }

    // Enables Workflows, Data Pipelines and Email, imports the two pipelines and publishes them (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        if (Environment.GetEnvironmentVariable("SEED_STAGE") == "post")
        {
            await SeedPostAsync(browser);
            return;
        }

        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        await ImportAsync(page, FeaturesRecipe);
        await Task.Delay(10000);
        await ImportAsync(page, PipelinesRecipe);
        await PublishPipelineAsync(page, "blogpostsexport");
        await PublishPipelineAsync(page, "blogpostsbyemail");
        Console.WriteLine("Prepared the features and the two published pipelines.");
        await context.CloseAsync();
    }

    // ---------- clips ----------

    private static async Task BuildAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("build-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First, page.Locator("input[name='Name']"));
            await scene.ClickAsync(page.Locator("input[name='Name']"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Export the blog posts when one is published", new() { Delay = 30 });
        });

        await scene.StepAsync("build-2", async () =>
        {
            await GoAsync(scene, page.Locator("button.save").First, Cy(scene, "start-picker"));
            await scene.ClickAsync(Cy(scene, "start-picker-ContentPublishedEvent"), pauseAfter: 300);
            await Cy(scene, "panel-activity-form").WaitForAsync(new() { Timeout = 60000 });
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 170), 200);
        });

        await scene.StepAsync("build-3", async () =>
        {
            await RingAsync(scene, Cy(scene, "panel-activity-form").GetByText("Content Types").First, 3.0);
            await RingAsync(scene, Cy(scene, "start-badge").First, Math.Max(2.0, durations["build-3"] - 5.0));
        });

        await scene.StepAsync("build-4", async () =>
        {
            await scene.OffCameraAsync(async () => await Cy(scene, "activity-panel-close").ClickAsync(), 200);
            var done = Port(page, "Done");
            await scene.MoveToAsync(done);
            await scene.HoldAsync(0.6);
            await done.ClickAsync();
            await scene.ClickAsync(Cy(scene, "quick-add-search"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("pipeline", new() { Delay = 45 });
            await scene.HoldAsync(1.0);
            await scene.ClickAsync(Cy(scene, "quick-add-RunDataPipelineTask"), pauseAfter: 300);
            await page.Locator("#RunDataPipelineTask_PipelineId").WaitForAsync(new() { Timeout = 30000 });
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 300), 200);
        });

        await scene.StepAsync("build-5", async () =>
        {
            await scene.SelectAsync(page.Locator("#RunDataPipelineTask_PipelineId"), "blogpostsexport");
            await scene.HoldAsync(1.5);

            // The panel applies the choice on Tab, then redraws the list empty although the choice is kept: choose it again off camera.
            await scene.OffCameraAsync(async () =>
            {
                await page.Keyboard.PressAsync("Tab");
                await Task.Delay(2500);
                await page.Locator("#RunDataPipelineTask_PipelineId").SelectOptionAsync("blogpostsexport");
                await Task.Delay(500);
            }, 300);

            await scene.HoldAsync(1.0);
        });

        await scene.StepAsync("build-6", async () =>
        {
            await RingAsync(scene, page.Locator("#RunDataPipelineTask_Parameters, textarea[name='RunDataPipelineTask.Parameters']").First, Math.Max(2.5, durations["build-6"] - 2.0));
        });

        await scene.StepAsync("build-7", async () =>
        {
            await RingAsync(scene, page.Locator("label[for='RunDataPipelineTask_WaitForCompletion']"), 2.0);
            await RingAsync(scene, Port(page, "Succeeded"), 1.5);
            await RingAsync(scene, Port(page, "Failed"), 1.5);
            await scene.HighlightAsync(page.Locator("label[for='RunDataPipelineTask_SkipWhenActive']"));
            await scene.HoldAsync(2.0);
        });

        await scene.StepAsync("build-8", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Cy(scene, "toolbar-publish"), pauseAfter: 300);
            await Cy(scene, "published-version").WaitForAsync(new() { Timeout = 60000 });
        });

        await Task.Delay(2500);
    }

    private static async Task WaitRunAsync(IPage page, string badge)
    {
        for (var i = 0; i < 90; i++)
        {
            var html = await page.EvaluateAsync<string>("async () => (await fetch('/Admin/DataPipelines/Runs')).text()");
            var doc = new System.Text.RegularExpressions.Regex("data-run-id[^>]*>.*?(Succeeded|Failed|Running|Queued|Cancelled)", System.Text.RegularExpressions.RegexOptions.Singleline).Match(html);

            if (doc.Success && doc.Groups[1].Value == badge)
            {
                return;
            }

            await Task.Delay(1000);
        }
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems?options.selectedContentType=BlogPost");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            await GoAsync(scene, page.Locator("a:has-text('Man must explore')").First, page.Locator("button[name='submit.Publish']").First);
            await scene.HoldAsync(1.5);
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").First, page.GetByText("has been published").First);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.OffCameraAsync(async () => await WaitRunAsync(page, "Succeeded"), 400);
            await scene.NavigateAsync("/Admin/DataPipelines/Runs");
            await RingAsync(scene, page.Locator("li[data-run-id] span.badge:has(i.fa-play)").First, 2.5);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await GoAsync(scene, page.Locator("a:has-text('Details')").First, page.Locator("h5:has-text('Delivered')").First);
            await RingAsync(scene, page.Locator("h5:has-text('Delivered')").First.Locator("xpath=following::li[1]"), Math.Max(2.5, durations["run-3"] - 4.0));
        });

        await Task.Delay(2500);
    }

    private static async Task ReactAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("react-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First, page.Locator("input[name='Name']"));
            await scene.ClickAsync(page.Locator("input[name='Name']"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Log the pipeline runs that fail", new() { Delay = 30 });
            await GoAsync(scene, page.Locator("button.save").First, Cy(scene, "start-picker"));
        });

        await scene.StepAsync("react-2", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbox-search"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("pipeline", new() { Delay = 45 });
            await scene.HoldAsync(1.0);
            await scene.ClickAsync(Cy(scene, "toolbox-activity-DataPipelineRunCompletedEvent"), pauseAfter: 300);
            await Cy(scene, "panel-activity-form").WaitForAsync(new() { Timeout = 60000 });
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 170), 200);
        });

        await scene.StepAsync("react-3", async () =>
        {
            await RingAsync(scene, page.GetByLabel("Data pipeline", new() { Exact = true }), 2.0);
            var runs = page.GetByLabel("Runs", new() { Exact = true });
            await scene.SelectAsync(runs, "Runs that failed or were cancelled");
            await scene.HoldAsync(1.2);

            // The panel applies the choice on Tab, then redraws the list with the old text: choose it again off camera.
            await scene.OffCameraAsync(async () =>
            {
                await page.Keyboard.PressAsync("Tab");
                await Task.Delay(2500);
                await runs.SelectOptionAsync(new SelectOptionValue { Label = "Runs that failed or were cancelled" });
                await Task.Delay(500);
            }, 300);

            await scene.HoldAsync(1.0);
        });

        await scene.StepAsync("react-4", async () =>
        {
            await scene.OffCameraAsync(async () => await Cy(scene, "activity-panel-close").ClickAsync(), 200);
            var done = Port(page, "Done");
            await scene.MoveToAsync(done);
            await scene.HoldAsync(0.6);
            await done.ClickAsync();
            await scene.ClickAsync(Cy(scene, "quick-add-search"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("log", new() { Delay = 45 });
            await scene.ClickAsync(Cy(scene, "quick-add-LogTask"), pauseAfter: 300);
            await page.GetByLabel("Log Level", new() { Exact = true }).WaitForAsync(new() { Timeout = 30000 });
            var text = page.GetByLabel("Text", new() { Exact = true });
            await scene.ClickAsync(text, pauseAfter: 100);
            await page.Keyboard.TypeAsync("A data pipeline run failed", new() { Delay = 30 });
            await page.Keyboard.PressAsync("Tab");
            await Task.Delay(1200);
            var level = page.GetByLabel("Log Level", new() { Exact = true });
            await scene.SelectAsync(level, "Error");
            await scene.HoldAsync(1.0);

            // The panel redraws the list with the old choice after Tab: choose it again off camera.
            await scene.OffCameraAsync(async () =>
            {
                await page.Keyboard.PressAsync("Tab");
                await Task.Delay(2500);
                await level.SelectOptionAsync("Error");
                await Task.Delay(500);
            }, 300);
        });

        await scene.StepAsync("react-5", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbar-publish"), pauseAfter: 300);
            await Cy(scene, "published-version").WaitForAsync(new() { Timeout = 60000 });
        });

        await scene.StepAsync("react-6", async () =>
        {
            await scene.NavigateAsync("/Admin/DataPipelines");
            var row = page.Locator("li[data-pipeline-id='blogpostsbyemail']");
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 300);
            await scene.HoldAsync(0.8);
            var run = row.Locator("a.dropdown-item:has-text('Run now')");
            await scene.MoveToAsync(run);
            await scene.OffCameraAsync(async () => { }, 100);
            await scene.WaitForNavigationAsync(async () => await run.ClickAsync());
            await page.GetByText("will run in the background").First.WaitForAsync(new() { Timeout = 30000 });
        });

        await scene.StepAsync("react-7", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await WaitRunAsync(page, "Failed");
                await Task.Delay(4000);
            }, 400);

            await scene.NavigateAsync("/Admin/DataPipelines/Runs");
            await RingAsync(scene, page.Locator("li[data-run-id] .text-danger").First, 3.0);
        });

        await scene.StepAsync("react-8", async () =>
        {
            await scene.NavigateAsync("/Admin/Workflows/Types");
            await GoAsync(scene, page.Locator("a[data-cy='all-instances']").First, page.Locator("body"));
        });

        await Task.Delay(3000);
    }
}

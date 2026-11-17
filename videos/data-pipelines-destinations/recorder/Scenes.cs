using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the data-pipelines-destinations video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static string FtpPassword => Environment.GetEnvironmentVariable("VIDEO_FTP_PASSWORD") ?? throw new InvalidOperationException("Set VIDEO_FTP_PASSWORD.");

    private static string SftpPassword => Environment.GetEnvironmentVariable("VIDEO_SFTP_PASSWORD") ?? throw new InvalidOperationException("Set VIDEO_SFTP_PASSWORD.");

    private static string Fingerprint => Environment.GetEnvironmentVariable("VIDEO_SFTP_FINGERPRINT") ?? throw new InvalidOperationException("Set VIDEO_SFTP_FINGERPRINT.");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-link", LinkAsync),
        ("04-email", EmailAsync),
        ("05-ftp", FtpAsync),
        ("06-sftp", SftpAsync),
        ("07-run", RunAsync),
        ("08-links", LinksAsync),
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

    // Expands each collapsed group of the left menu above a link (outermost first) and returns the link.
    private static async Task<ILocator> ExpandMenuAsync(Scene scene, string href)
    {
        var page = scene.Page;
        var link = page.Locator($"#left-nav a[href$='{href}']").First;
        await link.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 90000 });
        var ancestors = link.Locator("xpath=ancestor::ul[@data-title]");
        var count = await ancestors.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var ul = ancestors.Nth(i);

            if (!await ul.IsVisibleAsync())
            {
                var title = await ul.GetAttributeAsync("data-title");
                var caption = ul.Locator($"xpath=preceding-sibling::figcaption | ../figcaption[.//span[normalize-space()='{title}']] | ../*/figcaption[.//span[normalize-space()='{title}']]").First;
                await scene.ClickAsync(caption, pauseAfter: 150);
            }
        }

        return link;
    }

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

    private static ILocator PanelField(Scene scene, string label) => Cy(scene, "step-panel").GetByLabel(label, new() { Exact = true });

    // Types into a field of the step panel: clears it, types, then Tab applies the value.
    private static async Task FillFieldAsync(Scene scene, ILocator field, string text, bool clear = false)
    {
        await field.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
        await Task.Delay(250);
        await scene.ClickAsync(field, pauseAfter: 80);

        if (clear)
        {
            await field.FillAsync(string.Empty);
        }

        await scene.Page.Keyboard.TypeAsync(text, new() { Delay = 22 });
        await scene.Page.Keyboard.PressAsync("Tab");
    }

    private static async Task DraftSavedAsync(Scene scene)
    {
        await scene.Page.GetByText("Draft saved").First.WaitForAsync(new() { Timeout = 60000 });
        await Task.Delay(800);
    }

    // Drags the top edge of the step panel up so that a tall form fits.
    private static async Task GrowPanelAsync(IPage page, int pixels)
    {
        var handle = page.Locator("[data-cy='step-panel-resizer']").First;
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

    // Opens the designer of the pipeline (outside the capture) with the admin menu compacted.
    private static async Task OpenDesignerAsync(Scene scene, bool compact = true)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/DataPipelines");

        if (compact)
        {
            await page.Locator("button.leftbar-compactor").First.ClickAsync();
        }

        await scene.NavigateAsync("/Admin/DataPipelines/blogpostsexport/Edit");
        await Cy(scene, "step-source").WaitForAsync(new() { Timeout = 60000 });
        await Cy(scene, "step-file").WaitForAsync(new() { Timeout = 60000 });
        await Task.Delay(1500);
    }

    private static async Task AddAfterFileAsync(Scene scene, string search, string stepName)
    {
        var page = scene.Page;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await scene.MoveToAsync(Cy(scene, "step-file"));
        Console.WriteLine("   move " + sw.ElapsedMilliseconds);
        await Cy(scene, "step-file").ClickAsync(new() { Button = MouseButton.Right });
        Console.WriteLine("   right " + sw.ElapsedMilliseconds);
        await scene.ClickAsync(Cy(scene, "menu-add-after-Output"), pauseAfter: 200);
        Console.WriteLine("   menu " + sw.ElapsedMilliseconds);
        await scene.ClickAsync(Cy(scene, "quick-add-search"), pauseAfter: 100);
        Console.WriteLine("   search " + sw.ElapsedMilliseconds);
        await page.Keyboard.TypeAsync(search, new() { Delay = 40 });
        await scene.ClickAsync(Cy(scene, $"quick-add-{stepName}"), pauseAfter: 300);
        Console.WriteLine("   pick " + sw.ElapsedMilliseconds);
    }

    // ---------- seed (outside the capture) ----------

    private const string FeaturesRecipe = """
{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.DataPipelines", "OrchardCore.Email", "OrchardCore.Email.Smtp" ] } ] }
""";

    private const string PipelineRecipe = """
{
  "steps": [
    {
      "name": "DataPipelines",
      "Pipelines": [
        {
          "PipelineId": "blogpostsexport",
          "Name": "Blog posts export",
          "Description": "Exports the blog posts to a CSV file.",
          "IsEnabled": true,
          "Definition": {
            "Steps": [
              { "StepId": "source", "Type": "DataSource", "X": 120, "Y": 160, "Properties": { "DataSourceStepSettings": { "Source": "Contents", "DataSet": "BlogPost" } } },
              { "StepId": "file", "Type": "CreateFile", "X": 440, "Y": 160, "Properties": { "CreateFileStepSettings": { "Format": "csv", "FileName": "blog-posts" } } }
            ],
            "Connections": [
              { "SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input" }
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

    // Enables Data Pipelines, Email and SMTP, and imports the draft pipeline (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await ImportAsync(page, FeaturesRecipe);
        await Task.Delay(10000);
        await ImportAsync(page, PipelineRecipe);
        Console.WriteLine("Prepared Data Pipelines, Email, SMTP and the draft pipeline.");
        await context.CloseAsync();
    }

    // ---------- clips ----------

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("#search-box");

        await scene.StepAsync("features-1", async () =>
        {
            var link = await ExpandMenuAsync(scene, "/Admin/Features");
            await GoAsync(scene, link, box);
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("pipelines", new() { Delay = 40 });
            await Task.Delay(800);
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.ClickAsync(page.Locator("[id='OrchardCore.DataPipelines.Ftp']").First, pauseAfter: 200);
            await scene.ClickAsync(page.Locator("[id='OrchardCore.DataPipelines.Sftp']").First, pauseAfter: 200);
            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
            var enable = page.Locator("a.dropdown-item[data-action='Enable']").First;
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await enable.ClickAsync());
                await page.GetByText("were enabled").First.WaitForAsync(new() { Timeout = 120000 });
            }, 450);
        });

        await Task.Delay(2500);
    }

    private static async Task LinkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("link-1", async () =>
        {
            var pipelines = await ExpandMenuAsync(scene, "/Admin/DataPipelines");
            await GoAsync(scene, pipelines, page.Locator("li[data-pipeline-id='blogpostsexport']"));
            await GoAsync(scene, page.Locator("li[data-pipeline-id='blogpostsexport'] a.btn-primary"), Cy(scene, "step-file"));
            await Cy(scene, "step-source").WaitForAsync();
        });

        await scene.StepAsync("link-2", async () =>
        {
            await scene.OffCameraAsync(async () => await page.Locator("button.leftbar-compactor").First.ClickAsync(), 200);
            await scene.ClickAsync(Cy(scene, "port-file-out-Output"), pauseAfter: 300);
            await Cy(scene, "quick-add").WaitForAsync();
            await scene.ClickAsync(Cy(scene, "quick-add-search"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("share", new() { Delay = 40 });
            await scene.ClickAsync(Cy(scene, "quick-add-ShareDownloadLink"), pauseAfter: 300);
            await PanelField(scene, "Recipients").WaitForAsync(new() { Timeout = 30000 });
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 260), 200);
        });

        await scene.StepAsync("link-3", async () =>
        {
            await FillFieldAsync(scene, PanelField(scene, "Recipients"), "jamie.chen");
        });

        await scene.StepAsync("link-4", async () =>
        {
            await FillFieldAsync(scene, PanelField(scene, "Maximum downloads"), "3", clear: true);
            await FillFieldAsync(scene, PanelField(scene, "Email subject"), "Your blog posts export is ready", clear: true);
            await FillFieldAsync(scene, PanelField(scene, "Message"), "The file is ready to download.");
        });

        await DraftSavedAsync(scene);
    }

    private static async Task EmailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("email-1", async () =>
        {
            await AddAfterFileAsync(scene, "email", "SendByEmail");
            await PanelField(scene, "To").WaitForAsync(new() { Timeout = 30000 });
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 260), 200);
        });

        await scene.StepAsync("email-2", async () =>
        {
            await FillFieldAsync(scene, PanelField(scene, "To"), "reports@example.com");
            await scene.HighlightAsync(PanelField(scene, "Cc"));
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await RingAsync(scene, PanelField(scene, "Bcc"), 1.5);
        });

        await scene.StepAsync("email-3", async () =>
        {
            await FillFieldAsync(scene, PanelField(scene, "Subject"), "Blog posts export", clear: true);
            await FillFieldAsync(scene, PanelField(scene, "Body"), "The blog posts export is attached.", clear: true);
            await RingAsync(scene, Cy(scene, "step-panel").GetByLabel("The body is HTML"), 2.0);
        });

        await scene.StepAsync("email-4", async () =>
        {
            await RingAsync(scene, PanelField(scene, "Maximum size (MB)"), 3.0);
        });

        await DraftSavedAsync(scene);
    }

    private static async Task FtpAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("ftp-1", async () =>
        {
            await AddAfterFileAsync(scene, "ftp", "UploadToFtp");
            await PanelField(scene, "Host").WaitForAsync(new() { Timeout = 30000 });
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 260), 200);
            await FillFieldAsync(scene, PanelField(scene, "Host"), "localhost");
            await FillFieldAsync(scene, PanelField(scene, "User name"), "demo");
        });

        await scene.StepAsync("ftp-2", async () =>
        {
            await FillFieldAsync(scene, PanelField(scene, "Password"), FtpPassword);
            await page.GetByText("Leave empty to keep the current password.").First.WaitForAsync(new() { Timeout = 30000 });
            await scene.SelectAsync(PanelField(scene, "Encryption"), "None (plain FTP)");
            await RingAsync(scene, page.GetByText("Plain FTP isn't encrypted").First, 2.5);
        });

        await scene.StepAsync("ftp-3", async () =>
        {
            await FillFieldAsync(scene, PanelField(scene, "Remote folder"), "/exports");
            await RingAsync(scene, Cy(scene, "step-panel").GetByLabel("Replace existing files"), 2.0);
        });

        await scene.StepAsync("ftp-4", async () =>
        {
            await RingAsync(scene, PanelField(scene, "Password"), 2.0);
            await scene.ClickAsync(Cy(scene, "step-panel-close"), pauseAfter: 300);
            await RingAsync(scene, page.Locator("[data-node-id]").Filter(new() { HasText = "Upload to an FTP server" }).First, 2.5);
        });

        await DraftSavedAsync(scene);
    }

    private static async Task SftpAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("sftp-1", async () =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await AddAfterFileAsync(scene, "sftp", "UploadToSftp");
            Console.WriteLine("  add " + sw.ElapsedMilliseconds);
            await PanelField(scene, "Host").WaitForAsync(new() { Timeout = 30000 });
            Console.WriteLine("  label " + sw.ElapsedMilliseconds);
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 260), 200);
            Console.WriteLine("  grown " + sw.ElapsedMilliseconds);
            await FillFieldAsync(scene, PanelField(scene, "Host"), "localhost");
            Console.WriteLine("  host " + sw.ElapsedMilliseconds);
            await FillFieldAsync(scene, PanelField(scene, "Port"), "2222", clear: true);
            Console.WriteLine("  port " + sw.ElapsedMilliseconds);
            await FillFieldAsync(scene, PanelField(scene, "User name"), "demo");
        });

        await scene.StepAsync("sftp-2", async () =>
        {
            await FillFieldAsync(scene, PanelField(scene, "Password"), SftpPassword);
            await page.GetByText("Leave empty to keep the current password.").First.WaitForAsync(new() { Timeout = 30000 });
            await RingAsync(scene, PanelField(scene, "Private key"), 1.2);
            await FillFieldAsync(scene, PanelField(scene, "Host key fingerprint"), Fingerprint);
            await Task.Delay(600);
        });

        await scene.StepAsync("sftp-3", async () =>
        {
            await FillFieldAsync(scene, PanelField(scene, "Remote folder"), "/upload/exports");
            await scene.ClickAsync(Cy(scene, "step-panel-close"), pauseAfter: 300);
            var fit = page.Locator("[title='Fit to the steps'], [aria-label='Fit to the steps']").First;

            if (await fit.CountAsync() > 0)
            {
                await scene.ClickAsync(fit, pauseAfter: 300);
            }
        });

        await DraftSavedAsync(scene);
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene);
        await scene.Page.Keyboard.PressAsync("Escape");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbar-publish"), pauseAfter: 300);
            var anyway = page.GetByRole(AriaRole.Button, new() { Name = "Publish anyway" });
            await anyway.WaitForAsync(new() { Timeout = 30000 });
            await scene.HighlightAsync(page.GetByText("The connection isn't encrypted").First);
            await scene.HoldAsync(Math.Max(2.0, durations["run-1"] * 0.55));
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(anyway, pauseAfter: 300);
            await Cy(scene, "published-version").WaitForAsync(new() { Timeout = 60000 });
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbar-run"), pauseAfter: 300);
            await Cy(scene, "runs-tab").WaitForAsync();
            var badge = page.Locator(".dpd-run-head.is-selected .badge", new() { HasTextString = "Succeeded" }).First;
            await scene.OffCameraAsync(async () => await badge.WaitForAsync(new() { Timeout = 180000 }), 800);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await RingAsync(scene, Cy(scene, "run-deliveries"), Math.Max(3.0, durations["run-3"] - 3.0));
        });

        await scene.StepAsync("run-4", async () =>
        {
            var warning = Cy(scene, "run-log").GetByText("isn't encrypted").First;
            await RingAsync(scene, warning, 3.0);
            await GoAsync(scene, Cy(scene, "run-page").First, page.Locator("h5:has-text('Delivered')").First);
            await RingAsync(scene, page.Locator("h5:has-text('Delivered')").First, 2.5);
        });
    }

    private static async Task LinksAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("links-1", async () =>
        {
            var link = await ExpandMenuAsync(scene, "/Admin/SharedLinks");
            await GoAsync(scene, link, page.Locator("li[data-link-id]").First);
            await RingAsync(scene, page.Locator("li[data-link-id]").First, 3.0);
        });

        await scene.StepAsync("links-2", async () =>
        {
            var row = page.Locator("li[data-link-id]").First;
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 300);
            var revoke = row.Locator(".dropdown-menu a, .dropdown-menu button").Filter(new() { HasText = "Revoke" }).First;
            await scene.HighlightAsync(revoke);
            await scene.HoldAsync(2.5);
        });

        await Task.Delay(2500);
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the data-pipelines-import video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-read", ReadAsync),
        ("03-save", SaveAsync),
        ("04-run", RunAsync),
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
                await scene.ClickAsync(caption, pauseAfter: 120);
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

    private const string FeatureRecipe = """
{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.DataPipelines" ] } ] }
""";

    private const string ProductRecipe = """
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Product",
          "DisplayName": "Product",
          "Settings": { "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true } },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            { "PartName": "Product", "Name": "Product", "Settings": { "ContentTypePartSettings": { "Position": "1" } } }
          ]
        }
      ],
      "ContentParts": [
        {
          "Name": "Product",
          "Settings": {},
          "ContentPartFieldDefinitionRecords": [
            {
              "FieldName": "NumericField",
              "Name": "Price",
              "Settings": {
                "ContentPartFieldSettings": { "DisplayName": "Price", "Position": "0" },
                "NumericFieldSettings": { "Required": false, "Scale": 2, "Minimum": 0 }
              }
            },
            {
              "FieldName": "TextField",
              "Name": "Category",
              "Settings": { "ContentPartFieldSettings": { "DisplayName": "Category", "Position": "1" } }
            }
          ]
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

    // Enables Data Pipelines and creates the Product type (outside the capture). The CSV file is copied into the media folder beforehand.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await ImportAsync(page, FeatureRecipe);
        await Task.Delay(8000);
        await ImportAsync(page, ProductRecipe);
        Console.WriteLine("Prepared Data Pipelines and the Product type.");
        await context.CloseAsync();
    }

    // Opens the designer of the pipeline from the Pipelines page, with the admin menu compacted (outside the capture).
    private static async Task OpenDesignerAsync(Scene scene, bool compact)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/DataPipelines");

        if (compact)
        {
            await page.Locator("button.leftbar-compactor").First.ClickAsync();
        }

        var edit = page.Locator("li[data-pipeline-id]").Filter(new() { HasText = "Import products" }).Locator("a.btn-primary").First;
        await scene.NavigateAsync(await edit.GetAttributeAsync("href"));
        await Cy(scene, "data-pipeline-designer").WaitForAsync(new() { Timeout = 60000 });
        await page.Locator("[data-node-id]").First.WaitForAsync(new() { Timeout = 60000 });
        await Task.Delay(1500);
    }

    // Drags the top edge of the step panel up so that a tall form or table fits.
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

    private static async Task ReadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/DataPipelines");
        await ExpandMenuAsync(scene, "/Admin/DataPipelines");
        await scene.StartCaptureAsync();

        await scene.StepAsync("read-1", async () =>
        {
            await RingAsync(scene, page.Locator(".alert-info").First, Math.Min(4.0, durations["read-1"] - 2.5));
            await GoAsync(scene, page.Locator("a.create").First, page.Locator("input[name='Name']"));
        });

        await scene.StepAsync("read-2", async () =>
        {
            var name = page.Locator("input[name='Name']");
            await scene.ClickAsync(name, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Import products", new() { Delay = 40 });
            await GoAsync(scene, page.Locator("button.save").First, Cy(scene, "start-picker"));
        });

        await scene.StepAsync("read-3", async () =>
        {
            await scene.ClickAsync(page.Locator("button.leftbar-compactor").First, pauseAfter: 400);
            await Cy(scene, "start-picker").WaitForAsync();
            await scene.ClickAsync(Cy(scene, "start-picker-ReadMediaFile"), pauseAfter: 200);
            await Assertions.Expect(Cy(scene, "panel-step-title")).ToContainTextAsync("Read a media file", new() { Timeout = 30000 });
        });

        await scene.StepAsync("read-4", async () =>
        {
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 260), 200);
            var file = page.GetByLabel("File", new() { Exact = true });
            await scene.ClickAsync(file, pauseAfter: 100);
            await page.Keyboard.TypeAsync("imports/products.csv", new() { Delay = 40 });
            await page.Keyboard.PressAsync("Tab");
            await Assertions.Expect(page.Locator("[data-node-id]").First).ToContainTextAsync("imports/products.csv", new() { Timeout = 30000 });
            await RingAsync(scene, page.Locator("select").Filter(new() { Has = page.Locator("option", new() { HasTextString = "From the extension" }) }).First, 2.5);
            await RingAsync(scene, page.GetByLabel("The first row holds the names of the columns"), 2.0);
        });

        await scene.StepAsync("read-5", async () =>
        {
            await scene.ClickAsync(Cy(scene, "step-tab-data"), pauseAfter: 200);
            await scene.ClickAsync(Cy(scene, "preview-run"), pauseAfter: 200);
            await Assertions.Expect(page.Locator("[data-cy='preview-tab'] tbody tr")).ToHaveCountAsync(5, new() { Timeout = 60000 });
        });

        await Task.Delay(4000);
    }

    private static async Task SaveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene, compact: true);
        await page.Keyboard.PressAsync("Escape");
        await scene.StartCaptureAsync();
        var form = Cy(scene, "panel-step-form");

        await scene.StepAsync("save-1", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-node-id]:has-text('Read a media file') [data-port-side='output']").First, pauseAfter: 200);
            await Cy(scene, "quick-add").WaitForAsync();
            await scene.ClickAsync(Cy(scene, "quick-add-search"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Save content", new() { Delay = 40 });
            await scene.ClickAsync(Cy(scene, "quick-add-SaveContentItems"), pauseAfter: 200);
            await Assertions.Expect(Cy(scene, "panel-step-title")).ToContainTextAsync("Save content items", new() { Timeout = 30000 });
        });

        await scene.StepAsync("save-2", async () =>
        {
            await scene.OffCameraAsync(async () => await GrowPanelAsync(page, 330), 200);
            await scene.SelectAsync(page.GetByLabel("Content type", new() { Exact = true }), "Product");
            await page.GetByLabel("Rows", new() { Exact = true }).WaitForAsync(new() { Timeout = 30000 });

            // This build shows Items per save as 0 and Publish unchecked after the type is chosen, which blocks the rows: set the defaults off camera.
            await scene.OffCameraAsync(async () =>
            {
                await page.Locator("#DataPipelineStep_BatchSize").FillAsync("100");
                await page.Locator("#DataPipelineStep_Publish").CheckAsync();
                await Task.Delay(2500);
            }, 200);

            await RingAsync(scene, page.GetByText("The user who publishes the pipeline must be allowed").First, 3.0);
        });

        await scene.StepAsync("save-3", async () =>
        {
            await RingAsync(scene, page.Locator("select[name$='.Mode']").First, Math.Max(2.0, durations["save-3"] - 3.0));
        });

        await scene.StepAsync("save-4", async () =>
        {
            await scene.SelectAsync(page.Locator("select[name$='.MatchBy']").First, "DisplayText");
            await scene.SelectAsync(page.Locator("select[name$='.KeyField']").First, "Name");
        });

        await scene.StepAsync("save-5", async () =>
        {
            var rows = new[] { ("TitlePart.Title", "Name"), ("Product.Price", "Price"), ("Product.Category", "Category") };

            for (var i = 0; i < rows.Length; i++)
            {
                var add = page.Locator("[data-dp-add]").First;
                await add.ScrollIntoViewIfNeededAsync();
                await scene.ClickAsync(add, pauseAfter: 100);
                var row = page.Locator("[data-dp-row]").Nth(i);
                await scene.SelectAsync(row.Locator("select[name$='.Target']"), rows[i].Item1, pauseAfter: 100);
                await scene.SelectAsync(row.Locator("select[name$='.Source']"), rows[i].Item2, pauseAfter: 100);
            }
        });

        await scene.StepAsync("save-6", async () =>
        {
            foreach (var label in new[] { "Publish the items", "Items per save" })
            {
                await RingAsync(scene, page.Locator($"label:has-text('{label}')").First, 2.5);
            }

            await scene.ClickAsync(Cy(scene, "panel-tab-issues"), pauseAfter: 200);
            await Assertions.Expect(page.GetByText("No issues").First).ToBeVisibleAsync(new() { Timeout = 30000 });
        });

        await scene.StepAsync("save-7", async () =>
        {
            await scene.ClickAsync(Cy(scene, "step-tab-data"), pauseAfter: 200);
            await scene.ClickAsync(Cy(scene, "preview-run"), pauseAfter: 200);
            await Cy(scene, "preview-input-note").WaitForAsync(new() { Timeout = 60000 });
        });

        await Task.Delay(3000);
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene, compact: true);
        await page.Keyboard.PressAsync("Escape");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HoldAsync(Math.Max(1.0, durations["run-1"] - 9.0));
            await scene.ClickAsync(Cy(scene, "toolbar-publish"), pauseAfter: 200);
            await Cy(scene, "published-version").WaitForAsync(new() { Timeout = 60000 });
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbar-run"), pauseAfter: 200);
            await Cy(scene, "runs-tab").WaitForAsync();
            var badge = Cy(scene, "runs-tab").GetByText("Succeeded").First;
            await scene.OffCameraAsync(async () => await badge.WaitForAsync(new() { Timeout = 120000 }), 800);
            await scene.HighlightAsync(badge);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, Cy(scene, "run-page").First, page.Locator("h5:has-text('Delivered')").First);
            await scene.HoldAsync(1.5);
            await RingAsync(scene, page.Locator("h5:has-text('Delivered')").First.Locator("xpath=following::li[1]"), 4.0);
        });

        await Task.Delay(3000);
    }

    private static async Task CheckAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems/Product");
        await ExpandMenuAsync(scene, "/Admin/Contents/ContentItems/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("check-1", async () =>
        {
            await scene.HoldAsync(2.0);
            await RingAsync(scene, page.Locator("ul.list-group").First, 3.0);
        });

        await scene.StepAsync("check-2", async () =>
        {
            var link = page.Locator("a:has-text('Task chair')").First;
            await GoAsync(scene, link, page.Locator("input[value='Task chair']"));
        });

        await scene.StepAsync("again-1", async () =>
        {
            await scene.NavigateAsync("/Admin/DataPipelines");
            var row = page.Locator("li[data-pipeline-id]").Filter(new() { HasText = "Import products" }).First;
            await Assertions.Expect(row).ToContainTextAsync("Succeeded", new() { Timeout = 60000 });
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 300);
            await scene.HoldAsync(1.2);
            await scene.ClickAsync(row.Locator("a.dropdown-item:has-text('Run now')"), pauseAfter: 100);
        });

        await scene.StepAsync("again-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await page.WaitForURLAsync("**/Admin/DataPipelines/Run/**", new() { Timeout = 60000 });
                await Assertions.Expect(page.Locator(".badge:has-text('Succeeded')").First).ToBeVisibleAsync(new() { Timeout = 120000 });
            }, 600);

            var delivered = page.Locator("h5:has-text('Delivered')").First;
            await delivered.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(delivered.Locator("xpath=following::li[1]"));
        });

        await Task.Delay(3000);
    }
}

using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the data-pipelines-overview video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-create", CreateAsync),
        ("04-build", BuildAsync),
        ("05-run", RunAsync),
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

    private static async Task AtAsync(Stopwatch watch, int milliseconds)
    {
        var remaining = milliseconds - (int)watch.ElapsedMilliseconds;

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }
    }

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

    private static async Task OpenMenuAsync(Scene scene, string href, ILocator ready = null)
        => await GoAsync(scene, await ExpandMenuAsync(scene, href), ready);

    // Moves to a link or button, clicks it and cuts the page load out of the video.
    private static async Task GoAsync(Scene scene, ILocator link, ILocator ready = null)
    {
        await scene.MoveToAsync(link);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(async () => await link.ClickAsync());

            if (ready is not null)
            {
                await ready.WaitForAsync(new() { Timeout = 90000, State = WaitForSelectorState.Visible });
            }
        }, 450);
    }

    private static ILocator Cy(Scene scene, string name) => scene.Page.Locator($"[data-cy='{name}']");

    // Rings a target, holds, then clears.
    private static async Task RingAsync(Scene scene, ILocator target, double seconds, string caption = null)
    {
        await target.ScrollIntoViewIfNeededAsync();
        await scene.HighlightAsync(target, caption);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    // Opens the designer of the pipeline from the Pipelines page, with the menu collapsed (outside the capture).
    private static async Task OpenDesignerAsync(Scene scene, bool collapseToolbox, int nodes)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/DataPipelines");
        await page.Locator(".leftbar-compactor").ClickAsync();
        await scene.NavigateAsync(await page.Locator("a:has-text('Blog posts')").First.GetAttributeAsync("href"));
        await Cy(scene, "start-picker").Or(page.Locator(".dpd-node").First).First.WaitForAsync(new() { Timeout = 60000 });

        if (nodes > 0)
        {
            await Assertions.Expect(page.Locator(".dpd-node")).ToHaveCountAsync(nodes, new() { Timeout = 60000 });
        }

        if (collapseToolbox)
        {
            await Cy(scene, "toolbox-collapse").ClickAsync();
        }
    }

    private const string SeedRecipe = """
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "A weekend at the lake", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "A weekend at the lake" },
          "MarkdownBodyPart": { "Markdown": "Two quiet days by the water, with a book and a small boat." },
          "AutoroutePart": { "Path": "blog/weekend-at-the-lake" }
        },
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "Ten tips for a tidy workshop", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Ten tips for a tidy workshop" },
          "MarkdownBodyPart": { "Markdown": "Small habits that keep every tool where it belongs." },
          "AutoroutePart": { "Path": "blog/tidy-workshop" }
        },
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "Spring menu at the harbor cafe", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Spring menu at the harbor cafe" },
          "MarkdownBodyPart": { "Markdown": "The dishes we cook while the mornings are cool." },
          "AutoroutePart": { "Path": "blog/spring-menu" }
        },
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "Notes from a small orchard", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Notes from a small orchard" },
          "MarkdownBodyPart": { "Markdown": "Pruning, picking and a short list of lessons." },
          "AutoroutePart": { "Path": "blog/small-orchard" }
        }
      ]
    }
  ]
}
""";

    // Adds four invented blog posts to the one of the recipe (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", SeedRecipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000);
        Console.WriteLine("Seeded four posts.");
        await context.CloseAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        page.SetDefaultNavigationTimeout(120000);
        var box = page.Locator("#search-box");
        var enable = page.Locator("#btn-enable-OrchardCore_DataPipelines");

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Features", box);
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("data pipelines", new() { Delay = 30 });
            await enable.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.MoveToAsync(enable);
            await AtAsync(watch, Math.Max(300, (int)(durations["enable-2"] * 1000) - 4300));
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await enable.ClickAsync());
                await page.Locator("#btn-disable-OrchardCore_DataPipelines").WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 120000 });
            }, 450);

            var pipelines = await ExpandMenuAsync(scene, "/Admin/DataPipelines");
            var history = page.Locator("#left-nav a[href$='/Admin/DataPipelines/Runs']").First;
            await RingAsync(scene, pipelines, 1.0);
            await RingAsync(scene, history, 1.0);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DataPipelines");
        await scene.Page.Locator(".leftbar-compactor").ClickAsync();
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await RingAsync(scene, page.Locator(".list-group .alert-info").First, 2.0);
            await RingAsync(scene, page.Locator("a.create").First, 1.2);
            await GoAsync(scene, page.Locator("a.create").First, page.Locator("input[name='Name']"));
        });

        await scene.StepAsync("create-2", async () =>
        {
            var name = page.Locator("input[name='Name']");
            await scene.ClickAsync(name, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Blog posts", new() { Delay = 50 });
            await GoAsync(scene, page.Locator("button.save").First, Cy(scene, "start-picker"));
        });

        await scene.StepAsync("create-3", async () =>
        {
            var scale = Math.Min(1.0, (durations["create-3"] - 1.0) / 13.0);
            await RingAsync(scene, Cy(scene, "toolbox-sheet"), 6 * scale);
            await RingAsync(scene, Cy(scene, "start-picker"), 3 * scale);
            await RingAsync(scene, Cy(scene, "designer-panel").Locator("ul[role='tablist']").First, 4 * scale);
        });
    }

    private static async Task BuildAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene, collapseToolbox: true, nodes: 0);
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var sources = page.Locator("nav[aria-label='Data sources']");
        var form = Cy(scene, "panel-step-form");

        await scene.StepAsync("build-1", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbar-add-data-source"), pauseAfter: 200);
            await page.Locator("[data-cy^='data-set-Contents::']").First.WaitForAsync();
            await scene.HoldAsync(2.0);
            await scene.ClickAsync(sources.Locator("button:has-text('Content items')").First, pauseAfter: 200);
            await scene.HoldAsync(1.4);
            await scene.ClickAsync(sources.Locator("button:has-text('Users')").First, pauseAfter: 200);
            await scene.HoldAsync(1.0);
            await scene.ClickAsync(sources.Locator("button:has-text('Queries')").First, pauseAfter: 200);
        });

        await scene.StepAsync("build-2", async () =>
        {
            await scene.ClickAsync(sources.Locator("button:has-text('Content items')").First, pauseAfter: 200);
            await scene.ClickAsync(Cy(scene, "data-set-filter"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Blog Post", new() { Delay = 40 });
            await scene.ClickAsync(Cy(scene, "data-set-Contents::BlogPost").Locator("[data-cy='data-set-choose']"), pauseAfter: 200);
            await page.Locator(".dpd-node:has-text('Read a data source')").First.WaitForAsync();
            await Cy(scene, "step-panel").WaitForAsync();

            foreach (var label in new[] { "Fields to read", "Filters", "Maximum number of rows" })
            {
                await RingAsync(scene, form.Locator($"label:has-text('{label}')").First, 1.3);
            }
        });

        await scene.StepAsync("build-3", async () =>
        {
            await scene.ClickAsync(Cy(scene, "step-tab-data"), pauseAfter: 200);
            await scene.ClickAsync(Cy(scene, "preview-run"), pauseAfter: 200);
            await page.Locator(".dpd-preview-table").First.WaitForAsync(new() { Timeout = 60000 });
            await scene.HoldAsync(2.5);
            await scene.ClickAsync(Cy(scene, "step-tab-fields"), pauseAfter: 200);
            await Cy(scene, "fields-tab").WaitForAsync();
        });

        await scene.StepAsync("build-4", async () =>
        {
            await scene.ClickAsync(page.Locator(".dpd-node:has-text('Read a data source') [data-port-side='output']").First, pauseAfter: 200);
            await Cy(scene, "quick-add").WaitForAsync();
            await scene.HoldAsync(2.5);
            await scene.ClickAsync(Cy(scene, "quick-add-CreateFile"), pauseAfter: 200);
            await page.Locator(".dpd-node:has-text('Create a file')").First.WaitForAsync();
        });

        await scene.StepAsync("build-5", async () =>
        {
            await RingAsync(scene, form.Locator("label:has-text('Format')").First, 1.8);
            var file = form.Locator("input[name$='FileName']").First;
            await scene.ClickAsync(file, pauseAfter: 100);
            await file.FillAsync(string.Empty);
            await page.Keyboard.TypeAsync("blog-posts", new() { Delay = 40 });
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(file);
        });

        await scene.StepAsync("build-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator(".dpd-node:has-text('Create a file') [data-port-side='output']").First, pauseAfter: 200);
            await Cy(scene, "quick-add").WaitForAsync();
            await scene.HoldAsync(2.5);
            await scene.ClickAsync(Cy(scene, "quick-add-SaveToMedia"), pauseAfter: 200);
            await page.Locator(".dpd-node:has-text('Save to the media library')").First.WaitForAsync();
            await RingAsync(scene, form.Locator("label:has-text('Folder')").First, 1.6);
            var replace = form.Locator("label:has-text('Replace a file with the same name')").First;
            await replace.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(replace);
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(10.0);
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene, collapseToolbox: true, nodes: 3);
        await scene.Page.Locator("[title='Fit to the steps'], [aria-label='Fit to the steps']").First.ClickAsync(new() { Timeout = 10000 });
        await Task.Delay(1000);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HoldAsync(Math.Max(1.0, durations["run-1"] * 0.35));
            await scene.ClickAsync(Cy(scene, "panel-tab-issues"), pauseAfter: 200);
            await scene.HoldAsync(Math.Max(1.0, durations["run-1"] * 0.25));
            await scene.ClickAsync(Cy(scene, "toolbar-publish"), pauseAfter: 200);
            await Cy(scene, "published-version").WaitForAsync(new() { Timeout = 60000 });
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbar-run"), pauseAfter: 200);
            await Cy(scene, "runs-tab").WaitForAsync();
            await scene.OffCameraAsync(async () =>
            {
                await Assertions.Expect(page.Locator(".dpd-run-head .badge").First).ToContainTextAsync("Succeeded", new() { Timeout = 120000 });
            }, 1500);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await RingAsync(scene, page.Locator(".dpd-run-head").First, 2.2);

            for (var i = 0; i < 3; i++)
            {
                await RingAsync(scene, Cy(scene, "run-badge").Nth(i), 1.3);
            }
        });

        await scene.StepAsync("run-4", async () =>
        {
            await GoAsync(scene, Cy(scene, "run-page").First, page.Locator("h5:has-text('Steps')").First);

            foreach (var heading in new[] { "Steps", "Delivered", "Log" })
            {
                await RingAsync(scene, page.Locator($"h5:has-text('{heading}')").First, 1.8);
            }
        });

        await scene.StepAsync("run-5", async () =>
        {
            await GoAsync(scene, page.Locator("nav[aria-label='Breadcrumb'] a:has-text('Run History')").First, page.GetByText("The runs of Blog posts").First);
            await scene.HoldAsync(3.5);
            await GoAsync(scene, page.Locator("a:has-text('See every pipeline')").First);
        });

        await scene.StepAsync("run-6", async () =>
        {
            await GoAsync(scene, page.Locator("nav[aria-label='Breadcrumb'] a:has-text('Data Pipelines')").First, page.Locator("li[data-pipeline-id]").First);
            await RingAsync(scene, page.Locator("li[data-pipeline-id] .badge[title='The published version runs.']").First, 1.3);
            await RingAsync(scene, page.Locator("li[data-pipeline-id] a.badge[title='Last run']").First, 1.3);
            await scene.ClickAsync(page.Locator("li[data-pipeline-id] button.actions").First, pauseAfter: 300);
        });
    }
}

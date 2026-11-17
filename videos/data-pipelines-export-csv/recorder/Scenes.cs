using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the data-pipelines-export-csv video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-create", CreateAsync),
        ("03-source", SourceAsync),
        ("04-file", FileAsync),
        ("05-deliver", DeliverAsync),
        ("06-publish", PublishAsync),
        ("08-run", RunAsync),
        ("09-media", MediaAsync),
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

    private static ILocator Node(Scene scene, string text) => scene.Page.Locator($".dpd-node:has-text('{text}')").First;

    private static ILocator Form(Scene scene) => Cy(scene, "panel-step-form");

    // Rings a target, holds, then clears.
    private static async Task RingAsync(Scene scene, ILocator target, double seconds, string caption = null)
    {
        await target.ScrollIntoViewIfNeededAsync();
        await Task.Delay(500);
        await scene.HighlightAsync(target, caption);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    // Opens the designer of the pipeline from the list, with the menu collapsed and the Transforms group of the toolbox closed (outside the capture).
    private static async Task OpenDesignerAsync(Scene scene, bool collapseTransforms = true, bool fit = false)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/DataPipelines");
        await page.Locator(".leftbar-compactor").ClickAsync();
        await scene.NavigateAsync(await page.Locator("a:has-text('Blog posts export')").First.GetAttributeAsync("href"));
        await Cy(scene, "designer-toolbar").WaitForAsync(new() { Timeout = 60000 });

        if (collapseTransforms)
        {
            await Cy(scene, "toolbox-category-Transform").ClickAsync();
        }

        if (fit)
        {
            await page.Locator("[title='Fit to the steps'], [aria-label='Fit to the steps']").First.ClickAsync(new() { Timeout = 10000 });
            await Task.Delay(800);
        }
    }

    // Clicks an output port, filters the steps of the quick-add panel and chooses one.
    private static async Task AddStepAsync(Scene scene, ILocator port, string filter, string choice)
    {
        await scene.ClickAsync(port, pauseAfter: 250);
        var panel = Cy(scene, "quick-add");
        await panel.WaitForAsync();
        await scene.Page.Keyboard.TypeAsync(filter, new() { Delay = 40 });
        await scene.ClickAsync(Cy(scene, choice), pauseAfter: 250);
    }

    private static string TitleOf(string title) => title;

    private const string SeedRecipe = """
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {"ContentItemId": "4624xkvlhtri46pncxab3e9gsu", "ContentItemVersionId": "4c8z8sw5gvllpwmqyyy282dr1g", "ContentType": "BlogPost", "DisplayText": "Welcome to the Orchard Notes blog", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-01-12T09:00:00Z", "ModifiedUtc": "2026-01-12T09:00:00Z", "PublishedUtc": "2026-01-12T09:00:00Z", "TitlePart": {"Title": "Welcome to the Orchard Notes blog"}, "AutoroutePart": {"Path": "blog/welcome-to-the-orchard-notes-blog"}, "MarkdownBodyPart": {"Markdown": "Sample text for the welcome to the orchard notes blog post."}, "BlogPost": {"Subtitle": {"Text": "A first look at what we plan to share"}}},
        {"ContentItemId": "4zb340af7op63lnn8q2ky0kcxi", "ContentItemVersionId": "45vl1gfpsgxkfw0oe3ptz5rzqu", "ContentType": "BlogPost", "DisplayText": "How we prune apple trees in winter", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-01-26T10:30:00Z", "ModifiedUtc": "2026-01-26T10:30:00Z", "PublishedUtc": "2026-01-26T10:30:00Z", "TitlePart": {"Title": "How we prune apple trees in winter"}, "AutoroutePart": {"Path": "blog/how-we-prune-apple-trees-in-winter"}, "MarkdownBodyPart": {"Markdown": "Sample text for the how we prune apple trees in winter post."}, "BlogPost": {"Subtitle": {"Text": "Three cuts that keep the canopy open"}}},
        {"ContentItemId": "4h9ahcmlt753uhtn3huoubdm7y", "ContentItemVersionId": "4add0ymvezb9md6iv4qovnflfu", "ContentType": "BlogPost", "DisplayText": "Choosing rootstock for a small garden", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-02-09T08:15:00Z", "ModifiedUtc": "2026-02-09T08:15:00Z", "PublishedUtc": "2026-02-09T08:15:00Z", "TitlePart": {"Title": "Choosing rootstock for a small garden"}, "AutoroutePart": {"Path": "blog/choosing-rootstock-for-a-small-garden"}, "MarkdownBodyPart": {"Markdown": "Sample text for the choosing rootstock for a small garden post."}, "BlogPost": {"Subtitle": {"Text": "Size, vigor and soil in plain terms"}}},
        {"ContentItemId": "45rqhvi5zyorv2l1zomxgif49g", "ContentItemVersionId": "43jtsoc2tiy9xm3f661sovm6kj", "ContentType": "BlogPost", "DisplayText": "Our spring planting calendar", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-02-23T14:00:00Z", "ModifiedUtc": "2026-02-23T14:00:00Z", "PublishedUtc": "2026-02-23T14:00:00Z", "TitlePart": {"Title": "Our spring planting calendar"}, "AutoroutePart": {"Path": "blog/our-spring-planting-calendar"}, "MarkdownBodyPart": {"Markdown": "Sample text for the our spring planting calendar post."}, "BlogPost": {"Subtitle": {"Text": "What goes in the ground, and when"}}},
        {"ContentItemId": "4o571hplvy8zd3r1b8ip56lqh3", "ContentItemVersionId": "4eci9vf3sermb07xfoq7pf0p34", "ContentType": "BlogPost", "DisplayText": "Pollination: bees, wind and neighbors", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-03-09T11:45:00Z", "ModifiedUtc": "2026-03-09T11:45:00Z", "PublishedUtc": "2026-03-09T11:45:00Z", "TitlePart": {"Title": "Pollination: bees, wind and neighbors"}, "AutoroutePart": {"Path": "blog/pollination-bees-wind-and-neighbors"}, "MarkdownBodyPart": {"Markdown": "Sample text for the pollination: bees, wind and neighbors post."}, "BlogPost": {"Subtitle": {"Text": "Why one tree is rarely enough"}}},
        {"ContentItemId": "424yynt72vgtroxs4zbb5hkd7f", "ContentItemVersionId": "4el2hs9uu3awrvu168cwy1gsts", "ContentType": "BlogPost", "DisplayText": "Harvest day volunteer guide", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-03-23T09:30:00Z", "ModifiedUtc": "2026-03-23T09:30:00Z", "PublishedUtc": "2026-03-23T09:30:00Z", "TitlePart": {"Title": "Harvest day volunteer guide"}, "AutoroutePart": {"Path": "blog/harvest-day-volunteer-guide"}, "MarkdownBodyPart": {"Markdown": "Sample text for the harvest day volunteer guide post."}, "BlogPost": {"Subtitle": {"Text": "Tasks, times and what to bring"}}},
        {"ContentItemId": "4ywpgvjryist72jmzwyt5i490t", "ContentItemVersionId": "4dazvwie91u72c5df1tj8s1es0", "ContentType": "BlogPost", "DisplayText": "Cider pressing notes", "Latest": true, "Published": false, "Author": "admin", "CreatedUtc": "2026-04-06T13:00:00Z", "ModifiedUtc": "2026-04-06T13:00:00Z", "TitlePart": {"Title": "Cider pressing notes"}, "AutoroutePart": {"Path": "blog/cider-pressing-notes"}, "MarkdownBodyPart": {"Markdown": "Sample text for the cider pressing notes post."}, "BlogPost": {"Subtitle": {"Text": "Work in progress"}}},
        {"ContentItemId": "4elug5k3unz97xicdj6dshcp2n", "ContentItemVersionId": "4va4jezg8rm4na7jnsux87yvb3", "ContentType": "BlogPost", "DisplayText": "Winter storage ideas", "Latest": true, "Published": false, "Author": "admin", "CreatedUtc": "2026-04-20T15:20:00Z", "ModifiedUtc": "2026-04-20T15:20:00Z", "TitlePart": {"Title": "Winter storage ideas"}, "AutoroutePart": {"Path": "blog/winter-storage-ideas"}, "MarkdownBodyPart": {"Markdown": "Sample text for the winter storage ideas post."}, "BlogPost": {"Subtitle": {"Text": "Notes for a later post"}}}
      ]
    }
  ]
}
""";

    private const string FeatureRecipe = """
{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.DataPipelines" ] } ] }
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

    // Deletes the sample post of the recipe, imports the eight invented posts and enables Data Pipelines (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems?q=type%3ABlogPost");
        var row = page.Locator("li.list-group-item").Filter(new() { HasText = "Man must explore" }).First;
        await row.WaitForAsync();
        await row.Locator("button.dropdown-toggle, .actions button").Last.ClickAsync();
        await row.Locator("a.dropdown-item", new() { HasTextString = "Delete" }).First.ClickAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);
        await ImportAsync(page, SeedRecipe);
        await ImportAsync(page, FeatureRecipe);
        await Task.Delay(8000);
        Console.WriteLine("Prepared the posts and the feature.");
        await context.CloseAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            var pipelines = await ExpandMenuAsync(scene, "/Admin/DataPipelines");
            var history = page.Locator("#left-nav a[href$='/Admin/DataPipelines/Runs']").First;
            await scene.HoldAsync(0.6);
            await RingAsync(scene, pipelines, 1.2);
            await RingAsync(scene, history, 1.2);
            await GoAsync(scene, pipelines, page.Locator("a.create").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First, page.Locator("input[name='Name']"));
            var name = page.Locator("input[name='Name']");
            await scene.ClickAsync(name, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Blog posts export", new() { Delay = 40 });
            await GoAsync(scene, page.Locator("button.save").First, Cy(scene, "designer-toolbar"));
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbox-category-Transform"), pauseAfter: 300);
            var scale = Math.Min(1.0, (durations["create-3"] - 1.0) / 9.0);
            await RingAsync(scene, Cy(scene, "designer-toolbox"), 3.5 * scale);
            await RingAsync(scene, Cy(scene, "designer-canvas"), 2.5 * scale);
            await RingAsync(scene, Cy(scene, "designer-panel"), 3.0 * scale);
        });

        await scene.StepAsync("create-4", async () =>
        {
            var tab = Cy(scene, "panel-tab-pipeline");

            if (await tab.CountAsync() > 0)
            {
                await scene.ClickAsync(tab, pauseAfter: 200);
            }

            var description = Cy(scene, "panel-settings-form").GetByLabel("Description");
            await scene.ClickAsync(description, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Exports the published blog posts to a CSV file.", new() { Delay = 35 });
            await page.Keyboard.PressAsync("Tab");
            await Assertions.Expect(Cy(scene, "save-status")).ToContainTextAsync("Draft saved", new() { Timeout = 30000 });
            await scene.HighlightAsync(Cy(scene, "save-status"));
        });
    }

    private static async Task SourceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var sources = page.Locator("nav[aria-label='Data sources']");

        await scene.StepAsync("source-1", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbar-add-data-source"), pauseAfter: 300);
            await page.Locator("[data-cy^='data-set-Contents::']").First.WaitForAsync();
            await scene.HoldAsync(2.0);
            await scene.HighlightAsync(sources);
        });

        await scene.StepAsync("source-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Cy(scene, "data-set-filter"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("blog", new() { Delay = 50 });
            var add = Cy(scene, "data-set-Contents::BlogPost").Locator("[data-cy='data-set-choose']");
            await scene.MoveToAsync(add);
            await AtAsync(watch, Math.Max(500, (int)(durations["source-2"] * 1000) - 1200));
            await scene.ClickAsync(add, pauseAfter: 100);
        });

        await scene.StepAsync("source-3", async () =>
        {
            await Node(scene, "Read a data source").WaitForAsync();
            await Cy(scene, "step-panel").WaitForAsync();
            await RingAsync(scene, Node(scene, "Read a data source"), 1.6);
            await scene.HighlightAsync(Form(scene).Locator("label:has-text('Data set')").First);
        });

        await scene.StepAsync("source-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FieldsToReadAsync(scene);
        });

        await scene.StepAsync("source-5", async () =>
        {
            await FilterRowAsync(scene);
        });

        await scene.StepAsync("source-6", async () =>
        {
            await scene.ClickAsync(Cy(scene, "step-tab-data"), pauseAfter: 200);
            await scene.ClickAsync(Cy(scene, "preview-run"), pauseAfter: 200);
            await page.Locator("table.dpd-preview-table").First.WaitForAsync(new() { Timeout = 60000 });
        });
    }

    // Chooses the four fields of the narration in the Fields to read control (a Bootstrap select with a search box).
    private static async Task FieldsToReadAsync(Scene scene)
    {
        var page = scene.Page;
        var form = Form(scene);
        var button = form.Locator("button[data-id='DataPipelineStep_Fields']");
        await button.ScrollIntoViewIfNeededAsync();
        await scene.ClickAsync(button, pauseAfter: 250);
        var search = form.Locator(".bs-searchbox input");

        foreach (var word in new[] { "Title", "Subtitle", "Author", "Created" })
        {
            await search.FillAsync(string.Empty);
            await search.FocusAsync();
            await page.Keyboard.TypeAsync(word, new() { Delay = 40 });
            var option = form.Locator("a[role='option']:visible").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*" + word + @" \(") }).First;

            try
            {
                await option.WaitForAsync(new() { Timeout = 8000 });
            }
            catch (Exception)
            {
                await page.ScreenshotAsync(new() { Path = "C:/ocv/s4b/fail.png" });
                throw;
            }

            await option.ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(option, pauseAfter: 200);
        }

        await page.Keyboard.PressAsync("Escape");
    }

    // Adds the filter Published equals Yes.
    private static async Task FilterRowAsync(Scene scene)
    {
        var form = Form(scene);
        var add = form.Locator("[data-dp-add]").First;
        await add.ScrollIntoViewIfNeededAsync();
        await scene.ClickAsync(add, pauseAfter: 250);
        var row = form.Locator("[data-dp-filter-row]").First;
        await scene.SelectAsync(row.GetByLabel("Field"), "Published");
        await scene.SelectAsync(row.Locator("[data-dp-filter-boolean]"), "Yes");
        await scene.Page.Keyboard.PressAsync("Tab");
    }

    private static async Task FileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("file-1", async () =>
        {
            var port = Node(scene, "Read a data source").Locator("button[data-port-side='output']").First;
            await scene.HighlightAsync(port, "Rows");
            await scene.HoldAsync(1.2);
            await scene.ClearHighlightsAsync();
            await AddStepAsync(scene, port, "file", "quick-add-CreateFile");
            await Node(scene, "Create a file").WaitForAsync();
        });

        await scene.StepAsync("file-2", async () =>
        {
            await RingAsync(scene, Form(scene).GetByLabel("Format").First, 1.5);
            var file = Form(scene).Locator("input[name$='FileName']").First;
            await file.ClickAsync(new() { ClickCount = 3 });
            await page.Keyboard.TypeAsync("blog-posts", new() { Delay = 40 });
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(file);
        });

        await scene.StepAsync("file-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var first = Form(scene).Locator("label:has-text('Start with a row of column names')").First;
            await RingAsync(scene, first, 1.5);
            var second = Form(scene).Locator("#DataPipelineStep_UseDisplayNames");
            await second.ScrollIntoViewIfNeededAsync();
            await scene.HoldAsync(0.5);
            await scene.MoveToAsync(second);
            await second.CheckAsync();
            await scene.HoldAsync(0.3);
            await Assertions.Expect(Form(scene).GetByLabel("Name the columns with the labels of the fields")).ToBeCheckedAsync();
        });

        await scene.StepAsync("file-4", async () =>
        {
            var delimiter = Form(scene).GetByLabel("CSV delimiter");
            await delimiter.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            await scene.HighlightAsync(delimiter);
            await scene.HighlightAsync(Form(scene).GetByText("A comma by default").First);
        });

        await scene.StepAsync("file-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Cy(scene, "step-tab-data"), pauseAfter: 200);
            await scene.ClickAsync(Cy(scene, "preview-run"), pauseAfter: 200);
            await Cy(scene, "preview-files").WaitForAsync(new() { Timeout = 60000 });
        });
    }

    private static async Task DeliverAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("deliver-1", async () =>
        {
            var port = Node(scene, "Create a file").Locator("button[data-port-side='output']").First;
            await scene.HighlightAsync(port, "Files");
            await scene.HoldAsync(1.2);
            await scene.ClearHighlightsAsync();
            await AddStepAsync(scene, port, "media", "quick-add-SaveToMedia");
            await Node(scene, "Save to the media library").WaitForAsync();
        });

        await scene.StepAsync("deliver-2", async () =>
        {
            var folder = Form(scene).GetByLabel("Folder");
            await scene.ClickAsync(folder, pauseAfter: 100);
            await page.Keyboard.TypeAsync("exports", new() { Delay = 50 });
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("deliver-3", async () =>
        {
            var warning = Form(scene).Locator("[data-dp-rejected-extensions]").First;
            await warning.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            await scene.HighlightAsync(warning);
        });

        await scene.StepAsync("deliver-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var replace = Form(scene).GetByLabel("Replace a file with the same name");
            await replace.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            await scene.HighlightAsync(replace);
            await scene.HighlightAsync(Form(scene).GetByText("Otherwise the file is saved under a name with a number appended.").First);
        });

        await scene.StepAsync("deliver-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Cy(scene, "step-tab-data"), pauseAfter: 200);
            await scene.ClickAsync(Cy(scene, "preview-run"), pauseAfter: 200);
            await page.GetByText("Destinations are never executed by a preview").First.WaitForAsync(new() { Timeout = 60000 });
        });
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene, fit: true);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("publish-1", async () =>
        {
            await scene.ClickAsync(Cy(scene, "panel-tab-issues"), pauseAfter: 200);
            await page.GetByText("No issues: the pipeline can be published.").First.WaitForAsync();
        });

        await scene.StepAsync("publish-2", async () =>
        {
            await scene.HighlightAsync(Cy(scene, "toolbar-publish"));
            await scene.HoldAsync(0.5);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Cy(scene, "toolbar-publish"), pauseAfter: 200);
            await Cy(scene, "published-version").WaitForAsync(new() { Timeout = 60000 });
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenDesignerAsync(scene, fit: true);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAsync(Cy(scene, "toolbar-run"), pauseAfter: 200);
            await Cy(scene, "panel-tab-runs").WaitForAsync();
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await Assertions.Expect(Cy(scene, "runs-tab").Locator(".badge").First).ToContainTextAsync("Succeeded", new() { Timeout = 150000 });
            }, 600);

            var panel = Cy(scene, "runs-tab");
            await RingAsync(scene, Node(scene, "Read a data source").Locator("[data-cy='run-badge']").First, 1.2);
            await RingAsync(scene, Node(scene, "Create a file").Locator("[data-cy='run-badge']").First, 1.2);
            await RingAsync(scene, Node(scene, "Save to the media library").Locator("[data-cy='run-badge']").First, 1.2);
            await RingAsync(scene, panel.GetByText("Saved 'exports/blog-posts.csv'").First, 1.5);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await GoAsync(scene, Cy(scene, "run-page").First, page.Locator("h5:has-text('Steps')").First);

            foreach (var heading in new[] { "Steps", "Delivered", "Log" })
            {
                await RingAsync(scene, page.Locator($"h5:has-text('{heading}')").First, 1.6);
            }

            await RingAsync(scene, page.Locator("nav[aria-label='Breadcrumb'] a:has-text('Run History')").First, 1.6);
        });
    }

    private static async Task MediaAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("media-1", async () =>
        {
            var library = await ExpandMenuAsync(scene, "/Admin/Media");
            await GoAsync(scene, library, page.Locator("text=exports").First);
            await scene.ClickAsync(page.GetByText("exports", new() { Exact = true }).First, pauseAfter: 300);
            await page.GetByText("blog-posts.csv").First.WaitForAsync(new() { Timeout = 30000 });
        });

        await scene.StepAsync("media-2", async () =>
        {
            await scene.HighlightAsync(page.GetByText("blog-posts.csv").First, null, 16);
        });
    }
}

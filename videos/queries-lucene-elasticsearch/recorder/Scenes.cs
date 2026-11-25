using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the queries-lucene-elasticsearch video: see recording-plan.md.</summary>
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
        ("03-params", ParamsAsync),
        ("04-save", SaveAsync),
        ("06-elastic", ElasticAsync),
    ];

    private const string LuceneTemplate = "{\n  \"query\": {\n    \"bool\": {\n      \"must\": [\n        { \"term\": { \"Content.ContentItem.ContentType\": \"BlogPost\" } },\n        { \"term\": { \"Content.ContentItem.Published\": \"true\" } }\n      ]\n    }\n  },\n  \"sort\": [\n    { \"Content.ContentItem.CreatedUtc\": { \"order\": \"desc\" } }\n  ],\n  \"size\": {{ limit | default: 3 }}\n}";

    private const string ElasticTemplate = "{\n  \"query\": {\n    \"bool\": {\n      \"must\": [\n        { \"term\": { \"Content.ContentItem.ContentType\": \"BlogPost\" } },\n        { \"term\": { \"Content.ContentItem.Published\": true } }\n      ]\n    }\n  },\n  \"sort\": [\n    { \"Content.ContentItem.CreatedUtc\": \"desc\" }\n  ],\n  \"size\": {{ limit | default: 3 }}\n}";

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
                await scene.ClickAsync(caption, pauseAfter: 250);
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
                await ready.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Visible });
            }
        }, 450);
    }

    private static async Task RingAsync(Scene scene, ILocator target, double seconds)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    // Sets the contents of the n-th CodeMirror editor of the page (they re-indent what is typed).
    private static async Task SetCmAsync(Scene scene, int index, string text)
    {
        await scene.Page.EvaluateAsync("([i, t]) => document.querySelectorAll('.CodeMirror')[i].CodeMirror.setValue(t)", new object[] { index, text });
        await scene.PauseAsync(1200);
    }

    private static Task ToTopAsync(Scene scene) => scene.Page.EvaluateAsync("window.scrollTo({ top: 0, behavior: 'smooth' })");

    private static ILocator RunQuery(Scene scene) => scene.Page.Locator("button:text-is('Run Query')").First;

    private static ILocator Rows(Scene scene) => scene.Page.Locator("table.table tbody tr");

    private static ILocator Wrapper(Scene scene, string label) => scene.Page.Locator($".ocat-wrapper:has(> label:text-is('{label}'))");

    private const string PostsRecipe = """
{
  "steps": [
    {
      "name": "content",
      "Data": [
        { "ContentItemId": "4a1harbormorning000000000001", "ContentType": "BlogPost", "DisplayText": "A quiet morning at the harbor", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-09-01T08:00:00Z", "ModifiedUtc": "2026-09-01T08:00:00Z", "PublishedUtc": "2026-09-01T08:00:00Z", "TitlePart": { "Title": "A quiet morning at the harbor" }, "AutoroutePart": { "Path": "blog/a-quiet-morning-at-the-harbor" }, "MarkdownBodyPart": { "Markdown": "The fishing boats leave before sunrise, and the town wakes to the sound of their engines." } },
        { "ContentItemId": "4a2trailsearlystart00000002", "ContentType": "BlogPost", "DisplayText": "Five trails worth the early start", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-09-10T08:00:00Z", "ModifiedUtc": "2026-09-10T08:00:00Z", "PublishedUtc": "2026-09-10T08:00:00Z", "TitlePart": { "Title": "Five trails worth the early start" }, "AutoroutePart": { "Path": "blog/five-trails-worth-the-early-start" }, "MarkdownBodyPart": { "Markdown": "Each of these trails is at its best before nine in the morning." } },
        { "ContentItemId": "4a3weekwithoutphone00000003", "ContentType": "BlogPost", "DisplayText": "Notes from a week without a phone", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-09-20T08:00:00Z", "ModifiedUtc": "2026-09-20T08:00:00Z", "PublishedUtc": "2026-09-20T08:00:00Z", "TitlePart": { "Title": "Notes from a week without a phone" }, "AutoroutePart": { "Path": "blog/notes-from-a-week-without-a-phone" }, "MarkdownBodyPart": { "Markdown": "Seven days offline changed how I plan a trip." } },
        { "ContentItemId": "4a4lighthousekeeper00000004", "ContentType": "BlogPost", "DisplayText": "How the lighthouse keeper kept time", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-10-01T08:00:00Z", "ModifiedUtc": "2026-10-01T08:00:00Z", "PublishedUtc": "2026-10-01T08:00:00Z", "TitlePart": { "Title": "How the lighthouse keeper kept time" }, "AutoroutePart": { "Path": "blog/how-the-lighthouse-keeper-kept-time" }, "MarkdownBodyPart": { "Markdown": "Before radio signals, the keeper wound the clock by hand every night." } }
      ]
    }
  ]
}
""";

    private const string Types = "[ \"Article\", \"Blockquote\", \"Blog\", \"BlogPost\", \"Container\", \"Image\", \"LinkMenuItem\", \"Menu\", \"Page\", \"Paragraph\", \"RawHtml\" ]";

    private const string LuceneRecipe = """
{
  "steps": [
    { "name": "feature", "enable": [ "OrchardCore.Search", "OrchardCore.Lucene" ] },
    {
      "name": "CreateOrUpdateIndexProfile",
      "indexes": [
        {
          "IndexName": "Search", "ProviderName": "Lucene", "Type": "Content", "Name": "Search",
          "Properties": {
            "LuceneIndexMetadata": { "StoreSourceData": false, "AnalyzerName": "standardanalyzer" },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": TYPES, "Culture": "any" },
            "LuceneIndexDefaultQueryMetadata": { "DefaultVersion": "LUCENE_30", "QueryAnalyzerName": "standardanalyzer", "AllowLuceneQueries": false, "DefaultSearchFields": [ "Content.ContentItem.FullText" ] }
          },
          "IndexLatest": false, "Culture": "any", "IndexedContentTypes": TYPES
        }
      ]
    },
    { "name": "RebuildIndex", "includeAll": false, "indexNames": [ "Search" ] }
  ]
}
""";

    private const string ElasticRecipe = """
{
  "steps": [
    { "name": "feature", "enable": [ "OrchardCore.Elasticsearch" ] },
    {
      "name": "CreateOrUpdateIndexProfile",
      "indexes": [
        {
          "IndexName": "blogelastic", "ProviderName": "Elasticsearch", "Type": "Content", "Name": "blogelastic",
          "Properties": {
            "ElasticsearchIndexMetadata": { "StoreSourceData": true, "AnalyzerName": "standard" },
            "ElasticsearchDefaultQueryMetadata": { "QueryAnalyzerName": "standard", "DefaultSearchFields": [ "Content.ContentItem.FullText" ] },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": TYPES, "Culture": "any" }
          },
          "IndexLatest": false, "Culture": "any", "IndexedContentTypes": TYPES
        }
      ]
    },
    { "name": "RebuildIndex", "includeAll": false, "indexNames": [ "blogelastic" ] }
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

        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", recipe.Replace("TYPES", Types));
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(4000);
    }

    // Stage 1 (Lucene still disabled): four posts, then Lucene with the index Search. Stage 2 (Lucene enabled, Elasticsearch not): the index blogelastic.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        var response = await page.GotoAsync(BaseUrl + "/Admin/Lucene/query");
        var luceneOn = response is not null && response.Status == 200;
        string stage;

        if (!luceneOn)
        {
            await ImportAsync(page, PostsRecipe);
            await ImportAsync(page, LuceneRecipe);
            stage = "posts and Lucene";
        }
        else
        {
            await ImportAsync(page, ElasticRecipe);
            stage = "Elasticsearch";
        }

        await Task.Delay(8000);
        Console.WriteLine("Prepared: " + stage);
        await context.CloseAsync();
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            var link = await ExpandMenuAsync(scene, "/Admin/Lucene/query");
            await scene.HighlightAsync(link);
            await scene.HoldAsync(1.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, link, page.Locator("select#Id"));
        });

        await scene.StepAsync("run-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.SelectAsync(page.Locator("select#Id"), "Search");
            var editor = page.Locator(".CodeMirror").First;
            await scene.HoldAsync(Math.Max(0.5, durations["run-2"] - 2.6 - watch.Elapsed.TotalSeconds));
            await scene.HighlightAsync(editor);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, RunQuery(scene), Rows(scene).First);
            await scene.MoveToAsync(Rows(scene).First);
            await scene.HighlightAsync(page.Locator("text=/Found \\d+ result/").First);
        });
    }

    private static async Task ParamsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Lucene/query");
        await scene.Page.Locator("select#Id").SelectOptionAsync(new SelectOptionValue { Label = "Search" });
        await scene.Page.EvaluateAsync("t => document.querySelectorAll('.CodeMirror')[0].CodeMirror.setValue(t)", LuceneTemplate);
        await Task.Delay(1200);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("params-1", async () =>
        {
            await RingAsync(scene, page.Locator(".CodeMirror-line:has-text('limit')").First, 2.0);
            await GoAsync(scene, RunQuery(scene), Rows(scene).First);
            await scene.ScrollAsync(0, 600);
        });

        await scene.StepAsync("params-2", async () =>
        {
            await ToTopAsync(scene);
            await scene.HoldAsync(0.6);
            await SetCmAsync(scene, 1, "{ \"limit\": 2 }");
            await GoAsync(scene, RunQuery(scene), Rows(scene).First);
            await scene.ScrollAsync(0, 600);
        });
    }

    private static async Task SaveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Lucene/query");
        var page = scene.Page;
        await page.Locator("select#Id").SelectOptionAsync(new SelectOptionValue { Label = "Search" });
        await page.EvaluateAsync("t => document.querySelectorAll('.CodeMirror')[0].CodeMirror.setValue(t)", LuceneTemplate);
        await Task.Delay(1200);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.WhenAll(page.WaitForNavigationAsync(), RunQuery(scene).ClickAsync());
        await Rows(scene).First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("save-1", async () =>
        {
            var save = page.Locator("a:text-is('Save Query')").First;
            await scene.HighlightAsync(save);
            await scene.HoldAsync(1.2);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, save, Wrapper(scene, "Name"));
        });

        await scene.StepAsync("save-2", async () =>
        {
            var name = Wrapper(scene, "Name").Locator("input.form-control").First;
            await scene.ClickAsync(name, pauseAfter: 100);
            await page.Keyboard.TypeAsync("PublishedBlogPosts", new() { Delay = 40 });
            await scene.HighlightAsync(Wrapper(scene, "Name").Locator(".hint").First);
        });

        await scene.StepAsync("save-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var index = Wrapper(scene, "Index").Locator("select").First;
            await index.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            await scene.SelectAsync(index, "Search");
            var box = page.Locator(".form-check:has(label:text-is('Return Content Items')) input").First;
            await scene.ClickAsync(box, pauseAfter: 300);
            await scene.HighlightAsync(page.Locator(".form-check:has(label:text-is('Return Content Items'))").First);
        });

        await scene.StepAsync("save-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var save = page.Locator("button.btn.save").First;
            await save.ScrollIntoViewIfNeededAsync();
            await GoAsync(scene, save, page.Locator(".list-group-item:has-text('PublishedBlogPosts')"));
            await scene.HoldAsync(2.0);
            var run = page.Locator(".list-group-item:has-text('PublishedBlogPosts') a:text-is('Run')").First;
            await GoAsync(scene, run, Rows(scene).First);
            await scene.ScrollAsync(0, 600);
        });
    }

    private static async Task ElasticAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Elasticsearch/query");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("elastic-1", async () =>
        {
            await scene.SelectAsync(page.Locator("select#Id"), "blogelastic");
            await SetCmAsync(scene, 0, ElasticTemplate);
            await GoAsync(scene, RunQuery(scene), Rows(scene).First);
            await scene.ScrollAsync(0, 500);
        });

        await scene.StepAsync("elastic-2", async () =>
        {
            await GoAsync(scene, page.Locator("a:text-is('Save Query')").First, Wrapper(scene, "Name"));
            var name = Wrapper(scene, "Name").Locator("input.form-control").First;
            await scene.ClickAsync(name, pauseAfter: 100);
            await page.Keyboard.TypeAsync("PublishedBlogPostsElastic", new() { Delay = 40 });
        });

        await scene.StepAsync("elastic-3", async () =>
        {
            var index = Wrapper(scene, "Index").Locator("select").First;
            await index.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            await scene.SelectAsync(index, "blogelastic");
            await scene.ClickAsync(page.Locator(".form-check:has(label:text-is('Return Content Items')) input").First, pauseAfter: 300);
            var save = page.Locator("button.btn.save").First;
            await save.ScrollIntoViewIfNeededAsync();
            await GoAsync(scene, save, page.Locator(".list-group-item:has-text('PublishedBlogPostsElastic')"));
        });
    }
}

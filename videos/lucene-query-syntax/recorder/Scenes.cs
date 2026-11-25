using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the lucene-query-syntax video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-operators", OperatorsAsync),
        ("03-phrases", PhrasesAsync),
        ("04-wildcards", WildcardsAsync),
        ("05-fields", FieldsAsync),
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

    // Types a query into the search box, submits it, waits for the results and scrolls so that the box and the results are on screen.
    private static async Task SearchAsync(Scene scene, string query)
    {
        var page = scene.Page;
        var box = page.Locator("input[name='Terms']").First;
        await box.FillAsync(string.Empty);
        await scene.TypeAsync(box, query, 40);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(async () => await page.Keyboard.PressAsync("Enter"));
            await page.Locator("h2.post-title, .alert-warning").First.WaitForAsync(new() { Timeout = 30000 });
        }, 500);
        await scene.ScrollAsync(0, 420);
    }

    private static string Post(string title, string path, string subtitle, string markdown, string id, string date)
        => $"{{ \"ContentItemId\": \"{id}\", \"ContentType\": \"BlogPost\", \"DisplayText\": \"{title}\", \"Latest\": true, \"Published\": true, \"Author\": \"admin\", \"CreatedUtc\": \"{date}\", \"ModifiedUtc\": \"{date}\", \"PublishedUtc\": \"{date}\", \"TitlePart\": {{ \"Title\": \"{title}\" }}, \"MarkdownBodyPart\": {{ \"Markdown\": \"{markdown}\" }}, \"AutoroutePart\": {{ \"Path\": \"blog/{path}\" }}, \"BlogPost\": {{ \"Subtitle\": {{ \"Text\": \"{subtitle}\" }} }} }}";

    private static string PostsRecipe() =>
        "{ \"steps\": [ { \"name\": \"content\", \"Data\": [ "
        + string.Join(", ", new[]
        {
            Post("Pruning Quince Trees", "pruning-quince-trees", "A short guide for late winter", "Quince trees need light pruning in late winter. Cut out crossing branches and keep the center open so the fruit gets sun.", "4pqz8c7xw2m5n1r9t6v3k0h8jd", "2026-01-12T09:00:00Z"),
            Post("Pressing Cider at Home", "pressing-cider-at-home", "From the first crush to the first glass", "Cider starts with ripe apples and a clean press. Crush the apples, press the juice, and let it ferment for two weeks.", "7hd3k9f2s6b1x8m4w0q5n2e7ra", "2026-02-03T09:00:00Z"),
            Post("Apple Jelly Recipe", "apple-jelly-recipe", "Golden and firm", "Apple jelly is firm and golden. Simmer chopped apples with water, strain the juice, and boil it with sugar until it sets.", "2wn6t0j8c4v7z1p5g3y9k2b6ud", "2026-02-24T09:00:00Z"),
            Post("Apple Varieties for Cider", "apple-varieties-for-cider", "Mixing sharp and sweet", "Sharp and sweet apples balance a cider. Mix several varieties, and add a few quince for aroma.", "9ks1m5q7e3a0r8d2h6x4v1n9tf", "2026-03-17T09:00:00Z"),
            Post("Planting Fruit Trees in Autumn", "planting-fruit-trees-in-autumn", "Choosing the right spot", "Autumn is a good time to plant fruit trees. Dig a wide hole, water well, and stake the tree against wind.", "5bx0d4h8y2n6u1c7w3z9q5j4ma", "2026-04-07T09:00:00Z"),
            Post("Winter Pruning Checklist", "winter-pruning-checklist", "Before the buds open", "Winter pruning keeps fruit trees healthy. Remove dead wood first, then thin the crown. Clean your saw between trees.", "3rv8g2l6p0t4e9s1f5k7a3c8wh", "2026-05-12T09:00:00Z"),
        })
        + " ] } ] }";

    private const string SearchRecipe = """
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

    private const string Types = "[ \"Article\", \"Blockquote\", \"Blog\", \"BlogPost\", \"Container\", \"Image\", \"LinkMenuItem\", \"Menu\", \"Page\", \"Paragraph\", \"RawHtml\" ]";

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

    // Imports the six invented posts, then Lucene with the index Search, and makes it the default index (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await ImportAsync(page, PostsRecipe());
        await ImportAsync(page, SearchRecipe.Replace("TYPES", Types));
        await Task.Delay(8000);
        await page.GotoAsync(BaseUrl + "/Admin/Settings/search");
        await page.Locator("select[id$='DefaultIndexProfileName']").SelectOptionAsync(new SelectOptionValue { Label = "Search" });
        await page.Locator("button.save").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);
        Console.WriteLine("Prepared the posts and the index.");
        await context.CloseAsync();
    }

    private static async Task OperatorsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/search");
        await scene.StartCaptureAsync();

        await scene.StepAsync("op-1", async () => await SearchAsync(scene, "quince cider"));
        await scene.StepAsync("op-2", async () => await SearchAsync(scene, "quince AND cider"));
        await scene.StepAsync("op-3", async () => await SearchAsync(scene, "quince and cider"));
        await scene.StepAsync("op-4", async () => await SearchAsync(scene, "+quince cider"));
        await scene.StepAsync("op-5", async () => await SearchAsync(scene, "quince NOT pruning"));
    }

    private static async Task PhrasesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/search");
        await scene.StartCaptureAsync();

        await scene.StepAsync("phrase-1", async () => await SearchAsync(scene, "\"fruit trees\""));
        await scene.StepAsync("phrase-2", async () => await SearchAsync(scene, "\"pruning trees\"~2"));
    }

    private static async Task WildcardsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/search");
        await scene.StartCaptureAsync();

        await scene.StepAsync("wild-1", async () => await SearchAsync(scene, "qu?nce"));
        await scene.StepAsync("wild-2", async () => await SearchAsync(scene, "prun*"));
        await scene.StepAsync("wild-3", async () => await SearchAsync(scene, "*ince"));
        await scene.StepAsync("wild-4", async () => await SearchAsync(scene, "quinse~"));
        await scene.StepAsync("wild-5", async () => await SearchAsync(scene, "/pr.*ing/"));
    }

    private static async Task FieldsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/search");
        await scene.StartCaptureAsync();

        await scene.StepAsync("fields-2", async () => await SearchAsync(scene, "Content.ContentItem.DisplayText.Analyzed:quince"));
        await scene.StepAsync("fields-3", async () => await SearchAsync(scene, "Content.ContentItem.DisplayText.Analyzed:(fruit AND trees)"));
        await scene.StepAsync("fields-4", async () => await SearchAsync(scene, "Content.ContentItem.DisplayText.Normalized:[p TO r]"));
    }
}

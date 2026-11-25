using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the indexing-developer video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("04-settings", SettingsAsync),
        ("04-reset", ResetAsync),
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
                await caption.ClickAsync();
                await Task.Delay(300);
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
                await ready.WaitForAsync(new() { Timeout = 90000, State = WaitForSelectorState.Attached });
            }
        }, 450);
    }

    private const string Types = "[ \"BlogPost\" ]";

    private const string SetupRecipe = """
{
  "steps": [
    { "name": "feature", "enable": [ "OrchardCore.Lucene" ] },
    {
      "name": "CreateOrUpdateIndexProfile",
      "indexes": [
        {
          "IndexName": "blogposts", "ProviderName": "Lucene", "Type": "Content", "Name": "blogposts",
          "Properties": {
            "LuceneIndexMetadata": { "StoreSourceData": false, "AnalyzerName": "standardanalyzer" },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": TYPES, "Culture": "any" },
            "LuceneIndexDefaultQueryMetadata": { "DefaultVersion": "LUCENE_30", "QueryAnalyzerName": "standardanalyzer", "AllowLuceneQueries": false, "DefaultSearchFields": [ "Content.ContentItem.FullText" ] }
          },
          "IndexLatest": false, "Culture": "any", "IndexedContentTypes": TYPES
        }
      ]
    },
    { "name": "RebuildIndex", "includeAll": false, "indexNames": [ "blogposts" ] }
  ]
}
""";

    // Enables Lucene and creates the index blogposts (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", SetupRecipe.Replace("TYPES", Types));
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(8000);
        Console.WriteLine("Prepared Lucene and the index blogposts.");
        await context.CloseAsync();
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        var typesLink = await ExpandMenuAsync(scene, "/Admin/ContentTypes/List");
        await scene.NavigateAsync("/Admin/ContentTypes/List");
        await ExpandMenuAsync(scene, "/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("#left-nav a[href$='/Admin/ContentTypes/List']").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a[role='btn-edit-BlogPost']").First, page.Locator("a[href*='/ContentParts/BlogPost/Fields/Subtitle/Edit']").First);
            await GoAsync(scene, page.Locator("a[href*='/ContentParts/BlogPost/Fields/Subtitle/Edit']").First, page.GetByText("Lucene Index settings").First);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            var heading = page.GetByText("Lucene Index settings").First;
            await heading.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            await scene.ClickAsync(page.GetByLabel("Include this element in the index"), pauseAfter: 400);
            await page.Locator("#lucene-index-settings.show, [id$='LuceneContentIndexSettings_Keyword']").First.WaitForAsync();
        });

        await scene.StepAsync("settings-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClickAsync(page.GetByLabel("Keyword", new() { Exact = false }).First, pauseAfter: 700);
            await scene.ClickAsync(page.GetByLabel("Stored", new() { Exact = false }).First, pauseAfter: 700);
            var save = page.Locator("button.save").First;
            await save.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(save);
            await AtAsync(watch, Math.Max(500, (int)(durations["settings-3"] * 1000) - 4500));
            await GoAsync(scene, save, page.GetByText("field settings have been saved").First);
        });
    }

    private static async Task ResetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var row = page.Locator("[data-filter-value='blogposts' i]").First;

        await scene.StepAsync("reset-1", async () =>
        {
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 500);
            var reset = row.Locator("a.dropdown-item:text-is('Reset')").First;
            await GoAsync(scene, reset, page.GetByText("has been reset successfully").First);
        });

        await scene.StepAsync("reset-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 30; i++)
                {
                    var html = await page.EvaluateAsync<string>("() => fetch('/Admin/Lucene/query/blogposts').then(r => r.text())");

                    if (html.Contains("BlogPost.Subtitle"))
                    {
                        return;
                    }

                    await Task.Delay(2000);
                }
            }, 600);
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 500);
            await GoAsync(scene, row.Locator("a.dropdown-item:text-is('Query Index')").First, page.Locator("select#Id, button:text-is('Run Query')").First);

            if (await page.Locator("th:text-is('BlogPost.Subtitle')").CountAsync() == 0)
            {
                await GoAsync(scene, page.Locator("button:text-is('Run Query')").First, page.Locator("table.table").First);
            }

            var header = page.Locator("th:text-is('BlogPost.Subtitle')").First;
            await header.ScrollIntoViewIfNeededAsync();
            await Task.Delay(600);
            await scene.HighlightAsync(header);
            await scene.HighlightAsync(page.Locator("td:has-text('Problems look mighty small')").First);
        });
    }
}

using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the search-settings video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-default", DefaultAsync),
        ("03-search", SearchAsync),
        ("05-placeholder", PlaceholderAsync),
        ("07-permissions", PermissionsAsync),
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

    private static ILocator Label(Scene scene, string text) => scene.Page.GetByLabel(text, new() { Exact = true });

    private static async Task AtAsync(Stopwatch watch, int milliseconds)
    {
        var remaining = milliseconds - (int)watch.ElapsedMilliseconds;

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }
    }

    private const string SeedRecipe = """
{
  "steps": [
    { "name": "feature", "enable": [ "OrchardCore.Search", "OrchardCore.Lucene" ] },
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "Exploration notes from a week in the mountains", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Exploration notes from a week in the mountains" },
          "MarkdownBodyPart": { "Markdown": "We packed light, followed the river upstream and let the weather decide each day. Exploration is mostly patience, and these notes record what worked and what did not." },
          "AutoroutePart": { "Path": "blog/mountain-week" },
          "BlogPost": { "Subtitle": { "Text": "Ten days of maps, boots and slow mornings" } }
        },
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "Why exploration begins with a good map", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Why exploration begins with a good map" },
          "MarkdownBodyPart": { "Markdown": "A paper map shows the shape of a valley before you reach it. Good exploration starts at the kitchen table, with a pencil and a plan for the first three hours." },
          "AutoroutePart": { "Path": "blog/good-map" },
          "BlogPost": { "Subtitle": { "Text": "Plan the first three hours, then wander" } }
        },
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "A short guide to exploration on foot in a new city", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "A short guide to exploration on foot in a new city" },
          "MarkdownBodyPart": { "Markdown": "Walk for one hour without a destination, then follow the street that looks quietest. Exploration on foot turns a city into a set of small neighborhoods." },
          "AutoroutePart": { "Path": "blog/city-on-foot" },
          "BlogPost": { "Subtitle": { "Text": "One hour, no destination" } }
        }
      ]
    },
    {
      "name": "CreateOrUpdateIndexProfile",
      "indexes": [
        {
          "IndexName": "Posts", "ProviderName": "Lucene", "Type": "Content", "Name": "Posts",
          "Properties": {
            "LuceneIndexMetadata": { "StoreSourceData": false, "AnalyzerName": "standardanalyzer" },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": [ "BlogPost" ], "Culture": "any" },
            "LuceneIndexDefaultQueryMetadata": { "DefaultVersion": "LUCENE_30", "QueryAnalyzerName": "standardanalyzer", "AllowLuceneQueries": false, "DefaultSearchFields": [ "Content.ContentItem.FullText" ] }
          },
          "IndexLatest": false, "Culture": "any", "IndexedContentTypes": [ "BlogPost" ]
        },
        {
          "IndexName": "Pages", "ProviderName": "Lucene", "Type": "Content", "Name": "Pages",
          "Properties": {
            "LuceneIndexMetadata": { "StoreSourceData": false, "AnalyzerName": "standardanalyzer" },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": [ "Article" ], "Culture": "any" },
            "LuceneIndexDefaultQueryMetadata": { "DefaultVersion": "LUCENE_30", "QueryAnalyzerName": "standardanalyzer", "AllowLuceneQueries": false, "DefaultSearchFields": [ "Content.ContentItem.FullText" ] }
          },
          "IndexLatest": false, "Culture": "any", "IndexedContentTypes": [ "Article" ]
        }
      ]
    },
    { "name": "RebuildIndex", "includeAll": false, "indexNames": [ "Posts", "Pages" ] }
  ]
}
""";

    // Seeds the features, three invented posts and the indexes Posts and Pages (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", SeedRecipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(8000);

        // Waits until the Posts index finds the posts.
        for (var attempt = 0; attempt < 40; attempt++)
        {
            await page.GotoAsync(BaseUrl + "/search/Posts?terms=exploration");
            var text = await page.InnerTextAsync("body");

            if (text.Contains("Exploration notes from a week") && text.Contains("A short guide to exploration"))
            {
                Console.WriteLine("The Posts index holds the posts.");
                await context.CloseAsync();

                return;
            }

            await Task.Delay(3000);
        }

        throw new InvalidOperationException("The Posts index never listed the posts.");
    }

    private static async Task DefaultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/search");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("default-1", async () =>
        {
            var warning = page.Locator(".message-warning").Filter(new() { HasText = "No default search index has been configured." }).First;
            await scene.HighlightAsync(warning, "No default index");
        });

        await scene.StepAsync("default-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin");
            var settings = page.Locator("#left-nav ul[data-title='Settings']");

            if (!await settings.IsVisibleAsync())
            {
                await scene.ClickAsync(page.Locator("#left-nav figcaption:has(span.title:text-is('Settings'))").First, pauseAfter: 250);
            }

            var group = settings.Locator("figcaption:has(span.title:text-is('Search'))").First;

            if (await group.CountAsync() > 0)
            {
                await scene.ClickAsync(group, pauseAfter: 250);
            }

            var link = page.Locator("#left-nav a").Filter(new() { HasText = "Site Search" }).First;
            await scene.ClickAndWaitForNavigationAsync(link);
            await Label(scene, "Default search index").WaitForAsync();
        });

        await scene.StepAsync("default-3", async () =>
        {
            var list = Label(scene, "Default search index");
            await scene.MoveToAsync(list);
            await scene.SelectAsync(list, "Posts");
            await scene.HighlightAsync(page.GetByText("The default index to use for the search page."));
        });

        await scene.StepAsync("default-4", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var save = page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await AtAsync(watch, Math.Max(500, (int)(durations["default-4"] * 1000) - 5500));
            await scene.ClickAndWaitForNavigationAsync(save);
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        });
    }

    private static async Task SearchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("input[name='Terms']").First;

        await scene.StepAsync("search-1", async () =>
        {
            await scene.NavigateAsync("/search");
            await box.WaitForAsync();
            await scene.MoveToAsync(box);
            await scene.HighlightAsync(box, "Standard text");
        });

        await scene.StepAsync("search-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("exploration", new() { Delay = 10 });
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Button, new() { Name = "Search", Exact = true }).First);
            await page.Locator("ul.list-group").First.WaitForAsync();
            await scene.ScrollAsync(0, 350);
        });

        await scene.StepAsync("search-3", async () =>
        {
            await page.EvaluateAsync("window.scrollTo(0, 0)");
            await box.FillAsync("zebra");
            await scene.MoveToAsync(page.GetByRole(AriaRole.Button, new() { Name = "Search", Exact = true }).First);
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Button, new() { Name = "Search", Exact = true }).First);
            await page.GetByText("There are no such results.").First.WaitForAsync();
        });
    }

    private static async Task PlaceholderAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/search");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("placeholder-1", async () =>
        {
            var field = Label(scene, "Placeholder");
            await scene.ClickAsync(field, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Search the blog", new() { Delay = 10 });
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(field);
        });

        await scene.StepAsync("placeholder-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First);
            await scene.NavigateAsync("/search");
            await page.Locator("input[name='Terms']").First.WaitForAsync();
        });
    }

    private static async Task PermissionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("permissions-1", async () =>
        {
            var group = page.Locator("#left-nav ul[data-title='Access Control']");

            if (!await group.IsVisibleAsync())
            {
                await scene.ClickAsync(page.Locator("#left-nav figcaption:has(span.title:text-is('Access Control'))").First, pauseAfter: 250);
            }

            await scene.ClickAndWaitForNavigationAsync(group.GetByRole(AriaRole.Link, new() { Name = "Roles", Exact = true }).First);
            var admin = page.GetByRole(AriaRole.Link, new() { Name = "Administrator", Exact = true }).First;
            await admin.WaitForAsync();
            await scene.HighlightAsync(admin);
        });

        await scene.StepAsync("permissions-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var edit = page.Locator("a.edit[href$='/Roles/Edit/Anonymous']").First;
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await edit.ClickAsync());
                await page.Locator("#search-box").WaitForAsync(new() { Timeout = 60000 });
            });
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Query", new() { Delay = 15 });
            await Assertions.Expect(page.Locator("tr").Filter(new() { HasText = "Access admin panel" }).First).ToBeHiddenAsync(new() { Timeout = 20000 });
        });

        await scene.StepAsync("permissions-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("tr[data-text='Query any index']"));
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("tr[data-text=\"Query 'Posts' Index\"]"));
            await scene.ClickAsync(page.Locator("[id='Checkbox.QueryIndex_Posts']"), pauseAfter: 300);
        });

        await scene.StepAsync("permissions-4", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var save = page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await AtAsync(watch, Math.Max(500, (int)(durations["permissions-4"] * 1000) - 5500));
            await scene.ClickAndWaitForNavigationAsync(save);
            await page.GetByText("Role updated successfully.").First.WaitForAsync();
        });
    }
}

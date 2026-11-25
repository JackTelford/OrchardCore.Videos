using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the media-indexing video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string PdfPath = "C:/ocv/s4b/garden-planting-guide.pdf";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-field", FieldAsync),
        ("04-publish", PublishAsync),
        ("05-index", IndexAsync),
        ("06-search", SearchAsync),
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
                await ready.WaitForAsync(new() { Timeout = 90000, State = WaitForSelectorState.Attached });
            }
        }, 450);
    }

    private static async Task RingAsync(Scene scene, ILocator target, double seconds, string caption = null)
    {
        await target.ScrollIntoViewIfNeededAsync();
        await Task.Delay(400);
        await scene.HighlightAsync(target, caption);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    private static async Task TypeInAsync(Scene scene, ILocator field, string text, int delay = 35)
    {
        await field.ScrollIntoViewIfNeededAsync();
        await scene.ClickAsync(field, pauseAfter: 100);
        await scene.Page.Keyboard.TypeAsync(text, new() { Delay = delay });
    }

    private const string Types = "[ \"Article\" ]";

    private const string SetupRecipe = """
{
  "steps": [
    { "name": "feature", "enable": [ "OrchardCore.Search", "OrchardCore.Lucene" ] },
    {
      "name": "CreateOrUpdateIndexProfile",
      "indexes": [
        {
          "IndexName": "Articles", "ProviderName": "Lucene", "Type": "Content", "Name": "Articles",
          "Properties": {
            "LuceneIndexMetadata": { "StoreSourceData": false, "AnalyzerName": "standardanalyzer" },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": [ "Article" ], "Culture": "any" },
            "LuceneIndexDefaultQueryMetadata": { "DefaultVersion": "LUCENE_30", "QueryAnalyzerName": "standardanalyzer", "AllowLuceneQueries": false, "DefaultSearchFields": [ "Content.ContentItem.FullText" ] }
          },
          "IndexLatest": false, "Culture": "any", "IndexedContentTypes": [ "Article" ]
        }
      ]
    },
    { "name": "RebuildIndex", "includeAll": false, "indexNames": [ "Articles" ] }
  ]
}
""";

    // Enables Lucene and Search, creates the index Articles, picks it for the site search and uploads the fake PDF (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();

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

        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", SetupRecipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(8000);

        await page.GotoAsync(BaseUrl + "/Admin/Settings/search");
        await page.Locator("select[id$='DefaultIndexProfileName']").SelectOptionAsync(new SelectOptionValue { Label = "Articles" });
        await page.Locator("button.save").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);

        await page.GotoAsync(BaseUrl + "/Admin/Media");
        await page.Locator("input[type=file]").First.SetInputFilesAsync(PdfPath);
        await Task.Delay(6000);
        Console.WriteLine("Prepared: Lucene, the index Articles, the site search and the PDF.");
        await context.CloseAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("#search-box");

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Features", box);
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("media indexing", new() { Delay = 30 });
            await Assertions.Expect(page.Locator("#list-summary")).ToContainTextAsync("Showing 3 of");
            await scene.HighlightAsync(page.Locator(".feature-group:not(.d-none) h3").First, "Search");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await RingAsync(scene, page.Locator(".direct-dependencies .badge:has-text('Media Indexing')").First, 1.2, "Media Indexing");
            await scene.ClickAsync(page.Locator(".feature-group:not(.d-none) .list-group-select-all").First, pauseAfter: 300);
            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("a[data-action='Enable']").First, page.GetByText("were enabled").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("media indexing", new() { Delay = 30 });
            await Assertions.Expect(page.Locator("#list-summary")).ToContainTextAsync("Showing 3 of");
            await scene.HighlightAsync(page.Locator(".feature-group:not(.d-none) .badge:has-text('Enabled')").First);
            await scene.HoldAsync(1.0);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task FieldAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("field-1", async () =>
        {
            var link = await ExpandMenuAsync(scene, "/Admin/ContentTypes/List");
            await GoAsync(scene, link, page.Locator("a[role='btn-edit-Article']").First);
            await GoAsync(scene, page.Locator("a[role='btn-edit-Article']").First, page.Locator("a:has-text('Add Field')").First);
        });

        await scene.StepAsync("field-2", async () =>
        {
            await GoAsync(scene, page.Locator("a:has-text('Add Field')").First, page.Locator("input[data-displayname]"));
            await TypeInAsync(scene, page.Locator("input[data-displayname]"), "Documents");
            await page.Keyboard.PressAsync("Tab");
            var name = page.Locator("input[data-name]");

            if (string.IsNullOrEmpty(await name.InputValueAsync()))
            {
                await name.FillAsync("Documents");
            }

            var radio = page.Locator("label:has-text('Media Field')").First;
            await radio.ScrollIntoViewIfNeededAsync();
            await Task.Delay(400);
            await scene.ClickAsync(radio, pauseAfter: 300);
            await GoAsync(scene, page.Locator("button:has-text('Save & Configure')").First, page.GetByText("has been added").First);
        });

        await scene.StepAsync("field-3", async () =>
        {
            var heading = page.Locator("h4:has-text('Index settings')").First;
            await heading.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            var include = page.Locator("input[id$='LuceneContentIndexSettings_Included']").First;
            await scene.ClickAsync(include, pauseAfter: 400);
            await page.Locator("#lucene-index-settings.show").WaitForAsync();
            await RingAsync(scene, page.Locator("label:has-text('Keyword')").First, 1.5, "Leave cleared");
        });

        await scene.StepAsync("field-4", async () =>
        {
            await RingAsync(scene, page.Locator("label:has-text('Allow media text')").First, 2.0, "Allow media text");
            var save = page.Locator("button.save").First;
            await save.ScrollIntoViewIfNeededAsync();
            await GoAsync(scene, save, page.GetByText("field settings have been saved").First);
        });
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("publish-1", async () =>
        {
            var link = await ExpandMenuAsync(scene, "/Admin/Contents/ContentItems/");
            await GoAsync(scene, link, page.Locator("#new-dropdown"));
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("a.dropdown-item:has-text('Article')").First, page.Locator("input[id$='TitlePart_Title']"));
            await TypeInAsync(scene, page.Locator("input[id$='TitlePart_Title']"), "Autumn Garden Guide");
        });

        await scene.StepAsync("publish-2", async () =>
        {
            var wrapper = page.Locator(".field-wrapper-article-documents");
            await wrapper.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            await scene.ClickAsync(wrapper.Locator("button:has-text('Add media')").First, pauseAfter: 300);
            await scene.OffCameraAsync(async () =>
            {
                await page.Locator(".mf-picker-shell .file-item:has-text('garden-planting-guide.pdf')").First.WaitForAsync(new() { Timeout = 60000 });
            }, 600);
            await scene.ClickAsync(page.Locator(".mf-picker-shell .file-item:has-text('garden-planting-guide.pdf') input[name='select-file']").First, pauseAfter: 400);
            await scene.ClickAsync(page.Locator(".mf-picker-shell button:has-text('OK')").First, pauseAfter: 500);
            await scene.HoldAsync(0.5);
            await scene.ScrollAsync(0, 300);
        });

        await scene.StepAsync("publish-3", async () =>
        {
            var publish = page.Locator("button.publish").First;
            await publish.ScrollIntoViewIfNeededAsync();
            await GoAsync(scene, publish, page.GetByText("has been published").First);
        });
    }

    private static async Task IndexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var row = page.Locator("[data-filter-value='articles' i]").First;

        await scene.StepAsync("index-1", async () =>
        {
            var link = await ExpandMenuAsync(scene, "/Admin/indexing");
            await GoAsync(scene, link, row);
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 400);
            var reset = row.Locator("a.dropdown-item:text-is('Reset')").First;
            await scene.HighlightAsync(reset, "Reset");
            await scene.HoldAsync(1.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, reset, page.GetByText("has been reset successfully").First);
        });

        await scene.StepAsync("index-2", async () =>
        {
            await scene.HoldAsync(Math.Min(4.0, durations["index-2"] * 0.4));
            await GoAsync(scene, row.Locator("a:has-text('Edit')").First, page.Locator("#DefaultQueryFields"));
        });

        await scene.StepAsync("index-3", async () =>
        {
            var fields = page.Locator("#DefaultQueryFields");
            await fields.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            await scene.HighlightAsync(fields.Locator("label:has-text('Article.Documents.MediaText')").First);
            await scene.HighlightAsync(fields.Locator("label:has-text('Article.Documents.FileText')").First);
            await scene.HoldAsync(Math.Max(1.0, durations["index-3"] * 0.5));
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("index-4", async () =>
        {
            var watch = Stopwatch.StartNew();
            foreach (var name in new[] { "Article.Documents.MediaText", "Article.Documents.FileText" })
            {
                var box = page.Locator($"#DefaultQueryFields label:has-text('{name}') input[type=checkbox]").First;

                if (!await box.IsCheckedAsync())
                {
                    await scene.ClickAsync(box, pauseAfter: 400);
                }
            }

            var save = page.Locator(".action-bar-bottom button:has-text('Save')").First;
            await scene.MoveToAsync(save);
            var wait = (int)(durations["index-4"] * 1000) - 3600 - (int)watch.ElapsedMilliseconds;

            if (wait > 0)
            {
                await Task.Delay(wait);
            }

            await GoAsync(scene, save, page.GetByText("has been updated successfully").First);
        });
    }

    private static async Task SearchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("search-1", async () =>
        {
            var link = await ExpandMenuAsync(scene, "/Admin/Settings/search");
            var select = page.Locator("select[id$='DefaultIndexProfileName']");
            await GoAsync(scene, link, select);
            await scene.HighlightAsync(select, "Articles");
        });

        await scene.StepAsync("search-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/search");
            var terms = page.Locator("input[name='Terms']").First;
            await TypeInAsync(scene, terms, "persimmon", 50);
            await GoAsync(scene, page.Locator("button.btn-primary:has-text('Search')").First, page.Locator(".list-group-item").First);
        });

        await scene.StepAsync("search-3", async () =>
        {
            await scene.HighlightAsync(page.Locator(".list-group-item").First);
        });
    }
}

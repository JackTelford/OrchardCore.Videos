using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the full-text-search-guide video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-index", IndexAsync),
        ("04-settings", SettingsAsync),
        ("05-permission", PermissionAsync),
        ("06-try", TryAsync),
        ("07-type", TypeAsync),
        ("08-rebuild", RebuildAsync),
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

    // Opens a page of the left menu: expands each collapsed group above the link (outermost first), then clicks it.
    private static async Task OpenMenuAsync(Scene scene, string href)
    {
        var page = scene.Page;
        var link = page.Locator($"#left-nav a[href$='{href}']").First;
        await link.WaitForAsync(new() { State = WaitForSelectorState.Attached });
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

        await GoAsync(scene, link);
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
                await ready.WaitForAsync(new() { Timeout = 60000 });
            }
        }, 450);
    }

    // Holds until the toast of the click would last to the end of the narration, then clicks off camera.
    private static async Task SubmitAsync(Scene scene, Stopwatch watch, double seconds, ILocator button, ILocator ready)
    {
        await scene.MoveToAsync(button);
        await AtAsync(watch, Math.Max(300, (int)(seconds * 1000) - 3600));
        await GoAsync(scene, button, ready);
    }

    private const string SeedRecipe = """
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "Winter pruning for apple trees", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Winter pruning for apple trees" },
          "MarkdownBodyPart": { "Markdown": "Prune apple trees while they are dormant. Remove dead wood first, then thin crowded branches so light reaches the center of the tree." },
          "AutoroutePart": { "Path": "blog/winter-pruning" },
          "BlogPost": { "Subtitle": { "Text": "Grafting basics for small gardens" } }
        },
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "Choosing a cider apple", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Choosing a cider apple" },
          "MarkdownBodyPart": { "Markdown": "Cider makers blend apple varieties for sweetness, acidity and tannin. Press a small batch before committing a whole harvest." },
          "AutoroutePart": { "Path": "blog/cider-apple" },
          "BlogPost": { "Subtitle": { "Text": "Bittersweet or bittersharp" } }
        },
        {
          "ContentItemId": "[js: uuid()]", "ContentType": "BlogPost", "DisplayText": "Storing the autumn harvest", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Storing the autumn harvest" },
          "MarkdownBodyPart": { "Markdown": "Wrap sound pears in paper and keep them in a cool, dark cellar. Check the crates every week and remove fruit that starts to soften." },
          "AutoroutePart": { "Path": "blog/autumn-harvest" },
          "BlogPost": { "Subtitle": { "Text": "Cool cellars and slatted crates" } }
        }
      ]
    }
  ]
}
""";

    // Seeds the three invented posts (outside the capture). Lucene and Search stay disabled.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", SeedRecipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000);
        Console.WriteLine("Seeded the three posts.");
        await context.CloseAsync();
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("#search-box");

        await scene.StepAsync("features-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Features");
            await box.WaitForAsync();
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Lucene", new() { Delay = 30 });
            await page.Locator("#btn-enable-OrchardCore_Lucene").WaitForAsync();
        });

        await scene.StepAsync("features-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await SubmitAsync(scene, watch, durations["features-2"], page.Locator("#btn-enable-OrchardCore_Lucene"), page.Locator(".message-success, .alert-success").First);
            await scene.MoveToAsync(box);
        });

        await scene.StepAsync("features-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClickAsync(box, pauseAfter: 100);
            await box.FillAsync("Search");
            await page.Locator("#btn-enable-OrchardCore_Search").WaitForAsync();
            await page.Locator("#btn-enable-OrchardCore_Search").ScrollIntoViewIfNeededAsync();
            await SubmitAsync(scene, watch, durations["features-3"], page.Locator("#btn-enable-OrchardCore_Search"), page.Locator(".message-success, .alert-success").First);
            await scene.MoveToAsync(box);
        });
    }

    private static async Task IndexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("index-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/indexing");
            await scene.ClickAsync(page.Locator("button.create").First, pauseAfter: 300);
            var add = page.Locator(".card").Filter(new() { Has = page.Locator("h4", new() { HasTextString = "Content in Lucene" }) }).GetByRole(AriaRole.Link, new() { Name = "Add", Exact = true }).First;
            await add.WaitForAsync();
            await scene.HoldAsync(2.2);
            await GoAsync(scene, add);
        });

        await scene.StepAsync("index-2", async () =>
        {
            var indexName = Label(scene, "Index Name");
            await scene.ClickAsync(indexName, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Posts", new() { Delay = 60 });
            var name = Label(scene, "Name");
            await Assertions.Expect(name).ToHaveValueAsync("posts");
            await scene.HighlightAsync(name);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("index-3", async () =>
        {
            var types = page.Locator(".content-types").First;
            await types.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 120);
            await scene.ClickAsync(types.GetByText("Blog Post", new() { Exact = true }).First, pauseAfter: 300);
        });

        await scene.StepAsync("index-4", async () =>
        {
            var analyzer = page.Locator("select").Filter(new() { Has = page.Locator("option[value='standardanalyzer']") }).First;
            await analyzer.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200);
            await scene.HighlightAsync(analyzer);
            await scene.HighlightAsync(Label(scene, "Store Source Data"));
        });

        await scene.StepAsync("index-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var fields = page.Locator("#DefaultQueryFields").First;
            await fields.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 150);
            await scene.HighlightAsync(fields);
        });

        await scene.StepAsync("index-6", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var save = page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First;
            await save.ScrollIntoViewIfNeededAsync();
            await SubmitAsync(scene, watch, durations["index-6"], save, page.GetByText("An index has been created successfully.").First);
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("settings-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Settings/search");
            var list = Label(scene, "Default search index");
            await list.WaitForAsync();
            await scene.MoveToAsync(list);
            await scene.SelectAsync(list, "posts");
        });

        await scene.StepAsync("settings-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            var field = Label(scene, "Placeholder");
            await scene.ClickAsync(field, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Search the orchard journal", new() { Delay = 25 });
            await page.Keyboard.PressAsync("Tab");
            var save = page.Locator("button.save").First;
            await SubmitAsync(scene, watch, durations["settings-2"], save, page.GetByText("Site settings updated successfully.").First);
        });
    }

    private static async Task PermissionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("permission-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Roles/Index");
            var edit = page.Locator("a.edit[href$='/Roles/Edit/Anonymous']").First;
            await edit.WaitForAsync();
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await edit.ClickAsync());
                await page.Locator("#search-box").WaitForAsync(new() { Timeout = 60000 });
            });
        });

        await scene.StepAsync("permission-2", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("posts", new() { Delay = 30 });
            var first = page.Locator("tr[data-text=\"Query 'posts' Index\"]");
            await first.WaitForAsync();
            await Assertions.Expect(page.Locator("tr").Filter(new() { HasText = "Access admin panel" }).First).ToBeHiddenAsync(new() { Timeout = 20000 });
            await scene.HighlightAsync(first);
            await scene.ClickAsync(page.Locator("[id='Checkbox.QueryIndex_posts']"), pauseAfter: 300);
        });

        await scene.StepAsync("permission-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var save = page.Locator("button.save").First;
            await SubmitAsync(scene, watch, durations["permission-3"], save, page.GetByText("Role updated successfully.").First);
        });
    }

    private static async Task TryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("input[name='Terms']").First;
        var button = page.Locator("form:has(input[name='Terms']) button[type=submit]").First;

        await scene.StepAsync("try-1", async () =>
        {
            await scene.NavigateAsync("/search");
            await box.WaitForAsync();
            await scene.MoveToAsync(box);
        });

        await scene.StepAsync("try-2", async () =>
        {
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("apple", new() { Delay = 40 });
            await GoAsync(scene, button, page.GetByText("Winter pruning for apple trees").First);
            await scene.ScrollAsync(0, 560);
        });

        await scene.StepAsync("try-3", async () =>
        {
            await page.EvaluateAsync("window.scrollTo(0, 0)");
            await box.FillAsync(string.Empty);
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("grafting", new() { Delay = 40 });
            await GoAsync(scene, button, page.GetByText("There are no such results.").First);
        });
    }

    private static async Task TypeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("type-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/ContentTypes/List");
            var edit = page.Locator("a[role='btn-edit-BlogPost']").First;
            await edit.WaitForAsync();
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await edit.ClickAsync());
                await page.GetByText("Full-text", new() { Exact = true }).First.WaitForAsync(new() { Timeout = 60000 });
            });
            var heading = page.GetByText("Full-text", new() { Exact = true }).First;
            await heading.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 250);
        });

        await scene.StepAsync("type-2", async () =>
        {
            await scene.ClickAsync(page.Locator("input[id$='IncludeFullTextTemplate']").First, pauseAfter: 300);
            var editor = page.Locator("#fulltext-template .CodeMirror").First;
            await editor.WaitForAsync();
            await scene.ClickAsync(editor, pauseAfter: 150);
            await page.Keyboard.TypeAsync("{{ Model.Content.BlogPost.Subtitle.Text }}", new() { Delay = 12 });
        });

        await scene.StepAsync("type-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            var save = page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First;
            await save.ScrollIntoViewIfNeededAsync();
            await SubmitAsync(scene, watch, durations["type-3"], save, page.GetByText("Content type updated successfully.").First);
        });
    }

    private static async Task RebuildAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("rebuild-1", async () =>
        {
            var watch = Stopwatch.StartNew();
            await OpenMenuAsync(scene, "/Admin/indexing");
            var row = page.Locator("li[data-filter-value='posts']").First;
            await row.WaitForAsync();
            await scene.ClickAsync(row.Locator(".admin-list-actions button.dropdown-toggle"), pauseAfter: 500);
            var item = row.Locator(".actions-menu a.dropdown-item", new() { HasTextString = "Rebuild" });
            await scene.MoveToAsync(item);
            await AtAsync(watch, Math.Max(300, (int)(durations["rebuild-1"] * 1000) - 1500));
            await scene.WaitForNavigationAsync(async () =>
            {
                await item.ClickAsync();

                try
                {
                    await page.Locator("#modalOkButton").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1200 });
                    await page.Locator("#modalOkButton").ClickAsync();
                }
                catch (Exception)
                {
                }
            });
        });

        await scene.StepAsync("rebuild-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await page.GetByText("An index has been rebuilt successfully.").First.WaitForAsync();
            await AtAsync(watch, Math.Max(300, (int)(durations["rebuild-2"] * 1000) - 600));
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 60; i++)
                {
                    var text = await page.EvaluateAsync<string>("() => fetch('/search?Terms=grafting').then(r => r.text())");

                    if (text.Contains("Winter pruning for apple trees"))
                    {
                        return;
                    }

                    await Task.Delay(1000);
                }
            }, 300);
        });

        await scene.StepAsync("rebuild-3", async () =>
        {
            await scene.NavigateAsync("/search");
            var box = page.Locator("input[name='Terms']").First;
            await box.WaitForAsync();
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("grafting", new() { Delay = 40 });
            await GoAsync(scene, page.Locator("form:has(input[name='Terms']) button[type=submit]").First, page.GetByText("Winter pruning for apple trees").First);
        });
    }
}

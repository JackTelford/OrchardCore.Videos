using System.Diagnostics;
using System.Text.Json;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the elasticsearch-search video: see recording-plan.md.</summary>
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
        ("04-search", SearchAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);

            if (name == "03-create")
            {
                await WaitForDocumentsAsync();
            }
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

    private static async Task RevealAsync(Scene scene, ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();

        if (box is not null && (box.Y < 130 || box.Y + box.Height > 700))
        {
            await scene.ScrollAsync(0, box.Y - 300);
        }
    }

    private static async Task ExpandAsync(Scene scene, string group)
    {
        var list = scene.Page.Locator($"#left-nav ul[data-title='{group}']").First;

        if (!await list.IsVisibleAsync())
        {
            await scene.ClickAsync(scene.Page.Locator($"#left-nav figcaption:has(span.title:text-is('{group}'))").First, pauseAfter: 250);
        }
    }

    // Enables nothing on its own: seeds four invented Article content items (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();

        object Article(string id, string title, string path, string html) => new Dictionary<string, object>
        {
            ["ContentItemId"] = id,
            ["ContentType"] = "Article",
            ["DisplayText"] = title,
            ["Latest"] = true,
            ["Published"] = true,
            ["Author"] = "admin",
            ["TitlePart"] = new { Title = title },
            ["AutoroutePart"] = new { Path = path },
            ["HtmlBodyPart"] = new { Html = html },
        };

        var recipe = JsonSerializer.Serialize(new
        {
            steps = new[]
            {
                new
                {
                    name = "content",
                    data = new[]
                    {
                        Article("4c7qk3m9v2xd8hs1tb5ng6wzae", "Harvest Notes From Willow Creek Orchard", "harvest-notes-from-willow-creek-orchard",
                            "<p>The apple harvest at Willow Creek Orchard starts in early autumn. Pickers fill the first crates before sunrise.</p>"),
                        Article("4m2x8dv5qh1tz9kb3ra7jf6ycn", "Pressing Cider In Small Batches", "pressing-cider-in-small-batches",
                            "<p>A small press turns the autumn harvest into cider. We press once a week and bottle on Fridays.</p>"),
                        Article("4r9t3hb7wk5ne2xq8vd1ma6zjg", "Planting Pear Trees In Winter", "planting-pear-trees-in-winter",
                            "<p>Bare-root pear trees go into the ground in winter. Dig wide, water well and stake each tree.</p>"),
                        Article("4f6y1nc8sa3vx5pq2kb9td7hwm", "Visiting The Orchard Shop", "visiting-the-orchard-shop",
                            "<p>The orchard shop sells apples, pears and jars of honey from Tuesday to Saturday.</p>"),
                    },
                },
            },
        });

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", recipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("Seeded the articles.");
        await context.CloseAsync();
    }

    // Off camera: waits until the index on the Elasticsearch server holds the articles.
    private static async Task WaitForDocumentsAsync()
    {
        using var http = new HttpClient();

        for (var attempt = 0; attempt < 40; attempt++)
        {
            try
            {
                var response = await http.GetStringAsync("http://localhost:9204/_cat/indices?h=index,docs.count");
                Console.WriteLine("indices: " + response.Replace('\n', ';'));

                foreach (var line in response.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2 && parts[0].EndsWith("articles") && int.TryParse(parts[1], out var count) && count >= 4)
                    {
                        return;
                    }
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(3000);
        }

        throw new InvalidOperationException("The index never held the articles.");
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav figcaption:has(span.title:text-is('Tools'))").First.ClickAsync();
        await scene.StartCaptureAsync();
        var page = scene.Page;

        ILocator Row(string id) => page.Locator(".list-group-item").Filter(new() { Has = page.Locator($"#btn-enable-{id}, #btn-disable-{id}") }).First;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav ul[data-title='Tools']").GetByRole(AriaRole.Link, new() { Name = "Features", Exact = true }).First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Elasticsearch", new() { Delay = 5 });
            await page.Locator("#btn-enable-OrchardCore_Elasticsearch").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.HighlightAsync(Row("OrchardCore_Elasticsearch"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("#btn-enable-OrchardCore_Elasticsearch");
            await scene.MoveToAsync(enable);
            await AtAsync(watch, Math.Max(2000, (int)(durations["enable-2"] * 1000) - 4800));
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await enable.ClickAsync());
                await page.Locator(".toast").Filter(new() { HasText = "The feature Elasticsearch was enabled." }).First.WaitForAsync(new() { Timeout = 90000 });
            });
        });

        await scene.StepAsync("enable-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("Search", new() { Delay = 5 });
            var enable = page.Locator("#btn-enable-OrchardCore_Search");
            await enable.WaitForAsync();
            await scene.HighlightAsync(Row("OrchardCore_Search"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(enable);
            await AtAsync(watch, Math.Max(2500, (int)(durations["enable-3"] * 1000) - 4800));
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await enable.ClickAsync());
                await page.Locator(".toast").Filter(new() { HasText = "The feature Search was enabled." }).First.WaitForAsync(new() { Timeout = 90000 });
            });
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await ExpandAsync(scene, "Search");
            await ExpandAsync(scene, "Queries");
            var entry = page.Locator("#left-nav a").Filter(new() { HasText = "Run Elasticsearch Query" }).First;
            await entry.WaitForAsync();
            await scene.HighlightAsync(entry);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav figcaption:has(span.title:text-is('Search'))").First.ClickAsync();
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav ul[data-title='Search']").First.GetByRole(AriaRole.Link, new() { Name = "Indexes", Exact = true }).First);
            var add = page.GetByRole(AriaRole.Button, new() { Name = "Add index" }).First;
            await add.WaitForAsync();
            await scene.HighlightAsync(add);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add index" }).First, pauseAfter: 600);
            var card = page.Locator("#modalAddIndex .card").Filter(new() { HasText = "Content in Elasticsearch" });
            await card.WaitForAsync();
            await scene.HighlightAsync(card);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(card.GetByRole(AriaRole.Link, new() { Name = "Add" }));
        });

        await scene.StepAsync("create-3", async () =>
        {
            var name = Label(scene, "Index Name");
            await scene.ClickAsync(name, pauseAfter: 100);
            await page.Keyboard.TypeAsync("articles", new() { Delay = 5 });
            await Assertions.Expect(Label(scene, "Name")).ToHaveValueAsync("articles");
            await scene.HighlightAsync(name);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Name"));
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var types = page.Locator(".content-types");
            await RevealAsync(scene, types);
            await scene.ClickAsync(types.GetByLabel("Article", new() { Exact = true }), pauseAfter: 300);
            await scene.HighlightAsync(Label(scene, "Culture"));
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var analyzer = Label(scene, "Analyzer");
            await RevealAsync(scene, analyzer);
            await scene.HighlightAsync(analyzer);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Store Source Data"));
        });

        await scene.StepAsync("create-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var searchType = page.GetByLabel("Search type");
            await RevealAsync(scene, searchType);
            await scene.HighlightAsync(searchType);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            var fields = page.Locator("#DefaultQueryFields, [id*='QueryFields']").First;
            await RevealAsync(scene, fields);
            await scene.HighlightAsync(fields);
        });

        await scene.StepAsync("create-7", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var save = page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true });
            await scene.ScrollAsync(0, 1500);
            await scene.MoveToAsync(save);
            await AtAsync(watch, 800);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await save.ClickAsync());
                await page.Locator("li[data-filter-value='articles']").WaitForAsync(new() { Timeout = 120000 });
            });
            await scene.MoveToAsync(page.Locator("li[data-filter-value='articles'] .tags").First);
        });
    }

    private static async Task SearchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav figcaption:has(span.title:text-is('Settings'))").First.ClickAsync();
        await scene.Page.Locator("#left-nav figcaption:has(span.title:text-is('Search'))").Last.ClickAsync();
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("search-1", async () =>
        {
            var link = page.Locator("#left-nav a").Filter(new() { HasText = "Site Search" }).First;
            await scene.HighlightAsync(link);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(link);
            await scene.SelectAsync(Label(scene, "Default search index"), "articles");
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        });

        await scene.StepAsync("search-2", async () =>
        {
            await scene.NavigateAsync("/search");
            var box = page.Locator("input[name='Terms']").First;
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("harvest", new() { Delay = 20 });
        });

        await scene.StepAsync("search-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form:has(input[name='Terms']) button[type='submit']").First);
            await page.Locator("a:has-text('Harvest Notes From Willow Creek Orchard')").First.WaitForAsync();
            await scene.ScrollAsync(0, 380);
        });

        await scene.StepAsync("search-4");
    }
}

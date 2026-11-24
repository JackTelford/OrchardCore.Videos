using System.Diagnostics;
using System.Text.Json;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the lucene-search video: see recording-plan.md.</summary>
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
        ("04-fields", FieldsAsync),
        ("05-search", SearchAsync),
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
                await WaitForIndexAsync(browser);
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

    // Scrolls with the wheel, so the viewer sees it, until the element is comfortably in view.
    private static async Task RevealAsync(Scene scene, ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();

        if (box is not null && (box.Y < 130 || box.Y + box.Height > 700))
        {
            await scene.ScrollAsync(0, box.Y - 260);
        }
    }

    // Enables Search and seeds four invented Article content items (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#search-box").FillAsync("frontend search");
        await page.Locator("#btn-enable-OrchardCore_Search").ClickAsync();
        await page.Locator("#btn-disable-OrchardCore_Search").WaitForAsync(new() { State = WaitForSelectorState.Attached });

        object Article(string id, string version, string title, string path, string html, string date) => new Dictionary<string, object>
        {
            ["ContentItemId"] = id,
            ["ContentItemVersionId"] = version,
            ["ContentType"] = "Article",
            ["DisplayText"] = title,
            ["Latest"] = true,
            ["Published"] = true,
            ["CreatedUtc"] = date,
            ["ModifiedUtc"] = date,
            ["PublishedUtc"] = date,
            ["Author"] = "admin",
            ["TitlePart"] = new { Title = title },
            ["AutoroutePart"] = new { Path = path, SetHomepage = false },
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
                        Article("ujzde8gxd6ncf10epf91dhodzd", "oc9is0j8ht9lgmxg9edn581u33", "Starting a kitchen garden", "starting-a-kitchen-garden",
                            "<p>A kitchen garden needs sun, water and a plan. Begin with herbs such as basil and chives, then add tomatoes once the soil is warm.</p>", "2025-03-03T09:00:00Z"),
                        Article("xtplpft75v2seh60kvj50ce9uv", "w53efr4edt2sywb3wkh5dnsipz", "Composting for beginners", "composting-for-beginners",
                            "<p>Kitchen scraps, dry leaves and a little patience turn into rich compost. Spread it on the garden soil in early spring.</p>", "2025-03-05T09:00:00Z"),
                        Article("z5fk2z9ri19r0wyojfljooa5lq", "saj08xui6d39zzzzg4zdmen2kh", "Choosing tools for the garden", "choosing-tools-for-the-garden",
                            "<p>A sturdy spade, a hand fork and sharp pruning shears cover most jobs. Keep the blades clean and dry after each use.</p>", "2025-03-07T09:00:00Z"),
                        Article("vdgaj8gxbenyjqwx4hh5344tfj", "gvq4k7bn7xj8b7tfq7xkwo886v", "Winter reading list", "winter-reading-list",
                            "<p>Five long novels for short days, from a sea voyage to a quiet village mystery.</p>", "2025-03-10T09:00:00Z"),
                    },
                },
            },
        });

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", recipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("Enabled Search and seeded the articles.");
        await context.CloseAsync();
    }

    // Off camera: waits until the new index holds the three garden articles.
    private static async Task WaitForIndexAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();

        for (var attempt = 0; attempt < 40; attempt++)
        {
            await page.GotoAsync(BaseUrl + "/search/search?terms=garden");
            var text = await page.InnerTextAsync("body");

            if (text.Contains("Starting a kitchen garden") && text.Contains("Composting for beginners") && text.Contains("Choosing tools for the garden") && !text.Contains("Winter reading list"))
            {
                Console.WriteLine("The index holds the garden articles.");
                await context.CloseAsync();

                return;
            }

            await Task.Delay(3000);
        }

        throw new InvalidOperationException("The index never listed the three garden articles.");
    }

    private static ILocator FeatureRow(Scene scene, string id)
        => scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator($"#btn-enable-{id}, #btn-disable-{id}") }).First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "Lucene", delay: 90);
            await page.Locator("#btn-enable-OrchardCore_Lucene").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(FeatureRow(scene, "OrchardCore_Lucene"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FeatureRow(scene, "OrchardCore_Search_Lucene"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FeatureRow(scene, "OrchardCore_Search_Lucene_ContentPicker"));
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#btn-enable-OrchardCore_Lucene"));
            await page.Locator("#btn-disable-OrchardCore_Lucene").WaitForAsync(new() { State = WaitForSelectorState.Attached });
            await scene.TypeAsync(page.Locator("#search-box"), "Lucene", delay: 60);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add index" }).First, pauseAfter: 900);
            var card = page.Locator("#modalAddIndex .card").Filter(new() { HasText = "Content in Lucene" });
            await card.WaitForAsync();
            await scene.HighlightAsync(card);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(card.GetByRole(AriaRole.Link, new() { Name = "Add" }));
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(Label(scene, "Index Name"), "search", delay: 90);
            await Assertions.Expect(Label(scene, "Name")).ToHaveValueAsync("search");
            await scene.HighlightAsync(Label(scene, "Index Name"));
            await scene.HighlightAsync(Label(scene, "Name"));
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var types = page.Locator(".content-types");
            await RevealAsync(scene, types);
            await scene.ClickAsync(types.GetByLabel("Article", new() { Exact = true }), pauseAfter: 300);
            await scene.ClickAsync(types.GetByLabel("Blog Post", new() { Exact = true }), pauseAfter: 300);
            await scene.ClickAsync(types.GetByLabel("Page", new() { Exact = true }), pauseAfter: 300);
            await RevealAsync(scene, Label(scene, "Culture"));
            await scene.HighlightAsync(Label(scene, "Culture"));
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await RevealAsync(scene, Label(scene, "Analyzer"));
            await scene.HighlightAsync(Label(scene, "Analyzer"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Store Source Data"));
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var fields = page.Locator("#DefaultQueryFields");
            await RevealAsync(scene, fields);
            await scene.HighlightAsync(fields);
        });

        await scene.StepAsync("create-6", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var save = page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true });
            await scene.MoveToAsync(save);
            await AtAsync(watch, Math.Max(500, (int)(durations["create-6"] * 1000) - 5200));
            await scene.ClickAndWaitForNavigationAsync(save);
            await page.Locator("li[data-filter-value='search']").WaitForAsync();
        });
    }

    private static async Task FieldsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("fields-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a[role='btn-edit-Article']"));
            var heading = page.Locator("h3").Filter(new() { HasText = "Full-text" }).First;
            await RevealAsync(scene, heading);
            await scene.HighlightAsync(heading);
        });

        await scene.StepAsync("fields-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Include display text"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Include body parts"));
        });

        await scene.StepAsync("fields-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var parts = page.Locator("#parts");
            await RevealAsync(scene, parts);
            var title = parts.Locator("li").Filter(new() { HasText = "Title" }).First;
            await scene.ClickAndWaitForNavigationAsync(title.Locator("a", new() { HasTextString = "Edit" }).First);
            var include = Label(scene, "Include this element in the index");
            await RevealAsync(scene, include);
            await scene.HighlightAsync(include);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(include, pauseAfter: 300);
            await scene.HighlightAsync(include);
        });

        await scene.StepAsync("fields-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Keyword"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Stored"));
        });

        await scene.StepAsync("fields-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var heading = page.Locator("h4, h3").Filter(new() { HasText = "Lucene Index settings" }).First;
            await scene.HighlightAsync(heading);
        });

        await scene.StepAsync("fields-6", async () =>
        {
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task SearchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/search/search");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("input[name='Terms']").First;

        await scene.StepAsync("search-1", async () =>
        {
            await scene.MoveToAsync(box);
            await scene.HighlightAsync(box, "/search/search");
        });

        await scene.StepAsync("search-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(box, "garden", delay: 90);
        });

        await scene.StepAsync("search-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form:has(input[name='Terms']) button[type='submit']").First);
            await page.Locator("a:has-text('Starting a kitchen garden')").First.WaitForAsync();
            await scene.ScrollAsync(0, 380);
        });

        await scene.StepAsync("search-4", async () =>
        {
            await scene.MoveToAsync(page.Locator("input[name='Terms']").First);
        });

        await scene.StepAsync("search-5");
    }
}

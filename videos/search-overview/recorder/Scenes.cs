using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the search-overview video: see recording-plan.md.</summary>
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
        ("06-widget", WidgetAsync),
        ("07-visitor", VisitorAsync),
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

    // Seeds the three invented blog posts (outside the capture) through Deployments, JSON Import.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin");
        var html = await page.ContentAsync();
        var blogId = Regex.Match(html, @"/Admin/Contents/ContentItems/([a-z0-9]{26})/Display").Groups[1].Value;

        if (blogId.Length == 0)
        {
            throw new InvalidOperationException("The Blog content item id was not found in the admin menu.");
        }

        object Post(string id, string title, string subtitle, string path, string markdown, int order) => new Dictionary<string, object>
        {
            ["ContentItemId"] = id,
            ["ContentType"] = "BlogPost",
            ["DisplayText"] = title,
            ["Latest"] = true,
            ["Published"] = true,
            ["Author"] = "admin",
            ["TitlePart"] = new { Title = title },
            ["ContainedPart"] = new { ListContentItemId = blogId, Order = order },
            ["AutoroutePart"] = new { Path = path },
            ["MarkdownBodyPart"] = new { Markdown = markdown },
            ["BlogPost"] = new { Subtitle = new { Text = subtitle } },
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
                        Post("4qz7m2k9d1x6c3v8b5n0r4t7ya", "Planning the Autumn Harvest Festival", "Stalls, music and a long table under the apple trees", "blog/planning-the-autumn-harvest-festival",
                            "The harvest festival returns to Maple Lane this October. Volunteers are building stalls for apples, honey and bread, and a band will play beside the long table under the trees.\n\nBring a basket, a chair and an appetite.", 1),
                        Post("4wd3p8h1j6s2f9g5k7l0e4u3ib", "A Short History of Our Orchard", "Eighty trees, four varieties and one stubborn ladder", "blog/a-short-history-of-our-orchard",
                            "The orchard began with eighty apple trees planted along the river. Every autumn the harvest was carried to the cellar by hand, and the oldest ladder is still in use.\n\nToday the trees give four varieties of apple, from early green fruit to late red ones.", 2),
                        Post("4mb5t1y8c2v6x9n3z7a0q4r2ek", "Winter Baking With Preserved Fruit", "Jars from the cellar become pies", "blog/winter-baking-with-preserved-fruit",
                            "Jars of plum jam and baked apple fill the pantry shelves. This recipe turns preserved fruit into a pie that keeps the warmth of a summer afternoon.\n\nServe it warm, with cream.", 3),
                    },
                },
            },
        });

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", recipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("Seeded the posts of blog " + blogId + ".");
        await context.CloseAsync();
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("features-1", async () =>
        {
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add index" }).First, pauseAfter: 900);
            await page.Locator("#modalAddIndex .bg-warning").WaitForAsync();
            await scene.HighlightAsync(page.Locator("#modalAddIndex .bg-warning"));
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#modalAddIndex").GetByRole(AriaRole.Button, new() { Name = "Cancel" }), pauseAfter: 200);
            await scene.NavigateAsync("/Admin/Features");
            await page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "Lucene", delay: 50);
            await page.Locator("#btn-enable-OrchardCore_Lucene").WaitForAsync();
            var row = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_Lucene") });
            await scene.HighlightAsync(row);
            await scene.PauseAsync(400);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#btn-enable-OrchardCore_Lucene"));
            await page.Locator("#btn-disable-OrchardCore_Lucene").WaitForAsync(new() { State = WaitForSelectorState.Attached });
        });

        await scene.StepAsync("features-3", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "frontend search", delay: 60);
            await page.Locator("#btn-enable-OrchardCore_Search").WaitForAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#btn-enable-OrchardCore_Search"));
            await page.Locator("#btn-disable-OrchardCore_Search").WaitForAsync(new() { State = WaitForSelectorState.Attached });
        });
    }

    private static async Task IndexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("index-1", async () =>
        {
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add index" }).First, pauseAfter: 900);
            var card = page.Locator("#modalAddIndex .card").Filter(new() { HasText = "Content in Lucene" });
            await scene.HighlightAsync(card);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(card.GetByRole(AriaRole.Link, new() { Name = "Add" }));
        });

        await scene.StepAsync("index-2", async () =>
        {
            await scene.TypeAsync(Label(scene, "Index Name"), "website", delay: 90);
            await Assertions.Expect(Label(scene, "Name")).ToHaveValueAsync("website");
            await scene.HighlightAsync(Label(scene, "Index Name"));
            await scene.HighlightAsync(Label(scene, "Name"));
        });

        await scene.StepAsync("index-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Label(scene, "Blog Post"));
            await scene.ClickAsync(Label(scene, "Article"));
            await scene.HighlightAsync(Label(scene, "Culture"));
        });

        await scene.StepAsync("index-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var save = page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true });
            await scene.MoveToAsync(save);
            await scene.PauseAsync(3000);
            await scene.ClickAndWaitForNavigationAsync(save);
            await page.Locator(".list-group-item").Filter(new() { HasText = "website" }).First.WaitForAsync();
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("settings-1", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/search");
            await Label(scene, "Default search index").WaitForAsync();
            await scene.SelectAsync(Label(scene, "Default search index"), "website");
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.TypeAsync(Label(scene, "Placeholder"), "Search articles and posts", delay: 60);
            await page.Keyboard.PressAsync("Tab");
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
        });
    }

    private static async Task PermissionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/search");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("permission-1", async () =>
        {
            await scene.NavigateAsync("/Admin/Roles/Index");
            await page.Locator("a.edit[href$='/Roles/Edit/Anonymous']").WaitForAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a.edit[href$='/Roles/Edit/Anonymous']"));
        });

        await scene.StepAsync("permission-2", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "Query 'website' Index", delay: 60);
            await page.Locator("[id='Checkbox.QueryIndex_website']").WaitForAsync();
            await scene.ClickAsync(page.Locator("[id='Checkbox.QueryIndex_website']"), pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save"));
        });
    }

    private static async Task WidgetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("widget-1", async () =>
        {
            await scene.NavigateAsync("/Admin/Layers");
            var zone = page.Locator(".layer-zone[data-zone='Footer']");
            await zone.WaitForAsync();
            await scene.HighlightAsync(zone, "Footer");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(zone.GetByRole(AriaRole.Button, new() { Name = "Add Widget" }));
            await scene.ClickAndWaitForNavigationAsync(zone.Locator("a.add-list-widget[data-widget-type='SearchForm']"));
        });

        await scene.StepAsync("widget-2", async () =>
        {
            await scene.TypeAsync(Label(scene, "Title"), "Site search", delay: 60);
            await scene.SelectAsync(Label(scene, "The layer"), "Always");
            await scene.TypeAsync(Label(scene, "Placeholder"), "Search this site", delay: 60);
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(Label(scene, "Index Name"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.publish"));
        });
    }

    private static async Task VisitorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl,
            Path.Combine(Root, "no-sign-in.json"), Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/");
        await scene.Page.EvaluateAsync("window.scrollTo(0, 0)");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("form input[name='Terms']").First;

        await scene.StepAsync("visitor-1", async () =>
        {
            await scene.ScrollAsync(0, 3000);
            var footerBox = page.Locator("footer input[name='Terms']");
            await scene.TypeAsync(footerBox, "harvest", delay: 90);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("footer").GetByRole(AriaRole.Button, new() { Name = "Search" }));
        });

        await scene.StepAsync("visitor-2", async () =>
        {
            await scene.ScrollAsync(0, -3000);
            await scene.ScrollAsync(0, 350);
            await scene.HighlightAsync(page.Locator("ul.list-group"));
            await scene.MoveToAsync(page.Locator("ul.list-group a").First);
        });

        await scene.StepAsync("visitor-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(box);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.PressAsync("Backspace");
            await scene.HighlightAsync(box);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("visitor-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(box, "zebra", delay: 90);
            await scene.ClickAndWaitForNavigationAsync(box.Locator("xpath=ancestor::form").GetByRole(AriaRole.Button, new() { Name = "Search" }));
        });
    }
}

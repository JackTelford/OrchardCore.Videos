using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the queries-overview video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-screen", ScreenAsync),
        ("03-create", CreateAsync),
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

    private static async Task ExpandAsync(Scene scene, string group)
    {
        var list = scene.Page.Locator($"#left-nav ul[data-title='{group}']");

        if (!await list.IsVisibleAsync())
        {
            await scene.ClickAsync(scene.Page.Locator($"#left-nav figcaption:has(span.title:text-is('{group}'))").First, pauseAfter: 250);
        }
    }

    private static ILocator Row(Scene scene, string name) => scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator($"a:text-is('{name}')") }).First;

    private static async Task ScreenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("screen-1", async () =>
        {
            await ExpandAsync(scene, "Search");
            await ExpandAsync(scene, "Queries");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav ul[data-title='Queries']").GetByRole(AriaRole.Link, new() { Name = "All Queries", Exact = true }).First);
            var row = Row(scene, "RecentBlogPosts");
            await row.WaitForAsync();
            await scene.HighlightAsync(row.Locator("a:text-is('RecentBlogPosts')"));
        });

        await scene.StepAsync("screen-2", async () =>
        {
            var row = Row(scene, "RecentBlogPosts");
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("a:text-is('RecentBlogPosts')"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator(".ta-badge").First, "Source");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("a.btn:text-is('Run')"));
            await scene.PauseAsync(2300);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("a.btn:text-is('Edit')"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("a.btn:text-is('Delete')"));
        });

        await scene.StepAsync("screen-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("button.create").First, pauseAfter: 500);
            await page.Locator("#modalAddQuery").WaitForAsync();
            var card = page.Locator("#modalAddQuery .card").Filter(new() { HasText = "Queries the SQL database." });
            await card.WaitForAsync();
            await scene.HighlightAsync(card);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Queries/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(page.Locator("button.create").First, pauseAfter: 800);
            var card = page.Locator("#modalAddQuery .card").Filter(new() { HasText = "Queries the SQL database." });
            await card.WaitForAsync();
            await scene.ClickAndWaitForNavigationAsync(card.GetByRole(AriaRole.Link, new() { Name = "Add" }));
            await page.GetByLabel("Name", new() { Exact = true }).WaitForAsync();
        });

        await scene.StepAsync("create-2", async () =>
        {
            var name = page.GetByLabel("Name", new() { Exact = true });
            await scene.TypeAsync(name, "PublishedBlogPosts", delay: 30);
            await scene.HighlightAsync(name);
        });

        await scene.StepAsync("create-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("div.monaco-json-settings-editor-container").First, "Schema");
            await AtAsync(watch, 4500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 500);
            await AtAsync(watch, 7500);
            var returnDocuments = page.GetByLabel("Return Documents");
            await scene.HighlightAsync(returnDocuments);
            await AtAsync(watch, 12500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".CodeMirror").First, "Query");
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.GetByLabel("Return Documents"), pauseAfter: 100);
            await scene.ClickAsync(page.Locator(".CodeMirror").First, pauseAfter: 100);
            await page.Keyboard.TypeAsync("select DocumentId from ContentItemIndex where Published = true and ContentType = 'BlogPost'", new() { Delay = 3 });
        });

        await scene.StepAsync("create-5", async () =>
        {
            var watch = Stopwatch.StartNew();
            var save = page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await AtAsync(watch, 1200);
            await scene.ClickAndWaitForNavigationAsync(save);
            await page.GetByText("Query created successfully.").First.WaitForAsync();
            await scene.HighlightAsync(Row(scene, "PublishedBlogPosts").Locator("a:text-is('PublishedBlogPosts')"));
        });

        await scene.StepAsync("create-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var box = page.Locator("#search-box");
            await scene.ClickAsync(box, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Published", new() { Delay = 20 });
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button[name='submit.Filter']").First);
            await Assertions.Expect(page.Locator(".list-group-item").Filter(new() { Has = page.Locator("a:text-is('RecentBlogPosts')") })).ToHaveCountAsync(0);
            await scene.HighlightAsync(Row(scene, "PublishedBlogPosts").Locator("a:text-is('PublishedBlogPosts')"));
        });

        await scene.ClearHighlightsAsync();
    }
}

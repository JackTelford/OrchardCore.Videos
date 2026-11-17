using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the queries-in-templates-and-api video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-queries", QueriesAsync),
        ("02-post", PostAsync),
        ("04-grant", GrantAsync),
        ("04-call", CallAsync),
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
                await ready.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Visible });
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
        { "ContentItemId": "4q1allotment0000000000000001", "ContentType": "BlogPost", "DisplayText": "Notes From the Allotment", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-11-17T20:00:00Z", "ModifiedUtc": "2026-11-17T20:00:00Z", "PublishedUtc": "2026-11-17T20:00:00Z", "TitlePart": { "Title": "Notes From the Allotment" }, "AutoroutePart": { "Path": "blog/notes-from-the-allotment" }, "MarkdownBodyPart": { "Markdown": "Beans and courgettes went in the ground this week." } },
        { "ContentItemId": "4q2slowtravel00000000000002", "ContentType": "BlogPost", "DisplayText": "A Week of Slow Travel", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-11-17T20:10:00Z", "ModifiedUtc": "2026-11-17T20:10:00Z", "PublishedUtc": "2026-11-17T20:10:00Z", "TitlePart": { "Title": "A Week of Slow Travel" }, "AutoroutePart": { "Path": "blog/a-week-of-slow-travel" }, "MarkdownBodyPart": { "Markdown": "Seven days, three villages, and no schedule." } },
        { "ContentItemId": "4q3autumnmenu0000000000003", "ContentType": "BlogPost", "DisplayText": "Planning the Autumn Menu", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-11-17T20:20:00Z", "ModifiedUtc": "2026-11-17T20:20:00Z", "PublishedUtc": "2026-11-17T20:20:00Z", "TitlePart": { "Title": "Planning the Autumn Menu" }, "AutoroutePart": { "Path": "blog/planning-the-autumn-menu" }, "MarkdownBodyPart": { "Markdown": "Soups, roasted roots and a baked apple pudding." } }
      ]
    }
  ]
}
""";

    // Imports the three invented posts (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", SeedRecipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000);
        Console.WriteLine("Imported the posts.");
        await context.CloseAsync();
    }

    private static async Task QueriesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        var all = await ExpandMenuAsync(scene, "/Admin/Queries/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("queries-1", async () =>
        {
            await GoAsync(scene, all, page.Locator("a:has-text('RecentBlogPosts')").First);
        });

        await scene.StepAsync("queries-2", async () =>
        {
            await GoAsync(scene, page.Locator("a:has-text('RecentBlogPosts')").First, page.Locator(".CodeMirror").First);
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(page.Locator(".CodeMirror").First);
            await scene.HoldAsync(1.0);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task PostAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/blog/planning-the-autumn-menu");
        await scene.StartCaptureAsync();

        await scene.StepAsync("post-1", async () =>
        {
            for (var i = 0; i < 4; i++)
            {
                await scene.ScrollAsync(0, 700);
                await scene.PauseAsync(700);
            }
        });
    }

    private static async Task GrantAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("grant-1", async () =>
        {
            var edit = page.Locator("a.edit[href$='/Roles/Edit/Anonymous']").First;
            await GoAsync(scene, edit, page.Locator("#search-box"));
        });

        await scene.StepAsync("grant-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await page.Keyboard.TypeAsync("RecentBlogPosts", new() { Delay = 40 });
            var box = page.Locator("[id='Checkbox.ExecuteApi_RecentBlogPosts']");
            await box.WaitForAsync();
            await scene.ClickAsync(box, pauseAfter: 600);
            await SubmitAsync(scene, watch, durations["grant-2"], page.Locator("button.save[value='Save']").First, page.GetByText("Role updated successfully.").First);
        });
    }

    private static async Task CallAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("call-1", async () =>
        {
            await scene.NavigateAsync("/api/queries/RecentBlogPosts");
            await scene.HighlightAsync(page.Locator("pre").First, "GET /api/queries/RecentBlogPosts");
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("call-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/api/queries/RecentBlogPosts?parameters=%7B%22limit%22%3A2%7D");
            await scene.HighlightAsync(page.Locator("pre").First, "GET /api/queries/RecentBlogPosts?parameters={\"limit\":2}");
            await scene.PauseAsync(800);
        });
    }
}

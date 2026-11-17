using System.Text.Json;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Extend GraphQL". PrepareAsync enables GraphQL and Localization, sets the cultures en-US (default) and fr-FR and
/// imports three invented blog posts (JSON Import). All three clips only run queries in GraphiQL.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-explorer", ExplorerAsync),
        ("05-where", WhereAsync),
        ("07-run", RunAsync),
    ];

    private static async Task ImportAsync(IPage page, string json)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
    }

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90000);

        await ImportAsync(page, "{ \"steps\": [ { \"name\": \"Feature\", \"enable\": [ \"OrchardCore.Apis.GraphQL\", \"OrchardCore.Localization\" ], \"disable\": [] } ] }");
        await page.WaitForTimeoutAsync(5000);
        Console.WriteLine("Enabled GraphQL and Localization.");

        var blog = await page.EvaluateAsync<string>(@"async () => {
            const html = await (await fetch('/Admin/Contents/ContentItems/Blog')).text();
            const m = html.match(/\/Admin\/Contents\/ContentItems\/([0-9a-z]{26})\/(?:Display|Edit)/);
            return m ? m[1] : '';
        }");

        if (string.IsNullOrEmpty(blog))
        {
            throw new InvalidOperationException("The blog was not found.");
        }

        var posts = new[] { ("Harvest Notes", "harvest-notes"), ("Pruning in Winter", "pruning-in-winter"), ("Orchard Tour Dates", "orchard-tour-dates") }
            .Select((p, i) => new Dictionary<string, object>
            {
                ["ContentItemId"] = $"4orchardpost{i + 1:D13}y",
                ["ContentType"] = "BlogPost",
                ["DisplayText"] = p.Item1,
                ["Latest"] = true,
                ["Published"] = true,
                ["CreatedUtc"] = $"2027-03-0{i + 1}T09:00:00Z",
                ["ModifiedUtc"] = $"2027-03-0{i + 1}T09:00:00Z",
                ["PublishedUtc"] = $"2027-03-0{i + 1}T09:00:00Z",
                ["Author"] = "admin",
                ["Owner"] = "admin",
                ["TitlePart"] = new { Title = p.Item1 },
                ["ContainedPart"] = new { ListContentItemId = blog, ListContentType = "Blog", Order = 0 },
                ["AutoroutePart"] = new { Path = "blog/" + p.Item2 },
                ["MarkdownBodyPart"] = new { Markdown = "Notes." },
            }).ToArray();

        var recipe = JsonSerializer.Serialize(new
        {
            steps = new object[]
            {
                new { name = "Settings", LocalizationSettings = new { DefaultCulture = "en-US", SupportedCultures = new[] { "en-US", "fr-FR" } } },
                new { name = "Content", data = posts },
            },
        });
        await ImportAsync(page, recipe);
        Console.WriteLine("Set the cultures and imported the posts.");
        await context.CloseAsync();
    }

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static async Task<Scene> OpenQueryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string query, string root)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Admin/GraphQL?query=" + Uri.EscapeDataString(query));
        await scene.Page.Locator(".graphiql-explorer-root").GetByText(root).First.WaitForAsync(new() { Timeout = 60000 });
        await scene.Page.WaitForTimeoutAsync(800);
        await scene.StartCaptureAsync();
        return scene;
    }

    private static async Task RunQueryAsync(Scene scene, string text)
    {
        await scene.ClickAsync(scene.Page.Locator("button.execute-button").First, pauseAfter: 300);
        await scene.Page.Locator(".resultWrap").GetByText(text).First.WaitForAsync(new() { Timeout = 60000 });
        await scene.PauseAsync(1500);
    }

    private static async Task ExplorerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenQueryAsync(browser, directory, durations, "{\n  blogPost {\n    displayText\n    path\n  }\n}", "blogPost");

        await scene.StepAsync("explorer-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#left-nav a:has-text('GraphiQL')").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".queryWrap").First);
        });

        await scene.StepAsync("explorer-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await RunQueryAsync(scene, "\"path\"");
            await scene.HighlightAsync(scene.Page.Locator(".graphiql-explorer-root").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WhereAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenQueryAsync(browser, directory, durations, "{\n  blogPost(where: { path_contains: \"harvest\" }) {\n    displayText\n    path\n  }\n}", "blogPost");

        await scene.StepAsync("where-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".queryWrap").First, "where");
        });

        await scene.StepAsync("where-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await RunQueryAsync(scene, "harvest-notes");
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenQueryAsync(browser, directory, durations, "{\n  siteCultures {\n    culture\n    default\n  }\n}", "siteCultures");

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".queryWrap").First);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await RunQueryAsync(scene, "fr-FR");
            var row = scene.Page.Locator(".graphiql-explorer-root").GetByText("siteCultures").First;
            await row.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(row);
        });

        await scene.ClearHighlightsAsync();
    }
}

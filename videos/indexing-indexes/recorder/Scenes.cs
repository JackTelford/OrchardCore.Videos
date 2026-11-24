using System.Diagnostics;
using System.Text.Json;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the indexing-indexes video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-create", CreateAsync),
        ("04-actions", ActionsAsync),
        ("05-edit", EditAsync),
        ("06-worker", WorkerAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);

            // Between the clips, off camera: the second index "pages" that 04-actions needs.
            if (name == "02-create" && selected.Any(clip => clip.Name == "04-actions"))
            {
                await CreatePagesIndexAsync(browser);
            }
        }
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static ILocator Label(Scene scene, string text) => scene.Page.GetByLabel(text, new() { Exact = true });

    // Waits until a step has run for the given time.
    private static async Task AtAsync(Stopwatch watch, int milliseconds)
    {
        var remaining = milliseconds - (int)watch.ElapsedMilliseconds;

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }
    }

    // Enables the features and seeds three Article content items (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();

        foreach (var (filter, id) in new[] { ("Lucene", "OrchardCore_Lucene"), ("Background Tasks", "OrchardCore_BackgroundTasks") })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            await page.Locator("#search-box").FillAsync(filter);
            await page.Locator("#btn-enable-" + id).ClickAsync();
            await page.Locator("#btn-disable-" + id).WaitForAsync(new() { State = WaitForSelectorState.Attached });
        }

        object Article(string id, string title) => new Dictionary<string, object>
        {
            ["ContentItemId"] = id,
            ["ContentType"] = "Article",
            ["DisplayText"] = title,
            ["Latest"] = true,
            ["Published"] = true,
            ["Author"] = "admin",
            ["TitlePart"] = new { Title = title },
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
                        Article("4qz7m2k9d1x6c3v8b5n0r4t7ya", "Spring herb garden notes"),
                        Article("4wd3p8h1j6s2f9g5k7l0e4u3ib", "A rainy day soup"),
                        Article("4mb5t1y8c2v6x9n3z7a0q4r2ek", "Notes from a fictional coast road"),
                    },
                },
            },
        });

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", recipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("Enabled the features and seeded the articles.");
        await context.CloseAsync();
    }

    // Off camera: the index "pages" with the content type Page only.
    private static async Task CreatePagesIndexAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/indexing");
        await page.GetByRole(AriaRole.Button, new() { Name = "Add index" }).First.ClickAsync();
        var card = page.Locator("#modalAddIndex .card").Filter(new() { HasText = "Content in Lucene" });
        await card.GetByRole(AriaRole.Link, new() { Name = "Add" }).ClickAsync();
        await page.GetByLabel("Index Name", new() { Exact = true }).FillAsync("pages");
        await page.GetByLabel("Page", new() { Exact = true }).CheckAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
        await page.Locator("li[data-filter-value='pages']").WaitForAsync();
        await Task.Delay(3000);
        Console.WriteLine("Created the index pages.");
        await context.CloseAsync();
    }

    // Expands a group of the left menu when it is collapsed, then opens one of its pages.
    private static async Task OpenMenuAsync(Scene scene, string group, string link, string fallbackPath)
    {
        var page = scene.Page;
        var list = page.Locator($"#left-nav ul[data-title='{group}']");

        if (await list.CountAsync() == 0)
        {
            await scene.NavigateAsync(fallbackPath);

            return;
        }

        if (!await list.IsVisibleAsync())
        {
            await scene.ClickAsync(page.Locator($"#left-nav figcaption:has(span.title:text-is('{group}'))"), pauseAfter: 250);
        }

        await scene.ClickAndWaitForNavigationAsync(list.GetByRole(AriaRole.Link, new() { Name = link, Exact = true }));
    }

    private static ILocator Row(Scene scene, string name) => scene.Page.Locator($"li[data-filter-value='{name}']");

    private static Task OpenActionsAsync(Scene scene, ILocator row)
        => scene.ClickAsync(row.Locator(".admin-list-actions button.dropdown-toggle"), pauseAfter: 500);

    // Chooses an item of an open Actions menu, with or without a confirmation dialog.
    private static async Task ChooseActionAsync(Scene scene, ILocator row, string item)
    {
        await scene.WaitForNavigationAsync(async () =>
        {
            await scene.ClickAsync(row.Locator(".actions-menu a.dropdown-item", new() { HasTextString = item }), pauseAfter: 300);

            try
            {
                await scene.Page.Locator("#modalOkButton").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1200 });
                await scene.ClickAsync(scene.Page.Locator("#modalOkButton"));
            }
            catch (Exception)
            {
            }
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").FillAsync("Lucene");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await OpenMenuAsync(scene, "Search", "Indexes", "/Admin/indexing");
        });

        await scene.StepAsync("create-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add index" }).First, pauseAfter: 900);
            var card = page.Locator("#modalAddIndex .card").Filter(new() { HasText = "Content in Lucene" });
            await card.WaitForAsync();
            await AtAsync(watch, 3600);
            await scene.ClickAndWaitForNavigationAsync(card.GetByRole(AriaRole.Link, new() { Name = "Add" }));
        });

        await scene.StepAsync("create-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await AtAsync(watch, 3500);
            await scene.TypeAsync(Label(scene, "Index Name"), "articles", delay: 90);
            await Assertions.Expect(Label(scene, "Name")).ToHaveValueAsync("articles");
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.HighlightAsync(Label(scene, "Name"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Culture"));
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Label(scene, "Article"));
        });

        await scene.StepAsync("create-6", async () =>
        {
            var latest = Label(scene, "Index Latest version");
            await latest.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(latest);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 500);
        });

        await scene.StepAsync("create-7", async () =>
        {
            var watch = Stopwatch.StartNew();
            var save = page.Locator(".action-bar-bottom").GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true });
            await scene.MoveToAsync(save);
            await AtAsync(watch, 900);
            await scene.ClickAndWaitForNavigationAsync(save);
        });
    }

    private static async Task ActionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();
        var row = Row(scene, "articles");

        await scene.StepAsync("actions-1", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.HighlightAsync(Row(scene, "pages"));
            await AtAsync(watch, 5500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator(".admin-list-actions"));
            await AtAsync(watch, 10000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator(".tags"));
        });

        await scene.StepAsync("actions-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenActionsAsync(scene, row);
        });

        await scene.StepAsync("actions-3", async () =>
        {
            await scene.PauseAsync(300);
            await ChooseActionAsync(scene, row, "Synchronize");
        });

        await scene.StepAsync("actions-4", async () =>
        {
            var watch = Stopwatch.StartNew();
            await AtAsync(watch, 300);
            await OpenActionsAsync(scene, row);
            await AtAsync(watch, 900);
            await ChooseActionAsync(scene, row, "Reset");
        });

        await scene.StepAsync("actions-5", async () =>
        {
            var watch = Stopwatch.StartNew();
            await AtAsync(watch, 300);
            await OpenActionsAsync(scene, row);
            await AtAsync(watch, 900);
            await ChooseActionAsync(scene, row, "Rebuild");
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var row = Row(scene, "articles");

        await scene.StepAsync("edit-1", async () =>
        {
            var watch = Stopwatch.StartNew();
            await AtAsync(watch, 3500);
            await scene.ClickAndWaitForNavigationAsync(row.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }));
            await AtAsync(watch, 5500);
            await scene.HighlightAsync(Label(scene, "Index Name"));
        });

        await scene.StepAsync("edit-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300);
            await scene.ClickAsync(Label(scene, "Blog Post"));
            await scene.ClickAsync(Label(scene, "Index Latest version"));
            await AtAsync(watch, 7000);
            await scene.ClickAndWaitForNavigationAsync(page.Locator(".action-bar-bottom").GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
        });

        await scene.StepAsync("edit-3");

        await scene.StepAsync("edit-4", async () =>
        {
            await scene.ClickAsync(row.GetByRole(AriaRole.Button, new() { Name = "Delete" }).Or(row.GetByRole(AriaRole.Link, new() { Name = "Delete" })).First);
            await page.Locator("#modalCancelButton").WaitForAsync();
            await scene.PauseAsync(2000);
            await scene.ClickAsync(page.Locator("#modalCancelButton"));
            await OpenActionsAsync(scene, row);
            await scene.ClickAsync(row.Locator(".actions-menu a.dropdown-item", new() { HasTextString = "Force Delete" }), pauseAfter: 300);
            await page.Locator("#modalCancelButton").WaitForAsync();
            await scene.PauseAsync(2000);
            await scene.ClickAsync(page.Locator("#modalCancelButton"));
        });

        await scene.StepAsync("edit-5", async () =>
        {
            await scene.ClickAsync(page.Locator("#select-all"));
            await scene.ClickAsync(page.Locator("#bulk-action-menu-button"), pauseAfter: 3000);
            await page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(page.Locator("#select-all"));
        });
    }

    private static async Task WorkerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("worker-1", async () =>
        {
            var watch = Stopwatch.StartNew();
            await OpenMenuAsync(scene, "Tools", "Features", "/Admin/Features");
            await page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "Indexing Worker", delay: 70);
            var enable = page.Locator("#btn-enable-OrchardCore_Indexing_Worker");
            await enable.WaitForAsync();
            await scene.MoveToAsync(enable);
            await AtAsync(watch, 9000);
            await scene.ClickAndWaitForNavigationAsync(enable);
        });

        await scene.StepAsync("worker-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await AtAsync(watch, 1500);
            await OpenMenuAsync(scene, "Tools", "Background Tasks", "/Admin/BackgroundTasks");
            await AtAsync(watch, 4500);
            var task = page.Locator("li, tr, .list-group-item").Filter(new() { HasText = "Indexes Content Updater" }).Last;
            await task.WaitForAsync();
            await scene.HighlightAsync(task);
        });

        await scene.StepAsync("worker-3");
        await scene.ClearHighlightsAsync();
    }
}

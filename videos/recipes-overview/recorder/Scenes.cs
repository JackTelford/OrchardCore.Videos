using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Recipes". 02-setup runs on a site that is not set up (no Auto Setup, nothing is ever submitted);
/// the other clips run on the Blog site, where PrepareAsync enables the Admin Dashboard feature. 04-run changes the
/// site (it adds a dashboard widget): restore the snapshot before a new take.
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
        ("02-setup", SetupAsync),
        ("03-browse", BrowseAsync),
        ("04-run", RunAsync),
        ("05-permission", PermissionAsync),
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

    // Enables the Admin Dashboard feature outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.GotoAsync($"{BaseUrl}/Admin/Features", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#search-box").FillAsync("Admin Dashboard");
        await page.Locator("#btn-enable-OrchardCore_AdminDashboard").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(4000);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task CloseGroupAsync(IPage page, string group)
    {
        var button = page.Locator("#left-nav button").Filter(new() { HasTextRegex = new($"^\\s*{group}\\s*$") }).First;

        if (await button.IsVisibleAsync() && await button.GetAttributeAsync("aria-expanded") == "true")
        {
            await button.ClickAsync();
            await Task.Delay(500);
        }
    }

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await scene.MoveToAsync(scene.Page.Locator("#recipeButton"));
            await scene.HighlightAsync(scene.Page.Locator("#recipeButton"), "Recipe");
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#recipeButton"), pauseAfter: 600);
            var entries = scene.Page.Locator("#recipes .dropdown-menu a");

            for (var i = 0; i < Math.Min(await entries.CountAsync(), 6); i++)
            {
                await scene.MoveToAsync(entries.Nth(i));
                await scene.PauseAsync(500);
            }
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#recipes a[data-recipe-name='Blog']"), pauseAfter: 300);
        });
    }

    private static async Task BrowseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await CloseGroupAsync(scene.Page, "Tools");
        await scene.StartCaptureAsync();

        await scene.StepAsync("browse-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Tools", Exact = true }), pauseAfter: 300);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/Recipes']").First);
            await scene.Page.Keyboard.PressAsync("Tab", new() { Delay = 0 });
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "lucene", 10);
        });

        await scene.StepAsync("browse-2", async () =>
        {
            var row = scene.Page.Locator(".recipe-group .list-group-item", new() { HasText = "Blog - Lucene Query" });
            await scene.HighlightAsync(scene.Page.Locator(".recipe-group h4", new() { HasText = "The Blog Theme" }), "Group");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row, "Name and description");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator(".badge").First, "Tag");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("a.btn-primary"), "Run");
        });

        await scene.StepAsync("browse-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#search-box"), pauseAfter: 100);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await scene.Page.Keyboard.TypeAsync("blog", new() { Delay = 10 });
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Recipes");
        await scene.Page.Locator("#search-box").PressSequentiallyAsync("dashboard");
        await Task.Delay(800);
        await scene.StartCaptureAsync();
        var row = scene.Page.Locator(".recipe-group .list-group-item", new() { HasText = "Admin Dashboard Widget Sample" });

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HighlightAsync(row, "Admin Dashboard Widget Sample");
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(row.GetByText("Run", new() { Exact = true }), pauseAfter: 300);
            await scene.Page.Locator("#confirmRemoveModal.show").WaitForAsync();
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.MoveToAsync(scene.Page.Locator("#modalOkButton"));
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await scene.Page.Locator("#modalOkButton").ClickAsync();
                await scene.Page.Locator(".toast.message-success").First.WaitForAsync(new() { Timeout = 60000 });

                // Filter the list again, off camera, so the long descriptions of the other recipes never show.
                await scene.Page.Locator("#search-box").PressSequentiallyAsync("dashboard");
            }));
        });

        await scene.StepAsync("run-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("nav[aria-label='Breadcrumb'] a", new() { HasText = "Dashboard" }).First);
            await scene.Page.Locator(".dashboard-container", new() { HasText = "Orchard Core" }).First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".dashboard-container .card").First, "Orchard Core");
        });
    }

    private static async Task PermissionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await CloseGroupAsync(scene.Page, "Access Control");
        await CloseGroupAsync(scene.Page, "Tools");
        await scene.StartCaptureAsync();

        await scene.StepAsync("who-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Access Control", Exact = true }), pauseAfter: 300);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/Roles/Index']").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.edit[href$='/Roles/Edit/Editor']"));
        });

        await scene.StepAsync("who-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "recipes", 20);
            var badge = scene.Page.Locator("tr[data-text='Manage Recipes'] .badge").First;
            await scene.MoveToAsync(badge);
            await scene.HighlightAsync(badge, "Security Critical");
        });

        await scene.StepAsync("who-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(scene.Page.Locator("[id='Checkbox.ManageRecipes']"));
            await scene.HighlightAsync(scene.Page.Locator("tr[data-text='Manage Recipes']"));
        });
    }
}

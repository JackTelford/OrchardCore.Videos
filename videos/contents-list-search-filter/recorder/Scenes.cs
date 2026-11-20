using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Content Items List: Search, Filter and Act on Many Items". PrepareAsync seeds the hiking club data
/// outside the capture; 05-bulk changes the site, so it is recorded last, from a snapshot.
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
        ("02-list", ListAsync),
        ("03-search", SearchAsync),
        ("04-filter", FilterAsync),
        ("05-bulk", BulkAsync),
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

    // Seeds the site outside the capture: ten items of a hiking club, then a draft of the last published one.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);

        async Task CreateAsync(string type, string title, bool publish)
        {
            await page.GotoAsync($"{BaseUrl}/Admin/Contents/ContentTypes/{type}/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator(publish ? "button.publish" : "button.draft").ClickAsync();
            await page.WaitForURLAsync(url => !url.Contains("/Create"));
            await Task.Delay(1500);
        }

        await CreateAsync("Page", "Membership", true);
        await CreateAsync("Page", "Contact Us", true);
        await CreateAsync("Article", "Spring Trail Cleanup Day", true);
        await CreateAsync("Article", "Summer Hiking Schedule", true);
        await CreateAsync("Article", "Choosing Hiking Boots", true);
        await CreateAsync("Article", "Trail Safety Basics", true);
        await CreateAsync("Article", "Placeholder Notice 1", false);
        await CreateAsync("Article", "Placeholder Notice 2", false);
        await CreateAsync("Article", "Winter Snowshoe Schedule", false);
        await CreateAsync("Article", "Autumn Hiking Schedule", false);

        await page.GotoAsync($"{BaseUrl}/Admin/Contents/ContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("li.list-group-item").Filter(new() { Has = page.Locator(".contentitem a:text-is('Trail Safety Basics')") }).Locator("a.btn.edit").ClickAsync();
        await page.Locator("#TitlePart_Title").FillAsync("Trail Safety Basics Revised");
        await page.Locator("button.draft").ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("/Admin/Contents/ContentItems"));
        await Task.Delay(1500);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static ILocator Row(Scene scene, string title)
        => scene.Page.Locator("li.list-group-item").Filter(new() { Has = scene.Page.Locator($".contentitem a:text-is('{title}')") });

    private static ILocator Picker(Scene scene, string picker)
        => scene.Page.Locator($".bootstrap-select.{picker}- > button.dropdown-toggle");

    private static ILocator Option(Scene scene, string picker, string label)
        => scene.Page.Locator($".bootstrap-select.{picker}- .dropdown-menu a", new() { HasTextString = label }).First;

    private static ILocator SearchBox(Scene scene)
        => scene.Page.Locator("input[type='search'][placeholder='Search']").First;

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            if (await scene.Page.Locator("#left-nav a.oc-navigation-admin-content-content_items").IsVisibleAsync() is false)
            {
                await scene.ClickAsync(scene.Page.Locator("#left-nav button.oc-navigation-admin-content"), pauseAfter: 500);
            }

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.oc-navigation-admin-content-content_items"));
            await scene.Page.Locator("li.list-group-item .contentitem").First.WaitForAsync();
        });

        await scene.StepAsync("list-2", async () =>
        {
            var badges = scene.Page.Locator("li.list-group-item .contenttype .badge");

            for (var i = 0; i < 5; i++)
            {
                await scene.MoveToAsync(badges.Nth(i));
                await scene.PauseAsync(700);
            }
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Trail Safety Basics Revised"));
        });

        await scene.StepAsync("list-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = Row(scene, "Trail Safety Basics Revised");
            await scene.HighlightAsync(row.Locator("a.btn.edit"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Choosing Hiking Boots").Locator("a.btn.view"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 400);
        });

        await scene.StepAsync("list-5", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.HighlightAsync(scene.Page.Locator("#items"));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 900);
        });

        await scene.StepAsync("list-6", async () =>
        {
            await scene.ScrollAsync(0, -1200);
            await scene.HighlightAsync(scene.Page.Locator(".admin-list-layout-selector").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".admin-list-layout-selector a[title='Grid']"));
            await scene.PauseAsync(3000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".admin-list-layout-selector a[title='List']"));
        });
    }

    private static async Task SearchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("search-1", async () =>
        {
            await scene.TypeAsync(SearchBox(scene), "schedule", 40);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
        });

        await scene.StepAsync("search-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#items"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var rows = scene.Page.Locator("li.list-group-item .contentitem");

            for (var i = 0; i < 3; i++)
            {
                await scene.MoveToAsync(rows.Nth(i));
                await scene.PauseAsync(900);
            }
        });

        await scene.StepAsync("search-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#filter-dropdown"), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu[aria-labelledby='filter-dropdown'] a:text-is('Filter syntax')"), pauseAfter: 300);
            await scene.Page.Locator("#modalContentOptionsFilterSyntax.show").WaitForAsync();
        });

        await scene.StepAsync("search-4", async () =>
        {
            await scene.MoveToAsync(scene.Page.Locator("#modalContentOptionsFilterSyntax .modal-body"));
            await scene.ScrollAsync(0, 500);
            await scene.Page.EvaluateAsync("() => document.querySelector('#modalContentOptionsFilterSyntax .modal-body, #modalContentOptionsFilterSyntax .modal-content').scrollBy(0, 1)");
        });

        await scene.StepAsync("search-5", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#modalContentOptionsFilterSyntax .modal-footer button:text-is('Close')"), pauseAfter: 600);
            await scene.ClickAsync(SearchBox(scene), pauseAfter: 100);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await SearchBox(scene).PressSequentiallyAsync("schedule NOT winter", new() { Delay = 40 });
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
        });
    }

    private static async Task FilterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("filter-1", async () =>
        {
            await scene.ClickAsync(Picker(scene, "contenttypes"), pauseAfter: 500);
            await scene.TypeAsync(scene.Page.Locator(".bootstrap-select.contenttypes- .bs-searchbox input"), "art", 70);
            await scene.WaitForNavigationAsync(async () => await scene.ClickAsync(Option(scene, "contenttypes", "Article"), pauseAfter: 100));
        });

        await scene.StepAsync("filter-2", async () =>
        {
            await scene.ClickAsync(Picker(scene, "contentstatuses"), pauseAfter: 1500);
            await scene.WaitForNavigationAsync(async () => await scene.ClickAsync(Option(scene, "contentstatuses", "Unpublished"), pauseAfter: 100));
        });

        await scene.StepAsync("filter-3", async () =>
        {
            await scene.HighlightAsync(SearchBox(scene));
        });

        await scene.StepAsync("filter-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Picker(scene, "contentsorts"), pauseAfter: 1200);
            await scene.WaitForNavigationAsync(async () => await scene.ClickAsync(Option(scene, "contentsorts", "Title"), pauseAfter: 100));
        });

        await scene.StepAsync("filter-5", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#filter-dropdown"), pauseAfter: 600);
            var menu = scene.Page.Locator(".dropdown-menu[aria-labelledby='filter-dropdown']");
            await scene.MoveToAsync(menu.Locator("a:text-is('Only draft items')"));
            await scene.PauseAsync(1200);
            await scene.MoveToAsync(menu.Locator("a:text-is('Only published items')"));
            await scene.PauseAsync(1200);
            await scene.MoveToAsync(menu.Locator("a:text-is('Owned by me')"));
            await scene.PauseAsync(1200);
            await scene.ClickAndWaitForNavigationAsync(menu.Locator("a:text-is('Only draft items')"));
        });

        await scene.StepAsync("filter-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item .contenttype a", new() { HasTextString = "Article" }).First);
        });
    }

    private static async Task BulkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems?q=status:Draft");
        await scene.StartCaptureAsync();

        ILocator Box(string title) => Row(scene, title).Locator("input[name='itemIds']");

        await scene.StepAsync("bulk-1", async () =>
        {
            await scene.ClickAsync(Row(scene, "Winter Snowshoe Schedule").Locator("input[name='itemIds']"), pauseAfter: 1500);
            await scene.ClickAsync(Row(scene, "Autumn Hiking Schedule").Locator("input[name='itemIds']"), pauseAfter: 300);
        });

        await scene.StepAsync("bulk-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
        });

        await scene.StepAsync("bulk-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#actions .dropdown-item[data-action='PublishNow']"), pauseAfter: 300);
            await scene.Page.Locator("#confirmRemoveModal.show").WaitForAsync();
            await scene.PauseAsync(1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("bulk-4", async () =>
        {
            await scene.Page.Locator("text=Content published successfully.").WaitForAsync();
        });

        await scene.StepAsync("bulk-5", async () =>
        {
            await scene.TypeAsync(SearchBox(scene), "placeholder", 25);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
        });

        await scene.StepAsync("bulk-6", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#select-all").First, pauseAfter: 300);
            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button"), pauseAfter: 300);
            await scene.ClickAsync(scene.Page.Locator("#actions .dropdown-item[data-action='Remove']"), pauseAfter: 200);
            await scene.Page.Locator("#confirmRemoveModal.show").WaitForAsync();
        });

        await scene.StepAsync("bulk-7", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
        });
    }
}

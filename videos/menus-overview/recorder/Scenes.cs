using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the menus-overview video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string MenusUrl = "/Admin/Contents/ContentItems/Menu?Options.SelectedContentType=Menu&Options.CanCreateSelectedContentType=true";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-open", OpenAsync),
        ("03-add", AddAsync),
        ("04-arrange", ArrangeAsync),
        ("05-publish", PublishAsync),
        ("06-create", CreateAsync),
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

    private static ILocator MenuRow(Scene scene, string title) => scene.Page.Locator("li.list-group-item").Filter(new() { Has = scene.Page.Locator($"a:text-is('{title}')") }).First;

    private static ILocator ItemRow(Scene scene, string name) => scene.Page.Locator("#menu li.menu-item").Filter(new() { HasText = name }).First;

    // Off camera: opens the editor of the Main Menu from the list.
    private static async Task OpenEditorAsync(Scene scene)
    {
        await scene.Page.GotoAsync(Scenes.BaseUrl + MenusUrl);
        await MenuRow(scene, "Main Menu").Locator("a.edit").ClickAsync();
        await Assertions.Expect(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Menu Item" })).ToBeEnabledAsync(new() { Timeout = 60000 });
    }

    private static async Task ClickCardAsync(Scene scene, string card)
    {
        var link = scene.Page.Locator("#modalMenuItems .card").Filter(new() { HasText = card }).Locator("a.thumbnail-link-create").First;
        await scene.ClickAndWaitForNavigationAsync(link);
    }

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("#mainNav").GetByText("About").First.WaitForAsync();
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("open-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("#mainNav ul").First);
        });

        await scene.StepAsync("open-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin");
            var group = page.Locator("#left-nav ul[data-title='Content']");

            if (!await group.IsVisibleAsync())
            {
                await scene.ClickAsync(page.Locator("#left-nav figcaption:has(span.title:text-is('Content'))").First, pauseAfter: 250);
            }

            await scene.ClickAndWaitForNavigationAsync(group.GetByRole(AriaRole.Link, new() { Name = "Menus", Exact = true }).First);
            await MenuRow(scene, "Main Menu").WaitForAsync();
        });

        await scene.StepAsync("open-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            var edit = MenuRow(scene, "Main Menu").Locator("a.edit");
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await edit.ClickAsync());
                await page.GetByRole(AriaRole.Button, new() { Name = "Add Menu Item" }).WaitForAsync(new() { Timeout = 60000 });
            });
            await scene.HighlightAsync(Label(scene, "Title"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Alias"));
        });

        await scene.StepAsync("open-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#menu"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add Menu Item" }));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(MenusUrl);
        await OpenEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("add-1", async () =>
        {
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add Menu Item" }), pauseAfter: 300);
            await page.Locator("#modalMenuItems").WaitForAsync();
            await scene.PauseAsync(2500);
            await ClickCardAsync(scene, "Link Menu Item");
        });

        await scene.StepAsync("add-2", async () =>
        {
            var name = Label(scene, "Name");
            await scene.ClickAsync(name, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Browse", new() { Delay = 5 });
            var url = Label(scene, "Url");
            await scene.ClickAsync(url, pauseAfter: 100);
            await page.Keyboard.TypeAsync("~/blog", new() { Delay = 5 });
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(Label(scene, "Target"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".ocat-wrapper").Filter(new() { Has = page.Locator("#PermissionPicker") }).Last);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("button.publish").First);
        });

        await scene.StepAsync("add-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.publish").First);
            var row = ItemRow(scene, "Browse");
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ArrangeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(MenusUrl);
        await OpenEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("arrange-1", async () =>
        {
            var add = ItemRow(scene, "Browse").Locator("button.btn-primary").First;
            await scene.HighlightAsync(add);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(add, pauseAfter: 300);
            await page.Locator("#modalMenuItems").WaitForAsync();
        });

        await scene.StepAsync("arrange-2", async () =>
        {
            await ClickCardAsync(scene, "Link Menu Item");
            var name = Label(scene, "Name");
            await scene.ClickAsync(name, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Categories", new() { Delay = 5 });
            var url = Label(scene, "Url");
            await scene.ClickAsync(url, pauseAfter: 100);
            await page.Keyboard.TypeAsync("~/categories", new() { Delay = 5 });
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("arrange-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.publish").First);
            await Assertions.Expect(page.Locator("#menu li.menu-item[data-depth='1']")).ToHaveCountAsync(1);
            var bar = ItemRow(scene, "Browse").Locator(".menu-item-title").First;
            await bar.ScrollIntoViewIfNeededAsync();
            var box = await bar.BoundingBoxAsync();
            var startX = box.X + 60;
            var startY = box.Y + box.Height / 2;
            await scene.MoveToAsync(startX, startY);
            await page.Mouse.DownAsync();
            await scene.MoveToAsync(startX, startY + 10, 6);
            var aboutBox = await page.Locator("#menu li.menu-item").Filter(new() { HasText = "About" }).First.BoundingBoxAsync();
            await scene.MoveToAsync(startX, aboutBox.Y + 6, 30);
            await scene.PauseAsync(300);
            await page.Mouse.UpAsync();
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("arrange-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.draft").First);
            await page.GetByText("Your Menu draft has been saved.").First.WaitForAsync();
        });

        await scene.StepAsync("arrange-5", async () =>
        {
            var row = ItemRow(scene, "Categories");
            await scene.HighlightAsync(row.Locator("a.btn-success").First);
            await scene.HighlightAsync(row.Locator("a.btn-danger").First);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(row.Locator("a.btn-danger").First, pauseAfter: 300);
            await page.Locator("#confirmRemoveModal").WaitForAsync();
            await scene.PauseAsync(2500);
            await scene.ClickAsync(page.Locator("#modalCancelButton"), pauseAfter: 300);
            await page.Locator("#confirmRemoveModal").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        });
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("#mainNav").GetByText("About").First.WaitForAsync();
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("publish-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("#mainNav ul").First);
        });

        await scene.StepAsync("publish-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync(MenusUrl);
            var row = MenuRow(scene, "Main Menu");
            await row.WaitForAsync();
            await scene.HighlightAsync(row.Locator(".badge, .ta-badge").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Published|Draft") }).First);
            await scene.HighlightAsync(row.Locator(".badge, .ta-badge").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Draft") }).First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            var edit = row.Locator("a.edit");
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await edit.ClickAsync());
                await page.GetByRole(AriaRole.Button, new() { Name = "Add Menu Item" }).WaitForAsync(new() { Timeout = 60000 });
            });
        });

        await scene.StepAsync("publish-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var publish = page.Locator("button.publish").First;
            await scene.MoveToAsync(publish);
            await AtAsync(watch, Math.Max(500, (int)(durations["publish-3"] * 1000) - 5000));
            await scene.ClickAndWaitForNavigationAsync(publish);
            await page.GetByText("Your Menu has been published.").First.WaitForAsync();
        });

        await scene.StepAsync("publish-4", async () =>
        {
            await scene.NavigateAsync("/");
            await page.Locator("#mainNav").GetByText("Browse").First.WaitForAsync();
            await scene.ClickAsync(page.Locator("#mainNav").GetByText("Browse").First, pauseAfter: 500);
            await page.Locator("#mainNav").GetByText("Categories").First.WaitForAsync();
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(MenusUrl);
        await MenuRow(scene, "Main Menu").WaitForAsync();
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Link, new() { Name = "New Menu" }).Or(page.GetByRole(AriaRole.Button, new() { Name = "New Menu" })).First);
            await Label(scene, "Title").WaitForAsync();
        });

        await scene.StepAsync("create-2", async () =>
        {
            var title = Label(scene, "Title");
            await scene.ClickAsync(title, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Footer Menu", new() { Delay = 5 });
            await scene.HighlightAsync(Label(scene, "Alias"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add Menu Item" }));
        });

        await scene.StepAsync("create-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            await AtAsync(watch, Math.Max(300, (int)(durations["create-3"] * 1000) - 5000));
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.publish").First);
            await page.GetByText("Your Menu has been published.").First.WaitForAsync();
            await MenuRow(scene, "Footer Menu").WaitForAsync();
        });

        await scene.StepAsync("create-4", async () =>
        {
            var edit = MenuRow(scene, "Footer Menu").Locator("a.edit");
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await edit.ClickAsync());
                await Assertions.Expect(Label(scene, "Alias")).ToHaveValueAsync("footer-menu", new() { Timeout = 60000 });
            });
            await scene.HighlightAsync(Label(scene, "Alias"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add Menu Item" }));
        });

        await scene.ClearHighlightsAsync();
    }
}

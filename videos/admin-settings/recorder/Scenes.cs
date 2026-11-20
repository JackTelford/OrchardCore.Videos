using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Admin settings". PrepareAsync seeds six fake content items. Record in one take, in order: 02-open,
/// 03-menu, 04-topbar, 05-lists; each clip saves settings and the next starts from that state.
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
        ("02-open", OpenAsync),
        ("03-menu", MenuAsync),
        ("04-topbar", TopBarAsync),
        ("05-lists", ListsAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var items = new[]
        {
            ("Article", "Opening Day at Maple Street Bakery", true),
            ("Article", "Spring Menu Preview", false),
            ("Article", "Meet the Bakers", true),
            ("Article", "Holiday Opening Hours", true),
            ("Page", "Delivery Areas", true),
            ("Page", "Contact Us", true),
        }.Select(item => new Dictionary<string, object>
        {
            ["ContentType"] = item.Item1,
            ["ContentItemId"] = Guid.NewGuid().ToString("N")[..26],
            ["ContentItemVersionId"] = Guid.NewGuid().ToString("N")[..26],
            ["DisplayText"] = item.Item2,
            ["Latest"] = true,
            ["Published"] = item.Item3,
            ["Author"] = "admin",
            ["TitlePart"] = new Dictionary<string, string> { ["Title"] = item.Item2 },
        }).ToList();
        var json = JsonSerializer.Serialize(new { steps = new[] { new { name = "content", data = items } } });
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Seeded six items.");
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static ILocator Group(Scene scene, string name)
        => scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static async Task ToggleGroupAsync(Scene scene, string name, int pause = 400, bool open = true)
    {
        var group = Group(scene, name);
        var expanded = await group.GetAttributeAsync("aria-expanded") == "true";

        if (expanded != open)
        {
            await scene.ClickAsync(group, pauseAfter: pause);
        }
    }

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    // Saves the settings: the cursor goes to Save, the reload is cut out of the clip and the notification is shown.
    private static async Task SaveAsync(Scene scene)
    {
        var save = scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First;
        await scene.MoveToAsync(save);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(() => save.ClickAsync());
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        }, leadMilliseconds: 300);
    }

    private static ILocator Box(Scene scene, string label) => scene.Page.GetByLabel(label, new() { Exact = true }).First;

    private static async Task OpenSettingsAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/Settings/admin");
        await scene.Page.GetByLabel("Enable theme toggler", new() { Exact = true }).First.WaitForAsync();
    }

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            await ToggleGroupAsync(scene, "Settings", 500);
            var admin = scene.Page.Locator("#left-nav a[href$='/Admin/Settings/admin']").First;
            await admin.WaitForAsync();
            await scene.ClickAndWaitForNavigationAsync(admin);
            await scene.Page.GetByLabel("Enable theme toggler", new() { Exact = true }).First.WaitForAsync();
        });

        await scene.StepAsync("open-2", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("div.ocat-wrapper").Filter(new() { HasTextString = "Enable theme toggler" }).First, 1.5);
            await SaveAsync(scene);
        });
    }

    private static async Task MenuAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenSettingsAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("menu-1", async () =>
        {
            await scene.ClickAsync(Box(scene, "Enable Admin Menu filter"), pauseAfter: 150);
            await scene.HighlightAsync(Box(scene, "Enable Admin Menu filter"));
            await scene.HoldAsync(0.6);
            await scene.ClickAsync(Box(scene, "Display New menu"), pauseAfter: 150);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene);
            await scene.Page.Locator("#filter").First.WaitForAsync();
        });

        await scene.StepAsync("menu-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#filter").First);
            await scene.TypeAsync(scene.Page.Locator("#filter").First, "Roles", delay: 90);
            await scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = Exact("Roles") }).First.WaitForAsync();
        });

        await scene.StepAsync("menu-3", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.HoldAsync(0.5);
            await scene.ClickAsync(Group(scene, "New"));
            await scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = "New", Exact = true }).Locator("xpath=ancestor::li[1]").GetByRole(AriaRole.Link, new() { Name = "Article", Exact = true }).First.WaitForAsync();
        });

        await scene.StepAsync("menu-4", async () =>
        {
            var focused = scene.Page.GetByLabel("Focused", new() { Exact = true }).First;
            await scene.MoveToAsync(focused);
            await ShowAsync(scene, scene.Page.Locator("[id$='MenuBehavior']").First, 2);
            await ToggleGroupAsync(scene, "Content", 1000);
            await ToggleGroupAsync(scene, "Design", 300);
        });

        await scene.StepAsync("menu-5", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByLabel("Persistent", new() { Exact = true }).First);
            await SaveAsync(scene);
            await scene.MoveToAsync(scene.Page.GetByLabel("Persistent", new() { Exact = true }).First);
            await ToggleGroupAsync(scene, "Content", 800);

            if (!await scene.Page.Locator("#left-nav a[href$='/Admin/Layers']").First.IsVisibleAsync())
            {
                await scene.ClickAsync(Group(scene, "Design"), pauseAfter: 300);
            }
        });
    }

    private static async Task TopBarAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenSettingsAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("top-1", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("#adminQuickNavigationToggle").First, 1.5);
            await scene.Page.Keyboard.PressAsync("Control+K");
            await scene.Page.Locator("#adminQuickNavigationInput").WaitForAsync();
            await scene.HoldAsync(0.8);
            await scene.Page.Locator("#adminQuickNavigationInput").PressSequentiallyAsync("roles", new() { Delay = 90 });
            await scene.Page.Locator("#adminQuickNavigationResults li").Filter(new() { HasTextString = "Roles" }).First.WaitForAsync();
        });

        await scene.StepAsync("top-2", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.Page.Locator("#adminQuickNavigationModal").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await scene.ClickAsync(scene.Page.Locator("#bd-theme").First);

            foreach (var mode in new[] { "Auto", "Light", "Dark" })
            {
                await scene.MoveToAsync(scene.Page.Locator("button[data-bs-theme-value]").Filter(new() { HasTextRegex = Exact(mode) }).First);
                await scene.HoldAsync(1);
            }
        });

        await scene.StepAsync("top-3", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            var titles = Box(scene, "Display titles in top bar");
            await scene.MoveToAsync(titles);
            await scene.ClickAsync(titles);
            await ShowAsync(scene, titles, 1.5);
        });

        await scene.StepAsync("top-4", async () =>
        {
            var crumb = Box(scene, "Show breadcrumb");
            await scene.MoveToAsync(crumb);
            await scene.ClickAsync(crumb);
            await SaveAsync(scene);
            await scene.Page.Locator(".brand-wrapper-title").Filter(new() { HasTextString = "Settings" }).First.WaitForAsync();
        });
    }

    private static async Task ListsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenSettingsAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("lists-1", async () =>
        {
            var layout = scene.Page.GetByLabel("List layout", new() { Exact = true }).First;
            await scene.MoveToAsync(layout);
            await scene.SelectAsync(layout, "Grid");
            await ShowAsync(scene, layout, 1);
        });

        await scene.StepAsync("lists-2", async () =>
        {
            await scene.SelectAsync(scene.Page.GetByLabel("Actions layout", new() { Exact = true }).First, "Menu");
            await SaveAsync(scene);
        });

        await scene.StepAsync("lists-3", async () =>
        {
            await ToggleGroupAsync(scene, "Content", 500);
            var items = scene.Page.Locator("#left-nav").GetByRole(AriaRole.Link, new() { Name = "Content Items", Exact = true }).First;
            await scene.MoveToAsync(items);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(items), leadMilliseconds: 150);
            await scene.Page.GetByText("Last modified").First.WaitForAsync();
            await scene.ClickAsync(scene.Page.Locator("div.admin-list-grid-row").First.GetByRole(AriaRole.Button, new() { Name = "Actions", Exact = true }).First);
            await scene.Page.Locator(".actions-menu.show").First.WaitForAsync();
        });

        await scene.StepAsync("lists-4", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await ShowAsync(scene, scene.Page.Locator(".admin-list-layout-selector").First, 2);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".admin-list-layout-selector a[title='List']").First);
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "The Orchard Core admin". Order: 02-start, 03-menu, 04-pages, 05-quick, 07-topbar, then 06-content, which
/// enables Content Quick Navigation (05-quick needs it disabled).
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
        ("02-start", StartAsync),
        ("03-menu", MenuAsync),
        ("04-pages", PagesAsync),
        ("05-quick", QuickAsync),
        ("07-topbar", TopBarAsync),
        ("06-content", ContentAsync),
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

    private static ILocator Item(Scene scene, string label)
        => scene.Page.Locator($"#left-nav .item-label:has(span.title:text-is(\"{label}\"))").First;

    private static ILocator Card(Scene scene, string title)
        => scene.Page.Locator($".card:has(h5:text-is(\"{title}\"))").First;

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task OpenAdminAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
    }

    private static async Task StartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAdminAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("start-1", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator(".alert.alert-primary").First, 1.5);

            foreach (var title in new[] { "Create content", "Customize theme", "Manage users" })
            {
                await scene.MoveToAsync(Card(scene, title));
                await ShowAsync(scene, Card(scene, title), 2, clear: title != "Manage users");
            }
        });

        await scene.StepAsync("start-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(Card(scene, "Learn more"));
            await scene.HighlightAsync(Card(scene, "Learn more"));
            await scene.MoveToAsync(scene.Page.Locator(".card:has(h5:text-is(\"Learn more\")) a.btn").First);
        });

        await scene.StepAsync("start-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(scene.Page.Locator("a.ta-navbar-brand").First);
            await scene.HighlightAsync(scene.Page.Locator("a.ta-navbar-brand").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task MenuAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAdminAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("menu-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#left-nav").First);

            foreach (var label in new[] { "Content", "Design", "Access Control", "Tools", "Settings" })
            {
                await scene.MoveToAsync(Item(scene, label));
                await scene.PauseAsync(600);
            }
        });

        await scene.StepAsync("menu-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Item(scene, "Content"), pauseAfter: 800);
            await scene.ClickAsync(Item(scene, "Design"));
        });

        await scene.StepAsync("menu-3", async () =>
        {
            await scene.ClickAsync(Item(scene, "Content"), pauseAfter: 600);
            await scene.ClickAndWaitForNavigationAsync(Item(scene, "Content Items"));
        });

        await scene.StepAsync("menu-4", async () =>
        {
            await scene.MoveToAsync(Item(scene, "Blog"));
            await scene.MoveToAsync(Item(scene, "Settings"));
        });

        await scene.StepAsync("menu-5", async () =>
        {
            await ShowAsync(scene, Item(scene, "Blog"), 1.5);
            await ShowAsync(scene, Item(scene, "Main Menu"), 1.5);
            await scene.ClickAsync(Item(scene, "Tools"), pauseAfter: 600);
            await scene.HighlightAsync(Item(scene, "Admin Menus"));
        });

        await scene.StepAsync("menu-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".leftbar-compactor").First, pauseAfter: 500);
            await scene.MoveToAsync(scene.Page.Locator("#left-nav > ul > li > figure > figcaption > .item-label:has(span.title:text-is(\"Design\")) > .icon").First);
            await scene.HoldAsync(1.2);
            await scene.ClickAsync(scene.Page.Locator(".leftbar-compactor").First, pauseAfter: 300);
            await scene.Page.Locator("#left-nav .item-label span.title").First.WaitForAsync();
        });
    }

    private static async Task PagesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.Page.Locator("#items-form a:text-is('About')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("pages-1", async () =>
        {
            await scene.MoveToAsync(scene.Page.Locator("#items-form li.list-group-item").First);
            await scene.MoveToAsync(scene.Page.Locator("#items-form li.list-group-item").Last);
            await scene.HoldAsync(2.5);
            await ShowAsync(scene, Item(scene, "Content Items"), 1.5);
            await scene.HighlightAsync(scene.Page.Locator("h1.oc-breadcrumb-title").First);
        });

        await scene.StepAsync("pages-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#items-form a:text-is('About')").First);
            await scene.HighlightAsync(scene.Page.Locator("nav[aria-label='Breadcrumb']").First);
        });

        await scene.StepAsync("pages-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var crumb = scene.Page.Locator("nav[aria-label='Breadcrumb'] a:text-is('Manage Content')").First;
            await scene.MoveToAsync(crumb);
            await scene.ClickAndWaitForNavigationAsync(crumb);
        });
    }

    private static async Task QuickAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.Page.Locator("#adminQuickNavigationToggle").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("quick-1", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("#adminQuickNavigationToggle").First, 1.5);
            await scene.Page.Keyboard.PressAsync("Control+K");
            await scene.Page.Locator("#adminQuickNavigationModal.show").WaitForAsync();
            await scene.Page.Locator("#adminQuickNavigationResults li").First.WaitForAsync();
        });

        await scene.StepAsync("quick-2", async () =>
        {
            await scene.Page.Locator("#adminQuickNavigationInput").PressSequentiallyAsync("user", new() { Delay = 120 });
        });

        await scene.StepAsync("quick-3", async () =>
        {
            await scene.Page.Keyboard.PressAsync("ArrowDown");
            await scene.HoldAsync(0.9);
            await scene.Page.Keyboard.PressAsync("ArrowUp");
            await scene.HoldAsync(0.9);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
        });
    }

    private static async Task TopBarAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenAdminAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("top-1", async () =>
        {
            var visit = scene.Page.Locator(".user-top-navbar a[aria-label='Visit Site']").First;
            await scene.MoveToAsync(visit);
            await scene.HoldAsync(1);
            await scene.ClickAndWaitForNavigationAsync(visit);
        });

        await scene.StepAsync("top-2", async () =>
        {
            await scene.NavigateAsync("/Admin");
            await scene.HoldAsync(1.0);
            await scene.ClickAsync(scene.Page.Locator("#bd-theme").First, pauseAfter: 1000);
            await scene.ClickAsync(scene.Page.Locator("button[data-bs-theme-value='dark']").First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator("#bd-theme").First, pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator("button[data-bs-theme-value='light']").First, pauseAfter: 600);
        });

        await scene.StepAsync("top-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#navbarDropdown").First, pauseAfter: 1000);

            foreach (var entry in new[] { "Profile", "Change password" })
            {
                await scene.MoveToAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button").Filter(new() { HasTextString = entry }).First);
                await scene.PauseAsync(700);
            }
        });
    }

    private static async Task ContentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        const string Enable = "#btn-enable-OrchardCore_Contents_QuickNavigation";

        await scene.StepAsync("content-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "quick", delay: 110);
            await scene.Page.Locator(Enable).WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(Enable));
        });

        await scene.StepAsync("content-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(Enable));
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
            await scene.HoldAsync(1.5);
        });

        await scene.StepAsync("content-3", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Control+K");
            await scene.Page.Locator("#adminQuickNavigationModal.show").WaitForAsync();
            await scene.HoldAsync(0.8);
            await scene.Page.Locator("#adminQuickNavigationInput").PressSequentiallyAsync("about", new() { Delay = 120 });
            await scene.Page.Locator("#adminQuickNavigationResults li").Filter(new() { HasTextString = "About" }).First.WaitForAsync();
        });

        await scene.StepAsync("content-4", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.HoldAsync(0.5);
            await scene.ClickAsync(Item(scene, "Settings"), pauseAfter: 600);
            await scene.ClickAndWaitForNavigationAsync(Item(scene, "Admin"));
            var label = scene.Page.Locator("label:text-is('Recent content items in quick navigation')").First;
            await label.WaitForAsync();
            await label.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(label);
        });

        await scene.StepAsync("content-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, -3000);

            foreach (var label in new[] { "Enable theme toggler", "Enable Admin Menu filter", "Navigation menu behavior" })
            {
                await ShowAsync(scene, scene.Page.Locator($"label:text-is('{label}')").First, 2, clear: label != "Navigation menu behavior");
            }
        });

        await scene.ClearHighlightsAsync();
    }
}

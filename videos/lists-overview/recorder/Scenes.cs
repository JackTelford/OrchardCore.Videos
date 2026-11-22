using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Lists overview". Record in one take, in order: 03-type creates the Help Center type, 04-items creates
/// the list and three pages, 05-find and 06-site only read.
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
        ("02-blog", BlogAsync),
        ("03-type", TypeAsync),
        ("04-items", ItemsAsync),
        ("05-find", FindAsync),
        ("06-site", SiteAsync),
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

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, string caption = null, bool clear = true)
    {
        await scene.HighlightAsync(target, caption);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static ILocator Entry(Scene scene, string name)
        => scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static async Task OpenGroupAsync(Scene scene, string name, int pause = 400)
    {
        var group = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact(name) }).First;

        if (await group.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(group, pauseAfter: pause);
        }
    }

    private static ILocator Row(Scene scene, string text)
        => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static ILocator Button(Scene scene, string text)
        => scene.Page.Locator("a, button").Filter(new() { HasTextRegex = Exact(text) }).First;

    private static async Task BlogAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("blog-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Entry(scene, "Blog"));
            await scene.Page.GetByText("Edit Blog").First.WaitForAsync();
        });

        await scene.StepAsync("blog-2", async () =>
        {
            await ShowAsync(scene, Button(scene, "Edit Blog"), 2);
            await ShowAsync(scene, Button(scene, "Create Blog Post"), 2);
            await ShowAsync(scene, scene.Page.Locator("li.list-group-item").First, 2);
        });

        await scene.StepAsync("blog-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TypeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("type-1", async () =>
        {
            await OpenGroupAsync(scene, "Design");
            await OpenGroupAsync(scene, "Content Definition");
            await scene.ClickAndWaitForNavigationAsync(Entry(scene, "Content Types"));
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btnCreate").First);
        });

        await scene.StepAsync("type-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#DisplayName").First, "Help Center", delay: 55);
            await scene.Page.WaitForFunctionAsync("() => document.querySelector('#Name').value.length > 0");
            await scene.Page.Keyboard.PressAsync("Tab");
            await ShowAsync(scene, scene.Page.Locator("#Name").First, 2);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.create").First);
        });

        await scene.StepAsync("type-3", async () =>
        {
            foreach (var part in new[] { "Autoroute", "List", "Title" })
            {
                var box = scene.Page.Locator($".form-check:has(label:text-is('{part}')) input[type=checkbox]").First;
                await box.ScrollIntoViewIfNeededAsync();
                await scene.ClickAsync(box, pauseAfter: 250);
            }
        });

        await scene.StepAsync("type-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#add-selected-parts-button").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#parts li:has-text('Add a list behavior') a:has-text('Edit')").First);
        });

        await scene.StepAsync("type-5", async () =>
        {
            foreach (var label in new[] { "Page Size", "Enable Ordering", "Contained Content Types", "Show container", "Show full pager" })
            {
                await ShowAsync(scene, scene.Page.Locator($"label:has-text('{label}')").First, 1.4, clear: label != "Show full pager");
            }
        });

        await scene.StepAsync("type-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var page = scene.Page.Locator(".content-types label:text-is('Page')").First;
            await page.ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(page);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
        });
    }

    // The page load is cut out of the clip: the cursor is already on the button, and the click happens off camera.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(target), leadMilliseconds: 150);
    }

    private static async Task ItemsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.Page.Locator("#new-dropdown").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("items-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown").First);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-item").Filter(new() { HasTextRegex = Exact("Help Center") }).First);
        });

        await scene.StepAsync("items-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator(".dropdown-item").Filter(new() { HasTextRegex = Exact("Help Center") }).First);
            await scene.TypeAsync(scene.Page.Locator("input[name='TitlePart.Title']").First, "Help Center", delay: 12);
            await ShowAsync(scene, scene.Page.Locator(".hint:has-text('automatically generated')").First, 2);
            await NavAsync(scene, scene.Page.Locator("button.publish").First);
        });

        await scene.StepAsync("items-3", async () =>
        {
            await NavAsync(scene, Button(scene, "List Page Items"));
            await scene.Page.GetByText("The list is empty.").First.WaitForAsync();
        });

        await scene.StepAsync("items-4", async () =>
        {
            await ShowAsync(scene, scene.Page.GetByText("The list is empty.").First, 2);
            await NavAsync(scene, Button(scene, "Create Page"));
        });

        await scene.StepAsync("items-5", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[name='TitlePart.Title']").First, "Reset your password", delay: 12);
            await NavAsync(scene, scene.Page.Locator("button.publish").First);
        });

        await scene.StepAsync("items-6", async () =>
        {
            await NavAsync(scene, Button(scene, "Create Page"));
            await scene.TypeAsync(scene.Page.Locator("input[name='TitlePart.Title']").First, "Export your data", delay: 12);
            await NavAsync(scene, scene.Page.Locator("button.draft").First);
        });

        await scene.StepAsync("items-7", async () =>
        {
            await NavAsync(scene, Button(scene, "Create Page"));
            await scene.TypeAsync(scene.Page.Locator("input[name='TitlePart.Title']").First, "Change your email address", delay: 12);
            await NavAsync(scene, scene.Page.Locator("button.publish").First);
        });
    }

    private static ILocator HelpRow(Scene scene)
        => scene.Page.Locator("li.list-group-item:has(a:text-is('List Page Items'))").First;

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems?q=type:HelpCenter");
        await HelpRow(scene).WaitForAsync();
        await HelpRow(scene).Locator("a:text-is('List Page Items')").First.ClickAsync();
        await scene.Page.GetByText("Change your email address").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            var first = scene.Page.Locator("li.list-group-item:has(a:has-text('Edit'))").First;
            await ShowAsync(scene, first.Locator("a").Filter(new() { HasTextRegex = Exact("Edit") }).First, 3);
            await ShowAsync(scene, first.Locator("a").Filter(new() { HasTextRegex = Exact("View") }).First, 3);
        });

        await scene.StepAsync("find-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#filter-dropdown").First);

            foreach (var entry in new[] { "Only draft items", "Only published items", "Owned by me" })
            {
                await ShowAsync(scene, scene.Page.Locator(".dropdown-item").Filter(new() { HasTextRegex = Exact(entry) }).First, 1);
            }

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-item").Filter(new() { HasTextRegex = Exact("Only draft items") }).First);
        });

        await scene.StepAsync("find-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#filter-dropdown").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-item").Filter(new() { HasTextRegex = Exact("Only published items") }).First);
        });

        await scene.StepAsync("find-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[name='DisplayText']").First, "password", delay: 80);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
        });
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems?q=type:HelpCenter");
        await HelpRow(scene).WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            var view = HelpRow(scene).Locator("a").Filter(new() { HasTextRegex = Exact("View") }).First;
            await scene.MoveToAsync(view);
            var popup = await scene.Page.Context.RunAndWaitForPageAsync(() => scene.ClickAsync(view));
            await popup.WaitForLoadStateAsync();
            await scene.SwitchToAsync(popup);
            await scene.Page.GetByText("Help Center").First.WaitForAsync();
        });

        await scene.StepAsync("site-2", async () =>
        {
            var list = scene.Page.Locator("ul, ol, .list-group").Filter(new() { HasTextString = "Reset your password" }).Last;
            await scene.HighlightAsync(list);
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextRegex = Exact("Reset your password") }).First);
        });
    }
}

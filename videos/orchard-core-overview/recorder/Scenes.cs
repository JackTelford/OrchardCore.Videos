using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The recordings of the Orchard Core overview. Nothing is saved on the site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-tour", TourAsync),
        ("03-items", ItemsAsync),
        ("03-types", TypesAsync),
        ("04-design", DesignAsync),
        ("05-features", FeaturesAsync),
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

    // A group of the left menu is a button: it expands when it is collapsed.
    private static ILocator Group(Scene scene, string name)
        => scene.Page.Locator("#left-nav button, #left-nav a").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static async Task OpenGroupAsync(Scene scene, string name)
    {
        var group = Group(scene, name);

        if (await group.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(group, pauseAfter: 700);
        }
    }

    private static ILocator Entry(Scene scene, string name)
        => scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static ILocator Row(Scene scene, string text)
        => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task TourAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("header.masthead").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("tour-1", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("#mainNav a").Filter(new() { HasTextString = "Home" }).First, 1);
            await ShowAsync(scene, scene.Page.Locator("#mainNav a").Filter(new() { HasTextString = "About" }).First, 1);
            await scene.ScrollAsync(0, 600);
        });

        await scene.StepAsync("tour-2", async () =>
        {
            await scene.NavigateAsync("/Admin");
            await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();

            foreach (var title in new[] { "Create content", "Customize theme", "Manage users" })
            {
                await ShowAsync(scene, scene.Page.Locator(".card-title").Filter(new() { HasTextString = title }).First, 1);
            }
        });

        await scene.StepAsync("tour-3", async () =>
        {
            await ShowAsync(scene, Group(scene, "Content"), 1.5);
            await ShowAsync(scene, Group(scene, "Design"), 1.5);
            await ShowAsync(scene, Group(scene, "Tools"), 1.5, clear: false);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ItemsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("items-1", async () =>
        {
            await OpenGroupAsync(scene, "Content");
            await scene.ClickAndWaitForNavigationAsync(Entry(scene, "Content Items"));
            await Row(scene, "About").WaitForAsync();
            await scene.HoldAsync(1);
            await scene.HighlightAsync(Row(scene, "About"));
        });

        await scene.StepAsync("items-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "About").Locator("a.edit").First);
            await scene.Page.Locator("label").Filter(new() { HasTextString = "Title" }).First.WaitForAsync();

            foreach (var label in new[] { "Title", "Permalink", "HtmlBody" })
            {
                await ShowAsync(scene, scene.Page.Locator("label").Filter(new() { HasTextString = label }).First, 1);
            }
        });

        await scene.StepAsync("items-3", async () =>
        {
            await scene.ScrollAsync(0, 2000);
            await scene.HoldAsync(0.5);

            foreach (var selector in new[] { "button.publish", "button.draft", "a#previewButton" })
            {
                await ShowAsync(scene, scene.Page.Locator(selector).First, 2, clear: selector != "a#previewButton");
            }
        });
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("types-1", async () =>
        {
            await OpenGroupAsync(scene, "Design");
            await OpenGroupAsync(scene, "Content Definition");
            await scene.ClickAndWaitForNavigationAsync(Entry(scene, "Content Types"));
            await scene.Page.Locator("a[role='btn-edit-Article']").First.WaitForAsync();

            foreach (var type in new[] { "Article", "Blog", "Page" })
            {
                await ShowAsync(scene, scene.Page.Locator($"li.list-group-item:has(a[role='btn-edit-{type}'])").First, 1.5);
            }
        });

        await scene.StepAsync("types-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[role='btn-edit-Article']").First);
            await scene.Page.GetByText("Edit Content Type").First.WaitForAsync();
            await scene.Page.Locator("ul#fields > li").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 250);

            foreach (var selector in new[] { "ul#fields > li:nth-child(1)", "ul#fields > li:nth-child(2)", "ul#parts > li:nth-child(1)", "ul#parts > li:nth-child(2)", "ul#parts > li:nth-child(3)" })
            {
                await ShowAsync(scene, scene.Page.Locator(selector).First, 1.2);
            }
        });

        await scene.StepAsync("types-3", async () =>
        {
            var addField = scene.Page.Locator("a, button").Filter(new() { HasTextRegex = Exact("Add Field") }).First;
            var addParts = scene.Page.Locator("a, button").Filter(new() { HasTextRegex = Exact("Add Parts") }).First;
            await addField.ScrollIntoViewIfNeededAsync();
            await ShowAsync(scene, addField, 1.5);
            await addParts.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(addParts);
        });
    }

    private static async Task DesignAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("design-1", async () =>
        {
            await OpenGroupAsync(scene, "Design");
            await scene.ClickAndWaitForNavigationAsync(Entry(scene, "Themes"));
            await scene.Page.GetByText("The Blog Theme").First.WaitForAsync();
            await ShowAsync(scene, scene.Page.GetByText("This is the current Site theme").First, 1);
            await ShowAsync(scene, scene.Page.GetByText("This is the current Admin theme").First, 1);
            var make = scene.Page.GetByText("Make Current").First;
            await make.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(make);
        });

        await scene.StepAsync("design-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Entry(scene, "Widgets"));
            await scene.Page.GetByText("Widgets and Layers").First.WaitForAsync();

            foreach (var selector in new[] { "div.card:has(.layer-zone[data-zone='Content'])", "div.card:has(.layer-zone[data-zone='Footer'])", ".layer-zone[data-zone='Footer'] li.list-group-item" })
            {
                await ShowAsync(scene, scene.Page.Locator(selector).First, 2);
            }
        });

        await scene.StepAsync("design-3", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("li.list-group-item:has(input#Always)").First, 3);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item:has(input#Homepage)").First);
        });
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await scene.ClickAndWaitForNavigationAsync(Entry(scene, "Features"));
            await scene.Page.GetByText("Showing").First.WaitForAsync();
            await ShowAsync(scene, scene.Page.Locator(".feature-group:visible h3").First, 2);
            await scene.HighlightAsync(scene.Page.Locator(".feature-group li:visible").Filter(new() { Has = scene.Page.Locator(".direct-dependencies .badge") }).First);
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Workflows", delay: 60);
            var row = scene.Page.Locator("li[data-filter-value]").Filter(new() { Has = scene.Page.Locator("span.fw-semibold").Filter(new() { HasTextRegex = Exact("Workflows") }) }).First;
            await row.WaitForAsync();
            await row.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await scene.HoldAsync(0.4);
            await ShowAsync(scene, row.Locator(".badge").Filter(new() { HasTextString = "Disabled" }).First, 2);
            await scene.HighlightAsync(row.Locator("a").Filter(new() { HasTextRegex = Exact("Enable") }).First);
        });

        await scene.ClearHighlightsAsync();
    }
}

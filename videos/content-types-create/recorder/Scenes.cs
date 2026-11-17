using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Content Types: Create a Type and Add Parts and Fields". Each clip starts from the state the
/// previous one left: restore the snapshot before a new take (see recording-plan.md).
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
        ("02-create", CreateAsync),
        ("03-parts", PartsAsync),
        ("04-fields", FieldsAsync),
        ("05-order", OrderAsync),
        ("06-item", ItemAsync),
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

    private static ILocator AddField(Scene scene) => scene.Page.Locator("a", new() { HasTextRegex = new("^\\s*Add Field\\s*$") }).First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("button[name='submit.Save'][value='Save']").First;

    private static async Task AddFieldAsync(Scene scene, string name, string type, int delay = 10)
    {
        await scene.ClickAndWaitForNavigationAsync(AddField(scene));
        await scene.TypeAsync(scene.Page.Locator("#DisplayName"), name, delay);
        await scene.Page.Keyboard.PressAsync("Tab");
        await scene.ClickAsync(scene.Page.GetByLabel(type, new() { Exact = true }), pauseAfter: 300);
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            var menu = scene.Page.Locator("#left-nav");
            var design = menu.GetByRole(AriaRole.Button, new() { Name = "Design", Exact = true });
            await design.WaitForAsync();
            await scene.ClickAsync(design, pauseAfter: 500);
            var definition = menu.GetByRole(AriaRole.Button, new() { Name = "Content Definition", Exact = true });
            await definition.WaitForAsync();
            await scene.ClickAsync(definition, pauseAfter: 500);
            var types = menu.GetByRole(AriaRole.Link, new() { Name = "Content Types", Exact = true });
            await types.WaitForAsync();
            await scene.ClickAndWaitForNavigationAsync(types);
        });

        await scene.StepAsync("create-2", async () =>
        {
            foreach (var selector in new[] { "[role='btn-edit-Article']", "[role='btn-list-Article']", "[role='btn-delete-Article']", "#btn-create-Article" })
            {
                await scene.HighlightAsync(scene.Page.Locator(selector).First);
            }
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.HighlightAsync(scene.Page.Locator("#btnCreate"));
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btnCreate"));
            await scene.TypeAsync(scene.Page.Locator("#DisplayName"), "Event", 60);
            await scene.Page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#Name"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button.create"));
        });

        await scene.StepAsync("create-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.create"));
        });
    }

    private static async Task PartsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/AddPartsTo/Event");
        await scene.StartCaptureAsync();

        await scene.StepAsync("parts-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("ul.list-group").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByLabel("Title", new() { Exact = true }), pauseAfter: 400);
        });

        await scene.StepAsync("parts-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#add-selected-parts-button"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button[name='submit.AddAndConfigure'][value='TitlePart']"));
        });

        await scene.StepAsync("parts-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#add-selected-parts-button"));
            await scene.HighlightAsync(scene.Page.GetByLabel("Creatable"));
            await scene.HighlightAsync(scene.Page.GetByLabel("Listable"));
        });

        await scene.StepAsync("parts-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("ul#parts").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 250, 6);
            var row = scene.Page.Locator("ul#parts li").Filter(new() { HasText = "Title" }).First;
            await scene.HighlightAsync(row);
            await scene.HighlightAsync(row.Locator("a, button").Filter(new() { HasTextRegex = new("^\\s*Edit\\s*$") }).First);
            await scene.HighlightAsync(row.Locator("a, button").Filter(new() { HasTextRegex = new("^\\s*Remove\\s*$") }).First);
        });
    }

    private static async Task FieldsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Event");
        await scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Fields" }).First.ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("fields-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(AddField(scene));
            await scene.TypeAsync(scene.Page.Locator("#DisplayName"), "Venue", 40);
            await scene.Page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("fields-2", async () =>
        {
            await scene.ScrollAsync(0, 300, 6);
            await scene.ClickAsync(scene.Page.GetByLabel("Text Field", new() { Exact = true }), pauseAfter: 300);
            await scene.HighlightAsync(SaveButton(scene));
        });

        await scene.StepAsync("fields-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.Page.Locator("ul#fields").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200, 6);
            await scene.HighlightAsync(scene.Page.Locator("ul#parts li").Filter(new() { HasText = "Contains the fields for the current type" }).First);
        });

        await scene.StepAsync("fields-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("ul#fields").ScrollIntoViewIfNeededAsync();
            await AddFieldAsync(scene, "Starts", "Date Time Field", 20);
            await scene.HighlightAsync(scene.Page.Locator("button[value='SaveAndConfigure']"));
        });

        await scene.StepAsync("fields-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[value='SaveAndConfigure']"));
            await scene.HighlightAsync(scene.Page.Locator("input#Name[readonly]").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByLabel("Required", new() { Exact = true }), pauseAfter: 300);
            await scene.TypeAsync(scene.Page.GetByLabel("Hint", new() { Exact = true }), "Local time at the venue.", 8);
        });

        await scene.StepAsync("fields-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.Page.Locator("ul#fields").ScrollIntoViewIfNeededAsync();
            await AddFieldAsync(scene, "Capacity", "Numeric Field", 15);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.Page.Locator("ul#fields").ScrollIntoViewIfNeededAsync();
        });

        await scene.StepAsync("fields-7", async () =>
        {
            var row = scene.Page.Locator("ul#fields li").Filter(new() { HasText = "Venue" }).First;
            await scene.HighlightAsync(row.Locator("a").Filter(new() { HasTextRegex = new("^\\s*Edit\\s*$") }).First);
            await scene.HighlightAsync(row.Locator("a, button").Filter(new() { HasTextRegex = new("^\\s*Remove\\s*$") }).First);
        });
    }

    private static async Task OrderAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Event");
        await scene.Page.Locator("ul#fields").ScrollIntoViewIfNeededAsync();
        await scene.ScrollAsync(0, 200, 6);
        await scene.StartCaptureAsync();

        await scene.StepAsync("order-1", async () =>
        {
            var fields = scene.Page.Locator("ul#fields li");
            await scene.DragToAsync(fields.Filter(new() { HasText = "Starts" }).First, fields.Filter(new() { HasText = "Venue" }).First, 0, -10);
        });

        await scene.StepAsync("order-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task ItemAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("item-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-create-Event") }).First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#btn-create-Event"));
        });

        await scene.StepAsync("item-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btn-create-Event"));
            await scene.HighlightAsync(scene.Page.GetByText("Starts", new() { Exact = true }).First);
        });

        await scene.StepAsync("item-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.GetByLabel("Title", new() { Exact = true }), "Community Meetup", 8);
            var starts = scene.Page.Locator("input[type='datetime-local']");
            await scene.MoveToAsync(starts);
            await starts.FillAsync("2031-03-14T18:30");
            await scene.TypeAsync(scene.Page.GetByLabel("Venue", new() { Exact = true }), "Riverside Hall", 8);
            await scene.TypeAsync(scene.Page.GetByLabel("Capacity", new() { Exact = true }), "120", 8);
            await scene.HighlightAsync(scene.Page.Locator("button.publish"));
        });

        await scene.StepAsync("item-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish"));
            await scene.PauseAsync(2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("[role='btn-list-Event']"));
        });
    }
}

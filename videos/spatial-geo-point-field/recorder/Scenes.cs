using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Geo Point Field". The browser loads the map tiles from the public OpenStreetMap tile server.
/// Record in one take, in order: 02-enable (Spatial and Lucene), 03-add (the field Location on Page), 04-enter (the page Find Us),
/// 05-check. No PrepareAsync.
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
        ("02-enable", EnableAsync),
        ("03-add", AddAsync),
        ("04-enter", EnterAsync),
        ("05-check", CheckAsync),
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

    private static ILocator Group(Scene scene, string name)
        => scene.Page.Locator("#left-nav button:visible").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static async Task OpenGroupAsync(Scene scene, string name)
    {
        var group = Group(scene, name);

        if (await group.GetAttributeAsync("aria-expanded") == "false")
        {
            await group.ClickAsync();
            await scene.Page.WaitForTimeoutAsync(500);
        }
    }

    private static async Task<Scene> OpenDashboardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, params string[] groups)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();

        foreach (var group in groups)
        {
            await OpenGroupAsync(scene, group);
        }

        await scene.StartCaptureAsync();
        return scene;
    }

    // Clicks something that loads another page, with the wait cut out of the clip.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations, "Tools");
        var row = scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-enable-OrchardCore_Spatial, #btn-disable-OrchardCore_Spatial") }).First;

        await scene.StepAsync("enable-1", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("#left-nav a[href$='/Admin/Features']").First);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Spatial", delay: 50);
            await scene.Page.Locator("#list-summary").GetByText("Showing 1 of").First.WaitForAsync();
            await scene.MoveToAsync(row.Locator(".direct-dependencies").First);
            await scene.HighlightAsync(row.Locator(".direct-dependencies").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("#btn-enable-OrchardCore_Spatial").First);
            await scene.Page.GetByText("The feature Spatial was enabled.").First.WaitForAsync(new() { Timeout = 90000 });
            await scene.MoveToAsync(scene.Page.GetByText("The feature Spatial was enabled.").First);
            await scene.PauseAsync(2500);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "Spatial", delay: 20);
        });
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations, "Design", "Content Definition");

        await scene.StepAsync("add-1", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("#left-nav a[href$='/Admin/ContentTypes/List']").First);
            var edit = scene.Page.Locator("[role='btn-edit-Page']").First;
            await edit.ScrollIntoViewIfNeededAsync();
            await NavAsync(scene, edit);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await NavAsync(scene, scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Field", Exact = true }).First);
            await scene.TypeAsync(scene.Page.GetByLabel("Display Name", new() { Exact = true }).First, "Location", delay: 40);
            await scene.Page.Keyboard.PressAsync("Tab");
            var radio = scene.Page.GetByLabel("Geo Point Field", new() { Exact = true }).First;
            await radio.ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(radio, pauseAfter: 300);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("button[value='SaveAndConfigure']").First);
            var required = scene.Page.GetByLabel("Required", new() { Exact = true }).First;
            await scene.MoveToAsync(required);
            await scene.HighlightAsync(required);
            await scene.HoldAsync(1.5);
            await scene.HighlightAsync(scene.Page.GetByLabel("Hint", new() { Exact = true }).First);
        });

        await scene.StepAsync("add-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByLabel("Required", new() { Exact = true }).First, pauseAfter: 300);
            await scene.TypeAsync(scene.Page.GetByLabel("Hint", new() { Exact = true }).First, "Where visitors can find our shop.", delay: 25);
        });

        await scene.StepAsync("add-5", async () =>
        {
            var select = scene.Page.Locator("select[data-field-editor-select]").First;
            await select.ScrollIntoViewIfNeededAsync();
            await scene.SelectAsync(select, "Leaflet");
            var save = scene.Page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText("field settings have been saved").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("field settings have been saved").First);
        });

        await scene.StepAsync("add-6", async () =>
        {
            var row = scene.Page.Locator("#fields .list-group-item").Filter(new() { HasText = "Location" }).First;
            await scene.MoveToAsync(row);
            await scene.HighlightAsync(row);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EnterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations, "Content");

        await scene.StepAsync("enter-1", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("#left-nav a[href$='/Admin/Contents/ContentItems/']").First);
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown").First, pauseAfter: 300);
            await NavAsync(scene, scene.Page.Locator("#new-dropdown + .dropdown-menu").GetByRole(AriaRole.Link, new() { Name = "Page", Exact = true }).First);
            await scene.TypeAsync(scene.Page.GetByLabel("Title", new() { Exact = true }).First, "Find Us", delay: 40);
            await scene.Page.Locator(".leaflet-tile-loaded").First.WaitForAsync(new() { Timeout = 30000 });
        });

        await scene.StepAsync("enter-2", async () =>
        {
            await scene.Page.Locator("[data-leaflet-editor]").First.EvaluateAsync("e => e.scrollIntoView({ behavior: 'smooth', block: 'center' })");
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("enter-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-leaflet-map]").First, offsetX: -95, offsetY: -10, pauseAfter: 600);
        });

        await scene.StepAsync("enter-4", async () =>
        {
            var latitude = scene.Page.Locator("[data-latitude]").First;
            await scene.ClickAsync(latitude, pauseAfter: 200);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await latitude.PressSequentiallyAsync("47.6062", new() { Delay = 55 });
            var longitude = scene.Page.Locator("[data-longitude]").First;
            await scene.ClickAsync(longitude, pauseAfter: 200);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await longitude.PressSequentiallyAsync("-122.3321", new() { Delay = 55 });
            await scene.Page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("enter-5", async () =>
        {
            var publish = scene.Page.Locator("button.publish").First;
            await publish.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(publish);
            await scene.PauseAsync(1000);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => publish.ClickAsync());
                await scene.Page.GetByText("Your Page has been published.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("Your Page has been published.").First);
        });
    }

    private static async Task CheckAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Find Us" }).First.WaitForAsync();
        await scene.StartCaptureAsync();

        var row = scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Find Us" }).First;

        await scene.StepAsync("check-1", async () =>
        {
            await NavAsync(scene, row.Locator("a.btn.edit").First);
            await scene.Page.Locator(".leaflet-tile-loaded").First.WaitForAsync(new() { Timeout = 30000 });
            await scene.Page.Locator("[data-leaflet-editor]").First.EvaluateAsync("e => e.scrollIntoView({ behavior: 'smooth', block: 'center' })");
            await scene.PauseAsync(1200);
        });

        await scene.StepAsync("check-2", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("a:text-is('Manage Content')").First);
            var view = scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Find Us" }).First.Locator("a.btn.view").First;
            await scene.MoveToAsync(view);
            var page = await scene.Page.Context.RunAndWaitForPageAsync(() => view.ClickAsync());
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await scene.SwitchToAsync(page);
            var field = scene.Page.Locator(".field-type-geopointfield").First;
            await field.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(field);
        });

        await scene.ClearHighlightsAsync();
    }
}

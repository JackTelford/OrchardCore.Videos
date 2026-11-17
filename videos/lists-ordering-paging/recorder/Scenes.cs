using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Lists: Ordering, Paging and Contained Types". PrepareAsync imports seed.json (the blog Garden Notes with
/// eight posts). Record in one take, in order: 02-open, 03-types, 04-size, 05-ordering, 06-display. Each clip leaves the
/// settings the next one starts from.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string List = "/Admin/Contents/ContentItems/4gardennotesblog0000000001/Display";

    private const string Settings = "/Admin/ContentTypes/Blog/ContentParts/ListPart/Edit";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-open", OpenAsync),
        ("03-types", TypesAsync),
        ("04-size", SizeAsync),
        ("05-ordering", OrderingAsync),
        ("06-display", DisplayAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var json = File.ReadAllText(Path.Combine(Root, "recorder", "seed.json"));
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Imported the blog Garden Notes and its eight posts.");
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

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.Dialog += (_, dialog) =>
        {
            Console.WriteLine("DIALOG: " + dialog.Message);
            _ = dialog.DismissAsync();
        };

        return scene;
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

    // Clicks something that loads another page, with the wait cut out of the clip.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);
    }

    // Opens a page with the load cut out of the clip.
    private static async Task GoAsync(Scene scene, string path, ILocator ready)
    {
        await scene.OffCameraAsync(async () =>
        {
            await scene.NavigateAsync(path);
            await ready.WaitForAsync();
        }, leadMilliseconds: 100);
    }

    private static ILocator Setting(Scene scene, string label) => scene.Page.Locator($".form-check:has(label:text-is(\"{label}\"))").First;

    private static ILocator Field(Scene scene, string label) => scene.Page.Locator($".ocat-wrapper:has(label:text-is('{label}'))").First;

    private static ILocator Saved(Scene scene) => scene.Page.GetByText("part settings have been saved").First;

    private static async Task SaveAsync(Scene scene)
    {
        var save = scene.Page.Locator("button.save").First;
        await scene.MoveToAsync(save);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(() => save.ClickAsync());
            await Saved(scene).WaitForAsync();
        }, leadMilliseconds: 150);
        await scene.MoveToAsync(Saved(scene));
    }

    private static ILocator Next(Scene scene) => scene.Page.Locator("li[title='Go to next page.'] a").First;

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.Page.Locator("a[role='btn-edit-Blog']").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            var edit = scene.Page.Locator("a[role='btn-edit-Blog']").First;
            await scene.MoveToAsync(edit);
            await scene.HighlightAsync(edit);
            await scene.HoldAsync(1);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(edit), leadMilliseconds: 150);
        });

        await scene.StepAsync("open-2", async () =>
        {
            var row = scene.Page.Locator("li:has(a[href$='/ContentTypes/Blog/ContentParts/ListPart/Edit'])").First;
            await row.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(row);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("a[href$='/ContentTypes/Blog/ContentParts/ListPart/Edit']").First);
        });

        await scene.StepAsync("open-3", async () =>
        {
            foreach (var label in new[] { "Page Size", "Enable Ordering", "Contained Content Types", "Show container's header", "Show full pager" })
            {
                var target = label is "Page Size" or "Contained Content Types"
                    ? scene.Page.Locator($"label:text-is(\"{label}\")").First
                    : Setting(scene, label);
                await target.ScrollIntoViewIfNeededAsync();
                await ShowAsync(scene, target, 0.8);
            }
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(List);
        await scene.Page.GetByText("Create Blog Post").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("types-1", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("a:has-text('Create Blog Post')").First, 1.5);
            await scene.HighlightAsync(scene.Page.Locator("#ci-sortable, ul.list-group").First);
        });

        await scene.StepAsync("types-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, Settings, scene.Page.Locator(".content-types").First);
            await scene.Page.Locator(".content-types").First.ScrollIntoViewIfNeededAsync();
            await ShowAsync(scene, scene.Page.Locator(".content-types").First, 1);
            await scene.ClickAsync(scene.Page.Locator(".content-types label:text-is('Article')").First, pauseAfter: 300);
            await SaveAsync(scene);
        });

        await scene.StepAsync("types-3", async () =>
        {
            await GoAsync(scene, List, scene.Page.Locator("a.dropdown-toggle:has-text('New')").First);
            await scene.ClickAsync(scene.Page.Locator("a.dropdown-toggle:has-text('New')").First, pauseAfter: 300);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SizeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(List);
        await scene.Page.GetByText("Spring planting calendar").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("size-1", async () =>
        {
            await scene.MoveToAsync(scene.Page.GetByText("Spring planting calendar").First);
            await scene.MoveToAsync(scene.Page.GetByText("Planning the vegetable beds").First);
        });

        await scene.StepAsync("size-2", async () =>
        {
            await GoAsync(scene, Settings, Field(scene, "Page Size"));
            var size = Field(scene, "Page Size").Locator("input").First;
            await scene.ClickAsync(size, pauseAfter: 200);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await scene.Page.Keyboard.TypeAsync("3", new() { Delay = 80 });
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(300);
            await SaveAsync(scene);
        });

        await scene.StepAsync("size-3", async () =>
        {
            await GoAsync(scene, List, Next(scene));
            await scene.HighlightAsync(Next(scene));
        });

        await scene.StepAsync("size-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "/garden-notes", scene.Page.GetByText("Older Posts").First);
            await scene.Page.GetByText("Older Posts").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200);
        });

        await scene.StepAsync("size-5", async () =>
        {
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(scene.Page.GetByText("Older Posts").First), leadMilliseconds: 150);
            await scene.Page.GetByText("Newer Posts").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task OrderingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(Settings);
        await Setting(scene, "Enable Ordering").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("order-1", async () =>
        {
            await Setting(scene, "Enable Ordering").ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(Setting(scene, "Enable Ordering"));
        });

        await scene.StepAsync("order-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("label:text-is('Enable Ordering')").First, pauseAfter: 300);
            await SaveAsync(scene);
        });

        await scene.StepAsync("order-3", async () =>
        {
            await GoAsync(scene, List, scene.Page.Locator("#ci-sortable .ui-sortable-handle").First);
            await scene.HighlightAsync(scene.Page.Locator("#ci-sortable .ui-sortable-handle").First);
        });

        await scene.StepAsync("order-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, Next(scene));
            var handles = scene.Page.Locator("#ci-sortable .ui-sortable-handle");
            await handles.Nth(2).WaitForAsync();
            var response = scene.Page.WaitForResponseAsync(r => r.Url.Contains("/Admin/Lists/Order"), new() { Timeout = 15000 });
            await scene.DragToAsync(handles.Nth(2), handles.Nth(0), 0, -6);
            var result = await response;
            Console.WriteLine("Order response: " + result.Status);
        });

        await scene.StepAsync("order-5", async () =>
        {
            await GoAsync(scene, "/garden-notes", scene.Page.GetByText("Older Posts").First);
            await scene.Page.GetByText("Older Posts").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(scene.Page.GetByText("Older Posts").First), leadMilliseconds: 150);
            await scene.Page.GetByText("Newer Posts").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200);
        });

        await scene.StepAsync("order-6", async () =>
        {
            await GoAsync(scene, List, Next(scene));
            await NavAsync(scene, Next(scene));
            await scene.Page.Locator("li[title='Go to previous page.'] a").First.WaitForAsync();
            await NavAsync(scene, Next(scene));
            await scene.Page.Locator("li[title='Go to previous page.'] a").First.WaitForAsync();
        });

        await scene.StepAsync("order-7", async () =>
        {
            await GoAsync(scene, List, scene.Page.Locator("input[name='DisplayText']").First);
            await scene.TypeAsync(scene.Page.Locator("input[name='DisplayText']").First, "garden", delay: 50);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter")), leadMilliseconds: 150);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DisplayAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(Settings);
        await Setting(scene, "Show full pager").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("display-1", async () =>
        {
            await Setting(scene, "Show full pager").ScrollIntoViewIfNeededAsync();
            await ShowAsync(scene, Setting(scene, "Show full pager"), 2);
            await scene.ClickAsync(scene.Page.Locator("label:text-is('Show full pager')").First, pauseAfter: 300);
            await SaveAsync(scene);
        });

        await scene.StepAsync("display-2", async () =>
        {
            await GoAsync(scene, List, scene.Page.Locator("nav[aria-label='Listing pages']").First);
            await ShowAsync(scene, scene.Page.Locator("nav[aria-label='Listing pages']").First, 1.5);
            await NavAsync(scene, scene.Page.Locator("nav[aria-label='Listing pages'] a:text-is('3')").First);
        });

        await scene.StepAsync("display-3", async () =>
        {
            await GoAsync(scene, Settings, Setting(scene, "Show container's header"));
            await Setting(scene, "Show container's header").ScrollIntoViewIfNeededAsync();
            await ShowAsync(scene, Setting(scene, "Show container's header"), 1.5);
            await scene.ClickAsync(scene.Page.Locator("label:text-is(\"Show container's header\")").First, pauseAfter: 300);
            await SaveAsync(scene);
        });

        await scene.StepAsync("display-4", async () =>
        {
            await GoAsync(scene, List, scene.Page.Locator("#ci-sortable .btn.edit").First);
            await scene.HighlightAsync(scene.Page.Locator(".card").First);
            await scene.HoldAsync(1);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("#ci-sortable .btn.edit").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

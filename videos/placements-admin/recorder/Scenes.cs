using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Placements in the admin". PrepareAsync disables Placements (the Blog recipe enables it). Record in one take,
/// in order: 02-enable, 03-create, 04-shortcuts. Each clip leaves the site the next one starts from.
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
        ("03-create", CreateAsync),
        ("04-shortcuts", ShortcutsAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/Features", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#search-box").FillAsync("placements");
        var row = page.Locator(".list-group-item:has(code:text-is(\"OrchardCore.Placements\"))").First;
        await row.Locator("a:text-is(\"Disable\")").ClickAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.GetByText("was disabled.").First.WaitForAsync(new() { Timeout = 90000 });
        Console.WriteLine("Disabled Placements.");
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

    private static async Task GoAsync(Scene scene, string path, ILocator ready)
    {
        await scene.OffCameraAsync(async () =>
        {
            await scene.NavigateAsync(path);
            await ready.WaitForAsync();
        }, leadMilliseconds: 100);
    }

    private static async Task TypeInEditorAsync(Scene scene, string text)
    {
        await scene.ClickAsync(scene.Page.Locator(".CodeMirror").First, pauseAfter: 100);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await scene.Page.Keyboard.TypeAsync(text, new() { Delay = 8 });
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        var first = scene.Page.Locator(".list-group-item:has(code:text-is(\"OrchardCore.Placements\"))").First;
        var second = scene.Page.Locator(".list-group-item:has(code:text-is(\"OrchardCore.Placements.FileStorage\"))").First;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#search-box").First, pauseAfter: 200);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "placements", delay: 40);
            await scene.Page.Locator("#list-summary").GetByText("Showing 2 of").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".feature-group:not(.d-none) h3:text-is('Development')").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(first.GetByText("The Placements module provides a way").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(second.GetByText("Stores Placements in a local file.").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, first.Locator("a:text-is(\"Enable\")").First);
            await scene.Page.Locator(".list-group-item:has(code:text-is(\"OrchardCore.Placements\")) a:text-is(\"Disable\")").First
                .WaitForAsync(new() { Timeout = 60000 });
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "placements", delay: 12);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Placements");
        await scene.Page.GetByText("Nothing here!").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await ShowAsync(scene, scene.Page.GetByText("Nothing here!").First, 1.5);
            await NavAsync(scene, scene.Page.Locator("a.create").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ShapeType").First, "MediaField_Edit", delay: 45);
            await scene.HighlightAsync(scene.Page.GetByText("The type of the shape.").First);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, scene.Page.Locator(".CodeMirror").First, 2.5);
            await NavAsync(scene, scene.Page.Locator("button[name='submit'][value='Save']").First);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await TypeInEditorAsync(scene, "[{ \"place\": \"-\", \"differentiator\": \"BlogPost-Image\" }]");
        });

        await scene.StepAsync("create-5", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("button[name='submit'][value='Save']").First);
            await ShowAsync(scene, scene.Page.GetByText("MediaField_Edit").First, 1.2);
            await ShowAsync(scene, scene.Page.Locator("a.btn-primary:text-is('Edit'), a.edit").First, 1.2);
            await ShowAsync(scene, scene.Page.Locator("a.btn-danger").First, 1.2);
            await ShowAsync(scene, scene.Page.Locator("#search-box").First, 1.2);
        });

        await scene.StepAsync("create-6", async () =>
        {
            await GoAsync(scene, "/Admin/Contents/ContentItems/BlogPost", scene.Page.GetByText("Man must explore").First);
            var edit = scene.Page.Locator("a.btn.edit, a.btn-primary:text-is('Edit')").First;
            await NavAsync(scene, edit);
            var subtitle = scene.Page.Locator(".ocat-wrapper:has(label:text-is('Subtitle'))").First;
            await subtitle.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(subtitle);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ShortcutsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.Page.Locator("a[role='btn-edit-BlogPost']").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("shortcut-1", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("a[role='btn-edit-BlogPost']").First);
            var edit = scene.Page.Locator("#fields .list-group-item:has-text('Subtitle') a:text-is('Edit')").First;
            await edit.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(400);
            await NavAsync(scene, edit);
        });

        await scene.StepAsync("shortcut-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:text-is('Edit placements')").First, pauseAfter: 400);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First);
        });

        await scene.StepAsync("shortcut-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.GetByText("Placement in admin editor for the Subtitle field in a BlogPost").First);
            await scene.HighlightAsync(scene.Page.Locator("input[disabled], input[readonly]").First);
            await scene.HoldAsync(0.6);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".CodeMirror").First);
        });

        await scene.StepAsync("shortcut-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TypeInEditorAsync(scene, "[{ \"place\": \"-\", \"differentiator\": \"BlogPost-Subtitle\" }]");
            await NavAsync(scene, scene.Page.Locator("button[name='submit'][value='Save']").First);
        });

        await scene.StepAsync("shortcut-5", async () =>
        {
            await GoAsync(scene, "/Admin/Placements", scene.Page.GetByText("TextField_Edit").First);
            await ShowAsync(scene, scene.Page.GetByText("MediaField_Edit").First, 1.5);
            await ShowAsync(scene, scene.Page.GetByText("TextField_Edit").First, 1.5);
        });

        await scene.ClearHighlightsAsync();
    }
}

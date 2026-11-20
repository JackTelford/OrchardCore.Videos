using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Content Items: Create, Edit and Publish". Each clip starts from the state the previous one left
/// (see recording-plan.md): 03-create creates the Article, 05-edit changes it, 06-remove deletes it.
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
        ("03-create", CreateAsync),
        ("04-view", ViewAsync),
        ("05-edit", EditAsync),
        ("06-remove", RemoveAsync),
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

    private static ILocator Row(Scene scene, string title)
        => scene.Page.Locator("li.list-group-item").Filter(new() { Has = scene.Page.Locator($".contentitem a:text-is('{title}')") });

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            var content = scene.Page.Locator("#left-nav button.oc-navigation-admin-content");

            if (await scene.Page.Locator("#left-nav a.oc-navigation-admin-content-content_items").IsVisibleAsync() is false)
            {
                await scene.ClickAsync(content, pauseAfter: 600);
            }

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.oc-navigation-admin-content-content_items"));
            await scene.Page.Locator("li.list-group-item .contentitem").First.WaitForAsync();
        });

        await scene.StepAsync("list-2", async () =>
        {
            var about = Row(scene, "About");
            await scene.HighlightAsync(about.Locator(".contentitem a"));
            await scene.PauseAsync(1400);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(about.Locator(".contenttype .badge"));
            await scene.PauseAsync(1400);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(about.Locator(".tags .badge"));
            await scene.PauseAsync(1400);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(about.Locator(".metadata .badge").First);
            await scene.PauseAsync(1400);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(about.Locator("a.btn.edit"));
            await scene.HighlightAsync(about.Locator("a.btn.view"));
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[type=search][placeholder=Search], input#search-box, input[name='Options.DisplayText']").First, "Search");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#filter-dropdown"), "Filters");
        });

        await scene.StepAsync("list-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 600);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a.dropdown-item", new() { HasTextString = "Article" }).First);
            await scene.Page.Locator("#TitlePart_Title").WaitForAsync();
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#TitlePart_Title"), "Spring Garden Checklist", 30);
            await scene.ClickAsync(scene.Page.Locator("#AutoroutePart_Path"), pauseAfter: 400);
            await scene.HighlightAsync(scene.Page.Locator("#AutoroutePart_Path"), padding: 8);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator(".trumbowyg-editor").First, "Clear the beds, check the tools, and sort the seed packets.", 2);
            await scene.TypeAsync(scene.Page.Locator("#Article_Subtitle_Text"), "Jobs for the first warm weekend", 2);
            await scene.ScrollAsync(0, 400, 6);
            await scene.HighlightAsync(scene.Page.Locator(".field-wrapper-article-image").First, padding: 8);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button.draft"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button.publish"));
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("[data-action-group='oc-content-publish-action'] .dropdown-toggle-split"), pauseAfter: 2500);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator("[data-action-group='oc-content-save-action'] .dropdown-toggle-split"), pauseAfter: 400);
        });

        await scene.StepAsync("create-6", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish"));
            await scene.HighlightAsync(Row(scene, "Spring Garden Checklist").Locator(".tags .badge"));
        });
    }

    private static async Task ViewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("view-1", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Spring Garden Checklist").Locator("a.btn.view"));
        });

        await scene.StepAsync("view-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var popup = await scene.Page.RunAndWaitForPopupAsync(async () =>
                await scene.ClickAsync(Row(scene, "Spring Garden Checklist").Locator("a.btn.view"), pauseAfter: 300));
            await popup.WaitForLoadStateAsync();
            await scene.SwitchToAsync(popup);
            await popup.Locator("h1", new() { HasTextString = "Spring Garden Checklist" }).First.WaitForAsync();
            await scene.HighlightAsync(popup.Locator(".page-heading").First, "/spring-garden-checklist");
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("edit-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Spring Garden Checklist").Locator("a.btn.edit"));
            var body = scene.Page.Locator(".trumbowyg-editor").First;
            await body.FocusAsync();
            await scene.Page.Keyboard.PressAsync("Control+End");
            await body.PressSequentiallyAsync(" Order compost early.", new() { Delay = 4 });
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish"));
            await scene.HighlightAsync(Row(scene, "Spring Garden Checklist").Locator(".tags .badge"));
        });

        await scene.StepAsync("edit-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var popup = await scene.Page.RunAndWaitForPopupAsync(async () =>
                await scene.ClickAsync(Row(scene, "Spring Garden Checklist").Locator("a.btn.view"), pauseAfter: 300));
            await popup.WaitForLoadStateAsync();
            await scene.SwitchToAsync(popup);
            await popup.Locator("h1", new() { HasTextString = "Spring Garden Checklist" }).First.WaitForAsync();
        });
    }

    private static async Task RemoveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("remove-1", async () =>
        {
            var row = Row(scene, "Spring Garden Checklist");
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 2500);
            await scene.ClickAsync(row.Locator(".dropdown-item", new() { HasTextString = "Unpublish" }), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show .btn-primary, .modal.show button:text-is('Yes')").First);
            await Row(scene, "Spring Garden Checklist").Locator(".tags .badge", new() { HasTextString = "Draft" }).WaitForAsync();
        });

        await scene.StepAsync("remove-2", async () =>
        {
            var row = Row(scene, "Spring Garden Checklist");
            await scene.ClickAsync(row.Locator("button.actions"), pauseAfter: 500);
            await scene.HighlightAsync(row.Locator(".dropdown-item", new() { HasTextString = "Publish Draft" }));
        });

        await scene.StepAsync("remove-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => Row(scene, "Spring Garden Checklist").Locator("a.btn.edit").ClickAsync());
            await scene.ClickAsync(scene.Page.Locator(".edit-item-actions .btn-danger, .edit-item-actions a.btn:text-is('Delete'), .edit-item-actions button:text-is('Delete')").First, pauseAfter: 100);
        });

        await scene.StepAsync("remove-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show .btn-primary, .modal.show button:text-is('Ok')").First);
            await scene.PauseAsync(2500);
            await scene.ClickAsync(Row(scene, "About").Locator("button.actions"), pauseAfter: 300);
        });
    }
}

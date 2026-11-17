using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Write a Content Part With a Display Driver": on a stock Blank-recipe site (The Default Theme made
/// current off camera), the Title part attached to a new Workshop type, then used in the editor.
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
        ("06-attach", AttachAsync),
        ("07-use", UseAsync),
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

    // Enables The Default Theme and makes it current, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.GotoAsync($"{BaseUrl}/Admin/Themes", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var card = page.Locator(".card, .list-group-item").Filter(new() { Has = page.GetByText("The Default Theme", new() { Exact = true }) }).First;

        if (await card.GetByText("Enable", new() { Exact = true }).CountAsync() > 0)
        {
            await card.GetByText("Enable", new() { Exact = true }).First.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(4000);
            card = page.Locator(".card, .list-group-item").Filter(new() { Has = page.GetByText("The Default Theme", new() { Exact = true }) }).First;
        }

        await card.GetByText("Make Current", new() { Exact = true }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(4000);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 8 });
    }

    private static async Task AttachAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("attach-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".oc-breadcrumb-title, h1").First, "Design > Content Definition > Content Types");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btnCreate"));
            await FastAsync(scene.Page.Locator("#DisplayName"), "Workshop");
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("button.create").First.ClickAsync());
        });

        await scene.StepAsync("attach-2", async () =>
        {
            var row = scene.Page.Locator("li.list-group-item:has(label:text-is('Title'))").First;
            await scene.HighlightAsync(row, "Title");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(row.Locator("button[name='submit.AddAndConfigure']"));
        });

        await scene.StepAsync("attach-3", async () =>
        {
            var select = scene.Page.Locator(".ocat-limited-wrapper:has(label:text-is('Behavior option')) select").First;
            await scene.SelectAsync(select, "Title is editable and required");
            await scene.HighlightAsync(select);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save[name='submit.Save']").First);
        });
    }

    private static async Task UseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        IPage list = scene.Page;

        await scene.StepAsync("use-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a.btn:text-is('New Workshop')").First);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn:text-is('New Workshop')").First);
            await scene.Page.Locator("#TitlePart_Title").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#TitlePart_Title"), "Title");
        });

        await scene.StepAsync("use-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish[name='submit.Publish']"));
            var error = scene.Page.Locator("span.field-validation-error", new() { HasTextString = "A value is required for Title." }).First;
            await error.WaitForAsync();
            await scene.HighlightAsync(error);
        });

        await scene.StepAsync("use-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastAsync(scene.Page.Locator("#TitlePart_Title"), "Pruning Apple Trees");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish[name='submit.Publish']"));
        });

        await scene.StepAsync("use-4", async () =>
        {
            var popup = await scene.Page.RunAndWaitForPopupAsync(async () =>
                await scene.ClickAsync(scene.Page.Locator("a.btn.view").First, pauseAfter: 300));
            await popup.WaitForLoadStateAsync();
            await scene.SwitchToAsync(popup);
            var heading = popup.Locator("article.content-item h1, h1").First;
            await heading.WaitForAsync();
            await scene.HighlightAsync(heading);
        });

        await scene.StepAsync("use-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SwitchToAsync(list);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn.edit").First);
            await scene.Page.Locator("#TitlePart_Title").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#TitlePart_Title"));
        });
    }
}

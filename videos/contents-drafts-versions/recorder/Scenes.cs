using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Drafts and Versions". PrepareAsync seeds Parking Information (published, with a draft) and Spring
/// Event (draft only) outside the capture; each clip starts from the state the previous one left.
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
        ("02-draft", DraftAsync),
        ("03-publish", PublishAsync),
        ("04-unpublish", UnpublishAsync),
        ("05-find", FindAsync),
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

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);

        await page.GotoAsync($"{BaseUrl}/Admin/Contents/ContentTypes/Article/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#TitlePart_Title").FillAsync("Parking Information");
        await page.Locator("#Article_Subtitle_Text").FillAsync("Free parking behind the shop");
        await page.Locator("button.publish").ClickAsync();
        await page.WaitForURLAsync(url => !url.Contains("/Create"));
        await Task.Delay(1500);

        await page.GotoAsync($"{BaseUrl}/Admin/Contents/ContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Row(page, "Parking Information").Locator("a.btn.edit").ClickAsync();
        await page.Locator("#Article_Subtitle_Text").FillAsync("Free parking behind the shop on Saturdays");
        await page.Locator("button.draft").ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("/Admin/Contents/ContentItems"));
        await Task.Delay(1500);

        await page.GotoAsync($"{BaseUrl}/Admin/Contents/ContentTypes/Article/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#TitlePart_Title").FillAsync("Spring Event");
        await page.Locator("#Article_Subtitle_Text").FillAsync("Join us for the spring harvest");
        await page.Locator("button.draft").ClickAsync();
        await page.WaitForURLAsync(url => !url.Contains("/Create"));
        await Task.Delay(1500);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static ILocator Row(IPage page, string title)
        => page.Locator("li.list-group-item").Filter(new() { Has = page.Locator($".contentitem a:text-is('{title}')") });

    private static ILocator Row(Scene scene, string title) => Row(scene.Page, title);

    private static ILocator Menu(Scene scene, string title, string entry)
        => Row(scene, title).Locator(".actions-menu a.dropdown-item", new() { HasTextString = entry });

    private static async Task SetSubtitleAsync(Scene scene, string text)
    {
        var subtitle = scene.Page.Locator("#Article_Subtitle_Text");
        await subtitle.ScrollIntoViewIfNeededAsync();
        await scene.ClickAsync(subtitle, pauseAfter: 100);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await scene.Page.Keyboard.PressAsync("Delete");
        await subtitle.PressSequentiallyAsync(text, new() { Delay = 3 });
        await scene.Page.Keyboard.PressAsync("Tab");
    }

    private static async Task<IPage> OpenPopupAsync(Scene scene, ILocator link)
    {
        var popup = await scene.Page.RunAndWaitForPopupAsync(async () => await scene.ClickAsync(link, pauseAfter: 300));
        await popup.WaitForLoadStateAsync();
        await scene.SwitchToAsync(popup);
        return popup;
    }

    private static async Task DraftAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await Row(scene, "About").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("draft-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#left-nav a.oc-navigation-admin-content-content_items"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "About").Locator(".tags .ta-badge:has(.fa-check)"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "About").Locator("a.btn.edit"));
        });

        await scene.StepAsync("draft-2", async () =>
        {
            await SetSubtitleAsync(scene, "We grow apples, pears and plums.");
            await scene.HighlightAsync(scene.Page.Locator("button.publish"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button.draft"));
        });

        await scene.StepAsync("draft-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.draft[name='submit.Save']"));
            await scene.HighlightAsync(Row(scene, "About").Locator(".tags .ta-badge:has(.fa-check), .tags .ta-badge:has(.fa-pencil)").First);
            await scene.HighlightAsync(Row(scene, "About").Locator(".tags .ta-badge:has(.fa-pencil)"));
        });

        await scene.StepAsync("draft-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var popup = await OpenPopupAsync(scene, Row(scene, "About").Locator("a.btn.view"));
            await popup.Locator("body").WaitForAsync();
        });

        await scene.StepAsync("draft-5", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems");
            await Row(scene, "About").WaitForAsync();
            var popup = await OpenPopupAsync(scene, Row(scene, "About").Locator("a.btn.preview"));
            await popup.Locator("body").WaitForAsync();
        });
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await Row(scene, "About").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("publish-1", async () =>
        {
            await scene.ClickAsync(Row(scene, "About").Locator("button.actions"), pauseAfter: 400);
            await scene.HighlightAsync(Menu(scene, "About", "Publish Draft"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "About", "Publish Draft"));
        });

        await scene.StepAsync("publish-2", async () =>
        {
            await OpenPopupAsync(scene, Row(scene, "About").Locator("a.btn.view"));
        });

        await scene.StepAsync("publish-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems");
            await scene.WaitForNavigationAsync(() => Row(scene, "About").Locator("a.btn.edit").ClickAsync());
            await SetSubtitleAsync(scene, "Closed for the winter");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.draft[name='submit.Save']"));
        });

        await scene.StepAsync("publish-4", async () =>
        {
            await scene.ClickAsync(Row(scene, "About").Locator("button.actions"), pauseAfter: 400);
            await scene.ClickAsync(Menu(scene, "About", "Discard Draft"), pauseAfter: 300);
            await scene.Page.Locator("#confirmRemoveModal.show").WaitForAsync();
            await scene.PauseAsync(2000);
            await scene.WaitForNavigationAsync(async () => await scene.ClickAsync(scene.Page.Locator("#modalOkButton"), pauseAfter: 100));
        });
    }

    private static async Task UnpublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await Row(scene, "About").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("unpublish-1", async () =>
        {
            await scene.ClickAsync(Row(scene, "About").Locator("button.actions"), pauseAfter: 400);
            await scene.ClickAsync(Menu(scene, "About", "Unpublish"), pauseAfter: 300);
            await scene.Page.Locator("#confirmRemoveModal.show").WaitForAsync();
            await scene.PauseAsync(2000);
            await scene.WaitForNavigationAsync(async () => await scene.ClickAsync(scene.Page.Locator("#modalOkButton"), pauseAfter: 100));
        });

        await scene.StepAsync("unpublish-2", async () =>
        {
            await scene.HighlightAsync(Row(scene, "About").Locator(".tags .ta-badge:has(.fa-pencil)"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "About").Locator(".admin-list-actions"));
        });

        await scene.StepAsync("unpublish-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Row(scene, "About").Locator("button.actions"), pauseAfter: 400);
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "About", "Publish Draft"));
        });
    }

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await Row(scene, "Parking Information").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            var parking = Row(scene, "Parking Information");
            await scene.HighlightAsync(parking.Locator(".tags .ta-badge:has(.fa-check)"));
            await scene.HighlightAsync(parking.Locator(".tags .ta-badge:has(.fa-pencil)"));
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Spring Event").Locator(".tags .ta-badge:has(.fa-pencil)"));
        });

        await scene.StepAsync("find-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#filter-dropdown"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#filter-dropdown"), pauseAfter: 2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu[aria-labelledby='filter-dropdown'] a:text-is('Only draft items')"));
        });
    }
}

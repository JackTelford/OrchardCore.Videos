using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Autoroute", recorded in order on one fresh Blog site that has an Event type without the Autoroute part.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-add", AddAsync),
        ("03-settings", SettingsAsync),
        ("04-create", CreateAsync),
        ("05-change", ChangeAsync),
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

    /// <summary>Creates the Event content type (Title part only), outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/List");
        await page.Locator("#btnCreate").ClickAsync();
        await page.Locator("#DisplayName").PressSequentiallyAsync("Event", new() { Delay = 30 });
        await page.WaitForFunctionAsync("() => document.querySelector('#Name').value === 'Event'");
        await page.Locator("button.create").First.ClickAsync();
        var row = page.Locator("li.list-group-item").Filter(new() { Has = page.GetByText("Title", new() { Exact = true }) }).First;
        await row.Locator("input[type='checkbox']").First.ClickAsync();
        await page.Locator("#add-selected-parts-button").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        foreach (var path in new[] { "/Admin/Features", "/Admin/ContentTypes/List", "/Admin/Contents/ContentItems" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Event type created.");
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task WaitTextAsync(IPage page, string text, int timeout = 60_000)
        => await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = timeout });

    // Clicks a link or button that loads a new page; the load is cut out of the video (see Scene.OffCameraAsync).
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
    }

    // Opens a page during the capture, with the load cut out of the video.
    private static Task VisitAsync(Scene scene, string path)
        => scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(BaseUrl + path)), leadMilliseconds: 250);

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    // Replaces the text of a field: click, select all, delete, type (fast when the machine is busy).
    private static async Task ReplaceAsync(Scene scene, IPage page, ILocator field, string text, int delay = 25)
    {
        await scene.ClickAsync(field, pauseAfter: 100);
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.PressAsync("Delete");
        await scene.TypeAsync(field, text, delay: delay);
    }

    private static ILocator PermalinkGroup(IPage page) => page.Locator(".input-group").Filter(new() { Has = page.GetByLabel("Permalink") }).First;

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        var featureRow = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-disable-OrchardCore_Autoroute, #btn-enable-OrchardCore_Autoroute") }).First;

        await scene.StepAsync("add-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "autoroute", delay: 40);
            await featureRow.WaitForAsync();
            await scene.MoveToAsync(featureRow, -200);
            await scene.HighlightAsync(featureRow, "Enabled");
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await VisitAsync(scene, "/Admin/ContentTypes/List");
            await GoAsync(scene, page.Locator("a[role='btn-edit-Event']"));
            await page.Locator("h3").Filter(new() { HasTextString = "Parts" }).First.WaitForAsync();
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("#parts"), "Parts");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Add Parts", Exact = true }));
        });

        await scene.StepAsync("add-4", async () =>
        {
            var row = page.Locator("li.list-group-item").Filter(new() { HasTextString = "Provides a custom url for your content item." });
            await row.First.ScrollToMiddleAsync();
            await ShowAsync(scene, row.First, 3500);
            await GoAsync(scene, row.Locator("button[name='submit.AddAndConfigure']").First);
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/Event/ContentParts/AutoroutePart/Edit");
        await page.Locator("label.ocat-label").Filter(new() { HasTextString = "Pattern" }).First.WaitForAsync();
        await page.Locator(".CodeMirror").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await scene.MoveToAsync(page.Locator("label.ocat-label").Filter(new() { HasTextString = "Pattern" }).First);
        });

        foreach (var (step, label) in new[] { ("settings-2", "Allow custom path"), ("settings-3", "Allow path updates"), ("settings-4", "Show homepage options"), ("settings-5", "Allow disabled") })
        {
            await scene.StepAsync(step, async () =>
            {
                await scene.ClickAsync(page.GetByLabel(label), pauseAfter: 300);
            });
        }

        await scene.StepAsync("settings-6", async () =>
        {
            var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Container Settings" });
            await heading.ScrollToMiddleAsync();
            await ShowAsync(scene, heading, 4500);
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
            await page.Locator("#parts").WaitForAsync();
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await page.Locator("#new-dropdown").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Event" }).First);
            await page.GetByLabel("Title", new() { Exact = true }).First.WaitForAsync();
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Spring Planting Workshop", delay: 30);
            await scene.HighlightAsync(PermalinkGroup(page));
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }).First);
            await page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "Spring Planting Workshop" }).First.WaitForAsync();
        });

        await scene.StepAsync("create-4", async () =>
        {
            var row = page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "Spring Planting Workshop" }).First;
            await GoAsync(scene, row.Locator("a.edit").First);
            await page.WaitForFunctionAsync("() => document.querySelector('#AutoroutePart_Path, input[name$=\"AutoroutePart.Path\"]')?.value === 'spring-planting-workshop'");
            await scene.HighlightAsync(PermalinkGroup(page));
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var link = page.Locator("a.text-info:has(i.fa-external-link)").First;
            var popup = await scene.Context.RunAndWaitForPageAsync(async () => await scene.ClickAsync(link, pauseAfter: 200));
            await scene.SwitchToAsync(popup);
            await popup.Locator("h1").Filter(new() { HasTextString = "Spring Planting Workshop" }).First.WaitForAsync();
            await scene.HighlightAsync(popup.Locator("h1").First, "/spring-planting-workshop");
        });
    }

    private static async Task ChangeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "Spring Planting Workshop" }).First.WaitForAsync();
        await scene.StartCaptureAsync();

        ILocator Day() => page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "Spring Planting Day" }).First;
        ILocator PublishButton() => page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }).First;
        ILocator PathField() => page.GetByLabel("Permalink");

        async Task WaitPathAsync(string path)
            => await page.WaitForFunctionAsync("p => document.querySelector('input[name$=\"AutoroutePart.Path\"]')?.value === p", path);

        await scene.StepAsync("change-1", async () =>
        {
            var row = page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "Spring Planting Workshop" }).First;
            await GoAsync(scene, row.Locator("a.edit").First);
            await ReplaceAsync(scene, page, page.GetByLabel("Title", new() { Exact = true }).First, "Spring Planting Day");
        });

        await scene.StepAsync("change-2", async () =>
        {
            await GoAsync(scene, PublishButton());
            await WaitTextAsync(page, "Your Event has been published.");
            await GoAsync(scene, Day().Locator("a.edit").First);
            await WaitPathAsync("spring-planting-workshop");
            await scene.HighlightAsync(PermalinkGroup(page));
        });

        await scene.StepAsync("change-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.GetByLabel("Refresh the path"), pauseAfter: 300);
            await scene.PauseAsync(3000);
            await GoAsync(scene, PublishButton());
            await WaitTextAsync(page, "Your Event has been published.");
        });

        await scene.StepAsync("change-4", async () =>
        {
            await GoAsync(scene, Day().Locator("a.edit").First);
            await WaitPathAsync("spring-planting-day");
            await scene.HighlightAsync(PermalinkGroup(page));
        });

        await scene.StepAsync("change-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ReplaceAsync(scene, page, PathField(), "spring planting");
            await GoAsync(scene, PublishButton());
            var message = page.Locator(".field-validation-error").Filter(new() { HasTextString = "Please do not use any of the following characters" }).First;
            await message.WaitForAsync();
            await message.ScrollToMiddleAsync();
            await scene.HighlightAsync(message);
        });

        await scene.StepAsync("change-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ReplaceAsync(scene, page, PathField(), "spring-planting");
            await GoAsync(scene, PublishButton());
            await WaitTextAsync(page, "Your Event has been published.");
            await GoAsync(scene, Day().Locator("a.edit").First);
            await WaitPathAsync("spring-planting");
            await scene.HighlightAsync(PermalinkGroup(page));
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the editors.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

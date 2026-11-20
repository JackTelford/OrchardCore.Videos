using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Content Preview". PrepareAsync creates two articles off camera (the second with a saved draft)
/// and disables the Content Preview feature that the Blog recipe enables, so 02-enable can enable it on camera.
/// 02-enable changes the site: 03-live and 04-draft follow it.
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
        ("03-live", LiveAsync),
        ("04-draft", DraftAsync),
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
        page.SetDefaultTimeout(60_000);

        async Task GoAsync(string path) => await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.NetworkIdle });

        async Task CreateAsync(string title, string subtitle, string body)
        {
            await GoAsync("/Admin/Contents/ContentTypes/Article/Create");
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator("#Article_Subtitle_Text").FillAsync(subtitle);
            await page.Locator(".trumbowyg-editor").First.PressSequentiallyAsync(body, new() { Delay = 1 });
            await page.Locator("button.publish").ClickAsync();
            await page.WaitForURLAsync(url => !url.Contains("/Create"));
            await Task.Delay(1500);
        }

        await CreateAsync("Autumn Open Day", "Tours, seeds and warm cider", "Join us on Saturday at the greenhouse for guided tours, a seed swap and warm cider. Doors open at ten in the morning, and entry is free.");
        await CreateAsync("Winter Workshop Series", "Pruning and planting in the greenhouse", "Each workshop runs for two hours in the heated greenhouse. Bring gloves and a notebook.");

        await GoAsync("/Admin/Contents/ContentItems");
        await page.Locator("li.list-group-item").Filter(new() { HasText = "Winter Workshop Series" }).Locator("a.edit").First.ClickAsync();
        await page.Locator("#Article_Subtitle_Text").FillAsync("Pruning, planting and evening sessions");
        await page.Locator("button.draft").First.ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("/Admin/Contents/ContentItems"));
        await Task.Delay(1500);

        await GoAsync("/Admin/Features");
        await page.Locator("#search-box").FillAsync("Content Preview");
        await page.Locator("#btn-disable-OrchardCore_ContentPreview").ClickAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(5000);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 4 });
    }

    private static ILocator Row(IPage page, string title) => page.Locator("li.list-group-item").Filter(new() { HasText = title });

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();
        var row = scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-enable-OrchardCore_ContentPreview") }).First;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Content Preview", 25);
            await row.WaitForAsync();
            await scene.HighlightAsync(row, "Content Preview");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = scene.Page.Locator("#btn-enable-OrchardCore_ContentPreview");
            await scene.MoveToAsync(enable);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await enable.ClickAsync();
                await scene.Page.GetByText("The feature Content Preview was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            }));
            await FastAsync(scene.Page.Locator("#search-box"), "Content Preview");
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-disable-OrchardCore_ContentPreview") }).First);
        });
    }

    private static async Task WaitTopFrameAsync(IPage preview, string title, string subtitle)
    {
        await preview.WaitForFunctionAsync("([t, s]) => { const frames = [...document.querySelectorAll('#iframe-a, #iframe-b')].filter(f => f.contentDocument); const top = frames.sort((a, b) => (+b.style.zIndex || 0) - (+a.style.zIndex || 0))[0]; const d = top && top.contentDocument; return !!d && d.querySelector('h1') && d.querySelector('h1').textContent.trim() === t && (!s || (d.querySelector('.subheading') && d.querySelector('.subheading').textContent.trim() === s)); }", new[] { title, subtitle }, new() { Timeout = 30000 });
    }

    private static async Task LiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var list = scene.Page;
        IPage preview = null;

        await scene.StepAsync("live-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(list, "Autumn Open Day").Locator("a.edit").First);
            await list.Locator("#previewButton").WaitForAsync();
            await scene.HighlightAsync(list.Locator("#previewButton"), "Preview");
        });

        await scene.StepAsync("live-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            preview = await scene.Context.RunAndWaitForPageAsync(async () => await scene.ClickAsync(list.Locator("#previewButton"), pauseAfter: 300));
            await scene.SwitchToAsync(preview);
            await scene.OffCameraAsync(() => WaitTopFrameAsync(preview, "Autumn Open Day", "Tours, seeds and warm cider"));
        });

        await scene.StepAsync("live-3", async () =>
        {
            await scene.SwitchToAsync(list);
            var titleField = list.GetByLabel("Title", new() { Exact = true });
            await titleField.FocusAsync();
            await list.Keyboard.PressAsync("Control+A");
            await titleField.PressSequentiallyAsync("Autumn Open Day: Free Entry", new() { Delay = 4 });
            var subtitle = list.GetByLabel("Subtitle", new() { Exact = true });
            await subtitle.ScrollIntoViewIfNeededAsync();
            await subtitle.FocusAsync();
            await list.Keyboard.PressAsync("Control+A");
            await subtitle.PressSequentiallyAsync("Tours, seed swaps and warm cider", new() { Delay = 4 });
            await list.Locator("#TitlePart_Title").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("live-4", async () =>
        {
            await scene.SwitchToAsync(preview);
            try
            {
                await scene.OffCameraAsync(() => WaitTopFrameAsync(preview, "Autumn Open Day: Free Entry", "Tours, seed swaps and warm cider"));
            }
            catch (TimeoutException)
            {
                Console.WriteLine("  preview shows: " + await preview.EvaluateAsync<string>("() => [...document.querySelectorAll('iframe')].map(f => f.id + ':' + f.style.zIndex + ':' + (f.contentDocument?.querySelector('h1')?.textContent ?? '-') + '|' + (f.contentDocument?.querySelector('.subheading')?.textContent ?? '-')).join(' ; ')"));
                throw;
            }
        });

        await scene.StepAsync("live-5", async () =>
        {
            await scene.SwitchToAsync(list);
            await scene.WaitForNavigationAsync(() => list.Locator("a.cancel").First.ClickAsync());
            await scene.HighlightAsync(Row(list, "Autumn Open Day").First);
        });

        await scene.StepAsync("live-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SwitchToAsync(preview);
            await scene.OffCameraAsync(async () => await preview.Locator("#notConnectedWarning").WaitForAsync(new() { Timeout = 30000 }));
        });
    }

    private static async Task DraftAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var list = scene.Page;
        var winter = Row(list, "Winter Workshop Series").First;
        var autumn = Row(list, "Autumn Open Day").First;

        await scene.StepAsync("draft-1", async () =>
        {
            await scene.HighlightAsync(winter.GetByText("Draft", new() { Exact = true }).First);
            await scene.HighlightAsync(winter.Locator("a.preview").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(autumn.Locator("a.edit").First);
            await scene.HighlightAsync(autumn.Locator("a.view").First);
        });

        await scene.StepAsync("draft-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var tab = await scene.Context.RunAndWaitForPageAsync(async () => await scene.ClickAsync(winter.Locator("a.preview").First, pauseAfter: 300));
            await tab.WaitForLoadStateAsync();
            await scene.SwitchToAsync(tab);
            await tab.Locator("h1").First.WaitForAsync();
        });

        await scene.StepAsync("draft-3", async () =>
        {
            await scene.SwitchToAsync(list);
            var tab = await scene.Context.RunAndWaitForPageAsync(async () => await scene.ClickAsync(winter.Locator("a.view").First, pauseAfter: 300));
            await tab.WaitForLoadStateAsync();
            await scene.SwitchToAsync(tab);
            await tab.Locator("h1").First.WaitForAsync();
        });
    }
}

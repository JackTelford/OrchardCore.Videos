using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "File Upload Security", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-enable", EnableAsync),
        ("03-clean", CleanAsync),
        ("03-test", TestAsync),
        ("03-down", DownAsync),
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

    /// <summary>Nothing to seed. Warms the pages up once, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/Features", "/Admin/Media" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Pages warmed up.");
    }

    private static async Task WaitSiteAsync()
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        for (var i = 0; i < 60; i++)
        {
            try
            {
                var response = await client.GetAsync(BaseUrl + "/");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(2000);
        }
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<Scene, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static async Task CollapseMenuAsync(IPage page)
    {
        await page.Locator("#left-nav").WaitForAsync();
        var compactor = page.Locator("button.leftbar-compactor");
        if (await compactor.CountAsync() > 0)
        {
            await compactor.First.ClickAsync();
            await Task.Delay(800);
        }
    }

    // The test files are built in memory, outside git: the EICAR test text is never written to a file or to the repository.
    private static FilePayload TestFile(string name)
    {
        var text = name switch
        {
            "office-supplies.csv" => "Item,Quantity\nNotebook,12\nPencil,40\nStapler,3\n",
            "price-list.csv" => "Item,Price\nNotebook,4.50\nPencil,0.60\nStapler,9.00\n",
            _ => string.Join(string.Empty, new[] { "X5O!P%@AP[4", "\\PZX54(P^)7CC)7}", "$EICAR-STANDARD-", "ANTIVIRUS-TEST-FILE!", "$H+H*" }),
        };

        return new FilePayload { Name = name, MimeType = "text/csv", Buffer = System.Text.Encoding.ASCII.GetBytes(text) };
    }

    private static async Task ChooseFileAsync(Scene scene, string name)
    {
        var label = scene.Page.Locator("label.upload-button").First;
        var chooser = await scene.Page.RunAndWaitForFileChooserAsync(() => scene.ClickAsync(label, pauseAfter: 200));
        await chooser.SetFilesAsync(TestFile(name));
    }

    private static async Task PrepareUploadAsync(Scene scene, int step, string name, double holdSeconds)
    {
        var label = scene.Page.Locator("label.upload-button").First;
        await scene.MoveToAsync(label);
        await scene.HighlightAsync(label);
        await scene.HoldAsync(holdSeconds);
        await scene.ClearHighlightsAsync();
        await ChooseFileAsync(scene, name);
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

    private static ILocator Row(IPage page, string text) => page.Locator(".list-group-item, tr").Filter(new() { HasTextString = text }).First;

    private static async Task OpenAdminGroupAsync(IPage page, string name)
    {
        var group = page.Locator("#left-nav").GetByText(name, new() { Exact = true }).First;
        await group.ClickAsync();
        await Task.Delay(500);
    }

    // The message line of the Uploads panel is cut to one row until it is clicked: click its arrow, at the left of the text.
    private static async Task ExpandAsync(Scene scene, ILocator message)
    {
        await scene.ClickAsync(message, pauseAfter: 500);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Features", s => CollapseMenuAsync(s.Page));
        var page = scene.Page;
        var row = page.Locator("[data-filter-value]").Filter(new() { HasTextString = "OrchardCore.Antivirus" }).First;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "ClamAV", delay: 40);
            await row.WaitForAsync();
            await scene.MoveToAsync(row.Locator("a[id^='btn-enable-']").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await GoAsync(scene, row.Locator("a[id^='btn-enable-']").First);
            await WaitSiteAsync();
            await page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "ClamAV", delay: 20);
            await row.Locator("a[id^='btn-disable-']").First.WaitForAsync();
            await scene.HighlightAsync(row.Locator("a[id^='btn-disable-']").First);
        });
    }

    private static async Task CleanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Media", s => CollapseMenuAsync(s.Page));
        var page = scene.Page;

        await scene.StepAsync("upload-1", async () =>
        {
            await PrepareUploadAsync(scene, 1, "office-supplies.csv", 3.0);
        });

        await scene.StepAsync("upload-2", async () =>
        {
            await page.GetByText("office-supplies.csv").First.WaitForAsync();
            await scene.HoldAsync(1.0);
        });
    }

    private static async Task TestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Media", s => CollapseMenuAsync(s.Page));
        var page = scene.Page;

        await scene.StepAsync("upload-3", async () =>
        {
            await PrepareUploadAsync(scene, 3, "eicar-test.csv", 3.0);
        });

        await scene.StepAsync("upload-4", async () =>
        {
            await page.GetByText("Errors: 1").First.WaitForAsync();
            var message = page.GetByText("was rejected because ClamAV detected").Last;
            await message.WaitForAsync();
            await ExpandAsync(scene, message);
            await scene.HoldAsync(1.0);
        });

        await scene.StepAsync("upload-5", async () =>
        {
            await scene.MoveToAsync(600, 400);
            await scene.HoldAsync(Math.Max(0.5, durations["upload-5"] - 4.0));
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Clear Errors" }).First, pauseAfter: 400);
            await scene.HoldAsync(1.0);
        });
    }

    private static async Task DownAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Media", s => CollapseMenuAsync(s.Page));
        var page = scene.Page;

        await scene.StepAsync("down-1", async () =>
        {
            await PrepareUploadAsync(scene, 1, "price-list.csv", 3.0);
        });

        await scene.StepAsync("down-2", async () =>
        {
            await scene.OffCameraAsync(async () => await page.GetByText("Errors: 1").First.WaitForAsync(), 100);
            var message = page.GetByText("ClamAV antivirus scanner").Last;
            await message.WaitForAsync();
            await ExpandAsync(scene, message);
            await scene.HoldAsync(1.0);
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

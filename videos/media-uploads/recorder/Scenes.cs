using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The fake files (pictures and two 25 MB files of random bytes), made outside the repository.
    private static string Files => Path.Combine(Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(), "media-uploads-files");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-create", CreateAsync),
        ("04-resume", ResumeAsync),
        ("05-live", LiveAsync),
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

    // Seeds the folder Team Photos and its picture, outside the capture, while the features are still disabled.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120000);
        page.SetDefaultNavigationTimeout(120000);
        await page.GotoAsync(BaseUrl + "/Admin/Media", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(1500);

        await page.Locator("#folder-tree .treeroot .folder-actions a.action-button").First.ClickAsync();
        await page.GetByText("Create a subfolder").First.ClickAsync();
        await page.Locator("input[name=create-folder]").FillAsync("Team Photos");
        await page.Locator("#btn-submit").ClickAsync();
        await page.Locator("#folder-tree li.folder:has-text(\"Team Photos\")").First.WaitForAsync();
        await Task.Delay(1000);
        await page.Locator("#folder-tree li.folder:has-text(\"Team Photos\") a.folder-menu-item").First.ClickAsync();
        await Task.Delay(1500);
        await page.Locator("#fileupload").SetInputFilesAsync(Path.Combine(Files, "Summer Party 2019.jpg"));
        await page.Locator("div.table-row.file-item:has-text(\"Summer Party 2019.jpg\")").WaitForAsync();
        Console.WriteLine("Seeded Team Photos with Summer Party 2019.jpg");
        await Task.Delay(1500);
        await context.CloseAsync();
    }

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.SetDefaultTimeout(120000);
        scene.Page.SetDefaultNavigationTimeout(120000);

        return scene;
    }

    // Opens the page with the browser's storage cleared, so every take starts from the defaults.
    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(800);
    }

    private static ILocator FolderRow(IPage page, string name) => page.Locator($"#folder-tree li.folder:has(a.folder-menu-item:has-text(\"{name}\"))").Last;

    private static ILocator FileRow(IPage page, string name) => page.Locator($"div.table-row.file-item:has-text(\"{name}\")").First;

    // Selects a folder in the tree and waits until the list shows a given file (or the empty message).
    private static async Task SelectFolderAsync(IPage page, string name)
    {
        await page.Locator($"#folder-tree li.folder a.folder-menu-item:has-text(\"{name}\")").First.ClickAsync();
        await Task.Delay(1500);
    }

    // Clicks "Upload files" with the visible cursor and chooses a file in the dialog.
    private static async Task ChooseFileAsync(Scene scene, string name)
    {
        var page = scene.Page;
        var button = page.Locator("label[for=fileupload]");
        var chooser = await page.RunAndWaitForFileChooserAsync(async () =>
        {
            var (bx, by) = await scene.CenterOfAsync(button);
            await scene.MoveToAsync(bx, by, 14);
            await Task.Delay(150);
            await page.Mouse.DownAsync();
            await page.Mouse.UpAsync();
        });
        await chooser.SetFilesAsync(Path.Combine(Files, name));
    }

    private static ILocator FeatureRow(IPage page, string name) => page.Locator($"[data-filter-value^=\"{name} \"]").First;

    private static async Task SearchAsync(Scene scene, string text, bool visibleClick = false)
    {
        var box = scene.Page.Locator("#search-box");
        await box.WaitForAsync();
        await scene.PauseAsync(visibleClick ? 600 : 250);

        if (visibleClick)
        {
            await scene.TypeAsync(box, text, 10);
        }
        else
        {
            await box.FocusAsync();
            await box.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = 8 });
        }

        await scene.PauseAsync(300);

        if (await box.InputValueAsync() != text)
        {
            await box.FillAsync(text);
            await scene.PauseAsync(300);
        }
    }

    // Clicks an Enable button, with the restart of the tenant cut out of the capture.
    private static async Task EnableFeatureAsync(Scene scene, string featureId)
    {
        var page = scene.Page;
        var button = page.Locator($"#btn-enable-{featureId}");
        var (x, y) = await scene.CenterOfAsync(button);
        await scene.MoveToAsync(x, y, 14);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 300);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await SearchAsync(scene, "media", true);
            await FeatureRow(page, "Media SignalR").WaitForAsync();
            await scene.HighlightAsync(FeatureRow(page, "Media SignalR"));
            await scene.HighlightAsync(FeatureRow(page, "Media Slugify"));
            await scene.HighlightAsync(FeatureRow(page, "Media TUS Uploads"));
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await EnableFeatureAsync(scene, "OrchardCore_Media_Slugify");
            await SearchAsync(scene, "media");
            await FeatureRow(page, "Media TUS Uploads").WaitForAsync();
            await scene.HoldAsync(Math.Max(0.2, durations["enable-2"] - 5.2));
            await EnableFeatureAsync(scene, "OrchardCore_Media_Tus");
            await SearchAsync(scene, "media");
            await FeatureRow(page, "Media TUS Uploads").WaitForAsync();
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await SearchAsync(scene, "signalr");
            await FeatureRow(page, "Media SignalR").WaitForAsync();
            await scene.HoldAsync(Math.Max(0.2, durations["enable-3"] - 5.0));
            await EnableFeatureAsync(scene, "OrchardCore_Media_SignalR");
            await SearchAsync(scene, "signalr");
            await FeatureRow(page, "Media SignalR").WaitForAsync();
            await scene.HighlightAsync(FeatureRow(page, "Media SignalR"));
            await scene.HighlightAsync(FeatureRow(page, "SignalR"));
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.NavigateAsync("/Admin/Media/Options"), 300);
            var lifetime = page.Locator("label:has-text(\"Temporary file lifetime\")").First;
            await lifetime.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await scene.PauseAsync(400);
            await scene.HighlightAsync(lifetime);
            await scene.HoldAsync(Math.Max(0.2, durations["enable-4"] * 0.5));
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 2000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Media");
        await SelectFolderAsync(page, "Team Photos");
        await FileRow(page, "Summer Party 2019.jpg").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.HighlightAsync(FolderRow(page, "Team Photos"));
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FileRow(page, "Summer Party 2019.jpg"));
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#folder-tree .treeroot .folder-actions a.action-button").First);
            await page.GetByText("Create a subfolder").First.WaitForAsync();
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClickAsync(page.GetByText("Create a subfolder").First);
            await page.Locator("input[name=create-folder]").WaitForAsync();
            await scene.PauseAsync(500);
            await scene.TypeAsync(page.Locator("input[name=create-folder]"), "Images & docs", 10);
            await scene.HoldAsync(1.0);
            await scene.ClickAsync(page.Locator("#btn-submit"));
            await FolderRow(page, "images-docs").WaitForAsync();
            await scene.HighlightAsync(FolderRow(page, "images-docs"));
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#folder-tree li.folder a.folder-menu-item:has-text(\"images-docs\")").First);
            await ChooseFileAsync(scene, "The team (2020).jpg");
            await FileRow(page, "the-team-2020.jpg").WaitForAsync();
            await scene.HighlightAsync(FileRow(page, "the-team-2020.jpg"));
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ChooseFileAsync(scene, "The Team 2020.jpg");
            await FileRow(page, "the-team-2020-1.jpg").WaitForAsync();
            await scene.HighlightAsync(FileRow(page, "the-team-2020-1.jpg"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ResumeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Media");
        await SelectFolderAsync(page, "images-docs");
        await FileRow(page, "the-team-2020-1.jpg").WaitForAsync();

        var cdp = await scene.Context.NewCDPSessionAsync(page);
        await cdp.SendAsync("Network.enable");

        Task Throttle(int bytesPerSecond) => cdp.SendAsync("Network.emulateNetworkConditions", new Dictionary<string, object>
        {
            ["offline"] = false,
            ["latency"] = 0,
            ["downloadThroughput"] = -1,
            ["uploadThroughput"] = bytesPerSecond,
        });

        await Throttle(524288);
        await scene.StartCaptureAsync();

        var item = page.Locator(".upload-toast-item").First;
        var pause = item.Locator("button").First;

        await scene.StepAsync("resume-1", async () =>
        {
            await ChooseFileAsync(scene, "retreat-video.mp4");
            await item.WaitForAsync();
        });

        await scene.StepAsync("resume-2", async () =>
        {
            await scene.HighlightAsync(page.Locator(".upload-toast-container").First);
        });

        await scene.StepAsync("resume-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(pause);
            await page.WaitForFunctionAsync("() => !!document.querySelector('.upload-toast-item .is-paused, .upload-toast-item [class*=paused]')");
        });

        await scene.StepAsync("resume-4", async () =>
        {
            await Throttle(-1);
            await scene.ClickAsync(pause);
            await scene.OffCameraAsync(() => FileRow(page, "retreat-video.mp4").WaitForAsync(), 400);
        });

        await scene.StepAsync("resume-5", async () =>
        {
            await Throttle(524288);
            await ChooseFileAsync(scene, "office-tour.mp4");
            await item.WaitForAsync();
            await scene.OffCameraAsync(() => page.WaitForFunctionAsync("() => { const b = document.querySelector('.upload-toast-progress-bar'); return b && parseFloat(b.style.width) >= 20; }"), 400);
            await scene.NavigateAsync("/Admin/Media");
            await Throttle(524288);
        });

        await scene.StepAsync("resume-6", async () =>
        {
            await ChooseFileAsync(scene, "office-tour.mp4");
            await item.WaitForAsync();
            await scene.HoldAsync(1.8);
            await Throttle(-1);
            await scene.OffCameraAsync(() => FileRow(page, "office-tour.mp4").WaitForAsync(), 400);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var pageA = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Media");
        await SelectFolderAsync(pageA, "images-docs");
        await FileRow(pageA, "the-team-2020-1.jpg").WaitForAsync();

        var pageB = await scene.Context.NewPageAsync();
        pageB.SetDefaultTimeout(120000);
        await pageB.GotoAsync(BaseUrl + "/Admin/Media", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await SelectFolderAsync(pageB, "images-docs");
        await FileRow(pageB, "the-team-2020-1.jpg").WaitForAsync();
        await Task.Delay(2500);
        await pageA.BringToFrontAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("live-1", async () =>
        {
            await scene.HighlightAsync(pageA.Locator(".breadcrumb-path").First);
        });

        await scene.StepAsync("live-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SwitchToAsync(pageB);
            await ChooseFileAsync(scene, "summer-picnic.jpg");
            await FileRow(pageB, "summer-picnic.jpg").WaitForAsync();
        });

        await scene.StepAsync("live-3", async () =>
        {
            await scene.SwitchToAsync(pageA);
            await FileRow(pageA, "summer-picnic.jpg").WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });
            await scene.HighlightAsync(FileRow(pageA, "summer-picnic.jpg"));
        });

        await scene.StepAsync("live-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = FileRow(pageB, "summer-picnic.jpg");
            await row.Locator("a.action-button").ClickAsync();
            await pageB.Locator("#overlay_file_menu li[aria-label=Delete] a").ClickAsync();
            await pageB.Locator("button.ma-btn-primary:visible").First.ClickAsync();
            await FileRow(pageA, "summer-picnic.jpg").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached, Timeout = 15000 });
        });

        await scene.StepAsync("live-5", async () =>
        {
            await scene.HighlightAsync(pageA.Locator(".breadcrumb-path").First);
        });

        await scene.ClearHighlightsAsync();
        await pageB.CloseAsync();
    }
}

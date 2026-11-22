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

    // The two files of the upload clip: a text file and an SVG image, with fake content, outside the repository.
    private static string FilesDirectory => Path.Combine(Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(), "media-settings-files");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-lists", ListsAsync),
        ("02-role", RoleAsync),
        ("03-values", ValuesAsync),
        ("04-api", ApiAsync),
        ("04-recipes", RecipesAsync),
        ("02-upload", UploadAsync),
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

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.SetDefaultTimeout(120000);
        scene.Page.SetDefaultNavigationTimeout(120000);

        return scene;
    }

    // Opens a page with the browser's storage cleared, so every take starts with the menu as left after sign-in.
    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await scene.Page.Locator("#left-nav").WaitForAsync();
        await Task.Delay(500);
    }

    private static ILocator Group(IPage page, string title) => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator SettingsMedia(IPage page) => page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] button.item-label:has(span.title:text-is(\"Media\"))");

    private static ILocator SettingsMediaLink(IPage page, string text) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"Settings\"] ul[data-title=\"Media\"] a.item-label:has-text(\"{text}\")");

    // Clicks a link, with the page load cut out of the capture.
    private static async Task GoAsync(Scene scene, ILocator link)
    {
        await scene.MoveToAsync(link);
        var (x, y) = await scene.CenterOfAsync(link);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 300);
    }

    private static ILocator Row(IPage page, string label) => page.Locator($"div.row:has(label:text-is(\"{label}\"))").First;

    private static async Task OpenMediaSettingsMenuAsync(Scene scene)
    {
        var page = scene.Page;
        // The menu opens by itself on a page under Settings; click only what is still closed.
        var api = SettingsMediaLink(page, "API");

        if (!await api.IsVisibleAsync())
        {
            if (!await SettingsMedia(page).IsVisibleAsync())
            {
                await Group(page, "Settings").ClickAsync();
            }

            await SettingsMedia(page).ClickAsync();
        }

        await api.WaitForAsync();
        await Task.Delay(400);
    }

    private static async Task ListsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("lists-1", async () =>
        {
            await scene.ClickAsync(Group(page, "Settings"));
            await scene.ClickAsync(SettingsMedia(page));
            await GoAsync(scene, SettingsMediaLink(page, "Options"));
            await page.Locator("p.alert.alert-secondary").WaitForAsync();
            await scene.HighlightAsync(page.Locator("p.alert.alert-secondary").First);
        });

        await scene.StepAsync("lists-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "Supported sizes"));
        });

        await scene.StepAsync("lists-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "Restricted file extensions"));
        });

        await scene.StepAsync("lists-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "Allowed file extensions"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RoleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Roles/Edit/Editor");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("role-1", async () =>
        {
            var box = page.Locator("#search-box");
            await scene.ClickAsync(box, pauseAfter: 200);
            await box.PressSequentiallyAsync("additional permission", new LocatorPressSequentiallyOptions { Delay = 10 });
            await scene.PauseAsync(600);
            if (await box.InputValueAsync() != "additional permission")
            {
                await box.FillAsync("additional permission");
                await scene.PauseAsync(600);
            }

            var row = page.Locator("tr[data-text*=\"additional permission\" i]:visible").First;
            await scene.HighlightAsync(row);
            await scene.HighlightAsync(row.Locator(".badge").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ValuesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Media/Options");
        await page.Locator("p.alert.alert-secondary").WaitForAsync();
        await page.EvaluateAsync("() => window.scrollTo(0, 0)");
        await scene.StartCaptureAsync();

        await scene.StepAsync("values-1", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300, 8);
            await scene.HighlightAsync(Row(page, "Max browser cache days"));
            await scene.HighlightAsync(Row(page, "Max secure files browser cache days"));
            await scene.HighlightAsync(Row(page, "Max cache days"));
        });

        await scene.StepAsync("values-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300, 8);
            await scene.HighlightAsync(Row(page, "Max file size"));
            await scene.HighlightAsync(Row(page, "Max upload chunk size"));
            await scene.HighlightAsync(Row(page, "Temporary file lifetime"));
        });

        await scene.StepAsync("values-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300, 8);
            await scene.HighlightAsync(Row(page, "Cdn base url"));
            await scene.HighlightAsync(Row(page, "Assets request path"));
            await scene.HighlightAsync(page.Locator("div.row:has(label:text-is(\"Assets path\")):has(label:text-is(\"Assets users folder\"))").First);
        });

        await scene.StepAsync("values-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 3000, 8);
            await scene.HighlightAsync(Row(page, "Thumbnails disabled"));
            await scene.HighlightAsync(Row(page, "Storage provider"));
            await scene.HighlightAsync(Row(page, "Hierarchical namespace"));
            await scene.HighlightAsync(Row(page, "Atomic move"));
            await scene.HighlightAsync(Row(page, "SignalR"));
            await scene.HighlightAsync(Row(page, "TUS uploads"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ApiAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Media/Options");
        await OpenMediaSettingsMenuAsync(scene);
        await scene.StartCaptureAsync();

        var select = page.Locator("select[name$=\"AuthenticationScheme\"]");

        await scene.StepAsync("api-1", async () =>
        {
            await GoAsync(scene, SettingsMediaLink(page, "API"));
            await select.WaitForAsync();
            await scene.HighlightAsync(page.Locator("div.mb-3:has(select[name$=\"AuthenticationScheme\"])"));
        });

        await scene.StepAsync("api-2");

        await scene.StepAsync("api-3", async () =>
        {
            await scene.HoldAsync(2.5);
            await scene.SelectAsync(select, "Bearer");
        });

        await scene.StepAsync("api-4", async () =>
        {
            var clock = Stopwatch.StartNew();
            await scene.SelectAsync(select, "Cookie");
            await scene.ClearHighlightsAsync();
            await scene.HoldAsync(Math.Max(0.2, 5.5 - clock.Elapsed.TotalSeconds));
            await GoAsync(scene, page.Locator("button.save").First);
            await page.Locator(".message-success, .alert-success").First.WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static double SecondsOf(IReadOnlyDictionary<string, double> durations, string id) => durations.TryGetValue(id, out var seconds) ? seconds : 10;

    private static async Task RecipesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await Group(page, "Tools").ClickAsync();
        var link = page.Locator("#left-nav a[href$=\"/Admin/Recipes\"]");
        await link.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("recipes-1", async () =>
        {
            await GoAsync(scene, link);
            var box = page.Locator("#search-box");
            await box.WaitForAsync();
            await scene.PauseAsync(600);
            await scene.ClickAsync(box, pauseAfter: 200);
            await box.PressSequentiallyAsync("Media API", new LocatorPressSequentiallyOptions { Delay = 10 });
            await scene.PauseAsync(600);
            if (await box.InputValueAsync() != "Media API")
            {
                await box.FillAsync("Media API");
                await scene.PauseAsync(600);
            }

            await scene.HighlightAsync(page.Locator("li:visible, tr:visible, .list-group-item:visible").Filter(new() { HasText = "Bearer/PKCE" }).Last);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task UploadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Media");
        await page.Locator("div.table-row.file-item:has-text(\"about-bg.jpg\")").WaitForAsync();
        await scene.StartCaptureAsync();

        var button = page.Locator("label.upload-button");
        IFileChooser chooser = null;
        var clock = new Stopwatch();

        await scene.StepAsync("upload-1", async () =>
        {
            clock.Restart();
            await scene.HoldAsync(3.5);
            chooser = await page.RunAndWaitForFileChooserAsync(async () =>
            {
                await scene.MoveToAsync(button);
                await page.Mouse.DownAsync();
                await page.Mouse.UpAsync();
            });
            await scene.HoldAsync(Math.Max(0.2, 8.0 - clock.Elapsed.TotalSeconds));
            await chooser.SetFilesAsync(Path.Combine(FilesDirectory, "meeting-notes.txt"));
            await page.Locator(".notification-toast-item").First.WaitForAsync();
        });

        await scene.StepAsync("upload-2", async () =>
        {
            clock.Restart();
            await scene.HoldAsync(1.5);
            var second = await page.RunAndWaitForFileChooserAsync(async () =>
            {
                await scene.MoveToAsync(button);
                await page.Mouse.DownAsync();
                await page.Mouse.UpAsync();
            });
            await second.SetFilesAsync(Path.Combine(FilesDirectory, "company-logo.svg"));
            await page.Locator("div.table-row.file-item:has-text(\"company-logo.svg\")").WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }
}

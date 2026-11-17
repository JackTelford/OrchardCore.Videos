using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

// Media Cache against the Azurite emulator (development storage account only).
// The site is started with OrchardCore__Media__Azure__* environment variables; nothing is configured on camera.
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The Blog theme's stock pictures, uploaded without renaming (seed 2).
    private static string ImageFolder { get; } = Environment.GetEnvironmentVariable("VIDEO_IMAGE_FOLDER")
        ?? "C:/Code/oc-video-site/src/OrchardCore.Themes/TheBlogTheme/wwwroot/assets/img";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-purge", PurgeAsync),
        ("04-permission", PermissionAsync),
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

    // Seed 2 (outside the capture): enables Azure Media Storage and uploads the theme's three pictures to the library.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        context.SetDefaultTimeout(120000);
        var page = await context.NewPageAsync();

        await page.GotoAsync(BaseUrl + "/Admin/Features", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var box = page.Locator("#search-box");
        await box.WaitForAsync();
        await Task.Delay(800);
        await box.FillAsync("Azure Media Storage");
        await Task.Delay(800);
        var row = page.Locator("li[data-filter-value^=\"Azure Media Storage OrchardCore.Media.Azure.Storage \"]").First;
        var enable = row.Locator("a.btn:has-text(\"Enable\")").First;

        if (await enable.CountAsync() > 0)
        {
            await page.RunAndWaitForNavigationAsync(() => enable.ClickAsync());
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(4000);
        }

        Console.WriteLine("Azure Media Storage enabled");

        await page.GotoAsync(BaseUrl + "/Admin/Media", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("label.upload-button").First.WaitForAsync();

        if (await page.Locator("div.table-row.file-item:has-text(\"home-bg.jpg\")").CountAsync() == 0)
        {
            var chooser = await page.RunAndWaitForFileChooserAsync(() => page.Locator("label.upload-button").First.ClickAsync());
            await chooser.SetFilesAsync(new[] { "about-bg.jpg", "home-bg.jpg", "post-bg.jpg" }.Select(f => Path.Combine(ImageFolder, f)).ToArray());
            await page.Locator("div.table-row.file-item:has-text(\"post-bg.jpg\")").First.WaitForAsync();
            await Task.Delay(3000);
        }

        Console.WriteLine("Library files: " + await page.Locator("div.table-row.file-item").CountAsync());
        await context.CloseAsync();
    }

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.SetDefaultTimeout(120000);
        return scene;
    }

    private static ILocator Group(Scene scene, string title)
        => scene.Page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))").First;

    // Opens a top-level menu group when it is collapsed (no cursor movement).
    private static async Task ExpandGroupAsync(Scene scene, string title)
    {
        var group = Group(scene, title);

        if (await group.GetAttributeAsync("aria-expanded") != "true")
        {
            await group.ClickAsync();
            await Task.Delay(500);
        }
    }

    private static async Task OpenGroupAsync(Scene scene, string title)
    {
        var group = Group(scene, title);

        if (await group.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(group, pauseAfter: 300);
        }
    }

    // Moves to a menu link and clicks it; the page load is cut out of the video.
    private static async Task GoAsync(Scene scene, ILocator link)
    {
        await scene.MoveToAsync(link);
        var (x, y) = await scene.CenterOfAsync(link);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 300);
    }

    private static ILocator SettingsMediaItem(Scene scene, string text)
        => scene.Page.Locator($"#left-nav ul.nav-group-items[data-title=\"Settings\"] ul[data-title=\"Media\"] a.item-label:has(span.title:text-is(\"{text}\"))").First;

    private static async Task OpenSettingsMediaAsync(Scene scene)
    {
        await OpenGroupAsync(scene, "Settings");
        var media = scene.Page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] button.item-label:has(span.title:text-is(\"Media\"))").First;

        if (await media.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(media, pauseAfter: 300);
        }
    }

    private static ILocator FeatureRow(Scene scene, string prefix)
        => scene.Page.Locator($"li[data-filter-value^=\"{prefix}\"]").First;

    private static async Task SearchAsync(Scene scene, string text, int settle = 900)
    {
        var page = scene.Page;
        var box = page.Locator("#search-box");
        await box.WaitForAsync();
        await scene.OffCameraAsync(() => page.WaitForLoadStateAsync(LoadState.NetworkIdle), 100);
        await scene.PauseAsync(settle);
        await scene.TypeAsync(box, text, delay: 10);
        await scene.PauseAsync(settle / 2);

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var filtered = await page.Locator("li[data-filter-value]:not(.d-none)").CountAsync() < 12;
            Console.WriteLine($"  search '{text}' attempt {attempt}: value='{await box.InputValueAsync()}' filtered={filtered}");

            if (await box.InputValueAsync() == text && filtered)
            {
                return;
            }

            await box.FillAsync(text);
            await box.PressAsync("End");
            await scene.PauseAsync(900);
        }
    }

    private static async Task HoldUntilAsync(Scene scene, Stopwatch watch, double seconds)
    {
        var wait = seconds - watch.Elapsed.TotalSeconds;

        if (wait > 0)
        {
            await scene.HoldAsync(wait);
        }
    }

    private const string CacheFeature = "Media Cache OrchardCore.Media.Cache ";

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await ExpandGroupAsync(scene, "Tools");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            var watch = Stopwatch.StartNew();
            await GoAsync(scene, page.Locator("#left-nav ul.nav-group-items[data-title=\"Tools\"] a.item-label:has(span.title:text-is(\"Features\"))").First);
            await SearchAsync(scene, "Media Cache", 400);
            var row = FeatureRow(scene, CacheFeature);
            await row.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".feature-group:not(.d-none) h3:text-is(\"Content Management\")").First);
            await HoldUntilAsync(scene, watch, 6.3);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator(".admin-list-secondary").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var row = FeatureRow(scene, CacheFeature);
            var enable = row.Locator("a.btn:has-text(\"Enable\")").First;
            await scene.MoveToAsync(enable);
            await HoldUntilAsync(scene, watch, 5.5);
            var (x, y) = await scene.CenterOfAsync(enable);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 300);
            await SearchAsync(scene, "Media Cache", 300);
            await FeatureRow(scene, CacheFeature).Locator("a.btn:has-text(\"Disable\")").First.WaitForAsync();
            await scene.HighlightAsync(FeatureRow(scene, CacheFeature));
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenSettingsMediaAsync(scene);
            await GoAsync(scene, SettingsMediaItem(scene, "Cache"));
            await page.Locator("h1:has-text(\"Asset Cache\")").WaitForAsync();
            await scene.HighlightAsync(page.Locator(".alert-danger").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PurgeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/MediaCache");
        await page.Locator("h1:has-text(\"Asset Cache\")").WaitForAsync();
        await page.Locator(".alert.alert-secondary").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("purge-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".alert.alert-secondary").First);
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("ul.list-group li.list-group-item").First);
        });

        await scene.StepAsync("purge-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var purge = page.Locator("a[data-title=\"Purge All\"]").First;
            await scene.MoveToAsync(purge);
            var (x, y) = await scene.CenterOfAsync(purge);
            await page.Mouse.ClickAsync((float)x, (float)y);
            await page.Locator("#confirmRemoveModal").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await scene.HoldAsync(1.5);
            var ok = page.Locator("#modalOkButton");
            await scene.MoveToAsync(ok);
            await scene.HoldAsync(0.4);
            var (ox, oy) = await scene.CenterOfAsync(ok);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)ox, (float)oy)), 200);
        });

        await scene.StepAsync("purge-3", async () =>
        {
            var message = page.Locator("text=Asset cache purged.").First;
            await message.WaitForAsync();
            await scene.HighlightAsync(message);
        });

        await scene.StepAsync("purge-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/media/home-bg.jpg");
            await page.WaitForFunctionAsync("() => document.images.length == 1 && document.images[0].complete");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PermissionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await ExpandGroupAsync(scene, "Access Control");
        await scene.StartCaptureAsync();

        await scene.StepAsync("perm-1", async () =>
        {
            await GoAsync(scene, page.Locator("#left-nav ul.nav-group-items[data-title=\"Access Control\"] a.item-label:has(span.title:text-is(\"Roles\"))").First);
            var editor = page.Locator("li.list-group-item, tr").Filter(new() { Has = page.Locator("a.edit") }).Filter(new() { HasText = "Editor" }).First;
            await editor.WaitForAsync();
            await scene.HighlightAsync(editor);
            await scene.HoldAsync(1.2);
            await scene.ClearHighlightsAsync();
            var edit = editor.Locator("a.edit").First;
            await scene.MoveToAsync(edit);
            var (x, y) = await scene.CenterOfAsync(edit);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 200);
            await page.Locator("#search-box").WaitForAsync();
            await scene.MoveToAsync(1000, 110);
        });

        await scene.StepAsync("perm-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            var box = page.Locator("#search-box");
            await scene.ClickAsync(box, pauseAfter: 300);
            await scene.TypeAsync(box, "cache", delay: 10);
            var row = page.Locator("[id=\"Checkbox.ManageAssetCache\"]").First;
            await row.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            Console.WriteLine($"  perm-2 typed {watch.Elapsed.TotalSeconds:F1}");
            await scene.PauseAsync(400);
            await scene.HighlightAsync(page.Locator("tr:has([id=\"Checkbox.ManageAssetCache\"])").First);
            Console.WriteLine($"  perm-2 highlighted {watch.Elapsed.TotalSeconds:F1}");
            await scene.HoldAsync(1.0);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(row, pauseAfter: 300);
            Console.WriteLine($"  perm-2 ticked {watch.Elapsed.TotalSeconds:F1}");
            await HoldUntilAsync(scene, watch, 3.6);
            var save = page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            var (x, y) = await scene.CenterOfAsync(save);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 200);
            await page.Locator("text=Role updated successfully.").First.WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }
}

using Microsoft.Playwright;

namespace Recorder;

// Amazon Media Storage against the S3Mock emulator (fake keys only).
// The site is started with OrchardCore__Media__AmazonS3__* environment variables; nothing is configured on camera.
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The picture the Blog theme ships for the home page banner; it is uploaded without renaming.
    private static string UploadFile { get; } = Environment.GetEnvironmentVariable("VIDEO_UPLOAD_FILE")
        ?? "C:/Code/oc-video-site/src/OrchardCore.Themes/TheBlogTheme/wwwroot/assets/img/home-bg.jpg";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-enable", EnableAsync),
        ("03-check", CheckAsync),
        ("04-serve", ServeAsync),
        ("06-cache", CacheAsync),
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
        return scene;
    }

    private static ILocator Group(Scene scene, string title)
        => scene.Page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))").First;

    // A short pointer move: the machine is loaded, and every step of a move costs a round trip.
    private static async Task<(double X, double Y)> FastMoveAsync(Scene scene, ILocator locator)
    {
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        return (x, y);
    }

    private static async Task FastClickAsync(Scene scene, ILocator locator, int pauseAfter = 200)
    {
        var (x, y) = await FastMoveAsync(scene, locator);
        await scene.PauseAsync(120);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await scene.PauseAsync(pauseAfter);
    }

    // Opens a top-level menu group when it is collapsed.
    private static async Task OpenGroupAsync(Scene scene, string title)
    {
        var group = Group(scene, title);

        if (await group.GetAttributeAsync("aria-expanded") != "true")
        {
            await FastClickAsync(scene, group, 250);
        }
    }

    // Moves to a menu link and clicks it; the page load is cut out of the video.
    private static async Task GoAsync(Scene scene, ILocator link)
    {
        var (x, y) = await FastMoveAsync(scene, link);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 200);
    }

    private static ILocator SettingsMediaItem(Scene scene, string text)
        => scene.Page.Locator($"#left-nav ul.nav-group-items[data-title=\"Settings\"] ul[data-title=\"Media\"] a.item-label:has(span.title:text-is(\"{text}\"))").First;

    private static async Task OpenSettingsMediaAsync(Scene scene)
    {
        await OpenGroupAsync(scene, "Settings");
        var media = scene.Page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] button.item-label:has(span.title:text-is(\"Media\"))").First;

        if (await media.GetAttributeAsync("aria-expanded") != "true")
        {
            await FastClickAsync(scene, media, 250);
        }
    }

    private static ILocator FeatureRow(Scene scene, string prefix)
        => scene.Page.Locator($"li[data-filter-value^=\"{prefix}\"]").First;

    private static async Task SearchAsync(Scene scene, string text, int settle = 900)
    {
        var page = scene.Page;
        var box = page.Locator("#search-box");
        await scene.OffCameraAsync(async () =>
        {
            await box.WaitForAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }, 100);
        await scene.PauseAsync(settle / 3);
        await FastClickAsync(scene, box, 100);
        await box.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = 10 });
        await scene.PauseAsync(250);

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

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        const string Storage = "Amazon Media Storage OrchardCore.Media.AmazonS3 ";

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await GoAsync(scene, page.Locator("#left-nav ul.nav-group-items[data-title=\"Tools\"] a.item-label:has(span.title:text-is(\"Features\"))").First);
            await SearchAsync(scene, "Amazon Media", 400);
            await FeatureRow(scene, Storage).WaitForAsync();
            await scene.HighlightAsync(page.Locator(".feature-group:not(.d-none) h3:text-is(\"Hosting\")").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = FeatureRow(scene, Storage);
            await scene.HighlightAsync(row.Locator(".direct-dependencies .badge:visible").First);
            await scene.HoldAsync(0.6);
            await scene.ClearHighlightsAsync();
            var enable = row.Locator("a.btn:has-text(\"Enable\")").First;
            var (x, y) = await FastMoveAsync(scene, enable);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 200);
            await SearchAsync(scene, "Amazon Media", 300);
            await FeatureRow(scene, Storage).Locator("a.btn:has-text(\"Disable\")").First.WaitForAsync();
            await scene.HighlightAsync(FeatureRow(scene, Storage));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CheckAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        ILocator Wrapper(string label) => page.Locator($".ocat-wrapper:has(label:text-is(\"{label}\"))").First;

        await scene.StepAsync("check-1", async () =>
        {
            await OpenSettingsMediaAsync(scene);
            await GoAsync(scene, SettingsMediaItem(scene, "Amazon S3 Options"));
            await page.Locator("h1:has-text(\"Amazon S3 storage options\")").WaitForAsync();
            await scene.HighlightAsync(Wrapper("Bucket Name"));
            await scene.HighlightAsync(Wrapper("Base Path"));
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Wrapper("Create Bucket"));
        });

        await scene.StepAsync("check-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenSettingsMediaAsync(scene);
            await GoAsync(scene, SettingsMediaItem(scene, "Options"));
            await page.Locator("h1:has-text(\"Media Options\")").WaitForAsync();
            var storage = page.Locator("div.row:has(label:text-is(\"Storage provider\"))").First;
            var hier = page.Locator("div.row:has(label:text-is(\"Hierarchical namespace\"))").First;
            var atomic = page.Locator("div.row:has(label:text-is(\"Atomic move\"))").First;
            await page.EvaluateAsync("() => { const l = [...document.querySelectorAll('label')].find(e => e.textContent.trim() === 'Storage provider'); l.scrollIntoView({block: 'center'}); }");
            await scene.PauseAsync(400);
            await scene.HighlightAsync(storage);
            await scene.HoldAsync(2.4);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(hier);
            await scene.HoldAsync(2.4);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(atomic);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ServeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("serve-1", async () =>
        {
            await OpenGroupAsync(scene, "Media");
            await GoAsync(scene, page.Locator("#left-nav ul.nav-group-items[data-title=\"Media\"] a.item-label:has-text(\"Library\")").First);
            var empty = page.Locator("text=This folder is empty").First;
            await scene.OffCameraAsync(() => empty.WaitForAsync(), 200);
            await scene.HighlightAsync(empty);
        });

        await scene.StepAsync("serve-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var label = page.Locator("label.upload-button").First;
            var (x, y) = await FastMoveAsync(scene, label);
            await scene.PauseAsync(150);
            var chooser = await page.RunAndWaitForFileChooserAsync(() => page.Mouse.ClickAsync((float)x, (float)y));
            await chooser.SetFilesAsync(UploadFile);
            await scene.OffCameraAsync(() => page.Locator("div.table-row.file-item:has-text(\"home-bg.jpg\")").First.WaitForAsync(), 300);
            await scene.PauseAsync(400);
        });

        await scene.StepAsync("serve-3", async () =>
        {
            await scene.NavigateAsync("/media/home-bg.jpg");
            await page.Locator("img").First.WaitForAsync();
            await page.WaitForFunctionAsync("() => document.images.length > 0 && document.images[0].complete");
            await page.EvaluateAsync("""
                () => {
                  const label = document.createElement('div');
                  label.className = 'demo-caption';
                  label.textContent = '/media/home-bg.jpg';
                  Object.assign(label.style, { left: '24px', top: '24px' });
                  label.dataset.demo = 'caption';
                  document.documentElement.appendChild(label);
                }
                """);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CacheAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        const string Cache = "Amazon Media Image Cache OrchardCore.Media.AmazonS3.ImageCache ";

        await scene.StepAsync("cache-3", async () =>
        {
            await SearchAsync(scene, "Amazon Media Image Cache", 400);
            var row = FeatureRow(scene, Cache);
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
            await scene.HoldAsync(0.6);
            await scene.ClearHighlightsAsync();
            var enable = row.Locator("a.btn:has-text(\"Enable\")").First;
            var (x, y) = await FastMoveAsync(scene, enable);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 200);
            await SearchAsync(scene, "Amazon Media Image Cache", 300);
            await FeatureRow(scene, Cache).Locator("a.btn:has-text(\"Disable\")").First.WaitForAsync();
            await scene.HighlightAsync(FeatureRow(scene, Cache));
        });

        await scene.StepAsync("cache-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => page.GotoAsync(BaseUrl + "/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }));
            await page.WaitForFunctionAsync("""
                () => new Promise(resolve => {
                  const h = document.querySelector('header.masthead');
                  const m = h && /url\(["']?([^"')]+)/.exec(getComputedStyle(h).backgroundImage);
                  if (!m || !m[1].includes('/media/home-bg.jpg')) { resolve(false); return; }
                  const img = new Image();
                  img.onload = () => resolve(true);
                  img.onerror = () => resolve(false);
                  img.src = m[1];
                })
                """, null, new PageWaitForFunctionOptions { Timeout = 60000 });
            await scene.NudgeAsync();
            await scene.HighlightAsync(page.Locator("header.masthead").First, "banner profile");
        });

        await scene.ClearHighlightsAsync();
    }
}

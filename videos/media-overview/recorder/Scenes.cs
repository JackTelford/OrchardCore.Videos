using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-where", WhereAsync),
        ("03-library", LibraryAsync),
        ("04-content", ContentAsync),
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

    // Loads the Library once, outside the capture, so that the _Users folder exists before the first take.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/Media", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(1500);
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // Opens the page with the browser's storage cleared, so every take starts from the defaults.
    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    private static async Task WhereAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Contents/ContentItems");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        ILocator Group(string title) => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");
        ILocator MediaLink(string text) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"Media\"] a.item-label:has-text(\"{text}\")");

        await scene.StepAsync("where-1", async () =>
        {
            await scene.ClickAsync(Group("Media"));
            await MediaLink("Library").WaitForAsync();
            await scene.HighlightAsync(MediaLink("Library"));
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(MediaLink("Profiles"));
        });

        await scene.StepAsync("where-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(MediaLink("Profiles"));
            var row = page.Locator("li, tr, .list-group-item").Filter(new() { HasText = "banner" }).Last;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("where-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Group("Settings"));
            var media = page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] button.item-label:has(span.title:text-is(\"Media\"))");
            await scene.ClickAsync(media);
            var options = page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] ul[data-title=\"Media\"] a.item-label:has-text(\"Options\")");
            var api = page.Locator("#left-nav ul.nav-group-items[data-title=\"Settings\"] ul[data-title=\"Media\"] a.item-label:has-text(\"API\")");
            await api.ScrollIntoViewIfNeededAsync();
            await options.WaitForAsync();
            await scene.HighlightAsync(options);
            await scene.HoldAsync(7.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(api);
        });

        await scene.StepAsync("where-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Group("Tools"));
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav ul.nav-group-items[data-title=\"Tools\"] a.item-label:has-text(\"Features\")")), 400);
            await page.Locator("#search-box").WaitForAsync();
            await scene.PauseAsync(600);
            await scene.TypeAsync(page.Locator("#search-box"), "media");
            await scene.PauseAsync(700);
            if (await page.Locator("#search-box").InputValueAsync() != "media")
            {
                await page.Locator("#search-box").FillAsync("media");
                await scene.PauseAsync(700);
            }
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.HighlightAsync(page.Locator("[data-filter-value^=\"Media OrchardCore.Media \"] span.badge:has-text(\"Enabled\")").First);
        });

        await scene.StepAsync("where-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var slug = page.Locator("[data-filter-value^=\"Media Slugify \"]").First;
            var secure = page.Locator("[data-filter-value^=\"Secure Media \"]").First;
            await slug.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(slug);
            await scene.HoldAsync(8.0);
            await scene.ClearHighlightsAsync();
            await secure.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(secure);
        });

        await scene.StepAsync("where-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var tus = page.Locator("[data-filter-value^=\"Media TUS Uploads \"]").First;
            var signalr = page.Locator("[data-filter-value^=\"Media SignalR \"]").First;
            await tus.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(tus);
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await signalr.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(signalr);
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            var azure = page.Locator("[data-filter-value^=\"Azure Media Storage \"]").First;
            var s3 = page.Locator("[data-filter-value^=\"Amazon Media Storage \"]").First;
            await s3.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(azure);
            await scene.HighlightAsync(s3);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LibraryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Media");
        await page.Locator("div.table-row.file-item").Nth(2).WaitForAsync();
        await scene.StartCaptureAsync();

        var gridButton = page.Locator(".action-bar a:has(svg[data-icon=\"grip\"])");
        var listButton = page.Locator(".action-bar a:has(svg[data-icon=\"list\"])");

        await scene.StepAsync("library-1", async () =>
        {
            await scene.MoveToAsync(page.Locator("#folder-tree li.folder").First);
            await scene.HighlightAsync(page.Locator("#folder-tree"));
        });

        await scene.StepAsync("library-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("div.header-row").First);
            await scene.MoveToAsync(page.Locator("div.table-row.file-item").Nth(0));
            await scene.MoveToAsync(page.Locator("div.table-row.file-item").Nth(2));
        });

        await scene.StepAsync("library-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(gridButton);
            await scene.HighlightAsync(gridButton);
            await scene.HoldAsync(4.5);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(gridButton);
            await page.Locator("ol.file-items-grid img").Nth(2).WaitForAsync();
            await page.WaitForFunctionAsync("() => [...document.querySelectorAll('ol.file-items-grid img')].every(i => i.complete)");
        });

        await scene.StepAsync("library-4", async () =>
        {
            await scene.ClickAsync(listButton);
            await page.Locator("div.table-row.file-item").Nth(2).WaitForAsync();
            await scene.TypeAsync(page.Locator("#file-filter-input"), "post");
        });

        await scene.StepAsync("library-5", async () =>
        {
            await scene.ClickAsync(page.Locator("#clear-file-filter-button"));
            await page.Locator("div.table-row.file-item").Nth(2).WaitForAsync();
            await scene.MoveToAsync(page.Locator("label.upload-button"));
            await scene.HighlightAsync(page.Locator("label.upload-button"));
        });

        await scene.StepAsync("library-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("div.table-row.file-item").Nth(0).Locator("a.action-button"));
            await page.Locator("[id=\"overlay_file_menu\"]:visible").WaitForAsync();
        });

        await scene.StepAsync("library-7", async () =>
        {
            var root = page.Locator("#folder-tree li.folder").First;
            await scene.MoveToAsync(root);
            await scene.ClickAsync(root.Locator(".folder-actions a.action-button").First);
            await scene.PauseAsync(500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ContentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Contents/ContentTypes/Article/Create");
        await page.Locator(".media-field-app button:has-text(\"Add media\")").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("content-1", async () =>
        {
            var field = page.Locator(".field-wrapper-article-image");
            await field.ScrollIntoViewIfNeededAsync();
            await page.EvaluateAsync("() => { const f = document.querySelector('.field-wrapper-article-image'); const r = f.getBoundingClientRect(); window.scrollBy(0, r.top - (window.innerHeight - r.height) / 2); }");
            await scene.PauseAsync(500);
            await scene.HighlightAsync(field);
        });

        await scene.StepAsync("content-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator(".media-field-app button:has-text(\"Add media\")"));
            await page.Locator(".mf-picker-shell").WaitForAsync();
            await scene.OffCameraAsync(() => page.Locator(".mf-picker-media-gallery div.table-row.file-item").Nth(2).WaitForAsync());
        });

        await scene.StepAsync("content-3", async () =>
        {
            var row = page.Locator(".mf-picker-media-gallery div.table-row.file-item:has-text(\"about-bg.jpg\")");
            await scene.ClickAsync(row.Locator("input[name=\"select-file\"]"));
            await scene.PauseAsync(600);
            await scene.ClickAsync(page.Locator(".mf-modal-footer button.mf-btn-primary"));
            await page.Locator("li.mf-thumb-item img").WaitForAsync();
            await page.EvaluateAsync("() => window.scrollBy(0, 200)");
        });

        await scene.StepAsync("content-4", async () =>
        {
            await scene.NavigateAsync("/about");
            await page.Locator("header.masthead").WaitForAsync();
            await scene.PauseAsync(1000);
        });
    }
}

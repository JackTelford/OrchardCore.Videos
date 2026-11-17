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

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-list", ListAsync),
        ("04-create", CreateAsync),
        ("05-site", SiteAsync),
        ("06-manage", ManageAsync),
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

    // Moves the cursor to the element, then clicks it with the page load cut off camera.
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 400);
    }

    private static async Task WaitForHeaderAsync(Scene scene)
    {
        var page = scene.Page;
        await page.WaitForFunctionAsync("() => { const h = document.querySelector('header.masthead'); return !!h && getComputedStyle(h).backgroundImage.includes('rmode=pad'); }", null, new PageWaitForFunctionOptions { Timeout = 60000 });
        await page.EvaluateAsync("async () => { const h = document.querySelector('header.masthead'); const m = /url\\(\"?([^\")]+)\"?\\)/.exec(getComputedStyle(h).backgroundImage); return (await fetch(m[1])).ok; }");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        var media = page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Media\"))").First;
        var profiles = page.Locator("#left-nav ul.nav-group-items[data-title=\"Media\"] a.item-label:has-text(\"Profiles\")").First;
        var row = page.Locator("li.list-group-item").Filter(new() { HasText = "banner" }).First;

        await scene.StepAsync("list-1", async () =>
        {
            await scene.ClickAsync(media);
            await profiles.WaitForAsync();
            await scene.HoldAsync(1.0);
            await GoAsync(scene, profiles);
            await row.WaitForAsync();
            await scene.MoveToAsync(page.Locator("#search-box"));
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.HighlightAsync(page.GetByRole(AriaRole.Link, new() { Name = "banner", Exact = true }));
            await scene.HoldAsync(3.0);
            await scene.HighlightAsync(page.Locator(".admin-list-secondary").First);
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(page.Locator("#search-box"));
            await scene.HoldAsync(1.0);
            await scene.MoveToAsync(page.Locator("a.btn-primary", new() { HasText = "Edit" }).First);
            await scene.HoldAsync(1.0);
            await scene.MoveToAsync(page.Locator("a.btn-danger", new() { HasText = "Delete" }).First);
            await scene.HoldAsync(1.0);
            await scene.MoveToAsync(page.Locator("a.create").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/MediaProfiles");
        await page.Locator("a.create").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First);
            await page.Locator("#Name").WaitForAsync();
            await scene.MoveToAsync(page.Locator("#Name"));
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.ClickAsync(page.Locator("#Name"));
            await scene.TypeAsync(page.Locator("#Name"), "thumbnail", 3);
            await scene.ClickAsync(page.Locator("#Hint"));
            await scene.TypeAsync(page.Locator("#Hint"), "A thumbnail (320px x 240px, cropped)", 3);
            if (await page.Locator("#Hint").InputValueAsync() != "A thumbnail (320px x 240px, cropped)")
            {
                await page.Locator("#Hint").FillAsync("A thumbnail (320px x 240px, cropped)");
            }
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.SelectAsync(page.Locator("#SelectedWidth"), "-1");
            await page.Locator("#CustomWidth").WaitForAsync();
            await page.Locator("#CustomWidth").FillAsync("");
            await scene.ClickAsync(page.Locator("#CustomWidth"));
            await scene.TypeAsync(page.Locator("#CustomWidth"), "320", 3);
            await scene.SelectAsync(page.Locator("#SelectedHeight"), "240");
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.SelectAsync(page.Locator("#SelectedMode"), "2");
            await scene.MoveToAsync(page.Locator("#SelectedFormat"), 0, -45);
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.SelectAsync(page.Locator("#SelectedFormat"), "3");
            await page.Locator("#Quality").FillAsync("");
            await scene.ClickAsync(page.Locator("#Quality"));
            await scene.TypeAsync(page.Locator("#Quality"), "80", 3);
        });

        await scene.StepAsync("create-6", async () =>
        {
            var clock = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            await page.Locator("#AutoOrient").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(400);
            await scene.HighlightAsync(page.Locator("#AutoOrient"));
            await scene.HoldAsync(4.0);
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1500);
            while (clock.Elapsed.TotalSeconds < 8.0)
            {
                await scene.PauseAsync(200);
            }

            await GoAsync(scene, page.Locator("button[name='submit'][value='Save']").First);
            await page.Locator("li.list-group-item").Filter(new() { HasText = "thumbnail" }).First.WaitForAsync();
            await scene.MoveToAsync(page.Locator("li.list-group-item").Filter(new() { HasText = "thumbnail" }).First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("header.masthead").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("header.masthead"));
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.NavigateAsync("/blog/post-1"), 300);
            await page.Locator("header.masthead").WaitForAsync();
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.OffCameraAsync(() => scene.NavigateAsync("/Admin/MediaProfiles"), 300);
            var edit = page.Locator("li.list-group-item").Filter(new() { HasText = "banner" }).GetByRole(AriaRole.Link, new() { Name = "Edit" }).First;
            await edit.WaitForAsync();
            await GoAsync(scene, edit);
            await page.Locator("#SelectedMode").WaitForAsync();
        });

        await scene.StepAsync("site-4", async () =>
        {
            await scene.SelectAsync(page.Locator("#SelectedMode"), "3");
            await page.Locator("#BackgroundColor").WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await scene.ClickAsync(page.Locator("#BackgroundColor"));
            await scene.TypeAsync(page.Locator("#BackgroundColor"), "336699", 3);
            await scene.PauseAsync(1500);
            await GoAsync(scene, page.Locator("button[name='submit'][value='Save']").First);
            await page.Locator("li.list-group-item").Filter(new() { HasText = "banner" }).First.WaitForAsync();
        });

        await scene.StepAsync("site-5", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/");
                await WaitForHeaderAsync(scene);
            }, 300);
            await scene.HoldAsync(1.0);
        });

        await scene.StepAsync("site-6", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/blog/post-1");
                await WaitForHeaderAsync(scene);
            }, 300);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/MediaProfiles");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        var thumbnailRow = page.Locator("li.list-group-item").Filter(new() { HasText = "thumbnail" });

        await scene.StepAsync("manage-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"));
            await scene.TypeAsync(page.Locator("#search-box"), "thumb", 3);
            await scene.PauseAsync(500);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Keyboard.PressAsync("Enter")), 300);
            await thumbnailRow.First.WaitForAsync();
        });

        await scene.StepAsync("manage-2", async () =>
        {
            var clock = Stopwatch.StartNew();
            await GoAsync(scene, thumbnailRow.GetByRole(AriaRole.Link, new() { Name = "Edit" }).First);
            await page.Locator("#Name").WaitForAsync();
            await scene.HighlightAsync(page.Locator("#Name"));
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            while (clock.Elapsed.TotalSeconds < 5.0)
            {
                await scene.PauseAsync(200);
            }

            await GoAsync(scene, page.Locator("a.cancel").First);
            await page.Locator("li.list-group-item").Filter(new() { HasText = "banner" }).First.WaitForAsync();
        });

        await scene.StepAsync("manage-3", async () =>
        {
            await scene.ClickAsync(thumbnailRow.GetByRole(AriaRole.Link, new() { Name = "Delete" }).First);
            await page.Locator("#modalOkButton").WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await scene.PauseAsync(2000);
            var ok = page.Locator("#modalOkButton");
            await scene.MoveToAsync(ok);
            var (x, y) = await scene.CenterOfAsync(ok);
            await scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y));
        });

        await scene.ClearHighlightsAsync();
    }
}

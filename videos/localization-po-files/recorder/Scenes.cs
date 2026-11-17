using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the localization-po-files video: see recording-plan.md.</summary>
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
        ("04-result", ResultAsync),
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

    private static async Task AtAsync(Stopwatch watch, int milliseconds)
    {
        var remaining = milliseconds - (int)watch.ElapsedMilliseconds;

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }
    }

    // Opens a page of the left menu: expands each collapsed group above the link (outermost first), then clicks it.
    private static async Task OpenMenuAsync(Scene scene, string href, ILocator ready = null)
    {
        var page = scene.Page;
        var link = page.Locator($"#left-nav a[href$='{href}']").First;
        await link.WaitForAsync(new() { State = WaitForSelectorState.Attached });
        var ancestors = link.Locator("xpath=ancestor::ul[@data-title]");
        var count = await ancestors.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var ul = ancestors.Nth(i);

            if (!await ul.IsVisibleAsync())
            {
                var title = await ul.GetAttributeAsync("data-title");
                var caption = ul.Locator($"xpath=preceding-sibling::figcaption | ../figcaption[.//span[normalize-space()='{title}']] | ../*/figcaption[.//span[normalize-space()='{title}']]").First;
                await scene.ClickAsync(caption, pauseAfter: 120);
            }
        }

        await GoAsync(scene, link, ready);
    }

    // Moves to a link or button, clicks it and cuts the page load out of the video.
    private static async Task GoAsync(Scene scene, ILocator link, ILocator ready = null)
    {
        await scene.MoveToAsync(link);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(async () => await link.ClickAsync());

            if (ready is not null)
            {
                await ready.WaitForAsync(new() { Timeout = 90000, State = WaitForSelectorState.Attached });
            }
        }, 450);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("#search-box");
        var enable = page.Locator("#btn-enable-OrchardCore_Localization");

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Features", box);
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Localization", new() { Delay = 30 });
            await enable.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(enable);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, enable, page.Locator("#btn-disable-OrchardCore_Localization"));
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Settings/localization", page.Locator(".localization-settings-wrapper table tbody tr"));
        });

        await scene.StepAsync("enable-4", async () =>
        {
            var wrapper = page.Locator(".localization-settings-wrapper");
            await scene.SelectAsync(wrapper.Locator("select.form-select"), "fr");
            await scene.ClickAsync(wrapper.Locator("a.btn-success"), pauseAfter: 300);
            await Assertions.Expect(wrapper.Locator("table tbody tr")).ToHaveCountAsync(2);
            await GoAsync(scene, page.Locator("button.save").First, page.Locator(".toast.message-warning"));
        });

        await scene.StepAsync("enable-5", async () =>
        {
            await page.Locator(".toast.message-warning").Filter(new() { HasText = "The site has been restarted for the settings to take effect." }).First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".toast.message-warning").First);
        });
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("result-2", async () =>
        {
            var narration = durations["result-2"];
            await scene.NavigateAsync("/Admin/Features?culture=fr");
            await page.Locator("#search-box[placeholder='Rechercher']").WaitForAsync();
            var wait = narration * 0.42;
            var each = (narration - wait - 0.6) / 5;
            await scene.HoldAsync(wait);

            foreach (var target in new[]
            {
                page.Locator(".oc-breadcrumb-title:visible").First,
                page.Locator("#search-box"),
                page.Locator("#bulk-action-menu-button"),
                page.Locator("a[id^='btn-enable-']").First,
                page.Locator("#list-summary"),
            })
            {
                await scene.ClearHighlightsAsync();
                await scene.HighlightAsync(target);
                await scene.HoldAsync(each);
            }
        });

        await scene.StepAsync("result-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#left-nav"));
        });

        await scene.StepAsync("result-4", async () =>
        {
            await scene.ClearHighlightsAsync();
        });
    }
}

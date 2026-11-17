using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the admin-culture-and-language-header video: see recording-plan.md.</summary>
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
        ("03-cultures", CulturesAsync),
        ("04-pick", PickAsync),
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

    // Expands each collapsed group of the left menu above a link (outermost first) and returns the link.
    private static async Task<ILocator> ExpandMenuAsync(Scene scene, string href)
    {
        var page = scene.Page;
        var link = page.Locator($"#left-nav a[href$='{href}']").First;
        await link.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 90000 });
        var ancestors = link.Locator("xpath=ancestor::ul[@data-title]");
        var count = await ancestors.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var ul = ancestors.Nth(i);

            if (!await ul.IsVisibleAsync())
            {
                var title = await ul.GetAttributeAsync("data-title");
                var caption = ul.Locator($"xpath=preceding-sibling::figcaption | ../figcaption[.//span[normalize-space()='{title}']] | ../*/figcaption[.//span[normalize-space()='{title}']]").First;
                await scene.ClickAsync(caption, pauseAfter: 150);
            }
        }

        return link;
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
                await ready.WaitForAsync(new() { Timeout = 120000, State = WaitForSelectorState.Visible });
            }
        }, 450);
    }

    private static async Task RingAsync(Scene scene, ILocator target, double seconds)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Row(Scene scene) => scene.Page.Locator("[data-filter-value]").Filter(new() { Has = scene.Page.Locator("a.btn") }).First;

    // Clicks Enable on the visible row at the moment where the page will have loaded about 4 seconds before the narration ends.
    private static async Task EnableAsync(Scene scene, Stopwatch watch, double narration, string id, ILocator box, ILocator notice)
    {
        var button = scene.Page.Locator($"#btn-enable-{id}");
        await scene.MoveToAsync(button);
        await AtAsync(watch, Math.Max(300, (int)(narration * 1000) - 4800));
        await GoAsync(scene, button, notice);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("#search-box");

        await scene.StepAsync("enable-1", async () =>
        {
            var link = await ExpandMenuAsync(scene, "/Admin/Features");
            await GoAsync(scene, link, box);
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Admin Culture Picker", new() { Delay = 30 });
            await page.Locator("#btn-enable-OrchardCore_Localization_AdminCulturePicker").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await RingAsync(scene, page.Locator(".direct-dependencies .badge:has-text('Localization')").First, 2.0);
            await EnableAsync(scene, watch, durations["enable-2"], "OrchardCore_Localization_AdminCulturePicker", box, page.GetByText("was enabled").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Content Language Header", new() { Delay = 30 });
            var button = page.Locator("#btn-enable-OrchardCore_Localization_ContentLanguageHeader");
            await button.WaitForAsync();
            await RingAsync(scene, page.Locator(".direct-dependencies .badge:has-text('Localization')").First, 1.5);
            await GoAsync(scene, button, box);
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.PressAsync("Escape");
            await page.Keyboard.TypeAsync("Localization", new() { Delay = 30 });
            await Assertions.Expect(page.Locator("#list-summary")).ToContainTextAsync("Showing");
            await scene.HighlightAsync(page.Locator(".badge:has-text('Enabled')").First);
            await scene.HoldAsync(1.0);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task CulturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var wrapper = page.Locator(".localization-settings-wrapper");

        await scene.StepAsync("cultures-1", async () =>
        {
            var link = await ExpandMenuAsync(scene, "/Admin/Settings/localization");
            await GoAsync(scene, link, wrapper.Locator("table tbody tr").First);
            await RingAsync(scene, page.Locator("ul.user-top-navbar").First, 2.0);
        });

        await scene.StepAsync("cultures-2", async () =>
        {
            await scene.SelectAsync(wrapper.Locator("select").First, "fr");
            await scene.ClickAsync(wrapper.Locator("a.btn-success"), pauseAfter: 300);
            await Assertions.Expect(wrapper.Locator("table tbody tr")).ToHaveCountAsync(2);
            await GoAsync(scene, page.Locator("button.save").First, page.Locator(".toast.message-warning"));
        });

        await scene.StepAsync("cultures-3", async () =>
        {
            await RingAsync(scene, page.Locator(".toast.message-warning").First, 2.0);
            await scene.HighlightAsync(page.Locator("#oc-culture-picker"));
        });
    }

    private static async Task PickAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/localization");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var picker = page.Locator("#oc-culture-picker");

        await scene.StepAsync("pick-1", async () =>
        {
            await scene.ClickAsync(picker, pauseAfter: 300);
            await page.Locator(".language-menu-item").First.WaitForAsync();
        });

        await scene.StepAsync("pick-2", async () =>
        {
            await GoAsync(scene, page.Locator(".language-menu-item[data-language-value='fr']").First, picker.Filter(new() { HasText = "fran" }));
        });

        await scene.StepAsync("pick-3", async () =>
        {
            await RingAsync(scene, page.Locator("p.alert.alert-warning").Filter(new() { HasText = "The website might be restarted" }).First, 3.0);
        });

        await scene.StepAsync("pick-4", async () =>
        {
            var visit = page.Locator("ul.user-top-navbar a.nav-link:has(i.fa-external-link)").First;
            await scene.MoveToAsync(visit);
            await scene.HoldAsync(1.0);
            await GoAsync(scene, visit, page.Locator("h1, .masthead").First);
        });

        await scene.StepAsync("pick-5", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/localization");
        });

        await scene.StepAsync("pick-6", async () =>
        {
            await scene.ClickAsync(picker, pauseAfter: 300);
            await GoAsync(scene, page.Locator(".language-menu-item[data-language-value='en-US']").First, picker.Filter(new() { HasText = "English" }));
        });
    }
}

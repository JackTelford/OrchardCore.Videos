using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the localization-cultures video: see recording-plan.md.</summary>
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
        ("04-french", FrenchAsync),
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

    // Opens a group of the left menu when it is collapsed.
    private static async Task ExpandAsync(Scene scene, string group)
    {
        var list = scene.Page.Locator($"#left-nav ul[data-title='{group}']");

        if (!await list.IsVisibleAsync())
        {
            await scene.ClickAsync(scene.Page.Locator($"#left-nav figcaption:has(span.title:text-is('{group}'))"), pauseAfter: 300);
        }
    }

    private static ILocator Wrapper(Scene scene) => scene.Page.Locator(".localization-settings-wrapper");

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await ExpandAsync(scene, "Tools");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav ul[data-title='Tools']").GetByRole(AriaRole.Link, new() { Name = "Features", Exact = true }));
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "Localization", delay: 90);
            var enable = page.Locator("#btn-enable-OrchardCore_Localization");
            await enable.WaitForAsync();
            var heading = page.Locator("h3").Filter(new() { HasText = "Internationalization" }).First;
            await scene.HighlightAsync(heading);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(enable);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("#btn-enable-OrchardCore_Localization");
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await enable.ClickAsync());
                await page.Locator(".toast").Filter(new() { HasText = "The feature Localization was enabled." }).WaitForAsync(new() { Timeout = 60000 });
            });
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await ExpandAsync(scene, "Settings");
            await ExpandAsync(scene, "Localization");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav ul[data-title='Localization']").GetByRole(AriaRole.Link, new() { Name = "Cultures", Exact = true }));
        });
    }

    private static async Task CulturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/localization");
        var page = scene.Page;
        await Wrapper(scene).Locator("table tbody tr").First.WaitForAsync();
        await scene.StartCaptureAsync();
        var wrapper = Wrapper(scene);
        var rows = wrapper.Locator("tbody tr");

        await scene.StepAsync("cultures-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("h1").First);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".alert-warning").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("label.ocat-label").First);
        });

        await scene.StepAsync("cultures-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(wrapper.Locator("thead"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(rows.First);
        });

        await scene.StepAsync("cultures-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(wrapper.Locator("select"), "fr");
        });

        await scene.StepAsync("cultures-4", async () =>
        {
            await scene.ClickAsync(wrapper.Locator("a.btn-success"), pauseAfter: 700);
            await scene.SelectAsync(wrapper.Locator("select"), "de");
            await scene.ClickAsync(wrapper.Locator("a.btn-success"));
        });

        await scene.StepAsync("cultures-5", async () =>
        {
            await scene.ClickAsync(rows.Nth(1).Locator("input[type=radio]"), pauseAfter: 700);
            await scene.ClickAsync(rows.Nth(0).Locator("input[type=radio]"));
        });

        await scene.StepAsync("cultures-6", async () =>
        {
            await scene.HighlightAsync(rows.Nth(2).Locator("a[title='Remove culture']"));
        });

        await scene.StepAsync("cultures-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            var label = page.Locator("label.form-check-label").Filter(new() { HasText = "Fall back to parent" }).First;
            await scene.HighlightAsync(label);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("input[type=checkbox]").Last);
        });

        await scene.StepAsync("cultures-8", async () =>
        {
            var save = page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await save.ClickAsync());
                await page.Locator(".toast").Filter(new() { HasText = "Site settings updated successfully." }).WaitForAsync(new() { Timeout = 60000 });
            });
        });

        await scene.StepAsync("cultures-9", async () =>
        {
            var warning = page.Locator(".toast").Filter(new() { HasText = "The site has been restarted for the settings to take effect." });
            await warning.WaitForAsync(new() { Timeout = 60000 });
            await scene.HighlightAsync(warning);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FrenchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/localization");
        var page = scene.Page;
        await Wrapper(scene).Locator("table tbody tr").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("french-1", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/localization?culture=fr");
            await Assertions.Expect(page.Locator("button.save").First).ToContainTextAsync("Enregistrer");
        });

        await scene.StepAsync("french-2", async () =>
        {
            foreach (var target in new[] { page.Locator("h1").First, Wrapper(scene).Locator("thead"), Wrapper(scene).Locator("a.btn-success"), page.Locator("label.form-check-label").Last, page.Locator("button.save").First })
            {
                await scene.HighlightAsync(target);
                await scene.PauseAsync(1100);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("french-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/localization");
            await Assertions.Expect(page.Locator("button.save").First).ToContainTextAsync("Save");
        });
    }
}

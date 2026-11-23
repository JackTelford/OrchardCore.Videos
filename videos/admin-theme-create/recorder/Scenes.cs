using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

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

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("05-enable", EnableAsync),
        ("06-result", ResultAsync),
    ];

    private static ILocator ThemeCard(IPage page)
        => page.Locator(".theme-card").Filter(new() { Has = page.Locator("h4.card-title", new() { HasTextString = "MyAdminTheme" }) }).First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.OpenAsync("/Admin");
        await page.Locator("#ta-left-sidebar").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#ta-left-sidebar button, #ta-left-sidebar a").Filter(new() { HasTextString = "Design" }).First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#ta-left-sidebar a[href$='/Admin/Themes']").First);
            await page.Locator(".theme-card").First.WaitForAsync();
            await ThemeCard(page).ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(ThemeCard(page));
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(ThemeCard(page).Locator(".list-group-item").Filter(new() { Has = page.Locator("i.fa-tags") }).First, "admin");
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(2500);
            await scene.ClickAndWaitForNavigationAsync(ThemeCard(page).GetByRole(AriaRole.Button, new() { Name = "Make Current" }));
            await page.Locator(".toast-body").First.WaitForAsync(new() { Timeout = 60000 });
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await ThemeCard(page).ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(ThemeCard(page).Locator(".card-footer span.text-muted").First);
        });
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.OpenAsync("/Admin");
        await page.Locator(".ta-navbar-brand").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("result-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".ta-navbar-brand").First, "AdminBranding");
        });

        await scene.StepAsync("result-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#ta-left-sidebar button, #ta-left-sidebar a").Filter(new() { HasTextString = "Settings" }).First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#ta-left-sidebar a[href*='/Admin/Settings/general']").First);
            foreach (var label in new[] { "Site name", "Page title format", "Base url" })
            {
                await scene.HighlightAsync(page.Locator("label.ocat-label").Filter(new() { HasTextString = label }).First);
            }
        });

        await scene.StepAsync("result-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
        });
    }
}

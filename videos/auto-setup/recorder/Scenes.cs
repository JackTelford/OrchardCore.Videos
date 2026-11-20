using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Auto Setup". 01-setup-screen runs against a site WITHOUT Auto Setup configuration; 05-first-request runs against
/// a site WITH it (site name "Lakeside Journal") that nobody has requested yet. No state.json: both clips are signed out.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-setup-screen", SetupScreenAsync),
        ("05-first-request", FirstRequestAsync),
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

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task SetupScreenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("#SiteTimeZone").WaitForAsync();

        // The time zone of the recording machine is not shown.
        await scene.Page.Locator("#SiteTimeZone").SelectOptionAsync("America/Los_Angeles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("#SiteName").First, 1.5);
            await ShowAsync(scene, scene.Page.Locator("#recipeButton").First, 1.5);
            await scene.HighlightAsync(scene.Page.Locator("#SiteTimeZone").First);
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300);
            await ShowAsync(scene, scene.Page.Locator("#DatabaseProvider").First, 1);
            await scene.SelectAsync(scene.Page.Locator("#DatabaseProvider").First, "Postgres");

            foreach (var field in new[] { "#TablePrefix", "#ConnectionString", "#TableSchema" })
            {
                await ShowAsync(scene, scene.Page.Locator(field).First, 1.2);
            }
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 500);
            await ShowAsync(scene, scene.Page.Locator("fieldset legend").Filter(new() { HasTextString = "Super User" }).First, 1);

            foreach (var field in new[] { "#UserName", "#Email", "#Password", "#PasswordConfirmation" })
            {
                await ShowAsync(scene, scene.Page.Locator(field).First, 1);
            }
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FirstRequestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(180_000);
        scene.Page.SetDefaultTimeout(180_000);
        await scene.StartCaptureAsync();

        await scene.StepAsync("first-1", async () =>
        {
            await scene.HoldAsync(3);
            await scene.OffCameraAsync(() => scene.NavigateAsync("/"));
            await scene.PauseAsync(500);
        });

        await scene.StepAsync("first-2", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator(".navbar-brand").First, 1.5);
            await scene.MoveToAsync(scene.Page.Locator(".post-preview").First);
        });

        await scene.StepAsync("first-3", async () =>
        {
            await scene.OffCameraAsync(() => scene.NavigateAsync("/Login"), leadMilliseconds: 100);
            await scene.TypeAsync(scene.Page.Locator("#LoginForm_UserName"), UserName, delay: 30);
            await scene.TypeAsync(scene.Page.Locator("#LoginForm_Password"), Password, delay: 25);
            var submit = scene.Page.Locator("form.auth-form button[type=submit]").First;
            await scene.MoveToAsync(submit);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(submit), leadMilliseconds: 100);
        });

        await scene.StepAsync("first-4", async () =>
        {
            await scene.OffCameraAsync(() => scene.NavigateAsync("/Admin/Settings/general"), leadMilliseconds: 100);
            await ShowAsync(scene, scene.Page.GetByLabel("Site name", new() { Exact = true }).First, 2);
            await scene.HighlightAsync(scene.Page.GetByLabel("Default Time Zone").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

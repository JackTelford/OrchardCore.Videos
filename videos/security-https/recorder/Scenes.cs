using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Security: HTTPS". The site must be opened over HTTPS (https://localhost:5012): over HTTP the HTTPS settings page
/// ignores what is saved and disables its fields. Record 02-features, then 03-settings.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "https://localhost:5011";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-settings", SettingsAsync),
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

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static ILocator Group(Scene scene, string name)
        => scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).First;

    private static ILocator Link(Scene scene, string name)
        => scene.Page.Locator("#left-nav").GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static async Task OpenGroupAsync(Scene scene, string name, int pause = 300)
    {
        var group = Group(scene, name);

        if (await group.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(group, pauseAfter: pause);
        }
    }

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, bool clear = true)
    {
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools", 150);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(Link(scene, "Features")), leadMilliseconds: 100);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "HTTPS", delay: 40);
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-enable-OrchardCore_Https") }).First);
        });

        await scene.StepAsync("features-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btn-enable-OrchardCore_Https").First);
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings", 250);
            await OpenGroupAsync(scene, "Security", 250);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "HTTPS"));
            await scene.HighlightAsync(scene.Page.Locator("p.alert.alert-warning").First);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("input[type=checkbox][name$='.RequireHttps']").First);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            var hsts = scene.Page.Locator("select[name$='.StrictTransportSecurityMode']").First;
            await ShowAsync(scene, hsts, 2);
            await ShowAsync(scene, scene.Page.Locator(".alert.alert-danger").First, 2);
            await scene.PauseAsync(1500);
            await scene.SelectAsync(hsts, "FromConfiguration");
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".form-check").Filter(new() { Has = scene.Page.Locator("input[type=checkbox][name$='.RequireHttpsPermanent']") }).First);
        });

        await scene.StepAsync("settings-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[name$='.SslPort']").First);
        });

        await scene.StepAsync("settings-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        });

        await scene.StepAsync("settings-7", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("input[type=checkbox][name$='.RequireHttps']").First, 1.5);
            await scene.HighlightAsync(scene.Page.Locator("select[name$='.StrictTransportSecurityMode']").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

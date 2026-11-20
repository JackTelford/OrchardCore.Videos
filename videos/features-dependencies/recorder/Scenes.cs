using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Features and their dependencies". All on /Admin/Features. Record in this order, in one take:
/// 02-read, 05-demand (no change), then 03-enable (enables Users Registration and Email), then 04-disable.
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
        ("02-read", ReadAsync),
        ("05-demand", DemandAsync),
        ("03-enable", EnableAsync),
        ("04-disable", DisableAsync),
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

    private static async Task<Scene> OpenFeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();
        return scene;
    }

    private static ILocator Row(Scene scene, string id) => scene.Page.Locator($"[data-filter-value*='{id}']").First;

    private static ILocator Type(Scene scene, string text) => scene.Page.Locator("#search-box").First;

    private static async Task SearchAsync(Scene scene, string text)
    {
        await scene.TypeAsync(scene.Page.Locator("#search-box").First, text, delay: 40);
    }

    private static async Task ShowEmailAndUsersAsync(Scene scene, string badge)
    {
        var email = scene.Page.Locator("[data-filter-value^='Email OrchardCore.Email '] .metadata .badge").First;
        var users = scene.Page.Locator("[data-filter-value*='OrchardCore.Users.Registration'] .metadata .badge").First;
        await scene.PauseAsync(1000);
        await scene.HighlightAsync(email);
        await scene.PauseAsync(1800);
        await scene.ClearHighlightsAsync();
        await users.ScrollIntoViewIfNeededAsync();
        await scene.ScrollAsync(0, 150);
        await scene.HighlightAsync(users);
    }

    private static async Task ReadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenFeaturesAsync(browser, directory, durations);
        var registration = "[data-filter-value*='OrchardCore.Users.Registration']";

        await scene.StepAsync("read-1", async () =>
        {
            await scene.PauseAsync(2500);
            await SearchAsync(scene, "Users Registration");
        });

        await scene.StepAsync("read-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator($"{registration} .fw-semibold").First);
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator($"{registration} .direct-dependencies").First);
        });

        await scene.StepAsync("read-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(scene.Page.Locator($"{registration} .direct-dependencies .badge").Filter(new() { HasText = "Email" }).First);
            await scene.Page.Locator(".tooltip.show").First.WaitForAsync();
        });

        await scene.StepAsync("read-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator($"{registration} button.badge-show-more").First, pauseAfter: 300);
            await scene.Page.Locator($"{registration} .transitive-dependencies:not(.d-none)").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator($"{registration} .transitive-dependencies").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DemandAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenFeaturesAsync(browser, directory, durations);
        var visibility = scene.Page.Locator("button[data-id='visibility-filter']").First;
        var items = scene.Page.Locator(".dropdown-menu.show .dropdown-item");

        await scene.StepAsync("demand-1", async () =>
        {
            await scene.PauseAsync(5000);
            await scene.ClickAsync(visibility, pauseAfter: 600);
            await scene.ClickAsync(items.Filter(new() { HasText = "Hide on demand" }).First, pauseAfter: 300);
        });

        await scene.StepAsync("demand-2", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await SearchAsync(scene, "Roles Core Services");
            await scene.PauseAsync(2500);
            await scene.MoveToAsync(scene.Page.Locator("[data-filter-value*='OrchardCore.Roles.Core'] span[data-bs-toggle='tooltip']:has(> button[disabled])").First);
            await scene.Page.Locator(".tooltip.show").First.WaitForAsync();
        });

        await scene.StepAsync("demand-3", async () =>
        {
            await scene.ClickAsync(visibility, pauseAfter: 500);
            await scene.ClickAsync(items.Filter(new() { HasText = "Hide always enabled" }).First, pauseAfter: 300);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator("#search-box").First, pauseAfter: 200);
            await scene.Page.Keyboard.PressAsync("Escape");
            await SearchAsync(scene, "OrchardCore.Features");
            await scene.PauseAsync(2500);
            await scene.MoveToAsync(scene.Page.Locator("[data-filter-value*='OrchardCore.Features'] span[data-bs-toggle='tooltip']:has(> button[disabled])").First);
            await scene.Page.Locator(".tooltip.show").First.WaitForAsync();
        });
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenFeaturesAsync(browser, directory, durations);
        var registration = "[data-filter-value*='OrchardCore.Users.Registration']";

        await scene.StepAsync("enable-1", async () =>
        {
            await SearchAsync(scene, "email");
            await ShowEmailAndUsersAsync(scene, "Disabled");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator($"{registration} .metadata .badge").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(1500);
            var enable = scene.Page.Locator("a[href$='/Features/OrchardCore.Users.Registration/Enable']").First;
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => enable.ClickAsync());
                await scene.Page.GetByText("The feature Users Registration was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("The feature Users Registration was enabled.").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await SearchAsync(scene, "email");
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("[data-filter-value^='Email OrchardCore.Email '] .metadata .badge").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator($"{registration} .metadata .badge").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 150);
            await scene.HighlightAsync(scene.Page.Locator($"{registration} .metadata .badge").First);
        });
    }

    private static async Task DisableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenFeaturesAsync(browser, directory, durations);
        var registration = "[data-filter-value*='OrchardCore.Users.Registration']";

        await scene.StepAsync("disable-1", async () =>
        {
            await scene.PauseAsync(500);
            await SearchAsync(scene, "email");
            await scene.PauseAsync(800);
            await scene.ClickAsync(scene.Page.Locator("a[href$='/Features/OrchardCore.Email/Disable']").First, pauseAfter: 200);
            await scene.Page.Locator("#confirmRemoveModal.show").WaitForAsync();
        });

        await scene.StepAsync("disable-2", async () =>
        {
            await scene.PauseAsync(4500);
            await scene.HighlightAsync(scene.Page.Locator("#confirmRemoveModal .modal-body ul").First);
        });

        await scene.StepAsync("disable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1000);
            var yes = scene.Page.Locator("#modalOkButton").First;
            await scene.MoveToAsync(yes);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => yes.ClickAsync());
                await scene.Page.GetByText("The feature Email was disabled.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await SearchAsync(scene, "registration");
            await scene.HighlightAsync(scene.Page.Locator($"{registration} .metadata .badge").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

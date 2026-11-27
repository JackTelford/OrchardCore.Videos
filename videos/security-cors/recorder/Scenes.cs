using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "CORS". Record in one take, in order: 02-enable (enables CORS Configuration), 03-create (saves the policy
/// Partner portal), 04-unsafe (shows the warning, saves nothing). No PrepareAsync.
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
        ("02-enable", EnableAsync),
        ("03-create", CreateAsync),
        ("04-unsafe", UnsafeAsync),
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
        => scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static async Task OpenGroupAsync(Scene scene, string name)
    {
        var group = Group(scene, name);

        if (await group.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(group, pauseAfter: 300);
        }
    }

    // Clicks something that loads another page, with the wait cut out of the clip.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);
    }

    private static async Task ScrollToAsync(Scene scene, string selector)
    {
        await scene.ClearHighlightsAsync();
        await scene.Page.Locator(selector).First.EvaluateAsync("e => e.scrollIntoView({ behavior: 'smooth', block: 'center' })");
        await scene.PauseAsync(800);
    }

    private static string Card(int index) => $"#corsAdmin .card >> nth={index}";

    private static ILocator Text(Scene scene, int card) => scene.Page.Locator($"{Card(card)} >> input[type=text]").First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            var features = scene.Page.Locator("a[href$='/Admin/Features']").First;
            await scene.MoveToAsync(features);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(features), leadMilliseconds: 100);
            await scene.ClickAsync(scene.Page.Locator("#search-box").First, pauseAfter: 200);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "CORS", delay: 60);
            await scene.Page.Locator("[data-filter-value*='OrchardCore.Cors']").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("[data-filter-value*='OrchardCore.Cors']").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = scene.Page.Locator("[data-filter-value*='OrchardCore.Cors'] a:has-text('Enable')").First;
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => enable.ClickAsync());
                await scene.Page.GetByText("The feature CORS Configuration was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("The feature CORS Configuration was enabled.").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await OpenGroupAsync(scene, "Settings");
            await OpenGroupAsync(scene, "Security");
            var cors = scene.Page.Locator("a[href$='/Admin/Cors']").First;
            await scene.MoveToAsync(cors);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(cors), leadMilliseconds: 100);
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("p.alert.alert-warning").First);
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#list-alert:not(.d-none)").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Cors");
        await scene.Page.Locator("#corsAdmin button:has-text('Add a policy')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#corsAdmin button:has-text('Add a policy')").First, pauseAfter: 300);
            await scene.Page.Locator("#corsAdmin h3:has-text('New policy')").First.WaitForAsync();
            await ScrollToAsync(scene, Card(5));
        });

        await scene.StepAsync("create-2", async () =>
        {
            await ScrollToAsync(scene, Card(0));
            await Text(scene, 0).FillAsync("");
            await scene.TypeAsync(Text(scene, 0), "Partner portal", delay: 45);
            await scene.ClickAsync(scene.Page.Locator("label[for='set-default-policy']").First, pauseAfter: 300);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await ScrollToAsync(scene, Card(1));
            await scene.HighlightAsync(scene.Page.Locator($"{Card(1)} >> .form-check").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("label[for='allow-credentials']").First, pauseAfter: 300);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await ScrollToAsync(scene, Card(2));
            await scene.HighlightAsync(scene.Page.Locator("label[for='allowed-origins']").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(Text(scene, 2), "https://portal.example.com", delay: 25);
            await scene.ClickAsync(scene.Page.Locator($"{Card(2)} >> button:has-text('Add Origin')").First, pauseAfter: 300);
        });

        await scene.StepAsync("create-5", async () =>
        {
            await ScrollToAsync(scene, Card(3));
            await scene.ClickAsync(scene.Page.Locator("label[for='allowed-headers']").First, pauseAfter: 200);
            await scene.TypeAsync(Text(scene, 3), "Content-Type", delay: 30);
            await scene.ClickAsync(scene.Page.Locator($"{Card(3)} >> button:has-text('Add Header')").First, pauseAfter: 200);
            await ScrollToAsync(scene, Card(4));
            await scene.ClickAsync(scene.Page.Locator("label[for='allowed-methods']").First, pauseAfter: 200);
            await scene.TypeAsync(Text(scene, 4), "GET", delay: 60);
            await scene.ClickAsync(scene.Page.Locator($"{Card(4)} >> button:has-text('Add Method')").First, pauseAfter: 300);
        });

        await scene.StepAsync("create-6", async () =>
        {
            await ScrollToAsync(scene, Card(5));
            await scene.HighlightAsync(scene.Page.Locator(Card(5)).First);
        });

        await scene.StepAsync("create-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, "#corsAdmin .action-bar-bottom");
            var save = scene.Page.Locator("#corsAdmin .action-bar-bottom button.btn-primary").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText("The CORS settings have updated successfully.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task UnsafeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Cors");
        await scene.Page.Locator("#corsAdmin ul.list-group > li").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("unsafe-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#corsAdmin ul.list-group > li >> button:has-text('Edit')").First, pauseAfter: 300);
            await scene.Page.Locator("label[for='allow-credentials']").First.WaitForAsync();
            await ScrollToAsync(scene, "label[for='allow-credentials']");
            await scene.ClickAsync(scene.Page.Locator("label[for='allow-credentials']").First, pauseAfter: 300);
            await ScrollToAsync(scene, "label[for='allowed-origins']");
            await scene.ClickAsync(scene.Page.Locator("label[for='allowed-origins']").First, pauseAfter: 300);
        });

        await scene.StepAsync("unsafe-2", async () =>
        {
            await ScrollToAsync(scene, "#corsAdmin .action-bar-bottom");
            var save = scene.Page.Locator("#corsAdmin .action-bar-bottom button.btn-primary").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText("Affected policies").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.HighlightAsync(scene.Page.Locator(".toast.message-warning").First);
        });
    }
}

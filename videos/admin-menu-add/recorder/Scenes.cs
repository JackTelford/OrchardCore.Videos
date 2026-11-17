using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Add Admin Menu Entries to Your Module": 05-admin enables the module MyModule (built into the site,
/// disabled), finds My Root View in the admin menu, and opens its two pages. The wait for the tenant restart is cut
/// off camera.
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
        ("05-admin", AdminAsync),
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

    private static async Task FastAsync(ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 8 });
    }

    private static ILocator Group(IPage page, string text)
        => page.Locator("#left-nav button.nav-group-label", new() { HasTextRegex = new($"^\\s*{text}\\s*$") }).First;

    private static ILocator Link(IPage page, string text)
        => page.Locator("#left-nav a.item-label", new() { HasTextRegex = new($"^\\s*{text}\\s*$") }).First;

    private static async Task AdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("admin-1", async () =>
        {
            await scene.ClickAsync(Group(page, "Tools"), pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(Link(page, "Features"));
            await page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("admin-2", async () =>
        {
            await FastAsync(page.Locator("#search-box"), "MyModule");
            await page.Locator("#btn-enable-MyModule").WaitForAsync();
            await scene.PauseAsync(600);
            await scene.MoveToAsync(page.Locator("#btn-enable-MyModule"));
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await page.Locator("#btn-enable-MyModule").ClickAsync();
                await page.Locator(".toast.message-success").First.WaitForAsync(new() { Timeout = 120_000 });
            }));
            await scene.MoveToAsync(page.Locator(".toast.message-success").First);
        });

        await scene.StepAsync("admin-3", async () =>
        {
            var root = Group(page, "My Root View");
            await scene.HighlightAsync(root);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(root);
            await Link(page, "Child One").WaitForAsync();
        });

        await scene.StepAsync("admin-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Link(page, "Child One"));
            var paragraph = page.Locator("p", new() { HasTextRegex = new(@"^\s*View One\s*$") }).First;
            await paragraph.WaitForAsync();
            await scene.MoveToAsync(paragraph);
        });

        await scene.StepAsync("admin-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Link(page, "Child Two"));
            await page.Locator("p", new() { HasTextRegex = new(@"^\s*View Two\s*$") }).First.WaitForAsync();
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recording of "Docker Orchard Core": the Setup screen of a container that has never served a request, then the Blog it
/// installs. No Auto Setup, no state.json, no sign-in.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:8080";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("04-first-request", FirstRequestAsync),
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

    private static async Task FirstRequestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("#SiteName").WaitForAsync();
        scene.Page.SetDefaultTimeout(180_000);
        scene.Page.SetDefaultNavigationTimeout(180_000);
        await scene.StartCaptureAsync();

        await scene.StepAsync("first-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Setup" }).First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("first-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#DatabaseProvider").First);
            await scene.HoldAsync(0.8);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#SiteName").First, "Docker Demo", delay: 20);
            await scene.ClickAsync(scene.Page.Locator("#recipeButton").First, pauseAfter: 250);
            await scene.ClickAsync(scene.Page.Locator("#recipes a.dropdown-item[data-recipe-name='Blog']").First);
            await scene.SelectAsync(scene.Page.Locator("#SiteTimeZone").First, "America/Los_Angeles");
        });

        await scene.StepAsync("first-3", async () =>
        {
            await scene.Page.Locator("#UserName").First.PressSequentiallyAsync("admin", new() { Delay = 12 });
            await scene.Page.Locator("#Email").First.PressSequentiallyAsync("admin@example.com", new() { Delay = 8 });
            await scene.Page.Locator("#Password").First.PressSequentiallyAsync(Password, new() { Delay = 8 });
            await scene.Page.Locator("#PasswordConfirmation").First.PressSequentiallyAsync(Password, new() { Delay = 8 });
            await scene.MoveToAsync(scene.Page.Locator("#SubmitButton").First);
        });

        await scene.StepAsync("first-4", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#SubmitButton").First);
                await scene.Page.Locator("#mainNav .navbar-brand").WaitForAsync();
            });
            await scene.HighlightAsync(scene.Page.Locator("#mainNav .navbar-brand").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

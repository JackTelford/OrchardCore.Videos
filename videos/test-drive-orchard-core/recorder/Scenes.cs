using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Test Drive Orchard Core". The site is NOT provisioned by Auto Setup and there is no state.json:
/// 03-setup installs it on camera, 04-explore signs in on camera. Record 03-setup, then 04-explore.
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
        ("03-setup", SetupAsync),
        ("04-explore", ExploreAsync),
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

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("#SiteName").WaitForAsync();

        // The time zone of the recording machine is not shown.
        await scene.Page.Locator("#SiteTimeZone").SelectOptionAsync("America/Los_Angeles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("div.bg-light").First);
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#SiteName"), "Test Drive");
            await scene.PauseAsync(4000);
            await scene.ClickAsync(scene.Page.Locator("#recipeButton"));
            await scene.Page.Locator("#recipes .dropdown-menu").First.WaitForAsync();
            await scene.PauseAsync(2500);
            await scene.ClickAsync(scene.Page.Locator("#recipes .dropdown-item[data-recipe-name='Blog']").First);
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#SiteTimeZone"));
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#DatabaseProvider"));
        });

        await scene.StepAsync("setup-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#UserName"), "admin", delay: 25);
            await scene.TypeAsync(scene.Page.Locator("#Email"), "admin@example.com", delay: 15);
            await scene.TypeAsync(scene.Page.Locator("#Password"), Password, delay: 15);
            await scene.TypeAsync(scene.Page.Locator("#PasswordConfirmation"), Password, delay: 15);
            await scene.ClickAsync(scene.Page.Locator("#Password"), pauseAfter: 200);
        });

        await scene.StepAsync("setup-5", async () =>
        {
            scene.Page.SetDefaultNavigationTimeout(180000);
            scene.Page.SetDefaultTimeout(180000);
            await scene.HighlightAsync(scene.Page.Locator("#SubmitButton"));
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(scene.Page.Locator("#SubmitButton"));
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Locator("#SubmitButton").ClickAsync()));
            await scene.Page.Locator("a.navbar-brand").First.WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ExploreAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("header.masthead").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("explore-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a.navbar-brand").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("header.masthead h1").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 380);
        });

        await scene.StepAsync("explore-2", async () =>
        {
            await scene.NavigateAsync("/Admin");
            await scene.Page.Locator("#LoginForm_UserName").WaitForAsync();
            await scene.TypeAsync(scene.Page.Locator("#LoginForm_UserName"), UserName, delay: 50);
            await scene.TypeAsync(scene.Page.Locator("#LoginForm_Password"), Password, delay: 40);
        });

        await scene.StepAsync("explore-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form.auth-form button[type=submit]").First);
            await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();

            foreach (var title in new[] { "Create content", "Customize theme", "Manage users", "Learn more" })
            {
                await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = title }).First);
                await scene.PauseAsync(1500);

                if (title != "Learn more")
                {
                    await scene.ClearHighlightsAsync();
                }
            }
        });
    }
}

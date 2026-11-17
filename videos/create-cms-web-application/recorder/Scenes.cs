using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Create a CMS web application with dotnet new". The site is NOT provisioned by Auto Setup and there is no
/// state.json: 05-setup installs it on camera, 06-admin signs in on camera. Record 05-setup, then 06-admin.
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
        ("05-setup", SetupAsync),
        ("06-admin", AdminAsync),
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
            await scene.MoveToAsync(scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Setup" }).First);
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#SiteName"), "Orchard Core", delay: 60);
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#recipeButton"), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator("#recipes a.dropdown-item[data-recipe-name='Blog']").First);
        });

        await scene.StepAsync("setup-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#SiteTimeZone"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("setup-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#DatabaseProvider"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("setup-6", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#UserName"), "admin", delay: 25);
            await scene.TypeAsync(scene.Page.Locator("#Email"), "admin@example.com", delay: 15);
            await scene.TypeAsync(scene.Page.Locator("#Password"), Password, delay: 15);
            await scene.TypeAsync(scene.Page.Locator("#PasswordConfirmation"), Password, delay: 15);
            await scene.ClickAsync(scene.Page.Locator("#Password"), pauseAfter: 200);
        });

        await scene.StepAsync("setup-7", async () =>
        {
            scene.Page.SetDefaultNavigationTimeout(120_000);
            scene.Page.SetDefaultTimeout(120_000);
            await scene.MoveToAsync(scene.Page.Locator("#SubmitButton"));
            await scene.PauseAsync(600);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Locator("#SubmitButton").ClickAsync()));
            await scene.Page.Locator(".navbar-brand").First.WaitForAsync(new() { Timeout = 120_000 });
            await scene.ScrollAsync(0, 380);
        });
    }

    private static async Task AdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator(".navbar-brand").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("admin-1", async () =>
        {
            await scene.NavigateAsync("/admin");
            await scene.Page.Locator("#LoginForm_UserName").WaitForAsync();
        });

        await scene.StepAsync("admin-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#LoginForm_UserName"), UserName, delay: 50);
            await scene.TypeAsync(scene.Page.Locator("#LoginForm_Password"), Password, delay: 40);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).First);
            await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        });

        await scene.StepAsync("admin-3");
    }
}

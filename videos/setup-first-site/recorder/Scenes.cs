using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Setup: Install Your First Orchard Core Site". The site is NOT provisioned by Auto Setup: it starts
/// uninstalled, 03-finish installs it, 04-signin signs in. Record in the order 02-questions, 03-finish, 04-signin.
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
        ("02-questions", QuestionsAsync),
        ("03-finish", FinishAsync),
        ("04-signin", SignInAsync),
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

    // The server's own time zone is not shown: the list starts on its first entry, as it does on a server in UTC.
    private static Task FirstZoneAsync(Scene scene)
        => scene.Page.Locator("#SiteTimeZone").SelectOptionAsync(new SelectOptionValue { Index = 0 });

    private static async Task QuestionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("#SiteName").WaitForAsync();
        await FirstZoneAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("screen-1", async () =>
        {
            await scene.MoveToAsync(scene.Page.Locator("#culturesList"));
            await scene.HighlightAsync(scene.Page.Locator("#culturesList"));
            await scene.HoldAsync(4);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(scene.Page.Locator("#SiteName"));
            await scene.MoveToAsync(scene.Page.Locator("#SiteTimeZone"));
            await scene.MoveToAsync(scene.Page.Locator("fieldset legend").Filter(new() { HasTextString = "Super User" }).First);
        });

        await scene.StepAsync("name-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#SiteName"), "Contoso Bakery");
        });

        await scene.StepAsync("recipe-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#recipeButton"));
            await scene.HoldAsync(2);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#recipeButton"), pauseAfter: 4000);
            await scene.ClickAsync(scene.Page.Locator("#recipes .dropdown-item[data-recipe-name='Blog']").First);
        });

        await scene.StepAsync("zone-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#SiteTimeZone"));
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#SiteTimeZone"), "America/Los_Angeles");
        });

        await scene.StepAsync("db-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#DatabaseProvider"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FinishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(300_000);
        await scene.OpenAsync("/");
        await scene.Page.Locator("#SiteName").FillAsync("Contoso Bakery");
        await scene.Page.Locator("#recipeButton").ClickAsync();
        await scene.Page.Locator("#recipes .dropdown-item[data-recipe-name='Blog']").First.ClickAsync();
        await scene.Page.Locator("#SiteTimeZone").SelectOptionAsync("America/Los_Angeles");
        await scene.Page.Locator("#SubmitButton").ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("user-1", async () =>
        {
            var legend = scene.Page.Locator("fieldset legend").Filter(new() { HasTextString = "Super User" }).First;
            await scene.HighlightAsync(legend);
            await scene.HighlightAsync(scene.Page.Locator("fieldset legend + span, fieldset legend ~ span").First);
        });

        await scene.StepAsync("user-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#UserName"), "admin");
            await scene.TypeAsync(scene.Page.Locator("#Email"), "admin@example.com");
        });

        await scene.StepAsync("user-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#Password"));
            await scene.TypeAsync(scene.Page.Locator("#Password"), Password);
        });

        await scene.StepAsync("user-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#PasswordConfirmation"));
            await scene.TypeAsync(scene.Page.Locator("#PasswordConfirmation"), Password);
            await scene.MoveToAsync(scene.Page.Locator("#togglePasswordConfirmation"));
            await scene.HighlightAsync(scene.Page.Locator("#togglePassword"));
            await scene.HighlightAsync(scene.Page.Locator("#togglePasswordConfirmation"));
        });

        await scene.StepAsync("finish-1", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(scene.Page.Locator("#SubmitButton"));
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Locator("#SubmitButton").ClickAsync()));
            await scene.Page.Locator("a.navbar-brand").First.WaitForAsync();
        });

        await scene.StepAsync("finish-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a.navbar-brand").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SignInAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("signin-1", async () =>
        {
            await scene.NavigateAsync("/Admin");
            var heading = scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Log in" }).First;
            await heading.WaitForAsync();
            await scene.HighlightAsync(heading, "Your site address + /Admin");
        });

        await scene.StepAsync("signin-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.GetByLabel("Username or email address"), UserName);
            await scene.TypeAsync(scene.Page.GetByLabel("Password", new() { Exact = true }), Password);
            await scene.MoveToAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }));
        });

        await scene.StepAsync("signin-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }));
            await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();

            foreach (var title in new[] { "Create content", "Customize theme", "Manage users" })
            {
                await scene.HighlightAsync(scene.Page.Locator(".card-title").Filter(new() { HasTextString = title }).First);
                await scene.HoldAsync(1);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("settings-1", async () =>
        {
            var settings = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^\\s*Settings\\s*$") }).First;

            if (await settings.GetAttributeAsync("aria-expanded") == "false")
            {
                await scene.ClickAsync(settings, pauseAfter: 600);
            }

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^\\s*General\\s*$") }).First);
            await scene.Page.GetByLabel("Site name", new() { Exact = true }).WaitForAsync();
            await scene.HighlightAsync(scene.Page.GetByLabel("Site name", new() { Exact = true }));
            await scene.HighlightAsync(scene.Page.GetByLabel("Default Time Zone"));
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Tab, new() { Name = "Resources" }).First);
            var cdn = scene.Page.GetByLabel("Use framework CDN (Content Delivery Network)");
            await cdn.WaitForAsync();
            await scene.HighlightAsync(cdn);
        });

        await scene.ClearHighlightsAsync();
    }
}

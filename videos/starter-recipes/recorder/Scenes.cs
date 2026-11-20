using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Starter Recipes". Every clip is seen signed out: no state.json. 02-setup runs against a site that is
/// not set up; the other clips run against a site with four tenants (Default = SaaS, blog, agency, soon).
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
        ("02-setup", SetupAsync),
        ("04-blog", BlogAsync),
        ("05-agency", AgencyAsync),
        ("06-soon", SoonAsync),
        ("07-saas", SaasAsync),
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
        await scene.Page.Locator("#recipeButton").WaitForAsync();

        // The time zone of the recording machine is not shown.
        await scene.Page.Locator("#SiteTimeZone").SelectOptionAsync("America/Los_Angeles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await scene.PauseAsync(4000);
            await scene.MoveToAsync(scene.Page.Locator("#recipeButton"));
            await scene.PauseAsync(2500);
            await scene.ClickAsync(scene.Page.Locator("#recipeButton"), pauseAfter: 600);
        });

        await scene.StepAsync("setup-2", async () =>
        {
            foreach (var recipe in new[] { "Agency", "Blank", "Blog", "ComingSoon", "Headless", "SaaS" })
            {
                await scene.MoveToAsync(scene.Page.Locator($"#recipes .dropdown-item[data-recipe-name='{recipe}']").First);
                await scene.PauseAsync(700);
            }
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.PauseAsync(4300);
            await scene.ClickAsync(scene.Page.Locator("#recipes .dropdown-item[data-recipe-name='Blog']").First, pauseAfter: 600);
        });
    }

    private static async Task BlogAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/blog");
        await scene.Page.Locator(".post-preview").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("blog-1", async () =>
        {
            await scene.PauseAsync(5000);
            await scene.HighlightAsync(scene.Page.Locator(".site-heading").First);
        });

        await scene.StepAsync("blog-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var top = await scene.Page.EvaluateAsync<double>("document.querySelector('.post-preview').getBoundingClientRect().top");
            await scene.ScrollAsync(0, top - 140);
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator(".post-preview .post-meta").First);
        });

        await scene.StepAsync("blog-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var scrollY = await scene.Page.EvaluateAsync<double>("window.scrollY");
            await scene.ScrollAsync(0, -scrollY);
            await scene.PauseAsync(2500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#mainNav a:text-is('About')").First);
        });
    }

    private static async Task AgencyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/agency");
        await scene.Page.Locator("a.btn:text-is('Tell Me More')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("agency-1", async () =>
        {
            await scene.PauseAsync(7500);
            await scene.HighlightAsync(scene.Page.Locator("a.btn:text-is('Tell Me More')").First);
        });

        await scene.StepAsync("agency-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("a.btn:text-is('Tell Me More')").First, pauseAfter: 1800);
            await scene.PauseAsync(2000);
            var top = await scene.Page.EvaluateAsync<double>("document.querySelector('#portfolio').getBoundingClientRect().top");
            await scene.ScrollAsync(0, top - 90);
        });
    }

    private static async Task SoonAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/soon");
        await scene.Page.Locator("h1").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("soon-1", async () =>
        {
            await scene.PauseAsync(4500);

            foreach (var selector in new[] { "h1", ".masthead-content p", ".input-group-newsletter" })
            {
                await scene.HighlightAsync(scene.Page.Locator(selector).First);
                await scene.PauseAsync(1100);
                await scene.ClearHighlightsAsync();
            }

            foreach (var icon in await scene.Page.Locator(".social-icons a").AllAsync())
            {
                await scene.HighlightAsync(icon);
            }
        });

        await scene.StepAsync("soon-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator(".input-group-newsletter input").First, "visitor@example.com", delay: 55);
            await scene.MoveToAsync(scene.Page.Locator("#submit-button").First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator(".alert.alert-warning").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SaasAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator(".alert-primary").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("saas-1", async () =>
        {
            await scene.PauseAsync(3000);
            await scene.HighlightAsync(scene.Page.Locator(".alert-primary").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

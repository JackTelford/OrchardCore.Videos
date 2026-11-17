using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clip of the data-and-yessql video: see recording-plan.md.</summary>
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
        var page = scene.Page;
        var provider = page.Locator("#DatabaseProvider");

        // Outside the capture: the time zone, the site name, and a connection string that stays in the hidden field.
        await page.Locator("#SiteTimeZone").SelectOptionAsync("America/Los_Angeles");
        await page.Locator("#SiteName").FillAsync("Orchard Core");
        await provider.SelectOptionAsync(new SelectOptionValue { Label = "Sql Server" });
        await page.Locator("#ConnectionString").FillAsync("Server=db.example.com;Database=Orchard;Integrated Security=true");
        await provider.SelectOptionAsync(new SelectOptionValue { Label = "Sqlite" });
        await Assertions.Expect(page.Locator(".connectionString")).ToBeHiddenAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await scene.MoveToAsync(provider);
            await scene.HighlightAsync(provider, "Sqlite");
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(provider, "Sql Server");
            await page.Locator(".connectionString").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("#TablePrefix"), "Table Prefix");
            await scene.HoldAsync(0.8);
            await scene.HighlightAsync(page.Locator("#ConnectionString"), "Connection String");
            await scene.HoldAsync(0.8);
            await scene.HighlightAsync(page.Locator("#Schema"), "Table Schema");
            await scene.HoldAsync(0.8);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#connectionStringHint"));
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(page.Locator("#TablePrefix"));
            await scene.HoldAsync(2.5);
            await scene.MoveToAsync(page.Locator("#Schema"));
            await scene.HoldAsync(2.5);
            await scene.ClickAsync(page.Locator("#toggleConnectionString"), pauseAfter: 300);
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "YesSql Indexes": 06-tables runs two read-only SQL queries (ContentItemIndex and UserByRoleNameIndex)
/// in Search, Queries, Run SQL Query. It changes nothing on the site.
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
        ("06-tables", TablesAsync),
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

    private static async Task ShowResultAsync(Scene scene)
    {
        var page = scene.Page;
        await page.Locator("span.text-muted", new() { HasText = "Query executed in" }).First.WaitForAsync();
        await scene.ScrollAsync(0, 300);
        await scene.ScrollAsync(0, 350);
        await page.Locator("#monaco_editor .view-line").Nth(4).WaitForAsync();
        await scene.HighlightAsync(page.Locator("#monaco_editor").First);
    }

    private static async Task TablesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("tables-1", async () =>
        {
            await scene.ClickAsync(page.Locator("button.item-label.nav-group-label", new() { HasText = "Search" }).First, pauseAfter: 500);
            await scene.ClickAsync(page.Locator("ul[data-title='Search'] button.item-label", new() { HasText = "Queries" }).First, pauseAfter: 500);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("ul[data-title='Queries'] a.item-label", new() { HasText = "Run SQL Query" }).First);
            await page.Locator("label", new() { HasText = "Template" }).First.WaitForAsync();
        });

        await scene.StepAsync("tables-2", async () =>
        {
            var link = page.Locator("span.hint a", new() { HasText = "click here" }).First;
            await scene.HighlightAsync(link);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await link.ClickAsync();
                await page.Locator("span.text-muted", new() { HasText = "Query executed in" }).First.WaitForAsync();
            }));
        });

        await scene.StepAsync("tables-3", async () =>
        {
            await ShowResultAsync(scene);
        });

        await scene.StepAsync("tables-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Queries/Sql/Query?query=U0VMRUNUICogRlJPTSBVc2VyQnlSb2xlTmFtZUluZGV4");
            await ShowResultAsync(scene);
        });
    }
}

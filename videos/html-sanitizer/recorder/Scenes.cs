using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "HTML Sanitizer". Only 02-setting is recorded; it reads settings pages and saves nothing. No PrepareAsync.
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
        ("02-setting", SettingAsync),
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

    private static async Task ScrollToAsync(Scene scene, ILocator target)
    {
        await scene.ClearHighlightsAsync();
        await target.EvaluateAsync("e => e.scrollIntoView({ behavior: 'smooth', block: 'center' })");
        await scene.PauseAsync(900);
    }

    private static async Task SettingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.Page.Locator("a[role='btn-edit-Article']").First.WaitForAsync();
        await scene.StartCaptureAsync();

        var sanitize = scene.Page.Locator("div.form-check:has-text('Sanitize Html')").First;

        await scene.StepAsync("setting-1", async () =>
        {
            var edit = scene.Page.Locator("a[role='btn-edit-Article']").First;
            await scene.MoveToAsync(edit);
            await scene.HighlightAsync(edit);
            await scene.HoldAsync(0.7);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(edit), leadMilliseconds: 150);
            var row = scene.Page.Locator("#parts li:has(a[href$='/ContentTypes/Article/ContentParts/HtmlBodyPart/Edit'])").First;
            await row.WaitForAsync();
            await ScrollToAsync(scene, row);
            await scene.HighlightAsync(row);
            await scene.HoldAsync(0.7);
            await scene.ClearHighlightsAsync();
            var part = scene.Page.Locator("a[href$='/ContentTypes/Article/ContentParts/HtmlBodyPart/Edit']").First;
            await scene.MoveToAsync(part);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(part), leadMilliseconds: 150);
        });

        await scene.StepAsync("setting-2", async () =>
        {
            await sanitize.WaitForAsync();
            await ScrollToAsync(scene, sanitize);
            await scene.MoveToAsync(scene.Page.Locator("label.form-check-label:has-text('Sanitize Html')").First);
            await scene.HighlightAsync(sanitize);
        });

        await scene.StepAsync("setting-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/Admin/ContentTypes/BlogPost/ContentParts/MarkdownBodyPart/Edit");
                await sanitize.WaitForAsync();
            }, leadMilliseconds: 100);
            await ScrollToAsync(scene, sanitize);
            await scene.HighlightAsync(sanitize);
        });
    }
}

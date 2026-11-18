using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of placement-files: the Article editor before the startup project has a placement.json (05-before),
/// and the same editor after the site restarted with the file in place (05-after).
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
        ("05-before", BeforeAsync),
        ("05-after", AfterAsync),
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

    private const string CreatePage = "/Admin/Contents/ContentTypes/Article/Create";

    private static ILocator Row(Scene scene, string label)
        => scene.Page.Locator($".ocat-wrapper:has(> label.ocat-label:text-is(\"{label}\"))").First;

    private static async Task BeforeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var subtitle = scene.Page.Locator(".field-wrapper-article-subtitle").First;
        var banner = scene.Page.Locator(".field-wrapper-article-image").First;

        await scene.OpenAsync(CreatePage);
        await banner.WaitForAsync();
        try
        {
            await scene.Page.Locator(".trumbowyg-box").First.WaitForAsync(new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
        }

        await scene.Page.EvaluateAsync("window.scrollTo(0, 0)");
        await scene.PauseAsync(500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("before-1", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Title"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Permalink"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".html-body-part-wysiwyg-editor").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 700);
            await scene.PauseAsync(800);
            await scene.HighlightAsync(subtitle);
            await scene.HighlightAsync(banner);
        });

        await scene.StepAsync("before-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(subtitle, "Article-Subtitle");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(banner, "Article-Image");
        });
    }

    private static async Task AfterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var tabs = scene.Page.Locator(".edit-item-parts > ul.nav-tabs").First;
        var subtitle = scene.Page.Locator(".field-wrapper-article-subtitle").First;
        var banner = scene.Page.Locator(".field-wrapper-article-image").First;

        await scene.OpenAsync(CreatePage);
        await tabs.WaitForAsync();
        await subtitle.WaitForAsync();
        await scene.Page.EvaluateAsync("window.scrollTo(0, 0)");
        await scene.PauseAsync(500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("after-1", async () =>
        {
            await scene.HighlightAsync(tabs);
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(subtitle);
        });

        await scene.StepAsync("after-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".edit-item-parts > ul.nav-tabs a.nav-link:text-is(\"Media\")").First, pauseAfter: 900);
            await scene.HighlightAsync(banner);
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Content Definitions in Code": 07-seo enables the SEO feature (its migration creates the part
/// SeoMeta and the type Social Meta Settings) and shows both in the admin. The wait for the tenant to restart is
/// cut off camera.
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
        ("07-seo", SeoAsync),
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

    // Scrolls the element so that it sits in the upper part of the frame, as in the plan.
    private static async Task ScrollNearTopAsync(Scene scene, ILocator element, double top)
    {
        await element.WaitForAsync();
        var box = await element.BoundingBoxAsync();

        if (box is not null && box.Y > top + 40)
        {
            await scene.ScrollAsync(0, (float)(box.Y - top));
        }
    }

    private static async Task SeoAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("seo-1", async () =>
        {
            await FastAsync(page.Locator("#search-box"), "SEO");
            await page.Locator("#btn-enable-OrchardCore_Seo").WaitForAsync();
            await scene.MoveToAsync(page.Locator("#btn-enable-OrchardCore_Seo"));
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await page.Locator("#btn-enable-OrchardCore_Seo").ClickAsync();
                await page.Locator("#btn-disable-OrchardCore_Seo").WaitForAsync(new() { Timeout = 120_000 });
            }));
            await FastAsync(page.Locator("#search-box"), "SEO");
            await page.Locator("#btn-disable-OrchardCore_Seo").WaitForAsync();
        });

        await scene.StepAsync("seo-2", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/ListParts");
            var row = page.Locator("[data-type-name='SeoMetaPart']").First;
            await ScrollNearTopAsync(scene, row, 260);
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("seo-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#btn-edit-SeoMetaPart").First);
            var fields = page.Locator("#fields").First;
            await ScrollNearTopAsync(scene, fields, 220);
            await scene.HighlightAsync(fields);
        });

        await scene.StepAsync("seo-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/ContentTypes/List");
            var row = page.Locator("[data-type-name='SocialMetaSettings']").First;
            await ScrollNearTopAsync(scene, row, 260);
            await scene.HighlightAsync(row);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("[role='btn-edit-SocialMetaSettings']").First);
            await scene.PauseAsync(600);
            await ScrollNearTopAsync(scene, page.Locator("#fields").First, 220);
            await scene.PauseAsync(2200);
            await page.EvaluateAsync("() => window.scrollTo({ top: document.scrollingElement.scrollHeight, behavior: 'smooth' })");
            await scene.PauseAsync(1200);
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-page", PageAsync),
    ];

    // The clip reads public pages only: recorded signed out, so there is no state.json and nothing to seed.
    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "anonymous.json"), Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("page-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("a:has(h2.post-title:has-text(\"Man must explore\"))"));
            await page.Locator(".post-heading h1").WaitForAsync(new() { Timeout = 5000 });
        });

        await scene.StepAsync("page-2", async () => await scene.HighlightAsync(page.Locator("a.navbar-brand"), "Site.SiteName"));

        await scene.StepAsync("page-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".post-heading h1"));
            await scene.PauseAsync(4000);
            await scene.HighlightAsync(page.Locator(".post-heading h2.subheading"));
        });

        await scene.StepAsync("page-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var box = await page.Locator(".post-heading span.meta").BoundingBoxAsync();
            if (box is not null && box.Y + box.Height > 780)
            {
                await scene.ScrollAsync(0, (float)(box.Y + box.Height - 700));
            }

            await scene.HighlightAsync(page.Locator(".post-heading span.meta"), "t filter, Author");
        });

        await scene.ClearHighlightsAsync();
    }
}

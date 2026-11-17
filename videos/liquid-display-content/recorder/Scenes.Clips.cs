using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-page", PageAsync),
    ];

    // Nothing to seed: the clip reads the front end of the Blog recipe.
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task ScrollUntilVisibleAsync(Scene scene, ILocator locator, int delta = 400)
    {
        for (var i = 0; i < 8; i++)
        {
            var box = await locator.BoundingBoxAsync();
            if (box is not null && box.Y >= 0 && box.Y + box.Height <= 860)
            {
                return;
            }

            await scene.ScrollAsync(0, delta);
        }
    }

    // A read-only tour of the front end, recorded signed out.
    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "anonymous.json"), Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/");
        var page = scene.Page;
        await page.Locator("header.masthead h1").WaitForAsync();
        await page.Locator("div.post-preview").First.WaitForAsync(new() { State = WaitForSelectorState.Attached });
        await scene.StartCaptureAsync();

        await scene.StepAsync("page-1", async () => await scene.MoveToAsync(page.Locator("header.masthead h1")));

        await scene.StepAsync("page-2", async () => await scene.HighlightAsync(page.Locator("header.masthead"), "Header zone"));

        await scene.StepAsync("page-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ScrollUntilVisibleAsync(scene, page.Locator("div.post-preview").First);
            await scene.HighlightAsync(page.Locator("div.post-preview").First, "Summary display");
        });

        await scene.StepAsync("page-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ScrollUntilVisibleAsync(scene, page.Locator("footer.border-top").First);
            await scene.HighlightAsync(page.Locator("footer.border-top").First, "Footer zone");
        });

        await scene.StepAsync("page-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            var title = page.Locator("h2.post-title").First;
            await title.ScrollIntoViewIfNeededAsync();
            var (x, y) = await scene.CenterOfAsync(title);
            await scene.MoveToAsync(x, y, 8);
            await scene.WaitForNavigationAsync(async () => await page.Mouse.ClickAsync((float)x, (float)y));
            await page.Locator("header.masthead .post-heading h1").WaitForAsync();
            await scene.HighlightAsync(page.Locator("header.masthead"), "Header zone");
            await scene.PauseAsync(700);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 500, 6);
        });
    }
}

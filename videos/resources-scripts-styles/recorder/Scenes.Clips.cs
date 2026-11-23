using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("07-settings", SettingsAsync),
    ];

    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await page.Locator("figcaption#settings .nav-group-label").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("figcaption#settings .nav-group-label"), 300);
            await FastGoAsync(scene, page.Locator("li#general a.item-label"));
            await page.GetByRole(AriaRole.Tab, new() { Name = "Resources", Exact = true }).WaitForAsync();
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await FastClickAsync(scene, page.GetByRole(AriaRole.Tab, new() { Name = "Resources", Exact = true }));
            var heading = page.Locator("h5:has-text(\"Settings for scripts and stylesheets\")");
            await heading.WaitForAsync();
            await scene.HighlightAsync(heading);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Use resources cache busting").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Use framework CDN (Content Delivery Network)").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("settings-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Site CDN (Content Delivery Network) base url").Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper')][1]"));
        });

        await scene.StepAsync("settings-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Resource Debug Mode").Locator("xpath=ancestor::div[contains(@class,'ocat-limited-wrapper')][1]"));
        });

        await scene.StepAsync("settings-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            var save = page.GetByRole(AriaRole.Button, new() { Name = "Save" });
            if (!await save.IsVisibleAsync() || !await save.EvaluateAsync<bool>("e => { const r = e.getBoundingClientRect(); return r.bottom <= innerHeight && r.top >= 0; }"))
            {
                await save.ScrollIntoViewIfNeededAsync();
            }

            await scene.HighlightAsync(save);
        });
    }
}

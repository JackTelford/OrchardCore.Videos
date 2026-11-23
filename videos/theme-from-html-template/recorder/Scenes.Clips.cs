using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("05-zones", ZonesAsync),
        ("08-result", ResultAsync),
    ];

    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task OpenGroupAsync(Scene scene, string group, int pause = 300)
    {
        var button = scene.Page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\")), #left-nav button.item-label:has(span.title:text-is(\"{group}\"))").First;
        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await FastClickAsync(scene, button, pause);
        }
    }

    private static async Task ZonesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("zones-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings");
            await FastGoAsync(scene, page.Locator("#left-nav a:has(span.title:text-is(\"Zones\"))").First);
            var field = page.Locator("div.ocat-wrapper:has(label:has-text(\"Available zones for Layers\")) input[type=text]").First;
            await field.WaitForAsync();
            await scene.HighlightAsync(field);
        });

        await scene.StepAsync("zones-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenGroupAsync(scene, "Design");
            await FastGoAsync(scene, page.Locator("#left-nav a:has(span.title:text-is(\"Widgets\"))").First);
            await page.Locator("div.card-header:has-text(\"Content\")").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("div.card-header:has-text(\"Content\")").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("div.card-header:has-text(\"Footer\")").First);
        });

        await scene.StepAsync("zones-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/"));
            await page.Locator("footer.border-top").WaitForAsync();
            await scene.ScrollAsync(0, 3000, 10);
            await scene.HighlightAsync(page.GetByText("Copyright").First.Locator("xpath=ancestor::div[contains(@class,'container')][1]"));
        });
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("result-1", async () =>
        {
            await OpenGroupAsync(scene, "Design");
            await FastGoAsync(scene, page.Locator("#left-nav a:has(span.title:text-is(\"Themes\"))").First);
            await page.GetByRole(AriaRole.Heading, new() { Name = "Themes" }).First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("button:has-text(\"Make Current\")").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("span:has-text(\"This is the current Site theme\")").First);
        });

        await scene.StepAsync("result-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/"));
            await page.Locator(".post-preview").First.WaitForAsync();
            await scene.PauseAsync(1500);
            await scene.MoveToAsync(page.Locator("nav a:has-text(\"Home\")").First);
            await scene.MoveToAsync(page.Locator("nav a:has-text(\"About\")").First);
            await scene.ScrollAsync(0, 600, 10);
            await scene.ScrollAsync(0, 3000, 12);
        });

        await scene.StepAsync("result-3", async () =>
        {
            await scene.ScrollAsync(0, -3000, 12);
            await FastGoAsync(scene, page.Locator("h2.post-title").First);
            await page.Locator("header.masthead .post-heading").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".post-heading").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 420, 10);
        });
    }
}

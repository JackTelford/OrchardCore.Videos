using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
    ];

    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        var tools = page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))");
        if (await tools.GetAttributeAsync("aria-expanded") != "true")
        {
            await tools.ClickAsync();
            await page.WaitForTimeoutAsync(400);
        }

        var row = page.Locator("[data-filter-value*='CacheTicketStore']").First;
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#search-box"), "ticket", 60);
            await page.WaitForFunctionAsync("() => document.querySelectorAll('.feature-group [data-filter-value]:not(.d-none)').length === 1");
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(page.Locator(".feature-group:not(.d-none) h3").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var targets = new[]
            {
                row.Locator(".fw-semibold").First,
                row.Locator("code").First,
                row.Locator(".admin-list-secondary").First,
                row.Locator(".direct-dependencies .badge").First,
                row.Locator(".metadata .badge").First,
            };
            for (var i = 0; i < targets.Length; i++)
            {
                await scene.HighlightAsync(targets[i]);
                await scene.PauseAsync(1400);
                if (i < targets.Length - 1)
                {
                    await scene.ClearHighlightsAsync();
                }
            }
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = row.Locator("a.btn-primary").First;
            await scene.MoveToAsync(enable);
            await scene.HighlightAsync(enable);
        });
    }
}

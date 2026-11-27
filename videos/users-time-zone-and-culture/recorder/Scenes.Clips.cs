using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-prepare", PrepareClipAsync),
        ("03-edit", EditAsync),
        ("04-result", ResultAsync),
    ];

    private const string PostJson = "{\"steps\":[{\"name\":\"content\",\"Data\":[{\"ContentItemId\":\"4qm7x2k9r5t8c1v6b3n0d4w2ya\",\"ContentType\":\"BlogPost\",\"DisplayText\":\"A walk through the market\",\"Latest\":true,\"Published\":true,\"CreatedUtc\":\"2026-03-15T03:30:00Z\",\"ModifiedUtc\":\"2026-03-15T03:30:00Z\",\"PublishedUtc\":\"2026-03-15T03:30:00Z\",\"Author\":\"admin\",\"TitlePart\":{\"Title\":\"A walk through the market\"},\"AutoroutePart\":{\"Path\":\"blog/market-walk\"},\"MarkdownBodyPart\":{\"Markdown\":\"Fresh bread, ripe pears and a stall of spices. Saturday morning at the market is slow, loud and full of colour.\"},\"BlogPost\":{\"Subtitle\":{\"Text\":\"Saturday morning at the stalls\"}}}]}]}";

    // Seeds A (Localization), B (cultures) and C (the post).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await EnableFeaturesAsync(page, "OrchardCore.Localization");
        await Task.Delay(6000);
        await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"settings\",\"LocalizationSettings\":{\"DefaultCulture\":\"en-US\",\"SupportedCultures\":[\"en-US\",\"fr-FR\"]}},{\"name\":\"ReloadTenant\"}]}");
        await Task.Delay(6000);
        await ImportRecipeAsync(page, PostJson);
        await Task.Delay(3000);
        await page.GotoAsync(BaseUrl + "/blog/market-walk", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var text = await page.Locator("body").InnerTextAsync();
        Console.WriteLine("Post meta: " + (text.Contains("March 14, 2026") ? "March 14, 2026" : "NOT March 14"));
        await page.GotoAsync(BaseUrl + "/Admin/Settings/localization", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator(".localization-settings-wrapper table").WaitForAsync();
        Console.WriteLine("Cultures rows: " + await page.Locator(".localization-settings-wrapper table tbody tr").CountAsync());
        await context.CloseAsync();
    }

    private static async Task PrepareClipAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/blog/market-walk");
        var page = scene.Page;
        await scene.StartCaptureAsync();

        await scene.StepAsync("prepare-1", async () =>
            await scene.HighlightAsync(page.Locator(".post-heading .meta").First));

        await scene.StepAsync("prepare-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Admin/Features"));
            await page.Locator("#search-box").WaitForAsync();
            await FastTypeAsync(scene, page.Locator("#search-box"), "user time zone", 40);
            var row = page.Locator("[data-filter-value]").Filter(new() { HasText = "OrchardCore.Users.TimeZone" }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator("#btn-enable-OrchardCore_Users_TimeZone, [data-filter-value*=\"OrchardCore.Users.TimeZone\"] a:text-is(\"Enable\")").First, 0));
            await scene.OffCameraAsync(async () => await page.GetByText("The feature User Time Zone was enabled.").First.WaitForAsync());
        });

        await scene.StepAsync("prepare-3", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#search-box"), "user localization", 40);
            var row = page.Locator("[data-filter-value]").Filter(new() { HasText = "OrchardCore.Users.Localization" }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator("#btn-enable-OrchardCore_Users_Localization, [data-filter-value*=\"OrchardCore.Users.Localization\"] a:text-is(\"Enable\")").First, 0));
            await scene.OffCameraAsync(async () => await page.GetByText("The feature User Localization was enabled.").First.WaitForAsync());
        });

        await scene.StepAsync("prepare-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Admin/Settings/localization"));
            var table = page.Locator(".localization-settings-wrapper table").First;
            await table.WaitForAsync();
            await scene.HighlightAsync(table);
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("a.btn-success:has-text(\"Add culture\")").First);
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Users/Index");
        var page = scene.Page;
        await page.Locator("a.btn.btn-primary.btn-sm:has-text(\"Edit\")").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("edit-1", async () =>
        {
            var edit = page.Locator("a.btn.btn-primary.btn-sm:has-text(\"Edit\")").First;
            await scene.HighlightAsync(edit);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, edit);
        });

        await scene.StepAsync("edit-2", async () =>
        {
            var zone = page.GetByLabel("Default Time Zone");
            await zone.EvaluateAsync("e => e.scrollIntoView({ block: 'start' })");
            await page.WaitForTimeoutAsync(400);
            await scene.HighlightAsync(zone);
            await scene.HighlightAsync(page.GetByLabel("Default User Culture"));
        });

        await scene.StepAsync("edit-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var zone = page.GetByLabel("Default Time Zone");
            await scene.HighlightAsync(page.GetByText("Determines the time zone used when displaying and editing dates and times.").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(zone, "Europe/Paris", 300);
        });

        await scene.StepAsync("edit-4", async () =>
        {
            await scene.SelectAsync(page.GetByLabel("Default User Culture"), "fr-FR", 300);
            await scene.PauseAsync(5500);
            await FastGoAsync(scene, page.Locator("button.save").First);
        });
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/blog/market-walk");
        var page = scene.Page;
        await scene.StartCaptureAsync();

        await scene.StepAsync("result-1", async () =>
            await scene.HighlightAsync(page.Locator(".post-heading .meta").First));
    }
}

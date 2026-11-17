using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-record", RecordAsync),
        ("03-history", HistoryAsync),
        ("04-view", ViewAsync),
        ("05-compare", CompareAsync),
        ("06-restore", RestoreAsync),
        ("07-deleted", DeletedAsync),
    ];

    // OCV_SEED=A: enable Audit Trail. OCV_SEED=B: the articles, after Article was allowed.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var seed = Environment.GetEnvironmentVariable("OCV_SEED") ?? "A";
        var (context, page) = await SeedPageAsync(browser);
        if (seed == "A")
        {
            await EnableFeaturesAsync(page, "OrchardCore.AuditTrail");
            await Task.Delay(6000);
            await context.CloseAsync();
            return;
        }

        await CreateArticleAsync(page, "Summer Parking Notice", "Lot B closes on Friday");
        await CreateArticleAsync(page, "Harvest Festival Schedule", "Saturday, 10 AM to 4 PM");
        await EditArticleAsync(page, "Harvest Festival Schedule", null, "Saturday, 9 AM to 5 PM");
        await EditArticleAsync(page, "Harvest Festival Schedule", "Harvest Festival Cancelled", "Postponed to a later date");
        await page.GotoAsync(BaseUrl + "/Admin/AuditTrail/" + await ArticleIdAsync(page, "Harvest Festival Cancelled"), new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Lines: " + string.Join(" || ", await page.Locator("ul.admin-list-list > li").AllInnerTextsAsync()));
        await context.CloseAsync();
    }

    private static async Task CreateArticleAsync(IPage page, string title, string subtitle)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Article/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[name$=\"TitlePart.Title\"]").First.FillAsync(title);
        await page.GetByLabel("Subtitle").First.FillAsync(subtitle);
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
        await Task.Delay(1500);
    }

    private static async Task EditArticleAsync(IPage page, string current, string newTitle, string subtitle)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems?q=" + Uri.EscapeDataString(current), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator($"li.list-group-item:has-text(\"{current}\") a.edit").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        if (newTitle is not null)
        {
            await page.Locator("input[name$=\"TitlePart.Title\"]").First.FillAsync(newTitle);
        }

        await page.GetByLabel("Subtitle").First.FillAsync(subtitle);
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
        await Task.Delay(1500);
    }

    private static async Task<string> ArticleIdAsync(IPage page, string title)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems?q=" + Uri.EscapeDataString(title), new() { WaitUntil = WaitUntilState.NetworkIdle });
        var href = await page.Locator($"li.list-group-item:has-text(\"{title}\") a.edit").First.GetAttributeAsync("href");
        return href.Split('/')[4];
    }

    private static ILocator Line(IPage page, int index) => page.Locator("ul.admin-list-list > li").Nth(index);

    private static async Task<IPage> OpenAuditAsync(Scene scene)
    {
        var page = scene.Page;
        var id = await ArticleIdAsync(page, "Harvest Festival Cancelled");
        await scene.OpenAsync("/Admin/AuditTrail/" + id);
        await Line(page, 1).WaitForAsync();
        return page;
    }

    private static async Task RecordAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("record-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Settings\"))"), 700);
            await FastGoAsync(scene, page.Locator("#left-nav").GetByText("Audit Trail", new() { Exact = true }).Locator("visible=true").First);
            await FastClickAsync(scene, page.Locator("ul.nav-tabs a.nav-link:text-is(\"Content\"), ul.nav-tabs button.nav-link:text-is(\"Content\")").First, 500);
            await scene.HighlightAsync(page.GetByText("Allowed content types").First);
        });

        await scene.StepAsync("record-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator(".content-types label.form-check-label:text-is(\"Article\"), label.form-check-label:text-is(\"Article\")").First, 400);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First));
            await FastClickAsync(scene, page.Locator("ul.nav-tabs a.nav-link:text-is(\"Content\"), ul.nav-tabs button.nav-link:text-is(\"Content\")").First, 400);
        });
    }

    private static async Task HistoryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        var page = scene.Page;
        await page.Locator("li.list-group-item").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("history-1", async () =>
        {
            var row = page.Locator("li.list-group-item:has-text(\"Harvest Festival Cancelled\")").First;
            await FastClickAsync(scene, row.Locator("button.dropdown-toggle.actions").First, 600);
            var entry = row.Locator("a.dropdown-item:text-is(\"Audit Trail\")").First;
            await scene.HighlightAsync(entry);
            await scene.PauseAsync(800);
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, entry, 0));
        });

        await scene.StepAsync("history-2", async () =>
        {
            await Line(page, 1).WaitForAsync();
            foreach (var badge in await Line(page, 1).Locator("div.summary > *, .badge").AllAsync())
            {
                if (await badge.IsVisibleAsync())
                {
                    await scene.HighlightAsync(badge);
                    await scene.PauseAsync(700);
                    await scene.ClearHighlightsAsync();
                }
            }

            await scene.HighlightAsync(Line(page, 1));
        });
    }

    private static async Task ViewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = await OpenAuditAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("view-1", async () =>
        {
            await scene.HighlightAsync(Line(page, 2));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, Line(page, 2).Locator("a.btn:text-is(\"View\")").First);
        });

        await scene.StepAsync("view-2", async () =>
        {
            await page.Locator("input[name$=\"TitlePart.Title\"]").First.WaitForAsync();
            await scene.MoveToAsync(page.Locator("input[name$=\"TitlePart.Title\"]").First);
            await scene.PauseAsync(1500);
            await scene.MoveToAsync(page.GetByLabel("Subtitle").First);
        });
    }

    private static async Task CompareAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = await OpenAuditAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("compare-1", async () =>
        {
            await FastGoAsync(scene, Line(page, 1).Locator("a.btn:text-is(\"Details\")").First);
            await page.GetByText("Event Info").First.WaitForAsync();
            await scene.MoveToAsync(page.Locator("table").Nth(1));
        });

        await scene.StepAsync("compare-2", async () =>
        {
            await FastClickAsync(scene, page.Locator("ul.nav-tabs a.nav-link:text-is(\"Diff\"), ul.nav-tabs button.nav-link:text-is(\"Diff\")").First, 400);
            await page.Locator("#diffapp .diff-viewer").First.WaitForAsync();
            await page.WaitForTimeoutAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 200, 8);
        });

        await scene.StepAsync("compare-3", async () =>
        {
            var unified = page.Locator(".diff-viewer label.btn:text-is(\"Unified\")").First;
            await unified.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await page.WaitForTimeoutAsync(300);
            await FastClickAsync(scene, unified, 600);
        });
    }

    private static async Task RestoreAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = await OpenAuditAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("restore-1", async () =>
        {
            await scene.MoveToAsync(Line(page, 1));
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(Line(page, 2));
            await FastClickAsync(scene, Line(page, 2).Locator("a.btn:text-is(\"Restore\")").First, 300);
            await page.Locator("#modalOkButton").WaitForAsync();
        });

        await scene.StepAsync("restore-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#confirmRemoveModal .modal-body, .modal.show .modal-body").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("restore-3", async () =>
        {
            var message = page.GetByText("has been restored.").First;
            if (await message.IsVisibleAsync())
            {
                await scene.HighlightAsync(message);
                await scene.PauseAsync(1500);
                await scene.ClearHighlightsAsync();
            }

            await scene.HighlightAsync(Line(page, 1));
        });

        await scene.StepAsync("restore-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Admin/Contents/ContentItems"));
            var row = page.Locator("li.list-group-item:has-text(\"Harvest Festival Schedule\")").First;
            await scene.HighlightAsync(row.Locator(".badge:has-text(\"Published\")").First);
            await scene.HighlightAsync(row.Locator(".badge:has-text(\"Draft\")").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, row.Locator("button.dropdown-toggle.actions").First, 600);
            var publish = row.Locator("a.dropdown-item:has-text(\"Publish Draft\")").First;
            await scene.HighlightAsync(publish);
            await scene.PauseAsync(600);
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, publish, 0));
        });
    }

    private static async Task DeletedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        var page = scene.Page;
        await page.Locator("li.list-group-item:has-text(\"Summer Parking Notice\")").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("deleted-1", async () =>
        {
            var row = page.Locator("li.list-group-item:has-text(\"Summer Parking Notice\")").First;
            await FastClickAsync(scene, row.Locator("button.dropdown-toggle.actions").First, 600);
            var delete = row.Locator("a.dropdown-item:has-text(\"Delete\")").First;
            await scene.HighlightAsync(delete);
            await scene.PauseAsync(600);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, delete, 300);
            await page.Locator("#modalOkButton").WaitForAsync();
            await scene.PauseAsync(1200);
            await FastGoAsync(scene, page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("deleted-2", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))"), 600);
            await FastGoAsync(scene, page.Locator("#left-nav").GetByText("Audit Trail", new() { Exact = true }).Locator("visible=true").First);
            await Line(page, 1).WaitForAsync();
            await scene.HighlightAsync(Line(page, 1));
        });

        await scene.StepAsync("deleted-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, Line(page, 1).Locator("a.btn:text-is(\"Restore\")").First, 300);
            await page.Locator("#modalOkButton").WaitForAsync();
            await scene.PauseAsync(1500);
            await FastGoAsync(scene, page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("deleted-4", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Admin/Contents/ContentItems"));
            var row = page.Locator("li.list-group-item:has-text(\"Summer Parking Notice\")").First;
            await scene.HighlightAsync(row.Locator(".badge:has-text(\"Draft\")").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, row.Locator("button.dropdown-toggle.actions").First, 600);
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, row.Locator("a.dropdown-item:has-text(\"Publish Draft\")").First, 0));
        });
    }
}

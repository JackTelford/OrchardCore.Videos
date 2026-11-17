using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-settings", SettingsAsync),
        ("04-change", ChangeAsync),
        ("05-list", ListAsync),
        ("06-details", DetailsAsync),
        ("07-item", ItemAsync),
    ];

    // OCV_SEED=A: Jamie Chen, a second administrator. OCV_SEED=B: Jamie's Article, created while signed in as Jamie.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var seed = Environment.GetEnvironmentVariable("OCV_SEED") ?? "A";
        if (seed == "A")
        {
            var (context, page) = await SeedPageAsync(browser);
            await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"command\",\"Commands\":[\"createUser /UserName:jamie.chen /Password:" + Password + " /Email:jamie.chen@example.com /Roles:Administrator\"]}]}");
            Console.WriteLine("Seeded Jamie.");
            await context.CloseAsync();
            return;
        }

        var visitor = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var p = await visitor.NewPageAsync();
        await p.GotoAsync(BaseUrl + "/Login", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await p.Locator("#LoginForm_UserName").FillAsync("jamie.chen");
        await p.Locator("#LoginForm_Password").FillAsync(Password);
        await p.RunAndWaitForNavigationAsync(async () => await p.Locator("button[type=submit]").First.ClickAsync());
        await p.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Article/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await p.Locator("input[name$=\"TitlePart.Title\"]").First.FillAsync("Pruning workshop");
        await p.RunAndWaitForNavigationAsync(async () => await p.Locator("button.publish").First.ClickAsync());
        Console.WriteLine("Seeded Article as Jamie: " + p.Url);
        await visitor.CloseAsync();
    }

    private static async Task OpenGroupAsync(IPage page, string group)
    {
        var button = page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\"))");
        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await button.ClickAsync();
        }
    }

    private static ILocator AuditEntry(IPage page) => page.Locator("#left-nav").GetByText("Audit Trail", new() { Exact = true }).Locator("visible=true");

    private static ILocator Row(IPage page, int index) => page.Locator("ul.admin-list-list > li").Nth(index);

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))"), 300);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))"));
            await FastTypeAsync(scene, page.Locator("#search-box"), "Audit Trail", 40);
            var row = page.Locator("li.list-group-item").Filter(new() { HasText = "Provides a log for recording and viewing back-end changes." }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("a[href*=\"Features/OrchardCore.AuditTrail/Enable\"]").First;
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, enable, 0));
            await scene.OffCameraAsync(async () => await page.GetByText("The feature Audit Trail was enabled.").First.WaitForAsync());
        });

        await scene.StepAsync("enable-3", async () =>
        {
            var tools = AuditEntry(page).First;
            if (!await tools.IsVisibleAsync())
            {
                await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))"), 300);
            }

            await AuditEntry(page).First.WaitForAsync();
            Console.WriteLine("Tools entry count: " + await AuditEntry(page).CountAsync());
            await scene.HighlightAsync(AuditEntry(page).First, "Tools");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Settings\"))"), 900);
            await AuditEntry(page).First.WaitForAsync();
            await scene.HighlightAsync(AuditEntry(page).First, "Settings");
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Settings\"))"), 300);
            await FastGoAsync(scene, AuditEntry(page).First);
            await page.Locator("th:has-text(\"Enabled\")").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("th:has-text(\"Enabled\")").First);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.GetByRole(AriaRole.Tab, new() { Name = "Trimming" }));
            await scene.HighlightAsync(page.GetByLabel("Retention period"));
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.GetByRole(AriaRole.Tab, new() { Name = "Content", Exact = true }));
            await page.GetByText("Allowed content types").First.WaitForAsync();
            await FastClickAsync(scene, page.Locator("label.form-check-label:text-is(\"Article\")").First);
        });

        await scene.StepAsync("settings-4", async () =>
            await FastGoAsync(scene, page.Locator("button.save").First));
    }

    private static async Task ChangeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("change-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Content\"))"), 300);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Content Items\"))").First);
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Admin/Contents/ContentItems?q=Pruning"));
            await FastGoAsync(scene, page.Locator("li.list-group-item:has-text(\"Pruning workshop\") a.edit").First);
        });

        await scene.StepAsync("change-2", async () =>
        {
            var title = page.Locator("input[name$=\"TitlePart.Title\"]").First;
            await FastClickAsync(scene, title, 100);
            await page.Keyboard.PressAsync("Control+A");
            await title.PressSequentiallyAsync("Pruning workshop for beginners", new() { Delay = 25 });
            await FastGoAsync(scene, page.Locator("button[name=\"submit.Publish\"][value=\"submit.Publish\"]").First);
        });

        await scene.StepAsync("change-3", async () =>
            await page.GetByText("Your Article has been published.").First.WaitForAsync());
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))"), 300);
            await FastGoAsync(scene, AuditEntry(page).First);
            await Row(page, 1).WaitForAsync();
            await scene.HighlightAsync(Row(page, 1).Locator("div.summary").First);
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, 1).Locator("small").First);
            var version = Row(page, 1).Locator("a:has-text(\"Version 2\")").First;
            if (await version.CountAsync() > 0)
            {
                await scene.MoveToAsync(version);
            }

            await scene.PauseAsync(1200);
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, 1).Locator("div.actions").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, 2).Locator("a:text-is(\"Restore\")").First);
        });

        await scene.StepAsync("list-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            foreach (var title in new[] { "Date", "Category", "Sort" })
            {
                await FastClickAsync(scene, page.Locator($"button.dropdown-toggle[title=\"{title}\"]").First, 900);
                await page.Keyboard.PressAsync("Escape");
            }

            await FastClickAsync(scene, page.Locator("button.dropdown-toggle[title=\"Date\"]").First, 100);
        });

        await scene.StepAsync("list-5", async () =>
        {
            await page.Keyboard.PressAsync("Escape");
            await FastTypeAsync(scene, page.Locator("input[type=search]").First, "jamie", 60);
            await scene.WaitForNavigationAsync(async () => await page.Keyboard.PressAsync("Enter"));
            await FastClickAsync(scene, page.Locator("#filter-dropdown"), 100);
        });
    }

    private static async Task DetailsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/AuditTrail");
        var page = scene.Page;
        await Row(page, 1).WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("details-1", async () =>
        {
            await FastGoAsync(scene, Row(page, 1).Locator("a:text-is(\"Details\")").First);
            await page.GetByText("Event Info").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("table").First);
        });

        await scene.StepAsync("details-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var table = page.Locator("table").Nth(1);
            await table.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(table);
        });

        await scene.StepAsync("details-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, -300);
            await scene.HighlightAsync(page.Locator("a.nav-link:text-is(\"Diff\"), button.nav-link:text-is(\"Diff\")").First);
        });
    }

    private static async Task ItemAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("item-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Content\"))"), 300);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Content Items\"))").First);
            var row = page.Locator("li.list-group-item:has-text(\"Pruning workshop for beginners\")").First;
            await FastClickAsync(scene, row.Locator("button.actions").First, 800);
            await FastGoAsync(scene, row.Locator("a.dropdown-item:text-is(\"Audit Trail\")").First);
        });

        await scene.StepAsync("item-2", async () =>
        {
            var events = page.Locator("button.dropdown-toggle[title=\"Events\"]").First;
            await scene.HighlightAsync(events);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, events, 1000);
            await page.Keyboard.PressAsync("Escape");
        });
    }
}

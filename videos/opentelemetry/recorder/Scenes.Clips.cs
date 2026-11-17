using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-browse", BrowseAsync),
        ("04-traces", TracesAsync),
        ("05-metrics", MetricsAsync),
        ("06-logs", LogsAsync),
    ];

    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task BrowseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/marketing");
        var page = scene.Page;
        await scene.StartCaptureAsync();

        await scene.StepAsync("browse-1", async () =>
        {
            await scene.NavigateAsync("/");
            await scene.HighlightAsync(page.Locator("a.navbar-brand").First, "Default tenant");
        });

        await scene.StepAsync("browse-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/marketing");
            await scene.HighlightAsync(page.Locator("a.navbar-brand").First, "Marketing tenant");
        });

        await scene.ClearHighlightsAsync();
    }

    // Opens a page of the Aspire dashboard before the capture: the dashboard keeps a connection open, so the network never idles.
    private static async Task<Scene> OpenDashboardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, string ready, int count = 1)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        page.SetDefaultTimeout(60_000);
        await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator(ready).Nth(count - 1).WaitForAsync(new() { Timeout = 60_000 });
        await page.Locator("fluent-button[aria-label='Dismiss notification']").First.ClickAsync();
        await page.Mouse.MoveAsync(960, 540);
        await page.WaitForTimeoutAsync(800);
        return scene;
    }

    // The rows of data of a table (the header row is a row too).
    private static ILocator Rows(IPage page) => page.Locator("tbody [role=row]").Filter(new() { Has = page.Locator("[role=gridcell]") });

    private static ILocator Cell(IPage page, string text) => page.Locator($"td:has(span:text-is(\"{text}\")), td:text-is(\"{text}\")").First;

    // The value cell that follows the name of an attribute in a table of attributes.
    private static ILocator ValueOf(IPage page, string text) => page.Locator($"td:has(span:text-is(\"{text}\")) + td, td:text-is(\"{text}\") + td").First;

    private static async Task RingAttributeAsync(Scene scene, string name, string caption = null)
    {
        await scene.HighlightAsync(Cell(scene.Page, name), caption);
        await scene.HighlightAsync(ValueOf(scene.Page, name));
    }

    // The table of traces opens scrolled to its last rows: show the first ones.
    private static Task ScrollTopAsync(IPage page)
        => page.EvaluateAsync("() => document.querySelectorAll('*').forEach(e => { if (e.scrollTop) { e.scrollTop = 0; } })");

    // Clicks a button of a row of the table without scrolling it under the sticky header: the table is first scrolled to the top.
    private static async Task ClickRowButtonAsync(Scene scene, ILocator button, int pauseAfter)
    {
        var page = scene.Page;

        for (var attempt = 0; attempt < 12; attempt++)
        {
            await button.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await page.WaitForTimeoutAsync(150);
            var box = await button.BoundingBoxAsync();

            if (box is { Y: > 122 and < 690 })
            {
                var (x, y) = (box.X + box.Width / 2, box.Y + box.Height / 2);
                await scene.MoveToAsync(x, y, 10);
                await Task.Delay(120);
                await page.Mouse.ClickAsync((float)x, (float)y);
                await Task.Delay(pauseAfter);
                return;
            }
        }

        var last = await button.BoundingBoxAsync();
        throw new InvalidOperationException($"{button} is not in view: {last?.Y}.");
    }

    // Opens the Actions menu of a row and clicks an item, trying again if the menu did not open.
    private static async Task OpenMenuItemAsync(Scene scene, ILocator row, string item)
    {
        var page = scene.Page;
        var menuItem = page.GetByRole(AriaRole.Menuitem, new() { Name = item });

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await ClickRowButtonAsync(scene, row.Locator("fluent-button").First, 300);

            try
            {
                await menuItem.WaitForAsync(new() { Timeout = 4000 });
                break;
            }
            catch (TimeoutException) when (attempt < 2)
            {
            }
        }

        await FastClickAsync(scene, menuItem, 0);
    }

    // Opens the trace of a row through its Actions menu.
    private static async Task OpenTraceAsync(Scene scene, ILocator row)
    {
        var page = scene.Page;
        await OpenMenuItemAsync(scene, row, "View details");
        await page.GetByText("Total spans").First.WaitForAsync();

        await scene.PauseAsync(250);
    }

    private static async Task BackToTracesAsync(Scene scene)
    {
        var page = scene.Page;
        await page.GoBackAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Rows(page).Nth(3).WaitForAsync();
        await ScrollTopAsync(page);
        await scene.PauseAsync(250);
    }

    // Chooses an option of a drop-down list of the filter panel.
    private static async Task PickAsync(Scene scene, string name, string option)
    {
        var page = scene.Page;
        var combo = page.GetByRole(AriaRole.Combobox, new() { Name = name });
        var item = page.Locator($"fluent-option[text='{option}']").First;

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await combo.WaitForAsync();
            await FastClickAsync(scene, combo, 600);

            try
            {
                await item.WaitForAsync(new() { Timeout = 3000 });
                break;
            }
            catch (TimeoutException) when (attempt < 2)
            {
            }
        }
        await FastClickAsync(scene, item, 400);
    }

    private static async Task TracesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations, "/traces", "tbody [role=row]:has([role=gridcell])", 9);
        var page = scene.Page;
        await Rows(page).Nth(8).WaitForAsync();
        await ScrollTopAsync(page);
        await page.WaitForTimeoutAsync(500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("traces-1", async () =>
        {
            await scene.MoveToAsync(Rows(page).Nth(2).Locator("td").Nth(1));
        });

        await scene.StepAsync("traces-2", async () =>
        {
            await FastClickAsync(scene, page.GetByLabel("Add filter").First, 1500);
            await page.GetByText("Apply filter", new() { Exact = true }).WaitForAsync();
            await scene.PauseAsync(600);
            await PickAsync(scene, "Parameter", "orchardcore.tenant.name");
            await PickAsync(scene, "Condition", "==");
            await PickAsync(scene, "Value", "Marketing");
            await FastClickAsync(scene, page.GetByText("Apply filter", new() { Exact = true }), 0);
            await page.WaitForFunctionAsync("() => location.search.includes('filters=')", null, new() { Timeout = 15_000 });
            await Rows(page).Nth(3).WaitForAsync();
            await page.WaitForTimeoutAsync(800);
            await ScrollTopAsync(page);
            await scene.MoveToAsync(Rows(page).Nth(3).Locator("td").Nth(1));
        });

        await scene.StepAsync("traces-3", async () =>
        {
            await OpenTraceAsync(scene, Rows(page).Nth(1));
            await FastClickAsync(scene, page.GetByText("GET", new() { Exact = true }).First, 300);
            await Cell(page, "orchardcore.tenant.name").WaitForAsync();
            await RingAttributeAsync(scene, "orchardcore.tenant.name", "Marketing");
        });

        await scene.StepAsync("traces-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await BackToTracesAsync(scene);
            await OpenTraceAsync(scene, Rows(page).First);
            var recipe = page.GetByText("Execute recipe Blog", new() { Exact = true }).First;
            await recipe.WaitForAsync();
            await scene.ScrollAsync(0, 120, 6);
            await FastClickAsync(scene, recipe, 300);
            await Cell(page, "orchardcore.recipe.name").WaitForAsync();
            await RingAttributeAsync(scene, "orchardcore.recipe.name");
        });

        await scene.StepAsync("traces-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await BackToTracesAsync(scene);
            await OpenTraceAsync(scene, Rows(page).Nth(2));
            await FastClickAsync(scene, page.GetByText("Render shape Layout", new() { Exact = true }).First, 300);
            await Cell(page, "orchardcore.shape.type").WaitForAsync();
            await RingAttributeAsync(scene, "orchardcore.shape.type");
            await RingAttributeAsync(scene, "orchardcore.shape.binding");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task MetricsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations, "/metrics", "fluent-tree-item[title='http.server.request.duration']");
        var page = scene.Page;

        // The first chart of a page takes a while to draw: draw one off camera, then move to another instrument.
        await page.Locator("fluent-tree-item[title='http.server.request.duration']").ClickAsync();
        await Cell(page, "orchardcore.tenant.name").WaitForAsync();
        await page.WaitForTimeoutAsync(1500);
        await page.Locator("fluent-tree-item[title='kestrel.active_connections']").ClickAsync();
        await page.WaitForTimeoutAsync(2500);
        await page.Mouse.MoveAsync(960, 540);
        await scene.StartCaptureAsync();

        await scene.StepAsync("metrics-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("fluent-tree-item[title='http.server.request.duration']"), 600);
            await Cell(page, "orchardcore.tenant.name").WaitForAsync();
            await page.WaitForTimeoutAsync(1500);
            await RingAttributeAsync(scene, "orchardcore.tenant.name");
        });

        await scene.StepAsync("metrics-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("fluent-tree-item[title='orchardcore.tenants']"), 600);
            await scene.HighlightAsync(page.Locator("fluent-tree-item[title='orchardcore.tenant.container.duration']"));
            await scene.HighlightAsync(page.Locator("fluent-tree-item[title='orchardcore.tenant.activation.duration']"));
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("fluent-tree-item[title='dotnet.process.memory.working_set']"), 600);
            await page.WaitForTimeoutAsync(1500);
        });
    }

    private static async Task LogsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations, "/structuredlogs?filters=orchardcore.tenant.name%3Aequals%3AMarketing", "tbody [role=row]:has([role=gridcell])", 12);
        var page = scene.Page;
        await page.WaitForTimeoutAsync(1500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("logs-1", async () =>
        {
            // The table follows the newest records: take one of the rows on screen.
            var row = Rows(page).Nth(await Rows(page).CountAsync() - 6);
            // The first button of a row of the logs opens the details of the record.
            await ClickRowButtonAsync(scene, row.Locator("fluent-button").First, 300);
            await Cell(page, "orchardcore.tenant.name").WaitForAsync();
            await scene.PauseAsync(800);
            await RingAttributeAsync(scene, "orchardcore.tenant.name");
        });

        await scene.StepAsync("logs-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Close" }).First, 500);
            await scene.MoveToAsync(960, 500);
        });
    }
}

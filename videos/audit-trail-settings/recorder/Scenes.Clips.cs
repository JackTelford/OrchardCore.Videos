using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-events", EventsAsync),
        ("03-trimming", TrimmingAsync),
        ("04-users", UsersAsync),
        ("05-result", ResultAsync),
    ];

    // OCV_SEED=A: enable Audit Trail and Users Audit Trail. OCV_SEED=C: a failed sign-in and the Page Garden Club Meeting.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var seed = Environment.GetEnvironmentVariable("OCV_SEED") ?? "A";
        if (seed == "A")
        {
            var (context, page) = await SeedPageAsync(browser);
            await EnableFeaturesAsync(page, "OrchardCore.AuditTrail", "OrchardCore.Users.AuditTrail");
            await Task.Delay(6000);
            await context.CloseAsync();
            return;
        }

        var visitor = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var p = await visitor.NewPageAsync();
        await p.GotoAsync(BaseUrl + "/Login", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await p.Locator("#LoginForm_UserName").FillAsync("guest.visitor");
        await p.Locator("#LoginForm_Password").FillAsync("not-a-password");
        await p.RunAndWaitForNavigationAsync(async () => await p.Locator("button[type=submit]").First.ClickAsync());
        Console.WriteLine("Failed login: " + (await p.Locator("body").InnerTextAsync()).Contains("Invalid login attempt"));
        await visitor.CloseAsync();

        var (context2, page2) = await SeedPageAsync(browser);
        await page2.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Page/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page2.Locator("input[name$=\"TitlePart.Title\"]").First.FillAsync("Garden Club Meeting");
        await page2.RunAndWaitForNavigationAsync(async () => await page2.Locator("button.publish").First.ClickAsync());
        await context2.CloseAsync();
    }

    private static ILocator Menu(IPage page, string text) => page.Locator("#left-nav").GetByText(text, new() { Exact = true }).Locator("visible=true").First;

    private static async Task EventsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("events-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Settings\"))"), 700);
            await FastGoAsync(scene, Menu(page, "Audit Trail"));
            await page.Locator("th:has-text(\"Description\")").First.WaitForAsync();
        });

        await scene.StepAsync("events-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("tr.table-secondary:has(strong:text-is(\"Content\"))").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            var user = page.Locator("tr.table-secondary:has(strong:text-is(\"User\"))").First;
            await user.EvaluateAsync("e => e.scrollIntoView({ block: 'start' })");
            await scene.HighlightAsync(user);
        });

        await scene.StepAsync("events-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var user = page.Locator("tbody.check-all-container:has(tr.table-secondary strong:text-is(\"User\"))").First;
            await scene.HighlightAsync(user.GetByLabel("Select all"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, user.GetByLabel("Login failed", new() { Exact = true }), 900);
        });

        await scene.StepAsync("events-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var ip = page.GetByLabel("Allow client IP address");
            await ip.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await FastClickAsync(scene, ip, 600);
            await scene.HighlightAsync(ip.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("events-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await FastClickAsync(scene, page.Locator("ul.nav-tabs a.nav-link:text-is(\"Content\"), ul.nav-tabs button.nav-link:text-is(\"Content\")").First, 700);
        });

        await scene.StepAsync("events-6", async () =>
        {
            var pageBox = page.Locator("div.tab-pane.active").GetByLabel("Page", new() { Exact = true });
            await FastClickAsync(scene, pageBox, 700);
            await scene.HighlightAsync(pageBox.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("events-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First));
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        });
    }

    private static async Task TrimmingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/AuditTrail");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("trim-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("ul.nav-tabs a.nav-link:text-is(\"Trimming\"), ul.nav-tabs button.nav-link:text-is(\"Trimming\")").First, 600);
            await scene.HighlightAsync(page.GetByLabel("Retention period"));
        });

        await scene.StepAsync("trim-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("div.ocat-wrapper:has(label:text-is(\"Last run\")) div.form-control").First);
        });

        await scene.StepAsync("trim-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var days = page.GetByLabel("Retention period");
            await FastClickAsync(scene, days, 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("90", new KeyboardTypeOptions { Delay = 80 });
        });

        await scene.StepAsync("trim-4", async () =>
        {
            await scene.HighlightAsync(page.GetByLabel("Disable trimming task").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First));
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        });
    }

    private static async Task UsersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("users-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Settings\"))"), 600);
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Security\")), #left-nav button.item-label:has(span.title:text-is(\"Security\"))").First, 600);
            await FastGoAsync(scene, page.Locator("#left-nav a[href*=\"/Admin/Users/AuditTrail\"]").First);
        });

        await scene.StepAsync("users-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("select.form-select").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300, 20);
        });

        await scene.StepAsync("users-3", async () =>
        {
            var email = page.GetByLabel("Email", new() { Exact = true });
            await email.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await scene.SelectAsync(email, "PartialAsteriskRedactor", 300);
            await scene.PauseAsync(1000);
            await FastGoAsync(scene, page.Locator("input[type=submit][value=Save], button.save").First);
            await page.GetByLabel("Email", new() { Exact = true }).EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
        });

        await scene.StepAsync("users-4", async () =>
        {
            await page.EvaluateAsync("() => window.scrollTo(0, document.body.scrollHeight)");
            await scene.HighlightAsync(page.Locator("span.hint").First);
        });

        await scene.StepAsync("users-5", async () => await scene.PauseAsync(500));
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
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))"), 600);
            await FastGoAsync(scene, Menu(page, "Audit Trail"));
            var row = page.Locator("ul.admin-list-list > li, .list-group-item, tbody tr").Filter(new() { HasText = "Login failed" }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("result-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("span.badge:has(i.fa-wifi)").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("ul.admin-list-list > li, .list-group-item, tbody tr").Filter(new() { HasText = "Published" }).First);
        });

        await scene.StepAsync("result-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = page.Locator("ul.admin-list-list > li, .list-group-item, tbody tr").Filter(new() { HasText = "Login failed" }).First;
            await FastGoAsync(scene, row.GetByRole(AriaRole.Link, new() { Name = "Details" }));
            await page.GetByText("Event Info").First.WaitForAsync();
            await scene.PauseAsync(3000);
            await scene.HighlightAsync(page.Locator("tr:has(th:text-is(\"Client IP address\"))").First);
        });
    }
}

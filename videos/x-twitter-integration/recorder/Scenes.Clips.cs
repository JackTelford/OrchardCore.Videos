using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-keys", KeysAsync),
        ("04-signin", SigninSettingsAsync),
        ("05-login", LoginAsync),
        ("07-workflow", WorkflowAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await EnableFeaturesAsync(page, "OrchardCore.Workflows");
        await Task.Delay(6000);
        await context.CloseAsync();
    }

    private static async Task OpenGroupAsync(Scene scene, string group, int pause = 200)
    {
        var button = scene.Page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\")), #left-nav button.item-label:has(span.title:text-is(\"{group}\"))").First;
        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await FastClickAsync(scene, button, pause);
        }
    }

    private static ILocator TwRow(IPage page, string text) => page.Locator(".list-group-item").Filter(new() { HasText = text }).First;

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools", 150);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))"));
            await FastTypeAsync(scene, page.Locator("#search-box"), "Twitter", 40);
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.HighlightAsync(TwRow(page, "Sign in with X (Twitter)").Locator(".direct-dependencies").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("a[href*=\"/Features/OrchardCore.Twitter.Signin/Enable\"]").First));
            await page.GetByText("was enabled.").First.WaitForAsync();
        });

        await scene.StepAsync("features-3", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#search-box"), "Twitter", 30);
            await page.WaitForFunctionAsync("() => document.querySelectorAll('.feature-group [data-filter-value]:not(.d-none)').length === 2");
            foreach (var row in await page.Locator("[data-filter-value*=\"OrchardCore.Twitter\"]").AllAsync())
            {
                await scene.HighlightAsync(row.Locator(".badge:has-text(\"Enabled\")").First);
            }

            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await OpenGroupAsync(scene, "Settings", 300);
            await OpenGroupAsync(scene, "Integrations", 300);
        });
    }

    private static async Task KeysAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("keys-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings", 200);
            await OpenGroupAsync(scene, "Integrations", 200);
            var link = page.Locator("#left-nav a[href$=\"/Settings/OrchardCore.Twitter\"]").First;
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(link));
            await scene.HighlightAsync(page.Locator(".alert.alert-warning").First);
        });

        await scene.StepAsync("keys-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, page.Locator("input[name$=\".APIKey\"]").First, "example-api-key", 45);
            await FastTypeAsync(scene, page.Locator("input[name$=\".AccessToken\"]").First, "example-access-token", 45);
            await scene.HighlightAsync(page.Locator("label:text-is(\"API key\")").First);
            await scene.HighlightAsync(page.Locator("label:text-is(\"Access Token\")").First);
        });

        await scene.StepAsync("keys-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, page.Locator("input[type=\"password\"][name$=\".ConsumerSecret.Value\"]").First, "example-api-secret-key", 45, fill: true);
            await FastTypeAsync(scene, page.Locator("input[type=\"password\"][name$=\".AccessTokenSecret.Value\"]").First, "example-access-token-secret", 45, fill: true);
        });

        await scene.StepAsync("keys-4", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First);
                await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
            });
            await scene.HighlightAsync(page.Locator("input[type=\"password\"][name$=\".ConsumerSecret.Value\"]").First);
            await scene.HighlightAsync(page.Locator("input[type=\"password\"][name$=\".AccessTokenSecret.Value\"]").First);
        });

        await scene.StepAsync("keys-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(page.Locator("input[type=\"password\"][name$=\".ConsumerSecret.Value\"]").First);
            await scene.PauseAsync(500);
        });
    }

    private static async Task SigninSettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("signin-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings", 200);
            await OpenGroupAsync(scene, "Security", 200);
            await OpenGroupAsync(scene, "Authentication", 200);
            var link = page.Locator("#left-nav a[href$=\"/Settings/OrchardCore.Twitter.Signin\"]").First;
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(link));
        });

        await scene.StepAsync("signin-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input[name$=\".CallbackPath\"]").First, "/signin-x", 70);
            await scene.HighlightAsync(page.Locator("input[name$=\".CallbackPath\"]").First.Locator("xpath=following::*[contains(@class,'hint')][1]"));
        });

        await scene.StepAsync("signin-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var tokens = page.GetByLabel("Store tokens", new() { Exact = true });
            await scene.HighlightAsync(tokens.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First);
                await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
            });
        });
    }

    private static async Task LoginAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.Context.RouteAsync(new Regex("^(?!" + Regex.Escape(BaseUrl) + ")"), route => route.AbortAsync());
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(500);
        await scene.StartCaptureAsync();
        var button = page.Locator("button[name=\"provider\"]").First;

        await scene.StepAsync("login-1", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Login"));
            var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Use another service to log in" });
            await heading.WaitForAsync();
            await scene.HighlightAsync(page.Locator("section").Filter(new() { Has = heading }).First);
        });

        await scene.StepAsync("login-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(button);
        });
    }

    private static async Task WorkflowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await page.EvaluateAsync("() => { localStorage.setItem('orchardcore:workflows-designer:toolbox-collapsed', 'true'); localStorage.setItem('orchardcore:workflows-designer:panel-collapsed', 'true'); }");
        await scene.StartCaptureAsync();

        await scene.StepAsync("flow-1", async () =>
        {
            await OpenGroupAsync(scene, "Design", 150);
            await FastGoAsync(scene, page.Locator("#left-nav a[href$=\"/Admin/Workflows/Types\"]").First);
            await FastGoAsync(scene, page.Locator("a.create").First);
            await FastTypeAsync(scene, page.Locator("input#Name"), "Announce Blog Posts on X", 20);
            await FastGoAsync(scene, page.Locator("button.save:has-text(\"Create and open the designer\")"));
            await page.Locator("[data-cy=\"start-picker\"]").WaitForAsync();
        });

        await scene.StepAsync("flow-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("[data-cy=\"start-picker\"]"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("[data-cy=\"start-picker-ContentPublishedEvent\"]"), 200);
            await page.Locator("[data-cy=\"activity-panel\"]").WaitForAsync();
        });

        await scene.StepAsync("flow-3", async () =>
        {
            var panel = page.Locator("[data-cy=\"activity-panel\"]");
            await FastClickAsync(scene, panel.Locator(".bootstrap-select button.dropdown-toggle").First, 300);
            await FastClickAsync(scene, page.Locator(".dropdown-menu.show a.dropdown-item").Filter(new() { HasText = "Blog Post" }).First, 300);
            await page.Keyboard.PressAsync("Escape");
            await FastClickAsync(scene, page.Locator("[data-cy=\"panel-activity-title\"]"), 300);
        });

        await scene.StepAsync("flow-4", async () =>
        {
            await ClosePanelAsync(page);
            var port = page.Locator(".wfd-node").First.Locator(".wfd-port[data-outcome=\"Done\"]").First;
            await ClickPortAsync(scene, port);
            await page.Locator("[data-cy=\"quick-add\"]").WaitForAsync();
            await page.Locator("[data-cy=\"quick-add-search\"]").PressSequentiallyAsync("Twitter", new() { Delay = 40 });
            var entry = page.Locator("[data-cy=\"quick-add\"] [role=\"option\"]").Filter(new() { HasText = "Update X (Twitter) Status Task" }).First;
            await scene.HighlightAsync(entry);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, entry, 300);
            await page.Locator("[data-cy=\"activity-panel\"]").WaitForAsync();
        });

        await scene.StepAsync("flow-5", async () =>
        {
            var field = page.GetByLabel("Template for updating X (Twitter) status");
            await scene.HighlightAsync(field);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, field, "Our blog has a post to read: https://example.com/blog", 15);
            await FastClickAsync(scene, page.Locator("[data-cy=\"panel-apply\"]"), 200);
            await field.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await page.WaitForTimeoutAsync(800);
        });

        await scene.StepAsync("flow-6", async () =>
        {
            await ClosePanelAsync(page);
            var task = page.Locator(".wfd-node").Filter(new() { HasText = "Twitter" }).Last;
            await scene.HighlightAsync(task.Locator(".wfd-port").First);
            await scene.HighlightAsync(task.Locator(".wfd-port").Last);
        });

        await scene.StepAsync("flow-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("[data-cy=\"toolbar-publish\"]"), 200);
            await page.GetByText("The workflow was published").First.WaitForAsync();
        });
    }

    private static async Task ClickPortAsync(Scene scene, ILocator port)
    {
        var dot = port.Locator(".wfd-port-dot").First;
        var (x, y) = await scene.CenterOfAsync(dot);
        await scene.MoveToAsync(x, y, 10);
        await Task.Delay(150);
        await scene.Page.Mouse.DownAsync();
        await Task.Delay(80);
        await scene.Page.Mouse.UpAsync();
        await Task.Delay(200);
    }

    private static async Task ClosePanelAsync(IPage page)
    {
        var close = page.Locator("[data-cy=\"activity-panel-close\"]");
        if (await close.IsVisibleAsync())
        {
            await close.ClickAsync();
        }

        await page.Locator("[data-cy=\"activity-panel\"]").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }
}

using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-settings", SettingsAsync),
        ("04-login", LoginAsync),
    ];

    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task OpenGroupAsync(Scene scene, string group, int pause = 200)
    {
        var button = scene.Page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\")), #left-nav button.item-label:has(span.title:text-is(\"{group}\"))").First;
        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await FastClickAsync(scene, button, pause);
        }
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools", 200);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))"));
            await FastTypeAsync(scene, page.Locator("#search-box"), "Google", 40);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(page.Locator(".feature-group:has(h3:text-is(\"Google\")) h3").First);
            await scene.PauseAsync(3800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("[data-filter-value*=\"OrchardCore.Google.GoogleAuthentication\"]").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1000);
            var enable = page.Locator("a[href*=\"/Features/OrchardCore.Google.GoogleAuthentication/Enable\"]").First;
            await scene.ClickAndWaitForNavigationAsync(enable);
            await scene.OffCameraAsync(async () => await page.Locator(".toast", new() { HasText = "Google Authentication was enabled" }).First.WaitForAsync());
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
            await OpenGroupAsync(scene, "Settings");
            await OpenGroupAsync(scene, "Security");
            await OpenGroupAsync(scene, "Authentication");
            await FastGoAsync(scene, page.Locator("#left-nav a:has(span.title:text-is(\"Google\"))").First);
            await page.GetByRole(AriaRole.Heading, new() { Name = "Google Authentication Settings" }).WaitForAsync();
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.HighlightAsync(page.Locator(".alert.alert-warning").First);
            await scene.MoveToAsync(page.Locator(".alert.alert-warning a:has-text(\"See documentation\")").First);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("label:text-is(\"Client ID\")").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, page.GetByLabel("Client ID", new() { Exact = true }), "1234-orchard-video-demo", 40);
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input[type='password']").First, "demo-secret-not-real", 40, fill: true);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("settings-5", async () =>
            await scene.HighlightAsync(page.GetByLabel("CallbackPath", new() { Exact = true })));

        await scene.StepAsync("settings-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            var tokens = page.GetByLabel("Store tokens");
            await tokens.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(tokens.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("settings-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First);
            await scene.OffCameraAsync(async () => await page.Locator(".toast", new() { HasText = "Site settings updated successfully." }).First.WaitForAsync());
            await scene.HighlightAsync(page.Locator("input[type='password']").First);
        });
    }

    private static async Task LoginAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await scene.Context.RouteAsync(new Regex("^(?!" + Regex.Escape(BaseUrl) + ")"), route => route.AbortAsync());
        await page.WaitForTimeoutAsync(500);
        await scene.StartCaptureAsync();
        var button = page.Locator("button[name=\"provider\"][value=\"Google\"]").First;

        await scene.StepAsync("login-1", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Login"));
            await page.GetByRole(AriaRole.Heading, new() { Name = "Use another service to log in" }).WaitForAsync();
            await scene.HighlightAsync(page.Locator("h1:text-is(\"Log in\")").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("h1:text-is(\"Use another service to log in\")").First);
        });

        await scene.StepAsync("login-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(button);
        });

        await scene.StepAsync("login-3", async () => await scene.MoveToAsync(button));
    }
}

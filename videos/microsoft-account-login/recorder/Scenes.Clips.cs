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
            await OpenGroupAsync(scene, "Tools", 300);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))"));
            await FastTypeAsync(scene, page.Locator("#search-box"), "Microsoft Account", 60);
            await page.Locator("#btn-enable-OrchardCore_Microsoft_Authentication_MicrosoftAccount").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var enable = page.Locator("#btn-enable-OrchardCore_Microsoft_Authentication_MicrosoftAccount");
            await scene.MoveToAsync(enable);
            await scene.PauseAsync(3000);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(enable));
            await page.GetByText("Microsoft Account Authentication was enabled.").First.WaitForAsync();
            await FastTypeAsync(scene, page.Locator("#search-box"), "Microsoft Account", 60);
            await page.Locator("#btn-disable-OrchardCore_Microsoft_Authentication_MicrosoftAccount").WaitForAsync();
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
            await FastGoAsync(scene, page.Locator("#left-nav a:has(span.title:text-is(\"Microsoft\"))").First);
            await page.GetByRole(AriaRole.Heading, new() { Name = "Microsoft Account Authentication Settings" }).WaitForAsync();
        });

        await scene.StepAsync("settings-2", async () =>
            await FastTypeAsync(scene, page.Locator("#ISite_MicrosoftAccountSettings_AppId"), "11111111-2222-3333-4444-555555555555", 40));

        await scene.StepAsync("settings-3", async () =>
            await FastTypeAsync(scene, page.Locator("#ISite_MicrosoftAccountSettings_AppSecret"), "not-a-real-secret", 40, fill: true));

        await scene.StepAsync("settings-4", async () =>
        {
            var callback = page.Locator("#ISite_MicrosoftAccountSettings_CallbackPath");
            await scene.MoveToAsync(callback);
            await scene.HighlightAsync(callback);
        });

        await scene.StepAsync("settings-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var tokens = page.Locator("#ISite_MicrosoftAccountSettings_SaveTokens");
            await scene.MoveToAsync(tokens);
            await scene.HighlightAsync(tokens.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("settings-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(7800);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First));
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        });
    }

    private static async Task LoginAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.Context.ClearCookiesAsync();
        await scene.Context.RouteAsync(new Regex("^(?!" + Regex.Escape(BaseUrl) + ")"), route => route.AbortAsync());
        await scene.OpenAsync("/Login");
        var page = scene.Page;
        for (var i = 0; i < 4 && await page.Locator("button[name=\"provider\"]").CountAsync() == 0; i++)
        {
            await Task.Delay(2000);
            await page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        await scene.StartCaptureAsync();
        var button = page.Locator("button[name=\"provider\"]").First;

        await scene.StepAsync("login-1", async () =>
        {
            var heading = page.Locator("h1:text-is(\"Use another service to log in\")").First;
            await scene.MoveToAsync(heading);
            await scene.HighlightAsync(heading);
        });

        await scene.StepAsync("login-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(button);
            await scene.HighlightAsync(button);
        });

        await scene.ClearHighlightsAsync();
    }
}

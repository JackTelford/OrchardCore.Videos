using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("04-enable", EnableAsync),
        ("05-settings", SettingsAsync),
        ("06-login", LoginAsync),
    ];

    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task OpenGroupAsync(Scene scene, string group, int pause = 500)
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
            await FastTypeAsync(scene, page.Locator("#search-box"), "Entra ID", 70);
            var row = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_Microsoft_Authentication_AzureAD") }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("#btn-enable-OrchardCore_Microsoft_Authentication_AzureAD");
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(enable));
            await page.GetByText("was enabled").First.WaitForAsync();
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
            await FastGoAsync(scene, page.Locator("#left-nav a.microsoft-entra-id").First);
            await scene.HighlightAsync(page.Locator(".alert:has-text(\"The website might be restarted upon saving the settings\")").First);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, page.Locator("input[id$='DisplayName']").First, "Maple Works Microsoft account", 40);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input[id$='AppId']").First, "11111111-2222-3333-4444-555555555555", 25);
            await FastTypeAsync(scene, page.Locator("input[id$='TenantId']").First, "66666666-7777-8888-9999-000000000000", 25);
        });

        await scene.StepAsync("settings-4", async () =>
        {
            var callback = page.Locator("input[id$='CallbackPath']").First;
            await FastClickAsync(scene, callback, 200);
            await scene.HighlightAsync(callback);
        });

        await scene.StepAsync("settings-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("input[id$='SaveTokens']").First.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("settings-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First));
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        });
    }

    private static async Task LoginAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "signed-out.json"), Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Login");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(500);
        await scene.StartCaptureAsync();
        var button = page.Locator("button[name='provider']").First;

        await scene.StepAsync("login-1", async () =>
        {
            await scene.MoveToAsync(page.GetByRole(AriaRole.Heading, new() { Name = "Log in", Exact = true }));
            await scene.MoveToAsync(page.GetByRole(AriaRole.Heading, new() { Name = "Use another service to log in" }));
            await scene.HighlightAsync(button);
        });

        await scene.StepAsync("login-2", async () => await scene.PauseAsync(1500));

        await scene.StepAsync("login-3", async () => await scene.MoveToAsync(button));
    }
}

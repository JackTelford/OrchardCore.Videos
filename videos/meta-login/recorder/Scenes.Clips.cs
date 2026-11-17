using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-app", AppAsync),
        ("04-login", LoginSettingsAsync),
        ("05-signin", SigninAsync),
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

    private static ILocator FeatureRow(IPage page) => page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_Facebook_Login, #btn-disable-OrchardCore_Facebook_Login") }).First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools", 400);
            await FastGoAsync(scene, page.Locator("a.item-label[href$=\"/Admin/Features\"]"));
            await FastTypeAsync(scene, page.Locator("#search-box"), "Meta Login", 90);
            await scene.HighlightAsync(FeatureRow(page).Locator(".admin-list-secondary").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FeatureRow(page));
            await scene.MoveToAsync(page.Locator("#btn-enable-OrchardCore_Facebook_Login"));
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(page.Locator("#btn-enable-OrchardCore_Facebook_Login"));
                await page.Locator(".toast.message-success").First.WaitForAsync();
            });
            await FastTypeAsync(scene, page.Locator("#search-box"), "Meta Login", 60);
            await scene.HighlightAsync(page.Locator("#btn-disable-OrchardCore_Facebook_Login"));
        });
    }

    private static async Task AppAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("app-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings", 400);
            await OpenGroupAsync(scene, "Integrations", 400);
            await FastGoAsync(scene, page.Locator("a.item-label[href$=\"/Admin/Settings/OrchardCore.Facebook\"]"));
            await scene.HighlightAsync(page.Locator("p.alert.alert-warning, .alert.alert-warning").First);
        });

        await scene.StepAsync("app-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, page.Locator("input[name$=\"AppId\"]").First, "123456789012345", 60);
        });

        await scene.StepAsync("app-3", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input[type=\"password\"]").First, "fake-secret-for-video", 40, fill: true);
            await scene.HighlightAsync(page.Locator("input[type=\"password\"]").First);
        });

        await scene.StepAsync("app-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300, 8);
            await scene.HighlightAsync(page.Locator("input[name$=\"Version\"]").First);
            await scene.HighlightAsync(page.Locator("input[name$=\"SdkJs\"]").First);
            await scene.HighlightAsync(page.Locator("input[type=\"checkbox\"][name$=\"FBInit\"]").First.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("app-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(3000);
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(page.Locator("button.primaryAction.save, button.save").First);
                await page.Locator(".toast.message-success").First.WaitForAsync();
            });
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.HighlightAsync(page.Locator("input[type=\"password\"]").First);
        });
    }

    private static async Task LoginSettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("login-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings", 400);
            await OpenGroupAsync(scene, "Security", 400);
            await OpenGroupAsync(scene, "Authentication", 400);
            await FastGoAsync(scene, page.Locator("a.item-label[href$=\"/Admin/Settings/OrchardCore.Facebook.Login\"]"));
            await scene.HighlightAsync(page.Locator("span.hint a").Last);
        });

        await scene.StepAsync("login-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var callback = page.Locator("input[name$=\"CallbackPath\"]").First;
            await scene.HighlightAsync(callback);
            await scene.MoveToAsync(callback);
        });

        await scene.StepAsync("login-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var tokens = page.Locator("input[type=\"checkbox\"][name$=\"SaveTokens\"]").First;
            await scene.HighlightAsync(tokens.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
            await scene.MoveToAsync(tokens);
        });
    }

    private static async Task SigninAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "signed-out.json"), Path.GetDirectoryName(directory), durations);
        await scene.Context.RouteAsync(new Regex("^(?!" + Regex.Escape(BaseUrl) + ")"), route => route.AbortAsync());
        await scene.OpenAsync("/");
        var page = scene.Page;
        await scene.StartCaptureAsync();
        var button = page.Locator("button[name=\"provider\"][value=\"Facebook\"]").First;

        await scene.StepAsync("signin-1", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Login"));
            await page.GetByRole(AriaRole.Heading, new() { Name = "Use another service to log in" }).WaitForAsync();
        });

        await scene.StepAsync("signin-2", async () =>
        {
            await scene.HighlightAsync(button);
            await scene.MoveToAsync(button);
        });

        await scene.StepAsync("signin-3", async () => await scene.PauseAsync(1200));
    }
}

using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-enable", EnableAsync),
        ("04-configure", ConfigureAsync),
        ("05-login", LoginAsync),
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

    private static ILocator GhRow(IPage page) => page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_GitHub_Authentication, #btn-disable-OrchardCore_GitHub_Authentication") }).First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools", 150);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))"));
            await FastTypeAsync(scene, page.Locator("#search-box"), "github", 30);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var badge = GhRow(page).Locator(".direct-dependencies .badge").First;
            await scene.MoveToAsync(badge);
            await scene.HighlightAsync(badge, "Dependency");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(GhRow(page).Locator("#btn-enable-OrchardCore_GitHub_Authentication"));
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await GhRow(page).Locator("#btn-disable-OrchardCore_GitHub_Authentication").WaitForAsync();
            await FastTypeAsync(scene, page.Locator("#search-box"), "github", 60);
            await scene.HighlightAsync(GhRow(page));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ConfigureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("config-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings", 120);
            await OpenGroupAsync(scene, "Security", 120);
            await OpenGroupAsync(scene, "Authentication", 120);
            var link = page.Locator("#left-nav a:has(span.title:text-is(\"GitHub\"))").First;
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(link));
        });

        await scene.StepAsync("config-2", async () =>
            await FastTypeAsync(scene, page.GetByLabel("Client ID", new() { Exact = true }), "example-client-id", 60));

        await scene.StepAsync("config-3", async () =>
            await FastTypeAsync(scene, page.GetByLabel("Client secret", new() { Exact = true }), "example-client-secret", 60, fill: true));

        await scene.StepAsync("config-4", async () =>
        {
            var callback = page.GetByLabel("CallbackPath", new() { Exact = true });
            await scene.MoveToAsync(callback);
            await scene.HighlightAsync(callback, "Empty: /signin-github");
        });

        await scene.StepAsync("config-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var tokens = page.GetByLabel("Store tokens", new() { Exact = true });
            await tokens.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(tokens);
            await scene.HighlightAsync(tokens.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("config-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
            await scene.OffCameraAsync(async () => await page.GetByPlaceholder("The value is securely stored. Enter a new value to update it, or leave it blank to keep it.").WaitForAsync());
            await scene.HighlightAsync(page.GetByLabel("Client secret", new() { Exact = true }));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LoginAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "signed-out.json"), Path.GetDirectoryName(directory), durations);
        await scene.Context.RouteAsync(new Regex("^(?!" + Regex.Escape(BaseUrl) + ")"), route => route.AbortAsync());
        await scene.OpenAsync("/Login");
        var page = scene.Page;
        var github = page.Locator("button[name='provider'][value='GitHub']");
        for (var i = 0; i < 4 && await github.CountAsync() == 0; i++)
        {
            await Task.Delay(2000);
            await page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("login-1", async () =>
        {
            var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Use another service to log in" });
            var section = page.Locator("section").Filter(new() { Has = heading });
            await scene.MoveToAsync(heading);
            await scene.HighlightAsync(section);
        });

        await scene.StepAsync("login-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(github);
            await scene.HighlightAsync(github, "Sends the browser to GitHub");
        });

        await scene.ClearHighlightsAsync();
    }
}

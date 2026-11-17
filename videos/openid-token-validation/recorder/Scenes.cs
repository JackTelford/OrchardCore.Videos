using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "OpenID Connect Token Validation". PrepareAsync enables the Authorization Server. Record 03-enable, then 04-settings.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-enable", EnableAsync),
        ("04-settings", SettingsAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        await page.GotoAsync(BaseUrl + "/Admin/Features", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#search-box").FillAsync("OpenID Connect Authorization Server");
        await page.Locator("#btn-enable-OrchardCore_OpenId_Server").ClickAsync();
        await page.GetByText("was enabled.").First.WaitForAsync();
        Console.WriteLine("Enabled the Authorization Server.");
        await context.CloseAsync();
    }

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static ILocator Group(Scene scene, string name)
        => scene.Page.Locator("#left-nav button:visible").Filter(new() { HasTextRegex = Exact(name) }).First;

    private static async Task OpenGroupAsync(Scene scene, string name)
    {
        var group = Group(scene, name);

        if (await group.GetAttributeAsync("aria-expanded") == "false")
        {
            await scene.ClickAsync(group, pauseAfter: 300);
        }
    }

    // Clicks something that loads another page, with the wait cut out of the clip.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => target.ClickAsync()), leadMilliseconds: 150);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.Page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        var row = scene.Page.Locator(".list-group-item:has(#btn-enable-OrchardCore_OpenId_Validation), .list-group-item:has(#btn-disable-OrchardCore_OpenId_Validation)").First;

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "token validation", delay: 30);
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("#btn-enable-OrchardCore_OpenId_Validation").First);
            await scene.Page.GetByText("The feature OpenID Connect Token Validation was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            await scene.MoveToAsync(scene.Page.GetByText("The feature OpenID Connect Token Validation was enabled.").First);
            await scene.PauseAsync(2000);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "token validation", delay: 10);
            await scene.HighlightAsync(row);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        var tenant = scene.Page.Locator("input.openid-validation-tenant").First;
        var authority = scene.Page.Locator("#AuthorityField").First;
        var audience = scene.Page.Locator("#AudienceField").First;

        async Task SaveAsync(string message)
        {
            var save = scene.Page.Locator("button.save, button:has-text('Save')").First;
            await save.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText(message).First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);

            if (message.StartsWith("OpenID validation", StringComparison.Ordinal))
            {
                await scene.MoveToAsync(scene.Page.GetByText(message).First);
            }
        }

        await scene.StepAsync("settings-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings");
            await OpenGroupAsync(scene, "OpenID Connect");
            await NavAsync(scene, scene.Page.Locator("#left-nav a[href$='/Admin/OpenId/ValidationConfiguration']").First);
            await scene.Page.GetByText("Configure OpenID Connect validation settings").First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".alert-warning, p.alert").Filter(new() { HasText = "The website might be restarted" }).First);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(tenant);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(tenant, pauseAfter: 200);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await scene.Page.Keyboard.PressAsync("Backspace");
            await scene.Page.Keyboard.PressAsync("Tab");
            await authority.WaitForAsync();
            await scene.HighlightAsync(authority);
            await scene.PauseAsync(400);
            await scene.HighlightAsync(audience);
            await scene.PauseAsync(400);
            await scene.HighlightAsync(scene.Page.Locator("#DisableTokenTypeValidationField").First);
            await scene.PauseAsync(400);
            await scene.HighlightAsync(scene.Page.Locator(".mb-3:has(label:has-text('Metadata Address')), div:has(> label:has-text('Metadata Address'))").First);
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(authority, "https://idp.example.com", delay: 25);
            await scene.Page.Keyboard.PressAsync("Tab");
            await SaveAsync("An audience must be set when configuring the authority.");
        });

        await scene.StepAsync("settings-5", async () =>
        {
            await scene.TypeAsync(audience, "inventory-api", delay: 40);
            await scene.Page.Keyboard.PressAsync("Tab");
            await SaveAsync("OpenID validation configuration successfully updated.");
        });

        await scene.StepAsync("settings-6", async () =>
        {
            await scene.HighlightAsync(authority);
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(audience);
        });

        await scene.ClearHighlightsAsync();
    }
}

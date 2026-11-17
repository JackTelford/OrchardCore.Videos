using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "OpenID Connect Keys as Secrets". PrepareAsync enables the Authorization Server and Secrets (not OpenID Connect
/// Secrets). Record in one take, in order: 02-enable, 03-create, 04-select. The certificate lists of the server page are never opened.
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
        ("02-enable", EnableAsync),
        ("03-create", CreateAsync),
        ("04-select", SelectAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);

        foreach (var (search, id) in new[] { ("OpenID Connect Authorization Server", "OrchardCore_OpenId_Server"), ("Secrets", "OrchardCore_Secrets") })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#search-box").FillAsync(search);

            if (await page.Locator($"#btn-enable-{id}").CountAsync() > 0)
            {
                await page.Locator($"#btn-enable-{id}").ClickAsync();
                await page.GetByText("was enabled.").First.WaitForAsync();
            }

            Console.WriteLine("Enabled " + search);
        }

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

    private static async Task<Scene> OpenDashboardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();
        return scene;
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations);
        var row = scene.Page.Locator("[data-filter-value*='OrchardCore.OpenId.Secrets']").First;

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await NavAsync(scene, scene.Page.Locator("#left-nav a[href$='/Admin/Features']").First);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "OpenID", delay: 40);
            await row.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(row);
            await scene.PauseAsync(1500);
            var badges = row.Locator(".direct-dependencies .badge");

            for (var i = 0; i < Math.Min(2, await badges.CountAsync()); i++)
            {
                await scene.MoveToAsync(badges.Nth(i));
                await scene.PauseAsync(1200);
            }
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("#btn-enable-OrchardCore_OpenId_Secrets").First);
            await scene.Page.GetByText("The feature OpenID Connect Secrets was enabled.").First.WaitForAsync(new() { Timeout = 60000 });
            await scene.MoveToAsync(scene.Page.GetByText("The feature OpenID Connect Secrets was enabled.").First);
            await scene.PauseAsync(2000);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "OpenID", delay: 6);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations);

        async Task AddRsaAsync()
        {
            await scene.ClickAsync(scene.Page.Locator("button.create").First, pauseAfter: 400);
            await scene.Page.Locator("#modalSecretTypes.show").WaitForAsync();
            var create = scene.Page.Locator("#modalSecretTypes .card:has(h5:text-is('RSA Key')) a").First;
            await scene.PauseAsync(300);
            await NavAsync(scene, create);
        }

        await scene.StepAsync("create-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await OpenGroupAsync(scene, "Security");
            await NavAsync(scene, scene.Page.Locator("#left-nav a[href*='/Admin/Secrets']").First);
            await scene.Page.GetByText("Nothing here!").First.WaitForAsync();
            await scene.ClickAsync(scene.Page.Locator("button.create").First, pauseAfter: 400);
            await scene.Page.Locator("#modalSecretTypes.show").WaitForAsync();
            var card = scene.Page.Locator("#modalSecretTypes .card:has(h5:text-is('RSA Key'))").First;
            await scene.HighlightAsync(card);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, card.Locator("a").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name").First, "ServerSigningKey", delay: 40);
            await scene.HighlightAsync(scene.Page.GetByText("A unique name used by the settings").First);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#RsaKeySize").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#Description").First, "Signing key of the OpenID Connect server.", delay: 20);
            await scene.MoveToAsync(scene.Page.Locator("#ExpiresUtc").First);
            var save = scene.Page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText("Secret created successfully.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("Secret created successfully.").First);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await AddRsaAsync();
            await scene.TypeAsync(scene.Page.Locator("#Name").First, "ServerEncryptionKey", delay: 20);
            var save = scene.Page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText("Secret created successfully.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("Secret created successfully.").First);
        });

        await scene.StepAsync("create-5", async () =>
        {
            foreach (var name in new[] { "ServerEncryptionKey", "ServerSigningKey" })
            {
                var row = scene.Page.Locator(".list-group-item").Filter(new() { HasText = name }).First;
                await scene.HighlightAsync(row);
                await scene.HoldAsync(2.5);
                await scene.ClearHighlightsAsync();
            }
        });
    }

    private static async Task SelectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenDashboardAsync(browser, directory, durations);
        var card = scene.Page.Locator(".card:has(h5:text-is('RSA Keys from Secrets'))").First;

        await scene.StepAsync("select-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings");
            await OpenGroupAsync(scene, "OpenID Connect");
            await NavAsync(scene, scene.Page.Locator("#left-nav a[href$='/Admin/OpenId/ServerConfiguration']").First);
            await scene.Page.GetByText("Configure OpenID Connect server settings").First.WaitForAsync();
            await card.WaitForAsync();
            await card.EvaluateAsync("e => e.scrollIntoView({ behavior: 'smooth', block: 'center' })");
            await scene.PauseAsync(700);
            await scene.HighlightAsync(card);
        });

        await scene.StepAsync("select-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#SigningKeySecretName").First, "ServerSigningKey");
            await scene.HighlightAsync(scene.Page.GetByText("Select an RSA key or X.509 certificate secret to use for signing").First);
        });

        await scene.StepAsync("select-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#EncryptionKeySecretName").First, "ServerEncryptionKey");
            await scene.HighlightAsync(scene.Page.GetByText("Select an RSA key or X.509 certificate secret to use for encrypting").First);
        });

        await scene.StepAsync("select-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var save = scene.Page.Locator("button.save, button:has-text('Save')").First;
            await save.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(save);
            await scene.ClickAndWaitForNavigationAsync(save);
            await scene.Page.GetByText("OpenID server configuration successfully updated.").First.WaitForAsync(new() { Timeout = 60000 });
            await scene.HighlightAsync(scene.Page.GetByText("The website might be restarted").First);
            await scene.HoldAsync(0.8);
            await scene.ClearHighlightsAsync();
            await card.EvaluateAsync("e => e.scrollIntoView({ behavior: 'instant', block: 'center' })");
            await scene.PauseAsync(800);
            await scene.NudgeAsync();
            await scene.PauseAsync(2000);
        });
    }
}

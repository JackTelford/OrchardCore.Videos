using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Blazor CMS", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-content", ContentAsync),
        ("05-run", RunAsync),
        ("06-tenant", TenantAsync),
        ("06b-setup", TenantSetupAsync),
        ("07-site", SiteAsync),
        ("08-click", ClickAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private const string DefaultRecipe = """
        {"name":"","displayName":"","description":"","author":"","website":"","version":"","issetuprecipe":false,"categories":[],"tags":[],"steps":[{"name":"ContentDefinition","ContentTypes":[{"Name":"MarkdownPage","DisplayName":"Markdown Page","Settings":{"ContentTypeSettings":{"Creatable":true,"Listable":true,"Draftable":true,"Versionable":true,"Securable":true},"FullTextAspectSettings":{},"GraphQLContentTypeSettings":{}},"ContentTypePartDefinitionRecords":[{"PartName":"MarkdownPage","Name":"MarkdownPage","Settings":{}},{"PartName":"AliasPart","Name":"AliasPart","Settings":{"ContentTypePartSettings":{"Position":"1"}}},{"PartName":"MarkdownBodyPart","Name":"MarkdownBodyPart","Settings":{"ContentTypePartSettings":{"Position":"3"}}},{"PartName":"TitlePart","Name":"TitlePart","Settings":{"ContentTypePartSettings":{"Position":"0"}}}]}],"ContentParts":[]},{"name":"Content","data":[{"ContentItemId":"48swn17t4vn6dwvgkzk970q6dp","ContentItemVersionId":"4x0wa7sfp911brv9gmvdmp7rmf","ContentType":"MarkdownPage","DisplayText":"ORCHARD CORE","Latest":true,"Published":true,"ModifiedUtc":"2024-01-27T20:26:30.0529958Z","PublishedUtc":"2024-01-27T20:26:30.0704945Z","CreatedUtc":"2024-01-27T20:26:08.2132621Z","Author":"admin","MarkdownPage":{},"AliasPart":{"Alias":"orchard-core"},"MarkdownBodyPart":{"Markdown":"## Introduction \r\nOpen-source, modular, multi-tenant application framework and CMS for ASP.NET Core\r\n\r\n## Features\r\n- Media Management\r\n- Multi-Lingual\r\n- Modular\r\n- Multi-Tenant\r\n- Security\r\n- Workflows"},"TitlePart":{"Title":"ORCHARD CORE"}}]}]}
        """;

    private const string TenantRecipe = """
        {"name":"","displayName":"","description":"","author":"","website":"","version":"","issetuprecipe":false,"categories":[],"tags":[],"steps":[{"name":"ContentDefinition","ContentTypes":[{"Name":"MarkdownPage","DisplayName":"Markdown Page","Settings":{"ContentTypeSettings":{"Creatable":true,"Listable":true,"Draftable":true,"Versionable":true,"Securable":true},"FullTextAspectSettings":{},"GraphQLContentTypeSettings":{}},"ContentTypePartDefinitionRecords":[{"PartName":"MarkdownPage","Name":"MarkdownPage","Settings":{}},{"PartName":"AliasPart","Name":"AliasPart","Settings":{"ContentTypePartSettings":{"Position":"1"}}},{"PartName":"MarkdownBodyPart","Name":"MarkdownBodyPart","Settings":{"ContentTypePartSettings":{"Position":"3"}}},{"PartName":"TitlePart","Name":"TitlePart","Settings":{"ContentTypePartSettings":{"Position":"0"}}}]}],"ContentParts":[]},{"name":"Content","data":[{"ContentItemId":"48swn17t4vn6dwvgkzk970q6dp","ContentItemVersionId":"4x0wa7sfp911brv9gmvdmp7rmf","ContentType":"MarkdownPage","DisplayText":"ORCHARD CORE FOR TENANT 01","Latest":true,"Published":true,"ModifiedUtc":"2024-01-27T20:26:30.0529958Z","PublishedUtc":"2024-01-27T20:26:30.0704945Z","CreatedUtc":"2024-01-27T20:26:08.2132621Z","Author":"admin","MarkdownPage":{},"AliasPart":{"Alias":"orchard-core"},"MarkdownBodyPart":{"Markdown":"## Introduction \r\nOpen-source, modular, multi-tenant application framework and CMS for ASP.NET Core\r\n\r\n## Features\r\n- Media Management\r\n- Multi-Lingual\r\n- Modular\r\n- Multi-Tenant (as we are seeing)\r\n- Security\r\n- Workflows"},"TitlePart":{"Title":"ORCHARD CORE FOR TENANT 01"}}]}]}
        """;

    private static string TenantPassword { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    /// <summary>Nothing to seed in the site. Warms the pages up.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        foreach (var path in new[] { "/Admin", "/Admin/Features", "/Admin/DeploymentPlan/Import/Json", "/home" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Pages warmed up.");
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<Scene, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static async Task GoOffAsync(Scene scene, ILocator locator, int lead = 250)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: lead);
    }

    private static async Task ImportJsonAsync(IPage page, string json)
    {
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", json);
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task WaitTextAsync(IPage page, string text, int timeout = 60_000)
        => await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = timeout });

    // Clicks a link or button that loads a new page; the load is cut out of the video (see Scene.OffCameraAsync).
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
    }

    // Opens a page during the capture, with the load cut out of the video.
    private static Task VisitAsync(Scene scene, string path)
        => scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(BaseUrl + path)), leadMilliseconds: 250);

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Row(IPage page, string text) => page.Locator(".list-group-item, tr").Filter(new() { HasTextString = text }).First;

    private static async Task OpenAdminGroupAsync(IPage page, string name)
    {
        var group = page.Locator("#left-nav").GetByText(name, new() { Exact = true }).First;
        await group.ClickAsync();
        await Task.Delay(500);
    }

    private static async Task ContentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/DeploymentPlan/Import/Json", s => ImportJsonAsync(s.Page, DefaultRecipe));
        var page = scene.Page;

        await scene.StepAsync("content-1", async () =>
        {
            await ShowAsync(scene, page.Locator(".CodeMirror").First, 2000);
            await GoOffAsync(scene, page.Locator("button.import").First);
            await WaitTextAsync(page, "Recipe imported successfully!", 120_000);
        });

        await scene.StepAsync("content-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems");
            var row = page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "ORCHARD CORE" }).First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row.Locator(".badge").Filter(new() { HasTextString = "MarkdownPage" }).First);
        });

        await scene.StepAsync("content-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "ORCHARD CORE" }).First;
            await GoAsync(scene, row.Locator("a.edit").First);
            await page.WaitForFunctionAsync("() => [...document.querySelectorAll('input')].some(i => i.value === 'orchard-core')");
            await ShowAsync(scene, page.GetByLabel("Title", new() { Exact = true }).First, 1500);
            await scene.HighlightAsync(page.GetByLabel("Alias").First);
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/home", s => s.Page.GetByText("Hello, Orchard!").First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".sidebar").First);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.nav-link[href$='content/orchard-core']").First);
            await page.Locator("h1").Filter(new() { HasTextString = "ORCHARD CORE" }).First.WaitForAsync();
        });
    }

    private static async Task TenantAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Features", s => s.Page.Locator("#search-box").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("tenant-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 150);
            await page.Keyboard.TypeAsync("Tenants", new() { Delay = 50 });
            var enable = page.Locator("#btn-enable-OrchardCore_Tenants");
            await enable.WaitForAsync();
            await GoOffAsync(scene, enable);
            await WaitTextAsync(page, "was enabled", 120_000);
            await scene.MoveToAsync(900, 650);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 150);
            await page.Keyboard.TypeAsync("Tenants", new() { Delay = 50 });
            await scene.HighlightAsync(page.Locator("#btn-disable-OrchardCore_Tenants"));
        });

        await scene.StepAsync("tenant-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Tenants");
            await GoAsync(scene, page.Locator("a.create").First);
            await scene.TypeAsync(page.Locator("#Name"), "FirstTenant", delay: 45);
            await scene.TypeAsync(page.Locator("#RequestUrlPrefix"), "tenant01", delay: 45);
            await scene.SelectAsync(page.Locator("#RecipeName"), "Headless site");
            var provider = page.Locator("#DatabaseProvider");
            if (await provider.CountAsync() > 0 && await provider.First.IsVisibleAsync())
            {
                try
                {
                    await scene.SelectAsync(provider.First, "Sqlite");
                }
                catch (PlaywrightException)
                {
                }
            }

            await GoAsync(scene, page.Locator("button.create").First);
            await page.GetByText("FirstTenant").First.WaitForAsync();
        });

        await scene.StepAsync("tenant-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("#btn-setup-FirstTenant"), "Setup");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            var row = page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "FirstTenant" }).First;
            await scene.HighlightAsync(row.Locator(".badge").Filter(new() { HasTextString = "Uninitialized" }).First);
        });
    }

    /// <summary>Between 06-tenant and 07-site, off camera: sets the tenant up and imports its content. No frames.</summary>
    private static async Task TenantSetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(180_000);

        await page.GotoAsync(BaseUrl + "/Admin/Tenants");
        var setup = page.Locator("#btn-setup-FirstTenant");
        await setup.ClickAsync();
        var popup = page;
        await popup.WaitForURLAsync(u => u.Contains("tenant01"));
        await popup.WaitForLoadStateAsync();
        popup.SetDefaultTimeout(180_000);
        await popup.Locator("#SiteName").FillAsync("First Tenant Site");
        await popup.Locator("#SiteTimeZone").SelectOptionAsync(new SelectOptionValue { Value = "America/Los_Angeles" });
        var provider = popup.Locator("#DatabaseProvider");
        if (await provider.CountAsync() > 0 && await provider.IsVisibleAsync())
        {
            await provider.SelectOptionAsync(new SelectOptionValue { Value = "Sqlite" });
        }

        await popup.Locator("#UserName").FillAsync("admin");
        await popup.Locator("#Email").FillAsync("admin@example.com");
        await popup.Locator("#Password").FillAsync(TenantPassword);
        await popup.Locator("#PasswordConfirmation").FillAsync(TenantPassword);
        await popup.Locator("#SubmitButton").ClickAsync();
        await popup.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(8000);

        await popup.GotoAsync(BaseUrl + "/tenant01/Admin");
        await popup.Locator("#LoginForm_UserName, input[name='LoginForm.UserName']").First.FillAsync("admin");
        await popup.Locator("#LoginForm_Password, input[name='LoginForm.Password']").First.FillAsync(TenantPassword);
        await popup.GetByRole(AriaRole.Button, new() { Name = "Log in" }).First.ClickAsync();
        await popup.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await popup.GotoAsync(BaseUrl + "/tenant01/Admin/DeploymentPlan/Import/Json");
        await ImportJsonAsync(popup, TenantRecipe);
        await popup.Locator("button.import").First.ClickAsync();
        await WaitTextAsync(popup, "Recipe imported successfully!", 120_000);
        await popup.GotoAsync(BaseUrl + "/tenant01/home");
        await popup.GetByText("Hello, Orchard!").First.WaitForAsync();
        await context.CloseAsync();
        Console.WriteLine("Tenant set up.");
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/home", s => s.Page.GetByText("Hello, Orchard!").First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("site-1", async () =>
        {
            await scene.NavigateAsync("/tenant01/home");
            await page.GetByText("Hello, Orchard!").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".sidebar").First);
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.nav-link[href$='content/orchard-core']").First);
            await page.Locator("h1").Filter(new() { HasTextString = "ORCHARD CORE FOR TENANT 01" }).First.WaitForAsync();
        });
    }

    private static async Task ClickAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var sockets = 0;
        var scene = await NewSceneAsync(browser, directory, durations);
        await using var _ = scene;
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        scene.Page.WebSocket += (_, _) => sockets++;
        await scene.OpenAsync("/interactive");
        for (var i = 0; i < 60 && sockets == 0; i++)
        {
            await Task.Delay(500);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        var page = scene.Page;

        await scene.StepAsync("inter-3", async () =>
        {
            await scene.ClickAsync(page.Locator("button.btn-primary").First, pauseAfter: 300);
            await page.GetByText("Clicked!").First.WaitForAsync();
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the editors.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

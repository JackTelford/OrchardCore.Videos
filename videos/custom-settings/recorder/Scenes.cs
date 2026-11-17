using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Custom settings", recorded in order on one fresh site.</summary>
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
        ("04-edit", EditAsync),
        ("05-permission", PermissionAsync),
        ("06-deploy", DeployAsync),
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

    /// <summary>Disables Custom Settings and creates the empty deployment plan "Settings Export", outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#search-box").FillAsync("Custom Settings");
        var disable = page.Locator("#btn-disable-OrchardCore_CustomSettings");
        if (await disable.CountAsync() > 0)
        {
            await disable.ClickAsync();
            await page.Locator(".modal.show").GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
            await WaitTextAsync(page, "was disabled");
        }

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Index");
        await page.Locator("a[href*='/DeploymentPlan/Create']").First.ClickAsync();
        await page.GetByLabel("Name").First.FillAsync("Settings Export");
        await page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        foreach (var path in new[] { "/Admin/Features", "/Admin/ContentTypes/List", "/Admin/Roles", "/Admin/DeploymentPlan/Index" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Prepared.");
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

    private static async Task<Scene> StartAdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        return scene;
    }

    private static ILocator Nav(IPage page, string href) => page.Locator($"#left-nav a[href$='{href}']").First;

    private static async Task SaveTypeAsync(Scene scene)
    {
        await GoAsync(scene, scene.Page.Locator("button.btn-success.save[value='Save']").First);
        await WaitTextAsync(scene.Page, "Content type updated successfully.");
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, Nav(page, "/Admin/Features"));
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("Custom Settings", new() { Delay = 45 });
            await page.Locator("#btn-enable-OrchardCore_CustomSettings").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_CustomSettings"));
            await WaitTextAsync(page, "was enabled", 90_000);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("type-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Design");
            await OpenAdminGroupAsync(page, "Content Definition");
            await GoAsync(scene, Nav(page, "/Admin/ContentTypes/List"));
            await GoAsync(scene, page.Locator("#btnCreate"));
            await page.Locator("#DisplayName").WaitForAsync();
        });

        await scene.StepAsync("type-2", async () =>
        {
            await scene.TypeAsync(page.Locator("#DisplayName"), "Announcement Banner", delay: 45);
            await page.WaitForFunctionAsync("() => document.querySelector('#Name').value === 'AnnouncementBanner'");
            await scene.MoveToAsync(page.Locator("#Name"));
            await scene.PauseAsync(1000);
            await GoAsync(scene, page.Locator("button.create").First);
            await WaitTextAsync(page, "has been created");
        });

        await scene.StepAsync("type-3", async () =>
        {
            await scene.PauseAsync(2000);
            await GoAsync(scene, page.Locator("a.btn, button").Filter(new() { HasTextString = "Cancel" }).First);
            await page.GetByRole(AriaRole.Heading, new() { Name = "Fields" }).First.WaitForAsync();
        });

        await scene.StepAsync("fields-1", async () =>
        {
            var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Fields" }).First;
            await heading.ScrollToMiddleAsync();
            await ShowAsync(scene, heading, 2000);
            await GoAsync(scene, page.Locator("a[href*='/AddFieldsTo/']").First);
            await page.Locator("#DisplayName").WaitForAsync();
        });

        await scene.StepAsync("fields-2", async () =>
        {
            await scene.TypeAsync(page.Locator("#DisplayName"), "Banner Text", delay: 45);
            await page.WaitForFunctionAsync("() => document.querySelector('#Name').value === 'BannerText'");
            await scene.ClickAsync(page.Locator("input[type='radio'][value='TextField']"), pauseAfter: 400);
            await GoAsync(scene, page.Locator("button[name='submit.Save'][value='Save']"));
            await WaitTextAsync(page, "has been added");
        });

        await scene.StepAsync("fields-3", async () =>
        {
            await GoAsync(scene, page.Locator("a[href*='/AddFieldsTo/']").First);
            await scene.TypeAsync(page.Locator("#DisplayName"), "Show Banner", delay: 45);
            await page.WaitForFunctionAsync("() => document.querySelector('#Name').value === 'ShowBanner'");
            await scene.ClickAsync(page.Locator("input[type='radio'][value='BooleanField']"), pauseAfter: 400);
            await GoAsync(scene, page.Locator("button[name='submit.Save'][value='Save']"));
            await WaitTextAsync(page, "has been added");
        });

        await scene.StepAsync("opts-1", async () =>
        {
            var creatable = page.Locator("input[type='checkbox'][name$='Creatable']").First;
            await creatable.ScrollToMiddleAsync();
            foreach (var name in new[] { "Creatable", "Listable", "Draftable", "Versionable", "Securable" })
            {
                await scene.ClickAsync(page.Locator($"input[type='checkbox'][name$='{name}']").First, pauseAfter: 500);
            }
        });

        await scene.StepAsync("opts-2", async () =>
        {
            await SaveTypeAsync(scene);
        });

        await scene.StepAsync("stereo-1", async () =>
        {
            var edit = page.Locator("a[role='btn-edit-AnnouncementBanner']");
            await edit.ScrollToMiddleAsync();
            await GoAsync(scene, edit);
            var box = page.GetByLabel("Stereotype");
            await box.ScrollToMiddleAsync();
            await scene.TypeAsync(box, "CustomSettings", delay: 45);
            await scene.HighlightAsync(box);
        });

        await scene.StepAsync("stereo-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await SaveTypeAsync(scene);
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("edit-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Settings");
            var item = page.Locator("#left-nav a").Filter(new() { HasTextString = "Announcement Banner" }).First;
            await item.WaitForAsync();
            await scene.HighlightAsync(item);
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#left-nav a").Filter(new() { HasTextString = "Announcement Banner" }).First);
            await scene.TypeAsync(page.GetByLabel("Banner Text", new() { Exact = true }), "The spring sale starts on Friday.", delay: 40);
            await page.Keyboard.PressAsync("Tab");
            await scene.ClickAsync(page.GetByLabel("Show Banner", new() { Exact = true }), pauseAfter: 400);
            await GoAsync(scene, page.Locator("button.btn.btn-primary[type='submit'], button.primaryAction").Filter(new() { HasTextString = "Save" }).First);
            await page.GetByLabel("Banner Text", new() { Exact = true }).WaitForAsync();
        });

        await scene.StepAsync("edit-3", async () =>
        {
            var message = page.Locator(".message, .alert, .toast").Filter(new() { HasTextString = "Site settings updated successfully." }).First;
            if (await message.CountAsync() > 0)
            {
                await ShowAsync(scene, message, 2500);
            }

            await ShowAsync(scene, page.GetByLabel("Banner Text", new() { Exact = true }), 1500);
            await scene.HighlightAsync(page.GetByLabel("Show Banner", new() { Exact = true }));
        });
    }

    private static async Task PermissionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("perm-1");

        await scene.StepAsync("perm-2", async () =>
        {
            await OpenAdminGroupAsync(page, "Access Control");
            await GoAsync(scene, Nav(page, "/Admin/Roles/Index"));
            await GoAsync(scene, Row(page, "Editor").Locator("a.edit").First);
            await page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("perm-3", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("Announcement Banner", new() { Delay = 45 });
            var row = page.Locator("tr[data-text*='Announcement Banner']").First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("perm-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("input[id='Checkbox.ManageCustomSettings_AnnouncementBanner']"), pauseAfter: 400);
            await GoAsync(scene, page.Locator("button.save[value='Save'][name='submit']").First);
            await WaitTextAsync(page, "Role updated successfully.");
        });
    }

    private static async Task DeployAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAdminAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.StepAsync("deploy-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await OpenAdminGroupAsync(page, "Deployments");
            await GoAsync(scene, page.Locator("#left-nav a[href$='/Admin/DeploymentPlan/Index']").First);
            await GoAsync(scene, page.Locator("a[href*='/Admin/DeploymentPlan/Display/']").Filter(new() { HasTextString = "Manage Steps" }).First);
            await page.GetByText("Nothing here!").First.WaitForAsync();
        });

        await scene.StepAsync("deploy-2", async () =>
        {
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add Step" }).First, pauseAfter: 600);
            var filter = page.Locator(".modal.show input[type='text'], .modal.show input[type='search']").First;
            await scene.ClickAsync(filter, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Custom Settings", new() { Delay = 45 });
            var card = page.Locator(".deployment-step-item").Filter(new() { Has = page.Locator("h4.card-title", new() { HasTextString = "Custom Settings" }) }).First;
            await card.WaitForAsync();
            await GoAsync(scene, card.GetByRole(AriaRole.Link, new() { Name = "Add" }).First);
            await page.GetByLabel("Include all custom settings.").WaitForAsync();
        });

        await scene.StepAsync("deploy-3", async () =>
        {
            await ShowAsync(scene, page.GetByLabel("Include all custom settings.").Locator("xpath=ancestor::*[contains(@class,'form-check')][1]"), 3000);
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Create", Exact = true }).First);
            await WaitTextAsync(page, "Deployment plan step added successfully.");
        });

        await scene.StepAsync("deploy-4", async () =>
        {
            var row = page.Locator("li.list-group-item").Filter(new() { HasTextString = "Custom Settings" }).First;
            await scene.HighlightAsync(row);
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

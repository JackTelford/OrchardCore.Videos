using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-roles", RolesAsync),
        ("03-create", CreateAsync),
        ("04-permissions", PermissionsAsync),
        ("05-assign", AssignAsync),
        ("06-delete", DeleteAsync),
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

    // Creates the role Guest Blogger (it is deleted in the last clip), outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/Roles/Index", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        if (await page.Locator(".roles [data-filter-value=\"guest blogger\"]").CountAsync() == 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/Roles/Create", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#RoleName").FillAsync("Guest Blogger");
            await page.Locator("#RoleDescription").FillAsync("Writes posts for the blog as a guest.");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button.create").ClickAsync(), new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.NetworkIdle });
        }

        // Warm up the pages of the clips, so that the first take is not slowed down by compilation.
        foreach (var path in new[] { "/Admin", "/Admin/Roles/Edit/Anonymous", "/Admin/Roles/Edit/Administrator", "/Admin/Users/Index", "/Admin/Users/Create" })
        {
            await page.GotoAsync(BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        }

        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // Opens the page with the browser's storage cleared (the admin menu starts closed).
    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    // Moves to a link or button that navigates and clicks it; the page load is cut off camera.
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        await Task.Delay(180);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 400);
    }

    private static ILocator Role(IPage page, string name) => page.Locator($".roles [data-filter-value=\"{name}\"]");

    private static ILocator MenuItem(IPage page, string text)
        => page.Locator("ul[data-title=\"Access Control\"] a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{text}\\s*$") });

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("figcaption#accessControl button").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("roles-1", async () =>
        {
            await scene.ClickAsync(page.Locator("figcaption#accessControl button"));
            await page.Locator("ul[data-title=\"Access Control\"]").WaitForAsync();
            await GoAsync(scene, MenuItem(page, "Roles"));
            await Role(page, "administrator").WaitForAsync();
        });

        await scene.StepAsync("roles-2", async () =>
        {
            foreach (var name in new[] { "administrator", "anonymous", "authenticated" })
            {
                await scene.HighlightAsync(Role(page, name).Locator(".badge"));
                await scene.HoldAsync(1.5);
                await scene.ClearHighlightsAsync();
            }

            await scene.MoveToAsync(Role(page, "editor").Locator("a.delete"));
            await scene.HighlightAsync(Role(page, "editor").Locator("a.delete"));
        });

        await scene.StepAsync("roles-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, Role(page, "administrator").Locator("a.edit"));
            await page.Locator(".alert.alert-info").WaitForAsync();
            await scene.HighlightAsync(page.Locator(".alert.alert-info"));
        });

        await scene.StepAsync("roles-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.cancel"));
            await Role(page, "anonymous").WaitForAsync();
            await GoAsync(scene, Role(page, "anonymous").Locator("a.edit"));
            await page.Locator("fieldset.permissions-list").First.WaitForAsync();
        });

        await scene.StepAsync("roles-5", async () =>
        {
            await GoAsync(scene, page.Locator("a.cancel"));
            await Role(page, "administrator").WaitForAsync();
            await scene.MoveToAsync(page.Locator("a.btn.create"));
            await scene.HighlightAsync(page.Locator("a.btn.create"));
        });

        await scene.StepAsync("roles-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(Role(page, "editor").Locator(".admin-list-actions"));
            await scene.HighlightAsync(Role(page, "editor").Locator(".admin-list-actions"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Roles/Index");
        await Role(page, "administrator").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.btn.create"));
            await page.Locator("#RoleName").WaitForAsync();
            await scene.MoveToAsync(page.Locator("#RoleName"));
            await scene.HighlightAsync(page.Locator("#RoleName"));
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(page.Locator("#RoleDescription"));
            await scene.HighlightAsync(page.Locator("#RoleDescription"));
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#RoleName"), "Proofreader", 3);
            await scene.TypeAsync(page.Locator("#RoleDescription"), "Corrects posts written by other people.", 3);
            await scene.PauseAsync(300);

            if (await page.Locator("#RoleName").InputValueAsync() != "Proofreader")
            {
                await page.Locator("#RoleName").FillAsync("Proofreader");
            }

            if (await page.Locator("#RoleDescription").InputValueAsync() != "Corrects posts written by other people.")
            {
                await page.Locator("#RoleDescription").FillAsync("Corrects posts written by other people.");
            }
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.MoveToAsync(page.Locator("button.create"));
            await scene.HighlightAsync(page.Locator("button.create"));
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(page.Locator("button.save-configure"));
            await scene.HighlightAsync(page.Locator("button.save-configure"));
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.save-configure"));
            await page.Locator("fieldset.permissions-list").First.WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SearchAsync(Scene scene, string text)
    {
        var box = scene.Page.Locator("#search-box");
        await box.PressAsync("Escape");
        await scene.TypeAsync(box, text, 3);

        if (await box.InputValueAsync() != text)
        {
            await box.FillAsync(text);
        }

        await scene.PauseAsync(300);
    }

    private static async Task HoverEffectiveAsync(Scene scene, string rowText, bool required)
    {
        var page = scene.Page;
        var box = page.Locator($"tr[data-text=\"{rowText}\"] div.form-check[data-bs-toggle=\"tooltip\"]");
        await scene.MoveToAsync(box);

        try
        {
            await page.Locator(".tooltip.show").WaitForAsync(new() { Timeout = required ? 8000 : 2000 });
        }
        catch (TimeoutException) when (!required)
        {
        }
    }

    private static async Task PermissionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Roles/Edit/Proofreader");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("perms-1", async () =>
        {
            var group = page.Locator("fieldset.permissions-list").First;
            await scene.MoveToAsync(group.Locator("legend"));
            await scene.HighlightAsync(group.Locator("legend"));
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(group.Locator("thead"));
        });

        await scene.StepAsync("perms-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(page.Locator("div.bg-info-subtle"));
            await scene.HighlightAsync(page.Locator("div.bg-info-subtle"));
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#search-box"), "view all content", 3);
            await scene.PauseAsync(300);
            await HoverEffectiveAsync(scene, "View all content", false);
        });

        await scene.StepAsync("perms-3", async () =>
        {
            await SearchAsync(scene, "access admin");
            await scene.ClickAsync(page.Locator("[id=\"Checkbox.AccessAdminPanel\"]"));
        });

        await scene.StepAsync("perms-4", async () =>
        {
            await SearchAsync(scene, "list content");
            await scene.ClickAsync(page.Locator("[id=\"Checkbox.ListContent\"]"));
            await SearchAsync(scene, "edit content for others");
            await scene.ClickAsync(page.Locator("[id=\"Checkbox.EditContent\"]"));
        });

        await scene.StepAsync("perms-5", async () =>
        {
            await SearchAsync(scene, "manage roles");
            var badge = page.Locator("tr[data-text=\"Manage Roles\"] .badge").First;
            await scene.HighlightAsync(badge);
            await scene.MoveToAsync(badge);

            try
            {
                await page.Locator(".tooltip.show").WaitForAsync(new() { Timeout = 2000 });
            }
            catch (TimeoutException)
            {
            }
        });

        await scene.StepAsync("perms-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.Locator("#search-box").PressAsync("Escape");
            await GoAsync(scene, page.Locator("button.save"));
            await Role(page, "proofreader").WaitForAsync();
        });

        await scene.StepAsync("perms-7", async () =>
        {
            await GoAsync(scene, Role(page, "proofreader").Locator("a.edit"));
            await page.Locator("#search-box").WaitForAsync();
            await scene.PauseAsync(600);
            await scene.TypeAsync(page.Locator("#search-box"), "view all content", 3);
            await scene.PauseAsync(300);
            await HoverEffectiveAsync(scene, "View all content", true);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AssignAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await page.Locator("figcaption#accessControl button").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("assign-1", async () =>
        {
            await scene.ClickAsync(page.Locator("figcaption#accessControl button"));
            await page.Locator("ul[data-title=\"Access Control\"]").WaitForAsync();
            await GoAsync(scene, MenuItem(page, "Users"));
            await page.Locator("a.btn.create").WaitForAsync();
            await GoAsync(scene, page.Locator("a.btn.create"));
            await page.GetByLabel("User name", new() { Exact = true }).WaitForAsync();
        });

        await scene.StepAsync("assign-2", async () =>
        {
            await scene.PauseAsync(400);
            await scene.TypeAsync(page.GetByLabel("User name", new() { Exact = true }), "priya.raman", 3);
            await scene.TypeAsync(page.GetByLabel("Email", new() { Exact = true }), "priya.raman@example.com", 3);

            if (await page.GetByLabel("User name", new() { Exact = true }).InputValueAsync() != "priya.raman")
            {
                await page.GetByLabel("User name", new() { Exact = true }).FillAsync("priya.raman");
            }

            if (await page.GetByLabel("Email", new() { Exact = true }).InputValueAsync() != "priya.raman@example.com")
            {
                await page.GetByLabel("Email", new() { Exact = true }).FillAsync("priya.raman@example.com");
            }

            await scene.ClickAsync(page.Locator(".password-generator-button"));
        });

        await scene.StepAsync("assign-3", async () =>
        {
            await scene.ScrollAsync(0, 400);
            var field = page.Locator("label.ocat-label", new() { HasTextString = "Roles" });
            await scene.HighlightAsync(field);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("label.form-check-label", new() { HasTextString = "Proofreader" }));
        });

        await scene.StepAsync("assign-4", async () =>
        {
            await GoAsync(scene, page.Locator("button.save"));
            await page.Locator("text=priya.raman").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".badge:has-text(\"Proofreader\")").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DeleteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Roles/Index");
        await Role(page, "guest blogger").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("delete-1", async () =>
        {
            var delete = Role(page, "guest blogger").Locator("a.delete");
            await scene.MoveToAsync(delete);
            await scene.HighlightAsync(delete);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(delete);
            await page.Locator("#confirmRemoveModal").WaitForAsync();
        });

        await scene.StepAsync("delete-2", async () =>
        {
            await scene.MoveToAsync(page.Locator("#modalOkButton"));
            await scene.HoldAsync(2.2);
            await GoAsync(scene, page.Locator("#modalOkButton"));
            await page.Locator(".roles [data-filter-value=\"administrator\"]").WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }
}

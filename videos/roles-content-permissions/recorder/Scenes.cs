using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Content Permissions: By Content Type and by Owner". Each clip starts from the state the previous one
/// left (see recording-plan.md): 02-securable makes Article securable, 03-role creates the role Writer, 04-editor only
/// reads, 05-writer is recorded as the user jamie.chen (state-jamie.json).
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    public static string JamieStatePath => Path.Combine(Root, "state-jamie.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-securable", SecurableAsync),
        ("03-role", RoleAsync),
        ("04-editor", EditorAsync),
        ("05-writer", WriterAsync),
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

    // VIDEO_PREP=seed: creates the article Press Kit (owner admin). VIDEO_PREP=user: creates the user jamie.chen with the
    // role Writer and writes state-jamie.json. Both run outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var mode = Environment.GetEnvironmentVariable("VIDEO_PREP") ?? "seed";
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        if (mode == "seed")
        {
            await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Article/Create", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#TitlePart_Title").FillAsync("Press Kit");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button.publish").ClickAsync(), new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Warm up the pages of the clips.
            foreach (var path in new[] { "/Admin/ContentTypes/List", "/Admin/ContentTypes/Edit/Article", "/Admin/Roles/Index", "/Admin/Roles/Create", "/Admin/Roles/Edit/Editor", "/Admin/Contents/ContentItems/Article" })
            {
                await page.GotoAsync(BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            }
        }
        else
        {
            await page.GotoAsync(BaseUrl + "/Admin/Users/Create", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.GetByLabel("User name", new() { Exact = true }).FillAsync("jamie.chen");
            await page.GetByLabel("Email", new() { Exact = true }).FillAsync("jamie.chen@example.com");
            await page.Locator("input[type=password]").Nth(0).FillAsync(Password);
            await page.Locator("input[type=password]").Nth(1).FillAsync(Password);
            await page.Locator("label.form-check-label", new() { HasTextString = "Writer" }).ClickAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button.save").ClickAsync(), new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await context.CloseAsync();

            var jamie = await browser.NewContextAsync();
            var login = await jamie.NewPageAsync();
            login.SetDefaultTimeout(60_000);
            await login.GotoAsync(BaseUrl + "/Login");
            await login.Locator("#LoginForm_UserName").FillAsync("jamie.chen");
            await login.Locator("#LoginForm_Password").FillAsync(Password);
            await login.Locator("#LoginForm_Password").PressAsync("Enter");
            await login.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));

            // Warm up the pages of the last clip.
            foreach (var path in new[] { "/Admin/Contents/ContentItems/Article", "/Admin/Contents/ContentTypes/Article/Create" })
            {
                await login.GotoAsync(BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            }

            await jamie.StorageStateAsync(new BrowserContextStorageStateOptions { Path = JamieStatePath });
            await jamie.CloseAsync();

            return;
        }

        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string statePath = null)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, statePath ?? StatePath, Path.GetDirectoryName(directory), durations);

    // Opens the page with the browser's storage cleared (the Save menu remembers its last entry there).
    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    // Moves to a link or button that navigates and clicks it; the page load is cut off camera.
    private static async Task GoAsync(Scene scene, ILocator locator, ILocator ready = null)
    {
        await scene.MoveToAsync(locator);
        await Task.Delay(180);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y));

            if (ready is not null)
            {
                await ready.WaitForAsync();
                await Task.Delay(700);
            }
        }, 400);
    }

    // Types into a box and checks the value afterwards.
    private static async Task TypeIntoAsync(Scene scene, ILocator box, string text, int delay = 3)
    {
        await scene.TypeAsync(box, text, delay);

        if (await box.InputValueAsync() != text)
        {
            await box.FillAsync(text);
        }

        await scene.PauseAsync(300);
    }

    // Scrolls so that the element is near the top of the page area.
    private static async Task ScrollToAsync(Scene scene, ILocator locator, double top = 130)
    {
        await scene.ClearHighlightsAsync();
        var y = await locator.EvaluateAsync<double>("e => e.getBoundingClientRect().top");
        var delta = y - top;

        if (Math.Abs(delta) > 20)
        {
            await scene.ScrollAsync(0, delta);
            await scene.PauseAsync(400);
        }
    }

    private static async Task SearchAsync(Scene scene, string text)
    {
        var box = scene.Page.Locator("#search-box");
        await box.PressAsync("Escape");
        await TypeIntoAsync(scene, box, text);
    }

    private static ILocator Permission(IPage page, string text) => page.Locator($"tr[data-text=\"{text}\"]");

    private static async Task SecurableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/ContentTypes/List");
        await page.Locator("#search-box").WaitForAsync();
        await page.Locator("a[role=\"btn-edit-Article\"]").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("securable-1", async () =>
        {
            await TypeIntoAsync(scene, page.Locator("#search-box"), "Article");
            await page.Locator("a[role=\"btn-edit-BlogPost\"]").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await scene.MoveToAsync(page.Locator("a[role=\"btn-edit-Article\"]"));
            await scene.HighlightAsync(page.Locator(".list-group-item:has(a[role=\"btn-edit-Article\"])"));
        });

        await scene.StepAsync("securable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a[role=\"btn-edit-Article\"]"));
            await page.Locator("label:text-is(\"Securable\")").WaitForAsync();
            await scene.ClickAsync(page.Locator("label:text-is(\"Securable\")"));
            await scene.HighlightAsync(page.Locator(".form-check:has(label:text-is(\"Securable\"))"));
        });

        await scene.StepAsync("securable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.save").First);
            await page.Locator("#btnCreate").WaitForAsync();
            await scene.HighlightAsync(page.Locator("#btnCreate"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RoleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Roles/Index");
        await page.Locator("a.create").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("role-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First);
            await page.Locator("#RoleName").WaitForAsync();
        });

        await scene.StepAsync("role-2", async () =>
        {
            await TypeIntoAsync(scene, page.Locator("#RoleName"), "Writer");
            await TypeIntoAsync(scene, page.Locator("#RoleDescription"), "Writes and publishes articles.");
            await GoAsync(scene, page.Locator("button.save-configure"));
            await page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("role-3", async () =>
        {
            await scene.MoveToAsync(page.Locator("div.bg-info-subtle"));
            await scene.HighlightAsync(page.Locator("div.bg-info-subtle"));
        });

        await scene.StepAsync("role-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TypeIntoAsync(scene, page.Locator("#search-box"), "admin panel");
            await Permission(page, "Access admin panel").WaitForAsync();
            await scene.ClickAsync(page.Locator("[id=\"Checkbox.AccessAdminPanel\"]"));
        });

        await scene.StepAsync("role-5", async () =>
        {
            await SearchAsync(scene, "Article");
            await page.Locator("fieldset.permissions-list legend", new() { HasTextString = "Article Content Type" }).First.WaitForAsync();
            await ScrollToAsync(scene, page.Locator("fieldset.permissions-list:visible legend").First, 150);
            await scene.HighlightAsync(page.Locator("fieldset.permissions-list:visible legend").First);
        });

        await scene.StepAsync("role-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[id=\"Checkbox.PublishOwn_Article\"]"));
            await scene.ClickAsync(page.Locator("[id=\"Checkbox.ListContent_Article\"]"));
            await scene.HighlightAsync(Permission(page, "Publish or unpublish Article"));
            await scene.HighlightAsync(Permission(page, "List Article content items"));
            await scene.MoveToAsync(Permission(page, "Publish or unpublish Article for others"));
        });

        await scene.StepAsync("role-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator(".btn-group[data-action-group=\"oc-role-save-action\"] .dropdown-toggle-split"));
            await scene.MoveToAsync(page.Locator(".save-continue"));
            await Task.Delay(180);
            var (x, y) = await scene.CenterOfAsync(page.Locator(".save-continue"));
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 400);
            await page.Locator("#search-box").WaitForAsync();
            await TypeIntoAsync(scene, page.Locator("#search-box"), "Article");
            await ScrollToAsync(scene, page.Locator("fieldset.permissions-list:visible legend").First, 150);
        });

        await scene.StepAsync("role-8", async () =>
        {
            var edit = page.Locator("tr[data-text=\"Edit Article\"] td:nth-child(3) .form-check");
            await scene.MoveToAsync(edit);
            await scene.PauseAsync(2500);
            var view = page.Locator("tr[data-text=\"View Article by others\"] td:nth-child(3) .form-check");
            await scene.MoveToAsync(view);
            await page.Locator(".tooltip.show").WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EditorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Roles/Index");
        await page.Locator("[data-filter-value=\"editor\"] a.edit").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("editor-1", async () =>
        {
            await GoAsync(scene, page.Locator("[data-filter-value=\"editor\"] a.edit"), page.Locator("#search-box"));
            await page.Locator("#search-box").FillAsync("Articl");
            await scene.TypeAsync(page.Locator("#search-box"), "e", 2);
            await scene.PauseAsync(300);
        });

        await scene.StepAsync("editor-2", async () =>
        {
            await ScrollToAsync(scene, page.Locator("fieldset.permissions-list:visible legend").First, 150);
            await scene.HighlightAsync(page.Locator("fieldset.permissions-list:visible table").First, padding: 2);
        });

        await scene.StepAsync("editor-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await SearchAsync(scene, "content for others");
            await Permission(page, "Edit content for others").WaitForAsync();
            await ScrollToAsync(scene, page.Locator("fieldset.permissions-list:visible legend").First, 150);
            await scene.HighlightAsync(Permission(page, "Edit content for others"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WriterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, JamieStatePath);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Contents/ContentItems/Article");
        await page.Locator("a.btn:has-text(\"New Article\")").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("writer-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("a.btn:has-text(\"New Article\")"));
            await scene.HoldAsync(1.5);
            await scene.MoveToAsync(Row(page, "Press Kit"));
            await scene.HoldAsync(1.0);
            await scene.MoveToAsync(Row(page, "About"));
        });

        await scene.StepAsync("writer-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.btn:has-text(\"New Article\")"));
            await page.Locator("#TitlePart_Title").WaitForAsync();
            await scene.PauseAsync(500);
            await TypeIntoAsync(scene, page.Locator("#TitlePart_Title"), "Team Offsite Notes", 30);
            await scene.HighlightAsync(page.Locator("button.publish"));
            await scene.HoldAsync(1.2);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.publish"));
            await Row(page, "Team Offsite Notes").WaitForAsync();
        });

        await scene.StepAsync("writer-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(Row(page, "Team Offsite Notes").Locator("a.edit"));
            await scene.HighlightAsync(Row(page, "Team Offsite Notes"));
        });

        await scene.StepAsync("writer-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(page, "About"));
            await scene.HighlightAsync(Row(page, "Press Kit"));
        });

        await scene.StepAsync("writer-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(Row(page, "Team Offsite Notes"));
            await scene.HoldAsync(1.0);
            await scene.MoveToAsync(Row(page, "Press Kit"));
            await scene.HoldAsync(1.0);
            await scene.MoveToAsync(Row(page, "About"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static ILocator Row(IPage page, string title)
        => page.Locator("li.list-group-item").Filter(new() { HasTextString = title });
}

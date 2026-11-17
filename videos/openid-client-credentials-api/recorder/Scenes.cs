using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The client secret of the application, typed in 04-app (a password field, so the capture shows dots only). Chosen outside git.
    private static string ClientSecret => Environment.GetEnvironmentVariable("VIDEO_CLIENT_SECRET") ?? throw new InvalidOperationException("Set VIDEO_CLIENT_SECRET.");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-site", SiteAsync),
        ("03-role", RoleAsync),
        ("04-app", AppAsync),
        ("07-result", ResultAsync),
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

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.SetDefaultTimeout(120000);
        return scene;
    }

    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    private static ILocator Group(IPage page, string title) => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator Items(IPage page, string title) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"{title}\"]");

    private static ILocator Row(IPage page, string id) => page.Locator($"[data-filter-value*=\"{id}\"]:visible").First;

    // A short pointer move and a click, for steps that must fit a short narration on a loaded machine.
    private static async Task QuickClickAsync(Scene scene, ILocator locator)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await scene.PauseAsync(300);
    }

    // Clicks a link whose page is slow to load: the pointer moves on camera, the load is cut out.
    private static async Task GoAsync(Scene scene, ILocator link)
    {
        await link.WaitForAsync();
        await scene.MoveToAsync(link);
        await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), 300);
    }

    // Opens <group> > <sub> > <leaf> of the admin menu, expanding what is collapsed, and loads the page off camera.
    private static async Task OpenMenuAsync(Scene scene, string group, string sub, string leaf)
    {
        var page = scene.Page;
        var items = Items(page, group);
        var link = items.Locator($"ul[data-title=\"{sub}\"] a.item-label:has-text(\"{leaf}\")").First;
        if (!await link.IsVisibleAsync())
        {
            if (await items.Locator("button.item-label:visible").CountAsync() == 0 && await items.Locator("a.item-label:visible").CountAsync() == 0)
            {
                await QuickClickAsync(scene, Group(page, group));
            }

            if (!await link.IsVisibleAsync())
            {
                await QuickClickAsync(scene, items.Locator($"button.item-label:has(span.title:text-is(\"{sub}\"))").First);
            }
        }

        await GoAsync(scene, link);
    }

    // Types text and checks that it arrived (typing right after a load can be lost).
    private static async Task TypeVerifiedAsync(Scene scene, ILocator field, string text, int delay = 2)
    {
        await field.WaitForAsync();
        await scene.TypeAsync(field, text, delay);
        if (await field.InputValueAsync() != text)
        {
            await field.FillAsync(text);
            await scene.PauseAsync(300);
        }
    }

    private static async Task SearchAsync(Scene scene, string text)
    {
        var box = scene.Page.Locator("#search-box");
        await box.WaitForAsync();
        await scene.PauseAsync(300);
        await box.FocusAsync();
        await scene.Page.Keyboard.PressAsync("Control+A");
        await scene.TypeAsync(box, text, 2);
        await scene.PauseAsync(250);
        if (await box.InputValueAsync() != text)
        {
            await box.FillAsync(text);
            await scene.PauseAsync(250);
        }

        if (scene.Page.Url.Contains("/Roles/Edit") && await scene.Page.Locator("tr[data-text]:visible").CountAsync() == 0)
        {
            // The roles page filter can miss the last keystrokes of a fast typist: a harmless key makes it filter again.
            await box.PressAsync("End");
            await scene.PauseAsync(250);
        }
    }

    private static async Task ScrollToAsync(Scene scene, ILocator target, double top = 160, bool smooth = true)
    {
        await target.WaitForAsync();
        var delta = await target.EvaluateAsync<double>("(e, top) => { const y = Math.max(0, Math.min(e.getBoundingClientRect().top + window.scrollY - top, document.documentElement.scrollHeight - window.innerHeight)); return y - window.scrollY; }", top);
        if (smooth && Math.Abs(delta) > 4)
        {
            await scene.ScrollAsync(0, delta, 8);
        }
        else
        {
            await scene.Page.EvaluateAsync("(d) => window.scrollBy(0, d)", delta);
            await scene.PauseAsync(300);
        }
    }

    // Enables the feature of a row of the Features page: the click is late in the step, the restart is waited off camera.
    private static async Task EnableFeatureAsync(Scene scene, string id)
    {
        var page = scene.Page;
        var enable = page.Locator($"#btn-enable-{id.Replace('.', '_')}");
        await scene.MoveToAsync(enable);
        await scene.OffCameraAsync(async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(enable);
            await page.Locator($"[data-filter-value*=\"{id}\"]:visible").First.WaitForAsync();
            await page.Locator("#search-box").WaitForAsync();
            await Task.Delay(800);
        }, 300);
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Features");
        await page.Locator("#search-box").WaitForAsync();
        await page.Locator("#left-nav button.nav-group-label").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            await SearchAsync(scene, "OpenID");
            var row = Row(page, "OrchardCore.OpenId.Server");
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
            await scene.HoldAsync(1.0);
            await scene.ClearHighlightsAsync();
            await EnableFeatureAsync(scene, "OrchardCore.OpenId.Server");
            await SearchAsync(scene, "OpenID");
            await Row(page, "OrchardCore.OpenId.Server").Locator("a:has-text(\"Disable\"), button:has-text(\"Disable\")").First.WaitForAsync();
        });

        await scene.StepAsync("site-2", async () =>
        {
            var row = Row(page, "OrchardCore.OpenId.Validation");
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
            await scene.HoldAsync(1.0);
            await scene.ClearHighlightsAsync();
            await EnableFeatureAsync(scene, "OrchardCore.OpenId.Validation");
            await SearchAsync(scene, "OpenID");
            await Row(page, "OrchardCore.OpenId.Validation").Locator("a:has-text(\"Disable\"), button:has-text(\"Disable\")").First.WaitForAsync();
        });

        await scene.StepAsync("site-3", async () =>
        {
            await OpenMenuAsync(scene, "Settings", "OpenID Connect", "Authorization Server");
            await page.Locator("[data-openid-field=\"EnableTokenEndpoint\"]").WaitForAsync();
            await scene.PauseAsync(300);
            await scene.HighlightAsync(page.Locator("[data-openid-field=\"EnableTokenEndpoint\"]"));
            await scene.HoldAsync(0.9);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, page.Locator("h3:text-is(\"Flows\")").First, 130);
            var flow = page.Locator("[data-openid-field=\"AllowClientCredentialsFlow\"]");
            await flow.WaitForAsync();
            await scene.ClickAsync(flow);
            var save = page.Locator(".action-bar-bottom button.btn-primary:visible").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(save);
                await page.Locator("h3:text-is(\"Flows\")").First.WaitForAsync();
                await page.Locator("[data-openid-field=\"AllowClientCredentialsFlow\"]").WaitForAsync();
                await Task.Delay(800);
            }, 300);
            await ScrollToAsync(scene, page.Locator("h3:text-is(\"Flows\")").First, 130, false);
        });

        await scene.StepAsync("site-4", async () =>
        {
            await OpenMenuAsync(scene, "Settings", "OpenID Connect", "Token Validation");
            var tenant = page.Locator(".openid-validation-tenant").First;
            await tenant.WaitForAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(tenant);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RoleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Roles/Index");
        await page.Locator("a.create").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("role-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First);
            await page.Locator("#RoleName").WaitForAsync();
            await scene.PauseAsync(600);
            await TypeVerifiedAsync(scene, page.Locator("#RoleName"), "Content API Client", 20);
            await scene.HoldAsync(0.5);
            await GoAsync(scene, page.Locator("button.save-configure"));
            await page.Locator("#search-box").WaitForAsync();
            await page.Locator("h1, h2").Filter(new() { HasText = "Content API Client" }).First.WaitForAsync();
        });

        await scene.StepAsync("role-2", async () =>
        {
            await SearchAsync(scene, "api");
            var access = page.Locator("[id=\"Checkbox.AccessContentApi\"]");
            await access.WaitForAsync();
            await scene.ClickAsync(access, pauseAfter: 200);
            await SearchAsync(scene, "publish");
            var publish = page.Locator("[id=\"Checkbox.PublishContent\"]");
            await publish.WaitForAsync();
            await ScrollToAsync(scene, page.Locator("tr[data-text=\"Publish or unpublish content for others\"]"), 260, false);
            await scene.ClickAsync(publish, pauseAfter: 200);
        });

        await scene.StepAsync("role-3", async () =>
        {
            await GoAsync(scene, page.Locator("button.save").First);
            await page.Locator("li, tr, .list-group-item").Filter(new() { HasText = "Content API Client" }).First.WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AppAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/OpenId/Application");
        await page.Locator("a.create").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("app-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First);
            await page.Locator("#DisplayName").WaitForAsync();
            await page.Locator("#AllowClientCredentialsFlowFieldSet").WaitForAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(page.Locator("#DisplayName"));
            await scene.HighlightAsync(page.Locator("#Type"));
            await scene.HighlightAsync(page.Locator("[data-openid-field=\"ClientId\"]"));
            await scene.HoldAsync(1.5);
        });

        await scene.StepAsync("app-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TypeVerifiedAsync(scene, page.Locator("#DisplayName"), "Content Sync Job");
            await TypeVerifiedAsync(scene, page.Locator("[data-openid-field=\"ClientId\"]"), "content-sync");
            await scene.HighlightAsync(page.Locator("#Type"));
            var secret = page.Locator("[data-openid-field=\"ClientSecret\"]");
            await secret.WaitForAsync();
            await TypeVerifiedAsync(scene, secret, ClientSecret, 2);
        });

        await scene.StepAsync("app-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, page.Locator("h3:text-is(\"Flows\")").First, 130);
            await scene.ClickAsync(page.Locator("#AllowClientCredentialsFlow"));
            await page.Locator("#RoleGroup").WaitForAsync();
            await ScrollToAsync(scene, page.Locator("#RoleGroup"), 330);
        });

        await scene.StepAsync("app-4", async () =>
        {
            await scene.ClickAsync(page.Locator("#RoleGroup label:text-is(\"Content API Client\")").First);
            await scene.HoldAsync(0.8);
            await GoAsync(scene, page.Locator("button.save").First);
            await page.Locator("li.list-group-item").Filter(new() { HasText = "Content Sync Job" }).First.WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Contents/ContentItems");
        var title = page.Locator("a:text-is(\"Hello from the sync job\")").First;
        await title.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("result-1", async () =>
        {
            await scene.MoveToAsync(title);
            await scene.HighlightAsync(title);
            await scene.HoldAsync(2.0);
        });

        await scene.StepAsync("result-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, title);
            var field = page.GetByLabel("Title", new() { Exact = true });
            await field.WaitForAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(field);
        });

        await scene.ClearHighlightsAsync();
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Tenant feature profiles", recorded in order on a site with the Default, Bluebird and Maplewood tenants.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string Rules = "[\n  {\n    \"Rule\": \"Exclude\",\n    \"Expression\": \"OrchardCore.Workflows\"\n  },\n  {\n    \"Rule\": \"Exclude\",\n    \"Expression\": \"OrchardCore.Email.*\"\n  },\n  {\n    \"Rule\": \"Include\",\n    \"Expression\": \"OrchardCore.Email.Smtp\"\n  }\n]";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-enable", EnableAsync),
        ("04-create", CreateAsync),
        ("05-assign", AssignAsync),
        ("06-effect", EffectAsync),
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

    /// <summary>Gives the Bluebird and Maplewood tenants a description, outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var (tenant, description) in new[] { ("Bluebird", "Bluebird Bakery website"), ("Maplewood", "Maplewood Garden Center website") })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Tenants");
            var row = page.Locator("li.list-group-item").Filter(new() { Has = page.Locator($"a.text-break:text-is('{tenant}')") }).Last;
            await row.Locator("a.edit").First.ClickAsync();
            await page.Locator("#Description").FillAsync(description);
            await page.Locator("button.btn-primary:has-text('Save'), button.save").First.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        foreach (var path in new[] { "/Admin/Features", "/Admin/Features/Bluebird", "/Admin/Features/Maplewood" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Tenants described.");
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

    private static ILocator Group(IPage page, string key) => page.Locator($"button.nav-group-label.oc-navigation-admin-{key}");

    private static ILocator Row(IPage page, string tenant) => page.Locator("li.list-group-item").Filter(new() { Has = page.Locator($"a.text-break:text-is('{tenant}')") }).Last;

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1500, string caption = null)
    {
        await scene.HighlightAsync(locator, caption);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(Group(page, "tools"), pauseAfter: 250);
            await GoAsync(scene, page.Locator("a[href$='/Admin/Features'] >> visible=true").First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "Tenant Feature Profiles", delay: 25);
            await page.Locator("#btn-enable-OrchardCore_Tenants_FeatureProfiles").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("[data-filter-value*='Tenant Feature Profiles'] code").First, "Feature ID");
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_Tenants_FeatureProfiles"));
            await WaitTextAsync(page, "The feature Tenant Feature Profiles was enabled.");
            await scene.ClickAsync(Group(page, "multi_tenancy"), pauseAfter: 300);
            await scene.HighlightAsync(page.Locator("a[href$='/Admin/TenantFeatureProfiles'] >> visible=true").First);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(Group(page, "multi_tenancy"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("a[href$='/Admin/TenantFeatureProfiles'] >> visible=true").First);
            await ShowAsync(scene, page.GetByText("There are no feature profiles for the moment.").First, 1500);
            await GoAsync(scene, page.Locator("a.btn.create, a:has-text('Add Feature Profile')").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(page.Locator("input[name='Name']"), "Basic sites", delay: 40);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.HighlightAsync(page.Locator(".monaco-json-model-editor-container").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            var hint = page.Locator(".monaco-json-model-editor span.hint").First;
            await hint.ScrollToMiddleAsync();
            await scene.HighlightAsync(hint);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var editor = page.Locator(".monaco-json-model-editor-container").First;
            await editor.ScrollToMiddleAsync();
            await scene.ClickAsync(editor, pauseAfter: 200);
            var done = await page.EvaluateAsync<bool>("t => { if (!window.monaco) return false; const m = window.monaco.editor.getModels(); if (!m.length) return false; m[0].setValue(t); return true; }", Rules);

            if (!done)
            {
                await page.Keyboard.InsertTextAsync("[{\"Rule\":\"Exclude\",\"Expression\":\"OrchardCore.Workflows\"},{\"Rule\":\"Exclude\",\"Expression\":\"OrchardCore.Email.*\"},{\"Rule\":\"Include\",\"Expression\":\"OrchardCore.Email.Smtp\"}]");
                await page.Keyboard.PressAsync("Shift+Alt+F");
            }

            await scene.PauseAsync(800);
        });

        await scene.StepAsync("create-5", async () =>
        {
            var save = page.Locator("button.save[value='Save']").First;
            await save.ScrollToMiddleAsync();
            await GoAsync(scene, save);
            await page.Locator("li.list-group-item a.btn:text-is('Edit')").First.WaitForAsync();
        });

        await scene.StepAsync("create-6", async () =>
        {
            await GoAsync(scene, page.Locator("li.list-group-item a.btn:text-is('Edit')").First);
            await page.Locator(".monaco-json-model-editor-container .view-lines").First.WaitForAsync();
            await page.GetByText("OrchardCore.Workflows").First.WaitForAsync();
            await scene.PauseAsync(2500);
            await GoAsync(scene, page.Locator("a.btn:text-is('Cancel')").First);
        });
    }

    private static async Task AssignAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Tenants");
        await Group(page, "multi_tenancy").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("assign-1", async () =>
        {
            await ShowAsync(scene, Row(page, "Bluebird"), 2000);
            await GoAsync(scene, Row(page, "Bluebird").Locator("a.edit").First);
        });

        await scene.StepAsync("assign-2", async () =>
        {
            var field = page.Locator("select[name='FeatureProfiles']");
            await field.ScrollToMiddleAsync();
            await ShowAsync(scene, field, 1500);
            await scene.SelectAsync(field, "Basic sites");
            var save = page.Locator("button.save, button.btn-primary:has-text('Save')").First;
            await save.ScrollToMiddleAsync();
            await GoAsync(scene, save);
        });

        await scene.StepAsync("assign-3", async () =>
        {
            await ShowAsync(scene, Row(page, "Bluebird").Locator(".badge[title='Feature Profile']").First, 2500);
            await scene.HighlightAsync(Row(page, "Maplewood"));
        });
    }

    private static async Task EffectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("effect-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "OrchardCore.Workflows", delay: 40);
        });

        await scene.StepAsync("effect-2", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.GotoAsync(BaseUrl + "/Admin/Tenants")), leadMilliseconds: 250);
            await ShowAsync(scene, Row(page, "Bluebird").Locator("a.btn:has-text('Features')").First, 1500);
            await GoAsync(scene, Row(page, "Bluebird").Locator("a.btn:has-text('Features')").First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "OrchardCore.Workflows", delay: 30);
        });

        await scene.StepAsync("effect-3", async () =>
        {
            await page.Locator("#search-box").FillAsync(string.Empty);
            await scene.TypeAsync(page.Locator("#search-box"), "OrchardCore.Email", delay: 40);
        });

        await scene.StepAsync("effect-4", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.GotoAsync(BaseUrl + "/Admin/Tenants")), leadMilliseconds: 250);
            await GoAsync(scene, Row(page, "Maplewood").Locator("a.btn:has-text('Features')").First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "OrchardCore.Workflows", delay: 30);
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the forms.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

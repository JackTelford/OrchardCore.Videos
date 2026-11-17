using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Data Sources", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-consume", ConsumeAsync),
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

    private static async Task WaitSiteAsync()
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        for (var i = 0; i < 60; i++)
        {
            try
            {
                var response = await client.GetAsync(BaseUrl + "/");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(2000);
        }
    }

    /// <summary>Provisioning, outside the capture: Data Pipelines enabled, two users, and an empty pipeline named User directory.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#btn-enable-OrchardCore_DataPipelines").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitSiteAsync();
        await Task.Delay(6000);

        foreach (var (name, email) in new[] { ("jamie.chen", "jamie.chen@example.com"), ("priya.shah", "priya.shah@example.com") })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Users/Create");
            await page.Locator("#User_UserName").FillAsync(name);
            await page.Locator("#User_Email").FillAsync(email);
            await page.Locator("#User_Password").FillAsync("Fake-Pass-0001!");
            await page.Locator("#User_PasswordConfirmation").FillAsync("Fake-Pass-0001!");
            await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).First.ClickAsync();
            await page.WaitForURLAsync(u => !u.Contains("/Create"));
        }

        await page.GotoAsync(BaseUrl + "/Admin/DataPipelines/Create");
        await page.Locator("#Name").FillAsync("User directory");
        await page.GetByRole(AriaRole.Button, new() { Name = "Create", Exact = true }).First.ClickAsync();
        await page.Locator("[data-cy='toolbar-add-data-source']").WaitForAsync();

        foreach (var path in new[] { "/Admin/DataPipelines", "/Admin/Users" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Seeded.");
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

    private static async Task ConsumeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await using var _ = scene;
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/DataPipelines");
        var id = await page.Locator("li[data-pipeline-id]").First.GetAttributeAsync("data-pipeline-id");
        await scene.OpenAsync("/Admin/DataPipelines/" + id + "/Edit");
        await page.Locator("[data-cy='toolbar-add-data-source']").WaitForAsync();
        await page.Locator("[data-cy='start-picker']").WaitForAsync();
        await page.Locator("button.leftbar-compactor").ClickAsync();
        await Task.Delay(800);
        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);

        await scene.StepAsync("consume-1", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-add-data-source']"), pauseAfter: 400);
            await page.Locator("[data-cy='data-set-picker'] .card").First.WaitForAsync();
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("consume-2", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='data-set-category-Users']"), pauseAfter: 400);
            await page.Locator("[data-cy='data-set-Users::Users']").WaitForAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='data-set-Users::Users']"));
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("consume-3", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='data-set-Users::Users'] [data-cy='data-set-choose']"), pauseAfter: 400);
            await page.Locator("[data-cy='step-panel']").WaitForAsync();
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("consume-4", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='step-tab-fields']"), pauseAfter: 400);
            await page.Locator("[data-cy='fields-outputs']").GetByText("User id").First.WaitForAsync();
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("consume-5", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='step-tab-data']"), pauseAfter: 400);
            await scene.ClickAsync(page.Locator("[data-cy='preview-run']"), pauseAfter: 400);
            await page.Locator("[data-cy='preview-tab'] table").WaitForAsync();
            await scene.PauseAsync(600);
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

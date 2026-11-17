using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Data Pipelines", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-build", BuildAsync),
        ("03-run", RunAsync),
        ("04-react", ReactAsync),
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

    /// <summary>Provisioning, outside the capture: Workflows, Data Pipelines and Email enabled, the two pipelines imported and published.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        foreach (var id in new[] { "OrchardCore_Workflows", "OrchardCore_DataPipelines", "OrchardCore_Email" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            await page.Locator("#btn-enable-" + id).ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await WaitSiteAsync();
            await Task.Delay(6000);
        }

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", File.ReadAllText("C:/ocv/s6/dw-recipe.json"));
        await page.Locator("button.import").First.ClickAsync();
        await page.WaitForFunctionAsync("() => document.body.innerText.includes('Recipe imported successfully!')");

        foreach (var pipeline in new[] { "blogpostsexport", "blogpostsbyemail" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/DataPipelines");
            var row = page.Locator("li[data-pipeline-id='" + pipeline + "']");
            var edit = await row.Locator("a[href*='/Edit']").First.GetAttributeAsync("href");
            await page.GotoAsync(BaseUrl + edit);
            await page.Locator("[data-cy='toolbar-publish']").WaitForAsync();
            await Task.Delay(2500);
            await page.Locator("[data-cy='toolbar-publish']").ClickAsync();
            await Task.Delay(2500);
            var confirm = page.Locator(".modal.show button.btn-primary, [role='dialog'] button.btn-primary").First;
            if (await confirm.CountAsync() > 0 && await confirm.IsVisibleAsync())
            {
                Console.WriteLine("Confirm: " + await confirm.InnerTextAsync());
                await confirm.ClickAsync();
            }

            await page.GetByText("Version 1").First.WaitForAsync();
        }

        foreach (var path in new[] { "/Admin/DataPipelines", "/Admin/Workflows/Types", "/Admin/Contents/ContentItems" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Seeded.");
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

    private static async Task CollapseMenuAsync(IPage page)
    {
        await page.Locator("#left-nav").WaitForAsync();
        await page.Locator("button.leftbar-compactor").ClickAsync();
        await Task.Delay(800);
    }

    // Reads the status badge of the first row of the Run History.
    private static async Task<string> FirstRunBadgeAsync(IPage page)
    {
        var html = await page.EvaluateAsync<string>("async () => { const r = await fetch('/Admin/DataPipelines/Runs'); return await r.text(); }");
        var doc = System.Text.RegularExpressions.Regex.Match(html, "<li[^>]*data-run-id[^>]*>.*?</li>", System.Text.RegularExpressions.RegexOptions.Singleline);
        if (!doc.Success)
        {
            return "";
        }

        var badge = System.Text.RegularExpressions.Regex.Match(doc.Value, "<span[^>]*badge[^>]*>\\s*(?:<i[^>]*></i>)?\\s*([A-Za-z]+)");
        return badge.Success ? badge.Groups[1].Value : "";
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

    private static ILocator Panel(IPage page) => page.Locator("[data-cy='panel-activity-form']").First;

    private static async Task BuildAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Workflows/Types", s => CollapseMenuAsync(s.Page));
        var page = scene.Page;

        await scene.StepAsync("build-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First);
            await page.Locator("#Name").WaitForAsync();
            await scene.TypeAsync(page.Locator("#Name"), "Export the blog posts when one is published", delay: 25);
        });

        await scene.StepAsync("build-2", async () =>
        {
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Create and open the designer" }).First);
            await page.Locator("[data-cy='start-picker']").WaitForAsync();
            await scene.ClickAsync(page.Locator("[data-cy='start-picker-ContentPublishedEvent']"), pauseAfter: 400);
            await page.Locator("[data-cy='activity-panel']").WaitForAsync();
            await Panel(page).WaitForAsync();
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("build-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("[data-cy='activity-panel']").GetByText("Content Types").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='start-badge']").First);
        });

        await scene.StepAsync("build-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var close = page.Locator("[data-cy='activity-panel-close']");
            if (await close.CountAsync() > 0 && await close.First.IsVisibleAsync())
            {
                await scene.ClickAsync(close.First, pauseAfter: 300);
            }

            await scene.ClickAsync(page.Locator(".wfd-port[data-outcome='Done']").First, pauseAfter: 400);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "pipeline", delay: 10);
            var item = page.Locator("[data-cy='quick-add-RunDataPipelineTask']");
            await item.WaitForAsync();
            await scene.ClickAsync(item, pauseAfter: 250);
            await Panel(page).WaitForAsync();
        });

        await scene.StepAsync("build-5", async () =>
        {
            var pipeline = page.GetByLabel("Data pipeline", new() { Exact = true }).First;
            await scene.SelectAsync(pipeline, "Blog posts export");
            await page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(1500);

            // The panel can redraw the list empty after the value was applied: choose it again so that the screen shows the saved value.
            if (await pipeline.InputValueAsync() != "blogpostsexport")
            {
                await scene.SelectAsync(pipeline, "Blog posts export");
            }
        });

        await scene.StepAsync("build-6", async () =>
        {
            await scene.HighlightAsync(page.GetByLabel("Parameters", new() { Exact = true }).First);
        });

        await scene.StepAsync("build-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Wait for the run to end").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".wfd-port[data-outcome='Succeeded']").First);
            await scene.HighlightAsync(page.Locator(".wfd-port[data-outcome='Failed']").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Skip when a run is already queued or running").First);
        });

        await scene.StepAsync("build-8", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-publish']"), pauseAfter: 400);
            await page.Locator("[data-cy='published-version']").WaitForAsync();
            await scene.PauseAsync(600);
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentItems", s => CollapseMenuAsync(s.Page));
        var page = scene.Page;

        await scene.StepAsync("run-1", async () =>
        {
            var blog = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("a[href*='/Display']:text-is('Blog')") }).First;
            await GoAsync(scene, blog.GetByText("List BlogPost Items").First);
            var post = page.Locator(".list-group-item").Filter(new() { HasTextString = "Man must explore" }).First;
            await post.WaitForAsync();
            await GoAsync(scene, post.Locator("a[href*='/Edit']").First);
            await page.Locator("#TitlePart_Title").WaitForAsync();
            await GoAsync(scene, page.Locator("button[name='submit.Publish']").First);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 60; i++)
                {
                    if (await FirstRunBadgeAsync(page) == "Succeeded")
                    {
                        break;
                    }

                    await Task.Delay(1000);
                }

                await scene.NavigateAsync("/Admin/DataPipelines/Runs");
            }, 100);
            await scene.HighlightAsync(page.Locator("li[data-run-id] span.badge:has(i.fa-play)").First);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("li[data-run-id]").First.GetByText("Details").First);
            var delivered = page.GetByText("Delivered", new() { Exact = true }).First;
            await delivered.WaitForAsync();
            await delivered.ScrollToMiddleAsync();
            await scene.HighlightAsync(page.GetByText("Saved 'blog-posts.csv' to the media library.").First);
        });
    }

    // Off camera, no frames: removes the workflow that a failed take of 04-react left.
    private static async Task ResetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        await page.GotoAsync(BaseUrl + "/Admin/Workflows/Types");
        var row = page.Locator(".list-group-item").Filter(new() { HasTextString = "Log the pipeline runs that fail" }).First;
        await row.GetByText("Delete").First.ClickAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("Reset done.");
        await context.CloseAsync();
    }

    private static async Task ReactAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Workflows/Types", s => CollapseMenuAsync(s.Page));
        var page = scene.Page;

        await scene.StepAsync("react-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.create").First);
            await page.Locator("#Name").WaitForAsync();
            await scene.TypeAsync(page.Locator("#Name"), "Log the pipeline runs that fail", delay: 25);
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Create and open the designer" }).First);
            await page.Locator("[data-cy='start-picker']").WaitForAsync();
        });

        await scene.StepAsync("react-2", async () =>
        {
            await scene.TypeAsync(page.Locator("[data-cy='toolbox-search']"), "pipeline", delay: 10);
            var card = page.Locator("[data-cy='toolbox-activity-DataPipelineRunCompletedEvent']");
            await card.WaitForAsync();
            await scene.ClickAsync(card, pauseAfter: 400);
            await Panel(page).WaitForAsync();
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("react-3", async () =>
        {
            var pipeline = page.GetByLabel("Data pipeline", new() { Exact = true }).First;
            await scene.HighlightAsync(pipeline);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(page.GetByLabel("Runs", new() { Exact = true }).First, "Runs that failed or were cancelled");
            await page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("react-4", async () =>
        {
            var close = page.Locator("[data-cy='activity-panel-close']");
            if (await close.CountAsync() > 0 && await close.First.IsVisibleAsync())
            {
                await scene.ClickAsync(close.First, pauseAfter: 300);
            }

            await scene.ClickAsync(page.Locator(".wfd-port[data-outcome='Done']").First, pauseAfter: 400);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "log", delay: 10);
            var item = page.Locator("[data-cy='quick-add-LogTask']");
            await item.WaitForAsync();
            await scene.ClickAsync(item, pauseAfter: 400);
            await Panel(page).WaitForAsync();
            await scene.SelectAsync(page.GetByLabel("Log Level", new() { Exact = true }).First, "Error");
            await scene.TypeAsync(page.GetByLabel("Text", new() { Exact = true }).First, "A data pipeline run failed", delay: 8);
            await page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("react-5", async () =>
        {
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-publish']"), pauseAfter: 400);
            await page.Locator("[data-cy='published-version']").WaitForAsync();
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("react-6", async () =>
        {
            await scene.OffCameraAsync(() => scene.NavigateAsync("/Admin/DataPipelines"), 100);
            var row = page.Locator("li[data-pipeline-id='blogpostsbyemail']");
            await scene.ClickAsync(row.Locator("button.actions").First, pauseAfter: 400);
            var runNow = row.GetByText("Run now").First;
            await scene.MoveToAsync(runNow);
            var (x, y) = await scene.CenterOfAsync(runNow);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 200);
            await WaitTextAsync(page, "will run in the background");
        });

        await scene.StepAsync("react-7", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 60; i++)
                {
                    if (await FirstRunBadgeAsync(page) == "Failed")
                    {
                        break;
                    }

                    await Task.Delay(1000);
                }

                await Task.Delay(3000);
                await scene.NavigateAsync("/Admin/DataPipelines/Runs");
            }, 100);
            await scene.HighlightAsync(page.GetByText("The email could not be sent").First);
        });

        await scene.StepAsync("react-8", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.NavigateAsync("/Admin/Workflows/Types"), 100);
            await GoAsync(scene, page.Locator("a[data-cy='all-instances']").First);
            await page.Locator("li, tr").Filter(new() { HasTextString = "Finished" }).First.WaitForAsync();
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

using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Send an email when content is published", recorded in order on one fresh Blog site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The test mail server (smtp4dev in Docker): its web page and its SMTP port on this computer.
    public static string MailUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_MAIL_URL") ?? "http://localhost:3000";

    public static string SmtpPort { get; } = Environment.GetEnvironmentVariable("VIDEO_SMTP_PORT") ?? "2525";

    private const string WorkflowName = "Notify the editors when an article is published";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-setup", SetupAsync),
        ("03-create", CreateAsync),
        ("04-event", EventAsync),
        ("05-email", EmailAsync),
        ("06-publish", PublishAsync),
        ("07-try", TryAsync),
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

    /// <summary>Seeds the site, outside the capture: Workflows and the SMTP provider, pointed at the test mail server.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var feature in new[] { "OrchardCore_Workflows", "OrchardCore_Email_Smtp" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            await page.Locator($"#btn-enable-{feature}").ClickAsync();
            await WaitTextAsync(page, "was enabled");
        }

        await page.GotoAsync(BaseUrl + "/Admin/Settings/email");
        await page.Locator("a:has-text('SMTP')").First.ClickAsync();
        await page.Locator("#ISite_SmtpSettings_IsEnabled").CheckAsync();
        await page.Locator("#ISite_SmtpSettings_DefaultSender").FillAsync("noreply@example.com");
        await page.Locator("#ISite_SmtpSettings_Host").FillAsync("localhost");
        await page.Locator("#ISite_SmtpSettings_Port").FillAsync(SmtpPort);
        await page.Locator("button.save").First.ClickAsync();
        await WaitTextAsync(page, "Site settings updated successfully.");

        // Warms up the pages the clips open, so their first load is not slow.
        foreach (var path in new[] { "/Admin/Workflows/Types", "/Admin/Workflows/Types/Create", "/Admin/Contents/ContentItems" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Site prepared.");
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

    private static async Task OpenDesignerAsync(Scene scene)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await page.GetByRole(AriaRole.Link, new() { Name = WorkflowName }).First.ClickAsync();
        await page.Locator("[data-cy='start-picker'], .wfd-node").First.WaitForAsync();
        await page.Locator("[data-cy='panel-tab-issues']").ClickAsync();
        await Task.Delay(600);
    }

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        var mail = await scene.Context.NewPageAsync();
        await mail.GotoAsync(MailUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await mail.WaitForTimeoutAsync(1500);
        await scene.OpenAsync("/Admin/Settings/email");
        await Group(page, "settings").ClickAsync();
        await page.Locator("a[href$='/Admin/Settings/email'] >> visible=true").First.WaitForAsync();
        await page.BringToFrontAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("a[href$='/Admin/Settings/email'] >> visible=true").First);
            await scene.ClickAsync(page.Locator("a:has-text('SMTP')").First, pauseAfter: 200);
            await scene.ClearHighlightsAsync();

            foreach (var id in new[] { "IsEnabled", "DefaultSender", "Host", "Port" })
            {
                await scene.HighlightAsync(page.Locator($"#ISite_SmtpSettings_{id}"));
                await scene.PauseAsync(1700);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.SwitchToAsync(mail);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Workflows/Types");
        await Group(page, "design").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("a[href$='/Admin/Workflows/Types'] >> visible=true").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.btn.create").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.HighlightAsync(page.GetByText("More settings").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("input#Name"), WorkflowName, delay: 10);
            var button = page.GetByRole(AriaRole.Button, new() { Name = "Create and open the designer" });
            await GoAsync(scene, button);
            await page.Locator("[data-cy='start-picker']").WaitForAsync();
        });
    }

    private static async Task EventAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("event-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("[data-cy='designer-toolbox']"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='start-picker']"));
        });

        await scene.StepAsync("event-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='start-picker-ContentPublishedEvent']"));
            await page.Locator("[data-cy='start-badge']").WaitForAsync();
        });

        await scene.StepAsync("event-3", async () =>
        {
            await scene.ClickAsync(page.Locator("button.dropdown-toggle[data-id='ContentPublishedEvent_SelectedContentTypeNames']"));
            await scene.TypeAsync(page.GetByPlaceholder("Search content types"), "Article", delay: 20);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show ul.inner li a").Filter(new() { HasTextString = "Article" }).First);
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForFunctionAsync("() => document.querySelector('.wfd-node.is-start .wfd-node-body em')?.innerText.includes('Article')");
        });
    }

    private static async Task EmailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await page.Locator("button[aria-label='Close the activity panel']").ClickIfVisibleAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("email-1", async () =>
        {
            await scene.ClickAsync(page.Locator(".wfd-node.is-start [data-outcome='Done']"), pauseAfter: 200);
            await page.Locator("[data-cy='quick-add']").WaitForAsync();
            await scene.TypeAsync(page.Locator("[data-cy='quick-add-search']"), "email", delay: 15);
            await scene.HighlightAsync(page.Locator("[data-cy='quick-add-EmailTask']"));
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='quick-add-EmailTask']"), pauseAfter: 200);
        });

        await scene.StepAsync("email-2", async () =>
        {
            await page.Locator(".wfd-node.is-task").WaitForAsync();
            var resizer = page.Locator("[data-cy='activity-panel-resizer']");
            var (x, y) = await scene.CenterOfAsync(resizer);
            await scene.DragAsync(resizer, x, 330);
        });

        await scene.StepAsync("email-3", async () =>
        {
            foreach (var id in new[] { "AuthorExpression", "SenderExpression", "ReplyToExpression" })
            {
                await scene.HighlightAsync(page.Locator($"input[id$='{id}']"));
                await scene.PauseAsync(700);
                await scene.ClearHighlightsAsync();
            }

            await page.Locator("input[id$='RecipientsExpression']").ScrollIntoViewIfNeededAsync();
            await scene.TypeAsync(page.Locator("input[id$='RecipientsExpression']"), "editors@example.com", delay: 15);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("email-4", async () =>
        {
            await page.Locator("input[id$='SubjectExpression']").ScrollIntoViewIfNeededAsync();
            await scene.TypeAsync(page.Locator("input[id$='SubjectExpression']"), "An article was published", delay: 15);
            await page.Keyboard.PressAsync("Tab");
            await scene.SelectAsync(page.Locator("select[id$='BodyFormat']"), "Text");
        });

        await scene.StepAsync("email-5", async () =>
        {
            var editor = page.Locator("#textBodyDiv .CodeMirror").First;
            await scene.MoveToAsync(page.Locator("input[id$='SubjectExpression']"));
            await scene.ScrollAsync(0, 500);
            await scene.ClickAsync(editor, pauseAfter: 200);
            await page.Keyboard.InsertTextAsync("An article was published on the site. Open the admin to review it.");
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Apply" }), pauseAfter: 100);
        });

        await scene.StepAsync("email-6", async () =>
        {
            await scene.ClickAsync(page.Locator("button[aria-label='Close the activity panel']"), pauseAfter: 200);
            await scene.HighlightAsync(page.Locator(".wfd-node.is-task").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();

            foreach (var outcome in new[] { "Done", "Failed" })
            {
                await scene.HighlightAsync(page.Locator($".wfd-node.is-task [data-outcome='{outcome}']"));
                await scene.PauseAsync(1500);
                await scene.ClearHighlightsAsync();
            }
        });
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("publish-1", async () =>
        {
            var saved = page.Locator("[data-cy='designer-toolbar']").GetByText("Saved", new() { Exact = true });
            await saved.WaitForAsync();
            await scene.HighlightAsync(saved);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("[data-cy='toolbar-publish']"));
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("[data-cy='toolbar-publish']"));
        });

        await scene.StepAsync("publish-2", async () =>
        {
            await WaitTextAsync(page, "The workflow was published as version");
            await scene.HighlightAsync(page.Locator("[data-cy='published-version']"));
        });

        await scene.StepAsync("publish-3", async () =>
        {
            await scene.MoveToAsync(page.Locator("[data-cy='published-version']"));
        });
    }

    private static async Task TryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        var mail = await scene.Context.NewPageAsync();
        await mail.GotoAsync(MailUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await scene.OpenAsync("/Admin");
        await Group(page, "content").ClickAsync();
        await page.BringToFrontAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("try-1", async () =>
        {
            await GoAsync(scene, page.Locator("a.oc-navigation-admin-content-content_items"));
            await scene.ClickAsync(page.Locator("#new-dropdown, button:has-text('New')").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator(".dropdown-menu.show a:has-text('Article')").First);
        });

        await scene.StepAsync("try-2", async () =>
        {
            await scene.TypeAsync(page.Locator("#TitlePart_Title, input[name='TitlePart.Title']").First, "Spring garden tips", delay: 15);
            await scene.HighlightAsync(page.Locator("button.draft, button:has-text('Save Draft')").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("button.publish").Filter(new() { Visible = true }).First);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.publish").Filter(new() { Visible = true }).First);
        });

        await scene.StepAsync("try-3", async () =>
        {
            await WaitTextAsync(page, "Your Article has been published.");
            await scene.HighlightAsync(page.GetByText("Your Article has been published.").First);
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("try-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Workflows/Types");
            await GoAsync(scene, page.Locator("a.btn:has-text('Instances')").First);
            await scene.HighlightAsync(page.Locator("[data-cy='instance-count-Finished']"));
        });

        await scene.StepAsync("try-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SwitchToAsync(mail);
            await mail.WaitForFunctionAsync("() => document.body.innerText.includes('An article was published')", null, new() { Timeout = 60_000 });
            await scene.ClickAsync(mail.GetByText("An article was published", new() { Exact = true }).First, pauseAfter: 300);
            await mail.FrameLocator("iframe").GetByText("An article was published on the site").First.WaitForAsync();
        });
    }
}

internal static class LocatorExtensions
{
    public static async Task ClickIfVisibleAsync(this ILocator locator)
    {
        if (await locator.IsVisibleAsync())
        {
            await locator.ClickAsync();
        }
    }
}

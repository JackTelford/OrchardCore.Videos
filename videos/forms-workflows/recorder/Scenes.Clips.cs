using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-start", StartAsync),
        ("03-validate", ValidateAsync),
        ("04-valid", ValidAsync),
        ("05-connect", ConnectAsync),
        ("06-try", TryAsync),
    ];

    private const string UrlFile = "C:/ocv/s5/workflow-url-forms-workflows.txt";

    private static ILocator FormCard(IPage page, string type) => page.Locator($".widget-editor[data-content-type=\"{type}\"]");

    private static ILocator Field(ILocator scope, string suffix, string tag = "input") => scope.Locator($"{tag}[name$=\"{suffix}\"]").First;

    private static async Task AddWidgetAsync(IPage page, ILocator scope, string type)
    {
        await scope.Locator("button[title=\"Add Widget\"]").Last.ClickAsync();
        await scope.Locator($"a.add-widget[data-widget-type=\"{type}\"]").Last.ClickAsync();
    }

    private static async Task StandardAsync(ILocator card, string label)
    {
        await Field(card, "FormElementLabelPart.LabelOption", "select").SelectOptionAsync("Standard");
        await Field(card, "FormElementLabelPart.Label").FillAsync(label);
    }

    // Features, SMTP settings, the Thank You page and the Contact page with its form.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await EnableFeaturesAsync(page, "OrchardCore.Forms", "OrchardCore.Workflows", "OrchardCore.Workflows.Http", "OrchardCore.Email", "OrchardCore.Email.Smtp");
        await Task.Delay(6000);

        await page.GotoAsync(BaseUrl + "/Admin/Settings/email", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("a.nav-link:text-is(\"SMTP\")").ClickAsync();
        await page.Locator("#ISite_SmtpSettings_IsEnabled").CheckAsync();
        await page.Locator("#ISite_SmtpSettings_DefaultSender").FillAsync("noreply@example.com");
        await page.Locator("#ISite_SmtpSettings_Host").FillAsync("localhost");
        await page.Locator("#ISite_SmtpSettings_Port").FillAsync("2535");
        await page.Locator("#ISite_SmtpSettings_AutoSelectEncryption").UncheckAsync();
        await page.Locator("#ISite_SmtpSettings_RequireCredentials").UncheckAsync();
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.save").ClickAsync());

        // Thank You page.
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Page/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[name$=\"TitlePart.Title\"]").First.FillAsync("Thank You");
        await page.Locator("button[title=\"Add Widget\"]").Last.ClickAsync();
        await page.Locator("a.add-widget[data-widget-type=\"Paragraph\"]").Last.ClickAsync();
        var paragraph = FormCard(page, "Paragraph").First;
        await paragraph.WaitForAsync();
        await paragraph.Locator("textarea[name$=\"Paragraph.Content.Html\"]").First.EvaluateAsync("(e, h) => { const $ = window.jQuery; if ($ && $(e).data('trumbowyg')) { $(e).trumbowyg('html', h); } e.value = h; e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }", "<p>Thank you for reaching out. We will get back to you soon.</p>");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
        await page.GotoAsync(BaseUrl + "/thank-you", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Thank You page: " + (await page.Locator("body").InnerTextAsync()).Contains("We will get back to you soon"));

        // Contact page.
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Page/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[name$=\"TitlePart.Title\"]").First.FillAsync("Contact");
        await page.Locator("button[title=\"Add Widget\"]").Last.ClickAsync();
        await page.Locator("a.add-widget[data-widget-type=\"Form\"]").Last.ClickAsync();
        var form = FormCard(page, "Form").First;
        await form.WaitForAsync();
        await Field(form, "TitlePart.Title").FillAsync("Contact");
        await AddWidgetAsync(page, form, "ValidationSummary");
        await AddWidgetAsync(page, form, "Input");
        var name = FormCard(page, "Input").Nth(0);
        await name.WaitForAsync();
        await Field(name, "FormInputElementPart.Name").FillAsync("Name");
        await StandardAsync(name, "Name");
        await Field(name, "InputPart.Placeholder").FillAsync("Enter your name");
        await AddWidgetAsync(page, form, "Input");
        var email = FormCard(page, "Input").Nth(1);
        await email.WaitForAsync();
        await Field(email, "FormInputElementPart.Name").FillAsync("Email");
        await StandardAsync(email, "Email");
        await Field(email, "InputPart.Type", "select").SelectOptionAsync("Email");
        await Field(email, "InputPart.Placeholder").FillAsync("Enter your email address");
        await AddWidgetAsync(page, form, "TextArea");
        var message = FormCard(page, "TextArea").First;
        await message.WaitForAsync();
        await Field(message, "FormInputElementPart.Name").FillAsync("Message");
        await StandardAsync(message, "Message");
        await Field(message, "TextAreaPart.Placeholder").FillAsync("Enter your message");
        await Field(message, "TextAreaPart.Rows").FillAsync("4");
        await AddWidgetAsync(page, form, "Button");
        var button = FormCard(page, "Button").First;
        await button.WaitForAsync();
        await Field(button, "FormInputElementPart.Name").FillAsync("Send");
        await Field(button, "ButtonPart.Text").FillAsync("Submit");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
        await context.CloseAsync();
    }

    private static ILocator Port(ILocator node, string outcome) => node.Locator($".wfd-port[data-outcome=\"{outcome}\"]").First;

    private static ILocator Node(IPage page, string text, string without = null)
    {
        var node = page.Locator(".wfd-node").Filter(new() { HasText = text });
        return without is null ? node.First : node.Filter(new() { HasNotText = without }).First;
    }

    private static async Task ClickPortAsync(Scene scene, ILocator port)
    {
        var dot = port.Locator(".wfd-port-dot").First;
        var (x, y) = await scene.CenterOfAsync(dot);
        await scene.MoveToAsync(x, y, 10);
        await Task.Delay(150);
        await scene.Page.Mouse.DownAsync();
        await Task.Delay(80);
        await scene.Page.Mouse.UpAsync();
        await Task.Delay(200);
    }

    private static async Task AddActivityAsync(Scene scene, ILocator port, string search, string cy)
    {
        var page = scene.Page;
        await ClickPortAsync(scene, port);
        await page.Locator("[data-cy=\"quick-add\"]").WaitForAsync();
        await page.Locator("[data-cy=\"quick-add-search\"]").PressSequentiallyAsync(search, new() { Delay = 25 });
        await FastClickAsync(scene, page.Locator($"[data-cy=\"{cy}\"]"), 200);
    }

    private static async Task ClosePanelAsync(IPage page)
    {
        var close = page.Locator("[data-cy=\"activity-panel-close\"]");
        if (await close.IsVisibleAsync())
        {
            await close.ClickAsync();
        }

        await page.Locator("[data-cy=\"activity-panel\"]").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    private static async Task ValidateFieldAsync(Scene scene, ILocator port, string field)
    {
        var page = scene.Page;
        await AddActivityAsync(scene, port, "validate form field", "quick-add-ValidateFormFieldTask");
        var panel = page.Locator("[data-cy=\"activity-panel\"]");
        await panel.WaitForAsync();
        await FastTypeAsync(scene, panel.Locator("input[name$=\"FieldName\"]").First, field, 25);
        await FastTypeAsync(scene, panel.Locator("input[name$=\"ErrorMessage\"]").First, field + " is required", 25);
        await FastClickAsync(scene, panel.Locator("[data-cy=\"panel-apply\"]"), 200);
        await Node(page, "Validate " + field).WaitForAsync();
        await panel.Locator("input[name$=\"ErrorMessage\"]").First.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
        await page.WaitForTimeoutAsync(300);
    }

    // Opens the designer of the workflow off camera, with the side panes collapsed, and returns to the admin home.
    private static async Task<IPage> PrepareDesignerAsync(Scene scene, bool open = true)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.EvaluateAsync("() => { localStorage.setItem('orchardcore:workflows-designer:toolbox-collapsed', 'true'); localStorage.setItem('orchardcore:workflows-designer:panel-collapsed', 'true'); }");
        if (open)
        {
            await scene.OpenAsync("/Admin/Workflows/Types");
            await page.Locator("a:text-is(\"Contact Form Workflow\")").First.ClickAsync();
            await page.Locator(".wfd-node").First.WaitForAsync();
        }

        return page;
    }

    private static async Task StartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = await PrepareDesignerAsync(scene, open: false);
        await page.WaitForTimeoutAsync(600);
        foreach (var group in new[] { "Design" })
        {
            var button = page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\"))");
            if (await button.GetAttributeAsync("aria-expanded") != "true")
            {
                await button.ClickAsync();
            }
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("start-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Workflows\"))"));
            await FastGoAsync(scene, page.Locator("a.create:has-text(\"Create Workflow\")"));
            await FastTypeAsync(scene, page.Locator("input#Name"), "Contact Form Workflow", 25);
            await FastGoAsync(scene, page.Locator("button.save:has-text(\"Create and open the designer\")"));
        });

        await scene.StepAsync("start-2", async () =>
        {
            await FastClickAsync(scene, page.Locator("[data-cy=\"start-picker-HttpRequestEvent\"]"), 200);
            await page.Locator("[data-cy=\"activity-panel\"]").WaitForAsync();
        });

        await scene.StepAsync("start-3", async () =>
        {
            var panel = page.Locator("[data-cy=\"activity-panel\"]");
            var method = panel.Locator("select").First;
            await scene.MoveToAsync(method);
            await Task.Delay(250);
            await method.SelectOptionAsync("POST");
            await Task.Delay(250);
            await page.Keyboard.PressAsync("Tab");
            var url = "";
            for (var i = 0; i < 40 && string.IsNullOrEmpty(url); i++)
            {
                url = await page.Locator("[data-workflow-url-text]").First.InputValueAsync();
                await Task.Delay(250);
            }

            File.WriteAllText(UrlFile, url);
            var box = page.Locator("[data-workflow-url-text]").First;
            await box.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await scene.HighlightAsync(box.Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper')][1]"));
        });

        await scene.StepAsync("start-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var panel = page.Locator("[data-cy=\"activity-panel\"]");
            var token = panel.Locator("input[type=\"checkbox\"][name$=\"ValidateAntiforgeryToken\"]").First;
            await token.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await scene.HighlightAsync(token.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            var key = panel.Locator("input[name$=\"FormLocationKey\"]").First;
            await key.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await scene.HighlightAsync(key.Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper')][1]"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await ClosePanelAsync(page);
        await page.GetByText("All changes saved").First.WaitForAsync(new() { Timeout = 8000 }).ContinueWith(_ => { });
        await page.WaitForTimeoutAsync(2500);
    }

    private static async Task ValidateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = await PrepareDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("validate-1", async () =>
        {
            await AddActivityAsync(scene, Port(page.Locator(".wfd-node").First, "Done"), "bind", "quick-add-BindModelStateTask");
            await Node(page, "Bind Form Model State").WaitForAsync();
        });

        await scene.StepAsync("validate-2", async () =>
        {
            await ClosePanelAsync(page);
            await ValidateFieldAsync(scene, Port(Node(page, "Bind Form Model State"), "Done"), "Name");
        });

        await scene.StepAsync("validate-3", async () =>
        {
            await ClosePanelAsync(page);
            await ValidateFieldAsync(scene, Port(Node(page, "Validate Name"), "Done"), "Email");
        });

        await scene.StepAsync("validate-4", async () =>
        {
            await ClosePanelAsync(page);
            await AddActivityAsync(scene, Port(Node(page, "Validate Email"), "Done"), "validate form task", "quick-add-ValidateFormTask");
            await Node(page, "Validate Form", "Field").WaitForAsync();
        });

        await scene.StepAsync("validate-5", async () =>
        {
            await AddActivityAsync(scene, Port(Node(page, "Validate Form", "Field"), "Invalid"), "form location", "quick-add-HttpRedirectToFormLocationTask");
            await Node(page, "HTTP Redirect To Form Location").WaitForAsync();
            await ClosePanelAsync(page);
            await FastClickAsync(scene, page.Locator("[data-cy=\"zoom-fit\"]"), 300);
        });

        await page.WaitForTimeoutAsync(2500);
    }

    private static async Task ValidAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = await PrepareDesignerAsync(scene);
        await page.Locator("[data-cy=\"zoom-fit\"]").ClickAsync();
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("valid-1", async () =>
        {
            await AddActivityAsync(scene, Port(Node(page, "Validate Form", "Field"), "Valid"), "email", "quick-add-EmailTask");
            var panel = page.Locator("[data-cy=\"activity-panel\"]");
            await panel.WaitForAsync();
            var grip = page.Locator("[data-cy=\"activity-panel-resizer\"]");
            var (gx, gy) = await scene.CenterOfAsync(grip);
            await scene.MoveToAsync(gx, gy, 6);
            await page.Mouse.DownAsync();
            await page.Mouse.MoveAsync((float)gx, (float)(gy - 150), new() { Steps = 8 });
            await page.Mouse.UpAsync();
            await page.WaitForTimeoutAsync(300);
            await FastTypeAsync(scene, panel.Locator("input[name$=\"RecipientsExpression\"]").First, "owner@example.com", 15);
            await FastTypeAsync(scene, panel.Locator("input[name$=\"SubjectExpression\"]").First, "Contact form submitted by {{ Request.Form.Name }}", 8);
            await page.Keyboard.PressAsync("Tab");
            var body = panel.Locator(".CodeMirror, .monaco-editor, textarea[name$=\"HtmlBody\"]").First;
            await body.ScrollIntoViewIfNeededAsync();
            await panel.Locator("textarea[name$=\"HtmlBody\"]").First.EvaluateAsync("(e, t) => { const cm = e.parentElement.querySelector('.CodeMirror'); if (cm && cm.CodeMirror) { cm.CodeMirror.setValue(t); cm.CodeMirror.save(); } else { e.value = t; } e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }", "<p>Message: {{ Request.Form.Message }}</p>");
            await FastClickAsync(scene, panel.Locator("[data-cy=\"panel-apply\"]"), 200);
        });

        await scene.StepAsync("valid-2", async () =>
        {
            await ClosePanelAsync(page);
            await AddActivityAsync(scene, Port(Node(page, "Email Task"), "Done"), "http redirect", "quick-add-HttpRedirectTask");
            var panel = page.Locator("[data-cy=\"activity-panel\"]");
            await panel.WaitForAsync();
            await FastTypeAsync(scene, panel.Locator("input[name$=\"Location\"], textarea[name$=\"Location\"]").First, "/thank-you", 20);
            await page.Keyboard.PressAsync("Tab");
            await Node(page, "/thank-you").WaitForAsync();
            await ClosePanelAsync(page);
            await scene.HighlightAsync(Port(Node(page, "Email Task"), "Failed"));
        });

        await scene.StepAsync("valid-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("[data-cy=\"zoom-fit\"]"), 300);
            await FastClickAsync(scene, page.Locator("[data-cy=\"toolbar-publish\"]"), 200);
            await page.GetByText("The workflow was published").First.WaitForAsync();
        });
    }

    private static async Task ConnectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems?q=Contact");
        var compactor = page.Locator("button.leftbar-compactor");
        if (await compactor.CountAsync() > 0 && await page.Locator("#left-nav span.title").First.IsVisibleAsync())
        {
            await compactor.First.ClickAsync();
        }

        var url = File.ReadAllText(UrlFile).Trim();
        await scene.StartCaptureAsync();

        await scene.StepAsync("connect-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("li.list-group-item a:text-is(\"Contact\")").First);
            var form = FormCard(page, "Form").First;
            await form.WaitForAsync();
            foreach (var type in new[] { "Input", "TextArea", "Button", "ValidationSummary" })
            {
                foreach (var card in await FormCard(page, type).AllAsync())
                {
                    await card.Locator(".widget-editor-btn-collapse").First.ClickAsync();
                }
            }

            var action = Field(form, "FormPart.Action");
            await action.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await page.WaitForTimeoutAsync(300);
            await scene.HighlightAsync(action.Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper')][1]"));
        });

        await scene.StepAsync("connect-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var form = FormCard(page, "Form").First;
            await Field(form, "FormPart.Action").FillAsync(url);
            await Field(form, "FormPart.Action").EvaluateAsync("e => e.scrollIntoView({ block: 'start' })");
            await scene.ScrollAsync(0, -100, 4);
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(Field(form, "FormPart.Method", "select").Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper')][1]"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(form.Locator("input[type=\"checkbox\"][name$=\"FormPart.EnableAntiForgeryToken\"]").First.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
            await scene.HighlightAsync(form.Locator("input[type=\"checkbox\"][name$=\"FormPart.SaveFormLocation\"]").First.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("connect-3", async () =>
            await FastGoAsync(scene, page.Locator("button[name=\"submit.Publish\"][value=\"submit.Publish\"]").First));
    }

    private static async Task TryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/contact");
        var page = scene.Page;
        await page.Locator("button[name=\"Send\"]").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("try-1", async () =>
        {
            await FastTypeAsync(scene, page.Locator("textarea[name=\"Message\"]"), "Can you send me your price list?", 15);
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator("button[name=\"Send\"]"), 0));
            await page.Locator(".validation-summary-errors").First.WaitForAsync();
        });

        await scene.StepAsync("try-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input[name=\"Name\"]"), "Jamie Chen", 20);
            await FastTypeAsync(scene, page.Locator("input[name=\"Email\"]"), "jamie.chen@example.com", 8, fill: true);
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator("button[name=\"Send\"]"), 0));
            await page.WaitForURLAsync("**/thank-you");
        });

        await scene.StepAsync("try-3", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Admin/Workflows/Types"));
            await FastClickAsync(scene, page.Locator("a[data-cy=\"all-instances\"]").First, 100);
            var link = page.Locator("a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^[0-9a-z]{20,}$") }).First;
            await link.WaitForAsync();
            await FastClickAsync(scene, link, 100);
            await page.Locator(".wfd-node").First.WaitForAsync();
            var journal = page.GetByRole(AriaRole.Tab, new() { Name = "Journal" });
            await journal.WaitForAsync();
            await FastClickAsync(scene, journal, 300);
            await scene.ScrollAsync(0, 240, 8);
        });
    }
}

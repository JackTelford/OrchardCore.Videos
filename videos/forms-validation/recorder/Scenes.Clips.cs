using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-messages", MessagesAsync),
        ("03-rules", RulesAsync),
        ("04-try", TryAsync),
    ];

    private static ILocator FormCard(IPage page, string type) => page.Locator($".widget-editor[data-content-type=\"{type}\"]");

    private static ILocator Field(ILocator scope, string suffix, string tag = "input") => scope.Locator($"{tag}[name$=\"{suffix}\"]").First;

    private static async Task SetEditorAsync(IPage page, ILocator textarea, string html)
    {
        await textarea.EvaluateAsync("(e, h) => { const $ = window.jQuery; if ($ && $(e).data('trumbowyg')) { $(e).trumbowyg('html', h); } else { e.value = h; } e.value = h; e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }", html);
    }

    private static async Task AddWidgetAsync(IPage page, ILocator scope, string type)
    {
        await scope.Locator("button[title=\"Add Widget\"]").Last.ClickAsync();
        await scope.Locator($"a.add-widget[data-widget-type=\"{type}\"]").Last.ClickAsync();
    }

    private static async Task StandardAsync(IPage page, ILocator card, string label)
    {
        await Field(card, "FormElementLabelPart.LabelOption", "select").SelectOptionAsync("Standard");
        await Field(card, "FormElementLabelPart.Label").FillAsync(label);
    }

    // Features, the Thank You page, the Contact Form page, the workflow with its start event, and the form action.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await EnableFeaturesAsync(page, "OrchardCore.Forms", "OrchardCore.Workflows", "OrchardCore.Workflows.Http");
        await Task.Delay(6000);

        // Thank You page.
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Page/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[name$=\"TitlePart.Title\"]").First.FillAsync("Thank You");
        await page.Locator("button[title=\"Add Widget\"]").Last.ClickAsync();
        await page.Locator("a.add-widget[data-widget-type=\"RawHtml\"]").Last.ClickAsync();
        var raw = page.Locator(".widget-editor[data-content-type=\"RawHtml\"]").First;
        await raw.WaitForAsync();
        await SetEditorAsync(page, raw.Locator("textarea[name$=\"RawHtml.Content.Html\"]").First, "<p>Thank you for reaching out. We will reply soon.</p>");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
        await page.GotoAsync(BaseUrl + "/thank-you", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Thank You page: " + (await page.Locator("body").InnerTextAsync()).Contains("Thank you for reaching out"));

        // Contact Form page.
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Page/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[name$=\"TitlePart.Title\"]").First.FillAsync("Contact Form");
        await page.Locator("button[title=\"Add Widget\"]").Last.ClickAsync();
        await page.Locator("a.add-widget[data-widget-type=\"Form\"]").Last.ClickAsync();
        var form = FormCard(page, "Form").First;
        await form.WaitForAsync();
        await Field(form, "TitlePart.Title").FillAsync("Contact");
        await AddWidgetAsync(page, form, "Input");
        var name = FormCard(page, "Input").Nth(0);
        await name.WaitForAsync();
        await Field(name, "FormInputElementPart.Name").FillAsync("Name");
        await StandardAsync(page, name, "Name");
        await Field(name, "InputPart.Placeholder").FillAsync("Enter your name");
        await AddWidgetAsync(page, form, "Input");
        var email = FormCard(page, "Input").Nth(1);
        await email.WaitForAsync();
        await Field(email, "FormInputElementPart.Name").FillAsync("Email");
        await StandardAsync(page, email, "Email");
        await Field(email, "InputPart.Type", "select").SelectOptionAsync("Email");
        await Field(email, "InputPart.Placeholder").FillAsync("Enter your email address");
        await AddWidgetAsync(page, form, "TextArea");
        var message = FormCard(page, "TextArea").First;
        await message.WaitForAsync();
        await Field(message, "FormInputElementPart.Name").FillAsync("Message");
        await StandardAsync(page, message, "Message");
        await Field(message, "TextAreaPart.Placeholder").FillAsync("Enter your message");
        await Field(message, "TextAreaPart.Rows").FillAsync("5");
        await AddWidgetAsync(page, form, "Button");
        var button = FormCard(page, "Button").First;
        await button.WaitForAsync();
        await Field(button, "FormInputElementPart.Name").FillAsync("Submit");
        await Field(button, "ButtonPart.Text").FillAsync("Submit");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());

        // The workflow with its start event.
        await page.GotoAsync(BaseUrl + "/Admin/Workflows/Types", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("a.create:has-text(\"Create Workflow\")").ClickAsync());
        await page.Locator("input#Name").FillAsync("Contact Form Validation");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.save:has-text(\"Create and open the designer\")").ClickAsync());
        await page.Locator("[data-cy=\"start-picker-HttpRequestEvent\"]").ClickAsync();
        var panel = page.Locator("[data-cy=\"activity-panel\"]");
        await panel.WaitForAsync();
        await panel.Locator("select").First.SelectOptionAsync("POST");
        await page.Keyboard.PressAsync("Tab");
        string url = "";
        for (var i = 0; i < 40 && string.IsNullOrEmpty(url); i++)
        {
            url = await page.Locator("[data-workflow-url-text]").First.InputValueAsync();
            await Task.Delay(500);
        }

        Console.WriteLine("Workflow url length: " + url.Length);
        await page.Locator("[data-cy=\"activity-panel-close\"]").ClickAsync();
        await page.WaitForTimeoutAsync(4000);
        Console.WriteLine("Toolbar: " + string.Join(" | ", (await page.Locator("[data-cy^=\"toolbar\"]").AllInnerTextsAsync())));

        // The form action.
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems?q=Contact%20Form", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("li.list-group-item:has-text(\"Contact Form\") a.edit").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Field(FormCard(page, "Form").First, "FormPart.Action").FillAsync(url);
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
        await context.CloseAsync();
    }

    private static async Task MessagesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems?q=Contact%20Form");
        await page.Locator("li.list-group-item:has-text(\"Contact Form\") a.edit").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var form = FormCard(page, "Form").First;
        await form.WaitForAsync();
        var compactor = page.Locator("button.leftbar-compactor");
        if (await compactor.CountAsync() > 0 && await page.Locator("#left-nav span.title").First.IsVisibleAsync())
        {
            await compactor.First.ClickAsync();
        }

        foreach (var type in new[] { "Input", "TextArea", "Button" })
        {
            foreach (var card in await FormCard(page, type).AllAsync())
            {
                await card.Locator(".widget-editor-btn-collapse").First.ClickAsync();
            }
        }

        await page.AddStyleTagAsync(new PageAddStyleTagOptions { Content = "div.ocat-wrapper:has(> div.ocat-end > input[name$='FormPart.Action']) { display: none !important; }" });
        await Field(form, "TitlePart.Title").ScrollIntoViewIfNeededAsync();
        await page.WaitForTimeoutAsync(400);
        Console.WriteLine("Action visible: " + await Field(form, "FormPart.Action").IsVisibleAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("messages-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("input[type=\"checkbox\"][name$=\"FormPart.SaveFormLocation\"]").First.Locator("xpath=.."));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("messages-2", async () =>
        {
            var card = FormCard(page, "Input").Nth(0);
            await FastClickAsync(scene, card.Locator(".widget-editor-btn-expand").First, 300);
            var list = Field(card, "FormElementValidationPart.ValidationOption", "select");
            await list.ScrollIntoViewIfNeededAsync();
            await scene.SelectAsync(list, "Standard", 300);
            await scene.HighlightAsync(list);
        });

        await scene.StepAsync("messages-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            foreach (var card in new[] { FormCard(page, "Input").Nth(1), FormCard(page, "TextArea").First })
            {
                await FastClickAsync(scene, card.Locator(".widget-editor-btn-expand").First, 300);
                var list = Field(card, "FormElementValidationPart.ValidationOption", "select");
                await list.ScrollIntoViewIfNeededAsync();
                await scene.SelectAsync(list, "Standard", 300);
            }
        });

        await scene.StepAsync("messages-4", async () =>
        {
            var header = FormCard(page, "Input").Nth(0);
            await header.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, -80, 6);
            var insert = header.Locator("button[title=\"Insert Widget\"]").First;
            var (hx, hy) = await scene.CenterOfAsync(header.Locator(".widget-editor-header-text").First);
            await scene.MoveToAsync(hx, hy, 8);
            await header.Locator(".widget-editor-header-text").First.HoverAsync();
            await page.WaitForTimeoutAsync(500);
            await insert.ClickAsync();
            await page.WaitForTimeoutAsync(300);
            await FastClickAsync(scene, header.Locator(".widget-editor-header a.insert-widget[data-widget-type=\"ValidationSummary\"]").First, 300);
            var summary = FormCard(page, "ValidationSummary").First;
            await summary.WaitForAsync();
            await summary.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(summary.Locator("input[type=\"checkbox\"][name$=\"ValidationSummaryPart.ModelOnly\"]").First.Locator("xpath=.."));
        });

        await scene.StepAsync("messages-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button[name=\"submit.Publish\"][value=\"submit.Publish\"]").First);
        });
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

    private static async Task RulesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Workflows/Types");
        var page = scene.Page;
        await page.Locator("a:text-is(\"Contact Form Validation\")").First.ClickAsync();
        await page.Locator(".wfd-node").First.WaitForAsync();
        await page.Locator("[data-cy=\"toolbox-collapse\"]").ClickAsync();
        await page.Locator("[data-cy=\"panel-collapse\"]").ClickAsync();
        await page.EvaluateAsync("() => localStorage.setItem('orchardcore:workflows-designer:activity-panel-height', '300')");
        await page.WaitForTimeoutAsync(500);
        await scene.OpenAsync("/Admin");
        await page.WaitForTimeoutAsync(600);
        var design = page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Design\"))");
        if (await design.GetAttributeAsync("aria-expanded") != "true")
        {
            await design.ClickAsync();
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("rules-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Workflows\"))"));
            await FastGoAsync(scene, page.Locator("a:text-is(\"Contact Form Validation\")").First);
            await page.Locator(".wfd-node").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".wfd-node").First);
        });

        await scene.StepAsync("rules-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await AddActivityAsync(scene, Port(page.Locator(".wfd-node").First, "Done"), "bind", "quick-add-BindModelStateTask");
            await Node(page, "Bind Form Model State").WaitForAsync();
            await ClosePanelAsync(page);
        });

        await scene.StepAsync("rules-3", async () =>
            await ValidateFieldAsync(scene, Port(Node(page, "Bind Form Model State"), "Done"), "Name"));

        await scene.StepAsync("rules-4", async () =>
        {
            await ClosePanelAsync(page);
            await scene.HighlightAsync(Node(page, "Validate Name").Locator(".wfd-node-ports").First);
        });

        await scene.StepAsync("rules-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ValidateFieldAsync(scene, Port(Node(page, "Validate Name"), "Done"), "Email");
            await ClosePanelAsync(page);
            await ValidateFieldAsync(scene, Port(Node(page, "Validate Email"), "Done"), "Message");
            await ClosePanelAsync(page);
        });

        await scene.StepAsync("rules-6", async () =>
        {
            await AddActivityAsync(scene, Port(Node(page, "Validate Message"), "Done"), "validate form", "quick-add-ValidateFormTask");
            await Node(page, "Validate Form", "Field").WaitForAsync();
        });

        await scene.StepAsync("rules-7", async () =>
        {
            await AddActivityAsync(scene, Port(Node(page, "Validate Form", "Field"), "Invalid"), "redirect to form", "quick-add-HttpRedirectToFormLocationTask");
            await Node(page, "HTTP Redirect To Form Location").WaitForAsync();
            await ClosePanelAsync(page);
        });

        await scene.StepAsync("rules-8", async () =>
        {
            await ClickPortAsync(scene, Port(Node(page, "Validate Form", "Field"), "Valid"));
            await page.Locator("[data-cy=\"quick-add\"]").WaitForAsync();
            await scene.OffCameraAsync(async () =>
            {
                await page.Locator("[data-cy=\"quick-add-search\"]").PressSequentiallyAsync("redirect", new() { Delay = 25 });
                await page.Locator("[data-cy=\"quick-add-HttpRedirectTask\"]").ClickAsync();
                var panel = page.Locator("[data-cy=\"activity-panel\"]");
                await panel.WaitForAsync();
                await panel.Locator("input[name$=\"Location\"], textarea[name$=\"Location\"]").First.PressSequentiallyAsync("/thank-you", new() { Delay = 25 });
                await page.Keyboard.PressAsync("Tab");
                await Node(page, "/thank-you").WaitForAsync();
                await ClosePanelAsync(page);
            });
        });

        await scene.StepAsync("rules-9", async () =>
        {
            await FastClickAsync(scene, page.Locator("[data-cy=\"zoom-fit\"]"), 300);
            await FastClickAsync(scene, page.Locator("[data-cy=\"toolbar-publish\"]"), 200);
            await page.GetByText("The workflow was published as version").First.WaitForAsync();
        });
    }

    private static async Task TryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, null, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/contact-form");
        var page = scene.Page;
        await page.Locator("button[name=\"Submit\"]").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("try-1", async () =>
        {
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator("button[name=\"Submit\"]"), 0));
            await page.Locator(".validation-summary-errors").First.WaitForAsync();
        });

        await scene.StepAsync("try-2", async () =>
        {
            await page.Locator(".field-validation-error").Last.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 60, 6);
            foreach (var message in await page.Locator(".field-validation-error").AllAsync())
            {
                await scene.HighlightAsync(message);
            }

            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("try-3", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input[name=\"Name\"]"), "Jamie Chen", 20);
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator("button[name=\"Submit\"]"), 0));
            await page.Locator(".validation-summary-errors").First.WaitForAsync();
        });

        await scene.StepAsync("try-4", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input[name=\"Email\"]"), "jamie.chen@example.com", 8, fill: true);
            await FastTypeAsync(scene, page.Locator("textarea[name=\"Message\"]"), "I would like to know more about your services.", 5, fill: true);
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator("button[name=\"Submit\"]"), 0));
            await page.WaitForURLAsync("**/thank-you");
        });
    }
}

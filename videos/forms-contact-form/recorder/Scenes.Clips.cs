using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-create", CreateAsync),
        ("03-text", TextAsync),
        ("04-rest", RestAsync),
        ("05-site", SiteAsync),
    ];

    // Nothing to seed: the clips build the page.
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static ILocator FormCard(IPage page, string type) => page.Locator($".widget-editor[data-content-type=\"{type}\"]");

    private static ILocator Field(ILocator scope, string suffix, string tag = "input") => scope.Locator($"{tag}[name$=\"{suffix}\"]").First;

    // Opens the Contact Us editor off camera.
    private static async Task OpenContactEditorAsync(Scene scene, bool collapseInputs = false)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems?q=Contact%20Us");
        await page.Locator("li.list-group-item:has-text(\"Contact Us\") a.edit").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("button[title=\"Add Widget\"]").First.WaitForAsync();
        var compactor = page.Locator("button.leftbar-compactor");
        if (await compactor.CountAsync() > 0 && await page.Locator("#left-nav span.title").First.IsVisibleAsync())
        {
            await compactor.First.ClickAsync();
        }

        if (collapseInputs)
        {
            foreach (var input in await FormCard(page, "Input").AllAsync())
            {
                await input.Locator(".widget-editor-btn-collapse").First.ClickAsync();
            }
        }
    }

    private static async Task AddWidgetToFormAsync(Scene scene, string type)
    {
        var page = scene.Page;
        var form = FormCard(page, "Form").First;
        await FastClickAsync(scene, form.Locator("button[title=\"Add Widget\"]").Last, 300);
        await FastClickAsync(scene, form.Locator($"a.add-widget[data-widget-type=\"{type}\"]").Last, 300);
    }

    private static async Task StandardLabelAsync(Scene scene, ILocator card, string label)
    {
        await scene.SelectAsync(Field(card, "FormElementLabelPart.LabelOption", "select"), "Standard", 300);
        await FastTypeAsync(scene, Field(card, "FormElementLabelPart.Label"), label, 15);
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        foreach (var group in new[] { "Tools" })
        {
            var button = page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\"))");
            await page.WaitForTimeoutAsync(600);
            if (await button.GetAttributeAsync("aria-expanded") != "true")
            {
                await button.ClickAsync();
            }
        }
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            var features = page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))");
            await PointAsync(scene, features);
            await scene.PauseAsync(600);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, features);
            await FastTypeAsync(scene, page.Locator("#search-box"), "Forms", 40);
            await PointAsync(scene, page.Locator("a[href*=\"/Features/OrchardCore.Forms/Enable\"]").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("a[href*=\"/Features/OrchardCore.Forms/Enable\"]").First);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Content\"))"), 300);
            var items = page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Content Items\"))").First;
            await PointAsync(scene, items);
            await scene.PauseAsync(600);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, items);
            await FastClickAsync(scene, page.Locator("#new-dropdown"));
            await PointAsync(scene, page.Locator("a.dropdown-item[href*=\"/Contents/ContentTypes/Page/Create\"]").First);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("a.dropdown-item[href*=\"/Contents/ContentTypes/Page/Create\"]").First);
            await FastTypeAsync(scene, page.Locator("input[name$=\"TitlePart.Title\"]").First, "Contact Us", 25);
            await PointAsync(scene, page.Locator("input[name$=\"AutoroutePart.Path\"]").First);
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var add = page.Locator("button[title=\"Add Widget\"]").Last;
            await add.ScrollIntoViewIfNeededAsync();
            await FastClickAsync(scene, add, 300);
            await FastClickAsync(scene, page.Locator("a.add-widget[data-widget-type=\"Form\"]").Last, 300);
            await FormCard(page, "Form").First.WaitForAsync();
        });

        await scene.StepAsync("create-6", async () =>
            await FastTypeAsync(scene, Field(FormCard(page, "Form").First, "TitlePart.Title"), "Contact", 40));

        await scene.StepAsync("create-7", async () =>
        {
            var form = FormCard(page, "Form").First;
            foreach (var (suffix, tag) in new[] { ("FormPart.Action", "input"), ("FormPart.Method", "select"), ("FormPart.EncType", "select") })
            {
                await PointAsync(scene, Field(form, suffix, tag));
                await scene.PauseAsync(1500);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("create-8", async () =>
        {
            var form = FormCard(page, "Form").First;
            await Field(form, "FormPart.EnableAntiForgeryToken").ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(Field(form, "FormPart.EnableAntiForgeryToken").Locator("xpath=.."));
            await scene.HighlightAsync(Field(form, "FormPart.SaveFormLocation").Locator("xpath=.."));
        });

        await scene.StepAsync("create-9", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button.draft").First);
        });
    }

    private static async Task TextAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenContactEditorAsync(scene);
        var page = scene.Page;
        await FormCard(page, "Form").First.ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("text-1", async () =>
        {
            await AddWidgetToFormAsync(scene, "Input");
            await FormCard(page, "Input").Nth(0).WaitForAsync();
        });

        await scene.StepAsync("text-2", async () =>
            await FastTypeAsync(scene, Field(FormCard(page, "Input").Nth(0), "FormInputElementPart.Name"), "Name", 40));

        await scene.StepAsync("text-3", async () =>
            await PointAsync(scene, Field(FormCard(page, "Input").Nth(0), "FormElementPart.Id")));

        await scene.StepAsync("text-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var card = FormCard(page, "Input").Nth(0);
            await PointAsync(scene, Field(card, "FormElementLabelPart.LabelOption", "select"));
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await StandardLabelAsync(scene, card, "Name");
        });

        await scene.StepAsync("text-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var card = FormCard(page, "Input").Nth(0);
            await PointAsync(scene, Field(card, "InputPart.Type", "select"));
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, Field(card, "InputPart.Placeholder"), "Your full name", 30);
            await PointAsync(scene, Field(card, "InputPart.DefaultValue"));
        });

        await scene.StepAsync("text-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, FormCard(page, "Input").Nth(0).Locator(".widget-editor-btn-collapse").First, 300);
            await AddWidgetToFormAsync(scene, "Input");
            var second = FormCard(page, "Input").Nth(1);
            await second.WaitForAsync();
            await FastTypeAsync(scene, Field(second, "FormInputElementPart.Name"), "Email", 8);
            await StandardLabelAsync(scene, second, "Email address");
            await scene.SelectAsync(Field(second, "InputPart.Type", "select"), "Email", 300);
            await FastTypeAsync(scene, Field(second, "InputPart.Placeholder"), "name@example.com", 8);
        });

        await scene.StepAsync("text-7", async () =>
            await FastGoAsync(scene, page.Locator("button.draft").First));
    }

    private static async Task RestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenContactEditorAsync(scene, collapseInputs: true);
        var page = scene.Page;
        await FormCard(page, "Form").First.ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("rest-1", async () =>
        {
            await AddWidgetToFormAsync(scene, "Select");
            var card = FormCard(page, "Select").First;
            await card.WaitForAsync();
            await FastTypeAsync(scene, Field(card, "FormInputElementPart.Name"), "Topic", 20);
            await StandardLabelAsync(scene, card, "What is this about");
        });

        await scene.StepAsync("rest-2", async () =>
            await PointAsync(scene, Field(FormCard(page, "Select").First, "SelectPart.Editor", "select")));

        await scene.StepAsync("rest-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var card = FormCard(page, "Select").First;
            foreach (var (text, value) in new[] { ("General question", "general"), ("Support request", "support"), ("Sales", "sales") })
            {
                await FastClickAsync(scene, card.Locator("a:has-text(\"Add an option\")").First, 200);
                await FastTypeAsync(scene, card.Locator("input[placeholder=\"Enter the text\"]").Last, text, 20);
                await FastTypeAsync(scene, card.Locator("input[placeholder=\"Enter a value\"]").Last, value, 20);
            }

            Console.WriteLine("default checkboxes checked: " + string.Join(",", await card.Locator("table input[type=checkbox], table input[type=radio]").EvaluateAllAsync<bool[]>("els => els.map(e => e.checked)")));
        });

        await scene.StepAsync("rest-4", async () =>
        {
            await FastClickAsync(scene, FormCard(page, "Select").First.Locator(".widget-editor-btn-collapse").First, 300);
            await AddWidgetToFormAsync(scene, "TextArea");
            var card = FormCard(page, "TextArea").First;
            await card.WaitForAsync();
            await FastTypeAsync(scene, Field(card, "FormInputElementPart.Name"), "Message", 10);
            await StandardLabelAsync(scene, card, "Message");
            await FastTypeAsync(scene, Field(card, "TextAreaPart.Placeholder"), "How can we help", 10);
            await FastTypeAsync(scene, Field(card, "TextAreaPart.Rows"), "6", 10);
        });

        await scene.StepAsync("rest-5", async () =>
        {
            await FastClickAsync(scene, FormCard(page, "TextArea").First.Locator(".widget-editor-btn-collapse").First, 300);
            await AddWidgetToFormAsync(scene, "Button");
            var card = FormCard(page, "Button").First;
            await card.WaitForAsync();
            await FastTypeAsync(scene, Field(card, "FormInputElementPart.Name"), "Send", 20);
            await FastTypeAsync(scene, Field(card, "ButtonPart.Text"), "Send message", 20);
            await PointAsync(scene, Field(card, "ButtonPart.Type", "select"));
        });

        await scene.StepAsync("rest-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button.publish").First);
        });
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenContactEditorAsync(scene);
        var page = scene.Page;
        await page.Locator("input[name$=\"AutoroutePart.Path\"]").First.ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            await PointAsync(scene, page.Locator("input[name$=\"AutoroutePart.Path\"]").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/contact-us");
        });

        await scene.StepAsync("site-2", async () =>
        {
            foreach (var locator in new[] { page.Locator("input[name=\"Name\"]"), page.Locator("input[name=\"Email\"]"), page.Locator("select[name=\"Topic\"]") })
            {
                await PointAsync(scene, locator);
                await scene.PauseAsync(1400);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("site-3", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input[name=\"Name\"]"), "Jamie Chen", 3);
            await FastTypeAsync(scene, page.Locator("input[name=\"Email\"]"), "jamie.chen@example.com", 3, fill: true);
            await scene.SelectAsync(page.Locator("select[name=\"Topic\"]"), "Support request", 150);
            await FastTypeAsync(scene, page.Locator("textarea[name=\"Message\"]"), "When is the office open?", 3, fill: true);
        });

        await scene.StepAsync("site-4", async () =>
            await PointAsync(scene, page.Locator("button[name=\"Send\"]")));
    }
}

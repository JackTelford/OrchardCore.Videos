using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-type", TypeAsync),
        ("03-widget", WidgetAsync),
        ("04-place", PlaceAsync),
        ("06-template", TemplateAsync),
    ];

    private const string BodyText = "<p class=\"text-center\">{{ Site.SiteName }} is open Monday to Friday, 9:00 to 17:00.</p>";

    private const string TemplateText = "<article class=\"{{ Model.Classes | join: \" \" }} text-center\">\n    <h3>{{ Model.ContentItem.DisplayText }}</h3>\n    {{ Model.Content.LiquidPart | shape_render }}\n</article>";

    // Nothing to seed: the Blog recipe is the starting state.
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task ExpandDesignAsync(IPage page, bool contentDefinition)
    {
        await page.WaitForTimeoutAsync(600);
        await page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Design\"))").ClickAsync();
        if (contentDefinition)
        {
            await page.Locator("#left-nav button.item-label:has(span.title:text-is(\"Content Definition\"))").ClickAsync();
        }
    }

    private static async Task ScrollFooterAsync(Scene scene)
    {
        var footer = scene.Page.Locator("footer").First;
        for (var i = 0; i < 6; i++)
        {
            var box = await footer.BoundingBoxAsync();
            if (box is not null && box.Y + box.Height <= 860 && box.Y >= 0)
            {
                return;
            }

            await scene.ScrollAsync(0, 1000, 8);
            await scene.PauseAsync(400);
        }
    }

    private static async Task TypeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await ExpandDesignAsync(page, true);
        await scene.StartCaptureAsync();

        await scene.StepAsync("type-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("a.item-label:text-is(\"Content Types\"), #left-nav a.item-label:has(span.title:text-is(\"Content Types\"))").First);
            await FastGoAsync(scene, page.Locator("a#btnCreate"));
        });

        await scene.StepAsync("type-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input#DisplayName"), "Liquid Widget", 25);
            await page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(1200);
            await FastGoAsync(scene, page.Locator("button.create[type=\"submit\"]"));
        });

        await scene.StepAsync("type-3", async () =>
        {
            var row = page.Locator("li.list-group-item:has(label:text-is(\"Liquid\"))");
            await scene.HighlightAsync(row, "Liquid");
            await FastClickAsync(scene, row.Locator("input[type=\"checkbox\"]"));
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1500);
            await FastGoAsync(scene, page.Locator("#add-selected-parts-button"));
        });

        await scene.StepAsync("type-4", async () =>
        {
            await page.Locator("h3:text-is(\"Parts\")").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 250, 8);
            await scene.HighlightAsync(page.Locator("ul#parts"), "Parts");
        });
    }

    private static async Task WidgetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/LiquidWidget");
        var page = scene.Page;
        await page.Locator("label:text-is(\"Creatable\")").First.ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("widget-1", async () =>
        {
            foreach (var name in new[] { "Creatable", "Listable" })
            {
                await scene.HighlightAsync(page.Locator($".form-check:has(label:text-is(\"{name}\"))").First, name);
                await scene.PauseAsync(800);
                await FastClickAsync(scene, page.Locator($"label:text-is(\"{name}\")").First, 200);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("widget-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator(".ocat-limited-wrapper:has(label:text-is(\"Stereotype\")) input").First, "Widget", 60);
            await scene.PauseAsync(1200);
            await FastGoAsync(scene, page.Locator("button[name=\"submit.Save\"][value=\"Save\"]").First);
        });
    }

    private static async Task PlaceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await ExpandDesignAsync(page, false);
        await scene.StartCaptureAsync();
        var footerCard = page.Locator("div.card:has(> .card-header:text-is(\"Footer\"))");

        await scene.StepAsync("place-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Widgets\"))"));
            await footerCard.Locator("button.dropdown-toggle:has-text(\"Add Widget\")").First.ScrollIntoViewIfNeededAsync();
            await FastClickAsync(scene, footerCard.Locator("button.dropdown-toggle:has-text(\"Add Widget\")").First, 300);
            await FastGoAsync(scene, page.Locator("a.add-list-widget[data-zone=\"Footer\"][data-widget-type=\"LiquidWidget\"]"));
        });

        await scene.StepAsync("place-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator(".ocat-wrapper:has(label:text-is(\"Title\")) input").First, "Opening hours", 30);
            await scene.SelectAsync(page.Locator(".ocat-wrapper:has(label:text-is(\"The layer\")) select").First, "Always", 200);
            await scene.HighlightAsync(page.Locator(".form-check:has(label:text-is(\"Render title\"))").First, "Cleared");
        });

        await scene.StepAsync("place-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator(".ocat-wrapper:has(label:text-is(\"Body\")) .CodeMirror").First, 200);
            await page.Locator(".CodeMirror").First.EvaluateAsync("(e, t) => e.CodeMirror.setValue(t)", BodyText);
            var ok = await page.Locator(".CodeMirror").First.EvaluateAsync<string>("e => e.CodeMirror.getValue()");
            if (ok != BodyText)
            {
                throw new InvalidOperationException("The body does not hold the text: " + ok);
            }

            await scene.PauseAsync(1000);
            await FastGoAsync(scene, page.Locator("button.publish[name=\"submit.Publish\"]").First);
        });

        await scene.StepAsync("place-4", async () =>
            await scene.HighlightAsync(footerCard.Locator("li.list-group-item:has-text(\"Opening hours\")").First, "Opening hours"));

        await scene.StepAsync("place-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/");
            await ScrollFooterAsync(scene);
            await scene.HighlightAsync(page.Locator("footer p.text-center:has-text(\"is open Monday\")").First, "Liquid widget");
        });
    }

    private static async Task TemplateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await ExpandDesignAsync(page, false);
        await scene.StartCaptureAsync();

        await scene.StepAsync("template-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Templates\"))"));
            await FastGoAsync(scene, page.Locator("a.create:text-is(\"Add Template\")"));
            await FastTypeAsync(scene, page.Locator("input#Name"), "Widget__LiquidWidget", 15);
        });

        await scene.StepAsync("template-2", async () =>
        {
            await FastClickAsync(scene, page.Locator(".template-editor").First, 200);
            await page.WaitForSelectorAsync(".template-editor .monaco-editor");
            await page.EvaluateAsync("async (text) => { const monaco = await window.__orchardCoreMonacoReady; monaco.editor.getModels()[0].setValue(text); }", TemplateText);
            var ok = await page.EvaluateAsync<bool>("async (text) => { const monaco = await window.__orchardCoreMonacoReady; return monaco.editor.getModels()[0].getValue() === text; }", TemplateText);
            if (!ok)
            {
                throw new InvalidOperationException("The editor does not hold the template.");
            }

            await scene.PauseAsync(1200);
            await FastGoAsync(scene, page.Locator("button.save[type=\"submit\"][value=\"Save\"]").First);
        });

        await scene.StepAsync("template-3", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/");
                await page.Locator("footer").First.ScrollIntoViewIfNeededAsync();
                await page.EvaluateAsync("() => window.scrollTo(0, document.body.scrollHeight)");
            });
            await scene.HighlightAsync(page.Locator("footer article.widget-liquid-widget h3").First, "From the template");
        });

        await scene.StepAsync("template-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/ContentTypes/Edit/LiquidWidget");
            await FastClickAsync(scene, page.Locator("button.dropdown-toggle:text-is(\"Edit templates\")").First);
            await scene.HighlightAsync(page.Locator(".dropdown-menu.show").First, "Edit templates");
        });
    }
}

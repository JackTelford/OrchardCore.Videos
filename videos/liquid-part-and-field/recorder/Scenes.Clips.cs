using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-part", PartAsync),
        ("03-field", FieldAsync),
    ];

    private const string PartTemplate = "<p>Welcome to {{ Site.SiteName }}. Updated {{ ContentItem.ModifiedUtc | local | date: \"%B %d, %Y\" }}.</p>";

    // Seed: the content type Notice with the Title part.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input#DisplayName").FillAsync("Notice");
        await page.Keyboard.PressAsync("Tab");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.create[type=\"submit\"]").ClickAsync());
        var row = page.Locator("li.list-group-item:has(label:text-is(\"Title\"))").First;
        await row.Locator("input[type=\"checkbox\"]").CheckAsync();
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("#add-selected-parts-button").ClickAsync());
        Console.WriteLine("Notice parts: " + string.Join(" | ", await page.Locator("ul#parts li").AllInnerTextsAsync()));
        await context.CloseAsync();
    }

    private static async Task TypeEditorAsync(Scene scene, string text)
    {
        await scene.Page.Keyboard.TypeAsync(text, new KeyboardTypeOptions { Delay = 25 });
    }

    private static async Task ViewInNewTabAsync(Scene scene, IPage page)
    {
        var row = page.Locator("li.list-group-item:has-text(\"Open House\")").First;
        var tab = await scene.Context.RunAndWaitForPageAsync(async () => await FastClickAsync(scene, row.Locator("a.btn.view").First, 200));
        await tab.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await scene.SwitchToAsync(tab);
        Console.WriteLine("View tab: " + tab.Url);
    }

    private static async Task PartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Notice");
        var page = scene.Page;
        await page.Locator("h3:text-is(\"Parts\")").First.ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("part-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("a[role=button]:has-text(\"Add Parts\")").First);
            await page.GetByText("Choose the Parts to add to this Content Type.").First.WaitForAsync();
        });

        await scene.StepAsync("part-2", async () =>
        {
            var row = page.Locator("li.list-group-item").Filter(new() { HasText = "Provides a Liquid formatted body for your content item." }).First;
            await scene.HighlightAsync(row);
            await FastClickAsync(scene, row.Locator("input[type=\"checkbox\"]"), 300);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("#add-selected-parts-button"));
        });

        await scene.StepAsync("part-3", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Admin/Contents/ContentItems"));
            await FastClickAsync(scene, page.Locator("#new-dropdown"), 300);
            await FastGoAsync(scene, page.Locator("a.dropdown-item:text-is(\"Notice\")").First);
            await FastTypeAsync(scene, page.Locator("input[name$=\"TitlePart.Title\"]").First, "Open House", 25);
            await page.Locator(".ocat-wrapper:has(textarea.liquid-part-editor) .CodeMirror").First.WaitForAsync();
            await page.Locator(".ocat-wrapper:has(textarea.liquid-part-editor) .CodeMirror").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 150, 6);
        });

        await scene.StepAsync("part-4", async () =>
        {
            await FastClickAsync(scene, page.Locator(".ocat-wrapper:has(textarea.liquid-part-editor) .CodeMirror").First, 200);
            await scene.Page.Keyboard.TypeAsync(PartTemplate, new KeyboardTypeOptions { Delay = 12 });
        });

        await scene.StepAsync("part-5", async () =>
        {
            await FastClickAsync(scene, page.Locator(".ocat-wrapper:has(textarea.liquid-part-editor) .CodeMirror").First, 200);
            await page.Keyboard.PressAsync("Control+End");
            await page.Keyboard.TypeAsync(" {% if true %}", new KeyboardTypeOptions { Delay = 20 });
            await FastGoAsync(scene, page.Locator("button.publish").First);
            await page.Locator(".validation-summary-errors, .alert-danger, .field-validation-error").First.WaitForAsync();
        });

        await scene.StepAsync("part-6", async () =>
        {
            var editor = page.Locator(".ocat-wrapper:has(textarea.liquid-part-editor) .CodeMirror").First;
            await editor.ScrollIntoViewIfNeededAsync();
            await FastClickAsync(scene, editor, 200);
            await page.Keyboard.PressAsync("Control+End");
            if ((await page.Locator(".ocat-wrapper:has(textarea.liquid-part-editor) .CodeMirror-code").First.InnerTextAsync()).Contains("{% if true %}"))
            {
                for (var i = 0; i < 14; i++)
                {
                    await page.Keyboard.PressAsync("Backspace");
                }
            }
            else
            {
                await page.Keyboard.TypeAsync(PartTemplate, new KeyboardTypeOptions { Delay = 6 });
            }

            if (string.IsNullOrEmpty(await page.Locator("input[name$=\"TitlePart.Title\"]").First.InputValueAsync()))
            {
                await FastTypeAsync(scene, page.Locator("input[name$=\"TitlePart.Title\"]").First, "Open House", 15);
            }

            await FastGoAsync(scene, page.Locator("button.publish").First);
            await ViewInNewTabAsync(scene, page);
        });
    }

    private static async Task FieldAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Notice");
        var page = scene.Page;
        await page.Locator("a[role=button]:has-text(\"Add Field\")").First.ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("field-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("a[role=button]:has-text(\"Add Field\")").First);
            await FastTypeAsync(scene, page.Locator("input#DisplayName"), "Closing Note", 25);
            await page.Keyboard.PressAsync("Tab");
            await page.WaitForFunctionAsync("() => document.querySelector('#Name') && document.querySelector('#Name').value === 'ClosingNote'");
        });

        await scene.StepAsync("field-2", async () =>
        {
            var radio = page.Locator("label:has-text(\"Liquid Field\")").First;
            await radio.ScrollIntoViewIfNeededAsync();
            await FastClickAsync(scene, radio, 300);
            await FastGoAsync(scene, page.Locator("button.save").First);
            await page.Locator("h3:text-is(\"Fields\")").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 150, 6);
        });

        await scene.StepAsync("field-3", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Admin/Contents/ContentItems"));
            await FastGoAsync(scene, page.Locator("li.list-group-item:has-text(\"Open House\") a.btn.edit").First);
            var editor = page.Locator("div.field-wrapper-notice-closing-note .CodeMirror").First;
            await editor.WaitForAsync();
            await editor.ScrollIntoViewIfNeededAsync();
            await FastClickAsync(scene, editor, 200);
            await page.Keyboard.TypeAsync("<p><a href=\"{{ ContentItem | display_url }}\">Link to this notice</a></p>", new KeyboardTypeOptions { Delay = 12 });
        });

        await scene.StepAsync("field-4", async () =>
        {
            await FastGoAsync(scene, page.Locator("button.publish").First);
            await scene.PauseAsync(1200);
            await scene.OffCameraAsync(async () => await ViewInNewTabAsync(scene, page));
            await scene.Page.Locator("a:has-text(\"Link to this notice\")").First.WaitForAsync();
            await scene.Page.EvaluateAsync("() => window.scrollBy(0, 250)");
        });
    }
}

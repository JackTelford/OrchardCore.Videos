using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-create", CreateAsync),
        ("04-use", UseAsync),
    ];

    private const string UsageText = "[primary text=\"Read this first.\"]\n[primary]Read this first.[/primary]";

    private const string ContentText = "{% capture output %}\n{% if Args.Text != nil %}\n<span class=\"text-primary\">{{ Args.Text }}</span>\n{% else %}\n<span class=\"text-primary\">{{ Content }}</span>\n{% endif %}\n{% endcapture %}\n{{ output | sanitize_html | raw }}";

    // Seed A: Shortcode Templates disabled.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"feature\",\"enable\":[],\"disable\":[\"OrchardCore.Shortcodes.Templates\"]}]}");
        await Task.Delay(6000);
        var status = await page.APIRequest.GetAsync(BaseUrl + "/Admin/Shortcodes");
        Console.WriteLine("/Admin/Shortcodes status: " + status.Status);
        await context.CloseAsync();
    }

    private static async Task OpenGroupAsync(IPage page, string group)
    {
        var button = page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\"))");
        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await button.ClickAsync();
            await page.WaitForTimeoutAsync(400);
        }
    }

    private static Task SetCodeAsync(IPage page, string id, string text)
        => page.EvaluateAsync("([id, text]) => document.querySelector('#' + id + ' + .CodeMirror').CodeMirror.setValue(text)", new object[] { id, text });

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#search-box"), "shortcode", 50);
            var row = page.Locator("[data-filter-value*='OrchardCore.Shortcodes.Templates']").First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row.Locator(".direct-dependencies").First, "Dependencies");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = page.Locator("[data-filter-value*='OrchardCore.Shortcodes.Templates']").First;
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(row.Locator("a.btn.btn-primary:text-is(\"Enable\")").First));
            await page.GetByText("The feature Shortcode Templates was enabled.").First.WaitForAsync();
            var design = page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Design\"))");
            if (await design.GetAttributeAsync("aria-expanded") != "true")
            {
                await FastClickAsync(scene, design, 700);
            }

            await page.Locator("#left-nav").GetByText("Shortcodes", new() { Exact = true }).Locator("visible=true").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("#left-nav").GetByText("Shortcodes", new() { Exact = true }).Locator("visible=true").First);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await OpenGroupAsync(page, "Design");
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("#left-nav").GetByText("Shortcodes", new() { Exact = true }).Locator("visible=true").First);
            await scene.PauseAsync(600);
            await FastGoAsync(scene, page.Locator("a.create:has-text(\"Add Shortcode Template\")").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#Name"), "primary", 40);
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#Hint"), "Formats text in the primary color of the theme.", 12);
            await SetCodeAsync(page, "Usage", UsageText);
            await scene.ScrollAsync(0, 450, 8);
        });

        await scene.StepAsync("create-4", async () =>
        {
            var categories = page.Locator("#shortcodeCategories .multiselect").First;
            await categories.ScrollIntoViewIfNeededAsync();
            await FastClickAsync(scene, categories, 300);
            var option = page.Locator("#shortcodeCategories .multiselect__option:has-text(\"HTML Content\")").First;
            if (await option.CountAsync() > 0)
            {
                await FastClickAsync(scene, option, 300);
            }
            else
            {
                await page.Keyboard.TypeAsync("HTML Content");
                await page.Keyboard.PressAsync("Enter");
            }

            await FastTypeAsync(scene, page.Locator("#DefaultValue"), "[primary]Read this first.[/primary]", 10);
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ScrollAsync(0, 450, 8);
            await SetCodeAsync(page, "Content", ContentText);
            await page.Locator("#Content + .CodeMirror").First.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
        });

        await scene.StepAsync("create-6", async () =>
            await FastGoAsync(scene, page.Locator("button.save").First));

        await scene.StepAsync("create-7", async () =>
        {
            await scene.HighlightAsync(page.Locator("#search-box"), "Search");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            var row = page.Locator("li.list-group-item:has-text(\"primary\")").First;
            await scene.HighlightAsync(row.Locator("a.btn.btn-primary, a.edit").First);
            await scene.HighlightAsync(row.Locator("a.btn-danger, a.delete").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            var box = row.Locator("input[name=\"itemIds\"]").First;
            await FastClickAsync(scene, box, 800);
            await FastClickAsync(scene, box, 200);
        });
    }

    private static async Task UseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        var page = scene.Page;
        await page.Locator("li.list-group-item:has-text(\"About\") a.edit").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var body = page.Locator(".trumbowyg-editor").First;
        await body.WaitForAsync();
        await body.Locator("p").Last.ClickAsync();
        await page.Keyboard.PressAsync("Control+End");
        await page.Keyboard.PressAsync("Enter");
        await scene.StartCaptureAsync();

        await scene.StepAsync("use-1", async () =>
        {
            var button = page.Locator("button.trumbowyg-insertShortcode-button").First;
            await scene.HighlightAsync(button);
            await scene.PauseAsync(500);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, button, 200);
            await page.Locator("#shortcodeModal").WaitForAsync();
        });

        await scene.StepAsync("use-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#shortcodeModal input[type=\"search\"]").First, "prim", 60);
            var card = page.Locator("#shortcodeModal .card").Filter(new() { HasText = "primary" }).First;
            await scene.HighlightAsync(card);
            await scene.PauseAsync(700);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, card.Locator("a:text-is(\"Add\"), button:text-is(\"Add\")").First, 300);
        });

        await scene.StepAsync("use-3", async () =>
        {
            await scene.HighlightAsync(page.Locator(".trumbowyg-editor").GetByText("[primary]").First);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button.publish").First);
        });

        await scene.StepAsync("use-4", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/about"));
            await page.Locator("span.text-primary").First.WaitForAsync();
            await page.Locator("span.text-primary").First.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(page.Locator("span.text-primary").First);
        });
    }
}

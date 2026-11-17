using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-open", OpenAsync),
        ("03-any-page", AnyPageAsync),
    ];

    private const string TemplateRecipe = "{\"steps\":[{\"name\":\"Templates\",\"Templates\":{\"Content__BlogPost\":{\"Description\":\"Detail view of a blog post\",\"Content\":\"<article style=\\\"padding-top: 7rem\\\">\\n    <p class=\\\"lead\\\">Posted by</p>\\n    <h1>{{ Model.ContentItem.DisplayText }}</h1>\\n    {{ Model.Content | shape_render }}\\n</article>\"}}}]}";

    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await ImportRecipeAsync(page, TemplateRecipe);
        await page.GotoAsync(BaseUrl + "/Admin/Templates", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Templates: " + (await page.Locator("body").InnerTextAsync()).Contains("Content__BlogPost"));
        await page.GotoAsync(BaseUrl + "/blog/post-1", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var text = await page.Locator("body").InnerTextAsync();
        Console.WriteLine("Post: " + text.Contains("Posted by") + " " + text.Contains("Man must explore"));
        await context.CloseAsync();
    }

    private static Task BlockFontsAsync(Scene scene) => scene.Context.RouteAsync(new Regex(@"^https://fonts\.(googleapis|gstatic)\.com/"), route => route.AbortAsync());

    // In this build the preview page never loads its own script (OrchardCore.Templates Views/Preview/Index.cshtml:29, after the
    // resources tag of line 27), so the script is added to the tab off camera; the feature then behaves as designed.
    private static async Task LoadPreviewScriptAsync(IPage preview)
    {
        await preview.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await preview.AddScriptTagAsync(new PageAddScriptTagOptions { Url = "/OrchardCore.Templates/Scripts/template-editors/template-preview.js", Type = "module" });
        await preview.WaitForTimeoutAsync(800);
    }

    // The preview frame can stay empty on the first load: reload the preview tab until it shows the blog home page.
    private static async Task EnsurePreviewAsync(IPage preview)
    {
        for (var i = 0; i < 4; i++)
        {
            try
            {
                await preview.FrameLocator("#iframe").Locator(".post-preview").First.WaitForAsync(new() { Timeout = 6000 });
                return;
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Preview empty, reloading.");
                await preview.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });
                await LoadPreviewScriptAsync(preview);
            }
        }

        throw new InvalidOperationException("The preview never showed the page.");
    }

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await BlockFontsAsync(scene);
        await scene.OpenAsync("/Admin/Templates");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        var design = page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Design\"))");
        if (await design.GetAttributeAsync("aria-expanded") != "true")
        {
            await design.ClickAsync();
            await page.WaitForTimeoutAsync(400);
        }

        await scene.StartCaptureAsync();
        IPage preview = null;

        await scene.StepAsync("open-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("a[href$='/Admin/Templates']").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("a:has-text(\"Content__BlogPost\")").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("open-2", async () =>
        {
            await FastGoAsync(scene, page.Locator("a:has-text(\"Content__BlogPost\")").First);
            await page.Locator(".template-editor .monaco-editor").WaitForAsync();
            await scene.HighlightAsync(page.Locator("#Name"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".template-editor").First);
        });

        await scene.StepAsync("open-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var button = page.Locator("a.btn-info.action-bar-secondary").First;
            await scene.HighlightAsync(button);
            preview = await scene.Context.RunAndWaitForPageAsync(async () => await FastClickAsync(scene, button, 200));
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("open-4", async () =>
        {
            await preview.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await LoadPreviewScriptAsync(preview);
            await EnsurePreviewAsync(preview);
            await scene.SwitchToAsync(preview);
        });
    }

    private static async Task AnyPageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await BlockFontsAsync(scene);
        var editor = scene.Page;
        await scene.OpenAsync("/Admin/Templates/Edit/Content__BlogPost");
        await editor.Locator(".template-editor .monaco-editor").WaitForAsync();
        var preview = await scene.Context.RunAndWaitForPageAsync(async () => await editor.Locator("a.btn-info.action-bar-secondary").ClickAsync());
        await preview.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var frame = preview.FrameLocator("#iframe");
        await LoadPreviewScriptAsync(preview);
        await EnsurePreviewAsync(preview);
        await editor.BringToFrontAsync();
        await editor.EvaluateAsync("window.scrollTo(0, 0)");
        await scene.StartCaptureAsync();

        await scene.StepAsync("any-1", async () =>
        {
            await scene.SwitchToAsync(preview);
            await FastClickAsync(scene, frame.Locator(".post-preview a").First, 200);
            await frame.Locator("article h1").WaitForAsync();
        });

        await scene.StepAsync("any-2", async () =>
        {
            await scene.SwitchToAsync(editor);
            await FastClickAsync(scene, editor.Locator(".template-editor .view-lines").First, 200);
            await editor.Keyboard.PressAsync("Control+Home");
            await editor.Keyboard.PressAsync("ArrowDown");
            await editor.Keyboard.PressAsync("End");
            for (var i = 0; i < 4; i++)
            {
                await editor.Keyboard.PressAsync("ArrowLeft");
            }

            await editor.Keyboard.TypeAsync(" the editorial team", new KeyboardTypeOptions { Delay = 70 });
            await editor.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("any-3", async () =>
        {
            await frame.GetByText("Posted by the editorial team").First.WaitForAsync();
            await scene.SwitchToAsync(preview);
        });

        await scene.StepAsync("any-4", async () =>
        {
            await scene.SwitchToAsync(editor);
            await FastGoAsync(scene, editor.Locator("button.btn.save"));
        });

        await scene.StepAsync("any-5", async () =>
        {
            await preview.Locator("#notConnectedWarning").WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await scene.SwitchToAsync(preview);
            await scene.HighlightAsync(preview.Locator("#notConnectedWarning"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });
    }
}

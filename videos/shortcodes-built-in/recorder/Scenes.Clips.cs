using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-image", ImageAsync),
        ("03-options", OptionsAsync),
        ("04-list", ListAsync),
    ];

    // Creates the article Spring Garden Tour with three plain lines, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Article/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#TitlePart_Title").FillAsync("Spring Garden Tour");
        await page.Locator(".trumbowyg-editor").ClickAsync();
        await page.Keyboard.TypeAsync("Join us for a walk through the spring garden. The tulips are in bloom, and our volunteers are glad to answer questions.");
        await page.Keyboard.PressAsync("Enter");
        await page.Keyboard.TypeAsync("Photo from the entrance:");
        await page.Keyboard.PressAsync("Enter");
        await page.Keyboard.TypeAsync("Address of the photo:");
        await page.Locator("#Article_Subtitle_Text").FillAsync("A walk with the volunteers");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").ClickAsync());
        await page.GotoAsync(BaseUrl + "/spring-garden-tour", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Page: " + (await page.Locator("body").InnerTextAsync()).Replace("\n", " | ")[..Math.Min(300, (await page.Locator("body").InnerTextAsync()).Length)]);
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems/Article", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var edit = await page.Locator(".list-group-item").Filter(new() { HasText = "Spring Garden Tour" }).Locator("a.btn.edit").First.GetAttributeAsync("href");
        await page.GotoAsync(BaseUrl + edit, new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Editor paragraphs: " + string.Join(" | ", await page.Locator(".trumbowyg-editor > *").EvaluateAllAsync<string[]>("els => els.map(e => e.tagName + ':' + e.innerText.slice(0, 30))")));
        await context.CloseAsync();
    }

    private static ILocator Editor(IPage page) => page.Locator(".trumbowyg-editor");

    private static ILocator Line(IPage page, string text) => page.Locator(".trumbowyg-editor > *").Filter(new() { HasText = text }).First;

    private static async Task OpenEditorAsync(Scene scene)
    {
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems/Article");
        await page.Locator(".list-group-item").Filter(new() { HasText = "Spring Garden Tour" }).Locator("a.btn.edit").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Editor(page).WaitForAsync();
    }

    private static async Task EndOfLineAsync(Scene scene, string text)
    {
        await FastClickAsync(scene, Line(scene.Page, text), 150);
        await scene.Page.Keyboard.PressAsync("End");
    }

    private static async Task TypeInEditorAsync(IPage page, string text, int delay = 40)
        => await page.Keyboard.TypeAsync(text, new() { Delay = delay });

    private static async Task PublishAndLookAsync(Scene scene)
    {
        var page = scene.Page;
        await FastGoAsync(scene, page.Locator("button.publish").First);
        await scene.OffCameraAsync(async () =>
        {
            await scene.NavigateAsync("/spring-garden-tour");
            await page.Locator("img[src*='home-bg.jpg']").First.WaitForAsync();
            await page.GetByText("Photo from the entrance").First.EvaluateAsync("e => { const y = e.getBoundingClientRect().top + window.scrollY - 140; window.scrollTo({ top: y, behavior: 'instant' }); }");
        });
    }

    private static async Task ImageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/Article");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("image-1", async () =>
        {
            await scene.PauseAsync(1500);
            await FastGoAsync(scene, page.Locator(".list-group-item").Filter(new() { HasText = "Spring Garden Tour" }).Locator("a.btn.edit").First);
            await Editor(page).WaitForAsync();
            await scene.HighlightAsync(page.Locator("button[title='Insert Shortcode']"), "Insert Shortcode");
            await scene.HighlightAsync(page.Locator("button[title='Insert Media']"), "Insert Media");
        });

        await scene.StepAsync("image-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await EndOfLineAsync(scene, "Photo from the entrance");
            await FastClickAsync(scene, page.Locator("button[title='Insert Media']"), 300);
            await page.Locator(".mf-picker-shell").WaitForAsync();
            await page.Locator(".file-item").First.WaitForAsync();
        });

        await scene.StepAsync("image-3", async () =>
        {
            await FastClickAsync(scene, page.Locator(".file-item").Filter(new() { HasText = "home-bg.jpg" }).Locator("input[name='select-file']"), 1500);
            await FastClickAsync(scene, page.Locator(".mf-picker-shell button:text-is(\"OK\")").First);
            await page.Locator(".mf-picker-shell").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        });

        await scene.StepAsync("image-4", async () => await PublishAndLookAsync(scene));
    }

    private static async Task OptionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("options-1", async () =>
        {
            await EndOfLineAsync(scene, "Photo from the entrance");
            await page.Keyboard.PressAsync("ArrowLeft", new() { Delay = 0 });
            for (var i = 0; i < 19; i++)
            {
                await page.Keyboard.PressAsync("ArrowLeft");
            }

            await TypeInEditorAsync(page, " width=\"480\"", 40);
        });

        await scene.StepAsync("options-2", async () => await TypeInEditorAsync(page, " alt=\"Spring tulips\"", 40));

        await scene.StepAsync("options-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await PublishAndLookAsync(scene);
        });
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("list-1", async () =>
        {
            await EndOfLineAsync(scene, "Address of the photo");
            await page.Keyboard.TypeAsync(" ");
            await FastClickAsync(scene, page.Locator("button[title='Insert Shortcode']"), 300);
            await page.Locator("#shortcodeModal").WaitForAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(page.Locator("#shortcodeModal .nav-pills"), "Categories");
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#shortcodeModal input[type='search']"), "asset", 60);
            await FastClickAsync(scene, page.Locator("#shortcodeModal button:visible").Filter(new() { HasText = "Add" }).First);
            await page.Locator("#shortcodeModal").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        });

        await scene.StepAsync("list-3", async () =>
        {
            await EndOfLineAsync(scene, "Address of the photo");
            for (var i = 0; i < 12; i++)
            {
                await page.Keyboard.PressAsync("ArrowLeft");
            }

            await page.Keyboard.PressAsync("Backspace");
            await TypeInEditorAsync(page, "post-bg.jpg", 40);
        });

        await scene.StepAsync("list-4", async () =>
        {
            var publish = page.Locator("button.publish").First;
            await FastGoAsync(scene, publish);
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/spring-garden-tour");
                await page.Locator("img[src*='home-bg.jpg']").First.WaitForAsync();
                await page.GetByText("Photo from the entrance").First.EvaluateAsync("e => { const y = e.getBoundingClientRect().top + window.scrollY - 140; window.scrollTo({ top: y, behavior: 'instant' }); }");
            });
        });
    }
}

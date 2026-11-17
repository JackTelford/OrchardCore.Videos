using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-browse", BrowseAsync),
        ("03-routing", RoutingAsync),
        ("04-paging", PagingAsync),
    ];

    private static readonly (string Title, string Subtitle, string Body, string[] Tags)[] s_posts =
    [
        ("A week on the Amalfi coast", "Lemon groves, steep stairs, and slow ferries", "The ferry leaves at nine, and the lemons are the size of grapefruits.", ["Earth"]),
        ("Night trains across the continent", "Sleep somewhere between two capitals", "The sleeper car rocks gently, and the conductor brings tea at dawn.", ["Exploration"]),
        ("Packing light for a month away", "One bag, three shirts, and a good map", "Roll the shirts, fold the map, and leave the second pair of shoes at home.", []),
        ("A weekend in the mountains", "Thin air and long views", "The trail climbs for two hours, and the hut at the top serves soup.", ["Earth"]),
        ("Where to watch the night sky", "Dark places, clear skies, and a thermos of tea", "Drive an hour from the nearest town, switch off the lamp, and wait twenty minutes.", ["Space", "Exploration"]),
    ];

    // Creates five published blog posts in the Blog, one after the other, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("a:text-is(\"List BlogPost Items\")").ClickAsync());
        var create = await page.Locator("a:has-text(\"Create Blog Post\")").First.GetAttributeAsync("href");

        foreach (var (title, subtitle, body, tags) in s_posts)
        {
            await page.GotoAsync(BaseUrl + create, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator("#BlogPost_Subtitle_Text").FillAsync(subtitle);
            await page.Locator(".CodeMirror").First.EvaluateAsync("(e, t) => e.CodeMirror.setValue(t)", body);
            await page.Locator(".field-wrapper-blog-post-category label.form-check-label:has-text('Travel')").First.ClickAsync();

            foreach (var tag in tags)
            {
                await page.Locator(".field-wrapper-blog-post-tags .multiselect__tags").ClickAsync();
                await page.Locator(".field-wrapper-blog-post-tags input.multiselect__input").PressSequentiallyAsync(tag, new() { Delay = 30 });
                await page.Locator($".field-wrapper-blog-post-tags .multiselect__option:has-text('{tag}')").First.ClickAsync();
                await page.Keyboard.PressAsync("Escape");
            }

            await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
            await Task.Delay(2200);
        }

        foreach (var path in new[] { "/categories/travel", "/tags/earth", "/tags/exploration", "/tags/space" })
        {
            await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.NetworkIdle });
            Console.WriteLine(path + ": " + string.Join(" | ", await page.Locator(".post-preview h2.post-title").EvaluateAllAsync<string[]>("els => els.map(e => e.innerText.trim())")));
        }

        await context.CloseAsync();
    }

    private static async Task BrowseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        var page = scene.Page;
        await page.Locator(".post-preview").First.WaitForAsync();
        await page.Locator(".post-preview").First.EvaluateAsync("e => { window.scrollTo({ top: e.getBoundingClientRect().top + window.scrollY - 120, behavior: 'instant' }); }");
        await scene.StartCaptureAsync();

        await scene.StepAsync("browse-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".post-preview .term-badge").First);
            await scene.MoveToAsync(page.Locator(".post-preview .term-badge a").First);
        });

        await scene.StepAsync("browse-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator(".term-badge a", new() { HasText = "Travel" }).First);
            await page.Locator("header.masthead h1").WaitForAsync();
            await scene.HighlightAsync(page.Locator("header.masthead h1"), "/categories/travel");
        });

        await scene.StepAsync("browse-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            for (var i = 0; i < 3; i++)
            {
                await scene.ScrollAsync(0, 220, 8);
                await scene.PauseAsync(1500);
            }
        });

        await scene.StepAsync("browse-4", async () =>
        {
            await scene.ScrollAsync(0, -1000, 8);
            await FastGoAsync(scene, page.Locator(".term-badge a", new() { HasText = "Space" }).First);
            await page.Locator("header.masthead h1").WaitForAsync();
        });

        await scene.StepAsync("browse-5", async () =>
        {
            await scene.NavigateAsync("/tags");
            await page.Locator(".post-preview a").First.WaitForAsync();
            await scene.PauseAsync(2000);
            await FastGoAsync(scene, page.Locator(".post-preview a", new() { HasText = "Earth" }).First);
        });

        await scene.StepAsync("browse-6", async () =>
        {
            await scene.ScrollAsync(0, 450, 8);
            await scene.HighlightAsync(page.Locator(".post-preview").Last.Locator(".term-badge"));
        });
    }

    private static async Task RoutingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("routing-1", async () =>
        {
            await FastTypeAsync(scene, page.GetByPlaceholder("Search").First, "Categories", 20);
            await scene.WaitForNavigationAsync(async () => await page.Keyboard.PressAsync("Enter"));
            await page.Locator("a.btn.edit").First.WaitForAsync();
            await scene.PauseAsync(800);
            await FastGoAsync(scene, page.Locator("a.btn.edit").First);
        });

        await scene.StepAsync("routing-2", async () => await scene.HighlightAsync(page.GetByLabel("Permalink")));

        await scene.StepAsync("routing-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByLabel("Route contained items"));
        });

        await scene.StepAsync("routing-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 500, 8);
            await scene.PauseAsync(500);
            await FastGoAsync(scene, page.Locator("li.menu-item", new() { HasText = "Travel" }).GetByRole(AriaRole.Link, new() { Name = "Edit" }));
            await page.GetByLabel("Permalink").WaitForAsync();
            await scene.HighlightAsync(page.GetByLabel("Permalink"));
        });
    }

    private static async Task PagingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/general");
        var page = scene.Page;
        await page.GetByLabel("Page size", new() { Exact = true }).ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("paging-1", async () => await scene.HighlightAsync(page.GetByLabel("Page size", new() { Exact = true })));

        await scene.StepAsync("paging-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.GetByLabel("Page size", new() { Exact = true }));
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("3", new() { Delay = 120 });
            await scene.PauseAsync(600);
            var save = page.Locator("button.save").First;
            var (x, y) = await scene.CenterOfAsync(save);
            await scene.MoveToAsync(x, y, 10);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await page.Mouse.ClickAsync((float)x, (float)y));
                await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
                await page.Context.APIRequest.GetAsync(BaseUrl + "/categories/travel");
            });
            await page.GetByLabel("Page size", new() { Exact = true }).ScrollIntoViewIfNeededAsync();
        });

        await scene.StepAsync("paging-3", async () =>
        {
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/categories/travel"));
            await page.Locator("header.masthead h1").WaitForAsync();
            await page.Locator("ul.pagination").First.EvaluateAsync("e => e.scrollIntoView({block: 'center'})");
        });

        await scene.StepAsync("paging-4", async () =>
        {
            await FastGoAsync(scene, page.Locator("ul.pagination a", new() { HasTextRegex = new Regex("^2$") }).First);
            await page.Locator(".post-preview").First.EvaluateAsync("e => window.scrollTo({ top: e.getBoundingClientRect().top + window.scrollY - 140, behavior: 'instant' })");
        });
    }
}

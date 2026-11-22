using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-feature", FeatureAsync),
        ("03-taxonomy", TaxonomyAsync),
        ("04-categorize", CategorizeAsync),
        ("05-site", SiteAsync),
    ];

    // Adds the terms Food, Recipes and Restaurants to Categories, and a second blog post, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems/Taxonomy", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var edit = await page.Locator("li.list-group-item-action:has-text(\"Categories\") a.btn.edit").First.GetAttributeAsync("href");
        var taxonomyId = edit.Split('/')[4];
        var editUrl = $"/Admin/Contents/ContentItems/{taxonomyId}/Edit";

        async Task AddTermAsync(string title, string parentId)
        {
            var url = $"/Admin/Taxonomies/Create/Category?taxonomyContentItemId={taxonomyId}" + (parentId is null ? "" : $"&taxonomyItemId={parentId}");
            await page.GotoAsync(BaseUrl + url, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator("#Category_Icon_Text").EvaluateAsync("e => { e.value = 'fas fa-globe'; e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }");
            await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").ClickAsync());
        }

        await AddTermAsync("Food", null);
        await page.GotoAsync(BaseUrl + editUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        var add = await page.Locator("li.menu-item:has-text(\"Food\") a:text-is(\"Add\")").First.GetAttributeAsync("href");
        var foodId = System.Web.HttpUtility.ParseQueryString(new Uri(BaseUrl + add).Query)["taxonomyItemId"];
        await AddTermAsync("Recipes", foodId);
        await AddTermAsync("Restaurants", foodId);

        await page.GotoAsync(BaseUrl + editUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("a:text-is(\"List BlogPost Items\")").ClickAsync());
        var create = await page.Locator("a:has-text(\"Create Blog Post\")").First.GetAttributeAsync("href");
        await page.GotoAsync(BaseUrl + create, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#TitlePart_Title").FillAsync("A weekend in Lisbon");
        await page.Locator("#BlogPost_Subtitle_Text").FillAsync("Trams, tiles and custard tarts");
        await page.Locator("#MarkdownBodyPart_Markdown").EvaluateAsync("e => { e.value = 'Trams, tiles and a very good custard tart.'; }");
        await page.Locator(".field-wrapper-blog-post-category input[type=radio]").First.CheckAsync();
        await page.Locator(".field-wrapper-blog-post-tags .multiselect__tags").ClickAsync();
        await page.Locator(".field-wrapper-blog-post-tags .multiselect__option:has-text(\"Earth\")").First.ClickAsync();
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").ClickAsync());

        await page.GotoAsync(BaseUrl + "/", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Home posts: " + await page.Locator(".post-preview").CountAsync() + ", badges first: " + await page.Locator(".post-preview").First.Locator(".term-badge").CountAsync());
        await context.CloseAsync();
    }

    private static async Task FeatureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("feature-1", async () =>
        {
            await scene.PauseAsync(2200);
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))"), 300);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))"));
            await scene.TypeAsync(page.Locator("#search-box"), "taxonomies", 40);
        });

        await scene.StepAsync("feature-2", async () =>
            await scene.HighlightAsync(page.Locator("[data-filter-value*=\"The taxonomies module provides\"]")));

        await scene.StepAsync("feature-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("[data-filter-value*=\"Provides taxonomy filters\"]"));
        });
    }

    private static ILocator TaxRow(IPage page, string name) => page.Locator("li.list-group-item-action").Filter(new() { HasText = name });

    private static async Task TaxonomyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/Taxonomy");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("taxonomy-1", async () =>
        {
            await scene.PauseAsync(9500);
            await scene.HighlightAsync(TaxRow(page, "Categories"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(TaxRow(page, "Tags"));
        });

        await scene.StepAsync("taxonomy-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, TaxRow(page, "Categories").Locator("a.btn.edit"));
            await scene.HighlightAsync(page.Locator("select[name$=\"TermContentType\"]"));
        });

        await scene.StepAsync("taxonomy-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(page.Locator("ol#menu"));
        });

        await scene.StepAsync("taxonomy-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, -300);
            await scene.HighlightAsync(page.Locator("a.btn.btn-primary.btn-sm:text-is(\"Add Category\")"), "Top level");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            var add = page.Locator("li.menu-item:has-text(\"Food\") a:text-is(\"Add\")").First;
            await scene.MoveToAsync(add);
            await scene.HighlightAsync(add, "Child term");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("button.publish").First);
        });
    }

    private static async Task CategorizeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/BlogPost");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var tags = page.Locator(".field-wrapper-blog-post-tags");
        var category = page.Locator(".field-wrapper-blog-post-category");

        await scene.StepAsync("categorize-1", async () =>
        {
            await scene.PauseAsync(1800);
            await FastGoAsync(scene, page.Locator("li.list-group-item-action").Filter(new() { HasText = "Man must explore" }).Locator("a.btn.edit"));
            await category.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(tags);
            await scene.HighlightAsync(category);
        });

        await scene.StepAsync("categorize-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(category);
        });

        await scene.StepAsync("categorize-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1500);
            await FastClickAsync(scene, tags.Locator(".multiselect__tags"));
            await scene.PauseAsync(4500);
            await tags.Locator(".multiselect__input").PressSequentiallyAsync("Moon", new() { Delay = 90 });
            await tags.Locator(".multiselect__option--highlight[data-select=\"Press enter to create a tag\"]").WaitForAsync();
        });

        await scene.StepAsync("categorize-4", async () =>
        {
            await page.Keyboard.PressAsync("Escape");
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("button.publish").First);
        });
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("site-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".post-preview").First.Locator(".term-badge").First);
            await scene.HighlightAsync(page.Locator(".post-preview").First.Locator(".term-badge").Nth(1));
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator(".post-preview").First.Locator("a:has-text(\"Travel\")").First);
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.MoveToAsync(400, 600, 60);
            await scene.MoveToAsync(700, 700, 60);
        });
    }
}

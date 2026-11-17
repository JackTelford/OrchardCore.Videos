using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-settings", SettingsAsync),
        ("04-filter", FilterAsync),
        ("05-types", TypesAsync),
    ];

    private static async Task<string> CategoriesIdAsync(IPage page)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems/Taxonomy", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var edit = await page.Locator("li.list-group-item-action:has-text(\"Categories\") a.btn.edit").First.GetAttributeAsync("href");
        return edit.Split('/')[4];
    }

    private static async Task AddTermAsync(IPage page, string url, string title)
    {
        await page.GotoAsync(BaseUrl + url, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#TitlePart_Title").FillAsync(title);
        await page.Locator("#Category_Icon_Text").EvaluateAsync("e => { e.value = 'fas fa-landmark'; e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
    }

    // Seed: Blog Post listable and creatable, five more terms and posts, Categories published.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/Edit/BlogPost", new() { WaitUntil = WaitUntilState.NetworkIdle });
        foreach (var name in new[] { "Creatable", "Listable" })
        {
            var box = page.Locator($".form-check:has(label:text-is(\"{name}\")) input[type=checkbox]").First;
            if (!await box.IsCheckedAsync())
            {
                await page.Locator($"label:text-is(\"{name}\")").First.ClickAsync();
            }
        }

        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button[name=\"submit.Save\"][value=\"Save\"]").First.ClickAsync());

        var id = await CategoriesIdAsync(page);
        var editUrl = $"/Admin/Contents/ContentItems/{id}/Edit";
        var createUrl = $"/Admin/Taxonomies/Create/Category?taxonomyContentItemId={id}";
        await AddTermAsync(page, createUrl, "Food");
        await AddTermAsync(page, createUrl, "Technology");
        await page.GotoAsync(BaseUrl + editUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        var addHref = await page.Locator("li.menu-item").Filter(new() { HasText = "Technology" }).Locator("a:text-is(\"Add\")").First.GetAttributeAsync("href");
        Console.WriteLine("Add child href: " + addHref);
        await AddTermAsync(page, addHref, "Gadgets");
        await AddTermAsync(page, addHref, "Software");
        await page.GotoAsync(BaseUrl + editUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("a:text-is(\"List BlogPost Items\")").ClickAsync());
        var create = await page.Locator("a:has-text(\"Create Blog Post\")").First.GetAttributeAsync("href");

        foreach (var (title, category, tags) in new[]
        {
            ("Packing for a month away", "Travel", new[] { "Earth" }),
            ("Cooking at a high camp", "Food", new[] { "Exploration" }),
            ("A year of orbit photos", "Gadgets", new[] { "Space" }),
            ("Mapping the ocean floor", "Software", new[] { "Earth", "Exploration" }),
            ("Planning software for expeditions", "Software", new[] { "Exploration" }),
        })
        {
            await page.GotoAsync(BaseUrl + create, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator($".field-wrapper-blog-post-category label.form-check-label:has-text('{category}')").First.ClickAsync();
            foreach (var tag in tags)
            {
                await page.Locator(".field-wrapper-blog-post-tags .multiselect__tags").ClickAsync();
                await page.Locator(".field-wrapper-blog-post-tags input.multiselect__input").PressSequentiallyAsync(tag, new() { Delay = 30 });
                await page.Locator($".field-wrapper-blog-post-tags .multiselect__option:has-text('{tag}')").First.ClickAsync();
                await page.Keyboard.PressAsync("Escape");
            }

            await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
        }

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Items: " + (await page.Locator("body").InnerTextAsync()).Contains("11 items"));
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

    private static ILocator FilterButton(IPage page, string title) => page.Locator($"div.bootstrap-select > button.dropdown-toggle[title='{title}']").First;

    private static async Task ChooseAsync(Scene scene, string button, string option)
    {
        var page = scene.Page;
        await FastClickAsync(scene, FilterButton(page, button), 500);
        await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator("div.dropdown-menu.show a.dropdown-item").Filter(new() { HasText = option }).First, 0));
        await page.Locator("li.list-group-item").First.WaitForAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))"), 300);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))"));
            await page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#search-box"), "taxonomies", 50);
            await page.WaitForFunctionAsync("() => document.querySelectorAll('.feature-group [data-filter-value]:not(.d-none)').length === 2");
            await scene.HighlightAsync(page.Locator("[data-filter-value*='OrchardCore.Taxonomies.ContentsAdminList']").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("[data-filter-value*='OrchardCore.Taxonomies.ContentsAdminList'] a.btn-primary").First;
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(enable));
            await page.GetByText("The feature Taxonomies Contents List Filters was enabled.").First.WaitForAsync();
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Settings\"))"), 400);
            await FastGoAsync(scene, page.Locator("#left-nav").GetByText("Taxonomy Filters", new() { Exact = true }).Locator("visible=true").First);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await FastClickAsync(scene, page.Locator("label:text-is(\"Categories\")").First, 300);
            await FastClickAsync(scene, page.Locator("label:text-is(\"Tags\")").First, 300);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await FastGoAsync(scene, page.Locator("button.save").First);
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        });
    }

    private static async Task FilterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("filter-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Content\"))"), 300);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Content Items\"))").First);
            await scene.HighlightAsync(FilterButton(page, "Categories"));
            await scene.HighlightAsync(FilterButton(page, "Tags"));
        });

        await scene.StepAsync("filter-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, FilterButton(page, "Categories"), 300);
        });

        await scene.StepAsync("filter-3", async () =>
        {
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator("div.dropdown-menu.show a.dropdown-item").Filter(new() { HasText = "Software" }).First, 0));
            await page.Locator("li.list-group-item").First.WaitForAsync();
        });

        await scene.StepAsync("filter-4", async () => await ChooseAsync(scene, "Categories", "Show all"));

        await scene.StepAsync("filter-5", async () => await ChooseAsync(scene, "Categories", "Clear filter"));

        await scene.StepAsync("filter-6", async () => await ChooseAsync(scene, "Tags", "Exploration"));
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        var page = scene.Page;
        await page.Locator("li.list-group-item").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("types-1", async () => await ChooseAsync(scene, "Content Type", "Blog Post"));

        await scene.StepAsync("types-2", async () => await ChooseAsync(scene, "Content Type", "Article"));
    }
}

using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-category", CategoryAsync),
        ("03-tags", TagsAsync),
        ("04-edit", EditAsync),
        ("05-site", SiteAsync),
    ];

    // Removes the recipe's Tags and Category fields from Blog Post, adds the terms Trains and Food, and creates the post.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);

        if (Environment.GetEnvironmentVariable("FIX_BODY") == "1")
        {
            await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems/BlogPost", new() { WaitUntil = WaitUntilState.NetworkIdle });
            var editHref = await page.Locator(".list-group-item").Filter(new() { HasText = "A Night Train Across the Alps" }).Locator("a.edit").First.GetAttributeAsync("href");
            await page.GotoAsync(BaseUrl + editHref, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator(".CodeMirror").EvaluateAsync("e => e.CodeMirror.setValue('The sleeper leaves Zurich at dusk and reaches Vienna before breakfast.')");
            await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
            await context.CloseAsync();
            return;
        }

        foreach (var field in new[] { "Tags", "Category" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/Edit/BlogPost", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator($"a[href*='RemoveFieldFrom/BlogPost?name={field}']").ClickAsync();
            await page.Locator("#modalOkButton").WaitForAsync();
            await page.RunAndWaitForNavigationAsync(async () => await page.Locator("#modalOkButton").ClickAsync());
        }

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems/Taxonomy", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var edit = await page.Locator("li.list-group-item-action:has-text(\"Categories\") a.btn.edit").First.GetAttributeAsync("href");
        var taxonomyId = edit.Split('/')[4];
        var editUrl = $"/Admin/Contents/ContentItems/{taxonomyId}/Edit";

        async Task AddTermAsync(string title, string icon, string parentTitle)
        {
            await page.GotoAsync(BaseUrl + editUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
            var url = $"/Admin/Taxonomies/Create/Category?taxonomyContentItemId={taxonomyId}";
            if (parentTitle is not null)
            {
                var add = await page.Locator($"li.menu-item:has-text(\"{parentTitle}\") a:text-is(\"Add\")").First.GetAttributeAsync("href");
                url = add;
            }

            await page.GotoAsync(BaseUrl + url, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator("#Category_Icon_Text").EvaluateAsync("(e, v) => { e.value = v; e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }", icon);
            await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").ClickAsync());
        }

        await AddTermAsync("Trains", "fas fa-train", "Travel");
        await AddTermAsync("Food", "fas fa-utensils", null);
        await page.GotoAsync(BaseUrl + editUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("a:text-is(\"List BlogPost Items\")").ClickAsync());
        var create = await page.Locator("a:has-text(\"Create Blog Post\")").First.GetAttributeAsync("href");
        await page.GotoAsync(BaseUrl + create, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#TitlePart_Title").FillAsync("A Night Train Across the Alps");
        await page.Locator("#BlogPost_Subtitle_Text").FillAsync("Seven hours, one window seat");
        await page.Locator(".CodeMirror").EvaluateAsync("e => e.CodeMirror.setValue('The sleeper leaves Zurich at dusk and reaches Vienna before breakfast.')");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").ClickAsync());

        await page.GotoAsync(BaseUrl + "/blog/a-night-train-across-the-alps", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Post page: " + await page.Locator(".post-heading h1").InnerTextAsync());
        await context.CloseAsync();
    }

    private static async Task OpenBlogListAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        var href = await scene.Page.Locator("a:text-is(\"List BlogPost Items\")").First.GetAttributeAsync("href");
        await scene.OpenAsync(href);
    }

    private static ILocator FieldLabel(IPage page, string text) => page.Locator($"label.form-check-label:has-text('{text}')");

    private static async Task AddFieldAsync(Scene scene, string name)
    {
        var page = scene.Page;
        await FastGoAsync(scene, page.Locator("a:has-text('Add Field')"));
        await FastTypeAsync(scene, page.Locator("input[data-displayname]"), name, 12);
        await page.Keyboard.PressAsync("Tab");
        await page.Locator("input[type='radio'][value='TaxonomyField']").ScrollIntoViewIfNeededAsync();
        await FastClickAsync(scene, page.Locator("input[type='radio'][value='TaxonomyField']"));
        await FastGoAsync(scene, page.Locator("button[value='SaveAndConfigure']"));
    }

    private static async Task CategoryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        var page = scene.Page;
        await page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Design\"))").ClickAsync();
        await page.Locator("#left-nav button.item-label:has(span.title:text-is(\"Content Definition\"))").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("category-1", async () =>
            await FastGoAsync(scene, page.Locator("a[role='btn-edit-BlogPost']")));

        await scene.StepAsync("category-2", async () =>
        {
            await FastGoAsync(scene, page.Locator("a:has-text('Add Field')"));
            await FastTypeAsync(scene, page.Locator("input[data-displayname]"), "Category", 20);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("category-3", async () =>
        {
            var radio = page.Locator("input[type='radio'][value='TaxonomyField']");
            await radio.ScrollIntoViewIfNeededAsync();
            await FastClickAsync(scene, radio);
            await FastGoAsync(scene, page.Locator("button[value='SaveAndConfigure']"));
        });

        await scene.StepAsync("category-4", async () =>
        {
            await scene.SelectAsync(page.Locator("select[name$='TaxonomyContentItemId']"), "Categories");
            await FastTypeAsync(scene, page.Locator("textarea[name$='Hint']"), "Choose the one category that fits the post.", 15);
        });

        await scene.StepAsync("category-5", async () =>
        {
            await FastClickAsync(scene, page.Locator("input[type='checkbox'][name$='Unique']"));
            await FastClickAsync(scene, page.Locator("input[type='checkbox'][name$='LeavesOnly']"));
            foreach (var label in new[] { "Required", "Unique", "Leaves only" })
            {
                await scene.HighlightAsync(FieldLabel(page, label));
                await scene.PauseAsync(1200);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("category-6", async () =>
        {
            var editor = page.Locator("select[data-field-editor-select]");
            await editor.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200);
            await scene.HighlightAsync(editor);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("select[data-field-display-select]"));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button.save[value='Save']"));
        });

        await scene.StepAsync("category-7", async () =>
            await scene.HighlightAsync(page.Locator("li, tr, .list-group-item").Filter(new() { HasText = "Category" }).Filter(new() { HasText = "Taxonomy Field" }).Last));
    }

    private static async Task TagsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/BlogPost");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("tags-1", async () => await AddFieldAsync(scene, "Tags"));

        await scene.StepAsync("tags-2", async () =>
        {
            await scene.SelectAsync(page.Locator("select[name$='TaxonomyContentItemId']"), "Tags");
            await scene.PauseAsync(1500);
            foreach (var label in new[] { "Required", "Unique", "Leaves only" })
            {
                await scene.HighlightAsync(FieldLabel(page, label));
                await scene.PauseAsync(1500);
                await scene.ClearHighlightsAsync();
            }

            await scene.HighlightAsync(page.Locator("input[name$='Placeholder']"));
        });

        await scene.StepAsync("tags-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var editor = page.Locator("select[data-field-editor-select]");
            await editor.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 250);
            await scene.SelectAsync(editor, "Tags");
        });

        await scene.StepAsync("tags-4", async () =>
        {
            var open = page.Locator("input[type='checkbox'][name$='Open']");
            if (!await open.IsCheckedAsync())
            {
                await FastClickAsync(scene, open);
            }

            await scene.HighlightAsync(open.Locator("xpath=../.."));
        });

        await scene.StepAsync("tags-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var display = page.Locator("select[data-field-display-select]");
            await display.ScrollIntoViewIfNeededAsync();
            await scene.SelectAsync(display, "Tags");
            await scene.PauseAsync(4000);
            await FastGoAsync(scene, page.Locator("button.save[value='Save']"));
        });

        await scene.StepAsync("tags-6", async () =>
        {
            var tagsRow = page.Locator("li, tr, .list-group-item").Filter(new() { HasText = "Tags" }).Filter(new() { HasText = "Taxonomy Field" }).Last;
            await scene.HighlightAsync(tagsRow);
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("li, tr, .list-group-item").Filter(new() { HasText = "Category" }).Filter(new() { HasText = "Taxonomy Field" }).Last);
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenBlogListAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var post = page.Locator(".list-group-item").Filter(new() { HasText = "A Night Train Across the Alps" });
        var tags = page.Locator(".tags");

        await scene.StepAsync("edit-1", async () =>
        {
            await scene.HighlightAsync(page.GetByRole(AriaRole.Link, new() { Name = "Blog", Exact = true }).First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var editButton = post.Locator("a.edit");
            var (ex, ey) = await scene.CenterOfAsync(editButton);
            await scene.MoveToAsync(ex, ey, 10);
            await scene.OffCameraAsync(async () => await scene.WaitForNavigationAsync(async () => await page.Mouse.ClickAsync((float)ex, (float)ey)));
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await page.Locator("input[type='radio'][name$='UniqueValue']").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 150);
        });

        await scene.StepAsync("edit-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("label.form-check-label:has-text('Travel')").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("label.form-check-label:has-text('Trains')").First);
        });

        await scene.StepAsync("edit-4", async () =>
        {
            await tags.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(2000);
            await FastClickAsync(scene, page.Locator(".tags .multiselect__placeholder"));
            await tags.Locator("input.multiselect__input").PressSequentiallyAsync("Expl", new() { Delay = 60 });
            await FastClickAsync(scene, page.Locator(".tags .multiselect__option:has-text('Exploration')").First);
        });

        await scene.StepAsync("edit-5", async () =>
            await tags.Locator("input.multiselect__input").PressSequentiallyAsync("Night Train", new() { Delay = 60 }));

        await scene.StepAsync("edit-6", async () =>
        {
            await page.Keyboard.PressAsync("Enter");
            await page.Locator(".tags .multiselect__tag:has-text('Night Train')").WaitForAsync();
        });

        await scene.StepAsync("edit-7", async () =>
        {
            await page.EvaluateAsync("() => window.scrollTo(0, document.body.scrollHeight)");
            await FastGoAsync(scene, page.Locator("button.publish").First);
        });
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenBlogListAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("site-1", async () =>
        {
            var view = page.Locator(".list-group-item").Filter(new() { HasText = "A Night Train Across the Alps" }).Locator("a.view");
            var (x, y) = await scene.CenterOfAsync(view);
            await scene.MoveToAsync(x, y, 12);
            var tab = await page.Context.RunAndWaitForPageAsync(async () => await page.Mouse.ClickAsync((float)x, (float)y));
            await scene.OffCameraAsync(async () => await tab.WaitForLoadStateAsync(LoadState.NetworkIdle));
            await scene.SwitchToAsync(tab);
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.ScrollAsync(0, 600);
        });
    }
}

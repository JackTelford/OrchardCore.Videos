using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-create", CreateAsync),
        ("03-add", AddAsync),
        ("04-edit", EditAsync),
        ("05-organize", OrganizeAsync),
        ("06-publish", PublishAsync),
    ];

    private const string RecipeText = "{\"steps\":[{\"name\":\"ContentDefinition\",\"ContentTypes\":[{\"Name\":\"Topic\",\"DisplayName\":\"Topic\",\"Settings\":{\"ContentTypeSettings\":{}},\"ContentTypePartDefinitionRecords\":[{\"PartName\":\"TitlePart\",\"Name\":\"TitlePart\",\"Settings\":{\"ContentTypePartSettings\":{\"Position\":\"0\"}}}]}],\"ContentParts\":[]}]}";

    // Creates the Topic content type (a title only), outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await ImportRecipeAsync(page, RecipeText);
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/List", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Topic type present: " + (await page.Locator("a[role='btn-edit-Topic']").CountAsync() > 0));
        await context.CloseAsync();
    }

    // Opens the Topics editor without a returnUrl (not recorded).
    private static async Task OpenTopicsEditorAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        var href = await scene.Page.GetByRole(AriaRole.Link, new() { Name = "Topics", Exact = true }).First.GetAttributeAsync("href");
        await scene.OpenAsync(href.Split('?')[0]);
    }

    private static ILocator TermRow(IPage page, string term)
        => page.Locator("ol#menu > li.menu-item")
            .Filter(new() { Has = page.Locator(".menu-item-title > span", new() { HasTextRegex = new Regex($"^{term}$") }) });

    private static async Task AddTermAsync(Scene scene, ILocator addButton, string title)
    {
        var page = scene.Page;
        await FastGoAsync(scene, addButton);
        await FastTypeAsync(scene, page.Locator("input[name='TitlePart.Title']"), title, 6);
        await FastGoAsync(scene, page.Locator("button[name='submit.Publish']").First);
    }

    private static ILocator AddTopic(IPage page) => page.GetByRole(AriaRole.Link, new() { Name = "Add Topic", Exact = true });

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#new-dropdown"));
            await scene.PauseAsync(1200);
            await FastGoAsync(scene, page.Locator("ul.dropdown-menu a.dropdown-item[href*='/ContentTypes/Taxonomy/Create']"));
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(page.Locator("input[name='TitlePart.Title']"), "Topics", 40);
            await scene.HighlightAsync(page.Locator("input[name='AliasPart.Alias']"), "Generated from the title");
            await scene.HighlightAsync(page.Locator("input[name='AutoroutePart.Path']"));
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var term = page.Locator("select[name='TaxonomyPart.TermContentType']");
            await term.ScrollIntoViewIfNeededAsync();
            await scene.SelectAsync(term, "Topic");
            await scene.HighlightAsync(term);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button[name='submit.Publish']").First);
            await page.Locator("li.list-group-item-action:has-text(\"Topics\")").First.WaitForAsync();
        });
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("add-1", async () =>
        {
            await FastGoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Topics", Exact = true }).First);
            await AddTopic(page).ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(AddTopic(page));
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await AddTermAsync(scene, AddTopic(page), "Cooking");
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.HighlightAsync(TermRow(page, "Cooking").Locator(".menu-item-title .float-end"), "Edit, Add and Delete");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await AddTermAsync(scene, AddTopic(page), "Gardening");
        });

        await scene.StepAsync("add-4", async () =>
            await AddTermAsync(scene, TermRow(page, "Cooking").Locator(".menu-item-title a.btn-primary"), "Baking"));

        await scene.StepAsync("add-5", async () =>
        {
            await AddTermAsync(scene, TermRow(page, "Cooking").Locator(".menu-item-title a.btn-primary"), "Grilling");
            await AddTermAsync(scene, TermRow(page, "Cooking").Locator(".menu-item-title a.btn-primary"), "Roasting");
        });

        await scene.StepAsync("add-6", async () =>
        {
            await AddTermAsync(scene, TermRow(page, "Gardening").Locator(".menu-item-title a.btn-primary"), "Vegetables");
            await AddTermAsync(scene, AddTopic(page), "Herbs");
            await page.EvaluateAsync("() => window.scrollTo(0, document.body.scrollHeight)");
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenTopicsEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("edit-1", async () =>
        {
            await FastGoAsync(scene, TermRow(page, "Grilling").Locator(".menu-item-title a.btn-success"));
            await page.Locator("input[name='TitlePart.Title']").FillAsync("");
            await FastTypeAsync(scene, page.Locator("input[name='TitlePart.Title']"), "Barbecue", 20);
            await FastGoAsync(scene, page.Locator("button[name='submit.Publish']").First);
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.PauseAsync(3500);
            await FastClickAsync(scene, TermRow(page, "Roasting").Locator(".menu-item-title a.btn-danger"), 100);
        });

        await scene.StepAsync("edit-3", async () =>
        {
            await FastGoAsync(scene, page.Locator("#modalOkButton"));
            var toast = page.Locator(".oc-notify-toast-container .toast.message-success");
            await scene.HighlightAsync(toast);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task OrganizeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenTopicsEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var depths = "els => els.map(e => e.dataset.depth).join(',')";

        await scene.StepAsync("org-1", async () =>
        {
            await scene.PauseAsync(5000);
            await scene.DragToAsync(TermRow(page, "Gardening").Locator(".menu-item-title"), TermRow(page, "Cooking").Locator(".menu-item-title"), 0, -14);
            Console.WriteLine("depths after org-1: " + await page.Locator("ol#menu > li.menu-item").EvaluateAllAsync<string>(depths));
        });

        await scene.StepAsync("org-2", async () =>
        {
            await scene.PauseAsync(6000);
            await scene.DragToAsync(TermRow(page, "Herbs").Locator(".menu-item-title"), TermRow(page, "Cooking").Locator(".menu-item-title"), 70, -14);
            Console.WriteLine("depths after org-2: " + await page.Locator("ol#menu > li.menu-item").EvaluateAllAsync<string>(depths));
        });

        await scene.StepAsync("org-3", async () =>
        {
            var save = page.Locator("button[name='submit.Save']").First;
            await save.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(7000);
            await scene.ClickAndWaitForNavigationAsync(save);
        });
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenTopicsEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("pub-1", async () =>
        {
            await FastGoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Manage Content" }).First);
            await page.Locator("li.list-group-item-action:has-text(\"Topics\")").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("span.badge", new() { HasText = "Draft" }).First);
        });

        await scene.StepAsync("pub-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Topics", Exact = true }).First);
            await page.Locator("ol#menu").ScrollIntoViewIfNeededAsync();
        });

        await scene.StepAsync("pub-3", async () =>
        {
            await FastGoAsync(scene, page.Locator("button[name='submit.Publish']").First);
            await page.Locator("li.list-group-item-action:has-text(\"Topics\")").First.WaitForAsync();
        });
    }
}

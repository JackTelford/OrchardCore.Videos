using System.Text;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-index", IndexAsync),
        ("04-query", QueryAsync),
    ];

    private const string QueryTemplate = "{\n  \"size\": 10,\n  \"query\": {\n    \"bool\": {\n      \"must\": [\n        { \"term\": { \"Content.ContentItem.ContentType\": \"BlogPost\" } }\n        {% if categories %},\n        { \"match\": { \"BlogPost.Category.Ids\": { \"query\": \"{{ categories }}\", \"operator\": \"or\" } } }\n        {% endif %}\n        {% if tags %},\n        { \"match\": { \"BlogPost.Tags.Ids\": { \"query\": \"{{ tags }}\", \"operator\": \"or\" } } }\n        {% endif %}\n      ]\n    }\n  }\n}\n";

    private static string QueryPath => "/Admin/Lucene/query/Search?query=" + Uri.EscapeDataString(Convert.ToBase64String(Encoding.UTF8.GetBytes(QueryTemplate)));

    // Runs the Blog - Lucene Search recipe, adds the term Politics and three posts, and waits for the index (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await page.GotoAsync(BaseUrl + "/Admin/Recipes", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var row = page.Locator(".list-group-item, tr").Filter(new() { HasText = "Blog - Lucene Search" }).Last;
        await row.Locator("a:has-text(\"Run\"), button:has-text(\"Run\")").First.ClickAsync();
        await page.Locator("#modalOkButton").WaitForAsync();
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("#modalOkButton").ClickAsync());
        Console.WriteLine("Recipe: " + (await page.Locator("body").InnerTextAsync()).Contains("run successfully"));
        await Task.Delay(8000);

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems/Taxonomy", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var edit = await page.Locator("li.list-group-item-action:has-text(\"Categories\") a.btn.edit").First.GetAttributeAsync("href");
        var taxonomyId = edit.Split('/')[4];
        var editUrl = $"/Admin/Contents/ContentItems/{taxonomyId}/Edit";
        await page.GotoAsync(BaseUrl + $"/Admin/Taxonomies/Create/Category?taxonomyContentItemId={taxonomyId}", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#TitlePart_Title").FillAsync("Politics");
        await page.Locator("#Category_Icon_Text").EvaluateAsync("e => { e.value = 'fas fa-landmark'; e.dispatchEvent(new Event('input', { bubbles: true })); e.dispatchEvent(new Event('change', { bubbles: true })); }");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").ClickAsync());
        await page.GotoAsync(BaseUrl + editUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("a:text-is(\"List BlogPost Items\")").ClickAsync());
        var create = await page.Locator("a:has-text(\"Create Blog Post\")").First.GetAttributeAsync("href");

        foreach (var (title, category, tag) in new[] { ("Notes from a night train", "Travel", "Earth"), ("What the council voted on this week", "Politics", null), ("A town meeting, explained", "Politics", null) })
        {
            await page.GotoAsync(BaseUrl + create, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator(".CodeMirror").First.EvaluateAsync("(e, t) => e.CodeMirror.setValue(t)", title + ".");
            await page.Locator($".field-wrapper-blog-post-category label.form-check-label:has-text('{category}')").First.ClickAsync();
            if (tag is not null)
            {
                await page.Locator(".field-wrapper-blog-post-tags .multiselect__tags").ClickAsync();
                await page.Locator(".field-wrapper-blog-post-tags input.multiselect__input").PressSequentiallyAsync(tag, new() { Delay = 30 });
                await page.Locator($".field-wrapper-blog-post-tags .multiselect__option:has-text('{tag}')").First.ClickAsync();
                await page.Keyboard.PressAsync("Escape");
            }

            await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").First.ClickAsync());
        }

        for (var i = 0; i < 40; i++)
        {
            await page.GotoAsync(BaseUrl + QueryPath, new() { WaitUntil = WaitUntilState.NetworkIdle });
            var text = await page.Locator("body").InnerTextAsync();
            if (text.Contains("Found 4 result(s)"))
            {
                Console.WriteLine("Found 4 result(s)");
                break;
            }

            await Task.Delay(3000);
        }

        await context.CloseAsync();
    }

    private static async Task<string> TermIdAsync(IPage page, string term)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems/Taxonomy", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var edit = await page.Locator("li.list-group-item-action:has-text(\"Categories\") a.btn.edit").First.GetAttributeAsync("href");
        await page.GotoAsync(BaseUrl + $"/Admin/Contents/ContentItems/{edit.Split('/')[4]}/Edit", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var href = await page.Locator("li.menu-item").Filter(new() { HasText = term }).Locator("a:text-is(\"Edit\")").First.GetAttributeAsync("href");
        return href.Split('?')[0].Split('/').Last();
    }

    private static async Task EditFieldIndexAsync(Scene scene, string field)
    {
        var page = scene.Page;
        await FastGoAsync(scene, page.Locator($"li.list-group-item:has-text(\"{field}\") a:text-is(\"Edit\")").First);
        await page.Locator("label:text-is(\"Include this element in the index\")").First.ScrollIntoViewIfNeededAsync();
        await scene.ScrollAsync(0, 150, 8);
        await FastClickAsync(scene, page.Locator("label:text-is(\"Include this element in the index\")").First);
        await FastGoAsync(scene, page.Locator("button.save").First);
    }

    private static async Task IndexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/BlogPost");
        var page = scene.Page;
        page.SetDefaultTimeout(90_000);
        await page.WaitForTimeoutAsync(600);
        await page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Design\"))").ClickAsync();
        await page.Locator("#left-nav button.item-label:has(span.title:text-is(\"Content Definition\"))").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("index-1", async () => await EditFieldIndexAsync(scene, "Category"));

        await scene.StepAsync("index-2", async () => await EditFieldIndexAsync(scene, "Tags"));

        await scene.StepAsync("index-3", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Search\"))"), 300);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Indexes\"))"));
            await FastClickAsync(scene, page.Locator("button.dropdown-toggle.actions").First);
        });

        await scene.StepAsync("index-4", async () =>
        {
            await FastGoAsync(scene, page.Locator("li.list-group-item:has-text(\"Search\") a.dropdown-item:text-is(\"Rebuild\")").First);
            if (await page.Locator("#modalOkButton").IsVisibleAsync())
            {
                Console.WriteLine("A Rebuild Index dialog appeared.");
                await FastGoAsync(scene, page.Locator("#modalOkButton"));
            }
        });
    }

    private static async Task QueryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        var politics = await TermIdAsync(page, "Politics");
        for (var i = 0; i < 40; i++)
        {
            await page.GotoAsync(BaseUrl + QueryPath + "&parameters=" + Uri.EscapeDataString("{\"categories\":\"" + politics + "\"}"), new() { WaitUntil = WaitUntilState.NetworkIdle });
            if ((await page.Locator("body").InnerTextAsync()).Contains("Found 2 result(s)"))
            {
                break;
            }

            await Task.Delay(3000);
        }

        await page.GotoAsync(BaseUrl + QueryPath, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await scene.StartCaptureAsync();
        var runUrl = "";

        await scene.StepAsync("query-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".CodeMirror").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByText("Found 4 result(s)").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("query-2", async () =>
        {
            await FastGoAsync(scene, page.Locator("a.btn:text-is(\"Save Query\")"));
            await FastTypeAsync(scene, page.Locator("div.ocat-wrapper:has(> label:text-is(\"Name\")) input").First, "GetBlogsByFilter", 15);
            await scene.SelectAsync(page.Locator("div.ocat-wrapper:has(> label:text-is(\"Index\")) select").First, "Search", 200);
            await FastClickAsync(scene, page.Locator("label:text-is(\"Return Content Items\")").First);
        });

        await scene.StepAsync("query-3", async () => await FastGoAsync(scene, page.Locator("button.save:text-is(\"Save\")").First));

        await scene.StepAsync("run-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("li.list-group-item, tr").Filter(new() { HasText = "GetBlogsByFilter" }).Last.Locator("a.btn:text-is(\"Run\")").First);
            runUrl = new Uri(page.Url).PathAndQuery;
            Console.WriteLine("Run URL: " + runUrl[..Math.Min(90, runUrl.Length)]);
            await scene.HighlightAsync(page.Locator(".CodeMirror").Nth(1), "Parameters");
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var parameters = Uri.EscapeDataString("{\"categories\":\"" + politics + "\"}");
            await scene.OffCameraAsync(async () => await scene.NavigateAsync(runUrl + (runUrl.Contains('?') ? "&" : "?") + "parameters=" + parameters));
            await page.GetByText("Found 2 result(s)").First.WaitForAsync();
            await scene.HighlightAsync(page.GetByText("Found 2 result(s)").First);
        });
    }
}

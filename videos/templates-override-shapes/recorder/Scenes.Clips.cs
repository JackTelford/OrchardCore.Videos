using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-create", CreateAsync),
        ("05-shortcuts", ShortcutsAsync),
        ("06-manage", ManageAsync),
        ("04-debug", DebugAsync),
    ];

    private const string TemplateText = "<div class=\"post-preview\">\n  <a href=\"{{ Model.ContentItem | display_url }}\">\n    <h2 class=\"post-title\">{{ Model.ContentItem.DisplayText }}</h2>\n    <h3 class=\"post-subtitle\">\n      {{ Model.ContentItem.Content.BlogPost.Subtitle.Text }}\n    </h3>\n  </a>\n  <p class=\"post-meta\">\n    Written by {{ Model.ContentItem.Author }}\n  </p>\n</div>\n<hr class=\"my-4\" />";

    // Adds two blog posts to the Blog, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("a:text-is(\"List BlogPost Items\")").ClickAsync());
        var create = await page.Locator("a:has-text(\"Create Blog Post\")").First.GetAttributeAsync("href");

        foreach (var (title, subtitle) in new[] { ("Planning a market garden", "What the first season taught us"), ("Composting in a small kitchen", "A year of scraps in one bin") })
        {
            await page.GotoAsync(BaseUrl + create, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator("#BlogPost_Subtitle_Text").FillAsync(subtitle);
            await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.publish").ClickAsync());
        }

        await page.GotoAsync(BaseUrl + "/blog", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Posts on /blog: " + await page.Locator(".post-preview").CountAsync());
        await context.CloseAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/blog");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".post-preview").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Templates");
            await page.GetByText("There are no templates for the moment").WaitForAsync();
            await scene.HighlightAsync(page.Locator("a.create"));
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("a.create"));
        });

        await scene.StepAsync("create-3", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#Name"), "Content_Summary__BlogPost", 25);
            await FastTypeAsync(scene, page.Locator("#Description"), "Summary of a blog post in a list", 25);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await FastGoAsync(scene, page.Locator("button.save"));
            await page.GetByText("The content is mandatory.").WaitForAsync();
            await scene.ScrollAsync(0, 400);
        });

        await scene.StepAsync("create-5", async () =>
        {
            await page.WaitForSelectorAsync("#Content_editor .monaco-editor");
            await scene.ScrollAsync(0, -600);
            await page.EvaluateAsync("async (text) => { const monaco = await window.__orchardCoreMonacoReady; monaco.editor.getModels()[0].setValue(text); }", TemplateText);
            var ok = await page.EvaluateAsync<bool>("async (text) => { const monaco = await window.__orchardCoreMonacoReady; return monaco.editor.getModels()[0].getValue() === text && document.querySelector('#Content').value === text; }", TemplateText);
            if (!ok)
            {
                throw new InvalidOperationException("The editor does not hold the template: the take is not valid.");
            }
        });

        await scene.StepAsync("create-6", async () =>
        {
            await scene.HighlightAsync(page.Locator("button.save"));
            await scene.HighlightAsync(page.Locator("a.action-bar-secondary"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("create-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button.save"));
            await page.GetByText("Content_Summary__BlogPost").First.WaitForAsync();
        });

        await scene.StepAsync("create-8", async () =>
        {
            await scene.NavigateAsync("/blog");
            await page.GetByText("Written by admin").First.WaitForAsync();
        });
    }

    private static async Task DebugAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Templates");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("debug-1", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/debugging");
            await page.GetByText("Write shape debug information").First.WaitForAsync();
            await FastClickAsync(scene, page.GetByLabel("Write shape debug information"));
            await scene.HighlightAsync(page.Locator("label:has-text(\"Write shape debug information\")").First);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button.save"));
        });
    }

    private static async Task ShortcutsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("shortcuts-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("a[role='btn-edit-BlogPost']"));
            await page.GetByText("Edit Content Type - Blog Post").First.WaitForAsync();
            await FastClickAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Edit templates" }));
        });

        await scene.StepAsync("shortcuts-2", async () =>
            await FastGoAsync(scene, page.Locator("a.dropdown-item:has-text(\"Template for a Blog Post content item in detail views\")")));

        await scene.StepAsync("shortcuts-3", async () =>
        {
            await FastGoAsync(scene, page.Locator("a.cancel"));
            await FastClickAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Edit templates" }));
            await FastGoAsync(scene, page.Locator("a.dropdown-item:has-text(\"Template for a Blog Post content item in summary views\")"));
            await page.WaitForSelectorAsync("#Content_editor .monaco-editor");
            await page.GetByText("post-preview").First.WaitForAsync();
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Templates");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("manage-1", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#search-box"), "Summary", 40);
            await scene.WaitForNavigationAsync(async () => await page.Keyboard.PressAsync("Enter"));
            await FastGoAsync(scene, page.Locator("li.list-group-item a.btn:text-is(\"Edit\")").First);
            await page.WaitForSelectorAsync("#Content_editor .monaco-editor");
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("#Name"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("manage-3", async () =>
        {
            await FastGoAsync(scene, page.Locator("a.cancel"));
            await FastClickAsync(scene, page.Locator("a.btn-danger").First);
            await page.Locator("#modalOkButton").WaitForAsync();
            await scene.PauseAsync(1000);
            await FastGoAsync(scene, page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("manage-4", async () =>
        {
            await scene.NavigateAsync("/blog");
            await page.GetByText("Posted by admin on").First.WaitForAsync();
        });
    }
}

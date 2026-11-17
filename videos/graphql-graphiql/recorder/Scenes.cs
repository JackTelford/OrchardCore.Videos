using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string VariablesAddress = "/Admin/GraphQL?query=query%20Posts(%24first%3A%20Int)%20%7B%20blogPost(first%3A%20%24first)%20%7B%20displayText%20%7D%20%7D&variables=%7B%22first%22%3A2%7D";

    private const string ErrorsAddress = "/Admin/GraphQL?query=%7B%20blogPost%20%7B%20title%20%7D%20%7D";

    private const string BlogPostRow = ".graphiql-explorer-root [data-field-name=\"blogPost\"]";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-open", OpenAsync),
        ("03-build", BuildAsync),
        ("04-docs", DocsAsync),
        ("05-variables", VariablesAsync),
        ("06-errors", ErrorsAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static string Post(string id, string title, string path, string markdown, string blogId, int order, string date) => $$"""
        {
          "ContentItemId": "{{id}}",
          "ContentType": "BlogPost",
          "DisplayText": "{{title}}",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "{{date}}",
          "ModifiedUtc": "{{date}}",
          "PublishedUtc": "{{date}}",
          "Author": "admin",
          "TitlePart": { "Title": "{{title}}" },
          "AutoroutePart": { "Path": "{{path}}" },
          "MarkdownBodyPart": { "Markdown": "{{markdown}}" },
          "ContainedPart": { "ListContentItemId": "{{blogId}}", "ListContentType": "Blog", "Order": {{order}} }
        }
        """;

    private static async Task<string> ImportAsync(IPage page, string json)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        return await page.EvaluateAsync<string>("""
            async (json) => {
              const token = document.querySelector('form input[name="__RequestVerificationToken"]').value;
              const body = new URLSearchParams({ Json: json, __RequestVerificationToken: token });
              const response = await fetch('/Admin/DeploymentPlan/Import/Json', { method: 'POST', body, headers: { 'Content-Type': 'application/x-www-form-urlencoded' } });
              const text = await response.text();
              return response.status + ' ' + (text.includes('Recipe imported successfully') ? 'imported' : 'NOT imported');
            }
            """, json);
    }

    // Enables GraphQL and adds two blog posts (fake data) to the one of the Blog recipe, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        Console.WriteLine("feature: " + await ImportAsync(page, """{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.Apis.GraphQL" ], "disable": [] } ] }"""));

        for (var i = 0; i < 40; i++)
        {
            await Task.Delay(3000);
            var code = await page.EvaluateAsync<int>("async () => (await fetch('/api/graphql?query=%7BblogPost%7BdisplayText%7D%7D')).status");

            if (code == 200)
            {
                break;
            }
        }

        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var blogId = await page.EvaluateAsync<string>("""
            () => {
              const a = [...document.querySelectorAll('a[href$="/Display"]')].find(x => x.textContent.trim() === 'Blog');
              return a ? a.getAttribute('href').split('/ContentItems/')[1].split('/')[0] : '';
            }
            """);
        Console.WriteLine($"blog={blogId}");

        if (blogId is "")
        {
            throw new InvalidOperationException("Could not find the Blog.");
        }

        var json = $$"""
            { "steps": [ { "name": "content", "Data": [
            {{Post("4kq7m2x9d5w8e3r1t6y0u2i4oa", "Notes from the launch pad", "blog/notes-from-the-launch-pad", "A few lines from the morning of a launch.", blogId, 1, "2026-11-17T20:00:00Z")}},
            {{Post("7cz3n8b5v1m4k9j2h6g0f5d3sa", "A field guide to quiet places", "blog/a-field-guide-to-quiet-places", "Where to go when the day gets loud.", blogId, 2, "2026-11-17T21:00:00Z")}}
            ] } ] }
            """;
        Console.WriteLine("posts: " + await ImportAsync(page, json));

        var body = await page.EvaluateAsync<string>("async () => await (await fetch('/api/graphql?query=%7BblogPost%7BdisplayText%7D%7D')).text()");
        Console.WriteLine("seed check: " + body);
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static ILocator ToolsGroup(IPage page) => page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))");

    private static ILocator GraphiQLLink(IPage page) => page.Locator("#left-nav a[href$=\"/Admin/GraphQL\"]");

    private static Task WaitForSchemaAsync(IPage page) => page.Locator(BlogPostRow).First.WaitForAsync(new LocatorWaitForOptions { Timeout = 90_000 });

    // Opens GraphiQL at an address, waits for the schema, collapses the admin menu, shows the blogPost row.
    private static async Task OpenGraphiQLAsync(Scene scene, string address, bool collapse = true)
    {
        var page = scene.Page;
        await scene.OpenAsync(address);
        await WaitForSchemaAsync(page);

        if (collapse)
        {
            await page.Locator(".leftbar-compactor").ClickAsync();
            await Task.Delay(400);
        }

        await page.Locator(BlogPostRow).First.ScrollIntoViewIfNeededAsync();
        await page.Mouse.MoveAsync(760, 520);
        await Task.Delay(400);
    }

    private static Task<string> EditorTextAsync(IPage page)
        => page.EvaluateAsync<string>("() => document.querySelector('section.query-editor .CodeMirror').CodeMirror.getValue()");

    private static Task WaitResultAsync(IPage page, string text)
        => page.WaitForFunctionAsync("t => document.querySelector('section.result-window').innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = 90_000 });

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();

        if (await GraphiQLLink(page).IsVisibleAsync())
        {
            await ToolsGroup(page).ClickAsync();
            await GraphiQLLink(page).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        }

        await page.Mouse.MoveAsync(760, 520);
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            await scene.ClickAsync(ToolsGroup(page));
            await GraphiQLLink(page).WaitForAsync();
            await scene.HighlightAsync(GraphiQLLink(page));
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(GraphiQLLink(page));
                await WaitForSchemaAsync(page);
            }, 300);
            await scene.MoveToAsync(page.Locator("#left-nav a[href$=\"/Admin/GraphQL\"]"));
        });

        await scene.StepAsync("open-2", async () =>
        {
            await scene.ClickAsync(page.Locator(".leftbar-compactor"), pauseAfter: 400);
            await scene.PauseAsync(400);

            foreach (var (selector, caption) in new[] { (".graphiql-explorer-root", "Explorer"), ("section.query-editor", "Query"), ("section.result-window", "Result") })
            {
                await scene.HighlightAsync(page.Locator(selector), caption);
                await scene.PauseAsync(1200);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("open-3", async () =>
        {
            await scene.MoveToAsync(page.Locator("section.query-editor .CodeMirror-lines"));
            await scene.HighlightAsync(page.Locator("section.query-editor"));
        });

        await scene.StepAsync("open-4", async () =>
        {
            await scene.ClearHighlightsAsync();

            foreach (var (selector, caption) in new[] { ("button.execute-button", "Run"), ("button[title^=\"Prettify\"]", null), ("button[title$=\"History\"]", null), ("button[title=\"Toggle Explorer\"]", null) })
            {
                await scene.HighlightAsync(page.Locator(selector).First, caption);
                await scene.PauseAsync(1300);

                if (selector != "button[title=\"Toggle Explorer\"]")
                {
                    await scene.ClearHighlightsAsync();
                }
            }
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EnsureFieldAsync(Scene scene, string field)
    {
        var page = scene.Page;
        var text = await EditorTextAsync(page);

        if (!System.Text.RegularExpressions.Regex.IsMatch(text, $@"\b{field}\b"))
        {
            await scene.ClickAsync(page.Locator($".graphiql-explorer-root [data-field-name=\"{field}\"]").First, pauseAfter: 900);
        }
        else
        {
            await scene.MoveToAsync(page.Locator($".graphiql-explorer-root [data-field-name=\"{field}\"]").First);
            await scene.PauseAsync(900);
        }
    }

    private static async Task BuildAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenGraphiQLAsync(scene, "/Admin/GraphQL");
        await scene.StartCaptureAsync();

        await scene.StepAsync("build-1", async () =>
        {
            await scene.ClickAsync(page.Locator(BlogPostRow).First, pauseAfter: 1200);
        });

        await scene.StepAsync("build-2", async () =>
        {
            await EnsureFieldAsync(scene, "displayText");
            await EnsureFieldAsync(scene, "createdUtc");
            await page.Locator(".graphiql-explorer-root [data-field-name=\"path\"]").First.ScrollIntoViewIfNeededAsync();
            await EnsureFieldAsync(scene, "path");
        });

        await scene.StepAsync("build-3", async () =>
        {
            await scene.ClickAsync(page.Locator("button.execute-button"));
            await WaitResultAsync(page, "\"blogPost\"");
        });

        await scene.StepAsync("build-4", async () =>
        {
            await scene.ClickAsync(page.Locator(".graphiql-explorer-root [data-field-name=\"createdUtc\"]").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DocsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenGraphiQLAsync(scene, "/Admin/GraphQL");
        await scene.StartCaptureAsync();

        await scene.StepAsync("docs-1", async () =>
        {
            await scene.ClickAsync(page.Locator("button[title=\"Toggle Explorer\"]"), pauseAfter: 400);
            await scene.PauseAsync(400);
            await scene.ClickAsync(page.Locator("button.docExplorerShow"));
            await page.Locator("section.doc-explorer").WaitForAsync();
            await scene.PauseAsync(2500);
            await scene.ClickAsync(page.Locator("section.doc-explorer a.type-name:text-is(\"Query\")").First);
        });

        await scene.StepAsync("docs-2", async () =>
        {
            await scene.TypeAsync(page.Locator("section.doc-explorer .search-box input"), "blogPost", 30);
            await scene.PauseAsync(800);
            await scene.ClickAsync(page.Locator("section.doc-explorer a.field-name:text-is(\"blogPost\")").First);
        });

        await scene.StepAsync("docs-3", async () =>
        {
            await scene.MoveToAsync(page.Locator("section.doc-explorer .doc-explorer-contents"));
            await scene.ScrollAsync(0, 260);
            await page.Locator("#doc-args").WaitForAsync();
            await scene.HighlightAsync(page.Locator("#doc-args"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task VariablesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenGraphiQLAsync(scene, VariablesAddress);

        if (!await page.Locator("section.variable-editor .codemirrorWrap > .CodeMirror").First.IsVisibleAsync())
        {
            await page.Locator(".variable-editor-title-text:has-text(\"Query Variables\")").ClickAsync();
            await Task.Delay(500);
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("vars-2", async () =>
        {
            await scene.ClickAsync(page.Locator("button[title^=\"Prettify\"]"), pauseAfter: 1500);
            await scene.HighlightAsync(page.Locator("section.variable-editor"), "Query Variables");
        });

        await scene.StepAsync("vars-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("button.execute-button"));
            await WaitResultAsync(page, "\"blogPost\"");
            await scene.PauseAsync(1500);
            await scene.DoubleClickAsync(page.Locator("section.variable-editor .codemirrorWrap > .CodeMirror .cm-number").First);
            await page.Keyboard.TypeAsync("1");
            await scene.PauseAsync(600);
            await scene.ClickAsync(page.Locator("button.execute-button"));
            await page.WaitForFunctionAsync("() => document.querySelector('section.result-window').innerText.split('\"displayText\"').length === 2", null, new PageWaitForFunctionOptions { Timeout = 90_000 });
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ErrorsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenGraphiQLAsync(scene, ErrorsAddress);

        var mark = page.Locator("section.query-editor .CodeMirror-lint-mark-error").First;

        try
        {
            await mark.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });
        }
        catch (TimeoutException)
        {
            await page.Locator("section.query-editor .CodeMirror-lines").ClickAsync();
            await page.Keyboard.PressAsync("End");
            await page.Keyboard.TypeAsync(" ");
            await page.Keyboard.PressAsync("Backspace");
            await mark.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("errors-1", async () =>
        {
            await scene.MoveToAsync(mark);
            await scene.HoldAsync(0.5);
        });

        await scene.StepAsync("errors-2", async () =>
        {
            await scene.ClickAsync(page.Locator("button.execute-button"));
            await WaitResultAsync(page, "\"errors\"");
        });

        await scene.ClearHighlightsAsync();
    }
}

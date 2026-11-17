using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string AddressA = "/Admin/GraphQL?query=%7B%0A%20%20blogPost%20%7B%0A%20%20%20%20displayText%0A%20%20%7D%0A%7D";

    private const string AddressB = "/Admin/GraphQL?query=%7B%0A%20%20blog%20%7B%0A%20%20%20%20displayText%0A%20%20%20%20list%20%7B%0A%20%20%20%20%20%20contentItems%20%7B%0A%20%20%20%20%20%20%20%20...%20on%20BlogPost%20%7B%0A%20%20%20%20%20%20%20%20%20%20displayText%0A%20%20%20%20%20%20%20%20%20%20subtitle%0A%20%20%20%20%20%20%20%20%7D%0A%20%20%20%20%20%20%7D%0A%20%20%20%20%7D%0A%20%20%7D%0A%7D";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-query", QueryAsync),
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

    private static string Post(string id, string title, string path, string markdown, string blogId, int order, string subtitle, string date) => $$"""
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
          "ContainedPart": { "ListContentItemId": "{{blogId}}", "ListContentType": "Blog", "Order": {{order}} },
          "BlogPost": { "Subtitle": { "Text": "{{subtitle}}" } }
        }
        """;

    // Seeds one blog with exactly three published blog posts (fake data), replacing the sample post of the Blog recipe.
    // Runs outside the capture, on a fresh site with GraphQL disabled.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // The id of the Blog and of the sample post, read from the admin lists.
        var blogId = await page.EvaluateAsync<string>("""
            () => {
              const a = [...document.querySelectorAll('a[href$="/Display"]')].find(x => x.textContent.trim() === 'Blog');
              return a ? a.getAttribute('href').split('/ContentItems/')[1].split('/')[0] : '';
            }
            """);
        await page.GotoAsync(BaseUrl + $"/Admin/Contents/ContentItems/{blogId}/Display", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var sampleId = await page.EvaluateAsync<string>("""
            () => {
              const a = [...document.querySelectorAll('a[href*="/Edit"]')].find(x => x.textContent.trim() === 'Man must explore, and this is exploration at its greatest');
              return a ? a.getAttribute('href').split('/ContentItems/')[1].split('/')[0] : '';
            }
            """);
        Console.WriteLine($"blog={blogId} sample={sampleId}");

        if (blogId is "" || sampleId is "")
        {
            throw new InvalidOperationException("Could not find the Blog and the sample post.");
        }

        var json = $$"""
            { "steps": [ { "name": "content", "Data": [
            {{Post("dwdof7frmbuydevrr1xj5xf7ly", "Planning a Week in the Alps", "blog/planning-a-week-in-the-alps", "Notes on trains, huts and maps for a week in the mountains.", blogId, 0, "Trains, huts and a good map", "2026-03-02T09:00:00Z")}},
            {{Post("16kcqoi33qhar4cz4lqgeie807", "Packing Light for Long Walks", "blog/packing-light-for-long-walks", "What to leave at home when the route is long.", blogId, 1, "What stays at home", "2026-03-16T09:00:00Z")}},
            {{Post("zyfutu2r9h6z4q8u8fubx964y9", "A Lake Before Breakfast", "blog/a-lake-before-breakfast", "A short walk to the water before the day starts.", blogId, 2, "Early mornings by the water", "2026-03-30T09:00:00Z")}}
            ] } ] }
            """;

        // Imports the recipe like the JSON Import page does: a POST of the form with its antiforgery token.
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var status = await page.EvaluateAsync<string>("""
            async (json) => {
              const token = document.querySelector('form input[name="__RequestVerificationToken"]').value;
              const body = new URLSearchParams({ Json: json, __RequestVerificationToken: token });
              const response = await fetch('/Admin/DeploymentPlan/Import/Json', { method: 'POST', body, headers: { 'Content-Type': 'application/x-www-form-urlencoded' } });
              const text = await response.text();
              return response.status + ' ' + (text.includes('Recipe imported successfully') ? 'imported' : 'NOT imported');
            }
            """, json);
        Console.WriteLine($"import: {status}");

        // Deletes the sample post like the Delete action of the list does.
        await page.GotoAsync(BaseUrl + $"/Admin/Contents/ContentItems/{blogId}/Display", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var deleted = await page.EvaluateAsync<string>("""
            async (sampleId) => {
              const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
              const response = await fetch(`/Admin/Contents/ContentItems/${sampleId}/Delete`, { method: 'POST', body: new URLSearchParams({ __RequestVerificationToken: token }), headers: { 'Content-Type': 'application/x-www-form-urlencoded' } });
              return String(response.status) + ' ' + response.url;
            }
            """, sampleId);
        Console.WriteLine($"delete sample: {deleted}");
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static ILocator ToolsGroup(IPage page) => page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))");

    private static ILocator ToolsLink(IPage page, string text) => page.Locator($"#left-nav ul.nav-group-items[data-title=\"Tools\"] a.item-label:has-text(\"{text}\")");

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await scene.StartCaptureAsync();

        var row = page.Locator("#btn-enable-OrchardCore_Apis_GraphQL").Locator("xpath=ancestor::*[contains(@class,'list-group-item')][1]");

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(ToolsGroup(page));
            await ToolsLink(page, "Features").WaitForAsync();
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(ToolsLink(page, "Features")), 300);
            await page.Locator("#search-box").WaitForAsync();
            await scene.PauseAsync(600);
            await scene.TypeAsync(page.Locator("#search-box"), "GraphQL", 3);
            await scene.PauseAsync(500);

            if (await page.Locator("#search-box").InputValueAsync() != "GraphQL")
            {
                await page.Locator("#search-box").FillAsync("GraphQL");
            }

            await page.Locator("#btn-enable-OrchardCore_Apis_GraphQL").WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            await scene.HighlightAsync(row);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(page.Locator("#btn-enable-OrchardCore_Apis_GraphQL"));

            // The notification lasts about four seconds: click late, so it is still on screen when the step ends.
            while (clock.Elapsed.TotalSeconds < 5.0)
            {
                await scene.HoldAsync(0.2);
            }

            var (x, y) = await scene.CenterOfAsync(page.Locator("#btn-enable-OrchardCore_Apis_GraphQL"));
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y));
                await page.Locator("#btn-disable-OrchardCore_Apis_GraphQL").WaitForAsync();
            }, 100);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.PauseAsync(300);
            var link = ToolsLink(page, "GraphiQL");

            if (!await link.IsVisibleAsync())
            {
                await scene.ClickAsync(ToolsGroup(page));
            }

            await link.WaitForAsync();
            await scene.MoveToAsync(link);
            await scene.HighlightAsync(link);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task QueryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").WaitForAsync();
        await ToolsGroup(page).ClickAsync();
        await ToolsLink(page, "GraphiQL").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("query-1", async () =>
        {
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(ToolsLink(page, "GraphiQL")), 300);
            await page.Locator(".graphiql-explorer-root").GetByText("blogPost", new() { Exact = true }).First.WaitForAsync();
            await scene.MoveToAsync(page.Locator(".graphiql-explorer-root"));
            await scene.HoldAsync(1.5);
            await scene.MoveToAsync(page.Locator(".query-editor"));
            await scene.HoldAsync(1.5);
            await scene.MoveToAsync(page.Locator(".result-window"));
        });

        await scene.StepAsync("query-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync(AddressA);
                await page.Locator(".query-editor").GetByText("displayText").First.WaitForAsync();
                await page.Locator(".graphiql-explorer-root").GetByText("blogPost", new() { Exact = true }).First.WaitForAsync();
            }, 300);
            await scene.HighlightAsync(page.Locator(".graphiql-explorer-root").GetByText("blogPost", new() { Exact = true }).First);
        });

        await scene.StepAsync("query-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator(".execute-button"));
            await page.Locator(".result-window").GetByText("A Lake Before Breakfast").First.WaitForAsync();
        });

        await scene.StepAsync("query-4", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync(AddressB);
                await page.Locator(".query-editor").GetByText("contentItems").First.WaitForAsync();
            }, 300);
            await scene.ClickAsync(page.Locator(".execute-button"));
            await page.Locator(".result-window").GetByText("Early mornings by the water").First.WaitForAsync();
        });

        await scene.ClearHighlightsAsync();
    }
}

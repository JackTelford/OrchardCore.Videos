using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-parts", PartsAsync),
        ("02-roles", RolesAsync),
        ("03-type", TypeAsync),
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

    // Enables the GraphQL feature (the Blog recipe does not) and warms up the pages of the clips, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);

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

        foreach (var path in new[]
        {
            "/Admin",
            "/Admin/Roles/Index",
            "/Admin/Roles/Edit/Anonymous",
            "/Admin/ContentTypes/List",
            "/Admin/ContentTypes/Edit/Article",
            "/Admin/ContentTypes/Edit/BlogPost",
            "/Admin/ContentTypes/BlogPost/ContentParts/MarkdownBodyPart/Edit",
            "/Admin/ContentTypes/BlogPost/ContentParts/AutoroutePart/Edit",
            "/Admin/GraphQL",
        })
        {
            await page.GotoAsync(BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            Console.WriteLine($"warm {path}: {await page.TitleAsync()}");
        }

        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // Opens the page with the browser's storage cleared (the admin menu starts closed).
    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    // A shorter pointer move (the machine is loaded, so each mouse event is slow).
    private static async Task MoveAsync(Scene scene, ILocator locator)
    {
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 9);
    }

    private static async Task PressAsync(Scene scene, ILocator locator, int pauseAfter = 200)
    {
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 9);
        await Task.Delay(120);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await Task.Delay(pauseAfter);
    }

    // Moves to a link or button that navigates and clicks it; the page load is cut off camera.
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await MoveAsync(scene, locator);
        await Task.Delay(120);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 150);
    }

    // Holds until the step has run for the given number of seconds (camera time: use it before any off camera wait).
    private static Task HoldUntilAsync(Scene scene, Stopwatch watch, double seconds)
        => watch.Elapsed.TotalSeconds < seconds ? scene.HoldAsync(seconds - watch.Elapsed.TotalSeconds) : Task.CompletedTask;

    private static ILocator Group(IPage page, string title)
        => page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{title}\"))");

    private static ILocator MenuItem(IPage page, string group, string text)
        => page.Locator($"#left-nav ul[data-title=\"{group}\"] a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{text}\\s*$") });

    private static ILocator Role(IPage page, string name) => page.Locator($".roles [data-filter-value=\"{name}\"]");

    private static ILocator GraphQLGroup(IPage page) => page.Locator("fieldset.permissions-list:visible").First;

    private static async Task SearchPermissionsAsync(Scene scene, string text)
    {
        var box = scene.Page.Locator("#search-box");
        await box.WaitForAsync();
        await PressAsync(scene, box, pauseAfter: 200);
        await scene.TypeAsync(box, text, 9);

        if (await box.InputValueAsync() != text)
        {
            await box.FillAsync(text);
        }

        await scene.Page.Locator("tr[data-text=\"Execute GraphQL.\"]").WaitForAsync();
        await scene.PauseAsync(300);
    }

    // Opens the Content Types list through the menu, as the narration says.
    private static async Task OpenContentTypesAsync(Scene scene)
    {
        var page = scene.Page;
        await PressAsync(scene, Group(page, "Design"), pauseAfter: 120);
        await PressAsync(scene, page.Locator("#left-nav button.oc-navigation-admin-design-content_definition"), pauseAfter: 120);
        await GoAsync(scene, page.Locator("#left-nav a[href$=\"/Admin/ContentTypes/List\"]"));
        await page.Locator("a.edit-button[href$=\"/ContentTypes/Edit/Article\"]").WaitForAsync();
    }

    private static async Task PartsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await Group(page, "Design").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("parts-1", async () =>
        {
            await OpenContentTypesAsync(scene);
            await GoAsync(scene, page.Locator("a.edit-button[href$=\"/ContentTypes/Edit/BlogPost\"]"));
            var row = page.Locator("li:has(input[name=\"OrderedPartNames\"][value=\"MarkdownBodyPart\"])").First;
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
            await scene.HoldAsync(0.7);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a[href$=\"/ContentTypes/BlogPost/ContentParts/MarkdownBodyPart/Edit\"]"));
            await page.Locator("input[id$=\"Settings_Collapse\"]").WaitForAsync(new() { State = WaitForSelectorState.Attached });
        });

        await scene.StepAsync("parts-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("label[for$=\"Settings_Collapse\"]"));
            await scene.HoldAsync(0.8);
            await scene.ClearHighlightsAsync();
            await PressAsync(scene, page.Locator("input[id$=\"Settings_Collapse\"]"), pauseAfter: 300);
            await page.Locator("input[id$=\"Settings_PreventFieldNameCollision\"]").WaitForAsync();
            await scene.HighlightAsync(page.Locator("label[for$=\"Settings_Hidden\"]"));
        });

        await scene.StepAsync("parts-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("a.cancel"));
            await GoAsync(scene, page.Locator("a[href$=\"/ContentTypes/BlogPost/ContentParts/AutoroutePart/Edit\"]"));
            var hint = page.GetByText("NB. Setting is collapsed by default, and cannot be overwritten.");
            await hint.WaitForAsync();
            await scene.HighlightAsync(hint);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin");
        await Group(page, "Access Control").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("roles-1", async () =>
        {
            await PressAsync(scene, Group(page, "Access Control"), pauseAfter: 120);
            await GoAsync(scene, MenuItem(page, "Access Control", "Roles"));
            await GoAsync(scene, Role(page, "anonymous").Locator("a.edit"));
            await SearchPermissionsAsync(scene, "GraphQL");
            await scene.HighlightAsync(GraphQLGroup(page).Locator("legend"));
        });

        await scene.StepAsync("roles-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            await PressAsync(scene, page.Locator("[id=\"Checkbox.ExecuteGraphQL\"]"), pauseAfter: 200);
            await HoldUntilAsync(scene, watch, durations["roles-2"] - 2.0);
            await GoAsync(scene, page.Locator("button.save"));
            await Role(page, "anonymous").WaitForAsync();
        });

        await scene.StepAsync("roles-3", async () =>
        {
            var message = page.GetByText("Role updated successfully.");
            await message.First.WaitForAsync();
            await scene.HighlightAsync(message.First);
            await scene.HoldAsync(1.6);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, Role(page, "anonymous").Locator("a.edit"));
            await SearchPermissionsAsync(scene, "GraphQL");
            await scene.HighlightAsync(page.Locator("tr[data-text=\"Execute GraphQL.\"]"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TypeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/GraphQL");
        await page.Locator(".graphiql-explorer-root [data-field-name=\"article\"]").First.WaitForAsync(new() { Timeout = 120_000 });
        await scene.StartCaptureAsync();

        await scene.StepAsync("type-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".graphiql-explorer-root [data-field-name=\"article\"]").First);
            await scene.HoldAsync(1.5);
            await scene.ClearHighlightsAsync();
            await OpenContentTypesAsync(scene);
            await GoAsync(scene, page.Locator("a.edit-button[href$=\"/ContentTypes/Edit/Article\"]"));
            await page.GetByText("Edit Content Type - Article").First.WaitForAsync();
        });

        await scene.StepAsync("type-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            var box = page.Locator("#ContentTypeDefinition_Settings_Hidden");
            await PressAsync(scene, box, pauseAfter: 200);
            await HoldUntilAsync(scene, watch, durations["type-2"] - 2.2);
            await GoAsync(scene, page.Locator("button.save"));
            await page.GetByText("Content type updated successfully.").First.WaitForAsync();
        });

        await scene.StepAsync("type-3", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/Admin/GraphQL");
                await page.Locator(".graphiql-explorer-root [data-field-name=\"blogPost\"]").First.WaitForAsync(new() { Timeout = 120_000 });
            }, 300);
            await scene.HighlightAsync(page.Locator(".graphiql-explorer-root"));
        });

        await scene.ClearHighlightsAsync();
    }
}

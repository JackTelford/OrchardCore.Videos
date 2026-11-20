using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Orchard Core Content Types". None of them saves anything. PrepareAsync creates the part
/// Contact Details (Phone, Website) outside the capture.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-types", TypesAsync),
        ("03-anatomy", AnatomyAsync),
        ("04-editor", EditorAsync),
        ("05-parts", PartsAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task GoAsync(string path) => await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.NetworkIdle });

        await GoAsync("/Admin/ContentParts/Create");
        await page.Locator("input[name='Name']").FillAsync("ContactDetails");
        await page.Locator("button.create[type='submit']").ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("/ContentParts/Edit/"));
        await page.Locator("#ContentPartDefinition_DisplayName").FillAsync("Contact Details");
        await page.Locator("#ContentPartDefinition_Description").FillAsync("Phone number and website of a person or a company.");
        await page.Locator("button.save").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);

        foreach (var (name, type) in new[] { ("Phone", "TextField"), ("Website", "LinkField") })
        {
            await GoAsync("/Admin/ContentTypes/AddFieldsTo/ContactDetails");
            await page.Locator("#DisplayName").FillAsync(name);
            await page.Keyboard.PressAsync("Tab");
            await page.Locator($"input[name='FieldTypeName'][value='{type}']").CheckAsync();
            await page.Locator("button[name='submit.Save'][value='Save']").First.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(1000);
        }

        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task OpenGroupAsync(Scene scene, string group)
    {
        var button = scene.Page.Locator("#left-nav button.item-label").Filter(new() { HasTextRegex = new($"^\\s*{group}\\s*$") }).First;

        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(button, pauseAfter: 150);
        }
    }

    private static ILocator Row(Scene scene, string technicalName) => scene.Page.Locator($"[data-type-name='{technicalName}']").First;

    private static async Task RingAsync(Scene scene, ILocator locator, int hold = 1500, bool clear = true, string caption = null)
    {
        await scene.HighlightAsync(locator, caption);
        await scene.PauseAsync(hold);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static ILocator Setting(Scene scene, string label)
        => scene.Page.Locator($".ocat-limited-wrapper:has(label:text-is('{label}')) input").First;

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("types-1", async () =>
        {
            await OpenGroupAsync(scene, "Design");
            await OpenGroupAsync(scene, "Content Definition");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new("^\\s*Content Types\\s*$") }).First.ClickAsync());
            await RingAsync(scene, scene.Page.Locator("#search-box"), 1500);
            await scene.HighlightAsync(scene.Page.Locator("#btnCreate"));
        });

        await scene.StepAsync("types-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = Row(scene, "BlogPost");
            await row.ScrollIntoViewIfNeededAsync();
            await RingAsync(scene, row.Locator("a").First, 1200);
            var badge = row.Locator(".badge").First;
            await scene.MoveToAsync(badge);
            await scene.HighlightAsync(badge);
            await scene.PauseAsync(1000);
        });

        await scene.StepAsync("types-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var container = Row(scene, "Container");
            await container.ScrollIntoViewIfNeededAsync();
            var widget = container.Locator(".badge").Nth(1);
            await scene.MoveToAsync(widget);
            await RingAsync(scene, widget, 1000);
            var menu = Row(scene, "ContentMenuItem");
            await menu.ScrollIntoViewIfNeededAsync();
            var menuBadge = menu.Locator(".badge").Nth(1);
            await scene.MoveToAsync(menuBadge);
            await scene.HighlightAsync(menuBadge);
        });

        await scene.StepAsync("types-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.HighlightAsync(Row(scene, "Article").Locator("a.btn").First, padding: 4);
            await scene.HighlightAsync(Row(scene, "Article").Locator("[role='btn-list-Article']"));
            await scene.HighlightAsync(Row(scene, "Article").Locator("[role='btn-edit-Article']"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            var blogPost = Row(scene, "BlogPost");
            await blogPost.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(blogPost.Locator("[role='btn-list-BlogPost']"));
            await scene.HighlightAsync(blogPost.Locator("[role='btn-edit-BlogPost']"));
        });
    }

    private static async Task AnatomyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("anatomy-1", async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("a[role='btn-edit-Article']").ClickAsync());
            await RingAsync(scene, Setting(scene, "Technical Name"), 1200);
            await RingAsync(scene, Setting(scene, "Display Name"), 800, clear: false);
        });

        await scene.StepAsync("anatomy-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("label.form-check-label:text-is('Creatable')").First.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.Locator(".form-check:has(label.form-check-label:text-is('Creatable'))").First);
            await scene.HighlightAsync(scene.Page.Locator(".form-check:has(label.form-check-label:text-is('Listable'))").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".form-check:has(label.form-check-label:text-is('Draftable'))").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 150, 6);
            await scene.HighlightAsync(Setting(scene, "Stereotype"));
            await scene.HighlightAsync(Setting(scene, "Category"));
            await scene.HighlightAsync(Setting(scene, "Thumbnail"));
        });

        await scene.StepAsync("anatomy-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("h3:text-is('Fields')").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200, 6);
            await RingAsync(scene, scene.Page.Locator("ul#fields li", new() { HasText = "Subtitle" }).First, 2000);
            await scene.HighlightAsync(scene.Page.Locator("ul#fields li", new() { HasText = "Banner Image" }).First);
        });

        await scene.StepAsync("anatomy-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("ul#parts").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 150, 6);
            var rows = scene.Page.Locator("ul#parts li");

            for (var i = 0; i < Math.Min(await rows.CountAsync(), 4); i++)
            {
                await scene.MoveToAsync(rows.Nth(i));
                await scene.PauseAsync(500);
            }

            await scene.HighlightAsync(scene.Page.Locator("ul#parts"));
        });

        await scene.StepAsync("anatomy-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("ul#parts li", new() { HasText = "Contains the fields for the current type" }).First);
        });

        await scene.StepAsync("anatomy-6", async () =>
        {
            await scene.ClearHighlightsAsync();

            foreach (var part in new[] { "Title", "Autoroute", "HtmlBody" })
            {
                await scene.ClearHighlightsAsync();
                var row = scene.Page.Locator("ul#parts li", new() { HasText = part }).First;
                await scene.HighlightAsync(row.Locator("a.btn, button.btn").First);
                await scene.HighlightAsync(row.Locator("a.btn, button.btn").Nth(1));
                await scene.PauseAsync(900);
            }
        });

        await scene.StepAsync("anatomy-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("a.btn:text-is('Add Parts')").First.ClickAsync());
            await scene.ScrollAsync(0, 300, 12);
        });

        await scene.StepAsync("anatomy-8", async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("a.btn.cancel").First.ClickAsync());
            await scene.Page.Locator("a.btn:text-is('Add Field')").First.ScrollIntoViewIfNeededAsync();
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("a.btn:text-is('Add Field')").First.ClickAsync());
            await RingAsync(scene, scene.Page.Locator("#DisplayName"), 600);
            await RingAsync(scene, scene.Page.Locator("#Name"), 600);
            await scene.ScrollAsync(0, 250, 8);

            foreach (var type in new[] { "Boolean Field", "Date Field", "Media Field", "Numeric Field", "Text Field" })
            {
                await scene.ClearHighlightsAsync();
                var radio = scene.Page.GetByLabel(type, new() { Exact = true });
                await radio.ScrollIntoViewIfNeededAsync();
                await scene.HighlightAsync(radio);
                await scene.PauseAsync(500);
            }
        });
    }

    private static async Task EditorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("editor-1", async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("#btn-create-Article").ClickAsync());
            await scene.Page.Locator("#TitlePart_Title").WaitForAsync();
        });

        await scene.StepAsync("editor-2", async () =>
        {
            await RingAsync(scene, scene.Page.GetByText("Title", new() { Exact = true }).First, 700);
            await RingAsync(scene, scene.Page.GetByText("Permalink", new() { Exact = true }).First, 700);
            await scene.ScrollAsync(0, 250, 6);
            await RingAsync(scene, scene.Page.GetByText("HtmlBody", new() { Exact = true }).First, 800);
            await scene.ScrollAsync(0, 400, 6);
            await RingAsync(scene, scene.Page.GetByText("Subtitle", new() { Exact = true }).First, 800);
            await scene.HighlightAsync(scene.Page.GetByText("Banner Image", new() { Exact = true }).First);
        });

        await scene.StepAsync("editor-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("a.btn.cancel").First.ClickAsync());
            await scene.NavigateAsync("/about");
            await scene.Page.Locator("h1").First.WaitForAsync();
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("editor-4", async () =>
        {
            await scene.ScrollAsync(0, 600, 20);
        });
    }

    private static async Task PartsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("parts-1", async () =>
        {
            await OpenGroupAsync(scene, "Design");
            await OpenGroupAsync(scene, "Content Definition");
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new("^\\s*Content Parts\\s*$") }).First.ClickAsync());
            await scene.HighlightAsync(scene.Page.Locator("#btnCreate"));
        });

        await scene.StepAsync("parts-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 700, 20);
            await scene.ScrollAsync(0, -700, 20);
            var row = scene.Page.Locator("li.list-group-item", new() { HasText = "Contact Details" }).First;
            await row.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("parts-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("#btn-edit-ContactDetails, a[role='btn-edit-ContactDetails']").First.ClickAsync());
            await scene.HighlightAsync(Setting(scene, "Technical Name"));
            await scene.HighlightAsync(scene.Page.Locator("#ContentPartDefinition_DisplayName"));
            await scene.HighlightAsync(scene.Page.Locator("#ContentPartDefinition_Description"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".form-check:has(#ContentPartDefinition_Attachable)").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".form-check:has(#ContentPartDefinition_Reusable)").First);
        });

        await scene.StepAsync("parts-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("h3:text-is('Fields')").First.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.Locator("ul#fields").First);
        });
    }
}

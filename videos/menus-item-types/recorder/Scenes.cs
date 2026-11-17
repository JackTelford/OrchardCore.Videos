using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the menus-item-types video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-dialog", DialogAsync),
        ("03-link", LinkAsync),
        ("04-content", ContentAsync),
        ("05-html", HtmlAsync),
        ("06-permissions", PermissionsAsync),
        ("07-publish", PublishAsync),
        ("07-visitor", VisitorAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string state = null)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, state ?? StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task AtAsync(Stopwatch watch, int milliseconds)
    {
        var remaining = milliseconds - (int)watch.ElapsedMilliseconds;

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }
    }

    // Moves to a link or button, clicks it and cuts the page load out of the video.
    private static async Task GoAsync(Scene scene, ILocator link, ILocator ready = null)
    {
        await scene.MoveToAsync(link);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(async () => await link.ClickAsync());

            if (ready is not null)
            {
                await ready.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Visible });
            }
        }, 450);
    }

    private static async Task RingAsync(Scene scene, ILocator target, double seconds)
    {
        await target.ScrollIntoViewIfNeededAsync();
        await Task.Delay(400);
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    private static async Task TypeInAsync(Scene scene, ILocator field, string text, int delay = 35)
    {
        await field.ScrollIntoViewIfNeededAsync();
        await scene.ClickAsync(field, pauseAfter: 100);
        await scene.Page.Keyboard.TypeAsync(text, new() { Delay = delay });
    }

    private static ILocator PublishButton(Scene scene) => scene.Page.Locator("button[name='submit.Publish']").First;

    private static ILocator Row(Scene scene, string name) => scene.Page.Locator("li").Filter(new() { HasText = name }).Filter(new() { Has = scene.Page.Locator("a.btn, button.btn") }).Last;

    private static ILocator Field(Scene scene, string label) => scene.Page.GetByLabel(label, new() { Exact = true }).First;

    // The editor of the Main Menu, opened without a return address (outside the capture).
    private static async Task<string> OpenMenuEditorAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/Contents/ContentItems/Menu");
        var href = await scene.Page.Locator(".list-group-item:has-text('Main Menu') a.btn.edit").First.GetAttributeAsync("href");
        var url = href.Split('?')[0];
        await scene.OpenAsync(url);
        await scene.Page.Locator("button:has-text('Add Menu Item')").First.WaitForAsync();

        return url;
    }

    // Clicks Add Menu Item, then Add on the card of a type, and waits for the form.
    private static async Task AddItemAsync(Scene scene, string type, ILocator ready)
    {
        var page = scene.Page;
        await scene.ClickAsync(page.Locator("button:has-text('Add Menu Item')").First, pauseAfter: 300);
        await page.Locator("#modalMenuItems").WaitForAsync();
        var card = page.Locator("#modalMenuItems .card").Filter(new() { Has = page.Locator("h4", new() { HasTextString = type }) }).First;
        await scene.HoldAsync(0.6);
        await GoAsync(scene, card.Locator("a.thumbnail-link-create").First, ready);
    }

    private const string SeedRecipe = """
{ "steps": [ { "name": "content", "data": [ { "ContentItemId": "4openinghours0000000000a1b", "ContentType": "Article", "DisplayText": "Opening Hours", "Latest": true, "Published": true, "Author": "admin", "TitlePart": { "Title": "Opening Hours" }, "AutoroutePart": { "Path": "opening-hours" }, "HtmlBodyPart": { "Html": "<p>The shop is open every day from 9 a.m. to 5 p.m.</p>" } } ] } ] }
""";

    // Imports the article that the Content Menu Item points to (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", SeedRecipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000);
        Console.WriteLine("Imported the article.");
        await context.CloseAsync();
    }

    private static async Task DialogAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("dialog-1", async () =>
        {
            var menus = page.Locator("#left-nav a[href*='ContentItems/Menu?']").First;

            if (!await menus.IsVisibleAsync())
            {
                await scene.ClickAsync(page.Locator("#left-nav figcaption:has(span.title:text-is('Content'))").First, pauseAfter: 250);
            }

            await GoAsync(scene, menus, page.Locator("li:has-text('Main Menu')").First);
            await GoAsync(scene, page.Locator(".list-group-item:has-text('Main Menu') a.btn.edit").First, page.Locator("button:has-text('Add Menu Item')").First);
            await scene.HighlightAsync(page.Locator("button:has-text('Add Menu Item')").First);
        });

        await scene.StepAsync("dialog-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("button:has-text('Add Menu Item')").First, pauseAfter: 300);
            await page.Locator("#modalMenuItems").WaitForAsync();

            foreach (var title in new[] { "Link Menu Item", "Content Menu Item", "Html Menu Item" })
            {
                await scene.MoveToAsync(page.Locator("#modalMenuItems h4", new() { HasTextString = title }).First);
                await scene.HoldAsync(1.0);
            }
        });
    }

    private static async Task LinkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenMenuEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("link-1", async () =>
        {
            await AddItemAsync(scene, "Link Menu Item", Field(scene, "Name"));
            await TypeInAsync(scene, Field(scene, "Name"), "Editor Guide");
            await TypeInAsync(scene, Field(scene, "Url"), "https://docs.orchardcore.net/en/latest/");
        });

        await scene.StepAsync("link-2", async () =>
        {
            await TypeInAsync(scene, Field(scene, "Target"), "_blank");
            await page.Keyboard.PressAsync("Tab");
            await scene.HighlightAsync(Field(scene, "Target"));
        });

        await scene.StepAsync("link-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, PublishButton(scene), page.Locator("li:has-text('Editor Guide')").First);
            await scene.HighlightAsync(Row(scene, "Editor Guide"));
        });
    }

    private static async Task ContentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenMenuEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("content-1", async () =>
        {
            await AddItemAsync(scene, "Content Menu Item", Field(scene, "Name"));
        });

        await scene.StepAsync("content-2", async () =>
        {
            await TypeInAsync(scene, Field(scene, "Name"), "Hours");
            await scene.ClickAsync(page.Locator(".multiselect").First, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Opening", new() { Delay = 50 });
            var result = page.Locator(".multiselect__option").Filter(new() { HasText = "Opening Hours" }).First;
            await result.WaitForAsync(new() { Timeout = 20000 });
            await scene.ClickAsync(result, pauseAfter: 400);
        });

        await scene.StepAsync("content-3", async () =>
        {
            await scene.HighlightAsync(page.Locator(".ocat-wrapper:has(label:text-is('Selected Content Item')), .content-picker-field").First);
        });

        await scene.StepAsync("content-4", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var box = page.Locator("input[type=checkbox]").Filter(new() { Has = page.Locator("xpath=..") }).Last;
            await scene.HighlightAsync(page.GetByText("Check content permissions").First);
            await AtAsync(watch, Math.Max(500, (int)(durations["content-4"] * 1000) - 4500));
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, PublishButton(scene), page.Locator("li:has-text('Hours')").First);
        });
    }

    private static async Task HtmlAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenMenuEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("html-1", async () =>
        {
            await AddItemAsync(scene, "Html Menu Item", Field(scene, "Name"));

            foreach (var label in new[] { "Name", "Url", "Target", "Html" })
            {
                await scene.MoveToAsync(Field(scene, label));
                await scene.HoldAsync(0.6);
            }

            await scene.HighlightAsync(Field(scene, "Html"));
        });

        await scene.StepAsync("html-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TypeInAsync(scene, Field(scene, "Name"), "Contact us");
            await TypeInAsync(scene, Field(scene, "Url"), "mailto:hello@example.com");
        });

        await scene.StepAsync("html-3", async () =>
        {
            await TypeInAsync(scene, Field(scene, "Html"), "<em>Contact</em> us");
            await scene.HighlightAsync(Field(scene, "Html"));
        });

        await scene.StepAsync("html-4", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            await AtAsync(watch, Math.Max(500, (int)(durations["html-4"] * 1000) - 3500));
            await GoAsync(scene, PublishButton(scene), page.Locator("li:has-text('Contact')").First);
        });
    }

    private static async Task PermissionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenMenuEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("perm-1", async () =>
        {
            await GoAsync(scene, Row(scene, "Editor Guide").Locator("a.btn.edit, a:text-is('Edit')").First, Field(scene, "Name"));
            var permissions = page.Locator("label:text-is('Permissions')").First;
            await permissions.ScrollIntoViewIfNeededAsync();
            await Task.Delay(500);
            await scene.HighlightAsync(permissions);
        });

        await scene.StepAsync("perm-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator(".multiselect").First, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Access admin", new() { Delay = 50 });
            var option = page.Locator(".multiselect__option").Filter(new() { HasText = "Access admin panel" }).First;
            await option.WaitForAsync(new() { Timeout = 20000 });
            await scene.ClickAsync(option, pauseAfter: 400);
        });

        await scene.StepAsync("perm-3", async () =>
        {
            await scene.HoldAsync(Math.Min(3.0, durations["perm-3"] * 0.3));
            await GoAsync(scene, PublishButton(scene), page.Locator("li:has-text('Editor Guide')").First);
            await scene.HighlightAsync(Row(scene, "Editor Guide"));
        });
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenMenuEditorAsync(scene);
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("publish-1", async () =>
        {
            foreach (var name in new[] { "Editor Guide", "Hours", "Contact us" })
            {
                await scene.MoveToAsync(Row(scene, name));
                await scene.HoldAsync(1.6);
            }

            await scene.MoveToAsync(PublishButton(scene));
        });

        await scene.StepAsync("publish-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await AtAsync(watch, Math.Max(500, (int)(durations["publish-2"] * 1000) - 7500));
            await GoAsync(scene, PublishButton(scene), page.Locator(".toast.message-success").First);
            await scene.HighlightAsync(page.Locator(".toast.message-success").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("publish-3", async () =>
        {
            await scene.NavigateAsync("/");
            await page.Locator("#navbarResponsive").GetByText("Contact").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("#navbarResponsive"));
        });
    }

    private static async Task VisitorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, Path.Combine(Root, "no-sign-in.json"));
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("visitor-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#navbarResponsive"));
        });
    }
}

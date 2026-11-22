using System.Text.Json;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Navigation in Themes". PrepareAsync enables the Admin Dashboard, allows the page size selection and imports twelve
/// invented blog posts. Record in this order: 02-menu-site, 05-pager-blog, 05-pager-admin, then 04-trail-site (it clears Show breadcrumb).
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
        ("02-menu-site", MenuSiteAsync),
        ("05-pager-blog", PagerBlogAsync),
        ("05-pager-admin", PagerAdminAsync),
        ("04-trail-site", TrailSiteAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90000);

        await page.GotoAsync(BaseUrl + "/Admin/Features", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#search-box").FillAsync("Admin Dashboard");

        if (await page.Locator("#btn-enable-OrchardCore_AdminDashboard").CountAsync() > 0)
        {
            await page.Locator("#btn-enable-OrchardCore_AdminDashboard").ClickAsync();
            await page.GetByText("was enabled.").First.WaitForAsync();
        }

        Console.WriteLine("Enabled the Admin Dashboard.");

        await page.GotoAsync(BaseUrl + "/Admin/Settings/general", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.GetByLabel("Allow users to change the page size on listing pages").CheckAsync();
        await page.Locator("button.save").First.ClickAsync();
        await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
        Console.WriteLine("Allowed the page size selection.");

        var blog = await page.EvaluateAsync<string>(@"async () => {
            const html = await (await fetch('/Admin/Contents/ContentItems/Blog')).text();
            const m = html.match(/\/Admin\/Contents\/ContentItems\/([0-9a-z]{26})\/(?:Display|Edit)/);
            return m ? m[1] : '';
        }");

        if (string.IsNullOrEmpty(blog))
        {
            throw new InvalidOperationException("The blog was not found.");
        }

        var items = Enumerable.Range(1, 12).Select(n => new Dictionary<string, object>
        {
            ["ContentType"] = "BlogPost",
            ["ContentItemId"] = "[js: uuid()]",
            ["DisplayText"] = $"Orchard notes, part {n}",
            ["Author"] = "admin",
            ["Latest"] = true,
            ["Published"] = true,
            ["CreatedUtc"] = $"2025-03-{n:D2}T10:00:00Z",
            ["ModifiedUtc"] = $"2025-03-{n:D2}T10:00:00Z",
            ["PublishedUtc"] = $"2025-03-{n:D2}T10:00:00Z",
            ["TitlePart"] = new { Title = $"Orchard notes, part {n}" },
            ["ContainedPart"] = new { ListContentItemId = blog, Order = n },
            ["MarkdownBodyPart"] = new { Markdown = $"Fake text for part {n} of the orchard notes." },
            ["AutoroutePart"] = new { Path = $"blog/orchard-notes-part-{n}" },
            ["BlogPost"] = new { Subtitle = new { Text = $"A fake subtitle for part {n}" } },
        }).ToArray();

        var recipe = JsonSerializer.Serialize(new { steps = new object[] { new { name = "content", data = items } } });
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", recipe);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Imported twelve posts.");
        await context.CloseAsync();
    }

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

    private static async Task MenuSiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/Menu");
        await scene.Page.Locator("ul.admin-list a:has-text('Main Menu'), a:has-text('Main Menu')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("menusite-1", async () =>
        {
            var link = scene.Page.Locator("ul.admin-list a:has-text('Main Menu')").First;
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(link), leadMilliseconds: 150);
            await scene.Page.Locator("#menu").WaitForAsync();
            await scene.HighlightAsync(scene.Page.GetByLabel("Alias", new() { Exact = true }).First, "alias:main-menu");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#menu").First);
        });

        await scene.StepAsync("menusite-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/");
                await scene.Page.Locator("#mainNav").WaitForAsync();
            }, leadMilliseconds: 100);
            await scene.HighlightAsync(scene.Page.Locator("#navbarResponsive ul.navbar-nav").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PagerBlogAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("ul.pager a:has-text('Older Posts')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("blogpager-1", async () =>
        {
            await scene.ScrollAsync(0, 6000);
            var older = scene.Page.Locator("ul.pager a:has-text('Older Posts')").First;
            await scene.HighlightAsync(older);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(older);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => older.ClickAsync()), leadMilliseconds: 150);
            await scene.ScrollAsync(0, 6000);
            await scene.HighlightAsync(scene.Page.Locator("ul.pager a:has-text('Newer Posts')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PagerAdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/BlogPost");
        await scene.Page.Locator("nav[aria-label='Listing pages']").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("adminpager-1", async () =>
        {
            await scene.ScrollAsync(0, 6000);
            await scene.HighlightAsync(scene.Page.Locator("nav[aria-label='Listing pages']").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".pager-page-size").First, "Items per page");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TrailSiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/Menu");
        await scene.Page.Locator("ul.admin-list a:has-text('Main Menu')").First.ClickAsync();
        await scene.Page.Locator("#menu").WaitForAsync();
        await scene.StartCaptureAsync();

        var itemAddress = string.Empty;

        await scene.StepAsync("trailsite-1", async () =>
        {
            var edit = scene.Page.Locator("#menu li.menu-item:has-text('About') a.btn-success").First;
            await scene.MoveToAsync(edit);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(edit), leadMilliseconds: 150);
            itemAddress = scene.Page.Url.Replace(BaseUrl, string.Empty);
            await scene.Page.Locator("nav.oc-breadcrumb").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("nav.oc-breadcrumb").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("nav.oc-breadcrumb li.breadcrumb-item").First);
        });

        await scene.StepAsync("trailsite-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/Admin/Settings/admin");
                await scene.Page.GetByLabel("Show breadcrumb").WaitForAsync();
            }, leadMilliseconds: 100);
            await scene.ClickAsync(scene.Page.GetByLabel("Show breadcrumb").First, pauseAfter: 300);
            var save = scene.Page.Locator("button.save:has-text('Save')").First;
            await scene.MoveToAsync(save);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => save.ClickAsync());
                await scene.Page.GetByText("Site settings updated successfully.").First.WaitForAsync(new() { Timeout = 60000 });
            }, leadMilliseconds: 150);
            await scene.MoveToAsync(scene.Page.GetByText("Site settings updated successfully.").First);
        });

        await scene.StepAsync("trailsite-3", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync(itemAddress);
                await scene.Page.Locator("h1.oc-breadcrumb-title").WaitForAsync();
            }, leadMilliseconds: 100);
            await scene.HighlightAsync(scene.Page.Locator("h1.oc-breadcrumb-title").First);
        });

        await scene.ClearHighlightsAsync();
    }
}

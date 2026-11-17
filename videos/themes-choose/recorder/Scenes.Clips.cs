using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-page", PageAsync),
        ("03-site", SiteAsync),
        ("04-toggle", ToggleAsync),
        ("05-reset", ResetAsync),
    ];

    // Nothing to seed: the Blog recipe sets the themes. With MAKE_DEFAULT=1 it makes The Default Theme the Site theme (snapshot B).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        if (Environment.GetEnvironmentVariable("MAKE_DEFAULT") == "1")
        {
            await page.GotoAsync(BaseUrl + "/Admin/Themes", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.RunAndWaitForNavigationAsync(async () => await Card(page, "The Default Theme").Locator("button:text-is(\"Make Current\")").ClickAsync());
            await Card(page, "The Default Theme").Locator("span.text-muted:text-is(\"This is the current Site theme\")").WaitForAsync();
        }

        await page.GotoAsync(BaseUrl + "/Admin/Themes", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine(string.Join(" | ", await page.Locator("div.theme-card").EvaluateAllAsync<string[]>("els => els.map(e => e.querySelector('h4.card-title').innerText.trim() + ': ' + (e.querySelector('span.text-muted')?.innerText ?? '') + [...e.querySelectorAll('button')].map(b => ' [' + b.innerText.trim() + ']').join(''))")));
        await context.CloseAsync();
    }

    private static ILocator Card(IPage page, string name) => page.Locator($"div.theme-card:has(h4.card-title:text-is(\"{name}\"))");

    private static Task CenterAsync(ILocator locator) => locator.EvaluateAsync("e => e.scrollIntoView({block: 'center'})");

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.Locator("div.card:has(h5.card-title:has-text(\"Customize theme\"))").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("page-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("div.card:has(h5.card-title:has-text(\"Customize theme\"))"));
            await scene.PauseAsync(5600);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("#left-nav button:has(span.title:text-is(\"Design\"))"), 300);
            await scene.PauseAsync(300);
            await PointAsync(scene, page.Locator("#left-nav a[href$=\"/Admin/Themes\"]"), "Design, Themes");
        });

        await scene.StepAsync("page-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("a.btn:text-is(\"Manage themes\")"));
            await Card(page, "The Blog Theme").WaitForAsync();
        });

        await scene.StepAsync("page-3", async () =>
        {
            var blog = Card(page, "The Blog Theme");
            await CenterAsync(blog);
            await scene.HighlightAsync(blog);
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(blog.Locator("ul.list-group"));
        });

        await scene.StepAsync("page-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var admin = Card(page, "The Admin Theme");
            await CenterAsync(admin);
            await scene.HighlightAsync(admin.Locator("li.list-group-item:has(i.fa-tags)"));
        });

        await scene.StepAsync("page-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.PauseAsync(5500);
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(Card(page, "Safe Mode Theme").Locator("p.text-muted"));
        });
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Themes");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var visit = page.Locator("ul.user-top-navbar a.nav-link[href=\"/\"]");

        await scene.StepAsync("site-1", async () =>
        {
            await scene.MoveToAsync(visit);
            await scene.PauseAsync(3500);
            await scene.ClickAndWaitForNavigationAsync(visit);
            await page.Locator("nav#mainNav").WaitForAsync();
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Themes");
            var card = Card(page, "The Default Theme");
            await card.WaitForAsync();
            await CenterAsync(card);
            await scene.HighlightAsync(card);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var button = card.Locator("button:text-is(\"Make Current\")");
            await scene.MoveToAsync(button);
            await scene.PauseAsync(1200);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(button));
            await card.Locator("span.text-muted:text-is(\"This is the current Site theme\")").WaitForAsync();
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card(page, "The Default Theme").Locator("span.text-muted:text-is(\"This is the current Site theme\")"));
            await scene.PauseAsync(6500);
            await scene.ClearHighlightsAsync();
            var blog = Card(page, "The Blog Theme");
            await CenterAsync(blog);
            await scene.PauseAsync(1000);
            await scene.HighlightAsync(blog.Locator("button:text-is(\"Make Current\")"));
            await scene.PauseAsync(1100);
            await scene.HighlightAsync(blog.Locator("button:text-is(\"Disable\")"));
        });

        await scene.StepAsync("site-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(visit);
            await page.Locator("nav.navbar.fixed-top").WaitForAsync();
        });
    }

    private static async Task ToggleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Themes");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("toggle-1", async () =>
        {
            var blog = Card(page, "The Blog Theme");
            await CenterAsync(blog);
            await scene.HighlightAsync(blog.Locator("button:text-is(\"Make Current\")"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, blog.Locator("button:text-is(\"Make Current\")"));
            await Card(page, "The Blog Theme").Locator("span.text-muted:text-is(\"This is the current Site theme\")").WaitForAsync();
        });

        await scene.StepAsync("toggle-2", async () =>
        {
            var card = Card(page, "The Default Theme");
            await CenterAsync(card);
            await scene.HighlightAsync(card.Locator("button:text-is(\"Disable\")"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            var disable = card.Locator("button:text-is(\"Disable\")");
            await scene.MoveToAsync(disable);
            await scene.PauseAsync(500);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(disable));
            await card.Locator("button:text-is(\"Enable\")").WaitForAsync();
            await CenterAsync(card);
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(card.Locator("button:text-is(\"Enable\")"));
        });

        await scene.StepAsync("toggle-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.HighlightAsync(Card(page, "The Blog Theme").Locator("span.text-muted:text-is(\"This is the current Site theme\")"));
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card(page, "The Admin Theme").Locator("span.text-muted:text-is(\"This is the current Admin theme\")"));
        });
    }

    private static async Task ResetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Themes");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("div.alert.alert-danger:has(h4:text-is(\"Reset current themes\"))");

        await scene.StepAsync("reset-1", async () =>
        {
            await scene.ScrollAsync(0, 3000);
            await scene.HighlightAsync(box);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(box.Locator("a.btn-danger:text-is(\"Reset Site Theme\")"));
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(box.Locator("a.btn-danger:text-is(\"Reset Admin Theme\")"));
        });

        await scene.StepAsync("reset-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1000);
            await FastClickAsync(scene, box.Locator("a.btn-danger:text-is(\"Reset Site Theme\")"));
            await scene.PauseAsync(4800);
            await FastClickAsync(scene, page.Locator("#modalCancelButton"));
            await page.Locator("#modalCancelButton").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        });
    }
}

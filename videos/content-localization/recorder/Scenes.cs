using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the content-localization video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-part", PartAsync),
        ("04-original", OriginalAsync),
        ("05-translate", TranslateAsync),
        ("06-manage", ManageAsync),
        ("07-site", SiteAsync),
        ("08-permissions", PermissionsAsync),
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

    private static ILocator Label(Scene scene, string text) => scene.Page.GetByLabel(text, new() { Exact = true });

    private static async Task AtAsync(Stopwatch watch, int milliseconds)
    {
        var remaining = milliseconds - (int)watch.ElapsedMilliseconds;

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }
    }

    // Scrolls with the wheel until the element is comfortably in view.
    private static async Task RevealAsync(Scene scene, ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();

        if (box is not null && (box.Y < 130 || box.Y + box.Height > 700))
        {
            await scene.ScrollAsync(0, box.Y - 300);
        }
    }

    // Opens a page through the left menu: the groups to expand, then the link.
    private static async Task MenuAsync(Scene scene, string fallbackPath, params string[] path)
    {
        var page = scene.Page;
        var groups = path[..^1];
        var container = page.Locator($"#left-nav ul[data-title='{groups[^1]}']");

        foreach (var group in groups)
        {
            var list = page.Locator($"#left-nav ul[data-title='{group}']");

            if (await list.CountAsync() == 0)
            {
                await scene.NavigateAsync(fallbackPath);

                return;
            }

            if (!await list.IsVisibleAsync())
            {
                await scene.ClickAsync(page.Locator($"#left-nav figcaption:has(span.title:text-is('{group}'))").First, pauseAfter: 250);
            }
        }

        await scene.ClickAndWaitForNavigationAsync(container.GetByRole(AriaRole.Link, new() { Name = path[^1], Exact = true }).First);
    }

    private static ILocator ItemRow(Scene scene, string title) => scene.Page.Locator("li.list-group-item").Filter(new() { Has = scene.Page.Locator($"a:text-is('{title}')") }).First;

    // Clicks a button that loads a page, and cuts the load out of the clip.
    private static async Task LoadOffCameraAsync(Scene scene, ILocator target, string toast = null)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(async () => await target.ClickAsync());

            if (toast is not null)
            {
                await scene.Page.Locator(".toast").Filter(new() { HasText = toast }).First.WaitForAsync(new() { Timeout = 60000 });
            }
        });
    }

    // Enables Localization and sets the cultures en-US (default), fr-FR and es-ES (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.Locator("#search-box").FillAsync("Localization");
        await page.Locator("#btn-enable-OrchardCore_Localization").ClickAsync();
        await page.Locator("#btn-disable-OrchardCore_Localization").WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 90000 });

        await page.GotoAsync(BaseUrl + "/Admin/Settings/localization");
        var wrapper = page.Locator(".localization-settings-wrapper");
        await wrapper.Locator("tbody tr").First.WaitForAsync();

        foreach (var culture in new[] { "fr-FR", "es-ES", "en-US" })
        {
            if (await wrapper.Locator("tbody tr", new() { HasTextString = culture }).CountAsync() == 0)
            {
                await wrapper.Locator("select").SelectOptionAsync(culture);
                await wrapper.Locator("a.btn-success").ClickAsync();
            }
        }

        // Removes every other row.
        for (var i = await wrapper.Locator("tbody tr").CountAsync() - 1; i >= 0; i--)
        {
            var row = wrapper.Locator("tbody tr").Nth(i);
            var text = await row.InnerTextAsync();

            if (!text.Contains("en-US") && !text.Contains("fr-FR") && !text.Contains("es-ES"))
            {
                await row.Locator("a[title='Remove culture']").ClickAsync();
            }
        }

        await wrapper.Locator("tbody tr", new() { HasTextString = "en-US" }).Locator("input[type=radio]").CheckAsync();
        await page.Locator("button.save").First.ClickAsync();
        await page.Locator(".toast").Filter(new() { HasText = "The site has been restarted for the settings to take effect." }).WaitForAsync(new() { Timeout = 90000 });
        await page.GotoAsync(BaseUrl + "/Admin/Settings/localization");
        await wrapper.Locator("tbody tr").First.WaitForAsync();
        Console.WriteLine("Rows: " + (await wrapper.Locator("tbody").InnerTextAsync()).Replace('\n', ' '));
        await context.CloseAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var row = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_ContentLocalization") }).First;

        await scene.StepAsync("enable-1", async () =>
        {
            await MenuAsync(scene, "/Admin/Features", "Tools", "Features");
            await scene.TypeAsync(page.Locator("#search-box"), "Content Localization", delay: 40);
            await page.Locator("#btn-enable-OrchardCore_ContentLocalization").WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("#btn-enable-OrchardCore_ContentLocalization");
            var watch = Stopwatch.StartNew();
            await scene.HighlightAsync(enable);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(enable);
            await AtAsync(watch, Math.Max(2500, (int)(durations["enable-2"] * 1000) - 4300));
            await LoadOffCameraAsync(scene, enable, "The feature Content Localization was enabled.");
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await MenuAsync(scene, "/Admin/Settings/localization", "Settings", "Localization", "Cultures");
            await page.Locator(".localization-settings-wrapper tbody tr").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".localization-settings-wrapper thead"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".localization-settings-wrapper tbody"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("part-1", async () =>
        {
            await MenuAsync(scene, "/Admin/ContentTypes/List", "Design", "Content Definition", "Content Types");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a[role='btn-edit-Article']"));
        });

        await scene.StepAsync("part-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a:text-is('Add Parts')").First);
            var localization = page.GetByRole(AriaRole.Checkbox, new() { Name = "Localization", Exact = true });
            await RevealAsync(scene, localization);
            await scene.ClickAsync(localization);
            await page.Locator("button:has-text('Add Selected Parts')").WaitForAsync();
            await scene.HighlightAsync(page.Locator("button:has-text('Add Selected Parts')"));
        });

        await scene.StepAsync("part-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            await AtAsync(watch, Math.Max(500, (int)(durations["part-3"] * 1000) - 4600));
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button:has-text('Add Selected Parts')"));
            var part = page.Locator("#parts li").Filter(new() { HasText = "Localization" }).First;
            await part.WaitForAsync();
            await RevealAsync(scene, part);
            await scene.HighlightAsync(part);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task OriginalAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("original-1", async () =>
        {
            await MenuAsync(scene, "/Admin/Contents/ContentItems/Article", "Content", "Content Items", "Article");
            var row = ItemRow(scene, "About");
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("original-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(ItemRow(scene, "About").Locator("a.btn.edit"));
            var toggle = page.Locator("#contentLocalizationToggle");
            await toggle.WaitForAsync();
            await RevealAsync(scene, toggle);
            await scene.ClickAsync(page.Locator("#contentLocalizationToggle button"));
        });

        await scene.StepAsync("original-3", async () =>
        {
            var publish = page.Locator("button[name='submit.Publish'][value='submit.Publish']");
            await scene.ScrollAsync(0, 1500);
            await scene.HighlightAsync(publish);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(publish);
            var row = ItemRow(scene, "About");
            await row.WaitForAsync();
            await scene.HighlightAsync(row);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TranslateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/Article");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("translate-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(ItemRow(scene, "About").Locator("a.btn.edit"));
            var toggle = page.Locator("#contentLocalizationToggle");
            await toggle.WaitForAsync();
            await RevealAsync(scene, toggle);
            await scene.ClickAsync(page.Locator("#contentLocalizationToggle button"));
            await scene.HighlightAsync(page.Locator("#contentLocalizationCard i.fa-plus").First);
            await scene.HighlightAsync(page.Locator("#contentLocalizationCard i.fa-plus").Last);
        });

        await scene.StepAsync("translate-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var link = page.Locator("#contentLocalizationCard a[title='Create French (France)']");
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await link.ClickAsync());
                await page.Locator(".toast").Filter(new() { HasText = "Localized version of the content created successfully." }).First.WaitForAsync(new() { Timeout = 30000 });
            });
        });

        await scene.StepAsync("translate-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ScrollAsync(0, 120);
            await scene.HighlightAsync(Label(scene, "Title"));
            await AtAsync(watch, 2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Subtitle"));
            await AtAsync(watch, 4200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "Permalink"));
        });

        await scene.StepAsync("translate-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Label(scene, "Title"), pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("À propos", new() { Delay = 40 });
            await scene.ClickAsync(Label(scene, "Subtitle"), pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("Voici ce que je fais.", new() { Delay = 30 });
            var toggle = page.Locator("#contentLocalizationToggle");
            await RevealAsync(scene, toggle);
            await scene.ClickAsync(page.Locator("#contentLocalizationToggle button"));
            await scene.ScrollAsync(0, 300);
        });

        await scene.StepAsync("translate-5", async () =>
        {
            var publish = page.Locator("button[name='submit.Publish'][value='submit.Publish']");
            await scene.ScrollAsync(0, 1500);
            await scene.ClickAndWaitForNavigationAsync(publish);
            await ItemRow(scene, "À propos").WaitForAsync();
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/Article");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("manage-1", async () =>
        {
            await scene.ClickAsync(ItemRow(scene, "About").Locator("button.localizations"), pauseAfter: 600);
            await scene.HighlightAsync(ItemRow(scene, "About").Locator(".dropdown-menu.show i.fa-pen-to-square").First);
            await scene.HighlightAsync(ItemRow(scene, "About").Locator(".dropdown-menu.show i.fa-plus").First);
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var link = ItemRow(scene, "About").Locator("a.dropdown-item[title='Create Spanish (Spain)']");
            await scene.MoveToAsync(link);
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(async () => await link.ClickAsync());
                await page.Locator(".toast").Filter(new() { HasText = "Localized version of the content created successfully." }).First.WaitForAsync(new() { Timeout = 30000 });
            });
            await scene.ClickAsync(Label(scene, "Title"), pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("Acerca de", new() { Delay = 30 });
            await scene.ClickAsync(Label(scene, "Subtitle"), pauseAfter: 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("Esto es lo que hago.", new() { Delay = 25 });
            var save = page.Locator("button[name='submit.Save'][value='submit.Save']");
            await scene.ScrollAsync(0, 1500);
            await scene.HighlightAsync(save);
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(save);
        });

        await scene.StepAsync("manage-3", async () =>
        {
            foreach (var title in new[] { "Acerca de", "À propos", "About" })
            {
                var row = ItemRow(scene, title);
                await scene.HighlightAsync(row);
                await scene.PauseAsync(1500);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("manage-4", async () =>
        {
            var picker = page.Locator("div.bootstrap-select:has(select.culture-selectpicker) > button");
            await scene.ClickAsync(picker, pauseAfter: 600);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show a, .dropdown-menu.show li").Filter(new() { HasText = "fr-FR" }).First, pauseAfter: 300);
            await Assertions.Expect(page.Locator("li.list-group-item")).ToHaveCountAsync(2, new() { Timeout = 8000 });
            await scene.PauseAsync(1200);
            await scene.ClickAsync(picker, pauseAfter: 600);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show a, .dropdown-menu.show li").Filter(new() { HasText = "All cultures" }).First, pauseAfter: 300);
            await Assertions.Expect(page.Locator("li.list-group-item")).ToHaveCountAsync(4, new() { Timeout = 8000 });
        });
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/Article");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("site-1", async () =>
        {
            var viewTask = scene.Context.WaitForPageAsync();
            await scene.ClickAsync(ItemRow(scene, "About").Locator("a.btn.view"));
            var site = await viewTask;
            await site.WaitForLoadStateAsync();
            await scene.SwitchToAsync(site);
            await Assertions.Expect(site.Locator("h1")).ToContainTextAsync("About");
            await scene.HighlightAsync(site.Locator("h1").First, "/about");
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/a-propos");
            await Assertions.Expect(scene.Page.Locator("h1")).ToContainTextAsync("À propos");
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "/a-propos");
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task PermissionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("perm-1", async () =>
        {
            await MenuAsync(scene, "/Admin/Roles/Index", "Access Control", "Roles");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a.btn.edit[href$='/Roles/Edit/Author'], a.edit[href$='/Roles/Edit/Author']").First);
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 200);
            await page.Keyboard.TypeAsync("Localize", new() { Delay = 15 });
            await Assertions.Expect(page.Locator("tr").Filter(new() { HasText = "Access admin panel" }).First).ToBeHiddenAsync(new() { Timeout = 20000 });
        });

        await scene.StepAsync("perm-2", async () =>
        {
            foreach (var name in new[] { "Localize content for others", "Localize own content" })
            {
                var row = page.Locator("tr").Filter(new() { HasText = name }).First;
                await scene.HighlightAsync(row);
                await scene.PauseAsync(2500);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("perm-3", async () =>
        {
            foreach (var name in new[] { "Localize own content", "Localize content for others" })
            {
                var row = page.Locator("tr").Filter(new() { HasText = name }).First;
                await scene.HighlightAsync(row.Locator("td").Nth(1));
                await scene.PauseAsync(2500);
                await scene.ClearHighlightsAsync();
            }
        });
    }
}

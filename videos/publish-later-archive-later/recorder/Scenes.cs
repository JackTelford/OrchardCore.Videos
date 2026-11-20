using System.Globalization;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Publish Later and Archive Later": 02-features, 02-parts, 03-publish, 04-archive and 05-cancel. They
/// depend on each other and change the site (see the snapshots of the recording plan). The waits for the background
/// tasks (a few real minutes) are cut off camera. PrepareAsync creates the two seed items of 05-cancel.
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
        ("02-features", FeaturesAsync),
        ("02-parts", PartsAsync),
        ("03-publish", PublishAsync),
        ("04-archive", ArchiveAsync),
        ("05-cancel", CancelAsync),
    ];

    private const string Title = "Spring Sale Announcement";

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

    private static async Task FastAsync(ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 8 });
    }

    private static TimeZoneInfo Pacific()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        }
    }

    // The site's local time (America/Los_Angeles) in the given offset from now, with the seconds set to zero.
    private static string LocalIn(TimeSpan offset, int? hour = null)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow + offset, Pacific());
        local = new DateTime(local.Year, local.Month, local.Day, hour ?? local.Hour, hour is null ? local.Minute : 0, 0);
        return local.ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture);
    }

    private static ILocator Row(IPage page, string title)
        => page.Locator(".list-group-item").Filter(new() { HasText = title }).First;

    private static ILocator Group(IPage page, string field)
        => page.Locator(".action-bar-bottom .input-group").Filter(new() { Has = page.Locator($"input[name$='.{field}']") }).First;

    private static ILocator Menu(IPage page, string text)
        => page.Locator("#left-nav button.item-label, #left-nav a.item-label", new() { HasTextRegex = new($"^\\s*{text}\\s*$") }).First;

    // Moves to the link, then clicks it and waits for the page off camera, so that a slow load is cut from the clip.
    private static async Task OffNavigateAsync(Scene scene, ILocator link)
    {
        await scene.MoveToAsync(link);
        await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
        {
            await link.ClickAsync();
            await scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }));
    }

    private static async Task EnableFeatureAsync(Scene scene, string name)
    {
        var page = scene.Page;
        await FastAsync(page.Locator("#search-box"), name);
        var enable = page.Locator("a.btn-primary", new() { HasText = "Enable" }).Locator("visible=true").First;
        await enable.WaitForAsync();
        await scene.MoveToAsync(enable);
        await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
        {
            await enable.ClickAsync();
            await page.GetByText($"The feature {name} was enabled.").First.WaitForAsync(new() { Timeout = 120_000 });
        }));
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await scene.ClickAsync(Menu(page, "Tools"), pauseAfter: 500);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav a.item-label", new() { HasTextRegex = new(@"^\s*Features\s*$") }).First);
            await EnableFeatureAsync(scene, "Publish Later");
        });

        await scene.StepAsync("features-2", async () =>
        {
            await EnableFeatureAsync(scene, "Archive Later");
        });
    }

    private static async Task PartsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("parts-1", async () =>
        {
            await scene.ClickAsync(Menu(page, "Design"), pauseAfter: 150);
            await scene.ClickAsync(Menu(page, "Content Definition"), pauseAfter: 150);
            await OffNavigateAsync(scene, page.Locator("#left-nav a.item-label", new() { HasTextRegex = new(@"^\s*Content Types\s*$") }).First);
            await OffNavigateAsync(scene, page.Locator("a[role='btn-edit-Article']").First);
        });

        await scene.StepAsync("parts-2", async () =>
        {
            var add = page.Locator("a", new() { HasTextRegex = new(@"^\s*Add Parts\s*$") }).First;
            await add.ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(add);

            foreach (var part in new[] { "ArchiveLater", "PublishLater" })
            {
                var box = page.Locator("li", new() { Has = page.Locator($"label:text-is('{part}')") }).First.Locator("input[type='checkbox']").First;
                await box.ScrollIntoViewIfNeededAsync();
                await scene.ClickAsync(page.Locator($"label:text-is('{part}')").First, pauseAfter: 300);
            }

            await scene.ClickAndWaitForNavigationAsync(page.Locator("#add-selected-parts-button"));
            await scene.PauseAsync(1500);
            await page.Locator("#parts").ScrollIntoViewIfNeededAsync();
        });
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("publish-1", async () =>
        {
            await scene.ClickAsync(Menu(page, "Content"), pauseAfter: 150);
            await OffNavigateAsync(scene, page.Locator("#left-nav a.item-label", new() { HasTextRegex = new(@"^\s*Content Items\s*$") }).First);
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 200);
            await OffNavigateAsync(scene, page.Locator(".dropdown-menu a", new() { HasTextRegex = new(@"^\s*Article\s*$") }).First);
            await FastAsync(page.GetByLabel("Title", new() { Exact = true }).First, Title);
        });

        await scene.StepAsync("publish-2", async () =>
        {
            var group = Group(page, "ScheduledPublishLocalDateTime");
            await scene.HighlightAsync(group);
            var field = page.Locator("input[name$='.ScheduledPublishLocalDateTime']").First;
            await scene.ClickAsync(field, pauseAfter: 300);
            await field.FillAsync(LocalIn(TimeSpan.FromMinutes(3)));
        });

        await scene.StepAsync("publish-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button[name='submit.Save'][value='submit.PublishLater']").First);
        });

        await scene.StepAsync("publish-4", async () =>
        {
            var row = Row(page, Title);
            await scene.HighlightAsync(row.Locator(".badge", new() { HasText = "Draft" }).First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(row.Locator("i.fa-clock").First);
        });

        await scene.StepAsync("publish-5", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                var started = DateTime.UtcNow;

                while (DateTime.UtcNow - started < TimeSpan.FromMinutes(7))
                {
                    var status = await page.EvaluateAsync<int>("async () => (await fetch('/spring-sale-announcement', { cache: 'no-store' })).status");

                    if (status == 200)
                    {
                        break;
                    }

                    await Task.Delay(5000);
                }
            });

            await scene.WaitForNavigationAsync(() => page.ReloadAsync());
            await scene.MoveToAsync(page.Locator("h1, .oc-breadcrumb-title").First);
            await scene.HighlightAsync(Row(page, Title).Locator(".badge", new() { HasText = "Published" }).First);
        });

        await scene.StepAsync("publish-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/spring-sale-announcement");
        });
    }

    private static async Task ArchiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("archive-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a", new() { HasTextRegex = new($"^\\s*{Title}\\s*$") }).First);
            await scene.HighlightAsync(Group(page, "ScheduledArchiveLocalDateTime"));
        });

        await scene.StepAsync("archive-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var field = page.Locator("input[name$='.ScheduledArchiveLocalDateTime']").First;
            await scene.ClickAsync(field, pauseAfter: 300);
            await field.FillAsync(LocalIn(TimeSpan.FromMinutes(3)));
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button[name='submit.Publish'][value='submit.ArchiveLater']").First);
        });

        await scene.StepAsync("archive-3", async () =>
        {
            var row = Row(page, Title);
            await scene.HighlightAsync(row.Locator(".badge", new() { HasText = "Published" }).First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(row.Locator("i.fa-box-archive").First);
        });

        await scene.StepAsync("archive-4", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                var started = DateTime.UtcNow;

                while (DateTime.UtcNow - started < TimeSpan.FromMinutes(7))
                {
                    var status = await page.EvaluateAsync<int>("async () => (await fetch('/spring-sale-announcement', { cache: 'no-store' })).status");

                    if (status == 404)
                    {
                        break;
                    }

                    await Task.Delay(5000);
                }
            });

            await scene.WaitForNavigationAsync(() => page.ReloadAsync());
            await scene.HighlightAsync(Row(page, Title).Locator(".badge", new() { HasText = "Draft" }).First);
        });
    }

    // Creates the two seed items of 05-cancel, outside the capture, with the editor of the Article type.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task CreateAsync(string title, string field, string value, string button)
        {
            await page.GotoAsync($"{BaseUrl}/Admin/Contents/ContentTypes/Article/Create?returnUrl=%2FAdmin%2FContents%2FContentItems", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.GetByLabel("Title", new() { Exact = true }).First.FillAsync(title);
            await page.Locator($"input[name$='.{field}']").First.FillAsync(value);
            await page.RunAndWaitForNavigationAsync(() => page.Locator(button).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        var day30 = TimeSpan.FromDays(30);
        await CreateAsync("Holiday Hours", "ScheduledPublishLocalDateTime", LocalIn(day30, 9), "button[name='submit.Save'][value='submit.PublishLater']");
        await CreateAsync("Summer Campaign", "ScheduledArchiveLocalDateTime", LocalIn(day30, 17), "button[name='submit.Publish'][value='submit.ArchiveLater']");
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task CancelAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cancel-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a", new() { HasTextRegex = new(@"^\s*Holiday Hours\s*$") }).First);
            var cancel = page.Locator("button[value='submit.CancelPublishLater']").First;
            await scene.HighlightAsync(cancel);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await page.Locator("input[name$='.ScheduledPublishLocalDateTime']").First.FillAsync(LocalIn(TimeSpan.FromDays(45), 9));
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button[name='submit.Save'][value='submit.PublishLater']").First);
        });

        await scene.StepAsync("cancel-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a", new() { HasTextRegex = new(@"^\s*Holiday Hours\s*$") }).First);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button[value='submit.CancelPublishLater']").First);
        });

        await scene.StepAsync("cancel-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a", new() { HasTextRegex = new(@"^\s*Summer Campaign\s*$") }).First);
            var cancel = page.Locator("button[value='submit.CancelArchiveLater']").First;
            await scene.HighlightAsync(cancel);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(cancel);
        });
    }
}

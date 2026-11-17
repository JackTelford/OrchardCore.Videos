using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the go-to-production video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-proxy", ProxyAsync),
        ("04-https", HttpsAsync),
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

    private static async Task AtAsync(Stopwatch watch, int milliseconds)
    {
        var remaining = milliseconds - (int)watch.ElapsedMilliseconds;

        if (remaining > 0)
        {
            await Task.Delay(remaining);
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

    // Enables HTTPS and Reverse Proxy Configuration (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();

        foreach (var (filter, id) in new[] { ("https", "OrchardCore_Https"), ("reverse", "OrchardCore_ReverseProxy") })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            await page.Locator("#search-box").FillAsync(filter);
            await page.Locator("#btn-enable-" + id).ClickAsync();
            await page.Locator("#btn-disable-" + id).WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 90000 });
        }

        Console.WriteLine("Enabled HTTPS and Reverse Proxy Configuration.");
        await context.CloseAsync();
    }

    private static ILocator FeatureRow(Scene scene, string id)
        => scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator($"#btn-enable-{id}, #btn-disable-{id}") }).First;

    private static async Task ProxyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        ILocator Box(string header) => page.GetByLabel($"Enables the forwarding of the HTTP header {header}");

        await scene.StepAsync("proxy-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "reverse", delay: 40);
            var row = FeatureRow(scene, "OrchardCore_ReverseProxy");
            await row.WaitForAsync();
            await scene.HighlightAsync(row, "Reverse Proxy Configuration");
        });

        await scene.StepAsync("proxy-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await MenuAsync(scene, "/Admin/Settings/ReverseProxy", "Settings", "Reverse Proxy");
            await page.Locator("input[type=checkbox][name$='.EnableXForwardedFor']").WaitForAsync();
            var watch = Stopwatch.StartNew();
            foreach (var (header, caption) in new[] { ("X-Forwarded-For", "Client address"), ("X-Forwarded-Proto", "Protocol"), ("X-Forwarded-Host", "Host") })
            {
                await scene.HighlightAsync(Box(header), caption);
                await scene.PauseAsync(900);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("proxy-3", async () =>
        {
            var hints = page.Locator("span.hint", new() { HasTextString = "Current request value" });

            for (var i = 0; i < 3; i++)
            {
                await scene.HighlightAsync(hints.Nth(i));
                await scene.PauseAsync(1000);
                await scene.ClearHighlightsAsync();
            }

            foreach (var name in new[] { "EnableXForwardedFor", "EnableXForwardedProto", "EnableXForwardedHost" })
            {
                await scene.ClickAsync(page.Locator($"input[type=checkbox][name$='.{name}']"), pauseAfter: 400);
            }
        });

        await scene.StepAsync("proxy-4", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.HighlightAsync(page.Locator(".alert-warning", new() { HasTextString = "The website might be restarted" }));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            var save = page.Locator("button.save").First;
            await scene.MoveToAsync(save);
            await AtAsync(watch, Math.Max(2500, (int)(durations["proxy-4"] * 1000) - 6500));
            await scene.ClickAndWaitForNavigationAsync(save);
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync(new() { Timeout = 60000 });
        });
    }

    private static async Task HttpsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("https-1", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "https", delay: 40);
            var row = FeatureRow(scene, "OrchardCore_Https");
            await row.WaitForAsync();
            await scene.HighlightAsync(row, "HTTPS");
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await MenuAsync(scene, "/Admin/Settings/Https", "Settings", "Security", "HTTPS");
        });

        await scene.StepAsync("https-2", async () =>
        {
            await scene.HighlightAsync(page.Locator(".form-check").Filter(new() { HasText = "Require HTTPS for all requests" }).First, "Redirects all requests");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByText("For safety, changing HTTPS-only settings over HTTP has been prevented.").First, "Open the page over HTTPS");
        });

        await scene.StepAsync("https-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("select[name$='.StrictTransportSecurityMode']"), "HSTS mode");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".alert-danger").First);
        });

        await scene.StepAsync("https-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".form-check").Filter(new() { HasText = "Require HTTPS Permanent" }).First, "Permanent redirect");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(page.Locator("input[name$='.SslPort']"), "Redirect port");
        });

        await scene.ClearHighlightsAsync();
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Error pages in Orchard Core". The site uses the SaaS recipe (TheTheme has no HttpError view of its
/// own); PrepareAsync enables the Templates feature outside the capture. 04-admin creates a template: restore the
/// snapshot before a new take.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string TemplateText = "<h2 class=\"text-danger\">{{ \"We could not find that page.\" | t }}</h2>";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-builtin", BuiltinAsync),
        ("04-admin", AdminAsync),
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

    // Enables the Templates feature outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.GotoAsync($"{BaseUrl}/Admin/Features", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#search-box").FillAsync("Templates");
        await page.Locator("#btn-enable-OrchardCore_Templates").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(5000);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task ShowAsync(Scene scene, string path, string heading, string selector = "h1")
    {
        await scene.ClearHighlightsAsync();
        await scene.NavigateAsync(path);
        var h = scene.Page.Locator(selector, new() { HasTextString = heading }).First;
        await h.WaitForAsync();
        await scene.HighlightAsync(h, path);
    }

    private static async Task BuiltinAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("builtin-1", () => ShowAsync(scene, "/missing-page", "The page could not be found."));
        await scene.StepAsync("builtin-2", () => ShowAsync(scene, "/Error/403", "You do not have access to this resource."));
        await scene.StepAsync("builtin-3", () => ShowAsync(scene, "/Error/405", "An error occurred while executing this request."));
    }

    private static async Task AdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("admin-1", async () =>
        {
            var design = scene.Page.Locator("#left-nav button.item-label").Filter(new() { HasTextRegex = new("^\\s*Design\\s*$") }).First;

            if (await design.GetAttributeAsync("aria-expanded") != "true")
            {
                await scene.ClickAsync(design, pauseAfter: 400);
            }

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.item-label").Filter(new() { HasTextRegex = new("^\\s*Templates\\s*$") }).First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn.create").First);
            await scene.Page.Locator("#Name").WaitForAsync();
        });

        await scene.StepAsync("admin-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name"), "HttpError__NotFound", 15);
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator("#Content_editor"), "Liquid");
        });

        await scene.StepAsync("admin-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#Content_editor .monaco-editor").First, pauseAfter: 300);
            await scene.Page.Keyboard.InsertTextAsync(TemplateText);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
            await scene.Page.Locator("a", new() { HasTextString = "HttpError__NotFound" }).First.WaitForAsync();
        });

        await scene.StepAsync("admin-4", async () =>
        {
            await scene.NavigateAsync("/missing-page");
            var h = scene.Page.Locator("h2.text-danger", new() { HasTextString = "We could not find that page." }).First;
            await h.WaitForAsync();
            await scene.HighlightAsync(h, "/missing-page");
        });
    }
}

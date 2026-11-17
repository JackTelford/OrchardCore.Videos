using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "A Modular ASP.NET Core MVC Application With Orchard Core". The application is the plain MVC
/// application of the templates (no sign-in, no state.json). 06-run is recorded with the projects as generated,
/// 07-home after the two edits of the plan (a route with an empty pattern, and the view "Hello World").
/// The recorder's browser has no address bar, so each step rings the page with its address as the caption.
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
        ("06-run", RunAsync),
        ("07-home", HomeAsync),
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

    // Wraps the module's text in an element, with room above and below for the caption of the ring.
    private static Task WrapModuleTextAsync(Scene scene) => scene.Page.EvaluateAsync(@"() => {
        const container = document.querySelector('div.body-content');
        const text = [...container.childNodes].find(n => n.nodeType === 3 && n.textContent.includes('Hello from OrchardCore.Mvc.HelloWorld'));
        const span = document.createElement('span');
        span.id = 'module-text';
        span.style.display = 'inline-block';
        span.style.margin = '40px 0 64px';
        container.insertBefore(span, text);
        span.appendChild(text);
    }");

    private static Task SpaceHeadingAsync(Scene scene) => scene.Page.EvaluateAsync(@"() => {
        const heading = document.querySelector('div.body-content h1');
        heading.style.display = 'inline-block';
        heading.style.marginBottom = '64px';
    }");

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-2", async () =>
        {
            await scene.NavigateAsync("/OrchardCore.Mvc.HelloWorld/Home/Index");
            await WrapModuleTextAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("#module-text"), "/OrchardCore.Mvc.HelloWorld/Home/Index");
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".navbar, nav, body > div").First, "Layout of the web application");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#module-text"), "View of the module");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("footer").First, "Layout of the web application");
        });
    }

    private static async Task HomeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/OrchardCore.Mvc.HelloWorld/Home/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("home-1", async () =>
        {
            await scene.NavigateAsync("/");
            await SpaceHeadingAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("div.body-content h1").First, "/");
        });

        await scene.StepAsync("home-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/OrchardCore.Mvc.HelloWorld/Home/Index");
            await SpaceHeadingAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("div.body-content h1").First, "/OrchardCore.Mvc.HelloWorld/Home/Index");
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Import: Package and JSON Import". The package maple-street-package.zip (a Recipe.json at its root,
/// with one Article, Spring Menu) is made outside the repository; VIDEO_PACKAGE_DIR points at its folder.
/// Both clips change the site: restore the snapshot before a new take.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static string PackagePath => Path.Combine(Environment.GetEnvironmentVariable("VIDEO_PACKAGE_DIR") ?? "ocvideo-seed", "maple-street-package.zip");

    private const string RecipeText = "{\n  \"steps\": [\n    {\n      \"name\": \"Settings\",\n      \"SiteName\": \"Maple Street Bakery\"\n    }\n  ]\n}";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-package", PackageAsync),
        ("03-json", JsonAsync),
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

    private static async Task OpenGroupAsync(Scene scene, string group)
    {
        var button = scene.Page.Locator("#left-nav button.nav-group-label, #left-nav button.item-label").Filter(new() { HasTextRegex = new($"^\\s*{group}\\s*$") }).First;

        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(button, pauseAfter: 400);
        }
    }

    private static async Task GoAsync(Scene scene, string text)
        => await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = new($"^\\s*{text}\\s*$") }).First);

    private static async Task PackageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("package-1", async () =>
        {
            await OpenGroupAsync(scene, "Content");
            await GoAsync(scene, "Content Items");
            await scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Manage Content" }).WaitForAsync();
        });

        await scene.StepAsync("package-2", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await OpenGroupAsync(scene, "Deployments");
            await GoAsync(scene, "Package Import");
            await scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Import Deployment Package" }).WaitForAsync();
        });

        await scene.StepAsync("package-3", async () =>
        {
            var field = scene.Page.GetByLabel("Select a file");
            await scene.MoveToAsync(field);
            await field.SetInputFilesAsync(PackagePath);
        });

        await scene.StepAsync("package-4", async () =>
        {
            // The page after an import is slow: the wait is cut off camera, so the video jumps to the page with its message.
            var import = scene.Page.Locator("button.import");
            await scene.MoveToAsync(import);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await import.ClickAsync();
                await scene.Page.Locator(".toast.message-success").First.WaitForAsync(new() { Timeout = 60000 });
            }));
        });

        await scene.StepAsync("package-5", async () =>
        {
            await OpenGroupAsync(scene, "Content");
            await GoAsync(scene, "Content Items");
            await scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Manage Content" }).WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { Has = scene.Page.Locator(".contentitem a:text-is('Spring Menu')") }).First);
        });
    }

    private static async Task JsonAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("json-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools");
            await OpenGroupAsync(scene, "Deployments");
            await GoAsync(scene, "JSON Import");
            await scene.Page.Locator(".CodeMirror").First.WaitForAsync();
        });

        await scene.StepAsync("json-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".CodeMirror").First, pauseAfter: 300);
            await scene.Page.EvaluateAsync("text => document.querySelector('.CodeMirror').CodeMirror.setValue(text)", RecipeText);
        });

        await scene.StepAsync("json-3", async () =>
        {
            // The page after an import is slow: the wait is cut off camera, so the video jumps to the page with its message.
            var import = scene.Page.Locator("button.import");
            await scene.MoveToAsync(import);
            await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
            {
                await import.ClickAsync();
                await scene.Page.Locator(".toast.message-success").First.WaitForAsync(new() { Timeout = 60000 });
            }));
        });

        await scene.StepAsync("json-4", async () =>
        {
            await OpenGroupAsync(scene, "Settings");
            await GoAsync(scene, "General");
            await scene.HighlightAsync(scene.Page.Locator("#ISite_SiteName"));
        });
    }
}

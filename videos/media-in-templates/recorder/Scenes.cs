using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-profile", ProfileAsync),
        ("06-result", ResultAsync),
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

    // Imports seed.json (the thumbnail profile, the renditions template and the Renditions page) through Tools > Deployments > JSON Import.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator(".CodeMirror").First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
        var text = await File.ReadAllTextAsync(Path.Combine(Root, "seed.json"));
        await page.EvaluateAsync("text => document.querySelector('.CodeMirror').CodeMirror.setValue(text)", text);
        await Task.Delay(500);
        await page.Locator("button:has-text(\"Import\")").First.ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("seed imported");

        // Request the page once so that the five images are resized and cached.
        await page.GotoAsync(BaseUrl + "/renditions", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForFunctionAsync("() => { const i = [...document.querySelectorAll('.renditions img')]; return i.length === 5 && i.every(x => x.complete && x.naturalWidth > 0); }");
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task ProfileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/MediaProfiles");
        await page.Locator("a[href*='Edit?name=thumbnail']:not(.btn)").First.WaitForAsync();
        await scene.StartCaptureAsync();

        var banner = page.Locator("a[href*='Edit?name=banner']:not(.btn)").First;
        var thumbnail = page.Locator("a[href*='Edit?name=thumbnail']:not(.btn)").First;

        await scene.StepAsync("profile-1", async () =>
        {
            await scene.HighlightAsync(banner, "banner");
            await scene.HoldAsync(1.5);
            await scene.HighlightAsync(thumbnail, "thumbnail");
        });

        await scene.StepAsync("profile-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(thumbnail);
            var (x, y) = await scene.CenterOfAsync(thumbnail);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 300);
            await page.Locator("#SelectedWidth").WaitForAsync();
            await scene.HighlightAsync(page.Locator("#SelectedWidth"), "160");
            await scene.HoldAsync(0.7);
            await scene.HighlightAsync(page.Locator("#SelectedHeight"), "160");
            await scene.HoldAsync(0.7);
            await scene.HighlightAsync(page.Locator("#SelectedMode"), "Crop");
        });

        await scene.StepAsync("profile-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#Name"), "thumbnail");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/renditions");
        await page.WaitForFunctionAsync("() => { const i = [...document.querySelectorAll('.renditions img')]; return i.length === 5 && i.every(x => x.complete && x.naturalWidth > 0); }");
        await scene.StartCaptureAsync();

        ILocator Figure(int n, string part) => page.Locator($".renditions figure:nth-child({n}) {part}");

        await scene.StepAsync("result-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".renditions"), "home-bg.jpg");
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Figure(1, "code"));
        });

        await scene.StepAsync("result-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            string[] captions = ["max", "crop", "pad", "stretch"];

            for (var n = 1; n <= 4; n++)
            {
                await scene.HighlightAsync(Figure(n, "img"), captions[n - 1]);

                if (n < 4)
                {
                    await scene.HoldAsync(1.8);
                    await scene.ClearHighlightsAsync();
                }
            }
        });

        await scene.StepAsync("result-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Figure(5, "img"), "thumbnail");
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Figure(1, "code"), "200");
        });

        await scene.ClearHighlightsAsync();
    }
}

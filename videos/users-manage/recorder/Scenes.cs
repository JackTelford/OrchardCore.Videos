using Microsoft.Playwright;

namespace Recorder;

/// <summary>Shared plumbing of the clips (see Scenes.Clips.cs for the clips of this video).</summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

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

    // Opens a signed-in page outside the capture, for seeding.
    private static async Task<(IBrowserContext Context, IPage Page)> SeedPageAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        return (context, await context.NewPageAsync());
    }

    // Imports a recipe through Tools > Deployments > JSON Import.
    private static async Task ImportRecipeAsync(IPage page, string json)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror'); if (cm && cm.CodeMirror) { cm.CodeMirror.setValue(json); cm.CodeMirror.save(); } else { document.querySelector('#Json').value = json; } }", json);
        await page.Locator("button.import").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var text = await page.Locator("body").InnerTextAsync();
        if (text.Contains("rror", StringComparison.Ordinal) && !text.Contains("imported successfully"))
        {
            Console.WriteLine("Recipe import said: " + text[..Math.Min(400, text.Length)]);
        }
    }

    // Enables features through the Features page.
    private static async Task EnableFeaturesAsync(IPage page, params string[] ids)
    {
        var features = string.Join(",", ids.Select(id => $"\"{id}\""));
        await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"feature\",\"enable\":[" + features + "],\"disable\":[]}]}");
    }
}

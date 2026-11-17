using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clip of the localization-javascript video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("05-admin", AdminAsync),
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

    private const string FeaturesRecipe = """
{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.Localization", "OrchardCore.Cors" ] } ] }
""";

    private const string CulturesRecipe = """
{ "steps": [ { "name": "settings", "LocalizationSettings": { "DefaultCulture": "en-US", "SupportedCultures": [ "en-US", "fr" ] } } ] }
""";

    private const string CorsRecipe = """
{
  "steps": [
    {
      "name": "Settings",
      "CorsSettings": {
        "Policies": [
          { "Name": "Public site", "IsDefaultPolicy": true, "AllowAnyOrigin": false, "AllowedOrigins": [], "AllowAnyHeader": false, "AllowedHeaders": [], "AllowAnyMethod": false, "AllowedMethods": [], "AllowCredentials": false, "ExposedHeaders": [] },
          { "Name": "Partner portal", "IsDefaultPolicy": false, "AllowAnyOrigin": false, "AllowedOrigins": [], "AllowAnyHeader": false, "AllowedHeaders": [], "AllowAnyMethod": false, "AllowedMethods": [], "AllowCredentials": false, "ExposedHeaders": [] }
        ]
      }
    }
  ]
}
""";

    private static async Task ImportAsync(IPage page, string recipe)
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            try
            {
                await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
                await page.Locator(".CodeMirror").First.WaitForAsync(new() { Timeout = 5000 });
                break;
            }
            catch (Exception)
            {
                await Task.Delay(3000);
            }
        }

        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", recipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(4000);
    }

    // Enables Localization and CORS, supports en-US and fr, and creates the two policies (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await ImportAsync(page, FeaturesRecipe);
        await Task.Delay(8000);
        await ImportAsync(page, CulturesRecipe);
        await Task.Delay(6000);
        await ImportAsync(page, CorsRecipe);
        Console.WriteLine("Prepared Localization, CORS, the cultures and the policies.");
        await context.CloseAsync();
    }

    private static async Task AdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Cors");
        var page = scene.Page;
        await page.Locator("#corsAdmin li.list-group-item").First.WaitForAsync(new() { Timeout = 60000 });
        await scene.StartCaptureAsync();

        await scene.StepAsync("admin-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("#corsAdmin #search-box"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#corsAdmin button.btn-secondary").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#corsAdmin span.badge").First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("admin-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Cors?culture=fr");
            await page.Locator("#corsAdmin #search-box[placeholder='Rechercher']").WaitForAsync(new() { Timeout = 60000 });
            await page.Locator("#corsAdmin li.list-group-item").First.WaitForAsync();
        });

        await scene.StepAsync("admin-3", async () =>
        {
            await scene.ClickAsync(page.Locator("#corsAdmin li.list-group-item button.btn-primary").First, pauseAfter: 300);
            var title = page.Locator("#corsAdmin h5.card-title", new() { HasTextString = "Détails" });
            await title.WaitForAsync(new() { Timeout = 30000 });
            await scene.HighlightAsync(page.Locator("#corsAdmin h5.card-title").First);
        });
    }
}

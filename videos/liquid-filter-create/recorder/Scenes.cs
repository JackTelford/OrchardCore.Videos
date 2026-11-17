using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Write a Liquid Filter": 05-template writes the template Content__Slug__about that uses the filters
/// shout and with_site_name of the module Video Liquid Filters. PrepareAsync (outside the capture) enables the module
/// (built into the site) with a Feature step of JSON Import.
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
        ("05-template", TemplateAsync),
    ];

    private const string EnableModule = """
        { "steps": [ { "name": "feature", "enable": [ "Videos.LiquidFilters" ], "disable": [] } ] }
        """;

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

    // Enables the module with JSON Import, outside the capture. The page's code editor replaces the value of the
    // textarea on submit, so the form is posted by script.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        await page.GotoAsync($"{BaseUrl}/Admin/DeploymentPlan/Import/Json", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var result = await page.EvaluateAsync<string>("""
            async json => {
              const form = document.querySelector('form:has(#Json)');
              document.getElementById('Json').value = json;
              const response = await fetch(form.action, { method: 'POST', body: new FormData(form) });
              return (await response.text()).includes('Recipe imported successfully') ? 'ok' : 'failed';
            }
            """, EnableModule);
        Console.WriteLine($"Enable: {result}");
        await Task.Delay(8000);
        await page.GotoAsync($"{BaseUrl}/Admin/Templates", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task TemplateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Templates");
        await scene.StartCaptureAsync();

        const string Part1 = "{{ Model.ContentItem.DisplayText | shout }}";
        const string Whole = Part1 + " / {{ Model.ContentItem.DisplayText | with_site_name: \" - \" }}";

        await scene.StepAsync("template-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a.btn.create").First);
            await page.Locator("#Content_editor .monaco-editor").First.WaitForAsync();
            await page.Locator("#Name").FocusAsync();
            await page.Locator("#Name").PressSequentiallyAsync("Content__Slug__about", new() { Delay = 8 });
        });

        await scene.StepAsync("template-2", async () =>
        {
            await scene.ClickAsync(page.Locator("#Content_editor").First, pauseAfter: 300);
            await page.Keyboard.TypeAsync("{{ Model.ContentItem.DisplayText | ", new KeyboardTypeOptions { Delay = 45 });
            await page.Locator(".suggest-widget.visible").First.WaitForAsync();
            await scene.PauseAsync(2500);
            await page.Keyboard.TypeAsync("sh", new KeyboardTypeOptions { Delay = 120 });
            await scene.PauseAsync(1200);
            await page.Keyboard.TypeAsync("out }}", new KeyboardTypeOptions { Delay = 90 });

            if (await page.Locator(".suggest-widget.visible").CountAsync() > 0)
            {
                await page.Keyboard.PressAsync("Escape");
            }

            await page.WaitForFunctionAsync("v => document.querySelector('textarea.template-editor-value').value === v", Part1);
        });

        await scene.StepAsync("template-3", async () =>
        {
            await page.Keyboard.TypeAsync(" / {{ Model.ContentItem.DisplayText | with_site_name: \" - \" }}", new KeyboardTypeOptions { Delay = 45 });

            if (await page.Locator(".suggest-widget.visible").CountAsync() > 0)
            {
                await page.Keyboard.PressAsync("Escape");
            }

            await page.WaitForFunctionAsync("v => document.querySelector('textarea.template-editor-value').value === v", Whole);
        });

        await scene.StepAsync("template-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save[value='Save']").First);
            await scene.PauseAsync(1500);
            await scene.NavigateAsync("/about");
            await page.GetByText("ABOUT / About - Orchard Core").First.WaitForAsync();
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Debugging settings in Orchard Core". The clips depend on each other and change one setting:
/// 02-enable turns "Write shape debug information" on, 03-read reads the comments in the source of the home page
/// (the document is turned into a text view of its own HTML), 04-disable turns the option off.
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
        ("02-enable", EnableAsync),
        ("03-read", ReadAsync),
        ("04-disable", DisableAsync),
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

    private const string Box = "input[type='checkbox'][name$='WriteShapeDebugInformation']";

    private static async Task ShowSourceAsync(Scene scene)
    {
        await scene.Page.EvaluateAsync("""
            async () => {
              const text = await (await fetch(location.href, { credentials: 'omit' })).text();
              const escaped = text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
              const marked = escaped.replace(/&lt;!--shape-(?:start|end)\b[\s\S]*?--&gt;/g, m => '<span class="c">' + m + '</span>');
              document.head.innerHTML = '<meta charset="utf-8"><style>html,body{margin:0;background:#fff;color:#1f1f23}'
                + 'pre{margin:0;padding:16px 20px;font:15px/1.5 Consolas,"Courier New",monospace;white-space:pre-wrap;word-break:break-all}'
                + '.c{background:#d3f0de;border-radius:3px}</style>';
              document.body.className = '';
              document.body.removeAttribute('style');
              document.body.innerHTML = '<pre id="source"></pre>';
              document.getElementById('source').innerHTML = marked;
              window.scrollTo(0, 0);
            }
            """);
        await scene.Page.Locator("#source").WaitForAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var settings = scene.Page.Locator("#left-nav button.item-label").Filter(new() { HasTextRegex = new("^\\s*Settings\\s*$") }).First;

        if (await settings.GetAttributeAsync("aria-expanded") == "true")
        {
            await settings.ClickAsync();
            await Task.Delay(600);
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(settings, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.item-label[href$='/Admin/Settings/debugging']").First);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator($"div.form-check:has({Box})").First);
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("span.hint:has-text('Wrap rendered shapes')").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(Box), pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        });
    }

    private static async Task ReadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("read-1", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.PauseAsync(800);
            await scene.ScrollAsync(0, -500);
        });

        await scene.StepAsync("read-2", async () => await ShowSourceAsync(scene));

        await scene.StepAsync("read-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#source .c:has-text('shape-start')").First, "Start comment");
        });

        await scene.StepAsync("read-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#source .c:has-text('shape-start')").Nth(0));
            await scene.HighlightAsync(scene.Page.Locator("#source .c:has-text('shape-start')").Nth(1));
            await scene.PauseAsync(6000);
            await scene.ClearHighlightsAsync();
            var end = scene.Page.Locator("#source .c:has-text('shape-end')").First;
            await end.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(end, "End comment");
        });
    }

    private static async Task DisableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/debugging");
        await scene.StartCaptureAsync();

        await scene.StepAsync("off-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator($"div.form-check:has({Box})").First);
            await scene.PauseAsync(5500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(Box), pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
            await scene.Page.Locator(".toast.message-success").First.WaitForAsync();
        });

        await scene.StepAsync("off-2", async () =>
        {
            await scene.NavigateAsync("/");
            await ShowSourceAsync(scene);
        });
    }
}

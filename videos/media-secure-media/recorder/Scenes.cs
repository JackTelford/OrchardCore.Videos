using System.Text.Json;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // A file that never exists: the browser context then starts without a sign-in (a visitor).
    public static string VisitorStatePath => Path.Combine(Root, "state-visitor-never-created.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-visitor-before", VisitorBeforeAsync),
        ("02-enable", EnableAsync),
        ("02-visitor-after", VisitorAfterAsync),
        ("04-anonymous", AnonymousAsync),
        ("04-visitor", VisitorAsync),
        ("05-editor", EditorAsync),
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

    // Creates the two folders of the plan in the root of the Media Library, outside the capture (before Secure Media is enabled).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/Media", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(1500);

        foreach (var name in new[] { "press", "contracts" })
        {
            var result = await page.EvaluateAsync<string>(
                @"async (name) => {
                    const token = document.querySelector('input[name=""__RequestVerificationToken""]')?.value
                        ?? (window.bloom && window.bloom.antiForgeryToken) ?? '';
                    const response = await fetch('/api/media/CreateFolder?path=&name=' + encodeURIComponent(name), {
                        method: 'POST',
                        headers: { 'RequestVerificationToken': token },
                        credentials: 'same-origin',
                    });
                    return response.status + ' ' + (await response.text()).slice(0, 200);
                }",
                name);
            Console.WriteLine($"  create folder {name}: {result}");
        }

        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static Task<Scene> NewVisitorSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, VisitorStatePath, Path.GetDirectoryName(directory), durations);

    // Moves to the element, then clicks it with the page load cut off camera.
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 400);
    }

    // Points at Save, then clicks it late in the step, so that the toast of the result is on screen at the end of the narration.
    private static async Task SaveLateAsync(Scene scene, IReadOnlyDictionary<string, double> durations, string step)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var save = scene.Page.Locator("button.save");
        await scene.MoveToAsync(save);
        var (x, y) = await scene.CenterOfAsync(save);
        await scene.HoldAsync(Math.Max(0, durations[step] - 1.0 - clock.Elapsed.TotalSeconds));
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), 100);
        await scene.Page.GetByText("Role updated successfully.").First.WaitForAsync();
    }

    private static async Task TypeSearchAsync(Scene scene, string text)
    {
        var box = scene.Page.Locator("#search-box");
        await box.WaitForAsync();
        await scene.PauseAsync(600);
        await scene.TypeAsync(box, text, delay: 3);
        await scene.PauseAsync(500);

        if (await box.InputValueAsync() != text)
        {
            await box.FillAsync(text);
            await scene.PauseAsync(500);
        }
    }

    private static async Task VisitorStepAsync(Scene scene, string id)
    {
        await scene.StepAsync(id, async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("header.masthead"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task VisitorBeforeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewVisitorSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("header.masthead").WaitForAsync();
        await scene.StartCaptureAsync();
        await VisitorStepAsync(scene, "before-1");
    }

    private static async Task VisitorAfterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewVisitorSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("header.masthead").WaitForAsync();
        await scene.StartCaptureAsync();
        await VisitorStepAsync(scene, "after-1");
    }

    private static async Task VisitorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewVisitorSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/");
        await scene.Page.Locator("header.masthead").WaitForAsync();
        await scene.StartCaptureAsync();
        await VisitorStepAsync(scene, "visitor-1");
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        var enableButton = page.Locator("#btn-enable-OrchardCore_Media_Security");
        var row = page.Locator(".list-group-item:visible").Filter(new() { Has = enableButton }).Last;

        await scene.StepAsync("enable-1", async () =>
        {
            await TypeSearchAsync(scene, "secure media");
            await enableButton.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            await scene.HighlightAsync(row);
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(enableButton);
            var (x, y) = await scene.CenterOfAsync(enableButton);
            // Click late, so that the toast of the result is still on screen while the next step names it.
            await scene.HoldAsync(Math.Max(0, durations["enable-2"] - 0.9 - clock.Elapsed.TotalSeconds));
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Mouse.ClickAsync((float)x, (float)y)), 100);
            await page.GetByText("The feature Secure Media was enabled.").First.WaitForAsync();
        });

        await scene.StepAsync("enable-3", async () =>
        {
            var success = page.GetByText("The feature Secure Media was enabled.").First;
            var warning = page.GetByText("Review and update role").First;
            await scene.HighlightAsync(success);
            await scene.HighlightAsync(warning);
            await scene.MoveToAsync(page.Locator("a:has-text(\"Roles page\")").First);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(warning);
        });

        await scene.StepAsync("enable-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TypeSearchAsync(scene, "secure media");
            await page.Locator("#btn-disable-OrchardCore_Media_Security").WaitForAsync();
            var badge = page.Locator(".list-group-item:visible").Filter(new() { Has = page.Locator("#btn-disable-OrchardCore_Media_Security") }).Last.Locator("span.badge:has-text(\"Enabled\")").First;
            await scene.HighlightAsync(badge);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AnonymousAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Roles/Index");
        await page.Locator("a:text-is(\"Anonymous\")").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("anon-1", async () =>
        {
            var link = page.Locator("a:text-is(\"Anonymous\")").First;
            await scene.MoveToAsync(link);
            await scene.HighlightAsync(link);
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, link);
            await page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("anon-2", async () =>
        {
            await TypeSearchAsync(scene, "media content");
            var rootRow = page.Locator("tr[data-text=\"View media content in the root folder\"]");
            await rootRow.WaitForAsync();
            await scene.ClickAsync(page.Locator("input[name='Checkbox.ViewRootMediaContent']"));
        });

        await scene.StepAsync("anon-3", async () =>
        {
            await SaveLateAsync(scene, durations, "anon-3");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EditorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Roles/Index");
        await page.Locator("a:text-is(\"Editor\")").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("editor-1", async () =>
        {
            var link = page.Locator("a:text-is(\"Editor\")").First;
            await scene.MoveToAsync(link);
            await scene.HighlightAsync(link);
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, link);
            await page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("editor-2", async () =>
        {
            await TypeSearchAsync(scene, "media content");
            var press = page.Locator("tr[data-text=\"View media content in folder 'press'\"]");
            var contracts = page.Locator("tr[data-text=\"View media content in folder 'contracts'\"]");
            await press.WaitForAsync();
            await scene.HighlightAsync(press);
            await scene.HighlightAsync(contracts);
        });

        await scene.StepAsync("editor-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("input[name='Checkbox.ViewMediaContent_press']"));
        });

        await scene.StepAsync("editor-4", async () =>
        {
            await SaveLateAsync(scene, durations, "editor-4");
        });

        await scene.StepAsync("editor-5", async () =>
        {
            await GoAsync(scene, page.Locator("a:text-is(\"Editor\")").First);
            await TypeSearchAsync(scene, "media content");
            await page.Locator("input[name='Effective.ViewMediaContent_press']").WaitForAsync();
            await scene.HighlightAsync(page.Locator("input[name='Effective.ViewMediaContent_press']"));
            await scene.HighlightAsync(page.Locator("input[name='Effective.ViewMediaContent_contracts']"));
        });

        await scene.ClearHighlightsAsync();
    }
}

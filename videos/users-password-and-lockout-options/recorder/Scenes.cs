using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

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

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("05-rules", RulesAsync),
        ("06-signin", SignInAsync),
        ("06-unlock", UnlockAsync),
    ];

    private static async Task RulesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.OpenAsync("/Admin");
        await page.Locator("#left-nav").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("rules-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#left-nav button.nav-group-label, #left-nav a").Filter(new() { HasTextString = "Access Control" }).First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav a[href$='/Admin/Users/Index']").First);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a.create").Filter(new() { HasTextString = "Add User" }).First);
            await scene.TypeAsync(page.Locator("[name='User.UserName']"), "jamie", delay: 3);
            await scene.TypeAsync(page.Locator("[name='User.Email']"), "jamie@example.com", delay: 3);
            await scene.TypeAsync(page.Locator("[name='User.Password']"), "jam", delay: 3);
            await scene.TypeAsync(page.Locator("[name='User.PasswordConfirmation']"), "jam", delay: 3);
            await scene.ClickAsync(page.Locator(".password-toggle-button").First);
        });

        await scene.StepAsync("rules-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First);
            await page.Locator(".validation-summary-errors").First.WaitForAsync();
        });

        await scene.StepAsync("rules-3", async () =>
        {
            await scene.ClickAsync(page.Locator(".password-generator-button").First);
            await scene.ClickAsync(page.Locator(".password-toggle-button").First);
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First);
            await page.Locator(".toast-body").First.WaitForAsync();
        });
    }

    private static async Task SignInAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "signed-out.json"), Path.GetDirectoryName(directory), durations);
        var page = scene.Page;

        await scene.OpenAsync("/Login");
        await page.Locator("[name='LoginForm.UserName']").WaitForAsync();
        await scene.StartCaptureAsync();

        async Task AttemptAsync()
        {
            await page.Locator("[name='LoginForm.UserName']").FillAsync("jamie");
            await scene.TypeAsync(page.Locator("[name='LoginForm.Password']"), "jamie", delay: 3);
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Button, new() { Name = "Log in" }));
        }

        await scene.StepAsync("lock-1", async () =>
        {
            await scene.TypeAsync(page.Locator("[name='LoginForm.UserName']"), "jamie", delay: 3);
            await scene.TypeAsync(page.Locator("[name='LoginForm.Password']"), "jamie", delay: 3);
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Button, new() { Name = "Log in" }));
            await page.GetByText("Invalid login attempt.").First.WaitForAsync();
        });

        await scene.StepAsync("lock-2", async () =>
        {
            for (var i = 0; i < 8; i++)
            {
                if (await page.GetByText("The account is locked out").CountAsync() > 0)
                {
                    break;
                }

                await AttemptAsync();
            }
        });
    }

    private static async Task UnlockAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;

        await scene.OpenAsync("/Admin/Users/Index");
        var row = page.Locator(".list-group-item").Filter(new() { HasTextString = "jamie" }).First;
        await row.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("unlock-1", async () =>
        {
            await scene.HighlightAsync(row.Locator(".badge", new() { HasTextString = "Locked" }).First);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(row.Locator("button.actions, button:has-text('Actions'), .btn.dropdown-toggle").First);
        });

        await scene.StepAsync("unlock-2", async () =>
        {
            await scene.ClickAsync(page.GetByText("Unlock", new() { Exact = true }).First);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#modalOkButton"));
            await page.Locator(".toast-body").First.WaitForAsync();
        });
    }
}

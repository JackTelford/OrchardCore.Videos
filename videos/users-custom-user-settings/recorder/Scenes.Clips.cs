using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-section", SectionAsync),
        ("04-user", UserAsync),
        ("05-permissions", PermissionsAsync),
        ("06-deploy", DeployAsync),
    ];

    private const string UserPassword = "Start#1234";

    // Seeds jamie, priya (Editor) and the empty deployment plan Team profiles, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"command\",\"Commands\":[\"createUser /UserName:jamie /Password:" + UserPassword + " /Email:jamie@example.com\",\"createUser /UserName:priya /Password:" + UserPassword + " /Email:priya@example.com /Roles:Editor\"]},{\"name\":\"deployment\",\"Plans\":[{\"Name\":\"Team profiles\",\"Steps\":[]}]}]}");
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Index", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Plans: " + (await page.Locator("body").InnerTextAsync()).Contains("Team profiles"));
        await page.GotoAsync(BaseUrl + "/Admin/Users/Index", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Users: " + string.Join(" | ", await page.Locator("li.list-group-item").EvaluateAllAsync<string[]>("els => els.map(e => e.innerText.replace(/\\s+/g,' ').slice(0,70))")));
        await context.CloseAsync();
    }

    // Opens the groups of the admin menu in turn, then follows the last link.
    private static async Task MenuAsync(Scene scene, params string[] path)
    {
        for (var i = 0; i < path.Length - 1; i++)
        {
            var group = scene.Page.Locator($"#left-nav button.item-label:has(span.title:text-is(\"{path[i]}\"))");
            if (await group.GetAttributeAsync("aria-expanded") != "true")
            {
                await FastClickAsync(scene, group, 250);
            }
        }

        await FastGoAsync(scene, scene.Page.Locator($"#left-nav a.item-label:has(span.title:text-is(\"{path[^1]}\"))"));
    }

    private static ILocator RowOf(IPage page, string text) => page.Locator("li.list-group-item, tr").Filter(new() { HasText = text });

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("enable-1", async () =>
        {
            await MenuAsync(scene, "Tools", "Features");
            await scene.TypeAsync(page.Locator("#search-box"), "Custom User Settings", 30);
            await scene.HighlightAsync(page.Locator(".list-group-item:has(#btn-enable-OrchardCore_Users_CustomUserSettings) .direct-dependencies"), "Dependencies");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var enable = page.Locator("#btn-enable-OrchardCore_Users_CustomUserSettings");
            await scene.MoveToAsync(enable);
            await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(enable));
        });
    }

    private static async Task SectionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("section-1", async () =>
        {
            await MenuAsync(scene, "Design", "Content Definition", "Content Types");
            await FastGoAsync(scene, page.Locator("#btnCreate"));
        });

        await scene.StepAsync("section-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator("input[data-displayname]"), "User Profile", 5);
            await page.Keyboard.PressAsync("Tab");
            await FastGoAsync(scene, page.Locator("button.create"));
        });

        await scene.StepAsync("section-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("form ul.list-group").First, "Parts");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("a.cancel"));
        });

        foreach (var (step, name, label) in new[] { ("section-4", "Nickname", "Text Field"), ("section-5", "Newsletter", "Boolean Field") })
        {
            await scene.StepAsync(step, async () =>
            {
                await FastGoAsync(scene, page.Locator("a:text-is(\"Add Field\")"));
                await FastTypeAsync(scene, page.Locator("input[data-displayname]"), name, 5);
                await FastClickAsync(scene, page.Locator($"label:text-is(\"{label}\")"), 150);
                await FastGoAsync(scene, page.Locator("button.save").First);
                await page.EvaluateAsync("() => window.scrollTo(0, 560)");
            });
        }

        await scene.StepAsync("section-6", async () =>
        {
            foreach (var box in new[] { "Creatable", "Listable", "Draftable", "Versionable", "Securable" })
            {
                await FastClickAsync(scene, page.Locator($"label:text-is(\"{box}\")").First, 100);
            }

            await FastGoAsync(scene, page.Locator("button.save:text-is(\"Save\")").First);
            await RowOf(page, "UserProfile").Last.ScrollIntoViewIfNeededAsync();
        });

        await scene.StepAsync("section-7", async () =>
        {
            await FastGoAsync(scene, RowOf(page, "UserProfile").Locator("a:text-is(\"Edit\")").First);
            await FastTypeAsync(scene, page.Locator("input[list=\"stereotypeOptions\"]"), "CustomUserSettings", 10);
            await FastGoAsync(scene, page.Locator("button.save:text-is(\"Save\")").First);
        });

        await scene.StepAsync("section-8", async () =>
            await scene.HighlightAsync(page.Locator(".badge:has-text(\"CustomUserSettings\")").First, "Stereotype"));
    }

    private static async Task UserAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var tab = page.Locator("a[role=\"tab\"]:text-is(\"User Profile\")");

        await scene.StepAsync("user-1", async () =>
        {
            await MenuAsync(scene, "Access Control", "Users");
            await FastGoAsync(scene, RowOf(page, "jamie@example.com").Locator("a:text-is(\"Edit\")").First);
            await PointAsync(scene, tab, "User Profile");
        });

        await scene.StepAsync("user-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, tab);
            await scene.HighlightAsync(page.Locator(".field-wrapper-user-profile-nickname"), "Text Field");
            await scene.HighlightAsync(page.Locator(".field-wrapper-user-profile-newsletter"), "Boolean Field");
        });

        await scene.StepAsync("user-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, page.Locator(".field-wrapper-user-profile-nickname input.form-control"), "Jamie", 10);
            await FastClickAsync(scene, page.Locator(".field-wrapper-user-profile-newsletter label.form-check-label"));
            await FastGoAsync(scene, page.Locator("button.save:text-is(\"Save\")").First);
        });

        await scene.StepAsync("user-4", async () =>
        {
            await FastGoAsync(scene, RowOf(page, "jamie@example.com").Locator("a:text-is(\"Edit\")").First);
            await FastClickAsync(scene, tab);
        });

        await scene.StepAsync("user-5", async () =>
        {
            await FastGoAsync(scene, page.Locator(".breadcrumb a:text-is(\"Users\")"));
            await FastGoAsync(scene, page.Locator("a:text-is(\"Add User\")"));
        });
    }

    private static async Task PermissionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("perm-1", async () =>
        {
            await MenuAsync(scene, "Access Control", "Roles");
            await FastGoAsync(scene, RowOf(page, "Editor").Locator("a:text-is(\"Edit\")").First);
        });

        await scene.StepAsync("perm-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#search-box"), "Custom User", 20);
            await scene.HighlightAsync(page.Locator("tr[data-text=\"Manage Own Custom User Settings - User Profile\"]"));
        });

        await scene.StepAsync("perm-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("input[id=\"Checkbox.ManageOwnCustomUserSettings_UserProfile\"]"));
            await FastGoAsync(scene, page.Locator("button.save:text-is(\"Save\")").First);
        });

        await scene.StepAsync("perm-4", async () =>
        {
            await FastGoAsync(scene, RowOf(page, "Administrator").Locator("a:text-is(\"Edit\")").First);
            await scene.HighlightAsync(page.Locator(".alert-info:has-text(\"comprehensive access\")"));
        });
    }

    private static async Task DeployAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("deploy-1", async () =>
        {
            await MenuAsync(scene, "Tools", "Deployments", "Plans");
            await FastGoAsync(scene, RowOf(page, "Team profiles").Locator("a:text-is(\"Manage Steps\")").First);
        });

        await scene.StepAsync("deploy-2", async () =>
        {
            await FastClickAsync(scene, page.Locator("button:text-is(\"Add Step\")"));
            await page.Locator("#modalSteps").WaitForAsync();
            await FastTypeAsync(scene, page.Locator("#modalSteps #search-box"), "Custom User", 20);
            var item = page.Locator("#modalSteps .deployment-step-item:has-text(\"Custom User Settings\")");
            await scene.HighlightAsync(item);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, item.Locator("a:text-is(\"Add\")"));
        });

        await scene.StepAsync("deploy-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("label:has-text(\"Include all custom user settings.\")"), "Default: all");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button.create").First);
        });

        await scene.StepAsync("deploy-4", async () =>
            await scene.HighlightAsync(page.Locator("#stepOrder .badge:text-is(\"All\")"), "All sections"));
    }
}

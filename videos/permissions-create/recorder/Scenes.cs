using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Write a Permission": 04-roles reads the Manager role (it holds only Manage Roles) in the roles editor,
/// then the Administrator role. The role Manager is created in PrepareAsync with the Roles step of JSON Import,
/// outside the capture (the same result as the manual seed of the plan).
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
        ("04-roles", RolesAsync),
    ];

    private const string ManagerRole = """
        { "steps": [ { "name": "Roles", "Roles": [ { "Name": "Manager", "Description": "Manages the roles of the site.", "Permissions": [ "ManageRoles" ] } ] } ] }
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

    // Creates the role Manager with JSON Import, outside the capture. The page's code editor replaces the value of the
    // textarea on submit, so the form is posted by script.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.GotoAsync($"{BaseUrl}/Admin/DeploymentPlan/Import/Json", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var result = await page.EvaluateAsync<string>("""
            async json => {
              const form = document.querySelector('form:has(#Json)');
              document.getElementById('Json').value = json;
              const response = await fetch(form.action, { method: 'POST', body: new FormData(form) });
              return (await response.text()).includes('Recipe imported successfully') ? 'ok' : 'failed';
            }
            """, ManagerRole);
        Console.WriteLine($"Import: {result}");
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        var managerLink = page.Locator("a[href$='/Roles/Edit/Manager']").First;
        var adminLink = page.Locator("a[href$='/Roles/Edit/Administrator']").First;
        var search = page.Locator("#search-box");
        var rolesGroup = page.Locator("fieldset.permissions-list").Filter(new() { HasText = "(OrchardCore.Roles)" });
        var rolesLegend = rolesGroup.Locator("legend").First;
        var rolesTable = rolesGroup.Locator("table").First;
        var badge = rolesGroup.Locator(".badge").First;
        var allowManage = page.Locator("[id='Checkbox.ManageRoles']");
        var allowView = page.Locator("[id='Checkbox.ViewRoles']");
        var effectiveView = page.Locator("[id='Effective.ViewRoles']");

        await scene.StepAsync("roles-1", async () =>
        {
            await scene.PauseAsync(7500);
            await scene.ClickAndWaitForNavigationAsync(managerLink);
        });

        await scene.StepAsync("roles-2", async () =>
        {
            await scene.TypeAsync(search, "roles", delay: 90);
            await page.WaitForFunctionAsync("() => document.querySelectorAll('fieldset.permissions-list').length > 0 && [...document.querySelectorAll('fieldset.permissions-list')].filter(f => f.offsetParent !== null).length === 1");
            await scene.HighlightAsync(rolesLegend, "Feature name and identifier");
            await scene.PauseAsync(6000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(rolesTable, "Descriptions from the code");
        });

        await scene.StepAsync("roles-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(badge, "Security Critical");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(allowManage, "Allow");
        });

        await scene.StepAsync("roles-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(allowView, "Allow");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(effectiveView, "Effective");
        });

        await scene.StepAsync("roles-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a.cancel").First);
            await scene.ClickAndWaitForNavigationAsync(adminLink);
            await scene.HighlightAsync(page.Locator("form .alert.alert-info").First);
        });
    }
}

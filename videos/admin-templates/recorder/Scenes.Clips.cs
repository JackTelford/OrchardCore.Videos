using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-enable", EnableAsync),
        ("04-create", CreateAsync),
        ("05-manage", ManageAsync),
    ];

    private const string Liquid1 = "<li class=\"nav-item\">\n    <a href=\"{{ '~' | absolute_url }}\"\n       class=\"nav-link\"\n       data-bs-toggle=\"tooltip\"\n       data-bs-placement=\"bottom\"\n       title=\"{{ 'Visit Site' | t }}\"\n       role=\"button\">\n        <i class=\"fa-solid fa-width-auto fa-external-link me-1\" aria-hidden=\"true\"></i>\n        {{ 'Visit Site' | t }}\n    </a>\n</li>";

    private static readonly string s_liquid2 = Liquid1.Replace("        {{ 'Visit Site' | t }}", "        {{ 'Open Site' | t }}");

    private const string NavLink = ".user-top-navbar li.nav-item:has(i.fa-external-link)";

    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task SetEditorAsync(IPage page, string text)
    {
        await page.Locator(".template-editor .monaco-editor").First.WaitForAsync();
        await page.EvaluateAsync("async (text) => { const monaco = await window.__orchardCoreMonacoReady; const eds = monaco.editor.getEditors ? monaco.editor.getEditors() : []; if (eds.length) { eds[0].setValue(text); } else { monaco.editor.getModels()[0].setValue(text); } }", text);
    }

    private static async Task OpenGroupAsync(IPage page, string group)
    {
        var button = page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\"))");
        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await button.ClickAsync();
            await page.WaitForTimeoutAsync(400);
        }
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Tools\"))"), 300);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))"));
            await FastTypeAsync(scene, page.Locator("#search-box"), "admin templates", 40);
            await page.Locator("[data-filter-value*='OrchardCore.AdminTemplates']").First.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var row = page.Locator("[data-filter-value*='OrchardCore.AdminTemplates']").First;
            await scene.HighlightAsync(row.Locator(".direct-dependencies .badge").First);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(row.Locator("a.btn.btn-primary:text-is(\"Enable\")").First));
            await page.GetByText("The feature Admin Templates was enabled.").First.WaitForAsync();
            await FastTypeAsync(scene, page.Locator("#search-box"), "admin templates", 40);
            await page.Locator("[data-filter-value*='OrchardCore.AdminTemplates']").First.WaitForAsync();
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Design\"))"), 700);
            var templates = page.Locator("#left-nav").GetByText("Templates", new() { Exact = true }).Locator("visible=true").First;
            var admin = page.Locator("#left-nav").GetByText("Admin Templates", new() { Exact = true }).Locator("visible=true").First;
            await scene.HighlightAsync(templates);
            await scene.HighlightAsync(admin);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, admin);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Templates/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("a.btn.btn-secondary.create").First);
            await page.Locator(".action-bar-bottom").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator(".action-bar-bottom").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("create-2", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#Name"), "VisitSiteNavbarItem", 25);
            await FastTypeAsync(scene, page.Locator("#Description"), "Visit Site link with a text label", 15);
        });

        await scene.StepAsync("create-3", async () =>
        {
            await FastClickAsync(scene, page.Locator("#Content_editor").First, 200);
            await SetEditorAsync(page, Liquid1);
        });

        await scene.StepAsync("create-4", async () =>
            await scene.HighlightAsync(page.Locator("#Content_editor").First));

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(1000);
            await FastGoAsync(scene, page.Locator("button.btn.btn-primary.save").First);
        });

        await scene.StepAsync("create-6", async () =>
        {
            await scene.HighlightAsync(page.Locator(NavLink).First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Content\"))"), 400);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Content Items\"))").First);
            await scene.HighlightAsync(page.Locator(NavLink).First);
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Templates/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("manage-1", async () =>
        {
            await FastGoAsync(scene, page.Locator("a.btn.btn-primary.btn-sm:text-is(\"Edit\")").First);
            await page.Locator(".template-editor .view-lines:has-text(\"Visit Site\")").First.WaitForAsync();
            await scene.PauseAsync(2000);
            await SetEditorAsync(page, s_liquid2);
            await FastGoAsync(scene, page.Locator("button.btn.btn-primary.save").First);
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await FastClickAsync(scene, page.Locator("a.btn.btn-danger.btn-sm").First, 200);
            await page.Locator("#modalOkButton").WaitForAsync();
            await scene.PauseAsync(2000);
            await FastGoAsync(scene, page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("manage-3", async () =>
            await scene.HighlightAsync(page.Locator(NavLink).First));
    }
}

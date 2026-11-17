using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-list", ListAsync),
        ("03-create", CreateAsync),
        ("04-edit", EditAsync),
        ("05-actions", ActionsAsync),
        ("06-find", FindAsync),
    ];

    private const string UserPassword = "Password1!";

    // Seeds six users and disables one (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        var commands = new[]
        {
            ("alex.chen", "Editor"), ("sam.okafor", "Author"), ("taylor.brooks", "Contributor"),
            ("riley.nguyen", "Moderator"), ("casey.lopez", "Contributor"), ("drew.patel", "Author"),
        }.Select(u => $"\"createUser /UserName:{u.Item1} /Password:{UserPassword} /Email:{u.Item1}@example.com /Roles:{u.Item2}\"");
        await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"command\",\"Commands\":[" + string.Join(",", commands) + "]}]}");

        await page.GotoAsync(BaseUrl + "/Admin/Users/Index");
        var row = Row(page, "drew.patel");
        await row.Locator("button.actions").ClickAsync();
        await row.Locator(".actions-menu a.dropdown-item:text-is(\"Disable\")").ClickAsync();
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("#modalOkButton").ClickAsync());
        Console.WriteLine("Seeded.");
        await context.CloseAsync();
    }

    private static ILocator Row(IPage page, string name)
        => page.Locator($"li.list-group-item:has-text(\"{(name == "admin" ? "admin@test.com" : name + "@example.com")}\")");

    private static async Task OpenMenuAsync(Scene scene, string group, string item)
    {
        await scene.ClickAsync(scene.Page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\"))"));
        await scene.PauseAsync(700);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator($"#left-nav a.item-label:has(span.title:text-is(\"{item}\"))"));
    }

    // A quicker click, for steps whose narration is short.
    private static async Task FastClickAsync(Scene scene, ILocator locator, int pauseAfter = 250)
    {
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 10);
        await Task.Delay(120);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await Task.Delay(pauseAfter);
    }

    private static async Task ConfirmAsync(Scene scene)
    {
        await scene.PauseAsync(800);
        await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, scene.Page.Locator("#modalOkButton"), 0));
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("list-1", async () => await OpenMenuAsync(scene, "Access Control", "Users"));

        await scene.StepAsync("list-2", async () =>
        {
            var alex = Row(page, "alex.chen");
            await scene.MoveToAsync(alex.Locator("a.btn:text-is(\"Edit\")"));
            await scene.MoveToAsync(alex.Locator("a.btn:text-is(\"View\")"));
            await scene.MoveToAsync(alex.Locator("button.actions"));
            await scene.HighlightAsync(Row(page, "drew.patel").Locator(".badge:has-text(\"Disabled\")"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(Row(page, "admin").Locator("button.actions"), -90, 0);
        });
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Users/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a.btn.create"));
            await scene.TypeAsync(page.Locator("#User_UserName"), "morgan.ellis");
            await scene.TypeAsync(page.Locator("#User_Email"), "morgan.ellis@example.com");
            await scene.HighlightAsync(page.Locator("#User_PhoneNumber"), "Optional");
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator(".password-generator-button"));
            await scene.MoveToAsync(page.Locator(".copy-password-button"));
        });

        await scene.StepAsync("create-3", async () =>
        {
            await page.Locator("label:text-is(\"Author\")").First.ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(page.Locator("label:text-is(\"Author\")").First);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save"));
        });

        await scene.StepAsync("create-4", async () =>
            await scene.HighlightAsync(Row(page, "morgan.ellis").Locator(".badge:has-text(\"Unconfirmed\")"), "Unconfirmed"));

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = Row(page, "morgan.ellis");
            await scene.ClickAsync(row.Locator("button.actions"));
            await scene.ClickAsync(row.Locator(".actions-menu a.dropdown-item:text-is(\"Confirm email\")"));
            await ConfirmAsync(scene);
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Users/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("edit-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(page, "morgan.ellis").Locator("a.btn:text-is(\"Edit\")"));
            await scene.HighlightAsync(page.Locator("#User_UserName"), "Read-only");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#User_Email"), "Read-only");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#User_PhoneNumber"), "Editable");
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#User_PhoneNumber"), "+12065550142");
            await page.Locator("label:text-is(\"Contributor\")").First.ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(page.Locator("label:text-is(\"Contributor\")").First);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save"));
        });

        await scene.StepAsync("edit-3", async () =>
            await scene.ClickAndWaitForNavigationAsync(Row(page, "morgan.ellis").Locator("a.btn:text-is(\"View\")")));
    }

    private static async Task ActionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Users/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("actions-1", async () => await FastClickAsync(scene, Row(page, "morgan.ellis").Locator("button.actions")));

        await scene.StepAsync("actions-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(page, "morgan.ellis").Locator(".actions-menu a.dropdown-item:text-is(\"Change password\")"));
            await scene.TypeAsync(page.Locator("#NewPassword"), UserPassword, 12);
            await scene.TypeAsync(page.Locator("#PasswordConfirmation"), UserPassword, 12);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("button.submit"));
            await scene.NavigateAsync(await Row(page, "admin").Locator(".actions-menu a.dropdown-item:text-is(\"Change password\")").GetAttributeAsync("href"));
        });

        await scene.StepAsync("actions-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a.cancel"));
            var row = Row(page, "morgan.ellis");
            await FastClickAsync(scene, row.Locator("button.actions"));
            await FastClickAsync(scene, row.Locator(".actions-menu a.dropdown-item:text-is(\"Disable\")"));
            await ConfirmAsync(scene);
        });

        await scene.StepAsync("actions-4", async () =>
        {
            var row = Row(page, "morgan.ellis");
            await FastClickAsync(scene, row.Locator("button.actions"));
            await FastClickAsync(scene, row.Locator(".actions-menu a.dropdown-item:text-is(\"Enable\")"));
            await ConfirmAsync(scene);
        });

        await scene.StepAsync("actions-5", async () =>
        {
            var row = Row(page, "casey.lopez");
            await FastClickAsync(scene, row.Locator("button.actions"));
            await FastClickAsync(scene, row.Locator(".actions-menu a.dropdown-item:text-is(\"Delete\")"));
            await ConfirmAsync(scene);
            await FastClickAsync(scene, Row(page, "admin").Locator("button.actions"));
        });
    }

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Users/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("find-1", async () =>
        {
            await scene.TypeAsync(page.Locator(".action-bar input[type=\"search\"]"), "sam");
            await scene.WaitForNavigationAsync(async () => await page.Keyboard.PressAsync("Enter"));
        });

        await scene.StepAsync("find-2", async () =>
        {
            await FastClickAsync(scene, page.Locator("#filter-dropdown"));
            await scene.PauseAsync(3000);
            await FastClickAsync(scene, page.Locator(".dropdown-menu a.dropdown-item:text-is(\"Filter syntax\")"));
        });

        await scene.StepAsync("find-3", async () =>
        {
            await FastClickAsync(scene, page.Locator(".modal-footer button:text-is(\"Close\")").First);
            await scene.NavigateAsync("/Admin/Users/Index");
            await FastClickAsync(scene, page.Locator(".filter button.dropdown-toggle").Nth(0));
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator(".filter a.dropdown-item:has(span.text:text-is(\"Author\"))")));
            await FastClickAsync(scene, page.Locator(".filter button.dropdown-toggle").Nth(1));
            await scene.WaitForNavigationAsync(async () => await FastClickAsync(scene, page.Locator(".filter a.dropdown-item:has(span.text:text-is(\"Disabled Users\"))")));
            await FastClickAsync(scene, page.Locator(".filter button.dropdown-toggle").Nth(2));
        });

        await scene.StepAsync("find-4", async () =>
        {
            await scene.NavigateAsync("/Admin/Users/Index");
            await FastClickAsync(scene, Row(page, "taylor.brooks").Locator("input[name=\"itemIds\"]"));
            await FastClickAsync(scene, Row(page, "riley.nguyen").Locator("input[name=\"itemIds\"]"));
            await FastClickAsync(scene, page.Locator("#bulk-action-menu-button"));
            await scene.PauseAsync(1000);
            await FastClickAsync(scene, page.Locator(".dropdown-menu a.dropdown-item[data-action=\"Disable\"]"));
            await ConfirmAsync(scene);
        });
    }
}

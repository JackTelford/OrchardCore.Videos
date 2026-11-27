using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-setting", SettingAsync),
        ("04-change", ChangeAsync),
        ("05-checks", ChecksAsync),
    ];

    // Seed: a second user, jamie, with a confirmed address.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await SeedPageAsync(browser);
        await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"command\",\"Commands\":[\"createUser /UserName:jamie /Password:" + Password + " /Email:jamie.chen@example.com\"]}]}");
        await page.GotoAsync(BaseUrl + "/Admin/Users/Index", new() { WaitUntil = WaitUntilState.NetworkIdle });
        Console.WriteLine("Users: " + string.Join(" | ", await page.Locator("li.list-group-item").AllInnerTextsAsync()));
        await context.CloseAsync();
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
        await scene.OpenAsync("/Admin/Features");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await OpenGroupAsync(page, "Tools");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.PauseAsync(2800);
            await FastTypeAsync(scene, page.Locator("#search-box"), "change email", 70);
            await scene.PauseAsync(600);
            var row = page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_Users_ChangeEmail") }).First;
            await scene.HighlightAsync(row, "Users Change Email");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var button = page.Locator("#btn-enable-OrchardCore_Users_ChangeEmail");
            await scene.MoveToAsync(button);
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(button);
                await page.GetByText("The feature Users Change Email was enabled.").First.WaitForAsync();
            });
            await scene.HighlightAsync(page.GetByText("The feature Users Change Email was enabled.").First);
        });
    }

    private static async Task SettingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/userChangeEmail");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await OpenGroupAsync(page, "Settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("setting-1", async () =>
        {
            await scene.PauseAsync(3000);
            var item = page.Locator("#left-nav").GetByText("Change Email", new() { Exact = true }).Locator("visible=true").First;
            await scene.HighlightAsync(item, "Settings, Security, Change Email");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("input[type=checkbox][name$=\"AllowChangeEmail\"]").First.Locator("xpath=.."));
        });

        await scene.StepAsync("setting-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("input[type=checkbox][name$=\"AllowChangeEmail\"]").First.Locator("xpath=..").Locator("label").First, 300);
            await scene.PauseAsync(1500);
            await FastGoAsync(scene, page.Locator("button.save").First);
        });

        await scene.StepAsync("setting-3", async () =>
        {
            var message = page.GetByText("Site settings updated successfully.").First;
            if (await message.IsVisibleAsync())
            {
                await scene.HighlightAsync(message);
            }

            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("input[type=checkbox][name$=\"AllowChangeEmail\"]").First.Locator("xpath=.."));
        });
    }

    private static async Task ChangeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Users/Index");
        var page = scene.Page;
        await page.Locator("li.list-group-item").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("change-1", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(page.Locator("li.list-group-item").Filter(new() { HasText = "admin@test.com" }).First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("button#navbarDropdown").First, "User menu");
        });

        await scene.StepAsync("change-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.Locator("button#navbarDropdown").First, 1200);
            var item = page.Locator("a.dropdown-item:has-text(\"Change Email\")").First;
            await scene.MoveToAsync(item);
            await scene.PauseAsync(3000);
            await FastGoAsync(scene, item);
        });

        await scene.StepAsync("change-3", async () =>
        {
            await scene.PauseAsync(500);
            await scene.HighlightAsync(page.Locator("#Email"));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await page.Locator("#Email").FillAsync("");
            await FastTypeAsync(scene, page.Locator("#Email"), "owner@example.com", 60);
        });

        await scene.StepAsync("change-4", async () =>
        {
            await FastGoAsync(scene, page.Locator("form button.btn-primary").First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(page.GetByText("Thank you for changing your email.").First);
        });

        await scene.StepAsync("change-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () => await scene.NavigateAsync("/Admin/Users/Index"));
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(page.Locator("li.list-group-item").Filter(new() { HasText = "owner@example.com" }).First);
        });
    }

    private static async Task ChecksAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/ChangeEmail");
        var page = scene.Page;
        await page.Locator("#Email").WaitForAsync();
        var type = await page.Locator("#Email").GetAttributeAsync("type");
        Console.WriteLine("Email field type: " + type);
        if (type == "email")
        {
            await page.EvaluateAsync("document.querySelector('form.no-multisubmit').noValidate = true");
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("checks-1", async () =>
        {
            await scene.PauseAsync(3500);
            await FastGoAsync(scene, page.Locator("form button.btn-primary").First);
        });

        await scene.StepAsync("checks-2", async () =>
        {
            if (type == "email")
            {
                await page.EvaluateAsync("document.querySelector('form.no-multisubmit').noValidate = true");
            }

            await page.Locator("#Email").FillAsync("");
            await FastTypeAsync(scene, page.Locator("#Email"), "jamie.chen@example.com", 50);
            await scene.PauseAsync(800);
            await FastGoAsync(scene, page.Locator("form button.btn-primary").First);
        });

        await scene.StepAsync("checks-3", async () =>
        {
            if (type == "email")
            {
                await page.EvaluateAsync("document.querySelector('form.no-multisubmit').noValidate = true");
            }

            await page.Locator("#Email").FillAsync("");
            await FastTypeAsync(scene, page.Locator("#Email"), "owner", 80);
            await scene.PauseAsync(800);
            await FastGoAsync(scene, page.Locator("form button.btn-primary").First);
        });
    }
}

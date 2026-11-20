using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Content Parts: Create Reusable Groups of Fields". PrepareAsync creates the Customer type (with the
/// Title part) outside the capture; each clip starts from the state the previous one left.
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
        ("02-create", CreateAsync),
        ("03-settings", SettingsAsync),
        ("04-fields", FieldsAsync),
        ("05-attach", AttachAsync),
        ("06-content", ContentAsync),
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

    // Creates the Customer content type with the Title part, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.GotoAsync($"{BaseUrl}/Admin/ContentTypes/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[data-displayname]").FillAsync("Customer");
        await page.Keyboard.PressAsync("Tab");
        await page.Locator("button.create").ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("AddPartsTo"));
        await page.Locator(".list-group-item").Filter(new() { Has = page.Locator("label:text-is('Title')") }).Locator("input[type='checkbox']").CheckAsync();
        await page.Locator("#add-selected-parts-button").ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("/Admin/ContentTypes/Edit/Customer"));
        await Task.Delay(1500);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static ILocator Menu(Scene scene, string label, string tag = "button")
        => scene.Page.Locator($"#left-nav {tag}.item-label:has(> .title:text-is(\"{label}\"))");

    private static ILocator AddField(Scene scene) => scene.Page.Locator("a.btn:text-is('Add Field')").First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("button[name='submit.Save'][value='Save']").First;

    private static ILocator DisplayName(Scene scene) => scene.Page.Locator("#DisplayName, #ContentPartDefinition_DisplayName").First;

    private static ILocator Description(Scene scene) => scene.Page.Locator("#Description, #ContentPartDefinition_Description").First;

    private static async Task AddTextFieldAsync(Scene scene, string name, int delay)
    {
        await scene.ClickAndWaitForNavigationAsync(AddField(scene));
        await scene.TypeAsync(DisplayName(scene), name, delay);
        await scene.ClickAsync(scene.Page.Locator("input[name='FieldTypeName'][value='TextField']"), pauseAfter: 300);
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(Menu(scene, "Design"), pauseAfter: 300);
            await Menu(scene, "Content Definition").WaitForAsync();
            await scene.ClickAsync(Menu(scene, "Content Definition"), pauseAfter: 300);
            await Menu(scene, "Content Parts", "a").WaitForAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "Content Parts", "a"));
            await scene.HighlightAsync(scene.Page.Locator("a#btnCreate"));
            await scene.PauseAsync(700);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a#btnCreate"));
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".ocat-limited .hint, .hint").First);
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("input[name='Name']"), "PostalAddress", 15);
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.create[type='submit']"));
        });

        await scene.StepAsync("create-3");
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentParts/Edit/PostalAddress");
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#ContentPartDefinition_ContentPartDefinition_Name").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(DisplayName(scene), "Postal Address", 10);
            await scene.TypeAsync(Description(scene), "Street, city and postal code.", 5);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".form-check:has(#ContentPartDefinition_Attachable)").First);
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".form-check:has(#ContentPartDefinition_Reusable)").First);
            await scene.PauseAsync(600);
            await scene.ClickAsync(scene.Page.Locator("#ContentPartDefinition_Reusable"), pauseAfter: 500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save[name='submit.Save']").First);
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".oc-breadcrumb-title").First);
        });
    }

    private static async Task FieldsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentParts/Edit/PostalAddress");
        await scene.StartCaptureAsync();

        await scene.StepAsync("fields-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h3:text-is('Fields')").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(AddField(scene));
            await scene.PauseAsync(600);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(AddField(scene));
        });

        await scene.StepAsync("fields-2", async () =>
        {
            await scene.TypeAsync(DisplayName(scene), "Street", 15);
            await scene.PauseAsync(500);
            await scene.ClickAsync(scene.Page.Locator("input[name='FieldTypeName'][value='TextField']"), pauseAfter: 600);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("fields-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("ul#fields li:has-text('Street')").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await AddTextFieldAsync(scene, "City", 8);
        });

        await scene.StepAsync("fields-4", async () =>
        {
            await AddTextFieldAsync(scene, "Postal Code", 8);
            await scene.HighlightAsync(scene.Page.Locator("ul#fields").First);
        });
    }

    private static async Task AttachAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("attach-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a[role='btn-edit-Customer']"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[role='btn-edit-Customer']"));
        });

        await scene.StepAsync("attach-2", async () =>
        {
            await scene.Page.Locator("h3:text-is('Parts')").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 250, 6);
            await scene.HighlightAsync(scene.Page.Locator("h3:text-is('Parts')").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("a.btn:text-is('Add Parts')").First);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("a.btn:text-is('Add Named Part')").First);
        });

        await scene.StepAsync("attach-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn:text-is('Add Named Part')").First);
            await scene.PauseAsync(1200);
            await scene.ClickAsync(scene.Page.Locator("input[name='SelectedPartName'][value='PostalAddress']"), pauseAfter: 300);
        });

        await scene.StepAsync("attach-4", async () =>
        {
            await scene.TypeAsync(DisplayName(scene), "Billing Address", 8);
            await scene.PauseAsync(400);
            await scene.TypeAsync(Description(scene), "Where invoices are sent.", 5);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("attach-5", async () =>
        {
            await scene.Page.Locator("ul#parts").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200, 6);
            await scene.HighlightAsync(scene.Page.Locator("ul#parts li:has-text('Billing Address')").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn:text-is('Add Named Part')").First);
            await scene.ClickAsync(scene.Page.Locator("input[name='SelectedPartName'][value='PostalAddress']"), pauseAfter: 300);
            await scene.TypeAsync(DisplayName(scene), "Shipping Address", 5);
            await scene.TypeAsync(Description(scene), "Where orders are delivered.", 4);
        });

        await scene.StepAsync("attach-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.Page.Locator("ul#parts").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200, 6);
            await scene.HighlightAsync(scene.Page.Locator("ul#parts li:has-text('Billing Address')").First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("ul#parts li:has-text('Shipping Address')").First);
        });
    }

        private static async Task FastAsync(Scene scene, ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 2 });
    }

private static async Task ContentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("content-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a#btn-create-Customer"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a#btn-create-Customer"));
        });

        await scene.StepAsync("content-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#TitlePart_Title"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".content-part-wrapper:has(.ocat-label:has-text('Billing Address'))").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".content-part-wrapper:has(.ocat-label:has-text('Shipping Address'))").First);
        });

        await scene.StepAsync("content-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastAsync(scene, scene.Page.Locator("#TitlePart_Title"), "Jamie Chen");
            await FastAsync(scene, scene.Page.Locator("input[name='BillingAddress.Street.Text']"), "22 Cider Mill Road");
            await FastAsync(scene, scene.Page.Locator("input[name='BillingAddress.City.Text']"), "Applewood");
            await FastAsync(scene, scene.Page.Locator("input[name='BillingAddress.PostalCode.Text']"), "AP1 4CM");
        });

        await scene.StepAsync("content-4", async () =>
        {
            await FastAsync(scene, scene.Page.Locator("input[name='ShippingAddress.Street.Text']"), "8 Harbor Row");
            await FastAsync(scene, scene.Page.Locator("input[name='ShippingAddress.City.Text']"), "Fernbrook");
            await FastAsync(scene, scene.Page.Locator("input[name='ShippingAddress.PostalCode.Text']"), "FB2 8HR");
        });

        await scene.StepAsync("content-5", async () =>
        {
            await scene.PauseAsync(600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish[name='submit.Publish']"));
        });
    }
}

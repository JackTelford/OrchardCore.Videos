using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "Content Type Settings". PrepareAsync builds the baseline outside the capture: the Event type with
/// a description, three published Event items and the Blocks editor on Page. Every clip starts from the baseline
/// (restore the snapshot before each one).
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
        ("02-visibility", VisibilityAsync),
        ("03-drafts", DraftsAsync),
        ("04-securable", SecurableAsync),
        ("05-pickers", PickersAsync),
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

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task GoAsync(string path) => await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // 2.1 The Event type.
        await GoAsync("/Admin/ContentTypes/Create");
        await page.Locator("#DisplayName").FillAsync("Event");
        await page.Keyboard.PressAsync("Tab");
        await page.Locator("button.create").ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("AddPartsTo"));
        await page.Locator("li.list-group-item:has(label:text-is('Title')) input[type='checkbox']").CheckAsync();
        await page.Locator("#add-selected-parts-button").ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("/Admin/ContentTypes/Edit/Event"));
        await page.Locator(".ocat-limited-wrapper:has(label:text-is('Description')) input").First.FillAsync("Open days and community events at the library.");
        await page.Locator("button.btn.save[name='submit.Save'][value='Save']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);

        // 2.2 Three published Event items.
        foreach (var title in new[] { "Library Book Sale", "Community Cleanup Day", "Spring Open House" })
        {
            await GoAsync("/Admin/Contents/ContentTypes/Event/Create");
            await page.Locator("#TitlePart_Title").FillAsync(title);
            await page.Locator("button.publish").ClickAsync();
            await page.WaitForURLAsync(url => !url.Contains("/Create"));
            await Task.Delay(1500);
        }

        // 2.3 The Blocks editor on Page.
        await GoAsync("/Admin/ContentTypes/Page/ContentParts/FlowPart/Edit");
        await page.Locator("select[data-type-part-editor-select], select").Filter(new() { HasText = "Blocks" }).First.SelectOptionAsync(new SelectOptionValue { Label = "Blocks" });
        await page.Locator("button.save").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(ILocator field, string text, int delay = 6)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
    }

    private static ILocator Setting(Scene scene, string label)
        => scene.Page.Locator($".ocat-limited-wrapper:has(label:text-is('{label}')) input").First;

    private static ILocator Option(Scene scene, string label)
        => scene.Page.Locator($"label.form-check-label:text-is('{label}')").First;

    private static ILocator SaveType(Scene scene) => scene.Page.Locator("button.btn.save[name='submit.Save'][value='Save']").First;

    private static async Task HighlightOptionHintAsync(Scene scene, string label)
        => await scene.HighlightAsync(scene.Page.Locator($".form-check:has(label.form-check-label:text-is('{label}'))").First);

    private static async Task SaveAndWaitAsync(Scene scene)
    {
        await scene.WaitForNavigationAsync(() => SaveType(scene).ClickAsync());
    }

    private static async Task VisibilityAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("visibility-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item", new() { HasText = "Spring Open House" }).First);
            await scene.PauseAsync(700);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 200);
            await scene.HighlightAsync(scene.Page.Locator("ul.dropdown-menu a.dropdown-item:has-text('Event')").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator(".contenttypes- button.dropdown-toggle, .contenttypes-selectpicker button.dropdown-toggle").First, pauseAfter: 1200);
        });

        await scene.StepAsync("visibility-2", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.NavigateAsync("/Admin/ContentTypes/List");
            await scene.HighlightAsync(scene.Page.Locator("#btn-create-Event"));
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[role='btn-edit-Event']"));
        });

        await scene.StepAsync("visibility-3", async () =>
        {
            foreach (var label in new[] { "Technical Name", "Display Name", "Description" })
            {
                await scene.ClearHighlightsAsync();
                await scene.HighlightAsync(Setting(scene, label));
                await scene.PauseAsync(1500);
            }
        });

        await scene.StepAsync("visibility-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await HighlightOptionHintAsync(scene, "Creatable");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await HighlightOptionHintAsync(scene, "Listable");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Option(scene, "Creatable"), pauseAfter: 300);
            await scene.ClickAsync(Option(scene, "Listable"), pauseAfter: 300);
        });

        await scene.StepAsync("visibility-5", async () =>
        {
            await SaveAndWaitAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item", new() { HasText = "Event" }).First.Locator("[role='btn-list-Event']"));
        });

        await scene.StepAsync("visibility-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Contents/ContentItems");
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 1200);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator(".contenttypes- button.dropdown-toggle, .contenttypes-selectpicker button.dropdown-toggle").First, pauseAfter: 1200);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("visibility-7", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/List");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[role='btn-list-Event']"));
        });
    }

    private static async Task DraftsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/Event");
        await scene.Page.Locator("a:text-is('Spring Open House')").First.ClickAsync();
        await scene.Page.Locator("input.content-caption-text").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("drafts-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button.publish").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button.draft").First);
        });

        await scene.StepAsync("drafts-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var title = scene.Page.Locator("input.content-caption-text").First;
            await title.FocusAsync();
            await scene.Page.Keyboard.PressAsync("End");
            await title.PressSequentiallyAsync(" Schedule", new() { Delay = 20 });
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("button.draft").First.ClickAsync());
            var row = scene.Page.Locator("li.list-group-item", new() { HasText = "Spring Open House Schedule" }).First;
            await scene.HighlightAsync(row.Locator(".tags .badge").First);
            await scene.HighlightAsync(row.Locator(".tags .badge").Nth(1));
        });

        await scene.StepAsync("drafts-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item", new() { HasText = "Spring Open House Schedule" }).First.Locator(".contentitem a").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("drafts-4", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/Edit/Event");
            await HighlightOptionHintAsync(scene, "Draftable");
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Option(scene, "Draftable"), pauseAfter: 300);
            await SaveAndWaitAsync(scene);
        });

        await scene.StepAsync("drafts-5", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Event");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:text-is('Community Cleanup Day')").First);
            await scene.HighlightAsync(scene.Page.Locator("button.publish").First);
        });
    }

    private static async Task SecurableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("securable-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item", new() { HasText = "Editor" }).First);
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.edit[href*='/Roles/Edit/Editor']").First);
        });

        await scene.StepAsync("securable-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Event", 40);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("fieldset.permissions-list:not(.d-none) legend").First);
        });

        await scene.StepAsync("securable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/ContentTypes/Edit/Event");
            await HighlightOptionHintAsync(scene, "Securable");
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Option(scene, "Securable"), pauseAfter: 300);
            await SaveAndWaitAsync(scene);
        });

        await scene.StepAsync("securable-4", async () =>
        {
            await scene.NavigateAsync("/Admin/Roles/Edit/Editor");
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Event", 40);
            await scene.PauseAsync(1000);
        });
    }

    private static async Task PickersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Paragraph");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pickers-1", async () =>
        {
            foreach (var label in new[] { "Stereotype", "Category", "Thumbnail" })
            {
                await scene.ClearHighlightsAsync();
                await scene.HighlightAsync(Setting(scene, label));
                await scene.PauseAsync(1000);
            }

            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Setting(scene, "Stereotype"));
        });

        await scene.StepAsync("pickers-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".ocat-limited-wrapper:has(label:text-is('Stereotype')) .hint").First);
        });

        await scene.StepAsync("pickers-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastAsync(Setting(scene, "Category"), "Text");
            await FastAsync(Setting(scene, "Thumbnail"), "~/OrchardCore.ContentTypes/placeholder.png", 3);
            await scene.Page.Keyboard.PressAsync("Tab");
            await SaveAndWaitAsync(scene);
        });

        await scene.StepAsync("pickers-4", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentTypes/Page/Create");
            await scene.ClickAsync(scene.Page.Locator("button[title='Add Widget']").First, pauseAfter: 300);
            await scene.Page.Locator("#contentTypePickerModal.show, #contentTypePickerModal").First.WaitForAsync();
        });

        await scene.StepAsync("pickers-5", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".content-type-picker-categories a.nav-link:text-is('Text')").First, pauseAfter: 400);
            var card = scene.Page.Locator("#contentTypePickerModal .card:visible, #contentTypePickerModal .content-type-picker-item:visible").First;
            await scene.HighlightAsync(card);
        });
    }
}

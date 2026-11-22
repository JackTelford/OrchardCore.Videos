using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the flows-bag-part video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-add", AddAsync),
        ("03-edit", EditAsync),
        ("04-manage", ManageAsync),
        ("05-view", ViewAsync),
        ("06-blocks", BlocksAsync),
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

    // Expands each collapsed group of the left menu above a link (outermost first) and returns the link.
    private static async Task<ILocator> ExpandMenuAsync(Scene scene, string href)
    {
        var page = scene.Page;
        var link = page.Locator($"#left-nav a[href$='{href}']").First;
        await link.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 90000 });
        var ancestors = link.Locator("xpath=ancestor::ul[@data-title]");
        var count = await ancestors.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var ul = ancestors.Nth(i);

            if (!await ul.IsVisibleAsync())
            {
                var title = await ul.GetAttributeAsync("data-title");
                var caption = ul.Locator($"xpath=preceding-sibling::figcaption | ../figcaption[.//span[normalize-space()='{title}']] | ../*/figcaption[.//span[normalize-space()='{title}']]").First;
                await caption.ClickAsync();
                await Task.Delay(300);
            }
        }

        return link;
    }

    // Moves to a link or button, clicks it and cuts the page load out of the video.
    private static async Task GoAsync(Scene scene, ILocator link, ILocator ready = null)
    {
        await scene.MoveToAsync(link);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(async () => await link.ClickAsync());

            if (ready is not null)
            {
                await ready.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Visible });
            }
        }, 450);
    }

    private static async Task RingAsync(Scene scene, ILocator target, double seconds)
    {
        await target.ScrollIntoViewIfNeededAsync();
        await Task.Delay(500);
        await scene.HighlightAsync(target);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    private static async Task TypeInAsync(Scene scene, ILocator field, string text, int delay = 35)
    {
        await field.ScrollIntoViewIfNeededAsync();
        await scene.ClickAsync(field, pauseAfter: 100);
        await scene.Page.Keyboard.TypeAsync(text, new() { Delay = delay });
    }

    private static ILocator Card(Scene scene, int n) => scene.Page.Locator(".widget-editor").Nth(n);

    private static ILocator CardInput(Scene scene, int card, int input) => Card(scene, card).Locator(".widget-editor-body input.form-control").Nth(input);

    private const string TypesRecipe = """
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Question", "DisplayName": "Question",
          "Settings": { "ContentTypeSettings": { "Draftable": true, "Versionable": true, "Description": "A question with its answer." } },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            { "PartName": "Question", "Name": "Question", "Settings": { "ContentTypePartSettings": { "Position": "1" } } }
          ]
        },
        {
          "Name": "Tip", "DisplayName": "Tip",
          "Settings": { "ContentTypeSettings": { "Draftable": true, "Versionable": true, "Description": "A short piece of advice." } },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            { "PartName": "Tip", "Name": "Tip", "Settings": { "ContentTypePartSettings": { "Position": "1" } } }
          ]
        }
      ],
      "ContentParts": [
        { "Name": "Question", "Settings": {}, "ContentPartFieldDefinitionRecords": [ { "FieldName": "TextField", "Name": "Answer", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Answer", "Position": "0" } } } ] },
        { "Name": "Tip", "Settings": {}, "ContentPartFieldDefinitionRecords": [ { "FieldName": "TextField", "Name": "Details", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Details", "Position": "0" } } } ] }
      ]
    }
  ]
}
""";

    // Imports the types Question and Tip that the bag will hold (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", TypesRecipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000);
        Console.WriteLine("Imported Question and Tip.");
        await context.CloseAsync();
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        var page = scene.Page;
        var typesLink = await ExpandMenuAsync(scene, "/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await RingAsync(scene, typesLink, 1.0);
            await GoAsync(scene, page.Locator("#btnCreate"), page.Locator("#DisplayName"));
            await TypeInAsync(scene, page.Locator("#DisplayName"), "Help Page");
            await page.Keyboard.PressAsync("Tab");
            await GoAsync(scene, page.Locator("button.create").First, page.GetByText("Choose the Parts to add"));
        });

        await scene.StepAsync("add-2", async () =>
        {
            var box = page.Locator("li.list-group-item:has(label.form-check-label:text-is('Title')) input[type=checkbox]").First;
            await box.ScrollIntoViewIfNeededAsync();
            await Task.Delay(400);
            await scene.ClickAsync(box, pauseAfter: 300);
            await GoAsync(scene, page.Locator("#add-selected-parts-button"), page.GetByText("Edit Content Type").First);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await RingAsync(scene, page.Locator("#parts").First, 2.0);
            await GoAsync(scene, page.Locator("a:text-is('Add Named Part')").First, page.Locator("#DisplayName"));
        });

        await scene.StepAsync("add-4", async () =>
        {
            await TypeInAsync(scene, page.Locator("#DisplayName"), "Help Items");
            await page.Keyboard.PressAsync("Tab");
            await TypeInAsync(scene, page.Locator("#Description"), "Questions and tips.");
            await scene.HighlightAsync(page.GetByLabel("Bag").First);
            await scene.HoldAsync(0.8);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button:has-text('Save & Configure')").First, page.GetByText("Contained Content Types").First);
        });

        await scene.StepAsync("add-5", async () =>
        {
            await RingAsync(scene, page.Locator(".ocat-end:has(#SourceContentTypes)").First, 1.5);

            foreach (var name in new[] { "Question", "Tip" })
            {
                var box = page.Locator($"#ContentTypesContainer input[type=checkbox][value='{name}']").First;
                await box.ScrollIntoViewIfNeededAsync();
                await Task.Delay(300);
                await scene.ClickAsync(box, pauseAfter: 400);
            }
        });

        await scene.StepAsync("add-6", async () =>
        {
            await RingAsync(scene, page.Locator("input[placeholder='Summary']").First, 1.5);
            var collapse = page.Locator("input[type=checkbox][name$='CollapseContainedItems']").First;
            await collapse.ScrollIntoViewIfNeededAsync();
            await Task.Delay(400);
            await scene.HighlightAsync(collapse);
            await scene.HoldAsync(1.0);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(collapse, pauseAfter: 300);
        });

        await scene.StepAsync("add-7", async () =>
        {
            await RingAsync(scene, page.Locator("select[data-type-part-editor-select]").First, 1.5);
            var save = page.Locator("button.save").First;
            await GoAsync(scene, save, page.GetByText("part settings have been saved").First);
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await ExpandMenuAsync(scene, "/Admin/Contents/ContentItems/");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var add = page.Locator(".btn-bag-add button.dropdown-toggle").First;

        await scene.StepAsync("edit-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await scene.HoldAsync(2.0);
            await GoAsync(scene, page.Locator("ul.dropdown-menu a.dropdown-item:text-is('Help Page')").First, page.GetByText("Help Items").First);
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await TypeInAsync(scene, page.Locator("form input.content-caption-text").First, "Account Help");
            await scene.HighlightAsync(add);
        });

        await scene.StepAsync("edit-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(add, pauseAfter: 300);
            await scene.HoldAsync(1.0);
            await scene.ClickAsync(page.Locator(".btn-bag-add a.dropdown-item:text-is('Question')").First, pauseAfter: 300);
            await Assertions.Expect(page.Locator(".widget-editor")).ToHaveCountAsync(1, new() { Timeout = 30000 });
            await TypeInAsync(scene, CardInput(scene, 0, 0), "How do I reset my password?");
            await TypeInAsync(scene, CardInput(scene, 0, 1), "Use Forgot password on the sign-in page.");
        });

        await scene.StepAsync("edit-4", async () =>
        {
            await scene.ClickAsync(add, pauseAfter: 300);
            await scene.ClickAsync(page.Locator(".btn-bag-add a.dropdown-item:text-is('Tip')").First, pauseAfter: 300);
            await Assertions.Expect(page.Locator(".widget-editor")).ToHaveCountAsync(2, new() { Timeout = 30000 });
            await TypeInAsync(scene, CardInput(scene, 1, 0), "Keep your contact details current");
            await TypeInAsync(scene, CardInput(scene, 1, 1), "Review them twice a year.");
        });

        await scene.StepAsync("edit-5", async () =>
        {
            var publish = page.Locator("button.publish").First;
            await publish.ScrollIntoViewIfNeededAsync();
            await GoAsync(scene, publish, page.GetByText("has been published").First);
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("manage-1", async () =>
        {
            await GoAsync(scene, page.Locator("li:has-text('Account Help') a.btn.edit").First, page.Locator(".widget-editor").First);
            await RingAsync(scene, Card(scene, 0).Locator(".widget-editor-btn-expand"), 1.2);
            await scene.ClickAsync(Card(scene, 0).Locator(".widget-editor-btn-expand"), pauseAfter: 300);
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await RingAsync(scene, Card(scene, 1).Locator(".widget-editor-handle"), 1.2);
            await scene.DragToAsync(Card(scene, 1).Locator(".widget-editor-handle"), Card(scene, 0).Locator(".widget-editor-header"), 0, -10);
            await scene.HoldAsync(0.8);
        });

        await scene.StepAsync("manage-3", async () =>
        {
            var trash = Card(scene, 0).Locator(".widget-delete");
            await RingAsync(scene, trash, 1.0);
            await scene.ClickAsync(trash, pauseAfter: 300);
            await page.Locator("#modalCancelButton").WaitForAsync();
            await scene.HoldAsync(2.0);
            await scene.ClickAsync(page.Locator("#modalCancelButton"), pauseAfter: 300);
        });

        await scene.StepAsync("manage-4", async () =>
        {
            var publish = page.Locator("button.publish").First;
            await publish.ScrollIntoViewIfNeededAsync();
            await GoAsync(scene, publish, page.GetByText("has been published").First);
        });
    }

    private static async Task ViewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("view-1", async () =>
        {
            var view = page.Locator("li:has-text('Account Help') a.btn.view").First;
            await scene.MoveToAsync(view);
            var tab = await page.Context.RunAndWaitForPageAsync(async () => await view.ClickAsync());
            await tab.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await scene.SwitchToAsync(tab);
            await scene.HoldAsync(1.0);
        });
    }

    private static async Task BlocksAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/List");
        var page = scene.Page;
        await scene.NavigateAsync(await page.Locator("a[role='btn-edit-HelpPage']").First.GetAttributeAsync("href"));
        await page.Locator("#parts").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("blocks-1", async () =>
        {
            await GoAsync(scene, page.Locator("#parts li:has-text('Help Items') a:text-is('Edit')").First, page.Locator("select[data-type-part-editor-select]"));
            var select = page.Locator("select[data-type-part-editor-select]").First;
            await RingAsync(scene, select, 1.2);
            await scene.SelectAsync(select, "Blocks");
            await scene.HighlightAsync(page.Locator("input[name$='AddButtonText']").First);
            await scene.HighlightAsync(page.Locator("input[name$='ModalTitleText']").First);
        });

        await scene.StepAsync("blocks-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("button.save").First, page.GetByText("part settings have been saved").First);
            var row = page.Locator("#parts li:has-text('Help Items')").First;
            await row.ScrollIntoViewIfNeededAsync();
            await Task.Delay(600);
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("blocks-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems");
            await GoAsync(scene, page.Locator("li:has-text('Account Help') a.btn.edit").First, page.Locator("button.bagpart-blocks-picker-button"));
            var picker = page.Locator("button.bagpart-blocks-picker-button").First;
            await picker.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(picker);
            await scene.HoldAsync(0.8);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(picker, pauseAfter: 300);
            await page.Locator("#contentTypePickerModal .content-type-card").First.WaitForAsync();
            await scene.HoldAsync(2.0);
            await scene.ClickAsync(page.Locator("#contentTypePickerModal .content-type-card:has-text('Question') button").First, pauseAfter: 500);
        });
    }
}

using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clip of "Write a Content Field With Its Editor and Settings": 08-admin adds a Boolean field to the Event type,
/// sets its label, default value and editor, and uses it in the editor. The Event type is seeded by JSON Import in
/// PrepareAsync, outside the capture.
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
        ("08-admin", AdminAsync),
    ];

    private const string EventType = """
        {
          "steps": [
            {
              "name": "ContentDefinition",
              "ContentTypes": [
                {
                  "Name": "Event",
                  "DisplayName": "Event",
                  "Settings": { "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true } },
                  "ContentTypePartDefinitionRecords": [
                    { "PartName": "Event", "Name": "Event", "Settings": { "ContentTypePartSettings": { "Position": "2" } } },
                    {
                      "PartName": "AutoroutePart",
                      "Name": "AutoroutePart",
                      "Settings": {
                        "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "{{ Model.ContentItem | display_text | slugify }}" },
                        "ContentTypePartSettings": { "Position": "1" }
                      }
                    },
                    { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } }
                  ]
                }
              ],
              "ContentParts": [ { "Name": "Event", "Settings": {}, "ContentPartFieldDefinitionRecords": [] } ]
            }
          ]
        }
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

    // Creates the Event type with JSON Import, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.GotoAsync($"{BaseUrl}/Admin/DeploymentPlan/Import/Json", new() { WaitUntil = WaitUntilState.NetworkIdle });

        // The page replaces the value of the textarea with its code editor's own on submit, so the form is posted by script.
        var result = await page.EvaluateAsync<string>("""
            async json => {
              const form = document.querySelector('form:has(#Json)');
              document.getElementById('Json').value = json;
              const response = await fetch(form.action, { method: 'POST', body: new FormData(form) });
              return (await response.text()).includes('Recipe imported successfully') ? 'ok' : 'failed';
            }
            """, EventType);
        Console.WriteLine($"Import: {result}");
        await page.GotoAsync($"{BaseUrl}/Admin/ContentTypes/Edit/Event", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.GetByText("Edit Content Type - Event").First.WaitForAsync();
        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    private static async Task FastAsync(ILocator field, string text)
    {
        await field.FocusAsync();
        await field.PressSequentiallyAsync(text, new() { Delay = 8 });
    }

    private static async Task AdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Event");
        await scene.Page.GetByText("Edit Content Type - Event").First.WaitForAsync();
        await scene.Page.Locator("a.btn:has-text('Add Field')").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("admin-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h3:has-text('Fields')").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("ul#parts").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn:has-text('Add Field')").First);
        });

        await scene.StepAsync("admin-2", async () =>
        {
            await FastAsync(scene.Page.GetByLabel("Display Name"), "Featured");
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(600);
            var list = scene.Page.Locator("ul.list-group").Filter(new() { Has = scene.Page.Locator("label:has-text('Boolean Field')") }).First;
            await scene.HighlightAsync(list);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("label:has-text('Boolean Field')").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Save & Configure')").First);
            await scene.Page.GetByText("\"Featured\" settings").First.WaitForAsync();
        });

        await scene.StepAsync("admin-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByLabel("Hint", new() { Exact = true }).First);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByLabel("Label", new() { Exact = true }).First);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByLabel("Default value is true").First);
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await FastAsync(scene.Page.GetByLabel("Label", new() { Exact = true }).First, "Featured event");
            await scene.ClickAsync(scene.Page.GetByLabel("Default value is true").First);
        });

        await scene.StepAsync("admin-4", async () =>
        {
            var select = scene.Page.Locator("select[data-field-editor-select]").First;
            await select.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(select, "Standard, Switch");
            await scene.PauseAsync(3000);
            await scene.SelectAsync(select, "Switch");
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("admin-5", async () =>
        {
            var select = scene.Page.Locator("select[data-field-display-select]").First;
            await select.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(select, "Standard");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save").First);
            await scene.Page.GetByText("Edit Content Type - Event").First.WaitForAsync();
        });

        await scene.StepAsync("admin-6", async () =>
        {
            var row = scene.Page.Locator("#fields li").First;
            await scene.HighlightAsync(row);
            await scene.PauseAsync(3000);
            var badge = row.Locator(".badge").First;

            if (await badge.CountAsync() > 0)
            {
                await scene.MoveToAsync(badge);
            }
        });

        await scene.StepAsync("admin-7", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Contents/ContentTypes/Event/Create");
            await scene.Page.GetByLabel("Title", new() { Exact = true }).First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".form-switch").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await FastAsync(scene.Page.GetByLabel("Title", new() { Exact = true }).First, "Community meetup");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish").First);
        });

        await scene.StepAsync("admin-8", async () =>
        {
            await scene.NavigateAsync("/community-meetup");
            var block = scene.Page.Locator("div.field-type-booleanfield").First;
            await block.WaitForAsync();
            await scene.HighlightAsync(block);
        });
    }
}

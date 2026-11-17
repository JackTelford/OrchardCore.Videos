using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static IReadOnlyDictionary<string, double> s_durations = new Dictionary<string, double>();

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-editor", EditorAsync),
        ("03-list", ListAsync),
        ("04-display", DisplayAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        s_durations = durations;
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    // The seed: the content type Workshop with three fields and one published Workshop. Everything is invented.
    private const string SeedRecipe = """"
        {
          "name": "", "displayName": "", "description": "", "author": "", "website": "", "version": "",
          "issetuprecipe": false, "categories": [], "tags": [],
          "steps": [
            {
              "name": "ContentDefinition",
              "ContentTypes": [
                {
                  "Name": "Workshop",
                  "DisplayName": "Workshop",
                  "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
                  "ContentTypePartDefinitionRecords": [
                    { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
                    { "PartName": "AutoroutePart", "Name": "AutoroutePart", "Settings": { "ContentTypePartSettings": { "Position": "1" }, "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": false } } },
                    { "PartName": "Workshop", "Name": "Workshop", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
                  ]
                }
              ],
              "ContentParts": [
                {
                  "Name": "Workshop",
                  "Settings": {},
                  "ContentPartFieldDefinitionRecords": [
                    { "FieldName": "TextField", "Name": "Headline", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Headline", "Position": "0" } } },
                    { "FieldName": "NumericField", "Name": "Seats", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Seats", "Position": "1" }, "NumericFieldSettings": { "Scale": 0, "Minimum": 0, "Maximum": 40 } } },
                    { "FieldName": "TextField", "Name": "SkillLevel", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Skill Level", "Position": "2" }, "TextFieldPredefinedListEditorSettings": { "Editor": "Dropdown", "Options": [ { "name": "Beginner", "value": "Beginner" }, { "name": "Intermediate", "value": "Intermediate" } ] } } }
                  ]
                }
              ]
            },
            {
              "name": "Content",
              "data": [
                {
                  "ContentItemId": "4g9q0z3h6k2m8s5v1w7x4n9c3d",
                  "ContentItemVersionId": "5a2b8c4d6e1f7g3h9j0k4m5n6p",
                  "ContentType": "Workshop",
                  "DisplayText": "Spring Pottery Evening",
                  "Latest": true,
                  "Published": true,
                  "Author": "admin",
                  "TitlePart": { "Title": "Spring Pottery Evening" },
                  "AutoroutePart": { "Path": "spring-pottery-evening" },
                  "Workshop": { "Headline": { "Text": "Hands-on clay for beginners" }, "Seats": { "Value": 12 }, "SkillLevel": { "Text": "Beginner" } }
                }
              ]
            }
          ]
        }
        """";

    // Seeds the Workshop type and item outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 }, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror'); if (cm && cm.CodeMirror) { cm.CodeMirror.setValue(json); cm.CodeMirror.save(); } else { document.querySelector('#Json').value = json; } }", SeedRecipe);
        await page.Locator("button.import").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var text = await page.Locator("body").InnerTextAsync();
        Console.WriteLine("Import page said: " + text[..Math.Min(300, text.Length)].Replace('\n', ' '));
        await context.CloseAsync();
    }

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.SetDefaultTimeout(120_000);

        return scene;
    }

    // The clock of a step: its time without the off-camera waits, so a click lands on its sentence of the narration.
    private sealed class Clock(Scene scene, string id)
    {
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        private double _skipped;

        public double Duration { get; } = s_durations.TryGetValue(id, out var d) ? d : 2;

        public double Elapsed => _watch.Elapsed.TotalSeconds - _skipped;

        public async Task UntilAsync(double seconds)
        {
            Console.WriteLine($"    at {Elapsed:F1}s (target {seconds:F1}s of {Duration:F1}s)");
            var remaining = seconds - Elapsed;

            if (remaining > 0)
            {
                await scene.HoldAsync(remaining);
            }
        }

        // Holds until this fraction of the narration has been spoken.
        public Task FractionAsync(double fraction) => UntilAsync(Duration * fraction);

        // Holds until this many seconds before the end of the narration.
        public Task BeforeEndAsync(double seconds) => UntilAsync(Duration - seconds);

        public async Task OffAsync(Func<Task> wait, int lead = 400)
        {
            await scene.OffCameraAsync(async () =>
            {
                var t = Stopwatch.StartNew();
                await wait();
                _skipped += t.Elapsed.TotalSeconds + 0.4;
            }, lead);
        }
    }

    private static Task StepAsync(Scene scene, string id, Func<Clock, Task> actions)
        => scene.StepAsync(id, async () =>
        {
            var clock = new Clock(scene, id);
            await actions(clock);
            Console.WriteLine($"    done at {clock.Elapsed:F1}s of {clock.Duration:F1}s");
        });

    // A short cursor path: on a loaded machine every pointer move is a slow round trip.
    private static async Task ClickAsync(Scene scene, ILocator locator, int pauseAfter = 300)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await Task.Delay(100);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await Task.Delay(pauseAfter);
    }

    // Clicks something that loads a page; the load is cut out of the clip.
    private static async Task GoAsync(Scene scene, Clock clock, ILocator locator, int lead = 500)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await Task.Delay(100);
        await clock.OffAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), lead);
        await Task.Delay(150);
    }

    private static async Task TypeAsync(Scene scene, ILocator field, string text, int delay = 10)
    {
        await ClickAsync(scene, field, 150);
        await field.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = delay });
        await Task.Delay(200);

        if (await field.InputValueAsync() != text)
        {
            await field.FillAsync(text);
        }
    }

    private static async Task ScrollToAsync(Scene scene, ILocator element, double top)
    {
        var box = await element.BoundingBoxAsync();
        await scene.ScrollAsync(0, box!.Y - top, 6);
    }

    // Scrolls the Fields list of the content type to the top area of the screen.
    private static async Task ShowFieldsAsync(Scene scene, double top = 130)
    {
        var fields = scene.Page.Locator("#fields");
        await fields.WaitForAsync();
        await ScrollToAsync(scene, fields, top);
        await scene.MoveToAsync(900, 450, 8);
    }

    private static ILocator FieldRow(IPage page, string name) => page.Locator("#fields li").Filter(new() { HasText = name }).First;

    // 02-editor: a Numeric Field as a slider.
    private static async Task EditorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await page.Locator("#search-box").WaitForAsync();
        await scene.StartCaptureAsync();

        await StepAsync(scene, "editor-1", async clock =>
        {
            await TypeAsync(scene, page.Locator("#search-box"), "Workshop");
            var edit = page.Locator("a[role='btn-edit-Workshop']");
            await edit.WaitForAsync();
            await scene.PauseAsync(500);
            await clock.FractionAsync(0.55);
            await GoAsync(scene, clock, edit);
            await page.Locator("#fields").WaitForAsync();
        });

        await StepAsync(scene, "editor-2", async clock =>
        {
            await ShowFieldsAsync(scene, 150);
            var row = FieldRow(page, "Seats");
            await scene.HighlightAsync(row);
            await clock.FractionAsync(0.6);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, clock, page.Locator("a[href*='/ContentParts/Workshop/Fields/Seats/Edit']").First);
            await page.Locator("[data-field-editor-select]").WaitForAsync();
        });

        await StepAsync(scene, "editor-3", async clock =>
        {
            var minimum = page.Locator(".ocat-wrapper:has(label:text-is('Minimum'))").Last;
            var maximum = page.Locator(".ocat-wrapper:has(label:text-is('Maximum'))").Last;
            await ScrollToAsync(scene, minimum, 250);
            await scene.HighlightAsync(minimum);
            await scene.HighlightAsync(maximum);
        });

        await StepAsync(scene, "editor-4", async clock =>
        {
            await scene.ClearHighlightsAsync();
            var editor = page.Locator("[data-field-editor-select]");
            await ScrollToAsync(scene, editor, 300);
            await scene.HighlightAsync(editor);
            await clock.FractionAsync(0.6);
            await scene.SelectAsync(editor, "Slider", 300);
        });

        await StepAsync(scene, "editor-5", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await clock.FractionAsync(0.3);
            await GoAsync(scene, clock, page.Locator("button[name='submit.Save']").First);
            await ShowFieldsAsync(scene, 150);
            await scene.HighlightAsync(FieldRow(page, "Seats").Locator(".badge").First);
        });

        await StepAsync(scene, "editor-6", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await clock.OffAsync(() => scene.Page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Workshop/Create"));
            var handle = page.Locator(".numeric-field-slider-track .noUi-handle:visible").First;
            await handle.WaitForAsync();
            await scene.MoveToAsync(page.Locator(".numeric-field-slider").First);
        });

        await StepAsync(scene, "editor-7", async clock =>
        {
            var handle = page.Locator(".numeric-field-slider-track .noUi-handle:visible").First;
            var track = page.Locator(".numeric-field-slider-track:visible").First;
            var box = (await track.BoundingBoxAsync())!;
            await clock.FractionAsync(0.15);
            await scene.DragAsync(handle, box.X + box.Width * 0.75, box.Y + box.Height / 2);
            await scene.PauseAsync(700);
            var value = page.Locator(".numeric-field-slider-value:visible").First;
            await ClickAsync(scene, value, 100);
            await page.Keyboard.PressAsync("Control+A");
            await value.PressSequentiallyAsync("10", new LocatorPressSequentiallyOptions { Delay = 120 });
        });
    }

    // 03-list: a Text Field as a list of choices.
    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Workshop");
        await page.Locator("#fields").ScrollIntoViewIfNeededAsync();
        await ShowFieldsAsync(scene, 150);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "list-1", async clock =>
        {
            await scene.HighlightAsync(FieldRow(page, "Skill Level"));
            await clock.FractionAsync(0.5);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, clock, page.Locator("a[href*='/ContentParts/Workshop/Fields/SkillLevel/Edit']").First);
            await page.Locator("[data-field-editor-select]").WaitForAsync();
        });

        await StepAsync(scene, "list-2", async clock =>
        {
            var editor = page.Locator("[data-field-editor-select]");
            await ScrollToAsync(scene, editor, 200);
            await scene.SelectAsync(editor, "PredefinedList", 300);
            await page.Locator(".field-editor-predefinedlist").First.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await clock.FractionAsync(0.35);
            await scene.ScrollAsync(0, 300, 6);
        });

        await StepAsync(scene, "list-3", async clock =>
        {
            var panel = page.Locator(".field-editor-predefinedlist").First;
            await scene.SelectAsync(panel.Locator("select"), "Radio", 300);
            await ClickAsync(scene, panel.Locator("a:has-text('Add an option')").First, 200);
            var last = panel.Locator("table.options-table tbody tr:last-child input[placeholder='Enter a name']");
            await last.WaitForAsync();
            await scene.PauseAsync(300);
            await TypeAsync(scene, last, "Advanced");
        });

        await StepAsync(scene, "list-4", async clock =>
        {
            var panel = page.Locator(".field-editor-predefinedlist").First;
            await scene.HighlightAsync(panel.Locator("table.options-table th:has-text('Default?')").First);
        });

        await StepAsync(scene, "list-5", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await clock.FractionAsync(0.4);
            await GoAsync(scene, clock, page.Locator("button[name='submit.Save']").First);
            await ShowFieldsAsync(scene, 150);
        });

        await StepAsync(scene, "list-6", async clock =>
        {
            await clock.OffAsync(() => scene.Page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Workshop/Create"));
            var intermediate = page.Locator("label.form-check-label:text-is('Intermediate')");
            await intermediate.WaitForAsync();
            var radio = page.Locator(".form-check").Filter(new() { Has = intermediate }).Locator("input[type=radio]").First;
            await ClickAsync(scene, radio, 200);
            await scene.MoveToAsync(520, 330, 8);
            await radio.CheckAsync();
        });
    }

    // 04-display: a Text Field as a heading.
    private static async Task DisplayAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Workshop");
        await ShowFieldsAsync(scene, 150);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "display-1", async clock =>
        {
            await scene.HighlightAsync(FieldRow(page, "Headline"));
            await clock.FractionAsync(0.35);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, clock, page.Locator("a[href*='/ContentParts/Workshop/Fields/Headline/Edit']").First);
            var display = page.Locator("[data-field-display-select]");
            await display.WaitForAsync();
            await ScrollToAsync(scene, display, 300);
        });

        await StepAsync(scene, "display-2", async clock =>
        {
            var display = page.Locator("[data-field-display-select]");
            await scene.HighlightAsync(display);
            await clock.FractionAsync(0.65);
            await scene.SelectAsync(display, "Header", 300);
        });

        await StepAsync(scene, "display-3", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 150, 6);
            await clock.FractionAsync(0.4);
            await scene.SelectAsync(page.Locator(".field-display-header select").First, "h2", 300);
            await clock.FractionAsync(0.65);
            await GoAsync(scene, clock, page.Locator("button[name='submit.Save']").First);
            await ShowFieldsAsync(scene, 150);
            await scene.HighlightAsync(FieldRow(page, "Headline").Locator(".badge").First);
        });

        await StepAsync(scene, "display-4", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await clock.OffAsync(() => scene.Page.GotoAsync(BaseUrl + "/spring-pottery-evening"));
            var heading = page.Locator("h2.field-type-textfield").First;
            await heading.WaitForAsync();
            await scene.MoveToAsync(heading);
        });
    }
}

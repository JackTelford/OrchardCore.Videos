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
        ("02-add-picker", AddPickerAsync),
        ("03-pick", PickAsync),
        ("04-user-field", UserFieldAsync),
        ("04-user-pick", UserPickAsync),
        ("05-lucene-field", LuceneFieldAsync),
        ("05-lucene-pick", LucenePickAsync),
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

    private const string Seed1 = """
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Product",
          "DisplayName": "Product",
          "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            { "PartName": "AutoroutePart", "Name": "AutoroutePart", "Settings": { "ContentTypePartSettings": { "Position": "1" }, "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "products/{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": false } } },
            { "PartName": "Product", "Name": "Product", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
          ]
        },
        {
          "Name": "ProductBundle",
          "DisplayName": "Product Bundle",
          "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            { "PartName": "AutoroutePart", "Name": "AutoroutePart", "Settings": { "ContentTypePartSettings": { "Position": "1" }, "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "bundles/{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": false } } },
            { "PartName": "ProductBundle", "Name": "ProductBundle", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
          ]
        }
      ],
      "ContentParts": [
        { "Name": "Product", "Settings": { "ContentPartSettings": { "DisplayName": "Product" } }, "ContentPartFieldDefinitionRecords": [] },
        { "Name": "ProductBundle", "Settings": { "ContentPartSettings": { "DisplayName": "Product Bundle" } }, "ContentPartFieldDefinitionRecords": [] }
      ]
    },
    {
      "name": "content",
      "data": [
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Camp Stove", "Latest": true, "Published": true, "TitlePart": { "Title": "Camp Stove" }, "AutoroutePart": { "Path": "products/camp-stove" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Headlamp", "Latest": true, "Published": true, "TitlePart": { "Title": "Headlamp" }, "AutoroutePart": { "Path": "products/headlamp" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Insulated Bottle", "Latest": true, "Published": true, "TitlePart": { "Title": "Insulated Bottle" }, "AutoroutePart": { "Path": "products/insulated-bottle" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Rain Jacket", "Latest": true, "Published": true, "TitlePart": { "Title": "Rain Jacket" }, "AutoroutePart": { "Path": "products/rain-jacket" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Trail Backpack", "Latest": true, "Published": true, "TitlePart": { "Title": "Trail Backpack" }, "AutoroutePart": { "Path": "products/trail-backpack" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "ProductBundle", "DisplayText": "Weekend Camping Bundle", "Latest": true, "Published": true, "TitlePart": { "Title": "Weekend Camping Bundle" }, "AutoroutePart": { "Path": "bundles/weekend-camping-bundle" } }
      ]
    }
  ]
}
""";

    // Seed 2: three users (fake names) created by the Users recipe step; none of them signs in.
    private const string Seed2 = """
{
  "steps": [
    {
      "name": "Users",
      "Users": [
        { "UserId": "4a1c0000000000000000000001", "UserName": "morgan", "NormalizedUserName": "MORGAN", "Email": "morgan@example.com", "NormalizedEmail": "MORGAN@EXAMPLE.COM", "EmailConfirmed": true, "IsEnabled": true, "RoleNames": [ "Editor" ], "SecurityStamp": "stampmorgan0000000000000001" },
        { "UserId": "4a1c0000000000000000000002", "UserName": "jordan", "NormalizedUserName": "JORDAN", "Email": "jordan@example.com", "NormalizedEmail": "JORDAN@EXAMPLE.COM", "EmailConfirmed": true, "IsEnabled": true, "RoleNames": [ "Editor" ], "SecurityStamp": "stampjordan0000000000000002" },
        { "UserId": "4a1c0000000000000000000003", "UserName": "taylor", "NormalizedUserName": "TAYLOR", "Email": "taylor@example.com", "NormalizedEmail": "TAYLOR@EXAMPLE.COM", "EmailConfirmed": true, "IsEnabled": true, "RoleNames": [ "Author" ], "SecurityStamp": "stamptaylor0000000000000003" }
      ]
    }
  ]
}
""";

    // Seed 3: Lucene and its index (imported after clip 04-user-pick; run "Recorder prepare lucene" is not available, so it is a separate method).
    private const string Seed3 = """
{
  "steps": [
    { "name": "feature", "enable": [ "OrchardCore.Lucene", "OrchardCore.Search.Lucene.ContentPicker" ] },
    {
      "name": "CreateOrUpdateIndexProfile",
      "indexes": [
        {
          "IndexName": "Catalog",
          "ProviderName": "Lucene",
          "Type": "Content",
          "Name": "Catalog",
          "Properties": {
            "LuceneIndexMetadata": { "StoreSourceData": false, "AnalyzerName": "standardanalyzer" },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": [ "Product" ], "Culture": "any" },
            "LuceneIndexDefaultQueryMetadata": { "DefaultVersion": "LUCENE_30", "QueryAnalyzerName": "standardanalyzer", "AllowLuceneQueries": false, "DefaultSearchFields": [ "Content.ContentItem.FullText" ] }
          },
          "IndexLatest": false,
          "Culture": "any",
          "IndexedContentTypes": [ "Product" ]
        }
      ]
    }
  ]
}
""";

    private static async Task ImportJsonAsync(IPage page, string json)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(800);
        await page.EvaluateAsync("(json) => { const ta = document.querySelector('textarea'); ta.value = json; const cm = document.querySelector('.CodeMirror'); if (cm && cm.CodeMirror) { cm.CodeMirror.setValue(json); } }", json);
        await page.Locator("button.import").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500);
        var text = (await page.Locator("body").InnerTextAsync()).Replace("\n", " ");
        Console.WriteLine("import -> " + page.Url + " : " + text[..Math.Min(300, text.Length)]);
    }

    // Seeds 1 and 2 outside the capture; Seed 3 when VIDEO_SEED3 is set.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 }, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);

        if (Environment.GetEnvironmentVariable("VIDEO_SEED3") is { Length: > 0 })
        {
            await ImportJsonAsync(page, Seed3);
        }
        else
        {
            if (Environment.GetEnvironmentVariable("VIDEO_SKIP1") is not { Length: > 0 })
            {
                await ImportJsonAsync(page, Seed1);
            }

            await ImportJsonAsync(page, Seed2);
        }

        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

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
    private static async Task GoAsync(Scene scene, Clock clock, ILocator locator, int lead = 500, Func<Task> after = null)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await Task.Delay(100);
        await clock.OffAsync(async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y));

            if (after is not null)
            {
                await after();
            }
        }, lead);
        await Task.Delay(150);
    }

    private static Task JumpFieldsAsync(IPage page, double top = 120)
        => page.EvaluateAsync("(t) => { const e = [...document.querySelectorAll('h1,h2,h3,h4,h5')].find(h => h.textContent.trim() === 'Fields'); window.scrollBy(0, e.getBoundingClientRect().top - t); }", top);

    // Scrolls at once (inside an off-camera cut) so that the element sits this many pixels from the top.
    private static Task JumpAsync(IPage page, string selector, double top)
        => page.EvaluateAsync("([s, t]) => { const e = document.querySelector(s); window.scrollBy(0, e.getBoundingClientRect().top - t); }", new object[] { selector, top });

    private static async Task TypeAsync(Scene scene, ILocator field, string text, int delay = 3)
    {
        await ClickAsync(scene, field, 150);
        await field.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = delay });
        await Task.Delay(200);

        if (await field.InputValueAsync() != text)
        {
            await field.FillAsync(text);
        }
    }

    private static ILocator Label(IPage page, string name) => page.GetByLabel(name, new() { Exact = true });

    private static ILocator Heading(IPage page, string name) => page.GetByRole(AriaRole.Heading, new() { Name = name, Exact = true });

    private static async Task ScrollToAsync(Scene scene, ILocator element, double top)
    {
        var box = await element.BoundingBoxAsync();
        await scene.ScrollAsync(0, box!.Y - top, 6);
    }

    private static async Task ScrollTopAsync(Scene scene)
    {
        if (await scene.Page.EvaluateAsync<double>("() => window.scrollY") > 0)
        {
            await scene.ScrollAsync(0, -3000, 6);
        }
    }

    private static async Task BringIntoViewAsync(Scene scene, ILocator element, double top = 400)
    {
        var box = await element.BoundingBoxAsync();

        if (box!.Y < 100 || box.Y + box.Height > 740)
        {
            await ScrollToAsync(scene, element, top);
        }
    }

    private static async Task RingAsync(Scene scene, ILocator element, double seconds, string caption = null)
    {
        await scene.HighlightAsync(element, caption);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator MenuCaption(IPage page, string label)
        => page.Locator("#left-nav figcaption.nav-item-row").Filter(new() { HasTextRegex = new Regex($@"^\s*{Regex.Escape(label)}\s*$") }).First;

    // Opens a closed group of the admin menu without following its link.
    private static async Task OpenGroupAsync(Scene scene, string label, bool onCamera)
    {
        var toggle = MenuCaption(scene.Page, label).Locator("[aria-expanded]").First;

        if (await toggle.GetAttributeAsync("aria-expanded") != "false")
        {
            return;
        }

        if (onCamera)
        {
            await ClickAsync(scene, toggle, 300);
        }
        else
        {
            await toggle.ClickAsync();
            await Task.Delay(300);
        }
    }

    private static ILocator Picker(IPage page, string field)
        => page.Locator($".vue-multiselect[data-part='ProductBundle'][data-field='{field}']");

    private static ILocator FieldWrapper(IPage page, string field)
        => page.Locator($".ocat-wrapper.field-wrapper:has(.vue-multiselect[data-part='ProductBundle'][data-field='{field}'])");

    private static ILocator FieldTypeRadioLabel(IPage page, string type)
        => page.Locator("form[action*='AddFieldsTo'] ul.list-group label").Filter(new() { HasTextRegex = new Regex($@"^\s*{Regex.Escape(type)}\s*$") }).First;

    private static async Task ChooseFieldTypeAsync(Scene scene, string type, string value)
    {
        var label = FieldTypeRadioLabel(scene.Page, type);
        await label.WaitForAsync();
        await BringIntoViewAsync(scene, label);
        await ClickAsync(scene, label, 300);
        var radio = scene.Page.Locator($"input[name='FieldTypeName'][value='{value}']");

        if (!await radio.IsCheckedAsync())
        {
            await radio.CheckAsync();
        }
    }

    // Types in a picker, waits (off camera) for an option, holds so the list is readable, then clicks it.
    private static async Task PickOptionAsync(Scene scene, Clock clock, string field, string typed, string option, double hold = 0.8, double clickBeforeEnd = 0)
    {
        var picker = Picker(scene.Page, field);
        await ClickAsync(scene, picker.Locator(".multiselect__tags"), 150);
        var input = picker.Locator("input.multiselect__input");
        await input.PressSequentiallyAsync(typed, new LocatorPressSequentiallyOptions { Delay = 25 });
        var item = picker.Locator(".multiselect__option").Filter(new() { HasText = option }).First;
        await clock.OffAsync(() => item.WaitForAsync(), 100);
        await scene.HoldAsync(hold);

        if (clickBeforeEnd > 0)
        {
            await clock.BeforeEndAsync(clickBeforeEnd);
        }

        await ClickAsync(scene, item, 400);
    }

    // 02-add-picker: the field Products on Product Bundle.
    private static async Task AddPickerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/List");
        await OpenGroupAsync(scene, "Design", false);
        await OpenGroupAsync(scene, "Content Definition", false);
        await page.EvaluateAsync("() => document.querySelector(\"[role='btn-edit-ProductBundle']\").scrollIntoView({ block: 'center' })");
        await Task.Delay(500);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "add-1", async clock =>
        {
            await clock.UntilAsync(0.4);
            await scene.HighlightAsync(MenuCaption(page, "Content Types"), "Design, Content Definition, Content Types");
            await clock.UntilAsync(2.6);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, clock, page.Locator("[role='btn-edit-ProductBundle']"), 300, () => JumpAsync(page, "a.btn-info[href*='AddFieldsTo']", 300));
            var add = page.Locator("a.btn-info:has-text('Add Field')");
            await add.WaitForAsync();
            await clock.UntilAsync(5.6);
            await GoAsync(scene, clock, add, 300);
            await Label(page, "Display Name").WaitForAsync();
        });

        await StepAsync(scene, "add-2", async clock =>
        {
            await TypeAsync(scene, page.Locator("#DisplayName"), "Products");
            await page.Keyboard.PressAsync("Tab");
            await page.WaitForFunctionAsync("() => document.querySelector('#Name').value === 'Products'");
            await clock.UntilAsync(3.0);
            await ChooseFieldTypeAsync(scene, "Content Picker Field", "ContentPickerField");
            await clock.BeforeEndAsync(2.4);
            await GoAsync(scene, clock, page.Locator("button[value='SaveAndConfigure']"));
            await page.Locator("input[id$='_Multiple']").WaitForAsync();
        });

        await StepAsync(scene, "add-3", async clock =>
        {
            await clock.UntilAsync(0.3);
            await RingAsync(scene, page.Locator("label[for$='_Required']"), 2.4);
            await RingAsync(scene, page.Locator("label[for$='_Multiple']"), 2.4);
            await clock.BeforeEndAsync(2.4);
            await ClickAsync(scene, page.Locator("label[for$='_Multiple']"), 200);
        });

        await StepAsync(scene, "add-4", async clock =>
        {
            var wrapper = page.Locator(".ocat-wrapper").Filter(new() { Has = page.Locator("#SourceAll") }).First;
            await ScrollToAsync(scene, wrapper, 230);
            await scene.HighlightAsync(wrapper, "Contained Content Types");
            await clock.UntilAsync(9.0);
            await scene.ClearHighlightsAsync();
        });

        await StepAsync(scene, "add-5", async clock =>
        {
            var product = page.Locator("#ContentTypesContainer label").Filter(new() { HasTextRegex = new Regex(@"^\s*Product\s*$") }).First;
            await BringIntoViewAsync(scene, product, 400);
            await clock.UntilAsync(0.8);
            await ClickAsync(scene, product, 300);
        });

        await StepAsync(scene, "add-6", async clock =>
        {
            var hint = page.Locator("textarea[id$='_Hint']");
            await ScrollToAsync(scene, hint, 715);
            await TypeAsync(scene, hint, "Choose the products in this bundle.");
            await clock.BeforeEndAsync(3.4);
            await GoAsync(scene, clock, page.Locator("button.save").First, 300, async () =>
            {
                await Heading(page, "Fields").WaitForAsync();
                await JumpFieldsAsync(page);
            });
        });

        await scene.ClearHighlightsAsync();
    }

    // 03-pick: pick three products and publish.
    private static async Task PickAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems/ProductBundle");
        await scene.StartCaptureAsync();

        await StepAsync(scene, "pick-1", async clock =>
        {
            await clock.UntilAsync(1.0);
            await GoAsync(scene, clock, page.Locator("a.edit").First);
            await Picker(page, "Products").Locator(".multiselect__tags").WaitForAsync();
            await clock.UntilAsync(3.6);
            await scene.HighlightAsync(FieldWrapper(page, "Products"));
        });

        await StepAsync(scene, "pick-2", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await clock.UntilAsync(5.0);
            await PickOptionAsync(scene, clock, "Products", "pack", "Trail Backpack", 1.2, 1.6);
        });

        await StepAsync(scene, "pick-3", async clock =>
        {
            await PickOptionAsync(scene, clock, "Products", "stove", "Camp Stove", 0.5);
            await PickOptionAsync(scene, clock, "Products", "lamp", "Headlamp", 0.5);
            await page.Mouse.MoveAsync(1200, 500);
        });

        await StepAsync(scene, "pick-4", async clock =>
        {
            await clock.UntilAsync(0.8);
            await GoAsync(scene, clock, page.Locator("button.publish").First, 200);
        });

        await StepAsync(scene, "pick-5", async clock =>
        {
            await clock.OffAsync(() => scene.NavigateAsync("/bundles/weekend-camping-bundle"), 100);
            await page.Locator("div.field-type-contentpickerfield").WaitForAsync();
            await clock.UntilAsync(1.2);
            await scene.HighlightAsync(page.Locator("div.field-type-contentpickerfield"));
        });

        await StepAsync(scene, "pick-6", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await clock.UntilAsync(1.5);
            await GoAsync(scene, clock, page.Locator("div.field-type-contentpickerfield a").Filter(new() { HasText = "Camp Stove" }).First);
        });
    }

    // 04-user-field: the field Bundle Manager.
    private static async Task UserFieldAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/Edit/ProductBundle");
        await ScrollToAsync(scene, Heading(page, "Fields"), 120);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "user-1", async clock =>
        {
            await clock.UntilAsync(0.8);
            await GoAsync(scene, clock, page.Locator("a.btn-info:has-text('Add Field')"));
            await Label(page, "Display Name").WaitForAsync();
            await scene.PauseAsync(300);
            await TypeAsync(scene, page.Locator("#DisplayName"), "Bundle Manager");
            await page.Keyboard.PressAsync("Tab");
            await page.WaitForFunctionAsync("() => document.querySelector('#Name').value === 'BundleManager'");
            await clock.UntilAsync(5.5);
            await ChooseFieldTypeAsync(scene, "User Picker Field", "UserPickerField");
            await clock.BeforeEndAsync(2.8);
            await GoAsync(scene, clock, page.Locator("button[value='SaveAndConfigure']"));
            await page.Locator("input[id$='_Multiple']").WaitForAsync();
        });

        await StepAsync(scene, "user-2", async clock =>
        {
            await clock.UntilAsync(0.3);
            await RingAsync(scene, page.Locator("label[for$='_Required']"), 1.6);
            await RingAsync(scene, page.Locator("label[for$='_Multiple']"), 1.6);
            var roles = page.Locator("#userRolesVue label[for$='_DisplayAllUsers']");
            await ScrollToAsync(scene, page.Locator("#userRolesVue label.ocat-label").First, 250);
            await clock.UntilAsync(5.5);
            await ClickAsync(scene, roles, 500);
            var editor = page.Locator("#userRolesVue label").Filter(new() { HasTextRegex = new Regex(@"^\s*Editor\s*$") }).First;
            await editor.WaitForAsync();
            await ScrollToAsync(scene, page.Locator("#userRolesVue label.ocat-label").First, 180);
            await clock.UntilAsync(9.5);
            await ClickAsync(scene, editor, 300);
        });

        await StepAsync(scene, "user-3", async clock =>
        {
            await ScrollToAsync(scene, page.Locator("#DisplayMode"), 400);
            await clock.UntilAsync(1.5);
            await scene.HighlightAsync(page.Locator("#DisplayMode"));
            await clock.BeforeEndAsync(1.6);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, clock, page.Locator("button.save").First, 100, async () =>
            {
                await Heading(page, "Fields").WaitForAsync();
                await JumpFieldsAsync(page);
            });
        });

        await scene.HoldAsync(1.2);
    }

    // 04-user-pick: Bundle Manager jordan.
    private static async Task UserPickAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems/ProductBundle");
        await page.Locator("a.edit").First.ClickAsync();
        await Picker(page, "BundleManager").Locator(".multiselect__tags").WaitForAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(800);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "user-4", async clock =>
        {
            var picker = Picker(page, "BundleManager");
            await clock.UntilAsync(0.8);
            await ClickAsync(scene, picker.Locator(".multiselect__tags"), 150);
            await picker.Locator("input.multiselect__input").PressSequentiallyAsync("or", new LocatorPressSequentiallyOptions { Delay = 60 });
            var jordan = picker.Locator(".multiselect__option").Filter(new() { HasText = "jordan" }).First;
            var morgan = picker.Locator(".multiselect__option").Filter(new() { HasText = "morgan" }).First;
            await clock.OffAsync(async () =>
            {
                await jordan.WaitForAsync();
                await morgan.WaitForAsync();
            }, 100);
            Console.WriteLine("    options: " + string.Join(" | ", await picker.Locator(".multiselect__option").AllInnerTextsAsync()));
            await clock.BeforeEndAsync(2.2);
            await ClickAsync(scene, jordan, 400);
        });

        await StepAsync(scene, "user-5", async clock =>
        {
            await clock.UntilAsync(0.5);
            await GoAsync(scene, clock, page.Locator("button.publish").First, 100);
            await clock.OffAsync(() => scene.NavigateAsync("/bundles/weekend-camping-bundle"), 100);
            await page.Locator("div.field-type-userpickerfield").WaitForAsync();
            await scene.HighlightAsync(page.Locator("div.field-type-userpickerfield"));
        });

        await scene.ClearHighlightsAsync();
    }

    // 05-lucene-field: the Lucene editor of the Products field.
    private static async Task LuceneFieldAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/ContentTypes/Edit/ProductBundle");
        await ScrollToAsync(scene, Heading(page, "Fields"), 120);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "lucene-1", async clock =>
        {
            await clock.UntilAsync(0.6);
            await GoAsync(scene, clock, page.Locator("a[href*='/Fields/Products/Edit']").First, 500, async () =>
            {
                await page.Locator("#Editor").WaitForAsync();

                // The Title Pattern box (default code) is the last thing but one on this page, so a scroll alone cannot move it out of the frame:
                // blank space below the form lets the editor setting reach the top. Nothing on the screen changes.
                await page.EvaluateAsync("() => { const d = document.createElement('div'); d.style.height = '700px'; document.querySelector('form[action*=Fields]').appendChild(d); }");
            });
            await page.Locator("#Editor").WaitForAsync();
            var label = page.Locator("label.ocat-label").Filter(new() { HasText = "What type of editor" }).First;
            await clock.UntilAsync(4.0);

            // The Title Pattern box above the editor setting is in view for a moment only.
            await ScrollToAsync(scene, label, 150);
            await scene.HighlightAsync(page.Locator("#Editor"));
            await clock.UntilAsync(5.4);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(page.Locator("#Editor"), "Lucene", 300);
            await page.Locator("select[id$='_Index']").WaitForAsync();
            await ScrollToAsync(scene, label, 120);
        });

        await StepAsync(scene, "lucene-2", async clock =>
        {
            var index = page.Locator("select[id$='_Index']");
            await scene.HighlightAsync(index);
            await clock.UntilAsync(5.0);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(index, "Catalog", 600);
            await clock.UntilAsync(8.0);
            await clock.BeforeEndAsync(3.2);
            await GoAsync(scene, clock, page.Locator("button.save").First, 300, async () =>
            {
                await Heading(page, "Fields").WaitForAsync();
                await JumpFieldsAsync(page);
            });
        });
    }

    // 05-lucene-pick: search with the Lucene editor, nothing is saved.
    private static async Task LucenePickAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems/ProductBundle");
        await page.Locator("a.edit").First.ClickAsync();
        await Picker(page, "Products").Locator(".multiselect__tags").WaitForAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(800);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "lucene-3", async clock =>
        {
            var picker = Picker(page, "Products");
            await clock.UntilAsync(1.8);
            await ClickAsync(scene, picker.Locator(".multiselect__tags"), 150);
            await picker.Locator("input.multiselect__input").PressSequentiallyAsync("rain", new LocatorPressSequentiallyOptions { Delay = 60 });
            var item = picker.Locator(".multiselect__option").Filter(new() { HasText = "Rain Jacket" }).First;
            await clock.OffAsync(() => item.WaitForAsync(), 100);
        });

        await StepAsync(scene, "lucene-4", async clock =>
        {
            var input = Picker(page, "Products").Locator("input.multiselect__input");
            await input.FillAsync(string.Empty);
            await input.PressSequentiallyAsync("jacket", new LocatorPressSequentiallyOptions { Delay = 60 });
            await Task.Delay(1500);
        });
    }
}

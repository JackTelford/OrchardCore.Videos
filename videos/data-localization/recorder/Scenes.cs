using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the data-localization video: see recording-plan.md.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-editor", EditorAsync),
        ("04-translate", TranslateAsync),
        ("05-switch", SwitchAsync),
        ("06-statistics", StatisticsAsync),
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

    private static async Task AtAsync(Stopwatch watch, int milliseconds)
    {
        var remaining = milliseconds - (int)watch.ElapsedMilliseconds;

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }
    }

    // Expands each collapsed group of the left menu above a link (outermost first) and returns the link.
    private static async Task<ILocator> ExpandMenuAsync(Scene scene, string href)
    {
        var page = scene.Page;
        var link = page.Locator($"#left-nav a[href$='{href}']").First;
        await link.WaitForAsync(new() { State = WaitForSelectorState.Attached });
        var ancestors = link.Locator("xpath=ancestor::ul[@data-title]");
        var count = await ancestors.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var ul = ancestors.Nth(i);

            if (!await ul.IsVisibleAsync())
            {
                var title = await ul.GetAttributeAsync("data-title");
                var caption = ul.Locator($"xpath=preceding-sibling::figcaption | ../figcaption[.//span[normalize-space()='{title}']] | ../*/figcaption[.//span[normalize-space()='{title}']]").First;
                await scene.ClickAsync(caption, pauseAfter: 120);
            }
        }

        return link;
    }

    private static async Task OpenMenuAsync(Scene scene, string href, ILocator ready = null)
        => await GoAsync(scene, await ExpandMenuAsync(scene, href), ready);

    // Moves to a link or button, clicks it and cuts the page load out of the video.
    private static async Task GoAsync(Scene scene, ILocator link, ILocator ready = null)
    {
        await scene.MoveToAsync(link);
        await scene.OffCameraAsync(async () =>
        {
            await scene.WaitForNavigationAsync(async () => await link.ClickAsync());

            if (ready is not null)
            {
                await ready.WaitForAsync(new() { Timeout = 60000 });
            }
        }, 450);
    }

    // Holds until the toast of the click would last to the end of the narration, then clicks off camera.
    private static async Task SubmitAsync(Scene scene, Stopwatch watch, double seconds, ILocator button, ILocator ready)
    {
        await scene.MoveToAsync(button);
        await AtAsync(watch, Math.Max(300, (int)(seconds * 1000) - 3600));
        await GoAsync(scene, button, ready);
    }

    private static ILocator Editor(Scene scene) => scene.Page.Locator("#translation-editor");

    // Waits until the editor has loaded its strings.
    private static async Task EditorReadyAsync(Scene scene)
    {
        await Editor(scene).WaitForAsync(new() { Timeout = 60000 });
        await scene.Page.Locator("fieldset.translations-list legend").First.WaitForAsync(new() { Timeout = 60000 });
    }

    // Types in a field of the editor so that the last key lands the given number of seconds before the narration ends.
    private static async Task TypeTimedAsync(Scene scene, Stopwatch watch, double narration, double lastKeyBefore, ILocator field, string text, int delay = 45)
    {
        await scene.MoveToAsync(field);
        var typing = text.Length * (delay + 25);
        await AtAsync(watch, Math.Max(300, (int)((narration - lastKeyBefore) * 1000) - typing));
        await scene.ClickAsync(field, pauseAfter: 50);
        await scene.Page.Keyboard.TypeAsync(text, new() { Delay = delay });
    }

    private const string Recipe1 = """
{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.Localization", "OrchardCore.Localization.AdminCulturePicker" ] } ] }
""";

    private const string Recipe2 = """
{ "steps": [ { "name": "settings", "LocalizationSettings": { "DefaultCulture": "en-US", "SupportedCultures": [ "en-US", "fr-FR", "es-ES" ] } } ] }
""";

    private const string Recipe3 = """
{ "steps": [ { "name": "ContentDefinition", "ContentTypes": [ { "Name": "BlogPost", "DisplayName": "Blog Post", "Settings": { "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true } } } ] } ] }
""";

    private const string SeedScript = """
async () => {
  const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
  const frMap = {
    "Content Types|Blog": "Blog", "Content Types|Container": "Conteneur", "Content Types|Blockquote": "Citation", "Content Types|Paragraph": "Paragraphe",
    "Content Types|Raw Html": "HTML brut", "Content Types|Tag": "Étiquette", "Content Types|Category": "Catégorie",
    "Content Fields:BlogPost|Subtitle": "Sous-titre", "Content Fields:BlogPost|Banner Image": "Image de bannière", "Content Fields:BlogPost|Category": "Catégorie",
    "Content Fields:Article|Subtitle": "Sous-titre", "Content Fields:Article|Banner Image": "Image de bannière", "Content Fields:Blog|Banner Image": "Image de bannière",
  };
  const frSkip = [ "Content Types|Blog Post", "Content Fields:BlogPost|Tags", "Admin Menus:Admin menus|Main Menu" ];
  const esMap = { "Content Types|Blog Post": "Entrada de blog", "Content Types|Page": "Página", "Content Types|Article": "Artículo" };
  async function load(culture) {
    const response = await fetch(`/Admin/DataLocalization/GetStrings?culture=${culture}`);
    const data = await response.json();
    const all = [];
    for (const provider of data.providers) {
      all.push(...(provider.strings ?? []));
      for (const group of provider.subGroups ?? []) all.push(...group.strings);
    }
    return all;
  }
  async function save(culture, translations) {
    const response = await fetch("/Admin/DataLocalization/Save", {
      method: "POST",
      headers: { "Content-Type": "application/json", RequestVerificationToken: token },
      body: JSON.stringify({ culture, translations }),
    });
    return culture + " " + response.status + " " + translations.length;
  }
  const french = (await load("fr-FR")).map((s) => {
    const key = `${s.context}|${s.key}`;
    return { context: s.context, key: s.key, value: frSkip.includes(key) ? "" : (frMap[key] ?? s.key) };
  });
  const a = await save("fr-FR", french);
  const spanish = (await load("es-ES")).filter((s) => esMap[`${s.context}|${s.key}`]).map((s) => ({ context: s.context, key: s.key, value: esMap[`${s.context}|${s.key}`] }));
  const b = await save("es-ES", spanish);
  return a + " / " + b;
}
""";

    private static async Task ImportAsync(IPage page, string recipe)
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            try
            {
                await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
                await page.Locator(".CodeMirror").First.WaitForAsync(new() { Timeout = 5000 });
                break;
            }
            catch (Exception)
            {
                await Task.Delay(3000);
            }
        }

        await page.EvaluateAsync("json => { const cm = document.querySelector('.CodeMirror').CodeMirror; cm.setValue(json); cm.save(); }", recipe);
        await page.Locator("form button:has-text(\"Import\")").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(4000);
    }

    // Before 02-enable: the localization features, the three cultures and a creatable Blog Post.
    // After 02-enable (Data Localization enabled): the French and Spanish translations of the plan.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        var response = await page.GotoAsync(BaseUrl + "/Admin/DataLocalization/Index");

        if (response is null || response.Status == 404)
        {
            await ImportAsync(page, Recipe1);
            await Task.Delay(8000);
            await ImportAsync(page, Recipe2);
            await ImportAsync(page, Recipe3);
            Console.WriteLine("Prepared the cultures and the Blog Post type.");
        }
        else
        {
            await page.Locator("fieldset.translations-list legend").First.WaitForAsync(new() { Timeout = 60000 });
            Console.WriteLine(await page.EvaluateAsync<string>(SeedScript));
        }

        await context.CloseAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("#search-box");

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Features", box);
        });

        await scene.StepAsync("enable-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Data Localization", new() { Delay = 30 });
            await Assertions.Expect(page.Locator("#list-summary")).ToContainTextAsync("Showing 1 of");
            await scene.HighlightAsync(page.GetByRole(AriaRole.Heading, new() { Name = "Internationalization" }).First);
            var row = page.Locator("[data-filter-value]").Filter(new() { HasText = "Data Localization" }).First;
            await SubmitAsync(scene, watch, durations["enable-2"], row.GetByRole(AriaRole.Link, new() { Name = "Enable", Exact = true }), page.GetByText("The feature Data Localization was enabled.").First);
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var link = await ExpandMenuAsync(scene, "/Admin/DataLocalization/Index");
            await link.WaitForAsync();
            await scene.HighlightAsync(link);
        });
    }

    private static async Task EditorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var link = await ExpandMenuAsync(scene, "/Admin/DataLocalization/Index");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var culture = page.Locator("#culture-select");

        await scene.StepAsync("editor-1", async () =>
        {
            await GoAsync(scene, link);
            await EditorReadyAsync(scene);
            await scene.HighlightAsync(culture);
        });

        await scene.StepAsync("editor-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(culture, "fr-FR");
            await Assertions.Expect(page.Locator("fieldset.translations-list legend").First).ToContainTextAsync("translated", new() { Timeout = 60000 });

            foreach (var name in new[] { "Admin Menus", "Content Fields", "Content Types", "Permissions" })
            {
                var legend = page.Locator("fieldset.translations-list legend").Filter(new() { HasText = name }).First;
                await legend.EvaluateAsync("e => window.scrollTo({ top: e.getBoundingClientRect().top + window.scrollY - 70, behavior: 'smooth' })");
                await scene.HoldAsync(1.1);
            }
        });

        await scene.StepAsync("editor-3", async () =>
        {
            await page.EvaluateAsync("window.scrollTo({ top: 0, behavior: 'smooth' })");
            await scene.HoldAsync(0.8);
            var first = page.Locator("fieldset.translations-list").First;
            await scene.HighlightAsync(first.Locator("legend").First);
            await scene.HoldAsync(1.2);
            var row = first.Locator("tbody tr").First;
            await scene.HighlightAsync(row.Locator("td").First);
            await scene.HighlightAsync(row.Locator("input").First);
        });

        await scene.StepAsync("editor-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(page.Locator("#filter-select"), "Content Types");
            var box = page.Locator("#search-box");
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("Blog", new() { Delay = 40 });
            await scene.ClickAsync(page.Locator("#missing-only-toggle"), pauseAfter: 300);
        });
    }

    private static async Task TranslateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DataLocalization/Index");
        await EditorReadyAsync(scene);
        await scene.Page.Locator("#culture-select").SelectOptionAsync("fr-FR");
        await Assertions.Expect(scene.Page.Locator("fieldset.translations-list legend").First).ToContainTextAsync("translated", new() { Timeout = 60000 });
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var filter = page.Locator("#filter-select");
        var box = page.Locator("#search-box");

        async Task SearchAsync(string text)
        {
            await scene.ClickAsync(box, pauseAfter: 100);
            await box.PressAsync("Control+A");
            await page.Keyboard.TypeAsync(text, new() { Delay = 40 });
        }

        await scene.StepAsync("translate-1", async () =>
        {
            await scene.SelectAsync(filter, "Content Types");
            await SearchAsync("Blog Post");
        });

        await scene.StepAsync("translate-2", async () =>
        {
            var watch = Stopwatch.StartNew();
            var input = page.Locator("tr[data-text='blog post'] input").First;
            await TypeTimedAsync(scene, watch, durations["translate-2"], 3.5, input, "Article de blog");
            await scene.ClickAsync(page.Locator("button.save").First, pauseAfter: 100);
            await page.Locator(".toast.show").Filter(new() { HasText = "Translations saved successfully." }).First.WaitForAsync();
        });

        await scene.StepAsync("translate-3", async () =>
        {
            await scene.SelectAsync(filter, "Content Fields");
            await SearchAsync("Tags");
            var input = page.Locator("tr[data-text='tags'] input").First;
            await input.WaitForAsync();
            await scene.ClickAsync(input, pauseAfter: 50);
            await page.Keyboard.InsertTextAsync("É");
            await page.Keyboard.TypeAsync("tiquettes", new() { Delay = 40 });
        });

        await scene.StepAsync("translate-4", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.SelectAsync(filter, "Admin Menus");
            await SearchAsync("Main Menu");
            var input = page.Locator("tr[data-text='main menu'] input").First;
            await input.WaitForAsync();
            await TypeTimedAsync(scene, watch, durations["translate-4"], 5.0, input, "Menu principal");
        });

        await Assertions.Expect(page.GetByText("Unsaved changes")).ToBeHiddenAsync(new() { Timeout = 20000 });
        await scene.HoldAsync(0.5);
    }

    private static async Task SwitchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var picker = page.Locator("#oc-culture-picker");

        await scene.StepAsync("switch-1", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClickAsync(picker, pauseAfter: 300);
            var item = page.Locator(".language-menu-item[data-language-value='fr-FR']").First;
            await item.WaitForAsync();
            await scene.MoveToAsync(item);
            await AtAsync(watch, Math.Max(300, (int)(durations["switch-1"] * 1000) - 4000));
            await GoAsync(scene, item, picker.Filter(new() { HasText = "français" }));
        });

        await scene.StepAsync("switch-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("#left-nav").GetByText("Menu principal", new() { Exact = true }).First);
        });

        await scene.StepAsync("switch-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            var item = page.Locator(".dropdown-menu.show a").Filter(new() { HasText = "Article de blog" }).First;
            await item.WaitForAsync();
            await scene.HighlightAsync(item);
        });

        await scene.StepAsync("switch-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator(".dropdown-menu.show a").Filter(new() { HasText = "Article de blog" }).First, page.GetByText("New Article de blog").First);
            await scene.HoldAsync(1.5);
            var label = page.GetByText("Étiquettes").First;
            await label.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 120);
            await scene.HighlightAsync(label);
        });
    }

    private static async Task StatisticsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DataLocalization/Index");
        await EditorReadyAsync(scene);
        await scene.Page.Locator("#culture-select").SelectOptionAsync("fr-FR");
        await Assertions.Expect(scene.Page.Locator("fieldset.translations-list legend").First).ToContainTextAsync("translated", new() { Timeout = 60000 });
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("stats-1", async () =>
        {
            var link = page.Locator("a.btn").Filter(new() { HasText = "Statistics" }).First;
            await link.ScrollIntoViewIfNeededAsync();
            await GoAsync(scene, link, page.GetByText("Overall Progress").First);
            await scene.HighlightAsync(page.Locator(".progress").First);
        });

        await scene.StepAsync("stats-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var heading = page.GetByText("Progress by Culture").First;
            await heading.EvaluateAsync("e => window.scrollTo({ top: e.getBoundingClientRect().top + window.scrollY - 60, behavior: 'smooth' })");
            await scene.HoldAsync(1.0);
            var french = page.Locator("tr").Filter(new() { HasText = "French (France)" }).Locator(".progress").First;
            await scene.HighlightAsync(french);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("tr").Filter(new() { HasText = "English (United States)" }).Locator(".progress").First);
            await scene.HighlightAsync(page.Locator("tr").Filter(new() { HasText = "Spanish (Spain)" }).Locator(".progress").First);
        });

        await scene.StepAsync("stats-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var heading = page.GetByText("Progress by Category").First;
            await heading.EvaluateAsync("e => window.scrollTo({ top: e.getBoundingClientRect().top + window.scrollY - 60, behavior: 'smooth' })");
            await scene.HoldAsync(1.0);
            await scene.SelectAsync(page.Locator("#selected-culture"), "fr-FR");
            await page.GetByText("Permissions", new() { Exact = true }).Last.WaitForAsync();
        });
    }
}

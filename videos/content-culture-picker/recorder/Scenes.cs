using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of the content-culture-picker video: see recording-plan.md.</summary>
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
        ("03-switch", SwitchAsync),
        ("04-missing", MissingAsync),
        ("05-homepage", HomepageAsync),
        ("06-cookie", CookieAsync),
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

    // Opens a page of the left menu: expands each collapsed group above the link (outermost first), then clicks it.
    private static async Task OpenMenuAsync(Scene scene, string href, ILocator ready = null)
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

        await GoAsync(scene, link, ready);
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

    private static ILocator Heading(Scene scene, string text) => scene.Page.Locator("h1").Filter(new() { HasText = text }).First;

    private static ILocator Picker(Scene scene) => scene.Page.Locator("#oc-culture-picker");

    private static ILocator PickerMenu(Scene scene) => scene.Page.Locator(".dropdown-menu[aria-labelledby='oc-culture-picker']");

    private static ILocator PickerItem(Scene scene, string text) => PickerMenu(scene).Locator("a.dropdown-item").Filter(new() { HasText = text }).First;

    private static ILocator VisitSite(Scene scene) => scene.Page.GetByRole(AriaRole.Button, new() { Name = "Visit Site", Exact = true });

    private static ILocator Main(Scene scene) => scene.Page.Locator("main");

    // Opens the language menu of the picker and waits for it.
    private static async Task OpenPickerAsync(Scene scene)
    {
        await scene.ClickAsync(Picker(scene), pauseAfter: 100);
        await PickerMenu(scene).WaitForAsync();
        await scene.PauseAsync(600);
    }

    private const string Recipe1 = """
{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.Localization", "OrchardCore.ContentLocalization", "TheTheme" ] } ] }
""";

    private const string Recipe2 = """
{
  "steps": [
    {
      "name": "settings",
      "LocalizationSettings": { "DefaultCulture": "en", "SupportedCultures": [ "en", "fr" ], "FallBackToParentCulture": false },
      "HomeRoute": { "Action": "Display", "Controller": "Item", "Area": "OrchardCore.Contents", "ContentItemId": "39e41475c202fcff826c1e3eec" }
    },
    { "name": "themes", "site": "TheTheme" },
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        { "Name": "Article", "ContentTypePartDefinitionRecords": [ { "PartName": "LocalizationPart", "Name": "LocalizationPart", "Settings": {} } ] }
      ]
    },
    {
      "name": "content",
      "data": [
        {
          "ContentItemId": "39e41475c202fcff826c1e3eec", "ContentItemVersionId": "2d57d76fb3d6048bb12dd91c6d", "ContentType": "Article", "DisplayText": "Welcome", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Welcome" },
          "AutoroutePart": { "Path": "welcome", "SetHomepage": false },
          "HtmlBodyPart": { "Html": "<p>Welcome to Maple Street Cafe. We serve breakfast and lunch every day.</p><p><a href=\"/contact-us\">Contact us</a> | <a href=\"/opening-hours\">Opening hours</a></p>" },
          "LocalizationPart": { "LocalizationSet": "e9b1702bb1466859be63943293", "Culture": "en" }
        },
        {
          "ContentItemId": "71d8b5cc6dfecbc785e88a7f53", "ContentItemVersionId": "1ed11b37332494561d160ac3f2", "ContentType": "Article", "DisplayText": "Bienvenue", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Bienvenue" },
          "AutoroutePart": { "Path": "bienvenue", "SetHomepage": false },
          "HtmlBodyPart": { "Html": "<p>Bienvenue au Maple Street Cafe. Nous servons le petit-déjeuner et le déjeuner tous les jours.</p><p><a href=\"/nous-contacter\">Nous contacter</a> | <a href=\"/opening-hours\">Heures d'ouverture</a></p>" },
          "LocalizationPart": { "LocalizationSet": "e9b1702bb1466859be63943293", "Culture": "fr" }
        },
        {
          "ContentItemId": "da71c1433329308627f0f8ff7e", "ContentItemVersionId": "c1f920014dbd019e9531ab3dc7", "ContentType": "Article", "DisplayText": "Contact us", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Contact us" },
          "AutoroutePart": { "Path": "contact-us", "SetHomepage": false },
          "HtmlBodyPart": { "Html": "<p>Write to us at hello@example.com, or visit us during our opening hours.</p><p><a href=\"/opening-hours\">Opening hours</a></p>" },
          "LocalizationPart": { "LocalizationSet": "a37ade4f6e5ab7db36a8b77cd2", "Culture": "en" }
        },
        {
          "ContentItemId": "b39b140c4fcf7b16f7af70a91e", "ContentItemVersionId": "bce4e1e9ac443ec1ef39160f1f", "ContentType": "Article", "DisplayText": "Nous contacter", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Nous contacter" },
          "AutoroutePart": { "Path": "nous-contacter", "SetHomepage": false },
          "HtmlBodyPart": { "Html": "<p>Écrivez-nous à hello@example.com, ou passez nous voir pendant nos heures d'ouverture.</p><p><a href=\"/opening-hours\">Heures d'ouverture</a></p>" },
          "LocalizationPart": { "LocalizationSet": "a37ade4f6e5ab7db36a8b77cd2", "Culture": "fr" }
        },
        {
          "ContentItemId": "f5947e66147390029e4492b220", "ContentItemVersionId": "1659a945a34472ec9e9b737f69", "ContentType": "Article", "DisplayText": "Opening hours", "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Opening hours" },
          "AutoroutePart": { "Path": "opening-hours", "SetHomepage": false },
          "HtmlBodyPart": { "Html": "<p>We are open every day from 7:00 to 17:00.</p><p><a href=\"/\">Back to the home page</a></p>" },
          "LocalizationPart": { "LocalizationSet": "63a3ef9efb983076f5889aca45", "Culture": "en" }
        }
      ]
    }
  ]
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

    // Enables the localization features and The Default Theme, then stores the cultures and the five cafe pages (outside the capture).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        await ImportAsync(page, Recipe1);
        await Task.Delay(8000);
        await ImportAsync(page, Recipe2);
        Console.WriteLine("Seeded the cultures and the pages.");
        await context.CloseAsync();
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var box = page.Locator("#search-box");
        var row = page.Locator("[data-filter-value]").Filter(new() { HasText = "Content Culture Picker" }).First;

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Features", box);
            await scene.ClickAsync(box, pauseAfter: 100);
            await page.Keyboard.TypeAsync("content culture", new() { Delay = 30 });
            await page.GetByRole(AriaRole.Heading, new() { Name = "Internationalization" }).First.WaitForAsync();
            await row.WaitForAsync();
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.HighlightAsync(row.Locator(".admin-list-secondary"));
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator(".direct-dependencies"));
        });

        await scene.StepAsync("enable-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            await SubmitAsync(scene, watch, durations["enable-3"], row.GetByRole(AriaRole.Link, new() { Name = "Enable", Exact = true }), page.GetByText("The feature Content Culture Picker was enabled.").First);
            await box.FillAsync("content culture");
            await row.WaitForAsync();
            await scene.MoveToAsync(row);
        });
    }

    private static async Task SwitchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("switch-1", async () =>
        {
            await GoAsync(scene, VisitSite(scene), Picker(scene));
            await Heading(scene, "Welcome").WaitForAsync();
            await scene.HighlightAsync(Picker(scene));
        });

        await scene.StepAsync("switch-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Picker(scene), pauseAfter: 100);
            await PickerMenu(scene).WaitForAsync();
            await scene.HighlightAsync(PickerMenu(scene).Locator("a.dropdown-item"));
        });

        await scene.StepAsync("switch-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, PickerItem(scene, "français"), Heading(scene, "Bienvenue"));
            await Assertions.Expect(Picker(scene)).ToContainTextAsync("français");
        });

        await scene.StepAsync("switch-4", async () =>
        {
            await GoAsync(scene, Main(scene).GetByRole(AriaRole.Link, new() { Name = "Nous contacter", Exact = true }), Heading(scene, "Nous contacter"));
        });

        await scene.StepAsync("switch-5", async () =>
        {
            await OpenPickerAsync(scene);
            await GoAsync(scene, PickerItem(scene, "English"), Heading(scene, "Contact us"));
        });
    }

    private static async Task MissingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/contact-us");
        await scene.StartCaptureAsync();

        await scene.StepAsync("missing-1", async () =>
        {
            await GoAsync(scene, Main(scene).GetByRole(AriaRole.Link, new() { Name = "Opening hours", Exact = true }), Heading(scene, "Opening hours"));
        });

        await scene.StepAsync("missing-2", async () =>
        {
            await OpenPickerAsync(scene);
            await GoAsync(scene, PickerItem(scene, "français"), Heading(scene, "Opening hours"));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task HomepageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();
        var page = scene.Page;
        var option = page.GetByLabel("Homepage redirect");

        await scene.StepAsync("homepage-1", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/Settings/ContentCulturePicker", option);
        });

        await scene.StepAsync("homepage-2", async () =>
        {
            await scene.HighlightAsync(option);
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(option, pauseAfter: 300);
            await Assertions.Expect(option).ToBeCheckedAsync();
        });

        await scene.StepAsync("homepage-3", async () =>
        {
            var watch = Stopwatch.StartNew();
            await SubmitAsync(scene, watch, durations["homepage-3"], page.Locator("button.save").First, page.GetByText("Site settings updated successfully.").First);
        });

        await scene.StepAsync("homepage-4", async () =>
        {
            await GoAsync(scene, VisitSite(scene), Heading(scene, "Welcome"));
            await GoAsync(scene, Main(scene).GetByRole(AriaRole.Link, new() { Name = "Opening hours", Exact = true }), Heading(scene, "Opening hours"));
        });

        await scene.StepAsync("homepage-5", async () =>
        {
            await OpenPickerAsync(scene);
            await GoAsync(scene, PickerItem(scene, "français"), Heading(scene, "Bienvenue"));
        });
    }

    private static async Task CookieAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Settings/ContentCulturePicker");
        await scene.StartCaptureAsync();
        var page = scene.Page;

        await scene.StepAsync("cookie-1", async () =>
        {
            await scene.HighlightAsync(page.GetByLabel("Set Cookie"));
        });

        await scene.StepAsync("cookie-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".hint").Filter(new() { HasText = "Sets the localization cookie when switching cultures" }).First);
        });

        await scene.StepAsync("cookie-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenMenuAsync(scene, "/Admin/Settings/ContentRequestCultureProvider", page.GetByLabel("Set Cookie"));
        });

        await scene.StepAsync("cookie-4", async () =>
        {
            await scene.HighlightAsync(page.Locator(".hint").Filter(new() { HasText = "Sets the localization cookie based on the LocalizationPart culture" }).First);
        });

        await scene.StepAsync("cookie-5", async () =>
        {
            await scene.ClearHighlightsAsync();
        });
    }
}

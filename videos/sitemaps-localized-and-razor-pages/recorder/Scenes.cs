using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Sitemaps", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("02-sitemap", SitemapAsync),
        ("03-types", TypesAsync),
        ("03-sitemap", PagesAsync),
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

    private const string RecipeFeatures = """
        {
          "steps": [
            {
              "name": "feature",
              "enable": [
                "OrchardCore.Localization",
                "OrchardCore.ContentLocalization",
                "OrchardCore.Sitemaps",
                "OrchardCore.ContentLocalization.Sitemaps",
                "OrchardCore.Sitemaps.RazorPages"
              ]
            }
          ]
        }
        """;

    private const string RecipeContent = """
        {
          "steps": [
            {
              "name": "settings",
              "BaseUrl": "https://www.example.com",
              "LocalizationSettings": { "DefaultCulture": "en", "SupportedCultures": [ "en", "fr" ] }
            },
            {
              "name": "ContentDefinition",
              "ContentTypes": [
                {
                  "Name": "Article",
                  "DisplayName": "Article",
                  "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
                  "ContentTypePartDefinitionRecords": [
                    { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
                    {
                      "PartName": "AutoroutePart", "Name": "AutoroutePart",
                      "Settings": {
                        "ContentTypePartSettings": { "Position": "1" },
                        "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "{{ Model.ContentItem | display_text | slugify }}" }
                      }
                    },
                    { "PartName": "LocalizationPart", "Name": "LocalizationPart", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
                  ]
                },
                {
                  "Name": "DecoupledBlogPost",
                  "DisplayName": "Decoupled Blog Post",
                  "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
                  "ContentTypePartDefinitionRecords": [
                    { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } }
                  ]
                }
              ],
              "ContentParts": []
            },
            {
              "name": "content",
              "Data": [
                {
                  "ContentItemId": "4zn3x0pdqxryon8j1dybv7zxg0", "ContentType": "Article", "DisplayText": "Welcome to Maple Street Market",
                  "Latest": true, "Published": true, "Author": "admin",
                  "TitlePart": { "Title": "Welcome to Maple Street Market" },
                  "AutoroutePart": { "Path": "en/welcome" },
                  "LocalizationPart": { "LocalizationSet": "4vqjxxvbothd0wtflhbmuaptt0", "Culture": "en" }
                },
                {
                  "ContentItemId": "42vdhldsun7av8bppxx71c9jeq", "ContentType": "Article", "DisplayText": "Bienvenue au Maple Street Market",
                  "Latest": true, "Published": true, "Author": "admin",
                  "TitlePart": { "Title": "Bienvenue au Maple Street Market" },
                  "AutoroutePart": { "Path": "fr/bienvenue" },
                  "LocalizationPart": { "LocalizationSet": "4vqjxxvbothd0wtflhbmuaptt0", "Culture": "fr" }
                },
                {
                  "ContentItemId": "4qa3qifpuw23ydesok50llsd9d", "ContentType": "DecoupledBlogPost", "DisplayText": "Spring Planting Calendar",
                  "Latest": true, "Published": true, "Author": "admin",
                  "TitlePart": { "Title": "Spring Planting Calendar" }
                },
                {
                  "ContentItemId": "42veyx0gsaxmknuj0ewu1rfc04", "ContentType": "DecoupledBlogPost", "DisplayText": "Weekend Market Hours",
                  "Latest": true, "Published": true, "Author": "admin",
                  "TitlePart": { "Title": "Weekend Market Hours" }
                }
              ]
            }
          ]
        }
        """;

    private static async Task WaitSiteAsync()
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        for (var i = 0; i < 60; i++)
        {
            try
            {
                var response = await client.GetAsync(BaseUrl + "/");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(2000);
        }
    }

    private static async Task ImportJsonAsync(IPage page, string json)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", json);
        await page.Locator("button.import").First.ClickAsync();
        await page.WaitForFunctionAsync("() => document.body.innerText.includes('Recipe imported successfully!')");
    }

    /// <summary>Provisioning, outside the capture: the two recipes of the plan, then the sitemap Site Pages with an all-content-types source.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        await ImportJsonAsync(page, RecipeFeatures);
        await WaitSiteAsync();
        await Task.Delay(6000);
        await ImportJsonAsync(page, RecipeContent);
        await WaitSiteAsync();
        await Task.Delay(3000);

        await page.GotoAsync(BaseUrl + "/Admin/Sitemaps/Create");
        await page.Locator("#Name").FillAsync("Site Pages");
        await page.Locator("#Path").FillAsync("sitemap.xml");
        await page.Locator("button.create").ClickAsync();
        await page.WaitForURLAsync(u => u.EndsWith("/Admin/Sitemaps/List", StringComparison.OrdinalIgnoreCase));
        var row = page.Locator(".list-group-item").Filter(new() { HasTextString = "Site Pages" }).First;
        await row.Locator("a:has-text('Manage Sources')").First.ClickAsync();
        await page.Locator("button[data-bs-target='#modalSources']").ClickAsync();
        await page.Locator("#modalSources.show").WaitForAsync();
        await page.Locator("#modalSources .card").Filter(new() { HasTextString = "Sitemap Content Types" }).First.Locator("a:has-text('Add')").First.ClickAsync();
        await page.WaitForURLAsync(u => u.Contains("SitemapSource/Create"));
        await page.Locator("button.create").ClickAsync();
        await page.WaitForURLAsync(u => u.Contains("/Admin/Sitemaps/Display"));

        var xml = await new HttpClient(new HttpClientHandler { UseProxy = false }).GetStringAsync(BaseUrl + "/sitemap.xml");
        Console.WriteLine(xml.Length > 1500 ? xml[..1500] : xml);
        var post = await new HttpClient(new HttpClientHandler { UseProxy = false }).GetStringAsync(BaseUrl + "/blogpost/4qa3qifpuw23ydesok50llsd9d");
        Console.WriteLine(post.Contains("This is the blog post: Spring Planting Calendar") ? "Razor page OK" : "Razor page MISSING");

        foreach (var path in new[] { "/Admin/Features", "/Admin/Sitemaps/List", "/Admin/SitemapCache/List" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Seeded.");
    }

    // The recorder's cursor overlay adds a style element that an XML page shows as plain text: remove it there.
    private const string XmlCleanScript = """
        (() => {
          const clean = (n) => {
            if (n.nodeType === 1 && !document.contentType.includes('html')
              && (n.id === 'demo-cursor' || n.localName === 'style' || (n.classList && n.classList.contains('demo-ripple')))) { n.remove(); }
          };
          new MutationObserver((records) => records.forEach((r) => r.addedNodes.forEach(clean))).observe(document, { childList: true, subtree: true });
        })();
        """;

    private static async Task<Scene> NewXmlSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

        return scene;
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<Scene, Task> ready = null)
    {
        var scene = await NewXmlSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    // Shows an XML page zoomed so that it reads in the video, then, on camera, a tiny zoom change makes the screencast send a frame.
    private static async Task ZoomXmlAsync(IPage page, string zoom = "1.5")
    {
        await page.EvaluateAsync("z => { (document.body || document.documentElement).style.zoom = z; window.scrollTo(0, 0); }", zoom);
        await Task.Delay(500);
    }

    private static async Task NudgeXmlAsync(Scene scene, IPage page, string zoom = "1.501")
    {
        await page.EvaluateAsync("z => { (document.body || document.documentElement).style.zoom = z; }", zoom);
        await scene.PauseAsync(300);
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task WaitTextAsync(IPage page, string text, int timeout = 60_000)
        => await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = timeout });

    // Clicks a link or button that loads a new page; the load is cut out of the video (see Scene.OffCameraAsync).
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
    }

    // Opens a page during the capture, with the load cut out of the video.
    private static Task VisitAsync(Scene scene, string path)
        => scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(BaseUrl + path)), leadMilliseconds: 250);

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Row(IPage page, string text) => page.Locator(".list-group-item, tr").Filter(new() { HasTextString = text }).First;

    private static async Task OpenAdminGroupAsync(IPage page, string name)
    {
        var group = page.Locator("#left-nav").GetByText(name, new() { Exact = true }).First;
        await group.ClickAsync();
        await Task.Delay(500);
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Features", s => s.Page.Locator("#search-box").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("loc-3", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "sitemaps", delay: 40);
            var feature = page.GetByText("Localized Content Item Sitemaps", new() { Exact = true });
            await feature.WaitForAsync();
            await scene.HighlightAsync(feature);
        });
    }

    private static async Task SitemapAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Sitemaps/List", s => s.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Site Pages" }).First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("loc-4", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.WaitForNavigationAsync(() => page.GotoAsync("view-source:" + BaseUrl + "/sitemap.xml"));
                await page.GetByText("hreflang").First.WaitForAsync();
                await Task.Delay(800);
                await ZoomXmlAsync(page, "1.3");
            }, 100);
            await NudgeXmlAsync(scene, page, "1.301");
            await scene.HighlightAsync(page.GetByText("hreflang").First);
        });

        await scene.StepAsync("loc-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByText("xmlns:xhtml").First);
        });
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Sitemaps/List", s => s.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Site Pages" }).First.WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("raz-4", async () =>
        {
            await GoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Manage Sources" }).First);
            await page.Locator("li.list-group-item").First.WaitForAsync();
            await GoAsync(scene, page.Locator("li.list-group-item").GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }).First);
            await page.Locator("input.content-types-sitemap-index-all").WaitForAsync();
            await scene.ClickAsync(page.Locator("label[for='ContentTypesSitemapSource_IndexAll']"), pauseAfter: 300);
            var selection = page.Locator("#index-selected-row");
            await selection.GetByText("Select Content Types").WaitForAsync();
            await scene.HighlightAsync(selection.GetByText("Decoupled Blog Post", new() { Exact = true }));
        });
    }

    private static async Task PagesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin", async s =>
        {
            await s.Page.GotoAsync("view-source:" + BaseUrl + "/sitemap.xml");
            await s.Page.GetByText("/blogpost/").First.WaitForAsync();
            await ZoomXmlAsync(s.Page, "1.3");
        });
        var page = scene.Page;
        await NudgeXmlAsync(scene, page, "1.301");

        await scene.StepAsync("raz-5", async () =>
        {
            await scene.HighlightAsync(page.GetByText("/blogpost/").First);
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the editors.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}

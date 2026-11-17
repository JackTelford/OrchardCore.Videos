using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Product catalog", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-product", ProductAsync),
        ("02-term-type", TermTypeAsync),
        ("03-taxonomy", TaxonomyAsync),
        ("03-connect", ConnectAsync),
        ("04-slugify", SlugifyAsync),
        ("04-library", LibraryAsync),
        ("05-product", ProductsAsync),
        ("06-features", FeaturesAsync),
        ("06-index", IndexAsync),
        ("06-settings", SettingsAsync),
        ("06-permission", PermissionAsync),
        ("06-find", FindAsync),
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

    private static string Pictures { get; } = Environment.GetEnvironmentVariable("VIDEO_PICTURES") ?? "C:/ocv/s6/product-pictures";

    /// <summary>Nothing to seed in the site: the three pictures are files outside git. Warms the pages up once.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var path in new[] { "/Admin", "/Admin/ContentTypes/List", "/Admin/Contents/ContentItems", "/Admin/Features", "/Admin/Media", "/Admin/Roles/Index" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Pages warmed up.");
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 650);
        return scene;
    }

    private static async Task GoOffAsync(Scene scene, ILocator locator, int lead = 250)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: lead);
    }

    private static ILocator Nav(IPage page, string href) => page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator NavText(IPage page, string text) => page.Locator("#left-nav a").Filter(new() { HasTextString = text }).First;

    private static ILocator Save(IPage page) => page.Locator("button.save").First;

    private static async Task AddFieldAsync(Scene scene, string name, string type, bool configure)
    {
        var page = scene.Page;
        await GoAsync(scene, page.Locator("a[href*='AddFieldsTo/Product']").First);
        await scene.TypeAsync(page.Locator("input[data-displayname]").First, name, delay: 50);
        await scene.ClickAsync(page.GetByLabel(type, new() { Exact = true }).First, pauseAfter: 300);
        await GoAsync(scene, page.Locator(configure ? "button[value='SaveAndConfigure']" : "button[value='Save']").First);
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

    private static async Task ProductAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("product-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Design");
            await OpenAdminGroupAsync(page, "Content Definition");
            await GoAsync(scene, Nav(page, "/Admin/ContentTypes/List"));
            await GoAsync(scene, page.Locator("a#btnCreate, a[href$='/Admin/ContentTypes/Create']").First);
            await page.Locator("input[data-displayname]").First.WaitForAsync();
        });

        await scene.StepAsync("product-2", async () =>
        {
            await scene.TypeAsync(page.Locator("input[data-displayname]").First, "Product", delay: 60);
            await page.Keyboard.PressAsync("Tab");
            await GoAsync(scene, page.Locator("button.create").First);
            await WaitTextAsync(page, "has been created");
        });

        await scene.StepAsync("product-3", async () =>
        {
            await scene.ClickAsync(page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("label.form-check-label:text-is('Title')") }).First.Locator("label").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("#add-selected-parts-button"));
            await page.Locator("#parts").WaitForAsync();
        });

        await scene.StepAsync("product-4", async () =>
        {
            await ShowAsync(scene, page.Locator("#parts li, #parts .list-group-item").Filter(new() { HasTextString = "Contains the fields" }).First, 1500);
            await GoAsync(scene, page.Locator("a[href*='AddFieldsTo/Product']").First);
            await page.Locator("input[data-displayname]").First.WaitForAsync();
        });

        await scene.StepAsync("product-5", async () =>
        {
            await scene.TypeAsync(page.Locator("input[data-displayname]").First, "Price", delay: 50);
            await scene.ClickAsync(page.GetByLabel("Numeric Field", new() { Exact = true }).First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("button[value='SaveAndConfigure']").First);
            await page.GetByLabel("Required").First.WaitForAsync();
        });

        await scene.StepAsync("product-6", async () =>
        {
            await scene.ClickAsync(page.GetByLabel("Required").First, pauseAfter: 300);
            var scale = page.GetByLabel("Scale").First;
            await scene.ClickAsync(scale, pauseAfter: 150);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("2", new() { Delay = 50 });
            await page.Keyboard.PressAsync("Tab");
            await GoAsync(scene, Save(page));
            await WaitTextAsync(page, "field settings have been saved");
        });

        await scene.StepAsync("product-7", async () =>
        {
            await AddFieldAsync(scene, "Image", "Media Field", false);
            await WaitTextAsync(page, "has been added");
        });

        await scene.StepAsync("product-8", async () =>
        {
            await ShowAsync(scene, page.GetByRole(AriaRole.Heading, new() { Name = "Fields" }).First, 1200);
            await ShowAsync(scene, page.Locator("#parts").First, 1200);
            await GoAsync(scene, page.Locator("button.save[name='submit.Save'][value='Save']").First);
            await WaitTextAsync(page, "Content type updated successfully.");
        });
    }

    private static async Task TermTypeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/ContentTypes/List");
        var page = scene.Page;

        await scene.StepAsync("term-1", async () =>
        {
            await GoAsync(scene, page.Locator("a#btnCreate, a[href$='/Admin/ContentTypes/Create']").First);
            await scene.TypeAsync(page.Locator("input[data-displayname]").First, "Product Category", delay: 50);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("term-2", async () =>
        {
            await GoAsync(scene, page.Locator("button.create").First);
            await scene.ClickAsync(page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("label.form-check-label:text-is('Title')") }).First.Locator("label").First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("#add-selected-parts-button"));
            await Save(page).WaitForAsync();
            await GoAsync(scene, page.Locator("button.save[name='submit.Save'][value='Save']").First);
            await WaitTextAsync(page, "Content type updated successfully.");
        });
    }

    private static async Task TaxonomyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentItems");
        var page = scene.Page;
        ILocator AddTerm() => page.GetByRole(AriaRole.Link, new() { Name = "Add Product Category" }).Or(page.GetByRole(AriaRole.Button, new() { Name = "Add Product Category" })).First;

        async Task AddTermAsync(string title)
        {
            await GoAsync(scene, AddTerm());
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, title, delay: 40);
            await GoAsync(scene, page.Locator("button.publish").First);
            await page.GetByText(title, new() { Exact = true }).First.WaitForAsync();
        }

        await scene.StepAsync("tax-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Taxonomy" }).First);
        });

        await scene.StepAsync("tax-2", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Product Categories", delay: 40);
            await scene.SelectAsync(page.GetByLabel("Term Content Type"), "Product Category");
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Taxonomy has been published.");
        });

        await scene.StepAsync("tax-3", async () =>
        {
            var row = page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "Product Categories" }).First;
            await GoAsync(scene, row.Locator("a.edit").First);
            await AddTerm().WaitForAsync();
            await scene.HighlightAsync(AddTerm());
        });

        await scene.StepAsync("tax-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await AddTermAsync("Backpacks");
        });

        await scene.StepAsync("tax-5", async () =>
        {
            await AddTermAsync("Tents");
            await AddTermAsync("Sleeping Bags");
            var row = page.Locator(".list-group-item, li").Filter(new() { HasTextString = "Tents" }).Last;
            await ShowAsync(scene, row.GetByRole(AriaRole.Link, new() { Name = "Add" }).First, 1500);
        });

        await scene.StepAsync("tax-6", async () =>
        {
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Taxonomy has been published.");
        });
    }

    private static async Task ConnectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/ContentTypes/List");
        var page = scene.Page;

        await scene.StepAsync("conn-1", async () =>
        {
            await page.Locator("a[role='btn-edit-Product']").ScrollToMiddleAsync();
            await GoAsync(scene, page.Locator("a[role='btn-edit-Product']"));
            await GoAsync(scene, page.Locator("a[href*='AddFieldsTo/Product']").First);
            await page.Locator("input[data-displayname]").First.WaitForAsync();
        });

        await scene.StepAsync("conn-2", async () =>
        {
            await scene.TypeAsync(page.Locator("input[data-displayname]").First, "Category", delay: 50);
            await scene.ClickAsync(page.GetByLabel("Taxonomy Field", new() { Exact = true }).First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("button[value='SaveAndConfigure']").First);
            await page.Locator("select").First.WaitForAsync();
        });

        await scene.StepAsync("conn-3", async () =>
        {
            await scene.SelectAsync(page.Locator("select").First, "Product Categories");
            await scene.ClickAsync(page.GetByLabel("Unique").First, pauseAfter: 300);
            await GoAsync(scene, Save(page));
            await WaitTextAsync(page, "field settings have been saved");
        });

        await scene.StepAsync("conn-4", async () =>
        {
            await scene.HighlightAsync(page.Locator("#fields, h3:has-text('Fields') + *").First);
        });
    }

    private static async Task SlugifyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("slug-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, Nav(page, "/Admin/Features"));
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 150);
            await page.Keyboard.TypeAsync("Media Slugify", new() { Delay = 50 });
            var enable = page.Locator("#btn-enable-OrchardCore_Media_Slugify");
            await enable.WaitForAsync();
            await GoOffAsync(scene, enable);
            await WaitTextAsync(page, "was enabled", 120_000);
        });
    }

    private static async Task LibraryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Media");
        var page = scene.Page;

        await scene.StepAsync("lib-1", async () =>
        {
            await page.Locator("#folder-tree").WaitForAsync();
            await scene.ClickAsync(page.Locator("#folder-tree a[title='Folder Action']").First, pauseAfter: 400);
            await scene.ClickAsync(page.GetByText("Create a subfolder").First, pauseAfter: 400);
            await page.Locator("input[name='create-folder']").WaitForAsync();
        });

        await scene.StepAsync("lib-2", async () =>
        {
            await scene.TypeAsync(page.Locator("input[name='create-folder']"), "Product Photos", delay: 50);
            await scene.ClickAsync(page.Locator("#btn-submit"), pauseAfter: 400);
            await page.Locator("#folder-tree").GetByText("product-photos").First.WaitForAsync();
        });

        await scene.StepAsync("lib-3", async () =>
        {
            await scene.ClickAsync(page.Locator("#folder-tree").GetByText("product-photos").First, pauseAfter: 400);
            await ShowAsync(scene, page.Locator("label.upload-button").First, 1200);
            await page.Locator("#fileupload").SetInputFilesAsync(new[] { Pictures + "/Trail Backpack.jpg", Pictures + "/Two-Person Tent.jpg", Pictures + "/Down Sleeping Bag.jpg" });
            await page.GetByText("down-sleeping-bag.jpg").First.WaitForAsync();
            await page.GetByText("trail-backpack.jpg").First.WaitForAsync();
            await page.GetByText("two-person-tent.jpg").First.WaitForAsync();
        });
    }

    private static async Task FillProductAsync(Scene scene, string title, string price, string picture, string category, bool publish = true)
    {
        var page = scene.Page;
        await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, title, delay: 40);
        await scene.TypeAsync(page.GetByLabel("Price", new() { Exact = true }).First, price, delay: 40);
        await page.Keyboard.PressAsync("Tab");
        await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add media" }).First, pauseAfter: 500);
        await page.Locator(".mf-vfm").First.WaitForAsync();
        await scene.ClickAsync(page.Locator(".mf-vfm").GetByText("product-photos").First, pauseAfter: 500);
        await scene.ClickAsync(page.Locator(".mf-vfm").GetByText(picture).First.Locator("xpath=ancestor::*[.//input[@type='checkbox']][1]//input[@type='checkbox']").First, pauseAfter: 400);
        await scene.ClickAsync(page.Locator(".mf-vfm").GetByRole(AriaRole.Button, new() { Name = "OK" }).First, pauseAfter: 500);
        await page.Locator("img[src*='" + picture + "']").First.WaitForAsync();
        await scene.ClickAsync(page.GetByLabel(category, new() { Exact = true }).First, pauseAfter: 300);
    }

    private static async Task ProductsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Contents/ContentItems");
        var page = scene.Page;

        await scene.StepAsync("add-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Product" }).First);
            await page.GetByLabel("Price", new() { Exact = true }).First.WaitForAsync();
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Trail Backpack", delay: 40);
            await scene.TypeAsync(page.GetByLabel("Price", new() { Exact = true }).First, "89", delay: 40);
            await page.Keyboard.PressAsync("Tab");
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add media" }).First, pauseAfter: 500);
            await page.Locator(".mf-vfm").First.WaitForAsync();
            await scene.ClickAsync(page.Locator(".mf-vfm").GetByText("product-photos").First, pauseAfter: 500);
            await scene.ClickAsync(page.Locator(".mf-vfm").GetByText("trail-backpack.jpg").First.Locator("xpath=ancestor::*[.//input[@type='checkbox']][1]//input[@type='checkbox']").First, pauseAfter: 400);
            await scene.ClickAsync(page.Locator(".mf-vfm").GetByRole(AriaRole.Button, new() { Name = "OK" }).First, pauseAfter: 500);
            await page.Locator("img[src*='trail-backpack']").First.WaitForAsync();
        });

        await scene.StepAsync("add-4", async () =>
        {
            var radio = page.GetByLabel("Backpacks", new() { Exact = true }).First;
            await radio.ScrollToMiddleAsync();
            await scene.ClickAsync(radio, pauseAfter: 300);
        });

        await scene.StepAsync("add-5", async () =>
        {
            await ShowAsync(scene, page.Locator("button.draft").First, 1500);
            await GoAsync(scene, page.Locator("button.publish").First);
            await WaitTextAsync(page, "Your Product has been published.");
        });

        await scene.StepAsync("add-6", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                var second = await scene.Context.NewPageAsync();
                second.SetDefaultTimeout(60_000);
                foreach (var (title, price, picture, category) in new[] { ("Two-Person Tent", "199", "two-person-tent.jpg", "Tents"), ("Down Sleeping Bag", "129", "down-sleeping-bag.jpg", "Sleeping Bags") })
                {
                    await second.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Product/Create");
                    await second.GetByLabel("Title", new() { Exact = true }).First.FillAsync(title);
                    await second.GetByLabel("Price", new() { Exact = true }).First.FillAsync(price);
                    await second.GetByRole(AriaRole.Button, new() { Name = "Add media" }).First.ClickAsync();
                    await second.Locator(".mf-vfm").First.WaitForAsync();
                    await second.Locator(".mf-vfm").GetByText("product-photos").First.ClickAsync();
                    await second.Locator(".mf-vfm").GetByText(picture).First.Locator("xpath=ancestor::*[.//input[@type='checkbox']][1]//input[@type='checkbox']").First.CheckAsync();
                    await second.Locator(".mf-vfm").GetByRole(AriaRole.Button, new() { Name = "OK" }).First.ClickAsync();
                    await second.Locator("img[src*='" + picture.Replace(".jpg", "") + "']").First.WaitForAsync();
                    await second.GetByLabel(category, new() { Exact = true }).First.CheckAsync();
                    await second.Locator("button.publish").First.ClickAsync();
                    await second.WaitForFunctionAsync("() => document.body.innerText.includes('has been published')");
                }

                await second.CloseAsync();
                await page.BringToFrontAsync();
            }, leadMilliseconds: 250);
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Product");
            await page.GetByText("Down Sleeping Bag").First.WaitForAsync();
        });
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Features");
        var page = scene.Page;

        await scene.StepAsync("sf-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 150);
            await page.Keyboard.TypeAsync("Lucene", new() { Delay = 50 });
            var enable = page.Locator("#btn-enable-OrchardCore_Lucene");
            await enable.WaitForAsync();
            await GoOffAsync(scene, enable);
            await WaitTextAsync(page, "was enabled", 120_000);
        });

        await scene.StepAsync("sf-2", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 150);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("Search", new() { Delay = 50 });
            var enable = page.Locator("#btn-enable-OrchardCore_Search");
            await enable.WaitForAsync();
            await GoOffAsync(scene, enable);
            await WaitTextAsync(page, "was enabled", 120_000);
        });
    }

    private static async Task IndexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("idx-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Search");
            await GoAsync(scene, NavText(page, "Indexes"));
            await scene.ClickAsync(page.Locator("button.create[data-bs-target='#modalAddIndex']").First, pauseAfter: 500);
            var card = page.Locator("#modalAddIndex .card").Filter(new() { HasTextString = "Content in Lucene" }).First;
            await card.WaitForAsync();
            await ShowAsync(scene, card, 1500);
            await GoAsync(scene, card.Locator("a.btn-primary").First);
        });

        await scene.StepAsync("idx-2", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Index Name").First, "products", delay: 50);
            await scene.ClickAsync(page.GetByLabel("Product", new() { Exact = true }).First, pauseAfter: 300);
            var latest = page.GetByLabel("Index Latest version");
            await latest.ScrollToMiddleAsync();
            await ShowAsync(scene, latest, 1500);
            await GoAsync(scene, page.Locator("button[type='submit'].btn-primary").First);
            await WaitTextAsync(page, "An index has been created successfully.");
        });

        await scene.StepAsync("idx-3", async () =>
        {
            await ShowAsync(scene, page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "products" }).First, 1500);
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 60; i++)
                {
                    var response = await page.APIRequest.GetAsync(BaseUrl + "/search/products?terms=backpack");
                    if ((await response.TextAsync()).Contains("Trail Backpack"))
                    {
                        return;
                    }

                    await Task.Delay(2000);
                }
            }, leadMilliseconds: 250);
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("set-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Settings");
            await page.Locator("#left-nav").GetByText("Search", new() { Exact = true }).Last.ClickAsync();
            await Task.Delay(500);
            await GoAsync(scene, Nav(page, "/Admin/Settings/search"));
            await page.GetByLabel("Default search index").WaitForAsync();
        });

        await scene.StepAsync("set-2", async () =>
        {
            await scene.SelectAsync(page.GetByLabel("Default search index"), "products");
            await scene.TypeAsync(page.GetByLabel("Placeholder"), "Search products", delay: 40);
            await GoAsync(scene, Save(page));
            await WaitTextAsync(page, "Site settings updated successfully.");
        });
    }

    private static async Task PermissionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Settings/search");
        var page = scene.Page;

        await scene.StepAsync("perm-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Access Control");
            await GoAsync(scene, Nav(page, "/Admin/Roles/Index"));
            await GoAsync(scene, page.Locator("a.edit[href$='/Roles/Edit/Anonymous']").First);
            await page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("perm-2", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "products", delay: 50);
            await scene.ClickAsync(page.Locator("input[name='Checkbox.QueryIndex_products'], input[id='Checkbox.QueryIndex_products']").First, pauseAfter: 400);
            await GoAsync(scene, page.Locator("button.save[name='submit'][value='Save']").First);
            await WaitTextAsync(page, "Role updated successfully.");
        });
    }

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/");
        var page = scene.Page;

        await scene.StepAsync("find-1", async () =>
        {
            await scene.NavigateAsync("/search");
            await scene.TypeAsync(page.Locator("input[name='Terms']").First, "backpack", delay: 50);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Keyboard.PressAsync("Enter")), leadMilliseconds: 250);
            await page.GetByText("Trail Backpack").First.WaitForAsync();
        });

        await scene.StepAsync("find-2", async () =>
        {
            var result = page.GetByText("Trail Backpack").First;
            await ShowAsync(scene, result, 1500);
            await GoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Trail Backpack" }).First);
        });

        await scene.StepAsync("find-3", async () =>
        {
            await ShowAsync(scene, page.GetByText("89").First, 1500);
            await ShowAsync(scene, page.Locator("img[src*='trail-backpack']").First, 1500);
            await scene.HighlightAsync(page.GetByText("Backpacks").First);
        });

        await scene.StepAsync("find-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/search");
            await scene.TypeAsync(page.Locator("input[name='Terms']").First, "kayak", delay: 50);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => page.Keyboard.PressAsync("Enter")), leadMilliseconds: 250);
            await page.GetByText("There are no such results.").First.WaitForAsync();
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

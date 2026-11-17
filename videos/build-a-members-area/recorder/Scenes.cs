using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Members area", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-register", RegisterAsync),
        ("03-type", TypeAsync2),
        ("04-role", RoleAsync),
        ("05-lock", LockAsync),
        ("06-page", PageAsync),
        ("07-signup", SignupAsync),
        ("08-approve", ApproveAsync),
        ("09-member", MemberAsync),
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

    private const string SeedJson = """
        {
          "steps": [
            { "name": "feature", "enable": ["TheTheme"] },
            { "name": "themes", "site": "TheTheme" },
            {
              "name": "ContentDefinition",
              "ContentTypes": [
                {
                  "Name": "Page",
                  "DisplayName": "Page",
                  "Settings": {
                    "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true, "Securable": true }
                  },
                  "ContentTypePartDefinitionRecords": [
                    { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
                    {
                      "PartName": "AutoroutePart", "Name": "AutoroutePart",
                      "Settings": {
                        "ContentTypePartSettings": { "Position": "1" },
                        "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": true }
                      }
                    },
                    { "PartName": "HtmlBodyPart", "Name": "HtmlBodyPart", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
                  ]
                },
                {
                  "Name": "MembersPage",
                  "DisplayName": "Members Page",
                  "Settings": {
                    "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true }
                  },
                  "ContentTypePartDefinitionRecords": [
                    { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
                    {
                      "PartName": "AutoroutePart", "Name": "AutoroutePart",
                      "Settings": {
                        "ContentTypePartSettings": { "Position": "1" },
                        "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": true }
                      }
                    },
                    { "PartName": "HtmlBodyPart", "Name": "HtmlBodyPart", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
                  ]
                }
              ]
            },
            {
              "name": "content",
              "Data": [
                {
                  "ContentItemId": "4x7k2m9q5w3e8r1t6y0u4i2o9p",
                  "ContentType": "Page",
                  "DisplayText": "Welcome",
                  "Latest": true,
                  "Published": true,
                  "TitlePart": { "Title": "Welcome" },
                  "AutoroutePart": { "Path": "welcome", "SetHomepage": false },
                  "HtmlBodyPart": { "Html": "<p>Welcome to the site. Everyone can read this page.</p><p><a href=\"/members-lounge\">Members Lounge</a></p>" }
                }
              ]
            },
            {
              "name": "settings",
              "HomeRoute": { "Action": "Display", "Controller": "Item", "Area": "OrchardCore.Contents", "ContentItemId": "4x7k2m9q5w3e8r1t6y0u4i2o9p" }
            }
          ]
        }
        """;

    private static string MemberPassword { get; } = Environment.GetEnvironmentVariable("VIDEO_MEMBER_PASSWORD") ?? "Member-demo-2";

    /// <summary>The seed: the default theme, the two types, the Welcome page and the home route. Outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", SeedJson);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!", 120_000);
        await Task.Delay(4000);

        foreach (var path in new[] { "/", "/Admin/Features", "/Admin/Roles/Index", "/Admin/ContentTypes/List", "/Admin/Contents/ContentItems", "/Admin/Users/Index" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Seed done.");
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, bool visitor = false)
    {
        var scene = visitor
            ? await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "no-state.json"), Path.GetDirectoryName(directory), durations)
            : await NewSceneAsync(browser, directory, durations);
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

    private static ILocator Save(IPage page) => page.Locator("button.save, button.primaryAction.save").First;

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

    private static async Task RegisterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("register-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Tools");
            await GoAsync(scene, Nav(page, "/Admin/Features"));
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 150);
            await page.Keyboard.TypeAsync("registration", new() { Delay = 30 });
            var enable = page.Locator("a[id^='btn-enable-'][id*='Users_Registration']").First;
            await enable.WaitForAsync();
            await ShowAsync(scene, page.Locator(".list-group-item").Filter(new() { Has = enable }).First, 1200);
            await GoOffAsync(scene, enable);
            await page.Locator("#search-box").WaitForAsync();
            await scene.MoveToAsync(900, 650);
        });

        await scene.StepAsync("register-2", async () =>
        {
            await OpenAdminGroupAsync(page, "Settings");
            await OpenAdminGroupAsync(page, "Security");
            await GoAsync(scene, Nav(page, "/Admin/Settings/userRegistration"));
            await scene.HighlightAsync(page.Locator("label").Filter(new() { HasTextString = "Users must verify their email address" }).First);
        });

        await scene.StepAsync("register-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, page.Locator("label").Filter(new() { HasTextString = "Use site theme for registration page" }).First, 1500);
            await scene.HighlightAsync(page.Locator("label").Filter(new() { HasTextString = "Users must be approved before they can log in" }).First);
        });

        await scene.StepAsync("register-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.GetByLabel("Users must be approved before they can log in"), pauseAfter: 400);
            await GoAsync(scene, Save(page));
            await WaitTextAsync(page, "Site settings updated successfully.");
        });
    }

    private static async Task TypeAsync2(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("type-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Design");
            await OpenAdminGroupAsync(page, "Content Definition");
            await GoAsync(scene, Nav(page, "/Admin/ContentTypes/List"));
            await ShowAsync(scene, page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("[role='btn-edit-Page']") }).First, 1200);
            await ShowAsync(scene, page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("[role='btn-edit-MembersPage']") }).First, 1200);
            await page.Locator("[role='btn-edit-MembersPage']").ScrollToMiddleAsync();
            await GoAsync(scene, page.Locator("[role='btn-edit-MembersPage']"));
        });

        await scene.StepAsync("type-2", async () =>
        {
            var label = page.Locator("label").Filter(new() { HasTextString = "Securable" }).First;
            await label.ScrollToMiddleAsync();
            await scene.HighlightAsync(label);
        });

        await scene.StepAsync("type-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.GetByLabel("Securable"), pauseAfter: 400);
            await Save(page).ScrollToMiddleAsync();
            await GoAsync(scene, page.Locator("button.save[value='Save']").First);
        });
    }

    private static async Task RoleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("role-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Access Control");
            await GoAsync(scene, Nav(page, "/Admin/Roles/Index"));
            await ShowAsync(scene, page.Locator(".list-group-item").Filter(new() { HasTextString = "Administrator" }).First.Locator(".badge").First, 1500);
            await GoAsync(scene, page.Locator("a.create").First);
        });

        await scene.StepAsync("role-2", async () =>
        {
            await scene.TypeAsync(page.Locator("input[name='RoleName']"), "Member", delay: 40);
            await scene.TypeAsync(page.Locator("textarea[name='RoleDescription']"), "Approved members of the site.", delay: 30);
            await GoAsync(scene, page.Locator("button.save-configure").First);
            await page.Locator("#search-box").WaitForAsync();
        });

        await scene.StepAsync("role-3", async () =>
        {
            await scene.TypeAsync(page.Locator("#search-box"), "Members Page", delay: 45);
            await page.Locator("tr[data-text='View Members Page by others']").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("th").Filter(new() { HasTextString = "Allow" }).First);
            await scene.HighlightAsync(page.Locator("th").Filter(new() { HasTextString = "Effective" }).First);
        });

        await scene.StepAsync("role-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("tr[data-text='View Members Page by others'] input[id='Checkbox.View_MembersPage']"), pauseAfter: 400);
            await GoAsync(scene, Save(page));
            await WaitTextAsync(page, "Role updated successfully.");
        });
    }

    private static async Task EditRoleAsync(Scene scene, string filter, string checkbox, bool toggleView)
    {
        var page = scene.Page;
        await scene.TypeAsync(page.Locator("#search-box"), "view", delay: 45);
        var row = page.Locator("tr[data-text='View all content']").First;
        await row.ScrollToMiddleAsync();
    }

    private static async Task LockAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Roles/Index");
        var page = scene.Page;

        await scene.StepAsync("lock-1", async () =>
        {
            var row = page.Locator(".item[data-filter-value='anonymous']").First;
            await ShowAsync(scene, row, 1200);
            await GoAsync(scene, row.Locator("a.edit").First);
            await scene.TypeAsync(page.Locator("#search-box"), "view", delay: 45);
            await page.Locator("tr[data-text='View all content']").First.ScrollToMiddleAsync();
        });

        await scene.StepAsync("lock-2", async () =>
        {
            await ShowAsync(scene, page.Locator("tr[data-text='View all content']").First, 1500);
            var other = page.Locator("tr[data-text='View Page by others']").First;
            await other.ScrollToMiddleAsync();
            await scene.HighlightAsync(other);
        });

        await scene.StepAsync("lock-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await page.Locator("input[id='Checkbox.ViewContent']").First.ScrollToMiddleAsync();
            await scene.ClickAsync(page.Locator("input[id='Checkbox.ViewContent']").First, pauseAfter: 400);
            await page.Locator("input[id='Checkbox.View_Page']").First.ScrollToMiddleAsync();
            await scene.ClickAsync(page.Locator("input[id='Checkbox.View_Page']").First, pauseAfter: 400);
            await GoAsync(scene, Save(page));
            await WaitTextAsync(page, "Role updated successfully.");
        });

        await scene.StepAsync("lock-4", async () =>
        {
            var row = page.Locator(".item[data-filter-value='authenticated']").First;
            await GoAsync(scene, row.Locator("a.edit").First);
            await scene.TypeAsync(page.Locator("#search-box"), "view", delay: 45);
            await page.Locator("input[id='Checkbox.ViewContent']").First.ScrollToMiddleAsync();
            await scene.ClickAsync(page.Locator("input[id='Checkbox.ViewContent']").First, pauseAfter: 400);
            await GoAsync(scene, Save(page));
            await WaitTextAsync(page, "Role updated successfully.");
        });
    }

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;

        await scene.StepAsync("page-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Content");
            await GoAsync(scene, page.Locator("#left-nav a").Filter(new() { HasTextString = "Content Items" }).First);
            await scene.ClickAsync(page.Locator("#new-dropdown"), pauseAfter: 300);
            await GoAsync(scene, page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Members Page" }).First);
        });

        await scene.StepAsync("page-2", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Title", new() { Exact = true }).First, "Members Lounge", delay: 40);
            await scene.TypeAsync(page.GetByLabel("Permalink", new() { Exact = true }).First, "members-lounge", delay: 40);
            var body = page.Locator(".CodeMirror").First;
            await body.ScrollToMiddleAsync();
            await scene.ClickAsync(body, pauseAfter: 200);
            await page.Keyboard.TypeAsync("Welcome, members.", new() { Delay = 55 });
        });

        await scene.StepAsync("page-3", async () =>
        {
            await GoAsync(scene, page.Locator("button.publish").First);
            await page.GetByText("Members Lounge").First.WaitForAsync();
        });
    }

    private static async Task SignupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/", visitor: true);
        var page = scene.Page;

        await scene.StepAsync("signup-1", async () =>
        {
            await scene.PauseAsync(3000);
            await GoAsync(scene, page.Locator("a").Filter(new() { HasTextString = "Members Lounge" }).First);
            await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).First.WaitForAsync();
        });

        await scene.StepAsync("signup-2", async () =>
        {
            var link = page.GetByRole(AriaRole.Link, new() { Name = "Register as a new user" });
            await ShowAsync(scene, link, 1500);
            await GoAsync(scene, link);
        });

        await scene.StepAsync("signup-3", async () =>
        {
            await scene.TypeAsync(page.GetByLabel("Username", new() { Exact = true }).First, "jamie.chen", delay: 40);
            await scene.TypeAsync(page.GetByLabel("Email address").First, "jamie.chen@example.com", delay: 30);
            await scene.TypeAsync(page.GetByLabel("Password", new() { Exact = true }).First, MemberPassword, delay: 30);
            await scene.TypeAsync(page.GetByLabel("Confirm password").First, MemberPassword, delay: 30);
            await GoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Register" }).First);
            await page.GetByText("User Registration Pending").First.WaitForAsync();
        });

        await scene.StepAsync("signup-4", async () =>
        {
            await scene.HighlightAsync(page.GetByText("User Registration Pending").First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(page.GetByText("has to be approved before it can be used").First);
        });
    }

    private static async Task ApproveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin");
        var page = scene.Page;
        ILocator Row() => page.Locator(".list-group-item, tr").Filter(new() { HasTextString = "jamie.chen" }).First;

        await scene.StepAsync("approve-1", async () =>
        {
            await OpenAdminGroupAsync(page, "Access Control");
            await GoAsync(scene, Nav(page, "/Admin/Users/Index"));
            await scene.HighlightAsync(Row().Locator(".badge").Filter(new() { HasTextString = "Disabled" }).First);
        });

        await scene.StepAsync("approve-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Row().Locator("button.actions").First, pauseAfter: 400);
            await scene.ClickAsync(Row().Locator("a.dropdown-item").Filter(new() { HasTextString = "Enable" }).First, pauseAfter: 400);
            var ok = page.Locator("#modalOkButton");
            await ok.WaitForAsync();
            await GoOffAsync(scene, ok);
            await WaitTextAsync(page, "User account was successfully enabled.");
        });

        await scene.StepAsync("approve-3", async () =>
        {
            await GoAsync(scene, Row().Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
            var member = page.GetByLabel("Member", new() { Exact = true });
            await member.ScrollToMiddleAsync();
            await scene.ClickAsync(member, pauseAfter: 400);
            await GoAsync(scene, Save(page));
            await WaitTextAsync(page, "User updated successfully.");
        });

        await scene.StepAsync("approve-4", async () =>
        {
            await scene.ClickAsync(Row().Locator("button.actions").First, pauseAfter: 400);
            await scene.HighlightAsync(Row().Locator("a.dropdown-item").Filter(new() { HasTextString = "Disable" }).First);
        });
    }

    private static async Task MemberAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/", visitor: true);
        var page = scene.Page;

        await scene.StepAsync("member-1", async () =>
        {
            await GoAsync(scene, page.Locator("a").Filter(new() { HasTextString = "Members Lounge" }).First);
            await scene.TypeAsync(page.GetByLabel("Username or email address"), "jamie.chen", delay: 40);
            await scene.TypeAsync(page.GetByLabel("Password", new() { Exact = true }), MemberPassword, delay: 40);
            await GoOffAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).First);
            await page.GetByText("Welcome, members.").First.WaitForAsync();
        });

        await scene.StepAsync("member-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("h1").First);
        });

        await scene.StepAsync("member-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/");
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

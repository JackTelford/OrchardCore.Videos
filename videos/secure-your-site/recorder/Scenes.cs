using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Secure your site". PrepareAsync (run once, outside the capture) enables the features, creates three fake
/// users and two fake secrets. No clip saves anything.
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
        ("02-features", FeaturesAsync),
        ("03-roles", RolesAsync),
        ("03-users", UsersAsync),
        ("04-login", LoginAsync),
        ("04-registration", RegistrationAsync),
        ("05-https", HttpsAsync),
        ("05-headers", HeadersAsync),
        ("05-proxy", ProxyAsync),
        ("06-recaptcha", ReCaptchaAsync),
        ("06-rate-limits", RateLimitsAsync),
        ("06-cors", CorsAsync),
        ("07-audit", AuditAsync),
        ("07-secrets", SecretsAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        // 1. The features.
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var recipe = JsonSerializer.Serialize(new
        {
            steps = new object[]
            {
                new
                {
                    name = "feature",
                    enable = new[]
                    {
                        "OrchardCore.Https", "OrchardCore.Cors", "OrchardCore.ReCaptcha", "OrchardCore.ReCaptcha.Users", "OrchardCore.RateLimits",
                        "OrchardCore.ReverseProxy", "OrchardCore.Secrets", "OrchardCore.AuditTrail", "OrchardCore.Users.AuditTrail",
                        "OrchardCore.Users.2FA.AuthenticatorApp", "OrchardCore.Email", "OrchardCore.Users.Registration",
                    },
                },
            },
        });
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", recipe);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        await WaitForAdminAsync(page);

        // 2. The users (the step can be run again: users that exist are kept).
        await page.GotoAsync(BaseUrl + "/Admin/Users/Index", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var existing = await page.Locator("li.list-group-item").AllInnerTextsAsync();
        foreach (var (name, role) in new[] { ("alex.rivera", "Editor"), ("sam.okafor", "Author"), ("priya.nair", "Contributor") })
        {
            if (existing.Any(text => text.Contains(name)))
            {
                continue;
            }

            await page.GotoAsync(BaseUrl + "/Admin/Users/Create", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#User_UserName").FillAsync(name);
            await page.Locator("#User_Email").FillAsync($"{name}@example.com");
            await page.Locator("#User_Password").FillAsync(Password);
            await page.Locator("#User_PasswordConfirmation").FillAsync(Password);
            await page.Locator($"input[type=hidden][value='{role}']").Locator("xpath=preceding::input[@type='checkbox'][1]").CheckAsync();
            await page.Locator("button.btn-primary[type=submit], button.save, button[type=submit]:has-text('Create')").First.ClickAsync();
            await page.WaitForURLAsync(url => !url.Contains("/Create", StringComparison.OrdinalIgnoreCase));
        }

        // Confirm the email address of each user, and disable priya.nair: the actions are POST links of the row menu.
        await page.GotoAsync(BaseUrl + "/Admin/Users/Index", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        foreach (var name in new[] { "alex.rivera", "sam.okafor", "priya.nair" })
        {
            await RowActionAsync(page, name, "Confirm email");
        }

        await RowActionAsync(page, "priya.nair", "Disable");

        // 3. The secrets.
        foreach (var (name, description) in new[]
        {
            ("Payments.ApiKey", "API key for the payment service of the example shop."),
            ("Smtp.Password", "Password of the mail account that sends the example newsletter."),
        })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Secrets/Index", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            if ((await page.Locator("li.list-group-item").AllInnerTextsAsync()).Any(text => text.Contains(name)))
            {
                continue;
            }

            await page.GotoAsync(BaseUrl + "/Admin/Secrets/Create?type=TextSecret", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("#Name").FillAsync(name);
            await page.Locator("#TextValue").FillAsync("made-up-value-not-shown");
            await page.Locator("#Description").FillAsync(description);
            await page.Locator("button:has-text('Save')").Last.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await context.CloseAsync();
        Console.WriteLine("Prepared.");
    }

    private static async Task WaitForAdminAsync(IPage page)
    {
        for (var i = 0; i < 60; i++)
        {
            try
            {
                var response = await page.GotoAsync(BaseUrl + "/Admin", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

                if (response is { Ok: true })
                {
                    return;
                }
            }
            catch (PlaywrightException)
            {
            }

            await Task.Delay(2000);
        }
    }

    private static async Task RowActionAsync(IPage page, string user, string action)
    {
        var row = page.Locator("li.list-group-item").Filter(new() { HasTextString = user }).First;
        await row.Locator("button.dropdown-toggle.actions").First.ClickAsync();
        var item = row.Locator(".actions-menu a.dropdown-item").Filter(new() { HasTextRegex = Exact(action) }).First;

        if (await item.CountAsync() == 0)
        {
            return;
        }

        await item.ClickAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

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

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static ILocator Current(Scene scene)
        => scene.Page.Locator("#left-nav li.current > figure > figcaption a.item-label").First;

    private static ILocator Menu(Scene scene, string name)
        => scene.Page.Locator($"#left-nav a.item-label:has(span.title:text-is(\"{name}\"))").First;

    private static ILocator Row(Scene scene, string text)
        => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, string caption = null, bool clear = true)
    {
        await scene.HighlightAsync(target, caption);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task<Scene> OpenSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, string waitFor = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync(path);

        if (waitFor is not null)
        {
            await scene.Page.Locator(waitFor).First.WaitForAsync();
        }

        return scene;
    }

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Features", "#search-box");
        var group = scene.Page.Locator("div.feature-group:has(> h3:text-is('Security'))").First;
        await group.EvaluateAsync("e => e.scrollIntoView({ block: 'start' })");
        await scene.Page.EvaluateAsync("window.scrollBy(0, -150)");
        await scene.StartCaptureAsync();

        await scene.StepAsync("features-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Tools, Features");
            await ShowAsync(scene, group.Locator("> h3").First, 1.5);

            foreach (var feature in new[] { "HTTPS", "CORS Configuration", "ReCaptcha", "Rate Limits" })
            {
                await ShowAsync(scene, scene.Page.Locator($"li.list-group-item:has(span.fw-semibold:text-is('{feature}'))").First, 1.2, clear: feature != "Rate Limits");
            }
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = scene.Page.Locator("li.list-group-item:has(span.fw-semibold:text-is('Users Reset Password'))").First;
            await row.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(row.Locator("a.btn").First);
            await scene.HighlightAsync(row);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Roles/Index", "form.roles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("roles-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Access Control, Roles");
            await ShowAsync(scene, scene.Page.Locator("form.roles a:text-is('Anonymous')").First, 2);
            await scene.HighlightAsync(scene.Page.Locator("form.roles a:text-is('Authenticated')").First);
        });

        await scene.StepAsync("roles-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form.roles a:text-is('Anonymous')").First);
            await scene.Page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "View all content");
            var row = scene.Page.Locator("tr[data-text='View all content']").First;
            await row.WaitForAsync();
            await ShowAsync(scene, row.Locator("input[name^='Checkbox.']").First, 2);
            await scene.HighlightAsync(scene.Page.Locator("fieldset.permissions-list th:has-text('Effective')").First);
        });

        await scene.StepAsync("roles-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Roles/Edit/Editor");
            await scene.Page.Locator("#search-box").WaitForAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Manage Roles");
            var badge = scene.Page.Locator("tr[data-text='Manage Roles'] .badge").First;
            await badge.WaitForAsync();
            await scene.MoveToAsync(badge);
            await scene.HoldAsync(1);
            await scene.HighlightAsync(badge);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task UsersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Users/Index", "li.list-group-item:has-text('priya.nair')");
        await scene.StartCaptureAsync();

        await scene.StepAsync("users-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Access Control, Users");

            foreach (var user in new[] { "alex.rivera", "sam.okafor", "priya.nair" })
            {
                await ShowAsync(scene, Row(scene, user), 2, clear: user != "priya.nair");
            }
        });

        await scene.StepAsync("users-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = Row(scene, "sam.okafor");
            await scene.ClickAsync(row.Locator("button.dropdown-toggle.actions, button.dropdown-toggle").First);
            await ShowAsync(scene, row.Locator(".actions-menu a.dropdown-item:text-is('Disable'), .dropdown-menu a.dropdown-item:text-is('Disable')").First, 2.5);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator("#filter-dropdown").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.dropdown-item:text-is('Only disabled users')").First);
        });
    }

    private static async Task LoginAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Settings/userLogin", "label.form-check-label");
        await scene.StartCaptureAsync();

        await scene.StepAsync("login-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Settings, Security, User Login");
            await scene.HighlightAsync(scene.Page.Locator("label.form-check-label:has-text('Disable local password login')").First);
        });

        await scene.StepAsync("login-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("a[role='tab']:text-is('Two-Factor Authentication')").First);
            var label = scene.Page.Locator("label.form-check-label:has-text('Require two-factor authentication specific roles')").First;
            await label.WaitForAsync();
            await scene.ClickAsync(label);
            await scene.Page.Locator("#rolesContainer.show").WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator("#rolesContainer label.form-check-label:text-is('Administrator')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RegistrationAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Settings/userRegistration", "label.form-check-label");
        await scene.StartCaptureAsync();

        await scene.StepAsync("registration-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Settings, Security, Registration");
            await ShowAsync(scene, scene.Page.Locator("label.form-check-label:text-is('Users must verify their email address')").First, 3.5);
            await scene.HighlightAsync(scene.Page.Locator("label.form-check-label:text-is('Users must be approved before they can log in')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task HttpsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Settings/Https", ".toast.message-warning");
        await scene.StartCaptureAsync();

        await scene.StepAsync("https-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Settings, Security, HTTPS");
            await ShowAsync(scene, scene.Page.Locator("label.form-check-label:text-is('Require HTTPS')").First, 2.5);
            await ShowAsync(scene, scene.Page.Locator("label.ocat-label:text-is('HSTS mode')").First, 2);
            await scene.HighlightAsync(scene.Page.Locator(".alert-danger").First);
        });

        await scene.StepAsync("https-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, scene.Page.Locator(".toast.message-warning").First, 3);
            await scene.HighlightAsync(scene.Page.Locator("input[type='checkbox'][disabled]").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task HeadersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Settings/SecurityHeaders", "a[role='tab']");
        await scene.StartCaptureAsync();

        await scene.StepAsync("headers-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Settings, Security, Security Headers");
            await ShowAsync(scene, scene.Page.Locator("a[role='tab']:text-is('Content Security Policy')").First, 2.5);
            await scene.ClickAsync(scene.Page.Locator("a[role='tab']:text-is('Permissions Policy')").First);
            await ShowAsync(scene, scene.Page.Locator("tr:has(td:text-is('Camera'))").First, 2.5);
            await scene.ClickAsync(scene.Page.Locator("a[role='tab']:text-is('Referrer Policy')").First);
            await scene.HighlightAsync(scene.Page.Locator("select[name$='ReferrerPolicy']").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProxyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Settings/ReverseProxy", "label.form-check-label");
        await scene.StartCaptureAsync();

        await scene.StepAsync("proxy-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Settings, Reverse Proxy");

            foreach (var header in new[] { "X-Forwarded-For", "X-Forwarded-Proto", "X-Forwarded-Host" })
            {
                await ShowAsync(scene, scene.Page.Locator($"label.form-check-label:has-text('{header}')").First, 2.5, clear: header != "X-Forwarded-Host");
            }
        });

        await scene.StepAsync("proxy-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("span.hint:has-text('Current request value')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ReCaptchaAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Settings/recaptcha", "input[name$='SiteKey']");
        await scene.StartCaptureAsync();

        await scene.StepAsync("recaptcha-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Settings, Security, reCaptcha");
            await ShowAsync(scene, scene.Page.Locator("input[name$='SiteKey']").First, 3);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='SecretKey']").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RateLimitsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/RateLimits", "li.list-group-item:has-text('Default Global Policy')");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rate-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Tools, Rate Limits");
            await ShowAsync(scene, Row(scene, "Default Global Policy"), 3);
            await scene.HighlightAsync(Row(scene, "Default Global Policy").Locator(".badge:has-text('Enabled')").First);
        });

        await scene.StepAsync("rate-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var card = scene.Page.Locator("div.card:has(h5:text-is('Read-only Policies'))").First;
            await card.ScrollIntoViewIfNeededAsync();
            await ShowAsync(scene, card, 4);
            await scene.ScrollAsync(0, -1000);
            await scene.HighlightAsync(scene.Page.Locator("a.create:text-is('Add Policy')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CorsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Cors");
        await scene.Page.GetByText("CORS stands for").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("cors-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Settings, Security, Cross-Origin Resource Sharing");
            await scene.ClickAsync(scene.Page.Locator("button:text-is('Add a policy')").First);
            await scene.Page.Locator("label[for='allowed-origins']").WaitForAsync();
            await ShowAsync(scene, scene.Page.Locator("label[for='allowed-origins']").First, 2.5);
            await scene.HighlightAsync(scene.Page.Locator("label.ocat-label:text-is('Allowed origins')").First);
        });

        await scene.StepAsync("cors-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label[for='allow-credentials']").First);
            await scene.HighlightAsync(scene.Page.Locator("label[for='allow-credentials'] + span.hint").First);
            await scene.HoldAsync(3.5);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 600);
            await scene.HighlightAsync(scene.Page.Locator("div.card:has(h5.card-title:has-text('Methods'))").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AuditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Settings/AuditTrail", "label.ocat-label");
        await scene.StartCaptureAsync();

        await scene.StepAsync("audit-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Settings, Audit Trail");
            await ShowAsync(scene, scene.Page.Locator("label.ocat-label:text-is('Events to record')").First, 2);
            await scene.HighlightAsync(scene.Page.Locator("tr:has(td:text-is('Login failed'))").First);
        });

        await scene.StepAsync("audit-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("a[role='tab']:text-is('Trimming')").First);
            await ShowAsync(scene, scene.Page.Locator("label.ocat-label:text-is('Retention period')").First, 2.5);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='RetentionDays']").First);
        });

        await scene.StepAsync("audit-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/AuditTrail");
            await scene.Page.Locator("ul.list-group li.list-group-item").First.WaitForAsync();
            await ShowAsync(scene, Current(scene), 1.5, "Tools, Audit Trail");
            await ShowAsync(scene, scene.Page.Locator("ul.list-group li.list-group-item").First, 2.5);
            await ShowAsync(scene, scene.Page.Locator(".filter button[title='Date']").First, 2);
            await scene.HighlightAsync(scene.Page.Locator(".filter button[title='Category']").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SecretsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenSceneAsync(browser, directory, durations, "/Admin/Secrets/Index", "li.list-group-item:has-text('Smtp.Password')");
        await scene.StartCaptureAsync();

        await scene.StepAsync("secrets-1", async () =>
        {
            await ShowAsync(scene, Current(scene), 1.5, "Tools, Security, Secrets");
            await ShowAsync(scene, Row(scene, "Payments.ApiKey"), 3);
            await ShowAsync(scene, Row(scene, "Smtp.Password"), 2.5);
            await scene.HighlightAsync(Menu(scene, "Migrate Secrets"));
        });

        await scene.ClearHighlightsAsync();
    }
}

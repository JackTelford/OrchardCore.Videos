using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-settings", SettingsAsync),
        ("04-app", AppAsync),
        ("05-signin", SigninAsync),
        ("06-methods", MethodsAsync),
        ("06-email", EmailAsync),
        ("06-sms", SmsAsync),
        ("07-required", RequiredAsync),
        ("08-disable", DisableAsync),
    ];

    private const string KeyFile = "C:/ocv/s5/totp-key.txt";
    private const string EmlDir = "C:/Code/OrchardCore.Videos-renders/sites/s5/users-two-factor-authentication/App_Data/Sites/Default/Emails";
    private const string SiteLog = "C:/Code/OrchardCore.Videos-renders/sites/s5/users-two-factor-authentication/site-5015.log";

    public static async Task PrepareAsync(IBrowser browser)
    {
        Directory.CreateDirectory(EmlDir);
        foreach (var f in Directory.GetFiles(EmlDir))
        {
            File.Delete(f);
        }

        var (context, page) = await SeedPageAsync(browser);
        await EnableFeaturesAsync(page, "OrchardCore.Email.Smtp", "OrchardCore.Sms");
        await Task.Delay(6000);
        await page.GotoAsync(BaseUrl + "/Admin/Settings/email", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("a.nav-link:text-is(\"SMTP\")").ClickAsync();
        await page.Locator("#ISite_SmtpSettings_IsEnabled").CheckAsync();
        await page.Locator("#ISite_SmtpSettings_DefaultSender").FillAsync("noreply@example.com");
        await page.Locator("#ISite_SmtpSettings_DeliveryMethod").SelectOptionAsync(new SelectOptionValue { Label = "Specified pickup directory" });
        await page.Locator("#ISite_SmtpSettings_PickupDirectoryLocation").FillAsync("/");
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.save").ClickAsync());
        var emailText = await page.Locator("body").InnerTextAsync();
        Console.WriteLine("Email page after save: " + emailText.Replace((char)10, (char)32)[..Math.Min(400, emailText.Length)]);
        await Task.Delay(3000);
        await page.GotoAsync(BaseUrl + "/Admin/Settings/sms", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#ISite_SmsSettings_DefaultProvider").SelectOptionAsync(new SelectOptionValue { Label = "Log" });
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button.save").ClickAsync());
        Console.WriteLine("Settings saved: " + (await page.Locator("body").InnerTextAsync()).Contains("updated successfully"));
        await ImportRecipeAsync(page, "{\"steps\":[{\"name\":\"command\",\"Commands\":[\"createUser /UserName:jamie /Password:" + Password + " /Email:jamie.chen@example.com\"]}]}");
        await context.CloseAsync();
    }

    // RFC 6238, SHA-1, 30 seconds, six digits.
    private static string Totp(string key)
    {
        key = key.Replace(" ", string.Empty).ToUpperInvariant();
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bits = 0;
        var value = 0;
        var bytes = new List<byte>();
        foreach (var c in key)
        {
            value = (value << 5) | alphabet.IndexOf(c);
            bits += 5;
            if (bits >= 8)
            {
                bytes.Add((byte)((value >> (bits - 8)) & 0xFF));
                bits -= 8;
            }
        }

        var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        var counterBytes = BitConverter.GetBytes(counter);
        Array.Reverse(counterBytes);
        var hash = new HMACSHA1(bytes.ToArray()).ComputeHash(counterBytes);
        var offset = hash[^1] & 0x0F;
        var code = ((hash[offset] & 0x7F) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
        return (code % 1_000_000).ToString("D6");
    }

    private static async Task<string> FreshCodeAsync()
    {
        var key = File.ReadAllText(KeyFile).Trim();
        if (30 - (DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 30) < 4)
        {
            await Task.Delay(5000);
        }

        return Totp(key);
    }

    private static string NewestCode(string text)
    {
        text = text.Replace("=\r\n", string.Empty).Replace("=\n", string.Empty).Replace("=3D", "=");
        var m = Regex.Matches(text, @"code is\s*:?\s*(\d{6})");
        return m.Count > 0 ? m[^1].Groups[1].Value : null;
    }

    private static async Task<string> EmailCodeAsync()
    {
        for (var i = 0; i < 20; i++)
        {
            var newest = new DirectoryInfo(EmlDir).GetFiles("*.eml").OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault();
            if (newest is not null)
            {
                var code = NewestCode(File.ReadAllText(newest.FullName));
                if (code is not null)
                {
                    return code;
                }
            }

            await Task.Delay(500);
        }

        throw new InvalidOperationException("No email code found.");
    }

    private static async Task<string> SmsCodeAsync()
    {
        for (var i = 0; i < 20; i++)
        {
            using var stream = new FileStream(SiteLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var text = await reader.ReadToEndAsync();
            var m = Regex.Matches(text, @"A message with the body '.*?(\d{6}).*?' was set to");
            if (m.Count > 0)
            {
                return m[^1].Groups[1].Value;
            }

            await Task.Delay(500);
        }

        throw new InvalidOperationException("No SMS code found.");
    }

    // Signs the user in off camera. withCode: finish the second step with the authenticator code.
    private static async Task SignInAsync(Scene scene, string user, bool withCode)
    {
        var page = scene.Page;
        await scene.Context.ClearCookiesAsync();
        await scene.OpenAsync("/Login");
        await page.Locator("#LoginForm_UserName").FillAsync(user);
        await page.Locator("#LoginForm_Password").FillAsync(Password);
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button[type=submit]").First.ClickAsync());
        if (!withCode)
        {
            return;
        }

        for (var i = 0; i < 4 && !(await page.Locator("body").InnerTextAsync()).Contains("authenticator app"); i++)
        {
            await page.RunAndWaitForNavigationAsync(async () => await page.GetByText("Sign in another way").First.ClickAsync());
        }

        await page.Locator("#VerificationCode").FillAsync(await FreshCodeAsync());
        await page.RunAndWaitForNavigationAsync(async () => await page.Locator("button:has-text(\"Log in\")").Last.ClickAsync());
    }

    private static async Task OpenGroupAsync(Scene scene, string group, int pause = 250)
    {
        var button = scene.Page.Locator($"#left-nav button.nav-group-label:has(span.title:text-is(\"{group}\")), #left-nav button.item-label:has(span.title:text-is(\"{group}\"))").First;
        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await FastClickAsync(scene, button, pause);
        }
    }

    private static ILocator MRow(IPage page, string text) => page.Locator("div.row.border-bottom").Filter(new() { HasText = text }).First;

    private static ILocator FRow(IPage page, string id) => page.Locator($"[data-filter-value*=\"{id}\"]").First;

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await OpenGroupAsync(scene, "Tools", 200);
            await FastGoAsync(scene, page.Locator("#left-nav a.item-label:has(span.title:text-is(\"Features\"))"));
            await FastTypeAsync(scene, page.Locator("#search-box"), "two-factor", 40);
            foreach (var id in new[] { "OrchardCore.Users.2FA.AuthenticatorApp", "OrchardCore.Users.2FA.Email", "OrchardCore.Users.2FA.Sms" })
            {
                await scene.HighlightAsync(FRow(page, id));
                await scene.PauseAsync(1200);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("enable-2", async () =>
        {
            foreach (var id in new[] { "OrchardCore.Users.2FA.AuthenticatorApp", "OrchardCore.Users.2FA.Email", "OrchardCore.Users.2FA.Sms" })
            {
                await FastClickAsync(scene, page.Locator($"input[name='featureIds'][value='{id}']"), 200);
            }

            await FastClickAsync(scene, page.Locator("#bulk-action-menu-button"), 300);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("a[data-action='Enable']").First));
            await page.GetByText("were enabled").First.WaitForAsync();
        });

        await scene.StepAsync("enable-3", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#search-box"), "sms", 40);
            await scene.HighlightAsync(page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-disable-OrchardCore_Sms") }).First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await page.Locator("#search-box").FillAsync("");
            await FastTypeAsync(scene, page.Locator("#search-box"), "smtp", 40);
            await scene.HighlightAsync(page.Locator(".list-group-item").Filter(new() { Has = page.Locator("#btn-disable-OrchardCore_Email_Smtp") }).First);
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await SignInAsync(scene, "admin", true);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await page.AddStyleTagAsync(new PageAddStyleTagOptions { Content = ".ocat-wrapper:has(textarea) { display: none !important; }" });
        await scene.StartCaptureAsync();

        await scene.StepAsync("settings-1", async () =>
        {
            await OpenGroupAsync(scene, "Settings", 150);
            await OpenGroupAsync(scene, "Security", 150);
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav a:has(span.title:text-is(\"User Login\"))").First);
                await page.AddStyleTagAsync(new PageAddStyleTagOptions { Content = ".ocat-wrapper:has(textarea) { display: none !important; }" });
            });
            await FastClickAsync(scene, page.GetByRole(AriaRole.Tab, new() { Name = "Two-Factor Authentication" }), 200);
            await scene.HighlightAsync(page.Locator("legend:has-text(\"Two-Factor Settings\")").First);
        });

        await scene.StepAsync("settings-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.GetByLabel("Allow users to remember client").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 300);
            await scene.HighlightAsync(page.GetByLabel("Number of recovery codes to generate"));
        });

        await scene.StepAsync("settings-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.GetByLabel("Require two-factor authentication specific roles").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 400);
            await FastClickAsync(scene, page.GetByLabel("Administrator", new() { Exact = true }).Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 300);
            await scene.HighlightAsync(page.GetByText("Select the roles to require two-factor authentication for.").First);
        });

        await scene.StepAsync("settings-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var require = page.GetByLabel("Require two-factor authentication", new() { Exact = true });
            await FastClickAsync(scene, require.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 300);
            await scene.HighlightAsync(require.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("settings-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastClickAsync(scene, page.GetByLabel("Require two-factor authentication specific roles").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]//label"), 300);
            await scene.OffCameraAsync(async () => await scene.ClickAndWaitForNavigationAsync(page.Locator("button.save").First));
            await page.GetByText("Site settings updated successfully.").First.WaitForAsync();
            await page.AddStyleTagAsync(new PageAddStyleTagOptions { Content = ".ocat-wrapper:has(textarea) { display: none !important; }" });
            await FastClickAsync(scene, page.GetByRole(AriaRole.Tab, new() { Name = "Two-Factor Authentication" }), 200);
        });
    }

    private static async Task AppAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("app-1", async () =>
        {
            await FastClickAsync(scene, page.Locator("#navbarDropdown").First, 400);
            await FastGoAsync(scene, page.Locator("a.dropdown-item:has-text(\"Security\")").First);
            await scene.HighlightAsync(MRow(page, "Authenticator app"));
            await scene.HighlightAsync(MRow(page, "Email authentication"));
            await scene.HighlightAsync(MRow(page, "SMS authentication"));
        });

        await scene.StepAsync("app-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = MRow(page, "Authenticator app");
            await FastGoAsync(scene, row.GetByRole(AriaRole.Link, new() { Name = "Add" }));
            await page.Locator("#qrCode img").First.WaitForAsync();
            File.WriteAllText(KeyFile, (await page.Locator("kbd").First.InnerTextAsync()).Trim());
            await scene.HighlightAsync(page.Locator("#qrCode").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("kbd").First);
        });

        await scene.StepAsync("app-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, page.Locator("#Code"), await FreshCodeAsync(), 40);
            await scene.HighlightAsync(page.Locator("button:has-text(\"Verify\")").First);
        });

        await scene.StepAsync("app-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("button:has-text(\"Verify\")").First);
            await scene.HighlightAsync(page.GetByText("Store these codes in a safe place.").First);
        });

        await scene.StepAsync("app-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.Locator("a:text-is(\"Security\"), a:text-is(\"Two-Factor Authentication\")").First);
            var reset = page.GetByRole(AriaRole.Button, new() { Name = "Reset codes" }).Or(page.GetByRole(AriaRole.Link, new() { Name = "Reset codes" })).First;
            await reset.EvaluateAsync("e => e.scrollIntoView({ block: 'end' })");
            await scene.ScrollAsync(0, 100, 4);
            await scene.HighlightAsync(reset);
        });
    }

    private static async Task SigninAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.Context.ClearCookiesAsync();
        await scene.OpenAsync("/Login");
        var page = scene.Page;
        await scene.StartCaptureAsync();

        await scene.StepAsync("signin-1", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#LoginForm_UserName"), "admin", 40);
            await FastTypeAsync(scene, page.Locator("#LoginForm_Password"), Password, 5, fill: true);
            await FastGoAsync(scene, page.Locator("button[type=submit]").First);
        });

        await scene.StepAsync("signin-2", async () =>
        {
            await scene.HighlightAsync(page.GetByLabel("Remember this device").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByText("Sign in another way").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.GetByText("log in with a recovery code").First);
        });

        await scene.StepAsync("signin-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastTypeAsync(scene, page.Locator("#VerificationCode"), await FreshCodeAsync(), 40);
            await FastGoAsync(scene, page.Locator("button:has-text(\"Log in\")").Last);
        });
    }

    private static async Task MethodsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await SignInAsync(scene, "admin", true);
        await scene.OpenAsync("/TwoFactor");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("methods-1", async () =>
        {
            var row = MRow(page, "Email authentication");
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("methods-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var select = page.Locator("select[name$=\"PreferredProvider\"], select#PreferredProvider").First;
            // Product bug (UserSecurityTwoFactor.cshtml: the select is named UserSecurity.PreferredProvider, the action binds PreferredProvider): fix the name off camera so the post binds.
            await select.EvaluateAsync("e => e.name = 'PreferredProvider'");
            await select.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })");
            await scene.SelectAsync(select, "Authenticator", 300);
            await FastGoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Update" }).First);
        });
    }

    private static async Task EmailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await SignInAsync(scene, "admin", false);
        var page = scene.Page;
        await page.WaitForTimeoutAsync(400);
        await scene.StartCaptureAsync();

        await scene.StepAsync("email-1", async () =>
            await FastGoAsync(scene, page.GetByText("Sign in another way").First));

        await scene.StepAsync("email-2", async () =>
        {
            await FastClickAsync(scene, page.Locator("#RequestCode"), 300);
            await page.Locator("#RequestCodeFeedback").GetByText("A verification code has been sent").WaitForAsync();
        });

        await scene.StepAsync("email-3", async () =>
        {
            var code = "";
            await scene.OffCameraAsync(async () => code = await EmailCodeAsync());
            await FastTypeAsync(scene, page.Locator("#VerificationCode"), code, 40);
            await FastGoAsync(scene, page.Locator("button:has-text(\"Log in\")").Last);
            Console.WriteLine("After email login: " + page.Url + " code " + code);
        });
    }

    private static async Task SmsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await SignInAsync(scene, "admin", true);
        await scene.OpenAsync("/TwoFactor");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("sms-1", async () =>
        {
            await FastGoAsync(scene, page.GetByRole(AriaRole.Link, new() { Name = "Verify Phone Number" }).First);
            await FastTypeAsync(scene, page.Locator("#PhoneNumber"), "+12015550123", 25);
            await FastGoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Request Code" }).First);
        });

        await scene.StepAsync("sms-2", async () =>
        {
            var code = "";
            await scene.OffCameraAsync(async () => code = await SmsCodeAsync());
            await FastTypeAsync(scene, page.Locator("#Code"), code, 40);
            await FastGoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Verify" }).First);
            await page.GetByText("Your phone number has been confirmed.").First.WaitForAsync();
            await scene.HighlightAsync(MRow(page, "SMS authentication").GetByText("Verified").First);
        });
    }

    private static async Task RequiredAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "signed-out.json"), Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Login");
        var page = scene.Page;
        await scene.StartCaptureAsync();

        await scene.StepAsync("required-1", async () =>
        {
            await FastTypeAsync(scene, page.Locator("#LoginForm_UserName"), "jamie", 40);
            await FastTypeAsync(scene, page.Locator("#LoginForm_Password"), Password, 5, fill: true);
            await FastGoAsync(scene, page.Locator("button[type=submit]").First);
            await page.GetByText("Two-factor authentication must be enabled before proceeding.").First.WaitForAsync();
            await scene.HighlightAsync(page.GetByText("Two-factor authentication must be enabled before proceeding.").First);
        });

        await scene.StepAsync("required-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await FastGoAsync(scene, page.GetByRole(AriaRole.Button, new() { Name = "Enable" }).Or(page.GetByRole(AriaRole.Link, new() { Name = "Enable" })).First);
        });

        await scene.StepAsync("required-3", async () =>
        {
            await FastGoAsync(scene, page.Locator("a:text-is(\"Security\"), a:text-is(\"Two-Factor Authentication\")").First);
            await scene.HighlightAsync(page.Locator("div.card-header").GetByText("Enabled", new() { Exact = true }).First);
        });
    }

    private static async Task DisableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await SignInAsync(scene, "admin", true);
        await scene.OpenAsync("/Admin");
        var page = scene.Page;
        await page.WaitForTimeoutAsync(600);
        await scene.StartCaptureAsync();

        await scene.StepAsync("disable-1", async () =>
        {
            await OpenGroupAsync(scene, "Access Control", 200);
            await FastGoAsync(scene, page.Locator("#left-nav a:has(span.title:text-is(\"Users\"))").First);
            var row = page.Locator("li.list-group-item").Filter(new() { HasText = "jamie" }).First;
            await scene.MoveToAsync(row);
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("disable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = page.Locator("li.list-group-item").Filter(new() { HasText = "jamie" }).First;
            await FastClickAsync(scene, row.Locator("button.actions").First, 500);
            await scene.WaitForNavigationAsync(async () =>
            {
                await FastClickAsync(scene, row.Locator("a.dropdown-item:has-text(\"Disable two-factor authentication\")").First, 400);
                await page.Locator("#modalOkButton").WaitForAsync();
                await scene.PauseAsync(800);
                await FastClickAsync(scene, page.Locator("#modalOkButton"), 0);
            });
            var again = page.Locator("li.list-group-item").Filter(new() { HasText = "jamie" }).First;
            await FastClickAsync(scene, again.Locator("button.actions").First, 500);
            await scene.HighlightAsync(again.Locator(".dropdown-menu.show").First);
        });
    }
}

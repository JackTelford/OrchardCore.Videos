# Open questions: security-https

What a person must confirm on a running site before or while recording. Nothing here is stated in the narration as a
fact beyond what the source shows; each item says what was checked and what is left.

1. **Does the recorder's browser accept the development certificate?** The settings page only works over HTTPS, so the
   clips need an HTTPS address. `Scene.cs` creates the browser context without ignoring certificate errors, so the
   machine has to trust the ASP.NET Core development certificate (`dotnet dev-certs https --trust`). Not run, and the
   use of that certificate by Kestrel in the Development environment comes from ASP.NET Core, not from the Orchard
   Core source: confirm with `Recorder probe /Admin "() => document.title"` on `https://localhost:5011`.
2. **Selectors were read, not run.** The menu buttons and links (`OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`,
   `NavigationItemLink-admin.cshtml`), `#btn-enable-OrchardCore_Https`, the field names ending in `.RequireHttps`,
   `.StrictTransportSecurityMode`, `.RequireHttpsPermanent` and `.SslPort`, and `button.save` come from the views.
   Confirm each with `Recorder probe`. In particular, confirm that Tools, Settings and Security are buttons that
   expand their group, and that Features and HTTPS are links.
3. **The search on Tools, Features.** Typing HTTPS may also leave another feature whose description holds a link
   starting with `https://` (`OrchardCore.Modules/OrchardCore.Tenants/Manifest.cs:29`). Confirm what the list shows and
   that `features-1` and `features-2` still read well.
4. **Pronunciation of HSTS.** `storyboard.json` speaks HSTS as `H.S.T.S.`. Check it, and the words "HTTPS", "SSL" and
   "Production", with `scripts/pronounce.py` before narrating.
5. **The redirect itself was not run.** `OrchardCore.Https/Startup.cs:26-29` calls the ASP.NET Core HTTPS redirection
   when Require HTTPS is selected, and sets the redirect port from SSL Port (`:57`) and the permanent status code
   from Require HTTPS Permanent (`:52-55`). On a running site, confirm that after saving, a request to the HTTP
   address of any page is answered with a redirect to the HTTPS address (for example `curl -i` on an HTTP address
   of the same site), and which status code and port it uses with and without Require HTTPS Permanent. The screencast
   has no address bar, so the video shows this on slides only. Also confirm the case of an empty SSL Port: ASP.NET
   Core's redirection middleware is documented to skip the redirect when it cannot find an HTTPS port, which is outside
   the Orchard Core source. The narration only says that the site tries to work the port out (`settings-5`).
6. **The HSTS header was not observed.** The module registers a 365-day lifetime, subdomains included, no preload
   (`Startup.cs:60-65`). ASP.NET Core's HSTS middleware does not send the header to localhost or over HTTP, which
   is behavior outside the Orchard Core source, so it cannot be checked in the recording. Confirm on a real host in
   the Production environment with HSTS mode Enabled or From environment.
7. **Who can open the page.** The module grants Manage HTTPS to the Administrator role (`OrchardCore.Https/Permissions.cs:18-25`)
   and shows the menu item to users with that permission (`AdminMenu.cs:30`). The settings controller, however,
   authorizes the page with the group settings permission (`OrchardCore.Settings/Controllers/AdminController.cs:53`),
   which is shown as Manage settings and implied by Manage settings (`OrchardCore/OrchardCore.Settings.Core/SettingsPermissions.cs:21`);
   a permission registered for the group also grants it (`OrchardCore.Settings/Services/SiteSettingsAuthorizationHandler.cs:24-47`).
   The HTTPS module registers none for its group (it has no call to `AddSiteSettingsPermission`, unlike
   `OrchardCore.Email/Startup.cs:21`), and its display driver then requires Manage HTTPS as well
   (`Drivers/HttpsSettingsDisplayDriver.cs:45`). A role that has Manage HTTPS but not Manage settings may therefore see
   the menu item and be refused the page. The narration says nothing about roles. Confirm on a running site with such
   a role.
8. **The page over HTTP.** On slide `what-4` the narration and the slide quote "For safety, changing HTTPS-only
   settings over HTTP has been prevented." (`Drivers/HttpsSettingsDisplayDriver.cs:58`), which the driver adds when
   the page is opened over HTTP, and say that the changes you save are not applied (`:85-88` returns before any value
   is assigned). The narration does not say that fields are disabled, on purpose: the view disables the two check
   boxes and SSL Port with `asp-is-disabled` (`Views/HttpsSettings.Edit.cshtml:6, 30, 40`), and the tag helper that
   handles this attribute targets `input` elements only (`OrchardCore.DisplayManagement/TagHelpers/InputIsDisabledTagHelper.cs:5`),
   while the HSTS mode `select` carries the same attribute (`:16`). Confirm on a running site, over an HTTP address,
   that the message appears, that HSTS mode can be edited, and what Save shows. The settings controller adds "Site
   settings updated successfully." whenever the model state is valid (`OrchardCore.Settings/Controllers/AdminController.cs:106-122`),
   so the success message probably appears over HTTP although nothing was applied. Not run.
9. **Hint text of Require HTTPS Permanent.** The hint (`Views/HttpsSettings.Edit.cshtml:32`) calls 308 "Moved
   Permanently" and 307 "Found". Those are the names of 301 and 302. The narration does not read the hint or the
   status codes. The code uses 308 (`Startup.cs:54`), which the documentation calls 301.
10. **Load balancers.** Narration `after-3` says that a site behind a load balancer that passes plain HTTP sees HTTP
    requests, so the page ignores the changes you save, and that the Reverse Proxy settings can forward the protocol.
    The page checks `Request.IsHttps` (`Drivers/HttpsSettingsDisplayDriver.cs:54, 85`), and the Reverse Proxy
    documentation describes forwarding "the protocol (HTTP or HTTPS) used by the client"; the module has a setting
    labeled "Enables the forwarding of the HTTP header X-Forwarded-Proto" (`OrchardCore.ReverseProxy/Views/ReverseProxySettings.Edit.cshtml:21`)
    and calls `UseForwardedHeaders` (`OrchardCore.ReverseProxy/Startup.cs:23`). Confirm on a site behind a proxy that
    changes are ignored without forwarded headers and applied with them.
11. **Time of the reload after Save.** Saving reloads the tenant (`Drivers/HttpsSettingsDisplayDriver.cs:95`). Measure
    how long the page takes to come back in `settings-6`, so the message "Site settings updated successfully." is on
    screen when the narration of that step ends.
12. **Where the toasts sit.** The toast container of the admin theme is fixed to the end edge of the page
    (`OrchardCore.Themes/TheAdmin/Views/NotifyMessages.cshtml:3-12`); its `top` rule is commented out, so the vertical
    position is not known from the source. Look at the contact sheet of `02-features` and `03-settings` to see that the
    messages "The feature HTTPS was enabled." and "Site settings updated successfully." are fully on screen.
13. **Reading the option label.** The narration reads the option as "From environment, enabled in Production, disabled
    otherwise", which is the label with its dash read as a pause (`Views/HttpsSettings.Edit.cshtml:19`). Listen to it
    with `scripts/pronounce.py`.
14. **The Production environment.** "Production" in the narration is the environment name the site reads from its
    host (`Startup.cs:31-33`, `IHostEnvironment.IsProduction()`). The recording runs in Development, so the From
    environment option does not send the header there. Confirm on a Production host.

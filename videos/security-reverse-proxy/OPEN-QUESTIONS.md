# Open questions: security-reverse-proxy

What the author could not verify from the Orchard Core source or its documentation, and what a person confirms on a
running site before the video is published. Nothing below is stated in the narration as a fact beyond what the code and
the documentation say; where a sentence depends on it, the step is named.

## 1. Forwarded values behind a real proxy (`settings-4`, `why-1`, `why-2`)

- **Checked in the source.** The module calls `app.UseForwardedHeaders()` (`OrchardCore.ReverseProxy/Startup.cs:23`)
  with the forwarded headers taken from the saved settings (`Configuration/ForwardedHeadersOptionsConfiguration.cs:19`).
  The three hints of the page read the remote address, the scheme and the host of the current request
  (`Views/ReverseProxySettings.Edit.cshtml:13,24,35`; `OrchardCore/Modules/DefaultClientIPAddressAccessor.cs:17`). The
  documentation says each option "enables forwarding" of the client IP address, the original host and the protocol,
  and the Rate Limits page (`reference/modules/RateLimits/README.md`, "Reverse proxies and load balancers") says that
  enabling the feature with `X-Forwarded-For` lets the rate limits see the address of each client.
- **Not checked.** The video is recorded without a proxy, so no run shows the values change. What the ASP.NET Core
  middleware does with the headers is framework behavior, described on the Microsoft page that the module's
  documentation links to, and is not in the Orchard Core source.
- **Confirm.** Behind a proxy that sends `X-Forwarded-For`, `X-Forwarded-Proto` and `X-Forwarded-Host`: with the three
  options off, the hints show the values of the proxy's connection; after turning them on and saving, they show the
  forwarded address, protocol and host. If not, change `settings-4` ("turning an option on makes the site use the
  forwarded value instead") and the slides `why-1` to `why-3`.

## 2. Which senders are trusted when the lists are empty (`safe-1`, `safe-2`)

- **Checked in the source.** `ForwardedHeadersOptionsConfiguration.cs:21-22` clears `KnownIPNetworks` and
  `KnownProxies`, then adds only the values saved in the settings or read from the configuration. The admin page has
  no field for either list (`Views/ReverseProxySettings.Edit.cshtml`: three check boxes only). The `Settings` recipe
  step stores a property it does not know, such as `ReverseProxySettings`, in the site properties
  (`OrchardCore.Settings/Recipes/SettingsStep.cs`, the `default` branch), which is how the lists are set outside the
  page.
- **Not checked.** What the middleware accepts when both lists stay empty after the defaults are cleared. The
  narration only repeats the documentation's advice ("Always configure KnownProxies and KnownNetworks", "Only trust
  headers from known proxies") and says the page has no fields for them.
- **Confirm.** On a running site with the three options on and the lists empty, whether a request from another
  machine with a forged `X-Forwarded-For` changes the address the site sees. If it does, the video can say that
  turning the options on without the lists trusts every sender.

## 3. The override message (`safe-3`)

- **Checked in the source.** The message is
  `The current settings are coming from configuration sources, saving the settings will affect the AdminSettings not
  the configuration.` (`Drivers/ReverseProxySettingsDisplayDriver.cs:66`, an information notification, not a warning).
  It appears whenever the application calls `ConfigureReverseProxySettings()`
  (`Extensions/OrchardCoreBuilderExtensions.cs:20` sets the flag even when the configuration has no `ReverseProxy`
  section), so the narration says "when the application is set up to read these settings from the server
  configuration". The configuration values replace the saved ones (`PostConfigure`, same file).
- **Not checked.** `OrchardCore.Cms.Web/Program.cs` never calls the method, so the message cannot be recorded with the
  site of this plan and no clip shows it. The slide `safe-3` quotes the code. Whether the lists `KnownProxies` and
  `KnownNetworks` from the configuration replace the saved lists or are added to them depends on how the configuration
  binder fills an array that already holds values: that is framework behavior, not in the Orchard Core source. The
  narration only says that the configuration values take precedence, as the documentation does.
- **Confirm.** On an application that calls the method, that the message appears on the page, that a value from the
  configuration wins over the saved one, and what happens to a saved list when the configuration has a list.

## 4. The deployment step (`safe-4`)

- **Checked in the source.** `OrchardCore.ReverseProxy/Startup.cs:37-43` registers a deployment step named
  `Reverse Proxy settings` ("Exports the Reverse Proxy settings.") only when the Deployment feature is enabled. The
  Blog recipe enables Deployment.
- **Confirm.** That the step is listed under the deployment plan's add-step choices with this name. The video does not
  show it.

## 5. What the screen shows during the recording

- **Fresh site.** The three options are off and the hints read `127.0.0.1`, `http` and `localhost:5010` (the port in
  use). `settings-2` says "All three are off" and `settings-3` says the values are those of the local connection.
- **The label on screen.** The hint reads `Current request value;` with a semicolon
  (`Views/ReverseProxySettings.Edit.cshtml:13,24,35`). The narration says "Current request value" without it.
- **The search.** `enable-1` expects the list to show one feature after searching `reverse`: the only manifest of the
  source that mentions the word is this feature's. The filter itself is page script that was not run.
- **Toasts.** A success message stays 5 seconds (`OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`). The
  plan puts each click at the start of its step so the message is on screen during the first sentence of that
  narration. The restart of the tenant after enabling the feature and after saving plays on camera, because the
  message is part of the page that loads after it; the `OffCameraAsync` waits of the plan are only guards. Confirm in
  the contact sheets that the message is readable while it is mentioned (`enable-3`, `settings-5`), and that the
  restart stays short and does not leave an error page on screen.
- **Saving.** `Drivers/ReverseProxySettingsDisplayDriver.cs:91-106` sets the headers to `None` and then adds the
  checked ones, so a value saved by a recipe that the page cannot show (`XForwardedPrefix`) is dropped when the page
  is saved. The video does not mention it.

## 6. The permissions of the page (`enable-3`)

- **Checked in the source.** The menu entry needs Manage Reverse Proxy Settings (`AdminMenu.cs:31`). The page route asks
  for the group settings permission of the group `ReverseProxy` (`OrchardCore.Settings/Controllers/AdminController.cs:53`),
  which `Manage settings` implies (`OrchardCore/OrchardCore.Settings.Core/SettingsPermissions.cs:21`). This module
  registers no permission for the group (compare `OrchardCore.Email/Startup.cs:21`), and its display driver also asks
  for Manage Reverse Proxy Settings (`Drivers/ReverseProxySettingsDisplayDriver.cs:53`). A user in the Administrator role
  passes every permission check (`OrchardCore.Settings/Services/SuperUserHandler.cs:35`), and the module lists the
  Administrator role as a default holder of its permission (`Permissions.cs:22` here;
  `OrchardCore.Settings/Permissions.cs:9,21` for Manage settings). The narration says "Opening the page takes two
  permissions".
- **Not checked.** The two-permission rule was read from the code and not run.
- **Confirm.** On a running site, with a role that has Manage Reverse Proxy Settings but not Manage settings: the
  Settings menu shows Reverse Proxy, and opening the page is refused. With both permissions, the page opens. If a role
  with only Manage Reverse Proxy Settings can open the page, change `enable-3`.

## 7. Pronunciation (whole narration)

- **Spellings used without a `spoken` entry.** The three header names (`X-Forwarded-For`, `X-Forwarded-Proto`,
  `X-Forwarded-Host`), `IP`, `HTTP`, `HTTPS`, `Audit Trail` and `Rate Limits` are written as they are said, and the
  storyboard's `spoken` map is empty.
- **Confirm.** With `scripts/pronounce.py`, listen to `why-2`, `settings-2` and `outro` for the header names: the voice
  should say "X forwarded for", "X forwarded proto" and "X forwarded host" with the hyphens silent, and "Proto" as
  "pro-toe". If not, add `spoken` entries for them and for `IP` ("I.P.").

# Recording plan: security-reverse-proxy

How to record "Reverse Proxy Settings: Run Behind a Load Balancer" without deciding anything. Two chapters are slides
(`01-why`, `04-safe`) and are drawn by `slides.py`; two clips are recorded in the browser (`02-enable`, `03-settings`).

In this file a selector stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. Paths such as
`OrchardCore.ReverseProxy/AdminMenu.cs:27` are in the Orchard Core source (`src/OrchardCore.Modules/`,
`src/OrchardCore.Themes/` or `src/OrchardCore/`) of `OrchardCMS/OrchardCore` at commit 92db4a83, the commit this plan
was written from.

## The site it needs

- A local `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore`, run directly on `http://localhost:5010`
  (`VIDEO_BASE_URL`). **There is no proxy in front of it, no stand-in for one, and no extra HTTP header on the
  browser**: the page shows the values of the local connection, and the narration says so (`settings-3`).
- `ORCHARD_APP_DATA` set to an empty folder outside both repositories.
- Provisioned by Auto Setup with these environment variables (the password is chosen outside git, passed in
  `VIDEO_PASSWORD`, and never written in a file of the repository; the plan calls it `<password>`):

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address for the site's process, so the site never calls out. These
  only block the site's own outbound calls; they have nothing to do with the reverse proxy of the video.
- The browser signs in once outside the capture (`Recorder setup`, user `admin`, `VIDEO_PASSWORD`), and the session is
  saved as `state.json` (git-ignored). The sign-in is never recorded.
- Features to enable before recording: **none**. The Blog recipe enables the admin features the clips use
  (`OrchardCore.Features`, `OrchardCore.Settings`, `OrchardCore.Deployment`, and the others) and does not enable
  `OrchardCore.ReverseProxy` (`TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). The video enables it on
  camera in `02-enable`.
- Settings to save before recording: none. The Reverse Proxy settings start empty, so all three headers are off
  (`OrchardCore.ReverseProxy/Settings/ReverseProxySettings.cs:8`, the enum default is `None`).
- Seed data: none. The video shows no content item, user or record other than the administrator.
- External services and stand-ins: none.
- Check before the first take: open `/Admin/Features`, search for `reverse`, and confirm that the Reverse Proxy
  Configuration row has an **Enable** button, not a **Disable** button (the feature is off). If it has **Disable**,
  restore snapshot A (below).

## Order, dependencies and snapshots

Record in this order: `02-enable`, then `03-settings`. The slide chapters need no site and are made by `assemble.py`.

| Snapshot | Taken | Holds |
|---|---|---|
| A | After Auto Setup and `Recorder setup`, before any take. Stop the site, copy `App_Data` and `state.json` aside. | Fresh Blog site, Reverse Proxy Configuration off. |
| B | After a good take of `02-enable`. Stop the site, copy `App_Data` and `state.json` aside. | The feature is on, no setting is saved. |

- `02-enable` starts from A and **changes the site** (it enables the feature, which restarts the tenant).
- `03-settings` starts from B and **changes the site** (it saves the three headers).
- To record `02-enable` again, restore A. To record `03-settings` again, restore B. Restore the `state.json` that was
  copied with the `App_Data`, and start the site again before each take.

## Before every clip

The admin menu keeps its groups open or closed in the browser. Before `StartCaptureAsync`, open `/Admin` with
`scene.OpenAsync`, and collapse any group that is open: for each of
`#left-nav button.nav-group-label:text-is("Tools")` and `#left-nav button.nav-group-label:text-is("Settings")`, if its
`aria-expanded` is `true`, click it once and wait for `aria-expanded` to be `false`. The groups are toggle buttons
because they have no page of their own (`TheAdmin/Views/NavigationItemLink-admin.cshtml:24,69,73`). Both groups are
therefore closed when the capture starts, and the steps open them on camera.

## Clip `02-enable`

Starts on `/Admin`, groups collapsed. Selectors used:

| Name | Selector |
|---|---|
| Tools group | `#left-nav button.nav-group-label:text-is("Tools")` |
| Features link | `#left-nav a.item-label[href$="/Admin/Features"]` |
| Search box | `#search-box` |
| Summary line | `#list-summary` |
| Feature name | `span.fw-semibold:text-is("Reverse Proxy Configuration")` |
| Category heading | `h3:text-is("Infrastructure")` |
| Enable link | `a[href*="OrchardCore.ReverseProxy/Enable"]` |
| Success toast | `.toast.message-success:has-text("The feature Reverse Proxy Configuration was enabled.")` |
| Settings group | `#left-nav button.nav-group-label:text-is("Settings")` |
| Reverse Proxy link | `#left-nav a.item-label:has-text("Reverse Proxy")` |

Steps:

- `enable-1`
  - Actions: `PauseAsync(1800)` (the first sentence is spoken over the dashboard). `ClickAsync(Tools group,
    pauseAfter: 700)`. `ClickAndWaitForNavigationAsync(Features link)`. `TypeAsync(Search box, "reverse", 70)`. Wait
    until `#list-summary` has the text `Showing 1 of` (the list is filtered on the page, no navigation).
  - On screen when the narration ends: the Features page with `reverse` in the search box, **Tools** expanded in the
    menu with **Features** current, the heading **Infrastructure**, and one row: **Reverse Proxy Configuration** and
    `OrchardCore.ReverseProxy`, with its description and a blue **Enable** button.
  - Labels from: `OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features); `Views/Admin/Features.cshtml:27` (the
    Search box) and `:88` (the category heading); `Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11` (name and id).
- `enable-2`
  - Actions: `HighlightAsync(Category heading)`, `PauseAsync(2200)`, `ClearHighlightsAsync()`, then
    `MoveToAsync(Enable link)` and stop there. Do not click yet.
  - On screen when the narration ends: the same page, no highlight, the cursor resting on **Enable**.
  - Labels from: `OrchardCore.ReverseProxy/Manifest.cs:4` (Reverse Proxy Configuration) and `:9` (Infrastructure);
    `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable).
- `enable-3`
  - Actions: `ClickAndWaitForNavigationAsync(Enable link)` at the very start of the step (the tenant restarts, and
    the page loads again; this wait is on camera, since the new page has to load before the call returns). Then
    `OffCameraAsync(() => Success toast.WaitForAsync())` as a guard: the toast is part of the loaded page, so the call
    returns at once. The toast has the text `The feature Reverse Proxy Configuration was enabled.` and stays on screen
    for 5 seconds (`OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`). `PauseAsync(2500)`.
    `ClickAsync(Settings group, pauseAfter: 600)`. `HighlightAsync(Reverse Proxy link)`.
  - On screen when the narration ends: the Features page, **Settings** expanded in the menu with **Reverse Proxy**
    ringed in green. The toast is gone by then, and that is expected. In the contact sheet, check that the restart
    after the click stays short and shows no error page.
  - Labels from: `OrchardCore.Features/Controllers/AdminController.cs:317` (the message);
    `OrchardCore.ReverseProxy/AdminMenu.cs:26-27` (Settings, Reverse Proxy) and `:31` (the entry needs Manage Reverse
    Proxy Settings); `OrchardCore.ReverseProxy/Permissions.cs:8` (Manage Reverse Proxy Settings) and `:22` (the
    Administrator role has it by default); `OrchardCore.Settings/Controllers/AdminController.cs:53` (the page asks for
    the group settings permission, which `Manage settings` implies: `OrchardCore/OrchardCore.Settings.Core/SettingsPermissions.cs:8,21`;
    `ReverseProxy` has no permission of its own registered for the group, unlike `OrchardCore.Email/Startup.cs:21`);
    `OrchardCore.ReverseProxy/Drivers/ReverseProxySettingsDisplayDriver.cs:53` (the options need Manage Reverse Proxy
    Settings); `OrchardCore.Settings/Permissions.cs:9,21` (the Administrator role has Manage settings by default) and
    `OrchardCore.Settings/Services/SuperUserHandler.cs:35` (a user in the Administrator role passes every check).

When the clip is recorded, copy the site aside as snapshot B.

## Clip `03-settings`

Starts on `/Admin`, groups collapsed (restored from snapshot B). Selectors used:

| Name | Selector |
|---|---|
| Settings group | `#left-nav button.nav-group-label:text-is("Settings")` |
| Reverse Proxy link | `#left-nav a.item-label:has-text("Reverse Proxy")` |
| Warning | `.alert.alert-warning` |
| X-Forwarded-For option | `.form-check:has(input[name$="EnableXForwardedFor"])` |
| X-Forwarded-Proto option | `.form-check:has(input[name$="EnableXForwardedProto"])` |
| X-Forwarded-Host option | `.form-check:has(input[name$="EnableXForwardedHost"])` |
| X-Forwarded-For box | `input[type="checkbox"][name$="EnableXForwardedFor"]` |
| X-Forwarded-Proto box | `input[type="checkbox"][name$="EnableXForwardedProto"]` |
| X-Forwarded-Host box | `input[type="checkbox"][name$="EnableXForwardedHost"]` |
| Request value 1, 2, 3 | `.hint:has-text("Current request value")`, `.Nth(0)`, `.Nth(1)`, `.Nth(2)` |
| Save | `button.save` |
| Success toast | `.toast.message-success:has-text("Site settings updated successfully.")` |

Each box selector names the type, because the check box tag helper also writes a hidden input with the same name
(`Views/ReverseProxySettings.Edit.cshtml:9,20,31`), and a locator that matches two elements fails when it clicks.

Steps:

- `settings-1`
  - Actions: `ClickAsync(Settings group, pauseAfter: 600)`. `ClickAndWaitForNavigationAsync(Reverse Proxy link)`.
    `PauseAsync(600)`. `HighlightAsync(Warning)`.
  - On screen when the narration ends: `/Admin/Settings/ReverseProxy`, the menu with **Settings** expanded and
    **Reverse Proxy** current, the yellow warning ringed in green, and the three options below it, all unchecked.
  - Labels from: `OrchardCore.ReverseProxy/AdminMenu.cs:26-27`; the route `OrchardCore.Settings/Controllers/AdminController.cs:50`
    with the group id `ReverseProxy` (`OrchardCore.ReverseProxy/Drivers/ReverseProxySettingsDisplayDriver.cs:19`);
    the warning text `The website might be restarted upon saving the settings, potentially leading to temporary
    unresponsiveness during the process.` (`OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`, added at
    `ReverseProxySettingsDisplayDriver.cs:58`), which the narration quotes in full.
- `settings-2`
  - Actions: `ClearHighlightsAsync()`. `HighlightAsync(X-Forwarded-For option)`, `PauseAsync(1400)`,
    `ClearHighlightsAsync()`; the same with the X-Forwarded-Proto option and the X-Forwarded-Host option. Then
    `HighlightAsync(X-Forwarded-For option)`, `PauseAsync(3500)`, `ClearHighlightsAsync()`.
  - On screen when the narration ends: the three options in this order, all unchecked, no highlight.
  - Labels from: `OrchardCore.ReverseProxy/Views/ReverseProxySettings.Edit.cshtml:10` (`Enables the forwarding of the
    HTTP header X-Forwarded-For`), `:21` (`...X-Forwarded-Proto`), `:32` (`...X-Forwarded-Host`). The page order is
    For, Proto, Host, and the narration follows it. The default is off: `ReverseProxySettings.cs:8`.
- `settings-3`
  - Actions: `HighlightAsync(Request value 1)`, `PauseAsync(3000)`, `ClearHighlightsAsync()`; the same with request
    value 2 and 3. Then `ClearHighlightsAsync()` and hold.
  - On screen when the narration ends: no highlight, and under the options three lines that start with
    `Current request value;`: `127.0.0.1`, `http`, and `localhost:5010` (the port of `VIDEO_BASE_URL`). The
    semicolon is part of the label on screen.
  - Labels from: `ReverseProxySettings.Edit.cshtml:13` (the address, from
    `OrchardCore/Modules/DefaultClientIPAddressAccessor.cs:17-23`, which turns every loopback address into
    `127.0.0.1`), `:24` (the scheme) and `:35` (the host, with its port).
- `settings-4`
  - Actions: `ClickAsync(X-Forwarded-For box, pauseAfter: 600)`, `ClickAsync(X-Forwarded-Proto box, pauseAfter: 600)`,
    `ClickAsync(X-Forwarded-Host box, pauseAfter: 600)`.
  - On screen when the narration ends: the three boxes checked, the cursor on the last one.
  - Labels from: `ReverseProxySettings.Edit.cshtml:9,20,31` (the boxes).
- `settings-5`
  - Actions: `ClickAndWaitForNavigationAsync(Save)` at the very start of the step (the page saves, the tenant
    restarts, and `/Admin/Settings/ReverseProxy` loads again; this wait is on camera). Then
    `OffCameraAsync(() => Success toast.WaitForAsync())` as a guard: the toast is part of the loaded page, so the call
    returns at once. The toast has the text `Site settings updated successfully.` and stays for 5 seconds. Then hold.
  - On screen when the narration ends: `/Admin/Settings/ReverseProxy` with the three boxes checked and the same three
    request values as in `settings-3` (nothing changes, since no proxy sends a header). The toast is gone by then, and
    that is expected. In the contact sheet, check that the restart after the click stays short and shows no error page.
  - Labels from: `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (Save);
    `OrchardCore.Settings/Controllers/AdminController.cs:119` (the message).

## The slides

`01-why` and `04-safe` have no recording. Each step shows one slide of `slides.py`:

- `why-1`: "A proxy stands in front of your site", three cards: Visitor, Reverse proxy or load balancer, Your site.
- `why-2`: "Headers carry the original details", three cards: X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host.
- `why-3`: "Features that use the visitor's address", three cards: Audit Trail, Rate Limits, Without the headers.
- `safe-1`: "Trust only your own proxies", three cards: IP spoofing, Header injection, The rule.
- `safe-2`: "Known proxies and known networks", three cards: Known proxies, Known networks, Not on this page.
- `safe-3`: "When the configuration takes over", three cards: Configuration first, A message tells you, Saving.
- `safe-4`: "Per tenant, and in deployment plans", two cards: Per tenant, Deployment plans.

Where the slide texts come from: the three headers and what they carry are in the module's documentation page
(`reference/modules/ReverseProxy/README.md`, "Admin Configuration"); the Audit Trail records the client address
(`OrchardCore.AuditTrail/Services/AuditTrailManager.cs:96`, setting "Allow client IP address"); Rate Limits count
anonymous visitors by remote address (`reference/modules/RateLimits/README.md`, "Who shares a budget" and "Reverse
proxies and load balancers"); the risks and the rule are in the page's "Security Considerations"; the precedence and
the message are in "Configuration Precedence" and at `ReverseProxySettingsDisplayDriver.cs:66`; the deployment step
name is `OrchardCore.ReverseProxy/Startup.cs:42` (`Reverse Proxy settings`).

## Checks for private data

- The only account on screen is `admin` with `admin@example.com`. The recorded pages show no other user, no real name,
  no email and no token.
- The page shows `127.0.0.1` and `localhost:5010` only. Never open the site by the machine's own name or by a real
  address: the host line (`settings-3`, `settings-5`) would show it.
- The page ends with a hint that shows, as plain text, the address of the ASP.NET Core documentation about proxies
  (`ReverseProxySettings.Edit.cshtml:42`); it names no person.
- The password is never typed or shown on camera: the session comes from `state.json`.
- After assembling, look at the contact sheet of each clip, and OCR the frames at every scene change, as the skill
  asks, before the video is published.

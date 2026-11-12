# Recording plan: Go to Production

Two chapters of this video are recordings (`03-proxy` and `04-https`). Every other chapter is a card or slides, drawn
from `storyboard.json` and `slides.py`, so nothing else needs a browser. A selector below stands for
`scene.Page.Locator(...)`. Selectors are derived from the views of the Orchard Core source (file:line, relative to
the root of `OrchardCMS/OrchardCore`), not from a running site: confirm each one with `Recorder probe` before the
first take (the commands are in "Check the selectors").

## The site

- A checkout of `OrchardCMS/OrchardCore` at the commit the catalog was read from (`92db4a83`) or a later `main`.
  Run `src/OrchardCore.Cms.Web` on `http://localhost:5010`, over plain HTTP only. This video does not need an HTTPS
  listener: the HTTPS clip records the page opened over HTTP on purpose (see `OPEN-QUESTIONS.md`, question 1).
- Provision it with Auto Setup through environment variables, so no setup screen and no sign-in form is recorded.
  The site is not uninstalled when recording, so the Setup screen does not appear in this video.
- Sign in as user `admin`, email `admin@example.com`, with the password chosen outside git and passed as
  `VIDEO_PASSWORD` (written `<password>` here; never write a real one in a file of this repository).
- Name the site `Orchard Core`, time zone `America/Los_Angeles`, recipe `Blog`, database SQLite.
- Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls a service.
  NuGet reads the same two variables, so build the project first in a shell that does not set them
  (`dotnet build src/OrchardCore.Cms.Web`), and start the site with `--no-build`.

```bash
export ORCHARD_APP_DATA=<an empty folder outside both repositories>
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:5010
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
dotnet run --project src/OrchardCore.Cms.Web --no-launch-profile --no-build
```

The option names are those of `TenantSetupOptions`
(`src/OrchardCore.Modules/OrchardCore.AutoSetup/Options/TenantSetupOptions.cs`). `--no-launch-profile` keeps the
launch profile of the project (`src/OrchardCore.Cms.Web/Properties/launchSettings.json`) from replacing the URL.

### Features to enable before recording

Outside the capture, at `/Admin/Features`, enable the two features the clips talk about, with the buttons of the
Features page (`src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`):

- `HTTPS`, feature id `OrchardCore.Https`: the button `#btn-enable-OrchardCore_Https`.
- `Reverse Proxy Configuration`, feature id `OrchardCore.ReverseProxy`: the button `#btn-enable-OrchardCore_ReverseProxy`.

The Blog recipe enables neither (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). Nothing else is
enabled or disabled.

### Settings and seed data

No settings are saved and no content is created before recording. Both settings pages must be in their default
state when a take starts:

- `/Admin/Settings/ReverseProxy`: the three boxes are unchecked.
- `/Admin/Settings/Https`: Require HTTPS unchecked, HSTS mode `Disabled`, Require HTTPS Permanent unchecked, SSL Port
  empty (the page is opened over HTTP, so nothing is pre-filled).

No external service is used, so there is no stand-in. The only people and addresses on screen are `admin`,
`admin@example.com`, the site name `Orchard Core`, and the loopback address and `localhost:5010` that the Reverse
Proxy page prints for the current request.

### Sign-in

With the site running, from `.agents/skills/orchardcore-video-producer/scripts/recorder`:

```bash
export VIDEO_ROOT=<repo>/videos/go-to-production VIDEO_BASE_URL=http://localhost:5010
export VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password> MSYS_NO_PATHCONV=1
<bin>/Recorder setup
```

`state.json` stays in the folder and is ignored by git.

## Order of the clips, and the snapshot

1. Start the site, enable the two features, then stop it and copy `ORCHARD_APP_DATA` and `state.json` aside. This
   copy is the snapshot.
2. Record `03-proxy`. **It changes the site**: it saves the Reverse Proxy settings, which restarts the tenant.
3. Record `04-https`. It changes nothing: the HTTPS page does not save its fields over HTTP, and the clip saves nothing.

The two clips do not depend on each other, so either can be recorded first. Before another take of `03-proxy`, stop the
site, restore the snapshot, and start it again, so the three boxes start unchecked. `04-https` can be repeated at any time.

## Clip `03-proxy` (chapter 03, Reverse proxy)

Starts on `/Admin/Features`, opened with `scene.OpenAsync` before `StartCaptureAsync`, with an empty search box.
The row of a feature is
`scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-enable-OrchardCore_ReverseProxy, #btn-disable-OrchardCore_ReverseProxy") }).First`.

`proxy-1`
- Actions: `TypeAsync` in `#search-box` the text `reverse` (delay 60). Wait until the row of the feature is visible,
  then `HighlightAsync` on the row with the caption `Reverse Proxy Configuration`.
- On screen when the narration ends: the Features list reduced to the Reverse Proxy Configuration row, with a green
  ring and its caption, and the red `Disable` button (the feature is enabled).
- Labels: the search box `Features.cshtml:27`; the buttons `FeatureEntry.Buttons.SummaryAdmin.cshtml:40,53`; the
  feature name `src/OrchardCore.Modules/OrchardCore.ReverseProxy/Manifest.cs:4`; the menu `Tools > Features`
  `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`.

`proxy-2`
- Actions: `ClearHighlightsAsync`. `ClickAsync` the Settings group of the admin menu, the button `#settings .nav-group-label`, only when its
  `aria-expanded` is `false` (an item with children and no page of its own is a button,
  `src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24,29,73`; the menu is rendered collapsed and
  `Assets/js/TheAdmin/sidebar.ts` opens the group of the current page and what the browser stored, so the group is
  closed in a fresh browser context, and clicking an open group would close it). Then
  `ClickAndWaitForNavigationAsync` on the link `a[href="/Admin/Settings/ReverseProxy"]`. On the new page, ring each box
  in turn with `HighlightAsync` (one ring at a time, `ClearHighlightsAsync` between them, `PauseAsync(900)` after each):
  the label `scene.Page.GetByLabel("Enables the forwarding of the HTTP header X-Forwarded-For")` with the caption
  `Client address`, then `...X-Forwarded-Proto` with `Protocol`, then `...X-Forwarded-Host` with `Host`. End with
  `ClearHighlightsAsync`.
- On screen when the narration ends: the Reverse Proxy settings page with the amber warning above the form, the three
  unchecked boxes with their labels, and the hint line under each box. No ring.
- Labels: the menu path `Settings > Reverse Proxy` `src/OrchardCore.Modules/OrchardCore.ReverseProxy/AdminMenu.cs:26-27`;
  the route `Settings/{groupId}` `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:50`;
  the three labels `ReverseProxySettings.Edit.cshtml:10,21,32` (in `src/OrchardCore.Modules/OrchardCore.ReverseProxy/Views/`);
  the warning `src/OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`.

`proxy-3`
- Actions: ring the three hint lines in turn, `scene.Page.Locator("span.hint", new() { HasTextString = "Current request value" })`
  with `.Nth(0)` (the client address), `.Nth(1)` (the protocol) and `.Nth(2)` (the host), about one second each, no
  caption, then `ClearHighlightsAsync`. Then `ClickAsync` the three boxes in this order, `pauseAfter: 500`:
  `input[name$=".EnableXForwardedFor"]`, `input[name$=".EnableXForwardedProto"]`, `input[name$=".EnableXForwardedHost"]`.
- On screen when the narration ends: the three boxes checked, the three hint lines readable. Under the boxes the
  page prints the current request: the loopback address, `http`, and `localhost:5010`.
- Labels: `ReverseProxySettings.Edit.cshtml:13,24,35` (the text `Current request value;` is printed by the page, the
  narration does not read it); at the bottom the page also prints `Documentation available at:` and a link to
  Microsoft's documentation (`:42`), which is part of the page and is expected in the capture; the field names `ReverseProxySettingsViewModel.cs` (`EnableXForwardedFor`,
  `EnableXForwardedProto`, `EnableXForwardedHost`).

`proxy-4`
- Actions: `HighlightAsync` on the warning, `scene.Page.Locator(".alert-warning", new() { HasTextString = "The website might be restarted" })`,
  hold two seconds, `ClearHighlightsAsync`. Then `ClickAndWaitForNavigationAsync` on `button.save` (the Save button),
  and wait until `scene.Page.GetByText("Site settings updated successfully.")` is visible. If the restart of the tenant
  makes the wait longer than the narration, put the wait inside `scene.OffCameraAsync`.
- On screen when the narration ends: the Reverse Proxy page reloaded, the green message `Site settings updated
  successfully.` at the top, and the three boxes still checked.
- Labels: the button `SiteSettings.SaveButton.cshtml:2` and the message `Controllers/AdminController.cs:119` (both in
  `src/OrchardCore.Modules/OrchardCore.Settings/`); the warning `Views/Settings-Reload.Wrapper.cshtml:4`.
- This clip **changes the site** (see the snapshot above).

## Clip `04-https` (chapter 04, HTTPS)

Starts on `/Admin/Features`, opened before `StartCaptureAsync`, with an empty search box. The page of this clip is
opened over HTTP, as everything else. Nothing is saved.

`https-1`
- Actions: `TypeAsync` in `#search-box` the text `https` (delay 70). Wait until the row of the feature
  (`#btn-enable-OrchardCore_Https, #btn-disable-OrchardCore_Https`, filtered as in `03-proxy`) is visible, ring it with
  the caption `HTTPS`, hold about one second, `ClearHighlightsAsync`. Then `ClickAsync` the Settings group
  (`#settings .nav-group-label`, only when its `aria-expanded` is `false`), `ClickAsync` the Security group inside it
  (the button `button.item-label` that contains the text `Security`, again only when its `aria-expanded` is `false`),
  and `ClickAndWaitForNavigationAsync` on `a[href="/Admin/Settings/Https"]`.
- On screen when the narration ends: the HTTPS settings page with the warning message `For safety, changing HTTPS-only
  settings over HTTP has been prevented.` at the top, the amber restart warning, and the form with the two check boxes
  (Require HTTPS, Require HTTPS Permanent) and the SSL Port field disabled (grayed). The HSTS mode list is not
  disabled: the view's `asp-is-disabled` attribute is only handled for `input` elements (see `OPEN-QUESTIONS.md`,
  question 1). Do not open the list or change it.
- Labels: the feature name `src/OrchardCore.Modules/OrchardCore.Https/Manifest.cs:4`; the menu path
  `Settings > Security > HTTPS` `OrchardCore.Https/AdminMenu.cs:26-28`; the message `Drivers/HttpsSettingsDisplayDriver.cs:58`;
  the disabled inputs `Views/HttpsSettings.Edit.cshtml:6,30,40` (the attribute `asp-is-disabled`, handled for
  `input` elements by `src/OrchardCore/OrchardCore.DisplayManagement/TagHelpers/InputIsDisabledTagHelper.cs:5`); the list at `:16`
  carries the same attribute, which that tag helper does not handle.

`https-2`
- Actions: ring the row of the first box with
  `scene.Page.Locator(".form-check").Filter(new() { HasText = "Require HTTPS for all requests" })` and the caption
  `Redirects all requests`, hold about four seconds, `ClearHighlightsAsync`. Then ring the message
  `scene.Page.GetByText("For safety, changing HTTPS-only settings over HTTP has been prevented.")` with the caption
  `Open the page over HTTPS`, and keep it until the step ends.
- On screen when the narration ends: the message at the top of the page has a green ring and its caption.
- Labels: `Views/HttpsSettings.Edit.cshtml:7,8`; `Drivers/HttpsSettingsDisplayDriver.cs:58,85`.

`https-3`
- Actions: `ClearHighlightsAsync`, ring the drop-down list `select[name$=".StrictTransportSecurityMode"]` with the caption
  `HSTS mode`, hold about four seconds, `ClearHighlightsAsync`, then ring the red notice
  `.alert-danger` (the text starts `Use HSTS with caution`) and keep it until the step ends. The list is closed and
  shows only its current value, `Disabled`; the three options are named by the narration and shown on the slide of step `https-5`.
- On screen when the narration ends: the red notice with a green ring.
- Labels: `Views/HttpsSettings.Edit.cshtml:14,17,18,19,22` (`HSTS mode`, `Disabled`, `Enabled`,
  `From environment — enabled in Production, disabled otherwise`, the notice). The narration reads the third option as
  `From environment: enabled in Production, disabled otherwise`.

`https-4`
- Actions: `ClearHighlightsAsync`. Ring the row `scene.Page.Locator(".form-check").Filter(new() { HasText = "Require HTTPS Permanent" })`
  with the caption `Permanent redirect`, hold about four seconds, `ClearHighlightsAsync`. If the SSL Port field is
  below the fold at 1536x864, `ScrollAsync(0, 300)` (after clearing the ring: rings are fixed-position). Then ring
  `input[name$=".SslPort"]` with the caption `Redirect port` and keep it until the step ends.
- On screen when the narration ends: the SSL Port field, empty and disabled, with a green ring, and its hint line
  `The SSL Port Number, leave empty to let the middleware try to determinate it.` under it.
- Labels: `Views/HttpsSettings.Edit.cshtml:31,38,42`; the status code `src/OrchardCore.Modules/OrchardCore.Https/Startup.cs:54`.
- This clip changes nothing.

## Slide steps

These steps are drawn by `slides.py`; the recorder does nothing for them.

- `checklist-1`, `checklist-2`: four cards, from your machine to a live site (environment, settings, requests, keys); the
  first two are bright on the first step, the last two on the second.
- `config-1`: `Program.cs` of the web project, trimmed, with the exception handler outside Development, the environment
  variable and `appsettings.Production.json`.
- `config-2`: the database provider and connection string in the OrchardCore section, and the same two as environment variables, with a note that a tenant can set its own.
- `config-3`: an `AllowedHosts` line of `appsettings.json`, and three notes on host names, where to set it, and credentials.
- `config-4`: the layout of `App_Data` and the `ORCHARD_APP_DATA` variable.
- `proxy-5`: three cards on the order of the pipeline: the reverse proxy, then CORS and Security (the same order value, so one card), then authentication.
- `proxy-6`: `ConfigureReverseProxySettings` in the host and the ReverseProxy section, with the call and the section name bright.
- `proxy-7`: the same section with the known networks and the known proxies bright.
- `https-5`: the Settings step of a recipe with the HttpsSettings values, and three notes naming the HSTS values: Disabled, Enabled and FromConfiguration.
- `https-6`: three cards on the HSTS header: maximum age, subdomains, preload.
- `keys-1`: the default key ring folder, `Sites/Default/DataProtection-Keys` under `App_Data`, bright, and two notes.
- `keys-2`: three cards on why the key ring must be shared.
- `keys-3`: the Azure section of DataProtection and two notes (default container, `CreateContainer`).
- `keys-4`: container and blob names as Liquid templates, and the default blob path.
- `keys-5`: the Redis section, and two notes on persistence and `AllowAdmin`.
- `keys-6`: three cards on checking the log, and the fallback to the keys in `App_Data`.
- `ready-1`: four checked cards, the checklist.
- `ready-2`: three cards on what can be scripted: the host, configuration, a recipe.

## Check the selectors

Run these once with the site up and signed in, and fix the plan where an answer differs from what is written above
(a selector that matches nothing, or more than one element):

```bash
export MSYS_NO_PATHCONV=1
<bin>/Recorder probe /Admin/Features "() => [...document.querySelectorAll('[id^=btn-]')].filter(b => /Https|ReverseProxy/.test(b.id)).map(b => b.id)"
<bin>/Recorder probe /Admin/Settings/ReverseProxy "() => [...document.querySelectorAll('input[type=checkbox]')].map(i => i.name)"
<bin>/Recorder probe /Admin/Settings/Https "() => [...document.querySelectorAll('input,select')].map(i => i.name + ' ' + i.disabled)"
<bin>/Recorder probe /Admin/Settings/Https "() => document.body.innerText.includes('For safety')"   # must print true: the message is on the page of the same request
<bin>/Recorder probe /Admin/Features "() => [...document.querySelectorAll('#settings, #settings .nav-group-label, a[href=\"/Admin/Settings/Https\"], a[href=\"/Admin/Settings/ReverseProxy\"]')].map(e => e.tagName + ' ' + (e.id || e.getAttribute('href') || e.className) + ' ' + e.getAttribute('aria-expanded'))"
```

## Checks for private data

- Before recording, confirm that the site holds only the data above: no other user, no content that is not from the
  Blog recipe.
- After recording, look at every contact sheet (`review.py`), then OCR the video at one frame per second and at
  every scene change. The only names and addresses allowed are `admin`, `admin@example.com`, `Orchard Core`,
  `localhost:5010`, a loopback address, and the Microsoft documentation link that the Reverse Proxy page prints
  itself (`ReverseProxySettings.Edit.cshtml:42`).
- No password, token, connection string or machine path may appear: the password is typed outside the capture
  (`Recorder setup`), and none of the clips opens the sign-in page.
- Stop the site by its process ID, never by image name, and delete the copies of `ORCHARD_APP_DATA` and `state.json`
  when the video is published.

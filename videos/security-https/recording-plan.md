# Recording plan: security-https

Two recordings (`02-features`, `03-settings`) and eight slides. Everything is recorded in a browser, over HTTPS, on a
fresh site with no data of its own. Labels and selectors below were read from the Orchard Core source (commit
92db4a83, paths relative to the repository root), not run on a site: check each selector with `Recorder probe` before
recording, and look at every contact sheet.

## The site it needs

- **Orchard Core:** `src/OrchardCore.Cms.Web` of OrchardCMS/OrchardCore, run with `ASPNETCORE_ENVIRONMENT=Development`,
  so that Kestrel can use the ASP.NET Core development certificate (see `OPEN-QUESTIONS.md`, question 1).
- **The recording is made over HTTPS.** Over HTTP the HTTPS settings page ignores what is saved
  (`OrchardCore.Https/Drivers/HttpsSettingsDisplayDriver.cs:85-88`), shows a warning message (`:54-59`), and disables
  the two check boxes and SSL Port (`OrchardCore.Https/Views/HttpsSettings.Edit.cshtml:6, 30, 40`). A page opened over
  HTTP is therefore no use for the clips.
  Make the site listen on one HTTPS address only, for example
  `dotnet run --project src/OrchardCore.Cms.Web --no-launch-profile --urls https://localhost:5011`, and record with
  `VIDEO_BASE_URL=https://localhost:5011`.
- **Certificate:** the recorder's browser context does not ignore certificate errors, so the machine must trust the
  ASP.NET Core development certificate (`dotnet dev-certs https --trust`). Confirm once, before recording, that
  `Recorder probe /Admin "() => document.title"` loads without a certificate error (see `OPEN-QUESTIONS.md`).
- **Provisioning:** Auto Setup, as described in the `orchardcore-tester` skill of the Orchard Core repository
  (`references/autosetup.md`): recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`,
  email `admin@example.com`, password `<password>` (the one passed in `VIDEO_PASSWORD`, chosen outside git).
  `ORCHARD_APP_DATA` points at an empty folder outside both repositories. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead
  address in the site's process only, so the site never calls an outside service.
- **Features to enable before recording:** none. The Blog recipe already enables Features, Settings, Navigation and
  Security (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:38-44`). The HTTPS feature must still be
  **disabled** when the recording starts: on Tools, Features its button must read Enable.
- **Settings:** the HTTPS settings stay at their defaults: Require HTTPS cleared, HSTS mode Disabled, Require HTTPS
  Permanent cleared, SSL Port empty (`OrchardCore.Https/Settings/HttpsSettings.cs:20-35`). The SSL Port field is empty
  in the stored settings only: while Require HTTPS is not saved, the page fills the field with the port of the address
  it was opened on, here `5011` (`Drivers/HttpsSettingsDisplayDriver.cs:65-68`), so `03-settings` shows `5011` there.
- **External services:** none. No mail, SMS or outside call is made.
- **Seed data:** none. The clips show only the admin menu, the Features list and the HTTPS settings page. The only
  account is `admin` / `admin@example.com`.
- **Sign-in:** outside the capture, run `Recorder setup` (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`,
  `VIDEO_BASE_URL=https://localhost:5011`). It saves `state.json`, which stays out of git. There is no `PrepareAsync`
  and no `Recorder prepare` for this video.

## Order of the clips and snapshots

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-features` | snapshot **S0** | Yes: enables the HTTPS feature |
| 2 | `03-settings` | snapshot **S1** (the state `02-features` leaves) | Yes: saves the HTTPS settings (Require HTTPS selected, HSTS mode From environment) |

`03-settings` depends on `02-features`: the menu item Settings, Security, HTTPS exists only once the feature is enabled.

- **S0:** the provisioned site with `state.json` saved, before any clip. Stop the site, copy its `App_Data` folder and
  `state.json` aside, and start it again.
- **S1:** after a good take of `02-features`: stop the site, copy `App_Data` and `state.json` aside again.
- To record again: stop the site, restore S0 (full take, or a new take of `02-features`) or S1 (a new take of
  `03-settings`), and start the site.
- Saving the HTTPS settings reloads the tenant and leaves the settings on the site (Require HTTPS selected). Restore
  S1 before every new take of `03-settings`.

## Selectors

The menu is the `nav` element `#left-nav` (`OrchardCore.Themes/TheAdmin/Views/Navigation-admin.cshtml:24`). A group
such as Tools, Settings or Security has no page of its own, so it is a button that expands it; a page is a link
(`OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24-40`). Menus start collapsed in every new
browser context. In the lists below, a selector stands for `scene.Page.Locator(...)`.

- Menu group: `#left-nav` then the button named exactly `Tools`, `Settings` or `Security`
  (`scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = "Settings", Exact = true })`).
- Menu page: `#left-nav` then the link named exactly `Features` or `HTTPS`
  (`GetByRole(AriaRole.Link, new() { Name = "HTTPS", Exact = true })`).
- Settings fields: `input[type=checkbox][name$='.RequireHttps']`, `select[name$='.StrictTransportSecurityMode']`,
  `input[type=checkbox][name$='.RequireHttpsPermanent']`, `input[name$='.SslPort']`. The field names end in the
  property name whatever the prefix of the form is.
- Save button: `button.save` (`OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`).

## Clip `02-features`

**Starts on:** `/Admin`, opened with `OpenAsync` before the capture, over HTTPS. The page is loaded, the menu is
collapsed, and the cursor rests in the middle. Messages in this clip are toasts that disappear after 5 seconds
(`OrchardCore.Themes/TheAdmin/Views/Message.cshtml:20`, `OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`);
the container is fixed to the end edge of the page (`OrchardCore.Themes/TheAdmin/Views/NotifyMessages.cshtml:3-12`).
Look at the contact sheet to see where it sits vertically (see `OPEN-QUESTIONS.md`, question 12).

### `features-1`

- Click the button **Tools** in the left menu, then click the link **Features** with `ClickAndWaitForNavigationAsync`.
- With `TypeAsync` (delay 90), type `HTTPS` into `#search-box`.
- **On screen when the narration ends:** the Features page, with HTTPS in the search box and the list narrowed to the
  rows that match. The Security category heading is above the HTTPS row. A second row can appear, because the
  description of another feature holds a link that starts with `https://` (`OrchardCore.Modules/OrchardCore.Tenants/Manifest.cs:29`);
  that is expected and holds no private data.
- **Labels:** Tools `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26`; Features `AdminMenu.cs:27`; the search box
  `OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`.

### `features-2`

- `HighlightAsync` on the HTTPS row (the `.list-group-item` that contains `#btn-enable-OrchardCore_Https`), with no
  caption. If the list is shown as a grid, highlight the row that contains the same button.
- **On screen when the narration ends:** the ring around the HTTPS row, showing the name HTTPS, the description
  "This module will ensure HTTPS is used when accessing the website.", and the button Enable.
- **Labels:** name `OrchardCore.Modules/OrchardCore.Https/Manifest.cs:4`, description `Manifest.cs:8`, category
  `Manifest.cs:9`, category heading `OrchardCore.Features/Views/Admin/Features.cshtml:88`.

### `features-3`

- `ClearHighlightsAsync`, then click `#btn-enable-OrchardCore_Https` with `ClickAndWaitForNavigationAsync`. Click at
  the start of the step: the page reloads on the Features list and shows the message for 5 seconds, and the narration
  of this step is about 4 seconds long.
- **On screen when the narration ends:** the Features page after the reload, with the green message "The feature HTTPS
  was enabled." in its toast. If the reload takes longer than the narration, the step lasts until the message is
  on screen.
- **Labels:** Enable `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; message
  `OrchardCore.Features/Controllers/AdminController.cs:317`.

## Clip `03-settings`

**Starts on:** `/Admin`, opened with `OpenAsync` before the capture, over HTTPS, with the menu collapsed. The site is
in snapshot S1: the HTTPS feature is enabled and the settings are at their defaults.

### `settings-1`

- Click the button **Settings**, then the button **Security**, then click the link **HTTPS** with
  `ClickAndWaitForNavigationAsync`. The three clicks take less time than the narration.
- Then `HighlightAsync` on the warning at the top of the form (`p.alert.alert-warning`).
- **On screen when the narration ends:** the HTTPS settings page (`/Admin/Settings/Https`) with the yellow warning "The
  website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness during the
  process." ringed, and the four settings below it, **enabled** (the two check boxes and SSL Port are not greyed out).
  No message about changing settings over HTTP may be on screen: if one is, the page was opened over HTTP and the
  recording must be redone over HTTPS.
- **Labels:** Settings `OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36`; Security
  `OrchardCore.Modules/OrchardCore.Https/AdminMenu.cs:27`; HTTPS `AdminMenu.cs:28`; warning
  `OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`.

### `settings-2`

- `ClearHighlightsAsync`, then click `input[type=checkbox][name$='.RequireHttps']` with `ClickAsync`.
- **On screen when the narration ends:** Require HTTPS selected, with its hint "Require HTTPS for all requests".
  Require HTTPS Permanent stays cleared.
- **Labels:** `OrchardCore.Modules/OrchardCore.Https/Views/HttpsSettings.Edit.cshtml:7-8`.

### `settings-3`

- `HighlightAsync` on `select[name$='.StrictTransportSecurityMode']`, then on the red notice under it
  (`.alert.alert-danger`), then `ClearHighlightsAsync`. Near the end of the narration, when it says "Let's choose From
  environment", call `SelectAsync` on the select with the value `FromConfiguration`. A headless browser draws no open
  list, so only the new value shows.
- **On screen when the narration ends:** the label HSTS mode, the select showing "From environment — enabled in
  Production, disabled otherwise", and below it the red notice "Use HSTS with caution, as browsers may stop connecting over
  HTTP after receiving the header if HTTPS later becomes unavailable."
- **Labels:** label `HttpsSettings.Edit.cshtml:14`; options `:17-19`; notice `:22`.

### `settings-4`

- `HighlightAsync` on the row of `input[type=checkbox][name$='.RequireHttpsPermanent']` (its `.form-check`). Do not click.
- **On screen when the narration ends:** the ring around Require HTTPS Permanent, cleared, with its hint. Do not read
  the hint aloud: it names the status codes 308 and 307 and gives them the names of 301 and 302 (see `OPEN-QUESTIONS.md`).
- **Labels:** `HttpsSettings.Edit.cshtml:31-32`.

### `settings-5`

- `ClearHighlightsAsync`, then `HighlightAsync` on `input[name$='.SslPort']`. Do not type.
- **On screen when the narration ends:** the ring around SSL Port, which shows `5011`, the port of the page, and its
  hint "The SSL Port Number, leave empty to let the middleware try to determinate it." If the field is empty, the site
  was opened on the default HTTPS port, or Require HTTPS was already saved: run it on port 5011 as above, from snapshot
  S1.
- **Labels:** `HttpsSettings.Edit.cshtml:38-42`; the value comes from `Drivers/HttpsSettingsDisplayDriver.cs:65-68`,
  which fills the field with the port of the page only while Require HTTPS is not saved.

### `settings-6`

- `ClearHighlightsAsync`, then click `button.save` (the button Save) with `ClickAndWaitForNavigationAsync`. Click at the
  start of the step. The site reloads its tenant on save, so the page can take a few seconds; the step lasts until
  the page is back.
- **On screen when the narration ends:** the HTTPS settings page after the reload, with the green message "Site
  settings updated successfully." in its toast (visible 5 seconds after the page loads).
- **Labels:** Save `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; message
  `OrchardCore.Settings/Controllers/AdminController.cs:119`.

### `settings-7`

- No click. Optionally `HighlightAsync` on the Require HTTPS checkbox, then on the HSTS mode select. Clear the
  highlights at the end.
- **On screen when the narration ends:** the saved settings: Require HTTPS selected, HSTS mode reading "From
  environment — enabled in Production, disabled otherwise", Require HTTPS Permanent cleared, SSL Port `5011`.

## Slides

One image per step, drawn by `slides.py`.

- `why-1`: Plain HTTP (readable text) next to HTTPS (an encrypted connection).
- `what-1`: three cards: a request over HTTP, the redirect, the request over HTTPS.
- `what-2`: the four settings in the order of the page: Require HTTPS, HSTS mode, Require HTTPS Permanent, SSL Port.
- `what-3`: the HSTS header (365 days, subdomains included) and the three modes: Disabled, Enabled, From environment.
- `what-4`: the page opened over HTTPS (settings can be edited and saved) and over plain HTTP (the changes you save are
  not applied), with the message the page shows.
- `after-1`: the saved settings are applied, and requests over HTTP are redirected on every page.
- `after-2`: the settings belong to the tenant, and saving reloads the tenant (the page warns the website might be restarted).
- `after-3`: behind a load balancer: it handles HTTPS, the site sees HTTP and the page ignores the changes you save, the
  Reverse Proxy settings can forward the protocol.

## Checks for private data

- The screen shows only the admin menu, the Features list and the HTTPS settings page. Look for the account name
  `admin` and the address `admin@example.com` only; no other person, address or company appears.
- The password `<password>` is never typed on camera: the sign-in happens in `Recorder setup`, outside the capture.
  No browser password prompt appears.
- No machine name, user folder or path of this machine is visible; the only host in the capture is `localhost`.
- Run the OCR scan of the skill (one frame per second, every scene change, then five frames per second) on both clips
  and search the text for names, email addresses other than `admin@example.com`, and tokens.
- Do not commit `state.json`, a copy of `App_Data`, or any audio, image or MP4.

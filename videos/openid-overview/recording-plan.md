# Recording plan: OpenID Connect: Orchard Core as an Identity Provider

A person who reads only this file can record the video. Labels come from the Orchard Core source (the file and line are
given for each); `OPEN-QUESTIONS.md` lists what could not be checked without a running site. In this plan a selector
stands for `scene.Page.Locator(...)`. Confirm each selector once with `Recorder probe <path> "<script>"` before the take.
File references are relative to `src/OrchardCore.Modules` of the Orchard Core source at commit 92db4a83, unless they start
with `src/`.

The video has seven chapters. Chapters 01 and 02 and the slide of chapter 05 are slides (nothing to record). Chapters 03,
04 and 05 hold the three recorded clips: `03-features`, `04-enable` and `05-tour`.

## The site it needs

- **Source:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, run on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). The recording needs
  no Orchard Core change, so any checkout that builds will do.
- **Setup:** unattended with Auto Setup, so no setup or login form is recorded (see the `orchardcore-tester` skill of the
  Orchard Core repository, and `src/docs/reference/modules/AutoSetup/README.md`). Environment variables:

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>    # the password chosen outside git (VIDEO_PASSWORD)
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

- **Tenant:** the Default tenant, on a fresh `App_Data`. No other tenant.
- **Recipe:** Blog. Its `feature` step enables `OrchardCore.Features` (so Tools, Features exists) and does not enable any
  OpenID Connect feature, `OrchardCore.Secrets` or `OrchardCore.Users.ExternalAuthentication`
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-80`). Nothing OpenID Connect related may be enabled
  before the take: the video enables the authorization server itself.
- **Features to enable before recording:** none.
- **Settings to save before recording:** none. Do not open or save any OpenID Connect settings page before the take (the
  video shows the server settings page as it is before anyone has saved it).
- **External services and local stand-ins:** none. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, as
  `references/recording.md` asks, so the site calls nothing.
- **Seed data (create outside the capture):** only the administrator that Auto Setup creates: user `admin`, email
  `admin@example.com`, password `<password>`. There are no applications, scopes, secrets, certificates or content items to
  create, and none may exist: the Applications list must be empty when `tour-5` is recorded.
- **Sign in once outside the capture:** `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`
  writes `state.json` (git-ignored). The clips start already signed in.
- **Viewport and look:** the recorder's defaults (1536x864 CSS pixels, light scheme, `en-US`). Leave the admin list layout
  at its default (List): the Features page, the Applications page and the Scopes page then show a description line under each
  title (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`).

## Order, dependencies and snapshots

1. Start the site, let Auto Setup finish, run `Recorder setup`, then **stop the site and copy `App_Data` and `state.json`
   aside as snapshot A** (a fresh site, nothing enabled).
2. Record `03-features` from snapshot A. It changes nothing on the site, so it can be taken again at any time while the
   site is still in snapshot A.
3. Restore snapshot A, start the site, record `04-enable`. **This clip changes the site**: it enables the OpenID Connect
   Authorization Server, Management UI and Core Services features, and the tenant creates its default scopes (Email,
   Profile, Phone, Roles) and, when it first needs signing and encryption keys, its own certificates in its `App_Data`
   folder. Before a new take of `04-enable`, stop the site and restore snapshot A again.
4. After `04-enable`, stop the site and copy `App_Data` and `state.json` aside as **snapshot B**.
5. Record `05-tour` from snapshot B (it needs the features enabled by `04-enable`). It changes nothing (it saves no
   setting), so it can be taken again from the same site; restore snapshot B if anything was saved by mistake.

Clips that depend on each other: `05-tour` needs `04-enable`. `03-features` is independent, and is best recorded before
`04-enable` so that the narration of `features-2` matches a site where every OpenID Connect feature is disabled.

## Clip `03-features`

**Starts on:** the admin home page, `/Admin`, before the capture (the Blog recipe does not enable a dashboard feature, so the
page is whatever the admin theme shows at `/Admin`). Before `StartCaptureAsync`, expand the admin menu group **Tools** so that
its **Features** item is visible (`OrchardCore.Features/AdminMenu.cs:26-27`).

- `features-1`: hold on the admin home page for a moment with **Tools** expanded, then `ClickAndWaitForNavigationAsync` the menu
  item **Features** (link text "Features", inside the **Tools** group; fallback: `NavigateAsync("/Admin/Features")`). When
  the Features page has loaded, `TypeAsync` into the search box `#search-box` the text `OpenID` (typing is the last action,
  so it ends as the narration ends: "Let's search for OpenID").
  *Visible at the end:* the Features page with `OpenID` in the search box and the list already filtered to the
  **OpenID Connect** group. *Labels:* "Features" `OrchardCore.Features/Views/Admin/Features.cshtml:15`; search box
  `Features.cshtml:27`; the list filters as you type (`Assets/ts/features-index.ts`, `applyFilters`).
- `features-2`: `HighlightAsync` the group heading, the `h3` of the group whose text is "OpenID Connect"
  (`.feature-group h3`, `Features.cshtml:88`), then clear the highlight.
  *Visible at the end:* under the heading **OpenID Connect**, exactly five rows in this order (the list sorts by name,
  `OrchardCore.Features/Controllers/AdminController.cs:94`):
  OpenID Connect Authorization Server, OpenID Connect Client Integration, OpenID Connect Management UI, OpenID Connect
  Secrets, OpenID Connect Token Validation. There is **no** row named OpenID Connect Core Services, because the
  visibility filter hides on-demand features by default (`Features.cshtml:65`, `OrchardCore.OpenId/Manifest.cs:16`). If a Core Services
  row is visible, stop and read `OPEN-QUESTIONS.md`. Each row shows its name, its identifier in a code font, its description,
  its dependencies, the status **Disabled** and a button **Enable**.
- `features-3`: `ClearHighlightsAsync`, then `HighlightAsync` the Authorization Server row (selector
  `[data-filter-value*="OrchardCore.OpenId.Server"]`, the attribute is set in
  `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`). Do not click anything.
  *Visible at the end:* the row "OpenID Connect Authorization Server" circled, with its description ("Enables Orchard Core
  to function as an OpenID Connect authorization server/identity provider ..."). *Label:* `OrchardCore.OpenId/Manifest.cs:44-45`.
- `features-4`: clear the highlight; highlight the Client Integration row
  (`[data-filter-value*="OrchardCore.OpenId.Client"]`). *Visible at the end:* the row "OpenID Connect Client Integration"
  circled, description "Allows authentication of users through an external OpenID Connect authorization server (also known
  as an identity provider)." *Label:* `OrchardCore.OpenId/Manifest.cs:21-22`.
- `features-5`: clear the highlight; highlight the Management UI row (`[data-filter-value*="OrchardCore.OpenId.Management"]`).
  *Visible at the end:* the row "OpenID Connect Management UI" circled, description "Adds a user interface for managing
  OpenID Connect applications, scopes and permissions." *Label:* `OrchardCore.OpenId/Manifest.cs:33-34`. The dependency "enabling the
  server turns it on" is shown on the Authorization Server row in `enable-1`, not here.
- `features-6`: clear the highlight; highlight the Secrets row (`[data-filter-value*="OrchardCore.OpenId.Secrets"]`).
  *Visible at the end:* the row "OpenID Connect Secrets" circled, description "Allows storing OpenID Connect signing and
  encryption keys as secrets.", with a dependency badge "Secrets" (and "OpenID Connect Authorization Server"). The row must
  show no warning badge of a missing dependency; if it does, the Secrets module is not in this build. *Label:*
  `OrchardCore.OpenId/Manifest.cs:56-57`; the Secrets feature is `OrchardCore.Secrets/Manifest.cs:10-11`.
- `features-7`: clear the highlight; highlight the Token Validation row (`[data-filter-value*="OrchardCore.OpenId.Validation"]`).
  *Visible at the end:* the row "OpenID Connect Token Validation" circled, description "Validates tokens issued by the local
  OpenID Connect authorization server or other trusted servers supporting JWT and OpenID Connect discovery." *Label:*
  `OrchardCore.OpenId/Manifest.cs:68-69`. Then `ClearHighlightsAsync`.

## Clip `04-enable`

**Starts on:** `/Admin/Features`, opened before the capture, with the text `OpenID` already typed into `#search-box`
(so the same five rows as at the end of `features-2` are shown). The site is in snapshot A.

- `enable-1`: `HighlightAsync` the dependency badges of the Authorization Server row
  (`[data-filter-value*="OrchardCore.OpenId.Server"] .direct-dependencies`,
  `OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12`).
  *Visible at the end:* two badges ringed on the "OpenID Connect Authorization Server" row: "OpenID Connect Core Services" and
  "OpenID Connect Management UI" (sorted by name; the dependencies are `OrchardCore.OpenId/Manifest.cs:47-51`).
- `enable-2`: `ClearHighlightsAsync`, then click the **Enable** button of the **Authorization Server row only**
  (`[data-filter-value*="OrchardCore.OpenId.Server"] a:has-text("Enable")`; the page also has an **Enable** item in the
  **Bulk Actions** menu, do not use that one). Click within the first half second of the step: the notification below
  disappears five seconds after it appears (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:70-72`), and the
  narration of the step is about six seconds long. The click posts to the server, the tenant reloads with the new
  features, and the browser is sent back to `/Admin/Features` (`AdminController.cs` `Enable`, `Redirect(GetNextUrl(...))`).
  Use `ClickAndWaitForNavigationAsync`, and wait until the notification is visible
  (selector `text=The feature OpenID Connect Authorization Server was enabled.`); the reload can take several seconds, so cut
  that wait with `OffCameraAsync` (see `OPEN-QUESTIONS.md`).
  *Visible at the end:* the Features page after the reload, with the green notification "The feature OpenID Connect
  Authorization Server was enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`) still on screen. The search
  box is empty after the reload, so the list shows every category. `StepAsync` holds the live page until the narration of the
  step is over, and the time spent in `OffCameraAsync` is cut, so only the part of the narration that remains after the click
  and the lead-in (roughly four seconds) is recorded after the reload. The notification, dismissed five seconds after the page
  shows it, should therefore still be visible at the end; if the wait ended late and it is gone, take the clip again.
- `enable-3`: the search box is empty after the reload. `TypeAsync` `OpenID` into `#search-box` again, then `HighlightAsync`
  the **Enabled** status of the Authorization Server row and of the Management UI row, one after the other (the status badge is
  in the row's metadata container: `[data-filter-value*="OrchardCore.OpenId.Management"] .metadata .badge`, and the same with
  `.Server`; the dependency badges are in `.tags`, so do not use a bare `.badge`), then clear the highlights.
  *Visible at the end:* the same five rows, under the heading **OpenID Connect**, with the status **Enabled** on "OpenID
  Connect Authorization Server" and "OpenID Connect Management UI" and **Disabled** on Client Integration, Secrets and Token
  Validation; no Core Services row (it is enabled, and hidden by the default visibility filter). The **Enable** button remains
  on the three disabled rows. *Labels:* "Enabled", "Disabled" `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9,15`.

## Clip `05-tour`

**Starts on:** `/Admin/Features` (any admin page works; this one is stable), signed in, in snapshot B, before the capture.
Collapse every admin menu group before the capture, so that opening **Settings** and **Access Control** is seen.

- `tour-1`: open the admin menu: click **Settings**, then **OpenID Connect**, then `ClickAndWaitForNavigationAsync`
  **Authorization Server** (link texts as written; fallback `NavigateAsync("/Admin/OpenId/ServerConfiguration")`). Keep the three
  clicks within the narration (about eight seconds). Only the last click navigates; use `ClickAsync` for **Settings** and
  **OpenID Connect**, which expand menu groups. The admin menu holds two groups named **OpenID Connect**, one under **Settings**
  (`ServerAdminMenu.cs:18-22`) and one under **Access Control** (`ManagementAdminMenu.cs:18-22`), and both carry the id `openid`
  (`.Id("openid")`), so a locator on that id or on the text alone is ambiguous: scope it to the parent group that was just
  opened.
  *Visible at the end:* the page opened by **Authorization Server**, titled "Configure OpenID Connect server settings" in the
  breadcrumb. *Labels:*
  `OrchardCore.OpenId/ServerAdminMenu.cs:18-22`; page title `OrchardCore.OpenId/Views/ServerConfiguration/Index.cshtml:4`; route
  `OrchardCore.OpenId/Controllers/ServerConfigurationController.cs:49` (`/Admin/OpenId/ServerConfiguration`).
- `tour-2`: `HighlightAsync` the yellow warning at the top of the form (`form > p.alert-warning`, the first one; text "The website
  might be restarted upon saving the settings, potentially leading to temporary unresponsiveness during the process."; other
  `alert-warning` elements further down the form, about certificates, must not be matched). The settings editor may draw the
  same warning a second time just under it (`OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml`); highlight the first
  one, as the second changes nothing in the narration. Then
  `ClearHighlightsAsync` and `ScrollAsync(0, 250)` slowly so the heading **Endpoints** and its first checkboxes ("Enable Token
  Endpoint", "Enable Authorization Endpoint", ...) are on screen. Highlights are fixed-position: clear before scrolling.
  *Visible at the end:* the heading **Endpoints** with the "Enable ... Endpoint" checkboxes under it, all checked.
  *Labels:* `OrchardCore.OpenId/Views/ServerConfiguration/Index.cshtml:11`; `OrchardCore.OpenId/Views/OpenIdServerSettings.Edit.cshtml:17-96`.
- `tour-3`: `ScrollAsync` down until the heading **Flows** and its checkboxes are in view, then `HighlightAsync` the checkboxes
  "Allow Authorization Code Flow" and "Allow Refresh Token Flow" (label locators `label:has-text("Allow Authorization Code Flow")`,
  `label:has-text("Allow Refresh Token Flow")`). Do not click any checkbox and do not save.
  *Visible at the end:* the heading **Flows** with the six "Allow ... Flow" checkboxes, of which **only** "Allow Authorization
  Code Flow" and "Allow Refresh Token Flow" are checked, ringed. *Labels:* `OpenIdServerSettings.Edit.cshtml:105-163`; the defaults
  that make exactly these two checked come from `OrchardCore.OpenId/Services/OpenIdServerService.cs:68-83`.
- `tour-4`: `ClearHighlightsAsync`, then `ScrollAsync` down in two or three stages to show the heading **Proof Key for Code
  Exchange**, then **Pushed Authorization Requests**, then the heading **Advanced Options** with the blue notice "The following
  options are optional and intended for advanced use only." Stop scrolling as soon as that notice is in view: **the fields under
  it (Authority, certificate lists) must not be on screen** (see the private data checks). The **Authority** row follows the
  notice closely, so measure it once with `Recorder probe` (the bottom of the notice and the top of the **Authority** label in
  CSS pixels) and scroll to a position that shows the whole notice and not the label; if no such position exists, stop with the
  notice at the bottom edge of the viewport. On this site the **Authority** field is empty (nothing was saved, and the editor
  fills it from the saved setting, `OpenIdServerSettingsDisplayDriver.cs`, `model.Authority = settings.Authority?.AbsoluteUri`),
  and the certificate lists are hidden by the page script until a store location is chosen
  (`Assets/ts/openid-server-settings.ts`, `refreshEncryptionCertificates`), so the risk is small; the rule stays as a precaution.
  *Visible at the end:* the headings
  **Pushed Authorization Requests** and **Advanced Options** and the notice, with the bottom of the viewport above the Authority
  field. *Labels:* `OpenIdServerSettings.Edit.cshtml:173,193,213,220`.
- `tour-5`: click **Access Control** in the admin menu, then **OpenID Connect** inside the **Access Control** group (not the one
  under **Settings**, which may still be open from `tour-1`; collapse **Settings** first), then
  `ClickAndWaitForNavigationAsync` **Applications** (fallback `NavigateAsync("/Admin/OpenId/Application")`).
  *Visible at the end:* the Applications page: breadcrumb "Applications", the search box, the button "New application", and the
  empty message "Nothing here! There are no applications at the moment." *Labels:* `OrchardCore.OpenId/ManagementAdminMenu.cs:18-22`;
  `OrchardCore.OpenId/Views/Application/Index.cshtml:7,26,36`; route `OrchardCore.OpenId/Controllers/ApplicationController.cs:63`.
- `tour-6`: `ClickAndWaitForNavigationAsync` **Scopes** under the same **OpenID Connect** group of **Access Control** (fallback
  `NavigateAsync("/Admin/OpenId/Scope")`).
  *Visible at the end:* the Scopes page: breadcrumb "Scopes", the button "New scope", and four rows, sorted by display name
  (`OrchardCore.OpenId/Controllers/ScopeController.cs:103`): Email, Phone, Profile and Roles, each with a badge holding its name
  (email, phone, profile, roles) and a description line.
  If the list is empty, reload the page once (the default scopes are created by a deferred task of the first start with the
  feature, `OrchardCore.OpenId/Migrations/DefaultScopesMigration.cs:14-64`). *Labels:* `ManagementAdminMenu.cs:27`;
  `OrchardCore.OpenId/Views/Scope/Index.cshtml:7,26`; the display names `DefaultScopesMigration.cs:22,34,45,57`; route
  `ScopeController.cs:51`.

## Slides (drawn by `slides.py`, nothing to record)

- `why-1`: three cards, "Accounts everywhere", "Repeated sign-ins", "Access in many places", all bright.
- `why-2`: three cards, "One place to sign in", "Applications ask", "Tokens come back".
- `why-3`: three cards with check marks, "Your Orchard Core site", "Your user accounts", "Standard protocols".
- `flow-1`: four numbered cards; the first, "The application asks", is bright.
- `flow-2`: the same four cards; the second, "The person signs in", is bright.
- `flow-3`: the same four cards; the third, "The application gets tokens", is bright.
- `flow-4`: the same four cards; the fourth, "The API checks the token", is bright.
- `flow-5`: three cards, "Identity provider", "API", "Client", each naming the matching feature.
- `map-1`: four cards that give the menu path of each feature: Authorization Server, Token Validation, Authentication Client,
  and Applications and Scopes (the menu paths are those of `OrchardCore.OpenId/ServerAdminMenu.cs:18-22`, `ValidationAdminMenu.cs:18-20`,
  `ClientAdminMenu.cs:26-28` and `ManagementAdminMenu.cs:18-27`).

## Checks for private data

- The only account on the site is `admin` / `admin@example.com`; the site is named "Orchard Core". No real name, email,
  phone number, token or password may appear in any frame. Never type or show the real password; the placeholder in this
  plan is `<password>`.
- **Certificates.** The server settings page lists the certificates found in the recording machine's certificate stores
  (`OrchardCore.OpenId/Services/OpenIdServerService.cs`, `GetAvailableCertificatesAsync`), with their friendly names and subjects, which
  can hold real machine or company names. The clip never opens the certificate lists and never scrolls to them (`tour-4` stops
  at the notice under **Advanced Options**). After recording, OCR every frame of `05-tour` for the host name, the user name and
  any certificate subject of the machine.
- The URL bar is not recorded. The `Authority` field must stay off screen (it can hold the host name of the machine).
- The admin menu and the top bar show only `admin`. If the Orchard Core version shows the machine name anywhere, mask it.
- Scan the contact sheets (`review.py`) of the three clips for a red error page, a yellow "A certificate must be added to
  your server" message, or an empty Scopes list.

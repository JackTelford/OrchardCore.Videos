# Recording plan: Features, enable and disable what your site uses

This plan lets a person record the video without deciding anything. The audience is `admin`: the clips show the
admin UI only. Selectors in this file stand for `scene.Page.Locator(...)`. File references (`file:line`) are paths in
the Orchard Core checkout the content was read from (commit `92db4a83`), starting at its repository root.

| Chapter | Clip | Kind | Steps |
|---|---|---|---|
| 00 | `00-intro` | card | `intro` |
| 01 | `01-concept` | slides | `what-1` to `what-4` |
| 02 | `02-page` | recording | `page-1` to `page-4` |
| 03 | `03-filter` | recording | `find-1` to `find-5` |
| 04 | `04-enable` | recording | `enable-1` to `enable-3` |
| 05 | `05-disable` | recording | `disable-1` to `disable-3` |
| 06 | `06-bulk` | recording | `bulk-1` to `bulk-3` |
| 07 | `07-permission` | slide | `who-1` |
| 99 | `99-outro` | card | `outro` |

## 1. The site it needs

- **Application**: `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore`, on `http://localhost:5010`, with its own empty
  `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). Start it before anything else and stop it by
  its process ID.
- **Provisioning**: Auto Setup through environment variables, so no setup form is recorded (this video is not about the
  Setup screen). Set `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
  `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
  `...__AdminPassword=<password>` (the value of `VIDEO_PASSWORD`, chosen outside git), `...__DatabaseProvider=Sqlite`
  and `...__RecipeName=Blog`.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address. No clip opens a page of the public site.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` writes `state.json`
  (git-ignored). Create it from a clean browser context, so no admin-list layout cookie exists: every list then uses the
  default List layout with the Buttons actions layout, which is what the narration describes.
- **Admin settings**: leave Settings, Admin at its defaults (list layout, actions layout, menu behavior, "Let users choose
  the layout of a list"). Without a stored choice the lists use the List layout and the Buttons actions layout
  (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`; the `Admin` `List` section of
  `src/OrchardCore.Cms.Web/appsettings.json:19-25` is commented out), which is what the narration describes. "Let users
  choose the layout of a list" is on by default (`src/OrchardCore/OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:49`),
  so the Features page shows the List and Grid buttons beside the Status filter
  (`src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListLayoutSelector.cshtml:11`,
  `src/OrchardCore.Modules/OrchardCore.Admin/AdminListShapeTableProvider.cs:80`). No step uses them: never click them,
  because the Grid layout has no `li` rows and the locators below would find nothing.
- **Features to enable before recording**: none. The Blog recipe's own list is the starting point
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-80`). Before every take, check on the Features page
  that these four are Disabled: Background Tasks, Google Analytics, Google Tag Manager and Google Authentication.
- **Settings to save, external services, seed data**: none. The Features page lists features, not content, so there is no
  fake data to create. The only personal-looking value on screen is the signed-in user name `admin` in the top bar. The
  Google features are enabled in `06-bulk` without a tracking ID, and no clip opens a public page.
- **Window**: the recorder's standard 1536x864 viewport. The admin menu stays expanded (do not collapse it).

## 2. Order of the clips, and what each one changes

| Clip | Reads or changes the site | Needs |
|---|---|---|
| `02-page` | reads only | snapshot A |
| `03-filter` | reads only (the filters live in the page) | snapshot A |
| `04-enable` | **enables Background Tasks** | snapshot A |
| `05-disable` | **disables Background Tasks** | snapshot B: snapshot A with Background Tasks enabled |
| `06-bulk` | **enables Google Analytics and Google Tag Manager** | snapshot A (Background Tasks may be either) |

- Record in this order: `02-page`, `03-filter`, `04-enable`, `05-disable`, `06-bulk`. `05-disable` starts from the state
  `04-enable` leaves; the others do not depend on each other.
- **Snapshot A**: after Auto Setup and `Recorder setup`, stop the site and copy its `App_Data` folder and `state.json`
  aside. Restore it, and start the site again, before every full take and before re-recording `04-enable` or `06-bulk`.
  Never restore it for `05-disable` alone: in A, Background Tasks is Disabled and its row has an Enable button, not a
  Disable button.
- **Snapshot B**: restore A, start the site, enable Background Tasks outside the capture (Tools, Features, search for
  it, Enable), stop the site and copy `App_Data` aside. Restore B (not A) before re-recording `05-disable` alone.

## 3. How every recording clip works

- Each clip method starts with `scene.OpenAsync(...)` (not recorded), then `scene.StartCaptureAsync()`.
- The seconds in each entry are estimates from the step's words (0.4 s a word). `durations.json` has the exact length
  after `narrate.py`; keep the actions of a step shorter than its narration, and where an entry says "wait", use
  `PauseAsync` so the action lands on the words it illustrates.
- Navigate only with `ClickAndWaitForNavigationAsync`. Highlight rings are fixed on the page: call
  `ClearHighlightsAsync()` before scrolling or navigating.
- Bring an element to the middle of the window with
  `await locator.EvaluateAsync("e => e.scrollIntoView({block: 'center'})")` (an instant jump; the search bar is sticky).
- The success message is a toast at the top right of the page. It hides itself 5 seconds after the page appears
  (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`, `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml:15`).
  The end state of a step never depends on it; when an entry mentions it, it is a bonus on the contact sheet.
- Named locators used below (replace `<id>` by the feature id, with dots written as dots in `code` and `value`, and as
  underscores in element ids):

| Name | Locator | Source |
|---|---|---|
| search box | `#search-box` | `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27` |
| summary | `#list-summary` | `Features.cshtml:57-59` |
| Status button | `button[data-id="status-filter"]` | `Features.cshtml:68` (the rendered bootstrap-select button) |
| Visibility button | `button[data-id="visibility-filter"]` | `Features.cshtml:64` |
| filter menu item | `.bootstrap-select:has(button[data-id="<select id>"]) span.text:text-is("<option text>")`, with `<select id>` `status-filter` or `visibility-filter` | `Features.cshtml:65-66, 69-71` |
| row | `li[data-filter-value]:has(code:text-is("<id>"))` | `Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11` |
| Enable button | `#btn-enable-<id with _>` | `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` |
| Disable button | `#btn-disable-<id with _>` | `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53` |
| row checkbox | `input[name="featureIds"][value="<id>"]` | `Views/FeatureEntry.Checkbox.SummaryAdmin.cshtml` |
| toast | `.toast.message-success` | `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml:15` |
| dialog buttons | `#modalOkButton` (Yes), `#modalCancelButton` (No) | `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:30-31` |
| Tools group | `#left-nav button.nav-group-label` with the text Tools | `src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml` |
| menu entry | `#left-nav a.item-label:has(span.title:text-is("<label>"))` | same view |

The filters run only on the `changed.bs.select` event, so the Status and Visibility lists must be opened by clicking the
rendered button and an item of its menu. The menu of a closed list is already in the page (its `ul` always carries the
classes `dropdown-menu inner show`), so always scope an item by its wrapper, as in the table, and click the button first. `SelectAsync` on the hidden native select changes its value and filters
nothing. If a locator above finds nothing, run `MSYS_NO_PATHCONV=1 Recorder probe /Admin/Features "() => document.querySelector('#status-filter').parentElement.outerHTML"`
and adjust the locator to what the page renders; keep the actions as written.

## 4. Slides and cards

Drawn by `slides.py` from the helpers of `brand.py`; nothing is recorded.

- `intro`: the opening card, "Orchard Core Features" with the subtitle "Enable and disable what your site uses".
- `what-1`: four cards: Modules, Features, Enabled features, Setup recipe. The first two are bright, the others dimmed.
- `what-2`: the same four cards. Enabled features and Setup recipe are bright, the first two dimmed.
- `what-3`: four cards: Admin menu entries, Settings pages, Permissions, Disabled features.
- `what-4`: two cards: Enable a feature, Disable a feature.
- `who-1`: three cards: Features page, Administrator, Other roles (the Manage Features permission).
- `outro`: the closing card, subtitle "docs.orchardcore.net".

## 5. `02-page`: the Features page

Start page: `scene.OpenAsync("/Admin")`, the admin start page ("Welcome to Orchard Core",
`src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminDashboard.cshtml:2`). Every group of the left menu is collapsed.

- `page-1` (about 6 s). `PauseAsync(1800)` while the words say "Let's open the Features page", then click the Tools group
  of the left menu (`pauseAfter: 600`; the group opens). Then `ClickAndWaitForNavigationAsync` on the menu entry Features,
  so the click lands on the words "choose Features" (about 4.4 s). *Visible at the end*: the Features page, with the search
  box (placeholder "Search"), the Bulk Actions button, the Visibility and Status buttons, the List and Grid buttons, and the Tools group open in the
  left menu with Features marked as the current page. *Labels*: Tools and Features,
  `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; page title, `Views/Admin/Features.cshtml:5,15`.
- `page-2` (about 9 s). At the start highlight the heading of the first group on the page (`.feature-group:not(.d-none) h3`, `.First`, a
  category name). Wait about 4.5 s, `ClearHighlightsAsync()`, then highlight the summary. *Visible
  at the end*: the summary "Showing X of N features" ringed, with the first group underneath. X is lower than N from the
  start: the two Visibility options are selected, so the on-demand and the always enabled features are hidden when the
  page loads (`Assets/ts/features-index.ts:218`; counts at `:123-133`).
  *Labels*: the summary, `Features.cshtml:57-59`; the group heading, `Features.cshtml:88`; the grouping by category,
  `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:91`.
- `page-3` (about 12 s). Take the first visible row that shows a dependency badge,
  `li[data-filter-value]:not(.d-none):has(.direct-dependencies .badge)` (`.First`), and bring it to the middle of the window if it is not visible. Highlight, in the order of the words and
  clearing each ring before the next: its name (`span.fw-semibold`, from the start to about 3 s), its identifier (`code`,
  to about 5.2 s), its dependency badges (`.direct-dependencies`, to about 8 s), its status badge (`.metadata .ta-badge`,
  Enabled or Disabled, to about 10.8 s) and its description (`.admin-list-secondary`, to the end). *Visible at the end*:
  the description of that row ringed, with its name, badges and status badge on the line above it.
  *Labels*: `Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`, `Views/FeatureEntry.Description.SummaryAdmin.cshtml:8`,
  `Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:16`, `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9,15`.
- `page-4` (about 12 s). Take the first Enable button of a row that is off, `li[data-is-enabled="false"]:not(.d-none) a.btn-primary`
  (`.First`; a row that is off can have no button, so the button is part of the locator), bring it to the middle of the
  window and highlight it at the start, and keep the ring for about 8.5 s (the words reach "Disable turns off a feature
  that is on" at about 8.8 s); clear the ring. Take the first Disable button of a row that is on,
  `li[data-is-enabled="true"]:not(.d-none) a.btn-danger` (`.First`), bring it to the middle of the window and highlight it. *Visible at the end*: an Enabled row with its Disable button ringed. *Labels*:
  `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40,53`.

## 6. `03-filter`: search and filter

Start page: `scene.OpenAsync("/Admin/Features")`, scrolled to the top, search box empty, Status on "Any status", Visibility
with both options selected.

- `find-1` (about 8 s). Highlight the search box (`#search-box`) from the start; at about 5.6 s, where the words reach
  "Let's search for background tasks", `ClearHighlightsAsync()` and `TypeAsync` in the search box: `background tasks`
  (`delay: 90`, about 1.5 s). *Visible at the end*: only the
  group Tasks is left, with the row Background Tasks; the summary reads "Showing 1 of N features". *Labels*: placeholder,
  `Features.cshtml:27`; the search matches name, identifier and description,
  `src/OrchardCore.Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20` and `Assets/ts/features-index.ts:87-135`.
- `find-2` (about 8 s). Highlight the summary, wait about 3 s, `ClearHighlightsAsync()`, then press Escape with the cursor
  still in the search box: `scene.Page.Keyboard.PressAsync("Escape")` (click the box first if it lost the focus).
  *Visible at the end*: the search box empty, the groups and rows of the start of the page back (the Visibility filter
  still hides the on-demand and always enabled features), the summary back to the "Showing X of N features" of `page-2`.
  *Labels*: `src/OrchardCore.Modules/OrchardCore.Features/Assets/ts/features-index.ts:136-142`.
- `find-3` (about 11 s). Click the Status button; the menu opens with the header "Filter by status" and the options "Any
  status" (ticked), "Enabled only" and "Disabled only". Wait about 4.5 s, then click "Enabled only" (the menu closes).
  *Visible at the end*: the list shows only features that are Enabled, the summary count is lower than the total, and the
  groups without an enabled feature are gone. *Labels*: `Features.cshtml:68-71`.
- `find-4` (about 15 s). Leave the Status filter on "Enabled only", as `find-3` left it (Features is Enabled, so its row
  still shows once the Visibility filter lets it). Click the Visibility button: the menu opens with the header "Filter by visibility" and
  the options "Hide on demand" and "Hide always enabled", both ticked. At about 4 s highlight "Hide on demand", at about 9.5 s
  clear that ring and highlight "Hide always enabled". *Visible at the end*: the Visibility menu open over the list of enabled features, both options
  ticked, "Hide always enabled" ringed. *Labels*: `Features.cshtml:64-66`.
- `find-5` (about 13 s). At about 1 s click "Hide always enabled" in the open menu (its tick disappears and rows appear),
  then click the summary to close the menu. At about 4 s bring the row of Features (`code` text `OrchardCore.Features`, in
  the group Infrastructure) to the middle of the window and highlight its dimmed Disable button, the wrapper
  `span.d-inline-block[data-bs-toggle="tooltip"]` of that row. At about 7 s `MoveToAsync` that wrapper and keep the
  cursor on it. *Visible at the end*: the Features row with its dimmed Disable button and the tooltip "This feature is
  always enabled and cannot be manually disabled." next to it (wait for `.tooltip.show`). *Labels*:
  `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:32-33`; why the row is hidden by default,
  `src/OrchardCore.Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:21-22` and `Assets/ts/features-index.ts:87-135`.

## 7. `04-enable`: enable a feature

Start page: `scene.OpenAsync("/Admin/Features")`, from snapshot A (Background Tasks Disabled).

- `enable-1` (about 7 s). `TypeAsync` in the search box: `background tasks` (`delay: 80`). At about 3.5 s highlight the
  status badge of the row of Background Tasks (`.metadata .ta-badge`), at about 5.5 s clear it and highlight its Enable
  button (`#btn-enable-OrchardCore_BackgroundTasks`). *Visible at the end*: the group Tasks with the row Background Tasks
  (description "This module provides tools to manage background tasks."), the badge Disabled, and the Enable button
  ringed. *Labels*: name, description and category, `src/OrchardCore.Modules/OrchardCore.BackgroundTasks/Manifest.cs:4,8,9`;
  badge and button, `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15` and `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.
- `enable-2` (about 6 s). `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync` on
  `#btn-enable-OrchardCore_BackgroundTasks` right at the start of the step (the site reloads its configuration; allow up
  to about 10 s). Then wait for the toast. *Visible at the end*: the Features page, reloaded, with an empty search box and
  the list as at the start of the page (the Visibility filter is back to its default); the toast "The feature Background Tasks was enabled." is visible in the first 5 s after the reload.
  *Labels*: the message, `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`. Clicking Enable
  asks for no confirmation (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` carries no dialog attributes).
- `enable-3` (about 9 s). The Tools group of the left menu is open on this page because it holds the current page
  (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts:149-163`); if it is not, click Tools. Wait until the menu
  entry Background Tasks is visible, highlight it for about 4.5 s, `ClearHighlightsAsync()`, then `TypeAsync` in the search
  box: `background tasks`, and highlight the Disable button (`#btn-disable-OrchardCore_BackgroundTasks`). *Visible at the
  end*: the row of Background Tasks with the badge Enabled and its Disable button ringed; the left menu lists Background
  Tasks under Tools. *Labels*: `src/OrchardCore.Modules/OrchardCore.BackgroundTasks/AdminMenu.cs:18-19`;
  `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`.

## 8. `05-disable`: disable a feature

Start page: `scene.OpenAsync("/Admin/Features")`, from snapshot B (Background Tasks Enabled).

- `disable-1` (about 5 s). `PauseAsync(1400)` while the words say "Let's disable Background Tasks", then `TypeAsync` in the
  search box: `background tasks` (`delay: 80`), then, at about 3.4 s (the words "click Disable"),
  `ClickAsync` on `#btn-disable-OrchardCore_BackgroundTasks` (`pauseAfter: 400`). The link opens a dialog instead of
  navigating. *Visible at the end*: the dialog with the title "Disable Feature", the text "Are you sure you want to disable
  the Background Tasks feature? Continue?" and the buttons Yes and No. *Labels*:
  `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:45,53`; the dialog,
  `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-31,171-172`.
- `disable-2` (about 8 s). Highlight the dialog title (`#confirmRemoveModal .modal-title`), wait about 3.4 s, clear it,
  highlight `#modalOkButton` (Yes), wait about 1.8 s, clear it, highlight `#modalCancelButton` (No). *Visible at the end*:
  the dialog still open, with No ringed. *Labels*: `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53`
  (`data-title`, `data-ok-text`, `data-cancel-text`).
- `disable-3` (about 10 s). `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync` on `#modalOkButton` right at
  the start of the step (allow up to about 10 s for the site to reload its configuration). Then highlight the entries of
  the Tools group in the left menu (`#left-nav li.nav-group:has(button.nav-group-label:has-text("Tools")) ul.nav-group-items`). *Visible at the end*: the Features page, reloaded, with
  the Tools group open and no Background Tasks entry in it; the toast "The feature Background Tasks was disabled." is visible
  in the first 5 s after the reload. *Labels*: `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:321`.

## 9. `06-bulk`: several features at once

Start page: `scene.OpenAsync("/Admin/Features")`, from snapshot A (or after `05-disable`).

- `bulk-1` (about 9 s). From the start highlight the checkbox of the first row that has one,
  `li[data-filter-value]:not(.d-none) input[name="featureIds"]` (`.First`), for about 4.4 s (the words "use the checkbox on
  each row"); `ClearHighlightsAsync()`. Then `TypeAsync` in the search box: `google` (`delay: 90`), so the typing lands on the
  words "Let's search for google" (about 4.8 s). Wait until the group Google lists its rows: Google Analytics, Google
  Authentication and Google Tag Manager. At about 7 s `ClickAsync` the checkbox of Google Analytics
  (`input[name="featureIds"][value="OrchardCore.Google.Analytics"]`, `pauseAfter: 400`), and at about 8.4 s the one of
  Google Tag Manager (`value="OrchardCore.Google.TagManager"`). Do not check Google Authentication and do not use the
  select-all box of the group. *Visible at the end*: three rows, two checked and Google Authentication not; the select-all
  box of the group shows the partly selected state. *Labels*: `src/OrchardCore.Modules/OrchardCore.Google/Manifest.cs:10-33`;
  `Views/FeatureEntry.Checkbox.SummaryAdmin.cshtml`.
- `bulk-2` (about 9 s). At about 0.3 s `ClickAsync` `#bulk-action-menu-button` (Bulk Actions): the menu shows Disable and
  Enable. At about 1.5 s highlight the item Disable (`a[data-action="Disable"]`), at about 3.3 s clear it and highlight
  Enable (`a[data-action="Enable"]`), at about 6 s clear the ring, then `ClickAndWaitForNavigationAsync` on Enable (no
  dialog appears). *Visible at the end*: the Features page, reloaded, with an empty search box; the toast "The following
  features were enabled: ..." naming Google Analytics and Google Tag Manager (in the order the site lists them) is on the
  screen in the first 5 s after the reload. *Labels*: `Features.cshtml:34,37,38`; the message,
  `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`; no confirmation for a bulk action,
  `src/OrchardCore.Modules/OrchardCore.Features/Assets/ts/features-index.ts:156-168`.
- `bulk-3` (about 10 s). Let the toast stay for about 3 s, then `TypeAsync` in the search box: `google`. Highlight, one after
  the other for about 2 s each, the badge of Google Analytics, the badge of Google Tag Manager and the badge of Google
  Authentication (`.metadata .ta-badge` in each row). *Visible at the end*: Google Analytics and Google Tag Manager Enabled,
  Google Authentication Disabled, the badge of Google Authentication ringed. *Labels*:
  `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9,15`.

## 10. `07-permission`: the slide

- `who-1`: the slide "The Manage Features permission" with three cards. The names come from
  `src/OrchardCore/OrchardCore.Features.Core/FeaturesPermissions.cs:8` (Manage Features),
  `src/OrchardCore.Modules/OrchardCore.Features/Permissions.cs:15-22` (Administrator) and
  `src/OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19` (Access Control, Roles).

## 11. Checks for private data

- OCR the result at one frame per second and at every scene change. The text must hold only: `admin` (the user), the
  site name "Orchard Core", feature names, identifiers and descriptions, and the fake values above.
- No real name, email address, host name, path of this machine, tracking ID or token. The Blog recipe's sample content is
  not shown in any clip. The Google features are enabled without a tracking ID.
- `state.json` and the `App_Data` snapshots stay outside git.

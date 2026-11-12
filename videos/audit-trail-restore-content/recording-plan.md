# Recording plan: Audit Trail: Compare and Restore Content Versions

Everything a person needs to record this video without deciding anything. The slug is `audit-trail-restore-content`.
Source paths are relative to an Orchard Core checkout at commit `92db4a83` and are written `path:line`.
A selector written below stands for `scene.Page.Locator(...)`. Selectors were derived from the Razor views and scripts,
not from a running page: confirm each one with `Recorder probe` before the first take, and fix the plan if one differs.

## The site it needs

- **Orchard Core site**: `src/OrchardCore.Cms.Web` run on `http://localhost:5010` with its own `ORCHARD_APP_DATA` outside both
  repositories, provisioned by Auto Setup (see the `orchardcore-tester` skill of the Orchard Core repository). No setup or
  sign-in form is recorded.
- **Recipe**: Blog. **Site name**: Orchard Core. **Time zone**: `America/Los_Angeles`. **Tenant**: Default only.
- **Admin user**: user name `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in
  `VIDEO_PASSWORD`). The recorder signs in once with `Recorder setup`.
- **Features to enable before recording** (Tools, Features, outside the capture): **Audit Trail** (`OrchardCore.AuditTrail`).
  Nothing else is needed. The content part of the Audit Trail (`src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Startup.cs:20`)
  switches on by itself when Contents and Audit Trail are both enabled. Do **not** enable Users Audit Trail: the list must
  hold content events only.
- **Settings**: leave **Allow client IP address** unchecked (the default), so no IP address appears on any line. Leave the trimming
  settings as they are (retention period of ten days, trimming enabled): the seed is created minutes before the capture.
  The daily trimming task (`src/OrchardCore.Modules/OrchardCore.AuditTrail/Services/AuditTrailBackgroundTask.cs:13`, midnight) deletes events
  older than the retention period, so restore S1 only within ten days of creating it. If S1 is older, create the seed and S1 again, or tick
  **Disable trimming task** (Settings, Audit Trail, Trimming tab; `src/OrchardCore.Modules/OrchardCore.AuditTrail/Views/AuditTrailTrimmingSettings.Edit.cshtml:8`)
  outside the capture before taking S1.
- **List layout**: the default (List). Do not change the List layout in the Admin settings, and do not use the layout selector.
- **External services and stand-ins**: none are called by the site. The browser needs internet access for the Diff tab: it loads
  Prism from `cdnjs.cloudflare.com` (`src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventDiff.DetailAdmin.cshtml:19-20`)
  and the stylesheet of the Audit Trail (`src/OrchardCore.Modules/OrchardCore.AuditTrail/Assets/scss/styles.scss:128`, loaded on its list and
  Details pages) imports the Poppins web font from Google Fonts. If the proxy variables that block the site's own outbound calls are
  set, they apply to the site process only; the recorder's browser must still reach those hosts.
- **Browser**: the recorder's standard Chromium (1536x864 CSS pixels, light color scheme, `en-US`).

## Seed data (outside the capture)

The events are created by the site itself, so the seed is made by editing content in the admin UI, as `admin`, **after** clip
`02-record` has been recorded (until a content type is allowed, nothing is logged). Wait about a minute between the numbered
saves, so the lines show different relative times. Create the items in this order, so the article of the story is the most
recently modified row of Content Items:

1. **Summer Parking Notice** (used by `07-deleted`). Content, Content Items, **New**, **Article**. Title `Summer Parking Notice`,
   Subtitle `Lot B closes on Friday`. Leave every other field empty. Click **Publish**.
2. **Harvest Festival Schedule** (the story of `03` to `06`). Content, Content Items, **New**, **Article**. Title
   `Harvest Festival Schedule`, Subtitle `Saturday, 10 AM to 4 PM`. Leave every other field empty. Click **Publish**.
   This records Created and Published for the first version.
3. Content, Content Items, **Edit** on Harvest Festival Schedule. Change only the Subtitle to `Saturday, 9 AM to 5 PM`. Click **Publish**.
   This records Published for the second version.
4. **Edit** again. Change the Title to `Harvest Festival Cancelled` and the Subtitle to `Postponed to a later date`. Click **Publish**.
   This records Published for the third version. The article is called Harvest Festival Cancelled from here on, as the narration says.

Do not type into the body editor, and do not add images. Do not use an `AuditTrailPart` comment. No real names appear:
the only user is `admin`.

After step 4, open Content, Content Items, **Actions** on Harvest Festival Cancelled, **Audit Trail**, and check that the list holds
four lines for this article, newest first: Published, Published, Published, Created, each with the text "Version N of the
Article ... was ..." and a **View** and **Details** button. The two older Published lines and the Created line carry **Restore**
or not as follows: **Restore** is on the two older Published lines, and not on the newest Published line (its version is the
latest one) nor on the Created line. If the lines differ (more or fewer events, or no **Restore** where the plan says so), do not record: write what the list shows in
`OPEN-QUESTIONS.md` and ask for the plan to be corrected. Write down the version numbers the lines show, and the content item id
from the address (`/Admin/AuditTrail/<content item id>`): the clips below open that address before the capture starts. Keep the id
outside git.

## Order of the clips, and the snapshots

1. Start from a fresh site with the Audit Trail enabled and no content type allowed. **Snapshot S0** (copy `App_Data` and `state.json`).
2. Record `02-record`. It changes the site (it allows Article).
3. Create the seed above.
4. **Snapshot S1** (copy `App_Data` aside again). Every later clip starts from S1.
5. Record `03-history`, `04-view` and `05-compare`. They change nothing and can be taken in any order, any number of times.
6. Record `06-restore`. It changes the site (it adds a draft, then publishes it, and records Restored and Published events).
   Before a new take, stop the site, restore S1 and start it again.
7. Record `07-deleted`. It changes the site (it deletes, restores and publishes Summer Parking Notice). It does not depend on `06-restore`,
   but a new take needs S1 back. After `06-restore` the older Published line of the article may show or hide **Restore** differently
   from S1, which does not matter for `07-deleted`.

`02-record` is the only clip that starts from S0. A new take of `02-record` needs S0, and then the seed again, then S1 again.

## Clips

### `02-record` (chapter 02, Choose what to record)

Starts on `/Admin`, the dashboard, signed in. Changes the site. The narration has two steps.

- `record-1`: click the **Settings** entry of the admin menu, then **Audit Trail** under it (expand the Settings group if it is
  collapsed). Wait for the page: the heading **Audit Trail** and tabs. Click the tab link **Content** (selector
  `ul.nav-tabs a.nav-link:text-is("Content")`). Highlight the label **Allowed content types**.
  *On screen when the narration ends*: the Content tab selected, the label **Allowed content types**, the hint "The types of content
  whose events are recorded.", and the list of content types as check boxes, **Select all** first, with none selected.
  *Labels come from*: menu `src/OrchardCore.Modules/OrchardCore.AuditTrail/Navigation/AuditTrailAdminMenu.cs:42-43` (Settings, Audit Trail);
  tab name `src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Drivers/ContentAuditTrailSettingsDisplayDriver.cs:41`;
  label and hint `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/ContentAuditTrailSettings.Edit.cshtml:6,10`;
  check boxes `src/OrchardCore.Modules/OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml:10`.
  Fallback address if the menu cannot be clicked: `NavigateAsync("/Admin/Settings/AuditTrail")` (route `Settings/{groupId}`,
  `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:50`).
- `record-2`: click the label **Article** (selector `.content-types label.form-check-label:text-is("Article")`) so its check box is
  checked, then click **Save** (`button.save`), with `ClickAndWaitForNavigationAsync`. The page reloads on its first tab: click the
  tab **Content** again, so the checked **Article** is visible.
  *On screen when the narration ends*: the notification "Site settings updated successfully." if it is still shown, and the Content
  tab with **Article** checked.
  *Labels come from*: **Save** `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; message
  `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`; content type name "Article" is the display
  name in the Blog recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, content type `Article`).

### `03-history` (chapter 03, Find the history of an item)

Starts on `/Admin/Contents/ContentItems`, with the row of **Harvest Festival Cancelled** first in the list. Changes nothing.

- `history-1`: move to the row of Harvest Festival Cancelled and click its **Actions** button (`li.list-group-item` that has the
  text, then `button.dropdown-toggle.actions`). Highlight the entry **Audit Trail** in the dropdown, then click it with
  `ClickAndWaitForNavigationAsync`.
  *On screen when the narration ends*: the Audit Trail page of this article has started loading, or the dropdown is open with
  **Audit Trail** highlighted. (The page can finish loading in the next step.)
  *Labels come from*: menu path `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65,68` (Content, Content Items); button
  `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml:32`; entry
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentsAction_SummaryAdmin.cshtml:3`, placed in the
  actions menu by `.../Contents/AuditTrail/Drivers/AuditTrailContentsDriver.cs:28-31`.
- `history-2`: no click. Highlight the first line, then slide the highlight down the other lines, with `HighlightAsync`
  on each badge group (event, category, time, user name) of the first line.
  *On screen when the narration ends*: the Audit Trail page, the four lines of the article (Published, Published, Published, Created),
  the newest first, each with an event badge, the **Content** category badge, a relative time, the user name `admin`, and a text such
  as "Version N of the Article Harvest Festival Cancelled was published" (the lines of the older versions carry the title that version had,
  Harvest Festival Schedule); the **View**, **Restore** and **Details** buttons on the lines.
  The **Category** drop-down is absent, because the list of one item hides it
  (`src/OrchardCore.Modules/OrchardCore.AuditTrail/Views/AuditTrailAdminListFilters.cshtml:7-10`); the filter row shows **Date**, **Events** and **Sort**
  (`.../Views/AuditTrailAdminListFilters.cshtml:11-15`, filled for a single item by `.../Services/DefaultAuditTrailAdminListQueryService.cs:77-89`).
  *Labels come from*: `src/OrchardCore.Modules/OrchardCore.AuditTrail/Views/AuditTrailEvent.SummaryAdmin.cshtml`;
  `.../Views/AuditTrailEventName.SummaryAdmin.cshtml:6`; `.../Views/AuditTrailEventCategory.SummaryAdmin.cshtml:6`;
  `.../Views/AuditTrailEventMeta.SummaryAdmin.cshtml`; text
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventData.SummaryAdmin.cshtml:41`; newest first is the
  default sort, `src/OrchardCore.Modules/OrchardCore.AuditTrail/Startup.cs:76-79`; one item's list from
  `src/OrchardCore.Modules/OrchardCore.AuditTrail/Controllers/AdminController.cs:54-78`.

### `04-view` (chapter 04, View an earlier version)

Starts on `/Admin/AuditTrail/<content item id>`, the list of Harvest Festival Cancelled. Changes nothing.

- `view-1`: highlight the second line (the Published line of the second version), then click its **View** button with
  `ClickAndWaitForNavigationAsync`. Pick the line whose text names the second version; its article title is Harvest Festival Schedule.
  *On screen when the narration ends*: the line highlighted, or the new page loading.
  *Labels come from*: `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventActions.SummaryAdmin.cshtml:8`.
- `view-2`: no click. Wait until the editor is shown. Move the cursor over the Title and Subtitle fields. Do not click in a field:
  the fields are disabled (`src/OrchardCore.Modules/OrchardCore.Contents/Assets/js/audittrail-disabledcontent.js:2-5`).
  *On screen when the narration ends*: the page title (breadcrumb) in the form "Article Harvest Festival Schedule - Version N",
  with the breadcrumb link **Manage Content** before it; the editor with Title `Harvest Festival Schedule` and Subtitle
  `Saturday, 9 AM to 5 PM`, all fields greyed.
  *Labels come from*: breadcrumb `src/OrchardCore.Modules/OrchardCore.Contents/Views/AuditTrailContent/Display.cshtml:24-25`;
  controller `.../AuditTrail/Controllers/AuditTrailContentController.cs:49-85`; field labels
  `src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12` and the Blog recipe's Article part field **Subtitle**.

### `05-compare` (chapter 05, Compare two versions)

Starts on `/Admin/AuditTrail/<content item id>`. Changes nothing. Needs internet access (Prism from `cdnjs.cloudflare.com`).

- `compare-1`: click **Details** on the first line (the newest Published event) with `ClickAndWaitForNavigationAsync`. Move the cursor
  down the two tables.
  *On screen when the narration ends*: the page titled **Audit Trail Event**, the tabs (the first one selected), the table
  **Event Info** (rows Category, Event name, Triggered by username, Triggered by user ID, Triggered at timestamp, Client IP address) and
  the table **Content Item Info** (rows Content Item Id, Content type, Display text, Version, Comment). The Client IP address cell is empty.
  *Labels come from*: `src/OrchardCore.Modules/OrchardCore.AuditTrail/Views/AuditTrailEventDetail.DetailAdmin.cshtml:3-28`;
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventDetail.DetailAdmin.cshtml:15-57`;
  `src/OrchardCore.Modules/OrchardCore.AuditTrail/Views/Admin/Display.cshtml:4`; **Details**
  `src/OrchardCore.Modules/OrchardCore.AuditTrail/Views/AuditTrailEventActions.SummaryAdmin.cshtml:3`.
- `compare-2`: click the tab link **Diff** (`ul.nav-tabs a.nav-link:text-is("Diff")`). Wait for the diff viewer (`#diffapp .diff-viewer`)
  to show lines. Clear highlights. Scroll just enough to bring the changed Title and Subtitle lines into view; do not scroll while a
  highlight is shown.
  *On screen when the narration ends*: the Diff tab with the **Split** and **Unified** buttons at the top right, two columns titled
  with the article name and a time ("Harvest Festival Schedule - ..." on the left, "Harvest Festival Cancelled - ..." on the right),
  and changed lines shaded red on the left and green on the right, unchanged lines collapsed.
  If the area is empty, Prism did not load: check the internet access of the browser and record again.
  *Labels come from*: tab `src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Drivers/AuditTrailContentEventDisplayDriver.cs:108`;
  buttons and titles `src/OrchardCore.Modules/OrchardCore.AuditTrail/Assets/js/App.tsx:100-125`; view
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventDiff.DetailAdmin.cshtml:14-26`.
- `compare-3`: click the button **Unified** (`.diff-viewer label.btn:text-is("Unified")`). Pause on the single column.
  *On screen when the narration ends*: the unified view, one column, with red and green lines, including the lines that hold the
  title and the subtitle.
  *Labels come from*: `src/OrchardCore.Modules/OrchardCore.AuditTrail/Assets/js/App.tsx:108-112`.

### `06-restore` (chapter 06, Restore an earlier version)

Starts on `/Admin/AuditTrail/<content item id>`. Changes the site (needs S1 before every take).

- `restore-1`: highlight the Published line of the second version (the same line used in `04-view`), then click its **Restore** button
  (selector: the line's `a.btn:text-is("Restore")`). The confirmation dialog opens.
  *On screen when the narration ends*: the line highlighted and the dialog `#confirmRemoveModal` open, or opening.
  The newest Published line has no **Restore** button; point the cursor at that line first for a moment.
  *Labels come from*: `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventActions.SummaryAdmin.cshtml:10,22`;
  the button is absent on the version that is the latest one, same file, line 10.
- `restore-2`: highlight the dialog title and message, then click **Yes** (`#modalOkButton`) with `ClickAndWaitForNavigationAsync`.
  *On screen when the narration ends*: the dialog titled **Restore to Version** with the text "Are you sure you want to restore to
  version N?" and the buttons **Yes** and **No**, or the page already loading after the click.
  *Labels come from*: `.../Views/Items/AuditTrailContentEventActions.SummaryAdmin.cshtml:17-20`; dialog markup
  `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:19-31`.
- `restore-3`: no click. The page is the full Audit Trail list (all events) after the redirect.
  Highlight the notification, then the first line.
  *On screen when the narration ends*: the notification "'Harvest Festival Schedule' has been restored." and the first line, a **Restored**
  event with the text "The Article Harvest Festival Schedule was restored as Version N". If the notification has already faded, take
  a note of it in `OPEN-QUESTIONS.md` and keep the first line on screen.
  *Labels come from*: message `src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Controllers/AuditTrailContentController.cs:125`;
  redirect line 127; text `.../Views/Items/AuditTrailContentEventData.SummaryAdmin.cshtml:18`; the draft is created in
  `src/OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:845-884`.
- `restore-4`: `NavigateAsync("/Admin/Contents/ContentItems")` (or click Content, Content Items). Highlight the badges **Published** and
  **Draft** on the row of the article (titled Harvest Festival Schedule after the restore), clear the highlight, click **Actions** on that row,
  highlight **Publish Draft**, click it with `ClickAndWaitForNavigationAsync`.
  *On screen when the narration ends*: the Content Items list after the redirect, the notification "That Article has been published.",
  and the row of the article with the **Published** badge only.
  *Labels come from*: badges `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:12,19`; entry
  `.../Views/ContentsButtonActions_SummaryAdmin.cshtml:19`; message
  `src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:716`.

### `07-deleted` (chapter 07, Restore a deleted item)

Starts on `/Admin/Contents/ContentItems`, with the row of **Summer Parking Notice** visible. Changes the site (needs S1 before every take).

- `deleted-1`: click **Actions** on the row of Summer Parking Notice, highlight **Delete**, click it. The dialog opens: pause on it, then
  click **Ok** (`#modalOkButton`) with `ClickAndWaitForNavigationAsync`.
  *On screen when the narration ends*: the list after the redirect, the notification "That Article has been removed.", and no row
  named Summer Parking Notice.
  *Labels come from*: `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsButtonActions_SummaryAdmin.cshtml:40` (Delete, title
  "Delete", message "Are you sure you want to delete this content item?", buttons **Ok** and **Cancel**); message
  `src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:675`.
- `deleted-2`: open Tools, Audit Trail (click **Tools**, then **Audit Trail**; fallback `NavigateAsync("/Admin/AuditTrail")`) with
  `ClickAndWaitForNavigationAsync`. Highlight the first line.
  *On screen when the narration ends*: the Audit Trail list of all events, the first line a **Removed** event with the text
  "The Article Summer Parking Notice was removed.", and its buttons **View**, **Restore** and **Details**. The filter row above the
  list shows **Date**, **Category** and **Sort**.
  *Labels come from*: menu `src/OrchardCore.Modules/OrchardCore.AuditTrail/Navigation/AuditTrailAdminMenu.cs:33-34`; text
  `.../Views/Items/AuditTrailContentEventData.SummaryAdmin.cshtml:32`; filters
  `src/OrchardCore.Modules/OrchardCore.AuditTrail/Views/AuditTrailAdminListFilters.cshtml:6-15`.
- `deleted-3`: click **Restore** on the Removed line. The dialog opens: pause on it, then click **Yes** (`#modalOkButton`) with
  `ClickAndWaitForNavigationAsync`.
  *On screen when the narration ends*: the dialog titled **Restore content item** with the text "Are you sure you want to restore this
  content item?", or the page after the redirect with the notification "'Summer Parking Notice' has been restored.".
  *Labels come from*: `.../Views/Items/AuditTrailContentEventActions.SummaryAdmin.cshtml:25-37`; message
  `.../AuditTrail/Controllers/AuditTrailContentController.cs:125`.
- `deleted-4`: `NavigateAsync("/Admin/Contents/ContentItems")`. Highlight the badge **Draft** (and no **Published** badge) on the row of
  Summer Parking Notice, clear the highlight, click **Actions** on that row, click **Publish Draft** with `ClickAndWaitForNavigationAsync`.
  *On screen when the narration ends*: the list with the notification "That Article has been published." and the row of Summer
  Parking Notice with the **Published** badge.
  *Labels come from*: `.../Views/ContentsTags_SummaryAdmin.cshtml:12,19`; `.../Views/ContentsButtonActions_SummaryAdmin.cshtml:19`;
  `.../Controllers/AdminController.cs:716`.

## Slides

The slides are drawn by `slides.py`; there is nothing to record.

- `versions-1`: three cards, "An event for each change", "A full copy of the item", "The user and the time".
- `versions-2`: four cards, "View", "Compare", "Restore", "The published page stays".
- `versions-3`: the heading "What to know before you rely on it" with four cards; the first three are bright
  ("Chosen content types", "Retention period", "Two restorable events").
- `versions-4`: the same four cards, with the fourth, "Permissions", bright.

## Checks for private data

- Every user name on screen is `admin`; no other account exists. The email of the admin is `admin@example.com` and is not shown on any
  captured page. The Details table shows the **Triggered by user ID** of admin: a random identifier of the throwaway site, which is fine.
- No IP address is shown on a line or in the Details table (Allow client IP address stays unchecked).
- The titles are the invented ones above: Summer Parking Notice, Harvest Festival Schedule, Harvest Festival Cancelled. The Blog recipe's
  own sample items hold made-up text; look at the Content Items list in each contact sheet for anything unexpected.
- The address bar is not part of the capture, so the content item id is not shown; the Details table does show a **Content Item Id**
  (a random identifier of the throwaway site). That is fine.
- No password, token or `state.json` is committed. After recording, OCR the video as `SKILL.md` describes (one frame per second and at
  every scene change) and search the text for names, emails and numbers.

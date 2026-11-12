# Recording plan: audit-trail-overview

How to record this video without deciding anything. Read it from top to bottom once, then record the clips in the
order of section 3. Every label below was read in the Orchard Core source (path and line in the "Labels" lines, paths
relative to the root of the OrchardCore checkout; `.../AuditTrail/` stands for `src/OrchardCore.Modules/OrchardCore.AuditTrail/`,
and likewise `.../Contents/`, `.../Users/`, `.../Features/`, `.../Settings/`, `.../Admin/` and `.../OrchardCore.Title/`). Where a behavior could only be read from the code and not seen on a
running site, the step says "check" and `OPEN-QUESTIONS.md` repeats it.

In this file a selector such as `#search-box` stands for `scene.Page.Locator("#search-box")`, since the recorder's
methods take locators. Selectors are hints taken from the Razor templates: confirm each one once with
`Recorder probe` before the first take.

The narration of each step lasts about its word count divided by 2.5 seconds (the budget is given per step). The
actions of a step must end before its narration does.

## 1. The site it needs

| What | Value |
|---|---|
| Orchard Core | A checkout of `OrchardCMS/OrchardCore`, `src/OrchardCore.Cms.Web`, listening on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| App_Data | An empty folder outside both repositories (`ORCHARD_APP_DATA`) |
| Provisioning | Auto Setup through environment variables, so no setup or login form is recorded (see `references/recording.md` and the `orchardcore-tester` skill). Variables: `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` |
| Password | `<password>`: chosen outside git and passed in `VIDEO_PASSWORD`; never written in this repository |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls nothing |
| Browser | The recorder's Chromium (1536x864 CSS pixels, scale 1.25, light color scheme, `en-US`) |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`, once, outside the capture (`state.json`, git-ignored) |
| Features to enable before recording | None. The Blog recipe enables everything the video needs except Audit Trail, which the video enables on camera (clip `02-enable`). Do not enable Users Audit Trail: the log must hold content events only |
| Settings | None before recording. The video itself saves the Audit Trail settings (clip `03-settings`) |
| External services | None |
| Admin settings | Leave the defaults: the admin lists show the List layout, the row actions show buttons and an Actions menu, and the New menu is not in the admin menu |

The Blog recipe creates the content types Article, Blog and Blog Post (and others, such as Page, Tag and Category), a
Blog and a Blog Post of its own, and an Article named About. They are recipe content, not real data, and may appear in
the Content Items list and, as boxes sorted by display name, in the list "Allowed content types" of `settings-3`
(`src/OrchardCore.Modules/OrchardCore.Contents/ViewModels/SelectContentTypesViewModel.cs:26`).

The Blog recipe also installs an admin menu of its own (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`,
step "AdminMenu"): two top-level entries, Blog and Main Menu, and the entries Article, Page and Taxonomy under Content,
beside Content Items. They are merged into the built-in menu (the two "Content Items" become one entry,
`src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74-150`), so the clicks "Content, Content Items" of
`change-1` and `item-1` still reach one entry each. Leave them as they are.

## 2. Seed data, created outside the capture

Everything here is fake. Create it with a normal browser window, never during a capture.

### 2.1 Before any clip (snapshot A)

Create the user Jamie Chen, who makes the earlier change that the video shows:

1. Sign in as `admin`, open Access Control, Users, click Add User.
2. User name: `jamie.chen`. Email: `jamie.chen@example.com`. Password and Password confirmation: `<password>` (the same as
   admin's, so there is no second secret). Roles: check Administrator.
3. Click Save.

Labels: Access Control, Users: `src/OrchardCore.Modules/OrchardCore.Users/AdminMenu.cs:27,30`. Add User:
`.../Users/Views/UsersAdminListCreate.cshtml:7`. Create User page: `.../Users/Views/Admin/Create.cshtml:4`. User name:
`.../Users/Views/UserName.Edit.cshtml` (label "User name"). Email: `.../Users/Views/UserEmail.Edit.cshtml` (label "Email").
Password confirmation (placeholder): `.../Users/Views/UserFields.Edit.cshtml:27`. Roles: `.../Users/Views/UserRoleFields.Edit.cshtml:6`.
Save: `.../Users/Views/UserSaveButtons.Edit.cshtml:2`.

Jamie is a second administrator on purpose: an Administrator can create and publish an Article with no other
setup. Then copy the site's App_Data folder and `state.json` aside as **snapshot A**.

### 2.2 After clip `03-settings` (snapshot B)

The Audit Trail must be enabled and Article must be allowed (clips `02-enable` and `03-settings`) before this step, or
the log stays empty. Then create the earlier change, as Jamie Chen:

1. In a private window, open `http://localhost:5010/Login` and sign in as `jamie.chen` with `<password>`.
2. Open Content, Content Items, click New, choose Article.
3. Title: `Pruning workshop`. Leave every other field empty.
4. Click Publish. The message "Your Article has been published." appears.
5. Close the private window (do not use the recorder's `state.json`; the capture stays signed in as admin).

Labels: login page `.../Users/Views/Account/Login.cshtml:27`, field "Username or email address"
`.../Users/Views/LoginFormCredentials.cshtml:7`. New: `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`.
Title: `.../OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`. Publish: `.../Contents/Views/Content.PublishButton.cshtml:7,16`.
Message: `.../Contents/Controllers/AdminController.cs:473`.

Expected: Tools, Audit Trail lists two events by `jamie.chen` for Pruning workshop, Created and Published (check:
`OPEN-QUESTIONS.md`, item 2). Copy App_Data and `state.json` aside as **snapshot B**.

## 3. Order of the clips, and what each one changes

| Order | Clip | Starts from | Changes the site? | Restore before a new take |
|---|---|---|---|---|
| 1 | `02-enable` | snapshot A | Yes: enables the Audit Trail feature | snapshot A |
| 2 | `03-settings` | after `02-enable` | Yes: saves the Audit Trail settings (Article allowed) | snapshot A, then record `02-enable` again |
| (seed) | section 2.2 | after `03-settings` | Yes: Jamie's Article | |
| 3 | `04-change` | snapshot B | Yes: a second version of the Article, a Published event | snapshot B |
| 4 | `05-list` | after `04-change` | No | none (restore snapshot C to be exact) |
| 5 | `06-details` | after `04-change` | No | none |
| 6 | `07-item` | after `04-change` | No | none |

After `04-change`, copy App_Data aside as **snapshot C**: clips `05-list`, `06-details` and `07-item` read the log it
leaves and can be recorded again from snapshot C in any order. Clips `01-why` and `08-notes` are slides and need no
site. Stop the site (by its process ID) before copying or restoring App_Data, and start it again afterwards.

Record `05-list`, `06-details` and `07-item` soon after `04-change`: the rows show how long ago each event happened,
and the video reads best when the newest row is a few minutes old, not days.

## 4. Clips

Every clip opens its first page with `scene.OpenAsync` (not recorded), starts the capture, then runs its steps.
Collapse every admin menu section before the capture, unless a step says otherwise. The admin menu opens one section
at a time, so a section that was open on the page just loaded must be left alone (see `enable-3`).

### Clip `02-enable` (chapter 02, Enable the module)

Starts on `/Admin`.

**`enable-1`** (about 9 s)
- Click Tools in the admin menu (pauseAfter 300), then click Features (`ClickAndWaitForNavigationAsync`): the page
  `/Admin/Features` opens.
- Click the search box `#search-box` and type `Audit Trail` (`TypeAsync`, delay 40). Wait until `#list-summary` reads
  "Showing N of M features" with N smaller than M (the list is filtered in the page, no reload).
- `HighlightAsync` the row of the feature named exactly "Audit Trail". The row of "Users Audit Trail" may also be
  listed under it; leave it as it is.
- On screen when the narration ends: the Features page, the search box holding "Audit Trail", the heading "Security",
  the row "Audit Trail" with its description "Provides a log for recording and viewing back-end changes.", the badge
  "Disabled" and the button "Enable", with a green ring around the row.
- Labels: Tools, Features: `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`. Search placeholder:
  `.../Features/Views/Admin/Features.cshtml:27`. Category heading: `.../Features/Views/Admin/Features.cshtml:88`.
  Feature name, description and category: `src/OrchardCore.Modules/OrchardCore.AuditTrail/Manifest.cs:13-15`. Badge:
  `.../Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`. Enable: `.../Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.

**`enable-2`** (about 7 s)
- `ClearHighlightsAsync`. Click the Enable link of the Audit Trail row: `a[href*="OrchardCore.AuditTrail"]` inside the
  filtered list (it does not match "Users Audit Trail", whose id is `OrchardCore.Users.AuditTrail`), with
  `ClickAndWaitForNavigationAsync`. Enabling a feature reloads the site's features, so the first page after the click can
  take a few seconds: if it does, wrap the wait in `OffCameraAsync`, with the condition "the text 'The feature Audit
  Trail was enabled.' is visible".
- On screen when the narration ends: the Features page with the green message "The feature Audit Trail was enabled."
  at the top.
- Labels: Enable: `.../Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`. Message:
  `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`.

**`enable-3`** (about 10 s)
- The page just loaded has the Tools section open (its active entry is Features). If the Audit Trail entry of Tools
  is not visible, click Tools first. `HighlightAsync` the entry "Audit Trail" under Tools (caption "Tools").
- `ClearHighlightsAsync`. Click Settings in the admin menu (pauseAfter 300). `HighlightAsync` the entry "Audit
  Trail" under Settings (caption "Settings").
- On screen when the narration ends: the Settings section open, with a green ring around its entry "Audit Trail".
- Labels: both entries: `src/OrchardCore.Modules/OrchardCore.AuditTrail/Navigation/AuditTrailAdminMenu.cs:33-34`
  (Tools, Audit Trail) and `:42-43` (Settings, Audit Trail).

### Clip `03-settings` (chapter 03, Choose what to record)

Starts on `/Admin`, from the site left by `02-enable`.

**`settings-1`** (about 11 s)
- Click Settings in the admin menu, then click Audit Trail (`ClickAndWaitForNavigationAsync`): the page
  `/Admin/Settings/AuditTrail` opens. Wait until the table with the column headers Event, Description and Enabled is
  visible. If the Events tab is not the one shown, click the tab "Events".
- `HighlightAsync` the header cell "Enabled".
- On screen when the narration ends: the page Audit Trail with the table "Events to record": the row "Content" in bold
  and, under it, Created, Saved, Published, Unpublished, Removed, Cloned and Restored, each with a description and a
  checked box, and in the row "Content" a box "Select all" under the header "Enabled"; a ring around the header "Enabled".
- Labels: menu `.../AuditTrail/Navigation/AuditTrailAdminMenu.cs:42-43`. Page heading
  `.../AuditTrail/Views/AuditTrailSettings.Edit.cshtml:8`. "Events to record", Event, Description, Enabled:
  `.../AuditTrail/Views/AuditTrailSettings.Edit.cshtml:12,17-19`. Tab: `.../AuditTrail/Drivers/AuditTrailSettingsDisplayDriver.cs:76`.
  Event names and descriptions: `src/OrchardCore.Modules/OrchardCore.Contents/AuditTrail/Services/ContentAuditTrailEventConfiguration.cs:19-26`
  (the last argument, `true`, is why they are checked).

**`settings-2`** (about 9 s)
- `ClearHighlightsAsync`. Click the tab "Trimming". `HighlightAsync` the field "Retention period".
- On screen when the narration ends: the Trimming tab with "Disable trimming task" unchecked, "Retention period"
  holding 10 (ring around it), and "Last run" showing "Never".
- Labels: `.../AuditTrail/Views/AuditTrailTrimmingSettings.Edit.cshtml:8,15,18,23`; tab:
  `.../AuditTrail/Drivers/AuditTrailTrimmingSettingsDisplayDriver.cs:39`; default 10:
  `.../AuditTrail/Settings/AuditTrailTrimmingSettings.cs:5`; daily schedule:
  `.../AuditTrail/Services/AuditTrailBackgroundTask.cs:13`.

**`settings-3`** (about 10 s)
- `ClearHighlightsAsync`. Click the tab "Content". Wait until the label "Allowed content types" is visible. Click the
  checkbox labeled "Article" (`scene.Page.GetByLabel("Article", new() { Exact = true })`; fallback
  `input[value="Article"]`).
- On screen when the narration ends: the Content tab, "Allowed content types" with its hint "The types of content
  whose events are recorded.", the box "Article" checked and every other box unchecked.
- Labels: `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/ContentAuditTrailSettings.Edit.cshtml:6,10`; the
  check boxes: `.../Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml`; tab:
  `.../Contents/AuditTrail/Drivers/ContentAuditTrailSettingsDisplayDriver.cs:41`; the label "Article" is the display name
  of the type in the Blog recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, type Article); nothing is
  recorded until a type is checked: `.../Contents/AuditTrail/Handlers/AuditTrailContentHandler.cs:74` and
  `.../Contents/AuditTrail/Settings/ContentAuditTrailSettings.cs:5`.

**`settings-4`** (about 4 s)
- Click the button "Save" (`button.save`) with `ClickAndWaitForNavigationAsync`.
- On screen when the narration ends: the page Audit Trail reloaded, with the green message "Site settings updated
  successfully." at the top.
- Labels: Save `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; message
  `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`.

After this clip, create the seed of section 2.2 and take snapshot B.

### Clip `04-change` (chapter 04, Make a change)

Starts on `/Admin`, from snapshot B.

**`change-1`** (about 9 s; keep every pauseAfter at 300 or less)
- Click Content in the admin menu, then Content Items (`ClickAndWaitForNavigationAsync`): the page
  `/Admin/Contents/ContentItems` opens ("Manage Content").
- Click the search box (placeholder "Search"), type `Pruning`, press Enter (`WaitForNavigationAsync`). One row
  remains: "Pruning workshop", badge Article.
- Click Edit on that row (`ClickAndWaitForNavigationAsync`): the page "Edit Article" opens with the Title field
  holding "Pruning workshop".
- If the actions run longer than the narration, record the search as one move: open
  `/Admin/Contents/ContentItems?q=Pruning` in the step with `NavigateAsync` and keep the typing out of the video.
- On screen when the narration ends: the editor of the Article, the Title field holding "Pruning workshop".
- Labels: Content, Content Items: `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65,68`. Page title
  "Manage Content": `.../Contents/Views/Admin/List.cshtml:17`. Search: `.../Contents/Views/ContentsAdminListSearch.cshtml:28`. Edit:
  `.../Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`. Edit page title "Edit Article": `.../Contents/Views/Admin/Edit.cshtml:39`.

**`change-2`** (about 4 s; type fast, delay 25)
- Click the Title field, select its text (Ctrl+A), type `Pruning workshop for beginners`. Click Publish
  (`ClickAndWaitForNavigationAsync`; the main button of the split button, not the arrow).
- On screen when the narration ends: the new page is loading or loaded (see `change-3`); the typed title is in the
  video before the click.
- Labels: Title `src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`; Publish
  `.../Contents/Views/Content.PublishButton.cshtml:7,16`.

**`change-3`** (about 7 s)
- No action. Wait (before the step starts) until the message is visible.
- On screen when the narration ends: the Content Items list, which the Edit link returned to, with the green message
  "Your Article has been published." at the top and the row "Pruning workshop for beginners".
- Labels: `src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:966-967` (the edit and publish
  path). The module records the event; nothing about it is visible in this clip.

After this clip take snapshot C.

### Clip `05-list` (chapter 05, Read the log)

Starts on `/Admin`, from snapshot C. Expected log, newest first: Published (Version 2, by `admin`) and Published and
Created (Version 1, by `jamie.chen`). The event rows are the `li` elements of `ul.admin-list-list` after the first one
(the first `li` is the toolbar with the item count and the lists). Call the first event row Row 1, the next Row 2.

**`list-1`** (about 12 s)
- Click Tools, then Audit Trail (`ClickAndWaitForNavigationAsync`): the page `/Admin/AuditTrail` opens. Wait until Row 1
  is visible. `HighlightAsync` Row 1's `div.summary` (the badges and the text).
- On screen when the narration ends: the page Audit Trail with its search box, the item count above the rows, Row 1
  with the badges "Published", "Content", "x minutes ago" and "admin", ringed.
- Labels: Tools, Audit Trail `.../AuditTrail/Navigation/AuditTrailAdminMenu.cs:33-34`; breadcrumb
  `.../AuditTrail/Views/Admin/Index.cshtml:5`; badges: event name `.../AuditTrail/Views/AuditTrailEventName.SummaryAdmin.cshtml:6`,
  category `.../AuditTrail/Views/AuditTrailEventCategory.SummaryAdmin.cshtml:6`, time and user
  `.../AuditTrail/Views/AuditTrailEventMeta.SummaryAdmin.cshtml:9,12`; "Newest" is the default sort
  `.../AuditTrail/Startup.cs:78`; time wording `src/OrchardCore/OrchardCore.DisplayManagement/Shapes/DateTimeShapes.cs:93`.

**`list-2`** (about 12 s)
- `ClearHighlightsAsync`. `HighlightAsync` Row 1's `small` text (the version and the title). Move the cursor over the
  link "Version 2" (`MoveToAsync`), then over the title link, without clicking.
- On screen when the narration ends: Row 1's text "Version 2 of the Article Pruning workshop for beginners was
  published" ringed (check the exact sentence: `OPEN-QUESTIONS.md`, item 3).
- Labels: `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventData.SummaryAdmin.cshtml:10,41`.

**`list-3`** (about 16 s)
- `ClearHighlightsAsync`. `HighlightAsync` Row 1's `div.actions` (View, Details). Then `ClearHighlightsAsync` and
  `HighlightAsync` the link "Restore" in Row 2 (Jamie's Published event of Version 1, whose version is no longer the
  latest).
- On screen when the narration ends: Row 2 with its buttons View, Restore and Details and a ring around Restore.
- Labels: View, Restore `.../Contents/Views/Items/AuditTrailContentEventActions.SummaryAdmin.cshtml:8,22,37`; Details
  `.../AuditTrail/Views/AuditTrailEventActions.SummaryAdmin.cshtml:3`. Restore is shown only for Published and Removed events
  whose version is not the latest (same file, lines 10-39).

**`list-4`** (about 12 s; do not choose an option)
- `ClearHighlightsAsync`. Click the list "Date" (`button.dropdown-toggle[title="Date"]`) and hold 1 s on the open options;
  press Escape. Click "Category" (`button.dropdown-toggle[title="Category"]`), hold 1 s, press Escape. Click "Sort"
  (`button.dropdown-toggle[title="Sort"]`), hold 1 s, press Escape. Click "Date" again and leave it open until the
  narration ends (the narration ends on the date ranges). Choosing an option reloads the page and would change the list.
- On screen when the narration ends: the list "Date" open, showing Any date, Last 24 hours, Previous 48 hours, This week
  and the ranges below them, with the lists Category and Sort in the toolbar beside it. Press Escape after the step.
- Labels: titles `.../AuditTrail/Views/AuditTrailAdminListFilters.cshtml:6,9,15`; options Any date, Last 24 hours, This
  week `.../AuditTrail/Services/DefaultAuditTrailAdminListQueryService.cs:96,100,106`; All categories `:75`; sort options
  `.../AuditTrail/Startup.cs:78,83,87,100,113`.

**`list-5`** (about 15 s)
- Click the search box (`input[type=search]`, placeholder "Search"), type `jamie` (delay 60), press Enter
  (`WaitForNavigationAsync`). Wait until the item count shows two items. Click the Filters button (`#filter-dropdown`,
  the funnel icon). Do not open Filter syntax.
- On screen when the narration ends: the search box holding "jamie", only the two events by `jamie.chen`, and the menu
  of the Filters button open with the entries "Content" and "Filter syntax".
- Labels: search `.../AuditTrail/Views/AuditTrailAdminListSearch.cshtml:33`; Filters (tooltip) `:21`; Filter syntax `:28`;
  the default search term is the user name `.../AuditTrail/Services/DefaultAuditTrailAdminListFilterProvider.cs:171` (`WithDefaultTerm("username")`).

### Clip `06-details` (chapter 06, Event details)

Starts on `/Admin/AuditTrail` (no search term), from snapshot C.

**`details-1`** (about 11 s)
- Click Details in Row 1 (`ClickAndWaitForNavigationAsync`): the page `/Admin/AuditTrail/Display/<event id>` opens.
  Wait until the heading "Event Info" is visible. `HighlightAsync` the table under it.
- On screen when the narration ends: the title "Audit Trail Event" (breadcrumb), the table "Event Info" with Category
  "Content", Event name "Published", Triggered by username "admin", Triggered by user ID, Triggered at timestamp, and an
  empty Client IP address, ringed.
- Labels: `.../AuditTrail/Views/Admin/Display.cshtml:4`; `.../AuditTrail/Views/AuditTrailEventDetail.DetailAdmin.cshtml:3,8,12,16,20,24,28`.

**`details-2`** (about 9 s)
- `ClearHighlightsAsync`. `ScrollAsync(0, 300)` if the next table is not visible. `HighlightAsync` the table under
  "Content Item Info".
- On screen when the narration ends: the table "Content Item Info" with Content Item Id, Content type "Article",
  Display text "Pruning workshop for beginners", Version "Version 2", and an empty Comment, ringed.
- Labels: `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentEventDetail.DetailAdmin.cshtml:15,20,24,28,39,57`.

**`details-3`** (about 12 s; do not open the tab)
- `ClearHighlightsAsync`. `ScrollAsync(0, -300)` back to the top. `HighlightAsync` the tab "Diff" in the tab bar above
  the tables. Do not click it: the Diff tab shows the content of two versions as text, and the video leaves that to
  the video about comparing and restoring versions.
- On screen when the narration ends: the tab bar with the tab "Diff" ringed, the first tab's content under it.
- Labels: the tab is placed by `.../Contents/AuditTrail/Drivers/AuditTrailContentEventDisplayDriver.cs:108` ("Content:5#Diff").
  The tab compares the item as this event recorded it with the item as the latest earlier event of the same item
  recorded it (same file, lines 82-107; the query is by the content item id, not by version number).
- The page of a Content event asks the browser for the Prism scripts of the Diff tab from a public script CDN
  (`.../Contents/Views/Items/AuditTrailContentEventDiff.DetailAdmin.cshtml:18-21`), and the diff script of the module
  adds a web-font stylesheet. Nothing of this is visible while the tab stays closed, but record with a network
  connection, or the browser may wait on those requests before the page is idle.

### Clip `07-item` (chapter 07, History of one item)

Starts on `/Admin`, from snapshot C.

**`item-1`** (about 9 s)
- Click Content, then Content Items (`ClickAndWaitForNavigationAsync`). The list is sorted by Recently modified, so the
  Article "Pruning workshop for beginners" is the first row (if not, search for `Pruning` before the capture).
- Click the button "Actions" of that row (`button.actions` in the row), hold 0.8 s on the open menu, click "Audit Trail"
  (`ClickAndWaitForNavigationAsync`): the page `/Admin/AuditTrail/<content item id>` opens.
- On screen when the narration ends: the page Audit Trail listing the three events of the Article.
- Labels: Actions `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml:30-32`; Audit Trail (the menu item)
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/AuditTrailContentsAction_SummaryAdmin.cshtml:3`; sort default
  `.../Contents/Controllers/AdminController.cs:201`.

**`item-2`** (about 9 s)
- `HighlightAsync` the list "Events" (`button.dropdown-toggle[title="Events"]`), then click it and hold 1 s on the open
  options; press Escape (do not choose one). Do not show a "Category" list: it is absent on this page.
- On screen when the narration ends: the toolbar with the lists Date, Events and Sort and no list Category, and the
  three events of the Article under it.
- Labels: `.../AuditTrail/Views/AuditTrailAdminListFilters.cshtml:8-14` (the Category list is hidden and the Events list shown
  when the page comes from a content item); the Events options come from the category's events
  `.../AuditTrail/Services/DefaultAuditTrailAdminListQueryService.cs:77-88`.

## 5. Slide steps

Slide steps are drawn by `slides.py`; nothing is recorded for them.

- `why-1`: heading "Questions a log can answer"; four cards Who, What, When, Why; Who, What and When are bright.
- `why-2`: heading "What an event holds"; four cards Category, Event name, User, Time.
- `why-3`: heading "Categories of events"; two cards, Content (added by the Contents module) and User (added by the Users
  Audit Trail feature).
- `notes-1`: heading "Who can use it"; the permissions View audit trail, Manage audit trail settings and Manage
  settings, and the Administrator role.
- `notes-2`: heading "Comments on changes"; three cards: a content part, saved with the event, shown in the log.
- `notes-3`: heading "More to know"; three cards: user events, the Allow client IP address box, personal data.

The opening card (`intro`) and the closing card (`outro`) have no recording.

## 6. Checks for private data

- The video shows only fake names and addresses: `admin`, `jamie.chen`, `jamie.chen@example.com` (not shown on any
  clip), `admin@example.com` (not shown). No real person's name, email, phone number or token may appear.
- The Client IP address is not recorded (the box "Allow client IP address" stays unchecked), so no address appears in the
  log; check the rows and the Event Info table of `06-details`.
- The password is never typed on camera. Nothing in the capture shows `state.json` or the sign-in page.
- The browser must show no bookmark bar, autofill suggestion or extension.
- OCR the finished video at one frame per second and at every scene change (the skill's step 6) and search for: any
  address that does not end in `@example.com`, any user name other than `admin` and `jamie.chen`, any host name other
  than `localhost`, and the machine's user name or folders.
- The Event ids and Content Item Ids shown by `06-details` are generated identifiers of the throwaway site; they hold
  no private data.

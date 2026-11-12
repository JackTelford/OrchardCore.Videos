# Recording plan: Audit Trail Settings: Events, Retention and User Events

Read this file and nothing else to record the video. It lists the site, the order of the clips, and for every step the
page, the actions, what must be on screen when the narration of the step ends, and where each label comes from.

"OC" is the Orchard Core source at commit 92db4a83. In the `Labels` lines, paths start from the root of that source and
use these short forms:

- `AT` = `src/OrchardCore.Modules/OrchardCore.AuditTrail`
- `CT` = `src/OrchardCore.Modules/OrchardCore.Contents`
- `US` = `src/OrchardCore.Modules/OrchardCore.Users`
- `ST` = `src/OrchardCore.Modules/OrchardCore.Settings`
- `AA` = `src/OrchardCore/OrchardCore.AuditTrail.Abstractions`

A selector written as `...` stands for `scene.Page.Locator("...")`. Narrate first (`narrate.py`): the recorder holds
each step for the length of its narration.

## 1. The site

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of the Orchard Core checkout at 92db4a83, run on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data folder | `ORCHARD_APP_DATA` points to an empty folder outside both repositories |
| Provisioning | Auto Setup, so no setup or sign-in form is recorded: tenant `Default`, recipe `Blog`, site name `Orchard Core`, time zone `America/Los_Angeles`, database Sqlite |
| Administrator | user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address (for example `http://127.0.0.1:9`), `NO_PROXY=localhost,127.0.0.1`; this video calls no external service and needs no stand-in |

Auto Setup variables (values in angle brackets are not written to git):

```bash
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

This video does not record the Setup screen: it uses `state.json` and Auto Setup as every other video does.

### Features to enable before recording (outside the capture)

The Blog recipe enables Contents, Users and Deployment, but not the Audit Trail (OC `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`).

1. Open `/Admin/Features` (Tools, Features).
2. Type `Audit Trail` in Search. Select the checkboxes of **Audit Trail** and **Users Audit Trail** (both in the Security
   category), open Bulk Actions, choose Enable, and confirm.
3. Leave **Users Registration**, **Users Reset Password** and **Custom User Settings** disabled. With them off, the User
   category of the settings page holds only the nine base events (Logged in, Login failed, Login account locked,
   Enabled, Disabled, Created, Updated, Deleted, Confirmed) and the list of user properties holds no custom settings.
4. Do not enable Background Tasks or any other feature.

Settings: save none. The video starts from the defaults (no event settings saved, retention 10 days, trimming on, no
content type allowed, nothing stored in the user snapshot).

### Sign in and first snapshot

Sign the recorder in once (`Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`). This creates `state.json`
(git-ignored). The sign-in happens after the features are on, so the Audit Trail list later holds a **Logged in**
entry for `admin`: that is fake data and may stay.

Stop the site, copy its `App_Data` folder and `state.json` aside as **snapshot A**, and start the site again.

### Seed data (created outside the capture, after clip 02, starting from snapshot B)

Only fake values. No user account is created.

| What | How | Event it produces |
|---|---|---|
| One failed sign-in | In a private browser window (so the administrator session is not touched) open `http://localhost:5010/Login`. Type the user name `guest.visitor`, any text as the password, and sign in. The page answers "Invalid login attempt." (OC `src/OrchardCore.Modules/OrchardCore.Users/Controllers/AccountController.cs:206-212`). | Login failed, category User, user name `guest.visitor`, with the client IP address |
| One published Page | Open `/Admin/Contents/ContentTypes/Page/Create` (OC `src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:407`), type the title `Garden Club Meeting`, press **Publish** | Content events for the Page, with the client IP address |

Do not edit a Blog Post as seed data: its type is not allowed, so it leaves no entry (a useful check, not a scene).

## 2. Order of the clips and the snapshots

Clips that change the site: `02-events` (event settings, client IP address, allowed content types), `03-trimming`
(retention period), `04-users` (user snapshot setting). `05-result` only reads. `01-where` and `04-options` are slides,
`00-intro` and `99-outro` are cards: nothing is recorded for them.

| # | Action | Starts from | Changes the site |
|---|---|---|---|
| 1 | record `02-events` | snapshot A | yes: event settings, client IP address, allowed content type |
| 2 | stop the site, copy `App_Data` and `state.json` aside as **snapshot B**, start the site | | |
| 3 | record `03-trimming` | snapshot B | yes: retention period 90 |
| 4 | restore snapshot B, record `04-users` | snapshot B | yes: Email stored with PartialAsteriskRedactor |
| 5 | restore snapshot B, create the seed data above, stop the site, copy aside as **snapshot C**, start the site | snapshot B | yes: audit trail events |
| 6 | record `05-result` | snapshot C | no |

`03-trimming` and `04-users` do not depend on each other, so each starts from snapshot B. `05-result` depends only on
`02-events` (the Login failed event, the client IP address and the allowed Page type) and on the seed data.

To record one clip again: stop the site, restore the snapshot named in the table (`App_Data` and `state.json`), start the
site, record. `03-trimming` must start with a retention period of 10 (its narration says so), and `04-users` with every
drop-down empty.

Menus: every clip that starts on `/Admin` finds the admin menu groups closed. Opening them is part of the story in
`events-1`, `users-1` and `result-1`; the recorder must not open them before `StartCaptureAsync`.

Things a person must confirm on the running site before the first take are listed in OPEN-QUESTIONS.md.

## 3. Clips

### 00-intro (card) and 99-outro (card)

`intro` and `outro` are cards drawn by `assemble.py`: nothing to record.

### 01-where (slides, `slides.py`)

- `where-1`: heading "Two features, two settings pages". Three cards: "Tools, Features", "Settings, Audit Trail", "Settings, Security, User Audit Trail". The first is bright, the other two dimmed.
- `where-2`: same slide, the second and third cards bright, the first dimmed.

### 02-events (recording)

Starts on: `await scene.OpenAsync("/Admin")`, then `StartCaptureAsync()`. The admin dashboard of the Default tenant, menu groups closed.

- `events-1`
  - Actions: `ClickAsync("#adminMenu button.item-label:has-text(\"Settings\")", pauseAfter: 700)` (the Settings group opens); `ClickAndWaitForNavigationAsync("#adminMenu a[href$=\"/Admin/Settings/AuditTrail\"]")`; wait until `th:has-text("Description")` is visible.
  - On screen when the narration ends: the Audit Trail settings page on its first tab, **Events**: the label "Events to record" and the table with the columns **Event**, **Description**, **Enabled**, and the first category with its events.
  - Labels: menu "Settings" > "Audit Trail" `AT/Navigation/AuditTrailAdminMenu.cs:42-50`; the page URL `/Admin/Settings/AuditTrail` from `AA/Settings/AuditTrailSettingsGroup.cs:5` and `ST/Controllers/AdminController.cs:50`; "Events to record" `AT/Views/AuditTrailSettings.Edit.cshtml:12`; the columns Event, Description and Enabled `AT/Views/AuditTrailSettings.Edit.cshtml:17-19`; the tab name "Events" `AT/Drivers/AuditTrailSettingsDisplayDriver.cs:76`.
- `events-2`
  - Actions: `HighlightAsync` on the header row of the Content category, `tr.table-secondary:has(strong:text-is("Content"))`, when the narration says "The Content category"; hold about 2 seconds; `ClearHighlightsAsync()`; `ScrollAsync(0, 500)` (scroll until the header row of the User category is on screen); `HighlightAsync` on `tr.table-secondary:has(strong:text-is("User"))`. If the User category comes first on the page, do the same in the order of the narration (Content first, then User).
  - On screen when the narration ends: the User category header ringed, and its events listed below it (Logged in, Login failed, Login account locked, Enabled, Disabled, Created, Updated, Deleted, Confirmed, each with its description).
  - Labels: the category names are printed as written in the code `AT/Views/AuditTrailSettings.Edit.cshtml:30`; the Content events `CT/AuditTrail/Services/ContentAuditTrailEventConfiguration.cs:20-26`; the User events `US/AuditTrail/Services/UserAuditTrailEventConfiguration.cs:26-34`.
- `events-3`
  - Scope every locator of this step to the User category, because "Created" and other names exist in both categories: `var user = scene.Page.Locator("tbody.check-all-container:has(tr.table-secondary strong:text-is(\"User\"))");`
  - Actions: `ClearHighlightsAsync()`; `HighlightAsync(user.GetByLabel("Select all"))`; hold about 1.5 seconds; `ClearHighlightsAsync()`; `ClickAsync(user.GetByLabel("Login failed", new() { Exact = true }), pauseAfter: 900)`.
  - On screen when the narration ends: the **Login failed** checkbox is checked. The **Select all** checkbox of the User category is checked too (the page script checks it when every event of the category is on: `AT/Assets/ts/audit-trail-settings.ts`, lines 11-13 and 21-25).
  - Labels: "Select all" `AT/Views/AuditTrailSettings.Edit.cshtml:35`; "Login failed", off by default, `US/AuditTrail/Services/UserAuditTrailEventConfiguration.cs:27`.
- `events-4`
  - Actions: `ClearHighlightsAsync()`; `ScrollAsync(0, 700)` until the checkbox labeled "Allow client IP address" is fully on screen (it is under the table); `ClickAsync(scene.Page.GetByLabel("Allow client IP address"), pauseAfter: 600)`; `HighlightAsync` on the same checkbox.
  - On screen when the narration ends: the checkbox **Allow client IP address** checked and ringed, with its hint "Whether the client IP address is recorded."
  - Labels: `AT/Views/AuditTrailSettings.Edit.cshtml:72-73`.
- `events-5`
  - Actions: `ClearHighlightsAsync()`; `ScrollAsync(0, -2000)` back to the top; `ClickAsync("ul.nav-tabs a.nav-link:text-is(\"Content\")", pauseAfter: 700)`.
  - On screen when the narration ends: the **Content** tab is active: the label "Allowed content types", the hint "The types of content whose events are recorded.", and the content type checkboxes, none checked.
  - Labels: the tab name "Content" `CT/AuditTrail/Drivers/ContentAuditTrailSettingsDisplayDriver.cs:41`; "Allowed content types" and the hint `CT/Views/Items/ContentAuditTrailSettings.Edit.cshtml:6,10`; the empty default `CT/AuditTrail/Settings/ContentAuditTrailSettings.cs:5`; the check that skips a content type that is not selected `CT/AuditTrail/Handlers/AuditTrailContentHandler.cs:74`.
- `events-6`
  - Actions: `ClickAsync(scene.Page.Locator("div.tab-pane.active").GetByLabel("Page", new() { Exact = true }), pauseAfter: 700)`; `HighlightAsync` on that checkbox.
  - On screen when the narration ends: the **Page** checkbox checked and ringed; every other content type unchecked.
  - Labels: the display name of the type is "Page" in the Blog recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, content type `Page`), printed by `CT/Views/Shared/Components/SelectContentTypes/Default.cshtml:22-23`.
- `events-7`
  - Actions: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync("button.save")` (the button "Save"); wait up to 30 seconds for the text "Site settings updated successfully." to be visible. If the save plus the wait takes longer than the narration (about 4 seconds), put the wait in `scene.OffCameraAsync(...)` so the last frame shows the message.
  - On screen when the narration ends: the message "Site settings updated successfully." at the top of the page.
  - Labels: "Save" OC `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; the message `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`.

### 03-trimming (recording)

Starts on: `await scene.OpenAsync("/Admin/Settings/AuditTrail")` (not recorded; the Events tab is active, no message), then `StartCaptureAsync()`. Starts from snapshot B, so the retention period is still 10.

- `trim-1`
  - Actions: `ClickAsync("ul.nav-tabs a.nav-link:text-is(\"Trimming\")", pauseAfter: 600)`; `HighlightAsync(scene.Page.GetByLabel("Retention period"))`.
  - On screen when the narration ends: the **Trimming** tab with "Disable trimming task" (unchecked), "Retention period" with the value 10 (ringed) and "Last run".
  - Labels: the tab name "Trimming" `AT/Drivers/AuditTrailTrimmingSettingsDisplayDriver.cs:39`; "Retention period" and its hint `AT/Views/AuditTrailTrimmingSettings.Edit.cshtml:15,18`; the default of 10 `AT/Settings/AuditTrailTrimmingSettings.cs:5`.
- `trim-2`
  - Actions: `ClearHighlightsAsync()`; `HighlightAsync("div.ocat-wrapper:has(label:text-is(\"Last run\")) div.form-control")`; hold until the narration ends.
  - On screen when the narration ends: the **Last run** value box ringed (it reads "Never" on a site where the task has not run; a date is also fine).
  - Labels: "Last run" and "Never" `AT/Views/AuditTrailTrimmingSettings.Edit.cshtml:23,25`; the task, titled "Audit Trail Events Purger", with its daily schedule `AT/Services/AuditTrailBackgroundTask.cs:11-14`.
- `trim-3`
  - Actions: `ClearHighlightsAsync()`; `var days = scene.Page.GetByLabel("Retention period");` `ClickAsync(days)`; `scene.Page.Keyboard.PressAsync("Control+A")`; `scene.Page.Keyboard.TypeAsync("90", new() { Delay = 80 })`. The field is a plain number input and the view attaches no script to it (`AT/Views/AuditTrailTrimmingSettings.Edit.cshtml:17`), so the typed value is submitted as it is.
  - On screen when the narration ends: the field shows **90**.
  - Labels: the field is "Retention period", as in `trim-1`.
- `trim-4`
  - Actions: `HighlightAsync(scene.Page.GetByLabel("Disable trimming task"))` without clicking it; hold about 2.5 seconds; `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync("button.save")`; wait up to 30 seconds for "Site settings updated successfully." If this takes longer than the narration, put the wait in `OffCameraAsync`.
  - On screen when the narration ends: the message "Site settings updated successfully."
  - Labels: "Disable trimming task" and its hint `AT/Views/AuditTrailTrimmingSettings.Edit.cshtml:8-9`; "Save" and the message as in `events-7`.

### 04-options (slides, `slides.py`)

- `options-1`: heading "Ways to store a user property". Four cards: "Store", "PartialAsteriskRedactor", "ErasingRedactor", "HmacRedactor". The first two are bright.
- `options-2`: same slide, the last two cards bright.

### 04-users (recording)

Starts on: `await scene.OpenAsync("/Admin")` (menu groups closed), then `StartCaptureAsync()`. Starts from snapshot B; every drop-down of the page is empty.

- `users-1`
  - Actions: `ClickAsync("#adminMenu button.item-label:has-text(\"Settings\")", pauseAfter: 600)`; `ClickAsync("#adminMenu button.item-label:has-text(\"Security\")", pauseAfter: 600)`; `ClickAndWaitForNavigationAsync("#adminMenu a[href$=\"/Admin/Users/AuditTrail/Index\"]")`.
  - On screen when the narration ends: the page of the user audit trail settings with the heading **User Properties to Store** and the first properties, each with an empty drop-down.
  - Labels: menu "Settings" > "Security" > "User Audit Trail" `US/AuditTrail/AdminMenu.cs:28-30`; the URL `US/AuditTrail/Controllers/AuditTrailAdminController.cs:18`; "User Properties to Store" `US/Views/AuditTrailAdmin/Index.cshtml:15`.
- `users-2`
  - Actions: `HighlightAsync("select.form-select >> nth=0")`; hold about 2 seconds; `ClearHighlightsAsync()`; `ScrollAsync(0, 300)` slowly, so about ten property names and their empty drop-downs pass.
  - On screen when the narration ends: a stretch of the list with property names (for example AccessFailedCount, Email, EmailConfirmed, Id) and empty drop-downs. PasswordHash, ResetToken and UserTokens are not in the list.
  - Labels: the property names come from the user object `US/AuditTrail/Controllers/AuditTrailAdminController.cs:49-53`; the three properties that are never offered `US/AuditTrail/Handlers/UserEventHandler.cs:29-35`; the empty default (an empty option) `US/Views/AuditTrailAdmin/Index.cshtml:24` and `US/AuditTrail/Controllers/AuditTrailAdminController.cs:58-64`.
- `users-3`
  - Actions: `ClearHighlightsAsync()`; scroll until the row labeled "Email" is on screen; `SelectAsync(scene.Page.GetByLabel("Email", new() { Exact = true }), "PartialAsteriskRedactor")`; hold 1 second; `ClickAndWaitForNavigationAsync("input[type=submit][value=Save]")`; scroll to the "Email" row again.
  - On screen when the narration ends: the page reloaded, with the **Email** drop-down showing **PartialAsteriskRedactor**. This page shows no confirmation message: the redirect is the confirmation.
  - Labels: the option texts (empty, "ErasingRedactor", "PartialAsteriskRedactor", "Store") `US/Views/AuditTrailAdmin/Index.cshtml:24-31`; the redactors that are registered `US/AuditTrail/Startup.cs:42-45`; "Save" `US/Views/AuditTrailAdmin/Index.cshtml:41`; the redirect after saving `US/AuditTrail/Controllers/AuditTrailAdminController.cs:91`.
- `users-4`
  - Actions: scroll to the bottom of the page; `HighlightAsync("span.hint")`; never click the link in the hint.
  - On screen when the narration ends: the hint under the list (it mentions the "User Snapshot" property) ringed, with the **Save** button below.
  - Labels: the hint `US/Views/AuditTrailAdmin/Index.cshtml:37`; the events that store a snapshot (Created, Updated, Enabled, Disabled, Confirmed) `US/AuditTrail/Handlers/UserEventHandler.cs:90-100`, `:125-126` and `:231-237`; the removal of snapshots when a user is deleted `US/AuditTrail/Handlers/UserEventHandler.cs:102-121`; "User Snapshot" on the Details page `US/Views/Items/AuditTrailUserEventDetail.DetailAdmin.cshtml:20`.

### 05-result (recording)

Starts on: `await scene.OpenAsync("/Admin")` (menu groups closed), then `StartCaptureAsync()`. Starts from snapshot C: the seed data of section 1 exists.

- `result-1`
  - Actions: `ClickAsync("#adminMenu button.item-label:has-text(\"Tools\")", pauseAfter: 600)`; `ClickAndWaitForNavigationAsync("#adminMenu a[href$=\"/Admin/AuditTrail\"]")`; wait until the list shows a row with "Login failed"; `HighlightAsync(".list-group-item:has-text(\"Login failed\"), tbody tr:has-text(\"Login failed\") >> nth=0")`.
  - On screen when the narration ends: the Audit Trail list, newest first, with the **Login failed** entry (badges "Login failed" and "User") ringed.
  - Labels: menu "Tools" > "Audit Trail" `AT/Navigation/AuditTrailAdminMenu.cs:33-41`; the event and category badges `AT/Views/AuditTrailEventName.SummaryAdmin.cshtml:6` and `AT/Views/AuditTrailEventCategory.SummaryAdmin.cshtml:6`.
- `result-2`
  - Actions: `ClearHighlightsAsync()`; `HighlightAsync("span.badge:has(i.fa-wifi) >> nth=0")`; hold about 3 seconds; `ClearHighlightsAsync()`; `HighlightAsync` on the first row with the badge "Published" (the Page entry), `.list-group-item:has-text("Published"), tbody tr:has-text("Published") >> nth=0`.
  - On screen when the narration ends: a content entry for the Page ringed (event badge "Published", category "Content"); the badge with the IP address was ringed a moment before.
  - Expected: the older **Logged in** entry for `admin` (recorded by `Recorder setup` before the IP address setting was saved) has no badge, which is why the narration speaks only of entries recorded after the setting was allowed. Entries from the seed data and from later sign-ins carry the badge.
  - Labels: the IP address badge, shown only when an address was recorded, `AT/Views/AuditTrailEventMeta.SummaryAdmin.cshtml:14-18`.
- `result-3`
  - Actions: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync` on the link "Details" of the Login failed row (`scene.Page.Locator(".list-group-item, tbody tr").Filter(new() { HasText = "Login failed" }).First.GetByRole(AriaRole.Link, new() { Name = "Details" })`); wait for `h2:has-text("Event Info")`; after about 3 seconds `HighlightAsync("tr:has(th:text-is(\"Client IP address\"))")`.
  - On screen when the narration ends: the **Event Info** table (Category, Event name, Triggered by username, Triggered by user ID, Triggered at timestamp, Client IP address) with the **Client IP address** row ringed. The **User Info** table below shows no User Snapshot row, because a failed sign-in stores none. No snapshot panel is in the frame.
  - Labels: "Details" `AT/Views/AuditTrailEventActions.SummaryAdmin.cshtml:3`; "Event Info" and its rows `AT/Views/AuditTrailEventDetail.DetailAdmin.cshtml:3-28`; "User Info" `US/Views/Items/AuditTrailUserEventDetail.DetailAdmin.cshtml:5`.

## 4. Checks for private data

Look at every contact sheet (`review.py`) and then OCR the video, as `SKILL.md` asks.

- The only names on screen are `admin` and the typed name `guest.visitor` (a name that has no account). `admin@example.com` is never shown, there is no other account, and the Users list is never opened.
- The IP address badge shows the loopback address of the recording machine (`::1` or `127.0.0.1`), never a network address. If any other address appears, record again with the site reached through `localhost`.
- No password, token or key is typed on camera. The failed sign-in password is any text and is typed outside the capture.
- The page title and the Page `Garden Club Meeting` are fake. No real site name, host name or file path is visible (the browser address bar is not part of the capture).
- No User Snapshot panel appears in any frame (it would show the stored email, even redacted).

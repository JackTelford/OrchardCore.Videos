# Recording plan: Publish Later and Archive Later

A person who reads only this file records the video. In this plan a selector stands for `scene.Page.Locator(...)`,
and every label is spelled as the Orchard Core source spells it (the file and line follow each step). Paths of the
source are relative to `src/OrchardCore.Modules` of the Orchard Core checkout, unless they start with `src/`.

The video is an admin video: it shows the admin and the public site, and never code or configuration.

| Chapter | Clip | Kind | Steps |
|---|---|---|---|
| 00 | `00-intro` | card | `intro` |
| 01 | `01-idea` | slides | `idea-1`, `idea-2` |
| 02 | `02-features` | recording | `features-1`, `features-2` |
| 02 | `02-parts` | recording | `parts-1`, `parts-2` |
| 03 | `03-publish` | recording | `publish-1` to `publish-6` |
| 04 | `04-archive` | recording | `archive-1` to `archive-4` |
| 05 | `05-cancel` | recording | `cancel-1` to `cancel-3` |
| 06 | `06-know` | slides | `know-1`, `know-2` |
| 99 | `99-outro` | card | `outro` |

## 1. The site it needs

- **Orchard Core:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout at commit 92db4a83 or a later
  commit of the same branch, on `http://localhost:5010`, with its own `App_Data` folder outside both repositories
  (`ORCHARD_APP_DATA`). A fresh site: nothing is copied from another site.
- **Auto Setup** (no setup screen and no sign-in form are recorded), through environment variables:

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Outbound calls:** set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls out.
- **Sign-in:** `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`; the result is `state.json`
  (git-ignored). Sign in outside the capture.
- **Features to enable before recording:** none. The Blog recipe already enables Contents, Content Types, Features,
  Settings, Autoroute, Title and Html. The Publish Later and Archive Later features are off at the start, because
  clip `02-features` enables them on camera. Do not enable the User Time Zone feature and do not enable Background Tasks.
- **Settings:** none to change. The site time zone is `America/Los_Angeles` (Settings, General, Default Time Zone),
  set by Auto Setup.
- **External services and stand-ins:** none.
- **Fake seed data created outside the capture:** none before clip `03-publish`; the two items of the section "Seed data
  for clip `05-cancel`" are created after clip `04-archive`. The Blog recipe's own items (a blog, a post, an
  article called About, a menu) stay as they are; they are fake text from the recipe.
- **The clock:** the machine clock must be right. The scheduled changes happen when the real clock passes the chosen
  time, so a clip that waits takes real minutes (cut out of the video by `OffCameraAsync`).
- **Do not record around a daylight-saving change** of `America/Los_Angeles` (the hour from 01:00 to 03:00 local time
  of the change). The editor converts the typed time strictly, and a skipped or repeated local time is refused.
- **Keep the site in use.** The background tasks of a tenant run once the tenant has served a request (the shell is
  not warmed up by default), so open an admin page before the first wait.

## 2. Order, dependencies and snapshots

Record in this order; each clip starts from the state the previous one left.

| Order | Clip | Changes the site | Starts from | Restore before a new take |
|---|---|---|---|---|
| 1 | `02-features` | enables two features | fresh site | snapshot A |
| 2 | `02-parts` | attaches two parts to the Article type | `02-features` done | snapshot A2 |
| 3 | `03-publish` | creates and publishes the article Spring Sale Announcement | `02-parts` done | snapshot B |
| 4 | `04-archive` | schedules the archive, then unpublishes the article | `03-publish` done (the article is Published, no schedule) | snapshot C |
| 5 | `05-cancel` | edits the two seed items | `04-archive` done, then the seed data | snapshot D |

Snapshots (stop the site, copy `App_Data` and `state.json` aside, start the site again):

- **A:** after Auto Setup and `Recorder setup`, before any clip.
- **A2:** after `02-features`. **B:** after `02-parts`. **C:** after `03-publish`.
- **D:** after `04-archive` and the seed data of the section "Seed data for clip `05-cancel`".

The slides and the cards need no site. The clips `03-publish` and `04-archive` wait for the background task, so a
full take of the two takes about ten minutes of real time.

## 3. Values used in the clips

- **T3** is "the site's local time in three minutes", typed in the date and time fields. Compute it at the moment of
  the step (not earlier): take the current UTC time, add 3 minutes, convert it to `America/Los_Angeles` with
  `TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles")`, set the seconds to zero, and format it as
  `yyyy-MM-ddTHH:mm` (for example `2026-11-12T14:35`). Type it with `FillAsync` on the field (the field shows it in the
  browser's format, such as 11/12/2026, 02:35 PM). Do not type the digits one by one: the segments of a date and time
  field take their keys in an order that depends on the browser.
- **The item row** (ROW) is the list entry that contains the title: `.list-group-item` filtered by `HasText` with the
  title. If the list markup differs, use the row element that contains the title and the buttons of the item.
- **The badges of a row:** status badges `.badge` with the text `Published` or `Draft`; the schedule badge of Publish
  Later holds `i.fa-clock`; the schedule badge of Archive Later holds `i.fa-box-archive`.
- **The controls of the editor** (all in the action bar fixed at the bottom of the editor):

  | Name | Selector |
  |---|---|
  | Publish Later field | `input[name$='.ScheduledPublishLocalDateTime']` |
  | Publish Later button | `button[name='submit.Save'][value='submit.PublishLater']` |
  | Cancel Publish Later button | `button[value='submit.CancelPublishLater']` |
  | Archive Later field | `input[name$='.ScheduledArchiveLocalDateTime']` |
  | Archive Later button | `button[name='submit.Publish'][value='submit.ArchiveLater']` |
  | Cancel Archive Later button | `button[value='submit.CancelArchiveLater']` |

- **Timing of a step:** the recorder holds each step until its narration ends, and the actions of a step must be
  shorter than the narration (about 2.5 words per second, plus the pauses between sentences). Use `pauseAfter: 300`
  on clicks. The budget of each step is given below.

## 4. The clips

### Clip `02-features` (chapter 02)

Page it starts on: `/Admin` (the dashboard), opened with `OpenAsync` before `StartCaptureAsync`. The left menu is
collapsed.

#### Step `features-1` (about 9 s)

Actions:

1. Click "Tools" in the left menu (`#left-nav`, the text "Tools"), which expands its entries.
2. `ClickAndWaitForNavigationAsync` on "Features" in the left menu. The page `/Admin/Features` opens.
3. `TypeAsync` into the search box `#search-box` the text `Publish Later`. The list is filtered in the page to one row,
   "Publish Later", in the category "Content Management".
4. `ClickAndWaitForNavigationAsync` on the "Enable" button of that row (`a.btn-primary` with the text "Enable"; it is
   the only visible one). Wrap the wait for the reloaded page in `OffCameraAsync`, with the condition that the text
   "The feature Publish Later was enabled." is visible: enabling a feature restarts the tenant and the reload can take
   several seconds.

On screen when the narration ends: the Features page with the message "The feature Publish Later was enabled." above
the list, and the search box empty again.

Labels: "Tools" and "Features" `OrchardCore.Features/AdminMenu.cs:26-27`; the search box
`OrchardCore.Features/Views/Admin/Features.cshtml:27`; "Enable"
`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the message
`OrchardCore.Features/Controllers/AdminController.cs:317`; the feature name "Publish Later"
`OrchardCore.PublishLater/Manifest.cs:4`.

#### Step `features-2` (about 9 s)

Actions:

1. `TypeAsync` into `#search-box` the text `Archive Later` (the box was emptied by the reload).
2. `ClickAndWaitForNavigationAsync` on the "Enable" button of the one visible row, with the same off-camera wait for the
   text "The feature Archive Later was enabled."

On screen when the narration ends: the Features page with the message "The feature Archive Later was enabled."

Labels: the feature name "Archive Later" `OrchardCore.ArchiveLater/Manifest.cs:4`; the message
`OrchardCore.Features/Controllers/AdminController.cs:317`.

### Clip `02-parts` (chapter 02)

Page it starts on: `/Admin`, opened with `OpenAsync` before `StartCaptureAsync`.

#### Step `parts-1` (about 9 s)

Actions:

1. Click "Design" in the left menu (`#left-nav`), then "Content Definition" (it expands its entries).
2. `ClickAndWaitForNavigationAsync` on "Content Types". The list of content types opens.
3. `ClickAndWaitForNavigationAsync` on the "Edit" button of the Article row: `a[role='btn-edit-Article']`.

On screen when the narration ends: the page "Edit Content Type - Article".

Labels: "Design", "Content Definition", "Content Types" `OrchardCore.ContentTypes/AdminMenu.cs:23-25` (the menu entry is
"Design", not "Content"); "Edit" `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`; the
page title `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`.

#### Step `parts-2` (about 12 s)

Actions:

1. Scroll the "Add Parts" button into view (it is under the list of parts) and `ClickAndWaitForNavigationAsync` on it
   (`a` with the text "Add Parts"). The page "Add Parts" opens, with the sentence "Choose the Parts to add to this
   Content Type."
2. Scroll to the check box labeled ArchiveLater and click it, then scroll to the check box labeled PublishLater and click it.
   The list is alphabetical. The code writes the label as the name of the part without the word Part and without a space,
   so the two labels read `ArchiveLater` and `PublishLater`, not "Archive Later" and "Publish Later" (the narration of
   `parts-2` spells them that way and says them with a space through the storyboard's `spoken` map; see `OPEN-QUESTIONS.md`,
   question 1). Each label is followed by the description of the part.
3. `ClickAndWaitForNavigationAsync` on "Add Selected Parts" (`#add-selected-parts-button`, hidden until a box is
   checked). The page "Edit Content Type - Article" opens again.
4. `HoldAsync(1.5)` on the message at the top, then scroll the list of parts (`#parts`) into view with `ScrollAsync`.

On screen when the narration ends: the "Parts" list of the Article type with two new rows, PublishLater and ArchiveLater, each
followed by the part's description ("Adds the ability to schedule content items to be
published at a given future date and time." and the archive equivalent). The message "The following parts have been
added: ArchiveLaterPart, PublishLaterPart." (the message lists the names of the parts, which end in Part) is visible at the top
of the page just before the scroll.

Labels: "Add Parts" `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:201`; "Parts"
`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:144`; the check box labels
`OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:33` with the name from
`OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:19` and
`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:60`;
"Add Selected Parts" `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:47`; the message
`OrchardCore.ContentTypes/Controllers/AdminController.cs:372`; the descriptions
`OrchardCore.PublishLater/Migrations.cs:22` and `OrchardCore.ArchiveLater/Migrations.cs:23`.

### Clip `03-publish` (chapter 03)

Page it starts on: `/Admin`, opened with `OpenAsync` before `StartCaptureAsync`. Starts from snapshot B. Nothing is in the
Archive Later field during this clip.

#### Step `publish-1` (about 8 s)

Actions:

1. Click "Content" in the left menu (`#left-nav`), then `ClickAndWaitForNavigationAsync` on "Content Items".
2. Click the "New" button (`#new-dropdown`), then `ClickAndWaitForNavigationAsync` on "Article" in the open menu
   (`.dropdown-menu a`, text "Article").
3. `TypeAsync` into the field labeled "Title" (`GetByLabel("Title", Exact)`) the text `Spring Sale Announcement`.
   Leave every other field as it is.

On screen when the narration ends: the editor of a new Article (page `Contents/ContentTypes/Article/Create`) with the title
"Spring Sale Announcement" typed, and the action bar along the bottom of the window.

Labels: "Content" and "Content Items" `OrchardCore.Contents/AdminMenu.cs:65,68`; "New" and the type names
`OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:19-25`; "Title" `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`.

#### Step `publish-2` (about 11 s)

Actions:

1. `HighlightAsync` on the group that holds the Publish Later field and button
   (`.action-bar-bottom .input-group` that contains `input[name$='.ScheduledPublishLocalDateTime']`), without a caption.
2. Click the Publish Later field, and `FillAsync` it with T3.
3. `HoldAsync` until the narration ends.

On screen when the narration ends: the ringed Publish Later field showing the time three minutes ahead, with the "Publish
Later" button beside it. The Archive Later field and its "Archive Later" button are visible and empty, not ringed.

Labels: "Publish Later" and the field `OrchardCore.PublishLater/Views/PublishLaterPart.Edit.cshtml:11-12`; the fixed bar
`src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_action-bar.scss:14`. The field has no label of its
own.

#### Step `publish-3` (about 6 s)

Actions:

1. `ClearHighlightsAsync()`.
2. `ClickAndWaitForNavigationAsync` on the Publish Later button. The editor was opened from the list with a return address, so
   the browser returns to the Content Items list (`/Admin/Contents/ContentItems`).

On screen when the narration ends: the Content Items list with the message "Your Article draft has been saved." and the
row "Spring Sale Announcement" at the top.

Labels: the button `OrchardCore.PublishLater/Views/PublishLaterPart.Edit.cshtml:12` (it posts `submit.Save`, which saves a
draft: `OrchardCore.Contents/Controllers/AdminController.cs:424-440`); the message
`OrchardCore.Contents/Controllers/AdminController.cs:439`.

#### Step `publish-4` (about 11 s)

Actions:

1. `HighlightAsync` on the "Draft" badge of the row (`.badge` with the text "Draft").
2. After about 3 seconds, `ClearHighlightsAsync()`, then `MoveToAsync` the clock badge of the row (`i.fa-clock`) and
   keep the cursor there. The tooltip of the badge appears.

On screen when the narration ends: the row of the article with the "Draft" badge (and no "Published" badge), the clock badge
with a text such as "in 3 minutes", and the tooltip "Scheduled to be published on" followed by a date and a time.

Labels: "Draft" `OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:19`; the clock badge and the tooltip
`OrchardCore.PublishLater/Views/PublishLaterPart.SummaryAdmin.cshtml:10-11`.

#### Step `publish-5` (about 13 s)

Actions:

1. `OffCameraAsync`: poll with a `fetch` from the page, every 5 seconds, the address `/spring-sale-announcement` (the
   address the Autoroute pattern of the Article type builds from the title; `OPEN-QUESTIONS.md`, question 3), until the
   response status is 200. Give up after 6 minutes. The task runs every minute, so with T3 the wait is about 3 to 5 minutes.
   If the address is wrong, poll the list page `/Admin/Contents/ContentItems` instead, until the row of the article has the
   text "Published" and no text "Draft".
2. `WaitForNavigationAsync` with a reload of the list page, on camera.
3. `HighlightAsync` on the "Published" badge of the row.

On screen when the narration ends: the Content Items list, reloaded, with the row "Spring Sale Announcement" showing only the
"Published" badge: no "Draft" badge, no clock badge, no message.

Labels: "Published" `OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:12`; the task
`OrchardCore.PublishLater/Services/ScheduledPublishingBackgroundTask.cs:12-15,31`.

#### Step `publish-6` (about 4 s)

Actions:

1. `ClearHighlightsAsync()`, then `NavigateAsync("/spring-sale-announcement")`.

On screen when the narration ends: the public page of the article, with the title "Spring Sale Announcement". The page
is the Blog theme's page for an Article; nothing else is expected on it.

Source: `OrchardCore.Contents/Controllers/ItemController.cs` (the public page shows the published version only).

### Clip `04-archive` (chapter 04)

Page it starts on: `/Admin/Contents/ContentItems`, opened with `OpenAsync` before `StartCaptureAsync`. Starts from snapshot C: the
article is Published and has no schedule.

#### Step `archive-1` (about 11 s)

Actions:

1. `ClickAndWaitForNavigationAsync` on the title link "Spring Sale Announcement" of the row (`a` with that text). The
   editor of the published article opens.
2. `HighlightAsync` on the group that holds the Archive Later field and button
   (`.action-bar-bottom .input-group` that contains `input[name$='.ScheduledArchiveLocalDateTime']`).

On screen when the narration ends: the editor of the article with the ringed Archive Later field and "Archive Later" button
in the action bar. The "Unpublish" button is also in the bar, because the article is published.

Labels: "Archive Later" and the field `OrchardCore.ArchiveLater/Views/ArchiveLaterPart.Edit.cshtml:11-12`; "Unpublish"
`OrchardCore.Contents/Views/Content.UnpublishButton.cshtml:7,19` (line 19 is the button when the editor was opened from the
list with a return address, as here; shown only when the item is published,
`OrchardCore.Contents/Drivers/ContentsDriver.cs:112`).

#### Step `archive-2` (about 13 s)

Actions:

1. `ClearHighlightsAsync()`.
2. Click the Archive Later field, and `FillAsync` it with T3 (computed at that moment). The Publish Later field stays empty.
3. `ClickAndWaitForNavigationAsync` on the Archive Later button. The button posts `submit.Publish`, so the item is
   published and the browser returns to the list.

On screen when the narration ends: the Content Items list with the message "Your Article has been published." and the row
"Spring Sale Announcement" at the top.

Labels: the button `OrchardCore.ArchiveLater/Views/ArchiveLaterPart.Edit.cshtml:12`; it posts `submit.Publish`
(`OrchardCore.Contents/Controllers/AdminController.cs:546-551`, `:938-980`); the message
`OrchardCore.Contents/Controllers/AdminController.cs:967`.

#### Step `archive-3` (about 10 s)

Actions:

1. `HighlightAsync` on the "Published" badge of the row, for about 3 seconds, then `ClearHighlightsAsync()`.
2. `MoveToAsync` the archive badge of the row (`i.fa-box-archive`) and keep the cursor there. The tooltip appears.

On screen when the narration ends: the row with the "Published" badge, the archive-box badge with a text such as "in 3
minutes", and the tooltip "Scheduled to be archived on" followed by a date and a time. No "Draft" badge.

Labels: `OrchardCore.ArchiveLater/Views/ArchiveLaterPart.SummaryAdmin.cshtml:10-11`;
`OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:12`.

#### Step `archive-4` (about 13 s)

Actions:

1. `OffCameraAsync`: poll with a `fetch` every 5 seconds the address `/spring-sale-announcement` until the status is 404.
   Give up after 6 minutes. If the address is wrong, poll the list page until the row of the article no longer has the
   text "Published".
2. `WaitForNavigationAsync` with a reload of the list page, on camera.
3. `HighlightAsync` on the "Draft" badge of the row.

On screen when the narration ends: the reloaded list with the row "Spring Sale Announcement" showing the "Draft" badge only: no
"Published" badge, no archive-box badge.

Labels: "Draft" `OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:19`; the task
`OrchardCore.ArchiveLater/Services/ScheduledArchivingBackgroundTask.cs:13-16,32,57`.

### Seed data for clip `05-cancel`

Create these two items outside the capture, with the `admin` account, after `04-archive` is recorded, using the editor of
the Article type (Content, Content Items, New, Article). Compute the dates in the site time zone (the section "Values
used in the clips").

1. **Holiday Hours:** Title `Holiday Hours`. In the Publish Later field type the date 30 days ahead at 09:00 (format
   `yyyy-MM-ddT09:00`), then click "Publish Later". The item is a draft with a schedule.
2. **Summer Campaign:** Title `Summer Campaign`. In the Archive Later field type the date 30 days ahead at 17:00 (format
   `yyyy-MM-ddT17:00`), then click "Archive Later". The item is published with a schedule.

Then take snapshot D. Both items must show their schedule badge in the list: a clock badge on Holiday Hours, an
archive-box badge on Summer Campaign.

### Clip `05-cancel` (chapter 05)

Page it starts on: `/Admin/Contents/ContentItems`, opened with `OpenAsync` before `StartCaptureAsync`. Starts from snapshot D.

#### Step `cancel-1` (about 11 s)

Actions:

1. `ClickAndWaitForNavigationAsync` on the title link "Holiday Hours". The editor opens, with the stored time in the Publish
   Later field and the "Cancel Publish Later" button beside the Publish Later group.
2. `HighlightAsync` on the "Cancel Publish Later" button for about 2 seconds, then `ClearHighlightsAsync()`.
3. `FillAsync` the Publish Later field with the date 45 days ahead at 09:00 (format `yyyy-MM-ddT09:00`).
4. `ClickAndWaitForNavigationAsync` on the "Publish Later" button. The browser returns to the list.

On screen when the narration ends: the Content Items list with the message "Your Article draft has been saved." and the row
"Holiday Hours" with the "Draft" badge and a clock badge with a later text than before (the text is computed from the
distance in days: about "in 4 weeks" for the seed and about "in 6 weeks" after this step,
`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/DateTimeShapes.cs:46-69,198-206`).

Labels: "Cancel Publish Later" `OrchardCore.PublishLater/Views/PublishLaterPart.Edit.cshtml:19` (shown only when a schedule
exists, `:16`); the message `OrchardCore.Contents/Controllers/AdminController.cs:539`.

#### Step `cancel-2` (about 9 s)

Actions:

1. `ClickAndWaitForNavigationAsync` on the title link "Holiday Hours".
2. `ClickAndWaitForNavigationAsync` on the "Cancel Publish Later" button. The browser returns to the list.

On screen when the narration ends: the list with the message "Your Article draft has been saved." and the row "Holiday Hours"
with the "Draft" badge only: no clock badge.

Labels: `OrchardCore.PublishLater/Views/PublishLaterPart.Edit.cshtml:19`; the driver clears the schedule when this button is
used, `OrchardCore.PublishLater/Drivers/PublishLaterPartDisplayDriver.cs:54-56`.

#### Step `cancel-3` (about 10 s)

Actions:

1. `ClickAndWaitForNavigationAsync` on the title link "Summer Campaign". The editor shows the "Cancel Archive Later" button.
2. `HighlightAsync` on the "Cancel Archive Later" button for about 2 seconds, then `ClearHighlightsAsync()`.
3. `ClickAndWaitForNavigationAsync` on the "Cancel Archive Later" button. The browser returns to the list.

On screen when the narration ends: the list with the message "Your Article has been published." and the row "Summer Campaign"
with the "Published" badge only: no archive-box badge.

Labels: "Cancel Archive Later" `OrchardCore.ArchiveLater/Views/ArchiveLaterPart.Edit.cshtml:19`; it posts `submit.Publish`, so
the item is published (`OrchardCore.Contents/Controllers/AdminController.cs:546-551`); the driver clears the schedule,
`OrchardCore.ArchiveLater/Drivers/ArchiveLaterPartDisplayDriver.cs:50-52`.

## 5. Slides and cards

- `intro`: the opening card (chapter 00); `assemble.py` draws it from the storyboard.
- `idea-1`: a slide headed "Two scheduled changes" with two cards: Publish Later (a draft is published at the chosen date and
  time) and Archive Later (a published item is unpublished at the chosen date and time).
- `idea-2`: a slide headed "One part and one task for each" with four cards: a content part, a date and time, a background
  task, a change of state.
- `know-1`: a slide headed "Which time zone applies" with three cards: the site (Settings, General, Default Time Zone), the user
  (the User Time Zone feature), and the time stored as UTC.
- `know-2`: a slide headed "Permissions and timing" with three cards: who sees the controls, when the change happens, where
  the task runs.
- `outro`: the closing card (chapter 99).

## 6. Checks for private data

- Every name on screen is fake: the account `admin`, the titles Spring Sale Announcement, Holiday Hours and Summer Campaign,
  and the texts of the Blog recipe. No email address appears on screen (the profile is never opened).
- The password is never typed on camera (Auto Setup creates the account and `state.json` signs in), and it is not written in
  any file of the repository.
- The address bar is not part of the capture; the site name is "Orchard Core".
- Look at every contact sheet from `review.py`: no error message, no empty list, no real name.
- OCR the finished video (one frame per second, every scene change, then five frames per second) and search it for real names,
  email addresses other than `example.com`, tokens and the machine name.

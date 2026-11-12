# Recording plan: Admin Dashboard

This plan lets a person record the video without deciding anything. It is for the recorder, so it holds selectors,
scene calls, file:line references into the Orchard Core source and shell commands, which the video itself never shows.
A selector written here stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. The
`file:line` references are paths under `src/` of `OrchardCMS/OrchardCore` at commit `92db4a83`, where every label was
read.

The video has three recordings (`02-enable`, `03-add`, `04-arrange`), three groups of slides (`01-how`, `05-who`,
`06-types`) and two cards. Slides and cards are drawn from `slides.py` and the storyboard: nobody records them.

## The site it needs

- **Source**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout that contains the Admin Dashboard
  module (`src/OrchardCore.Modules/OrchardCore.AdminDashboard`), run on `http://localhost:5010` with its own
  `ORCHARD_APP_DATA` folder outside both repositories.
- **Provisioning**: Auto Setup, so no setup form and no sign-in form are recorded. The host project registers it
  (`OrchardCore.Cms.Web/Program.cs`, `AddSetupFeatures("OrchardCore.AutoSetup")`). Environment variables
  (`OrchardCore.AutoSetup` documentation, "User Secrets and Environment Variables"):

  ```
  OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  OrchardCore__AutoSetup__Tenants__0__SiteName=Orchard Core
  OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
  OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

  `<password>` is the password chosen outside git, the same value as `VIDEO_PASSWORD`. It is never written in the
  repository and never shown on screen.
- **Recipe**: Blog (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). It enables Admin, Features, Roles,
  Users, Themes, Contents, Content Types, Html, Title and Markdown, among others, and creates the roles Moderator,
  Editor, Author and Contributor. It does **not** enable Admin Dashboard.
- **Tenant**: the Default tenant. Site name `Orchard Core`, time zone `America/Los_Angeles`.
- **Features to enable before recording**: none. Admin Dashboard must be **disabled** when `02-enable` starts: that
  clip enables it. Check on `/Admin/Features` that the Admin Dashboard row shows Disabled, and on
  `/Admin/ContentTypes/List` that no content type named Html Dashboard Widget exists (the feature's data migration
  creates it: `OrchardCore.AdminDashboard/Migrations.cs:23-38`, `Migrations/dashboard-widgets.recipe.json:8`).
- **Settings**: none changed. Keep the defaults of the Admin settings (breadcrumb shown, light admin theme, page
  titles shown in the page and not in the top bar: `DisplayTitlesInTopbar` is off by default,
  `OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:20`).
- **Admin menu**: the Blog recipe adds its own "Admin menus" entries (Blog, Main Menu, Content) to the side menu.
  They are expected on screen.
- **External services**: none. Start the site with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so it never
  calls a mail, SMS or other provider. Do not run the module's `dashboard-widgets-samples` recipe: its widget shows
  links to GitHub and Discord.
- **Browser**: the recorder's Chromium at 1536x864 CSS pixels, device scale 1.25, light color scheme, `en-US`. The
  width matters: the dashboard grid has six columns from 1400 px up (`OrchardCore.AdminDashboard/Assets/scss/dashboard.scss:39-44`),
  and the narration says "six columns" on a wide screen.
- **Sign-in**: once, outside the capture, with `Recorder setup` (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`),
  which saves `state.json` (git-ignored).

## Seed data

Only the widgets below are created outside the capture. They use the Add Widget form, exactly as `03-add` shows it,
after `03-add` is recorded (see the order below). All text is invented.

| Title | Body text | Position | Width | Height | Button |
|---|---|---|---|---|---|
| Welcome, editors (made on camera in `03-add`) | Welcome to the editorial desk. Draft in your own section, and ask a colleague to review before you publish. | 0 | 2 | 1 | Publish |
| Style reminders | Use sentence case in headlines. Add alt text to every image. | 1 | 2 | 1 | Publish |
| Office hours | The editorial desk answers questions on weekdays from 9:00 to 17:00. | 2 | 1 | 1 | Publish |
| Holiday schedule | Share your days off with the desk by the end of the month. | 3 | 1 | 1 | Save Draft |

To create one: open `/Admin/dashboard/manage`, select Add Widget, then Html Dashboard Widget, fill in the Title, the
body and the Position, Width and Height fields, and select the button of the table. "Holiday schedule" is saved as a
draft and never published: it must be absent from `/Admin` and present in `/Admin/dashboard/manage`.

After seeding, `/Admin/dashboard/manage` at 1536 px shows the four widgets in one row (2 + 2 + 1 + 1 columns) in the
order "Welcome, editors", "Style reminders", "Office hours", "Holiday schedule". `/Admin` shows the first three only.

## Order, dependencies and snapshots

Record in this order. Each recording starts from the state the previous one left.

1. **Snapshot S0**: after Auto Setup and `Recorder setup`, Admin Dashboard disabled. Stop the site and copy its
   `App_Data` folder and `state.json` aside.
2. **`02-enable`**. Changes the site: enables Admin Dashboard (its data migration creates the Html Dashboard Widget type, and
   the role updater adds the default dashboard permission to the roles named Editor, Moderator, Author and
   Contributor: `OrchardCore.Roles/Services/RoleUpdater.cs:55-94`. The Administrator role gets no claim there
   (`RoleUpdater.cs:206-210`): it holds every permission as the system administrator role,
   `OrchardCore.Settings/Services/SuperUserHandler.cs:33-40`).
3. **Snapshot S1**: stop the site, copy `App_Data` and `state.json` aside.
4. **`03-add`**. Changes the site: creates the widget "Welcome, editors".
5. **Seed** the three other widgets of the table, off camera.
6. **Snapshot S2**: stop the site, copy `App_Data` and `state.json` aside.
7. **`04-arrange`**. Changes the site: the order and sizes of the widgets, and the widgets' stored positions.

Retakes: `02-enable` restores S0; `03-add` restores S1 (and then the seed and S2 are made again); `04-arrange`
restores S2. To restore: stop the site by its process ID, replace `App_Data` and `state.json` with the copy, start the
site again.

`03-add` and `04-arrange` depend on `02-enable` (the feature) and `04-arrange` depends on `03-add` and the seed (the
widgets it moves).

## Before each recording of a dashboard

The dashboard's resize handles measure a grid cell at 240 px and recalculate it only when the window is resized, not
when the page loads (`OrchardCore.AdminDashboard/Assets/ts/dashboard.ts:56-70`, `:237`). Before `StartCaptureAsync` of
`04-arrange`, run `scene.Page.EvaluateAsync("() => window.dispatchEvent(new Event('resize'))")` so a drag snaps to the
real cell width.

## Clip `02-enable`

Starts on `/Admin` (opened before the capture, not recorded). The page is the default home page of the admin.

- `enable-1`: the page shows a message headed "Welcome to Orchard Core" and four cards. Move the cursor over
  the card titles in the order Create content, Customize theme, Manage users, Learn more, one after the other
  (`MoveToAsync` on each `h5.card-title`). Highlight the heading "Welcome to Orchard Core" first and clear the
  highlight before moving on. **At the end of the narration**: the whole page is visible with the cursor on the Learn
  more card, and no highlight. Labels: `OrchardCore.Admin/Views/AdminDashboard.cshtml:2`,
  `OrchardCore.Contents/Views/AdminDashboardContents.cshtml:4`, `OrchardCore.Themes/Views/AdminDashboardThemes.cshtml:4`,
  `OrchardCore.Users/Views/AdminDashboardUsers.cshtml:4`, `OrchardCore.Admin/Views/AdminDashboardDocumentation.cshtml:4`.
  The cards come in this order because of their positions 10, 20, 30 and 40 in the `AdminDashboardShapeTableProvider.cs`
  files of the four modules.
- `enable-2`: click Tools in the side menu, then Features (`ClickAndWaitForNavigationAsync` on the link whose href ends
  with `/Admin/Features`), then `TypeAsync` the text `Admin Dashboard` in `#search-box`. Wait until the summary text of
  `#list-summary` shows that the list is filtered and the row of Admin Dashboard is visible under the group heading
  Content Management. **At the end of the narration**: the Features page with the search box holding "Admin
  Dashboard", the group heading Content Management, and the row of Admin Dashboard with the Disabled badge and the
  Enable button. Labels: `OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features),
  `OrchardCore.Features/Views/Admin/Features.cshtml:27` (Search), `OrchardCore.AdminDashboard/Manifest.cs:4` (Name),
  `:16` (Category). If the filter leaves rows of other features, note them: see OPEN-QUESTIONS.md.
- `enable-3`: `ClickAndWaitForNavigationAsync` on the Enable link of the Admin Dashboard row
  (`a[id^="btn-enable-"][href*="OrchardCore.AdminDashboard"]`). Enabling reloads the site's features: wait for the
  message inside `scene.OffCameraAsync`, with the condition that an element containing the text "was enabled" is
  visible. **At the end of the narration**: the success message "The feature Admin Dashboard was enabled." is visible
  above the list of features. Label: `OrchardCore.Features/Controllers/AdminController.cs:317`; button:
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`. If the layout of the lists shows the row
  actions in a menu, open the Actions menu of the row first.
- `enable-4`: `ClickAndWaitForNavigationAsync` on the site name in the top bar, right of the "Show / hide menu" button
  (`a.ta-navbar-brand`, text "Orchard Core"; it is not in the side menu), which goes to `/Admin`. The first request after enabling runs the feature's migration, so wait
  inside `scene.OffCameraAsync` until an `h1` with the text "Dashboard" is visible. **At the end of the narration**:
  the page is titled Dashboard (`h1`), the "Manage Dashboard" button is at the top right (`a.btn.btn-primary.btn-sm`),
  and a yellow alert says "Click on Manage Dashboard button to add dashboard widget.". Labels:
  `OrchardCore.Admin/Views/AdminBranding.cshtml:5` (placed in the top bar by
  `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:59`), `OrchardCore.AdminDashboard/Views/Dashboard/Index.cshtml:28`,
  `:35`, `:50`.

## Clip `03-add`

Starts on `/Admin` (opened before the capture), from snapshot S1: the dashboard is empty and shows the alert.

- `add-1`: `ClickAndWaitForNavigationAsync` on the "Manage Dashboard" button, which opens `/Admin/dashboard/manage`.
  Highlight the buttons "Close Editor" (`a.btn.btn-success`) and "Add Widget" (`button#new-dropdown`) one after the
  other, with no caption, and clear the highlights. **At the end of the narration**: the editor page, titled
  Dashboard, with an empty grid and the two buttons at the top right. Labels:
  `OrchardCore.AdminDashboard/Views/Dashboard/Manage.cshtml:23` (Close Editor), `:29` (Add Widget);
  `Views/Dashboard/Index.cshtml:28`.
- `add-2`: click `button#new-dropdown` (Add Widget); the menu lists one item, "Html Dashboard Widget"
  (`a.add-list-widget`). Highlight it, then `ClickAndWaitForNavigationAsync` on it. It opens
  `/Admin/Contents/ContentTypes/HtmlDashboardWidget/Create` with a `returnUrl` that points back to the editor.
  **At the end of the narration**: the creation form is visible, titled "New Html Dashboard Widget", with the Title
  field first. Labels: `OrchardCore.AdminDashboard/Migrations/dashboard-widgets.recipe.json:8` (display name),
  `OrchardCore.Contents/Views/Admin/Create.cshtml:26` (the "New {0}" title), `Views/Dashboard/Manage.cshtml:34`.
- `add-3`: `TypeAsync` the text `Welcome, editors` in the Title field (label "Title"), then `TypeAsync` the body text
  of the seed table in the body editor, which is the editable area of the rich-text editor
  (`div.trumbowyg-editor` inside `.html-body-part-wysiwyg-editor`). **At the end of the narration**: the Title and
  the body hold the typed text. Labels: `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`;
  `OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml:21`; the editor is chosen by
  `Migrations/dashboard-widgets.recipe.json` (`"Editor": "Wysiwyg"`).
- `add-4`: scroll down to the three fields Position, Width and Height (`ScrollAsync`, with no highlight on screen).
  Replace the default values: click each field, press Ctrl+A
  (`scene.Page.Keyboard.PressAsync("Control+A")`) to select its value, then `TypeAsync` `0` in Position, `2` in Width
  and `1` in Height, pressing Tab after each (a text box applies its value on blur). Highlight the three fields together, then clear the highlights.
  **At the end of the narration**: Position shows 0, Width shows 2 and Height shows 1. Labels:
  `OrchardCore.AdminDashboard/Views/DashboardPart.Edit.cshtml:4`, `:12`, `:20`.
- `add-5`: highlight "Save Draft" (`button.draft`) for a moment, clear it, then `ClickAndWaitForNavigationAsync` on
  Publish (`button.publish`; it is the first button of a split button, because the page has a return address). The
  site returns to `/Admin/dashboard/manage`. **At the end of the narration**: the editor shows the widget "Welcome,
  editors", two columns wide, with its body text, and the success message "Your Html Dashboard Widget has been
  published." is visible. Labels: `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`,
  `OrchardCore.Contents/Views/Content.SaveDraftButton.cshtml:19`, `OrchardCore.Contents/Controllers/AdminController.cs:473`;
  the return to the editor is `AdminController.cs:795-797`. If the message disappears before the end of the narration,
  take it as is: the contact sheet only needs the widget on screen.

After this clip, create the seed widgets, then snapshot S2.

## Clip `04-arrange`

Starts on `/Admin/dashboard/manage` (opened before the capture), from snapshot S2: four widgets in one row. Run the
resize event first (see "Before each recording of a dashboard"). In each step, do not click Undo, Delete, Unpublish,
Publish Draft or Discard Draft: the clip only opens menus.

- `arrange-1`: `DragToAsync` from the drag handle of Office hours
  (`.dashboard-wrapper:has-text("Office hours") .dashboard-handle`) to the left half of the widget "Welcome, editors"
  (`.dashboard-wrapper:has-text("Welcome, editors")`, offset 20 px from its left edge and 60 px from its top). The
  dashboard moves the widget in place with a 150 ms animation. **At the end of the narration**: the order is "Office
  hours", "Welcome, editors", "Style reminders", "Holiday schedule", all in one row. Source:
  `OrchardCore.AdminDashboard/Views/Dashboard.DragHandle.cshtml:1`, `Assets/ts/dashboard.ts:121-141`,
  `Drivers/DashboardContentDisplayDriver.cs:46-50`.
- `arrange-2`: wait, with the condition that `#dashboard-undo-message` no longer has the class `d-none`. Highlight the
  link "Undo" (`#dashboard-undo-message a`). **At the end of the narration**: the green message "Dashboard
  successfully updated. Undo." is visible above the grid, with a highlight ring around Undo. Labels:
  `Views/Dashboard/Manage.cshtml:54`; behavior: `Assets/ts/dashboard.ts:94-113`.
- `arrange-3`: clear the highlight. `DragAsync` the bottom right corner handle of Style reminders
  (`.dashboard-wrapper:has-text("Style reminders") .dashboard-resize-handle-se`) by 60 px to the right and 120 px down.
  A dashed orange outline follows the drag, then the widget snaps. The result must be a widget three columns wide and
  two rows high; if the layout at the recording size gives another size, adjust the two offsets until it does (a
  growth snaps up to the next whole cell: `Assets/ts/dashboard.ts:26-39`). **At the end of the narration**: Style
  reminders is wider and taller, Office hours and Welcome, editors stay in the first row, and Holiday schedule sits in
  the second row. Source: `Assets/ts/dashboard.ts:24`, `:145-196`; `Assets/scss/dashboard.scss:106-129`.
- `arrange-4`: no action. Hold the frame (`HoldAsync`). **At the end of the narration**: the same screen, with the
  message "Dashboard successfully updated." still visible.
- `arrange-5`: click the Actions button (the three dots, `div.btn-group[title="Actions"] > button`) in the header of
  "Welcome, editors". **At the end of the narration**: the menu is open and lists Edit, Delete and Unpublish, in this
  order. Source: `Views/DashboardWidget.DetailAdmin.cshtml:9-14`, `Views/Dashboard.EditButton.cshtml:2`,
  `Views/Dashboard.DeleteButton.cshtml:4`, `Views/Dashboard.UnpublishButton.cshtml:4`,
  `Drivers/DashboardContentDisplayDriver.cs:42-44`, `:70`.
- `arrange-6`: press Escape to close the menu, scroll so the Holiday schedule widget is visible if needed (clear
  highlights first), then click its Actions button. **At the end of the narration**: the menu is open and lists Edit,
  Delete, Publish Draft and Discard Draft, with no Unpublish. Source: `Views/Dashboard.PublishButton.cshtml:3`,
  `Views/Dashboard.DiscardDraftButton.cshtml:5`, `Drivers/DashboardContentDisplayDriver.cs:79`, `:88`.
- `arrange-7`: press Escape to close the menu, scroll to the top, then `ClickAndWaitForNavigationAsync` on "Close
  Editor". **At the end of the narration**: `/Admin` shows the title Dashboard and the "Manage Dashboard" button, the
  three published widgets in the order "Office hours", "Welcome, editors", "Style reminders" (the last one three columns wide
  and two rows high), no drag handles, no Actions buttons, and no Holiday schedule. Source:
  `Views/Dashboard/Manage.cshtml:23`, `Controllers/DashboardController.cs:59` (published widgets only),
  `:114` (the editor lists the latest versions).

## Slides

No recording. Each slide step is drawn by `slides.py`.

- `how-1`: heading "A start page made of widgets", three cards, the first bright: "A widget is a content item".
- `how-2`: the same slide, the second card bright: "A grid sets the layout".
- `how-3`: the same slide, the third card bright: "Only published widgets show".
- `who-1`: heading "Two permissions", two cards: "Access the Admin Dashboard" and "Manage the Admin Dashboard".
- `who-2`: heading "Who has them by default", three cards: Administrator, Editor, Moderator, Author, Contributor,
  and "Access Control, Roles".
- `who-3`: heading "A widget also needs view permission", three cards, the last two bright.
- `types-1`: heading "A widget type is a content type", four cards, the first two bright (create the type, set its
  Stereotype).
- `types-2`: the same slide, the last two bright (add the parts, it joins Add Widget).

The cards `intro` and `outro` are drawn from the storyboard.

## Checks for private data

- The only account is `admin` with the email `admin@example.com`. Every other name on screen is invented (the widget
  titles and texts of the seed table). No other email, phone number or token appears.
- The password is never typed on camera and never shown: the sign-in comes from `state.json`.
- The site is a fresh Auto Setup site, so no mask script is needed. After recording, look at every contact sheet
  (`review.py`) for an error message, an empty list, or a name that is not in this plan.
- Before publishing, OCR the video at one frame per second and at every scene change, and search for real names,
  email addresses and links other than `docs.orchardcore.net` and `orchardcore.net`. The home page of the admin shows a
  Documentation button that links to `https://docs.orchardcore.net`.

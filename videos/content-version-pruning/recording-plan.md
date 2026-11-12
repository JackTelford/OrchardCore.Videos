# Recording plan: Content Version Pruning

A person who reads only this file can record the video. The narration is in `storyboard.json`; narrate first
(`narrate.py`), because the recorder holds each step for the length of its narration. Locators below are written
as the recorder's `scene.Page.Locator(...)` calls take them; they come from the Orchard Core source, and a
`Recorder probe` of the page confirms them before a take. Every label is spelled as the code has it, with the
file and line of the Orchard Core source in `src/OrchardCore.Modules/` (commit 92db4a83) after the label.

Credentials: the recording signs in as the user `admin` (admin@example.com) with the password `<password>`, a value
chosen outside git and passed in `VIDEO_PASSWORD`. Never write it in a file of the repository.

## The site it needs

- **Application**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout at commit 92db4a83 or a later
  commit that keeps the labels listed in this plan. Run it in the Development environment on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with its own `ORCHARD_APP_DATA` folder outside both repositories. The `orchardcore-tester`
  skill of that repository describes running it in the background.
- **Auto Setup, no setup form**: set these environment variables before the first start, with the password read from
  `VIDEO_PASSWORD`:

  ```bash
  export ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5010
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

- **Time zone**: `America/Los_Angeles`. The task runs on its schedule in the time zone of the site
  (`src/OrchardCore/OrchardCore/BackgroundTasks/BackgroundTaskScheduler.cs`, `CanRun` and `GetNextStartTime`), which
  is why the tooltip of the Next run badge of `task-2` holds a date and time in that zone (the text of the badge is a
  relative time, such as "in 7 hours").
- **No outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (for example `http://127.0.0.1:9`).
  The video needs no external service, mail server or emulator.
- **Features to enable before recording**: none. The Blog recipe enables neither Content Version Pruning nor
  Background Tasks (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step), and the
  video enables both on camera. Before the first take, open Tools, Features on the fresh site and check that both are
  disabled. Without the Background Tasks feature there is no Tools, Background Tasks page, and the pruning task never
  runs on its schedule, because its default is disabled (`VersionPruning/Tasks/ContentVersionPruningBackgroundTask.cs:15`).
- **Settings to change before recording**: none. Leave the Admin settings at their defaults, so the admin lists use the
  List layout and the Buttons layout of the row actions (`OrchardCore.Admin.Abstractions/AdminListOptions.cs`): a task
  row then shows the buttons Edit and Enable (or Disable), and a drop-down button labeled Actions.
- **Fake seed data**: none to create. The video never opens a content item. The content of the Blog recipe (a blog,
  its posts, tags and categories, written by the recipe) is the only content on the site. Do not add real content.
- **Types the video selects**: Blog Post and Page, both created by the Blog recipe with Versionable on.

## Order of the clips and the snapshots

`01-how` is made of slides: nothing to record. The three clips depend on each other and change the site, so record
them in this order: `02-features`, `03-settings`, `04-task`.

| Snapshot | State of the site | Taken |
|---|---|---|
| S0 | Auto Setup done, `admin` signed in once with `Recorder setup` (`state.json`), Content Version Pruning and Background Tasks both disabled | Before `02-features`: stop the site, copy `ORCHARD_APP_DATA` aside |
| S1 | Both features enabled, settings never saved | After `02-features`: stop the site, copy aside |
| S2 | S1 and the pruning settings saved with Blog Post and Page selected | After `03-settings`: stop the site, copy aside |

- To record `02-features` again, restore S0. To record `03-settings` again, restore S1. To record `04-task` again,
  restore S2 (the clip enables the task and runs it, which changes the site).
- Restoring means: stop the site (by its process id), replace the `ORCHARD_APP_DATA` folder with the copy, start the
  site again, and keep the same `state.json`.
- `04-task` needs the background service to have loaded the task. After every start of the site, and after
  `02-features`, the service learns the tasks of the tenant on its next polling (one minute by default), and a task
  that it did not load yet shows the badge "Waiting for the background service to load this task"
  (`OrchardCore.BackgroundTasks/Views/BackgroundTaskEntry.Status.SummaryAdmin.cshtml:12`) and a disabled item
  "Run now". Before the capture of `04-task`, open `/Admin/BackgroundTasks` and reload it every 15 seconds, for at most
  3 minutes, until the row `[data-task-name="OrchardCore.Contents.VersionPruning.Tasks.ContentVersionPruningBackgroundTask"][data-task-status="Idle"]`
  exists. Do this outside the capture.

## Locators used by several clips

- Admin menu: `scene.Page.Locator("#left-nav")` (`TheAdmin/Views/Navigation-admin.cshtml`). Tools and Settings are
  groups without a page of their own, so each is a `button` of that menu:
  `scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = "Tools", Exact = true })`. Their pages are
  links in the group that opens: `scene.Page.Locator("#left-nav").GetByRole(AriaRole.Link, new() { Name = "Features", Exact = true })`.
  Wait for the link to be visible after opening the group (the group opens with an animation). If the menu has two
  buttons with the same name, use the one that is a direct child of the first level of the menu.
- Notifications are toasts in `div.toast-container`; a success toast stays on screen for 5 seconds
  (`OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`). Never make a toast the only proof of a step: the
  frame at the end of each narration shows a state of the page that lasts.

## 01-how (slides, no recording)

- `how-1`: the slide "Old versions pile up", three cards: "Publish a change", "That copy is an archived version",
  "Versionable content types".
- `how-2`: the slide "What pruning keeps and deletes", three cards: "Always kept", "Kept by your rules", "Deleted".
- `how-3`: the slide "Three steps", four cards: "Enable the features", "Choose the rules", "Enable the background
  task", "Pruning is permanent".

## 02-features

Starts on `/Admin` (opened before `StartCaptureAsync`), with the admin menu closed. Needs S0. Changes the site: it
enables two features (the tenant reloads after each one, and the first request after a reload takes a few seconds).

- `features-1`: Click the menu button Tools (`Tools`, `Settings/AdminMenu.cs:32`), then click the link Features
  (`Features`, `OrchardCore.Features/AdminMenu.cs:27`) with `ClickAndWaitForNavigationAsync`. On the Features page
  (`/Admin/Features`), type `pruning` into `scene.Page.Locator("#search-box")` with `TypeAsync` (placeholder "Search",
  `Features/Views/Admin/Features.cshtml:27`). The list filters as you type. If a row other than Content Version
  Pruning stays visible, type `version pruning` instead.
  On screen when the narration ends: the Features page with `pruning` in the search box and, under the heading
  "Content Management", the row "Content Version Pruning" with an Enable button
  (`Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the button has the primary style, not the green one of
  the Background Tasks page). Label: the feature name and category are in
  `OrchardCore.Contents/Manifest.cs:31-35`.
- `features-2`: Click the Enable button of that row (`scene.Page.Locator("a[id^='btn-enable-']:visible")`, there must
  be exactly one) with `ClickAndWaitForNavigationAsync`. Wait for the toast text "The feature Content Version
  Pruning was enabled." (`Features/Controllers/AdminController.cs:317`). The list reloads without the filter: type
  `pruning` into the search box again.
  On screen when the narration ends: the row "Content Version Pruning" with a red Disable button, and the search box
  holding `pruning`.
- `features-3`: Empty the search box (`await scene.Page.Locator("#search-box").FillAsync("")`, not visible), then
  type `Background Tasks` into it with `TypeAsync`. The row "Background Tasks" appears under the heading "Tasks"
  (`OrchardCore.BackgroundTasks/Manifest.cs:4,9`). Click the Enable button of that row only
  (`scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.GetByText("Background Tasks", new() { Exact = true }) }).Locator("a[id^='btn-enable-']")`;
  the search matches the name, the id and the description of a feature (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`),
  so another row can stay visible: scope the locator by the exact name of the row) with `ClickAndWaitForNavigationAsync`. Wait for the toast text "The feature Background Tasks was enabled."
  (`Features/Controllers/AdminController.cs:317`), then type `Background Tasks` into the search box again.
  On screen when the narration ends: the row "Background Tasks" with a red Disable button.

After the clip: stop the site and take snapshot S1.

## 03-settings

Starts on `/Admin`. Needs S1. Changes the site (saves the settings of the site).

- `settings-1`: Click the menu button Settings (`Settings/AdminMenu.cs:36`), then click the link Content Version
  Pruning (`VersionPruning/AdminMenu.cs:27`) with `ClickAndWaitForNavigationAsync`. The page is
  `/Admin/Settings/ContentVersionPruningSettings`.
  On screen when the narration ends: the page titled "Settings" (breadcrumb, `Settings/Views/Admin/Index.cshtml`)
  with the four settings: the Disable check box, Retention period (days), Archived versions to keep per item, and
  Content types to prune, with nothing selected. Labels: `OrchardCore.Contents/Views/ContentVersionPruningSettings_Edit.cshtml:7,14,23,32`.
  The narration names two permissions, neither shown on screen. "Manage Content Version Pruning settings"
  (`VersionPruning/ContentVersionPruningPermissions.cs:10`) is checked by the menu item (`VersionPruning/AdminMenu.cs:29`)
  and by the settings editor (`VersionPruning/Drivers/ContentVersionPruningSettingsDisplayDriver.cs:31-36`).
  "Manage settings" (`src/OrchardCore/OrchardCore.Settings.Core/SettingsPermissions.cs:8`) is checked by the page itself
  (`Settings/Controllers/AdminController.cs:53`, `ManageGroupSettings`, implied by `ManageSettings`): the module does not
  register a group permission with `AddSiteSettingsPermission` (`VersionPruning/Startup.cs`), as the Settings module
  does for its own groups (`Settings/Startup.cs:50-51`), so the narrower permission alone does not open the page.
  The Administrator role holds both by default (`VersionPruning/ContentVersionPruningPermissionProvider.cs:15-22`,
  `Settings/Permissions.cs`).
- `settings-2`: `HighlightAsync` the Disable check box with its hint text
  (`scene.Page.GetByLabel("Disable", new() { Exact = true })`; label "Disable", hint "Whether the pruning task is
  disabled.", `ContentVersionPruningSettings_Edit.cshtml:7-8`). Do not click it.
  On screen when the narration ends: the ring around the cleared Disable check box.
- `settings-3`: `ClearHighlightsAsync`, then `HighlightAsync` the number field
  `scene.Page.GetByLabel("Retention period (days)")` (label at `ContentVersionPruningSettings_Edit.cshtml:14`). It holds
  `30`, the default (`VersionPruning/Models/ContentVersionPruningSettings.cs`, `RetentionDays`). Do not type in it.
  On screen when the narration ends: the ring around Retention period (days) with the value 30.
- `settings-4`: `ClearHighlightsAsync`, then `HighlightAsync` the number field
  `scene.Page.GetByLabel("Archived versions to keep per item")` (label at `ContentVersionPruningSettings_Edit.cshtml:23`).
  It holds `1`, the default (`VersionsToKeep`). Do not type in it.
  On screen when the narration ends: the ring around Archived versions to keep per item with the value 1.
- `settings-5`: `ClearHighlightsAsync`. Under the label "Content types to prune"
  (`ContentVersionPruningSettings_Edit.cshtml:32`), click the check box Blog Post
  (`scene.Page.Locator("input[type=checkbox][value='BlogPost']")`), then the check box Page
  (`scene.Page.Locator("input[type=checkbox][value='Page']")`). Do not click "Select all"
  (`Views/Shared/Components/SelectContentTypes/Default.cshtml:10`). `HighlightAsync` the block of check boxes if it
  helps the viewer.
  On screen when the narration ends: Blog Post and Page checked, every other type and Select all unchecked.
- `settings-6`: `ClearHighlightsAsync`, then click the button Save (`scene.Page.Locator("button.save")`, label
  "Save", `Settings/Views/SiteSettings.SaveButton.cshtml:2`) with `ClickAndWaitForNavigationAsync`. The page reloads
  at the same address. Wait for the toast text "Site settings updated successfully."
  (`Settings/Controllers/AdminController.cs:119`).
  On screen when the narration ends: the reloaded page with Blog Post and Page still checked (the saved values); the
  toast may already be gone.

After the clip: stop the site and take snapshot S2.

## 04-task

Starts on `/Admin`. Needs S2 and the wait for the loaded task described under "Order of the clips". Changes the site
(enables the task and runs it once).

- `task-1`: Click the menu button Tools, then the link Background Tasks
  (`OrchardCore.BackgroundTasks/AdminMenu.cs:19`; the page title is "Background Tasks", `BackgroundTask/Index.cshtml:7`)
  with `ClickAndWaitForNavigationAsync`. The page is
  `/Admin/BackgroundTasks`. Find the row, called `row` in the steps below:
  `scene.Page.Locator("[data-task-name='OrchardCore.Contents.VersionPruning.Tasks.ContentVersionPruningBackgroundTask']")`
  with the title "Content Version Pruning Background Task" (`VersionPruning/Tasks/ContentVersionPruningBackgroundTask.cs:13`),
  scroll it into view if the list is long, and `HighlightAsync` its badge "Disabled"
  (`row.Locator(".badge").Filter(new() { HasText = "Disabled" })`,
  `BackgroundTasks/Views/BackgroundTaskEntry.DefaultTags.SummaryAdmin.cshtml:8`).
  On screen when the narration ends: the row with the title, the description "Regularly deletes old non-latest,
  non-published content item versions.", the badge Disabled ringed, and a green Enable button.
- `task-2`: `ClearHighlightsAsync`, then click the green Enable button of that row (`row.GetByRole(AriaRole.Link, new() { Name = "Enable", Exact = true })`,
  `BackgroundTasks/Views/BackgroundTaskEntry.Buttons.SummaryAdmin.cshtml:14`) with `ClickAndWaitForNavigationAsync`.
  The list reloads without a filter, with the toast "The task has been enabled."
  (`BackgroundTasks/Controllers/BackgroundTaskController.cs:460`). Then `HighlightAsync` the badge "Next run" of the row
  (`row.Locator("[data-next-run]")`, `BackgroundTaskEntry.NextRun.SummaryAdmin.cshtml:26`) and the Edit button of the row
  (`row.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true })`, `BackgroundTaskEntry.Buttons.SummaryAdmin.cshtml:6`).
  Do not click Edit.
  On screen when the narration ends: the row with the badge Enabled, the Next run badge ringed (its text is a relative
  time such as "in 7 hours", not "due"; its tooltip holds the date and time of the next midnight in the time zone of the
  site, `BackgroundTaskEntry.NextRun.SummaryAdmin.cshtml:22`), and the red Disable button next to Edit.
- `task-3`: `ClearHighlightsAsync`. Click the drop-down button labeled "Actions" of that row
  (`row.GetByRole(AriaRole.Button, new() { Name = "Actions", Exact = true })`, `Admin/Views/AdminListActions.cshtml:32`),
  `HighlightAsync` the item "Run now" of the opened menu
  (`row.GetByRole(AriaRole.Link, new() { Name = "Run now", Exact = true })`,
  `BackgroundTasks/Views/BackgroundTaskEntry.ActionsMenuItems.SummaryAdmin.cshtml:11`; the item next to it is
  "Details"; if "Run now" is grayed out the task is not Idle yet, so stop and wait as described above), then
  `ClearHighlightsAsync` and click "Run now" with `ClickAndWaitForNavigationAsync`. The list reloads with the toast
  "The task 'Content Version Pruning Background Task' has been queued and will run shortly."
  (`BackgroundTaskController.cs:557`). Wait, off camera if it takes long
  (`scene.OffCameraAsync`), for the badge `[data-last-run][data-run-result='Succeeded']` inside the row, which the
  page shows by itself within a few seconds, and `HighlightAsync` it.
  On screen when the narration ends: the row with the badge "Last run" (green, with the time and the duration of the
  run) ringed. If the run failed, the row shows its error: do not use the take, find the cause first.
- `task-4`: no action beyond holding the frame: keep the ring on the Last run badge.
  On screen when the narration ends: the same row, Last run ringed, Enabled badge, Next run badge.

After the clip: stop the site. Restore S2 before another take.

## Checks for private data

- The pages recorded are Features, the pruning settings and Background Tasks, plus the dashboard that the clips start
  on. None shows a real name, email address, phone number or token. The only account is `admin` (admin@example.com).
- Do not open Edit, Details or a content item on camera: the Details page of a task shows its technical name and its
  schedule in code styling, and the Features page shows the feature ids next to the names. The narration never reads
  them. Do not hold the frame on them.
- Before publishing, OCR the video at one frame per second and at every scene change, as the skill asks, and search the
  text for names, addresses and tokens. Check that `VIDEO_PASSWORD` appears nowhere in the capture.
- No mask script is needed: the site is fresh. Never commit `state.json`, an MP4, audio or a copy of `App_Data`.

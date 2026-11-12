# Recording plan: Background Tasks: See, Schedule and Run Them

Audience: admin. Six recorded clips (`02-enable`, `03-list`, `04-details`, `05-schedule`, `06-run`, `07-toggle`), two
slide clips (`01-why`, `08-limits`) and two cards. A selector written below stands for `scene.Page.Locator(...)`.
Labels come from the Orchard Core source at commit `92db4a83`; every `file:line` is relative to `src/` of that checkout.

Do not decide anything while recording: where this plan gives a value, a wait or a selector, use it. If something on
screen differs from "what must be visible", stop, fix the site, and take the clip again.

## 1. The site

| | |
|---|---|
| Orchard Core | A checkout of `OrchardCMS/OrchardCore` at commit `92db4a83` (or a later `main` whose labels still match the file:line references below). Run `src/OrchardCore.Cms.Web`. |
| Address | `http://localhost:5010` (`VIDEO_BASE_URL`). |
| Data | Its own empty `App_Data` outside both repositories (`ORCHARD_APP_DATA`). Never committed. |
| Provisioning | Auto Setup through environment variables (see `references/autosetup.md` of the `orchardcore-tester` skill of the Orchard Core repository). Recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`. No setup or sign-in form is recorded. |
| Password | Chosen outside git and passed in `VIDEO_PASSWORD` (the recording plan writes `<password>`; it is never written anywhere in the repository). |
| Outbound calls | Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address before starting the site. No scene needs a mail, SMS or payment service, and nothing external is installed. |
| Background service | Defaults. Do not set `ShellWarmup`, `PollingTime`, `MinimumIdleTime` or `MaxRecentRuns`: the waits below are written for the defaults. Without `ShellWarmup`, the background service runs the tasks of a tenant only after that tenant has handled a request (`OrchardCore/OrchardCore/Modules/ModularBackgroundService.cs:394-398`, `HasPipeline()`; the documentation says the same in "Running in a Web Farm"). **After every start of the site, open `http://localhost:5010/` once** (any page of the tenant), then count the waiting time below from that request. |
| Admin settings | Defaults: Settings, Admin shows **List layout** = `Use configuration default`, **Actions layout** = `Use configuration default`, and **Let users choose the layout of a list** checked (`OrchardCore.Admin/Views/AdminSettings.Edit.cshtml:86-115`; the menu is Settings, Admin: `OrchardCore.Admin/AdminMenu.cs:26-27`). The list therefore starts in the List layout with the Buttons actions (`OrchardCore.Admin.Abstractions/AdminListOptions.cs:30-36`). |
| Sign-in | `Recorder setup` once, outside the capture, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`; the result is `state.json` (git-ignored). |

### Features to enable before recording (outside the capture)

The Blog recipe does not enable the Background Tasks module (the video enables it on camera in `02-enable`) and
registers only the three Media tasks. Two more features give the list a task that runs every minute and a task that
is disabled by default. Sign in with a normal browser, open Tools, Features, and for each name below search for it and
select **Enable** (the notification reads "The feature <name> was enabled."):

1. **Publish Later** (`OrchardCore.PublishLater`): registers the task "Scheduled Content Items Publisher".
2. **Content Version Pruning** (`OrchardCore.Contents.VersionPruning`): registers the task "Content Version Pruning Background Task", disabled by default.

Do **not** enable Background Tasks yet. Do not enable any other feature: the list must hold exactly these five tasks,
in this order (the list is sorted by title):

| Title on screen | Technical name (the `data-task-name` of its row) | Default schedule (the Schedule field) | Starts |
|---|---|---|---|
| Abandoned File Uploads Cleaner | `OrchardCore.Media.Services.ChunkFileUploadBackgroundTask` | `0 0 * * *` | Enabled |
| Content Version Pruning Background Task | `OrchardCore.Contents.VersionPruning.Tasks.ContentVersionPruningBackgroundTask` | `0 0 * * *`, atomic (lock timeout 3000, lock expiration 30000) | **Disabled** |
| Remote Media Cache Cleaner | `OrchardCore.Media.Services.RemoteMediaCacheBackgroundTask` | `30 0 * * *` | Enabled |
| Resized Media Cache Cleaner | `OrchardCore.Media.Services.ResizedMediaCacheBackgroundTask` | `0 0 * * *` | Enabled |
| Scheduled Content Items Publisher | `OrchardCore.PublishLater.Services.ScheduledPublishingBackgroundTask` | `* * * * *` | Enabled |

Titles, descriptions and schedules come from the `BackgroundTask` attributes in
`OrchardCore.Modules/OrchardCore.Media/Services/ResizedMediaCacheBackgroundTask.cs:8`, `RemoteMediaCacheBackgroundTask.cs:12`,
`OrchardCore/OrchardCore.Media.Core/Services/ChunkFileUploadBackgroundTask.cs:5`,
`OrchardCore.Modules/OrchardCore.PublishLater/Services/ScheduledPublishingBackgroundTask.cs:12` and
`OrchardCore.Modules/OrchardCore.Contents/VersionPruning/Tasks/ContentVersionPruningBackgroundTask.cs:11`.

### Seed data

None beyond what the Blog recipe creates. The tasks need no content, and no clip shows a content item.

### Snapshots and waiting

The execution state (last run, recent runs, run counts) is kept in memory by the running site. Stopping or restarting
the site clears it; the settings (schedule, enabled or disabled) are in the database in `App_Data`.

1. Enable the two features above, then stop the site (by its process ID, never by image name) and copy `App_Data`
   aside as **S1**. Start the site again and open `http://localhost:5010/` once (the tenant must handle a request before
   the background service runs its tasks).
2. Keep the site running for **at least 5 minutes** after that request. The Scheduled Content Items Publisher runs about
   once a minute, and clips `03-list` and `04-details` need its Last run badge and at least three rows in its Recent runs.
3. Record `02-enable`. Then stop the site, copy `App_Data` aside as **S2**, start the site again, open
   `http://localhost:5010/` once, and wait **at least 5 minutes** again.
4. Record `03-list` to `07-toggle` in that order, without restarting the site in between.

To take a clip again: stop the site, restore the snapshot named in the table below, start the site, open
`http://localhost:5010/` once, wait 5 minutes.

| Clip | Changes the site | Restore before another take |
|---|---|---|
| `02-enable` | Enables Background Tasks | S1 |
| `03-list` | Nothing | none |
| `04-details` | Nothing | none |
| `05-schedule` | Saves a new schedule for Resized Media Cache Cleaner (database) | S2 |
| `06-run` | In-memory runs of four tasks; Content Version Pruning Background Task and Abandoned File Uploads Cleaner get a Last run | S2 (the restart clears the runs) |
| `07-toggle` | Disables, then enables Remote Media Cache Cleaner (database) | S2, then record `06-run` again first, so Remote Media Cache Cleaner has a Last run |

Dependencies: `02-enable` first. `03-list` and `04-details` only read. `05-schedule`, `06-run` and `07-toggle` can follow
in any order after the 5-minute wait, but record them in the storyboard order so that one restore covers the others.
Do not record while the site clock is within two minutes of midnight or of 12:30 AM in the `America/Los_Angeles` time
zone: the daily tasks would run during the take and change their badges.

### Check before every recorded clip except `02-enable` (outside the capture)

Open Tools, Background Tasks. All of these must hold:

- Five rows, in the order of the table above.
- No row shows "Waiting for the background service to load this task". If one does, reload every 10 seconds until none does (it takes at most about a minute). Reading `/Admin/BackgroundTasks/States` in the page, which answers a list with one entry per task, is the same check.
- No row shows a red error line or a "Failed runs" badge.
- Scheduled Content Items Publisher shows a Last run badge with a check mark, and its Recent runs (details page) holds at least three rows.
- The rows' Next run badges show a time, except Content Version Pruning Background Task, which shows "not scheduled".

## 2. Cards and slides

- `intro` (card, chapter 00): nothing to record. The card shows the video title and its subtitle.
- `outro` (card, chapter 99): nothing to record.
- `why-1` (slide): "Work a site does on its own", four cards (Clean up, Publish, Remove old data, Keep indexes current), all bright.
- `why-2` (slide): "The Background Tasks module", four cards with check marks (See what ran, Control the schedule, Run on demand, Restricted access).
- `limits-1` (slide): "When a run fails", four cards (Last run badge, Failed runs badge, Error message, Last error). It stands in for a failure, which a fresh site cannot show (the three Media tasks and the two others never fail).
- `limits-2` (slide): "Memory, restarts and web farms", four cards (Kept in memory, Cleared on restart, Session affinity, Run now).

## 3. Clips

For every step: keep the actions shorter than the step's narration (`durations.json`); a longer step only adds a pause,
but the last frame then shows the middle of the actions. Use `TypeAsync` with a delay of 35. The recorder
holds each step until its narration ends.

Names used below: `CHUNK` = `OrchardCore.Media.Services.ChunkFileUploadBackgroundTask`, `PRUNING` =
`OrchardCore.Contents.VersionPruning.Tasks.ContentVersionPruningBackgroundTask`, `REMOTE` =
`OrchardCore.Media.Services.RemoteMediaCacheBackgroundTask`, `RESIZED` =
`OrchardCore.Media.Services.ResizedMediaCacheBackgroundTask`, `PUBLISHER` =
`OrchardCore.PublishLater.Services.ScheduledPublishingBackgroundTask`. `ROW(X)` = `[data-task-name="X"]` (the row
element, in the List and in the Grid layout: `Drivers/BackgroundTaskEntryDisplayDriver.cs:21-23` of
`OrchardCore.Modules/OrchardCore.BackgroundTasks`; every path below without a module name is in that module).

**Success notifications are toasts that hide themselves.** Every green message below ("The feature Background Tasks was
enabled.", "The task has been updated.", the queued, enabled and disabled messages) is a Bootstrap toast, `TOAST` =
`.oc-notify-toast-container .toast.message-success`, in the top right corner of the page. It is shown when the page has
loaded and hides itself 5 seconds later (`OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`, 5000 ms;
`OrchardCore.Themes/TheAdmin/Views/Message.cshtml:19-20`; `OrchardCore.Themes/TheAdmin/Assets/js/NotifyMessages/NotifyMessages.ts:1-5`). A step that has to end on a toast, or that
highlights one, would otherwise find it gone. So, in every step below that says **keep the toast**: right after the click
that loads the page (`ClickAndWaitForNavigationAsync` has returned), call `scene.MoveToAsync(TOAST)` and leave the cursor
on the toast until the step ends; Bootstrap 5.3 does not hide a toast while the pointer is over it, and hides it
5 seconds after the pointer leaves. A step that highlights a toast does so before the cursor leaves it. Nothing else on
these pages needs the cursor in the meantime. Whether the toast really stays is to be confirmed in the first take
(OPEN-QUESTIONS.md, point 10).

### Clip `02-enable`

Starts on `/Admin` (the admin home), signed in. Before `StartCaptureAsync`, expand the **Tools** group of the sidebar
(`#left-nav`), so the capture opens on the open group.
Source of the labels: `AdminMenu.cs:18-19` (Tools, Background Tasks) and `OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features);
`OrchardCore.Features/Views/Admin/Features.cshtml:27-28` (Search), `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable);
the category "Tasks" is the `Category` of `Manifest.cs:9`.

- `enable-1`: click the sidebar link **Features** (`#left-nav a:text-is("Features")`) with `ClickAndWaitForNavigationAsync`.
  Type `Background Tasks` into `#search-box`. Wait until the group titled **Tasks** (`h3:text-is("Tasks")` in `.feature-group`) is the
  only one visible and its row shows "Background Tasks". Highlight the **Tasks** heading with the caption `Tasks`, then clear
  the highlight, move to the row's Enable button (`a#btn-enable-OrchardCore_BackgroundTasks`, otherwise `a.btn:text-is("Enable")`
  inside the row that holds the text `OrchardCore.BackgroundTasks`) and click it with `ClickAndWaitForNavigationAsync`.
  **Keep the toast.** Must be visible when the narration ends: the Features page after the redirect, with the green notification
  "The feature Background Tasks was enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`).
- `enable-2`: highlight `TOAST` (the cursor is still on it) with the caption `Feature enabled`, hold 1.5 seconds, clear the highlight. The Tools
  group is still open (the Features page is under Tools). Highlight the sidebar link **Background Tasks**
  (`#left-nav a:text-is("Background Tasks")`) with the caption `Tools, Background Tasks`, then click it with
  `ClickAndWaitForNavigationAsync`. Must be visible when the narration ends: the page titled **Background Tasks** with the five
  rows. Right after enabling, the rows may show "Waiting for the background service to load this task" for a moment: the
  state of the tasks outlives the restart of the tenant, so a row normally shows its badges at once. If one row still shows
  the waiting badge at the end of the step, record the clip again after waiting.

### Clip `03-list`

Starts on `/Admin/BackgroundTasks?layout=List` (the query asks for the List layout and stores that choice in the
`orchardcore_admin_list_layout` cookie: `OrchardCore.Admin/Services/AdminListLayoutResolver.cs:36-41`). Run the check of section 1 first.
Source of the labels: `Views/BackgroundTask/Index.cshtml:7` (Background Tasks), `:20` (Search), `:42-49` (column titles Title, Status,
Last run, Next run, Actions); `Views/BackgroundTaskEntry.DefaultTags.SummaryAdmin.cshtml:8` (Enabled, Disabled);
`Views/BackgroundTaskEntry.LastRun.SummaryAdmin.cshtml:22-24, 40` (Last run, never);
`Views/BackgroundTaskEntry.NextRun.SummaryAdmin.cshtml:26-43` (Next run, due, never, not scheduled);
`OrchardCore.Admin/Views/AdminListLayoutSelector.cshtml:11, 14` (List, Grid);
`Controllers/BackgroundTaskController.cs:135-142` (Enabled, Disabled, Running or queued, Last run failed);
`Views/BackgroundTasksFilters.cshtml:13` (Status, Filter by status).

- `list-1`: highlight, one after the other and holding about 1.5 seconds each, in `ROW(PUBLISHER)`: the title link
  (`a:text-is("Scheduled Content Items Publisher")`, caption `Title`), the description (`.admin-list-secondary`, caption
  `Description`), the Enabled badge (`.badge:has-text("Enabled")`, caption `Enabled`), the Last run badge (`[data-last-run]`,
  caption `Last run`) and the Next run badge (`[data-next-run]`, caption `Next run`). Must be visible when the narration ends:
  all five rows, with the last ring on the Next run badge (text like "Next run in 20 seconds").
- `list-2`: clear the highlights. Highlight `[data-last-run]` of `ROW(PUBLISHER)` (a green badge: check mark, "Last run", "a
  moment ago" or "1 minute ago", then "·" and a duration such as "4 ms"), hold 2 seconds, clear. Then highlight `[data-last-run]` of
  `ROW(CHUNK)` with the caption `never` (the badge reads "Last run never"). Must be visible when the narration ends: the ring on
  that "never" badge.
- `list-3`: clear the highlights. Highlight `.admin-list-layout-selector` with the caption `List and Grid`, hold 1.5 seconds, clear,
  then click `.admin-list-layout-selector a[title="Grid"]` with `ClickAndWaitForNavigationAsync`. The address becomes
  `/Admin/BackgroundTasks?layout=Grid`. Highlight the column headers **Last run** and **Next run**
  (`[role="columnheader"]:text-is("Last run")` and `[role="columnheader"]:text-is("Next run")`: `OrchardCore.Admin/Views/AdminList-Grid.cshtml:228-232`; the
  `data-title` attributes are on the cells of the rows, not on the headers), one at a time. Must be visible when the narration ends: the Grid layout with the columns Title, Status, Last run, Next run and Actions.
  If the List and Grid buttons are missing, **Let users choose the layout of a list** is unchecked: check it in Settings,
  Admin, save, and record the clip again.
- `list-4`: type `cache` into `#search-box` and press Enter with `WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"))`.
  Two rows remain (Remote Media Cache Cleaner, Resized Media Cache Cleaner). Open the Status filter (the button titled **Status** of
  `.filter .bootstrap-select`), hold 3 seconds on the open menu (**Enabled**, **Disabled**, **Running or queued**, **Last run failed**),
  then choose **Enabled** (`.bootstrap-select .dropdown-menu.show a:has-text("Enabled")`); picking an option reloads the list.
  Do not choose **Last run failed** or **Running or queued** on camera: they give an empty list. Must be visible when the narration
  ends: the two cache rows in the Grid layout, `cache` in the search box.

### Clip `04-details`

Starts on `/Admin/BackgroundTasks?layout=List` (`03-list` ends in the Grid layout; the query switches back to the List layout and stores that choice in the cookie). Run the check of section 1 first.
Source of the labels: `Views/BackgroundTask/Details.cshtml:35` (Execution), `:48-119` (Status, Last run, Last run duration,
Last run triggered by, Next run, Completed runs, Failed runs), `:137` (Settings), `:155-171` (Schedule, Use tenant pipeline, Atomic),
`:205-222` (Recent runs, Started, Duration, Triggered by, Result, Error).

- `details-1`: click the title link of `ROW(PUBLISHER)` (`a:text-is("Scheduled Content Items Publisher")`) with `ClickAndWaitForNavigationAsync`.
  Highlight the Execution card (`.card:has(.card-header:text-is("Execution"))`) with the caption `Execution`. Must be visible when
  the narration ends: the details page titled "Scheduled Content Items Publisher" with the Execution card: Status **Idle**, Last run
  **Succeeded** with its date and "(a moment ago)" or similar, Last run duration, Last run triggered by **Schedule**, Next run,
  Completed runs, Failed runs **0**.
- `details-2`: clear the highlights. Highlight the Settings card (`.card:has(.card-header:text-is("Settings"))`) with the caption
  `Settings` (Technical name, Enabled, Schedule, Use tenant pipeline **No**, Atomic **No**), hold 2 seconds, clear the highlight, scroll
  down (`ScrollAsync(0, 400)`) until the Recent runs card is in view, then highlight it
  (`.card:has(.card-header:text-is("Recent runs"))`, caption `Recent runs`). Must be visible when the narration ends: the table with
  its five headers and at least three rows, each with trigger **Schedule** and result **Succeeded**.

### Clip `05-schedule`

Starts on `/Admin/BackgroundTasks?layout=List`. Run the check of section 1 first. Before `StartCaptureAsync`, read and keep
the `title` attribute of `ROW(RESIZED) [data-next-run]` (the exact date and time of its next run) as `nextBefore`.
Source of the labels: `Views/BackgroundTask/Edit.cshtml:4` (Edit Task Settings), `:15, 18` (Properties, Advanced), `:26, 33, 40` (Title,
Description, Schedule), `:44` (Reset Schedule), `:47` (hint), `:55` (Use Tenant Pipeline), `:67, 76` (Lock timeout (ms), Lock expiration
(ms)), `:71` (hint "Ignored if there is no distributed lock feature enabled."), `:87-88` (Save, Cancel);
`Controllers/BackgroundTaskController.cs:405` ("The schedule must be a valid cron expression."), `:425` ("The task has been updated.");
`Views/BackgroundTaskEntry.Buttons.SummaryAdmin.cshtml:6` (Edit).

- `schedule-1`: click the **Edit** button of `ROW(RESIZED)` (`a.btn:text-is("Edit")`) with `ClickAndWaitForNavigationAsync`. The page is
  titled **Edit Task Settings** and opens on the **Properties** tab. Highlight the Title value (`#Title`, read-only, caption `Read-only`),
  then the Description (`#Description`) and the Schedule (`#Schedule`, caption `Schedule`). Must be visible when the narration ends: Title
  "Resized Media Cache Cleaner", Description "Resized media cache cleanup.", Schedule `0 0 * * *`, the **Reset Schedule** button.
- `schedule-2`: clear the highlights. empty the `#Schedule` field with `FillAsync("")`, then
  `TypeAsync(#Schedule, "every night")`. Click **Save** (`button.save`) with `WaitForNavigationAsync(() => click)`: the page is posted
  back. Highlight the error `span[data-valmsg-for="Schedule"]` with the caption `Rejected`. Must be visible when the narration ends:
  the field still holding `every night` and, under it, the red message "The schedule must be a valid cron expression."
- `schedule-3`: clear the highlights. Click **Reset Schedule** (`#use-default-schedule`): the field shows `0 0 * * *` again (hold 1
  second so it can be read). Then `FillAsync("")` the field and `TypeAsync(#Schedule, "*/10 * * * *")`. Click **Save** with
  `ClickAndWaitForNavigationAsync`. **Keep the toast.** Must be visible when the narration ends: the list, with the green notification
  "The task has been updated." (a toast in the top right corner, not a banner above the list).
- `schedule-4`: highlight `ROW(RESIZED) [data-next-run]` with the caption `Next run`, hold 1.5 seconds. Then
  `OffCameraAsync`: every 5 seconds call `scene.NavigateAsync("/Admin/BackgroundTasks")` and read the `title` attribute of
  `ROW(RESIZED) [data-next-run]`; stop when it differs from `nextBefore`. The background service applies the change at its next
  check, which takes at most about a minute plus a few seconds; give up after 150 seconds (then the take is void: restore S2 and start
  again). After the wait, highlight the same badge again with the caption `Next run`. Must be visible when the narration ends: the ring
  on a Next run badge that now shows a short time such as "in 6 minutes" instead of "in 5 hours".
- `schedule-5`: clear the highlights. Click the **Edit** button of `ROW(RESIZED)` with `ClickAndWaitForNavigationAsync`. Highlight the label
  **Use Tenant Pipeline** (`label[for="UsePipeline"]`, caption `Use Tenant Pipeline`; the box is unchecked), hold 1.5 seconds, clear,
  then click the **Advanced** tab (`#advanced-tab`) and highlight `#LockTimeout` and `#LockExpiration` (both 0, captions
  `Lock timeout (ms)` and `Lock expiration (ms)`). Do not save: leave the page as it is. Must be visible when the narration ends: the
  Advanced tab with both fields and the hint "Ignored if there is no distributed lock feature enabled."

### Clip `06-run`

Starts on `/Admin/BackgroundTasks?layout=List`. Run the check of section 1 first, and also check that Abandoned File Uploads
Cleaner and Content Version Pruning Background Task show a Last run badge reading "never" (the site was restored from S2, so they never ran).
Source of the labels: `Views/BackgroundTaskEntry.ActionsMenuItems.SummaryAdmin.cshtml:11, 15, 18` (Run now, Details);
`OrchardCore.Admin/Views/AdminListActions.cshtml:30-32` (the **Actions** button of a row);
`Controllers/BackgroundTaskController.cs:153` (Run now, bulk), `:557-558` ("The task '{0}' has been queued and will run shortly.",
"{0} tasks have been queued and will run shortly."); `Views/BackgroundTask/_BackgroundTaskStatus.cshtml:7, 13` (Running, Queued);
`OrchardCore.Admin/Views/AdminListToolbar.cshtml:67, 73` (the bulk **Actions** button, "Bulk Action"); the **Ok** button is
`OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`.

Timing note for `run-1` and `run-2`. The background service checks the tasks about once a minute, and a run requested on
demand ends the wait between two checks early (`OrchardCore/OrchardCore/Modules/ModularBackgroundService.cs:374-392`). It starts the
requested run one second after the request when the wait began less than 10 seconds before (the minimum idle time), and at once
otherwise; a task that takes a few milliseconds can then end before the page that follows the click is drawn. The Scheduled Content
Items Publisher runs at every check, so its Last run badge tells when the wait began. To make the **Queued** badge of `run-2`
likely, start `run-1` just after a check: off camera, before `StartCaptureAsync`, reload the list every 2 seconds until the `title`
attribute of `ROW(PUBLISHER) [data-last-run]` differs from the first value read, then start the capture at once and shorten the hold
of `run-1` to 0.5 seconds, so the click comes less than 10 seconds after that check. Whether this is enough is to be confirmed in the
first take (OPEN-QUESTIONS.md, point 5).

- `run-1`: open the **Actions** dropdown of `ROW(CHUNK)` (`button.actions` inside the row), highlight the menu item
  `a[data-run-task]` with the caption `Run now`, hold 1.5 seconds, and click it with `ClickAndWaitForNavigationAsync`. **Keep the toast.**
  Must be visible when the narration ends: the list with the notification "The task 'Abandoned File Uploads Cleaner' has been queued and will run shortly."
- `run-2`: do not click anything (the cursor stays on the toast). While a task is queued or running, the page asks the server for the
  states of the tasks after 2 seconds, then after 3, 4.5, 6.75 and up to 10 seconds, and replaces the rows that changed
  (`OrchardCore.BackgroundTasks/Assets/ts/background-task-state.ts:11-14, 121-131`). At the first frame of the step, the row of
  Abandoned File Uploads Cleaner must show a **Queued** or **Running** badge; if it already shows its Last run result, the page
  was drawn too late (the run takes a few milliseconds, see the timing note below): restore S2 and record the clip again from `run-1`. Wait on camera (timeout 20 seconds) for
  `ROW(CHUNK) [data-last-run][data-run-result="Succeeded"]`, then highlight that badge with the caption `Last run`. Must be visible when
  the narration ends: the ring on a Last run badge with a check mark and a duration such as "5 ms", on a row that was not reloaded
  (the address did not change).
- `run-3`: clear the highlights. Open the **Actions** dropdown of `ROW(PRUNING)`, highlight `a[data-run-task]` with the caption `Run now`,
  click it with `ClickAndWaitForNavigationAsync` (notification "The task 'Content Version Pruning Background Task' has been queued and
  will run shortly."). Wait on camera (timeout 30 seconds) for `ROW(PRUNING) [data-last-run][data-run-result="Succeeded"]`. Highlight the
  **Disabled** badge of that row (caption `Disabled`) and its Last run badge (caption `Last run`). Must be visible when the narration
  ends: the row still **Disabled**, with a Last run badge with a check mark, and its Next run badge still "not scheduled".
- `run-4`: clear the highlights. Tick the checkboxes of `ROW(REMOTE)` and `ROW(RESIZED)` (`input[name="taskNames"][value="<name>"]`). The
  toolbar now reads "2 selected" and shows the bulk **Actions** button (`#bulk-action-menu-button`; it only appears when two or more
  rows are ticked, and the Status filter hides). Click it, highlight `a.dropdown-item[data-action="Run"]` with the caption `Run now`,
  click it: a dialog titled **Bulk Action** opens ("Are you sure you want to run now these items?"); hold 1.5 seconds, then click
  **Ok** (`#modalOkButton`) with `ClickAndWaitForNavigationAsync`. **Keep the toast.** Must be visible when the narration ends: the list with the notification
  "2 tasks have been queued and will run shortly."

### Clip `07-toggle`

Starts on `/Admin/BackgroundTasks?layout=List`. Run the check of section 1 first; `ROW(REMOTE)` must show a Last run badge
(from `06-run`) and the **Enabled** badge.
Source of the labels: `Views/BackgroundTaskEntry.Buttons.SummaryAdmin.cshtml:10, 14` (Disable, Enable);
`Views/BackgroundTaskEntry.DefaultTags.SummaryAdmin.cshtml:8` (Enabled, Disabled); `Views/BackgroundTaskEntry.NextRun.SummaryAdmin.cshtml:43`
(not scheduled); `Controllers/BackgroundTaskController.cs:460, 489` ("The task has been enabled.", "The task has been disabled.").

- `toggle-1`: highlight the red **Disable** button of `ROW(REMOTE)` (`a.btn-danger:text-is("Disable")`, caption `Disable`), click it with
  `ClickAndWaitForNavigationAsync`. **Keep the toast.** Highlight the **Disabled** badge (caption `Disabled`) and the Next run badge
  (`[data-next-run]`, caption `Next run`) of that row. Must be visible when the narration ends: the notification "The task has been
  disabled.", the **Disabled** badge, and the Next run badge reading "Next run not scheduled".
- `toggle-2`: clear the highlights. Click the green **Enable** button of `ROW(REMOTE)` (`a.btn-success:text-is("Enable")`) with
  `ClickAndWaitForNavigationAsync`. **Keep the toast.** Highlight the **Enabled** badge and the Next run badge. Must be visible when the narration ends: the
  notification "The task has been enabled.", the **Enabled** badge, and a Next run badge with a time such as "in 5 hours".

## 4. Checks for private data

- Every screen shows only the fake site: the site name **Orchard Core**, the user `admin`, and task titles. No clip opens a content item,
  a user list or a profile. No email address appears; if one does, it is `admin@example.com`.
- A failed run would put an error message, and on the details page a stack trace, on screen. Those hold paths of the machine that
  records. No task of this plan fails. If a row shows a red error line or a "Failed runs" badge in any take, stop, find the cause, and
  record again; never publish such a take.
- The technical names on the Details page and in the address bar are type names of Orchard Core, not private data.
- The password is never typed on camera (the sign-in is `state.json`). Check that `state.json` and the `App_Data` snapshots stay outside git.
- After assembling, OCR the video as `SKILL.md` says (one frame per second, every scene change, then five frames per second) and search for
  the machine's user name, folder names, host names, and anything that looks like an email address other than `admin@example.com`.

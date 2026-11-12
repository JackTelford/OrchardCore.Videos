# Recording plan: Write a Background Task: Schedules, Locks and Monitoring

This video is mostly slides. It has **one recording**, `05-admin`, which shows in the admin what the code of the slides
produces. The task it shows is `ResizedMediaCacheBackgroundTask` of the Media module, which the Blog recipe already
installs: the recording needs no module of its own and no seed data. A selector below stands for
`scene.Page.Locator(...)`. File references are in the Orchard Core source, as `path:line`; a bare file name such as `Details.cshtml` is under
`src/OrchardCore.Modules/OrchardCore.BackgroundTasks/Views/BackgroundTask/`, and `AdminMenu.cs` and
`BackgroundTaskController.cs` are under `src/OrchardCore.Modules/OrchardCore.BackgroundTasks/` (in `Controllers/` for the controller).

## The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, run on `http://localhost:5010` (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup, so no setup or sign-in form is recorded (see `references/recording.md`, "The site to record") |
| Auto Setup values | Tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, recipe `Blog`, database provider `Sqlite`, user `admin`, email `admin@example.com`, password from `VIDEO_PASSWORD` (written `<password>` here, chosen outside git) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address (`http://127.0.0.1:9`). The video calls no external service |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`, once, outside the capture. The site is provisioned by Auto Setup, so this is not the Setup screen |
| Features to enable before recording | Background Tasks (`OrchardCore.BackgroundTasks`). Nothing else: the Blog recipe enables the Media module, whose Startup registers the task (`src/OrchardCore.Modules/OrchardCore.Media/Startup.cs:77`) |
| Settings | None. Leave the `BackgroundService` configuration at its defaults: the polling time is one minute |
| Seed data | None. There are no content items, users or settings to create. The task list comes from the modules of the recipe |

Environment, for the record (Auto Setup names from `src/docs/reference/modules/AutoSetup/README.md`):

```bash
export ORCHARD_APP_DATA=<empty folder outside both repositories>
export ASPNETCORE_URLS=http://localhost:5010
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

### Preparing the site (outside the capture)

Do this once per take, in `PrepareAsync` of the recorder or by hand, signed in as `admin`:

1. Open `/Admin/Features`. Type `Background Tasks` in the search box (`#search-box`). Click the **Enable** button of
   the Background Tasks feature (`a[href*='OrchardCore.BackgroundTasks/Enable']`, `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
   Wait for the page to reload with the notification that the feature was enabled.
2. Open `/Admin/BackgroundTasks`. The background service loads the tasks of a tenant on its next round, which takes up
   to about two minutes after the feature is enabled. Until then the row of every task shows **Waiting for the background
   service to load this task** (`Views/BackgroundTaskEntry.Status.SummaryAdmin.cshtml:12`). Reload the page every 15 seconds
   until this holds, and stop after 3 minutes (if it never holds, the site is not ready and the take is not started):
   the row `[data-task-name='OrchardCore.Media.Services.ResizedMediaCacheBackgroundTask']` contains a `[data-next-run]` badge.
3. Do not click **Edit** on a task, and do not enable, disable or change any task: saved settings are stored in the
   database and would replace the values of the attribute.

The list is expected to hold three tasks, sorted by title: **Abandoned File Uploads Cleaner**, **Remote Media Cache
Cleaner** and **Resized Media Cache Cleaner** (`src/OrchardCore/OrchardCore.Media.Core/Services/ChunkFileUploadBackgroundTask.cs:5`,
`src/OrchardCore.Modules/OrchardCore.Media/Services/RemoteMediaCacheBackgroundTask.cs:12`,
`src/OrchardCore.Modules/OrchardCore.Media/Services/ResizedMediaCacheBackgroundTask.cs:8`). Only the last one is used.

## Order of the clips

There is one recording, so there is no order to follow: record `05-admin`, then assemble.

- **What it changes:** the database is not changed. The only change is in memory: the **Run now** of `admin-3` adds one
  completed run to the execution state of the task, which the page shows as **Completed runs 1**.
- **Before a new take:** stop the site, restore the copy of `App_Data` saved after step 1 of the preparation, start the
  site (run `Recorder setup` again if the saved sign-in is refused), and repeat step 2 of the preparation. Restarting the site is what resets the
  execution state; recording again without a restart shows two runs.
- **Time of day:** the task is scheduled at midnight in the time zone of the site. Do not record within five minutes
  of midnight in `America/Los_Angeles`, or a scheduled run can start during `admin-3` and **Last run triggered by**
  shows **Schedule** instead of **Run now**.

## Clip `05-admin`

Starts on `/Admin/BackgroundTasks` (opened with `OpenAsync`, not recorded). Before `StartCaptureAsync`, expand **Tools**
in the admin menu (the button of the menu item **Tools**, `AdminMenu.cs:18`), so the item **Background Tasks** is
visible under it: the link `a[href$='/Admin/BackgroundTasks']` (`AdminMenu.cs:19`). No highlight is left on the page when
the capture starts. In the steps below, "the row" is
`[data-task-name='OrchardCore.Media.Services.ResizedMediaCacheBackgroundTask']`.

### `admin-1`

The narration says that the task appears under Tools, Background Tasks, that its title and description come from the
attribute, and that the Next run badge comes from its schedule.

Actions, each highlight shown for about 1.5 seconds (`PauseAsync(1500)`), then `ClearHighlightsAsync()`:

1. `HighlightAsync` on the menu link `a[href$='/Admin/BackgroundTasks']`. No caption.
2. `HighlightAsync` on the title link of the row: `row.Locator("a", HasTextString = "Resized Media Cache Cleaner")`.
3. `HighlightAsync` on the description of the row: `row.Locator(".admin-list-secondary")`.
4. `HighlightAsync` on the badge `row.Locator("[data-next-run]")`, and leave it highlighted.

Visible when the narration ends: the Background Tasks list, with the ring on the **Next run** badge of the row
**Resized Media Cache Cleaner**. The row shows the description **Resized media cache cleanup.**, the tag **Enabled**,
the badge **Last run never**, and the badge **Next run** with a time relative to now (its tooltip holds the date and time). No row shows an error, and none
shows **Waiting for the background service to load this task**.

Labels: **Tools**, **Background Tasks** `AdminMenu.cs:18-19`; title and description
`src/OrchardCore.Modules/OrchardCore.Media/Services/ResizedMediaCacheBackgroundTask.cs:8`; **Enabled** `Views/BackgroundTaskEntry.DefaultTags.SummaryAdmin.cshtml:8`;
**Last run**, **never** `Views/BackgroundTaskEntry.LastRun.SummaryAdmin.cshtml:22-24`; **Next run**
`Views/BackgroundTaskEntry.NextRun.SummaryAdmin.cshtml:26`.

### `admin-2`

The narration says that the Settings card shows the technical name, which is the full name of the class, and the
schedule, the cron expression from the attribute.

Actions:

1. `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync` on the title link of the row
   (`row.Locator("a", HasTextString = "Resized Media Cache Cleaner")`). The page is
   `/Admin/BackgroundTasks/Details/OrchardCore.Media.Services.ResizedMediaCacheBackgroundTask`.
2. Let `settings` be the card `.card` that contains `.card-header` with the text **Settings**. `HighlightAsync` on
   `settings.Locator("dt:text-is('Technical name') + dd")`, then `PauseAsync(1500)`, then `ClearHighlightsAsync()`.
3. `HighlightAsync` on `settings.Locator("dt:text-is('Schedule') + dd")`, and leave it highlighted.

Visible when the narration ends: the details page of the task. The **Settings** card on the right shows **Technical
name** `OrchardCore.Media.Services.ResizedMediaCacheBackgroundTask`, **Enabled** with the badge **Enabled**, **Schedule**
`0 0 * * *` inside the ring, **Use tenant pipeline** **No**, and **Atomic** **No**. The **Execution** card on the left
shows **Status** **Idle**, **Last run** **Never**, a **Next run** time, **Completed runs** 0 and **Failed runs** 0.

Labels: `Details.cshtml:137` (**Settings**), `:140` (**Technical name**), `:155` (**Schedule**), `:167` (**Atomic**, shown but not narrated),
`:35` (**Execution**), `:71-75` (**Last run**, **Never**), `:116` (**Completed runs**).

### `admin-3`

The narration says: let's open Actions and queue a run on this server, whatever the schedule; the Execution card then
reports the last run: its result, how long it took, and what triggered it. The word-for-word label of the menu item is
**Run now**; the narration does not say it, so the click and the ring on it carry that information.

Actions (keep the total under the narration; the wait is the longest part):

1. `ClearHighlightsAsync()`, then `ClickAsync` on the button `button[data-task-actions]` (the text **Actions**,
   `_BackgroundTaskActions.cshtml:5`). `PauseAsync(600)`.
2. `ClickAndWaitForNavigationAsync` on the menu item `a[data-run-task]` (the text **Run now**,
   `_BackgroundTaskActions.cshtml:14`). The page loads again at the same address, with the notification **The task
   'Resized Media Cache Cleaner' has been queued and will run shortly.** (`BackgroundTaskController.cs:557`).
3. Wait, with a timeout of 20 seconds, for the locator `dd[data-last-run] [data-run-result='Succeeded']` to be visible.
   The page checks the status every 2 seconds while a task is queued, and replaces the Execution card by itself
   (`OrchardCore.BackgroundTasks/Assets/ts/background-task-state.ts:12`). Do not reload the page.
4. `HighlightAsync` on `dd[data-last-run]`, `PauseAsync(1200)`, `ClearHighlightsAsync()`.
5. `HighlightAsync` on `dt:text-is('Last run duration') + dd`, `PauseAsync(1200)`, `ClearHighlightsAsync()`.
6. `HighlightAsync` on `dt:text-is('Last run triggered by') + dd`, and leave it highlighted.

Visible when the narration ends: the **Execution** card shows **Last run** with the badge **Succeeded** and a time,
**Last run duration** with a short duration in milliseconds, **Last run triggered by** **Run now** inside the ring, and
**Completed runs** 1 with **Failed runs** 0. Under the cards, the **Recent runs** table holds one row whose **Triggered
by** is **Run now** and whose **Result** is **Succeeded**. No **Last error** card is shown.

Labels: `Details.cshtml:71`, `:87`, `:90-91`, `:116`, `:205`; **Succeeded** `_BackgroundTaskRunResult.cshtml:6`.

## Slide steps

The slides are drawn by `slides.py`; nothing is recorded for them. Each slide shows:

- `how-1`: four cards about the background service; the first two (one service, the tasks of a tenant) are bright.
- `how-2`: the same cards; the third (a scope for each run) is bright.
- `how-3`: the same cards; the fourth (the Background Tasks feature) is bright.
- `write-1`: the code of `ResizedMediaCacheBackgroundTask` (without its attribute, which the next chapter shows), with the class line and the `DoWorkAsync` line bright.
- `write-2`: the same code, with the `_logger` field, the constructor and the two `GetRequiredService` lines bright.
- `write-3`: the same code, with the `try` and `catch` blocks bright.
- `attr-1`: the attribute of `ResizedMediaCacheBackgroundTask` with Title and Description bright, and two cards.
- `attr-2`: the same attribute with Schedule bright, and two cards about the cron expression and its default (every five minutes in the attribute, every minute without it).
- `attr-3`: the attribute of `ContentVersionPruningBackgroundTask` with Schedule and Enable bright, and two cards.
- `attr-4`: two cards, about the pipeline option and the settings saved in the admin.
- `reg-1`: the `Startup` class of the Media module with the two `AddSingleton` lines bright, and one card.
- `farm-1`: three cards about several servers running several background services.
- `farm-2`: the lock method of the background service, and the `IsAtomic` property.
- `farm-3`: three cards about what the lock needs; the first two are bright.
- `cfg-1`: the `BackgroundService` section of the configuration, and four short notes about its settings.
- `hook-1`: the `IBackgroundTaskEventHandler` interface and the `BackgroundTaskEventContext` class.
- `hook-2`: a handler class, and the line that registers it.
- `mon-1`: the `IBackgroundTaskMonitor` interface, and one card.
- `mon-2`: the `BackgroundTaskState` class and the `BackgroundTaskRun` class, side by side.
- `mon-3`: the `MyService` class of the documentation, with the lines that read the state and request a run bright.

## Checks for private data

- The site is new and holds no real data. The only user is `admin` with `admin@example.com`; the password is never
  typed on camera (the sign-in comes from `state.json`, which is git-ignored and never committed).
- Look at every contact sheet: no row of the list shows an error, no page shows the password, no notification shows
  anything but the one of `admin-3`.
- OCR the video at one frame per second and at every scene change, and search for `@` addresses other than
  `admin@example.com`, for host names other than `localhost`, and for folder names of the machine (the technical name
  of the task is a type name and holds none).
- Do not commit `state.json`, a mask script, an audio file, a recording or a copy of `App_Data`.

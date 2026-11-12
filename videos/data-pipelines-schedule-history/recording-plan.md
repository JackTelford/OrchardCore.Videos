# Recording plan: Data Pipelines: Schedule Runs and Read the History

An admin video: every clip is a browser recording of the admin UI, and the slides hold no code. This plan is for the
recorder. A selector stands for `scene.Page.Locator(...)`. Source references are relative to `src/OrchardCore.Modules`
of the Orchard Core checkout the site runs from.

Narrate first (`narrate.py`), since the recorder holds each step for the length of its narration. The narration never
names the Actions menu item that starts a run (its label contains a word the content rules refuse), so the plan names
it: **Run now**.

## 1. The site

- Run `src/OrchardCore.Cms.Web` on `http://localhost:5010`, with its own `ORCHARD_APP_DATA` folder outside both
  repositories, and `HTTP_PROXY`/`HTTPS_PROXY` set to a dead address (this video calls no outside service).
- Provision it with Auto Setup (not recorded): recipe **Blog**, site name **Orchard Core**, time zone
  `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (the password the recorder gets
  in `VIDEO_PASSWORD`, chosen outside git; never write it down here or in a file of the repository).
- Sign the recorder in once (`Recorder setup`), outside the capture. The sign-in file stays out of git.
- Language `en-US`, light color scheme, 1536x864.

### Features to enable (Tools > Features, outside the capture)

The Blog recipe already enables Contents, Media and Deployment. Enable these, then check the menus **Tools > Data
Pipelines > Pipelines**, **Tools > Data Pipelines > Run History** and **Design > Workflows** exist:

| Feature (as listed on the page) | Why |
|---|---|
| Data Pipelines (`OrchardCore.DataPipelines`) | The pipelines and the run history. Its dependencies (Data Pipelines Core, Users) come with it. |
| Workflows (`OrchardCore.Workflows`) | The **Run Data Pipeline Task** and the designer. |
| Timer Workflows Activities (`OrchardCore.Workflows.Timers`) | The **Timer Event**. Without it the designer does not offer the event (`Timers/Startup.cs:8`, `Manifest.cs:29`). |

No external service is needed. The Email feature stays disabled.

### Fake seed data (outside the capture)

1. **Media > Library**: create the folder `imports`, and upload a file `subscribers.csv` (made in a temporary folder
   outside the repository) with these rows:

   ```
   Name,Email
   Jamie Chen,jamie.chen@example.com
   Priya Natarajan,priya.natarajan@example.com
   Luca Moretti,luca.moretti@example.com
   ```

2. **Tools > Deployments > JSON Import** (`Deployment/AdminMenu.cs:30`): paste this recipe in the **JSON** box and choose
   **Import**. It creates three pipelines as drafts (`Recipes/DataPipelinesRecipeStep.cs`). Import it once per site:
   importing it again creates the pipelines again.

   ```json
   {
     "steps": [
       {
         "name": "DataPipelines",
         "Pipelines": [
           {
             "Name": "Blog posts export",
             "Description": "Exports the blog posts to a file of the media library.",
             "IsEnabled": true,
             "Definition": {
               "Steps": [
                 {"StepId": "source", "Type": "DataSource", "X": 180, "Y": 320, "Properties": {"DataSourceStepSettings": {"Source": "Contents", "DataSet": "BlogPost"}}},
                 {"StepId": "file", "Type": "CreateFile", "X": 480, "Y": 320, "Properties": {"CreateFileStepSettings": {"Format": "csv", "FileName": "blog-posts"}}},
                 {"StepId": "save", "Type": "SaveToMedia", "X": 780, "Y": 320, "Properties": {"SaveToMediaStepSettings": {"Folder": "exports", "Overwrite": true}}}
               ],
               "Connections": [
                 {"SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input"},
                 {"SourceStepId": "file", "SourcePort": "Output", "TargetStepId": "save", "TargetPort": "Input"}
               ]
             }
           },
           {
             "Name": "Subscriber list check",
             "Description": "Reads the subscriber file of the media library and saves a checked copy.",
             "IsEnabled": true,
             "Definition": {
               "Steps": [
                 {"StepId": "source", "Type": "ReadMediaFile", "X": 180, "Y": 320, "Properties": {"ReadMediaFileStepSettings": {"Path": "imports/subscribers.csv"}}},
                 {"StepId": "file", "Type": "CreateFile", "X": 480, "Y": 320, "Properties": {"CreateFileStepSettings": {"Format": "csv", "FileName": "subscribers-checked"}}},
                 {"StepId": "save", "Type": "SaveToMedia", "X": 780, "Y": 320, "Properties": {"SaveToMediaStepSettings": {"Folder": "exports", "Overwrite": true}}}
               ],
               "Connections": [
                 {"SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input"},
                 {"SourceStepId": "file", "SourcePort": "Output", "TargetStepId": "save", "TargetPort": "Input"}
               ]
             }
           },
           {
             "Name": "Newsletter export",
             "Description": "Exports the blog posts for the newsletter.",
             "IsEnabled": true,
             "Definition": {
               "Steps": [
                 {"StepId": "source", "Type": "DataSource", "X": 180, "Y": 320, "Properties": {"DataSourceStepSettings": {"Source": "Contents", "DataSet": "BlogPost"}}},
                 {"StepId": "file", "Type": "CreateFile", "X": 480, "Y": 320, "Properties": {"CreateFileStepSettings": {"Format": "csv", "FileName": "newsletter"}}},
                 {"StepId": "save", "Type": "SaveToMedia", "X": 780, "Y": 320, "Properties": {"SaveToMediaStepSettings": {"Folder": "exports", "Overwrite": true}}}
               ],
               "Connections": [
                 {"SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input"},
                 {"SourceStepId": "file", "SourcePort": "Output", "TargetStepId": "save", "TargetPort": "Input"}
               ]
             }
           }
         ]
       }
     ]
   }
   ```

3. **Publish two of the pipelines as `admin`** (an imported pipeline is a draft, and a run executes with the access of
   the user who published it): in **Tools > Data Pipelines > Pipelines**, choose **Edit** on **Blog posts export**,
   then **Publish** in the toolbar of the designer (if the dialog **Publish the pipeline?** lists warnings, choose
   **Publish anyway**; the message "Version 1 was published." follows, or "The pipeline was published." when the result carries no version: `Assets/designer/src/App.vue:472` of the Data Pipelines module). Do the same for **Subscriber list check**. Leave **Newsletter export** unpublished: its **Draft** badge is on screen in `run-1`.
4. **Seed the history** in this order, waiting for each run to end (its badge in **Tools > Data Pipelines > Run
   History** reads **Succeeded** or **Failed**) before starting the next:
   1. **Blog posts export**: **Actions**, **Run now**, **Run**, twice.
   2. **Subscriber list check**: **Actions**, **Run now**, **Run** (it succeeds). Then, in **Media > Library**, open
      `imports` and delete `subscribers.csv`. Start **Subscriber list check** again: this run fails, because the file its
      first step reads is gone (`Steps/Media/ReadMediaFileStep.cs`, "The media library has no file ...").
   3. Three runs started by a workflow. Build a temporary workflow named **Seed runs** exactly as in `sched-1` to `sched-6`,
      but with the CRON Expression `* * * * *` (every minute) and without the time zone box. Publish it, and wait until
      **Run History** lists three runs whose **Started by** badge reads **Workflow** (about five minutes: the timer is
      checked once a minute by the "Timed Workflow Starter" background task, `Timers/TimerBackgroundTask.cs`). Then delete
      the workflow (**Design > Workflows**, **Delete** in its row, which is in its **Actions** menu when the admin list
      layout shows one: `Views/WorkflowTypeEntry.Buttons.SummaryAdmin.cshtml:8`, `Views/WorkflowTypeEntry.ActionsMenuItems.SummaryAdmin.cshtml`),
      so that it queues no run during a capture. Deleting a workflow deletes its instances too (`Services/WorkflowTypeStore.cs:102-121`).
      Watch **Run History** and delete the workflow as soon as the third **Workflow** run is listed: a fourth run would
      make eight runs in step 5 and nine in `hist-1`, and the history cannot be edited. If it happens, restore the
      starting `App_Data` and redo the seed.
5. Check **Run History**: seven runs, newest first: three **Workflow** runs (**Succeeded**), the **Failed** run of
   **Subscriber list check**, its earlier **Succeeded** run, and two **Succeeded** runs of **Blog posts export**.
6. Stop the site and copy its `App_Data` folder (and the sign-in file) aside, outside the repository. Restore this copy
   before every full take.

## 2. Order of the clips

Record in this order, from the snapshot above. Each clip starts from the state the previous one left.

| Order | Clip | Depends on | Changes the site |
|---|---|---|---|
| 1 | `02-run` | The seed. | Yes: one more run in the history (the newest one). |
| 2 | `03-schedule` | The features, and no leftover **Seed runs** workflow. | Yes: the workflow **Nightly blog posts export** exists. |
| 3 | `04-list` | The seed, and clip `02-run` (its run is the newest row). | No. |
| 4 | `04-details` | The seed, and clip `02-run`. | No. |

The slides (`01-concepts`, `05-keep`) and the cards need no site. To record one clip again, restore the snapshot and
record the clips before it that it depends on; to record `03-schedule` again, delete the workflow **Nightly blog posts
export** first (**Design > Workflows**, **Delete** in its row, or in its **Actions** menu).

## 3. Cards and slides

- `intro`: the opening card (title and subtitle of chapter 00). No site.
- `runs-1`: slide **What a run is**, cards 1 and 2 (Published version, In the background) bright, cards 3 and 4 dimmed.
- `runs-2`: the same slide, cards 3 and 4 (Access of the publisher, One run at a time) bright.
- `schedule-1`: slide **A schedule is a workflow**, four cards: Timer Event, Run Data Pipeline Task, Run History, Features.
- `keep-1`: slide **Cancel runs, keep the history**, three cards: Cancel a run, Permission, Retention.
- `outro`: the closing card.

## 4. Clip `02-run` (chapter 02, "Start a run")

Starts on `/Admin/DataPipelines` (**Tools > Data Pipelines > Pipelines**), opened before the capture.

- `run-1` (31 words). Actions: highlight the badges of the **Blog posts export** row (**Version 1** and the last run
  badge, for example **Succeeded** and its time), then those of **Subscriber list check** (**Failed**), then the
  **Draft** badge of **Newsletter export**. Visible at the end: the list of three pipelines, with the **Version 1**
  badges, the last run badges and the **Draft** badge. Labels: `Views/Admin/Index.cshtml:52` ("Draft"), `:56` ("Version
  {0}"), `:71` ("Last run"); menu `DataPipelines/AdminMenu.cs:18-20`.
- `run-2` (30 words). Actions: click **Actions** in the **Blog posts export** row
  (`li[data-pipeline-id]` with that text, then `button.actions`); the menu shows **Run now**, **Run history**, **Start
  over**, **Delete**. Hover **Run history** for a moment, then click **Run now**. In the confirmation dialog (title
  **Run now**, text "Run the pipeline 'Blog posts export' now?") click **Run**, with `WaitForNavigationAsync`: the page
  of the run opens. Visible at the end: the page of the new run (**Run of Blog posts export**), with the success message
  "The pipeline 'Blog posts export' will run in the background." (`Controllers/AdminController.cs:252`). Labels:
  `Views/Admin/Index.cshtml:85-94`.
- `run-3` (29 words). The page of a run refreshes itself every 3 seconds while the run has not ended
  (`Views/Admin/Run.cshtml:18`), and a navigation the scene did not start freezes the capture. So begin the step with
  `scene.OffCameraAsync`, waiting (with a reload of the page between tries) until the badge at the top of the page reads
  **Succeeded**, then `NavigateAsync` to the same address, so the capture continues on a page that no longer refreshes.
  Then highlight the badge, **Version 1**, and the **Queued**, **Started** and **Ended** rows. Visible at the end: the
  run page with **Succeeded**, the version, and the three times (shown in UTC). Labels: `Views/Admin/Run.cshtml:30-39`.

## 5. Clip `03-schedule` (chapter 03, "Schedule a pipeline")

Starts on `/Admin/Workflows/Types` (**Design > Workflows**, `Workflows/AdminMenu.cs:18-19`), which has no workflow, opened
before the capture.

- `sched-1` (24 words). Actions: click **Create Workflow** (`Views/WorkflowType/Index.cshtml:25`), with
  `ClickAndWaitForNavigationAsync`. Type `Nightly blog posts export` in **Name** (`Create.cshtml:21`; the field has the
  focus). Click **Create and open the designer** (`Create.cshtml:131`), with `WaitForNavigationAsync`, and wait until
  `[data-cy="start-picker"]` is visible. Visible at the end: the empty designer with **Start with an event** and its
  buttons (**Timer Event** among them). Keep the actions under 9 seconds.
- `sched-2` (27 words). Actions: hover then click the **Timer Event** button (`[data-cy="start-picker-TimerEvent"]`).
  The designer adds the activity as the start activity (`Services/WorkflowTypeDraftManager.cs:175`) and opens its
  settings below the canvas. Wait until the field **CRON Expression** is visible. Visible at the end: the **Timer Event**
  card with its **Start** badge, and its settings with **CRON Expression** (default value `*/5 * * * *`) and **Use site
  time zone**. Labels: `Assets/designer/src/canvas/StartPicker.vue`, `Timers/TimerEvent.cs:34`,
  `Views/Items/TimerEvent.Fields.Edit.cshtml:5,16`.
- `sched-3` (30 words). Actions: click the **CRON Expression** field (`scene.Page.GetByLabel("CRON Expression")`), select
  its text with `Control+A`, type `0 2 * * *`, press `Tab`. The settings panel applies a change by itself 600 ms after a
  field changes (a text field changes when it loses the focus, a check box at once), and has an **Apply** button that
  does the same, which turns into **Saved!** once the change is applied (the `ServerFormHost` component in
  `wwwroot/Scripts/Workflows/designer/workflows-designer.js`, `autoApplyDelay` 600, `data-cy="panel-apply"`; strings in
  `Services/WorkflowsDesignerJSLocalizer.cs:132-134`). Wait for **Saved!** before the next action. Click the **Use site
  time zone** check box (click its label), and wait for **Saved!** again. Visible at the end: the field with `0 2 * * *`,
  the box checked. The toolbar then says "All changes saved" (`:166`).
- `sched-4` (30 words). Actions: hover the **Done** outcome port of the **Timer Event** card
  (`[aria-label^="Outcome Done"]`): an outcome that leads nowhere shows a **+** only while the pointer is on it
  (`Assets/designer/src/styles/designer.css`, `.wfd-port:not(:disabled):not(.is-connected):hover .wfd-port-dot::after`).
  Click the port without dragging; a small list titled "Add the next activity, after Done" opens with a **Search
  activities** box. Type `Run Data`, and click **Run Data Pipeline Task** in the list (or press `Enter`). The designer adds
  the activity to the right, connected, and opens its settings. Visible at the end: both cards, joined by a transition,
  and the settings of **Run Data Pipeline Task** with **Data pipeline**. Labels: `Services/WorkflowsDesignerJSLocalizer.cs:333`,
  `DataPipelines/Workflows/RunDataPipelineTask.cs:44`.
- `sched-5` (30 words). Actions: choose **Blog posts export** in **Data pipeline** (`scene.SelectAsync`, the first option
  reads "Choose a data pipeline"; the list holds every pipeline by name, the unpublished **Newsletter export** too,
  `Workflows/DataPipelineActivityDisplayDrivers.cs:23`), press `Tab`, and wait for **Saved!**. Scroll the settings if needed (clear highlights first), and click the
  check box **Skip when a run is already queued or running**. Leave **Wait for the run to end** as it is (checked).
  Visible at the end: **Data pipeline** set to **Blog posts export**, and **Skip when a run is already queued or running**
  checked. Labels: `DataPipelines/Views/Items/RunDataPipelineTask.Fields.Edit.cshtml:4,7,30,40`.
- `sched-6` (27 words). Actions: click **Publish** in the toolbar. If the dialog **Publish the workflow?** lists warnings
  (the outcomes **Succeeded**, **Failed** and **Skipped** lead nowhere), click **Publish anyway**. Visible at the end: the
  message "The workflow was published as version 1." (or "The workflow was published.", the text the designer uses when
  the result carries no version, `Assets/designer/src/App.vue`, `doPublish`) and the version badge of the toolbar. Labels:
  `Services/WorkflowsDesignerJSLocalizer.cs:190-197,221`.

## 6. Clip `04-list` (chapter 04, "Read the run history")

Starts on `/Admin/DataPipelines/Runs` (**Tools > Data Pipelines > Run History**, `DataPipelines/AdminMenu.cs:25`).

- `hist-1` (38 words). Actions: highlight the first row, then its badges one after the other, from left to right: the
  status (icon and word), the version (a branch icon and **Version 1**), the queued time (a calendar icon), the duration
  (a stopwatch icon) and the last badge (a play icon). The names "The version that ran", "Queued", "Duration" and "Started
  by" are tooltips (`title` attributes) that a headless capture does not draw: the narration does not name them, and the
  screen shows icons and values only. Visible at the end: the list of eight runs, the newest first (the run of `02-run` on
  top), each with its badges. Labels: `Views/Admin/Runs.cshtml:97-103`.
- `hist-2` (35 words). Actions: highlight the badge with the play icon on the top row (it reads **Manual · admin**), then
  the one on a **Workflow** row (it reads only **Workflow**, with no user name). Visible at the end: one **Manual** badge
  and one **Workflow** badge, both highlighted. Source: `Views/Admin/Runs.cshtml:103` (the tooltip is "Started by"),
  `Services/DataPipelineRunRequest.cs`.
- `hist-3` (20 words). Actions: highlight the **Search by pipeline** box, clear the highlight, click the **Status** menu
  (`[data-dp-status-filter]`; its items are **Every status**, **Queued**, **Running**, **Succeeded**, **Failed**,
  **Cancelled**), click **Failed** with `ClickAndWaitForNavigationAsync`. Visible at the end: the list filtered to one run,
  **Subscriber list check**, **Failed**, with its error in red under the row, and the button now reading **Failed**.
  Labels: `Views/Admin/Runs.cshtml:25,56-59`.

## 7. Clip `04-details` (chapter 04, continued)

Starts on `/Admin/DataPipelines/Runs?status=Failed` (the end of `hist-3`), opened before the capture.

- `hist-4` (29 words). Actions: click **Details** on the only row (`ClickAndWaitForNavigationAsync`). Highlight the badge
  **Failed** with the version, then the rows **Queued**, **Started**, **Ended** and **Error**. Visible at the end: the page
  **Run of Subscriber list check**, with the **Failed** badge, the three times, and the **Error** row in red (the media
  library has no file `imports/subscribers.csv`). Labels: `Views/Admin/Run.cshtml:9,30-44`; `Views/Admin/Runs.cshtml:108`.
- `hist-5` (42 words). Actions: highlight the **Steps** table (columns **Step**, **Status**, **Rows in**, **Rows out**,
  **Files**, **Warnings**), its three rows (**Read a media file**, **Create a file**, **Save to the media library**) with
  their **Pending** badges and zeros, then clear the highlight, scroll down, and highlight the **Log** list. Visible at
  the end: the **Log** heading with its two entries, both red, both starting with the name of the step in bold (**Read a
  media file:**) and reading "The media library has no file 'imports/subscribers.csv'." Labels:
  `Views/Admin/Run.cshtml:63-72,119-132`. Why the steps are **Pending** and the Log holds the error twice: the file is
  checked before the run starts any step. `ReadMediaFileStep.DescribeAsync` adds the error while the engine analyzes the
  pipeline (`Steps/Media/ReadMediaFileStep.cs:38-47,131`), `DataPipelineExecutor.ExecuteAsync` then returns a
  failed result without running a step (`OrchardCore.DataPipelines.Core/Services/DataPipelineExecutor.cs:42-55`), so
  no step ever reports its progress and `step.Error` stays empty (`Services/DataPipelineRunExecutor.cs:94-96,204-241`);
  the run records the issue in the Log, and its error once more (`:223-236`). The steps after the failed one are not
  checked (`OrchardCore.DataPipelines.Core/Services/DataPipelineAnalyzer.cs:264-267`), so they add nothing.
- `hist-6` (40 words). Actions: `NavigateAsync("/Admin/DataPipelines/Runs")`, click **Details** on the first row (the
  **Succeeded** run of `02-run`, `ClickAndWaitForNavigationAsync`), then highlight the **Steps** table (three **Succeeded**
  steps, with their rows and files), then **Delivered** and its link ("Saved 'exports/blog-posts.csv' to the media
  library."). Visible at the end: the **Delivered** list with that link, followed by the muted name of the step
  (**Save to the media library**). Labels: `Views/Admin/Run.cshtml:63-115`; the text comes from
  `Steps/Media/SaveToMediaStep.cs:87`.

## 8. Checks for private data

- Only the fake people above (Jamie Chen, Priya Natarajan, Luca Moretti), `@example.com` addresses, the user `admin` and
  the sample posts of the Blog recipe appear. No real names, emails, domains, passwords or tokens.
- The password never appears: it is not typed on camera (the sign-in comes from the sign-in file).
- Look at every contact sheet (`review.py`): no empty list, no error toast, no half-loaded designer; the run pages of
  `run-3`, `hist-4` and `hist-6` have ended runs.
- OCR the final video at one frame per second and at every scene change, and search for any text outside the fake data.

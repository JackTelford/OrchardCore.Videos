# Recording plan: Workflows and the Database Session

How to record the clips of `storyboard.json` without having to decide anything. The slides are drawn by `slides.py`
and need no recording. Selectors are written as the locator they stand for: `#search-box` is
`scene.Page.Locator("#search-box")`. Paths such as `OrchardCore.Workflows/Views/...` are in `src/OrchardCore.Modules`
of the Orchard Core checkout; the labels were read at commit `92db4a83`.

Ten narrated steps of the video are recordings, in four clips: `02-enable`, `02-activity`, `03-workflow`, `03-run`.
The other chapters are slides and cards.

## 1. The site

- **Orchard Core:** `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore` at the commit above or one
  that still has the same labels, on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder
  outside both repositories. Block outbound calls with `HTTP_PROXY` / `HTTPS_PROXY` set to a dead address.
- **Auto Setup** (no setup form and no sign-in form are recorded), environment variables:

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

  `<password>` is chosen outside git and passed to the recorder in `VIDEO_PASSWORD` (`VIDEO_USER_NAME=admin`). The
  Blog recipe enables the Deployment and Recipes features that the seed import below needs
  (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step).
- **Sign in once**, outside the capture, with `Recorder setup`, which saves `state.json` (never committed).
- **Features before recording:** at Tools, Features (`/Admin/Features`), enable **Workflows** (`OrchardCore.Workflows`;
  Liquid and Scripting, its dependencies, are enabled with it). Leave **Session Workflows Activities**
  (`OrchardCore.Workflows.Session`) disabled: the first clip enables it on camera.
- **Settings:** none. **External services:** none (no email, no HTTP call: the workflow only logs).
- **Seed data:** one workflow, "Order handoff", imported once between `02-enable` and `02-activity` (section 2). Its
  names, the order number and the log texts are invented.

### The workflow to import

Open Tools, Deployments, JSON Import (`OrchardCore.Deployment/AdminMenu.cs:30`), paste this recipe into the **JSON**
field, and click **Import** (`OrchardCore.Deployment/Views/Import/Json.cshtml:21`); the page reloads with the
notification "Recipe imported successfully!" (`Controllers/ImportController.cs:185`). The format is the one of
`OrchardCore.Workflows/Recipes/workflows-variables-samples.recipe.json`. The properties left out use their defaults:
the Join's mode is `WaitAll`, the Log level is `Information`. The Commit Transaction, Fork and Join activities have no
title, so their cards keep the labels of the code.

```json
{
  "steps": [
    {
      "name": "WorkflowType",
      "data": [
        {
          "WorkflowTypeId": "orderhandoff",
          "Name": "Order handoff",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Variables": [],
          "Activities": [
            {
              "ActivityId": "start",
              "Name": "StartedByWorkflowEvent",
              "X": 40,
              "Y": 120,
              "IsStart": true,
              "Properties": {}
            },
            {
              "ActivityId": "fork",
              "Name": "ForkTask",
              "X": 310,
              "Y": 120,
              "IsStart": false,
              "Properties": {
                "Forks": [ "Billing", "Shipping" ]
              }
            },
            {
              "ActivityId": "logbilling",
              "Name": "LogTask",
              "X": 580,
              "Y": 20,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": { "Title": "Prepare billing" },
                "Text": { "Expression": "Billing prepared for order 1001", "Syntax": "Liquid" }
              }
            },
            {
              "ActivityId": "logshipping",
              "Name": "LogTask",
              "X": 580,
              "Y": 220,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": { "Title": "Prepare shipping" },
                "Text": { "Expression": "Shipping prepared for order 1001", "Syntax": "Liquid" }
              }
            },
            {
              "ActivityId": "join",
              "Name": "JoinTask",
              "X": 850,
              "Y": 120,
              "IsStart": false,
              "Properties": {}
            },
            {
              "ActivityId": "commit",
              "Name": "CommitTransactionTask",
              "X": 1120,
              "Y": 120,
              "IsStart": false,
              "Properties": {}
            },
            {
              "ActivityId": "logdone",
              "Name": "LogTask",
              "X": 1120,
              "Y": 320,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": { "Title": "Report the result" },
                "Text": { "Expression": "Order 1001 committed", "Syntax": "Liquid" }
              }
            }
          ],
          "Transitions": [
            { "SourceActivityId": "start", "SourceOutcomeName": "Done", "DestinationActivityId": "fork" },
            { "SourceActivityId": "fork", "SourceOutcomeName": "Billing", "DestinationActivityId": "logbilling" },
            { "SourceActivityId": "fork", "SourceOutcomeName": "Shipping", "DestinationActivityId": "logshipping" },
            { "SourceActivityId": "logbilling", "SourceOutcomeName": "Done", "DestinationActivityId": "join" },
            { "SourceActivityId": "logshipping", "SourceOutcomeName": "Done", "DestinationActivityId": "join" },
            { "SourceActivityId": "join", "SourceOutcomeName": "Joined", "DestinationActivityId": "commit" },
            { "SourceActivityId": "commit", "SourceOutcomeName": "Valid", "DestinationActivityId": "logdone" }
          ]
        }
      ]
    }
  ]
}
```

After the import, check it at Design, Workflows (`/Admin/Workflows/Types`): "Order handoff" is listed. Open it, and
check the **Issues** tab of the workflow panel: "No issues found." A warning that the activity type
`CommitTransactionTask` isn't available means the Session feature is not enabled yet
(`OrchardCore.Workflows/Services/WorkflowTypeDraftManager.cs:455`). If two cards overlap, change the `X` and `Y` of the
activities in the JSON (a card is 220 px wide) and import again: an import of an existing workflow updates it.

## 2. Order of the clips, and what changes the site

| Order | Clip | Starts from | Changes the site? |
|---|---|---|---|
| 1 | `02-enable` | Snapshot A: Workflows enabled, Session disabled, no seed | Yes: enables the Session feature |
| 2 | (outside the capture) import the workflow above | | Yes: creates "Order handoff". Copy `App_Data` and `state.json` aside now: **Snapshot B** |
| 3 | `02-activity` | Snapshot B | No (open the editor, change nothing) |
| 4 | `03-workflow` | Snapshot B | No (open the editors, change nothing) |
| 5 | `03-run` | Snapshot B | Yes: creates one workflow instance with its journal |

Take a copy of `App_Data` and `state.json` as **Snapshot A** before clip 1. To record a clip again, stop the site,
restore the snapshot the clip starts from, and start it again: for `02-enable` that is A, then the import again;
for `03-run` it is B. `02-activity` and `03-workflow` can be recorded again at any time after step 2 as long as no
field was changed: the designer saves a draft as soon as something changes (a banner then says who changed it); if
that happens, restore B.

Shared state of the designer, before the capture of `02-activity`, `03-workflow` and `03-run`: the designer remembers
in the browser whether its two side panes are collapsed (`TOOLBOX_COLLAPSED_KEY`, `Assets/designer/src/App.vue:73`).
Set each pane as the clip says, outside the capture. Collapse the activities pane with **Hide the activities**
(`Services/WorkflowsDesignerJSLocalizer.cs:66`) and the workflow panel with **Collapse the panel** (line 145); the
rails they leave have an expand button. **Fit to content** (line 358) is a button of the canvas controls. The
activity panel has a pin button (`PinActivityPanel`, line 149; `activity-panel-pinned` in `ActivityPanel.vue:75`):
unpinned, which is the default, the panel opens over the lower part of the canvas only while an activity is selected
and closes with Escape. Leave it unpinned in every clip.

## 3. The clips

### `02-enable`: Tools, Features

Starts on `/Admin/Features` (Tools, Features: `OrchardCore.Features/AdminMenu.cs:26-27`), opened with `OpenAsync`
before the capture, signed in. The list shows every feature, the search box is empty and nothing is highlighted.

- `enable-1`: type `session` into `#search-box` (placeholder "Search",
  `OrchardCore.Features/Views/Admin/Features.cshtml:27`). The list keeps one group, headed **Workflows**
  (`Features.cshtml:88`), with one row: **Session Workflows Activities** followed by `OrchardCore.Workflows.Session`
  (`Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`; the name is `OrchardCore.Workflows/Manifest.cs:38`), the
  description "Provides 'YesSql' Session-related activities.", a dependency badge **Workflows**
  (`Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml`, from `Dependencies` in `Manifest.cs:40`), the status
  **Disabled** and an **Enable** button.
  Highlight the row with `HighlightAsync(scene.Page.GetByText("OrchardCore.Workflows.Session"), "Session Workflows Activities")`.
  *On screen when the narration ends:* the search box with `session`, the **Workflows** heading and the one row,
  ringed in green. No other group.
- `enable-2`: click the **Enable** button of that row, the link
  `a.btn-primary[href*="Features/OrchardCore.Workflows.Session/Enable"]` (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:51`;
  its id is `btn-enable-` followed by the feature id with dots changed to underscores: check it with `Recorder probe`).
  The click posts and redirects to `/Admin/Features`; enabling a feature restarts the shell, so the page can take a
  few seconds: wrap the wait in `OffCameraAsync` with the condition
  `scene.Page.GetByText("The feature Session Workflows Activities was enabled.")` visible
  (`OrchardCore.Features/Controllers/AdminController.cs:317`). Then type `session` into `#search-box` again, since the
  reload empties it. *On screen when the narration ends:* the green notification "The feature Session Workflows
  Activities was enabled.", the search box with `session`, and the row with the status **Enabled** and a **Disable**
  button.

### `02-activity`: the activity in the designer

Starts on the designer of "Order handoff": open `/Admin/Workflows/Types` with `OpenAsync` (Design, Workflows:
`OrchardCore.Workflows/AdminMenu.cs:18-19`), click the link **Order handoff**
(`Views/WorkflowTypeEntry.Fields.SummaryAdmin.cshtml:7`) with `ClickAndWaitForNavigationAsync`, and wait until
`[data-cy="activity-commit"]` is visible. Before `StartCaptureAsync`: the workflow panel is collapsed, the activities
pane is open, its search box is empty, no activity is selected, the activity panel is closed, and **Fit to content**
was clicked so that the seven cards show.

- `activity-1`: type `commit` into the search box of the activities pane (`[data-cy="toolbox-search"]`, placeholder
  "Search activities", `WorkflowsDesignerJSLocalizer.cs:46`). The pane (title **Activities**, line 63) keeps one
  category, **Session** (`Activities/CommitTransactionTask.cs:31`, `ActivityCategoryLocalizationSources.cs`), with one
  card: **Commit Transaction** and "Commit the current transaction."
  (`Views/Items/CommitTransactionTask.Fields.Thumbnail.cshtml:1-2`). Highlight that card,
  `[data-cy^="toolbox-activity-"]` of the pane. **Do not click it**: a click adds the activity to the workflow.
  *On screen when the narration ends:* the pane with `commit` in the search box and the **Session** category with the
  ringed card, the canvas beside it.
- `activity-2`: clear the search box (select its text, press Delete), clear the highlight, and ring the Commit
  Transaction card on the canvas, `[data-cy="activity-commit"]`: its header **Commit Transaction Task**
  (`Activities/CommitTransactionTask.cs:29`), its body **Commit Transaction** (`Views/Items/CommitTransactionTask.Fields.Design.cshtml:7`)
  and the three outcome ports **Done**, **Valid**, **Invalid** (`CommitTransactionTask.cs:38`). Then open its editor
  by double-clicking the card header (the card's button has the title "Settings (or double-click the activity)"). The
  activity panel opens at the bottom of the canvas, over it (it is unpinned by default: leave it so), with the **Settings** tab: a **Title** field (`Views/ActivityMetadata.Edit.cshtml:5`)
  and the section **When it fails** with Retries, Delay (seconds), Delay between retries and Once the retries are spent
  (`Views/ActivityRetryPolicy.Edit.cshtml:7`); nothing else. Change nothing. *On screen when the narration ends:* the
  ringed card with its three ports and the activity panel with **Title** and **When it fails**.

### `03-workflow`: Fork, Join and Commit Transaction

Starts on the same designer, opened as in `02-activity`. Before `StartCaptureAsync`: both side panes are collapsed
to their rails, **Fit to content** was clicked (the whole workflow fills the canvas), no activity is selected, the
activity panel is closed.

- `workflow-1`: ring, one after the other, about 1.5 seconds each, clearing the previous ring: the start card
  `[data-cy="activity-start"]` (**Started By Workflow Event**, `Events/StartedByWorkflowEvent.cs:27`), the Fork card
  `[data-cy="activity-fork"]` (**Fork**, with "Fork execution into Billing, Shipping",
  `Views/Items/ForkTask.Fields.Design.cshtml:9`), then the two Log cards `[data-cy="activity-logbilling"]` (**Prepare
  billing**) and `[data-cy="activity-logshipping"]` (**Prepare shipping**), the last two together. *On screen when the
  narration ends:* the whole workflow, with the two Log cards ringed.
- `workflow-2`: clear the rings, then ring the Join card `[data-cy="activity-join"]` (**Join**, "Await all inbound
  activities before joining execution into a single path.", `Views/Items/JoinTask.Fields.Design.cshtml:8`), the
  Commit Transaction card `[data-cy="activity-commit"]`, and last the Log card `[data-cy="activity-logdone"]`
  (**Report the result**), following the **Valid** port of the Commit Transaction card. *On screen when the narration
  ends:* the whole workflow with the **Report the result** card ringed.
- `workflow-3`: clear the rings. Double-click the header of the Fork card: the activity panel shows the **Forks**
  field with `Billing, Shipping` and the hint "Enter one or more comma-separated fork names."
  (`Views/Items/ForkTask.Fields.Edit.cshtml:5-9`); ring the field. Press Escape, then double-click the header of
  the Join card: the panel shows the **Mode** drop-down list with `WaitAll` selected
  (`Views/Items/JoinTask.Fields.Edit.cshtml:5`; the options are the names of the enumeration, `WaitAll` and
  `WaitAny`); ring it. Change nothing. *On screen when the narration ends:* the Join editor with **Mode** ringed.

### `03-run`: run the workflow and read the journal

Starts on the same designer, opened as in `02-activity`, with both side panes collapsed and **Fit to content**
clicked. Restore Snapshot B before each take: the clip creates an instance.

- `run-1`: click **Run…** (`[data-cy="toolbar-run"]`, `WorkflowsDesignerJSLocalizer.cs:286`). The dialog **Run the
  workflow** (line 288) opens and says "The workflow has no input variables." (line 294). Click **Run**
  (`[data-cy="run-start"]`). Wait until `[data-cy="run-status"]` shows `Finished`, off camera if it takes more than a
  second. *On screen when the narration ends:* the dialog with the **Result** section: **Instance**, a `Finished`
  badge, and the button **Open the instance** (line 311).
- `run-2`: click **Open the instance** (`[data-cy="run-open-instance"]`) with `ClickAndWaitForNavigationAsync`. The
  instance page (`Views/Workflow/Details.cshtml`) shows the details card (Workflow ID, Created, Status, Correlation
  ID) and, below it, the read-only designer; scroll down until the viewer and its workflow panel are in view (clear
  highlights before scrolling). The panel preference is shared with the designer, so the workflow panel is still
  collapsed here, and its tab buttons are hidden: click the **Journal** button of its rail
  (`[data-cy="panel-rail-journal"]`, `Assets/designer/src/panel/WorkflowPanel.vue`), which expands the panel on the
  Journal tab. If the panel is already expanded, click the tab `[data-cy="panel-tab-journal"]`
  (`WorkflowsDesignerJSLocalizer.cs:272`). *Expected journal*, from the code (see OPEN-QUESTIONS.md): eight records,
  each with the status **Completed**:

  | # | Activity | Outcomes |
  |---|---|---|
  | 1 | Started By Workflow Event | Done |
  | 2 | Fork Task | Billing, Shipping |
  | 3 | Prepare shipping | Done |
  | 4 | Join Task | (none) |
  | 5 | Prepare billing | Done |
  | 6 | Join Task | Joined |
  | 7 | Commit Transaction Task | Done, Valid |
  | 8 | Report the result | Done |

  Ring the two Join records, `[data-cy="journal-record-4"]` and `[data-cy="journal-record-6"]`. On the canvas, the
  Join card shows the badge `×2` (`Assets/designer/src/canvas/ActivityNode.vue`, the executed badge). *On screen when
  the narration ends:* the **Journal** tab with the records in order and the two Join records ringed.
- `run-3`: clear the ring, and click the record of the Commit Transaction task, `[data-cy="journal-record-7"]`: the
  activity panel opens on that run. Ring the outcomes text of the record (`[data-cy="journal-record-7"] [data-cy="journal-outcomes"]`,
  `Done, Valid`). *On screen when the narration ends:* record #7 with `Completed` and `Done, Valid` ringed, and record
  #8 (**Report the result**) below it.

## 4. The slides

Each slide step is drawn by `slides.py` (the code is taken from `OrchardCMS/OrchardCore` at the commit above, trimmed
with `// ...`). Nothing to record.

- `intro`: the opening card, "Workflows and the Database Session", "Commit, Fork and Join".
- `session-1`: three cards, "One scope", "One session", "One commit"; the first two are bright.
- `session-2`: the registration of the session in `OrchardCoreBuilderExtensions.cs` (`CommitAsync` before dispose,
  `CancelAsync` on an exception), with two notes.
- `session-3`: `PersistAsync` of the workflow manager and `SaveAsync` of the workflow store, "No SaveChangesAsync
  here: the shell scope commits."
- `commit-1`: `CommitTransactionTask.cs`, with the model state check and the `Invalid` return bright.
- `commit-2`: the same file, with `SaveChangesAsync` and the `Valid` return bright.
- `commit-3`: `ModelBinderAccessorFilter.cs` and `LocalModelBinderAccessor.cs`, and the note about `NullModelUpdater`.
- `commit-4`: `SessionStartup`, the empty display driver and the empty view model.
- `fork-1`: `ForkTask.cs`: the `Forks` property and `Outcome(Forks)`.
- `fork-2`: the stack and the loop of `WorkflowManager.cs`.
- `fork-3`: the `switch (Mode)` of `JoinTask.cs`, with notes on `WaitAll`, `WaitAny` and `Joined`.
- `where-1`: two cards, "After the Join" bright, "Invalid" dimmed.
- `where-2`: the same cards, "Invalid" bright.
- `outro`: the closing card.

## 5. Checks for private data

- The only names on screen are `admin`, "Order handoff", the activity titles and the order number 1001: all invented.
  The Workflow ID of the instance is generated.
- OCR the capture of each clip at one frame per second and at every scene change. Search for any email address other
  than `admin@example.com`, any person's name, any host other than `localhost`, and `<password>`'s real value. The
  site name is "Orchard Core".
- The password never appears: no sign-in form is recorded, and `state.json` stays out of git.

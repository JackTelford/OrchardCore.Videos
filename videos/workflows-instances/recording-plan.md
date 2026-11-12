# Recording plan: Workflow Instances

How to record `workflows-instances` without having to decide anything. The video has one chapter of slides, four
recorded clips and two cards. Read `README.md` for the short version, `OPEN-QUESTIONS.md` for what a person must
confirm on the running site, and `references/recording.md` of the skill for the recorder's scene API.

Abbreviations in the "Label from" lines (all paths are in Orchard Core at commit `92db4a83`, `src/OrchardCore.Modules`):

- `WF` is `OrchardCore.Workflows`.
- `DES` is `OrchardCore.Workflows/Assets/designer/src`, the Vue app of the designer and of the instance page.
- `LOC` is `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs`, the texts of that app.
- `DESJS` is `OrchardCore.Workflows/wwwroot/Scripts/Workflows/designer/workflows-designer.js`, the built app with the shared
  designer package it imports (one long line: search it for the name quoted in each reference).
- `IDXJS` is `OrchardCore.Workflows/wwwroot/Scripts/workflow-index-editors/workflow-index.js`, the built script of the list of
  instances with the shared list component it imports (search it for the name quoted).

In this file a selector such as `[data-cy=retry-button]` stands for `scene.Page.Locator("[data-cy=retry-button]")`.

## 1. The site it needs

### 1.1 Build and start

- `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with its own
  `ORCHARD_APP_DATA` folder outside both repositories, provisioned by Auto Setup (no setup or login form is recorded):

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>`, the one chosen outside git and passed to the recorder as `VIDEO_PASSWORD` |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- Outbound calls are blocked as `references/recording.md` asks (`HTTP_PROXY` and `HTTPS_PROXY` set to a dead address),
  with one exception, because the faulted instances of this video call a stand-in service on the same machine (1.4):
  also set `NO_PROXY=127.0.0.1,localhost` for the site's process. If the stand-in is not reached, see
  `OPEN-QUESTIONS.md` (question 6).
- Sign the recorder in once, outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and
  `VIDEO_PASSWORD=<password>`.
- Leave the admin list layout at its default (List): the video shows the List layout (`OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`).
- The recording is a fresh site with fake data only, so it needs no mask script.

### 1.2 Features to enable before recording

In the admin, **Tools > Features** (`OrchardCore.Features/AdminMenu.cs:27`), enable, outside the capture:

- **Workflows** (`OrchardCore.Workflows`, `WF/Manifest.cs:12-18`).
- **HTTP Workflows Activities** (`OrchardCore.Workflows.Http`, `WF/Manifest.cs:20-26`): the seed uses its **Http Request
  Task** and **Signal Event** activities.

The **Deployment** feature, which holds **Tools > Deployments > JSON Import**, is already enabled by the Blog recipe
(`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:57`).

### 1.3 Seed: two workflows

Open **Tools > Deployments > JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:30`), paste the whole text below in
the **JSON** field (`OrchardCore.Deployment/Views/Import/Json.cshtml:15`), and select **Import**. It creates two enabled
workflows, published as their first version, with a variable `orderNumber` that a run takes as its input. The activity
titles are what the narration and the screens show: **Order received**, **Check stock**, **Stock confirmed**,
**Payment requested**, **Wait for payment** and **Payment received**. The cards show no code: a Log Task shows its text,
an Http Request Task its method and URL, and a Signal Event its signal.

```json
{
  "name": "workflow-instances-video-seed",
  "displayName": "Workflow instances video: seed",
  "description": "Two workflows with fake data for the Workflow Instances video.",
  "author": "The Orchard Core Team",
  "website": "https://orchardcore.net",
  "version": "1.0.0",
  "issetuprecipe": false,
  "categories": [],
  "tags": [],
  "steps": [
    {
      "name": "WorkflowType",
      "data": [
        {
          "WorkflowTypeId": "orderfollowup",
          "Name": "Order follow-up",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Variables": [
            { "Name": "orderNumber", "TypeName": "string", "Description": "The order to follow up.", "IsInput": true }
          ],
          "Activities": [
            {
              "ActivityId": "orderreceived", "Name": "StartedByWorkflowEvent", "X": 40, "Y": 40, "IsStart": true,
              "Properties": { "ActivityMetadata": { "Title": "Order received" } }
            },
            {
              "ActivityId": "checkstock", "Name": "HttpRequestTask", "X": 380, "Y": 40, "IsStart": false,
              "Properties": {
                "ActivityMetadata": { "Title": "Check stock" },
                "HttpMethod": "GET",
                "Url": { "Expression": "http://127.0.0.1:5099/stock", "Syntax": "Literal" },
                "HttpResponseCodes": "200"
              }
            },
            {
              "ActivityId": "stockconfirmed", "Name": "LogTask", "X": 720, "Y": 40, "IsStart": false,
              "Properties": {
                "ActivityMetadata": { "Title": "Stock confirmed" },
                "Text": { "Expression": "The order is in stock.", "Syntax": "Literal" }
              }
            }
          ],
          "Transitions": [
            { "SourceActivityId": "orderreceived", "SourceOutcomeName": "Done", "DestinationActivityId": "checkstock" },
            { "SourceActivityId": "checkstock", "SourceOutcomeName": "200", "DestinationActivityId": "stockconfirmed" }
          ]
        },
        {
          "WorkflowTypeId": "paymentfollowup",
          "Name": "Payment follow-up",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Variables": [
            { "Name": "orderNumber", "TypeName": "string", "Description": "The order waiting for its payment.", "IsInput": true }
          ],
          "Activities": [
            {
              "ActivityId": "paymentrequested", "Name": "StartedByWorkflowEvent", "X": 40, "Y": 40, "IsStart": true,
              "Properties": { "ActivityMetadata": { "Title": "Payment requested" } }
            },
            {
              "ActivityId": "waitforpayment", "Name": "SignalEvent", "X": 380, "Y": 40, "IsStart": false,
              "Properties": {
                "ActivityMetadata": { "Title": "Wait for payment" },
                "SignalName": { "Expression": "payment-received", "Syntax": "Literal" }
              }
            },
            {
              "ActivityId": "paymentreceived", "Name": "LogTask", "X": 720, "Y": 40, "IsStart": false,
              "Properties": {
                "ActivityMetadata": { "Title": "Payment received" },
                "Text": { "Expression": "The payment arrived.", "Syntax": "Literal" }
              }
            }
          ],
          "Transitions": [
            { "SourceActivityId": "paymentrequested", "SourceOutcomeName": "Done", "DestinationActivityId": "waitforpayment" },
            { "SourceActivityId": "waitforpayment", "SourceOutcomeName": "Done", "DestinationActivityId": "paymentreceived" }
          ]
        }
      ]
    }
  ]
}
```

Check: **Design > Workflows** lists **Order follow-up** and **Payment follow-up**, and neither shows **Disabled** or
**No startup task**. Do not leave the recipe's text, or any other code, on screen in the capture.

The recipe step is `WF/Recipes/WorkflowTypeStep.cs`; the activity names are the class names (`StartedByWorkflowEvent`,
`HttpRequestTask`, `SignalEvent`, `LogTask`), and the properties are the ones the activities read
(`WF/Http/Activities/HttpRequestTask.cs:117-152`, `WF/Http/Activities/SignalEvent.cs:33-37`, `WF/Activities/LogTask.cs:39-43`).
The **Literal** syntax is registered at `WF/Startup.cs:116`.

### 1.4 Seed: the stand-in service

The **Check stock** activity of **Order follow-up** calls `http://127.0.0.1:5099/stock`. A run is **Finished** when
that address answers `200`, and **Faulted** when nothing listens there (the connection fails, the activity throws, and the
engine faults the instance: `WF/Services/WorkflowManager.cs:717-770`).

The stand-in is a throwaway static server, started from an empty folder outside both repositories:

```bash
mkdir standin && cd standin
echo "12 units" > stock
python -m http.server 5099 --bind 127.0.0.1
```

Stop it with Ctrl+C in its terminal, or by its process id, never by image name. It is only a local file server: it
answers nothing else and holds no data. Remove the folder at the end.

### 1.5 Seed: seven runs

Start every run outside the capture, the same way: **Design > Workflows**, select the name of the workflow (the
designer opens), select **Run…** in its toolbar (`LOC` line 286), type the value in the **orderNumber** field of the
**Run the workflow** dialog, select **Run**, check the **Instance** status in the dialog's result, and select **Close**.
**Run…** needs the **Execute workflows** permission, which the administrator has (`WF/Permissions.cs:19-23`), and runs
the published version of a workflow that starts with **Started By Workflow** (`WF/Controllers/WorkflowDesignerController.cs:1118-1132`).

Do them in this order, with the stand-in running for the first four and stopped for the last three:

| # | Stand-in | Workflow | orderNumber | Status shown in the dialog |
|---|---|---|---|---|
| 1 | running | Order follow-up | `A-1001` | Finished |
| 2 | running | Order follow-up | `A-1002` | Finished |
| 3 | running | Payment follow-up | `A-1003` | Halted |
| 4 | running | Payment follow-up | `A-1004` | Halted |
| 5 | stopped | Order follow-up | `A-1005` | Faulted |
| 6 | stopped | Order follow-up | `A-1006` | Faulted |
| 7 | stopped | Order follow-up | `A-1007` | Faulted |

The page **Design > Workflows > Instances** (the **Instances** button) must then show, newest first, the counts
**All** 7, **Halted** 2, **Running** 0, **Faulted** 3, **Finished** 2, **Aborted** 0, and every row with a
**Version 1** badge. If a count is different, delete the instances and start again. Three Faulted instances are needed:
one is retried from its own page in `04-faulted`, and the other two are retried together in `05-bulk`, because the bulk
actions need at least two checked rows (`IDXJS`, "f()>1").

### 1.6 Snapshot

With the seven runs in place and the stand-in **stopped**: stop the site, copy its `App_Data` folder and `state.json`
aside (outside both repositories), and call it the **seeded snapshot**. Restore it, and start the site again, before
every full take, and before a new take of a clip that changes the site (section 2).

## 2. Order of the clips, and which ones change the site

| Clip | Needs | Changes the site | Stand-in |
|---|---|---|---|
| `00-intro`, `99-outro` | nothing (cards) | no | n/a |
| `01-instance` | nothing (slides) | no | n/a |
| `02-list` | the seeded snapshot | no | any |
| `03-waiting` | the seeded snapshot | no | any |
| `04-faulted` | the seeded snapshot | **yes**: retries the instance `A-1007` (it becomes Finished) | **running** |
| `05-bulk` | the state `04-faulted` leaves | **yes**: retries `A-1005` and `A-1006`, cancels `A-1003` and `A-1004` | **running** |

Record in this order: `02-list`, `03-waiting`, `04-faulted`, `05-bulk`. `02-list` and `03-waiting` show the counts of the
seeded snapshot (7, 2, 0, 3, 2, 0), so record them before `04-faulted`, or restore the snapshot first. To record
`04-faulted` or `05-bulk` again, stop the site, restore the seeded snapshot, start the site, and record `04-faulted`
again before `05-bulk`. Start the stand-in (1.4) before `04-faulted` and keep it running until `05-bulk` is done.

## 3. The clips

Every recorded clip starts on the page named under its heading, which the recorder opens before the capture starts
(`scene.OpenAsync`). Keep each step's actions shorter than its narration. The text to type is fake.

### `00-intro` (card, no recording)

`intro`: the opening card, drawn by the assembler. Nothing to record.

### `01-instance` (slides, no recording)

- `instance-1`: the slide "One workflow, many instances": three cards, Definition, Event and Instance.
- `status-1`: the slide "Instances that have not ended": two cards, Halted and Running.
- `status-2`: the slide "Instances that have stopped": three cards, Finished, Faulted and Aborted.

### `02-list`

Starts on `/Admin` (the dashboard). Before the capture starts, make sure the **Design** group of the admin menu is
collapsed (the menu remembers what was left open: if **Design** is open, click it once to close it). Expanding it is the
first action of `list-1`.

`list-1`

- Actions: click the **Design** row of the admin menu to expand it (it is a button: it has children and no page of its
  own); click the **Workflows** link under it with `ClickAndWaitForNavigationAsync` (`li#workflows a`). Then
  `HighlightAsync` the **Instances** badge of the first workflow row (`a.badge:has-text("Instances")`), wait about 2
  seconds, `ClearHighlightsAsync`, and `HighlightAsync` the toolbar button `[data-cy=all-instances]`.
- On screen at the end: the list of workflows, with **Order follow-up** and **Payment follow-up** and an **Instances**
  badge on each, and a green ring on the **Instances** button beside **Create Workflow**.
- Label from: `WF/AdminMenu.cs:18-19` (Design, Workflows); `WF/Views/WorkflowType/Index.cshtml:24` (Instances button);
  `WF/Views/WorkflowTypeEntry.DefaultTags.SummaryAdmin.cshtml:20-24` (the badge, shown when the workflow has instances).

`list-2`

- Actions: `ClearHighlightsAsync`; click `[data-cy=all-instances]` with `ClickAndWaitForNavigationAsync`. On the page of
  the instances of every workflow, `HighlightAsync` in turn, about 2.5 seconds each and clearing in between: the
  workflow name of the first row (`[data-cy=instance-workflow]`, first), its ID link (`a[href*="/Workflow/Details/"]`, first),
  its status badge (`[data-cy=instance-status]`, first), its version badge (`[data-cy=instance-version]`, first), and its
  **Created** badge (the first `.metadata .badge`).
- On screen at the end: the page **Instances** with the seven rows, newest first (the three Faulted, then the two Halted,
  then the two Finished), each with the workflow name, ID, status, **Version 1** and **Created**; the **Created** badge of
  the first row highlighted.
- Label from: `WF/Views/Workflow/Index.cshtml:53` (breadcrumb Instances); `WF/Views/WorkflowEntry.Fields.SummaryAdmin.cshtml:7-13`
  (workflow name, then ID); `WF/Views/WorkflowEntry.DefaultTags.SummaryAdmin.cshtml:19-24` (status, **Version {0}**);
  `WF/Views/WorkflowEntry.DefaultMeta.SummaryAdmin.cshtml:6` (**Created**); `WF/Controllers/WorkflowController.cs:204-208, 285`
  (the newest first, **Recently created**).

`list-3`

- Actions: `ClearHighlightsAsync`; `HighlightAsync` the counts bar `[data-cy=instance-counts]` for about 3 seconds;
  `ClearHighlightsAsync`; click `[data-cy=instance-count-Faulted]` with `ClickAndWaitForNavigationAsync` (the list now
  holds the three Faulted instances, and the **Faulted** button is the selected one); then `HighlightAsync` the group of
  drop-downs in the toolbar (`.btn-group.filter`). Do not open or change the drop-downs.
- On screen at the end: the list of the three Faulted instances, the counts bar with **Faulted 3** selected, and a green ring on
  the toolbar's **Workflow**, **Status**, **Created** and **Sort** drop-downs.
- Label from: `WF/Views/Workflow/Index.cshtml:37-45, 66-75` (the counts and their labels); `WF/Views/WorkflowInstancesFilters.cshtml:16-20`
  (**Workflow**, **Status**, **Created**, **Sort**); `WF/Controllers/WorkflowController.cs:285-305` (the choices). The drop-downs submit the filter when a choice is made
  (`IDXJS`, "changed.bs.select"), which this clip does not use.

### `03-waiting`

Starts on `/Admin/Workflows/Instances` (all instances, the seeded counts).

`open-1`

- Actions: click `[data-cy=instance-count-Halted]` with `ClickAndWaitForNavigationAsync` (two Halted rows of **Payment
  follow-up**); click the ID link of the first row (`a[href*="/Workflow/Details/"]`, first) with
  `ClickAndWaitForNavigationAsync`; then `HighlightAsync` the details card (`.workflow-details`).
- On screen at the end: the top of the instance page: the breadcrumb ending with the instance's ID, the tab pills
  **Workflow** and **State** (the **Workflow** pill selected), and the card with **Workflow ID**, **Created**, **Status**
  (Halted) and **Correlation ID** (None). The **State** tab is never opened: it shows JSON.
- Label from: `WF/Views/Workflow/Details.cshtml:18, 21, 34, 40, 46, 61, 63`.

`open-2`

- Actions: `ClearHighlightsAsync`; `ScrollAsync(0, 250)` so that the whole designer (its toolbar and its canvas) is in
  the frame; `HighlightAsync` the text `[data-cy=instance-version]` in the designer's toolbar (**Runs on version 1**).
- On screen at the end: the read-only designer, with the title **Payment follow-up**, **Runs on version 1** highlighted,
  the legend, the three activities **Payment requested**, **Wait for payment** and **Payment received**, and the panel
  with the **Variables** and **Journal** tabs on the right.
- Label from: `DES/App.vue:678-681` and `LOC` line 234 (**Runs on version {0}**); `DES/App.vue:683-695`, `LOC` lines 262-266 (the legend).

`open-3`

- Actions: `ClearHighlightsAsync`; `HighlightAsync` `[data-cy=blocking-badge]` (on **Wait for payment**), wait about 3
  seconds, `ClearHighlightsAsync`, then `HighlightAsync` the legend `[data-cy=viewer-legend]`.
- On screen at the end: the legend highlighted; **Payment requested**, its connection and **Wait for payment** outlined in
  green; the **Blocking** badge (hourglass) on **Wait for payment**; **Payment received** not outlined.
- Label from: `DES/canvas/ActivityNode.vue:250-253`, `LOC` lines 262-266 (**Blocking**; "The instance waits on these activities.";
  "Activities and connections the instance ran."); `DES/styles/designer.css:1096-1099, 1279-1285` (green for what ran).

`open-4`

- Actions: `ClearHighlightsAsync`; `HighlightAsync` the **Variables** tab `[data-cy=panel-tab-variables]` (it is the selected
  tab when the page opens), wait about 2 seconds, then `HighlightAsync` the row of `orderNumber` in the table of that tab.
- On screen at the end: the **Variables** tab with one row, highlighted: the name **orderNumber** with an **Input** badge and the
  description "The order waiting for its payment.", the type **Text** (the display name of the variable type `string`), and the
  **Value** `A-1004` (the newest of the two Halted instances), without quotes.
- Label from: `DES/panel/WorkflowPanel.vue:74`, `LOC` lines 68, 70, 72-75, 102; `DES/panel/VariablesTab.vue:226-248`;
  `DES/variables/variableValues.ts:15-16, 120` (the type's display name; the value as text).

`open-5`

- Actions: `ClearHighlightsAsync`; click `[data-cy=panel-tab-journal]`; click the record of **Wait for payment**
  (`[data-cy=journal-record-2]`), which opens the activity panel at the bottom of the designer, over the canvas, on its **Runs** tab
  (a record of the journal opens the run it stands for); then click `[data-cy=activity-tab-details]` (**Details**), as the narration says.
- On screen at the end: the **Journal** tab listing record `#1` **Payment requested** (**Completed**, outcome Done) and
  record `#2` **Wait for payment** (**Waiting**), and the activity panel at the bottom, on its **Details** tab, showing the
  **Type**, and **This instance** with the badge **Waiting on this activity**.
- A click on an activity of the canvas also opens its panel, on **Details** (`DESJS`, "show-node": in the read-only page a pointer
  press on an activity emits it). This clip uses the journal record instead.
- Label from: `DES/panel/WorkflowPanel.vue:74, 192-198` (a record opens its activity's panel); `DES/panel/JournalTab.vue:27-58`;
  `DES/panel/ActivityPanel.vue:567-586`; `LOC` lines 148, 258-260, 272-278.

### `04-faulted`

Starts on `/Admin/Workflows/Instances`. The stand-in is running (1.4), so the retry succeeds.

`fault-1`

- Actions: click `[data-cy=instance-count-Faulted]` with `ClickAndWaitForNavigationAsync`; click the ID link of the first
  row (`A-1007`, the newest) with `ClickAndWaitForNavigationAsync`; `HighlightAsync` the **Fault Message** row of the card
  (`.ocat-wrapper:has(label:text-is("Fault Message"))`).
- On screen at the end: the top of the instance page of an **Order follow-up** instance, with **Status** Faulted and the
  **Fault Message** row, highlighted, holding the error text.
- Label from: `WF/Views/Workflow/Details.cshtml:46, 51-58` (**Fault Message** is shown only for a faulted instance).

`fault-2`

- Actions: `ClearHighlightsAsync`; `ScrollAsync(0, 250)`; `HighlightAsync` `[data-cy=activity-checkstock]`.
- On screen at the end: the designer with **Order received** and the connection to **Check stock** outlined in green,
  **Check stock** with a red outline and a red mark (`[data-cy=faulted-badge]`) highlighted, and **Stock confirmed** plain,
  not outlined.
- Label from: `DES/canvas/ActivityNode.vue:241-243`, `LOC` line 268; `DES/styles/designer.css:1287-1292`.

`fault-3`

- Actions: `ClearHighlightsAsync`; click `[data-cy=panel-tab-journal]`; `HighlightAsync` the error line of the second record
  (`[data-cy=journal-record-2] [data-cy=journal-error]`).
- On screen at the end: the **Journal** tab with record `#1` **Order received** (**Completed**, Done, a duration in ms) and
  record `#2` **Check stock** (**Faulted**, a duration in ms) with its error text, highlighted.
- Label from: `DES/panel/JournalTab.vue:27-58`; `DES/panel/journalStatus.ts:21-35`; `LOC` lines 272-279, 316.

`fault-4`

- Actions: `ClearHighlightsAsync`; click `[data-cy=journal-record-2]`. The activity panel opens at the bottom of the designer, over the
  canvas (it covers about the lower 40% of the designer unless it is pinned: `DES/styles/designer.css:189-210`), on its
  **Runs** tab, with the run `#2` already open.
- On screen at the end: the activity panel for **Check stock**, on **Runs**, with the line of run `#2` (**Faulted**, time, duration)
  and, under it, the error of the run. The panel's title bar shows the **Retry from here** button.
- Label from: `DES/panel/WorkflowPanel.vue:192-198`, `DES/panel/ActivityPanel.vue:514-523`; `DES/panel/RunsTab.vue:99-120`;
  `LOC` lines 155-158, 317.

`fault-5`

- Actions: click `[data-cy=retry-button]` (**Retry from here**). Do not answer the dialog.
- On screen at the end: the dialog titled **Retry the instance?** with the text "The instance runs again from Check stock,
  with its current state." and its two buttons, **Retry** (`#modalOkButton`) and **Cancel** (`#modalCancelButton`), over the page.
  It is the admin theme's own dialog (`DESJS`, "confirmAction" calls the page's `confirmDialog`;
  `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-64, 225`).
- Label from: `LOC` lines 317-319 (**Retry from here**, **Retry the instance?**, the message); `DES/panel/ActivityPanel.vue:345-352`.

`fault-6`

- Actions: click `#modalOkButton` (**Retry**) in the dialog. Wait (`scene.Page.Locator("text=The instance ran again: Finished.").WaitForAsync()`, a Playwright call, not a scene method) for the text
  "The instance ran again: Finished." to appear (up to 10 seconds); it stays 5 seconds (`DESJS`, "showToast": the default timeout is 5000 ms). Hold for about 7
  seconds from the click, so the message is on screen during the first sentence. Then `NavigateAsync` to the page's own address (a reload: the card at the top then reads **Status** Finished), `ScrollAsync(0, 600)`
  to the bottom of the page, and `HighlightAsync` the **Restart** button (`a.btn-warning:has-text("Restart")`).
- On screen at the end: the bottom of the instance page with **Back**, **Restart** (highlighted) and **Delete**.
  Earlier in the step: the message "The instance ran again: Finished." over the designer, with **Stock confirmed** now outlined.
- Label from: `LOC` lines 321 (**The instance ran again: {0}.**), 318-319; `DES/App.vue:612-623`; `WF/Views/Workflow/Details.cshtml:89-97`;
  `WF/Services/WorkflowManager.cs:371-420` (the retry runs the version the instance started on).

### `05-bulk`

Starts on `/Admin/Workflows/Instances`, after `04-faulted`: the counts are **All** 7, **Halted** 2, **Running** 0,
**Faulted** 2, **Finished** 3, **Aborted** 0. The stand-in is running. The **Actions** button of the toolbar appears only when
two or more rows are checked (`IDXJS`, "f()>1"), and each menu choice only works then: every bulk action here checks two rows.

`bulk-1`

- Actions: click `[data-cy=instance-count-Faulted]` with `ClickAndWaitForNavigationAsync` (two rows, `A-1005` and `A-1006`); tick
  the checkbox of the first row, then that of the second (`input[name=itemIds]`, `Nth(0)` then `Nth(1)`). When the second is
  ticked, the toolbar's drop-downs give way to the **Actions** button (`#bulk-action-menu-button`) and the text "2 selected";
  click it.
- On screen at the end: both Faulted rows ticked, and the **Actions** menu open with **Retry**, **Cancel** and **Delete**.
- Label from: `WF/Controllers/WorkflowController.cs:318-323`; `OrchardCore.Admin/Views/AdminListToolbar.cshtml:63-76`;
  `WF/Views/WorkflowEntry.Checkbox.SummaryAdmin.cshtml:7`; `IDXJS` ("selected-items", "actions").

`bulk-2`

- Actions: click **Retry** in the menu (`a.dropdown-item[data-action=Retry]`). The dialog **Bulk Action** reads "Are you sure you
  want to retry these items?": click `#modalOkButton` (**Ok**) with `WaitForNavigationAsync`.
- On screen at the end: the list after the redirect, with the message "2 faulted instances ran again." at the top, and the counts bar
  with **Faulted 0** and **Finished 5**. The page keeps the **Faulted** filter, so its list is empty and reads "Nothing here!".
- Label from: `OrchardCore.Admin/Views/AdminListToolbar.cshtml:73`; `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99` (**Ok**);
  `WF/Controllers/WorkflowController.cs:464-486, 531-566` (the message; the warning about instances that couldn't be retried does not
  appear here); `WF/Views/Workflow/Index.cshtml:86`.

`bulk-3`

- Actions: click `[data-cy=instance-count-Halted]` with `ClickAndWaitForNavigationAsync` (two Halted rows); tick `#select-all` (it
  ticks both rows); click `#bulk-action-menu-button`; click `a.dropdown-item[data-action=Cancel]`; the dialog reads "Are you sure you
  want to cancel these items?": click `#modalOkButton` with `WaitForNavigationAsync`. Keep the pauses short: this step has four
  clicks and two page loads.
- On screen at the end: the list with the message "2 instances were canceled." at the top and the counts bar with **Halted 0**
  and **Aborted 2** (the list is under the **Halted** filter, so it is empty and reads "Nothing here!").
- Label from: `WF/Controllers/WorkflowController.cs:568-609` (Cancel makes every checked instance that is not **Finished** **Aborted**, the faulted ones too, and clears what it waits on);
  `WF/Views/Workflow/Index.cshtml:42-45`; `OrchardCore.Admin/Views/AdminListToolbar.cshtml:31`.

### `99-outro` (card, no recording)

`outro`: the closing card, drawn by the assembler. Nothing to record.

## 4. Checks for private data

- The capture shows only the fake data of this plan: the order numbers `A-1001` to `A-1007`, the workflow names, the
  address `127.0.0.1:5099`, and the IDs and times the engine generates. No real name, email, token or password appears: the
  administrator is `admin` with `admin@example.com`, and the password is never typed on camera.
- OCR the video at one frame per second and at every scene change, as the skill asks, and look for: a real machine name or
  user name in the **Fault Message** text or in a path, a proxy address, and the password.
- Look at the **Fault Message** of the faulted instances on a contact sheet before assembling (it is the text of an error the
  operating system writes; see `OPEN-QUESTIONS.md`, question 5), and at the **State** tab: it must never be opened.
- Stop the stand-in and the site by their process ids, and remove the `standin` folder, the snapshot and the `App_Data`
  copies when the video is published.

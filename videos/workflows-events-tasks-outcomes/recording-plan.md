# Recording plan: Workflows: Events, Tasks and Outcomes

For the person who records. File references are in the Orchard Core checkout at the commit the catalog was read
from (`catalog/features.json`, `source.commit`), under `src/OrchardCore.Modules/`. A path that starts with
`OrchardCore.` (for example `OrchardCore.Contents/...`) is relative to that folder; any other path (`Views/...`,
`Activities/...`, `Services/...`, `Assets/designer/src/...`, `AdminMenu.cs`) is relative to
`OrchardCore.Workflows/`. A selector stands for `scene.Page.Locator(...)`;
`GetByLabel(...)` and `GetByRole(...)` are the same calls of the scene's page. The narration is in
`storyboard.json`; the recorder holds each step for the length of its narration, so keep the actions of a step
shorter than the narration.

The video builds one workflow, **Announce published pages**, in four recordings, then publishes a page to run it.

```
Content Published Event (Start, Page) --Done--> Fork Task (Forks: Editors, Audit)
                                                   |-- Editors --> Notify Task
                                                   |-- Audit   --> Log Task
```

## 1. The site it needs

- **Site.** A fresh site, provisioned by Auto Setup (see the `orchardcore-tester` skill, `references/autosetup.md`):
  run `OrchardCore.Cms.Web` on `http://localhost:5010` with `ORCHARD_APP_DATA` pointing at an empty folder outside
  both repositories. Recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, tenant `Default`.
  No setup screen and no sign-in form are recorded.
- **User.** `admin`, email `admin@example.com`. The password is chosen outside git and passed in
  `VIDEO_PASSWORD` (write `<password>` in notes, never a real one). `Recorder setup` signs in once and saves
  `state.json` (git-ignored).
- **Outbound calls.** Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address in the environment of the site (not of the
  recorder). This video sends no email, SMS or request,
  so it needs no external service and no local stand-in.
- **Features to enable before recording** (outside the capture): only **Workflows** (`OrchardCore.Workflows`), on
  **Tools > Features** (`OrchardCore.Features/AdminMenu.cs:25-26`). Its dependencies (Liquid and
  Scripting) come with it. Leave **HTTP Workflows Activities**, **Timer Workflows Activities**, **Session Workflows
  Activities** and Email off: the video names none of their activities, and the Activities pane and the start picker
  then list fewer activities. The core activities and those of the Contents and Users modules are still listed.
  The Blog recipe does not enable Workflows (`../OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-79`).
- **Settings.** None to save. The admin theme is the recipe's (TheAdmin).
- **Seed data.** No workflow exists when `02-event` starts (the list under **Design > Workflows** is empty). Do not
  create a Page before `05-run`. The Blog recipe's own content (Main Menu, About, Blog, tags) stays as it is; its
  owner is `admin`.
- **The workflow panel stays collapsed.** The right panel of the designer opens on its **Variables** tab, whose first
  paragraph prints a JavaScript call and a Liquid tag (`Services/WorkflowsDesignerJSLocalizer.cs:69`, shown by
  `Assets/designer/src/panel/VariablesTab.vue:223`). An admin video must not show them, so the panel is collapsed to
  its rail for the whole video; the sheet is hidden then (`WorkflowPanel.vue:307`, `v-show`). The designer remembers
  this in the browser: key `orchardcore:workflows-designer:panel-collapsed`, value `true`
  (`WorkflowPanel.vue:26,61`; prefix in `Assets/designer/src/ui/preferences.ts:4`). Every clip starts a new browser
  context, so put it in the init script (file kept **outside** git, passed in `VIDEO_INIT_SCRIPT`), one line:
  `try { localStorage.setItem('orchardcore:workflows-designer:panel-collapsed', 'true'); } catch (e) {}`.
  Check it on the first take with `Recorder probe /Admin/Workflows/Types "() => Object.keys(localStorage)"` and by
  looking at the designer: the right edge shows a narrow rail of four icons (the expand button, then one icon for each
  of the tabs Variables, Workflow and Issues; `WorkflowPanel.vue:279-299`), no panel.
- **Never hover the Variables icon of that rail.** Hovering a rail icon opens its tab over the canvas
  (`WorkflowPanel.vue:279-292`, "peek"), and the Variables tab prints the code. Keep the cursor off the rail; every
  action below stays on the canvas, the Activities pane or the toolbar.
- **Never open the Available data tab** of the activity panel, and never open If Else Task or any activity whose
  editor has an expression box: their placeholders and hints print JavaScript and Liquid.
- **The version pill shows from the start.** Creating a workflow saves its first version, which is empty
  (`Controllers/WorkflowTypeController.cs:275` calls `Services/WorkflowTypeStore.cs:81-87`, which calls
  `Services/WorkflowTypeVersionStore.cs:95-111`), and the designer shows the version the workflow type points to
  (`Controllers/WorkflowDesignerController.cs:994-997`, `App.vue:648-655`). So the toolbar shows a pill **Version 1**
  from `event-2` on, before anything is published, and publishing the finished workflow makes it **Version 2**. This is
  expected: do not retake a clip because of it, and do not rely on the pill to tell a draft from a published workflow.
- **Viewport.** The recorder's 1536x864. Collapse the admin menu in the designer pages if the page has the toggle,
  so the canvas has the width (the designer page has no breadcrumb and takes the whole page).

## 2. Order, dependencies and snapshots

Record in this order; each clip starts from the site the previous one left (the draft of the workflow is saved on
the server as the designer works). Each clip opens a fresh browser, so it starts by opening the workflow again.

| Clip | Needs | Changes the site |
|---|---|---|
| `02-event` | Workflows enabled, no workflow | Creates the workflow and its draft: the event |
| `03-outcomes` | the snapshot after `02-event` | Draft: adds the Fork Task, connected |
| `04-tasks` | the snapshot after `03-outcomes` | Draft: adds the Notify Task and the Log Task, connected |
| `05-run` | the snapshot after `04-tasks` | Publishes the workflow, creates the page "Opening hours", creates an instance |

Take a snapshot (stop the site, copy `App_Data` and `state.json`) before `02-event` and after each clip. To record
a clip again, stop the site, restore the snapshot taken after the previous clip, start the site, and record. Do not
record `05-run` on a site where the workflow is already published: the pill would already read Version 2 or higher,
and the page of the earlier run would exist. The cards `00-intro` and `99-outro` and the slides `01-blocks` need no site.

**Opening the workflow at the start of `03-outcomes`, `04-tasks` and `05-run`** (before `StartCaptureAsync`, not
recorded): `scene.OpenAsync("/Admin/Workflows/Types")`, click the link named **Announce published pages**
(`Views/WorkflowTypeEntry.Fields.SummaryAdmin.cshtml:7`), and wait until the canvas shows the activities of the
draft (`[data-node-id]` count, one per node: `ActivityNode.vue:223`; 1 for `03-outcomes`, 2 for `04-tasks`, 4 for
`05-run`. Do not count `[data-cy^="activity-"]`: it also matches the controls of the activity panel, such as
`activity-panel` and `activity-panel-pin`, which are in the page even while the panel is closed). The
workflow type id is a number that depends on the site; do not hard-code it. If an activity is cut off by the edge of
the canvas, use the canvas's **Fit to content** control before the capture starts (its label is
`WorkflowsDesignerJSLocalizer.cs:358`; the control belongs to the shared designer package, so find it with
`Recorder probe`). In `04-tasks` and `05-run` all the activities must be fully visible, on one screen, and the Fork
Task's two outcomes must be easy to tell apart.

## 3. The clips

### `00-intro` (card) and `99-outro` (card)

Cards drawn by the assembler from the storyboard (title, subtitle). Nothing to record.

### `01-blocks` (slides, drawn by `slides.py`)

Four slides with the heading "Events, tasks and outcomes" and four cards (Event, Task, Outcome, Connection); the
card of the step is bright, the others are dimmed.

- `blocks-1`: the Event card is bright (waits for something; as the start activity, starts the workflow).
- `blocks-2`: the Task card is bright (does something).
- `blocks-3`: the Outcome card is bright (how an activity ended; some have several). The narration names If Else
  Task (`Activities/IfElseTask.cs:25,63`: True, False) and Email Task
  (`OrchardCore.Email/Workflows/Activities/EmailTask.cs:35,139`: Done, Failed).
- `blocks-4`: the Connection card is bright (links an outcome to the next activity; an activity no connection
  reaches never runs: `Services/WorkflowTypeDraftManager.cs:531`).

### `02-event` (clip): create the workflow and add the event

Starts on the dashboard: `scene.OpenAsync("/Admin")`, the admin menu with **Design** visible, nothing expanded.
Changes the site: creates the workflow and its first activity.

- `event-1`: Highlight the **Design** entry of the admin menu (`AdminMenu.cs:18`), click it, then click
  **Workflows** (`AdminMenu.cs:19`) with `ClickAndWaitForNavigationAsync`. Wait until the page shows the heading
  "Workflows" and the message "Nothing here! There are no workflow types for the moment." Then click the
  **Create Workflow** button (`Views/WorkflowType/Index.cshtml:25`) with `ClickAndWaitForNavigationAsync`. On screen
  when the narration ends: the Create Workflow page with the empty **Name** field and the hint under it. Labels:
  `AdminMenu.cs:18-19`, `Views/WorkflowType/Index.cshtml:25,34` and `Create.cshtml:21`.
- `event-2`: `TypeAsync(GetByLabel("Name"), "Announce published pages")`; leave **More settings** closed. Click the
  **Create and open the designer** button (`Views/WorkflowType/Create.cshtml:131`) with
  `ClickAndWaitForNavigationAsync`, then wait until `[data-cy="start-picker"]` is visible. On screen when the
  narration ends: the designer, the Activities pane on the left, the empty canvas and, over it, the box titled
  "Start with an event" (`WorkflowsDesignerJSLocalizer.cs:334-335`). The name appears in the toolbar, followed by
  the pill "Version 1" (see section 1). The right panel is a rail (see section 1).
- `event-3`: Move to `[data-cy="start-picker-ContentPublishedEvent"]` (the button reads "Content Published Event":
  `Workflows/Activities/ContentPublishedEvent.cs:22` in `OrchardCore.Contents`; it is the first button of the
  picker: `StartPicker.vue:13`) and click it. Hold on the result. On screen when the narration ends: the picker is
  gone, one activity is on the canvas, and the activity panel has opened at the bottom.
- `event-4`: Highlight the **Start** badge (`[data-cy="start-badge"]`, `ActivityNode.vue:236`, label
  `WorkflowsDesignerJSLocalizer.cs:329`) of the node, whose header reads "Content Published Event" and whose body
  reads "Content Published" and "Any content type" (`Views/Items/ContentPublishedEvent.Fields.Design.cshtml:4,6`).
  Then move to the activity panel, whose Settings tab shows **Title** and **Content Types** (`Views/ActivityMetadata.Edit.cshtml:5`,
  `OrchardCore.Contents/Views/Items/ContentPublishedEvent.Fields.Edit.cshtml:5`). Why the badge: the first event
  added to a workflow is the start activity (`Services/WorkflowTypeDraftManager.cs:175`). Clear the highlights
  before the next step. On screen when the narration ends: the node with its Start badge, and the open panel.
- `event-5`: Click the Content Types picker (a bootstrap-select list whose placeholder reads "Any content type":
  `OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Picker.cshtml:16`; find its button with
  `Recorder probe`, probably `scene.Page.Locator(".bootstrap-select button.dropdown-toggle")` inside the panel).
  Its list is attached to `body` (`data-container="body"`), and has a search box with the placeholder "Search
  content types" (`Picker.cshtml:20`). Type `Page` in the search box, then click the option **Page**, then close
  the list by pressing Escape or clicking the label "Content Types". Tick only **Page**. Wait until the node body
  reads "Page" instead of "Any content type" (the settings are applied when the field loses focus: press Tab or
  click the label if the node does not change within two seconds). On screen when the narration ends: the node
  reading "Content Published" and "Page", and the Content Types field showing the tag Page.

### `03-outcomes` (clip): the Done outcome and the Fork Task

Starts on the designer of **Announce published pages** (section 2), one activity on the canvas (the event, with
Start and Page), nothing selected, the activity panel closed. Changes the site: adds the Fork Task.

- `outcomes-1`: Move to the **Done** port on the right edge of the event node
  (`[data-cy^="port-"][data-outcome="Done"]` inside the node, `OutcomePort.vue:29-30`; the label reads "Done":
  `Workflows/Activities/ContentActivity.cs:60` in `OrchardCore.Contents`). Hold two seconds on it: the dot turns
  into a green circle with a plus (`Assets/designer/src/styles/designer.css:1466-1480`, only while the cursor is on
  an unconnected port). Click it. On screen when the narration ends: the menu "Add the next activity, after Done"
  (`WorkflowsDesignerJSLocalizer.cs:333`, `[data-cy="quick-add"]`) open next to the port, with the search box
  "Search activities" and the list of activities.
- `outcomes-2`: `TypeAsync([data-cy="quick-add-search"], "fork")`; the list shrinks to one row, "Fork Task" with the
  category "Control Flow" (`Activities/ForkTask.cs:19,21`). Click `[data-cy="quick-add-ForkTask"]`. Wait until a
  second node exists. On screen when the narration ends: the Fork Task node to the right of the event, the
  connecting line, and the activity panel open on the Fork Task (field **Forks**). The Fork Task has no required
  setting (`ViewModels/ForkTaskViewModel.cs`), so its header shows no issue badge.
- `outcomes-3`: Highlight the line between the two nodes (the transition is a path drawn by `TransitionEdge.vue`;
  if it cannot be highlighted, move the cursor along it from the event to the Fork Task), then clear the highlight.
  The Fork Task node reads "Fork Task" in its header, and its body shows the heading "Fork" and "No forks"
  (`Views/Items/ForkTask.Fields.Design.cshtml`); it has no port yet, since a Fork Task without forks has no outcome
  (`Activities/ForkTask.cs:33-35`). On screen when the narration ends: the event, the line, the Fork
  Task.
- `outcomes-4`: `TypeAsync(GetByLabel("Forks"), "Editors, Audit")`, then press Tab. The label "Forks" and its hint
  "Enter one or more comma-separated fork names." come from `Views/Items/ForkTask.Fields.Edit.cshtml:5,9`. Wait until
  the Fork Task node shows two ports. On screen when the narration ends: the field with the value, the node body
  "Fork execution into Editors, Audit", and the two ports.
- `outcomes-5`: Highlight the two ports of the Fork Task (labels "Editors" and "Audit": the outcomes are the fork
  names, `Activities/ForkTask.cs:40`; the node's ports are `[data-outcome="Editors"]` and `[data-outcome="Audit"]`),
  one after the other for about two seconds each, then clear the highlights. Close the activity panel
  (`[data-cy="activity-panel-close"]`, label "Close the activity panel") if it covers a port. On screen when the
  narration ends: the Fork Task with the ports Editors and Audit, both unconnected.

### `04-tasks` (clip): Notify Task, Log Task and a connection

Starts on the designer with two nodes (event, Fork Task connected to it; the Fork Task has the ports Editors and
Audit), nothing selected, the panel closed. Changes the site: adds two tasks and two connections.

- `tasks-1`: Click the **Editors** port of the Fork Task (it shows a plus on hover). In the menu, `TypeAsync(
  [data-cy="quick-add-search"], "notify")`; the row reads "Notify Task", category "UI"
  (`Activities/NotifyTask.cs:34,36`). Click `[data-cy="quick-add-NotifyTask"]`. On screen when the narration ends:
  the Notify Task node to the right of the Fork Task, connected to Editors, and the activity panel open on its
  settings: **Title**, **Notification Type**, **Message**, then the section "When it fails"
  (`Views/Items/NotifyTask.Fields.Edit.cshtml:6,15`, `Views/ActivityRetryPolicy.Edit.cshtml:7`). The Message is
  required and empty (`ViewModels/NotifyTaskViewModel.cs`), so the header of the node shows a red issue badge with
  1 until `tasks-2` types the message (`Services/WorkflowTypeDraftManager.cs:402-422`): expected, and the narration
  does not mention it yet.
- `tasks-2`: Highlight the **Notification Type** select, which shows "Success" (the first value of the list:
  `../OrchardCore/OrchardCore.DisplayManagement/Notify/NotifyEntry.cs:8-13`); do not change it. `TypeAsync(
  GetByLabel("Message"), "Announced by the workflow, a page was published.")`, then press Tab. Wait until the node
  body shows the message and its header reads "Success Notification" (`Views/Items/NotifyTask.Fields.Design.cshtml:20`).
  The hint under Message reads "The message to show. With Liquid support." (`NotifyTask.Fields.Edit.cshtml:19`):
  it is product text, not code; keep the cursor off it. On screen when the narration ends: the node with the
  message, the Message field filled.
- `tasks-3`: First `TypeAsync([data-cy="toolbox-search"], "log")` (placeholder "Search activities",
  `WorkflowsDesignerJSLocalizer.cs:46`), then click the filter **Tasks** (the label of the radio `#wfd-kind-tasks`,
  group "Kind of activities": `Assets/designer/src/toolbox/ActivityToolbox.vue:38-44,59-67`; labels
  `WorkflowsDesignerJSLocalizer.cs:49-50`). Type first, then click Tasks: the pane opens every category as soon as a
  search text or a filter is set (`ActivityToolbox.vue:17,19`), so clicking Tasks with an empty search lists all the
  tasks, and the cards of If/Else, Script, Set Variable and others have descriptions that name JavaScript and Liquid.
  With "log" typed the pane lists the card **Log** (category "Primitives", "Write a log entry.":
  `Views/Items/LogTask.Fields.Thumbnail.cshtml:1-2`) and the two cards "User Logged in" and "User Logged out" of the
  Users module (plain text); the Tasks filter then keeps only the Log card. Click it
  (`[data-cy="toolbox-activity-LogTask"]`). A Log Task node is added in the middle of the visible canvas, not
  connected. Its Text is required and empty (`ViewModels/LogTaskViewModel.cs`), so its badge shows 2 issues, in red,
  until `tasks-4` types the text. Never leave the pane with the search empty and the filter on **All** while
  scrolling it. On screen when the narration ends: the Log Task node on the canvas and the Activities pane showing
  only the Log card.
- `tasks-4`: The new node may overlap another one: drag it by its header (`.wfd-node-header`) to the empty area to
  the right of the Fork Task and below the Notify Task, then click its settings button (`[data-cy="node-edit"]`,
  title "Settings (or double-click the activity)": a drag closes the panel). Highlight the **Log Level** select,
  which shows "Information" (`Views/Items/LogTask.Fields.Edit.cshtml:6`; the default is `Activities/LogTask.cs:35`);
  do not change it. `TypeAsync(GetByLabel("Text"), "Audit, a page was published.")`, then Tab. Wait until the node
  body reads `Information "Audit, a page was published."` (`Views/Items/LogTask.Fields.Design.cshtml`). If the
  actions are longer than the narration, move the drag to the end of `tasks-3`. On screen when the narration ends:
  the Log Task with its text, not connected.
- `tasks-5`: Highlight the issue badge in the header of the Log Task node (`[data-cy="issue-badge"]`,
  `ActivityNode.vue:259`; it shows the number of issues, and its tooltip lists them: `WorkflowsDesignerJSLocalizer.cs:331`). Now that the text is
  applied it counts 1, amber: the warning "This activity can't be reached from a start activity, so it never runs."
  (`Services/WorkflowTypeDraftManager.cs:531`; amber because it has no error: `ActivityNode.vue:257`). If it still
  shows 2 in red, the text was not applied yet: press Tab in the Text field and wait. Do not open the Issues tab
  (it is behind the rail). On screen when the narration ends: the Log Task with the badge, highlighted.
- `tasks-6`: `DragToAsync` from the **Audit** port of the Fork Task to the Log Task node (the middle of the node;
  a click on the port would open the menu of activities instead, so the pointer must move between pressing and
  releasing, with `steps`). Wait until a line joins them and `[data-cy="issue-badge"]` is gone from the Log Task: the
  designer saves the connection on its own, and the issues come back with the save (`state/designerStore.ts:285-288`,
  the `issues` of `Controllers/WorkflowDesignerController.cs:459`), so allow a few seconds. On screen when the
  narration ends: both tasks connected to the Fork Task, no badge.
- `tasks-7`: Highlight the **Done** port of the Notify Task and then that of the Log Task, about two seconds each,
  then clear the highlights (`Activities/NotifyTask.cs:55`, `Activities/LogTask.cs:46`). On screen when the
  narration ends: the whole workflow on one screen, the two Done ports unconnected. Take the snapshot.

### `05-run` (clip): publish, run it, look at the instance

Starts on the designer with the whole workflow (four activities, all connected, no badge), unpublished.
Changes the site: publishes the workflow, creates a page, creates an instance.

- `run-1`: Click **Publish** (`[data-cy="toolbar-publish"]`, `WorkflowsDesignerJSLocalizer.cs:190`). The toolbar
  first reads "All changes saved" (`:166`). It must publish at once: if the box "Publish the workflow?" or "The
  workflow can't be published" opens, the workflow has an issue; stop and fix the workflow before recording (it is
  not part of the story). Before the click the pill `[data-cy="published-version"]` reads "Version 1" (section 1);
  keep it in view. On screen when the narration ends: the same pill now reading "Version 2" (the text is "Version "
  and a number, `WorkflowsDesignerJSLocalizer.cs:218`; `Services/WorkflowTypeVersionStore.cs:111` adds 1 to the
  latest version, which is the empty one saved when the workflow was created), and the toast "The workflow was
  published as version 2." (`:221`) if it is still visible.
- `run-2`: In the admin menu click **Content**, then **Content Items** (`OrchardCore.Contents/AdminMenu.cs:65,68`)
  with `ClickAndWaitForNavigationAsync`. On the list click the **New** button
  (`OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`, `#new-dropdown`); a drop-down lists the content
  types you can create. Click **Page**. Wait until the editor shows the field **Title**
  (`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`). On screen when the narration ends: the editor of a Page.
- `run-3`: `TypeAsync(GetByLabel("Title"), "Opening hours")`; click **Publish** (`button.publish`,
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`) with `ClickAndWaitForNavigationAsync` (it returns
  to the Content Items list because the editor was opened from it with a `returnUrl`). On screen when the
  narration ends: the Content Items list after the redirect.
- `run-4`: Hold on the messages at the top of the list. Expected: a green message "Announced by the workflow, a page
  was published." (the Notify Task, `Activities/NotifyTask.cs:62`) next to "Your Page has been published."
  (`OrchardCore.Contents/Controllers/AdminController.cs:473`). Highlight the first one, then the second, about
  two seconds each. If the Notify message is missing, the workflow did not run: check that it is published and that
  the page type is Page, and record the clip again. On screen when the narration ends: both messages, and the
  page "Opening hours" in the list.
- `run-5`: `NavigateAsync("/Admin/Workflows/Types")`, then click **Instances**
  (`[data-cy="all-instances"]`, `Views/WorkflowType/Index.cshtml:24`) with `ClickAndWaitForNavigationAsync`. On
  screen when the narration ends: the Instances page titled "Instances", the status counts with **Finished** at
  one (`Views/Workflow/Index.cshtml:37-46`), and one row with the workflow name, the instance id, the badge
  "Finished" and the badge "Version 2" (`Views/WorkflowEntry.DefaultTags.SummaryAdmin.cshtml:21-24`; version 1 is the
  empty workflow saved when it was created, see section 1). The id is a random string; it is not private.
- `run-6`: Click the id of the instance (the link of the row, `a[href*="Details"]` in the list; confirm with
  `Recorder probe`). Wait until the designer canvas of the instance page shows the four nodes (four `[data-node-id]` elements; the
  viewer uses the same node component). Scroll down by
  the height of the details card (`ScrollAsync(0, ...)`) so that the whole canvas is visible. On screen when the
  narration ends: the four activities and the connections highlighted as run (the legend in the toolbar reads
  "Activities and connections the instance ran.": `WorkflowsDesignerJSLocalizer.cs:266`), including both
  branches. Do not click the **State** tab of the page: it prints the state of the instance as JSON.

## 4. Checks for private data and for code on screen

- Only fake data appears: the site name "Orchard Core", the user `admin`, the workflow name, the page title
  "Opening hours", and the two messages typed above. The browser shows only `localhost`. The ids of the instance
  and of the content item are random. Search the frames by OCR for the machine's user name, folder names, a real
  email address, and any host other than `localhost`.
- Search the frames by OCR for `variable(`, `{{`, `{%`, `Workflow.`, `input(` and `JavaScript`: none may appear
  except the words in the hints "With Liquid support." under Message and Text, which are product text (see
  `OPEN-QUESTIONS.md`). A frame that shows the Variables paragraph means the panel was not collapsed: record the
  clip again.
- Look at every contact sheet: no "Loading the workflow..." frame at the end of a step, no error message, no
  "Publish anyway" or "Fix these errors" box, and no empty Activities pane.

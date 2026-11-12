# Open questions: Workflow Instances

What the author could not verify from the Orchard Core source (commit `92db4a83`) or its documentation, and what a person
must confirm on a running site before the first take. None of these is stated as a fact in the narration: each one is
either only in recording-plan.md, or something the narration says that the source supports and the screen must show.

`WF` is `src/OrchardCore.Modules/OrchardCore.Workflows`, `DES` is `WF/Assets/designer/src`, `DESJS` is
`WF/wwwroot/Scripts/Workflows/designer/workflows-designer.js` and `IDXJS` is
`WF/wwwroot/Scripts/workflow-index-editors/workflow-index.js` (the built scripts, which hold the shared packages that the
sources import: the sources of those packages are not in the tree that was read).

Checked in those built scripts, and so not open: the filter drop-downs submit when a choice is made (`IDXJS`, "changed.bs.select");
the **Actions** menu of the list shows only when two or more rows are checked (`IDXJS`, "f()>1"), which is why the plan checks two rows
for each bulk action and seeds three Faulted instances; a pointer press on an activity of the instance page opens its panel
(`DESJS`, "show-node" with the read-only canvas); the retry confirmation is the admin theme's dialog, with **Retry** and **Cancel**
buttons (`DESJS`, "confirmAction"; `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-64`); and the message after a retry lasts 5
seconds (`DESJS`, "showToast").

## 1. The seed imports and starts the runs as planned

Checked: the recipe step (`WF/Recipes/WorkflowTypeStep.cs`), the model classes (`WorkflowType`, `ActivityRecord`,
`WorkflowVariableDefinition`, `WorkflowExpression<T>`), the module's own sample recipe
(`WF/Recipes/workflows-variables-samples.recipe.json`), and how the designer's **Run…** decides it can run a workflow
(`WF/Controllers/WorkflowDesignerController.cs:1118-1132`). The seed in recording-plan.md was not run.

Confirm on the site:

- **Tools > Deployments > JSON Import** accepts the pasted text. The **JSON** field is a code editor
  (`OrchardCore.Deployment/Views/Import/Json.cshtml:6-9`), so pasting may need the editor's own value to be set rather than
  typing; the import must end without an error message.
- Both workflows appear in **Design > Workflows** without the **Disabled** or **No startup task** badge, and the designer of
  each shows **Run…** in its toolbar.
- The **Run the workflow** dialog asks for **orderNumber**, and the result shows **Finished**, **Halted** and **Faulted** as the
  table in section 1.5 says. In particular a **Check stock** activity that answers `200` from the file server leaves
  the instance Finished, and a **Wait for payment** activity (a Signal Event in the middle of the workflow) leaves it Halted
  (the engine halts an event that is not the start: `WF/Services/WorkflowManager.cs:664-696`).
- Each instance row shows a **Version 1** badge, and the **Variables** tab of an instance shows the value of **orderNumber**
  (the value is read from the instance's properties: `WF/Controllers/WorkflowDesignerController.cs:1087-1112`).
- The cards of the read-only designer show the seeded titles (**Check stock**, **Wait for payment**), not the type names.

## 2. The bulk actions on screen

Checked: the labels and messages (`WF/Controllers/WorkflowController.cs:318-323, 556-564, 604-606`), the toolbar markup
(`OrchardCore.Admin/Views/AdminListToolbar.cshtml:63-76`) and the script that shows the **Actions** button (`IDXJS`). Confirm:

- ticking two rows shows the **Actions** button (`#bulk-action-menu-button`) and hides the drop-downs, and the menu holds **Retry**, **Cancel**
  and **Delete** (`a.dropdown-item[data-action=...]`); `#select-all` ticks every row of the page;
- each choice opens a dialog titled **Bulk Action** with "Are you sure you want to retry these items?" (or "cancel"), whose **Ok**
  button is `#modalOkButton` (`OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`);
- after **Retry**, the list under the **Faulted** filter is empty ("Nothing here!") with the message "2 faulted instances ran
  again.": the redirect keeps the filter of the form (`WF/Controllers/WorkflowController.cs:464-486`). If the page shows another
  filter, the end frames described for `bulk-2` and `bulk-3` change, and the plan must say so.

## 3. The activity panel and the retry on the instance page

Checked: selecting a record of the **Journal** tab opens the panel of its activity, on **Runs** with that run open
(`DES/panel/WorkflowPanel.vue:192-198`, `DES/panel/ActivityPanel.vue:136-141`); **Retry from here** is in the panel's title bar of a faulted
instance (`DES/panel/ActivityPanel.vue:514-523`); the dialog and the message after it (`WF/Services/WorkflowsDesignerJSLocalizer.cs:317-321`).
Confirm that the panel opens as described for `open-5`, `fault-4` and `fault-5`, that the **Details** tab can then be selected, that
the **Retry** button of the dialog is `#modalOkButton`, and that the message "The instance ran again: Finished." is on screen during
the first sentence of `fault-6` (the plan holds 7 seconds before the reload; the message lasts 5).

## 4. Scroll amounts and the admin menu

- The read-only designer is `min(100vh - 11rem, 40rem)` high (`DES/styles/designer.css:48-51`), so about 640 pixels at 864. The plan scrolls
  250 pixels to bring the whole designer into the frame (`open-2`, `fault-2`) and 600 to reach the bottom buttons (`fault-6`). Probe the page and
  adjust the numbers so that the designer's toolbar is at the top of the frame and its right panel and the bottom panel are visible.
- The **Workflows** link of the admin menu: its item has the id `workflows` (`WF/AdminMenu.cs:21`), and the plan uses `li#workflows a`. Probe the menu's
  markup, and fix the selector if the id is on another element. Probe also the other selectors of the plan (`[data-cy=...]` come from the
  designer's sources and views; `.workflow-details` from `WF/Views/Workflow/Details.cshtml:31`).

## 5. The text of the Fault Message

The **Fault Message** is the message of the exception the activity threw (`OrchardCore.Workflows.Abstractions/Models/WorkflowExecutionContext.cs:339-343`).
For a call to a port nobody listens on, it is the message of the HTTP client and of the operating system: its exact words depend on
both. The narration never quotes it. Before recording, read it on a contact sheet: it must be one readable line, name only
`127.0.0.1:5099`, and hold no machine name, user name or proxy address.

## 6. The proxy and the loopback address

recording.md asks for a dead `HTTP_PROXY` and `HTTPS_PROXY`. This video's only outbound call is from **Check stock** to the file server on
`127.0.0.1:5099`, so recording-plan.md also sets `NO_PROXY=127.0.0.1,localhost`. Confirm that the first two runs end Finished. If
they end Faulted with a proxy error, start the site without the two proxy variables for this video: the site sends no mail, SMS or payment
request (no such feature is enabled), so nothing else could leave the machine.

## 7. The run opened in the Runs tab shows no data

The workflows of the seed do not record activity data (it is off for a workflow that a recipe creates without it: `RecordActivityData` in
`OrchardCore.Workflows.Abstractions/Models/WorkflowType.cs`, and the documentation says the same). The run opened in `fault-4` then shows its error and the note
"No data was recorded for this run..." (`WF/Services/WorkflowsDesignerJSLocalizer.cs:158`). This is intended: turning the setting on would show the expressions of the
activities, which are code, in an admin video. The narration of `fault-4` only speaks of the error.

## 8. The Running group, and an instance that was not saved

The counts bar and the **Status** drop-down group four statuses under **Running**: Idle, Starting, Resuming and Executing
(`WF/Controllers/WorkflowController.cs:33-40, 191`). The narration of `status-1` and its slide name the four, as the code has them.
The video shows no instance in one of them: an instance is saved when a run ends, and a start whose first activity cannot execute
returns before saving (`WF/Services/WorkflowManager.cs`, `StartWorkflowCoreAsync`, status Idle). Confirm that the counts bar reads
**Running 0** in the seeded snapshot, and that no row shows Idle, Starting, Resuming or Executing.

## 9. The journal is on, and finished instances are kept

The Journal tab, the outcomes in it, the **Runs** tab and the bulk **Retry** (which finds the faulted activity in the journal) need the
journal, which is on unless the `Workflows:Journal` section turns it off (`OrchardCore.Workflows.Abstractions/Services/WorkflowJournalOptions.cs`,
`Enabled` defaults to true; `WF/Startup.cs:69`). The sentence "Every time a workflow runs, Orchard Core creates an instance" holds for the
seed because both workflows leave **Delete finished workflows** off (`DeleteFinishedWorkflows` is false in the seed): with it on, an instance is
deleted when it finishes (`WF/Services/WorkflowManager.cs`, `CompleteRunAsync`). Confirm that the **Journal** tab of each seeded instance lists records.

## 10. The activity panel covers the lower part of the designer

Unpinned, the activity panel is placed over the lower 40% of the read-only designer (`DES/styles/designer.css:189-203`). Confirm, in the frames at the end of
`open-5`, `fault-4` and `fault-5`, that the three activities of the workflow are still visible above it (the designer fits the workflow to its frame before the
panel opens, `DES/App.vue`, `canvas.fit()`). If they are not, pin the panel before the capture (the pin button of its title bar, `LOC` line 149)
and say so in the plan.

## 11. Green and blue on the activity that waits

**Wait for payment** is both an activity the instance ran (a journal record with the status Waiting, so a green border and a green mark on its header) and the
activity it waits on (the **Blocking** badge and a blue ring): `DES/styles/designer.css:1195-1198, 1279-1285`. The narration says only that what the instance ran is
highlighted in green and that the badge marks the activity it waits on. Confirm on screen that both read clearly in the frame of `open-3`.

## 12. The text on the Workflow drop-down of the toolbar

On the page of every workflow's instances the first drop-down has the title **Workflow** and the choices **All workflows** and the workflow names
(`WF/Views/WorkflowInstancesFilters.cshtml:16`, `WF/Controllers/WorkflowController.cs:312-317`). The choice **All workflows** has an empty value, and the
drop-down is not set to show a fixed text (the other three are: `data-selected-text-format="static"`), so the button may read **All workflows** instead of
**Workflow**. The narration of `list-3` names the filter by what it filters ("workflow, status and creation date"), not by the button text. Confirm
what the button reads in the frame at the end of `list-3`.


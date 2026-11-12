# Open questions: Workflows: Events, Tasks and Outcomes

What the author could not check from the Orchard Core source (commit `92db4a83`) or the documentation, and what a
person must confirm on a running site while recording. The narration states nothing in this list as a fact that
the source does not show; each item says what was checked and what remains.

## 1. The shared designer package is not in the source snapshot

The designer imports its shared components from `@bloom/...` (`.scripts/bloom`), a folder the snapshot does not
contain (`Assets/designer/vite.config.ts:12`). So these were read from the documentation only, or not at all:

- **When the settings of an activity are applied.** `docs/.../Workflows/README.md` (section "Drafts and Publishing")
  says they are applied when you leave a field and when you select another activity; `ActivityPanel.spec.ts`
  shows the panel applies on close. The plan presses Tab after each value. Confirm that the node on the canvas
  changes within a second or two (the Fork Task's two ports in `outcomes-4`, the Notify message in `tasks-2`, the
  Log text in `tasks-4`, "Page" in `event-5`). If it does not, find what applies the form (the localizer has
  "Apply", "Applying..." and "Saved!": `WorkflowsDesignerJSLocalizer.cs:132-134`) and add that click to the step.
- **The canvas controls.** The localizer has "Fit to content", "Zoom in", "Zoom out" and "Reset zoom to 100%"
  (`:355-358`), but the control itself is in the shared package. Find its selector with `Recorder probe`. The
  narration never mentions it; it is only needed if a node is cut off.
- **The toast and its duration.** `run-1` expects "The workflow was published as version ..." for a moment. The
  narration only relies on the version pill in the toolbar, which stays.
- **The preference key.** `createPreferences("orchardcore:workflows-designer:")` is read from
  `Assets/designer/src/ui/preferences.ts:4`; its helper is in the shared package. The plan sets
  `orchardcore:workflows-designer:panel-collapsed` to `true` in an init script. Confirm that the designer opens with
  its right panel collapsed to a rail. If the key is different, collapse the panel once with its "Collapse the
  panel" button and read the key from `localStorage`.

## 2. Code-looking text the product prints on the screens of this video

The programme asks an admin video to show no code. These texts are plain product text, not code, but a reviewer
should decide whether they are acceptable on screen:

- The hints under **Message** (Notify Task) and **Text** (Log Task) end with "With Liquid support."
  (`Views/Items/NotifyTask.Fields.Edit.cshtml:19`, `LogTask.Fields.Edit.cshtml:19`). They are visible in `tasks-2` and
  `tasks-4`.
- The descriptions of the cards of the Activities pane name JavaScript and Liquid (If/Else, For Each, For Loop,
  Liquid, Set Output, Set Property, Set Variable, While Loop: their `*.Fields.Thumbnail.cshtml`). The plan types
  "log" in the search first and then selects Tasks, so only the Log card is listed (`tasks-3`): the pane opens every
  category as soon as a filter or a search text is set (`ActivityToolbox.vue:17,19`), so selecting Tasks with an
  empty search would list those cards. Confirm on the first take that no card with such a description is on a frame.
- The first paragraph of the **Variables** tab of the right panel prints `variable("name")` and
  `{{ Workflow.Variables.name }}` (`WorkflowsDesignerJSLocalizer.cs:69`), and the tab is the one open by default
  (`WorkflowPanel.vue:59`). The plan keeps the panel collapsed. If the recording shows that paragraph at any
  moment, even for one frame, record the clip again.
- The condition of an If Else Task is typed in a box whose placeholder is a JavaScript or a Liquid example
  (`Views/Items/IfElseTask.Fields.Edit.cshtml`). The video does not open it; it only names the task's two outcomes
  on a slide (`blocks-3`).

## 3. Behaviors the narration describes that are only confirmed on a site

- **`outcomes-1`: the plus on an unconnected port.** It comes from the stylesheet (`designer.css:1466-1480`): the
  dot grows and shows a plus only while the pointer is over a port that has no connection. Check that the
  capture shows it (the cursor must rest on the port for a second).
- **`run-4`: the notification.** `NotifyTask` adds the message to the notifier (`Activities/NotifyTask.cs:62`)
  during the request that publishes the page, which redirects to the Content Items list. The author did not read the
  notification filter that carries it across the redirect. Confirm that the green message appears on the list,
  next to "Your Page has been published.", and that both stay visible for the eight seconds of the step. If the
  messages disappear on their own, record the step when they appear, with the cursor still.
- **`tasks-2`: "Success".** The Notification Type select starts on its first value, Success
  (`NotifyEntry.cs:8-13`; the property has no other default, `NotifyTask.cs:42-46`). Confirm what the select shows
  in a new Notify Task, and that the header of the node reads "Success Notification". If it shows another value,
  change the narration of `tasks-2` ("Let's keep Success") and the typed message accordingly.
- **`tasks-5` and `tasks-6`: the badge.** The warning "This activity can't be reached from a start activity, so it
  never runs." is produced when the draft is validated (`WorkflowTypeDraftManager.cs:529-533`). Confirm that the badge
  on the Log Task counts exactly one issue after its text is typed (an empty required Text counts as a second, an
  error: `WorkflowTypeDraftManager.cs:402-422`), and that it goes away once the Audit outcome is connected. The save
  of the connection returns the issues, and the store applies them (`Controllers/WorkflowDesignerController.cs:459`,
  `state/designerStore.ts:285-288`); the delay of the automatic save comes from the shared package, so confirm the
  badge is gone within a few seconds.
- **`tasks-1`, `tasks-3`: red badges before the narration explains them.** The Message of a Notify Task and the Text
  of a Log Task are required and empty when the activity is added, so each node shows a red badge until its value is
  typed (the next step in both cases). Confirm it, and that the badge of the Log Task is amber and counts 1 at the end
  of `tasks-4`, as `tasks-5` says.
- **`tasks-6`: the drag.** The drag from a port to a node is a pointer gesture of the shared canvas, which is not in
  the snapshot (`ConnectOutcome` in the localizer says "drag to an activity to connect it"). Confirm that
  `DragToAsync` makes the connection; if the canvas needs intermediate pointer moves, press the mouse on the port,
  move in several steps, and release on the node.
- **`run-1`, `run-5`: version numbers.** Creating a workflow saves its first version, empty
  (`WorkflowTypeStore.cs:81-87`, `WorkflowTypeVersionStore.cs:95-111`), and the designer shows the version the
  workflow type points to (`WorkflowDesignerController.cs:994-997`). Confirm that the pill reads "Version 1" from the
  first frame of the designer, reads "Version 2" after **Publish**, and that the Instances page says "Version 2". The
  narration does not state any number: it says only that the version shown in the toolbar changes.
- **`run-1`: no confirmation box.** Publishing opens a box ("Publish the workflow?") only when the draft has
  warnings (`draft/publishDecision.ts`; the shared `decidePublish` is not in the snapshot). The finished workflow
  should have none: its validation rules are in `WorkflowTypeDraftManager.cs:425-538`, and none applies to it. If the box opens, find the warning
  (the Issues tab, `WorkflowsDesignerJSLocalizer.cs:126`) and fix the workflow, not the narration.
- **`run-5`, `run-6`: the instance.** The workflow keeps its finished instances unless **Delete successfully
  completed workflows** is checked, which it is not by default (`Views/WorkflowType/Create.cshtml:62-66`). Confirm that
  the list shows one Finished instance on the published version, and that the page of the instance shows the
  executed path highlighted on all four activities. The page has a details card above the read-only designer
  (`Views/Workflow/Details.cshtml`); the plan scrolls down to the whole canvas. Confirm the canvas fits.
- **`run-2`: the New button.** The Blog recipe makes Article, Blog and Page creatable
  (`blog.recipe.json:136,248,448`); Blog Post is not, which is why the workflow filters on Page. Confirm that the
  **New** drop-down lists Page, and that the editor of a Page asks for nothing but a title before **Publish**.

## 4. Things the narration says that come from the engine, not from the screen

- An event that is the start activity runs at once, and one anywhere else makes the workflow wait
  (`blocks-1`). Read from `Services/WorkflowManager.cs:662` (the first pass resumes the event; later events block)
  and `OrchardCore.Contents/Workflows/Activities/ContentEvent.cs:46` (the event halts). The video does not show a
  waiting event; the instances and event videos can.
- A Fork Task follows all of its outcomes (`outcomes-5`): `Activities/ForkTask.cs:40` returns every fork name, and
  with the default setting **Follow the first transition only** each outcome has one connection, which is the case
  here. Confirm on the instance page (`run-6`) that both branches are highlighted.
- If Else Task has True and False, and Email Task has Done and Failed (`blocks-3`): `Activities/IfElseTask.cs:63`,
  `OrchardCore.Email/Workflows/Activities/EmailTask.cs:139`. Neither is shown on screen. The Email Task needs the
  Email feature, which this video does not enable.
- "Only an event can be the start activity": the designer offers **Set as start activity** only on events
  (`DesignerCanvas.vue:266-271`), and the first event added becomes the start (`WorkflowTypeDraftManager.cs:175`);
  the server copies the start flag it receives without checking the type (`WorkflowTypeDraftManager.cs:136`). The
  narration says only what the designer does.

## 5. Selectors to find with `Recorder probe`

The Content Types picker (a bootstrap-select with its list attached to `body`), the issue badge of a node, the
rail of the right panel, the link of an instance in the Instances list, and the entries **Design**, **Workflows**,
**Content** and **Content Items** of the admin menu (whether a parent entry opens by a click or by pointing at it) are
described by their markup in the source, but their final DOM comes from scripts the author could not run. Check each locator in the plan against the
page before the first take.

# Open questions: Write a Workflow Activity

What was checked in the source of Orchard Core at commit `92db4a83`, and what a person must confirm on a running site or with
a compiler before the video is recorded. Nothing below is stated in the narration as a fact that was not read in the source.

## Code on the slides

The code was not compiled. It was copied from the source, and differs from it only in the ways listed here.

- **Line breaks and trims.** Long signatures are broken over several lines (`class-1`, `class-4`, `driver-1`, `expr-1`,
  `event-1`, and the `<h4>` of the Design view in `views-1` and `views-2`), and what is left out is marked `// ...`. The
  statements are those of `src/OrchardCore.Modules/OrchardCore.Workflows/Activities/NotifyTask.cs`,
  `src/OrchardCore.Modules/OrchardCore.Workflows/Drivers/NotifyTaskDisplayDriver.cs`, `src/OrchardCore.Modules/OrchardCore.Workflows/ViewModels/NotifyTaskViewModel.cs` and `src/OrchardCore.Modules/OrchardCore.Workflows/Views/Items/NotifyTask.Fields.*.cshtml`. In the
  Design view, the `<h4>` is spread over three lines where the original has one; the rendered text may differ by a space that
  the CSS does not show. `class-2` shows the constructor with its body replaced by `// ...`, and without the fields.
- **Event slides (`event-1`, `event-2`)** come from `src/OrchardCore.Modules/OrchardCore.DataPipelines/Workflows/DataPipelineRunCompletedEvent.cs`
  and `WorkflowDataPipelineRunHandler.cs`, trimmed with `// ...`. They need the Data Pipelines module to exist at the commit that
  is recorded against.
- **Expression slides (`expr-1`, `expr-2`)** show the real `IfElseTaskViewModel`, the `UpdateAsync` method of the real
  `IfElseTaskDisplayDriver` and the `IfElseTask.Fields.Edit.cshtml` view, trimmed. In `UpdateAsync`, the `Validate<bool>(...)` call
  is broken over three lines, and the two `activity.Properties.Remove(...)` lines of the former syntax settings are hidden by
  `// ...`. The `EditActivity` method is not shown: the built-in drivers wrap its call in
  `WorkflowExpressionSyntaxes.Resolve(...)` for activities saved before a syntax could be chosen for each expression, which a new
  activity does not need.
- **The documentation's own Notify Task example is not used**, because it does not compile against the source (see the
  documentation issues reported with this video).

## To confirm on a running site (clip `06-designer`)

The designer code was read (`src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src`), but the screen was not seen. Confirm each point on a first take:

1. **`designer-1`**: the **Design** group of the admin menu can be opened so that **Workflows** is visible on the list page. The
   narration says "Under Design, Workflows". A workflow that exists only as a saved draft is listed and opens in the designer.
2. **`designer-2`**: typing `notify` leaves exactly one card, the Notify Task, under **UI**, with the category open without a
   click. This holds only when Notifications is disabled.
3. **`designer-3`**: a click on the **Done** port of the event opens the list (the port opens it on a click, and starts a
   connection on a drag). The new node lands to the right of the event, connected, and the activity panel opens by itself.
4. **`designer-4`**: the node shows an issue badge because **Message** is required. The issue is produced by replaying the
   editor (`src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowActivityEditorValidator.cs`), so confirm that the badge is there with the empty message, and that
   its only cause is the empty message. The default type is **Success** (the first value of `NotifyType`), with the check-circle
   icon and the title "Success Notification".
5. **`designer-5`**: the panel shows **Settings** and **Available data** and no **Outputs** tab; **Title** comes first and
   **When it fails** last. The panel title shows "Notify Task" twice (the title and the small display text), because no
   custom title is set; check that this does not look like an error.
6. **`designer-6`**: selecting **Information**, typing the message and pressing Tab applies the change (the documentation says
   changes are applied when the field is left), and the node body then shows "Information Notification" and the message. The
   server-side form host of the panel is in a shared package that is not part of the source that was read, so the exact moment of
   the apply (on change, or on blur) is not verified.
7. **Layout**: the activity panel covers the bottom of the canvas. Check that the Notify Task node is not hidden behind it in
   `designer-4` and `designer-6`; the designer keeps a new node visible
   (`src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/App.vue:277`), but its height depends on the window.
8. **Nothing is run**: the clip does not run the workflow, because where the notification appears after a run is not
   verified. The narration does not say it.

## Pronunciation

The `spoken` map of `storyboard.json` only inserts spaces into identifiers (for example "Execute Async") and spells
`OrchardCore.Workflows` as "Orchard Core dot Workflows". `narrate.py` and `pronounce.py` were not run (they make audio). A person
must listen to the identifiers once, in particular "Async", "Thumbnail", "I Workflow Manager" and "I Activity Description".

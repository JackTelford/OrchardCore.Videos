# Open questions: workflows-overview

The narration holds only what the source and the documentation show. These points depend on how the pages behave on
a running site, which was not available while the content was written. Each says what was checked, and what a person
must confirm while recording (and fix in the narration, the plan or both if it differs). Paths are in the Orchard Core
checkout the labels were read from.

## 1. The notifications after the Article is published (`run-2`)

*Narration:* "The Notify Task shows its message, next to the notification Orchard Core adds when content is published."

*Checked:* `NotifyTask.ExecuteAsync` adds a notification with `INotifier` (`src/OrchardCore.Modules/OrchardCore.Workflows/Activities/NotifyTask.cs:59-64`);
the content editor adds "Your Article has been published." after `PublishAsync` (`.../OrchardCore.Contents/Controllers/AdminController.cs:961-968`),
and `PublishedAsync` triggers the event inside that call (`.../OrchardCore.Contents/Workflows/Handlers/ContentsHandler.cs:34-37`).
Editing a published item creates a draft first (`VersionOptions.DraftRequired`, `AdminController.cs:816`), so `PublishAsync`
does not return early.

*Confirm:* that both success notifications show on the **Content Items** list after the redirect, and in which order
(the Notify Task's first is expected). If only one shows, change `run-2`.

## 2. The notification after enabling the feature (`enable-3`)

*Checked:* `AdminController.Enable` passes one feature, and the message is built from "The feature {1} was enabled."
(`.../OrchardCore.Features/Controllers/AdminController.cs:247,317`). `FeatureService.EnableOrDisableFeaturesAsync` hands
the callback only the features it was given, not the dependencies it enables with them (Liquid and Scripting, if they
are not enabled yet), so the plan expects the singular "The feature Workflows was enabled."
(`src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:74-83`). The Features search matches the name,
identifier and description of each feature (`Drivers/FeatureEntryDisplayDriver.cs:20`); the only other feature whose
description mentions workflows is **Data Pipelines** (`OrchardCore.DataPipelines/Manifest.cs:23`), so that row is
expected under its **Data** heading, above the **Workflows** group: the page sorts the groups by category name
(`.../OrchardCore.Features/Controllers/AdminController.cs:91`) and the rows of a group by name (`:94`), so the
**Workflows** group reads HTTP Workflows Activities, Session Workflows Activities, Timer Workflows Activities, then
Workflows, which is the order `enable-2` names the three activity features in. The narration names only the four features of the
**Workflows** category, which the manifest shows: `OrchardCore.Workflows/Manifest.cs:13-38`.

*Confirm:* the exact text, and that the link `#btn-enable-OrchardCore_Workflows` exists (the id is built with
`Html.GenerateIdFromName`, `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the framework's default replaces dots with an
underscore, and that code is not in the Orchard Core source). Also confirm which rows the Features search leaves
visible for `workflows`, and the count the summary shows (expected: the four Workflows rows and Data Pipelines).
After the enable, the redirect goes to the plain Features page, with no search (`AdminController.cs:290-312`), so the
plan types `workflows` again to bring the notification and the Workflows rows into one screen: confirm that the
notification stays visible above the groups, and that the Workflows row then shows **Disable**.

## 3. The Content Types picker (`build-2`)

*Narration:* "Content Types limits the event to the types you choose. Let's pick Article, so other content does not start
the workflow."

*Checked:* the editor renders `SelectContentTypes` in `Picker` mode, a multi-select with a live search, enhanced by the
`Selectpicker` global of the bootstrap-select resource (`.../OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Picker.cshtml`,
`.../OrchardCore.Contents/Assets/ts/content-type-picker.ts`); it is appended to the page body (`data-container="body"`). An
empty selection means any content type (`ContentEvent.CanExecuteAsync`, `.../OrchardCore.Contents/Workflows/Activities/ContentEvent.cs`).

*Confirm:* the markup of the opened list in the headless capture (use `Recorder probe`), so the plan's clicks can be
written as locators, that the list is drawn in the capture, and that the change is applied when the list closes (the
card's body must then read "Article"). If the open list is not drawn, leave the picker empty and change `build-2` to
say that the event applies to any content type.

## 4. The problem flagged on a Notify Task without a message (`build-5`)

*Narration:* "A message is required, so the card and the Issues tab flag a problem."

*Checked:* `Message` is `[Required]` (`.../OrchardCore.Workflows/ViewModels/NotifyTaskViewModel.cs:10`), an activity whose editor
does not validate is reported as an error (`WorkflowTypeDraftManager.cs:412-421`), and the documentation says the same
(Workflow Designer, the **Issues** tab). The designer shows an issue count on the card (`Assets/designer/src/canvas/ActivityNode.vue:259`)
and on the **Issues** tab (`Assets/designer/src/panel/WorkflowPanel.vue:341-346`).

*Confirm:* that both counts show right after the task is added, and clear once the **Message** field is left. Right
after the pick from the quick-add list, the server's issues may also hold the warning "This activity can't be reached
from a start activity, so it never runs." until the transition is saved (`WorkflowTypeDraftManager.cs`, the unreachable
rule), so the count may read 2 for a moment. The step waits for the badge to go, and the narration does not mention it.
Also confirm that a validation message of the required field does not put code or odd text in the Issues list.

## 5. What the designer does when an activity is added (`build-1`, `build-4`)

*Checked:* the first event added is marked as the start activity (`WorkflowTypeDraftManager.cs:175`), the activity panel opens
by itself when the activity has an editor (`Assets/designer/src/App.vue:273-275`), and an activity picked from the list
after an outcome is added to its right and connected (`App.vue:309-326`). The default **Notification Type** is
**Success**, the first value of `NotifyType` (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifyEntry.cs:8-14`).

*Confirm:* all four on screen, and that the start panel offers **Content Published Event** (it is the first of the
suggested events in `StartPicker.vue`, when the **Contents** feature is enabled; the buttons shown depend on the features
enabled, so the narration never lists them).

## 6. Publishing (`build-6`)

*Checked:* the toast text is "The workflow was published as version {0}." and the toolbar shows a "Version {0}" badge
(`WorkflowsDesignerJSLocalizer.cs:218,221`; `App.vue:358,648-656`). The narration says only that Publish makes the draft
the version that runs, and never gives a number. The number comes from the code: creating a workflow saves it through
`IWorkflowTypeStore.SaveAsync` (`WorkflowTypeController.cs:271-277`), which creates its version 1, an empty definition
(`WorkflowTypeStore.cs:87`, `WorkflowTypeVersionStore.cs:90-114`), so the empty designer already shows a "Version 1"
badge and the first Publish creates version 2 (a different fingerprint). The documentation says versions are numbered
from the first publish (see the doc issues of this video).

*Confirm:* the badge "Version 1" on the empty workflow, the toast "The workflow was published as version 2." and the
badge "Version 2" after the first Publish, and that no confirmation dialog opens (it opens only for warnings,
`Assets/designer/src/draft/publishDecision.ts`). A new workflow whose only activities are connected should have none.

## 7. The instance (`instances-1`, `instances-2`)

*Narration:* "The page counts the instances by status, and this one is Finished." and "Let's open it to see the activities that ran, highlighted on the diagram."

*Checked:* the **Finished** count and status filter exist (`.../OrchardCore.Workflows/Views/Workflow/Index.cshtml:39-44`); the
instance page shows the executed path of its journal in the read-only designer (documentation, "Execution Journal"; the
`is-executed` class in `ActivityNode.vue`), and the journal is enabled by default (`src/OrchardCore/OrchardCore.Workflows.Abstractions/Services/WorkflowJournalOptions.cs:11`);
finished instances are kept unless **Delete successfully completed workflows** is checked, and the **Create Workflow**
page leaves it unchecked (`WorkflowTypeController.cs:249-255`; `ViewModels/WorkflowTypePropertiesViewModel.cs:33`).

*Confirm:* that the single instance is **Finished** (the code ends an instance as Finished when nothing blocks it:
`WorkflowManager.cs`, `ExecuteWorkflowAsync`; the start event resumes at once on the first pass), that
the **Instances** badge shows in the workflow's row, and that both cards are highlighted on the instance page.

## 8. The admin menu entries

*Checked:* **Tools** > **Features** (`OrchardCore.Features/AdminMenu.cs:26-28`), **Design** > **Workflows**
(`OrchardCore.Workflows/AdminMenu.cs:18-23`), **Content** > **Content Items** (`OrchardCore.Contents/AdminMenu.cs:65,68`).

*Confirm:* that the entries are reachable by clicking those labels in this order in the admin theme (the group opens, then
the entry), and that the **Design** group holds the **Workflows** entry once the feature is enabled.

## 9. The Article in Content Items (`run-1`)

*Narration:* "Under Content, Content Items, Edit opens an article in the editor, which has a Publish button."

*Checked:* the Content Items list shows only listable content types (`.../OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs:182`).
In the Blog recipe the `Article` type sets `Listable` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:131-140`) and
the `BlogPost` type does not (`:188-197`), so a Blog Post is not listed there. The recipe creates one Article, "About"
(step `content`). The video therefore uses an Article, not a Blog Post.

*Confirm:* that the row "About" shows in the list with its **Edit** button, and that **Publish** on the edited Article
returns to the list (the **Edit** link carries a return address, `AdminController.cs:849-860`).

## 10. Code on the Variables tab (`05-designer`, `06-build`)

*Checked:* the workflow panel opens on its **Variables** tab (`Assets/designer/src/panel/WorkflowPanel.vue:59`), whose body
starts with a hint that holds a JavaScript call and a Liquid expression (`WorkflowsDesignerJSLocalizer.cs:69`,
`Assets/designer/src/panel/VariablesTab.vue:223`). An admin video shows no code, so the plan opens both clips on the
**Issues** tab and never selects **Variables**.

*Confirm:* on every contact sheet of these two clips, that no frame shows that hint, and that the tooltip of the tab (its `title`)
is not drawn in the capture.

## 11. The hint "With Liquid support." (`build-4`, `build-5`)

*Checked:* the **Message** field of the Notify Task has the hint "The message to show. With Liquid support."
(`.../OrchardCore.Workflows/Views/Items/NotifyTask.Fields.Edit.cshtml:19`). It is a label of the screen, not code, and
the narration never reads it; the typed message is plain text.

*Confirm:* with the maintainer whether a screen that names Liquid in a hint still counts as an admin video that shows no
Liquid. If it does not, the clip needs a crop or a blur on that line (the plan's choice is to leave it).

## 12. Pronunciation

No `spoken` entry was added: the narration holds no dotted name, symbol or generic type, and the skill says the voice
reads acronyms such as SMS and API correctly. After the first narration, listen to "HTTP Workflows Activities"
(`enable-2`, `more-1`), "Notify" (`build-3`, `build-4`) and "If Else Task" (`more-2`), and if the voice misreads one,
add an entry checked with `scripts/pronounce.py`.

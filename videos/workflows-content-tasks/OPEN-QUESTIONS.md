# Open questions

What could not be checked by reading the Orchard Core source (commit `92db4a83`, no running site), and what a person
must confirm on a running site before or while recording. Each item says what was checked and what is left. Nothing
below is stated in the narration as a fact beyond what the source shows, except where an item says "narrated".

## Recording

- **Typing into Content Properties** (`create-3`, `update-3`).
  - Checked: the field is a CodeMirror editor made from a textarea by `OrchardCore.Contents/Assets/ts/content-task-editor.ts`,
    through `initLiquidPatternEditor` of the `@orchardcore/bloom` package, whose source is not in the checkout. The
    textarea id suffix and the file names were checked.
  - To confirm: whether the editor closes brackets and quotes by itself, or indents after a line break, which would
    corrupt the typed text (the plan types it on one line to avoid indentation); that a click on the visible editor
    (`[data-task-editor='content'] .CodeMirror`) gives it the focus, so that the page keyboard (`Control+A`, then the
    text, then Tab) reaches it (`Scene.TypeAsync` is not used, because it clicks the editor's hidden input element
    first); and that Ctrl+A selects the default value in the editor and not the page.
  - Fallback if typing does not work: set the value off camera with the editor's own API
    (`document.querySelector('[data-task-editor="content"] .CodeMirror').CodeMirror.setValue(text)`), then press Tab,
    and on camera only click into the editor and scroll it. The narration of the two steps stays true.
- **When the Settings form of an activity is applied** (every clip that types in the panel).
  - Checked: the Settings tab says "Edit the selected activity. Changes are saved as you make them."
    (`WorkflowsDesignerJSLocalizer.cs:59`); `ActivityPanel.vue` applies the form (`settle`) when another activity is
    selected or the panel closes, and the designer has the texts "Apply", "Applying…" and "Saved!"
    (`WorkflowsDesignerJSLocalizer.cs:132-134`). The form host itself (`ServerFormHost` of `@bloom/designer`) is not in
    the checkout.
  - To confirm: that Tab after typing applies the change by itself (the toolbar then says **All changes saved**), or
    whether an **Apply** button has to be clicked. The plan says to click it if it is there.
- **Which activity panel shows after an add.** `App.vue` (`addActivity`) opens the panel of an activity that has an
  editor (`hasEditor`), and the plan relies on it. Confirm that the panel is open, on **Settings**, in each of
  `create-1`, `update-1` and `publish-1`, and that the **When it fails** section below the fields does not push the
  fields out of view at 1536x864 (the panel can be resized).
- **The Content Types picker** (`event-3`). It is a bootstrap-select with a live search whose menu is attached to
  `body` (`data-container="body"`). Confirm that the recorder can click its options, and that the change is applied
  (the canvas card of the event changes from **Any content type** to **Article**) when the picker closes and Tab is
  pressed.
- **Available data** (`update-2`).
  - Checked: `Create Content Task` provides the property `ContentItem` with the fields of a content item
    (`CreateContentTask.cs:142-146`, `WorkflowValueMembers.ContentItem` with `ContentItemId`), and the tab builds the
    expressions `property("ContentItem")` and `property("ContentItem").ContentItemId` from them
    (`available/availableData.ts`, `expressionsOf` and `memberExpressionsOf`). Only the activities on a path to the
    selected one are listed (`upstreamNodeIds`).
  - To confirm: how the groups and the **Fields** list look on screen (the documentation says each group is a tab, "From
    {0}", with the first group that has values shown first). The narration says only that the tab lists expressions for
    every earlier activity. If the tab shows nothing useful for the step, change the narration, not the tab.
- **Order of the Content Items list** (`run-3`). Checked: the default order is **Recently modified**, newest first
  (`DefaultContentsAdminListFilterProvider.cs:306-307`). To confirm: that the announcement and the article are the two
  newest rows after the run. Open the announcement from its row if the order differs.
- **Publishing the workflow** (`run-1`). Checked: `WorkflowTypeDraftManager.Validate` has no rule for an outcome that
  leads nowhere, so **Failed** and **Noop** left unconnected raise no warning. Not checked: the errors that
  `ValidateActivitySettingsAsync` finds by replaying each editor. Confirm that the **Issues** tab is empty for the four
  cards, so that no "Publish the workflow?" dialog opens.
- **The port to click.** The plan uses `.wfd-port:not(.is-connected)[data-cy$='-Done']`, which is unique at each step
  because the **Done** ports of the earlier cards are connected (`OutcomePort.vue:27`). Confirm that the class
  `is-connected` is on a port as soon as its transition is drawn.

## Behavior at run time

- **The whole chain** (narrated in `create-1` to `run-3`).
  - Checked: publishing an article runs the workflow inside the request. The event gives `Content Item` and `Content
    Event` as inputs (`ContentsHandler.cs:54-71`). Create Content Task creates a draft of Announcement and stores it as
    the property `ContentItem` (`CreateContentTask.cs:105-125`). Update Content Task evaluates
    `property("ContentItem").ContentItemId` as JavaScript (`ContentActivity.GetContentItemIdAsync`,
    `JavaScriptWorkflowScriptEvaluator`), loads the draft (`VersionOptions.DraftRequired`) and merges the JSON
    (`UpdateContentTask.cs:98,131`). Publish Content Task evaluates `property("ContentItem")`, which comes back as the
    content item (`ContentActivity.GetContentAsync`), finds that it has a draft (`HasDraft`: not published or not the
    latest version) and publishes it (`PublishContentTask.cs:33-58`).
  - To confirm: that the announcement ends up published, with the title and the body, and that the Jint engine reads
    `ContentItemId` of the .NET object as the plan expects.
- **Liquid values in the body** (`update-3`, narrated).
  - Checked: the workflow registers `Input`, `Output`, `Properties` and `Variables` of `Workflow` for Liquid
    (`OrchardCore.Workflows/Startup.cs:50-55`); the Liquid documentation says a template reads the public properties of
    any object reachable from its model, matched without regard to case (`Liquid/README.md`, "Extending Liquid from a
    module"); the Workflows documentation gives `{{ Workflow.Input.ContentEvent.ContentType }}` and
    `{{ Workflow.Input.ContentItem.DisplayText }}` ("Liquid Expressions and ContentItem Events").
  - To confirm: that the body reads "The Article Pumpkin Soup in Autumn was published." If the content type renders
    empty, replace it with another documented value.
- **Liquid output is encoded** (narrated in `properties-1`). Checked: `CreateContentTask` and `UpdateContentTask` pass a
  `JavaScriptEncoder` to the workflow expression evaluator (`CreateContentTask.cs:99`, `UpdateContentTask.cs:130`), and
  `LiquidWorkflowExpressionEvaluator` renders the template with the encoder it is given (`:76`). To confirm: publish an
  article whose title holds a double quotation mark and check that the announcement is still created.
- **The loop guards** (narrated in `limits-1`). Checked in `CreateContentTask.cs`, `UpdateContentTask.cs` and
  `PublishContentTask.cs`: an `InvalidOperationException` is thrown, and `WorkflowManager.cs:762-772` puts the instance
  in the faulted state (`workflowContext.Fault`) when the task has no retry policy. To confirm: publish an article with
  a workflow that creates an Article, publishes it, and starts from Content Published Event for Article, and look at
  the instance in **Instances**. Also confirm whether the article itself is still published when the workflow faults;
  the video does not say.
- **"By default"** (narrated in `merge-2`, `limits-1` and the `limits-2` slide). Every task has a **When it fails**
  section (`ActivityRetryPolicy.Edit.cshtml`) with **Retries** and **Once the retries are spent**. Checked: with the
  defaults (0 retries, **Fault the workflow**) the policy is not stored and `GetRetryPolicy` returns null
  (`ActivityRetryPolicyDisplayDriver.cs:75-92`, `ActivityExtensions.cs:19-30`), so `WorkflowManager.cs:762-772` faults
  the instance. To confirm: that a task added in the designer shows 0 retries and **Fault the workflow**.
- **A Failed outcome and the session.** On invalid content both `CreateContentTask` (`:132`) and `UpdateContentTask`
  (`:176`) call `_session.CancelAsync()` before they take **Failed**. When the workflow runs inside the request that
  publishes an article, this may also cancel the pending changes of that request. Not narrated; the narration says only
  that the task takes **Failed**, which can be connected to another activity, and that the validation result becomes the
  last result. Confirm what happens to the article, because a developer who connects **Failed** to another activity will
  want to know.
- **Update Content Task without an id.** The card says "Update content provided via the ContentItemId input argument"
  when **Content Item ID** is empty (`UpdateContentTask.Fields.Design.cshtml:9`), but the task throws "failed to
  evaluate the 'ContentItemId'" (`UpdateContentTask.cs:76-77`), because `GetContentItemIdAsync` reads only the expression
  (`ContentActivity.cs:181-196`). Narrated in `merge-2` as what the code does. Confirm by running a workflow that leaves
  the field empty. The card's text is a user interface text to fix, not a documentation page, so it is reported in the
  issues of this video's review and not in the documentation issues.

## Slides and wording

- **The recipe slide** (`export-1`).
  - Checked: the step name `WorkflowType` (`WorkflowTypeStep.cs`), the export that builds it from the published
    workflow (`WorkflowTypeController.ExportWorkflows`, `AllWorkflowTypeDeploymentSource.cs:28-49`), the file name
    `Recipe.json` in the zip (`DeploymentPlanResult.cs:38`), the property names of an activity record (`ActivityId`,
    `Name`, `X`, `Y`, `IsStart`, `Properties`) and of a transition (`SourceActivityId`, `SourceOutcomeName`,
    `DestinationActivityId`), as in `workflows-variables-samples.recipe.json` and the models of
    `OrchardCore.Workflows.Abstractions`.
  - To confirm: the property `Content` of Publish Content Task, holding `{ "Expression": ... }`, is the name of the C#
    property (`ContentActivity.cs:52-56`); the expression is stored without a `Syntax`, since the content drivers do not
    set one and null values are not written (`JOptions.cs:24`). A transition may also hold an `Id`. Export the published
    workflow with **Export** in the designer, and compare the activity and the transition with the slide; the ids and the
    `X` and `Y` values in the slide stand for those the designer generates.
- **Names of activities.** The narration says Create Content Task, Update Content Task, Publish Content Task and Content
  Published Event: the display names, which are the headers of the cards and the entries of the quick-add menu. The
  activities pane of the designer shows the shorter Create Content, Update Content and Publish Content (the thumbnails)
  and the body of a card has them as headings. Confirm that the first screen of each clip does not mislead a viewer; if
  it does, add a caption with `HighlightAsync`.
- **The name of the draft-saved event.** The display name is "Content Save Draft Event" (`ContentDraftSavedEvent.cs:22`)
  while its thumbnail and card say "Content Draft Saved". The `concepts-1` slide writes "Save Draft", as the display
  name does; the narration says "saved as a draft".
- **Pronunciation.** `storyboard.json` has `"Noop": "no-op"` and `"WorkflowType": "Workflow Type"` in `spoken`. Neither
  was checked with `pronounce.py`. Also listen to how the voice reads "JSON", "Liquid", "ID" in "Content Item ID", and
  "Content Item" and "Content Event" (written as two words in the narration on purpose).

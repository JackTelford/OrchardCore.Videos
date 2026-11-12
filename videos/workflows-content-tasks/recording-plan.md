# Recording plan: Workflows: Create, Update and Publish Content Automatically

How to record this video without deciding anything. Every label comes from the Orchard Core source at commit
`92db4a83`; `file:line` paths are relative to `src/OrchardCore.Modules/` of that checkout unless they start with
`src/`. A selector below stands for `scene.Page.Locator(...)`; `GetByLabel` stands for `scene.Page.GetByLabel(...)`.
The password is never written here: the recorder reads it from `VIDEO_PASSWORD`, chosen outside git.

Audience: developer. The video is slides with code for the ideas and the source, and browser recordings for the
designer and for the result. There are five recorded clips and six slide clips. The slides are drawn by `slides.py`.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web`, run on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an
  empty folder outside both repositories.
- **Auto Setup** (no setup form and no sign-in form are recorded), through environment variables:
  - `OrchardCore__AutoSetup__AutoSetupPath` = empty
  - `OrchardCore__AutoSetup__Tenants__0__ShellName` = `Default`
  - `OrchardCore__AutoSetup__Tenants__0__SiteName` = `Orchard Core`
  - `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` = `America/Los_Angeles`
  - `OrchardCore__AutoSetup__Tenants__0__AdminUsername` = `admin`
  - `OrchardCore__AutoSetup__Tenants__0__AdminEmail` = `admin@example.com`
  - `OrchardCore__AutoSetup__Tenants__0__AdminPassword` = the value of `VIDEO_PASSWORD` (`<password>`)
  - `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` = `Sqlite`
  - `OrchardCore__AutoSetup__Tenants__0__RecipeName` = `Blog`
- **Tenant**: `Default` only. **Recipe**: Blog (it creates the `Article` content type used by the video).
- **Sign-in**: `Recorder setup` once, outside the capture, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`; it saves
  `state.json` (git-ignored). No clip shows a sign-in form.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address. The video needs no external service, no
  mail server and no local stand-in.
- **Features to enable before recording** (outside the capture), at **Tools, Features** (`/Admin/Features`,
  `OrchardCore.Features/AdminMenu.cs:26-27`): search for `Workflows`, and enable the feature named **Workflows**
  (`OrchardCore.Workflows/Manifest.cs`). Enable none of **HTTP Workflows Activities**, **Timer Workflows Activities**
  and **Session Workflows Activities**. The Blog recipe already enables Contents (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`); Liquid
  and Scripting are dependencies of Workflows (`OrchardCore.Workflows/Manifest.cs`), and are enabled with it.
- **Settings**: none to change. A new workflow records the data of its activities by default
  (`OrchardCore.Workflows/Controllers/WorkflowTypeController.cs:252-253`), which is wanted.
- **Seed data**, created outside the capture. Everything on screen is invented.
  1. The **Announcement** content type. Open **Tools, Deployments, JSON Import**
     (`OrchardCore.Deployment/AdminMenu.cs:18-33`, `/Admin/DeploymentPlan/Import/Json`), paste this recipe in the
     **JSON** field, and click **Import** (the page then shows "Recipe imported successfully!",
     `OrchardCore.Deployment/Controllers/ImportController.cs:185`). It can also be sent with a `fetch` call in the
     `Recorder prepare` command.

     ```json
     {
       "steps": [
         {
           "name": "ContentDefinition",
           "ContentTypes": [
             {
               "Name": "Announcement",
               "DisplayName": "Announcement",
               "Settings": {
                 "ContentTypeSettings": {
                   "Creatable": true,
                   "Listable": true,
                   "Draftable": true,
                   "Versionable": true
                 }
               },
               "ContentTypePartDefinitionRecords": [
                 {
                   "PartName": "TitlePart",
                   "Name": "TitlePart",
                   "Settings": { "ContentTypePartSettings": { "Position": "0" } }
                 },
                 {
                   "PartName": "HtmlBodyPart",
                   "Name": "HtmlBodyPart",
                   "Settings": { "ContentTypePartSettings": { "Position": "1" } }
                 }
               ]
             }
           ],
           "ContentParts": []
         }
       ]
     }
     ```

     The shape is the one of the `ContentDefinition` step of the Blog recipe
     (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, line 128 onward).
  2. No workflow, no article and no other content item is created by hand. The article of the last clip is created on
     camera. The Blog recipe's own content (the blog, its tags and categories) stays as it is.
- **Snapshots**: after the seed, stop the site and copy its `App_Data` folder and `state.json` aside as `snapshot-0`.
  Do the same after each clip that changes the site (see the next section).

## The order of the clips

The five recorded clips build one workflow, and the workflow lives in the site's database: each clip starts from the
state the previous clip left. Record them in this order, and do not skip one.

| Order | Clip | Depends on | Changes the site | Snapshot to keep afterwards |
| --- | --- | --- | --- | --- |
| 1 | `02-event` | the seed (`snapshot-0`) | creates the workflow **Announce published articles**, with its event | `snapshot-1` |
| 2 | `03-create` | `snapshot-1` | adds Create Content Task to the draft | `snapshot-2` |
| 3 | `04-update` | `snapshot-2` | adds Update Content Task to the draft | `snapshot-3` |
| 4 | `05-publish` | `snapshot-3` | adds Publish Content Task to the draft | `snapshot-4` |
| 5 | `06-run` | `snapshot-4` | publishes the workflow, creates an article, and so an announcement | none |

To record a clip again, stop the site, restore the snapshot its row names, start the site, and sign in with the saved
`state.json`. The designer saves a draft as it is edited (the toolbar shows "All changes saved",
`OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:166`), so a draft left by one clip is still there when
the next clip opens the designer. The slide clips (`01-concepts`, `03-properties`, `04-merge`, `05-lookup`,
`07-limits`, `08-export`) are drawn from `slides.py` and need no recording.

Every recorded clip runs in the same browser settings as the others (1536x864, light, `en-US`). Open the designer
before `StartCaptureAsync`, and close the activity panel unless a clip says otherwise.

## Names that appear on screen

The designer shows an activity by two texts, and both are in the code, so both appear in the clips:

- The header of a card on the canvas is the display name of the activity: **Content Published Event**,
  **Create Content Task**, **Update Content Task**, **Publish Content Task**
  (`OrchardCore.Contents/Workflows/Activities/ContentPublishedEvent.cs:22`, `CreateContentTask.cs:40`,
  `UpdateContentTask.cs:45`, `PublishContentTask.cs:24`; the header is `node.displayText` in
  `OrchardCore.Workflows/Assets/designer/src/canvas/ActivityNode.vue`).
- The body of the card starts with a shorter heading from the activity's Design view: **Content Published**,
  **Create Content**, **Update Content**, **Publish Content**
  (`OrchardCore.Contents/Views/Items/CreateContentTask.Fields.Design.cshtml:4`, and the views next to it).
- The narration uses the display names (the full names of the headers), because they are also what the list of the
  quick-add menu shows (`QuickAddMenu.vue`, `activity.displayText`).

## Clip `02-event`: Start from a content event

Starts on **Workflows**, the list of workflows, `/Admin/Workflows/Types`
(`OrchardCore.Workflows/Controllers/WorkflowTypeController.cs:87`). It is empty: "Nothing here! There are no workflow
types for the moment." (`OrchardCore.Workflows/Views/WorkflowType/Index.cshtml`). Before `StartCaptureAsync`, open the
**Design** menu in the admin menu so that **Workflows** is visible and selected
(`OrchardCore.Workflows/AdminMenu.cs:18-19`).

- `event-1`
  - Actions: click **Create Workflow** (`a.btn.create`, `Views/WorkflowType/Index.cshtml:25`) with
    `ClickAndWaitForNavigationAsync`. On the Create Workflow page, type `Announce published articles` into
    `GetByLabel("Name", new() { Exact = true })` (`Views/WorkflowType/Create.cshtml:21`), with `TypeAsync`. Leave **More settings** closed.
    Click **Create and open the designer** (`button.save`, `Views/WorkflowType/Create.cshtml:131`) with
    `ClickAndWaitForNavigationAsync`. Wait for `[data-cy='start-picker']` to be visible.
  - Visible when the narration ends: the designer of the workflow **Announce published articles**: the toolbar with
    the workflow name, the activities pane on the left, an empty canvas, and the **Start with an event** section with
    its buttons in the middle (`StartPicker.vue`, `WorkflowsDesignerJSLocalizer.cs:334`).
  - Labels from: `OrchardCore.Workflows/AdminMenu.cs:18-19` ("Design", "Workflows"),
    `Views/WorkflowType/Index.cshtml:25` ("Create Workflow"), `Views/WorkflowType/Create.cshtml:131`
    ("Create and open the designer").
- `event-2`
  - Actions: click `[data-cy='start-picker-ContentPublishedEvent']` (its text is **Content Published Event**).
    `ClickAsync`, then wait for `[data-cy='panel-activity-form']` to be visible: the activity panel opens by itself
    for an activity that has an editor (`OrchardCore.Workflows/Assets/designer/src/App.vue`, `addActivity`).
  - Visible when the narration ends: one card on the canvas, headed **Content Published Event**, with the green
    **Start** badge (the first event added becomes the start activity,
    `OrchardCore.Workflows/Services/WorkflowTypeDraftManager.cs:175`), and its body showing **Any content type**
    (`OrchardCore.Contents/Views/Items/ContentPublishedEvent.Fields.Design.cshtml:6`). Its port is labeled **Done**
    (`OrchardCore.Contents/Workflows/Activities/ContentActivity.cs:60`). The activity panel is open at the bottom, on
    the **Settings** tab, with **Content Types** in it.
  - Labels from: `OrchardCore.Contents/Workflows/Activities/ContentPublishedEvent.cs:22`.
- `event-3`
  - Actions: click the **Content Types** picker (the button of the `select[data-content-type-picker]`'s
    bootstrap-select, `OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Picker.cshtml`; its placeholder
    is **Any content type**). In the search box ("Search content types"), type `Article`. Click the `Article` option.
    Close the picker (Escape, or a click on an empty part of the panel), then press Tab so the change is applied.
    The Settings tab says that changes are saved as you make them (`WorkflowsDesignerJSLocalizer.cs:59`); if the panel
    also shows an **Apply** button (`WorkflowsDesignerJSLocalizer.cs:132`), click it after Tab. Wait until the toolbar
    says **All changes saved**.
  - Visible when the narration ends: the **Content Types** picker showing the tag **Article**, and the canvas card of
    the event showing **Article** in its body instead of **Any content type**.
  - Labels from: `OrchardCore.Contents/Views/Items/ContentPublishedEvent.Fields.Edit.cshtml:5`
    ("Content Types"), `Views/Shared/Components/SelectContentTypes/Picker.cshtml:16,20`.

## Clip `03-create`: Create Content

Starts on the designer of **Announce published articles** (from the Workflows list, **Edit** of the workflow, which
opens `/Admin/Workflows/Types/Edit/<id>`; `<id>` is the number in the **Edit** link). The state is the one `snapshot-1`
holds: the event card with **Article**. Click an empty part of the canvas so no activity is selected and the activity
panel is closed.

- `create-1`
  - Actions: move the cursor to the **Done** port of the event: `.wfd-port:not(.is-connected)[data-cy$='-Done']` (there
    is one port only; its `aria-label` starts with "Outcome Done:",
    `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:332`). While the pointer is over a port that leads
    nowhere yet, its dot grows and shows a plus (`designer.css`, `.wfd-port:not(:disabled):not(.is-connected):hover`),
    so `MoveToAsync` the port, hold for half a second, then click it. A menu titled "Add the next activity, after
    Done" opens (`WorkflowsDesignerJSLocalizer.cs:333`). Type `Create Content` in its search box (`[data-cy='quick-add-search']`; the placeholder is
    **Search activities**). Click `[data-cy='quick-add-CreateContentTask']` (**Create Content Task**, category
    **Content**). Wait for `[data-cy='panel-activity-form']`.
  - Visible when the narration ends: a second card, **Create Content Task**, to the right of the event and connected
    to its **Done** port by a transition, and the activity panel open on **Settings** with **Content Type**,
    **Publish** and **Content Properties**, and below them the **When it fails** section that every task has
    (`OrchardCore.Workflows/Views/ActivityRetryPolicy.Edit.cshtml:7`; leave its defaults, **Retries** 0).
  - Labels from: `OrchardCore.Contents/Workflows/Activities/CreateContentTask.cs:40`,
    `OrchardCore.Contents/Views/Items/CreateContentTask.Fields.Edit.cshtml:6,18,25`.
- `create-2`
  - Actions: `SelectAsync` on `GetByLabel("Content Type", new() { Exact = true })` with the value `Announcement` (the
    options are the display names of the content types, in no fixed order, and the select has no empty option, so the
    choice is always made explicitly,
    `OrchardCore.Contents/Workflows/Drivers/CreateContentTaskDisplayDriver.cs:20-22`). Move the cursor to the
    **Publish** checkbox and `HighlightAsync` it with no caption, to show that it is unchecked. Do not click it.
  - Visible when the narration ends: **Content Type** showing **Announcement**, the **Publish** checkbox unchecked
    and highlighted, and the card on the canvas showing `Announcement` under its heading **Create Content** (the
    technical name of the type, `CreateContentTask.Fields.Design.cshtml:7`).
  - Labels from: `CreateContentTask.Fields.Edit.cshtml:6,18`.
- `create-3`
  - Actions: `ClearHighlightsAsync`. The **Content Properties** field is a CodeMirror editor made from the textarea
    (`OrchardCore.Contents/Assets/ts/content-task-editor.ts`), and holds the default value
    `{ "DisplayText": "Enter a title" }`. Its own input element is hidden, so do not use `TypeAsync` on it. Instead:
    `ClickAsync` the visible editor (`[data-task-editor='content'] .CodeMirror`), then
    `scene.Page.Keyboard.PressAsync("Control+A")`, then `scene.Page.Keyboard.TypeAsync(text, new() { Delay = 20 })`
    with the text below on one line, then `scene.Page.Keyboard.PressAsync("Tab")`, and wait for **All changes saved**:

    ```
    { "DisplayText": "Announcement: {{ Workflow.Input.ContentItem.DisplayText }}", "TitlePart": { "Title": "Announcement: {{ Workflow.Input.ContentItem.DisplayText }}" } }
    ```

    The slide `properties-1` shows the same JSON with line breaks, which is the same value. See `OPEN-QUESTIONS.md`
    about how the editor reacts to typing, and the fallback.
  - Visible when the narration ends: the **Content Properties** editor holding the JSON above (it wraps on two or
    three lines), with the Liquid expressions in it readable, and the toolbar saying **All changes saved**.
  - Labels from: `CreateContentTask.Fields.Edit.cshtml:25` ("Content Properties").

## Clip `04-update`: Update Content

Starts on the designer, in the state of `snapshot-2`: the event and **Create Content Task**, connected. No activity is
selected and the activity panel is closed.

- `update-1`
  - Actions: point at, then click, the **Done** port of **Create Content Task** (as in `create-1`): the one
    `.wfd-port:not(.is-connected)[data-cy$='-Done']` (the card has two ports, **Done** and **Failed**, `CreateContentTask.cs:68`;
    the **Done** port of the event is already connected, and `is-connected` is set in `OutcomePort.vue:27`). In the quick-add menu type
    `Update Content` and click `[data-cy='quick-add-UpdateContentTask']`. Wait for `[data-cy='panel-activity-form']`.
  - Visible when the narration ends: a third card, **Update Content Task**, connected to **Create Content Task**,
    and the activity panel on **Settings** with the fields **Content Item ID**, **Content Properties** and **Publish**
    in that order. The card body reads "Update content provided via the ContentItemId input argument" while
    **Content Item ID** is empty (`UpdateContentTask.Fields.Design.cshtml:9`; see `OPEN-QUESTIONS.md`): let it stay on
    screen only for as long as the step needs.
  - Labels from: `UpdateContentTask.cs:45`, `UpdateContentTask.Fields.Edit.cshtml:6,15,27`.
- `update-2`
  - Actions: type `property("ContentItem").ContentItemId` into `GetByLabel("Content Item ID", new() { Exact = true })` (`TypeAsync`), press
    Tab. Click `[data-cy='activity-tab-data']` (**Available data**,
    `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:77`). Hold a moment on the list, without clicking
    an expression.
  - Visible when the narration ends: the **Available data** tab open, listing the values the earlier activities
    provide, among them the content item of **Create Content Task**; the card on the canvas reads "Update content
    using expression:" followed by the expression. If the card is hidden by the panel, move the pointer over it.
  - Labels from: `UpdateContentTask.Fields.Edit.cshtml:6`;
    `OrchardCore.Contents/Workflows/Activities/CreateContentTask.cs:142-146` (the value `ContentItem` the task
    provides as a property); `OrchardCore.Workflows/Assets/designer/src/available/availableData.ts` (the expression
    `property("ContentItem")` and its field `ContentItemId`).
- `update-3`
  - Actions: click `[data-cy='activity-tab-settings']` (**Settings**). Click the visible **Content Properties** editor
    (`[data-task-editor='content'] .CodeMirror`), press Control+A, and type this text on one line with
    `scene.Page.Keyboard.TypeAsync(text, new() { Delay = 20 })` (as in `create-3`); then press Tab and wait for **All
    changes saved**. Do not check **Publish**.

    ```
    { "HtmlBodyPart": { "Html": "<p>The {{ Workflow.Input.ContentEvent.ContentType }} {{ Workflow.Input.ContentItem.DisplayText }} was published.</p>" } }
    ```

  - Visible when the narration ends: the **Settings** tab, with the expression in **Content Item ID**, the JSON in
    **Content Properties**, and **Publish** unchecked.
  - Labels from: `UpdateContentTask.Fields.Edit.cshtml:15,27`.

## Clip `05-publish`: Publish Content

Starts on the designer, in the state of `snapshot-3`: the event, **Create Content Task** and **Update Content Task**.
No activity is selected and the activity panel is closed.

- `publish-1`
  - Actions: point at, then click, the **Done** port of **Update Content Task** (as in `create-1`): the one
    `.wfd-port:not(.is-connected)[data-cy$='-Done']` (the **Done** ports of the two earlier cards are connected). In the quick-add menu type `Publish Content` and click `[data-cy='quick-add-PublishContentTask']`.
    Wait for `[data-cy='panel-activity-form']`.
  - Visible when the narration ends: a fourth card, **Publish Content Task**, connected to **Update Content Task**,
    and the activity panel on **Settings** with the field **Expression** (and the **When it fails** section below it). The
    card body reads "Publish content provided via the ContentItem input argument" while the field is empty (`PublishContentTask.Fields.Design.cshtml:9`).
  - Labels from: `PublishContentTask.cs:24`, `PublishContentTask.Fields.Edit.cshtml:5`. The card has two ports,
    **Published** and **Noop** (`PublishContentTask.cs:29`).
- `publish-2`
  - Actions: type `property("ContentItem")` into `GetByLabel("Expression", new() { Exact = true })` (`TypeAsync`),
    press Tab, and wait for **All changes saved**. Click an empty part of the canvas to close the panel.
  - Visible when the narration ends: the card **Publish Content Task** reading "Publish content using expression:"
    followed by `property("ContentItem")`, on a canvas that shows the four connected cards in a row (zoom to fit if
    needed: **Fit to content**, `WorkflowsDesignerJSLocalizer.cs:358`).
  - Labels from: `PublishContentTask.Fields.Design.cshtml:13`.

## Clip `06-run`: Run the workflow

Starts on the designer, in the state of `snapshot-4`: four connected cards, no panel open. The toolbar button
**Publish** is enabled (`OrchardCore.Workflows/Assets/designer/src/App.vue:757`).

- `run-1`
  - Actions: click `[data-cy='toolbar-publish']` (**Publish**). If the designer opens a dialog titled "Publish the
    workflow?" because of warnings, stop and fix the workflow instead: the workflow of this video has none. Wait for
    a toast whose text starts with "The workflow was published": it reads "The workflow was published as version 1." when
    the designer knows the version number, and "The workflow was published." otherwise
    (`App.vue:358`, `WorkflowsDesignerJSLocalizer.cs:197,221`).
  - Visible when the narration ends: the toast, and the toolbar showing the published version.
  - Labels from: `App.vue:757`, `WorkflowsDesignerJSLocalizer.cs:190`.
- `run-2`
  - Actions: `NavigateAsync("/Admin/Contents/ContentTypes/Article/Create")`
    (`OrchardCore.Contents/Controllers/AdminController.cs:407`). Type `Pumpkin Soup in Autumn` into
    `GetByLabel("Title", new() { Exact = true })` (`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`; the Article also
    has a Subtitle field, so the label must match exactly). Click the green **Publish** button
    (`button.publish[name='submit.Publish']`, `OrchardCore.Contents/Views/Content.PublishButton.cshtml`) with
    `WaitForNavigationAsync`. Wait for the notification "Your Article has been published."
    (`AdminController.cs:473`).
  - Visible when the narration ends: the page after publishing, with the notification "Your Article has been
    published.".
  - Labels from: `OrchardCore.Contents/Views/Content.PublishButton.cshtml`.
- `run-3`
  - Actions: `NavigateAsync("/Admin/Contents/ContentItems")` (`AdminController.cs:67`), the **Content Items** list. The
    list shows the content items by their last change, so the two newest rows are **Announcement: Pumpkin Soup in
    Autumn** and **Pumpkin Soup in Autumn**, each with the badge **Published**
    (`OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:12`). Hold on the list for about two seconds,
    `HighlightAsync` the row of the announcement, then `ClearHighlightsAsync`, and click its title with
    `ClickAndWaitForNavigationAsync` to open the editor.
  - Visible when the narration ends: the editor of the announcement, with **Title** holding "Announcement: Pumpkin
    Soup in Autumn" and the body holding "The Article Pumpkin Soup in Autumn was published." (Wait for the body editor
    to have loaded.) No error message and no **Draft** badge is on screen.
  - Labels from: `OrchardCore.Contents/AdminMenu.cs:68` ("Content Items"); `ContentsTags_SummaryAdmin.cshtml:12`.

If the list shows no announcement, the workflow did not run to its end: open **Instances** in the designer toolbar to
see where it stopped, fix the cause, restore `snapshot-4`, and record `06-run` again.

## Slides

One line each. The slides are drawn from `slides.py`; nothing is recorded.

- `concepts-1`: two cards, "Events start a workflow" (Created, Updated, Save Draft, Published, Unpublished, Deleted and
  Versioned) and "Tasks work on content items"; the first is bright.
- `concepts-2`: the helper `TriggerWorkflowEventAsync` of `ContentsHandler`, in a code panel, from
  `OrchardCore.Contents/Workflows/Handlers/ContentsHandler.cs`.
- `concepts-3`: four cards: Create Content Task and Update Content Task (Done, Failed), Publish Content Task
  (Published, Noop), and Retrieve, Unpublish and Delete Content Task, with their outcomes.
- `properties-1`: the JSON of Content Properties, and two cards: "Liquid first" and "Then JSON".
- `properties-2`: two cards, Done and Failed, the outcomes of Create Content Task.
- `merge-1`: the lines of `UpdateContentTask.ExecuteAsync` that load the draft, merge the JSON, update and validate.
- `merge-2`: the lines that read the Content Item ID (and throw without it) and choose between `PublishAsync` and
  `SaveDraftAsync`, inside the `if (!inlineEventOfSameContentItemId)` block of `UpdateContentTask.ExecuteAsync`
  (`UpdateContentTask.cs:143-153`).
- `lookup-1`: `ContentActivity.GetContentAsync`, with the `else` branch bright (the input, then the property).
- `lookup-2`: `PublishContentTask`: its outcomes, and the `Noop` when the item has no draft.
- `limits-1`: the loop guards of `CreateContentTask` and `PublishContentTask`.
- `limits-2`: three cards: a guard or a missing id throws (by default the instance is faulted), invalid content takes
  Failed, no draft takes Noop.
- `export-1`: the `WorkflowType` recipe step, in a panel titled `Recipe.json` (the file that the export puts in its zip
  package, `OrchardCore/OrchardCore.Deployment.Abstractions/DeploymentPlanResult.cs:38`), one activity record (Publish
  Content Task) and one transition.

## Checks for private data

Before the video is assembled, and again on the result:

- Every name on screen is invented: the workflow (**Announce published articles**), the article (**Pumpkin Soup in
  Autumn**), the announcement, the user `admin` and the address `admin@example.com`. No other user, address or phone
  number is created or shown.
- The password is typed nowhere: the sign-in comes from `state.json`. If the capture shows a password field, record
  again.
- The Blog recipe's own sample content is kept as it is; look at the **Content Items** list in `run-3` for any author
  name other than `admin`.
- OCR the video at one frame per second and at every scene change, and search the text for `@` addresses other than
  `admin@example.com`, for real names, for the machine's folder names and for any token.
- No folder of this machine, no `App_Data` copy and no `state.json` is committed.

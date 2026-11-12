# Recording plan: Data Pipelines and Workflows

This plan lets a person record the video without deciding anything. Every selector below stands for
`scene.Page.Locator(...)`; a label is written as the code spells it, with the file and line in the Orchard Core source
(`src/OrchardCore.Modules/...`) it was read from. The `data-cy` selectors are the test hooks of the workflow designer
(a Vue application); they were read from its source, not from a running site, so confirm each one once with
`Recorder probe` (see OPEN-QUESTIONS.md).

The audience is admin: the clips show the admin screens and never open a code editor. The seed recipe below is typed
outside the capture.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with
  its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Auto Setup** (no setup form is recorded): recipe **Blog**, site name **Orchard Core**, time zone
  `America/Los_Angeles`, tenant **Default**, user `admin`, email `admin@example.com`, password `<password>` (chosen
  outside git, passed in `VIDEO_PASSWORD`; never typed on camera).
- **Outbound calls** blocked with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Browser**: the standard recorder viewport (1536x864 CSS pixels, light color scheme, `en-US`), signed in once with
  `Recorder setup`; `state.json` stays out of git.
- **Features to enable before recording**, in **Tools** > **Features** (`/Admin/Features`; search the name, check the
  feature, **Bulk Actions** > **Enable**; `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:34-38`):
  - **Workflows** (`OrchardCore.Workflows`; it enables Liquid and Scripting too);
  - **Data Pipelines** (`OrchardCore.DataPipelines`; it enables Data Pipelines Core and Users);
  - **Email** (`OrchardCore.Email`), only so that the **Send by email** step exists. Do not enable any email provider
    such as SMTP, and do not save email settings: the second pipeline must fail because of that.
  - The Blog recipe already enables Contents, Media, Deployment (for **JSON Import**), Features and Users. Do not
    enable the Timer, HTTP, Notifications or Session workflow features: the clips do not use them.
- **Settings**: none to save. The administrator role has all the Data Pipelines and Workflows permissions by default.
- **External services**: none. There is no mail server, not even a stand-in.

### Seed data (created outside the capture)

1. **Import the two pipelines.** Open **Tools** > **Deployments** > **JSON Import**
   (`/Admin/DeploymentPlan/Import/Json`; `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30`), paste the
   recipe below in the **JSON** box, and select **Import**. The step is the `DataPipelines` recipe step
   (`src/OrchardCore.Modules/OrchardCore.DataPipelines/Recipes/DataPipelinesRecipeStep.cs`); the property names are the
   ones of the settings classes of the steps (`DataSourceStepSettings`, `CreateFileStepSettings`,
   `SaveToMediaStepSettings`, `SendByEmailStepSettings`). The data is fake: a CSV file of the blog posts, and an
   `@example.com` recipient.

   ```json
   {
     "steps": [
       {
         "name": "DataPipelines",
         "Pipelines": [
           {
             "PipelineId": "blogpostsexport",
             "Name": "Blog posts export",
             "Description": "Saves the blog posts as a CSV file in the media library.",
             "IsEnabled": true,
             "Definition": {
               "Steps": [
                 {
                   "StepId": "source", "Type": "DataSource", "X": 120, "Y": 200,
                   "Properties": { "DataSourceStepSettings": { "Source": "Contents", "DataSet": "BlogPost", "Fields": [ "DisplayText", "Author", "PublishedUtc" ] } }
                 },
                 {
                   "StepId": "file", "Type": "CreateFile", "X": 440, "Y": 200,
                   "Properties": { "CreateFileStepSettings": { "Format": "csv", "FileName": "blog-posts" } }
                 },
                 {
                   "StepId": "media", "Type": "SaveToMedia", "X": 760, "Y": 200,
                   "Properties": { "SaveToMediaStepSettings": { "Folder": "", "Overwrite": true } }
                 }
               ],
               "Connections": [
                 { "SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input" },
                 { "SourceStepId": "file", "SourcePort": "Output", "TargetStepId": "media", "TargetPort": "Input" }
               ]
             }
           },
           {
             "PipelineId": "blogpostsbyemail",
             "Name": "Blog posts by email",
             "Description": "Sends the blog posts by email to the editors.",
             "IsEnabled": true,
             "Definition": {
               "Steps": [
                 {
                   "StepId": "source", "Type": "DataSource", "X": 120, "Y": 200,
                   "Properties": { "DataSourceStepSettings": { "Source": "Contents", "DataSet": "BlogPost", "Fields": [ "DisplayText", "Author", "PublishedUtc" ] } }
                 },
                 {
                   "StepId": "file", "Type": "CreateFile", "X": 440, "Y": 200,
                   "Properties": { "CreateFileStepSettings": { "Format": "csv", "FileName": "blog-posts" } }
                 },
                 {
                   "StepId": "mail", "Type": "SendByEmail", "X": 760, "Y": 200,
                   "Properties": { "SendByEmailStepSettings": { "To": "editors@example.com", "Subject": "Blog posts", "Body": "The blog posts are attached." } }
                 }
               ],
               "Connections": [
                 { "SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input" },
                 { "SourceStepId": "file", "SourcePort": "Output", "TargetStepId": "mail", "TargetPort": "Input" }
               ]
             }
           }
         ]
       }
     ]
   }
   ```

2. **Publish both pipelines.** An imported pipeline is a draft, and a draft never runs
   (`DataPipelinesRecipeStep.cs`, remarks of the class). Open **Tools** > **Data Pipelines** > **Pipelines**
   (`/Admin/DataPipelines`), select **Edit** on **Blog posts export**, check that the designer shows three connected
   steps and that its **Issues** tab lists no error, and select **Publish** (`[data-cy="toolbar-publish"]` in the
   pipeline designer, `src/OrchardCore.Modules/OrchardCore.DataPipelines/Assets/designer/src/App.vue:769`). If a
   confirmation asks about warnings, confirm it. Do the same for **Blog posts by email**. The user who publishes
   (`admin`) is the user the runs execute for.
3. **Check the list.** The list of pipelines shows both pipelines with the badge **Version 1**, no **Draft** badge, and
   no last-run badge (`Views/Admin/Index.cshtml:52-71`). Do not run either pipeline before the first take: the Run
   History of the video starts empty, and the Media library has no `blog-posts.csv` yet. To check that the pipelines
   work, restore this snapshot afterwards, or check them on a copy of the site.
4. **Take snapshot A**: stop the site, copy its `App_Data` folder (and `state.json`) aside, start it again.

The blog post of the video is the one the Blog recipe creates, **Man must explore, and this is exploration at its
greatest** (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:1000-1002`). No other record is created.

## Order, dependencies and snapshots

Record in this order; each clip starts from the state the previous one leaves.

| Order | Clip | Starts from | What it changes in the site |
|---|---|---|---|
| 1 | `01-concepts` | nothing | Nothing: slides only. |
| 2 | `02-build` | snapshot A | Creates and publishes the workflow **Export the blog posts when one is published**. Take **snapshot B** after it. |
| 3 | `03-run` | snapshot B | Publishes a new version of the blog post, which queues one run of **Blog posts export** (it succeeds), saves `blog-posts.csv` in the Media library, and finishes one workflow instance. Take **snapshot C** after it. |
| 4 | `04-react` | snapshot C | Creates and publishes the workflow **Log the pipeline runs that fail**, queues one run of **Blog posts by email** (it fails), and finishes one instance of the second workflow. |

To record a clip again, stop the site, restore the snapshot of the clip, and start the site. `04-react` needs the
first workflow and its finished instance (it shows both in its last step), so never record it from snapshot A or B.

Narrate first (`narrate.py`), so the recorder holds each step for its narration, then `Recorder record 02-build`,
`03-run` and `04-react` one at a time.

## Clips

### 01-concepts (slides, no recording)

- `concept-1`: the slide **Run a pipeline by hand or from a workflow**, with four cards: **By hand**, **By an event**,
  **At set times**, **After a run**. (The label of the first card, **Run now**, comes from
  `Views/Admin/Index.cshtml:92`.)
- `concept-2`: the slide **Two activities in the Data category**, with two cards: **Run Data Pipeline Task** and
  **Data Pipeline Run Completed Event** (`Workflows/RunDataPipelineTask.cs:44`,
  `Workflows/DataPipelineRunCompletedEvent.cs:26`).
- `concept-3`: the slide **What a run uses**, with four cards: **The published version**, **In the background**,
  **The access of the publisher**, **An enabled pipeline**.

### 02-build

**Starts on** `/Admin/Workflows/Types`, opened before the capture, with the admin menu collapsed. The list is empty:
**Nothing here!** and **There are no workflow types for the moment.** (`OrchardCore.Workflows/Views/WorkflowType/Index.cshtml:34`).
Start the capture. The activity panel and the Activities pane keep their last size and state in the browser: start
from a browser profile where the Activities pane is expanded and the activity panel is not pinned.

Each activity added on the canvas opens its settings in the activity panel at the bottom
(`Assets/designer/src/App.vue:270-274`). A setting is applied when its field loses focus, so press Tab after typing or
choosing a value (`src/docs/reference/modules/Workflows/README.md`, "Workflow Designer"). If the panel hides a port you
need, close it with `[data-cy="activity-panel-close"]`.

- `build-1`: click **Create Workflow** (`ClickAndWaitForNavigationAsync` on `a.create`;
  `Views/WorkflowType/Index.cshtml:25`). On the page **Create Workflow**, type
  `Export the blog posts when one is published` in the **Name** field, which has the focus
  (`Views/WorkflowType/Create.cshtml:21`; its hint is at line 25). Leave **More settings** closed
  (`Create.cshtml:30`).
  *On screen when the narration ends:* the Create Workflow page with the name typed, **More settings** collapsed, and the
  **Create and open the designer** button visible (`Create.cshtml:131`).
- `build-2`: click **Create and open the designer** (`ClickAndWaitForNavigationAsync` on `button.save`), wait until
  `[data-cy="start-picker"]` is visible, then click the button **Content Published Event**
  (`[data-cy="start-picker-ContentPublishedEvent"]`; the text is the display text of the activity,
  `src/OrchardCore.Modules/OrchardCore.Contents/Workflows/Activities/ContentPublishedEvent.cs:22`; the heading
  **Start with an event** and its hint come from `Services/WorkflowsDesignerJSLocalizer.cs:334-335`). Wait until the
  activity panel `[data-cy="activity-panel"]` is visible with the form `[data-cy="panel-activity-form"]` loaded.
  *On screen when the narration ends:* the designer with one activity, **Content Published Event**, with its **Start**
  badge, and the activity panel open on the Settings tab showing **Content Types**.
- `build-3`: highlight the **Content Types** control in the panel (`Contents/Views/Items/ContentPublishedEvent.Fields.Edit.cshtml:5`;
  its placeholder is **Any content type**, `Contents/Views/Shared/Components/SelectContentTypes/Picker.cshtml`), leave it
  empty, then clear the highlight and highlight the **Start** badge (`[data-cy="start-badge"]`;
  `Assets/designer/src/canvas/ActivityNode.vue:236`, `Services/WorkflowsDesignerJSLocalizer.cs:329`).
  *On screen when the narration ends:* a highlight ring around the **Start** badge of the event.
- `build-4`: if the activity panel covers the **Done** port, close it. Click the port **Done** of the event
  (`.wfd-port[data-outcome="Done"]`; the outcome is `Contents/Workflows/Activities/ContentActivity.cs:60`). The menu
  `[data-cy="quick-add"]` opens with the title **Add the next activity, after Done**
  (`WorkflowsDesignerJSLocalizer.cs:333`). Type `pipeline` in `[data-cy="quick-add-search"]`
  (placeholder **Search activities**, line 46). The list shows **Run Data Pipeline Task** with the category **Data**
  first, then **Data Pipeline Run Completed Event** (`QuickAddMenu.vue:124-127`; the category is
  `DataPipelinesLocalizationSources.cs:14`). Click **Run Data Pipeline Task** (`[data-cy="quick-add-RunDataPipelineTask"]`).
  *On screen when the narration ends:* two connected activities, **Content Published Event** and **Run Data Pipeline Task**,
  and the panel open on the settings of the task with the label **Data pipeline**.
- `build-5`: in the panel, choose **Blog posts export** in the list **Data pipeline** (`SelectAsync` on
  `scene.Page.GetByLabel("Data pipeline", new() { Exact = true })`, since the cards of the Activities pane carry labels such as "Add Run Data Pipeline Task" that a default match would also find; the label is `DataPipelines/Views/Items/RunDataPipelineTask.Fields.Edit.cshtml:4`, the
  empty option **Choose a data pipeline** is line 7), then press Tab.
  *On screen when the narration ends:* **Blog posts export** selected, and the hint **The published version of the pipeline
  runs in the background, with the access of the user who published it.** below it (line 14). The card of the task on
  the canvas reads **Waits for the run** (`RunDataPipelineTask.Fields.Design.cshtml:7`).
- `build-6`: highlight the **Parameters** box (`RunDataPipelineTask.Fields.Edit.cshtml:19`) and leave it empty. While it is empty it shows the placeholder `Region=EMEA` (line 21): fake data, and the narration does not read it. The hint
  under the box shows a placeholder written with braces: it is the text of the product, the narration does not read it,
  so do not highlight or zoom on the hint.
  *On screen when the narration ends:* a highlight ring around the empty **Parameters** box.
- `build-7`: highlight the checkbox **Wait for the run to end** (checked by default; line 30), then the ports
  **Succeeded** and **Failed** of the task (`.wfd-port[data-outcome="Succeeded"]` and `.wfd-port[data-outcome="Failed"]`;
  `Workflows/RunDataPipelineTask.cs:93`), then the checkbox **Skip when a run is already queued or running** (line 40).
  Do not change either checkbox.
  *On screen when the narration ends:* a highlight ring around **Skip when a run is already queued or running**, unchecked.
- `build-8`: clear the highlights, click **Publish** (`[data-cy="toolbar-publish"]`;
  `WorkflowsDesignerJSLocalizer.cs:190`). No confirmation is expected; if the dialog **Publish the workflow?** appears,
  stop and find the warning it lists. Wait until `[data-cy="published-version"]` reads **Version 1**.
  *On screen when the narration ends:* the toolbar of the designer with the badge **Version 1** and **All changes saved**
  (`WorkflowsDesignerJSLocalizer.cs:166`).

After the clip, take **snapshot B**.

### 03-run

**Starts on** `/Admin/Contents/ContentItems`, opened before the capture. The list holds the content items of the Blog
recipe, including the blog post **Man must explore, and this is exploration at its greatest**.

- `run-1`: click the title link of the blog post (`ClickAndWaitForNavigationAsync`; the link is
  `Contents/Views/ContentsTitle_SummaryAdmin.cshtml:10`; the list is **Content** > **Content Items**,
  `Contents/AdminMenu.cs:68`). On the edit page, do not change anything, and click **Publish**
  (`button[name="submit.Publish"]`, `Contents/Views/Content.PublishButton.cshtml:19`) with
  `ClickAndWaitForNavigationAsync`.
  *On screen when the narration ends:* the Content Items list again, with the message **Your Blog Post has been published.**
  (`Contents/Controllers/AdminController.cs:967`).
- `run-2`: first, off camera, wait until the run has ended: `await scene.OffCameraAsync(...)` around a loop that
  fetches `/Admin/DataPipelines/Runs` from the page every second (up to 60 seconds), parses the HTML, and stops when the
  first `li[data-run-id]` has a `span.badge` whose text is `Succeeded`. (The word also appears in the status drop-down
  of the page, so check the badge of the row, not the page text.) Then `NavigateAsync("/Admin/DataPipelines/Runs")`,
  which stands for **Tools** > **Data Pipelines** > **Run History** (`DataPipelines/AdminMenu.cs:18-25`), and highlight
  the last badge of the row, the one with the play icon (`li[data-run-id] span.badge:has(i.fa-play)`;
  `Views/Admin/Runs.cshtml:103`).
  *On screen when the narration ends:* one row, **Blog posts export**, with the badges **Succeeded**, **Version 1**, its
  queued time, its duration, and **Workflow**, and a highlight ring around the last badge.
- `run-3`: click **Details** in the row (`ClickAndWaitForNavigationAsync`; `Views/Admin/Runs.cshtml:108`). On the page
  **Run of Blog posts export**, scroll down by a few lines if the heading **Delivered** (`Views/Admin/Run.cshtml:98`)
  is not visible, and highlight the list under it.
  *On screen when the narration ends:* the table **Steps** with its columns **Rows in** and **Rows out**
  (`Run.cshtml:69-70`), and under **Delivered** the line **Saved 'blog-posts.csv' to the media library.**
  (`DataPipelines/Steps/Media/SaveToMediaStep.cs:87`), highlighted. The badge at the top reads **Succeeded**.

After the clip, take **snapshot C**.

### 04-react

**Starts on** `/Admin/Workflows/Types`, opened before the capture. The list shows the workflow **Export the blog posts
when one is published**.

- `react-1`: click **Create Workflow**, type `Log the pipeline runs that fail` in **Name**, and click
  **Create and open the designer** (both with `ClickAndWaitForNavigationAsync`; same labels as `build-1` and `build-2`).
  Wait until `[data-cy="start-picker"]` is visible.
  *On screen when the narration ends:* the designer of the empty second workflow, with the panel **Start with an event**
  on the canvas and the pane **Activities** on the left.
- `react-2`: type `pipeline` in `[data-cy="toolbox-search"]` (placeholder **Search activities**; the pane title **Activities**
  is `App.vue:811` of the workflow designer). The category **Data** opens by itself while a search is active (`Assets/designer/src/toolbox/ActivityToolbox.vue:19`) and shows
  two cards, **Run Data Pipeline** and **Data Pipeline Run Completed**
  (`DataPipelines/Views/Items/RunDataPipelineTask.Fields.Thumbnail.cshtml:1`,
  `DataPipelineRunCompletedEvent.Fields.Thumbnail.cshtml:1`). Click the card **Data Pipeline Run Completed**
  (`[data-cy="toolbox-activity-DataPipelineRunCompletedEvent"]`); its text reads **Executes when a run of a data pipeline
  ends.** Wait until the form of the panel is loaded.
  *On screen when the narration ends:* one activity, **Data Pipeline Run Completed Event**, with the **Start** badge, and the
  panel open on its settings, showing **Data pipeline** and **Runs**.
- `react-3`: highlight the list **Data pipeline** (`GetByLabel("Data pipeline", new() { Exact = true })`; its first option is **Any data pipeline**,
  `DataPipelineRunCompletedEvent.Fields.Edit.cshtml:4-7`), clear the highlight, choose **Runs that failed or were cancelled**
  in the list **Runs** (`SelectAsync` on `scene.Page.GetByLabel("Runs", new() { Exact = true })`; the options are lines
  20-22: **Every run**, **Runs that succeeded**, **Runs that failed or were cancelled**), then press Tab.
  *On screen when the narration ends:* **Runs that failed or were cancelled** selected, and the card of the event reading
  **Failed runs** (`DataPipelineRunCompletedEvent.Fields.Design.cshtml:7`).
- `react-4`: if the panel covers the port, close it. Click the port **Done** of the event, type `log` in
  `[data-cy="quick-add-search"]`, and click **Log Task** (`[data-cy="quick-add-LogTask"]`; category **Primitives**;
  `src/OrchardCore.Modules/OrchardCore.Workflows/Activities/LogTask.cs:25`). In the panel, choose **Error** in **Log Level**
  (`Views/Items/LogTask.Fields.Edit.cshtml:6`; the options are the log levels), type `A data pipeline run failed` in
  **Text** (line 15; `GetByLabel("Text", new() { Exact = true })`), and press Tab. The hint under **Text** mentions another syntax: the narration does not read it, so do not highlight or zoom on it.
  *On screen when the narration ends:* two connected activities, the event and **Log Task**, and the panel showing
  **Log Level** set to **Error** and **Text** filled in.
- `react-5`: click **Publish** and wait until `[data-cy="published-version"]` reads **Version 1**.
  *On screen when the narration ends:* the toolbar with **Version 1** and **All changes saved**.
- `react-6`: `NavigateAsync("/Admin/DataPipelines")`. In the row of **Blog posts by email**
  (`li[data-pipeline-id="blogpostsbyemail"]`), click **Actions** (`button.actions`; `Views/Admin/Index.cshtml:85-87`), click
  **Run now** in the menu, and confirm with the button **Run** of the dialog titled **Run now**, whose message reads
  **Run the pipeline 'Blog posts by email' now?** (`Index.cshtml:92`). The click redirects to the page of the run, with
  the message **The pipeline 'Blog posts by email' will run in the background.** (`Controllers/AdminController.cs:252`).
  *On screen when the narration ends:* the page **Run of Blog posts by email** with that message. The page refreshes itself
  every 3 seconds while the run is queued or running (`Views/Admin/Run.cshtml:18`); see OPEN-QUESTIONS.md.
- `react-7`: first, off camera, wait until the run has failed and the second workflow had time to run: fetch
  `/Admin/DataPipelines/Runs` every second (up to 60 seconds) until the first `li[data-run-id]` has a `span.badge` with the
  text `Failed`, then wait 3 more seconds. Then `NavigateAsync("/Admin/DataPipelines/Runs")` and highlight the red error
  line of the first row (`li[data-run-id] .text-danger`; `Views/Admin/Runs.cshtml:130`).
  *On screen when the narration ends:* the first row, **Blog posts by email**, with the badge **Failed** and its error in
  red below it (expected text: **The email could not be sent: Email settings must be configured before an Email message
  can be sent.**, from `SendByEmailStep.cs:161` and `src/OrchardCore/OrchardCore.Email.Core/Services/DefaultEmailService.cs:43`);
  the row of **Blog posts export** with **Succeeded** is under it.
- `react-8`: `NavigateAsync("/Admin/Workflows/Types")`, then click the button **Instances**
  (`a[data-cy="all-instances"]`, `ClickAndWaitForNavigationAsync`; `Views/WorkflowType/Index.cshtml:24`), which opens
  `/Admin/Workflows/Instances` titled **Instances**. The workflows live under **Design** > **Workflows**
  (`Workflows/AdminMenu.cs:18-19`).
  *On screen when the narration ends:* the status buttons with **Finished** showing 2 (`Views/Workflow/Index.cshtml:43`),
  and two rows, one per workflow, both with the status **Finished**. If a **Faulted** or **Halted** count is not 0, stop:
  see OPEN-QUESTIONS.md.

## Checks for private data

- Every name on screen is fake: the user `admin`, the address `admin@example.com`, the recipient `editors@example.com`,
  and the sample blog post of the Blog recipe. The Run History, the run page and the Instances page hold identifiers,
  dates and times only.
- The password is never typed on camera; the sign-in comes from `state.json`, which is never committed. The mail
  settings of the site stay empty, so there is no server name or account to show.
- No MP4, audio file, `state.json`, mask script, or copy of `App_Data` goes in the repository.
- OCR the video at one frame per second and at every scene change, and search the text for `@` addresses other than
  `example.com`, for real names, and for anything that looks like a token.

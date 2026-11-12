# Open questions: Data Pipelines and Workflows

The narration and the recording plan were written from the source of Orchard Core, without a running site. Each item
says what was checked in the source and what a person must confirm on a site before recording.

## 1. The second pipeline fails because the site has no email settings (steps `react-6`, `react-7`)

- **Checked:** `SendByEmailStep.ExecuteAsync` (`src/OrchardCore.Modules/OrchardCore.DataPipelines/Steps/Destinations/SendByEmailStep.cs:113-161`)
  calls the email service and throws "The email could not be sent: ..." when the result fails.
  `DefaultEmailService.SendAsync` (`src/OrchardCore/OrchardCore.Email.Core/Services/DefaultEmailService.cs:36-44`) fails
  with "Email settings must be configured before an Email message can be sent." when
  `DefaultEmailProviderResolver.GetAsync` returns no provider, which happens when no default provider name is set
  (`DefaultEmailProviderResolver.cs:22-42`).
- **Confirm:** with only the Email feature enabled (no SMTP feature, no saved settings), a run of **Blog posts by email**
  ends as **Failed**, with that message, and the Email feature does not set a default provider by itself. If it does,
  the narration "The site has no email settings, so the run fails" is wrong: pick another way to make a run fail, or
  change the sentence.

## 2. Both workflow instances are Finished (step `react-8`)

- **Checked:** the first workflow halts on the **Run Data Pipeline Task** (`Workflows/RunDataPipelineTask.cs:141`), the
  handler resumes it when the run ends (`Workflows/WorkflowDataPipelineRunHandler.cs:22-41`), and the **Succeeded**
  outcome leads nowhere, so the instance should end. The event of the second workflow starts an instance only for a run
  that failed or was cancelled (`Workflows/DataPipelineRunCompletedEvent.cs:53-65`).
- **Confirm:** after the clips, the page **Instances** shows exactly two instances, both **Finished** (statuses are
  `Views/Workflow/Index.cshtml:37-44`), and none **Faulted** or **Halted**. The successful run of **Blog posts export**
  must not start an instance of the second workflow.

## 3. Test hooks and activity names of the designer (steps `build-2`, `build-4`, `react-2`, `react-4`)

- **Checked:** the selectors `[data-cy="start-picker-ContentPublishedEvent"]`, `[data-cy="quick-add-RunDataPipelineTask"]`,
  `[data-cy="toolbox-activity-DataPipelineRunCompletedEvent"]` and `[data-cy="quick-add-LogTask"]` follow the code of the
  designer (`Assets/designer/src/api/presets.ts:6`, `canvas/StartPicker.vue`, `canvas/QuickAddMenu.vue`,
  `toolbox/ActivityToolbox.vue`), where the key of an activity is its `Name`. The `Name` of an event is written in its
  class (`ContentPublishedEvent`, `DataPipelineRunCompletedEvent`); the `Name` of a task is the name of its class (`TaskActivity.cs:8` of `OrchardCore.Workflows.Abstractions`:
  `typeof(TActivity).Name`), so `RunDataPipelineTask` and `LogTask`.
- **Confirm:** with `Recorder probe`, list the `data-cy` values of the quick-add menu and the Activities pane, and
  correct the plan if a name differs.

## 4. Names shown on the canvas and in the Activities pane (steps `build-4`, `react-2`)

- **Checked:** the canvas header of an activity and the quick-add list show its display text, **Run Data Pipeline Task**
  and **Data Pipeline Run Completed Event** (`Assets/designer/src/canvas/ActivityNode.vue:234`, `QuickAddMenu.vue:126`).
  The cards of the Activities pane show the thumbnail of the activity, titled **Run Data Pipeline** and
  **Data Pipeline Run Completed** (`DataPipelines/Views/Items/*.Fields.Thumbnail.cshtml:1`), and the body of an activity
  on the canvas repeats those short titles. The narration uses the display text for the first, and the thumbnail title
  where the viewer clicks a card (`react-2`).
- **Confirm:** on screen, the words of the narration match the words the viewer sees at that moment.

## 5. A page that refreshes itself during the capture (step `react-6`)

- **Checked:** the page of a run that is not over carries a meta refresh every 3 seconds (`Views/Admin/Run.cshtml:18`),
  and **Run now** redirects to that page (`Controllers/AdminController.cs:252-254`, `RunNow`).
- **Confirm:** the screencast survives a navigation the page starts by itself. If the frames freeze after the refresh,
  wait for the run to end before the step (off camera), or end the step on the Run History page instead.

## 6. Publishing a blog post without a change starts the workflow (step `run-1`)

- **Checked:** the Contents workflow handler triggers **Content Published Event** from `PublishedAsync`
  (`src/OrchardCore.Modules/OrchardCore.Contents/Workflows/Handlers/ContentsHandler.cs:34-37`), and the edit page's
  **Publish** button saves a draft version and publishes it.
- **Confirm:** selecting **Publish** on the unchanged blog post queues a run, and the run ends within a minute (the run
  starts when the request ends, `Services/DataPipelineRunManager.cs:82-90`; a background task starts missed runs every
  minute, `BackgroundTasks/DataPipelineRunsBackgroundTask.cs`).

## 7. The seed recipe and the published pipelines (seed data)

- **Checked:** the property names of the recipe come from `DataSourceStepSettings`, `CreateFileStepSettings`,
  `SaveToMediaStepSettings`, `SendByEmailStepSettings` and the example of the documentation
  (`src/docs/reference/modules/DataPipelines/README.md`, "Recipes and deployment"); the fields `DisplayText`, `Author` and
  `PublishedUtc` are in `ContentDataColumns.cs`; the ports are named `Output` and `Input` (`DataPipelinePort.cs`).
- **Confirm:** the import succeeds, both designers show no error in **Issues**, **Publish** works (a confirmation about
  warnings may appear), the first run writes `blog-posts.csv` to the root of the media library, and the run page lists
  **Saved 'blog-posts.csv' to the media library.** under **Delivered** (`SaveToMediaStep.cs:87`).

## 8. The designer applies a setting when its field loses focus (steps `build-5`, `react-3`, `react-4`)

- **Checked:** the documentation of the designer says changes in an activity editor are applied when you leave a field
  (`src/docs/reference/modules/Workflows/README.md`, "Drafts and Publishing"). The code of the form host is in the shared
  `@bloom` scripts, which are not in the source read.
- **Confirm:** pressing Tab after each value applies it, the ports of the task show **Succeeded** and **Failed** at once,
  and **Publish** is enabled afterwards.

## 9. Pronunciation

The narration uses no initialism that needs a `spoken` entry. A person should still listen to "Run History", "Log Task"
and "Done" once the audio exists.

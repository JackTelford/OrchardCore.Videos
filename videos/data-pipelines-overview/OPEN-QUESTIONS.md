# Open questions: Data Pipelines overview

What a person must confirm on a running site before or while recording. The narration states only what the Orchard Core
source and the documentation show; each item below is something the source could not settle. Source paths are relative to
`src/OrchardCore.Modules/OrchardCore.DataPipelines` unless they start with `src/`.

## 1. The shared parts of the designer are not in the source that was read

The designer imports shared components from `@bloom/designer` (`Assets/designer/vite.config.ts` aliases `@bloom` to
`.scripts/bloom`, which is not part of the source copy). Their labels and markup could not be read. Confirm:

- **Issues tab.** With the three steps of the video, the tab reads "No issues: the pipeline can be published." (the text
  comes from `Assets/designer/src/i18n.ts:11`, but the shared issues list renders it) and the Issues tab shows no count.
- **Publish.** Selecting **Publish** publishes at once, with no confirmation dialog (a dialog opens only for warnings, see
  `Assets/designer/src/App.vue:493-515`), and shows the toast "Version 1 was published." and the **Version 1** pill
  (`data-cy="published-version"`).
- **Save indicator.** `run-1` says "The designer saves the draft as you work". The source shows the autosave
  (`App.vue:112`, the documentation says the same) but not the label of the shared save indicator. The plan highlights
  nothing for it; if the take should show it, find its selector with `Recorder probe`.
- **Fit control.** `build-6` may need the canvas control that fits the steps. `Assets/designer/src/i18n.ts:14` names the
  text "Fit to the steps" (`FitToContent`), but the control itself is in the shared canvas. If no such control exists, collapse
  the **Pipeline** panel (`[data-cy="panel-collapse"]`) before `build-6` so the three steps fit.
- **Data set dialog.** The dialog is the shared `ModalDialog`. The plan clicks the data source pills
  (`nav[aria-label="Data sources"] button`) and the card buttons by the `data-cy` values read in `DataSetPicker.vue`;
  confirm that the attributes reach the elements the plan targets.

## 2. Layout at 1536x864

Whether the three steps (240 px wide each, about 100 px apart: `Assets/designer/src/canvas/geometry.ts:22`) and the step panel
fit at the recording size, with the admin menu and the toolbox collapsed and the **Pipeline** panel open (384 px by default,
`PipelinePanel.vue:77`). If the **Maximum number of rows** label of `build-2` is hidden under the step panel, scroll the panel or
pin it (`[data-cy="step-panel-pin"]`) before the narration reaches it.

## 3. The Features page and the admin menu

- The id of the **Enable** link (`btn-enable-OrchardCore_DataPipelines`) is built by `Html.GenerateIdFromName`
  (`src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`); the plan assumes the dots
  become underscores. Confirm with `Recorder probe`, and fix the plan if the id differs.
- The rows listed for the search `data pipelines`: the plan expects **Data Pipelines**, **Data Pipelines - FTP** and **Data Pipelines - SFTP**
  (the manifests name them), and the Core feature hidden by the **Hide on demand** filter. After `enable-2`, show the hidden
  features once to confirm that **Data Pipelines Core** was enabled with it (the narration says it is; it follows from the
  `Dependencies` of `Manifest.cs` and from `ShellDescriptorFeaturesManager.GetFeaturesToEnable`).
- What the page shows after **Enable**: the notification reads "The feature Data Pipelines was enabled." and names only the feature
  that was clicked (`OrchardCore.Features/Controllers/AdminController.cs:313`), so **Data Pipelines Core** is not named on
  screen. The page that follows the click is served after the shell restarts, which may take longer than the 30 s default
  navigation wait of the recorder; confirm the time on the first take and raise the timeout if needed.
- The admin menu markup (`button` for a group, `a` for a page, `src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml`)
  and whether the **Tools** group is still open after the page reloads. The plan clicks **Tools** only when it is closed.

## 4. What the first run shows

- The preview of **Blog Post** (`build-3`) lists the five fake posts, and the **Fields** tab lists the fields with their types.
- The run ends within the narration of `run-2`, or `OffCameraAsync` cuts the wait. The step badges of `run-3` read the number of rows
  and files each step handled (`runs/runOverlay.ts:46-68`): "5 rows" for **Read a data source**, "1 file" for **Create a file**, and
  for **Save to the media library**, which has no output and a file input only, what it received: "1 file" (the badge shows rows only for a step with a
  row port). Confirm the three badges.
- The **Delivered** list of `run-4` reads "Saved 'blog-posts.csv' to the media library." (`Steps/Media/SaveToMediaStep.cs:87`), so the file is
  at the root of the media library (the **Folder** setting is empty).
- The **Run History** page reached from the breadcrumb is the page of one pipeline ("The runs of Blog posts, the most recent first.")
  and its link **See every pipeline** leads to the page of every pipeline (`Views/Admin/Runs.cshtml:31-32`).

## 5. Text on screen that looks like code

An admin video shows no code. Some screens of the product hold text that looks like code, written by the product itself, not
presented by the video. The plan avoids highlighting or scrolling to it, but it is visible. A maintainer should confirm that
this is acceptable in an admin video:

- **File name** (`build-5`): the box opens holding the default `{PipelineName}-{Date:yyyyMMdd-HHmmss}`
  (`OrchardCore.DataPipelines.Core/Steps/Files/CreateFileStepSettings.cs`), shows the same text as its placeholder once it is
  cleared, and has a hint that names placeholders between braces (`Views/CreateFile.Fields.Edit.cshtml:19-20`).
- **Read a data source** (`build-2`): the hint under **Filters** names a placeholder between braces
  (`Views/DataSource.Fields.Edit.cshtml:71`), and the **Parameters** box is in a monospace font with the placeholder `name=value`
  (line 102). The options of **Fields to read** read like "Title (TitlePart.Title)" (line 49); the plan does not open that list.
- **Fields tab** (`build-3` ends on it): each field shows its reference in square brackets, in a code element
  (`Assets/designer/src/panel/FieldsTab.vue:222`, `preview/previewTable.ts:106`).

If this is not acceptable, the video needs another way to show these screens (for example a take that ends `build-3` on the
**Data** tab, and a narration without the **Fields** tab); the narration cannot hide them.

## 6. Pronunciation

The storyboard speaks "FTP", "SFTP" and "CSV" as "F.T.P.", "S.F.T.P." and "C.S.V.", and "blog-posts" as "blog posts". "C.S.V." is the
spelling the specification names; "F.T.P." and "S.F.T.P." are new. A person must confirm all four with `scripts/pronounce.py` before
the narration is generated.

# Open questions: Data Pipelines, export content items to a CSV file

The narration states only what was read in the Orchard Core source (commit `92db4a83`) and in the documentation pages
of the video. These are the points the source cannot settle. Each one says what was checked and what a person must
confirm on a running site before the first take. The references are in the `OrchardCMS/OrchardCore` repository.

Short names: `DP` is `src/OrchardCore.Modules/OrchardCore.DataPipelines`, `UI` is `DP/Assets/designer/src`.

## 1. How the voice says "CSV"

Checked: `narrate.py` has no entry for "CSV", and `storyboard.json` has an empty `spoken` map. The specification
(`references/specs.md`, "Voice") names "C.S.V." as the way to write a letter-by-letter initialism, and says a person
confirms each new spelling.

To confirm: run `scripts/pronounce.py` on a sentence of the video (for example step `file-2`) with the spellings `CSV`
and `C.S.V.`. If `CSV` is not read as three letters, add `"CSV": "C.S.V."` to `spoken`. The narration never speaks the
extension `.csv`: it says "blog-posts" and "CSV file".

## 2. The "Fields to read" control (`source-4`)

Checked: `DP/Views/DataSource.Fields.Edit.cshtml:41-54` renders a `<select multiple data-dp-select data-live-search="true"
data-actions-box="true" data-show-selected-tags="true">` with the title "Every field", turned into a searchable
multi-select by `window.Selectpicker` (`DP/Assets/Scripts/data-pipelines-editors.js:30-38`). The DOM that component
draws (the button, the search box, the option rows) is not in the source tree that was read.

To confirm: with `Recorder probe` on the designer, find the button of the control (the one whose label is "Every field"),
its search box, and the rows of the options, and write the three locators into `Scenes.cs`. Check that clicking an
option applies the selection (the step card changes to "4 fields").

## 3. The labels in the list of fields (`source-4`, `source-5`)

Checked in the code, not on a site:

- The option texts are "label (technical name)" (`DP/Views/DataSource.Fields.Edit.cshtml:49`). The labels of the common
  fields are `Author`, `Created` (name `CreatedUtc`) and `Published` (`DP/DataSources/Contents/ContentDataColumns.cs:49,50,52`).
- The label of the title is "Title" (name `TitlePart.Title`): the part's label is the part name without "Part"
  (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-64`;
  the migration of `TitlePart` sets no display name, `src/OrchardCore.Modules/OrchardCore.Title/Migrations.cs:31`), and
  `ContentDataColumns.cs:79` keeps "Title" instead of "Title Title".
- The label of the subtitle is "Subtitle" (name `BlogPost.Subtitle`): the display name of the field in the Blog recipe
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, part `BlogPost`).

To confirm: the four options exist with these labels, and the filter field list has `Published (Boolean)` (that list
shows the technical name and the type, `field.Name (field.Type)`, in `DP/Views/Shared/DataPipelineFieldOptions.cshtml:11`).
Also note the order of the columns of the preview and of the CSV file: the hint of the control says "The fields to
read, in this order." (`DP/Views/DataSource.Fields.Edit.cshtml:54`), and the editor stores the fields in the order the
form posts them (`DP/Drivers/SourceAndFileStepDisplayDrivers.cs:91`), which is the order of the options of a native
multi-select, not necessarily the order of the clicks. The narration never states an order.

The first row of the CSV file with **Name the columns with the labels of the fields** on is expected to read Title,
Subtitle, Author, Created (`DS/Files/CsvDataFileFormat.cs:49`). The narration names Title and Created as examples,
since the label of Author is the same as its technical name.

## 4. Texts of the shared designer components (`create-4`, `publish-1`, `publish-2`, `run-1`)

Checked: `Draft saved`, `No issues: the pipeline can be published.`, `Version {0} was published.` and `The run was
queued.` come from `UI/App.vue`, `UI/i18n.ts:11` and, for the save status, from the built script of the designer
(`DP/wwwroot/Scripts/DataPipelines/designer/data-pipelines-designer.js`, key `DraftSaved`), because the package that
holds the shared components is not in the source tree.

To confirm on a running site: the toolbar says "Draft saved" for a newly created pipeline (`HasDraft` is true at
`DP/Controllers/DataPipelineDesignerController.cs:127`, because `DataPipelineManager.CreateAsync` gives the pipeline an
empty draft at line 118), the message after **Publish** is shown long enough to be read, and no confirmation dialog
appears when the pipeline has no warning (`decidePublish`, shared component).

Also confirm what the toolbar shows while the description is typed. The description is saved by the settings endpoint
(`DP/Controllers/DataPipelineDesignerController.cs:473-509`), not by the autosave that shows "Saving…", and the shared
form component (`ServerFormHost`, not in the source tree) may or may not change the status. The narration only says that
the designer saves a draft as you work and that the toolbar says "Draft saved", which holds either way. After
**Publish**, the toolbar is expected to read "No unpublished changes" (the built script, key `NoUnpublishedChanges`),
not "Draft saved"; the narration of `publish-2` names the message and the version badge only.

## 5. The seed import (section 1 of `recording-plan.md`)

Checked: the Contents module's `content` recipe step imports the items with `IContentManager.ImportAsync`
(`src/OrchardCore.Modules/OrchardCore.Contents/Recipes/ContentStep.cs`). The type `BlogPost` is not marked Creatable in
the Blog recipe, and the data source lists every type the user may view
(`DP/DataSources/Contents/ContentItemsDataSource.cs:60-77`), so imported posts without a container are read.

To confirm: the eight posts exist after the import, with the author `admin`, the given creation dates, and the two
drafts as unpublished (`Published` false, `Latest` true). If the import ignores the dates or the author, the video is
not affected (nothing reads them aloud). Do not accept a site where the preview shows other than six rows.

The Blog recipe does not mark the `BlogPost` type as listable (`blog.recipe.json`, type `BlogPost`: `Draftable` and
`Versionable` only), so the Content Items list neither offers the type in its drop-down nor lists its items without a
type term (`src/OrchardCore.Modules/OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs:182-190`,
`Controllers/AdminController.cs:901`). The plan therefore uses the search term `type:BlogPost`, which the list accepts
for a type that is not listable (same file, line 145). To confirm: the search term lists the sample post and, after
the import, the eight posts, and the **Actions** menu of a row has **Delete**.

The data source reads the latest version of the items for a user who may edit the type, drafts included
(`DP/DataSources/Contents/ContentItemsDataSource.cs:116,129-131`), and the filter on **Published** drops the two
drafts (same file, line 179). The administrator holds the edit permission, so the six rows depend on that filter, as the
narration of `source-5` says.

## 6. Where the file lands (`deliver-2`, `media-1`)

Checked: `FileSystemStore.CreateFileFromStreamAsync` creates the missing folders
(`src/OrchardCore/OrchardCore.FileStorage.FileSystem/FileSystemStore.cs:372-374`), so the folder `exports` does not
need to exist. The narration does not say the pipeline creates it. Other media stores (Azure Blob, Amazon S3) were not
read.

To confirm: after `08-run`, the media library shows the folder `exports` with the file `blog-posts.csv`, with no suffix
such as "(2)" in its name.

## 7. The list of the dialog of the data sets (`source-1`, `source-2`)

Checked: the buttons of the left column are built from the data sources that have at least one data set
(`UI/dataSets/DataSetPicker.vue:51-66`), and the Blog recipe creates the query `RecentBlogPosts` and has the
`Queries` and `Users` features enabled. The filter matches the name, the technical name, the description, the group and
the data source, so typing `blog` also finds the query `RecentBlogPosts`.

To confirm: the left column has the buttons All, Content items, Queries and Users (the order depends on the order of the
data sources), and the cards after `blog` are Blog, Blog Post and RecentBlogPosts.

## 8. The run (`run-1` to `run-3`)

Checked: a run is dispatched when the request that queued it ends (`DP/Services/DataPipelineRunDispatcher.cs:62`); the
background task only starts runs that were missed (`DP/BackgroundTasks/DataPipelineRunsBackgroundTask.cs`). The badges of
the canvas count rows read and written and files written (`UI/runs/runOverlay.ts:46-68`, and the metrics in
`src/OrchardCore/OrchardCore.DataPipelines.Core/Services/DataPipelineChannelPorts.cs`): "6 rows" on the first card, and
"1 file" on the two others.

To confirm: the run ends in a few seconds, the badges read as above, and the log line reads "Created the file
'blog-posts.csv' with 6 rows.".

The breadcrumb of the run page has a **Run History** link (`DP/Views/Admin/Run.cshtml:11`) that is scoped to the
pipeline of the run: it opens "Run History of Blog posts export" (`DP/Views/Admin/Runs.cshtml:7`), not the runs of every
pipeline. The narration of `run-3` says only that the link lists the runs of this pipeline. To confirm: the link text
and the title of the page it opens.

## 9. The warning of the Save to the media library step (`deliver-3`)

Checked: with the default media options (`.csv` and `.xlsx` allowed; `.json`, `.jsonl` and `.zip` not) the editor shows a
warning that lists `.json, .jsonl, .zip` and names the `OrchardCore:Media:AllowedFileExtensions` setting
(`DP/Views/SaveToMedia.Fields.Edit.cshtml:16`, `DP/Drivers/MediaStepDisplayDrivers.cs:31-40`). A comment in the driver
(line 31) speaks of ".csv files with its default options", but the default list of the Media module does include
`.csv` (`src/OrchardCore.Modules/OrchardCore.Media/Services/MediaOptionsConfiguration.cs:32`) and the documentation says
the same, so the code wins.

To confirm: the warning does not list `.csv`. The text of the warning names a configuration key on screen, which an
admin video otherwise avoids; this is also raised as a question about the specification.

# Recording plan: Data Pipelines, export content items to a CSV file

This plan is for the person who records the video. Read it from top to bottom once, then follow the sections in
order. Nothing in it needs a decision: every value to type, every label to click, and what must be on screen when a
step's narration ends, is written down. Selectors stand for `scene.Page.Locator(...)`. File references are in the
`OrchardCMS/OrchardCore` repository, at the commit the site is built from.

Abbreviations used in the references:

| Short | Path |
|-------|------|
| `DP` | `src/OrchardCore.Modules/OrchardCore.DataPipelines` |
| `UI` | `src/OrchardCore.Modules/OrchardCore.DataPipelines/Assets/designer/src` (the Vue designer) |
| `DPC` | `src/OrchardCore/OrchardCore.DataPipelines.Core` |
| `DS` | `src/OrchardCore/OrchardCore.DataSources.Core` |

The designer is a Vue application whose shared components (save status, publish dialog, toasts) come from a package
that is not in the Orchard Core source tree. Their texts (`Draft saved`, `Saving…`, `Version {0} was published.`) are
in the built script `DP/wwwroot/Scripts/DataPipelines/designer/data-pipelines-designer.js`, searched by key
(`DraftSaved`, `PublishedVersion`). The first take must confirm them on screen (see `OPEN-QUESTIONS.md`).

## 1. The site it needs

- **Build**: `src/OrchardCore.Cms.Web` of an Orchard Core checkout that has the Data Pipelines module, served on
  `http://localhost:5010`. Its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Setup**: Auto Setup, so no setup form and no sign-in form is recorded (this video is not about the Setup screen).
  Values, passed as environment variables (the password is chosen outside git and passed in `VIDEO_PASSWORD`; write
  `<password>` anywhere it has to be named):

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address. This video calls no external service.
- **Time zone**: `America/Los_Angeles` (the dates of the preview are in UTC and are not read aloud).
- **Features to enable before recording** (Tools, Features, search for the name, enable, outside the capture):
  - `Data Pipelines` (`OrchardCore.DataPipelines`). Its dependencies, `Data Pipelines Core` and `Users`, come with it.
  - The Blog recipe has already enabled what the video uses: `Contents`, `Media`, `Queries` (the Queries data source
    adds a "Queries" button to the dialog of the data sets) and `Deployment` (for the seed).
  - Leave `Email`, `Workflows`, `Data Pipelines - FTP` and `Data Pipelines - SFTP` disabled: the toolbox then has no
    Send by email, Share a download link, FTP or SFTP step. It still lists steps the narration does not name (Read a
    media file, Zip files, Send to a web API, Save content items, and the transforms), so show a list of steps only
    as long as a step needs it (see the notes of `file-1` and `deliver-1`).
- **Settings**: none. Do not set `OrchardCore:Media:AllowedFileExtensions` or any `OrchardCore:DataPipelines` option.
  With the default media options the editor of the **Save to the media library** step shows a warning that names
  JSON, JSON Lines and zip files, and accepts CSV files. The narration of `deliver-3` describes that warning, so it
  must be on screen.
- **Sign-in**: `Recorder setup` once, outside the capture (user `admin`, `VIDEO_PASSWORD`), which writes `state.json`
  (git-ignored). Every clip starts signed in.
- **Menu**: collapse the admin menu before `StartCaptureAsync` on every clip that starts on the designer (the
  designer needs the whole width). The clips `02-create` and `09-media` use the menu on camera, so they start with
  it expanded and the entry they use closed.

### The seed data (outside the capture)

All names and titles are invented. The blog posts below are the only blog posts of the site: eight posts, six
published and two drafts, so the Published filter of the video visibly drops two rows.

1. Delete the sample post the Blog recipe creates (Content, Content Items, type `type:BlogPost` in the **Search** box
   and press Enter, the post whose title starts with "Man must explore", **Actions**, **Delete**, **Ok**). The type
   drop-down of the list does not offer Blog Post, because the Blog recipe does not mark the type as listable, and the
   list without a type shows listable types only; the `type:` term works for any type
   (`src/OrchardCore.Modules/OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs:145,182-190`,
   `src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:901`,
   `Views/ContentsButtonActions_SummaryAdmin.cshtml:40`). The six rows of the narration depend on it.
2. Import the eight posts: Tools, Deployments, JSON Import, paste the text below into the **JSON** box, select
   **Import**. The text is a recipe with one `content` step, handled by the Contents module
   (`src/OrchardCore.Modules/OrchardCore.Contents/Recipes/ContentStep.cs`).

```json
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {"ContentItemId": "4624xkvlhtri46pncxab3e9gsu", "ContentItemVersionId": "4c8z8sw5gvllpwmqyyy282dr1g", "ContentType": "BlogPost", "DisplayText": "Welcome to the Orchard Notes blog", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-01-12T09:00:00Z", "ModifiedUtc": "2026-01-12T09:00:00Z", "PublishedUtc": "2026-01-12T09:00:00Z", "TitlePart": {"Title": "Welcome to the Orchard Notes blog"}, "AutoroutePart": {"Path": "blog/welcome-to-the-orchard-notes-blog"}, "MarkdownBodyPart": {"Markdown": "Sample text for the welcome to the orchard notes blog post."}, "BlogPost": {"Subtitle": {"Text": "A first look at what we plan to share"}}},
        {"ContentItemId": "4zb340af7op63lnn8q2ky0kcxi", "ContentItemVersionId": "45vl1gfpsgxkfw0oe3ptz5rzqu", "ContentType": "BlogPost", "DisplayText": "How we prune apple trees in winter", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-01-26T10:30:00Z", "ModifiedUtc": "2026-01-26T10:30:00Z", "PublishedUtc": "2026-01-26T10:30:00Z", "TitlePart": {"Title": "How we prune apple trees in winter"}, "AutoroutePart": {"Path": "blog/how-we-prune-apple-trees-in-winter"}, "MarkdownBodyPart": {"Markdown": "Sample text for the how we prune apple trees in winter post."}, "BlogPost": {"Subtitle": {"Text": "Three cuts that keep the canopy open"}}},
        {"ContentItemId": "4h9ahcmlt753uhtn3huoubdm7y", "ContentItemVersionId": "4add0ymvezb9md6iv4qovnflfu", "ContentType": "BlogPost", "DisplayText": "Choosing rootstock for a small garden", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-02-09T08:15:00Z", "ModifiedUtc": "2026-02-09T08:15:00Z", "PublishedUtc": "2026-02-09T08:15:00Z", "TitlePart": {"Title": "Choosing rootstock for a small garden"}, "AutoroutePart": {"Path": "blog/choosing-rootstock-for-a-small-garden"}, "MarkdownBodyPart": {"Markdown": "Sample text for the choosing rootstock for a small garden post."}, "BlogPost": {"Subtitle": {"Text": "Size, vigor and soil in plain terms"}}},
        {"ContentItemId": "45rqhvi5zyorv2l1zomxgif49g", "ContentItemVersionId": "43jtsoc2tiy9xm3f661sovm6kj", "ContentType": "BlogPost", "DisplayText": "Our spring planting calendar", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-02-23T14:00:00Z", "ModifiedUtc": "2026-02-23T14:00:00Z", "PublishedUtc": "2026-02-23T14:00:00Z", "TitlePart": {"Title": "Our spring planting calendar"}, "AutoroutePart": {"Path": "blog/our-spring-planting-calendar"}, "MarkdownBodyPart": {"Markdown": "Sample text for the our spring planting calendar post."}, "BlogPost": {"Subtitle": {"Text": "What goes in the ground, and when"}}},
        {"ContentItemId": "4o571hplvy8zd3r1b8ip56lqh3", "ContentItemVersionId": "4eci9vf3sermb07xfoq7pf0p34", "ContentType": "BlogPost", "DisplayText": "Pollination: bees, wind and neighbors", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-03-09T11:45:00Z", "ModifiedUtc": "2026-03-09T11:45:00Z", "PublishedUtc": "2026-03-09T11:45:00Z", "TitlePart": {"Title": "Pollination: bees, wind and neighbors"}, "AutoroutePart": {"Path": "blog/pollination-bees-wind-and-neighbors"}, "MarkdownBodyPart": {"Markdown": "Sample text for the pollination: bees, wind and neighbors post."}, "BlogPost": {"Subtitle": {"Text": "Why one tree is rarely enough"}}},
        {"ContentItemId": "424yynt72vgtroxs4zbb5hkd7f", "ContentItemVersionId": "4el2hs9uu3awrvu168cwy1gsts", "ContentType": "BlogPost", "DisplayText": "Harvest day volunteer guide", "Latest": true, "Published": true, "Author": "admin", "CreatedUtc": "2026-03-23T09:30:00Z", "ModifiedUtc": "2026-03-23T09:30:00Z", "PublishedUtc": "2026-03-23T09:30:00Z", "TitlePart": {"Title": "Harvest day volunteer guide"}, "AutoroutePart": {"Path": "blog/harvest-day-volunteer-guide"}, "MarkdownBodyPart": {"Markdown": "Sample text for the harvest day volunteer guide post."}, "BlogPost": {"Subtitle": {"Text": "Tasks, times and what to bring"}}},
        {"ContentItemId": "4ywpgvjryist72jmzwyt5i490t", "ContentItemVersionId": "4dazvwie91u72c5df1tj8s1es0", "ContentType": "BlogPost", "DisplayText": "Cider pressing notes", "Latest": true, "Published": false, "Author": "admin", "CreatedUtc": "2026-04-06T13:00:00Z", "ModifiedUtc": "2026-04-06T13:00:00Z", "TitlePart": {"Title": "Cider pressing notes"}, "AutoroutePart": {"Path": "blog/cider-pressing-notes"}, "MarkdownBodyPart": {"Markdown": "Sample text for the cider pressing notes post."}, "BlogPost": {"Subtitle": {"Text": "Work in progress"}}},
        {"ContentItemId": "4elug5k3unz97xicdj6dshcp2n", "ContentItemVersionId": "4va4jezg8rm4na7jnsux87yvb3", "ContentType": "BlogPost", "DisplayText": "Winter storage ideas", "Latest": true, "Published": false, "Author": "admin", "CreatedUtc": "2026-04-20T15:20:00Z", "ModifiedUtc": "2026-04-20T15:20:00Z", "TitlePart": {"Title": "Winter storage ideas"}, "AutoroutePart": {"Path": "blog/winter-storage-ideas"}, "MarkdownBodyPart": {"Markdown": "Sample text for the winter storage ideas post."}, "BlogPost": {"Subtitle": {"Text": "Notes for a later post"}}}
      ]
    }
  ]
}
```

3. Check the result in Content, Content Items (type `type:BlogPost` in the **Search** box, as in step 1): eight posts,
   the last two listed as drafts. If the import changed the author or dates (see `OPEN-QUESTIONS.md`), the narration is not affected: it
   never reads a date or an author aloud. If the import fails, create the same eight posts in any other way that
   gives the same titles, subtitles, published state and the author `admin`.
4. Do not create any pipeline, and do not create the `exports` folder of the media library: the pipeline creates it.
   The media library keeps the three images the Blog recipe uploads (`about-bg.jpg`, `home-bg.jpg`, `post-bg.jpg`).
5. Stop the site and copy its `App_Data` folder and `state.json` aside. This is the snapshot **S0**.

## 2. Order of the clips, and snapshots

The clips build one pipeline, so each clip starts from the site the previous clip left. Record them in this order,
which is also the order they play in. The slide clips need no site.

| Order | Clip | Starts from | Changes the site |
|-------|------|-------------|------------------|
| 1 | `02-create` | S0 | Creates the pipeline "Blog posts export" with its description |
| 2 | `03-source` | S1 (after `02-create`) | Adds the Read a data source step, its fields and its filter |
| 3 | `04-file` | S2 | Adds the Create a file step |
| 4 | `05-deliver` | S3 | Adds the Save to the media library step |
| 5 | `06-publish` | S4 | Publishes the pipeline (its first published version) |
| 6 | `08-run` | S5 | Runs it once; the file `exports/blog-posts.csv` appears in the media library |
| 7 | `09-media` | S6 (after `08-run`) | Nothing |

Take a snapshot (stop the site, copy `App_Data` and `state.json`, start the site) after each clip: S1 after
`02-create`, S2 after `03-source`, and so on. To record a clip again, stop the site, restore the snapshot that
precedes it, and start the site. A full new take starts from S0. Never record `08-run` twice on the same site: a
second run would leave two runs in the history, and the video must show exactly one.

Slide clips: `01-idea` and `07-access` are drawn by `slides.py`; there is nothing to record.

## 3. Conditions for every recorded clip

- Open the clip's first page with `scene.OpenAsync(...)` before `StartCaptureAsync`. The pipeline's identifier is
  random, so open the list (`/Admin/DataPipelines`) and click the pipeline's name ("Blog posts export") instead of
  typing an address. Wait for `[data-cy=designer-toolbar]` before starting the capture.
- Keep the actions of a step shorter than its narration. The recorder holds the step until its narration ends.
- Every field that applies on blur needs a Tab press after typing.
- Highlights are fixed-position: call `ClearHighlightsAsync()` before scrolling and before the next step.
- The toolbox and the right panel are open by default; do not collapse them. The one exception is the group
  **Transforms** of the toolbox: its list holds steps whose editors use formulas, which this video does not show.
  Before `StartCaptureAsync` of every clip that starts on the designer (`03-source`, `04-file`, `05-deliver`,
  `06-publish`, `08-run`), click its heading (`[data-cy=toolbox-category-Transform]`) so the group is collapsed. The
  choice is not saved, so it has to be repeated after each page load.
- The step panel is not pinned; it opens over the bottom of the canvas when a step is added or selected.

## 4. The clips

### Clip `02-create`: Create the pipeline

Starts on `/Admin` (the dashboard), signed in as `admin`, with the **Tools** entry of the admin menu closed.

- `create-1`: click **Tools**, then **Data Pipelines** in the admin menu (let the viewer read the two entries
  **Pipelines** and **Run History**; highlight both for a moment), then click **Pipelines** with
  `ClickAndWaitForNavigationAsync`. When the narration ends, the page **Data Pipelines** is shown: the search box,
  the **Add Pipeline** button and the message "Nothing here! There are no data pipelines for the moment."
  Labels: `DP/AdminMenu.cs:18-26`, `DP/Views/Admin/Index.cshtml:20,118`.
- `create-2`: click **Add Pipeline** (`ClickAndWaitForNavigationAsync`). On the page **Add Pipeline**, type
  `Blog posts export` into the **Name** field (`TypeAsync`), then click **Create** (`ClickAndWaitForNavigationAsync`).
  When the narration ends, the designer is loading or open (wait for `[data-cy=designer-toolbar]`).
  Labels: `DP/Views/Admin/Create.cshtml:11,20`, `DP/Controllers/AdminController.cs:93-124`.
- `create-3`: the designer is open on an empty pipeline. Click the heading **Transforms** of the toolbox
  (`[data-cy=toolbox-category-Transform]`) so that the four headings **Sources**, **Transforms**, **Files** and
  **Destinations** are all visible without scrolling (this is not saved; it resets on the next page load). Highlight
  the toolbox (`[data-cy=designer-toolbox]`), then the canvas (`[data-cy=designer-canvas]`, which shows "Start with a
  source"), then the tabs of the right panel (`[data-cy=designer-panel]`): **Pipeline**, **Issues**, **History**.
  When the narration ends, the whole designer is visible and the three highlights are cleared.
  Labels: `DP/Services/DataPipelineDesignerModelBuilder.cs:303-312` (the headings are plural),
  `UI/App.vue:816` (**Steps**), `UI/panel/PipelinePanel.vue:89` (the tabs), `UI/canvas/StartPicker.vue:20`.
- `create-4`: click the **Pipeline** tab (`[data-cy=panel-tab-pipeline]`) if it is not selected. In the **Description**
  box (`scene.Page.GetByLabel("Description")` inside `[data-cy=panel-settings-form]`) type
  `Exports the published blog posts to a CSV file.`, then press Tab. Highlight the save status
  (`[data-cy=save-status]`), which reads **Draft saved**. A new pipeline is created with an empty draft
  (`DP/Services/DataPipelineManager.cs:118`), so the toolbar reads **Draft saved** from the moment the designer
  opens. The description is not part of the draft: the form posts it to the settings endpoint
  (`DP/Controllers/DataPipelineDesignerController.cs:473-509`, `DataPipelineManager.UpdateSettingsAsync`), which does
  not go through the autosave, so **Saving…** is not expected to show. Wait for the text **Draft saved** before
  highlighting, and keep what is on screen if **Saving…** shows anyway. When the narration ends, the description is in
  the box and the toolbar shows **Draft saved**.
  Labels: `DP/Views/DataPipelineDesigner/DataPipelineDesignerSettings.cshtml:12,15`; **Draft saved** is the key
  `DraftSaved` of the shared save-status component (it shows when the status is saved and the pipeline has a draft;
  the built script `data-pipelines-designer.js`); the designer is told the pipeline has a draft at
  `DP/Controllers/DataPipelineDesignerController.cs:127`.

### Clip `03-source`: Read the blog posts

Starts on the designer of "Blog posts export" (open the list, click the name), empty canvas, **Pipeline** tab
selected, the admin menu collapsed.

- `source-1`: click the toolbar button **Add a data source** (`[data-cy=toolbar-add-data-source]`). The dialog **Add a
  data source** opens (`[data-cy=data-set-picker]`). Highlight its left column (`nav[aria-label="Data sources"]`),
  which lists **All**, **Content items**, **Queries** and **Users**, each with a count. When the narration ends, the
  dialog is open, the cards of the data sets are visible, and the left column is highlighted.
  Labels: `UI/App.vue:707`, `UI/dataSets/DataSetPicker.vue:61,70,83`, the data source names `Content items`
  (`DP/DataSources/Contents/ContentItemsDataSource.cs:56`), `Queries`
  (`DP/DataSources/Queries/QueriesDataSource.cs:44`), `Users` (`DP/DataSources/Users/UsersDataSource.cs:50`).
- `source-2`: click the **Filter** box (`[data-cy=data-set-filter]`) and type `blog`. The cards **Blog**, **Blog Post**
  and **RecentBlogPosts** (a query, badge "Queries") remain. Click **Add** on the **Blog Post** card
  (`[data-cy="data-set-Contents::BlogPost"] [data-cy=data-set-choose]`), in the last second of the narration. When
  the narration ends, the dialog has closed or is closing.
  Labels: `UI/dataSets/DataSetPicker.vue:78,123`; the card title is the display name of the content type
  (`DP/DataSources/Contents/ContentItemsDataSource.cs:262`, "Blog Post" from the Blog recipe).
- `source-3`: wait for the step: a card **Read a data source** appears on the canvas (`.dpd-node` with that text) and
  the step panel opens below the canvas on the **Settings** tab. Highlight the card, then the **Data set** box of the
  panel, which shows "Blog Post" and "Content items" with the button **Change**. When the narration ends, the card
  reads "Content items › Blog Post" and "Every field".
  Labels: `DPC/Steps/Sources/DataSourceStep.cs:43`, `DP/Views/DataSource.Fields.Edit.cshtml:12,30`,
  `DP/Views/DataSource.Fields.Design.cshtml:9,11`.
- `source-4`: in the **Fields to read** control (it shows the title "Every field"; it is a searchable multi-select, see
  `OPEN-QUESTIONS.md`), open the list and select the four options whose text starts with `Title (`, `Subtitle (`,
  `Author (` and `Created (`: type the word in the list's search box, then click the option (the search for
  `Title` also finds `Subtitle (BlogPost.Subtitle)`, so click the option that starts with `Title (`). Close the list
  with Escape. When the narration ends, the control shows the four chosen fields and the card reads "4 fields".
  Labels: `DP/Views/DataSource.Fields.Edit.cshtml:41-54`; the option texts are "label (technical name)" and the labels
  come from `DP/DataSources/Contents/ContentDataColumns.cs:49,52,79` (Author, Created, Title) and from the field's
  display name in the Blog recipe (Subtitle).
- `source-5`: click **Add a filter** (`[data-dp-add]` under **Filters**). In the new row, choose the field
  `Published (Boolean)` in the first drop-down (`SelectAsync`, aria-label "Field"; its value is `Published`, not
  `PublishedUtc`), leave or choose the comparison **equals** (aria-label "Comparison"), and choose **Yes** in the value
  drop-down (`[data-dp-filter-boolean]`, aria-label "Value", options "Choose a value", "Yes", "No"; the text box
  `[data-dp-filter-value]` has the same aria-label and is hidden for a Boolean field, so do not locate the drop-down
  by its label alone). Press Tab. When the narration ends, the row
  reads Published, equals, Yes, and the card reads "4 fields · 1 filter".
  Labels: `DP/Views/DataSource.Fields.Edit.cshtml:59,70`, `DP/Views/Shared/DataPipelineDataSourceFilterRow.cshtml:13,16,28-31,47`,
  `DP/DataSources/Contents/ContentDataColumns.cs:50` (**Published**).
- `source-6`: click the tab **Data** of the step panel (`[data-cy=step-tab-data]`), then **Preview**
  (`[data-cy=preview-run]`). Wait for the table (`table.dpd-preview-table`). When the narration ends, the section
  **Rows** shows "6 rows": six published posts, the columns Title, Subtitle, Author and Created in the order the
  preview gives, and neither "Cider pressing notes" nor "Winter storage ideas".
  Labels: `UI/panel/StepPanel.vue:97`, `UI/panel/PreviewTab.vue:56,58,80`, `UI/runs/runOverlay.ts:17`.

### Clip `04-file`: Write the CSV file

Starts on the designer after S2: the card **Read a data source** is on the canvas, nothing is selected, the step panel
is closed.

- `file-1`: highlight the output port of the card **Read a data source** (`button[data-port-side=output]` inside that
  card; its title says "Rows (Rows) · Click to add the next step, or drag to connect"), then click it. A small panel
  **Add a step after Rows** opens with a search box (placeholder "Search the steps"); type `file`; click **Create a
  file** (`[data-cy=quick-add-CreateFile]`). When the narration ends, a card **Create a file** is on the canvas,
  connected to the first card, and the step panel shows its settings. The port is a bare dot on the card (its name
  is only in the tooltip), so call `HighlightAsync(port, "Rows")` to put the name the narration says on screen, and
  clear it before the list opens. Start typing in the search box at once: the unfiltered list also offers steps whose
  editors use formulas, and it should not stay on screen.
  Labels: `UI/canvas/StepPort.vue:66-76`, `UI/canvas/QuickAddMenu.vue:95,108`, `DPC/Steps/Files/CreateFileStep.cs:33`.
- `file-2`: highlight the **Format** drop-down, which reads "CSV (comma separated values) (.csv)". In the **File name**
  box, which holds the default text for the name, select everything (triple click) and type `blog-posts`, then press
  Tab. When the narration ends, the box reads `blog-posts` and the card reads the format and the name.
  Labels: `DP/Views/CreateFile.Fields.Edit.cshtml:4,9,17`, `DS/Files/CsvDataFileFormat.cs:23`,
  `DPC/Steps/Files/CreateFileStepSettings.cs:14,20` (CSV and the default name are the defaults),
  `DPC/Steps/DataPipelineTemplate.cs:95` (`WithExtension` adds the extension).
- `file-3`: highlight the checked box **Start with a row of column names**, then click the unchecked box **Name the
  columns with the labels of the fields**. When the narration ends, both boxes are checked. With the second box on,
  the first row of the file holds the labels Title, Subtitle, Author and Created; with it off, the technical names
  TitlePart.Title, BlogPost.Subtitle, Author and CreatedUtc. The narration names Title and Created as the examples
  because Author is the same in both.
  Labels: `DP/Views/CreateFile.Fields.Edit.cshtml:40,49`, `DPC/Steps/Files/CreateFileStepSettings.cs:25,31`
  (the first is on by default, the second off), `DS/Files/CsvDataFileFormat.cs:49`.
- `file-4`: highlight the **CSV delimiter** box (it holds a comma) and its hint "A comma by default; type tab for
  tab-separated values.". Change nothing. When the narration ends, the box and the hint are highlighted.
  Labels: `DP/Views/CreateFile.Fields.Edit.cshtml:68,71`.
- `file-5`: clear the highlights; click the tab **Data**, then **Preview**. Wait for the list of files
  (`[data-cy=preview-files]`). When the narration ends, the section **Files** shows one file: `blog-posts.csv`,
  `text/csv`, its size and "6 rows".
  Labels: `UI/panel/PreviewTab.vue:56,115`, `DS/Files/CsvDataFileFormat.cs:27-29`.

### Clip `05-deliver`: Save the file

Starts on the designer after S3: two cards on the canvas, nothing selected, the step panel closed.

- `deliver-1`: call `HighlightAsync(port, "Files")` on the output port of the card **Create a file** (a bare dot; its
  tooltip says "Files (Files)"), clear it, then click the port; the panel **Add a step after Files** opens; type
  `media` at once; click **Save to the media library** (`[data-cy=quick-add-SaveToMedia]`).
  When the narration ends, a third card **Save to the media library** is connected to the second card and its
  settings are open (the box **Folder**, a yellow warning, the box **Replace a file with the same name**).
  Labels: `DP/Steps/Media/SaveToMediaStep.cs:31`, `UI/canvas/QuickAddMenu.vue:95`.
- `deliver-2`: click the **Folder** box (placeholder "exports") and type `exports`, then press Tab. When the narration
  ends, the box reads `exports` and the card reads `/exports`.
  Labels: `DP/Views/SaveToMedia.Fields.Edit.cshtml:4,7`, `DP/Views/SaveToMedia.Fields.Design.cshtml:3`.
- `deliver-3`: highlight the yellow warning (`[data-dp-rejected-extensions]`). It reads "The media library doesn't
  accept .json, .jsonl, .zip files, so a run fails when it saves one. Create files in a format it accepts, such as an
  Excel workbook, or add the extensions to its allowed file extensions (the OrchardCore:Media:AllowedFileExtensions
  setting)." Scroll the step panel if the warning is cut off (clear the highlight first). The warning is part of the
  editor and names a configuration key on screen; do not zoom in on it and do not read it aloud (the narration does not).
  When the narration ends, the whole warning is visible.
  Labels: `DP/Views/SaveToMedia.Fields.Edit.cshtml:16`, `DP/Drivers/MediaStepDisplayDrivers.cs:31-40`,
  `src/OrchardCore.Modules/OrchardCore.Media/Services/MediaOptionsConfiguration.cs:32` (`.csv` is in the default list;
  `.json`, `.jsonl` and `.zip` are not).
- `deliver-4`: highlight the checked box **Replace a file with the same name** and its hint "Otherwise the file is
  saved under a name with a number appended.". Change nothing. When the narration ends, both are highlighted.
  Labels: `DP/Views/SaveToMedia.Fields.Edit.cshtml:26,27`, `DP/Steps/Media/SaveToMediaStepSettings.cs:17`
  (on by default).
- `deliver-5`: clear the highlights; click the tab **Data**, then **Preview**. Wait for the section **Files**. When the
  narration ends, the blue note "Destinations are never executed by a preview; this is what the step would
  receive." is shown, with the file `blog-posts.csv` under it.
  Labels: `UI/panel/PreviewTab.vue:70`.

### Clip `06-publish`: Publish the pipeline

Starts on the designer after S4: three cards connected in a row, the **Pipeline** tab selected, nothing selected.

- `publish-1`: click the tab **Issues** of the right panel (`[data-cy=panel-tab-issues]`). When the narration ends,
  the tab shows "No issues: the pipeline can be published." and has no red or yellow count.
  Labels: `UI/panel/PipelinePanel.vue:89,94`, `UI/i18n.ts:11`.
- `publish-2`: highlight the **Publish** button (`[data-cy=toolbar-publish]`), then click it. No dialog appears (a
  dialog only appears for warnings or errors). A message "Version 1 was published." shows for a few seconds, and the
  badge "Version 1" (`[data-cy=published-version]`) appears next to the pipeline name; the **Run** button becomes
  enabled. Click **Publish** in the first half of the narration so the message is on screen while it plays. When the
  narration ends, the badge and the enabled **Run** button are visible.
  Labels: `UI/App.vue:471-473,678-680,764,771`.

### Clip `08-run`: Run the pipeline

Starts on the designer of the published pipeline after S5 (open the list, click the name), **Pipeline** tab selected,
the admin menu collapsed.

- `run-1`: click **Run** (`[data-cy=toolbar-run]`). The message "The run was queued." shows, and the **History** tab
  opens (`[data-cy=panel-tab-runs]`) with the new run, first Queued or Running. Do not wait for the end of the run
  here. When the narration ends, the **History** tab is open and shows one run.
  Labels: `UI/App.vue:600-604,759-764`, `UI/panel/PipelinePanel.vue:89`.
- `run-2`: wait for the status **Succeeded** in the run row (`[data-cy=runs-tab] .badge`, polled by the designer every
  2 seconds; the whole run lasts a few seconds). If it has not started after 20 seconds, wrap the wait in
  `scene.OffCameraAsync(...)` with the condition "the badge of the first run reads Succeeded" (the background task
  starts queued runs every minute). Then highlight, one after the other, the badge "6 rows" of the card **Read a data
  source**, the badges "1 file" of the cards **Create a file** and **Save to the media library**, and, in the
  **History** tab (scroll the panel body to it, after clearing the highlights), the section **Deliveries**, whose
  entry reads "Saved 'exports/blog-posts.csv' to the media library.". When the narration ends, the status
  **Succeeded**, the badges and the delivery line are visible (all at once if the panel size allows; otherwise the
  delivery line and the badges in turn).
  Labels: `UI/runs/runOverlay.ts:17,19,27`, `UI/panel/RunsTab.vue:159`, `DP/Steps/Media/SaveToMediaStep.cs:87`.
- `run-3`: click **Open the run** (`[data-cy=run-page]`, `ClickAndWaitForNavigationAsync`). On the run page, highlight
  in turn the headings **Steps**, **Delivered** and **Log**, then the breadcrumb link **Run History** while the
  narration says "The Run History link there lists the runs of this pipeline": the link goes to **Run History of Blog
  posts export**, the runs of this pipeline only (`DP/Views/Admin/Run.cshtml:11`, `DP/Views/Admin/Runs.cshtml:7`), not
  to the runs of every pipeline, which the **Run History** entry of the **Tools** menu lists. Do not click the
  breadcrumb link, and do not click the delivered link (it downloads the file). When the narration ends, the run page shows the badge Succeeded, the
  table of steps (Rows in, Rows out, Files, Warnings), the delivered line and the log line "Created the file
  'blog-posts.csv' with 6 rows.".
  Labels: `UI/panel/RunsTab.vue:126`, `DP/Views/Admin/Run.cshtml:11,63,69-72,98,119`,
  `DPC/Steps/Files/CreateFileStep.cs:114`.

### Clip `09-media`: Find the file

Starts on `/Admin` signed in, with the **Media** entry of the admin menu closed.

- `media-1`: click **Media** in the admin menu, then **Library** (`ClickAndWaitForNavigationAsync`). In the folder tree
  of the media library (left), click the folder `exports`. When the narration ends, the list of files shows
  `blog-posts.csv` and nothing else.
  Labels: `src/OrchardCore.Modules/OrchardCore.Media/AdminMenu.cs:18,21`; the root of the folder tree is "Media
  Library" (`Assets/media-gallery/src/services/HierarchicalTreeBuilder.ts:37` in the Media module).
- `media-2`: highlight the row of `blog-posts.csv`. Do not click it and do not download it. When the narration ends,
  the row is highlighted.

## 5. The slides and the cards

Drawn by `slides.py`, nothing to record. One line each:

- `idea-1`: four cards (Source, Transform, File, Destination); the first (Source) is bright.
- `idea-2`: the same four cards; the second (Transform, "Optional") is bright.
- `idea-3`: the same four cards; the third (File) and the fourth (Destination) are bright.
- `access-1`: two cards, "Publish" and "Run", about the access of the user who published the pipeline.
- `access-2`: three cards, the names of the three permissions: Manage data pipelines (security critical), Run data
  pipelines, View data pipelines and their runs.
- `intro` and `outro` are the opening and closing cards; nothing to record.

The chapter cards (chapters 01 to 09) are drawn by the assembly; they have no narration.

## 6. Checks for private data

- Everything on screen is invented: the pipeline name, the description, the eight blog posts (titles and
  subtitles), the folder and file names. The only account is `admin` with the address `admin@example.com`. The password
  is never typed on camera (`state.json` holds the session) and never written in a file of the repository.
- Author in the preview and the file is `admin`. No real person, address, phone number or token appears.
- The media library shows only the three images of the Blog recipe and `blog-posts.csv`.
- OCR the assembled video at one frame per second and at every scene change, and search for anything that is not in
  the lists above (the checks in `SKILL.md`, step 6). The address bar is not recorded, so `localhost:5010` and the
  random pipeline identifier do not appear.
- Keep `state.json`, the snapshots (`App_Data` copies) and any mask script outside git.

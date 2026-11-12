# Recording plan: Data Pipelines overview

How to record `data-pipelines-overview` without having to decide anything. A selector in this plan stands for
`scene.Page.Locator(...)`. File references are to the Orchard Core source (`OrchardCMS/OrchardCore`, `src/`), at the
commit the content was written against. The designer is a Vue application; its labels are in
`src/OrchardCore.Modules/OrchardCore.DataPipelines/Assets/designer/src` (`DP/designer` below), its server-rendered
pages in `src/OrchardCore.Modules/OrchardCore.DataPipelines` (`DP` below).

The video has six chapters between the cards: two are slides (nothing to record), four are recordings
(`02-enable`, `03-create`, `04-build`, `05-run`).

## 1. The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout that contains the Data Pipelines module |
| Port | 5010 (`VIDEO_BASE_URL=http://localhost:5010`) |
| App_Data | its own folder outside both repositories (`ORCHARD_APP_DATA`) |
| Setup | Auto Setup, no setup form and no sign-in form on camera: tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, recipe `Blog`, SQLite, user `admin` with email `admin@example.com` |
| Password | chosen outside git, passed as `VIDEO_PASSWORD`; written `<password>` everywhere in the repository |
| Environment variables | `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address; the video needs no external service and no local stand-in |
| Sign-in | `Recorder setup` once, outside the capture (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD`), saved as `state.json` (git-ignored) |
| Media settings | leave the `OrchardCore:Media:AllowedFileExtensions` setting out of the configuration: the default list accepts `.csv`, which the run saves (`src/OrchardCore.Modules/OrchardCore.Media/Services/MediaOptionsConfiguration.cs:32`) |
| Admin settings | leave them at their defaults: `05-run` navigates through the breadcrumb trail (`nav[aria-label="Breadcrumb"]`), which the **Show breadcrumb** setting hides when unchecked; it is checked by default and the Blog recipe does not change it (`src/OrchardCore/OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:23`) |

Features: the Blog recipe already enables Contents, Media, Queries, SQL queries, Users and the Admin menu
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-80`). **Do not enable Data Pipelines before recording:**
`02-enable` enables it on camera. The video needs no other feature: leave Email, Workflows, FTP and SFTP disabled.

Settings: none. The site keeps the Blog recipe's defaults.

### Seed data (create outside the capture)

The Blog recipe creates one Blog Post, "Man must explore, and this is exploration at its greatest". Add four more, all
fake, so the preview and the file hold a few rows. In the admin menu open **Blog** (an item the Blog recipe adds), select
**Create Blog Post** (`src/OrchardCore.Modules/OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52`), type the title
and a one-sentence body, and publish. Leave the other fields empty.

| Title | Body (one sentence) |
|---|---|
| A weekend at the lake | Two quiet days by the water, with a book and a small boat. |
| Ten tips for a tidy workshop | Small habits that keep every tool where it belongs. |
| Spring menu at the harbor cafe | The dishes we cook while the mornings are cool. |
| Notes from a small orchard | Pruning, picking and a short list of lessons. |

Do not create content items with real names. The author of every item is the `admin` user.

## 2. Order, dependencies and snapshots

The clips change the site and each one starts from what the previous one left, so record them in this order and keep a
copy of `App_Data` (stop the site, copy the folder and `state.json`, start the site) at each snapshot.

| Snapshot | State | Taken | Needed by |
|---|---|---|---|
| S0 | Fresh site, seed data created, signed in, **Data Pipelines disabled** | before `02-enable` | `02-enable` |
| S1 | Data Pipelines (and Data Pipelines Core) enabled, no pipeline | after `02-enable` | `03-create` |
| S2 | Pipeline named `Blog posts` exists, empty draft | after `03-create` | `04-build` |
| S3 | `Blog posts` holds the three steps, a draft that was never published, no run, no CSV file in the media library | after `04-build` | `05-run` |

To take a clip again, stop the site, restore the snapshot the clip needs (the `App_Data` copy), and start the site.
Before stopping the site for a snapshot, wait about 10 seconds after the clip ended: the designer saves moves of the steps
with a short delay (the steps and their connections are saved when they are added).
`05-run` publishes, runs and saves a file to the media library, so restore S3 before every take of it.

Slides (chapters 01 and 06) and the cards (00 and 99) are drawn from `slides.py`; nothing is recorded for them.

Common settings of the four recordings:

- Viewport and scene API as in `references/recording.md`. Open the first page with `OpenAsync`, do the "Before capture"
  actions, then `StartCaptureAsync`.
- Navigate on camera only with `ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync` or `NavigateAsync`.
- A step's actions are shorter than its narration. The narration lengths below are estimates at about 2.5 words a
  second; `durations.json` has the real ones.
- Designer pages: the admin menu is collapsed for `03-create` onwards (click `.leftbar-compactor`, the button titled
  "Collapse / expand menu", `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:79`). A clip starts in a new browser
  context, so every clip does it again. `02-enable` keeps the menu expanded.
- Where a wait is slow (the shell restarts after a feature is enabled, a run executes), wrap the wait in
  `OffCameraAsync` with a condition that is false at the start, as the step says.

## 3. Clips

### Clip `02-enable` (chapter 02, from S0)

Starts on `/Admin` (the admin home page: the Blog recipe does not enable the admin dashboard). Before capture:
`OpenAsync("/Admin")`. The menu is expanded and every group is collapsed.

`enable-1` (about 6 s)
- Actions: `ClickAsync` the button `#ta-left-sidebar button:has-text("Tools")` (pauseAfter 500). Then
  `ClickAndWaitForNavigationAsync` the link `#ta-left-sidebar a[href$="/Admin/Features"]` (the **Features** item of
  Tools). Then `TypeAsync` the box `#search-box` with `data pipelines`.
- On screen when the narration ends: the Features page, the Search box holding `data pipelines`, and the rows **Data
  Pipelines**, **Data Pipelines - FTP** and **Data Pipelines - SFTP**, each with an **Enable** button. The row of
  **Data Pipelines Core** is not listed (the **Hide on demand** filter is on by default).
- Labels: Tools and Features in `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; the search box in
  `.../OrchardCore.Features/Views/Admin/Features.cshtml:27`; the feature names in
  `.../OrchardCore.DataPipelines/Manifest.cs` and `.../OrchardCore.DataPipelines.Ftp/Manifest.cs`.

`enable-2` (about 13 s)
- Actions: the **Enable** link of the row named exactly **Data Pipelines** (not the FTP or SFTP row) is
  `#btn-enable-OrchardCore_DataPipelines`. Clicking it posts a form and the server redirects to the Features page, which is a
  full page load, so never click it with `ClickAsync` (the capture would freeze): `MoveToAsync` the link (the cursor rests on it
  for a moment), then call `OffCameraAsync` with a wait that runs `ClickAndWaitForNavigationAsync` on the link
  `scene.Page.Locator("#btn-enable-OrchardCore_DataPipelines")` and then `WaitForAsync()` on the locator
  `#btn-disable-OrchardCore_DataPipelines` (it does not exist before the click). The first request after the feature is
  enabled can take longer than the 30 s default of a navigation: call `scene.Page.SetDefaultNavigationTimeout(120000)` before the
  click if the take times out. The click itself is not on camera, which is the intended cut. Then, if
  `#ta-left-sidebar button:has-text("Data Pipelines")` is not visible, `ClickAsync` `#ta-left-sidebar button:has-text("Tools")`
  (the script that restores open groups may have left Tools open already; never click Tools when it is open). Then
  `ClickAsync` the button `#ta-left-sidebar button:has-text("Data Pipelines")`, and `HighlightAsync` the links
  `#ta-left-sidebar a[href$="/Admin/DataPipelines"]` and `#ta-left-sidebar a[href$="/Admin/DataPipelines/Runs"]`
  (hold about 2 s each, `ClearHighlightsAsync` after).
- On screen when the narration ends: the Features page with the Tools group open, its **Data Pipelines** section open and
  the two links **Pipelines** and **Run History** under it. A notification reads "The feature Data Pipelines was enabled."; it
  names only the feature that was clicked, so **Data Pipelines Core** is not named on screen
  (`src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:313`, called with the requested features by
  `src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:80`).
- Labels: Data Pipelines, Pipelines and Run History in `DP/AdminMenu.cs:18-25`; Core is a dependency of Data Pipelines in
  `DP/Manifest.cs` (`Dependencies`), and the Features page enables the dependencies with the feature
  (`src/OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:163`).
- Before the first take, confirm the Enable link's id with `Recorder probe /Admin/Features "() => [...document.querySelectorAll('a[id^=btn-enable-OrchardCore_DataPipelines]')].map(a => a.id)"`
  (id built by `Html.GenerateIdFromName`, `.../OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
  This clip changes the site: take S1 afterwards.

### Clip `03-create` (chapter 03, from S1)

Starts on `/Admin/DataPipelines`, the Pipelines page, empty. Before capture: `OpenAsync("/Admin/DataPipelines")`;
`ClickAsync` `.leftbar-compactor`.

`create-1` (about 7 s)
- Actions: `HighlightAsync` the message `.list-group .alert-info` (hold 2.5 s), `ClearHighlightsAsync`, `HighlightAsync`
  the button `a.create` (hold 1.5 s), `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` `a.create`.
- On screen when the narration ends: the **Add Pipeline** page, with the **Name** field and the **Create** button.
- Labels: "Nothing here! There are no data pipelines for the moment." and "Add Pipeline" in `DP/Views/Admin/Index.cshtml:20,118`.

`create-2` (about 6 s)
- Actions: `TypeAsync` the field `input[name="Name"]` with `Blog posts`, then `ClickAndWaitForNavigationAsync` the button
  `button.save` (labeled **Create**). Wrap the wait for the designer in `OffCameraAsync` with the condition
  "`[data-cy="start-picker"]` is visible" (false until the designer has loaded).
- On screen when the narration ends: the designer of **Blog posts**: the toolbar with the title, the empty canvas and the
  **Start with a source** card.
- Labels: **Name** and **Create** in `DP/Views/Admin/Create.cshtml:11,20`; "Start with a source" in
  `DP/designer/canvas/StartPicker.vue:20`.

`create-3` (about 15 s)
- Actions, in this order, each followed by `ClearHighlightsAsync`: `HighlightAsync` `[data-cy="toolbox-sheet"]` (the
  **Steps** list with its groups **Sources**, **Transforms**, **Files**, **Destinations**; hold 6 s), `HighlightAsync`
  `[data-cy="start-picker"]` (hold 3 s), `HighlightAsync` `[data-cy="designer-panel"] ul[role="tablist"]` (the tabs
  **Pipeline**, **Issues**, **History**; hold 4 s). Do not click anything.
- On screen when the narration ends: the whole designer, the **Steps** list on the left, the empty canvas, the panel on
  the right with its three tabs.
- Labels: **Steps** in `DP/designer/App.vue:816`; the group names in `DP/Services/DataPipelineDesignerModelBuilder.cs:303-312`;
  the tab names in `DP/designer/panel/PipelinePanel.vue:89`.
- This clip changes the site: the pipeline exists. Take S2 afterwards.

### Clip `04-build` (chapter 04, from S2)

Starts on the designer of **Blog posts**. Before capture: `OpenAsync("/Admin/DataPipelines")`; `ClickAsync` `.leftbar-compactor`;
`ClickAndWaitForNavigationAsync` the link `a:has-text("Blog posts")`; wait until `[data-cy="start-picker"]` is visible;
`ClickAsync` `[data-cy="toolbox-collapse"]` (the toolbox becomes a rail and the canvas gains its width).

`build-1` (about 13 s)
- Actions: `ClickAsync` the button `[data-cy="toolbar-add-data-source"]` (**Add a data source**); wait until a card
  `[data-cy^="data-set-Contents::"]` is visible (many cards match: use the locator's `.First`, since a locator that matches
  several elements fails in strict mode). After about 4 s `ClickAsync` `nav[aria-label="Data sources"] button:has-text("Content items")`;
  after about 3 s more `ClickAsync` `nav[aria-label="Data sources"] button:has-text("Users")`; after about 2 s more
  `ClickAsync` `nav[aria-label="Data sources"] button:has-text("Queries")`.
- On screen when the narration ends: the **Add a data source** dialog on its **Queries** pill, listing the saved query
  **RecentBlogPosts**.
- Labels: "Add a data source" in `DP/designer/App.vue:707`; the dialog in `DP/designer/dataSets/DataSetPicker.vue:70-97`; the
  names of the data sources in `src/OrchardCore.Modules/OrchardCore.DataPipelines/DataSources/Contents/ContentItemsDataSource.cs:56`
  ("Content items"), `.../Users/UsersDataSource.cs:50` ("Users"), `.../Queries/QueriesDataSource.cs:44` ("Queries").

`build-2` (about 13 s)
- Actions: `ClickAsync` `nav[aria-label="Data sources"] button:has-text("Content items")`; `TypeAsync`
  `[data-cy="data-set-filter"]` with `Blog Post`; `ClickAsync` the button
  `[data-cy="data-set-Contents::BlogPost"] [data-cy="data-set-choose"]` (labeled **Add**). Wait until the node
  `.dpd-node:has-text("Read a data source")` and the panel `[data-cy="step-panel"]` are visible. Then `HighlightAsync`, one at a time
  (hold 1.5 s each, `ClearHighlightsAsync` between, scroll the panel with `ScrollAsync` first if the label is hidden, never
  while a highlight is shown): the labels `[data-cy="panel-step-form"] label:has-text("Fields to read")`,
  `label:has-text("Filters")` and `label:has-text("Maximum number of rows")`.
- On screen when the narration ends: the canvas with the step **Read a data source** (**Content items › Blog Post**,
  **Every field**), and the **Settings** tab of its panel showing the fields to read, the filters and the maximum number
  of rows.
- Labels: `DP/Views/DataSource.Fields.Edit.cshtml:41,59,76` and `DP/Views/DataSource.Fields.Design.cshtml`; the step name in
  `src/OrchardCore/OrchardCore.DataPipelines.Core/Steps/Sources/DataSourceStep.cs:43`.

`build-3` (about 11 s)
- Actions: `ClickAsync` the tab `[data-cy="step-tab-data"]` (**Data**); `ClickAsync` the button `[data-cy="preview-run"]`
  (**Preview**); wait until `.dpd-preview-table` is visible (hold about 3 s on the rows). Then `ClickAsync` the tab
  `[data-cy="step-tab-fields"]` (**Fields**) and wait until `[data-cy="fields-tab"]` lists fields.
- On screen when the narration ends: the **Fields** tab, listing the fields of the Blog Post data set with their types.
- Labels: tab names in `DP/designer/panel/StepPanel.vue:97`; **Preview** and its hint in `DP/designer/panel/PreviewTab.vue:56-58`.

`build-4` (about 12 s)
- Actions: `ClickAsync` the output port `.dpd-node:has-text("Read a data source") [data-port-side="output"]`; wait until
  `[data-cy="quick-add"]` is visible (hold 3 s: it shows **Add a step after ...** and the steps that receive rows); then `ClickAsync`
  `[data-cy="quick-add-CreateFile"]` (**Create a file**). Wait until `.dpd-node:has-text("Create a file")` is visible and connected.
- On screen when the narration ends: two connected steps, **Read a data source** and **Create a file**, with the settings of
  **Create a file** open in the panel.
- Labels: `DP/designer/canvas/QuickAddMenu.vue:88,95,127`; the port's hint in `DP/designer/canvas/StepPort.vue` ("Click to add the next step, or drag to connect");
  the step name in `src/OrchardCore/OrchardCore.DataPipelines.Core/Steps/Files/CreateFileStep.cs:33`.

`build-5` (about 13 s)
- Actions: `HighlightAsync` `[data-cy="panel-step-form"] label:has-text("Format")` (hold 2 s; the box reads **CSV (comma separated values)** followed by its extension,
  the default); `ClearHighlightsAsync`; clear the file name box (`[data-cy="panel-step-form"] input[name$="FileName"]`, which opens holding the default
  `{PipelineName}-{Date:yyyyMMdd-HHmmss}`) with the locator's own `FillAsync("")` (the scene has no fill method), `TypeAsync` it
  with `blog-posts`, then press Tab with `scene.Page.Keyboard.PressAsync("Tab")` (the box applies on blur). `HighlightAsync` the
  box (hold 2 s).
  Do not highlight or scroll to the hint under the file name box.
- On screen when the narration ends: the **Create a file** settings with **Format** on CSV and **File name** holding
  `blog-posts`; the node shows **blog-posts** under the format.
- Labels: `DP/Views/CreateFile.Fields.Edit.cshtml:4,17`; the node text in `DP/Views/CreateFile.Fields.Design.cshtml`.

`build-6` (about 15 s)
- Actions: `ClickAsync` the output port `.dpd-node:has-text("Create a file") [data-port-side="output"]`; wait until
  `[data-cy="quick-add"]` is visible (hold 3 s: it lists the steps that receive files); `ClickAsync` `[data-cy="quick-add-SaveToMedia"]`
  (**Save to the media library**). Wait until `.dpd-node:has-text("Save to the media library")` is visible. `HighlightAsync`
  `[data-cy="panel-step-form"] label:has-text("Folder")` (hold 2 s), `ClearHighlightsAsync`, `HighlightAsync`
  `[data-cy="panel-step-form"] label:has-text("Replace a file with the same name")` (hold 2 s). Leave **Folder** empty and the
  box checked (the defaults).
- On screen when the narration ends: three connected steps on the canvas (**Read a data source**, **Create a file**, **Save to
  the media library**), the settings of the last one in the panel. If the third step is outside the visible canvas, use the
  canvas control titled "Fit to the steps" (`DP/designer/i18n.ts:14`) before the narration ends.
- Labels: `DP/Views/SaveToMedia.Fields.Edit.cshtml:4,26`; the step name in `DP/Steps/Media/SaveToMediaStep.cs:31`. The step is offered
  because the Media feature is enabled (`DP/Startups/MediaStartup.cs`).
- Do not publish. This clip changes the site: the draft holds three steps. Take S3 afterwards.

### Clip `05-run` (chapter 05, from S3)

Starts on the designer of **Blog posts**. Before capture: `OpenAsync("/Admin/DataPipelines")`; `ClickAsync` `.leftbar-compactor`;
`ClickAndWaitForNavigationAsync` the link `a:has-text("Blog posts")`; wait until three `.dpd-node` are visible; `ClickAsync`
`[data-cy="toolbox-collapse"]`; make sure no step is selected (click an empty spot of the canvas). The panel on the right is on
its **Pipeline** tab.

`run-1` (about 11 s)
- Actions: after about 4 s, `ClickAsync` the tab `[data-cy="panel-tab-issues"]` (**Issues**) and hold; after about 3 s more,
  `ClickAsync` the button `[data-cy="toolbar-publish"]` (**Publish**). Wait until `[data-cy="published-version"]` is visible.
  If a confirmation dialog opens instead (the pipeline has warnings), stop and write what it says in the take's notes: the
  video does not cover it (see OPEN-QUESTIONS.md).
- On screen when the narration ends: the toolbar with the **Version 1** pill next to the title and the **Run** button enabled;
  the **Issues** tab open.
- Labels: tab names `DP/designer/panel/PipelinePanel.vue:89`; **Publish** `DP/designer/App.vue:771`; the toast "Version 1 was
  published." `DP/designer/App.vue:472`; "No issues: the pipeline can be published." `DP/designer/i18n.ts:11`.

`run-2` (about 9 s)
- Actions: `ClickAsync` the button `[data-cy="toolbar-run"]` (**Run**). The panel moves to its **History** tab (`[data-cy="runs-tab"]`) and
  lists the run. Wrap the wait for the run to end in `OffCameraAsync` with the condition "the badge in `.dpd-run-head` reads
  **Succeeded**" (it reads **Queued** or **Running** first).
- On screen when the narration ends: the **History** tab with one run badged **Succeeded**, **Version 1**, **Manual by admin**, and the
  toast "The run was queued." (or already gone).
- Labels: **Run** `DP/designer/App.vue:764`; the toast `DP/designer/App.vue:604`; the badges `DP/designer/runs/runOverlay.ts:23-30`.

`run-3` (about 10 s)
- Actions: the run is already selected after **Run** (clicking it again would deselect it: do not click). `HighlightAsync`
  `.dpd-run-head` (hold 2.5 s), `ClearHighlightsAsync`, then `HighlightAsync` the badges `[data-cy="run-badge"]` of the three steps in turn, as
  `.Nth(0)`, `.Nth(1)` and `.Nth(2)` of that locator (a locator that matches three elements fails in strict mode and rings
  nothing), 1.5 s each, with `ClearHighlightsAsync` between. The badges read what each step handled
  (`DP/designer/runs/runOverlay.ts:46-68`): **5 rows** for **Read a data source**, **1 file** for **Create a file**, and
  **1 file** for **Save to the media library**, whose only port is a file input, so its badge counts the files it received and no
  rows (`DataPipelineStepType.cs`, `GetInputs`, in `src/OrchardCore/OrchardCore.DataPipelines.Abstractions/Steps/`); confirm on the take.
- On screen when the narration ends: the canvas with a status badge on each of the three steps, and the run's details in the panel.
- Labels: `DP/designer/panel/RunsTab.vue`, `DP/designer/runs/runOverlay.ts:17-19`.

`run-4` (about 13 s)
- Actions: `ClickAndWaitForNavigationAsync` the link `[data-cy="run-page"]` (**Open the run**). On the run page `HighlightAsync`, in turn
  (hold 2 s each, `ClearHighlightsAsync` between): the heading `h5:has-text("Steps")` with its table, the heading
  `h5:has-text("Delivered")` with its list, and the heading `h5:has-text("Log")`. Do not click the delivered link: it opens the file.
- On screen when the narration ends: the page **Run of Blog posts**, the table with the columns **Rows in**, **Rows out**, **Files** and
  **Warnings**, the **Delivered** list reading "Saved 'blog-posts.csv' to the media library.", and the **Log**.
- Labels: `DP/Views/Admin/Run.cshtml:9,63,69-70,98,119`; the delivery text `DP/Steps/Media/SaveToMediaStep.cs:87`.

`run-5` (about 11 s)
- Actions: `ClickAndWaitForNavigationAsync` `nav[aria-label="Breadcrumb"] a:has-text("Run History")`. Hold 5 s on the page, which reads
  "The runs of Blog posts, the most recent first." with the run (**Succeeded**, **Version 1**, the duration, **Manual · admin**).
  Then `ClickAndWaitForNavigationAsync` the link `a:has-text("See every pipeline")`.
- On screen when the narration ends: the **Run History** page of every pipeline, with the same run.
- Labels: `DP/Views/Admin/Runs.cshtml:31-32,56,103,108`.

`run-6` (about 11 s)
- Actions: `ClickAndWaitForNavigationAsync` `nav[aria-label="Breadcrumb"] a:has-text("Data Pipelines")`. `HighlightAsync` the badge
  `li[data-pipeline-id] .badge[title="The published version runs."]` (**Version 1**, hold 1.5 s), `ClearHighlightsAsync`, then
  `HighlightAsync` the link `li[data-pipeline-id] a.badge[title="Last run"]` (**Succeeded** and its age, hold 1.5 s), and
  `ClearHighlightsAsync`. The row has a third badge (**Modified**, `title="Modified"`) that the narration does not name, so
  never highlight `li[data-pipeline-id] .badge` as a whole: it matches three elements and fails in strict mode. Then `ClickAsync` the button
  `li[data-pipeline-id] button.actions` (**Actions**). Do not click an item of the menu.
- On screen when the narration ends: the Pipelines page with **Blog posts**, its badges, and the open **Actions** menu showing **Run now**,
  **Run history**, **Start over** and **Delete**.
- Labels: `DP/Views/Admin/Index.cshtml:56,71,85-99`.
- This clip changes the site (a run, a file in the media library): restore S3 before taking it again.

## 4. The slides (nothing to record)

| Step | The slide shows |
|---|---|
| `why-1` | "Data that has to move": three cards, Export, Send, Import. |
| `why-2` | "A pipeline is a set of connected steps": four cards, with Sources and Transforms bright. |
| `why-3` | The same four cards, with Files and Destinations bright. |
| `more-1` | "More than a first export": Transforms, Destinations, Workflows. |
| `more-2` | "Who can use pipelines": the three permissions, then a fourth card marked "!" for the publisher's access. |

## 5. Checks for private data

- The only user on screen is `admin` (top bar, **Manual · admin**, the Author column of the preview). No email address is displayed on
  any page of the video; if one appears, it must be `admin@example.com`.
- The preview shows the five fake Blog Posts and opaque identifiers (content item ids, the owner's user id); nothing else.
- The Features page lists no tenant and no real names. The run page and the log show only the pipeline's own messages.
- Search the OCR of the video, at one frame per second and at every scene change, for any name, email address or domain that is not the
  fake data above, `example.com` or `orchardcore.net`.
- The password never appears: the recorder signs in outside the capture, and `state.json` is not committed.

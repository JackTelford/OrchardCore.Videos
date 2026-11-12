# Recording plan: Data Pipelines: Import Rows as Content Items

How to record `data-pipelines-import` without having to decide anything. Selectors stand for `scene.Page.Locator(...)`.
`DP` below is `src/OrchardCore.Modules/OrchardCore.DataPipelines` in the Orchard Core source, and `DPD` is
`DP/Assets/designer/src` (the Vue source of the designer, built into `DP/wwwroot/Scripts/DataPipelines/designer`). Every
`file:line` names where a label on screen is written. The video has 5 slides (steps `how-1` to `how-3`, `rej-1` and
`rej-2`, drawn by `slides.py`) and 4 recordings (clips `02-read`, `03-save`, `04-run`, `05-check`).

## 1. The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with
  `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories. Outbound calls blocked with `HTTP_PROXY` and
  `HTTPS_PROXY` set to a dead address. Nothing in this video needs the network.
- **Auto Setup** (the host has `OrchardCore.AutoSetup` registered, see `references/recording.md`), with these environment
  variables, so no Setup screen and no sign-in form is recorded:
  - `OrchardCore__AutoSetup__Tenants__0__ShellName` = `Default`
  - `OrchardCore__AutoSetup__Tenants__0__SiteName` = `Orchard Core`
  - `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` = `America/Los_Angeles`
  - `OrchardCore__AutoSetup__Tenants__0__AdminUsername` = `admin`
  - `OrchardCore__AutoSetup__Tenants__0__AdminEmail` = `admin@example.com`
  - `OrchardCore__AutoSetup__Tenants__0__AdminPassword` = `<password>` (the one chosen outside git, also passed to the
    recorder as `VIDEO_PASSWORD`)
  - `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` = `Sqlite`
  - `OrchardCore__AutoSetup__Tenants__0__RecipeName` = `Blog`
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` saves `state.json` (git-ignored).
- **Features**: the Blog recipe already enables Contents, Content Types, Content Fields, Title, Media and Deployment, which
  the video uses. Only **Data Pipelines** (`OrchardCore.DataPipelines`) has to be enabled; it brings along the Data Pipelines
  Core and Users features. Workflows, Email, FTP and SFTP stay disabled: the pipeline has no use for them.
- **External services**: none.
- **Settings**: none to change. The media library accepts `.csv` files by default
  (`src/OrchardCore.Modules/OrchardCore.Media/Services/MediaOptionsConfiguration.cs:32`).

### Seed data, created before any capture

1. **Enable the feature.** Sign in, open **Tools > Deployments > JSON Import** (`/Admin/DeploymentPlan/Import/Json`), paste
   this into the **JSON** box, click **Import**, then wait for the page to load again:

   ```json
   {
     "steps": [
       { "name": "feature", "enable": [ "OrchardCore.DataPipelines" ] }
     ]
   }
   ```

2. **Create the Product content type**, the same way (**Tools > Deployments > JSON Import**). A Product has a title and a
   Product part with a required Price (two decimals, at least 0) and a Category:

   ```json
   {
     "steps": [
       {
         "name": "ContentDefinition",
         "ContentTypes": [
           {
             "Name": "Product",
             "DisplayName": "Product",
             "Settings": {
               "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true }
             },
             "ContentTypePartDefinitionRecords": [
               { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
               { "PartName": "Product", "Name": "Product", "Settings": { "ContentTypePartSettings": { "Position": "1" } } }
             ]
           }
         ],
         "ContentParts": [
           {
             "Name": "Product",
             "Settings": {},
             "ContentPartFieldDefinitionRecords": [
               {
                 "FieldName": "NumericField",
                 "Name": "Price",
                 "Settings": {
                   "ContentPartFieldSettings": { "DisplayName": "Price", "Position": "0" },
                   "NumericFieldSettings": { "Required": true, "Scale": 2, "Minimum": 0 }
                 }
               },
               {
                 "FieldName": "TextField",
                 "Name": "Category",
                 "Settings": {
                   "ContentPartFieldSettings": { "DisplayName": "Category", "Position": "1" }
                 }
               }
             ]
           }
         ]
       }
     ]
   }
   ```

   Check it: **Design > Content Definition > Content Types** lists **Product**, and `/Admin/Contents/ContentItems/Product`
   lists no item.

3. **Put the file in the media library**: create `imports/products.csv` in the media folder of the tenant, which is
   `<ORCHARD_APP_DATA>/Sites/Default/Media/imports/products.csv` (UTF-8, any line ending). The exact content, five rows
   under the header, the last one without a price:

   ```text
   Name,Price,Category
   Reading lamp,34.90,Lighting
   Standing desk,329.00,Furniture
   Task chair,189.50,Furniture
   Ceramic mug,12.00,Kitchen
   Wall shelf,,Furniture
   ```

   Alternative with the same result: in **Media > Library**, create the folder `imports` and upload the file into it by
   hand (the scene API has no file upload, so it is never done on camera). Check it: **Media > Library**, folder
   `imports`, lists `products.csv`.

4. **Check the starting state**: `/Admin/DataPipelines` shows "Nothing here! There are no data pipelines for the moment."
   and `/Admin/DataPipelines/Runs` shows no run.

Then stop the site and copy `ORCHARD_APP_DATA` aside as snapshot `seeded`.

## 2. Order of the clips and snapshots

The clips depend on each other: each starts from the site the previous one left. Record them in this order, copying
`ORCHARD_APP_DATA` aside when a clip ends (stop the site first). The slides need no site.

| Clip | Starts from | Changes the site | Snapshot to keep afterwards |
|---|---|---|---|
| `02-read` | `seeded` | Creates the pipeline "Import products" with its first step (a draft). | `after-02` |
| `03-save` | `after-02` | Adds the Save content items step and its settings to the draft. | `after-03` |
| `04-run` | `after-03` | Publishes the pipeline, runs it: four Product items, one run. | `after-04` |
| `05-check` | `after-04` | Runs the pipeline a second time: four items updated, a second run. | none |

To record a clip again, stop the site, restore the snapshot it starts from (`seeded` for `02-read`), start the site, and
sign in with the saved `state.json`. To record everything again, restore `seeded`.

Clips `03-save`, `04-run` and `05-check` start on a pipeline that exists already. Its address holds a random identifier, so
each of them opens `/Admin/DataPipelines`, clicks **Edit** on the row "Import products" (the button
`a.btn-primary` of the row, `DP/Views/Admin/Index.cshtml:84`), and waits for the designer (`[data-cy='data-pipeline-designer']`)
and for the step cards to be drawn (`[data-node-id]`, the card of a step; `[data-cy^='step-']` would also match the step panel and its tabs, `DPD/canvas/StepNode.vue:182-185`), all before `StartCaptureAsync`.

### Designer layout, for the clips that use it

The designer is wide: the toolbox on the left, the canvas, the pipeline panel (**Pipeline**, **Issues**, **History**) on the
right, and the step panel over the bottom of the canvas. Before the capture of `03-save` and `04-run` (clip `02-read` does the
first point inside its step `read-3`):

- compact the admin menu, so the designer has the whole width: click `button.leftbar-compactor` (title "Collapse / expand
  menu", `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:79`) once; the compact state is kept from page to page
  (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts:325`), so check it at the start of each take;
- close the step panel (no step selected; press `Escape` in the canvas) unless the clip starts with a step open;
- when a step panel has to show a tall form, drag its top edge (`[data-cy='step-panel-resizer']`) up before the capture
  until the form fits, and scroll the panel with `ScrollAsync` when a step needs the lower part of the form.

## 3. Clip `02-read` (chapter 02, "Read the file")

Starts on `/Admin/DataPipelines`, opened with `OpenAsync` (not recorded): the page title area shows "Data Pipelines", the
**Search** box and the **Add Pipeline** button, and the alert "Nothing here! There are no data pipelines for the moment."
(`DP/Views/Admin/Index.cshtml:20` and `:118`). The admin menu is expanded (not compact) on **Tools > Data Pipelines**, with
**Pipelines** as the current item (`DP/AdminMenu.cs:18-22`), before `StartCaptureAsync`.

- `read-1`: highlight the alert (`.alert-info`) with `HighlightAsync`, hold about 4 seconds, `ClearHighlightsAsync`, then
  `ClickAndWaitForNavigationAsync` on `a.create` (**Add Pipeline**). *On screen at the end*: the Add Pipeline page, with the
  **Name** box focused. Labels: `DP/Views/Admin/Index.cshtml:20`; `DP/AdminMenu.cs:18-20` (Tools, Data Pipelines, Pipelines).
- `read-2`: `TypeAsync` in `input[name='Name']`: `Import products`; `ClickAndWaitForNavigationAsync` on `button.save`
  (**Create**); wait until `[data-cy='data-pipeline-designer']` is visible and `[data-cy='start-picker']` is visible. *On
  screen at the end*: the designer with the toolbar "Data Pipelines / Import products", an empty canvas and the "Start with a
  source" picker. Labels: `DP/Views/Admin/Create.cshtml:11` and `:20`; the toolbar `DPD/App.vue:669-673`. The hint under the
  box ("You design the pipeline next...") is `Create.cshtml:15`.
- `read-3`: first `ClickAsync` on `button.leftbar-compactor` to compact the admin menu (the designer then has the whole width),
  wait for the "Start with a source" picker to be redrawn; then `ClickAsync` on `[data-cy='start-picker-ReadMediaFile']`
  (**Read a media file**); wait until
  `[data-cy='step-panel']` is visible and `[data-cy='panel-step-title']` has the text "Read a media file". *On screen at the
  end*: a **Read a media file** card on the canvas, with the label "Choose a file", and its panel open on **Settings** with
  the **File** box empty. Labels: `DPD/canvas/StartPicker.vue:20`; `DP/Steps/Media/ReadMediaFileStep.cs:32`;
  `DP/Views/ReadMediaFile.Fields.Design.cshtml` ("Choose a file").
- `read-4`: `TypeAsync` in the **File** box (`GetByLabel("File", Exact)`): `imports/products.csv`; press `Tab`; wait until the
  card of the step shows the text `imports/products.csv` (`[data-node-id]` has that text). Then `HighlightAsync` on the
  **Format** select (its selected option reads "From the extension of the file") for about 3 seconds, then on the checkbox
  "The first row holds the names of the columns (CSV, Excel)", which is checked, and `ClearHighlightsAsync`. *On screen at the
  end*: **File** = `imports/products.csv`, **Format** = "From the extension of the file", the checkbox checked. Labels:
  `DP/Views/ReadMediaFile.Fields.Edit.cshtml:4`, `:13`, `:16`, `:29`.
- `read-5`: `ClickAsync` on `[data-cy='step-tab-data']` (**Data**), then on `[data-cy='preview-run']` (**Preview**); wait
  until `[data-cy='preview-tab'] tbody tr` counts 5 rows. *On screen at the end*: the table with the columns **Name**,
  **Price** and **Category**, each with the type **Text**, five rows, and in the last row (Wall shelf) the text `null` in the
  Price column; all five rows must be visible without scrolling the page (resize the step panel before the capture if not).
  Labels: `DPD/panel/StepPanel.vue:97` ("Settings", "Data", "Fields"); `DPD/panel/PreviewTab.vue:56`
  ("Preview"); `DPD/preview/previewTable.ts:24` ("Text") and `:51` ("null").

Leaves the site with the pipeline "Import products": one step, **Read a media file**, in the draft (saved automatically by the
designer a moment after each change; wait for the toolbar to read "Saved" or "Draft saved" before stopping the site).

## 4. Clip `03-save` (chapter 03, "Save content items")

Starts in the designer of "Import products" (see section 2), with the admin menu compact, the **Read a media file** card on
the canvas, no step panel open, and the pipeline panel on **Pipeline**.

- `save-1`: `ClickAsync` on the output dot of the **Read a media file** card
  (`[data-node-id]:has-text('Read a media file') [data-port-side='output']`; the button is named "Connect the output
  Rows"); wait until `[data-cy='quick-add']` is visible (its title reads "Add a step after Rows"); `TypeAsync` in
  `[data-cy='quick-add-search']`: `Save content`; `ClickAsync` on `[data-cy='quick-add-SaveContentItems']`; wait until
  `[data-cy='panel-step-title']` has the text "Save content items" and the canvas has two cards joined by a line. *On screen at
  the end*: **Read a media file** and **Save content items** connected, the second one selected with its panel open and the
  **Content type** list reading "Choose a content type". Labels: `DPD/canvas/QuickAddMenu.vue:95` and `:108` ("Search the
  steps"); `DP/Steps/Contents/SaveContentItemsStep.cs:49`.
- `save-2`: `SelectAsync` on `GetByLabel("Content type", Exact)` with the value `Product`; wait until the label **Rows**
  (`GetByLabel("Rows", Exact)`) is visible (the form reloads after a content type is chosen). `HighlightAsync` on the hint
  under the list ("The user who publishes the pipeline must be allowed to edit, and to publish, items of this type.") for
  about 3 seconds, then `ClearHighlightsAsync`. *On screen at the end*: **Content type** = Product, and under it the labels
  **Rows**, **Match on**, **Values**. Labels: `DP/Views/SaveContentItems.Fields.Edit.cshtml:6`, `:9`, `:15`.
- `save-3`: `HighlightAsync` on the **Rows** list (`select[name$='.Mode']`), whose selected option reads "Update the item a
  row matches, or create one"; nothing is changed. *On screen at the end*: the ring on **Rows** with that option. Labels:
  `SaveContentItems.Fields.Edit.cshtml:22` and `:25`.
- `save-4`: `SelectAsync` on `select[name$='.MatchBy']` with the value `DisplayText` (label "Display text"), then
  `SelectAsync` on `select[name$='.KeyField']` with the value `Name` (label "Name (Text)"). Wait until both show those
  options. *On screen at the end*: **Match on** = "Display text" `=` "Name (Text)". Labels:
  `SaveContentItems.Fields.Edit.cshtml:33`, `:37-38`; `DP/Views/Shared/DataPipelineFieldOptions.cshtml:11` (the option text is
  the field name and its type).
- `save-5`: scroll the panel so that **Values** is visible. Three times: `ClickAsync` on `[data-dp-add]` (**Add a value**),
  then in the new row (`[data-dp-row]`, the nth one) `SelectAsync` on `select[name$='.Target']` and on
  `select[name$='.Source']`:

  | Row | Target (value) | Option text of the target | Source (value) |
  |---|---|---|---|
  | 1 | `TitlePart.Title` | "Title (TitlePart.Title)" | `Name` |
  | 2 | `Product.Price` | "Price (Product.Price)" | `Price` |
  | 3 | `Product.Category` | "Category (Product.Category)" | `Category` |

  The whole sequence is about 10 seconds, shorter than the narration; shorten the `pauseAfter` of the clicks if a take runs
  over. *On screen at the end*: three rows, each "Title (TitlePart.Title) ← Name (Text)", "Price (Product.Price) ← Price
  (Text)", "Category (Product.Category) ← Category (Text)". Labels: `SaveContentItems.Fields.Edit.cshtml:50` and `:61`;
  `DP/Views/Shared/DataPipelineContentMappingRow.cshtml:9`, `:15`, `:21`. The option text of a target is its label and its
  technical name, computed by `ContentDataColumns.Build` (`DP/DataSources/Contents/ContentDataColumns.cs:30`): see OPEN-QUESTIONS.md.
- `save-6`: scroll the panel down so that **Publish the items** (checked) and **Items per save** (100) are visible and
  `HighlightAsync` each for about 3 seconds; `ClearHighlightsAsync`; then `ClickAsync` on `[data-cy='panel-tab-issues']`
  (the **Issues** tab of the pipeline panel); wait until the tab shows a text starting "No issues". *On screen at the end*:
  the **Issues** tab with "No issues: the pipeline can be published." (or "...the draft can be published."), and the step
  panel still showing the checked box. Labels: `SaveContentItems.Fields.Edit.cshtml:70` and `:77`;
  `DPD/panel/PipelinePanel.vue:89` ("Issues"); `DPD/i18n.ts:11` ("No issues: the pipeline can be published.").
- `save-7`: `ClickAsync` on `[data-cy='step-tab-data']` of the step panel, then on `[data-cy='preview-run']`; wait until
  `[data-cy='preview-input-note']` is visible. *On screen at the end*: the note "Destinations are never executed by a preview;
  this is what the step would receive." above a table of the five rows with **Name**, **Price** and **Category**. Labels:
  `DPD/panel/PreviewTab.vue:70`.

Leaves the site with the draft of "Import products": two connected steps, the second one fully set. Wait for the toolbar to
read "Saved" before stopping the site. No content item exists yet (a preview never runs a destination).

## 5. Clip `04-run` (chapter 04, "Publish and run")

Starts in the designer of "Import products" (see section 2), with the admin menu compact, no step panel open, the pipeline
panel on **Pipeline**. The toolbar shows **Publish** enabled and **Run** disabled.

- `run-1`: `ClickAsync` on `[data-cy='toolbar-publish']` (**Publish**); wait until `[data-cy='published-version']` is visible.
  No confirmation dialog is expected (the pipeline has no warning); if "Publish the pipeline?" appears, stop and report it.
  *On screen at the end*: the badge "Version 1" next to the title, **Run** enabled, and the toast "Version 1 was published."
  if it is still showing. Labels: `DPD/App.vue:769-771` (Publish), `:676-681` (badge), `:472` (toast).
- `run-2`: `ClickAsync` on `[data-cy='toolbar-run']` (**Run**); the pipeline panel opens its **History** tab; wait until
  `[data-cy='runs-tab']` shows a badge with the text "Succeeded" (wrap the wait in `OffCameraAsync` if it can last longer
  than the narration; it is expected to last a few seconds). `HighlightAsync` on that badge. *On screen at the end*: the
  **History** tab with one run, status "Succeeded", "Version 1 · ..." under it, and the run's details with its steps.
  Labels: `DPD/App.vue:759-764` (Run), `DPD/panel/PipelinePanel.vue:89` ("History"), `DPD/runs/runOverlay.ts:27`
  ("Succeeded").
- `run-3`: `ClickAndWaitForNavigationAsync` on `[data-cy='run-page']` (**Open the run**); wait until the page title reads "Run of
  Import products" and the badge "Succeeded" is visible; `ScrollAsync` so that the **Delivered** list is on screen and
  `HighlightAsync` on its item. *On screen at the end*: the run page, the heading **Delivered**, and the item "Created 4 and
  updated 0 'Product' content items; skipped 0 and rejected 1 rows." followed by "Save content items"; the **Steps** table
  and, under **Log**, the warning "1 rows couldn't be saved." may be visible too. Labels: `DPD/panel/RunsTab.vue:126` (Open
  the run); `DP/Views/Admin/Run.cshtml:9` (title), `:98` (Delivered), `:119` (Log); the delivery text is
  `DP/Steps/Contents/SaveContentItemsStep.cs:174`, the log line `:171`.

Leaves the site with the pipeline published (version 1), a successful run, and four Product items. Do not delete any of them.

## 6. Clip `05-check` (chapter 05, "Check the result")

Starts on `/Admin/Contents/ContentItems/Product`, opened with `OpenAsync` (not recorded), with the admin menu expanded again
(click `button.leftbar-compactor` if it is compact) on **Content** (`src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65-68`; the entry **Content Items** leads to
the same list without the type filter, so it may not be marked as the current item on this address).

- `check-1`: `HoldAsync(2)`, then `HighlightAsync` on the list of items for about 3 seconds. *On screen at the end*: four
  items, "Reading lamp", "Standing desk", "Task chair" and "Ceramic mug" (not "Wall shelf"), all of the type Product.
- `check-2`: `ClickAndWaitForNavigationAsync` on the link "Task chair" (the title of the row); wait until the editor of the
  item is shown with an input holding `Task chair`. *On screen at the end*: the editor of "Task chair" with its **Title**, its
  **Price** (189.5 or 189.50) and its **Category** (`Furniture`) visible without scrolling; scroll just enough if not. Labels:
  the field names are the display names of the seed in section 1.
- `again-1`: `NavigateAsync("/Admin/DataPipelines")`; wait until the row "Import products" shows the badge "Version 1" and a
  last run badge starting "Succeeded". Then `ClickAsync` on the **Actions** button of the row (`button.actions`), and on the item
  "Run now" of its menu (`a.dropdown-item` with that text); a confirmation dialog titled "Run now" asks "Run the pipeline
  'Import products' now?"; `ClickAsync` on its **Run** button (the button with the class `btn-primary` of the dialog).
  *On screen at the end*: the confirmation has been accepted and the browser is loading the run page. Labels:
  `DP/Views/Admin/Index.cshtml:85-92` (**Actions**, "Run now", the dialog), `DP/Controllers/AdminController.cs` (`RunNow`).
- `again-2`: wait for the run page (`/Admin/DataPipelines/Run/...`); the page refreshes itself every 3 seconds while the run is
  not over (`DP/Views/Admin/Run.cshtml:18`), so wrap the wait for the badge "Succeeded" in `OffCameraAsync`; `ScrollAsync` so
  that **Delivered** is on screen, `HighlightAsync` on its item. *On screen at the end*: the run page, status "Succeeded", and
  the item "Created 0 and updated 4 'Product' content items; skipped 0 and rejected 1 rows." under **Delivered**.

## 7. Slides

Drawn by `slides.py`, no recording.

- `how-1`: three cards, "What an import needs": a file in the media library, a content type, a data pipeline.
- `how-2`: two cards, "The steps of an import": Read a media file, Save content items.
- `how-3`: three cards, "What happens to a row": the three choices of the **Rows** setting of Save content items, written as
  the editor labels them.
- `rej-1`: two cards, "Rows that cannot be saved": checked first, and the Rejected output with the Error field.
- `rej-2`: three cards, "Keep the rejected rows": Rejected, Create a file, Save to the media library.

## 8. Checks for private data

Everything in this video is fake: the user `admin` with `admin@example.com`, the products, and the file. Before assembling:

- look at the contact sheet of each clip (`review.py`): no sign-in form, no password, no error message, no browser
  notification, and the media library and the content items list hold nothing but the seed and the import;
- OCR the video at one frame per second and at every scene change, and search for a real name, a real email address, a host
  name of this machine, a path under `C:\Users`, and the password; none may appear (the run page shows the user `admin` and
  times in UTC, which is fine);
- the designer shows no absolute path: the file is `imports/products.csv`, relative to the media library.

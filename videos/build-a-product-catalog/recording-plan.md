# Recording plan: Build a Product Catalog

This plan lets a person record every clip of the video without deciding anything. A selector written here stands for
`scene.Page.Locator(...)`. File and line references are relative to `src/OrchardCore.Modules` of the Orchard Core source
at commit `92db4a83`, unless the path starts with `src/`. The audience is admin: the plan holds selectors and file
references, but the narration, the slides and the captions do not.

## 1. The site it needs

| What | Value |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore` at commit `92db4a83`, run on `http://localhost:5010` |
| App data | An empty folder outside both repositories, passed as `ORCHARD_APP_DATA` |
| Setup | Auto Setup through environment variables (see below). No setup form and no sign-in form is recorded |
| Recipe | `Blog` (the setup recipe named Blog). It enables Content Types, Taxonomies, Media and Indexing, and it makes the Blog theme the site theme |
| Site name, time zone | `Orchard Core`, `America/Los_Angeles` |
| Database | `Sqlite` |
| User | `admin`, `admin@example.com`, the password chosen outside git and passed as `VIDEO_PASSWORD` (written `<password>` in this plan) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls a mail, SMS or payment provider |
| Browser | The recorder's Chromium (1536x864 CSS pixels, light color scheme, `en-US`), signed in once with `Recorder setup` (state.json stays out of git) |

Auto Setup variables (names from `src/OrchardCore.Modules/OrchardCore.AutoSetup/Options/TenantSetupOptions.cs`):

```
OrchardCore__AutoSetup__AutoSetupPath=
OrchardCore__AutoSetup__Tenants__0__ShellName=Default
OrchardCore__AutoSetup__Tenants__0__SiteName=Orchard Core
OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
```

Features the video itself enables on camera: Media Slugify (clip `04-slugify`), Lucene and Search (clip
`06-features`). Taxonomies, Media, Content Types, Title and Indexing are already enabled by the Blog recipe
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). Do not enable anything else before
recording. If the site was set up with another recipe, enable Taxonomies and Media first, outside the capture.

Settings: none to save before recording.

External services and stand-ins: none. Search uses the Lucene index on the local disk.

## 2. Seed data (created outside the capture)

Only the three product pictures exist before the first take. Everything else is created on camera.

1. Create three JPEG files of 1200x800 pixels, outside the repository (for example with an image tool or a short script),
   each a plain colored background with the product name drawn in large letters. Never use a photo of a real person or a
   real brand. Name them exactly:
   - `Trail Backpack.jpg` (green background)
   - `Two-Person Tent.jpg` (orange background)
   - `Down Sleeping Bag.jpg` (blue background)
2. Keep them in a folder named `product-pictures` outside git. The folder path is read by clip `04-library` and by the
   off-camera part of clip `05-product`.
3. The fake products of the catalog:

| Title | Price | Category | Picture |
|---|---|---|---|
| Trail Backpack | 89 | Backpacks | `Trail Backpack.jpg` |
| Two-Person Tent | 199 | Tents | `Two-Person Tent.jpg` |
| Down Sleeping Bag | 129 | Sleeping Bags | `Down Sleeping Bag.jpg` |

No real names, emails or companies appear anywhere. The only email address on screen can be `admin@example.com`.

## 3. Order of the clips and snapshots

Clips are recorded in the order they play, because each one starts from the state the previous one left. There is no
recorder `PrepareAsync`; the site starts fresh.

| Order | Clip | Changes the site | Needs |
|---|---|---|---|
| 1 | `02-product` | Yes: creates the Product type with two fields | A fresh site |
| 2 | `02-term-type` | Yes: creates the Product Category type | Clip 1 not required, but record it second |
| 3 | `03-taxonomy` | Yes: creates the Product Categories taxonomy and three terms | Product Category type |
| 4 | `03-connect` | Yes: adds the Category field to Product | Product type, taxonomy |
| 5 | `04-slugify` | Yes: enables Media Slugify | Nothing |
| 6 | `04-library` | Yes: creates the product-photos folder and three files | Media Slugify enabled, seed pictures |
| 7 | `05-product` | Yes: creates three products | Product type with three fields, the taxonomy, the pictures |
| 8 | `06-features` | Yes: enables Lucene, Search and Indexing | Nothing |
| 9 | `06-index` | Yes: creates the index `products` | Lucene and Search enabled, products published |
| 10 | `06-settings` | Yes: saves the Site Search settings | The index |
| 11 | `06-permission` | Yes: changes the Anonymous role | The index |
| 12 | `06-find` | No | Everything above |

Snapshots: stop the site and copy its App_Data folder aside at these points. Restore the copy before a new take of the
clip that follows it.

- Snapshot A: fresh site after Auto Setup (before clip 1).
- Snapshot B: after clip 3 (`03-taxonomy`).
- Snapshot C: after clip 6 (`04-library`).
- Snapshot D: after clip 7 (`05-product`).
- Snapshot E: after clip 11 (`06-permission`).

Re-recording one clip means restoring the snapshot just before it and replaying the clips between that snapshot and the
clip, without capturing them.

Slide clips (`01-needs`, `06-how`, `07-recap`) and the cards need no recording.

## 4. Clips

Every clip opens its start page with `scene.OpenAsync(path)` before `scene.StartCaptureAsync()`. In every step, keep the
actions shorter than the narration. A wait names its condition; never wait for a fixed time alone.

A note on the Media Library and the picker: they are a script-driven application (a Vue gallery,
`OrchardCore.Media/Assets/media-gallery/src`). Find their elements with `Recorder probe` before the first take; the
selectors below that are marked "probe" are the ones to confirm (see OPEN-QUESTIONS.md).

### 4.1 Clip `02-product` (chapter 02)

Start page: `/Admin` (the admin home), signed in as `admin`. Menus collapsed except what the narration shows.
The site changes: the Product type is created.

**`product-1`**
- Actions: click the admin menu item `Design`, then `Content Definition`, then `Content Types`
  (`ClickAndWaitForNavigationAsync` on the last one). Then click the button `a#btnCreate` (text `Create new type`).
- Visible at the end: the form "Create" page with the fields `Display Name` and `Technical Name` and the `Create`
  button; the page title area shows `New Content Type`.
- Labels from: `OrchardCore.ContentTypes/AdminMenu.cs:23-25`, `Views/Admin/List.cshtml:20`, `Views/Admin/Create.cshtml:5,14,22`.

**`product-2`**
- Actions: `TypeAsync` into the field `Display Name` (`input[data-displayname]`) the text `Product` (delay 60). The
  `Technical Name` field (`input[data-name]`) fills in `Product` after the key presses (the script runs on `keyup` and
  `blur`: press `Tab` once if it is empty). Click the button `button.create` (text `Create`) with
  `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Add Parts page, with the notice `The "Product" content type has been created.` above the text
  `Choose the Parts to add to this Content Type.` and the list of parts with checkboxes.
- Labels from: `Views/Admin/Create.cshtml:14,22,30`, `Controllers/AdminController.cs:162`, `Views/Admin/AddPartsTo.cshtml:18`.

**`product-3`**
- Actions: click the checkbox of the part labeled `Title` (`GetByLabel("Title", Exact)` inside the parts list; its hint
  reads `Provides a Title for your content item.`). The button `#add-selected-parts-button` (`Add Selected Parts`)
  appears when a part is checked. Click it with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Edit Content Type page for Product, with a notice that the part has been added (the site
  writes the part's name, `TitlePart`, in the notice), and the `Parts` list with `Title` and `Product`.
- Labels from: `Views/Admin/AddPartsTo.cshtml:47`, `Controllers/AdminController.cs:372`, `OrchardCore.Title/Migrations.cs:33`.
  The part is listed under the name `Title` because the display name of a part without a custom one is its name without
  the suffix Part (`OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-63`).

**`product-4`**
- Actions: `HighlightAsync` on the row of the `Parts` list that reads `Product` with the hint `Contains the fields for the
  current type`, hold, `ClearHighlightsAsync`. Then click the link `Add Field` (`a[href*="AddFieldsTo/Product"]`) with
  `ClickAndWaitForNavigationAsync`.
- Visible at the end: the page `Add New Field To "Product"` with the fields `Display Name`, `Technical Name` and the
  `Field Type` list.
- Labels from: `Views/Admin/Edit.cshtml:144,180,137`, `Views/Admin/AddFieldTo.cshtml:20,38,54`.

**`product-5`**
- Actions: `TypeAsync` into `Display Name` (`input[data-displayname]`) the text `Price`. Click the radio button labeled
  `Numeric Field` (`GetByLabel("Numeric Field")`). Click `button[value="SaveAndConfigure"]` (`Save & Configure`) with
  `ClickAndWaitForNavigationAsync`.
- Visible at the end: the settings page titled `"Price" settings`, with the numeric settings (Required, Hint, Scale...).
- Labels from: `Views/Admin/AddFieldTo.cshtml:38,54,62,72` (the type list shows the field type names with spaces,
  `CamelFriendly()`), `Views/Admin/EditField.cshtml:30`.

**`product-6`**
- Actions: click the checkbox `Required`; select the input `Scale` (`GetByLabel("Scale")`), clear it, `TypeAsync` the text
  `2`, press `Tab`. Click `button.save` (`Save`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Edit Content Type page for Product, with the notice `The "Price" field settings have been
  saved.` and the field `Price` (badge-free row) in the `Fields` list.
- Labels from: `OrchardCore.ContentFields/Views/NumericFieldSettings.Edit.cshtml:15,38,41` (hint: `The number of digits
  after the decimal point.`), `Views/Admin/EditField.cshtml:120`, `Controllers/AdminController.cs:908`.

**`product-7`**
- Actions: click `Add Field` again. `TypeAsync` into `Display Name` the text `Image`. Click the radio button `Media Field`.
  Click `button[value="Save"]` (`Save`, the one next to `Save & Configure`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Edit Content Type page for Product, with the notice `The field "Image" has been added.` and
  the fields `Price` and `Image` in the `Fields` list.
- Labels from: `Views/Admin/AddFieldTo.cshtml:71`, `Controllers/AdminController.cs:768`. A Media Field accepts several
  pictures by default (`OrchardCore.Media.Core/Settings/MediaFieldSettings.cs:9`, in `src/OrchardCore`).

**`product-8`**
- Actions: `HighlightAsync` the `Fields` list, then the `Parts` list, `ClearHighlightsAsync`. Click the green button
  `Save` (`button.save[name="submit.Save"][value="Save"]`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Content Types list, with the notice `Content type updated successfully.` and the row
  `Product` (technical name badge `Product`).
- Labels from: `Views/Admin/Edit.cshtml:88,144,215`, `Controllers/AdminController.cs:225`.

### 4.2 Clip `02-term-type` (chapter 02)

Start page: `/Admin/ContentTypes/List`. The site changes: the Product Category type is created.

**`term-1`**
- Actions: click `a#btnCreate` (`Create new type`) with `ClickAndWaitForNavigationAsync`; `TypeAsync` into `Display Name`
  the text `Product Category`.
- Visible at the end: the create form with `Product Category` in `Display Name` and `ProductCategory` in `Technical Name`.
- Labels from: `Views/Admin/List.cshtml:20`, `Views/Admin/Create.cshtml:14,22`.

**`term-2`**
- Actions: click `button.create` (`Create`); on the Add Parts page click the checkbox `Title`, click
  `#add-selected-parts-button` (`Add Selected Parts`); on the Edit page click `button.save` (`Save`). Use
  `ClickAndWaitForNavigationAsync` for each of the three clicks that load a page. The step has four clicks; give each
  `pauseAfter: 300` so the actions end before the narration does.
- Visible at the end: the Content Types list with the notice `Content type updated successfully.` and the rows `Product`
  and `Product Category`.
- Labels from: `Views/Admin/Create.cshtml:30`, `Views/Admin/AddPartsTo.cshtml:47`, `Views/Admin/Edit.cshtml:215`.

### 4.3 Clip `03-taxonomy` (chapter 03)

Start page: `/Admin/Contents/ContentItems` (title `Manage Content`). The site changes: the taxonomy and its terms.

**`tax-1`**
- Actions: click the admin menu item `Content`, then `Content Items` (`ClickAndWaitForNavigationAsync`). Click the
  dropdown button `#new-dropdown` (`New`), then the item `Taxonomy` (`a.dropdown-item`, exact text `Taxonomy`) with
  `ClickAndWaitForNavigationAsync`.
- Visible at the end: the page `New Taxonomy` with the field `Title` and the select `Term Content Type` showing
  `Select a term content type`.
- Labels from: `OrchardCore.Contents/AdminMenu.cs:65,68`, `Views/ContentsAdminListCreate.cshtml:20,25`,
  `OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml:26,29`. The type `Taxonomy` is creatable and listable
  (`OrchardCore.Taxonomies/Migrations.cs:23-27`), and it has no display name, so its display name is `Taxonomy`.

**`tax-2`**
- Actions: `TypeAsync` into `Title` (`GetByLabel("Title", Exact)`) the text `Product Categories`. In the select
  `Term Content Type` choose the option `Product Category` (`SelectAsync`). Click `button.publish` (`Publish`) with
  `ClickAndWaitForNavigationAsync`.
- Visible at the end: the content list `Manage Content` with the notice `Your Taxonomy has been published.` and the row
  `Product Categories`.
- Labels from: `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`, `TaxonomyPart.Edit.cshtml:22-28` (the select lists the
  content types without a stereotype), `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`,
  `OrchardCore.Contents/Controllers/AdminController.cs:473`.

**`tax-3`**
- Actions: click the `Edit` button of the row `Product Categories` (`ContentsButtonEdit_SummaryAdmin`, the button
  `a.edit` in that row) with `ClickAndWaitForNavigationAsync`. `HighlightAsync` the button `Add Product Category`.
- Visible at the end: the page `Edit Taxonomy` with the green-ringed button `Add Product Category` and an empty list of
  terms.
- Labels from: `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`, `TaxonomyPart.Edit.cshtml:21,48`.
  The button is shown only when the taxonomy has been saved (`canAddTerm`, line 21).

**`tax-4`**
- Actions: `ClearHighlightsAsync`. Click `Add Product Category` (`ClickAndWaitForNavigationAsync`). `TypeAsync` into
  `Title` the text `Backpacks`. Click `button.publish` (`Publish`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Edit Taxonomy page again, with the term `Backpacks` in the list under the button, each term row
  showing the buttons `Edit`, `Add` and `Delete`.
- Labels from: `TaxonomyPart.Edit.cshtml:48`, `OrchardCore.Taxonomies/Views/Content.TermAdmin.cshtml:27,36,45`,
  `OrchardCore.Taxonomies/Controllers/AdminController.cs:135,165` (the term is added to a draft of the taxonomy and the
  page returns to the taxonomy editor).

**`tax-5`**
- Actions: add the terms `Tents` and `Sleeping Bags` the same way (click `Add Product Category`, type the title, click
  `Publish`). Keep each cycle short (`pauseAfter: 300`). Then `HighlightAsync` the `Add` button of the term `Tents`
  (the one inside the row of `Tents`), hold, `ClearHighlightsAsync`. Do not click it.
- Visible at the end: the taxonomy editor listing `Backpacks`, `Tents` and `Sleeping Bags`.
- Labels from: `Content.TermAdmin.cshtml:36` (the `Add` button passes the term as parent, `taxonomyItemId`, line 34).

**`tax-6`**
- Actions: click `button.publish` (`Publish`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the page `Edit Taxonomy` again, with the notice `Your Taxonomy has been published.` above the three
  terms `Backpacks`, `Tents` and `Sleeping Bags`. The page was reached from the term editor without a return address, so
  publishing does not go back to the content list: the editor reloads itself.
- Labels from: `Content.PublishButton.cshtml:7`; the terms are added to a draft: `OrchardCore.Taxonomies/Controllers/AdminController.cs:110`;
  the reload after publishing without a return address: `OrchardCore.Contents/Controllers/AdminController.cs:841-847`
  (`EditInternalAsync`); the notice: `AdminController.cs:964-967`. Publishing matters later: the product editor lists the
  terms of the latest version (`OrchardCore.Taxonomies/Drivers/TaxonomyFieldDisplayDriver.cs:44`), but the category name on
  the product page is read from the published taxonomy (`Helper/TaxonomyOrchardHelperExtensions.cs:19-27`).

### 4.4 Clip `03-connect` (chapter 03)

Start page: `/Admin/ContentTypes/List`. The site changes: the Category field is added.

**`conn-1`**
- Actions: click the admin menu items `Design`, `Content Definition`, `Content Types` (`ClickAndWaitForNavigationAsync`).
  Click the `Edit` button of the row `Product` (`a[role="btn-edit-Product"]`), then the link `Add Field`
  (`a[href*="AddFieldsTo/Product"]`), both with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the page `Add New Field To "Product"`.
- Labels from: `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24` (the Edit button of a row of
  the list), `Views/Admin/Edit.cshtml:137`.

**`conn-2`**
- Actions: `TypeAsync` into `Display Name` the text `Category`; click the radio button `Taxonomy Field`; click
  `button[value="SaveAndConfigure"]` (`Save & Configure`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the settings page `"Category" settings` with the select `Taxonomy` showing `Select a Taxonomy`.
- Labels from: `Views/Admin/AddFieldTo.cshtml:62,72`, `OrchardCore.Taxonomies/Views/TaxonomyFieldSettings.Edit.cshtml:30,33`.

**`conn-3`**
- Actions: select the option `Product Categories` in the select `Taxonomy` (`SelectAsync`); click the checkbox `Unique`;
  click `button.save` (`Save`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Edit Content Type page for Product with the notice `The "Category" field settings have been
  saved.`
- Labels from: `TaxonomyFieldSettings.Edit.cshtml:56-57` (hint `Whether only one element can be selected.`),
  `Views/Admin/EditField.cshtml:120`, `Controllers/AdminController.cs:908`.

**`conn-4`**
- Actions: `HighlightAsync` the `Fields` list (three rows: `Price`, `Image`, `Category`), hold, `ClearHighlightsAsync`.
  Nothing to save: fields are stored as soon as they are added.
- Visible at the end: the `Fields` list with `Price`, `Image` and `Category`.
- Labels from: `Views/Admin/Edit.cshtml:88`.

### 4.5 Clip `04-slugify` (chapter 04)

Start page: `/Admin`. The site changes: Media Slugify is enabled.

**`slug-1`**
- Actions: click the admin menu items `Tools`, `Features` (`ClickAndWaitForNavigationAsync`). `TypeAsync` into the search
  box `#search-box` (placeholder `Search`) the text `Media Slugify`. Wait until the row named `Media Slugify` is the only
  visible feature row. Click its `Enable` button (`a[href*="/Features/OrchardCore.Media.Slugify/Enable"]`) with
  `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Features page with the notice `The feature Media Slugify was enabled.`
- Labels from: `OrchardCore.Features/AdminMenu.cs:26,27`, `Views/Admin/Features.cshtml:27`,
  `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`, `Controllers/AdminController.cs:317`,
  `OrchardCore.Media/Manifest.cs:56-57`. Existing files and folders keep their names: the normalizer runs only when a
  file is uploaded (`OrchardCore.Media/Endpoints/Api/UploadMediaEndpoint.cs:90`) or a folder is created
  (`CreateFolderEndpoint.cs:51`).

### 4.6 Clip `04-library` (chapter 04)

Start page: `/Admin/Media`. Needs the seed pictures and Media Slugify enabled. The site changes: one folder, three files.

**`lib-1`**
- Actions: click the admin menu items `Media`, `Library` (`ClickAndWaitForNavigationAsync`); wait until the folder tree
  shows the root folder (probe: the first row of `#folder-tree`, labeled `Files`). Click the folder menu button of the
  root row (probe: `#folder-tree a[title="Folder Action"]`, an ellipsis icon), then the menu item `Create a subfolder`.
- Visible at the end: the dialog titled `Create folder` with the path `/` and the input with the placeholder `Enter a folder
  name`.
- Labels from: `OrchardCore.Media/AdminMenu.cs:18,21`, `Services/MediaJSLocalizer.cs:31,64,65`,
  `Assets/media-gallery/src/components/FolderMenu.vue:68`, `ModalFolderAction.vue:14`.

**`lib-2`**
- Actions: `TypeAsync` into `input[name="create-folder"]` the text `Product Photos`. Click `#btn-submit` (`Ok`). Wait until
  the tree shows a folder named `product-photos`.
- Visible at the end: the folder tree with the folder `product-photos` under the root.
- Labels from: `ModalFolderAction.vue:14,34` and `FolderActionModalService.ts:44` (button text `Ok`, `MediaJSLocalizer.cs:69`),
  `OrchardCore.Media/Endpoints/Api/CreateFolderEndpoint.cs:51` (the folder name is slugified).

**`lib-3`**
- Actions: click the folder `product-photos` in the tree. `HighlightAsync` the button `Upload files`
  (`label.upload-button`, text `Upload files`). The system file dialog cannot be shown by the capture: call
  `scene.Page.Locator("#fileupload").SetInputFilesAsync(...)` with the three seed files, in the order `Trail Backpack.jpg`,
  `Two-Person Tent.jpg`, `Down Sleeping Bag.jpg`. Wait until the list shows three files (`trail-backpack.jpg`,
  `two-person-tent.jpg`, `down-sleeping-bag.jpg`) and no upload toast remains. `ClearHighlightsAsync`.
- Visible at the end: the folder `product-photos` selected, with the three files listed by their slugified names.
- Labels from: `Assets/media-gallery/src/App.vue:94`, `MediaJSLocalizer.cs:27`, `UploadMediaEndpoint.cs:90`.
  The input `#fileupload` is hidden inside the label, which is why the files are set on it directly.

### 4.7 Clip `05-product` (chapter 05)

Start page: `/Admin/Contents/ContentItems`. Needs the Product type with its three fields, the taxonomy and the pictures.
The site changes: three products are published.

**`add-1`**
- Actions: click the admin menu items `Content`, `Content Items` (`ClickAndWaitForNavigationAsync`). Click `#new-dropdown`
  (`New`), then the item `Product` (`a.dropdown-item`, exact text `Product`; not `Product Category`) with
  `ClickAndWaitForNavigationAsync`.
- Visible at the end: the page `New Product` with the editor fields `Title`, `Price`, `Image` and `Category`
  (`Category` lists `Backpacks`, `Tents`, `Sleeping Bags`).
- Labels from: `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20,25`,
  `OrchardCore.Contents/Views/Admin/Create.cshtml:26`, `TitlePart.Edit.cshtml:12`. The field labels are the display names
  given in clips `02-product` and `03-connect`.

**`add-2`**
- Actions: `TypeAsync` into `Title` the text `Trail Backpack`; `TypeAsync` into `Price` the text `89`, press `Tab`.
- Visible at the end: `Trail Backpack` in `Title` and `89` in `Price`.
- Labels from: `TitlePart.Edit.cshtml:12`, the field display name `Price`.

**`add-3`**
- Actions: click the button `Add media` in the field `Image` (probe: `button` with text `Add media` inside the media field
  application of that field). Wait for the window titled `Select Media`. In the window click the folder `product-photos`
  in its tree, then click the file `trail-backpack.jpg` (probe: a row or card of the list, selected state shows a count
  such as `1 selected`), then click the button `OK`. Wait until the field shows the picture.
- Visible at the end: the field `Image` shows the thumbnail of `trail-backpack.jpg`.
- Labels from: `OrchardCore.Media/Services/MediaJSLocalizer.cs:119,137,126`,
  `Assets/media-picker/src/components/MediaPickerModal.vue:18,49` (the `OK` button is disabled until a file is selected).

**`add-4`**
- Actions: click the radio button `Backpacks` in the field `Category` (the field is unique, so it is a radio button).
  Scroll the page down if needed, so the buttons at the bottom are visible.
- Visible at the end: the editor with `Backpacks` selected in `Category` and the buttons `Publish` and `Save Draft` at the
  bottom.
- Labels from: `OrchardCore.Taxonomies/Views/TaxonomyField.Edit.cshtml:67-91` (radio buttons when the field is Unique),
  `OrchardCore.Contents/Views/Content.SaveDraftButton.cshtml:7,16,19`. A new type is draftable
  (`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:86`). Only published versions are indexed unless
  `Index Latest version` is checked (`OrchardCore.Indexing/Views/ContentIndexMetadata.Edit.cshtml:31`, default off:
  `src/OrchardCore/OrchardCore.Indexing.Core/Models/ContentIndexMetadata.cs:5`).

**`add-5`**
- Actions: `HighlightAsync` the button `Save Draft` (`button.draft`), hold until the narration says that search does not
  find a draft, `ClearHighlightsAsync`; do not click it. Then click `button.publish` (`Publish`, the main button of the
  split button group) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the content list `Manage Content` with the notice `Your Product has been published.` and the row
  `Trail Backpack`.
- Labels from: `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7,16,19` (the page was opened from the list with a
  return address, so the button is part of a split button whose main action is `Publish`),
  `OrchardCore.Contents/Controllers/AdminController.cs:473`.

**`add-6`**
- Actions: wrap the following in `scene.OffCameraAsync(...)`: in a second page of the same browser context (not the
  capture page), create and publish the products `Two-Person Tent` (Price `199`, picture `two-person-tent.jpg`, Category
  `Tents`) and `Down Sleeping Bag` (Price `129`, picture `down-sleeping-bag.jpg`, Category `Sleeping Bags`) with the same
  editor and the same clicks as in `add-1` to `add-5`. The wait condition is that the list at
  `/Admin/Contents/ContentItems/Product` shows three rows. Then, on the capture page,
  `NavigateAsync("/Admin/Contents/ContentItems/Product")`.
- Visible at the end: the list titled `Product` with the three rows `Trail Backpack`, `Two-Person Tent`,
  `Down Sleeping Bag`.
- Labels from: `OrchardCore.Contents/Controllers/AdminController.cs:67` (route `Contents/ContentItems/{contentTypeId?}`) and
  `OrchardCore.Contents/Views/Admin/List.cshtml:17-24` (the title becomes the display name of the type).

### 4.8 Clip `06-features` (chapter 06)

Start page: `/Admin/Features`. The site changes: Lucene, Search and Indexing are enabled.

**`sf-1`**
- Actions: `TypeAsync` into `#search-box` the text `Lucene`. Wait until the visible rows include the row named `Lucene`.
  Click its `Enable` button (`a[href*="/Features/OrchardCore.Lucene/Enable"]`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Features page with a notice that the feature `Lucene` was enabled (the notice lists every
  feature the click enabled; Indexing is already enabled by the recipe).
- Labels from: `OrchardCore.Lucene/Manifest.cs:10-20`, `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`,
  `OrchardCore.Features/Controllers/AdminController.cs:317`. Do not click `Lucene (Obsolete)`, `Lucene Worker` or
  `Lucene Content Picker`.

**`sf-2`**
- Actions: clear `#search-box` and `TypeAsync` the text `Search`. Wait until the row named `Search` (category `Search`)
  is visible. Click its `Enable` button (`a[href*="/Features/OrchardCore.Search/Enable"]`, not
  `OrchardCore.Search.Lucene`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Features page with the notice `The feature Search was enabled.`
- Labels from: `OrchardCore.Search/Manifest.cs:10-19` (dependency on `OrchardCore.Indexing`, whose description is
  `Provides index management.`: `OrchardCore.Indexing/Manifest.cs:11-16`; Lucene depends on it too:
  `OrchardCore.Lucene/Manifest.cs:10-20`).

### 4.9 Clip `06-index` (chapter 06)

Start page: `/Admin`. The three products are published before this clip. The site changes: the index `products`.

**`idx-1`**
- Actions: click the admin menu items `Search`, `Indexes` (`ClickAndWaitForNavigationAsync`). Click the button `Add index`
  (`button.create[data-bs-target="#modalAddIndex"]`). In the window titled `Available Sources`, `HighlightAsync` the
  card `Content in Lucene`, then click its `Add` button (`#modalAddIndex a.btn-primary`) with
  `ClickAndWaitForNavigationAsync`.
- Visible at the end: the page `New 'Content in Lucene' index` with the fields `Index Name` and `Name`.
- Labels from: `OrchardCore.Indexing/AdminMenu.cs:19,22`, `Views/Admin/Index.cshtml:31,61`,
  `OrchardCore.Lucene/Startup.cs:98`, `OrchardCore.Indexing/Views/IndexProfile.Link.cshtml:29`,
  `Views/Admin/Create.cshtml:6`.

**`idx-2`**
- Actions: `TypeAsync` into `Index Name` the text `products` (the field `Name` fills in `products` by itself while you
  type; its hint is `A unique name for the index.`). Under `Content types` click the checkbox labeled `Product`
  (`GetByLabel("Product", Exact)`; not `Product Category`). `HighlightAsync` the checkbox `Index Latest version`, which
  stays unchecked, hold, `ClearHighlightsAsync`. Click `button[type="submit"].btn-primary` (`Save`) with
  `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Indexes list with the notice `An index has been created successfully. The synchronizing process
  was triggered in the background.`
- Labels from: `OrchardCore.Indexing/Views/IndexProfileFields.Edit.cshtml:6,14,18`,
  `Views/ContentIndexMetadata.Edit.cshtml:17,31-32`, `Views/Admin/Create.cshtml:16`,
  `Controllers/AdminController.cs:257`, `Assets/js/technical-name-generator.js` (the field `Name` follows the field
  `Index Name`, lower-cased and hyphenated). The page of a Lucene index is long: below `Content types` and
  `Index Latest version` it also shows `Analyzer`, `Store Source Data` and a `Default Query Settings` block with `Query
  field names` (`OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs:41-85`,
  `Views/LuceneIndexProfile.Edit.cshtml`, `Views/LuceneQuerySettings.Edit.cshtml:37-39`). Leave all of them at their
  default values; the narration does not mention them, and `Save` is reached by scrolling.

**`idx-3`**
- Actions: `HighlightAsync` the row `products` in the list, hold, `ClearHighlightsAsync`. After the clip, and before
  `06-settings`, wait outside the capture until `/search/products?terms=backpack` lists the Trail Backpack, so the
  background build is complete. Make the request from the signed-in recorder context, not from a private window: until
  `06-permission` only a user who holds the index permission may query it, and a visitor is sent to the sign-in page
  (`OrchardCore.Search/Controllers/SearchController.cs:110-113`). No Indexing Worker feature is needed for the build
  (see OPEN-QUESTIONS.md, point 9).
- Visible at the end: the Indexes list with the row `products`.
- Labels from: `OrchardCore.Indexing/Views/Admin/Index.cshtml:44-46`, `OrchardCore.Indexing/Views/IndexProfile.Fields.SummaryAdmin.cshtml:7`.
  Later publishes are indexed at the end of their request (`src/OrchardCore/OrchardCore.Indexing.Core/Handlers/IndexingContentHandler.cs:86-99`).

### 4.10 Clip `06-settings` (chapter 06)

Start page: `/Admin`. The site changes: the Site Search settings.

**`set-1`**
- Actions: click the admin menu items `Settings`, `Search`, `Site Search` (`ClickAndWaitForNavigationAsync`; the page is
  `/Admin/Settings/search`).
- Visible at the end: the settings page with the select `Default search index` and the input `Placeholder`.
- Labels from: `OrchardCore.Search/AdminMenu.cs:25-27`, `Views/SearchSettings.Edit.cshtml:6,17`,
  `src/OrchardCore/OrchardCore.Search.Abstractions/SearchConstants.cs:5`.

**`set-2`**
- Actions: select the option `products` in `Default search index` (`SelectAsync`; the empty option reads `Select a default
  index`). `TypeAsync` into `Placeholder` the text `Search products`. Click the green `Save` button
  (`button.save`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the settings page with the notice `Site settings updated successfully.` and the two values kept.
- Labels from: `SearchSettings.Edit.cshtml:6,9,17`, `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`,
  `OrchardCore.Settings/Controllers/AdminController.cs:119`.

### 4.11 Clip `06-permission` (chapter 06)

Start page: `/Admin/Settings/search`. The site changes: the Anonymous role.

**`perm-1`**
- Actions: click the admin menu items `Access Control`, `Roles` (`ClickAndWaitForNavigationAsync`). Click `Edit` in the row
  `Anonymous` (`a.edit[href$="/Roles/Edit/Anonymous"]`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the page `Edit 'Anonymous' Role` with the box `Search permissions` and the table of permissions.
- Labels from: `OrchardCore.Roles/AdminMenu.cs:18,19`, `Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`,
  `Views/Admin/Edit.cshtml:6`, `Views/Admin/_RolePermissions.cshtml:49`.

**`perm-2`**
- Actions: `TypeAsync` into `#search-box` (placeholder `Search permissions`) the text `products`. Wait until the table
  shows the rows that contain `products`. Click the checkbox in the `Allow` column of the row `Query 'products' Index`
  (`input[name="Checkbox.QueryIndex_products"]`); not the row `Query Lucene products Index`. Click the `Save` button
  (`button.save[name="submit"][value="Save"]`) with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the Roles list with the notice `Role updated successfully.`
- Labels from: `src/OrchardCore/OrchardCore.Indexing.Core/IndexingPermissions.cs:15` (the permission description
  `Query '{0}' Index` receives the index name `products`), `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49,68-70`,
  `OrchardCore.Roles/Controllers/AdminController.cs:213`.

### 4.12 Clip `06-find` (chapter 06)

Start page: `/` (the home page of the site). The capture signs in as `admin` (the stored session), which already has the
permission; the permission of the Anonymous role is checked once outside the capture in a private window.

**`find-1`**
- Actions: `NavigateAsync("/search")`. `TypeAsync` into the input `input[name="Terms"]` (placeholder `Search products`)
  the text `backpack`. Click the button `Search` (`button[type="submit"]` of the search form) with
  `WaitForNavigationAsync`.
- Visible at the end: the page titled `Search`, with the box showing `backpack` and the result list.
- Labels from: `OrchardCore.Search/Controllers/SearchController.cs:64` (route `search/{index?}`),
  `OrchardCore.Themes/TheBlogTheme/Views/Search-Form.liquid:9,17`, `Search-List.liquid:9`.

**`find-2`**
- Actions: `HighlightAsync` the result `Trail Backpack` (title, price, picture, category), hold, `ClearHighlightsAsync`.
  Click the title link `Trail Backpack` with `ClickAndWaitForNavigationAsync`.
- Visible at the end: the product page, with the heading `Trail Backpack`.
- Labels from: `OrchardCore.Title/Views/TitlePart.Summary.cshtml:3` (the title is a link), the results use the Summary
  display (`OrchardCore.Themes/TheBlogTheme/Views/Search-Results.liquid:17`).

**`find-3`**
- Actions: `HighlightAsync` the price, the picture and the category on the product page, one after the other (about
  1.5 seconds each), `ClearHighlightsAsync`.
- Visible at the end: the product page showing the price `89`, the picture and the category `Backpacks`.
- Labels from: `OrchardCore.ContentFields/Views/NumericField.cshtml`, `OrchardCore.Media/Views/MediaField.cshtml`,
  `OrchardCore.Taxonomies/Views/TaxonomyField.cshtml`.

**`find-4`**
- Actions: `NavigateAsync("/search")`, `TypeAsync` into `input[name="Terms"]` the text `kayak`, click `Search` with
  `WaitForNavigationAsync`.
- Visible at the end: the search page with a yellow message `There are no such results.`
- Labels from: `OrchardCore.Themes/TheBlogTheme/Views/Search-Results.liquid:23`.

## 5. Slides

Rendered by `slides.py`; nothing to record.

- `needs-1`, `needs-2`, `needs-3`: four cards, "A content type", "A taxonomy", "The Media Library", "Search". The card of
  each narrated idea is bright.
- `how-1`: three cards that say what an index does.
- `how-2`: three cards, the three steps of a working search.
- `how-3`: three cards, what happens when a visitor searches.
- `recap-1`: four cards with check marks, one per feature.

## 6. Checks for private data

- The only account on screen is `admin`; the only email is `admin@example.com`. The sign-in password is never typed on
  camera and never appears on screen.
- The Blog recipe adds the Blog content (a blog post and an About page) written by `admin`. The recording never opens it.
- Product names, categories and pictures are the fake ones of section 2. The pictures are plain colored images with
  text; none is a photo.
- OCR the finished video at one frame per second and at every scene change, as the skill asks, and look for names, email
  addresses, phone numbers, file paths of the recording machine (the file dialog is never opened) and tokens.

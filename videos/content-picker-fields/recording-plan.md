# Recording plan: Content Picker and User Picker Fields

This plan is for the person who records the video. It holds every decision: the site, the seed data, the order of the
clips, and for each step what to do and what must be on screen when its narration ends. Where the plan says "selector",
it stands for `scene.Page.Locator(...)`. The labels come from the Orchard Core source at commit 92db4a83 of
`OrchardCMS/OrchardCore`; each one is followed by its file and line, relative to the root of that repository.

Open [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) first: it lists what could not be checked without a running site.

## The site it needs

| Setting | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore` at `main` (the plan was written from commit 92db4a83) |
| Auto Setup | Recipe **Blog** (theme TheBlogTheme, admin theme TheAdmin), tenant Default, site name **Orchard Core**, time zone `America/Los_Angeles`, database SQLite |
| Super user | user name `admin`, email `admin@example.com`, password `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git) |
| Address | `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `ORCHARD_APP_DATA` folder outside both repositories |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls no mail, SMS or other service |
| Sign-in | `Recorder setup` once, outside the capture, saves `state.json` (never committed) |

This video is not about the Setup screen, so it is recorded with Auto Setup and a saved sign-in. No external service is
needed: the Lucene editor of chapter 05 reads a local index, and the Elasticsearch editor is only mentioned on a slide,
so no Elasticsearch server is started. See the `orchardcore-tester` skill of the Orchard Core repository
(`references/autosetup.md`) for the Auto Setup environment variables.

The Blog recipe already enables the features the first four chapters need: Content Fields (`OrchardCore.ContentFields`),
Users, Roles, Title, Autoroute, Content Types, Contents, Deployment (for JSON Import) and Indexing. It also creates the
roles Moderator, Editor, Author and Contributor.
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` and `Roles` steps)

### Features to enable, and when

| Feature (name on the Features page) | Id | When |
|---|---|---|
| Lucene | `OrchardCore.Lucene` | After clip `04-user-pick`, by "Seed 3" below, before `05-lucene-field` |
| Lucene Content Picker | `OrchardCore.Search.Lucene.ContentPicker` | Same moment |

Keep both off while recording chapters 02 to 04, so that no editor drop-down in those clips offers Lucene.
Enabling them by hand: Tools, Features (`OrchardCore.Features/AdminMenu.cs:26,27`), search for the name, enable. "Seed 3"
does it with a recipe instead. The Lucene Content Picker feature depends on the obsolete id `OrchardCore.Search.Lucene`
(`OrchardCore.Lucene/Manifest.cs`, the `OrchardCore.Search.Lucene.ContentPicker` entry), which is enabled with it.

## Seed data (create it outside the capture)

All names are invented. Create the data in this order.

### Seed 1: content types and content items (JSON Import)

Open Tools, Deployments, JSON Import (`OrchardCore.Deployment/AdminMenu.cs:30`), paste the recipe into the box labeled
JSON, and select Import (`OrchardCore.Deployment/Views/Import/Json.cshtml:15,21`). It creates two content types, each
with a Title, an Autoroute and a part named after the type (the part that "Add Field" adds the fields to), five products and one empty bundle. The structure
copies the Blog recipe's `BlogPost` definition and content items.

```json
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Product",
          "DisplayName": "Product",
          "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            { "PartName": "AutoroutePart", "Name": "AutoroutePart", "Settings": { "ContentTypePartSettings": { "Position": "1" }, "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "products/{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": false } } },
            { "PartName": "Product", "Name": "Product", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
          ]
        },
        {
          "Name": "ProductBundle",
          "DisplayName": "Product Bundle",
          "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            { "PartName": "AutoroutePart", "Name": "AutoroutePart", "Settings": { "ContentTypePartSettings": { "Position": "1" }, "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "bundles/{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": false } } },
            { "PartName": "ProductBundle", "Name": "ProductBundle", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
          ]
        }
      ],
      "ContentParts": [
        { "Name": "Product", "Settings": { "ContentPartSettings": { "DisplayName": "Product" } }, "ContentPartFieldDefinitionRecords": [] },
        { "Name": "ProductBundle", "Settings": { "ContentPartSettings": { "DisplayName": "Product Bundle" } }, "ContentPartFieldDefinitionRecords": [] }
      ]
    },
    {
      "name": "content",
      "data": [
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Camp Stove", "Latest": true, "Published": true, "TitlePart": { "Title": "Camp Stove" }, "AutoroutePart": { "Path": "products/camp-stove" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Headlamp", "Latest": true, "Published": true, "TitlePart": { "Title": "Headlamp" }, "AutoroutePart": { "Path": "products/headlamp" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Insulated Bottle", "Latest": true, "Published": true, "TitlePart": { "Title": "Insulated Bottle" }, "AutoroutePart": { "Path": "products/insulated-bottle" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Rain Jacket", "Latest": true, "Published": true, "TitlePart": { "Title": "Rain Jacket" }, "AutoroutePart": { "Path": "products/rain-jacket" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "Product", "DisplayText": "Trail Backpack", "Latest": true, "Published": true, "TitlePart": { "Title": "Trail Backpack" }, "AutoroutePart": { "Path": "products/trail-backpack" } },
        { "ContentItemId": "[js:uuid()]", "ContentType": "ProductBundle", "DisplayText": "Weekend Camping Bundle", "Latest": true, "Published": true, "TitlePart": { "Title": "Weekend Camping Bundle" }, "AutoroutePart": { "Path": "bundles/weekend-camping-bundle" } }
      ]
    }
  ]
}
```

The display names of the two parts (`ContentPartSettings`) make the pages of clip `02-add-picker` read "Product Bundle"
instead of the part's name, ProductBundle (`ContentPartSettingsExtensions.cs:54-62`).

Check: the list of Product items (`/Admin/Contents/ContentItems/Product`) holds five items and the list of Product Bundle
items (`/Admin/Contents/ContentItems/ProductBundle`) holds one. Content, Content Items holds more than these six, because
the Blog recipe adds its own items (a menu, two taxonomies, a blog, a blog post, an article and a footer:
`blog.recipe.json`, the `content` step). `/bundles/weekend-camping-bundle` shows the title and no Products field;
Design, Content Definition, Content Types lists Product and Product Bundle.
The recipe has not been run (see OPEN-QUESTIONS.md, item 6): if the import reports an error, fix the recipe or create
the same two types and six items by hand with the same names, parts and paths.

### Seed 2: users

Access Control, Users (`OrchardCore.Users/AdminMenu.cs:27,30`), then Add User (`Views/UsersAdminListCreate.cshtml:7`)
for each row. Fill User name, Email, and Password with the Generate password button (the password is never used again:
none of these users signs in), tick the roles, and select Save (`Views/UserSaveButtons.Edit.cshtml:2`). Labels:
`Views/UserName.Edit.cshtml:4`, `Views/UserEmail.Edit.cshtml:4`, `Views/UserFields.Edit.cshtml`, `Views/UserRoleFields.Edit.cshtml:13`.

| User name | Email | Roles to tick |
|---|---|---|
| morgan | `morgan@example.com` | Editor |
| jordan | `jordan@example.com` | Editor |
| taylor | `taylor@example.com` | Author |

The user `admin` keeps the role Administrator only. Typing `or` in the User Picker must then list jordan and morgan,
and not taylor (an Author) or admin.

### Seed 3: the Lucene recipe (after clip `04-user-pick`)

Open Tools, Deployments, JSON Import again and import this recipe. It enables the two features of the table above and
creates the index Catalog, of the source "Content in Lucene" (`OrchardCore.Lucene/Startup.cs:98`), holding the Product
items only. Its structure copies `blog.lucene.search.recipe.json` of TheBlogTheme.

```json
{
  "steps": [
    { "name": "feature", "enable": [ "OrchardCore.Lucene", "OrchardCore.Search.Lucene.ContentPicker" ] },
    {
      "name": "CreateOrUpdateIndexProfile",
      "indexes": [
        {
          "IndexName": "Catalog",
          "ProviderName": "Lucene",
          "Type": "Content",
          "Name": "Catalog",
          "Properties": {
            "LuceneIndexMetadata": { "StoreSourceData": false, "AnalyzerName": "standardanalyzer" },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": [ "Product" ], "Culture": "any" },
            "LuceneIndexDefaultQueryMetadata": { "DefaultVersion": "LUCENE_30", "QueryAnalyzerName": "standardanalyzer", "AllowLuceneQueries": false, "DefaultSearchFields": [ "Content.ContentItem.FullText" ] }
          },
          "IndexLatest": false,
          "Culture": "any",
          "IndexedContentTypes": [ "Product" ]
        }
      ]
    }
  ]
}
```

Without the recipe: enable the two features on the Features page, then Search, Indexes, Add index
(`OrchardCore.Indexing/Views/Admin/Index.cshtml:31`), choose "Content in Lucene" and select Add, enter Index Name
**Catalog** and Name **Catalog** (`Views/IndexProfileFields.Edit.cshtml:6,14`), keep the Analyzer, tick only the content
type Product under Content types, leave "Index Latest version" cleared, and Save.

Wait for the index: creating it starts the synchronizing in the background
(`OrchardCore.Indexing/Controllers/AdminController.cs:255,257`). The index is ready when it returns the Product items:
check it once on a copy of the site, by doing the steps of `05-lucene-field` and typing `rain` in the Products picker of the
bundle's editor, which must list Rain Jacket. Then take Snapshot C on the site you record, not on the copy.

## Order, dependencies and snapshots

The clips change the site and depend on each other. Record them in this order, which is also the order they play in:

| # | Clip | Chapter | Changes the site | Needs |
|---|---|---|---|---|
| 1 | `02-add-picker` | 02 | Adds the field Products (Content Picker Field) to Product Bundle | Seeds 1 and 2 |
| 2 | `03-pick` | 03 | Publishes the Weekend Camping Bundle with three products | Clip 1 |
| 3 | `04-user-field` | 04 | Adds the field Bundle Manager (User Picker Field) | Clip 1 |
| 4 | `04-user-pick` | 04 | Publishes the bundle with Bundle Manager set | Clips 2 and 3 |
| 5 | `05-lucene-field` | 05 | Sets the Products field's editor to Lucene and its index to Catalog | Seed 3, clip 4 |
| 6 | `05-lucene-pick` | 05 | Nothing is saved | Clip 5 |

The slide clips (`01-relate`, `05-index`, `06-recap`) and the two cards need no site and can be made at any time.

Snapshots (copy `App_Data` and `state.json` aside, with the site stopped):

1. **Snapshot A**: after Seeds 1 and 2, before clip 1. Restore it before every full take, and before re-recording
   clips 1 to 4.
2. **Snapshot B**: after clip 4, before Seed 3. To record chapter 05 again from scratch, restore B, then import Seed 3.
3. **Snapshot C**: after Seed 3 and the index is ready, before clip 5. Restore it before re-recording clip 5 or 6.

To re-record one clip, restore the snapshot that precedes it and replay the clips between the snapshot and it.

## Clips

Every clip is recorded at 1536x864 in the light color scheme. The admin menu may stay as it is.

**Rule for the Content Picker settings page** (clips 1 and 5): the "Title Pattern" box under Hint holds a default text
that is code. It must never be in the frame when a step ends, nor at rest for more than a moment. Scroll so that the
frame shows the settings above it (clip 1) or starts at "What type of editor should be used?" (clip 5). Look at every
contact sheet for it.

**Ids on the settings pages.** The inputs that the settings of a field draw (Required, Allow multiple, Source, Hint,
Placeholder, Display All Users, Index) have ids and names with a prefix of the form
`ProductBundle_Products_ContentPickerFieldSettingsDriver_Multiple` (part, field, driver, then the property:
`ContentPartFieldDefinitionDisplayDriver.cs:12-24` in `OrchardCore.ContentTypes.Abstractions`, applied by `DefaultHtmlDisplay.cs:93`). The plan
therefore selects them by the end of the id (`[id$='_Multiple']`). The ids of the page itself (`#DisplayName`, `#Name`,
`#Editor`, `#DisplayMode`) and the fixed ids `#ContentTypesContainer`, `#StereotypesContainer` and `#userRolesVue` have no prefix.
Confirm each selector once with `Recorder probe` before recording.

### `02-add-picker` (chapter 02, 6 steps)

Starts on `/Admin/ContentTypes/List` with Design and Content Definition expanded in the admin menu and the rows
Product and Product Bundle both visible, because the narration of `add-1` names the two types (expand the menu before
`StartCaptureAsync`). Narration is in `storyboard.json`.

- `add-1`: Highlight the menu entry Content Types (caption "Design, Content Definition, Content Types"), then clear it.
  Click the Edit button of the Product Bundle row, selector `[role='btn-edit-ProductBundle']`, with
  `ClickAndWaitForNavigationAsync`. On the page "Edit Content Type - Product Bundle", click the button Add Field,
  selector `a.btn-info:has-text('Add Field')`, with `ClickAndWaitForNavigationAsync`.
  *On screen at the end:* the page titled Add New Field To "Product Bundle", with Display Name, Technical Name and the
  Field Type list. The title shows the display name of the part, which Seed 1 sets to Product Bundle (a part without that
  setting is titled with its name, ProductBundle: `ContentPartSettingsExtensions.cs:54-62`). Labels: `OrchardCore.ContentTypes/AdminMenu.cs:23,24,25`, `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`,
  `Views/Admin/Edit.cshtml:137`, `Views/Admin/AddFieldTo.cshtml:20`.
- `add-2`: Type `Products` in Display Name (selector `#DisplayName`) and press Tab; a script copies the letters and digits of the
  Display Name into Technical Name (selector `#Name`), so wait until it shows `Products`
  (`OrchardCore.ContentTypes/wwwroot/Scripts/autocomplete/autocomplete.js`; it drops the spaces). Click the radio of Content
  Picker Field (selector `input[name='FieldTypeName'][value='ContentPickerField']`; the list is in alphabetical order, so
  Playwright scrolls to it). Click the button Save & Configure (selector
  `button[value='SaveAndConfigure']`) with `ClickAndWaitForNavigationAsync`.
  *On screen at the end:* the page titled "Products" settings, scrolled to the top: Required and Allow multiple.
  Labels: `Views/Admin/AddFieldTo.cshtml:38,46,54,72`, `Views/Admin/EditField.cshtml:30`. The field type name "Content Picker
  Field" is the name `ContentPickerField` in words (`CamelFriendly`, `AddFieldTo.cshtml:62`).
- `add-3`: Highlight the checkbox labels Required and Allow multiple one after the other (about 1.5 s each; selectors
  `label[for$='_Required']`, `label[for$='_Multiple']`), clear the highlights, then click the checkbox
  `input[id$='_Multiple']`.
  *On screen at the end:* Allow multiple ticked, Required not ticked. Labels:
  `OrchardCore.ContentFields/Views/ContentPickerFieldSettings.Edit.cshtml:17,27`.
- `add-4`: Highlight the whole group of three radios under the label Contained Content Types (selector `[id$='_Source']`;
  caption "Contained Content Types"), then clear it. The radio **Content Types** is selected by default; do not change it.
  *On screen at the end:* the three radios Display All Content Types, Content Types and Stereotype (with its hint "All
  content types of these Stereotypes"), Content Types selected, and below them the list of content types with "Select
  all". The box Stereotypes may also be on screen (OPEN-QUESTIONS.md, item 10): that is not a problem, but keep the
  Title Pattern box out of the frame. Labels: `ContentPickerFieldSettings.Edit.cshtml:34,39,45,51`; the default comes from `ContentPickerFieldSettingsDriver.cs:115,124`.
- `add-5`: Click the checkbox of **Product** in the list (selector `#ContentTypesContainer label:text-is('Product')`).
  Do not tick Product Bundle or any other type.
  *On screen at the end:* only Product ticked in the list of content types. Labels: `OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml:10,23`.
- `add-6`: Scroll until the Hint box (selector `textarea[id$='_Hint']`) is just above the action bar that stays fixed at the
  bottom of the frame, so that Title Pattern is below the frame. Type `Choose the products in this bundle.` in it. Click
  Save (selector `button.save`, text "Save") with `ClickAndWaitForNavigationAsync`. The action bar is fixed at the bottom of the
  window (`OrchardCore.Themes/TheAdmin/Assets/scss/components/_action-bar.scss`, `.action-bar-bottom`), so Save needs no
  scrolling; do not click in the Title Pattern box.
  *On screen at the end:* the page "Edit Content Type - Product Bundle", with the row "Products" and the words "Content
  Picker Field" beside it, under the heading Fields. Labels: `ContentPickerFieldSettings.Edit.cshtml:82`,
  `Views/Admin/EditField.cshtml:120`, `Views/Admin/Edit.cshtml:88,115`.

### `03-pick` (chapter 03, 6 steps)

Starts on `/Admin/Contents/ContentItems/ProductBundle` (the list of Product Bundle items, one row: Weekend Camping Bundle).
Changes the site: the bundle is published with three products. Restore Snapshot A and record clip 1 again first to repeat it.

- `pick-1`: Click the Edit button of the row Weekend Camping Bundle (selector `a.edit`, with
  `ClickAndWaitForNavigationAsync`). Highlight the Products field (selector `.ocat-wrapper:has(.vue-multiselect[data-part='ProductBundle'][data-field='Products'])`).
  *On screen at the end:* the editor with the field Products and, under it, the hint "Choose the products in this bundle."
  Labels: `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`; the field is drawn by
  `ContentPickerField.Edit.cshtml:29,33` (the label is the field's display name, the hint is the setting Hint).
- `pick-2`: Click inside the picker (the text input inside `.vue-multiselect[data-part='ProductBundle'][data-field='Products']`; the attributes are written by `ContentPickerField.Edit.cshtml:29`)
  and type `pack` with `TypeAsync`. Wait until an option containing "Trail Backpack" is visible (probably
  `.multiselect__option:has-text('Trail Backpack')`; use `Recorder probe` to confirm: the component is not in the source
  tree, see OPEN-QUESTIONS.md, item 1). Hold 1 s so the list is readable, then click the option.
  *On screen at the end:* Trail Backpack selected in the field. Behavior: `DefaultContentPickerResultProvider.cs:45,51,54,75`
  (text contained in the title or in the content type name, 50 results at most, sorted by title; the query takes the
  50 before sorting, which does not matter with five products). The typed lower-case `stove` finds Camp Stove only if the
  database ignores case, as SQLite does for ASCII letters in a `LIKE` comparison (to confirm: OPEN-QUESTIONS.md, item 11).
- `pick-3`: Repeat for `stove` (select Camp Stove) and `lamp` (select Headlamp).
  *On screen at the end:* Trail Backpack, Camp Stove and Headlamp selected in the field; Insulated Bottle and Rain Jacket
  not selected.
- `pick-4`: Click Publish (selector `button.publish`) with `ClickAndWaitForNavigationAsync`.
  *On screen at the end:* the list of Product Bundle items with the notification "Your Product Bundle has been published."
  Labels: `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`, `Controllers/AdminController.cs:966,967` (the message
  of an edit that publishes; line 473 is the message of a new item). The page returns to the list because the Edit link
  carries the list as `returnUrl` (`AdminController.cs:841-858`).
- `pick-5`: `NavigateAsync("/bundles/weekend-camping-bundle")`, then highlight the field (selector
  `div.field-type-contentpickerfield`).
  *On screen at the end:* the bundle's page with the word Products followed by Trail Backpack, Camp Stove and Headlamp as links, in
  any order, separated by commas. Behavior: `OrchardCore.ContentFields/Views/ContentPickerField.cshtml` (lines 17 to 34).
- `pick-6`: Click the link Camp Stove with `ClickAndWaitForNavigationAsync`.
  *On screen at the end:* the page `/products/camp-stove` titled Camp Stove.

### `04-user-field` (chapter 04, 3 steps)

Starts on `/Admin/ContentTypes/Edit/ProductBundle` (the type editor, listing the field Products). Changes the site: adds a field.

- `user-1`: Click Add Field (selector `a.btn-info:has-text('Add Field')`) with `ClickAndWaitForNavigationAsync`. Type
  `Bundle Manager` in Display Name and press Tab (Technical Name then shows `BundleManager`). Click the radio of User Picker Field (selector
  `input[name='FieldTypeName'][value='UserPickerField']`). Click Save & Configure with `ClickAndWaitForNavigationAsync`.
  *On screen at the end:* the page titled "Bundle Manager" settings, scrolled to the top. The User Picker Field appears in
  the Field Type list only while the Users feature is enabled (`OrchardCore.ContentFields/Startup.cs:158`).
- `user-2`: Highlight Required and Allow multiple (do not tick them). Scroll to the label Roles. Click the checkbox Display
  All Users (selector `input[id$='_DisplayAllUsers']`) to clear it; the list of roles appears. Tick **Editor** (the checkbox labeled
  Editor in the list of roles).
  *On screen at the end:* Display All Users cleared, Editor ticked, the other roles not ticked. Labels:
  `OrchardCore.ContentFields/Views/UserPickerFieldSettings.Edit.cshtml:9,19,26,32`. Behavior: `DefaultUserPickerResultProvider.cs:34,37`.
- `user-3`: Scroll to the label "What type of display mode should be used?" (selector `#DisplayMode`) and highlight the drop-down
  list; it shows Standard. Do not change it. The narration ends with "We keep Standard and save.": click Save
  (selector `button.save`) with `ClickAndWaitForNavigationAsync` as soon as the narration ends, inside the clip's tail of one second.
  *On screen at the end of the narration:* the drop-down list showing Standard; the tail then shows the type editor with Bundle Manager listed
  as a User Picker Field. The two options are Standard and User Names (`UserPickerField.DisplayOption.cshtml:4`,
  `UserPickerField-UserNames.DisplayOption.cshtml:4`; label `Views/Admin/EditField.cshtml:100`). Behavior:
  `UserPickerField.cshtml:8` (Standard looks the users up), `ViewModels/DisplayUserPickerFieldUserNamesViewModel.cs:10` and
  `Drivers/UserPickerFieldDisplayDriver.cs:93` (User Names reads the names stored at the last save).

### `04-user-pick` (chapter 04, 2 steps)

Before `StartCaptureAsync`: open `/Admin/Contents/ContentItems/ProductBundle`, click the Edit button of Weekend Camping
Bundle, wait for the editor (the fields Products with its three products, and Bundle Manager, empty). Changes the site: the bundle
is published with the manager.

- `user-4`: Click the picker of Bundle Manager (the input inside `.vue-multiselect[data-part='ProductBundle'][data-field='BundleManager']`; the attributes are written by `UserPickerField.Edit.cshtml:28`)
  and type `or`. Wait until jordan and morgan are listed, hold 4 s (the narration says that taylor is not offered), then
  click jordan.
  *On screen at the end:* jordan selected in Bundle Manager. The list shown while holding contains jordan and morgan only.
- `user-5`: Click Publish (selector `button.publish`) with `ClickAndWaitForNavigationAsync`, then
  `NavigateAsync("/bundles/weekend-camping-bundle")` and highlight the field (selector `div.field-type-userpickerfield`).
  *On screen at the end:* the bundle's page with Bundle Manager followed by jordan, next to the Products field.

### `05-lucene-field` (chapter 05, 2 steps)

Before recording: Seed 3 done and the index ready (Snapshot C). Starts on `/Admin/ContentTypes/Edit/ProductBundle`.
Changes the site: the Products field uses the Lucene editor.

- `lucene-1`: Click the Edit button of the row Products (selector `li:has-text('Products') a:has-text('Edit')`) with
  `ClickAndWaitForNavigationAsync`. Scroll so that the label "What type of editor should be used?" is at the top of the
  frame (Title Pattern must be out of the frame). Highlight the drop-down list (selector `#Editor`), then choose
  **Lucene** with `SelectAsync`.
  *On screen at the end:* the drop-down list showing Lucene, with the Index setting below it (the script that shows it
  is not in the source tree: OPEN-QUESTIONS.md, item 8). Labels:
  `Views/Admin/EditField.cshtml:79`, `OrchardCore.Lucene/Shapes/LuceneContentPickerShapeProvider.cs:26`
  (the options are Standard and Lucene).
- `lucene-2`: Highlight the Index drop-down list (selector `select[id$='_Index']`), choose **Catalog** with `SelectAsync`, then click Save
  (selector `button.save`) with `ClickAndWaitForNavigationAsync`.
  *On screen at the end:* the type editor with the row Products and a badge Lucene (the badge with the pen icon). Labels:
  `OrchardCore.Lucene/Views/ContentPickerFieldLuceneEditorSettings.Edit.cshtml:5,13`, `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:123`.

### `05-lucene-pick` (chapter 05, 2 steps)

Before `StartCaptureAsync`: open the bundle's editor as in `04-user-pick`. Nothing is saved: leave the page without
publishing when the clip ends.

- `lucene-3`: Click the picker of Products and type `rain`. Wait until Rain Jacket is listed.
  *On screen at the end:* the list showing Rain Jacket. Behavior: `LuceneContentPickerResultProvider.cs:64,67,69` (the typed text
  is compared with the start of the lower-cased title; the field's content types are the filter).
- `lucene-4`: Clear the typed text (select all, Delete) and type `jacket`. Wait until the list has no entry.
  *On screen at the end:* the picker with `jacket` typed and no entry in the list (the component may show a message such
  as "No result found": see OPEN-QUESTIONS.md, item 1).

### Slide steps and cards

| Step | What it shows |
|---|---|
| `intro` | Opening card, no recording |
| `relate-1` | Slide "Ways to relate content items": Bag Part, List Part, Taxonomies bright, Content Picker Field dimmed |
| `relate-2` | The same slide with Content Picker Field bright |
| `relate-3` | Slide "Typical uses of the pickers": product bundle, blog post, any content item |
| `relate-4` | Slide "Three picker fields": Content Picker, User Picker, Localization Set Content Picker |
| `index-1` | Slide "Where the picker searches": Standard, Lucene, Elasticsearch |
| `recap-1` | Slide "Content pickers at a glance": the two fields bright |
| `recap-2` | The same slide with References and Editors bright |
| `outro` | Closing card, no recording |

## Checks for private data

- Only the invented names of this plan appear: the products, the bundle, the users morgan, jordan and taylor, and the
  address `admin@example.com`. No other user, e-mail address, host name or token. Look for the admin menu's user name and the
  profile menu.
- No password on screen: the Generate password button is used only in Seed 2, outside the capture.
- The Title Pattern box (default code) is not in any frame at the end of a step: check the contact sheets of `02-add-picker`
  and `05-lucene-field`, and OCR the video for the characters `{{`.
- The Lucene index folder and `App_Data` are not copied into the repository; `state.json` is git-ignored.

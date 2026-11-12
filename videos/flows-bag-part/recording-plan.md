# Recording plan: Bag Part, Embed Content Items Inside a Content Item

The video has four slides and five browser recordings. The recordings use the Bag part of the Flows module on a Blog
site, with two content types seeded outside the capture (**Question** and **Tip**) and one content type created on
camera (**Help Page**). Nothing is built and no code is typed on camera.

In this plan a selector stands for `scene.Page.Locator(...)`. `OC` is the Orchard Core checkout the catalog was read
from (commit `92db4a83`), and every path below is relative to its root. `CT` stands for
`src/OrchardCore.Modules/OrchardCore.ContentTypes` and `FL` for `src/OrchardCore.Modules/OrchardCore.Flows`. A label is
quoted as the code spells it, with the file and line where it comes from. Where a selector depends on markup that this plan could only read, not run, it
says so: confirm it with `Recorder probe` before the first take.

## The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of `OC` (or a later checkout), on `http://localhost:5010` |
| Data | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup (`src/docs/reference/modules/AutoSetup/README.md`), so no setup form is recorded |
| Tenant | `Default` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Recipe | `Blog` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`), site theme `TheBlogTheme` (line 85) |
| User | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Database | Sqlite |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls no outside service |

Why the Blog recipe: its feature list enables everything the video uses, so no feature is switched on before
recording: `OrchardCore.Flows` (`blog.recipe.json` line 60, the module that provides the Bag part),
`OrchardCore.Widgets` (75, a dependency of Flows: `src/OrchardCore.Modules/OrchardCore.Flows/Manifest.cs` line 14),
`OrchardCore.Contents` (54), `OrchardCore.ContentTypes` (55), `OrchardCore.ContentFields` (52, for the Text field),
`OrchardCore.Title` (73) and `OrchardCore.Deployment` (57, for the JSON Import used to seed). The recipe also makes
three creatable types (Article, Blog and Page, lines 136, 248 and 448), so once **Help Page** exists the **New** button
of the Content Items page is a drop-down list with four entries
(`src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml` lines 12 to 28), which is the
page the narration of `edit-1` describes.

Features to enable before recording: none. Settings to save: none. External services and local stand-ins: none.

### Seed, outside the capture

1. Start the site. Auto Setup installs it on the first request.
2. Run `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, which saves `state.json` (git-ignored).
3. Import the two content types that the Bag will hold. Open `/Admin/DeploymentPlan/Import/Json` (menu Tools,
   Deployments, JSON Import: `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs` lines 18, 19 and 30; route
   in `Controllers/ImportController.cs` line 20), put the recipe below in the **JSON** box and select **Import**
   (`Views/Import/Json.cshtml` lines 15 and 21). The box is a CodeMirror editor; set its text with
   `document.querySelector('.CodeMirror').CodeMirror.setValue(...)` in a `Recorder probe` or `PrepareAsync`, then submit
   the form. The page answers **Recipe imported successfully!** (`ImportController.cs` line 185).

   ```json
   {
     "steps": [
       {
         "name": "ContentDefinition",
         "ContentTypes": [
           {
             "Name": "Question",
             "DisplayName": "Question",
             "Settings": {
               "ContentTypeSettings": {
                 "Draftable": true,
                 "Versionable": true,
                 "Description": "A question with its answer."
               }
             },
             "ContentTypePartDefinitionRecords": [
               { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
               { "PartName": "Question", "Name": "Question", "Settings": { "ContentTypePartSettings": { "Position": "1" } } }
             ]
           },
           {
             "Name": "Tip",
             "DisplayName": "Tip",
             "Settings": {
               "ContentTypeSettings": {
                 "Draftable": true,
                 "Versionable": true,
                 "Description": "A short piece of advice."
               }
             },
             "ContentTypePartDefinitionRecords": [
               { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
               { "PartName": "Tip", "Name": "Tip", "Settings": { "ContentTypePartSettings": { "Position": "1" } } }
             ]
           }
         ],
         "ContentParts": [
           {
             "Name": "Question",
             "Settings": {},
             "ContentPartFieldDefinitionRecords": [
               { "FieldName": "TextField", "Name": "Answer", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Answer", "Position": "0" } } }
             ]
           },
           {
             "Name": "Tip",
             "Settings": {},
             "ContentPartFieldDefinitionRecords": [
               { "FieldName": "TextField", "Name": "Details", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Details", "Position": "0" } } }
             ]
           }
         ]
       }
     ]
   }
   ```

   The shape of this recipe is copied from the content types of the Blog recipe (`blog.recipe.json` lines 352 to 374 for
   a type with one own part, lines 705 to 722 for the part with a Text field). It was not run. Because the recipe sets
   neither `Creatable` nor `Listable`, both types stay out of the **New** list and of the Content Items list
   (`ContentTypeSettings.cs` properties default to false); they are only offered by the bag. Keep the display name of
   each type equal to its technical name (**Question**, **Tip**): the Standard editor of the bag builds the list of
   types it lets a card move among from the display names (`FL/Views/BagPart.Edit.cshtml` line 14 joins the type
   definitions, and `ContentTypeDefinition.ToString()` returns the display name), and compares it with the technical
   name of each card (`FL/Assets/ts/flows.edit.ts` lines 238 to 242). With another display name (for example **Quick
   Tip** for `Tip`) the drag of `manage-2` is refused.

   If the import reports an error, create the same two types by hand at `/Admin/ContentTypes/List` instead. A type
   made in the admin is not the same as one made by the recipe: `ContentDefinitionService.AddTypeAsync` makes it
   Creatable, Draftable, Versionable, Listable and Securable, with the thumbnail `~/OrchardCore.ContentTypes/placeholder.png`
   (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Services/ContentDefinitionService.cs` lines 81 to 91, and
   `Startup.cs` line 45). So, for each of **Question** and **Tip**:
   1. **Create new type**, **Display Name** `Question` (or `Tip`), **Create**; on **Add Parts** tick **Title** and select
      **Add Selected Parts**.
   2. On **Edit Content Type**, select **Add Field** (`CT/Views/Admin/Edit.cshtml` line 137). On the page it opens,
      type **Display Name** `Answer` (or `Details`), choose the **Field Type** **Text Field**
      (`CT/Views/Admin/AddFieldTo.cshtml` lines 38, 54 and 62; the first entry of the list is preselected, so choose
      it explicitly) and select **Save**.
   3. Back on **Edit Content Type**, in the settings above the **Fields** and **Parts** lists
      (`CT/Views/ContentTypeSettings.Edit.cshtml`), fill **Description** with `A question with its answer.` (or
      `A short piece of advice.`), untick **Creatable**, **Listable** and **Securable** (lines 17, 30 and 69), clear
      **Thumbnail** (line 105), and select **Save**. **Draftable** and **Versionable** may stay ticked. Without these changes the two types would also appear in the
      **New** list (six entries instead of four) and in the Content Items list, and the cards of the Blocks dialog in
      `blocks-3` would show a thumbnail.
4. Check the seed. At `/Admin/ContentTypes/List` both **Question** and **Tip** are listed, and opening **Question**
   shows the parts **Question** (own part) and **Title**, with a field **Answer**. There is no content type named
   **Help Page** yet.
5. Stop the site and copy its `App_Data` folder and `state.json` aside. This is snapshot **S0**.

The data typed on camera is the only content: the content type **Help Page** with its bag **Help Items**, and the
content item **Account Help** with a **Question** and a **Tip**. Everything is invented.

## Order and snapshots

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-add` | S0 | Yes: creates the content type Help Page with the part Title and the named Bag part Help Items (contained types Question and Tip, Collapse Contained Items on, editor Standard) |
| 2 | `03-edit` | S1 | Yes: creates and publishes the content item Account Help, with a Question and a Tip |
| 3 | `04-manage` | S2 | Yes: publishes Account Help again with the Tip before the Question |
| 4 | `05-view` | S3 | No |
| 5 | `06-blocks` | S3 | Yes: sets the editor of Help Items to Blocks |

S1 is the site as `02-add` leaves it, S2 as `03-edit` leaves it and S3 as `04-manage` leaves it. After recording a
clip, stop the site and copy `App_Data` aside under the next number. `03-edit` depends on `02-add`, `04-manage` on
`03-edit`, and `05-view` and `06-blocks` on `04-manage`. To record one clip again, restore the snapshot it starts from.
To record everything, restore S0 and record in the order above. `05-view` and `06-blocks` both start from S3; record
`05-view` first, because `06-blocks` changes the editor of the bag. The slide steps `bag-1` to `bag-4` need no site.

Narrate before recording (`scripts/narrate.py`): the recorder holds each step for the length of its narration. The
time estimates below use 2.5 words per second; check the real lengths in `durations.json`. A step whose actions run
longer than its narration only adds a pause, but the frame at the end of the narration then shows the middle of the
actions, so shorten the typed text before lengthening the step.

## Slide steps

The slides are drawn by `slides.py`, with no site. Each one is on screen for the narration of its step.

- `bag-1`: heading "A Bag holds content items", four cards; the first three ("Holds content items", "You choose the
  types", "Edited in place") are bright, the fourth is dimmed.
- `bag-2`: the same four cards; "Holds content items" and "One document" are bright.
- `bag-3`: heading "Flow part and Bag part", two cards: "Flow part" (widgets, each with an alignment and a size) and
  "Bag part" (items of the content types you choose, with no layout settings).
- `bag-4`: heading "Where the Bag part comes from", three cards: "The Flows feature", "Recipes such as Blog", "Any other
  site" (Tools, then Features).

Sources for the statements on the slides: the Bag part keeps its items in the part's own list
(`src/OrchardCore/OrchardCore.Flows.Core/Models/BagPart.cs` line 9, and `src/docs/reference/modules/Flow/BagPart.md`
line 5, "stored in the database as one single document"); the Flow part holds widgets and gives each an alignment and a
size (`Models/FlowPart.cs` line 9, `Models/FlowMetadata.cs` lines 16 and 17, and the Widget-only type list of
`Views/FlowPartSettings.Edit.cshtml` line 9 with the "What default alignment should be used?" label at line 24); the
Bag part has no alignment or size (`Views/BagPart.Edit.cshtml` passes `data-flowmetadata="false"`, lines 70 and 91);
the feature is named "Flows" (`src/OrchardCore.Modules/OrchardCore.Flows/Manifest.cs` line 12) and the menu is
Tools, Features (`src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs` lines 26 and 27).

## Clip `02-add`

Starts on `/Admin/ContentTypes/List` (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs`
line 72), opened with `scene.OpenAsync` before `StartCaptureAsync`. Expand the sidebar menu **Design**, **Content
Definition** first, so the path of the narration is visible with **Content Types** current
(`src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs` lines 23 to 25). The page title is **Content Types**
(`Views/Admin/List.cshtml` line 6), the list holds the types of the Blog recipe plus **Question** and **Tip**, and no
type is named Help Page. In the lines below, `CT` stands for `src/OrchardCore.Modules/OrchardCore.ContentTypes`.

- `add-1` (about 9 s). Highlight the menu entry **Content Types**, then click **Create new type**
  (`CT/Views/Admin/List.cshtml` line 20; selector `#btnCreate`) with `ClickAndWaitForNavigationAsync`. On the page
  **New Content Type** (`CT/Views/Admin/Create.cshtml` line 5), type `Help Page` into **Display Name** (line 14; selector
  `#DisplayName`, typed with `TypeAsync`, Tab afterwards so the field blurs). The **Technical Name** (line 22; selector
  `#Name`) fills itself from the display name when a key is released or the field loses focus
  (`CT/Assets/js/autocomplete.ts` lines 12 to 22). Do not type in it. Then click **Create** (line 30; selector
  `button.create`) with `ClickAndWaitForNavigationAsync`. *Visible when the narration ends:* the page **Add Parts** with
  the green message `The "Help Page" content type has been created.` (`CT/Controllers/AdminController.cs` line 162),
  or, if the narration ends a moment earlier, the Create form with Help Page typed and the Technical Name filled.
  Before the first take, read the Technical Name on a rehearsal: the next clips do not use it by name, they reach the
  type from the Content Types list, but a name that is not letters only (a hyphen, a space) is a sign that the script
  did not run and the take must be redone.
- `add-2` (about 10 s). The page is **Add Parts** (`CT/Views/Admin/AddPartsTo.cshtml` line 10), with the line "Choose
  the Parts to add to this Content Type." (line 18) and one row per part, alphabetical. Scroll to the row whose label
  reads **Title** (line 33; the part display name is the part name without "Part":
  `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs`
  lines 54 to 62), and click its checkbox. Selector for the checkbox:
  `li.list-group-item:has(label.form-check-label:text-is("Title")) input[type=checkbox]`. The button **Add Selected
  Parts** (line 47; selector `#add-selected-parts-button`) appears only once a box is ticked; click it with
  `ClickAndWaitForNavigationAsync`. Do not use the per-row **Add & Configure** buttons (line 36). *Visible at the end:*
  the page **Edit Content Type - Help Page** (`CT/Views/Admin/Edit.cshtml` line 63), the message
  `The "TitlePart" part has been added.` (`CT/Controllers/AdminController.cs` line 372; it is on screen at the end of this step and its wording is not read
  aloud), and under **Parts** (line 144) one row, **Title** (with **Edit** and **Remove**). There is no row **Help
  Page**: the list skips a part that has no stored definition (`CT/Views/Admin/Edit.cshtml` lines 25 to 30), and the
  part of the type itself gets one only when a field is added to it (`CT/Services/ContentDefinitionService.cs` line 82
  stores the type with that part but writes no part definition). The list is sorted by the positions of the parts
  (`CT/Views/Admin/Edit.cshtml` lines 32 and 42); **Title** takes position 1 here (`AddPartToTypeAsync`, line 143, and
  `GetPartPosition`, lines 481 to 521, count the own part without a position as 0 and give the next free number).
  Confirm the list on the first rehearsal (see OPEN-QUESTIONS.md).
- `add-3` (about 10 s). Hold on the type page, highlighting the **Parts** list for about two seconds. Then click **Add
  Named Part** (`CT/Views/Admin/Edit.cshtml` line 202; selector `a:text-is("Add Named Part")`) with
  `ClickAndWaitForNavigationAsync`. *Visible at the end:* the page **Add Named Part** (`CT/Views/Admin/AddReusablePartTo.cshtml`
  line 11) with the empty fields **Display Name**, **Technical Name** and **Description** and, below them, one radio
  button labelled **Bag** next to the hint "Provides a collection behavior for your content item where you can place
  other content items." (`src/OrchardCore.Modules/OrchardCore.Flows/Migrations.cs` line 26), already selected
  (`CT/Controllers/AdminController.cs` line 310 preselects the first reusable part; only the Bag part is reusable on this site).
- `add-4` (about 8 s). Type `Help Items` into **Display Name** (`AddReusablePartTo.cshtml` line 23; selector
  `#DisplayName`) and Tab. Let **Technical Name** (line 31, `#Name`) fill itself. Type `Questions and tips.` into
  **Description** (line 39; selector `#Description`). Highlight the selected **Bag** radio button. Click **Save &
  Configure** (line 76; selector `button:text-is("Save & Configure")`) with `ClickAndWaitForNavigationAsync`. The label
  is the text `Save &amp; Configure` in the source, which the browser shows as "Save & Configure". *Visible at the end:*
  the page **Edit Part - Help Items** (`CT/Views/Admin/EditTypePart.cshtml` line 22), with the green message
  `The "BagPart" part has been added.` (`CT/Controllers/AdminController.cs` line 453, which names the part by its technical
  name; the narration does not read it), the light-green note "Some part settings common to all types can also be
  defined at the Part Definition level." (line 29), the fields **Display Name** (value `Help Items`) and
  **Description**, and, below, the bag settings.
- `add-5` (about 10 s). On **Edit Part - Help Items**, the bag settings start with a required label **Contained Content
  Types** over two radio buttons, **Content Types** (selected) and **Stereotype** with the hint "All content types of
  these Stereotypes" (`src/OrchardCore.Modules/OrchardCore.Flows/Views/BagPartSettings.Edit.cshtml` lines 6, 11, 17).
  Highlight that radio group (selector `.ocat-end:has(#SourceContentTypes)`), without clicking **Stereotype** (the
  take must not change the source). Below it, a second **Contained
  Content Types** label (line 24) with the hint "The content types that this bag can contain, e.g. Slide for a Slide
  Show." (line 26) and a **Select all** box, then one box per content type, alphabetical by display name
  (`src/OrchardCore.Modules/OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml` lines 10 and
  22). Tick **Question** and then **Tip** (selectors
  `#ContentTypesContainer input[type=checkbox][value="Question"]` and `...[value="Tip"]`; the value is the technical
  name). *Visible at the end:* exactly those two boxes ticked, with their labels on screen; scroll so the radio group
  and both ticked boxes fit in the frame if the page allows it. The take must not tick **Select all**.
  The view also holds a third block, a label **Stereotypes** with a text box (lines 31 to 37), which the page script
  `FL/Assets/ts/bag-part-settings.ts` is meant to hide while **Content Types** is selected. The script looks the radio
  buttons up by the name `Source`, but their names carry the prefix of the driver, so it may find nothing and leave
  the box visible (OPEN-QUESTIONS.md, item 11). If the **Stereotypes** box is on screen, leave it empty, keep it out of
  the highlight, and scroll so that it is not the focus of the frame. The narration does not mention it.
- `add-6` (about 13 s). Scroll down to the label **Display Type** (`BagPartSettings.Edit.cshtml` line 40), an empty text
  box that shows the grey placeholder `Summary` and the hint "The display type to use when rendering the content items.
  Default is Summary." (line 43; selector `input[placeholder="Summary"]`). Highlight it, and leave it empty. Highlight
  the checkbox **Collapse Contained Items** (line 51; selector `input[type=checkbox][name$="CollapseContainedItems"]`)
  and its hint "When checked, the contained items will be collapsed when the edit form is loaded." (line 52), then click
  the box. *Visible at the end:* **Display Type** empty and **Collapse Contained Items** ticked.
- `add-7` (about 8 s). Scroll to the label "What type of editor should be used?" (`CT/Views/Admin/EditTypePart.cshtml`
  line 70) over a drop-down list whose selected entry is **Standard**
  (`src/OrchardCore.Modules/OrchardCore.Flows/Views/BagPart.Option.cshtml` line 4; selector
  `select[data-type-part-editor-select]`). Highlight it; do not open it. Click **Save** (line 111; selector
  `button.save`) with `ClickAndWaitForNavigationAsync`. *Visible at the end:* the page **Edit Content Type - Help Page**
  with the green message `The "Help Items" part settings have been saved.` (`CT/Controllers/AdminController.cs` line
  1084) and under **Parts** the rows **Title** and **Help Items**, in that order (positions 1 and 2:
  `AddReusablePartToTypeAsync`, line 158, and `GetPartPosition`, lines 481 to 521), each with the buttons **Edit** and **Remove**
  (`CT/Views/Admin/Edit.cshtml` lines 163 and 174). If the page does not list **Help Items**, one of the
  settings was rejected: the message "At least one content type must be selected." (`Settings/BagPartSettingsDisplayDriver.cs`
  line 97) means the two boxes were not ticked; record the clip again.

If the page **Edit Part** shows the display name **Bag** instead of **Help Items**, the part was added from **Add
Parts** instead of **Add Named Part**: restore S0 and record the clip again.

## Clip `03-edit`

Starts on `/Admin/Contents/ContentItems` (`src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs`
line 67), opened before the capture, with the sidebar menu **Content** expanded so that **Content Items** is current
(`src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs` lines 65 and 68). The page title is **Manage Content**
(`Views/Admin/List.cshtml` line 17) and it lists the listable content items that the Blog recipe creates (their titles come from
`blog.recipe.json`). In the lines below, `FL` stands for `src/OrchardCore.Modules/OrchardCore.Flows`.

- `edit-1` (about 7 s). Click **New** (`Contents/Views/ContentsAdminListCreate.cshtml` line 20; selector `#new-dropdown`),
  hold about two seconds on the open list of the creatable types (Article, Blog, Help Page, Page), then click **Help
  Page** (selector `ul.dropdown-menu a.dropdown-item:text-is("Help Page")`) with `ClickAndWaitForNavigationAsync`.
  *Visible at the end:* the page **New Help Page** (`Contents/Views/Admin/Create.cshtml` line 26) with the label **Title**
  and an empty input, and under it the label **Help Items** (`Contents/Views/ContentPart-BagPart.Edit.cshtml` line 23;
  the label is the display name given in `add-4`) with a button **Add Item** and no card.
- `edit-2` (about 9 s). Type `Account Help` into the page's **Title** input (`src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml`
  line 12; selector `form input.content-caption-text` with `.First`, which is the page's own Title because the bag
  has no card yet; confirm with `Recorder probe`). Highlight the button **Add Item** (`FL/Views/BagPart.Edit.cshtml`
  line 79; selector `.btn-bag-add button.dropdown-toggle`). *Visible at the end:* the title typed, the label **Help
  Items**, an empty bag and the highlighted **Add Item** button. The button is a drop-down list because the bag holds
  two content types; with one type it would read "Add " followed by the type's name (line 73).
- `edit-3` (about 12 s). Click **Add Item** (`.btn-bag-add button.dropdown-toggle`); the list shows **Question** and
  **Tip** (`FL/Views/BagPart.Edit.cshtml` line 94; selector `.btn-bag-add a.dropdown-item`). Click **Question**. The
  card is fetched from the server and appended to the bag, so wait until `.widget-editor` has a count of 1 before typing
  (`FL/Assets/ts/flows.edit.ts` lines 54 to 58). The card (`FL/Views/ContentCard-BagPart.Edit.cshtml` line 24) has a
  header with the text "Question" in grey, a move handle, an arrow button, and a red trash button, and a body with
  the fields **Title** and **Answer** (the labels come from the seed: `TitlePart.Edit.cshtml` line 12 and the field
  display name `Answer`). Type `How do I reset my password?` into the card's **Title** (selector
  `.widget-editor >> nth=0 >> .widget-editor-body input.form-control >> nth=0`) and `Use Forgot password on the sign-in page.` into its **Answer**
  (`.widget-editor >> nth=0 >> .widget-editor-body input.form-control >> nth=1`). The card also holds hidden inputs (`FL/Views/BagPart.Fields.Edit.cshtml`), which the class `form-control` leaves out; never type into a bare `input` of a card. *Visible at the end:* one card with both fields filled. The header of the card does not
  repeat the typed title while it is being typed: the script that would update it looks for markup that this card does
  not have (`FL/Assets/ts/flows.edit.ts` lines 155 to 168), so nothing in the narration refers to the header text.
- `edit-4` (about 12 s). Click **Add Item**, then **Tip**. Wait for `.widget-editor` to have a count of 2; the new card
  is appended after the Question card. Type `Keep your contact details current` into its **Title** (`.widget-editor >> nth=1
  >> .widget-editor-body input.form-control >> nth=0`) and `Review them twice a year.` into its **Details** (`.widget-editor >> nth=1 >> .widget-editor-body input.form-control >> nth=1`). *Visible at the end:* two
  cards, Question first and Tip second, both filled in. Scroll only as much as needed to show both cards and the
  **Add Item** button.
- `edit-5` (about 5 s). Click **Publish** (`Contents/Views/Content.PublishButton.cshtml` line 19, the main button of the split
  button; selector `button.publish`) with `ClickAndWaitForNavigationAsync`. The page returns to the Content Items list
  (`Contents/Controllers/AdminController.cs` line 797, the `returnUrl` set by the list). *Visible at the end:* the page
  **Manage Content** with the green message `Your Help Page has been published.` (line 473) and, in the list, a row **Account
  Help** with the buttons **Edit** and **View** (`Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml` lines 13 and 18).

## Clip `04-manage`

Starts on `/Admin/Contents/ContentItems`, opened before the capture, with the row **Account Help** visible.

- `manage-1` (about 10 s). Click **Edit** in the row **Account Help** (selector
  `li:has-text("Account Help") a.btn.edit`, or the equivalent row element of the list layout in use) with
  `ClickAndWaitForNavigationAsync`. The editor opens with the page **Title** and the bag **Help Items**; both cards are
  collapsed, because the setting was ticked in `add-6` and the page loads saved items (not just-added ones) collapsed
  (`FL/Views/ContentCard-BagPart.Edit.cshtml` line 16). A collapsed card shows only its header: the item's title, the type
  name in grey, the handle, the arrow button, and the red trash button. Highlight the arrow button of the first card
  (selector `.widget-editor >> nth=0 >> .widget-editor-btn-expand`) and click it. *Visible at the end:* the Question card
  open, with its Title and Answer, the Tip card still collapsed. The arrow button toggles the class `collapsed` on its
  card (`FL/Assets/ts/flows.edit.ts` lines 148 to 152).
- `manage-2` (about 10 s). Highlight the move handle of the Tip card (selector
  `.widget-editor >> nth=1 >> .widget-editor-handle`, the cross-arrows icon at the left of the header,
  `ContentCard-BagPart.Edit.cshtml` line 28). Drag it above the Question card with `DragToAsync(from, to, 0, -10)`,
  where `to` is the header of the Question card (`.widget-editor >> nth=0 >> .widget-editor-header`). The bag is a
  sortable list that moves cards by this handle (`FL/wwwroot/Scripts/flows/flows.edit.js`, the call to
  `Sortable.create` with `handle:".widget-editor-handle"`). The list refuses a move when the content type of the card is
  not in the list of types of the bag (`FL/Assets/ts/flows.edit.ts` lines 236 to 242), and that list is made of display
  names (`FL/Views/BagPart.Edit.cshtml` line 14): the seed keeps display name and technical name equal, so the move is
  accepted. If the drag does not move the card, drive it with
  `Page.Mouse` (down on the handle, several small moves, up) and look at the contact sheet. *Visible at the end:* the
  Tip card above the Question card.
- `manage-3` (about 6 s). Highlight the red trash button of the Tip card (selector
  `.widget-editor >> nth=0 >> .widget-delete`, `ContentCard-BagPart.Edit.cshtml` line 37) and click it. A dialog titled
  **Delete** appears with the text "Are you sure you want to remove this item?" and the buttons **Ok** and **Cancel**
  (`ContentCard-BagPart.Edit.cshtml` line 37 for the title and the message; the button texts come from
  `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml` line 99). Hold about two seconds, then click **Cancel**
  (selector `#modalCancelButton`). *Visible at the end:* the dialog gone and both cards still in the bag. Do not click
  **Ok**: the removal would apply when the page is published.
- `manage-4` (about 4 s). Click **Publish** (`button.publish`) with `ClickAndWaitForNavigationAsync`. *Visible at the end:*
  the Content Items list with the green message `Your Help Page has been published.`.

## Clip `05-view`

Starts on `/Admin/Contents/ContentItems`, opened before the capture, with the row **Account Help** visible.

- `view-1` (about 14 s). Click **View** in the row **Account Help** (`Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml`
  line 18; selector `li:has-text("Account Help") a.btn.view`). The link has `target="_blank"`, so the page opens in a new
  tab: capture it with `context.RunAndWaitForPageAsync` and `scene.SwitchToAsync(page)`. Wait for the load to finish. The
  page is the published content item at its default address (the type has no Autoroute part, so there is no friendly
  URL). Hold on it. *Visible when the narration ends:* the site's page, with **Account Help** as the heading of the page,
  and below it the two items of the bag, **Keep your contact details current** (the Tip) above **How do I reset my
  password?** (the Question), each with its text. Nothing in the take may click a title: each item's title is
  rendered as a link (`src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Summary.cshtml` line 3) whose target is
  not verified (see OPEN-QUESTIONS.md). The items are rendered with the display type Summary because **Display Type** was
  left empty (`FL/Views/BagPart.cshtml` line 9); with `TheBlogTheme`, the item shape is the Contents module's
  `Content.Summary.cshtml`, not the theme's `Content.liquid`, which is why the Detail masthead does not repeat for every
  item. If a masthead image appears for each item, **Display Type** holds a value: restore S3 and check `add-6`.

## Clip `06-blocks`

Starts on the type page of Help Page, `/Admin/ContentTypes/Edit/<technical name of Help Page>`
(`CT/Controllers/AdminController.cs` line 167), opened before the capture; reach it from the Content Types list if the
technical name is not known. The page title is **Edit Content Type - Help Page** and the **Parts** list holds **Title** and **Help Items**.

- `blocks-1` (about 13 s). Click **Edit** on the row **Help Items** (`CT/Views/Admin/Edit.cshtml` line 163; selector
  `#parts li:has-text("Help Items") a:text-is("Edit")`) with `ClickAndWaitForNavigationAsync`. On **Edit Part - Help Items**
  scroll to the label "What type of editor should be used?" (`CT/Views/Admin/EditTypePart.cshtml` line 70) and highlight
  its drop-down list, which shows **Standard**. Choose **Blocks** with `SelectAsync(select, "Blocks")` (selector
  `select[data-type-part-editor-select]`; the entries are **Standard** and **Blocks**:
  `FL/Views/BagPart.Option.cshtml` line 4 and `FL/Views/BagPart-Blocks.Option.cshtml` line 4). Two fields appear below it:
  **Add Button Text**, with the grey placeholder `Add Block`, and **Modal Title Text**, with the grey placeholder `Select
  Block` (`FL/Views/BagPartBlocksEditorSettings.Edit.cshtml` lines 5, 7, 13 and 15; selectors `input[name$="AddButtonText"]`
  and `input[name$="ModalTitleText"]`). The container is hidden until **Blocks** is chosen
  (`CT/Views/Admin/EditTypePart.cshtml` line 84, `style="display: none"`, switched by the page script). Highlight both
  fields. *Visible at the end:* the drop-down showing **Blocks** and both fields, empty, with their placeholders.
- `blocks-2` (about 6 s). Leave both fields empty. Click **Save** (`button.save`) with `ClickAndWaitForNavigationAsync`.
  *Visible at the end:* the page **Edit Content Type - Help Page** with the message `The "Help Items" part settings have been
  saved.`, and under **Parts** the row **Help Items** now showing a small badge with the text "Blocks"
  (`CT/Views/Admin/Edit.cshtml` lines 187 to 189, the badge titled **Editor**).
- `blocks-3` (about 12 s). Go to `/Admin/Contents/ContentItems` with `scene.NavigateAsync`, and click **Edit** in the row
  **Account Help** (same selector as `manage-1`). The bag **Help Items** now has a button reading **Add Block**, with a plus
  icon (`FL/Views/BagPart-Blocks.Edit.cshtml` line 100; selector `button.bagpart-blocks-picker-button`), and the two saved
  cards are collapsed, Tip first. Highlight the button and click it. A dialog opens with the title **Select Block**
  (`FL/Views/BagPart-Blocks.Edit.cshtml` line 129), a **Search** box and a side list with the entry **All** at the left,
  and one card for each of the two content types (`FL/Views/ContentTypePicker.cshtml` lines 10 to 43), each with its name
  (**Question**, **Tip**), its description ("A question with its answer.", "A short piece of advice.") and a button **Add**.
  Hold on the open dialog for about two seconds, then click **Add** on the **Question** card (selector
  `#contentTypePickerModal .content-type-card:has-text("Question") button`). The dialog closes and a third card (an empty
  Question) appears at the end of the bag (`FL/Assets/ts/content-type-picker.ts` lines 268 and 273, which insert the card
  and then hide the dialog). *Visible at the end:* the dialog closed and the new card at the end of the bag, below the two
  collapsed ones. Do not publish: the clip ends with the editor open, and the added card is never saved.

## Checks for private data

- The only person on screen is the user `admin` (`admin@example.com`). The content items of the Blog recipe (their
  titles are in `blog.recipe.json`) are invented, and no other record is shown.
- The typed texts are the ones in this plan: Help Page, Help Items, Questions and tips., Account Help, "How do I reset
  my password?", "Use Forgot password on the sign-in page.", "Keep your contact details current", "Review them twice a
  year." None names a person, a company, an address or a number.
- No password is typed on camera and none appears in a field. The recorder signs in outside the capture
  (`state.json`).
- The address bar and the tab titles show `localhost` only. If the chosen browser shows the address, check that it holds
  no machine name, user name or folder path.
- After assembling, read the contact sheets of the five clips (`shots/<clip>.jpg`) before the OCR scan: no error
  message, empty list or half-loaded card at the end of any step.

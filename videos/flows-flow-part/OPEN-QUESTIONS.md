# Open questions: Flow part

The narration of this video states only what the Orchard Core source at commit `92db4a83`, its recipes and its
documentation show. Nothing here was seen on a running site. The person who records the video checks each point on the
first take, writes the answer under it, and changes the recording plan (or tells the author to change the narration)
when the site behaves differently.

## 1. Moving a widget by its handle (`build-8`)

- **Read:** each widget card has a handle (`OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:34`), and the Flow
  editor starts a sortable list for its cards (`Assets/ts/flow-part.ts`, `Assets/ts/flows.edit.ts`,
  `initFlowSortableWidgets`). The sortable component itself (`initSortableWidgets`) lives in the `@orchardcore/bloom`
  package, which is not part of the source tree this plan was written from.
- **To confirm:** that `scene.DragToAsync` from `.widget-editor-handle` of the Blockquote card to the left half of the
  Paragraph card's header reorders the cards in the headless browser, and that the two 50% cards then show the quote
  on the left.
- **If it does not work:** try `DragAsync` to explicit coordinates; if no drag works, remove the step `build-8` from
  the storyboard and the plan (the narration claims that a widget is dragged by its handle), and tell the author.

## 2. Controls that appear on hover (`build-5`, `build-6`, `build-9`)

- **Read:** the size, alignment, toggle and insert buttons of a widget are in `.btn-widget-metadata`, which is
  `visibility: collapse` until the cursor is over `.widget-editor-header` (`Assets/scss/flows.edit.scss`). The narration
  says "Hover over a widget's header to see its controls."
- **To confirm:** that `MoveToAsync` on the header makes the buttons visible and clickable (Playwright waits for
  visibility), that they stay visible while the cursor moves from the header to a button that sits above the card's top
  edge, and that the 0.3 s transition does not leave the controls half visible in the frame at the end of the step.

## 3. The rich text area of the Paragraph widget (`build-3`)

- **Read:** the Paragraph widget's field uses the WYSIWYG editor (`blog.recipe.json:759-772`), which the Flow editor
  loads through `OrchardCore.ContentFields/Views/HtmlField-Wysiwyg.Edit.cshtml` and
  `Assets/ts/html-field-wysiwyg.ts` (an editor from the `@orchardcore/bloom` package).
- **To confirm:** the selector of the area to type in (expected `.trumbowyg-editor` inside the card), that
  `PressSequentiallyAsync` types into it, and that the text appears in the frame. If typing does not work, fill the
  area before the capture and keep the step's actions to the clicks.

## 4. Pronunciation

The voice has not been run on this text. A person confirms, with `scripts/pronounce.py`:

- "Raw Html" (as the type is named), which the storyboard maps to "Raw H.T.M.L." through `spoken` (a mixed-case "Html" may be read as a word), "Blockquote", "Permalink", "Hillside Bakery";
- "Add & Configure", which the storyboard maps to "Add and Configure" through `spoken`;
- "50%" and "100%" (expected: fifty percent, one hundred percent), and "25% to 100%" in `why-3`;
- "Ok" (the label of the button in `build-10`), and "Inherit" (`build-6`).

## 5. The admin screens of this version (`build-1`, `blocks-2`)

- **Read:** the **New** button with the creatable types is `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml`;
  the **Edit** and **View** buttons of a row are `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml`.
  The content items list can be drawn as a list or as a grid (`TheAdmin`), which this plan did not read.
- **To confirm:** the **New** button is present with the Blog recipe (Article, Blog and Page are creatable, so a list
  of types is expected, not a single "New Page" button), the **Content** menu group opens by a click, and the row of the
  page keeps an **Edit** button in the layout the admin uses.

## 6. The way to the Add Parts list of the Article type (`part-1`, `part-2`)

- **Read:** the path **Design**, **Content Definition**, **Content Types** is the menu of the Content Types module
  (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`); the **Edit** button of a type is
  `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`; the **Add Parts** button is `Views/Admin/Edit.cshtml:201`.
  `AddPartsTo` lists the attachable parts that the type does not have yet
  (`OrchardCore.ContentTypes/Controllers/AdminController.cs:255-282`); the Flow part is attachable
  (`OrchardCore.Flows/Migrations.cs:19-21`), and the Article type of the Blog recipe has no Flow part
  (`blog.recipe.json:131-186`).
- **To confirm:** that the sidebar of the Blog recipe opens **Design** and **Content Definition** by a click and shows
  one **Content Types** link, that the **Edit** button of the Article row needs no scroll, that **Flow** is in the Add
  Parts list for Article with its description, and where it is in the list (the list is long; the row may need a scroll
  before the highlight).

## 7. The page on the site (`site-1`)

- **Read:** the Blog theme makes the flow a row and gives `widget-size-50` half the width from 576 px
  (`wwwroot/css/bootstrap-oc.css` of `TheBlogTheme`), and `Widget-Paragraph.liquid` and `Widget-Blockquote.liquid`
  write the widget's classes (size, alignment).
- **To confirm:** at 1536 px wide the quote is on the left and the paragraph on the right, each half the width, the
  quote centered; and that the address of the page is `/hillside-bakery`.

## 8. A Container widget with its own Flow part (`need-2`)

- **Read:** the Container type of the Blog recipe has a Flow part (`blog.recipe.json:320-349`), and the Flow editor
  handles nested flows (`retargetFieldNames` in `Assets/ts/flows.edit.ts`). The video says that widgets can hold other
  widgets, and does not show it.
- **To confirm:** on a running site, add a Container widget to a page, then a Paragraph inside it, publish, and check
  the front end. If the nested editor or the front end does not work, tell the author to remove the sentence.

## 9. The Blocks editor (`blocks-1` to `blocks-4`)

- **Read:** the list "What type of editor should be used?" offers **Standard** and **Blocks** for the Flow part; with
  **Blocks** the plus button opens the dialog **Select Widget** (`FlowPart-Blocks.Edit.cshtml`), whose sidebar lists
  **All** and the categories of the types, and whose cards show the thumbnail and the description when the type has
  them (`ContentTypePicker.cshtml`, `content-type-picker.ts`).
- **To confirm:** the dialog opens with the five cards and the **All** category only, and that a search for `quote`
  leaves the Blockquote card (`blocks-4`). The Blog recipe sets no category and no thumbnail on its widget types, and its
  `ContentDefinition` step does not apply the `ContentTypesOptions` defaults (`RecipeSteps/ContentDefinitionStep.cs:52-67`;
  `Services/ContentDefinitionService.cs:90-91` applies them only to a type created in the admin), so the video does not
  show them; the sentence about them in `blocks-3` ("can illustrate", "gets both from the Category and Thumbnail
  settings") rests on the source and on the ContentTypes docs, and the step shows the dialog with only **All**.

## 10. The sidebar of the Blog recipe (`build-1`)

- **Read:** the Blog recipe adds a custom admin menu (the `AdminMenu` step of `blog.recipe.json`) with a **Content**
  group that holds **Content Items** and entries for the Article, Page and Taxonomy types. The navigation manager merges
  menu items that have the same caption (`src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs`, `Merge`),
  so the sidebar is expected to show one **Content** group and one **Content Items** link, with those extra entries.
- **To confirm:** that exactly one link reads **Content Items** (a locator that matches two links stops the clip), and
  what the **Content** group shows when it is open. Write the locator that worked in the plan.

## 11. The order of the cards in the Select Widget dialog (`blocks-2`)

- **Read:** the cards come from the widget types in the order the content definition lists them
  (`ListWidgetTypeDefinitionsAsync` does not sort, `FlowPart-Blocks.Edit.cshtml` serializes them as they come), unlike
  the plus menu of the Standard editor and the **Restrict Content Types** boxes, which are sorted by display name.
- **To confirm:** the order of the five cards on a site set up with the Blog recipe. The narration names no order, so
  nothing in the video depends on it.

## 12. The header text of a new widget card (`build-3`, `build-4`)

- **Read:** the header text of a card is the widget's display text followed by its type name
  (`ContentCard-FlowPart.Edit.cshtml:38`); a widget that was just added has no display text, so the header is expected
  to read **Paragraph** or **Blockquote**, with a leading space.
- **To confirm:** the header text on screen. The narration only names the widgets by their type names.

## 13. The Permalink of a new page (`build-2`)

- **Read:** the Page type has an Autoroute pattern and allows a custom path (`blog.recipe.json:473-474`), so the
  Permalink field shows the hint "Leave empty to auto-generate it." (`OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:46`),
  and the script of the editor does not fill the field from the title (`Assets/ts/autoroute-part.ts`). The narration
  says the Permalink is generated from the title when it is left empty.
- **To confirm:** that the field is empty while the title is typed, and that the published page is at
  `/hillside-bakery` (needed by `04-site`).

## 14. The path to Features (`need-1`)

- **Read:** the narration and the slide of `need-1` say that features are enabled under **Tools**, **Features**
  (`OrchardCore.Features/AdminMenu.cs:26-27`). The video never opens that screen.
- **To confirm:** that the sidebar of a site set up with the Blog recipe shows **Tools** with **Features** in it.

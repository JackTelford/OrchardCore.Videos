# Open questions: Widgets overview

Everything in the narration was read from the Orchard Core source (commit 92db4a83) and the Widgets documentation. The
points below could not be settled from the source alone. Check them on the running site before recording, and change the
narration step named in each question if the site disagrees. Paths are relative to
`src/OrchardCore.Modules/` unless they start with `src/`.

## 1. The on-screen name of the part is "WidgetsList"

- **Checked:** the migration of `OrchardCore.Widgets/Migrations.cs:16-23` creates the part `WidgetsListPart` with a description
  and no display name. `ContentPartSettingsExtensions.DisplayName` (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-63`)
  then returns the name without its suffix `Part`, and `EditPartViewModel` (`OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:19-31`)
  keeps that value, so the CamelFriendly fallback on its next line never applies. The same name is read by the Add Parts
  list (`OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:33`), the Parts list of the type (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:153`), the page "Edit Part - ..." and the section of
  the content editor (`OrchardCore.Contents/Views/ContentPart-WidgetsListPart.Edit.cshtml:23`).
- **To confirm:** the label reads exactly `WidgetsList` (one word, no space) on those four screens.
- **If it differs:** change the word in the narration of `idea-3`, `allow-1`, `allow-2`, `allow-4`, `add-1`, `manage-1` and `outro`, in the
  slides (`slides.py`, the card "The WidgetsList part"), in `video.json`, in `README.md`, and in the `spoken` entry of
  `storyboard.json`.

## 2. A widget card that was just added opens collapsed

- **Checked:** `OrchardCore.Widgets/Controllers/AdminController.cs:67` builds the card of a widget added with the plus button
  with `Inline = true`, and `ContentCard-WidgetsListPart.Edit.cshtml:15` adds the class `collapsed` for it, which hides the
  card body (`Assets/scss/widgetslist.edit.scss`). Cards that were saved render open (`WidgetsListPart.Edit.cshtml:56`).
- **To confirm:** after choosing Paragraph, the new card shows only its header, and the right-arrow button opens it.
- **If the card opens by itself:** remove "Expand it, and" from the narration of `add-3` and the expand click from the plan.
- **Also to confirm:** the Wysiwyg editor of the new card starts after the card is inserted. The source says it does:
  `OrchardCore.ContentFields/Assets/ts/html-field-wysiwyg.ts:5` registers the editor with `observeAndInit`, whose compiled form
  uses a `MutationObserver`, so `.trumbowyg-editor` should appear inside the card within a moment. If it does not appear, the
  step cannot be recorded as written: reload the page, add the Paragraph again, and tell the maintainer.

## 3. Where the widgets appear on the front end

- **Checked:** `WidgetsListPartDisplayDriver.DisplayAsync` (`OrchardCore.Widgets/Drivers/WidgetsListPartDisplayDriver.cs:42-86`)
  adds each widget to the named zone of the layout, in the detail view only. TheBlogTheme renders the zones Header, Messages,
  Content (the page body) and Footer (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:35-47`). The Blog recipe
  already puts a widget in the Footer zone on the layer Always, with the position 10 (`blog.recipe.json:1088-1104`).
- **Not decided by the source:** whether a widget of the Content zone shows above or below the text of the article, and
  whether the paragraph of the Footer zone shows above or below the copyright widget of the layer.
- **To confirm:** open `/about` after `add-5`, look at both. The narration of `add-6` only says "in the Content zone" and
  "in the footer", which holds either way; if the quote does not show in the article area at all, replace the Content zone by
  another zone in `allow-3`, `add-1`, `add-4` and `add-6`.

## 4. The list of widget types of a Blog site

- **Checked:** the Blog recipe defines Container, Blockquote, Image, Paragraph and RawHtml (display name "Raw Html") with the
  stereotype Widget (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:320-440`). The recipe enables no other module
  that defines a widget type: the modules that do (Forms, ReCaptcha forms, Search, Facebook) are not in its list of features.
- **Why the search finds them:** the row's `data-filter-value` is the display name only
  (`OrchardCore.ContentTypes/Drivers/ContentTypeEntryDisplayDriver.cs:19`). The stereotype matches because the List layout wraps
  the row's content and tags in one `.list-item-search-text` element (`OrchardCore.ContentTypes/Views/ContentTypeEntry.SummaryAdmin.cshtml:14`)
  and the search reads the text of such elements (`OrchardCore.Resources/Assets/js/list-management.js:228-240`). The List layout is
  the default (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`); in the Grid layout the badge is not searched.
- **To confirm:** searching the Content Types list for `Widget` leaves exactly those five rows, each with a badge reading
  Widget, and the Add Widget dropdown lists the same five in that order (sorted by display name:
  `OrchardCore.Widgets/Views/WidgetsListPart.Edit.cshtml:86`).
- **If the list differs:** change the names in the narration of `types-2` and the card text of `idea-1` in `slides.py`.

## 5. Dragging a card into another zone

- **Checked:** the cards are sortable across zones, and the zone field of a moved card is updated
  (`OrchardCore.Widgets/Assets/ts/widgetslist.edit.ts`, `initWidgetsListSortable`, `onReparented`; the comment there says any
  widget can move into any zone).
- **To confirm:** the recorder's `DragToAsync` moves the Blockquote card from the Content zone into the Footer zone with the
  library behind the editor (SortableJS). If it does not move the card, try `DragToAsync` with the target set to the body of the
  Footer zone, then to the empty space under the Paragraph card. If no drag works, record `manage-1` with the card dragged
  inside its own zone (two cards in the Footer zone, swapped) and change "or to move it into another zone" in the narration to
  the claim that was shown.

## 6. Selectors and labels shown only as tooltips

- The plus buttons **Add Widget** and **Insert Widget** and the trash button have no text: the first two only a `title`
  (`WidgetsListPart.Edit.cshtml:82`, `ContentCard-WidgetsListPart.Edit.cshtml:28`), the trash button not even that. A headless
  capture does not draw a tooltip, so the plan highlights them with a caption. Confirm that the caption is readable on the
  frame at the end of `add-2` and `manage-3`.
- Every selector of `recording-plan.md` (`#search-box`, `.list-group-item`, `a.edit-button`, `.widget-list-zone`,
  `.widget-editor`, `.widget-editor-btn-expand`, `.trumbowyg-editor`, `.widget-list-delete`, `#modalOkButton`, ...) was read
  from the views and scripts, not tried. Run `Recorder probe` on each page. The Content Types list may use the Grid layout
  instead of the List layout (`OrchardCore.ContentTypes/Views/Admin/List.cshtml:30-35`): then the rows are table rows, not
  `.list-group-item`, and the search for `Widget` in `types-2` finds no row (see question 4). The site is set up with the
  default, List.
- Do not select the stereotype badge (or any other badge or button that carries `data-bs-toggle="tooltip"`) by its `title`
  attribute: TheAdmin turns each such element into a Bootstrap tooltip when the page loads
  (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:190-193`), and Bootstrap moves the `title` into
  `data-bs-original-title` and removes it (`src/OrchardCore.Modules/OrchardCore.Resources/wwwroot/Scripts/bootstrap.js:3555-3565`). The plan selects the badge by its text. The plus buttons **Add Widget** and **Insert Widget** open
  a dropdown instead (`data-bs-toggle="dropdown"`), so their `title` stays; confirm it with the probe.
- The Paragraph widget uses a Wysiwyg editor; the About article's own body is also a Wysiwyg editor. Scope the editor
  locator to the card of the widget, or the typing goes into the article body.

## 7. Settings are kept without a click on Save in the type page

- **Checked:** `AdminController.EditTypePartPOST` (`OrchardCore.ContentTypes/Controllers/AdminController.cs:996-1087`) stores the
  zones with `AlterTypePartAsync` and redirects, and the attach of `Add & Configure` is stored by `AddPartToTypeAsync`
  (`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:143-156`).
- **To confirm:** after `allow-3`, reload the Article type and the About article: the section WidgetsList shows the cards
  Content and Footer without any further save.

## 8. Pronunciation

- `storyboard.json` has four `spoken` entries: `WidgetsList` as "Widgets List", `Add & Configure` as "Add and Configure",
  `Raw Html` as "Raw H.T.M.L." (`types-2`) and `Wysiwyg` as "Wizzy wig" (`types-4`). The last two are new spellings that no
  one has heard yet: generate a sample of each with `scripts/pronounce.py` before narrating, and drop an entry if the voice
  reads the written form correctly without it (or change it if the spelling is read wrongly).
- Also listen to "stereotype" and "Blockquote" in the sample, and add a `spoken` entry if the voice reads them wrongly.

## 9. Menu path of the first clips

- `types-1` clicks Design, Content Definition, Content Types (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`), and `feature-1`
  clicks Tools, Features (`OrchardCore.Features/AdminMenu.cs:26-27`). The Blog recipe also adds an admin menu of its own
  with an entry Content, Content Items (`blog.recipe.json:1188-1260`); it does not add Design or Tools.
- **To confirm:** each of the four names appears once in the admin menu of the recorded site, and the three clicks of
  `types-1` fit in the 15 words of its narration (about 6 seconds). If they do not, open the group Design before the capture
  and click only Content Definition and Content Types on camera, and say so in `recording-plan.md`.
- The narration of `add-1` names two labels: the menu entry **Content Items** (group **Content**, `OrchardCore.Contents/AdminMenu.cs:65-68`)
  and the title **Manage Content** of the page it opens (`OrchardCore.Contents/Views/Admin/List.cshtml:17`). The Blog recipe's
  own entry has the same captions and is merged with the built-in one (`src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74-150`).
  Confirm that the page shows the title **Manage Content** and that the menu shows **Content Items** once.

## 10. Rows that stay in the Features list

- **Checked:** the search of the Features page reads the name, the id and the description of each feature
  (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), and the list holds disabled features as well. A search for
  `Widgets` therefore leaves more than the row Widgets: the descriptions of Layers, Flows, Forms, Admin Dashboard and the Meta
  widgets feature contain the word. The narration of `feature-1` does not count rows, and `recording-plan.md` finds the row by
  its id `OrchardCore.Widgets` and by the button `#btn-disable-OrchardCore_Widgets`.
- **To confirm:** the row Widgets is visible on the page without scrolling after the search (the page is grouped by category
  and Widgets is in the category Content, `OrchardCore.Widgets/Manifest.cs:15`), and the button id is the one written above.
- **If the row is out of sight:** scroll to it before `feature-1` ends, and say so in `recording-plan.md`.

## 11. What an anonymous visitor sees

- **Checked:** the Blog widget types are Securable (`blog.recipe.json:320-440`), and the display driver shows a securable widget
  only to a user who may view it (`OrchardCore.Widgets/Drivers/WidgetsListPartDisplayDriver.cs:67`). The permission of such a type
  is implied by the global View content permission (`src/OrchardCore/OrchardCore.Contents.Core/ContentTypePermissionsHelper.cs:19`),
  which the default Anonymous role holds (`OrchardCore.Contents/Permissions.cs:91-95`). The recording is made signed in as
  admin, so the front end of `add-6` is the page an administrator sees.
- **To confirm:** open `/about` in a private window after `add-5`. The paragraph is in the footer, as the narration of `add-3`
  ("the text that visitors will read") and `add-6` say. If it is missing, change "visitors" in `add-3`.

## 12. The name of the theme

- **Checked:** the manifest of the theme names it "The Blog Theme" (`src/OrchardCore.Themes/TheBlogTheme/Manifest.cs:5`), and
  the Blog recipe sets it as the site theme (`blog.recipe.json:83-86`). The narration of `idea-2` and the card "Named zones" of
  its slide write the name the way the manifest does.
- **To confirm:** the page Design, Themes lists "The Blog Theme" as the current site theme. If the list shows another name,
  change `idea-2` and the card text in `slides.py`.

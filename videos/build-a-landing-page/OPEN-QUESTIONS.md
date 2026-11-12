# Open questions: Build a Landing Page With Flows, Widgets and Layers

Everything in the narration was read from the Orchard Core source (the commit in `catalog/SOURCE.md`) and its
documentation. The points below could not be seen on a running site. Each one says what was checked in the source and what
a person must confirm while recording. None of them is stated in the narration as more than the source supports.

## 1. How the voice reads some words

The storyboard's `spoken` map is empty on purpose: no spelling below could be heard while the content was written, and the
skill asks a person to confirm each one first. Candidates to test with `scripts/pronounce.py` before narrating. Add to
`spoken` only the ones that fix a misreading:

- `Url` as `U.R.L.` (the label is "Url", and the voice may say "earl"). It appears in `layer-5`, `layer-6` and `layer-7`;
  as a sentence ends with "Url and more" in `layer-5`, check that the extra period of `U.R.L.` does not add a pause.
- `Html` as `H.T.M.L.` (the widget type "Raw Html" in `page-2`).
- `/spring-workshop` as `slash spring workshop`, and `spring-workshop` as `spring workshop` (`page-1`, `layer-6`). List the
  longer one first in the map, since the replacement is a plain text replacement in the order of the map.

The captions keep the written form in any case.

## 2. Dragging in the Flow editor and on Widgets and Layers (`page-6`, `widget-5`)

- Checked: both lists are sortable with the handle named in `recording-plan.md`
  (`OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:34`, `OrchardCore.Layers/wwwroot/Scripts/layers-index/layers-index.js`, which saves the
  new position by a request as soon as the drop ends and then shows "Widget successfully moved. Undo.").
- To confirm: that `scene.DragToAsync` drops the item in headless Chromium (the lists use a drag library). The plan gives
  one retry and then asks to report the video as blocked, because the narration of `page-6` and `page-8` assumes the new order.

## 3. Two blocks side by side, and the centered quote (`page-4`, `page-5`, `page-8`)

- Checked: in the editor, the size choice sets the card's column class (`OrchardCore.Flows/Assets/ts/flows.edit.ts:127-136`).
  On the front end, the Flow renders `section.flow` with the classes `widget-size-50` and `widget-align-center`
  (`OrchardCore.Flows/Views/FlowPart.cshtml`), and The Blog Theme makes `.flow` a row and gives `widget-size-50` half of it
  from the small breakpoint up (`TheBlogTheme/Assets/TheBlogTheme/src/scss/modules/_flow.scss`, `_widgets.scss`).
- To confirm: at 1536 px the two blocks really sit on one row, the quote is centered in its half, and the header text of
  the Page (`TheBlogTheme/Views/Content.liquid`) shows the title Spring Workshop.

## 4. The permalink (`page-1`, `page-8`)

- Checked: the Page type allows a custom path (`AllowCustomPath`, `blog.recipe.json:473`), and the Url condition compares the
  request path, case-insensitively, with the value (`OrchardCore.Rules/Services/UrlConditionEvaluator.cs`).
- To confirm: after Publish, `/spring-workshop` opens the page, so that the Url condition of `layer-6` matches it.

## 5. The label of the Flow editor (`page-2`)

- Checked: the part is attached to Page without a display name, so the editor shows the part name without "Part", which is
  "Flow", followed by the part description (`ContentPartSettingsExtensions.cs:60`, `OrchardCore.Flows/Migrations.cs:19-20`).
- To confirm: the label on screen reads "Flow". If it reads differently, change the word in `page-2`, `parts-1` and `choose-1`
  only if the label is spoken there.

## 6. Where the Footer widget appears (`widget-5`, `result-1`)

- Checked: layer widgets are added to the zone of the layout with their position, in the order of the position
  (`OrchardCore.Layers/Services/LayerFilter.cs:157`, `LayerService.GetLayerWidgetsMetadataAsync`), and the Blog theme renders
  the zone Footer inside its `footer` element (`TheBlogTheme/Views/Layout.liquid`). The new widget starts at position 11,
  and a drop above the widget Footer (position 10) saves position 9 (`layers-index.js`).
- To confirm: after the drop, the front end shows the new widget above the social icons of the Footer widget, as `result-1`
  expects. The narration of `result-1` says only that the footer shows the widget, so an order that differs does not make
  it wrong, but the recording plan's "on screen" line should then be corrected.

## 7. The site as a visitor sees it

The clips are recorded signed in as `admin`. The widget types are securable (`"Securable": true` in the recipe) and the
narration never says what a visitor sees. If the video should show an anonymous visit, a person must first check that the
anonymous role may view the Paragraph widget type.

## 8. Menu and button selectors

The selectors in `recording-plan.md` were written from the views, not from a rendered page: the admin menu links by address
(`a[href$='/Layers']`, `a[href$='/Settings/zones']`, `a[href$='/Contents/ContentItems']`), the buttons by their label, and
the editors by the classes the views add. Check each with `Recorder probe` once on the running site, and correct the plan
when the markup differs (the admin menu is the most likely: its groups open from a script, and the admin menu behavior
setting `Focused` may close the group that was open when another one opens,
`OrchardCore.Admin.Abstractions/Models/AdminSettings.cs`).

## 9. The Filter box of the Available Rules dialog (`layer-5`, `layer-6`)

- Checked: the script compares what is typed with the whole text of each card, its title and its description, on `keyup`
  (`OrchardCore.Rules/Assets/ts/conditions-modal.ts:29-51`), so the narration says only that the box "narrows the cards as you
  type". By hand over the thumbnails in `OrchardCore.Rules/Views/Items/*.Fields.Thumbnail.cshtml`, no other card contains "url",
  so typing `Url` should leave the Url condition card alone.
- To confirm: after `Url` is typed, one card is visible. If a second card stays, change the Do line of `layer-6` in
  `recording-plan.md` (the click is `.item:visible a.condition-thumbnail`, which would then match two cards).

## 10. The size and alignment controls have no text label (`page-3` to `page-5`)

- Checked: the alignment control is an icon-only button without a title (`OrchardCore.Flows/Views/FlowMetadata.Edit.cshtml:10-13`),
  and the size control shows the number, such as `100%` (line 35). The narration names them "the alignment menu" and "the size
  menu", which are descriptions, not labels; the options inside are the labels Left, Center, Right, Justify, Inherit and
  25%, 33%, 50%, 66%, 75%, 100% (lines 16-28, 39-54).
- To confirm: on screen the two controls are easy to tell apart in the card header. If not, the recording should highlight each
  one while it is named.

## 11. The rich text editor of the Paragraph widget (`page-3`, `widget-3`)

- Checked: the field Content of Paragraph uses the editor `Wysiwyg` (`blog.recipe.json:769`), which draws Trumbowyg
  (`OrchardCore.ContentFields/Views/HtmlField-Wysiwyg.Edit.cshtml:14-19,43`) on a textarea. The widget editor of a Flow is fetched
  by a script when the widget type is chosen (`OrchardCore.Flows/Assets/ts/flows.edit.ts:23-60`).
- To confirm: `.trumbowyg-editor` appears inside the new Paragraph card, so typing works as the plan says. If the editor is not
  drawn, the step cannot be recorded and the video is blocked.

## 12. Order of the two blocks after the drag (`page-6`, `page-8`)

- Checked: the two cards are `col-md-6` once both are set to 50% (`flows.edit.ts:127-136`), so they stand side by side in the
  editor and the narration of `page-6` says "to the left of", not "above". The saved order is the order of the cards in the page
  when Publish is selected (`FlowPartDisplayDriver.UpdateAsync` follows the posted prefixes).
- To confirm: after the drop the Blockquote is on the left in the editor, and `/spring-workshop` shows it first.

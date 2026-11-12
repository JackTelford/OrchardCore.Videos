# Open questions: Field Editors and Display Modes

Everything in the narration was read in the Orchard Core source at commit 92db4a83 (views, drivers, settings, the
bundled scripts under `wwwroot`, and the Blog recipe). Nothing was run: no site was started for this video. The points
below are what a person must confirm on a running site before recording, with what was checked. Paths are relative to
that checkout; "CT" stands for `src/OrchardCore.Modules/OrchardCore.ContentTypes` and "CF" for
`src/OrchardCore.Modules/OrchardCore.ContentFields`.

## 1. The seed recipe

- Checked: the shape of the recipe follows `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json` (content definition
  with a part named after the type, fields with `ContentPartFieldSettings`, a `Content` step with `AutoroutePart`) and the
  recipe in `src/docs/guides/create-blazor-cms/README.md`. The Predefined List options use the property names `name` and
  `value` of `CF/Settings/TextFieldPredefinedListEditorSettings.cs` (`ListValueOption`).
- The recipe also stores `"Editor": "Dropdown"` for the list editor of Skill Level, so that `list-3` changes it to Radio
  buttons on screen. The names `Dropdown` and `Radio` are the members of `EditorOption`
  (`CF/Settings/TextFieldPredefinedListEditorSettings.cs:12-16`), and the JSON options read enums by name
  (`OrchardCore.Abstractions/Json/JOptions.cs:19`). The step name `Content` and the key `data` are matched without regard to
  case (`NamedRecipeStepHandler.cs:19`, `JOptions.cs:28`); `blog.recipe.json` writes `content` and `Data`.
- To confirm: the recipe imports without an error in Tools, Deployments, JSON Import; the Workshop type lists Headline, Seats
  and Skill Level in that order; `/spring-pottery-evening` shows the Workshop; the Skill Level settings (after choosing the
  Predefined List editor) show the list editor Drop down list and the options Beginner and Intermediate that the seed stores.
  The imported item has no owner and no dates (the seed gives only an author): confirm that the Content Items list and the
  page show it without an error.

## 2. Search on the Content Types page (`editor-1`)

- Checked: `CT/Views/Admin/List.cshtml:9,15` turns on client-side search, and `list-management.js` hides the rows that do
  not match.
- To confirm: typing Workshop leaves exactly one row, with its Edit button, on a Blog site. If another type matches,
  the narration ("narrows the list to that type") must change.

## 3. Defaults of the two drop-down lists

- Checked: the standard Numeric editor is labeled Number (`NumericField.Option.cshtml:4`), the standard Text display mode is
  labeled Standard (`TextField.DisplayOption.cshtml:4`), and the Predefined List editor has the choices Drop down list and
  Radio buttons (`TextFieldPredefinedListEditorSettings.Edit.cshtml:19,20`). Without stored settings the list editor starts as
  Radio (the first member of `EditorOption`), so the seed stores Dropdown and `list-3` has a visible change to make.
- To confirm: Number is selected for Seats before the change (`editor-4`), Standard for Headline (`display-2`), the
  Predefined List settings open with Drop down list selected (`list-2`), and the recorded take shows Radio buttons after
  the pick in `list-3` and radio buttons on the New Workshop page in `list-6`.
- The order of the choices in the editor and display mode drop-down lists is not in the source (it comes from the shape
  table of the active theme), so the narration never states an order or a count, and the slides list Standard first and the
  rest alphabetically.

## 4. The slider (`editor-6`, `editor-7`)

- Checked: `NumericField-Slider.Edit.cshtml` draws the number box with the minimum and the maximum beside it, and a track
  with both under it. The bundled script `numeric-field-slider.js` creates a noUiSlider on the track, writes the box on the
  slider's `change` event and moves the handle on the box's `keyup` event. The library is in the checkout
  (`OrchardCore.Resources/wwwroot/Scripts/nouislider/nouislider.js`, version 15.7.0 in `Assets/package.json`): its handle
  class is `noUi-handle`, and its default format writes two decimals (`toFixed(2)`), which the component does not override,
  so the box reads something like `30.00` after a drag although Scale is 0. The plan and the narration allow for it: the
  narration does not quote the number and the typed `10` replaces it.
- To confirm: the drag with the mouse moves the handle and the box then reads two decimals; typing `10` moves the handle to a
  quarter of the track; the number box is empty on the New Workshop page before the drag.

## 5. The options table (`list-2` to `list-4`)

- Checked: the table is drawn by `options-editor-fields.js` (the source of its component is in the `bloom` package, not in
  the checkout; the bundled copy under `wwwroot` was read). It has the columns Option Label, Value and Default?, a text box per
  cell with the placeholder Enter a name, a remove button and a drag handle per row, and an Add an option link styled as a button
  (an anchor without an address, `btn btn-light`) under the table that appends an empty row. A row whose Value was not edited by hand takes the text of the label as it is typed. The
  Default? column is a radio group; clicking a selected radio clears it.
- To confirm: Add an option can be clicked with the locator of the plan (`a:has-text("Add an option")`); the new row's Value box fills in as
  Advanced is typed; the three options are still listed after Save when the field is opened again (`list-5` does not show
  this, so check it once outside the capture).

## 6. The site (`display-4`)

- Checked: `TextField-Header.Display.cshtml` draws the text in the tag chosen by the level (h2 for H2) with the classes
  `field field-type-textfield field-name-...`, and `TextField.cshtml` draws it in a div. TheBlogTheme's `Content.liquid`
  draws the title and then `Model.Content`, which holds the fields of the Workshop part.
- Also checked: TheBlogTheme has no `placement.json` and no view for the Workshop type or for the fields (its views are
  listed in `src/OrchardCore.Themes/TheBlogTheme/Views`, and only `Content.liquid` applies to a type without its own view),
  so nothing in the theme hides or redraws the fields.
- To confirm: the Workshop page really shows the three fields under the title (site-level placement rules, if any, are
  data and are not in the source), the heading has the selector `h2.field-type-textfield`, and no cached copy of the page
  shows the plain line after the display mode changed.

## 7. Pronunciation

- The storyboard has the entry Url spoken as U.R.L. (`catalog-1`). It follows the convention of the skill but has not been
  checked with `pronounce.py`, which this session did not run. A person must confirm it, and also how the voice says
  Multi-line, Monaco, H2 and Checkbox List.

## 8. What the badges show

- The badges next to a field on the type page print the stored name of the editor or display mode, not its label
  (`CT/Views/Admin/Edit.cshtml:119,123`). The narration reads a badge only for Slider and Header, whose stored names are the
  same as their labels. The badge for Predefined List reads PredefinedList; the narration does not read it.

## 9. The catalog slides (`catalog-1` to `catalog-3`)

- Checked: every name on the three slides is the label of an `*.Option.cshtml` or `*.DisplayOption.cshtml` view of the
  module (`CF/Views`), compared character by character. No other source adds an editor or a display mode to the fields
  named there: a search of the whole checkout for shape methods named `*_Option__*` and `*_DisplayOption__*` finds only
  the Lucene and Elasticsearch editors of the Content Picker field, which the video does not name and which the recording
  site does not enable. The Link Field and the YouTube Field have a display mode list but no editor list.
- Not covered, on purpose: the Html Field (Standard, Multiline, Monaco editor, Trumbowyg editor, Wysiwyg editor), the Content
  Picker field, the Link Field, the Localization Set Content Picker Field, the User Picker Field and the YouTube Field are not
  named in the editors slides. The narration says only what each named field offers, and the display modes slide says
  "every other field" of the module. A person may decide to add the Html Field; its names need a pronunciation check first.
- To confirm: the lists in the editor and display mode drop-down lists on a running site match the slides, in the admin
  theme of the recording site.

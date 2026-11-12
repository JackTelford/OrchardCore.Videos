# Open questions: Taxonomy Field: Categories and Tags on Your Content

Every label in the narration was read in the Orchard Core source at commit `92db4a83` (`src/OrchardCore.Modules`,
`src/OrchardCore.Themes/TheBlogTheme`). The narration states only what the source shows. The points below are what the
source cannot settle: they need a person with a running site (the Blog recipe, TheBlogTheme, `localhost`) before or
while recording. If one of them turns out differently, change the narration of the step named, not the site.

## Behavior to confirm on the running site

1. **The tags editor on screen** (`edit-4`, `edit-5`, `edit-6`). Only the compiled bundle of the editor is in the source
   tree (`OrchardCore.Taxonomies/wwwroot/Scripts/taxonomy-field-tags/taxonomy-field-tags.js`, from the TypeScript that
   imports `@orchardcore/bloom/components/tags-editor`, which is not in the tree). The bundle builds a `vue-multiselect`
   (version 3.2.0, shipped in `OrchardCore.Resources/wwwroot/Scripts/vue-multiselect.js`) with the placeholder
   `Type to search` (or the Placeholder setting), `taggable` when Open is on, the tag hint `Press enter to create a tag`,
   and `close-on-select` equal to the Unique setting. From that source: the empty box shows the placeholder; the list stays
   open after a selection when Unique is off; typed text that is not the exact name of a tag adds an entry that creates it,
   and that entry shows the hint through the library's `data-select` attribute when it is the highlighted entry.
   Confirm on screen: the empty box shows `Type to search`; typing `Expl` lists Exploration and an `Expl` entry; clicking
   Exploration leaves the list open with the chip in the box; typing `Night Train` leaves a single entry that shows
   `Press enter to create a tag` in its highlighted state (the narration of `edit-5` says the box offers it); Enter shows
   the chip `Night Train`. The CSS classes of the plan (`multiselect__placeholder`, `multiselect__input`,
   `multiselect__option`, `multiselect__tag`) are those of the vendored library file; the rendered page was not seen.
2. **The Open checkbox** (`tags-3`, `tags-4`). `TaxonomyFieldTagsEditorSettings.Open` defaults to true
   (`OrchardCore.Taxonomies/Settings/TaxonomyFieldTagsEditorSettings.cs:14`). The settings driver puts the checkbox in the
   `Content` zone of the field settings page, after the settings of the field and above the editor question
   (`TaxonomyFieldTagsEditorSettingsDriver.cs`, `EditField.cshtml:74-79`), and `edit-field-selector.js` shows it only when
   the editor question reads Tags. Confirm that it is checked by default on a field created from scratch, that it appears
   between **Leaves only** and the editor question right after the Tags option is selected, and that the page position
   keeps both it and the question in the frame (`tags-3`).
3. **The field list label** (`category-3`). The Field Type label `Taxonomy Field` is the field's name `TaxonomyField`
   turned into words by `CamelFriendly()` (`OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml:62`). Confirm the text on
   screen. The same call produces the grey `Taxonomy Field` after each field's name in the type editor (`category-7`).
4. **The front end after the fields are added again** (`site-2`). The Blog Post template renders the shapes
   `BlogPost-Category` and `BlogPost-Tags` (`TheBlogTheme/Views/Content-BlogPost.liquid:31-32`). The Tags template applies
   only to the Tags display mode (`BlogPost-Tags-TaxonomyField-Tags.Display.liquid`). Both match only the technical names
   `Category` and `Tags`. Confirm that the post shows a category badge with its icon and a badge for each tag, and that
   the page has no error, in particular for the tag created in the editor: `TagController` creates the tag without
   running the editor of the Tag type, so it has no Title part value, only its display text
   (`OrchardCore.Taxonomies/Controllers/TagController.cs`, "Create tag term but only run content handlers"), and the theme
   links each badge with `display_for`.
5. **The seed icons** (seed step 2 of `recording-plan.md`). Category terms require an icon
   (`blog.recipe.json:793-805`, `Required: true`), chosen with the icon picker (a dropdown whose items are
   `.iconpicker-item`). The library lists `fas fa-train` and `fas fa-utensils` (`fontawesome-iconpicker.js:4848,5055`) and
   the plan names them, so the badge of `Trains` on the site (`site-2`) is the same on every take. Confirm that the
   picker shows them and that `.iconpicker-item[title*='fa-train']` and `.iconpicker-item[title*='fa-utensils']` each match
   one item; the narration does not name the icons. Confirm too that the term form's save button is labeled **Publish**; the Create action of the
   Taxonomies module saves the term into a draft of the taxonomy, so the taxonomy must be published as well, and the
   front end (`taxonomy_terms`) reads the published version.
6. **The order of the fields in the editor** (`edit-2`, `edit-4`). A field added in the type editor takes the next
   position (`ContentDefinitionService.cs:494`), so the editor should show Subtitle, Banner Image, Category, Tags. Confirm
   that, and that Category and Tags are not hidden in a collapsed card.
7. **The Hint in the Standard editor** (`edit-2`). `TaxonomyField.Edit.cshtml:21` renders it with `Hint_Admin` above the
   radio buttons. Confirm where it appears relative to the label.
8. **The disabled radio button** (`edit-3`). With Unique and Leaves only on, the radio of a term that has children is
   `disabled` (`TaxonomyField.Edit.cshtml:83`). Confirm that Travel looks disabled in the admin theme and that the
   highlight ring reads well on it.
9. **The View button** (`site-1`). The source settles the rest: the admin list uses the List layout and shows Edit and
   View as buttons of the row, in the `Actions` zone (`AdminListOptions.cs:30,36`, `ContentsDriver.cs:79`). Confirm that
   View opens the new tab (`target="_blank"`, `ContentsButtonEdit_SummaryAdmin.cshtml:18`) and that `SwitchToAsync` attaches
   to it.
10. **The address of the post** (`site-1`). The path is built from the Autoroute pattern of Blog Post
    (`blog.recipe.json:216`): the container's title and the post's title, slugified. Confirm it is
    `/blog/a-night-train-across-the-alps`.
11. **Timing of `category-3`, `tags-1` and `tags-5`.** Their actions include two page loads, typing and a scroll, in
    about 8 to 9 seconds of narration. If the recorder reports that actions run longer than the narration, shorten the
    typing delay first, and only then change the narration.
12. **The Placements shortcut.** The Blog recipe enables the Placements feature, which adds an **Edit placements**
    drop-down above the settings of a field (`OrchardCore.Placements/Settings/PlacementContentPartFieldDefinitionDisplayDriver.cs`,
    location "Shortcuts"). It is not narrated; check that it does not cover the **Placeholder** and **Hint** inputs.
13. **Pronunciation.** `storyboard.json` has one `spoken` entry, `Save & Configure` as `Save and Configure`, the form every
    other video that names this button uses; confirm with `scripts/pronounce.py` that the voice says it as written.
    `category-6` reads two labels that are questions ("What type of editor should be used?" and "What type of display
    mode should be used?") one after the other: confirm that the voice reads them as the labels on screen and that the
    step stays easy to follow; if it does, change the narration, not the labels. `Leaves only` has no entry:
    confirm that the voice says "leaves" (rhymes with "sleeves"), as it is written in the label.
14. **The Blog entry of the admin menu** (`edit-1`). The step highlights the **Blog** menu entry
    (`GetByRole(AriaRole.Link, Name = "Blog", Exact = true).First`). The entry comes from the AdminMenu step of the Blog
    recipe (`blog.recipe.json:1197`); confirm that the locator finds it at the top level of the menu and that it is visible
    without expanding anything.

## Facts that were checked in the source

- Inline tag creation needs the **Manage taxonomies** permission: the editor turns **Open** off for users without it
  (`TaxonomyField-Tags.Edit.cshtml:22`), and `TagController` answers `Unauthorized()` (`TagController.cs:40`).
- A created tag is added at the root of the taxonomy, and a draftable taxonomy is published at once (`TagController.cs:102-108`),
  before the post is saved. This is why `04-edit` must start from a snapshot without the `Night Train` tag.
- The Tags display mode of the module reads the stored `TagNames` (`DisplayTaxonomyFieldTagsViewModel`), which are written
  when the content item is saved (`TaxonomyFieldTagsDisplayDriver.cs:126`). TheBlogTheme looks the terms up live
  (`taxonomy_terms`), so the video does not say anything about renaming a tag.
- With **Unique** off the Tags editor accepts several tags: the editor is built with `multiple` equal to the opposite of
  Unique (`taxonomy-field-tags.js`), and the settings **Required**, **Unique** and **Leaves only** are those of every
  Taxonomy Field (`TaxonomyFieldSettings.Edit.cshtml:42-70`), so `tags-2` can say they stay off and a post can have any number
  of tags.

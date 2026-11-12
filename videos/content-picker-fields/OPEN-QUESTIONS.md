# Open questions: Content Picker and User Picker Fields

The narration says only what was read in the Orchard Core source (commit 92db4a83) and in the documentation. These are
the points that could not be checked without a running site. Confirm each one on the site before recording, and tell
the author if a step must change. Paths are relative to the root of `OrchardCMS/OrchardCore`.

## 1. The picker component is not in the source tree

The search box of the picker is drawn by a script that imports `@orchardcore/bloom/components/multiselect-picker`
(`src/OrchardCore.Modules/OrchardCore.ContentFields/Assets/ts/vue-multiselect-fields.ts`). That package is not part of the
source that was read, so these points come from the server code only:

- That results appear while typing, without Enter, and without a minimum number of typed characters. Steps `pick-2`,
  `user-4`, `lucene-3`: the narration says "we type pack" and so on. If the picker waits for more characters, type more
  (for the users, `jor` lists jordan only, which loses the point about the role: use another text that morgan, jordan and
  taylor share, and change the narration of `user-4`).
- That a selected item stays in the field and that the field shows several (`pick-3`). The server sends the selected items
  to the picker (`ContentPickerField.Edit.cshtml:29`).
- The classes to use as selectors (the plan guesses the usual `vue-multiselect` classes), and what the picker shows when
  nothing matches (`lucene-4`; the script has a "No result found" string, `Services/ContentFieldsJSLocalizer.cs`).
- The picker also has strings "Not published", "Remove" and "Type to search". None is narrated. If a draft item
  shows "Not published" in a picker, an optional step could mention it.

## 2. The Placeholder setting of the Content Picker Field

The settings page has a Placeholder box (`ContentPickerFieldSettings.Edit.cshtml:74`) and the documentation says it is
the text of the picker input. `ContentPickerField.Edit.cshtml:29` passes no placeholder to the picker, and
`vue-multiselect-fields.ts` passes none for the Content Picker (it does for the User Picker: `UserPickerField.Edit.cshtml:28`,
`data-placeholder`). So the setting has no effect on the Content Picker in the code that was read; this is reported as a
documentation difference. The video does not use the setting. Check on a site whether text typed there ever shows.

## 3. The Elasticsearch editor returns no result

`src/OrchardCore/OrchardCore.Elasticsearch.Core/Providers/ElasticsearchContentPickerResultProvider.cs`, lines 108 to 126:
each hit becomes a `ContentPickerResult`, but nothing adds it to the `results` list that the method returns. Read as it
stands, the picker finds nothing with the Elasticsearch editor. The video therefore shows only the editor option and its
Index setting on a slide (`index-1`), never a search, and does not record it. With an Elasticsearch server
(`src/docs/reference/modules/Elasticsearch/docker-compose.yml`) a person can confirm or refute this, and the slide can then
say more or less.

## 4. The Lucene editor and "Display All Content Types"

`ContentPickerAdminController.cs:64` passes the field's selected content types to the provider, and
`LuceneContentPickerResultProvider.cs:67` filters the index on them. With the source "Display All Content Types" that list
is empty, so the Lucene editor probably returns nothing. The video selects "Content Types" with Product ticked, on
purpose. The Lucene editor also lists drafts only if the index has "Index Latest version" ticked.

## 5. The Title Pattern box

The settings page of the Content Picker Field holds the box "Title Pattern" (`ContentPickerFieldSettings.Edit.cshtml:90`),
whose default text is code. Confirm that the framing in `add-6` and `lucene-1` keeps it out of the frame. The Save button
needs no scrolling: the action bar is `position: fixed` at the bottom of the window
(`OrchardCore.Themes/TheAdmin/Assets/scss/components/_action-bar.scss`), and both Save and Save & Configure stay beside each
other at 1536 px because the default actions layout is Buttons (`AdminListOptions.cs:36`; the secondary buttons move to a
menu only below 768 px or when the admin settings choose the Menu layout: `actionBar.ts`). Confirm that the bar does not
hide the Hint box or the picker list at the framing of `add-6` and `pick-2`.

## 6. The seed recipes have not been run

The two recipes of `recording-plan.md` (Seed 1 and Seed 3) copy the structure of `blog.recipe.json` and
`blog.lucene.search.recipe.json`, but no site was available to run them. Check: that the import ends without errors; that the
Product Bundle editor shows no leftover part; that the pages `/bundles/weekend-camping-bundle` and
`/products/camp-stove` exist; that the index Catalog is created, synchronized and listed on Search, Indexes.

## 7. How the field looks on the page

The pages of the two types use the default content template of TheBlogTheme (`Views/Content.liquid`, which renders all
the shapes). The blog post and article templates of that theme render named shapes only, which is why the video uses its
own types. Confirm that `pick-5` and `user-5` show the field name and the values in a readable way (the code writes a name
and links separated by commas: `ContentPickerField.cshtml`, `UserPickerField.cshtml`), and that the links of the products
work. The order of the links follows the content manager's answer, not necessarily the order picked: the narration does not
promise an order.

## 8. The Lucene editor's settings on the field page

`lucene-1` says the Index setting lists the Lucene indexes. The server code builds the list
(`ContentPickerFieldLuceneEditorSettingsDriver.cs`, `Views/ContentPickerFieldLuceneEditorSettings.Edit.cshtml`), but showing
and hiding it when the editor changes is done by a script of the Bloom package (`OrchardCore.ContentTypes/Assets/js/edit-field-selector.ts`).
Confirm that the Index drop-down list appears when Lucene is chosen. Its id is not `Index`: the settings inputs carry a prefix
(`ProductBundle_Products_ContentPickerFieldLuceneEditorSettingsDriver_Index`), so the plan selects it with `select[id$='_Index']`.

## 9. Pronunciation

Check with `scripts/pronounce.py`: Lucene, Elasticsearch, "Save & Configure" (spoken as "Save and Configure" through the
storyboard's "spoken" entry), "o r" in `user-4`, and the user names jordan, morgan and taylor.

## 10. The Contained Content Types radios and the Stereotypes box

`content-picker-field-settings.ts` shows and hides the Stereotypes box and the content type list when a radio changes, and
it finds the radios with `getElementById("Source")` and `getElementsByName("Source")`. The view writes the ids and names with
the prefix of the settings driver (`ContentPickerFieldSettings.Edit.cshtml:35` uses `Html.IdFor`), so by the code that was read
these lookups find nothing and the script does nothing (`text-field-settings.ts` has a comment about this very problem).
Then the box Stereotypes and the list of content types are both on screen whatever radio is selected, and the Title Pattern
box stays a plain text box (`getElementById("TitlePattern")` also finds nothing). The narration does not depend on either
behavior. Confirm on a site what the page looks like at the end of `add-4` and `add-5`, and keep the frame as the plan says.

## 11. Case of the typed text

The default editor compares the typed text with the title through the database (`DefaultContentPickerResultProvider.cs:51`).
Under SQLite, as the plan sets it up, `LIKE` ignores the case of ASCII letters, which is what `stove` (for Camp Stove) needs.
Other databases may not. The Lucene editor lower-cases the typed text and the title itself (`LuceneContentPickerResultProvider.cs:64`,
`AspectsContentIndexHandler.cs:47`). Confirm that `pack`, `stove`, `lamp` and `rain` find their products.

## 12. Display name of the part

The pages of the field editor take the part's display name from the setting `ContentPartSettings.DisplayName`, and fall back to
the part's name (`ProductBundle`) when it is empty (`ContentPartSettingsExtensions.cs:54-62`). Seed 1 sets it to
Product Bundle so that the page "Add New Field To" and the list of parts read like the type. Confirm that the import keeps the
setting; without it, the narration is still true and only the title of that page reads `ProductBundle`.

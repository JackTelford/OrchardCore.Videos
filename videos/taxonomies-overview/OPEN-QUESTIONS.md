# Open questions: taxonomies-overview

Everything in the narration was read from the Orchard Core source at commit `92db4a83` and from the Blog recipe and
TheBlogTheme templates. The points below were not checked on a running site. The narration states none of them as a
fact beyond what the code shows, but a person must look at each one when recording.

## Confirm on a running site

1. **Features search.** The narration says the Taxonomies module has two features, and the clip searches for
   `taxonomies`. From the source the search matches the name, the id and the description of a feature
   (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), and no other manifest mentions taxonomies, so
   exactly two rows should remain: **Taxonomies** (Enabled) and **Taxonomies Contents List Filters** (Disabled),
   under **Content Management**. Check it once on screen.
2. **Draft until published.** `taxonomy-4` says the terms are stored inside the taxonomy, so the taxonomy is
   published to publish its terms. The code loads the taxonomy with `VersionOptions.DraftRequired` when a term is
   added (`AdminController.cs:110`) and the Taxonomy type is draftable (`Migrations.cs:24`). After seed step S1,
   check that the Categories row of the Content Items list shows a draft until the taxonomy is published, and that
   the front end only shows the new terms afterwards.
3. **Tags editor hint.** `categorize-3` says that pressing Enter on a name that does not exist creates a tag. The
   script of the Tags editor creates one only when the **Open** setting is on (default on) and the user has the
   Manage taxonomies permission (`TaxonomyField-Tags.Edit.cshtml`, `TagController.cs`). Typing `Moon` must show
   "Press enter to create a tag". Do not press Enter in the recording. The words are not text of the page: the style
   sheet draws them from the attribute `data-select` of the highlighted entry (`vue-multiselect.css:340-343`), so the
   recorder waits on the attribute, not on a text locator, and the screen must show the hint under the typed name.
4. **Disabled parent.** `categorize-2` says Food is disabled and Recipes and Restaurants can be chosen. The view
   renders a disabled radio button for a term with children when Leaves only is on
   (`TaxonomyField.Edit.cshtml:66-90`). Check it on screen after seed step S1. If the radio for Food is not greyed,
   rewrite the sentence.
5. **Term page content.** `site-2` says the Travel page lists the content categorized with Travel, newest first. The
   query of the term page (`DefaultContentsTaxonomyListQueryService.cs:33-50`) filters only on the term and on
   `CreatedUtc`, with no condition on Published or Latest. Check that the page lists the two published posts and
   nothing else. The narration does not say that only published items appear.
6. **Badges and links.** `site-1` says each badge is a link to a term page. The Blog theme's summary template wraps
   each badge in a link to the term (`Content-BlogPost.Summary.liquid:15-36`). Check that clicking **Travel** opens
   the term page. The term's address is not stated anywhere.
7. **Leaving the editor.** In `04-categorize` the clip leaves the Blog Post editor without saving. Check that no
   "unsaved changes" dialog stops the next navigation. In the taxonomy editor the only such prompt belongs to the
   script that reorders terms and appears after a reorder only (`wwwroot/Scripts/menu/menu.js`), and `03-taxonomy`
   does not reorder.
8. **Selectors marked "confirm"** in `recording-plan.md`: the admin menu items Tools and Features, and the Term
   Content Type drop-down. The field wrappers (`.field-wrapper-blog-post-tags`, `.field-wrapper-blog-post-category`:
   `HtmlClassify` turns `BlogPost` into `blog-post`), the Tags editor box (`.multiselect__tags`, because the input
   has a width of 0 until the list is open), the feature rows (`[data-filter-value]`) and the rows of the Content
   Items list (`li.list-group-item-action`, `OrchardCore.Admin/Views/AdminList.cshtml:60`) were read from the source,
   but check each once with `Recorder probe`.
9. **Icon of a new term.** The Category type has a required **Icon** field with the Icon picker editor
   (`blog.recipe.json:796-809`), and the value stays empty until an icon is clicked (`text-field-icon-picker.ts:36`).
   Seed steps S1.2 to S1.4 therefore pick an icon for Food, Recipes and Restaurants: open the drop-down button beside
   the heart button, type `globe` in the search box ("Type to filter"), click the first icon. The gesture was not
   tried on screen. If **Publish** answers "A value is required for Icon.", the click did not register.
10. **Button label on the Blog row.** `ListPartSummaryAdmin.cshtml:10` builds the label from the technical name of
    the contained type, so the button should read **List BlogPost Items** (the page it opens uses the display name:
    **Create Blog Post**). Check both labels on screen.
11. **Escape in the Tags editor.** `categorize-4` presses Escape once to close the list and empty the typed `Moon`.
    The editor is vue-multiselect with `preserveSearch` off, whose input closes the list and clears the text on the
    key-up of Escape (`vue-multiselect.esm.js:666-680`, `:1277`). Check on screen that the text disappears and the
    chips Earth, Exploration and Space stay. Do not use Backspace to erase the text: with an empty box it removes the
    last chip (`removeLastElement`, `:630-636`).
12. **Menu path of `taxonomy-1`.** The narration says the taxonomy "is listed under Content, Content Items". The
    clip opens the filtered list by its address, so the menu is not clicked on screen. The labels are
    `OrchardCore.Contents/AdminMenu.cs:65,68` (**Content**, **Content Items**). Check that the admin menu of the
    recorded site shows them under these names.
13. **Tags list while the post already has all three tags.** `categorize-3` opens the list on the post that has Earth,
    Exploration and Space selected, and the narration says the box "lists the existing tags". The editor does not
    set `hide-selected`, so the three tags should show, marked as selected. If the list is empty or reads "No tags
    found", rewrite the sentence or record this clip on "A weekend in Lisbon" (tag Earth only), which leaves
    Exploration and Space to list.

## Not narrated, on purpose

- Dragging a term to reorder it or to nest it under another term. The code supports it (`menu.scss`, the hierarchy
  posted by `TaxonomyPartDisplayDriver`), but a separate video covers organizing terms, and the gesture was not
  checked on screen.
- Whether a term page also lists the content categorized with the child terms of that term. The documentation
  (`src/docs/reference/modules/Taxonomies/README.md:21`) says the page lists the content categorized "as part of
  that Term hierarchy"; the query of the term page (`DefaultContentsTaxonomyListQueryService.cs:33-35`) matches the
  exact term only. The narration says only that the page lists the content categorized with the term.
- The address of a term page, and how a taxonomy gets one (the **Route contained items** option and the
  **Allow contained item routing** setting): `site-3` only names the option.

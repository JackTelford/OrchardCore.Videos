# Open questions: taxonomies-contents-list-filters

Everything in the narration was read from the Orchard Core source at commit `92db4a83` (no site was run while this
content was written or reviewed). The points below were checked in the source but not on a running site, or were left
out of the narration on purpose. A person recording the video confirms each on the real site and corrects the narration
or the plan if the site disagrees.

## To confirm on the running site (the narration depends on them)

1. **Indented child terms** (step `filter-2`: "with child terms indented below their parent"). The source indents a
   child term with two non-breaking spaces per level (`OrchardCore.Taxonomies/Drivers/TaxonomyContentsAdminListDisplayDriver.cs:21,99-103`).
   The dropdown is drawn by bootstrap-select (`OrchardCore.Resources/wwwroot/Vendor/CrestApps/bootstrap-select/bootstrap-select.js`),
   which writes the option text with `textContent` (`:566`) from the option's `text` (`:1576`); that property strips
   only ASCII white space, which a non-breaking space is not, so the indentation should stay. Confirm it is visible. If
   it is not, change the narration to say that the child terms follow their parent.
2. **A term matches only the items that have exactly that term** (not narrated). The filter compares
   the term id with the rows of the taxonomy index (`TaxonomyContentsAdminListFilter.cs:43-48`), the index holds
   only the terms selected on the field (`TaxonomyIndexProvider.cs:83-95`), and the field handler adds no ancestor
   (`OrchardCore.Taxonomies/Handlers/TaxonomyFieldHandler.cs`). So Technology should list no item with the seed data
   (the Category field of the Blog recipe accepts leaf terms only, `blog.recipe.json:661-672`). The narration of
   `filter-3` was cut back to what is shown (Software); choose Technology once off camera if a person wants to
   add the parent-term point.
3. **"Show all" and "Clear filter"** (steps `filter-4` and `filter-5`). Show all is the taxonomy id filter (any term of the
   taxonomy); Clear filter has an empty value and adds no filter (`TaxonomyContentsAdminListDisplayDriver.cs:84-94`,
   `TaxonomyContentsAdminListFilter.cs:34-50`, and the redirect only carries a non-empty value,
   `TaxonomyContentsAdminListDisplayDriver.cs:138-141`). Confirm
   that after Show all the About article, the Blog item, the Taxonomy items and the Main Menu item are gone, and that
   after Clear filter they are back.
4. **Which taxonomy buttons a content type shows** (steps `types-1` and `types-2`). The buttons are limited to the
   taxonomies that a taxonomy field of the selected type uses (`TaxonomyContentsAdminListDisplayDriver.cs:51-63`);
   while no type is selected every chosen taxonomy is offered. The selected type reaches that driver through
   `ContentOptionsDisplayDriver.EditAsync` (`FilterResult.MapTo`, `:31`), which runs first because the feature depends
   on Taxonomies, Content Types and Contents, in that chain (`OrchardCore.Taxonomies/Manifest.cs:22`,
   `OrchardCore.ContentTypes/Manifest.cs:9`). Confirm that Blog Post shows Categories and Tags, and Article shows none.
5. **The label list of the Content Type dropdown.** It offers only listable types plus "All content types"
   (`OrchardCore.Contents/Controllers/AdminController.cs:885-902`). The recipe's Blog Post type is not listable
   (`blog.recipe.json:188-196`), so the seed makes it so. Confirm that Blog Post and Article are in the list.
6. **The messages.** "The feature Taxonomies Contents List Filters was enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`)
   and "Site settings updated successfully." (`OrchardCore.Settings/Controllers/AdminController.cs:119`). Confirm where
   they show on the page (they must still be visible when the narration of `enable-3` and `settings-3` ends).
7. **The Features search** (step `enable-2`): "taxonomies" should leave exactly two rows (the name, id and description of
   each feature are searched, `FeatureEntryDisplayDriver.cs:20`, and the page script compares them in lower case,
   `OrchardCore.Features/Assets/ts/features-index.ts`). No other manifest of the source has the word in its name, id or
   description, but a site may add modules of its own. If a third feature matches, the narration "Two features match" is
   wrong.
8. **Seed creation of the posts.** The posts are created from the Blog editor (**Create Blog Post**,
   `OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:51-52`), so each has the Blog as its container, which the
   path pattern of the Blog Post type needs (`blog.recipe.json:216`). Confirm that the button is shown in the editor of
   the Blog and that a post is saved without a validation message about its path. Only the titles, categories and tags
   matter for the video.
9. **Selectors** in recording-plan.md for the bootstrap-select buttons and their options are guesses from the markup
   in the views; confirm them with `Recorder probe`.
10. **The selection of a dropdown after the page reloads** (steps `filter-4` and `filter-5`). The page reloads only when
    the selection of the dropdown changes (`changed.bs.select`, built into
    `OrchardCore.Contents/wwwroot/Scripts/contents-admin-list/contents-admin-list.js`). The select is bound to the
    model with `asp-for` and the value that was posted is added to the model state, so the dropdown should show the term
    that was chosen as selected after the reload (`OrchardCore.Taxonomies/Views/ContentsAdminListTaxonomyFilter.cshtml:4`,
    `TaxonomyContentsAdminListDisplayDriver.cs:136`). If it does not, "Clear filter" is the first option and is already
    the selected one, so choosing it does nothing and the list stays filtered: the narration of `filter-5` then needs
    another route to the unfiltered list. Confirm that Show all is shown as selected after `filter-4`.
11. **The order of the taxonomy buttons.** The order of the buttons follows the order in which the stored taxonomies are
    loaded (`TaxonomyContentsAdminListDisplayDriver.cs:66-69`), and the checkboxes of the settings page follow the order of an
    unordered query (`TaxonomyContentsAdminListSettingsDisplayDriver.cs:43`). The narration names "Categories and
    Tags" without claiming an order on screen, but the highlights of `filter-1` follow the real order. Note it.
12. **The icon of a new term** (seed B). The Category term type has a required Icon field with an icon picker
    (`blog.recipe.json:793-810`). The picker is drawn by a script (`text-field-icon-picker.ts`), so its selectors
    (`.icon-picker-toggle`, `.iconpicker-item`) are guesses: confirm that choosing the first icon is enough for the term to
    be accepted.

## Behaviors found in the code and left out of the narration (not verified)

- **The search text drops the taxonomy filters.** In the post of the list, when the search text differs from the
  original text, the action redirects with the new text only and does not evaluate the other editors
  (`OrchardCore.Contents/Controllers/AdminController.cs:319-325`), so the taxonomy values are not carried. Typing a
  search text while a term filter is active probably clears the term filter. Not narrated.
- **Two taxonomies at once.** Each taxonomy filter adds its own condition on the taxonomy index
  (`TaxonomyContentsAdminListFilter.cs:25-52`). Whether choosing a term of Categories and a term of Tags returns the
  items that have both depends on how the query library joins the same index twice. Not narrated or shown.
- **What the dropdown shows as selected after the page reloads.** See point 10. The narration does not mention a
  selected term.
- **A term filter and a content type that does not use the taxonomy.** The filter does not look at the content type
  (`TaxonomyContentsAdminListFilter.cs`), and the redirect after choosing a type carries the term that the dropdowns
  had posted (`TaxonomyContentsAdminListDisplayDriver.cs:130-145`), so a term filter that is active while another content
  type is chosen can produce an empty list, without a dropdown to clear it. The clips avoid it by starting every clip on a
  freshly loaded page.
- **Permissions of the settings page.** A role that holds "Manage taxonomies" but not "Manage settings" sees the menu item
  Settings, Taxonomy Filters, but the page is refused (`OrchardCore.Settings/Controllers/AdminController.cs:53`). The
  narration names both permissions; no such role was tried.
- **Typo in the source.** The hint on the Taxonomy Filters page reads "The taxonomies to filters in the contents list."
  (`OrchardCore.Taxonomies/Views/TaxonomyContentsAdminListSettings.Edit.cshtml:6`). It is not read aloud.

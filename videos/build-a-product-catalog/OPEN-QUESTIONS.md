# Open questions: Build a Product Catalog

Everything in the narration was read from the source (Orchard Core at commit `92db4a83`). The points below are the ones
a person must confirm on a running site before the first take. None of them is stated in the narration as more than the
source says.

## Labels the source builds at run time

1. **Part names in the Add Parts list.** The list shows the display name of each part. For a part without a custom
   display name, that is the part's name without the suffix Part
   (`OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-63`), so `TitlePart`
   appears as `Title`. Check that the checkbox is labeled `Title` and that its hint reads `Provides a Title for your
   content item.` (clips `02-product` `product-3`, `02-term-type` `term-2`).
2. **Root folder of the Media Library.** The tree's root is named by the script translation `FileLibrary`, which the
   server sets to `Files` (`OrchardCore.Media/Services/MediaJSLocalizer.cs:31`,
   `Assets/media-gallery/src/services/HierarchicalTreeBuilder.ts:90`). Check that the first row of the tree reads `Files`
   (clip `04-library`, `lib-1`).
3. **Index and permission names.** Typing `products` in `Index Name` should fill `Name` with `products`
   (`OrchardCore.Indexing/Assets/js/technical-name-generator.js`), and the permission of that index should read
   `Query 'products' Index` in the Anonymous role (`src/OrchardCore/OrchardCore.Indexing.Core/IndexingPermissions.cs:15`).
   Typing `products` in `Search permissions` also lists a Lucene row, `Query Lucene products Index`; the plan ticks the other
   one (clip `06-permission`, `perm-2`).
4. **Notices.** The exact wording of the notice after enabling a feature that has dependencies
   (`OrchardCore.Features/Controllers/AdminController.cs:317`) and after adding a part
   (`OrchardCore.ContentTypes/Controllers/AdminController.cs:372`, which writes the part's name) should be read on screen;
   the narration does not quote them.

## Script-driven screens

5. **The Media Library and the media picker** are a Vue application. The plan names the labels from the source, but the
   selectors for the folder menu (`#folder-tree a[title="Folder Action"]`), the file rows and cards, and the `OK` button
   of the `Select Media` window are to be found with `Recorder probe` (clips `04-library` and `05-product`, `add-3`).
   Setting the files on the hidden input `#fileupload` replaces the system file dialog, which a headless capture cannot
   show.
6. **Slugified names.** `Media Slugify` is expected to give `product-photos`, `trail-backpack.jpg`, `two-person-tent.jpg`
   and `down-sleeping-bag.jpg`. The slug service (`OrchardCore.Modules.Services.SlugService`) was not run; check the three
   names on screen. The narration states only `product-photos` and `trail-backpack`.

## Search behavior to confirm

7. **Query field names.** The page of a new Lucene index lists, under `Default Query Settings`, the fields known before any
   content type is chosen: `ContentItemId`, `ContentItemVersionId` and the full-text field `Content.ContentItem.FullText`. The
   code pre-selects the full-text field for a new index (`OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs:81-82`,
   `src/OrchardCore/OrchardCore.Lucene.Core/Handlers/LuceneContentIndexProfileHandler.cs:104-106`), and the video leaves it
   as it is. The full text holds the display text, so the title of a product is searchable
   (`OrchardCore.Contents/Handlers/FullTextAspectContentHandler.cs:49-51`, `IncludeDisplayText` defaults to true in
   `Models/FullTextAspectSettings.cs:27`). If no field were checked, the search service would query every indexed field
   (`src/OrchardCore/OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:55-66`; hint `Leave it blank to query all
   fields.`, `LuceneQuerySettings.Edit.cshtml:61`). Confirm on screen which of the three fields is checked, that `backpack`
   finds the Trail Backpack and that `kayak` finds nothing.
8. **Drafts.** The narration says that, with the default index settings, search does not find a draft. The source indexes
   only the published version unless `Index Latest version` is checked (hint text in
   `OrchardCore.Indexing/Views/ContentIndexMetadata.Edit.cshtml:32`, default off in `ContentIndexMetadata.cs:5`). Confirm
   once by saving a product as a draft and searching for it.
9. **Index build time.** The index is filled by a background process after `Save`. The plan waits, outside the capture,
   until `/search/products?terms=backpack` returns the Trail Backpack before `06-settings` is recorded, from the signed-in
   recorder context (a visitor is challenged until `06-permission`). No Indexing Worker feature is needed: the handlers that
   record and index changes belong to the Indexing feature itself (`OrchardCore.Indexing/Startup.cs:51-57`), the Worker only
   adds a background task (`Startup.cs:88-94`), and `Save` synchronizes the new index after the end of the request
   (`src/OrchardCore/OrchardCore.Indexing.Core/DefaultIndexProfileManager.cs:205-212`), which processes the indexing tasks
   recorded when the products were published (`IndexingTaskManager.cs`, `ContentIndexProfileHandler.cs:55-62`). Confirm that
   the Trail Backpack appears without enabling anything else.
10. **Visitors.** The capture is signed in as `admin`, who holds every permission. The permission of the Anonymous role is
    the only thing that lets a visitor search (`OrchardCore.Search/Controllers/SearchController.cs:110`). Confirm once in a
    private window, outside the capture, that `/search` works after clip `06-permission`.
11. **Result display.** The results use the Summary display of Product. The Blog theme has no generic summary template (only
    for Blog Post and Category), so the default `OrchardCore.Contents/Views/Content.Summary.cshtml` renders the header zone,
    where the Title part puts its link (`OrchardCore.Title/Drivers/TitlePartDisplayDriver.cs:34-35`,
    `Views/TitlePart.Summary.cshtml:3`), and the content zone, where the three fields put themselves
    (`NumericFieldDisplayDriver.cs:31-32`, `OrchardCore.Media/Drivers/MediaFieldDisplayDriver.cs:48-49`,
    `TaxonomyFieldDisplayDriver.cs:35-36`). The price is printed as stored, without the scale
    (`OrchardCore.ContentFields/Views/NumericField.cshtml`), so `89` and not `89.00`. Check on `/search` that all four appear
    and look right; the narration of `find-2` names all four. The category name on the product page comes from the
    published taxonomy (`OrchardCore.Taxonomies/Helper/TaxonomyOrchardHelperExtensions.cs:19-27`), which is why the taxonomy is
    published in `tax-6`.

## Taxonomy editor

12. **Child terms.** The narration says a term has an `Add` button that creates a child term
    (`OrchardCore.Taxonomies/Views/Content.TermAdmin.cshtml:36`). The video does not click it; confirm that the button is
    shown on each term row.
13. **Edit page after a term is added.** After a term is published, the source redirects to the Edit page of the taxonomy
    (`OrchardCore.Taxonomies/Controllers/AdminController.cs:165`). That page then has no return address, so `Publish` is a
    single button, and publishing reloads the same `Edit Taxonomy` page with the notice `Your Taxonomy has been published.`
    instead of going back to the content list (`OrchardCore.Contents/Controllers/AdminController.cs:841-847`). Check that the
    terms listed there come from the draft that the term created.

## Speech

14. Ask the narrator to check the spoken forms of `/search` (written "slash search" through the storyboard's `spoken` map),
    `Save & Configure` (said "Save and Configure"), `Slugify`, `Lucene` and `Ok`, with `scripts/pronounce.py`.
15. `Index Latest version` is not named in the narration. The word `later` in `idx-3` only says that products published
    after the index exists are added to it (the deferred indexing task of
    `src/OrchardCore/OrchardCore.Indexing.Core/Handlers/IndexingContentHandler.cs:86-99`).
16. **Fonts of the Blog theme.** The layout of the Blog theme links Google Fonts
    (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`), and the dead proxy of the plan applies to the site, not to the
    recorder's browser. On a machine without internet access the fonts of the search page and of the product page fall back
    to the browser's. Check how `/search` looks in the capture before recording `06-find`.
17. **Admin menu of the Blog recipe.** The recipe adds its own `Content` menu with `Content Items`, `Article`, `Page` and
    `Taxonomy` (`blog.recipe.json`, step `AdminMenu`). Items with the same caption are merged
    (`src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74`), so `Content` and `Content Items` should each
    appear once. Confirm before the first take that the menu clicks of the plan match a single element.

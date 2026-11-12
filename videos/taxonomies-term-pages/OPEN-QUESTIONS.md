# Open questions: Taxonomies: Term Pages That List Their Content

Everything the narration states was read in the Orchard Core source (the commit the programme uses), but the source
could not be run while the video was written. A person must confirm the points below on a running site, with the
recording plan's seed, before the clips are recorded. Paths are relative to `src/` of the Orchard Core checkout.

## Not said in the narration, because the source does not settle it

1. **Drafts and unpublished items on a term page.** The query that lists a term's items
   (`OrchardCore/OrchardCore.Taxonomies.Core/DefaultContentsTaxonomyListQueryService.cs:34-50`) filters on
   `TaxonomyIndex.TermContentItemId` only. It has no `Published` or `Latest` condition, and `TaxonomyIndexProvider`
   (`OrchardCore.Modules/OrchardCore.Taxonomies/Indexing/TaxonomyIndexProvider.cs:23-100`) writes records for the published
   version and for the latest draft of an item. No `IContentsTaxonomyListFilter` is registered by the module. Check:
   (a) publish a post, then change it and select Save Draft: does it appear twice on its term page? (b) does a post that
   was never published appear? The narration says "the posts that have Travel selected" and never "published posts", and
   the recording plan asks for posts with a published version and no draft. If the answer is that drafts do appear, a
   documentation issue may be worth raising; the video needs no change.
2. **How the pager looks in The Blog Theme.** The pager is created in
   `OrchardCore.Modules/OrchardCore.Taxonomies/Drivers/TermPartContentDriver.cs:69` as a numbered `Pager` shape and drawn by
   `Category-TermPart.liquid:10-13`. The theme has no pager templates of its own, and the `previous_text` and `next_text` it passes
   are applied only to a `PagerSlim` (`OrchardCore/OrchardCore.DisplayManagement.Liquid/Tags/ShapePagerTag.cs`), so the module's
   shape methods draw the links with their default texts `<<`, `<`, the page numbers, `>` and `>>`
   (`OrchardCore.Modules/OrchardCore.Navigation/PagerShapesTableProvider.cs:146-366`). The shape returns no links below two pages
   (`PagerShapesTableProvider.cs:179`). The narration of `paging-3` and
   `paging-4` says only that "a pager appears under" the list and that "page two" shows the three older posts. Confirm with
   Page size 3 and six posts: the pager is visible under the third summary, `ul.pagination a` with the text `2` exists (else
   the plan's fallback, the next-page link), and page 2 holds posts 2 and 1 and the recipe post.
3. **The `pageSize` query parameter.** `TermPartContentDriver.cs:95` builds the pager with `new Pager(pagerParameters, pageSize)`
   (`OrchardCore/OrchardCore.Navigation.Core/Pager.cs:15-18,41-45`), which takes the `pageSize` of the query string when there is one,
   without the check against the page size options and the maximum that the other constructor makes. The narration says only that the
   number of items on a page is the Page size in Settings, General, which holds when the address has no `pageSize`. Check:
   open `/categories/travel?pageSize=2` and see whether the page holds two posts. If it does, a documentation or code issue may be
   worth raising; the video needs no change.
4. **Addresses of nested terms.** `AutoroutePartIndex.cs:186-189` builds a child term's address on its parent's. It is
   not narrated or shown.

## Said in the narration, read in the source, not shown on screen

Confirm each one on a running site; if one is wrong, change the narration and the slide of the step named.

| Step | Statement | Where it was read |
|---|---|---|
| `idea-3` | The page of a parent term does not list an item that has only a child term selected. | The list matches the term's own id (`DefaultContentsTaxonomyListQueryService.cs:34`); a Taxonomy field stores only the selected ids (`OrchardCore.Modules/OrchardCore.Taxonomies/Drivers/TaxonomyFieldDisplayDriver.cs:88`). The Taxonomies documentation says "as part of that Term hierarchy" (see the documentation issues). To check: add a child term under Travel (Add), select it in a post, publish the taxonomy, and look at `/categories/travel`. |
| `idea-2` | A term page's address is the taxonomy's permalink followed by the term's permalink. | `AutoroutePartIndex.cs:161-175`, `AutoroutePartHandler.cs:172`. |
| `needs-2` | A term has a page only once the published taxonomy contains it, so the taxonomy must be published after terms are added. | Terms are saved into a draft of the taxonomy (`OrchardCore.Modules/OrchardCore.Taxonomies/Controllers/AdminController.cs:110`), and only published routes are loaded (`OrchardCore/OrchardCore.Autoroute.Core/Services/AutorouteEntries.cs:181`). To check: add a term to Tags, open its page before and after selecting Publish on the Tags taxonomy. |
| `needs-3` | Without Manage contained item routes on the type of the terms, the address ends with the identifier of the term and its name, and the term's own Permalink is not used. | `OrchardCore.Modules/OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:172` and `DefaultRouteContentHandler.cs:22-30`. To check: clear the option on the Tag type (Design, Content Definition, Content Types, Tag, then Edit on its Autoroute part), save, publish the Tags taxonomy again, and look at the address of a tag page. Restore the option afterwards. |
| `routing-3` | Without Route contained items, the terms have no pages. | `AutoroutePartIndex.cs:141` (contained items are indexed only when the option is on). "No pages" means no routed address: the Item controller can still display a contained item when it is given a `jsonPath` in the query string (`OrchardCore.Modules/OrchardCore.Contents/Controllers/ItemController.cs:25-27`), which no link of the site produces. To check: clear it on a throw-away taxonomy, publish, and look at its terms' links. |
| `needs-1` | The Route contained items option, and every term address, need the Autoroute feature. | The option is a box of the Autoroute module (`OrchardCore.Modules/OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:94-100`) and the routes are loaded by `AutorouteEntries.cs`; the Taxonomies feature depends only on `OrchardCore.ContentTypes` (`OrchardCore.Modules/OrchardCore.Taxonomies/Manifest.cs:14`). To check: on a throw-away site with Taxonomies and without Autoroute, open a taxonomy and look for the Permalink box. |

## Things to look at while recording

- **Taxonomy Permalink.** `routing-2` expects the Permalink box of the Categories taxonomy to be greyed out
  (`AutoroutePart.Edit.cshtml:37`, because the Taxonomy type's Autoroute settings in `Migrations.cs:36-41` do not allow a
  custom path). The narration does not mention it. If the box is editable, nothing changes in the text.
- **Route contained items checked.** `routing-3` expects the box checked (`blog.recipe.json:961`). The label is read from
  `AutoroutePart.Edit.cshtml:100`; its hint text is not read in the narration.
- **The Edit buttons.** `routing-1` expects one result, the Categories taxonomy, with an `a.btn.edit` button. The layout of the Content
  Items list (List or Grid) can change the markup around it, not the button.
- **Term editor.** `routing-4` expects the title "Edit Category" and a Permalink box holding `travel`.
- **Settings reload.** After `paging-2` saves the General settings the tenant reloads
  (`DefaultSiteSettingsDisplayDriver.cs:140`). Confirm how long the first request takes, and that the signed-in session survives it.
- **Order of the home page.** `browse-1` and `browse-2` work with any post that has a Travel badge; nothing depends on
  which post is first. `browse-4` needs a Space badge on the Travel page: posts 5 and the recipe post have it.
- **The heading of a Tag page.** The theme has a `Content-Category` template and none for Tag, so the Tag pages use
  `Content.liquid` (a heading with the tag's name). Confirm the heading text on `/tags/space`.
- **Scroll distances.** The distances of `browse-3`, `browse-6`, `paging-3` and `paging-4` (`ScrollAsync`) are estimates: the height
  of the theme's header and of a summary was not measured. Adjust them so that the three summaries named in each step are in view
  when its narration ends.
- **The Tags editor of the seed.** The Tags box of a post is the tags editor component of the Taxonomies module
  (`OrchardCore.Modules/OrchardCore.Taxonomies/Views/TaxonomyField-Tags.Edit.cshtml`), and a tag typed that does not exist can be
  created from it (`TaxonomyFieldTagsEditorSettings.Open` is true by default, `Settings/TaxonomyFieldTagsEditorSettings.cs`). Pick the existing
  tag from the list, or seed the posts with `fetch` calls, so that no fourth tag appears in the pages the plan lists.

## Pronunciation to confirm

The narration says "Autoroute" (`needs-1`) and "Permalink". Check both with `scripts/pronounce.py`; if the voice misreads
"Autoroute", add a "spoken" entry to `storyboard.json` (for example "Auto route").

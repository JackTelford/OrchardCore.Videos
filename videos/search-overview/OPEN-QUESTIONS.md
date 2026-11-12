# Open questions: search-overview

The narration states only what the source of Orchard Core (commit `92db4a83`) and its documentation show. The author could
not run a site, so the points below were read from the code and have to be confirmed on the running site before a take,
with `Recorder probe`. If an answer differs from what the plan says, fix `recording-plan.md`; if it differs from what the
narration says, report it before recording.

## Seed data

1. **The JSON import of the three posts has not been run.** The items copy the shape of the stock `BlogPost` of
   `blog.recipe.json` (`TitlePart`, `ContainedPart`, `AutoroutePart`, `MarkdownBodyPart`, `BlogPost.Subtitle`). The
   `Owner` and the `Category` and `Tags` fields are left out. Confirm that the import succeeds, that `/blog` lists four
   posts, and that a post page and the summary in the results show no error. The Blog theme reads the category and tag
   fields through `taxonomy_terms`, which returns nothing for a missing field (`TaxonomyTermsFilter.cs`), so no error is
   expected. The plan has a fallback: create the posts with the admin UI.
2. **Fake text only.** "Maple Lane" is an invented place. If a reviewer prefers a different invented name, change it in
   `recording-plan.md` only; the narration does not name it.

## Clip `02-features`

3. **Message after Enable.** `AdminController.cs:317` of the Features module builds "The feature {name} was enabled." (one
   feature) or "The following features were enabled: ..." (several). Confirm which one Lucene and Search give, with
   the Blog recipe's features already on. The narration only says that the message names the feature.
4. **Filter text for Search.** The filter box matches the name, the id and the description of a feature
   (`FeatureEntryDisplayDriver.cs:20`). `frontend search`, which the narration of `features-3` says to filter for, is taken
   from the description of the Search feature ("Provides frontend search capabilities against indexes."). No other
   manifest has that phrase. Confirm that it leaves the Search row visible.
5. **The Add index dialog before Lucene.** The warning "You need to enable a feature which provides an index source." is
   shown when no index source is registered (`Index.cshtml:65-67`). Confirm that the Blog recipe registers no source, so
   the warning shows on a fresh Blog site.
6. **Features page after the reload.** The page reloads at the top; confirm the green message is visible without scrolling.

## Clip `03-index`

7. **Field order of the form.** `ContentIndexMetadata_Edit` and `LuceneIndexProfile_Edit` are both placed at `Content:5`
   (`ContentIndexProfileDisplayDriver.cs`, `LuceneIndexProfileDisplayDriver.cs`), so the order of Culture, Content types,
   Index Latest version, Analyzer and Store Source Data on screen was not determined. The plan finds every field by its
   label. Confirm that Culture and Content types fit on one screen for step `index-3`.
8. **Culture default.** A new index has `Culture` null, so the browser selects the first option, "Any culture"
   (`ContentIndexMetadata.Edit.cshtml:9`). Confirm that the select shows it.
9. **Name follows Index Name.** `technical-name-generator.js` copies the text of Index Name, lower case and hyphenated,
   into Name while Name is empty. Confirm that this happens with the recorder's typing, and that Name shows `website`.
10. **Query field names on creation.** `NewAsync` runs the `InitializingAsync` handlers
    (`DefaultIndexProfileManager.cs`, `NewAsync`), so `LuceneContentIndexProfileHandler.PopulateAsync` fills the field
    mappings with the three base fields (content item id, version id and `Content.ContentItem.FullText`) before the form is
    built. The "Query field names" list of the new-index form therefore shows those three, and the full-text one is
    pre-selected (`LuceneIndexProfileDisplayDriver.cs`, `context.IsNew && field == FullTextKey`). Saving the form keeps the
    selected field as the default search field, so the search page queries the full text, which is the title and the body
    (`FullTextAspectContentHandler.cs`, both settings default to true). Confirm that the list shows the full-text field
    ticked, that `/search/website?Terms=harvest` returns exactly the two posts, and do not use a word that appears in
    another field (for example `admin`). If a search finds nothing after Synchronize, open the index in Edit and look at
    "Default Query Settings" (Version, Query field names). The video does not mention these settings.
11. **The index is filled after Save.** The Save action starts a synchronization after the response
    (`DefaultIndexProfileManager.cs`, `SynchronizeAsync`), which processes the log of indexing tasks
    (`ContentIndexProfileHandler.cs`, `SynchronizedAsync`). Confirm, with the check of the plan, that the seeded posts are
    found without Rebuild.
12. **Labels in the list.** The list shows the provider (`Lucene`) and the type (`Content`) as badges
    (`IndexProfile.DefaultTags.SummaryAdmin.cshtml`) in the List layout and in a "Source" column in the Grid layout. The
    narration says "the labels Lucene and Content". Confirm what the default layout shows.

## Clips `04-settings` and `05-permission`

13. **No field for the page title.** `SearchSettings` has a `PageTitle`, but `SearchSettings.Edit.cshtml` shows only
    "Default search index" and "Placeholder". The video does not mention a page title. Saving the form assigns `PageTitle`
    from a model that has no such field (`SearchSettingsDisplayDriver.cs`, `UpdateAsync`), so it is cleared by the first save.
14. **The permission filter.** The "Search permissions" box filters rows on the description in `data-text`. Confirm that
    typing `Query 'website' Index` (with the apostrophes) shows exactly one row, and not the Lucene permission "Query Lucene
    website Index" (`LuceneIndexPermissionHelper.cs`), which is a different permission. If the apostrophes break the
    filter, type `website` and tick the row whose text is "Query 'website' Index".
15. **Where the row appears.** The narration does not name the group of the permission, because the group of a permission
    is the feature of its provider and was not confirmed (Indexing and Search are the expected ones). The plan finds the
    row by its id, `Checkbox.QueryIndex_website`; confirm that the id is built from the `Name` of the index.
16. **Without the permission.** `SearchController.cs:110-113` returns `ChallengeOrForbid()`, so a signed-out visitor is
    challenged. The video does not show this state; the narration only says a visitor needs the permission.

## Clip `06-widget`

17. **Where the widget appears.** The widget is placed in the `Footer` zone, which the Blog theme renders at the bottom
    of every page (`Layout.liquid`, `render_section "Footer"`). The recipe's own footer widget has position 10 and the new
    widget gets `max + 1`, so it should come after it. Confirm what the footer looks like, and that "Render title" unchecked
    leaves the widget without a heading.
18. **After Publish.** The editor was opened with a return address, so Publish should return to "Widgets and Layers"
    with the message "Your Search Form has been published." Confirm the page and the message.
19. **Index Name of the widget.** It holds the **Name** of an index (it is used as the `index` part of the address of the
    search page, which looks the index up by its name): empty means the default index. The narration says only that
    leaving it empty uses the default index.

## Clip `07-visitor`

20. **Scrolling.** The search box of the widget has `autofocus`, so the page may open already scrolled to the footer;
    the plan scrolls to the top before the capture. The scroll amounts (3000, 350) are guesses to be adjusted with a probe.
21. **Results.** Confirm that the two posts are listed with their titles and subtitles, that the order is not narrated
    (it is not), and that the page shows two search forms (the page's own and the footer widget's).
22. **Placeholder and no result.** Step `visitor-3` clears the box of the search page (the first `input[name='Terms']`, in the
    form of the page) and expects the placeholder `Search articles and posts` of the Site Search settings to show in it
    (`SearchController.cs` sets `SearchForm.Placeholder`; `Search-Form.liquid` of the Blog theme draws it). Confirm that the
    placeholder shows on the page after a search, and that the footer widget shows its own `Search this site`. Step
    `visitor-4`: confirm that `zebra` gives "There are no such results." and no error.

## Voice and timing

23. **Pronunciation.** Not checked with `pronounce.py`: Lucene, Elasticsearch, Azure AI Search, Synchronize, frontend, and
    "Query 'website' Index" (the narration of `permission-2` writes the label with its apostrophes, as the code does; confirm
    that the voice does not read them aloud, and add a `spoken` entry if it does). The storyboard has no `spoken` entries.
24. **Hold times.** The narration is 706 words (about 4.7 minutes). Check on the first take that the actions of every step
    end before its narration does, especially `features-2`, `features-3` and `index-1`, which reload or open a page.
25. **Menu entries on screen.** The narration names menu paths ("Open Tools, Features", "Open Settings, Search, Site Search",
    "Open Access Control, Roles", "Open Design, Widgets"), and the plan opens each page by its address with `NavigateAsync`, so
    the viewer sees the page change and not the menu. The leaf entries of the admin menu are links
    (`NavigationItemLink-admin.cshtml`, class `item-label`, with the address as `href`), and the groups are buttons that
    expand on a click (`NavigationItem-admin.cshtml`); how the menu looks while the capture runs (collapsed groups, the
    focused or persistent behavior) was not checked. If a person wants the menu on screen, expand the group before
    `StartCaptureAsync` and click the leaf link inside the step, after confirming the selectors with `Recorder probe`.
    The plan keeps the address navigation, which does not depend on the state of the menu.

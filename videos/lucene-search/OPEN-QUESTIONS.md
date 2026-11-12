# Open questions: Lucene video

What the author could not verify from the source or the documentation, with what was checked and what a person must
confirm on a running site (the Orchard Core source at commit 92db4a83 was read, nothing was run). The narration states
only what the source shows; the points below are kept out of it.

## Before narrating

1. **Pronunciation.** `spoken` in `storyboard.json` is empty. Confirm with `scripts/pronounce.py` how the voice says
   "Lucene", "Elasticsearch", "Azure AI Search" and "full-text", and add a `spoken` entry only if one is wrong. Also confirm
   the two spellings that carry symbols: "Lucene (Obsolete)" (`enable-2`: the voice should say "Lucene Obsolete", without
   spelling out the brackets) and "Query 'search' Index" (`search-4`: the quotes should be silent, so it reads "Query search
   Index"). If either is wrong, add an entry such as "Lucene (Obsolete)": "Lucene Obsolete".

## While recording

2. **Order of the editors on the Create index page.** The content settings (Culture, Content types) come from
   `OrchardCore.Indexing/Drivers/ContentIndexProfileDisplayDriver.cs:49` and the Lucene settings (Analyzer, Store Source Data)
   from `OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs:55`; both are placed at `Content:5`, so the source does
   not say which comes first. Steps `create-3` and `create-4` do not depend on the order (each scrolls to its own field), but
   confirm that both groups are on the page.
3. **"Name" is filled in from "Index Name".** The script `OrchardCore.Indexing/Assets/js/technical-name-generator.js:17-26` fills
   "Name" while the user has not typed in it. Confirm on screen that typing `search` in "Index Name" fills "Name"; the
   narration of `create-2` says so. If "Name" stays empty, type `search` in it and shorten the narration.
4. **Notification after enabling Lucene.** The text is "The feature Lucene was enabled." for one feature and "The
   following features were enabled: ..." for several (`OrchardCore.Features/Controllers/AdminController.cs:317`). Lucene's
   dependencies (Indexing, Content Types, Queries Core Services) are enabled by the Blog recipe, so the singular text is
   expected. Confirm. The narration says only "a message confirms".
5. **Description of the Lucene feature.** The Features page shows "Creates Lucene indexes to support search scenarios,
   introduces a preconfigured container-enabled content type." (`OrchardCore.Lucene/Manifest.cs:13`). A search of the
   Lucene module and its core library finds no content type being created, so the second half looks stale. The
   narration of `enable-2` says only that Lucene provides indexes. Confirm what appears after enabling, and whether
   a content type is added.
6. **Name of the Title part.** The row of the part on the "Edit Content Type - Article" page uses the part name without its
   trailing "Part" (`OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:60`), so "Title" is
   expected, unless a part definition sets another display name. Confirm the row text used by `fields-3`.
7. **Administrator access to the search page.** `SearchController.cs:110` asks for the permission "Query any index" on the
   index. The Search module grants it to the Administrator role (`OrchardCore.Search/Permissions.cs`), and the Search feature
   is enabled after the site was set up. Confirm that `/search/search` opens for `admin`. If it answers "forbidden", grant
   the permission to the Administrator role off camera (Security, Roles) before taking S2.
   The narration of `search-4` names two permissions, "Query any index" (`OrchardCore.Indexing.Core/IndexingPermissions.cs:10`)
   and "Query 'search' Index", the template "Query '{0}' Index" with the Name of the index (same file, line 15; resolved by
   `IndexingAuthorizationHandler.cs:34`). Only the Administrator role holds "Query any index" by default
   (`OrchardCore.Search/Permissions.cs:18-24`), which is why the narration names the Anonymous role.
   The Lucene module also lists a third, "Query Lucene {0} Index" (`OrchardCore.Lucene/Permissions.cs:27`,
   `LuceneIndexPermissionHelper.cs:14-17`), which no controller checks (the search page asks for the first two). On the
   Roles page of a running site, confirm the exact labels of the three entries for the index `search`, and that
   giving the role Anonymous "Query any index" is what lets a signed-out visitor search.
8. **First filling of the index.** The index is filled after the request that saves it, in a background job
   (`OrchardCore.Indexing.Core/DefaultIndexProfileManager.cs:209`) that reads the indexing tasks recorded for the content. The plan
   waits for the three garden articles. Confirm that the seed articles, which are imported before the index exists,
   are found without a Reset; if not, the plan's fallback (Reset from Actions) is the answer, and the video is unchanged.
9. **Look of the results.** The results use the "Summary" display of each content item (`OrchardCore.Search/Views/Search-Results.cshtml:35`)
   in the Blog theme. Confirm that the three articles show their title, and see whether the title links to the page; the narration
   says only "shown as summaries". Also note the order in which the page gives the three results (it follows the score).

## Settings on screen that the narration does not describe

10. **Query analyzer, Allow Lucene queries and Version.** All three are shown in "Default Query Settings" while `create-5` is
    on screen. In the source, `AllowLuceneQueries` and `QueryAnalyzerName` are saved (`LuceneIndexProfileDisplayDriver.cs:120-125`)
    and are read nowhere else; `LuceneSearchService.cs` builds its parser with the analyzer of the index ("Analyzer") and parses
    Lucene query syntax whatever the checkbox says. "Version" offers one entry, labelled "Lucene 4.8"
    (`LuceneIndexProfileDisplayDriver.cs:67`), which is a version number and so cannot be read in a narration.
    Confirm on a running site whether any of the three changes the behavior of the search page before describing them.
11. **Keeping the settings of a part after an index exists.** "Include this element in the index", "Keyword" and "Stored" change
    the documents built from then on (`OrchardCore.Contents/Indexing/ContentItemIndexCoordinator.cs:57-77`). Whether an existing
    index needs Reset or Rebuild, and when a new field shows in "Query field names", was not checked on a site. The
    video does not say.

## Servers

12. **Lucene Worker and Indexing Worker.** In the source the feature "Lucene Worker" (`OrchardCore.Search.Lucene.Worker`) has no
    startup class of its own; the only other reference to its id in the whole source tree is the data migration below. The background task "Indexes Content Updater" (`OrchardCore.Indexing/ContentIndexingBackgroundTask.cs:11`,
    every minute) is registered by the feature "Indexing Worker" (`OrchardCore.Indexing/Startup.cs:89`), and the data
    migration `OrchardCore.Indexing/DataMigrations/WorkerFeatureMigrations.cs:17-29` enables Indexing Worker if Lucene Worker or
    Elasticsearch Worker is already enabled when the migration runs. The documentation says that Lucene Worker "creates a background
    task". The narration (`servers-2` to `servers-4`) says what the source shows, not what the documentation says: the task belongs to
    Indexing Worker, Lucene Worker "adds no task itself", a site on one server updates its own index when it handles a change
    (`OrchardCore.Indexing.Core/Handlers/IndexingContentHandler.cs:86-99`, registered with the Indexing feature in
    `OrchardCore.Indexing/Startup.cs:56-57`), and a site on several servers relies on the task to reach the other servers
    (the only caller of `ProcessRecordsForAllIndexesAsync` is the task). To confirm on a running site: enable
    Lucene Worker alone, then look for the task in the background tasks of the admin, and again with Indexing Worker enabled.
    Lucene Worker depends on Lucene (Obsolete), so the recorder never enables it.
13. **Several servers.** The statement that each server keeps its own files and its own record of how far it has indexed comes
    from the comment of `OrchardCore.Lucene.Core/Services/LuceneIndexingState.cs:10` and from the index path in `LuceneIndexStore.cs:47`;
    a multi-server run was not tried.

## Scope

14. **The Lucene Query page.** The module's only admin menu entry, Search, Queries, Run Lucene Query, is a page with a query editor that
    holds a query in the format of Elasticsearch, written as JSON (`OrchardCore.Lucene/Views/Admin/Query.cshtml`). An administrator video
    shows no JSON, so the page, and the action Query Index on a row of the Indexes list, are not recorded. The question for the
    maintainer: may an administrator video show the Run Lucene Query screen, if the narration never reads the query and only
    leaves the query that the page starts with (a query that matches all documents) and selects Run Query? Until it is
    answered, the video does not mention hand-written queries.

## On-screen text

15. **The address of the search page.** The Search feature adds no menu entry or link to its page, and the recorder shows no
    address bar, so `search-1` writes the address as the caption of the ring around the search field: "/search/search" (the
    word search, a slash, the index name). It is the address of a page of the site, not an API route and not code, but it is
    text on screen in an administrator video: a person confirms that the caption is acceptable, and otherwise removes the caption
    (the narration of `search-1` still says the address). On a running site, confirm that `/search/search` opens for `admin`
    once the Search feature is enabled and the index is filled.

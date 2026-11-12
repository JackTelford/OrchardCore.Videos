# Open questions: Elasticsearch

What was checked in the source (Orchard Core at commit 92db4a83) and the documentation, and what a person must confirm on
a running site before or while recording. Nothing below is in the narration as a fact unless the answer says so.

## 1. Pages left out because they are JSON editors

**Run Elasticsearch Query** (Search > Queries) and **Index Info** (Actions menu of an index) are editors for JSON text
(`src/OrchardCore.Modules/OrchardCore.Elasticsearch/Views/Admin/Query.cshtml`, `.../Views/Admin/IndexInfo.cshtml`). The
admin audience rule forbids showing JSON, so no recording opens them; the `tools-1` and `tools-2` slides only name them.
Whether an admin video may show a page that holds a JSON editor is a question for the maintainer. If it may, a recording of **Query
Index** (Actions > Query Index runs a query that matches every document and lists the documents in a table) is the
natural next clip.

## 2. The locators of the admin menu

The plan expands menu groups and clicks entries by their text (Tools, Features; Search, Indexes; Settings, Search, Site
Search). The labels are read from the menu providers in the source, but the redesigned admin menu was not run: confirm how
a group opens, and that each entry is a link the recorder can click. If clicking through the menu takes longer than the
narration of the step, use `NavigateAsync` to `/Admin/Features`, `/Admin/indexing` and `/Admin/Settings/search`, and keep
the narration (the page title then shows the same words).

## 3. The four Elasticsearch rows of the Features page

`enable-1` names the rows **Elasticsearch**, **Elasticsearch (Obsolete)**, **Elasticsearch Worker** and **Elasticsearch
Content Picker** (names from `Manifest.cs` of the module). Confirm that the filter `Elasticsearch` lists these four, and
that the **Enable** button id is `btn-enable-OrchardCore_Elasticsearch`. If the list shows more rows, nothing changes in
the narration.

## 4. The default query fields of a new index

`create-6` says the full-text field is already selected in **Query field names**. Read from
`ElasticsearchIndexProfileDisplayDriver.cs:93-94`: for a new index the entry `Content.ContentItem.FullText` is selected
by default. Confirm on screen. If it is not selected, change the narration of `create-6` before recording: the search page
returns no results when no field is selected (`ElasticsearchService.cs:86`).

## 5. The Elasticsearch Worker feature

Its manifest description is "Provides a background task to keep indices in sync with other instances." No startup class
of the module registers anything for `OrchardCore.Search.Elasticsearch.Worker`. The only code that mentions it is
`OrchardCore.Indexing/DataMigrations/WorkerFeatureMigrations.cs`, which runs once, when Indexing is first enabled, and then
enables **Indexing Worker** (the task "Indexes Content Updater", every minute) if the Elasticsearch Worker was already
on. The feature also depends on **Elasticsearch (Obsolete)**. To confirm on a running site: what enabling the Elasticsearch
Worker adds after Indexing was already enabled (Tools > Background Tasks, Tools > Features). The video names the feature
in `enable-1` and says nothing else about it, and it is never enabled on camera.

## 6. The Query analyzer setting

The editor has a **Query analyzer** select, and the documentation says it is the analyzer used when executing queries.
`ElasticsearchService.SearchAsync` passes `indexMetadata.GetQueryAnalyzerName()`, which returns the analyzer of the index
(**Analyzer**), not this setting; nothing else reads it. The narration names neither the label nor an effect.

## 7. The environment of the stand-in

- With `HTTP_PROXY` and `HTTPS_PROXY` pointing at a dead address, does .NET reach `http://localhost:9200`? The plan adds
  `NO_PROXY=localhost,127.0.0.1`. Confirm that the site reaches the server (the **Add index** step succeeds).
- The authentication type defaults to Basic, and no user name or password is set. Confirm that the client accepts this
  against a server with security turned off (`ElasticsearchClientFactory.cs`).
- The docs' compose file uses a given server version; confirm the Elasticsearch client library of the checkout accepts
  it (otherwise use the version the checkout's package reference expects).
- With the connection set before the feature is enabled, `enable-2` must not show an error page.

## 8. The seed reaches the index

The index is filled from the log of indexing tasks, which holds only the content saved while Indexing was enabled. The
import of the four articles creates those tasks (the import runs the created and published handlers). Confirm with the
`_count` check of the plan after `03-create`. The index covers the Article type, and the Blog recipe creates one
Article of its own, **About**, so the expected count is 5 (the four seed articles and **About**). Whether **About** has an
indexing task depends on whether the content handler that logs the tasks was active when the recipe created it: read from
`CreateIndexingTaskContentHandler.cs` and `ContentStep.cs`, not run. A count of 4 is acceptable; the search for `harvest`
does not depend on **About**, whose text is placeholder text.

## 9. The look of the search results

`Search-Results.cshtml` shows each content item with its Summary display. Confirm that the Article summary of the Blog
theme shows its title (as a link) and some of its body, and that the word `harvest` finds exactly two articles. Confirm
that `/search` has no page title and shows the placeholder "Enter your search term(s)".

## 10. The order of the blocks on the create page

Two blocks of the editor sit at the same position (the content block with Culture, Content types and Index Latest
version, and the Elasticsearch block with Analyzer and Store Source Data), so the source does not fix their order. The
plan scrolls to whichever comes first. If **Analyzer** comes before **Content types**, record `create-4` and `create-5`
in that order and tell the narration's author: the steps keep their ids, so only the text order changes.

## 11. A visitor who is not signed in

`search-4` says a signed-out visitor can search only when their role may query the index, through **Query any index** or
**Query 'articles' Index**. Read from `SearchController.cs` (the authorization of `QuerySearchIndex` against the index
profile) and `IndexingAuthorizationHandler.cs`. Confirm: open `/search` in a private window; the visitor is sent to the
sign-in page, and the permissions appear on the page of the Anonymous role under Security, Roles.

## 12. The search type that the default option stores

In `create-6` the narration says only what the select shows: **Search type** starts as **Multi-Match Query (Default)**. The
option has the empty string as its value (`ElasticsearchIndexProfileDisplayDriver.cs:73`), the save stores the posted value
in `ElasticsearchDefaultQueryMetadata.SearchType` (`:134`), and `GetSearchType()` returns `query_string` when that value is
null (`ElasticsearchDefaultQueryMetadata.cs:13-21`). ASP.NET Core binds an empty form value to null for a string unless the
property says otherwise, and no override was found in the source. So the search page may run a Query String Query, not a
Multi-Match Query (`ElasticsearchService.cs:136` and `:148`). This is code reading, not run. To confirm on a running site:
create the index, search for `harvest`, and read which query the server receives (for example with the debug mode of the
connection in the site configuration). The results of the video are the same with either query.

## 13. Articles published after the index exists

`search-3` says an article published later is added to the index when it is published. Read from
`IndexingContentHandler.cs`: on create, update, publish and unpublish, a task at the end of the request deletes the
document and adds it again in every content index whose content types include the type (and whose culture and
published-or-latest setting match). It does not need the Indexing Worker. To confirm on a running site, outside the
capture: publish a new Article with the word `harvest` in its title, wait a few seconds, search again, and expect a new
result. Delete that article and restore the snapshot afterwards.

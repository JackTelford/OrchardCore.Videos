# Open questions: Lucene and Elasticsearch Queries

Every label and behavior in the narration was read from the Orchard Core source at commit `92db4a83`. Nothing was run:
no site, no browser, no Elasticsearch. A person who records the video confirms the points below on a running site before
the first take. "Checked" says what was read; "Confirm" says what to look at.

## 1. Behavior derived from reading code

1. **The Run button of the Queries list runs the query at once.**
   Checked: `LuceneQuery.Buttons.SummaryAdmin.cshtml` links to `QueryIndex` with the template (Base64) and the index name;
   `AdminController.QueryIndex` (`OrchardCore.Lucene/Controllers/AdminController.cs:71-86`) calls the POST overload
   `Query(AdminQueryViewModel)`, which searches, and passes no parameters (`model.Parameters` becomes `{ }`).
   Confirm: after the click, the table is already on the page, with no need to click **Run Query**; `save-4` relies on it.
   The same for `ElasticsearchQuery` (`OrchardCore.Elasticsearch/Controllers/AdminController.cs:92-111`).
2. **`{{ limit | default: 3 }}` renders `3` when no parameter is passed and `2` for `{ "limit": 2 }`.**
   Checked: the template is rendered with `ILiquidTemplateManager.RenderStringAsync` and the parameters become Liquid
   values (`OrchardCore.Lucene.Core/Services/LuceneQuerySource.cs:78`, `OrchardCore.Lucene/Controllers/AdminController.cs:181`); `default` is a standard Liquid filter of Fluid.
   Confirm: both runs (`params-1`, `params-2`) return valid JSON; a parameter written as a JSON number is read as a number
   by `JConvert.DeserializeObject<Dictionary<string, object>>`, so `2` is output without quotes.
3. **Counts differ between the two sources.**
   Checked: on the Lucene page `Count` is `TotalHits`, the number of all matching documents
   (`OrchardCore.Lucene.Core/LuceneQueryService.cs:97`), while the table holds `size` rows; on the Elasticsearch page
   `Count` is the number of hits returned (`ElasticsearchIndexManager.cs:520`), and `TotalCount` (line 521) is not shown.
   Confirm: `Found 5 result(s)` with three rows on Lucene, `Found 3 result(s)` with three rows on Elasticsearch. The
   narration states no count; only the recording plan does.
4. **`Return Content Items` on Elasticsearch needs the source data.**
   Checked: `ElasticsearchQuerySource.cs` reads `ContentItemVersionId` from each hit's `_source`;
   `ElasticsearchContentIndexProfileHandler.cs:43,66-75` maps `_source` and the keyword fields only when
   `StoreSourceData` is true (default `true`, `ElasticsearchIndexMetadata.cs:10`). Lucene always stores
   `ContentItemId` and `ContentItemVersionId` (`LuceneIndexManager.cs:264-265`).
   Confirm (optional, not in the video's clips): a saved Elasticsearch query with **Return Content Items** returns content
   items from `api/queries/PublishedBlogPostsElastic`.
5. **Save Query does not carry the index.** Checked: the link in `Admin/Query.cshtml:64` passes only `id` and `query`, and
   `LuceneQueryDisplayDriver.EditAsync` reads only `Query` from the request. Confirm: **Index** shows `Select an index` on
   the create page (`save-1`, `elastic-2`).

## 2. Things that need a running site

1. **The admin menu.** `run-1` opens **Search, Queries** in the admin menu; the menu has several behaviors (focused or
   not, collapsed or not) that the source does not settle. Use `Recorder probe` to find the real selectors, and open the
   menu on camera in the way the site shows it. The text of the entries is checked
   (`OrchardCore.Lucene/AdminMenu.cs:18-20`, `OrchardCore.Queries/AdminMenu.cs:18-22`).
2. **CodeMirror.** The query editors come from `CodeMirror.fromTextArea` (`lucene-query.js`, bundled from
   `@orchardcore/bloom/components/codemirror-query-editor`, which is not in the source folder). `setValue` on the
   `.CodeMirror` element's `CodeMirror` property is the CodeMirror 5 API; confirm the property exists, and that the form
   posts the edited value (it should: `fromTextArea` saves on submit).
3. **The seed.** Confirm that **Tools, Recipes** lists **Blog - Lucene Search** on a site made with the Blog recipe (the
   list keeps the recipes of the extensions that are available, `OrchardCore.Recipes/Controllers/AdminController.cs:160-168`), that it
   runs without error (it enables `OrchardCore.Search` and `OrchardCore.Lucene`, creates the index, sets the search settings
   and a role permission, and rebuilds the index), and the number of documents that **Run Lucene Query** reports with the
   Match All query. The plan expects 9; any stable number is fine because the narration states none.
4. **Creating the blog posts.** The path (**Blog** in the admin menu, then **Create Blog Post**) comes from
   `ListPartNavigationAdmin.cshtml:52`, which shows that button when the list holds one content type. Confirm it, and how
   the Markdown body editor accepts typed text.
5. **Elasticsearch behind the Docker stand-in.** Checked: `ElasticsearchClientFactory.cs:25` builds
   `BasicAuthentication(Username, Password)` unless another authentication type is set, so the plan always sets
   throwaway `Username` and `Password` values; the compose file of the documentation disables security
   (`xpack.security.enabled=false`), so the container should ignore them. Confirm that the client connects and that
   **Run Elasticsearch Query** finds the index. Confirm that the `NO_PROXY` setting keeps the dead proxy away.
6. **The import recipe for `blogelastic`.** It is `blog.elastic.search.recipe.json` with another name, in the **JSON Import**
   box. Confirm that the `feature` step and the index step can share one recipe (the Blog recipes do this), and that the
   lowercase name `blogelastic` is accepted.
7. **Term queries on `Published`.** The Lucene snippet compares `Content.ContentItem.Published` with the string `"true"`
   and the Elasticsearch snippet with the boolean `true`; both are the snippets of `TheBlogTheme/Recipes/Snippets`.
   Confirm that each returns the five blog posts on its own source (`Found 5` on Lucene).
8. **The results table.** Lucene shows every stored field, so the table is wide; confirm that the first columns and the
   `Save Query` button are readable at 1536x864 without scrolling, or highlight the line above the table.
9. **The height of the query editor page.** The **Schema** editor on the create page has a minimum height of 300 px
   (`OrchardCore.Queries/Views/Query.Fields.Edit.cshtml:17`), and **Index**, **Return Content Items** and **Query** follow
   it (`Content:1` before `Content:5`, `QueryDisplayDriver.cs:63`, `LuceneQueryDisplayDriver.cs:72`). Confirm at 1536x864
   whether `save-1`, `save-3` and `elastic-2` show those fields without scrolling; if they do not, scroll with the mouse
   wheel until the field of the narration is on screen, and note the amount in this plan.
10. **The Queries list with the Elasticsearch query.** `elastic-3` expects the rows `RecentBlogPosts`
    (`SQL query`), `PublishedBlogPosts` and `PublishedBlogPostsElastic` in the List layout. Checked: the list does not
    sort (`QueryContext.Sorted` stays `false`, `OrchardCore.Queries/Controllers/AdminController.cs:80-83`), so the rows
    should follow the order in which the queries were created. Confirm that order and that all three fit on screen.

## 3. Pronunciation

A person checks these with `scripts/pronounce.py` before the voice is generated: "Lucene", "Elasticsearch", "JSON",
"Blog - Lucene Query" and "Blog - Elasticsearch Query" (the hyphen), "Url" (the narration writes `Url`, the label of the
configuration key), and the four entries of `"spoken"` in `storyboard.json` ("Content Item slash Blog Post" for
`ContentItem/BlogPost`; "Return Content Items" for `ReturnContentItems`; "Recent Blog Posts" for `RecentBlogPosts`;
"U.R.L." for `Url`).

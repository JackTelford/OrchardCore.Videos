# Recording plan: Lucene and Elasticsearch Queries

How to record this video without deciding anything. The narration is in `storyboard.json`; this file says what the
site holds, what each clip does on screen, and what must be visible when each step's narration ends. A selector below
stands for `scene.Page.Locator(...)`. Every label was read from the Orchard Core source at commit `92db4a83`
(`src/OrchardCore.Modules/...`); the `file:line` after a label says where. Things nobody could check without a running
site are in `OPEN-QUESTIONS.md`: check them first, with `Recorder probe`.

The video is for developers. Its slides carry the code; the recorded clips show only browser screens. The video has
four clips (`02-run`, `03-params`, `04-save`, `06-elastic`) and five slide clips.

## 1. The site it needs

| Setting | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout |
| Provisioning | Auto Setup, tenant `Default`, recipe **Blog**, site name `Orchard Core`, time zone `America/Los_Angeles`, database `Sqlite` |
| User | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Data | `ORCHARD_APP_DATA` in an empty folder outside both repositories |
| Port | `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, **and** `NO_PROXY=localhost,127.0.0.1`, so the site can still reach the local Elasticsearch |
| Elasticsearch connection | environment variable `OrchardCore__Search__Elasticsearch__Url=http://localhost` set before the site starts (the section is `OrchardCore:Search:Elasticsearch`, `ElasticsearchConnectionOptionsConfigurations.SectionName` in `OrchardCore.Elasticsearch.Core/Services/ElasticsearchConnectionOptionsConfigurations.cs:13`; `Ports` defaults to `9200` there, line 31). Nothing else is needed while the Elasticsearch feature is disabled |

The Blog recipe enables `OrchardCore.Queries`, `OrchardCore.Queries.Sql`, `OrchardCore.Indexing`, `OrchardCore.Deployment`
and `TheBlogTheme`, and creates the SQL query `RecentBlogPosts` (`TheBlogTheme/Recipes/blog.recipe.json:1180`). It does
not enable Lucene or Elasticsearch.

The admin list layout stays at its default (List layout, Buttons actions): `AdminListOptions.DefaultLayout` and
`DefaultActionsLayout` in `OrchardCore.Admin.Abstractions/AdminListOptions.cs`. The List layout shows **Run**, **Edit**
and **Delete** as buttons and the source as a badge.

### Seed data, created outside the capture

All data is invented. Do the seed in this order.

1. **Four more blog posts.** Open the **Blog** entry of the admin menu (added by the Blog recipe) and use the
   **Create Blog Post** button of the blog's page (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52`) once for
   each post below, in this order, so the first one created is the oldest. Type the title, type the sentence into the
   body, click **Publish** (`OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`).

   | Title | Body |
   |---|---|
   | `A quiet morning at the harbor` | `The fishing boats leave before sunrise, and the town wakes to the sound of their engines.` |
   | `Five trails worth the early start` | `Each of these trails is at its best before nine in the morning.` |
   | `Notes from a week without a phone` | `Seven days offline changed how I plan a trip.` |
   | `How the lighthouse keeper kept time` | `Before radio signals, the keeper wound the clock by hand every night.` |

   With the blog post of the Blog recipe, the site now has five published Blog Post items.
2. **The Lucene index.** Open **Tools, Recipes** (`OrchardCore.Recipes/AdminMenu.cs:18-19`), find the recipe
   **Blog - Lucene Search** (`displayName` in `blog.lucene.search.recipe.json`), click **Run** and answer **Yes**
   (`RecipeEntry.Buttons.SummaryAdmin.cshtml:14-15`). Do **not** run **Blog - Lucene Query**: it replaces `RecentBlogPosts`,
   which is what the video builds by hand. The recipe enables `OrchardCore.Search` and `OrchardCore.Lucene`, creates the
   index `Search` and rebuilds it.
3. **Wait for the index.** Open **Search, Queries, Run Lucene Query**, choose the index `Search`, click **Run Query**
   with the Match All query. Repeat until the line above the table is stable. Expect `Found 9 result(s)` (menu, blog,
   five blog posts, About article, footer); the narration states no count, so another stable number is fine, but write
   it down.
4. Check that the Queries list (**Search, Queries, All Queries**) holds one query, `RecentBlogPosts`.

Then stop the site and copy `App_Data` aside as **snapshot S0**, with `state.json`.

## 2. Order of the clips, and what each one changes

Record in this order. `02-run` and `03-params` change nothing: repeat them as often as needed. `04-save` and
`06-elastic` change the site.

| Order | Clip | Starts from | Changes the site | After a good take |
|---|---|---|---|---|
| 1 | `02-run` | S0 | no | |
| 2 | `03-params` | S0 | no | |
| 3 | `04-save` | S0 | creates the Lucene query `PublishedBlogPosts` | stop the site, copy `App_Data` as **snapshot S1** |
| 4 | prepare Elasticsearch (section 3) | S1 | creates the index `blogelastic` | stop the site, copy `App_Data` as **snapshot S2** |
| 5 | `06-elastic` | S2 | creates the Elasticsearch query `PublishedBlogPostsElastic` | |

`06-elastic` is the only clip that needs Docker, and it depends on `04-save`: its last step shows both queries in the
list. To record `04-save` again, restore S0, record it, repeat section 3, take a new S2. To record `06-elastic` again,
restore S2. When you restore S0 or S1, also remove the Elasticsearch container's volume (section 3) so the index is
created again.

## 3. Prepare Elasticsearch (between `04-save` and `06-elastic`)

1. Start Elasticsearch with the Docker Compose file of the documentation,
   `src/docs/reference/modules/Elasticsearch/docker-compose.yml`, services `es01` and `es02` only (not Kibana):
   `docker compose -f <OrchardCore checkout>/src/docs/reference/modules/Elasticsearch/docker-compose.yml up -d es01 es02`.
   The file disables security and publishes port `9200`. On Windows with WSL2, apply the `vm.max_map_count` setting
   described in `src/docs/reference/modules/Elasticsearch/README.md` first.
2. Wait until `http://localhost:9200` answers (`curl http://localhost:9200` returns JSON with a `cluster_name`).
3. Start the site with `OrchardCore__Search__Elasticsearch__Url=http://localhost` (section 1) and with
   `OrchardCore__Search__Elasticsearch__Username` and `OrchardCore__Search__Elasticsearch__Password` set to throwaway
   values chosen outside git. The client builds Basic credentials from these two values unless another authentication
   type is set (`ElasticsearchClientFactory.cs:25`), and security is disabled in the container, so any value is
   accepted. Neither value appears on screen.
4. Open **Tools, Deployments, JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:30`), paste the recipe below into the
   **JSON** box and click **Import** (`Views/Import/Json.cshtml:15,21`). It enables the Elasticsearch feature and
   creates and rebuilds an index named `blogelastic`. It is `blog.elastic.search.recipe.json` with another name and
   without the settings and role steps: the name differs from `Search` so that no index name is shared between the
   two sources.

   ```json
   {
     "steps": [
       { "name": "feature", "enable": [ "OrchardCore.Elasticsearch" ] },
       {
         "name": "CreateOrUpdateIndexProfile",
         "indexes": [
           {
             "IndexName": "blogelastic",
             "ProviderName": "Elasticsearch",
             "Type": "Content",
             "Name": "blogelastic",
             "Properties": {
               "ElasticsearchIndexMetadata": { "StoreSourceData": true, "AnalyzerName": "standard" },
               "ElasticsearchDefaultQueryMetadata": {
                 "QueryAnalyzerName": "standard",
                 "DefaultSearchFields": [ "Content.ContentItem.FullText" ]
               },
               "ContentIndexMetadata": {
                 "IndexLatest": false,
                 "IndexedContentTypes": [
                   "Article", "Blockquote", "Blog", "BlogPost", "Container", "Image",
                   "LinkMenuItem", "Menu", "Page", "Paragraph", "RawHtml"
                 ],
                 "Culture": "any"
               }
             },
             "IndexLatest": false,
             "Culture": "any",
             "IndexedContentTypes": [
               "Article", "Blockquote", "Blog", "BlogPost", "Container", "Image",
               "LinkMenuItem", "Menu", "Page", "Paragraph", "RawHtml"
             ]
           }
         ]
       },
       { "name": "RebuildIndex", "includeAll": false, "indexNames": [ "blogelastic" ] }
     ]
   }
   ```
5. Wait for the index: open **Search, Queries, Run Elasticsearch Query** (the entry exists only now that the feature is
   enabled and `Url` is set, `OrchardCore.Elasticsearch/AdminMenu.cs:24`), choose `blogelastic`, click **Run Query** with
   the Match All query until the count is stable (expect `Found 9 result(s)`).
6. Stop the site and copy `App_Data` as snapshot S2.

After the last take: `docker compose -f <...docker-compose.yml> down -v`, delete the throwaway values, and delete the
snapshots.

## 4. Texts used in the clips

Set the contents of a query editor by script, not by typing: the editors are CodeMirror editors made with
`CodeMirror.fromTextArea` (`OrchardCore.Lucene/wwwroot/Scripts/lucene-query/lucene-query.js`), which re-indents what is
typed. Each editor is a `.CodeMirror` element holding its `CodeMirror` instance, so the call is

```csharp
await scene.Page.EvaluateAsync("([i, t]) => document.querySelectorAll('.CodeMirror')[i].CodeMirror.setValue(t)",
    new object[] { index, text });
```

then wait 1.2 s so the text can be read. On **Run Lucene Query** and **Run Elasticsearch Query** the Query editor is
`.CodeMirror` number 0 and the Parameters editor number 1. In the query editors of the Queries module it is number 0.

**Lucene template** (`LUCENE_TEMPLATE`), the query of `TheBlogTheme/Recipes/Snippets/recentBlogPosts.Lucene.json` with
a parameter:

```
{
  "query": {
    "bool": {
      "must": [
        { "term": { "Content.ContentItem.ContentType": "BlogPost" } },
        { "term": { "Content.ContentItem.Published": "true" } }
      ]
    }
  },
  "sort": [
    { "Content.ContentItem.CreatedUtc": { "order": "desc" } }
  ],
  "size": {{ limit | default: 3 }}
}
```

**Elasticsearch template** (`ELASTIC_TEMPLATE`), the query of `recentBlogPosts.Elasticsearch.json` with the same
parameter:

```
{
  "query": {
    "bool": {
      "must": [
        { "term": { "Content.ContentItem.ContentType": "BlogPost" } },
        { "term": { "Content.ContentItem.Published": true } }
      ]
    }
  },
  "sort": [
    { "Content.ContentItem.CreatedUtc": "desc" }
  ],
  "size": {{ limit | default: 3 }}
}
```

Names of the saved queries: `PublishedBlogPosts` (Lucene) and `PublishedBlogPostsElastic` (Elasticsearch). A name takes
ASCII letters and digits only, starts with a letter, and must not exist (`QueryDisplayDriver.cs:93,101`):
`RecentBlogPosts` exists already, so never use it.

## 5. The clips

### `02-run`: Run a Lucene query (chapter 02)

Starts on `/Admin` (the dashboard), signed in with `state.json`, with the admin menu as the site shows it after
sign-in. Record the whole clip with no site change.

- `run-1`: **Actions.** Click **Search** in the admin menu to open it, then **Queries** if it is collapsed; highlight
  **Run Lucene Query**; click it with `ClickAndWaitForNavigationAsync`. Clear the highlight after the page loads.
  **On screen when the narration ends:** the page **Lucene Query** (breadcrumb), the **Index** drop-down on
  **Select an index**, the Query editor holding the Match All query with `"size": 10`, and the Parameters editor
  holding nothing. The admin menu is still open, showing **Search, Queries** with **All Queries**, **Run Lucene Query**
  and **Run SQL Query** (the Blog recipe enables `OrchardCore.Queries.Sql`, so the SQL entry is listed too; no
  **Run Elasticsearch Query** entry exists yet). **Labels:** `OrchardCore.Lucene/AdminMenu.cs:18-20`,
  `OrchardCore.Queries/AdminMenu.cs:18-22`, `OrchardCore.Queries/Sql/AdminMenu.cs:18-20`, breadcrumb
  `OrchardCore.Lucene/Views/Admin/Query.cshtml:14`.
- `run-2`: **Actions.** `SelectAsync` on the **Index** drop-down (`select#Id`) with the label `Search`. Highlight the
  Query editor for the last two seconds. **On screen at the end:** **Index** shows `Search`; the editor still shows
  `"match_all": { }` and `"size": 10`. **Labels:** `Query.cshtml:20,23,25,30`.
- `run-3`: **Actions.** Click **Run Query** (`button:text-is("Run Query")`) with `ClickAndWaitForNavigationAsync`; wait
  for `table.table tbody tr`; move the cursor over the first row; highlight the line above the table.
  **On screen at the end:** the line `Found 9 result(s) in N ms` (the count you wrote down in the seed; it is the number of
  matching documents), the **Save Query** button, and the table whose header row is `#` followed by stored field names,
  with ten rows at most (here nine). Under the table the note `The results displayed are coming from the stored fields
  in Lucene.` is below the visible area: do not scroll. **Labels:** `Query.cshtml:47,54,64,97`.

### `03-params`: Parameters (chapter 03, second clip)

Starts on `/Admin/Lucene/query`. Before the capture: choose the index `Search`, set the Query editor (number 0) to
`LUCENE_TEMPLATE`, leave Parameters empty. Do not click **Run Query** yet. Record with no site change.

- `params-1`: **Actions.** Highlight the `"size": {{ limit | default: 3 }}` line for two seconds (scroll the editor if the line is
  not visible), click **Run Query** with `ClickAndWaitForNavigationAsync`, wait for the table.
  **On screen at the end:** the template in the editor, `Found 5 result(s) in N ms`, and a table with exactly three
  rows, numbered 1 to 3. (`OrchardCore.Lucene.Core/LuceneQueryService.cs:97`: `Count` is the total of matching documents, the table holds
  `size` rows; see `OPEN-QUESTIONS.md`.) **Labels:** `Query.cshtml:47,54`.
- `params-2`: **Actions.** Set the Parameters editor (number 1) to `{ "limit": 2 }`, wait 1.2 s, click **Run Query** with
  `ClickAndWaitForNavigationAsync`, wait for the table. **On screen at the end:** Parameters holds
  `{ "limit": 2 }`, the line above the table reads `Found 5 result(s) in N ms`, and the table has exactly two rows.
  **Labels:** `Query.cshtml:39,42,47`.

### `04-save`: Save the query (chapter 04)

Starts on `/Admin/Lucene/query`. Before the capture: choose the index `Search`, set the Query editor to
`LUCENE_TEMPLATE`, click **Run Query**, wait for the table (three rows). The capture starts on the result page.
Changes the site: it creates `PublishedBlogPosts`. Restore S0 before another take. The create page is tall (the
**Schema** editor is 300 px high): settle with `Recorder probe` which fields are on screen without scrolling, and
how far to scroll for **Index** and **Return Content Items** in `save-3` (`OPEN-QUESTIONS.md`, 2.9).

- `save-1`: **Actions.** Highlight **Save Query** (`a:text-is("Save Query")`), then click it with
  `ClickAndWaitForNavigationAsync`; clear the highlight. **On screen at the end:** the editor page of a Lucene query,
  titled `New Lucene query` in its breadcrumb under **Queries**, with **Name** empty, **Schema** (an empty JSON
  editor), **Index** on `Select an index`, **Return Content Items** unchecked, and the **Query** editor holding the
  template. **Labels:** `OrchardCore.Lucene/Views/Admin/Query.cshtml:64`, `OrchardCore.Queries/Views/Admin/Create.cshtml:4-5`,
  `OrchardCore.Queries/Views/Query.Fields.Edit.cshtml:4,14`, `OrchardCore.Lucene/Views/LuceneQuery.Edit.cshtml:9,22,29`.
- `save-2`: **Actions.** `TypeAsync` into the **Name** box (`.ocat-wrapper:has(> label:text-is("Name")) input.form-control`;
  do not use `input[name$="Name"]`, which also matches the hidden `SourceName` input of `Create.cshtml:16`) the text
  `PublishedBlogPosts`, delay 40 ms; highlight the hint under it.
  **On screen at the end:** **Name** holds `PublishedBlogPosts` and the hint `The technical name of the query.` is
  visible. **Labels:** `Query.Fields.Edit.cshtml:4,8`.
- `save-3`: **Actions.** `SelectAsync` on the **Index** drop-down (`.ocat-wrapper:has(> label:text-is("Index")) select`)
  with the label `Search`; click the **Return Content Items** check box
  (`.form-check:has(label:text-is("Return Content Items")) input`); highlight it with its hint.
  **On screen at the end:** **Index** shows `Search`, **Return Content Items** is checked, with its hint `Check to
  return the corresponding content items.` **Labels:** `LuceneQuery.Edit.cshtml:9,12,14,22,23`.
- `save-4`: **Actions.** Click **Save** (`button.btn.save`) with `ClickAndWaitForNavigationAsync`; wait 2.5 s on the
  Queries list; click **Run** in the row of `PublishedBlogPosts`
  (`.list-group-item:has-text("PublishedBlogPosts") a:text-is("Run")`) with `ClickAndWaitForNavigationAsync`; wait for
  `table.table tbody tr`. **On screen at the end:** the page **Lucene Query**, **Index** on `Search`, the Query editor
  holding the saved template, Parameters holding `{ }`, `Found 5 result(s) in N ms`, and a table with three rows.
  Between the two page loads the list showed the notification `Query created successfully.`, the rows
  `RecentBlogPosts` and `PublishedBlogPosts`, and on the second a badge `Lucene query` with the buttons **Run**, **Edit**
  and **Delete**. **Labels:** `Query.Fields.Buttons.cshtml:3`, `OrchardCore.Queries/Controllers/AdminController.cs:162`,
  `LuceneQuery.SummaryAdmin.cshtml:2`, `LuceneQuery.Buttons.SummaryAdmin.cshtml:22`, `Query.Buttons.SummaryAdmin.cshtml`
  (**Edit**, **Delete**).

### `06-elastic`: Run and save an Elasticsearch query (chapter 06, second clip)

Needs Docker running and the site restored from S2 (section 3). Starts on `/Admin/Elasticsearch/query`, opened before
the capture. Changes the site: it creates `PublishedBlogPostsElastic`. Restore S2 before another take.

- `elastic-1`: **Actions.** `SelectAsync` on the **Index** drop-down with the label `blogelastic`; set the Query editor
  (number 0) to `ELASTIC_TEMPLATE`; wait 1.2 s; click **Run Query** with `ClickAndWaitForNavigationAsync`; wait for
  `table.table tbody tr`. **On screen at the end:** the page **Elasticsearch Query**, **Index** on `blogelastic`, the
  Elasticsearch template in the editor, `Found 3 result(s) in N ms` (the Elasticsearch count is the number of documents
  returned, `OrchardCore.Elasticsearch.Core/Services/ElasticsearchIndexManager.cs:520`), **Save Query**, the note `The results displayed in this table are
  coming from the _source data.`, and a table of three rows. **Labels:**
  `OrchardCore.Elasticsearch/Views/Admin/Query.cshtml:18,24,27,29,51,58,66,76`.
- `elastic-2`: **Actions.** Click **Save Query** with `ClickAndWaitForNavigationAsync`; `TypeAsync` into **Name** the
  text `PublishedBlogPostsElastic`, delay 40 ms. **On screen at the end:** the editor page of an Elasticsearch query,
  titled `New Elasticsearch query`, with **Name** `PublishedBlogPostsElastic`, **Index** on `Select an index`,
  **Return Content Items** unchecked and the Query editor holding the template. **Labels:**
  `OrchardCore.Elasticsearch/Views/ElasticQuery.Edit.cshtml:9,12,14,22,29`.
- `elastic-3`: **Actions.** `SelectAsync` on **Index** with `blogelastic`; click **Return Content Items**; click **Save**
  with `ClickAndWaitForNavigationAsync`; hold on the list. **On screen at the end:** the Queries list with three rows,
  `RecentBlogPosts` with the badge `SQL query`, `PublishedBlogPosts` with the badge `Lucene query`, and
  `PublishedBlogPostsElastic` with the badge `Elasticsearch query`, each with **Run**, **Edit** and **Delete** buttons
  (the SQL query has a **Run** button too) and the notification `Query created successfully.` **Labels:**
  `ElasticQuery.SummaryAdmin.cshtml:2`, `ElasticQuery.Buttons.SummaryAdmin.cshtml:21`,
  `OrchardCore.Queries/Views/SqlQuery.SummaryAdmin.cshtml:4`, `SqlQuery.Buttons.SummaryAdmin.cshtml:16`.

## 6. The slide steps

Each line says what the slide in `slides.py` shows. The slides are drawn by `slides.py`; nothing is recorded.

- `why-1`: three cards, "An index answers a JSON query", "Every caller keeps a copy", "A saved query keeps one copy".
- `why-2`: three cards, Lucene (inside your site), Elasticsearch (on a server), and the same fields.
- `tmpl-1`: the Lucene query of the Blog theme, `recentBlogPosts.Lucene.json`, on 14 lines.
- `tmpl-2`: the same query with `"size": {{ limit | default: 3 }}`, that line bright, and two note cards (`limit`,
  `default: 3`).
- `docs-1`: four cards; the first two bright: unchecked returns documents, and the Run pages always show documents.
- `docs-2`: the same four cards; the last two bright: checked loads content items by version ID, and Elasticsearch
  reads that ID from the source data.
- `connect-1`: the `OrchardCore`, `Search`, `Elasticsearch` section with `ConnectionType`, `Url` and `Ports`, and two
  note cards (`Url`, `Ports`).
- `differ-1`: two code panels, the Lucene and Elasticsearch versions of the query, with the `Published` line and the
  sort line bright in both.
- `recipe-1`: the `queries` step of `blog.lucene.query.recipe.json` with `Source`, `Name`, `Index`, `Template` and
  `ReturnContentItems`.
- `recipe-2`: three cards: `Blog - Lucene Query`, `Blog - Elasticsearch Query`, and examples that read their Template
  from a file.
- `recipe-3`: the Schema `{ "type": "ContentItem/BlogPost" }` and two note cards (`Optional`, `Content items`).

## 7. Checks for private data

- Everything on screen is the invented data of section 1: the five blog titles, `admin`, `admin@example.com`.
  No real name, email, phone number, domain or token; `localhost` is the only host.
- The password is never typed on camera (the session comes from `state.json`) and never written in a file.
- No machine path, user name of the computer, or Docker container id is visible: the Run pages show document fields only.
- OCR the final video at one frame per second and at every scene change, as `SKILL.md` asks, and search the text for
  `@`, `http`, `C:` and any name that is not in the seed.

# Open questions: Queries in Liquid, Razor and the API

What was checked in the Orchard Core source at commit `92db4a83` and in the documentation, and what a person must
confirm on a running site before the clips are recorded. Where the answer differs from the narration, the narration of
the named step changes; the rest of the video stays. Paths are relative to `src/OrchardCore.Modules`, except those that
start with `OrchardCore/`, `OrchardCore.Themes/` or `docs/`, which are relative to `src`.

## 1. The API ignores the sign-in cookie, and answers Not Found without the permission

- Checked: `QueryApiController` is `[Authorize(AuthenticationSchemes = "Api")]` and `[AllowAnonymous]`
  (`OrchardCore.Queries/Controllers/QueryApiController.cs:12-14`). It returns `NotFound()` when the caller lacks
  `ExecuteApi_<name>`, with a comment that this is intentional (`:52-56`). The `Api` scheme forwards to the bearer
  scheme only (`OrchardCore/OrchardCore.Infrastructure/Security/ApiAuthenticationHandler.cs`), and the OpenAPI documentation says API
  endpoints accept bearer tokens only and never use session cookies (`docs/reference/modules/OpenApi/README.md`, the
  section "API Authentication", line 34).
- To confirm: with the recorder signed in as `admin` (cookie) and nothing granted to the Anonymous role,
  `GET /api/queries/RecentBlogPosts` answers `404`; after `Execute Api - RecentBlogPosts` is allowed for the Anonymous
  role it answers `200` with JSON, in a browser holding the admin cookie and in `curl` without any header. What the
  browser shows for the `404` body (empty page, problem details, the theme's not-found page).
- Steps affected: `api-3`, `api-4`, `grant-2`, `call-1`.

## 2. The JSON of the response

- Checked: `SQLQueryResults` has the property `Items` only; `LuceneQueryResults` and `ElasticsearchQueryResults` add
  `Count` (`OrchardCore/OrchardCore.Search.Lucene.Abstractions/LuceneQueryResults.cs`, `OrchardCore/OrchardCore.Elasticsearch.Core/Models/ElasticsearchQueryResults.cs`).
  The controller returns `new ObjectResult(result)` (`QueryApiController.cs:71`). The Queries documentation writes the
  properties as `items` and `count`. No MVC JSON option that sets the naming policy was found in the source.
- To confirm: the property is written `items` (camel case), the three items are full content items, and a SQL query
  has no `count`. How the browser shows the response (one wrapped text block, or a JSON viewer).
- If the response is a single wrapped block of text, the viewer cannot count three or two items; decide with the
  recorder whether to tick a "Pretty-print" option of the browser's JSON view (if it offers one) in `call-1` and
  `call-2`. The plan names the request in a caption instead, because the address bar is not in the capture.
- Steps affected: `call-1`, `call-2`, `api-5` (the slide shows `items` and `count` as the documentation does).

## 3. Parameters of the right type reach the SQL

- Checked: the SQL parser takes a caller's value when it is in the parameter dictionary, and the default of
  `@name:default` otherwise (`OrchardCore.Queries/Sql/SqlParser.cs:257-266`). The query string and the body are
  deserialized into `Dictionary<string, object>` (`QueryApiController.cs:59-66`). The Liquid filter passes its named
  arguments (`Liquid/QueryFilter.cs`), the Razor helpers a dictionary (`Razor/QueryOrchardRazorHelperExtensions.cs`).
- To confirm: `?parameters={"limit":2}` returns two posts (`call-2`). The slides `liquid-2` and `razor-2` show
  `limit` set to five, as the documentation does for Liquid; they were not run. Confirm that a Liquid number and a C#
  `int` work as `LIMIT @limit` on SQLite (the database of the recording) and on SQL Server, MySQL and PostgreSQL.

## 4. The front-end page prints the titles

- Checked: `OrchardCore.Themes/TheBlogTheme/Views/Content-BlogPost.liquid:34-37` loops over `Queries.RecentBlogPosts | query` and prints
  `{{ item | display_text }}` after the post's parts. The theme is the site theme of the Blog recipe.
- To confirm: the three titles of the seed posts appear at the bottom of `/blog/planning-the-autumn-menu`, newest first,
  and how they are laid out (the template puts no separator between them, so they may run together on one line).
  Step `post-1` says "lists the titles of the three most recent posts below the post" and nothing about the layout.
- Also to confirm: the order of the items in `items` (`call-1`, `call-2`) and of the titles on the page. The SQL query sorts
  the document ids (`ORDER BY CreatedUtc DESC`), then `SqlQuerySource` loads the documents with `ISession.GetAsync`
  (`OrchardCore.Queries/Sql/SqlQuerySource.cs:137-140`). The YesSql source is not part of the checkout, so whether
  `GetAsync` returns the documents in the order of the ids was not verified. The narration names no order. The plan
  expects newest first.
- The query has no condition on **Published** or **Latest**, and the content item index holds one record per stored
  version, so the three titles hold only while every seed post has one version (see the seed section of
  `recording-plan.md`). The documentation shows the same query with `AND Published = 1`.

## 5. The admin screens of `01-queries` and `04-grant`

- Checked in the views: the menu labels (**Search**, **Queries**, **All Queries**; **Access Control**, **Roles**), the
  **Add Query** button, the **Edit 'RecentBlogPosts' query** title, **Return Documents**, **Query**, **Save**, **Search
  permissions**, **Allow**, and the checkbox id `Checkbox.ExecuteApi_RecentBlogPosts`.
- To confirm: how the admin menu expands in the admin theme at 1536 px (the selector of `queries-1`); whether the
  Queries list shows the **SQL query** badge or the **Source** column by default; that the query text ends with
  `LIMIT @limit:3` and is fully visible after the scroll of `queries-2`; that the Roles list shows the Anonymous
  role with an **Edit** button; that the permission appears under the group **Queries** with the description
  **Execute Api - RecentBlogPosts** (the role editor groups permissions with no category by feature name,
  `OrchardCore.Roles/Controllers/AdminController.cs:412-427`).
- Steps affected: `queries-1`, `queries-2`, `grant-1`, `grant-2`.

## 6. Creating the seed posts

- Checked: the Blog list offers **Create Blog Post** (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52`) and
  the editor has **Title** (`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`) and a Markdown body; **Publish** is in
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml`. The autoroute pattern of a blog post is the container's
  display text and its own, slugified (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:216`).
- To confirm: the Blog Post editor saves with only a title and a body, and the three posts get the paths
  `blog/notes-from-the-allotment`, `blog/a-week-of-slow-travel` and `blog/planning-the-autumn-menu`.
- To confirm: the query in Run SQL Query on `ContentItemIndex` (the check in `recording-plan.md`) shows one row per post.
  Publishing a new item from the editor is expected to store one version, so a post has one index record.

## 7. Behaviours stated from the code and not run

- An unknown query name: `Queries.<name>` gives nil (`Startup.cs:47-57`, the member accessor) and the `query` filter returns
  nil for a value that is not a query (`Liquid/QueryFilter.cs`); `QueryAsync` returns `null` and `ContentQueryAsync`
  skips results without a content item id (`Razor/*.cs`; a result that is a JSON object with a `ContentItemId` is
  converted to a content item, `Razor/ContentQueryOrchardRazorHelperExtensions.cs:28-31`). Steps `liquid-4` and `razor-3`.
- `QueryAsync` and `ContentQueryAsync` read `HttpContext.RequestServices` of the injected helper
  (`Razor/QueryOrchardRazorHelperExtensions.cs:17`), so they run during a request. Step `razor-4` therefore says "in any
  other view".
- `DisplayAsync` with the default display type renders a post with its Detail template. A view placed in the post's own
  detail template (TheBlogTheme's `Content-BlogPost.liquid` or a Razor equivalent) would run the query again for each
  item it displays, so the Razor slides use an unnamed view and the `"Summary"` display type, which TheBlogTheme
  renders with `Content-BlogPost.Summary.liquid`. Step `razor-1`.
- `DisplayAsync` is an extension of `IOrchardDisplayHelper`, the type of the `Orchard` property of the Orchard Core
  Razor base class, not of `IOrchardHelper`; `razor-4` therefore says that the query methods, not `DisplayAsync`, extend
  `IOrchardHelper`. Confirm in a view that does not use the base class.

## 8. Pronunciation

Have a person listen to the narration before recording. The `spoken` map of `storyboard.json` already reads
`ContentQueryAsync`, `QueryAsync`, `DisplayAsync`, `IOrchardHelper` and `RecentBlogPosts` as separate words. Not checked
with `scripts/pronounce.py`: how the voice says "Api" in the permission label "Execute Api - ..." (`api-3`, `grant-2`; the `spoken` entry
"Execute Api - " makes the voice pause where the label has its hyphen, and the subtitles keep the hyphen), "nil"
(`liquid-4`), "ID" (`razor-3`),
"Lucene" and "Elasticsearch" (`api-5`), and "null" (`razor-3`). Add a `spoken` entry only after a person confirms it.

## 9. Left out on purpose

The `executeQuery` and `executeQueryAsync` script functions, the GraphQL schema of a query, the recipe step and custom
query sources are not in this video. The behaviour of `executeQuery` when no parameters are passed (its second argument
is cast to a dictionary, `QueryGlobalMethodProvider.cs:41`) was not verified.

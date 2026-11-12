# Open questions: Build a Headless Site

What the author could not verify from the Orchard Core source and documentation. None of it is stated in the narration as
a fact beyond what the code shows; each item says what was read and what a person must confirm on a running site (the
author ran nothing: no site, no recorder, no voice).

## The behavior the story depends on

1. **An anonymous visitor can run the saved query with only two grants.** Read: the middleware requires `ExecuteGraphQL`
   (`OrchardCore.Apis.GraphQL/GraphQLMiddleware.cs:71`); the field `recentBlogPosts` requires only
   `ExecuteApi_RecentBlogPosts` (`OrchardCore.Queries/Sql/GraphQL/SqlQueryFieldTypeProvider.cs:162`); no content permission is
   checked on the results of a saved query. Confirm: after clip `05-permissions`, the page of clip `07-after` lists the three
   posts, and a signed-out `POST /api/graphql` with the query of `app.js` answers `200` with the posts.
2. **The page fails before the policy exists.** Expected: with the CORS feature enabled and no policy, no CORS header is
   sent (`OrchardCore.Cors/Services/CorsOptionsConfiguration.cs:20-23`), so the browser blocks the `POST` that carries
   `Content-Type: application/json`, and the page prints `The request failed: Failed to fetch`. Confirm the wording in the
   recorder's Chromium, and that the `OPTIONS` preflight is answered by the CORS middleware once the policy exists:

   ```bash
   curl -i -X OPTIONS "http://localhost:5010/api/graphql" \
     -H "Origin: http://localhost:5173" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: content-type"
   ```

   The answer must carry `Access-Control-Allow-Origin: http://localhost:5173`. If the preflight is not answered, the
   narration of `cors-6` ("the browser asks the site if this address is allowed, the policy answers") needs a check.
3. **Anonymous already has "View all content" with its Allow box checked.** Read: the Contents module gives the permission to
   the Anonymous stereotype (`OrchardCore.Contents/Permissions.cs:91-95`) and the Roles module applies stereotypes to roles
   (`OrchardCore.Roles/Services/RoleUpdater.cs`). Confirm that the row shows **Allow** checked on a site made by the Blog
   recipe. If only **Effective** is checked, change the end of `perm-3` ("as its Allow box shows") and the plan.
4. **Field names in the Explorer.** `blogPost` and `recentBlogPosts` come from the camel-case name converter
   (`OrchardCore.Apis.GraphQL/OrchardFieldNameConverter.cs`) applied to the content type `BlogPost` and the query
   `RecentBlogPosts`; `displayText` and `publishedUtc` are declared in `ContentItemType.cs`. Confirm the four names in the
   Explorer. The video does not query a part (such as the Markdown body), because the documentation and the code disagree on
   part field names (see the documentation issues).

   **The saved query is a field only if the content type field exists first.** `BuildContentTypeFieldType` looks up the
   `BlogPost` field among the fields already in the schema (`SqlQueryFieldTypeProvider.cs:184-190`) and returns nothing when
   it is missing. `ContentTypeQuery` is registered by the Contents module (`OrchardCore.Contents/GraphQL/Startup.cs`,
   through `AddContentGraphQL`) and `SqlQueryFieldTypeProvider` by the SQL Queries module
   (`OrchardCore.Queries/Sql/GraphQL/Startup.cs`); the schema builders run in registration order
   (`OrchardCore.Apis.GraphQL/Services/SchemaService.cs`), which the source read here does not settle. The Queries
   documentation and the Blog recipe (a query whose schema names `ContentItem/BlogPost`) describe this case as working.
   Confirm on the first take, on a fresh Blog site, that `recentBlogPosts` is in the Explorer after clip `02-features`. If it
   is not, chapter 04 (`query-5`), the front end (`app.js`) and the clips from `04-query` on need another query or another
   source for the page.
5. **The `parameters` argument.** Narration `query-5` says it is "written as JSON text" and "passes values to the SQL
   parameters", which is what `SqlQueryFieldTypeProvider.cs:150-182` does. No query with `parameters` is run in the video; if
   one is added, check that a JSON number reaches `LIMIT @limit:3` as the Blog theme's widget does.
6. **The REST route.** `more-5` says a saved query is "also served over REST, by name": the route `api/queries/{name}` accepts
   `GET` and `POST` (`OrchardCore.Queries/Controllers/QueryApiController.cs:10-36`). The response shape and whether a cookie
   signs the request in were not checked, so neither is claimed.
7. **The recipe's SQL has no `Published` filter.** Narration `perm-3` says "its SQL decides what it returns". With the seed
   (published once, never edited, no draft) the results are the three posts. A site with drafts could return them too: not
   claimed in the video.

7a. **The `blogPost` field and an anonymous visitor.** Narration `perm-3` says the `blogPost` field returns every post to a role
   that has View all content. Read: the field is declared with the permission View own content for its type
   (`OrchardCore.ContentManagement.GraphQL/Queries/ContentTypeQuery.cs:69`), which View all content implies
   (`OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:31`), and `ContentItemFilters.cs:37` adds no restriction for a
   user who has View all content. The page of the video never calls `blogPost`. Confirm after clip `05-permissions`, signed out:
   `POST /api/graphql` with `{ blogPost(first: 3) { displayText } }` answers `200` with the posts.
7b. **The default policy of CORS.** The video checks Set as default policy and says the default policy is the one the CORS
   middleware applies. `OrchardCore.Cors/Services/CorsOptionsConfiguration.cs:84` reads like a fallback to the first policy when
   none is marked, but `CorsOptions.DefaultPolicyName` (ASP.NET Core, outside the source read here) is not null by default, so the
   fallback may never apply. The video does not rely on it. Confirm on a running site: save a policy without the box checked, then
   send the `OPTIONS` request of question 2; the answer should carry no `Access-Control-Allow-Origin` header.
7c. **The run button of GraphiQL.** The button is a play icon without text; its title is `Execute Query (Ctrl-Enter)`
   (`wwwroot/Scripts/graphiql-orchard.js`, the bundled `graphiql` 1.x). Narration `graphiql-2` calls it "the run button, titled
   Execute Query". Confirm on the first take that the title is readable (a tooltip is not part of a screencast) and, if it is
   not, hover the button long enough, or rephrase the step after looking at the capture.

## Seed data and the front end

8. **JSON Import of the seed** (`recording-plan.md`) was not run. Unchecked: the `content` step with a `Data` array, as the
   Blog recipe writes it, accepts the three items without a `ContainedPart`, `Owner` or dates; the import stamps `CreatedUtc`
   and `PublishedUtc` with the time of the import (`UpdateContentsHandler.cs`, `DefaultContentManager.cs` around line 1000),
   so the three posts are the newest and show the same date on the page. The **JSON** field is a CodeMirror editor: setting
   its value from a script is a guess (`import-json.js` was not read). The plan has a fallback.
9. **The front-end page.** Opened with `scene.Page.GotoAsync` on the recorder's page, in the signed-in context. The `fetch` of
   `app.js` is cross-origin and sends no cookies, so it is anonymous. Confirm in the first take that the page does not show
   the signed-in user's data, and that opening a second origin does not disturb the screencast (the recorder's
   `NavigateAsync` restarts the screencast after a navigation; a plain `GotoAsync` before `StartCaptureAsync` should not need it).

## Selectors

10. **Admin menu entries** (Tools, Search, Access Control, Settings and their children) are driven by visible text; the markup
    of the admin theme was not read. The plan gives a `NavigateAsync` fallback for each.
11. **GraphiQL** is the bundled `graphiql` package (`Assets/package.json`), and its markup was read only through strings of
    `wwwroot/Scripts/graphiql-orchard.js`: `.execute-button` (title `Execute Query (Ctrl-Enter)`), `.result-window`,
    `.docExplorerShow` (text **Docs**). The Explorer rows (`blogPost`, `recentBlogPosts`, the arguments) come from
    `graphiql-explorer`; find their selectors on the first take. The video never types in the GraphiQL editor: the query
    comes from `?query=` (`Assets/js/App.tsx:46`).
12. **The CORS editor** is a Vue component (`OrchardCore.Cors/Assets/ts/cors-admin-index.ts`); the ids and labels in the plan
    are from that file, and the compiled script in `wwwroot` has the same ids. The text boxes have no id: the plan locates
    them through their label.
13. **Query editors.** The Schema editor is Monaco and the Query editor is CodeMirror: both are only read in clip `04-query`.

## Voice

14. **Spellings in the storyboard's `spoken` map** need a person's confirmation with `scripts/pronounce.py`, which was not
    run (it writes audio): `GraphiQL` as "Graphical", `Execute Api - RecentBlogPosts` as "Execute A.P.I. dash Recent Blog
    Posts", `blogPost`, `recentBlogPosts`, `orderBy`, `publishedUtc` as "published U.T.C.", `Content-Type` as "Content Type".
    Also check how the voice reads `CORS` (as a word, or letter by letter), `GraphQL`, `SQL`, `OpenID` and `OAuth`; add a
    `spoken` entry only if one is misread.

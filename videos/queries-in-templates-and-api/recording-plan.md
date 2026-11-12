# Recording plan: Queries in Liquid, Razor and the API

A developer video. Most of it is slides (`slides.py`); four clips are browser recordings: `01-queries`, `02-post`,
`04-grant` and `04-call`. Everything below was read from the Orchard Core source at commit `92db4a83`; the
`file:line` references are in `src/OrchardCore.Modules` unless the path starts with `OrchardCore/` or
`OrchardCore.Themes/`, which are relative to `src`. A bare `blog.recipe.json` is
`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`. Open questions that need a running site are in `OPEN-QUESTIONS.md`.

In this plan a selector stands for `scene.Page.Locator(...)`.

## The site it needs

- A local site, built from `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on
  `http://localhost:5010`, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- Provisioned by Auto Setup, so no setup form is recorded (see `references/recording.md` of the skill). Environment
  variables, with the password chosen outside git:

  ```
  OrchardCore__AutoSetup__AutoSetupPath=
  OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  OrchardCore__AutoSetup__Tenants__0__SiteName=Orchard Core
  OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
  OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

- Block outbound calls: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, in the environment of the site process
  only. No external service is needed. Every `curl` check below passes `--noproxy '*'`, so that a proxy variable in
  the shell of the person who records is not used for `localhost`.
- Tenant: Default. Time zone: `America/Los_Angeles`. Recipe: **Blog**.
- Features to enable before recording: none. The Blog recipe enables **Queries**, **SQL Queries**, **Roles**, **Lists**
  and **Templates**, and sets TheBlogTheme as the site theme (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`,
  the `feature` step and the `themes` step). Leave every other feature as the recipe leaves it. In particular, do not
  enable the OpenID Connect features: the API clip relies on a request without a token.
- The query. The Blog recipe saves the query `RecentBlogPosts` (SQL source, Return Documents on):
  `SELECT DocumentId FROM ContentItemIndex WHERE ContentType='BlogPost' ORDER BY CreatedUtc DESC LIMIT @limit:3`
  (`blog.recipe.json:1176-1185`). Do not edit it, and do not create other queries: the list in `01-queries` shows
  exactly one row.
- The template. TheBlogTheme's `Views/Content-BlogPost.liquid` (lines 34-37) runs the query with
  `Queries.RecentBlogPosts | query` and prints each `display_text`. Do not add a template of your own.

### Seed data, created outside the capture

Create three published blog posts, one after the other, in this order (the last one is the newest). Wait at least
five seconds between two of them, so their creation times differ.

| # | Title | Markdown body |
|---|-------|---------------|
| 1 | `Notes From the Allotment` | `Beans and courgettes went in the ground this week.` |
| 2 | `A Week of Slow Travel` | `Seven days, three villages, and no schedule.` |
| 3 | `Planning the Autumn Menu` | `Soups, roasted roots and a baked apple pudding.` |

For each post: sign in as `admin`, click **Blog** in the admin menu (a link the recipe adds,
`blog.recipe.json`, the `AdminMenu` step), click **Create Blog Post**
(`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52`), fill **Title** and the Markdown body (an EasyMDE editor,
`OrchardCore.Markdown/Views/MarkdownBodyPart-Wysiwyg.Edit.cshtml`), leave every other
field empty (if the editor refuses to save, fill the field it names with a short invented text), and click **Publish**
(`OrchardCore.Contents/Views/Content.PublishButton.cshtml`). The recipe's own post, "Man must explore, and this is
exploration at its greatest", stays as it is; it is the oldest, so it is not among the three newest.

Never edit, unpublish or delete a seed post after publishing it. The query has no condition on **Published** or
**Latest** (`blog.recipe.json:1181`). The content item index holds one record per stored version
(`OrchardCore/OrchardCore.ContentManagement/Records/ContentItemIndex.cs:32-37`), and deleting a post only clears its
**Published** and **Latest** flags (`OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:937-942`). So a
second version or a deleted post changes the three titles. After any mistake, stop the site, delete its `App_Data`
folder, and start again from an empty one.

Check the seed (outside the capture):

1. `http://localhost:5010/blog/planning-the-autumn-menu` opens the third post.
2. In `Admin`, Search, Queries, All Queries, open `RecentBlogPosts`: the query ends with `LIMIT @limit:3` and
   **Return Documents** is checked.
3. `curl --noproxy '*' -s -o /dev/null -w "%{http_code}\n" http://localhost:5010/api/queries/RecentBlogPosts` prints `404`:
   the Anonymous role has no permission to run the query yet. After `04-grant` it prints `200`.
4. In `Admin`, Search, Queries, Run SQL Query (`OrchardCore.Queries/Sql/AdminMenu.cs:20`), type
   `SELECT ContentItemId, DisplayText, Published, Latest FROM ContentItemIndex WHERE ContentType='BlogPost'` into **Template**
   (`Views/Admin/Query.cshtml:42`) and click **Query** (`:60`). The editor below the form shows a JSON array of four objects,
   one for each post, each with `Published` and `Latest` set to 1 on SQLite (`Query.cshtml:80-99`). Nothing is saved; do not
   click **Create**.

### Sign-in

`state.json` is made once, outside the capture, with `Recorder setup` (`VIDEO_USER_NAME=admin`,
`VIDEO_PASSWORD=<password>`). It is git-ignored. The API does not use that cookie; `04-call` works because of the
permission granted to the Anonymous role in `04-grant`.

## Snapshot, order and dependencies

1. When the seed is created and checked, stop the site and copy its `App_Data` folder and `state.json` aside. This
   is the snapshot.
2. Record in this order, which is also the order the clips play in: `01-queries`, `02-post`, `04-grant`, `04-call`.
3. `01-queries` and `02-post` only read the site. They can be recorded again at any time.
4. `04-grant` changes the site: it grants **Execute Api - RecentBlogPosts** to the Anonymous role. Before another take
   of it (or of a full run), stop the site, restore the snapshot, and start the site again.
5. `04-call` depends on `04-grant`. If `04-grant` was not recorded from the snapshot just before, grant the
   permission by hand first (Access Control, Roles, Anonymous) and make sure the `curl` check above prints `200`.
   `04-call` does not change the site, so it can be recorded again without a restore.

The slides need no site.

## Clips

### Clip `01-queries` (chapter 01, A query with a name)

Start page: `/Admin`, signed in. Before `StartCaptureAsync`, expand the admin menu to Search, Queries (plain
`scene.Page` clicks on the **Search** and **Queries** entries), so the link **All Queries** is visible
(`OrchardCore.Queries/AdminMenu.cs:18-22`). If the menu is not visible at 1536 px wide, open the admin menu button
first.

`queries-1`
- Actions: `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('All Queries')").First)`. If
  that selector finds nothing, use `a[href$='/Admin/Queries/Index']` (the route template is `Queries/{action}/{id?}`,
  `Controllers/AdminController.cs:20`). Then `await scene.PauseAsync(600)`.
- On screen when the narration ends: the **Queries** page (`Views/Admin/Index.cshtml:6`) with one row,
  `RecentBlogPosts`, with the SQL query badge or the **Source** column, depending on the list layout of the admin, and
  the **Add Query** button (`Index.cshtml:25`).
- Labels from: `OrchardCore.Queries/AdminMenu.cs:18` (Search), `:21` (Queries), `:22` (All Queries);
  `Views/Query.Fields.SummaryAdmin.cshtml:4` (the name link); `Views/SqlQuery.SummaryAdmin.cshtml:4` (the "SQL query" badge).

`queries-2`
- Actions: `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('RecentBlogPosts')").First)`;
  wait until `.CodeMirror` is visible; `await scene.ScrollAsync(0, 500)`, then check with a screenshot of the frame
  that both the **Return Documents** checkbox and the last line of the **Query** editor are fully visible, and adjust the
  scroll by steps of 100 if not. Then `await scene.HighlightAsync(scene.Page.Locator(".CodeMirror").First)`, a pause of
  one second, and `await scene.ClearHighlightsAsync()`.
- On screen when the narration ends: **Edit 'RecentBlogPosts' query**, scrolled so that the checked **Return Documents**
  checkbox and the query text ending with `LIMIT @limit:3` are both visible. The **Save** and **Cancel** buttons stay
  unclicked.
- Labels from: `Views/Admin/Edit.cshtml:4` (the title), `Views/Query.Fields.Edit.cshtml:4` (Name) and `:14` (Schema),
  `Views/SqlQuery.Edit.cshtml:26` (Return Documents) and `:53` (Query), `Views/Query.Fields.Buttons.cshtml:3` (Save).
- Do not click Save. Leave the page with the next clip.

### Clip `02-post` (chapter 02, Queries in Liquid)

Start page: `/blog/planning-the-autumn-menu`, signed in or not (the page is the same). Open it before
`StartCaptureAsync`, with the page loaded and at the top.

`post-1`
- Actions: `await scene.ScrollAsync(0, 700)`, `await scene.PauseAsync(700)`, `await scene.ScrollAsync(0, 700)`, and
  again until the three titles are on screen (stop at the bottom of the page). Each scroll is a smooth wheel scroll of
  the scene API; keep the total under the narration (about twelve seconds).
- On screen when the narration ends: the end of the post page, below the post body and its tags, with the three titles
  printed one after the other, in this order: `Planning the Autumn Menu`, `A Week of Slow Travel`,
  `Notes From the Allotment`. The recipe's post is not among them.
- Where the labels come from: `OrchardCore.Themes/TheBlogTheme/Views/Content-BlogPost.liquid:34-37` prints the titles.
  The default limit is `LIMIT @limit:3` (`blog.recipe.json:1181`).

### Clip `04-grant` (chapter 04, Queries through the web API)

Start page: `/Admin/Roles`, signed in, opened before `StartCaptureAsync`. This clip changes the site.

`grant-1`
- Actions: `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("tr:has-text('Anonymous') a.edit, .list-group-item:has-text('Anonymous') a.edit").First)`
  (the **Edit** button of the Anonymous row, `OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`).
  Then `await scene.PauseAsync(500)`.
- On screen when the narration ends: **Edit 'Anonymous' Role** (`Views/Admin/Edit.cshtml:6`), with the **Search
  permissions** box (`Views/Admin/_RolePermissions.cshtml:49`) and the permission tables below it.
- Labels from: `OrchardCore.Roles/AdminMenu.cs:18` (Access Control) and `:19` (Roles); `Views/Admin/Index.cshtml:15`
  (the Roles page title).

`grant-2`
- Actions: `await scene.TypeAsync(scene.Page.Locator("#search-box"), "RecentBlogPosts", 45)`; wait until only the
  **Queries** group with its one matching row is left; `await scene.ClickAsync(scene.Page.Locator("[id='Checkbox.ExecuteApi_RecentBlogPosts']"))`
  (the **Allow** box of **Execute Api - RecentBlogPosts**); `await scene.PauseAsync(600)`; then
  `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save[value='Save']").First)`.
- On screen when the narration ends: the Roles page with the notice **Role updated successfully.**
  (`OrchardCore.Roles/Controllers/AdminController.cs:213`). If the notice has already faded, the Roles list is enough.
- Labels from: `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49` (Search permissions), `:131` (the checkbox id
  `Checkbox.<permission name>`), `Views/Admin/Edit.cshtml:33` (Save); `OrchardCore/OrchardCore.Queries.Abstractions/QueryPermissions.cs:21-25`
  (the permission name `ExecuteApi_{0}` and its label **Execute Api - {0}**).
- After the capture, open the Anonymous role again and check the box is allowed, and run the `curl` check: it prints
  `200`.

### Clip `04-call` (chapter 04, Queries through the web API)

Start page: `/`, opened before `StartCaptureAsync`. Needs the permission of `04-grant`. The site is not changed.

`call-1`
- Actions: `await scene.NavigateAsync("/api/queries/RecentBlogPosts")`, then
  `await scene.HighlightAsync(scene.Page.Locator("pre").First, "GET /api/queries/RecentBlogPosts")` (if the response
  is not drawn in a `pre` element, use `scene.Page.Locator("body")`), then `await scene.PauseAsync(800)`. The caption
  names the request, since the address bar is not in the capture. Clear it with `await scene.ClearHighlightsAsync()`
  at the end of the step.
- On screen when the narration ends: the JSON response in the browser, starting with the `items` property and its
  array of three content items (the three seed posts), with no error. The address bar is not part of the capture.
- Where it comes from: `OrchardCore.Queries/Controllers/QueryApiController.cs:10-14` (route, scheme, anonymous),
  `:29-36` (GET and POST), `:52-56` (the permission, and `NotFound` without it), `:71` (the result is returned as
  an object).

`call-2`
- Actions: `await scene.NavigateAsync("/api/queries/RecentBlogPosts?parameters=%7B%22limit%22%3A2%7D")` (the value is
  the URL-encoded JSON object `{"limit":2}`), then
  `await scene.HighlightAsync(scene.Page.Locator("pre").First, "GET /api/queries/RecentBlogPosts?parameters={\"limit\":2}")`
  (same fallback to `body`), then `await scene.PauseAsync(800)`. Clear the caption with `await scene.ClearHighlightsAsync()`
  at the end of the step.
- On screen when the narration ends: the JSON response with two content items in `items`: `Planning the Autumn Menu` and
  `A Week of Slow Travel`.
- Where it comes from: `QueryApiController.cs:59-66` (the `parameters` value, or the body of a POST, is deserialized
  into the parameter dictionary), and the query text `LIMIT @limit:3` (`blog.recipe.json:1181`).

## Slide steps

One line each: what the slide shows.

- `intro`: the opening card, "Queries in Liquid, Razor and the API", subtitle "Run a saved query by its name".
- `liquid-1`: the four lines of TheBlogTheme's `Content-BlogPost.liquid` that run `Queries.RecentBlogPosts | query` and print each `display_text`; two notes beside them.
- `liquid-2`: the same loop with `query: limit: 5`; notes on the parameter and on case-sensitive names.
- `liquid-3`: two cards: a query that returns content items, and a query that returns rows.
- `liquid-4`: two cards: an unknown query name gives nil, and the `Queries` property works in every Liquid template.
- `razor-1`: the `ContentQueryAsync` loop of the Queries documentation, with `DisplayAsync` and the `"Summary"` display type, and a note card for each method.
- `razor-2`: the same loop with a `Dictionary<string, object>` holding `limit`.
- `razor-3`: two cards comparing `ContentQueryAsync` and `QueryAsync`.
- `razor-4`: `@inject OrchardCore.IOrchardHelper Orchard`, and two notes on where the `Orchard` helper comes from.
- `api-1`: three `curl` requests: a GET, a GET with `parameters` in the query string, and a POST with the parameters as the body.
- `api-2`: the unsafe and the safe SQL of the documentation side by side (`{{ input }}` against `@pattern`), with a note for each.
- `api-3`: four cards: Execute Api - All queries, Execute Api - RecentBlogPosts, Manage queries, and Not Found without the permission.
- `api-4`: a `curl` request with an `Authorization: Bearer <access token>` header, and notes on anonymous requests and the sign-in cookie.
- `api-5`: the result objects: `items` for a SQL query, `items` and `count` for a Lucene or Elasticsearch query, and a note on content items against rows.
- `recap-1`: three cards: Liquid, Razor and Web API, each with the call that runs `RecentBlogPosts`.
- `outro`: the closing card, subtitle `docs.orchardcore.net`.

## Checks for private data

Before the clips are used, look at every frame (and OCR the video at one frame per second and at every scene change):

- The sign-in is `admin` with the email `admin@example.com`; no screen shows the email or any password. The Roles and
  Queries pages show no user list.
- The post page shows the author `admin`; the JSON of `call-1` and `call-2` holds `"author"` and `"owner"` values (the
  user name and a generated user id). Both are fake: the site is a fresh one.
- No frame shows a real name, a real address or a token. The titles and bodies of the seed posts are the invented ones
  above. The slides use only `example.com`.
- The address bar and the port are not in the capture. The browser tab shows no other site.
- No error page, empty list or stack trace appears in a contact sheet.

# Recording plan: Build a Headless Site

For the person who records the video. Read it from top to bottom once, then follow it clip by clip. Nothing here asks
you to decide anything. A selector written in backticks stands for `scene.Page.Locator(...)`. Source paths are relative
to the root of the `OrchardCMS/OrchardCore` checkout the site runs from (`file:line`).

The narration is in `storyboard.json`. Narrate first (`narrate.py`), then record: each step is held for the length of its
narration. This is a developer video: the browser clips show the admin and a small web page, and the code is on slides.

Selectors for the admin pages are read from the Razor views and TypeScript of the modules. Selectors for GraphiQL are
read from its bundle (`OrchardCore.Apis.GraphQL/wwwroot/Scripts/graphiql-orchard.js`) and from `Assets/js/App.tsx`.
None of them was run against a live site: `OPEN-QUESTIONS.md` lists what to confirm on the first take.

## The site it needs

Run `src/OrchardCore.Cms.Web` of an Orchard Core checkout on `http://localhost:5010`, with its own `App_Data` outside
both repositories (`ORCHARD_APP_DATA`), provisioned by Auto Setup so no setup or sign-in form is recorded. The site is
a **fresh** site with the **Blog** recipe: the video relies on the Blog recipe's content type `BlogPost`, its saved
query `RecentBlogPosts` and the features it enables (Queries, SQL Queries).

```bash
export ASPNETCORE_URLS=http://localhost:5010
export ORCHARD_APP_DATA=<an empty folder outside both repositories>
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword='<password>'
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
# Only in the shell that runs the site: its outbound calls go nowhere.
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9
```

`<password>` is the password chosen outside git; it is the value of `VIDEO_PASSWORD` for the recorder. Nothing in this
plan or in the video types it: the recorder signs in once with `Recorder setup` and every clip starts from `state.json`.

Before recording, in this order:

1. Start the site, open `http://localhost:5010/`, and let Auto Setup finish. Check that **Tools > Features** shows
   **GraphQL** (category Api) and **CORS Configuration** (category Security) with an **Enable** button (not enabled), and
   **Queries** and **SQL Queries** with the **Enabled** badge. Do not enable anything else. The clip `02-features` enables the first two on camera.
2. Run `Recorder setup` to sign in as `admin` and save `state.json` (git-ignored).
3. Create the three fake blog posts below with **Tools > Deployments > JSON Import** (outside the capture).
4. Create the front-end folder below and serve it on `http://localhost:5173`.
5. Take the snapshot **S0** (see "Order of the clips").

### Seed data: three blog posts

Paste this recipe in the **JSON** field of **Tools > Deployments > JSON Import** and select **Import**. The page
answers **Recipe imported successfully!**. The posts have no dates: the site stamps the time of the import, so they are
the three most recent posts, newer than the sample post of the Blog recipe, whichever way they are sorted. They have no
draft and are not edited afterwards: the saved query `RecentBlogPosts` has no `Published` filter, so a draft or an
older version of a post would show up in its results.

```json
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "20j063o1bw72ghw8gnlehmyl09",
          "ContentType": "BlogPost",
          "DisplayText": "Planting calendar for a small garden",
          "Latest": true,
          "Published": true,
          "TitlePart": { "Title": "Planting calendar for a small garden" },
          "MarkdownBodyPart": {
            "Markdown": "Start tomatoes indoors in early spring, and move them outside after the last frost.\n\nHerbs such as basil and chives grow well on a sunny windowsill."
          }
        },
        {
          "ContentItemId": "3cbnkzllr3t3pr10qlva2l82c8",
          "ContentType": "BlogPost",
          "DisplayText": "Five herbs for a sunny windowsill",
          "Latest": true,
          "Published": true,
          "TitlePart": { "Title": "Five herbs for a sunny windowsill" },
          "MarkdownBodyPart": {
            "Markdown": "Basil, chives, mint, parsley and thyme need little room and a lot of light.\n\nWater them when the top of the soil is dry."
          }
        },
        {
          "ContentItemId": "l5dgobw5py4o6liw6u2awrutlc",
          "ContentType": "BlogPost",
          "DisplayText": "Visiting the village market",
          "Latest": true,
          "Published": true,
          "TitlePart": { "Title": "Visiting the village market" },
          "MarkdownBodyPart": {
            "Markdown": "The market opens on Saturday morning with vegetables, bread and flowers.\n\nArrive early for the best choice."
          }
        }
      ]
    }
  ]
}
```

The **JSON** field is a CodeMirror editor. If typing into it does not work, set its value from the page with
`document.querySelector('.CodeMirror').CodeMirror.setValue(<the text>)`, then select **Import**. If the import fails,
create three published posts by hand from the **Blog** entry of the admin menu with the same titles, once each, and
publish them directly (no draft, no later edit).

To check the seed, run the query once in GraphiQL (`/Admin/GraphQL`, after clip `02-features`): the three titles above
come first. If the sample post "Man must explore, and this is exploration at its greatest" is among the three results,
the seed is wrong: restore the snapshot and import again.

### The front end (outside both repositories)

The front end is the customer's application, played by two static files. Create a folder outside both repositories
(`<front-end folder>`) with these two files, and serve it:

```bash
python -m http.server 5173 --directory <front-end folder>
```

`index.html`:

```html
<!doctype html>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Recent posts</title>
<style>
  body { font: 22px/1.5 system-ui, sans-serif; margin: 3rem auto; max-width: 40rem; }
  button { font: inherit; padding: 0.4rem 1rem; }
  #message { color: #b00020; min-height: 1.5em; }
  li { margin-bottom: 0.5rem; }
</style>
<h1>Recent posts</h1>
<button id="load">Load posts</button>
<p id="message"></p>
<ul id="posts"></ul>
<script src="app.js"></script>
```

`app.js` (the slides of the steps `front-2` and `front-3` show this code, trimmed):

```javascript
const endpoint = "http://localhost:5010/api/graphql";
const query = "{ recentBlogPosts { displayText publishedUtc } }";

const message = document.getElementById("message");
const posts = document.getElementById("posts");

async function loadPosts() {
  message.textContent = "Loading...";
  posts.replaceChildren();

  try {
    const response = await fetch(endpoint, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ query }),
    });
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    const { data } = await response.json();

    for (const post of data.recentBlogPosts) {
      const item = document.createElement("li");
      item.textContent = `${post.displayText} (${post.publishedUtc.slice(0, 10)})`;
      posts.append(item);
    }

    message.textContent = "";
  } catch (error) {
    message.textContent = `The request failed: ${error.message}`;
  }
}

document.getElementById("load").addEventListener("click", loadPosts);
```

The page is on another origin than the site (`http://localhost:5173` against `http://localhost:5010`), and its `fetch`
sends no cookies (the default `credentials` of `fetch`), so the site sees an anonymous visitor even in the recorder's
signed-in browser context. The recorder's `OpenAsync` and `NavigateAsync` add `VIDEO_BASE_URL` to their argument, so
they cannot open this page: use `scene.Page.GotoAsync("http://localhost:5173/")` before `StartCaptureAsync`, in the two
clips that show the page.

### Settings and external services

- No site setting is changed outside the capture. The Anonymous role keeps its defaults until clip `05-permissions`.
- No external service. The page on port 5173 stands for the customer's application, and nothing else leaves the machine.
- The admin menu is not collapsed: clips use it to reach their pages. If a menu entry cannot be driven by the recorder,
  `NavigateAsync` to the address given in the clip instead (the narration still names the menu path).

## Order of the clips

Record in the order the video plays them. Each clip starts from the state the previous one left.

| # | Clip | Changes the site | Needs |
|---|---|---|---|
| 1 | `02-features` | Enables GraphQL and CORS Configuration (the tenant restarts) | S0 |
| 2 | `03-graphiql` | No | GraphQL enabled |
| 3 | `04-query` | No (the query editor is left without saving) | GraphQL enabled |
| 4 | `05-permissions` | Yes: saves the Anonymous role | GraphQL enabled, Anonymous without the two permissions |
| 5 | `06-before` | No | Anonymous permissions saved, **no CORS policy** |
| 6 | `07-policy` | Yes: saves the policy `Frontend` (the tenant reloads) | No CORS policy yet |
| 7 | `07-after` | No | The `Frontend` policy saved |

Snapshots (stop the site, copy `ORCHARD_APP_DATA` and `state.json` aside, start the site again):

- **S0**: after the seed, before `02-features`. Restore it before a full take, or to record `02-features` again.
- **S1**: after `02-features`. Restore it to record `05-permissions` again (the Anonymous role must be back to its
  defaults). `03-graphiql` and `04-query` can start from S1 or any later snapshot.
- **S2**: after `05-permissions`. Restore it to record `06-before` or `07-policy` again (there must be no policy).
- `07-after` needs the policy: record `07-policy` first, or restore a snapshot taken after it.

The slide clips (`01-idea`, `06-code`, `08-further`) are drawn from `slides.py` and need no site.

## Clips

Every clip is a new browser context with `state.json`, so it starts signed in as `admin`. "Ends with" is what must be on
screen when the narration of the step ends.

### `02-features`

Starts on `/Admin` (the dashboard), signed in. Open it before the capture.

- `features-1`: In the left admin menu select **Tools**, then **Features** (`ClickAndWaitForNavigationAsync`; fallback
  `NavigateAsync("/Admin/Features")`). In `#search-box` type `GraphQL`. Select the **Enable** button of the **GraphQL**
  row, `#btn-enable-OrchardCore_Apis_GraphQL` (`ClickAndWaitForNavigationAsync`). Ends with the Features page reloaded and
  the notification **The feature GraphQL was enabled.** at the top. Labels: **Tools** and **Features**
  `OrchardCore.Features/AdminMenu.cs:26-27`; **Search** `OrchardCore.Features/Views/Admin/Features.cshtml:27`; **Enable**
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; notification
  `OrchardCore.Features/Controllers/AdminController.cs:317`; the category **Api** is the `Category` of the GraphQL module,
  `OrchardCore.Apis.GraphQL/Manifest.cs:9`; the feature name **GraphQL** is its `Name`, `Manifest.cs:4`.
- `features-2`: Empty the search box (`FillAsync("")`, then `TypeAsync` appends), type `CORS`, and select **Enable** in the
  **CORS Configuration** row, `#btn-enable-OrchardCore_Cors` (`ClickAndWaitForNavigationAsync`). Empty the box again,
  type `Queries`, and highlight the rows **Queries** and **SQL Queries**, both showing the **Enabled** badge. Ends with
  those two rows highlighted. Keep the actions shorter than the narration: type fast (`delay: 35`). Labels: **CORS
  Configuration** is the `Name` in `OrchardCore.Cors/Manifest.cs:4`; **Queries** and **SQL Queries** are the feature names
  in `OrchardCore.Queries/Manifest.cs:24` and `:35`; **Enabled** badge
  `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`; the Blog recipe enables both features,
  `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json` (the `feature` step, `OrchardCore.Queries` and
  `OrchardCore.Queries.Sql`).

### `03-graphiql`

Starts on `/Admin`.

- `graphiql-1`: Select **Tools**, then **GraphiQL** in the left admin menu (`ClickAndWaitForNavigationAsync`; fallback
  `NavigateAsync("/Admin/GraphQL")`). Wait until the GraphiQL screen has drawn (the spinner `.loader` is gone and the
  Explorer panel, on the left, lists fields). Highlight the Explorer row **blogPost** (caption `blogPost`). Ends with
  the GraphiQL page, the Explorer panel open, and **blogPost** ringed. Labels: **GraphiQL** `OrchardCore.Apis.GraphQL/AdminMenu.cs:19`;
  the route is `[Admin("GraphQL", "GraphQL")]`, `OrchardCore.Apis.GraphQL/Controllers/AdminController.cs:9`; the Explorer is open
  by default, its toggle is labeled **Explorer**, `OrchardCore.Apis.GraphQL/Assets/js/App.tsx:118`; the field `blogPost` is
  the content type `BlogPost` with its first letter in lower case (`OrchardFieldNameConverter.cs`, camel case); the requests
  use the signed-in session, `App.tsx:28` (`credentials: 'include'`).
- `graphiql-2`: Clear the highlight, then `NavigateAsync` to `/Admin/GraphQL?query=` followed by the URL-encoded text
  below (the page reads `?query=` into the editor, `App.tsx:46`), so nothing is typed into the editor:

  ```
  {
    blogPost(first: 3, orderBy: { publishedUtc: DESC }) {
      displayText
      publishedUtc
    }
  }
  ```

  Select the run button of the GraphiQL toolbar (`.execute-button`, a play icon without text whose title is
  `Execute Query (Ctrl-Enter)`; the narration calls it "the run button, titled Execute Query"; fallback: press
  `Control+Enter` in the editor) and wait until the result pane (`.result-window`) contains `"data"`.
  Ends with the query on the left and, on the right, the JSON response with the three titles of the seed under `data`.
  Labels: the title **Execute Query** is in the GraphiQL bundle (`graphiql-orchard.js`); `first`, `orderBy`, `where`,
  `skip` and `status` are the arguments of `ContentItemsFieldType`,
  `src/OrchardCore/OrchardCore.ContentManagement.GraphQL/Queries/ContentItemsFieldType.cs:44-76`; `displayText` and `publishedUtc`
  are fields of `ContentItemType`, `.../Queries/Types/ContentItemType.cs:35,48-50`; `DESC`
  `.../Queries/Types/ContentItemOrderByInput.cs:39`.
- `graphiql-3`: In the Explorer panel, under **blogPost**, ring the argument rows `where`, `orderBy`, `first`, `skip`
  and `status` one after the other (`HighlightAsync`, about one second each; expand the field in the Explorer if the
  arguments are folded). Ends with the arguments ringed. The arguments and the default of `status`
  (`PublicationStatusEnum.Published`) are in `ContentItemsFieldType.cs:44-76`.
- `graphiql-4`: Clear the highlights. Select the **Docs** button at the top right of the GraphiQL toolbar
  (`.docExplorerShow`), then **blogPost** in the documentation panel (it shows **Represents a Blog Post.**), then the
  type **BlogPost** it returns, and ring the line of `publishedUtc` (**The date and time of publication**). Ends with the
  Docs panel open on the `BlogPost` type, `publishedUtc` ringed. Labels: `Represents a {0}.` with the display name **Blog
  Post** `.../Queries/ContentTypeQuery.cs:60,65`; `The date and time of publication` `ContentItemType.cs:50`.

### `04-query`

Starts on `/Admin`.

- `query-1`: In the left admin menu select **Search**, **Queries**, then **All Queries** (`ClickAndWaitForNavigationAsync`;
  fallback `NavigateAsync("/Admin/Queries/Index")`). Ends with the list showing one query, **RecentBlogPosts**, with the
  badge **SQL query**, and its buttons; the row is ringed. Labels: `OrchardCore.Queries/AdminMenu.cs:18-22`; badge
  `OrchardCore.Queries/Views/SqlQuery.SummaryAdmin.cshtml:4`; the query comes from the Blog recipe,
  `blog.recipe.json` (the `queries` step, `Name` `RecentBlogPosts`, `Source` `Sql`).
- `query-2`: Select **Add Query** (`button.create`). The dialog **Available Query Types** opens with one card, **SQL**
  (hint "Queries the SQL database.") and its **Add** button: ring the card. Select **Cancel** in the footer of the
  dialog. Ends with the dialog closed (or, if the take runs short, with the card ringed). Labels:
  `OrchardCore.Queries/Views/Admin/Index.cshtml:25` (**Add Query**), `:50` (**Available Query Types**), `:72` (**Cancel**);
  `OrchardCore.Queries/Views/Query-Sql.Link.cshtml:6,10` (**SQL**, **Add**).
- `query-3`: Select **Edit** in the row of **RecentBlogPosts** (`ClickAndWaitForNavigationAsync`). The page is titled
  **Edit 'RecentBlogPosts' query**. Wait for the editors to draw (the Schema editor, `.monaco-json-settings-editor-container`,
  shows text). Ring the check box **Return Documents** (checked), then the **Query** editor, whose text ends with
  `LIMIT @limit:3`. Do not type anything and do not select **Save**. Ends with **Return Documents** and **Query** ringed.
  Labels: **Edit** `OrchardCore.Queries/Views/Query.Buttons.SummaryAdmin.cshtml:6`; title `OrchardCore.Queries/Views/Admin/Edit.cshtml:4`;
  **Return Documents** `OrchardCore.Queries/Views/SqlQuery.Edit.cshtml:26`; **Query** `:53`. The SQL text and the
  `@limit:3` default come from `blog.recipe.json` (`Template` of `RecentBlogPosts`).
- `query-4`: Clear the highlights, scroll to the top if needed, and ring the **Schema** editor, which shows
  `{ "type": "ContentItem/BlogPost" }`. Ends with the Schema field ringed. Do not edit the editor (Monaco closes braces and
  quotes by itself). Label: **Schema** `OrchardCore.Queries/Views/Query.Fields.Edit.cshtml:14`, hint "The schema the
  GraphQL API will use to return results." `:18`. The schema value is the base64 text of the `Schema` property in
  `blog.recipe.json`, which decodes to `{ "type": "ContentItem/BlogPost" }`.
- `query-5`: Clear the highlights, then `NavigateAsync` to `/Admin/GraphQL?query=` followed by the URL-encoded text below:

  ```
  {
    recentBlogPosts {
      displayText
      publishedUtc
    }
  }
  ```

  Ring the Explorer row **recentBlogPosts** (it is listed along with **blogPost**), then select **Execute Query** and wait
  until the result pane contains `"recentBlogPosts"`. Ends with the query on the left and the three titles of the seed
  under `data.recentBlogPosts` on the right, the Explorer row still ringed (ring it again after the result if the page
  redrew). Source: the saved query becomes a field in `OrchardCore.Queries/Sql/GraphQL/SqlQueryFieldTypeProvider.cs:50-84`
  (field name = query name, a query without a `Schema` is skipped, `:52-55`); the `parameters` argument `:152,195`.

### `05-permissions`

Starts on `/Admin`.

- `perm-1`: In the left admin menu select **Access Control**, then **Roles** (`ClickAndWaitForNavigationAsync`; fallback
  `NavigateAsync("/Admin/Roles/Index")`: the route is `Roles/{action}/{id?}`, `OrchardCore.Roles/Controllers/AdminController.cs:22`, so the
  address without `Index` does not exist). The list shows the roles, with **Anonymous** among them. Select **Edit** in the row
  of **Anonymous** (`ClickAndWaitForNavigationAsync`). Ends with the page titled **Edit 'Anonymous' Role** and the
  permissions table (columns **Permission**, **Allow**, **Effective**). Labels: `OrchardCore.Roles/AdminMenu.cs:18-19`;
  **Edit** `OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`; title
  `OrchardCore.Roles/Views/Admin/Edit.cshtml:6`; columns `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml`.
- `perm-2`: In `#search-box` (placeholder **Search permissions**) type `execute`. Select the **Allow** box of the row
  **Execute GraphQL.** (`label[for='Checkbox.ExecuteGraphQL']`; the label ends with a period) and of the row **Execute Api
  - RecentBlogPosts** (`label[for='Checkbox.ExecuteApi_RecentBlogPosts']`). Ring the row **Execute GraphQL Mutations.**
  (`Checkbox.ExecuteGraphQLMutations`), left unchecked, then clear the highlight. Select **Save** (`button.save`,
  `WaitForNavigationAsync`). Ends with the Roles list and the notification **Role updated successfully.** Labels:
  `OrchardCore.Apis.GraphQL.Abstractions/GraphQLPermissions.cs:10,12` (`src/OrchardCore/`); `Execute Api - {0}`
  `src/OrchardCore/OrchardCore.Queries.Abstractions/QueryPermissions.cs:23`; **Search permissions**
  `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49`; **Save** `OrchardCore.Roles/Views/Admin/Edit.cshtml:33`;
  notification `OrchardCore.Roles/Controllers/AdminController.cs:213`. If the **Save** drop-down is in the way, the
  default action is **Save**.
- `perm-3`: `NavigateAsync("/Admin/Roles/Edit/Anonymous")`, type `view all content` in `#search-box`, and ring the row
  **View all content** with its **Allow** box checked. Do not change anything. Ends with that row ringed. Label:
  `src/OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:29` ("View all content"); Anonymous receives it by
  default, `OrchardCore.Contents/Permissions.cs:91-95`. The `blogPost` field is declared with the permission View own
  content for its content type (`.../ContentManagement.GraphQL/Queries/ContentTypeQuery.cs:69`), which View all content
  implies (`CommonPermissions.cs:31`), and it returns every post to a role that has View all content
  (`.../ContentManagement.GraphQL/Queries/ContentItemFilters.cs:37`). The saved query checks only its own permission,
  `SqlQueryFieldTypeProvider.cs:162,204`, and loads its rows without a content permission check.

### `06-before`

Starts on the front-end page: `scene.Page.GotoAsync("http://localhost:5173/")` before `StartCaptureAsync`. The page
shows the title **Recent posts**, the button **Load posts**, an empty message and an empty list.

- `front-4`: Select **Load posts** (`#load`, a plain click, no navigation). Wait until `#message` contains `The request
  failed` (`WaitForFunctionAsync`). Ring `#message`. Ends with `The request failed: Failed to fetch` in red under the
  button, and no list. (The wording after the colon is Chromium's message for a request the browser blocked.) The failure
  is expected: the CORS feature is enabled but has no policy, so the site sends no CORS headers; see
  `OrchardCore.Cors/Services/CorsOptionsConfiguration.cs:20-23`.

### `07-policy`

Starts on `/Admin`.

- `cors-1`: In the left admin menu select **Settings**, **Security**, then **Cross-Origin Resource Sharing**
  (`ClickAndWaitForNavigationAsync`; fallback `NavigateAsync("/Admin/Cors")`). The page is titled **CORS Settings**, shows
  the warning "The current tenant will be reloaded when the CORS is executed." and the message "Nothing here! There are no
  CORS policies for the moment." Select **Add a policy** (`button:has-text("Add a policy")`, no navigation). Ends with the
  policy editor open: the heading **New policy** and the card **Details**. Labels:
  `OrchardCore.Cors/AdminMenu.cs:18-20`; `OrchardCore.Cors/Views/Admin/Index.cshtml:11,13`;
  `OrchardCore.Cors/Services/CorsJSLocalizer.cs:17,28,30`; the editor is `OrchardCore.Cors/Assets/ts/cors-admin-index.ts`.
- `cors-2`: In the **Policy name** box (the text input of the card **Details**) empty the value (`FillAsync("")`) and type
  `Frontend`. Select **Set as default policy** (`#set-default-policy`). Ends with `Frontend` in the box and the check
  box checked. The code makes the first policy the default when none is marked (`OrchardCore.Cors/Services/CorsOptionsConfiguration.cs:78-84`),
  so the policy would still apply, but the narration says the box is checked and the list shows the **Default Policy** badge only when it is: do not skip this box. Labels: `CorsJSLocalizer.cs:28,30`; ids `cors-admin-index.ts:110`; the heading shows the policy name.
- `cors-3`: Select **Allow credentials** (`#allow-credentials`, checked when the editor opens, `cors-admin-index.ts:290`) to
  clear it. Scroll to the card **Origins**. In the box of **Allowed origins**
  (`.ocat-wrapper:has(label:text-is("Allowed origins")) input.form-control`) type `http://localhost:5173` and select
  **Add Origin**. Ends with the origin listed under **Allowed origins**, **Allow credentials** cleared, **Allow any
  origin** still cleared. Do not type a trailing slash or a path. Labels: `CorsJSLocalizer.cs:33,37,39,40`; **Add Origin**
  is "Add" plus the option type, `cors-admin-index.ts:71`.
- `cors-4`: Clear the highlights before scrolling. The editor shows the card **Headers** above the card **Methods**, so work from the top down: clear **Allow any header**
  (`#allowed-headers`), in the box of **Allowed headers** type `Content-Type` and select **Add Header**; then clear **Allow any
  method** (`#allowed-methods`), in the box of **Allowed methods** type `POST` and select **Add Method**. Scroll to the bottom and select **Save**
  (`.action-bar-bottom button:text-is("Save")`, wrap it in `WaitForNavigationAsync`: it submits the whole form and the
  tenant reloads, so allow several seconds). Ends with the page reloading after the save. The defaults of a new policy
  (credentials, any method and any header checked; no origin) are in `cors-admin-index.ts:281-293`; the labels
  `CorsJSLocalizer.cs:43,45,46,49,51,52`.
- `cors-5`: Ring the notification **The CORS settings have updated successfully.** at the top of the page, then the row
  **Frontend** and its badge **Default Policy** in the list. Ends with both ringed. Labels:
  `OrchardCore.Cors/Controllers/AdminController.cs:144`; badge `cors-admin-index.ts:351` and `CorsJSLocalizer.cs:18`.
  Saved settings are site settings of the tenant, `OrchardCore.Cors/Services/CorsService.cs`.

### `07-after`

Starts on the front-end page: `scene.Page.GotoAsync("http://localhost:5173/")` before `StartCaptureAsync` (a fresh
load, so the message is empty).

- `cors-6`: Select **Load posts** (`#load`). Wait until `#posts li` has three items. Ends with the three titles of the seed
  in the list, each followed by a date in parentheses, and an empty message. The date is the day of the import, the
  same for the three posts.
- `cors-7`: Ring `#posts`, then hold. Ends with the list ringed. (If the take runs short, the ring can stay from `cors-6`.)

## Slides

The slides are drawn by `slides.py`; nothing is recorded for them.

- `idea-1`: three cards: Orchard Core, An HTTP API, The front end.
- `idea-2`: three cards: GraphQL, Queries, CORS, each with one line (the Queries card reads "Saved queries with a schema
  become fields of that endpoint.").
- `idea-3`: three cards with the permissions: Execute GraphQL, Execute Api - RecentBlogPosts, Execute GraphQL Mutations.
- `front-1`: the heading "Ways to send a query", a code panel with three kinds of request (POST with a JSON body, POST
  with a query text body and the `application/graphql` content type, GET with a query string) and three cards that name
  them. The endpoint also accepts a POST that carries `query` in the query string (`GraphQLMiddleware.cs:115-118`). Source: `OrchardCore.Apis.GraphQL/GraphQLMiddleware.cs:96-123,186-208`.
- `front-2`: `app.js`, the request: the endpoint, the query, `fetch` with the JSON body, and the check of the response.
- `front-3`: `app.js`, the drawing of the list and the message of a failure, and the markup of `index.html` below.
- `more-1`: the recipe with the `feature` step and the `settings` step that holds `CorsSettings`; three cards. Source:
  `docs/reference/modules/Cors/README.md`, section "Recipe and deployment support"; the deployment step name
  `OrchardCore.Cors/Startup.cs:43` (**Cors settings**).
- `more-2`: the `queries` recipe step of `RecentBlogPosts` (source, name, template, schema, `ReturnContentItems`), from
  `blog.recipe.json`.
- `more-3`: the `OrchardCore:Apis:GraphQL` section of the configuration with six of its keys (the last is
  `MaxNumberOfResultsValidationMode` set to `Enabled`), and three cards. Source: `OrchardCore.Apis.GraphQL/Startup.cs:61-90` and
  `ConfigurationSchema.json`. Only the `Enabled` mode enforces `MaxNumberOfResults` (a validation error,
  `ValidationRules/MaxNumberOfResultsValidationRule.cs:51-60`); in the other modes the rule only logs, and
  `ContentItemsFieldType.cs:203-218` pages with the `first` of the request, so the slide sets the mode.
- `more-4`: four cards, the first two bright: private content (cookie or OAuth token) and writes (mutations need Execute
  GraphQL Mutations). Source: `GraphQLMiddleware.cs:65-71`, `ValidationRules/RequiresPermissionValidationRule.cs:63`.
- `more-5`: the same four cards, the last two bright: the REST route of a saved query and the Headless site recipe.
  Source: `OrchardCore.Queries/Controllers/QueryApiController.cs:10-36`;
  `OrchardCore.Themes/TheAdmin/Recipes/headless.recipe.json` (`displayName` **Headless site**).

## Checks for private data

Before assembling, look at every contact sheet and OCR the result as the skill asks (every second, at scene changes):

- The only person is `admin` with `admin@example.com`; the only names are the three post titles above, the sample post of the
  Blog recipe, and the site name **Orchard Core**. No real name, address, phone number or token anywhere.
- The password is never typed on camera and never in a file of the repository; `state.json` stays out of git.
- The only addresses on screen are `localhost:5010`, `localhost:5173` and `example.com` (the slides). The page
  **CORS Settings** of clip `07-policy` also shows the link text `https://docs.microsoft.com/en-us/aspnet/core/security/cors`
  (`OrchardCore.Cors/Views/Admin/Index.cshtml:14`), and the **Schema** hint of clip `04-query` links to `docs.orchardcore.net`
  (`OrchardCore.Queries/Views/Query.Fields.Edit.cshtml:18`): both are text of Orchard Core, expected, and not private.
- The front-end folder, `ORCHARD_APP_DATA` and the snapshots stay outside both repositories.

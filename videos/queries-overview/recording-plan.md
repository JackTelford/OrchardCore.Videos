# Recording plan: Queries: Save and Reuse Search and Database Queries

A person who reads only this file can record the video. Two chapters are recordings (`02-screen` and `03-create`); the
others are slides, drawn by `slides.py`, and need nothing from the site.

In this file a selector stands for `scene.Page.Locator(...)`. Source references are relative to the root of the
`OrchardCMS/OrchardCore` checkout, and `Q/` stands for `src/OrchardCore.Modules/OrchardCore.Queries/`.

## 1. The site

A throwaway local site, never a production one.

- Run `src/OrchardCore.Cms.Web` of an OrchardCore checkout on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own
  `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). Block outbound calls with `HTTP_PROXY` and
  `HTTPS_PROXY` set to a dead address: this video calls no external service.
- Provision it with Auto Setup through environment variables (`src/OrchardCore.Cms.Web/Program.cs:9` registers it with
  `AddSetupFeatures("OrchardCore.AutoSetup")`). Auto Setup is used, so no Setup screen and no sign-in form are recorded.

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>`: chosen outside git, the same value as `VIDEO_PASSWORD` |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

  Source of the variable names: `src/docs/reference/modules/AutoSetup/README.md`.
- Sign in once outside the capture with `Recorder setup` (user `admin`, password from `VIDEO_PASSWORD`). The sign-in is
  saved as `state.json` in the video's folder, which is git-ignored. Never write the password in a file of the repository.
- **Features:** nothing to enable. The Blog recipe enables `OrchardCore.Queries` and `OrchardCore.Queries.Sql`
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:68-69`), and `OrchardCore.Queries.Core` is enabled by
  its dependents (`Q/Manifest.cs`). Lucene and Elasticsearch stay disabled: the Add Query dialog must show one card, SQL.
- **Settings:** none. The admin list keeps its default layout, **List**, with row actions as **Buttons**
  (`AdminListOptions`: `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`).
- **External services:** none.
- **Seed data:** none to create. The Blog recipe saves one query, **RecentBlogPosts**, an SQL query that returns content
  items (`blog.recipe.json:1176-1186`). Its Blog theme template runs it by name
  (`src/OrchardCore.Themes/TheBlogTheme/Views/Content-BlogPost.liquid:34`).
- The fake data in this video is the site above, the query name `PublishedBlogPosts` typed in `03-create`, and the query
  text from the documentation (`src/docs/reference/modules/Queries/README.md`, "Examples").

## 2. Order, dependencies and the snapshot

1. Start the site, let Auto Setup finish, run `Recorder setup`, stop the site, and **copy its `App_Data` folder aside
   (the snapshot)** together with `state.json`.
2. Record `02-screen` first. It changes nothing on the site, but it expects exactly one query in the list.
3. Record `03-create` second. **It changes the site**: it adds the query `PublishedBlogPosts`.
4. Before any new take of `02-screen` or `03-create`, stop the site, restore the snapshot, and start the site again.
   After a take of `03-create`, the list holds two queries, so `02-screen` must not be recorded again without a restore.

The clips do not depend on each other otherwise.

## 3. Clip `02-screen` (storyboard chapter 02)

Starts on `/Admin` (the dashboard; `Admin` is the default admin URL prefix), signed in, with the left admin menu
collapsed (no group open). Open the page with `scene.OpenAsync("/Admin")` before `StartCaptureAsync()`.

`Q/Controllers/AdminController.cs:20` gives the page its address, `/Admin/Queries/Index`.

### Step `screen-1`

Actions:
1. Click the menu group **Search** (`#left-nav figcaption#search`, id from `Q/AdminMenu.cs:19-20`), pause 600 ms.
2. Click the group **Queries** (`#left-nav` text **Queries**, exact match; `Q/AdminMenu.cs:21`), pause 600 ms.
3. `ClickAndWaitForNavigationAsync` on **All Queries** (`li#searchallqueries a`; `Q/AdminMenu.cs:22-26`).
4. Wait until the row of **RecentBlogPosts** is visible (`.list-group-item` with the text `RecentBlogPosts`).
5. `HighlightAsync` on the name link of that row (`.list-group-item a` with the text `RecentBlogPosts`), no caption.

Visible when the narration ends: the **Queries** page (page title **Queries**, `Q/Views/Admin/Index.cshtml:6`), the
search box with the placeholder **Search**, the **Add Query** button, and one row, **RecentBlogPosts**, with a green ring
on its name. The admin menu shows Search, Queries and All Queries open, with All Queries as the current item. On this
site the **Search** group also holds **Indexes** (the Blog recipe enables Indexing), and the **Queries** group also holds
**Run SQL Query**, added by the SQL Queries feature; do not click either one.

Labels: **Search**, **Queries**, **All Queries** in `Q/AdminMenu.cs:18,21,22`; **Run SQL Query** in
`Q/Sql/AdminMenu.cs:20`; **Indexes** in `src/OrchardCore.Modules/OrchardCore.Indexing/AdminMenu.cs:22`; **Add Query** in
`Q/Views/Admin/Index.cshtml:25`; the row name is a link to Edit (`Q/Views/Query.Fields.SummaryAdmin.cshtml:4`).

### Step `screen-2`

The narration is about 11 seconds. The pauses below put each ring on the words that name it; if `durations.json` gives a
clearly different length, scale the pauses.

Actions:
1. `ClearHighlightsAsync()`, then `HighlightAsync` on the name link of the **RecentBlogPosts** row (`.list-group-item a`
   with the text `RecentBlogPosts`), no caption, pause 1800 ms ("Each row shows the name").
2. `ClearHighlightsAsync()`, then `HighlightAsync` on the source badge of the row (`.list-group-item .ta-badge`), caption
   `Source` (the badge's tooltip is **Source**), pause 2200 ms ("and a badge for the source").
3. `ClearHighlightsAsync()`, then `HighlightAsync` on **Run** (`.list-group-item a.btn` with the exact text `Run`),
   pause 2300 ms ("SQL Queries adds a Run button").
4. `ClearHighlightsAsync()`, then `HighlightAsync` on **Edit** (same locator, text `Edit`), pause 1500 ms,
   `ClearHighlightsAsync()`, then `HighlightAsync` on **Delete** (text `Delete`) and leave it. Do not click any of the
   buttons; **Delete** opens a confirmation.

Visible when the narration ends: the row with the badge **SQL query** and the buttons **Run**, **Edit** and **Delete**, in
that order, the last one ringed. No dialog is open.

Labels: **SQL query** and the tooltip **Source** in `Q/Views/SqlQuery.SummaryAdmin.cshtml:3-4`; **Run** in
`Q/Views/SqlQuery.Buttons.SummaryAdmin.cshtml:16` (added by the SQL Queries feature); **Edit** and **Delete** in
`Q/Views/Query.Buttons.SummaryAdmin.cshtml:6,12`.

### Step `screen-3`

Actions:
1. `ClearHighlightsAsync()`.
2. Click **Add Query** (`button.create`; `Q/Views/Admin/Index.cshtml:25`). Wait until `#modalAddQuery` is visible.
3. `HighlightAsync` on the SQL card (`#modalAddQuery .card` with the text `Queries the SQL database.`), no caption.

Visible when the narration ends: the dialog titled **Available Query Types** with a single card, **SQL**, the text
**Queries the SQL database.** and an **Add** button; the **Cancel** button in the footer. The card has a green ring.

Labels: **Available Query Types** in `Q/Views/Admin/Index.cshtml:50`; **SQL**, **Queries the SQL database.** and **Add**
in `Q/Views/Query-Sql.Link.cshtml:6,7,10`; **Cancel** in `Q/Views/Admin/Index.cshtml:72`.

## 4. Clip `03-create` (storyboard chapter 03)

Starts on the Queries page: `scene.OpenAsync("/Admin/Queries/Index")`, after restoring the snapshot (one query in the
list). Admin menu collapsed or as it was left; it is not part of this story.

### Step `create-1`

Actions:
1. Click **Add Query** (`button.create`). Wait until `#modalAddQuery` is visible, pause 800 ms.
2. `ClickAndWaitForNavigationAsync` on the **Add** link of the SQL card (`#modalAddQuery .card` with the text
   `Queries the SQL database.`, link **Add**). The address becomes `/Admin/Queries/Create/Sql`.
3. Wait until the **Name** field is visible.

Visible when the narration ends: the create page with the page title **New Sql query** (the source name is written as the
source is named, `Sql`), the field **Name** (empty, focused), and below it the **Schema** editor.

Labels: **New {0} query** in `Q/Views/Admin/Create.cshtml:4`; the card in `Q/Views/Query-Sql.Link.cshtml`.

### Step `create-2`

Actions:
1. `TypeAsync` into the **Name** field (`scene.Page.GetByLabel("Name", new() { Exact = true })`), text
   `PublishedBlogPosts`, delay 45 ms.
2. `HighlightAsync` on the field, no caption.

Visible when the narration ends: **Name** holds `PublishedBlogPosts`, ringed, with its hint **The technical name of the
query.** below it. Nothing else is filled in.

Labels: **Name** and the hint in `Q/Views/Query.Fields.Edit.cshtml:4,8`. The name rules (letters and digits, first
character a letter, unique) are in `Q/Drivers/QueryDisplayDriver.cs:90-103` (`ToSafeName`,
`src/OrchardCore/OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:201`).

### Step `create-3`

The narration is about 15 seconds: the Schema for the first 5, the scroll to the rest of the form until 8, **Return
Documents** until 13, and **Query** to the end. If `durations.json` gives a clearly different length, scale the pauses.

Actions:
1. `ClearHighlightsAsync()`.
2. `HighlightAsync` on the Schema editor (`div.monaco-json-settings-editor-container`), caption `Schema`. Leave it empty.
   Do not type in it. Pause 4500 ms.
3. `ClearHighlightsAsync()`, then `ScrollAsync(0, 500)` so the part of the form that comes from the SQL source is in
   view. Pause 2400 ms.
4. `HighlightAsync` on the **Return Documents** checkbox (`scene.Page.GetByLabel("Return Documents")`), pause 5200 ms,
   then `ClearHighlightsAsync()` and `HighlightAsync` on the **Query** editor (`.CodeMirror`), caption `Query`.

Visible when the narration ends: the lower part of the form: **Return Documents** (unchecked) with its hints **Check to
return the corresponding documents.** and **When checked, the result has to be a list of document ids.**, a yellow
warning about Liquid output expressions, and the **Query** editor (empty), ringed.

Labels: **Schema** and its hint **The schema the GraphQL API will use to return results.**
(`Q/Views/Query.Fields.Edit.cshtml:14,18`); **Return Documents** and the hints
(`Q/Views/SqlQuery.Edit.cshtml:26-29`); **Query** (`Q/Views/SqlQuery.Edit.cshtml:53`). The Schema is optional:
`Q/Drivers/QueryDisplayDriver.cs:86-89` validates it only when it is not empty. What Return Documents does:
`Q/Sql/SqlQuerySource.cs:91-140`.

### Step `create-4`

Actions:
1. `ClearHighlightsAsync()`.
2. Click the **Return Documents** checkbox label, so the box is checked.
3. `TypeAsync` into the Query editor (`.CodeMirror`), delay 30 ms (the specification allows 25 to 90 ms), text on a
   single line:
   `select DocumentId from ContentItemIndex where Published = true and ContentType = 'BlogPost'`
   (the example "returns all published Blog Posts" of `src/docs/reference/modules/Queries/README.md`, section
   "Executing SQL Queries, Examples").

Keep the actions under the narration (about 7 seconds): the typing takes about 3 seconds at 30 ms per character, so skip
the pause after the click.

Visible when the narration ends: **Return Documents** checked, and the Query editor holding the text above in one line,
with no error under it.

### Step `create-5`

Actions:
1. `ClickAndWaitForNavigationAsync` on **Save** (`button.save`; `Q/Views/Query.Fields.Buttons.cshtml:3`).
2. Wait until the row of **PublishedBlogPosts** is visible in the list, and the message **Query created successfully.** is
   visible (`scene.Page.GetByText("Query created successfully.")`).
3. `HighlightAsync` on the new row's name link.

Visible when the narration ends: the Queries page with two rows, **PublishedBlogPosts** (ringed) and **RecentBlogPosts**
(in either order: the list is not sorted), each with the badge **SQL query** and the buttons **Run**, **Edit**, **Delete**, and the message **Query created
successfully.** (`Q/Controllers/AdminController.cs:162`). If the message disappears before the narration ends, see
OPEN-QUESTIONS.md.

### Step `create-6`

Actions:
1. `ClearHighlightsAsync()`.
2. `TypeAsync` into the search box (`#search-box`), text `Published`, delay 60 ms. Click the search button
   (`button[name='submit.Filter']`, **Search**) with `ClickAndWaitForNavigationAsync`. Wait until only
   **PublishedBlogPosts** is listed (the filter matches names that contain the text:
   `src/OrchardCore/OrchardCore.Queries.Core/Services/DefaultQueryManager.cs`, `LocateQueriesAsync`).
3. `HighlightAsync` on the name link of that row, no caption.

Visible when the narration ends: the search box holding `Published`, and one row, **PublishedBlogPosts**, ringed.
Nothing is selected or deleted.

Labels: **Search** (`Q/Views/Admin/Index.cshtml:20-21`); the filter by name: `Q/Controllers/AdminController.cs:80-83`
(`QueryContext.Name`), submitted by `IndexFilterPOST` (`:111`).

## 5. The slides

`slides.py` draws these. They need no recording; the line says what each slide shows, and where the facts come from.

- `why-1`: heading "Without saved queries", two cards, **Copied** and **Edited everywhere**.
- `why-2`: heading "A saved query": the `Query` class (`src/OrchardCore/OrchardCore.Queries.Abstractions/Models/Query.cs:8-26`)
  with three cards, **Name**, **Source**, **Schema**, and two captions about `ReturnContentItems` and the source's own
  values (kept in the properties of the `Entity`).
- `why-3`: heading "Features of the module", four cards: **Queries Core Services**, **Queries**, **SQL Queries**,
  **Lucene and Elasticsearch** (`Q/Manifest.cs:10-42`; what the core feature registers: `Q/Startup.cs:36-62`; what
  Queries registers: `Q/Startup.cs:25-34`; the sources: `Q/Sql/Startup.cs:24`, `src/OrchardCore.Modules/OrchardCore.Lucene/Startup.cs:53-54`,
  `src/OrchardCore/OrchardCore.Elasticsearch.Core/ServiceCollectionExtensions.cs:18`). A feature enabled by its
  dependents only cannot be enabled or disabled by hand: `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:14-19`.
- `run-1`: heading "Run a query by name", four cards: Liquid (`Q/Startup.cs:47-58`, `Q/Liquid/QueryFilter.cs`),
  Razor (`Q/Razor/ContentQueryOrchardRazorHelperExtensions.cs`), Scripts (`Q/QueryGlobalMethodProvider.cs:18`), HTTP API
  (`Q/Controllers/QueryApiController.cs:10,29-37`).
- `run-2`: heading "Who can run a query", three cards: **Manage queries**, **Execute Api - All queries**,
  **Execute Api - RecentBlogPosts** (`src/OrchardCore/OrchardCore.Queries.Abstractions/QueryPermissions.cs:14-25`; one permission
  per saved query: `Q/Permissions.cs:30-35`; default roles: `Q/Permissions.cs:39-52`; the API answers Not Found:
  `Q/Controllers/QueryApiController.cs:49,56`).
- `recipe-1`: heading "The Queries recipe step", the step as `blog.recipe.json:1176-1186` has it (without its `Schema`),
  inside the `steps` list of a recipe, and two captions (`Q/Recipes/QueryStep.cs:44,53,75`).
- `deploy-1`: heading "Queries in a deployment plan", two cards: **All Queries** and **Queried Content Items**
  (`Q/Views/Items/AllQueriesDeploymentStep.Thumbnail.cshtml`, `Q/Views/Items/QueryBasedContentDeploymentStep.Fields.Thumbnail.cshtml`;
  the second one needs Deployment and Contents: `Q/Startup.cs:64`; it refuses a query that does not return content items:
  `Q/Deployment/QueryBasedContentDeploymentStepDriver.cs:55-58`).
- `source-1`: heading "1. Implement IQuerySource", `SqlQuerySource` (`Q/Sql/SqlQuerySource.cs:20-60`, trimmed).
- `source-2`: heading "2. Register it with a name", `Startup` of the SQL feature (`Q/Sql/Startup.cs:17-30`, trimmed; the
  `AddQuerySource` line is the bright one) and `AddQuerySource`
  (`src/OrchardCore/OrchardCore.Queries.Abstractions/Extensions/ServiceCollectionExtensions.cs:7-17`). The manager resolves a
  source by its name: `DefaultQueryManager.cs:74,112`.
- `source-3`: heading "3. Edit it with a display driver", `SqlQueryDisplayDriver.UpdateAsync` (`Q/Sql/Drivers/SqlQueryDisplayDriver.cs:81-123`, trimmed).
- `source-4`: heading "4. Add its card to Add Query", `Query-Sql.Link.cshtml` (`Q/Views/Query-Sql.Link.cshtml`, attributes on
  separate lines) next to the names of the Link views of SQL, Lucene
  (`src/OrchardCore.Modules/OrchardCore.Lucene/Views/Query-Lucene.Link.cshtml`) and Elasticsearch
  (`src/OrchardCore.Modules/OrchardCore.Elasticsearch/Views/Query-Elasticsearch.Link.cshtml`).
- `source-5`: heading "5. Read recipe data in a handler", `SqlQueryHandler` (`Q/Sql/SqlQueryHandler.cs`, trimmed, with the
  check of the query's `Source` that the narration mentions); the recipe step reaches it through `NewAsync` and `UpdateAsync`
  (`Q/Recipes/QueryStep.cs:62,75`).

## 6. Checks for private data

- The pages recorded are the admin Queries pages of a fresh site: no person's name, email or phone number is on them. The
  only account is `admin`; its email `admin@example.com` is not shown by these pages.
- Before publishing, OCR the video at one frame per second and at every scene change (ffmpeg `select='gt(scene,0.02)'`),
  then at 5 frames per second, and search the text for anything that is not `Orchard Core`, `admin`, the query names
  `RecentBlogPosts` and `PublishedBlogPosts`, and the labels above. No password, token, real host name or real email may appear.
- The video shows the site on `localhost`; the address bar is not part of the recording.
- No `state.json`, mask script, audio, MP4 or copy of `App_Data` is committed.

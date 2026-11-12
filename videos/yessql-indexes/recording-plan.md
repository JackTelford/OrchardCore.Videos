# Recording plan: YesSql Indexes

The video has one recording, the clip `06-tables` (chapter 06, "See the tables"). Every other chapter is a card or a
slide: they are drawn by `slides.py` and need no site. The clip only reads the database (the Run SQL Query page runs
SELECT statements only), so it changes nothing on the site.

Selectors below stand for `scene.Page.Locator(...)`, since the recorder's methods take locators. The file:line
references point into the Orchard Core source at commit `92db4a83`, under `src/`. Selectors read from the source have
not been tried on a running site: confirm each one with `Recorder probe` before recording (see
`OPEN-QUESTIONS.md`).

## The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore` at the commit above, `src/OrchardCore.Cms.Web`, built and run
  on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- **Auto Setup**, so no setup or sign-in form is recorded. Environment variables, from the Auto Setup documentation
  (`docs/reference/modules/AutoSetup/README.md`):
  - `OrchardCore__AutoSetup__Tenants__0__ShellName` = `Default`
  - `OrchardCore__AutoSetup__Tenants__0__SiteName` = `Orchard Core`
  - `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` = `America/Los_Angeles`
  - `OrchardCore__AutoSetup__Tenants__0__AdminUsername` = `admin`
  - `OrchardCore__AutoSetup__Tenants__0__AdminEmail` = `admin@example.com`
  - `OrchardCore__AutoSetup__Tenants__0__AdminPassword` = `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git)
  - `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` = `Sqlite`
  - `OrchardCore__AutoSetup__Tenants__0__RecipeName` = `Blog`
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls a provider.
- **Features**: the Blog recipe enables everything the clip uses, so nothing is enabled by hand:
  `OrchardCore.Queries`, `OrchardCore.Queries.Sql` (the feature "SQL Queries"), `OrchardCore.Roles` and
  `OrchardCore.Users` (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:30-80`). Before recording, open
  Tools, Features once and check that "SQL Queries" and "Roles" are enabled; stop and read
  `OPEN-QUESTIONS.md` if one is not.
- **Seed data**: none. The clip shows the rows that the Blog recipe and the setup create: the sample content items of
  the recipe (their titles are the recipe's own sample text) and the user `admin` with the role Administrator
  (`OrchardCore.Modules/OrchardCore.Users/Services/SetupEventHandler.cs:31`).
- **External services**: none.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` writes `state.json`
  (git-ignored). No mask script is needed, the site is new and holds fake data only.
- **Check before the first take**: with the signed-in browser, open
  `/Admin/Queries/Sql/Query?query=U0VMRUNUICogRlJPTSBVc2VyQnlSb2xlTmFtZUluZGV4` (the base64 of
  `SELECT * FROM UserByRoleNameIndex`) and `/Admin/Queries/Sql/Query?query=U0VMRUNUICogRlJPTSBDb250ZW50SXRlbUluZGV4`
  (`SELECT * FROM ContentItemIndex`). Both must show "Query executed in ... ms" and a result editor with rows. If
  the first one shows no result, `UserByRoleNameIndex` has no rows: do not record `tables-4` and read
  `OPEN-QUESTIONS.md`.

## Order of the clips, and state

There is one clip. It depends on no other clip and leaves the site as it found it, so any number of takes can follow
each other without restoring a snapshot. The only state a take leaves is in the browser: the admin menu remembers
which groups were left open (`OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:84-85`), which is why
each take starts from a new browser context with `state.json`, as the recorder does.

## Clip `06-tables`

**Starts on:** `/Admin` (the admin home), opened with `scene.OpenAsync("/Admin")` before the capture, with the menu
groups closed. Then `scene.StartCaptureAsync()`.

The narration of each step is 27 to 32 words, about 11 to 13 seconds. Keep each step's actions shorter than that.

### `tables-1`

- **Actions**
  1. `scene.ClickAsync(scene.Page.Locator("button.item-label.nav-group-label", new() { HasText = "Search" }), pauseAfter: 500)`
     opens the group Search.
  2. `scene.ClickAsync(scene.Page.Locator("ul[data-title='Search'] button.item-label", new() { HasText = "Queries" }), pauseAfter: 500)`
     opens the group Queries.
  3. `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("ul[data-title='Queries'] a.item-label", new() { HasText = "Run SQL Query" }))`.
  4. Wait until `scene.Page.Locator("label", new() { HasText = "Template" })` is visible.
- **Visible when the narration ends**: the page titled "SQL Query" (breadcrumb "Queries", "SQL Query"), with the
  empty Template editor, the Parameters editor, the Query button, and under the Template the hint "You can click
  here to query all content items." The admin menu shows Search, Queries, and Run SQL Query as the selected item.
  No error message, no result yet.
- **Labels from**: "Search", "Queries", "Run SQL Query" in `OrchardCore.Modules/OrchardCore.Queries/Sql/AdminMenu.cs:18-20`;
  "Template" in `OrchardCore.Modules/OrchardCore.Queries/Views/Admin/Query.cshtml:42`; the hint in
  `OrchardCore.Modules/OrchardCore.Queries/Views/Admin/Query.cshtml:46`; the menu markup in `OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:69-81`
  and `NavigationItem-admin.cshtml:92`. The feature name "SQL Queries" is in
  `OrchardCore.Modules/OrchardCore.Queries/Manifest.cs:35`.

### `tables-2`

- **Actions**
  1. `scene.HighlightAsync(scene.Page.Locator("span.hint a", new() { HasText = "click here" }))` draws the ring (no
     caption).
  2. After about 1.5 seconds, `scene.ClearHighlightsAsync()`, then
     `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("span.hint a", new() { HasText = "click here" }))`.
     The page loads `/Admin/Queries/Sql/Query?Query=U0VMRUNUICogRlJPTSBDb250ZW50SXRlbUluZGV4` (the link is built with
     `Url.Action(..., Query = matchAllQuery)`, `Query.cshtml:46`; the key is `Query`, with a capital Q), which runs the query at
     once (`OrchardCore.Modules/OrchardCore.Queries/Sql/Controllers/AdminController.cs:43-55`).
  3. Wait until `scene.Page.Locator("span.text-muted", new() { HasText = "Query executed in" })` is visible.
- **Visible when the narration ends**: the top of the page, with `SELECT * FROM ContentItemIndex` in the Template
  editor. Do not scroll yet.
- **Labels from**: the link text "click here" in `OrchardCore.Modules/OrchardCore.Queries/Views/Admin/Query.cshtml:46`; the query text in
  `OrchardCore.Modules/OrchardCore.Queries/Sql/ViewModels/AdminQueryViewModel.cs:7`; "Query executed in {0} ms" in `OrchardCore.Modules/OrchardCore.Queries/Views/Admin/Query.cshtml:82`.

### `tables-3`

- **Actions**
  1. `scene.ScrollAsync(0, 650)` (in two or three smaller scrolls) until `scene.Page.Locator("#monaco_editor")` is in
     the lower half of the viewport, and the section "Final SQL Query" has scrolled out or sits above it.
  2. Wait until `scene.Page.Locator("#monaco_editor .view-line")` has at least 5 lines: the editor formats its content
     about 300 ms after it appears (`OrchardCore.Modules/OrchardCore.Queries/Assets/ts/queries-admin-query.ts:34-36`).
  3. `scene.HighlightAsync(scene.Page.Locator("#monaco_editor"))` (no caption), kept until the end of the step.
- **Visible when the narration ends**: the result editor with the first rows as formatted JSON objects, one key per
  line, and the key `DocumentId` readable in the first object. The text "Query executed in ... ms" above it.
- **Labels from**: the result editor in `OrchardCore.Modules/OrchardCore.Queries/Views/Admin/Query.cshtml:100`; the key `DocumentId` is a column of the
  table (`OrchardCore.ContentManagement/Records/ContentItemIndex.cs:14`).

### `tables-4`

- **Actions**
  1. `scene.ClearHighlightsAsync()`, then
     `scene.NavigateAsync("/Admin/Queries/Sql/Query?query=U0VMRUNUICogRlJPTSBVc2VyQnlSb2xlTmFtZUluZGV4")` (the base64
     of `SELECT * FROM UserByRoleNameIndex`, no characters to percent-encode). Do not type in the editor.
  2. Wait until `scene.Page.Locator("span.text-muted", new() { HasText = "Query executed in" })` is visible, then scroll
     as in `tables-3` until `#monaco_editor` is in view, and wait for its formatted lines.
  3. `scene.HighlightAsync(scene.Page.Locator("#monaco_editor"))`.
- **Visible when the narration ends**: the result editor, highlighted, with formatted JSON that has the keys
  `RoleName` and `Count`; the text "Query executed in ... ms" above it. The Blog recipe creates no user besides
  `admin`, so the result is one object: `RoleName` is the role name normalized to upper case (`ADMINISTRATOR`,
  `OrchardCore.Modules/OrchardCore.Users/Startup.cs:90`) and `Count` is 1. The narration says a reduce index row does not
  point to one document: the object has no `DocumentId` key, which a person confirms (see `OPEN-QUESTIONS.md`).
- **Labels from**: the columns in `OrchardCore.Users.Core/Indexes/UserByRoleNameIndex.cs:9-10`; the index provider is
  registered when the feature Roles is enabled (`OrchardCore.Modules/OrchardCore.Users/Startup.cs:371-378`).

## Slides

The slides are drawn by `slides.py`; there is nothing to record. One line each, for a person checking the result:

- `why-1`: three cards: No ORM, Documents, A relational database.
- `why-2`: the JSON of the Content column of a document, and cards for the Id, the Type and the Version.
- `why-3`: four cards (a plain class, a regular SQL table, the same transaction, only indexed properties); the first
  two are bright.
- `why-4`: the same four cards; the last two are bright.
- `map-1`: the `ProductIndex` class, and three cards (MapIndex, Sku and Price, Published).
- `map-2`: the `ProductIndexProvider` class, with the Published line dimmed.
- `map-3`: the same class with only the Published line bright, and two cards about the versions.
- `map-4`: the `AliasPartIndex` class, and the part of its provider that returns null.
- `setup-1`: the Startup class with `AddIndexProvider` and `AddDataMigration`, and two cards (AddIndexProvider,
  AddScopedIndexProvider).
- `setup-2`: the `Migrations` class with the creation of the table bright.
- `setup-3`: the same class with the `AlterIndexTableAsync` call bright.
- `query-1`: the controller, with the query bright.
- `query-2`: four cards about the session: buffered writes, SaveChangesAsync, the shell scope, any other scope.
- `query-3`: the lines of the user picker that combine two indexes.
- `query-4`: two cards: IContentManager and ISession.
- `reduce-1`: the `UserByRoleNameIndex` class, and the Map and Group part of its provider.
- `reduce-2`: the Reduce and Delete part of its provider, and the call that creates the table.
- `declare-1`: three cards about several records for a document.
- `declare-2`: the provider of `ContentItemIndex` with `SingleRecordPerDocument` bright, and three cards.
- `declare-3`: the same provider with `ExclusiveToDocumentType` bright, and three cards.
- `declare-4`: the use of `NoIndexOptimizations` in the content picker, and two cards.

The opening card (`intro`) and the closing card (`outro`) are drawn from the storyboard.

## Checks for private data

Before publishing, scan the video at one frame per second and at every scene change, as the skill asks.

- The only data on screen is the Blog recipe's sample content (titles in the `DisplayText` column), the user name
  `admin`, and the user id in the `Owner` column. None is a real person's. If any other name, email or id shows, the
  site was not new: record again on a fresh site.
- The email on the site is `admin@example.com`, and it does not appear in these pages. The password is never typed on
  camera (`Recorder setup` signs in outside the capture) and never shown.
- The result editor must not show the table `Document` or the table of users: only `ContentItemIndex` and
  `UserByRoleNameIndex` are queried in this clip. Never query `Document` with `*` for a take: the documents of users
  hold password hashes.
- No `state.json`, no `App_Data` copy, no mask script in the commit.

# Recording plan: GraphiQL

A selector written here stands for `scene.Page.Locator(...)`. Orchard Core paths are relative to `src/` of the
OrchardCore checkout. The labels of GraphiQL itself (Docs, Prettify, History, Query Variables, the play button) are drawn
by the GraphiQL library that the module bundles, so their source is the quoted string in
`OrchardCore.Modules/OrchardCore.Apis.GraphQL/wwwroot/Scripts/graphiql-orchard.js` (one line, so no line number).
OPEN-QUESTIONS.md lists what to confirm on the running site while recording.

## The site it needs

- A local site from `OrchardCore.Cms.Web`, with its own `App_Data` outside both repositories (`ORCHARD_APP_DATA`), on
  `http://localhost:5010` (`VIDEO_BASE_URL`), provisioned by Auto Setup: recipe **Blog**, site name **Orchard Core**,
  time zone `America/Los_Angeles`, tenant `Default`, user `admin`, email `admin@example.com`, password chosen outside git
  and passed as `VIDEO_PASSWORD` (written `<password>` everywhere else). Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead
  address: the site calls no external service.
- `Recorder setup` signs in and saves `state.json` (git-ignored). Every clip starts in a new browser context loaded from
  `state.json`, so what GraphiQL keeps in the browser (history, the last query, panel sizes) and the state of the admin
  menu (collapsed or not) are empty again at the start of every clip. Nothing carries over from one clip to the next.
- Features to enable before recording (outside the capture, in `PrepareAsync` or by hand): **GraphQL**
  (`OrchardCore.Apis.GraphQL`, category Api). The Blog recipe does not enable it. Open `/Admin/Features` (Tools,
  Features), type `GraphQL` in `#search-box`, click `#btn-enable-OrchardCore_Apis_GraphQL`, and wait for
  `#btn-disable-OrchardCore_Apis_GraphQL` to appear. The GraphQL parts of the other features the Blog recipe enables
  (Contents, Autoroute, Markdown, Lists, Users) switch on with it. Source: `OrchardCore.Features/AdminMenu.cs`,
  `OrchardCore.Features/Views/Admin/Features.cshtml:27`, `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml`.
- Seed data (fake, outside the capture): the Blog recipe already holds one published blog post, "Man must explore, and
  this is exploration at its greatest". Add two more, so the result of a query has three blog posts:
  1. In the admin menu, open **Blog** (a top-level item of the Blog recipe), then click **Create Blog Post**. Source:
     `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:1197`, `OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52`.
  2. In **Title** (`TitlePart.Edit.cshtml:12`) type `Notes from the launch pad`, leave the other fields empty, and click
     **Publish** (`Content.PublishButton.cshtml:7`).
  3. Do it again with the title `A field guide to quiet places`.
  4. Check the seed: while signed in, open `/api/graphql?query={blogPost{displayText}}` in the recorder's browser. The
     response must list three `displayText` values, newest first (the default order, `ContentItemsFieldType.cs:472`):
     "A field guide to quiet places", "Notes from the launch pad", and the recipe's own post. A 404 means the feature is
     not enabled (the path is mapped only while the feature is enabled, `Startup.cs`, `Configure`); a response with
     `errors` means the query failed; a missing title means that post was saved as a draft, not published (the
     default `status` is published, `ContentItemsFieldType.cs:232-237`). Create the posts in the order above, so that the
     order of the results is the one the narration and the slides show.
- No external service, no mask script: the site is fresh and holds only the fake data above. The Explorer, the Docs and
  the result show content types and the three titles; no user name, email or phone number is queried.
- Snapshot: after the seed, stop the site, copy `App_Data` and `state.json` aside, start the site again. No clip changes
  the site (queries only read), so the snapshot is restored only after a failed take that edited content.

## Order of the clips

The five recordings are independent: each opens its own page from `state.json`. Record them in the storyboard's order:
`02-open`, `03-build`, `04-docs`, `05-variables`, `06-errors`. The three slide clips (`01-what`, `05-address`,
`07-code`) are drawn by `slides.py`, with nothing to record. No clip depends on another, none changes the site.

## What every clip does first (before `StartCaptureAsync`)

Unless the clip says otherwise (`02-open`):

1. `await scene.OpenAsync("/Admin/GraphQL")` (or the address given by the clip). The page is `GraphQL`/`Index` of
   `OrchardCore.Apis.GraphQL`, routed as `Admin/GraphQL`: `AdminController.cs:9` (`[Admin("GraphQL", "GraphQL")]`).
2. Wait until the schema has loaded: `scene.Page.Locator(".graphiql-explorer-root [data-field-name=\"blogPost\"]").First`
   is visible (before that the Explorer shows "No Schema Available"). Source: `Assets/js/App.tsx:52-58`.
3. Collapse the admin menu to give the screen the whole width: click `.leftbar-compactor` (title "Collapse / expand
   menu", `TheAdmin/Views/Layout.cshtml:79`), then wait 400 ms for the `left-sidebar-compact` transition
   (`Assets/css/graphiql-orchard.css:9`, `.left-sidebar-compact #graphiql`).
4. Scroll the Explorer so that the `blogPost` row is visible:
   `await scene.Page.Locator(".graphiql-explorer-root [data-field-name=\"blogPost\"]").First.ScrollIntoViewIfNeededAsync()`.

Locators used by several clips:

| Name | Locator |
|---|---|
| Explorer row of a field | `.graphiql-explorer-root [data-field-name="<field>"]`, then `.First`: the clickable row; an object field shows an arrow, a scalar field a check box. The fields of an open object are listed alphabetically under it |
| Query editor | `section.query-editor .CodeMirror` (its text: `document.querySelector('section.query-editor .CodeMirror').CodeMirror.getValue()`) |
| Result | `section.result-window` |
| Play button | `button.execute-button` (title "Execute Query (Ctrl-Enter)") |
| Prettify | `button[title^="Prettify"]` |
| History | `button[title$="History"]` |
| Explorer button | `button[title="Toggle Explorer"]` (label "Explorer", `App.tsx:114-120`) |
| Docs button | `button.docExplorerShow` (text "Docs") |
| Documentation Explorer | `section.doc-explorer` |
| Variables editor | `section.variable-editor .CodeMirror`; its tab: `.variable-editor-title-text` with the text "Query Variables" |

Wait for a result like this: `await scene.Page.WaitForFunctionAsync("() => document.querySelector('section.result-window').innerText.includes('\"blogPost\"')")`
(adapt the text to the step). Never type a query in the editor on camera: its brackets close themselves and its hints
pop up. Queries come from the Explorer or from the address.

## Clips

### `02-open`: Open GraphiQL (chapter 02)

Starts on `await scene.OpenAsync("/Admin")` with the admin menu expanded. Before the capture, make sure the Tools group is
closed: if `a[href$="/Admin/GraphQL"]` is visible, click the header of the Tools group
(`#left-nav li.nav-group > figure > figcaption > button.nav-group-label` filtered with the text "Tools": a group without a page of its own renders its label as a button that toggles the children, `TheAdmin/Views/NavigationItemLink-admin.cshtml`) and wait until the link is hidden. The menu
state is restored from the browser, so this check is needed on every take. Move the mouse to the middle of the page.

- `open-1`: click the header of the Tools group (`#left-nav li.nav-group > figure > figcaption > button.nav-group-label` with the text "Tools");
  wait until `a[href$="/Admin/GraphQL"]` is visible; `HighlightAsync` on it; pause 1000 ms;
  `ClickAndWaitForNavigationAsync` on it; wait for the schema (step 2 of "every clip", without collapsing the menu).
  When the narration ends: GraphiQL is open at `/Admin/GraphQL`, the menu still expanded, with Tools open and GraphiQL
  marked as the current item. Label: `OrchardCore.Apis.GraphQL/AdminMenu.cs:18-19` ("Tools", "GraphiQL"); the permission
  that shows the item: `AdminMenu.cs:21`, `GraphQLPermissions.cs:12` ("Execute GraphQL.").
- `open-2`: click `.leftbar-compactor` and wait 400 ms (the menu collapses and GraphiQL takes the width). Then, one after
  the other, with `PauseAsync(1200)` between them and `ClearHighlightsAsync()` after each: `HighlightAsync` on
  `.graphiql-explorer-root` with the caption "Explorer", on `section.query-editor` with the caption "Query", and on
  `section.result-window` with the caption "Result". When the narration ends: the three areas are visible side by side,
  no ring left. Label: `App.tsx:114-120` (the title of the left panel is the library's "Explorer").
- `open-3`: `MoveToAsync` the middle of `section.query-editor .CodeMirror-lines`, and `HighlightAsync` the editor without
  a caption. When the narration ends: the editor shows the comment block that starts with "# Welcome to GraphiQL" and
  lists the keyboard shortcuts ("Run Query:  Ctrl-Enter", "Auto Complete:  Ctrl-Space"). Source: the string "Welcome
  to GraphiQL" in `graphiql-orchard.js`.
- `open-4`: `ClearHighlightsAsync()`, then ring in turn, 1300 ms each, clearing between them: `button.execute-button` (caption
  "Run"), `button[title^="Prettify"]`, `button[title$="History"]`, `button[title="Toggle Explorer"]`. Do not click them.
  When the narration ends: the ring is on the Explorer button. Labels: the strings "Prettify", "History" and the title
  "Execute Query (Ctrl-Enter)" in `graphiql-orchard.js`; "Explorer" in `App.tsx:118`.

### `03-build`: Build and run a query (chapter 03)

Starts on `/Admin/GraphQL`, with "every clip does first".

- `build-1`: `ClickAsync` on `.graphiql-explorer-root [data-field-name="blogPost"]` (pauseAfter 1200). When the narration
  ends: the `blogPost` row is open (arrow pointing down) and lists its fields, and the editor holds a query with
  `blogPost` in it. The Explorer adds an object field without sub-fields (`_addFieldToSelections` in the bundle sets no
  selection set), so the editor is expected to show `blogPost` alone, with a red mark until a field is ticked. If it shows
  sub-fields, leave them. Rows of the Explorer come from the schema built by
  `OrchardCore.ContentManagement.GraphQL/Queries/ContentTypeQuery.cs:60-65` (the field name is the content type name in
  camel case: `blogPost`, `blog`).
- `build-2`: `ClickAsync` in turn (pauseAfter 900 each) on `.graphiql-explorer-root [data-field-name="displayText"]`,
  `[data-field-name="createdUtc"]` and `[data-field-name="path"]` (each with `.First`). Click a row only when its check box
  is empty: if the editor already holds the field, a click would remove it. Scroll the Explorer (`ScrollAsync`, with the
  cursor over `.graphiql-explorer-root`) when a row is below the visible part. There is no `autoroutePart` row: the
  Autoroute part is collapsed, so its `path` is listed with the other fields of `blogPost`. When the narration ends: the
  editor shows `blogPost` with `displayText`, `createdUtc` and `path`. Sources: `ContentItemType.cs:35,52-54`
  (`displayText`, `createdUtc`), `AutorouteQueryObjectType.cs:14` (`path`), `OrchardCore.Autoroute/Startup.cs:94-97`
  (`Collapse = true`), `TypedContentTypeBuilder.cs:271-307` (a collapsed part adds its fields to the content item type).
- `build-3`: `ClickAsync` on `button.execute-button`; wait for the result to contain `"blogPost"`. When the narration
  ends: the result shows `{ "data": { "blogPost": [ ... ] } }` with three entries, newest first ("A field guide to quiet
  places", "Notes from the launch pad", then the recipe's post), each with `displayText`, `createdUtc` and `path` (`blog/post-1`
  for the recipe's post, generated ones for the other two). Fields the Explorer ticked by itself may also be there.
- `build-4`: `ClickAsync` on `.graphiql-explorer-root [data-field-name="createdUtc"]`. When the narration ends: the
  `createdUtc` line is gone from the editor, its check box is empty, and the result still shows the earlier response.
  Source: the Explorer and the editor share one `query` state, `App.tsx:48,126-132,141`.

### `04-docs`: Browse the schema (chapter 04)

Starts on `/Admin/GraphQL`, with "every clip does first".

- `docs-1`: `ClickAsync` on `button[title="Toggle Explorer"]` (the Explorer hides; wait 400 ms), `ClickAsync` on
  `button.docExplorerShow`; wait for `section.doc-explorer`; `PauseAsync(2500)`; `ClickAsync` on
  `section.doc-explorer a.type-name` with the text "Query". When the narration ends: the Documentation Explorer shows the
  fields of Query, with the search box (placeholder "Search Query...") above them and, next to each field, its short
  description. Sources: "Documentation Explorer", "root types", "Search " in `graphiql-orchard.js`.
- `docs-2`: `TypeAsync` `blogPost` in `section.doc-explorer .search-box input` (delay 90); `PauseAsync(800)` (the
  search waits briefly after the last key); `ClickAsync` on `section.doc-explorer a.field-name` whose text is exactly
  `blogPost`, `.First` (the match in Query comes first; types and fields of other types whose names contain `blogPost`
  are listed under "other results"). When the narration ends: the field page shows the description "Represents a Blog Post." under the title
  of the panel. Source: `ContentTypeQuery.cs:65` (`S["Represents a {0}.", typeDefinition.DisplayName]`; the display name
  of BlogPost is "Blog Post", `blog.recipe.json`).
- `docs-3`: `MoveToAsync` `section.doc-explorer .doc-explorer-contents`, `ScrollAsync(0, 260)` until `#doc-args` is
  visible, then `HighlightAsync` on `#doc-args`. When the narration ends: the "arguments" list shows `first`, `orderBy`,
  `skip`, `status` and `where`, each with its description, and the ring is on the list. The server declares them as `where`,
  `orderBy`, `first`, `skip`, `status`; the Explorer sorts the arguments of every field by name in place when it renders
  (`args.sort` in the bundle) and the Docs panel reads the same objects, so the list is expected alphabetical: confirm
  on screen (OPEN-QUESTIONS.md). Source: `OrchardCore.ContentManagement.GraphQL/Queries/ContentItemsFieldType.cs:44-76`.

### `05-variables`: Variables (chapter 05, after the slide clip `05-address`)

Starts on this address (the query and its variables, percent-encoded as the page itself writes them,
`App.tsx:103-111`):

```
/Admin/GraphQL?query=query%20Posts(%24first%3A%20Int)%20%7B%20blogPost(first%3A%20%24first)%20%7B%20displayText%20%7D%20%7D&variables=%7B%22first%22%3A2%7D
```

Do "every clip does first" with this address (the `blogPost` row is in the Explorer as usual). The page opens with the
query on one line and the Query Variables pane open and showing `{ "first": 2 }` over three lines
(`App.tsx:80-100` formats the variables). If the pane is closed, click `.variable-editor-title-text` with the text
"Query Variables" before the capture.

- `vars-2`: `ClickAsync` on `button[title^="Prettify"]` (pauseAfter 1500), then `HighlightAsync` on
  `section.variable-editor` with the caption "Query Variables". When the narration ends: the editor shows the query
  over several lines, `query Posts($first: Int)` on top, and the ring is on the pane with `"first": 2`.
- `vars-3`: `ClearHighlightsAsync()`; `ClickAsync` on `button.execute-button`; wait for the result to contain
  `"blogPost"`; `PauseAsync(1500)`. Then `DoubleClickAsync` on `section.variable-editor .CodeMirror .cm-number` (selects
  the 2), `await scene.Page.Keyboard.TypeAsync("1")`, `PauseAsync(600)`, `ClickAsync` on `button.execute-button`, and wait
  until the result contains exactly one `"displayText"`
  (`...innerText.split('"displayText"').length === 2`). When the narration ends: the pane shows `"first": 1`, the editor
  still shows the same query, the result lists one blog post. With `first` of 2 the two newest posts come back ("A field guide
  to quiet places", "Notes from the launch pad"), with 1 only the newest (`ContentItemsFieldType.cs:203-228`; without
  `orderBy` the order is `CreatedUtc` descending, line 472).

### `06-errors`: When a query fails (chapter 06)

Starts on this address (the query `{ blogPost { title } }`):

```
/Admin/GraphQL?query=%7B%20blogPost%20%7B%20title%20%7D%20%7D
```

Do "every clip does first" with this address. Before the capture, wait for `section.query-editor .CodeMirror-lint-mark-error`
to be visible (up to 10 s). If no mark appears, click at the end of the query in the editor, press `End`, type a space
and delete it with `Backspace`, then wait again.

- `errors-1`: `MoveToAsync` on `section.query-editor .CodeMirror-lint-mark-error` and hold the cursor there (the editor's
  tooltip with the message appears next to it). When the narration ends: `title` is underlined in red in the editor, the
  result is still empty. Source: the library's validation of the query against the loaded schema; `blogPost` has no
  `title` field (`ContentItemType.cs`; the Title module has no GraphQL type).
- `errors-2`: `ClickAsync` on `button.execute-button`; wait for the result to contain `"errors"`. When the narration
  ends: the result shows an `errors` list with a message and no `data`. Source: `GraphQLMiddleware.cs:176-179` (the
  response is 400 when there are errors) and `App.tsx:31-42` (the body is shown whatever the status).

## Slide steps (nothing to record)

- `idea-1`: three cards, "A page of the admin", "The same endpoint", "Your session", all bright. Sources:
  `AdminMenu.cs`, `Views/Admin/Index.cshtml:67` (`~/api/graphql`), `App.tsx:28` (cookie).
- `idea-2`: the JSON body of a request (`query`, `operationName`, `variables`) and the response (`data`), under the title
  `POST /api/graphql` (the page appends `?culture=...` to the address, `Index.cshtml:67`; the slide leaves it out). The
  response lists the two newest posts, in the order the seed gives. Sources: the form in
  `docs/reference/modules/Apis.GraphQL/README.md`, `ContentItemsFieldType.cs:472`.
- `vars-1`: the address of the page with `query` and `variables`, and three cards that say what each parameter holds.
  Source: `App.tsx:80-111`.
- `code-1`: the `fetcher` function of `App.tsx`, trimmed, and three cards (POST, credentials, any status). Source:
  `App.tsx:17-42`.
- `code-2`: an HTTP client call (`curl`) to `/api/graphql` with a cookie-or-token card and a permission card. The token is
  a placeholder.
- `code-3`: the same call, with the three statuses 200, 400 and 401 as cards. Source: `GraphQLMiddleware.cs:174-179`.

## Checks for private data

- The Explorer, Docs and result show content type names, field names and the three blog post titles above. Look at every
  contact sheet: no email, no real name, no token.
- The admin menu shows the user name `admin`; the email `admin@example.com` is not on any screen of these clips.
- The address of the page, the `contentItemId` values and the paths are generated on the fresh site; the address bar is
  not in the recording.
- No `state.json`, no `App_Data` and no password is committed.

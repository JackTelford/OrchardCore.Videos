# Open questions: GraphiQL

The narration holds only what was read in the source of Orchard Core (`OrchardCore.Apis.GraphQL`, `OrchardCore.ContentManagement.GraphQL`,
`OrchardCore.Users/GraphQL`, `docs/reference/modules/Apis.GraphQL`) and in the bundle the module ships
(`wwwroot/Scripts/graphiql-orchard.js`, built from `Assets/js/App.tsx`). The bundle is a build of the GraphiQL library, so
its labels and behavior could be read as strings and code, but not seen. No site was run. A person confirms the list
below on the running site while recording, and says what differs before the video is published.

## Labels drawn by the GraphiQL library (read in the bundle, to confirm on screen)

- "Explorer" (the title of the left panel, and the label of the toolbar button, `App.tsx:118-119`), "Docs" (the button
  that opens the documentation panel), "Documentation Explorer" (its title), "root types", "Search Query..." (the search
  box of the Query type), "arguments" (a field's list of arguments), "Prettify", "History", "Query Variables", and the
  title "Execute Query (Ctrl-Enter)" of the play button. The narration says "the Explorer", "Docs", "the Documentation
  Explorer", "Prettify", "History", "the play button", "the Query Variables pane".
- What the narration says the toolbar does: Prettify formats the query, History lists the queries run before, and the
  Explorer button shows or hides the left panel (the last one is `handleToggleExplorer`, `App.tsx:66-68`). Prettify and
  History are the library's own buttons; their effect was not read in the module's source.
- The layout the narration describes: the Explorer on the left, the editor in the middle, the result on the right, the
  Docs button above the editor, the Query Variables bar under it.
- The first lines of the editor are the comment block "# Welcome to GraphiQL ..." with "Run Query:  Ctrl-Enter" and
  "Auto Complete:  Ctrl-Space", shown when the address holds no `query` and the browser has no stored query.

## Behavior of the Explorer, to confirm

- Clicking `blogPost` adds it to the query. In the bundle, `_addFieldToSelections` of the Explorer adds a field without a
  selection set, and the function that picks default fields (`id`, `edges`, `node`, `nodes`, else the first two leaf
  fields) is called only when an inline fragment is added. So the editor is expected to show `blogPost` alone, marked as
  an error until a field is ticked. Confirm on screen. If the Explorer does add fields, the narration of the step still
  holds (it names none), but a click on a row that is already ticked removes it: the recording plan says to click a row only
  when its check box is empty.
- There is no `autoroutePart` field on `blogPost`. The Autoroute module configures its part as collapsed
  (`OrchardCore.Autoroute/Startup.cs:94-97`), so `TypedContentTypeBuilder.cs:271-307` copies the part's `path` onto the
  content item type. The Explorer is expected to list `path` among the fields of `blogPost` (alphabetical order, between
  `owner` and `published`). The narration says `path` comes from the Autoroute part. Confirm that the row exists and is a
  check box.
- The comment block of the editor is replaced when the Explorer writes the first query. Confirm that the editor does not
  keep it, and that the rows `displayText`, `createdUtc` and `path` are reachable by `[data-field-name]` once `blogPost`
  is open.
- Unticking `createdUtc` removes the line from the editor (both read the same `query` state, `App.tsx:48,126-132,141`).
- The result lists the blog posts newest first (`ContentItemsFieldType.cs:472`, ordered by `CreatedUtc` descending when
  there is no `orderBy`). Confirm that the order on screen is "A field guide to quiet places", "Notes from the launch pad",
  then the recipe's post, which is what the narration ("the newest first") and the response on the slide show.

## Behavior of Docs, to confirm

- The root view lists `query: Query`, and no other root type: no module of the checked source adds fields to Mutation
  or Subscription (`SchemaService.cs` removes an empty root type). The narration only says to click Query.
- The fields of Query are listed with short descriptions, and a search for `blogPost` finds the field. Its page shows
  "Represents a Blog Post." (`ContentTypeQuery.cs:65`, with the display name "Blog Post") and the five arguments
  `first`, `orderBy`, `skip`, `status`, `where`, each with its description (`ContentItemsFieldType.cs:44-76`).
- The order of the arguments on screen. The server declares `where`, `orderBy`, `first`, `skip`, `status`, but the
  Explorer's field view sorts `field.args` by name in place (`args.sort` with `localeCompare` in the bundle) every time it
  renders, and the Docs panel reads the same schema objects. The narration lists them in that sorted order. If the
  panel shows the declared order, change the order of the list in step `docs-3`.
- A search for `blogPost` in the Query panel lists the field of Query first and, under "other results", the types and
  fields whose names contain it (`Search` result component of the bundle). The plan clicks the first field named exactly
  `blogPost`.

## Variables, address and errors, to confirm

- An address with `variables` opens the Query Variables pane, formatted over three lines (`App.tsx:80-100`, and the
  bundle opens the pane when initial variables exist). A double click on the number selects it.
- The query `{ blogPost { title } }` shows a red mark under `title` as soon as the schema has loaded, without an edit,
  and the response of a run holds `errors` and no `data`. `recording-plan.md` has a fallback if the mark needs a change.
- The HTTP statuses are read from `GraphQLMiddleware.cs:174-179`: 200 without errors, 401 when a validation error has the
  code "Unauthorized" (`RequiresPermissionValidationRule.cs`: a field or a mutation that needs a permission), 400 for
  any other error. Confirm in the network panel of a browser. The case of a caller without the Execute GraphQL
  permission is a challenge (`GraphQLMiddleware.cs:79`), not covered by the narration.
- "Queries run as the user who is signed in": `App.tsx:28` sends the cookie, and the middleware keeps the cookie user when
  the API scheme does not authenticate (`GraphQLMiddleware.cs:64-74`). Confirm with `{ me { userName } }` in GraphiQL:
  the response must name `admin` (`OrchardCore.Users/GraphQL/CurrentUserQuery.cs`).

## Left out of the narration on purpose

- **Request Headers.** The tab is part of the library, but `fetcher` in `App.tsx:17` takes only the query parameters and
  sends fixed headers, so a header typed in the tab is not sent. Confirm in the network panel before anyone relies on it.
- **Introspect.** The schema is requested once when the page loads (`App.tsx:52-58`) and passed to both panels. Whether
  the Introspect button refreshes it is not established. After a change to a content type, the safe way is to reload the page;
  the server builds a new schema when the content definitions change (`SchemaService.cs`).
- **Merge and Copy.** Not used in the recording (the clipboard and the address bar are not in a headless capture).
- **A user without the Execute GraphQL permission who opens `/Admin/GraphQL`.** `AdminController.Index` has no permission
  check of its own; only the menu item is hidden (`AdminMenu.cs:21`). What the page shows then was not checked.

## Pronunciation (`spoken` in storyboard.json), to confirm with `scripts/pronounce.py`

The samples were not made (the script writes audio files). Spellings to confirm: `GraphiQL` said "Graphical",
`Ctrl-` said "Control " (so "Ctrl-Enter" is "Control Enter"), `createdUtc` said "created U.T.C.", `orderBy` said "order by", `blogPost` said "blog post", `displayText` said "display text", "OAuth" said "O Auth". Also
listen for how the voice reads "Autoroute" and "JSON".

## The address on the first slide

`idea-2` titles the request `POST /api/graphql`. The page appends the current UI culture to it
(`Views/Admin/Index.cshtml:67`, `?culture=` and the name of `CultureInfo.CurrentUICulture`), and that name depends on the
site and the machine, so the slide leaves it out.

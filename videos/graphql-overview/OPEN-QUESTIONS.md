# Open questions: GraphQL: Query Your Content With One Request

What could not be verified without a running site. The narration states only what the source of Orchard Core at
commit `92db4a83` says; a person confirms the items below on a running site before recording, and changes the
narration (not the recording) when one of them is wrong. Nothing here was run: the content was written from the source
and the documentation only.

## Names that the source computes (confirm in the Explorer of GraphiQL)

Checked by reading the code, not by running it:

- `blogPost` and `blog` are the query fields of the content types `BlogPost` and `Blog`: the type name converted to
  camel case (`OrchardCore.ContentManagement.GraphQL/Queries/ContentTypeQuery.cs:50-83`,
  `OrchardCore.Apis.GraphQL/OrchardFieldNameConverter.cs`).
- `list` is the field of the List part of the Blog type, and `contentItems` is its field of content items. The part
  field name is the part name without the suffix "Part", in camel case
  (`OrchardCore.ContentManagement.GraphQL/Extensions/StringExtensions.cs:10`, `Queries/Types/TypedContentTypeBuilder.cs`,
  `OrchardCore.Lists/GraphQL/ListQueryObjectType.cs:24`).
- `subtitle` sits directly on `blogPost`, as a string, because the part named like its content type is collapsed
  (`Options/GraphQLContentOptions.cs:101`) and a text field is exposed as its text
  (`OrchardCore.ContentFields/GraphQL/Fields/ContentFieldsProvider.cs`).
- `AutoroutePart` is collapsed by default (`OrchardCore.Autoroute/Startup.cs:92-98`), so its `path` field sits directly
  on the content type, not under an `autoroute` field; the documentation does not say either way. A person can confirm
  `path` in the Explorer under `blogPost`.
- `displayText` is a field of every content type (`Queries/Types/ContentItemType.cs`); the Title part has no GraphQL
  type of its own.
- Query A returns the posts newest first (`Queries/ContentItemsFieldType.cs:472`), and `list.contentItems` returns
  them by their `Order` (`ListQueryObjectType.cs`). The narration says "newest first" only for query A.
- The `... on BlogPost` inline fragment selects the fields of the concrete type of each item of the list.

## The recording

- The selectors of GraphiQL (`.execute-button`, `.result-window`, `.query-editor`, `.graphiql-explorer-root`) were
  read from the bundled script, not from a running page. The page: Explorer on the left, editor in the middle, response
  on the right, toolbar with the play button and the **Explorer** button (`Assets/js/App.tsx:118`). The order comes
  from the source: the Explorer is rendered before GraphiQL in a row (`App.tsx:125-144`), and the bundled GraphiQL puts
  the query editor before the response in a row (`.editorBar`, `.queryWrap`, `.resultWrap`). Confirm the layout at
  1536x864, and what the editor shows when the page opens without a `query` parameter (the bundled default is the
  "# Welcome to GraphiQL" comment; `query-1` makes no claim about it).
- Whether `blogPost` is visible in the Explorer without scrolling (many content types add fields), and whether the
  response of query B fits the pane without scrolling.
- That loading `/Admin/GraphQL?query=...` replaces the content of the editor. In the bundled script the query of the
  address wins over a query kept in the browser storage, and the tab bar is not shown (`App.tsx` passes no `tabs`
  property); confirm it by loading address A and then address B in the same browser profile.
- The admin menu: how the **Tools** group opens in the redesigned admin, and the locators for **Tools**, **Features**
  and **GraphiQL**. The labels are verified (`OrchardCore.Features/AdminMenu.cs:26-27`,
  `OrchardCore.Apis.GraphQL/AdminMenu.cs:18-19`); the markup is not.
- That the row of the GraphQL feature, found with the search box, shows the heading **Api** and the description
  "Apis using the GraphQL Specification." (`Manifest.cs:4-9`), and that the Enable button has the id
  `btn-enable-OrchardCore_Apis_GraphQL`. The view builds the id with `Html.GenerateIdFromName(featureId)`
  (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`), the ASP.NET Core helper, which replaces
  the dots of the feature id with underscores by default; the id is by analogy with the example of the recorder
  (`#btn-enable-OrchardCore_Secrets`). Confirm it with `Recorder probe`.
- That **GraphiQL** appears in the menu of `admin` right after the feature is enabled. The menu entry needs the
  `ExecuteGraphQL` permission, which the Administrator role receives through `ExecuteGraphQLMutations`
  (`OrchardCore.Apis.GraphQL/Permissions.cs:16-25`, `Abstractions/GraphQLPermissions.cs:12`): `RoleUpdater` adds the
  default permissions of a feature to the existing roles when the feature is enabled
  (`OrchardCore.Roles/Services/RoleUpdater.cs:43-47`); confirm it in the roles of the site after `02-enable`.
- The seed (`recording-plan.md`, section 2) was not imported: confirm that the JSON import keeps `CreatedUtc`, keeps
  the `Path` of the Autoroute part as written, and fills the index of the Contained part (the `list` query reads
  that index). Confirm that the sample post can be deleted from Content Items with **Delete** and **Ok**.

## Claims of the slides that no recording shows

- Status codes and the authentication of `http-3` and `http-4`: read from `OrchardCore.Apis.GraphQL/GraphQLMiddleware.cs`
  (lines 63-80 and 174-179) and `OrchardCore.Infrastructure/Security/ApiAuthenticationHandler.cs`. Run one request
  without a session (expect 401), one with the session cookie of `admin` (200), and, on a site with the OpenID Connect
  Authorization Server and Token Validation features, one with a bearer token. Two cases give a 401 and the narration
  names only the second: a user without the Execute GraphQL permission is challenged before the query runs, with no
  `errors` list (`GraphQLMiddleware.cs:71-80`); a user with it who lacks the permission of a field, such as the view
  permission of a content type, gets the `errors` list and the status 401 (`RequiresPermissionValidationRule.cs`,
  `ContentTypeQuery.cs:69`). Confirm both with one request each.
- The curl commands of `http-1` and `http-2` and the variables example were not executed. `curl -g` is needed for the
  GET command because of the braces in the address.
- `schema-3`: "after you change a content type, the next request builds the schema again" comes from
  `OrchardCore.Apis.GraphQL/Services/SchemaService.cs` and `ContentTypeQuery.GetIdentifierAsync`. Confirm that adding a
  field to a content type shows in the Explorer after reloading GraphiQL.
- `schema-1`, card 4: the **Hide** and **Collapse** settings of the content type editor
  (`OrchardCore.ContentTypes/Views/GraphQLContentTypeSettings.Edit.cshtml`, `GraphQLContentTypePartSettings.Edit.cshtml`)
  are named on a slide only; confirm that they show in the editor of a content type and of a part.
- `config-1`: the values on the slide are an example (the documentation uses 50 and 100); the defaults are in
  `OrchardCore.Apis.GraphQL/Startup.cs:82-90`.

## Pronunciation (confirm each with `scripts/pronounce.py` before narrating)

The `spoken` entries of `storyboard.json` replace written identifiers by spaced words, so the captions keep the written
form. Two are not plain spacing and need a person's ear:

- `GraphiQL` is spoken as "graphical" (the name is pronounced that way by its authors; the voice may otherwise read
  it letter by letter).
- `/api/graphql` is spoken as "slash API slash GraphQL" and `application/graphql` as "application slash GraphQL".
- "Api", the label of the category in `enable-2`, is written as it is on screen; the voice may read it as a word instead
  of letter by letter. If it does, add an entry to `spoken` that keeps the label in the captions.

Also listen to "GraphQL" itself, "blogPost" ("blog post"), and the identifiers `ISchemaBuilder`, `BuildAsync`,
`GetIdentifierAsync`, `AddObjectGraphType` and `ObjectGraphType`.

# Open questions: GraphQL: Settings, Permissions and Hidden Fields

What was checked is the source of Orchard Core at commit `92db4a83` (no running site). The narration states only what
that source shows. A person confirms the items below on a running site, or by listening, before the video is
published.

## To confirm on a running site (the recording shows them)

1. **Anonymous role at the start of `02-roles`.** `OrchardCore.Apis.GraphQL/Permissions.cs:16-26` gives no default
   GraphQL permission to Anonymous, and the Contents module grants Anonymous "View all content"
   (`OrchardCore.Contents/Permissions.cs:92-96`). The role updater copies these defaults to existing roles when a
   feature is enabled (`OrchardCore.Roles/Services/RoleUpdater.cs:43-80`). Confirm that, after enabling GraphQL,
   "Execute GraphQL." is unchecked and "View all content" is allowed on `/Admin/Roles/Edit/Anonymous`. `roles-3` says
   the content permissions of the role decide what anonymous requests see. The claims of the Anonymous role are applied
   to every request, signed in or not, by `OrchardCore.Roles/Services/RolesPermissionsHandler.cs:43-56` (the core
   `PermissionHandler.cs:19` ignores users who are not signed in). Optionally request `/api/graphql` from a private
   window before and after `02-roles` and compare the answers.
2. **The Explorer of GraphiQL.** Confirm that the root fields `article`, `blog`, `blogPost` and `page` are listed
   before `03-type`, and that `article` is missing after the type is saved with **Hide**. The schema is cached per
   culture and rebuilt when the content definition identifier changes
   (`OrchardCore.Apis.GraphQL/Services/SchemaService.cs:27-60`, `ContentTypeQuery.cs:36-40`), so the reload should
   be enough. The selectors `.graphiql-explorer-root` and `.graphiql-explorer-<field>` were found in the text of the
   GraphiQL bundle (`wwwroot/Scripts/graphiql-orchard.js`: a node gets the classes `graphiql-explorer-node
   graphiql-explorer-` plus the field name), not on a page that was opened. Confirm them in the browser.
3. **The sidebar menu.** The paths come from the menu builders (`Access Control, Roles`; `Design, Content Definition,
   Content Types`; `Tools, GraphiQL`; `Tools, Features`). Confirm that each group opens on a click and that the
   links can be clicked by their visible text.
4. **Row text of the parts.** The parts list of **Blog Post** shows `D[partDefinition.DisplayName(), ...]`
   (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:153`). The migrations of `MarkdownBodyPart` and `AutoroutePart`
   set no display name, so the code gives "MarkdownBody" and "Autoroute"
   (`ContentPartSettingsExtensions.cs:54-63`). The narration describes the parts ("the Markdown body part", "the
   Autoroute part") and does not quote a row. Confirm the row text, and the page title "Edit Part - ...".
5. **Position of Hide on the type editor.** The option is placed at `Content:5`
   (`GraphQLContentTypeSettingsDisplayDriver.cs`), after the settings of the type. Confirm how far the recorder has to
   scroll, and that the **Save** button is reachable.
6. **The notices after saving.** "Role updated successfully." (`OrchardCore.Roles/Controllers/AdminController.cs:213`)
   and "Content type updated successfully." (`OrchardCore.ContentTypes/Controllers/AdminController.cs:225`) are read
   from the code. The notice after saving a part is not asserted anywhere, because the part clip never saves.
7. **Administrator role.** The narration says the Administrator role holds Execute GraphQL Mutations by default
   (`Permissions.cs:19-25`). The role editor of the Administrator role lists no permissions (it shows the notice
   "This system role provides comprehensive access by granting all available permissions."), so this is not visible
   on screen.

## Read from the code, not run

1. **Disabled mode of `MaxNumberOfResultsValidationMode`.** The narration says that Disabled "logs the excess instead" (true in the code; it does not say whether the result is capped).
   The documentation says that only the maximum number of results is returned. In
   `MaxNumberOfResultsValidationRule.cs:68` the rule assigns a new argument to its own lambda parameter, which does not
   change the query, and `ContentItemsFieldType.PageQuery` (`:205`) takes the original `first`, so the code shows no
   cap. Run a query with `first` above `MaxNumberOfResults` in Disabled mode (the default outside Development), and
   count the results. If the answer is capped, add that to `options-3`.
2. **`MaxComplexity` and `FieldImpact`.** The code passes `FieldImpact ?? 0` as `DefaultObjectImpact` and
   `MaxComplexity` to the complexity rule of the GraphQL library (`GraphQLMiddleware.cs:164-169`). The XML
   documentation of GraphQL.NET 8.8.5 (the copy in the local NuGet cache; the version at the Orchard Core commit was not
   checked) describes `DefaultObjectImpact` as the default complexity impact of object fields and `MaxComplexity` as the
   maximum calculated complexity, which is what the narration says. When `FieldImpact` is not set the code uses 0, not
   the library's default of 1; the narration does not state what happens then.
3. **`IgnoreField` on other graph types.** The narration claims the effect for `ContentItemType`, its interface and the
   `where` filter only (`ContentItemType.cs:75`, `ContentItemInterface.cs:30`, `ContentItemWhereInput.cs:52`). The
   options are also read for the fields of a collapsed part (`TypedContentTypeBuilder.cs:63`) and for content fields
   (`DynamicContentTypeBuilder.cs:78`), but a part's own graph type does not override `AddField`.
4. **The response to a request without Execute GraphQL.** The middleware calls `ChallengeAsync` with the API scheme
   (`GraphQLMiddleware.cs:65-79`). The narration says "challenges the request"; the status code and body depend on
   the authentication handler and are not described.
5. **Content permissions.** The narration summarizes `ContentItemFilters.PreQueryAsync` and `PostQueryAsync`, and the
   permission check of the type query (`ContentTypeQuery.cs:69`). The dynamic permission of a type is implied by the
   global one (`ContentTypePermissionsHelper.cs`), which is why "View own content" gives the items a user owns.
6. **`DefaultNumberOfResults` outside content type queries.** `options-2` says it for a content type query only
   (`ContentItemsFieldType.cs:205-210`). Other paged fields that use `PagingArguments` read it too
   (`ResolveFieldContextExtensions.cs:49-52`), but the `contentItems` field of a list part has its own default of 10
   (`OrchardCore.Lists/GraphQL/ListQueryObjectType.cs:26`). Query the `contentItems` of the list part of a Blog item without `first` to see it.
7. **Prevent Field Name Collision.** The narration only names the option. In the code it prefixes the names of the
   content fields of a collapsed part with the part name (`GraphQLContentOptions.GetFieldName`, used by
   `DynamicContentTypeBuilder.cs`).

## Wording

1. **Permission labels end with a period.** The role editor shows "Execute GraphQL." and "Execute GraphQL
   Mutations." (`GraphQLPermissions.cs:10`, `:12`). The narration says them without the period, which a voice would
   read as the end of a sentence. The plan and the slides keep the exact label where it is typed or shown.
2. **Pronunciation.** The `spoken` entries of `storyboard.json` only add spaces to option names
   (`MaxDepth`, `IgnoreField`, ...), and write `DiscoverableSterotypes` as "Discoverable Stereotypes" (the property is
   spelled `Sterotypes` in the code). "GraphiQL" is spoken as "graphical". Listen to each with `scripts/pronounce.py`
   before narrating. The entries "OrchardCore" (spoken "Orchard Core") and "SiteLayers" (spoken "Site Layers") need
   the same check. The voice may also need a fix for "GraphQL" itself, for the section name "Apis", and for the status
   "401".
3. **Slides.** They were built in memory and measured (no line or card is wider than its box), but no image was
   saved or looked at. Look at the fourteen slides once after `assemble.py` builds them.

# Open questions: Call the Orchard Core API With Client Credentials

Everything in the narration was read from the source at commit `92db4a83` and from the documentation pages in the
tracker entry. The points below follow from the source or from the OAuth standard, but could not be seen on a running
site. A person confirms each one on the recording site (the `S4` state of `recording-plan.md`) before the video is
narrated and recorded, and changes the narration if the site answers differently. The paths follow the rule at the top
of `recording-plan.md` (`src/OrchardCore/` for the shared libraries, `src/OrchardCore.Modules/` for the modules).

1. **Status codes** (`fail-1`, `fail-2` and the slides `fail-*`). Source: the three endpoints answer with
   `ChallengeOrForbid` (`OrchardCore.Contents/Endpoints/Api/*.cs`), which challenges when the user is not
   authenticated and forbids otherwise (`OrchardCore.Abstractions/Modules/Extensions/HttpContextExtensions.cs:17`);
   `GET` and `DELETE` return `NotFound`; `POST` returns `BadRequest` for an unknown content type and a validation
   problem when validation fails (`CreateEndpoint.cs`). To confirm: a call with no token, and a call with Token
   Validation disabled, return `401`; a call with a token whose role lacks `Access content via the api` returns `403`;
   a `GET` with an unknown id returns `404`; a `POST` without `ContentType` returns `400`; a `POST` that fails
   validation returns `400` with the names of the fields.
2. **Case-sensitive names** (`call-2`). Source: `ContentItemConverter.Read` compares the property names of the body with
   `nameof(ContentItem.ContentType)` and the like in a `switch`
   (`OrchardCore.ContentManagement.Abstractions/ContentItemConverter.cs:52-91`), so the camelCase body of the Contents
   documentation (`contentType`) is not read. The narration and the slide claim this only for those content item
   properties (`ContentType`, `ContentItemId`, ...). The names of the parts and fields inside the body are not claimed:
   they go into a JSON node (`ContentItemConverter.cs:93-98`) whose name matching depends on node options
   (`JOptions.Node` is case insensitive, `OrchardCore.Abstractions/Json/JOptions.cs`), which was not traced. To
   confirm: a `POST` of `{"contentType":"Article", ...}` answers `400`, and the PascalCase body of the plan answers
   `200`. Optional: whether `titlePart` instead of `TitlePart` is accepted (the video says nothing about it).
3. **The body creates and publishes an `Article`** (`call-1`, `result-1`, `result-2`). Source: the `Article` type of the
   Blog recipe has `TitlePart` and `AutoroutePart` with a pattern that needs no container
   (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`), and `TitlePartHandler` copies the title into the
   display text when the display text is empty. To confirm: the `POST` of the plan returns `200`, the response holds
   `ContentItemId`, `DisplayText` = `Hello from the sync job` and `Published` = `true`, and the article is listed
   under Content, Content Items with the badge `Published`.
4. **Token request without a scope** (`token-1`). The code sends no `scope`. Source: roles are added to the access
   token with their destination set explicitly, whatever the scope
   (`OrchardCore.OpenId/Controllers/AccessController.cs:497-516, 644-654`), and the current tenant is always an audience
   (`:683`). To confirm: the token request answers `200`, the response has an `access_token` property, and the token
   is accepted by `/api/content`.
5. **Default of the validation tenant** (`site-4`). Source: when the validation settings were never saved and the
   server feature is enabled, the tenant is the name of the current tenant
   (`OrchardCore.OpenId/Services/OpenIdValidationService.cs:58-66`). To confirm: after enabling both features and
   without saving, Settings, OpenID Connect, Token Validation shows `Default` in `Authorization server tenant`, with
   no error, and `/api/content` accepts a token without saving that page.
6. **Permissions copied into the token** (`flow-3`, `fail-3`). Source: the client credentials exchange adds the role
   names and the claims of the roles (the permissions) to the access token when it is issued
   (`AccessController.cs:497-514`; `OrchardCore.Roles.Core/Services/RoleService.cs:21`;
   `OrchardCore.Roles/Services/RoleStore.cs:237`). The permission check of a request reads the claims of the token
   (`OrchardCore.Infrastructure/Security/AuthorizationHandlers/PermissionHandler.cs:23`); no claims transformation or
   other code re-reads the roles of the application per request (searched: no `IClaimsTransformation`; the only
   readers of the application roles are the token exchange and the admin screens). To confirm: a token requested
   before a permission was added to the role still gets `403` on the call that needs it, and a token requested
   afterwards gets `200`.
7. **Implied permissions and delete** (`role-3`, `call-3`, `call-4`). Source: `EditContent` is implied by
   `PublishContent`, and `ViewContent` by `EditContent`
   (`OrchardCore.Contents.Core/CommonPermissions.cs:17,21,29`); nothing implies `DeleteContent` (`:25`). The granting
   service follows the implications (`OrchardCore.Infrastructure/Security/DefaultPermissionGrantingService.cs`). The
   code on the `call-3` slide prints the status of the delete instead of throwing, because the role of the video has
   no `Delete content for others`. To confirm: with only `Access content via the api` and `Publish or unpublish
   content for others`, the `GET`, the `POST` (create and update) succeed and the `DELETE` answers `403` (the
   program prints `Forbidden`); with `Delete content for others` added to the role and a new token, it answers `200`.
8. **Labels of the elements the plan names by selector**. The ids `btn-enable-OrchardCore_OpenId_Server`,
   `btn-enable-OrchardCore_OpenId_Validation`, `Checkbox.AccessContentApi`, `Checkbox.PublishContent`,
   `RoleEntries_0__Selected` and the like come from the views and ASP.NET Core's id generation, which was not run. To
   confirm: probe each page once with `Recorder probe` before writing the scenes.
9. **Pronunciation**. The `spoken` map holds two spellings for `call-2`, `ContentItemId` as "Content Item I.D." and
   `ContentType` as "Content Type", which a person confirms with `scripts/pronounce.py`. Listen to the narration for
   `OpenID`, `JSON`, `bearer`, `connect slash token`, `api slash content`, and `client ID`, and add a spelling to
   `spoken` only after a person has confirmed it with `scripts/pronounce.py`.
10. **The Authenticated role also applies** (`flow-3`, `call-4`). Source: `RolesPermissionsHandler` grants the
   permissions of the Anonymous and Authenticated roles to every authenticated principal, a client credentials token
   included (`OrchardCore.Roles/Services/RolesPermissionsHandler.cs:43-56`); the default stereotypes give both roles
   `View all content` (`OrchardCore.Contents/Permissions.cs:7-10,87-95`, applied by `RoleUpdater.cs`), but the
   Blog recipe and the setup were not run. To confirm: under Access Control, Roles, the Authenticated role has `View
   all content` allowed. If it has, a `GET` also succeeds for a role without `View all content`; the narration only
   says that `View all content` is the permission a read needs.
11. **Recommended parameters hint** (`app-3`). Checking `Allow Client Credentials Flow` also opens a hint that lists
   `scope` among the recommended parameters (`Views/Application/Create.cshtml:126`), while the code on the slides sends
   no scope. The video does not mention the hint. To confirm: the token request without a scope succeeds (question 4).

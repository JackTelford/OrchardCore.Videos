# Open questions: Content API

What was read in the Orchard Core source (commit 92db4a83, paths relative to `src/`) and what a person must confirm on
a running site before or while recording. Nothing below is stated as a fact in the narration unless the source shows it.
The narration claims only what the code shows; these points are the ones the code cannot settle by reading.

## Checks on the seed site

1. **Plain HTTP on localhost.** The OpenID server turns off OpenIddict's transport security requirement
   (`OrchardCore.Modules/OrchardCore.OpenId/Configuration/OpenIdServerConfiguration.cs:226`), so the token request on
   `http://localhost:5010` should work. The validation of the token on the API calls was not read for the same
   requirement. If the calls answer `401` with a valid token, run the site on HTTPS and use `https://` in the commands. The dead
   proxy of the site process should not matter: for a token issued by the same tenant the validation uses the keys of
   that tenant and does not fetch a discovery document (`.../Configuration/OpenIdValidationConfiguration.cs:114-160`);
   confirm it.
2. **Token request without a scope.** The code adds the tenant name as an audience whatever the scopes
   (`OrchardCore.Modules/OrchardCore.OpenId/Controllers/AccessController.cs:676-692`) and the validation accepts that
   audience (`.../Configuration/OpenIdValidationConfiguration.cs:158`). Confirm that a request with only
   `grant_type`, `client_id` and `client_secret` returns an access token, and that the first call answers `200`.
3. **The Token Validation feature is required.** The `Api` authentication scheme only forwards to the OpenIddict
   validation handler when that feature is enabled
   (`.../Configuration/OpenIdValidationConfiguration.cs:219-226`); otherwise it forwards to a scheme named `Bearer`
   that no feature registers (`OrchardCore/OrchardCore.Infrastructure/Security/ApiAuthenticationHandler.cs:27-33`).
   Confirm that the calls answer `401` before the feature is enabled, and `200` after it (the slide `auth-1` and the
   narration say to enable it). Also confirm that the token request is refused (error `unsupported_grant_type`) while
   **Allow Client Credentials Flow** is unchecked under **Settings**, **OpenID Connect**, **Authorization Server**, even
   though the application allows the flow: the server adds the grant type only when that setting is on
   (`OrchardCore.Modules/OrchardCore.OpenId/Configuration/OpenIdServerConfiguration.cs:136-139`), which is why `auth-1`
   says to check it.
4. **Token lifetime.** If the seed calls start answering `401` after a pause, request a new token (step 0). A token
   keeps the permissions the role had when the token was issued.

## Checks on what the slides show

5. **Fields of the response.** `slides.py` shows `ContentItemId`, `ContentType`, `DisplayText`, `Latest`, `Published`
   and the parts of the body, and hides the rest with `// ...`. The writer of the item is
   `OrchardCore/OrchardCore.ContentManagement.Abstractions/ContentItemConverter.cs:9-31`. Confirm on the real
   response: `DisplayText` equals the title when the body does not send one
   (`OrchardCore.Modules/OrchardCore.Title/Handlers/TitlePartHandler.cs:79-86`), and `Latest` and `Published` have the
   values on the slides `create-2`, `create-3`, `update-3`.
6. **Body names are case sensitive.** The reader compares the property names exactly
   (`ContentItemConverter.cs:54-92`), so a body with `contentType` has no content type and the endpoint answers `400`
   (`OrchardCore.Modules/OrchardCore.Contents/Endpoints/Api/CreateEndpoint.cs:61-64`). The step `api-3` says names are
   case sensitive; confirm the `400` (it is also in the table of section 2 of `recording-plan.md`).
7. **Ignored properties.** `Merge` removes `Published`, `Latest`, `ContentItemId`, `ContentItemVersionId`,
   `ContentType` and the three dates from the data of the item
   (`OrchardCore/OrchardCore.ContentManagement.Abstractions/ContentItemExtensions.cs:158-230`), so a body cannot set
   them; `ContentType` of the body still picks the type of an item that is created. A `ContentItemId` in the body of a
   create that matches no item is ignored too: the item gets a generated identifier. Confirm by posting a body with
   `"ContentItemId": "unknown"` and a `ContentType`: the response has another identifier.
8. **Problem details titles.** The titles "Authentication required" and "Access forbidden" are written by
   `ApiProblemDetailsMiddleware.cs:78` and `:83`, only when the response has no body of its own. Confirm that a call
   with no token has the JSON body of the slide `status-3` (with more members, hidden by `// ...`) and that the
   `403` of a token without the permission has the second title. The table of section 2 of `recording-plan.md` has the
   row for the `403`: make it on a copy of `App_Data`, with a second application whose role has no permission. The
   body is written after the OpenID Connect validation handler has set the status and the `WWW-Authenticate` header;
   whether that handler writes a body of its own was not verified (the middleware then writes none:
   `ApiProblemDetailsMiddleware.cs:47-53`).
9. **Validation answer.** The `400` with an `errors` object comes from `TypedResults.ValidationProblem`
   (`CreateEndpoint.cs:106` and `:141`), keyed by member name (`:157-172`). It was not reproduced here: the `Article`
   type of the Blog recipe has no required field. Confirm the shape if a content type with a required title is at
   hand (the setting `EditableRequired` of the title part gives the message "A value is required for Title."
   with the member name `Title`: `OrchardCore.Modules/OrchardCore.Title/Handlers/TitlePartHandler.cs:55-63`).
10. **Delete.** `DELETE` loads the published version (`DeleteEndpoint.cs:37`), so an item with only a draft answers
    `404`, and `RemoveAsync` clears `Published` and `Latest` on every version that has either flag
    (`OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:903-947`). Confirm that "Old notes" is
    absent from **Content Items** afterwards (the list shows the latest versions:
    `OrchardCore.Modules/OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs:44-51`).
11. **Draft on a non-versionable type.** The step `update-2` says "for a versionable type" that `draft` set to true
    keeps the published version live. That is what the code does for a versionable type such as `Article`
    (`blog.recipe.json:138`); for a type that is not versionable, the single version is marked as not published first
    (`DefaultContentManager.cs:165-172`) and the draft save unpublishes the item. The video does not describe that
    case. Confirm it on a type without the Versionable setting if it matters to the maintainer.

## Checks on the recorded clips

12. **`03-role`.** Confirm that the search `via the api` leaves one row, and `content for others` leaves the three rows
    of the plan (rows of other modules with the same words may show up; they are not highlighted). Confirm that the
    Effective column of "Edit content for others" is checked while its Allow column is not. The role page of the
    **Administrator** role shows no permission list (it says it grants every permission), which is why `ApiClient` is
    used.
13. **`07-admin`.** Confirm the order of the rows (the list is sorted by the last modification), the badges
    **Published** and **Draft** next to each other on "Spring planting notes", and the author shown on the rows
    created through the API. The principal of the client credentials flow has the display name of the application as its
    name (`AccessController.cs:488-491`); confirm that the list shows that fake name or nothing.
14. **Ownership.** An update through the API merges the body into the item with `Merge`, which turns the body model
    back into JSON with the converter; the converter writes `Owner`, `Author` and `DisplayText` even when they are
    `null`, and `Merge` copies each of these keys that is present (`ContentItemExtensions.cs:160-187`). Whether an update with a partial body clears the owner of the item was not
    verified, and the narration does not say anything about it. Check the owner of "Spring planting notes" after
    step 4 of the seed; if it is cleared, tell the maintainer.
15. **Publish permission on update.** The update branch checks `EditContent` and then publishes
    (`CreateEndpoint.cs:111-125`, `:147`); only the create branch checks `PublishContent`
    (`CreateEndpoint.cs:73`). The slide `perm-3` and the step `perm-3` list what each branch checks and nothing more.
    The step `perm-3` says what each call "adds": these are the permissions the endpoints demand, not a claim that no
    other permission can do the job.

16. **"Own" variations of the permissions.** The endpoints demand *View all content*, *Edit content for others*,
    *Publish or unpublish content for others* and *Delete content for others*. When the item has an owner equal to the
    `NameIdentifier` claim of the caller, the handler checks the "own" permission of the same name instead
    (`OrchardCore.Modules/OrchardCore.Contents/Security/ContentTypeAuthorizationHandler.cs:40-46` and `:85-93`; the
    comment in `CommonPermissions.cs:12-13` says so). The principal that the client credentials flow builds has a
    `sub` claim and no `NameIdentifier` claim (`AccessController.cs:481-492`), and `UpdateContentsHandler.cs:25-29` and
    `:45-49` store `null` as the owner in that case; `null == null` is then true in `HasOwnership`. So a role with only
    *Edit own content* and *Publish or unpublish own content* may be enough for items created through the API. Confirm
    with a role that holds only the "own" permissions if the maintainer wants the matrix to say more. The narration
    does not say that the "for others" permissions are the only way.
17. **Server and application flow.** The application page and the server settings page each have **Allow Client
    Credentials Flow**. The server one decides whether the grant type exists at all
    (`OrchardCore.Modules/OrchardCore.OpenId/Configuration/OpenIdServerConfiguration.cs:136-139`) and cannot be saved with the
    token endpoint disabled (`Services/OpenIdServerService.cs:173-179`). The application page receives the server
    settings (`Views/Application/Create.cshtml:19`) and its script hides the application's own box when the server
    does not allow the flow (`wwwroot/Scripts/openid-application-editor/application-create.js`, which is built from the
    shared editor component; the TypeScript of the component is not in this checkout). It also disables the box until
    the **Type** is **Confidential client**. Confirm on the running site that, with the order of the plan (features,
    server settings, role, application), the box shows after choosing **Confidential client** and that checking it
    reveals the **Client Credentials Roles** list (`Views/Application/Create.cshtml:233-235`).

## Voice

18. The `spoken` entries of the storyboard have not been checked with `scripts/pronounce.py`. Check them before
    narrating: `api/content` (said "API slash content"), `application/json` ("application slash JSON"),
    `ContentItemId` ("Content Item I.D."), `ContentType`, `DisplayText`, `GraphQL` ("Graph Q.L.") and the verbs `GET`,
    `POST`, `DELETE`, written in capitals in the narration and said as words. Also listen to "OpenID Connect" and
    "JSON". If an entry sounds wrong, change it in `storyboard.json` `spoken` only.
    Also listen to how the voice reads the status codes (200, 400, 401, 403, 404); if it says them badly, add `spoken` entries such as "four oh four".

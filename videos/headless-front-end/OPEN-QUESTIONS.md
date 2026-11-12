# Open questions: headless-front-end

What was checked in the source, and what a person must confirm on a running site or with the voice before recording.
The narration states only what the source shows; the points below are behaviors of a running site, a browser or the
voice.

## Pronunciation (check with `scripts/pronounce.py` before narrating)

- `GraphiQL` is spoken through the storyboard entry `"GraphiQL": "Graphical"`. Confirm that the voice says
  "graphical", as the project does.
- `CORS` has no entry. Listen to it: if the voice spells it letter by letter or says something other than "cors",
  add an entry to `"spoken"`, written with dots and no spaces if it is spelled out.
- `Redirect Uris` (the label of the field in `openid-4`) is spoken through the storyboard entry
  `"Redirect Uris": "Redirect U.R.I.s"`. Confirm that the voice says the letters, and adjust the entry if it does not.
- Also listen to `OPTIONS`, `curl`, `OpenID`, `GraphQL` and "single-page app".
- The narration never reads an address or a path aloud; the endpoint paths are on slides only.

## Behavior of the running site

1. **Anonymous queries.** Source: `GraphQLMiddleware.cs` authorizes `ExecuteGraphQL`; `ContentItemFilters.cs` limits
   the result to what the caller may view; the role Anonymous gets `View all content` from the Contents reader
   permissions (`OrchardCore.Contents/Permissions.cs:91-95`, label in `OrchardCore.Contents.Core/CommonPermissions.cs:29`). Confirm that after `graphql-2` the stand-in page, which
   sends no cookie and no token, receives the three titles. If the result is empty, check that Anonymous still has
   `View all content`.
2. **The blocked call before the policy.** Source: with `OrchardCore.Cors` disabled no CORS header is sent; the CORS
   documentation says a rejected request often appears as a generic network error. Confirm that the page shows
   `The browser blocked the request: Failed to fetch` and that the browser's message is not different on the
   recording machine.
3. **The preflight after the policy.** The page sends a JSON `POST`, so the browser sends an `OPTIONS` request first.
   Source: the CORS middleware is registered before authentication (`OrchardCoreConstants.ConfigureOrder.Cors` is lower than
   `Authentication`), and the default policy allows any method and header. Confirm in the browser's network tools
   that the preflight succeeds and that no error is shown after `cors-5`.
4. **JSON Import of the seed.** The two `BlogPost` items follow the shape of the items of the Blog recipe, without a
   blog to contain them, an owner or an author. Confirm that the notice `Recipe imported successfully!` appears and
   that the posts are published. If not, create them from the content items list with the same titles.
5. **Ids of the Enable buttons.** `btn-enable-` plus the feature id with dots replaced by underscores comes from
   `GenerateIdFromName` and the skeleton `Scenes.cs`; it was not run. Confirm `btn-enable-OrchardCore_Apis_GraphQL`,
   `btn-enable-OrchardCore_Cors`, `btn-enable-OrchardCore_OpenId_Server` and `btn-enable-OrchardCore_OpenId_Validation`.
   Fallback: the link `Enable` inside the row whose text holds the feature's name.
6. **Notice texts.** `The feature GraphQL was enabled.`, `The feature CORS Configuration was enabled.` and
   `The feature OpenID Connect Token Validation was enabled.` come from the code in `AdminController.cs:317` with the
   features' manifest names. When the Authorization Server is enabled with its dependencies, the notice may be the
   plural form (`The following features were enabled: ...`); use whichever text appears as the wait condition.
7. **The roles editor.** The permission rows are filtered by the text of the search field; confirm that typing
   `Execute GraphQL` leaves the row `Execute GraphQL.` visible and that the checkbox `Checkbox.ExecuteGraphQL`
   can be clicked (the `Anonymous` role is not the Administrator role, so the boxes are enabled).
8. **GraphiQL.** The `query` parameter, the `Prettify` button and `button.execute-button` come from `App.tsx` and from
   the built bundle. Confirm the pre-filled query, the layout after `Prettify`, and that the result pane lists three
   `displayText` values.
9. **The Admin menu.** The narration names menu paths (`Tools, GraphiQL`; `Settings, Security, Cross-Origin Resource
   Sharing`; `Access Control, OpenID Connect, Applications`). They come from the admin menu builders. The clips open
   the pages by their addresses and do not show the menu; confirm each address opens the page.
10. **The server with its default settings.** Source: `OpenIdServerService.GetSettingsFromContainer` returns the
    authorization code and refresh token flows, with every endpoint, when nothing was saved, and the routes are mapped
    from those values. Confirm on the recording machine that the page `Authorization Server` shows these boxes
    checked.
11. **Certificates.** `OpenIdServerService.GetSigningKeysAsync` and `GetEncryptionKeysAsync` create and keep their own
    keys when no certificate is selected, but the view `OpenIdServerSettings.Edit.cshtml` shows the warning `A
    certificate must be added to your server to set up the OpenID Connect module.` in the `Advanced Options` section
    when the machine has no certificate in its stores. The clip stays above that section. Confirm that the server
    issues tokens without a certificate on the recording machine (for example with a client credentials request from
    the sibling video's flow), because the narration of `openid-2` says that the server runs with the defaults.
12. **Token Validation by default.** Source: `OpenIdValidationService.GetSettingsFromContainer` returns the current
    tenant as the authorization server when the Server feature is enabled and nothing was saved, and
    `OpenIdValidationConfiguration` then forwards the `Api` authentication scheme to the validation handler. Confirm
    on the page that `Authorization server tenant` shows `Default`, and with a real bearer token that the GraphQL
    endpoint accepts it. The video does not show a token call.
13. **The authorization code flow slides.** `flow-1` and `flow-2` describe the flow from `AccessController.cs`
    (`Authorize` challenges an unauthenticated user, shows the consent form when the application's `Consent type` is
    `Explicit consent` and no permanent authorization exists yet, then signs in with a code; `Token` has no
    `DisableCors`, while `Authorize` and `Logout` do) and from the standalone Media recipe, which allows the app origin for
    `/connect/token`. Nothing was run: a browser app and a redirect are not recorded.
14. **The `roles` scope.** The video only says to select scopes "such as profile and roles". In `AccessController.cs`
    (`GetDestinations`) a role claim reaches the access token whatever the scopes are, and the identity token only
    with the `roles` scope; the Media documentation says the scope "carries them into the token". Confirm the
    behavior before anyone writes more on it.
15. **The recipe slides.** The steps follow `media-api-pkce.recipe.json` and `media-api-standalone.recipe.json`
    with the values of this video. The value `"explicit"` of `ConsentType` and the `ScopeEntries` follow the UI
    options of the application editor. Import the two slides' steps on a fresh site with **Tools > Deployments >
    JSON Import** to confirm they are accepted, and add the step `ReloadTenant` after the CORS step if the policy is
    not applied (the Media recipe ends with it).

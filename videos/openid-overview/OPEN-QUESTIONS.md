# Open questions: what a person must confirm on a running site

Everything in the narration is read from the Orchard Core source (commit 92db4a83) and its documentation. The points below
could not be run in the authoring session. Check each one while recording; if one is different, change the narration step
named in brackets and say so in the pull request.

1. **Core Services is hidden on the Features page** [`features-2`, `enable-3`]. The page selects "Hide on demand" in the
   Visibility filter by default (`src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:65`) and the feature
   OpenID Connect Core Services is `EnabledByDependencyOnly` (`OrchardCore.OpenId/Manifest.cs:16`), so the narration says the list
   hides it. After typing `OpenID` there should be five rows. Confirm that no Core Services row is shown, and that the group is
   headed "OpenID Connect".
2. **Rows and their order** [`features-2` to `features-7`]. The rows are sorted by name; the narration follows the order
   Authorization Server, Client Integration, Management UI, Secrets, Token Validation. The filter looks at the name, the
   identifier and the description (`FeatureEntryDisplayDriver.cs:20`); no `Manifest.cs` under the source's `src` folder other
   than `OrchardCore.OpenId/Manifest.cs` contains "openid" in any case, so no other feature should match. Confirm it on the
   screen, and that the Secrets row has no yellow "missing dependency" badge (the Secrets module is referenced by
   `OrchardCore.Application.Cms.Core.Targets.csproj:115`, so it should be present).
3. **The admin menu** [`tour-1`, `tour-5`, `tour-6`]. The menu paths come from the navigation providers
   (`OrchardCore.OpenId/ServerAdminMenu.cs:18-22`, `ManagementAdminMenu.cs:18-27`). Confirm in the admin theme how **Settings**,
   **Access Control** and **OpenID Connect** open (click, hover), find a locator for each item with `Recorder probe`, and confirm
   that **Access Control** holds **OpenID Connect** with **Applications** and **Scopes** once the Management UI feature is on.
   Both **Settings** and **Access Control** hold a group named **OpenID Connect**, and both groups use the id `openid`
   (`ServerAdminMenu.cs:20-21`, `ManagementAdminMenu.cs:20-21`), so each locator must be scoped to its parent group.
4. **Enable and the notification** [`enable-2`]. The Enable link posts, the tenant is reloaded, and the page redirects to the
   list. Confirm how many seconds the reload takes, and that the green notification "The feature OpenID Connect Authorization
   Server was enabled." is on screen when the narration of the step ends. It is dismissed five seconds after it appears
   (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:70-72`). Cut a slow reload with `OffCameraAsync`.
   The notification names only the feature that was clicked: the controller passes the requested features, not their
   dependencies, to the message (`src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:74-79`), so the narration does not
   promise that it lists the dependencies.
5. **Dependencies are enabled with the server** [`enable-1`, `enable-3`]. The Enable action asks for the dependencies to be
   enabled too (`force` is true in `OrchardCore.Features/Controllers/AdminController.cs:247`). Confirm that after the take the
   Management UI row shows Enabled and Core Services is enabled (show it once with the Visibility filter if there is any doubt).
6. **The server settings page before anything is saved** [`tour-2`, `tour-3`]. With no saved settings the service returns
   defaults: every endpoint enabled, and only the Authorization Code Flow and the Refresh Token Flow allowed
   (`OrchardCore.OpenId/Services/OpenIdServerService.cs:68-83`). Confirm that the page shows exactly these checkboxes checked, that
   the other Flows checkboxes are unchecked and visible (the script shows them from the state of the endpoints,
   `OrchardCore.OpenId/Assets/ts/openid-server-settings.ts`), and that no validation message is shown on first load. The warning "The website might be restarted upon saving the settings
   ..." is written in the page (`OrchardCore.OpenId/Views/ServerConfiguration/Index.cshtml:10-12`) and again by the wrapper that
   `AddTenantReloadWarningWrapper` adds to the editor (`OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml`), so it may be
   shown twice in a row. The narration speaks of one warning; if two show, ring the first.
7. **Advanced Options and private data** [`tour-4`]. The certificate lists show the certificates of the recording machine's
   stores, and the warning "A certificate must be added to your server ..." appears when there are none
   (`OrchardCore.OpenId/Views/OpenIdServerSettings.Edit.cshtml:267-303`). The server creates its own signing and encryption
   certificates when none is selected (`OpenIdServerService.cs`, `GetSigningKeysAsync`), so that warning does not describe the
   server's state. The page script also hides the whole certificate row, warning included, until a store location is chosen
   (`Assets/ts/openid-server-settings.ts`, `refreshEncryptionCertificates`), and the **Authority** field is empty while nothing
   is saved. The video does not show this area: the clip stops at the notice under the heading. Confirm that no
   certificate name, subject or host name is visible at the end of `tour-4`, and OCR the clip for them.
8. **The default scopes** [`tour-6`]. Enabling the server creates the scopes Email, Profile, Phone and Roles through a deferred
   task (`OrchardCore.OpenId/Migrations/DefaultScopesMigration.cs:14-64`). Confirm that the list shows them right after
   `04-enable` (reload once if needed), with the display names written like this. The list sorts by display name
   (`OrchardCore.OpenId/Controllers/ScopeController.cs:103`), so the narration names them in the order they appear on screen:
   Email, Phone, Profile, Roles.
9. **The Applications list is empty** [`tour-5`]. The empty message is "Nothing here! There are no applications at the moment."
   (`OrchardCore.OpenId/Views/Application/Index.cshtml:36`). Confirm that the site has no application before the take.
10. **The sign-in flow of the slides** [`flow-1` to `flow-4`]. The slides describe the Authorization Code Flow from
    `OrchardCore.OpenId/Controllers/AccessController.cs` (`Authorize`: redirect to the login page when nobody is signed in, an
    approval page with the text "Do you want to grant ... access to your data? (scopes requested: ...)" when the application's
    consent type is explicit, then a code sent back to the application). It was not run end to end. A person who has a test
    client should confirm the order of the screens once; nothing in the video shows them.
11. **Token Validation and the site's API** [`flow-4`, `features-7`]. The claim that the feature lets the API of the site accept
    the tokens comes from `OrchardCore.OpenId/Configuration/OpenIdValidationConfiguration.cs:219-226` (it replaces the scheme the
    API authentication uses) and from the Contents API endpoints, which authorize with that scheme
    (`OrchardCore.Contents/Endpoints/Api/CreateEndpoint.cs:34`). It was not tested with a token on a running site.
12. **Pronunciation.** The storyboard has no `spoken` entries. Check with `scripts/pronounce.py` how the voice says "OpenID",
    "OAuth", "API" and "UI" before narrating, and add an entry only after a person confirms the spelling.

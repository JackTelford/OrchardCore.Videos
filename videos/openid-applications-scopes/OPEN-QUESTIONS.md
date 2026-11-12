# Open questions: OpenID Connect Applications and Scopes

Everything in the narration was read from the Orchard Core source at commit `92db4a83` (paths relative to
`src/OrchardCore.Modules/OrchardCore.OpenId/`, written `OpenId/...`). Nothing was run: no site was started. A person
must confirm the following on a running site before recording, and fix the narration (`storyboard.json`) if the site
behaves differently.

## Behavior that comes from the bundled script, not from C# or views

1. **Which flows the application form shows.** The narration (`service-4`) says the form lists only the flows that the
   authorization server allows, that Client Credentials Roles appears when Allow Client Credentials Flow is checked
   (`service-5`), that Public client hides the Client Secret row and disables Allow Client Credentials Flow (`web-1`),
   that Allow Refresh Token Flow is disabled until a flow that uses refresh tokens is checked (`web-2`), and that the
   redirect section (Redirect Uris, Allow End Session Endpoint, Require Proof Key for Code Exchange, Require Pushed
   Authorization Requests, Consent type) opens when Allow Authorization Code Flow is checked (`web-3` to `web-5`).
   What was checked: the `data-*` attributes of `OpenId/Views/Application/Create.cshtml:16-24`, and the logic in the
   bundled script `OpenId/wwwroot/Scripts/openid-application-editor/application-create.js`, which was read and does
   all of this (flows hidden and unchecked when the server settings do not allow them; the Client Secret row
   collapsed, and Allow Client Credentials Flow disabled and unchecked, when the Type is not confidential; the Client
   Credentials Roles section shown while Allow Client Credentials Flow is checked; Allow Refresh Token Flow disabled
   unless the password, authorization code or hybrid flow is checked; the redirect section shown while the implicit,
   authorization code or hybrid flow is checked; Post-Logout Redirect Uris shown while Allow End Session Endpoint is
   checked). The component's TypeScript source is in a package that is not in this source tree, so only the bundle
   was read. To confirm: with the server settings of the recording plan, open New application and check that exactly
   the three flows are listed, and that each of the behaviors above happens on screen.
2. **Where the flows block ends up in the viewport.** `web-1` expects the Type list and the grayed-out Allow Client
   Credentials Flow to be visible together at 1536x864 after the Client Secret row collapses. If they are not, the
   recorder scrolls by the smallest amount that shows both.

## What the source does not show

3. **Enforcement of Allowed Scopes and flows.** The narration says what the labels say ("the scopes an application is
   allowed to request", `scopes-6`) and what the code stores: the checked scopes are saved as permissions of the
   application (`OpenId/OpenIdApplicationSettings.cs:230-235`). Whether a token request for a scope that is not
   checked is rejected is done by the OpenIddict library, which is not in this source. The video does not claim an
   error message. A person may confirm by requesting a token outside the video.
4. **The default scopes.** `OpenId/Migrations/DefaultScopesMigration.cs` creates Email, Profile, Phone and Roles in a
   deferred task when the Authorization Server feature is first enabled (`OpenId/Startup.cs:127`). Confirm that the
   Scopes list shows exactly these four after enabling the feature in the plan, with the descriptions and the name
   badges `scopes-1` describes, and that they are sorted Email, Phone, Profile, Roles.
5. **The roles under Client Credentials Roles.** The list is every role of the site
   (`OpenId/Controllers/ApplicationController.cs:137-143`). The plan checks `Editor`, which the Blog recipe creates
   (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:88-110`). Write down the exact list on the
   recording site (Administrator, and perhaps Anonymous and Authenticated, with the four Blog roles), and confirm
   that `Editor` is in it.
6. **Enabling the feature.** The plan uses Tools > Features, search, then Bulk Actions > Enable. Confirm that the
   Management UI and Core Services features are enabled with the Authorization Server feature without a confirmation
   dialog that needs another click, and note any dialog in the plan.
7. **The admin menu on camera.** `service-1`, `scopes-1` and `scopes-5` click Access Control, OpenID Connect and
   Applications or Scopes in the admin menu (`OpenId/ManagementAdminMenu.cs:18-27`). How the menu opens (focused,
   accordion, or kept open on the current page) depends on the admin theme settings of the site. Confirm the exact
   clicks and whether the section is already expanded when the clip starts.
8. **The server settings save.** Saving Settings > OpenID Connect > Authorization Server shows "OpenID server
   configuration successfully updated." and restarts the tenant (`OpenId/Controllers/ServerConfigurationController.cs`).
   Confirm that the site accepts the save without a certificate, which the validation code allows
   (`OpenId/Services/OpenIdServerService.cs:95-250`), and how long the restart takes.
9. **The delete confirmation.** The dialog text is built from `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`
   (title "Delete", message "Are you sure you want to remove this element?", buttons "Ok" and "Cancel"). Confirm that
   it appears for the Delete button of the Applications list, which carries `data-url-af="UnsafeUrl RemoveUrl"`
   (`OpenId/Views/OpenIdApplicationEntry.Buttons.SummaryAdmin.cshtml`).
10. **The row selectors.** The rows of the lists are rendered by the AdminList shape with `list-group-item` as the item
    class (`OpenId/Views/Application/Index.cshtml:37`). The plan names rows by their text; confirm the selector that
    reaches the Edit and Delete buttons of one row.

11. **Keeping the secret on edit.** `manage-2` says that leaving Client Secret empty on the Edit page keeps the secret.
    What was checked: the Edit action copies the posted secret into the descriptor only when it is not empty
    (`OpenId/OpenIdApplicationSettings.cs:49-52`), the descriptor is first filled from the saved application
    (`OpenId/OpenIdApplicationSettings.cs:39-42`), and the page shows the placeholder "enter a new value to replace
    the existing secret" (`OpenId/Views/Application/Edit.cshtml:62`). What the OpenIddict library does with the
    unchanged value in `UpdateAsync` is not in this source. To confirm: save Inventory Sync without typing a secret,
    then request a token with the secret generated in `service-3` (outside the video).
12. **Edit and Delete as buttons.** `service-1` says each row has Edit and Delete buttons. The row template renders
    them through the actions layout of the admin settings, whose default is Buttons
    (`OrchardCore.Admin.Abstractions/AdminListOptions.cs:36`; `OpenId/Views/OpenIdApplicationEntry.SummaryAdmin.cshtml`).
    Confirm on the recording site that the rows show the two buttons and not an Actions menu.
13. **The Redirect Uris input.** `Views/Application/Create.cshtml:145` gives the Redirect Uris text box the class
    `form-select` (the Edit page uses `form-control`). Check how it looks on the Create page in `web-3`; if it draws a
    drop-down arrow, it is the form's own styling and nothing to fix in the narration.
14. **Hints that are code-like text.** Opening a flow checkbox shows its "Recommended Parameters" hint with parameter
    names (`Create.cshtml:82,126`), and the Additional resources hint shows a tenant format with braces
    (`Views/Scope/Create.cshtml:42`). They are the form's own text and are not read aloud.
15. **Server settings before the first save.** The plan's step 2 lists every box that must be checked, because the
    defaults of `OpenId/Services/OpenIdServerService.cs:72-83` apply only while nothing has been saved. Confirm on the
    page that the three flows and the three endpoints of that table are as the plan says before selecting Save.

## Pronunciation

16. The storyboard has two `spoken` entries, written from the voice's likely misreadings and **not tested** (the
    skill's `pronounce.py` saves audio, which this step does not do): `Client Id` is spoken as `Client I.D.`, and
    `Uris` as `U.R.I.s` (for "Redirect Uris" and "Post-Logout Redirect Uris"). A person should confirm both with
    `scripts/pronounce.py`, and also listen to "OpenID", "inventory-sync", "storefront-web", "inventory-api" and
    "example.com".

## Not recorded on purpose

17. The consent page that the explicit consent type shows (`OpenId/Views/Access/Authorize.cshtml`) needs an external
    client and a sign-in. `web-5` describes the three Consent type options from the hint text of the form
    (`OpenId/Views/Application/Create.cshtml:208-212`) and does not show the page.

# Open questions: OpenID Connect Server: Flows, Endpoints and Keys

The content was written from the Orchard Core source (the commit in `catalog/SOURCE.md`) and its documentation, without
a running site. Everything below is something a person must confirm on a running site before or while recording. Each
item says what was checked in the source and what to look at.

## About the page and its defaults

1. **Defaults before the first save.** Source: `OrchardCore.OpenId/Services/OpenIdServerService.cs:61-80` returns, when
   no server settings are stored, every endpoint enabled and only the authorization code and refresh token flows
   allowed. The Blog recipe and the Auto Setup recipes do not mention OpenID (searched for `OpenId` in the recipes of
   the checkout). The migrations that run when the feature is enabled change the stored settings only if some are already
   stored (`Migrations/PushedAuthorizationRequestsMigration.cs`), so nothing is stored on a fresh site. The narration of
   `endpoints-2` states these defaults. Confirm: on a site where the feature was just enabled, the page opens with the
   seven endpoint checkboxes and **Require logout confirmation** selected (`OpenIdServerSettings.cs:48` defaults it to
   true) and exactly **Allow Authorization Code Flow** and **Allow Refresh Token Flow** selected.
2. **The restart warning, once or twice.** `Views/ServerConfiguration/Index.cshtml:10-12` writes the warning, and the
   display driver calls `AddTenantReloadWarningWrapper()` (`Drivers/OpenIdServerSettingsDisplayDriver.cs:37`), whose wrapper
   `OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml` writes the same sentence. The wrapper is added to the root
   shape, and `Index.cshtml:14` renders only `Model.Content` (the page of the Settings module renders the whole shape
   instead, `OrchardCore.Settings/Views/Admin/Index.cshtml`), so the page most likely shows the warning once.
   The narration says "a warning at the top". Confirm what is on screen; either way the narration holds, and
   `HighlightAsync` rings the first one.
3. **Flows start collapsed.** `Views/OpenIdServerSettings.Edit.cshtml` renders the flow rows with the `collapse` class
   and `Assets/ts/openid-server-settings.ts:122-197` expands them. Confirm that all six rows are visible a second after
   the load, and that after clearing **Enable Token Endpoint** only **Allow Implicit Flow** remains (`flows-2`).
4. **Scroll geometry.** Confirm that, at 1536x864, **Enable Token Endpoint** and **Allow Client Credentials Flow** fit
   in one screen (`04-flows`), and that the **Proof Key for Code Exchange** section and **Allow Client Credentials
   Flow** fit in one screen (`07-save`). If not, the plan says what to do; note the scroll values used.
5. **The validation message.** `Controllers/ServerConfigurationController.cs:80-89` adds the error under the key
   `RequirePushedAuthorizationRequests`, and the page shows `Html.ValidationSummary()`. Confirm that the message **The
   pushed authorization endpoint must be enabled when enforcing pushed authorization requests.** appears in the summary
   at the top of the form (`global-3`), and whether it also appears beside the checkbox.
6. **Saving restarts the site's configuration.** `ServerConfigurationController.cs:96-98` shows **OpenID server
   configuration successfully updated.** and releases the tenant. Confirm the redirect completes by itself, the message
   is on the page that follows, and how long it takes (the plan waits up to 60 seconds off camera).

## About the Features page and the menu

7. **Search for OpenID.** The narration of `enable-1` says the OpenID Connect features are listed together under the
   **OpenID Connect** category. Confirm which rows the search **OpenID** shows (other features may mention OpenID in
   their description, and the search covers descriptions), that **OpenID Connect Core Services** is hidden by the default
   filter (`EnabledByDependencyOnly`, `OrchardCore.OpenId/Manifest.cs:10-17`), and that the **Enable** button has the id
   `btn-enable-OrchardCore_OpenId_Server`.
8. **The enable message.** `OrchardCore.Features/Controllers/AdminController.cs:317` writes **The feature {name} was
   enabled.** for one feature. The list it names is the list that was passed in
   (`OrchardCore.Features.Core/Services/FeatureService.cs:78-80`), so only the selected feature is named, not its
   dependencies. The narration says Orchard Core also enables the features it depends on (`Manifest.cs:42-52`; the
   controller forces the dependencies, `AdminController.cs:247`).
   Confirm on the **Features** page that **OpenID Connect Management UI** is **Enabled** afterwards, and that the message
   names only **OpenID Connect Authorization Server**.
9. **Menu layout.** `ServerAdminMenu.cs` and `ManagementAdminMenu.cs` both give their **OpenID Connect** item the id
   `openid`. Confirm how the redesigned admin menu shows the two groups, **Settings** and **Access Control**, and which
   click opens each (`enable-3`, `endpoints-1`).

## About what the narration says beyond the labels

10. **Introspection, Revocation, Pushed Authorization.** These endpoints are handled by the OpenIddict library, not by
    code in the Orchard Core source. The narration states only what the labels, the hints and the validation rules say:
    **Enable Introspection Endpoint** and **Enable Revocation Endpoint** are "for checking and revoking tokens" (from
    their names), the pushed authorization endpoint "can be enabled only together with" the authorization endpoint
    (`OpenIdServerService.cs:232-238`), and pushed authorization requests can be required for every application
    (`:240-246`, hint at `Views/OpenIdServerSettings.Edit.cshtml:204`). A reviewer who knows the standards should
    confirm the wording of `endpoints-6`.
11. **Proof Key for Code Exchange.** The narration repeats the hint (`Views/OpenIdServerSettings.Edit.cshtml:184`): it
    applies to every registered application, whether or not the application enables it. "Check that your client
    libraries support it" comes from the application page (`Views/Application/Create.cshtml:182`). The narration
    does not explain how the proof key works.
12. **Self-signed certificates.** `advanced-2` says that with no certificate selected Orchard Core creates self-signed
    certificates for the site, and creates new ones when the site starts if the existing ones expire within a week
    (`OpenIdServerService.cs:284-352` and `356-426`: a new certificate is created when none is valid for more than seven
    days; `:571-624`: self-signed, valid for three months; `OrchardCore.OpenId/Tasks/OpenIdBackgroundTask.cs` removes
    expired ones). The keys are read inside `Configure(OpenIddictServerOptions)`
    (`Configuration/OpenIdServerConfiguration.cs:61-70`), and `Startup.cs` registers no change token source for those
    options, so the check runs when the tenant builds its options, not on a timer; "when the site starts" is the reading
    of that code. Confirm on a running site that the tenant folder under the site's data folder
    (`IdentityModel-Signing-Certificates`, `IdentityModel-Encryption-Certificates`) gets certificate files after the
    first request to the server; the narration gives no duration of the certificates.
13. **Applications offer only the allowed flows.** `idea-3` and `save-2` say that an application can use only the flows
    allowed on this page. Source: `Controllers/ApplicationController.cs:419-431` returns the server settings (the defaults
    when nothing is saved) to `Views/Application/Create.cshtml:7-20`, which passes them as `data-allow-...-flow`
    attributes to the application editor. The built file `wwwroot/Scripts/openid-application-editor/application-create.js`
    contains the editor: it hides the field set of each flow the server does not allow and clears its checkbox, and it
    shows every flow when the server settings are not configured. The server side agrees: only the allowed flows add
    grant types and response types to the server options (`Configuration/OpenIdServerConfiguration.cs:120-179`). Confirm
    in **Access Control, OpenID Connect, Applications** that **Create** offers only the flows that are selected on the
    server page.
14. **Examples of clients.** `idea-1` says a client can be a website or a service that calls your APIs. The source
    supports the service case (client credentials, `AccessController.cs:465-519`) and the website case (authorization
    code flow with a sign-in on this site, `:55-115`). A mobile app is a common kind of client, but nothing in the source
    shows it, so it is not named.

15. **No usable certificate.** `Views/OpenIdServerSettings.Edit.cshtml:267` and `:339` draw the certificate list only
    when `AvailableCertificates` is not empty, and `Assets/ts/openid-server-settings.ts:36` and `:73` return early when the
    list is missing. On a machine without any non-archived certificate that has a private key
    (`OpenIdServerService.cs:251-282`), the page therefore shows two yellow warnings in Advanced Options and keeps the
    **Store Name** lists visible while the location is **None**, which would contradict `advanced-2` on screen. Confirm
    on the recording machine that, with the ASP.NET Core development certificate present, the **Store Name** lists and
    the certificate lists are hidden while the location is **None**, and that **CurrentUser** shows only the
    **Store Name** list (`advanced-3`).
16. **What the certificates protect.** `advanced-2` says the server uses a signing certificate and an encryption
    certificate (`OpenIdServerConfiguration.cs:61-70`) and does not say which tokens they protect, because the default
    access token format is ASP.NET Core Data Protection (`:204-206`). `advanced-5` says JSON Web Token produces standard
    access tokens that are signed and, unless the encryption is turned off, encrypted. The code sets
    `DisableAccessTokenEncryption` from the setting, whose default is cleared (`:57`), and adds encryption credentials
    (`:61-65`); the word "standard" and the "third-party resource servers" of `advanced-6` come from the documentation
    page (`docs/reference/modules/OpenId/README.md`, Token Format), not from code. A reviewer who knows OpenIddict should
    confirm the wording of `advanced-5` and `advanced-6`.

## About the voice

17. **Pronunciation.** No `spoken` entries are used. Listen to how the voice reads "OpenID", "Orchard Core" and
    "ASP.NET Core Data Protection" (the last through the global `ASP.NET` entry), and confirm with
    `scripts/pronounce.py` before narrating. The acronyms PKCE, PAR and JWT are written out in full on purpose.

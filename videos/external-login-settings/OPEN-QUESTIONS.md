# Open questions: External Login

What was read in the source, and what a person must confirm on a running site before or while recording. The narration
states only what the source shows; the points below are about the recording and about details of the rendering that
the source does not settle. Source paths are relative to `src/OrchardCore.Modules/` of the Orchard Core checkout.

## 1. The Features page hides on-demand features by default

**Read:** `OrchardCore.Features/Views/Admin/Features.cshtml:64-65` renders the Visibility filter with the option
**Hide on demand** already `selected`, filtering on `data-is-on-demand` in `exclude` mode; External Authentication is
`EnabledByDependencyOnly` (`OrchardCore.Users/Manifest.cs:22-32`), and its buttons are disabled with the tooltips of
`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:19, 25`.
**Confirm:** on a fresh page load the row External Authentication is not listed, ticking off **Hide on demand** lists
it, and the tooltip shows on hovering the inactive button. Narration depending on it: `features-1`, `features-2`, `features-4`.
Also confirm the exact notification text after enabling OpenID Connect Client Integration (it may list the
dependencies it enabled too).

## 2. The starting state of "Disable new external registration"

**Read:** `ExternalAuthenticationMigrations` (`OrchardCore.Users/DataMigrations/ExternalAuthenticationMigrations.cs`) is
registered by the base Users startup (`OrchardCore.Users/Startup.cs:79`) and stores `DisableNewRegistrations = true`
when the Users Registration feature is not enabled at that time. The class default of the setting is `false`
(`OrchardCore.Users/Models/ExternalRegistrationSettings.cs`), and the Users documentation sample shows `false`.
The migration is registered in the base Users startup, so it runs when the Users feature is first enabled (the Blog
recipe), when Users Registration is not enabled: the source therefore gives a stored value of selected.
**Confirm:** which value a fresh Blog site actually has after the client feature is enabled. The recording plan does
not depend on the answer (it sets the option explicitly before `03-login`), and the narration says only that the
option "is selected" on screen. If the real default differs, the story of chapter 03 (the refusal) still holds
because the plan selects the option.

## 3. The stand-in provider has not been run

**Read:** the stand-in uses a public application without a client secret, because a plain-text `ClientSecret` in the
`OpenIdClientSettings` recipe step is stored as written (`OpenIdClientSettingsStep.cs:31`) but read back with
`IDataProtector.Unprotect` (`OpenIdClientConfiguration.cs:85-96`, `SecretValueExtensions.cs:53-67`), so it would not be
used. The recipe step models of the OpenID module (`OrchardCore.OpenId/Recipes/`), the client configuration
(`OrchardCore.OpenId/Configuration/OpenIdClientConfiguration.cs:69`: the HTTPS metadata requirement is off for an HTTP
authority; `OpenIdServerConfiguration.cs:226`: the transport security requirement is off), and the server keys
(`OpenIdServerService.cs:356-425`: a managed certificate is generated when none is set, with an ephemeral RSA key as the last resort).
**Confirm on a running site:** that the round trip works between two tenants of one instance on plain HTTP, with a
public client (code flow with PKCE, no secret at the token endpoint):
(a) the Default tenant reaches `http://localhost:5010/idp` with `NO_PROXY=localhost,127.0.0.1` set;
(b) the scopes `profile` and `email` are accepted for the application with the scope entries of the recipe;
(c) no consent page appears with the consent type `implicit`;
(d) the cookies of the two tenants do not collide in one browser context (they are tenant-named and path-scoped);
(e) the form post to `/signin-oidc` completes and the user info endpoint answers;
(f) the OpenID Connect correlation and nonce cookies are set with `SameSite=None` and `Secure` by default, so on plain HTTP the
callback can fail with "Correlation failed." Chromium treats `localhost` as a secure context, so it probably works, but it is the
most likely way the round trip breaks. If it does, serve the instance over HTTPS with a development certificate trusted
only for that process and change the two recipes' addresses accordingly.
If the idp refuses the token request of the public client, change the application to a confidential one and set the
client up by hand, off camera, on the Default tenant in Settings, OpenID Connect, Authentication Client
(`ClientAdminMenu.cs:26-28`), which protects the secret as the client expects. The secret is entered there, chosen
outside git, and is never written in a recipe or in a file of this repository.
If the round trip cannot be made to work, `03-login`, `05-register` and `06-straight` cannot be recorded as planned,
and the steps `login-2`, `login-3`, `new-1`, `new-2`, `new-3` and `straight-1` must be rewritten around another
stand-in. No narration step depends on any of this except through those clips.

## 4. The refusal message on the login page

**Read:** `ExternalAuthenticationsController.cs:180-186` sends a notification error "New registrations are disabled for
this site." and redirects to the login page; the login layout renders a Messages section
(`OrchardCore.Themes/TheAdmin/Views/Layout-Login.cshtml`).
**Confirm:** the message is visible on the login page of the Default tenant (admin theme) after the redirect, and
stays on screen long enough for `login-3`.

## 5. The Registration page without Users Registration

**Read:** with Users Registration disabled, the only group on the page is External Authentication
(`ExternalRegistrationSettingsDisplayDriver.cs:44`; the General group belongs to `RegistrationSettingsDisplayDriver`).
`OrchardCore.DisplayManagement/Zones/ZoneShapes.cs` renders a tab strip only when there is more than one group.
**Confirm:** how the single group looks (a card, probably with no tab strip). The narration does not mention a tab on
this page; the plan accepts both renderings.

## 6. The address of the External Logins page

**Read:** the user menu item links to the action `ExternalLogins` of `ExternalAuthenticationsController` in the area
`OrchardCore.Users` (`Views/UserMenuItems-ExternalLogins.cshtml:2`), which the default area route maps to
`/OrchardCore.Users/ExternalAuthentications/ExternalLogins` (`OrchardCore.Mvc.Core/Routing/DefaultAreaControllerRouteMapper.cs:9`).
The route named `ExternalLogins` (`Startup.cs:313-323`, path `ExternalLogins`) targets the Account controller, which has
no `ExternalLogins` action, so the path `/ExternalLogins` does not lead to the page. The Blog theme does not render the
user menu (`TheBlogTheme/Views/Layout.liquid`), so the visitor of `05-register` and `06-straight` has no menu item to
select; the plan navigates to the address.
**Confirm:** the address answers with the page **Registered Logins** for the signed-in visitor.

## 7. The registration form

**Read:** `ExternalAuthenticationsController.cs:195-196` fills the Username with a generated value (`u` followed by an
identifier, `GenerateUsernameAsync`) and the Email with the email claim when the provider sends one;
`Views/ExternalAuthentications/RegisterExternalLogin.cshtml:17-48` shows the fields.
**Confirm:** whether the stand-in provider sends the email claim for `taylor.brooks` (then the Email field is
prefilled and the plan says to leave it), and that the generated username is readable on screen in `new-2`.

## 8. Wording of two hints in the settings pages

Not a recording question, and verified in the source. The hint of **Use a script to set user roles, user claims and
user properties based on external provider claims** says that no implemented event handler is triggered when it is
selected, and the hint of **Use a script to generate userName based on external provider claims** says that handlers
defined in modules are not triggered. The controller calls every registered handler whatever the settings
(`ExternalAuthenticationsController.cs:609-618, 708-725`), so the narration says nothing about handlers and the plan
never highlights those hints.

## 9. All three registration options with no email claim

**Read:** with **Do not ask username**, **Do not ask email address** and **Do not create local password** all selected,
the controller registers the user at once with the email claim as the address (`ExternalAuthenticationsController.cs:201-224`);
the Email field is shown on the form whenever the address is empty (`RegisterExternalLogin.cshtml:25`). The narration
(`reg-5`) states only the case where the provider shares an email address.
**Confirm (not narrated):** what the visitor sees when the provider shares no email address and all three options are
selected (the registration probably fails validation and the form is shown with an error).

# Open questions: OpenID Connect Token Validation

What was checked in the Orchard Core source, and what a person must confirm on a running site before the video is
published. None of these is stated in the narration as more than the source supports.

## Responses of a protected endpoint (step `endpoint-3`)

- Checked: `ChallengeOrForbid` forbids an authenticated caller and challenges an anonymous one
  (`OrchardCore.Abstractions/Modules/Extensions/HttpContextExtensions.cs:17`). With no handler behind the API scheme,
  the challenge answers 401 (`OrchardCore.Infrastructure/Security/ApiAuthenticationHandler.cs:37-52`).
- Not in the source: the 401 and 403 that the OpenIddict validation handler itself writes. A comment in
  `OrchardCore.OpenApi/Startup.cs:119-128` says a challenge issued without an earlier authenticate is answered with 403
  when OpenIddict handles it, so the order matters.
- To confirm: with the feature enabled and the Tenant setting `Default`, call an endpoint that carries
  `[Authorize(AuthenticationSchemes = "Api")]` (for example the Demo module's `api/demo` controller) without a token
  (expect 401), with a valid token of a user that lacks the permission (expect 403), and with one that has it (expect 200).
  The narration does not state the status codes of these two cases (only the 401 of a missing handler, `flow-2`, is in the source); if the calls confirm 401 and 403, a person may add them.

## What a valid token gives the endpoint (step `flow-3`)

- Checked: the configuration registers the validation handler and points the API scheme at it
  (`Configuration/OpenIdValidationConfiguration.cs:49-84, 219-226`).
- Not in the source: that the handler builds the user from the claims of the token (OpenIddict library behavior). The
  documentation says the permission checks of an API evaluate the roles carried by the token
  (`docs/reference/modules/OpenApi/README.md`, "Missing roles"). Confirm that a call with a token issued for a user with
  a role passes a permission check granted to that role.

## A default that works before anything is saved (steps `settings-2`)

- Checked: with the Authorization Server feature enabled and nothing stored, the settings are read as
  Tenant = the current tenant (`Services/OpenIdValidationService.cs:51-66`), and the validation configuration uses
  these values (`Configuration/OpenIdValidationConfiguration.cs`, `GetValidationSettingsAsync`).
- To confirm: a token issued by the tenant is accepted by its own API before the Token Validation page is ever saved.

## Remote authority on a tenant that also runs the Authorization Server (steps `settings-3` to `settings-6`)

- Checked: in `Configure(OpenIddictValidationOptions)` there is no `return` after the block that applies an Authority
  (`Configuration/OpenIdValidationConfiguration.cs:100-140`). With Tenant empty, `CreateTenantScope` returns the current
  scope (`:231-240`), the Authorization Server service exists there, and `GetServerSettingsAsync` returns the settings
  when they are valid (`:242-265`). The default server settings (nothing stored) are valid: authorization code and
  refresh token flows with their endpoints (`Services/OpenIdServerService.cs:60-83`, rules at `:95-250`). So on the
  recording site the code that follows replaces `Configuration` with the local signing keys, sets `Issuer` to the server's
  Authority (null by default), adds the audience `oct:Default`, and, as the server has no Authority, installs an issuer
  validator that compares the token issuer with `settings.Tenant` (`:124-187`), which is empty in Authority mode.
- The narration of `settings-6` says only what this code shows: this site also runs the Authorization Server, its issuer
  and keys are applied after the authority, and a remote authority belongs on a site without it. `settings-3` says "name a
  remote server", not "trust".
- To confirm: on a site where the Authorization Server is enabled and an Authority is saved, call an endpoint with a token
  of the remote server and one of the local server. If the remote token is accepted after all, change the last two
  sentences of `settings-6`. If it is rejected, consider recording the second half of the clip on a tenant without the
  Authorization Server.
- When the local server settings are invalid, `GetServerSettingsAsync` returns null and the override is skipped
  (`:132-136`, `:250-261`).

## When the handler is registered (steps `flow-3`, `recipe-2`)

- Checked: `Configure(string name, ApiAuthorizationOptions)` always points the API scheme at the validation scheme
  (`Configuration/OpenIdValidationConfiguration.cs:219-226`); the scheme itself is added only when
  `GetValidationSettingsAsync` returns settings (`:49-84`), that is, when the settings validate, and, for a tenant, when the
  server settings of that tenant validate. With nothing stored and no Authorization Server on the tenant, the settings
  are empty, fail the "tenant or authority" rule (`Services/OpenIdValidationService.cs:86`) and no handler is registered.
- The warning is logged only while the tenant is running (`IsRunning()`, `:250`, `:275`). A recipe that runs during setup
  does not log it, and the invalid settings are then used as they are.

## The Authorize attribute with AllowAnonymous (steps `endpoint-1`, `endpoint-2`)

- Checked: the Demo controller carries `[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]`
  and each action checks the permission and returns `ChallengeOrForbid` itself
  (`OrchardCore.Demo/Controllers/ContentApiController.cs:10, 25-30`). The documentation example of a secured endpoint
  uses `[Authorize(AuthenticationSchemes = "Api")]` without `AllowAnonymous` (`docs/reference/modules/OpenApi/README.md`,
  "Adding New API Endpoints").
- Not in the Orchard Core source: that ASP.NET Core still authenticates the named scheme for an endpoint that is also
  `AllowAnonymous`. To confirm: call `api/demo` with a valid token and a user whose role holds the Demo API access
  permission, and check that the action sees an authenticated user.

## The page as it renders (steps `settings-1`, `settings-4`, `settings-5`)

- The restart warning is written in the page (`Views/ValidationConfiguration/Index.cshtml:11`) and added again by the
  editor wrapper (`OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml`). Confirm whether it shows twice.
- Confirm that the error "An audience must be set when configuring the authority." is shown in the validation summary
  at the top (`Html.ValidationSummary()`, `Index.cshtml:8`), and not only next to the field.
- Confirm that the success message stays on screen long enough for the narration of `settings-5`.
- Confirm that the admin menu entries Settings, OpenID Connect and Token Validation can be clicked in that order; the
  plan has a fallback to the page address.

## The features list (steps `enable-1`, `enable-2`)

- Confirm that a search for "token validation" shows two rows, OpenID Connect Authorization Server and OpenID Connect
  Token Validation, because the description of the first one contains the phrase (`Manifest.cs:45`).
- Confirm that the Enable link (a POST through the anti-forgery script) works in the recorder, and what the success
  message says. The controller writes one message that lists the features it enabled
  (`OrchardCore.Features/Controllers/AdminController.cs:313-323`).

## Another tenant as the issuer (step `issuer-4`)

- Checked in `Services/OpenIdValidationService.cs:177-221`: a tenant other than the current one must exist, must have
  the Authorization Server feature, must not use reference access tokens, and must have a scope whose resources include
  the prefix `oct:` followed by the name of the current tenant. No second tenant is recorded, so this has not been run.
- To confirm: create a second tenant `Reporting` with the Authorization Server, add a scope with the resource
  `oct:Reporting` on the server tenant, and save the Token Validation settings of `Reporting` with the server tenant's
  name. The slide uses `Reporting` as an invented tenant name.

## Not narrated, noted for the documentation

- The hint under Audience says "If not specified, the tenant name will be used as the default audience"
  (`Views/OpenIdValidationSettings.Edit.cshtml:45`). The validation requires an audience when an Authority is set
  (`Services/OpenIdValidationService.cs:152`) and refuses one when a tenant is set (`:142`).
- `Deployment/OpenIdValidationDeploymentSource.cs` writes the settings nested under the key `OpenIdValidationSettings`,
  while the recipe step reads the properties at the root of the step (`Recipes/OpenIdValidationSettingsStep.cs`). A
  deployment plan exported and imported again may therefore store empty settings. The video does not show deployment.
- The documentation says the `roles` scope carries the roles into the token (`docs/reference/modules/Media/MediaGallery.md`,
  `docs/reference/modules/OpenApi/README.md`). In `Controllers/AccessController.cs:644-672` the role claim goes to the
  access token whether or not the `roles` scope is granted, and to the identity token only with the scope.
- Saving a Tenant equal to the current tenant is accepted by the validation even when the Authorization Server feature
  is not enabled (`Services/OpenIdValidationService.cs:177`), and then no handler is registered
  (`Configuration/OpenIdValidationConfiguration.cs:67-84`).

## Pronunciation

- `storyboard.json` maps `ChallengeOrForbid` to "Challenge Or Forbid" and `AllowAnonymous` to "Allow Anonymous" in `spoken`. Confirm the sample with
  `scripts/pronounce.py` before narrating. Check also "OpenID", "401", "403" and "API".

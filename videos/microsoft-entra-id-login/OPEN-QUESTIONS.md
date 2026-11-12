# Open questions: Microsoft Entra ID: Sign In With Work Accounts

What was checked in the Orchard Core source (commit 92db4a83) and the documentation, and what a person must confirm on a
running site before the video is recorded or published. Paths are relative to `src/OrchardCore.Modules/` unless they say
otherwise.

## Checked in the source

- The menu path Settings, Security, Authentication, Microsoft Entra ID, and its permission
  (`OrchardCore.Microsoft.Authentication/AdminMenuAAD.cs:25-28`, `Permissions.cs`).
- The labels of the settings page: Display Name, AppId, TenantId, CallbackPath (placeholder `/signin-oidc`), Store tokens, and
  the notice about the restart (`Views/MicrosoftEntraIDSettings.Edit.cshtml:7-60`). Display Name, AppId and TenantId are required
  (`ViewModels/AzureADSettingsViewModel.cs:7-13`); CallbackPath, when filled in, must start with a slash (`:16`).
- The sign-in button exists only when AppId and TenantId are not blank and the settings are valid
  (`Configuration/AzureADOptionsConfiguration.cs:37`, `Configuration/AzureADSettingsConfiguration.cs:37`,
  `Services/AzureADService.cs:47-65`). Its text is the Display Name (`OrchardCore.Users/Views/Account/Login.cshtml:43`).
- The sign-in goes through the Microsoft identity platform with the OpenID Connect handler, and the authority is built from
  the TenantId (`Configuration/OpenIdConnectOptionsConfiguration.cs:35`). Orchard Core never sets a client secret for this scheme, and the settings page has no field for one.
- Matching after the return: a user linked to the login signs in; otherwise a user with the same email address gets the
  "Link your account." page; otherwise registration, unless "Disable new external registration" is selected
  (`OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:113-186`; labels in
  `Views/ExternalAuthentications/LinkExternalLogin.cshtml` and `Views/ExternalRegistrationSettings.Edit.cshtml:23`).
- The feature depends on External Authentication, which can only be enabled by a dependency
  (`OrchardCore.Users/Manifest.cs:22-32`, `EnabledByDependencyOnly`), and the External Logins item of the user menu appears when an
  external scheme exists (`OrchardCore.Users/Drivers/ExternalAuthenticationUserMenuDisplayDriver.cs:20`).
- Enabling the feature enables External Authentication with it (`OrchardCore/Shell/ShellDescriptorFeaturesManager.cs`,
  `GetFeaturesToEnable`, with `force` true from `Features/Controllers/AdminController.cs:247`). The message that follows names
  only the feature that was selected, because `FeatureService.EnableOrDisableFeaturesAsync`
  (`OrchardCore.Features.Core/Services/FeatureService.cs:79-80`) passes the requested list, not the enabled dependencies, to
  the notification. The narration (`enable-2`) therefore says that a message confirms the feature, not that it lists both.
- The match by email address happens only when the sign-in carries an email claim (`ExternalLoginInfoExtensions.GetEmail` reads
  `ClaimTypes.Email` or `email`) and `RequireUniqueEmail` is true, which is its default
  (`OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:148-153`, `Services/IdentityOptionsConfigurations.cs:31`).
  The settings screens have no field for it. The narration (`match-2`) says "the email address that Microsoft reports".
- A user who is linked, or found by email, can still be refused: the login form events refuse disabled users, users who have not
  confirmed their email address, and users who await approval (`Handlers/DisabledUserLoginFormEvent.cs`,
  `EmailConfirmationLoginFormEvent.cs`, `UserModerationLoginFormEvent.cs`). The narration (`match-1`) says "as long as that user
  is allowed to sign in".

## To confirm on a running site

1. **The round trip with a real directory.** Nothing here was run. The narration says that selecting the button sends the
   browser to Microsoft, which follows from `ExternalLogin` returning a challenge
   (`ExternalAuthenticationsController.cs:83`). A person with a test directory should confirm that the browser lands on the
   Microsoft sign-in page and returns to the callback path, that the "Link your account." page appears for a user with the
   same email address, and what appears when the email claim is missing.
2. **What the button does in the recording setup.** With outbound calls blocked and made-up identifiers, selecting the button
   is expected to fail while the site loads the metadata of the authority. The recording plan avoids the click. Confirm
   that, with those conditions, saving the settings and opening `/Login` produce no error page and no error in the log.
3. **Labels of the Azure portal.** New registration, Supported account types, Redirect URI, Authentication, "Implicit grant and
   hybrid flows", "Access tokens (used for implicit flows)", "ID tokens (used for implicit and hybrid flows)", Token
   configuration, Add optional claim, ID, email, App registrations, and the names "Application (client) ID" and "Directory
   (tenant) ID" come from the two documentation pages, not from code. A person must compare them with the portal.
4. **Which tokens must be enabled.** The reference page says to enable both Access tokens and ID tokens, and that login fails
   with errors without them; the guide says "at least ID tokens". The code does not set a response type
   (`Configuration/OpenIdConnectOptionsConfiguration.cs:24-42`), so the flow comes from the defaults of ASP.NET Core. The
   narration (`register-3`) only says that the reference documentation asks for both, and leaves out the claim about errors.
   Confirm the minimum on a test directory before saying more.
5. **The default of "Disable new external registration" on a fresh site.** `ExternalAuthenticationMigrations` is registered by
   the main Users feature (`OrchardCore.Users/Startup.cs:79`, in the class without a feature attribute), so it runs when Users is
   installed at setup, not when External Authentication is enabled. Its deferred task
   (`DataMigrations/ExternalAuthenticationMigrations.cs:27-32`) stores `DisableNewRegistrations = enumValue == 0 || !isRegistrationFeatureEnabled`.
   The Blog recipe does not enable Users Registration, so the setting is expected to be selected on the recorded site: a person
   with no linked user and no matching email address would see "New registrations are disabled for this site." until it is
   cleared under Settings, Security, Registration. That depends on the order in which setup stores the site settings, which
   only a running site shows. Open Settings, Security, Registration on the fresh site and look at the box. If it is
   selected, consider adding a sentence to `match-3` and the outro. The narration only says "if those settings disable new
   registrations".
6. **The message after Enable (`enable-2`).** From the code (see above) the message is expected to read "The feature Microsoft Entra
   ID (Azure Active Directory) Authentication was enabled." and not to name External Authentication
   (`Features/Controllers/AdminController.cs:317`). Confirm the text on a running site, that it is still on screen when the
   narration ends, and that the row of External Authentication stays hidden by the "Hide on demand" filter.
7. **The restart.** The settings page warns that the website might be restarted. Confirm how long Save and Enable take, and
   whether the green message "Site settings updated successfully." (`OrchardCore.Settings/Controllers/AdminController.cs:119`)
   is on screen when `settings-6` ends.
8. **Menu selectors.** `#adminMenu button.item-label` for the groups and `#adminMenu a.microsoft-entra-id` for the link are read
   from `Themes/TheAdmin/Views/NavigationItem-admin.cshtml` and `NavigationItemLink-admin.cshtml`, not tried. The ids
   `AzureADSettings_*` of the inputs are expected from the shape prefix. Check them with `Recorder probe`.
9. **Roles from groups (`roles-1`, `roles-2`).** The video only points to the guide. The guide asks to add the groups token
   in the portal, and its script reads a claim of type `.../claims/role` with the object ids of the groups; whether a groups
   token reaches the script under that claim type was not checked. The script itself is not shown in the video.
10. **Redirect URI of a tenant with a URL prefix.** The reference page gives the callback path under the root URL of the app.
    For a tenant served under a prefix, the callback path is relative to the base path of the request; this was not tried.
11. **Spoken spellings** (`spoken` in `storyboard.json`): "AppId" as "App I.D.", "TenantId" as "Tenant I.D.", and "ID" as
    "I.D.", which also changes "IDs". Check each, and the words "Entra" and "organizations", with
    `scripts/pronounce.py` before narrating.

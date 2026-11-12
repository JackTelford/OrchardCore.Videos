# Open questions: Secure Your Site

Everything the narration says was read from the Orchard Core source or its documentation (the labels, the menu paths, the
defaults). Nothing was run: no site was available to the author. A person must confirm the points below on the site of
`recording-plan.md` before recording, and fix the plan or the narration if a point does not hold.

## Selectors

- Every selector in `recording-plan.md` was written from the Razor and TypeScript sources: the admin menu
  (`#left-nav li.current`, `span.title`), the settings tabs (`a[role="tab"]`), the lists (`li.list-group-item`), the Actions dropdown
  (`button.dropdown-toggle.actions`), the toast (`.toast.message-warning`) and the bootstrap-select buttons of the Audit Trail
  filters (`.filter button[title="Date"]`). Run `Recorder probe` on each page and adjust any that matches nothing or too much.
- The menu entry of the open page is expected to be marked `current` and its parents opened on load. The sidebar script says so;
  confirm that this holds for the settings pages (they are matched by the route values `area` and `groupId`).

## Facts to confirm on a running site

- **HTTPS over plain HTTP** (`https-2`): the code disables the four fields and adds the warning "For safety, changing HTTPS-only settings
  over HTTP has been prevented." when the request is not HTTPS (`HttpsSettingsDisplayDriver.cs:58`, `HttpsSettings.Edit.cshtml:6,16,30,40`).
  Checked in the code: the warning has no dismissal delay (`NotifierExtensions.cs`, `Message.cshtml`), so the toast does not close by itself.
  A person must confirm on the running site that it is on screen when the step starts. If the maintainer would rather show the options
  enabled, the page must be opened over HTTPS: that needs a trusted development certificate on the recording machine, because the
  recorder does not ignore certificate errors, and `https-2` must then be rewritten.
- **Anonymous and the role list** (`roles-1`, `roles-2`): the code creates Administrator, Anonymous and Authenticated as system
  roles and gives Anonymous the View all content permission (`SystemRolesMigrations.cs`, `OrchardCore.Contents/Permissions.cs:93`).
  Checked in the code: the Anonymous permissions are also granted to signed-in users (`RolesPermissionsHandler.cs`). Confirm that the
  three show in the list with the System badge, and that Allow and Effective are both ticked for View all content on the Anonymous
  editor of a fresh Blog site.
- **Tabs** (`login-2`, `headers-1`, `audit-2`): the tab names (General, Two-Factor Authentication; Content Security Policy,
  Permissions Policy, Referrer Policy; Events, Trimming) come from the `Location` strings of the display drivers. Confirm how they
  render, and that the Two-Factor Authentication tab shows once Two-Factor Authenticator App Method is enabled and Roles is on.
- **Two-factor for chosen roles** (`login-2`): the code redirects a signed-in user of a chosen role who has not enabled two-factor
  authentication to the two-factor page, on the request after the sign-in that computed the claim
  (`TwoFactorAuthenticationClaimsProvider.cs:29`, `TwoFactorAuthenticationAuthorizationFilter.cs:71`). Do not test this on the
  recording site: save the setting on a throwaway copy only.
- **reCaptcha** (`recaptcha-1`): the code validates and shows the challenge on the sign-in, registration and password recovery
  pages whenever the Site Key, Secret Key and API address are set and the ReCaptcha Users feature is enabled, with no attempt
  threshold (`ReCaptchaLoginFormDisplayDriver.cs:21`, `LoginFormEventEventHandler.cs:30`). The ReCaptcha documentation describes a
  threshold per IP address instead. Confirm on a throwaway copy with made-up keys that the challenge is shown at once.
- **Reverse proxy** (`proxy-1`, `proxy-2`): the Reverse Proxy page has only the three header boxes. It has no list of known proxies or
  networks (they exist only in configuration and in recipes), so the video does not ask the viewer to trust a proxy. The narration says to
  turn on only the headers the site's own proxy sends. Checked in the code: a rate-limit budget is counted per signed-in user, otherwise per
  `Connection.RemoteIpAddress` (`RateLimitPartitionHelpers.GetClientPartitionKey`), and the forwarded headers middleware of the Reverse Proxy
  module runs before the rate limiter (order -300 against 0). What the site trusts when no known proxy is set was not verified and is left out.
- **Rate Limits** (`rate-1`): "per minute" comes from the description of the seeded Default Global Policy
  (`default-global-policy.recipe.json:9`, a fixed window of 60 seconds). The seed recipe reads the recipe parameters `AdminUserId` and
  `AdminUsername`; with the Users module enabled, `RecipeEnvironmentSuperUserProvider` supplies them from the site's super user, so the
  policy is expected to be seeded when the feature is enabled through JSON Import. Confirm that the list row shows that description and
  the Enabled badge. If the list is empty, add the policy by hand with the same name and description, a Global policy and a Fixed window
  limiter, and enable it.
- **Audit Trail** (`audit-1` to `audit-3`): "Login failed" is not enabled by default
  (`UserAuditTrailEventConfiguration.cs:27`) and the retention default is ten days (`AuditTrailTrimmingSettings.cs:5`). "Administrators
  cannot edit the log" comes from the Audit Trail documentation; confirm that the list offers no edit or delete action.
- **Secrets** (`secrets-1`): "encrypted" is checked for the default database store (`DatabaseSecretStore.cs:92` protects the value with
  Data Protection; the Azure Key Vault store was not read), "never displayed again" comes from the hint of the secret editor
  (`Edit.cshtml:74`). Confirm the two seeded secrets show a Text Secret badge and a store badge.
- **Features page** (`features-1`, `features-2`): the Security group, the row names HTTPS, CORS Configuration, ReCaptcha and Rate
  Limits, and the Enable and Disable buttons come from the manifests and the Features views. Confirm the group heading and the names
  on the page.
- **Users created in the admin** (`users-1`, `users-2`): `AdminController.CreatePost` creates a plain `User`, so the email address is not
  confirmed and the row shows an Unconfirmed badge until Confirm email is chosen in its Actions menu (`UserActionsMenu.cshtml:35`). The
  recording plan confirms the three seeded users. Confirm on the site that the badge is gone and that the Actions menu then lists Change
  password, Disable and Delete.

## Pronunciation

`spoken` in `storyboard.json` holds only HSTS ("H.S.T.S."), so a voice spells it out; no spelling was confirmed by ear. Listen to the narrated audio (or use
`scripts/pronounce.py`) for these words and add a `spoken` entry only if one is wrong: HTTPS, HSTS, CORS, reCaptcha, ReCaptcha,
X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host, Anonymous, Authenticated.

## Left out on purpose

- Account lockout and the password rules are not admin settings (they are ASP.NET Core Identity options set in configuration), so the
  video does not cover them. A person may decide to add a sentence saying so.
- Secret Stores, Migrate Secrets with a credential to move, and the Users Audit Trail page are named but not shown. The Sanitize Html
  check box (Html Body part, Markdown Body part, Html and Markdown fields; on by default) is left out because a separate video covers it.

# Recording plan: Secure Your Site

This plan lets one person record the video without deciding anything. The video is an admin video: it shows the admin
screens of a fresh Blog site and never saves a setting. Every step is a short walk over a page that already holds
what the narration describes.

In this plan, a selector in code font stands for `scene.Page.Locator(...)`. Source paths are relative to
`src/OrchardCore.Modules/` of the Orchard Core checkout, except those that start with `OrchardCore/`,
`OrchardCore.Themes/` or `docs/`, which are relative to `src/`. The selectors were read from the Razor and TypeScript sources and not run against a site: look at every
contact sheet, and see `OPEN-QUESTIONS.md` for what to check first.

Three helpers are used by name in the clips below.

| Helper | Meaning |
|---|---|
| `current` | `#left-nav li.current > figure > figcaption a.item-label`: the admin menu entry of the page that is open. The menu opens the section that holds the current page, so it is visible without a click (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts`, `applyNavStateFromPreferences`). |
| `menu(Name)` | `#left-nav a.item-label:has(span.title:text-is("Name"))`: another entry of the admin menu, by its label. |
| Highlight | `scene.HighlightAsync(locator, caption)`. Call `scene.ClearHighlightsAsync()` before the next highlight, before a scroll and before a navigation. |

## The site it needs

- **Site**: `src/OrchardCore.Cms.Web` of the Orchard Core checkout, run on `http://localhost:5010` over plain HTTP, with
  its own empty `App_Data` outside both repositories (`ORCHARD_APP_DATA`), and `HTTP_PROXY` and `HTTPS_PROXY` set to a
  dead address.
- **Auto Setup** (the site is not recorded during setup): one tenant, `ShellName` `Default`, `SiteName` `Orchard Core`,
  `SiteTimeZone` `America/Los_Angeles`, `AdminUsername` `admin`, `AdminEmail` `admin@example.com`, `AdminPassword` the
  value of `VIDEO_PASSWORD` (written `<password>` in this plan, chosen outside git), `DatabaseProvider` `Sqlite`,
  `RecipeName` `Blog`. Set them as environment variables `OrchardCore__AutoSetup__Tenants__0__<Name>`.
- **Features the Blog recipe already enables and the clips use**: Users, Roles, Security, Features, Deployment (for JSON
  Import), Contents.
- **Features to enable before recording** (outside the capture): open Tools, Deployments, JSON Import
  (`OrchardCore.Deployment/AdminMenu.cs:30`; the Blog recipe enables the Deployment feature), paste this into the JSON
  box and select Import (`OrchardCore.Deployment/Views/Import/Json.cshtml`). The tenant reloads; wait until `/Admin`
  loads again.

  ```json
  {
    "steps": [
      {
        "name": "feature",
        "enable": [
          "OrchardCore.Https",
          "OrchardCore.Cors",
          "OrchardCore.ReCaptcha",
          "OrchardCore.ReCaptcha.Users",
          "OrchardCore.RateLimits",
          "OrchardCore.ReverseProxy",
          "OrchardCore.Secrets",
          "OrchardCore.AuditTrail",
          "OrchardCore.Users.AuditTrail",
          "OrchardCore.Users.2FA.AuthenticatorApp",
          "OrchardCore.Email",
          "OrchardCore.Users.Registration"
        ]
      }
    ]
  }
  ```

  Leave every other feature as the recipe left it: the `02-features` clip shows a mix of enabled and disabled
  features in the Security group (for example Users Reset Password, Users Change Email and the other two-factor
  methods stay disabled).
  Why each one: HTTPS, CORS Configuration, ReCaptcha, ReCaptcha Users, Rate Limits, Reverse Proxy, Secrets, Audit Trail
  and Users Audit Trail give the pages of the clips; Two-Factor Authenticator App Method adds the Two-Factor
  Authentication tab to User Login (`OrchardCore.Users/TwoFactorAuthenticationStartup.cs:24`, `:158`); Users
  Registration adds the Registration page and needs Email (`OrchardCore.Users/Manifest.cs:46-55`). Rate Limits seeds
  the Default Global Policy the first time it is enabled
  (`OrchardCore.RateLimits/Migrations/GlobalRateLimitsMigrations.cs`).
- **Settings**: none are saved, and none must be. In particular: never save the reCaptcha keys (with keys saved, the
  sign-in page asks for the challenge, and the recorder can no longer sign in), never tick Disable local password login,
  and never save a two-factor requirement (the next sign-in would be sent to the two-factor page).
- **External services**: none. Outbound calls stay blocked. Nothing in the clips needs Google, a mail server or a proxy.
- **Seed data** (after the features are enabled, so that the Audit Trail records it; outside the capture):
  - Users, through Access Control, Users, Add User (fields User name, Email, Password, Password confirmation, and the
    Roles check boxes), then Save. Password `<password>` for each.

    | User name | Email | Roles |
    |---|---|---|
    | `alex.rivera` | `alex.rivera@example.com` | Editor |
    | `sam.okafor` | `sam.okafor@example.com` | Author |
    | `priya.nair` | `priya.nair@example.com` | Contributor |

    A user created in the admin has an unconfirmed email address (`OrchardCore.Users/Controllers/AdminController.cs:297-313`
    creates a plain `User`; only registration, setup and the user command set `EmailConfirmed`), so each new row shows a
    yellow Unconfirmed badge and its Actions menu lists Confirm email (`OrchardCore.Users/Views/UserFields.cshtml:11`,
    `OrchardCore.Users/Views/UserActionsMenu.cshtml:35`). Clear it for each of the three users: in the row open
    Actions, choose Confirm email, and select Ok in the dialog. The badge is gone and the menu lists Change password,
    Disable and Delete.

    Then disable `priya.nair`: in her row open Actions, choose Disable, and select Ok in the dialog. Her row shows a red
    Disabled badge.
  - Secrets, through Tools, Security, Secrets, Add Secret, the Text Secret card, Create. Fields Name, Value,
    Description. Save each.

    | Name | Value | Description |
    |---|---|---|
    | `Payments.ApiKey` | any made-up text (`<secret value>`, it is never displayed) | API key for the payment service of the example shop. |
    | `Smtp.Password` | any made-up text (`<secret value>`) | Password of the mail account that sends the example newsletter. |
  - Sign the recorder in (`Recorder setup`) after the users exist: the Audit Trail then holds the user events of the
    seed data and the admin's Logged in event, so `07-audit` does not show an empty list.
- **Snapshot**: stop the site after the seed, copy `App_Data` and `state.json` aside. Restore them before every full take.
  No clip saves anything, so a single clip can be recorded again without a restore.

## Commands

From the root of this repository, in Git Bash (`<repo>` is the path of the checkout, `<bin>` is `%TEMP%/ocvideo/bin/Release/net10.0`):

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/secure-your-site/recorder
export MSYS_NO_PATHCONV=1 VIDEO_ROOT=<repo>/videos/secure-your-site VIDEO_BASE_URL=http://localhost:5010
export VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
<bin>/Recorder setup       # after the features and the seed data exist: it signs in and saves state.json
cd <repo>
python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/secure-your-site
<bin>/Recorder record all  # or the names of some clips
```

`videos/secure-your-site/recorder/Scenes.cs` (one method per clip, one `scene.StepAsync` per step id below) is written by
the person who records; this plan is its specification.

## Order of the clips

All clips start from the snapshot above and none changes the site, so none depends on another. Record them in
storyboard order: `02-features`, `03-roles`, `03-users`, `04-login`, `04-registration`, `05-https`, `05-headers`,
`05-proxy`, `06-recaptcha`, `06-rate-limits`, `06-cors`, `07-audit`, `07-secrets`. Which clips change the site: none.
The chapter `01` and `08` slides and the two cards need no recording.

Two clips leave something unsaved on screen on purpose and must end there without navigating: `04-login` (the
two-factor role box is ticked) and `06-cors` (the policy editor is open). The recorder ends the clip by closing the
page, which discards both.

## Slides

- `overview-1`: the slide Four questions to ask of your site, four cards, all bright (Who can do what, How do people
  sign in, How are connections and requests protected, What is kept safe and recorded), each with the menu it points to.
- `recap-1`: the slide The checklist, four cards; the first two (Who can do what, How people sign in) are bright.
- `recap-2`: the same slide; the last two (How connections and requests are protected, What is kept safe and recorded)
  are bright.

## `02-features` (Tools, Features)

Start page: `/Admin/Features`. Before the capture, scroll the Security group to the top:
`div.feature-group:has(> h3:text-is("Security"))` then `EvaluateAsync("e => e.scrollIntoView({ block: 'start' })")`,
then `scene.Page.EvaluateAsync("window.scrollBy(0, -150)")` so the sticky bar does not cover the heading. Start the capture.

Source: `OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features), `OrchardCore.Features/Views/Admin/Features.cshtml:88`
(group heading is the category), `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml` (row name),
`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml` (Enable and Disable buttons).

`features-1`
- Highlight `current` with the caption `Tools, Features` (1.5 s). Clear.
- Highlight `div.feature-group:has(> h3:text-is("Security")) > h3` (1.5 s). Clear.
- In turn (1.2 s each), highlight the rows `li.list-group-item:has(span.fw-semibold:text-is("HTTPS"))`,
  `li.list-group-item:has(span.fw-semibold:text-is("CORS Configuration"))`,
  `li.list-group-item:has(span.fw-semibold:text-is("ReCaptcha"))` and
  `li.list-group-item:has(span.fw-semibold:text-is("Rate Limits"))`, each time clearing before the next.
- On screen at the end: the Security heading and its rows, with the ring on the Rate Limits row, whose Disable button
  is visible.

`features-2`
- Clear. Move the cursor to the Disable or Enable button of the row `Users Reset Password`
  (`li.list-group-item:has(span.fw-semibold:text-is("Users Reset Password")) a.btn`) without clicking (`MoveToAsync`),
  and highlight that row (2 s).
- On screen at the end: the row of a disabled feature, with its Enable button, next to enabled ones. Do not click.

## `03-roles` (Access Control, Roles)

Start page: `/Admin/Roles`.

Source: `OrchardCore.Roles/AdminMenu.cs:18-19` (Access Control, Roles), `OrchardCore.Roles/Views/Admin/Index.cshtml:15`,
`OrchardCore.Roles/Views/RoleEntry.DefaultTags.SummaryAdmin.cshtml:6-9` (System badge),
`OrchardCore/OrchardCore.Roles.Core/DefaultSystemRoleProvider.cs:32,39` (what Anonymous and Authenticated stand for).

`roles-1`
- Highlight `current` with the caption `Access Control, Roles` (1.5 s). Clear.
- Highlight `form.roles a:text-is("Anonymous")` (2 s). Clear. Highlight `form.roles a:text-is("Authenticated")` (2 s).
- On screen at the end: the roles list with Administrator, Anonymous, Authenticated, Author, Contributor, Editor and
  Moderator, the System badge on the first three, and the ring on Authenticated.

`roles-2`
- Clear. `ClickAndWaitForNavigationAsync(form.roles a:text-is("Anonymous"))`. Wait for `#search-box` to be visible.
- `TypeAsync(#search-box, "View all content")`. Wait until `tr[data-text="View all content"]` is the only visible row of
  `.permissions-list > table > tbody > tr`.
- Highlight `tr[data-text="View all content"] input[name^="Checkbox."]` (2 s), then the column header
  `fieldset.permissions-list th:text-is("Effective")` (the first match) (2 s).
- On screen at the end: the Anonymous editor with one permission row, View all content, whose Allow and Effective
  boxes are both checked, under the Allow and Effective headings.
- Source: `OrchardCore.Roles/Views/Admin/Edit.cshtml:6`, `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:40,43,49`
  (Allow, Effective, Search permissions), `OrchardCore.Roles/Assets/ts/roles-edit.ts` (the search matches
  `data-text`), `OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:29` (View all content),
  `OrchardCore.Roles/Controllers/AdminController.cs:516` (Anonymous permissions are effective for signed-in users).

`roles-3`
- Clear. `NavigateAsync("/Admin/Roles/Edit/Editor")`. Wait for `#search-box`.
- `TypeAsync(#search-box, "Manage Roles")`. Wait until `tr[data-text="Manage Roles"] .badge:text-is("Security Critical")`
  is visible and no other row is.
- `MoveToAsync(tr[data-text="Manage Roles"] .badge)` (1 s, shows the tooltip), then highlight that badge (2 s).
- On screen at the end: the Edit 'Editor' Role page with one row, Manage Roles, with the red Security Critical badge.
- Source: `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:125` (the badge and its tooltip text),
  `OrchardCore/OrchardCore.Roles.Core/RolesPermissions.cs:8` (Manage Roles, security critical).

## `03-users` (Access Control, Users)

Start page: `/Admin/Users`. Wait for the four rows: `admin` and the three seeded users.

Source: `OrchardCore.Users/AdminMenu.cs:27-30` (Access Control, Users), `OrchardCore.Users/Views/Admin/Index.cshtml:4`,
`OrchardCore.Users/Views/UserRolesMeta.cshtml:10` (the role badges of a row), `OrchardCore.Users/Views/UserFields.cshtml:22-23`
(Disabled badge).

`users-1`
- Highlight `current` with the caption `Access Control, Users` (1.5 s). Clear.
- In turn (2 s each, clearing between), highlight `li.list-group-item:has-text("alex.rivera")`,
  `li.list-group-item:has-text("sam.okafor")` and `li.list-group-item:has-text("priya.nair")`.
- On screen at the end: the users list, every row with its role badges (Administrator, Editor, Author, Contributor),
  no Unconfirmed badge, and the ring on the priya.nair row, which carries the red Disabled badge.

`users-2`
- Clear. `ClickAsync(li.list-group-item:has-text("sam.okafor") button.dropdown-toggle.actions)`. The menu lists
  Change password, Disable and Delete (Confirm email is gone once the seed confirmed the address). Highlight `li.list-group-item:has-text("sam.okafor") .actions-menu a.dropdown-item:text-is("Disable")`
  (2.5 s). Do not click it. Clear. `scene.Page.Keyboard.PressAsync("Escape")` closes the menu.
- `ClickAsync(#filter-dropdown)`, then `ClickAndWaitForNavigationAsync(a.dropdown-item:text-is("Only disabled users"))`.
- On screen at the end: the list filtered to `priya.nair` alone, with the Disabled badge.
- Source: `OrchardCore.Users/Views/UserActionsMenu.cshtml:16,20,24,29,35,40` (Change password, Disable, Enable, Unlock, Confirm email, Delete),
  `OrchardCore.Admin/Views/AdminListActions.cshtml:30-32` (the Actions dropdown),
  `OrchardCore.Users/Views/UsersAdminListSearch.cshtml:18,22-23` (the filter menu),
  `OrchardCore.Users/Handlers/DisabledUserLoginFormEvent.cs:62` (a disabled user cannot sign in).

## `04-login` (Settings, Security, User Login)

Start page: `/Admin/Settings/userLogin`. The General tab is open.

Source: `OrchardCore.Users/AdminMenu.cs:41` (Settings, Security, User Login), `OrchardCore.Users/Drivers/LoginSettingsDisplayDriver.cs:13,40`
(group `userLogin`, tab General), `OrchardCore.Users/Views/LoginSettings.Edit.cshtml:73-74`.

`login-1`
- Highlight `current` with the caption `Settings, Security, User Login` (1.5 s). Clear.
- Highlight `label.form-check-label:has-text("Disable local password login")` (4 s). Do not click it.
- On screen at the end: the General tab with the ring on Disable local password login and its hint, which ends with
  "Make sure there is at least one admin with linked account". The label is written with a full stop,
  "Disable local password login." (`LoginSettings.Edit.cshtml:73`); the narration says it without the full stop.

`login-2`
- Clear. `ClickAsync(a[role="tab"]:text-is("Two-Factor Authentication"))`. Wait for
  `label.form-check-label:has-text("Require two-factor authentication specific roles")`.
- `ClickAsync(that label)` ticks the box and opens the role list. Wait for `#rolesContainer.show`.
- Highlight `#rolesContainer label.form-check-label:text-is("Administrator")` (3 s). Do not tick any role. Do not press
  Save. Do not navigate; the clip ends here.
- On screen at the end: the Two-Factor Authentication tab, the specific-roles box ticked, and the role list (Administrator,
  Author, Contributor, Editor, Moderator) with the ring on Administrator, none ticked.
- Source: `OrchardCore.Users/Drivers/RoleLoginSettingsDisplayDriver.cs:53` and `TwoFactorLoginSettingsDisplayDriver.cs:41`
  (the tab name), `OrchardCore.Users/Views/LoginSettingsRoles.Edit.cshtml:8,15`,
  `OrchardCore.Users/Services/TwoFactorAuthenticationClaimsProvider.cs:29` and
  `OrchardCore.Users/Filters/TwoFactorAuthenticationAuthorizationFilter.cs:71` (a user in a chosen role without two-factor
  authentication is redirected to the two-factor page), `OrchardCore/OrchardCore.Roles.Abstractions/RoleServiceExtensions.cs:16-33`
  (the role list holds Administrator but not Anonymous or Authenticated).

## `04-registration` (Settings, Security, Registration)

Start page: `/Admin/Settings/userRegistration`.

Source: `OrchardCore.Users/RegistrationAdminMenu.cs:28` (Settings, Security, Registration),
`OrchardCore.Users/Drivers/RegistrationSettingsDisplayDriver.cs:14`, `OrchardCore.Users/Views/RegistrationSettings.Edit.cshtml:23,33`,
`OrchardCore.Users/Manifest.cs:46-55` (the feature name Users Registration and its description),
`OrchardCore.Users/Handlers/EmailConfirmationLoginFormEvent.cs:31` (with Users must verify their email address on, a user whose email address is not confirmed cannot sign in; the page warns about it only after the box is ticked, `RegistrationSettings.Edit.cshtml:12,21`). The box stays unticked.

`registration-1`
- Highlight `current` with the caption `Settings, Security, Registration` (1.5 s). Clear.
- Highlight `label.form-check-label:text-is("Users must verify their email address")` (3.5 s). Clear.
- Highlight `label.form-check-label:text-is("Users must be approved before they can log in")` (3.5 s). Do not click
  either box.
- On screen at the end: the Registration page with the ring on Users must be approved before they can log in; the first
  box is visible above it, unticked.

## `05-https` (Settings, Security, HTTPS)

Start page: `http://localhost:5010/Admin/Settings/Https`, over plain HTTP on purpose. Wait for the warning toast
`.toast.message-warning`. Nothing is saved.

Source: `OrchardCore.Https/AdminMenu.cs:28`, `OrchardCore.Https/Drivers/HttpsSettingsDisplayDriver.cs:17,58`
(the warning is added when the request is not HTTPS, and the form keeps the stored values), `OrchardCore.Https/Views/HttpsSettings.Edit.cshtml:7,14,22`.

`https-1`
- Highlight `current` with the caption `Settings, Security, HTTPS` (1.5 s). Clear.
- Highlight `label.form-check-label:text-is("Require HTTPS")` (2.5 s). Clear.
- Highlight `label.ocat-label:text-is("HSTS mode")` (2 s). Clear. Highlight `.alert-danger` (the HSTS caution) (3 s).
- On screen at the end: the HTTPS page with the red caution "Use HSTS with caution, as browsers may stop connecting over
  HTTP after receiving the header if HTTPS later becomes unavailable." ringed.

`https-2`
- Clear. Highlight `.toast.message-warning` (3 s). Clear. Highlight `input[type="checkbox"][disabled]` (the first match)
  (3 s).
- On screen at the end: the warning "For safety, changing HTTPS-only settings over HTTP has been prevented." in the
  top right and the disabled controls of the form (Require HTTPS, HSTS mode, Require HTTPS Permanent and SSL Port,
  `HttpsSettings.Edit.cshtml:6,16,30,40`). The warning does not close by itself: `WarningAsync` passes no dismissal delay
  (`NotifierExtensions.cs`) and `TheAdmin/Views/Message.cshtml` hides a toast only when a delay above zero is given.
  If a toast has gone anyway, reload the page before the step.

## `05-headers` (Settings, Security, Security Headers)

Start page: `/Admin/Settings/SecurityHeaders`. Nothing is saved (saving restarts the site).

Source: `OrchardCore.Security/AdminMenu.cs:28`, `OrchardCore.Security/Drivers/SecuritySettingsDisplayDriver.cs:19,79,84,89`
(the three tabs), `OrchardCore.Security/Views/ContentSecurityPolicySettings.Edit.cshtml:108`,
`OrchardCore.Security/Views/PermissionsPolicySettings.Edit.cshtml:26,169`, `OrchardCore.Security/Views/ReferrerPolicySettings.Edit.cshtml:36`.

`headers-1`
- Highlight `current` with the caption `Settings, Security, Security Headers` (1.5 s). Clear.
- Highlight `a[role="tab"]:text-is("Content Security Policy")` (2.5 s). Clear.
- `ClickAsync(a[role="tab"]:text-is("Permissions Policy"))`, highlight `tr:has(td:text-is("Camera"))` (2.5 s). Clear.
- `ClickAsync(a[role="tab"]:text-is("Referrer Policy"))`, highlight the select `select[name$="ReferrerPolicy"]` (2.5 s).
- On screen at the end: the Referrer Policy tab with the drop-down set to No referrer, ringed.

## `05-proxy` (Settings, Reverse Proxy)

Start page: `/Admin/Settings/ReverseProxy`. Nothing is saved (saving restarts the site).

Source: `OrchardCore.ReverseProxy/AdminMenu.cs:27` (the entry is directly under Settings),
`OrchardCore.ReverseProxy/Views/ReverseProxySettings.Edit.cshtml:10-24` (labels and the current request values),
`OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4` (the restart warning at the top),
`OrchardCore/OrchardCore.RateLimits.Core/Services/RateLimitPartitionHelpers.cs` (`GetClientPartitionKey`: the signed-in user,
otherwise `Connection.RemoteIpAddress`), `OrchardCore.ReverseProxy/Startup.cs` (`UseForwardedHeaders` runs at order -300, before
`UseRateLimiter` at order 0), `docs/reference/modules/RateLimits/README.md`, section "Reverse proxies and load balancers".

`proxy-1`
- Highlight `current` with the caption `Settings, Reverse Proxy` (1.5 s). Clear.
- In turn (2.5 s each, clearing between), highlight `label.form-check-label:has-text("X-Forwarded-For")`,
  `label.form-check-label:has-text("X-Forwarded-Proto")` and `label.form-check-label:has-text("X-Forwarded-Host")`.
- On screen at the end: the three boxes, unticked, with the ring on the X-Forwarded-Host one.

`proxy-2`
- Clear. Highlight `span.hint:has-text("Current request value")` (the first match, under X-Forwarded-For) (4 s).
- On screen at the end: the line "Current request value;" with the loopback address of the machine, ringed.

## `06-recaptcha` (Settings, Security, reCaptcha)

Start page: `/Admin/Settings/recaptcha`. The fields stay empty and nothing is typed or saved.

Source: `OrchardCore.ReCaptcha/AdminMenu.cs:28` (the label is `reCaptcha`), `OrchardCore.ReCaptcha/Views/ReCaptchaSettings.Edit.cshtml:4-17`,
`OrchardCore.ReCaptcha/Drivers/ReCaptchaSettingsDisplayDriver.cs:15`, `OrchardCore.ReCaptcha/Manifest.cs:19` (ReCaptcha Users),
`OrchardCore.ReCaptcha/Users/Handlers/LoginFormEventEventHandler.cs:30`, `RegistrationFormEventHandler.cs:30` and
`PasswordRecoveryFormEventEventHandler.cs:16,19` (the challenge is validated on sign-in, registration and password recovery).

`recaptcha-1`
- Highlight `current` with the caption `Settings, Security, reCaptcha` (1.5 s). Clear.
- Highlight `input[name$="SiteKey"]` (3 s). Clear. Highlight `input[name$="SecretKey"]` (3 s).
- On screen at the end: the Site Key and Secret Key fields, empty, with their hints "Your Google's reCaptcha Site Key."
  and "Your Google's reCaptcha Secret Key.", the second field ringed.

## `06-rate-limits` (Tools, Rate Limits)

Start page: `/Admin/RateLimits`.

Source: `OrchardCore.RateLimits/AdminMenu.cs:18-19`, `OrchardCore.RateLimits/Views/Admin/Index.cshtml:7,26,69,78`
(title Rate Limit Policies, Add Policy, Read-only Policies), `OrchardCore.RateLimits/Views/RateLimitPolicy.DefaultTags.SummaryAdmin.cshtml:8`
(the Enabled badge), `OrchardCore.RateLimits/Migrations/default-global-policy.recipe.json:8-9` (the Default Global Policy and its description).

`rate-1`
- Highlight `current` with the caption `Tools, Rate Limits` (1.5 s). Clear.
- Highlight `li.list-group-item:has-text("Default Global Policy")` (3 s). Clear.
- Highlight `li.list-group-item:has-text("Default Global Policy") .badge:has-text("Enabled")` (2.5 s).
- On screen at the end: the Rate Limit Policies page with the Default Global Policy row, its description ("Limits how many
  requests each signed-in user, or each IP address for anonymous visitors, can send per minute...") and the Enabled badge ringed.

`rate-2`
- Clear. If the Read-only Policies card is not fully visible, `ScrollAsync(0, 500)` first. Highlight
  `div.card:has(h5:text-is("Read-only Policies"))` (4 s). Clear. Scroll back to the top (`ScrollAsync(0, -500)`) if it was
  scrolled. Highlight `a.create:text-is("Add Policy")` (3 s).
- On screen at the end: the top of the page with the Add Policy button ringed.

## `06-cors` (Settings, Security, Cross-Origin Resource Sharing)

Start page: `/Admin/Cors`. The list is empty: "Nothing here! There are no CORS policies for the moment." Nothing is saved.

Source: `OrchardCore.Cors/AdminMenu.cs:20`, `OrchardCore.Cors/Views/Admin/Index.cshtml:11,14` (CORS Settings, "CORS stands for
Cross-Origin Resource Sharing"), `OrchardCore.Cors/Services/CorsJSLocalizer.cs:17,33-40` (labels), `OrchardCore.Cors/Assets/ts/cors-admin-index.ts`
(the editor markup and ids `allowed-origins` and `allow-credentials`), `docs/reference/modules/Cors/README.md`.

`cors-1`
- Highlight `current` with the caption `Settings, Security, Cross-Origin Resource Sharing` (1.5 s). Clear.
- `ClickAsync(button:text-is("Add a policy"))`. Wait for `label[for="allowed-origins"]`.
- Highlight `label[for="allowed-origins"]` (Allow any origin) (2.5 s). Clear. Highlight `label.ocat-label:text-is("Allowed origins")` (2.5 s).
- On screen at the end: the policy editor (Details, Credentials, Origins cards) with the Allowed origins field ringed.

`cors-2`
- Clear. Highlight `label[for="allow-credentials"]` and then its hint `label[for="allow-credentials"] + span.hint` (the hint says
  "This setting is not allowed in combination with Allow Any Origin.") (3.5 s). Clear. `ScrollAsync(0, 600)` so the Headers and Methods
  cards are visible, then highlight `div.card:has(h5.card-title:has-text("Methods"))` (3 s). Do not press Save or Cancel; the
  clip ends here.
- On screen at the end: the Methods card of the editor, ringed.

## `07-audit` (Settings, Audit Trail; Tools, Audit Trail)

Start page: `/Admin/Settings/AuditTrail` (the Events tab). Nothing is saved or ticked.

Source: `OrchardCore.AuditTrail/Navigation/AuditTrailAdminMenu.cs:33-34,42-43` (both entries), `OrchardCore.AuditTrail/Views/AuditTrailSettings.Edit.cshtml:12,72`,
`OrchardCore.AuditTrail/Drivers/AuditTrailSettingsDisplayDriver.cs:76` and `AuditTrailTrimmingSettingsDisplayDriver.cs:39` (tabs Events and Trimming),
`OrchardCore.Users/AuditTrail/Services/UserAuditTrailEventConfiguration.cs:27` (Login failed is not enabled by default),
`OrchardCore.AuditTrail/Views/AuditTrailTrimmingSettings.Edit.cshtml:8,15`, `OrchardCore.AuditTrail/Settings/AuditTrailTrimmingSettings.cs:5` (ten days).

`audit-1`
- Highlight `current` with the caption `Settings, Audit Trail` (1.5 s). Clear.
- Highlight `label.ocat-label:text-is("Events to record")` (2 s). Clear. Highlight `tr:has(td:text-is("Login failed"))` (4 s). Do not tick it. (`tr:has-text("Login failed")` would also match the
  Login account locked row, whose description is "An attempt to login failed because the user is locked out.",
  `UserAuditTrailEventConfiguration.cs:27-28`.)
- On screen at the end: the Events table with the User category and the Login failed row ringed, its box unticked, next
  to its description "An attempt to login failed."

`audit-2`
- Clear. `ClickAsync(a[role="tab"]:text-is("Trimming"))`. Highlight `label.ocat-label:text-is("Retention period")` (2.5 s), then the
  field `input[name$="RetentionDays"]` (3 s).
- On screen at the end: the Trimming tab with Disable trimming task unticked and Retention period 10, ringed.

`audit-3`
- Clear. `NavigateAsync("/Admin/AuditTrail")`. Wait for the first event row.
- Highlight `current` with the caption `Tools, Audit Trail` (1.5 s). Clear. Highlight the first event row (`ul.list-group li.list-group-item` after the toolbar; the first match
  that contains a time badge) (2.5 s). Clear. Highlight `.filter button[title="Date"]` and then `.filter button[title="Category"]` (2 s each).
- On screen at the end: the Audit Trail list with user events (Logged in, Created), the Date and Category filters visible, the Category filter ringed.
- Source: `OrchardCore.AuditTrail/Views/Admin/Index.cshtml:5`, `OrchardCore.AuditTrail/Views/AuditTrailAdminListFilters.cshtml:6,9`,
  `OrchardCore.AuditTrail/Views/AuditTrailEventMeta.SummaryAdmin.cshtml` (time and user name badges),
  `docs/reference/modules/AuditTrail/README.md` (the log cannot be changed by users, including administrators).

## `07-secrets` (Tools, Security, Secrets)

Start page: `/Admin/Secrets`. Wait for the two seeded rows.

Source: `OrchardCore.Secrets/AdminMenu.cs:20-21,33,40,47` (Tools, Security, Secrets, Migrate Secrets, Secret Stores),
`OrchardCore.Secrets/Views/Admin/Index.cshtml:39` (Add Secret), `OrchardCore.Secrets/Views/Admin/Edit.cshtml:28,74` (name used by settings; stored
values are never displayed), `OrchardCore.Secrets/Views/Migration/Index.cshtml:43,51` (Migrate Secrets), `docs/reference/modules/Secrets/README.md` (values are encrypted).

`secrets-1`
- Highlight `current` with the caption `Tools, Security, Secrets` (1.5 s). Clear.
- Highlight `li.list-group-item:has-text("Payments.ApiKey")` (3 s). Clear. Highlight `li.list-group-item:has-text("Smtp.Password")` (2.5 s). Clear.
- Highlight `menu(Migrate Secrets)` (3.5 s). Do not click it.
- On screen at the end: the Secrets list with both secrets, each with its Text Secret, store and Never expires badges, and the Migrate Secrets entry of the
  admin menu ringed under Tools, Security.

## Checks for private data

- Every email on screen ends in `@example.com`: the users list, the Users audit events, the admin account.
- The Reverse Proxy page prints the address of the current request: it must be the loopback address (`::1` or `127.0.0.1`). If the
  machine shows a network address, record from a browser that reaches the site through `localhost`.
- The Audit Trail list shows no IP address (Allow client IP address stays off).
- The secrets' values are never on screen (the fields are password fields); their names and descriptions are the fake ones above.
- No real password, key or token is typed on camera: the reCaptcha fields stay empty, and `<password>` is only typed outside the capture.
- OCR the finished video at one frame per second and at every scene change for the words `example.com`, `@`, `localhost` and
  any name that is not in the seed tables, as the skill's step 6 describes.

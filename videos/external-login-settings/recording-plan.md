# Recording plan: External Login

How to record `external-login-settings` without deciding anything. The video is an admin video: nothing on screen may
show code, a script editor, or a configuration file. The recipes in this plan are for the person preparing the site and
are applied off camera. A selector below stands for `scene.Page.Locator(...)`. Source references (`file:line`) are
relative to the `OrchardCMS/OrchardCore` checkout the site is built from (`src/OrchardCore.Modules/...`).

Every label in the narration was read in the source. Anything that could not be confirmed without a running site is
listed in `OPEN-QUESTIONS.md`: read it before the first take.

## 1. The site it needs

**One Orchard Core instance, two tenants on the same port.** The subject of the video is a site that lets visitors sign
in with an account from another service. Outbound calls are blocked (see `references/recording.md`), so the "other
service" is a second tenant of the same instance acting as an OpenID Connect authorization server. The first tenant
signs in through it with the OpenID Connect Client Integration feature. To the viewer it is just "a provider named
Example Accounts".

| | Tenant `Default` (the site on camera) | Tenant `idp` (the stand-in provider, never on camera except as a redirect) |
|---|---|---|
| URL | `http://localhost:5010/` | `http://localhost:5010/idp` |
| Auto Setup | `ShellName` `Default`, `SiteName` `Orchard Core`, `SiteTimeZone` `America/Los_Angeles`, `DatabaseProvider` `Sqlite`, `RecipeName` `Blog`, `AdminUsername` `admin`, `AdminEmail` `admin@example.com`, `AdminPassword` the value of `VIDEO_PASSWORD` | `ShellName` `idp`, `SiteName` `Example Accounts`, `SiteTimeZone` `America/Los_Angeles`, `DatabaseProvider` `Sqlite`, `RecipeName` `Blog`, `RequestUrlPrefix` `idp`, `AdminUsername` `idpadmin`, `AdminEmail` `idpadmin@example.com`, `AdminPassword` the value of `VIDEO_PASSWORD` |

- Configure the two tenants with the Auto Setup environment variables described in
  `docs/reference/modules/AutoSetup/README.md` (`OrchardCore__AutoSetup__Tenants__0__...` and `...__1__...`), run from
  `src/OrchardCore.Cms.Web` with `ORCHARD_APP_DATA` pointing to an empty folder outside both repositories, as in
  `references/recording.md`.
- Keep the dead `HTTP_PROXY` / `HTTPS_PROXY` of `references/recording.md`, and add `NO_PROXY=localhost,127.0.0.1`. The
  Default tenant calls the `idp` tenant over HTTP (discovery document, token and user info endpoints) and must not go
  through the dead proxy.
- The password is never written down: use `VIDEO_PASSWORD` everywhere a `<password>` appears below.
- Record in a signed-out or signed-in browser context as each clip says. The recorder's `state.json` is the sign-in
  of `admin` on the Default tenant, made once with `Recorder setup`.

### Features to enable before recording

- Default tenant: none beyond the Blog recipe. **Do not enable Users Registration, Email or any provider.** The clip
  `02-features` enables OpenID Connect Client Integration on camera, which is what turns External Authentication on.
- `idp` tenant: OpenID Connect Core Services, OpenID Connect Management UI and OpenID Connect Authorization Server
  (feature ids `OrchardCore.OpenId`, `OrchardCore.OpenId.Management`, `OrchardCore.OpenId.Server`: see
  `OrchardCore.OpenId/Manifest.cs:11-17, 31-52`). The recipe below enables them.

### Seed the `idp` tenant (off camera, once)

Sign in to `http://localhost:5010/idp/Login` as `idpadmin`, open Tools, Deployments, JSON Import
(`OrchardCore.Deployment/AdminMenu.cs:18-30`) and import this recipe. The property names are those of the recipe step
models (`OrchardCore.OpenId/Recipes/OpenIdServerSettingsStepModel.cs`, `OpenIdApplicationStepModel.cs`).

The application is a **public** client and has no client secret, on purpose: the secret of the `OpenIdClientSettings`
recipe step is stored as written, but the client reads it back with the data protection API
(`OrchardCore.OpenId/Configuration/OpenIdClientConfiguration.cs:85-96`, `OrchardCore.Secrets.Abstractions/SecretValueExtensions.cs:53-67`),
so a secret written in plain text in a recipe is not used and the token request would fail. A public client needs no
secret (`OrchardCore.OpenId/OpenIdApplicationSettings.cs:54-57`). See `OPEN-QUESTIONS.md`, question 3, for the fallback.

```json
{
  "steps": [
    {
      "name": "feature",
      "enable": [ "OrchardCore.OpenId", "OrchardCore.OpenId.Management", "OrchardCore.OpenId.Server" ]
    },
    {
      "name": "OpenIdServerSettings",
      "Authority": "http://localhost:5010/idp",
      "EnableAuthorizationEndpoint": true,
      "EnableTokenEndpoint": true,
      "EnableUserInfoEndpoint": true,
      "EnableLogoutEndpoint": true,
      "AllowAuthorizationCodeFlow": true,
      "AllowRefreshTokenFlow": true
    },
    {
      "name": "OpenIdApplication",
      "ClientId": "example-site",
      "DisplayName": "Example site",
      "Type": "public",
      "ConsentType": "implicit",
      "RedirectUris": "http://localhost:5010/signin-oidc",
      "PostLogoutRedirectUris": "http://localhost:5010/signout-callback-oidc",
      "AllowAuthorizationCodeFlow": true,
      "AllowRefreshTokenFlow": true,
      "AllowLogoutEndpoint": true,
      "ScopeEntries": [ { "Name": "email" }, { "Name": "profile" } ]
    }
  ]
}
```

Then create one user in the `idp` tenant (Access Control, Users, Add User: `OrchardCore.Users/Views/UsersAdminListCreate.cshtml:7`):

| User Name | Email | Password |
|---|---|---|
| `taylor.brooks` | `taylor.brooks@example.com` | `<password>` |

Do not use `admin@example.com` for this user: the Default tenant's `admin` would then share the email address and the
first sign-in would show "Link your account." instead of "Create a new account." (the second question of the step
`how-3`, `ExternalAuthenticationsController.cs:148-176`).

### Seed the Default tenant (off camera, after the clip `02-features`)

Clip `02-features` enables the client feature. Right after it, still off camera:

1. Open Tools, Deployments, JSON Import on the Default tenant and import this recipe:

   ```json
   {
     "steps": [
       {
         "name": "OpenIdClientSettings",
         "DisplayName": "Example Accounts",
         "Authority": "http://localhost:5010/idp",
         "ClientId": "example-site",
         "CallbackPath": "/signin-oidc",
         "SignedOutCallbackPath": "/signout-callback-oidc",
         "Scopes": "openid profile email",
         "ResponseType": "code",
         "ResponseMode": "form_post"
       }
     ]
   }
   ```

   (`OrchardCore.OpenId/Recipes/OpenIdClientSettingsStep.cs`.) "Example Accounts" is the text of the provider button.
2. Open Settings, Security, Registration (`/Admin/Settings/userRegistration`). Make sure **Disable new external
   registration** is **selected** (it is expected to be selected already, see `OPEN-QUESTIONS.md`, question 2), and
   select Save. Make sure **Do not ask username**, **Do not ask email address** and **Do not create local password**
   are cleared, and **Use a script to generate userName based on external provider claims** is cleared.
3. Open Settings, Security, User Login (`/Admin/Settings/userLogin`), tab External Login. Make sure **Use external
   provider for login.** and **Use a script to set user roles, user claims and user properties based on external
   provider claims** are cleared. Select Save.
4. Open `http://localhost:5010/Login` in a signed-out browser: the page must show a button named Example Accounts. If
   not, the client settings are wrong: stop here and fix them (the server log of the Default tenant says why).

Fake data in this plan: `admin`, `idpadmin`, `taylor.brooks` and their `@example.com` addresses, the site names, the
client id `example-site`. There is no other data to seed: the Blog recipe content is on the home page only.

## 2. Order of the clips, dependencies, snapshots

The clips change the site, so they are recorded in this order; the seed steps of the Default tenant come between
the first and the second clip. A snapshot is a copy of the whole `App_Data` folder (both tenants) and of `state.json`, made with
the site stopped.

| Snapshot | Taken | Site state |
|---|---|---|
| S0 | After the `idp` tenant is seeded and `Recorder setup` is done | Default tenant: Blog only. `idp` tenant: server, application, user `taylor.brooks`. |
| S1 | After `02-features` and the "Seed the Default tenant" steps | OpenID Connect Client Integration and External Authentication enabled; provider button "Example Accounts"; **Disable new external registration** selected; **Use external provider for login.** cleared. |
| S2 | After `04-registration` | As S1, but **Disable new external registration** cleared. |
| S3 | After `05-register` | As S2, plus the local user created for `taylor.brooks`, linked to the provider. |

| Order | Clip | Starts from | Changes the site? |
|---|---|---|---|
| 1 | `02-features` | S0 | Yes: enables the client feature and External Authentication. Take S1 after the seed steps. |
| 2 | `03-login` | S1 | No (nothing is created: the sign-in is refused). |
| 3 | `04-registration` | S1 | Yes: clears **Disable new external registration**. Take S2. |
| 4 | `05-register` | S2 | Yes: creates the local user and the link. Take S3. |
| 5 | `06-settings` | S3 | Yes: selects **Use external provider for login.** |
| 6 | `06-straight` | the state `06-settings` left | No. Needs the user created by `05-register` and the option selected by `06-settings`. |

To record one clip again, restore the snapshot in the "Starts from" column (for `06-straight`: S3, then do the clip
`06-settings` again, or select the option by hand and save), start the site, and record.

Common settings of every clip: `Recorder` defaults of `references/recording.md` (1536x864, light colour scheme,
en-US). The admin menu is expanded before `StartCaptureAsync` whenever a step says "the menu shows".

### Signing a visitor in at the provider (off camera, clips `03-login`, `05-register`, `06-straight`)

These three clips show a visitor who is signed out of the Default tenant. Before `StartCaptureAsync`, in the clip's
scene:

1. `scene.Context.ClearCookiesAsync()` (signs `admin` out).
2. `scene.OpenAsync("/idp/Login")`, fill **Username or email address** with `taylor.brooks`, **Password** with
   `<password>`, and select **Log in** (`OrchardCore.Users/Views/LoginFormCredentials.cshtml:7`, `LoginForm.Edit.cshtml:9`);
   wait for the navigation to finish.
3. `scene.OpenAsync("/Login")` for `03-login` and `05-register`; `scene.OpenAsync("/")` for `06-straight`.

The visitor is then already signed in at the provider, so selecting the provider button returns to the site at once
and no provider sign-in form is recorded. This is why the narration says only that "the provider confirms who the
visitor is".

## 3. The clips

### Chapter 02, clip `02-features` (admin, from S0)

Starts on `/Admin/Features`, signed in as `admin` (the `state.json` session). Before `StartCaptureAsync`: expand Tools
in the admin menu so Features is visible; if the page offers a layout selector (List, Grid), choose List; wait for
`#search-box`. The Visibility filter shows its default: **Hide on demand** and **Hide always enabled** selected
(`OrchardCore.Features/Views/Admin/Features.cshtml:64-67`). A row below means the `.list-group-item` that contains the
feature name (rows of on-demand features have buttons without an id, so match on the name).

- `features-1`. Actions: highlight the **Features** item of the admin menu, then type `External Authentication` in
  `#search-box` (`TypeAsync`, delay 60), and highlight the Visibility button (`button[data-id="visibility-filter"]`)
  with the caption "Hide on demand". End of the narration: the Features page with the search text typed, the
  row External Authentication not among the rows listed, the Visibility button highlighted, the menu showing Tools, Features.
  Labels: Tools, Features `OrchardCore.Features/AdminMenu.cs:26-28`; Visibility and Hide on demand `Features.cshtml:64-65`.
- `features-2`. Actions: clear highlights, select the Visibility button, in the open list select **Hide on demand** so
  it is no longer ticked, close the list (Escape). The row **External Authentication** appears. Highlight its Enable
  button (inactive: a disabled button, `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:25-26`).
  End: the row External Authentication listed, its grey Enable button highlighted.
- `features-3`. Actions: clear highlights, replace the search text with `OpenID Connect Client`, highlight the row
  **OpenID Connect Client Integration** (`OrchardCore.OpenId/Manifest.cs:19-29`), then select its Enable button
  (`#btn-enable-OrchardCore_OpenId_Client`) with `ClickAndWaitForNavigationAsync`. The Features page loads again. End:
  the Features page after the reload, with the green notification that the feature was enabled (the text is built from
  `OrchardCore.Features/Controllers/AdminController.cs:317`: "The feature ... was enabled." or "The following features
  were enabled: ..."; the Visibility filter is back to its default after the reload).
- `features-4`. Actions: select the Visibility button, untick **Hide on demand**, close the list, type
  `External Authentication` in `#search-box`, then move the cursor onto the inactive Disable button of the row (the
  wrapping `span[data-bs-toggle="tooltip"]`) so the tooltip shows. End: the row External Authentication with its red
  Disable button inactive, and the tooltip "This feature is automatically enabled based on dependency and cannot be
  manually disabled." visible (`FeatureEntry.Buttons.SummaryAdmin.cshtml:19`). Wait for the tooltip text before the
  narration ends.

After this clip: seed the Default tenant (section 1), then take snapshot S1.

### Chapter 03, clip `03-login` (visitor, from S1)

Sign the visitor in at the provider as described in section 2. The clip starts on `http://localhost:5010/Login`: the
login page of the Default tenant, with the local form (**Username or email address**, **Password**, **Remember me**,
**Log in**) in the left column and **Use another service to log in** with the button **Example Accounts** in the right
column.

- `login-1`. Actions: highlight the heading **Use another service to log in** (`h1:has-text("Use another service to log in")`),
  then highlight the button `button[name="provider"][value="OpenIdConnect"]` with the caption "Example Accounts". End:
  both columns visible, heading and button highlighted. Labels: `OrchardCore.Users/Views/Account/Login.cshtml:27`
  (Log in), `:38` (Use another service to log in), `:43` (the button text is the provider's display name, set in the
  client recipe above).
- `login-2`. Actions: clear highlights, select the button `button[name="provider"][value="OpenIdConnect"]` with
  `ClickAndWaitForNavigationAsync`; wait until the browser is back on `/Login` and the text "New registrations are
  disabled for this site." is on the page (`GetByText`). End: the login page again, with the message shown (the
  redirect chain through the provider is too quick to show).
- `login-3`. Actions: highlight the message "New registrations are disabled for this site." End: the message
  highlighted at the top of the login page, the local form and the button still visible. Source of the message:
  `OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:182`.

### Chapter 04, clip `04-registration` (admin, from S1)

Signed in as `admin`. Starts on `/Admin/Settings/userRegistration` with Settings, Security expanded in the admin menu
so Registration is visible and highlighted as the current page. Wait for the network to be
idle. If the page shows a tab strip, the **External Authentication** tab is the one selected; if it shows no tab strip
(only one group exists without Users Registration), the group is shown directly: both are fine. Wait until the
checkbox **Disable new external registration** is visible (`GetByLabel("Disable new external registration")`). The
script editor of the username script must not be visible: its wrapper (`#ScriptToGenerateUsername_Wrapper`) has the
class `d-none` while **Use a script to generate userName based on external provider claims** is cleared. Never tick
that box on camera.

- `reg-1`. Actions: highlight the **Registration** menu item, then the heading "Configuring External Authentication for
  User Registration with Third-Party Providers". End: the top of the page with the heading highlighted.
  Labels: `OrchardCore.Users/RegistrationAdminMenu.cs:26-28` (Settings, Security, Registration), tab and group name
  `Drivers/ExternalRegistrationSettingsDisplayDriver.cs:44`, heading `Views/ExternalRegistrationSettings.Edit.cshtml:14`.
- `reg-2`. Actions: clear highlights, highlight the checkbox **Disable new external registration** with its hint
  (`:23-24`). End: the checkbox ticked and highlighted, with its hint visible.
- `reg-3`. Actions: clear highlights (before any scrolling), untick **Disable new external registration**, scroll to
  the **Save** button (`button.save`, `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`) and select it with
  `ClickAndWaitForNavigationAsync`. End: the page reloaded with the notification "Site settings updated successfully."
  (`OrchardCore.Settings/Controllers/AdminController.cs:119`) and the checkbox cleared.
- `reg-4`. Actions: highlight the checkboxes **Do not ask username**, **Do not ask email address** and **Do not create
  local password**, one ring each, with no click (`ExternalRegistrationSettings.Edit.cshtml:34, 45, 56`). End: the
  three rings visible, the three checkboxes cleared.
- `reg-5`. Actions: none beyond keeping the three rings; move the cursor slowly along the hint of **Do not ask email
  address** (`:46`) and then the hint of **Do not create local password** (`:57`). Do not point at the hint of **Do
  not ask username** (`:35`): it names a code interface. End: as `reg-4`.
- `reg-6`. Actions: clear highlights, highlight the label **Use a script to generate userName based on external
  provider claims** (`:67`). Do not tick it. End: the label highlighted, no script editor on screen.

After this clip: take snapshot S2.

### Chapter 05, clip `05-register` (visitor, from S2)

Sign the visitor in at the provider as described in section 2 (as `taylor.brooks`). The clip starts on
`http://localhost:5010/Login`.

- `new-1`. Actions: select the button `button[name="provider"][value="OpenIdConnect"]` with
  `ClickAndWaitForNavigationAsync`; wait for the heading "Create a new account." (`h4`). End: the page headed **Create a
  new account.** (`OrchardCore.Users/Views/ExternalAuthentications/RegisterExternalLogin.cshtml:9`), with the text that says the visitor
  authenticated with the provider (`:11`) and the form. That text names the provider by its scheme name: it reads
  "You've successfully authenticated with **OpenIdConnect**." and not "Example Accounts" (`:11` shows
  `ViewData["LoginProvider"]`, set from `info.LoginProvider` at `ExternalAuthenticationsController.cs:156`).
- `new-2`. Actions: highlight the fields in the order of the narration: **Username** (its value is a generated value
  starting with the letter u: leave it), **Email**, **Password**, **Confirm password** (`:19, 28, 37, 43`). If the
  **Email** field is already filled in (the provider may share the address), leave it. End: the four fields visible,
  the last one highlighted.
- `new-3`. Actions: clear highlights. If **Email** is empty, type `taylor.brooks@example.com`. Type `<password>` in
  **Password** and in **Confirm password** (`TypeAsync`). Select **Register** (`:52`) with
  `ClickAndWaitForNavigationAsync`; the home page of the Blog loads. Then `NavigateAsync` to
  `/OrchardCore.Users/ExternalAuthentications/ExternalLogins`, the address of the **External Logins** link of the user
  menu (`OrchardCore.Users/Views/UserMenuItems-ExternalLogins.cshtml`; see `OPEN-QUESTIONS.md`, question 6). End: the
  page with the heading **Registered Logins**, one row named `OpenIdConnect`, and its **Remove** button
  (`Views/ExternalAuthentications/ExternalLogins.cshtml:12, 26`). The row proves the visitor is signed in and linked.

After this clip: take snapshot S3.

### Chapter 06, clip `06-settings` (admin, from S3)

Signed in as `admin`. Starts on `/Admin/Settings/userLogin` with Settings, Security expanded so **User Login** is
visible. Before `StartCaptureAsync`: select the tab **External Login** once, wait until `#doc_editor_wrapper` and
`#SyncRolesScript_wrapper` are hidden (`getComputedStyle(...).display === 'none'`, set by
`OrchardCore.Users/Assets/ts/external-login-settings.ts`), then select the tab **General** again. Those wrappers hold a
code editor: never tick **Use a script to set user roles, user claims and user properties based on external provider
claims**, and never let the editor appear on screen.

- `loginset-1`. Actions: highlight the **User Login** menu item, select the tab **External Login**
  (`a.nav-link:has-text("External Login")`), wait for `#doc_editor_wrapper` to be hidden again, then highlight the
  checkbox **Use external provider for login.** with its hint. End: the External Login tab selected, the heading
  "Configuring External Authentication for User Login with Third-Party Providers" and the first checkbox highlighted,
  no editor visible. Labels: `OrchardCore.Users/AdminMenu.cs:40-42` (Settings, Security, User Login), tab
  `Drivers/ExternalLoginSettingsDisplayDriver.cs:38`, checkbox and hint `Views/ExternalLoginSettings.Edit.cshtml:15-16`.
- `loginset-2`. Actions: clear highlights, highlight the label **Use a script to set user roles, user claims and user
  properties based on external provider claims** (`:26`). Do not tick it and do not highlight its hint (it names a
  code interface). End: that label highlighted, no editor visible.
- `loginset-3`. Actions: clear highlights, tick **Use external provider for login.**, select **Save**
  (`ClickAndWaitForNavigationAsync`). End: the page reloaded with "Site settings updated successfully."; the first tab
  is shown again.

### Chapter 06, clip `06-straight` (visitor, from the state `06-settings` left)

Sign the visitor in at the provider as described in section 2 (as `taylor.brooks`), then `scene.OpenAsync("/")`.

- `straight-1`. Actions: `NavigateAsync("/Login")`; the browser goes to the provider and returns to the home page of the
  Blog without showing the login form (`OrchardCore.Users/Services/ExternalLoginFormEvents.cs:36-79`); wait for the
  home page. Then `NavigateAsync("/OrchardCore.Users/ExternalAuthentications/ExternalLogins")`. End: the page **Registered Logins**
  with the row `OpenIdConnect` and **Remove**, which shows the visitor is signed in. The login form must never appear
  during the clip.

After the clip: restore S3 (the option **Use external provider for login.** must be cleared again before any other take).

## 4. Slides and cards

- `how-1` (slide): the heading "Sign in with another service" and three cards: A provider adds a button; The provider
  confirms identity; The site keeps its own account.
- `how-2` (slide): "What External Authentication provides" and four cards: Pages for a first sign-in; Two groups of
  settings; Linked accounts; Used by every provider.
- `how-3` (slide): "Signing in with a provider" and three cards: Already linked?; Same email address?; New
  registrations allowed?
- `intro`, `outro` (cards): no recording; the title and subtitle come from the storyboard.

## 5. Checks for private data

- Only fake names appear: `admin`, `taylor.brooks`, the site names Orchard Core and Example Accounts, and addresses at
  `example.com`. The Blog recipe's home page holds sample posts and no real names.
- No password is typed visibly: the password fields show dots. `<password>` is never written to a file, a shell history
  or a log of the take; `VIDEO_PASSWORD` is set only in the shell that runs the recorder.
- No provider settings on screen: the Authority and client id are only in the recipes applied off camera, and
  `Recorder` never opens the OpenID Connect settings pages. There is no client secret anywhere in this setup.
- No real provider is contacted: confirm in the browser's network list of one take that the only hosts are
  `localhost`. The only address bar contents that may appear are `localhost` addresses (the capture shows the page,
  not the address bar).
- Run the OCR check of `SKILL.md` step 6 on the finished video and search for `@` and for the
  value of `VIDEO_PASSWORD`.

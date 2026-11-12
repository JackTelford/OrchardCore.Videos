# Recording plan: OpenID Connect Client

This plan is for the person who records the video. It needs no decisions: the site, the seed data, the order of the
clips and every action of every step are listed. Placeholders: `<password>` is the password chosen outside git
(`VIDEO_PASSWORD`), and `<client-secret>` is a client secret generated for this recording and kept outside git. Both
appear on screen only as dots in password fields.

Selectors below stand for `scene.Page.Locator(...)`. A field of the Authentication Client page is written as
`.ocat-wrapper:has(> label:text-is("<label>")) input`, a checkbox as `.form-check:has(label:text-is("<label>")) input`.
Source paths are relative to the root of the Orchard Core checkout, and every label was read there.

## The site it needs

- Run `src/OrchardCore.Cms.Web` of an Orchard Core checkout on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing
  at an empty folder outside both repositories.
- One site, two tenants. The Default tenant is the client that the video shows. A second tenant with the URL prefix
  `idp` plays the identity provider. Both are installed by Auto Setup on their first request, so no setup or login form
  is recorded. This video is not about the Setup screen.

| Setting | Default tenant (the client) | `idp` tenant (the provider) |
| --- | --- | --- |
| `ShellName` | `Default` | `idp` |
| `SiteName` | `Orchard Core` | `Example Identity` |
| `SiteTimeZone` | `America/Los_Angeles` | `America/Los_Angeles` |
| `AdminUsername` | `admin` | `jamie.chen` |
| `AdminEmail` | `admin@example.com` | `jamie.chen@example.com` |
| `AdminPassword` | `<password>` | `<password>` |
| `DatabaseProvider` | `Sqlite` | `Sqlite` |
| `RecipeName` | `Blog` | `Blog` |
| `RequestUrlPrefix` | (empty) | `idp` |

  Set them as environment variables of the site process: `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`,
  `OrchardCore__AutoSetup__Tenants__1__ShellName=idp`, and so on for every row (the index 0 is the Default tenant, 1 is
  `idp`). Leave `AutoSetupPath` empty. The option names are in
  `src/OrchardCore.Modules/OrchardCore.AutoSetup/Options/TenantSetupOptions.cs`. The tenant `idp` is created when the
  Default tenant is installed.
- Block outbound calls as the skill says: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, and also
  `NO_PROXY=localhost,127.0.0.1`. The client tenant calls the provider tenant from the server (discovery, token and
  user information), and those calls must not go through the dead proxy.
- Install the Default tenant by opening `http://localhost:5010/` once, then the provider by opening
  `http://localhost:5010/idp` once. Wait until each shows its home page.
- Features on the Default tenant: only what the Blog recipe enables. The clips enable OpenID Connect Client Integration
  themselves (`02-enable`). Do **not** enable Secrets: the Client Secret field must be a plain password field.
- Sign the recorder in once on the Default tenant (`Recorder setup`, user `admin`, `VIDEO_PASSWORD`), which saves
  `state.json`. Never save the setting "Use external provider for login." on the Default tenant: it sends the admin
  login to the provider and breaks `Recorder setup` when a take is repeated.

### Seed on the provider (`idp` tenant), outside the capture

Sign in at `http://localhost:5010/idp/Admin` as `jamie.chen` (password `<password>`), then:

1. Tools, Features (`/idp/Admin/Features`, `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`). Search
   for `OpenID Connect Authorization Server` and click Enable on that row (`#btn-enable-OrchardCore_OpenId_Server`).
   Its dependencies (OpenID Connect Core Services, OpenID Connect Management UI) turn on with it
   (`OrchardCore.OpenId/Manifest.cs`).
2. Settings, OpenID Connect, Authorization Server (`ServerAdminMenu.cs:18-22`). Check Enable Authorization Endpoint,
   Enable Token Endpoint and Enable User Info Endpoint
   (`Views/OpenIdServerSettings.Edit.cshtml:25,35,75`). The Flows checkboxes appear after the endpoints are checked
   (`Assets/ts/openid-server-settings.ts`). Check Allow Authorization Code Flow (`:113`). Leave everything else as it is,
   and click Save. The transport security requirement is always off (`Configuration/OpenIdServerConfiguration.cs:226`),
   so plain http works.
3. Access Control, OpenID Connect, Applications (`ManagementAdminMenu.cs:18-24`), then New application
   (`Views/Application/Index.cshtml:26`). Fill the form (`Views/Application/Create.cshtml`):
   - Display Name (`:26`): `Example Site`
   - Type (`:34`): Confidential client (`:37`)
   - Client Id (`:45`): `example-site`
   - Client Secret (`:57`): `<client-secret>` (the shuffle button next to the field generates one: keep it outside git
     and use the same value in clip `03-connect`)
   - Allow Authorization Code Flow (`:79`): checked
   - Redirect Uris (`:143`): `http://localhost:5010/signin-oidc`
   - Consent type (`:200`): Implicit consent (`:204`). With this choice the provider shows no Authorization page
     (`Controllers/AccessController.cs:136`), which keeps clip `05-signin` short.
   - Allowed Scopes (`:221`): check `email` and `profile`
   - Click Save (`:248`).
4. Leave the provider as it is. The user who signs in on camera is `jamie.chen` of this tenant (a fake person,
   `jamie.chen@example.com`). That email address differs from `admin@example.com` on purpose: with the same address, the
   client would show Link your account. instead of Create a new account.

### Fake data

| What | Value |
| --- | --- |
| Client admin | `admin`, `admin@example.com`, `<password>` |
| Provider user (signs in on camera) | `jamie.chen`, `jamie.chen@example.com`, `<password>` |
| Provider application | Display Name `Example Site`, Client Id `example-site`, secret `<client-secret>` |
| Typed in the client settings | Display Name `Example Identity`, Authority `http://localhost:5010/idp`, ClientId `example-site`, CallbackPath `/signin-oidc`, Scopes `email` |
| Username typed on the registration page | `jamie.chen` |

## Order and snapshots

The clips are recorded in the order they play, and each one starts from what the previous one left:
`02-enable`, `03-connect`, `04-registration`, `05-signin`, `06-account`. `04-login` and `01-how` are slides.

Stop the site and copy `App_Data` and `state.json` aside at these points. Restore the matching copy (site stopped)
before a new take of a clip.

| Snapshot | Taken | Restore it before |
| --- | --- | --- |
| S0 | both tenants installed, the provider seeded, `Recorder setup` done | `02-enable` |
| S1 | after `02-enable` (the client feature is enabled) | `03-connect` |
| S2 | after `03-connect` (the client settings are saved) | `04-registration` |
| S3 | after `04-registration` (the registration options are saved) | `05-signin` |
| S4 | after `05-signin` (the local account `jamie.chen` exists) | `06-account` |

Clips that change the site: `02-enable` (enables a feature), `03-connect` and `04-registration` (save settings),
`05-signin` (creates the local account and its link to the provider). `06-account` changes nothing.

Do a full dry run by hand from S0 before recording: enable the feature, save the settings, sign in, and check that
every page of the clips below appears as described. See OPEN-QUESTIONS.md for what to look at.

## Chapter 00: opening card

`intro`: card. Nothing to record.

## Chapter 01: slides

Drawn by `slides.py`, no recording.

- `how-1`: three cards, Identity provider, Your site and Local account. The first two are bright.
- `how-2`: four cards, Authority, ClientId, Client Secret and CallbackPath, with what each one is.
- `how-3`: three cards, The login page, The provider and Your site.
- `how-4`: three cards, A linked account, The same email address as a local account and No matching account.

## Clip 02-enable (chapter 02)

Session: `admin` on the Default tenant, from `state.json`. Before `StartCaptureAsync`, open `/Admin/Features`
(Tools, Features: `OrchardCore.Features/AdminMenu.cs:26-27`). The search box is empty and all features are listed.

- `enable-1`: Type `OpenID Connect` in `#search-box` (`OrchardCore.Features/Views/Admin/Features.cshtml:27`), delay 70.
  Wait until the row `.list-group-item` that contains `#btn-enable-OrchardCore_OpenId_Client` is visible, and highlight
  it without a caption. On screen at the end: the list filtered to the OpenID Connect features, the row "OpenID Connect
  Client Integration" with its id `OrchardCore.OpenId.Client`, its description "Allows authentication of users through an
  external OpenID Connect authorization server (also known as an identity provider).", the dependency badges "OpenID
  Connect Core Services" and "External Authentication", and the Enable button
  (`OrchardCore.OpenId/Manifest.cs:21`, `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
- `enable-2`: Clear the highlight, then `ClickAndWaitForNavigationAsync` on `#btn-enable-OrchardCore_OpenId_Client`.
  After the page reloads, if the search box is empty, type `OpenID Connect` again so the row is on screen. On screen at
  the end: the notification "The feature OpenID Connect Client Integration was enabled."
  (`OrchardCore.Features/Controllers/AdminController.cs:317`) and the same row with a Disable button
  (`#btn-disable-OrchardCore_OpenId_Client`) and the Enabled badge.

## Clip 03-connect (chapter 03)

Session: `admin` on the Default tenant, from `state.json`. Before `StartCaptureAsync`, open `/Admin` (the dashboard),
with the admin menu visible. The page that the clip works on is `/Admin/Settings/OrchardCore.OpenId.Client`. The page is
long: scroll with `ScrollAsync` before moving to a field below the fold, and clear highlights before scrolling.

- `connect-1`: Click the menu item Settings, then OpenID Connect, then Authentication Client
  (`OrchardCore.OpenId/ClientAdminMenu.cs:26-28`); the last click is `ClickAndWaitForNavigationAsync`. Type
  `Example Identity` in `.ocat-wrapper:has(> label:text-is("Display Name")) input`
  (`Views/OpenIdClientSettings.Edit.cshtml:14`). On screen at the end: the page with the alert "The website might be
  restarted upon saving the settings, potentially leading to temporary unresponsiveness during the process." at the top
  (`OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`), and Display Name filled with Example Identity.
- `connect-2`: Type `http://localhost:5010/idp` in the Authority field (`:23`) and `example-site` in the ClientId field
  (`:32`). Press Tab after each. On screen at the end: Display Name, Authority and ClientId filled, each label marked as
  required, and the hint "The 'client_id' assigned by the identity provider." under ClientId (`:36`).
- `connect-3`: Type `/signin-oidc` in the CallbackPath field (`:41`; its placeholder is the same path, `:43`). Press Tab.
  Highlight the field. On screen at the end: CallbackPath filled, with its hint "The request path, relative to the
  application's base path, where the user-agent will be redirected. The middleware will handle this request upon arrival."
  (`:45`).
- `connect-4`: Scroll until the Scopes field (`:71`) is visible. Type `email` in it, press Tab, and highlight the field.
  On screen at the end: Scopes holds `email`, and the hint "Request additional scopes beyond 'openid' and 'profile'."
  (`:75`) is visible.
- `connect-5`: Scroll up until SignedOut CallbackPath (`:50`) and SignedOut Redirect Uri (`:62`) are on screen (they sit
  above Scopes), move the cursor to each in turn, then scroll down and move to the Store tokens? checkbox (`:83`). Type nothing and click nothing. Highlight Store tokens? last. On screen at
  the end: Store tokens? unchecked and highlighted, with the hint "Store external authentication tokens for future use.".
- `connect-6`: Scroll until the headings Code Authentication Flow (`:92`), Hybrid Authentication Flow (`:111`) and Implicit
  Authentication Flow (`:158`) are on screen, or as many as fit. Click `input.openid-use-code-id-token-flow` (the label
  "Use code id_token response type", `:120`): it becomes checked and the Client Secret field opens. Then click
  `input.openid-use-code-flow` (the label "Use 'code' response type", `:101`): the first checkbox clears
  (`Assets/ts/openid-client-settings.ts:57`). On screen at the end: only Use 'code' response type is checked, and the
  Client Secret field is open below the Hybrid Authentication Flow checkboxes.
- `connect-7`: Wait until `.openid-client-secret` has the class `show`. Type `<client-secret>` in
  `.ocat-wrapper:has(> label:text-is("Client Secret")) input[type=password]` (`:147`, rendered by
  `OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:103`). Then move the cursor over the Response Mode
  select (`:184`) without changing it. On screen at the end: the Client Secret field filled with dots, and Response Mode
  showing form_post. The field must stay a password field: do not click any eye or copy button.
- `connect-8`: Click `button.save` (the label "Save", `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`) with
  `ClickAndWaitForNavigationAsync`. The tenant restarts: wait up to 20 seconds for the notification "Site settings updated
  successfully." (`OrchardCore.Settings/Controllers/AdminController.cs:119`). Hold at the top of the page for the first
  two sentences of the narration so the alert (it is on the page from `connect-1`) and the notification are on screen, then
  scroll to the Client Secret field. On screen
  at the end: the Client Secret field empty, with its placeholder "The value is securely stored. Enter a new value to
  update it, or leave it blank to keep it." (`SecretInputTagHelper.cs:103`), and Use 'code' response type still checked.
  If the save shows a validation message, such as "Unknown response type ."
  (`OrchardCore.OpenId/Services/OpenIdClientService.cs:82`; with no flow checked the page posts the response type "none",
  `OrchardCore.OpenId/Drivers/OpenIdClientSettingsDisplayDriver.cs:159-163`), a flow checkbox is not checked: restore S1 and record again.

## Clip 04-registration (chapter 04)

Session: `admin` on the Default tenant. Before `StartCaptureAsync`, open `/Admin`, with the admin menu visible.

- `reg-1`: Click the menu item Settings, then Security, then Registration
  (`OrchardCore.Users/RegistrationAdminMenu.cs:27-28`; the menu exists because External Authentication is enabled,
  `OrchardCore.Users/Startup.cs:301`); the last click is `ClickAndWaitForNavigationAsync`. The page is
  `/Admin/Settings/userRegistration`. The Blog recipe does not enable User Registration, so the External Authentication
  options are the only editor on this page, and `OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:87,113` renders a single group without a tab strip: there is
  no tab to click. On screen at the end: the heading "Configuring External Authentication for User Registration with
  Third-Party Providers" (`OrchardCore.Users/Views/ExternalRegistrationSettings.Edit.cshtml:14`) and the checkboxes under
  it, all unchecked: the four of `reg-2`, and a fifth, "Use a script to generate userName based on external provider
  claims" (`:67`), which the narration does not cover.
- `reg-2`: Move the cursor to each checkbox in turn, and highlight it for the length of its sentence: Disable new external
  registration (`:23`), Do not ask username (`:34`), Do not ask email address (`:45`) and Do not create local password
  (`:56`). Clear the highlight between them, and leave the fifth checkbox (`:67`) alone. On screen at the end: the
  checkboxes, still unchecked, Do not create local password highlighted, with the hints under them.
- `reg-3`: Click the checkboxes Do not ask email address and Do not create local password
  (`.form-check:has(label:text-is("Do not ask email address")) input` and the same for "Do not create local password").
  Click `button.save` with `ClickAndWaitForNavigationAsync`. On screen at the end: the notification "Site settings updated
  successfully.", Do not ask email address and Do not create local password checked, the other two unchecked.

## Clip 04-login (chapter 04, slide)

- `login-1`: slide with two cards, Use external provider for login. and Disable local password login., under the heading
  "Settings, Security, User Login". Nothing is recorded, and neither option is saved on the site.

## Clip 05-signin (chapter 05)

Session: **signed out**. The recorder always loads `state.json`, so create the scene as usual, then call
`await scene.Context.ClearCookiesAsync()` before `OpenAsync("/Login")`. This clears the cookies of both tenants, which
share the host. Do not use the admin session in this clip. The later clip `06-account` gets a fresh context with
`state.json`, so clearing cookies here does not sign the recorder out of it.

- `signin-1`: On screen from `StartCaptureAsync`: the Default tenant login page (`/Login`). Highlight the heading "Use
  another service to log in" (`OrchardCore.Users/Views/Account/Login.cshtml:38`), then the button
  `button[name="provider"][value="OpenIdConnect"]` whose text is "Example Identity" (`:43`). The local form with Username or
  email address, Password and Log in is on the left (`Views/LoginFormCredentials.cshtml:7`). On screen at the end: both
  columns, the provider button highlighted.
- `signin-2`: `ClickAndWaitForNavigationAsync` on `button[name="provider"][value="OpenIdConnect"]`. The browser goes through
  `/OrchardCore.Users/ExternalAuthentications/ExternalLogin` (the form of `Login.cshtml:40`, in the area route of the Users
  module) and `/idp/connect/authorize` to `/idp/Login`. Wait until the URL contains
  `/idp/Login` and `#UserName` is visible. On screen at the end: the sign-in page of the provider tenant, with Username
  or email address, Password and Log in, and the address `/idp/Login`.
- `signin-3`: Type `jamie.chen` in `#UserName` and `<password>` in `#Password`, delay 55, then `ClickAndWaitForNavigationAsync`
  on the submit button "Log in" (`Views/LoginForm.Edit.cshtml:9`). Do not click the eye button next to the password. Wait until the URL
  contains `ExternalLoginCallback` and the heading "Create a new account." is visible (several redirects, about two
  seconds). On screen at the end: the page Create a new account. on the Default tenant. If the redirects take longer
  than the narration, type faster: the click on Log in must happen at least 3 seconds before the narration ends.
- `signin-4`: On the page "Create a new account." (`OrchardCore.Users/Views/ExternalAuthentications/RegisterExternalLogin.cshtml:9`),
  only the Username field is shown, filled with a generated name that starts with `u`, because the email address and
  password fields are hidden by the options saved in `04-registration` (`:16-34`). Select the text of the Username field
  (triple click) and type `jamie.chen`. Highlight the field. On screen at the end: the text "You've successfully
  authenticated with OpenIdConnect. Please fill the form below and click the Register button to complete the
  registration." (`:11`), the Username field holding `jamie.chen`, and the Register button.
- `signin-5`: Click the button "Register" (`:52`) with `ClickAndWaitForNavigationAsync`. Wait until the URL is the site
  root. On screen at the end: the home page of the Blog recipe on the Default tenant.

## Clip 06-account (chapter 06)

Session: `admin` on the Default tenant, from `state.json`, from snapshot S4. Before `StartCaptureAsync`, open `/Admin`
with the admin menu visible.

- `account-1`: Click the menu item Access Control, then Users (`OrchardCore.Users/AdminMenu.cs:27-30`); the last click is
  `ClickAndWaitForNavigationAsync`. The page is `/Admin/Users`. Highlight the row of `jamie.chen`
  (`OrchardCore.Users/Views/UserFields.cshtml:7`). On screen at the end: the Users list with `admin` and `admin@example.com`,
  and `jamie.chen` with `jamie.chen@example.com`, with no Unconfirmed badge, the second row highlighted.
- `account-2`: Keep the highlight and let the cursor rest on the row. Nothing is clicked. On screen at the end: the same
  list.

## Chapter 99: closing card

`outro`: card. Nothing to record.

## Checks for private data

- Every name is fake: `Jamie Chen`, `jamie.chen@example.com`, `admin@example.com`. No other name, email address, phone
  number, token or real domain may appear. The only host on screen is `localhost`.
- The passwords and the client secret appear only as dots in password fields. Do not click the eye buttons. If a
  browser or an extension fills a field, record again.
- Run the OCR check of the skill at one frame per second and at every scene change, and search the text for anything but
  the values above. Pay attention to the frames of `signin-2` and `signin-3`, where several pages flash by.
- No `state.json`, no copy of `App_Data`, and no environment file is committed.

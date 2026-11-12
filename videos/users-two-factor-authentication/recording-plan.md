# Recording plan: Two-Factor Authentication

Everything a person needs to record this video without deciding anything. Labels were read from the Orchard Core
source at commit `92db4a83` (the commit `catalog/features.json` was read from); `file:line` references are relative to
`src/OrchardCore.Modules/` of that checkout unless they start with `src/`. In this plan, a selector stands for
`scene.Page.Locator(...)`. Where a selector is only a description ("the row of ..."), check it once with
`Recorder probe` on the running site before recording; the labels are fixed, the surrounding markup is not.

The video has nine recordings (`02-enable`, `03-settings`, `04-app`, `05-signin`, `06-methods`, `06-email`, `06-sms`,
`07-required`, `08-disable`) and four slides. The recordings do **not** play in the order they are recorded: read
"Recording order" first.

## The site

- **Source**: `src/OrchardCore.Cms.Web` of an OrchardCore checkout at the commit above (or one whose labels match),
  run in the **Development** environment. The Log SMS provider that stands in for an SMS service is registered only
  there (`OrchardCore.Sms/Startup.cs:31-34`).
- **Setup**: Auto Setup on `http://localhost:5010`, recipe **Blog**, site name **Orchard Core**, time zone
  `America/Los_Angeles`, tenant `Default`, user `admin`, email `admin@example.com`, password `<password>` (the value of
  `VIDEO_PASSWORD`, chosen outside git). `ORCHARD_APP_DATA` points at an empty folder outside both repositories.
  See `references/recording.md` of the skill. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so nothing
  leaves the machine.
- The setup creates `admin` with a verified email address (`OrchardCore.Users/Services/SetupEventHandler.cs:28`).
  That is why the Email method shows **Verified** for `admin` from the moment its feature is enabled.
- **Features that must be enabled before recording** (seed, below): **SMTP Email Provider** and **SMS**. Do not enable
  any "Two-Factor ..." feature: `02-enable` does that on camera.
- **Features the recording enables itself**: Two-Factor Authenticator App Method, Two-Factor Email Method, Two-Factor
  SMS Method (and, by dependency, Two-Factor Authentication Services).
- **External services and their local stand-ins**:
  - Email: the SMTP provider with the delivery method **Specified pickup directory**. Each message is written as an
    `.eml` file in `<ORCHARD_APP_DATA>/Sites/Default/Emails` and nothing is sent.
  - SMS: the **Log** provider. Each message is written to the site's console output and log file as
    `A message with the body '...' was set to '+12015550123'.` (`OrchardCore/OrchardCore.Sms.Core/Services/LogSmsProvider.cs:33`).
  - Authenticator app: none. The recorder computes the code itself ("Computing the codes").
- Viewport 1536x864, light color scheme, `en-US`, as in the skill. The admin theme is the default (TheAdmin).

### Seed data (create it outside the capture, once, on the signed-in `admin` session)

1. **Tools > Features**: enable **SMTP Email Provider** and **SMS**.
2. **Settings > Communication > Email**: on the **SMTP** tab tick **Enable SMTP Provider**, set **Sender email address**
   to `noreply@example.com`, **Delivery method** to **Specified pickup directory**, **Pickup directory location** to
   `/`, and save (`OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml`). Do not try to choose the provider first:
   the **Providers** tab lists enabled providers only (`OrchardCore.Email/Drivers/EmailSettingsDisplayDriver.cs:84-98`),
   and saving an enabled SMTP provider sets it as the default one when none is set
   (`OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:193-197`). Open the **Providers** tab afterwards and
   check that **Default provider** shows **Simple Mail Transfer Protocol (SMTP)**
   (`OrchardCore.Email.Smtp/Services/SmtpEmailProvider.cs:20`).
3. **Settings > Communication > SMS**: on the **Providers** tab set **Default SMS provider** to **Log**, and save
   (`OrchardCore.Sms/Views/SmsSettings.Edit.cshtml:6-9`).
4. **Access Control > Users > Add User**: **User name** `jamie`, **Email** `jamie.chen@example.com`, **Password** and its
   confirmation `<password>`, no role. Then, in the **Actions** menu of the row of `jamie`, choose **Confirm email**
   and confirm (`OrchardCore.Users/Views/UserActionsMenu.cshtml:35`). Jamie's email address is now verified, which the
   clip `07-required` depends on.
5. Run `Recorder setup` to save `state.json` for `admin`. Do it now, before any two-factor method exists. Only
   `02-enable` and `04-app` use it (see "Recording order").
6. Stop the site, copy `ORCHARD_APP_DATA` and `state.json` aside as snapshot **S0**, and start the site again.

No other seed is needed: no content, no other users, no phone number, no authenticator key.

## Computing the codes

The recorder types the codes itself. Nothing here is committed: the key stays in a file outside git (for example
`%TEMP%/ocvideo/authenticator-key.txt`), and is deleted whenever the site is restored to a snapshot older than **S2**.

- **Authenticator app code**: the page of `app-2` shows the key in `kbd` (grouped by four letters, in lower case).
  Read it with `scene.Page.Locator("kbd")`, store it, and compute the six-digit code of the current 30-second period
  (RFC 6238, SHA-1). Type it at once, and compute it again for every sign-in. Python version, tested against the
  RFC 6238 vectors:

  ```python
  import base64, hmac, hashlib, struct, sys, time
  def code(key, at=None):
      key = key.replace(" ", "").upper()
      raw = base64.b32decode(key + "=" * (-len(key) % 8))
      counter = struct.pack(">Q", int(at or time.time()) // 30)
      digest = hmac.new(raw, counter, hashlib.sha1).digest()
      offset = digest[-1] & 0x0F
      return f"{(struct.unpack('>I', digest[offset:offset + 4])[0] & 0x7FFFFFFF) % 1_000_000:06d}"
  print(code(sys.argv[1]))
  ```

  The same thing in C#: HMAC-SHA1 of the 8-byte big-endian counter `floor(unixSeconds / 30)`, dynamic truncation, `% 1000000`,
  left-padded with zeros to six digits. If fewer than 3 seconds remain in the period, wait for the next one.
- **Email code**: after the click on **click here**, open the newest `.eml` in `<ORCHARD_APP_DATA>/Sites/Default/Emails`
  and read the six digits after `Your code is`. The subject is `Your verification code`
  (`OrchardCore/OrchardCore.Users.Core/Models/EmailAuthenticatorLoginSettings.cs`). The body can be quoted-printable.
- **SMS code**: after the click on **Request Code**, read the six digits after `Your code is` in the newest
  `A message with the body '...' was set to '+12015550123'.` line of the console output or of the log file in
  `<ORCHARD_APP_DATA>/logs`. The SMS settings are empty, so the text is the default one
  (`OrchardCore.Users/Drivers/SmsAuthenticatorLoginSettingsDisplayDriver.cs:36-39`).
- Type each code as soon as it is read or computed: all three kinds expire after a short time.

## Recording order, snapshots and dependencies

The play order is `02, 03, 04, 05, 06, 07, 08`. Record in this order instead:

| # | Clip | Needs | Changes the site | Snapshot to restore before a new take | Snapshot to save after |
|---|------|-------|------------------|---------------------------------------|------------------------|
| 1 | `02-enable` | S0 | Enables the three method features | S0 | S1 |
| 2 | `04-app` | S1 (`admin` has no method yet, the site does not require two-factor authentication yet) | `admin` gets the authenticator app, recovery codes and two-factor authentication | S1 | S2 |
| 3 | `03-settings` | S2 | Saves **Allow users to remember client** and **Require two-factor authentication** | S2 | S3 |
| 4 | `07-required` | S3 (the site requires two-factor authentication, `jamie` has a verified email and no method) | `jamie` gets two-factor authentication through the Email method | S3 | S4 |
| 5 | `08-disable` | S4 (`jamie` has it enabled) | Turns it off for `jamie` | S4 | S5 |
| 6 | `06-methods` | S5 | Saves the preferred method of `admin` (**Authenticator**) | S5 | S6 |
| 7 | `05-signin` | S6 | Nothing stored | none | none |
| 8 | `06-email` | S6 | Nothing stored (one `.eml` file is written) | none | none |
| 9 | `06-sms` | S6 | Verifies a phone number for `admin` | S6 | S7 |

Why this order:

- **`admin` must have two-factor authentication before the site requires it.** While **Require two-factor authentication**
  is on, a user who has not enabled it gets the claim "required" and the filter of the module redirects every page
  except the two-factor pages to the Two-Factor Authentication page
  (`Services/TwoFactorAuthenticationClaimsProvider.cs:21-30`, `Filters/TwoFactorAuthenticationAuthorizationFilter.cs:60-72`).
  The claim is added not only at sign-in: ASP.NET Core Identity rebuilds the principal of a cookie that is older than the
  validation interval (30 minutes by default; the source of Orchard Core does not change it), and the module keeps its
  claims through that rebuild (`OrchardCore.Users.Core/Services/ConfigureSecurityStampOptions.cs`). `state.json` is
  always older than that. With the requirement on, a session of `admin` taken from `state.json` would land on the
  Two-Factor Authentication page instead of `/Admin`. So `04-app` comes before `03-settings`.
- **`state.json` is used by `02-enable` and `04-app` only**, both recorded while the site does not require two-factor
  authentication. Enabling a method changes the security stamp of `admin`, so the saved cookie is rejected after
  `04-app` anyway.
- **From `03-settings` on, every clip that needs `admin` signed in opens signed out**
  (`scene.Page.Context.ClearCookiesAsync()`, then `OpenAsync("/Login")`) and signs in before `StartCaptureAsync()`:
  `admin`, `<password>`, **Log in**, then the current authenticator code, **Log in**. Leave **Remember this device**
  unticked. Until `06-methods` has saved the preferred method, the first prompt can be the Email one (**Please click
  here to receive a one-time verification code via email.**): click **Sign in another way**, off camera, until **Please
  use your authenticator app to obtain one-time verification code, and enter it below** shows.
- `07-required` comes after `03-settings` because it needs the requirement, and `08-disable` comes after `07-required`
  because it turns off what `07-required` gave `jamie`. A new take of `07-required` needs S3 back: once `jamie` has
  recovery codes, **Enable** opens the Two-Factor Authentication page instead of the Recovery Codes page
  (`Controllers/TwoFactorAuthenticationBaseController.cs:134-150`).
- `06-methods` comes before `05-signin` and `06-email` because **Preferred Two-Factor Method** is what makes the
  sign-in offer the authenticator app first. Without it the first method shown is the first one the site lists, which
  can be Email.
- `06-sms` comes last: once the phone number is verified, **Sign in another way** has a third method, and from the
  authenticator app it would not lead to Email.
- Between clips, the recorder's browser context for `jamie` is separate from the one for `admin`.
- **Take notes of the key** while recording `04-app` (see "Computing the codes"). After a restore to S2 or later the
  stored key is still valid; after a restore to S1 or earlier, delete it.
- **Continuity.** In the play order the site already requires two-factor authentication when `04-app` plays, but the
  clip is recorded before the requirement exists. Nothing narrated depends on it. Keep the card header of the
  Two-Factor Authentication page (it shows **Disable two-factor authentication** instead of **Enabled** at that time)
  out of the last frame of `app-5`, as described there.

## Clips

### `02-enable` (chapter 02, Enable the methods)

Starts on the admin dashboard (`/Admin`), signed in as `admin` with `state.json`, nothing expanded. Site: S0.

#### `enable-1`

- Click **Tools** in the admin menu to expand it, then click **Features** (`ClickAndWaitForNavigation`). The menu does
  not expand anything by itself (`TheAdmin/Views/NavigationItem-admin.cshtml`). If the label sits in a flyout, click it
  there.
- On `/Admin/Features`, type `two-factor` in the search box `#search-box` (`TypeAsync`). The list filters as you type.
- Highlight, one after the other, the rows **Two-Factor Authenticator App Method**, **Two-Factor Email Method** and
  **Two-Factor SMS Method**, as the narration names them.
- Visible when the narration ends: the Features page with the search text `two-factor`, and the three rows of the
  **Security** category, each with its name and its feature id, its dependencies as badges, and a blue **Enable**
  button. **Two-Factor Authentication Services** is not listed: the **Hide on demand** filter hides it by default.
- Source: `OrchardCore.Features/AdminMenu.cs:26-27`; `Views/Admin/Features.cshtml:27,65`;
  `Views/FeatureEntry.Fields.SummaryAdmin.cshtml`; `OrchardCore.Users/Manifest.cs:122-169`.

#### `enable-2`

- Tick the check box of each of the three rows: `input[name='featureIds'][value='OrchardCore.Users.2FA.AuthenticatorApp']`,
  `input[name='featureIds'][value='OrchardCore.Users.2FA.Email']` and
  `input[name='featureIds'][value='OrchardCore.Users.2FA.Sms']`.
- Click **Bulk Actions** (`#bulk-action-menu-button`), then **Enable** in the menu (`a[data-action='Enable']`)
  (`ClickAndWaitForNavigation`). The page reloads.
- Wait until the green notification `The following features were enabled: ...` is visible.
- Visible when the narration ends: the reloaded Features page, with the notification at the top. It lists the three
  selected features only (`The following features were enabled: ...`): the controller passes the selected features to the
  notification, not the dependencies that were enabled with them (`FeatureService.cs:74-81`). **Two-Factor Authentication
  Services** is not named there; its row appears when the **Hide on demand** filter is cleared, and the narration does not
  depend on it.
- Source: `Views/Admin/Features.cshtml:34-38`; `Views/FeatureEntry.Checkbox.SummaryAdmin.cshtml:11`;
  `Controllers/AdminController.cs:160-180,312-318`. The bulk form posts `Force=true`, which enables the dependencies.

#### `enable-3`

- The search box is empty after the reload. Type `sms`: the row **SMS** (its button is a red **Disable**, so it is
  enabled) and the row **Two-Factor SMS Method** are among the matches. Highlight **SMS**.
- Clear the box (`Escape`), type `smtp`, and highlight the row **SMTP Email Provider** (red **Disable** button).
- Visible when the narration ends: the Features page filtered on `smtp`, with **SMTP Email Provider** enabled.
- Source: `OrchardCore.Sms/Manifest.cs:9-14`; `OrchardCore.Email.Smtp/Manifest.cs`; `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml`.

Save snapshot **S1** after this clip.

### `03-settings` (chapter 03, Site settings)

Recorded third, after `04-app`. Starts signed out: before `StartCaptureAsync()`, sign in as `admin` (`<password>`, then
the current authenticator code), then open `/Admin`. Site: S2. `admin` already has two-factor authentication, so the
requirement that this clip saves does not lock it out (see "Recording order").

#### `settings-1`

- Click **Settings**, then **Security**, then **User Login** in the admin menu (`ClickAndWaitForNavigation` on the last
  one). Settings are saved per group; this is the group `userLogin`.
- The page opens on the **General** tab. Click the tab **Two-Factor Authentication**.
- Highlight the legend **Two-Factor Settings**.
- Visible when the narration ends: the tab **Two-Factor Authentication** selected, the legend **Two-Factor Settings**
  at the top, and under it the options **Allow users to remember client**, **Require two-factor authentication**,
  **Number of recovery codes to generate** (value 5) and **Use site theme for two-factor authentication pages**, all
  unticked, followed by **Require two-factor authentication specific roles** (unticked; it belongs to this first group
  and is shown because the Roles feature is enabled). The groups below (email, authenticator app, SMS) must be **out
  of the frame**: do not scroll.
- Keep the whole step shorter than the narration (about 9 seconds): do not wait between the menu clicks.
- Source: `OrchardCore.Users/AdminMenu.cs:40-47`; `Drivers/LoginSettingsDisplayDriver.cs:40`;
  `Drivers/TwoFactorLoginSettingsDisplayDriver.cs:41`; `Views/TwoFactorLoginSettings.Edit.cshtml:13,22,35,42,55`;
  `Drivers/RoleLoginSettingsDisplayDriver.cs:35-53` (position 6 puts the roles option right after the position 5 group).

#### `settings-2`

- Click the check box **Allow users to remember client** (`scene.Page.GetByLabel("Allow users to remember client")`).
- Highlight the field **Number of recovery codes to generate** (leave the value 5).
- Visible when the narration ends: **Allow users to remember client** ticked, with its hint about how long the client
  is remembered, and the field with 5.
- Source: `Views/TwoFactorLoginSettings.Edit.cshtml:22-24,42-46`.

#### `settings-3`

The narration starts with the roles because that rule holds while **Require two-factor authentication** is cleared, as it
is here.

- Click the check box **Require two-factor authentication specific roles**
  (`GetByLabel("Require two-factor authentication specific roles")`). The hint **Select the roles to require
  two-factor authentication for.** and one check box per assignable role open under it (for example Administrator,
  Author, Contributor, Editor, Moderator).
- Tick **Administrator**. Highlight the list.
- Visible when the narration ends: the roles list open, with **Administrator** ticked, and **Require two-factor
  authentication** still cleared.
- Source: `Views/LoginSettingsRoles.Edit.cshtml:8,15-24`; `Drivers/RoleLoginSettingsDisplayDriver.cs:35-53`;
  `Roles/Handlers/RoleTwoFactorAuthenticationHandler.cs` (a user in a selected role must use it). This group is shown only
  because the Roles feature is enabled (the Blog recipe enables it).

#### `settings-4`

- Click the check box **Require two-factor authentication** (`GetByLabel("Require two-factor authentication", new() { Exact = true })`).
- Highlight it with its hint.
- Visible when the narration ends: **Require two-factor authentication** ticked, with the roles list still open below.
- Source: `Views/TwoFactorLoginSettings.Edit.cshtml:35-36`; `OrchardCore.Users.Core/Services/DefaultTwoFactorAuthenticationHandler.cs`
  (the handler returns the option for every user) and `DefaultTwoFactorAuthenticationHandlerCoordinator.cs` (the requirement
  holds when any handler says so, so the roles option adds nothing once this option is on);
  `Filters/TwoFactorAuthenticationAuthorizationFilter.cs:60-72`.

#### `settings-5`

- Click **Require two-factor authentication specific roles** again to clear it (the list collapses).
- Click **Save** (`button.save`, in the bar at the bottom) (`ClickAndWaitForNavigation`).
- Wait for the green notification **Site settings updated successfully.** and for the tab **Two-Factor Authentication**.
  If the page returns on the **General** tab, click **Two-Factor Authentication** again.
- Visible when the narration ends: the notification, **Allow users to remember client** and **Require two-factor
  authentication** ticked, **Require two-factor authentication specific roles** cleared.
- Source: `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; `OrchardCore.Settings/Controllers/AdminController.cs:119`;
  `Drivers/RoleLoginSettingsDisplayDriver.cs:56-90`.

Save snapshot **S3** after this clip. The site now requires two-factor authentication. `admin` has it already, so
nothing changes for `admin`; a user who has not enabled it gets the claim "required" at sign-in
(`Services/TwoFactorAuthenticationClaimsProvider.cs:21-30`).

### `03-tab` (slide, chapter 03)

#### `tab-1`

A slide, nothing to record. It lists the four groups of the tab and what each holds, so the two text areas with the
message texts are never on screen.

### `07-required` (chapter 07, A required second factor)

Recorded fourth. Starts signed out, on `/Login`, in a **new browser context without `state.json`**. Site: S3. This is the
only clip that signs in as `jamie`.

#### `required-1`

- Type `jamie` in **Username or email address** (`#UserName`) and `<password>` in **Password** (`#Password`)
  (`TypeAsync`). Do not click the eye button of the password field. Click **Log in** (`ClickAndWaitForNavigation`).
- The site redirects to the Two-Factor Authentication page by itself (the filter of the module sends a user who must
  enable two-factor authentication there from any page). Wait for it.
- Highlight the red message **Two-factor authentication must be enabled before proceeding.**
- Visible when the narration ends: the page **Two-Factor Authentication**, outside the admin layout, with that red
  message at the top; the card header with a green **Enable** button; the warning **Two-factor authentication is
  disabled.**; and under **Two-factor methods** the rows **Authenticator app** (**Add**), **Email authentication**
  (**Verified**) and **SMS authentication** (**Verify Phone Number**).
- Source: `Views/LoginFormCredentials.cshtml:7,13`; `Views/LoginForm.Edit.cshtml:9`; `Filters/TwoFactorAuthenticationAuthorizationFilter.cs`;
  `Views/TwoFactorAuthentication/Index.cshtml:26-29,36,46-50,66-69`; `Views/Items/TwoFactorMethod.*.Actions.cshtml`.

#### `required-2`

- Click the green **Enable** button (`ClickAndWaitForNavigation`). The site enables two-factor authentication, generates
  the recovery codes and opens the **Recovery Codes** page.
- Do **not** reload this page: the codes are kept for five seconds only.
- Visible when the narration ends: the **Recovery Codes** page (breadcrumb **Two-Factor Authentication / Recovery
  Codes**), the notifications **Two-factor authentication has been enabled.** and **New recovery codes were generated.**,
  the warning **Store these codes in a safe place.**, and the list of recovery codes (fake, from a throwaway site).
- Source: `Controllers/TwoFactorAuthenticationController.cs:351-374`; `Controllers/TwoFactorAuthenticationBaseController.cs:54-70,134-150`;
  `Views/TwoFactorAuthentication/ShowRecoveryCodes.cshtml:9-21`.

#### `required-3`

- Click the breadcrumb **Two-Factor Authentication** (`ClickAndWaitForNavigation`).
- Highlight the dimmed green **Enabled** badge in the card header.
- Visible when the narration ends: the Two-Factor Authentication page with **Enabled** in place of a button, the blue
  message **Two-factor authentication can't be disabled for your user.**, the rows **Email authentication**
  (**Verified**) and the others, and the **Recovery options** header.
- Source: `Views/TwoFactorAuthentication/Index.cshtml:42-45,70-76,131`. The red message of `required-1` is gone because
  enabling two-factor authentication refreshes the sign-in of a user who must use it
  (`Controllers/TwoFactorAuthenticationBaseController.cs:106-132`).

Save snapshot **S4** after this clip.

### `08-disable` (chapter 08, Disable it for a user)

Recorded fifth. Starts signed out: before `StartCaptureAsync()`, sign in as `admin` (`<password>`, then the current
authenticator code), then open `/Admin`. Site: S4 (`jamie` has two-factor authentication, from `07-required`).

#### `disable-1`

- Click **Access Control** to expand it, then **Users** (`ClickAndWaitForNavigation`).
- Move the cursor over the row of `jamie` (`MoveToAsync`); highlight the row.
- Visible when the narration ends: the **Users** list with the rows `admin` and `jamie`, each with **Edit** and
  **View** buttons (the row of `admin` has no **View**) and an **Actions** button.
- Source: `OrchardCore.Users/AdminMenu.cs:27-37`; `Views/UserButtons.cshtml`; `Views/UsersAdminListCreate.cshtml:7`.

#### `disable-2`

- In the row of `jamie`, click **Actions** (the button with the `actions` class; the layout of the row actions is
  **Buttons** by default, with an **Actions** drop-down: `OrchardCore.Admin/Views/AdminListActions.cshtml:30-32`). Among
  other items, the menu lists **Change password**, **Disable**, **Delete** and, after them, **Disable two-factor
  authentication** (`UserActionsMenu` is placed at `ActionsMenu:5`, `UserTwoFactorActionsMenu` at `ActionsMenu:10`:
  `Drivers/UserDisplayDriver.cs:39`, `Drivers/UserTwoFactorDisplayDriver.cs:14`). The item is listed because `admin`
  holds the permission and `jamie` has two-factor authentication enabled.
- Click **Disable two-factor authentication** (not **Disable**, which disables the user). A dialog opens with the title
  **Disable two-factor authentication** and the message **Are you sure you want to disable two-factor authentication for
  this user?**. Click **Ok** (`#modalOkButton`) (`WaitForNavigationAsync`). The Users list loads again.
- Click **Actions** in the row of `jamie` once more, to show that the item is gone, and highlight the menu.
- Visible when the narration ends: the Users list with the menu of `jamie` open and no **Disable two-factor
  authentication** item in it.
- Source: `OrchardCore.Users/Views/UserTwoFactorActionsMenu.cshtml:9-12`; `Controllers/AdminTwoFactorAuthenticationController.cs:25-47`;
  `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`; `src/OrchardCore/OrchardCore.Users.Core/UsersPermissions.cs:29`
  (the permission is **Disable two-factor authentication for any user**; the Administrator role has it,
  `Services/TwoFactorPermissionProvider.cs:17-23`).

Save snapshot **S5** after this clip.

### `04-app` (chapter 04, Set up an authenticator app)

Recorded second. Starts on the admin dashboard (`/Admin`), signed in as `admin` with `state.json`. Site: S1. The saved
cookie is older than the validation interval, so the site rebuilds the principal of `admin` at the first request; the
site does not require two-factor authentication yet, so the principal gets no "required" claim and `/Admin` opens
normally. This clip stores the authenticator key.

#### `app-1`

- Click the user menu (`#navbarDropdown`, the user icon at the top right of the admin), then **Security**
  (`ClickAndWaitForNavigation`).
- The Two-Factor Authentication page opens, outside the admin layout. Highlight the three rows under **Two-factor
  methods**.
- Visible when the narration ends: the card **Two-Factor Authentication** with a green **Enable** button, the warning
  **Two-factor authentication is disabled.**, and the rows **Authenticator app** (**Add**), **Email authentication**
  (**Verified**) and **SMS authentication** (**Verify Phone Number**). No red message: the site does not require
  two-factor authentication at this point of the recording.
- Source: `OrchardCore.Users/Views/UserMenu.DetailAdmin.cshtml:7`; `Views/UserMenuItems-TwoFactor.cshtml:3`;
  `Views/TwoFactorAuthentication/Index.cshtml:36,66-69,118`; `Views/Items/TwoFactorMethod.Authenticator.*.cshtml`.

#### `app-2`

- Click **Add** in the row **Authenticator app** (`ClickAndWaitForNavigation`). The page **Configure Authenticator App**
  opens (breadcrumb **Two-Factor Authentication / Configure Authenticator App**).
- Wait until the QR code is drawn (`#qrCode canvas` or `#qrCode img`).
- Read the key from `kbd` (outside the narration) and store it (see "Computing the codes").
- Highlight the QR code, then the key.
- Visible when the narration ends: the numbered instructions, with the links **Android** and **iOS** of Microsoft
  Authenticator and Google Authenticator, the QR code, the key in `kbd`, and the **Verification code** field with the
  **Verify** button below (scroll by 300 pixels at most if they are below the fold, and clear highlights first).
- Source: `Views/AuthenticatorApp/Index.cshtml:17-49`; `Controllers/AuthenticatorAppController.cs:60-76,148-176`.

#### `app-3`

- Compute the current code from the stored key. Type it in **Verification code** (`#Code`) (`TypeAsync`). Highlight the
  **Verify** button. **Do not click it yet.**
- Visible when the narration ends: the six digits in the field, and the highlighted **Verify** button.
- Source: `Views/AuthenticatorApp/Index.cshtml:45-49`.

#### `app-4`

- Click **Verify** (`ClickAndWaitForNavigation`). The site enables two-factor authentication and opens **Recovery
  Codes**. Do **not** reload this page: the codes are kept for five seconds only
  (`Controllers/TwoFactorAuthenticationBaseController.cs:68`).
- Highlight the warning **Store these codes in a safe place.**
- Visible when the narration ends: the **Recovery Codes** page with the notifications **Your authenticator app has been
  verified.** and **New recovery codes were generated.**, the warning, and the list of codes (the number is the value
  of **Number of recovery codes to generate**, 5).
- Source: `Controllers/AuthenticatorAppController.cs:78-107`; `Controllers/TwoFactorAuthenticationBaseController.cs:134-150`;
  `Views/TwoFactorAuthentication/ShowRecoveryCodes.cshtml:9-26`.

#### `app-5`

- Click the breadcrumb **Two-Factor Authentication** (`ClickAndWaitForNavigation`).
- Scroll down until the header **Recovery options** is in view (`ScrollAsync(0, 400)`), then highlight the button **Reset
  codes**. Do not click it.
- Visible when the narration ends: **Recovery options**, **Recovery codes** with the description and the yellow **Reset
  codes** button. Above it, scrolled out of view: the rows **Authenticator app** (**Set**, **Reset**), **Email
  authentication** (**Verified**), **SMS authentication** (**Verify Phone Number**), and the **Preferred Two-Factor
  Method** list with the option **Select the method** shown. The card header at the very top is also out of the frame.
  At this point of the recording it holds the warning button **Disable two-factor authentication** and the message
  **Two-factor authentication is enabled.**, because the site does not require two-factor authentication yet (the
  finished video plays this clip after chapter 03). If the scroll leaves any of them in the frame, scroll further
  (`ScrollAsync(0, 500)` or more) until only **Recovery options** and **Reset codes** remain.
- Source: `Views/TwoFactorAuthentication/Index.cshtml:42-45,70-76,96-112,129-168`;
  `Views/Items/TwoFactorMethod.Authenticator.Actions.cshtml:7-8`.

Save snapshot **S2** after this clip.

### `06-methods` (chapter 06, Email and SMS codes)

Recorded sixth. Before `StartCaptureAsync()`: sign in as `admin` (`<password>`, then the current authenticator code; do
not use `state.json`), then open `/TwoFactor`. Site: S5. This clip saves a preference for `admin`. The site requires
two-factor authentication here, so the card header shows **Enabled** and the message **Two-factor authentication can't
be disabled for your user.**

#### `methods-1`

- Highlight the row **Email authentication** and its **Verified** badge.
- The second sentence of the narration (**Verify Email**) describes a user whose email address is not verified yet;
  it is not on screen, and there is nothing to do for it.
- Visible when the narration ends: the **Email authentication** row with **Verified** highlighted, and the row **SMS
  authentication** with **Verify Phone Number**.
- Source: `Views/Items/TwoFactorMethod.Email.Actions.cshtml:8-18`; `Views/Items/TwoFactorMethod.Email.Content.cshtml`.

#### `methods-2`

- Select the option `Authenticator` in **Preferred Two-Factor Method** (`SelectAsync` on the `select` of
  `PreferredProvider`; the options are the names of the methods, **Authenticator** and **Email**). Click **Update**
  (`ClickAndWaitForNavigation`).
- Wait for the notification **Preferences were updated successfully.**
- Visible when the narration ends: the page with that notification and **Authenticator** selected in the list.
- Source: `Views/TwoFactorAuthentication/Index.cshtml:96-112`; `Controllers/TwoFactorAuthenticationController.cs:234-266,
  440-470`.

Save snapshot **S6** after this clip.

### `05-signin` (chapter 05, Sign in with a code)

Recorded after `06-methods`. Starts signed out on `/Login` (log off first, outside the capture, then
`OpenAsync("/Login")`). Site: S6. Nothing is stored.

#### `signin-1`

- Type `admin` in **Username or email address** (`#UserName`) and `<password>` in **Password** (`#Password`)
  (`TypeAsync`). Click **Log in** (`ClickAndWaitForNavigation`). The page asks for the second step.
- Visible when the narration ends: the page with the heading **Your login is protected with a second authentication
  method.**, the text **Please use your authenticator app to obtain one-time verification code, and enter it below**,
  the **Verification code** field, the check box **Remember this device** and the **Log in** button.
- Source: `Views/LoginFormCredentials.cshtml:7,13`; `Controllers/AccountController.cs:178-188`;
  `Views/TwoFactorAuthentication/LoginWithTwoFactorAuthentication.cshtml:19,43-57`; `Views/AuthenticatorAppValidation.cshtml:1`.

#### `signin-2`

- Highlight, one after the other, the check box **Remember this device** (do **not** tick it: a remembered browser
  would skip the code in the later clips), the link **Sign in another way** (in **Having trouble? Sign in another
  way.**), and the link **log in with a recovery code** (in **Don't have access to your authenticator device? You
  can log in with a recovery code.**). Do not click them.
- **Remember this device** is there because `03-settings` ticked **Allow users to remember client**. **Sign in another
  way** is there because `admin` has two methods (the authenticator app and Email).
- Visible when the narration ends: the same page with the last highlight on the recovery code link.
- Source: `Views/TwoFactorAuthentication/LoginWithTwoFactorAuthentication.cshtml:52,66-84`;
  `Controllers/TwoFactorAuthenticationController.cs:60-91`.

#### `signin-3`

- Compute the current code, type it in **Verification code** (`#VerificationCode`), and click **Log in** (the large
  button of the form) (`ClickAndWaitForNavigation`).
- Visible when the narration ends: the page the site opens after a sign-in (the home page of the site, or the admin).
- Source: `Controllers/TwoFactorAuthenticationController.cs:97-152`.

### `06-email` (chapter 06, Email and SMS codes)

Recorded after `05-signin`. Before `StartCaptureAsync()`: signed out on `/Login`, type `admin`, `<password>`, click **Log
in**, and wait for the second-step page (the authenticator app prompt). Site: S6. Nothing is stored; one `.eml` file is
written.

#### `email-1`

- Click the link **Sign in another way** (`ClickAndWaitForNavigation`).
- Visible when the narration ends: the second-step page for the Email method: **Please click here to receive a one-time
  verification code via email.** above the **Verification code** field.
- Source: `Views/TwoFactorAuthentication/LoginWithTwoFactorAuthentication.cshtml:72`; `Views/EmailAuthenticatorValidation.cshtml:4`;
  `Controllers/TwoFactorAuthenticationController.cs:440-470`.

#### `email-2`

- Click **click here** (`#RequestCode`). The page requests the code without reloading.
- Wait until the green message is visible in `#RequestCodeFeedback`: **A verification code has been sent via email.
  Please check your email for the code.**
- Visible when the narration ends: that green message under the prompt.
- Source: `Views/EmailAuthenticatorValidation.cshtml:4-6`; `Endpoints/EmailAuthenticator/SendCode.cs:29-69`.

#### `email-3`

- Read the code from the newest `.eml` file (outside the narration, `OffCameraAsync` if it takes more than a
  second), type it in **Verification code** (`#VerificationCode`), and click **Log in** (`ClickAndWaitForNavigation`).
- Visible when the narration ends: the page the site opens after a sign-in.
- Source: `Controllers/TwoFactorAuthenticationController.cs:97-152`.

### `06-sms` (chapter 06, Email and SMS codes)

Recorded last. Before `StartCaptureAsync()`: sign in as `admin` (`<password>`, then the current authenticator code),
then open `/TwoFactor`. Site: S6. This clip verifies a phone number for `admin`.

#### `sms-1`

- Click **Verify Phone Number** in the row **SMS authentication** (`ClickAndWaitForNavigation`). The page **Configure
  SMS Authenticator** opens.
- Type `+12015550123` in **Phone Number** (`#PhoneNumber`). Click **Request Code** (`ClickAndWaitForNavigation`).
  If the page reports **Invalid phone number used.**, use another valid number with the `+1` country code.
- Visible when the narration ends: the page **SMS Code Validation** (breadcrumb **Two-Factor Authentication /
  Configure SMS Authenticator / SMS Code Validation**), the notification **We have successfully sent an verification
  code to your phone number. Please retrieve the code from your device.**, and the **Verification code** field.
- Keep the actions short: the narration is about 9 seconds.
- Source: `Views/Items/TwoFactorMethod.Phone.Actions.cshtml:8`; `Views/SmsAuthenticator/Index.cshtml:12-30`;
  `Views/SmsAuthenticator/ValidateCode.cshtml:13-30`; `Controllers/SmsAuthenticatorController.cs:112-128`.

#### `sms-2`

- Read the code from the console output or the log (outside the narration), type it in **Verification code**
  (`#Code`), and click **Verify** (`ClickAndWaitForNavigation`).
- Wait for the notification **Your phone number has been confirmed.** on the Two-Factor Authentication page.
- Highlight the **Verified** badge of the row **SMS authentication**.
- Visible when the narration ends: the Two-Factor Authentication page with that notification and **Verified** on the
  row **SMS authentication** (an **Edit** button follows the badge, because **Allow user to change their phone number** is on by default: `OrchardCore.Users.Core/Models/LoginSettings.cs:24`).
- Source: `Controllers/SmsAuthenticatorController.cs:160-187`; `Views/Items/TwoFactorMethod.Phone.Actions.cshtml:10-17`.

Save snapshot **S7** after this clip.

### Slides

- `how-1`: the slide "Two steps to sign in": the cards **Username and password** and **A one-time code**.
- `how-2`: the slide "Three methods": the cards **Authenticator app**, **Email** and **SMS**.
- `how-3`: the slide "Who does what": the cards **Administrator** and **Each user**.
- `tab-1`: the slide "The rest of the Two-Factor Authentication tab": the four groups of the tab and what each holds.

## Checks for private data

- Only fake data is on screen: the users `admin` and `jamie`, the addresses `admin@example.com`,
  `jamie.chen@example.com` and `noreply@example.com`, and the phone number `+12015550123`. OCR the finished video for any
  other name, address or number.
- The password is never typed visibly: the password field shows dots, and the eye button is never clicked.
- The authenticator key, the QR code and the recovery codes belong to the throwaway site. The key is kept in a file
  outside the repository and deleted after the take.
- The email and SMS bodies are not shown. The two text areas that hold their default texts (**Email Body**, **Message
  Body**) must stay out of the frame in `settings-1` to `settings-5`, because their default texts hold a template
  placeholder with braces. Look at the contact sheets: if the braces of a default text are visible at the end of a step,
  do not use the take, and raise it with the maintainer (it is a question about admin videos, not about this plan).
- The address bar is not part of the capture. The page title and `localhost:5010` must not show a machine name.
- Look at every contact sheet from `review.py` before assembling, for error messages (a red **Invalid verification
  code.** means a code expired between reading and typing) and for the recovery codes page, which must show the codes.

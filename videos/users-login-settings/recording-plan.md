# Recording plan: User Login Settings

How to record `users-login-settings` without deciding anything. Read the whole plan once, then record the clips in the
order of "Order of the clips". In this plan a selector stands for `scene.Page.Locator(...)`, a label for
`scene.Page.GetByLabel(...)`, and a text for `scene.Page.GetByText(...)`. Every label below is written as the code spells
it, with the file and line of the Orchard Core source where it comes from (paths are relative to
`src/OrchardCore.Modules/OrchardCore.Users` unless they start with `src/`). The source is the commit named in
`catalog/SOURCE.md`.

The video is an **admin** video: it shows the admin and the login page, never code. Use the browser only.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of a checkout at the commit of `catalog/SOURCE.md`, run on
  `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- **Auto Setup** (the host registers it, `src/OrchardCore.Cms.Web/Program.cs:9`), through environment variables, so no setup form is recorded:

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

  `VIDEO_PASSWORD` is chosen outside git and also passed to the recorder (`<password>` below). Never write it in a file
  of the repository.
- **Recipe**: Blog. It enables the Users feature, the admin theme `TheAdmin` and the site theme `TheBlogTheme`
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, steps `feature` and `themes`).
- **Features**: only the ones the Blog recipe enables. Do **not** enable Two-Factor Authentication Services, Two-Factor
  Authenticator App Method, Two-Factor Email Method, Two-Factor SMS Method, External Authentication, Users Registration,
  Users Reset Password, Users Change Email or Rate Limits. Each of them adds a tab to the User Login page, a link to the
  login page, or a limit, and the narration describes a site without them. Check `/Admin/Features` before taking the
  snapshot.
- **Settings**: leave the User Login settings as the setup made them (nothing saved). Leave the lockout options of
  ASP.NET Core Identity unset, so the site uses their defaults.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (for example `http://127.0.0.1:9`), so the
  site never calls an external service. The video needs no external service and no local stand-in.
- **Browser**: the recorder's Chromium (1536x864 CSS pixels, light, `en-US`), signed in as `admin` through `state.json`
  (`Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`). Each scene opens in its own browser
  context with the saved sign-in, so every clip is signed in as `admin`. The login page does not show who is signed in
  (`src/OrchardCore.Themes/TheAdmin/Views/Layout-Login.cshtml`, and `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`), so the clips that
  show `/Login` need no extra sign-out.

## Seed data

Create it before the snapshot, outside the capture. One user, with invented values:

| Field (label on the Add User page) | Value |
| --- | --- |
| User name | `jamie.chen` |
| Email | `jamie.chen@example.com` |
| Password, and the second password field (its label **Password Confirmation** is hidden, its placeholder reads "Password confirmation") | `<password>` in both, the value of `VIDEO_PASSWORD` (the same as the password of `admin`) |
| Roles | none ticked |

Steps: sign in as `admin`, open **Access Control** > **Users** (`/Admin/Users/Index`), choose **Add User**
(`Views/UsersAdminListCreate.cshtml:7`), fill the fields above (`Views/UserFields.Edit.cshtml:12`, `:23`, `:27`; the
role list, `Views/UserRoleFields.Edit.cshtml:6`, stays unticked), choose **Save** (`Views/UserSaveButtons.Edit.cshtml:2`).
The notification **User created successfully.** appears (`Controllers/AdminController.cs:320`).

Then confirm the email address of `jamie.chen`. A user created on the Add User page is not confirmed
(`Controllers/AdminController.cs:297-299` creates `new User()`, whose `EmailConfirmed` is false; only the setup user and
registered users are confirmed, `Services/SetupEventHandler.cs:28`, `UserService.cs:365`), so the Users list would show a
yellow **Unconfirmed** badge next to **Locked** in `07-lockout`. In the row of `jamie.chen` open **Actions**, choose
**Confirm email** (`Views/UserActionsMenu.cshtml:35`), and choose **Ok** in the dialog. The notification **The email for
jamie.chen has been successfully confirmed.** appears (`Controllers/AdminController.cs:453`). The Users list then shows
`admin` and `jamie.chen`, and neither has a **Locked** or an **Unconfirmed** badge.

`jamie.chen` is created through the normal path (`UserService.CreateUserAsync`, which calls the user manager), so the
account can be locked out. If the **Locked** badge never appears in clip `07-lockout`, see `OPEN-QUESTIONS.md`.

**Snapshot S0**: after the seed, stop the site and copy the `App_Data` folder and `state.json` aside. S0 is the state
every clip starts from.

## Order of the clips

All clips start from S0, so none depends on another. Record them in this order, which is the order they play in:

1. `02-open` changes nothing.
2. `03-remember` changes the site (saves two settings). Restore S0 before and after.
3. `04-theme` changes the site (saves one setting). Restore S0 before and after.
4. `05-local` changes nothing. It must not save anything.
5. `06-details` changes the site (saves two settings). Restore S0 before and after.
6. `07-lockout` changes the site (locks and unlocks `jamie.chen`). It leaves the account unlocked, but a take that stops
   half way leaves it locked: restore S0 before every new take.

To restore S0: stop the site (by its process ID, never by image name), replace `App_Data` and `state.json` with the copy,
start the site, and wait until `http://localhost:5010/Login` answers.

## Locators used by several clips

| What | Locator | Source |
| --- | --- | --- |
| Username field of the login page | `#LoginForm_UserName` (the recorder's own sign-in uses it, `.agents/skills/orchardcore-video-producer/scripts/recorder/Program.cs:63-64`) | `Views/LoginFormCredentials.cshtml:7` |
| Password field | `#LoginForm_Password` | `Views/LoginFormCredentials.cshtml:13` |
| Remember me checkbox | `#LoginForm_RememberMe` (confirm with `Recorder probe /Login`) | `Views/LoginFormCredentials.cshtml:26` |
| Log in button | role Button, name `Log in` (exact) | `Views/LoginForm.Edit.cshtml:9` |
| A settings checkbox | label with the exact text of the checkbox | `Views/LoginSettings.Edit.cshtml` |
| Save button of a settings page | `button.save` | `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` |
| Notification after Save | text `Site settings updated successfully.` | `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119` |
| Admin menu | `#left-nav` | `src/OrchardCore.Themes/TheAdmin/Views/Navigation-admin.cshtml` |
| Row of a user in the Users list | `.list-group-item` or a table row, filtered by the text `jamie.chen` | `Views/UsersAdminList.cshtml` |

The User Login page is `/Admin/Settings/userLogin` (`src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:50`
and `Drivers/LoginSettingsDisplayDriver.cs:13`). Open it by URL when a step says "open the User Login page", except in
`open-2`, where the menu is the story.

In Git Bash, set `MSYS_NO_PATHCONV=1` before `Recorder probe /Admin/Settings/userLogin ...`.

## Clips

### `02-open` (chapter 02): the login page, and the way to the settings

Opens on `/Login` (`OpenAsync`, before the capture). The page shows the heading **Log in**, the fields, **Remember me**
unticked and the **Log in** button. Do not type anything.

- `open-1`: move the cursor over the **Username or email address** field, then the **Password** field, then the
  **Remember me** checkbox, then the **Log in** button, with a highlight on each as the cursor reaches it (clear each
  highlight before the next). Never click and never type. Ends with the login page as it opened, **Remember me**
  unticked, no error, empty fields. Labels: `Views/LoginFormCredentials.cshtml:7`, `:13`, `:26`;
  `Views/LoginForm.Edit.cshtml:9`; heading `Views/Account/Login.cshtml:27`.
- `open-2`: `NavigateAsync("/Admin")`. In `#left-nav` click **Settings**, then **Security**, then the link **User Login**
  with `ClickAndWaitForNavigationAsync` (its address ends with `/Admin/Settings/userLogin`). In a fresh browser context
  nothing is expanded (`src/OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`), so the three clicks are visible. Keep the actions under 9 seconds
  (the narration is about 9 seconds long). Ends on the User Login page, with **Settings** and **Security** expanded in the
  menu. Labels and path: `AdminMenu.cs:40-47` (**Settings**, **Security**, **User Login**).
- `open-3`: on the User Login page, move the cursor to the **Allow user to be remembered during login** checkbox and
  highlight it, then move to the **Allow user to change their phone number** checkbox and highlight it (clear the first
  highlight before the second). Do not click. Ends with the whole list of checkboxes visible without scrolling, and
  exactly these two ticked: **Allow user to be remembered during login** and **Allow user to change their phone number**;
  the other five (**Use persistent authentication cookies by default**, **Allow user to change their username**,
  **Allow user to change their email address**, **Use site theme for login page**, **Disable local password login.**) are
  unticked. There is no tab strip: with only the Users feature, the page has a single group
  (`src/OrchardCore/OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:72`). Labels:
  `Views/LoginSettings.Edit.cshtml:15`, `:52`; defaults: `src/OrchardCore/OrchardCore.Users.Core/Models/LoginSettings.cs:15`
  (`AllowRememberMe`), `:24` (`AllowChangingPhoneNumber`).

### `03-remember` (chapter 03): Remember me

Opens on `/Admin/Settings/userLogin` (`OpenAsync`). Restore S0 before the take.

- `remember-1`: move the cursor to the checkbox **Allow user to be remembered during login** and highlight it. Do not
  click. Ends with that checkbox highlighted and ticked. Labels: `Views/LoginSettings.Edit.cshtml:15`. Source of the
  narration: `Views/LoginFormCredentials.cshtml:21` (the **Remember me** checkbox is rendered only when `AllowRememberMe`
  is true) and `Controllers/AccountController.cs:143-145` (the checkbox value is the `rememberMe` argument of the sign-in,
  which makes the authentication cookie persistent).
- `remember-2`: clear the highlight. Move the cursor to the checkbox **Use persistent authentication cookies by default**
  and highlight it together with its hint ("When Remember me is allowed, this is the default checkbox value. ..."),
  clear the highlight, click the checkbox (it becomes ticked), then `ClickAndWaitForNavigationAsync` on `button.save`.
  Keep the actions under 7 seconds. Ends on the User Login page with the notification **Site settings updated
  successfully.** visible and both **Allow user to be remembered during login** and
  **Use persistent authentication cookies by default** ticked. Labels: `Views/LoginSettings.Edit.cshtml:24`, `:25`;
  `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (**Save**);
  `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119` (the notification).
- `remember-3`: `NavigateAsync("/Login")`, then move the cursor to the **Remember me** checkbox and highlight it. Ends with
  **Remember me** ticked, fields empty, no error. Source of the behavior: `Controllers/AccountController.cs:99`
  (`RememberMe = loginSettings.UsePersistentAuthenticationCookie`).
- `remember-4`: clear the highlight. `NavigateAsync("/Admin/Settings/userLogin")`, click the checkbox
  **Allow user to be remembered during login** (it becomes unticked), `ClickAndWaitForNavigationAsync` on `button.save`,
  then `NavigateAsync("/Login")`. Keep the actions under 7 seconds. Ends on the login page with **no Remember me
  checkbox** between the **Password** field and the **Log in** button. Source: `Views/LoginFormCredentials.cshtml:21`
  (the checkbox is rendered only when `AllowRememberMe` is true).
- `remember-5`: `NavigateAsync("/Admin/Settings/userLogin")`, then move the cursor to the checkbox
  **Use persistent authentication cookies by default** and highlight it together with its hint. Do not click and do not
  choose **Save**. Keep the actions under 6 seconds. Ends on the User Login page with **Allow user to be remembered during
  login** unticked, and **Use persistent authentication cookies by default** ticked and highlighted with its hint ("When
  Remember me is allowed, this is the default checkbox value. When Remember me is disabled, this controls whether login
  cookies are persistent."). Source: `Views/LoginSettings.Edit.cshtml:25` (the hint), `Controllers/AccountController.cs:143-145`
  and `Controllers/ExternalAuthenticationsController.cs:627-629` (the persistent cookie setting decides when Remember me is
  not allowed).

### `04-theme` (chapter 04): the theme of the login page

Opens on `/Admin/Settings/userLogin` (`OpenAsync`). Restore S0 before the take.

- `theme-1`: highlight the checkbox **Use site theme for login page** with its hint **Requires an active site theme.**,
  clear the highlight, click the checkbox (it becomes ticked), then `ClickAndWaitForNavigationAsync` on `button.save`.
  Ends with the notification **Site settings updated successfully.** visible and **Use site theme for login page** ticked.
  Labels: `Views/LoginSettings.Edit.cshtml:62`, `:63`. Before the click, the login page uses the admin theme
  (`Services/UsersThemeSelector.cs:45` and `:86`).
- `theme-2`: `NavigateAsync("/Login")`, then highlight the site name in the navigation bar (`#mainNav .navbar-brand`, the
  text **Orchard Core**). Ends with the login form inside the layout of `TheBlogTheme`, the navigation bar with the site
  name above it. The theme loads Google Fonts: if the machine is offline the page still renders, with fallback fonts.
  Source: `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:27`.

### `05-local` (chapter 05): local password login

Opens on `/Admin/Settings/userLogin` (`OpenAsync`). The site is in S0. **Never tick this checkbox and never choose Save in
this clip**: with no external provider, the login page would show no form and nobody could sign in again
(`Views/Account/Login.cshtml:24-32`, and `Controllers/AccountController.cs:119-124` refuses the password sign-in).

- `local-1`: move the cursor to the checkbox **Disable local password login.** (the label ends with a period) and highlight
  the checkbox together with its hint ("When selected, users are not allowed to login with username/password. Make sure
  there is at least one admin with linked account"). Do not click. Ends with the checkbox unticked and the hint
  highlighted. Labels: `Views/LoginSettings.Edit.cshtml:73`, `:74`. The narration reads "linked account" as an account linked to an external
  provider: `Views/ExternalAuthentications/LinkExternalLogin.cshtml:8` and `:10` ("Link your account."), and
  `Controllers/ExternalAuthenticationsController.cs:232` ("User account linked to {LoginProvider} provider.").
- `local-2`: keep the highlight on the checkbox, with the caption `Leave this option off` (a caption of
  `HighlightAsync`). No other action. Ends with the same screen, the checkbox still unticked. Source of the narration:
  `Views/Account/Login.cshtml:24-32` (the form is rendered only when local login is not disabled), `:34-48` (the provider
  buttons under **Use another service to log in**).

### `06-details` (chapter 06): the details of a user

Opens on `/Admin/Users/Index` (`OpenAsync`). Restore S0 before the take. The list shows the rows `admin` and `jamie.chen`.

- `details-1`: in the row of `jamie.chen`, `ClickAndWaitForNavigationAsync` on its **Edit** button (role Link, name `Edit`,
  exact; `Views/UserButtons.cshtml:11`). Keep the path of the edit page in a variable (`new Uri(scene.Page.Url).PathAndQuery`)
  for `details-3`. Move the cursor to the **User name** field, then the **Email** field, then the **Phone Number** field,
  with a highlight on each in turn. Ends on the edit page of `jamie.chen`, with **User name** and **Email** greyed out
  (disabled) and **Phone Number** editable. Labels: `Views/UserName.Edit.cshtml:4`, `Views/UserEmail.Edit.cshtml:4`,
  `Views/UserPhoneNumber.Edit.cshtml:4`; the rule: `Drivers/UserInformationDisplayDriver.cs:51`, `:59`, `:68`. Path:
  **Access Control** > **Users** (`AdminMenu.cs:27-30`).
- `details-2`: clear the highlights. `NavigateAsync("/Admin/Settings/userLogin")`, click the checkbox
  **Allow user to change their username**, click the checkbox **Allow user to change their email address**, then
  `ClickAndWaitForNavigationAsync` on `button.save`. Ends with the notification **Site settings updated successfully.**
  visible and these ticked: **Allow user to change their username**, **Allow user to change their email address**, **Allow
  user to change their phone number**. Labels: `Views/LoginSettings.Edit.cshtml:34`, `:43`, `:52`.
- `details-3`: `NavigateAsync(<the path kept in details-1>)`, then highlight the **User name** field and the **Email** field.
  Ends on the edit page of `jamie.chen` with **User name**, **Email** and **Phone Number** all editable (white fields).
  Do not change a value and do not choose **Save**. Source of the narration:
  `Drivers/UserInformationDisplayDriver.cs:51`, `:59`, `:68` (a new user is always editable, `context.IsNew`).

### `07-lockout` (chapter 07): a locked account

Opens on `/Login` (`OpenAsync`). Restore S0 before every take. The account `jamie.chen` exists and is not locked. The wrong
password is `<password>` followed by the letter `x` (the value of `VIDEO_PASSWORD` plus one letter, so it can never be
the password of `jamie.chen`; the field is masked on screen, and the eye button `#togglePassword` is never clicked).

Record `lockout-1` to `lockout-5` in one take, without a pause. The lock ends by itself after the lockout time span of
the Identity options (not set by Orchard Core, so the default of ASP.NET Core Identity applies; believed to be five
minutes, see `OPEN-QUESTIONS.md`), and the **Unlock** item is offered only while the account is locked
(`Views/UserActionsMenu.cshtml:27-30`, `UserManager.IsLockedOutAsync`). If the lock has ended before `lockout-4`, the
**Unlock** item is missing and `lockout-3` shows no badge: restore S0 and record the take again.

- `lockout-1`: click `#LoginForm_UserName` and type `jamie.chen` (`TypeAsync`), click `#LoginForm_Password` and type the
  wrong password (`<password>` followed by `x`), then `ClickAndWaitForNavigationAsync` on the **Log in** button. Ends on the login page with the message
  **Invalid login attempt.** visible above the form, the **Username or email address** field still holding `jamie.chen`
  and the password field empty. Source: `Controllers/AccountController.cs:206` (message), `:142` (`lockoutOnFailure: true`
  counts the failed attempt).
- `lockout-2`: wrap the repeated attempts in `scene.OffCameraAsync(...)`. Repeat until the text
  **The account is locked out** is visible: if `#LoginForm_UserName` is empty, fill it with `jamie.chen`; fill
  `#LoginForm_Password` with the same wrong password; click **Log in** and wait for the page to load. Stop after at most 15
  attempts and report it (see `OPEN-QUESTIONS.md`). The wait condition is the message, not a fixed count of attempts.
  Ends on the login page with **The account is locked out** visible (no period in the message). When locked, the code
  returns the page without the form (`Controllers/AccountController.cs:190-196`, `return View()`), so the form may be
  absent; that is expected. Source: `Controllers/AccountController.cs:192`.
- `lockout-3`: `NavigateAsync("/Admin/Users/Index")`. Move the cursor to the badge **Locked** in the row of `jamie.chen` and
  highlight it. Ends on the Users list, the row of `jamie.chen` with the red **Locked** badge, the row of `admin` without
  one. Do not read the badge's tooltip. Source: `Views/UserFields.cshtml:16-17`; path **Access Control** > **Users**
  (`AdminMenu.cs:27-30`).
- `lockout-4`: clear the highlight. In the row of `jamie.chen`, click the **Actions** button (role Button, name `Actions`;
  `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml:32`; if the list uses the Menu layout, the
  button is the ellipsis with the accessible name `Actions`, `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions-Menu.cshtml`), click the item **Unlock**
  (`Views/UserActionsMenu.cshtml:29`), wait until the dialog titled **Unlock user** with the text "Are you sure you want
  to unlock this user?" is visible (`#confirmRemoveModal`), pause for one second, then click **Ok** (`#modalOkButton`) with
  `WaitForNavigationAsync`. Ends on the Users list after the redirect. Labels: `Views/UserActionsMenu.cshtml:29`;
  buttons **Ok** and **Cancel**: `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`.
- `lockout-5`: highlight the notification **User unlocked successfully.** and then the row of `jamie.chen`. Ends with the
  notification visible and the row of `jamie.chen` without the **Locked** badge. Source:
  `Controllers/AdminController.cs:633`.

## Slides

The slides are drawn by `slides.py`; nothing is recorded for them.

- `overview-1`: the heading "The User Login page" and three cards: the path Settings, Security, User Login; one checkbox
  for each option; more tabs with more features.
- `overview-2`: the heading "Four groups of options" and four cards: Remember me, Login page theme, Local password
  login, User details.
- `overview-3`: the heading "Account lockout" and three cards: not an option of this page, set by a developer, handled in the Users list.

## Checks for private data

- The only addresses on screen are `admin@example.com` (not shown on the pages recorded) and `jamie.chen@example.com`;
  the only people are the invented `jamie.chen` and `admin`.
- No password is visible: the password fields are masked and `#togglePassword` is never clicked. Only the wrong password of `07-lockout` (`<password>` followed by `x`) is typed on camera, and the field masks it.
- Look at every contact sheet (`review.py`): no error page, no leftover real name, no stack trace in `lockout-2`.
- OCR the final video at one frame per second and at every scene change, and search for `@` followed by anything other
  than `example.com`, and for any real name or token.
- Remove the throwaway site and its `App_Data` copies when the video is accepted. No `state.json`, MP4 or audio file is
  committed.

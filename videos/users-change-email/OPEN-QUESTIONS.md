# Open questions: Change Email

What was read from the source of Orchard Core (commit `92db4a83`) and what a person must confirm on a running site
before or while recording. The narration states only what the source shows; each item below is a part that only a
running site can settle (the text of a message, a framework behavior, a look).

## 1. The message for an address that is not an email address

- Checked: `ViewModels/ChangeEmailViewModel.cs` declares the message "Invalid Email." on the `Email` property, but the
  attribute it uses, `OrchardCore.Email.EmailAddressAttribute`
  (`OrchardCore.Infrastructure.Abstractions/Email/EmailAddressAttribute.cs:23`), returns its own result with the text
  "Invalid email address." Which of the two reaches the page is decided by the MVC validation code, which is outside
  this source: the source registers data annotations localization (`OrchardCore.Mvc.Core/Startup.cs:110`) and no custom
  attribute adapter, but it cannot show whether MVC keeps the text of the validation result or replaces it with the
  `ErrorMessage` of the attribute. So the text on screen is either "Invalid email address." or "Invalid Email.", and
  the plan names both.
- Confirm: in clip `05-checks`, step `checks-3`, read the message under the field and above the form. The narration
  only says "a validation message", so either text is safe. If it differs from both, write the real text in
  `recording-plan.md`.
- Also confirm that the field `#Email` of the page has `type="text"`. The attribute does not derive from the data
  annotations one, so the tag helper should not choose `type="email"`; if it does, the browser stops the submit of
  `owner`, and the plan gives the fix (`noValidate`).

## 2. The session after the address changes

- Checked: `UserService.ChangeEmailAsync` (`OrchardCore.Users.Core/Services/UserService.cs:167`) calls the ASP.NET Core
  Identity `UserManager.ChangeEmailAsync`, which is outside this source. Identity may refresh the security stamp of
  the user and mark the address as confirmed when it saves it. The narration says neither.
- Confirm: after `04-change`, whether the saved `state.json` still opens `/Admin/Users/Index` (if it opens the sign-in
  page, `Recorder setup` fixes it, which the plan already asks for), and whether the row of `admin` shows an
  **Unconfirmed** badge (`Views/UserFields.cshtml:11`). The plan expects no badge in `change-5`; if there is one, the
  step still works but its end frame must be checked against the narration.

## 3. How the Change Email pages look under The Blog Theme

- Checked: the two pages ask for the layout `Layout__Login` (`Views/ChangeEmail/Index.cshtml:4`,
  `Views/ChangeEmail/ChangeEmailConfirmation.cshtml:2`). Only the admin theme has that layout
  (`OrchardCore.Themes/TheAdmin/Views/Layout-Login.cshtml`), and the controller is not an admin controller
  (`Controllers/ChangeEmailController.cs`), so the pages should be drawn by the site theme with its normal layout.
  The Blog recipe's theme draws no user menu (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`), which is why the
  video opens the page from the admin user menu.
- Confirm: that `change-3`, `change-4` and the three `05-checks` steps show the heading, the field, the button and the
  messages readably in that theme. The narration describes the content of the pages, never their look.

## 4. Who can open the setting

- Checked: the menu item and the settings editor both require the permission **Manage security settings and all
  users** (`ChangeEmailAdminMenu.cs:29`, `Drivers/ChangeEmailSettingsDisplayDriver.cs`), and the settings route also
  requires a settings permission (`OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs`), because
  no group permission is registered for `userChangeEmail`. The page `/ChangeEmail` itself only needs a signed-in user
  (`Controllers/ChangeEmailController.cs:14`).
- Confirm (only if a later version of the video names a permission): which roles can open Settings, Security, Change
  Email. The narration names none.

## 5. Signing in with the changed address

- Checked: the narration of `change-5` says the user can type the address in the **Username or email address** field.
  The sign-in finds a user by name and then by address (`Controllers/AccountController.cs:138`,
  `OrchardCore.Users.Core/Services/UserService.cs:284-290`), the second only when unique addresses are required, which
  is the default (`Models/IdentitySettings.cs`).
- Confirm: after the take, sign out and sign in with `owner@example.com` and the password (not on camera).

## 6. Messages that fade, and the confirmation dialog of the seed

- The green messages of `enable-2` and `setting-2` are admin notifications. If one has faded before the narration of
  its step ends, hold the step longer or ring the changed row or box instead, as the plan allows for `setting-3`.
- The **Enable** control of the Features page is a link that the admin scripts turn into a POST
  (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`, the action is `[HttpPost]` at
  `Controllers/AdminController.cs:225`). Confirm that `ClickAndWaitForNavigationAsync` catches that navigation, and that
  the click inside `OffCameraAsync` cuts the tenant restart cleanly: look at the contact sheet of `enable-2`, which
  must jump from the cursor over **Enable** to the reloaded page with the message, with no frozen frame.
- Saving the Change Email setting requests no restart in the source read (the only release request of the Settings
  module is in the General settings driver), so `setting-2` waits on camera. If a take shows a long wait, use the same
  wrapper as `enable-2`.
- The dialog opened by **Confirm email** in the seed (`Views/UserActionsMenu.cshtml:35`) is drawn by the admin scripts,
  and the label of its accept button was not read from this source. The seed is outside the capture, so any accept
  button will do.

## 7. Pronunciation

- The narration never reads an email address aloud, and `spoken` is empty. After `narrate.py`, listen to `enable-2`,
  `setting-1` and `change-4` for "Users Change Email" and "Username or email address".

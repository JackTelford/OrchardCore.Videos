# Recording plan: users-reset-password

How to record the video without deciding anything. A selector stands for `scene.Page.Locator(...)`; a label lookup
stands for `scene.Page.GetByLabel(...)`, `GetByRole(...)` or `GetByText(...)`. Every label below is written as the code
spells it, with the file and line of the Orchard Core source (commit 92db4a83, paths under `src/OrchardCore.Modules/`).
Parts that could not be checked from the source are marked "see OPEN-QUESTIONS" and listed in `OPEN-QUESTIONS.md`.

The video has six recorded clips (`02-enable`, `02-email`, `03-allow`, `04-request`, `05-mail`, `06-reset`) and two
slide clips (`01-how`, `07-notes`). The audience is admin: nothing on screen is code.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore`, on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with its own `App_Data` outside both repositories (`ORCHARD_APP_DATA`), provisioned by Auto Setup as
  `references/recording.md` describes: Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, Default
  tenant (no URL prefix), user `admin` with email `admin@example.com`. No setup or login form is recorded.
- **Admin sign-in**: `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`. The password is chosen outside git. The
  recorder signs in once (`Recorder setup`) and saves `state.json` (never committed).
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, as `references/recording.md` says. The SMTP
  connection below goes straight to the local port and is not an HTTP request (see OPEN-QUESTIONS 1).
- **Time zone and culture** of the browser: the recorder's defaults (`en-US`, light color scheme).
- **External service: a local SMTP catcher.** smtp4dev in Docker, started before the site is used and removed afterwards:

  ```bash
  docker run --rm -d --name ocvideo-smtp4dev -p 3000:80 -p 2525:25 rnwood/smtp4dev
  ```

  SMTP is `localhost:2525`; its web interface is `http://localhost:3000` (`VIDEO_MAIL_URL`). Stop it with
  `docker rm -f ocvideo-smtp4dev`. Image, ports and interface: see OPEN-QUESTIONS 1.
- **Three passwords, all chosen outside git, all different, all accepted by the password rules of the site** (the Users
  module leaves the ASP.NET Core Identity defaults in place: see OPEN-QUESTIONS 9):
  `<password>` (the administrator, `VIDEO_PASSWORD`), `<user-password>` (Jamie's first password, `VIDEO_USER_PASSWORD`)
  and `<new-password>` (typed in `06-reset`, `VIDEO_NEW_PASSWORD`). Never write a real value in a file of the repository.

### Features to enable before recording (outside the capture)

The Blog recipe enables Users but not Email. Enable these two in **Tools > Features** (`/Admin/Features`) and leave
**Users Reset Password** disabled:

1. **Email** (`OrchardCore.Email`).
2. **SMTP Email Provider** (`OrchardCore.Email.Smtp`).

Having both enabled before `02-enable` makes the message of that clip the singular "The feature Users Reset Password
was enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`). If Email were still disabled, the message
would list both features.

### Settings to save before recording (outside the capture)

**Settings > Communication > Email** (`/Admin/Settings/email`). Labels from `OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml`:

| Label | Value |
|---|---|
| Enable SMTP Provider (line 10) | checked |
| Sender email address (line 17) | `noreply@example.com` |
| Delivery method (line 26) | Network |
| Host name (line 50) | `localhost` |
| Port number (line 59) | `2525` |
| Auto Select Encryption method (line 90) | unchecked |
| Encryption method to use (line 97) | the option that starts with "None" |
| Require credentials (line 120) | unchecked |
| Ignore Invalid SSL Certificate (line 180) | unchecked |

Save the page. Saving with SMTP enabled and no default provider selects the SMTP provider
(`OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:193-196`); check that **Default provider**
(`OrchardCore.Email/Views/EmailSettings.Edit.cshtml:20`) shows "Simple Mail Transfer Protocol (SMTP)". Then open
**Tools > Testing > Email Test** (`OrchardCore.Email/AdminMenu.cs:27-30`), send one message to `admin@example.com`
(the page confirms with "Message sent successfully.", `OrchardCore.Email/Controllers/AdminController.cs:82`), see it
in the catcher, and delete it. The setting **Allow the users to reset their password** stays unchecked (its default).

### Seed data (outside the capture)

One fake user, created in **Access Control > Users** with the Create User page (`/Admin/Users/Create`,
`OrchardCore.Users/Views/Admin/Create.cshtml`). Fields and labels from `Views/UserName.Edit.cshtml:4`,
`Views/UserEmail.Edit.cshtml:4` and `Views/UserFields.Edit.cshtml:12-27`:

| Label | Value |
|---|---|
| User name | `jamie.chen` |
| Email | `jamie.chen@example.com` |
| Password | `<user-password>` |
| Password confirmation (placeholder) | `<user-password>` |

Leave the other fields as they are, assign no role, and select **Save** (`Views/UserSaveButtons.Edit.cshtml:2`).
Registration is not enabled, so the user needs no confirmed email address.

When this is done, run `Recorder setup`, then copy `App_Data` and `state.json` aside as **snapshot S0**.

## Order of the clips, and what changes the site

Record in this order (list the clips in this order in `s_clips` of `Scenes.cs`, since `Recorder record` runs the selected
clips in the order of that list). `04-request`, `05-mail` and `06-reset` are recorded together, in one run, within 15 minutes.

| # | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-enable` | S0 | Yes: enables Users Reset Password (the tenant restarts) |
| 2 | `02-email` | after S0, any time | No |
| 3 | `03-allow` | after `02-enable` | Yes: saves the Reset Password setting. Take **snapshot S1** now |
| 4 | `04-request` | S1, empty inbox, no cooldown | Yes: sends the email, starts the one-minute cooldown of Jamie, creates the token |
| 5 | `05-mail` | right after `04-request` | No (reads the inbox) |
| 6 | `06-reset` | right after `05-mail`, the email of `04-request` unused and not older than 15 minutes | Yes: Jamie's password changes, and the link of the email stops working |

**Retakes.**

- `02-enable`, `02-email`, `03-allow`: stop the site, restore S0, start it, record again from `02-enable`.
- `04-request`, `05-mail` or `06-reset`: all three are recorded again together. Stop the site, restore S1, start it
  (a restart clears the one-minute cooldown, which lives in the memory cache unless a distributed cache is configured; otherwise wait more than 60 seconds), empty
  the inbox of the catcher, then `Recorder record 04-request 05-mail 06-reset`. A second request for Jamie within a
  minute of the first shows the same confirmation page but sends no email
  (`OrchardCore.Users/Controllers/ResetPasswordController.cs:111-118, 188-206`), so a clip that depends on the email would fail.
- Narrate first (`narrate.py`), so the steps are held for the length of their narration.

## How the clips are written (the same in every clip)

- Clips `02-enable`, `02-email` and `03-allow` use `NewSceneAsync`, so they start signed in as `admin` from `state.json`.
- Clips `04-request`, `05-mail` and `06-reset` show a visitor who is signed out. Every scene loads `state.json`, so in
  these three call `await scene.Context.ClearCookiesAsync();` before the first page is opened. This does not change the
  file, so the next clip is signed in again.
- `OpenAsync` and `NavigateAsync` prepend `VIDEO_BASE_URL`, so neither can reach the catcher. For the catcher and for the
  reset link use `await scene.Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle })` **before**
  `StartCaptureAsync()`.
- Menu entries are highlighted only where a step says so; every page is opened by URL before the capture starts, with the
  admin menu expanded to the entry the step names (click its parents before `StartCaptureAsync`).

---

## Clip `02-enable` (chapter 02)

Starts on `/Admin/Features`, opened with `scene.OpenAsync("/Admin/Features")`. The search box is empty and the page is
loaded. Use the row helper of the worked example:
`Row() = scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-enable-OrchardCore_Users_ResetPassword, #btn-disable-OrchardCore_Users_ResetPassword") }).First`
(see OPEN-QUESTIONS 3 about the list layout).

- `enable-1`: `await scene.TypeAsync(scene.Page.Locator("#search-box"), "reset password", delay: 60);` then wait until
  `#btn-enable-OrchardCore_Users_ResetPassword` is visible. `HighlightAsync(Row())`, then
  `MoveToAsync(Row().Locator(".direct-dependencies"))`.
  *On screen when the narration ends:* the search box with `reset password`; under the **Security** heading one row,
  **Users Reset Password**, its description "The reset password feature allows users to reset their password.", the
  badges **Users** and **Email**, the status **Disabled**, and the **Enable** button, with a green ring around the row.
  *Labels:* Search placeholder `OrchardCore.Features/Views/Admin/Features.cshtml:27`; name, description and category
  `OrchardCore.Users/Manifest.cs:57-66`; badges `OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml`;
  status `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml`.
- `enable-2`: `await scene.ClearHighlightsAsync();` then
  `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btn-enable-OrchardCore_Users_ResetPassword"));` and wait
  until the text "The feature Users Reset Password was enabled." is visible (the tenant restarts, so allow up to 20 seconds).
  *On screen when the narration ends:* the Features page, reloaded, with the green message "The feature Users Reset
  Password was enabled." at the top.
  *Labels:* **Enable** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; message
  `OrchardCore.Features/Controllers/AdminController.cs:317`. The link is turned into a POST form with the anti-forgery token of the page by `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:117-154`; see OPEN-QUESTIONS 3.

## Clip `02-email` (chapter 02)

Starts on `/Admin/Settings/email`, opened with `scene.OpenAsync("/Admin/Settings/email")`. Before the capture, click the
menu entries **Settings** and **Communication** so that **Email** shows (`OrchardCore.Email/AdminMenu.cs:38-40`).

- `email-1`: `HighlightAsync(scene.Page.GetByLabel("Default provider"), "Default provider")`.
  *On screen when the narration ends:* the Email settings page with **Default provider** showing "Simple Mail Transfer
  Protocol (SMTP)", ringed.
  *Labels:* `OrchardCore.Email/Views/EmailSettings.Edit.cshtml:20`; the option text
  `OrchardCore.Email.Smtp/Services/SmtpEmailProvider.cs:20`.
- `email-2`: `await scene.ClearHighlightsAsync();` then open the SMTP part of the page: click the tab named **SMTP** if the
  page has tabs (`scene.Page.GetByRole(AriaRole.Tab, new() { Name = "SMTP" })`; see OPEN-QUESTIONS 2). Highlight in turn
  `GetByLabel("Sender email address")`, `GetByLabel("Host name")`, `GetByLabel("Port number")`, with
  `PauseAsync(900)` between them, and leave the three rings in place. Do not select Save.
  *On screen when the narration ends:* the SMTP settings with **Sender email address** `noreply@example.com`, **Host
  name** `localhost` and **Port number** `2525`, each ringed.
  *Labels:* `OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:17, 50, 59`.

## Clip `03-allow` (chapter 03)

Starts on `/Admin/Settings/userResetPassword`, opened with `scene.OpenAsync("/Admin/Settings/userResetPassword")`
(group id `userResetPassword`, `OrchardCore.Users/Drivers/ResetPasswordSettingsDisplayDriver.cs:13`). Before the capture,
click the menu entries **Settings** and **Security** so that **Reset Password** shows. Both checkboxes are unchecked.

- `allow-1`: highlight the menu entry `scene.Page.Locator("a[href$='/Admin/Settings/userResetPassword']").First` with the
  caption "Settings, Security, Reset Password" (if it is not visible, skip this ring: see OPEN-QUESTIONS 2), then
  `ClearHighlightsAsync()`, then highlight `GetByLabel("Allow the users to reset their password")` and
  `GetByLabel("Use site theme for password reset page")`.
  *On screen when the narration ends:* the Reset Password settings page, both checkboxes unchecked and ringed.
  *Labels:* menu `OrchardCore.Users/ResetPasswordAdminMenu.cs:26-28`; checkboxes
  `OrchardCore.Users/Views/ResetPasswordSettings.Edit.cshtml:8, 18`.
- `allow-2`: `ClearHighlightsAsync()`, then `await scene.ClickAsync(scene.Page.GetByLabel("Allow the users to reset their password"));`
  *On screen when the narration ends:* **Allow the users to reset their password** checked, with its hint "An email will
  be sent to the user's email address with a reset link." (`Views/ResetPasswordSettings.Edit.cshtml:9`).
- `allow-3`: `HighlightAsync(scene.Page.GetByLabel("Use site theme for password reset page"))` (its hint is "Requires an
  active site theme.", `Views/ResetPasswordSettings.Edit.cshtml:19`), `PauseAsync(1200)`, `ClearHighlightsAsync()`, then
  `await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));`
  and wait until "Site settings updated successfully." is visible. Leave **Use site theme for password reset page** unchecked.
  *On screen when the narration ends:* the page reloaded with the message "Site settings updated successfully." and
  **Allow the users to reset their password** checked.
  *Labels:* **Save** `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; message
  `OrchardCore.Settings/Controllers/AdminController.cs:119`.

## Clip `04-request` (chapter 04)

A signed-out visitor. Before the capture: `await scene.Context.ClearCookiesAsync();` then `await scene.OpenAsync("/Login");`.
The inbox of the catcher is empty and the one-minute cooldown of Jamie has expired (see "Retakes").

- `request-1`: `HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Forgot your password?" }))`. Stay on the
  login page.
  *On screen when the narration ends:* the login page with **Username or email address**, **Password**, the **Log in**
  button, and under it the link **Forgot your password?**, ringed.
  *Labels:* `OrchardCore.Users/Views/LoginFormForgotPassword.cshtml:2`; button `Views/LoginForm.Edit.cshtml:9`; fields
  `Views/LoginFormCredentials.cshtml:7, 13`.
- `request-2`: `ClearHighlightsAsync()`, then
  `await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Forgot your password?" }));`
  (the page `/ForgotPassword`), then
  `await scene.TypeAsync(scene.Page.GetByLabel("Username or email address"), "jamie.chen@example.com");`
  *On screen when the narration ends:* the heading **Forgot password?**, the field **Username or email address** holding
  `jamie.chen@example.com`, and the **Submit** button.
  *Labels:* `OrchardCore.Users/Views/ForgotPasswordForm.Edit.cshtml:3, 16`; `Views/ForgotPasswordFormIdentifier.cshtml:4`;
  route `OrchardCore.Users/Startup.cs:608-617`.
- `request-3`: `await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }));`
  and wait until "Please check your email to reset your password." is visible.
  *On screen when the narration ends:* the heading **Forgot Password Confirmation** and the sentence "Please check your
  email to reset your password."
  *Labels:* `OrchardCore.Users/Views/ResetPassword/ForgotPasswordConfirmation.cshtml:5-6`.
  After the clip, off camera: confirm that a message with the subject "Reset your password" is in the catcher (poll for up
  to 10 seconds). The confirmation page appears even when the email provider reports a failure
  (`OrchardCore.Users/Controllers/ResetPasswordController.cs:111-125`; `OrchardCore.Users/Services/UserEmailService.cs:103-105` ignores the
  result), so this check is the only proof. If the message is
  missing, fix the SMTP settings, restore S1 and restart.

## Clip `05-mail` (chapter 05)

Before the capture: `await scene.Page.GotoAsync(Environment.GetEnvironmentVariable("VIDEO_MAIL_URL"), new() { WaitUntil = WaitUntilState.NetworkIdle });`
The inbox holds exactly one message, the one of `04-request`. Nothing on this page is clicked except the message. The
screens of the catcher are not described in the narration (see OPEN-QUESTIONS 1).

- `mail-1`: `HighlightAsync(scene.Page.GetByText("Reset your password").First)`.
  *On screen when the narration ends:* the inbox list with one message ringed: subject **Reset your password**, recipient
  `jamie.chen@example.com`, sender `noreply@example.com`.
  *Labels:* the subject `OrchardCore.Users/Services/UserEmailService.cs:85`.
- `mail-2`: `ClearHighlightsAsync()`, then `await scene.ClickAsync(scene.Page.GetByText("Reset your password").First);`, wait
  until the body of the message is shown, and scroll (`ScrollAsync`) only if the last line of the body is not visible. Move
  the cursor next to the link **Reset Password** with `MoveToAsync`, but do not click it: the clip `06-reset` opens the link.
  *On screen when the narration ends:* the body of the message: "Someone recently requested that the password be reset for
  **jamie.chen**.", "To reset your password please click this link: **Reset Password**", "If you did not request a password
  reset please ignore this email - your password will not be changed.", and "Please be aware that this link will expire in 15
  minutes."
  *Labels:* `OrchardCore.Users/Views/TemplateUserLostPassword.cshtml:10, 12, 14, 16`; the 15 minutes
  `OrchardCore/OrchardCore.Users.Core/Models/PasswordResetTokenProviderOptions.cs:10`. If the preview shows the message in a
  frame, the link is `scene.Page.FrameLocator("iframe").GetByRole(AriaRole.Link, new() { Name = "Reset Password" })`
  (see OPEN-QUESTIONS 1).

## Clip `06-reset` (chapter 06)

A signed-out visitor who has the link of the email. Before the capture, in this order:

1. Off camera, read the reset link of the newest message of the catcher: open the catcher in a second page of the scene
   (`var mail = await scene.Context.NewPageAsync(); await mail.GotoAsync(mailUrl);`), open the message, read the `href`
   of the link **Reset Password**, then `await mail.CloseAsync();`. The link looks like
   `http://localhost:5010/ResetPassword?userId=...&code=...` (`OrchardCore.Users/Services/UserEmailService.cs:74-83`). Do not
   click the link in the catcher. If the `href` cannot be read this way, use the HTTP API of the catcher (see OPEN-QUESTIONS 1).
2. `await scene.Context.ClearCookiesAsync();`
3. `await scene.Page.GotoAsync(link, new() { WaitUntil = WaitUntilState.NetworkIdle });` Opening the page does not use the
   token; only the form's submit does.

Do not use `NavigateAsync` in this clip; every page change is a click on a link or a button.

- `reset-1`: highlight in turn `GetByLabel("Username or email address")`, `GetByLabel("New password", new() { Exact = true })`
  and `GetByLabel("New password confirmation")`, with `PauseAsync(700)` between them, and leave the rings in place.
  *On screen when the narration ends:* the heading **Reset password**, the three empty fields **Username or email
  address**, **New password** and **New password confirmation**, ringed, and the **Reset password** button.
  *Labels:* `OrchardCore.Users/Views/ResetPasswordForm.Edit.cshtml:3, 16`; `Views/ResetPasswordFormIdentifier.cshtml:6, 12, 19`.
  The link carries a user id, but the page reads only the token (`OrchardCore.Users/Controllers/ResetPasswordController.cs:134-151`),
  so the username field is empty.
- `reset-2`: `ClearHighlightsAsync()`, then type `jamie.chen@example.com` in **Username or email address**, and the value of
  `VIDEO_NEW_PASSWORD` (`<new-password>`) in **New password** and again in **New password confirmation**. Press Tab after
  the last field. The two password fields show dots; the password is never visible.
  *On screen when the narration ends:* the three fields filled, the two password fields masked, no validation message.
- `reset-3`: `await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Reset password" }));`
  and wait until "Your password has been reset." is visible. If a red message appears under **New password** instead, the
  password does not meet the rules of the site: change `<new-password>`, restore S1 and record the three clips again.
  *On screen when the narration ends:* the heading **Reset Password**, the sentence "Your password has been reset.", and
  the link **Click here to log in**.
  *Labels:* `OrchardCore.Users/Views/ResetPassword/ResetPasswordConfirmation.cshtml:5-8`.
- `reset-4`: `await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Click here to log in" }));`
  (the page `/Login`), type `jamie.chen@example.com` in **Username or email address**, type `<new-password>` in
  `GetByLabel("Password", new() { Exact = true })` (do not click the eye button of the field), then
  `await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }));` and wait
  until the address is `http://localhost:5010/`.
  *On screen when the narration ends:* the home page of the site (the Blog theme), not the login page, and no "Invalid
  login attempt." message. What the home page shows of a signed-in user: see OPEN-QUESTIONS 5.
  *Labels:* `OrchardCore.Users/Views/LoginFormCredentials.cshtml:7, 13`; `Views/LoginForm.Edit.cshtml:9`; the redirect to the
  home page `OrchardCore.Users/Controllers/AccountBaseController.cs:41`.

---

## Slides

- `how-1`: slide "Recover a password without an administrator" with three cards: **The user asks** ("On the login page,
  Forgot your password? opens a short form."), **The site sends an email**, **The user chooses a password**. All bright.
- `how-2`: slide "What the site needs" with three cards: **The Users Reset Password feature**, **An email provider**,
  **A setting**. All bright.
- `note-1`: slide "Good to know" with four cards (**One email per minute**, **Delivery problems stay hidden**,
  **Verified email addresses**, **Rate Limits**); the first two are bright.
- `note-2`: the same slide; the last two cards are bright.

## Checks for private data

- The frames show only fake data: `jamie.chen`, `jamie.chen@example.com`, `noreply@example.com`, `admin@example.com`,
  `localhost`. No real name, address, token or password appears.
- The password fields are masked in every clip; the eye button of the login page is never clicked; no password is typed in a
  field that shows its text.
- The reset link (with its token) is never on screen: it is read off camera and opened by address. The catcher's
  **Headers** and **Source** views, which show the machine name, are never opened.
- OCR the video at one frame per second and at every scene change, then at 5 frames per second, and search for the real
  name of the machine, the Windows user name, any password value, and `code=` tokens.
- Remove the container and delete `state.json`, the copied `App_Data` folders and the environment variables afterwards.

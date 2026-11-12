# Open questions: users-reset-password

What the narration states is read from the Orchard Core source (commit 92db4a83). The points below could not be checked
without a running site or a running mail catcher. None of them changes a narrated fact; they decide how the recorder
reaches a screen. Answer each on the recording site and, if the answer differs, tell the author before recording.

## 1. The mail catcher (smtp4dev)

*Checked:* nothing, because its interface is not part of the Orchard Core source. The plan relies on these points, all from
memory of the project's documentation:

- the Docker image `rnwood/smtp4dev` listens for SMTP on container port 25 and serves its web interface on port 80, so
  `-p 3000:80 -p 2525:25` gives `http://localhost:3000` and `localhost:2525`;
- the inbox shows a message as a row with its subject, and a click on the row opens it and shows its body;
- the inbox is emptied from its own interface (or its HTTP API) before each take;
- the HTML body is shown in a frame, in which the link **Reset Password** can be read (`href`) but should not be clicked.

*Confirm:* the ports and the image start; the message of the reset email shows the subject "Reset your password" and the
body described in the plan; how to empty the inbox; how to read the `href` of the link (in the frame, or from the API of
the catcher); that the plan's locators (`GetByText("Reset your password")`, the frame locator) find the elements. If the
inbox does not update by itself, reload the page before `StartCaptureAsync` in `05-mail`.

*Also confirm:* the SMTP connection to `localhost:2525` is not affected by `HTTP_PROXY` and `HTTPS_PROXY` (the SMTP provider
opens a socket; the settings page has proxy fields of its own, which stay empty), and that **Encryption method to use** =
None works against the catcher.

## 2. Menu entries and the tabs of the Email settings page

*Checked:* the menu paths and labels in `ResetPasswordAdminMenu.cs:26-28` and `OrchardCore.Email/AdminMenu.cs:38-40`, and
the group ids (`userResetPassword`, `email`). The Email settings page is built from two editors placed at `Content:1#Providers`
(`EmailSettingsDisplayDriver.cs`) and `Content:5#SMTP` (`SmtpSettingsDisplayDriver.cs`): the part after `#` is a tab name in
the placement syntax, so the page probably has tabs named "Providers" and "SMTP".

*Confirm:* the page shows those tabs, and which one is open first (the plan expects "Providers", with **Default provider**).
If the page has no tabs, the plan's click on the "SMTP" tab is skipped. Confirm that clicking **Settings** and then **Security**
(or **Communication**) in the admin menu reveals the entry before the capture, and that
`a[href$='/Admin/Settings/userResetPassword']` finds it; otherwise skip the ring around the menu entry in `allow-1`.

## 3. The Features list

*Checked:* the row markup (`FeatureEntry.*.SummaryAdmin.cshtml`), the button ids (`btn-enable-OrchardCore_Users_ResetPassword`,
built by `GenerateIdFromName`, as in the worked example), the search script (`features-index.ts`: it matches the lower-cased
name, id and description), and the success message (`AdminController.cs:317`).

Also checked: the defaults of the admin lists are the List layout and the Buttons actions layout
(`OrchardCore.Admin.Abstractions/AdminListOptions.cs`, and `Features.cshtml:92` gives the List rows the class `list-group-item`),
and the link **Enable** is turned into a POST form with the anti-forgery token of the page by `TheAdmin.ts:117-154`
(the controller action accepts POST only; the Features page has POST forms, so the token exists). The search text `reset password`
matches the feature name and description (`FeatureEntryDisplayDriver.cs:20`), and no other manifest has that phrase.

*Confirm:* the site keeps the default List layout and Buttons actions (an administrator or the host configuration can change
them; in the Grid layout the rows are table rows, and the helper must change); the search for `reset password` leaves one
row; the browser navigates when **Enable** is clicked; the tenant restart after enabling leaves the page usable within the 20
seconds of the plan.

## 4. A user whose email address is not verified

*Checked:* `ResetPasswordController.ForgotPasswordPOST` adds the error "Before you can reset your password, you need to verify
your email address." when `UsersMustValidateEmail` is on and the address is not confirmed, and then redirects to the
confirmation page unconditionally (lines 111-125), so the error is never displayed and no email is sent. The narration
(`note-2`) says only that such users get no reset email.

*Confirm (not recorded):* on a site with Users Registration set to **Users must verify their email address**, a user with
an unconfirmed address sees the confirmation page and receives nothing. The unreachable error message may deserve an issue.

## 5. What the home page shows after the login

*Checked:* a successful login redirects to `~/` (`AccountBaseController.cs:41`).

*Confirm:* what the front-end theme of the Blog recipe shows of a signed-in user (it may show nothing). The narration of
`reset-4` says only that the login succeeds and the home page opens. If the theme shows nothing, the end frame is the home page
without an error, which is what the plan expects.

## 6. The link lifetime

*Checked:* the email template prints the token lifespan of `PasswordResetTokenProviderOptions` (15 minutes,
`PasswordResetTokenProviderOptions.cs:10`), and nothing in the source configures that option.

*Confirm:* the email of the catcher says "Please be aware that this link will expire in 15 minutes." If it says another
number, change `mail-2` before recording.

## 7. Rate limits

*Checked:* with the Rate Limits feature enabled, `ResetPasswordRateLimitsStartup` registers a sliding window of 20 requests per
minute per IP address for the POST of the forgot password route and of the reset password route, both under the policy
`password-recovery` (`Startup.cs:651-662`; `RateLimitPartitionHelpers.cs`, window of one minute). The Rate Limits feature is not
enabled on the recording site, so the slide `note-2` is the only place it appears.

Both routes use the same policy name, so both build the partition key `password-recovery:<IP address>` (`RateLimitPartitionHelpers.cs:132-135`),
and the framework most likely gives the two forms one shared counter of 20 per minute. The slide says only that the two forms
accept 20 submissions per minute from one IP address, which is true in either case. Only the POST of each form is limited,
not the display of the pages.

*Confirm (optional):* with the Rate Limits feature enabled on a test site, whether submitting the two forms counts against one
budget. Nothing on the recording site depends on it.

## 8. "Use site theme for password reset page"

*Checked:* `UsersThemeSelector.cs:70-71` gives the pages of the `ResetPassword` controller the admin theme at priority 100 when
the box is unchecked and at priority -100 when it is checked. The site theme has priority 0 (`SiteThemeSelector.cs:29`) and
`ThemeManager.cs:57` sorts the results from the highest priority down, so the admin theme wins by default and the site theme
wins when the box is checked. The video states only that, and leaves the box unchecked.

*Confirm (not recorded):* nothing; do not check the box on camera.

## 9. Password rules

*Checked:* the Users module sets no `PasswordOptions` (`UserService.cs:319` only reports the minimum length of the configured
options), so the rules are those of ASP.NET Core Identity unless the host configures them: at least 6 characters, a digit,
a lowercase letter, an uppercase letter and a non-alphanumeric character, as the framework documents them.

*Confirm:* a password of the three kinds in the plan is accepted by the **Create User** page (the seed), by the reset form
and by the login. A rejected password shows a red message under **New password** in `reset-3`.

## 10. A default email provider that is no longer registered

*Checked:* the narration (`note-1`) says that the user still sees the confirmation page when the email provider reports that
sending failed. `ForgotPasswordPOST` ignores the result of `UserEmailService.SendPasswordResetAsync`
(`ResetPasswordController.cs:113`, `UserEmailService.cs:103-105`). The SMTP provider turns its own errors, an unreachable server
included, into a failed result (`SmtpEmailProviderBase.cs:98-101`), and without any default provider the email service returns a
failed result (`DefaultEmailService.cs:37-44`). When the stored default provider name has no registered provider (for example
after the SMTP feature is disabled), `DefaultEmailProviderResolver.cs:39` throws `InvalidEmailProviderException`, and nothing in
the controller catches it, so the request probably ends in an error page. The video avoids saying that every failure is hidden.

*Confirm (not recorded):* the two outcomes on a test site, with an unreachable SMTP host and with the SMTP feature disabled
while it is still the default provider.

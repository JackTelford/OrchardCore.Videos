# Open questions

Things the narration or the recording plan relies on that the source or the documentation did not settle. Each item says
what was checked in the source (Orchard Core commit `92db4a83`) and what a person must confirm on a running site before
recording. Until then, the narration holds only what the code says.

1. **Names and order of the methods.** The options of **Preferred Two-Factor Method** and the order of the methods at
   sign-in come from the provider names and the token provider map of ASP.NET Core Identity, which are not in this
   source. The plan expects the options **Authenticator** and **Email** (and **Phone** once a phone number is
   verified), and expects **Sign in another way** to lead from the authenticator app to Email while only those two
   methods are valid. It also expects that, before a preferred method is saved, the first prompt of `admin` can be the
   Email one, which the sign-in routine of the plan handles by clicking **Sign in another way**. Confirm all three with
   `Recorder probe`. The plan sets the preferred method to **Authenticator** so that the sign-in offers the app first
   (`Controllers/TwoFactorAuthenticationController.cs:440-470`). The narration of `methods-2` says "the authenticator app",
   not the label of the option.

2. **When a method counts as available.** The views show **Verified** for Email and SMS and **Verify Email** or
   **Verify Phone Number** otherwise (`Views/Items/TwoFactorMethod.Email.Actions.cshtml`,
   `TwoFactorMethod.Phone.Actions.cshtml`), and the setup marks the `admin` email as confirmed
   (`Services/SetupEventHandler.cs:28`). The rule that a method is available only with a confirmed email address or phone
   number is applied by the Identity token providers, not by this source. Confirm that the Email row of `admin` shows
   **Verified** as soon as **Two-Factor Email Method** is enabled, and that the row of `jamie` does too after
   **Confirm email**.

3. **The hint of Require two-factor authentication.** The hint under the option says that any user with enabled
   two-factor authentication will be required to use it (`Views/TwoFactorLoginSettings.Edit.cshtml:36`). The code
   does more: it sends a user who has not enabled it to the Two-Factor Authentication page after signing in, and
   keeps sending them there until they do (`Filters/TwoFactorAuthenticationAuthorizationFilter.cs:60-72`,
   `Services/TwoFactorAuthenticationClaimsProvider.cs:21-30`). The narration (`settings-4`, `required-1`) follows the
   code. Confirm on the site that `jamie` lands on the Two-Factor Authentication page with the red message
   **Two-factor authentication must be enabled before proceeding.** straight after **Log in**, and that other pages
   redirect there too.

4. **The saved sign-in of `admin`.** The plan records `04-app` before `03-settings` because a user who has not enabled
   two-factor authentication is locked out of every page but the two-factor pages once the site requires it, and
   because ASP.NET Core Identity rebuilds the principal of an old cookie (30 minutes by default; the source of Orchard
   Core sets no other interval; `OrchardCore.Users.Core/Services/ConfigureSecurityStampOptions.cs` keeps the custom claims
   through the rebuild), which adds the claim to the saved sign-in of `admin`. This is the reasoning of the code, not
   something seen on a site. Confirm that `02-enable` and `04-app` open signed in from `state.json` while the site does
   not require two-factor authentication, and that after `03-settings` a session of `admin` that has no method would
   land on the Two-Factor Authentication page (a check that is useful but not needed for the recording).

5. **The recovery codes page.** The codes are kept in the cache for five seconds after they are generated
   (`Controllers/TwoFactorAuthenticationBaseController.cs:68`). The narration says the page does not show them again
   later. Confirm that a reload after more than five seconds redirects to the Two-Factor Authentication page, and that the
   page stays readable while the clip is recorded (the recorder must not reload it).

6. **Remember this device.** `signin-2` says the browser skips the code at later sign-ins and states no duration. The
   hint of **Allow users to remember client** ends with a duration that comes from the options of the main
   authentication cookie (`Views/TwoFactorLoginSettings.Edit.cshtml:24`); the cookie that remembers a browser is the
   Identity one for that purpose, and its lifetime is not set in this source. The plan never ticks **Remember this
   device**. Do not add a duration to the narration unless a person has measured it.

7. **Pronunciation.** Check with `scripts/pronounce.py` how the voice reads "QR", "SMS", "Jamie", "Microsoft
   Authenticator", "Google Authenticator" and "sign-in". The storyboard has no `spoken` entry; add one only after a
   person confirms a spelling.

8. **The menus on screen.** The plan assumes the default row actions layout of the Users list (**Buttons**, with an
   **Actions** drop-down), and the user menu of the admin theme (`#navbarDropdown`, an icon at the top right). If the
   site shows an ellipsis button or another control, the narration ("the Actions menu of Jamie's row", "the user menu")
   still holds, but the selectors of `app-1` and `disable-2` change.

9. **The text areas of the messages.** **Email Body** and **Message Body** are filled with their default texts, which
   hold a template placeholder with braces. They are not narrated and the plan keeps them out of the frame (the
   settings clip never scrolls below the first group, and the rest of the tab is a slide). Confirm on the running site
   that no step of `03-settings` brings them into view at 1536x864, and that the first group, with the roles option
   that follows it, ends above them.

10. **A phone number the site accepts.** `+12015550123` is validated with a phone number library
    (`OrchardCore/OrchardCore.Infrastructure/PhoneFormatValidator.cs`); it is the example number of the United States
    in that library and uses the reserved fictional range 555-01xx. Confirm it is accepted; if the page says **Invalid
    phone number used.**, use another fictional number with the `+1` country code and update the plan.

11. **Where the Log SMS provider writes.** It logs a warning with the message body and the number
    (`OrchardCore/OrchardCore.Sms.Core/Services/LogSmsProvider.cs:33`). Confirm that the line appears on the console of
    the running site (and in its log file) at the logging level the site uses, so that the recorder can read the code.

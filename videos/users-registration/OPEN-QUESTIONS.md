# Open questions: User Registration

What was checked in the Orchard Core source (commit `92db4a83`, paths relative to the repository root), and what a
person has to confirm on a running site while recording. None of the narration depends on an unconfirmed fact: the
items below are either not narrated, or narrated only as far as the code shows.

`U` is `src/OrchardCore.Modules/OrchardCore.Users`.

## 1. The local inbox (smtp4dev)

The tracker asks for a local SMTP catcher. The plan uses `rnwood/smtp4dev` with its web page on port 3000 and SMTP on
port 2525. Nothing in the Orchard Core source covers it, so confirm on the machine that records:

- the image name and the two port mappings of the `docker run` command in recording-plan.md;
- that the web page lists the message with the subject "Confirm your account" (`U/Services/UserEmailService.cs:63`), and
  shows its body without the raw header view;
- that the link "click here" in the body can be clicked, and whether it opens in a new tab. If it does not, step
  `verify-4` falls back to reading the address off camera; the plan says how, but the script is not written;
- that the confirmation address is on `http://localhost:5010`, because the link is built from the request's host
  (`U/Services/UserEmailService.cs:46-54`).

## 2. The Register page with "Use site theme for registration page"

Step `settings-4` says the page is shown "in the design of your site, instead of the admin theme". That is what the
theme selector does (`U/Services/UsersThemeSelector.cs:86`: it offers the admin theme with priority 100, which wins, or
with priority -100 when the box is selected, which lets the site theme selector, priority 0, win;
`SiteThemeSelector.cs:29`, `ThemeManager.cs:57`), and the hint "Requires an active site theme." is on `U/Views/RegistrationSettings.Edit.cshtml:44`.

Not checked: how the Register page looks with `TheBlogTheme`. That theme has no login layout of its own (only `TheAdmin`
has `Layout-Login.cshtml`), and the Register view asks for the layout `Layout__Login`
(`U/Views/Registration/Register.cshtml:2`). The video never records that page with the box selected.

The other pages of the story do not follow that box. The pages "Email confirmation sent" and "Email confirmation" belong
to the Email confirmation controller, and the theme selector has no case for it (`U/Services/UsersThemeSelector.cs:41-75`,
`default: return null`), so they appear in the site theme, `TheBlogTheme`, whatever the box says. The Register and User
Registration Pending pages belong to the Registration controller and appear in the login layout of the admin theme while
the box is cleared (`:67-69,86`). Only `UsersThemeSelector`, `AdminThemeSelector` and `SiteThemeSelector` are theme
selectors in `src/OrchardCore.Modules`. Not checked: how the two confirmation pages look in `TheBlogTheme`. To confirm
while recording: that the heading and the sentence of each are visible without scrolling in clips `05-verify-visitor`.

## 3. The tab names on the Registration settings page

Step `external-1` names the tabs "External Authentication" and "General". The code puts the settings of the Registration
feature in the tab "General" (`U/Drivers/RegistrationSettingsDisplayDriver.cs:47`, `Content:5#General`) and the external
registration options in the tab "External Authentication" (`U/Drivers/ExternalRegistrationSettingsDisplayDriver.cs:44`),
and a tab strip is drawn only when there are at least two tabs
(`src/OrchardCore/OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:70-93`, `.../Shapes/GroupShapes.cs:14-70`). So the
three checkboxes of clips `03-settings`, `05-verify-settings` and `06-approve-settings` have no tab strip, and the tab
strip appears when a provider feature enables External Authentication.

To confirm: enable "GitHub Authentication" on a throwaway copy (it needs no credentials to enable), open Settings,
Security, Registration, and check that the tabs read "General" and "External Authentication". The external options are
shown on slides, not recorded, because signing in with a provider needs credentials from the provider.

## 4. Accounts created through a provider

Step `external-3` says that accounts created through a provider follow the same email and approval options as the
Register page. This is from the code: both paths call `UserService.RegisterAsync`
(`src/OrchardCore/OrchardCore.Users.Core/Services/UserService.cs:355-386`, called from
`U/Controllers/ExternalAuthenticationsController.cs:209` and `:307`), which reads `UsersMustValidateEmail` and
`UsersAreModerated` (`:365-366`), and the controller then runs the same login form events
(`ExternalAuthenticationsController.cs:240` and `:328`). It has not been seen with a real provider.

Step `external-2` says that with "Do not ask username", "Do not ask email address" and "Do not create local password"
all selected, the account can be created without any form (`ExternalAuthenticationsController.cs:201-209`). When the
provider sends no email address, the registration with an empty email may fail; the narration says "can be created".

## 5. A missing email provider

Step `enable-3` says that "without a provider no message is sent". This is from the code: the email service returns a
failed result when no provider is configured (`src/OrchardCore/OrchardCore.Email.Core/Services/DefaultEmailService.cs:39-44`,
the provider resolver returns null when there is no default provider name, `DefaultEmailProviderResolver.cs:42`). The registration
code ignores that result (`U/Handlers/EmailConfirmationRegistrationFormEvents.cs:33`), so a visitor still reaches the
page "Email confirmation sent" and no message arrives. The narration states only that no message is sent; what the visitor
sees is not narrated and not shown. If it should be, record it on a site without a provider.

## 6. The warning above the email checkbox

`U/Views/RegistrationSettings.Edit.cshtml:14,21` shows the warning through a collapse that every click on the checkbox
toggles, and the warning starts folded. So the warning appears when the box is selected on a page that loaded with the
box cleared (clips `03-settings` and `05-verify-settings`), and also when a saved box is cleared (clip
`06-approve-settings`). Confirm both on the recording; the narration of `settings-2` only describes the first.

## 7. What the Features page shows after Enable

Steps `enable-1` and `enable-2` rely on:

- the id `btn-enable-OrchardCore_Users_Registration` (built by `GenerateIdFromName` from the feature id; the example
  scene of the recorder uses the same pattern for `OrchardCore.Secrets`);
- the dependency badges "Users" and "Email" in the row, in the default List layout of the admin lists
  (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`);
- the notification "The feature Users Registration was enabled." (`F/Controllers/AdminController.cs:317`, with
  `F` = `src/OrchardCore.Modules/OrchardCore.Features`). The notification names only the feature that was selected:
  the action passes the one feature it was given to the notifier (`AdminController.cs:247`, `FeatureService.cs`
  `EnableOrDisableFeaturesAsync`), not the dependencies that were enabled with it.

If the site's admin settings select the Grid layout or the Menu actions layout, the badges sit in a column and the
Actions button of clip `06-approve-admin` is an ellipsis button instead (defaults: List and Buttons).

## 8. The visitor of clip 04-register

Step `register-3` says the visitor "is signed in and sent to the home page". The code signs the visitor in unless a
registration event cancels it (`src/OrchardCore/OrchardCore.Users.Core/Services/UserService.cs:378-381`) and redirects to
the home page when there is no return address (`U/Controllers/RegistrationController.cs:81,93-100`). The layout of
`TheBlogTheme` renders no user menu (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`), so the home page shows
no sign-in indicator and the recording shows only the home page. Confirm that the page loads
without an error.

## 9. Password rules

No Orchard Core code under `U` sets password rules; the plan assumes the defaults of ASP.NET Core Identity. Confirm that
the chosen `VIDEO_VISITOR_PASSWORD` is accepted by the Register form before the first take.

## 10. Requests of the browser to the internet

`TheBlogTheme` links to Google Fonts (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:10-11`). The pages in the
site theme (the home page, "Email confirmation sent", "Email confirmation") therefore make requests from the browser to
`fonts.googleapis.com`. `HTTP_PROXY` and `HTTPS_PROXY` are set for the site, not for the browser of the recorder. Confirm
that these pages still reach network idle (the recorder waits for it after a navigation) on the machine that records, and
that a font that fails to load does not change the look of the page in a way that shows up in the frames.

## 11. Words to listen for before narrating

`storyboard.json` has no `spoken` entries: the narration spells nothing out. Before running `narrate.py`, a person
should check with `scripts/pronounce.py` how the voice says the words a listener could trip over: "Ok" (`approve-4`,
the button of the confirmation dialog, `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`), "SMTP" (`enable-3`),
"GitHub" and "Meta" (`external-1`, `external-3`), and the first names "Jamie", "Morgan" and "Riley". If "Ok" is read
as two letters, a `spoken` entry is needed.

## 12. The tab that opens on the Email settings page

Step `enable-3` ends on the "Providers" tab of Settings, Communication, Email. The page draws its tabs in the order of
their position, and both the "Providers" tab (`src/OrchardCore.Modules/OrchardCore.Email/Drivers/EmailSettingsDisplayDriver.cs:59`)
and the "SMTP" tab (`SmtpSettingsDisplayDriver.cs:88`) have position 5, so which one is first is not settled by the
code that was read. The plan therefore says to click "Providers" when "SMTP" is the open tab. Confirm which tab opens.

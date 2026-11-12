# Open questions: google-login

What was checked in the Orchard Core source (commit `92db4a83`), and what a person must confirm on a running site or on
Google's side before the narration is recorded. Nothing below is stated in the narration as a fact beyond what the
source shows.

## Google's side

1. **The Google API console.** The documentation page (`src/docs/reference/modules/Google/README.md`) walks through
   Google's screens: it asks for the Google+ API, then lists the questions of a credentials wizard. The Google+ API and
   that wizard may no longer exist, and no label of Google's pages can be verified from the Orchard Core source. The
   narration therefore stays at the level the source supports: credentials for your application in the Google API
   console (the words of the hints in `GoogleAuthenticationSettings.Edit.cshtml:24,32`), a Client ID, a client secret,
   and an authorized redirect URI that points to the site (the documentation: "set the authorized redirect URI to
   point to your Orchard instance"). Someone should open Google's current console, confirm that **Client ID**,
   **client secret** and **authorized redirect URI** are still the names it uses, and change `how-2` and the slide of
   the same name if not.
2. **What the Google button does.** Not recorded, and not in the Orchard Core source. `login-3` says that selecting
   Google sends the browser to Google, where the person signs in and approves access. The source shows that the button
   posts to `ExternalAuthentications/ExternalLogin`, which returns a challenge for the scheme named Google
   (`ExternalAuthenticationsController.cs:70-84`); the redirect itself is made by the ASP.NET Core Google handler.
   Confirm on a running site, without letting the request leave the machine (for example by intercepting the
   response), that the answer is a redirect to Google's authorization address, and that its return address is the
   site's address followed by the callback path (`/signin-google` when CallbackPath is empty).
3. **The default callback path.** `settings-5` says the default is `signin-google`, the placeholder of the field
   (`GoogleAuthenticationSettings.Edit.cshtml:39`), as the documentation says. The default itself comes from the ASP.NET
   Core Google options, not from this source: confirm that a redirect made with an empty CallbackPath ends with
   `/signin-google`.

## Screens to confirm in the browser

4. **Admin menu (`settings-1`).** The menu is rendered collapsed (`TheAdmin/Views/NavigationItem-admin.cshtml`). Confirm
   that clicking **Settings**, **Security** and **Authentication** in turn opens each group in `#left-nav`, that the
   texts are unique enough for the locators, and that the page opens when **Google** (`#google`) is clicked. If the
   menu behaves differently, the plan's fallback is the direct address.
5. **Features page (`enable-1` to `enable-3`).** Confirm that typing in `#search-box` reduces the list to the
   **Google** category with three rows (no other feature manifest of the source mentions Google, and a module that declares explicit features gets no extra default feature, `src/OrchardCore/OrchardCore/Extensions/Features/FeaturesProvider.cs:22-82`, so no other row is expected); that the row of Google Authentication shows the dependency badge
   **External Authentication**; that the **Enable** link is the one matched by
   `a[href*="/Features/OrchardCore.Google.GoogleAuthentication/Enable"]` and that its click enables the feature and
   returns to the Features page; and that, with the filter **Hide on demand** cleared, External Authentication is
   listed as **Enabled** afterwards (`enable-3` says it is enabled with Google Authentication; the source shows the
   dependency, `Manifest.cs:12-21`, and the tooltip "This feature is automatically enabled based on dependency and
   cannot be manually disabled.").
6. **Toasts.** The messages **The feature Google Authentication was enabled.** and **Site settings updated
   successfully.** are toasts (`TheAdmin/Views/Message.cshtml`), and the source shows that a success message hides
   itself 5 seconds after it appears (`OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:70-71` passes 5000 ms;
   `Message.cshtml` sets `data-bs-autohide` and `data-bs-delay` from it). So neither toast is on screen when the
   narration of its step ends, and the recording plan places the click so that the toast shows while the narration says
   "a message confirms". Confirm on a running site that the toast of `enable-3` and of `settings-7` appears (a) after
   the reload that follows the release of the tenant (`GoogleAuthenticationSettingsDisplayDriver.cs:92`, and the
   feature change), and (b) early enough to be on screen during that sentence. If the reload takes several seconds, the
   toast can come after the words: take the clip again with a shorter `PauseAsync` in `enable-3`.
7. **The placeholder after saving (`settings-7`).** The placeholder is long: **The value is securely stored. Enter a
   new value to update it, or leave it blank to keep it.** Confirm that the box is wide enough to show it whole; if
   it is cut, the narration is still correct, but the frame shows only the start of the sentence.
8. **Store tokens (`settings-6`).** The narration says the box is cleared by default (the setting is a boolean that
   starts as false: `GoogleAuthenticationSettings.cs`). Confirm that the box is unchecked on a fresh page.
9. **Login page (`login-1`).** Confirm that a signed-in administrator who opens `/Login` sees the two columns **Log
   in** and **Use another service to log in** at the recorder's viewport, with one **Google** button, and that
   nothing else is drawn (no field filled in by the browser).
10. **The Secrets feature.** With Secrets disabled, the API secret key is a password box (`SecretInputTagHelper.cs:96`).
    With it enabled, a drop-down list precedes the box (`OrchardCore.Secrets/Views/Shared/SecretInput.cshtml`), and
    `related-1` says that the field can use a secret then. The video is recorded with Secrets disabled, so that sentence
    is only a slide: confirm the drop-down on a running site if it is to be shown later.

## Pronunciation

11. **`spoken` entries.** The storyboard maps `CallbackPath` to "Callback Path", `signin-google` to "sign in google", and
    `URI` to "U.R.I.". None is confirmed: check each with `scripts/pronounce.py` before narrating, together with
    "Client ID", "API" and "Log in". The map is applied as a plain text replacement (`narrate.py`), so keep these
    spellings out of other words.

## Behavior not narrated, noted for the maintainers

12. **Who can open the page.** The menu entry needs the permission **Manage Google Authentication settings**
    (`GoogleAuthenticationAdminMenu.cs:32`), and the editor needs it too (`GoogleAuthenticationSettingsDisplayDriver.cs:43`).
    The page also goes through the settings controller, which asks for the group permission
    (`OrchardCore.Settings/Controllers/AdminController.cs:53`); the Google startup registers no permission for its group
    (`GoogleAuthenticationStartup.cs`), so a role may need **Manage settings** as well. `related-1` only says that the
    Administrator role has the first permission (`GoogleAuthenticationPermissionProvider.cs:17-25`). Confirm with a test
    role before anything stronger is said.
13. **A plain client secret in a recipe.** The documentation's recipe example sets `ClientSecret` to a plain text value.
    The `Settings` step copies the properties unchanged (`OrchardCore.Settings/Recipes/SettingsStep.cs`, default case),
    and the options read the value with Data Protection (`SecretValueExtensions.cs`, `GoogleOptionsConfiguration.cs:71-76`),
    which cannot read a plain text value. Confirm on a running site that a client secret set this way does not work,
    and that the supported way is to save it from the admin page (or to use a secret). When the value cannot be read,
    the options of the Google handler get no client secret, so check too whether requests then fail. Reported in
    `docIssues`.
14. **The return trip (`how-3` and its slide).** Read from `ExternalAuthenticationsController.ExternalLoginCallback`, not
    run, because it needs a real Google test project. A linked user is signed in (`ExternalAuthenticationsController.cs:113-139`);
    a user with the same email address is offered the page **Link your account.**, which asks for that user's password
    (`:150-175`, `Views/ExternalAuthentications/LinkExternalLogin.cshtml`), but only when unique email addresses are
    required, which is the default (`OrchardCore.Users/Models/IdentitySettings.cs:24`); otherwise the person registers,
    unless **Disable new external registration** is selected (`:180-187`). Confirm this once with a throw-away Google
    test project and a test account, and note that the registration form can be skipped when the Registration settings
    ask for no user name, no email address and no password.
15. **New external registrations on a fresh site.** `how-3` and its slide say that the person registers *unless
    external registration is disabled*, because on the site this video is recorded on they are. The migration that
    creates the external registration settings sets `DisableNewRegistrations = enumValue == 0 || !isRegistrationFeatureEnabled`
    (`OrchardCore.Users/DataMigrations/ExternalAuthenticationMigrations.cs:32`). It is registered in the Users startup
    without a `[Feature]` attribute (`OrchardCore.Users/Startup.cs:79`), so it runs when the Users feature is installed,
    and the Blog recipe does not enable `OrchardCore.Users.Registration`. Read, not run. A person without a linked user
    or a matching email address then gets the message **New registrations are disabled for this site.**
    (`ExternalAuthenticationsController.cs:180-187`) and is sent back to the login page. Confirm in Settings, Security,
    Registration, External Authentication tab that **Disable new external registration** is selected on the fresh site,
    and that the migration did run at setup. The narration does not state the default, only the condition.

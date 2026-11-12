# Open questions: Meta Login

What the narration and the recording plan state was read from the Orchard Core source and from the documentation page
`src/docs/reference/modules/Facebook/README.md`. Nothing was run. A person confirms the points below on the running site
before recording, and on the Meta side where noted.

## On the running site

1. **The message after enabling.** The narration of `enable-3` quotes "The feature Meta Login was enabled." It was read
   from the code: the message names only the feature whose button was selected, not the two dependencies enabled with it
   (`OrchardCore.Features/Controllers/AdminController.cs:317`,
   `OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:79-80`). Confirm the wording on the running site, and
   that the Meta Login row shows "External Authentication" and "Meta Core Components" as badges while the page has no row
   of their own (both are enabled only by dependency, which "Hide on demand" hides by default). The narration of
   `enable-2` relies on those two badges.
2. **The search "Meta Login".** The narration says the result sits in the Meta category, with the description
   "Authenticates users from Meta." Confirm that the search leaves exactly that row visible
   (`OrchardCore.Features/Assets/ts/features-index.ts`).
3. **Selectors and ids.** The ids `btn-enable-OrchardCore_Facebook_Login` and `btn-disable-OrchardCore_Facebook_Login`,
   the menu links `a.item-label[href$=...]`, the toggles `button.item-label:has(> span.title:text-is(...))`,
   `input[name$="AppId"]` and the other selectors of `recording-plan.md` come from reading the views. Check each with
   `Recorder probe`. Only the toggle selector was tried, on a static copy of the menu markup in Playwright: the plain
   `button.item-label:text-is("Tools")` matched nothing there, the `:has(> span.title:text-is("Tools"))` form matched.
4. **Restart time.** Enabling the feature and saving the Meta App page both restart the tenant. How long the next page
   takes to load was not measured. If it outlasts the narration of `enable-3` (about seven seconds) or `app-5` (about
   eight seconds), the success message, which hides itself after five seconds, is missed; the plan names the
   `OffCameraAsync` fallback.
5. **The login page with made-up values.** Confirm that after the two values are saved, `/Login` lists the Facebook
   button without an error, and that the two sections ("Log in", "Use another service to log in") sit side by side
   in the viewport used for recording (`OrchardCore.Users/Views/Account/Login.cshtml`).
6. **The text area of the Meta App page.** The field "Parameters for the FB.init() call" shows a default list of
   settings. It is part of the page, so it is probably inside the frame of every clip that shows the page (`app-1`
   to `app-5`), because the page is short. The video would then show a few lines of default configuration text. The
   narration never reads it. A reviewer decides whether that is acceptable for an audience that is never shown
   configuration; the alternative is to move the narration of `app-4` into a slide, which changes nothing about the
   text area being on the page in the other clips.
7. **The Secret Key placeholder.** After saving, the field is empty and shows a placeholder. Confirm that the whole
   sentence "The value is securely stored. Enter a new value to update it, or leave it blank to keep it." fits the
   field at the recording size; if it is cut off, `app-5` still holds, because the narration says "securely stored".
8. **The default callback path.** `need-3` and `login-2` say that an empty CallbackPath means `/signin-facebook`.
   The module only sets the path when the field has a value (`OrchardCore.Facebook/Login/Configuration/FacebookLoginConfiguration.cs:99-102`);
   the default comes from the ASP.NET Core Facebook handler, which is not in this source tree, and from the documentation
   (`src/docs/reference/modules/Facebook/README.md`, "If no value is provided ... `/signin-facebook`"). A person confirms
   it in a test app, or in the handler's options.
9. **The Secret Key editor with Secrets enabled.** `app-3` describes the plain password box that the Secret Key shows
   when the Secrets feature is disabled (`OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:77-86`).
   With the feature enabled the field is the editor of the Secrets module instead, which was not recorded or described.
10. **The login checks of a linked user.** `after-1` says a login check can stop the sign-in, such as a disabled account
    (`OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:127`,
    `OrchardCore.Users/Handlers/DisabledUserLoginFormEvent.cs:36`). Read from the code; no sign-in was run.

11. **The default of "Disable new external registration".** The narration of `after-3` is conditional on that option
    (`OrchardCore.Users/Views/ExternalRegistrationSettings.Edit.cshtml:23`). The data migration that creates the setting
    selects it when the Registration feature is not enabled (`OrchardCore.Users/DataMigrations/ExternalAuthenticationMigrations.cs:32`),
    so on the Blog site of this video it is probably selected, and a visitor who is not linked and whose email address
    matches no local user would see "New registrations are disabled for this site." Enabling the Registration feature
    later does not change the option (nothing else writes it). Confirm under Settings, Security, Registration after
    `02-enable`. No clip signs a visitor in through Meta, so no recording depends on it.

## On the Meta side (not verifiable from the source)

12. **The words of the narration about Meta.** `need-2` and `need-3` use only what the documentation says: the
   application ID and secret are under Basic Settings of the app, the Meta Login product is enabled for web apps, and a
   valid OAuth redirect URI is set. A person with a Meta developer account confirms that the labels of the Meta portal
   still read that way, and that the redirect URI to enter is the address of the site followed by the callback path
   (`/signin-facebook` by default). The narration says only that the path of the redirect URI "must match the callback path
   of your site". Adjust the two steps and the slide if the portal differs.
13. **What happens after the click.** The video stops before the Facebook button is selected, so the page of Meta is
   not described. The narration says that selecting the button "sends the browser to Meta to sign in", which the code
   shows (`OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:83`). Nothing was run against Meta.
14. **The slide of chapter 06.** It describes the three outcomes of the callback from the code (a linked user, a user
    found by email, anyone else). The email match needs the identity option "RequireUniqueEmail" (on by default) and an
    email address in the claims Meta returns; the narration of `after-2` states the match and does not say which
    claims Meta returns. Confirm with a real Meta app that an email address is returned.

## Pronunciation (a person confirms each spelling with `scripts/pronounce.py` before recording)

The storyboard's `spoken` map adds three fixes. None was checked with the voice.

| Written | Spoken as | Why |
| --- | --- | --- |
| `CallbackPath` | `Callback Path` | One word with a capital inside; the voice may run it together |
| `/signin-facebook` | `slash sign in dash facebook` | A path; the voice may skip the slash or read "signin" as one word |
| `URI` | `U.R.I.` | The voice may say "your eye" or "yoori" |

"OAuth" is deliberately left out of the narration and the slides. The voice may also
need a fix for "Meta Login", "Facebook" and "SDK"; listen to the take before recording.

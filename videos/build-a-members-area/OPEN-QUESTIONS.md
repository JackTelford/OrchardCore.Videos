# Open questions: Build a Members Area

Everything in the narration comes from the source of `OrchardCMS/OrchardCore` at commit `92db4a83` and its
documentation. No site was run to write it. A person must confirm the points below on a running site before the
recording, and tell the author if one does not hold, because the narration says it as a fact.

## Things the narration relies on

1. **Anonymous and Authenticated start with "View all content".** Read from the default permissions of the Contents
   module (`OrchardCore.Contents/Permissions.cs:88-96`, applied by `OrchardCore.Roles/Services/RoleUpdater.cs`). Not seen on a
   Blank site. Open both roles in the role editor right after Auto Setup: the row "View all content" must be ticked in
   the Allow column. Step `idea-3` and `lock-1` say it.
2. **A signed-out visitor who opens a page they may not view is sent to the login page, and the login returns to that
   page.** Read from `OrchardCore.Contents/Controllers/ItemController.cs:36` (`ChallengeOrForbid`), the cookie login path
   (`OrchardCore.Users/Startup.cs:129`) and `OrchardCore.Users/Controllers/AccountBaseController.cs:31`. Steps `signup-2`
   and `member-2` say it. Confirm the login page shows the link "Register as a new user" in that situation.
3. **Registration with approval ends on "User Registration Pending", and the account is listed as Disabled.** Read from
   `UserService.RegisterAsync` (`IsEnabled = !UsersAreModerated`) and `UserModerationLoginFormEvent`. Steps `signup-4`,
   `approve-1`, `approve-2`.
4. **Registration does not give the new account a role.** `UserService.RegisterAsync` and `CreateUserAsync` add none.
   If a workflow or setting of the Blank site adds one, `approve-3` is wrong.
5. **The Member role takes effect at the member's first sign-in.** The role is assigned in `08-approve` before the
   member ever signs in, so the order avoids the question of a signed-in user whose role changes. If the clips are
   recorded in another order, check that the page opens for the member.
6. **Enabling Users Registration enables Email, and no warning appears when no mail provider is configured.** Read from
   the manifest (`OrchardCore.Users/Manifest.cs:47`); no filter or notifier in `OrchardCore.Email` warns. Step
   `register-2` also says that "Users must verify their email address" requires a configured email provider; that is
   from `src/docs/topics/security/registration.md` and from `EmailConfirmationRegistrationFormEvents`, which sends the
   confirmation email. It was not run.
7. **The accounts created through Google, GitHub or Meta follow the Registration settings.** Read from
   `OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:209,307` (both paths call
   `UserService.RegisterAsync`). The "External Authentication" tab of the Registration page appears when a provider
   feature is enabled (`ExternalRegistrationSettingsDisplayDriver.cs:44`, registered in `ExternalAuthenticationStartup`).
   Steps `external-1` and `external-2` say it. No provider was configured to check it.

## Things the recording plan relies on

8. **The seed recipe was not imported.** Its structure is copied from `blog.recipe.json` (content definition, content,
   themes, feature and HomeRoute settings). Confirm that JSON Import accepts it, that the default theme becomes the site
   theme, that `Welcome` is the home page, and that the link to `/members-lounge` appears on it.
9. **The ids and attributes used as selectors were read, not observed.** In particular
   `input[id='Checkbox.ViewContent']` and `input[id='Checkbox.View_Page']` (the dynamic permission names are built from
   the templates in `ContentTypePermissionsHelper.cs`), `a[id^='btn-enable-'][id*='Users_Registration']`, the row
   attribute `data-filter-value` of the roles list, `[role='btn-edit-MembersPage']`, and `#modalOkButton`. Check each one
   with `Recorder probe`.
10. **The body editor of Page and Members Page is the standard one** (no `Editor` setting in the seed), which is a code
    editor for HTML. Typing plain words must be accepted and shown as text on the page.
11. **The row actions of the Users list are in a menu titled "Actions"** (the "Buttons" layout shows a button, the "Menu"
    layout an ellipsis button with the same title). The plan assumes the button can be found by that title.
12. **Pages that end with a notification.** The plan expects the messages "Site settings updated successfully.",
    "Role updated successfully.", "User account was successfully enabled." and "User updated successfully." Their
    text is in the controllers; the narration does not read them.

## A decision for the maintainer

13. **The tracker note asks for a local SMTP catcher (smtp4dev) for this video.** The script keeps email verification
    off, so no mail is sent and no catcher is needed: a members area works with approval alone, and another video of the
    programme covers the confirmation email. If the maintainer wants the confirmation email shown here, the script needs
    a configured email provider, a catcher, and two more chapters; the video would also grow past eight minutes.

## Pronunciation

14. **The storyboard maps "Save & Configure" to "Save and Configure"** in `spoken`, so the voice does not read the
    ampersand. The skill asks a person to confirm a spelling with `scripts/pronounce.py` before it is used. Listen also
    to "Ok" (step `approve-2`, the label of the confirmation button; add `spoken` "O.K." if the voice says it as a word), "Securable", "Authenticated" and "Anonymous" (steps `type-2`, `idea-2`, `lock-2`) and add a `spoken` entry if the
    voice stumbles.

## Found while checking the source

15. **The login, register and pending pages are drawn with the admin theme's login layout.** Read from
    `OrchardCore.Users/Services/UsersThemeSelector.cs:37-86` (the theme selector returns the admin theme with a higher
    priority when the setting "Use site theme for registration page" or the login setting of the same name is off, and
    both are off in this video) and `OrchardCore.Themes/TheAdmin/Views/Layout-Login.cshtml`. Not seen on a running
    site. Look at `07-signup` and `09-member` once: if the pages look wrong next to the site theme of the Welcome
    page, tell the author before recording, because the narration does not mention a theme.
16. **Each of Google Authentication, GitHub Authentication and Meta Login adds a page under Settings, Security,
    Authentication.** Read from `OrchardCore.Google/GoogleAuthenticationAdminMenu.cs`, `OrchardCore.GitHub/AdminMenuGitHubLogin.cs`
    and `OrchardCore.Facebook/AdminMenuLogin.cs`. The slides `external-1` and `external-2` only name these pages; no
    provider is enabled while recording. The Meta Login page holds the callback path; the application id and secret
    of Meta are on a different page (Settings, Integrations, Meta App), which the narration does not name.
17. **Which roles open a Members Page after the clean-up** (`lock-4`). Read from the permission chain: View all content
    is implied by Edit content for others (`OrchardCore.Contents.Core/CommonPermissions.cs:29`), the permission of the
    type is implied by View all content and by Edit of the type (`ContentTypePermissionsHelper.cs:19`), and the role
    Editor receives Edit content for others by default (`OrchardCore.Contents/Permissions.cs:51-63`). Nothing to
    record; listed so a person who tries a role with only Edit own content knows it does not open the page.

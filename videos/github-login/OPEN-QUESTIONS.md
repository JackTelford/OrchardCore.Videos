# Open questions: github-login

What the author could not verify from the Orchard Core source (commit 92db4a83) and its documentation, and what a
person confirms on a running site before the narration is recorded. The narration states none of these as fact beyond
what the source shows; the question says what to check and which step to change if the answer differs.

## 1. The text of the GitHub button

The button on the login page is labeled with the display name of the authentication scheme, which the module takes from
`GitHubAuthenticationDefaults.DisplayName` of the `AspNet.Security.OAuth.GitHub` package
(`OrchardCore.GitHub/Configuration/AuthenticationOptionsConfiguration.cs:41`, `OrchardCore.Users/Views/Account/Login.cshtml:43`).
That package is not in the source tree read for this video.

- Check: on `/Login` of the recording site, the button reads **GitHub**, and its tooltip reads "Log in using your
  GitHub account".
- If it differs: change `login-1`, `login-2`, `return-3` (the narration says "offers GitHub", "the GitHub button"),
  and the selector `button[name='provider'][value='GitHub']` of `recording-plan.md`.

## 2. What the GitHub side looks like

The narration and the slide `register-1` say "Developer applications", "OAuth app" and "Authorization Callback URL".
These words come from the documentation page (`src/docs/reference/modules/GitHub/README.md`), not from a GitHub
screen, and GitHub is not recorded. Two points need a person with a GitHub account:

- Whether GitHub still calls the page "Developer applications" and the field "Authorization Callback URL", and that
  the Client ID and a client secret are both found in the app (`register-2`).
- Whether the app should be an OAuth app, as the documentation says, or a GitHub App, as the hint under **Client ID**
  says ("The Client ID defined in the general tab of your GitHub App",
  `OrchardCore.GitHub/Views/GithubAuthenticationSettings.Edit.cshtml:24`). The video follows the documentation.

## 3. Callback address of a tenant with a URL prefix

The narration says "the address of your site followed by /signin-github". The documentation writes
`[tenant]/signin-github`. The code gives the path relative to the application's base path
(`Configuration/GitHubAuthenticationOptionsConfiguration.cs:59,74-77`); that a tenant prefix is part of the address
is not verified. Check on a tenant with a prefix which address GitHub is sent back to.

## 4. The notification after enabling a feature

Enabling a feature shows a notification ("The feature {0} was enabled." or the plural form, listing every feature
enabled, `OrchardCore.Features/Controllers/AdminController.cs:313-317`). Enabling GitHub Authentication also enables
External Authentication, so the text may list both. The narration does not quote it. Check that it stays visible long
enough for the contact sheet of `enable-3`; if not, no narration changes.

## 5. The Dependencies badge

`enable-2` says the row lists External Authentication as a dependency. The source renders it as a badge in the list
layout of the Features page (the default layout is List and the default row actions are Buttons:
`OrchardCore.Admin.Abstractions/AdminListOptions.cs`; a site setting can override them) (`OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:11-17`). Check
that the recording site shows the list layout by default, and that the badge is visible next to the row; in the grid
layout the dependencies are a column and the selector `.direct-dependencies .badge` of `recording-plan.md` changes.

## 6. The admin menu and the Settings page on screen

Only the labels of the admin menu path are verified in the source
(`OrchardCore.GitHub/AdminMenuGitHubLogin.cs:25-28`). The selectors of `recording-plan.md` for the menu groups
(buttons inside `#left-nav`) and for the form fields (`GetByLabel`) are read from the views, not run. Run the recorder's
`probe` command on `/Admin` and on `/Admin/Settings/OrchardCore.GitHub.Authentication` and adjust the selectors, not
the narration. Also check at 1536x864 whether **Store tokens** and **Save** are visible without scrolling
(`config-5`, `config-6`).

## 7. Saving the settings restarts the tenant

The page warns that the website might restart (`GithubAuthenticationSettings.Edit.cshtml:7`) and the driver requests a
release of the shell (`Drivers/GithubAuthenticationSettingsDisplayDriver.cs:92`). Check how long the page after **Save**
takes to load, so that `config-6` ends on the saved state, and that the notification **Site settings updated
successfully.** is visible then (`OrchardCore.Settings/Controllers/AdminController.cs:119`).

## 8. Who can open the page

The menu item needs the permission **Manage GitHub Authentication settings**, which the Administrator role has
(`OrchardCore.GitHub/Permissions.cs:8,20-24`). The settings page also asks for the **Manage settings** permission of the
Settings module, because the GitHub group registers no permission of its own
(`OrchardCore.Settings/Controllers/AdminController.cs:53`). So a role with only the GitHub permission sees the menu
item, and whether it can open the page is unverified. The narration does not mention permissions.

## 9. Pronunciation

Check with `scripts/pronounce.py` before narrating: the spoken form "slash sign in dash GitHub" for `/signin-github`
(the `spoken` entry of `storyboard.json`), and the words "OAuth", "GitHub", "Client ID" and "CallbackPath".

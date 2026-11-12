# Open questions: X (Twitter): Sign In With X and Post From Workflows

What was checked in the source of Orchard Core (commit `92db4a83`) and the documentation page
`src/docs/reference/modules/X/README.md`, and what a person must confirm on a running site before the clips are
recorded. The narration keeps to what the source shows; every item below is kept out of it, or written so that it holds
either way.

## 1. Does the module still work against X?

Sign in and posting are not exercised by this video, and the narration never says they succeed.

- Sign in uses the handler of the `Microsoft.AspNetCore.Authentication.Twitter` package (OAuth 1.0a request and access
  tokens), configured in `OrchardCore.Modules/OrchardCore.Twitter/Signin/Configuration/TwitterOptionsConfiguration.cs`.
- Posting uses `TwitterClient.UpdateStatus` (`OrchardCore.Modules/OrchardCore.Twitter/Services/TwitterClient.cs`), which
  sends a form post to the status-update endpoint of version 1.1 of the X API, signed by
  `TwitterClientMessageHandler.cs`.

To confirm, with a real X developer app of your own and a throwaway site (not for the recording): does the sign-in
round trip complete, and does the task post? Which access level of the X API does the app need? The answer decides
whether `docs.orchardcore.net` can keep describing both features without a caveat. The narration of steps `login-2`
and `flow-7` only says what the module sends or where it sends the visitor.

## 2. The default callback path

The documentation and the placeholder of the CallbackPath field both say `/signin-x`
(`OrchardCore.Modules/OrchardCore.Twitter/Views/TwitterSigninSettings.Edit.cshtml:22`). The code sets the path only
when the field has a value (`TwitterOptionsConfiguration.cs`, the `CallbackPath.HasValue` test), so an empty field
leaves the default of the handler, and the handler's default is `/signin-twitter` (the string is in the assemblies of
`Microsoft.AspNetCore.Authentication.Twitter` 8.0.0 and 10.0.13 found in a NuGet cache; the version Orchard Core pins
was not readable, since `Directory.Packages.props` is not part of the source copy).

The video avoids the question: step `signin-2` types `/signin-x` explicitly and says to register the same path in the
X app, and the narration names no default.

To confirm on a running site with the four keys saved and CallbackPath left empty: `curl -i http://localhost:5010/signin-x`
and `curl -i http://localhost:5010/signin-twitter`. The path the handler owns answers with something other than 404.

## 3. Pronunciation

`storyboard.json` has two `spoken` entries: `CallbackPath` is said "callback path", and `/signin-x` is said "slash sign in x".
Check both, and the reading of "X (Twitter)" with its parentheses (heard in almost every step), with
`scripts/pronounce.py` before narrating. Change the spelling if a voice reads them badly.

## 4. The login page while signed in (`login-1`)

`AccountController.Login` is anonymous and, for a signed-in user, only clears the return URL
(`OrchardCore.Modules/OrchardCore.Users/Controllers/AccountController.cs:75-80`), so the page should render for the
recorder's `admin` session. Confirm that the "Use another service to log in" section shows. If the build redirects,
use a browser context without the saved sign-in for this clip, as the plan says.

## 5. Pages and selectors that come from views, not from a running site

All labels were read in the source. These locators were read from views and scripts and not run:

- The admin menu: `#adminMenu`, how a group opens, whether a group stays open from one page to the next, and the
  nesting Settings > Integrations > X (Twitter) and Settings > Security > Authentication > Sign In with X (Twitter).
  The menu is rendered by the admin theme, which was not read. If a click on a group is not needed (already open),
  skip it.
- The Features page: `.list-group-item` rows (the List layout is the default; a Grid layout draws the same page
  differently), `.direct-dependencies`, and the href of the Enable link
  (`a[href*="/Features/OrchardCore.Twitter.Signin/Enable"]`). The search is applied by a script; its source
  (`OrchardCore.Modules/OrchardCore.Features/Assets/ts/features-index.ts`) matches the typed text against the name, the
  id and the description of each row, so `Twitter` lists the two X (Twitter) rows only. The Enable link is a plain link
  to a route that accepts POST only (`AdminController.cs:226`, `[HttpPost]`) and carries `data-url-af="UnsafeUrl"`:
  confirm that a click on it enables the feature.
- The login button: its `value` is the name of the scheme (`Twitter`); only its text, "X (Twitter)", is certain.
- The workflow designer: the `data-cy` attributes come from the Vue sources under
  `OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/`. The shared components of the `@bloom` package
  (the save status text, the form host that applies a setting when the field loses the focus) are not in the source copy:
  the documentation says settings are applied when you leave a field, and the toolbar text "All changes saved" comes from
  `WorkflowsDesignerJSLocalizer.cs:166`. The designer source shows that the panel opens by itself when an activity with
  settings is added (`Assets/designer/src/App.vue:273-275`) and that the plus of an unconnected outcome is drawn on hover
  (`Assets/designer/src/styles/designer.css:1475-1476`). Confirm that a click on the "Done" outcome, after the hover,
  opens the menu "Add the next activity, after Done" (step `flow-4`: the narration says to point at the outcome until a
  plus appears and to select it), and that the issue count leaves the task after Tab.
- The Content Types picker of the event is a `bootstrap-select` list rendered in the page body
  (`data-container="body"`): confirm the locators of step `flow-3`.
- The text of the toast after Publish contains the number of the published version; the narration does not.

## 6. The email of the X account (`first-2`)

The step says "if X provides an email that matches a local account". The handler is asked to retrieve user details
(`RetrieveUserDetails = true`), but whether X returns an email depends on the app and on X. Nothing in the video
depends on it.

The branch without a page is read from the code and not run: with "Do not ask username", "Do not ask email address" and
"Do not create local password" all selected in the external registration settings, the site creates the account and
signs it in without showing the page Create a new account
(`OrchardCore.Modules/OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:201-205`). The narration of
`first-2` says so in general terms ("create accounts without asking"). If that registration fails (for example, when the
email is missing and the site needs one), the code shows the page Create a new account with the error instead; the
video does not cover it.

## 6b. The notification on the login page (slide of `first-2`)

The fourth card says the site reports "New registrations are disabled for this site." That text is an error
notification added before the redirect to the login page (`ExternalAuthenticationsController.cs:182`). Whether the
login layout of the theme shows notifications was not read, so the card says "The site reports" and the narration does
not mention the message. To confirm, with an X developer app of your own and a throwaway site: select "Disable new
external registration", return from X with an account that has no local account, and look at the login page.

## 7. Notes for the maintainers (not in the video)

- `TwitterSettingsDisplayDriver.EditAsync` shows the integration settings to users with the `ManageTwitterSignin`
  permission, while `UpdateAsync` requires `ManageTwitter`
  (`OrchardCore.Modules/OrchardCore.Twitter/Drivers/TwitterSettingsDisplayDriver.cs`, the two `AuthorizeAsync` calls). The
  `admin` role has both, so this does not show in the video.
- A network failure while the task posts is rethrown by `TwitterClient.UpdateStatus`, so it faults the workflow
  instead of following the Failed outcome. The narration of `flow-6` says only that Failed follows when X returns an error.
- The recipe step documented for the settings stores `ConsumerSecret` and `AccessTokenSecret` as given, while the code
  that reads them expects values protected with Data Protection (see the docIssues of the task result).

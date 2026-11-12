# Recording plan: Meta Login

For a person who records this video without having to decide anything. In this file a selector such as
`button.primaryAction.save` stands for `scene.Page.Locator("button.primaryAction.save")`. Paths such as
`OrchardCore.Facebook/AdminMenu.cs:27` are in the Orchard Core source (`src/OrchardCore.Modules/` unless the path says
otherwise), at the commit the narration was written against. Text between quotation marks is what the screen shows.

The video has four recordings (`02-enable`, `03-app`, `04-login`, `05-signin`), two slide chapters (`01-need`,
`06-after`) and the two cards. Meta is never contacted: **no clip clicks the Facebook button.**

## The site it needs

| What | Value |
| --- | --- |
| Orchard Core | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010` |
| App data | `ORCHARD_APP_DATA` = an empty folder outside both repositories |
| Auto Setup | tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, recipe `Blog`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`) |
| Auto Setup variables | `OrchardCore__AutoSetup__Tenants__0__ShellName`, `__SiteName`, `__SiteTimeZone`, `__AdminUsername`, `__AdminEmail`, `__AdminPassword`, `__DatabaseProvider` (Sqlite), `__RecipeName` (`Blog`), as in the Auto Setup documentation |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address for the site process |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` saves `state.json` (git-ignored) |
| Time of day | no clock is visible in any clip |

**Features.** Enable nothing before recording. The Blog recipe leaves Meta Login, Meta Core Components, External
Authentication and Secrets disabled, and that is the state the first clip starts from. Check it before the first take:
on `Tools > Features`, the row of Meta Login has an Enable button, and the Secrets feature is not enabled.

**Settings that must keep their defaults.**

- Secrets stays disabled. With it enabled, Secret Key shows a richer editor than the plain password box the
  narration describes (`OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:96`).
- `Settings > Security > User Login`: "Use external provider for login." stays cleared
  (`OrchardCore.Users/Views/ExternalLoginSettings.Edit.cshtml:15`; it is cleared unless something selects it: the setting defaults to false,
  `OrchardCore/OrchardCore.Users.Core/Models/ExternalLoginSettings.cs:5`, and the data migration also starts it at false,
  `OrchardCore.Users/DataMigrations/ExternalAuthenticationMigrations.cs:44`). When it is selected and one provider exists, the
  login page sends the browser straight to Meta (`OrchardCore.Users/Services/ExternalLoginFormEvents.cs:38`).

**External services.** Meta (Facebook) is the only one. It has no local stand-in and is never called: the values
entered are made up, and the clip that shows the Facebook button stops before selecting it.

**Seed data.** None. The only values are typed on camera:

| Field | Value |
| --- | --- |
| Application Identifier | `123456789012345` |
| Secret Key | `fake-secret-for-video` (a password field: it shows dots) |

## Signing the recorder in

This video has no `PrepareAsync`: nothing is seeded. Once Auto Setup has finished and the site answers on
`http://localhost:5010`, build the recorder with this video's folder and save the session:

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/meta-login/recorder
export VIDEO_ROOT=<repo>/videos/meta-login VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
<bin>/Recorder setup
```

Then stop the site and copy its `App_Data` aside as snapshot S0. Narrate with `scripts/narrate.py videos/meta-login` and
record the clips in the order below.

## Clip order, dependencies and snapshots

Record in this order, which is also the order they play in.

| Clip | Needs | Changes the site | Starts from snapshot |
| --- | --- | --- | --- |
| `02-enable` | a fresh site | yes: enables Meta Login, Meta Core Components, External Authentication | S0 |
| `03-app` | `02-enable` | yes: saves the Application Identifier and the Secret Key | S1 |
| `04-login` | `02-enable` | no (nothing is saved) | S2 |
| `05-signin` | `03-app` | no (nothing is clicked), and signed out | S2 |

Snapshots (stop the site before copying `App_Data`, keep `state.json` with each):

- **S0**: after Auto Setup and `Recorder setup`, before any clip.
- **S1**: after `02-enable`.
- **S2**: after `03-app`.

To record `02-enable` again, restore S0. To record `03-app` again, restore S1 (from S2 the fields are already filled and
Secret Key shows the stored-value message, which the first steps must not show). `04-login` and `05-signin` can be
recorded again from S2 as often as needed.

Saving either settings page, and enabling a feature, restarts the tenant at the end of the request
(`OrchardCore.Facebook/Drivers/FacebookSettingsDisplayDriver.cs:98`; the notice at
`OrchardCore.Facebook/Views/FacebookSettings.Edit.cshtml:7`). The page that follows takes a moment to load: wait for it
with the conditions given below.

## Selectors used by several clips

| What | Selector | Where the label comes from |
| --- | --- | --- |
| Menu group toggle | `button.item-label:has(> span.title:text-is("Tools"))`, and the same with `"Settings"`, `"Integrations"`, `"Security"`, `"Authentication"` | `OrchardCore.Settings/AdminMenu.cs:32` ("Tools") and `:36` ("Settings"), `OrchardCore.Facebook/AdminMenu.cs:26`, `AdminMenuLogin.cs:26-27`; an item with children and no page of its own renders as a `button.item-label` (`OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml`, `isToggleOnlyItem`) whose text is in a `span.title` (`NavigationItemText.cshtml`). `button.item-label:text-is("Tools")` alone matches nothing: the `span.title` inside the button has the same text, so Playwright does not count the button itself as the smallest element holding it |
| Features page link | `a.item-label[href$="/Admin/Features"]` ("Features") | `OrchardCore.Features/AdminMenu.cs:27` |
| Meta App page link | `a.item-label[href$="/Admin/Settings/OrchardCore.Facebook"]` ("Meta App") | `OrchardCore.Facebook/AdminMenu.cs:27` |
| Meta page link | `a.item-label[href$="/Admin/Settings/OrchardCore.Facebook.Login"]` ("Meta") | `OrchardCore.Facebook/AdminMenuLogin.cs:28` |
| Feature search | `#search-box` | `OrchardCore.Features/Views/Admin/Features.cshtml:27` |
| Feature row | `.list-group-item` that has `#btn-enable-OrchardCore_Facebook_Login, #btn-disable-OrchardCore_Facebook_Login` (the same `Row` helper as the example clip of the recorder) | `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` |
| Enable button | `#btn-enable-OrchardCore_Facebook_Login` ("Enable") | same file, line 40 |
| Disable button | `#btn-disable-OrchardCore_Facebook_Login` ("Disable") | same file, line 53 |
| Application Identifier | `input[name$="AppId"]` | `OrchardCore.Facebook/Views/FacebookSettings.Edit.cshtml:20` |
| Secret Key | `input[type="password"]` (the only one on the page) | `FacebookSettings.Edit.cshtml:29`; `SecretInputTagHelper.cs:96` |
| Save | `button.primaryAction.save` ("Save") | `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` |
| Success message | `.toast.message-success`, text "Site settings updated successfully." | `OrchardCore.Settings/Controllers/AdminController.cs:119` |
| CallbackPath | `input[name$="CallbackPath"]` | `OrchardCore.Facebook/Views/FacebookLoginSettings.Edit.cshtml:20` |
| Store tokens | `input[type="checkbox"][name$="SaveTokens"]` (the check box also has a hidden input of the same name) | same file, line 32 |
| Facebook button | `button[name="provider"][value="Facebook"]` | `OrchardCore.Users/Views/Account/Login.cshtml:43` |

The ids and selectors above come from reading the views, not from a running site. Confirm each one with
`Recorder probe` before the first take, for example
`Recorder probe /Admin/Features "() => !!document.querySelector('#btn-enable-OrchardCore_Facebook_Login')"` (set
`MSYS_NO_PATHCONV=1` in Git Bash). The list layout of the Features page is the one recorded, with the actions as
buttons; both are the defaults (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30` and `:36`), so leave
the admin list settings alone. The Grid layout names its rows differently.

Before every clip that opens the admin menu, make sure no group is expanded: a scene starts in a new browser context, so
the menu starts collapsed. Expanding the groups is part of the story in these clips, so it happens after
`StartCaptureAsync`.

## Chapter 00 and chapter 99: the cards

- `intro`: the opening card, no recording.
- `outro`: the closing card, no recording.

## Chapter 01 (slides)

- `need-1`: three cards, "The visitor selects Facebook", "The visitor signs in at Meta", "Meta sends the visitor back".
- `need-2`: four cards under "What you need from Meta"; "Application ID" and "Application secret" are bright.
- `need-3`: the same four cards; "Meta Login product" and "Redirect URI" are bright.

## Chapter 02: clip `02-enable`

Page it starts on: `/Admin` (the dashboard), signed in, opened with `OpenAsync` before `StartCaptureAsync`. Starts from
S0. Changes the site.

`enable-1`

- Click the menu group toggle "Tools" (pauseAfter 400), then `ClickAndWaitForNavigationAsync` on
  `a.item-label[href$="/Admin/Features"]`.
- Wait until `#search-box` is visible, then `TypeAsync(#search-box, "Meta Login", delay: 90)`.
- On screen when the narration ends: the Features page with "Meta Login" in the search box, one group headed "Meta"
  with one row, "Meta Login", under it, the row's description "Authenticates users from Meta." and its Enable button.
  Highlight the description (`.admin-list-secondary` inside the row) with no caption.
- Labels: "Tools" and "Features" at `OrchardCore.Features/AdminMenu.cs:26-27`; the group heading at
  `OrchardCore.Features/Views/Admin/Features.cshtml:88`; the description at `OrchardCore.Facebook/Manifest.cs:24`.
- Check on the first take that the search finds the row: the search matches the row's `data-filter-value`
  (`OrchardCore.Features/Assets/ts/features-index.ts`, `applyFilters`). If another feature also matches "Meta Login",
  the narration is wrong and the take must be reported, not recorded.

`enable-2`

- `ClearHighlightsAsync`, `HighlightAsync(Row("OrchardCore_Facebook_Login"))` with no caption, then `MoveToAsync` the
  Enable button without clicking it.
- On screen when the narration ends: the Meta Login row with a green ring, the cursor on its Enable button. The row
  shows two badges after its name, "External Authentication" and "Meta Core Components", which are its direct
  dependencies (`OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:13-18`). The rows of those two
  features are hidden on this page, because both are enabled only by dependency ("Hide on demand" in
  `OrchardCore.Features/Views/Admin/Features.cshtml:65`).
- Labels: "Enable" at `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the dependencies at
  `OrchardCore.Facebook/Manifest.cs:25-29`; the names "Meta Core Components" at `Manifest.cs:14` and "External
  Authentication" at `OrchardCore.Users/Manifest.cs:24`. Enabling a feature also enables the dependencies that are not
  enabled yet (`OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:163-168`; the Enable action passes
  `force` as true).

`enable-3`

- `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_Facebook_Login)`. Enabling
  restarts the tenant, so the page that follows can take several seconds. Wait for the Features page to load
  (`#search-box` visible), then `TypeAsync(#search-box, "Meta Login", delay: 60)` and
  `HighlightAsync(#btn-disable-OrchardCore_Facebook_Login)`.
- On screen at the start of the narration: the success message, a toast at the right edge of the page: "The feature
  Meta Login was enabled." It names only the feature whose button was selected, not the two dependencies enabled with
  it (the list handed to the message is the requested feature, `OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:79-80`).
  The toast hides itself after five seconds (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`),
  so it is gone when the step ends. On screen when the narration ends: the Meta Login row with the button "Disable"
  ringed.
- If the click, the restart and the reload take longer than the narration (about seven seconds), the message is missed.
  Record it again with the wait wrapped in `scene.OffCameraAsync(() => scene.Page.WaitForSelectorAsync(".toast.message-success"))`
  after the click.
- Labels: the message at `OrchardCore.Features/Controllers/AdminController.cs:317` (the singular form, for one feature); "Disable" at
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53` (the `btn-disable-` anchor, which also carries the label "Disable").

After this clip, stop the site, copy `App_Data` and `state.json` as S1.

## Chapter 03: clip `03-app`

Page it starts on: `/Admin`, signed in, menu collapsed. Starts from S1. Changes the site (saves the settings).

`app-1`

- Click the toggle "Settings" (pauseAfter 400), click the toggle "Integrations"
  (pauseAfter 400), then `ClickAndWaitForNavigationAsync` on `a.item-label[href$="/Admin/Settings/OrchardCore.Facebook"]`.
- On screen when the narration ends: the page with the heading "Meta Integration", the warning notice at the top ("The
  website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness during the
  process.") ringed with `HighlightAsync(p.alert.alert-warning)`, and under it the fields "Application Identifier" and
  "Secret Key". The menu shows the path Settings, Integrations, Meta App.
- Labels: `OrchardCore.Facebook/AdminMenu.cs:25-27`; "Meta Integration" at `FacebookSettings.Edit.cshtml:15`; the
  notice at line 7.

`app-2`

- `ClearHighlightsAsync`, then `TypeAsync(input[name$="AppId"], "123456789012345", delay: 60)`.
- On screen when the narration ends: "Application Identifier" (marked required) with `123456789012345` in the field.
- Label: `FacebookSettings.Edit.cshtml:20` (the `ocat-label-required` class marks it required; the model has `[Required]`,
  `OrchardCore.Facebook/ViewModels/FacebookSettingsViewModel.cs:8`).

`app-3`

- `TypeAsync(input[type="password"], "fake-secret-for-video", delay: 40)`, then
  `HighlightAsync(input[type="password"])` with no caption.
- On screen when the narration ends: "Secret Key" with a row of dots in the field, ringed.
- Label: `FacebookSettings.Edit.cshtml:29`. The field is the plain password box because Secrets is disabled
  (`SecretInputTagHelper.cs:96`).

`app-4`

- `ClearHighlightsAsync`, `ScrollAsync(0, 300)` so that "Javascript SDK Version" is near the top of the frame, then
  `HighlightAsync(input[name$="Version"])`, `HighlightAsync(input[name$="SdkJs"])` and
  `HighlightAsync(input[type="checkbox"][name$="FBInit"])` (the "Init on every page" check box), with no captions. Do not highlight the
  text area "Parameters for the FB.init() call", and do not type in any of the four fields.
- On screen when the narration ends: the three fields ringed, with their default values. The text area under them is
  probably in the frame as well; see `OPEN-QUESTIONS.md`.
- Labels: `FacebookSettings.Edit.cshtml:37`, `:46`, `:58`. Why the narration says Meta Login does not use them:
  `OrchardCore.Facebook/Login/Configuration/FacebookLoginConfiguration.cs:46-104` reads only the application
  identifier, the secret, CallbackPath and Store tokens.

`app-5`

- `ClearHighlightsAsync`, `ScrollAsync(0, 700)` until `button.primaryAction.save` is visible, then
  `ClickAndWaitForNavigationAsync(button.primaryAction.save)`. Wait until `.toast.message-success` is visible. The
  restart makes this wait longer than a normal page load. Then `ScrollAsync(0, -2000)` and
  `HighlightAsync(input[type="password"])`.
- On screen at the start of the narration: the success message "Site settings updated successfully." (it hides itself
  after five seconds). On screen when the narration ends: the Meta Integration page, the Application Identifier still
  holding `123456789012345`, and the Secret Key field empty with the text "The value is securely stored. Enter a new
  value to update it, or leave it blank to keep it." as its placeholder, ringed.
- Labels: "Save" at `SiteSettings.SaveButton.cshtml:2`; the message at `OrchardCore.Settings/Controllers/AdminController.cs:119`;
  the placeholder at `SecretInputTagHelper.cs:103`.
- If the wait and the reload outlast the narration (about eight seconds), wrap the wait in
  `scene.OffCameraAsync(() => scene.Page.WaitForSelectorAsync(".toast.message-success"))`.

After this clip, stop the site, copy `App_Data` and `state.json` as S2.

## Chapter 04: clip `04-login`

Page it starts on: `/Admin`, signed in, menu collapsed. Starts from S2. Changes nothing: do not click Save.

`login-1`

- Click the toggle "Settings" (pauseAfter 400), then "Security", then "Authentication" (pauseAfter
  400 each), then `ClickAndWaitForNavigationAsync` on `a.item-label[href$="/Admin/Settings/OrchardCore.Facebook.Login"]`.
- On screen when the narration ends: the page with the heading "Login Settings", the fields "CallbackPath" and "Store
  tokens", and at the bottom the line "Meta Integration settings must be configured in order to use the Login Feature"
  with its link; the link is ringed with `HighlightAsync(span.hint a)` (the last `span.hint` of the page).
- Labels: `OrchardCore.Facebook/AdminMenuLogin.cs:25-28`; "Login Settings" at `FacebookLoginSettings.Edit.cshtml:15`; the
  line at `:40`.

`login-2`

- `ClearHighlightsAsync`, `HighlightAsync(input[name$="CallbackPath"])`, `MoveToAsync` the field. Do not type.
- On screen when the narration ends: "CallbackPath" with an empty field showing the placeholder `/signin-facebook`,
  ringed.
- Labels: `FacebookLoginSettings.Edit.cshtml:20` (label) and `:22` (placeholder).

`login-3`

- `ClearHighlightsAsync`, `HighlightAsync(input[type="checkbox"][name$="SaveTokens"])`, `MoveToAsync` the check box. Do not click it.
- On screen when the narration ends: the check box "Store tokens" cleared and ringed, with its hint "Store external
  authentication tokens for reuse".
- Labels: `FacebookLoginSettings.Edit.cshtml:32-33`.

## Chapter 05: clip `05-signin`

Page it starts on: `/` (the home page of the Blog), **signed out**. Create this scene with a storage state path that
does not exist, for example `Scene.StartAsync(browser, name, BaseUrl, Path.Combine(Scenes.Root, "signed-out.json"), outputRoot, durations)`:
the scene then has no session (`Scene.cs:109`, `File.Exists(storageStatePath) ? storageStatePath : null`). Never create
that file. Starts from S2. Changes nothing.

`signin-1`

- `NavigateAsync("/Login")`. Wait until the heading "Use another service to log in" is visible.
- On screen when the narration ends: the login page with two sections side by side, "Log in" (Username or email
  address, Password) and "Use another service to log in".
- Labels: `OrchardCore.Users/Views/Account/Login.cshtml:27` and `:38`; `OrchardCore.Users/Views/LoginFormCredentials.cshtml`.
- If the browser leaves the site instead of opening the page, "Use external provider for login." is selected: clear it
  and record again from S2.

`signin-2`

- `HighlightAsync(button[name="provider"][value="Facebook"])` with no caption, and `MoveToAsync` the button without
  clicking it.
- On screen when the narration ends: a single button labeled "Facebook" (not "Meta") under "Use another service to log
  in", ringed.
- Label: `Login.cshtml:43`; the label "Facebook" comes from `FacebookLoginConfiguration.cs:65`; the button is listed only
  once the Application Identifier and the Secret Key are saved (`FacebookLoginConfiguration.cs:46-56`).

`signin-3`

- Keep the ring and the cursor on the button. `PauseAsync(1200)`. **Do not click.**
- On screen when the narration ends: the same frame as `signin-2`. Selecting the button posts to
  `ExternalAuthentications/ExternalLogin`, which challenges the provider and redirects to Meta
  (`OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:83`); that page is never recorded.

## Chapter 06 (slides)

- `after-1`: three cards, "Already linked", "Email matches a local user", "Anyone else"; the first is bright
  (`ExternalAuthenticationsController.cs:113-139`; the login checks that can stop the sign-in, such as a disabled
  account, are the `ValidatingLoginAsync` calls at `:127` and `OrchardCore.Users/Handlers/DisabledUserLoginFormEvent.cs:36`).
- `after-2`: the same cards, the second is bright (`ExternalAuthenticationsController.cs:150-175`; heading "Link your
  account." at `LinkExternalLogin.cshtml:8`).
- `after-3`: the same cards, the third is bright, "Registered as a user, unless external registrations are disabled" (`ExternalAuthenticationsController.cs:178-268`: the registration page appears unless the three options "Do not ask username", "Do not ask email address" and "Do not create local password" are all selected, `:201-205`; `OrchardCore.Users/Views/ExternalRegistrationSettings.Edit.cshtml:34`, `:45`, `:56`; with "Disable new external registration" (`ExternalRegistrationSettings.Edit.cshtml:23`) selected the visitor gets the message "New registrations are disabled for this site." and goes back to the login page, `:180-187`. A site whose Registration feature is not enabled when the Users feature is first enabled starts with that option selected, `OrchardCore.Users/DataMigrations/ExternalAuthenticationMigrations.cs:32`; the Blog site of this video is such a site, and no clip changes the option).
- `after-4`: three other cards, "Registration options", "Login options", "External Logins"
  (`OrchardCore.Users/RegistrationAdminMenu.cs:28`, `OrchardCore.Users/AdminMenu.cs:42`,
  `OrchardCore.Users/Views/UserMenuItems-ExternalLogins.cshtml:3`). The page behind "External Logins" lists the linked
  providers with a "Remove" button (shown when the user has a password or more than one login, `ExternalAuthenticationsController.cs:430`;
  `ExternalLogins.cshtml:12` and `:26`), and the providers that can still be linked under "Add another service to log in."
  as buttons named after the provider, here "Facebook" (`ExternalLogins.cshtml:42` and `:49`).

## Checks for private data

- Every value on screen is made up: `123456789012345`, the dots of the Secret Key field, and the site name "Orchard
  Core". No clip shows an email address, a real name or a real domain. The account used to sign in is `admin`.
- The Secret Key field is a password field: confirm by OCR that no clip shows its text in clear.
- No clip loads a page of Meta. In the network log of each take, no request goes to a host other than the site's own.
- `state.json`, any `signed-out.json`, `App_Data` copies and the password never go into git. `VIDEO_PASSWORD` is not typed
  on camera.
- Run the OCR scan of `SKILL.md` step 6 (one frame per second, every scene change, five frames per second) before
  publishing, and search for email addresses and the strings "secret", "token" and "password".

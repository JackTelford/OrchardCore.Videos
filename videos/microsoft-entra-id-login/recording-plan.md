# Recording plan: Microsoft Entra ID: Sign In With Work Accounts

How to record this video without deciding anything. Where this plan writes a selector, it stands for
`scene.Page.Locator(...)`. A label comes from the Orchard Core source at the file and line given, so check a label there
before changing it. `<password>` is the password chosen outside git and passed in `VIDEO_PASSWORD`; it is never written
here, and it is never typed on camera (the clips use the session saved by `Recorder setup`).

Of the ten chapters, three are recordings (04, 05, 06), two are cards (00 and 99), and five are slides (01, 02, 03, 07,
08), drawn by `slides.py`; the cards and the slides need no site. Nothing in this video clicks the sign-in button of the login page (see "Do not click the sign-in button").

## The site it needs

- Build and run `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout on `http://localhost:5010`, with its
  own empty `App_Data` outside both repositories (`ORCHARD_APP_DATA`). See `references/recording.md` and the
  `orchardcore-tester` skill of that checkout.
- Auto Setup provisions the Default tenant, so no setup form and no sign-in form are recorded:
  - `OrchardCore__AutoSetup__Tenants__0__ShellName` = `Default`
  - `OrchardCore__AutoSetup__Tenants__0__SiteName` = `Orchard Core`
  - `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` = `America/Los_Angeles`
  - `OrchardCore__AutoSetup__Tenants__0__AdminUsername` = `admin`
  - `OrchardCore__AutoSetup__Tenants__0__AdminEmail` = `admin@example.com`
  - `OrchardCore__AutoSetup__Tenants__0__AdminPassword` = `<password>`
  - `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` = `Sqlite`
  - `OrchardCore__AutoSetup__Tenants__0__RecipeName` = `Blog`
- Block outbound calls: set `HTTP_PROXY` and `HTTPS_PROXY` of the site's process to a dead address. The site never needs
  to call Microsoft in this video.
- Features to enable before recording: none. The Blog recipe enables `OrchardCore.Users` and `OrchardCore.Features`, which is
  all the clips need. `OrchardCore.Microsoft.Authentication.AzureAD` and `OrchardCore.Users.ExternalAuthentication` must
  be disabled when clip `04-enable` starts, because that clip enables them on camera. Check it first: on
  `/Admin/Features`, the row of Microsoft Entra ID (Azure Active Directory) Authentication shows an Enable button.
- Settings to save before recording: none. Clip `05-settings` types them on camera.
- External services: none. The Azure portal is not recorded (its chapters are slides), no tenant of Microsoft Entra ID is
  used, and no stand-in is needed.
- Seed data: none beyond the `admin` user that Auto Setup creates. The values typed on camera are made up:

| Field | Value |
|---|---|
| Display Name | `Maple Works Microsoft account` |
| AppId | `11111111-2222-3333-4444-555555555555` |
| TenantId | `66666666-7777-8888-9999-000000000000` |
| CallbackPath | left empty |
| Store tokens | left cleared |

- `Recorder setup` signs in once (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`) and writes `state.json`, which
  stays out of git. The video has no `PrepareAsync` and needs no mask script, since the site is made from scratch.

## Do not click the sign-in button

Selecting the button posts to `ExternalLogin`, which challenges the Microsoft Entra ID scheme
(`src/OrchardCore.Modules/OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:83`). The handler then loads
the metadata of the authority built from the TenantId (`OpenIdConnectOptionsConfiguration.cs:35` of
`OrchardCore.Microsoft.Authentication`) before it redirects. With outbound calls blocked and a made-up tenant, that fails,
and the browser would show an error page instead of Microsoft. So the clip stops on the login page: it highlights and
hovers the button, never clicks it.

## Order, dependencies and snapshots

1. Start the site from an empty `App_Data`, run `Recorder setup`, stop the site, and copy `App_Data` and `state.json`
   aside. This is the snapshot "before 04". Start the site again.
2. Record `04-enable`. It changes the site: it enables two features, and the tenant restarts.
3. Record `05-settings`. It depends on 04 (the settings page exists only when the feature is enabled), and it changes the
   site: it saves the settings, and the tenant may restart.
4. Record `06-login`. It depends on 05 (the button exists only when the settings are saved). It changes nothing.

Before a new take of 04, or of 04 and 05 together, stop the site, restore the snapshot "before 04", and start it again.
To record 05 again alone: restore the snapshot, enable the feature by hand outside the capture (the same Enable button on
`/Admin/Features`), and record 05. To record 06 again, nothing needs restoring while the settings of 05 are saved.

The slide chapters (01, 02, 03, 07, 08) are made from `slides.py` and are not recorded.

## Clip `04-enable` (chapter 04, Enable the feature)

Starts on `/Admin`, opened with `scene.OpenAsync("/Admin")` before `StartCaptureAsync()`. The admin menu is collapsed.
Nothing on that page may show data other than the fake data of the Blog recipe.

- `enable-1`
  - Click the menu group "Tools": `#adminMenu button.item-label` whose text is exactly "Tools"
    (`src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26`). Then
    `ClickAndWaitForNavigationAsync` on the link `#adminMenu a.item-label` whose text is exactly "Features"
    (`AdminMenu.cs:27`); the page is `/Admin/Features`.
  - `TypeAsync` into `#search-box` (placeholder "Search", `Views/Admin/Features.cshtml:27`) the text `Entra ID`, with
    `delay: 70`.
  - Wait until `#list-summary` starts with "Showing 1 of" (`Features.cshtml:56-59`): the search matches the name, the id
    and the description of a feature, and only this feature contains "Entra ID" (`Manifest.cs:25-27`).
  - `HighlightAsync` the row of the feature: the `.list-group-item` that holds `#btn-enable-OrchardCore_Microsoft_Authentication_AzureAD`
    (the example scene of `Scenes.cs` builds the same kind of `Row` helper).
  - On screen when the narration ends: the search box with "Entra ID", the heading "Microsoft Authentication"
    (`Features.cshtml:88`), and one row, "Microsoft Entra ID (Azure Active Directory) Authentication" with the id
    `OrchardCore.Microsoft.Authentication.AzureAD`, the description "Authenticates users with their Microsoft Entra ID
    Account.", a dependency badge "External Authentication" beside the name (`Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml`,
    which lists the direct dependencies), the status "Disabled" and an Enable button, ringed in green. The row of External
    Authentication itself is not in the list, since the filter "Hide on demand" is selected by default
    (`Features.cshtml:65`) and the feature is enabled by dependency only (`OrchardCore.Users/Manifest.cs:22-32`).
  - Label sources: "Tools" and "Features" in `Features/AdminMenu.cs:26-27`; the feature name, category and description in
    `Microsoft.Authentication/Manifest.cs:25-27`.
- `enable-2`
  - `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync` on `#btn-enable-OrchardCore_Microsoft_Authentication_AzureAD`
    (the link "Enable", `Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`). Enabling restarts the tenant, so the
    redirect to `/Admin/Features` can take a few seconds.
  - Wait until the page shows the text "was enabled" (`Features/Controllers/AdminController.cs:317`), then hold.
  - On screen when the narration ends: the Features page with the green message "The feature Microsoft Entra ID (Azure
    Active Directory) Authentication was enabled." The message names only the feature that was selected, not External
    Authentication: `Enable` passes that one feature (`AdminController.cs:247`) and `FeatureService.EnableOrDisableFeaturesAsync`
    hands the same one-item list to the notification (`OrchardCore.Features.Core/Services/FeatureService.cs:79-80`), while
    the dependency is enabled by `ShellDescriptorFeaturesManager.GetFeaturesToEnable`. Do not wait for, or expect, a message
    that lists External Authentication.
  - The end of this clip is the start state of 05.

## Clip `05-settings` (chapter 05, Configure the settings)

Starts on `/Admin`, opened with `scene.OpenAsync("/Admin")` before `StartCaptureAsync()`. The site is in the state clip 04
left. The `:NN` line numbers of this clip are lines of
`src/OrchardCore.Modules/OrchardCore.Microsoft.Authentication/Views/MicrosoftEntraIDSettings.Edit.cshtml`. The ids of the
inputs are expected to be `AzureADSettings_DisplayName`, `AzureADSettings_AppId`, `AzureADSettings_TenantId`,
`AzureADSettings_CallbackPath` and `AzureADSettings_SaveTokens`, which is why the selectors end with `[id$='...']`; confirm them
before recording with
`MSYS_NO_PATHCONV=1 Recorder probe /Admin/Settings/OrchardCore.Microsoft.Authentication.AzureAD "() => [...document.querySelectorAll('input')].map(i => i.id)"`.

- `settings-1`
  - Click, in the admin menu, the groups "Settings", "Security" and "Authentication" in this order (each a
    `#adminMenu button.item-label` whose text is exactly that word), then `ClickAndWaitForNavigationAsync` on the link
    `#adminMenu a.microsoft-entra-id` (text "Microsoft Entra ID"; `Microsoft.Authentication/AdminMenuAAD.cs:25-29` adds the
    class `microsoft-entra-id`). "Settings", "Security" and "Authentication" have children and no page of their own, so
    the admin theme renders each as a `button` with the class `item-label` that toggles its children, and "Microsoft Entra ID"
    as an `a` link (`Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:23-36,69`). The page is
    `/Admin/Settings/OrchardCore.Microsoft.Authentication.AzureAD`.
  - `HighlightAsync` the warning notice at the top of the form, the alert that starts with "The website might be restarted
    upon saving the settings" (`Views/MicrosoftEntraIDSettings.Edit.cshtml:7`). Do not click its "See documentation"
    link (`:8`), which opens a new tab.
  - On screen when the narration ends: the form titled "Microsoft Entra ID Authentication Settings" (`:15`), with the
    notice ringed and the four fields empty.
- `settings-2`
  - `ClearHighlightsAsync()`, then `TypeAsync` into `input[id$='DisplayName']` (label "Display Name", `:20`; the hint is "The
    display name of the provider.", `:24`) the text `Maple Works Microsoft account`, with `delay: 40`.
  - On screen when the narration ends: Display Name filled in.
- `settings-3`
  - `TypeAsync` into `input[id$='AppId']` (label "AppId", `:29`) the text `11111111-2222-3333-4444-555555555555`, with
    `delay: 25`; then `TypeAsync` into `input[id$='TenantId']` (label "TenantId", `:38`) the text
    `66666666-7777-8888-9999-000000000000`, with `delay: 25`. Display Name, AppId and TenantId carry the required marker
    (`ocat-label-required`, `:20`, `:29`, `:38`).
  - On screen when the narration ends: the three fields filled with the values of the table above.
- `settings-4`
  - `ClickAsync` on `input[id$='CallbackPath']` (label "CallbackPath", `:47`), type nothing, then `HighlightAsync` it.
    The field is empty and shows the grey placeholder `/signin-oidc` (`:49`), and the hint "The request path within the
    application's base path where the user-agent will be returned. The middleware will process this request when it
    arrives." (`:51`).
  - On screen when the narration ends: the empty CallbackPath field, ringed, with its placeholder.
- `settings-5`
  - `ClearHighlightsAsync()`, then `HighlightAsync` the row of the checkbox `input[id$='SaveTokens']` (label "Store tokens",
    `:59`; hint "Store external authentication tokens for reuse", `:60`). Do not tick it.
  - On screen when the narration ends: the unticked "Store tokens" checkbox, ringed, with its hint.
- `settings-6`
  - `ClearHighlightsAsync()`. Then `ClickAndWaitForNavigationAsync` on `button.save` (label "Save", `Settings/Views/SiteSettings.SaveButton.cshtml:2`).
    No scroll step is needed: `Scene.CenterOfAsync` calls `ScrollIntoViewIfNeededAsync` before the cursor moves.
    Wait until the page shows the text "Site settings updated successfully." (`Settings/Controllers/AdminController.cs:119`),
    then hold. Saving asks for a release of the tenant, so the answer can take a few seconds.
  - On screen when the narration ends: the same settings page, reloaded, with the green message "Site settings updated
    successfully." and the saved values in Display Name, AppId and TenantId.
  - The saved settings are the start state of 06. Do not record 06 before this step has succeeded.

## Clip `06-login` (chapter 06, The sign-in button)

This scene is signed out. Create it with `Scene.StartAsync(browser, name, BaseUrl, <a path to a file that does not
exist>, outputRoot, durations)` instead of `NewSceneAsync`, so no `state.json` is loaded (`Scene.cs:93-109` starts without a
sign-in when the file is missing). Open `/Login` with `scene.OpenAsync("/Login")` before `StartCaptureAsync()`. No password
is typed in this clip.

- `login-1`
  - `MoveToAsync` the heading "Log in" (`h1`, `Users/Views/Account/Login.cshtml:27`) on the left, then `MoveToAsync` the heading
    "Use another service to log in" (`h1`, `Login.cshtml:38`) on the right. Match the first heading by its exact text (for
    example `scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Log in", Exact = true })`), because a plain text filter
    "Log in" also matches the second heading, which contains "log in" in lower case. Then `HighlightAsync` the button
    `button[name='provider']` with the text "Maple Works Microsoft account" (`Login.cshtml:43`).
  - On screen when the narration ends: the login page with the local "Log in" form on the left, and on the right the
    heading "Use another service to log in" and one button, "Maple Works Microsoft account", ringed in green. At 1536 px wide
    the two sections sit side by side (`col-md-6`).
- `login-2`
  - Keep the ring on the button and `PauseAsync(1500)`; no other action.
  - On screen when the narration ends: the same frame as for `login-1`.
- `login-3`
  - `MoveToAsync` the button `button[name='provider']`, so the cursor rests on it. Do not click.
  - On screen when the narration ends: the button, ringed, with the cursor on it; the page has not changed, and the URL is
    still `/Login`.

## Slide steps

These steps are drawn by `slides.py`; nothing is recorded for them. Each line says what the slide shows.

- `how-1`: "Signing in with Microsoft Entra ID", four numbered cards; the first two are bright (select the button, sign in at Microsoft).
- `how-2`: the same slide with the last two cards bright (return to your site, match the person).
- `register-1`: "The app registration, in the Azure portal", four cards; "New registration" is bright.
- `register-2`: the same slide; "Copy two IDs" is bright.
- `register-3`: the same slide; "Authentication" is bright.
- `register-4`: the same slide; "Token configuration" is bright.
- `who-1`: "The tenant ID decides who can sign in", three cards; "Directory (tenant) ID" is bright.
- `who-2`: the same slide; "organizations" and "common" are bright.
- `match-1`: "When Microsoft sends the person back", four cards; "A linked user exists" is bright.
- `match-2`: the same slide; "A user has the same email" is bright.
- `match-3`: the same slide; "No user matches" is bright.
- `match-4`: the same slide; "Link later" is bright.
- `match-5`: "Options for every external provider", three cards with the labels "Use external provider for login.", "Disable local password login." and "Disable new external registration", and the settings page of each.
- `roles-1`: "Roles from Entra ID groups", three cards; the first two are bright (groups, the groups claim).
- `roles-2`: the same slide; the third card, "A User Login option", is bright.

## Checks for private data

- Every value on screen is fake: the `admin` user, `admin@example.com`, the Display Name, the two made-up identifiers, and the
  Blog recipe's sample content. No real directory, organization, person, email address, token or password is used.
- The password is never typed on camera and never written down; `state.json` stays out of git.
- Look at every contact sheet from `review.py` for `04-enable`, `05-settings` and `06-login`: no error page, no developer
  exception page, no stack trace, no log message, and no address of a real site.
- OCR the finished video at one frame per second and at every scene change (see "Check for private data" in the skill),
  and search for `@` addresses other than `admin@example.com`, for any GUID other than the two above, and for the word
  "Bearer".
- Stop the site by its process id, and delete the copied `App_Data` snapshot when the video is published.

# Recording plan: Microsoft Account sign-in

How to record `microsoft-account-login` without deciding anything. Selectors below stand for `scene.Page.Locator(...)`.
Labels and ids come from the Orchard Core source at the commit named in `catalog/SOURCE.md`; paths are relative to
the root of that checkout, and these prefixes are used:

- `MA` = `src/OrchardCore.Modules/OrchardCore.Microsoft.Authentication`
- `USERS` = `src/OrchardCore.Modules/OrchardCore.Users`
- `FEATURES` = `src/OrchardCore.Modules/OrchardCore.Features`
- `SETTINGS` = `src/OrchardCore.Modules/OrchardCore.Settings`

The element ids in this plan are derived from the source, not seen on a running site: probe each one once with
`Recorder probe` before recording (see `OPEN-QUESTIONS.md`).

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of the checkout named in `catalog/SOURCE.md`, built and run on
  `http://localhost:5010` with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Setup**: a fresh site provisioned by Auto Setup, so no setup or login form is recorded. This video is not about
  the Setup screen. Tenant `Default`, recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`,
  database SQLite, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git and passed in
  `VIDEO_PASSWORD`; never written to a file of the repository).
- **Outbound calls**: start the site with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so it never calls out.
  The plan never clicks the **Microsoft Account** button, so no request reaches Microsoft.
- **Features to enable before recording**: none. At the start of `02-enable`, **Microsoft Account Authentication** and
  **External Authentication** must be disabled (the Blog recipe does not enable them). Leave **Secrets**, **Users
  Registration** and **Email** disabled (no feature that the Blog recipe enables depends on **Secrets**,
  `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-82`; only **OpenID Connect Secrets** and **Secrets - Azure
  Key Vault** do): with **Secrets** enabled, the **Secret Key** field becomes a drop-down list
  (User-specified value, Use a secret) and the recording would not match the narration.
- **Settings to leave alone**: the external login settings stay at their defaults. In particular, **Use external
  provider for login.** must stay cleared; when it is selected and only one provider exists, the login page sends the
  visitor straight to the provider and never shows the button
  (`USERS/Services/ExternalLoginFormEvents.cs:38`).
- **External service and its stand-in**: Microsoft. There is no stand-in, because the video stops before the
  redirect. The AppId and Secret Key are sample values typed in `03-settings`; they are not registered anywhere.
- **Seed data created outside the capture**: none. The only data are the two sample values below, typed on camera.

| Field | Value typed |
|---|---|
| AppId | `11111111-2222-3333-4444-555555555555` |
| Secret Key | `not-a-real-secret` (the field shows dots) |

- **Sign-in**: run `Recorder setup` once (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`). It saves `state.json`,
  which stays out of git.

## Order, snapshots and what changes the site

Record in this order; each clip starts from the state the previous one left.

| # | Clip | Starts from | Changes the site? |
|---|---|---|---|
| 1 | `02-enable` | Snapshot **S0**: fresh site after Auto Setup, `Recorder setup` done | Yes: enables **Microsoft Account Authentication** and, with it, **External Authentication** |
| 2 | `03-settings` | Snapshot **S1**: the state `02-enable` left | Yes: saves the Microsoft Account settings and restarts the tenant |
| 3 | `04-login` | Snapshot **S2**: the state `03-settings` left | No. It clears the cookies of its own browser context only |

`01-how` and `05-return` are slides: they need no site and can be recorded in any order.

To take a snapshot, stop the site (by its process id), copy its `App_Data` folder and `state.json` aside, and start the
site again. To record a clip again, stop the site, restore the snapshot the clip starts from, start the site, and wait
until `/Admin` answers. Never commit a snapshot.

## Clip `02-enable`

**Starts on** `/Admin`, signed in (`state.json`). Before `StartCaptureAsync`: `OpenAsync("/Admin")` and wait for
`#adminMenu` to be visible. Collapse nothing and expand nothing: the menu is part of the story.

### `enable-1`

1. Click the **Tools** group of the admin menu (the `button` named `Tools` inside `#adminMenu`).
2. Click the **Features** link under it, with `ClickAndWaitForNavigationAsync`. The page is `/Admin/Features`.
3. `TypeAsync` in `#search-box` the text `Microsoft Account` (delay 60).
4. Wait until `#btn-enable-OrchardCore_Microsoft_Authentication_MicrosoftAccount` is visible.

**On screen when the narration ends**: the **Features** page filtered to one row. The category heading **Microsoft
Authentication**, the row **Microsoft Account Authentication**, its description **Authenticates users with their
Microsoft Account.**, and a primary-colored **Enable** button. The cursor rests near the row.

**Labels from**: **Tools** and **Features** `FEATURES/AdminMenu.cs:26-27`; **Search** placeholder
`FEATURES/Views/Admin/Features.cshtml:27`; category heading `FEATURES/Views/Admin/Features.cshtml:88`; name,
category and description `MA/Manifest.cs:14-16`; **Enable** `FEATURES/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.

### `enable-2`

1. `ClickAndWaitForNavigationAsync` on `#btn-enable-OrchardCore_Microsoft_Authentication_MicrosoftAccount`. The link
   posts to the server and the tenant restarts; wait up to 60 seconds for `#search-box` to be visible again. If the
   wait is long, wrap it in `OffCameraAsync` with this condition: the success message below is visible.
2. `TypeAsync` in `#search-box` the text `Microsoft Account` again (the page reloads unfiltered), and wait until
   `#btn-disable-OrchardCore_Microsoft_Authentication_MicrosoftAccount` is visible.

**On screen when the narration ends**: the green message **The feature Microsoft Account Authentication was
enabled.** at the top of the page, and under it the filtered row with a red **Disable** button in place of **Enable**.

**Labels from**: the message `FEATURES/Controllers/AdminController.cs:317` (the message names only the feature that was
clicked; **External Authentication** is enabled silently as a dependency, `MA/Manifest.cs:17-20` and
`USERS/Manifest.cs:23-26`); **Disable** `FEATURES/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53`.

## Clip `03-settings`

**Starts on** `/Admin`, signed in, from snapshot **S1**. Before `StartCaptureAsync`: `OpenAsync("/Admin")` and wait
for `#adminMenu`. Do not expand the **Settings** group beforehand.

### `settings-1`

1. Click the **Settings** group in `#adminMenu`, then **Security**, then **Authentication** (each is a group that
   opens on a click).
2. Click the **Microsoft** link, with `ClickAndWaitForNavigationAsync`. The address is
   `/Admin/Settings/OrchardCore.Microsoft.Authentication.MicrosoftAccount`.
3. Wait until the heading **Microsoft Account Authentication Settings** is visible.

**On screen when the narration ends**: the settings page with the yellow warning **The website might be restarted upon
saving the settings, potentially leading to temporary unresponsiveness during the process.**, the **See
documentation** link at its end, and the heading **Microsoft Account Authentication Settings** below it. The menu
shows **Microsoft** as the current item under **Settings**, **Security**, **Authentication**.

**Labels from**: menu path `MA/AdminMenuMicrosoftAccount.cs:25-28`; warning and link
`MA/Views/MicrosoftAccountSettings.Edit.cshtml:7-8`; heading `MA/Views/MicrosoftAccountSettings.Edit.cshtml:15`.

### `settings-2`

1. `TypeAsync` in `#ISite_MicrosoftAccountSettings_AppId` the text `11111111-2222-3333-4444-555555555555` (delay 40).

**On screen when the narration ends**: the **AppId** row, marked as required, with the sample value typed and the hint
**The Application ID found on the Microsoft developer portal parameters.** under it.

**Labels from**: `MA/Views/MicrosoftAccountSettings.Edit.cshtml:20` (label and required marker) and `:24` (hint);
the required rule `MA/ViewModels/MicrosoftAccountSettingsViewModel.cs:8`.

### `settings-3`

1. `TypeAsync` in `#ISite_MicrosoftAccountSettings_AppSecret` the text `not-a-real-secret` (delay 40). The field is a
   password input, so only dots show.

**On screen when the narration ends**: the **Secret Key** row with a field of dots (no readable text) and the hint
**The Application secret key found on the Microsoft developer portal parameters.** under it.

**Labels from**: `MA/Views/MicrosoftAccountSettings.Edit.cshtml:29` and `:32`. The field itself is drawn by
`src/OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs`: when the **Secrets** feature is
disabled it renders a plain password input (id `ISite_MicrosoftAccountSettings_AppSecret`) with the placeholder
**Enter a new value** (`:104`).

### `settings-4`

1. `MoveToAsync` the field `#ISite_MicrosoftAccountSettings_CallbackPath`, then `HighlightAsync` on it. Type nothing.

**On screen when the narration ends**: the **CallbackPath** row with an empty field that shows the placeholder
**/signin-microsoft**, ringed in green, and the hint **The request path within the application's base path where the
user-agent will be returned. The middleware will process this request when it arrives.**

**Labels from**: `MA/Views/MicrosoftAccountSettings.Edit.cshtml:37` (label), `:39` (placeholder), `:41` (hint).

### `settings-5`

1. `ClearHighlightsAsync`, then `MoveToAsync` the checkbox `#ISite_MicrosoftAccountSettings_SaveTokens` and
   `HighlightAsync` on it. Do not select it.

**On screen when the narration ends**: the **Store tokens** checkbox, cleared and ringed, with the hint **Store
external authentication tokens for reuse** next to it.

**Labels from**: `MA/Views/MicrosoftAccountSettings.Edit.cshtml:49-50`.

### `settings-6`

1. `ClearHighlightsAsync`. Scroll down if needed (`ScrollAsync`) so the **Save** button is visible.
2. `ClickAndWaitForNavigationAsync` on `.action-bar-bottom button.save` (the **Save** button). The tenant restarts
   after the save: wait up to 60 seconds for the success message below. If the wait is long, wrap it in
   `OffCameraAsync` with the message as its condition.

**On screen when the narration ends**: the green message **Site settings updated successfully.** at the top of the
page; the **AppId** field still holding the sample value; the **Secret Key** field empty with the placeholder **The
value is securely stored. Enter a new value to update it, or leave it blank to keep it.** (it shows the secret was
kept, and shows no readable secret).

**Labels from**: **Save** `SETTINGS/Views/SiteSettings.SaveButton.cshtml:2`; message
`SETTINGS/Controllers/AdminController.cs:119`; restart on save `MA/Drivers/MicrosoftAccountSettingsDisplayDriver.cs:92`;
placeholder `src/OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:103`.

## Clip `04-login`

**Starts on** `/Login`, as a visitor who is not signed in, from snapshot **S2**. Before `StartCaptureAsync`:

1. `await scene.Context.ClearCookiesAsync()` (the scene starts with the administrator's session from `state.json`).
2. `OpenAsync("/Login")`.
3. Wait for `button[name="provider"]` to be visible. The tenant restarts after the settings are saved, so if it is not
   there after a few seconds, reload the page and wait again. If it never appears, the AppId or the Secret Key was not
   saved: restore **S1** and record `03-settings` again.

### `login-1`

1. `MoveToAsync` the heading `h1` whose text is `Use another service to log in`, then `HighlightAsync` on it.

**On screen when the narration ends**: the login page with the **Log in** form (**Username or email address**,
**Password**) on the left, and on the right the heading **Use another service to log in**, ringed, with the
primary-colored **Microsoft Account** button under it. No user is signed in.

**Labels from**: **Log in** `USERS/Views/Account/Login.cshtml:27`; **Username or email address** and **Password**
`USERS/Views/LoginFormCredentials.cshtml:7,13`; **Use another service to log in** `USERS/Views/Account/Login.cshtml:38`;
the button text is the display name of the provider, **Microsoft Account**,
`MA/Configuration/MicrosoftAccountOptionsConfiguration.cs:53` and `USERS/Views/Account/Login.cshtml:43`. The button only
exists when both AppId and Secret Key are saved (`MA/Configuration/MicrosoftAccountOptionsConfiguration.cs:40-49`).

### `login-2`

1. `ClearHighlightsAsync`, then `MoveToAsync` the **Microsoft Account** button (`button[name="provider"]`) and
   `HighlightAsync` on it. **Do not click it**: a click posts to the server, which redirects the browser to Microsoft
   (`USERS/Controllers/ExternalAuthenticationsController.cs:74-83`), and the screencast would end on a page that is not
   part of this video.

**On screen when the narration ends**: the same login page, with a green ring around the **Microsoft Account** button.
The address has not changed.

**Labels from**: `USERS/Views/Account/Login.cshtml:43`.

End the clip with `ClearHighlightsAsync`.

## Slides

The slides are drawn by `slides.py`; nothing is recorded for them.

### `01-how`

- `how-1`: heading "The sign-in round trip": three cards, the visitor chooses Microsoft, Microsoft signs the visitor
  in, the visitor returns to your site.
- `how-2`: heading "What you create at Microsoft": three cards, Application ID (AppId field), Secret key (Secret Key
  field), Callback path (default /signin-microsoft).
- `how-3`: heading "Work or school accounts too?": two cards, Microsoft Account (this video) and Microsoft Entra ID
  (work or school accounts as well, with a multi-tenant app).

### `05-return`

- `return-1`: heading "When Microsoft sends the visitor back": four cards, the first (a linked local user exists, the
  visitor is signed in) bright, the others dimmed.
- `return-2`: the same four cards, the second (a local user has the same email: Link your account) bright.
- `return-3`: the same four cards, the third (Create a new account) and the fourth (external registration settings)
  bright.
- `return-4`: heading "Link a Microsoft account later": two cards, External Logins in the user menu, and Add another
  service to log in.

The labels of these slides come from `USERS/Views/ExternalAuthentications/LinkExternalLogin.cshtml:8` (**Link your
account.**), `USERS/Views/ExternalAuthentications/RegisterExternalLogin.cshtml:9-43` (**Create a new account.**,
**Username**, **Email**, **Password**, **Confirm password**), `USERS/Views/UserMenuItems-ExternalLogins.cshtml:3`
(**External Logins**) and `USERS/Views/ExternalAuthentications/ExternalLogins.cshtml` (**Add another service to log in.**).
The order of the three outcomes follows `USERS/Controllers/ExternalAuthenticationsController.cs:113-268`.

## Checks for private data

- Every value on screen is one of the sample values above, or a label. The administrator is `admin` with
  `admin@example.com`; no other person, email or domain appears. The address bar is not part of the frame.
- The **Secret Key** field must show dots in every frame: no frame may show `not-a-real-secret` in readable text
  (zoom into the contact sheet of `settings-3` and `settings-6`).
- No frame shows a page of Microsoft: `login-2` ends before any click.
- OCR the finished clips at one frame per second and at every scene change, and search for `@` (only
  `admin@example.com` is allowed), for `http`, and for the password used for `<password>` (it must never appear).
- Do not commit `state.json`, a snapshot of `App_Data`, audio, or video files.

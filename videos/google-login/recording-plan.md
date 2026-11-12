# Recording plan: google-login

How to record the three recorded clips of this video (`02-enable`, `03-settings`, `04-login`) without deciding
anything. The two slide chapters (`01-how`, `05-related`) need no recording. Paths such as
`src/OrchardCore.Modules/...` are in the Orchard Core source (`OrchardCMS/OrchardCore`, the commit the content was
written from is `92db4a83`; any later commit that still has the same labels works). A selector written here stands for
`scene.Page.Locator(...)`.

## The site it needs

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web`, run on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| App_Data | an empty folder outside both repositories (`ORCHARD_APP_DATA`) |
| Provisioning | Auto Setup, no setup form is recorded (this video is not about the Setup screen) |
| Recipe | Blog (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`) |
| Site name, time zone | `Orchard Core`, `America/Los_Angeles` |
| Tenant | Default tenant only, no prefix |
| Admin user | user name `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Database | Sqlite |

Start command (Git Bash), from the Orchard Core checkout:

```bash
export ORCHARD_APP_DATA=<empty folder outside both repositories>
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9   # the site never calls an outside service
cd src/OrchardCore.Cms.Web && dotnet run -c Release --urls http://localhost:5010
```

Preconditions to check on the fresh site before the first take:

- **Features.** Only the features of the Blog recipe are enabled. **Google Authentication is disabled** (clip
  `02-enable` enables it). **Secrets (`OrchardCore.Secrets`) must be disabled**: the Blog recipe does not enable it, and
  only OpenID depends on it. With Secrets enabled, the API secret key field shows a drop-down list in front of the
  password box, and `settings-4` and `settings-7` no longer match the screen.
- **Settings.** Everything keeps its default. In particular, under Settings, Security, User Login, the External Login
  tab has **Use external provider for login.** cleared, and **Disable local password login.** (General tab) is cleared
  (`src/OrchardCore.Modules/OrchardCore.Users/Views/LoginSettings.Edit.cshtml:73`). If the
  first is selected and Google is the only service, `/Login` redirects straight to Google
  (`src/OrchardCore.Modules/OrchardCore.Users/Services/ExternalLoginFormEvents.cs:36-77`) and clip `04-login` shows
  nothing.
- **Registration settings.** Under Settings, Security, Registration, the External Authentication tab also keeps its
  default. On this site **Disable new external registration** is selected (`OPEN-QUESTIONS.md` item 15), and the video
  never shows that tab.
- **External service.** Google is never contacted and has no local stand-in. The identifiers typed in clip
  `03-settings` are fake, and **the Google button is never clicked** (see `04-login`).
- **Fake seed data.** None is created outside the capture. The only values typed on camera are:

  | Field | Value |
  |---|---|
  | Client ID | `1234-orchard-video-demo` |
  | API secret key | `demo-secret-not-real` (a password box, so only dots are drawn) |

Sign the recorder in once, outside the capture (`Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`),
and keep `state.json` out of git. This video has no `PrepareAsync`: the clips create the state they need.

## Order of the clips, and what each one changes

Record in this order, because each clip starts from the state the previous one left:

| Order | Clip | Changes the site? | Starts from |
|---|---|---|---|
| 1 | `02-enable` | Yes: enables the feature Google Authentication (and, by dependency, External Authentication) | snapshot A |
| 2 | `03-settings` | Yes: saves the Client ID and the API secret key, and releases the tenant | snapshot B |
| 3 | `04-login` | No | snapshot C |

Snapshots (stop the site by its process ID, copy its `App_Data` folder and `state.json` aside, start it again):

- **A** is the fresh site after Auto Setup and `Recorder setup`, with Google Authentication disabled.
- **B** is taken after a good take of `02-enable`.
- **C** is taken after a good take of `03-settings`.

To record a clip again, stop the site, restore the snapshot it starts from, and start the site. Restoring `App_Data`
together with `state.json` keeps the data-protection keys, so the saved sign-in stays valid. `01-how` and `05-related`
are slides: nothing to restore.

Enabling the feature and saving the settings both release the tenant, so the next request after each of them can take
a few seconds. Wait for the concrete condition written in each step, and use `scene.OffCameraAsync(...)` for that wait
so it is cut out of the clip.

## Before every take

- Chromium, viewport, scale and locale are the recorder's defaults (see `references/recording.md`).
- The admin menu groups are collapsed when a clip starts (the menu is rendered collapsed). If a group is open, click
  its label before `StartCaptureAsync()`.
- Messages (notifications) in the admin are toasts at the top right. A success message hides itself 5 seconds after it
  appears (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:70-71` passes 5000 ms, and
  `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml` turns that into `data-bs-autohide` and `data-bs-delay`). Both
  toasts of this video are success messages. So a toast is **gone** when the narration of its step ends: it must be on
  screen while the narration says "a message confirms" (`enable-3`, `settings-7`), and the last frame of those steps
  does not show it. Look at the contact sheets of both steps, and take the clip again if the toast came after that
  sentence or was cut off.

## Clip `02-enable`

Start page: `/Admin`, opened with `scene.OpenAsync("/Admin")`, not recorded. Then `StartCaptureAsync()`.

Keep every `pauseAfter` at 300 ms or less in the first step: it has two menu clicks, a page load and typing to fit in
the narration.

- `enable-1`: click the group **Tools** in `#left-nav`, then click **Features** with
  `ClickAndWaitForNavigationAsync`. The page `/Admin/Features` loads. Wait until `#search-box` is visible. Type
  `Google` into `#search-box` with `TypeAsync` (the list filters while typing).
  On screen when the narration ends: the Features page, the search box holding `Google`, and the list reduced to one
  category headed **Google** with three rows, **Google Analytics**, **Google Authentication** and **Google Tag
  Manager**, each with the status **Disabled**.
  Labels: `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features);
  `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27` (search box);
  `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:14-15` (Disabled).
- `enable-2`: at the start of the step, `HighlightAsync` the category heading
  `.feature-group:has(h3:text-is("Google")) h3`. When the narration reaches its second sentence (about 4 seconds
  later), `ClearHighlightsAsync()`, then `HighlightAsync` the row of Google Authentication,
  `[data-filter-value*="OrchardCore.Google.GoogleAuthentication"]`. Do not click anything.
  On screen when the narration ends: a ring around the Google Authentication row, with its description
  **Authenticates users with their Google Account.**, the dependency badge **External Authentication**, the status
  **Disabled** and the **Enable** button visible.
  Labels: `src/OrchardCore.Modules/OrchardCore.Google/Manifest.cs:12-21` (name, category, description, dependency);
  `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:10-18`
  (dependency badge). The row of External Authentication is not listed: the filter **Hide on demand** is selected by
  default (`Features.cshtml:65`), and that feature is enabled by dependency only. Do not change the filter.
- `enable-3`: `ClearHighlightsAsync()`, then `PauseAsync(1000)` so that the click comes after the first sentence has
  started, then click **Enable** on the Google Authentication row:
  `a[href*="/Features/OrchardCore.Google.GoogleAuthentication/Enable"]`, with `ClickAndWaitForNavigationAsync`. The
  link posts the request (`data-url-af="UnsafeUrl"`) and the page reloads on `/Admin/Features`. Enabling the feature
  releases the tenant, so the reload can take a few seconds, and that part stays on camera. Then wait, off camera,
  until a toast containing `The feature Google Authentication was enabled.` is visible:
  `scene.OffCameraAsync(() => scene.Page.Locator(".toast", new() { HasText = "Google Authentication was enabled"
  }).WaitForAsync())`. Do not add any other wait after it: the toast hides 5 seconds after it appears.
  On screen while the narration says "a message confirms" (about 5 to 9 seconds into the step): the Features page with
  the toast **The feature Google Authentication was enabled.** at the top right. When the narration ends the toast is
  gone, and the search box is empty again with every category listed; that is expected.
  Labels: `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable);
  `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317` (the message).

After this clip, take snapshot B.

## Clip `03-settings`

Start page: `/Admin`, opened with `scene.OpenAsync("/Admin")`, not recorded, menu groups collapsed. Then
`StartCaptureAsync()`.

- `settings-1`: click the group **Settings** in `#left-nav`, then **Security**, then **Authentication**, then
  **Google** (the last click with `ClickAndWaitForNavigationAsync`). The page
  `/Admin/Settings/OrchardCore.Google.GoogleAuthentication` loads. Wait until the heading **Google Authentication
  Settings** is visible. Use `pauseAfter: 200` on the first three clicks; the narration is short.
  If, after the **Authentication** click, the item **Google** is not visible within 3 seconds, call
  `scene.NavigateAsync("/Admin/Settings/OrchardCore.Google.GoogleAuthentication")` instead and say so in the take notes.
  On screen when the narration ends: the settings page, the heading **Google Authentication Settings**, and the
  menu showing the path Settings, Security, Authentication, Google with **Google** marked as the current page.
  Labels: `src/OrchardCore.Modules/OrchardCore.Google/GoogleAuthenticationAdminMenu.cs:25-28` (Settings, Security,
  Authentication, Google); `src/OrchardCore.Modules/OrchardCore.Google/Views/GoogleAuthenticationSettings.Edit.cshtml:15`
  (the heading).
- `settings-2`: `HighlightAsync` the warning `.alert.alert-warning`, then `MoveToAsync` the link **See
  documentation** inside it. Do not click it (it opens a new tab).
  On screen when the narration ends: a ring around the warning **The website might be restarted upon saving the
  settings, potentially leading to temporary unresponsiveness during the process.**, with the cursor on **See
  documentation**.
  Labels: `GoogleAuthenticationSettings.Edit.cshtml:7-8`.
- `settings-3`: `ClearHighlightsAsync()`. `HighlightAsync` the label **Client ID** (it carries a red asterisk), wait
  about 1.5 seconds, `ClearHighlightsAsync()`, then `TypeAsync(scene.Page.GetByLabel("Client ID", new() { Exact =
  true }), "1234-orchard-video-demo", 60)`.
  On screen when the narration ends: **Client ID** holds `1234-orchard-video-demo`, and its hint **The client id
  defined in Google API console for the application.** is visible.
  Labels: `GoogleAuthenticationSettings.Edit.cshtml:20,24`; the asterisk comes from
  `src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_ocat.scss:27-34`.
- `settings-4`: `TypeAsync(scene.Page.Locator("input[type='password']"), "demo-secret-not-real", 60)`. The page has
  exactly one password box, the one of **API secret key**. Press Tab afterwards.
  On screen when the narration ends: the field **API secret key** filled with dots, its hint **The client secret
  defined in Google API console for the application.** visible. No clear text of the secret is on screen.
  Labels: `GoogleAuthenticationSettings.Edit.cshtml:29,32`;
  `src/OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:96` (password box).
- `settings-5`: `HighlightAsync` the **CallbackPath** input, `scene.Page.GetByLabel("CallbackPath", new() { Exact =
  true })`. Do not type anything.
  On screen when the narration ends: a ring around the empty **CallbackPath** field, whose placeholder reads
  `/signin-google`, with its hint **The request path within the application's base path where the user-agent will be
  returned. The middleware will process this request when it arrives.** visible.
  Labels: `GoogleAuthenticationSettings.Edit.cshtml:37,39,41`.
- `settings-6`: `ClearHighlightsAsync()`, then `HighlightAsync` the check box **Store tokens**,
  `scene.Page.GetByLabel("Store tokens")`. Do not click it.
  On screen when the narration ends: a ring around the cleared check box **Store tokens**, with its hint **Store
  external authentication tokens for reuse** visible. If the check box is below the visible area, `ScrollAsync`
  after `ClearHighlightsAsync()` and before the highlight.
  Labels: `GoogleAuthenticationSettings.Edit.cshtml:49-50`.
- `settings-7`: `ClearHighlightsAsync()`, then click **Save** (`button.save`) with `ClickAndWaitForNavigationAsync`.
  The page reloads on the same address (the save redirects to it). Saving releases the tenant, so the reload can take a
  few seconds, and that part stays on camera. Then wait, off camera, until the toast `Site settings updated
  successfully.` is visible: `scene.OffCameraAsync(() => scene.Page.Locator(".toast", new() { HasText = "Site
  settings updated successfully." }).WaitForAsync())`. Then `HighlightAsync` the password box
  (`input[type='password']`). The toast hides 5 seconds after it appears (see "Before every take").
  On screen while the narration says "A message confirms" (the second sentence): the toast **Site settings updated
  successfully.** at the top right. On screen when the narration ends: **Client ID** still holding
  `1234-orchard-video-demo`, and the **API secret key** box empty with a ring and the placeholder **The value is
  securely stored. Enter a new value to update it, or leave it blank to keep it.** The toast is gone by then.
  Labels: `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (Save);
  `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119` (the message);
  `SecretInputTagHelper.cs:103` (the placeholder). The tenant is released at the end of this request
  (`src/OrchardCore.Modules/OrchardCore.Google/Authentication/Drivers/GoogleAuthenticationSettingsDisplayDriver.cs:92`).

After this clip, take snapshot C.

## Clip `04-login`

Start page: `/Admin`, opened with `scene.OpenAsync("/Admin")`, not recorded. Before `StartCaptureAsync()`, add a safety
net so that nothing can leave the site, even if a click goes wrong: `await scene.Page.Context.RouteAsync(new
Regex("^(?!" + Regex.Escape(baseUrl) + ")"), route => route.AbortAsync())`, where `baseUrl` is `VIDEO_BASE_URL`. The
proxy variables only protect the site's own calls: the recording browser is not behind them.

**The Google button is never clicked in this clip.** A click would send the recording browser to Google's own site,
and the frames would show it.

- `login-1`: `scene.NavigateAsync("/Login")`. The administrator is signed in, and the login page still shows (the action
  only clears the return address, `src/OrchardCore.Modules/OrchardCore.Users/Controllers/AccountController.cs:75-79`).
  Wait until the heading **Use another service to log in** is visible. `HighlightAsync` the heading **Log in**
  (`h1:text-is("Log in")`), and after about 2 seconds `ClearHighlightsAsync()` and `HighlightAsync` the heading
  `h1:text-is("Use another service to log in")`.
  On screen when the narration ends: the login page in two columns, **Log in** with the login form on the left,
  **Use another service to log in** with a ring on the right.
  Labels: `src/OrchardCore.Modules/OrchardCore.Users/Views/Account/Login.cshtml:27,38`.
- `login-2`: `ClearHighlightsAsync()`, then `HighlightAsync` the button
  `button[name="provider"][value="Google"]`.
  On screen when the narration ends: a ring around one button labeled **Google**, the only button of the section.
  Labels: `Login.cshtml:43`; the label comes from the scheme display name,
  `src/OrchardCore.Modules/OrchardCore.Google/Authentication/Configuration/GoogleOptionsConfiguration.cs:52`. The button
  is listed only when a Client ID and a secret are saved (`GoogleOptionsConfiguration.cs:40-47`).
- `login-3`: `MoveToAsync` the same button and leave the cursor on it, with the ring still drawn. Do not click.
  On screen when the narration ends: the cursor over the **Google** button with its ring, still on `/Login`.

## Slides

One line each: what the slide shows (drawn by `slides.py`, no recording).

- `how-1`: the heading **Sign in with a Google Account** and three cards: **A Google button**, **Google checks the
  person**, **Back to your site**.
- `how-2`: the heading **What Google provides** and three cards: **Client ID**, **Client secret**, **Authorized
  redirect URI**.
- `how-3`: the heading **When Google sends the person back** and three cards: **A linked Google Account**, **The same
  email address**, **No matching user**.
- `related-1`: the heading **Permission and secrets**; the cards **Permission** and **Secrets** are bright, the other
  two are dimmed.
- `related-2`: the heading **Other sign-in settings**; the cards **User Login: External Login** and
  **Registration: External Authentication** are bright, the other two are dimmed.

## Checks for private data

Before the clips go to assembly:

- Look at every contact sheet (`review.py`): the Features page shows no other tenant; no toast shows an error; no frame
  shows a page that is not the site's (a click on the Google button would show one: discard that take).
- The Client ID and the API secret key are the fake values above. The secret is drawn as dots in every frame.
- The user name `admin` and the address `admin@example.com` are the only account details of the site. `04-login`
  does not open the user menu, so no address is drawn; if one is, it must be `admin@example.com`.
- Run the OCR scan of the skill (one frame per second, every scene change, then five frames per second) and search for
  real names, email addresses, phone numbers and tokens.
- `state.json`, `App_Data` copies, the password and any mask script stay out of the commit.

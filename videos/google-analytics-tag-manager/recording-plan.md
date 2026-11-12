# Recording plan: Google Analytics and Tag Manager

This plan is for the person who records the video. Read only this file and follow it. Selectors written like
`#search-box` stand for `scene.Page.Locator("#search-box")`. Every `file:line` is in `src/` of OrchardCore at commit
`92db4a837d` (see `catalog/SOURCE.md`), and says where a label on screen comes from. The selectors were read from the
views and the scripts and have not been run against a site: if one does not match, check it with `Recorder probe`
before changing the step.

The video is an **admin** video: it shows the admin and the front end of the site, never the page source or any code.

## 1. The site it needs

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of a checkout at commit `92db4a837d` or a later `main`, run on `http://localhost:5010` |
| App data | `ORCHARD_APP_DATA` = an empty folder outside both repositories |
| Setup | Auto Setup (no setup screen, no sign-in screen recorded): recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, database provider `Sqlite`, user `admin`, email `admin@example.com`, password `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git) |
| Auto Setup variables | `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls another service |
| Theme | The Blog recipe sets **TheBlogTheme** for the site and **TheAdmin** for the admin: keep them |
| Admin lists | Leave the admin settings as the recipe leaves them: the Features page is drawn in the **List** layout with **Buttons** for the actions of a row (the defaults) |
| Features enabled before recording | None to add. The Blog recipe already enables Features, Settings, Navigation, Deployment and the others the video passes through. **OrchardCore.Google.Analytics** and **OrchardCore.Google.TagManager** must be **disabled**: clip `02-enable` enables them on camera |
| Settings before recording | None. The Tracking ID and the Container ID are empty |
| External services | None. The Google features only add scripts that the *browser* loads from `www.googletagmanager.com` (see the question in `OPEN-QUESTIONS.md` about blocking those requests on the recording machine) |
| Sign-in | `Recorder setup` signs in once as `admin` and saves `state.json` (git-ignored) **before** any clip is recorded, so the admin menu starts with every group collapsed |

### The fake data

There is nothing to create outside the capture: the Blog recipe provides the front end (a **Home** and an **About** link
in the main menu, and one blog post). The only values typed on camera are two obviously fake IDs, the placeholders of
the documentation:

| Field | Value typed |
|---|---|
| Tracking ID | `G-XXXXXXXXXX` |
| Container ID | `GTM-XXXXXXX` |

No other value is typed. The admin user's email `admin@example.com` is never on screen in this video.

## 2. Order of the clips, and what changes the site

Record in the order the clips play. Each one starts from the state the previous one leaves.

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-enable` | Snapshot **A**: fresh site, both Google features disabled | Yes: enables Google Analytics and Google Tag Manager |
| 2 | `03-analytics` | Snapshot **B**: taken after `02-enable` | Yes: saves the Tracking ID |
| 3 | `04-tagmanager` | Snapshot **C**: taken after `03-analytics` | Yes: saves the Container ID |
| 4 | `05-site` | Snapshot **D**: taken after `04-tagmanager` | No |

Slides (`01-why`, `06-limits`) are drawn from `slides.py`; there is nothing to record for them.

Snapshots: stop the site and copy its `App_Data` folder (and `state.json`) aside after each of A, B, C and D. To record a
clip again, stop the site, restore the snapshot it starts from, start the site, and record only that clip. `05-site`
changes nothing, so it can be recorded again without restoring.

## 3. Clips

### `02-enable` (chapter 02, Enable the features)

**Start page:** `/Admin`, opened with `scene.OpenAsync("/Admin")` before `StartCaptureAsync`. The menu groups are collapsed.
Nothing is typed or selected.

- `enable-1`
  - Click **Tools** in the admin menu: `#adminMenu` text `Tools` (exact match). The group opens.
  - Click **Features** in the opened group with `ClickAndWaitForNavigationAsync` (the page is `/Admin/Features`).
  - Type `Google` into the search box with `TypeAsync(#search-box, "Google")`. The list filters while typing; no button is pressed.
  - Visible when the narration ends: the **Features** page with `Google` in the search box and one category, **Google**, with three rows (Google Analytics, Google Authentication, Google Tag Manager), each with a **Disabled** badge and an **Enable** button.
  - Labels: Tools `OrchardCore.Settings/AdminMenu.cs:32`; Features `OrchardCore.Features/AdminMenu.cs:27`; search box `Views/Admin/Features.cshtml:27` (placeholder **Search**); category title `Features.cshtml:88`, from `Category = "Google"` in `OrchardCore.Google/Manifest.cs:15,26,33`; badge `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`.
- `enable-2`
  - `HighlightAsync` on the **Google** category title (`h3` with the text `Google`), then in turn on the names of the rows **Google Analytics**, **Google Authentication** and **Google Tag Manager**, as the narration lists them (use the row's `span.fw-semibold`). When the narration reaches "Google Authentication lets users sign in", the highlight is on the **Google Authentication** row. No clicks.
  - Visible when the narration ends: the three rows, with the ring on **Google Authentication**.
  - Labels: row names `OrchardCore.Google/Manifest.cs:14,25,32`; the description "Authenticates users with their Google Account." is on screen under the name (`Manifest.cs:16`).
- `enable-3`
  - `ClearHighlightsAsync()`.
  - Click the checkbox of the **Google Analytics** row: `input[name='featureIds'][value='OrchardCore.Google.Analytics']`. Click the checkbox of the **Google Tag Manager** row: `input[name='featureIds'][value='OrchardCore.Google.TagManager']`. The checkboxes have no visible label.
  - Then `MoveToAsync` the **Enable** button of the **Google Analytics** row, `#btn-enable-OrchardCore_Google_Analytics`, without clicking it, while the narration says each row has its own **Enable** button.
  - Visible when the narration ends: the two checkboxes ticked (Google Authentication not ticked), the cursor on the **Enable** button of the first row.
  - Labels: `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; checkbox `Views/FeatureEntry.Checkbox.SummaryAdmin.cshtml`.
- `enable-4`
  - Click **Bulk Actions** (`#bulk-action-menu-button`). The menu opens with **Disable** and **Enable**.
  - Click **Enable** in that menu (`a[data-action='Enable']`) with `ClickAndWaitForNavigationAsync`. Enabling the features restarts the tenant, so the page takes a moment to come back.
  - **Timing:** this click is the last action of the step and it lands in the last second of the narration (add a `PauseAsync` before it), so the success message (below) does not appear before `enable-5` starts. The message is a toast that hides itself after 5 seconds (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`).
  - Visible when the narration ends: the reloaded **Features** page (the search box is empty again, the whole list is shown).
  - Labels: **Bulk Actions** `Features.cshtml:34`; **Enable** `Features.cshtml:38`.
- `enable-5`
  - No click. Type `Google` into the search box again (`TypeAsync(#search-box, "Google")`) while the message is shown.
  - Visible when the narration ends: a green toast at the top right that names Google Analytics and Google Tag Manager as enabled (the text is "The following features were enabled: ..." followed by the two names, in the order the service lists them; the order does not matter), and the **Google** category with **Enabled** badges on Google Analytics and Google Tag Manager and **Disabled** on Google Authentication. If the toast is gone when the narration ends, record the clip again with the click of `enable-4` later in its step.
  - Labels: message `OrchardCore.Features/Controllers/AdminController.cs:317`; badge `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`.

**After the clip:** take snapshot **B**.

### `03-analytics` (chapter 03, Google Analytics)

**Start page:** `/Admin`, opened before `StartCaptureAsync`. All menu groups are collapsed.

- `analytics-1`
  - Click **Settings** in the admin menu (`#adminMenu` text `Settings`, exact match), then **Integrations** (exact match), then **Google Analytics** with `ClickAndWaitForNavigationAsync` (the page is `/Admin/Settings/OrchardCore.Google.Analytics`).
  - `HighlightAsync` on the **See documentation** link (`a.seedoc`), then on the title **Google Analytics Settings** (`h3`), as the narration reaches them.
  - Visible when the narration ends: the page with the **See documentation** link above the title **Google Analytics Settings**, and the empty **Tracking ID** field below.
  - Labels: Settings `OrchardCore.Google/GoogleAnalyticsAdminMenu.cs:25`; Integrations `:26`; Google Analytics `:27`; title `Views/GoogleAnalyticsSettings.Edit.cshtml:14`; link `:7`.
- `analytics-2`
  - `ClearHighlightsAsync()`, then `HighlightAsync` on the label **Tracking ID**, then on the hint under the field.
  - Visible when the narration ends: the label **Tracking ID** (with the required marker) and the hint **The tracking ID defined in your Google Analytics account.**, with a ring on the hint.
  - Labels: `Views/GoogleAnalyticsSettings.Edit.cshtml:19` (label, `ocat-label-required`) and `:23` (hint). The field is required: `Analytics/ViewModels/GoogleAnalyticsSettingsViewModel.cs:7`.
- `analytics-3`
  - `ClearHighlightsAsync()`, then `TypeAsync(input[id$='TrackingID'], "G-XXXXXXXXXX")`.
  - Visible when the narration ends: `G-XXXXXXXXXX` in the **Tracking ID** field, not saved yet.
- `analytics-4`
  - Click **Save** (`button.save`, label `Save`) with `ClickAndWaitForNavigationAsync`. This is the first action of the step. The save is a form post that redirects to the same settings page, so the page reloads at once and shows the toast. Saving these settings does not restart the tenant: only the drivers that call `RequestRelease` do, and the Google Analytics and Google Tag Manager drivers do not (`Analytics/Drivers/GoogleAnalyticsSettingsDisplayDriver.cs:43-60`, `TagManager/Drivers/GoogleTagManagerSettingsDisplayDriver.cs:43-60`; the controller is `OrchardCore.Settings/Controllers/AdminController.cs:83-123`).
  - Visible when the narration ends: the green toast **Site settings updated successfully.** and `G-XXXXXXXXXX` in the **Tracking ID** field. If the toast has not appeared or is already gone, record the clip again.
  - Labels: **Save** `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; message `OrchardCore.Settings/Controllers/AdminController.cs:119`.
- `analytics-5`
  - No actions. Let the page rest.
  - Visible when the narration ends: the **Google Analytics Settings** page with `G-XXXXXXXXXX` saved in the field.

**After the clip:** take snapshot **C**.

### `04-tagmanager` (chapter 04, Google Tag Manager)

**Start page:** `/Admin`, opened before `StartCaptureAsync`. All menu groups are collapsed (the clip opens a new browser context from `state.json`, which remembers no open group).

- `tagmanager-1`
  - Click **Settings**, then **Integrations**, then **Google Tag Manager** with `ClickAndWaitForNavigationAsync` (the page is `/Admin/Settings/OrchardCore.Google.TagManager`).
  - Visible when the narration ends: the page titled **Google Tag Manager Settings**, with the empty **Container ID** field.
  - Labels: `OrchardCore.Google/GoogleTagManagerAdminMenu.cs:25-27`; title `Views/GoogleTagManagerSettings.Edit.cshtml:14`.
- `tagmanager-2`
  - `HighlightAsync` on the label **Container ID**, then on the hint under the field; `ClearHighlightsAsync()`; then `TypeAsync(input[id$='ContainerID'], "GTM-XXXXXXX")` during the last sentence.
  - Visible when the narration ends: `GTM-XXXXXXX` in the **Container ID** field, not saved yet, and the hint **The container ID defined in your Google Tag Manager account.**
  - Labels: `Views/GoogleTagManagerSettings.Edit.cshtml:19` (label) and `:23` (hint); required: `TagManager/ViewModels/GoogleTagManagerSettingsViewModel.cs:7`.
- `tagmanager-3`
  - Click **Save** (`button.save`) with `ClickAndWaitForNavigationAsync`, as the first action of the step.
  - Visible when the narration ends: the green toast **Site settings updated successfully.** and `GTM-XXXXXXX` in the **Container ID** field. If the toast is not there, record the clip again.
  - Labels: as `analytics-4`.
- `tagmanager-4`
  - No actions. Let the page rest.
  - Visible when the narration ends: the **Google Tag Manager Settings** page with `GTM-XXXXXXX` saved.

**After the clip:** take snapshot **D**, then run the checks of section 5.

### `05-site` (chapter 05, On the public site)

**Start page:** `/Admin/Settings/OrchardCore.Google.Analytics`, opened before `StartCaptureAsync` (the page of `03-analytics`, with `G-XXXXXXXXXX` saved). The capture starts on it.

- `site-1`
  - `NavigateAsync("/")`: the home page of the site (TheBlogTheme, the blog with its post). Then `ScrollAsync(0, 400)` slowly and back up, to show that the page is a normal page.
  - Visible when the narration ends: the front end home page, no admin menu, no error.
  - The recorder is signed in as `admin`, yet the page is what a visitor sees: the layout of TheBlogTheme has no link or button for a signed-in user (`Views/Layout.liquid`), and the two filters do not look at the user (`GoogleAnalyticsFilter.cs:38-48`).
  - Source: the layout renders the head scripts at `OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:19`; the filters are `Analytics/GoogleAnalyticsFilter.cs:38-48` and `TagManager/GoogleTagManagerFilter.cs:34-44`.
- `site-2`
  - Click **About** in the main menu with `ClickAndWaitForNavigationAsync(#mainNav, text "About", exact)`.
  - Visible when the narration ends: the **About** page of the site.
  - Source: the main menu is created by the Blog recipe (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:843-849`, the link goes to `~/about`).
- `site-3`
  - `NavigateAsync("/Admin/Settings/OrchardCore.Google.Analytics")`: the admin settings page of `03-analytics`. `HighlightAsync` on the title **Google Analytics Settings** at the end of the step.
  - Visible when the narration ends: the admin settings page, with the ring on its title.
  - Source: admin pages are skipped by `!AdminAttribute.IsApplied(...)`, `GoogleAnalyticsFilter.cs:38` and `GoogleTagManagerFilter.cs:34`.

### Slides (nothing to record)

| Step | What the slide shows |
|---|---|
| `why-1` | Heading "Two Google features"; four cards, the first two bright: **Google Analytics** and **Google Tag Manager** with the ID each one takes |
| `why-2` | Same cards; the second and third are bright: **Google Tag Manager** ("Takes the container ID. You choose the tags in your Google account."), and **A script on each public page** |
| `why-3` | Same cards; the fourth is bright: **Either or both**, "Each has its own settings page. No saved ID, no script." |
| `limits-1` | Heading "Four cases leave a script out"; the first two cards bright: **Pages of the admin**, **Responses that are not full pages** |
| `limits-2` | Same cards; the last two bright: **No ID saved**, **No consent to track** |
| `limits-3` | Heading "Stopping a script"; two cards: **Disable the feature** (under Tools, Features) and **The ID is required** |
| `limits-4` | Heading "Permissions"; three cards: **Manage Google Analytics settings**, **Manage Google Tag Manager settings**, **Administrator role** |

The cards and cards-to-narration match are in `slides.py`. The opening card (`intro`) and the closing card (`outro`)
need no recording either.

## 4. Where each statement of the narration comes from

| Statement | Source |
|---|---|
| Each feature adds its Google script to the head of public pages | `Analytics/GoogleAnalyticsFilter.cs:48`, `TagManager/GoogleTagManagerFilter.cs:44` (`RegisterHeadScript`; the Google Analytics block holds two script elements, the loader and the configuration, which is why the narration does not say "one script") |
| Only full pages, never the admin | `GoogleAnalyticsFilter.cs:38` (`IsViewOrPageResult`, `AdminAttribute.IsApplied`) |
| Nothing until an ID is saved | `GoogleAnalyticsFilter.cs:46`, `GoogleTagManagerFilter.cs:42` |
| Nothing when the visitor's consent does not allow tracking | `GoogleAnalyticsFilter.cs:40`, `GoogleTagManagerFilter.cs:36` (`CanTrack`, `true` when no consent feature is present) |
| The ID cannot be saved empty | `GoogleAnalyticsSettingsViewModel.cs:7`, `GoogleTagManagerSettingsViewModel.cs:7`; `Drivers/*SettingsDisplayDriver.cs` write the value only when the model is valid |
| Disabling the feature stops the script | the filter is registered by the feature's own startup, `OrchardCore.Google/GoogleAuthenticationStartup.cs:72,89` |
| The two features are independent | separate settings classes, drivers, menu entries and filters |
| The scripts load from Google | `www.googletagmanager.com` in both filters |
| Permissions and the Administrator role | `OrchardCore.Google/Permissions.cs:11-15`, `GoogleAnalyticsPermissionsProvider.cs:19`, `GoogleTagManagerPermissionProvider.cs:21`; the menu entries are shown only to users with the permission, `GoogleAnalyticsAdminMenu.cs:30`, `GoogleTagManagerAdminMenu.cs:31` and `OrchardCore.Navigation.Core/NavigationManager.cs:208-253`; the editor is built only for a user with the permission, `Analytics/Drivers/GoogleAnalyticsSettingsDisplayDriver.cs:31-34` and `TagManager/Drivers/GoogleTagManagerSettingsDisplayDriver.cs:31-34`. The slide says "needed", not "enough": the page itself also asks for the Manage settings permission (see question 1 of `OPEN-QUESTIONS.md`) |

## 5. Checks for private data and for the result

Off camera, once the snapshot **D** is running:

1. The scripts are on a public page, and only there. With the recorder (`MSYS_NO_PATHCONV=1` in Git Bash):
   `Recorder probe / "() => document.head.innerHTML.includes('gtag/js?id=G-XXXXXXXXXX') && document.head.innerHTML.includes('GTM-XXXXXXX')"`
   must print `true`, and
   `Recorder probe /Admin/Settings/OrchardCore.Google.Analytics "() => document.head.innerHTML.includes('googletagmanager.com')"`
   must print `false`. Nothing of this is shown on camera.
2. Look at the contact sheet of every clip (`review.py`): the frame at the end of each step shows what its entry above says, with no error message and no empty list.
3. After assembling, OCR the video at one frame per second and at every scene change, and search for real names, email addresses, phone numbers, tokens and IDs. The only IDs on screen are `G-XXXXXXXXXX` and `GTM-XXXXXXX`; the description of the Google Analytics row of the Features page shows the text of the feature (it ends with `gtag.js` in parentheses), which is a label of the product and not read aloud.
4. No real Google account, tracking ID or container ID is typed, imported or shown.

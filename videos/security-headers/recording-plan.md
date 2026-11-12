# Recording plan: security-headers

How to record the browser clips of this video without deciding anything. A selector written below stands for
`scene.Page.Locator(...)`. Every label comes from the Orchard Core source at the commit the catalog was read from;
paths are relative to the root of the `OrchardCMS/OrchardCore` checkout, and `M` stands for `src/OrchardCore.Modules`.

The video has two kinds of parts:

- Cards (`00-intro`, `99-outro`) and slides (`01-headers`, `06-know`): nothing to record. Slides are drawn by `slides.py`.
- Four recordings: `02-open`, `03-csp`, `04-permissions`, `05-referrer`.

## The site it needs

- A checkout of `OrchardCMS/OrchardCore` (the commit the catalog was read from, or a later `main`). Run
  `src/OrchardCore.Cms.Web` on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both
  repositories (`ORCHARD_APP_DATA`). The host does not call the method that makes the headers come from configuration
  (`src/OrchardCore.Cms.Web/Program.cs`), so the settings page shows no "coming from configuration sources" message.
  Do not add one: `know-2` is a slide and nothing on screen depends on it.
- Provision it with Auto Setup (see the `orchardcore-tester` skill of that repository), so no setup or sign-in form is
  recorded:

  | Setting | Value |
  |---|---|
  | Tenant (`ShellName`) | `Default` |
  | `SiteName` | `Orchard Core` |
  | `SiteTimeZone` | `America/Los_Angeles` |
  | `RecipeName` | `Blog` |
  | `AdminUsername` | `admin` |
  | `AdminEmail` | `admin@example.com` |
  | `AdminPassword` | `<password>`, chosen outside git and passed in `VIDEO_PASSWORD` |
  | `DatabaseProvider` | `Sqlite` |

- Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The video uses no external service,
  so there is no local stand-in to start.
- Features to enable before recording: none. The Blog recipe enables `OrchardCore.Security`
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:43`), which is what `open-2` shows. Do not disable it.
- Settings to save before recording: none. The video starts from the default Security settings: the Content Security
  Policy table is empty, every feature of the Permissions Policy is on None, and the Referrer Policy is No referrer.
- Seed data: none. No content item, user or role is needed; the clips show only admin screens.
- Sign in once outside the capture (`Recorder setup`, user `admin`, password from `VIDEO_PASSWORD`) and keep
  `state.json` out of git.

### Why only these values

The headers are sent with every response of the site, the admin included, so a value saved in one clip also applies to
the pages the next clip records. Type only the values below. Leave Default Source, Script Source and Style Source
empty, leave both checkboxes unchecked, and never select Enable Sandbox or Upgrade Insecure Requests on camera: the
recorder draws its cursor and highlights with inline script and style, and a sandbox or a narrow source list can stop
them, or stop the admin forms from working. The values typed in this video leave all of that alone:

| Clip | Where | Value |
|---|---|---|
| `03-csp` | Frame Ancestors | `'self'` (with the single quotes) |
| `03-csp` | Object Source | `'none'` (with the single quotes) |
| `04-permissions` | Geolocation | Self, with the origin `https://maps.example.com` |
| `05-referrer` | Referrer Policy | Same origin |

## Snapshot and order

1. Provision the site, sign in, then stop the site and copy its `App_Data` folder and `state.json` aside. This is the
   **defaults snapshot**.
2. Record in this order: `02-open`, `03-csp`, `04-permissions`, `05-referrer`. `02-open` changes nothing.
   `03-csp`, `04-permissions` and `05-referrer` each save settings, so each one **changes the site**.
3. **Restore the defaults snapshot before every saving clip, the first take included.** Stop the site, restore the
   copy of `App_Data` and `state.json`, start the site, and open `/Admin` once outside the capture so the site has
   started. Each saving clip needs its own tab at the defaults: `03-csp` an empty table, `04-permissions` every feature on
   None, `05-referrer` No referrer. The three clips do not depend on each other, and the expected headers in the table
   of the off-camera checks assume this: each clip is checked on a site that holds only its own saved settings.
4. `02-open` can be recorded on the snapshot or on the site after any other clip: it saves nothing.

## Clip `02-open`

Starts on `/Admin` (the "Welcome to Orchard Core" page, `M/OrchardCore.Admin/Views/AdminDashboard.cshtml:2`), opened
with `OpenAsync` before `StartCaptureAsync`. Before the capture, make sure the Tools and Settings groups of the left
menu are collapsed: the menu restores what was last left open, and a click on an open group closes it.

Menu selectors (the menu items get a class from their labels, `src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:288-296`):

| Item | Selector |
|---|---|
| Tools group | `li.oc-navigation-admin-tools > figure > figcaption button` |
| Features link | `li.oc-navigation-admin-tools-features a` |
| Settings group | `li.oc-navigation-admin-settings > figure > figcaption button` |
| Security group | `li.oc-navigation-admin-settings-security > figure > figcaption button` |
| Security Headers link | `li.oc-navigation-admin-settings-security-security_headers a` |

- `open-1`: click the Tools group, then click the Features link with `ClickAndWaitForNavigationAsync`, then
  `TypeAsync("#search-box", "Security")`. On screen when the narration ends: the Features page (heading "Features")
  with "Security" in the search box and the list filtered to a few features, one of them named Security. Labels:
  `M/OrchardCore.Features/AdminMenu.cs:26-27`, `M/OrchardCore.Features/Views/Admin/Features.cshtml:15,27`. The search
  matches the name, id and description of each feature
  (`M/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`) and filters while typing
  (`M/OrchardCore.Features/Assets/ts/features-index.ts:149`).
- `open-2`: `HighlightAsync` the row of the Security feature, `[data-filter-value^="Security OrchardCore.Security"]`.
  Do not click anything in the row. On screen when the narration ends: a green ring around the Security row, which shows
  the "Enabled" badge and a Disable button, and no Enable button. Labels:
  the badge is `M/OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` ("Enabled"); the buttons are in
  `M/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40,53` ("Enable", "Disable"). The recipe that
  enabled it: `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:43`. The row also shows a description that
  the narration does not read.
- `open-3`: `ClearHighlightsAsync`, click the Settings group, the Security group, and the Security Headers link with
  `ClickAndWaitForNavigationAsync`. Then `HighlightAsync("p.alert.alert-warning")`, a pause of about 1.5 s, clear it, and
  `HighlightAsync("ul[role=tablist]")`. On screen when the narration ends: the page at
  `/Admin/Settings/SecurityHeaders` with the heading "Settings", the yellow warning "The website might be restarted upon
  saving the settings, potentially leading to temporary unresponsiveness during the process.", the three tabs in this
  order, Content Security Policy (selected), Permissions Policy, Referrer Policy, ringed, and a Save button below the
  form. Labels: `M/OrchardCore.Security/AdminMenu.cs:26-28`, tabs `M/OrchardCore.Security/Drivers/SecuritySettingsDisplayDriver.cs:79,84,89`,
  warning `M/OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`, heading
  `M/OrchardCore.Settings/Views/Admin/Index.cshtml:4`. Where the warning sits (above or inside the tabs) is listed in
  `OPEN-QUESTIONS.md`: highlight it wherever it is.

## Clip `03-csp`

Starts on `/Admin/Settings/SecurityHeaders`, opened with `OpenAsync` before the capture, with the defaults snapshot
restored. The Content Security Policy tab is the one shown. The table has these rows, in this order, each with an empty
box (`M/OrchardCore.Security/SecurityHeaderDefaults.cs:11-28`, labels `M/OrchardCore.Security/Views/ContentSecurityPolicySettings.Edit.cshtml:14-56`):
Base URI, Child Source, Connect Source, Default Source, Font Source, Form Action, Frame Ancestors, Frame Source, Image
Source, Manifest Source, Media Source, Object Source, Script Source, Style Source, Sandbox. Row boxes:
`tr:has(td:text-is("<label>")) input`. The page is taller than the 864 px viewport, so use `ScrollAsync` as each step
says, and clear highlights before every scroll.

- `csp-1`: `HighlightAsync` the tab link `ul[role=tablist] a:text-is("Content Security Policy")`, then move the cursor
  down the first rows of the table. On screen when the narration ends: the Content Security Policy tab with the
  Enable Sandbox and Upgrade Insecure Requests checkboxes unchecked and the table with empty boxes. The Name column
  header is `M/OrchardCore.Security/Views/ContentSecurityPolicySettings.Edit.cshtml:88`.
- `csp-2`: highlight, one after the other and about 1 s each, the boxes of Default Source, Script Source, Style Source,
  Image Source and Font Source (scroll between them if one is out of view). Type nothing. On screen when the narration
  ends: all boxes still empty, with the last ring on Font Source.
- `csp-3`: `ClearHighlightsAsync`, scroll to the hint under the table ("Allows administrators to control resources the
  user agent is allowed to load for a given page.", `M/OrchardCore.Security/Views/ContentSecurityPolicySettings.Edit.cshtml:108`)
  and move the cursor to the Default Source box. Type nothing. On screen when the narration ends: the table and the hint,
  boxes empty.
- `csp-4`: scroll so the rows Frame Ancestors and Object Source are both in view. Then
  `TypeAsync(tr:has(td:text-is("Object Source")) input, "'none'")` and
  `TypeAsync(tr:has(td:text-is("Frame Ancestors")) input, "'self'")`, in this order (the narration names Object Source
  first). Type the single quotes. On screen when the narration ends: Object Source holds `'none'`, Frame Ancestors holds
  `'self'`, every other box is empty. Labels: lines 32 and 47 of the same view.
- `csp-5`: `ClearHighlightsAsync`, scroll to the top, `HighlightAsync(.form-check:has(label:text-is("Enable Sandbox")))`,
  pause about 3 s, clear it, `HighlightAsync(.form-check:has(label:text-is("Upgrade Insecure Requests")))`. Do not click
  either checkbox. On screen when the narration ends: both checkboxes unchecked, a ring on Upgrade Insecure Requests, and
  the hint "Enables a sandbox for the requested resource similar to the iframe sandbox attribute." under Enable Sandbox.
  Labels: lines 68, 70 and 78 of the same view.
- `csp-6`: `ClearHighlightsAsync`, scroll back down so the two typed values are in view, and move the cursor over
  them. No other action. On screen when the narration ends: `'none'` in Object Source and `'self'` in Frame Ancestors.
- `csp-7`: as the first action, `ClickAndWaitForNavigationAsync("button.save")` (Save,
  `M/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`). The form is posted, the site restarts its tenant and
  the page redirects to `/Admin/Settings/SecurityHeaders`, which shows the first tab again. A green toast reads "Site
  settings updated successfully." (`M/OrchardCore.Settings/Controllers/AdminController.cs:119`) and hides itself after
  5 seconds (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`), so the click must be the
  step's first action. Wait for `.toast.message-success` to be visible, pause 2 s, then scroll down so the two saved values
  are in view. On screen when the narration ends (the toast is gone by then): the Content Security Policy tab with the
  warning still above the form and the saved `'none'` and `'self'` in Object Source and Frame Ancestors. If the
  redirect takes more than 4 seconds, the toast is gone before the narration says "a message confirms": record the clip
  again.

This clip changes the site: restore the defaults snapshot before every take, the first one included.

## Clip `04-permissions`

Starts on `/Admin/Settings/SecurityHeaders`, opened with `OpenAsync` before the capture, on the defaults snapshot just
restored. The table lists one row per
browser feature, in this order (`M/OrchardCore.Security/SecurityHeaderDefaults.cs:30-61`, labels
`M/OrchardCore.Security/Views/PermissionsPolicySettings.Edit.cshtml:14-102`): Accelerometer, Ambient Light Sensor,
Autoplay, Battery, Camera, Display Capture, Document Domain, Encrypted Media, Full Screen, Game Pad, Geolocation,
Gyroscope, Layout Animations, Legacy ImageFormat, Magnetometer, Microphone, Midi, Oversized Images, Payment, Picture In
Picture, Public Key Retrieval, Speaker Selection, Screen Wake Lock, Synchronous XML Http Request, Unoptimized Images,
Unsized Media, USB, Web Share, Web XR. Each row has three radio buttons, `input[name="<policy name>"][value="None" | "Any" | "Self"]`,
where the policy name is lower case, such as `geolocation`, and a box `#<policy name>AllowedOrigins` that is hidden until
Self is selected. Row cells: `tr:has(td:text-is("<label>"))`.

- `perm-1`: `ClickAsync("ul[role=tablist] a:text-is(\"Permissions Policy\")")`; this switches the tab without loading a
  page. Scroll so the rows Camera to Microphone are in view, then `HighlightAsync` the Geolocation row, then Camera, then
  Microphone, about 1 s each, in the order the narration says them (Geolocation, Camera, Microphone). On screen when the
  narration ends: the Permissions Policy tab with the rows in view, a ring on Microphone, and every radio on None.
- `perm-2`: `ClearHighlightsAsync`, scroll to the top of the table, and `HighlightAsync` the column headers
  `thead th:text-is("None")`, then `thead th:text-is("Any")`, then `thead th:text-is("Self")`, about 2 s each, so each
  ring appears when the narration says that word. On screen when the narration ends: a ring on the Self header and the
  columns Name, None, Any and Self over rows that are all on None. Labels:
  `M/OrchardCore.Security/Views/PermissionsPolicySettings.Edit.cshtml:114-117`. The hint under the table ("Allows a site to
  control which features and APIs can be used in the browser.") is line 169.
- `perm-3`: `ClearHighlightsAsync`, scroll so the Geolocation row is in view, `ClickAsync(input[name="geolocation"][value="Self"])`,
  wait for `#geolocationAllowedOrigins` to be visible, then `TypeAsync("#geolocationAllowedOrigins", "https://maps.example.com")`.
  Type the whole origin, with `https://`. On screen when the narration ends: in the Geolocation row Self is selected and
  the box next to it holds `https://maps.example.com`; every other row is on None. The box shows the product's placeholder
  until the origin is typed (see the private-data checks). The radios and the box:
  `M/OrchardCore.Security/Views/PermissionsPolicySettings.Edit.cshtml:156,161`; the script that opens the box and copies
  the value into a hidden field: `M/OrchardCore.Security/Assets/ts/permissions-policy-settings.ts`.
- `perm-4`: as the first action, `ClickAndWaitForNavigationAsync("button.save")`. Wait for `.toast.message-success`
  ("Site settings updated successfully."), pause 2 s, then `ClickAsync` the Permissions Policy tab again (the page shows the
  first tab after the redirect) and scroll so the Geolocation row is in view. On screen when the narration ends: the
  Permissions Policy tab with Geolocation on Self and `https://maps.example.com` in its box, the other rows on None.

This clip changes the site: restore the defaults snapshot before every take, the first one included.

## Clip `05-referrer`

Starts on `/Admin/Settings/SecurityHeaders`, opened with `OpenAsync` before the capture, on the defaults snapshot just
restored. The Referrer Policy tab holds one list, labeled "Referrer Policy", with these options in this order: No
referrer, No referrer when downgrade, Origin, Origin when cross origin, Same origin, Strict origin, Strict origin when
cross origin, Unsafe URL (`M/OrchardCore.Security/Views/ReferrerPolicySettings.Edit.cshtml:7-34`). The list is
`select[name$="ReferrerPolicy"]`.

- `ref-1`: `ClickAsync("ul[role=tablist] a:text-is(\"Referrer Policy\")")`, then `HighlightAsync` the list and, after
  about 3 s, the hint under it ("Allows a site to control how much information the browser includes when navigating away
  from a page.", line 36). On screen when the narration ends: the Referrer Policy tab with the list on "No referrer" and
  the hint.
- `ref-2`: `ClearHighlightsAsync`, then `SelectAsync(select[name$="ReferrerPolicy"], "same-origin")`. A headless browser draws
  no open list, so the click shows a ripple and the value changes. On screen when the narration ends: the list shows
  "Same origin". Select it during the third sentence of the narration, at the moment it says "Let's choose Same origin"
  (the first two sentences describe No referrer and Same origin while the list still shows "No referrer").
- `ref-3`: as the first action, `ClickAndWaitForNavigationAsync("button.save")`. Wait for `.toast.message-success`, pause
  2 s, then `ClickAsync` the Referrer Policy tab again (the first tab shows after the redirect). On screen when the
  narration ends: the Referrer Policy tab with the list on "Same origin", the warning above the form.

This clip changes the site: restore the defaults snapshot before every take, the first one included.

## Slides

- `why-1` (slide, chapter 1): three cards, "The browser asks", "The site answers", "The browser follows", all bright.
- `why-2` (slide): four cards, the first two bright: Content Security Policy and X-Content-Type-Options.
- `why-3` (slide): the same four cards, the last two bright: Permissions Policy and Referrer Policy.
- `why-4` (slide): four cards titled "What the site sends by default": X-Content-Type-Options and Referrer Policy with a
  check mark ("always sent"), Content Security Policy and Permissions Policy with "if" ("sent only after you give a value").
- `know-1` (slide, chapter 6): two cards, the first bright: "Permissions" ("The page takes Manage Security Headers
  Settings and Manage settings. Administrator has both."). The labels: `M/OrchardCore.Security/SecurityPermissions.cs:8`,
  `src/OrchardCore/OrchardCore.Settings.Core/SettingsPermissions.cs:8`.
- `know-2` (slide): the same two cards, the second bright: "Set by a developer".

## Off-camera checks

The headers never appear on camera, so check them outside the capture, with the recorder's probe, after each
saving clip (wait until the site answers a request after the save). With `MSYS_NO_PATHCONV=1` set, run:

```bash
<bin>/Recorder probe /Admin "async () => { const r = await fetch('/'); return Object.fromEntries([...r.headers.entries()].filter(([k]) => /^(content-security-policy|permissions-policy|referrer-policy|x-content-type-options)$/i.test(k))); }"
```

| When | Expected headers |
|---|---|
| Defaults, before any clip | `x-content-type-options: nosniff`, `referrer-policy: no-referrer`; no `content-security-policy`, no `permissions-policy` |
| After `03-csp` | the defaults, plus `content-security-policy: frame-ancestors 'self';object-src 'none'` |
| After `04-permissions` (from the snapshot) | the defaults, plus `permissions-policy: geolocation=(self "https://maps.example.com")` |
| After `05-referrer` (from the snapshot) | `referrer-policy: same-origin`; `x-content-type-options: nosniff`; no `content-security-policy`, no `permissions-policy` |

The code behind the values: `M/OrchardCore.Security/Startup.cs:38-41`, `M/OrchardCore.Security/Options/SecurityHeadersOptions.cs`,
`M/OrchardCore.Security/Services/PermissionsHeaderPolicyProvider.cs:28-74`. If a header differs from the table, do not
record: write what the site sent in `OPEN-QUESTIONS.md` and tell the author. In particular, a `content-security-policy`
header after `04-permissions` or `05-referrer` (for example one that lists empty directives) means the empty boxes of the
first tab were stored as empty text instead of nothing: stop at the first take and report it.

## Checks for private data

- The video shows only fake data: the site name "Orchard Core", the user menu "admin", the address
  `https://maps.example.com`. Nothing else is typed on camera except the two Content Security Policy values, the word
  "Security" in the search box and the options chosen in the clips.
- The allowed-origins box of a Permissions Policy row shows the product's placeholder, two example addresses on a domain
  that is not example.com (`M/OrchardCore.Security/Views/PermissionsPolicySettings.Edit.cshtml:161`), when it opens. It is
  visible for the moment between selecting Self and typing in `perm-3`. It is product text, not data, but it names a real
  domain: type right after the box opens, and check the contact sheet of the clip and the OCR of the video for it.
- No password is typed on camera; `state.json` keeps the session. Do not write the password in any file.
- The Features page lists feature ids and descriptions only. The page address is never shown (the capture is the page,
  not the browser window).
- After assembling, read the OCR of the video for any address that is not `example.com`, `localhost` or
  `orchardcore.net`, and for any person's name.

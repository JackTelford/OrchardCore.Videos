# Recording plan: Resources: Declare and Include Scripts and Stylesheets

Only one chapter of this video is a recording: chapter 07, clip `07-settings`, the Resources tab of **Settings, General**.
Every other chapter is a slide (drawn by `slides.py`, nothing to record). The Orchard Core source paths below are
relative to the root of a checkout of `OrchardCMS/OrchardCore` (the labels were read at commit `92db4a83`). In this
file a selector stands for `scene.Page.Locator(...)`, and `scene.Page.GetByLabel(...)` or `scene.Page.GetByRole(...)`
where the line says so.

## The site it needs

- **Orchard Core:** `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore`, on `http://localhost:5010`, with
  its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Setup:** Auto Setup, so no setup or login form is recorded (the Setup screen is not part of this video). Recipe
  **Blog**, tenant `Default`, site name **Orchard Core**, time zone `America/Los_Angeles`, database SQLite.
- **User:** `admin`, email `admin@example.com`, and the password chosen outside git, passed in `VIDEO_PASSWORD` (the plan
  never holds it; where a password is needed it reads `<password>`).
- **Features to enable:** none. The Resources module and the Settings module are always enabled
  (`IsAlwaysEnabled`: `src/OrchardCore.Modules/OrchardCore.Resources/Manifest.cs:10`,
  `src/OrchardCore.Modules/OrchardCore.Settings/Manifest.cs:10`).
- **Settings:** leave **Settings, General** exactly as Auto Setup leaves it. Before the first take, check on the
  Resources tab that **Use resources cache busting** is checked, **Use framework CDN (Content Delivery Network)** is not
  checked, **Site CDN (Content Delivery Network) base url** is empty, and **Resource Debug Mode** shows
  "From environment — disabled in Production, enabled otherwise". These are the defaults of the code
  (`SiteSettings.cs:29-31`, `ResourceOptions.cs:7-10`); the narration of `settings-3` to `settings-6` says so.
- **External services:** none. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, as `references/recording.md` asks, so
  the site never calls out. The CDN settings are only shown, never turned on, so no CDN is contacted.
- **Seed data:** none. The clip shows an admin page, not content.
- **Sign-in:** `Recorder setup` once, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, which writes `state.json`
  (git-ignored). Do not commit it.
- **Admin menu:** the **Settings** group must be collapsed when the capture starts, since `settings-1` expands it. The
  menu restores the groups the user last left open, so open `/Admin`, collapse **Settings** if it is open, and save the
  sign-in again.

## Order, and what changes the site

One clip, so there is no order to keep. `07-settings` changes nothing: it never clicks **Save**, so the settings stay at
their defaults and a new take can start at once. If a take saved the page by mistake, stop the site, restore the copy of
`App_Data` made after Auto Setup, and start it again.

| Clip | Starts on | Depends on | Changes the site |
|---|---|---|---|
| `07-settings` | `/Admin` (the admin home page), signed in | Nothing | No |

## Clip `07-settings`

Before `StartCaptureAsync`: `scene.OpenAsync("/Admin")` (the admin home page, not recorded; the Blog recipe does not
enable the Admin Dashboard feature), then wait until the **Settings** group of the admin menu is visible. The page is at 1536x864 CSS pixels, light color scheme.

Seven steps, in the storyboard's order. Keep the actions of each step shorter than its narration (the narration of a step
is 11 to 37 words, between 4 and 15 seconds).

### `settings-1`

Narration: "The resource settings are in the admin. Open Settings, then General."

Actions:

1. `ClickAsync` the **Settings** group of the admin menu: `figcaption#settings .nav-group-label` (a toggle-only
   `button`; it expands the group). Wait until `li#general a.item-label` is visible.
2. `ClickAndWaitForNavigationAsync` `li#general a.item-label` (the **General** link, to `/Admin/Settings/general`).
   Wait until the Resources tab link, `GetByRole(AriaRole.Tab, new() { Name = "Resources", Exact = true })`, is visible.

On screen when the narration ends: the **General** page of the settings, with its tabs (**Site**, **Resources**,
**Cache**) and the **Site** tab shown; in the admin menu **Settings** is open and **General** is the current item. The
page also shows a warning, "The website might be restarted upon saving the settings, potentially leading to temporary
unresponsiveness during the process." (`DefaultSiteSettingsDisplayDriver.cs:47`, `Views/Settings-Reload.Wrapper.cshtml:4`).
It is part of the page and takes some height from the Resources tab.

Label from: `src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36` (**Settings**) and `:39` (**General**); the
route is `AdminController.cs:50` (`Settings/{groupId}`, `groupId` is `general`).

### `settings-2`

Narration: "On the Resources tab, Settings for scripts and stylesheets holds the options."

Actions:

1. `ClickAsync` `GetByRole(AriaRole.Tab, new() { Name = "Resources", Exact = true })`.
2. Wait until the heading `h5:has-text("Settings for scripts and stylesheets")` is visible, then
   `HighlightAsync` it (no caption).

On screen when the narration ends: the **Resources** tab is the active one, its heading **Settings for scripts and
stylesheets** has the green ring, and the four options below it (two check boxes, a text box and a drop-down list)
are visible without scrolling.

Label from: `src/OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Resources.Edit.cshtml:11` (heading); the tab
is named by the placement `Content:1#Resources;20`, `Drivers/DefaultSiteSettingsDisplayDriver.cs:54`, and printed by
`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:59`.

### `settings-3`

Narration: "Use resources cache busting appends a version hash to the address of local scripts and stylesheets. The hash
changes with the file, so browsers fetch an edited file again. It is on by default."

Actions:

1. `ClearHighlightsAsync()`.
2. `HighlightAsync` the check box and its label, `GetByLabel("Use resources cache busting")` (use the label's parent
   `div.form-check` so the ring includes the hint text).

On screen when the narration ends: the ring is around the row **Use resources cache busting**, the box is checked, and
the hint "Whether cache busting (append a version) is used for scripts and stylesheets" is visible inside the ring.

Label from: `Settings-Resources.Edit.cshtml:19` (label) and `:20` (hint). Default: `SiteSettings.cs:31`.

### `settings-4`

Narration: "Use framework CDN (Content Delivery Network) serves registered resources that have a CDN address, such as jQuery,
from it instead of the local file. It is off by default, so a site works without an internet connection."

Actions:

1. `ClearHighlightsAsync()`.
2. `HighlightAsync` the row of `GetByLabel("Use framework CDN (Content Delivery Network)")` (the `div.form-check`).

On screen when the narration ends: the ring is around the row **Use framework CDN (Content Delivery Network)**, the box
is not checked, and the hint "Whether a framework CDN is used for registered scripts and stylesheets, such as jQuery,
or their local version" is visible inside the ring.

Label from: `Settings-Resources.Edit.cshtml:29` (label) and `:30` (hint). Default: `SiteSettings.cs:29`.

### `settings-5`

Narration: "Site CDN (Content Delivery Network) base url adds a prefix, such as the address of your own CDN, to the address of
local scripts and stylesheets. It is ignored when empty, or when debug mode is on."

Actions:

1. `ClearHighlightsAsync()`.
2. `HighlightAsync` `GetByLabel("Site CDN (Content Delivery Network) base url")` together with its hint (use the
   closest `div.ocat-wrapper`). Do not type in the box.

On screen when the narration ends: the ring is around the label, the empty text box and the hint "A base CDN URL
prefixed to the local scripts and stylesheets. It is disabled if the value is empty or if the Resource Debug Mode is
enabled, e.g. https://cdn.mysite.com" (the address in that hint is part of the page).

Label from: `Settings-Resources.Edit.cshtml:36` (label) and `:40` (hint).

### `settings-6`

Narration: "Resource Debug Mode chooses between debuggable and minified files. By default, it follows the environment:
debuggable files everywhere except in Production. The other two options force one or the other."

Actions:

1. `ClearHighlightsAsync()`.
2. `HighlightAsync` `GetByLabel("Resource Debug Mode")` together with its hint (the closest `div.ocat-limited-wrapper`).
   Do not open the list and do not change the value.

On screen when the narration ends: the ring is around **Resource Debug Mode**, the drop-down list shows
"From environment — disabled in Production, enabled otherwise", and the hint "Determines whether scripts and
stylesheets load in their debuggable or minified form." is visible.

Label from: `Settings-Resources.Edit.cshtml:45` (label), `:48-50` (options) and `:52` (hint). The environment rule is
`src/OrchardCore.Modules/OrchardCore.Resources/ResourceManagementOptionsConfiguration.cs:1263-1264`.

### `settings-7`

Narration: "Saving these settings reloads the tenant, so the next request uses the saved values."

Actions:

1. `ClearHighlightsAsync()` (before any scroll: highlights are fixed-position).
2. If the **Save** button is not in the viewport, `ScrollAsync(0, 400)` until it is. Then `HighlightAsync`
   `GetByRole(AriaRole.Button, new() { Name = "Save" })`. Do not click it.

On screen when the narration ends: the ring is around the **Save** button, with the options of the Resources tab still
in view. Nothing is saved.

Label from: `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`. The reload is
`Drivers/DefaultSiteSettingsDisplayDriver.cs:140` (`RequestRelease`), which releases the shell when the request ends
(`src/OrchardCore/OrchardCore.Abstractions/Shell/IShellReleaseManager.cs`).

## The slides

Slides need no recording. Each line says what the slide of the step shows, so the narration can be checked against it.

- `why-1`: "Without a resource manager", three cards: the same library twice, the wrong order, tags everywhere.
- `why-2`: "The Resources module", three cards: Declare, Include, Render.
- `declare-1`: a manifest with a script and a style definition of `MyTheme`, and three cards: Name, Type, Version.
- `declare-2`: the definition of the script with `SetDependencies`, `SetUrl` and `SetCdn`, and cards on `SetUrl`, the
  tilde and `SetCdn`.
- `declare-3`: the options configuration class that adds the manifest, and the `Startup` method that registers it.
- `include-1`: a Liquid and a Razor view that include `bootstrap` by name (script at the foot, and style).
- `include-2`: the same script with a version, and cards for no version, a major number and major and minor numbers.
- `include-3`: the same script with `depends_on` (Liquid) and `depends-on` (Razor), and cards on where the dependencies
  come from.
- `include-4`: four cards for the `at` option: Head, Foot, Inline, Not set.
- `render-1`: a Liquid layout and a Razor layout with the `resources` tags, and a note on Header and Footer.
- `view-1`: three tags in Liquid and in Razor: a source only, a name and a source for a script, a name and a source for
  a style, with a note on each.
- `view-2`: unnamed script and style blocks in Liquid and in Razor, and two notes.
- `view-3`: a named script block that depends on jQuery in Liquid and in Razor, and two notes.
- `links-1`: the `link` tag in Liquid and Razor, and the element it renders.
- `links-2`: two `meta` tags with the same name in Liquid and Razor, and the combined element they render.
- `api-1`: a class that injects `IResourceManager` and calls `RegisterResource`, `UseVersion` and `AtFoot`.
- `api-2`: `RegisterHeadScript`, `RegisterFootScript`, `RegisterMeta` and `AppendMeta`, and the package that holds them.

## Checks for private data

The clip shows only the Settings pages of a fresh site with fake data: the site name **Orchard Core**, the user `admin`
and `admin@example.com`. Before assembling, look at the contact sheet of `07-settings` and OCR the video: no real name,
email address, host name or token must appear, the user menu must not show a personal name, and the browser must show
no password. The only address on screen is the example in the hint of **Site CDN (Content Delivery Network) base url**.

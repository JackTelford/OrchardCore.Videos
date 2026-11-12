# Recording plan: Robots

How to record `seo-robots` without deciding anything. Read `storyboard.json` for the narration; this file holds
everything else. Selectors below stand for `scene.Page.Locator(...)` (or the `GetByLabel` form where it says so).
They are read from the markup in the Orchard Core source, not from a running site: probe each one once with
`Recorder probe` before recording (see `OPEN-QUESTIONS.md`). Source paths are relative to
`src/OrchardCore.Modules/` of `OrchardCMS/OrchardCore` unless they start with `src/OrchardCore/`.

## The site it needs

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore` (the catalog was read at commit `92db4a83`) |
| Provisioning | Auto Setup, tenant `Default`, recipe `Blog`, site name `Orchard Core`, time zone `America/Los_Angeles`, SQLite |
| User | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed as `VIDEO_PASSWORD`; never written in this repository) |
| Address | `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data folder | its own `App_Data` outside both repositories (`ORCHARD_APP_DATA`), empty before Auto Setup |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address; this video calls no external service |
| Admin path | the default (`Admin`); do not set `OrchardCore:Admin:AdminUrlPrefix`, because the hint of the second checkbox shows it |
| Sign-in | `Recorder setup` once, outside the capture, saves `state.json` (git-ignored) |

Features: enable none before recording. The video enables **SEO** on camera, so SEO must be disabled at the start
(the Blog recipe does not enable it). **Sitemaps** and **Static File Provider** stay disabled for the whole take:
if Sitemaps were enabled, the Robots page would show a fourth option, "Include all sitemaps.", which the narration
introduces only on a slide. The Blog recipe already enables Contents, Media, Shortcode Templates (which brings
Shortcodes) and Recipes, the features SEO depends on, so enabling SEO enables only SEO.

Settings: none. Leave the **Base url** of Settings, General empty.

No `robots.txt` file may exist: `src/OrchardCore.Cms.Web` has no `wwwroot/robots.txt`, and the tenant has no
`App_Data/Sites/Default/wwwroot` folder. Check `http://localhost:5010/robots.txt` before the first take: it must answer
404 while SEO is disabled.

Seed data: none. The video shows only settings pages, the Features page and `/robots.txt`. Nothing is created
outside the capture except the sign-in.

External services and stand-ins: none.

## Order, dependencies and snapshots

1. Stop the site after Auto Setup and `Recorder setup`. Copy `App_Data` and `state.json` aside as **snapshot A**
   (SEO disabled, nothing saved).
2. Record `02-enable`. It changes the site: it enables SEO.
3. Stop the site and copy `App_Data` aside as **snapshot B** (SEO enabled, Robots settings never saved).
4. Record `03-default`. It changes nothing.
5. Record `04-settings`. It changes the site: it saves a rule in Additional rules.

| Clip | Needs | Changes the site | Before a new take |
|---|---|---|---|
| `01-why` | nothing (slides) | no | none |
| `02-enable` | snapshot A | enables SEO | restore snapshot A |
| `03-default` | snapshot B | no | none |
| `04-settings` | snapshot B | saves the Robots settings with a rule | restore snapshot B |
| `05-notes` | nothing (slides) | no | none |

Use a new browser context for every clip (the recorder does): a browser cache must never hold an earlier
`robots.txt`.

Plain-text pages: Chromium draws `/robots.txt` as small plain text. In `03-default` and `04-settings`, right after
navigating to `/robots.txt`, run `await scene.Page.EvaluateAsync("document.documentElement.style.zoom = '1.6'")` so the
lines are readable in the 1080p frame. It changes how the page is drawn, not the file.

## Slide steps

Chapters `01` and `05` are slides drawn by `slides.py`; nothing is recorded for them.

- `why-1`: three cards, "Web robots", "A robots.txt file", "Which parts to visit".
- `why-2`: three cards, "The SEO feature", "The Robots settings", "The robots.txt file".
- `why-3`: three cards named after the Robots settings: "Add a rule to allow all web robots.", "Add a rule to disallow web robots to the admin routes.", "Additional rules".
- `sitemaps-1`: three cards, the first ("Include all sitemaps.") bright, the other two dimmed.
- `sitemaps-2`: the same cards, "Base url" and "Sitemap lines" bright. The third card says the lines list enabled sitemap indexes and the enabled sitemaps outside them (`Sitemaps/Services/SitemapsRobotsProvider.cs:47-82`).
- `file-1`: heading "A physical robots.txt file takes precedence", two cards ("The site serves that file", "The Robots page can show a warning"), and a panel with the warning text of the Robots page.

## Clip `02-enable`

Starts on: `/Admin`, opened before `StartCaptureAsync()`, signed in as `admin`, with every menu group collapsed (a new
browser context starts that way).

### `enable-1`

Actions:
1. Click the menu button **Tools** (`figcaption#tools button`; the group is a toggle with no page of its own).
2. Wait until the link **Features** is visible in the group (`ul[data-title='Tools'] a:text-is('Features')`), then `ClickAndWaitForNavigationAsync` on it.
3. Wait until `#search-box` is visible, then `TypeAsync(#search-box, "SEO", 90)`.

Visible when the narration ends: the Features page with `SEO` in the search box, and the row **SEO** (with the id
`OrchardCore.Seo`) under the heading **Content Management**, its **Enable** button on the right. Other rows that
match "SEO" in their text may also be shown; that is fine.

Labels from: `OrchardCore.Settings/AdminMenu.cs:32` ("Tools"), `OrchardCore.Features/AdminMenu.cs:27` ("Features"),
`OrchardCore.Features/Views/Admin/Features.cshtml:27` (the "Search" box), `OrchardCore.Seo/Manifest.cs:4` ("SEO") and `:9` ("Content Management").

### `enable-2`

Actions:
1. `HighlightAsync` the SEO row (the `.list-group-item` that contains the text `OrchardCore.Seo`, or the row the probe finds), `PauseAsync(2000)`, `ClearHighlightsAsync()`.
2. `ClickAndWaitForNavigationAsync` on the **Enable** link of that row: `a[href*='/Features/OrchardCore.Seo/Enable']`. The page reloads on the Features page.
3. Wait until the text "The feature SEO was enabled." is visible. The message names only the feature that was requested, so it is the same whatever else the Blog recipe enabled.

Visible when the narration ends: the Features page after the reload with the success notification "The feature SEO was enabled." at the top.

Label from: `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` ("Enable"), the message from `OrchardCore.Features/Controllers/AdminController.cs:317`. The `Enable` action passes only the requested feature to the message (`AdminController.cs:247` and `src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:80`).

After the clip, snapshot B (see "Order").

## Clip `03-default`

Starts on: `/Admin/Features`, opened before `StartCaptureAsync()`. SEO is enabled (snapshot B).

### `default-1`

Actions:
1. `PauseAsync(3500)`.
2. `NavigateAsync("/robots.txt")`; wait until the page body contains `User-agent`.
3. Run the zoom (see "Order, dependencies and snapshots").

Visible when the narration ends: the plain-text page of `/robots.txt` with the lines `User-agent: *` and `Disallow: /Admin`, large enough to read.

Source: `OrchardCore.Seo/Startup.cs:53-56` (the route), `OrchardCore.Seo/Services/SiteSettingsRobotsProvider.cs:27-35` (the lines).

### `default-2`

Actions:
1. `HighlightAsync(scene.Page.Locator("pre"))` with no caption (Chromium puts a plain-text response in a `pre`).
2. Nothing else: the step holds until its narration ends.

Visible when the narration ends: the same two lines, ringed.

## Clip `04-settings`

Starts on: `/Admin/Features`, opened before `StartCaptureAsync()`. SEO is enabled and the Robots settings were never saved (snapshot B).

### `settings-1`

Actions (the menu path is the story, so open each group on camera, waiting for the next item to be visible):
1. Click the button **Settings** (`figcaption#settings button`).
2. Click the button **Search** in the group (`ul[data-title='Settings'] button:text-is('Search')`).
3. Click the button **Search Engine Optimization** (`ul[data-title='Search'] button:text-is('Search Engine Optimization')`).
4. `ClickAndWaitForNavigationAsync` on the link **Robots** (`ul[data-title='Search Engine Optimization'] a:text-is('Robots')`). The address is `/Admin/Settings/robotsSettings`.

Visible when the narration ends: the Robots page with two checked checkboxes, "Add a rule to allow all web robots." and "Add a rule to disallow web robots to the admin routes.", an empty text area labelled "Additional rules", and a **Save** button. No warning is shown.

Labels from: `OrchardCore.Seo/AdminMenu.cs:25` ("Settings"), `:26` ("Search"), `:27` ("Search Engine Optimization"), `:30` ("Robots");
`OrchardCore.Seo/Views/RobotsSettings.Edit.cshtml:11`, `:22`, `:30`; `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` ("Save").
Menu markup: `OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml` (src path `src/OrchardCore.Themes/TheAdmin/Views/`).

### `settings-2`

Actions:
1. `HighlightAsync` the `.form-check` that contains "Add a rule to allow all web robots.".
2. Nothing else: the step holds until its narration ends.

Visible when the narration ends: that checkbox, checked, with its hint "Adds: User-agent: *" ringed.

Labels from: `OrchardCore.Seo/Views/RobotsSettings.Edit.cshtml:11` (label), `:12` (hint).

### `settings-3`

Actions:
1. `ClearHighlightsAsync()`.
2. `HighlightAsync` the `.form-check` that contains "Add a rule to disallow web robots to the admin routes.".
3. Nothing else: the step holds until its narration ends. Do not clear the checkbox.

Visible when the narration ends: the second checkbox, checked, with its hint "Adds: Disallow: /Admin" ringed.

Labels from: `OrchardCore.Seo/Views/RobotsSettings.Edit.cshtml:22` (label), `:23` (hint, the admin path follows `AdminOptions.AdminUrlPrefix`).

### `settings-4`

Actions:
1. `ClearHighlightsAsync()`.
2. `TypeAsync(scene.Page.GetByLabel("Additional rules"), "Disallow: /drafts/", 60)`, then press Tab (a text box applies its value when it loses focus).

Visible when the narration ends: the text area "Additional rules" holding `Disallow: /drafts/`, with its hint "Optionally, you can include any additional rules." under it.

Labels from: `OrchardCore.Seo/Views/RobotsSettings.Edit.cshtml:30` (label), `:33` (hint).

### `settings-5`

Actions:
1. `ClickAndWaitForNavigationAsync` on the **Save** button (`button.save`). The page reloads on `/Admin/Settings/robotsSettings`.
2. Wait until the text "Site settings updated successfully." is visible.

Visible when the narration ends: the Robots page with the notification "Site settings updated successfully." and the text area still holding `Disallow: /drafts/`.

Label from: `OrchardCore.Settings/Controllers/AdminController.cs:119`.

### `settings-6`

Actions:
1. `PauseAsync(1500)`.
2. `NavigateAsync("/robots.txt")`; wait until the page body contains `drafts`.
3. Run the zoom.
4. `HighlightAsync(scene.Page.Locator("pre"))` with no caption.

Visible when the narration ends: three lines, `User-agent: *`, `Disallow: /Admin`, `Disallow: /drafts/`, in that order, ringed. Sitemaps is disabled, so no `Sitemap:` line is present.

Source: `OrchardCore.Seo/Services/SiteSettingsRobotsProvider.cs:27-40` (the order within the file).

## After recording

Restore snapshot A before anyone records another video on the same data folder. Stop the site by its process ID.

## Checks for private data

- The only name on screen is the user `admin`; the only address is `localhost`. Run the OCR check of the skill (one frame per second and at scene changes) and look for any other name, email address or token.
- `/robots.txt` shows only the three lines above. The rule `Disallow: /drafts/` is invented.
- No password is typed on camera (the sign-in is done outside the capture) and `state.json` is never committed.

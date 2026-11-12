# Recording plan: Static Files per Tenant

The video has two recordings (`02-enable` and `04-serve`) and six slide clips. Everything else is drawn by `slides.py`.
A selector in this plan stands for `scene.Page.Locator(...)`. File references are to the Orchard Core source the
catalog was read from (commit `92db4a83`); `src/...` is its root. The narration lives in `storyboard.json`; read each
step's text before you record it, since the actions must be shorter than the narration (about 12 seconds for 30 words).

## The site it needs

Run `src/OrchardCore.Cms.Web` on `http://localhost:5010`, with a fresh `ORCHARD_APP_DATA` folder outside both
repositories, written `<app-data>` below. Give it as an absolute path: `ShellOptionsSetup.cs:20-31` combines it with the
content root, and the tenant folders of this video are `<app-data>/Sites/<Tenant>/wwwroot`. Set `HTTP_PROXY` and
`HTTPS_PROXY` to a dead address, so the site never calls an outside service. The site has no external service.

**Set it up with Auto Setup**, which installs the three tenants on their first request
(`src/docs/reference/modules/AutoSetup/README.md`). The site is recorded after setup, so no setup screen and no
sign-in form is shown. The password is the one chosen outside git and passed in `VIDEO_PASSWORD`; it is never written in
a file of the repository, and the commands below read it from that variable.

```bash
export OrchardCore__AutoSetup__AutoSetupPath=""

export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=SaaS

export OrchardCore__AutoSetup__Tenants__1__ShellName=Maple
export OrchardCore__AutoSetup__Tenants__1__SiteName="Maple Bakery"
export OrchardCore__AutoSetup__Tenants__1__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__1__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__1__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__1__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__1__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__1__RecipeName=Blog
export OrchardCore__AutoSetup__Tenants__1__RequestUrlPrefix=bakery

export OrchardCore__AutoSetup__Tenants__2__ShellName=Cedar
export OrchardCore__AutoSetup__Tenants__2__SiteName="Cedar Garden"
export OrchardCore__AutoSetup__Tenants__2__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__2__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__2__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__2__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__2__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__2__RecipeName=Blog
export OrchardCore__AutoSetup__Tenants__2__RequestUrlPrefix=garden
```

Then request `http://localhost:5010/`, `http://localhost:5010/bakery` and `http://localhost:5010/garden` once, in that
order (the Default tenant first), to install them.

| Tenant | Technical name | URL prefix | Recipe | Why |
|---|---|---|---|---|
| Default | `Default` | none | SaaS | The SaaS recipe enables `OrchardCore.Tenants` and `OrchardCore.Features` (`src/OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json:28,21`). The Blog recipe, the usual choice, does not enable the Tenants module, which the video needs. |
| Maple | `Maple` | `bakery` | Blog | Its `Static File Provider` is enabled on camera. The Features feature is always enabled (`src/OrchardCore.Modules/OrchardCore.Features/Manifest.cs:17`), so every tenant has Tools, Features. |
| Cedar | `Cedar` | `garden` | Blog | Its `Static File Provider` is enabled before recording. |

The technical name differs from the prefix on purpose: the video says the folder is named after the tenant, not after
its prefix (`src/OrchardCore.Modules/OrchardCore.Tenants/Startup.cs:104-105`).

Check that **Multi-Tenancy, Tenants** lists Default, Maple and Cedar, all with the **Running** badge, and that the
list shows its default layout (rows with buttons). If the admin setting **List layout** was changed, set it back to
**Use configuration default**.

### Features to enable before recording

- Default: none beyond the SaaS recipe.
- Maple: none. The video enables `Static File Provider` on it (clip `02-enable`).
- Cedar: enable `Static File Provider` by hand, in a normal browser signed in to the Default tenant as `admin`:
  Multi-Tenancy, Tenants, the **Features** button of the Cedar row, type `static` in the search box, **Enable**.
  Check that the message "The feature Static File Provider was enabled." appears.

No setting, content item or other data is needed.

### The seed files (created outside the capture)

The files are in the tenant folders before the first take. The feature uses a `wwwroot` folder that already exists
(`Startup.cs:73-76` only creates it when it is missing). Run this once, after the tenants are installed:

```bash
APP_DATA="<app-data>"   # the folder given to ORCHARD_APP_DATA
mkdir -p "$APP_DATA/Sites/Maple/wwwroot" "$APP_DATA/Sites/Cedar/wwwroot"
printf '# robots.txt of Maple Bakery\nUser-agent: *\nDisallow: /drafts\n' > "$APP_DATA/Sites/Maple/wwwroot/robots.txt"
printf '# robots.txt of Cedar Garden\nUser-agent: *\nDisallow: /\n' > "$APP_DATA/Sites/Cedar/wwwroot/robots.txt"
```

Check the starting state (state S0) in a browser:

- `http://localhost:5010/garden/robots.txt` shows the three lines of Cedar's file.
- `http://localhost:5010/bakery/robots.txt` answers 404, because Maple does not have the feature yet. If it shows a
  robots file, the SEO feature is enabled on Maple: disable it, since the video relies on the plain static file.

### Recorder setup

No `PrepareAsync` is needed: the site is seeded by the steps above. Run `Recorder setup` once to sign in to the
Default tenant (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD`), which saves `state.json` (never committed). The two clips
need no mask script: every name on screen is invented, and the site is fresh.

## Order of the clips

1. Stop the site and copy `<app-data>` and `state.json` aside. This copy is state S0. Restore both together, since
   the sign-in cookie is protected by keys kept in `<app-data>`.
2. Record `02-enable`. It **changes the site**: it enables `Static File Provider` on Maple. Before another take of
   this clip, stop the site, restore the S0 copy, and start the site again. The restore undoes the feature on Maple, so record `02-enable` again before `04-serve`.
3. Record `04-serve`. It needs `02-enable` to have been recorded (Maple serves its file only once the feature is
   enabled) and the seed files. It does not change the site, so it can be recorded again at any time.

The slide clips (`01-why`, `02-recipe`, `03-files`, `05-code`, `06-know`) are drawn by `slides.py` and need no site.

## Clip 02-enable (chapter 02, Enable the feature)

Starts on `/Admin` of the Default tenant (`scene.OpenAsync("/Admin")`, signed in through `state.json`), with the admin
menu as it opens. Scene: `02-enable`.

`enable-1`
- Click the admin menu item **Multi-Tenancy** (`li.menu-multitenancy`; its id is `multitenancy`), with a 600 ms pause.
  Label: `src/OrchardCore.Modules/OrchardCore.Tenants/AdminMenu.cs:30`.
- `ClickAndWaitForNavigationAsync` on the link **Tenants** under it (`AdminMenu.cs:33`). If the menu cannot be driven
  reliably, `NavigateAsync("/Admin/Tenants")` gives the same page.
- Wait until the list shows a row for Maple (the link `a:text-is("Maple")`, `Views/ShellSettingsEntry.Fields.SummaryAdmin.cshtml:9`).
- `HighlightAsync` on the **Features** button of the Maple row: `a[href*="/Admin/Features/Maple"]`. Label:
  `Views/ManageFeaturesActionButton.cshtml:6`; it is shown for running tenants other than Default (line 4), so the Default
  row has none.
- On screen when the narration ends: the Tenants list with the Default, Maple and Cedar rows, each with the Running
  badge (`Views/ShellSettingsEntry.State.SummaryAdmin.cshtml:17-18`), and a ring around the Features button of the Maple row.

`enable-2`
- `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` on the Features button of the Maple row. The page is
  `/Admin/Features/Maple` (`src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:58`) and its
  breadcrumb reads **Features** (`Views/Admin/Features.cshtml:15`). The page never writes the tenant's name, so the ring
  carries it:
  `HighlightAsync(scene.Page.Locator("#search-box"), "Features of the Maple tenant")` (`Features.cshtml:27`), held for
  about 2 seconds, then `ClearHighlightsAsync`.
- `TypeAsync(#search-box, "static")`. The list filters while you type (`Assets/ts/features-index.ts`, the `input` handler);
  only the category **Infrastructure** (`Features.cshtml:88`, the category in `Manifest.cs` of the Tenants module, line 22)
  remains, with one row.
- `HighlightAsync` on that row: `[data-filter-value]:not(.d-none)`.
- On screen when the narration ends: the search box holding `static`, the heading **Infrastructure**, and the single
  row **Static File Provider** with its description "Provides a way to serve independent static files for each tenant."
  (`Tenants/Manifest.cs:20-21`), the badge **Disabled** and the **Enable** button, with a ring around the row.

`enable-3`
- `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` on the **Enable** button of the row:
  `a#btn-enable-OrchardCore_Tenants_FileProvider` (`src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`;
  the id comes from the feature id with its dots turned into underscores, so confirm it with a probe, or use
  `[data-filter-value]:has-text("Static File Provider") a:text-is("Enable")`).
- Wait for the message "The feature Static File Provider was enabled." (`AdminController.cs:317`; the feature name is
  "Static File Provider", `Tenants/Manifest.cs:20`). The page returns to `/Admin/Features/Maple` with an empty search box.
- `TypeAsync(#search-box, "static")` again, so the row is on screen.
- On screen when the narration ends: the success message, and the row **Static File Provider** with the badge
  **Enabled** and the button **Disable**.

The remaining sentence of `enable-3` (a tenant can enable the feature itself, under Tools, Features) has no action:
labels from `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`. Leave the screen as it is.

## Clip 04-serve (chapter 04, Request the files)

Needs `02-enable` recorded and the seed files. A plain text file is drawn very small, so enlarge it before the first
page loads (outside the capture) with an init script on the page:

```csharp
await scene.Page.AddInitScriptAsync(
    "document.addEventListener('DOMContentLoaded', () => { if (document.contentType === 'text/plain') document.body.style.zoom = '2.4'; });");
await scene.OpenAsync("/bakery/robots.txt");
await scene.StartCaptureAsync();
```

The first request to Maple after the feature was enabled starts the tenant again and can take a few seconds: it
happens in `OpenAsync`, before the capture starts. The text pages need no sign-in.

`serve-1`
- `HighlightAsync(scene.Page.Locator("pre"), "/bakery/robots.txt")` (Chromium draws a text file inside a `pre`).
  The address bar is not part of the recording, so the caption is what tells the viewer the path.
- On screen when the narration ends: the three lines of Maple's file, large, with the ring and the caption
  `/bakery/robots.txt`:
  `# robots.txt of Maple Bakery`, `User-agent: *`, `Disallow: /drafts`.
  The files are served by `src/OrchardCore.Modules/OrchardCore.Tenants/Startup.cs:90-101`.

`serve-2`
- `ClearHighlightsAsync`, then `NavigateAsync("/garden/robots.txt")`, then
  `HighlightAsync(scene.Page.Locator("pre"), "/garden/robots.txt")`.
- On screen when the narration ends: the three lines of Cedar's file, large, with the ring and the caption
  `/garden/robots.txt`: `# robots.txt of Cedar Garden`, `User-agent: *`, `Disallow: /`.

If the ring does not follow the zoomed text on the contact sheet (`review.py`), remove the zoom from the init script
and record the text at its own size, or move the ring to the whole `body`.

## Slide steps

These steps are drawn by `slides.py`; no recording is made for them.

- `why-1`: "Static files of the application", three cards: one web root, served to every tenant, owned by the application.
- `why-2`: "The Static File Provider feature", four cards: a file provider for each tenant, a wwwroot folder for each tenant,
  served at the tenant's address, same names with different files.
- `recipe-1`: "Enable the feature in a recipe", the feature step of a recipe with `OrchardCore.Tenants.FileProvider` in its
  list of features to enable, and cards for the names `name`, `enable` and `disable`. Source: `src/docs/reference/modules/Recipes/README.md:96-100`.
- `files-1`: "The folder of each tenant", the tree `App_Data/Sites/Maple/wwwroot/robots.txt` and `Cedar/wwwroot/robots.txt` with
  their URL prefixes and paths, and three cards (named after the tenant, created by the feature if missing, served at the
  tenant's domain and prefix). Source: `Startup.cs:62,73-76,104-105`.
- `files-2`: "Files and subfolders", the tree of the Maple folder with a `robots.txt` and an `images/logo.png`, each with its
  URL, and two cards (no upload screen, same path under the tenant's address).
- `code-1`: "Registering the provider", the `FileProviderStartup` registration of `ITenantFileProvider` and `IStaticFileProvider`
  (`Startup.cs:56-84`), trimmed.
- `code-2`: "Serving the files", the `Configure` method with `UseStaticFiles` (`Startup.cs:86-102`), trimmed: the file provider,
  the default content type, unknown file types served, a 30-day cache comment.
- `code-3`: "Using the files in a view", an `img` with `~/images/logo.png` and `asp-append-version`, and the HTML rendered for the
  Maple tenant. Sources: `src/OrchardCore/OrchardCore/Modules/ModularTenantRouterMiddleware.cs:28-37` (the prefix is part of
  the path base) and `src/OrchardCore/OrchardCore.Mvc.Core/Startup.cs:150-151` (the file version provider).
- `know-1`: "Which file is served", two cards: the application's wwwroot first, for the same path (`src/OrchardCore.Cms.Web/Program.cs:26`), and
  the robots.txt of the tenant folder over the SEO robots settings (`src/docs/reference/modules/Seo/README.md:26`).
- `know-2`: "Removing a tenant", two cards: the site folder, wwwroot included, is removed with the tenant
  (`src/OrchardCore/OrchardCore/Shell/Removing/ShellSiteFolderRemovingHandler.cs:32-39`), and keep your own copy of the files.

## Checks for private data

- Every name on screen is invented: the tenants Default, Maple Bakery and Cedar Garden, the user `admin`, the address
  `admin@example.com`, the three seed files. The password is never shown; it is only typed outside the capture.
- OCR the contact sheets and the final video for `localhost`, a real person's name, an email address that is not
  `@example.com`, and the password. The caption of `serve-1` and `serve-2` shows only a path, never a host.
- Confirm that no admin notification other than "The feature Static File Provider was enabled." is on screen, and that the
  recording has no `App_Data` path, sign-in cookie or token.
- Do not commit `state.json`, the copy of `<app-data>`, the recordings or any audio.

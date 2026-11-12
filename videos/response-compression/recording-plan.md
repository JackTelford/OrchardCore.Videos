# Recording plan: Response Compression

The video has one recording (`02-enable`, four steps). Everything else is a card or a slide that `slides.py` draws, so
nothing else is recorded. In this plan a selector stands for `scene.Page.Locator(...)`. File references are relative to
`src/OrchardCore.Modules/` in the Orchard Core source at commit `92db4a83`, unless they start
with `src/`.

## The site it needs

- A fresh local site from `src/OrchardCore.Cms.Web`, run on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own
  `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). Run it in Kestrel, not behind IIS or a proxy, so
  nothing else compresses or rewrites the responses.
- **Auto Setup, no setup screen on camera.** Provision the Default tenant with these environment variables (the Auto
  Setup keys are in `src/docs/reference/modules/AutoSetup/README.md` of the Orchard Core source):
  - `OrchardCore__AutoSetup__Tenants__0__ShellName` = `Default`
  - `OrchardCore__AutoSetup__Tenants__0__SiteName` = `Orchard Core`
  - `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` = `America/Los_Angeles`
  - `OrchardCore__AutoSetup__Tenants__0__AdminUsername` = `admin`
  - `OrchardCore__AutoSetup__Tenants__0__AdminEmail` = `admin@example.com`
  - `OrchardCore__AutoSetup__Tenants__0__AdminPassword` = `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git)
  - `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` = `Sqlite`
  - `OrchardCore__AutoSetup__Tenants__0__RecipeName` = `Blog` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`)
- **Features to enable before recording:** none. The Features module is always enabled, and the clip needs Response
  Compression to be **disabled** when it starts. No recipe in the Orchard Core source enables it, so the Blog recipe
  leaves it off. Confirm that in the first take (see the starting state below).
- **Settings and seed data:** none. The clip shows no content items and no settings. Leave the admin lists at their
  defaults, the `List` layout with the actions as `Buttons` (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`;
  the Blog recipe sets neither, and the `AdminListOptions` lines of `src/OrchardCore.Cms.Web/appsettings.json:24-25` are
  comments). The selectors below depend on that: with the `Menu` layout the Enable and Disable links sit inside an
  Actions drop-down.
- **External services:** none. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (for example `http://127.0.0.1:9`)
  so the site never calls out.
- **Sign-in:** `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` saves `state.json`
  (git-ignored). No `prepare` step is needed: this video has no `PrepareAsync`.

### The starting state, and the snapshot

1. Start the site once, let Auto Setup finish, and run `Recorder setup`.
2. Run `Recorder probe /Admin/Features "() => document.getElementById('btn-enable-OrchardCore_ResponseCompression')?.textContent"`
   (with `MSYS_NO_PATHCONV=1` in Git Bash). It must print `Enable`. If it prints nothing, the button has another id: list
   the links of the row instead, with `() => [...document.querySelectorAll("[data-filter-value*='OrchardCore.ResponseCompression'] a")].map(a => a.id + ' ' + a.textContent.trim())`,
   and use what it prints. The id is built by `Html.GenerateIdFromName` (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`)
   and was not run when this plan was written, so the clip below selects the button by the row and its class, not by id.
3. Stop the site, and copy its `App_Data` folder and `state.json` aside. This is the snapshot.

## Order of the clips, and what changes the site

There is one clip, so no order. **`02-enable` changes the site:** it enables the Response Compression feature of the
Default tenant. Before every new take, stop the site, restore the snapshot's `App_Data`, and start the site again, so the
row shows the Disabled badge and the Enable button again.

Narrate before recording (`scripts/narrate.py videos/response-compression`): the recorder holds each step for the length
of its narration. The storyboard's `spoken` map holds new spellings (`gzip`, `Nginx`, `BREACH` and the identifiers); a
person confirms them with `scripts/pronounce.py` first, as the specification asks.

## Clip `02-enable`

**Starts on:** `/Admin` (the dashboard), opened with `scene.OpenAsync("/Admin")` before the capture. The admin menu is
**not** expanded: opening it is part of the first step. Before the capture, run `Recorder probe /Admin` to see how the
menu is built (the entries are `Tools`, then `Features`: `OrchardCore.Features/AdminMenu.cs:26-27`). Use the real
structure of the menu in the first step; if `Features` is already visible without opening `Tools`, skip the first click.

The browser is the recorder's: Chromium at 1536x864, light scheme, `en-US`.

### `enable-1`

Narration: open Tools, then Features; the page lists features by category; search for Response Compression in the
Infrastructure category (about 10 seconds).

Actions, shorter than the narration (about 6 seconds):

1. `ClickAsync` on the `Tools` entry of the admin menu, `pauseAfter: 700` (skip when `Features` is already visible).
2. `ClickAndWaitForNavigationAsync` on the `Features` link of the menu (its address ends with `/Admin/Features`).
3. Wait until `#search-box` is visible and the heading `h3` with the text `Infrastructure` is in the page.
4. `TypeAsync` on `#search-box` with `Response Compression`, delay 60 ms. The page filters its rows as you type (no
   Enter, no navigation: `OrchardCore.Features/Assets/ts/features-index.ts`, the `input` handler).

On screen when the narration ends: the **Features** page (the breadcrumb reads `Features`), the search box holding
`Response Compression`, one row left, under the heading `Infrastructure`, and the summary reading `Showing 1 of N
features` (N is the total of the site; do not depend on its value). The row shows the name `Response Compression`, its
identifier `OrchardCore.ResponseCompression` in a code style, and the description text `The Response Compression module
allows to enable the gzip compression.` (the description is in the module's manifest; the narration does not read it).

Labels from: `OrchardCore.Features/AdminMenu.cs:26-27` (`Tools`, `Features`), `OrchardCore.Features/Views/Admin/Features.cshtml:15`
(breadcrumb), `:27` (search box, placeholder `Search`), `:59` (`Showing {0} of {1} features`), `:88` (category heading),
`OrchardCore.ResponseCompression/Manifest.cs:9` (category `Infrastructure`), `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`
(name and identifier).

### `enable-2`

Narration: the row shows the feature's name, its identifier and a Disabled badge; select Enable (about 6.5 seconds).

Actions:

1. `HighlightAsync` on the identifier (`[data-filter-value*='OrchardCore.ResponseCompression'] code`), then on the badge
   (`[data-filter-value*='OrchardCore.ResponseCompression']` with the text `Disabled`), about 1.2 seconds each.
2. `ClearHighlightsAsync()`, then `PauseAsync(1200)` so that the click falls on the last words, `Select Enable`.
3. `ClickAndWaitForNavigationAsync` on the Enable link of the row: `[data-filter-value*='OrchardCore.ResponseCompression'] a.btn-primary`
   (the text is `Enable`). The link posts the form with a script and the page reloads on the Features page.

The first request after the click rebuilds the tenant and can take a few seconds (`OrchardCore.Features/Controllers/AdminController.cs:227-255`
redirects to the Features page, and the feature change releases the shell: `src/OrchardCore/OrchardCore/Shell/ShellHost.cs:174-175`).
`ClickAndWaitForNavigationAsync` waits for the new page, and the clip shows the old page meanwhile. Do not wrap it in
`OffCameraAsync`: that would cut the click itself out of the clip. If the wait lasts longer than about 4 seconds, stop the
take, warm the site by opening `/Admin` once, restore the snapshot and record again.

On screen when the narration ends: the Features page, reloaded, with the green confirmation message of the next step
already showing in the top right corner of the page (a toast that hides itself 5 seconds after the page loads, see
`enable-3`).

Labels from: `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15` (`Disabled`), `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`
(`Enable`).

### `enable-3`

Narration: the page confirms that the feature Response Compression was enabled; the tenant is rebuilt on its next
request and its pipeline then includes the compression middleware (about 10.5 seconds).

The confirmation is a toast, not text in the page: the admin theme draws it in the top right corner and hides it by itself
5 seconds after the page loads (`src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml`, `data-bs-delay`, line 20; the 5000 comes from
`SuccessAsync` in `src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`, which `OrchardCore.Features/Controllers/AdminController.cs:317`
calls). It is on screen for about the first sentence of the narration, which is the sentence that talks about it.

Actions (about 3 seconds), the first thing the step does:

1. `HighlightAsync` on the toast: `scene.Page.Locator(".oc-notify-toast-container .toast").Filter(new() { HasText = "The feature Response Compression was enabled." })`.
2. `HoldAsync(2.5)`.
3. `ClearHighlightsAsync()`, before the toast hides: a highlight is a fixed ring that stays on the page, and it would
   outline an empty corner.

The step then holds until its narration ends (`StepAsync` does that). If the toast is not on the page when the step starts,
`HighlightAsync` prints `nothing to highlight` after 3 seconds: stop the take, restore the snapshot and record again.

On screen when the narration ends: the Features page, reloaded, with the search box empty and no toast and no ring. This
is expected: the second sentence of the narration talks about the tenant, which has nothing to show.

Label from: `OrchardCore.Features/Controllers/AdminController.cs:317` (`The feature {1} was enabled.`, with the feature's name in
place of `{1}`; the name is `Response Compression`, from `OrchardCore.ResponseCompression/Manifest.cs:4`). The message names only the feature
that was selected, as `src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:74-89` (`EnableOrDisableFeaturesAsync`) passes it. The
tenant rebuild is stated by the comment at `src/OrchardCore/OrchardCore.Infrastructure/Shell/ShellDescriptorManager.cs:133`
and the code of `ShellHost.ChangedAsync` (`src/OrchardCore/OrchardCore/Shell/ShellHost.cs:174-175`).

### `enable-4`

Narration: the row shows an Enabled badge and a Disable button; the feature belongs to this tenant, so each tenant that
needs compression has the feature enabled separately (about 10.5 seconds).

Actions (about 6 seconds):

1. `ClearHighlightsAsync()`.
2. `TypeAsync` on `#search-box` with `Response Compression`, delay 60 ms, so the list shows the row again.
3. `HighlightAsync` on the badge of the row (the text `Enabled`), then on its Disable button
   (`[data-filter-value*='OrchardCore.ResponseCompression'] a.btn-danger`, the text `Disable`). Do **not** click Disable.

On screen when the narration ends: one row, `Response Compression`, with the `Enabled` badge and the red `Disable` button
highlighted.

Labels from: `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` (`Enabled`), `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53`
(`Disable`).

## Slide steps (drawn by `slides.py`, not recorded)

- `how-1`: a code panel titled "An HTTP exchange": a request with `Accept-Encoding: gzip` and a response with `Content-Encoding: gzip`.
- `how-2`: three cards: what compresses well, what gains little, and what compression costs.
- `recipe-1`: the Feature recipe step in `site.recipe.json`, with `OrchardCore.ResponseCompression` under `enable`, and two note cards for `name` and `enable`.
- `code-1`: `Startup.cs` of the module (the usings and the namespace are shown as `// ...`), with `ConfigureServices` (`AddResponseCompression`, `EnableForHttps = true`) bright.
- `code-2`: the same file, with `Order => -5` and `Configure` (`UseResponseCompression`) bright.
- `code-3`: three cards: the request header, the content type of the response, and the default list of types (HTML, CSS, JavaScript, JSON, XML, plain text and WebAssembly).
- `code-4`: the middle of the host's `Program.cs`: `UseStaticFiles()` before `UseOrchardCore()`.
- `check-1`: a terminal panel with two `curl` commands (one asks for gzip, one for br) and two cards naming the header each returns.
- `check-2`: three cards: a request without `Accept-Encoding`, several formats listed without weights (the middleware prefers Zstandard, then Brotli, then gzip), and a web server in front of the application.
- `when-1`: two cards: compress in front of the application, or in the application with the module.
- `when-2`: two cards: `EnableForHttps` is true, and the risk of attacks such as BREACH.

## Private data

The clip shows only feature names and the admin menu. Before assembling, look at every contact sheet (`review.py`) and
check that:

- no frame shows an email address other than `admin@example.com`, a real host name, a token, or a password (the sign-in
  is saved in `state.json` before the capture, so no sign-in form is recorded);
- the site name reads `Orchard Core` and the address bar is not part of the capture;
- no error page or empty list appears, and the Features list is the site's own (no copied `App_Data`).

Do not commit `state.json`, an `App_Data` copy, the audio, or an MP4.

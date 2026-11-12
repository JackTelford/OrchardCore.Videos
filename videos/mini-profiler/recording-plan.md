# Recording plan: Mini Profiler

How to record this video without deciding anything. A selector written below stands for `scene.Page.Locator(...)`;
`file:line` references are in the Orchard Core source the content was written from (commit `92db4a837d` of
`OrchardCMS/OrchardCore`, `src/OrchardCore.Modules/` unless stated). The narration of a clip is in
`storyboard.json`; narrate before recording (`durations.json`).

The video has four recordings (`02-enable`, `03-roles`, `04-profile`, `06-admin`) and five slide chapters (`01`, `05`,
`07`, `08`, `09`, drawn by `slides.py`; nothing to record). `OPEN-QUESTIONS.md` lists what nobody has seen on a running
site: read it before the first take.

## The site it needs

- **Host:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). Outbound calls are
  blocked with `HTTP_PROXY` / `HTTPS_PROXY` set to a dead address. The recorder's browser is not behind that proxy:
  TheBlogTheme asks Google Fonts for two stylesheets (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12-13`), so
  record on a machine that reaches the internet, or see `OPEN-QUESTIONS.md` (question 6).
- **Auto Setup** (the Setup screen is not recorded; see the `orchardcore-tester` skill, `references/autosetup.md`, for the
  variable names): `ShellName` `Default`, `SiteName` `Orchard Core`, `SiteTimeZone` `America/Los_Angeles`,
  `DatabaseProvider` `Sqlite`, `RecipeName` `Blog`, `AdminUsername` `admin`, `AdminEmail` `admin@example.com`,
  `AdminPassword` the value of `VIDEO_PASSWORD`, which is chosen outside git. Write `<password>` wherever a password
  would go; never a real one.
- **Themes:** the Blog recipe sets TheBlogTheme for the site and TheAdmin for the admin
  (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:83-87`). Both render the `Footer` zone
  (`TheBlogTheme/Views/Layout.liquid:44-46`, `TheAdmin/Views/Layout.cshtml:96`), which the widget needs.
- **Features:** the Blog recipe enables the Features and Roles modules the clips use. **Mini Profiler is not enabled by
  the recipe:** it must be disabled when `02-enable` starts, and enabled for `03-roles`, `04-profile` and
  `06-admin` (see the order below). Do not enable any other feature.
- **Settings:** none. Leave the admin list layout at its default.
- **Roles:** `Editor` exists after the Blog recipe (`blog.recipe.json:88-111`); `03-roles` opens it and saves nothing.
- **Seed data:** none. The Blog recipe's own content (a blog with one post, a menu, tags, a footer widget) is what
  `04-profile` shows. The recipe's authors are the admin user. Create nothing by hand.
- **External services:** none, so no local stand-in.
- **Sign-in:** `Recorder setup` signs in as `admin` with `<password>` and saves `state.json` (git-ignored). The sign-in
  is not recorded.
- **Init script (`VIDEO_INIT_SCRIPT`), kept outside the repository:** the widget's popup prints the name of the
  machine that serves the request (`.mp-machine-name`, from the upstream script). A script that adds one style
  element with this single rule hides it in every page, and holds no name:

  ```js
  document.addEventListener("DOMContentLoaded", () => {
      const style = document.createElement("style");
      style.textContent = ".mp-result .mp-info .mp-machine-name { display: none !important; }";
      document.head.appendChild(style);
  });
  ```

  The popup is rendered after the page loads, so the rule is in place before it exists.

## Order, snapshots and what changes the site

1. Provision the site with Auto Setup, **stop it, and copy `App_Data` and `state.json` aside as snapshot A** (Mini
   Profiler disabled). Start the site again.
2. Record **`02-enable`** from snapshot A. It **changes the site**: Mini Profiler becomes enabled. Stop the site and
   copy `App_Data` aside as **snapshot B** (Mini Profiler enabled).
3. Record `03-roles`, `04-profile` and `06-admin`, in any order, from snapshot B. None of them changes the site:
   `03-roles` never selects **Allow** or **Save**, and the other two only read pages.
4. A retake of `02-enable` starts from snapshot A (stop the site, restore, start). A retake of any other clip starts
   from snapshot B. Between takes of `04-profile` and `06-admin`, nothing needs restoring.

`PrepareAsync` is not needed: there is nothing to seed. Before `04-profile` and `06-admin`, request the page once or
twice outside the capture, so that the views are compiled and the first request does not show a slow cold start (the
clip's own instructions below do it with `OpenAsync`).

## Clip `02-enable`

**Starts on:** `/Admin` (opened with `OpenAsync`, not recorded), signed in as `admin`. Mini Profiler is disabled.

- `enable-1`: `NavigateAsync("/Admin/Features")`; the page title is **Features**
  (`OrchardCore.Features/Views/Admin/Features.cshtml:5,15`; the menu entry is **Tools**, **Features**,
  `OrchardCore.Features/AdminMenu.cs:26-27`, the route `Features/{tenant?}`, `Controllers/AdminController.cs:58`).
  Then `TypeAsync(#search-box, "Mini Profiler")` (`Features.cshtml:27`; the list filters as you type and Enter does
  nothing, `Assets/ts/features-index.ts:136-149`), and wait until `#list-summary` reads "Showing 1 of" followed by a
  number and "features". Then `HighlightAsync` on the category heading `.feature-group h3` with the text
  **Performance** (`Features.cshtml:88`; the category comes from `OrchardCore.MiniProfiler/Manifest.cs:9`).
  **Visible when the narration ends:** the **Performance** heading, and under it the row **Mini Profiler**
  `OrchardCore.MiniProfiler` with its description "Adds the Mini Profiler widget to pages."
  (`Manifest.cs:8`, `Views/FeatureEntry.Description.SummaryAdmin.cshtml`, `FeatureEntry.Fields.SummaryAdmin.cshtml`),
  a **Disabled** badge, and the **Enable** button.
- `enable-2`: `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync` on the **Enable** link
  `a[href*="/Features/OrchardCore.MiniProfiler/Enable"]` (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; select
  it by its address, because its id is built from the feature id). The site reloads the tenant; wrap the wait in
  `OffCameraAsync(() => scene.Page.GetByText("The feature Mini Profiler was enabled.").WaitForAsync())`
  (`Controllers/AdminController.cs:317`). **Visible when the narration ends:** the message "The feature Mini Profiler was
  enabled." at the top of the Features page. The widget may already be there too; do not wait for it and do not
  mention it.

Ends by leaving the page as it is. Take snapshot B after this clip.

## Clip `03-roles`

**Starts on:** `/Admin/Roles/Index` (`OpenAsync`), signed in as `admin`, Mini Profiler enabled. The page title is
**Roles** (`OrchardCore.Roles/Views/Admin/Index.cshtml:15`; the menu entry is **Access Control**, **Roles**,
`OrchardCore.Roles/AdminMenu.cs:18-19`).

- `roles-1`: `ClickAndWaitForNavigationAsync` on `a.edit[href$="/Roles/Edit/Editor"]` (the **Edit** button of the
  **Editor** row, `Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`). The page title is "Edit 'Editor' Role"
  (`Views/Admin/Edit.cshtml:6`). `TypeAsync(#search-box, "Mini Profiler")` (the field with the placeholder **Search
  permissions**, `Views/Admin/_RolePermissions.cshtml:49`), and wait until exactly two rows `tr[data-text]` are visible
  (`_RolePermissions.cshtml:119`; the page filters as you type). **Visible when the narration ends:** the group
  **Mini Profiler** `(OrchardCore.MiniProfiler)` (the legend, `_RolePermissions.cshtml:57-61`; the title is the
  feature's name because the permissions have no category, `Controllers/AdminController.cs:412-427`) with the two rows
  "View Mini Profiler widget on back end pages" and "View Mini Profiler widget on front end pages"
  (`OrchardCore.MiniProfiler/Permissions.cs:8-9`), in that order, under the columns **Permission**, **Allow** and
  **Effective**, with both **Allow** boxes unchecked.
- `roles-2`: the narration names the front end permission first, then the back end one, but the page lists them
  alphabetically (back end first, `_RolePermissions.cshtml:90-93`). `HighlightAsync` on
  `tr[data-text="View Mini Profiler widget on front end pages"]`, `PauseAsync(4500)` (the first sentence of the
  narration is about that long), `ClearHighlightsAsync()`, `HighlightAsync` on
  `tr[data-text="View Mini Profiler widget on back end pages"]`, `PauseAsync(4500)`, then `HighlightAsync` on the front
  end row again so that both rows are ringed, and hold until the narration ends. **Do not** select an **Allow** box and **do not** select **Save**: the take must leave the role as
  it is. **Visible when the narration ends:** the same two rows, highlighted. Clear the highlights, then leave.

## Clip `04-profile`

**Starts on:** `/` (home page), signed in as `admin`, Mini Profiler enabled. Before `StartCaptureAsync`, `OpenAsync("/")`
twice (the warm-up). Then wait for `body > .mp-results .mp-button` to be visible, with a 30 second limit: the widget is
created once the page has loaded. The page may collect more than one result button; **every action below uses the
first one**, `.mp-results .mp-result >> nth=0`.

- `profile-1`: `NavigateAsync("/")`, wait for the first `.mp-button`, then `HighlightAsync` on it. The widget is the
  upstream MiniProfiler widget in its default position, the top-left corner (documentation, "View profiling results";
  `Views/MiniProfiler.cshtml:4-7` renders it without options on site pages). The page is the Blog home page, whose item
  is the blog (`blog.recipe.json:113-119`). The button shows the time of the request in milliseconds. **Visible when the
  narration ends:** the Blog home page in TheBlogTheme, with the widget button at the top-left corner and its ring.
  Say nothing about the number; it changes on every take.
- `profile-2`: `ClearHighlightsAsync()`, `ClickAsync` on the first `.mp-button`. Wait for the first result's
  `.mp-popup` to be visible. Then `HighlightAsync` on the table header cell `th` with the text **duration (ms)**
  inside that popup. **Visible when the narration ends:** the popup with the tree of timings (a table whose first
  rows are the request, its filters and its views), the **duration (ms)** column ringed, and no machine name in the
  popup header (the init script hides it). The label **duration (ms)** is written by the upstream script, not by Orchard
  Core: see `OPEN-QUESTIONS.md`, question 2.
- `profile-3`: `ClearHighlightsAsync()`, then `ClickAsync` on `.mp-popup a.mp-toggle-trivial` with the text
  **show trivial**. This link exists only when the profile has quick steps (see `OPEN-QUESTIONS.md`, question 2). Wait for `tr.oc-mp-shape` to be visible (Orchard Core's script gives that class
  to every row whose label starts with **Shape:**, `Assets/ts/miniprofiler.ts:114-124`; the label is written by
  `ShapeStep.cs:34`). Then `ScrollIntoViewIfNeededAsync` on the first `tr.oc-mp-shape`, then `HighlightAsync` on its
  `.mp-label`, in that order (a highlight is fixed on the screen: scroll first). **Visible when the narration ends:**
  rows whose labels read "Shape: " followed by a shape type, with the first one ringed. The link reads **hide
  trivial** afterwards.
- `profile-4`: `ClearHighlightsAsync()`, then `ClickAsync` on the first `.mp-popup a.mp-queries-show` (the SQL
  column's link: the SQL duration of a step, `Assets/ts/miniprofiler.ts:286`). Wait for `.mp-overlay .mp-queries
  tr.oc-mp-query` to have at least one match (`miniprofiler.ts:258`). Then set the overlay's scroll back to the top:
  `scene.Page.Locator(".mp-overlay .mp-queries").EvaluateAsync("e => e.scrollTop = 0")`, and `HighlightAsync` on the first
  `.mp-queries tr.oc-mp-query pre code` (the SQL statement). **Visible when the narration ends:** the queries view over a
  dimmed page: the column heading **Query** with the hint **In the order they ran**
  (`Views/MiniProfiler.cshtml:20-21`), the first row "#1" with its facts **Duration**, **Started at**, **First result**
  (when the query has one) and **Step** (`Views/MiniProfiler.cshtml:22-25`, `miniprofiler.ts:226-253`), and its SQL
  statement ringed on the right.

Ends on the queries view. Nothing is saved; the next clip opens its own page.

## Clip `06-admin`

**Starts on:** `/Admin/Features` (`OpenAsync`), signed in as `admin`, Mini Profiler enabled. Before `StartCaptureAsync`:
`OpenAsync("/Admin/Features")` twice (the warm-up), then run
`scene.Page.EvaluateAsync("localStorage.removeItem('OrchardCore.MiniProfiler.Collapsed')")` (the key that remembers a
collapsed widget, `Assets/ts/miniprofiler.ts:16`) and `OpenAsync("/Admin/Features")` once more. Wait for `#oc-mp-toggle` to
be visible: the script shows it when it docks the widget (`miniprofiler.ts:304-307`).

- `admin-1`: `MoveToAsync` the button `#oc-mp-toggle`, `ClickAsync` it (the widget tucks away; the button's title becomes
  **Show MiniProfiler**), `PauseAsync(900)`, `ClickAsync` it again (the widget comes back; the title is **Hide
  MiniProfiler**; both titles are in `Views/MiniProfiler.cshtml:15`, applied by `miniprofiler.ts:32-44`). Keep the cursor
  on the toggle. The labels **Hide MiniProfiler** and **Show MiniProfiler** are the toggle's `title` and `aria-label`; a
  browser tooltip is not part of a captured page, so the viewer may see only the arrow (`‹` or `›`) while the narration
  names the label. This is expected: do not add a caption. **Visible when the narration ends:** an admin page (Features) with the widget docked in the middle of
  the right edge, expanded, with its toggle on top (`Views/MiniProfiler.cshtml:8-16`: the position is right unless the
  language is right-to-left).

## Slides (nothing to record)

- `what-1`: four cards: request timings, MVC filters and views, rendered shapes, YesSql commands.
- `what-2`: four cards: a specific problem, added work, exposed details, limited access.
- `flags-1`: three cards: hover a badge, **Duplicate**, **Error** (`Views/MiniProfiler.cshtml:26-30`).
- `flags-2`: three cards: the "<n> ms without queries" row, the "Mostly in <step> (<n> ms)" row, and the red
  exclamation mark (`Views/MiniProfiler.cshtml:31-35`, `miniprofiler.ts:158-169,278-293`).
- `conf-1`: the options registered in `Program.cs` (the documentation's example, with its `using`), and three notes.
- `conf-2`: three cards: widget permissions, `ResultsAuthorize` or `ResultsAuthorizeAsync`, `ShouldProfile`.
- `custom-1`: the documentation's `Step` example in `MyService.cs`, and two notes.
- `missing-1`: four cards: the feature, the permission, the `Footer` zone, expired results.

## Checks for private data

Before assembling, OCR the clips (every second, at every scene change, and at 5 frames per second) and search the text for:

- the name of the machine that ran the site: the popup header must not show it (`.mp-machine-name` is hidden by the init
  script; check the popup and the queries view on every frame of `profile-2`, `profile-3` and `profile-4`);
- the Windows user name, a drive letter or a folder of this computer in a stack trace snippet, a SQL statement or a
  tooltip (the queries view prints the statements of the request);
- any email address other than `admin@example.com`, and any password or token (none is typed in this video; the
  sign-in comes from `state.json`);
- real people in the Blog recipe's content (there are none to expect; the check confirms it);
- the real address of the site: only `localhost` may appear.

Never publish a take that shows one of these: fix the site or the init script, and record again.

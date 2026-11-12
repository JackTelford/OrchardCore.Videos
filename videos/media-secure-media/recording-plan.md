# Recording plan: Secure Media

How to record the video without deciding anything. Paths such as `src/OrchardCore.Modules/...` are relative to the
Orchard Core checkout the labels were read from (commit 92db4a83). A selector written as `#search-box` stands for
`scene.Page.Locator("#search-box")`. Nothing in this folder is a real person's data: every name, address and password
below is fake or comes from outside git.

## The site it needs

- A checkout of `OrchardCMS/OrchardCore` at or after commit 92db4a83, running `src/OrchardCore.Cms.Web` on
  `http://localhost:5010`, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`), the tenant
  `Default`, and outbound calls blocked (`HTTP_PROXY` and `HTTPS_PROXY` set to a dead address).
- Provisioned by Auto Setup (no setup form, no login form is recorded), with these values (option names as in
  `src/docs/reference/modules/AutoSetup/README.md` of the Orchard Core checkout):

  | Setting | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (the same value as `VIDEO_PASSWORD`, chosen outside git) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- The Blog recipe is what this video needs, and it already holds everything except the folders below:
  - It enables Media and Roles (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:42` and `:65`).
  - It creates the Editor role (`blog.recipe.json:96`). Anonymous, Authenticated and Administrator are system roles.
  - It puts `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg` in the root folder of the Media Library
    (`blog.recipe.json:1124-1139`). The Blog home page uses `home-bg.jpg` as the picture behind its title
    (`blog.recipe.json:985`, `src/OrchardCore.Themes/TheBlogTheme/Views/Content-Blog.liquid:4-9`), and the home
    page of the site is the Blog (`blog.recipe.json:114-119`). That picture is what the visitor clips show.
- **Secure Media must be disabled** when recording starts: Tools, Features shows Secure Media as Disabled. It is
  enabled on camera in `02-enable`. Enable no other feature.
- External services: none. No mail, SMS or payment provider is called.
- Sign in once outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` writes
  `state.json` (git-ignored). The admin clips use it. The visitor clips use no sign-in (see "Visitor clips").

### Seed data (outside the capture)

Create two empty folders in the root folder of the Media Library, once, before Secure Media is enabled:

1. Sign in as `admin`, and open Media, Library (`/Admin/Media`).
2. In the folder panel, open the menu of the root folder (the ellipsis button titled "Folder Action"), choose
   "Create a subfolder", type `press` in the box with the placeholder "Enter a folder name", and press Enter.
3. Do the same for `contracts`.

Source of the labels: `src/OrchardCore.Modules/OrchardCore.Media/Services/MediaJSLocalizer.cs:62-65` and
`Assets/media-gallery/src/components/ModalFolderAction.vue`. No file is needed in the folders: a permission exists for
every folder, empty or not. Create no other folder and upload nothing: the permission list of the role editor shows
one row per folder of the root, and the video names only `press` and `contracts` (`_Users` and `mediafields` never get a
folder permission, `src/OrchardCore.Modules/OrchardCore.Media/SecureMediaPermissions.cs:135-139`).

Check before going on: Media, Library lists `press` and `contracts`, next to the three pictures of the Blog recipe;
Tools, Features shows Secure Media as Disabled.

Then stop the site, copy its `App_Data` folder and `state.json` aside as **snapshot A**, and start the site again.

## Visitor clips

`02-visitor-before`, `02-visitor-after` and `04-visitor` show the site as a visitor who is not signed in. The
`NewSceneAsync` helper of the `Scenes.cs` skeleton always passes `state.json` to `Scene.StartAsync`; these three clips call
`Scene.StartAsync(browser, name, BaseUrl, <a path that does not exist, such as state-visitor.json in the workspace>, ...)`
instead. When that file is missing, the browser context starts without a sign-in
(`.agents/skills/orchardcore-video-producer/scripts/recorder/Scene.cs:109`). Never create that file.

## Order of the clips, and the state each one needs

Record in this order, which is the order of the storyboard. Only three clips change the site.

| Order | Clip | Signed in as | Starts from | Changes the site |
|---|---|---|---|---|
| 1 | `02-visitor-before` | nobody | snapshot A | no |
| 2 | `02-enable` | admin | snapshot A | **yes**: enables Secure Media |
| 3 | `02-visitor-after` | nobody | snapshot **B** | no |
| 4 | `04-anonymous` | admin | snapshot B | **yes**: the Anonymous role gets a permission |
| 5 | `04-visitor` | nobody | snapshot **C** | no |
| 6 | `05-editor` | admin | snapshot C | **yes**: the Editor role gets a permission |

- Snapshot B: take it right after `02-enable` (stop the site, copy `App_Data` and `state.json` aside, start it again).
  Clips 3 and 4 start from it.
- Snapshot C: take it right after `04-anonymous`. Clips 5 and 6 start from it.
- To record a clip again, restore the snapshot it starts from (stop the site, put the copy of `App_Data` back, start
  the site, wait until `/` answers). To record `02-visitor-before` or `02-enable` again, restore snapshot A.
- `02-visitor-after` must be recorded before `04-anonymous`, since `04-anonymous` opens the root folder to visitors.
- Wait until the Blog home page (`/`) answers before each take, so the first request is not recorded slow.

## Clips

### `02-visitor-before` (Chapter 2)

Visitor scene. `OpenAsync("/")` opens the Blog home page before the capture, then `StartCaptureAsync()`.

- `before-1`: no click. `HighlightAsync(scene.Page.Locator("header.masthead"))` with no caption, held until the end of
  the narration. Visible at the end: the Blog home page of an anonymous visitor (no admin bar), with a photograph
  filling the header behind the title "Blog" and the subtitle "This is the description of your blog", and the green
  ring around the header. Source: `blog.recipe.json:967` and `:976`, `Content-Blog.liquid:4-9` (paths as in "The site it needs").

### `02-enable` (Chapter 2)

Admin scene. `OpenAsync("/Admin/Features")`, then `StartCaptureAsync()`. Starts on Tools, Features, with the search box
empty and every group listed.

- `enable-1`: `TypeAsync(scene.Page.Locator("#search-box"), "secure media", delay: 60)`, then wait until
  `#btn-enable-OrchardCore_Media_Security` is visible. Visible at the end: the search box holds "secure media", and the
  list shows one row under Content Management: the name "Secure Media" with the feature id next to it, the description
  "Adds permissions to restrict access to media folders.", the badge "Disabled" and the button "Enable". Source:
  `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features),
  `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27` (the search box),
  `src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs:67-69` (name and description),
  `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml` (the badge).
- `enable-2`: `HighlightAsync` on the row (the `.list-group-item` that contains `#btn-enable-OrchardCore_Media_Security`)
  for about 3 seconds, then `ClearHighlightsAsync()`, then `MoveToAsync` the button "Enable" and click it with
  `OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Locator("#btn-enable-OrchardCore_Media_Security").ClickAsync()))`:
  enabling a feature reloads the tenant, which can take several seconds, and the video should jump from the click to
  the result. Then wait until the text "The feature Secure Media was enabled." is visible. Visible at the end: the
  Features page after the reload, with the messages of the next step. Source:
  `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (the button).
- `enable-3`: `HighlightAsync` on the text "The feature Secure Media was enabled." and on the text that contains
  "Review and update role" (two rings, no captions), then `MoveToAsync` the link "Roles page" without clicking it.
  Visible at the end: a success message "The feature Secure Media was enabled." and a warning that reads "Secure Media
  is now enabled. Review and update role permissions to ensure users can access only the media they should.", with
  the links "Roles page" and "secure media documentation". Source:
  `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317` and
  `src/OrchardCore.Modules/OrchardCore.Media/Events/SecureMediaFeatureEventHandler.cs:39`.
- `enable-4`: `ClearHighlightsAsync()`, then `TypeAsync(scene.Page.Locator("#search-box"), "secure media", delay: 60)`,
  then `HighlightAsync` on the badge "Enabled" of the Secure Media row. Visible at the end: the Secure Media row with
  the badge "Enabled" and the button "Disable". Source: `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml` and
  `FeatureEntry.Buttons.SummaryAdmin.cshtml` (the button "Disable").

This clip changes the site: take **snapshot B** afterwards.

### `02-visitor-after` (Chapter 2)

Visitor scene, from snapshot B. `OpenAsync("/")`, then `StartCaptureAsync()`.

- `after-1`: `HighlightAsync(scene.Page.Locator("header.masthead"))` with no caption, held until the end of the
  narration. Visible at the end: the Blog home page with the same title "Blog" and subtitle, but **no photograph** in
  the header (the header shows its plain background), and the green ring. If the header looks the same as in
  `before-1`, the recording is wrong: check the picture's request in the browser (it must answer 404) before
  recording again. Source: `src/OrchardCore.Modules/OrchardCore.Media/Services/SecureMediaMiddleware.cs:50-57`.

### `04-anonymous` (Chapter 4)

Admin scene, from snapshot B. `OpenAsync("/Admin/Roles")`, then `StartCaptureAsync()`. Starts on the Roles page, which
lists Administrator, Anonymous, Authenticated, Author, Contributor, Editor and Moderator.

- `anon-1`: `HighlightAsync` on the link "Anonymous" for about 3 seconds, `ClearHighlightsAsync()`, then
  `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Anonymous", Exact = true }))`.
  Visible at the end: the page "Edit 'Anonymous' Role" (breadcrumb Roles / Edit 'Anonymous' Role), the Description
  box, the two lines explaining Allow and Effective, and the search box with the placeholder "Search permissions".
  Source (all in `src/OrchardCore.Modules/OrchardCore.Roles/`): `AdminMenu.cs:18-19` (Access Control, Roles),
  `Views/RoleEntry.Fields.SummaryAdmin.cshtml:7` (the link), `Views/Admin/Edit.cshtml:6` (the title),
  `Views/Admin/_RolePermissions.cshtml:40`, `:43` and `:49` (the two explanations and the search box).
- `anon-2`: `TypeAsync(scene.Page.Locator("#search-box"), "media content", delay: 60)`, wait until
  `tr[data-text="View media content in the root folder"]` is visible, then
  `ClickAsync(scene.Page.Locator("input[name='Checkbox.ViewRootMediaContent']"))`. Visible at the end: only the
  group "Secure Media" with the hint "(OrchardCore.Media.Security)" is left, with the rows "View media content in all
  folders", "View media content in folder 'contracts'", "View media content in folder 'press'", "View media content in
  the root folder", "View others media content" and "View own media content", in that order, and the Allow box of
  "View media content in the root folder" ticked. Source: `src/OrchardCore.Modules/OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:57-61` (the group),
  `:68-70` (the columns), `:119-131` (the rows and boxes);
  `src/OrchardCore/OrchardCore.Media.Core/MediaPermissions.cs:32-38` (the four labels),
  `src/OrchardCore.Modules/OrchardCore.Media/SecureMediaPermissions.cs:13-16` (the folder labels).
- `anon-3`: `ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save"))`, then wait until the text "Role updated
  successfully." is visible. Visible at the end: the Roles page with the message "Role updated successfully." Source (in
  `src/OrchardCore.Modules/OrchardCore.Roles/`): `Views/Admin/Edit.cshtml:33` (the button "Save"),
  `Controllers/AdminController.cs:213`.

This clip changes the site: take **snapshot C** afterwards.

### `04-visitor` (Chapter 4)

Visitor scene, from snapshot C. `OpenAsync("/")`, then `StartCaptureAsync()`.

- `visitor-1`: `HighlightAsync(scene.Page.Locator("header.masthead"))` with no caption, held until the end of the
  narration. Visible at the end: the Blog home page with the photograph back in the header, as in `before-1`.
  Source (in `src/OrchardCore.Modules/OrchardCore.Media/Services/`): `SecureMediaMiddleware.cs:50-57`,
  `ViewMediaFolderAuthorizationHandler.cs:86-91`.

### `05-editor` (Chapter 5)

Admin scene, from snapshot C. `OpenAsync("/Admin/Roles")`, then `StartCaptureAsync()`.

- `editor-1`: `HighlightAsync` on the link "Editor" for about 3 seconds, `ClearHighlightsAsync()`, then
  `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Editor", Exact = true }))`.
  Visible at the end: the page "Edit 'Editor' Role", with the Description box and the permission list below it.
  Source (in `src/OrchardCore.Modules/OrchardCore.Roles/`): `Views/RoleEntry.Fields.SummaryAdmin.cshtml:7`,
  `Views/Admin/Edit.cshtml:6`.
- `editor-2`: `TypeAsync(scene.Page.Locator("#search-box"), "media content", delay: 60)`, wait until
  `tr[data-text="View media content in folder 'press'"]` is visible, then `HighlightAsync` on that row and on
  `tr[data-text="View media content in folder 'contracts'"]` (two rings, no captions). Visible at the end: the six rows
  of the group "Secure Media" as in `anon-2`, with the rows for the folders `contracts` and `press` ringed, and every
  Allow box unticked. Source: `src/OrchardCore.Modules/OrchardCore.Media/SecureMediaPermissions.cs:13-16` (the label "View media content in
  folder" followed by the folder name),
  `src/OrchardCore.Modules/OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:119`.
- `editor-3`: `ClearHighlightsAsync()`, then
  `ClickAsync(scene.Page.Locator("input[name='Checkbox.ViewMediaContent_press']"))`. Visible at the end: the Allow box of
  "View media content in folder 'press'" ticked, and the Allow box of "View media content in folder 'contracts'"
  unticked. Source: `src/OrchardCore.Modules/OrchardCore.Media/SecureMediaPermissions.cs:13-16` (the permission name is
  "ViewMediaContent_" followed by the folder name).
- `editor-4`: `ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save"))`, then wait until the text "Role updated
  successfully." is visible. Visible at the end: the Roles page with the message "Role updated successfully." Source (in
  `src/OrchardCore.Modules/OrchardCore.Roles/`): `Views/Admin/Edit.cshtml:33`, `Controllers/AdminController.cs:213`.
- `editor-5`: `ClickAndWaitForNavigationAsync` on the link "Editor" again, `TypeAsync` "media content" in `#search-box`,
  wait until `input[name='Effective.ViewMediaContent_press']` is visible, then `HighlightAsync` on the Effective boxes
  of the `press` row (`input[name='Effective.ViewMediaContent_press']`) and the `contracts` row
  (`input[name='Effective.ViewMediaContent_contracts']`). Visible at the end: in the `press` row, the Allow and the
  Effective boxes are both ticked; in the `contracts` row, both are unticked. Other Effective boxes can be ticked
  (the root folder, because the Anonymous role has it; "View own media content", because the Authenticated role has
  it): that is expected. Source (in `src/OrchardCore.Modules/OrchardCore.Roles/`):
  `Views/Admin/_RolePermissions.cshtml:68-70` and `:137`, `Controllers/AdminController.cs:451-476` (the effective
  permissions of a role).

## Slide and card steps

The slides are drawn by `slides.py`; nothing is recorded for them.

- `intro` (card, chapter 00): the opening card with the title "Orchard Core Secure Media" and the subtitle "Control who
  can see each media folder".
- `why-1`: three cards headed "Without Secure Media": "Open by address", "No rules per folder", "Sensitive files are open
  too".
- `why-2`: two cards headed "With Secure Media": "View permissions" and "Not found for everyone else".
- `perm-1`: four cards headed "The view permissions", one for each of the four permissions, named as in the role editor.
- `perm-2`: two cards headed "Permissions for folders": "One permission per folder" and "Created with the folder".
- `perm-3`: three cards headed "How they relate to other permissions": "A folder permission", "Manage All Media Folders" and
  "Manage Media".
- `know-1`: two cards headed "Good to know", the first ("The Media Library") bright and the second dimmed.
- `know-2`: the same slide, the second card ("Attached editor of a Media Field") bright and the first dimmed.
- `outro` (card, chapter 99): the closing card with the subtitle "docs.orchardcore.net".

## Checks for private data

- The only account on screen is `admin` (`admin@example.com`). No real name, email address, phone number, token or
  password appears: the Roles pages show role names only, and the Features page shows feature names.
- Folder names on screen are `press` and `contracts`. No other folder, no `_Users` folder and no file name other than
  the three pictures of the Blog recipe is ever shown.
- The password is typed nowhere on camera: the sign-in is made outside the capture.
- Look at every contact sheet (`review.py`): the frame at the end of each step must match the "Visible at the end"
  line above, with no error page, no empty list and no stale message from an earlier take.
- OCR the video at one frame per second and at every scene change, as the skill asks, and search the text for
  anything that is not in this plan.

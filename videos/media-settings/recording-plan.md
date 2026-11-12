# Recording plan: Media settings

For the person who records the video. Everything to decide is decided here. In this file a selector stands for
`scene.Page.Locator(...)`, and a source reference is a path in the Orchard Core checkout at commit `92db4a83`
(`src/OrchardCore.Modules/OrchardCore.Media/...` unless the path starts elsewhere). Narrate first
(`narrate.py`), then record, because the recorder holds each step for the length of its narration.

The video has six recordings and three slide steps. No recording needs a `PrepareAsync`: the site is not seeded,
and the only files to prepare are two small files outside the repository (see "Files to upload").

## The site it needs

- **Host**: `src/OrchardCore.Cms.Web` of an Orchard Core checkout at the commit above, on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with its own `ORCHARD_APP_DATA` folder outside both repositories. Block outbound calls by setting
  `HTTP_PROXY` and `HTTPS_PROXY` to a dead address.
- **Auto Setup** (the `orchardcore-tester` skill of the Orchard Core repository describes it), with these values:
  `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...SiteName=Orchard Core`,
  `...SiteTimeZone=America/Los_Angeles`, `...AdminUsername=admin`, `...AdminEmail=admin@example.com`,
  `...AdminPassword=<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`),
  `...DatabaseProvider=Sqlite`, `...RecipeName=Blog`. This video does not show the Setup screen, so Auto Setup is
  used and `state.json` is allowed (it stays out of git).
- **Features**: the Blog recipe already enables Media, Recipes, Roles and Settings
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-79`). Do not enable anything else. In
  particular leave **Media TUS Uploads**, **Media SignalR**, **Secure Media**, **Media Slugify** and **Media Cache**
  off, and leave every OpenID Connect feature off: the Options page then shows a cross for SignalR and TUS
  uploads, as the narration of `values-4` expects, and the Media API stays on Cookie. The SignalR check on that page
  is true as soon as any feature registers SignalR in the tenant (`src/OrchardCore.Modules/OrchardCore.SignalR/Startup.cs:16`,
  the Azure and Redis variants, and `Media SignalR`, which depends on it). The Blog recipe enables none of them, so keep
  it that way.
- **Configuration**: the Options page shows the values of the site's configuration. Add no `OrchardCore:Media` section
  anywhere (no `appsettings` file, no environment variable, no user secret), so every value is the default. The
  narration does not say a number, but the screen must show the defaults, which a viewer can compare with the
  documentation.
- **Settings**: Media API authentication stays **Cookie (default)**. Nothing else is set.
- **Seed data**: none. The Blog recipe puts three theme images (`about-bg.jpg`, `home-bg.jpg`, `post-bg.jpg`) in the
  root of the media library (`blog.recipe.json:1124-1138`); they stay. The roles `Editor`, `Author`, `Contributor` and
  `Moderator` exist from the same recipe (`blog.recipe.json:88-111`).
- **Mask script**: not needed, the site is made from scratch.

### Files to upload

Create them once, in a folder outside the repository (`VIDEO_FILES`), with fake content:

```bash
mkdir -p "$VIDEO_FILES"
printf 'Agenda for the team meeting.\n' > "$VIDEO_FILES/meeting-notes.txt"
printf '<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64"><circle cx="32" cy="32" r="28" fill="#41B670"/></svg>\n' > "$VIDEO_FILES/company-logo.svg"
```

## Order, dependencies and snapshots

1. Start the site, let Auto Setup finish, run `Recorder setup` (signs in as `admin`), stop the site, and **copy the
   `ORCHARD_APP_DATA` folder and `state.json` aside**. This is the snapshot.
2. Record the clips in this order: `02-lists`, `02-role`, `03-values`, `04-api`, `04-recipes`, `02-upload`.
   None of them depends on another.
3. Only `02-upload` changes the site: it adds `company-logo.svg` to the media library. Before another take of
   `02-upload`, restore the snapshot (stop the site, restore the copy, start it again), or delete the file from the
   library first. Saving the Media API page in `04-api` stores the value it already has (Cookie) and does not reload the
   site, so it needs no restore, but restore the snapshot before a full take anyway.
4. `04-api` must never save **Bearer**. If a take ends with Bearer selected and saved, restore the snapshot: the Media
   Library cannot sign in until the scheme is Cookie again.

Common setup of every recording: viewport 1536x864, light color scheme, `en-US`, the signed-in session of
`state.json`, the admin menu as left after sign-in. The admin menu is the `nav` with id `left-nav`
(`src/OrchardCore.Themes/TheAdmin/Views/Navigation-admin.cshtml:24`). A menu entry that has children and no page
of its own is a `button`, an entry with a page is a link
(`src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml`). Probe the real markup once with
`Recorder probe /Admin "() => document.querySelector('#left-nav').outerHTML"` before the first take.

Where a step below opens a page through the menu and the menu click cannot be made reliably, the fallback is
`NavigateAsync` to the page's address, which is given with each step. Say so in the take notes.

## Slides (slides.py)

- `pages-1`: four cards, "File types", "Upload limits", "Caching" and "Storage", each with one line.
- `pages-2`: two cards, "Options" (read-only, permission View Media Options) and "API" (editable, needs the permissions
  Manage Media API authentication settings and Manage settings: the page itself is authorized with the general
  settings permission, `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:50-55`, because the
  Media module registers no permission for its settings group, `src/OrchardCore.Modules/OrchardCore.Media/Startup.cs:95`;
  the editor needs its own, `Drivers/MediaApiSettingsDisplayDriver.cs:47,54`).
- `rules-1`: three cards, "Allowed file extensions", "Restricted file extensions" and "In neither list", each with
  what happens to such a file.

## Recording `02-lists` (chapter 02)

**Starts on**: `/Admin` (the dashboard), the admin menu with every group collapsed. Open the page before the capture;
start the capture; the first step opens the Options page through the menu.

- `lists-1`: click the **Settings** button of the admin menu, then the **Media** button inside the Settings group
  (`#left-nav ul.nav-group-items button` containing the text Media; the top-level **Media** group is another entry), then the
  link **Options** (`#left-nav a[href$="/Admin/Media/Options"]`) with `ClickAndWaitForNavigation`. Wait until the
  heading **Media options** is shown. Then highlight the grey note at the top (`p.alert.alert-secondary`) with no caption.
  On screen at the end of the narration: the page **Media options**, the note with its **See documentation** link
  ringed, and the first rows below it. Fallback address: `/Admin/Media/Options`.
  Labels: `AdminMenu.cs:34-45`; `Views/Admin/Options.cshtml:13` (title), `:16-17` (note and link).
- `lists-2`: clear the highlights, highlight the row **Supported sizes** (`div.row:has(label:text-is("Supported sizes"))`).
  On screen: the row with the sizes in a line, and its hint "The accepted sizes for custom width and height."
  Labels: `Views/Admin/Options.cshtml:21-24`. The Width and Height lists the narration mentions come from
  `Controllers/MediaProfilesController.cs:354-355` and `Views/MediaProfiles/Create.cshtml:34,47` (not shown).
- `lists-3`: clear the highlights, highlight the row **Restricted file extensions**
  (`div.row:has(label:text-is("Restricted file extensions"))`). On screen: the row, its list ending in the three
  extensions, and the hint "Users need the 'Upload media file extensions requiring additional permission'
  permission to upload files with these extensions." Labels: `Views/Admin/Options.cshtml:29-32`;
  permission text `src/OrchardCore/OrchardCore.Media.Core/MediaPermissions.cs:20`.
- `lists-4`: clear the highlights, highlight the row **Allowed file extensions**
  (`div.row:has(label:text-is("Allowed file extensions"))`). On screen: the row with the beginning of the list (the
  field is one line, the list is longer than the field: do not scroll inside it) and its hint "The list of allowed
  file extensions." Labels: `Views/Admin/Options.cshtml:37-40`. The lists must not overlap and a configured list
  replaces the default: `Services/MediaOptionsValidator.cs:16-26`, `Services/MediaOptionsConfiguration.cs:95-101`.

## Recording `02-role` (chapter 02)

**Starts on**: `/Admin/Roles/Edit/Editor` (opened before the capture; wait until the field **Search permissions**,
`#search-box`, is shown).

- `role-1`: type `additional permission` into `#search-box` with `TypeAsync` (the list filters on key up), then
  highlight the one remaining row (`tr[data-text*="additional permission"]`) and its badge
  (`tr[data-text*="additional permission"] .badge`, text **Security Critical**) with no caption. On screen: the page
  **Edit 'Editor' Role**, the search box with the text, and one row, **Upload media file extensions requiring
  additional permission**, with the red **Security Critical** badge. Do not tick or save anything, and do not use the
  Administrator role: its page shows a notice instead of a permission list.
  Labels: `src/OrchardCore.Modules/OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49` (search), `:122-125`
  (badge); `src/OrchardCore.Modules/OrchardCore.Roles/Views/Admin/Edit.cshtml:6` (title); the menu path **Access
  Control**, **Roles** is `src/OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19`. The state of the checkbox is
  not narrated.

## Recording `02-upload` (chapter 02)

**Starts on**: `/Admin/Media` (Media, Library). Wait until the three theme images are listed (`about-bg.jpg` visible)
before `StartCaptureAsync`. The hidden file input is `#fileupload`; the button is the label
`label.upload-button` with the title **Upload files** (`Assets/media-gallery/src/App.vue:93-97`;
`Services/MediaJSLocalizer.cs:27`).

- `upload-1`: **timing matters**: the toast closes by itself after 5 seconds
  (`Assets/media-gallery/src/components/NotificationToast.vue:74-75`, a warning stays 5000 ms), and the narration of
  the step runs about 13 seconds and names the notification at its end. So: about 4 seconds after the step starts
  (as the voice says "We choose Upload files"), move to `label.upload-button` and click it inside
  `scene.Page.RunAndWaitForFileChooserAsync(...)`, and keep the file chooser object; about 8 seconds after the step
  starts call `fileChooser.SetFilesAsync("$VIDEO_FILES/meeting-notes.txt")`. The toast then stays on screen until the
  narration ends.
  The extension `.txt` is in neither list, so the library refuses the file before any upload: a toast with the summary
  **Validation error** and a detail line appears at the top right, and the file is not added to the list.
  On screen at the end of the narration: the toast **Validation error**, the unchanged list of three images.
  Source: `Assets/media-gallery/src/services/UppyFileUpload.ts:77-86` (the summary is the label
  **Validation error**, `Services/MediaJSLocalizer.cs:103`; the detail line is the text of the upload library and is
  not narrated). The list the page allows comes from `Controllers/AdminController.cs:57-72`.
- `upload-2`: close the toast of the previous step if it is still shown (the button `.notification-toast-close` inside
  `.notification-toast-item`), then upload `$VIDEO_FILES/company-logo.svg` the same way, with the file chosen about 4
  seconds after the step starts (as the voice says "This user holds the permission"), so that the file is listed before
  the voice says "appears in the list". `.svg` is restricted and this user holds the permission (the Administrator
  role is granted every permission: `src/OrchardCore.Modules/OrchardCore.Settings/Services/SuperUserHandler.cs:35-37`;
  the Administrator role claims are not read, `src/OrchardCore.Modules/OrchardCore.Roles/Services/RoleClaimsProvider.cs:53`), so the file is accepted: wait until
  `company-logo.svg` is listed. On screen: the file `company-logo.svg` in the list and, if shown, the **Uploads**
  toast. Source: `Services/MediaOptionsExtensions.cs:5-10` (a restricted type passes when the user has
  `UploadRestrictedMedia`), `Endpoints/Api/UploadMediaEndpoint.cs:93-94`.
  **This clip changes the site**; see the order above.

## Recording `03-values` (chapter 03)

**Starts on**: `/Admin/Media/Options`, scrolled to the top. Each step clears the highlights **before** it scrolls
(highlights are fixed-position), scrolls, then highlights the rows it names. A row is
`div.row:has(label:text-is("<label>"))`.

- `values-1`: scroll down by 300 px; highlight the rows **Max browser cache days**, **Max secure files browser cache
  days** and **Max cache days**. On screen: the three rows with their hints and their values.
  Labels: `Views/Admin/Options.cshtml:45-64`.
- `values-2`: clear, scroll down by 300 px; highlight **Max file size**, **Max upload chunk size** and **Temporary
  file lifetime**. On screen: the three rows with their hints. Labels: `Views/Admin/Options.cshtml:69-88`.
  The refusal of a larger file before the upload starts is not shown on camera: it comes from the upload library's size
  restriction (`UppyFileUpload.ts:139`, `Views/Admin/Index.cshtml:22`) and `MediaIndexViewModel.MaxFileSize`.
- `values-3`: clear, scroll down by 300 px; highlight **Cdn base url**, **Assets request path** and the one row that
  holds both **Assets path** and **Assets users folder**. On screen: those rows and their hints.
  Labels: `Views/Admin/Options.cshtml:93-117`.
- `values-4`: clear, scroll to the bottom of the page; highlight **Thumbnails disabled**, **Storage provider** and
  then the four rows **Hierarchical namespace**, **Atomic move**, **SignalR** and **TUS uploads**. On screen at the end:
  the last six rows, with a check mark for Hierarchical namespace and Atomic move, a cross for SignalR and TUS
  uploads, **Local** as the storage provider, and a cross with **No** on the row Thumbnails disabled. Labels: `Views/Admin/Options.cshtml:122-166`;
  the local store: `src/OrchardCore/OrchardCore.FileStorage.FileSystem/FileSystemStore.cs:11-13`, wrapped by
  `DefaultMediaFileStore`, which passes the name and the capabilities through (`Startup.cs:119-166`,
  `src/OrchardCore/OrchardCore.Media.Core/DefaultMediaFileStore.cs:252-254`).

## Recording `04-api` (chapter 04)

**Starts on**: `/Admin/Media/Options` with the **Settings** and **Media** groups of the admin menu already open (open
them before the capture), so that the link **API** is visible in the menu.

- `api-1`: click the link **API** (`#left-nav a[href$="/Admin/Settings/mediaApi"]`) with `ClickAndWaitForNavigation`;
  then highlight the select `select[name$="AuthenticationScheme"]` and the hint under it. On screen: the label
  **Media API authentication**, the select showing **Cookie (default)**, and the hint "How the Media Gallery, uploads
  and realtime updates authenticate to the Media API (api/media). Exactly one scheme is active at a time ..." read in
  full. Fallback address: `/Admin/Settings/mediaApi`.
  Labels: `AdminMenu.cs:41-45`; `Views/MediaApiSettings.Edit.cshtml:6-8`; `Drivers/MediaApiSettingsDisplayDriver.cs:43`.
- `api-2`: keep the highlights. Nothing else happens. On screen: the select with **Cookie (default)**.
- `api-3`: after about three seconds, `SelectAsync(select[name$="AuthenticationScheme"], "Bearer")`. **Do not save.**
  On screen: the select showing **Bearer — OAuth2 + PKCE (requires OpenID Connect)**. Source:
  `Drivers/MediaApiSettingsDisplayDriver.cs:44`. The message the narration describes is
  `Services/MediaJSLocalizer.cs:93`, shown by `Assets/media-gallery/src/App.vue:415-433` (not shown on camera).
- `api-4`: **timing matters**: a success notification hides itself after 5 seconds
  (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`,
  `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml`), and the voice names it about 5 to 7 seconds into the step.
  So first `SelectAsync(select[name$="AuthenticationScheme"], "Cookie")` and clear the highlights at the start of the
  step, then click the button **Save** (`button.save`) with `ClickAndWaitForNavigation` about 3 seconds after the step
  starts (as the voice says "select Save"). Wait until the notification "Site settings updated successfully." is shown.
  On screen when the voice says "Site settings updated successfully": that notification and the select showing
  **Cookie (default)**. At the end of the step the notification has hidden itself and the select shows **Cookie
  (default)**.
  Source: `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (Save),
  `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119` (notification). The site reloads only when the scheme changes
  (`Drivers/MediaApiSettingsDisplayDriver.cs:63-68`); with Cookie kept it does not.

## Recording `04-recipes` (chapter 04)

**Starts on**: `/Admin` with the **Tools** group of the admin menu open (open it before the capture).

- `recipes-1`: click the link **Recipes** (`#left-nav a[href$="/Admin/Recipes"]`) with `ClickAndWaitForNavigation`;
  type `Media API` into `#search-box` (the list filters on key up); highlight the entry **Media API — Bearer/PKCE**
  (its name and description). **Never click Run.** On screen at the end: the three entries **Media API —
  Bearer/PKCE**, **Media API — Standalone external app** and **Media API — Standalone external app (localhost dev)**
  with their descriptions, and the ring on the Bearer entry. Fallback address: `/Admin/Recipes`.
  The narration says the description of the recipe tells you to replace the local address; that sentence is in the
  description shown on the page ("IMPORTANT: replace the https://localhost:5001 host ..."), which is too long to read
  on camera. Labels: `src/OrchardCore.Modules/OrchardCore.Recipes/AdminMenu.cs:18-19`,
  `src/OrchardCore.Modules/OrchardCore.Recipes/Views/Admin/Index.cshtml:15`,
  `src/OrchardCore.Modules/OrchardCore.Recipes/Views/RecipeEntry.Fields.SummaryAdmin.cshtml`; the display names are in `Recipes/media-api-pkce.recipe.json:3`,
  `Recipes/media-api-standalone.recipe.json:3` and `Recipes/media-api-standalone-localhost.recipe.json:3`.

## Checks for private data

- The site is made from scratch with the user `admin` and the address `admin@example.com`; no real name, email address,
  phone number, token or password is on any screen. The password never appears in a recording: the recorder signs in
  outside the capture.
- The two uploaded files hold fake text and a green circle. The file chooser is never shown, so no local path is
  visible. The Media Library lists file names only.
- The descriptions of the recipes on `04-recipes` name `localhost` and sample addresses: that is text of the recipe,
  not private data.
- OCR the finished video at one frame per second and at every scene change, as the skill describes, and search for
  `@`, `C:\`, `\Users\` and any name that is not `admin` or `Orchard Core`.

# Recording plan: Media Cache

Read this file and nothing else to record the video. Three chapters are recordings (`02-enable`, `03-purge`,
`04-permission`); `01-how` and `05-clean` are slides. In this file a selector stands for `scene.Page.Locator(...)`.
`file:line` references are to the Orchard Core source at commit `92db4a83` (the one the catalog was read from).

The browser is headless and the capture is the page only, so no address bar is ever visible.

## The site it needs

A fresh site made from the `src/OrchardCore.Cms.Web` project of that commit, with the remote store of the story
(Azure Blob Storage) replaced by the Azurite emulator.

| | |
|---|---|
| Run | `src/OrchardCore.Cms.Web` on `http://localhost:5010`, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`), provisioned by Auto Setup through environment variables (the `orchardcore-tester` skill, `references/autosetup.md`). No setup form and no sign-in form are recorded. |
| Tenant | `Default` only. |
| Setup recipe | `Blog` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). |
| Site name, time zone | `Orchard Core`, `America/Los_Angeles`. |
| Sign-in | user `admin`, email `admin@example.com`, password `<password>` (passed in `VIDEO_PASSWORD`, chosen outside git; never written in a file of the repository). |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address (`http://127.0.0.1:9`), and `NO_PROXY=localhost,127.0.0.1`, so the site never calls a real service but still reaches Azurite on the same machine. Without `NO_PROXY` the proxy would also catch the calls to Azurite. |
| Window | The recorder's usual viewport (1536x864, scale 1.25, light color scheme, `en-US`). |

### The remote store: Azurite

The story needs a remote media store. Use the Azurite emulator in Docker, never a real Azure account:

```bash
docker run -d --name azurite-media -p 10000:10000 -v <folder outside both repositories>:/data \
  mcr.microsoft.com/azure-storage/azurite azurite-blob --blobHost 0.0.0.0 --location /data
```

If the Azure SDK of the build reports an unsupported API version, add `--skipApiVersionCheck` to the Azurite command.
Remove the container and its data folder when the recording is finished.

### Host configuration

Set these environment variables for the site process before it starts, so the host configuration is complete from the
first request (the names follow the comment on line 11 of `src/OrchardCore.Cms.Web/appsettings.json`):

| Variable | Value |
|---|---|
| `OrchardCore__Media__Azure__ConnectionString` | `UseDevelopmentStorage=true` (Azurite's built-in shortcut: it holds no key) |
| `OrchardCore__Media__Azure__ContainerName` | `media` |
| `OrchardCore__Media__Azure__CreateContainer` | `true` |
| `OrchardCore__Media__Azure__UseHierarchicalNamespace` | `false` (Azurite is a flat namespace; this skips the account probing) |

Do not set `OrchardCore__Media__RemoteCacheMaxStale` or `OrchardCore__Media__ResizedCacheMaxStale`: the video says the
cleaners do nothing without them.

With these variables set but the feature **Azure Media Storage** not enabled, the site still uses its local media
store and the Media Cache page reports "not configured". That is the state `02-enable` needs.

### The two cache folders are not in `App_Data`

Both caches are folders of the web root of the web project, one per tenant:
`src/OrchardCore.Cms.Web/wwwroot/Default/ms-cache` holds the local copies of the remote files (the cache this video
purges), and `src/OrchardCore.Cms.Web/wwwroot/Default/media-cache` holds resized pictures
(`DefaultMediaFileStoreCacheFileProvider.cs`, `AssetsCachePath`; `PhysicalFileSystemResizedImageCache.cs:12,24`; both
under `GetWebRootPathOrDefault()`). A copy of `App_Data` does not contain them, so:

- when you restore S0, **delete both folders** (they must not exist at the start of `02-enable`);
- S1 includes them: copy `ms-cache` and `media-cache` aside with `App_Data` and the emulator's data, and put them back
  when you restore S1, so `03-purge` starts with a full cache (the three uploaded pictures are in `ms-cache`);
- when the recording is finished, delete both folders.

### Features at the start (state S0)

The Blog recipe enables `OrchardCore.Features` (Tools, Features), `OrchardCore.Roles` (Access Control, Roles) and
`OrchardCore.Media`, and creates the roles Moderator, Editor, Author and Contributor (`blog.recipe.json`, lines 31 to
111). Nothing else is needed. Keep these **disabled** until the clips enable them:

- Media Cache (`OrchardCore.Media.Cache`), enabled by the clip `02-enable`;
- Azure Media Storage (`OrchardCore.Media.Azure.Storage`), enabled by hand between `02-enable` and `03-purge`.

Take the snapshot **S0** here: stop the site, copy `App_Data` aside (and the empty Azurite data folder), start it again.

### Seed 2 (between `02-enable` and `03-purge`, outside the capture)

1. Sign in as `admin`. In Tools, Features, enable **Azure Media Storage** (its button is `a.btn` with text `Enable` in the
   row named Azure Media Storage, category Hosting). Media Cache is already enabled. Wait for the notification "The
   feature Azure Media Storage was enabled." and check the log: it must not say "Azure Media Storage is enabled but
   not active".
2. The Blog recipe imported three pictures into the local store at setup, and the new remote store starts empty, so the
   front end would lose its header pictures. Upload the same three files into the root folder of the Media Library
   (Media, Library): `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg`, taken from
   `src/OrchardCore.Themes/TheBlogTheme/wwwroot/assets/img/` of the Orchard Core checkout. They are the theme's own
   stock pictures: no people, nothing to mask. Use the file chooser of the library (set the files on its
   `input[type=file]`).
3. Check before going on: the Library lists the three files; `http://localhost:5010/media/home-bg.jpg` answers `200`
   with the picture; the container `media` of Azurite holds the three blobs; the home page of the site shows its
   header picture.
4. Take the snapshot **S1**: stop the site, copy `App_Data`, **the Azurite data folder and the two cache folders**
   aside, start both again. Look inside `ms-cache` first: the three uploaded pictures should be there, because an upload
   copies the file into the cache right away.

## Order and dependencies

| Order | Clip | Starts from | Changes the site? | Before a new take |
|---|---|---|---|---|
| 1 | `02-enable` | S0 | Yes: enables Media Cache | Restore S0, and delete both cache folders |
| (Seed 2, then snapshot S1) | | | | |
| 2 | `03-purge` | S1 | Yes: empties `ms-cache`, then one picture fills it again | Restore S1 with its cache folders (any state works, but restore for a repeatable take) |
| 3 | `04-permission` | S1 | Yes: grants a permission to the role Editor | Restore S1 |

`03-purge` and `04-permission` do not depend on each other; both need S1. The slide chapters `01-how` and `05-clean`
need no site. To record everything again: restore S0 (App_Data and an empty Azurite folder, no cache folders), and start with
`02-enable`.

## Chapter 01, `01-how` (slides)

No recording. `slides.py` draws one slide per step, with the heading and three or four cards each:

- `why-1`: "A remote store and a local copy": the three steps of the idea (files in a remote store, the site serves them, the first request makes a copy).
- `why-2`: "How the copy is used and kept": later requests, image resizing, uploads, deletes and moves.
- `why-3`: "When to purge the cache": free disk space, fetch fresh copies, in front of a content delivery network.

## Chapter 02, `02-enable` (recording)

Starting page: `/Admin` (the dashboard), signed in as `admin`. Before the capture, expand the **Tools** group of the
sidebar (`#left-nav`) so the item **Features** is visible. The step order below is the narration order.

- `enable-1` (about 12 s of narration).
  Actions: click the **Features** item of the Tools group and wait for the navigation; click the search box
  (`#search-box`, placeholder "Search") and type `Media Cache` with a delay of about 60 ms per character; wait until the
  row of Media Cache is visible (`#btn-enable-OrchardCore_Media_Cache` is visible; if the id differs, use the
  `a.btn` with text `Enable` inside the row whose name is Media Cache). Highlight the description of that row.
  Visible when the narration ends: the Features page filtered on Media Cache; under the category heading **Content
  Management**, the row **Media Cache** with its description "The media cache module adds remote file store cache
  support.", the status **Disabled** and the button **Enable**.
  Labels: `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features);
  `src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs:44-52` (name, description, category).
- `enable-2` (about 14 s).
  Actions: move the cursor to the **Enable** button of the Media Cache row, wait about 1 s, then click it with
  `ClickAndWaitForNavigationAsync`. Wait until the text "The feature Media Cache was enabled." is visible.
  Visible when the narration ends: the Features page after the reload, with the green notification "The feature Media
  Cache was enabled.". (The search box is empty again; do not type again.)
  Labels: `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml` (button
  **Enable**); `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317` (notification);
  `src/OrchardCore.Modules/OrchardCore.Media.Azure/Manifest.cs:12` and
  `src/OrchardCore.Modules/OrchardCore.Media.AmazonS3/Manifest.cs:12` (the two storage features the narration names,
  "Azure Media Storage" and "Amazon Media Storage").
- `enable-3` (about 13 s).
  Actions: in the sidebar click the group **Settings**, then **Media** inside it (not the top-level **Media**, which
  holds Library and Profiles), then **Cache** (`#left-nav a[href$="/Admin/MediaCache"]`), with
  `ClickAndWaitForNavigationAsync` for the last click. Wait until the text "Asset Cache" is visible in the title area.
  Highlight the red message.
  Visible when the narration ends: the page titled **Asset Cache**, with the red message "The asset cache feature is
  enabled, but a remote media store feature is not enabled, or not configured with appsettings.json." and no
  **Purge All** button. (This message is the product's text: the narration paraphrases it. See `OPEN-QUESTIONS.md`.)
  Labels: `src/OrchardCore.Modules/OrchardCore.Media/MediaCacheAdminMenu.cs:18-20` (Settings, Media, Cache);
  `src/OrchardCore.Modules/OrchardCore.Media/Views/MediaCache/Index.cshtml:3` (title) and `:28` (message).

After this clip, do **Seed 2**.

## Chapter 03, `03-purge` (recording)

Starting page: `/Admin/MediaCache`, opened before the capture, from the state S1. Before the capture check that the page
shows the grey notice and the **Purge All** row (if it shows the red message, Azure Media Storage is not active: look
at the log).

- `purge-1` (about 13 s).
  Actions: highlight the grey notice (`.alert.alert-secondary`), wait about 3 s, clear it, then highlight the
  **Purge All** row (`ul.list-group li.list-group-item`).
  Visible when the narration ends: the page titled **Asset Cache** with the grey notice "Media is configured with
  appsettings.json.", and the single row **Purge All** with the hint "Purge all the assets from the cache" and a red
  **Purge All** button on its right.
  Labels: `src/OrchardCore.Modules/OrchardCore.Media/Views/MediaCache/Index.cshtml:8` (notice), `:17-20` (row, hint,
  button).
- `purge-2` (about 8 s, the shortest step: keep the actions quick).
  Actions: click the red button (`a[data-title="Purge All"]`; the words "Purge All" appear twice on the page, so do not
  select by text); wait until `#confirmRemoveModal` is visible; hold about 1.5 s so the dialog can be read; click the
  button **Ok** (`#modalOkButton`) inside `WaitForNavigationAsync` (the click posts the form and the page reloads).
  Visible when the narration ends: the page reloaded after the purge (the dialog is gone). The dialog, seen during the
  step, is titled **Purge All**, asks "Are you sure you want to purge the entire asset cache?" and has the buttons
  **Ok** and **Cancel**.
  Labels: `Index.cshtml:17` (title and message); `Ok` and `Cancel` come from the layout of the admin theme,
  `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`, because the Purge All button sets no button text of its own.
- `purge-3` (about 13 s).
  Actions: wait until the text "Asset cache purged." is visible (an information notification); highlight it.
  Visible when the narration ends: the Cache page with the notification "Asset cache purged." above the same grey
  notice and **Purge All** row. The "with errors" message of the narration is **not** recorded (it needs a file that
  is in use and cannot be deleted).
  Labels: `src/OrchardCore.Modules/OrchardCore.Media/Controllers/MediaCacheController.cs:72` ("Asset cache purged."),
  `:68` ("Asset cache purged, with errors.").
- `purge-4` (about 11 s).
  Actions: `NavigateAsync("/media/home-bg.jpg")`; wait until the picture is loaded
  (`document.images.length == 1 && document.images[0].complete`).
  Visible when the narration ends: the browser showing the picture `home-bg.jpg` alone on the page. The picture is the
  Blog theme's header picture; nothing else is on screen.
  The browser must not have requested `/media/home-bg.jpg` before this step: media files are sent with a browser cache
  time of days, and a browser that holds the picture shows it without asking the site. Every clip uses a fresh browser
  context, so this holds as long as the seed checks above are made with another tool (for example `curl`). Confirm the
  take by watching `ms-cache/home-bg.jpg` disappear at the purge and reappear after this request.
  Why this address and not the Media Library: the library's thumbnails are resized pictures, which have their own cache
  and are not read from the cache that was just purged, so only the original address proves the file is downloaded
  again (`src/OrchardCore.Modules/OrchardCore.Media/Services/MediaFileStoreResolverMiddleware.cs:65-107`).

## Chapter 04, `04-permission` (recording)

Starting page: `/Admin`, from the state S1. Before the capture, expand the **Access Control** group of the sidebar so the
item **Roles** is visible.

- `perm-1` (about 10 s).
  Actions: click **Roles** and wait for the navigation; move the cursor to the row of the role **Editor**, highlight it,
  then click its **Edit** button (`a.edit` inside that row; the rows are `li.list-group-item` in the List layout and
  `tr` in the Grid layout; if the actions are collected in a menu, open it first) with `ClickAndWaitForNavigationAsync`.
  Visible when the narration ends: the page titled "Edit 'Editor' Role", with the Description box, the hint boxes
  **Allow** and **Effective**, and the box "Search permissions".
  Labels: `src/OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19` (Access Control, Roles);
  `src/OrchardCore.Modules/OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml` (**Edit**);
  `src/OrchardCore.Modules/OrchardCore.Roles/Views/Admin/Edit.cshtml:6` (page title);
  `src/OrchardCore.Modules/OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:40,43,49`.
- `perm-2` (about 11 s).
  Actions: click the search box (`#search-box`, placeholder "Search permissions") and type `cache` with a delay of about
  60 ms; wait until the row **Manage Asset Cache Folder** is visible under the group **Media Cache** (with the hint
  "(OrchardCore.Media.Cache)");
  highlight the row **Manage Asset Cache Folder**; click its **Allow** checkbox (`[id="Checkbox.ManageAssetCache"]`);
  then click **Save** (`button.save`) with `WaitForNavigationAsync`.
  Visible when the narration ends: the Roles list after saving, with the notification "Role updated successfully.".
  The checked box and the row are visible during the step, before the save.
  Labels: `_RolePermissions.cshtml:49` (search box), `:69` (**Allow** column), `:131` (checkbox id);
  `src/OrchardCore/OrchardCore.Media.Core/MediaPermissions.cs:29` ("Manage Asset Cache Folder");
  `src/OrchardCore.Modules/OrchardCore.Media/MediaCachePermissions.cs` (the permission is registered by the Media Cache
  feature only, which is why the group is named Media Cache);
  `src/OrchardCore.Modules/OrchardCore.Roles/Views/Admin/Edit.cshtml` (**Save**);
  `src/OrchardCore.Modules/OrchardCore.Roles/Controllers/AdminController.cs:213` ("Role updated successfully.").

## Chapter 05, `05-clean` (slides)

No recording. `slides.py` draws one slide per step, with two cards each:

- `clean-1`: "Two caches": the cards **Remote file cache** and **Resized image cache**.
- `clean-2`: "Two cleaners, listed in Tools, Background Tasks": the cards **Remote Media Cache Cleaner** and
  **Resized Media Cache Cleaner**.
- `clean-3`: "Schedule and time limit": the cards **Once a day by default** and **A time limit sets what is removed**.

The task names and schedules come from
`src/OrchardCore.Modules/OrchardCore.Media/Services/RemoteMediaCacheBackgroundTask.cs:12` and
`ResizedMediaCacheBackgroundTask.cs:8`.

## Checks for private data

- The only person on screen is the user `admin` (`admin@example.com`). The sidebar and the pages recorded show no other
  name, email, phone number or token.
- Do not open Settings, Media, Azure Blob Options at any time during the capture (the connection string of the
  emulator is configuration, not content, but the page is out of the story).
- The three pictures are the Blog theme's stock images; check that no other file is in the Media Library.
- After assembling, OCR the video at one frame per second and at every scene change, and look for anything other than
  `admin`, `example.com` addresses and the labels of this plan.
- No password, key or connection string is written in this folder or visible in any frame.

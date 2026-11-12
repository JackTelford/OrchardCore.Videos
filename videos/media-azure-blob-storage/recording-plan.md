# Recording plan: Azure Media Storage

How to record the four "clip" recordings of this video without deciding anything. In the Orchard Core source below,
`<oc>` is the checkout of `OrchardCMS/OrchardCore` that the site runs from, and a selector such as
`#search-box` stands for `scene.Page.Locator("#search-box")`. Every label comes from the source file named on its line.

The audience is developers: slides show configuration and code, and a recording is used only for the screens that are
part of the story (the Features page, the options pages, the Media Library, a page of the site).

## 1. The site it needs

- **Site**: `src/OrchardCore.Cms.Web` of `<oc>`, on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own
  `ORCHARD_APP_DATA` folder outside both repositories. The site is not uninstalled: it is provisioned by Auto Setup, so
  no setup or sign-in form is recorded.
- **Auto Setup** (environment variables of the site process, see `docs/reference/modules/AutoSetup/README.md`):

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Recipe**: Blog. It enables `OrchardCore.Media`, and its home page (the `Blog` content item, route `/`) shows a header
  banner built from the media file `home-bg.jpg` with the `banner` media profile
  (`<oc>/src/OrchardCore.Themes/TheBlogTheme/Views/Content-Blog.liquid:4-9`,
  `<oc>/src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:1111-1121`). The recipe copies three pictures into the
  local media folder (`blog.recipe.json:1124-1136`). Enabling Azure Media Storage does not move them, so until
  `home-bg.jpg` is uploaded into the container the header of the home page has no picture. The clips never show the
  home page before `cache-4`.
- **External service and its local stand-in**: Azure Blob Storage, replaced by the Azurite emulator in Docker. Start it
  before the site, with its data in a folder outside git so a snapshot can be taken:

  ```bash
  docker run --name azurite-video -d -p 10000:10000 -v <scratch>/azurite-data:/data \
    <azurite-image> \
    azurite-blob --location /data --blobHost 0.0.0.0 --skipApiVersionCheck
  ```

  `<azurite-image>` is the image that the Azurite documentation names for running the emulator in Docker, with its
  default tag (the slide of `conf-4` shows only a placeholder for the same reason). `--skipApiVersionCheck` is only
  needed when the site's log says that Azurite does not support the API version of the storage client. When the
  recordings are done: `docker rm -f azurite-video`, and delete the data folder.
- **Configuration of the site** (environment variables of the site process, set before it starts; both features stay
  disabled until a clip enables them, and the settings are read only then):

  | Variable | Value |
  |---|---|
  | `OrchardCore__Media__Azure__ConnectionString` | `UseDevelopmentStorage=true` |
  | `OrchardCore__Media__Azure__ContainerName` | `oc-media` |
  | `OrchardCore__Media__Azure__ImageCache__ConnectionString` | `UseDevelopmentStorage=true` |
  | `OrchardCore__Media__Azure__ImageCache__ContainerName` | `oc-imagecache` |
  | `NO_PROXY` | `127.0.0.1,localhost` |

  The emulator account is the published development account of Azure Storage: it holds no real data and needs no
  secret in git. The `HTTP_PROXY` and `HTTPS_PROXY` variables of `references/recording.md` still point at a dead
  address, and `NO_PROXY` keeps the calls to the emulator working. No `BasePath` is set.
- **Features to enable before recording**: none. Azure Media Storage and Azure Media Image Cache are enabled on camera.
- **Settings**: none to save.
- **Seed data to create outside the capture**: one picture, `home-bg.jpg`, copied without renaming from
  `<oc>/src/OrchardCore.Themes/TheBlogTheme/wwwroot/assets/img/home-bg.jpg` into a folder outside git, for example
  `<scratch>/upload/home-bg.jpg`. It is a picture shipped with the Blog theme and shows no person. Nothing else is seeded.
- **Sign in** once outside the capture (`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`), which saves
  `state.json` (git-ignored). There is no `PrepareAsync`.
- **Local cache folders**: the site keeps two folders under its web root (`<web root>/Default/ms-cache`, the local copies
  of the files read from the container, and `<web root>/Default/media-cache`, the default cache of resized images). They
  are not in `App_Data`. Delete both whenever a snapshot is restored
  (`OrchardCore.Media.Azure/Startup.cs:65`, `OrchardCore.Media/Services/PhysicalFileSystemResizedImageCache.cs:12`,
  `OrchardCore.Media.Core/DefaultMediaFileStoreCacheFileProvider.cs:16`).

## 2. Order of the clips, and what each one changes

Record in this order. Each clip starts from the state the previous one left.

| Order | Clip | Changes the site? | Depends on |
|---|---|---|---|
| 1 | `03-enable` | Yes: enables Azure Media Storage and Media Cache; the tenant starts again and creates the container `oc-media` | the starting state |
| 2 | `03-check` | No | `03-enable` |
| 3 | `04-serve` | Yes: writes `home-bg.jpg` to the container and a local copy to `ms-cache` | `03-enable` |
| 4 | `06-cache` | Yes: enables Azure Media Image Cache; the home page writes one resized image to the container `oc-imagecache` | `04-serve` (the home page needs `home-bg.jpg`) |

**Snapshots.** Stop the site (by its process ID) and Azurite (`docker stop azurite-video`), then copy the site's
`ORCHARD_APP_DATA` folder and `<scratch>/azurite-data` into `snapshots/S<n>/`:

- `S0`: after Auto Setup and the sign-in, before `03-enable`.
- `S1`: after `03-enable` (this is also the state for `03-check`).
- `S2`: after `04-serve`.

To record a clip again, stop the site and Azurite, delete the web root folders `ms-cache` and `media-cache` of the
`Default` tenant, restore the snapshot that comes before the clip (`S0` for `03-enable`, `S1` for `03-check` and
`04-serve`, `S2` for `06-cache`), start Azurite, then start the site.

## 3. The clips

Cursor, click ripples and timing follow `references/recording.md`. Every step below keeps its actions shorter than its
narration, and ends with the screen described under "End of the step".

### Clip `03-enable` (chapter 03)

Starts on `/Admin` (the dashboard), with the page loaded before `StartCaptureAsync`.

- `enable-1`
  - Actions: click the menu item **Tools**, then **Features** (`ClickAndWaitForNavigationAsync`); the page is
    `/Admin/Features`. Type `Azure Media` into `#search-box` (`TypeAsync`). Wait until the row
    `[data-filter-value^='Azure Media Storage OrchardCore.Media.Azure.Storage ']` is visible and the row
    `[data-filter-value^='Media Cache ']` is hidden. Highlight the category heading `.feature-group h3` whose text is
    **Hosting**.
  - End of the step: the search box holds `Azure Media`; the **Hosting** heading is on top of the rows **Azure Media
    Storage**, **Azure Media Image Cache**, and **Azure Media Image Cache (Obsolete)**, all with the status **Disabled**.
  - Labels: **Tools**, **Features** `OrchardCore.Features/AdminMenu.cs:26-27`; search box `Views/Admin/Features.cshtml:27`;
    category **Hosting** `OrchardCore.Media.Azure/Manifest.cs:18,29,40`; names `Manifest.cs:12,23,34`; the filter matches
    name, id and description `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`.
- `enable-2`
  - Actions: highlight the dependency badge **Media Cache** of the Azure Media Storage row, the element
    `.direct-dependencies .badge` inside the row `[data-filter-value^='Azure Media Storage OrchardCore.Media.Azure.Storage ']`
    (both layouts of the page show it, as a tag of the row or in the **Dependencies** column). Click **Enable** inside the row
    `[data-filter-value^='Azure Media Storage OrchardCore.Media.Azure.Storage ']`
    (`ClickAndWaitForNavigationAsync`; the button is the link `a.btn` with the text **Enable**). The tenant starts
    again, so wait for the Features page to be shown again; if that takes longer than the narration, wrap the wait in
    `OffCameraAsync`. Type `Azure Media` into `#search-box` again.
  - End of the step: the **Azure Media Storage** row shows the status **Enabled** and a **Disable** button; no error message.
  - Labels: **Enable**, **Enabled** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`,
    `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml`; dependency `OrchardCore.Media.Azure/Manifest.cs:14-17`, its badge
    `OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12-18` (placed in both layouts by
    `Drivers/FeatureEntryDisplayDriver.cs:37-39`).

### Clip `03-check` (chapter 03)

Starts on `/Admin`, loaded before `StartCaptureAsync`. If the menu cannot be driven reliably, finish each navigation with
`NavigateAsync` of the path given below; the frame at the end of the step must be the same.

- `check-1`
  - Actions: click the menu items **Settings**, **Media**, **Azure Blob Options** (`ClickAndWaitForNavigationAsync` on
    the last); the page is `/Admin/MediaAzureBlob/Options`. Highlight the **Connection String** row, then the **Container
    Name** and **Base Path** rows, then the three check boxes.
  - End of the step: the page titled **Azure Blob storage options**; **Connection String** shows `Configured` and no
    value; **Container Name** shows `oc-media`; **Base Path** is empty; **Create Container** is checked, **Remove
    Container** and **Remove Files From Base Path** are not, and all three are disabled.
  - Labels: **Settings**, **Media**, **Azure Blob Options** `OrchardCore.Media.Azure/AdminMenu.cs:18-20`; the page
    `Views/Admin/Options.cshtml:2` (title), `:10` **Connection String**, `:15` **Configured**, `:26` **Container Name**,
    `:33` **Base Path**, `:43` **Create Container**, `:53` **Remove Container**, `:63` **Remove Files From Base Path**; route
    `Controllers/AdminController.cs:11`.
- `check-2`
  - Actions: click the menu items **Settings**, **Media**, **Options** (`ClickAndWaitForNavigationAsync` on the last); the
    page is `/Admin/Media/Options`. `ClearHighlightsAsync`, scroll down until the rows **Storage provider**,
    **Hierarchical namespace** and **Atomic move** are visible, then highlight each in turn.
  - End of the step: **Storage provider** shows `Azure Blob (Gen1)`; **Hierarchical namespace** and **Atomic move** each show
    a red cross.
  - Labels: **Options** `OrchardCore.Media/AdminMenu.cs:36-40`; rows `Views/Admin/Options.cshtml:131,139,147`; the value
    `Azure Blob (Gen1)` is `StorageName` of `OrchardCore.FileStorage.AzureBlob/BlobFileStore.cs:63-65`.

### Clip `04-serve` (chapter 04)

Starts on `/Admin`, loaded before `StartCaptureAsync`.

- `serve-1`
  - Actions: click the menu items **Media**, then **Library** (`ClickAndWaitForNavigationAsync`); the page is `/Admin/Media`.
    Wait until the text `This folder is empty` is shown, then highlight it.
  - End of the step: the Media Library shows the root folder with the message `This folder is empty`. The folder tree
    lists a folder named `_Users`: loading the library creates the folder of the signed-in user in the container
    (`OrchardCore.Media/Endpoints/Api/GetDirectoryTreeEndpoint.cs:46-51`, a marker blob `_Users/<user id>/OrchardCore.Media.txt`
    on a flat account), and the file pane, with that message, is drawn only when the library has at least one folder
    (`OrchardCore.Media/Assets/media-gallery/src/App.vue:136`). Do not expand `_Users`: the folder below it is named with
    the user's id.
  - Labels: **Media**, **Library** `OrchardCore.Media/AdminMenu.cs:18-25`; `This folder is empty`
    `OrchardCore.Media/Services/MediaJSLocalizer.cs:32`, shown by `App.vue:186-193`; `_Users` is `DefaultAssetsUsersFolder`
    `OrchardCore.Media/Services/MediaOptionsConfiguration.cs:65`.
- `serve-2`
  - Actions: move to the **Upload files** button (`label[for='fileupload']`) and click it while a file chooser is
    intercepted: `var chooser = await scene.Page.RunAndWaitForFileChooserAsync(() => scene.ClickAsync(label));` then
    `await chooser.SetFilesAsync("<scratch>/upload/home-bg.jpg");`. If the chooser cannot be intercepted, call
    `scene.Page.Locator("#fileupload").SetInputFilesAsync(...)` after `MoveToAsync(label)`. Wait until the row
    `home-bg.jpg` is listed and the upload progress has gone.
  - End of the step: the root folder lists `home-bg.jpg`; no error in the upload area.
  - Labels: **Upload files** `OrchardCore.Media/Services/MediaJSLocalizer.cs:27`, input `Assets/media-gallery/src/App.vue:93-96`.
- `serve-3`
  - Actions: `NavigateAsync("/media/home-bg.jpg")`. Wait until the picture is shown, then
    `HighlightAsync(scene.Page.Locator("img"), "/media/home-bg.jpg")`.
  - End of the step: the picture alone, with the caption `/media/home-bg.jpg` (the recording shows no address bar, so the
    caption names the path).
  - Labels: the path is `AssetsRequestPath`, default `/media` (`OrchardCore.Media/Services/MediaOptionsConfiguration.cs:66,110`).

### Clip `06-cache` (chapter 06)

Starts on `/Admin/Features`, loaded before `StartCaptureAsync` (the menu path was shown in `enable-1`).

- `cache-3`
  - Actions: type `Azure Media Image Cache` into `#search-box`. Wait until the row
    `[data-filter-value^='Azure Media Image Cache OrchardCore.Media.Azure.ImageCache ']` is visible (not the **(Obsolete)**
    row). Click **Enable** inside that row (`ClickAndWaitForNavigationAsync`; it changes the tenant, so allow a few
    seconds, off camera if needed). Type `Azure Media Image Cache` again.
  - End of the step: the row **Azure Media Image Cache** shows the status **Enabled**; the **(Obsolete)** row is still **Disabled**.
  - Labels: `OrchardCore.Media.Azure/Manifest.cs:21-30`; **Enable** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.
- `cache-4`
  - Actions: `NavigateAsync("/")`. Wait until `header.masthead` has a `background-image` that contains `/media/home-bg.jpg`
    and the picture has loaded (`WaitForFunctionAsync` that loads the URL in an `Image` and resolves on `onload`). Highlight
    `header.masthead` with the caption `banner profile`.
  - End of the step: the home page of the blog with its header banner picture visible behind the title.
  - Labels: banner `<oc>/src/OrchardCore.Themes/TheBlogTheme/Views/Content-Blog.liquid:9`; the `banner` profile
    `blog.recipe.json:1113`.

### Checks off camera (after `04-serve` and after `06-cache`)

Not recorded, but the narration depends on them. With the Azure CLI (any storage browser works as well):

```bash
az storage blob list --connection-string "UseDevelopmentStorage=true" --container-name oc-media --output table
az storage blob list --connection-string "UseDevelopmentStorage=true" --container-name oc-imagecache --output table
```

After `04-serve`, `oc-media` lists `home-bg.jpg` with the content type `image/jpeg`, and the marker blob
`_Users/<user id>/OrchardCore.Media.txt` that opening the library created. After `cache-4`, `oc-imagecache` lists
one blob, named `<two characters>/<key>.<extension>`. If one of them does not match, write it in `OPEN-QUESTIONS.md`.

## 4. The slides

One line each; they are drawn by `slides.py`.

- `why-1`: three cards: the local disk is not permanent, instances share media, Blob Storage keeps the files.
- `why-2`: three cards: the two features and the Obsolete identifier, each with its feature id.
- `why-3`: four cards, numbered: request, local copy, container, response.
- `conf-1`: the `OrchardCore:Media:Azure` configuration; the path of the section and the lines of `ConnectionString` and `ContainerName` are bright; cards "Required".
- `conf-2`: the same configuration; the lines of `BasePath` and `CreateContainer` are bright; cards "Optional".
- `conf-3`: two environment variable exports, and two cards on the order of the configuration sources.
- `conf-4`: the Docker command that starts Azurite (the image is a placeholder), the export of the development storage connection string, and two cards.
- `svc-1`: the registration code of the Azure Media Storage startup class, trimmed.
- `svc-2`: the startup class that replaces the temporary store of resumable uploads, and a card.
- `cache-1`: the `ImageCache` configuration, and two cards.
- `cache-2`: the `IResizedImageCache` interface, trimmed, and the line that registers the Azure implementation.
- `tenant-1`: two configurations side by side, a container per tenant and a folder per tenant, and a card on the Liquid filters.
- `tenant-2`: the two removal settings with their comments, and two cards.
- `tenant-3`: three cards: Gen1 flat namespace, Gen2 hierarchical namespace, detection at startup.
- `tenant-4`: the `UseHierarchicalNamespace` setting, and two cards.

## 5. Checks for private data

- The only account in the clips is `admin` with `admin@example.com`; the password is never typed on camera (`state.json`).
- The connection string is `UseDevelopmentStorage=true`; no storage account name or key of a real account is typed,
  shown or logged. The Options page shows `Configured`, never the value.
- The only file uploaded is `home-bg.jpg` of the Blog theme (it shows a desk with a notebook, a laptop and a phone, and
  no person). In the Media Library keep the folder `_Users` collapsed: the folder below it is named with the user's id.
- Look at every contact sheet for the Blog recipe's sample content on the home page, and for any name other than the
  invented ones. OCR the finished video at one frame per second and at every scene change, and search for real names,
  email addresses other than `@example.com`, and tokens.
- Remove the Azurite container and its data folder when the recordings are done.

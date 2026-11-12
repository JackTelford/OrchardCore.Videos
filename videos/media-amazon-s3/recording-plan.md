# Recording plan: Amazon S3 Media Storage

How to record the four "clip" recordings of this video without deciding anything. In the Orchard Core source below,
`<oc>` is the checkout of `OrchardCMS/OrchardCore` that the site runs from (commit `92db4a83` was read for this plan), and
a selector such as `#search-box` stands for `scene.Page.Locator("#search-box")`. Every label comes from the source file
named on its line.

The audience is developers: slides show configuration and code, and a recording is used only for the screens that are
part of the story (the Features page, the options pages, the Media Library, a page of the site). The terminal commands
of the slide `conf-5` are a slide, not a recording.

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

- **Recipe**: Blog. It enables `OrchardCore.Media`, and its home page (the `Blog` content item, set as the home route in
  the `settings` step) shows a header banner built from the media file `home-bg.jpg` with the `banner` media profile
  (`<oc>/src/OrchardCore.Themes/TheBlogTheme/Views/Content-Blog.liquid:4-9`,
  `<oc>/src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:114-119, 982-990, 1111-1121`). The recipe copies three
  pictures into the local media folder (`blog.recipe.json:1124-1136`). Enabling Amazon Media Storage does not move them
  (nothing in the module does, see `OPEN-QUESTIONS.md`), so until `home-bg.jpg` is uploaded into the bucket the header of
  the home page has no picture. The clips never show the home page before `cache-4`.
- **External service and its local stand-in**: Amazon S3, replaced by the **S3Mock** emulator in Docker. The
  documentation names it as known to work with these settings (`<oc>/src/docs/reference/modules/Media.AmazonS3/README.md`,
  "Configuring a Local Emulator"), and it accepts any credentials, so no password or key of any kind goes into git. Start it
  before the site:

  ```bash
  docker run --name s3mock-video -d -p 9444:9090 adobe/s3mock:latest
  ```

  This is the documentation's command with `--name` and `-d` added. The emulator keeps its data inside the container. When
  the recordings are done: `docker rm -f s3mock-video`.
- **Configuration of the site** (environment variables of the site process, set before it starts; both features stay
  disabled until a clip enables them, and the settings are read only then):

  | Variable | Value |
  |---|---|
  | `OrchardCore__Media__AmazonS3__ServiceURL` | `http://localhost:9444/` |
  | `OrchardCore__Media__AmazonS3__ForcePathStyle` | `true` |
  | `OrchardCore__Media__AmazonS3__Credentials__AccessKey` | `dummy` |
  | `OrchardCore__Media__AmazonS3__Credentials__SecretKey` | `dummy` |
  | `OrchardCore__Media__AmazonS3__BucketName` | `oc-media` |
  | `OrchardCore__Media__AmazonS3__BasePath` | `media` |
  | `OrchardCore__Media__AmazonS3__CreateBucket` | `true` |
  | `OrchardCore__Media__AmazonS3__ImageCache__BucketName` | `oc-imagecache` |
  | `OrchardCore__Media__AmazonS3__ImageCache__CreateBucket` | `true` |
  | `NO_PROXY` | `127.0.0.1,localhost` |

  No `Region` is set: the module accepts a `ServiceURL` in its place
  (`<oc>/src/OrchardCore.Modules/OrchardCore.Media.AmazonS3/AwsStorageOptionsExtension.cs:12-26`). The `dummy` keys are the
  documentation's own placeholder values for emulators. The `HTTP_PROXY` and `HTTPS_PROXY` variables of
  `references/recording.md` still point at a dead address, and `NO_PROXY` keeps the calls to the emulator working.
  `BasePath` is `media` so that the page of the clip `check-1` shows a value in that row; the module trims the slashes of
  the path, so `media` and `/media` are the same (`<oc>/src/OrchardCore/OrchardCore.FileStorage.AmazonS3/AwsFileStorage.cs:311-319`).
  The image cache has a `BasePath` setting of its own, which the plan leaves unset, so its objects are named `<two characters>/<key>.<extension>`
  (`<oc>/src/OrchardCore.Modules/OrchardCore.Media.AmazonS3/Services/AWSS3ResizedImageCache.cs:176-181`).
- **Features to enable before recording**: none. Amazon Media Storage and Amazon Media Image Cache are enabled on camera.
- **Settings**: none to save.
- **Seed data to create outside the capture**: one picture, `home-bg.jpg`, copied without renaming from
  `<oc>/src/OrchardCore.Themes/TheBlogTheme/wwwroot/assets/img/home-bg.jpg` into a folder outside git, for example
  `<scratch>/upload/home-bg.jpg`. It is a picture shipped with the Blog theme and shows no person. Nothing else is seeded.
- **Sign in** once outside the capture (`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`), which saves
  `state.json` (git-ignored). There is no `PrepareAsync`.
- **Local cache folders**: the site keeps two folders under its web root (`<web root>/Default/ms-cache`, the local copies
  of the files read from the bucket, and `<web root>/Default/media-cache`, the default cache of resized images). They are
  not in `App_Data`. Delete both whenever a snapshot is restored
  (`<oc>/src/OrchardCore.Modules/OrchardCore.Media.AmazonS3/Startup.cs:77-78`,
  `<oc>/src/OrchardCore/OrchardCore.Media.Core/DefaultMediaFileStoreCacheFileProvider.cs:16`,
  `<oc>/src/OrchardCore.Modules/OrchardCore.Media/Services/PhysicalFileSystemResizedImageCache.cs:12`).

## 2. Order of the clips, and what each one changes

Record in this order. Each clip starts from the state the previous one left.

| Order | Clip | Changes the site? | Depends on |
|---|---|---|---|
| 1 | `03-enable` | Yes: enables Amazon Media Storage and Media Cache; the tenant starts again and creates the bucket `oc-media` | the starting state |
| 2 | `03-check` | No | `03-enable` |
| 3 | `04-serve` | Yes: writes `media/home-bg.jpg` to the bucket and a local copy to `ms-cache` | `03-enable` |
| 4 | `06-cache` | Yes: enables Amazon Media Image Cache; the tenant creates the bucket `oc-imagecache`, and the home page writes one resized image to it | `04-serve` (the home page needs `home-bg.jpg`) |

**Snapshots.** Stop the site (by its process ID) and copy its `ORCHARD_APP_DATA` folder into `snapshots/S<n>/`:

- `S0`: after Auto Setup and the sign-in, before `03-enable`.
- `S1`: after `03-enable` (this is also the state for `03-check` and `04-serve`).
- `S2`: after `04-serve`, with the file uploaded.

To record a clip again: stop the site, delete the web root folders `ms-cache` and `media-cache` of the `Default` tenant,
and restore the snapshot that comes before the clip (`S0` for `03-enable`, `S1` for `03-check` and `04-serve`, `S2` for
`06-cache`). The emulator keeps its data only in its container, so for every restore also run `docker rm -f s3mock-video`,
start a new container with the command above, and then start the site: with `Create Bucket` set, it creates the buckets
when the tenant starts. For `S2`, upload `home-bg.jpg` through **Media**, **Library** before the take, off camera. Then
start the take.

## 3. The clips

Cursor, click ripples and timing follow `references/recording.md`. Every step below keeps its actions shorter than its
narration, and ends with the screen described under "End of the step". The admin menu is the redesigned one; if a group
cannot be opened reliably by clicking, finish each navigation with `NavigateAsync` of the path given in the step: the frame
at the end of the step must be the same.

### Clip `03-enable` (chapter 03)

Starts on `/Admin` (the dashboard), with the page loaded before `StartCaptureAsync`.

- `enable-1`
  - Actions: click the menu item **Tools**, then **Features** (`ClickAndWaitForNavigationAsync`); the page is
    `/Admin/Features`. Type `Amazon Media` into `#search-box` (`TypeAsync`). Wait until the row
    `[data-filter-value^='Amazon Media Storage OrchardCore.Media.AmazonS3 ']` is visible and the row
    `[data-filter-value^='Media Cache ']` is hidden. Highlight the category heading `.feature-group h3` whose text is
    **Hosting**.
  - End of the step: the search box holds `Amazon Media`; the **Hosting** heading is on top of the rows **Amazon Media
    Storage**, **Amazon Media Image Cache**, and **Amazon Media Image Cache (Obsolete)**, all with the status **Disabled**.
  - Labels: **Tools**, **Features** `OrchardCore.Features/AdminMenu.cs:26-27`; search box
    `OrchardCore.Features/Views/Admin/Features.cshtml:27`; category **Hosting** `OrchardCore.Media.AmazonS3/Manifest.cs:18,30,41`
    and the heading `Features.cshtml:88`; names `Manifest.cs:12,23,35`; the filter matches name, id and description
    `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`.
- `enable-2`
  - Actions: highlight the dependency badge **Media Cache** shown on the Amazon Media Storage row, if the layout shows it.
    Click **Enable** inside the row `[data-filter-value^='Amazon Media Storage OrchardCore.Media.AmazonS3 ']`
    (`ClickAndWaitForNavigationAsync`; the button is the link `a.btn` with the text **Enable**). The tenant starts again, so
    wait for the Features page to be shown again; if that takes longer than the narration, wrap the wait in
    `OffCameraAsync`. Type `Amazon Media` into `#search-box` again.
  - End of the step: the **Amazon Media Storage** row shows the status **Enabled** and a **Disable** button; no error message.
  - Labels: **Enable**, **Enabled** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`,
    `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml`; dependency `OrchardCore.Media.AmazonS3/Manifest.cs:14-17`.

### Clip `03-check` (chapter 03)

Starts on `/Admin`, loaded before `StartCaptureAsync`.

- `check-1`
  - Actions: click the menu items **Settings**, **Media**, **Amazon S3 Options** (`ClickAndWaitForNavigationAsync` on the
    last); the page is `/Admin/OrchardCore.Media.AmazonS3/Admin/Options`. Highlight the **Bucket Name** row, then the **Base
    Path** row, then the **Create Bucket** row.
  - End of the step: the page titled **Amazon S3 storage options**, with the notice "Amazon S3 storage is configured with
    appsettings.json." and a link **See documentation.**; **Bucket Name** shows `oc-media`; **Base Path** shows `media`;
    **Create Bucket** is checked and disabled, with the hint "Whether a bucket is created on startup if it does not exist."
    There is no row for keys, and no error notification.
  - Labels: **Settings**, **Media**, **Amazon S3 Options** `OrchardCore.Media.AmazonS3/AdminMenu.cs:17-19`; the page
    `Views/Admin/Options.cshtml:2` (title), `:5` (notice), `:6` (**See documentation.**), `:10` **Bucket Name**, `:17`
    **Base Path**, `:27` **Create Bucket**, `:28` (hint); the error that would replace "no error notification" is
    `Controllers/AdminController.cs:40`. The path has no route attribute: it comes from the default area pattern with the
    admin prefix (`OrchardCore.Admin/AdminAreaControllerRouteMapper.cs:12-49`), so confirm it with the menu link
    (see `OPEN-QUESTIONS.md`).
- `check-2`
  - Actions: click the menu items **Settings**, **Media**, **Options** (`ClickAndWaitForNavigationAsync` on the last); the
    page is `/Admin/Media/Options`. `ClearHighlightsAsync`, scroll down until the rows **Storage provider**, **Hierarchical
    namespace** and **Atomic move** are visible, then highlight each in turn.
  - End of the step: **Storage provider** shows `Amazon S3`; **Hierarchical namespace** and **Atomic move** each show a red
    cross.
  - Labels: **Options** `OrchardCore.Media/AdminMenu.cs:35-40`; rows `OrchardCore.Media/Views/Admin/Options.cshtml:131,139,147`;
    the value `Amazon S3` is `StorageName` of `OrchardCore.FileStorage.AmazonS3/AwsFileStorage.cs:18`, the crosses are
    `AwsFileStorage.cs:20` (`hasHierarchicalNamespace: false, supportsAtomicMove: false`), and a move is a copy followed by a
    delete (`AwsFileStorage.cs:188-192`).

### Clip `04-serve` (chapter 04)

Starts on `/Admin`, loaded before `StartCaptureAsync`.

- `serve-1`
  - Actions: click the menu items **Media**, then **Library** (`ClickAndWaitForNavigationAsync`); the page is `/Admin/Media`.
    Wait until the text `This folder is empty` is shown, then highlight it.
  - End of the step: the Media Library shows the empty root folder with the message `This folder is empty`.
  - Labels: **Media**, **Library** `OrchardCore.Media/AdminMenu.cs:18-21`; `This folder is empty`
    `OrchardCore.Media/Services/MediaJSLocalizer.cs:32`.
- `serve-2`
  - Actions: move to the **Upload files** button (`label[for='fileupload']`) and click it while a file chooser is
    intercepted: `var chooser = await scene.Page.RunAndWaitForFileChooserAsync(() => scene.ClickAsync(label));` then
    `await chooser.SetFilesAsync("<scratch>/upload/home-bg.jpg");`. If the chooser cannot be intercepted, call
    `scene.Page.Locator("#fileupload").SetInputFilesAsync(...)` after `MoveToAsync(label)`. Wait until the row
    `home-bg.jpg` is listed and the upload progress has gone.
  - End of the step: the root folder lists `home-bg.jpg`; no error in the upload area.
  - Labels: **Upload files** `OrchardCore.Media/Services/MediaJSLocalizer.cs:27`, input
    `OrchardCore.Media/Assets/media-gallery/src/App.vue:93-94`. The copy in the local cache folder is written by the upload
    itself: `OrchardCore.Media/Endpoints/Api/UploadMediaEndpoint.cs:141-146` calls
    `MediaEndpointHelpers.cs:214-245`.
- `serve-3`
  - Actions: `NavigateAsync("/media/home-bg.jpg")`. Wait until the picture is shown, then
    `HighlightAsync(scene.Page.Locator("img"), "/media/home-bg.jpg")`.
  - End of the step: the picture alone, with the caption `/media/home-bg.jpg` (the recording shows no address bar, so the
    caption names the path).
  - Labels: the path is `AssetsRequestPath`, default `/media` (`docs/reference/modules/Media/README.md`, `MediaOptions`);
    a file with no local copy is read from the bucket by `OrchardCore.Media/Services/MediaFileStoreResolverMiddleware.cs:65-104`;
    **Settings**, **Media**, **Cache** `OrchardCore.Media/MediaCacheAdminMenu.cs:18-22`.

### Clip `06-cache` (chapter 06)

Starts on `/Admin/Features`, loaded before `StartCaptureAsync` (the menu path was shown in `enable-1`).

- `cache-3`
  - Actions: type `Amazon Media Image Cache` into `#search-box`. Wait until the row
    `[data-filter-value^='Amazon Media Image Cache OrchardCore.Media.AmazonS3.ImageCache ']` is visible (not the
    **(Obsolete)** row). Click **Enable** inside that row (`ClickAndWaitForNavigationAsync`; it changes the tenant, so
    allow a few seconds, off camera if needed). Type `Amazon Media Image Cache` again.
  - End of the step: the row **Amazon Media Image Cache** shows the status **Enabled**; the **(Obsolete)** row is still
    **Disabled**; the row **Amazon Media Storage** is not part of the search, so it is not on screen.
  - Labels: `OrchardCore.Media.AmazonS3/Manifest.cs:21-31`; **Enable** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.
- `cache-4`
  - Actions: `NavigateAsync("/")`. Wait until `header.masthead` has a `background-image` that contains
    `/media/home-bg.jpg` and the picture has loaded (`WaitForFunctionAsync` that loads the URL in an `Image` and resolves on
    `onload`). Highlight `header.masthead` with the caption `banner profile`.
  - End of the step: the home page of the blog with its header banner picture visible behind the title.
  - Labels: banner `<oc>/src/OrchardCore.Themes/TheBlogTheme/Views/Content-Blog.liquid:9`; the `banner` profile
    `blog.recipe.json:1113`; the cache is read and written by `OrchardCore.Media/Middleware/MediaImageProcessingMiddleware.cs:112-190`
    and `OrchardCore.Media.AmazonS3/Services/AWSS3ResizedImageCache.cs:27-77`.

### Checks off camera (after `03-enable`, after `04-serve` and after `06-cache`)

Not recorded, but the narration depends on them. With the AWS CLI (any S3 client works as well; the keys are the dummy ones):

```bash
export AWS_ACCESS_KEY_ID=dummy AWS_SECRET_ACCESS_KEY=dummy AWS_DEFAULT_REGION=us-east-1
aws --endpoint-url http://localhost:9444 s3 ls
aws --endpoint-url http://localhost:9444 s3 ls s3://oc-media --recursive
aws --endpoint-url http://localhost:9444 s3 ls s3://oc-imagecache --recursive
```

After `03-enable`, the first command lists `oc-media`, and the log of the site shows "Starting with S3 Media Configuration.
BucketName: oc-media; BasePath: media" (`Startup.cs:65-66`). After `04-serve`, `oc-media` lists `media/home-bg.jpg`, and the
folder `<web root>/Default/ms-cache` holds `home-bg.jpg`. After `cache-3`, the first command also lists `oc-imagecache`.
After `cache-4`, `oc-imagecache` lists one object, named `<two characters>/<key>.<extension>`. If one of them does not match,
write it in `OPEN-QUESTIONS.md`.

**If a bucket is missing.** When the log of the site shows "Unable to create Amazon S3 Bucket" and the bucket is not in the
list, the emulator refused the creation request. Create the bucket by hand, still off camera, before the clip that needs it:
`aws --endpoint-url http://localhost:9444 s3 mb s3://oc-media` before `04-serve`, and
`aws --endpoint-url http://localhost:9444 s3 mb s3://oc-imagecache` before `06-cache`. The site needs no restart: it
only reads and writes objects in a bucket that exists. When the bucket is listed, the error comes from the request that
blocks public access, nothing is to be done, and the log line goes into `OPEN-QUESTIONS.md`. The same applies after every
restore of a snapshot, since each one starts a new emulator.

## 4. The slides

One line each; they are drawn by `slides.py`.

- `why-1`: three cards: the local disk is not permanent, instances share media, an S3 bucket keeps the files.
- `why-2`: three cards: the three features (the third marked Obsolete), each with its feature id.
- `why-3`: four cards, numbered: request, local copy, bucket, response.
- `conf-1`: the `OrchardCore:Media:AmazonS3` configuration; the lines of `BucketName` and `Region` are bright; cards "Required".
- `conf-2`: four numbered cards: the Credentials section, profiles, environment variables, the role of the host.
- `conf-3`: the same configuration; the lines of `BasePath` and `CreateBucket` are bright; cards "Optional".
- `conf-4`: four environment variable exports, and two cards on the order of the configuration sources and the double underscore.
- `conf-5`: the Docker command that starts S3Mock, the emulator configuration (`ServiceURL`, `ForcePathStyle`, dummy keys), and three cards.
- `svc-1`: the registration code of the Amazon Media Storage startup class, trimmed.
- `svc-2`: the startup class that replaces the temporary store of resumable uploads, trimmed.
- `cache-1`: the `ImageCache` configuration, and two cards.
- `cache-2`: the `IResizedImageCache` interface, trimmed, and the two lines that register the Amazon implementation.
- `tenant-1`: two configurations side by side, a bucket per tenant and a folder per tenant, and two cards: the rendered name is used as is, never lowercased (a tenant named `Default` gives `Default-media`), and it must be a valid bucket name.
- `tenant-2`: the `RemoveBucket` setting with its comment, and three cards: false by default, the objects are not deleted first, the image cache has its own setting.

## 5. Checks for private data

- The only account in the clips is `admin` with `admin@example.com`; the password is never typed on camera (`state.json`).
- The keys are `dummy`; no access key, secret key, profile or account of a real AWS account is typed, shown or logged. The
  options page shows no key, and a slide shows only placeholders (`<access key>`, `<secret key>`, `dummy`).
- The only file uploaded is `home-bg.jpg` of the Blog theme.
- Look at every contact sheet for the Blog recipe's sample content on the home page, and for any name other than the
  invented ones. OCR the finished video at one frame per second and at every scene change, and search for real names,
  email addresses other than `@example.com`, and tokens.
- Remove the S3Mock container when the recordings are done.

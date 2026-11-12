# Open questions: Amazon S3 Media Storage

Each item says what was checked in the source of Orchard Core (commit `92db4a83`) and its documentation, and what a
person must confirm on a running site before the video is recorded or published.

## The emulator stands in for Amazon S3

1. **Which emulator.** The tracker suggests MinIO or LocalStack. The documentation of the module names two emulators as
   known to work with its settings, S3Mock and LocalS3 (`docs/reference/modules/Media.AmazonS3/README.md`, "Configuring a
   Local Emulator"). The plan uses S3Mock: it is documented, it accepts any credentials, and its command and port come
   from the documentation, so no password or key of any kind enters git (MinIO documents default root credentials).
   MinIO and LocalStack are not mentioned by the documentation or the source, so they are unverified. Confirm that S3Mock
   works with the module: the tenant starts without an error in the log; the buckets `oc-media` and `oc-imagecache` are
   created; uploading, listing, reading, copying and deleting an object work. Only the Docker command and the
   `ServiceURL` change for another emulator.
2. **Bucket creation on an emulator.** When `CreateBucket` is set, the tenant creates the bucket and then sends a public
   access block (`OrchardCore.Media.AmazonS3/Services/AwsTenantEventsBase.cs:62-91`). An emulator that does not implement
   that request makes the second call fail, and the code logs "Unable to create Amazon S3 Bucket." although the bucket
   exists (`AwsTenantEventsBase.cs:98-101`). Confirm that the log of the recording site has no such error; if it has,
   the narration of `conf-3` still holds for Amazon S3, but the take would show an error in the log only. When the first
   call fails and the bucket does not exist, `recording-plan.md` ("If a bucket is missing") creates it by hand before the
   clip that needs it.
3. **`ForcePathStyle` and `ServiceURL` reach the client.** The section is bound with `GetAWSOptions(string.Empty)`
   (`AwsStorageOptionsExtension.cs:60`), while the comment above it (`:57-58`) says that the `AmazonS3Config` type
   parameter is necessary to bind S3-specific properties such as `ForcePathStyle`. No version of the AWS packages is pinned
   in the source that was read, so whether `ForcePathStyle` is applied could not be checked. `ServiceURL` is validated
   from the same object (`:21`), so a `ServiceURL` that is not bound shows as "not configured" on the options page. The
   slide `conf-5` and the narration of the same name follow the documentation. Confirm on the emulator that the requests
   use path-style addressing (the emulator's log shows `/oc-media/...` paths, not a host name that starts with the
   bucket), and that the options page shows the bucket name.
4. **No region.** The recording sets a `ServiceURL` and no `Region`, as the documentation asks for emulators. The bucket is
   created with `UseClientRegion = true` (`AwsTenantEventsBase.cs:62-66`). Confirm that this works without a region.
5. **Proxy variables.** `references/recording.md` points `HTTP_PROXY` and `HTTPS_PROXY` at a dead address, and the plan adds
   `NO_PROXY=127.0.0.1,localhost`. Nothing in the source says that the AWS client honors `NO_PROXY`. Confirm that the calls
   to the emulator work; if not, leave the proxy variables out of the environment of the recording site.

## What the narration says that a recording cannot show

6. **The local copy is made by the upload** (`serve-2`, `serve-3`). Checked: the upload endpoint calls
   `MediaEndpointHelpers.PreCacheRemoteMediaAsync` after writing the file
   (`OrchardCore.Media/Endpoints/Api/UploadMediaEndpoint.cs:141-146`, `MediaEndpointHelpers.cs:214-245`), and a request for a
   file with no local copy is served after `MediaFileStoreResolverMiddleware` reads it from the store
   (`OrchardCore.Media/Services/MediaFileStoreResolverMiddleware.cs:65-104`). Confirm that the folder
   `<web root>/Default/ms-cache` holds `home-bg.jpg` right after `serve-2`.
7. **Files already in the local media folder** (`serve-1`). The narration says only that the library reads the bucket,
   which is empty. Checked: no class of the module moves or copies existing files when the feature is enabled
   (`Startup.cs`, `Services/*`), and the documentation does not say either way. Confirm on the site that the three pictures
   that the Blog recipe imported (`about-bg.jpg`, `home-bg.jpg`, `post-bg.jpg`) are not listed in the library after
   `03-enable`, and that `/media/home-bg.jpg` answers 404 until `home-bg.jpg` is uploaded: the plan relies on both.
8. **Where the object goes.** With the base path `media`, the object of the upload is `media/home-bg.jpg`
   (`AwsFileStorage.cs:311-319` trims the slashes). The upload sends no content type
   (`AwsFileStorage.cs:291-296`), so the object has the emulator's default; the narration does not mention it.
9. **The resized image is written to the cache bucket** (`cache-4`). Checked: `MediaImageProcessingMiddleware` calls
   `IResizedImageCache.GetAsync` and `SetAsync` (`OrchardCore.Media/Middleware/MediaImageProcessingMiddleware.cs:113-170`),
   and `AWSS3ResizedImageCache.SetAsync` writes the object (`Services/AWSS3ResizedImageCache.cs:53-76`). Confirm that, after
   the home page loads, the bucket `oc-imagecache` holds one object, and that the folder `<web root>/Default/media-cache`
   stays empty.
10. **The home page banner** (`cache-4`) depends on the Blog recipe: the `Blog` content item has the media path
    `home-bg.jpg` and the theme resizes it with the `banner` profile (`Content-Blog.liquid:4-9`). Confirm that the header
    shows the picture after the upload and before the image cache is enabled, so the take of `cache-4` shows the same
    image whichever cache serves it.
11. **Credentials order** (`conf-2`). The order (the `Credentials` section, profiles, environment variables, the role of an
    ECS task or an EC2 instance) is the one the documentation gives for the AWS packages. It is not implemented in Orchard
    Core: the module only sets the `Credentials` of the options when the section has a key
    (`AwsStorageOptionsExtension.cs:62-81`). It was not run against AWS. The documentation also says that a host inside
    AWS needs only `BucketName`; the code requires a `Region` or a `ServiceURL` in the section
    (`AwsStorageOptionsExtension.cs:19-24`), and the narration of `conf-1` follows the code. Confirm on AWS that with
    only `BucketName` the log shows "Region or ServiceURL is a required attribute for S3 storage." and the site keeps its
    local file storage.
12. **Bucket creation, access blocking and removal on AWS** (`conf-3`, `tenant-2`). The statements come from
    `AwsTenantEventsBase.cs` (creation without ACLs, a public access block with all four flags, deletion on tenant removal)
    and were not run against AWS. `RemoveBucket` calls `DeleteBucketAsync` without emptying the bucket
    (`AwsTenantEventsBase.cs:119-131`). The narration of `tenant-2` says only that the removal asks S3 to delete the bucket,
    without deleting its objects first, and "reports an error if S3 refuses". The documentation says that the files of the
    image cache go with the tenant when it has a bucket of its own (`docs/reference/modules/Media.AmazonS3/README.md`,
    note under "Amazon Media Image Cache"), which the code does not do by itself. Confirm on the emulator and on AWS what
    happens when the bucket still holds objects, and whether the removal of the tenant reports the error.
13. **Resumable uploads** (`svc-2`). The startup class replaces the temporary store only when `OrchardCore.Media.Tus` is
    enabled and the media settings are valid (`Startup.cs:193-216`); the store writes to `_tus-uploads/{tenantId}/{fileId}`
    in the media bucket and ignores the base path (`Services/S3TusTempStore.cs:263`). It was not run, and the video
    does not show the object names.
14. **Templated names** (`tenant-1`). The rendered name is used as is: nothing lowercases it
    (`Services/AwsStorageOptionsConfiguration.cs:45-61`, `OrchardCore.Liquid.Abstractions/FluidOptionsParser.cs:25-34`), so
    the documentation's `{{ ShellSettings.Name }}-media` gives `Default-media` for the default tenant. The narration says
    only that the result must be a valid bucket name, and the slide says that `Default-media` has a capital letter. The
    documentation says the name is lowercased; the code does not do it (`AwsStorageOptionsConfiguration.cs:47`,
    `FluidOptionsParser.cs:22-31`). Confirm what the emulator and S3 accept. Also confirm that `{{ ShellSettings.Name }}` renders the name of the
    tenant: `FluidOptionsParser` renders with `TemplateOptions.Default` and sets `ShellSettings` as a value
    (`FluidOptionsParser.cs:36-41`), and no member registration for `ShellSettings` was found in the source that was read.
    The documentation of the Amazon, Azure and Tenants modules all describe the same syntax.
15. **The image cache has no base path on the slide.** The documentation's sample uses `"BasePath": "/media"` for the image
    cache. The cache joins the base path with `TrimEnd('/')` only (`AWSS3ResizedImageCache.cs:176-181`), so its object names
    start with a slash, unlike the media store, which trims both ends. The video does not show the setting.

## Screens

16. **The options page route.** `AdminController` of the module has no route attribute, so its path comes from the default
    area pattern with the admin prefix: `/Admin/OrchardCore.Media.AmazonS3/Admin/Options`
    (`OrchardCore.Admin/AdminAreaControllerRouteMapper.cs:12-49`). The step `check-1` clicks the menu path, so the path is
    only needed if the menu cannot be driven. Confirm it.
17. **The Features page.** Checked: the search matches the name, the id and the description of a feature
    (`FeatureEntryDisplayDriver.cs:20`), the category heading is `Hosting` (`Manifest.cs`), and enabling a feature
    enables its dependencies. Confirm that the search `Amazon Media` shows exactly the three rows named in the plan, that the
    **Hosting** heading is shown above them, and that the row shows the dependency badge **Media Cache** in the layout the
    recording uses.
18. **The admin menu.** The labels **Settings**, **Media**, **Amazon S3 Options**, **Options**, **Library**, **Tools**,
    **Features** and **Cache** are read from the menu providers. Confirm how the redesigned admin menu opens a group (click,
    hover, or an expanded group) so the steps can be recorded with the clicks the narration describes.
19. **The empty Media Library.** The text `This folder is empty` is a localized string of the library
    (`MediaJSLocalizer.cs:32`); confirm it is the text shown for the empty root folder of a bucket.

## Pronunciation

20. The voice has to say `S3`, `S3Mock`, `AWS`, `Amazon S3`, `URL`, `Liquid`, `Media TUS Uploads`, and the spoken
    replacements of the storyboard (`IMediaFileStore` as "I Media File Store", `IResizedImageCache` as "I Resized Image
    Cache", `App_Data` as "App Data", `ShellSettings` as "Shell Settings", `S3Mock` as "S3 Mock"). Check them with
    `scripts/pronounce.py` before narrating; no new spelling was tried here. The narration writes the configuration path as
    "the Orchard Core, Media, Amazon S3 section", while the slides show `OrchardCore:Media:AmazonS3`.

# Open questions: Azure Media Storage

Each item says what was checked in the source of Orchard Core (commit `92db4a83`) and its documentation, and what a
person must confirm on a running site before the video is recorded or published.

## Azurite stands in for Azure

1. **Does the module work against Azurite?** The tracker asks for a recording against the Azurite emulator in Docker.
   Checked: the module builds its clients from the connection string with `new BlobContainerClient(connectionString,
   containerName)`, `new BlobServiceClient(connectionString)` and `new DataLakeServiceClient(connectionString)`
   (`OrchardCore.FileStorage.AzureBlob/BlobFileStore.cs:77-81`), and probes the account type with `GetAccountInfoAsync`
   (`BlobFileStore.cs:94-176`). Nothing in the source mentions Azurite or `UseDevelopmentStorage=true`. Confirm that, with
   `OrchardCore__Media__Azure__ConnectionString=UseDevelopmentStorage=true`: the tenant starts without an error in the
   log; the container `oc-media` is created; uploading, reading and deleting a file work; and Settings, Media, Options
   shows `Azure Blob (Gen1)`. Read the log for the information line "Azure Blob Storage flat namespace detected"
   (`BlobFileStore.cs:142`): the page shows `Azure Blob (Gen1)` and two crosses as well when Azurite does not answer the
   account information request and the store falls back to flat-namespace operations with the warning "Unable to
   determine the Azure Blob Storage account type" (`BlobFileStore.cs:161-168`), so the page alone does not prove the
   detection. If Azurite rejects the API version of the storage client, start it with `--skipApiVersionCheck`, and say
   so in the recording plan.
2. **The Docker command on the slide `conf-4`** (`azurite-blob`, `--blobHost 0.0.0.0`, port 10000, and the image) comes
   from the documentation of Azurite, not from Orchard Core. Confirm it starts an emulator that the site reaches at the
   address `UseDevelopmentStorage=true` points to, on the machine that records. The slide and the narration name the
   image only as "the image that the Azurite documentation names", because the name of the image contains the host name
   of a container registry, a domain outside the three the programme allows. The recording plan names it the same way,
   as `<azurite-image>`.
3. **Pronunciation.** The voice has to say `Azurite`, `Gen1`, `Gen2`, `Blob`, `TUS` (in "Media TUS Uploads", step `svc-2`), and the spoken replacements of the
   storyboard (`IMediaFileStore` as "I Media File Store", `IResizedImageCache` as "I Resized Image Cache", `App_Data` as
   "App Data", `OrchardCore` as "Orchard Core"). Check them with `scripts/pronounce.py` before narrating; no new
   spelling was tried here.

## What the narration says that a recording cannot show

4. **Files already in the App_Data folder are not copied** (step `serve-1`). Checked: no class of the Azure Media module
   moves or copies existing files when the feature is enabled (`Startup.cs`, `Services/*`), and the documentation does
   not say either way. Confirm on the site: after enabling Azure Media Storage, the three pictures that the Blog recipe
   imported (`about-bg.jpg`, `home-bg.jpg`, `post-bg.jpg`) are not listed in the Media Library and
   `/media/home-bg.jpg` answers 404 until `home-bg.jpg` is uploaded. Opening the library does write one blob into the
   container, the marker of the folder of the signed-in user (question 11); it is not one of the existing files.
5. **The blob is written with a content type taken from the extension** (`serve-2`). Checked:
   `BlobFileStore.CreateFileFromStreamAsync` asks the `IContentTypeProvider` for the type, and falls back to
   `application/octet-stream` (`BlobFileStore.cs:659-690`). Confirm with the storage tool that `home-bg.jpg` has
   `image/jpeg`.
6. **A request copies the blob to a local folder when no copy exists** (`serve-3`). Checked:
   `MediaFileStoreResolverMiddleware` (`OrchardCore.Media/Services/MediaFileStoreResolverMiddleware.cs:65-104`) and the
   cache provider (`OrchardCore.Media.Core/DefaultMediaFileStoreCacheFileProvider.cs`). Uploading does not write the
   local copy (`OrchardCore.Media.Core/Events/DefaultMediaFileStoreCacheEventHandler.cs` only deletes on delete and move).
   The narration therefore does not say which request is the first. The library shows no image in its default table
   view (only the grid view has an `img`, `Assets/media-gallery/src/App.vue:141-147`, and the default is the table view,
   `services/LocalStorage.ts:9`), so with the default view the first request is the one of `serve-3`. If the grid view
   was chosen earlier (the library saves the choice in the local storage of the browser), the thumbnail request copies
   the blob during `serve-2`. Confirm that the folder `<web root>/Default/ms-cache` holds `home-bg.jpg` after `serve-3`,
   and note whether it already did after `serve-2`.
7. **The resized image is written to the cache container** (`cache-4`). Checked: `MediaImageProcessingMiddleware` calls
   `IResizedImageCache.GetAsync` and `SetAsync`, and `AzureBlobResizedImageCache` writes the blob
   (`OrchardCore.Media.Azure/Services/AzureBlobResizedImageCache.cs:52-77`). Confirm that, after the home page loads,
   the container `oc-imagecache` holds one blob, and that the folder `<web root>/Default/media-cache` stays empty. The
   recording plan deletes that folder before the take; before the Azure cache is enabled, the local cache (the folder
   `media-cache`, `PhysicalFileSystemResizedImageCache.cs:12,24`) is the one that stores any resized image, such as a
   thumbnail of the grid view of the library.
8. **The home page banner** (`cache-4`) depends on the Blog recipe: the `Blog` content item has the media path
   `home-bg.jpg` and the theme resizes it with the `banner` profile (`Content-Blog.liquid:4-9`). Confirm that the
   header shows the picture after the upload and before the cache is enabled, so the take of `cache-4` shows the same
   image whichever cache serves it.

## Screens

9. **The Features page.** Checked: the search matches the name, the id and the description of a feature
   (`FeatureEntryDisplayDriver.cs:20`), the category heading is `Hosting` (`Manifest.cs`), and enabling a feature enables
   its dependencies (`OrchardCore.Features/Controllers/AdminController.cs:247`). Confirm that the search `Azure Media`
   shows exactly the three rows named in the plan, that the **Hosting** heading is shown above them, and that the row
   shows the dependency **Media Cache** in the layout the recording uses.
10. **The admin menu.** The labels **Settings**, **Media**, **Azure Blob Options**, **Options**, **Library** and **Tools**,
    **Features** are read from the menu providers. Confirm how the redesigned admin menu opens a group (click,
    hover, or an expanded group) so the steps `check-1`, `check-2`, `serve-1` and `enable-1` can be recorded with the
    clicks the narration describes.
11. **The empty Media Library.** The text `This folder is empty` is a localized string of the library
    (`MediaJSLocalizer.cs:32`). The pane that contains it is drawn only when the library has at least one folder
    (`App.vue:136`; `assetsStore` holds the folders of the tree, `services/HierarchicalTreeBuilder.ts:255-262`), and
    loading the library creates the folder `_Users/<user id>` for a user who holds the permission Manage Own Media
    (`Endpoints/Api/GetDirectoryTreeEndpoint.cs:46-51`; the administrator of a site owns every permission). Confirm that
    the root folder of a fresh container shows `This folder is empty` next to the folder tree, that the tree shows
    `_Users`, and that the library shows no warning on a flat-namespace account.

## Gen2 accounts

12. **Nothing about Gen2 was run.** Azurite has no hierarchical namespace, so the statements of steps `tenant-3` and
    `tenant-4` come from the source only: directories and atomic moves use the Data Lake client when the account has a
    hierarchical namespace (`BlobFileStore.cs:203-232, 417-433, 546-560`), a folder of a flat account is a marker file
    named `OrchardCore.Media.txt` that listings skip (`BlobFileStore.cs:39, 325`), and `UseHierarchicalNamespace`
    overrides the detection (`BlobFileStore.cs:94-176`). A person with a Gen2 account should confirm them.
13. **The Media Library warning on a Gen2 account** (`tenant-4`) is a notification added by
    `OrchardCore.Media.Azure/Filters/MediaGalleryCapabilitiesFilter.cs:31-44`. Its text says the Media Library has not
    been fully verified against Gen2 storage and that full support is planned. The narration says only that the library
    warns on a Gen2 account. If the filter is removed, the last sentence of `tenant-4` must change.

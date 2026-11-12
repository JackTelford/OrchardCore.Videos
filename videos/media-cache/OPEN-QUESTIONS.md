# Open questions: Media Cache

Everything in the narration was read from the Orchard Core source at commit `92db4a83`. Nothing was run, because this
step has no running site. A person with the recording site (see `recording-plan.md`) should confirm the points below
before recording, or while looking at the contact sheets.

## Behavior to confirm on a running site

1. **Purge All empties only the cache of the original remote files.** `clean-1` says resized images are kept in a
   separate cache that Purge All leaves alone. In the source, the Purge action calls `PurgeAsync` of the cache file
   provider, whose root is `<web root>/<tenant name>/ms-cache`
   (`src/OrchardCore/OrchardCore.Media.Core/DefaultMediaFileStoreCacheFileProvider.cs`, `AssetsCachePath` and
   `PurgeAsync`), while the default resized image cache is `<web root>/<tenant name>/media-cache`
   (`src/OrchardCore.Modules/OrchardCore.Media/Services/PhysicalFileSystemResizedImageCache.cs:12,24`). Check it: list
   both folders under the web root of the site, request a resized picture (add a width to the address of a media file),
   purge, and list them again. Only `ms-cache` should be emptied.
2. **A purged file is downloaded again on the next request** (`purge-4`). In the source,
   `MediaFileStoreResolverMiddleware` copies the file from the store when it is not cached
   (`src/OrchardCore.Modules/OrchardCore.Media/Services/MediaFileStoreResolverMiddleware.cs:65-107`). Check that
   `ms-cache/home-bg.jpg` is absent right after the purge and present after the request.
3. **"Asset cache purged, with errors."** (`purge-3`). The narration says the message reads this way "if a file is in
   use and the site cannot delete it". The source sets it when `PurgeAsync` catches an `IOException`
   (`DefaultMediaFileStoreCacheFileProvider.cs`, `PurgeAsync`), and the details go to the log. This cannot be recorded
   without a locked file. Observation for the maintainers, not for the video: other exceptions (for example
   `UnauthorizedAccessException`) are not caught there, and `MediaCacheController.Purge` has no `return` after
   `RedirectToAction` when the cache is not registered (`MediaCacheController.cs:62`), which the page prevents by hiding
   the button.
4. **The two cleaners** (`clean-2` and `clean-3`). The source registers "Remote Media Cache Cleaner" (schedule `30 0 * * *`) and
   "Resized Media Cache Cleaner" (schedule `0 0 * * *`) as background tasks of the Media feature
   (`src/OrchardCore.Modules/OrchardCore.Media/Startup.cs:77-78`), and both return at once when `RemoteCacheMaxStale` /
   `ResizedCacheMaxStale` is not set. Confirm that, with the Background Tasks feature enabled, Tools, Background Tasks
   lists both titles, that they show a next run about a day apart from the last, and that nothing is removed without the
   time limits. The Blog recipe does not enable Background Tasks; the video does not record that page.
5. **The remote cleaner's rule is not narrated in detail** (`clean-3`). It removes a copy older than the time limit only when the file
   is gone from the store, or was changed there before the limit (`RemoteMediaCacheBackgroundTask.cs`, the two checks
   `entry is null` and `entry.LastModifiedUtc > ... && entry.LastModifiedUtc < minWriteTimeUtc`). The narration therefore
   only says the tasks "do nothing until the host configuration sets a time limit".
6. **Content delivery network advice** (`why-3`) comes from the documentation only (the "Media Cache" sections of
   `Media.Azure/README.md` and `Media.AmazonS3/README.md`). It is not tested.
7. **The notice on the Cache page.** The page shows "Media is configured with appsettings.json." when a remote store
   has registered its cache (`Index.cshtml:6-8`, `MediaCacheController.cs:45`). It was read for Azure Media Storage; the
   Amazon Media Storage startup registers the cache in the same way (`OrchardCore.Media.AmazonS3/Startup.cs:93`) but was
   not run.

## Screens and selectors to confirm

8. **Texts on the Cache page.** Two on-screen messages hold the name of the host configuration file (`Index.cshtml:8`
   and `:28`). The narration paraphrases them ("a notice that media is configured", "configured in the host
   configuration of the site") because an admin video never reads that name. The recording shows them as they are.
9. **Features page.** Confirm that the search box finds Media Cache by name; that the Enable button of the row has the
   id `btn-enable-OrchardCore_Media_Cache` (it comes from the HTML helper `GenerateIdFromName`, which replaces dots with
   underscores); and the exact text of the notification, "The feature Media Cache was enabled."
   (`Controllers/AdminController.cs:317` in the Features module).
10. **Sidebar.** Confirm how Settings, Media, Cache is reached: the sidebar has two items named Media (the top-level one,
    and the one inside Settings). The plan targets the last item by its address, `a[href$="/Admin/MediaCache"]`.
11. **Roles page.** Confirm that the Edit button of a role is visible without opening an actions menu (the layout of the
    list actions is a setting of the admin), that the group of the permission is titled "Media Cache" with the hint
    "(OrchardCore.Media.Cache)" (derived from the feature that registers the permission provider, and matching the
    permissions table of `Permissions.md`), and that the notification after Save reads "Role updated successfully.".
12. **The dialog.** The Purge All button sets a title and a message but no button texts, so the dialog takes "Ok" and
    "Cancel" from the admin theme's layout (`Layout.cshtml:99`). Confirm on screen. The narration does not name the
    button: it says "Let's confirm."
13. **Azurite.** Confirm that the connection string shortcut and `UseHierarchicalNamespace=false` work against Azurite
    with the SDK of the build, and that `NO_PROXY` keeps the calls to Azurite out of the dead proxy.
14. **Pronunciation.** Check "Amazon S3" and "Azure Blob Storage" with `scripts/pronounce.py` before narrating; no
    entry in `spoken` was needed in the draft.

## For the maintainers

15. **Where the video is embedded.** The tracker entry names one documentation page, `Security/Permissions.md`, which only
    lists the permission `ManageAssetCache` in a large table. The sections that explain what this video shows are the
    "Media Cache" sections of `Media.Azure/README.md` and `Media.AmazonS3/README.md`, and the permissions table of
    `Media/README.md`. `video.json` follows the tracker (`embeddedIn` is `Permissions.md`) and links to the Azure and
    Amazon S3 pages as well. Should the embed marker go on the Media pages instead?
16. **Cache folders and snapshots.** The two caches live in the web root of the web project, not in the tenant's
    `App_Data`; `recording-plan.md` says so, but confirm the folder names (`Default/ms-cache` and
    `Default/media-cache`) on the recording site.

## Added by the fact-check

17. **What is copied at once** (`why-2`). The narration says only that a file uploaded in the media library is copied
    right away. In the source, `MediaEndpointHelpers.PreCacheRemoteMediaAsync` is called by `UploadMediaEndpoint` and by
    `MoveMediaEndpoint` (the rename of one file) only. `MoveMediaListEndpoint`, which the library uses to move files to
    another folder (`FileLibraryManager.ts`, `fileListMove`), and the copy endpoint do not pre-cache, and no cache call
    was found for the resumable (TUS) uploads. Moving or deleting a file does remove its old copy
    (`DefaultMediaFileStoreCacheEventHandler`, via `MediaMovedAsync` and `MediaDeletedFileAsync`). Check on the
    recording site: after an upload, `ms-cache/<file>` exists at once; after moving a file to another folder, the old
    path is gone and the new path appears only after the first request.
18. **Who makes the copy** (`why-1`, `intro`). The Media Cache feature itself registers only the page and the permission
    (`MediaCacheStartup`, `Startup.cs:319-327`). The cache provider is registered by the storage features (Azure Media
    Storage, Amazon Media Storage), and the middleware that downloads a missing file is added by the Media feature when
    such a cache exists (`Startup.cs:259-265`). The narration therefore says "when your site stores media in Azure Blob
    Storage or Amazon S3, it keeps a local copy", and presents Media Cache as the feature that adds the Cache page.
19. **Editor and the admin panel** (`perm-1`, `perm-2`). Every page of the admin needs the permission Access admin
    panel as well (`AdminFilter.cs:58`). The default stereotype of the Editor role includes it
    (`OrchardCore.Admin/Permissions.cs`), and a role created by the Blog recipe receives the stereotype permissions of
    the installed features (`RoleUpdater.RoleCreatedAsync`), but this was not run. The video grants only Manage Asset
    Cache Folder. Confirm on the recording site that the Roles page of Editor shows Access admin panel as allowed, so
    that a user in that role could open the Cache page. The narration does not claim it.

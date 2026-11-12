# Open questions: Media Uploads

What was checked in the source (commit `92db4a83`) and what a person must confirm on a running site before the clips are
recorded. Where a check fails, change the narration in `storyboard.json` first, then record. Source paths are relative to
`src/OrchardCore.Modules/OrchardCore.Media/` unless they start with `src/`.

## 1. Pronunciation

Nothing was narrated, so no spelling was tried. Check with `scripts/pronounce.py` and listen to:

- **TUS** in "Media TUS Uploads" (steps `intro`, `why-2`, `enable-2`, `enable-4`, `create-5`, `resume-1`, `outro`). The storyboard has no
  entry for it. If the voice spells it letter by letter or says it wrongly, add a `spoken` entry.
- **SignalR**: the storyboard maps it to "Signal R". Confirm that it sounds right in "Media SignalR" and "SignalR Redis Backplane".
- The file names in the `spoken` map: `The team (2020).jpg`, `the-team-2020.jpg`, `the-team-2020-1.jpg`, `images-docs`,
  `retreat-video.mp4`, `office-tour.mp4`, `summer-picnic.jpg`. "M.P. 4" and "J.P.G." are guesses for how the voice reads them.

## 2. A slug that already exists (`create-5`)

Read: with Media TUS Uploads enabled, the file is first stored under a temporary id, then moved with
`GetAvailableMediaFilePathAsync` (`Startup.cs:731`), which adds `-1`, `-2`, ... to the name when the path is taken. The client
only compares the name as typed with the names in the list (`Assets/media-gallery/src/services/UppyFileUpload.ts:355-366`), so
`The Team 2020.jpg` is not blocked before the upload. Without TUS, `UploadMediaEndpoint.cs:124` refuses the second file with
**A file with this name already exists in the current folder.**, and the documentation says that both files cannot be uploaded.

Confirm on the site, with the three features enabled, that the second picture appears as `the-team-2020-1.jpg`. The narration only
claims this case. Optionally disable Media TUS Uploads once to see the refusal; it is not in the video.

## 3. Renaming does not slugify (`names-2`)

Read: only `CreateFolderEndpoint.cs:51`, `UploadMediaEndpoint.cs:90` and the TUS handlers (`Startup.cs:608`, `:657`) call the name
normalizer. The rename dialog sends the typed name to `MoveMediaEndpoint`, which does not normalize it, and the gallery has no
slug code. The narration says "so does a file you rename". Confirm: rename `the-team-2020.jpg` to `Team Photo 2020.jpg` and check
that the list keeps the typed name. (Restore snapshot S2 afterwards.)

## 4. The Options page rows (`enable-4`)

The page shows **SignalR** and **TUS uploads** with a check mark or a cross (`Views/Admin/Options.cshtml:155`, `:163`). The TUS
row tests a marker service of the feature. The SignalR row (and the same test that tells the gallery to open its connection,
`Controllers/AdminController.cs:52`) only checks that `IHubContext<MediaHub>` can be resolved, and the base SignalR feature
registers hub contexts for every hub (`src/OrchardCore.Modules/OrchardCore.SignalR/Startup.cs`). It is therefore likely that the row
shows a check mark as soon as the **SignalR** feature is enabled, whether or not **Media SignalR** is. In this video both are enabled,
so the narration is true, but do not rely on that row to prove Media SignalR.

Confirm after `enable-3` that both rows have a check mark, and that **Temporary file lifetime** is visible without a long scroll.

## 5. The Features page (`02-enable`)

- The Enable buttons are links that post (`data-url-af`); confirm that `ClickAndWaitForNavigationAsync` sees the navigation.
- The message names only the requested feature: confirm **The feature Media SignalR was enabled.** and that **SignalR** is enabled too
  (its row shows **Disable**), as `enable-3` says.
- Confirm that the search box is empty after each reload, and how long the first reload takes (tenant restart).
- The rows the filters show are read from the manifests (the search matches name, id and description, `Drivers/FeatureEntryDisplayDriver.cs:20` in
  `OrchardCore.Features`): the plan lists them for `enable-1` and `enable-3`. Confirm on the running site that the three rows to highlight are on
  screen together, or scroll between them, and that the page layout in use (list or grid) shows the **Enable** and **Disable** buttons the plan names.

## 6. The pause button and the throttle (`resume-1` to `resume-4`)

- The pause and play controls are icon buttons; their names **Pause upload** and **Resume upload** are tooltips
  (`Assets/media-gallery/src/components/UploadToast.vue:32-34`) that the capture does not draw. The narration says "the pause button"
  and "a play icon". Confirm that the first button of `.upload-toast-item` is the right one.
- Confirm that `Network.emulateNetworkConditions` with `uploadThroughput` slows the tus PATCH requests in the recorder's Chromium, that the
  speed text shows (for example `512 KB/s`), and that 524288 bytes per second keeps the bar below 60 % at the end of `resume-2`.
- The pause aborts the current request (`UppyFileUpload.ts:173`, `uppy.pauseResume`). Confirm that the bar then continues from where it stopped,
  not from 0 %, after the click in `resume-4`.

## 7. Short interruptions retry by themselves (`resume-2`)

Read: the tus plugin is configured with `retryDelays: [0, 1000, 3000, 5000]` and `onShouldRetry: () => true`
(`UppyFileUpload.ts:233-238`). Confirm once, outside the capture, by setting `offline` to true for two seconds with the CDP
call during an upload and back to false: the upload must carry on. If it does not, rewrite the second sentence of `resume-2`.

## 8. Resuming after a reload (`resume-5`, `resume-6`)

Read: the fingerprint of an upload is the name, the type, the size and the endpoint (`UppyFileUpload.ts:241-245`), tus-js-client keeps the
upload address in the browser's local storage and removes it on success (`removeFingerprintOnSuccess`), and the server keeps the progress of an
upload in a cache whose expiration is sliding and lasts the temporary file lifetime, one hour by default, counted from the last activity of the upload
(`Startup.cs:589-591`, `Services/DistributedMediaTusStore.cs:251-257`, `Services/MediaOptionsConfiguration.cs:76`). The narration of `enable-4` says only that
the setting is how long an unfinished upload can wait for its next part. Nothing in the source calls `RemoveExpiredFilesAsync`
(`Services/DistributedMediaTusStore.cs:207`), so whether the partial data on disk is removed after the hour is not known: do not narrate it. The chunk size in the
Media Library is fixed at 5 MB (`UppyFileUpload.ts:234-236`, `Views/Admin/Index.cshtml:21`); the narration does not give it.

Confirm that after the reload the bar of `office-tour.mp4` starts above 0 %, and that the Media Library reopens on `images-docs`
(`Assets/media-gallery/src/services/LocalStorage.ts:42-52`). The gallery also shows a "Resumed" icon next to the check mark when it detects a
resumed upload (`UploadToast.vue:42`, `UppyFileUpload.ts:157`, `:252-258`, `:504`); it is visible for three seconds and is not narrated. Note whether it appears.

## 9. Live updates (`05-live`)

Read: the hub adds a connection to the group of a folder only if the user may manage it (`Hubs/MediaHub.cs:22-27`), every file event is sent to
the group of the parent folder (`Hubs/MediaSignalREventHandler.cs`), and the gallery answers by loading the files of the folder it shows
(`Assets/media-gallery/src/services/SignalR.ts:65-67`). The returned folders are not used, so the narration speaks about files only.
Confirm that the row appears in window A within a few seconds of the upload in window B, and disappears after the delete, with no reload.
Confirm that a folder created in window B does not appear in the tree of window A (not narrated; for information).
Without Media SignalR a window refetches the files of a folder each time a folder is selected in the tree (`Assets/media-gallery/src/services/EventBusService.ts:91-94`,
`services/FileLibraryManager.ts:359-389`, cached list first, then the server's), and the code has no polling. That is why `why-3` says the list "does not update by
itself" and `live-5` says a window shows the changes of others "only after you reload the page or open a folder". Optionally confirm it once with Media SignalR disabled.
The recorder opens pages with `NetworkIdle`: the hub should use a WebSocket, which does not hold a request open. If `OpenAsync` times out on `/Admin/Media` once
Media SignalR is enabled, the connection fell back to long polling: use `NavigateAsync` with a shorter wait or open the page before the capture.

## 10. The several servers chapter (`servers-1`, `servers-2`)

`servers-2` restates the documentation (`src/docs/reference/modules/Media/MediaGallery.md`, the SignalR section) and the feature names
`SignalR Azure Backplane` and `SignalR Redis Backplane` (`src/OrchardCore.Modules/OrchardCore.SignalR.Azure/Manifest.cs`,
`OrchardCore.SignalR.Redis/Manifest.cs`). Nothing in the slides can be shown on a single-server site.

`servers-1` does not restate the documentation. The documentation says that the servers can share a temporary folder (`OrchardCore:TempDirectory:Path`) or use sticky
sessions. The source shows that a shared folder is not enough on its own: the file data goes to the folder (`Services/DiskTusTempStore.cs`, the default store), but the
length, offset, metadata and expiration of each upload (`Services/DistributedMediaTusStore.cs:54-146`) and the metadata of a finished upload
(`Services/DistributedTusUploadMetadataStore.cs`) go to `IDistributedCache`, which is a `MemoryDistributedCache` per server unless a distributed cache is registered
(`src/OrchardCore/OrchardCore.Infrastructure/Cache/OrchardCoreBuilderExtensions.cs:50`; the **Redis Cache** feature, `OrchardCore.Redis.Cache`, replaces it in
`src/OrchardCore.Modules/OrchardCore.Redis/Startup.cs:62-72`). The narration therefore names both. A person must confirm on two servers that the upload carries on when
the folder is shared and the **Redis Cache** feature is enabled, and fails when only one of the two is shared, before the video is recorded. The cloud storage features
(`OrchardCore.Media.Azure`, `OrchardCore.Media.AmazonS3`) replace the temporary store (`Startup.cs` of each module) and are not mentioned in the video.

# Open questions: Media Library: Upload, Organize and Reuse Files

What was checked in the Orchard Core source (commit `92db4a83`) and what a person must still confirm on a running
site. Nothing below is stated as a fact in the narration unless it says so.

## To confirm on the running site, before the first take

1. **The page heading.** `M/Views/Admin/Index.cshtml` renders `<h1 style="display:none">` with the title segment
   "Assets". The source does not show whether "Assets" appears anywhere on the Library page (a breadcrumb or a page
   title of the admin theme). The narration never names the heading; it says "Media, Library" only, the labels of
   `M/AdminMenu.cs`. If the page shows "Assets", decide whether `tour-1` should mention it.
2. **The admin menu on the Library page.** The plan assumes that on `/Admin/Media` the **Media** group is expanded and
   **Library** is visible, as the 4.0 menu marks the current item and its ancestors. Not verified in a browser.
3. **PrimeVue class names.** The folder lists of the Move file and Copy file dialogs (`p-treeselect`) and the three-dot
   menus (`p-menu`) come from PrimeVue, which is not in the module's source. The plan addresses them by text and by the
   class names `.p-treeselect`, `.p-treeselect-panel`, `.p-treeselect-overlay`. Probe them (`Recorder probe`) and fix
   the selectors in `files-3`, `files-4` and `folders-1`. Also check whether **Files** is expanded in the list that
   opens, or must be expanded first.
4. **After a copy, the open folder's list.** `Vue/services/FileLibraryManager.ts` (`fileCopy`) appends the copied file to
   the list of the folder that is open, even when the copy was made into another folder. The plan clicks the
   destination folder at the start of the third sentence of `files-4`, a few seconds after **Ok**; confirm what the list
   of the open folder shows during those seconds (a second `cider-menu.jpg` row would be wrong to keep on camera: then
   click the destination folder right after **Ok**), and that the list is correct after a refresh.
5. **Duplicate upload in the root folder.** The gallery's own check (`Vue/services/UppyFileUpload.ts`, the
   `files-added` handler) builds the path `/apple-harvest.jpg` for the root folder, while the stored paths have no
   leading slash, so for the root the refusal probably comes from the server's answer
   (`M/Endpoints/Api/UploadMediaEndpoint.cs`). Both give the same text, "A file with this name already exists in the
   current folder." Confirm that the **Validation error** toast shows it, and whether the **Uploads** panel also shows
   "Errors: 1".
6. **The timing of the Uploads panel.** Check marks stay 3 seconds after a success, toasts 5 seconds. On a fast local
   site the progress bar may not be visible at all. If `upload-2` shows no panel at the end of its first sentence,
   upload larger files, or shorten the wait in `upload-3`.
7. **Dropping files on the page.** The narration of `upload-1` says files can be dragged from the computer onto the
   page. The code registers the Uppy drop target on the page body (`Vue/services/UppyFileUpload.ts`, `DropTarget`
   with `target: document.body`). The overlay "Drop your file here" (`#customdropzone`, `Vue/App.vue`, line 3) only
   shows with the class `in`, which no code of the gallery adds, so the overlay may never appear; the narration does
   not mention it. Confirm by dropping a file on the page.
8. **Available Storage.** `FileSystemStore.GetPermittedStorageAsync` returns the free space of the drive that holds the
   media folder, or nothing when the process cannot read it, in which case the popover shows "Unspecified"
   (`M/Endpoints/Api/GetPermittedStorageEndpoint.cs`). Confirm that the recording machine shows a size, as `find-6`
   describes.
9. **Where the picker opens.** `reuse-2` assumes that the **Select Media** dialog opens at the root folder and lists
   files with check boxes like the library. The dialog mounts the same gallery, but the source does not show which
   folder it opens first.
10. **The local storage key.** The gallery keeps its preferences under a key (`LS_ID`) defined in the shared
    `.scripts/bloom` folder of Orchard Core, which is not part of the source provided. The plan relies on each clip
    starting from a fresh browser context.
11. **The `_Users` folder.** `M/Endpoints/Api/GetDirectoryTreeEndpoint.cs` (lines 46 to 51) creates `_Users/<user id>` for
    a user who holds Manage Own Media, and `ToDtoAsync` keeps it for the admin, so the tree, the Move file and Copy file
    folder lists and the Select Media dialog should show a folder **_Users** that the narration never names. Confirm that
    it shows and where it sits in the tree (the order is the file system's), and decide whether `tour-2` should say what
    it is. `folders-4` says "Every folder except Files also offers Delete"; the menu of **_Users** does offer it, but the
    server refuses with "Cannot delete a system folder" (`DeleteFolderEndpoint.cs`, line 54), so the sentence is about
    the menu only.

## Checked in the source, but not stated in the narration

- **Moving several files by dragging.** The Move entry of a file's menu moves only that file (`Vue/components/FileMenu.vue`,
  `files: [props.fileItem]`). Several ticked files move by dragging them onto a folder of the tree, which opens a
  **Move file(s)** dialog and moves only after **Ok** (`Vue/composables/useFolderDragDrop.ts`). The narration of
  `files-5` does not mention it, because the clip does not record a drag. A person may add a sentence and a drag
  (`DragToAsync` from a ticked row to a folder row, then **Cancel** in the dialog so the library stays as it is) after
  trying it on a running site.
- **The message of an empty folder.** `This folder is empty` is drawn inside a block shown only when the library holds at
  least one folder besides the root (`Vue/App.vue`, line 136, `assetsStore`, which holds the folders of the tree). On the
  recording site the folders **Harvest Festival**, **Orchard Photos** and **_Users** exist, so `folders-3` shows it; in a
  library without any subfolder the message would not appear.
- **Content that uses a file after it is renamed, moved or deleted.** No media event handler rewrites the paths a
  media field stores (the only handlers are the cache and SignalR ones), so content probably keeps the old path. The
  video says nothing about it. If a person confirms it on a running site, a short caution could be added to `files-2`
  or `files-3`.
- **Folder deletion.** The default file store deletes a folder with everything in it
  (`FileSystemStore.TryDeleteDirectoryAsync`). `folders-5` says so for "the default file storage" only; the Azure and
  Amazon stores are not checked.
- **Allowed file types.** The upload endpoint and the gallery refuse other file types (`M/Endpoints/Api/UploadMediaEndpoint.cs`;
  the message is "This file extension is not allowed: {0}"), but the video does not name the list or show the refusal.
  The Rename dialog's check is `isValidFileExtension` (`@bloom/media/file-extensions`, not in the source provided), so
  `files-2` says only that the new name keeps an allowed file extension.

## Pronunciation

The `spoken` map of the storyboard is empty. Check with `scripts/pronounce.py` how the voice says "Ok" and "OK" (the
labels of the dialogs), "TUS" (the label **Media TUS Uploads**), "SignalR" and "Slugify" before narrating, and add
a fix to `spoken` if one of them is misread.

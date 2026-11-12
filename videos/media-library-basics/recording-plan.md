# Recording plan: Media Library: Upload, Organize and Reuse Files

How to record the `media-library-basics` video without having to decide anything. Read it from top to bottom once, then
follow the sections in order. Every label below was read in the Orchard Core source (checkout at commit `92db4a83`,
called `OC` here); the paths are relative to `OC/src/OrchardCore.Modules/OrchardCore.Media/`, written `M/`.

Short names used for source files:

- `M/Services/MediaJSLocalizer.cs`: every label of the gallery (the "media-gallery" group) and of the media field (the
  "media-field" group). Called `Labels` below, with the line number.
- `Vue/`: `M/Assets/media-gallery/src/`, the Vue source of the gallery (`App.vue`, `components/*.vue`).
- `Picker/`: `M/Assets/media-picker/src/components/`, the media field and its "Select Media" dialog.

In this plan a selector stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. Selectors
were written from the Vue templates and the Razor views; PrimeVue's own class names (the Move and Copy folder list, the
folder menu) are not in the source of the module, so check them with `Recorder probe` before the first take (see
`OPEN-QUESTIONS.md`).

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- **Auto Setup** (one tenant, `Default`): recipe `Blog`, site name `Orchard Core`, time zone `America/Los_Angeles`,
  database `Sqlite`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git and passed as
  `VIDEO_PASSWORD`; the recorder signs in as `VIDEO_USER_NAME=admin`). The keys are
  `OrchardCore__AutoSetup__Tenants__0__ShellName`, `...SiteName`, `...SiteTimeZone`, `...AdminUsername`,
  `...AdminEmail`, `...AdminPassword`, `...DatabaseProvider`, `...RecipeName`, with an empty
  `OrchardCore__AutoSetup__AutoSetupPath`. Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead
  address. The video is recorded after the setup, so no setup screen or sign-in form is captured.
- **Features**: the Blog recipe enables `OrchardCore.Media` (`OC/src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`,
  line 65), which is all the video needs. Open `/Admin/Features`, search for `Media`, and check that **Media Slugify**,
  **Media TUS Uploads**, **Media SignalR**, **Secure Media**, **Media Cache** and every Azure and Amazon storage feature
  are disabled. Slugify would rename the uploaded files, TUS adds pause buttons to the Uploads panel, and Secure Media
  changes who can see the files. Do not enable any feature.
- **Settings and configuration**: none. Leave the `OrchardCore:Media` configuration at its defaults (in particular, do
  not set `DisableThumbnails`: the grid button must be visible), and leave **Settings, Media, API** on its default.
- **External services**: none.
- **What the Blog recipe already puts in the library** (`blog.recipe.json`, lines 1124 to 1137): `about-bg.jpg`,
  `home-bg.jpg` and `post-bg.jpg` in the root folder. They are the pictures of the Blog theme and hold no personal data.
- **The system folder `_Users`.** The first time `admin` loads the Library, `M/Endpoints/Api/GetDirectoryTreeEndpoint.cs`
  (lines 46 to 51) creates `_Users/<id of the admin user>`, because the admin holds Manage Own Media (Manage All Media
  Folders implies it: `OC/src/OrchardCore/OrchardCore.Media.Core/MediaPermissions.cs`, lines 9 to 16; the Administrator
  stereotype, `M/PermissionProvider.cs`, line 34), and `ToDtoAsync` (`M/Endpoints/Api/MediaEndpointHelpers.cs`, lines 67 to
  96) keeps every folder the admin may manage. So **Files** always has a folder **_Users** besides the folders this plan
  creates; it also shows in the folder list of the Move file and Copy file dialogs and in the Select Media dialog. The
  narration does not mention it. Never expand or open it on camera (its child folder is named by the user's generated id),
  and never delete it (the server answers "Cannot delete a system folder": `M/Endpoints/Api/DeleteFolderEndpoint.cs`, lines
  52 to 55). The tree lists the folders in the order the file store returns them (`FileSystemStore.GetDirectoriesAsync`), so
  the place of **_Users** among **Old Drafts** and **Orchard Photos** depends on the operating system. The state tables
  below leave it out: every state has it.

### The fake files (made outside the capture, never committed)

Make the files with Pillow into a folder outside both repositories. Every picture is generated noise with a label, so
no real photo is used:

```bash
python - <<'PY'
import os
from PIL import Image, ImageDraw, ImageFont

out = os.path.join(os.environ.get("TEMP", "/tmp"), "ocvideo-media-seed")
colors = [(176, 58, 46), (212, 172, 13), (39, 174, 96), (41, 128, 185), (125, 60, 152), (211, 84, 0)]
font = ImageFont.load_default(size=56)

def make(folder, name, width, label, color):
    os.makedirs(os.path.join(out, folder), exist_ok=True)
    height = width * 3 // 4
    image = Image.blend(Image.effect_noise((width, height), 40).convert("RGB"), Image.new("RGB", (width, height), color), 0.6)
    ImageDraw.Draw(image).text((32, 32), label, fill=(255, 255, 255), font=font)
    image.save(os.path.join(out, folder, name), quality=85)

make("root", "draft-photo-1.jpg", 800, "Draft 1", colors[0])
make("root", "draft-photo-2.jpg", 800, "Draft 2", colors[1])
make("old-drafts", "old-flyer.jpg", 800, "Old flyer", colors[2])
for n in range(1, 13):
    make("orchard-photos", f"photo-{n:02d}.jpg", 400 + 60 * n, f"Photo {n}", colors[n % 6])
make("upload", "apple-harvest.jpg", 1600, "Apple harvest", colors[3])
make("upload", "cider-menu.jpg", 1400, "Cider menu", colors[4])
print(out)
PY
```

The widths of the twelve `photo-NN.jpg` files grow with the number, so each one is larger than the one before it,
which makes the Size sort in `find-3` visible. Nothing in the folder is uploaded yet except by the next section.

### Seeding the library (not recorded)

In a normal browser window, signed in as `admin`, open `/Admin/Media` and, using the library itself:

1. In the root folder **Files**, click **Upload files** and choose `draft-photo-1.jpg` and `draft-photo-2.jpg` from
   the `root` folder of the seed folder.
2. Point at **Files**, click the button with three dots, choose **Create a subfolder**, type `Old Drafts`, and click
   **Ok**. Select **Old Drafts**, click **Upload files**, and choose `old-flyer.jpg`.
3. Select **Files** again, create the subfolder `Orchard Photos` the same way, select it, click **Upload files**, and
   choose the twelve files of the `orchard-photos` folder at once.

Check the result before the first take. Root folder: `about-bg.jpg`, `draft-photo-1.jpg`, `draft-photo-2.jpg`,
`home-bg.jpg`, `post-bg.jpg`. Folder tree under **Files**: **Old Drafts** (one file), **Orchard Photos** (twelve files)
and the system folder **_Users** (see above). Do not record from the browser window that seeded the site. Sign the
recorder in (next section), then take snapshot **S0**.

### Signing the recorder in

Run `Recorder setup` once (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`). Every clip opens its own fresh browser
context from `state.json`, which holds only the sign-in. The gallery keeps its view (list or grid), the selected
folder, the page size and the thumbnail size in the browser's local storage (`Vue/services/LocalStorage.ts`), so each
clip starts at the root folder **Files**, in the list view, with 10 files per page. If a clip opens in another folder
or in the grid view, `state.json` holds preferences from an earlier visit of the library: run `Recorder setup` again.

Then take snapshot **S0**: stop the site (by its process ID) and copy its `App_Data` folder and the recorder's
`state.json` aside. A restored `App_Data` folder goes with the `state.json` copied next to it.

## The order of the clips, and which ones change the site

Record the clips in the order they play. Each state below is the content of the library; the folders hold only what is
listed, except for the system folder **_Users**, which every state has under **Files** (see "The system folder
`_Users`" above).

| State | Content |
|---|---|
| **S0** | Root: `about-bg.jpg`, `draft-photo-1.jpg`, `draft-photo-2.jpg`, `home-bg.jpg`, `post-bg.jpg`. Folders: **Old Drafts** (`old-flyer.jpg`), **Orchard Photos** (`photo-01.jpg` to `photo-12.jpg`). |
| **S1** | S0 plus `apple-harvest.jpg` and `cider-menu.jpg` in the root. |
| **S2** | S1 plus an empty folder **Harvest Festival**, and without **Old Drafts**. |
| **S3** | S2 with `apple-harvest.jpg` renamed `harvest-table.jpg` and moved into **Harvest Festival**, a copy of `cider-menu.jpg` in **Harvest Festival**, and without `draft-photo-1.jpg` and `draft-photo-2.jpg`. |

| Clip | Starts in | Changes the site | Ends in |
|---|---|---|---|
| `02-tour` | S0 | no | S0 |
| `03-upload` | S0 | yes: uploads two files | S1 |
| `04-folders` | S1 | yes: creates a folder, deletes a folder | S2 |
| `05-files` | S2 | yes: rename, move, copy, delete | S3 |
| `06-find` | S3 (S0 to S3 all work: it only needs **Orchard Photos**) | no | unchanged |
| `07-reuse` | S3 (it needs `harvest-table.jpg` in **Harvest Festival**) | no: the Article is never saved | S3 |

After every clip that changes the site (`03-upload`, `04-folders`, `05-files`), stop the site and copy `App_Data` aside
as S1, S2 and S3. To record a clip again, stop the site, restore the snapshot the clip starts in (the table above),
and start the site. `05-files` depends on `03-upload` (the two uploaded files) and `04-folders` (the folder **Harvest
Festival**); `07-reuse` depends on `05-files`.

## Rules for every clip

- Open the clip's start page with `scene.OpenAsync("/Admin/Media")` before `StartCaptureAsync()`, and wait until
  `#fileContainer` is visible and the table `.file-items-table` shows rows (the page shows a spinner while it loads).
  Open `/Admin` once after starting the site, outside the capture, so the first request is not slow.
- **Hover first.** The three-dot button of a folder (`.folder-actions`) has opacity 0 until the pointer is over the
  folder row (`Vue/style.css`, line 97 to 104). Move the pointer to the row with `MoveToAsync` before clicking the
  button.
- **Icon-only buttons have no text.** The trash can, download, grid, drive and gear buttons, the three-dot buttons and
  the sort arrows show an icon; their names are tooltips, and a headless browser draws no tooltip. Where a step
  mentions one of them, draw `HighlightAsync(locator, caption)` around it with the caption given below.
- The three-dot menus are PrimeVue popup menus. Click an entry by its exact text, for example `text="Rename"` (with the
  quotation marks, which make the match exact).
- Dialogs are drawn at the end of the page body, not inside the library; address them page-wide. The **Ok** button is
  `#btn-submit` in the Create folder, Delete folder, Rename file, Move file and Copy file dialogs; the Delete file
  and Move file(s) dialogs have no id on it: use `.action-modal button:has-text("Ok")` there.
- A toast (message in the top right corner) closes by itself after 5 seconds, or 8 seconds when it is an error
  (`Vue/components/NotificationToast.vue`, line 74; the toasts of this video are warnings and successes, so 5 seconds); the check marks in the **Uploads** panel disappear 3 seconds after the upload succeeds
  (`Vue/components/UploadToast.vue`, lines 127 to 138, handler of `UploadSuccess`). Start the action that causes them so that they are
  on screen when the narration of the step reaches their sentence.
- Keep the actions of a step shorter than its narration.

## Slides

The chapters `01` and `08` are slides, drawn by `slides.py`; nothing is recorded for them.

- `why-1`: heading "One library for the files of a site"; three cards, the first bright: "Files in many places".
- `why-2`: the same slide with the second card bright: "One library".
- `why-3`: the same slide with the third card bright: "Used by content".
- `more-1`: heading "Optional features of the Media module"; three bright cards: "Media Slugify", "Media TUS Uploads",
  "Media SignalR". The names are the feature names of `M/Manifest.cs`.

The opening card (`intro`) and the closing card (`outro`) are drawn by the assembling script from the titles of the
storyboard.

## Clip `02-tour`

**Starts on** `/Admin/Media`, state S0, nothing selected. Nothing is clicked in this clip except for pointer moves;
`HighlightAsync` marks what the narration names. Before `StartCaptureAsync()`, check that the admin menu shows
**Media** with **Library** under it (on the Library page the group is expanded and **Library** is the current entry);
if the group is collapsed, click **Media** before the capture starts.

- `tour-1`: `HighlightAsync(scene.Page.Locator("nav#left-nav a:has-text('Library')"), "Media, Library")`. On screen at
  the end: the ring around **Library** in the left menu, with the library page behind it. Labels: **Media** and
  **Library** from `M/AdminMenu.cs`, lines 18 and 21; the permission that shows the entry (`ManageMedia`), line 22.
- `tour-2`: `ClearHighlightsAsync()`, then highlight `#folder-tree` with the caption "Folder tree". After about 4
  seconds, clear it and highlight `.file-items-table` with the caption "Files of the folder". On screen at the end: the
  ring around the file list; the tree shows **Files** with **Old Drafts**, **Orchard Photos** and the system folder
  **_Users** under it (see "The system folder `_Users`" above; the narration does not name the folders of the tree, so
  leave **_Users** collapsed). Label **Files**: `Labels`, line 31 (`FileLibrary`); the server names the root folder with an
  empty string (`M/Services/MediaDirectoryTreeCache.cs`, `BuildTreeAsync`), so the tree shows the label, as
  `Vue/services/HierarchicalTreeBuilder.ts`, line 246 (`setServerDirectoryTree`) has it.
- `tour-3`: clear the highlights, then highlight `.file-items-table .table-head` with the caption "Name, Last modified,
  Size". On screen at the end: the ring around the header row of the list. Labels: **Name**, **Last modified**,
  **Size**: `Labels`, lines 37 to 39; the list shows files only, folders are in the tree (`Vue/services/FileLibraryManager.ts`,
  `getFileLibraryStoreAsync`: `currentFiles.filter(f => !f.isDirectory)`).
- `tour-4`: clear the highlights, then highlight `label.upload-button` with the caption "Upload files", and after
  about 3 seconds `.action-bar a[title="Delete"]` with the caption "Delete" and `.action-bar a[title="Download"]`
  with the caption "Download". On screen at the end: three rings in the toolbar above the list. Labels: **Upload
  files**: `Labels`, line 27; the two buttons are titled **Delete** and **Download** (`Labels`, lines 28 and 29);
  placement in `Vue/App.vue`, lines 93 to 110.

## Clip `03-upload`

**Starts on** `/Admin/Media`, state S0, folder **Files** selected (it is, by default). The files to upload are
`apple-harvest.jpg` and `cider-menu.jpg` in the `upload` folder of the seed folder. Keep a variable `chooser` of type
`IFileChooser` for the whole clip.

- `upload-1`: highlight `#folder-tree .treeroot` with the caption "Files" for about 3 seconds, then
  `ClearHighlightsAsync()`, then `chooser = await scene.Page.RunAndWaitForFileChooserAsync(() =>
  scene.ClickAsync(scene.Page.Locator("label.upload-button")))`. Do not choose the files yet: the headless browser
  shows no file dialog. On screen at the end: the pointer on **Upload files**, the list unchanged (five files).
  Label **Upload files**: `Labels`, line 27; `Vue/App.vue`, line 93 to 97 (the label wraps the hidden input
  `#fileupload`). The sentence about dragging files from the computer is only narrated: a person cannot drag a file out
  of the operating system with the recorder, so nothing is recorded for it.
- `upload-2`: at the very start of the step, `await chooser.SetFilesAsync(new[] { <seed>/upload/apple-harvest.jpg,
  <seed>/upload/cider-menu.jpg })`. The **Uploads** panel appears at the bottom right with both file names, a progress
  bar, and a check mark each, and both files join the list. Wait until the list shows both names
  (`.file-items-table .file-name-cell:has-text("apple-harvest.jpg")`). On screen at the end: the list with seven files
  (`about-bg.jpg`, `apple-harvest.jpg`, `cider-menu.jpg`, `draft-photo-1.jpg`, `draft-photo-2.jpg`, `home-bg.jpg`,
  `post-bg.jpg`); the panel may have disappeared by then (it closes 3 seconds after the success). Labels: **Uploads**:
  `Labels`, line 73, shown by `Vue/components/UploadToast.vue`, line 9.
- `upload-3`: `PauseAsync(4500)`, then repeat the upload of `apple-harvest.jpg`: click `label.upload-button` inside
  `RunAndWaitForFileChooserAsync`, and `SetFilesAsync` with `<seed>/upload/apple-harvest.jpg` again. Wait for the toast
  (`.notification-toast-detail:has-text("A file with this name already exists in the current folder.")`). On screen at
  the end: the toast in the top right corner, titled **Validation error**, with the text "A file with this name already
  exists in the current folder."; the list still has seven files, and `apple-harvest.jpg` appears once. The Uploads panel
  may also show "Errors: 1" with the same message. Both messages are `Labels`, lines 103 and 104; the check is made by the
  server (`M/Endpoints/Api/UploadMediaEndpoint.cs`, "A file with this name already exists in the current folder.") and by
  the gallery (`Vue/services/UppyFileUpload.ts`, the `files-added` handler). The pause is there so that the toast, which
  lasts 5 seconds, is on screen while the narration reads its message; adjust it with the contact sheet.

## Clip `04-folders`

**Starts on** `/Admin/Media`, state S1, folder **Files** selected. The tree shows **Old Drafts**, **Orchard Photos** and
**_Users** (collapsed, leave it so).

- `folders-1`: `MoveToAsync(scene.Page.Locator("#folder-tree .treeroot"))`, then `ClickAsync(scene.Page.Locator("#folder-tree
  .treeroot .folder-actions a.action-button"))`. The folder menu opens with a single entry, **Create a subfolder**. On
  screen at the end: the open menu under the three-dot button of **Files**. Label **Create a subfolder**: `Labels`, line
  64; the menu is built in `Vue/components/FolderMenu.vue`, lines 66 to 82 (the root folder has no **Delete** entry,
  `isRoot`, lines 47 and 74); the button is drawn only on hover, `Vue/components/FolderRow.vue`, line 25.
- `folders-2`: `ClickAsync(scene.Page.Locator("text='Create a subfolder'"))`. The dialog titled **Create folder** opens
  (`Labels`, line 65) with the path `/` in bold, an input with the placeholder "Enter a folder name" and the buttons
  **Cancel** and **Ok**. `TypeAsync(scene.Page.Locator("input[name='create-folder']"), "Harvest Festival")`, then
  `ClickAsync(scene.Page.Locator("#btn-submit"))`. On screen at the end: **Harvest Festival** listed under **Files** in
  the tree (before **Old Drafts**: the gallery inserts a new folder in name order among its siblings,
  `Vue/services/FileLibraryManager.ts`, lines 159 to 166, while the folders that were already there keep the order the
  server gave them), the dialog closed. Labels: **Ok** `Labels`, line 69; the placeholder is written in
  the template, `Vue/components/ModalFolderAction.vue`, line 14; the input is named `create-folder`, line 15.
- `folders-3`: `ClickAsync(scene.Page.Locator("#folder-tree li.folder:has-text('Harvest Festival') .folder-name"))`, then
  highlight `.breadcrumb-path` with the caption "Breadcrumb". On screen at the end: the breadcrumb reading **Files**
  then **Harvest Festival**, the message "This folder is empty" in the middle of the page (`Labels`, line 32; shown by
  `Vue/App.vue`, lines 186 to 193), and the ring around the breadcrumb. Breadcrumb: `Vue/composables/useBreadcrumbs.ts`.
- `folders-4`: `ClearHighlightsAsync()`, `MoveToAsync(scene.Page.Locator("#folder-tree li.folder:has-text('Old Drafts')"))`,
  click its three-dot button (`#folder-tree li.folder:has-text('Old Drafts') .folder-actions a.action-button`). The menu
  shows **Create a subfolder** and **Delete**. When the narration reaches "Choose it on Old Drafts", click
  `text="Delete"`: the dialog titled **Delete folder** opens with the question "Are you sure you want to delete this
  folder?" and the folder name **Old Drafts**. Do not click **Ok** in this step. On screen at the end: the open **Delete
  folder** dialog over the library. Labels: **Delete** `Labels`, line 49 (entry added only when the folder is not the
  root, `Vue/components/FolderMenu.vue`, lines 74 to 81); **Delete folder** and the question: `Labels`, lines 66 and 67.
- `folders-5`: with the dialog still open, wait about 4 seconds (the first sentence, about the default file storage, is
  spoken over the dialog), then click **Ok** (`#btn-submit`) when the narration reaches "Click Ok". Wait until the tree
  no longer lists **Old Drafts** (`#folder-tree li.folder:has-text('Old Drafts')` hidden). On screen at the end: the tree
  without **Old Drafts** (**Harvest Festival**, **Orchard Photos** and **_Users** remain), and the root folder **Files**
  selected with its list of seven files, because after a folder is deleted the gallery selects the parent of the deleted
  folder, whichever folder was open (`Vue/services/EventBusService.ts`, lines 116 to 123). The sentence about the
  files inside the folder is true because the default file store deletes a directory recursively
  (`OC/src/OrchardCore/OrchardCore.FileStorage.FileSystem/FileSystemStore.cs`, `TryDeleteDirectoryAsync`).

## Clip `05-files`

**Starts on** `/Admin/Media`, state S2, folder **Files** selected. The list shows, sorted by name: `about-bg.jpg`,
`apple-harvest.jpg`, `cider-menu.jpg`, `draft-photo-1.jpg`, `draft-photo-2.jpg`, `home-bg.jpg`, `post-bg.jpg`. A row's
menu button is the three-dot button in its last cell: `.file-items-table .table-row.file-item:has-text("<name>")
.table-cell:last-child a.action-button`.

- `files-1`: `ClickAsync` on the menu button of the row `apple-harvest.jpg`. The menu opens with **Rename**, **Move**,
  **Copy**, **View**, **Download**, **Delete** in this order. On screen at the end: the open menu with the six entries.
  Labels: `Vue/components/FileMenu.vue`, lines 60 to 98; `Labels`, lines 41, 43, 45, 47, 48, 49.
- `files-2`: `ClickAsync(scene.Page.Locator("text='Rename'"))`. The dialog titled **Rename file** opens with the label **File
  name** and the input `input[name='rename']` holding `apple-harvest.jpg`. Clear it (`FillAsync("")`), then
  `TypeAsync(input, "harvest-table.jpg")`, and click `#btn-submit`. On screen at the end: the list with
  `harvest-table.jpg` between `draft-photo-2.jpg` and `home-bg.jpg`. Labels: **Rename file** `Labels`, line 42; **File
  name**, line 54 (`Vue/components/ModalFileAction.vue`, line 30); the extension check (message "Invalid file
  extension", `Labels` line 105) is in the same file, lines 116 and 117, which is why the new name keeps `.jpg`.
- `files-3`: `ClickAsync` on the menu button of the row `harvest-table.jpg`, then `text="Move"`. The dialog titled **Move
  file** opens with a folder selector whose placeholder is "Select a folder". Click the selector
  (`.p-treeselect`), expand **Files** if its children are not shown, click **Harvest Festival** in the list that opens
  (`.p-treeselect-panel, .p-treeselect-overlay` contains it; the same name is in the tree behind, so address the
  panel; the list holds **Harvest Festival**, **Orchard Photos** and **_Users**, so do not open **_Users**), then click
  `.action-modal button:has-text("Ok")`. Wait for the toast. On screen at the end: the toast
  "Success / File(s) moved successfully." in the top right corner, and the list without `harvest-table.jpg`. Labels:
  **Move file** `Labels`, line 44; **Select a folder**, line 55; the toast, lines 99 and 100. The Move dialog's folder
  list is `getDirectoryTreeNode()` in `Vue/services/HierarchicalTreeBuilder.ts`; its root is labeled **Files** (line 37).
- `files-4`: `ClickAsync` on the menu button of the row `cider-menu.jpg`, then `text="Copy"`. The dialog titled **Copy file**
  opens (`Labels`, line 46) with the same folder selector. During the first sentence, choose **Harvest Festival** as in
  `files-3`. At the start of the second sentence ("Click Ok"), click `.action-modal button:has-text("Ok")` and wait for the
  toast. At the start of the third sentence ("Open Harvest Festival"), click **Harvest Festival** in the tree
  (`#folder-tree li.folder:has-text('Harvest Festival') .folder-name`). On screen at the end: the toast "File copied
  successfully." (`Labels`, line 101) may already be gone; the list of **Harvest Festival** shows `cider-menu.jpg` and
  `harvest-table.jpg`. Do not wait between the toast and the folder click: right after a copy the gallery also adds the
  copy to the list of the folder that is still open (`Vue/services/FileLibraryManager.ts`, `fileCopy`), which would be
  wrong to show for long.
- `files-5`: `ClickAsync` on the root row of the tree (`#folder-tree .treeroot .folder-name`). Then tick the check boxes of
  the rows `draft-photo-1.jpg` and `draft-photo-2.jpg` (`input[name='select-file']` in each row). Highlight
  `.action-bar a[title="Delete"]` with the caption "Delete" and `.action-bar a[title="Download"]` with the caption
  "Download". On screen at the end: two ticked rows, the figure 2 next to the trash can and download icons
  (`Vue/App.vue`, lines 101 and 108), and the two rings. The narration says the buttons act on all the selected files
  (the Delete button of `files-6` does; the Download button downloads them: `Vue/services/Utils.ts`,
  `downloadSelectedFiles`). Dragging the ticked files onto a folder of the tree also works, but it is neither narrated
  nor recorded (see `OPEN-QUESTIONS.md`).
- `files-6`: `ClearHighlightsAsync()`, then `ClickAsync(scene.Page.Locator(".action-bar a[title='Delete']"))`. The dialog
  titled **Delete file** opens with the question "Are you sure you want to delete these files?". When the narration
  reaches "Click Ok", click `.action-modal button:has-text("Ok")`. On screen at the end: the list with four
  files, `about-bg.jpg`, `cider-menu.jpg`, `home-bg.jpg`, `post-bg.jpg`. Labels: **Delete file** `Labels`, line 50; the
  question, line 52 (plural, used when more than one file is selected, `Vue/services/ConfirmModalService.ts`).

## Clip `06-find`

**Starts on** `/Admin/Media`, any state from S0 to S3, folder **Files** selected, list view, 10 files per page.

- `find-1`: `ClickAsync(scene.Page.Locator("#folder-tree li.folder:has-text('Orchard Photos') .folder-name"))`. Wait for the
  list to show `photo-01.jpg`. Highlight `#file-pager` with the caption "Pager". On screen at the end: the breadcrumb
  **Files**, **Orchard Photos**; ten rows (`photo-01.jpg` to `photo-10.jpg`); the pager below the list with **First**,
  **Previous**, **1**, **2**, **Next**, **Last**, and the ring around it. The narration says ten files per page (the
  default page size, `Vue/services/LocalStorage.ts`, line 10, and the 10 entry of the page size list, `Vue/App.vue`,
  line 63) and two pages for the twelve files. The pager is drawn whenever the folder has files (`Vue/App.vue`, lines 195
  to 197), with the buttons that cannot be used disabled; with twelve files the page links are 1 and 2
  (`PagerComponent.vue`, `pageLinks`). Labels: `Labels`, lines 82 to 85; `Vue/components/PagerComponent.vue`, lines 5 to
  27.
- `find-2`: `ClearHighlightsAsync()`, then `TypeAsync(scene.Page.Locator("#file-filter-input"), "photo-1")`. On screen at
  the end: the filter box holding `photo-1`, and three rows, `photo-10.jpg`, `photo-11.jpg` and `photo-12.jpg`.
  Label (the placeholder): "Search and filter..." `Labels`, line 30; `Vue/App.vue`, lines 125 and 126; the match is "the
  name contains the text, ignoring case", `Vue/composables/useFileListFiltering.ts`, lines 9 to 11.
- `find-3`: `ClickAsync(scene.Page.Locator("#clear-file-filter-button"))` so the whole folder is listed again, then click
  the header `.table-head .table-cell:has-text("Size")` twice, pausing 2 seconds between the clicks. Highlight
  `.table-head` with the caption "Click a heading to sort". On screen at the end: the list sorted by size from the
  largest, `photo-12.jpg` first, and an arrow next to **Size**. Labels: `Labels`, lines 37 to 39;
  `Vue/components/FileItemsComponent.vue`, lines 12 to 23; the second click reverses the order,
  `Vue/services/EventBusService.ts`, `changeSort`.
- `find-4`: `ClearHighlightsAsync()`, then `ClickAsync(scene.Page.Locator(".action-bar a[title='Grid view']"))`. Wait until
  the thumbnails that are in view are loaded, with a time limit, because the thumbnails have `loading="lazy"` and one
  below the fold never loads: `WaitForFunctionAsync("() => { const shown = [...document.querySelectorAll('.file-items-grid
  img')].filter(i => { const r = i.getBoundingClientRect(); return r.bottom > 0 && r.top < window.innerHeight; }); return
  shown.length > 0 && shown.every(i => i.complete && i.naturalWidth > 0); }", null, new PageWaitForFunctionOptions {
  Timeout = 10000 })`. Highlight the button with the caption "Grid view", hold 3 seconds, then click the same button
  again (its title is now **Table view**, `.action-bar a[title='Table view']`). On screen at the end: the list view
  again, 10 rows sorted by size from the largest (`photo-12.jpg` to `photo-03.jpg`), with the filter box empty. While
  the grid is shown: ten thumbnails in the same order, one page of the two (the page size is still 10). Labels: **Grid
  view** and **Table view** are written in `Vue/App.vue`, line 112 (not in `Labels`); the toggle is drawn only when
  thumbnails are enabled. Other file types show an icon with their extension in the grid (`Vue/App.vue`, lines 148 to
  158); all the files of this folder are images, so the icon is not shown.
- `find-5`: `ClearHighlightsAsync()`, `ClickAsync(scene.Page.Locator("a.settings-btn"))`. The settings popover opens with
  **Items per page** (a list with 10, 20, 50, 100) and **Thumbnail size** (buttons **Normal** and **Large**). Highlight
  `a.settings-btn` with the caption "Settings", then `SelectAsync(scene.Page.Locator("#settings-page-size"), "20")`. On
  screen at the end: the open popover with both rows, and the list with the twelve photos (`photo-12.jpg` down to
  `photo-01.jpg`, because the Size sort of `find-3` is still on), all on one page because 20 files fit on a page. Do not
  click **Large**: it only changes the grid, which is not shown again. Labels: **Items per page**, **Thumbnail size**,
  **Normal**, **Large** are not in `Labels`; they are written in `Vue/App.vue`, lines 61, 70, 76 and 82 (the gear
  button's title **Settings**, line 54). The default page size is 10 (`Vue/services/LocalStorage.ts`, line 10).
- `find-6`: `ClearHighlightsAsync()`, `ClickAsync(scene.Page.Locator("a.storage-info-btn"))` (this click also closes the
  settings popover of `find-5`, `Vue/App.vue`, lines 470 to 473). On screen at the end: the popover under the drive
  button, showing **Available Storage** and a size (the free space of the disk that holds the site's `App_Data`). Highlight `a.storage-info-btn` with the caption "Storage Info". Labels: **Available Storage**
  `Labels`, line 110, shown by `Vue/App.vue`, lines 44 and 45; the value comes from
  `M/Endpoints/Api/GetPermittedStorageEndpoint.cs` and the file store (`FileSystemStore.GetPermittedStorageAsync`); the
  media store passes it through `IMediaEventHandler.MediaPermittedStorageAsync` (`OrchardCore.Media.Core/DefaultMediaFileStore.cs`,
  lines 239 to 250), and no handler of the source changes it. The endpoint needs Manage Media and Manage All Media
  Folders, which the admin has; a user with fewer permissions would see "Unavailable" (`Vue/App.vue`, lines 48 to 50).

## Clip `07-reuse`

**Starts on** `/Admin/Contents/ContentTypes/Article/Create` (the Create page of the Article content type, route
`OC/src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs`, line 407), state S3. Before the capture,
scroll the page until the field **Banner Image** and its button **Add media** are in view, and leave the pointer
somewhere neutral. Do not type anything, and never save: leave the page without clicking a publish or save button.

- `reuse-1`: highlight `div.media-field-app` with the caption "Banner Image" for about 4 seconds, then
  `ClearHighlightsAsync()` and `ClickAsync(scene.Page.Locator("div.media-field-app button:has-text('Add media')"))` at the
  end of the step. Wait for the dialog (`.mf-picker-media-gallery .file-items-table`). On screen at the end: the dialog
  opening over the editor. Labels: **Banner Image** is the display name of the field in the Blog recipe
  (`blog.recipe.json`, lines 690 to 697, part `Article`); **Add media**: `Labels`, line 119, drawn by
  `Picker/MediaFieldBasic.vue`, line 15; the field's markup is `M/Views/MediaField.Edit.cshtml` (class
  `media-field-app`).
- `reuse-2`: in the dialog titled **Select Media** (`Labels`, line 137; `Picker/MediaPickerModal.vue`, line 18), click
  `.mf-picker-media-gallery #folder-tree li.folder:has-text('Harvest Festival') .folder-name`, then tick the row
  `harvest-table.jpg` (`.mf-picker-media-gallery .table-row.file-item:has-text("harvest-table.jpg")
  input[name='select-file']`). The footer shows "1 selected" and enables **OK**. Highlight
  `.mf-picker-media-gallery label.upload-button` with the caption "Upload files" before choosing the folder. About 2
  seconds before the end of the narration, click `.mf-modal-footer button.mf-btn-primary` (**OK**, `Labels`, line 126;
  `Picker/MediaPickerModal.vue`, lines 44 to 49). On screen at the end: the dialog closing, or closed, with the
  editor behind it. The dialog holds the same tree, **Upload files** button and list as the library
  (`Picker/MediaPickerModal.vue`, which mounts the gallery with `mountMediaAppAsPicker`).
- `reuse-3`: `ClearHighlightsAsync()`, then wait for the thumbnail (`div.media-field-app img`, loaded). Highlight
  `div.media-field-app` with the caption "Banner Image". On screen at the end: the field holding the picture of
  `harvest-table.jpg`, with the toolbar buttons **Add media** (disabled, because the field takes one file),
  **Remove** and **Anchor** (shown because the recipe sets `AllowAnchors` for the field and the file is an image:
  `Picker/MediaFieldBasic.vue`, lines 8 to 46 and 187 to 195; the narration does not name them). The path is stored in the field's hidden input
  (`Picker/MediaFieldBasic.vue`, line 4, the input holding `serializedPaths`; `OC/src/OrchardCore/OrchardCore.Media.Core/Fields/MediaField.cs`,
  property `Paths`). Label **Remove**: `Labels`, line 120.

## Checks for private data

Before recording, check:

- the site is a fresh Auto Setup site with the fake data above; no real name, email, phone number, address or token is
  in the library, in the Article editor, or in the admin screens. The only user is `admin` with `admin@example.com`;
- the file names on screen are `about-bg.jpg`, `home-bg.jpg`, `post-bg.jpg` (theme pictures), `draft-photo-1.jpg`,
  `draft-photo-2.jpg`, `old-flyer.jpg`, `photo-01.jpg` to `photo-12.jpg`, `apple-harvest.jpg`, `cider-menu.jpg` and
  `harvest-table.jpg`, and the folders **Harvest Festival**, **Old Drafts** and **Orchard Photos**, plus the system
  folder **_Users**, which stays collapsed (its child folder is named by the admin user's generated id);
- the **Available Storage** popover of `find-6` shows only a size, no path or drive name;
- the password is never typed on camera and never written in this repository: `VIDEO_PASSWORD` is set in the shell
  only.

After recording, look at every contact sheet of `review.py` (the frame at the end of each step must show what is
described above, with no error message, and no empty list unless the step says so). Then run the private-data scan of
the skill (OCR at one frame per second and at every scene change) and search the text for real names, email addresses
other than `admin@example.com`, phone numbers and tokens.

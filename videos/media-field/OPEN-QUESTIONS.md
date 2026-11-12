# Open questions: Media Field

What was checked in the source, and what a person must confirm on a running site before or while recording. The
narration only states what the source shows; each item below is something the source cannot settle (how a screen
looks, how a headless browser behaves), or a point where the documentation and the code differ.

## To confirm on the running site

1. **The Photos images appear on the Page.** The narration of `attach-9` says the page shows the images. From the code:
   a Page has no template of its own, so the Blog theme's `Content.liquid` renders all of its shapes
   (`src/OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:17`), and the field's display view draws an `img`
   for an image file (`src/OrchardCore.Modules/OrchardCore.Media/Views/MediaField.cshtml:16-19`). Confirm that the
   theme shows the field and its images in the page body. If it does not, record the front-end steps on a Page of
   another theme, or drop `attach-9` and `attached-5`. Two more points of the same screens: the display view writes the
   name of the field (**Photos**, **Visitor Guide**) before its files (`MediaField.cshtml:10`), so it is on screen; and
   the images are plain `img` tags of 1600 x 900 pixels without a size, so whether the theme's style sheet scales them
   to the width of the page, or shows them at full size, was not read. Check the frame of `attach-9` and `attached-5`
   and scroll or crop the capture accordingly.
2. **The alternative text is not on screen.** `attach-9` says the media text of an image becomes its alternative text.
   The view does it (`MediaField.cshtml:18-19`), but the page shows nothing of it. After recording, read the page
   source: the first image should have the alternative text `Spring orchard`, the second an empty one.
3. **The Technical Name fills in** when the Display Name is typed (`add-3`, `attached-1`). Read from
   `src/OrchardCore.Modules/OrchardCore.ContentTypes/Assets/js/autocomplete.ts`; confirm it on the page, and that
   the field type radio button reads **Media Field**.
4. **Admin menu locators.** The markup of the admin menu (**Design**, **Content Definition**, **Content Types**,
   **Content**, **Content Items**) was read in `src/OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml` and
   `NavigationItemText.cshtml`: each entry is an `.item-label` inside `#left-nav` with a `span.title`, and a group
   without a page of its own is a button. The plan uses `#left-nav` with an exact text match; confirm on the running
   site that each text matches one visible element (use `Recorder probe`). The Blog
   recipe adds a menu of its own (`blog.recipe.json:1188-1306`) with a **Content** group, a **Content Items** link and a
   link per content type (**Article**, **Page**, **Taxonomy**); the admin merges items with the same text
   (`NavigationManager.cs:74-93`), so **Content** and **Content Items** should each appear once, with **Page** as an
   extra entry of **Content**. Confirm that on the running site; if **Content Items** appears twice, scope the locator
   to the **Content** group. The **Page** link of the **New** menu of the Content Items list is scoped to
   `.dropdown-menu` for that reason (`attach-1`).
5. **The library folder gesture.** The documentation says to right-click a folder to create a subfolder
   (`src/docs/reference/modules/Media/MediaGallery.md`, "Folder Management"). The code has no right-click handler in
   the gallery; each folder of the tree has an ellipsis button, titled "Folder Action", with the menu entry
   **Create a subfolder** (`.../media-gallery/src/components/FolderRow.vue:26`, `FolderMenu.vue:2-4`). Seed the folder
   with the ellipsis menu and confirm it works; it happens outside the capture.
6. **The dialog opens at the root, in the list view.** From `LocalStorage.ts`: the grid choice is off by default and the
   open folder is remembered in the browser's local storage. Confirm that a fresh browser context shows the root folder
   of the library, with a check box in front of each file name. If the dialog opens in the grid, click the toggle in
   its toolbar before `attach-3`.
7. **Switching Photos from Standard to Gallery keeps its files** (`gallery-1`, `gallery-2`). Both editors read and
   write the same list of paths (`MediaField-Gallery.Edit.cshtml:46`, `MediaField.Edit.cshtml:46`), so the two files
   chosen earlier should appear as cards. Confirm it. Do **not** switch a field that holds files to **Attached**: the
   files stay at their library paths until the `move-attached-media-fields` recipe step moves them
   (`src/docs/reference/modules/Media/README.md`, "Recipe step: move-attached-media-fields").
8. **Dragging cards reorders them** (`gallery-3`). The cards use the browser's native drag and drop
   (`GalleryContainer.vue:189-210`); `DragToAsync` is documented to drive it. If it does nothing, the step can drop the
   drag and the narration of `gallery-3` must then lose its second sentence.
9. **Headless upload for the Attached editor** (`attached-3`). The **Add media** button of the Attached editor is a label
   of a hidden file input (`MediaFieldAttached.vue:9-25`); a click would open the system file dialog, which a headless
   browser does not draw. The plan sets the file on the input with `SetInputFilesAsync`. Confirm that the thumbnail
   `visitor-guide.pdf` appears, that the upload message completes, and that **Publish** then moves the file (the link
   of `attached-5` starts with `mediafields/Page/`).
10. **Administrators have the permission of `attached-6`.** From the code: the Administrator role is given
    `ManageMediaFolder`, which implies `Manage Attached Media Fields Folder`
    (`src/OrchardCore.Modules/OrchardCore.Media/PermissionProvider.cs:34`,
    `src/OrchardCore/OrchardCore.Media.Core/MediaPermissions.cs:14`). Confirm in the role editor of an administrator,
    and that a role such as Editor does not have it by default (the Editor role is given `ManageMedia`, not the attached
    folder permission, `PermissionProvider.cs:43-50`).
11. **The masthead of the Page.** Both front-end steps (`attach-9`, `attached-5`) show the Page with the picture
    `post-bg.jpg` as its masthead (`Content.liquid:4`). It shows an astronaut in a spacesuit working outside a
    spacecraft: no face, name or brand is legible. It is the theme's own asset and the plan lets it stay on screen. If a
    reviewer does not want it, the alternative is a Page type displayed by another theme, which changes the plan.
12. **Notification texts.** The messages are read from the controllers: `The field "Photos" has been added.`,
    `The "Photos" field settings have been saved.`, `Your Page has been published.` Confirm the wording on screen, and
    adjust the "On screen" lines of the plan if a message differs.
13. **Pronunciation of the two spoken entries.** The storyboard maps `.pdf` (`attached-2`) to `dot P.D.F.` and
    `Save & Configure` (`add-3`, `attached-1`) to `Save and Configure`. Check both with `scripts/pronounce.py` before
    narrating; the subtitles keep the written form.

## Where the documentation and the code differ

These are also reported to the programme as documentation issues; the video follows the code.

| Page | The documentation says | The code does |
| --- | --- | --- |
| `topics/media/media-picker.md` | A content type is edited from `Content → Content Definition` | The menu is **Design**, **Content Definition**, **Content Types** (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`) |
| `topics/media/media-picker.md` | The field setting is called `Media types` | The settings are **Allow all default media types**, and a check box per extension, grouped as `image types`, `application types`, and so on (`MediaFieldSettings.Edit.cshtml:50-86`) |
| `reference/modules/Media/MediaGallery.md` | The Standard editor has "no direct upload" from the field | The dialog embeds the whole library, including **Upload files** (`MediaPickerModal.vue`, `App.vue:93-97`, `Permissions.ts`: `canManage` is always true) |
| `reference/modules/Media/MediaGallery.md` | Attached files move to their final location "when the content item is published" | They move whenever the editor form is saved, **Publish** or a draft, except in a preview (`MediaFieldDisplayDriver.cs:105-111`, `AttachedMediaFieldFileService.cs:135-148`) |
| `reference/modules/Media/MediaGallery.md` | A subfolder is created, and a folder deleted, by right-clicking the folder in the tree | The gallery has no right-click handler; each folder has an ellipsis button titled "Folder Action" with the menu (`FolderRow.vue:26`, `FolderMenu.vue:2-4`, under `OrchardCore.Media/Assets/media-gallery/src/components`) |
| `reference/modules/Media/MediaGallery.md` | The file list can be sorted by name, size, MIME type or last modified date | The list view has three clickable headers, **Name**, **Last modified** and **Size**; no header sorts by MIME type (`FileItemsComponent.vue:12-25`, under `OrchardCore.Media/Assets/media-gallery/src/components`) |

# Recording plan: Media overview

How to record `media-overview` without deciding anything. Narrate first (`narrate.py`), then record the three clips
(`02-where`, `03-library`, `04-content`). The video has 22 narrated steps; the clips hold 17 of them, the slides 3, the
cards 2. Labels are checked against the Orchard Core source at commit 92db4a83; paths below are relative to the root of
`OrchardCMS/OrchardCore`. A selector in this plan stands for `scene.Page.Locator(...)`.

Nothing in this video is saved to the site: no file is uploaded, no folder is created, no feature is enabled, no content
item is saved. Every selector below is read from the source and must be confirmed on the running site before the take
(`Recorder probe`); if a selector fails, find the element by its visible text and write the working selector back here.

## The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore` at the commit above or a descendant that keeps the labels of
  this plan; `src/OrchardCore.Cms.Web`, run on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty
  folder outside both repositories.
- **Auto Setup** (see `references/recording.md` and the `orchardcore-tester` skill), so no setup or sign-in form is
  recorded. Tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, database `Sqlite`, recipe
  **`Blog`**, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git and passed to the
  recorder as `VIDEO_PASSWORD`; never written in a file of the repository). The environment variables are
  `OrchardCore__AutoSetup__Tenants__0__ShellName`, `__SiteName`, `__SiteTimeZone`, `__AdminUsername`, `__AdminEmail`,
  `__AdminPassword`, `__DatabaseProvider`, `__RecipeName` (`docs/reference/modules/AutoSetup/README.md`).
- **Block outbound calls**: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Features to enable before recording**: none. The Blog recipe enables **Media** (`OrchardCore.Media`,
  `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:65`) and none of the other Media features. Do **not**
  enable any other feature: `where-4` to `where-6` show them as Disabled.
- **Settings**: none to save. Leave every setting as Auto Setup creates it.
- **External services**: none, and no local stand-in.
- **Seed data**: none to create. Everything the video shows comes from the Blog recipe:
  - three files in the root folder of the library: `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg`
    (`blog.recipe.json:1124-1137`, the `media` step, with no folder in the target paths);
  - one media profile named `banner`, hint "A banner image (2048px x 600px, cropped)" (`blog.recipe.json:1111-1121`);
  - the Article content type, whose Image field is labeled **Banner Image** (`blog.recipe.json:695`), and an Article
    titled **About** at the path `about` (`blog.recipe.json:1068`) whose header the theme draws from `about-bg.jpg`
    (`src/OrchardCore.Themes/TheBlogTheme/Views/Content-Article.liquid:4-9`).
- **The user's own folder**: the first time the Library loads, the site creates the folder `_Users` with a folder named by
  the signed-in user's id inside it (`OrchardCore.Media/Endpoints/Api/GetDirectoryTreeEndpoint.cs:47-51`,
  `Services/MediaOptionsConfiguration.cs:65`). Load `/Admin/Media` once outside the capture, and only then copy `App_Data` aside as the
  snapshot, so that every take, also after a restore, shows the same tree. The `_Users` folder and the id are generated, hold no file and no real data, and are
  expected in the frames of `03-library`. Never open, rename or delete them.
- **Browser state**: the gallery keeps its view, page size and selected folder in the browser's local storage. The admin
  menu uses its default behavior, Focused (`AdminSettings.MenuBehavior`, `OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:29`;
  the Blog recipe does not change it): when a page loads only the group that holds the current page is open, and opening a
  group closes the other groups of the same level (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts:18` and `:168`).
  A Focused menu stores nothing about open groups. At the start of every clip, before `StartCaptureAsync`, run
  `scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }")`, then reload the page
  (`scene.Page.ReloadAsync()`) and wait for the page to be ready. The sign-in is a cookie, so it survives.
- **Viewport and theme**: the recorder's defaults (1536x864, light, en-US). Leave the admin theme and menu as the page
  loads; do not switch the theme.

## Order of the clips

The three clips are independent and change nothing, so they can be recorded in any order and again at will. Take one snapshot of
`App_Data` (and `state.json`) after Auto Setup **and after the first load of the Library**, so that the restored site
already holds the `_Users` folder; restore it only if a take went wrong. The only state a clip leaves behind is
the browser's local storage (cleared at the start of the next clip) and, after the first load of the Library, the
`_Users` folder described above. `04-content` ends on an unsaved Article form that is simply left (closing the scene
discards it); never click **Save Draft** or **Publish**.

## Slides (`01-why`)

Drawn by `slides.py`; nothing to record.

- `why-1`: heading "Without a shared place for files" and three cards, all bright: The same picture, again; Copies drift
  apart; Files get lost.
- `why-2`: heading "One library for the files of the site" and two cards, both bright: One library; Attached to content.
- `why-3`: heading "From upload to page" and three numbered cards, all bright: Upload, Attach, Show.

## Clip `02-where`

**Starts on** `/Admin/Contents/ContentItems` (the Content Items list), signed in as `admin` (local storage cleared and the
page reloaded as above). The menu is Focused (see "Browser state"), so the only group open is the one that holds this page,
the **Content** group of the Blog recipe's custom menu (or whichever group the menu marks as holding the page); **Media**,
**Settings** and **Tools** are closed. Wait for `#left-nav`. Start the capture.

In this clip the menu groups are opened with a click on camera. A group with children and no page of its own is a button
(`button.item-label.nav-group-label` at the first level, `button.item-label` below it); a page is a link
(`a.item-label`). Source: `src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24-33` and `:73`,
`NavigationItem-admin.cshtml:92` (the `ul` of a group has `data-title` set to the group's label, and the class `nav-group-items` at the first level, `:97`).
The Blog recipe adds three custom first-level items to the menu (**Blog**, **Main Menu**, **Content** with a nested
**Content Items**; `blog.recipe.json:1197-1253`); none is labeled Media, Settings or Tools, so the exact-text selectors
below match once. Always use `text-is` (exact) and not `has-text` (substring) for the label of a group.

- `where-1` (narration about 8 s).
  - Actions: move to and click the first-level button of the group **Media**:
    `#left-nav button.nav-group-label:has(span.title:text-is("Media"))`. Wait until the link `#left-nav ul.nav-group-items[data-title="Media"]
    a.item-label:has-text("Library")` is visible. Draw a highlight ring around the **Library** link, then around the
    **Profiles** link (`a.item-label:has-text("Profiles")` in the same `ul`), one after the other, keeping the actions
    under 6 s. Do not click either link.
  - On screen when the narration ends: the Media group expanded in the left menu (the Focused menu has closed the group that
    held the page), with **Library** and **Profiles** visible, and a highlight ring on **Profiles**. The page behind it is
    still the Content Items list.
  - Labels: `OrchardCore.Media/AdminMenu.cs:18` (Media), `:21` (Library), `:26` (Profiles).

- `where-2` (about 9 s).
  - Actions: click **Profiles** with `ClickAndWaitForNavigationAsync`
    (`#left-nav ul.nav-group-items[data-title="Media"] a.item-label:has-text("Profiles")`). Wait for the row of the profile `banner`.
    Draw a highlight ring around that row. Do not click **Edit** or **Delete**.
  - On screen: the page **Media Profiles** (breadcrumb at the top), one row named `banner` with the hint "A banner image
    (2048px x 600px, cropped)" and the buttons **Edit** and **Delete**, ringed. The search box and **Add Media Profile**
    are above the row.
  - Labels: `OrchardCore.Media/Views/MediaProfiles/Index.cshtml:6` (Media Profiles), `:25` (Add Media Profile),
    `Views/MediaProfileEntry.Buttons.SummaryAdmin.cshtml:6-8` (Edit, Delete); the profile is `blog.recipe.json:1111-1121`;
    the page is `[Admin("MediaProfiles", ...)]`, `Controllers/MediaProfilesController.cs:58`.

- `where-3` (about 16 s).
  - Actions: click the first-level button **Settings** (`#left-nav button.nav-group-label:has(span.title:text-is("Settings"))`); then,
    inside it, the button **Media** (`#left-nav ul.nav-group-items[data-title="Settings"] button.item-label:has(span.title:text-is("Media"))`).
    Opening **Settings** closes the first-level **Media** group that `where-2` left open (Focused menu), but the page
    still has two elements labeled **Media**, one at the first level and one inside **Settings**: always scope the
    selector to `ul.nav-group-items[data-title="Settings"]`. Wait until the links **Options** and **API**
    (`ul.nav-group-items[data-title="Settings"] ul[data-title="Media"] a.item-label`) are visible, scrolling the menu if they are below the
    fold (`ScrollIntoViewIfNeededAsync` on the **API** link). Draw a highlight ring around **Options** while the narration
    says "Options" and around **API** while it says "API". Do **not** click **Options** or **API**: the Options page
    contains the notice "Media is configured with appsettings.json.", which an admin video does not show.
  - On screen: the Settings group expanded with its **Media** subgroup open, **Options** and **API** both visible, and a
    ring on **API**.
  - Labels: `OrchardCore.Settings/AdminMenu.cs:36` (Settings), `OrchardCore.Media/AdminMenu.cs:35` (Media), `:36` (Options),
    `:41` (API). The documentation page `MediaGallery.md` says "Configuration -> Settings -> Media API" and calls the setting
    "Authentication scheme"; the code has no Configuration menu and labels the setting "Media API authentication"
    (`Views/MediaApiSettings.Edit.cshtml:6`), so the code wins.

- `where-4` (about 11 s).
  - Actions: clear highlights. Click the first-level button **Tools** (`#left-nav button.nav-group-label:has(span.title:text-is("Tools"))`),
    then click the link **Features** (`#left-nav ul.nav-group-items[data-title="Tools"] a.item-label:has-text("Features")`) with
    `ClickAndWaitForNavigationAsync`. On the page, type `media` into the search box (`TypeAsync("#search-box", "media")`)
    and wait until `#list-summary` has changed from its first text. The list filters while you type (an `input` listener,
    `OrchardCore.Features/Assets/ts/features-index.ts:149`); Enter does nothing there, so do not press it. Scroll to the top. Draw a highlight ring around the
    status badge **Enabled** of the **Media** row
    (`[data-filter-value^="Media OrchardCore.Media "]`, the badge is `span.badge` inside it).
  - On screen: the page **Features**, the text `media` in the search box, the filtered list under category headings, and the
    **Media** row with its **Enabled** badge ringed. The list also shows a row that mentions the media library in its
    description (**Data Pipelines**); the narration does not talk about it, and no count is shown or read.
  - Labels: `OrchardCore.Features/AdminMenu.cs:26` (Tools), `:27` (Features); `Views/Admin/Features.cshtml:27` (Search);
    `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` (Enabled), `:15` (Disabled); the search matches the name, the
    id and the description (`Drivers/FeatureEntryDisplayDriver.cs:20`, `Assets/ts/features-index.ts`).

- `where-5` (about 13 s).
  - Actions: clear highlights; bring the row **Media Slugify** into view and ring it
    (`[data-filter-value^="Media Slugify "]`); about 9 s later (the end of the first sentence) clear the ring, bring the row
    **Secure Media** into view and ring it (`[data-filter-value^="Secure Media "]`). Highlights are fixed-position: clear them before every scroll. If the
    two rows fit on one screen, keep both rings for the end of the step. Do not click **Enable**: enabling Secure Media
    changes who can see media files.
  - On screen: the **Secure Media** row (and **Media Slugify** if it fits) with a ring, status **Disabled**, and the
    description of each row visible.
  - Labels: `OrchardCore.Media/Manifest.cs` (features **Media Slugify** and **Secure Media** and their descriptions).

- `where-6` (about 17 s).
  - Actions: clear highlights; ring in turn the rows **Media TUS Uploads** (`[data-filter-value^="Media TUS Uploads "]`),
    **Media SignalR** (`[data-filter-value^="Media SignalR "]`), then scroll down to the category **Hosting** and ring
    **Azure Media Storage** (`[data-filter-value^="Azure Media Storage "]`) and **Amazon Media Storage**
    (`[data-filter-value^="Amazon Media Storage "]`). The first sentence (about 7 s) describes the first two features: spend
    about 3 s on **Media TUS Uploads** and about 4 s on **Media SignalR**. The second sentence (about 10 s) names the last
    two: scroll to **Hosting** at its start and keep both rings until the end of the step. The narration does not say the
    names TUS or SignalR; the rings show them. Do not click **Enable**.
  - On screen: the **Hosting** category with the rows **Azure Media Storage** and **Amazon Media Storage** ringed, both
    **Disabled**.
  - Labels: `OrchardCore.Media/Manifest.cs` (Media TUS Uploads, Media SignalR);
    `OrchardCore.Media.Azure/Manifest.cs` (Azure Media Storage); `OrchardCore.Media.AmazonS3/Manifest.cs` (Amazon Media
    Storage).

## Clip `03-library`

**Starts on** `/Admin/Media` (`OrchardCore.Media/Controllers/AdminController.cs:43`), signed in as `admin`. Clear local
storage and reload as above (the table view, 10 items per page and the root folder are then the defaults). Wait until the
loading spinner (`.file-loading-overlay`) is hidden and `div.table-row.file-item` has three rows. Start the capture.
Locator note: the gallery's Tailwind classes contain a colon (`tw:flex`); do not use them in selectors.

- `library-1` (about 8 s).
  - Actions: move the cursor over the root row of the folder tree and draw a highlight ring around the whole tree
    (`#folder-tree`).
  - On screen: the Library page; on the left the folder tree whose first row is the root folder **Files**, with the
    folder `_Users` below it (expected, see "The user's own folder"); on the right the table with three files.
  - Labels: root name **Files**: `OrchardCore.Media/Services/MediaJSLocalizer.cs:31` (`FileLibrary`),
    `Assets/media-gallery/src/services/HierarchicalTreeBuilder.ts:246`; the server sends an empty root name
    (`Services/MediaDirectoryTreeCache.cs:74`). Do not narrate or show a heading "Assets": the page title is hidden
    (`Views/Admin/Index.cshtml:9`).

- `library-2` (about 10 s).
  - Actions: clear highlights; draw a highlight ring around the header row of the table (`div.header-row`), then move the
    cursor down the three rows.
  - On screen: the table with the headers **Name**, **Last modified**, **Size** and the rows `about-bg.jpg`,
    `home-bg.jpg`, `post-bg.jpg` (sorted by name), the header row ringed.
  - Labels: `MediaJSLocalizer.cs:37-39`; `Assets/media-gallery/src/components/FileItemsComponent.vue:9-19`.

- `library-3` (about 12 s).
  - Actions: clear highlights; move the cursor along the toolbar above the table to the button that shows a grid icon
    (`.action-bar a:has(svg[data-icon="grip"])`) and draw a highlight ring around it while the first sentence is spoken
    (about 5 s); the delete, download and grid buttons have no visible text, and their titles are native tooltips that
    must not be relied on. Then clear the ring and click the button. The toolbar has four controls on the left:
    **Upload files**, a delete button, a download button and this one. Wait until
    `ol.file-items-grid img` shows all three thumbnails (`img.complete`).
  - On screen: a grid of three thumbnails with the file names under them; the toolbar button shows a list icon.
  - Labels: `Assets/media-gallery/src/App.vue:110-116`. The toolbar order is **Upload files**, delete, download, view
    toggle (`App.vue:93-116`); other controls (the search box and its clear button, the storage and settings buttons) sit to
    the right and above. The narration describes the button by its grid icon, since the buttons before it show only icons
    (their titles are "Delete" and "Download", `MediaJSLocalizer.cs:28-29`, and the fallback title of this one is "Grid
    view", `App.vue:112`).

- `library-4` (about 9 s).
  - Actions: click the same button again (`.action-bar a:has(svg[data-icon="list"])`) and wait for the table; then
    `TypeAsync("#file-filter-input", "post")`.
  - On screen: the table with the single row `post-bg.jpg`, the text `post` in the search box (placeholder when empty:
    "Search and filter...").
  - Labels: `MediaJSLocalizer.cs:30`; the filter is a case-insensitive "contains" on the file name
    (`Assets/media-gallery/src/composables/useFileListFiltering.ts:7-13`).

- `library-5` (about 12 s).
  - Actions: click the clear button of the search box (`#clear-file-filter-button`) and wait for three rows; move the
    cursor to the **Upload files** button (`label.upload-button`) and draw a highlight ring around it. **Do not click it**:
    a click opens the system file chooser, and nothing may be uploaded in this video.
  - On screen: the table with three rows again, an empty search box, and the **Upload files** button ringed.
  - Labels: `App.vue:93-97`, `MediaJSLocalizer.cs:27` (`UploadFiles`); drop zone text "Drop your file here" (`:25`) is not
    shown (it appears only while files are dragged over the page); client-side checks of the extension and the size:
    `docs/reference/modules/Media/MediaGallery.md` (Upload Behavior) and `Assets/media-gallery/src/services/UppyFileUpload.ts`.

- `library-6` (about 6 s).
  - Actions: clear highlights; click the row menu button at the end of the first row (`div.table-row.file-item >> nth=0 >>
    a.action-button`, an ellipsis icon). Wait for the popup menu (`[id="overlay_file_menu"]:visible`). Do not click an item.
  - On screen: the popup open next to the first row with the six items **Rename**, **Move**, **Copy**, **View**,
    **Download**, **Delete**.
  - Labels: `Assets/media-gallery/src/components/FileMenu.vue:60-98`, `MediaJSLocalizer.cs:41-49`.

- `library-7` (about 10 s).
  - Actions: move the cursor over the root row of the tree and click its menu button (`#folder-tree li.folder >> nth=0 >>
    .folder-actions a.action-button`, tooltip "Folder Action"). The open file menu closes by itself
    (`FolderMenu.vue` emits `CloseFileMenus`). Do not click **Create a subfolder**.
  - On screen: the popup of the root folder with the single item **Create a subfolder** (the root has no **Delete**), and
    the table behind it.
  - Labels: `Assets/media-gallery/src/components/FolderMenu.vue:66-80` (**Create a subfolder**, **Delete** only when the
    folder is not the root, `:47-50` and `:74`), `MediaJSLocalizer.cs:62-64`.

## Clip `04-content`

**Starts on** `/Admin/Contents/ContentTypes/Article/Create` (`OrchardCore.Contents/Controllers/AdminController.cs:407`),
the empty form of the content type Article, signed in as `admin`. Clear local storage and reload as above (the gallery
inside the picker then starts on the root folder in table view). Wait until `.media-field-app button` with the text "Add
media" is visible. Start the capture. The form is never saved.

- `content-1` (about 9 s).
  - Actions: scroll until the Banner Image field is in the middle of the viewport
    (`.field-wrapper-article-image`) and draw a highlight ring around it.
  - On screen: the label **Banner Image**, the empty area with the text "No Files", and the button **Add media**, all inside
    the ring.
  - Labels: **Banner Image** is the display name of the Article's Image field (`blog.recipe.json:695`); the editor class
    comes from `OrchardCore.Media/Views/MediaField.Edit.cshtml` (`field-wrapper-{part}-{field}`); "No Files":
    `MediaJSLocalizer.cs:118`.

- `content-2` (about 6 s).
  - Actions: clear highlights; click **Add media** (`.media-field-app button:has-text("Add media")`). Wait for the dialog
    (`.mf-picker-shell`) and for the three rows inside it (`.mf-picker-media-gallery div.table-row.file-item`). The button is
    only enabled while the field is empty (`Assets/media-picker/src/components/MediaFieldBasic.vue:187-189`), which is why
    this clip uses the empty form and not the **About** article, whose field already holds a file.
  - On screen: the dialog titled **Select Media** over the form, with the folder tree and the table of the library inside it.
  - Labels: `MediaFieldBasic.vue:15`, `MediaJSLocalizer.cs:119` (Add media), `:137` (Select Media),
    `Assets/media-picker/src/components/MediaPickerModal.vue:18`.

- `content-3` (about 12 s).
  - Actions: tick the checkbox of the row `about-bg.jpg` in the dialog
    (`.mf-picker-media-gallery div.table-row.file-item:has-text("about-bg.jpg") input[name="select-file"]`); the footer then
    reads "1 selected". Click the confirm button of the footer (`.mf-modal-footer button.mf-btn-primary`, enabled once a file
    is selected). Wait until the dialog is gone and a thumbnail is shown (`li.mf-thumb-item img`).
  - On screen: the dialog closed; the field shows the thumbnail of `about-bg.jpg` with its name under it; **Add media** is
    disabled (a field for a single file that already holds one).
  - Labels: `MediaPickerModal.vue:38-52`; the footer button reads "OK" or "Ok" (see OPEN-QUESTIONS.md); the narration does
    not name it.

- `content-4` (about 14 s).
  - Actions: `NavigateAsync("/about")` (the form is discarded; no source of the content editor registers a `beforeunload`
    handler, so no unsaved-changes prompt is expected, but confirm it on the first take: OPEN-QUESTIONS.md).
    Wait for `header.masthead`, then wait 1 s so that the header's background image has loaded (it is a CSS background,
    so there is no `img` to wait for). Do not scroll.
  - On screen: the site's About page: the header with its background image across the top, the title "About" and the
    subheading "This is what I do.", with the page text starting below.
  - Source: the route is the Autoroute path `about` (`blog.recipe.json:1068`); the About article stores `about-bg.jpg` in
    its `Article.Image` field (`blog.recipe.json:1083`), the file that `content-3` picks, which is why the narration says
    "this same file"; the theme requests the image through the
    profile `banner` (`TheBlogTheme/Views/Content-Article.liquid:9`); the profile crops (`ResizeMode.Crop`,
    `OrchardCore.Media/Controllers/MediaProfilesController.cs:364`; mode 2 in `blog.recipe.json:1117`, the third member of `OrchardCore.Media.Abstractions/Processing/ResizeMode.cs`: Undefined, Max, Crop). The narration says
    only "cropped to a wide banner": the source image is 1900 px wide and the engine never enlarges an image
    (`Processing/VipsImageProcessingEngine.cs:111-123`, `Size.Down`), so the header image is not necessarily 2048 px wide.

## Checks for private data

Before assembling, and again on the finished video (OCR at one frame per second and at every scene change, then at five
frames per second):

- The only names are `admin`, the site name "Orchard Core" and the text of the Blog recipe (lorem ipsum, "About"). The only
  email address allowed on screen is `admin@example.com`; any other email address is leftover data.
- Look at `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg` in the grid (`library-3`), in the picker (`content-3`) and in the
  header (`content-4`): none may show an identifiable person, and a person decides whether the trademarks of `about-bg.jpg` are acceptable (see
  OPEN-QUESTIONS.md).
- The tree shows `_Users` and a generated user id: neither is private, but a different folder name or a file the take did not
  create is leftover data from a copied site: restore the snapshot taken after Auto Setup.
- The "Last modified" column shows the date of the setup: acceptable.
- No password, token, connection string or absolute path of this machine on screen. `VIDEO_PASSWORD` is never typed on
  camera (the sign-in is `state.json`, git-ignored).
- The browser's address bar is not recorded; the page URLs above are for the recorder only.

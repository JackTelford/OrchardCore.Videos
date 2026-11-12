# Recording plan: Media Field

How to record `media-field` without having to decide anything. Read it top to bottom once, then follow the clips in
order. The narration is in `storyboard.json`; this file says what is on screen while each step is spoken.

**Source paths.** A label or behavior is followed by `file:line` in `OrchardCMS/OrchardCore` at commit `92db4a83`
(the commit this plan was checked against). Three prefixes are shortened:

| Short | Path |
| --- | --- |
| `[Media]` | `src/OrchardCore.Modules/OrchardCore.Media` |
| `[Types]` | `src/OrchardCore.Modules/OrchardCore.ContentTypes` |
| `[Contents]` | `src/OrchardCore.Modules/OrchardCore.Contents` |

In this file a bare selector stands for `scene.Page.Locator(...)`. The picker, the field editors and the media library
are Vue apps: always use locators, never element handles (the apps redraw).

## 1. The site it needs

This video is recorded on a **fresh site**, with Auto Setup, so no setup or sign-in form is captured. It does not use a
copy of any real site, so it needs no mask script.

| | |
| --- | --- |
| Orchard Core | A checkout of `OrchardCMS/OrchardCore`, run with `src/OrchardCore.Cms.Web` on `http://localhost:5010` |
| `ORCHARD_APP_DATA` | An empty folder outside both repositories |
| Auto Setup | `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...SiteName=Orchard Core`, `...SiteTimeZone=America/Los_Angeles`, `...AdminUsername=admin`, `...AdminEmail=admin@example.com`, `...AdminPassword=<password>`, `...DatabaseProvider=Sqlite`, `...RecipeName=Blog` (the `orchardcore-tester` skill of the Orchard Core repository describes how to start the site this way). The Auto Setup feature is registered by `src/OrchardCore.Cms.Web/Program.cs:7-9` (`AddSetupFeatures("OrchardCore.AutoSetup")`), the configuration section is `AutoSetup` (`src/OrchardCore.Modules/OrchardCore.AutoSetup/Startup.cs:26`) and the environment variable form is the one of `src/docs/reference/modules/AutoSetup/README.md` |
| Sign-in | User `admin`, email `admin@example.com`, password `<password>`: chosen outside git and passed in `VIDEO_PASSWORD` |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls a provider |
| Tenant | `Default`, no other tenant |
| Features | None to enable. The Blog recipe enables Media, Content Types, Contents, Content Fields, Autoroute and Title (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:50-73`) |
| Settings | None. Leave the Media options and the Media API settings at their defaults (cookie authentication) |
| Theme | The theme of the Blog recipe (TheBlogTheme). A Page is displayed by its `Content.liquid`, which renders all of the content item's shapes (`src/OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:17`) |
| External services | None, so no stand-in is needed |
| Browser | The recorder's own Chromium, signed in by `Recorder setup`. See "Library preferences" below |

The Blog recipe also builds a menu of its own for the admin (the `AdminMenu` step, `blog.recipe.json:1188-1306`): a
**Content** group with a **Content Items** link and one link per content type (**Article**, **Page**, **Taxonomy**,
from `ContentTypesAdminNode`, `[Contents]/AdminNodes/ContentTypesAdminNodeNavigationBuilder.cs:49-62`). The admin
merges items that have the same text (`src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74-93`), so
there is one **Content** group and one **Content Items** entry, but the group also holds a **Page** link, and **Page**
is also an entry of the **New** menu of the Content Items list (`attach-1`). Scope every locator of those two words:
never look for **Page** or **Content Items** on the whole page.

The Blog recipe also puts three theme images in the library root (`about-bg.jpg`, `home-bg.jpg`, `post-bg.jpg`,
`blog.recipe.json:1124-1139`). They show brand names in their pictures, so the clips never open the library root:
the files used on camera are in a folder of their own (seed below).

### Seed data (outside the capture)

All the data is invented. Nothing of it is committed: the files are made in a folder outside the repository.

**1. The four files.** Make them with Pillow 10.1 or later (`pip install pillow`), in a folder that this plan calls
`<seed>`:

```python
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

out = Path("<seed>")            # a folder outside the repository
out.mkdir(parents=True, exist_ok=True)

def picture(name, caption, top, bottom):
    image = Image.new("RGB", (1600, 900))
    draw = ImageDraw.Draw(image)
    for y in range(900):
        t = y / 899
        draw.line([(0, y), (1600, y)], fill=tuple(int(a + (b - a) * t) for a, b in zip(top, bottom)))
    draw.text((800, 450), caption, font=ImageFont.load_default(size=120), fill="white", anchor="mm")
    image.save(out / name, quality=90)

picture("spring-orchard.jpg", "Spring orchard", (130, 196, 120), (46, 112, 64))
picture("greenhouse.jpg", "Greenhouse", (150, 200, 190), (40, 100, 110))
picture("workshop.jpg", "Workshop", (214, 180, 120), (120, 80, 40))

page = Image.new("RGB", (1240, 1754), "white")
ImageDraw.Draw(page).text((620, 400), "Visitor guide", font=ImageFont.load_default(size=100), fill="black", anchor="mm")
page.save(out / "visitor-guide.pdf", resolution=150)
```

The three pictures carry the words that the media text of the video repeats ("Spring orchard"), so what the
narration says about describing an image is true of what is on screen.

**2. The library folder.** Sign in as `admin` and open **Media > Library** (`[Media]/AdminMenu.cs:18-24`).
Create a folder named `open-house` at the root: open the ellipsis menu of the root folder in the tree (its title is
"Folder Action"), choose **Create a subfolder** (`[Media]/Assets/media-gallery/src/components/FolderMenu.vue:1-5`
and `Services/MediaJSLocalizer.cs:64`), type `open-house`, and confirm. Open the folder, and upload the four files
with **Upload files** (set them on the hidden file input `#fileupload`, with `SetInputFilesAsync`):
`spring-orchard.jpg`, `greenhouse.jpg`, `workshop.jpg` and `visitor-guide.pdf`. The folder lists four files.

**3. Nothing else.** The `Page` content type is left as the Blog recipe defines it: it is creatable
(`blog.recipe.json:448`), has no Media Field, and no Page exists. The recipe's own sample content (the Blog, a
blog post, the About article) stays as it is and is not shown.

### Library preferences

The media library saves the folder that was open and the list or grid choice in the browser's local storage, and
the Gallery editor saves its own card or list choice. A browser that saw the seeding would open the dialog in the
`open-house` folder. So: **after seeding, sign in again in a fresh browser context** (`Recorder setup` with
`VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`) so that `state.json` holds no saved preferences. A fresh context opens the
Select Media dialog at the root folder, in the list view (`[Media]/Assets/media-gallery/src/services/LocalStorage.ts:9`
sets the grid choice off by default), with a check box in the first column of every row. In the list view the root
folder shows only file names and file-type icons, so it may be on screen in `attach-2`. The grid view, whose
thumbnails show the theme pictures, must never be on screen at the root: if the dialog opens in the grid view, the
browser has saved preferences, so start again from a fresh context.

### Snapshots

Stop the site, copy its `ORCHARD_APP_DATA` folder and `state.json` aside as `snapshot-0` (seeded, signed in). The clips
change the site, and each clip starts from the state that the previous one left, so **copy the folder aside again after
every clip** (`snapshot-02`, `snapshot-03`, ...). To record a clip again, stop the site, restore the snapshot that was
taken before it, and start the site.

## 2. The order of the clips

The clips depend on each other and are recorded in the order they play: `02-add`, `03-settings`, `04-attach`,
`05-gallery`, `06-attached`. The slides (`01-idea`, `06-notes`) and the two cards need no recording.

| Clip | Needs | Changes the site |
| --- | --- | --- |
| `02-add` | `snapshot-0` | Adds the field Photos to the Page type |
| `03-settings` | `02-add` | Saves the settings of Photos (hint, anchors, image types) |
| `04-attach` | `03-settings` | Creates and publishes the Page "Spring Open House" |
| `05-gallery` | `04-attach` | Switches Photos to the Gallery editor, and publishes the Page in another order |
| `06-attached` | `05-gallery` | Adds the field Visitor Guide, and attaches a PDF to the Page |

**The id of the Page.** `05-gallery` and `06-attached` open the Page by its edit address, whose id the site chooses.
After `04-attach`, open **Content > Content Items**, click **Edit** on "Spring Open House", copy the id from the
address (`/Admin/Contents/ContentItems/<page-id>/Edit`), and keep it as `<page-id>` for the next two clips. Take it
again after every restore of a snapshot made before `04-attach`.

**The edit address used on camera** is always `/Admin/Contents/ContentItems/<page-id>/Edit?returnUrl=%2FAdmin%2FContents%2FContentItems`:
with a return address, **Publish** goes back to the Content Items list and shows its message, as **Edit** does in the
list (`[Contents]/Controllers/AdminController.cs:795-797` for a Page being created, `:841-858` for an existing one).

## 3. The clips

Every `OpenAsync` below happens before `StartCaptureAsync`, so it is not recorded. Every navigation during a clip uses
`ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync` or `NavigateAsync`. Wait conditions are named; never wait
for a fixed time except to let a notification stay on screen.

### Clip `02-add` (chapter 2, "Add a Media Field")

Starts on: `/Admin`, the dashboard. The admin menu groups **Design** and **Content Definition** are collapsed.
Collapsed groups are part of the story of the first step.

**`add-1`** (18 words)
- Actions: click **Design** in the admin menu, then **Content Definition**, then **Content Types**
  (`ClickAndWaitForNavigationAsync` on the last one). Each entry is the element `.item-label` inside `#left-nav`, whose
  `span.title` holds the text, so use `#left-nav` with `GetByText("Design", Exact)`, then `"Content Definition"`, then
  `"Content Types"`. **Design** and **Content Definition** are buttons that open their group (they have no page of their
  own); **Content Types** is a link. The groups are collapsed in a fresh browser context, and the children of a collapsed
  group are not visible, so click the groups in this order and wait for the next entry to be visible before clicking it
  (`TheAdmin/Views/NavigationItem-admin.cshtml`, `NavigationItemText.cshtml`, `Navigation-admin.cshtml`, under
  `src/OrchardCore.Themes`).
- Wait: the address ends with `/Admin/ContentTypes/List` and the row of **Page** shows its **Edit** button
  (`[role="btn-edit-Page"]`).
- On screen at the end: the **Content Types** list, with the type **Page** in it.
- Labels: **Design**, **Content Definition**, **Content Types**: `[Types]/AdminMenu.cs:23-25`. The list title
  "Content Types": `[Types]/Views/Admin/List.cshtml:6`.

**`add-2`** (22 words)
- Actions: click **Edit** in the row of **Page** (`[role="btn-edit-Page"]`, `ClickAndWaitForNavigationAsync`). On the
  type editor, scroll until the heading **Fields** and the button **Add Field** are in view, highlight the heading,
  then click **Add Field** (`ClickAndWaitForNavigationAsync`).
- Wait: the title `Add New Field To "Page"` and the field **Display Name** are visible.
- On screen at the end: the Add Field form for Page, empty, with the list of field types below the two name fields.
- Labels: **Edit**: `[Types]/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`. **Fields**:
  `[Types]/Views/Admin/Edit.cshtml:88`. **Add Field**: `[Types]/Views/Admin/Edit.cshtml:137`. The title:
  `[Types]/Views/Admin/AddFieldTo.cshtml:20`.
- Note: the Fields list of the Page type is empty (the Page has no field). The **Add Field** button shows because the
  fields are registered (`Edit.cshtml:84`).

**`add-3`** (24 words)
- Actions: type `Photos` into **Display Name** (`input[name="DisplayName"]`) and press Tab, so the **Technical Name**
  field fills in `Photos` by itself. Click the radio button labeled **Media Field** (`input[name="FieldTypeName"][value="MediaField"]`).
  Click **Save & Configure** (`button[value="SaveAndConfigure"]`, `ClickAndWaitForNavigationAsync`).
- Wait: the title `"Photos" settings` and the notification `The field "Photos" has been added.` are visible.
- On screen at the end: the settings page of the field Photos (its Display Name `Photos`, the read-only Technical Name,
  and the first settings) with the green notification.
- Labels: **Display Name**, **Technical Name**, **Field Type**, **Save & Configure**:
  `[Types]/Views/Admin/AddFieldTo.cshtml:38,46,54,72`. The Technical Name fills in from the Display Name:
  `[Types]/Assets/js/autocomplete.ts:5-23`. **Media Field** is the field type name `MediaField` made readable:
  `AddFieldTo.cshtml:62`. The notification: `[Types]/Controllers/AdminController.cs:768`. The settings title:
  `[Types]/Views/Admin/EditField.cshtml:30`.

### Clip `03-settings` (chapter 3, "The settings of the field")

Starts on: `/Admin/ContentParts/Page/Fields/Photos/Edit` (the settings of Photos, opened directly, without a return
address, so **Save** returns to the type editor, `[Types]/Controllers/AdminController.cs:915-925`). Wait for the title
`"Photos" settings`.

**`settings-1`** (37 words)
- Actions: highlight the check box **Required** (it stays clear), then **Multiple** (it is ticked), then clear the
  highlights; click the text box **Hint** (`GetByLabel("Hint", Exact)`, an `input` of type text,
  `MediaFieldSettings.Edit.cshtml:40-43`) and type `Choose the photos for this page.` The narration ends with the words
  "let's type one", so start typing at the end of the step.
- On screen at the end: **Required** clear, **Multiple** ticked, and the Hint text in its field.
- Labels: **Required**, **Multiple**, **Hint**: `[Media]/Views/MediaFieldSettings.Edit.cshtml:7,16,40`. Multiple is on by
  default: `src/OrchardCore/OrchardCore.Media.Core/Settings/MediaFieldSettings.cs:9`.

**`settings-2`** (35 words)
- Actions: highlight **Allow media text** (ticked by default), clear the highlight, highlight **Allow anchors** (clear
  by default) and click it so it is ticked.
- On screen at the end: **Allow media text** and **Allow anchors** both ticked.
- Labels: `[Media]/Views/MediaFieldSettings.Edit.cshtml:25,34`. Defaults: `MediaFieldSettings.cs:12,14`
  (`AllowMediaText` true, `AllowAnchors` false). The narration says the first is on by default and the second is
  off, so show **Allow media text** already ticked before highlighting it.

**`settings-3`** (37 words)
- Actions: scroll to **Allow all default media types** (ticked), click it to clear it, and wait until the block of
  file types (`#MediaExtensionWrapper`) is shown. Click the check box labeled **image types**: the image extensions
  below it become ticked. Highlight it.
- On screen at the end: **Allow all default media types** clear, **image types** ticked, with its extensions (such
  as `.jpg`, `.jpeg`, `.png`, `.gif`) ticked, and the other groups (**application types**, **audio types**,
  **text types**, **video types**) clear.
- Labels: `[Media]/Views/MediaFieldSettings.Edit.cshtml:50,72` (the group label is `{0} types` with the first part of
  the content type: `image`, `application`, `audio`, `text`, `video`). The group box ticks its extensions:
  `[Media]/Assets/ts/media-field-settings.ts:1-14`. That the dialog then lists only those files:
  the folder is read by `GetDirectoryContentEndpoint.cs:59,64-73` (the `extensions` argument) with
  `[Media]/Endpoints/Api/MediaEndpointHelpers.cs:144-170`, and the same filter is in `GetMediaItemsEndpoint.cs:61-78`;
  the gallery passes the extensions of the field in `FileLibraryManager.ts:326,366,385` (under
  `[Media]/Assets/media-gallery/src/services`).

**`settings-4`** (22 words)
- Actions: scroll to the question **What type of editor should be used?**, highlight its list, and leave **Standard**
  chosen (`SelectAsync` with the value `""`, only to show the list; do not choose another). Click **Save**
  (`button.save`, `ClickAndWaitForNavigationAsync`).
- Wait: the type editor (`Edit Content Type - Page`) and the notification `The "Photos" field settings have been saved.`
- On screen at the end: the type editor, with **Photos** in the Fields list (no editor badge, since the Standard
  editor is the default) and the notification.
- Labels: the question: `[Types]/Views/Admin/EditField.cshtml:79`. The three choices: **Standard**
  `[Media]/Views/MediaField.Option.cshtml:4`, **Gallery** `[Media]/Views/MediaField-Gallery.Option.cshtml:4`,
  **Attached** `[Media]/Views/MediaField-Attached.Option.cshtml:4`. **Save**: `EditField.cshtml:120`. The
  notification: `[Types]/Controllers/AdminController.cs:908`.
- The editor list is chosen by value, never by position: the values are `""` (Standard), `Gallery` and `Attached`,
  and their order comes from the shape table.

### Clip `04-attach` (chapter 4, "Attach media to a page")

Starts on: `/Admin/Contents/ContentItems` (the **Content Items** list). Wait for the **New** button
(`#new-dropdown`).

**`attach-1`** (22 words)
- Actions: before the capture, expand **Content** in the admin menu so that **Content Items** shows. Highlight the
  entry **Content Items** (it is the page that is open) and clear the highlight; click **New** (`#new-dropdown`);
  highlight **Page** in the open menu; click **Page** (`ClickAndWaitForNavigationAsync`). Scope **Page** to the open
  menu: `#new-dropdown` is followed by its `.dropdown-menu` (`ContentsAdminListCreate.cshtml:18-22`), so use the link of
  exact name **Page** inside `#new-dropdown + .dropdown-menu`. The admin menu holds a **Page** link too (see section 1), and a locator on the whole
  page would match twice.
- Wait: the title `New Page` and the field **Title** are visible.
- On screen at the end: the editor of a Page, empty.
- Labels: **Content**, **Content Items**: `[Contents]/AdminMenu.cs:65,68`. **New**:
  `[Contents]/Views/ContentsAdminListCreate.cshtml:20`. The title: `[Contents]/Views/Admin/Create.cshtml:26`.
  The Page is creatable: `blog.recipe.json:448`. **Page** is not a fixed label: it is the Display Name of the content
  type, set by the recipe (`blog.recipe.json:444-445`), and the **New** menu writes it from the type definition
  (`ContentsAdminListCreate.cshtml:25`).

**`attach-2`** (23 words)
- Actions: type `Spring Open House` into **Title**. Scroll until the field **Photos** is fully in view: its Hint
  `Choose the photos for this page.`, the empty area with the text **No Files**, and the toolbar button **Add media**.
  Highlight the Hint and **No Files**, clear the highlights, and click **Add media** (`.media-field-app button.mf-btn-primary`).
  Wait until the dialog **Select Media** is open and the text **Loading media browser...** is gone.
- On screen at the end: the **Select Media** dialog open over the editor, with the library inside it (folder tree on
  the left, the toolbar with **Upload files** and the search box, and the file list of the root folder in the list
  view: the three theme files by name, with no thumbnails).
- Labels: **Title**: `src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`. **Hint**
  (the field's help line): `[Media]/Views/MediaField.Edit.cshtml:39`. **No Files**, **Add media**, **Select Media**,
  **Loading media browser...**: `[Media]/Services/MediaJSLocalizer.cs:118,119,137,138`; the button:
  `[Media]/Assets/media-picker/src/components/MediaFieldBasic.vue:10-16`; the dialog:
  `MediaPickerModal.vue:18`.
- The Photos field may sit below other cards of the Page editor; scroll with `ScrollAsync`. Clear the highlights
  before scrolling (highlights are fixed on the page).

**`attach-3`** (31 words)
- Actions: in the folder tree of the dialog, click the folder `open-house`; wait until the file list shows three rows.
  Highlight the search box (`#file-filter-input`, placeholder **Search and filter...**) and the button **Upload files**
  (do not use them), clear the highlights. Tick the check boxes of the rows `spring-orchard.jpg` and `greenhouse.jpg`
  (`.file-item` rows, `input[name="select-file"]`).
- On screen at the end: the list of `open-house` with **three image rows only** (`greenhouse.jpg`, `spring-orchard.jpg`,
  `workshop.jpg`: the PDF is **not** listed), two of them ticked, and `2 selected` at the bottom left of the dialog.
- Labels: **Upload files** and **Search and filter...**: `[Media]/Services/MediaJSLocalizer.cs:27,30`; the toolbar:
  `[Media]/Assets/media-gallery/src/App.vue:93-126`. The folder tree and list: `App.vue:19`,
  `Assets/media-gallery/src/components/FileItemsComponent.vue:35`. The PDF is not listed because the field allows
  image types: `[Media]/Endpoints/Api/MediaEndpointHelpers.cs:144-170` (called by `GetDirectoryContentEndpoint.cs:64-73`).
- If the PDF is listed, the settings of `03-settings` were not saved: restore `snapshot-02` and record `03-settings` again.

**`attach-4`** (22 words)
- Actions: highlight the text `2 selected` in the footer of the dialog, clear the highlight, click **OK** in the
  footer (the primary button, `.mf-picker-shell .mf-modal-footer .mf-btn-primary`). Wait until the dialog is closed.
- On screen at the end: the editor with two thumbnails in the Photos field, each with its file name under the picture
  (`spring-orchard.jpg`, `greenhouse.jpg`). Neither is selected.
- Labels: the footer count: `[Media]/Assets/media-picker/src/components/MediaPickerModal.vue:39`; **OK**:
  `MediaPickerModal.vue:43-50` and `Services/MediaJSLocalizer.cs:126`. Each thumbnail shows its name:
  `Assets/media-picker/src/components/ThumbsContainer.vue` (the `mf-filename` span).

**`attach-5`** (31 words)
- Actions: click the thumbnail of `spring-orchard.jpg` (`.mf-thumb-item`): it is highlighted (a border and a shadow in the primary color of the admin theme,
  `[Media]/Assets/shared/media-admin.css:14`) and three buttons appear in the toolbar. Highlight **Remove**, **Media text** and **Anchor** one after the other, clear the highlights,
  then click **Media text**. Wait for the box **Enter media text**.
- On screen at the end: the box **Enter media text** open, with a text area labeled **Media text** (empty) and the
  buttons **Cancel** and **OK**.
- Labels: the buttons appear only when a file is selected, and **Anchor** only for an image whose field allows
  anchors: `[Media]/Assets/media-picker/src/components/MediaFieldBasic.vue:19-47`. **Remove**, **Media text**,
  **Anchor**, **Enter media text**, **Cancel**, **OK**: `[Media]/Services/MediaJSLocalizer.cs:120-127`.
  The box: `MediaFieldBasic.vue:86-106`.

**`attach-6`** (19 words)
- Actions: type `Spring orchard` into the text area (`.mf-form-textarea`), then click **OK** in the box
  (`.mf-modal .mf-modal-footer .mf-btn-primary`). Wait until the box is closed.
- On screen at the end: the thumbnail of `spring-orchard.jpg` still selected, the toolbar still showing its buttons.
- Labels: as in `attach-5`. The text is kept with the file: `MediaFieldBasic.vue:326-331`.

**`attach-7`** (32 words)
- Actions: click **Anchor**. Wait for the box **Select an anchor**, which shows the picture. Click on the picture at
  about 70% of its width and 35% of its height (the crosshair marker moves there). Highlight **Reset Anchor** (do not
  click it), clear the highlight, then click **OK** in the box.
- On screen at the end: the box closed, the thumbnail still selected.
- Labels: **Anchor**, **Select an anchor**, **Reset Anchor**, **OK**: `[Media]/Services/MediaJSLocalizer.cs:123-126`.
  The box and the reset to the center: `MediaFieldBasic.vue:114-140` and `:355-358` (the reset sets the point to
  `0.5, 0.5`).

**`attach-8`** (11 words)
- Actions: scroll to the top of the editor if needed, click **Publish** (`button.publish`, the main button of the
  split button: leave the arrow alone). Wait for the **Content Items** list.
- On screen at the end: the **Content Items** list, the green notification `Your Page has been published.` and the
  row **Spring Open House**.
- Labels: **Publish**: `[Contents]/Views/Content.PublishButton.cshtml:19`. The notification:
  `[Contents]/Controllers/AdminController.cs:473`. The return to the list because the address carries a return
  address: `[Contents]/Controllers/AdminController.cs:795-798`.

**`attach-9`** (27 words)
- Actions: `NavigateAsync("/spring-open-house")` at the start of the step, as the narration says "Let's open the page
  on the site" (the address comes from the title, `blog.recipe.json:474`: the
  Autoroute pattern of the Page is the slug of its title). Wait until both pictures are loaded
  (`img` elements of `.field-name-page-photos` that are `complete` with a `naturalWidth` above 0). Scroll so the title,
  the name **Photos** and the pictures are as visible as the theme allows.
- On screen at the end: the page "Spring Open House" of the site, with the name **Photos** and the two images (the green
  "Spring orchard" and the blue-green "Greenhouse" pictures) below the title. The pictures are plain `img` tags of
  1600 x 900 pixels without a size: whether the theme scales them to fit the frame is not known (see
  `OPEN-QUESTIONS.md`). Scroll so that as much of the two pictures as the theme shows is in the frame.
- The masthead of every Page is the theme's own picture `post-bg.jpg`, painted by `Content.liquid`
  (`src/OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:4`): an astronaut at work outside a spacecraft, with no
  face, name or brand legible. It may stay on screen.
- Labels: the title of the page, and the display name of the field, **Photos**: it is a `span` with the class `name`
  written before the files (`[Media]/Views/MediaField.cshtml:10`; the name is the one typed in `add-3`, it is not
  translated). The images and the alternative text come from
  `[Media]/Views/MediaField.cshtml:14-25` (an `img` whose `alt` is the media text of that file).
- After the clip, check (not on camera) that the first image has `alt="Spring orchard"` and the second `alt=""`,
  as described in `OPEN-QUESTIONS.md`.

### Clip `05-gallery` (chapter 5, "The Gallery editor")

Starts on: `/Admin/ContentParts/Page/Fields/Photos/Edit`, the settings of Photos. Wait for the title
`"Photos" settings`.

**`gallery-1`** (44 words)
- Actions: scroll to the question **What type of editor should be used?**, highlight its list, and choose **Gallery**
  (`SelectAsync` with the value `Gallery`). Click **Save** (`ClickAndWaitForNavigationAsync`).
- On screen at the end: the type editor (`Edit Content Type - Page`) with the notification `The "Photos" field settings
  have been saved.` and **Photos** in the Fields list, now with a badge that reads **Gallery**.
- Labels: as in `settings-4`. The badge: `[Types]/Views/Admin/Edit.cshtml:121-124` (it shows the editor name).

**`gallery-2`** (37 words)
- Actions: `NavigateAsync` to the edit address of the Page (section 2). Wait until the dashed card of the Photos field is visible
  (`.field-wrapper-page-photos .mf-gallery-add-card`, the wrapper has the same class for the Gallery editor as for the
  others: `MediaField-Gallery.Edit.cshtml:33`; the card: `GalleryContainer.vue:98-113`). The narration opens with "Let's
  edit the Page again", so the navigation happens at the start of the step. Scroll
  until the field is in view. Highlight, one after the other, the buttons of the first card (media text, anchor,
  download, remove) and the dashed **Add media** card at the end, then clear the highlights.
- On screen at the end: the field Photos as **cards**: the two files (`spring-orchard.jpg`, `greenhouse.jpg`) with a row
  of small icon buttons under each, and the dashed card **Add media** after them. The files are the ones chosen in
  `04-attach`: the Standard and the Gallery editors keep the same list.
- Labels: **Add media**: `[Media]/Services/MediaJSLocalizer.cs:119`, shown on the add card at
  `[Media]/Assets/media-picker/src/components/GalleryContainer.vue:106-113`. The icon buttons (their titles are
  `Edit media text`, `Set anchor`, `Download`, `Remove`, in English only): `GalleryCardItem.vue:58-98`. The Gallery
  editor shows cards by default: `GalleryContainer.vue:151`. Both editors store the same list of paths:
  `[Media]/Views/MediaField-Gallery.Edit.cshtml:46` and `MediaField.Edit.cshtml:46`
  (the same value, `Paths`, feeds both editors).
- If the first card shows a warning icon, a file path is wrong: stop and see `OPEN-QUESTIONS.md`.

**`gallery-3`** (25 words)
- Actions: click the toolbar button titled **List view** (the files become rows), wait until
  `.field-wrapper-page-photos ol.mf-gallery-list` is visible, click **Grid view** (the cards return) and wait until
  `.field-wrapper-page-photos ol.mf-gallery-cards` is visible again. Drag the card of `greenhouse.jpg` onto the card of `spring-orchard.jpg` (`DragToAsync`: the two
  swap places). Click **Publish** (`button.publish`) and wait for the Content Items list.
- On screen at the end: the **Content Items** list with the notification `Your Page has been published.`
- Labels: the toolbar buttons (the titles `List view` and `Grid view` are English only): `GalleryContainer.vue:18-49`.
  Reordering by drag: `GalleryContainer.vue:189-210`. **Publish**: as in `attach-8`. For an item that exists, the
  message `Your Page has been published.` is written by `[Contents]/Controllers/AdminController.cs:962-967`
  (`PublishOrUnpublishAsync`, `:938-980`, reached by `EditAndPublishPOST`, `:548-551`), which reads the editor form first (`EditInternalAsync`, `UpdateEditorAsync` at `:828`).
- If the drag does not reorder the cards, see `OPEN-QUESTIONS.md`; the step is still valid with the cards unmoved
  only if the narration is adjusted.

### Clip `06-attached` (chapter 6, "The Attached editor")

Starts on: `/Admin/ContentTypes/Edit/Page`, the type editor of Page. Wait for the **Fields** heading.

**`attached-1`** (27 words)
- Actions: click **Add Field** (`ClickAndWaitForNavigationAsync`), type `Visitor Guide` into **Display Name**
  (the **Technical Name** fills in as `VisitorGuide`), click the radio button **Media Field**, click
  **Save & Configure** (`ClickAndWaitForNavigationAsync`).
- Wait: the title `"Visitor Guide" settings` and the notification `The field "Visitor Guide" has been added.`
- On screen at the end: the settings page of Visitor Guide.
- Labels: as in `add-2` and `add-3`.

**`attached-2`** (33 words)
- Actions: clear **Multiple**. Clear **Allow all default media types** and wait for the block of file types. Tick the
  check box labeled `.pdf` (`GetByLabel(".pdf", Exact)`, in the group **application types**; scroll to it).
  Choose **Attached** in the list under **What type of editor should be used?** (`SelectAsync` with the value
  `Attached`). Click **Save** (`ClickAndWaitForNavigationAsync`).
- On screen at the end: the type editor, the notification `The "Visitor Guide" field settings have been saved.`, and
  **Visitor Guide** in the Fields list with a badge that reads **Attached**. Before saving, the frame shows **Multiple**
  clear, `.pdf` ticked and **Attached** chosen.
- Labels: as in `settings-1` to `settings-4`; **Attached**: `[Media]/Views/MediaField-Attached.Option.cshtml:4`; the
  extension check boxes are labeled with the extension: `[Media]/Views/MediaFieldSettings.Edit.cshtml:84-86`.

**`attached-3`** (33 words)
- Actions: `NavigateAsync` to the edit address of the Page (section 2). Scroll until the field **Visitor Guide** is in
  view. Highlight its button **Add media** (the label of a hidden file input) and **do not click it**: a click opens
  the system file dialog, which the recorder cannot draw. Set the file directly on the hidden input:
  `scene.Page.Locator(".field-wrapper-page-visitor-guide input[type=file]").SetInputFilesAsync("<seed>/visitor-guide.pdf")`
  (the wrapper of the field has the class `field-wrapper-<part>-<field>`, with each name made lower case and split at
  its capital letters: `MediaField-Attached.Edit.cshtml:36`, `src/OrchardCore/OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:77-110`;
  the Photos field has none, because the Gallery editor has no file input, and other file inputs of the page are
  not read, so the locator stays inside the wrapper). Wait until a thumbnail named `visitor-guide.pdf` is in the field and the upload toast at the bottom
  right (header **Uploads**, a row for the file) shows a check mark for it: `UploadList.vue`.
- On screen at the end: the field Visitor Guide with one thumbnail (a document icon and the name `visitor-guide.pdf`),
  and the field **Photos** above it in the Gallery editor. No dialog is open.
- Labels: **Add media** on a label with a file input: `[Media]/Assets/media-picker/src/components/MediaFieldAttached.vue:6-24`;
  **Uploads**: `Services/MediaJSLocalizer.cs:133`; the toast: `[Media]/Assets/media-picker/src/components/UploadList.vue:6-60`. The field has no picker dialog: `[Media]/Views/MediaField-Attached.Edit.cshtml:44-58`.
- Check the headless upload works (`OPEN-QUESTIONS.md`).

**`attached-4`** (24 words)
- Actions: click **Publish** (`button.publish`) and wait for the Content Items list.
- On screen at the end: the **Content Items** list with the notification `Your Page has been published.`
- Labels: as in `attach-8` and, for an item that exists, `[Contents]/Controllers/AdminController.cs:962-967`. The editor
  form is read (`UpdateEditorAsync`, `:828`) before the item is published, so the field driver runs. The file moves out of
  the temporary folder when the content item is saved:
  `[Media]/Services/AttachedMediaFieldFileService.cs:135-148` and `:190-215`, called from `[Media]/Drivers/MediaFieldDisplayDriver.cs:105-111`.

**`attached-5`** (31 words)
- Actions: `NavigateAsync("/spring-open-house")`. Wait until the link of the guide is visible
  (`.field-name-page-visitor-guide a`, `MediaField.cshtml:11,22-24`). Scroll to the bottom of the page content until the
  section **Visitor Guide** shows its link. Highlight the link.
- On screen at the end (the masthead picture may be out of the frame, as in `attach-9`): the page with the name
  **Photos** and its two pictures, and under them the name **Visitor Guide** (`MediaField.cshtml:10`) with a link whose
  text is the path of the file (it starts with `mediafields/Page/` and ends with `.pdf`), since no media text was
  entered.
- Labels: a file that is not an image is shown as a link, with the media text or else the path:
  `[Media]/Views/MediaField.cshtml:22-24`. The folder `mediafields/<content type>/<content item id>`:
  `[Media]/Services/AttachedMediaFieldFileService.cs:277`.

### Slides and cards (not recorded)

| Step | What the slide or card shows |
| --- | --- |
| `intro` | Opening card: the Orchard Core logo, "The Orchard Core Media Field", subtitle "Attach images and files to your content" |
| `idea-1` | Slide "What the field holds": four cards (A location, not a copy; One file, many pages; Media text; Anchor) |
| `idea-2` | Slide "Three editors": three cards (Standard, Gallery, Attached) |
| `attached-6` | Slide "Good to know about attached files": three cards (A folder for each page; Removed with the page; A permission of its own) |
| `outro` | Closing card with the subtitle "docs.orchardcore.net" |

`attached-6` is the second clip (`06-notes`) of chapter 6. Its claims come from the code: the folder
(`[Media]/Services/AttachedMediaFieldFileService.cs:277`), the removal with the content item
(`[Media]/Handlers/AttachedMediaFieldContentHandler.cs:18-24`), and the permission
(`src/OrchardCore/OrchardCore.Media.Core/MediaPermissions.cs:14`; the Administrator role has `ManageMediaFolder`, which
implies it: `[Media]/PermissionProvider.cs:34`).

## 4. Checks for private data

Before assembling, look at the contact sheet of every clip, and afterwards at the OCR of the whole video:

- The only person is the user `admin` with `admin@example.com`; no other name, email address or phone number appears.
- No password, token or `VIDEO_PASSWORD` is on screen. The sign-in form is never captured (Auto Setup and
  `state.json`).
- No file path of this machine appears: the file dialog is never opened, and `<seed>` is only in the script.
- The three theme pictures of the library root (`about-bg.jpg` shows brand names; `home-bg.jpg` and `post-bg.jpg`
  show objects and an astronaut) may be on screen **by name only**: the root folder in the list view, as in
  `attach-2` and the first moment of `attach-3`. Their thumbnails must never be on screen: the library is never shown
  in the grid view at the root. The masthead of the Page (`post-bg.jpg`) is the one exception, see `attach-9`.
- The address bar is not captured (the recorder shows only the page); the site name is "Orchard Core".
- The sample blog content of the Blog recipe (the sample post and its text) does not appear in any clip except as an
  entry of the Content Items list.

## 5. Commands

The scenes (`recorder/Scenes.cs`, one method per clip with one `scene.StepAsync` per step id of this plan, in the
order above) are written by the recording session from this plan. The commands, from the repository root, in Git Bash
on Windows (forward slashes):

```bash
export MSYS_NO_PATHCONV=1
export VIDEO_ROOT=<repo>/videos/media-field VIDEO_BASE_URL=http://localhost:5010
export VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>      # the password is chosen outside git

cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/media-field/recorder
<bin>/Recorder setup                                        # signs in, in a fresh context; saves state.json (git-ignored)
cd ../../../../..

python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/media-field
<bin>/Recorder record 02-add                                # then 03-settings, 04-attach, 05-gallery, 06-attached
python .agents/skills/orchardcore-video-producer/scripts/review.py videos/media-field 02-add   # one contact sheet per clip
```

Copy the site's `ORCHARD_APP_DATA` folder aside after every clip, as described in section 1 (Snapshots).

# Recording plan: Menus overview

How to record the clips of this video without having to decide anything. A file:line reference is relative to `src/` of the
Orchard Core checkout the site is built from, and a short module name stands for its folder: `Menu/AdminMenu.cs` is
`OrchardCore.Modules/OrchardCore.Menu/AdminMenu.cs`, and `Contents/`, `Alias/`, `Title/` and `Features/` are
`OrchardCore.Modules/OrchardCore.Contents/` and so on. In this file a selector written in
backticks stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. Everything not marked as
"before the capture" happens between `StartCaptureAsync` and the end of the clip, one `StepAsync` per step id below.

The video has two cards (`intro`, `outro`: nothing to record), four slide steps and five recorded clips
(`02-open`, `03-add`, `04-arrange`, `05-publish`, `06-create`). Narrate first (`narrate.py`), then record.

## 1. The site

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout that contains the Menu module and the Blog recipe (`TheBlogTheme`). |
| Address | `http://localhost:5010` (`VIDEO_BASE_URL`). |
| Data | An empty folder outside both repositories, passed as `ORCHARD_APP_DATA`. |
| Setup | Auto Setup, no setup form and no sign-in form on camera. Tenant `Default`, `SiteName` "Orchard Core", `SiteTimeZone` `America/Los_Angeles`, `AdminUsername` `admin`, `AdminEmail` `admin@example.com`, `AdminPassword` the value of `VIDEO_PASSWORD` (chosen outside git, written in this repository as `<password>` only), `DatabaseProvider` `Sqlite`, `RecipeName` `Blog`. The environment variables are named `OrchardCore__AutoSetup__Tenants__0__...` (see `docs/reference/modules/AutoSetup/README.md`). |
| Network | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address in the environment of the site process, so the site never calls a mail, SMS or payment provider. No external service is used by the site. The variables do not reach the recorder's browser (its context sets no proxy, `Scene.cs`), and the Blog theme asks the browser for two things on the internet: the fonts of `fonts.googleapis.com` (`TheBlogTheme/Views/Layout.liquid:12-13`) and Bootstrap's script from `cdn.jsdelivr.net` (`TheBlogTheme/ResourceManagementOptionsConfiguration.cs:15-18`: the script has only a CDN address). The machine that records `02-open` and `05-publish` needs access to these two hosts; the drop-down of `publish-4` is Bootstrap's (OPEN-QUESTIONS.md, question 14). |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` writes `state.json` (git-ignored). |
| Browser | The recorder's defaults: 1536x864 CSS pixels, scale 1.25, light color scheme, `en-US`. Leave the admin menu as the page draws it in every clip. |

### Features

Nothing to enable. The Blog recipe enables `OrchardCore.Menu` (and Contents, Title, Alias, Dynamic Cache, Autoroute, Admin Menu).
Before the first take, check once on Tools, Features that Menu is enabled.

### Seed data

None to create. The Blog recipe creates everything the clips use:

- the menu "Main Menu" (alias `main-menu`) with two Link Menu Items, "Home" (Url `~/`) and "About" (Url `~/about`)
  (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:817-856`), published;
- the content items the new links point to: the Blog (path `blog`) and the Categories taxonomy (path `categories`);
- the theme `TheBlogTheme`, whose layout shows the menu with the alias `main-menu` in its navigation bar
  (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:32`).

The fake values typed in the clips are the item names "Browse" and "Categories", the title "Footer Menu", and the
addresses `~/blog` and `~/categories`.

### Snapshots

Stop the site (by its process ID), copy the `App_Data` folder aside, and start the site again:

- `snap-0`: right after Auto Setup, before any clip (also holds the first sign-in);
- `snap-3`: after `03-add`; `snap-4`: after `04-arrange`; `snap-5`: after `05-publish`.

To record a clip again, stop the site, restore the snapshot named in the table below, and start it again.

## 2. Order of the clips, and what they change

| Clip | Starts from | Changes the site | Restore before a new take |
|---|---|---|---|
| `02-open` | `snap-0` | nothing | nothing to restore |
| `03-add` | `snap-0` | adds "Browse" to a draft of Main Menu | `snap-0` |
| `04-arrange` | `snap-3` | adds "Categories" inside "Browse", reorders, saves the draft | `snap-3` |
| `05-publish` | `snap-4` | publishes Main Menu | `snap-4` |
| `06-create` | `snap-5` | creates and publishes "Footer Menu" | `snap-5` |

Record in this order: each clip depends on the one before it (the items exist, the draft exists, the menu is
published). Take `snap-3`, `snap-4` and `snap-5` right after the clip of the same number.

## 3. Locators used by several clips

The views are in `OrchardCore.Modules/OrchardCore.Menu/Views/` and `OrchardCore.Modules/OrchardCore.Contents/Views/`.
The class names come from them; confirm each one once with `Recorder probe` before the first take.

- **Menus page** (`MenusUrl`): `/Admin/Contents/ContentItems/Menu?Options.SelectedContentType=Menu&Options.CanCreateSelectedContentType=true`
  (the address the Content, Menus item of the admin menu links to, `OrchardCore.Modules/OrchardCore.Menu/AdminMenu.cs:33-39`).
  Check it by clicking that item once and reading `scene.Page.Url`.
- **A row of the Menus page**: the list row that holds the title link of the menu, for example the row with the link
  "Main Menu". Its buttons are `a.edit` (label "Edit", `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`),
  and its badges read "Published" and "Draft" (`ContentsTags_SummaryAdmin.cshtml:12,19`).
- **Open the Main Menu editor** (used before the capture of `03-add` and `04-arrange`, and inside `publish-3`): open
  `MenusUrl`, click `a.edit` in the row of "Main Menu", and wait until the button "Add Menu Item" is enabled (it is
  disabled while the menu was never saved, `Menu/Views/MenuPart.Edit.cshtml:19,30`). The page opened this way has a
  `returnUrl`, so its buttons Publish and Save Draft are split buttons (with a drop-down "and continue") and a "Cancel"
  link shows (`Contents/Views/Content.PublishButton.cshtml:5-26`, `Content.SaveDraftButton.cshtml`,
  `Contents/Views/Content.Edit.cshtml:28-31`). Only a page opened from the list has it: after an item is added, edited
  or deleted, the Menu controller redirects to the editor without a `returnUrl` (`Menu/Controllers/AdminController.cs:139,239,278`).
  On that page Publish and Save Draft are single buttons, no "Cancel" link shows, and Save Draft or Publish stays on the
  editor instead of returning to the list (`Contents/Controllers/AdminController.cs:841-858`, `EditInternalAsync`).
- **The item list of the editor**: `#menu` with one `li.menu-item` per item, in order, each with `data-index` (top level
  "0", "1", "2"; an item inside the third one has "2-0") and `data-depth`. The title bar of an item is
  `.menu-item-title`: it is the drag handle. The buttons of a row are `a.btn-success` (label "Edit"),
  `button.btn-primary` (label "Add") and `a.btn-danger` (label "Delete") (`MenuItem.Admin.cshtml:31-49`).
  Find a row with `li.menu-item` filtered by its text, for example "Browse".
- **The window "Available Menu Items"**: `#modalMenuItems`. It holds one `.card` per item type, titled "Link Menu Item",
  "Content Menu Item" and "Html Menu Item"; the button of a card is `a.thumbnail-link-create` (label "Add"); the
  window has a "Cancel" button (`MenuPart.Edit.cshtml:60-79`, `MenuItem.Thumbnail.cshtml:16`). Always pick the card
  by its title, never by its position.
- **The form of a Link Menu Item**: the fields labeled "Name", "Url" and "Target" (`LinkMenuItemPart.Edit.cshtml:4,11,18`), then a
  "Permissions" picker (`MenuItemPermissionPart.Edit.cshtml`) that these clips leave empty,
  for example `GetByLabel("Name", new() { Exact = true })`. Its button is `button.publish` (label "Publish").
  The form has no returnUrl, so Publish is a single button and there is no Save Draft
  (`Content.PublishButton.cshtml:5-8`).
- **The editor of a menu, buttons at the bottom**: `button.publish` ("Publish"), `button.draft` ("Save Draft").
- **The front end**: the navigation bar `#mainNav`, whose menu is rendered by `TheBlogTheme/Views/Layout.liquid:32`.
  A top-level item with items inside it is rendered as a drop-down toggle (`TheBlogTheme/Views/MenuItem.liquid:1-13`,
  `MenuItemLink-LinkMenuItem.liquid:3-4`).

Waits are always for a concrete condition: a heading, a row or a button becomes visible or enabled, never a fixed
delay (the fixed pauses below are pauses for the narration, not waits).

## 4. The clips

### Slides (`01-menus`)

One line per step; `slides.py` draws them.

- `menus-1`: heading "Two kinds of navigation", two cards: "The admin menu" and "Menus".
- `menus-2`: heading "The parts of a menu", three cards: "Title", "Alias" and "Menu items".
- `menus-3`: heading "Three types of menu items", three cards: "Link Menu Item", "Content Menu Item", "Html Menu Item".
- `menus-4`: heading "How a theme shows a menu", three cards: "Alias", "Published version" and "Publish".

### `02-open`: open a menu

Starts on `/` (the front end), signed in as admin. Before the capture: `OpenAsync("/")`, then wait until `#mainNav`
shows the links "Home" and "About". Changes nothing.

- `open-1`: with `HighlightAsync` draw a ring around the links "Home" and "About" in `#mainNav`. Nothing else moves.
  On screen at the end: the front page with the ring around the two links.
  Source: `blog.recipe.json:829-850` (the two items), `Layout.liquid:32`.
- `open-2`: `ClearHighlightsAsync`; `NavigateAsync("/Admin")`; click "Content" in the admin menu (match the text exactly:
  the admin menu of the Blog recipe also has "Content Items" and a top-level "Main Menu"); in the sub menu that
  shows, `ClickAndWaitForNavigationAsync` on "Menus" (exact text); wait until the row "Main Menu" is visible.
  On screen at the end: the Menus page with the row "Main Menu" (badge "Published") and the button "New Menu".
  Source: `Menu/AdminMenu.cs:33-34` (labels "Content", "Menus"); `Contents/Views/ContentsAdminListCreate.cshtml:14`
  (label "New Menu"). The narration also mentions Tools, Features: nothing to show for that sentence.
- `open-3`: `ClickAndWaitForNavigationAsync` on `a.edit` of the row "Main Menu"; wait until "Add Menu Item" is visible; then
  `HighlightAsync` the field labeled "Title" (shows "Main Menu"), and after 3 seconds `ClearHighlightsAsync` and
  `HighlightAsync` the field labeled "Alias" (shows "main-menu").
  On screen at the end: the editor of the Main Menu, the Alias field ringed.
  Source: `Title/Views/TitlePart.Edit.cshtml:12`, `Alias/Views/AliasPart.Edit.cshtml:10`; recipe `blog.recipe.json:822,852`.
- `open-4`: `ClearHighlightsAsync`; `HighlightAsync` the list `#menu` (the rows "Home" and "About" with the labels
  "Edit", "Add" and "Delete"), wait 3 seconds, `ClearHighlightsAsync`, `HighlightAsync` the button "Add Menu Item".
  On screen at the end: the button "Add Menu Item" ringed above the two rows.
  Source: `MenuPart.Edit.cshtml:30-31`, `MenuItem.Admin.cshtml:31-49`.

### `03-add`: add a menu item

Starts on the Main Menu editor. Before the capture: "Open the Main Menu editor" (section 3), from `snap-0`.

- `add-1`: click the button "Add Menu Item" (the window "Available Menu Items" opens: wait until `#modalMenuItems` is visible),
  `PauseAsync(2500)` so the three cards can be read, then `ClickAndWaitForNavigationAsync` on `a.thumbnail-link-create` in the card
  titled "Link Menu Item".
  On screen at the end: the form of a Link Menu Item with empty fields "Name", "Url" and "Target", and below them the
  "Permissions" picker (optional, left alone: the type carries `MenuItemPermissionPart`, `Menu/Views/MenuItemPermissionPart.Edit.cshtml`).
  Source: `MenuPart.Edit.cshtml:30-31,60`; `LinkMenuItemPart.Thumbnail.cshtml:1-2`; `Menu/Controllers/AdminController.cs:47-69`.
- `add-2`: `TypeAsync` in "Name": `Browse`; `TypeAsync` in "Url": `~/blog`; press Tab; then `HighlightAsync` the field "Target"
  (left empty), hold 2 seconds, `ClearHighlightsAsync`, and `HighlightAsync` the block of the "Permissions" picker (left empty):
  `scene.Page.Locator(".ocat-wrapper").Filter(new() { Has = scene.Page.Locator("#PermissionPicker") }).Last`, which has a size
  even before the picker is drawn. The narration names Target first and Permissions second, in that order.
  On screen at the end: Name "Browse", Url "~/blog", Target empty, and the block of the "Permissions" picker ringed.
  Source: `LinkMenuItemPart.Edit.cshtml:4,11,18`; `MenuItemPermissionPart.Edit.cshtml:13-19` (label "Permissions", hint
  "The permissions required to display this menu item (optional)."); `MenuShapes.cs:258-280` (an item is hidden from a user
  who lacks the selected permissions; with none selected it is shown to everyone).
- `add-3`: `ClearHighlightsAsync`; `HighlightAsync` the button "Publish" of the form and hold. No click yet.
  On screen at the end: the filled form with the button "Publish" ringed.
  Source: `Contents/Views/Content.PublishButton.cshtml:7`; `Menu/Controllers/AdminController.cs:71-139` (the form adds the
  item to a draft of the menu and does not publish the menu).
- `add-4`: `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` on the button "Publish"; wait until the row "Browse"
  is visible in `#menu`; `HighlightAsync` that row.
  On screen at the end: the Main Menu editor with the rows Home, About, Browse in this order and the row "Browse"
  ringed (its address `~/blog` is shown in the row).
  Source: `Menu/Controllers/AdminController.cs:137-139` (draft saved, redirect to the editor); `MenuItem.Admin.cshtml`.

### `04-arrange`: nest and reorder

Starts on the Main Menu editor. Before the capture: "Open the Main Menu editor" (section 3), from `snap-3`. The
rows are Home, About, Browse.

- `arrange-1`: `HighlightAsync` the button "Add" in the row "Browse" (not the "Add Menu Item" button), hold 1.5 seconds,
  `ClearHighlightsAsync`, click that button "Add", wait until `#modalMenuItems` is visible.
  On screen at the end: the window "Available Menu Items" with its three cards.
  Source: `MenuItem.Admin.cshtml:39-40`; `Menu/Assets/ts/menu-part.ts` (the window passes the item to the link of the
  card, which adds `menuitemid` to the address).
- `arrange-2`: `ClickAndWaitForNavigationAsync` on `a.thumbnail-link-create` in the card titled "Link Menu Item";
  `TypeAsync` in "Name": `Categories`; `TypeAsync` in "Url": `~/categories`; press Tab.
  On screen at the end: the Link Menu Item form with Name "Categories" and Url "~/categories" (Target and the "Permissions"
  picker untouched).
  Source: `LinkMenuItemPart.Edit.cshtml:4,11`; `Menu/Controllers/AdminController.cs:71-139`.
- `arrange-3`: `ClickAndWaitForNavigationAsync` on the button "Publish"; wait until the row "Categories" is visible, indented
  under "Browse" (`data-depth="1"`). Then drag "Browse" above "About" with the mouse, in this exact way:
  1. `var box = await browseTitleBar.BoundingBoxAsync(); var startX = box.X + 60; var startY = box.Y + box.Height / 2;`
     where `browseTitleBar` is `.menu-item-title` of the row "Browse" (the point is 60 pixels from its left edge, on the
     name, away from the buttons, which are not a drag handle); then `await scene.MoveToAsync(startX, startY);`
  2. `await scene.Page.Mouse.DownAsync();`
  3. `await scene.MoveToAsync(startX, startY + 10, 6);` (starts the drag; the row "Categories" leaves the list while the
     row "Browse" is dragged, and comes back with it);
  4. `var aboutBox = await aboutRow.BoundingBoxAsync();` where `aboutRow` is `li.menu-item` with the text "About", read
     after step 3; then `await scene.MoveToAsync(startX, aboutBox.Y + 6, 30);` (the same x as the start: a sideways
     move of more than 40 pixels would change the level);
  5. `await scene.PauseAsync(300); await scene.Page.Mouse.UpAsync();`
  Wait until the rows read Home, Browse, Categories, About, with "Categories" at `data-depth="1"`.
  On screen at the end: the list in that order, "Categories" indented under "Browse".
  Source: `Menu/Assets/ts/menu.ts` and the script it loads (`wwwroot/Scripts/menu/menu.js`: the handle is
  `.menu-item-title`, a move of more than 40 pixels sideways changes the level, the children travel with the dragged item).
- `arrange-4`: `ClickAndWaitForNavigationAsync` on `button.draft` (label "Save Draft"); wait until the page shows the message
  "Your Menu draft has been saved." and the list again.
  On screen at the end: the editor with the message and the order Home, Browse, Categories, About. The editor was
  reached through the redirect that follows an added item, so it has no `returnUrl`: Save Draft is a single button and
  the editor is shown again (section 3).
  Source: `Contents/Controllers/AdminController.cs:538-539`, `Content.SaveDraftButton.cshtml:7`;
  `Menu/Views/MenuPart.Edit.cshtml:37` (the order is posted with the form).
- `arrange-5`: `HighlightAsync` the buttons "Edit" and "Delete" of the row "Categories"; hold 1 second; `ClearHighlightsAsync`; click
  "Delete" of that row; wait until `#confirmRemoveModal` is visible (title "Delete", text "Are you sure you want to remove
  this element?", buttons "Ok" and "Cancel"); `PauseAsync(3000)`; click `#modalCancelButton`; wait until the
  window is gone. Never click "Ok".
  On screen at the end: the list unchanged, with "Categories" still under "Browse".
  Source: `MenuItem.Admin.cshtml:31-49`; `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`;
  `Menu/Controllers/AdminController.cs:243-279` (Delete removes the item with the items inside it).

### `05-publish`: publish a menu

Starts on `/`, signed in as admin. Before the capture: `OpenAsync("/")` from `snap-4`; wait until `#mainNav` shows
"Home" and "About" and no "Browse".

- `publish-1`: `HighlightAsync` the links in `#mainNav`.
  On screen at the end: the front page whose navigation bar lists only Home and About, ringed.
  Source: `Menu/MenuShapes.cs:51` (the published version is read, not the draft).
- `publish-2`: `ClearHighlightsAsync`; `NavigateAsync(MenusUrl)`; wait until the row "Main Menu" is visible; `HighlightAsync` its
  two badges "Published" and "Draft"; hold 1.5 seconds; `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` on the
  button `a.edit` of that row (the narration ends with "Let's open it with the Edit button"); wait until "Add Menu Item"
  is enabled.
  On screen at the end: the editor of the Main Menu, opened from the list.
  Source: `Contents/Views/ContentsTags_SummaryAdmin.cshtml:12,19`; `ContentsButtonEdit_SummaryAdmin.cshtml:13`.
- `publish-3`: starts in the editor that `publish-2` opened. `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` on the
  left part of the split button, the one labeled
  "Publish" (`button.publish` that is a submit button, not the drop-down toggle); wait until the Menus page shows
  "Your Menu has been published." and the row "Main Menu" has only the badge "Published".
  On screen at the end: the Menus page with the message and a single badge "Published" on "Main Menu".
  Source: `Contents/Controllers/AdminController.cs:966-967` (message); `Content.PublishButton.cshtml:19`; the
  page has a returnUrl, so Publish returns to the list (`Contents/Controllers/AdminController.cs:841-858`, `EditInternalAsync`).
- `publish-4`: `NavigateAsync("/")`; wait until `#mainNav` shows "Home", "Browse" and "About" in this order; click "Browse"
  in `#mainNav`; wait until the drop-down shows "Categories".
  On screen at the end: the front page with the drop-down list under "Browse" showing "Categories".
  Source: `TheBlogTheme/Views/MenuItem.liquid`, `MenuItemLink-LinkMenuItem.liquid`; the publish clears the cache of the
  menu (`Alias/Handlers/AliasPartHandler.cs:59-62`). If the navigation bar still shows the old menu, stop and read
  OPEN-QUESTIONS.md, question 8. If the click on "Browse" opens the blog page instead of a drop-down list, Bootstrap's
  script did not load in the recorder's browser: read question 14.

### `06-create`: create a menu

Starts on the Menus page. Before the capture: `OpenAsync(MenusUrl)` from `snap-5`; wait until the row "Main Menu" is visible.

- `create-1`: `ClickAndWaitForNavigationAsync` on the button "New Menu"; wait until the field "Title" is visible.
  On screen at the end: an empty editor: Title and Alias empty, the button "Add Menu Item" disabled.
  Source: `Contents/Views/ContentsAdminListCreate.cshtml:14`; `Menu/Views/MenuPart.Edit.cshtml:19,30`.
- `create-2`: `TypeAsync` in "Title": `Footer Menu`; `HighlightAsync` the field "Alias" (left empty) for 3 seconds;
  `ClearHighlightsAsync`; `HighlightAsync` the disabled button "Add Menu Item".
  On screen at the end: Title "Footer Menu", Alias empty, the button "Add Menu Item" ringed and disabled.
  Source: `Alias/Views/AliasPart.Edit.cshtml:15-18` (hint "Leave empty to re-generate it."); `MenuPart.Edit.cshtml:19`.
- `create-3`: `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` on the left part of the split button, the one labeled
  "Publish"; wait until the Menus page shows "Your Menu has been published." and the rows "Main Menu" and "Footer Menu".
  On screen at the end: the Menus page with the message and both rows.
  Source: `Contents/Controllers/AdminController.cs:472-473`, `Contents/Controllers/AdminController.cs:769-798` (`CreateInternalAsync` returns to the returnUrl).
- `create-4`: `ClickAndWaitForNavigationAsync` on `a.edit` of the row "Footer Menu"; wait until the field "Alias" shows
  `footer-menu`; `HighlightAsync` the field "Alias" for 3 seconds, `ClearHighlightsAsync`, `HighlightAsync` the button
  "Add Menu Item" (now enabled).
  On screen at the end: the editor of "Footer Menu", Alias "footer-menu", the enabled button "Add Menu Item" ringed.
  Source: `Menu/Migrations/menu.recipe.json:31` (the alias pattern), `Alias/Handlers/AliasPartHandler.cs:90-130`.

## 5. After each take

Run `review.py` on the clip and look at every contact sheet: the frame at the end of each step must show what its
narration says, with no error message and no empty list. Take the snapshot named above after `03-add`, `04-arrange`
and `05-publish`.

## 6. Checks for private data

The site is fresh and every value is fake or comes from the Blog recipe, so no mask script is needed. Before publishing,
OCR the video at one frame per second, at every scene change and at 5 frames per second, and search the text for
anything that is not one of these:

- the names "Home", "About", "Browse", "Categories", "Footer Menu", "Main Menu", the site name "Orchard Core" (the brand
  of the navigation bar), the user name `admin` and the address `admin@example.com`;
- the addresses `localhost`, `~/blog`, `~/categories`, `main-menu`, `footer-menu`;
- the sample text of the Blog recipe on the front page.

The password must appear nowhere (no sign-in is recorded). The address bar is not part of the capture; if a take shows
a port or a path other than the ones above, record it again.

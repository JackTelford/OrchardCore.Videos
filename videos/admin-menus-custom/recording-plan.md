# Recording plan: Admin Menus: Add Your Own Links to the Admin Navigation

Read this file and nothing else to record the video. Every label below is the one the Orchard Core source spells, with
the file and line where it comes from (paths are under `src/` of `OrchardCMS/OrchardCore` at commit `92db4a83`). A
selector written here stands for `scene.Page.Locator(...)`. Narrate first (`narrate.py`), then record: the recorder
holds each step for the length of its narration.

The video has three slide steps (nothing to record) and six recorded clips, `02-list` to `07-access`.

## 1. The site it needs

| What | Value |
|---|---|
| Site | `src/OrchardCore.Cms.Web` of an Orchard Core checkout at `92db4a83` or later, on `http://localhost:5010`, with its own `App_Data` (`ORCHARD_APP_DATA`) outside both repositories. |
| Setup | Auto Setup, so no setup form and no sign-in form are recorded. Tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, database provider `Sqlite`, recipe **Blog**, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`). |
| Recipe | **Blog** (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). This video needs it: the recipe enables the Admin Menu, Contents, Lists, Media, Roles and Features features (the `feature` step, `blog.recipe.json:31-80`), creates the roles Moderator, Editor, Author and Contributor (line 88), the content types Blog (with a list part) and Blog Post and Page, one blog named Blog, and a menu named `Admin menus` (the `AdminMenu` step, line 1188). |
| Features to enable by hand | None. Open `/Admin/Features`, search `Admin Menu`: the feature is enabled. The Content Types node and the Lists node need no extra feature (they come with Contents and Lists, which the recipe enables). |
| Settings | Leave the admin settings at their defaults: the lists use the List layout and the row actions use Buttons (`AdminListOptions.DefaultLayout` and `DefaultActionsLayout`, `OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`). Do not set `OrchardCore:Admin:List` anywhere. The admin theme is TheAdmin (set by the recipe). |
| Outbound calls | None are made. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address anyway. |
| External services | None. |
| Browser | The recorder's Chromium, 1536x864, light color scheme, `en-US`. The left admin menu (`#left-nav`) must be visible at that width; if it is collapsed, expand it before `StartCaptureAsync` in every clip. |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD` once, outside the capture. `state.json` stays out of git. |

### Seed data (outside the capture, once, before snapshot 0)

All fake. The only seed is one more blog, so the Lists node has two items to list.

1. Open `/Admin/Contents/ContentTypes/Blog/Create` (route `Contents/ContentTypes/{id}/Create`,
   `OrchardCore.Contents/Controllers/AdminController.cs:407`).
2. Type the title `Travel Notes`, leave everything else empty, and select **Publish**.
3. Open `/Admin/Contents/ContentItems/Blog` and check that the list shows two blogs, `Blog` and `Travel Notes`.
4. Open `/Admin/Roles/Index` and check that the role `Author` is listed (the recipe creates it). The address needs the
   action: the route is `Roles/{action}/{id?}` with the action written in (`OrchardCore.Roles/Controllers/AdminController.cs:22`,
   `OrchardCore.Admin/AdminAreaControllerRouteMapper.cs:47`), so `/Admin/Roles` alone is not a page.
5. Open `/Admin/AdminMenu/List` and check that it lists exactly one menu, `Admin menus`, with the badge `Enabled`.

Then stop the site and copy `App_Data` and `state.json` aside as **snapshot 0**.

## 2. Order of the clips, and what each one changes

The clips depend on each other, and they play in the order they are recorded.

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-list` | snapshot 0 | Nothing. |
| 2 | `03-create` | snapshot 0 | Creates the menu `Editor Shortcuts`. |
| 3 | `04-link` | after `03-create` | Adds the link `Media Library`. |
| 4 | `05-group` | after `04-link` | Adds the placeholder `Editorial` and moves `Media Library` into it. |
| 5 | `06-dynamic` | after `05-group` | Adds a Content Types node and a Lists node, and moves both into `Editorial`. |
| 6 | `07-access` | after `06-dynamic` | Sets the permission `Manage Media` on `Media Library`, grants a permission to the role `Author`, disables the menu and enables it again. |

Snapshots: after each clip, stop the site and copy `App_Data` and `state.json` aside as `after-03`, `after-04`,
`after-05`, `after-06`. To record one clip again, stop the site, restore the snapshot it starts from (the one in the
"Starts from" column), start the site, and record only that clip. To record everything again, restore snapshot 0 and
run the clips in the order above. A clip that fails halfway leaves the site changed: always restore before the next take.

The slide steps `ideas-1`, `ideas-2` (clip `01-ideas`) and `reuse-1` (clip `08-reuse`) need no site.

## 3. Common rules for the recorded clips

- `OpenAsync` and every action before `StartCaptureAsync` are not recorded. Use them to open the starting page and to
  reach a state the story does not show.
- Expand and collapse the left menu groups before `StartCaptureAsync` unless the step opens them. A menu group
  remembers what it was last left open as: before `02-list`, make sure the group **Tools** is collapsed.
- Every action that loads a page goes through `ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync` or
  `NavigateAsync`. This includes the drag of a node (the page reloads when the move succeeds).
- Type with `scene.TypeAsync` (delay 60). Press Tab after typing in a text box that the story does not leave by a click.
- Success messages appear at the top of the page after a save. Let them show: do not dismiss them.
- The menu id in the addresses is random. Reach the tree of a menu by clicking **Edit Nodes** on its row, never by a typed address.
- Highlights (`HighlightAsync`) are fixed on the page: call `ClearHighlightsAsync` before scrolling or navigating.

## 4. The clips

### Slide steps

| Step | What the slide shows |
|---|---|
| `ideas-1` | Heading "Two ideas": three cards, "Admin menu", "Admin node" and "Merged into the admin menu". |
| `ideas-2` | Heading "Four kinds of node": four cards, "Link Admin Node", "Placeholder Admin Node", "Content Types" and "Lists". |
| `reuse-1` | Heading "Reuse your menus on other sites": two cards, "Deployment plan" and "Recipe". |

### Clip `02-list` (chapter 02, "The Admin Menus page")

Starts on `/Admin`, signed in as `admin`, from snapshot 0. `Tools` collapsed. Nothing is changed.

**`list-1`**
- Actions: click the left menu item **Tools** (`#left-nav .item-label` with the text `Tools`); its group opens. Click
  **Admin Menus** in that group with `ClickAndWaitForNavigationAsync`. Then `HighlightAsync` the group's item **Features**
  with the caption `Enable the feature here` (it is in the same group, `Tools`).
- On screen when the narration ends: the Admin Menus page with the title **Admin Menus** and the button **Add Admin Menu**; the left
  menu with **Tools** open, **Admin Menus** active, and a green ring around **Features**.
- Labels: Tools, Admin Menus: `OrchardCore.Modules/OrchardCore.AdminMenu/AdminMenu.cs:25-26`; Features:
  `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; the Blog recipe enables the feature:
  `blog.recipe.json:49`.

**`list-2`**
- Actions: `ClearHighlightsAsync`. Move to the row **Admin menus**. `HighlightAsync` its badge **Enabled**; then the
  three buttons **Edit Nodes**, **Edit** and **Delete** (one ring at a time, 800 ms apart). Finish by clicking the dropdown button
  **Actions** of that row: the menu shows **Disable**. Do not choose it.
- On screen when the narration ends: the row **Admin menus** with the open Actions menu showing **Disable**.
- Labels: Enabled / Disabled: `Views/AdminMenuEntry.DefaultTags.SummaryAdmin.cshtml:8`; Edit Nodes, Edit, Delete:
  `Views/AdminMenuEntry.Buttons.SummaryAdmin.cshtml:6,8,10`; Actions: `OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml`
  (the dropdown of the Buttons layout); Disable / Enable: `Views/AdminMenuEntry.ActionsMenuItems.SummaryAdmin.cshtml:7`.
  The menu name `Admin menus`: `blog.recipe.json:1192`.

**`list-3`**
- Actions: press Escape to close the Actions menu. Click **Edit Nodes** on the row **Admin menus** with
  `ClickAndWaitForNavigationAsync`. The page **Edit Nodes for 'Admin menus'** lists the tree. `HighlightAsync` the rows
  **Blog** and **Main Menu** (800 ms apart), then the row **Content**; then `HighlightAsync` the entries **Blog** and **Main Menu** in the left menu.
- On screen when the narration ends: the tree with the rows Blog (label Link), Main Menu (label Link), Content (label
  Placeholder) and, inside Content, the rows Content Items (Link) and a Content Types row with the badges Article,
  Page and Taxonomy; the left menu with rings around Blog and Main Menu.
- Labels: the page title `Edit Nodes for '{0}'`: `Views/Node/List.cshtml:16`; the row labels Link and Placeholder:
  `Views/Items/LinkAdminNode.Fields.TreeSummary.cshtml`, `Views/Items/PlaceholderAdminNode.Fields.TreeSummary.cshtml:11`;
  the nodes: `blog.recipe.json:1196-1270`.

### Clip `03-create` (chapter 03, "Create a menu")

Starts on `/Admin/AdminMenu/List`, from snapshot 0. Changes the site.

**`create-1`**
- Actions: click **Add Admin Menu** with `ClickAndWaitForNavigationAsync`. On the page **Create Admin Menu** type
  `Editor Shortcuts` into the field **Name** (`input[name="Name"]`). Click **Create** (`button.create`) with
  `ClickAndWaitForNavigationAsync`.
- On screen when the narration ends: the Admin Menus list with the rows **Admin menus** and **Editor Shortcuts**.
- Labels: Add Admin Menu: `Views/Menu/List.cshtml:25`; Create Admin Menu, Name, Create: `Views/Menu/Create.cshtml:4,13,21`.

**`create-2`**
- Actions: `HighlightAsync` the badge **Enabled** of the row **Editor Shortcuts**, and hold.
- On screen when the narration ends: the row **Editor Shortcuts** with the badge **Enabled** ringed; the left menu
  shows no new entry, because the menu has no nodes.
- Labels / behavior: the badge: `Views/AdminMenuEntry.DefaultTags.SummaryAdmin.cshtml:8`; a menu is built into the
  admin menu only when it is enabled and has a node: `Services/AdminMenuNavigationProvidersCoordinator.cs:43`.

### Clip `04-link` (chapter 04, "Add a link")

Starts on `/Admin/AdminMenu/List`, from `after-03`. Changes the site.

**`link-1`**
- Actions: click **Edit Nodes** on the row **Editor Shortcuts** (`ClickAndWaitForNavigationAsync`). The tree shows the
  message **Nothing here! There are no nodes on this tree for the moment.** Click **Add Node**: the dialog
  **Available Admin Nodes** opens with four cards (**Link Admin Node**, **Placeholder Admin Node**, **Content Types**, **Lists**; the last two
  come from the Contents and Lists features, which the recipe enables: `Contents/Startup.cs:259`, `Lists/Startup.cs:72`).
  `HighlightAsync` the card **Link Admin Node** (text "Adds a link with
  a text and a url.").
- On screen when the narration ends: the open dialog **Available Admin Nodes**, the card **Link Admin Node** ringed.
- Labels: `Views/Node/List.cshtml:16,27,59,73`; the card: `Views/Items/LinkAdminNode.Fields.TreeThumbnail.cshtml:3-4`.

**`link-2`**
- Actions: click **Add** in the card **Link Admin Node** (`#modalTreeNodes .admin-node-thumbnail` with the title
  `Link Admin Node`, then `a.btn` with the text `Add`), with `ClickAndWaitForNavigationAsync`. The page **Create Node**
  opens with the heading **Link**. Type `Media Library` into **Link Text** (`input[name="LinkText"]`) and `Media` into
  **Link Url** (`input[name="LinkUrl"]`). `HighlightAsync` the hint under **Link Url**.
- On screen when the narration ends: both fields filled in; the hint "The url of the link. A link will be shown only
  if it or one of their children have a url. The url will be relative to the root of the admin site" ringed.
- Labels: `Views/Items/LinkAdminNode.Fields.TreeEdit.cshtml:14,19,28,31`; Create Node: `Views/Node/Create.cshtml:6`.

**`link-3`**
- Actions: `ClearHighlightsAsync`. Move to **Target** (`input[name="Target"]`) and `HighlightAsync` the field and its hint
  "The target attribute of the A tag, see more:". Leave the field empty.
- On screen when the narration ends: the **Target** field ringed, empty.
- Labels: `Views/Items/LinkAdminNode.Fields.TreeEdit.cshtml:36-45` (the suggested values appear in a native list, which
  the recorder does not draw).

**`link-4`**
- Actions: `ClearHighlightsAsync`. Click **Pick** in the **Icon** row (`button.icon-picker-trigger`): the dialog
  **Pick an icon** opens. Type `image` into its search box (`#iconPickerModal input.iconpicker-search`), and click the
  first icon of the grid (`#iconPickerModal .iconpicker-item`, first visible). The dialog closes and the small icon
  button next to **Pick** shows the chosen icon.
- On screen when the narration ends: the form with the chosen icon shown on the button left of **Pick**; the dialog is closed.
- Labels: Icon, Pick: `Views/Items/LinkAdminNode.Fields.TreeEdit.cshtml:50,61`; Pick an icon: `:99`; the hint "Select an icon
  to display when this link is a first level menu item.": `:73`. The picker: `Assets/js/admin-menu-icon-picker.js:10-16`.
  The narration says only that the icon shows beside the link text: `OrchardCore.Themes/TheAdmin/Views/NavigationItemText.cshtml:31-34`
  draws the icon at every level, while the hint on screen and the documentation say "first level" (see OPEN-QUESTIONS.md).

**`link-5`**
- Actions: click **Create** (`button.create`, `ClickAndWaitForNavigationAsync`). The tree shows the message **Admin node
  added successfully.** and the row **Media Library** with `Media` and the label **Link**. Click the entry **Media Library** in
  the left menu (`ClickAndWaitForNavigationAsync`): the media page opens.
- On screen when the narration ends: the media page of the admin (the page title is **Assets**, `OrchardCore.Media/Views/Admin/Index.cshtml:8`; the menu calls it **Media**,
  **Library**), with the entry **Media Library** and its
  icon in the left menu.
- Labels: the message: `Controllers/NodeController.cs:149`; Create: `Views/Node/Create.cshtml:25`. The media page is
  `OrchardCore.Modules/OrchardCore.Media/Controllers/AdminController.cs:43` (route `Media`).

### Clip `05-group` (chapter 05, "Group links")

Starts on the tree of `Editor Shortcuts` (before `StartCaptureAsync`: open `/Admin/AdminMenu/List` and click **Edit Nodes** on the
row **Editor Shortcuts**), from `after-04`. Changes the site.

**`group-1`**
- Actions: click **Add Node**. In the dialog click **Add** in the card **Placeholder Admin Node** ("Adds a menu item that
  acts as a container for others."), with `ClickAndWaitForNavigationAsync`. The page **Create Node** opens with the heading
  **Placeholder**. `HighlightAsync` the three rows **Link Text**, **Icon** and **Permissions** in turn (700 ms apart).
- On screen when the narration ends: the Placeholder form with **Link Text**, **Icon** and **Permissions**, and no address field.
- Labels: `Views/Items/PlaceholderAdminNode.Fields.TreeThumbnail.cshtml:3-4`; `Views/Items/PlaceholderAdminNode.Fields.TreeEdit.cshtml:13,18,27,61`.

**`group-2`**
- Actions: `ClearHighlightsAsync`. Type `Editorial` into **Link Text**. Click **Pick**, type `pen` in the search box of the
  dialog **Pick an icon**, click the first icon. Click **Create** (`ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the tree with the rows **Media Library** (Link) and, after it, **Editorial** (Placeholder), both at the same level.
- Labels: as `group-1`; the message **Admin node added successfully.**: `Controllers/NodeController.cs:149`.

**`group-3`**
- Actions: drag the row **Media Library** by its title bar (`li.menu-item[data-treenode-id] > .menu-item-title`) onto
  **Editorial**: `DragToAsync(from: the title bar of Media Library, to: the empty child list of Editorial
  (li > ol.menu-item-links), offsetX: 40, offsetY: 2)`. Wrap it in `WaitForNavigationAsync`: the page reloads when the move
  succeeds. If the drop does not nest (see OPEN-QUESTIONS.md), do the fallback described there instead.
- On screen when the narration ends: the tree with **Editorial** and, below it and indented, **Media Library**.
- Labels / behavior: drag and drop with SortableJS, one list per node, a reload after each move:
  `Assets/ts/admin-menu-node-list.ts:19-95`; the move: `Controllers/NodeController.cs:322`.

**`group-4`**
- Actions: click the entry **Editorial** in the left menu (it is a group; clicking it opens it). `HighlightAsync` the entry
  **Media Library** inside it.
- On screen when the narration ends: the left menu with **Editorial** (and its icon) open, and **Media Library** inside it, ringed.
- Labels / behavior: a placeholder with no item that has an address is not shown:
  `OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:262-282`.

### Clip `06-dynamic` (chapter 06, "Links that list content")

Starts on the tree of `Editor Shortcuts` (before `StartCaptureAsync`: open `/Admin/AdminMenu/List` and click **Edit Nodes** on the row
**Editor Shortcuts**), from `after-05`. Changes the site. Also collapse the group **Editorial** in the left menu before
`StartCaptureAsync`, because `group-4` of the previous clip left it open and the menu remembers it (`dynamic-3` clicks it to open it).

**`dynamic-1`**
- Actions: click **Add Node**; in the dialog click **Add** in the card **Content Types** ("Add a link for each one of the
  selected content types."), with `ClickAndWaitForNavigationAsync`. The page shows the heading **Content Types**, the check
  box **Show All Content Types** (leave it clear) and the card **Select Content Types**. `HighlightAsync` the check box
  **Show All Content Types**, then tick **Blog Post** and **Page** in the list (`input.content-type-checkbox`, labels
  `Blog Post` and `Page`). Click **Create** (`ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the tree with a row for the Content Types node showing the badges `BlogPost` and `Page` (a badge shows the technical name of the type, not its display name **Blog Post**), and the
  label **Content Types**.
- Labels: `OrchardCore.Modules/OrchardCore.Contents/Views/Items/ContentTypesAdminNode.Fields.TreeThumbnail.cshtml:3-4`;
  `...TreeEdit.cshtml:13,23,67,71`; the row: `...TreeSummary.cshtml:27,35` (the badge is `contentType.Name`, the label is **Content Types**).

**`dynamic-2`**
- Actions: click **Add Node**; in the dialog click **Add** in the card **Lists** ("Adds links for list content items"),
  with `ClickAndWaitForNavigationAsync`. In **Content type** (`select[name="ContentType"]`, a native list: use `scene.SelectAsync`) choose `Blog` (on this site the
  only content type that holds a list, so the only option besides "Select a content type":
  `OrchardCore.Lists/AdminNodes/ListsAdminNodeDriver.cs:79-88`). Clear the check box **Add parent menu item** (`input.add-parent-link-checkbox`, checked at first; the row
  **Icon for the parent menu item** collapses). Click **Create** (`ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the tree with a row `Blog` and the label **Lists**, after the Content Types row.
- Labels: `OrchardCore.Modules/OrchardCore.Lists/Views/Items/ListsAdminNode.Fields.TreeThumbnail.cshtml:3-4`;
  `...TreeEdit.cshtml:15,21,24,64,65,71`; the row: `...ListsAdminNode.Fields.TreeSummary.cshtml:22-23` (the display name of the type, then **Lists**).

**`dynamic-3`**
- Actions: drag the row of the Content Types node onto **Editorial** (same gesture as `group-3`; Editorial now has a child, so
  the drop target is the child list below **Media Library**), with `WaitForNavigationAsync`. Then drag the Lists row the same
  way. Then click **Editorial** in the left menu to open it.
- On screen when the narration ends: the left menu with **Editorial** open and its entries **Media Library**, **Blog Post**,
  **Page**, **Blog** and **Travel Notes** (in the order the site renders them; the narration does not depend on it).
- Labels / behavior: the Content Types node adds `Contents/ContentItems/<type>` links
  (`OrchardCore.Contents/AdminNodes/ContentTypesAdminNodeNavigationBuilder.cs:48-66`); the Lists node adds one link for
  each content item of the type (the latest version of each, at most 100, sorted by display text), and each link opens the
  admin **Display** page of the item, not its edit page (`OrchardCore.Lists/AdminNodes/ListsAdminNodeNavigationBuilder.cs:25,87-119`;
  `OrchardCore.Lists/Handlers/ListPartHandler.cs:14-20`). The query runs on every page of the admin: nothing sets cache metadata on the admin navigation shape
  (`OrchardCore.Admin/AdminMenuFilter.cs:58-63`), so the dynamic cache does not hold it.

### Clip `07-access` (chapter 07, "Control who sees a menu")

Starts on the tree of `Editor Shortcuts` (before `StartCaptureAsync`: open `/Admin/AdminMenu/List` and click **Edit Nodes** on the row
**Editor Shortcuts**), from `after-06`. Changes the site.

**`access-1`**
- Actions: click **Edit** on the row **Media Library** (it is inside **Editorial**; `a.btn-primary` with the text `Edit`
  in that row), with `ClickAndWaitForNavigationAsync`. The page **Edit Node** opens. Click the field **Permissions**
  (`#PermissionPicker`), type `Manage Media`, and choose the option whose text is exactly **Manage Media** (not "Manage Media
  Profiles" or another). A chip **Manage Media** shows in the field. Click **Save** (`button.save`, `ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the tree with the message **Admin node updated successfully.** (the chip is visible
  for at least 1.5 s before Save).
- Labels: Edit Node, Permissions, Save: `Views/Node/Edit.cshtml:6,45`, `Views/Items/LinkAdminNode.Fields.TreeEdit.cshtml:87`;
  the hint "The permissions required to display this menu item (optional).": `:89`; Manage Media:
  `OrchardCore/OrchardCore.Media.Core/MediaPermissions.cs:16`; the message: `Controllers/NodeController.cs:231`.

**`access-2`**
- Actions: `NavigateAsync("/Admin/Roles/Index")` (or choose **Access Control**, **Roles** in the left menu). Click **Edit** on the row **Author** (`a.edit`, `ClickAndWaitForNavigationAsync`). The page
  **Edit 'Author' Role** opens. Type `View Admin Menu` into the box **Search permissions** (`#search-box`). The rows
  **View Admin Menu - Editor Shortcuts**, **View Admin Menu - Admin menus** and **View Admin Menu - View All** remain. Tick the
  check box in the **Allow** column of **View Admin Menu - Editor Shortcuts**
  (`tr[data-text="View Admin Menu - Editor Shortcuts"] input[name^="Checkbox."]`). Click **Save**
  (`button.save`, `ClickAndWaitForNavigationAsync`).
- On screen when the narration ends: the list of roles with the message **Role updated successfully.** (the ticked row is
  visible for at least 1.5 s before Save).
- Labels: Roles, in the group **Access Control**: `OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19`; Edit:
  `Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`; the page title `Edit '{0}' Role`: `Views/Admin/Edit.cshtml:6`; Search
  permissions, Permission, Allow, Effective: `Views/Admin/_RolePermissions.cshtml`; the permission names:
  `OrchardCore.AdminMenu/Permissions.cs:11` ("View Admin Menu - {0}") and
  `OrchardCore.AdminMenu.Abstractions/AdminMenuPermissions.cs:8,10` ("Manage the admin menu", "View Admin Menu - View All");
  the message: `OrchardCore.Modules/OrchardCore.Roles/Controllers/AdminController.cs:213`. The narration says the role sees the menu items its permissions allow: the permission only lets the menu be built for the
  role, and every item then checks its own permissions (`Services/AdminMenuNavigationProvidersCoordinator.cs:47-52`,
  `OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:208-257`). The clip does not sign in as **Author**. The Administrator role is not edited: its page says
  "This system role provides comprehensive access by granting all available permissions." (`_RolePermissions.cshtml`).

**`access-3`**
- Actions: `NavigateAsync("/Admin/AdminMenu/List")`. Before the click, `HighlightAsync` the entry **Editorial** in the left
  menu, then `ClearHighlightsAsync`. Click **Actions** in the row **Editor Shortcuts** and choose **Disable**
  (`ClickAndWaitForNavigationAsync`): the message **Admin menu toggled successfully.** shows, the badge reads **Disabled**,
  and **Editorial** is gone from the left menu. Click **Actions** again and choose **Enable** (`ClickAndWaitForNavigationAsync`):
  the badge reads **Enabled** and **Editorial** is back.  The switch of a single node, which the narration mentions at the end, is not recorded.
- On screen when the narration ends: the list with **Editor Shortcuts** **Enabled** and **Editorial** in the left menu. The
  frame in the middle of the step shows **Disabled** and no **Editorial**.
- Labels: Actions, Disable, Enable: `OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml`, `Views/AdminMenuEntry.ActionsMenuItems.SummaryAdmin.cshtml:7`;
  the message: `Controllers/MenuController.cs:303`; the node switch the narration mentions (a dropdown beside **Delete**
  with **Disable** or **Enable**): `Views/MenuItem.TreeSummary.cshtml:19,50-58`.

## 5. Checks for private data

- The site is fresh, so no mask script is needed. Every name on screen is fake: `admin`, `Editor Shortcuts`, `Editorial`,
  `Media Library`, `Travel Notes`, `Author`.
- No password is typed on camera. `admin@example.com` is not shown by any of these pages; if the user menu shows it, record again
  with the user menu closed.
- The tree rows show random identifiers in the addresses of the recipe's nodes (`Admin/Contents/ContentItems/<id>/Display`).
  They are generated by the setup and identify nothing.
- OCR the finished video at one frame per second and at every scene change, and search the text for `@`, real
  names, host names other than `localhost`, and anything that looks like a token.
- Delete the site, its `App_Data` copies and `state.json` after the video is published.

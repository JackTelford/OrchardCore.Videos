# Open questions: Admin Menus: Add Your Own Links to the Admin Navigation

Every sentence of the narration was read from the source of Orchard Core at commit `92db4a83` (the file and line of each
label are in `recording-plan.md`). Nothing in the narration is an open question. What follows is what could not be
checked without a running site: how the screens behave while recording, and a pronunciation. Check each one before
the first take; if the answer changes a narration or a "what must be visible" line, change it and tell the maintainer.

## 1. Where the custom entries land in the left menu

**Checked in the source:** a node with no position gets the position `null` (`OrchardCore/OrchardCore.Navigation.Core/MenuItem.cs`),
which `FlatPositionComparer` sorts as `before.` (`OrchardCore/OrchardCore.DisplayManagement/Zones/FlatPositionComparer.cs`,
`GetNormalizedSpan`). That suggests the custom top-level entries are drawn before the standard ones that have a number as position.
**To confirm:** in `04-link` (`link-5`) and `05-group` (`group-4`), where **Media Library** and **Editorial** appear in the left menu
and that the menu is not so long that the entry is outside the 1536x864 frame (scroll the left menu if it is, with
`ScrollAsync`, and clear highlights first). The narration never says where the entries sit.

## 2. The recipe's own menu in the left menu (`list-3`)

**Checked in the source:** the Blog recipe's menu `Admin menus` has the links **Blog** and **Main Menu** and the placeholder
**Content** (`blog.recipe.json:1188-1270`). Entries with the same text are merged into one by the navigation manager
(`NavigationManager.Merge`), so the placeholder **Content** joins the standard **Content** group.
**To confirm:** the left menu shows **Blog** and **Main Menu** as entries of their own (the narration says "the admin menu on the left
shows these entries"), and that **Content** shows once, not twice.

## 3. Dropping a node into an empty placeholder (`group-3`)

**Checked in the source:** the tree is drag and drop with SortableJS, one list per node, the child list of a node is always
rendered even when empty, and a drop sends one request (`MoveNode`) and reloads the page
(`Assets/ts/admin-menu-node-list.ts`).
**To confirm:** an empty child list has no height, so the drop target is thin. Try `DragToAsync` onto the empty list with a small
vertical offset (2 to 6 px below the title bar of **Editorial**). If the node does not nest, either move the pointer by hand
with `scene.Page.Mouse` (move to the title bar, press, move in 10 steps to the line below **Editorial**, release), or send the
same request the page sends, from the page and outside the capture, before the step, and record the step as the reload that
follows. The request is a POST to `/Admin/AdminMenu/Node/MoveNode` with the form fields `__RequestVerificationToken` (read it
from `input[name='__RequestVerificationToken']`), `treeId` (`#menu[data-tree-id]`), `nodeToMoveId` (the `data-treenode-id` of
the node's `li`), `destinationNodeId` (the `data-treenode-id` of **Editorial**'s `li`) and `position` (0). The result must be
the same tree as a drag.

## 4. The icon picker (`link-4`, `group-2`)

**Checked in the source:** the dialog **Pick an icon** holds an inline Font Awesome picker with a search box of the class
`iconpicker-search`, and choosing an icon closes the dialog (`Assets/js/admin-menu-icon-picker.js`).
**To confirm:** the selectors `#iconPickerModal input.iconpicker-search` and `#iconPickerModal .iconpicker-item`, that typing
`image` or `pen` leaves at least one icon, and that the icon shows on the button next to **Pick** after the dialog closes.
Pick another word if a search finds nothing; the narration does not name the icon.

## 5. The permission picker (`access-1`)

**Checked in the source:** the field **Permissions** is a multi-select (`#PermissionPicker`) with the texts "Type to search",
"Select", "Remove" and "No result found" (`Services/AdminMenuJSLocalizer.cs`), and it lists every permission by its description;
the description of the permission for the media library is **Manage Media** (`OrchardCore.Media.Core/MediaPermissions.cs:16`).
**To confirm:** how to open the list and pick the option whose text is exactly **Manage Media** (other options start with the same
words), and that the chosen permission shows as a chip before **Save**.

## 6. The role page shows the menu's permission at once (`access-2`)

**Checked in the source:** each menu adds the permission **View Admin Menu - <menu name>** to the permission list
(`Permissions.cs`), and the role page lists the permissions of every provider (`Views/Admin/_RolePermissions.cshtml`).
**To confirm:** after `03-create`, the page **Edit 'Author' Role** lists **View Admin Menu - Editor Shortcuts** (the list of menus
is cached; the page should show the menu created before). If it does not, restart the site once after `after-06` and say so here.
Also confirm that the role **Author** has none of the three **View Admin Menu** permissions ticked before the step, so that
the tick on camera is a change.

## 7. The pronunciation of "Url"

The labels **Link Url** are spoken through the storyboard's `spoken` map as "U.R.L." (`"Url": "U.R.L."`). Per the skill, a person must
confirm the spelling with `scripts/pronounce.py` before narrating; the voice may read it as a word otherwise.

## 8. Sample data of the Lists node

**To confirm:** with the blogs **Blog** and **Travel Notes**, the Lists node (parent item cleared) puts two entries, **Blog** and
**Travel Notes**, in the group **Editorial**, and no entry of the same text as an entry of the standard menu or of the recipe's menu
is merged with them at that level (`dynamic-3`). The entry **Blog** of the recipe's menu is at the top level, so it is not at the same level.

## 9. The icon of a link inside a group (`link-4`, `group-4`)

**Checked in the source:** the narration says the icon shows beside the link text in the admin menu. The hint on screen ("Select an icon
to display when this link is a first level menu item.", `Views/Items/LinkAdminNode.Fields.TreeEdit.cshtml:73`) and the documentation say
"first level". The template of the admin menu draws the icon of any item that has an icon class, at every level
(`OrchardCore.Themes/TheAdmin/Views/NavigationItemText.cshtml:31-34`), so the source does not support "first level only".
**To confirm:** after `group-3`, whether **Media Library** keeps its icon inside **Editorial** in the left menu. The narration of
`group-4` does not mention an icon, so nothing needs to change either way; if the icon is missing, tell the maintainer, because the
hint and the documentation would then be right and the template would not explain it.

## 10. A blog added later (`dynamic-3`)

**Checked in the source:** the Lists node queries the latest version of the content items of the type each time the admin menu is built, and
nothing caches the admin menu (`OrchardCore.Lists/AdminNodes/ListsAdminNodeNavigationBuilder.cs:112-119`, `OrchardCore.Admin/AdminMenuFilter.cs`).
**To confirm:** after recording, publish one more blog and open any admin page: its link is in **Editorial** without any change to the
menu. The sentence "A blog added later gets its link without any change to the menu" depends on it.


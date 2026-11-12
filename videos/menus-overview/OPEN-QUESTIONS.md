# Open questions: Menus overview

The narration says only what the source of Orchard Core (commit `92db4a83`) and its documentation show. The points below
were read from the code but not seen on a running site. A person confirms each one while recording the clip named in
brackets; if the site disagrees, the narration of the step named changes, not the site.

Paths are relative to `src/` of the Orchard Core checkout.

## Narration claims to confirm on screen

1. **The admin menu entry "Content", "Menus" on a Blog site** (`open-2`). `Menu/AdminMenu.cs:33-34` adds Content, Menus.
   The Blog recipe also creates its own admin menu nodes: a top-level "Blog", a top-level "Main Menu" that opens the
   menu editor directly, and a "Content" node with "Content Items" and entries for the content types Article, Page and Taxonomy
   (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:1188-1307`, step `AdminMenu`). Confirm that the entry
   "Menus" is still reachable under "Content", whether the sub menu opens on a click or on hover, and what the
   recorder must click. The plan says "click Content, then Menus"; adapt the plan, not the narration, if the sub menu
   differs.
2. **The sentence about Tools, Features** (`open-2`). `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27` puts "Features" under "Tools", and the
   manifest names the module "Menu" (`Menu/Manifest.cs`). Confirm that the Features page lists a feature named "Menu",
   and that enabling it makes "Menus" appear under "Content" for the administrator.
3. **The buttons of the Link Menu Item form** (`add-3`, `arrange-2`). The narration names a "Publish" button that adds the
   item to the menu and does not publish the menu. Source: the form is the Menu module's `Admin/Create.cshtml`, its
   buttons come from `Contents/Drivers/ContentsDriver.cs:EditAsync` ("Publish" always, "Save Draft" only for a draftable
   type, "Unpublish" only for a published item, "Delete" only for a stored item), the type `LinkMenuItem` is not draftable
   (`Menu/Migrations/menu.recipe.json`), and the post adds the item to a draft of the menu
   (`Menu/Controllers/AdminController.cs:71-139`). Confirm that the form shows "Publish" and nothing else, and that the
   label is "Publish" (the form has no returnUrl).
4. **Draft and published** (`add-4`, `publish-1`, `publish-2`, `publish-3`). Adding an item saves a draft of the menu
   (`SaveDraftAsync`, `AdminController.cs:137`); the theme reads the published version (`Menu/MenuShapes.cs:51`).
   Confirm: after `03-add` the Menus page shows the badges "Published" and "Draft" on Main Menu, the front end still
   shows Home and About, and after Publish the row has only "Published".
5. **The drag** (`arrange-3`). The sortable list is `#menu` (`wwwroot/Scripts/menu/menu.js`): the handle is
   `.menu-item-title`, the list is a flat list whose level is `data-depth`, a drag that moves the pointer more than 40
   pixels to the right or left changes the level by one, and the children of the dragged item travel with it. Confirm
   that the mouse sequence of the plan (down, 10 pixels, move to the top of the row "About", up) reorders the list on the
   recorder's Chromium and that "Categories" follows "Browse". Confirm, outside the capture, that a move of about 80
   pixels to the right nests an item under the one above, because the narration says "dragging sideways changes its
   level".
6. **The order is stored by Save Draft** (`arrange-4`). The reorder only fills a hidden field (`Hierarchy`), which the
   Menu part reads when the menu is saved (`Menu/Drivers/MenuPartDisplayDriver.cs:UpdateAsync`). Confirm that after
   Save Draft the order Home, Browse, Categories, About is kept, and that the message "Your Menu draft has been saved."
   shows (`Contents/Controllers/AdminController.cs:538-539`).
7. **The confirmation of Delete** (`arrange-5`). The link has `data-url-af="RemoveUrl UnsafeUrl"`, so the admin theme
   shows a dialog with the title "Delete", the text "Are you sure you want to remove this element?" and the buttons "Ok"
   and "Cancel" (`OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`, `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts`). Confirm that the dialog
   shows on this page and that "Cancel" leaves the item in place. The narration also says that Delete removes the items
   inside the item; the code does (`AdminController.cs:243-279`), confirm it outside the capture with a spare item.
8. **The menu is live after Publish** (`publish-4`). The Blog theme caches the menu for five minutes under the cache
   tag `alias:main-menu` (`TheBlogTheme/Views/Layout.liquid:32`) and publishing a menu clears that tag
   (`Alias/Handlers/AliasPartHandler.cs:59-62`). Confirm that the front end shows Home, Browse and About right after
   Publish. If it does not, stop: the narration of `publish-4` is wrong for this theme, and the clip needs a reload that
   the narration does not mention.
9. **The drop-down** (`publish-4`). `TheBlogTheme/Views/MenuItem.liquid` and `MenuItemLink-LinkMenuItem.liquid` render an item with
   items inside it as a toggle with a drop-down list (`data-bs-toggle="dropdown"`). Confirm that a click on "Browse" opens the
   list with "Categories" and does not leave the page. The toggle works only if Bootstrap's script is loaded: see question 14.
10. **The generated alias** (`create-4`). The alias pattern of the type Menu is the slug of the title
    (`Menu/Migrations/menu.recipe.json:31`) and `AliasPartHandler.cs:90-130` computes it when the alias is empty.
    Confirm that the alias reads `footer-menu` after the first Publish.
11. **"Add Menu Item" is disabled until the menu is saved** (`create-1`, `create-2`). `MenuPart.Edit.cshtml:19,30` disables
    it while the menu has no stored identifier. Confirm that the button is disabled on the page opened by "New Menu".
12. **Where Publish lands** (`publish-3`, `create-3`). "New Menu" and "Edit" in the Menus page add a returnUrl, so
    Publish returns to the Menus page (`Contents/Controllers/AdminController.cs:769-798`, `841-858`). Confirm the page
    after Publish shows "Your Menu has been published." (`AdminController.cs:472-473`, `966-967`).
13. **The Url values** (`add-2`, `arrange-2`). `~/blog` and `~/categories` follow the form of the items the Blog recipe
    creates (`~/about`). The form accepts them (`Menu/Drivers/LinkMenuItemPartDisplayDriver.cs`: only an unsafe or
    malformed address is refused). Confirm that no error shows on Publish. The video does not open those pages; confirm
    anyway that `/categories` and `/blog` are real pages, so a viewer who follows the links is not shown an error.
14. **The browser reaches the internet for the front end** (`open-1`, `publish-1`, `publish-4`). The Blog theme loads Google
    Fonts with two `link` tags (`TheBlogTheme/Views/Layout.liquid:12-13`) and Bootstrap's script `TheBlogTheme-bootstrap-bundle`,
    which has only a CDN address, `cdn.jsdelivr.net` (`TheBlogTheme/ResourceManagementOptionsConfiguration.cs:15-18`; a resource
    without a local address falls back to its CDN address, `OrchardCore.ResourceManagement.Abstractions/ResourceDefinition.cs:404-406`).
    The proxy variables of the plan are set on the site process; the recorder's browser context sets no proxy
    (`scripts/recorder/Scene.cs`, `BrowserNewContextOptions`). Confirm on the recording machine that both hosts are reachable
    and that `OpenAsync("/")` finishes (it waits for the network to be idle). Without Bootstrap's script, a click on "Browse"
    follows its link (`MenuItemLink-LinkMenuItem.liquid:4`) instead of opening the drop-down list, and the narration of
    `publish-4` would not match the screen. If the machine cannot reach the CDN, the clip needs another way to serve the
    script, which a person decides; the narration stays as it is.
15. **The Permissions picker** (`add-1`, `add-2`). The narration of `add-2` says that Permissions is optional and limits who
    sees the item. The hint of the picker says "The permissions required to display this menu item (optional)."
    (`Menu/Views/MenuItemPermissionPart.Edit.cshtml:17`), and the theme skips an item whose selected permissions the user lacks
    (`Menu/MenuShapes.cs:258-280`). Confirm that the Link Menu Item form shows the label "Permissions" below "Target" and that
    the plan's locator (the `.ocat-wrapper` block that holds `#PermissionPicker`) is visible, so `HighlightAsync` can ring it.
    The picker is drawn by a script (`vuejs`, `vue-multiselect`); confirm it loads without the internet.

## Pronunciation (a person confirms with `scripts/pronounce.py` before narrating)

The storyboard's `spoken` map holds four fixes. None was tried, because no audio is generated while writing:

- `Url` spoken as "U.R.L." (the label as the code spells it; a voice may say "earl");
- `Html` spoken as "H.T.M.L." (in "Html Menu Item");
- `main-menu` spoken as "main menu" and `footer-menu` as "footer menu" (so the hyphen is not read out).

Listen too to "drop-down", "Categories" and "Browse" as a verb-like name, and to the gap after "Menus." in `open-2`.

## Not covered on purpose

Content Menu Items, Html Menu Items, the field "Target", item permissions, and rendering a menu in a theme are covered by
other videos of the series; this video names them once (`menus-3`, `add-2`, `create-4`).

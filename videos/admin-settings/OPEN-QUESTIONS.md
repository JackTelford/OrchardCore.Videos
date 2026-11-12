# Open questions: admin-settings

What the narration says was read from the source of `OrchardCMS/OrchardCore` at commit `92db4a83`. The points below
could not be settled from the source alone. For each: what was checked, what the narration does about it, and what a
person must confirm on a running site (the Blog recipe, defaults as in `recording-plan.md`) before recording.

## 1. Who can open Settings, Admin

- Checked: `PermissionsAdminSettings.cs` grants **Manage Admin Settings** to the Administrator role only, and the
  menu item `Admin` requires it (`AdminMenu.cs:30`). The page itself is served by
  `OrchardCore.Settings/Controllers/AdminController.cs:53`, which asks for the group permission
  `ManageGroupSettings` with the resource `admin`. No module registers a group permission for `admin`
  (`AddSiteSettingsPermission` is called only for general, debugging and email settings), so that check falls back to
  the permission it implies, **Manage settings** (`SettingsPermissions.cs:21`). The settings editor also asks for
  **Manage Admin Settings** (`AdminSiteSettingsDisplayDriver.cs:38,63`).
- Narration (`open-1`): "By default, only the Administrator role can open this page." It names no permission.
- Reading of the source: the default stereotypes of both permissions list the Administrator role only
  (`PermissionsAdminSettings.cs:12-20`, `OrchardCore.Settings/Permissions.cs:17-25`), so the sentence of the narration
  holds. A role that has only **Manage Admin Settings** would see the menu entry but, by this reading, get a
  Forbid answer from the page (`AdminController.cs:53-56`), because no group permission is registered for `admin`.
- To confirm (not needed for the take): create a role that has only **Manage Admin Settings** (and **Access admin
  panel**), sign in as a user of that role, and see whether Settings, Admin opens or answers with an error. If it needs
  **Manage settings** too, the documentation of the Admin module should say so.

## 2. The items under "New" and its place in the menu

- Checked: the **New** entry lists the content types that are creatable, ordered by display name
  (`Contents/AdminMenu.cs:84`), at position `-1`. A top-level item with no position gets the empty position
  (`Shapes/Shape.cs:60`), which the comparer reads as `0` (`Zones/FlatPositionComparer.cs:160-162`); **Content** has
  position `1`, and no navigation provider puts a top-level item before `-1`. So **New** is the first entry of the
  menu, above the recipe's **Blog** and **Main Menu** (`blog.recipe.json:1188-1307`, positions null). In the Blog
  recipe the creatable types are Article, Blog and Page (`blog.recipe.json:136,248,448`), and the Taxonomies migration
  also makes **Taxonomy** creatable (`Taxonomies/Migrations.cs:26`).
- Narration (`menu-3`): "such as Article and Page", and "at the top of the admin menu".
- To confirm: the items under **New** (expected Article, Blog, Page, Taxonomy) and that **New** is the first entry of
  the menu on the recorded site. The recipe's own **Content** group also lists Article, Page and Taxonomy (see
  question 10), so a locator for these names must be scoped to the **New** group.

## 3. Focused and Persistent menu sections

- Checked: `TheAdmin/Assets/js/TheAdmin/sidebar.ts` (`isFocused`, `applyFocusedAccordion`, `persistNavState`) and the
  hints of the page. With Focused, a section opened by a click closes the others of its list through Bootstrap's
  `data-bs-parent`; with Persistent, the open sections are stored in the browser (local storage and a cookie, keyed
  by tenant: `adminPreferences.ts:17,43-44`), which is what the narration says without naming the place.
- Narration (`menu-4`, `menu-5`) repeats the hint texts of the page.
- To confirm: in Focused mode, clicking Content then Design leaves only Design open. The attribute names the list of
  top-level sections as the parent, which by Bootstrap's collapse behavior also closes the **Settings** group (the
  current page's section) when Content opens; that part belongs to Bootstrap, not to this repository, so the plan
  accepts either frame. The narration says only what happens when a page loads and that opening a section closes the
  others. In Persistent mode after a reload, Content and Design stay open as the clip
  leaves them.

## 4. The Filter box

- Checked: `TheAdmin/Assets/js/NavigationAdmin/NavigationAdmin.ts`. A match is a case-insensitive substring of an
  item's title; matching items are shown with the groups that hold them; Escape in the box empties it and restores the
  sections as they were. Ctrl+Shift+F focuses the box (shown as the tooltip of the box). The narration does not name
  the shortcut.
- To confirm: typing `Roles` leaves **Access Control** and **Roles** and hides every other item. If another item
  also matches, pick another word with a single match and update `menu-2` and the plan.

## 5. Breadcrumb trail and titles in the top bar

- Checked: `BreadcrumbTagHelper.cs` (with Show breadcrumb off, only the heading remains), `TheAdmin/Views/Layout.cshtml`
  (with Display titles in top bar on, the `Title` section is rendered in the top bar instead of the page) and
  `TheAdmin/Views/Breadcrumb.cshtml` (the trail stays in the page, above the content).
- The Settings page declares a trail of one entry (`Settings/Views/Admin/Index.cshtml:4`), so the change in `top-4`
  is a short trail line that disappears. The narration says that on pages such as the content editor the trail leads
  back to **Manage Content** (`Contents/Views/Admin/Edit.cshtml:39-48`), which is not shown on screen.
- To confirm: the trail line is visible above the heading before `top-4`, and gone after it; the title **Settings**
  appears in the top bar after `top-4`. If the single-entry trail is too faint on camera, record `top-4` on a page with
  a longer trail (the edit page of a content item) and update the plan.

## 6. The theme toggler when it is turned off

- Checked: with the option off, the layout writes the theme `lite` into the page (`ThemeTogglerService.cs`), but the
  script `theme-head` (a dependency of the admin scripts) then sets the theme from the browser's stored choice or from
  the operating system's color scheme (`Themes/Assets/ts/constants.ts`, `theme-head.ts`).
- Narration (`top-2`): only says what the option adds when it is on: the button, with Auto, Light and Dark, and that
  the choice is kept in the person's browser (`theme-manager.ts`, local storage). It makes no claim about the off state.
- To confirm (for a later edit, not needed for the take): what theme a browser that stored Dark shows after the option
  is cleared.

## 7. The layout selector and the Grid columns

- Checked: `AdminListLayoutSelector.cshtml`, `AdminList.cshtml:95-121` (the selector sits in the toolbar row at the top
  of the list), `AdminListLayoutPreference.cs` (a cookie, per list, per browser), `AdminListLayoutResolver.cs`
  (the cookie wins over the site layout when users may choose), `Contents/Views/ContentsAdminList.cshtml:34-39` (the
  Grid columns **Title**, **Type**, **Status**, **Last modified**, and an actions column), and the labels **Published**
  and **Draft** (`ContentsTags_SummaryAdmin.cshtml:12,19`).
- Narration (`lists-3`, `lists-4`): the four column titles; the ellipsis button; List and Grid buttons "above the rows";
  the choice kept "for that list, for that person only"; clearing the option makes every list follow the site layout
  (also what the documentation says, and `AdminListLayoutResolver.cs`, when selection is not allowed).
- To confirm: the buttons are at the top of the list in both layouts; the row menu opens from the ellipsis in the Grid
  layout; the cookie of `lists-4` does not change what `lists-3` shows (the order of the clip avoids it).

## 8. Pronunciation

- The storyboard maps "Ctrl+K" to "Control K" in `spoken` (the written form stays in the captions). Check the sample of
  `scripts/pronounce.py` before narrating. The same spelling is not needed elsewhere.
- "Filter", "Roles", "Auto", "Light", "Dark" and the labels of the options are plain words.

## 9. The first Save of `open-2`

- `open-2` clicks **Save** on a page nothing changed. The source posts the form and redirects with the notification
  **Site settings updated successfully.** (`Settings/Controllers/AdminController.cs:119`), so the take shows the
  message. To confirm: the message appears when no value changed, and no validation error is shown.

## 10. The recipe's own admin menu and the "Content Items" link

- Checked: the Blog recipe runs an `AdminMenu` step (`blog.recipe.json:1188-1307`). Its **Content** group (a placeholder
  node) holds a **Content Items** link to `Admin/Contents/ContentItems/` (with a final slash) and entries for Article,
  Page and Taxonomy (`ContentTypesAdminNodeNavigationBuilder.cs`). `NavigationManager.Merge` merges items of the same
  caption and keeps the values of one of the two, depending on the order of the providers
  (`NavigationManager.cs:97-142`), so the address behind **Content Items** may or may not end with a slash.
- Plan: the steps find **Content Items** by its role and name, never by its address, and scope **Article** to the
  **New** group.
- To confirm: one **Content Items** link and one **Content** group on screen, and the order of the groups.

## 11. Hints that name a configuration file

- Checked: the hints under **List layout** and **Actions layout** read "... Use the configuration default to inherit
  OrchardCore:Admin:List:DefaultLayout from appsettings.json." (`AdminSettings.Edit.cshtml:94,107`).
- Narration: it names the options and their choices only. The hint is page text that appears on screen while the
  options are shown, and the rules for an admin video forbid showing configuration files. The plan keeps the highlight
  on the select and away from the hint.
- To confirm: whether the product's own hint text, which names a configuration file and a key, is acceptable on screen in
  an admin video. If not, the frames of `lists-1` and `lists-2` need a crop or a caption over the hint, which is a
  decision for the person who assembles the video.

## 12. The name of the theme button

- Checked: **Toggle theme** is the `aria-label` and the hidden text of the button (`ToggleTheme.cshtml:3,5`); the screen
  shows an icon, and only Auto, Light and Dark are drawn as text. The narration uses the name the code has.
- To confirm: whether the phrase "a Toggle theme button" reads well over a frame that shows an icon, or whether a caption
  or a hover tooltip is wanted. No tooltip is defined for this button in the source.

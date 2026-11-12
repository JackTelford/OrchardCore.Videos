# Open questions: admin-tour

Everything in the narration was read from the source (Orchard Core commit `92db4a83`) and the documentation. The
points below could not be settled without a running site. A person confirms each one on the recording site and corrects
`storyboard.json` or `recording-plan.md` if the screen differs.

## 1. The order and the children of the menu on a Blog site

**Checked.** The positions of the sections: Content `1`, Design `2`, Search `6`, Access Control `7`, Media `after.15`,
Tools `end.50`, Settings `end.100`
(`src/OrchardCore/OrchardCore.Navigation.Core/NavigationConstants.cs`, `src/OrchardCore.Modules/OrchardCore.Media/AdminMenu.cs:18`,
`src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:32,36`), and how positions sort
(`src/OrchardCore/OrchardCore.DisplayManagement/Zones/FlatPositionComparer.cs`). The Blog recipe also creates an admin
menu with the links Blog and Main Menu and a "Content" node with Content Items and entries for Article, Page and
Taxonomy (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:1188-1290`, no position set). Entries with the
same label are merged (`src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74`). A node without a
position is sorted as `before`, which is lower than any number (`FlatPositionComparer.cs`, `GetNormalizedSpan`), so
Blog and Main Menu should be the first two entries of the menu, above Content; the merged Content section keeps the
position `1` of the Contents module.

**To confirm on screen.**
- That Blog and Main Menu are the first two entries (as the code order says), and that they link to the blog and to
  the Main Menu content item.
- That the Content section shows Content Items once (the module's entry and the recipe's entry merged), and which
  other entries it holds (Article, Page, Taxonomy, Menus).
- That Design, Access Control, Tools and Settings hold the pages the clips click (Themes, Users, Admin Menus, Admin).

The narration names sections and never gives their order or their number, so a different order changes only the
recording.

## 2. Pronunciation

`storyboard.json` has two `spoken` entries: "Ctrl+K" is read "Control K", and "Esc key" is read "Escape key". They
were not tried with `scripts/pronounce.py`. A person confirms both with a sample, and the gaps around "Control K".

## 3. Typed words in the palette

- `quick-2` types `user`. From the source, the results are Users (path Access Control) and User Login (path Settings,
  Security), Users first. Confirm there is no other menu entry whose title holds "user" on the Blog site.
- `content-3` types `about`. From the source, the content item About appears with the path Content. Confirm it is the
  only result, and that the result list is not empty because of the 15-result limit.

## 4. The message after enabling a feature

`content-2` ends on the message "The feature Content Quick Navigation was enabled." It is a toast that hides itself
after 5 seconds (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`, `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml:20`). The narration of the step is
about 5 seconds long: confirm the message is still shown when the step ends. The plan accepts the status "Enabled" of
the row as a fallback.

## 5. The theme toggle keeps its choice from page to page

**Checked.** The admin layout loads the script `the-admin`, which depends on `theme-manager` and on `admin-main`, which
depends on `theme-head`
(`src/OrchardCore.Themes/TheAdmin/ResourceManagementOptionsConfiguration.cs:17,27`). The toggle offers Auto, Light and
Dark; the script stores the choice in the browser's local storage, `theme-head` applies it before each admin page is
drawn, and Auto follows the color scheme of the device
(`src/OrchardCore.Modules/OrchardCore.Themes/Assets/ts/constants.ts`, `theme-manager.ts`, `theme-head.ts`). The server
also renders the page from a cookie `<tenant>-admintheme` and defaults to Auto
(`src/OrchardCore.Modules/OrchardCore.Themes/Services/ThemeTogglerService.cs:31`); no code that writes this cookie was
found.

**To confirm.** After choosing Dark and opening another admin page, the admin stays dark with no flash of the light
theme. The narration says "Your browser remembers the choice".

## 6. Hover on the collapsed menu

In the collapsed menu, hovering over a section icon opens a flyout with its pages
(`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts`, `mouseenter`). Confirm that the recorder's cursor
movement opens the flyout in the headless browser, and that clicking the button again expands the menu.

## 7. What the recorder keeps between clips

Each clip starts from `state.json`. Confirm that the theme choice (clip `07-topbar`) and the collapsed menu (clip
`03-menu`) do not reach the next clip; both clips put them back anyway.

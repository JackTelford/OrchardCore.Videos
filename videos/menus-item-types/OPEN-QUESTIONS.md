# Open questions: Menu items

Everything the narration states was read in the source of Orchard Core at commit `92db4a83` (the Menu module, the
Contents module, TheBlogTheme and the pages named in recording-plan.md). Nothing was run. A person must confirm the
following on a running Blog site before recording, and report any difference so the narration can be changed.

## Things the recording depends on

1. **The path Content, Menus in the admin menu.** `AdminMenu.cs:33-34` of the Menu module defines **Content** then
   **Menus**. The Blog recipe also creates an admin menu of its own (`blog.recipe.json`, step `AdminMenu`) that has a
   **Content** entry and a **Main Menu** shortcut. The navigation manager merges the items that have the same caption
   (`src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74-114`), so one **Content** group holding **Menus**
   is expected. Check on the running site that it shows one group and not two, and find the locators with `Recorder probe`.
2. **The Edit button of the Menus list.** Expected: one row, **Main Menu**, with **Edit** (`ContentsButtonEdit_SummaryAdmin.cshtml:13`),
   whose address carries a `returnUrl`. The plan removes it so the menu editor shows a plain **Publish** button. Confirm
   that this is the editor the redirect after Add and Edit lands on.
3. **The buttons of the menu item form.** The form (Link, Content and Html types) gets its buttons from the Contents module
   (`ContentsDriver.cs:110-115`), not from the Menu module. Expected: one **Publish** button (the three item types are not
   draftable, a new item has no id so there is no **Delete**, and it is not published so there is no **Unpublish**). The
   Menu module only saves the item into the draft of the menu (`AdminController.cs:137,237`) and ignores which button was
   pressed. If the form shows other buttons or labels, the steps `link-3`, `content-4`, `html-4` and `perm-3` and the
   recording plan change.
4. **The order of the cards in the window Available Menu Items**, and of the fields on the Content Menu Item form
   (**Name**, **Check content permissions**, **Selected Content Item**). The cards come from the content type
   definitions with the stereotype MenuItem (`MenuPart.Edit.cshtml:65-73`) in the order the content definition manager
   lists them. The narration does not rely on any order.
5. **The address of the Content Menu Item row.** The row and the front end build the address from the id of the selected
   item through the route values of Autoroute (`ContentMenuItemPart.Admin.cshtml:5-10`,
   `OrchardCore.Autoroute/Routing/AutorouteValuesAddressScheme.cs`). Expected: `/opening-hours`. The narration says only
   that the link is built from the content item. What the link becomes when the selected item has no address, or when its
   address changes while the menu is cached, was not checked and is not stated.
6. **The Permissions picker.** Expected: typing `Access admin` lists the option **Access admin panel**
   (`AdminPermissions.cs:8`; the picker lists the description of every permission,
   `MenuItemPermissionPartDisplayDriver.cs:37-45`). Confirm that the option is listed once, and that the item **Editor Guide**
   shows for the administrator and not for the signed-out visitor after the menu is published. The code requires every
   listed permission (`MenuShapes.cs:266-280`) and builds the sub items only for an item that is shown (`MenuShapes.cs:108-140`);
   neither is demonstrated.
7. **An address with the scheme `mailto`.** `html-2` types `mailto:hello@example.com` in **Url** of the Html Menu Item.
   The sanitizer allows the scheme (`OrchardCoreBuilderExtensions.cs:22`), so the editor should accept it. If the form
   shows "... is an invalid url.", use `~/about` instead and change the narration of `html-2`.
8. **Italic text in the front end.** The one tag typed in **Html** (`<em>Contact</em> us`) must be kept by the sanitizer
   (the default list of allowed tags comes from the sanitizer library and is not in this repository, so it was not read;
   the project only adds the attribute `class` and the schemes `mailto` and `tel`, and removes `form`:
   `src/OrchardCore/OrchardCore.Infrastructure/Html/OrchardCoreBuilderExtensions.cs:17-24`) and show as a slanted word in
   the header of TheBlogTheme (`MenuItemLink-HtmlMenuItem.liquid`). **Bold was not used on purpose**: the theme styles every
   header link with `font-weight: 800` and capitals (`TheBlogTheme/wwwroot/css/styles.css:11057-11062`) and loads Open Sans
   only up to 800 (`Layout.liquid:13`), so a `strong` tag has no heavier face to switch to. What was not checked: that the
   slant is clear enough at the size of the header links (0.75rem, capitals). Confirm on the contact sheet after `publish-3`;
   if it is too faint, pick another tag that changes the look (and change the words "italic" and "italics" in `html-3` and
   `publish-3`).
9. **The seed by JSON Import.** The JSON of recording-plan.md was not imported. If the import fails, create the Article in
   the admin as the plan also describes.
10. **The signed-out clip.** `07-visitor` must create its scene with a storage state path that does not exist
    (`Scene.cs:109`), which `NewSceneAsync` of `Scenes.cs` does not allow because it passes the path of `state.json`.
    The scene is created with `Scene.StartAsync` directly. Confirm the context has no cookie (the front end shows no
    signed-in toolbar).

## Behavior stated by the slides but not recorded

11. **Check content permissions** (`types-3`, `content-4`). From `MenuShapes.cs:291-306`: with the option on, an item is
    shown only if the user may view the published version of the selected content item, and is hidden when that item has
    no published version. The option stays off in the recording. A person may want to try it with a signed-out visitor
    before deciding whether to record it on, and to check the claim about caching in the hint
    (`ContentMenuItemPart.Edit.cshtml:15`).
12. **Sanitize Html** (`types-4`, `html-4`). It is a setting of the part **Html Menu Item** in the definition of the
    content type, on by default (`Settings/HtmlMenuItemPartSettings.cs`, `Views/HtmlMenuItemPartSettings.Edit.cshtml:7`).
    The video names it and does not open the type editor. The path to it, Design, Content Definition, Content Types, then
    **Edit** on the type and **Edit** on its part, is in the source (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`,
    `Views/Admin/Edit.cshtml:163`); what a person must confirm is that the part editor of **Html Menu Item** shows the
    checkbox **Sanitize Html** (`Settings/HtmlMenuItemPartSettingsDisplayDriver.cs:12-19`).

13. **The notification of `publish-2`.** The toast "Your Menu has been published." hides itself 5 seconds after the page
    loads (`NotifierExtensions.cs:71`, `TheAdmin/Views/Message.cshtml`, `TheAdmin/Assets/js/NotifyMessages/NotifyMessages.ts`).
    The plan clicks **Publish** about 3 seconds into the step so the toast shows during the second sentence. Confirm with the
    real duration of the step (`durations.json`) that the toast is on screen when the words "A message confirms" are spoken.

## Pronunciation

14. The storyboard's `spoken` map says **Url** as "U.R.L." and **Html** as "H.T.M.L." (the labels of the fields and the
    type). The spellings follow `references/storyboard.md` but were not listened to: confirm them with `pronounce.py`
    before narrating. Both words are always inside a sentence, never at its end, so the replacement does not double a
    period. "HTML" in capitals and "URL" are left to the voice.

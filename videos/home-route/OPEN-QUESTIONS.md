# Open questions: home-route

What was checked in the Orchard Core source (commit 92db4a83), and what a person must confirm on a running site before
recording. Nothing in the narration depends on an unanswered question; the facts below are kept out of it.

## To confirm on the running site (they only affect how the clips are recorded)

1. **Row and button locators in the admin lists.** The Content Items, Roles and Features lists are drawn by an admin
   list that can use a List or a Grid layout. The plan finds a row with `.list-group-item, tr`, and its Edit button with
   `a.edit` (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml`, `OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`).
   Confirm that both layouts match, or fix the selector in the scene.
2. **The Features search.** The search matches the name, the id and the description of a feature
   (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), so "Home Route" and "Autoroute" find their rows.
   Confirm which other rows the search lists, and that the Home Route and Autoroute rows both show the badge "Enabled".
3. **The admin menu.** The plan opens the groups Tools, Design (and inside it Content Definition) and Access Control
   before the capture (`OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`). Confirm that a click on a group
   expands it in the headless browser and that the item stays visible after the next page loads.
4. **The notification.** "Your Article has been published." is a toast (`OrchardCore.Themes/TheAdmin/Views/Message.cshtml`)
   that may hide itself after a delay (`data-bs-autohide`). Confirm that it is still on screen at the end of
   `set-4` and `change-1`; if not, publish later in the step.
5. **The Pattern box.** The Autoroute settings page starts with the Pattern box, which holds a template written in
   Liquid (`OrchardCore.Autoroute/Views/AutoroutePartSettings.Edit.cshtml:13-19`). An admin video must not show code,
   and the box is on screen as soon as the page opens, so the plan hides it with an init script (section 1 of the
   plan, `VIDEO_INIT_SCRIPT`). Confirm that the rule `.ocat-limited-wrapper:has(textarea[name$='Pattern'])` hides the
   label, the box (and the editor that the page script may draw over it) and the hint, and nothing else. If the page
   script wraps the box in another element, adjust the selector, and check the frames of `allow-2` and `allow-3`.
6. **The front end.** Confirm how `TheBlogTheme` draws the article as the home page (title, subtitle and body), so that
   `set-5` shows the title of the article and not a blank page.
7. **Pronunciation.** Listen to "Autoroute", "permalink" and "homepage" in the generated voice (`scripts/pronounce.py`),
   and add a "spoken" entry if one is misread.
8. **The custom admin menu of the Blog recipe.** The recipe adds its own admin menu entries
   (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `AdminMenu` step from line 1188): a link
   "Blog", a link "Main Menu" and a group "Content" with "Content Items". Confirm on the running site whether "Content"
   and "Content Items" appear once or twice in the left menu. The plan opens the Content Items list by its address, so
   it does not depend on the answer, but the narration reads "Content, Content Items".
9. **The link named "Blog".** Three links are named "Blog" on the Content Items page (the left menu, the title of the
   Blog row and its content type). The plan uses the Edit button of the row found through its links. Confirm that the
   locator of `change-1` matches exactly one button.
10. **The labels of the seed form.** The body field of the Article editor is labeled with the name of the part
    (`OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml:22`, which I read as "HtmlBody"), and the image field is
    labeled "Banner Image" (`blog.recipe.json:695`). Confirm both on the running site; they matter only for the hand
    made seed, which is not recorded.
11. **The system roles in the list.** `permission-1` expects Administrator, Anonymous and Authenticated in the list of
    roles, next to the four roles of the recipe (`OrchardCore.Roles/Migrations/SystemRolesMigrations.cs:26`). Confirm the
    list, or drop the names from the description of what is visible.

## Checked in the source, deliberately not said in the narration

- The home route is applied when the item is published, not when a draft is saved
  (`OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:67-92`; the hint says "once published"). What happens to the
  flag when a draft is saved with the box checked and published later is not described: the editor resets the flag when
  it is built (`OrchardCore.Autoroute/Drivers/AutoroutePartDisplayDriver.cs:53`).
- The previous home page keeps its permalink in the code path (`OrchardCore.Autoroute/Routing/AutorouteTransformer.cs`
  matches any path of the index), but links built for the home page item are not described, and neither is what `/`
  shows when the home route is empty (`OrchardCore.HomeRoute/Routing/HomeRouteTransformer.cs:21-27` returns no route).
  Confirm on a site before any video says so.
- A content item that is contained in another (a bag or a taxonomy term) can also be made the home page
  (`AutoroutePartHandler.cs:208-232`). The video covers top-level items only.
- Cloning an item resets its flag (`AutoroutePartHandler.cs:159`).

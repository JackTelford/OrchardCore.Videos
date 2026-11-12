# Open questions: navigation-in-themes

Every label, name and behavior in the narration, the slides and the recording plan was read from the Orchard Core source at
commit `92db4a83` and from `src/docs/reference/modules/Navigation/README.md` and `Menu/README.md`. No site was run. A person
must confirm the following on a running site while recording, and fix the narration or the plan if the site differs.

## Screens to confirm

1. **The trail of a menu item.** With the Admin Dashboard feature enabled, the editor of a menu item (`/Admin/Menu/Edit`) is
   expected to show `Dashboard › Manage Content › Edit Main Menu › Edit Link Menu Item`, and the heading `Edit Link Menu Item`.
   Read from `OrchardCore.Modules/OrchardCore.Menu/Views/Admin/Edit.cshtml:19-26`,
   `OrchardCore.Modules/OrchardCore.AdminDashboard/Services/DashboardBreadcrumbProvider.cs:33-39` and
   `OrchardCore.Themes/TheAdmin/Views/Breadcrumb.cshtml`. The narration of `trailsite-1` names these four nodes.
2. **Clearing Show breadcrumb.** Expected: the trail disappears and the `h1` with class `oc-breadcrumb-title` stays
   (`OrchardCore/OrchardCore.Navigation.Core/BreadcrumbTagHelper.cs:146-160`). The narration of `trailsite-3` says so.
3. **The pager of the blog.** With twelve posts added to the sample post, expected: page one ends with `Older Posts →`
   and no `← Newer Posts`; page two (three posts) shows `← Newer Posts` and no `Older Posts →`. Read from
   `OrchardCore.Modules/OrchardCore.Lists/Services/ContainerService.cs` (the slim pager query: page one sets only `After`, page two sets
   only `Before`) and `OrchardCore/OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:35-38`, which keeps the
   `CreatedUtc` an imported item carries, so the cursors follow the dates of the seed (the sample post is the newest, because
   its date is the time of the setup). Confirm on the running site that the twelve posts keep their dates after the JSON Import
   and that the second page lists exactly three posts. If the dates were lost, set them by editing the posts.
   The plain `Items per page` selector of the Navigation module also shows beside this pager, because page size selection is
   allowed for the whole recording (`OrchardCore.Modules/OrchardCore.Navigation/PagerShapesTableProvider.cs:454-459`, and the Blog Theme has no
   `Pager_PageSizeSelector` template). Confirm that it does not hide the links.
4. **The pager of the admin.** The clip `05-pager-admin` opens `/Admin/Contents/ContentItems/BlogPost`, not the unfiltered list of
   Content, Content Items: **Blog Post** is not a listable type in the Blog recipe, so the unfiltered list has five items
   on one page and draws no page links (`blog.recipe.json:188-195`, `DefaultContentsAdminListFilterProvider.cs:182`,
   `PagerShapesTableProvider.cs:179`). The filtered address lists a type even when it is not listable (same file, line 145).
   Expected: thirteen items on two pages, the heading `Blog Post`, the numbered pages with the first, previous, next and last
   links as icons (`OrchardCore.Themes/TheAdmin/Views/Pager_First.cshtml` and its siblings), and the selector labeled `Items per page`
   inside the same `nav` (`TheAdmin/Views/Pager_PageSizeSelector.cshtml`), once the tenant has restarted after saving Settings, General.
   Confirm that the heading and the pager appear as described, and that the selector is visible at the same time as the pager.
5. **The Features page.** The plan says to click the `Enable` button of the row of Admin Dashboard. The button is generated in
   `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml`; its id is built by
   `GenerateIdFromName`, so use the visible button, not an id.
6. **The editor of Main Menu.** Expected: the `Alias` field shows `main-menu` (set by the Blog recipe,
   `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:852`) and the items `Home` and `About` are listed under
   `Add Menu Item`. Confirm that both the field and the list are visible without scrolling at 1536 x 864; if not, scroll in
   the step instead of changing the narration.

## Behavior that the narration states from the source

7. **Cache invalidation.** `render-2` says that publishing the menu drops the cached copy because the cache tag is the alias.
   Source: `OrchardCore.Modules/OrchardCore.Alias/Handlers/AliasPartHandler.cs` (`PublishedAsync` removes the tag
   `alias:<alias>`) and `OrchardCore/OrchardCore.DisplayManagement/TagHelpers/BaseShapeTagHelper.cs` (`cache-tag`). To confirm:
   publish the menu and reload the home page within five minutes; the change should show at once.
8. **Cache attributes need Dynamic Cache.** `render-1` says so. Source: only `DynamicCacheShapeDisplayEvents`
   (`OrchardCore.Modules/OrchardCore.DynamicCache/Startup.cs:30`) reads the cache settings of a shape.
9. **A Liquid tag across lines.** `slides.py` writes the `shape` tag of Liquid on several lines for the slide. The theme's own
   file has it on one line (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:32`); the `shape_pager` tag of the same theme
   already spans two lines (`Views/Blog-ListPart.liquid`), and both tags use the same argument list parser, but a template with
   the tag on several lines was not run.
10. **`PagerId`.** The slide of `shapes-1` shows `Pager-MainBlog.cshtml` for a pager whose `PagerId` is `MainBlog`. The
    alternate is added when the shape has the property `PagerId` (`OrchardCore.Modules/OrchardCore.Navigation/PagerShapesTableProvider.cs:36`).
    No module of the repository sets that property and the documentation does not say where a developer sets it, so the video
    does not show how.
11. **The Title zone.** `trail-1` says the admin theme renders the Title zone. Read from
    `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:61-66,88-93`. The layouts of The Default Theme and The Blog Theme render no
    Title zone (`Views/Layout.cshtml` and `Views/Layout.liquid`), so a front end page places the tag helper where its own template
    wants the trail; the video does not show a front end trail, and the tag helper gives no heading there unless `heading` is set
    (`BreadcrumbTagHelper.cs:248-256`). A person may add a front end example if the video should cover it.
12. **The names of a menu.** `templates-4` says a menu's name is its title as one word, without spaces or hyphens, each word
    capitalized. Read from `OrchardCore.Modules/OrchardCore.Menu/MenuShapes.cs` (`FormatName`: a space or a hyphen is dropped and the next
    letter is made upper case; the other letters are kept as typed). The title `Main Menu` gives `MainMenu`. Confirm on the running site that
    a template named `Menu-MainMenu.cshtml` in a copy of the theme takes the place of the menu template.

## Pronunciation (the spoken map of storyboard.json)

Check each spelling with `scripts/pronounce.py` before narrating: `Pager Slim`, `Menu Item Link`, `Menu Item`, `I Breadcrumb Provider`, `Add Breadcrumb Provider`, `Pager Parameters`, `Pager Options`, `Pager Async`,
`Pager Link`, `Pager Page Size Selector`, `Detail Admin`, `Model dot Items`, `shape pager`. `narrate.py` applies the map with a
plain text replacement, in the order of the file, so `MenuItemLink` stays before `MenuItem`. No step contains `MainMenu` as a word to
speak: the voice would say `Main Menu` for both the title and the name, so `templates-4` points at the first card instead.

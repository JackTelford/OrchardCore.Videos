# Open questions: lists-overview

What was read from the source and could not be seen on a running site. Each item says what was checked and what a
person must confirm while recording. Source paths are relative to `src/OrchardCore.Modules/`, unless they start with
`src/`.

1. **Search and status filter of a list page** (steps `find-2`, `find-3`, `find-4`). The inputs of the search panel are
   named `DisplayText` and `Status` with no prefix (`OrchardCore.Lists/Views/ListPartDetailAdminSearchPanel.cshtml:25`
   to `:31`), while the display driver reads them with the prefix of the part
   (`OrchardCore.Lists/Drivers/ListPartDisplayDriver.cs`, `TryUpdateModelAsync(listPartFilterViewModel, Prefix)` in the
   detail admin shape). The part driver sets `Prefix` to the part name only while it builds the display
   (`src/OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentPartDisplayDriverTPart.cs:78` to `:212`),
   and a shape initializer runs later, when the result is applied
   (`ContentItemDisplayCoordinator.cs`, `ShapeResult.ApplyAsync`), so the prefix is probably empty by then and the
   unprefixed inputs bind. No test in the source tree covers it. Confirm that **Only draft items**, **Only published items** and the search box change the rows
   as the plan says.
2. **Technical Name** (step `type-2` and the start pages of `05-find` and `06-site`). The name is computed in the
   browser from the Display Name (`OrchardCore.ContentTypes/Assets/js/autocomplete.ts`). The plan expects `HelpCenter`.
   If it reads differently, use the name shown in the field in the addresses of the plan.
3. **Row button on Content Items** (step `items-3`). The button on a list's row builds "List {0} Items" from the
   technical name of the contained type (`OrchardCore.Lists/Views/ListPartSummaryAdmin.cshtml:10`), while the bar on the
   list page uses its display name (`ListPartNavigationAdmin.cshtml:32`). For Page both read **List Page Items**.
   For the Blog the row would read the technical name, `BlogPost`, so `02-blog` opens the Blog from the admin menu and
   never uses that row. Confirm the label on the Help Center row.
4. **The "Blog" link of the admin menu** (step `blog-1`). It comes from the Blog recipe's admin menu step
   (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:1197`). Confirm it is visible in the left menu of the
   default admin theme without opening another menu, and that it opens the page with **Edit Blog** and **Create Blog
   Post**.
5. **Menu path** (step `type-1`). Design, Content Definition, Content Types are the labels of
   `OrchardCore.ContentTypes/AdminMenu.cs:23` to `:25`. Confirm they are reachable in the admin menu as the narration
   says, and whether the menus must be expanded before the capture for the clicks to fit the narration.
6. **Public page of the list** (steps `site-1` to `site-3`). From the code, the list shows published items only,
   most recent first, each drawn with its Summary display
   (`OrchardCore.Lists/Views/ListPart.cshtml`, `OrchardCore.Lists/Models/ContainedItemOptions.cs:14`). The contained
   type is Page and not Article because TheBlogTheme has `Views/Content-Article.liquid`: the shape alternates of a
   content item put `Content__Article` after `Content_Summary` (`OrchardCore.Contents/ContentShapeAlternatesFactory.cs`),
   alternates are tried from the last to the first (`DefaultHtmlDisplay.GetShapeBindingAsync`), and the theme's
   template has no display type in its name, so an Article in a list would be drawn by that template (a header block
   for the page, no link on the title) and not by `TitlePart.Summary.cshtml`. For Page the theme has no template, so
   `OrchardCore.Contents/Views/Content.Summary.cshtml` draws it and `OrchardCore.Title/Views/TitlePart.Summary.cshtml`
   (a link on the title, line 3) fills its header. Not seen on a running site: confirm that the Help Center page shows
   "Change your email address" and "Reset your password" as links, in that order, that "Export your data" is absent,
   and that a click on a title opens the page of that item, with the title in the page header
   (`OrchardCore.Themes/TheBlogTheme/Views/Content.liquid`).
7. **Buttons of a draft row** (step `find-1`). The narration says published items also have **View**
   (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`, shown only when a published version
   exists). The same view adds **Preview Draft** to a row that has a draft when the user may preview content
   (`:21`). Confirm the "Export your data" row shows **Edit** and **Preview Draft** and no **View**, and the other two
   rows show **Edit** and **View**.
8. **Bar of links on the Page editor** (step `items-4`). The bar comes from the contained part driver
   (`OrchardCore.Lists/Drivers/ContainedPartDisplayDriver.cs:30` and `:123`, and the navigation view). Confirm it
   appears on the Create page opened from **Create Page**, with **List Page Items**, **Edit Help Center** and **Create Page**.
9. **Where Publish and Save Draft lead** (steps `items-5`, `items-6`, `items-7`). From `CreateInternalAsync`
   (`OrchardCore.Contents/Controllers/AdminController.cs:795`), the browser goes to the return address, which is the
   page of the list. Confirm it for the three clicks.
10. **Pronunciation.** Check "Autoroute" and "permalink" with the voice (`scripts/pronounce.py`) and add a `spoken`
    entry only if a person confirms a better spelling. `spoken` is empty for now.
11. **Content Items and contained pages.** The narration makes no claim on whether the pages of the list also
    show on the Content Items page; the plan never relies on it.
12. **Timing.** `type-1`, `type-3` and `type-4` have the most clicks for the narration (about 11, 12 and 10 seconds).
    Shorten the pauses, or expand the menus before the capture, if a step runs longer than its narration.
13. **Permalink of the new items** (steps `items-2`, `site-1`, `site-3`). The Autoroute part of a type made in the
    admin has the default settings (no custom path, pattern `{{ ContentItem.DisplayText | slugify }}`,
    `OrchardCore.Autoroute/Models/AutoroutePartSettings.cs`), so the Permalink field of the Help Center editor is
    read-only and empty, and the path is built when the item is created
    (`OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs`, `CreatedAsync`). Confirm that the narration "The
    permalink is generated for you" fits what the screen shows, that the list opens at `/help-center`, and that the
    page "Reset your password" opens at `/reset-your-password` (the Page type of the Blog recipe uses the same
    pattern with a custom path allowed).
14. **Owned by me** (step `find-2`). The narration says it keeps the items you own. From the source, the filter keeps
    items whose owner is the signed-in user (`OrchardCore.Lists/Services/ContainerService.cs`,
    `ApplyContainedItemOptionsFilter`, `ContentsStatus.Owner`), and the owner of an item is set to the user who
    created it (`src/OrchardCore/OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:27`). The recording
    never chooses it. If you choose it once while rehearsing, all three rows should remain, since `admin` created them.

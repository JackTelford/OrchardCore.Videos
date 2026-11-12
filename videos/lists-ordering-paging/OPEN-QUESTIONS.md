# Open questions: Lists: Ordering, Paging and Contained Types

The narration says only what the source shows. These points could not be settled by reading the source: a person
confirms each one on the running site (the seed of `recording-plan.md`) before recording the clip that depends on it.
Paths are under `src/OrchardCore.Modules/OrchardCore.Lists` of the Orchard Core source unless they say otherwise.

## 1. Does a drop save the new order, and on which page? (`order-4`, `order-5`)

*Checked.* `Assets/Scripts/ci-list-ordering.js` reads the pager cursors from `data-before` and `data-after` and posts
them as `pagerSlimParameters[before]` and `pagerSlimParameters[after]` with `URLSearchParams`. `Views/ListPartDetailAdmin.cshtml:41-42`
writes them as `data-before="@Model.Pager.Before"` and `data-after="@Model.Pager.After"`. A `PagerSlim` has no `Before` on the
first page and no `After` on the last page, and neither on a list of one page (`Services/ContainerService.cs:263,311-312`),
and a shape returns null for a property that is null (`OrchardCore/OrchardCore.DisplayManagement/Shapes/Shape.cs:192-201`).
Razor leaves out an attribute whose value is null, so `dataset.before` (or `dataset.after`) is `undefined`, and
`URLSearchParams` sends the text `undefined` for it. `Controllers/OrderController.cs:30-54` then calls `int.Parse` on every
cursor that is not null, and `undefined` is not null, so it throws a `FormatException`: the request answers 500, `response.ok`
is false, and the page shows the alert "There was an error when updating the order of the items."
The source therefore shows that a drop fails on the first page, on the last page and on a list of one page, and works only
on a page between them, where both cursors are numbers. For the second page of three (Page Size 3, orders 3, 4 and 5, so
`Before` is 3 and `After` is 5) the controller turns the cursors into 6 and 2, the query returns orders 3, 4 and 5 again, and the
reorder hits the right rows (`Controllers/OrderController.cs:30-89`, `Services/ContainerService.cs:156-312`). This is a reading of the
source, not a run: it needs the confirmation below. The documentation says the change "is persisted immediately"
(`src/docs/reference/modules/Lists/README.md`); it does not mention the first and last page.
*What the video does.* `order-4` opens the second page and drags there, `order-5` shows the second page of the site, and no step
drags on the first, the last or a single page. `order-6` and the narration say nothing about dragging a post that was just created.
*Confirm.* (a) On `LIST` with Page Size 3 and Enable Ordering on, open the second page and drag the third row onto the first: no
alert opens; the request to `/Admin/Lists/Order` (the container id is in the body, not in the address) answers 200; after a reload
the second page of the admin list and the second page of `/garden-notes` (**Older Posts**) show **Compost basics for beginners** first.
(b) To test the diagnosis, not to record it: drag a row on the first page. If the alert opens (and the server log shows a
`FormatException`), report it to the maintainers, and do not add that drag to the video until it is fixed; if the drop is saved, the
narration and the plan work as they are, and the second page is only a precaution.
*If (a) fails,* do not record `05-ordering`: set the video to `blocked` with the finding, and report it to the maintainers.

## 2. Dragging with Show full pager (not in the video)

*Checked.* With Show full pager on, the pager is a `Pager` shape, which has no `Before` or `After` (`Drivers/ListPartDisplayDriver.cs:157-170`),
so the script has no cursors to send (see 1), and a request without cursors reads the first page (`Controllers/OrderController.cs:30-58`). On any
page after the first, a drop would reorder the wrong items, or fail. The video never drags with the full pager on: `05-ordering`
comes before `06-display`, and `display-2` only clicks a page number. Keep that order, and do not add a drag to `06-display`.
*Confirm* that page 3 in `display-2` shows handles on its two rows (ordering is on and the page has more than one item),
and that the narration does not mention them.

## 3. The label of the list button on the Content Items page (not named in the video)

*Checked.* `Views/ListPartSummaryAdmin.cshtml:10` puts the content type **name** of the contained type (`BlogPost`) in
"List {0} Items", while `Views/ListPartNavigationAdmin.cshtml:32` puts its **display name** (`Blog Post`). With the data
localization feature off, the first probably reads "List BlogPost Items". The video opens the list by its address and never
names that button. *Confirm* the text on screen before any video names it.

## 4. The drag handle (`order-3`)

*Checked.* The handle is the shape `ListPart_Sortable`, added to the `Selectors` zone of the row
(`Services/ListPartAdminListDisplayManager.cs:29`, `Views/ListPart.Sortable.cshtml`), which `OrchardCore.Contents/Views/Content.SummaryAdmin.cshtml`
draws first in the row, with a four-way arrow icon. *Confirm* that it sits at the start of each row, as `order-3` says
("at its start"), and that the admin list is in the List layout with no layout selector.

## 5. Front-end labels and the pager of TheBlogTheme (`size-4`, `size-5`, `order-5`)

*Checked.* `OrchardCore.Themes/TheBlogTheme/Views/Blog-ListPart.liquid:5-6` sets "← Newer Posts" and "Older Posts →", and
`OrchardCore/OrchardCore.DisplayManagement.Liquid/Tags/ShapePagerTag.cs` applies those texts only to the slim pager. The
narration says "Older Posts" and "Newer Posts" without the arrows, which cannot be spoken.
*Confirm* that `/garden-notes` shows only **Older Posts →** on page one, and both links on page two. With Show full pager
on, the theme has no pager views, so the site probably shows the default `<<`, `<`, numbers, `>` and `>>` instead of those
labels: the video does not show the site with the full pager, and the narration of `display-1` and `display-2` speaks of the admin page only.

## 6. The admin pager (`size-3`, `order-6`, `display-2`)

*Checked.* The admin theme shows the slim pager as arrow icons with the titles "Go to previous page." and "Go to next page."
(`OrchardCore.Themes/TheAdmin/Views/Pager_Previous.cshtml:14`, `Pager_Next.cshtml:14`), and the full pager as first, previous,
numbers, next and last (`OrchardCore.Modules/OrchardCore.Navigation/PagerShapesTableProvider.cs`, `Pager_Links`).
The `title` is merged into `Model.Tag`, which is the `li` of the list shape (`OrchardCore/OrchardCore.DisplayManagement/Shapes/CoreShapes.cs:84`), so the plan selects
`li[title='Go to next page.'] a`; a selector on `a[title=...]` matches nothing.
*Confirm* that the first page shows only the next arrow, the last page only the previous arrow, and that no "Items per page"
selector appears (`AllowPageSizeSelection` is off by default, `OrchardCore/OrchardCore.Navigation.Core/PagerOptions.cs`).

## 7. The seed (`recording-plan.md`)

*Checked.* The recipe has the shape of the Blog recipe's content step; `CreatedUtc` is kept by the import
(`OrchardCore/OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:34-40`), and `Owner` and `Author`, left out of the
recipe, stay the importing user (`OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:1076-1087`), so the posts say
"Posted by admin". *Confirm* that **JSON Import** accepts it unchanged ("Recipe imported successfully!"), that the admin page
of the list shows **Spring planting calendar** first and **Planning the vegetable beds** last, that `/garden-notes` answers,
and that each post's address, `/garden-notes/<slug>`, answers.

## 8. Numbering when Enable Ordering is turned on (`order-2`)

*Checked.* `Settings/ListPartSettingsDisplayDriver.cs:72-76` and `Services/ContainerService.cs:74-145` number the items from
the newest (0) to the oldest, each time the box goes from cleared to checked, so the list keeps its order. *Confirm* that the
admin page shows the same three posts in the same order right after the save. Turning the box off and on again would
number the items by date again and lose a manual order: restore a snapshot for a new take, never toggle the box.

## 9. The header card on a post's editor (`display-4`)

*Checked.* The card appears on a post's editor when the post has a `ContainedPart` with a `ListContentType` and the setting is on
(`Drivers/ContainedPartDisplayDriver.cs:93-141`); its text is the display text of the list (`OrchardCore.Contents/Views/Content.HeaderAdmin.cshtml`).
The card is placed at `Content:1` and the navigation card at `Content:1.5` (`Drivers/ContainedPartDisplayDriver.cs:129,153`), but the parts of a post sit at
positions 0 (Title), 1 (Autoroute), 2 and 3 of the type, so the source does not say whether the card is above or below the Title field. The narration says only that
the card appears on the editor page.
*Confirm* that the card "Garden Notes" is above the navigation card on the editor of the first post, and note where it sits against the Title field.

## 10. Messages and titles read from the source, not from a screen

"Edit Part - List" (`OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:22`), the message `The "List" part settings have been saved.`
(`OrchardCore.ContentTypes/Controllers/AdminController.cs:1084`), the part's name "List" in the **Parts** list (a part with no display name
loses the end "Part"), and the buttons **Edit Blog**, **Create Blog Post** and **New**. *Confirm* each on screen; the narration names only the
labels of the settings, **Create Blog Post**, **New**, **Article**, **Blog Post**, **Edit**, **Older Posts** and **Newer Posts**.

## 11. The search of the list (`order-7`)

*Checked.* The search box and the filter links use unprefixed names (`name="DisplayText"`, `?Status=Draft`:
`Views/ListPartDetailAdminSearchPanel.cshtml:25-31`), and the list binds them with `TryUpdateModelAsync(listPartFilterViewModel, Prefix)`
(`Drivers/ListPartDisplayDriver.cs:131,147`). The part driver sets `Prefix` to the part name only inside `using (BuildPrefix(...))` while it builds its
result (`OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentPartDisplayDriverTPart.cs:78-89,201-212`), the shape is initialized later,
when the result is applied (`OrchardCore/OrchardCore.DisplayManagement/Views/ShapeResult.cs`), and the driver's `Prefix` starts empty
(`OrchardCore/OrchardCore.DisplayManagement/Handlers/DisplayDriverBase.cs:7`) and is restored when the `using` ends. The filter is therefore read without a
prefix, which matches the names of the search box. The narration relies on this.
*Confirm* that a search for "garden" narrows the list to two rows, **Dealing with garden pests** and **Choosing tools for a small garden**, and
that neither row has a handle. *If the search does not filter,* drop `order-7` (and its sentence) and record the rest.

## 12. What else is on the part settings page (`open-3`)

*Checked.* Besides the five settings of the List part, `ContentTypes/Views/Admin/EditTypePart.cshtml` draws the green note about the Part Definition (line 29),
shortcut links added by other features (Placement and Template drivers, location `Shortcuts`), and, because the view `Lists/Views/ListPart.DisplayOption.cshtml` exists,
the drop-down "What type of display mode should be used?" with the option **Standard** (lines 88-103). The narration names the five settings of the part
and gives no count. *Confirm* on screen that nothing else on the page looks like a sixth setting of the List part, and that the five labels can be read without scrolling
in the recorder's viewport (the plan scrolls by 300 px if not).

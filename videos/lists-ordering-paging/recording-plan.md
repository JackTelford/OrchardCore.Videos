# Recording plan: Lists: Ordering, Paging and Contained Types

Read this file and nothing else to record the video. Narrate first (`narrate.py`), then record the clips in the order
below, each one from the snapshot named for it. Selectors in this plan stand for `scene.Page.Locator(...)`. Every
`file:line` is in the Orchard Core source, under `src/OrchardCore.Modules` unless the path starts with `OrchardCore/` or
`OrchardCore.Themes/`; a module there is written without its `OrchardCore.` prefix (`Lists` is `OrchardCore.Lists`,
`ContentTypes` is `OrchardCore.ContentTypes`, `Contents`, `Deployment` and `Navigation` follow the same rule).

## The site it needs

- **Host:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with its own
  `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`), provisioned by Auto Setup (`references/recording.md`).
  Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site calls nothing outside.
- **Auto Setup values:** recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`,
  email `admin@example.com`, password `<password>` (chosen outside git, passed to the recorder in `VIDEO_PASSWORD`),
  the default SQLite database. This is not a video about the Setup screen: record with Auto Setup and a `state.json`.
- **Features:** none to enable. The Blog recipe enables `OrchardCore.Lists`, `OrchardCore.Contents`,
  `OrchardCore.ContentTypes` and `OrchardCore.Deployment` (needed by the seed), and sets the themes **TheAdmin** (admin)
  and **TheBlogTheme** (site) (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` and `themes` steps).
- **Settings to leave alone:** the admin list layout stays **List**, and the layout selector stays off
  (`AdminListOptions`: `DefaultLayout` List, `AllowUserSelection` false); the pager options stay at their defaults, so the
  "Items per page" selector is not on screen (`AllowPageSizeSelection` false). Do not change either for this video.
- **List part settings at the start** (the Blog recipe's, `blog.recipe.json:303-316`): the type **Blog** has the **List**
  part with Page Size 10 and Contained Content Types **Blog Post** only; **Enable Ordering**, **Show container's header**
  and **Show full pager** are cleared. The type **Blog Post** is not creatable on its own, so it is created from the list's buttons.
- **Browser:** the recorder's defaults (1536x864, light, en-US). The admin URL prefix is the default, `/Admin`.

### The fake seed data (create it outside the capture)

The Blog recipe gives its own blog a random id, so the seed creates a separate blog, **Garden Notes**, with fixed ids, at
`/garden-notes`, and eight posts. The sample blog of the recipe stays untouched and is never shown.

1. Sign in to the admin as `admin`.
2. Open **Tools, Deployments, JSON Import** (`Deployment/AdminMenu.cs:30`, route `Deployment/Controllers/ImportController.cs:20`; page `/Admin/DeploymentPlan/Import/Json`).
3. Set the **JSON** field (a CodeMirror editor: set it through the editor object of the page, or paste) to the recipe
   below, and select **Import**. The page says "Recipe imported successfully!" (`Deployment/Controllers/ImportController.cs`,
   the `Json` action).

The container is `4gardennotesblog0000000001`; the posts are `4gardennotespost0000000001` to `4gardennotespost0000000008`,
created one week apart (a `CreatedUtc` per post, which the import keeps: `OrchardCore/OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:34-40`),
so "newest first" is the same on every take. Each post names its list and the list's content type
(`ListContentType`), which the post editor needs for its navigation and header (`Lists/Drivers/ContainedPartDisplayDriver.cs:30-66,93-141`).

```json
{
  "steps": [
    {
      "name": "Content",
      "data": [
        {"ContentItemId": "4gardennotesblog0000000001", "ContentType": "Blog", "DisplayText": "Garden Notes", "Latest": true, "Published": true, "CreatedUtc": "2025-03-01T09:00:00Z", "ModifiedUtc": "2025-03-01T09:00:00Z", "PublishedUtc": "2025-03-01T09:00:00Z", "TitlePart": {"Title": "Garden Notes"}, "HtmlBodyPart": {"Html": "Notes from a small community garden."}, "AutoroutePart": {"Path": "garden-notes"}, "ListPart": {}},
        {"ContentItemId": "4gardennotespost0000000001", "ContentType": "BlogPost", "DisplayText": "Planning the vegetable beds", "Latest": true, "Published": true, "CreatedUtc": "2025-03-03T09:00:00Z", "ModifiedUtc": "2025-03-03T09:00:00Z", "PublishedUtc": "2025-03-03T09:00:00Z", "TitlePart": {"Title": "Planning the vegetable beds"}, "ContainedPart": {"ListContentItemId": "4gardennotesblog0000000001", "ListContentType": "Blog", "Order": 0}, "MarkdownBodyPart": {"Markdown": "Sketch the plot first, then mark the sunny corners."}, "AutoroutePart": {"Path": "garden-notes/planning-the-vegetable-beds"}, "BlogPost": {"Subtitle": {"Text": "Where to put each bed"}}},
        {"ContentItemId": "4gardennotespost0000000002", "ContentType": "BlogPost", "DisplayText": "Starting tomatoes from seed", "Latest": true, "Published": true, "CreatedUtc": "2025-03-10T09:00:00Z", "ModifiedUtc": "2025-03-10T09:00:00Z", "PublishedUtc": "2025-03-10T09:00:00Z", "TitlePart": {"Title": "Starting tomatoes from seed"}, "ContainedPart": {"ListContentItemId": "4gardennotesblog0000000001", "ListContentType": "Blog", "Order": 0}, "MarkdownBodyPart": {"Markdown": "Sow the seeds indoors, in small pots, near a bright window."}, "AutoroutePart": {"Path": "garden-notes/starting-tomatoes-from-seed"}, "BlogPost": {"Subtitle": {"Text": "Six weeks before the last frost"}}},
        {"ContentItemId": "4gardennotespost0000000003", "ContentType": "BlogPost", "DisplayText": "Compost basics for beginners", "Latest": true, "Published": true, "CreatedUtc": "2025-03-17T09:00:00Z", "ModifiedUtc": "2025-03-17T09:00:00Z", "PublishedUtc": "2025-03-17T09:00:00Z", "TitlePart": {"Title": "Compost basics for beginners"}, "ContainedPart": {"ListContentItemId": "4gardennotesblog0000000001", "ListContentType": "Blog", "Order": 0}, "MarkdownBodyPart": {"Markdown": "Mix green scraps with dry leaves, and keep the pile damp."}, "AutoroutePart": {"Path": "garden-notes/compost-basics-for-beginners"}, "BlogPost": {"Subtitle": {"Text": "Turn kitchen scraps into soil"}}},
        {"ContentItemId": "4gardennotespost0000000004", "ContentType": "BlogPost", "DisplayText": "Choosing tools for a small garden", "Latest": true, "Published": true, "CreatedUtc": "2025-03-24T09:00:00Z", "ModifiedUtc": "2025-03-24T09:00:00Z", "PublishedUtc": "2025-03-24T09:00:00Z", "TitlePart": {"Title": "Choosing tools for a small garden"}, "ContainedPart": {"ListContentItemId": "4gardennotesblog0000000001", "ListContentType": "Blog", "Order": 0}, "MarkdownBodyPart": {"Markdown": "A trowel, a fork and a pair of shears cover most jobs."}, "AutoroutePart": {"Path": "garden-notes/choosing-tools-for-a-small-garden"}, "BlogPost": {"Subtitle": {"Text": "Fewer tools, used more often"}}},
        {"ContentItemId": "4gardennotespost0000000005", "ContentType": "BlogPost", "DisplayText": "Watering without waste", "Latest": true, "Published": true, "CreatedUtc": "2025-03-31T09:00:00Z", "ModifiedUtc": "2025-03-31T09:00:00Z", "PublishedUtc": "2025-03-31T09:00:00Z", "TitlePart": {"Title": "Watering without waste"}, "ContainedPart": {"ListContentItemId": "4gardennotesblog0000000001", "ListContentType": "Blog", "Order": 0}, "MarkdownBodyPart": {"Markdown": "A slow soak in the morning beats a daily sprinkle."}, "AutoroutePart": {"Path": "garden-notes/watering-without-waste"}, "BlogPost": {"Subtitle": {"Text": "Water early, water deep"}}},
        {"ContentItemId": "4gardennotespost0000000006", "ContentType": "BlogPost", "DisplayText": "Companion planting guide", "Latest": true, "Published": true, "CreatedUtc": "2025-04-07T09:00:00Z", "ModifiedUtc": "2025-04-07T09:00:00Z", "PublishedUtc": "2025-04-07T09:00:00Z", "TitlePart": {"Title": "Companion planting guide"}, "ContainedPart": {"ListContentItemId": "4gardennotesblog0000000001", "ListContentType": "Blog", "Order": 0}, "MarkdownBodyPart": {"Markdown": "Basil beside tomatoes, and marigolds along the edges."}, "AutoroutePart": {"Path": "garden-notes/companion-planting-guide"}, "BlogPost": {"Subtitle": {"Text": "Neighbors that help each other"}}},
        {"ContentItemId": "4gardennotespost0000000007", "ContentType": "BlogPost", "DisplayText": "Dealing with garden pests", "Latest": true, "Published": true, "CreatedUtc": "2025-04-14T09:00:00Z", "ModifiedUtc": "2025-04-14T09:00:00Z", "PublishedUtc": "2025-04-14T09:00:00Z", "TitlePart": {"Title": "Dealing with garden pests"}, "ContainedPart": {"ListContentItemId": "4gardennotesblog0000000001", "ListContentType": "Blog", "Order": 0}, "MarkdownBodyPart": {"Markdown": "Check the undersides of leaves, and pick off what you find."}, "AutoroutePart": {"Path": "garden-notes/dealing-with-garden-pests"}, "BlogPost": {"Subtitle": {"Text": "Gentle ways to keep leaves intact"}}},
        {"ContentItemId": "4gardennotespost0000000008", "ContentType": "BlogPost", "DisplayText": "Spring planting calendar", "Latest": true, "Published": true, "CreatedUtc": "2025-04-21T09:00:00Z", "ModifiedUtc": "2025-04-21T09:00:00Z", "PublishedUtc": "2025-04-21T09:00:00Z", "TitlePart": {"Title": "Spring planting calendar"}, "ContainedPart": {"ListContentItemId": "4gardennotesblog0000000001", "ListContentType": "Blog", "Order": 0}, "MarkdownBodyPart": {"Markdown": "Cool-season crops first, tender plants after the last frost."}, "AutoroutePart": {"Path": "garden-notes/spring-planting-calendar"}, "BlogPost": {"Subtitle": {"Text": "What goes in the ground, and when"}}}
      ]
    }
  ]
}
```

Check the seed before taking a snapshot:

- `/Admin/Contents/ContentItems/4gardennotesblog0000000001/Display` is titled **Garden Notes** and lists eight posts,
  **Spring planting calendar** first and **Planning the vegetable beds** last, with the buttons **Edit Blog** and
  **Create Blog Post**, a **Search** box, and no pager.
- `/garden-notes` shows the posts newest first (all eight, since Page Size is 10), each "Posted by admin".
- Nothing on screen has a real name, address or number (see the checks at the end).

## Snapshots, clip order and what each clip changes

Stop the site, copy `App_Data` and `state.json` aside after each row below, and restore the copy named in "Starts from"
before every take of a clip. A clip needs the state its predecessor left, so the clips are recorded in play order.

| Order | Clip | Starts from | Changes the site | Snapshot after |
|---|---|---|---|---|
| 1 | `02-open` | S0 (after the seed) | No | S0 |
| 2 | `03-types` | S0 | Yes: the Blog type's List part also accepts **Article** | S1 |
| 3 | `04-size` | S1 | Yes: Page Size becomes 3 | S2 |
| 4 | `05-ordering` | S2 | Yes: Enable Ordering is on, the posts are numbered, and the third post of the second page is moved to the top of that page | S3 |
| 5 | `06-display` | S3 | Yes: Show full pager and Show container's header are on | S4 |

The slide clip `01-settings` and the cards `00-intro` and `99-outro` need no site.

Two addresses are used in every clip (the admin prefix is `/Admin`):

- **LIST** = `/Admin/Contents/ContentItems/4gardennotesblog0000000001/Display`, the admin page of the list
  (`Contents/Controllers/AdminController.cs:482`).
- **SETTINGS** = `/Admin/ContentTypes/Blog/ContentParts/ListPart/Edit`, the settings of the List part of the Blog type
  (`ContentTypes/Controllers/AdminController.cs:966`). Its title is "Edit Part - List" (`ContentTypes/Views/Admin/EditTypePart.cshtml:22`).
  After **Save**, the site goes to the page **Edit Content Type - Blog** and shows the green message
  `The "List" part settings have been saved.` (`ContentTypes/Controllers/AdminController.cs:1084,1087`).

Guards for every take: register a handler for the page's dialogs and stop the take if one opens (an alert after a drop
means the order was not saved: see `OPEN-QUESTIONS.md`), and clear every highlight before a scroll.

## 00-intro (card)

`intro` is the opening card. It shows the title and the subtitle from the storyboard and needs no recording.

## 01-settings (slides, drawn by `slides.py`)

- `settings-1`: the slide "A list and the items it holds", with three cards: The list, The contained items, The content type.
- `settings-2`: the slide "The settings of the List part", four cards (Contained Content Types, Page Size, Enable Ordering,
  Display options); the first two are bright.
- `settings-3`: the same slide; the last two cards are bright.

## 02-open (clip)

Starts on `/Admin/ContentTypes/List` (opened before `StartCaptureAsync`). Source of the page: `ContentTypes/Controllers/AdminController.cs:72`.
The page title is "Content Types". It changes nothing.

- `open-1`: move to the Blog row and highlight its **Edit** button, then `ClickAndWaitForNavigationAsync(a[role='btn-edit-Blog'])`.
  *On screen when the narration ends:* the page "Edit Content Type - Blog", with the **Parts** heading and its list in view.
  *Labels:* the menu path Design, Content Definition, Content Types is `ContentTypes/AdminMenu.cs:23-25`; the **Edit**
  button is `ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`; the title is `ContentTypes/Views/Admin/Edit.cshtml:63`.
- `open-2`: scroll to the **Parts** list if needed, highlight the **List** row (its hint reads "Add a list behavior."), then
  `ClickAndWaitForNavigationAsync(a[href$='/ContentTypes/Blog/ContentParts/ListPart/Edit'])`, the **Edit** button of that row.
  *On screen:* the page "Edit Part - List" with the form of the part, starting at **Page Size**.
  *Labels:* **Parts** `ContentTypes/Views/Admin/Edit.cshtml:144`; the row is named "List" because a part with no display name
  loses the end "Part" (`OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-63`); the hint is `Lists/Migrations.cs:23`; the **Edit** button is `ContentTypes/Views/Admin/Edit.cshtml:163`.
- `open-3`: move the cursor over the five settings in this order, about 0.8 s each, highlighting the label (clear it before
  the next): **Page Size**, **Enable Ordering**, **Contained Content Types**, **Show container's header**, **Show full pager**.
  If the last one is below the fold, `ScrollAsync(0, 300)` while the narration names it (after clearing the highlight).
  *On screen:* the form with the five settings, the cursor on **Show full pager**; nothing is changed or saved. The page
  holds more than the five settings, and the narration points at none of it: the green note "Some part settings common to
  all types can also be defined at the Part Definition level." above the form (`ContentTypes/Views/Admin/EditTypePart.cshtml:29`),
  possibly shortcut links of other features above the settings, and, below them, the drop-down "What type of display mode
  should be used?" with the single option **Standard** (`EditTypePart.cshtml:91`, `Lists/Views/ListPart.DisplayOption.cshtml:4`).
  *Labels:* `Lists/Views/ListPartSettings.Edit.cshtml:4,16,23,34,44`.

## 03-types (clip)

Starts on LIST (state S0), opened before `StartCaptureAsync`.

- `types-1`: highlight the **Create Blog Post** button (`a:has-text('Create Blog Post')`), then the list of posts, no click.
  *On screen:* the title "Garden Notes", the card with **Edit Blog** and **Create Blog Post**, the search box and the eight posts.
  *Labels:* `Lists/Views/ListPartNavigationAdmin.cshtml:43,52` ("Edit {0}", "Create {0}" with the display names "Blog" and "Blog Post").
- `types-2`: `NavigateAsync(SETTINGS)`; scroll to **Contained Content Types** and highlight the group of checkboxes
  (`.content-types`) for 1 s; clear the highlight; `ClickAsync(.content-types label:text-is('Article'))`;
  `ClickAndWaitForNavigationAsync(button.save)` (the **Save** button).
  *On screen:* the page "Edit Content Type - Blog" with the green message `The "List" part settings have been saved.`
  *Labels:* the checkboxes are named after the types' display names, with a **Select all** box first
  (`Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml`); the error for no type is
  "At least one content type must be selected." (`Lists/Settings/ListPartSettingsDisplayDriver.cs:59`), not shown on camera;
  **Save** `ContentTypes/Views/Admin/EditTypePart.cshtml:111`.
- `types-3`: `NavigateAsync(LIST)`, then `ClickAsync(a.dropdown-toggle:has-text('New'))`.
  *On screen:* the **New** menu open under the card, with two entries, **Article** then **Blog Post** (sorted by display
  name: `Contents/ViewModels/SelectContentTypesViewModel.cs:26`), and **Edit Blog** next to it. The single **Create Blog Post** button is gone.
  *Labels:* `Lists/Views/ListPartNavigationAdmin.cshtml:56-69`.

## 04-size (clip)

Starts on LIST (state S1), opened before `StartCaptureAsync`.

- `size-1`: no action beyond a slow cursor move down the list.
  *On screen:* all eight posts, from **Spring planting calendar** to **Planning the vegetable beds**, and no pager arrow below them.
  *Why:* Page Size 10 holds eight posts; newest first is `Lists/Services/ContainerService.cs:353,392`.
- `size-2`: `NavigateAsync(SETTINGS)`; click the **Page Size** field (`.ocat-wrapper:has(label:text-is('Page Size')) input`),
  select its content (`Control+A`), type `3`, press Tab; `ClickAndWaitForNavigationAsync(button.save)`.
  *On screen:* the page "Edit Content Type - Blog" with the green message `The "List" part settings have been saved.`
  *Labels:* **Page Size** `Lists/Views/ListPartSettings.Edit.cshtml:4`, hint at `:8`.
- `size-3`: `NavigateAsync(LIST)`; highlight the arrow below the rows (`li[title='Go to next page.'] a`).
  *On screen:* three posts (**Spring planting calendar**, **Dealing with garden pests**, **Companion planting guide**) and,
  below them, one arrow pointing right. There is no arrow to the left on the first page.
  *Labels:* the title "Go to next page." is on the list item `li` that holds the arrow link, not on the link: `OrchardCore.Themes/TheAdmin/Views/Pager_Next.cshtml:14`
  merges it into `Model.Tag`, which is the item the list shape creates (`OrchardCore/OrchardCore.DisplayManagement/Shapes/CoreShapes.cs:84`). Select the link as
  `li[title='Go to next page.'] a`. The admin list uses the slim pager (`Lists/Drivers/ListPartDisplayDriver.cs:173-181`).
- `size-4`: `NavigateAsync('/garden-notes')`; scroll down until the three post previews and the link **Older Posts →** are in view.
  *On screen:* the Blog page of TheBlogTheme with the same three posts and the link **Older Posts →** under them.
  *Labels:* `OrchardCore.Themes/TheBlogTheme/Views/Blog-ListPart.liquid:5-6` (the arrows are part of the labels).
- `size-5`: `ClickAndWaitForNavigationAsync(a:has-text('Older Posts'))`; scroll until the links are in view.
  *On screen:* page two, with **Watering without waste**, **Choosing tools for a small garden** and **Compost basics for
  beginners**, and both links, **← Newer Posts** and **Older Posts →**, under them.
  *Labels:* the same template as `size-4` (`OrchardCore.Themes/TheBlogTheme/Views/Blog-ListPart.liquid:5-6`); the second page has a previous cursor, so the slim pager adds the first link (`Navigation/PagerShapesTableProvider.cs`, `PagerSlim`).

## 05-ordering (clip)

Starts on SETTINGS (state S2), opened before `StartCaptureAsync`. The List part's other settings stay as they are.

- `order-1`: highlight the **Enable Ordering** line and its hint "Check this option to enable manual ordering of items.", no click.
  *On screen:* the line, with its box cleared. *Labels:* `Lists/Views/ListPartSettings.Edit.cshtml:16-17`.
- `order-2`: `ClickAsync(label:text-is('Enable Ordering'))`, then `ClickAndWaitForNavigationAsync(button.save)`.
  *On screen:* the green message `The "List" part settings have been saved.`
  *Why:* turning it on numbers the existing items by creation date, newest first (`Lists/Settings/ListPartSettingsDisplayDriver.cs:72-76`, `Lists/Services/ContainerService.cs:74-145`).
- `order-3`: `NavigateAsync(LIST)`; highlight the first row's handle (`#ci-sortable .ui-sortable-handle >> nth=0`).
  *On screen:* the same three posts as before, in the same order, each row with a four-way arrow handle at its start;
  no layout selector; the arrow to the next page below. *Labels:* the handle is `Lists/Views/ListPart.Sortable.cshtml`;
  it shows only for a page of more than one item with no search or filter (`Lists/Drivers/ListPartDisplayDriver.cs:184-186`).
- `order-4`: `ClickAndWaitForNavigationAsync(li[title='Go to next page.'] a)` to open the second page (the item with the title "Go to next page."
  holds the link: see `size-3`), then
  `DragToAsync(#ci-sortable .ui-sortable-handle >> nth=2, #ci-sortable .ui-sortable-handle >> nth=0)` with a small
  negative vertical offset, so the row lands before the first; wait for the response of the POST to `/Admin/Lists/Order`
  to be successful (a guard only, not part of the story).
  *On screen:* the second page with **Compost basics for beginners** first, then **Watering without waste**, then
  **Choosing tools for a small garden**, and no alert. Before the drop the page shows them in the order **Watering without waste**,
  **Choosing tools for a small garden**, **Compost basics for beginners**.
  *Why the second page:* the script posts the pager cursors `data-before` and `data-after`, and the first and the last page of
  a list have only one of them (a single page has none): see `OPEN-QUESTIONS.md`, 1. The second page of three has both. Never drag on the
  first or the last page in this video.
  *Source:* `Lists/Assets/Scripts/ci-list-ordering.js` (the drop), `Lists/Controllers/OrderController.cs:27` (the save, with the route `Lists/Order/{containerId?}`: the script posts to `/Admin/Lists/Order` and sends the container id in the body).
- `order-5`: `NavigateAsync('/garden-notes')`; scroll down to **Older Posts →**; `ClickAndWaitForNavigationAsync(a:has-text('Older Posts'))`;
  scroll until the first posts are in view.
  *On screen:* the second page of the Blog page with **Compost basics for beginners** first, then **Watering without waste**, then
  **Choosing tools for a small garden**, and both links, **← Newer Posts** and **Older Posts →**, under them. (The first page, which
  `size-4` showed, is unchanged: the drop moved a post inside the second page.)
- `order-6`: `NavigateAsync(LIST)`, then `ClickAndWaitForNavigationAsync(li[title='Go to next page.'] a)` twice.
  *On screen:* the last page, with **Starting tomatoes from seed** and **Planning the vegetable beds**, and only the arrow to the previous page below them (`li[title='Go to previous page.'] a`). No post is created on camera.
  *Why:* a new item gets the next order number, after the last one (`Lists/Services/ContainerService.cs:30-43`, `Lists/Drivers/ContainedPartDisplayDriver.cs:56-59,83-86`), so it lands on the last page. Do not drag on this page.
- `order-7`: `NavigateAsync(LIST)`; `TypeAsync(input[name='DisplayText'], 'garden')`; `WaitForNavigationAsync(() => Page.Keyboard.PressAsync('Enter'))`.
  *On screen:* the search box holds "garden", and two rows, **Dealing with garden pests** and **Choosing tools for a small garden**, with no handle on either.
  *Labels:* the box's placeholder is **Search** (`Lists/Views/ListPartDetailAdminSearchPanel.cshtml:30`); a search or a status filter turns ordering off (`Lists/Drivers/ListPartDisplayDriver.cs:184-186`).

## 06-display (clip)

Starts on SETTINGS (state S3), opened before `StartCaptureAsync`.

- `display-1`: highlight the **Show full pager** line and its hint for 2 s; `ClickAsync(label:text-is('Show full pager'))`;
  `ClickAndWaitForNavigationAsync(button.save)`.
  *On screen:* the green message `The "List" part settings have been saved.`
  *Labels:* `Lists/Views/ListPartSettings.Edit.cshtml:44-45` (the hint: "When enabled, the list uses a full pager with numbered pages. Note: This requires an additional database query to determine the total item count.").
- `display-2`: `NavigateAsync(LIST)`; highlight the pager under the rows (`nav[aria-label='Listing pages']`); then `ClickAndWaitForNavigationAsync(nav[aria-label='Listing pages'] a:text-is('3'))`.
  *On screen:* page 3 of the list, and the pager with the first-page and previous-page arrows, the numbers 1, 2 and 3 (3 active), and the next-page and last-page arrows.
  *Labels:* the arrows' titles are `OrchardCore.Themes/TheAdmin/Views/Pager_First.cshtml:14`, `Pager_Previous.cshtml:14`, `Pager_Next.cshtml:14`, `Pager_Last.cshtml:14`; the numbered pager is `Navigation/PagerShapesTableProvider.cs` (`Pager_Links`). Do not drag on this page.
- `display-3`: `NavigateAsync(SETTINGS)`; highlight the **Show container's header** line and its hint "Check this option to show a header
  for the owning content."; `ClickAsync(label:text-is("Show container's header"))`; `ClickAndWaitForNavigationAsync(button.save)`.
  *On screen:* the green message `The "List" part settings have been saved.`
  *Labels:* `Lists/Views/ListPartSettings.Edit.cshtml:34-35`.
- `display-4`: `NavigateAsync(LIST)`; highlight the card above the navigation card; then
  `ClickAndWaitForNavigationAsync(#ci-sortable .btn.edit >> nth=0)`, the **Edit** button of the first row.
  *On screen:* the editor of the first post of the list (**Spring planting calendar**), with a card holding "Garden Notes" above the navigation card. The card
  and the navigation card are placed at `Content:1` and `Content:1.5`, which share their numbers with the parts of the post (TitlePart is 0, AutoroutePart is 1),
  so where the card sits against the Title field is not known from the source: look at the screen, and keep the narration as it is (it says only that the card appears on the editor page).
  *Source:* the card on the list page `Lists/Drivers/ListPartDisplayDriver.cs:112`; on a post's editor `Lists/Drivers/ContainedPartDisplayDriver.cs:131-134`; its text is the list's display text (`Contents/Views/Content.HeaderAdmin.cshtml`).

## 99-outro (card)

`outro` is the closing card (subtitle "docs.orchardcore.net"). It needs no recording.

## Checks for private data

- Everything is fake: the user `admin` with `admin@example.com`, the blog "Garden Notes", and eight made-up post titles.
  No other user, email address, phone number, token or domain is needed; the address bar shows `localhost`.
- The post pages say "Posted by admin" (the importing user). No other name appears.
- Never type or show the real password: the recorder signs in outside the capture (`state.json`, kept out of git).
- After assembling, OCR the video at one frame per second and at every scene change, as `SKILL.md` asks, and search the text
  for anything that is not on the list above. The sample blog and its sample post of the Blog recipe must not appear.

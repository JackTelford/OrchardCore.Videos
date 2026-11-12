# Recording plan: Content Items List: Search, Filter and Act on Many Items

This plan is for the person who records the video. It needs no decisions: the site, the fake data, the order of the
clips and, for every step, the actions and what the screen must show when the narration of that step ends. The
narration is in `storyboard.json`; run `narrate.py` first, so the recorder holds each step for the length of its
narration (about 0.4 seconds per word; the approximate length of each step is written next to it below).

This is an admin video: it records the admin and shows no code. A selector in this file stands for
`scene.Page.Locator(...)`. Selectors are derived from the Orchard Core source and are marked "probe": confirm each one
once with `Recorder probe <path> "<script>"` before the first take.

Source references are `file:line` in the Orchard Core checkout, read at commit `92db4a83`. A path is relative to
`src/OrchardCore.Modules/OrchardCore.Contents/` unless it starts with another module (`OrchardCore.Admin/`,
`OrchardCore.ContentTypes/`, `OrchardCore.Settings/`, `OrchardCore.Title/`) or with `TheAdmin/`
(`src/OrchardCore.Themes/TheAdmin/`). If a label on the running site differs from the source, the source file wins:
fix the narration, not the site.

## 1. The site

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of a checkout at commit `92db4a83` or later |
| App data | An empty folder outside both repositories, in `ORCHARD_APP_DATA`. Never a copy of another site's `App_Data` |
| URL | `http://localhost:5010` (`VIDEO_BASE_URL` otherwise) |
| Provisioning | Auto Setup, so no setup or sign-in form is recorded (below) |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Recipe | `Blog` |
| Administrator | user `admin`, email `admin@example.com`, password chosen outside git and passed in `VIDEO_PASSWORD`; write it as `<password>` everywhere in the repository |
| Features | Only those the Blog recipe enables. Enable nothing else. In particular the features "Add Content To Deployment Plan", "Export Content To Deployment Target" and "View Or Download Content As JSON" (ids `OrchardCore.Contents.Deployment.*`, `Manifest.cs:47,60,68`) must stay disabled: the Blog recipe enables the Deployment module but not these, and with them on the **Actions** menus gain entries the narration does not mention (`Deployment/AddToDeploymentPlan/AddToDeploymentPlanContentsAdminListDisplayDriver.cs:22`, `Deployment/ExportContentToDeploymentTarget/ExportContentToDeploymentTargetContentsAdminListDisplayDriver.cs:31`) |
| Admin settings | The defaults: **List layout** and **Actions layout** on "Use configuration default", **Let users choose the layout of a list** checked (`OrchardCore.Admin/Views/AdminSettings.Edit.cshtml:86-115`) |
| Page size | The list must show 10 items per page. If Settings, General, **Page size** holds another number, set it to 10 before the snapshot (`OrchardCore.Settings/AdminMenu.cs:36-39`, `OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:54`) |
| Outbound calls | Start the site with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, for the site's process only |
| External services | None |

Auto Setup, as environment variables of the site's process (the keys are listed in
`docs/reference/modules/AutoSetup/README.md`):

```bash
export ORCHARD_APP_DATA=<empty folder outside both repositories>
export ASPNETCORE_URLS=http://localhost:5010
export OrchardCore__AutoSetup__AutoSetupPath=""
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

Sign the recorder in once, outside the capture (`Recorder setup` with `VIDEO_USER_NAME=admin` and
`VIDEO_PASSWORD`), which writes `state.json` (git-ignored, never committed).

## 2. The fake data to create outside the capture

The video needs 15 listable content items: the 5 that the Blog recipe creates, and the 10 below. The invented subject
is a hiking club; no real people, companies or addresses appear anywhere.

What the Blog recipe creates, and shows in the list: **Main Menu** (type Menu), **Tags** and **Categories** (type
Taxonomy), **Blog** (type Blog) and **About** (type Article). The recipe also creates a Blog Post and a Footer
widget; neither shows in the unfiltered list, because their types are not Listable
(`Services/DefaultContentsAdminListFilterProvider.cs:182`). Leave these items as they are; they are the old ones.

Create the 10 items **in this order**, a few seconds apart, signed in as `admin` (the list orders by the date of the last
modification, so the order below is the order on screen). For an Article open
`/Admin/Contents/ContentTypes/Article/Create`; for a Page open `/Admin/Contents/ContentTypes/Page/Create`
(`Controllers/AdminController.cs:407`). Type only the **Title** (label "Title",
`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`), leave every other field empty, and click **Publish** or
**Save Draft** (`Views/Content.PublishButton.cshtml:7`, `Views/Content.SaveDraftButton.cshtml:7`). If an editor refuses
to save because another field is required, fill that field with the text `Sample` and write it in `OPEN-QUESTIONS.md`.

| # | Title | Type | Button to click | Result |
|---|---|---|---|---|
| 1 | Membership | Page | Publish | Published |
| 2 | Contact Us | Page | Publish | Published |
| 3 | Spring Trail Cleanup Day | Article | Publish | Published |
| 4 | Summer Hiking Schedule | Article | Publish | Published |
| 5 | Choosing Hiking Boots | Article | Publish | Published |
| 6 | Trail Safety Basics | Article | Publish | Published |
| 7 | Placeholder Notice 1 | Article | Save Draft | Draft only |
| 8 | Placeholder Notice 2 | Article | Save Draft | Draft only |
| 9 | Winter Snowshoe Schedule | Article | Save Draft | Draft only |
| 10 | Autumn Hiking Schedule | Article | Save Draft | Draft only |

Then edit item 6: open `/Admin/Contents/ContentItems`, click **Edit** on "Trail Safety Basics", change the Title to
`Trail Safety Basics Revised` and click **Save Draft** (not Publish). The item keeps its published version and gains
a newer draft, so the list shows both badges on it. This edit is done last, so that item is the most recently modified.

The list must look like this (check it before taking the snapshot):

- `/Admin/Contents/ContentItems` shows "10 items / 15 items in total" and a pager. Page 1, from the top: Trail Safety
  Basics Revised (Published and Draft badges), Autumn Hiking Schedule (Draft), Winter Snowshoe Schedule (Draft),
  Placeholder Notice 2 (Draft), Placeholder Notice 1 (Draft), Choosing Hiking Boots (Published), Summer Hiking
  Schedule (Published), Spring Trail Cleanup Day (Published), Contact Us (Published), Membership (Published). Page 2
  holds the 5 items of the Blog recipe.
- Searching `schedule` finds exactly 3 items: Autumn Hiking Schedule, Winter Snowshoe Schedule, Summer Hiking Schedule.
- The Filters menu entry "Only draft items" (the link `?q=status:Draft`) finds exactly 5 items: Trail Safety Basics
  Revised, Autumn Hiking Schedule, Winter Snowshoe Schedule, Placeholder Notice 2, Placeholder Notice 1.
- The Content Type picker, Article, finds 9 items (the 8 articles above and About).

Stop the site, copy its app data folder and `state.json` aside (the **snapshot**), and start the site again.

## 3. Order of the clips, and what each one changes

Record in this order. `02-list`, `03-search` and `04-filter` only read the site: they can be recorded in any order and
as often as needed. `05-bulk` publishes 2 items and deletes 2: record it **last**, and before every new take of it stop
the site, restore the snapshot of section 2, and start the site again.

| Order | Clip | Starts on | Changes the site? |
|---|---|---|---|
| 1 | `02-list` | `/Admin` | No. It clicks Grid and then List, which sets a cookie of the browser context only |
| 2 | `03-search` | `/Admin/Contents/ContentItems` | No |
| 3 | `04-filter` | `/Admin/Contents/ContentItems` | No |
| 4 | `05-bulk` | `/Admin/Contents/ContentItems?q=status:Draft` | **Yes**: restore the snapshot before every take |

The cards and the slides (section 4) need no recording.

## 4. Cards and slides

- `intro`: the opening card. Nothing to record.
- `idea-1`: the slide "What each row tells you": four cards, Title, Type, Status, Last modified.
- `idea-2`: the slide "Four ways to narrow and order the list": four cards, Search, Content Type, Show, Sort.
- `idea-3`: the slide "Commands for many items": three cards, Publish Now, Unpublish, Delete.
- `outro`: the closing card. Nothing to record.

The slides are drawn by `slides.py`. "Publish Now" is the exact label of the bulk command
(`Controllers/AdminController.cs:208`); the narration never says it, because the specification bans the word "now"
(the programme's validator rejects "now" in narration).

## 5. Clips

For every clip: the viewport is 1536x864, and no highlight stays on screen at the end of a step unless the step says
so. Clear highlights before scrolling. Every navigation goes through `ClickAndWaitForNavigationAsync`,
`WaitForNavigationAsync` or `NavigateAsync`. Keep each step's actions shorter than its narration.

Selectors used in several clips:

| Name | Selector (probe) | Source |
|---|---|---|
| Admin menu | `#left-nav` | `TheAdmin/Views/Navigation-admin.cshtml:24` |
| Search box | `input[type='search'][placeholder='Search']` | `Views/ContentsAdminListSearch.cshtml:28` |
| Filters button | `#filter-dropdown` | `Views/ContentsAdminListSearch.cshtml:16` |
| Filter menu entry | `.dropdown-menu[aria-labelledby='filter-dropdown'] a:text-is('<label>')` | `Views/ContentsAdminListSearch.cshtml:19-23` |
| Dialog | `#modalContentOptionsFilterSyntax` | `Views/ContentsAdminListSearch.cshtml:33` |
| Dialog Close button | `#modalContentOptionsFilterSyntax .modal-footer button:text-is('Close')` | `Views/ContentsAdminListSearch.cshtml:107` |
| Content Type picker | `.bootstrap-select.contenttypes-selectpicker > button.dropdown-toggle` | `Views/ContentsAdminListFilters.cshtml:8` |
| Show picker | `.bootstrap-select.contentstatuses-selectpicker > button.dropdown-toggle` | `Views/ContentsAdminListFilters.cshtml:10` |
| Sort picker | `.bootstrap-select.contentsorts-selectpicker > button.dropdown-toggle` | `Views/ContentsAdminListFilters.cshtml:11` |
| Picker option | `.bootstrap-select.<picker class> .dropdown-menu a:has-text('<label>')` | bootstrap-select markup |
| Picker live search | `.bootstrap-select.contenttypes-selectpicker .bs-searchbox input` | live search on, `Views/ContentsAdminListFilters.cshtml:8` |
| Item count | `#items` | `Views/ContentsAdminListSummary.cshtml:7` |
| Selected count | `#selected-items` | `Views/ContentsAdminListSummary.cshtml:8` |
| Select-all box | `#select-all` | `Views/ContentsAdminListSummary.cshtml:5` |
| Row | `li.list-group-item:has-text('<title>')` | `OrchardCore.Admin/Views/AdminList.cshtml` |
| Row checkbox | `li.list-group-item:has-text('<title>') input[name='itemIds']` | `Views/ContentsCheckbox_SummaryAdmin.cshtml:4` |
| Row Actions button | `li.list-group-item:has-text('<title>') button.actions` | `OrchardCore.Admin/Views/AdminListActions.cshtml:31` |
| Bulk Actions button | `#bulk-action-menu-button` | `Views/ContentsAdminList.Fields.BulkActions.cshtml:5` |
| Bulk command | `#actions .dropdown-item[data-action='PublishNow']`, `[data-action='Unpublish']`, `[data-action='Remove']` | `Views/ContentsAdminListBulkActions.cshtml:5`, `Controllers/AdminController.cs:208-210` |
| Confirm dialog, Ok | `#confirmRemoveModal`, `#modalOkButton` | `TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:19,30` |
| Layout buttons | `.admin-list-layout-selector a[title='List']`, `.admin-list-layout-selector a[title='Grid']` | `OrchardCore.Admin/Views/AdminListLayoutSelector.cshtml:11,14` |

### Clip `02-list` (chapter 02, "The content items list")

Starts on: `/Admin` (opened before the capture). About 68 seconds of narration in all.

- `list-1` (about 10 s). Click "Content" in the admin menu (`#left-nav`) to open its section, then click "Content
  Items" (`ClickAndWaitForNavigationAsync`). "Content" has no page of its own, so it is a button that only opens the
  section (`TheAdmin/Views/NavigationItemLink-admin.cshtml`, `isToggleOnlyItem`). The Blog recipe adds its own admin menu
  (the top-level entries "Blog" and "Main Menu", and under "Content" the entries "Article", "Page" and "Taxonomy"); an
  entry with the same caption is merged into one (`OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74-150`), so
  "Content Items" appears once, but select it by its exact text and confirm that on the first probe. The extra entries
  stay on screen and are not narrated. On screen at the end: the address starts with `/Admin/Contents/ContentItems` (the
  recipe's own entry ends with a slash), the page is titled "Manage Content", with the first row "Trail Safety Basics
  Revised". Labels: `AdminMenu.cs:65` ("Content"), `AdminMenu.cs:68`
  ("Content Items"), `Views/Admin/List.cshtml:17` ("Manage Content"). The order, by the date of the last modification:
  `Services/DefaultContentsAdminListFilterProvider.cs:97,306`.
- `list-2` (about 9 s). No click. Move the cursor slowly down the type badges of the first five rows. On screen at the end:
  the list as at the end of `list-1`, with the cursor over a type badge. Source: only listable types, and those the
  role may view, are listed (`Services/DefaultContentsAdminListFilterProvider.cs:182-196`); the label "Listable"
  (`OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:30`).
- `list-3` (about 16 s). Highlight the first row with `HighlightAsync(row)` (its title, its type badge, the two badges
  "Published" and "Draft", the time of the last change and the author); hold, so the ring is on the two badges while the narration explains
  them. On screen at the end: the first row with the ring. Labels: `Views/ContentsTags_SummaryAdmin.cshtml:12` ("Published"), `:19` ("Draft"); the type badge
  `Views/ContentsType_SummaryAdmin.cshtml:6`; the time and the author `Views/ContentsMeta_SummaryAdmin.cshtml:11-19`.
- `list-4` (about 10 s). `ClearHighlightsAsync()`. Highlight the **Edit** button of the first row, then the **View**
  button, then clear the highlights and click the **Actions** button of the same row. On screen at the end: the Actions
  menu open, showing "Publish Draft", "Discard Draft", "Unpublish", "Delete" and "Clone". Do not click a command.
  Labels: `Views/ContentsButtonEdit_SummaryAdmin.cshtml:13` ("Edit"), `:18` ("View");
  `Views/ContentsButtonActions_SummaryAdmin.cshtml:19,30,35,40,45`; `OrchardCore.Admin/Views/AdminListActions.cshtml:32` ("Actions").
- `list-5` (about 10 s). Press Escape to close the menu. Highlight the item count at the top of the list ("10 items / 15
  items in total"), hold, clear the highlight, then `ScrollAsync(0, 900)` to the bottom of the page. On screen at the end:
  the pager below the list. Source: `Views/ContentsAdminListSummary.cshtml:7`.
- `list-6` (about 13 s). `ScrollAsync(0, -1200)` back to the top. Highlight the List and Grid buttons at the end of the
  row that holds the count, clear the highlight, click **Grid** (`ClickAndWaitForNavigationAsync`), hold 3 seconds on the grid with the column
  headings Title, Type, Status, Last modified and Actions, then click **List** (`ClickAndWaitForNavigationAsync`). On
  screen at the end: the List layout again. Labels: `OrchardCore.Admin/Views/AdminListLayoutSelector.cshtml:11,14`
  ("List", "Grid"); the column headings `Views/ContentsAdminList.cshtml:35-39`. The buttons show because **Let users
  choose the layout of a list** is on, which is the default.

### Clip `03-search` (chapter 03, "Search")

Starts on: `/Admin/Contents/ContentItems` (opened before the capture; the search box has the focus). About 56 seconds.

- `search-1` (about 13 s). `TypeAsync(search box, "schedule")`, then press Enter inside `WaitForNavigationAsync`. On
  screen at the end: `?q=schedule` in the address, the search box holding `schedule`, 3 rows. The search is a post
  followed by a redirect (`Controllers/AdminController.cs:316`). The filter is named "Display Text"
  (`Views/Items/ContentsAdminFilters-DisplayText.Thumbnail.cshtml:7`).
- `search-2` (about 11 s). Highlight the item count, then the three rows. On screen at the end: the count "3 items / 3
  items in total" and the rows Autumn Hiking Schedule, Winter Snowshoe Schedule, Summer Hiking Schedule.
- `search-3` (about 12 s). `ClearHighlightsAsync()`. Click the Filters button (`#filter-dropdown`), the funnel icon at the start of the search box, whose
  label "Filters" is only its tooltip: the menu lists "Only
  draft items", "Only published items", "Owned by me" and "Filter syntax". Click "Filter syntax", and wait until the dialog
  is visible (`#modalContentOptionsFilterSyntax` has the class `show`). On screen at the end: the dialog titled
  "Available Filters" with its cards (Display Text, Content Type, Content Type Stereotype, Status, Sort). Labels:
  `Views/ContentsAdminListSearch.cshtml:17` ("Filters"), `:20-23`, `:38` ("Available Filters").
- `search-4` (about 12 s). Scroll the dialog (`ScrollAsync(0, 500)` with the cursor over it) until the card "Filter
  syntax" is fully visible. On screen at the end: that card, with Default, Single and Multiple, and the entries OR, AND,
  NOT, `( ... )` and `" ... "` with their examples. Source: `Views/ContentsAdminListSearch.cshtml:47-86`.
- `search-5` (about 8 s). Click **Close** (`:text-is('Close')` in the dialog footer). Click the search box, select its
  text (`Control+A`), type `schedule NOT winter` (the narration says these words), and press Enter inside `WaitForNavigationAsync`. On screen at the
  end: 2 rows (Autumn Hiking Schedule, Summer Hiking Schedule) and the search box holding `schedule NOT winter`. The
  word without a term name is the default term `text`, so this is the dialog's example `text:foo NOT bar` at
  `Views/ContentsAdminListSearch.cshtml:75` without the term name (`:51-52` says the term name is optional). A default term is written back as typed (`DefaultTermNode.ToString` of YesSql).

### Clip `04-filter` (chapter 04, "Filter and sort")

Starts on: `/Admin/Contents/ContentItems` (opened before the capture; no filter). About 57 seconds. A choice in a picker
reloads the page by itself: the bundled script clicks the hidden Filter button on `changed.bs.select`
(`wwwroot/Scripts/contents-admin-list/contents-admin-list.js`), an event that the vendored picker script dispatches as a
native `CustomEvent` (`OrchardCore.Resources/wwwroot/Vendor/CrestApps/bootstrap-select/bootstrap-select.js:834-841,2939,3229`).
Click the option inside `WaitForNavigationAsync`.

- `filter-1` (about 10 s). Click the **Content Type** picker. The menu opens with the header "Filter by content type", a
  search box and the options "All content types", Article, Blog, Menu, Page, Taxonomy. Type `art` in the picker's search
  box, then click "Article". On screen at the end: 9 rows, all of type Article, and the search box holding
  `type:Article`. Labels: `Views/ContentsAdminListFilters.cshtml:8` ("Content Type", "Filter by content type"),
  `Controllers/AdminController.cs:896` ("All content types").
- `filter-2` (about 10 s). Click the **Show** picker (header "Filter by status"; options Latest, Owned by me, Published,
  Unpublished, All versions), then click "Unpublished". On screen at the end: 5 rows (Trail Safety Basics Revised,
  Autumn Hiking Schedule, Winter Snowshoe Schedule, Placeholder Notice 2, Placeholder Notice 1) and the search box
  holding `type:Article status:Draft` (the terms may come in the other order). Labels:
  `Controllers/AdminController.cs:187-195`, `Views/ContentsAdminListFilters.cshtml:10`. What it lists:
  `Services/DefaultContentsAdminListFilterProvider.cs:30,33` (Unpublished is "the newest version is not published",
  so it includes Trail Safety Basics Revised).
- `filter-3` (about 9 s). Highlight the search box. No navigation. On screen at the end: the search box with its ring
  and the two terms.
- `filter-4` (about 8 s). `ClearHighlightsAsync()`. Click the **Sort** picker (header "Sort by"; options Recently created,
  Recently modified, Recently published, Title), then click "Title". On screen at the end: the same 5 rows in
  alphabetical order (Autumn Hiking Schedule, Placeholder Notice 1, Placeholder Notice 2, Trail Safety Basics Revised,
  Winter Snowshoe Schedule), and the search box also holding `sort:Title`. Labels:
  `Controllers/AdminController.cs:200-203`, `Views/ContentsAdminListFilters.cshtml:11`.
- `filter-5` (about 12 s). Click the Filters button (`#filter-dropdown`), move the cursor over "Only draft items", "Only
  published items" and "Owned by me", then click "Only draft items" (`ClickAndWaitForNavigationAsync`). On screen at the
  end: `?q=status:Draft` in the address, the same 5 rows (in the default order, Trail Safety Basics Revised first), and
  the search box holding only `status:Draft`: the terms `type:Article` and `sort:Title` of the earlier steps are gone,
  because the menu entry is a link that replaces the whole search text, which the narration says. Labels:
  `Views/ContentsAdminListSearch.cshtml:20-22`.
- `filter-6` (about 8 s). Click the type badge "Article" of the first row (the link inside the badge,
  `ClickAndWaitForNavigationAsync`). On screen at the end: the list of Article items only (9 rows). Source:
  `Views/ContentsType_SummaryAdmin.cshtml:6`.

### Clip `05-bulk` (chapter 05, "Act on many items")

Starts on: `/Admin/Contents/ContentItems?q=status:Draft` (opened before the capture): the 5 drafts, the filters visible.
About 64 seconds. **This clip changes the site**: restore the snapshot before every take.

- `bulk-1` (about 16 s). The narration opens with "From the list of drafts", so the page already shows the 5 drafts.
  Tick the box of "Winter Snowshoe Schedule" (row checkbox); pause 1.5 seconds (the narration says that one selected
  item shows no Actions menu; the pickers stay, the **Actions** button stays hidden); tick the box of "Autumn Hiking
  Schedule". On screen at the end: both boxes ticked, the **Actions** button in the bar, the three pickers hidden, and
  "2 selected" in place of the item count. Source: the Actions menu appears when more than one box is ticked
  (`wwwroot/Scripts/contents-admin-list/contents-admin-list.js`, the bundled bulk select list started by
  `Assets/ts/contents-admin-list.ts:1`); the word "selected" `Views/ContentsAdminList.cshtml:12`; "Actions"
  `Views/ContentsAdminList.Fields.BulkActions.cshtml:6`.
- `bulk-2` (about 11 s). Click the **Actions** button (`#bulk-action-menu-button`). On screen at the end: the menu open
  with "Publish Now", "Unpublish" and "Delete". Labels: `Controllers/AdminController.cs:208-210`.
- `bulk-3` (about 6 s). Click "Publish Now" (`[data-action='PublishNow']`), wait until `#confirmRemoveModal` is visible
  (title "Bulk Action", text "Are you sure you want to publish now these items?", buttons "Ok" and "Cancel"),
  `PauseAsync(1500)`, then click **Ok** (`#modalOkButton`) with `ClickAndWaitForNavigationAsync`. On screen at the end:
  the list after the reload. Labels: `Views/ContentsAdminListBulkActions.cshtml:5`; the buttons "Ok" and "Cancel"
  `TheAdmin/Views/Layout.cshtml:99`. If the dialog is still on screen when the narration ends, shorten the pauses.
- `bulk-4` (about 8 s). Wait until the toast "Content published successfully." is visible
  (`text=Content published successfully.`). On screen at the end: the unfiltered list, with "Autumn Hiking Schedule"
  and "Winter Snowshoe Schedule" as rows 2 and 3 (publishing does not change the modification date, so they keep their
  places: `OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:80-92`), showing the Published badge and no Draft badge. The toast hides by itself
  after a few seconds; it only has to show during the first seconds of the step. The reload drops the filter, which the narration says
  (`Controllers/AdminController.cs:404` redirects to the plain list). Label: `Controllers/AdminController.cs:366`.
- `bulk-5` (about 6 s). `TypeAsync(search box, "placeholder", delay 25)`, press Enter inside `WaitForNavigationAsync`. On
  screen at the end: 2 rows (Placeholder Notice 1 and Placeholder Notice 2) and the search box holding `placeholder`.
  The step ends before anything is ticked.
- `bulk-6` (about 9 s). Tick the **select-all** box (`#select-all`), click the **Actions** button, click "Delete"
  (`[data-action='Remove']`). On screen at the end: the dialog "Bulk Action" with "Are you sure you want to delete
  these items?" and the buttons "Ok" and "Cancel". This step packs three actions: keep every pause under 600 ms.
- `bulk-7` (about 8 s). Click **Ok** (`ClickAndWaitForNavigationAsync`) at the start of the step. On screen at the end:
  the unfiltered list without the two placeholders (neither "Placeholder Notice 1" nor "Placeholder Notice 2" appears
  on the page); the toast "Content removed successfully." shows during the first seconds. Label:
  `Controllers/AdminController.cs:398`.

Look at every contact sheet (`review.py`): the frame at the end of each step must show what its narration describes,
with no error message and no empty list.

## 6. Checks for private data

Before assembling, OCR the video at one frame per second and at every scene change, and search the text for real names,
email addresses and tokens. The only email address that may appear is `admin@example.com`, and the user name
`admin` is expected. The fake data of this video holds no email address and no phone number. Stop the site by its process
ID. Keep `state.json`, the snapshot and any mask script outside git.

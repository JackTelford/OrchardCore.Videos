# Recording plan: lists-overview

This plan is for the person who records the video. It needs nothing else: the site, the seed data, the order of the
clips, and for every step the actions, the waits, and what must be on screen when the narration of that step ends.
A selector in this plan stands for `scene.Page.Locator(...)`. The ones that come from a view are followed by the
file and line; check the rest with `Recorder probe` before you rely on them. Source paths are relative to
`src/OrchardCore.Modules/` of the Orchard Core checkout unless they start with `OrchardCore.Themes`.

## The site

- A throwaway local site: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on
  `http://localhost:5010`, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- Provisioned by Auto Setup, so no Setup screen and no sign-in form is recorded (environment variables, see the
  Auto Setup module documentation):
  `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
  `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
  `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog`.
  The password is chosen outside git and passed in `VIDEO_PASSWORD`; never write a real one in a file.
- The recipe is **Blog** (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). It already enables
  `OrchardCore.Lists`, `OrchardCore.Autoroute`, `OrchardCore.Title`, `OrchardCore.ContentTypes`,
  `OrchardCore.AdminMenu` and the Contents feature (lines 31 to 80 of the recipe), so **there is no feature to enable
  before recording**. The recipe also creates the "Blog" link in the admin menu (line 1197), the Blog list
  with one sample post, and the Page type that this video uses as the contained type (Title, Autoroute and Flow
  parts, lines 444 to 498 of the recipe). The video uses Page and not Article on purpose: TheBlogTheme has a template
  for Article (`OrchardCore.Themes/TheBlogTheme/Views/Content-Article.liquid`) that also draws an article inside a
  list on the site, without the link of its title, and it has no template for Page, so a Page in a list is drawn by
  `OrchardCore.Contents/Views/Content.Summary.cshtml` with a link on its title.
- Block outbound calls: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (for example `http://127.0.0.1:9`).
- Sign in once outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`; it writes
  `state.json` (git-ignored, never committed). Light color scheme, `en-US`, the default admin theme.
- Do not touch the admin list layout selector, the admin settings, or any other setting: the Content Items and
  list pages must show their defaults (List layout, buttons beside each row).
- No mask script is needed: the site is made from scratch with fake data only.

## Seed data

Nothing is seeded before the first clip. Everything the video needs is created by the clips themselves, with these
exact invented values:

| What | Value | Created by |
|---|---|---|
| Content type, Display Name | `Help Center` (the Technical Name fills in by itself; it should read `HelpCenter`) | clip `03-type` |
| Parts of the type | Autoroute, List, Title | clip `03-type` |
| Contained Content Types | Page | clip `03-type` |
| List item, Title | `Help Center` | clip `04-items` |
| Page item 1, Title, published | `Reset your password` | clip `04-items` |
| Page item 2, Title, saved as a draft | `Export your data` | clip `04-items` |
| Page item 3, Title, published | `Change your email address` | clip `04-items` |

Leave every other field of the pages empty. The Page editor has a Title, a Permalink and a Flow editor: add no widget
to the Flow editor and leave **Set as homepage** unchecked.

The third page is created last on purpose: the list shows the most recently created item first
(`OrchardCore.Lists/Services/ContainerService.cs:353` and `:392`), so the order on screen is Change your email
address, Export your data, Reset your password.

## Clip order, dependencies and snapshots

Record in this order: `02-blog`, `03-type`, `04-items`, `05-find`, `06-site`.
`01-idea` is a slide clip made by `slides.py`; it needs no recording.

| Clip | Needs | Changes the site |
|---|---|---|
| `02-blog` | the fresh site | no |
| `03-type` | the fresh site | **yes**: creates the Help Center type |
| `04-items` | the site after `03-type` | **yes**: creates the Help Center list and three pages |
| `05-find` | the site after `04-items` | no |
| `06-site` | the same site as `05-find` | no |

Take three copies of `App_Data` (and `state.json`) with the site stopped:

1. **Snapshot A**: after Auto Setup and `Recorder setup`, before any clip.
2. **Snapshot B**: after `03-type`.
3. **Snapshot C**: after `04-items`.

To record a clip again, stop the site, restore the snapshot that precedes it (A for `02-blog` and `03-type`, B for
`04-items`, C for `05-find` and `06-site`), and start the site. A full take starts from A. The clips that follow a
clip you record again must be recorded again too, because they depend on the data it creates.

## Slides

Slide clips are drawn by `slides.py`; nothing is recorded. One line per slide step:

- `idea-1`: the heading "A list is a container item" with four cards; the first two are bright: content that
  belongs together (a blog and its posts, a help center and its pages), and a list as a content item that holds
  others.
- `idea-2`: the same slide with the last two cards bright: the List part, and one list per item.
- `idea-3`: the heading "Where you meet a list" with three cards: the admin page of a list, the site, and the Lists
  feature in Tools, Features.

## Clips

### 02-blog (chapter "The Blog is a list")

Start page: `/Admin` (the admin home page; the Blog recipe does not enable the Admin Dashboard feature), opened with
`scene.OpenAsync("/Admin")` before `StartCaptureAsync`. The admin menu is not expanded further than it is after sign-in.

- `blog-1`: Move to the **Blog** link of the admin menu (a link with the text `Blog`; it is created by the recipe,
  `blog.recipe.json:1197`) and click it with `ClickAndWaitForNavigationAsync`. Wait until the page has the buttons of
  the list (the text `Edit Blog`). On screen when the narration ends: the page of the Blog list.
- `blog-2`: No navigation. Highlight, one after the other and about two seconds each, the **Edit Blog** button and the
  **Create Blog Post** button (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:43` and `:52`), then the first
  row of the list below them. Clear the highlights at the end. The **List Blog Post Items** button is not shown on this
  page (`ListPartNavigationAdmin.cshtml:28`, hidden when the action is Display). On screen: the buttons and the one
  sample post, "Man must explore, and this is exploration at its greatest".
- `blog-3`: No navigation. Highlight the row of that post and keep it for the whole step. On screen: the page of the
  Blog list with the sample post highlighted.

### 03-type (chapter "Create a list type")

Start page: `/Admin` (the admin home page), opened before `StartCaptureAsync`. State: Snapshot A.

- `type-1`: Click **Design** in the admin menu, then **Content Definition**, then **Content Types**
  (`OrchardCore.ContentTypes/AdminMenu.cs:23` to `:25`); the last click loads the Content Types page, so use
  `ClickAndWaitForNavigationAsync`. Then click **Create new type** (`#btnCreate`,
  `OrchardCore.ContentTypes/Views/Admin/List.cshtml:20`) with `ClickAndWaitForNavigationAsync`. On screen when the
  narration ends: the New Content Type page with empty **Display Name** and **Technical Name** fields. If the menu
  clicks take longer than the narration, expand **Design** and **Content Definition** before `StartCaptureAsync` and
  keep only the last click and **Create new type** on camera.
- `type-2`: Type `Help Center` in **Display Name** (`#DisplayName`) with `TypeAsync`. The **Technical Name**
  (`#Name`) fills in by itself (`OrchardCore.ContentTypes/Assets/js/autocomplete.ts`); wait until it is not empty,
  and press Tab. Highlight the Technical Name field for two seconds. Click **Create** (`button.create`,
  `Views/Admin/Create.cshtml:30`) with `ClickAndWaitForNavigationAsync`. On screen: the Add Parts page, with the
  message that the type has been created.
- `type-3`: The parts list is long and alphabetical: scroll (`ScrollAsync`) so each checkbox is visible before you
  click it. Check **Autoroute**, **List** and **Title**, in this order, which is also the order of the page: the parts are sorted by
  display name, so one downward scroll reaches all three (labels from the part names without "Part";
  `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:33`; the hints are "Provides a custom url for your content
  item.", "Add a list behavior." and "Provides a Title for your content item."). A suggested selector for one
  checkbox: `.form-check:has(label:text-is("List")) input[type=checkbox]`. The **Add Selected Parts** button
  (`#add-selected-parts-button`) is hidden until a checkbox is checked. Keep the clicks short: the narration is about
  twelve seconds. On screen: the three boxes checked and the **Add Selected Parts** button shown.
- `type-4`: Click **Add Selected Parts** (`#add-selected-parts-button`) with `ClickAndWaitForNavigationAsync`. On the
  Edit Content Type page, click the **Edit** button on the row of the **List** part (suggested selector:
  `#parts li:has-text("Add a list behavior") a:has-text("Edit")`; the page has a **Parts** heading,
  `Views/Admin/Edit.cshtml:144`, and the button is at `:163`) with `ClickAndWaitForNavigationAsync`. On screen: the
  page "Edit Part - List" with the list settings. Do not click **Add & Configure**: it adds a single part and skips
  the type page.
- `type-5`: No navigation. Highlight in turn, about one and a half seconds each, following the voice, the labels **Page Size**, **Enable Ordering**,
  **Contained Content Types**, **Show container's header** and **Show full pager**
  (`OrchardCore.Lists/Views/ListPartSettings.Edit.cshtml:4`, `:16`, `:23`, `:34`, `:44`). Page Size reads 10 and no
  box is checked. Clear the highlights before any scroll. On screen: the settings page with the last highlight on
  **Show full pager**.
- `type-6`: Scroll back to **Contained Content Types**, and check **Page** (suggested selector:
  `.content-types label:text-is("Page")`; the list is alphabetical and also holds **Select all**,
  `OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml`). Click **Save**
  (`button.save`, `OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:111`) with
  `ClickAndWaitForNavigationAsync`. On screen: the Edit Content Type page for Help Center, with the **List** part
  among the parts and the message that the part settings have been saved.

### 04-items (chapter "Add items to a list")

Start page: `/Admin/Contents/ContentItems` (Content, Content Items), opened before `StartCaptureAsync`; the narration
of `items-1` names the menu path, and the menu is not clicked on camera. State: Snapshot B. The list is sorted by
recently modified, the default.

- `items-1`: Click **New** (`#new-dropdown`, `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:19`) so the
  menu opens, and highlight **Help Center** in it (a `.dropdown-item` with the text `Help Center`). On screen: the
  open New menu with **Help Center** highlighted.
- `items-2`: Click **Help Center** in the menu with `ClickAndWaitForNavigationAsync`. In the **Title** field
  (`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`; suggested selector `input[name="TitlePart.Title"]`) type
  `Help Center` with `TypeAsync`. The **Permalink** field is read-only (the Autoroute settings of the new type allow no
  custom path, `AutoroutePart.Edit.cshtml:37`) and stays empty on this page, with the hint "The url of the content
  item. It will be automatically generated." (`:50`): the address (`help-center`) is built when the item is created
  (`OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs`, `CreatedAsync`, default pattern in `AutoroutePartSettings.cs`).
  Do not wait for it to fill in; highlight the empty field and its hint for two seconds. Click **Publish**
  (`button.publish`, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`) with
  `ClickAndWaitForNavigationAsync`. Wait for the Content Items page. On screen: Content Items with the message "Your
  Help Center has been published." and the Help Center row first.
- `items-3`: Click the **List Page Items** button on the Help Center row
  (`OrchardCore.Lists/Views/ListPartSummaryAdmin.cshtml:10`) with `ClickAndWaitForNavigationAsync`. Wait for the text
  `The list is empty.` On screen: the page of the Help Center list, with the buttons **Edit Help Center** and **Create
  Page**, the search box, and "The list is empty." (`OrchardCore.Lists/Views/ListPartDetailAdmin.cshtml:49`).
- `items-4`: Highlight the message "The list is empty." for two seconds and clear it. Click **Create Page** (in
  `.second-action-bar`, `ListPartNavigationAdmin.cshtml:52`) with `ClickAndWaitForNavigationAsync`. On screen: the
  Page editor, with the bar of links above it (**List Page Items**, **Edit Help Center**, **Create Page**).
- `items-5`: In the **Title** field type `Reset your password` with `TypeAsync`. Click **Publish** with
  `ClickAndWaitForNavigationAsync`; the browser returns to the page of the list, because the **Create Page** link
  carries the address of that page (`ListPartNavigationAdmin.cshtml:51`, `asp-route-returnUrl`). On screen: the
  Help Center list page with one row, "Reset your password", and the message "Your Page has been published."
- `items-6`: Click **Create Page** again. In **Title** type `Export your data`. Click **Save Draft**
  (`button.draft`, `OrchardCore.Contents/Views/Content.SaveDraftButton.cshtml:19`; not the arrow beside it) with
  `ClickAndWaitForNavigationAsync`. On screen: the Help Center list page with two rows, "Export your data" above
  "Reset your password", and the message "Your Page draft has been saved." Keep the actions short: the
  narration is about nine seconds.
- `items-7`: Click **Create Page** again. In **Title** type `Change your email address`. Click **Publish** with
  `ClickAndWaitForNavigationAsync`. On screen: the Help Center list page with three rows, "Change your email address"
  above "Export your data" above "Reset your password", and the message "Your Page has been published." Keep the
  actions short: the narration is about eight seconds.

### 05-find (chapter "Find items in a list")

Start page: the Help Center list page. Before `StartCaptureAsync`, open
`/Admin/Contents/ContentItems/HelpCenter` (use the Technical Name that `type-2` showed), click **List Page Items**
on the one row, and wait for the text `Change your email address`. State: Snapshot C. The page shows three rows:
Change your email address, Export your data, Reset your password.

- `find-1`: No navigation. Highlight the first row's **Edit** button and **View** button (a draft has no **View**
  button, `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13` and `:18`; the "Export your data" row
  shows **Edit** and **Preview Draft** (`:21`), because it has a draft and the admin may preview content, and no
  **View**; say nothing about **Preview Draft** on camera), about three seconds each, then clear the highlights. On screen: the list with
  all three rows.
- `find-2`: Click the funnel icon button beside the search box (`#filter-dropdown`; its title is "Filters" and it has
  no visible text, `OrchardCore.Lists/Views/ListPartDetailAdminSearchPanel.cshtml:21`). The menu shows **Only draft
  items**, **Only published items** and **Owned by me** (`:25` to `:27`); highlight the three entries, then click
  **Only draft items** with `ClickAndWaitForNavigationAsync`. On screen: the list with the single row "Export your
  data".
- `find-3`: Click `#filter-dropdown` again and click **Only published items** with
  `ClickAndWaitForNavigationAsync`. On screen: two rows, "Change your email address" and "Reset your password".
- `find-4`: Click the search box (placeholder **Search**, `input[name="DisplayText"]`), type `password` with
  `TypeAsync`, and press Enter with `WaitForNavigationAsync`. On screen: one row, "Reset your password", with the
  search box showing `password`.

### 06-site (chapter "See the list on the site")

Start page: `/Admin/Contents/ContentItems/HelpCenter`, opened before `StartCaptureAsync` (the Content Items page
filtered to the Help Center type, one row). State: Snapshot C.

- `site-1`: Click **View** on the Help Center row. It opens the site in a new tab (`target="_blank"`,
  `ContentsButtonEdit_SummaryAdmin.cshtml:18`): wait for the popup, then `SwitchToAsync(popup)`. Wait until the page
  shows the title "Help Center". On screen: the public page of the Help Center list.
- `site-2`: No navigation. Highlight the list of pages for the whole step: two titles, "Change your email address"
  then "Reset your password", each a link (`OrchardCore.Lists/Views/ListPart.cshtml` draws each published item with its
  Summary display; the title is the link of `OrchardCore.Title/Views/TitlePart.Summary.cshtml:3`). "Export your data" is
  not shown, because the public list queries published items only (`ContainedItemOptions.cs:14`). Do not scroll while a
  highlight is shown.
- `site-3`: Click the title "Reset your password" with `ClickAndWaitForNavigationAsync`. On screen: the page of that
  item (address `/reset-your-password`), with its title in the header of the page (`TheBlogTheme/Views/Content.liquid`).

## Checks for private data

- Nothing real is typed on camera: the only values are the invented titles in the table above. The password is never
  typed on camera (the session comes from `state.json`).
- The user is `admin` with `admin@example.com`; if the user menu or a page shows another name or email, stop and
  fix the site.
- The sample post of the Blog recipe ("Man must explore, and this is exploration at its greatest") shows in
  `02-blog`; it is sample text from the recipe, not a real person's data. No other recipe content is opened.
- After the last take, run the OCR scan of the skill (one frame per second and at every scene change) on the
  video, and search for real names, email addresses other than `example.com`, phone numbers and tokens. The site
  address is `localhost`; the Permalink and the address of the site pages show only invented titles.
- Do not commit `state.json`, any `App_Data` copy, or an MP4 or audio file.

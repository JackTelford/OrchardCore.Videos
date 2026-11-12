# Recording plan: admin-settings

Everything a person needs to record this video without deciding anything. A selector written here stands for
`scene.Page.Locator(...)` (or `GetByLabel` / `GetByRole`, as named). Source paths are relative to the root of the
`OrchardCMS/OrchardCore` checkout at commit `92db4a83` (the catalog's commit); record on a build of that commit or a
later one, and compare the screen with the "Labels from" lines if anything differs.

## The site it needs

| | |
|---|---|
| Host | `src/OrchardCore.Cms.Web`, on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories. |
| Setup | Auto Setup (see the `orchardcore-tester` skill, `references/autosetup.md`): tenant `Default`, site name **Orchard Core**, time zone `America/Los_Angeles`, recipe **Blog**, SQLite, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, the same value in `VIDEO_PASSWORD`). No setup screen and no sign-in form are recorded. |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The video needs no external service. |
| Features | None to enable. The Blog recipe enables `OrchardCore.Admin`, `TheAdmin` (as the admin theme), `OrchardCore.Contents`, `OrchardCore.ContentTypes`, `OrchardCore.Roles`, `OrchardCore.Themes`, `OrchardCore.Taxonomies` and the others the video touches. **Leave `OrchardCore.Contents.QuickNavigation` and `OrchardCore.AdminDashboard` disabled**: the first (listed as Content Quick Navigation, `Contents/Manifest.cs:23-24`) adds a field to the Settings, Admin page (`src/OrchardCore.Modules/OrchardCore.Contents/QuickNavigation/ContentQuickNavigationSettingsDisplayDriver.cs:24,38`) and the second changes the page at `/Admin`. The Blog recipe enables neither (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-80`). |
| The recipe's own admin menu | The Blog recipe also runs an `AdminMenu` step (`blog.recipe.json:1188-1307`). It adds the top-level entries **Blog** and **Main Menu**, and a **Content** group that is merged by caption (`NavigationManager.cs:74-152`) with the module's **Content** group: besides **Content Items** it holds the entries **Article**, **Page** and **Taxonomy**. So **Article**, **Page** and **Taxonomy** appear twice once **Display New menu** is on, under **Content** and under **New**: scope every locator for these entries to one group, as the steps below do. |
| Sign-in | `Recorder setup` once, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, saves `state.json` (git-ignored). Every clip opens in a new browser context built from `state.json`, so the browser starts each clip with no theme choice, no remembered menu sections and no list layout cookie. If a take is made in a persistent browser instead, clear the site data of `localhost:5010` before each clip. |
| Viewport | The recorder's default (1536x864, light color scheme). |

### Seed data (create it outside the capture, before the snapshot)

The Content Items list of a fresh Blog site holds only a few rows. Open **Content, Content Items**, use its **New**
drop-down (the **New** entry of the admin menu does not exist yet: **Display New menu** is off at S0), pick the type,
type the title, and click **Publish** or **Save Draft** (`Contents/Views/Content.PublishButton.cshtml:7`,
`Content.SaveDraftButton.cshtml:7`, `ContentsAdminListCreate.cshtml:20`), so the Grid layout has something to show.
All names are invented. Leave every body empty, or type the words "Sample text".

| Type | Title | Button to click |
|---|---|---|
| Article | Opening Day at Maple Street Bakery | **Publish** |
| Article | Spring Menu Preview | **Save Draft** only (do not click **Publish**) |
| Article | Meet the Bakers | **Publish** |
| Article | Holiday Opening Hours | **Publish** |
| Page | Delivery Areas | **Publish** |
| Page | Contact Us | **Publish** |

Then open **Content, Content Items** once and check that the list shows these rows together with the rows the recipe
creates (Blog, About, which is an Article, Main Menu, and the Tags and Categories taxonomies; the blog post and the
footer widget of the recipe are not listed because their types are not listable), with one **Draft** in the **Status**
column (`ContentsTags_SummaryAdmin.cshtml:12,19`).

### Snapshot

After setup and the seed, stop the site and copy `App_Data` and `state.json` aside. This is snapshot **S0**. The site
then holds the default admin settings (below). Restore S0 before every full take, and start the site again.

## Admin settings at the start of each clip

Default values of a site that never saved the page (`src/OrchardCore/OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:7-49`;
no recipe of the Blog setup writes the admin settings).
A tick is a checked box.

| Option | S0 and `02-open` | start of `03-menu` | start of `04-topbar` | start of `05-lists` | after `05-lists` |
|---|---|---|---|---|---|
| Enable theme toggler | ✔ | ✔ | ✔ | ✔ | ✔ |
| Enable Admin Menu filter | | | ✔ | ✔ | ✔ |
| Enable quick navigation | ✔ | ✔ | ✔ | ✔ | ✔ |
| Display New menu | | | ✔ | ✔ | ✔ |
| Display titles in top bar | | | | ✔ | ✔ |
| Show breadcrumb | ✔ | ✔ | ✔ | | |
| Navigation menu behavior | Focused | Focused | Persistent | Persistent | Persistent |
| List layout | Use configuration default | Use configuration default | Use configuration default | Use configuration default | Grid |
| Actions layout | Use configuration default | Use configuration default | Use configuration default | Use configuration default | Menu |
| Let users choose the layout of a list | ✔ | ✔ | ✔ | ✔ | ✔ |

(`02-open` saves the page without changing a value, so `03-menu` starts from S0.)

## Order of the clips

Record in the order they play: `02-open`, `03-menu`, `04-topbar`, `05-lists`. Each clip starts from the state the one
before it leaves (the table above). **Every clip changes the site except `02-open`**: `03-menu`, `04-topbar` and
`05-lists` each save settings. To record one clip again, restore S0, then open **Settings, Admin** outside the capture
and set the options to the clip's start state (the table), click **Save**, and start the take. To record everything
again, restore S0 and run the four clips in order.

`01-areas` (a slide) and the cards need no recording.

## Chapter 00, the opening card

`intro` (card). Nothing to record.

## Chapter 01, `01-areas` (slide)

- `areas-1`: the slide "One page, three areas" with three cards: **The admin menu**, **The top bar and page headings**
  and **Lists and actions**, all bright. Drawn by `slides.py`.

## Chapter 02, clip `02-open`

Starts on: `/Admin` (the admin home), opened before the capture with `scene.OpenAsync("/Admin")`. The menu is fully
collapsed. Sign in state from `state.json`.

A group of the admin menu that has no page of its own toggles when its label is clicked
(`src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24,29,35,76-88`; nothing in the source makes a group
link to its first child, `LinkToFirstChild` is read by no view): the groups **Settings**, **Content**, **Design** and **New**
are buttons, found with `#left-nav` and `GetByRole(AriaRole.Button, new() { Name = "<label>", Exact = true })`. A group
button toggles: click it only when its `aria-expanded` is `false` if the step needs the group open.

- `open-1` (narration about 8 s)
  - Actions: click the **Settings** group button in `#left-nav`; wait until `#left-nav a[href$="/Admin/Settings/admin"]`
    is visible; `ClickAndWaitForNavigationAsync` on that link (its label is **Admin**).
  - Wait: the heading **Settings** is visible and the form with the option labels is on screen.
  - On screen when the narration ends: the page at `/Admin/Settings/admin`, **Admin** highlighted under **Settings** in
    the menu, the first options of the form visible.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Admin/AdminMenu.cs:26-27` (**Settings**, **Admin**),
    `src/OrchardCore.Modules/OrchardCore.Settings/Views/Admin/Index.cshtml:4` (the heading **Settings**).
- `open-2` (narration about 10 s)
  - Actions: `HighlightAsync` on the row of **Enable theme toggler** (its label and the hint under it:
    `scene.Page.Locator("div.ocat-wrapper", new() { HasText = "Enable theme toggler" })`), pause 1.5 s,
    `ClearHighlightsAsync`; then click the **Save** button (`GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true })`,
    in the bar at the bottom) with `ClickAndWaitForNavigationAsync`.
  - Wait: the text **Site settings updated successfully.** is visible.
  - On screen when the narration ends: the same page, reloaded, with the notification **Site settings updated
    successfully.** and the options unchanged.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminSettings.Edit.cshtml:8,10` (label and hint of
    the first option), `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (**Save**),
    `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119` (the notification).

## Chapter 03, clip `03-menu`

Starts on: `/Admin/Settings/admin`, opened before the capture. Options as in the start table. Changes the site:
**Enable Admin Menu filter** and **Display New menu** on, **Navigation menu behavior** to Persistent.

- `menu-1` (narration about 9 s)
  - Actions: click the checkbox `GetByLabel("Enable Admin Menu filter")`, `HighlightAsync` it for 1 s; click
    `GetByLabel("Display New menu")`; `ClearHighlightsAsync`; click **Save** (`ClickAndWaitForNavigationAsync`).
  - Wait: **Site settings updated successfully.** is visible and `#filter` is visible.
  - On screen when the narration ends: the page reloaded, the **Filter** box above the admin menu, **New** as the first
    entry of the menu (above the recipe's **Blog** and **Main Menu** and above **Content**), both checkboxes checked.
  - Labels from: `AdminSettings.Edit.cshtml:18` and `:38` (the two labels), `src/OrchardCore.Themes/TheAdmin/Views/Navigation-admin.cshtml:21`
    (the placeholder **Filter**), `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:88` (**New**, position `-1`). Why it
    is first: an item without a position gets `""` (`OrchardCore.DisplayManagement/Shapes/Shape.cs:60`), which the
    comparer reads as `0` (`Zones/FlatPositionComparer.cs:160-162`), and **Content** has position `1`
    (`NavigationConstants.cs:7`); no navigation provider puts a top-level item before `-1`.
- `menu-2` (narration about 12 s)
  - Actions: click `#filter`; `TypeAsync` the text `Roles` into it.
  - Wait: `#left-nav` shows an item whose text is **Roles** (`#left-nav a` with that text visible).
  - On screen when the narration ends: only the group **Access Control** (open) with **Roles** under it, and the
    other items hidden; **Roles** is in the box.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19` (**Access Control**, **Roles**).
- `menu-3` (narration about 16 s)
  - Actions: `scene.Page.Keyboard.PressAsync("Escape")` while the box has focus (it empties the box and restores the menu),
    then click the **New** group button.
  - Wait: the **Article** entry inside the **New** group is visible. Scope it to the group, because **Article** also
    appears under **Content** (see "The recipe's own admin menu"):
    `scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = "New", Exact = true }).Locator("xpath=ancestor::li[1]").GetByRole(AriaRole.Link, new() { Name = "Article", Exact = true })`.
  - On screen when the narration ends: the full menu with **New** open as its first entry and its items listed. The items
    are **Article**, **Blog**, **Page** and **Taxonomy** (the types of the Blog recipe and of the Taxonomies feature that
    are creatable: `blog.recipe.json:136,248,448`, `Taxonomies/Migrations.cs:26`); the narration names only Article and Page.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:84-98` (the items are the creatable
    content types, ordered by display name; an item is shown to a user who may create that type, line 102).
- `menu-4` (narration about 12 s)
  - Actions: `MoveToAsync` the radio `GetByLabel("Focused", new() { Exact = true })` (this scrolls the radios into view),
    `HighlightAsync` the group of radios (`[id$="MenuBehavior"]`) for 2 s, `ClearHighlightsAsync`; click the **Content**
    group button in the menu, pause 1 s, then click the **Design** group button (both groups are closed before the
    click, so each click opens).
  - Wait: after the second click the **Content** group is closed (its items are not visible) and **Design** is open.
  - On screen when the narration ends: **Focused** selected in the radio group, and in the menu only **Design** open
    (**Content** and **New** closed). Look at the frame: **Content** must be closed. Whether the **Settings** group,
    which holds the current page, closes when another group opens is not settled by the source (see
    `OPEN-QUESTIONS.md`); either is acceptable for the take.
  - Labels from: `AdminSettings.Edit.cshtml:69,73,74`.
- `menu-5` (narration about 12 s)
  - Actions: click the radio label `GetByLabel("Persistent", new() { Exact = true })`; click **Save**
    (`ClickAndWaitForNavigationAsync`); wait for the notification; `MoveToAsync` the radio
    `GetByLabel("Persistent", new() { Exact = true })` again (the reload scrolled the page to the top); click the **Content**
    group button (closed after the reload: only **Settings**, the group of the current page, is open), pause 0.8 s,
    click the **Design** group button.
  - Wait: both `#left-nav` groups show their items.
  - On screen when the narration ends: **Persistent** selected, and **Settings** (the current page), **Content** and
    **Design** open at once in the menu.
  - Labels from: `AdminSettings.Edit.cshtml:78,79,81`.

## Chapter 04, clip `04-topbar`

Starts on: `/Admin/Settings/admin`, opened before the capture, with the options of the `04-topbar` column. The page
shows a short trail above the heading (one entry, **Settings**). Changes the site: **Display titles in top bar** on,
**Show breadcrumb** off.

- `top-1` (narration about 12 s)
  - Actions: `HighlightAsync` on `#adminQuickNavigationToggle` (the magnifier in the top bar) for 1.5 s,
    `ClearHighlightsAsync`; press `Control+K` with `scene.Page.Keyboard.PressAsync` (if the palette does not open,
    click `#adminQuickNavigationToggle` instead); wait until `#adminQuickNavigationInput` is visible; `TypeAsync`
    the text `roles` into it.
  - Wait: `#adminQuickNavigationResults li` holds a result whose title is **Roles**.
  - On screen when the narration ends: the palette open over the page, the text `roles` in its box, the result
    **Roles** with its parent **Access Control** under it. The palette stays open until the next step.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Admin/Views/QuickNavigationNavbarItem.cshtml:13,23`
    (tooltip **Quick navigation (Ctrl+K)**, placeholder **Go to an admin page...**), `AdminSettings.Edit.cshtml:28`.
- `top-2` (narration about 9 s)
  - Actions: press `Escape` (the palette closes; wait until `#adminQuickNavigationModal` is hidden); click `#bd-theme`
    (**Toggle theme**); hover **Auto**, **Light**, **Dark** one after the other (1 s each). Do not pick **Dark**.
  - Wait: the three items are visible in the drop-down list.
  - On screen when the narration ends: the **Toggle theme** drop-down open with **Auto**, **Light**, **Dark**.
  - Labels from: `src/OrchardCore.Themes/TheAdmin/Views/ToggleTheme.cshtml:3-5,13,21,29`,
    `AdminSettings.Edit.cshtml:8`. **Toggle theme** is the accessible name of the button (`aria-label`, line 3); the screen
    shows a half-filled circle icon, and only **Auto**, **Light** and **Dark** are drawn as text.
- `top-3` (narration about 8 s)
  - Actions: press `Escape` (the drop-down closes); `MoveToAsync` `GetByLabel("Display titles in top bar")`; click it;
    `HighlightAsync` it for 1.5 s; `ClearHighlightsAsync`.
  - Wait: the checkbox is checked.
  - On screen when the narration ends: **Display titles in top bar** checked and not saved; the trail and the heading
    **Settings** still in the page.
  - Labels from: `AdminSettings.Edit.cshtml:48`.
- `top-4` (narration about 12 s)
  - Actions: `MoveToAsync` `GetByLabel("Show breadcrumb")`; click it (it becomes unchecked); click **Save**
    (`ClickAndWaitForNavigationAsync`).
  - Wait: **Site settings updated successfully.** is visible and `.brand-wrapper-title` contains **Settings**.
  - On screen when the narration ends: the page reloaded, the title **Settings** in the top bar next to the site name
    **Orchard Core**, no trail above the form, **Show breadcrumb** unchecked, **Display titles in top bar** checked.
  - Labels from: `AdminSettings.Edit.cshtml:58`, `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:61-63,89-91`.

## Chapter 05, clip `05-lists`

Starts on: `/Admin/Settings/admin`, opened before the capture, with the options of the `05-lists` column. This clip
must not click the **List** or **Grid** buttons of a list before `lists-4`: a click stores a layout in a cookie, and
that cookie wins over the **List layout** option (the site layout is only the starting point). Changes the site:
**List layout** to Grid and **Actions layout** to Menu.

- `lists-1` (narration about 8 s)
  - Actions: `MoveToAsync` `GetByLabel("List layout", new() { Exact = true })`; `SelectAsync` it with `Grid`;
    `HighlightAsync` the select (not its hint) for 1 s; `ClearHighlightsAsync`.
  - Wait: the select shows **Grid**.
  - On screen when the narration ends: the **List layout** select showing **Grid**, not saved. (A headless browser
    draws no open list; the narration names the three choices.)
  - Labels from: `AdminSettings.Edit.cshtml:86,89,90,91`. The hints under **List layout** and **Actions layout** (lines
    94 and 107) name a configuration file and a configuration key. They are part of the page and stay on screen, but the
    narration never reads them: keep the cursor and the highlight on the select, and do not zoom on the hint (see
    `OPEN-QUESTIONS.md`).
- `lists-2` (narration about 16 s)
  - Actions: `SelectAsync` `GetByLabel("Actions layout", new() { Exact = true })` with `Menu`; click **Save**
    (`ClickAndWaitForNavigationAsync`).
  - Wait: **Site settings updated successfully.** is visible.
  - On screen when the narration ends: the page reloaded, **List layout** showing **Grid**, **Actions layout**
    showing **Menu**, and the notification.
  - Labels from: `AdminSettings.Edit.cshtml:99,102,103,104`. The sentence that the option also covers the secondary actions
    of the bottom action bar of edit pages comes from the hint (line 107), `TheAdmin/Views/Layout.cshtml:12,48` (the body
    attribute `data-actions-layout`) and `TheAdmin/Assets/js/TheAdmin/actionBar.ts:15`; no edit page is shown.
- `lists-3` (narration about 12 s)
  - Actions: click the **Content** group button in the menu (it is closed after the reload of `lists-2`: its
    `aria-expanded` is `false`; click only then, a second click closes it); click the link **Content Items**
    (`scene.Page.Locator("#left-nav").GetByRole(AriaRole.Link, new() { Name = "Content Items", Exact = true })`,
    `ClickAndWaitForNavigationAsync`; do not select it by its address, because the recipe's own admin menu entry of the same
    name is merged with the module's, and either one may end up with the address, with or without a final slash); wait
    for the column header **Last modified**; click the ellipsis button of the first row
    (`scene.Page.Locator("div.admin-list-grid-row").First.GetByRole(AriaRole.Button, new() { Name = "Actions", Exact = true })`,
    class `actions`).
  - Wait: the menu of that row (`.actions-menu.show`) is visible.
  - On screen when the narration ends: the page **Manage Content** (its title in the top bar) in the Grid layout,
    with the column headers **Title**, **Type**, **Status**, **Last modified**, and the menu of the first row open.
    The seed rows are listed, one of them with the status **Draft**.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65,68` (**Content**, **Content Items**),
    `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsAdminList.cshtml:34-39` (the column titles),
    `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions-Menu.cshtml:23-24` (the ellipsis button),
    `src/OrchardCore.Modules/OrchardCore.Contents/Views/Admin/List.cshtml:17` (**Manage Content**).
- `lists-4` (narration about 15 s)
  - Actions: press `Escape` (the row menu closes); `HighlightAsync` on `.admin-list-layout-selector` (the **List** and
    **Grid** buttons at the top of the list) for 2 s; `ClearHighlightsAsync`; click its **List** button
    (`.admin-list-layout-selector a[title="List"]`, a link drawn as a button, `ClickAndWaitForNavigationAsync`; the address
    gets `?layout=List`).
  - Wait: the rows are drawn in the List layout (each row is a block with its title, and the ellipsis button at its
    end).
  - On screen when the narration ends: the Content Items list in the List layout, with the **List** button of the
    selector active and the **Grid** button beside it.
  - Labels from: `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListLayoutSelector.cshtml:10-15`,
    `AdminSettings.Edit.cshtml:115`.

## Chapter 99, the closing card

`outro` (card). Nothing to record.

## Checks for private data

The site is made from scratch with the Blog recipe, so there is nothing real to mask. Still:

- Every name on screen is the recipe's or the seed's (Blog, About, Main Menu, Tags, Categories, the six seed titles). The only
  user is `admin`; its email `admin@example.com` is never on a recorded page.
- Look at each contact sheet (`review.py`): no error page, no 403 or 404, no empty list in `lists-3` and `lists-4`,
  and the browser's address bar is not part of the frame.
- OCR the final video at one frame per second and at every scene change, and search for anything that is not from the
  list above (a real name, an email other than `admin@example.com`, a token, a local path).
- The password is never typed on camera (the recorder signs in from `state.json`); the string `<password>` stands for
  a value that is not in git.

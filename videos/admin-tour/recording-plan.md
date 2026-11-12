# Recording plan: The Orchard Core admin

Everything a person needs to record this video without deciding anything. Paths such as `src/OrchardCore.Modules/...`
are in the `OrchardCMS/OrchardCore` source (commit `92db4a83`). A selector written below stands for
`scene.Page.Locator(...)`. The narration is in `storyboard.json`; narrate it first (`scripts/narrate.py`), because the
recorder holds each step for the length of its narration.

The video has eight chapters between its cards: two are slide chapters (`01-glance`, `08-recap`, nothing to record) and
six are recordings (`02-start`, `03-menu`, `04-pages`, `05-quick`, `06-content`, `07-topbar`).

## The site it needs

A fresh site, provisioned by Auto Setup, with the **Blog** recipe. The Blog recipe is the site because it gives the
menu its sections (Content, Design, Media, Search, Access Control, Tools, Settings), its own custom admin menu (the
Blog and Main Menu links), and content items to open and find (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`).

| Setting | Value |
|---|---|
| Recipe | `Blog` |
| Tenant | `Default` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Admin user | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Database | `Sqlite`, in its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`) |
| Address | `http://localhost:5010` (`VIDEO_BASE_URL`) |

Auto Setup is given through environment variables, so no setup or sign-in form is recorded
(`OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
`...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
`...__AdminPassword=$VIDEO_PASSWORD`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog`). Set `HTTP_PROXY` and
`HTTPS_PROXY` to a dead address, so the site calls no outside service. See `references/recording.md` of the skill.

**Features.** Enable nothing before recording. The Blog recipe enables everything the video shows (Admin, Navigation,
AdminMenu, Contents, Features, Themes, Users, Roles, Media, Queries, Indexing, `TheAdmin`). These must be **disabled**
at the start, and stay so unless a step below enables them:

- Content Quick Navigation (`OrchardCore.Contents.QuickNavigation`): clip `06-content` enables it on camera, and clip
  `05-quick` needs it disabled.
- Notifications, Admin Dashboard, Admin Culture Picker (the Localization features): the top bar and the start page
  must show only what the Blog recipe gives them.

**Settings.** Leave **Settings, Admin** at its defaults: Enable theme toggler on, Enable quick navigation on, Enable
Admin Menu filter off, Display New menu off, Display titles in top bar off, Show breadcrumb on, Navigation menu
behavior Focused. Nothing in this video saves a setting.

**Seed data.** None to create. The recipe's own content is the data: the `About` article (type Article), `Blog`, the
blog post `Man must explore, and this is exploration at its greatest`, `Main Menu`, `Tags`, `Categories` and the
`Footer` widget. They hold sample text only (no person, email or company).

**Browser.** Chromium at 1536x864, light color scheme, `en-US`, the menu expanded, and no theme chosen yet (nothing is
stored, so the admin follows the device: it renders light with the light color scheme, and the open toggle then marks
Light as the active entry, not Auto, because the script stores nothing until an entry is clicked:
`src/OrchardCore.Modules/OrchardCore.Themes/Assets/ts/constants.ts:getPreferredTheme`). Sign in once outside the
capture (`Recorder setup`), which saves `state.json` (git-ignored). Every clip starts from that file, so a choice made
in the browser during a clip (the theme is kept in the browser's local storage, the collapsed menu in the admin
preferences cookie: `sidebar.ts`) does not reach the next clip, but each clip below still puts it back.

## Order of the clips, and what changes the site

Record in this order. Take a snapshot of `App_Data` and of `state.json` after `Recorder setup` ("snapshot A").

| Order | Clip | Starts on | Changes the site? | Depends on |
|---|---|---|---|---|
| 1 | `02-start` | `/Admin` | No | Snapshot A |
| 2 | `03-menu` | `/Admin` | No | Snapshot A |
| 3 | `04-pages` | `/Admin/Contents/ContentItems` | No | The `About` article exists |
| 4 | `05-quick` | `/Admin/Contents/ContentItems` | No | Content Quick Navigation **disabled** |
| 5 | `06-content` | `/Admin/Features` | **Yes**: enables Content Quick Navigation | Snapshot A |
| 6 | `07-topbar` | `/Admin` | No | None |

- To record the whole video again, stop the site, restore snapshot A, start the site, and record clips 1 to 6.
- To record clip `05-quick` or `06-content` again after `06-content` was taken, restore snapshot A first: the palette
  of `05-quick` must list menu pages only, and `06-content` must find the feature disabled.
- Clips `02-start`, `03-menu`, `04-pages` and `07-topbar` do not change the site: record them again at any time.
- Only `06-content` changes the site, and only by enabling a feature. The palette of the clips recorded after it also
  lists content items; nothing else shows a difference.

Before every clip, check that the menu is expanded and that no section of the menu is open on `/Admin`.

## Slide steps (nothing to record)

- `intro`: the opening card, the logo over "The Orchard Core admin" and "Find your way around".
- `glance-1`: slide "Every admin page has three parts", three cards: Top bar, Admin menu, Page.
- `glance-2`: slide "Its own theme, and one permission", two cards with check marks: Its own theme, One permission
  opens it.
- `recap-1`: slide "Four ways to find your way", four cards: Admin menu, Quick navigation, Breadcrumb, Logo and site
  name.
- `outro`: the closing card, "The Orchard Core admin" and "docs.orchardcore.net".

## Clip `02-start`

Starts on `/Admin` (opened before the capture). Wait until the heading "Welcome to Orchard Core" and the four cards
are shown. Every section of the menu is closed.

- `start-1`: highlight `.alert.alert-primary` for 1.5 seconds, clear it. Then, one after the other, move to and
  highlight the cards `.card:has(h5:text-is("Create content"))`, `.card:has(h5:text-is("Customize theme"))` and
  `.card:has(h5:text-is("Manage users"))` for about 2 seconds each, clearing each ring before the next. Click nothing.
  *Visible at the end:* the start page, the banner "Welcome to Orchard Core" and the four cards, with a ring on the
  "Manage users" card.
  *Labels:* `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminDashboard.cshtml:2-3`;
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/AdminDashboardContents.cshtml:4`;
  `src/OrchardCore.Modules/OrchardCore.Themes/Views/AdminDashboardThemes.cshtml:4`;
  `src/OrchardCore.Modules/OrchardCore.Users/Views/AdminDashboardUsers.cshtml:4`.
- `start-2`: move to `.card:has(h5:text-is("Learn more"))` and highlight it. Move to its "Documentation" button
  (`.card:has(h5:text-is("Learn more")) a.btn`) without clicking it: it opens the documentation site in a separate tab.
  *Visible at the end:* the ring on the "Learn more" card, the cursor on its Documentation button, no other tab.
  *Label:* `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminDashboardDocumentation.cshtml:4-6`
  (`target="_blank"` on the button).
- `start-3`: clear the ring, then move to `a.ta-navbar-brand` (the logo and site name at the top left) and highlight it.
  Do not click it. *Visible at the end:* a ring on the logo and the site name "Orchard Core" in the top bar, the
  "Learn more" card below.
  *Label:* `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminBranding.cshtml:5-8` (the link goes to `~/Admin`).

## Clip `03-menu`

Starts on `/Admin` (opened before the capture), the menu expanded, every section closed. In this clip, a menu entry is
`#left-nav .item-label:has(span.title:text-is("<label>"))`; use `.First` when two entries share a label. Sections have
no page of their own, so clicking their label opens or closes them without leaving the page.

- `menu-1`: highlight `#left-nav` with a ring. Then move the cursor down the menu, over the labels Content, Design,
  Access Control, Tools and Settings, about 0.6 seconds each. *Visible at the end:* the whole menu with every section
  closed, a ring around it, the cursor on Settings. *Labels:*
  `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65` (Content),
  `src/OrchardCore.Modules/OrchardCore.Themes/AdminMenu.cs:18` (Design),
  `src/OrchardCore.Modules/OrchardCore.Users/AdminMenu.cs:27` (Access Control),
  `src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:32` (Tools) and `:36` (Settings).
- `menu-2`: clear the ring. Click the Content label, wait 0.8 seconds, then click the Design label. *Visible at the
  end:* Design open (Themes and the other design pages listed), Content closed again.
- `menu-3`: click the Content label (Design closes), then `ClickAndWaitForNavigationAsync` on the Content Items entry
  (`.First`). *Visible at the end:* the "Manage Content" page, the Content section open in the menu with Content Items
  marked as the current page. *Label:* `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:68`.
- `menu-4`: no click. Move the cursor slowly down the open menu, from the first entry to Settings. *Visible at the
  end:* the same page, the menu under the cursor.
- `menu-5`: highlight the entries Blog and Main Menu (`#left-nav .item-label:has(span.title:text-is("Blog"))` and
  `...("Main Menu")`), 1.5 seconds each, clearing each ring. Click the Tools label, then highlight the Admin Menus entry.
  *Visible at the end:* Tools open with a ring on Admin Menus; the Blog and Main Menu entries visible in the menu.
  *Labels:* `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:1197` (Blog) and `:1217` (Main Menu), the
  AdminMenu step of the recipe; `src/OrchardCore.Modules/OrchardCore.AdminMenu/AdminMenu.cs:25-26` (Tools, Admin Menus).
  *Where they sit (from the code):* both recipe nodes have no position, which the menu sorts as "before"
  (`FlatPositionComparer.cs`, null becomes `before.`), so Blog and Main Menu should be the first two entries of the
  menu, above Content. *To check on the running site:* the rest of `OPEN-QUESTIONS.md`.
- `menu-6`: click `.leftbar-compactor` (the button at the bottom of the menu): the menu narrows to icons. Move to the
  icon of the Design section (`#left-nav > ul > li > figure > figcaption > .item-label:has(span.title:text-is("Design")) > .icon`;
  a looser selector such as `li:has(...) .icon` also matches the icon of every page in the section and fails in
  strict mode), wait until its flyout (the section name, then its pages) is shown, hold 2 seconds, move away. Click
  `.leftbar-compactor` again. *Visible at the end:*
  the menu **expanded** again, with its labels, as at the start of the clip.
  *Label:* `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:79` (title "Collapse / expand menu");
  behavior in `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts` (compact flyouts).

## Clip `04-pages`

Starts on `/Admin/Contents/ContentItems` (opened before the capture); the list shows the content items, among them
`About`.

- `pages-1`: move the cursor slowly down over the list of content items for about 4 seconds (the narration names the
  content items first). Then highlight the menu entry Content Items
  (`#left-nav .item-label:has(span.title:text-is("Content Items"))`, `.First`) for 1.5 seconds, clear the ring, and
  highlight `h1.oc-breadcrumb-title` (the page title "Manage Content"), holding it until the step ends. *Visible at
  the end:* the title "Manage Content" with a ring, the list of content items below it, the Content Items entry marked
  in the menu. *Labels:* `src/OrchardCore.Modules/OrchardCore.Contents/Views/Admin/List.cshtml:17` (title),
  `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:68` (Content Items).
- `pages-2`: clear the ring. `ClickAndWaitForNavigationAsync` on the link `#items-form a:text-is("About")` (`.First`).
  Wait for the edit page, then highlight `nav[aria-label="Breadcrumb"]`. *Visible at the end:* the page "Edit Article",
  its breadcrumb "Manage Content", "Article", "Edit Article" with a ring.
  *Labels:* `src/OrchardCore.Modules/OrchardCore.Contents/Views/Admin/Edit.cshtml:39-47`.
- `pages-3`: clear the ring. Move to `nav[aria-label="Breadcrumb"] a:text-is("Manage Content")`, then
  `ClickAndWaitForNavigationAsync` on it. *Visible at the end:* the "Manage Content" list again.
  *Label:* `src/OrchardCore.Modules/OrchardCore.Contents/Views/Admin/Edit.cshtml:40`.

## Clip `05-quick`

Starts on `/Admin/Contents/ContentItems`. Content Quick Navigation must be disabled.

- `quick-1`: highlight `#adminQuickNavigationToggle` (the magnifier, whose tooltip is "Quick navigation (Ctrl+K)")
  for 1.5 seconds, clear the ring, then press the shortcut with `scene.Page.Keyboard.PressAsync("Control+K")`. Wait
  until `#adminQuickNavigationModal.show` and at least one `#adminQuickNavigationResults li` are shown. *Visible at
  the end:* the palette open over the page, the input showing "Go to an admin page...", the list of destinations, and
  the footer hints "to navigate", "to open", "to close".
  *Labels:* `src/OrchardCore.Modules/OrchardCore.Admin/Views/QuickNavigationNavbarItem.cshtml:13` (tooltip),
  `:23` (placeholder), `:28-30` (footer). Behavior: `src/OrchardCore.Modules/OrchardCore.Admin/Assets/js/quick-navigation.ts`.
- `quick-2`: `TypeAsync` on `#adminQuickNavigationInput`, text `user`, delay 120. *Visible at the end:* the results
  "Users" with the path "Access Control" and "User Login" with the path "Settings", "Security" under its name; the
  first result highlighted. *Labels:* `src/OrchardCore.Modules/OrchardCore.Users/AdminMenu.cs:27,30` (Access Control,
  Users) and `:40-42` (Settings, Security, User Login).
- `quick-3`: keep the cursor on the input of the palette for the whole step: moving it over the results highlights
  the hovered result (the `mousemove` handler of `quick-navigation.ts`) and would break the arrow-key sequence.
  Press ArrowDown (the second result is highlighted), wait 0.9 seconds, press ArrowUp (back to Users), wait
  0.9 seconds, then `WaitForNavigationAsync` around a press of Enter. *Visible at the end:* the Users page (title
  "Users"), the palette closed. The Esc key is only named in the narration: do not press it in this clip.
  *Label:* `src/OrchardCore.Modules/OrchardCore.Users/Views/Admin/Index.cshtml:4`.

## Clip `06-content`

Starts on `/Admin/Features` (opened before the capture). Content Quick Navigation must be disabled: the element
`#btn-enable-OrchardCore_Contents_QuickNavigation` exists. **This clip changes the site.**

- `content-1`: `TypeAsync` on `#search-box`, text `quick`, delay 110. Wait until the list shows the row of Content
  Quick Navigation, then highlight `#btn-enable-OrchardCore_Contents_QuickNavigation`. *Visible at the end:* the
  Features page filtered to the feature "Content Quick Navigation" (category Content Management, status Disabled), a
  ring on its Enable button. *Labels:* `src/OrchardCore.Modules/OrchardCore.Contents/Manifest.cs:24` (name);
  `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27` (Search);
  `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable).
- `content-2`: clear the ring, `ClickAndWaitForNavigationAsync` on `#btn-enable-OrchardCore_Contents_QuickNavigation`.
  Wait for `.toast.message-success`. *Visible at the end:* the message "The feature Content Quick Navigation was
  enabled." on the right side, near the top of the page (a fixed toast container at the end side of the page:
  `src/OrchardCore.Themes/TheAdmin/Views/NotifyMessages.cshtml`; it hides itself after 5 seconds, `NotifierExtensions.cs:71`).
  If it is gone before the narration ends, the row must show the status "Enabled"
  (`src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml`); record again if
  neither is visible.
  *Label:* `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`.
- `content-3`: press `Control+K` with `scene.Page.Keyboard.PressAsync`, wait for the palette, then `TypeAsync` on
  `#adminQuickNavigationInput`, text `about`, delay 120. Wait until a result titled "About" is shown. *Visible at the
  end:* the palette with the result "About" and the path "Content" under it. *Label:*
  `src/OrchardCore.Modules/OrchardCore.Contents/QuickNavigation/ContentItemNavigationSource.cs:90` (the path "Content").
- `content-4`: press Escape to close the palette. Click the Settings label of the menu, then
  `ClickAndWaitForNavigationAsync` on the Admin entry (`#left-nav .item-label:has(span.title:text-is("Admin"))`).
  The field is the last one of the page: it sits below the whole Admin settings block (theme toggler, menu filter,
  quick navigation, New menu, titles, breadcrumb, menu behavior, list layout, actions layout, user layout choice;
  `ContentQuickNavigationSettingsDisplayDriver.cs` places it at `Content:4`, after `AdminSiteSettingsDisplayDriver.cs`
  at `Content:3`), so a single small scroll may not reach it. Scroll down with `ScrollAsync(0, 400)`, repeated until
  the label is inside the window (`HighlightAsync` also scrolls an element into view), then highlight
  `label:text-is("Recent content items in quick navigation")`. Change nothing, save nothing. *Visible at the end:* the
  "Settings" page with a ring on the label "Recent content items in quick navigation" and its field showing 50.
  *Labels:* `src/OrchardCore.Modules/OrchardCore.Admin/AdminMenu.cs:26-27` (Settings, Admin);
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentQuickNavigationSettings.Edit.cshtml:4`;
  default 50 in `src/OrchardCore.Modules/OrchardCore.Contents/QuickNavigation/ContentQuickNavigationSettings.cs:15`.
- `content-5`: clear the ring, scroll back to the top (`ScrollAsync(0, -3000)`, more than the page is long, so it stops at the top). Highlight in turn, 2 seconds each,
  `label:text-is("Enable theme toggler")`, `label:text-is("Enable Admin Menu filter")` and
  `label:text-is("Navigation menu behavior")`, clearing each ring first. Click nothing. *Visible at the end:* the top
  of the Admin settings with a ring on "Navigation menu behavior", every checkbox unchanged.
  *Labels:* `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminSettings.Edit.cshtml:8,18,69`.

## Clip `07-topbar`

Starts on `/Admin`. The top bar holds, from left to right on its right side: the magnifier, the theme toggle, the Visit
Site icon, and the user icon (`src/OrchardCore.Modules/OrchardCore.Admin/Drivers/QuickNavigationNavbarDisplayDriver.cs:21`,
`src/OrchardCore.Themes/TheAdmin/Drivers/ToggleThemeNavbarDisplayDriver.cs:21`,
`src/OrchardCore.Modules/OrchardCore.Admin/Drivers/VisitSiteNavbarDisplayDriver.cs:12`).

- `top-1`: move to the Visit Site icon (`.user-top-navbar a[aria-label="Visit Site"]`: the link has a `title`, but
  once the page script has made it a Bootstrap tooltip the `title` attribute is gone, and the attributes `aria-label`
  and `data-bs-original-title` hold "Visit Site"; `a[title="Visit Site"]` matches nothing), wait 1 second for its
  tooltip "Visit Site", then `ClickAndWaitForNavigationAsync` on it. Wait for the page to load. *Visible at the end:* the front end of the site
  (the Blog theme, the home page showing the blog), in the same tab.
  *Label:* `src/OrchardCore.Modules/OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:2`.
- `top-2`: `NavigateAsync("/Admin")` (the first 1.5 seconds). Click `#bd-theme` (the theme toggle): the menu shows
  Auto, Light and Dark. Wait 1 second. Click `button[data-bs-theme-value="dark"]`: the admin turns dark. Wait 1.5
  seconds. Click `#bd-theme`, then `button[data-bs-theme-value="light"]`: the admin is light again. Keep every wait
  short, so the actions end before the narration. *Visible at the end:* the admin start page in the **light** theme, the
  toggle closed. *Labels:* `src/OrchardCore.Themes/TheAdmin/Views/ToggleTheme.cshtml:3` (Toggle theme), `:13` (Auto),
  `:21` (Light), `:29` (Dark).
- `top-3`: click `#navbarDropdown` (the user icon). Wait 1 second, then move the cursor over the entries without
  clicking any. **Never click Log off**, Profile or Change password. *Visible at the end:* the open user menu: "Signed
  in as admin", Profile, Change password, Log off. *Labels:*
  `src/OrchardCore.Modules/OrchardCore.Users/Views/UserMenuItems-SignedUser.cshtml:3` (Signed in as),
  `UserMenuItems-Profile.cshtml:9` (Profile), `UserMenuItems-ChangePassword.DetailAdmin.cshtml:11` (Change password),
  `UserMenuItems-SignOut.cshtml:4` (Log off), in the same folder.

## Checks for private data

The site is fresh and every value is fake, so no mask script is needed. Still, look at every contact sheet
(`review.py`) and OCR the assembled video (one frame per second, at every scene change, and at 5 frames per second)
for:

- The name in the top bar and the user tooltip: only `Orchard Core` and `admin`.
- The email `admin@example.com` is fake data (example.com) and may be shown: the Users page that `quick-3` ends on
  lists each user with the email beside the name (`src/OrchardCore.Modules/OrchardCore.Users/Views/UserFields.cshtml:7`).
  Check that list shows only the user `admin` with that address. No clip opens the profile or the user editor.
- The front end in `top-1`: sample blog text of the recipe, no person's name, no real address.
- The password: it is never typed on camera (the session comes from `state.json`).
- Any file `state.json`, `App_Data` or mask script stays out of git.

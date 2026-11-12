# Recording plan: Themes, change the look of your site

This plan lets a person record the video without deciding anything. The audience is `admin`: the clips show the admin UI
and the front end of the site. Selectors in this file stand for `scene.Page.Locator(...)`. File references (`file:line`)
are paths in the Orchard Core checkout the content was read from (commit `92db4a83`), starting at its repository root,
except those that start with `.agents/`, which are in this repository.

| Chapter | Clip | Kind | Steps |
|---|---|---|---|
| 00 | `00-intro` | card | `intro` |
| 01 | `01-concept` | slides | `what-1`, `what-2`, `what-3`, `what-4` |
| 02 | `02-page` | recording | `page-1` to `page-5` |
| 03 | `03-site` | recording | `site-1` to `site-4` |
| 04 | `04-toggle` | recording | `toggle-1` to `toggle-3` |
| 05 | `05-reset` | recording | `reset-1`, `reset-2` |
| 06 | `06-permission` | slide | `who-1` |
| 99 | `99-outro` | card | `outro` |

## 1. The site it needs

- **Application**: `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore`, on `http://localhost:5010`, with its own empty
  `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). Start it before anything else and stop it by its
  process ID.
- **Provisioning**: Auto Setup through environment variables, so no setup form is recorded (this video is not about the
  Setup screen). Set `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
  `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
  `...__AdminPassword=<password>` (the value of `VIDEO_PASSWORD`, chosen outside git), `...__DatabaseProvider=Sqlite`
  and `...__RecipeName=Blog`. The Blog recipe is required: it enables The Blog Theme, The Admin Theme and Safe Mode Theme,
  and sets The Blog Theme as the Site theme and The Admin Theme as the Admin theme
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:77-86`).
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` of the site's process to a dead address, so the site never calls
  an outside service. This does not affect the recorder's browser. The Blog Theme asks Google Fonts for its fonts
  (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12-13`), so the recording machine needs internet access for
  the front end to look as designed; without it the browser uses fallback fonts (see `OPEN-QUESTIONS.md`).
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` writes `state.json`
  (git-ignored). Create it from a clean browser context.
- **Features to enable before recording**: none. Leave every feature as the Blog recipe leaves it.
- **Settings to save, external services, seed data**: none. The Themes page lists themes, not content, and the front end
  shows the content the Blog recipe creates, which is fake. Leave Settings, Admin and Settings, Security, User Login at
  their defaults. The setting "Enable theme toggler" is **on** by default
  (`src/OrchardCore/OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:7-8`,
  `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminSettings.Edit.cshtml:8`) and the Blog recipe does not change it, so
  the top bar of the admin shows a "Toggle theme" button
  (`src/OrchardCore.Themes/TheAdmin/Drivers/ToggleThemeNavbarDisplayDriver.cs:20`). The navigation bar of The Default
  Theme shows one too whenever it is the Site theme, whatever the setting says
  (`src/OrchardCore.Themes/TheTheme/Drivers/ToggleThemeNavbarDisplayDriver.cs:21`). With the setting on and no cookie the
  value "auto" applies (`src/OrchardCore.Modules/OrchardCore.Themes/Services/ThemeTogglerService.cs:29-39`, used by the layouts
  of both themes), so the admin and The Default Theme follow the color scheme of the browser, and the recorder's browser reports the light scheme
  (this repository's `.agents/skills/orchardcore-video-producer/scripts/recorder/Scene.cs`, `ColorScheme.Light`). Never click the button.
- **Starting state, "snapshot A"**: after Auto Setup and `Recorder setup`, open `/Admin/Themes` and check that the cards
  show exactly this, then stop the site and copy its `App_Data` folder and `state.json` aside:

  | Card | State |
  |---|---|
  | The Blog Theme | `This is the current Site theme`, no buttons |
  | The Admin Theme | `This is the current Admin theme`, no buttons |
  | The Agency Theme | Make Current, Enable |
  | The Coming Soon Theme | Make Current, Enable |
  | The Default Theme | Make Current, Enable |
  | Safe Mode Theme | Make Current, Disable |

  The cards are sorted with the current ones first (`src/OrchardCore.Modules/OrchardCore.Themes/Controllers/AdminController.cs:92`)
  and in an order the code does not fix after that. Never find a card by its position: find it by its title (see the
  locators below).
- **Window**: the recorder's standard 1536x864 viewport. The admin menu stays expanded (do not collapse it).
- **Private data**: the only personal-looking values are the user name `admin` (the top bar of the admin and the user menu
  of The Default Theme), the author "admin" of the blog posts, and the card author "The Orchard Core Team". The Themes
  page shows each theme's version number; it is read from the build and is not private.

## 2. Order of the clips, and what each one changes

| Clip | Reads or changes the site | Needs |
|---|---|---|
| `02-page` | reads only | snapshot A |
| `03-site` | **enables The Default Theme and makes it the Site theme** | snapshot A |
| `04-toggle` | **makes The Blog Theme the Site theme again, then disables The Default Theme** | snapshot B |
| `05-reset` | reads only (opens a confirmation dialog and cancels it) | snapshot A, or the state `04-toggle` leaves |

- Record in this order: `02-page`, `03-site`, `04-toggle`, `05-reset`. `04-toggle` starts from the state `03-site` leaves.
  `04-toggle` leaves the site in the same state as snapshot A (The Blog Theme current, The Default Theme disabled), so
  `05-reset` can follow it.
- **Snapshot A**: described above. Restore it, and start the site again, before every full take and before recording
  `03-site` again.
- **Snapshot B**: restore A, start the site, open `/Admin/Themes` outside the capture, click Make Current on The Default
  Theme, and wait until its card shows `This is the current Site theme`. Stop the site and copy `App_Data` aside.
  Restore B before recording `04-toggle` alone.

## 3. How every recording clip works

- Each clip method starts with `scene.OpenAsync(...)` (not recorded), then `scene.StartCaptureAsync()`.
- The seconds in each entry are estimates from the step's words (0.4 s a word). `durations.json` has the exact length
  after `narrate.py`; keep the actions of a step shorter than its narration, and where an entry says "at about N s", use
  `PauseAsync` so the action lands on the words it illustrates.
- Navigate only with `ClickAndWaitForNavigationAsync` or `NavigateAsync`. Highlight rings are fixed on the page: call
  `ClearHighlightsAsync()` before scrolling or navigating.
- Bring an element to the middle of the window with
  `await locator.EvaluateAsync("e => e.scrollIntoView({block: 'center'})")` (an instant jump). Return to the top with
  `await scene.Page.EvaluateAsync("window.scrollTo(0, 0)")`.
- **Enabling or disabling a theme changes the features of the tenant, which reloads it** (a comment of the controller notes
  that the current shell is disposed after the update,
  `src/OrchardCore.Modules/OrchardCore.Themes/Controllers/AdminController.cs:138`), and the page the redirect lands on can
  take several seconds to load. `ClickAndWaitForNavigationAsync` waits for the whole reload (network idle), so a wait placed
  after it finds the page already loaded and cuts nothing: the stall would stay in the clip as a frozen frame. Wrap the
  call itself in `OffCameraAsync` instead, after resting the cursor on the button on camera:

  ```csharp
  await scene.MoveToAsync(button);                                                  // on camera: the cursor glides to the button
  await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(button));   // the click, the restart and the reload are cut out
  await marker.WaitForAsync();                                                      // a check only: returns at once
  ```

  `OffCameraAsync` shows 0.9 s of the page as it is, then drops the frames until the reloaded page is ready, and takes that
  time out of the clip (`Scene.cs`, `OffCameraAsync`, `ClickAndWaitForNavigationAsync`). The click ripple is not recorded; the
  video cuts from the cursor on the button to the reloaded page. The entries below say which steps use this
  pattern (`site-2` and `toggle-2`, which enable and disable a feature) and which do not (`toggle-1`, which only saves the
  site settings because The Blog Theme is already enabled). If a take of `toggle-1` shows a pause longer than 2 s after the
  click, use the pattern there too.
- In this plan `HighlightAsync(<card>, <part>)` means `HighlightAsync(` the locator of the part inside the card `)`, and
  `HighlightAsync(<locator>, "caption")` has the caption as its second argument. `HighlightAsync` scrolls the element into
  view if it is not, and the ring is fixed on the page. `ScrollAsync(0, n)` turns the mouse wheel in 12 steps, with the cursor
  where it is.
- The success messages are toasts at the top right of the page. Each one hides itself 5 seconds after the page appears
  (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`,
  `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml:15`). The end state of a step never depends on a toast; when an
  entry mentions one, it is a bonus on the contact sheet.
- Named locators used below (`<name>` is the title of a card, written as the card shows it):

| Name | Locator | Source |
|---|---|---|
| Customize theme card | `div.card:has(h5.card-title:has-text("Customize theme"))` | `src/OrchardCore.Modules/OrchardCore.Themes/Views/AdminDashboardThemes.cshtml:4` |
| Manage themes button | `a.btn:text-is("Manage themes")` | `AdminDashboardThemes.cshtml:6` |
| Design menu button | `#left-nav button:has(span.title:text-is("Design"))` | `src/OrchardCore.Themes/TheAdmin/Views/NavigationItemText.cshtml`, `src/OrchardCore.Modules/OrchardCore.Themes/Views/NavigationItemText-design.Id.cshtml:4` |
| Themes menu link | `#left-nav a[href$="/Admin/Themes"]` | `src/OrchardCore.Modules/OrchardCore.Themes/AdminMenu.cs:21-24` |
| Visit Site link | `ul.user-top-navbar a.nav-link[href="/"]` (an icon; its tooltip is Visit Site; do not select it by its title, since Bootstrap moves the title when it builds the tooltip) | `src/OrchardCore.Modules/OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:2` |
| theme card | `div.theme-card:has(h4.card-title:text-is("<name>"))` | `Views/ThemeEntry.SummaryAdmin.cshtml:1`, `Views/Items/ThemeEntry-Title.SummaryAdmin.cshtml:6` |
| Make Current button | `<theme card> button:text-is("Make Current")` | `Views/Items/ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7` |
| Disable button | `<theme card> button:text-is("Disable")` | `Views/Items/ThemeEntry-ButtonsToggleState.SummaryAdmin.cshtml:9` |
| Enable button | `<theme card> button:text-is("Enable")` | `ThemeEntry-ButtonsToggleState.SummaryAdmin.cshtml:16` |
| current marker | `<theme card> span.text-muted:text-is("This is the current Site theme")`, or `...Admin theme` for The Admin Theme | `Views/Items/ThemeEntry-Current.SummaryAdmin.cshtml:8, 13` |
| description | `<theme card> p.text-muted` | `Views/Items/ThemeEntry-Descriptions.SummaryAdmin.cshtml:11-13` |
| attributes list | `<theme card> ul.list-group` | `Views/ThemeEntry.SummaryAdmin.cshtml:24`, `Views/Items/ThemeEntry-Attributes.SummaryAdmin.cshtml` |
| tags row | `<theme card> li.list-group-item:has(i.fa-tags)` | `ThemeEntry-Attributes.SummaryAdmin.cshtml:24-31` |
| Reset box | `div.alert.alert-danger:has(h4:text-is("Reset current themes"))` | `Views/Admin/Index.cshtml:30-31` |
| Reset Site Theme button | `<Reset box> a.btn-danger:text-is("Reset Site Theme")` | `Views/Admin/Index.cshtml:33` |
| Reset Admin Theme button | `<Reset box> a.btn-danger:text-is("Reset Admin Theme")` (highlight only) | `Views/Admin/Index.cshtml:34` |
| dialog buttons | `#modalOkButton` (Ok, never clicked), `#modalCancelButton` (Cancel) | `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:30-31` |
| toast | `.toast.message-success` | `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml:15` |

(`Views/...` paths above are in `src/OrchardCore.Modules/OrchardCore.Themes/`.)

## 4. Clips

### `00-intro`, card

- `intro`: the opening card, "Orchard Core Themes", subtitle "Change the look of your site". Nothing to record.

### `01-concept`, slides (chapter 01, "What a theme is")

- `what-1`: heading "A theme is the look of a site", three cards: Layout, Colors and fonts, Your content.
- `what-2`: heading "Themes can include content templates", three cards: Layout and styling, Content templates, The Blog Theme
  (its templates for blog posts, categories and tags are the files `Content-BlogPost.liquid`, `Content-Category.liquid`
  and `Tag-TermPart.liquid` in `src/OrchardCore.Themes/TheBlogTheme/Views/`).
- `what-3`: heading "Two themes for every site", three cards: Site theme, Admin theme, Sign-in page (the setting
  "Use site theme for login page", `src/OrchardCore.Modules/OrchardCore.Users/Views/LoginSettings.Edit.cshtml:62`, under
  Settings, Security, User Login, `src/OrchardCore.Modules/OrchardCore.Users/AdminMenu.cs:40-45`; the Admin theme is used
  unless it is checked, `src/OrchardCore.Modules/OrchardCore.Users/Services/UsersThemeSelector.cs:86`).
- `what-4`: heading "The setup recipe picks the Site theme", four cards (Blog, Agency, Coming Soon, Software as a Service,
  each with its theme) and a line below them: "Blank site and Headless site set no Site theme." The recipe names are the
  `displayName` of each recipe and the themes are the `site` value of its `themes` step: `TheBlogTheme/Recipes/blog.recipe.json:3,83-86`,
  `TheAgencyTheme/Recipes/agency.recipe.json:3,69-72`, `TheComingSoonTheme/Recipes/comingsoon.recipe.json:3,66-69`,
  `TheTheme/Recipes/saas.recipe.json:3,39-42`, `TheAdmin/Recipes/blank.recipe.json:3,65-67` and
  `TheAdmin/Recipes/headless.recipe.json:3,107-109`, all under `src/OrchardCore.Themes/`.

### `02-page`, recording (chapter 02, "The Themes page"), needs snapshot A

Start page: `/Admin` (the admin home page, not recorded: `scene.OpenAsync("/Admin")`, then wait for the Customize theme
card to be visible). The page shows the "Welcome to Orchard Core" alert and a row of cards, one of them Customize theme
(`src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminDashboard.cshtml`).

- `page-1` (about 13 s). At the start, `HighlightAsync(Customize theme card)`; the ring stays while "The Customize theme card
  has a Manage themes button that opens the Themes page" is spoken. At about 8.4 s (the words "The same page"),
  `ClearHighlightsAsync()` and click the Design menu button so its entries open; the click ends before the words "in the
  admin menu" end (about 11.2 s). At about 11.6 s (the words "under Design, Themes"),
  `HighlightAsync(Themes menu link, "Design, Themes")`.
  *Visible at the end*: the admin home page, the Design group open in the left menu with a ring on Themes.
  *Labels*: Customize theme, Manage themes (`AdminDashboardThemes.cshtml:4,6`); Design, Themes (`AdminMenu.cs:18,21`).
- `page-2` (about 14 s). `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync(Manage themes button)` at once; the
  page loads while "The Themes page shows a card for each theme you can apply" is spoken. Wait until the title "The Blog Theme" is on the
  page. Do not scroll.
  *Visible at the end*: the Themes page from its top; the first row of cards holds The Blog Theme and The Admin Theme, whose
  footers say `This is the current Site theme` and `This is the current Admin theme`, in whichever order the page lists
  them, and a third card with its two buttons. The page title is Themes.
  *Labels*: Themes (`Views/Admin/Index.cshtml:12`); This is the current Site theme, This is the current Admin theme
  (`ThemeEntry-Current.SummaryAdmin.cshtml:8,13`); the current ones come first (`AdminController.cs:92`).
- `page-3` (about 11 s). At the start, scroll The Blog Theme card to the middle of the window (`scrollIntoView`,
  `block: 'center'`), so that its thumbnail and its attributes list are both visible, and `HighlightAsync(The Blog Theme card)`
  (the thumbnail, the name and the description are in it). At about 5 s (the words "Below them are the author"),
  `ClearHighlightsAsync()` and `HighlightAsync(The Blog Theme card, attributes list)`. If the card is taller than the window,
  keep its attributes list in view, which the end of the step needs, and let the top of the card be cut
  (`OPEN-QUESTIONS.md`, item 3).
  *Visible at the end*: The Blog Theme card with a ring around its list: author "The Orchard Core Team", the website,
  the tags "Blog, Bootstrap, Liquid", and the version.
  *Labels*: the thumbnail, name, description and attributes come from `Views/Items/ThemeEntry-Thumbnail.SummaryAdmin.cshtml:6`,
  `ThemeEntry-Title.SummaryAdmin.cshtml:6`, `ThemeEntry-Descriptions.SummaryAdmin.cshtml:11-13` and
  `ThemeEntry-Attributes.SummaryAdmin.cshtml:6-35`; the values come from `src/OrchardCore.Themes/TheBlogTheme/Manifest.cs:4-10`.
- `page-4` (about 13 s). `ClearHighlightsAsync()`. At the start, scroll The Admin Theme card into view
  (`scrollIntoView`, `block: 'center'`) and `HighlightAsync(The Admin Theme card, tags row)`; the ring stays to the end of
  the step. Do not click anything: The Admin Theme has no buttons.
  *Visible at the end*: The Admin Theme card with a ring on its tags row, which reads "Admin", and its footer
  `This is the current Admin theme`.
  *Labels*: the tag is `ManifestConstants.AdminTag`, `src/OrchardCore/OrchardCore.Abstractions/Modules/Manifest/ManifestConstants.cs:12`,
  set in `src/OrchardCore.Themes/TheAdmin/Manifest.cs:14-17`; a theme with it is an admin theme
  (`AdminController.cs:231-234`), and Make Current sets the Admin theme for it and the Site theme for any other
  (`AdminController.cs:127-136`).
- `page-5` (about 12 s). `ClearHighlightsAsync()`. Start from the top of the page. At about 6 s (the words "Safe Mode Theme"),
  `ScrollAsync(0, 420)` smoothly, then `HighlightAsync(Safe Mode Theme card, description)`, which lands before the words
  "which describes itself" (about 7.2 s).
  *Visible at the end*: the lower row of cards, with Safe Mode Theme in view and a ring on its description "A fail-safe theme
  when no other themes are available.". The cards of The Agency Theme, The Coming Soon Theme and The Default Theme are each in
  view at some moment of the step; if the order of the cards puts one of them out of view, scroll so that it is shown at the
  step's start, and Safe Mode Theme at its end.
  *Labels*: the names and descriptions are in the manifests, `src/OrchardCore.Themes/TheAgencyTheme/Manifest.cs:4-10`,
  `TheComingSoonTheme/Manifest.cs:4-10`, `TheTheme/Manifest.cs:4-19` and `SafeMode/Manifest.cs:4-10`.

### `03-site`, recording (chapter 03, "Change the Site theme"), needs snapshot A

Start page: `/Admin/Themes` (not recorded), from its top.

- `site-1` (about 11 s). Move the cursor to the Visit Site link at once (its tooltip "Visit Site" shows while the cursor
  rests on it). At about 5 s (the words "click the Visit Site icon"), `ClickAndWaitForNavigationAsync(Visit Site link)`.
  Wait until `nav#mainNav` is on the page.
  *Visible at the end*: the home page of the front end in The Blog Theme: its navigation bar with the site name "Orchard
  Core" and the blog below it. Nothing from the admin.
  *Labels*: Visit Site (`src/OrchardCore.Modules/OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:2`); the Site theme is The
  Blog Theme (`blog.recipe.json:83-86`; layout `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:25`).
- `site-2` (about 15 s, the stall case). `NavigateAsync("/Admin/Themes")` at the start, and wait until the title "The Default
  Theme" is on the page. Scroll The Default Theme card to the middle of the window and `HighlightAsync(The Default Theme
  card)`. At about 5 s (the words "when you click Make Current" are not spoken yet), `ClearHighlightsAsync()` and
  `MoveToAsync(The Default Theme card, Make Current button)`. At about 7.5 s call
  `OffCameraAsync(() => ClickAndWaitForNavigationAsync(The Default Theme card, Make Current button))`, so that the cut falls at
  about 8.5 s, the end of the words "when you click Make Current". Then
  `<The Default Theme card, current marker>.WaitForAsync()`.
  *Visible at the end*: the Themes page from its top after the reload, the card of The Default Theme in the first row with
  `This is the current Site theme`, and, for about 4 s after the camera resumes, two toasts at the top right: "The Default Theme
  was enabled" and "The Default Theme was set as the default Site theme", which the narration of the last sentence
  mentions. If the toasts are gone before that sentence ends, the click came too early: call `OffCameraAsync` about a second
  later. The toasts are a bonus: the end state is the card.
  *Labels*: Make Current (`ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7`); the messages are
  `AdminController.cs:145,148` (the first only because The Default Theme is disabled, `AdminController.cs:139-146`).
- `site-3` (about 11 s). `ClearHighlightsAsync()`. At the start, `HighlightAsync(The Default Theme card, current marker)`; the
  ring stays while the first sentence is spoken (to about 7 s). At about 7 s (the words "The Blog Theme offers"),
  `ClearHighlightsAsync()` and scroll The Blog Theme card to the middle of the window. At about 8.8 s (the words "Make
  Current"), `HighlightAsync(The Blog Theme card, Make Current button)`, then, at about 10 s (the word "Disable"),
  `HighlightAsync(The Blog Theme card, Disable button)`.
  *Visible at the end*: The Blog Theme card with its two buttons, Make Current and Disable, and rings on them.
  *Labels*: `ThemeEntry-Current.SummaryAdmin.cshtml:13`, `ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7`,
  `ThemeEntry-ButtonsToggleState.SummaryAdmin.cshtml:9`; a current theme has no buttons
  (`src/OrchardCore.Modules/OrchardCore.Themes/Drivers/ThemeEntryDisplayDriver.cs:19-29`).
- `site-4` (about 9 s). `ClearHighlightsAsync()`, scroll to the top (`window.scrollTo(0, 0)`), and at about 2 s
  `ClickAndWaitForNavigationAsync(Visit Site link)`. Wait until `nav.navbar.fixed-top` is on the page.
  *Visible at the end*: the home page of the front end in The Default Theme: a navigation bar across the top with the site
  name "Orchard Core", the main menu, the "Toggle theme" button (see section 1) and the user menu "admin", and the blog
  content below it with another layout and styling than at the end of `site-1` (`OPEN-QUESTIONS.md`, item 1).
  *Labels*: the layout is `src/OrchardCore.Themes/TheTheme/Views/Layout.cshtml:45`.

### `04-toggle`, recording (chapter 04, "Enable and disable themes"), needs snapshot B

Start page: `/Admin/Themes` (not recorded), from its top. The Default Theme is the current Site theme and is enabled; The Blog
Theme shows Make Current and Disable.

- `toggle-1` (about 12 s, no restart). Scroll The Blog Theme card to the middle of the window and `HighlightAsync(The Blog Theme card,
  Make Current button)`. At about 3 s (the words "clicking Make Current"), `ClearHighlightsAsync()` and
  `ClickAndWaitForNavigationAsync(The Blog Theme card, Make Current button)` on camera. Then
  `<The Blog Theme card, current marker>.WaitForAsync()`.
  *Visible at the end*: the Themes page from its top; The Blog Theme card shows `This is the current Site theme`; one toast,
  "The Blog Theme was set as the default Site theme", is a bonus (there is no "was enabled" message, since The Blog Theme
  was already enabled, `AdminController.cs:139-148`).
- `toggle-2` (about 12 s, the stall case). Scroll The Default Theme card to the middle of the window and `HighlightAsync(The Default
  Theme card, Disable button)`. At about 2 s (the words "Let's click Disable"), `ClearHighlightsAsync()` and
  `MoveToAsync(The Default Theme card, Disable button)`. At about 3.7 s call
  `OffCameraAsync(() => ClickAndWaitForNavigationAsync(The Default Theme card, Disable button))`, so that the cut falls at about
  4.6 s, before "A message confirms it" (4.8 s). Then `<The Default Theme card, Enable button>.WaitForAsync()`, scroll The Default Theme
  card to the middle of the window again, and at about 8 s (the words "offers Enable") `HighlightAsync(The Default Theme card,
  Enable button)`.
  *Visible at the end*: The Default Theme card with its buttons Make Current and Enable, a ring on Enable; a toast "The
  Default Theme was disabled." is a bonus.
  *Labels*: Disable, Enable (`ThemeEntry-ButtonsToggleState.SummaryAdmin.cshtml:9,16`); the message is `AdminController.cs:203`.
- `toggle-3` (about 13 s). `ClearHighlightsAsync()`, scroll to the top, and `HighlightAsync(The Blog Theme card, current marker)`.
  At about 5 s, `ClearHighlightsAsync()` and `HighlightAsync(The Admin Theme card, current marker)`. Do not click anything.
  *Visible at the end*: the top of the Themes page, the two current cards with no Enable, Disable or Make Current buttons, and
  a ring on the marker of The Admin Theme.
  *Labels*: `ThemeEntry-Current.SummaryAdmin.cshtml:8,13`; the buttons are added only for a theme that is not current
  (`Drivers/ThemeEntryDisplayDriver.cs:19-29`).

### `05-reset`, recording (chapter 05, "Reset the current themes"), needs snapshot A

Start page: `/Admin/Themes` (not recorded), from its top. **Never click Reset Admin Theme, and never click Ok in the dialog**:
a reset clears the setting: after Reset Site Theme the front end has no Site theme, and after Reset Admin Theme the admin pages
are drawn by the Site theme (`src/OrchardCore.Modules/OrchardCore.Themes/Services/SiteThemeSelector.cs:29`,
`src/OrchardCore.Modules/OrchardCore.Admin/AdminThemeSelector.cs:30-33`, `src/OrchardCore/OrchardCore.DisplayManagement/Theming/ThemeManager.cs:52-55`).

- `reset-1` (about 13 s). `ScrollAsync(0, 3000)` smoothly to the bottom of the page at the start, then
  `HighlightAsync(Reset box)`. At about 5.6 s (the words "Reset Site Theme"), `ClearHighlightsAsync()` and
  `HighlightAsync(Reset Site Theme button)`, then, at about 7.2 s (the words "Reset Admin Theme"),
  `HighlightAsync(Reset Admin Theme button)`.
  *Visible at the end*: the bottom of the Themes page, the red box "Reset current themes" with its text "The following
  actions reset the themes. It is used for sites that can't use theming." and the two buttons, with rings on them.
  *Labels*: `Views/Admin/Index.cshtml:31-34`.
- `reset-2` (about 12 s). `ClearHighlightsAsync()`. At about 1.5 s (the words "asks you to confirm"),
  `ClickAsync(Reset Site Theme button)`; the dialog titled "Reset Site Theme" opens with the text "Are you sure you want to
  reset the Site theme?" and the buttons Ok and Cancel, and stays open until the cancel. At about 9.6 s (the words "Leave them alone"),
  `ClickAsync(#modalCancelButton)`. Wait until the dialog is gone.
  *Visible at the end*: the bottom of the Themes page, no dialog, nothing changed. If the page is on a different
  screen because the dialog moved it, scroll back to the Reset box.
  *Labels*: `Views/Admin/Index.cshtml:33`; the dialog is built by `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-63`
  for a link marked `UnsafeUrl RemoveUrl` (`:118-182`), from the link's `data-title` and `data-message`, with the button texts of `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`.

### `06-permission`, slide (chapter 06, "Permissions")

- `who-1`: heading "The Apply a Theme permission", three cards: Themes page, Administrator, Other roles. The permission
  is `ApplyTheme`, displayed as "Apply a Theme" (`src/OrchardCore.Modules/OrchardCore.Themes/Permissions.cs:8`); the Themes
  page and its menu entry need it (`AdminController.cs:46-51`, `AdminMenu.cs:23`); the Administrator role has it by default
  (`Permissions.cs:18-25`); roles are edited under Access Control, Roles (`src/OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19`).

### `99-outro`, card

- `outro`: the closing card, "Orchard Core Themes", subtitle "docs.orchardcore.net". Nothing to record.

## 5. Checks for private data

- Before publishing, scan the video at one frame per second and at every scene change. The only names on screen are
  `admin`, "Orchard Core", "The Orchard Core Team" and the fake content of the Blog recipe.
- The front end shows the signed-in user "admin" in The Default Theme. No email address is on any screen of this video;
  if one appears (a profile page, a tooltip), record the clip again.
- The recorder's `state.json` and the site's `App_Data` folder stay outside git. The password is never typed on camera.

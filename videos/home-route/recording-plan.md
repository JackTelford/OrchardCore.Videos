# Recording plan: Home Page: Choose What Your Site Shows First

An admin video: the recorder shows the admin screens and the front end of the site, never code. Read this file top to
bottom once, then record the clips in the order of "Order of the takes". Run `scripts/narrate.py` first: each step is
held for the length of its narration (`durations.json`). The seconds below are estimates (0.4 s per word) to size the
actions; the real lengths are in `durations.json`.

Selectors are written as `scene.Page.Locator(...)` calls (or the other `scene.Page.GetBy...` calls). Source references
are paths inside `src/OrchardCore.Modules` of the Orchard Core checkout at commit 92db4a83, except where a folder is
named.

## 1. The site it needs

A fresh Orchard Core site, provisioned by Auto Setup (see `references/recording.md` of the skill for how to pass the
values; `ORCHARD_APP_DATA` points at an empty folder outside both repositories; set `HTTP_PROXY` and `HTTPS_PROXY` to a
dead address so the site calls no outside service):

| Auto Setup value | Value |
|---|---|
| Shell name (tenant) | `Default` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Recipe | `Blog` |
| Database provider | `Sqlite` |
| Admin user name | `admin` |
| Admin email | `admin@example.com` |
| Admin password | `<password>`: chosen outside git, passed to the recorder in `VIDEO_PASSWORD` |

Run it on `http://localhost:5010` (`VIDEO_BASE_URL`). The recorder signs in once with `Recorder setup`
(`VIDEO_USER_NAME=admin`), which writes `state.json` (git-ignored). Chromium at the specification's viewport.

What the Blog recipe gives this video (do not change any of it):

- The features `OrchardCore.HomeRoute`, `OrchardCore.Autoroute`, `OrchardCore.Contents`, `OrchardCore.ContentTypes`,
  `OrchardCore.Features` and `OrchardCore.Roles` are enabled (the recipe's `feature` step,
  `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-80`). Nothing is enabled or disabled before
  recording.
- The home page is the Blog content item: the `HomeRoute` of the recipe's `settings` step points at it
  (`blog.recipe.json:113-119`). Its permalink is `blog`.
- The content types Article, Blog and Page have the setting Show homepage options checked on their Autoroute part;
  Blog Post has it unchecked (`blog.recipe.json:159, 280, 475, 217`).
- The roles Moderator, Editor, Author and Contributor exist with no permissions (`blog.recipe.json:88-110`).
- The Display New menu setting stays off (its default). The seed below uses the New button of the Content Items list.
- The recipe also builds a custom admin menu (its `AdminMenu` step, from `blog.recipe.json:1188`): a top-level link
  named "Blog", a link "Main Menu", and a group "Content" with a link "Content Items". So a link named "Blog" exists
  in the left menu of every admin page, besides the title and the content type of the Blog row in the Content Items
  list. Never find the Blog row by the link name alone (see `change-1`).
- No external service is used. Nothing else is configured: no other tenant, culture or theme change.

### The Pattern box is hidden by a page script (required for `03-allow`)

The settings page of the Autoroute part starts with the box "Pattern", which holds a template written in Liquid
(`OrchardCore.Autoroute/Views/AutoroutePartSettings.Edit.cshtml:13-19`). An admin video never shows code, and the box
is on screen as soon as the page opens, so scrolling cannot keep it out of the frame at every moment of `allow-2` and
`allow-3`. Hide it for the whole recording with an init script that you keep outside git (it holds no name, so it
could be shared, but this repository commits only text of the kinds JSON, Python and Markdown). Save these lines in
a file outside both repositories and set `VIDEO_INIT_SCRIPT` to its path before `Recorder record`
(`.agents/skills/orchardcore-video-producer/scripts/recorder/Scene.cs:117-120` adds it to every page before it draws):

```js
const style = document.createElement("style");
style.textContent = ".ocat-limited-wrapper:has(textarea[name$='Pattern']) { display: none !important; }";
document.documentElement.appendChild(style);
```

The selector matches the wrapper of the label and the box on this page only (no other `.ocat-limited-wrapper` of the
video holds a textarea named `...Pattern`). After the first take of `03-allow`, check the frames: no template text
(double braces, pipes) may be visible.

## 2. The seed data (create it by hand, outside the capture)

One content item. Sign in as `admin`, open Content, Content Items, select New, then Article, and enter:

| Field | Value |
|---|---|
| Title | `Welcome to Juniper Lane Studio` |
| Permalink | leave empty (it is generated from the title: `welcome-to-juniper-lane-studio`) |
| Set as homepage | leave unchecked |
| Body (the editor labels this field with the name of the part, "HtmlBody"; type the text in it) | `Juniper Lane Studio is a small pottery workshop. Visit the shop, join a weekend class, or book a private lesson.` |
| Subtitle | `Handmade pottery and weekend workshops` |
| Banner Image (the Image field of the Article part) | leave empty |

The labels come from the recipe (`blog.recipe.json:685, 695` for Subtitle and Banner Image) and from
`OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml:22` (the label shows the display name of the part, which is the
part name without "Part" when the recipe sets none). The type Article sets the editor of its body to a rich text editor
(`blog.recipe.json:171`); type the text into it.

Select Publish. "Juniper Lane Studio" is an invented name; do not use a real business or person anywhere else.

Check the starting state before taking the first copy of the site (look only, save nothing):

1. `/` shows the Blog (its title and its blog post).
2. Content, Content Items lists `Welcome to Juniper Lane Studio` first (type Article, status Published).
3. Editing the Blog item shows, under Permalink, the blue note "This content item is the current homepage" and no
   Set as homepage checkbox. Editing `Welcome to Juniper Lane Studio` shows the Set as homepage checkbox. Leave both
   editors without saving.
4. Tools, Features: Home Route and Autoroute both show Enabled.
5. Access Control, Roles lists a role named Editor.

Then stop the site and copy its data folder aside as snapshot **S0**. Restore S0 before every take of `04-set`.

## 3. Order of the takes

The playing order is the chapter order, but the clips depend on the state of the site, so record in this order:

1. `02-features`, `03-allow`, `06-permission`: they only read the site and save nothing. They can be taken in any
   order, and again at any time.
2. `04-set`: starts from S0. It makes `Welcome to Juniper Lane Studio` the home page (it changes the site: the
   home route, and a new version of the item). After a good take, stop the site and copy its data folder aside as
   snapshot **S1**.
3. `05-change`: starts from S1 (the article is the home page). It makes the Blog the home page again. To take
   `04-set` again, restore S0. To take `05-change` again, restore S1.

The clips `04-set` and `05-change` leave the site changed (the home page and the item versions); never record another
clip of this video on a copy of the site that has been through them, and restore S0 for the next full session.

## 4. The clips

Every clip: open its start page with `scene.OpenAsync(...)` (not recorded), then `StartCaptureAsync()`. Clear a
highlight (`ClearHighlightsAsync()`) before the page changes or scrolls. Every navigation during the capture uses
`ClickAndWaitForNavigationAsync` or `NavigateAsync`. Keep each step's actions shorter than its narration.

### 4.1 `02-features` (chapter 02, Two features): read only

Start page: `/Admin` (the dashboard). Before the capture, click Tools in the admin menu (`scene.Page.Locator("#left-nav")`
and its item with the text `Tools`) so the item Features is shown. `OrchardCore.Features/AdminMenu.cs:26-27`.

- `features-1` (about 9.6 s). Click the menu item `Features` with `ClickAndWaitForNavigationAsync`, so `/Admin/Features`
  opens. Click `scene.Page.Locator("#search-box")` and type `Home Route` with `TypeAsync`. Wait until the row
  `scene.Page.Locator("[data-filter-value]", new() { HasText = "OrchardCore.HomeRoute" })` is visible. Highlight that row
  with `HighlightAsync` (no caption).
  Visible when the narration ends: the group heading "Infrastructure", the row "Home Route" with its id and the badge
  "Enabled", ringed.
  Labels: `OrchardCore.Features/Views/Admin/Features.cshtml:27` (search box), `:88` (the category heading),
  `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` ("Enabled"),
  `OrchardCore.HomeRoute/Manifest.cs:12-14` (name "Home Route", category "Infrastructure"). The search matches the name,
  the id and the description of a feature (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), so "Home Route"
  finds the row by its name.
- `features-2` (about 10.8 s). `ClearHighlightsAsync()`. Click the search box, select its text (`TripleClick` or
  Ctrl+A) and type `Autoroute`. Wait until the row with the id `OrchardCore.Autoroute` is visible and the Home Route
  row is hidden. Highlight the Autoroute row.
  Visible when the narration ends: the group heading "Navigation", the row "Autoroute" with the badge "Enabled" and,
  next to it, the badges of its direct dependencies, among them "Home Route"
  (`OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:13-18`, drawn in the Dependencies column
  of the Grid layout), ringed. Other rows that contain the word may also be listed; only the Autoroute row is ringed.
  Labels: `OrchardCore.Autoroute/Manifest.cs:4,14` (name "Autoroute", category "Navigation", dependency on
  `OrchardCore.HomeRoute` at `:9-13`).

### 4.2 `03-allow` (chapter 03, Allow a content type): read only, saves nothing

Start page: `/Admin`. Before the capture, open the admin menu group Design, then Content Definition inside it, so the
item Content Types is shown (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`).

- `allow-1` (about 7.6 s). Click the menu item `Content Types` (`ClickAndWaitForNavigationAsync`): `/Admin/ContentTypes/List`
  opens. Click `scene.Page.Locator("a[role='btn-edit-Article']")` (`ClickAndWaitForNavigationAsync`): the type editor
  `/Admin/ContentTypes/Edit/Article` opens.
  Visible when the narration ends: the page "Edit Content Type - Article".
  Labels: `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24` ("Edit"),
  `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63` (the title).
- `allow-2` (about 7.2 s). Scroll down (`ScrollAsync(0, ...)`) until the heading "Parts" and the row "Autoroute" (with
  its description "Provides a custom url for your content item.") are on screen. Highlight the Edit button of that row:
  `scene.Page.Locator("a[href*='/ContentTypes/Article/ContentParts/AutoroutePart/Edit']")`. Clear the highlight and
  click it (`ClickAndWaitForNavigationAsync`): the part settings open.
  Visible when the narration ends: the page "Edit Part - Autoroute", at its top, with the rows "Allow custom path",
  "Allow path updates" and "Show homepage options" below the green notice. The box "Pattern" is not on screen: the
  init script of section 1 hides it. Without that script this step would show Liquid code, which this video never shows.
  Labels: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:144` ("Parts"), `:163` ("Edit"),
  `OrchardCore.Autoroute/Migrations.cs:23` (the description), `OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:22`
  (the title "Edit Part - {0}"; the part is shown as "Autoroute": its name without "Part").
- `allow-3` (about 12.8 s). Scroll until the checkbox row "Show homepage options" is in the middle of the screen (the
  page is short: it may already be there). The box "Pattern" stays hidden by the init script of section 1; do not point
  at it. Highlight the checkbox row:
  `scene.Page.Locator(".form-check", new() { Has = scene.Page.Locator("input[name$='ShowHomepageOption']") })`.
  Do not click the checkbox and do not save the page.
  Visible when the narration ends: "Show homepage options" checked, with its hint "Check to allow the content items of
  this content type to be set as the homepage. It will only be visible to users with the appropriate permission.",
  ringed. The clip ends here; leave the page without saving.
  Labels: `OrchardCore.Autoroute/Views/AutoroutePartSettings.Edit.cshtml:44-46` (the label and the hint).

### 4.3 `04-set` (chapter 04, Set the home page): changes the site

Start page: `/` (the front end, signed in). Start from snapshot S0. The page shows the Blog.

- `set-1` (about 6.8 s). No click. `HoldAsync(2)`, then `ScrollAsync(0, 300)` slowly so the viewer sees the blog.
  Visible when the narration ends: the Blog home page with its title and blog post.
  Source: `blog.recipe.json:113-119` (the home page of the recipe).
- `set-2` (about 6 s). `NavigateAsync("/Admin/Contents/ContentItems")` (the clip starts on the front end, which has no
  admin menu, so the page is opened by its address while the narration reads the menu path). Wait until the row of
  `Welcome to Juniper Lane Studio` is visible. Click its Edit button
  (`scene.Page.Locator(".list-group-item, tr").Filter(new() { HasText = "Welcome to Juniper Lane Studio" }).Locator("a.edit").First`,
  `ClickAndWaitForNavigationAsync`); if the button is not found, click the title link
  (`scene.Page.GetByRole(AriaRole.Link, new() { Name = "Welcome to Juniper Lane Studio", Exact = true })`).
  Visible when the narration ends: the editor "Edit Article", with the title in the Title field.
  Labels: `OrchardCore.Contents/AdminMenu.cs:65,68` ("Content", "Content Items"),
  `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml` ("Edit"), `OrchardCore.Contents/Views/Admin/Edit.cshtml:39`.
- `set-3` (about 9.2 s). `HighlightAsync` the Permalink field (`scene.Page.GetByLabel("Permalink")`), wait 2 s, clear it,
  then highlight the checkbox row of Set as homepage
  (`scene.Page.Locator(".form-check", new() { Has = scene.Page.Locator("input[name$='SetHomepage']") })`).
  The Permalink field holds `welcome-to-juniper-lane-studio`.
  Visible when the narration ends: the checkbox "Set as homepage", unchecked, with its hint "Check to set this content
  item as the homepage once published.", ringed, under the Permalink field.
  Labels: `OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30` ("Permalink"), `:72-73` (checkbox and hint).
- `set-4` (about 6.8 s). `ClearHighlightsAsync()`. Click the checkbox
  (`scene.Page.Locator("input[type='checkbox'][name$='SetHomepage']")`). About 2 s into the step, click the Publish
  button (`scene.Page.Locator("button.publish")`, `ClickAndWaitForNavigationAsync`). The editor was opened from the
  Content Items list, so the page returns to that list.
  Visible when the narration ends: the Content Items list with the green message "Your Article has been published."
  (If the message has already gone, click Publish later in the step.)
  Labels: `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19` ("Publish"),
  `OrchardCore.Contents/Controllers/AdminController.cs:966-967` (the message).
- `set-5` (about 5.6 s). `NavigateAsync("/")`. Wait until the text `Welcome to Juniper Lane Studio` is visible.
  Visible when the narration ends: the front end, with the article's title and its body text as the home page.
- `set-6` (about 7.2 s). `NavigateAsync("/Admin/Contents/ContentItems")`, then click the Edit button of the row
  `Welcome to Juniper Lane Studio` (as in `set-2`). Highlight
  `scene.Page.Locator(".alert.alert-info", new() { HasText = "This content item is the current homepage" })`.
  Visible when the narration ends: under the Permalink field, the blue note "This content item is the current
  homepage", ringed, and no Set as homepage checkbox. Leave the editor without saving.
  Labels: `OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:61`.

After the take, check that `/` shows the article. Copy the data folder aside as S1 (section 3).

### 4.4 `05-change` (chapter 05, Change the home page): changes the site

Start page: `/Admin/Contents/ContentItems`. Start from snapshot S1 (the article is the home page).

- `change-1` (about 8.4 s). Open the editor of the Blog item with the Edit button of its row
  (`scene.Page.Locator(".list-group-item, tr").Filter(new() { Has = scene.Page.GetByRole(AriaRole.Link, new() { Name = "Blog", Exact = true }) }).Locator("a.edit").First`,
  `ClickAndWaitForNavigationAsync`). Do not click a link named "Blog" on the whole page: three links have that name
  (the left menu, the title of the row and the content type of the row), and the locator would be ambiguous. The
  other rows do not match: the blog post is not listed, and the content types of the other rows are not named "Blog".
  The editor opens and shows the Set as homepage checkbox. Click the checkbox
  (`scene.Page.Locator("input[type='checkbox'][name$='SetHomepage']")`), then click `scene.Page.Locator("button.publish")`
  (`ClickAndWaitForNavigationAsync`). The page returns to the Content Items list.
  Visible when the narration ends: the Content Items list with the message "Your Blog has been published."
  Labels as in `set-3` and `set-4`; the message uses the display name of the type ("Blog").
- `change-2` (about 11.2 s). `NavigateAsync("/")`. Wait until the Blog page is shown (the title of the blog post is
  visible). `HoldAsync(3)`.
  Visible when the narration ends: the Blog as home page, as at the start of `04-set`.
- `change-3` (about 7.6 s). `NavigateAsync("/Admin/Contents/ContentItems")`, click the Edit button of the row
  `Welcome to Juniper Lane Studio`, then highlight the Set as homepage checkbox row (as in `set-3`). Leave the editor
  without saving.
  Visible when the narration ends: the editor of the article with the checkbox "Set as homepage" ringed, and no blue
  note.

After the take, `/` shows the Blog again.

### 4.5 `06-permission` (chapter 06, Who can set it): read only, saves nothing

Start page: `/Admin`. Before the capture, open the admin menu group Access Control so the item Roles is shown
(`OrchardCore.Roles/AdminMenu.cs:18-19`).

- `permission-1` (about 10.4 s). Click the menu item `Roles` (`ClickAndWaitForNavigationAsync`): `/Admin/Roles/Index` opens (`OrchardCore.Roles/Controllers/AdminController.cs:22`).
  After 3 s, highlight the row of the role Administrator
  (`scene.Page.Locator(".list-group-item, tr").Filter(new() { HasText = "Administrator" }).First`).
  Visible when the narration ends: the list of roles (Administrator, Anonymous, Authenticated, Author, Contributor,
  Editor, Moderator), the Administrator row ringed.
  Labels: `OrchardCore.Roles/Views/Admin/Index.cshtml` (page "Roles").
- `permission-2` (about 10.4 s). `ClearHighlightsAsync()`. Click the Edit button of the Editor row
  (`scene.Page.Locator(".list-group-item, tr").Filter(new() { HasText = "Editor" }).Locator("a.edit").First`,
  `ClickAndWaitForNavigationAsync`); if it is not found, `NavigateAsync("/Admin/Roles/Edit/Editor")`. Click
  `scene.Page.Locator("#search-box")` (placeholder "Search permissions") and type `homepage`. Wait until the row
  `scene.Page.Locator("tr[data-text='Set homepage.']")` is the only permission row on screen. Highlight that row. Do not
  click the Allow checkbox and do not save.
  Visible when the narration ends: the group heading "Autoroute" with "(OrchardCore.Autoroute)", the single row
  "Set homepage." with its Allow checkbox unchecked, ringed.
  Labels: `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49` (the search box), `:57` (the group heading),
  `:119` (the row), `src/OrchardCore/OrchardCore.Autoroute.Core/AutoroutePermissions.cs:8` ("Set homepage."),
  `OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12` ("Edit").

### 4.6 Slides (`01-idea`, chapter 01)

Drawn by `slides.py`; nothing is recorded.

- `idea-1`: the slide "What the home page is", with two cards: "The address of the site" and "Home Route".
- `idea-2`: the slide "Two features, one choice", with the cards "Home Route" and "Autoroute"; Autoroute is bright.
- `idea-3`: the slide "What the checkbox needs", with three cards: a content type with the Autoroute part, the setting
  Show homepage options, and the permission Set homepage (a permission of a role; the Administrator role has it by
  default).

## 5. Checks for private data

- Only the fake data above appears: the user name `admin`, the email `admin@example.com` (if it shows), the title
  "Welcome to Juniper Lane Studio" and its text, and the content of the Blog recipe.
- No password, token or key is typed or shown. The sign-in is in `state.json`, which is never committed.
- OCR every frame (one per second, and at every scene change) and search for real names, email addresses other than
  `@example.com`, phone numbers and the path of any folder of the machine. The headless browser draws no address bar;
  if a full address with a machine name ever shows, record the clip again.
- In every frame of `03-allow`, no template text of the Pattern box (double braces, pipe characters) is visible: the
  init script of section 1 must have been active. An admin video never shows code.
- Nothing from this recording is committed: no MP4, audio, image, `state.json` or copy of the site's data folder.

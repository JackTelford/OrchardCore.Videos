# Recording plan: Create a Theme From an HTML Template

This plan is for the person who records the video. It needs no decision: every site setting, every click, every wait
and every thing that must be on screen when a step ends is written here. A selector below stands for
`scene.Page.Locator(...)`.

The video has 10 chapters. Only two of them are recordings:

| Clip | Chapter | Steps | Screens |
| --- | --- | --- | --- |
| `05-zones` | 05 Zones | `zones-1`, `zones-2`, `zones-3` | Settings, Zones; Design, Widgets; the footer of the front end |
| `08-result` | 08 Use the theme | `result-1`, `result-2`, `result-3` | Design, Themes; the front end; a post |

The other chapters are cards and slides (see "Slides and cards" at the end): they need no site.

This video has no theme project to build on camera. The code of the video is on slides, and the screens show
**TheBlogTheme** (the port of the Clean Blog template that ships with Orchard Core) on a site set up with its
Blog recipe, because the narration says so.

## The site it needs

Nothing here is recorded.

1. **Source.** A checkout of `OrchardCMS/OrchardCore` (the commit the video was written from is `92db4a83`, any later
   `main` with the same screens is fine). Build `src/OrchardCore.Cms.Web`.
2. **Run it** on `http://localhost:5010` with its own `App_Data` folder outside both repositories, provisioned by
   Auto Setup, so no setup or sign-in form is ever recorded:

   ```bash
   export ORCHARD_APP_DATA=<an empty folder outside both repositories>
   export OrchardCore__AutoSetup__AutoSetupPath=
   export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
   export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
   export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
   export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
   export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
   export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
   export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
   export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
   export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9   # the site never calls an outside service
   dotnet run --project src/OrchardCore.Cms.Web --no-build --urls http://localhost:5010
   ```

   `<password>` is the password chosen outside git; the recorder reads it from `VIDEO_PASSWORD`. Request `/` once
   and wait until the console says the site was provisioned.
3. **Recipe, theme, tenant.** The Blog recipe (`blog.recipe.json` in `src/OrchardCore.Themes/TheBlogTheme/Recipes`)
   makes TheBlogTheme the site theme and TheAdmin the admin theme (its `themes` step, lines 82 to 86). The tenant is
   `Default`, in the time zone `America/Los_Angeles`, named "Orchard Core" (the name appears in the navigation bar of
   the front end).
4. **Features to enable.** None. The Blog recipe enables Layers, Widgets, Menu, Themes, Templates, Settings and
   everything else the clips show (`feature` step, lines 30 to 81).
5. **Settings.** None to change. Leave the Resources settings as the recipe leaves them (no CDN for local files), so
   the icons of the front end come from the site.
6. **External services.** None is needed, so there is no stand-in. Two things on the front end are fetched by the
   *browser* from the internet, not by the site: the two Google Fonts stylesheets of
   `TheBlogTheme/Views/Layout.liquid` (lines 12 and 13) and the Bootstrap script bundle from `cdn.jsdelivr.net`
   (`ResourceManagementOptionsConfiguration.cs` of TheBlogTheme). Without internet access the page uses fallback
   fonts, and the navigation bar does not collapse; neither changes what the narration says. Pick one way and keep
   it for every take of the video.
7. **Seed data.** All of it comes from the Blog recipe, and all of it is fake. Create nothing by hand. It must look
   like this when the clips start:
   - A menu "Main Menu" with the items "Home" and "About" (the navigation bar of the front end).
   - A blog "Blog" at the home page `/`, with one post, "Man must explore, and this is exploration at its greatest",
     at `/blog/post-1`, written by the user `admin`, with the tags Earth, Exploration and Space and the category
     Travel.
   - An article "About" at `/about`.
   - A widget "Footer" (type Raw Html) in the layer "Always", zone "Footer", with three round icons and the line
     "Copyright © Your Website 2021".
   - The zones "Content" and "Footer" (`settings` step of the recipe, `LayerSettings`).
8. **Sign in once.** Run the recorder's `setup` command with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`.
   It writes `state.json`, which is git-ignored: never commit it. The pages of the front end need no sign-in, and the
   layout of TheBlogTheme shows nothing that depends on the user.
9. **Snapshot.** Stop the site, copy its `App_Data` folder and `state.json` aside, and start the site again.

## Order and dependencies

- Record `05-zones`, then `08-result`. They do not depend on each other.
- **No clip changes the site.** Every step only opens pages and highlights elements. Never press Save, Make Current,
  Enable, Disable, Reset Site Theme or Reset Admin Theme. A new take therefore needs no restored snapshot; if a button
  was pressed by mistake, restore the snapshot from step 9 above before the next take.
- Run `narrate.py` before recording, so the recorder holds each step for the length of its narration.

## Clip `05-zones`

**Starts on:** `/Admin` (the admin home, opened with `scene.OpenAsync("/Admin")`, not recorded). The admin menu is
on the left. Leave every group of the admin menu collapsed: opening Settings and Design is part of the story. The
default menu behavior ("Focused", `AdminSettings.cs:27-29`) opens only the group that holds the current page, and
opening a group closes the others, so no group needs to be closed by hand.

**Source of the labels:** "Settings" is the top-level group of `OrchardCore.Settings/AdminMenu.cs:36`, and "Zones" is
added to it by `OrchardCore.Layers/AdminMenu.cs:33-34`. The groups "Settings" and "Design" have no page of their own,
so each label is a button that opens the group (`TheAdmin/Views/NavigationItemLink-admin.cshtml:24-36`); the
separate "Toggle {name}" button exists only for an item that links to a page (`NavigationItem-admin.cshtml:107-124`).
"Design" and "Widgets" are in `OrchardCore.Layers/AdminMenu.cs:26-27`. The field and its hint are in
`OrchardCore.Layers/Views/LayerSettings.Edit.cshtml:4-12`. The page "Widgets and Layers" is
`OrchardCore.Layers/Views/Admin/Index.cshtml:20-47`.

### `zones-1`

- **Actions.** Click the label "Settings" of the admin menu (`ul#adminMenu button.nav-group-label:has-text("Settings")`).
  Wait until the children of Settings are visible (`ul#adminMenu a.item-label:has-text("Zones")` and the others,
  "General" and "Debugging"). Click "Zones" with `ClickAndWaitForNavigationAsync`. Wait until the address ends with
  `/Admin/Settings/zones` and the label "Available zones for Layers" is visible. Then `HighlightAsync` the text box
  under that label
  (`div.ocat-wrapper:has(label:has-text("Available zones for Layers")) input[type=text]`).
- **On screen when the narration ends.** The page "Settings" with the field "Available zones for Layers" holding
  `Content, Footer`, ringed in green, and the hint below it ("A comma separated list of values, representing the
  available zones for widgets. Note that you'll need to render these zones in the layout template, as they do not
  automatically render.").
- **Do not** click the Save button of the page.

### `zones-2`

- **Actions.** `ClearHighlightsAsync()`. Click the label "Design" of the admin menu
  (`ul#adminMenu button.nav-group-label:has-text("Design")`; opening it closes Settings). Wait until
  `ul#adminMenu a.item-label:has-text("Widgets")` is visible, then click it with `ClickAndWaitForNavigationAsync`.
  Wait until the heading "Zones" (`h3`) and two cards are visible. Highlight the card header "Content" (`div.card-header:has-text("Content")`) while
  the narration says Content, then, at "Footer zone", clear that highlight and highlight the card header "Footer"
  (`div.card-header:has-text("Footer")`).
- **On screen when the narration ends.** The page titled "Widgets and Layers" with the heading "Zones" and two cards,
  "Content" and "Footer". The "Footer" card header is ringed and its card lists one widget named "Footer" with the
  badge "Always". The right column shows the heading "Layers" with the layers "Always" and "Homepage".
- **Do not** drag a widget or click "Add Widget".

### `zones-3`

- **Actions.** `ClearHighlightsAsync()`. `NavigateAsync("/")` (the home page of the front end). Wait until
  `footer.border-top` exists. Scroll to the bottom with `ScrollAsync(0, 3000)` and wait until the footer is fully in
  view. `HighlightAsync(footer.border-top)`.
- **On screen when the narration ends.** The bottom of the front end: the footer of TheBlogTheme with its three round
  icons and "Copyright © Your Website 2021", ringed in green.
- **Source:** the footer is `render_section "Footer"` in `TheBlogTheme/Views/Layout.liquid:44-46`; the widget is the
  item with the display text "Footer" in the `content` step of `blog.recipe.json` (line 1091), and its markup is
  `Recipes/Snippets/footer.html`.

## Clip `08-result`

**Starts on:** `/Admin` (opened with `scene.OpenAsync("/Admin")`, not recorded), with every admin menu group collapsed.

**Source of the labels:** "Design" and "Themes" are in `OrchardCore.Themes/AdminMenu.cs:18-21`; the button "Make Current" is
`OrchardCore.Themes/Views/Items/ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7`; the text "This is the current Site
theme" is `ThemeEntry-Current.SummaryAdmin.cshtml:13`; the card title is the `Name` of the theme manifest,
"The Blog Theme" (`TheBlogTheme/Manifest.cs:5`).

### `result-1`

- **Actions.** Click the label "Design" of the admin menu (`ul#adminMenu button.nav-group-label:has-text("Design")`).
  Wait until `ul#adminMenu a.item-label:has-text("Themes")` is visible, then click it with
  `ClickAndWaitForNavigationAsync`. Wait until the heading "Themes" and the cards of the themes are visible.
  Highlight the first "Make Current" button that is visible (`button:has-text("Make Current")`, first match, on a card
  that is not the current one) while the narration says "Make Current". Then clear that highlight and highlight the
  text "This is the current Site theme" (`span:has-text("This is the current Site theme")`) on the card titled
  "The Blog Theme".
- **On screen when the narration ends.** The page "Themes" with the card "The Blog Theme" (description "A theme
  adapted for blogs.", tags Blog, Bootstrap, Liquid) showing "This is the current Site theme" ringed in green, and
  other theme cards around it.
- **Do not** click "Make Current", "Enable" or "Disable", and do not scroll to the red area "Reset current themes".

### `result-2`

- **Actions.** `ClearHighlightsAsync()`. `NavigateAsync("/")`. Wait until `.post-preview` exists. Hold the top of the
  page for the first sentence (header image and navigation bar visible), move the cursor along the links "Home" and
  "About" of the navigation bar, `ScrollAsync(0, 600)` to bring the post list into view, then `ScrollAsync(0, 3000)`
  to reach the footer. Keep the whole sequence shorter than the narration.
- **On screen when the narration ends.** The footer of the front end at the bottom of the page. During the step the
  frames show, in this order: the masthead with the title of the Blog and the navigation bar with the site name
  "Orchard Core" and the links "Home" and "About"; the post preview "Man must explore, and this is exploration at its
  greatest" with its subtitle, "Posted by admin on" and a date, and the tags; the footer.
- **Source:** `TheBlogTheme/Views/Content-Blog.liquid`, `Content-BlogPost.Summary.liquid:1-31`, `Layout.liquid:23-46`.

### `result-3`

- **Actions.** `ScrollAsync(0, -3000)` back to the top. Click the post title (`h2.post-title`) with
  `ClickAndWaitForNavigationAsync`. Wait until `header.masthead .post-heading` exists. `HighlightAsync(.post-heading)`
  for the first sentence. `ClearHighlightsAsync()`, then `ScrollAsync(0, 700)` so the first paragraphs of the body are
  in view while the narration reaches "the body of the post renders below it".
- **On screen when the narration ends.** The page `/blog/post-1`: the masthead above, scrolled so that its lower part
  and the first paragraphs of the post body are both visible.
- **Source:** `TheBlogTheme/Views/Content-BlogPost.liquid:14-24` (title, subtitle, author, date) and `:30` (the body).

## Slides and cards

These steps have no recording. Each line says what the slide shows (drawn by `slides.py`).

- `intro`: the opening card, "Create a Theme From an HTML Template".
- `why-1`: three cards, "HTML pages", "Stylesheet and scripts" and "Images".
- `why-2`: three cards, "The template" (Clean Blog), "The theme" (TheBlogTheme) and "Its source code".
- `why-3`: three cards, "A CMS application", "An empty theme project" and "The HTML template".
- `why-4`: four cards, "Copy the assets", "Declare the resources", "Build the layout" and "Template the content".
- `assets-1`: the file tree of the theme `MyTheme`, with the folders copied from the template highlighted, and three cards.
- `assets-2`: the same tree with `css/` and `styles.css` highlighted, and the cards "In the project" and "In the browser".
- `res-1`: four cards on why resources are named: version hashes, minified and debug files, CDN addresses, dependency order.
- `res-2`: the class `ResourceManagementOptionsConfiguration`, with its two `using` lines, and `DefineStyle`, `DefineScript` and `SetUrl` highlighted.
- `res-3`: the `Startup` class, with its two `using` lines, registering the class with `AddResourceConfiguration`, and a card.
- `res-4`: the resource of TheBlogTheme with `SetDependencies` between two `// ...` lines, and a card.
- `layout-1`: the trimmed `index.html` of the template (the `head`, the navigation bar with one menu item, the footer and the script tag), and three cards ("Keep the markup", "Replace what is hardcoded", "Add zones").
- `layout-2`: the head of `Layout.liquid` with the `style` and `script` tags highlighted.
- `layout-3`: the same head with the four `resources` tags highlighted.
- `layout-4`: the body of `Layout.liquid` with the `menu` shape and `render_body` highlighted.
- `layout-5`: the same body with the three `render_section` zones highlighted.
- `layout-6`: the Razor version of the layout.
- `menu-1`: `Menu.liquid` and `MenuItem.liquid` of TheBlogTheme.
- `content-1`: four cards of template file names, all bright.
- `content-2`: the same four cards, the two `Content-BlogPost` names bright.
- `content-3`: the post heading of the template above the same heading in `Content-BlogPost.liquid`.
- `content-4`: `Content-Article.liquid` with `shape_render`, and two cards ("Model.Content", "shape_render").
- `content-5`: a `placement.json` rule that hides `TitlePart`, and two cards.
- `content-6`: two cards, "Templates reference" ("Template names, and the properties each template receives.") and "TheBlogTheme".
- `recipe-1`: `blog.recipe.json` with the `feature`, `themes` and `settings` steps highlighted (the steps between them are shown as `// ...`).
- `recipe-2`: the same file with its first lines (`name`, `displayName`, `description`, `issetuprecipe`) highlighted.
- `summary-1`: four cards that recap the chapters.
- `outro`: the closing card, subtitle "docs.orchardcore.net".

## Checks for private data

Before the video is published, OCR it at one frame per second and at every scene change, then at five frames per
second, and search the text for:

- Any name of a person, any email address other than `admin@example.com`, any phone number, any token or key.
  The only user name that may appear is `admin`, in "Posted by admin" and in the user menu of the admin.
- The password: it is never typed on camera, and it must not appear in any URL, field or frame.
- A machine name, user folder or drive path (the address bar is not recorded, but check every page). Only `localhost`
  addresses may appear.
- Commit nothing but text: no MP4, audio, `state.json`, mask script or copy of `App_Data`.

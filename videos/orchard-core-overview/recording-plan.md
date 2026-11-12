# Recording plan: Orchard Core overview

How to record every clip of this video without deciding anything. A selector written in a step stands for
`scene.Page.Locator(...)`. Labels are spelled as the Orchard Core source spells them; every one has its file and line
in the `src/` tree of `OrchardCMS/OrchardCore` (the catalog was read at commit 92db4a83, see `catalog/SOURCE.md`). Paths
below start at `src/`. Questions this plan cannot settle from the source are in `OPEN-QUESTIONS.md`: read it first.

The video has five recordings (`02-tour`, `03-items`, `03-types`, `04-design`, `05-features`), five slide clips drawn by
`slides.py` (`01-what`, `05-recipes`, `06-tenants`, `07-build`, `08-next`) and the two cards. All the recordings only
look at the admin and the front end of one site. The multi-tenancy chapter is slides only: no second tenant is needed.

## 1. The site it needs

One fresh site, with the sample content of the Blog recipe and nothing else. It is audience "admin": nothing is
configured, written or shown in code.

| | |
|---|---|
| Orchard Core | A build of `src/OrchardCore.Cms.Web` from `OrchardCMS/OrchardCore` at or after commit 92db4a83 (the catalog commit). Do not run the recorder against another host project. |
| Address | `http://localhost:5010` (`VIDEO_BASE_URL`), a free port. |
| `App_Data` | A new, empty folder outside both repositories (`ORCHARD_APP_DATA`). It is never committed. |
| Provisioning | Auto Setup (the host project registers it: `OrchardCore.Cms.Web/Program.cs:9`), through environment variables, so no Setup screen and no sign-in form is recorded. |
| Outbound calls | Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address for the site process, so the server calls no external service. The browser may still ask an external font service for the Blog theme fonts (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12-13`): record online so the front end shows its fonts, or accept the fallback fonts. Nothing private is sent either way. |

Auto Setup values (names read from `OrchardCore.Modules/OrchardCore.AutoSetup/Options/TenantSetupOptions.cs:21-81`; the
section is `OrchardCore:AutoSetup`, written `OrchardCore__AutoSetup__...` in the environment):

```bash
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"   # <password>, chosen outside git
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

- **Recipe**: `Blog` (display name "Blog", `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). The narration says
  "this blog came from the Blog recipe", so no other recipe will do.
- **Language**: English (en-US), the recorder's default. The narration names the English labels.
- **Features to enable before recording**: none. The Blog recipe already enables Content Preview, Layers, Widgets,
  Templates, Menu, Media, Deployment and Remote Deployment (`blog.recipe.json:25-75`).
- **Features that must stay disabled**: Workflows (all its features) and Tenants. `features-2` shows Workflows as
  disabled; the recipe does not enable it (`blog.recipe.json:25-75` has no Workflows entry).
- **Settings**: none changed. Do not touch Settings, Themes, or the menu.
- **Fake seed data**: none to create. The site holds only what the Blog recipe creates (`blog.recipe.json`): the Blog
  "Blog" (path `blog`, the home page), one Blog Post ("Man must explore, and this is exploration at its greatest"),
  the Article "About" (path `about`), the Menu "Main Menu" with the links Home and About, the Taxonomies Tags and
  Categories, the widget "Footer" (layer Always, zone Footer), the layers Always and Homepage, the zones Content and
  Footer, and the roles Moderator, Editor, Author and Contributor. Do not add, edit or delete any of it.
- **External services**: none, so no local stand-in.
- **Sign-in**: run `Recorder setup` once with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`. The session is
  saved in `state.json` in the video folder (git-ignored, never committed). Every clip below runs signed in as `admin`.
- **Mask script**: none (`VIDEO_INIT_SCRIPT` unset). The site is new and holds no real name.
- **Window**: the recorder's defaults (1536x864 CSS pixels, scale 1.25, light color scheme; `Scene.cs:105-111`). The admin
  menu is the default one: `AdminSettings.MenuBehavior` is `Focused` and the lists use the "List" layout with "Buttons"
  actions (`OrchardCore.Admin.Abstractions/Models/AdminSettings.cs`, `AdminListOptions.cs:30,36`). Change none of it.

Build and start, then narrate and record. The recorder is built with the scenes of this video: `recorder/Scenes.cs` in
the video folder, with one method per recording, listed under its clip id (`02-tour`, ...) in the `s_clips` table, and one
`scene.StepAsync` per step id below, written from this plan before the build.

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/orchard-core-overview/recorder
export VIDEO_ROOT=<repo>/videos/orchard-core-overview VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
export MSYS_NO_PATHCONV=1
<bin>/Recorder setup
python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/orchard-core-overview
<bin>/Recorder record all
```

## 2. Order, and what changes the site

No clip changes the site: they only open pages, and the narration never saves anything. The five recordings do not
depend on each other, so they can be recorded in any order, one at a time; the storyboard order is
`02-tour`, `03-items`, `03-types`, `04-design`, `05-features`.

Never click **Publish**, **Unpublish**, **Save Draft**, **Make Current**, **Enable**, **Disable**, **Remove**,
**Delete**, or the layout selector (List, Grid) of a list during a take: each would change the site or the way its
lists look, and the next clips expect it as the recipe left it. After setup, copy the `App_Data`
folder and `state.json` aside anyway; if a click slips, stop the site, restore the copy, start it again. A restore is
the only "reset" this video needs.

Common to every recording:

- `OpenAsync` the start page, wait for the condition in "Starts on", then `StartCaptureAsync`.
- On `/Admin` every group of the left menu is collapsed, and every clip starts there (or on the front end) in a new
  browser context. On any other admin page the menu opens the group that holds the current page, and, because the menu
  is `Focused`, opening a group closes the other groups at the same level
  (`TheAdmin/Assets/js/TheAdmin/sidebar.ts`, `applyNavStateFromPreferences` and `applyFocusedAccordion`). So the
  "Design group is already open" of `design-2` is true because the Themes page, which the previous step loaded, is under
  Design. Where a step expands a group, the click is part of the story and is made on camera; click a group only when its
  `aria-expanded` is `false`.
- Left-menu entries are found inside `#left-nav`. A group (Content, Design, Tools, Content Definition) has no page of
  its own, so it is a button that expands it
  (`TheAdmin/Views/NavigationItemLink-admin.cshtml:23-24,29-38`, `TheAdmin/Views/NavigationItem-admin.cshtml`): click it,
  then click the entry inside. A page entry is a link.
- Keep each step's actions shorter than its narration. A highlight is `HighlightAsync(locator)` with no caption,
  and `ClearHighlightsAsync()` before any scroll or navigation (highlights are fixed-position).

## 3. Recordings

### `02-tour` (chapter 2, "A site and its admin")

Starts on `/` (the home page of the site, the Blog), signed in. Wait until the heading in `header.masthead` is visible
and the post summaries are in the page. Menu: not applicable on the front end.

- `tour-1`: the front end.
  - Actions: highlight the navbar link Home (`#mainNav a >> text="Home"`), then About (`#mainNav a >> text="About"`),
    one second each. Clear the highlights. Scroll down 600 px (`ScrollAsync(0, 600)`) so the first post summary is in
    the window.
  - Visible when the narration ends: the blog's navbar with Home and About scrolled out or at the top, and a post
    summary ("Man must explore, and this is exploration at its greatest"). No error, no admin bar.
  - Labels: menu items Home and About, `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:834,843` (the recipe creates one Blog Post); navbar
    `OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:27,32`; the home page is the Blog item
    (`OrchardCore.Themes/TheBlogTheme/Views/Content-Blog.liquid`, `Blog-ListPart.liquid`).
- `tour-2`: the admin welcome page.
  - Actions: `NavigateAsync("/Admin")`. Wait until the heading "Welcome to Orchard Core" is visible. Highlight, in this
    order, the card titles "Create content", "Customize theme" and "Manage users"
    (`.card-title >> text="Create content"` and so on), one second each.
  - Visible when the narration ends: the welcome message and the four cards: Create content, Customize theme, Manage
    users, Learn more, with the left menu, all its groups collapsed, at the side. The order of the cards is set by the
    positions 10, 20, 30 and 40 in the `AdminDashboardShapeTableProvider.cs` of Contents, Themes, Users and Admin.
  - Labels: `OrchardCore.Modules/OrchardCore.Admin/Views/AdminDashboard.cshtml:2`;
    `OrchardCore.Contents/Views/AdminDashboardContents.cshtml:4`;
    `OrchardCore.Themes/Views/AdminDashboardThemes.cshtml:4`;
    `OrchardCore.Users/Views/AdminDashboardUsers.cshtml:4`;
    `OrchardCore.Admin/Views/AdminDashboardDocumentation.cshtml:4` (paths under `OrchardCore.Modules/`).
- `tour-3`: the left menu.
  - Actions: clear the highlights. Highlight, in this order, the menu entries Content (`#left-nav >> text="Content"`),
    Design (`#left-nav >> text="Design"`) and Tools (`#left-nav >> text="Tools"`), two seconds each. Do not click them.
  - Visible when the narration ends: the left menu with its groups collapsed and Tools highlighted.
  - Labels: Content `OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65`; Design
    `OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23` and `OrchardCore.Themes/AdminMenu.cs`; Tools
    `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26`. Under Design the narration names Themes, Widgets and
    Content Types; under Tools it names Features and Recipes (`OrchardCore.Recipes/AdminMenu.cs`).

### `03-items` (chapter 3, "Content items and content types")

Starts on `/Admin` (the welcome page), signed in. Wait until "Welcome to Orchard Core" is visible.

- `items-1`: the Manage Content page.
  - Actions: click the group Content (`#left-nav >> text="Content"`), then
    `ClickAndWaitForNavigationAsync(#left-nav a >> text="Content Items")`. Wait until the heading "Manage Content" and
    the row "About" are visible. At the end of the narration highlight the row of About (the list row that holds the Edit
    button of the Article About, `li.list-group-item:has-text("About")`; no other row contains that word).
  - Visible when the narration ends: the Manage Content page with the search box, the rows (Blog, About, and the other
    listable items) and, on the About row, the type, the status "Published" and the modified time.
  - Labels: Content Items `OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:68`; the page title "Manage Content"
    `Views/Admin/List.cshtml:17`; status "Published" and "Draft" `Views/ContentsTags_SummaryAdmin.cshtml:12,19`; type
    badge `Views/ContentsType_SummaryAdmin.cshtml:6`; modified time `Views/ContentsMeta_SummaryAdmin.cshtml` (all under
    `OrchardCore.Modules/OrchardCore.Contents/`). The default list layout is "List", so the column titles of the Grid
    layout are not on screen; the narration only names what a row shows (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`).
    The type badge shows the technical name of the type (`contentItem.ContentType`), here Article. The list also offers a
    layout selector (`AllowUserListLayoutSelection` is on by default): leave it alone.
- `items-2`: the editor.
  - Actions: `ClickAndWaitForNavigationAsync` on the Edit button of the About row
    (`li.list-group-item:has-text("About") a.edit`). Wait until the field with the label Title shows "About". Highlight,
    in this order, the label Title, the label Permalink and the label HtmlBody (the editor under the Permalink, with the
    hint "The body of the content item."), one second each.
  - Visible when the narration ends: the editor of About: the title "About", the permalink "about" and the top of the
    HtmlBody editor, under the breadcrumb "Edit Article".
  - Labels: button Edit `Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`; breadcrumb "Edit {0}" `Views/Admin/Edit.cshtml:39`;
    Title `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`; Permalink
    `OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`; HtmlBody and its hint
    `OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml:22,34` (the label is the display name of the part, which is
    its name without "Part": `OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentTypePartExtensions.cs:7-26`
    and `Settings/ContentPartSettingsExtensions.cs:54-64`; the Article type sets the editor Wysiwyg in `blog.recipe.json`,
    part HtmlBodyPart) (paths under `OrchardCore.Modules/` unless given). The Article type also has the fields Subtitle
    and Banner Image (`blog.recipe.json`, part Article), below the body.
- `items-3`: the action bar.
  - Actions: clear the highlights; scroll to the bottom of the editor (`ScrollAsync(0, 2000)`) until the action bar is
    in the window. Highlight, in this order, Publish (`button.publish`), Save Draft (`button.draft`) and Preview
    (`a#previewButton`), two seconds each. Do not click any of them. The page was opened from the list, so it has a return address and Publish and Save Draft are
    split buttons with a menu arrow (`Content.PublishButton.cshtml:11-26`); `button.publish` and `button.draft` are the main
    buttons, the menu items have other classes. About is published, so Unpublish and Delete are in the bar too
    (`Drivers/ContentsDriver.cs:112-120`): do not click them.
  - Visible when the narration ends: the action bar of the editor with Publish, Save Draft and Preview, and the
    highlight on Preview.
  - Labels: Publish `Views/Content.PublishButton.cshtml:7,16`; Save Draft `Views/Content.SaveDraftButton.cshtml:7,16`;
    the buttons are added by `Drivers/ContentsDriver.cs:110-115` (Contents module); Preview
    `OrchardCore.ContentPreview/Views/ContentPreview.Button.cshtml:13`, a link that opens a new tab (not followed in this
    video). Preview is part of the Content Preview feature, enabled by the recipe (`blog.recipe.json:53`).
  - Leave the page by ending the clip. Nothing was changed, so nothing is saved.

### `03-types` (chapter 3, second clip)

Starts on `/Admin`, signed in. Wait until "Welcome to Orchard Core" is visible.

- `types-1`: the Content Types list.
  - Actions: click the group Design (`#left-nav >> text="Design"`), click the group Content Definition
    (`#left-nav >> text="Content Definition"`), then
    `ClickAndWaitForNavigationAsync(#left-nav a >> text="Content Types")`. Wait until the heading "Content Types" and the
    row Article are visible. Highlight, in this order, the rows of the types Article, Blog and Page, found by the Edit
    button each row holds (`li.list-group-item:has(a[role='btn-edit-Article'])`, `...btn-edit-Blog`, `...btn-edit-Page`;
    a text match on "Blog" would also catch the row of Blog Post), one and a half seconds each.
  - Visible when the narration ends: the Content Types list with the search box, "Create new type", and the rows
    Article, Blog and Page.
  - Labels: Design > Content Definition > Content Types
    `OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25`; the page `Views/Admin/List.cshtml:6` and the button "Create new type" `:20`. The
    display names Article, Blog and Page are in `blog.recipe.json` (ContentDefinition step).
- `types-2`: the editor of the Article type.
  - Actions: clear the highlights. `ClickAndWaitForNavigationAsync(a[role='btn-edit-Article'])` (the Edit button of the
    Article row: `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`). Wait until the breadcrumb "Edit Content Type - Article"
    is visible. Scroll down in steps of 300 px until the heading Fields is at the top of the window and the heading Parts
    is still below it or just visible. Highlight, in this order, the field rows Subtitle and Banner Image (`ul#fields > li` first and
    second, `Views/Admin/Edit.cshtml:90`), then the part rows Title, Autoroute and HtmlBody (`ul#parts > li:nth-child(1)`,
    `:nth-child(2)` and `:nth-child(3)`, `:146`), two seconds each.
  - Visible when the narration ends: the headings Fields (with the rows Subtitle and Banner Image) and Parts (with the
    rows Title, Autoroute, HtmlBody and Article, in this order: positions 0, 1, 2 and 3 in `blog.recipe.json`). Each
    row except Article has a dashed hint: "Provides a Title for your content item.", "Provides a custom url for your
    content item." and "Provides an HTML Body for your content item."; the row Article says "Contains the fields for the
    current type". The HtmlBody row also carries a badge "Wysiwyg", the editor set for that part in the recipe
    (`Views/Admin/Edit.cshtml:187-189`); it is part of the page, not an error.
  - Labels: breadcrumb `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`; headings Fields `:88` and Parts `:144`; the
    hint "Contains the fields for the current type" `:180`. The field names Subtitle and Banner Image are the
    DisplayName settings in the recipe (`blog.recipe.json`, part Article). The part labels are the display names of the
    parts, their names without "Part" (Title, Autoroute, HtmlBody: `ContentTypePartExtensions.cs:7-26`,
    `ContentPartSettingsExtensions.cs:54-64`, `Edit.cshtml:153`; no module sets another display name for them), and the
    hints are the descriptions set by `OrchardCore.Title/Migrations.cs:33`, `OrchardCore.Autoroute/Migrations.cs:23`
    and `OrchardCore.Html/Migrations.cs:34`. The narration says "Autoroute for the permalink" because the Autoroute part
    shows the field Permalink (`AutoroutePart.Edit.cshtml:30`).
- `types-3`: the buttons.
  - Actions: clear the highlights. Scroll so that the button Add Field (under the Fields list) is in the window,
    highlight it; then scroll to the button Add Parts (under the Parts list) and highlight it, two seconds each. Do
    not click either.
  - Visible when the narration ends: the button Add Parts highlighted, with Add Named Part next to it.
  - Labels: Add Field `Views/Admin/Edit.cshtml:137`; Add Parts `:201`; Add Named Part `:202`
    (`OrchardCore.Modules/OrchardCore.ContentTypes/`).

### `04-design` (chapter 4, "Themes, widgets and layers")

Starts on `/Admin`, signed in. Wait until "Welcome to Orchard Core" is visible.

- `design-1`: the Themes page.
  - Actions: click the group Design, then `ClickAndWaitForNavigationAsync(#left-nav a >> text="Themes")`. Wait until the
    heading Themes and the card "The Blog Theme" are visible. Highlight, in this order, the text "This is the current
    Site theme" on the card "The Blog Theme", the text "This is the current Admin theme" on the card "The Admin Theme",
    and the first button "Make Current" (scroll down 300 px if no card with that button is in the window). Do not
    click it.
  - Visible when the narration ends: the theme cards with "Make Current" highlighted on a card that is not current.
  - Labels: Themes `OrchardCore.Modules/OrchardCore.Themes/AdminMenu.cs:21`; "This is the current Site theme"
    `Views/Items/ThemeEntry-Current.SummaryAdmin.cshtml:13`; "This is the current Admin theme" `:8`; "Make Current"
    `Views/Items/ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7` (the button is on every card that is not current:
    `Drivers/ThemeEntryDisplayDriver.cs:19-27`); the theme names "The Blog Theme"
    (`OrchardCore.Themes/TheBlogTheme/Manifest.cs:5`) and "The Admin Theme" (`OrchardCore.Themes/TheAdmin/Manifest.cs:5`).
- `design-2`: the Widgets and Layers page, zones.
  - Actions: clear the highlights. `ClickAndWaitForNavigationAsync(#left-nav a >> text="Widgets")` (the Design group is
    already open). Wait until the heading "Widgets and Layers" and the heading Zones are visible. Highlight, in this
    order, the zone cards with the header Content and the header Footer (`div.card:has(.layer-zone[data-zone="Content"])`,
    `div.card:has(.layer-zone[data-zone="Footer"])`), then the widget "Footer" in the Footer card
    (`.layer-zone[data-zone="Footer"] li.list-group-item`), two seconds each.
  - Visible when the narration ends: the Zones column with the cards Content and Footer, the Content card holding no
    widget, and the widget Footer with the badge Always inside the Footer card (`Views/Admin/Index.cshtml:46-70`).
  - Labels: Widgets `OrchardCore.Modules/OrchardCore.Layers/AdminMenu.cs:27`; page title "Widgets and Layers"
    `Views/Admin/Index.cshtml:20`; heading Zones `:34`; the zones Content and Footer `blog.recipe.json:121-124`; the
    widget Footer, layer Always, zone Footer `blog.recipe.json:1097-1098`.
- `design-3`: the layers.
  - Actions: clear the highlights. Highlight, in this order, the layer Always and the layer Homepage in the Layers
    column on the right (`li.list-group-item:has(input#Always)`, `li.list-group-item:has(input#Homepage)`: the checkbox
    of a layer has its name as id, `Layer.Checkbox.SummaryAdmin.cshtml:8`; do not use a text match, because the widget
    Footer in the Zones column is also a `li.list-group-item` that carries the badge "Always"), three seconds each. Do
    not select a layer: the checkbox only highlights the widgets of the layer.
  - Visible when the narration ends: the Layers column with Always and Homepage, the Homepage row highlighted.
  - Labels: heading Layers `Views/Admin/Index.cshtml:121`; the layers and their descriptions ("The widgets in this layer
    are displayed on any page of this site." and "...only displayed on the homepage.") `blog.recipe.json:1144-1171`.

### `05-features` (chapter 5, "Features and recipes", first clip)

Starts on `/Admin`, signed in. Wait until "Welcome to Orchard Core" is visible.

- `features-1`: the Features page.
  - Actions: click the group Tools (`#left-nav >> text="Tools"`), then `ClickAndWaitForNavigationAsync(#left-nav a >> text="Features")`.
    Wait until the heading Features and the line "Showing ... of ... features" are visible. Highlight, in this
    order, the first category heading that is on screen (`.feature-group:visible h3`, first match: a category whose rows
    are all hidden by the default filters is hidden with them, `Views/Admin/Features.cshtml:85-87`), and then the
    description and the dependency badges of the first visible feature row that has dependency badges
    (`.feature-group li:visible` that holds at least one `.direct-dependencies .badge`, first match), two seconds each.
  - Visible when the narration ends: the Features page with the search box, the category heading and a row showing its
    description and the badges of the features it depends on.
  - Labels: Tools > Features `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; page `Views/Admin/Features.cshtml:5,27,57,88`;
    descriptions `Views/FeatureEntry.Description.SummaryAdmin.cshtml`; dependency badges
    `Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml`. The page lists only features the default filters show
    ("Hide on demand", "Hide always enabled" are selected: `Views/Admin/Features.cshtml:65-66`).
- `features-2`: the search.
  - Actions: clear the highlights. `TypeAsync(#search-box, "Workflows", delay: 60)`. Do not press Enter. Wait until the
    row "Workflows" is visible and the line "Showing ... of ... features" no longer shows the full count (the page
    filters while you type: `OrchardCore.Features/Assets/ts/features-index.ts`, the `input` handler). The list then also holds
    HTTP Workflows Activities, Session Workflows Activities and Timer Workflows Activities, which are listed
    before Workflows (rows are sorted by name), so find the row by its name, exactly: the row is
    `li[data-filter-value]:has(span.fw-semibold:text-is("Workflows"))` (`FeatureEntry.Fields.SummaryAdmin.cshtml:11`).
    Highlight, inside that row, the badge "Disabled" (`.badge:has-text("Disabled")`) and the button "Enable"
    (`a:text-is("Enable")`; its id is `btn-enable-` followed by the id of the feature written by `GenerateIdFromName`,
    most likely `btn-enable-OrchardCore_Workflows`: confirm it on the running site), two seconds each. Do not click Enable.
  - Visible when the narration ends: the filtered list, the row "Workflows" with its badge "Disabled" and the button
    "Enable" highlighted.
  - Labels: search box `Views/Admin/Features.cshtml:27`; the search matches the name, the id and the description
    (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`); "Disabled" `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`;
    "Enable" `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the feature "Workflows" (category Workflows)
    `OrchardCore.Workflows/Manifest.cs:12-18` (paths under `OrchardCore.Modules/`).

## 4. Slides and cards

Slides are drawn by `slides.py`, one image per step; nothing is recorded for them.

- Card `intro` (opening card, chapter 00): the logo, the title and the subtitle; the narration is read over it.
- `what-1`: three stacked boxes, built on each other: Orchard Core CMS on top of Orchard Core Framework on top of
  ASP.NET Core; the CMS and Framework boxes are bright.
- `what-2`: the same slide; the Framework and ASP.NET Core boxes are bright.
- `what-3`: the same slide; only the CMS box is bright.
- `recipe-1`: four cards that say what a recipe does: Enables features, Defines content types, Chooses the themes,
  Creates starting content.
- `recipe-2`: three cards about deployment plans: Deployment plan, Package, Import or send.
- `tenants-1`: one panel, "One Orchard Core application", holding three tenant cards: Default, partner and shop, each
  with an address on example.com and the recipe it was set up with.
- `tenants-2`: the same slide, with the bar "Managed from the Default tenant: Multi-Tenancy > Tenants" bright.
- `build-1`: four cards (Full CMS, Decoupled CMS, Headless CMS, Framework only); the first is bright.
- `build-2`: the same cards; Decoupled CMS and Headless CMS are bright.
- `build-3`: the same cards; Framework only is bright.
- `next-1`: four cards (Modules Reference, Module pages, Glossary, Getting started) and the address
  docs.orchardcore.net; the first two are bright.
- `next-2`: the same slide; Glossary is bright.
- `next-3`: the same slide; Getting started is bright.
- Card `outro` (closing card, chapter 99): the logo, the title and the subtitle docs.orchardcore.net.

Check each slide in the assembled video: no text is cut off, and the bright cards follow the narration.

## 5. Checks for private data

Before assembling and again before publishing, OCR the video once per second, at every scene change, and at five frames
per second (see `SKILL.md`, "Check for private data"). Expected text only: the site name "Orchard Core", the user name
`admin`, the sample content of the Blog recipe, the fake tenant names and addresses of the slides (example.com), and the
theme names. Look for, and fix by recording again:

- an email address other than `admin@example.com` (the only one expected, and it is not shown on these pages), a real
  person's name, a phone number, a token or key;
- the author name "admin" is the only name on the content list; no other user exists on the site;
- the footer of the Blog theme is the recipe's snippet: three social icons whose links are "#!" and the text
  "Copyright © Your Website 2021" (`OrchardCore.Themes/TheBlogTheme/Recipes/Snippets/footer.html`). It names no one;
- the sample post is the recipe's own text; nothing in it is private;
- the page address and any host name of the machine are not part of a recording (the browser window is not captured);
- no password is typed or shown at any point: the sign-in comes from `state.json`.

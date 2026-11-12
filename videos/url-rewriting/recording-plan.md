# Recording plan: URL Rewriting

How to record the clips of `storyboard.json` without having to decide anything. A selector written as
`#search-box` stands for `scene.Page.Locator("#search-box")`. File and line references point into
`OrchardCMS/OrchardCore` at commit `92db4a83`; `M/` stands for `src/OrchardCore.Modules/OrchardCore.UrlRewriting/`,
`C/` for `src/OrchardCore/OrchardCore.UrlRewriting.Core/` and `F/` for `src/OrchardCore.Modules/OrchardCore.Features/`.

The video has one slide clip (`01-concepts`, nothing to record) and six recorded clips: `02-enable`, `03-redirect`,
`03-redirect-test`, `04-rewrite`, `04-rewrite-test` and `05-manage`.

## The site it needs

- **Host**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- **Auto Setup** (not recorded, no setup or sign-in form on camera). Set these environment variables before the first
  request; the password is chosen outside git and is the same value as `VIDEO_PASSWORD`:

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Tenant**: the Default tenant only, without a URL prefix. The rules of the video apply to the addresses of this
  tenant.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls out.
- **Sign-in**: run `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` once, right after
  Auto Setup, and keep `state.json` (git-ignored) next to the snapshot below.
- **Features**: the Blog recipe enables the features the video relies on (Features, Media, Autoroute, Contents,
  Admin Menu: `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, step `feature`) and does **not**
  enable URL Rewriting. The starting state has URL Rewriting **disabled**: clip `02-enable` turns it on, on camera.
  Do not enable it in a `PrepareAsync`.
- **Settings**: change none. Admin lists use the defaults, the List layout with Buttons actions
  (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs`: `DefaultLayout`, `DefaultActionsLayout`).
  The narration never names the "Last modified" column, which exists only in the Grid layout.
- **External services**: none.
- **Fake seed data**: none to create. The Blog recipe provides everything the clips use:
  - the page **About** (content type Article, Autoroute path `about`, subtitle "This is what I do.") shown by
    `/about`, with a large `h1` reading "About" (`TheBlogTheme/Views/Content-Article.liquid:15`);
  - the home page, the Blog (`h1` reading "Blog"), which is the page the clips open before the capture starts;
  - the media files `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg`, served at `/media/about-bg.jpg` and so on
    (Media request path `/media`: `src/OrchardCore.Modules/OrchardCore.Media/Services/MediaOptionsConfiguration.cs:66`).
  - There is no address `/about-us` and no address `/img/...` before the rules exist. The rules are created on camera.

## Order, dependencies and snapshots

Record in this order. Each clip starts from the state the previous one left, except the two test clips, which
change nothing.

| Order | Clip | Needs | Changes the site |
|---|---|---|---|
| 1 | `02-enable` | Snapshot S0 | Enables URL Rewriting. Take snapshot S1 afterwards. |
| 2 | `03-redirect` | S1 | Adds the rule "Redirect about-us to about". Take snapshot S2 afterwards. |
| 3 | `03-redirect-test` | S2 | Nothing (front end requests only). |
| 4 | `04-rewrite` | S2 | Adds the rule "Serve media from img". Take snapshot S3 afterwards. |
| 5 | `04-rewrite-test` | S3 | Nothing (front end requests only). |
| 6 | `05-manage` | S3 | Moves the rule "Serve media from img" to the top, then deletes it. |

- **Snapshot S0**: after Auto Setup and `Recorder setup`, stop the site, copy the `App_Data` folder and `state.json`
  aside, and start the site again. A snapshot is a copy of both, taken with the site stopped. S1, S2 and S3 are taken the
  same way after the clip named above.
- **Before a new take**, stop the site, restore the snapshot named in the "Needs" column (with its `state.json`),
  start the site, and open `/` and `/Admin` once in a normal browser tab or with `Recorder probe`, so the first request
  after the start (which is slow) is not recorded.
- **Every save restarts the site.** Saving, deleting or enabling here releases the tenant, so the page that follows
  takes a few seconds. The steps below put those waits off camera (`OffCameraAsync`). At the start of every clip,
  call `scene.Page.SetDefaultNavigationTimeout(60_000)` before `StartCaptureAsync`.
- `03-redirect-test` must run before any edit or delete of the redirect rule, since a browser may cache a permanent
  redirect. Each clip opens a new browser context, so the test clips are not affected by the earlier ones.

## Locators used by several clips

- **Tools** (admin menu entry): `scene.Page.GetByText("Tools", new() { Exact = true }).First`
  (`M/AdminMenu.cs:18`, `F/AdminMenu.cs:26`).
- **URL Rewriting menu item**: `a[href$="/Admin/UrlRewriting/Index"]` (`M/AdminMenu.cs:19-23`; the item also carries
  the class `url-rewriting`, line 20).
- **Features menu item**: `a[href$="/Admin/Features"]` (`F/AdminMenu.cs:27-28`).
- **Open the menu item**: if the item is not visible, click **Tools** first, then click the item with
  `ClickAndWaitForNavigationAsync`. Do not expand the menu before `StartCaptureAsync`: opening it is part of the story.
- **Success message**: `scene.Page.GetByText("<message>")`, waited for with `WaitForAsync` until visible.
- **Rows of the rule list**: `#rewrite-rules-sortable-list .item` (`M/Views/Admin/Index.cshtml:36`,
  `M/Drivers/RewriteRulesDisplayDriver.cs:31`). A row by rule name:
  `scene.Page.Locator("#rewrite-rules-sortable-list .item").Filter(new() { HasText = "<rule name>" })`.
  In a row: drag handle `.ui-sortable-handle`, **Edit** `a.btn-primary` (text Edit), **Delete** `a.btn-danger` (text Delete).
- **Form fields** of the rule editor, by label: `scene.Page.GetByLabel("Display Name")`, `GetByLabel("Match URL Pattern")`,
  `GetByLabel("Case Insensitive")`, `GetByLabel("Query String Policy")`, `GetByLabel("Redirect Type")`,
  `GetByLabel("The Substitution URL Pattern")`, `GetByLabel("Stop Evaluation of Subsequent Rules")`.
  **Save**: `button.btn-primary` with text Save (`M/Views/Admin/Create.cshtml:14`).
- Type text with `TypeAsync` and press Tab after each value, so the field loses focus.
- **Restart alert**: the editor of a rule asks for a tenant restart when it is saved
  (`M/Drivers/RewriteRulesDisplayDriver.cs:53`, `src/OrchardCore/OrchardCore.DisplayManagement/Extensions/BuildShapeContextExtensions.cs:6`).
  The wrapper it adds, `Settings_Wrapper__Reload`, is the shape of the template `Settings-Reload.Wrapper.cshtml` of the
  Settings module (the name rule is in `src/OrchardCore/OrchardCore.DisplayManagement/Descriptors/ShapeTemplateStrategy/BasicShapeTemplateHarvester.cs`,
  method `Adjust`), and the Blog recipe enables that module. So a yellow alert, "The website might be restarted upon saving
  the settings, potentially leading to temporary unresponsiveness during the process."
  (`src/OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`), sits above the fields on the
  pages "New 'Redirect' rule", "New 'Rewrite' rule" and "Edit '...' rule". The narration does not mention it. Leave it on
  screen and do not hide it.

## Clip `02-enable`

Starts on `/Admin` (opened before `StartCaptureAsync`, menu collapsed). Snapshot S0, URL Rewriting disabled.

- `enable-1`: click **Tools**, then the **Features** menu item (`ClickAndWaitForNavigationAsync`). On the Features
  page, `TypeAsync(#search-box, "URL Rewriting")`. Wait until the row `[data-filter-value*="OrchardCore.UrlRewriting"]`
  is visible. Then `HighlightAsync` on that row, without a caption.
  *Visible at the end*: the Features page, "URL Rewriting" in the search box, the row "URL Rewriting
  OrchardCore.UrlRewriting" (description "Enables URL rewriting for incoming requests.", category Infrastructure) with a
  blue **Enable** button, inside a green ring.
  *Labels*: Tools and Features `F/AdminMenu.cs:26-27`; search placeholder `F/Views/Admin/Features.cshtml:27`; row
  `F/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`; name and description `M/Manifest.cs:4` and `:8`, category `:9`.
- `enable-2`: `ClearHighlightsAsync`, then `MoveToAsync(#btn-enable-OrchardCore_UrlRewriting)`. Then
  `OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_UrlRewriting))`, and wait for the
  text "The feature URL Rewriting was enabled." to be visible.
  *Visible at the end*: the Features page, reloaded, with the green message "The feature URL Rewriting was enabled."
  *Labels*: Enable `F/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; message `F/Controllers/AdminController.cs:317`.
- `enable-3`: if the URL Rewriting menu item is not visible, click **Tools**. Then `HighlightAsync` on the URL
  Rewriting menu item, without a caption.
  *Visible at the end*: the Tools section of the admin menu open, with **URL Rewriting** in a green ring.
  *Labels*: `M/AdminMenu.cs:18-23`; permission `M/Services/UrlRewritingPermissionProvider.cs:9` and the Administrator
  default on lines 17-22; its label "Manage URLs rewriting rules" `C/UrlRewritingPermissions.cs:8`.

## Clip `03-redirect`

Starts on `/Admin` (opened before `StartCaptureAsync`). Snapshot S1: the feature is enabled, the rule list is empty.

- `redirect-1`: click **Tools** if the URL Rewriting item is not visible, then click it
  (`ClickAndWaitForNavigationAsync`).
  *Visible at the end*: the page "URL Rewriting Rules" (title in the breadcrumb), the search box, the **Add Rule**
  button, and the message "Nothing here! There are no rewrite rules at the moment."
  *Labels*: `M/Views/Admin/Index.cshtml:9` (title), `:23` (Search), `:28` (Add Rule), `:43` (empty message).
- `redirect-2`: click `button.create` (text **Add Rule**). Wait until `#modalAddRule` has the class `show`, then
  `PauseAsync(2000)`. Then click the **Add** link of the Redirect card, `#modalAddRule .card:has(h4:text-is("Redirect")) a.btn`
  (`ClickAndWaitForNavigationAsync`). Never use a bare "Add": the window has one per card.
  *Visible at the end*: the page "New 'Redirect' rule" with the fields Display Name (empty, focused), Match URL
  Pattern, Case Insensitive (unchecked), Query String Policy ("Append Query String from Original Request"), Redirect
  Type ("Found (302)") and The Substitution URL Pattern, and the buttons **Save** and **Cancel**. Before the click, the
  window "Available Rule Types" showed the cards Redirect ("URL Redirect Rule") and Rewrite ("URL Rewrite Rule").
  *Labels*: `M/Views/Admin/Index.cshtml:28` and `:64`; cards `M/Views/RewriteRule.Link.cshtml:11-12` and `:19`;
  names `C/Services/UrlRedirectRuleSource.cs:19-20`, `C/Services/UrlRewriteRuleSource.cs:19-20`;
  title `M/Views/Admin/Create.cshtml:4`.
- `redirect-3`: `TypeAsync(Display Name, "Redirect about-us to about")`, then `TypeAsync(Match URL Pattern, "^/about-us$")`,
  Tab after each.
  *Visible at the end*: both fields filled; the hint under Match URL Pattern ("Specifies a pattern using regular
  expressions to match incoming URLs, following the syntax defined in ECMA-262.").
  *Labels*: `M/Views/RewriteRule.Fields.Edit.cshtml:4` and `:8`; `M/Views/UrlRedirectRule.Edit.cshtml:4` and `:7`.
- `redirect-4`: `ClickAsync(Case Insensitive)`.
  *Visible at the end*: the box checked, with its hint "When checked, the pattern will be case insensitive."
  *Labels*: `M/Views/UrlRedirectRule.Edit.cshtml:15-16`.
- `redirect-5`: `HighlightAsync(Query String Policy select)`, without a caption. Do not change the value.
  *Visible at the end*: the select, showing "Append Query String from Original Request", inside a green ring.
  *Labels*: `M/Views/UrlRedirectRule.Edit.cshtml:22`; options `M/Drivers/UrlRedirectRuleDisplayDriver.cs:42-43`.
- `redirect-6`: `ClearHighlightsAsync`, `HighlightAsync(Redirect Type select)`, `PauseAsync(2500)`, `ClearHighlightsAsync`, then
  `SelectAsync(Redirect Type select, "Moved Permanently (301)")`.
  *Visible at the end*: the select showing "Moved Permanently (301)". The other options, not shown open, are
  "Found (302)", "Temporary Redirect (307)" and "Permanent Redirect (308)".
  *Labels*: `M/Views/UrlRedirectRule.Edit.cshtml:29`; options `M/Drivers/UrlRedirectRuleDisplayDriver.cs:47-50`; the
  default is Found (`:36-38`).
- `redirect-7`: `TypeAsync(The Substitution URL Pattern, "/about")`, Tab, then `MoveToAsync(Save)` (do not click).
  *Visible at the end*: the field holding `/about`, the cursor resting on **Save**.
  *Labels*: `M/Views/UrlRedirectRule.Edit.cshtml:36`; **Save** `M/Views/Admin/Create.cshtml:14`.
- `redirect-8`: `OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(Save))`, then wait for the text "Rule created
  successfully." to be visible, then `HighlightAsync` on the row filtered by "Redirect about-us to about".
  *Visible at the end*: the page "URL Rewriting Rules" with the green message "Rule created successfully." and one row:
  drag handle, checkbox, the name "Redirect about-us to about", a badge "Redirect", a badge with the creation time and
  the user name `admin`, and the buttons **Edit** and **Delete**, in a green ring.
  *Labels*: message `M/Controllers/AdminController.cs:162`; row `M/Views/RewriteRule.SummaryAdmin.cshtml`,
  `M/Views/RewriteRule.DefaultTags.SummaryAdmin.cshtml:7`, `M/Views/RewriteRule.DefaultMeta.SummaryAdmin.cshtml`,
  `M/Views/RewriteRule.Buttons.SummaryAdmin.cshtml:9` and `:15`.

## Clip `03-redirect-test`

Starts on `/` (the Blog home page, opened before `StartCaptureAsync`). Snapshot S2: the redirect rule exists, and the
site has restarted since it was saved. A helper used by both test clips:
`string Address() => new Uri(scene.Page.Url).PathAndQuery;`

- `redirect-test-1`: `HighlightAsync(h1.First, "Request: /about-us?ref=newsletter")`, where `h1` is `scene.Page.Locator("h1")`.
  *Visible at the end*: the Blog home page with its large title "Blog" in a green ring and the green label
  "Request: /about-us?ref=newsletter" under it.
- `redirect-test-2`: `ClearHighlightsAsync`, `NavigateAsync("/about-us?ref=newsletter")`, then wait until
  `scene.Page.Url` ends with `/about?ref=newsletter`, then `HighlightAsync(h1.First, "Result: " + Address())`.
  *Visible at the end*: the About page (title "About", subtitle "This is what I do.") with the title in a green ring
  and the label "Result: /about?ref=newsletter".
  *Source*: the redirect flags `C/Services/UrlRedirectRuleSource.cs:72-87`; the About page
  `blog.recipe.json` (content item "About").

## Clip `04-rewrite`

Starts on `/Admin/UrlRewriting/Index` (opened before `StartCaptureAsync`). Snapshot S2: one rule in the list.

- `rewrite-1`: click `button.create` (**Add Rule**), wait for `#modalAddRule` to have the class `show`,
  `PauseAsync(1500)`, then click the **Add** link of the Rewrite card, `#modalAddRule .card:has(h4:text-is("Rewrite")) a.btn`
  (`ClickAndWaitForNavigationAsync`).
  *Visible at the end*: the page "New 'Rewrite' rule" with the fields Display Name, Match URL Pattern, Case
  Insensitive, Query String Policy, Stop Evaluation of Subsequent Rules (unchecked) and The Substitution URL Pattern.
  *Labels*: `M/Views/Admin/Index.cshtml:28`; `M/Views/RewriteRule.Link.cshtml:11` and `:19`; `M/Views/Admin/Create.cshtml:4`.
- `rewrite-2`: `TypeAsync(Display Name, "Serve media from img")`, then `TypeAsync(Match URL Pattern, "^/img/(.*)$")`, Tab after each.
  *Visible at the end*: both fields filled.
  *Labels*: `M/Views/RewriteRule.Fields.Edit.cshtml:4`; `M/Views/UrlRewriteRule.Edit.cshtml:4`.
- `rewrite-3`: `TypeAsync(The Substitution URL Pattern, "/media/$1")`, Tab. The text is a plain string, not an
  interpolated one.
  *Visible at the end*: the field holding `/media/$1`.
  *Labels*: `M/Views/UrlRewriteRule.Edit.cshtml:39`.
- `rewrite-4`: `ClickAsync(Case Insensitive)`, then `SelectAsync(Query String Policy select, "Exclude Query String from Original Request")`.
  *Visible at the end*: Case Insensitive checked, and the select showing "Exclude Query String from Original Request".
  *Labels*: `M/Views/UrlRewriteRule.Edit.cshtml:15` and `:22`; options `M/Drivers/UrlRewriteRuleDisplayDriver.cs:40-41`.
- `rewrite-5`: `ClickAsync(Stop Evaluation of Subsequent Rules)`, then `MoveToAsync(Save)` (do not click).
  *Visible at the end*: the box checked, with its hint "When selected, subsequent rules will be ignored if this rule
  is matched.", and the cursor resting on **Save**.
  *Labels*: `M/Views/UrlRewriteRule.Edit.cshtml:32-33`.
- `rewrite-6`: `OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(Save))`, wait for the text "Rule created
  successfully.", then `HighlightAsync` on `#rewrite-rules-sortable-list` without a caption.
  *Visible at the end*: the page "URL Rewriting Rules" with the message "Rule created successfully." and two rows in a
  green ring: "Redirect about-us to about" (badge Redirect) first, "Serve media from img" (badge Rewrite) second.
  *Labels*: `M/Controllers/AdminController.cs:162`; badge `M/Views/RewriteRule.DefaultTags.SummaryAdmin.cshtml:7`.

## Clip `04-rewrite-test`

Starts on `/` (opened before `StartCaptureAsync`). Snapshot S3: both rules exist, and the site has restarted since the
last save. Use the `Address()` helper of `03-redirect-test`.

- `rewrite-test-1`: `NavigateAsync("/img/about-bg.jpg")`, then wait until the document holds an `img` element, then
  `HighlightAsync(scene.Page.Locator("img"), "Address: " + Address())`.
  *Visible at the end*: the photograph of the About page header, shown as an image document, in a green ring, with the
  label "Address: /img/about-bg.jpg". The label proves that the address did not change.
  *Source*: the file is `about-bg.jpg` of the Media library (`blog.recipe.json`, step `media`); the rule
  `C/Services/UrlRewriteRuleSource.cs:39-53`.

## Clip `05-manage`

Starts on `/Admin/UrlRewriting/Index` (opened before `StartCaptureAsync`). Snapshot S3: two rows, "Redirect about-us to
about" first and "Serve media from img" second.

- `manage-1`: `HighlightAsync` on the row "Redirect about-us to about", without a caption.
  *Visible at the end*: the first row in a green ring, showing drag handle, checkbox, name, badge "Redirect", creation
  badges, **Edit** and **Delete**.
  *Labels*: `M/Views/RewriteRule.Handle.SummaryAdmin.cshtml:7` (tooltip "Drag to reorder"); the other row views as in `redirect-8`.
- `manage-2`: `ClearHighlightsAsync`, click **Edit** in the row "Redirect about-us to about"
  (`ClickAndWaitForNavigationAsync`), `PauseAsync(2500)`, then click **Cancel** (`a.cancel`, `ClickAndWaitForNavigationAsync`).
  *Visible at the end*: back on "URL Rewriting Rules" with both rows unchanged. In the middle of the step, the page
  "Edit 'Redirect about-us to about' rule" showed the saved values.
  *Labels*: `M/Views/Admin/Edit.cshtml:4`, `:14` and `:20`; `M/Views/RewriteRule.Buttons.SummaryAdmin.cshtml:9`.
- `manage-3`: run the drag inside
  `scene.Page.RunAndWaitForResponseAsync(() => scene.DragToAsync(handle, firstRow, 0, -12), r => r.Url.EndsWith("/url-rewriting/resort"))`,
  where `handle` is `.ui-sortable-handle` in the row "Serve media from img" and `firstRow` is the row "Redirect about-us to about".
  Then wait until the first element of `#rewrite-rules-sortable-list .item` has the text "Serve media from img". If the row
  did not move, reload the page with `NavigateAsync("/Admin/UrlRewriting/Index")` and drag again with
  `scene.DragAsync(handle, x, y)`, where `x` is the horizontal and `y` the vertical center of the first row, minus 20.
  *Visible at the end*: the list with "Serve media from img" first and "Redirect about-us to about" second.
  *Source*: `M/Assets/js/sortable-rules.js:3` and `:46-54`, `M/Endpoints/Rules/SortRulesEndpoint.cs:14`.
- `manage-4`: click **Delete** in the row "Serve media from img" (`a.btn-danger`), wait until `#confirmRemoveModal` is
  visible, `PauseAsync(2000)`, `MoveToAsync(#modalOkButton)`, then
  `OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(#modalOkButton))`, and wait for the text "Rule deleted
  successfully.".
  *Visible at the end*: the page "URL Rewriting Rules" with the green message "Rule deleted successfully." and a single row,
  "Redirect about-us to about". In the middle of the step, the window titled "Delete" asked "Are you sure you want to
  remove this element?" with the buttons **Ok** and **Cancel**.
  *Labels*: `M/Views/RewriteRule.Buttons.SummaryAdmin.cshtml:15`; window `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`;
  message `M/Controllers/AdminController.cs:253`.

## Slides

Drawn by `slides.py`; nothing to record.

- `why-1`: two cards, "A page is renamed" and "A folder of files moves".
- `kinds-1`: four cards; Redirect and its example are bright, Rewrite and its example are dimmed.
- `kinds-2`: the same four cards; Rewrite and its example are bright.
- `order-1`: three cards: "A list, from the top down", "Stop Evaluation of Subsequent Rules", "Admin addresses are left alone".
- `pattern-1`: four cards: the caret, the dollar sign, the parentheses, and `$1`.

## Checks for private data

- The site is new: the only user is `admin` with the email `admin@example.com`; the only badge with a name reads `admin`.
- Look at every contact sheet for: the password (never typed on camera), a real host name in an address, and the footer of
  the Blog theme (its links are `#!` placeholders and sit below the visible area).
- The only addresses on screen are `/about-us`, `/about`, `/img/about-bg.jpg` and the admin pages of `localhost`.
- The stock text of the Blog recipe (the sample blog post) is not personal data; it appears only below the title of the
  home page, if at all.
- OCR the finished video for an email address other than `admin@example.com`, a real person's name, and the password.

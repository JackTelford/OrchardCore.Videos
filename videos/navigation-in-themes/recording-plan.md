# Recording plan: navigation-in-themes

A person records the four browser clips of this video from this plan alone. The video is for developers: its code is on
slides (drawn by `slides.py`), so the recorder only shows the screens that the code produces. Read
`.agents/skills/orchardcore-video-producer/references/recording.md` first; this plan only adds what is specific to the
video. In this file a selector stands for `scene.Page.Locator(...)`.

Every label below was read from the Orchard Core source at commit `92db4a83` (paths are relative to `src/` of
`OrchardCMS/OrchardCore`). Nothing was run on a site: `OPEN-QUESTIONS.md` lists what to confirm while recording.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout at that commit or a later one,
  started on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- **Auto Setup**, so no setup or sign-in form is recorded. Set these variables before the first start, with the
  password read from the variable that holds it (chosen outside git, never written in a file):

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9
  ```

- **Tenant and themes**: the `Default` tenant, with the Blog recipe's themes: **The Blog Theme** on the front end (its
  menu and pager are Liquid templates, `TheBlogTheme/Views/Menu.liquid` and `Blog-ListPart.liquid`) and **The Admin
  Theme** in the admin. Do not change the theme.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD` saves `state.json` (git-ignored).
- **Features that the Blog recipe already enables** (check them on Tools, Features; `OrchardCore.Navigation` is always
  enabled): Menu, Dynamic Cache, Deployment (it holds Tools, Deployments, JSON Import), Lists, Alias, Autoroute.
- **Feature to enable before recording**: **Admin Dashboard** (`OrchardCore.AdminDashboard`). Open Tools, Features,
  type `Admin Dashboard` in the search box, and click the **Enable** button of its row. Wait for the page to reload. The
  breadcrumb clip needs it, because this feature registers the provider that adds the Dashboard link to the trails of the
  admin (`OrchardCore.Modules/OrchardCore.AdminDashboard/Startup.cs:39`).
- **Settings before recording**: Settings, General (`/Admin/Settings/general`). Tick **Allow users to change the page size
  on listing pages** (under **Page size selection**), leave **Page size** at `10` and **Page size options** at
  `10, 25, 50, 100`, and click **Save**. The tenant restarts: wait for the message `Site settings updated successfully.`,
  then reload the page and check that the box is still ticked. (`OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:62-86`)
  Both pagers then render the **Items per page** selector, also the slim pager of the blog on the front end (the Blog Theme has
  no `Pager_PageSizeSelector` template, so the module's plain markup shows there).
- **Settings that a clip changes**: Settings, Admin (`/Admin/Settings/admin`), **Show breadcrumb**. It is ticked by default.
  The clip `04-trail-site` clears it.

## Seed data (outside the capture)

The site starts with the Blog recipe's content: the **Main Menu** (alias `main-menu`, with the links **Home** and
**About**), the **Blog**, one blog post, and an **About** page. The pager clips need more than one page of blog posts, so
add twelve fake posts:

1. Open Content, Content Items, click the title **Blog**, and copy the content item id from the address of the editor
   (`/Admin/Contents/ContentItems/<blog-id>/Edit`).
2. Print the recipe with the script below, with the id in place of `<blog-id>`.
3. Open Tools, Deployments, **JSON Import** (`/Admin/DeploymentPlan/Import/Json`), paste the output into the **JSON**
   editor, and click **Import**. The message is `Recipe imported successfully!`
   (`OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30`, `Views/Import/Json.cshtml`, `Controllers/ImportController.cs:185`).
4. Check the result: the home page `/` lists ten posts and ends with **Older Posts →**; `/Admin/Contents/ContentItems/BlogPost`
   lists thirteen items on two pages (the twelve posts and the sample post). The unfiltered list of Content, Content Items
   does not show them: **Blog Post** is not a listable type in the Blog recipe (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:188-195`),
   and only listable types are listed when no type is selected
   (`OrchardCore.Modules/OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs:182`), which leaves five items
   on one page (Main Menu, Tags, Categories, Blog and About), a page with no page links (`OrchardCore.Modules/OrchardCore.Navigation/PagerShapesTableProvider.cs:179`).

The posts hold no real name: the author is `admin`. Create nothing else.

The script (save it outside the repository, or paste it in a terminal):

```bash
python - <blog-id> <<'EOF'
import json, sys
blog_id = sys.argv[1]
items = []
for n in range(1, 13):
    day = f"2025-03-{n:02d}T10:00:00Z"
    title = f"Orchard notes, part {n}"
    items.append({
        "ContentType": "BlogPost",
        "ContentItemId": "[js: uuid()]",
        "DisplayText": title,
        "Author": "admin",
        "Latest": True,
        "Published": True,
        "CreatedUtc": day,
        "ModifiedUtc": day,
        "PublishedUtc": day,
        "TitlePart": {"Title": title},
        "ContainedPart": {"ListContentItemId": blog_id, "Order": n},
        "MarkdownBodyPart": {"Markdown": f"Fake text for part {n} of the orchard notes."},
        "AutoroutePart": {"Path": f"blog/orchard-notes-part-{n}"},
        "BlogPost": {"Subtitle": {"Text": f"A fake subtitle for part {n}"}},
    })
print(json.dumps({"steps": [{"name": "content", "data": items}]}, indent=2))
EOF
```

## Order, dependencies and snapshots

1. Put the site in the starting state above (Admin Dashboard enabled, page size selection allowed, twelve posts added,
   **Show breadcrumb** ticked). Sign the recorder in. Stop the site, copy its `App_Data` folder and `state.json` aside,
   and start the site again. This copy is the snapshot.
2. Record the clips in this order. None of them needs another clip to have been recorded first, because each opens its
   own start page:
   1. `02-menu-site` (reads the site only)
   2. `05-pager-blog` (reads the site only)
   3. `05-pager-admin` (reads the site only)
   4. `04-trail-site` (**changes the site**: it clears **Show breadcrumb**)
3. After `04-trail-site`, stop the site and restore the snapshot (or tick **Show breadcrumb** again at Settings, Admin and
   click **Save**) before any new take of a clip. To record everything again, restore the snapshot first.

## Clip 02-menu-site

- **Starts on**: the Menus list, `/Admin/Contents/ContentItems/Menu` (the page of Content, Menus). Open it with
  `scene.OpenAsync`, before the capture. Expand nothing: the menu is collapsed.
- **Capture starts**: after the list is shown.

`menusite-1` (35 words)
- Click the title **Main Menu** in the list: `ul.admin-list a:has-text('Main Menu')`, with
  `scene.ClickAndWaitForNavigationAsync`. The editor of the menu opens.
- Wait for `#menu` (the list of the menu items).
- `scene.HighlightAsync(scene.Page.GetByLabel("Alias", new() { Exact = true }), "alias:main-menu")`. The **Alias** field
  holds `main-menu`.
- `scene.PauseAsync(2500)`, then `scene.ClearHighlightsAsync()`, then `scene.HighlightAsync(scene.Page.Locator("#menu"))`
  around the two items **Home** and **About**.
- On screen when the narration ends: the editor of **Main Menu**, with the **Alias** field showing `main-menu` and a ring
  around the list of the two link items.
- Labels: Content, Menus `OrchardCore.Modules/OrchardCore.Menu/AdminMenu.cs:33-34`; **Alias**
  `OrchardCore.Modules/OrchardCore.Alias/Views/AliasPart.Edit.cshtml:10`; the items list `OrchardCore.Modules/OrchardCore.Menu/Views/MenuPart.Edit.cshtml:37`;
  the alias `main-menu` and the items **Home** and **About** `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:818-854`.

`menusite-2` (28 words)
- `scene.ClearHighlightsAsync()`, then `scene.NavigateAsync("/")`. Wait for `#mainNav`.
- `scene.HighlightAsync(scene.Page.Locator("#navbarResponsive ul.navbar-nav"))`: the links **Home** and **About** in the
  navigation bar of the theme.
- On screen when the narration ends: the home page of the blog, with a ring around **Home** and **About** in the navigation
  bar.
- Labels: the layout renders the menu by its alias at `OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:32`; the
  navigation bar is `#mainNav` (line 25) and the list is `ul.navbar-nav` of `Views/Menu.liquid:1-2`.

## Clip 04-trail-site

- **Starts on**: the editor of **Main Menu**. Open the Menus list `/Admin/Contents/ContentItems/Menu` with
  `scene.OpenAsync`, click the title **Main Menu** (not recorded), and wait for `#menu`. Keep the address of the editor in a
  variable.
- **Needs**: the Admin Dashboard feature enabled and **Show breadcrumb** ticked (see the site above).
- **Capture starts**: after `#menu` is shown.

`trailsite-1` (39 words)
- Click the **Edit** button of the **About** row: `#menu li.menu-item:has-text('About') a.btn-success`, with
  `scene.ClickAndWaitForNavigationAsync`. The page `/Admin/Menu/Edit` opens. Keep its address in a variable.
- Wait for `nav.oc-breadcrumb`.
- `scene.HighlightAsync(scene.Page.Locator("nav.oc-breadcrumb"))`; `scene.PauseAsync(2500)`;
  `scene.ClearHighlightsAsync()`; then `scene.HighlightAsync(scene.Page.Locator("nav.oc-breadcrumb li.breadcrumb-item").First)`
  around the first node, **Dashboard**.
- On screen when the narration ends: the editor of the menu item, with the trail
  `Dashboard › Manage Content › Edit Main Menu › Edit Link Menu Item` above the heading **Edit Link Menu Item**, and a ring
  around **Dashboard**. **Edit Link Menu Item** is plain text; the other three nodes are links.
- Labels: the trail is declared at `OrchardCore.Modules/OrchardCore.Menu/Views/Admin/Edit.cshtml:19-26` (name `MenuEdit`,
  **Manage Content**, **Edit {0}** with the menu title, **Edit {0}** with the type **Link Menu Item**); **Dashboard** is added
  by `OrchardCore.Modules/OrchardCore.AdminDashboard/Services/DashboardBreadcrumbProvider.cs:33-39`; the type name **Link Menu
  Item** is in `OrchardCore.Modules/OrchardCore.Menu/Migrations/menu.recipe.json:58`; the `nav` is drawn by
  `OrchardCore.Themes/TheAdmin/Views/Breadcrumb.cshtml:24-31` and carries the class `oc-breadcrumb`
  (`OrchardCore.Modules/OrchardCore.Navigation/BreadcrumbShapes.cs:17`).

`trailsite-2` (18 words)
- `scene.ClearHighlightsAsync()`, then `scene.NavigateAsync("/Admin/Settings/admin")`. Wait for the checkbox.
- Click the checkbox **Show breadcrumb**: `scene.ClickAsync(scene.Page.GetByLabel("Show breadcrumb"))`. It becomes cleared.
- Click **Save**: `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save:has-text('Save')"))`.
- On screen when the narration ends: the page Settings, Admin after the save, with **Show breadcrumb** cleared and the message
  `Site settings updated successfully.`
- Labels: **Show breadcrumb** `OrchardCore.Modules/OrchardCore.Admin/Views/AdminSettings.Edit.cshtml:58`; Settings, Admin
  `OrchardCore.Modules/OrchardCore.Admin/AdminMenu.cs:26-27`; **Save** `OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`;
  the message `OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`.
- If the action takes longer than the narration, record again with a shorter pause: the end of the narration must show the
  saved page.

`trailsite-3` (18 words)
- `scene.NavigateAsync(<the address kept in trailsite-1>)`. Wait for `h1.oc-breadcrumb-title`.
- `scene.HighlightAsync(scene.Page.Locator("h1.oc-breadcrumb-title"))`.
- On screen when the narration ends: the same menu item editor, with no trail, and the heading **Edit Link Menu Item** with a
  ring around it.
- Labels: the heading is rendered by the tag helper when the trail is hidden
  (`OrchardCore/OrchardCore.Navigation.Core/BreadcrumbTagHelper.cs:146-160`).
- This clip leaves **Show breadcrumb** cleared: restore the snapshot afterwards.

## Clip 05-pager-blog

- **Starts on**: the home page `/`, with `scene.OpenAsync`. The blog lists ten posts per page.
- **Capture starts**: at the top of the page.

`blogpager-1` (29 words)
- `await scene.ScrollAsync(0, 6000)`: the end of the page, where the pager is.
- `scene.HighlightAsync(scene.Page.Locator("ul.pager a:has-text('Older Posts')"))`; `scene.PauseAsync(2500)`;
  `scene.ClearHighlightsAsync()`.
- `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("ul.pager a:has-text('Older Posts')"))`. The second page opens
  (the address gets an `after` parameter).
- `await scene.ScrollAsync(0, 6000)`; `scene.HighlightAsync(scene.Page.Locator("ul.pager a:has-text('Newer Posts')"))`.
- On screen when the narration ends: the end of the second page, with a ring around **← Newer Posts** and no **Older Posts**
  link (three posts are listed on this page). The plain **Items per page** selector of the Navigation module is visible beside
  the pager on both pages, because page size selection is allowed (see the site above); that is expected, and `pagertheme-2`
  explains it. Do not touch it.
- Labels: the texts come from `OrchardCore.Themes/TheBlogTheme/Views/Blog-ListPart.liquid:5-6` (`← Newer Posts`,
  `Older Posts →`); the list gets the classes `pager` and `pagination` in
  `OrchardCore.Modules/OrchardCore.Navigation/PagerShapesTableProvider.cs:204-205`. The Blog uses the slim pager because
  **Show full pager** is not ticked on its list part (`OrchardCore.Modules/OrchardCore.Lists/Views/ListPartSettings.Edit.cshtml:44`).

## Clip 05-pager-admin

- **Starts on**: `/Admin/Contents/ContentItems/BlogPost`, the list of Blog Post content items, with `scene.OpenAsync`. The
  unfiltered list of Content, Content Items has only five items, on one page, so it draws no page links; **Blog Post** is not
  a listable type, so no menu entry and no filter option leads to this address, and it is opened by its address, as the entry
  Content, Menus does for its type. The page heading is **Blog Post**.
- **Needs**: the twelve posts and **Allow users to change the page size on listing pages** (see the site above). With the
  thirteen posts and a page size of ten the list has two pages.
- **Capture starts**: at the top of the list.

`adminpager-1` (42 words)
- `await scene.ScrollAsync(0, 6000)`: the end of the list.
- `scene.HighlightAsync(scene.Page.Locator("nav[aria-label='Listing pages']"))`: the pager (numbered pages, and the links to
  the first, previous, next and last pages, drawn as icons).
- `scene.PauseAsync(3000)`; `scene.ClearHighlightsAsync()`.
- `scene.HighlightAsync(scene.Page.Locator(".pager-page-size"), "Items per page")`: the selector at the end of the pager, inside
  the same `nav`.
- On screen when the narration ends: the end of the list of Blog Post items, with a ring and the caption **Items per page**
  around the selector, and the numbered pages still visible beside it.
- Labels: the route `Contents/ContentItems/{contentTypeId?}` is at `OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:67`,
  the heading **Blog Post** is the display name of the type (`OrchardCore.Modules/OrchardCore.Contents/Views/Admin/List.cshtml:14-26`
  and `blog.recipe.json:189`); the pager is wrapped in `nav aria-label="Listing pages"` by
  `OrchardCore.Themes/TheAdmin/Views/Pager.cshtml:6`; the selector and **Items per page** are
  `OrchardCore.Themes/TheAdmin/Views/Pager_PageSizeSelector.cshtml:8-9`, rendered by the pager itself
  (`OrchardCore.Modules/OrchardCore.Navigation/PagerShapesTableProvider.cs:362-365`); the list footer that holds the pager is
  `OrchardCore.Modules/OrchardCore.Admin/Views/AdminList.cshtml:159-170`.

## Slides

These steps have no recording. `slides.py` draws them; nothing is recorded for them.

- `shapes-1`: the Views folder of a theme as a tree, with the template names that match a shape (`Menu`, `Menu-MainMenu`,
  `Breadcrumb`, `Breadcrumb-MenuEdit`, `Pager`, `Pager-MainBlog`), and two cards: Shape and Template.
- `shapes-2`: three cards, one for each family of shapes: Menu, Breadcrumb, Pager.
- `render-1`: the menu tag helper in Razor (left, bright) and the shape tag in Liquid (right, dimmed), and the card **alias**.
- `render-2`: the same two panels, the cache tag line of the Razor panel and the whole Liquid panel bright, and the card **cache tag**.
- `templates-1`: the Menu template (`Menu.cshtml`) and two notes.
- `templates-2`: the MenuItem template (`MenuItem.cshtml`) and two notes.
- `templates-3`: the MenuItemLink template for link menu items and two notes.
- `templates-4`: four cards with the alternate file names of a menu.
- `trail-1`: the view imports and the breadcrumb tag helper in a view, the tag helper lines bright, and three notes.
- `trail-2`: the same view, the breadcrumb item lines bright, and three notes.
- `provider-1`: a breadcrumb provider class, the line that registers it, and two notes.
- `trailtheme-1`: four cards with the template names of a trail.
- `pager-1`: a controller action that creates the pager shape, and three notes.
- `pager-2`: the line that displays the pager in Razor, the Liquid pager tag of the blog theme, and the slim pager code.
- `pagertheme-1`: the templates `Pager.cshtml`, `Pager_Next.cshtml` and `Pager_Link.cshtml` of The Default Theme.
- `pagertheme-2`: three cards about the page size selector: the setting, the default markup, and the theme template.

## Checks for private data

- Every name on screen is fake: the user is `admin` (`admin@example.com`), the posts are `Orchard notes, part 1` to
  `part 12` and the Blog recipe's own sample post and **About** page. The site is named `Orchard Core`.
- No password appears anywhere: the Auto Setup password and the sign-in come from `VIDEO_PASSWORD`, which is never written in a
  file of the repository. Do not record the user menu or a sign-in form.
- OCR every frame (see SKILL.md, step 6) and search the text for the user's real name, e-mail addresses other than
  `@example.com`, and the machine name. The address bar is not in the recording.
- Do not commit `state.json`, the copy of `App_Data`, the MP4, or the audio.

# Recording plan: Taxonomies: Term Pages That List Their Content

For the person who records this video. Follow it as written: it needs no decision. Every label below is spelled as the
Orchard Core source spells it; the file and line that holds it is given for each step (paths are relative to the
Orchard Core checkout, `src/`). In this plan a selector stands for `scene.Page.Locator(...)`, since the recorder's
methods take locators. General rules, the scene API and the pitfalls are in
`.agents/skills/orchardcore-video-producer/references/recording.md`.

Audience: admin. The recording shows the admin screens and the front end of the site only: no code, no JSON, no
configuration files, no Liquid. The page the viewer sees in the Content Types editor is never recorded (its Pattern box
shows Liquid): do not open Design, Content Definition, Content Types.

The browser has no address bar in the recording. Where the narration says an address, the step puts it on screen with
`HighlightAsync(..., "<address>")`, or the Permalink field shows it.

## 1. The site it needs

| Setting | Value |
|---|---|
| Orchard Core | a checkout of `OrchardCMS/OrchardCore`, run from `src/OrchardCore.Cms.Web`, on `http://localhost:5010` |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup, so no setup or login form is recorded (see `references/recording.md`) |
| Recipe | **Blog** (it enables Taxonomies, Autoroute, Lists, Markdown and sets The Blog Theme as the site theme) |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Database | SQLite |
| Super user | user name `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Outbound calls | blocked: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address |

Auto Setup values, as environment variables of the site process (the password comes from `VIDEO_PASSWORD`; the
Auto Setup reference is `docs/reference/modules/AutoSetup/README.md`):

```bash
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

Features to enable before recording: none. The Blog recipe has already enabled every feature this video shows
(`OrchardCore.Taxonomies`, `OrchardCore.Autoroute`, `OrchardCore.Lists`, `OrchardCore.Contents`,
`OrchardCore.Settings`; `TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). Do not enable
"Taxonomies Contents List Filters" (`OrchardCore.Taxonomies.ContentsAdminList`): it adds a "Taxonomy Filters" entry to the
admin menu that this video does not talk about.

Settings to leave alone: Settings, General, **Page size** stays at its default of 10 until the clip `04-paging`
changes it (default: `OrchardCore.Modules/OrchardCore.Settings/Services/SiteService.cs:59`).

What the Blog recipe provides and this video relies on (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`):

- A taxonomy **Categories** (permalink `categories`, `RouteContainedItems` on, line 960) with one term, **Travel**
  (permalink `travel`, line 950) of the content type Category, whose Autoroute part has "Manage contained item routes"
  on (line 577).
- A taxonomy **Tags** (permalink `tags`, line 917) with three terms of the content type Tag, **Earth**, **Exploration**
  and **Space** (permalink `earth`, `exploration`, `space`; Manage contained item routes on, line 535).
- One blog post, "Man must explore, and this is exploration at its greatest" (category Travel; tags Earth, Exploration
  and Space), in the Blog at the permalink `blog`.
- The front end uses The Blog Theme: the home page lists the posts as summaries with a badge per category and per tag;
  a term page has the term's name as its heading, then the summaries, then the pager
  (`OrchardCore.Themes/TheBlogTheme/Views/`: `Blog-ListPart.liquid`, `Content-BlogPost.Summary.liquid`,
  `Content-Category.liquid`, `Content.liquid`, `Category-TermPart.liquid`, `Tag-TermPart.liquid`,
  `TermContentItem-Tag.liquid`).

### The exact fake seed data (create it outside the capture)

Create five blog posts through the admin, in this order, one after the other (leave at least two seconds between two
posts: the term pages sort by creation time). Each one is created in the Blog, so it is listed on the home page:
Content, Content Items, then in the row of the **Blog** item the button whose label starts with "List" and ends with
"Items", then **Create Blog Post** (`OrchardCore.Modules/OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52`).
Fill the form as the table says, leave Banner Image and Permalink empty, and select **Publish**
(`OrchardCore.Modules/OrchardCore.Contents/Views/Content.PublishButton.cshtml`). Do not select Save Draft, and do not edit a
post after publishing it: every post must have a published version and no draft.

The fields of the form are Title, Subtitle, the body editor, Tags and Category (labels from the Blog recipe,
`blog.recipe.json` lines 611 to 672, the BlogPost type). Category is a list of radio buttons: select Travel. Tags is
a search box: type the tag name and pick the existing tag from the list that opens. Do not create a tag or a category.

| # | Title | Subtitle | Body | Category | Tags |
|---|---|---|---|---|---|
| 1 | A week on the Amalfi coast | Lemon groves, steep stairs, and slow ferries | The ferry leaves at nine, and the lemons are the size of grapefruits. | Travel | Earth |
| 2 | Night trains across the continent | Sleep somewhere between two capitals | The sleeper car rocks gently, and the conductor brings tea at dawn. | Travel | Exploration |
| 3 | Packing light for a month away | One bag, three shirts, and a good map | Roll the shirts, fold the map, and leave the second pair of shoes at home. | Travel | none |
| 4 | A weekend in the mountains | Thin air and long views | The trail climbs for two hours, and the hut at the top serves soup. | Travel | Earth |
| 5 | Where to watch the night sky | Dark places, clear skies, and a thermos of tea | Drive an hour from the nearest town, switch off the lamp, and wait twenty minutes. | Travel | Space, then Exploration |

The post of the recipe is left as it is. The seed can also be made with `fetch` calls in a recorder `PrepareAsync`; what
counts is the end state below.

Check the end state with the browser, signed in as `admin`, before taking the snapshot:

- `/categories/travel` lists six posts, newest first: posts 5, 4, 3, 2 and 1, then "Man must explore, and this is exploration at its greatest". No pager.
- `/tags/earth` lists three posts: 4, 1, and the recipe post. `/tags/exploration` lists three: 5, 2, and the recipe post. `/tags/space` lists two: 5 and the recipe post.
- `/tags` lists the three tags; `/categories` lists the one category.
- Each of these pages shows each post once. If a post shows twice, it has a draft: publish it again, or create it again.
- Settings, General shows **Page size** 10.

### Snapshot

When the end state is right, stop the site (by its process ID), then copy the data folder aside as `snapshot-seeded`.
Start the site on the data folder again, sign in once with `Recorder setup` to write `state.json` (git-ignored), and
keep `state.json` with the snapshot. Restore `snapshot-seeded` before every full take (see section 2).

### Private data

The site is made from scratch: the only names are the invented ones above, `admin` and `admin@example.com`. No mask
script is needed.

## 2. The order of the clips, and what changes the site

| Clip | Starts from | Changes the site |
|---|---|---|
| `02-browse` | the seeded snapshot, Page size 10 | no |
| `03-routing` | the seeded snapshot, Page size 10 | no: never select Save or Publish; the clip ends on the term editor, which is left without saving |
| `04-paging` | the seeded snapshot, Page size 10 | **yes**: Page size 10 becomes 3 and the site reloads its settings |

Record in the order `02-browse`, `03-routing`, `04-paging`. The first two do not depend on each other. `04-paging` needs
Page size 10 when it starts (its narration says "set to ten") and leaves it at 3, so: before a new take of
`04-paging`, and before a take of `02-browse` or `03-routing` that follows it, stop the site, restore `snapshot-seeded`
and `state.json`, and start the site again.

Narrate first (`narrate.py`), then record: the recorder holds each step for the length of its narration.

## 3. The clips

Common to all three clips: Chromium at 1536x864, the admin menu collapsed, signed in through `state.json`. Everything
that is not a step (opening the first page, scrolling to the start position) is done before `StartCaptureAsync`. A
step's actions must be shorter than its narration. Clear the highlights (`ClearHighlightsAsync`) before scrolling.

### Clip `02-browse` (chapter 02, "Browse by term")

Starts on: `/` (the home page of the blog). Wait until at least one `.post-preview` is visible, then scroll the page
(`window.scrollTo`, before the capture) so that the title of the first post and the badges under it are in the
viewport.

- `browse-1`: `HighlightAsync(scene.Page.Locator(".post-preview .term-badge").First)` (a green ring around the badges
  of the first post, no caption), then `MoveToAsync` to the first link inside the ring. Visible at the end: the first
  post with its category badge (Travel) and tag badges ringed. Source:
  `OrchardCore.Themes/TheBlogTheme/Views/Content-BlogPost.Summary.liquid:15-34` and `Blog-ListPart.liquid:2`.
- `browse-2`: `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync(scene.Page.Locator(".term-badge a", new() { HasText = "Travel" }).First)`;
  wait until `header.masthead h1` is visible and its text is `Travel`; then
  `HighlightAsync(scene.Page.Locator("header.masthead h1"), "/categories/travel")`. Visible at the end: the top of the
  term page, with the heading Travel ringed and the caption `/categories/travel`. Source: the badge,
  `Content-BlogPost.Summary.liquid:18`; the heading, `Content-Category.liquid:1-18`; the address,
  `OrchardCore/OrchardCore.Autoroute.Core/Indexes/AutoroutePartIndex.cs:161-175`.
- `browse-3`: `ClearHighlightsAsync`; scroll down in three steps of about 220 pixels with `ScrollAsync(0, 220)` and
  `PauseAsync(1500)` between them. Visible at the end: the first three summaries of the list (posts 5, 4 and 3), each
  with its title, subtitle, date and badges; the heading is scrolled away. Source:
  `OrchardCore.Themes/TheBlogTheme/Views/Category-TermPart.liquid:1-3` (summaries) and
  `OrchardCore/OrchardCore.Taxonomies.Core/DefaultContentsTaxonomyListQueryService.cs:47` (newest first).
- `browse-4`: `ScrollAsync(0, -1000)` so that the first post's badges are in view again;
  `ClickAndWaitForNavigationAsync(scene.Page.Locator(".term-badge a", new() { HasText = "Space" }).First)`; wait until
  `header.masthead h1` is visible and its text is `Space`. Visible at the end: the top of the term page of the tag Space,
  with the heading Space and the first summary (post 5) under it. Source:
  `Content-BlogPost.Summary.liquid:28`, `Content.liquid:1-14` (the heading of a Tag term page: the theme has no
  Content-Tag template).
- `browse-5`: `NavigateAsync("/tags")`; wait until `header.masthead h1` has the text `Tags` and three
  `.post-preview a` links are visible; `PauseAsync(2500)`; then
  `ClickAndWaitForNavigationAsync(scene.Page.Locator(".post-preview a", new() { HasText = "Earth" }))` as the last action
  of the step. Visible at the end: the term page of Earth has loaded (heading Earth), or is loading; the next step holds on it.
  Source: `TermContentItem-Tag.liquid:2-4` (the link of each tag), `OrchardCore.Modules/OrchardCore.Taxonomies/Views/TaxonomyPart.cshtml`
  (the taxonomy page lists its terms).
- `browse-6`: scroll with `ScrollAsync(0, 450)` until the three summaries (posts 4 and 1, then the recipe post) are in
  view; `HighlightAsync(scene.Page.Locator(".post-preview").Last.Locator(".term-badge"))`. Visible at the end: the three
  summaries of the Earth page, and a ring around the badges of the last one (the recipe post, with its Travel, Earth,
  Exploration and Space badges). Source: `Tag-TermPart.liquid:1-3`;
  `OrchardCore.Modules/OrchardCore.Taxonomies/Indexing/TaxonomyIndexProvider.cs:83-96` (one index record per selected term).

### Clip `03-routing` (chapter 03, "Give terms their pages")

Starts on: `/Admin/Contents/ContentItems` (Content, Content Items), the search box empty. The admin menu stays collapsed.

- `routing-1`: `TypeAsync(scene.Page.GetByPlaceholder("Search"), "Categories")`; then
  `WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"))`; wait until a single item, `Categories`, is listed;
  `PauseAsync(1200)`; then `ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn.edit").First)` as the last action of
  the step. Visible at the end: the editor of the Categories taxonomy has loaded (or is loading). Source: "Content" and
  "Content Items", `OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65,68`; the search box,
  `OrchardCore.Modules/OrchardCore.Contents/Views/ContentsAdminListSearch.cshtml:28`; the button "Edit",
  `OrchardCore.Modules/OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`.
- `routing-2`: `HighlightAsync(scene.Page.GetByLabel("Permalink"))`. Visible at the end: the top of the taxonomy editor
  with the Permalink box ringed; it shows `categories` and is greyed out (the Taxonomy type does not allow a custom path). Source:
  the label, `OrchardCore.Modules/OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`; greyed out,
  `AutoroutePart.Edit.cshtml:37` and `OrchardCore.Modules/OrchardCore.Taxonomies/Migrations.cs:36-41`.
- `routing-3`: `ClearHighlightsAsync`; `HighlightAsync(scene.Page.GetByLabel("Route contained items"))`. Visible at the
  end: the checkbox "Route contained items" is checked and ringed, under the Permalink. Source:
  `OrchardCore.Modules/OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:94-100` (shown because the Taxonomy type allows
  contained item routing, `Migrations.cs:40`); checked by the recipe, `blog.recipe.json:961`. Do not select or clear it.
- `routing-4`: `ClearHighlightsAsync`; `ScrollAsync(0, 500)` until the list of terms is in view (one row, `Travel`, with
  the buttons Edit, Add and Delete); `PauseAsync(800)`;
  `ClickAndWaitForNavigationAsync(scene.Page.Locator("li.menu-item", new() { HasText = "Travel" }).GetByRole(AriaRole.Link, new() { Name = "Edit" }))`;
  wait until the Permalink box is visible and holds `travel`; `HighlightAsync(scene.Page.GetByLabel("Permalink"))`. Visible
  at the end: the editor of the term Travel (title "Edit Category"), with the Permalink box ringed and holding `travel`.
  Leave the page without saving. Source: the buttons, `OrchardCore.Modules/OrchardCore.Taxonomies/Views/Content.TermAdmin.cshtml:27,36,45`;
  the title, `OrchardCore.Modules/OrchardCore.Taxonomies/Views/Admin/Edit.cshtml:19`; the Permalink, `AutoroutePart.Edit.cshtml:30`.

### Clip `04-paging` (chapter 04, "Control the page size"). Changes the site.

Starts on: `/Admin/Settings/general` (Settings, General), with the box **Page size** in view (scroll before the
capture if it is not), showing `10`.

- `paging-1`: `HighlightAsync(scene.Page.GetByLabel("Page size", new() { Exact = true }))`. `Exact = true` is required here and in
  `paging-2`: without it the locator also matches the boxes labelled "Page size options" (hidden) and "Allow users to change the
  page size on listing pages", and Playwright refuses to act on a locator that matches three elements
  (`Settings-Site.Edit.cshtml:54,67,74`). Visible at the end: the Page size box ringed, holding `10`.
  Source: `OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:54`; the menu path,
  `OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36,39`; the value that term pages use,
  `OrchardCore.Modules/OrchardCore.Settings/PagerOptionsConfiguration.cs:27` and
  `OrchardCore.Modules/OrchardCore.Taxonomies/Drivers/TermPartContentDriver.cs:52`.
- `paging-2`: `ClearHighlightsAsync`; `ClickAsync(scene.Page.GetByLabel("Page size", new() { Exact = true }))`;
  `scene.Page.Keyboard.PressAsync("Control+A")`; `scene.Page.Keyboard.TypeAsync("3", new() { Delay = 120 })` (not the scene's
  `TypeAsync`: it clicks the box again, which drops the selection, and the box would hold `103`, above the maximum page size of 100, and
  would not save: `OrchardCore.Modules/OrchardCore.Settings/Drivers/DefaultSiteSettingsDisplayDriver.cs:116-119`, the maximum is set in `SiteService.cs:60`; the
  scene's `TypeAsync` is in `scripts/recorder/Scene.cs`);
  `PauseAsync(600)`; `WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button.save")))`. The tenant reloads its
  settings on save and the first request after it can be slow: wrap the wait in
  `scene.OffCameraAsync(...)`, with the condition "the text `Site settings updated successfully.` is visible", and in the same
  off-camera block warm up the front end with a request to `/categories/travel` made through
  `scene.Page.Context.APIRequest`, so `paging-3` does not wait on camera. Visible at the end: the Page size box holding `3` and the
  message "Site settings updated successfully." Source: the button, `OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`;
  the message, `OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`; the reload,
  `OrchardCore.Modules/OrchardCore.Settings/Drivers/DefaultSiteSettingsDisplayDriver.cs:140`.
- `paging-3`: `NavigateAsync("/categories/travel")`; wait until `header.masthead h1` has the text `Travel` and three
  `.post-preview` are present; `ScrollAsync(0, 700)` until the pager under the third summary is in view. Visible at the
  end: three summaries (posts 5, 4 and 3) and, under them, the pager (`ul.pagination`) with its link to the second page. Source:
  `OrchardCore.Modules/OrchardCore.Navigation/PagerShapesTableProvider.cs:179` (the pager appears from two pages),
  `Category-TermPart.liquid:13`.
- `paging-4`: `ClickAndWaitForNavigationAsync(scene.Page.Locator("ul.pagination a", new() { HasTextRegex = new Regex("^2$") }))`
  (if the pager shows no page numbers, the link to the next page); wait until three `.post-preview` are present; scroll with
  `ScrollAsync(0, 450)` until they are in view. Visible at the end: the second page, with posts 2 and 1 and the recipe post.
  Source: `OrchardCore/OrchardCore.Taxonomies.Core/DefaultContentsTaxonomyListQueryService.cs:49-50` (skip and take by page).

### The slide steps

The slides are drawn by `slides.py`; there is nothing to record.

- `idea-1`: the first card, "One page for each term", is bright; the other two are dimmed.
- `idea-2`: the second card, "An address of its own: /categories/travel", is bright.
- `idea-3`: the third card, "A list of its content", is bright.
- `needs-1`: the card "Route contained items" is bright, with its text that names the Autoroute feature; the other three are dimmed.
- `needs-2`: the card "A published taxonomy" is bright.
- `needs-3`: the card "Manage contained item routes" is bright.
- `needs-4`: the card "A taxonomy field" is bright.

Sources behind the cards: the Autoroute feature (its name in the Features list is "Autoroute", `OrchardCore.Modules/OrchardCore.Autoroute/Manifest.cs:4`;
the Taxonomies feature does not depend on it, `OrchardCore.Modules/OrchardCore.Taxonomies/Manifest.cs:10-16`) provides the Permalink and Route contained items
boxes (`AutoroutePart.Edit.cshtml:30,94-100`) and the routes themselves (`AutorouteEntries.cs`); the address and the list, `OrchardCore/OrchardCore.Autoroute.Core/Indexes/AutoroutePartIndex.cs:161-175` and
`DefaultContentsTaxonomyListQueryService.cs:34`; only published taxonomies are routed,
`OrchardCore/OrchardCore.Autoroute.Core/Services/AutorouteEntries.cs:181`; the term's own Permalink is used only with
Manage contained item routes, `OrchardCore.Modules/OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:172`, otherwise the address
ends with the identifier of the term and its name, `OrchardCore.Modules/OrchardCore.Autoroute/Handlers/DefaultRouteContentHandler.cs:22-30`.

### The cards

`intro` (chapter 00) and `outro` (chapter 99) are cards drawn by the assembler: nothing to record.

## 4. Checks for private data

Before the clips go to `assemble.py`, and again on the finished video:

- Every name on screen is one of the invented ones of the seed table, `admin`, `Travel`, `Earth`, `Exploration`, `Space`.
  The only email address that can appear is `admin@example.com`.
- No password is typed or shown: the sign-in is done outside the capture, through `state.json`.
- The base URL (`localhost`) is not in the frame: the recording has no address bar, and the only addresses on screen are
  the captions `/categories/travel` and the Permalink boxes.
- Look at every contact sheet (`review.py`): no error page, no empty list, no post shown twice, no draft badge.
- OCR the video at one frame per second and at every scene change, then at five frames per second, as the skill's step 6 says.

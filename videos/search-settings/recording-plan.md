# Recording plan: Site Search Settings

How to record `search-settings` without deciding anything. Orchard Core source references are `file:line` in
`OrchardCMS/OrchardCore` at commit `92db4a83` (paths under `src/`). A selector below stands for
`scene.Page.Locator(...)`; labels are written as the code spells them. Narrate first (`narrate.py`), because every
step is held for the length of its narration.

Clips are four recordings (`02-default`, `03-search`, `05-placeholder`, `07-permissions`) and three groups of slides
(`01-overview`, `04-results`, `06-address`). The two cards (`intro`, `outro`) need nothing from the site.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout at `92db4a83` (or a later
  commit with the same labels), on `http://localhost:5010`.
- **Data folder**: `ORCHARD_APP_DATA` points to a new empty folder outside both repositories.
- **Auto Setup** (no Setup screen is recorded, this video is not about it): tenant `Default`, recipe `Blog`, site name
  `Orchard Core`, time zone `America/Los_Angeles`, database `Sqlite`, user `admin`, email `admin@example.com`, password
  `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git). See the `orchardcore-tester` skill,
  `references/autosetup.md`, for the environment variables.
- **Outbound calls blocked**: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The Blog theme asks Google Fonts
  for fonts; with the proxy it falls back to the system fonts, which is fine.
- **Sign-in**: once, outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, which
  saves `state.json` (git-ignored). Every clip runs signed in as `admin`.
- **External services**: none. Lucene keeps its index files under the site's data folder, so there is no stand-in.
- **Theme**: `TheBlogTheme` (what the Blog recipe selects). Its `Search-Form.liquid`, `Search-List.liquid` and
  `Search-Results.liquid` draw the search page (`OrchardCore.Themes/TheBlogTheme/Views`), not the module's own views.

### Features to enable before recording

`Search` (`OrchardCore.Search`) and `Lucene` (`OrchardCore.Lucene`). The seed recipe below enables both. Nothing else
is switched on: the Blog recipe already enables `Indexing`, `Deployment`, `Roles` and `Layers`.

### Settings that must be in their starting state

- **Settings, Search, Site Search** has **no default index and no placeholder**. The seed recipe must not contain a
  `Settings` step with `SearchSettings`.
- **Roles**: the `Anonymous` role has no `Query` permission (the Blog recipe grants it none).
- **Settings, General**: Page size is left at its default.

### The exact seed (made outside the capture)

1. Sign in in a normal browser (or with `Recorder probe`) and open **Tools, Deployments, JSON Import**
   (`/Admin/DeploymentPlan/Import/Json`, `OrchardCore.Deployment/AdminMenu.cs:30`).
2. Paste this recipe in the **JSON** box and select **Import**. It enables the two features, adds three fake blog
   posts to the one the Blog recipe creates, creates the two Lucene indexes **Posts** (blog posts) and **Pages**
   (articles), and rebuilds them. The two index definitions copy `blog.lucene.search.recipe.json` of the Blog theme.

```json
{
  "steps": [
    {
      "name": "feature",
      "enable": [
        "OrchardCore.Search",
        "OrchardCore.Lucene"
      ]
    },
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "[js: uuid()]",
          "ContentType": "BlogPost",
          "DisplayText": "Exploration notes from a week in the mountains",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "TitlePart": { "Title": "Exploration notes from a week in the mountains" },
          "MarkdownBodyPart": {
            "Markdown": "We packed light, followed the river upstream and let the weather decide each day. Exploration is mostly patience, and these notes record what worked and what did not."
          },
          "AutoroutePart": { "Path": "blog/mountain-week" },
          "BlogPost": { "Subtitle": { "Text": "Ten days of maps, boots and slow mornings" } }
        },
        {
          "ContentItemId": "[js: uuid()]",
          "ContentType": "BlogPost",
          "DisplayText": "Why exploration begins with a good map",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "TitlePart": { "Title": "Why exploration begins with a good map" },
          "MarkdownBodyPart": {
            "Markdown": "A paper map shows the shape of a valley before you reach it. Good exploration starts at the kitchen table, with a pencil and a plan for the first three hours."
          },
          "AutoroutePart": { "Path": "blog/good-map" },
          "BlogPost": { "Subtitle": { "Text": "Plan the first three hours, then wander" } }
        },
        {
          "ContentItemId": "[js: uuid()]",
          "ContentType": "BlogPost",
          "DisplayText": "A short guide to exploration on foot in a new city",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "TitlePart": { "Title": "A short guide to exploration on foot in a new city" },
          "MarkdownBodyPart": {
            "Markdown": "Walk for one hour without a destination, then follow the street that looks quietest. Exploration on foot turns a city into a set of small neighborhoods."
          },
          "AutoroutePart": { "Path": "blog/city-on-foot" },
          "BlogPost": { "Subtitle": { "Text": "One hour, no destination" } }
        }
      ]
    },
    {
      "name": "CreateOrUpdateIndexProfile",
      "indexes": [
        {
          "IndexName": "Posts",
          "ProviderName": "Lucene",
          "Type": "Content",
          "Name": "Posts",
          "Properties": {
            "LuceneIndexMetadata": { "StoreSourceData": false, "AnalyzerName": "standardanalyzer" },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": [ "BlogPost" ], "Culture": "any" },
            "LuceneIndexDefaultQueryMetadata": {
              "DefaultVersion": "LUCENE_30",
              "QueryAnalyzerName": "standardanalyzer",
              "AllowLuceneQueries": false,
              "DefaultSearchFields": [ "Content.ContentItem.FullText" ]
            }
          },
          "IndexLatest": false,
          "Culture": "any",
          "IndexedContentTypes": [ "BlogPost" ]
        },
        {
          "IndexName": "Pages",
          "ProviderName": "Lucene",
          "Type": "Content",
          "Name": "Pages",
          "Properties": {
            "LuceneIndexMetadata": { "StoreSourceData": false, "AnalyzerName": "standardanalyzer" },
            "ContentIndexMetadata": { "IndexLatest": false, "IndexedContentTypes": [ "Article" ], "Culture": "any" },
            "LuceneIndexDefaultQueryMetadata": {
              "DefaultVersion": "LUCENE_30",
              "QueryAnalyzerName": "standardanalyzer",
              "AllowLuceneQueries": false,
              "DefaultSearchFields": [ "Content.ContentItem.FullText" ]
            }
          },
          "IndexLatest": false,
          "Culture": "any",
          "IndexedContentTypes": [ "Article" ]
        }
      ]
    },
    {
      "name": "RebuildIndex",
      "includeAll": false,
      "indexNames": [ "Posts", "Pages" ]
    }
  ]
}
```

3. Wait for the rebuild (it runs in the background after the request). Then check, **without saving anything**:
   - **Search, Indexes** (`/Admin/indexing`) lists **Posts** and **Pages**.
   - `/search` shows the warning *No default search index has been configured.* (no default index yet).
   - `/search/Posts?terms=exploration` lists four posts: the Blog recipe's *Man must explore, and this is exploration at
     its greatest* and the three posts above. `/search/Posts?terms=zebra` shows *There are no such results.*
     `/search/Pages?terms=about` lists the *About* article.
   - If a search returns nothing after a minute, select the index on **Search, Indexes** and apply the **Rebuild** bulk
     action (`OrchardCore.Indexing/Controllers/AdminController.cs`, `IndexingEntityAction.Rebuild`), then check again.
   - **Settings, Search, Site Search** shows the **Default search index** list with **Pages** and **Posts**, and an
     empty **Placeholder**.
4. Stop the site and copy its data folder and `state.json` aside as the **seed snapshot**.

All text above is invented: no real people, companies, e-mail addresses or domains. The only user name on screen is
`admin`.

## Order of the clips

Record in this order. Each clip starts from the state the previous recording left, except where a snapshot is named.

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-default` | the **seed snapshot** (no default index) | yes: the default index becomes **Posts** |
| 2 | `03-search` | the state after `02-default` | no |
| 3 | `05-placeholder` | the state after `02-default` | yes: the **Placeholder** becomes *Search the blog* |
| 4 | `07-permissions` | the state after `05-placeholder` (any state with the default index set works) | yes: `Anonymous` gets **Query 'Posts' Index** |

- After recording `02-default`, stop the site and copy its data folder aside as the **default-set snapshot**.
- `03-search` and `05-placeholder` both start from the **default-set snapshot**: restore it before `05-placeholder`
  (the placeholder must be empty at its start).
- To record one clip again, restore the snapshot named in the table, start the site, and record only that clip.
- The slides (`01-overview`, `04-results`, `06-address`) and the cards are drawn from `slides.py` and the storyboard;
  they depend on no site state.

## Clips

### `02-default` (chapter 02, Choose the default index)

Starts on: `/search`, opened with `scene.OpenAsync("/search")` before the capture, from the **seed snapshot**. The page
shows the Blog theme's navigation bar and the warning. It has no masthead and no search box, because the controller
returns the view without a model and the theme's `Search-List` template is not drawn
(`OrchardCore.Search/Controllers/SearchController.cs:97`).

- `default-1`
  - Actions: highlight the warning with `HighlightAsync(scene.Page.Locator(".message-warning").Filter(new() { HasText = "No default search index has been configured." }), "No default index")`;
    pause until the narration ends. (On the front end the warning is not a toast and not an `.alert`: it is a
    `div.message.message-warning` with `role="alert"`, drawn by the core `Message` shape in the `Messages` zone of the
    Blog layout, `TheBlogTheme/Views/Layout.liquid:39`; the Blog theme has no `Message` template, and styles
    `.message-warning` like a Bootstrap alert, `TheBlogTheme/Assets/TheBlogTheme/src/scss/modules/_messages.scss`;
    `OrchardCore.DisplayManagement/Shapes/CoreShapes.cs:122-131`.)
  - On screen at the end: the search page with the warning text *No default search index has been configured.*
    ringed. No search box is expected on this page.
  - Label: `OrchardCore.Search/Controllers/SearchController.cs:95`.
- `default-2`
  - Actions: `ClearHighlightsAsync()`; `NavigateAsync("/Admin")`; click **Settings** in the admin menu (`#settings`, the
    group heading, `OrchardCore.Settings/AdminMenu.cs:36-37`); click **Search** inside the **Settings** group (not the
    top-level **Search** group, which holds **Indexes**); click **Site Search** (`#searchsettings a`,
    `OrchardCore.Search/AdminMenu.cs:27-31`) with `ClickAndWaitForNavigationAsync`. If a locator cannot find an entry,
    inspect the menu with `Recorder probe /Admin "() => document.querySelector('#left-nav').outerHTML"`
    (`MSYS_NO_PATHCONV=1` in Git Bash). The three clicks and the page load must fit in about 8 seconds: use
    `pauseAfter: 300` on the clicks. The page must be `/Admin/Settings/search`.
  - On screen at the end: the **Site Search** settings page with the fields **Default search index** (showing
    *Select a default index*) and **Placeholder** (empty), the admin menu open on **Settings, Search, Site Search**.
  - Labels: `OrchardCore.Search/Views/SearchSettings.Edit.cshtml:6,9,17`; menu `OrchardCore.Search/AdminMenu.cs:25-27`.
- `default-3`
  - Actions: `MoveToAsync` the **Default search index** list (`scene.Page.GetByLabel("Default search index")`);
    `SelectAsync(thatList, "Posts")`; `HighlightAsync` the hint under the list
    (`scene.Page.GetByText("The default index to use for the search page.")`).
  - On screen at the end: the list shows **Posts**, the hint *The default index to use for the search page.* is ringed.
  - Labels: `SearchSettings.Edit.cshtml:6,12`. The list holds every index profile name
    (`OrchardCore.Search/Drivers/SearchSettingsDisplayDriver.cs:51`).
- `default-4`
  - Actions: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save"))` (the **Save**
    button, `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`); wait until
    `scene.Page.Locator(".toast-body")` has the text *Site settings updated successfully.*; pause.
  - On screen at the end: the settings page reloaded with **Posts** selected. The success toast
    *Site settings updated successfully.* was on screen during the second sentence; if it has faded by the end of
    the step, look at the contact sheet at the middle of the step to confirm it appeared.
  - Label: `OrchardCore.Settings/Controllers/AdminController.cs:119`.

### `03-search` (chapter 03, Try the search page)

Starts on: `/` (`scene.OpenAsync("/")` before the capture, the Blog home page), from the state after `02-default`.
The narration of `search-1` says "Let's open the search page", so the clip opens it.

- `search-1`
  - Actions: `NavigateAsync("/search")`, which shows the masthead *Search*, an empty search box and the **Search**
    button; `MoveToAsync` the search box (`input[name='Terms']`); `HighlightAsync(thatBox, "Standard text")`; pause.
  - On screen at the end: the empty box showing the grey text *Enter your search term(s)*, ringed.
  - Label: `TheBlogTheme/Views/Search-Form.liquid:9` (the module's own view has the same text,
    `OrchardCore.Search/Views/Search-Form.cshtml:10`).
- `search-2`
  - Actions: `ClearHighlightsAsync()`; `TypeAsync(input[name='Terms'], "exploration")`; click the **Search** button
    (`scene.Page.GetByRole(AriaRole.Button, new() { Name = "Search", Exact = true })`) with
    `ClickAndWaitForNavigationAsync`; pause.
  - On screen at the end: the results list (`ul.list-group`) with four posts, each with its title, subtitle and
    *Posted by admin on <date>*; the search box still holds *exploration*.
  - Label: `TheBlogTheme/Views/Search-Form.liquid:17` (the **Search** button); the summaries are drawn by
    `TheBlogTheme/Views/Content-BlogPost.Summary.liquid`.
- `search-3`
  - Actions: `await scene.Page.Locator("input[name='Terms']").FillAsync("")` to empty the box; `TypeAsync(thatBox, "zebra")`;
    click **Search** with `ClickAndWaitForNavigationAsync`; pause.
  - On screen at the end: the box holds *zebra*, and below it the yellow alert *There are no such results.*
  - Label: `TheBlogTheme/Views/Search-Results.liquid:23`; `OrchardCore.Search/Views/Search-Results.cshtml:43`.

### `05-placeholder` (chapter 05, Set the placeholder)

Starts on: `/Admin/Settings/search` (`scene.OpenAsync`), from the **default-set snapshot**. The page shows **Posts**
selected and an empty **Placeholder**.

- `placeholder-1`
  - Actions: `TypeAsync(scene.Page.GetByLabel("Placeholder", new() { Exact = true }), "Search the blog")`; press Tab so
    the field loses focus; `HighlightAsync(thatField)`; pause.
  - On screen at the end: the **Placeholder** field holding *Search the blog*, ringed.
  - Labels: `SearchSettings.Edit.cshtml:17,20` (the hint is *The placeholder text to use in the search bar.*).
- `placeholder-2`
  - Actions: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save"))`;
    `NavigateAsync("/search")`; pause.
  - On screen at the end: `/search` with the empty search box showing the grey text *Search the blog*.
  - Label: `TheBlogTheme/Views/Search-Form.liquid:11`. That line uses the Liquid filter `d` of Data Localization, which the
    seed does not enable: check this step on the site before recording (`OPEN-QUESTIONS.md`, point 11).

### `07-permissions` (chapter 07, Who can search)

Starts on: `/Admin` (the admin home page, `scene.OpenAsync("/Admin")`), from the state after `05-placeholder`.

- `permissions-1`
  - Actions: click **Access Control** in the admin menu (the group heading; it has no id, find it by its text);
    click **Roles** (`#roles a`) with `ClickAndWaitForNavigationAsync`; `HighlightAsync` the **Administrator** name
    link in the list (`scene.Page.GetByRole(AriaRole.Link, new() { Name = "Administrator", Exact = true })`, a plain
    link, `RoleEntry.Fields.SummaryAdmin.cshtml`); pause. The role itself is not opened: its page lists no permissions
    but says that this system role grants all available permissions (`_RolePermissions.cshtml:32`).
  - On screen at the end: the **Roles** list with *Administrator*, *Anonymous* and the other roles; *Administrator*
    ringed.
  - Labels: `OrchardCore.Roles/AdminMenu.cs:18-19`; the permission names `OrchardCore.Search.Abstractions/SearchPermissions.cs:8`
    (**Manage Search Settings**) and `OrchardCore.Indexing.Core/IndexingPermissions.cs:10` (**Query any index**);
    the Administrator stereotype `OrchardCore.Search/Permissions.cs:21`.
- `permissions-2`
  - Actions: `ClearHighlightsAsync()`; click the **Edit** button of the **Anonymous** row
    (`scene.Page.Locator("a.edit[href$='/Roles/Edit/Anonymous']")`) with `ClickAndWaitForNavigationAsync`;
    `TypeAsync(scene.Page.Locator("#search-box"), "Query")`; pause. The **Edit** button is an `a` element with
    `role="button"` (`OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`), so a locator built with
    `AriaRole.Link` does not find it; use `AriaRole.Button` if a role locator is preferred. The page is
    `/Admin/Roles/Edit/Anonymous` (`OrchardCore.Roles/Controllers/AdminController.cs:22`).
  - On screen at the end: the page titled *Edit 'Anonymous' Role*, the **Search permissions** box holding *Query*, and
    the list narrowed to the permissions with *Query* in their name, including **Query any index**,
    **Query 'Pages' Index** and **Query 'Posts' Index**, each with an empty **Allow** box.
  - Labels: `OrchardCore.Roles/Views/Admin/Edit.cshtml:6`; `_RolePermissions.cshtml:49` (**Search permissions**);
    `_RolePermissions.cshtml:69` (**Allow**); `IndexingPermissions.cs:15` (**Query '{0}' Index**).
- `permissions-3`
  - Actions: `HighlightAsync` the row **Query any index** (`tr[data-text="Query any index"]`); pause one second;
    `ClearHighlightsAsync()`; `HighlightAsync` the row **Query 'Posts' Index** (`tr[data-text="Query 'Posts' Index"]`);
    `ClickAsync` its **Allow** check box (`[id="Checkbox.QueryIndex_Posts"]`).
  - On screen at the end: the **Allow** box of **Query 'Posts' Index** checked, the row ringed; **Query 'Pages' Index**
    and **Query any index** unchecked.
  - Labels: as `permissions-2`.
- `permissions-4`
  - Actions: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save"))` (the
    **Save** button of the role); wait until the toast has the text *Role updated successfully.*
  - On screen at the end: the **Roles** list with the toast *Role updated successfully.*
  - Label: `OrchardCore.Roles/Controllers/AdminController.cs:213`.

## Slides

One image per step, drawn by `slides.py`; nothing to record.

- `overview-1`: the heading *The page, the index and the settings* and three cards; the first, *Search page*, is bright.
- `overview-2`: the same slide with *Search index* bright.
- `overview-3`: the same slide with *Site Search settings* bright.
- `results-1`: the heading *What the results page shows* and three cards; *Summary* is bright.
- `results-2`: the same slide with *Paging* bright.
- `results-3`: the same slide with *Highlights* bright.
- `address-1`: the heading *The address decides the index* and three cards, all bright: */search*, */search/Pages*, and
  an index name that does not exist.
- `address-2`: the heading *The Search Form widget* and three cards, all bright: *Search Form widget*, *Index Name*,
  *Placeholder*.

## Checks for private data

- The only user name on screen is `admin` (*Posted by admin*). No e-mail address, password or token is shown by any
  clip: `admin@example.com` and `<password>` are typed only outside the capture.
- The site name in the Blog header is *Orchard Core*; the footer reads *Copyright © Your Website 2021* (the Blog
  recipe's own text). The post texts are the invented ones of the seed.
- The address bar is not part of the recording; no clip shows a local machine name or path.
- OCR the finished clips at one frame per second and at every scene change, as the skill describes, and search for
  `@`, digits that look like phone numbers, and any name that is not in this plan.

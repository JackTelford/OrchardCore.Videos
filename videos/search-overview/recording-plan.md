# Recording plan: search-overview

How to record this video without deciding anything. Read `OPEN-QUESTIONS.md` first: it lists what the author could
not check on a running site, and what to confirm with `Recorder probe` before a take.

Paths such as `OrchardCore.Search/AdminMenu.cs:27` are relative to `src/OrchardCore.Modules/` (modules) or
`src/OrchardCore/` (libraries) of `OrchardCMS/OrchardCore`. A selector written in this plan stands for
`scene.Page.Locator(...)`, and `Label("...")` stands for `scene.Page.GetByLabel("...", new() { Exact = true })`.
Locators whose id contains a dot are written as attribute selectors, because a dot in a CSS id needs escaping.

## What the video shows

| Chapter | Clip | Kind | Steps |
|---|---|---|---|
| 00 | `00-intro` | card | `intro` |
| 01 | `01-how` | slides | `how-1`, `how-2`, `how-3` |
| 02 | `02-features` | recording | `features-1`, `features-2`, `features-3` |
| 03 | `03-index` | recording | `index-1`, `index-2`, `index-3`, `index-4` |
| 04 | `04-settings` | recording | `settings-1`, `settings-2` |
| 05 | `05-permission` | recording | `permission-1`, `permission-2` |
| 06 | `06-widget` | recording | `widget-1`, `widget-2` |
| 07 | `07-visitor` | recording, signed out | `visitor-1`, `visitor-2`, `visitor-3`, `visitor-4` |
| 08 | `08-maintain` | slides | `maintain-1` |
| 99 | `99-outro` | card | `outro` |

## The site it needs

This video is recorded on a site that is already set up, so Auto Setup provisions it and no Setup screen or sign-in
form is recorded. It is not the Setup video: a `state.json` is used for every clip except `07-visitor`.

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore` on `main` (the labels were read at commit `92db4a83`).
  Run `src/OrchardCore.Cms.Web` on `http://localhost:5010` with its own empty `App_Data` folder outside both
  repositories (`ORCHARD_APP_DATA`), as described by the `orchardcore-tester` skill of that repository.
- **Auto Setup** (environment variables; the password comes from `VIDEO_PASSWORD`, chosen outside git, never typed
  into this repository):

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"   # <password>
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

- **Tenant**: Default only.
- **Recipe**: `Blog` (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). It enables Indexing, Layers, Widgets,
  Roles, Markdown and Deployment, and sets the zones `Content` and `Footer` and the layers `Always` and `Homepage`.
  It does not enable Search or Lucene: **both must be disabled when the take starts**, because clip `02-features`
  enables them on camera. Check on `/Admin/Features` that Search and Lucene have an Enable button.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (`http://127.0.0.1:9`) for the site process, as
  the recorder reference asks. The Blog theme's layout links Google Fonts (`TheBlogTheme/Views/Layout.liquid`): the
  browser, not the site, requests them, so the proxy variables of the site do not change that. Online they load and
  change nothing that matters; offline the page falls back to the system fonts. Either is fine.
- **External services**: none. Lucene keeps its index in files in the tenant's application data
  (`OrchardCore.Lucene.Core/LuceneIndexStore.cs:45-49`).
- **Time zone and culture**: `America/Los_Angeles`, `en-US` (the recorder's default).
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, once, before the seed. The result,
  `state.json`, is git-ignored. No password is ever typed on camera.

### The seed data (created outside the capture, once)

Three fake blog posts are added to the Blog that the recipe creates, so a search has something to find. The text is
invented. Two of the three posts contain the word `harvest`; none contains `zebra`; the stock post and the About page of
the recipe contain neither.

| Title | Subtitle | Contains "harvest" |
|---|---|---|
| Planning the Autumn Harvest Festival | Stalls, music and a long table under the apple trees | yes (title and body) |
| A Short History of Our Orchard | Eighty trees, four varieties and one stubborn ladder | yes (body) |
| Winter Baking With Preserved Fruit | Jars from the cellar become pies | no |

1. In the admin menu, open **Blog** (an entry the Blog recipe adds). The address ends with
   `/Admin/Contents/ContentItems/<id>/Display`: copy `<id>`, the content item id of the Blog.
2. Open **Tools**, **Deployments**, **JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:30`), paste the following with
   `<blog-id>` replaced by the id from step 1 (three times), and select Import.

   ```json
   {
     "steps": [
       {
         "name": "content",
         "data": [
           {
             "ContentItemId": "4qz7m2k9d1x6c3v8b5n0r4t7ya",
             "ContentType": "BlogPost",
             "DisplayText": "Planning the Autumn Harvest Festival",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": { "Title": "Planning the Autumn Harvest Festival" },
             "ContainedPart": { "ListContentItemId": "<blog-id>", "Order": 1 },
             "AutoroutePart": { "Path": "blog/planning-the-autumn-harvest-festival" },
             "MarkdownBodyPart": {
               "Markdown": "The harvest festival returns to Maple Lane this October. Volunteers are building stalls for apples, honey and bread, and a band will play beside the long table under the trees.\n\nBring a basket, a chair and an appetite."
             },
             "BlogPost": { "Subtitle": { "Text": "Stalls, music and a long table under the apple trees" } }
           },
           {
             "ContentItemId": "4wd3p8h1j6s2f9g5k7l0e4u3ib",
             "ContentType": "BlogPost",
             "DisplayText": "A Short History of Our Orchard",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": { "Title": "A Short History of Our Orchard" },
             "ContainedPart": { "ListContentItemId": "<blog-id>", "Order": 2 },
             "AutoroutePart": { "Path": "blog/a-short-history-of-our-orchard" },
             "MarkdownBodyPart": {
               "Markdown": "The orchard began with eighty apple trees planted along the river. Every autumn the harvest was carried to the cellar by hand, and the oldest ladder is still in use.\n\nToday the trees give four varieties of apple, from early green fruit to late red ones."
             },
             "BlogPost": { "Subtitle": { "Text": "Eighty trees, four varieties and one stubborn ladder" } }
           },
           {
             "ContentItemId": "4mb5t1y8c2v6x9n3z7a0q4r2ek",
             "ContentType": "BlogPost",
             "DisplayText": "Winter Baking With Preserved Fruit",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": { "Title": "Winter Baking With Preserved Fruit" },
             "ContainedPart": { "ListContentItemId": "<blog-id>", "Order": 3 },
             "AutoroutePart": { "Path": "blog/winter-baking-with-preserved-fruit" },
             "MarkdownBodyPart": {
               "Markdown": "Jars of plum jam and baked apple fill the pantry shelves. This recipe turns preserved fruit into a pie that keeps the warmth of a summer afternoon.\n\nServe it warm, with cream."
             },
             "BlogPost": { "Subtitle": { "Text": "Jars from the cellar become pies" } }
           }
         ]
       }
     ]
   }
   ```

3. Open `/blog`: four posts are listed (the three above and the post of the recipe). If the import fails, create the
   three posts by hand with **Blog**, then the button that adds a Blog Post, using the same title, subtitle and
   Markdown text.
4. Create the posts **before** clip `03-index`: the Indexing feature records a task for every content change
   (`OrchardCore.Indexing/CreateIndexingTaskContentHandler.cs:31-37,49`), and the index created on camera catches up from
   that log.

Nothing else is seeded. The video is recorded with the admin menu in its default state.

## Order, snapshots and dependencies

Record in the order of the chapters; each clip starts from the state the previous one left.

| Clip | Needs | Changes the site | Snapshot to take afterwards |
|---|---|---|---|
| `02-features` | seed, Search and Lucene disabled | enables the features Lucene and Search | `snap-02` |
| `03-index` | `snap-02` | creates the index `website` | `snap-03` (after the check below) |
| `04-settings` | `snap-03` | saves the Site Search settings | `snap-04` |
| `05-permission` | `snap-04` | adds a permission to the Anonymous role | `snap-05` |
| `06-widget` | `snap-05` | adds the Search Form widget | `snap-06` |
| `07-visitor` | `snap-06`, no sign-in | nothing | none |

A snapshot is a copy of the whole `App_Data` folder (it holds the database and the Lucene files) made while the site is
stopped. Before the first take, also copy `App_Data` right after the seed as `snap-00`. To record a clip again, stop
the site, restore the snapshot named in "Needs", and start it again. To record everything again, restore `snap-00`.

Checks to run off camera, as admin, before the snapshot named in brackets:

- After `03-index` (`snap-03`): open `/search/website?Terms=harvest`. It lists "Planning the Autumn Harvest Festival" and
  "A Short History of Our Orchard", and not "Winter Baking With Preserved Fruit". Open `/search/website?Terms=zebra`: it
  shows "There are no such results." If nothing is listed, wait one minute and reload; if it is still empty, open
  **Search**, **Indexes**, the Actions menu of the `website` row, select Synchronize and confirm with Yes, then check
  again; as a last resort use Rebuild in the same menu.
- After `04-settings` (`snap-04`): open `/search?Terms=harvest` (no index in the address): same two posts.
- After `05-permission` (`snap-05`): in a private window that is not signed in, open `/search?Terms=harvest`: the same
  two posts, with no redirection to a sign-in page.
- After `06-widget` (`snap-06`): in the same private window, open `/`: the search box is at the bottom of the page.

## The recorder

Every clip but one is made with `NewSceneAsync(browser, directory, durations)`, which uses `state.json`. Clip
`07-visitor` must be signed out: create its scene with a state path that does not exist, so `Scene.StartAsync` passes no
storage state (`StorageStatePath = File.Exists(...) ? ... : null`):

```csharp
await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl,
    Path.Combine(Root, "no-sign-in.json"), Path.GetDirectoryName(directory), durations);   // that file must not exist
```

No `PrepareAsync` is needed: the seed is imported by hand once. Narrate first (`narrate.py`), then record.

## Clip `02-features`

**Starts on**: `/Admin/indexing` (Search, Indexes), opened with `scene.OpenAsync` before the capture. Behind the
dialog the list says "Nothing here! There are no indexes at the moment." (`OrchardCore.Indexing/Views/Admin/Index.cshtml:40`).

**Step `features-1`**

- Actions: `ClickAsync` on the button named `Add index` (`Index.cshtml:31`), pause 900 ms. Wait until
  `#modalAddIndex .bg-warning` is visible.
- On screen at the end: the "Available Sources" dialog (`Index.cshtml:61`) with the warning "You need to enable a
  feature which provides an index source." (`Index.cshtml:67`). `HighlightAsync` on `#modalAddIndex .bg-warning`.
- Labels: `Add index`, `Available Sources`, the warning: `OrchardCore.Indexing/Views/Admin/Index.cshtml:31,61,67`.

**Step `features-2`**

- Actions: `ClearHighlightsAsync`; `ClickAsync` on `#modalAddIndex` button `Cancel` (`Index.cshtml:90`);
  `NavigateAsync("/Admin/Features")`; wait for `#search-box`; `TypeAsync("#search-box", "Lucene", delay: 90)`; wait until
  `#btn-enable-OrchardCore_Lucene` is visible; let
  `row = scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-enable-OrchardCore_Lucene") })`
  (the row of the feature Lucene, the one the narration names), `HighlightAsync(row)`, pause 1200 ms,
  `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync("#btn-enable-OrchardCore_Lucene")`; wait until
  `#btn-disable-OrchardCore_Lucene` is attached.
- The filter `Lucene` leaves four rows (the filter matches the name, the id and the description): `Lucene`, `Lucene
  (Obsolete)`, `Lucene Worker` and `Lucene Content Picker` (`OrchardCore.Lucene/Manifest.cs`). Only the row `Lucene`
  (id `OrchardCore.Lucene`) is enabled; its button is the one with the id `btn-enable-OrchardCore_Lucene`. The other three
  stay as they are.
- On screen at the end: the Features page after the reload, with the filter cleared and a green message at the top that
  names the enabled feature (the text is "The feature Lucene was enabled." or lists Lucene with the features it
  needed).
- Labels: `Features` is under **Tools** (`OrchardCore.Features/AdminMenu.cs:26-27`); the filter box `#search-box`
  (`Features.cshtml:27`); the `Enable` link and its id (`FeatureEntry.Buttons.SummaryAdmin.cshtml:40`); the message
  (`OrchardCore.Features/Controllers/AdminController.cs:317`).

**Step `features-3`**

- Actions: `TypeAsync("#search-box", "frontend search", delay: 90)` (the narration says to filter for `frontend search`: the
  filter matches the name, the id and the description of a feature, and the Search feature's description is "Provides frontend search capabilities against indexes.":
  `OrchardCore.Search/Manifest.cs`, `FeatureEntryDisplayDriver.cs:20`); wait until `#btn-enable-OrchardCore_Search` is
  visible; `ClickAndWaitForNavigationAsync("#btn-enable-OrchardCore_Search")`; wait until
  `#btn-disable-OrchardCore_Search` is attached.
- On screen at the end: the Features page after the reload, with a green message that names the Search feature.
- Labels: as in `features-2`.

## Clip `03-index`

**Starts on**: `/Admin/indexing` (state `snap-02`: Search and Lucene enabled), opened before the capture.

**Step `index-1`**

- Actions: `ClickAsync` on `Add index`, pause 900 ms. The dialog has a heading `Lucene` and a card titled
  `Content in Lucene` with the text "Create an Lucene index based on site contents." (sic). Let
  `card = scene.Page.Locator("#modalAddIndex .card").Filter(new() { HasText = "Content in Lucene" })`;
  `HighlightAsync(card)`, pause 1500 ms, `ClearHighlightsAsync`, then
  `ClickAndWaitForNavigationAsync(card.GetByRole(AriaRole.Link, new() { Name = "Add" }))`.
- On screen at the end: the page "New 'Content in Lucene' index" with the fields Index Name and Name.
- Labels: `Add index`, `Available Sources`, `Add`: `OrchardCore.Indexing/Views/Admin/Index.cshtml:31,61` and
  `IndexProfile.Link.cshtml:29`; the provider heading `Lucene` (`OrchardCore.Lucene.Core/ServiceCollectionExtensions.cs:60`);
  `Content in Lucene` and its description (`OrchardCore.Lucene/Startup.cs:98-99`); the page title
  (`OrchardCore.Indexing/Views/Admin/Create.cshtml:6`).

**Step `index-2`**

- Actions: `TypeAsync(Label("Index Name"), "website", delay: 90)`. Typing in Index Name fills Name with the same text,
  lower case, because Name is still empty (`OrchardCore.Indexing/Assets/js/technical-name-generator.js`). Wait until
  `Label("Name")` has the value `website` (`Expect(...).ToHaveValueAsync("website")`). `HighlightAsync(Label("Index Name"))`
  and `HighlightAsync(Label("Name"))`.
- On screen at the end: both fields hold `website`, with their highlights.
- Labels: `Index Name`, `Name`: `OrchardCore.Indexing/Views/IndexProfileFields.Edit.cshtml:6,14`. The Index Name field is
  disabled after the index exists (`IndexProfileFields.Edit.cshtml:8`, `asp-is-disabled="!Model.IsNew"`).
- What the narration says about the two fields: Index Name is the name of the index in the provider (it becomes
  `IndexName`, and the provider's full name is built from it, `OrchardCore.Indexing/Drivers/IndexProfileDisplayDriver.cs`,
  `UpdateAsync`; `IndexProfile.IndexFullName` in `OrchardCore.Indexing.Abstractions/Models/IndexProfile.cs`). Name is the
  unique name of the profile (`IndexProfile.Name`, "A unique name for the index."): the list shows it
  (`IndexProfile.Fields.SummaryAdmin.cshtml`), the Site Search settings list it, the address `/search/{name}` and the
  permission `Query '{name}' Index` use it. The narration does not call either field "the technical name", because the
  script that fills Name (`technical-name-generator.js`) treats Name as the generated technical name of Index Name.

**Step `index-3`**

- Actions: `ClearHighlightsAsync`; `ClickAsync(Label("Blog Post"))` (a checkbox), `ClickAsync(Label("Article"))`; both
  must be checked. Then `HighlightAsync(Label("Culture"))`, whose select shows `Any culture`. If the Culture select and the
  checkboxes do not fit on screen together, use `ScrollAsync(0, -200)` before the highlight.
- On screen at the end: `Blog Post` and `Article` checked in Content types, and the Culture select, highlighted, on
  `Any culture`. The other options (Index Latest version, Analyzer, Store Source Data, Default Query Settings) are left
  as they are and are not named in the narration.
- Labels: `Culture`, `Any culture`, `Content types`, the hint "The content types to index. Choose at least one.":
  `OrchardCore.Indexing/Views/ContentIndexMetadata.Edit.cshtml:6,9,17,19`; the checkbox labels are the display names
  of the types, `Blog Post` and `Article` (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:132,189`;
  `OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml`).

**Step `index-4`**

- Actions: `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` on the button named `Save` (exact; `Create.cshtml:16`),
  after `MoveToAsync` brings it into view; wait until a `.list-group-item` containing `website` is visible.
- On screen at the end: the Indexes list with the green message "An index has been created successfully. The synchronizing
  process was triggered in the background.", and a row `website` with the labels `Lucene` and `Content`. The message
  is at the top of the page and the row below it; both must be visible (scroll to the top if needed).
- Labels: `Save` (`Create.cshtml:16`); the message (`OrchardCore.Indexing/Controllers/AdminController.cs:257`); the labels
  `Lucene` (provider) and `Content` (type) are the badges of `IndexProfile.DefaultTags.SummaryAdmin.cshtml`.

## Clip `04-settings`

**Starts on**: `/Admin/indexing` (state `snap-03`), opened before the capture.

**Step `settings-1`**

- Actions: `NavigateAsync("/Admin/Settings/search")` (the page that **Settings**, **Search**, **Site Search** opens:
  `OrchardCore.Search/AdminMenu.cs:25-27`); wait until `Label("Default search index")` is visible;
  `SelectAsync(Label("Default search index"), "website")`.
- On screen at the end: the select shows `website`; the field below it, `Placeholder`, is empty.
- Labels: `Default search index`, `Select a default index`, `Placeholder`: `OrchardCore.Search/Views/SearchSettings.Edit.cshtml:6,9,17`.
  The page has no field for a page title (`SearchSettings.Edit.cshtml` has only these two).

**Step `settings-2`**

- Actions: `TypeAsync(Label("Placeholder"), "Search articles and posts", delay: 60)`; press Tab (the value applies on blur);
  `ClickAndWaitForNavigationAsync` on the button named `Save` (`OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`).
- On screen at the end: the settings page after the reload, with the green message "Site settings updated successfully."
  (`OrchardCore.Settings/Controllers/AdminController.cs:119`), `website` selected and the placeholder text kept.

## Clip `05-permission`

**Starts on**: `/Admin/Settings/search` (state `snap-04`), opened before the capture.

**Step `permission-1`**

- Actions: `NavigateAsync("/Admin/Roles/Index")` (**Access Control**, **Roles**: `OrchardCore.Roles/AdminMenu.cs:18-19`; the
  address is `/Admin/Roles/Index`, because the controller route is `Roles/{action}/{id?}` with the action name filled in
  by `AdminAreaControllerRouteMapper.cs`, so the page is not at `/Admin/Roles`); wait until
  the Anonymous row is visible; `ClickAndWaitForNavigationAsync("a.edit[href$='/Roles/Edit/Anonymous']")`
  (`OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`).
- On screen at the end: the page "Edit 'Anonymous' Role" (`OrchardCore.Roles/Views/Admin/Edit.cshtml:6`) with the
  "Search permissions" box and the permission groups below it.

**Step `permission-2`**

- Actions: `TypeAsync("#search-box", "Query 'website' Index", delay: 60)` (the box is the "Search permissions" box,
  `_RolePermissions.cshtml:49`; the narration reads the label with its quotes. The filter shows a row when the whole text is
  part of the description, or when every word of the text is: `list-management.js`, `matchesSearchText`. The quotes make the
  Lucene permission "Query Lucene website Index" fail the second test, so it stays out of the list).
  Wait until exactly one permission row is visible. `ClickAsync("[id='Checkbox.QueryIndex_website']")` and check that it
  is checked; pause 900 ms. Then `ClickAndWaitForNavigationAsync("button.save")` (the `Save` button,
  `Edit.cshtml:33`).
- On screen at the end: the Roles list with the green message "Role updated successfully."
  (`OrchardCore.Roles/Controllers/AdminController.cs:213`). The ticked box is on screen in the middle of the step.
- Labels: the permission "Query '{0}' Index" with the name of the index (`OrchardCore.Indexing.Core/IndexingPermissions.cs:15`;
  its name is `QueryIndex_website`, from the `Name` of the index); "Query any index" (`IndexingPermissions.cs:10`), which
  implies it (`IndexingPermissions.cs:15`, `QuerySearchIndex` in the implied-by list).
- The search page asks for this permission with the index as the resource
  (`OrchardCore.Search/Controllers/SearchController.cs:110`).

## Clip `06-widget`

**Starts on**: `/Admin/Roles/Index` (state `snap-05`), opened before the capture.

**Step `widget-1`**

- Actions: `NavigateAsync("/Admin/Layers")` (**Design**, **Widgets**: `OrchardCore.Layers/AdminMenu.cs:26-27`; the page is titled
  "Widgets and Layers"); wait until the Footer zone card is visible. Let
  `zone = scene.Page.Locator(".layer-zone[data-zone='Footer']")` (`OrchardCore.Layers/Views/Admin/Index.cshtml:50`);
  `HighlightAsync(zone, "Footer")`, pause 1500 ms, `ClearHighlightsAsync`; `ClickAsync(zone.GetByRole(AriaRole.Button,
  new() { Name = "Add Widget" }))` (`Index.cshtml:85`); `ClickAndWaitForNavigationAsync(zone.Locator("a.add-list-widget[data-widget-type='SearchForm']"))`
  (`Index.cshtml:96-97`; the item text is `Search Form`).
- On screen at the end: the editor of a Search Form widget, with the fields Title, Render title, The layer, Placeholder
  and Index Name.
- Labels: the display name `Search Form`: `OrchardCore.Search/Migrations/SearchMigrations.cs:23`.

**Step `widget-2`**

- Actions: `TypeAsync(Label("Title"), "Site search", delay: 60)`; `SelectAsync(Label("The layer"), "Always")`;
  `TypeAsync(Label("Placeholder"), "Search this site", delay: 60)`; press Tab; `HighlightAsync(Label("Index Name"))`
  (left empty), pause 1500 ms, `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync("button.publish")`
  (the `Publish` button, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`). The editor was opened with a
  `returnUrl`, so the view draws a button group: `button[name='submit.Publish']` would match two buttons (`Publish` and
  the "and continue" item of its menu, `:24`) and Playwright would refuse the click. `button.publish` is only the first.
- On screen at the end: back on "Widgets and Layers", with the green message "Your Search Form has been published."
  (`OrchardCore.Contents/Controllers/AdminController.cs:473`) and the widget `Site search` in the Footer zone with the layer
  `Always`.
- Labels: `Title`, `Render title`, `The layer`, `Select a layer`: `OrchardCore.Layers/Views/LayerMetadata.Edit.cshtml:4,15,22,25`;
  `Placeholder` (required) and `Index Name` with the hint "Leave it empty to use the default index.":
  `OrchardCore.Search/Views/SearchFormPart.Edit.cshtml:4,12,15`; the layer `Always` comes from the recipe
  (`blog.recipe.json`, step `layers`).

## Clip `07-visitor`

**Starts on**: `/` (state `snap-06`), signed out (see "The recorder"), opened before the capture, and scrolled to the top
with `scene.Page.EvaluateAsync("window.scrollTo(0, 0)")`: the search box has `autofocus`
(`OrchardCore.Search/Views/SearchFormPart.cshtml:8`), so the browser may have scrolled the page to it on load.

**Step `visitor-1`**

- Actions: `ScrollAsync(0, 3000)` until the box in the footer is on screen (a wheel that cannot go further is harmless);
  `box = scene.Page.Locator("footer input[name='Terms']")`; `TypeAsync(box, "harvest", delay: 90)`;
  `ClickAndWaitForNavigationAsync(scene.Page.Locator("footer").GetByRole(AriaRole.Button, new() { Name = "Search" }))`
  (`SearchFormPart.cshtml:9`).
- On screen at the end: the search page, with the term `harvest` in the search box. The footer box showed the
  placeholder `Search this site` before the text was typed.

**Step `visitor-2`**

- Actions: scroll to the top (`ScrollAsync(0, -3000)`), then `ScrollAsync(0, 350)` so the search box and the results are
  both visible (adjust with `Recorder probe`); `HighlightAsync(scene.Page.Locator("ul.list-group"))`; `MoveToAsync` the
  title of the first result.
- On screen at the end: two results, "Planning the Autumn Harvest Festival" and "A Short History of Our Orchard", each with its
  subtitle. The order may vary; do not narrate it.
- Labels: the results list is `ul.list-group` of the Blog theme (`OrchardCore.Themes/TheBlogTheme/Views/Search-Results.liquid`).

**Step `visitor-3`**

- Actions: `ClearHighlightsAsync`; `box = scene.Page.Locator("form input[name='Terms']").First` (the form of the page, which comes
  before the footer in the document; not the one of the footer); `ClickAsync(box)`; then
  `scene.Page.Keyboard.PressAsync("Control+A")` and `scene.Page.Keyboard.PressAsync("Backspace")`, so the typed word `harvest`
  disappears on camera; `HighlightAsync(box)`; pause 1500 ms. Keep the highlight until the step ends.
- On screen at the end: the box of the search page empty and highlighted, showing the placeholder
  `Search articles and posts`, which the step `settings-2` typed into the Site Search settings.
- Labels: the page's own box receives the placeholder of the settings (`OrchardCore.Search/Controllers/SearchController.cs`,
  `model.SearchForm.Placeholder = placeholder`) and the Blog theme draws it (`OrchardCore.Themes/TheBlogTheme/Views/Search-Form.liquid`,
  `placeholder="{{ placeholder }}"`).

**Step `visitor-4`**

- Actions: `ClearHighlightsAsync`; the same `box`; `TypeAsync(box, "zebra", delay: 90)`;
  `ClickAndWaitForNavigationAsync(box.Locator("xpath=ancestor::form").GetByRole(AriaRole.Button, new() { Name = "Search" }))`
  (the button of the page's own form, `Search-Form.liquid`). Scroll to the top if the page opens scrolled.
- On screen at the end: the search page for `zebra`, with the yellow alert "There are no such results." under the search
  box.
- Labels: `There are no such results.`: `OrchardCore.Search/Views/Search-Results.cshtml:43` and
  `OrchardCore.Themes/TheBlogTheme/Views/Search-Results.liquid:23`.

## Slides and cards

- `00-intro` (card): the opening card, title "Site Search in Orchard Core", subtitle "From enabled features to a working
  search page".
- `how-1`: four cards under "Four pieces behind site search"; the first, "An index", is bright.
- `how-2`: the same slide; the second card, "A search provider", is bright.
- `how-3`: the same slide; the third and fourth cards, "The Search feature" and "A permission", are bright.
- `maintain-1`: three cards under "Three actions on an index": Synchronize, Reset and Rebuild, all bright. Their names are
  the actions of an index in the list (`OrchardCore.Indexing/Views/IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:13,23,32`).
- `99-outro` (card): the closing card, subtitle `docs.orchardcore.net`.

## Checks for private data

Before the clips are assembled, look at every contact sheet and, after assembling, OCR the video (one frame per second and at
every scene change) and search the text for anything real:

- The only e-mail address is `admin@example.com`, and only if a page shows it; the user name is `admin`.
- No password is typed or shown; the password manager of the browser is off; the sign-in form is never recorded.
- The posts are the invented posts of the seed and the stock post of the Blog recipe; no real person, company or address.
- The address bar is not part of the recording; the URL `localhost:5010` does not appear.
- The tenant name is "Orchard Core"; no machine path or user name appears in a notification or an error.

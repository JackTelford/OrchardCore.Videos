# Recording plan: taxonomies-contents-list-filters

The video: "Taxonomies: Filter the Content Items List by Term" (audience: admin, 3:26 of narration). It shows how the
feature Taxonomies Contents List Filters is enabled, how the taxonomies to filter by are chosen, and how the Content
Items list is filtered by a term.

In this plan a selector stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. A
`file:line` reference is a path in the Orchard Core source under `src/OrchardCore.Modules/`, unless it starts with
`src/` (the labels were read at commit `92db4a83`). Where a selector is a guess about markup that a script builds
(the dropdowns are drawn by bootstrap-select), it is listed in "Selectors to confirm" and must be checked once with
`Recorder probe` before the first take.

## The clips and the order to record them in

| Order | Clip | Chapter | Starts on | Changes the site |
|---|---|---|---|---|
| 1 | `02-enable` | 02 Enable the feature | `/Admin` (the dashboard, admin menu visible) | Yes: enables the feature |
| 2 | `03-settings` | 03 Choose the taxonomies | `/Admin` | Yes: saves the Taxonomy Filters settings |
| 3 | `04-filter` | 04 Filter the list by term | `/Admin` | No |
| 4 | `05-types` | 05 Filters follow the content type | `/Admin/Contents/ContentItems`, no filter applied | No |

The slide clip `01-why` (chapter 01) needs no recording: `slides.py` draws it. Chapters 00 and 99 are cards.

Dependencies: `03-settings` needs `02-enable` (the menu item Settings, Taxonomy Filters exists only while the feature is
enabled). `04-filter` and `05-types` need `03-settings` (the buttons exist only for the taxonomies that were saved).
Clips 04 and 05 only read the site: filtering the list changes nothing that is stored, so they can be taken again in
any order and any number of times.

Snapshots (copy the site's data folder and `state.json` aside each time):

- **S1**: after the seed data below, before clip 02. The feature is disabled and the Taxonomy Filters settings are empty.
- **S2**: after clip 02 is accepted. The feature is enabled; the settings are still empty.
- **S3**: after clip 03 is accepted. The feature is enabled, and Categories and Tags are saved as filters.

To take clip 02 again, stop the site, restore S1, start the site. To take clip 03 again, restore S2. For clips 04 and 05
restore S3 only if the site was changed by hand in between. Start every take with the site restarted from the
snapshot, so no stale shell or cache is in play.

## The site it needs

- A checkout of `OrchardCMS/OrchardCore`; run `src/OrchardCore.Cms.Web` on `http://localhost:5010` with
  `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- Auto Setup, so no setup form is recorded (this is not a Setup screen video, so the sign-in in `state.json` is used):
  tenant `Default`, recipe `Blog`, site name `Orchard Core`, time zone `America/Los_Angeles`, database provider
  `Sqlite`, user `admin`, email `admin@example.com`, password `<password>` (the real value is chosen outside git and
  passed in `VIDEO_PASSWORD`; never write it anywhere in the repository). The environment variables are
  `OrchardCore__AutoSetup__Tenants__0__ShellName`, `...__SiteName`, `...__SiteTimeZone`, `...__AdminUsername`,
  `...__AdminEmail`, `...__AdminPassword`, `...__DatabaseProvider`, `...__RecipeName` (see the Auto Setup page of the
  documentation and the `orchardcore-tester` skill).
- `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address: the site calls no external service, and this video needs none.
- Sign the recorder in once, outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and
  `VIDEO_PASSWORD=<password>`, which writes `state.json` (git-ignored).
- Features: enable nothing by hand. The Blog recipe already enables Taxonomies, Contents, Content Types, Features and
  Settings (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:38-72`). The feature
  `OrchardCore.Taxonomies.ContentsAdminList` must be **disabled** when clip 02 starts: the recipe does not enable it.
- Settings: Settings, Taxonomy Filters does not exist before clip 02 and must be empty (nothing checked) before clip 03.
- External services and local stand-ins: none.

## Seed data (made outside the capture, before S1)

The Blog recipe creates the taxonomies **Categories** (one term, Travel) and **Tags** (Earth, Exploration, Space), the
content type **Blog Post** with a field **Category** (a taxonomy field on Categories, one term, leaf terms only) and a
field **Tags** (a taxonomy field on Tags), and one sample post. The sample post, "Man must explore, and this is
exploration at its greatest", has the category Travel and the tags Earth, Exploration and Space. Keep it.
Reference: `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:625-672` (the fields) and `:850-1060` (the
taxonomies and the post).

### A. Make Blog Post listable and creatable

The recipe's Blog Post type is neither (`blog.recipe.json:188-196`), so its items would not appear in the Content
Items list, and the Content Type list would not offer Blog Post.

1. Admin menu: Design, Content Definition, Content Types (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`).
2. In the row of **Blog Post**, select **Edit** (`a[role='btn-edit-BlogPost']`,
   `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`).
3. Check **Creatable** and **Listable** (`OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:17,30`).
   Only Listable matters for the video (it puts Blog Post in the Content Type dropdown and its items in the list);
   Creatable is checked as well so that the type behaves like the other content types of the site.
4. Select **Save** (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:215`).

### B. Add terms to Categories and publish it

The Taxonomy type is draftable, and adding a term works on a draft (`OrchardCore.Taxonomies/Controllers/AdminController.cs:110`).
The filter lists the terms of the **published** taxonomy, and the settings page lists only published taxonomies
(`OrchardCore.Taxonomies/Settings/TaxonomyContentsAdminListSettingsDisplayDriver.cs:43`), so the taxonomy must be
published at the end.

Every term of Categories has the content type Category, and that type has a **required** field **Icon**
(`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:793-810`, `"Required": true` at `:806`) whose editor is the
icon picker (`OrchardCore.ContentFields/Views/TextField-IconPicker.Edit.cshtml:24-37`). The value is stored only when
an icon is picked in the dropdown (`OrchardCore.ContentFields/Assets/ts/text-field-icon-picker.ts:36-38`), so a term
whose icon was not picked is refused with a validation message. Do the following for every new term, called "the icon
step" below: in the **Icon** field, open the dropdown with the caret button (`.icon-picker-toggle`, next to the
heart icon) and click the first icon of the list (`.iconpicker-item`). The icon is not shown in the video.

1. Content, Content Items. In the row of the **Categories** item (type Taxonomy), select **Edit**.
2. Select **Add Category** (`OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml:48`). Enter the Title `Food`, do the
   icon step, then select the green **Publish** button (`OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`).
   The page returns to the Categories editor.
3. Select **Add Category** again: Title `Technology`, the icon step, **Publish**.
4. In the row of the term **Technology**, select **Add** (`OrchardCore.Taxonomies/Views/Content.TermAdmin.cshtml:36`):
   Title `Gadgets`, the icon step, **Publish**.
5. In the row of **Technology**, select **Add** again: Title `Software`, the icon step, **Publish**.
6. Back on the Categories taxonomy, select **Publish** so the published version holds all the terms.

The terms read, in this order and hierarchy: Travel, Food, Technology (with the children Gadgets and Software).
Leave Travel as it is: it must stay a leaf term, because the Category field accepts leaf terms only.

### C. Create five blog posts

Create the posts from the Blog item, so that each post has the Blog as its container (the path pattern of the Blog
Post type uses the container, `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:216`; the Blog contains Blog
Post items, `:309-314`):

1. Content, Content Items. In the row of the **Blog** item, select **Edit**
   (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`). In the bar at the top of the Blog editor
   select **Create Blog Post** (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:51-52`, shown in the editor of
   an existing item by `OrchardCore.Lists/Drivers/ListPartDisplayDriver.cs:74-85`).
2. Fill the post as the table says, then select **Publish**. The page returns to the Blog editor
   (the button carries the `returnUrl` of the Blog editor); select **Create Blog Post** for the next post.

In the **Title** field type the title. In the **Category** field pick the radio button of the term (the radio button
of Technology is disabled: leaf terms only). In the **Tags** field click the field, type the tag, and pick the
existing tag from the list; never create a new tag (the editor shows "Press enter to create a tag" when nothing
matches, `OrchardCore.Taxonomies/Services/TaxonomiesJSLocalizer.cs:20`). Leave the subtitle, the body and the banner
image empty.

| Title | Category | Tags |
|---|---|---|
| Packing for a month away | Travel | Earth |
| Cooking at a high camp | Food | Exploration |
| A year of orbit photos | Gadgets | Space |
| Mapping the ocean floor | Software | Earth, Exploration |
| Planning software for expeditions | Software | Exploration |

With the sample post, the site now has six blog posts.

### D. Check the seed, then take S1

On Content, Content Items (before the feature is enabled, no taxonomy button exists yet):

- the list holds eleven items in all: the six posts (type Blog Post), the About article (type Article), the Blog item,
  the two Taxonomy items (Categories, Tags), and the Main Menu item (type Menu, listable by its own recipe,
  `OrchardCore.Menu/Migrations/menu.recipe.json:11`). If the total differs, note the real total here; the narration
  does not state a count;
- in Design, Content Definition, Content Types, Blog Post shows Creatable and Listable checked;
- the row of the Categories taxonomy shows the status **Published** and no **Draft** status: a draft left over from the
  steps of B would hide the new terms from the filter, which reads the published version
  (`OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:9-20`). If **Draft** is shown, edit the taxonomy and
  select **Publish**;
- the editor of Categories (select **Edit**, then leave the page without saving) lists the terms Travel, Food and
  Technology, with Gadgets and Software below Technology.

Stop the site, copy the data folder and `state.json` aside as **S1**, start the site again.

## Selectors to confirm with `Recorder probe`

Run `Recorder probe /Admin/Features "() => document.title"` style checks once, and write the confirmed selectors down
here before the first take. The table gives the starting point.

| What | Selector to start from | Source |
|---|---|---|
| Admin menu entries | the links with the visible text `Tools`, `Features`, `Settings`, `Taxonomy Filters`, `Content`, `Content Items` | `OrchardCore.Features/AdminMenu.cs:26-27`, `OrchardCore.Taxonomies/AdminMenu.cs:26-27`, `OrchardCore.Contents/AdminMenu.cs:65-68` |
| Features search box | `#search-box` | `OrchardCore.Features/Views/Admin/Features.cshtml:27` |
| Feature rows | `[data-filter-value]`; the row of the feature has `OrchardCore.Taxonomies.ContentsAdminList` in `data-filter-value` | `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20` |
| Enable button of the feature | `a#btn-enable-OrchardCore_Taxonomies_ContentsAdminList` (or `[data-filter-value*='OrchardCore.Taxonomies.ContentsAdminList'] a.btn-primary`) | `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` |
| Taxonomy checkboxes | the checkbox whose label is `Categories`, and the one labeled `Tags` (a label is attached to each) | `OrchardCore.Taxonomies/Views/TaxonomyContentsAdminListSettings.Edit.cshtml:13-14` |
| Save button of settings | `button.save` with the text `Save` | `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` |
| Taxonomy filter button | `div.bootstrap-select:has(select.taxonomy-selectpicker) > button.dropdown-toggle[title='Categories']` (and `[title='Tags']`) | `OrchardCore.Taxonomies/Views/ContentsAdminListTaxonomyFilter.cshtml:4` |
| Content Type button | `div.bootstrap-select:has(select.contenttypes-selectpicker) > button.dropdown-toggle` (title `Content Type`) | `OrchardCore.Contents/Views/ContentsAdminListFilters.cshtml:8` |
| An option of an open dropdown | `div.dropdown-menu.show a.dropdown-item` filtered by its text | bootstrap-select markup |
| Icon picker of a term (seed B only) | the caret button `.icon-picker-toggle` in `.icon-picker-field`, then the first `.iconpicker-item` | `OrchardCore.ContentFields/Views/TextField-IconPicker.Edit.cshtml:26-37`, `OrchardCore.ContentFields/Assets/ts/text-field-icon-picker.ts:36-38` |

Choosing an option of any of these dropdowns submits the list's form by itself: the script clicks the hidden
`submit.Filter` button on `changed.bs.select` (`OrchardCore.Contents/Assets/ts/contents-admin-list.ts` loads the
component, built into `OrchardCore.Contents/wwwroot/Scripts/contents-admin-list/contents-admin-list.js`). The page then
reloads, so every option click is a navigation: use `ClickAndWaitForNavigationAsync`.

Enabling a feature and saving settings can restart the tenant, and the next page may take several seconds. Keep that
wait off camera: `scene.OffCameraAsync(...)` with a wait for a selector that only the loaded page has (for example
the success message), never a fixed delay.

## Clip `02-enable`

Start on `/Admin` (opened before `StartCaptureAsync`, left admin menu visible). Signed in as `admin`. The feature
is disabled.

- `enable-1`: click **Tools** in the admin menu, then **Features** (`ClickAndWaitForNavigationAsync`). Wait until
  `#search-box` is visible. When the narration ends the Features page is on screen: the breadcrumb reads Features, the
  search box is empty, and the list of features is shown grouped by category. Labels: `OrchardCore.Features/AdminMenu.cs:26-27`,
  `OrchardCore.Features/Views/Admin/Features.cshtml:15`.
- `enable-2`: type `taxonomies` in `#search-box` (`TypeAsync`). Wait until exactly two rows are shown (rows without the
  `d-none` class). When the narration ends two rows are visible in the Content Management group: **Taxonomies** with
  the badge **Enabled**, and **Taxonomies Contents List Filters** with the badge **Disabled**, the description
  **Provides taxonomy filters in the contents list.** and a blue **Enable** button. Optionally `HighlightAsync` the
  second row. Labels: `OrchardCore.Taxonomies/Manifest.cs:12,20-21`,
  `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9,15`,
  `OrchardCore.Features/Views/FeatureEntry.Description.SummaryAdmin.cshtml:8`. The filter matches the name, id and
  description (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`).
- `enable-3`: clear any highlight, then click **Enable** in the row of Taxonomies Contents List Filters
  (`ClickAndWaitForNavigationAsync`). Wait (off camera if it takes long) until the success message is shown. When the
  narration ends the Features page has reloaded with the message **The feature Taxonomies Contents List Filters was
  enabled.** at the top. Source: `OrchardCore.Features/Controllers/AdminController.cs:317`.

Take S2 after this clip is accepted.

## Clip `03-settings`

Start on `/Admin`, left admin menu visible. The feature is enabled.

- `settings-1`: click **Settings** in the admin menu, then **Taxonomy Filters** (`ClickAndWaitForNavigationAsync`).
  When the narration ends the settings page is on screen with the breadcrumb **Settings**, the label **Taxonomies**,
  its hint, and two unchecked checkboxes labeled **Categories** and **Tags**. Labels:
  `OrchardCore.Taxonomies/AdminMenu.cs:26-27`, `OrchardCore.Settings/Views/Admin/Index.cshtml:4`,
  `OrchardCore.Taxonomies/Views/TaxonomyContentsAdminListSettings.Edit.cshtml:4`. The hint in the source reads "The taxonomies to filters
  in the contents list." (a typo in the source); the narration does not read it. The narration names two
  permissions. "Manage taxonomies" (`OrchardCore.Taxonomies/Permissions.cs:8`) is required by the menu item
  (`OrchardCore.Taxonomies/AdminMenu.cs:30`) and by the editor of the page
  (`OrchardCore.Taxonomies/Settings/TaxonomyContentsAdminListSettingsDisplayDriver.cs:38`). "Manage settings" is
  required by the page itself: the controller authorizes the permission of a settings group
  (`OrchardCore.Settings/Controllers/AdminController.cs:53,85`), the Taxonomies module registers no permission for its
  group (`OrchardCore.Taxonomies/Startup.cs:79-89`), so the group falls back to the "Manage settings" permission
  (`src/OrchardCore/OrchardCore.Settings.Core/SettingsPermissions.cs:8,21`). The Administrator role holds both by
  default (`OrchardCore.Taxonomies/Permissions.cs:18-25`, `OrchardCore.Settings/Permissions.cs:17-24`), so the permissions
  are not shown on screen.
- `settings-2`: click the checkbox labeled **Categories**, then the one labeled **Tags**. When the narration ends both
  checkboxes are checked. Only the published taxonomies appear
  (`OrchardCore.Taxonomies/Settings/TaxonomyContentsAdminListSettingsDisplayDriver.cs:43`): with the seed that is
  Categories and Tags, and no other checkbox.
- `settings-3`: click **Save** (`ClickAndWaitForNavigationAsync`; the page posts and redirects to itself). Wait (off
  camera if needed) until the message is shown. When the narration ends the same settings page is shown, both
  checkboxes still checked, with the message **Site settings updated successfully.** at the top. Sources:
  `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`, `OrchardCore.Settings/Controllers/AdminController.cs:119`.

Take S3 after this clip is accepted.

## Clip `04-filter`

Start on `/Admin`, left admin menu visible. Categories and Tags are saved as filters. Every option click below reloads
the page; wait for the list to show again (a visible row) before the step ends.

- `filter-1`: click **Content** in the admin menu, then **Content Items** (`ClickAndWaitForNavigationAsync`). When the
  narration ends the page titled **Manage Content** is on screen with the unfiltered list. In the bar above the rows
  the buttons **Content Type**, **Show** and **Sort** are followed by two more buttons titled **Categories** and
  **Tags** (their order is the order in which the two taxonomies are loaded, which the source does not fix: it may
  read Tags, Categories; note the real order and use it for the highlights, the narration names both without an
  order). `HighlightAsync` the Categories and Tags buttons (clear the highlights at the start of the next step).
  Labels: `OrchardCore.Contents/Views/Admin/List.cshtml:17`, `OrchardCore.Contents/Views/ContentsAdminListFilters.cshtml:8,10,11`,
  `OrchardCore.Taxonomies/Views/ContentsAdminListTaxonomyFilter.cshtml:4`. The taxonomy buttons come after Sort
  because their position is `Actions:40.5` and `Actions:40.10`
  (`OrchardCore.Taxonomies/Drivers/TaxonomyContentsAdminListDisplayDriver.cs:115,119`), against `Actions:10.1` for the
  others (`OrchardCore.Contents/Drivers/ContentOptionsDisplayDriver.cs:37`).
- `filter-2`: click the **Categories** button. When the narration ends the dropdown is open and shows, from the top:
  the header **Filter by Categories**, a search box, and the options **Clear filter**, **Show all**, **Travel**,
  **Food**, **Technology**, then **Gadgets** and **Software** indented below Technology. Do not choose an option yet.
  Sources: `OrchardCore.Taxonomies/Drivers/TaxonomyContentsAdminListDisplayDriver.cs:86,91,99-103` (the labels and the
  two non-breaking spaces per level), `OrchardCore.Taxonomies/Views/ContentsAdminListTaxonomyFilter.cshtml:4`
  (`data-header`, `data-live-search`). The terms come from the published Categories taxonomy (seed step B.6).
- `filter-3`: click the option **Software** (`ClickAndWaitForNavigationAsync`). When the narration ends the list shows
  exactly two rows, both of type Blog Post: **Mapping the ocean floor** and **Planning software for expeditions**.
  Source of the behavior: `OrchardCore.Taxonomies/Services/TaxonomyContentsAdminListFilter.cs:43-48` (a term matches
  the items whose index row holds that term id) and `OrchardCore.Taxonomies/Indexing/TaxonomyIndexProvider.cs:83-95`
  (one index row per term selected on the field). The narration says nothing about parent terms; Technology is not chosen on camera.
- `filter-4`: click the **Categories** button, then the option **Show all** (`ClickAndWaitForNavigationAsync`). When
  the narration ends the list shows exactly the six blog posts: Man must explore, and this is exploration at its
  greatest; Packing for a month away; Cooking at a high camp; A year of orbit photos; Mapping the ocean floor;
  Planning software for expeditions. The About article, the Blog item, the Taxonomy items and the Main Menu item are
  gone. Source: `OrchardCore.Taxonomies/Services/TaxonomyContentsAdminListFilter.cs:36-42`.
- `filter-5`: click the **Categories** button, then the option **Clear filter** (`ClickAndWaitForNavigationAsync`). When
  the narration ends the unfiltered list is back: its first page (the page size of the site is 10,
  `OrchardCore.Settings/Services/SiteService.cs:59`), with the pager below it, and the item count shows the total of the
  seed check D. The Taxonomy items, the Blog item and the About article are among the items again, on the first page
  or the second according to the sort order. Source: `OrchardCore.Taxonomies/Drivers/TaxonomyContentsAdminListDisplayDriver.cs:86`.
  The page reloads only when the selection of the dropdown changes (`changed.bs.select`). This step relies on the
  dropdown showing **Show all** as selected after the reload of `filter-4`; if the dropdown shows **Clear filter** as
  selected after a reload, choosing it changes nothing and the list stays filtered: stop and tell the maintainer, the
  narration of this step then needs another route to the unfiltered list (OPEN-QUESTIONS.md, point 10).
- `filter-6`: click the **Tags** button, then the option **Exploration** (`ClickAndWaitForNavigationAsync`). When the
  narration ends the list shows exactly four rows: Man must explore, and this is exploration at its greatest;
  Cooking at a high camp; Mapping the ocean floor; Planning software for expeditions.

This clip changes nothing that is stored.

## Clip `05-types`

Start on `/Admin/Contents/ContentItems` with no filter applied (opened before `StartCaptureAsync`, so the Categories
and Tags buttons are visible next to Sort). Start the clip from a freshly loaded page, so that no term filter from a
previous clip is still in the address: a term filter that is still active would apply to the other content type too
(`TaxonomyContentsAdminListFilter.cs` does not look at the content type).

- `types-1`: click the **Content Type** button, then the option **Blog Post** (`ClickAndWaitForNavigationAsync`).
  When the narration ends the list shows the six blog posts, and the Categories and Tags buttons are still in the
  bar after Sort. Sources: `OrchardCore.Contents/Controllers/AdminController.cs:896` (the first option is
  **All content types**), `OrchardCore.Taxonomies/Drivers/TaxonomyContentsAdminListDisplayDriver.cs:51-63` (a
  taxonomy is offered when the selected type has a taxonomy field that uses it, and every chosen taxonomy is offered
  while no type is selected; Blog Post has the fields Category and Tags, `blog.recipe.json:643-672`). The type that
  comes from the filter text of the address is mapped before the Taxonomies driver runs, because the feature depends
  on Taxonomies, which depends on Content Types, which depends on Contents
  (`OrchardCore.Taxonomies/Manifest.cs:22`, `OrchardCore.ContentTypes/Manifest.cs:9`,
  `OrchardCore.Contents/Drivers/ContentOptionsDisplayDriver.cs:31`).
- `types-2`: click the **Content Type** button, then the option **Article** (`ClickAndWaitForNavigationAsync`). When
  the narration ends the list shows the single item **About** (type Article), and the bar holds only Content Type,
  Show and Sort: no Categories button and no Tags button
  (`TaxonomyContentsAdminListDisplayDriver.cs:59-62` returns nothing when no field of the type uses a chosen taxonomy).
  The Article type has no taxonomy field (`blog.recipe.json:131-187` is the type, `:677` its part: no taxonomy field is defined outside `:643-672`).

This clip changes nothing that is stored.

## The slides (chapter 01, drawn by `slides.py`, nothing to record)

- `why-1`: three cards, "Taxonomies categorize content", "The list grows", "One term, scattered items", all bright.
- `why-2`: heading "Taxonomies Contents List Filters", three check cards: "One button per taxonomy", "Filter by term",
  "Only matching items".
- `why-3`: heading "Three steps", three numbered cards: "Enable the feature" (Tools, Features), "Choose the taxonomies"
  (Settings, Taxonomy Filters), "Filter the list" (Content, Content Items).

## Checks for private data

- The site is fresh and every record is fake: the user `admin` with `admin@example.com`, the titles of the table above,
  the terms of the Blog recipe. No mask script is needed; do not create one.
- The only e-mail address that can appear is `admin@example.com`, in no frame of this video (the user menu is not
  opened). OCR the video at one frame per second and at every scene change, then at 5 frames per second, and look for
  any other name, address or token.
- The password never appears: the sign-in is in `state.json`, which is git-ignored, and no clip types it.
- Do not commit the site's data folder, `state.json`, the audio, the clips or the MP4.

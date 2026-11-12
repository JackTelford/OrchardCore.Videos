# Recording plan: taxonomies-overview

How to record this video without deciding anything. Every label comes from the Orchard Core source at commit
`92db4a83` (`src/OrchardCore.Modules/OrchardCore.Taxonomies`, `OrchardCore.Contents`, `OrchardCore.Features`,
`OrchardCore.Lists`, `OrchardCore.Autoroute` and `src/OrchardCore.Themes/TheBlogTheme`). In this file a selector
stands for `scene.Page.Locator(...)`. A selector marked "confirm" is derived from the view, not from a running page:
check it once with `Recorder probe` before recording.

The video has eight chapters: the opening card (00), the closing card (99), two slide chapters (01 and 06) and four
recordings (02 to 05) of the admin and of the front end. No recording changes the site.

## The site it needs

- Run `src/OrchardCore.Cms.Web` of an Orchard Core checkout that contains the Taxonomies module, on
  `http://localhost:5010` (`VIDEO_BASE_URL`), with an empty `ORCHARD_APP_DATA` folder outside both repositories.
- Provision it with Auto Setup (the `OrchardCore.AutoSetup` setup feature; see `references/autosetup.md` of the
  `orchardcore-tester` skill), using these environment variables, so no Setup screen is recorded:

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

  `<password>` is chosen outside git and passed again as `VIDEO_PASSWORD` to the recorder (`VIDEO_USER_NAME=admin`).
  Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- Recipe: **Blog**. Its feature step enables `OrchardCore.Taxonomies`, `OrchardCore.Lists`, `OrchardCore.Autoroute`
  and the themes TheAdmin (admin) and TheBlogTheme (site) (`blog.recipe.json:31-81`). Enable nothing else. Leave
  **Taxonomies Contents List Filters** disabled: the video shows it as disabled.
- No setting to save, no external service, no stand-in.
- Run `Recorder setup` once (it saves `state.json`, which stays out of git).

### What the Blog recipe already creates (do not create it again)

| Item | Value | Source |
|---|---|---|
| Taxonomy `Tags` | term content type **Tag**, terms Earth, Exploration, Space, "Route contained items" checked | `blog.recipe.json:855-920` |
| Taxonomy `Categories` | term content type **Category**, term Travel, "Route contained items" checked | `blog.recipe.json:921-963` |
| Blog Post field **Tags** | editor and display mode Tags | `blog.recipe.json:643-658` |
| Blog Post field **Category** | Unique and Leaves only | `blog.recipe.json:659-673` |
| Category term field **Icon** | **Required**, Icon picker editor: every term of the Category type needs an icon (Travel has one) | `blog.recipe.json:796-809` |
| Blog Post | "Man must explore, and this is exploration at its greatest": Category Travel, tags Earth, Exploration, Space | `blog.recipe.json:999-1058` |

## Seed data

Create this outside the capture, once, then take the snapshot (see "Order and state"). All names are invented.

**S1. The terms Food, Recipes and Restaurants.**

1. Open `/Admin/Contents/ContentItems/Taxonomy`. On the row **Categories**, click **Edit**.
2. Click **Add Category** (`TaxonomyPart.Edit.cshtml:48`). The page is titled **New Category**
   (`Views/Admin/Create.cshtml:19`). In **Title** type `Food`. Then pick an icon: the field **Icon** is required
   (`blog.recipe.json:796-809`, `TextFieldDisplayDriver.cs:51`) and keeps an empty value until an icon is clicked
   (`text-field-icon-picker.ts:36`), so **Publish** would answer "A value is required for Icon." Click the drop-down
   button with the caret beside the heart button (`.icon-picker-toggle`, `TextField-IconPicker.Edit.cshtml:32`), type
   `globe` in the search box of the list (placeholder "Type to filter",
   `OrchardCore.Resources/wwwroot/Scripts/fontawesome-iconpicker.js`), and click the first icon shown. Leave the
   other fields as they are. Click **Publish**. The Categories editor opens again with Food in the list.
3. On the row **Food**, click **Add**. In **Title** type `Recipes`. Pick an icon in **Icon** as in step 2. Click
   **Publish**.
4. On the row **Food**, click **Add** again. In **Title** type `Restaurants`. Pick an icon in **Icon** as in step 2.
   Click **Publish**.
5. Back in the Categories editor, click **Publish** at the bottom of the page. The terms are saved inside the
   taxonomy as a draft until the taxonomy is published (`AdminController.cs:110` loads it with
   `VersionOptions.DraftRequired`, and the Taxonomy type is draftable, `Migrations.cs:24`); the front end reads
   the published version.
6. Check: the editor lists Travel, Food, and under Food (indented) Recipes and Restaurants.

**S2. A second post about travel.**

1. Open `/Admin/Contents/ContentItems/Blog`. On the row **Blog**, click **List BlogPost Items**: the button on this
   row is labeled with the technical name of the contained type, not its display name
   (`ListPartSummaryAdmin.cshtml:10`, `ListPartDisplayDriver.cs:98`). On the page that opens, click **Create Blog
   Post** (`ListPartNavigationAdmin.cshtml:52`, labeled with the display name): the post must be created from this
   page, so that it belongs to the Blog.
2. Fill the editor:
   - **Title**: `A weekend in Lisbon`
   - **Subtitle**: `Trams, tiles and custard tarts`
   - the Markdown body: `Trams, tiles and a very good custard tart.`
   - **Category**: select **Travel**
   - **Tags**: click the field, type `Ear`, click **Earth** in the list
3. Click **Publish**.
4. Check: open `/`. The Blog lists "A weekend in Lisbon" first, then "Man must explore, and this is exploration at
   its greatest". "A weekend in Lisbon" shows the badges Travel and Earth.

**S3. Final check.** Open `/Admin/Features`, search `taxonomies`, and check that **Taxonomies** is Enabled and
**Taxonomies Contents List Filters** is Disabled. Close the browser, stop the site, and copy `App_Data` aside.

## Order and state

- Narrate first (`narrate.py`), then record: the recorder holds each step for the length of its narration.
- Every recording reads the site and changes nothing, so they do not depend on each other and can be recorded in any
  order, from the same snapshot: `02-feature`, `03-taxonomy`, `04-categorize`, `05-site`.
- No clip changes the site, so no snapshot has to be restored between two takes. One exception: in `04-categorize`
  the cursor types a name into the Tags editor. If Enter is pressed by mistake there, a tag is created at once
  (`TagController.cs`, the term is added to the Tags taxonomy and published). Stop the site and restore the snapshot
  before the next take.
- The recordings start signed in (`state.json`). Every clip opens its first page with `scene.OpenAsync` before
  `StartCaptureAsync`, and navigates only with `ClickAndWaitForNavigationAsync`.
- The slide chapters (`01-why`, `06-more`) need no site.

## Clips

### 02-feature (chapter 02, short `features`)

Starts on `/Admin` (the admin dashboard), with the admin menu as it is after sign-in, and `/Admin/Features` not yet
opened. Needs: the seed above.

- `feature-1`: `ClickAsync` on **Tools** in the admin menu to expand it, then `ClickAndWaitForNavigationAsync` on
  **Features** under it (`OrchardCore.Features/AdminMenu.cs:26-27`; the menu selectors are "confirm": use
  `Recorder probe` once on `/Admin`). The page is titled **Features**. Then `TypeAsync(#search-box, "taxonomies")`
  (`Features.cshtml:27`, the box has the placeholder **Search**). The search matches the name, the id and the
  description of each feature (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), so it keeps exactly
  the two features of the Taxonomies module (no other manifest mentions taxonomies). Wait until the line that reads
  "Showing N of M features" shows N smaller than M (`Features.cshtml:57-59`). The narration is 22 words, about 9
  seconds: wait about 2.5 seconds (the first sentence), then do the two clicks without a pause (about 3 seconds),
  and type at 40 ms per character as soon as the page is shown, so the typing falls where the narration says "type
  taxonomies in the Search box". When the narration ends, the box holds `taxonomies`,
  and the list shows the heading **Content Management** (`Features.cshtml:88`, the category heading) with the two
  features under it.
- `feature-2`: `HighlightAsync` on the row that contains the description "The taxonomies module provides a way to
  categorize content items." (`Manifest.cs:13`). The row is the element that carries the attribute `data-filter-value`
  (`FeatureEntryDisplayDriver.cs:20`; the search hides and shows exactly these elements, `features-index.ts`), so the
  selector is `[data-filter-value*="The taxonomies module provides"]`; the row holds the name **Taxonomies**, its
  description, the dependency badge and the status. When the narration ends, the ring is around that row, whose
  status reads **Enabled** (`FeatureEntry.DefaultMeta.SummaryAdmin.cshtml`), and the **Content Management** heading
  is visible above it. Click nothing on the row.
- `feature-3`: `ClearHighlightsAsync`, then `HighlightAsync` on the row that contains "Provides taxonomy filters in
  the contents list." (`Manifest.cs:21`; selector `[data-filter-value*="Provides taxonomy filters"]`). When the
  narration ends, the ring is around the row **Taxonomies Contents List Filters**, whose status reads **Disabled**,
  with a dependency badge for Taxonomies. Do not click **Enable**.

### 03-taxonomy (chapter 03, short `taxonomy`)

Starts on `/Admin/Contents/ContentItems/Taxonomy` (the Content Items list filtered by the type Taxonomy,
`OrchardCore.Contents/Controllers/AdminController.cs:67` and `:91-94`; the **Content Type** filter is labeled in
`ContentsAdminListFilters.cshtml:8`). Needs: the seed above.

- `taxonomy-1`: hold the page for about 10 seconds (the first sentence, and the start of the second), then
  `HighlightAsync` on the row **Categories** for about 1.5 seconds and on the row **Tags** until the narration ends.
  A row is the `li.list-group-item-action` that holds the name (`Page.Locator("li.list-group-item-action").Filter(new()
  { HasText = "Categories" })`; `OrchardCore.Admin/Views/AdminList.cshtml:60`; the header row of the list has no
  `list-group-item-action` class; confirm with `Recorder probe`). When the narration ends, the page is titled
  **Taxonomy** under the breadcrumb **Manage Content** (`OrchardCore.Contents/Views/Admin/List.cshtml:16-27`), and the
  list shows the rows **Categories** and **Tags**, each with the type badge `Taxonomy`
  (`ContentsType_SummaryAdmin.cshtml`) and no other row; the ring is around **Tags**.
- `taxonomy-2`: `ClickAndWaitForNavigationAsync` on the **Edit** button of the row **Categories**
  (`a.btn.edit` inside the row, `ContentsButtonEdit_SummaryAdmin.cshtml:13`). The page is titled **Edit Taxonomy**
  (breadcrumb `Edit {0}` with the type name Taxonomy, `OrchardCore.Contents/Views/Admin/Edit.cshtml:39`; the display name defaults to
  "Taxonomy", `ContentDefinitionManagerExtensions.cs:12`). Then `HighlightAsync` on the **Term Content Type**
  drop-down (`TaxonomyPart.Edit.cshtml:26-29`; `select[name$="TermContentType"]`, confirm). When the narration ends,
  the page shows **Title** `Categories`, **Alias**, **Permalink**, and **Term Content Type** with `Category`
  selected, ringed.
- `taxonomy-3`: `ClearHighlightsAsync`. If the term list is not fully visible, `ScrollAsync(0, 300)`, then
  `HighlightAsync` on the list `ol#menu` (`TaxonomyPart.Edit.cshtml:58`). When the narration ends, the ring is
  around four rows: **Travel**, **Food**, and, indented under Food, **Recipes** and **Restaurants**, each with the
  buttons **Edit**, **Add** and **Delete** (`Content.TermAdmin.cshtml:27,36,45`).
- `taxonomy-4`: `ClearHighlightsAsync`; `HighlightAsync` on the button **Add Category**
  (`a.btn.btn-primary.btn-sm` whose text is "Add Category", `TaxonomyPart.Edit.cshtml:48`) with the caption
  `Top level`; after about four seconds `ClearHighlightsAsync`, `MoveToAsync` to the **Add** button of the row
  **Food** (`li.menu-item` that holds the text `Food`, then `a` with the text "Add") and `HighlightAsync` it with the
  caption `Child term`; after about five more seconds `ClearHighlightsAsync` and `HighlightAsync` on the taxonomy's
  **Publish** button (`button.publish`, `Content.PublishButton.cshtml:7,19`, in the action bar at the bottom of the
  page). The three sentences of the narration last about 4, 5 and 6 seconds, which is where the three rings change.
  Click nothing. When the narration ends, the ring is around **Publish**.

### 04-categorize (chapter 04, short `categorize`)

Starts on `/Admin/Contents/ContentItems/BlogPost` (the list of Blog Posts). Needs: the seed above.

- `categorize-1`: `ClickAndWaitForNavigationAsync` on the **Edit** button of the row
  "Man must explore, and this is exploration at its greatest" (`a.btn.edit` inside that row), after about two
  seconds, where the narration says "Let's edit a Blog Post". The page is titled **Edit Blog Post**. Then
  `ScrollIntoViewIfNeededAsync()` on the **Category** field (`.field-wrapper-blog-post-category`; the **Tags** field is
  directly above it, and the two fields are shorter than the 864-pixel window, so both labels are in view), and
  `HighlightAsync` on each field
  (`.field-wrapper-blog-post-tags` and `.field-wrapper-blog-post-category`: the class is built with `HtmlClassify`,
  which turns the part name `BlogPost` into `blog-post`; `TaxonomyField.Edit.cshtml:15`,
  `TaxonomyField-Tags.Edit.cshtml:36`, `OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:77-110`). The fields come
  after the Markdown body because of the part positions (`blog.recipe.json:186-243`). When the narration ends, both
  fields are ringed.
- `categorize-2`: `ClearHighlightsAsync`; `HighlightAsync` on the **Category** field only. When the narration
  ends, the field shows four radio buttons: **Travel** selected, **Food** disabled (greyed), and, indented,
  **Recipes** and **Restaurants** enabled (`TaxonomyField.Edit.cshtml:66-90`; radio buttons because Unique is set,
  disabled because Leaves only is set and Food has children, `blog.recipe.json:669-670`). Click nothing.
- `categorize-3`: `ClearHighlightsAsync`; wait about 1.5 seconds, then `ClickAsync` on the box of the **Tags** editor
  (`.field-wrapper-blog-post-tags .multiselect__tags`), not on the input inside it: until the list is open that input
  has a width of 0 (`OrchardCore.Resources/wwwroot/Scripts/vue-multiselect.esm.js:1113-1124`), so a click on it is not
  possible. The list of tags opens and shows **Earth**, **Exploration** and **Space** (the editor does not hide the
  selected ones: `hide-selected` is not set in `taxonomy-field-tags.js`). Leave it open, without typing, until the
  narration has said "and you select the ones you want" (about 7 seconds into the step). Then `TypeAsync` the text
  `Moon` into `.field-wrapper-blog-post-tags .multiselect__input` (slow, 90 ms per character). Wait until the
  highlighted entry of the list carries the attribute for the hint:
  `.field-wrapper-blog-post-tags .multiselect__option--highlight[data-select="Press enter to create a tag"]`
  (`TaxonomiesJSLocalizer.cs:20`, `taxonomy-field-tags.js`, `tag-placeholder`). The words are drawn by the style
  sheet from that attribute (`OrchardCore.Resources/wwwroot/Styles/vue-multiselect.css:340-343`), so a text locator
  never finds them. When the narration ends, the entry "Moon" with the hint is visible. **Never press Enter** in this
  clip. If the entry does not show, the Open setting of the Tags editor is off or the user lacks the Manage taxonomies
  permission: stop and see OPEN-QUESTIONS.md.
- `categorize-4`: press Escape once, in the Tags input and nothing else: the key closes the list and empties the
  typed text (`vue-multiselect.esm.js:666-680` and `:1277`, `preserveSearch` is off), so the field holds the chips
  **Earth**, **Exploration** and **Space** again. Do not press Backspace: on an empty box it removes the last chip.
  `ClearHighlightsAsync`, then `HighlightAsync` on the editor's **Publish** button (`button.publish` in the action bar
  at the bottom of the page, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7,19`) at once, where the
  narration says "When the post is published". Do not click it. When the narration ends, the ring is around
  **Publish** and the fields above are unchanged. Leave the page without saving:
  if the browser asks to confirm leaving, accept outside the capture.

### 05-site (chapter 05, short `site`)

Starts on `/` (the Blog home page of TheBlogTheme). Needs: the seed above (S1 is not visible here; S2 is).

- `site-1`: `HighlightAsync` on the badges of the first post, `.post-preview` (first) then `.term-badge`
  (`TheBlogTheme/Views/Content-BlogPost.Summary.liquid:15-36`). When the narration ends, the first summary
  "A weekend in Lisbon" is on screen with its badges **Travel** and **Earth** ringed, and the second summary is
  visible below with **Travel**, **Earth**, **Exploration** and **Space**.
- `site-2`: `ClearHighlightsAsync`; `ClickAndWaitForNavigationAsync` on the link whose badge reads **Travel** in the
  first summary (`.post-preview` first, then `a` with the text "Travel"). The term page opens
  (`Content-Category.liquid`, listing through `Category-TermPart.liquid`). When the narration ends, the page shows
  the heading **Travel** and the two summaries, "A weekend in Lisbon" above "Man must explore, and this is
  exploration at its greatest" (newest first, `DefaultContentsTaxonomyListQueryService.cs:47`).
- `site-3`: no click. `HoldAsync` on the same page, moving the cursor slowly over the list. When the narration
  ends, the Travel page is still on screen, unchanged.

### Slides

- `why-1`: three cards, "Typed by hand", "Named differently", "Hard to browse", all bright.
- `why-2`: a diagram with the taxonomy "Categories" on the left, the terms "Travel" and "Food", and the child terms
  "Recipes" and "Restaurants" under Food.
- `why-3`: three cards, "Taxonomy", "Term" and "Taxonomy Field", with the last one bright.
- `more-1`: three cards, "Taxonomy Field settings", "Taxonomy Filters" and "Manage taxonomies"; the first is bright.
- `more-2`: the same cards, the second bright.
- `more-3`: the same cards, the third bright.

The opening card (`intro`) and the closing card (`outro`) are drawn by `assemble.py`.

## Where each label comes from

| Label | Source |
|---|---|
| Tools, Features; Search; Showing N of M features | `OrchardCore.Features/AdminMenu.cs:26-27`, `Views/Admin/Features.cshtml:27,57-59` |
| Taxonomies, Taxonomies Contents List Filters, Content Management | `OrchardCore.Taxonomies/Manifest.cs:11-23` |
| Enabled, Disabled | `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml` |
| Content Type (filter), Edit | `OrchardCore.Contents/Views/ContentsAdminListFilters.cshtml:8`, `ContentsButtonEdit_SummaryAdmin.cshtml:13` |
| Term Content Type, Add {term type}, Edit, Add, Delete | `OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml:26,48`, `Content.TermAdmin.cshtml:27,36,45` |
| Title, Alias, Permalink, Route contained items | `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`, `OrchardCore.Alias/Views/AliasPart.Edit.cshtml:10`, `OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30,100` |
| Publish | `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7,19` |
| List BlogPost Items, Create Blog Post | `OrchardCore.Lists/Views/ListPartSummaryAdmin.cshtml:10` (technical type name), `ListPartNavigationAdmin.cshtml:52` (display name) |
| Tags, Category (field names), Unique, Leaves only | `blog.recipe.json:643-673`, `OrchardCore.Taxonomies/Views/TaxonomyFieldSettings.Edit.cshtml:56,66` |
| Required, Open | `TaxonomyFieldSettings.Edit.cshtml:46`, `TaxonomyFieldTagsEditorSettings.Edit.cshtml` |
| Press enter to create a tag, Type to search | `OrchardCore.Taxonomies/Services/TaxonomiesJSLocalizer.cs:16-20` |
| Design, Content Definition | `OrchardCore.ContentTypes/AdminMenu.cs:23-24` |
| Settings, Taxonomy Filters | `OrchardCore.Taxonomies/AdminMenu.cs:27` |
| Manage taxonomies | `OrchardCore.Taxonomies/Permissions.cs:8` |
| Taxonomy Field (field type name) | `OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml:62` (the type name made readable) |

## Checks for private data

- Only the invented names of this plan appear: admin, the terms, the posts "A weekend in Lisbon" and the post of the
  Blog recipe. The only email address is `admin@example.com`, and it is not shown in any step. No password is typed
  on camera: the recorder signs in outside the capture.
- OCR the video at one frame per second and at every scene change, and search for any name, address, phone number or
  token that is not in this plan. The Blog recipe's footer and its lorem ipsum text are part of the recipe.
- The address bar is not part of a headless recording, so no host name appears.

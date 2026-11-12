# Recording plan: Content Localization

How to record `content-localization` without deciding anything. The narration is in `storyboard.json`; this file says
what is on the site before the first clip, the order of the clips, and for every step what to do, what must be on
screen when its narration ends, and where each label comes from in the Orchard Core source (paths are relative to the
root of `OrchardCMS/OrchardCore`, at the commit recorded in `catalog/features.json`).

In the locators below, a selector stands for `scene.Page.Locator(...)`. Narration, slides and captions of this video
contain no code; only this plan does.

## The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout that has the Content Localization module, on `http://localhost:5010` |
| Data | `ORCHARD_APP_DATA` points at an empty folder outside both repositories |
| Setup | Auto Setup (see the `orchardcore-tester` skill of the Orchard Core repository, `references/autosetup.md`): **Blog** recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` chosen outside git and passed to the recorder in `VIDEO_PASSWORD` |
| Tenant | Default tenant, no URL prefix |
| Outbound calls | Blocked: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address |
| Browser | The recorder's Chromium, light color scheme, `en-US`, so the admin and the culture names are in English |
| External services | None |

The setup creates the role Administrator, and the Blog recipe the roles Author, Contributor, Editor and Moderator
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:88-111`), the content types Article, Blog, Blog Post and
Page, and one Article, **About** (permalink `about`, subtitle "This is what I do.", owned by the setup user,
`blog.recipe.json:1059-1090`). The Article type can be saved as a draft, so **Save Draft** is on its editor
(`blog.recipe.json`, the `ContentTypeSettings` of Article: `Draftable`). The recipe does not enable the Localization
feature. This video translates that Article.

## Seed (outside the capture, once)

The seed creates the cultures. It is not recorded, because the Cultures page belongs to another video, and the Cultures
page only exists once the Localization feature is on. Do it by hand once, or write the same clicks as the video's
`PrepareAsync`.

1. Start the site. Run `Recorder setup` to sign in as `admin`. This writes `state.json`, which is git-ignored.
2. Open `/Admin/Features`. Search for `Localization`. Click **Enable** on the feature named **Localization** (the
   row with the description "Provides support for UI localization."; the link is
   `a[href$="/Features/OrchardCore.Localization/Enable"]`). **Do not enable Content Localization**: that is what
   `enable-2` does on camera. Wait for the notice "The feature Localization was enabled."
3. Open `/Admin/Settings/localization` (menu: Settings, Localization, Cultures). The page has the label "Define which
   cultures are supported by the site.", a table with the columns **Culture** and **Default culture**, a drop-down
   list of cultures and a green **Add culture** button (labels:
   `src/OrchardCore.Modules/OrchardCore.Localization/Services/LocalizationJSLocalizer.cs:16-20`).
   - In the drop-down list choose `fr-FR (French (France))` and click **Add culture**.
   - Choose `es-ES (Spanish (Spain))` and click **Add culture**.
   - Choose `en-US (English (United States))` and click **Add culture** if the table has no `en-US` row.
   - Remove every other row (the trash button, title "Remove culture"). The table must hold exactly `en-US`,
     `fr-FR` and `es-ES`. The site needs at least one culture, so add before removing.
   - Select the radio button of the `en-US` row (title "Set as default").
   - Leave "Fall back to parent culture(s)" as it is. Click **Save**
     (`src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`).
   - Wait for the warning "The site has been restarted for the settings to take effect." Reload the page and check
     the three rows and the default.
4. Open `/Admin/Contents/ContentItems/Article`. The list must hold exactly one item, **About**, with the badges
   "Article" and "Published", and no culture badge. If it holds anything else, the site is not fresh: start again.
5. Stop the site. Copy its `App_Data` folder and `state.json` aside as the snapshot **start**. Start the site again.

No content is created in the seed. Every French and Spanish item is created on camera.

## Order of the clips, and the snapshots

The clips run in the order of the storyboard, and each one starts from the state the previous one left. Chapter 01 has
no recording (three slides).

| Order | Clip | Changes the site | Needs |
|---|---|---|---|
| 1 | `02-enable` | Yes: enables Content Localization | snapshot **start** |
| 2 | `03-part` | Yes: adds the Localization part to Article | `02-enable` |
| 3 | `04-original` | Yes: publishes About, which receives its culture | `03-part` |
| 4 | `05-translate` | Yes: creates and publishes the French item | `04-original` |
| 5 | `06-manage` | Yes: creates the Spanish draft | `05-translate` |
| 6 | `07-site` | No | `06-manage` |
| 7 | `08-permissions` | No: never click **Save** on the role | `02-enable` |

After each clip that changes the site, stop the site and copy `App_Data` and `state.json` aside as **after-02**,
**after-03**, and so on. To record one clip again, stop the site, restore the snapshot of the clip before it, and
start the site. For a full take, restore **start**. The site restarts its tenant when a feature is enabled, so the
first request after `02-enable` can take a few seconds.

Every clip opens its first page with `OpenAsync` (not recorded), then calls `StartCaptureAsync`. The admin menu is
expanded in the sidebar before the capture, with no section opened, when a step clicks through it.

## Slides (chapter 01, clip `01-how`)

Drawn by `slides.py`; nothing to record.

- `how-1`: three cards, one for each culture (About in en-US, À propos in fr-FR, Acerca de in es-ES), each with its
  own title, subtitle and status, joined by a bar labeled "Linked by the Localization part".
- `how-2`: two cards, "Culture" and "Localization set", the two values of the part.
- `how-3`: three numbered cards: create the item, create a translation, translate and publish.

## Clip `02-enable`

Starts on `/Admin` (any admin page). The Features page and the Cultures page are reached on camera through the menu.

### `enable-1`

- Click **Tools** in the admin menu, then **Features** (`ClickAndWaitForNavigationAsync`). Labels:
  `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`.
- `TypeAsync` in `#search-box` (placeholder "Search"): `Content Localization`. The list filters as you type.
- `HighlightAsync` the row titled **Content Localization**, in the group heading **Internationalization**
  (`src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27,88`; the category and name come from
  `src/OrchardCore.Modules/OrchardCore.ContentLocalization/Manifest.cs:12-18`). Rows for Content Culture Picker and
  Localized Content Item Sitemaps can also match the search; keep the Content Localization row visible.
- On screen at the end: the Features page, "Content Localization" in the search box, its row with a blue **Enable**
  button.

### `enable-2`

- `ClearHighlightsAsync`. `HighlightAsync` the **Enable** button of the Content Localization row:
  `a[href$="/Features/OrchardCore.ContentLocalization/Enable"]` (the exact feature id keeps it apart from the
  Content Culture Picker row; the button is
  `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
- `ClickAndWaitForNavigationAsync` on it. The tenant restarts, so wrap the wait for the notice in
  `OffCameraAsync(() => Page.GetByText("The feature Content Localization was enabled.").WaitForAsync())`
  (`src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`).
- The Localization feature was enabled by the seed, and the narration says so
  (`src/OrchardCore.Modules/OrchardCore.Localization/Manifest.cs:10-16` and
  `OrchardCore.ContentLocalization/Manifest.cs:16`, the dependencies).
- On screen at the end: the Features page with the green notice "The feature Content Localization was enabled."

### `enable-3`

- Click **Settings**, then **Localization**, then **Cultures** in the admin menu (`ClickAndWaitForNavigationAsync` on
  **Cultures**; URL `/Admin/Settings/localization`). Labels:
  `src/OrchardCore.Modules/OrchardCore.Localization/AdminMenu.cs:34-38`. With only the Content Localization feature on,
  the Localization section holds just **Cultures**.
- `HighlightAsync` the table header **Default culture**, then the table rows `en-US`, `fr-FR`, `es-ES`
  (`src/OrchardCore.Modules/OrchardCore.Localization/Services/LocalizationJSLocalizer.cs:16-20`).
- On screen at the end: the label "Define which cultures are supported by the site.", the table with the three rows,
  the `en-US` radio button selected, and the **Add culture** button below. Change nothing and do not click **Save**.

## Clip `03-part`

Starts on `/Admin` (any admin page).

### `part-1`

- Click **Design**, **Content Definition**, **Content Types** (`ClickAndWaitForNavigationAsync` on **Content Types**;
  URL `/Admin/ContentTypes/List`). Labels:
  `src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-27`.
- `ClickAndWaitForNavigationAsync` on `[role="btn-edit-Article"]` (the **Edit** button of the Article row,
  `src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`).
- On screen at the end: the page titled "Edit Content Type - Article" with the **Parts** list (Title, Autoroute, HtmlBody
  and Article, and no Localization; a part without a display name shows its name without the word "Part", so the body part
  reads HtmlBody, `Edit.cshtml:152` and `ContentPartSettingsExtensions.cs:54-62`) and the **Add Parts** button below it
  (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63,144,201`).

### `part-2`

- `ClickAndWaitForNavigationAsync` on the link **Add Parts** (`GetByRole(AriaRole.Link, new() { Name = "Add Parts", Exact = true })`;
  not **Add Named Part**). URL `/Admin/ContentTypes/AddPartsTo/Article`.
- The page says "Choose the Parts to add to this Content Type." and lists the attachable parts, each with a check box
  and a hint
  (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:18,32-34`). Scroll until the item
  labeled **Localization** is on screen (`ScrollAsync`); its hint reads "Provides a way to create localized version of
  content." (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Migrations.cs:25-26`). The label is "Localization"
  because the display name drops the word "Part"
  (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-62`).
- `ClickAsync` the check box of **Localization** (`GetByLabel("Localization", new() { Exact = true })`).
- Wait until the button **Add Selected Parts** is visible: it is hidden until a box is checked
  (`AddPartsTo.cshtml:47`). `HighlightAsync` it. Do not click it yet.
- On screen at the end: the **Localization** item checked, and the highlighted **Add Selected Parts** button.

### `part-3`

- `ClearHighlightsAsync`. `ClickAndWaitForNavigationAsync` on **Add Selected Parts**. The page "Edit Content Type -
  Article" opens. Wait for the success notice `The "LocalizationPart" part has been added.` (it names the part, not its
  display name: `src/OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs:372`).
- `HighlightAsync` the new **Localization** row of the Parts list (it has the same hint text).
- Do **not** click **Save**: the part is added to the type at once
  (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:143-148`).
- On screen at the end: the notice and the Parts list with **Localization** highlighted, after the Article part.

## Clip `04-original`

Starts on `/Admin/Contents/ContentItems/Article`: the list of Article items (the page is titled "Article", with the
breadcrumb "Manage Content", `src/OrchardCore.Modules/OrchardCore.Contents/Views/Admin/List.cshtml:14-35`), with one
row, **About**.

### `original-1`

- The narration says to open Content, Content Items. `ClickAndWaitForNavigationAsync` on **Content**, then **Content
  Items** (`src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65-68`), then narrow the list to the Article type
  with the list's content type filter (the exact control is in OPEN-QUESTIONS.md, item 11); the page ends on
  `/Admin/Contents/ContentItems/Article`.
- `HighlightAsync` the About row's badges (type "Article", status "Published") and then its buttons **Edit**, **View**
  and **Actions**, to show that the row has no globe badge and no **Localizations** button. The badge and the button
  only appear when the item has a culture and a localization set
  (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Views/LocalizationPart.SummaryAdmin.cshtml:3-5` and
  `Views/LocalizationPart.SummaryAdminLinks.cshtml:3-9`).
- On screen at the end: the About row without a culture badge and without a **Localizations** button.

### `original-2`

- `ClearHighlightsAsync`. `ClickAndWaitForNavigationAsync` on the **Edit** button of the About row
  (`li.list-group-item:has(a:text-is("About")) a.btn.edit`;
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`). The page is
  "Edit Article" with the fields Title, Permalink, the body, Subtitle and Banner Image.
- Scroll to the bottom (`#contentLocalizationToggle` into view). The Localization part is the last part of the type,
  so it is below the other fields (`ContentDefinitionService.cs:481-492` appends it).
- `ClickAsync` the bar `#contentLocalizationToggle button`. It reads "Current Culture: English (United States) |
  en-US" and opens a card with the text "Please save your content item before creating localizations"
  (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Views/LocalizationPart.Edit.cshtml:10-16,45`).
- On screen at the end: the open card with the message, and the bar above it.

### `original-3`

- Scroll to the bottom action bar. `HighlightAsync` the green **Publish** button, then `ClickAndWaitForNavigationAsync`
  on `button[name="submit.Publish"][value="submit.Publish"]` (not the split-button menu item "and continue";
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/Content.PublishButton.cshtml:11-27`). Because the editor was
  opened from the list, Publish returns to the list
  (`src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:858`).
- The first save gives the item the default culture and creates its localization set
  (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Drivers/LocalizationPartDisplayDriver.cs:47-60`).
- `HighlightAsync` the new globe badge (`en-US`, tooltip "Culture") and the **Localizations** button, which is the
  first button of the row's actions.
- On screen at the end: the list with the notice "Your Article has been published." and the About row with the badge
  `en-US` and the **Localizations** button.

## Clip `05-translate`

Starts on `/Admin/Contents/ContentItems/Article`, with the About row showing `en-US`.

### `translate-1`

- `ClickAndWaitForNavigationAsync` on **Edit** of the About row. Scroll to `#contentLocalizationToggle`; `ClickAsync`
  `#contentLocalizationToggle button`. The card lists the cultures other than the current one, as links:
  "French (France) | fr-FR" and "Spanish (Spain) | es-ES", each with a plus icon (`i.fa-plus`) and the title "Create
  French (France)" or "Create Spanish (Spain)" (`LocalizationPart.Edit.cshtml:34-38`).
- `HighlightAsync` the two plus icons.
- On screen at the end: the bar "Current Culture: English (United States) | en-US" and the card with the two links.

### `translate-2`

- `ClickAndWaitForNavigationAsync` on `#contentLocalizationCard a[title="Create French (France)"]`. The link posts to
  the Localize action (the page's script turns the link into a POST), which copies the item as a draft and opens the
  copy in the editor
  (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Controllers/AdminController.cs:36-76`,
  `DefaultContentLocalizationManager.cs:66-99` and `src/OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:976-984`).
- Wait for the notice "Localized version of the content created successfully." (an information notice).
- On screen at the end: the top of the "Edit Article" page of the French copy, with the notice.

### `translate-3`

- `ScrollAsync` so that the Title field, the Permalink field and the Subtitle field are all on screen if the window
  allows it; otherwise scroll from the top to the Subtitle in the time of the step.
- `HighlightAsync` the Title field (value "About"), then the Subtitle field (value "This is what I do."), then the
  empty **Permalink** field. The permalink is empty because the localization clears it
  (`src/OrchardCore.Modules/OrchardCore.Autoroute/Handlers/AutorouteContentLocalizationHandler.cs:14-15`); the hint
  under it reads "The url of the content item. Leave empty to auto-generate it."
  (`src/OrchardCore.Modules/OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30,46`).
- On screen at the end: the Permalink field, empty, highlighted, with the English title above it.

### `translate-4`

- `ClearHighlightsAsync`. In the Title field (`GetByLabel("Title", new() { Exact = true })`, label from
  `src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`): select all (`Ctrl+A`), then `TypeAsync`
  `À propos`.
- In the Subtitle field (`GetByLabel("Subtitle", new() { Exact = true })`, label from
  `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:685`): select all, then `TypeAsync`
  `Voici ce que je fais.`
- Scroll to `#contentLocalizationToggle` and click its button. The bar now reads "Current Culture: French (France) |
  fr-FR". The card lists "English (United States) | en-US" with a pen icon (`i.fa-pen-to-square`, title "Edit
  English (United States)") and "Spanish (Spain) | es-ES" with a plus icon.
- On screen at the end: the open card with the pen and the plus, and the bar for French (France).

### `translate-5`

- Scroll to the bottom action bar. `ClickAndWaitForNavigationAsync` on
  `button[name="submit.Publish"][value="submit.Publish"]`. It returns to the Article list.
- The permalink is generated from the title on save
  (`src/OrchardCore.Modules/OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:397-440`). Note it for `site-2`: it
  should be `a-propos` (the slug drops the accent,
  `src/OrchardCore/OrchardCore/Modules/Services/SlugService.cs:25-36`).
- On screen at the end: the list with the notice "Your Article has been published." and two rows, **À propos** with
  `fr-FR` and **About** with `en-US`, both "Published".

## Clip `06-manage`

Starts on `/Admin/Contents/ContentItems/Article`, with the rows À propos and About.

### `manage-1`

- `ClickAsync` the **Localizations** button of the **About** row
  (`li.list-group-item:has(a:text-is("About")) button.localizations`;
  `src/OrchardCore.Modules/OrchardCore.ContentLocalization/Views/LocalizationPart.SummaryAdminLinks.cshtml:7-9`).
  The menu holds "French (France) | fr-FR" with a pen icon and "Spanish (Spain) | es-ES" with a plus icon
  (`SummaryAdminLinks.cshtml:17,21`). `HighlightAsync` the two icons.
- Do not click an entry yet.
- On screen at the end: the open menu of the About row, with the pen icon next to French (France) | fr-FR and the plus
  icon next to Spanish (Spain) | es-ES, both highlighted.

### `manage-2`

- `ClearHighlightsAsync`. `ClickAndWaitForNavigationAsync` on `a.dropdown-item[title="Create Spanish (Spain)"]`. This
  opens the Spanish copy of the English item as a draft, with the notice "Localized version of the content created
  successfully." Wait for that notice.
- Title: select all, `TypeAsync` `Acerca de`. Subtitle: select all, `TypeAsync` `Esto es lo que hago.`
- Scroll to the bottom action bar. `HighlightAsync` the blue **Save Draft** button
  (`src/OrchardCore.Modules/OrchardCore.Contents/Views/Content.SaveDraftButton.cshtml:16-19`), then
  `ClickAndWaitForNavigationAsync` on `button[name="submit.Save"][value="submit.Save"]`. It returns to the list.
- On screen at the end: the list with the notice "Your Article draft has been saved." and the row **Acerca de** with
  the badge "Draft".

### `manage-3`

- `ClearHighlightsAsync`. `HighlightAsync`, one after the other, the badges of each row: **Acerca de** ("Draft",
  `es-ES`), **À propos** ("Published", `fr-FR`), **About** ("Published", `en-US`). Badges:
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:11-19` and
  `LocalizationPart.SummaryAdmin.cshtml:5`.
- On screen at the end: the three rows, each with its own status badge and culture badge.

### `manage-4`

- `ClearHighlightsAsync`. `ClickAsync` the drop-down titled **Culture**
  (`div.bootstrap-select:has(select.culture-selectpicker) > button`). Its header reads "Filter by culture" and its
  options are **All cultures**, `en-US`, `fr-FR`, `es-ES`
  (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Views/ContentsAdminList-LocalizationPartFilter.cshtml:4`
  and `Drivers/LocalizationContentsAdminListDisplayDriver.cs:43-56`).
- Click the option `fr-FR`. Wait until the list shows only the row **À propos**. The list script clicks the filter
  button when the selection changes, so the list reloads by itself
  (`src/OrchardCore.Modules/OrchardCore.Contents/wwwroot/Scripts/contents-admin-list/contents-admin-list.js`, the
  `changed.bs.select` handler of every `.selectpicker` without the class `nosubmit`; the Culture drop-down has
  `selectpicker culture-selectpicker` and no `nosubmit`; the handler is installed because the page has the wrapper
  `.bulk-select-list`, `Views/ContentsAdminList.cshtml:12`, and the checkbox `#select-all`,
  `Views/ContentsAdminListSummary.cshtml:5`). If the list has not reloaded after three seconds, click the
  magnifier button of the search bar (`button[name="submit.Filter"]`).
- Open the drop-down again, click **All cultures**, and wait until the three rows are back (same fallback).
- On screen at the end: the list with the three rows again, and the **Culture** drop-down closed.

## Clip `07-site`

Starts on `/Admin/Contents/ContentItems/Article` with the three rows. The recording crosses to the public site, so
nothing in this clip is edited.

### `site-1`

- `ClickAsync` the **View** button of the About row (`li.list-group-item:has(a:text-is("About")) a.btn.view`,
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`). It opens a new tab;
  wait for it (`Context.WaitForPageAsync`) and call `SwitchToAsync(page)`.
- Wait for the heading `h1` with the text "About". The Blog theme shows the title and the subtitle "This is what I do."
  in the page header
  (`src/OrchardCore.Themes/TheBlogTheme/Views/Content-Article.liquid:14-17`).
- The capture does not show the browser address bar, so `HighlightAsync` the `h1` with the caption `/about`.
- On screen at the end: the English page, with the highlighted heading and the caption `/about`.

### `site-2`

- `ClearHighlightsAsync`. `NavigateAsync("/a-propos")` in the same tab. If the permalink noted at `translate-5` is
  different, use that one and the same value in the caption.
- Wait for the heading `h1` with the text "À propos". `HighlightAsync` it with the caption `/a-propos`.
- On screen at the end: the French page with the title "À propos" and the subtitle "Voici ce que je fais.", and the
  caption.

### `site-3`

- `ClearHighlightsAsync`. No action: hold on the French page (`HoldAsync`). Do not open the Spanish item: it is a draft
  and has no public page.
- On screen at the end: the French page, without a highlight.

## Clip `08-permissions`

Starts on `/Admin` (any admin page). Never click **Save** here: the clip only reads.

### `perm-1`

- Click **Access Control**, then **Roles** (`ClickAndWaitForNavigationAsync` on **Roles**; URL `/Admin/Roles`;
  `src/OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19`).
- `ClickAndWaitForNavigationAsync` on the **Edit** button of the **Author** row
  (`a.btn.edit[href$="/Roles/Edit/Author"]`, which cannot match another row;
  `src/OrchardCore.Modules/OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`).
  URL `/Admin/Roles/Edit/Author`; the roles of the Blog recipe are in `blog.recipe.json:88-111`.
- `TypeAsync` in `#search-box` (placeholder "Search permissions",
  `src/OrchardCore.Modules/OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49`): `Localize`.
- The filter keeps one group, titled **Content Localization** with the hint `(OrchardCore.ContentLocalization)`, and two
  rows: "Localize content for others" and "Localize own content"
  (`src/OrchardCore/OrchardCore.ContentLocalization.Abstractions/ContentLocalizationPermissions.cs:8-10`; group
  title from the feature name, `src/OrchardCore.Modules/OrchardCore.Roles/Controllers/AdminController.cs:412-427` and
  `_RolePermissions.cshtml:57-63`). The third permission of the module, "Manage ContentCulturePicker settings", does
  not match the search.
- On screen at the end: the Author role page with the search box containing `Localize`, the group heading and the two
  rows, with the columns **Permission**, **Allow** and **Effective**.

### `perm-2`

- `HighlightAsync` the row "Localize content for others", then the row "Localize own content". Do not click the check
  boxes.
- On screen at the end: both rows highlighted in turn. On the Author role, "Localize content for others" is not
  checked, and "Localize own content" is checked in both columns (see step `perm-3` for why).

### `perm-3`

- `ClearHighlightsAsync`. `HighlightAsync` the **Allow** cell of "Localize own content", then the **Allow** cell of
  "Localize content for others".
- Where the defaults come from: the module's default permissions for roles named Administrator, Editor, Author and
  Contributor (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Permissions.cs:22-45`), applied to existing roles
  when the feature is installed (`src/OrchardCore.Modules/OrchardCore.Roles/Services/RoleUpdater.cs:44-95`). Before
  recording this clip, confirm on screen that the Author role has the state described above. If it does not, stop and
  add it to `OPEN-QUESTIONS.md`: the narration of `perm-3` then needs a change.
- The edit permission of the type is also required
  (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Controllers/AdminController.cs:55`).
- On screen at the end: the two rows, with the **Allow** cells highlighted. Leave the page without saving.

## Checks for private data

- The site is fresh, so it holds no real name, email or number. The only account is `admin` with the email
  `admin@example.com`; the password is never typed on camera (the session comes from `state.json`).
- The titles typed on camera are fake: About, À propos, Acerca de, and the subtitles "This is what I do.", "Voici ce
  que je fais.", "Esto es lo que hago."
- OCR the finished video at one frame per second and at every scene change, as the skill describes, and look for
  anything that is not one of the strings above, the site name "Orchard Core", `admin` or `example.com`. The browser
  address bar is not captured; the captions `/about` and `/a-propos` are the only paths shown.
- Do not commit `state.json`, a copy of `App_Data`, the snapshots, or the password.

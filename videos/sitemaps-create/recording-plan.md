# Recording plan: Sitemaps: Help Search Engines Find Your Pages

Everything a person needs to record this video without deciding anything. `storyboard.json` has the narration, and
`OPEN-QUESTIONS.md` lists the few things to check on the running site before recording. In this file a selector stands for
`scene.Page.Locator(...)`. The sources are paths in the Orchard Core checkout (`src/OrchardCore.Modules/...` unless
stated), read at commit `92db4a83`.

## 1. The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore`, on `http://localhost:5010`, with its own `ORCHARD_APP_DATA` folder outside both repositories |
| Provisioning | Auto Setup, so no setup form and no sign-in form is recorded. Recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git and passed in `VIDEO_PASSWORD`; never written in a file of this repository) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address; this video calls no external service |
| Features | Only those the Blog recipe enables. **Sitemaps is not enabled** when the recording starts (clip `02-setup` enables it). Do not enable **SEO**, **Localized Content Item Sitemaps**, **Sitemaps for Decoupled Razor Pages** or **Sitemaps Cleanup** |
| Base url | Empty when the recording starts (clip `02-setup` sets it). The Blog recipe and Auto Setup do not set it |
| Admin settings | The defaults: lists as rows with buttons (`AdminListOptions.DefaultLayout` is `List`, `DefaultActionsLayout` is `Buttons`: `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30-36`) |
| Sign-in | `Recorder setup` once, outside the capture (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD`), which writes `state.json` (git-ignored) |
| Mask script | None. The site is made from scratch with fake data, so there is nothing to mask |

### Seed data (create it once, outside the capture, before taking snapshot S0)

The Blog recipe already creates these routable items, all published: the **Blog** (path `blog`, set as the home page),
the blog post **Man must explore, and this is exploration at its greatest** (path `blog/post-1`), the article **About**
(path `about`), and the taxonomies **Tags** (path `tags`) and **Categories** (path `categories`)
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`).

Add three articles, signed in as `admin`, with **Content > Content Items > New > Article** (page
`/Admin/Contents/ContentTypes/Article/Create`). For each, type the Title and the Body, leave Subtitle and Image empty
unless the form requires them (then type `A short note.` as the Subtitle), and choose **Publish** (a draft is not listed
in a sitemap). The permalink is made from the title.

| Title | Body | Permalink that results |
|---|---|---|
| Spring Planting Calendar | Sow lettuce and peas in March, and start tomatoes indoors in April. | `spring-planting-calendar` |
| Five Herbs for a Windowsill | Basil, chives, mint, parsley, and thyme grow well in pots by a bright window. | `five-herbs-for-a-windowsill` |
| Composting Basics | Mix kitchen scraps with dry leaves, and turn the pile every week. | `composting-basics` |

Check the permalinks in **Content > Content Items** before going on. Create no other content, no other sitemap, and no
other Base url than the one in clip `02-setup`.

## 2. Order, dependencies and snapshots

The clips change the site and each starts from the state the previous one left, so they are recorded in the order they
play. Take a snapshot (copy of the `ORCHARD_APP_DATA` folder, and of `state.json`) at each point below, with the site
stopped. To record one clip again, stop the site, restore the snapshot named in the "Starts from" column, delete the folder
`wwwroot/Default/sm-cache` of the checkout (the sitemap cache: `OrchardCore.Sitemaps/Cache/DefaultSitemapCacheProvider.cs:180-183`),
and start the site again.

| Order | Clip | Starts from | What it changes | Snapshot taken after it |
|---|---|---|---|---|
| 1 | `02-setup` | S0: fresh Blog site plus the seed data, Sitemaps disabled, Base url empty | Enables the Sitemaps feature; sets the Base url to `https://www.example.com` | S1 |
| 2 | `03-create` | S1 | Creates the sitemap **Pages**, permalink `pages.xml` | S2 |
| 3 | `04-source` | S2 | Adds the Sitemap Content Types source (Index All Content Types) to **Pages** | S3 |
| 4 | `05-view` | S3 | Nothing in the settings (the sitemap is built and cached on the first request) | none, S3 stays valid |
| 5 | `06-types` | S3 | Edits the source: only Article and Blog Post, with their own values | S4 |
| 6 | `07-path` | S4 | Adds a Sitemap Custom Path source with the path `tags` | S5 |
| 7 | `08-part` | S5 | Attaches the Sitemap part to the Article type; excludes the article About | none (last clip) |

Slide clips `01-what` and `09-reach` are drawn from `slides.py`; they need no site. The cards `00-intro` and `99-outro`
need none either.

## 3. The clips

A click listed under "Starts on (before the capture)" is made before `StartCaptureAsync` with `scene.Page.Locator(...).ClickAsync()`, and is not recorded. After the click of **Manage Sources**, wait until the button `button[data-bs-target="#modalSources"]` (**Add Source**) is visible, which shows that the page of the sitemap has loaded.
Every Create, Save, Add, Publish and Enable click that loads a new page goes through `ClickAndWaitForNavigationAsync`; a
click that only opens or changes something on the same page (the **Add Source** button, which opens a modal, and the
checkboxes) goes through `scene.ClickAsync`, because there is no navigation to wait for. A page that opens in
another tab goes through `SwitchToAsync`. Choose options of a list by their visible label. Keep each step's actions shorter
than its narration. Where this plan says "highlight", use `HighlightAsync` and clear it before scrolling or navigating. Enabling a feature and
saving the General settings reload the site (the shell): wait for the reload inside `scene.OffCameraAsync(...)`, with a
condition that only the new state meets (named in the step), so the wait is cut out of the clip.

### `01-what` (slides, chapter 01)

- `what-1`: the slide "What a sitemap lists": four cards (Address, Last modified, Change Frequency, Priority), all bright.
- `what-2`: the slide "A sitemap in Orchard Core": four cards (Name, Permalink, Enabled, Sources), all bright.
- `what-3`: the slide "Two kinds of source": two cards (Sitemap Content Types, Sitemap Custom Path).

### `02-setup` (clip, chapter 02): enable the module, set the Base url

Starts on: `OpenAsync("/Admin")` (the admin home page, signed in). Needs S0.

- `setup-1`: In the admin menu click **Tools**, then **Features** (`ClickAndWaitForNavigationAsync`; if the actions of this step still run longer than its narration, open the page with `NavigateAsync("/Admin/Features")` instead of the two menu clicks). Click the search box `#search-box` and type `Sitemaps`. The list keeps the rows whose name or id contains the
  text: **Sitemaps**, **Sitemaps for Decoupled Razor Pages**, **Sitemaps Cleanup**, and **Localized Content Item
  Sitemaps**. Highlight the row whose name is exactly **Sitemaps** (its `<code>` shows the id `OrchardCore.Sitemaps`; select
  it with a `code` locator that matches `^OrchardCore\.Sitemaps$` inside a `.list-group-item`). Click the **Enable** button of
  that row, inside the row (the button is `a#btn-enable-OrchardCore_Sitemaps` by the view's rule, check it on the page). Wait
  for the reload off camera (`OffCameraAsync`, condition: that row shows a **Disable** button instead of **Enable**). Visible at the end: the Features page after
  the redirect, the Sitemaps row showing **Disable**, the success notification at the top.
  Source: `OrchardCore.Features/Views/Admin/Features.cshtml:27` (search box), `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable), `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11` (name and id), `OrchardCore.Sitemaps/Manifest.cs:11-12` (name Sitemaps).
- `setup-2`: In the admin menu click **Tools**, then **Search Engine Optimization**, so that its three items **Sitemaps**,
  **Sitemap Indexes** and **Sitemaps Cache** are visible. Highlight the three items for about two seconds, clear the
  highlight, then click **Sitemaps** (the link whose accessible name is exactly "Sitemaps") with `ClickAndWaitForNavigationAsync`.
  Visible at the end: the Sitemaps page (`/Admin/Sitemaps/List`).
  Source: `OrchardCore.Sitemaps/AdminMenu.cs:17-31`.
- `setup-3`: Nothing is clicked. Highlight the yellow warning at the top of the page, `.alert.alert-warning`, whose text is
  "The site settings 'Base url' property is empty, but is required to generate sitemaps." Visible at the end: the warning, and
  under it the "Nothing here! There are no sitemaps for the moment." message.
  Source: `OrchardCore.Sitemaps/Views/Admin/List.cshtml:9-12` and `:39`.
- `setup-4`: In the admin menu click **Settings**, then **General** (fallback: `NavigateAsync("/Admin/Settings/general")`).
  The first tab, **Site**, holds the field **Base url**. Click the Base url field (`input[type=url]` whose name ends with
  `BaseUrl`, the one that is not the CDN field), type `https://www.example.com` (use `TypeAsync` with a delay of 30), press Tab, then click the **Save** button
  (`button.save`). Wait for the reload off camera (`OffCameraAsync`, condition: the notification "Site settings updated successfully." is shown).
  The narration has three sentences; if the actions still run longer than it, open the page with `NavigateAsync` instead of the two menu clicks.
  Visible at the end: the General settings page with that notification and the Base url field showing `https://www.example.com/`
  (the trailing slash is added by the site).
  Source: `OrchardCore.Settings/AdminMenu.cs:36-45`, `OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:33-38`, `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`, `OrchardCore.Settings/Controllers/AdminController.cs:119`.

### `03-create` (clip, chapter 03): create a sitemap

Starts on: `OpenAsync("/Admin/Sitemaps/List")`. Needs S1. The page shows no warning and the "Nothing here!" message.

- `create-1`: Click **Add Sitemap** (`a.create`) with `ClickAndWaitForNavigationAsync`. On the "Create Sitemap" page click the
  Name field (`#Name`) and type `Pages`. Visible at the end: the form with Name `Pages`, the Permalink field empty, Enabled checked.
  Source: `OrchardCore.Sitemaps/Views/Admin/List.cshtml:30`, `OrchardCore.Sitemaps/Views/Admin/Create.cshtml:17-19`.
- `create-2`: Do not type in Permalink. Highlight the Permalink field (`#Path`) together with the text before it
  (`https://www.example.com/`) and the hint under it, "The url of the sitemap. Leave empty to auto-generate it."
  Visible at the end: the highlighted Permalink group.
  Source: `OrchardCore.Sitemaps/Views/Admin/Create.cshtml:24-37`, `OrchardCore.Sitemaps/Services/SitemapHelperService.cs:75-78` (name made into a slug plus `.xml`).
- `create-3`: Highlight the **Enabled** checkbox (`#Enabled`, checked) and its hint "Uncheck to disable routing to this
  sitemap." Clear the highlight, then click **Create** (`button.create`) with `ClickAndWaitForNavigationAsync`.
  Visible at the end: the Sitemaps list (the page the form redirects to; there is no success message).
  Source: `OrchardCore.Sitemaps/Views/Admin/Create.cshtml:39-52`, `OrchardCore.Sitemaps/Controllers/AdminController.cs:189-223`.
- `create-4`: Wait for the row **Pages** to show. Highlight its **Enabled** badge, then the buttons **Manage Sources**,
  **View**, **Edit** and **Delete** at the end of the row (do not click any), and give the highlight of **View** the caption
  `/pages.xml`, the generated permalink (the list does not show it; the Edit page does). The row also holds an **Actions**
  button after the four buttons, which opens the menu with **Disable**: leave it closed. Visible at the end: the row with the badge and the four buttons.
  Source: `OrchardCore.Sitemaps/Views/SitemapListEntry.DefaultTags.SummaryAdmin.cshtml:6-9`, `OrchardCore.Sitemaps/Views/SitemapListEntry.Buttons.SummaryAdmin.cshtml:6-15`.

### `04-source` (clip, chapter 04): add a Sitemap Content Types source

Starts on: `OpenAsync("/Admin/Sitemaps/List")`. Needs S2.

- `source-1`: Click **Manage Sources** in the row **Pages** (`ClickAndWaitForNavigationAsync`). The page "Sitemap" shows
  "Nothing here! There are no sources on this sitemap for the moment." Then click **Add Source** (the button
  `button[data-bs-target="#modalSources"]`, with `scene.ClickAsync`: it opens a modal and does not navigate). Visible at the end: the modal "Available Sources" opening.
  Source: `OrchardCore.Sitemaps/Views/SitemapListEntry.Buttons.SummaryAdmin.cshtml:6`, `OrchardCore.Sitemaps/Views/Admin/Display.cshtml:17-19`, `:46-48`.
- `source-2`: Wait for `#modalSources.show`. Highlight the two cards
  **Sitemap Content Types** ("Add sitemap entries for each one of the selected content types.") and **Sitemap Custom Path**
  ("Add a sitemap entry for a path."). Clear the highlight, then click **Add** in the card of Sitemap Content Types
  (`#modalSources .card` that has the text "Sitemap Content Types", then its `a` with the text "Add") with
  `ClickAndWaitForNavigationAsync`. Visible at the end: the page "Create Sitemap Source".
  Source: `OrchardCore.Sitemaps/Views/Admin/Display.cshtml:60-82`, `OrchardCore.Contents/Views/Items/ContentTypesSitemapSource.Thumbnail.cshtml:3-4`, `OrchardCore.Sitemaps/Views/CustomPathSitemapSource.Thumbnail.cshtml:3-4`, `OrchardCore.Sitemaps/Views/SitemapSource.Thumbnail.cshtml:8`.
- `source-3`: Highlight the checkbox **Index All Content Types** (`input.content-types-sitemap-index-all`, checked). Do not
  click it. Visible at the end: the highlighted checked box, and under it the card "Index All Content Types" with Change Frequency and Priority.
  Source: `OrchardCore.Contents/Views/Items/ContentTypesSitemapSource.Edit.cshtml:12-15`; the default is checked (`src/OrchardCore/OrchardCore.Sitemaps.Abstractions/Models/ContentTypesSitemapSource.cs:11`).
- `source-4`: Highlight the **Change Frequency** list (showing **Daily**) and the **Priority** list
  (showing **0.5**) of the card "Index All Content Types" (`#index-all-row select`, two lists). Change nothing. Clear the
  highlight, then click **Create** (`button.create`) with `ClickAndWaitForNavigationAsync`. Visible at the end: the sitemap
  page with the success notification "Sitemap source added successfully."
  Source: `OrchardCore.Contents/Views/Items/ContentTypesSitemapSource.Edit.cshtml:37-62`, `OrchardCore.Sitemaps/Views/Source/Create.cshtml:24-27`, `OrchardCore.Sitemaps/Controllers/SourceController.cs:116`.
- `source-5`: Highlight the badge **All** and the text "Sitemap Content Types" next to it, in the list of sources. Visible at the end: the list item with the badge.
  Source: `OrchardCore.Contents/Views/Items/ContentTypesSitemapSource.SummaryAdmin.cshtml:18-21`, `:38`.

### `05-view` (clip, chapter 05): view the sitemap

Starts on (before the capture): `OpenAsync("/Admin/Sitemaps/List")`, click **Manage Sources** in the row **Pages**. Needs S3.
Before the first take, open `http://localhost:5010/pages.xml` once by hand and read `OPEN-QUESTIONS.md`, question 1.

- `view-1`: Click **View** (`a.btn-success`, text "View", target `_blank`) while waiting for the popup:
  `var popup = await scene.Page.RunAndWaitForPopupAsync(() => scene.ClickAsync(...));` then `await scene.SwitchToAsync(popup);`.
  Wait until the page shows the sitemap. Visible at the end: the browser's XML tree of `/pages.xml`, from its top.
  Source: `OrchardCore.Sitemaps/Views/Admin/Display.cshtml:21-24`.
- `view-2`: Scroll the XML slowly with `ScrollAsync(0, 300)` twice, pausing about two seconds each time, so that at least
  three complete entries are readable. Each entry has an address, a last modified date (see `OPEN-QUESTIONS.md`, question 2), a change
  frequency of `daily` and a priority of `0.5`. The code builds each address as the Base url (which ends in `/`) followed by
  a path that starts with `/`, so an address is expected to read `https://www.example.com//about`: this is the stop condition
  of `OPEN-QUESTIONS.md`, question 1, to settle before recording this clip and every clip after it. Visible at the end: complete entries in the middle of the screen.
  Source: `OrchardCore.Contents/Sitemaps/ContentTypesSitemapSourceBuilder.cs:105-107`, `:150-196`.

### `06-types` (clip, chapter 06): choose content types

Starts on (before the capture): `OpenAsync("/Admin/Sitemaps/List")`, click **Manage Sources** in the row **Pages**. Needs S3.

- `types-1`: Click **Edit** on the source (the `a.btn-primary` with the text "Edit" in the list item "All / Sitemap Content
  Types") with `ClickAndWaitForNavigationAsync`. On the page "Edit Sitemap Source" click the checkbox **Index All Content
  Types** to uncheck it. Visible at the end: the unchecked box, the new checkbox **Limit Items** under it, and the card
  "Select Content Types" with the list of types.
  Source: `OrchardCore.Sitemaps/Views/SitemapSource.SummaryAdmin.cshtml:5-9`, `OrchardCore.Sitemaps/Views/Source/Edit.cshtml:6`, `src/OrchardCore.Modules/OrchardCore.Contents/Assets/ts/content-types-sitemap-source.ts` (the panels switch on the checkbox).
- `types-2`: Highlight the list of types (`#index-selected-row`): the display names in alphabetical order, Article, Blog,
  Blog Post, Category, Page, Tag, Taxonomy. Check **Article**, then **Blog Post**, by clicking the checkbox of the row whose
  label text is exactly that (`#index-selected-row li` that has a `label` with the text "Article", then its checkbox). The
  Change Frequency and Priority lists of a row only become usable after its box is checked. Visible at the end: the two checked rows.
  Source: `OrchardCore.Contents/Views/Items/ContentTypesSitemapSource.Edit.cshtml:68-132`, `OrchardCore.Contents/Sitemaps/ContentTypesSitemapSourceDriver.cs:29-39` (alphabetical order), `OrchardCore.Autoroute/Sitemaps/AutorouteContentTypeProvider.cs:45` (only types with the Autoroute part).
- `types-3`: In the **Blog Post** row choose **Weekly** in Change Frequency and **0.8** in Priority (`SelectAsync` with those
  labels). In the **Article** row choose **Monthly** in Change Frequency and leave Priority at **0.5**. Then click **Save**
  (`button.save`) with `ClickAndWaitForNavigationAsync`. Visible at the end: the sitemap page with the success notification
  "Sitemap source updated successfully."
  Source: `OrchardCore.Contents/Views/Items/ContentTypesSitemapSource.Edit.cshtml:96-122`, `OrchardCore.Sitemaps/Views/Source/Edit.cshtml:29`, `OrchardCore.Sitemaps/Controllers/SourceController.cs:189`.
- `types-4`: Highlight the two badges of the source (**Article** and **Blog Post**, with "Sitemap Content Types" next to
  them). Clear the highlight. Click **View** and switch to the popup as in `view-1`. Scroll once with `ScrollAsync(0, 300)`.
  Visible at the end: the XML with only the addresses of the blog post and the three articles plus About (no Blog, Tags or
  Categories), the blog post showing `weekly` and `0.8`, the articles `monthly` and `0.5`.
  Source: `OrchardCore.Contents/Views/Items/ContentTypesSitemapSource.SummaryAdmin.cshtml:27-33`, `OrchardCore.Contents/Sitemaps/DefaultContentItemsQueryProvider.cs:54-61`.

### `07-path` (clip, chapter 07): add a custom path

Starts on (before the capture): `OpenAsync("/Admin/Sitemaps/List")`, click **Manage Sources** in the row **Pages**. Needs S4.

- `path-1`: Click **Add Source** (`button[data-bs-target="#modalSources"]`), wait for `#modalSources.show`, then click **Add**
  in the card **Sitemap Custom Path** with `ClickAndWaitForNavigationAsync`. Visible at the end: the page "Create Sitemap
  Source" with the heading "Sitemap Custom Path" in the form.
  Source: `OrchardCore.Sitemaps/Views/CustomPathSitemapSource.Edit.cshtml:5`.
- `path-2`: Click the **Path** field and type `tags`. Highlight the hint under it, "The path to add to the sitemap. e.g.
  path/to/something". Visible at the end: the field with `tags`, after the text `https://www.example.com/`, and the hint.
  Source: `OrchardCore.Sitemaps/Views/CustomPathSitemapSource.Edit.cshtml:13-26`, `src/OrchardCore/OrchardCore.Sitemaps.Abstractions/Models/CustomPathSitemapSource.cs:8` (spaces and dots are refused).
- `path-3`: In **Change Frequency** choose **Monthly**; leave Priority at **0.5**. Click **Create** (`button.create`) with
  `ClickAndWaitForNavigationAsync`. Highlight the new list item: the text `tags` and "Sitemap Custom Path". Visible at the end: the
  sitemap page with two sources, the content types one and the custom path one.
  Source: `OrchardCore.Sitemaps/Views/CustomPathSitemapSource.Edit.cshtml:27-33`, `OrchardCore.Sitemaps/Views/CustomPathSitemapSource.SummaryAdmin.cshtml:4`.
- `path-4`: Click **View**, switch to the popup, and scroll to the end of the XML with `ScrollAsync(0, 600)`. Visible at the
  end: the last entry, the address of the Tags page, with `monthly` and `0.5`.
  Source: `OrchardCore.Sitemaps/Builders/CustomPathSitemapSourceBuilder.cs:34-48`.

### `08-part` (clip, chapter 08): control a single page

Starts on: `OpenAsync("/Admin")` (the admin home page). Needs S5.

- `part-1`: In the admin menu click **Design**, **Content Definition**, **Content Types**, in that order. On the Content
  Types page click **Edit** in the row Article (`a[role="btn-edit-Article"]`) with `ClickAndWaitForNavigationAsync`.
  Visible at the end: the page "Edit Content Type - Article" with its parts, listed as Title, Autoroute, HtmlBody and
  Article (the list shows the part name without "Part", so the body part reads **HtmlBody**), and the
  buttons **Add Parts** and **Add Named Part** under them.
  Source: `OrchardCore.ContentTypes/AdminMenu.cs:23-25`, `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`, `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:201` (the buttons), `:153` (part names), `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-64` (name without "Part").
- `part-2`: Click **Add Parts** (`ClickAndWaitForNavigationAsync`). In the list "Choose the Parts to add to this Content
  Type." check **Sitemap** (the row with the label "Sitemap", its description reads "Provides an optional part that allows
  content items to be excluded, or configured, on a content item."). Highlight that description for about two seconds. Click
  **Add Selected Parts** (`#add-selected-parts-button`, shown once a box is checked) with `ClickAndWaitForNavigationAsync`.
  Visible at the end: the page "Edit Content Type - Article" with the notification `The "SitemapPart" part has been added.` (the
  message of **Add Selected Parts** names the part as stored, with "Part"; only **Add & Configure** shows the display name) and
  **Sitemap** in the list of parts. The part is saved by that button, so no other click is needed.
  Source: `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:18`, `:34`, `:47`, `OrchardCore.ContentTypes/Controllers/AdminController.cs:360` and `:372` (the message joins the part names, not the display names), `OrchardCore.Sitemaps/Migrations.cs:18-20` (description; the display name is the part name without "Part": `OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:29`).
- `part-3`: In the admin menu click **Content**, then **Content Items** (`ClickAndWaitForNavigationAsync`). In the row **About**
  click **Edit** (`a.edit` in the row whose title link has the text "About"). On the editor click the tab **SEO** and check the
  box **Override sitemap configuration** (`input[data-sitemap-override-toggle]`). Visible at the end: the SEO tab with the
  box checked and, under it, **Exclude**, **Change Frequency** and **Priority**.
  Source: `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`, `OrchardCore.Sitemaps/Drivers/SitemapPartDisplayDriver.cs:14` (the SEO tab), `OrchardCore.Sitemaps/Views/SitemapPart.Edit.cshtml:5-6`.
- `part-4`: Highlight the **Exclude** checkbox with its hint "Check to exclude this content item from the site map.", then
  the **Change Frequency** and **Priority** lists (do not change them). Clear the highlight, check **Exclude**
  (`input[name$="Exclude"]`), then click **Publish** (`button.publish`, the green main button) with
  `ClickAndWaitForNavigationAsync`. Do not select it by `name="submit.Publish"`: the editor was opened from the list with a
  `returnUrl`, so the Publish button is a split button, and its **and continue** menu item has the same name. Visible at the end:
  the page the editor returns to (the Content Items list), with the notification "Your Article has been published."
  Source: `OrchardCore.Sitemaps/Views/SitemapPart.Edit.cshtml:10-16`, `:18-43`, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19` (main button) and `:24` (menu item), `OrchardCore.Contents/Controllers/AdminController.cs:966-967` (the message of an edit; `:472-473` is the one of a new item).
- `part-5`: `NavigateAsync("/pages.xml")` (the same page the **View** button opens). Scroll once with `ScrollAsync(0, 300)`.
  Visible at the end: the XML with the three articles and the blog post, and no entry for `about`.
  Source: `OrchardCore.Contents/Sitemaps/ContentTypesSitemapUpdateHandler.cs:15-17` (publishing updates the sitemap), `OrchardCore.Contents/Sitemaps/ContentTypesSitemapSourceBuilder.cs:133-139` (an excluded item is skipped).

### `09-reach` (slides, chapter 09)

- `reach-1`: the slide "Sitemaps in the robots file": four cards with check marks (The SEO feature is enabled, The site has
  no robots file of its own, The Base url is set, Include all sitemaps is checked and saved); the first three are bright, the
  fourth is dimmed.
  Source: `OrchardCore.Seo/Startup.cs:44-57` and `OrchardCore.Seo/Services/RobotsMiddleware.cs:30-34` (a robots file on disk is served as it is), `OrchardCore.Sitemaps/Startup.cs:135-141` (the provider needs the SEO feature), `OrchardCore.Sitemaps/Services/SitemapsRobotsProvider.cs:25` (needs a Base url) and `:32` (needs a saved `SitemapsRobotsSettings` whose `IncludeSitemaps` is true), `:47` and `:71` (enabled sitemap indexes first, then enabled sitemaps that no enabled index contains).
- `reach-2`: the same slide with the four cards bright. The label of the fourth is **Include all sitemaps.** on the Robots
  page (`OrchardCore.Sitemaps/Views/SitemapsRobotsSettings.Edit.cshtml:10`), reached by Settings, Search, Search Engine
  Optimization, Robots (`OrchardCore.Seo/AdminMenu.cs:26-37`). See `OPEN-QUESTIONS.md`, question 11.

### Cards

- `intro`: the opening card "Orchard Core Sitemaps / Help search engines find your pages".
- `outro`: the closing card, subtitle `docs.orchardcore.net`.

## 4. Signing in, narrating and recording

Commands for the person who records, run in Git Bash from the root of this repository (`<repo>`). `<bin>` is `%TEMP%/ocvideo/bin/Release/net10.0`.
The site runs on `http://localhost:5010` as described in section 1. This video has no `PrepareAsync`: its only seed is the
content of section 1, created by hand before snapshot S0. `recorder/Scenes.cs` of this folder holds one method per clip,
written from section 3.

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/sitemaps-create/recorder
export VIDEO_ROOT=<repo>/videos/sitemaps-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
<bin>/Recorder setup
cd <repo>
python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/sitemaps-create
<bin>/Recorder record 02-setup 03-create 04-source 05-view 06-types 07-path 08-part
```

Record the clips in the order of the table in section 2, restoring the snapshot named there before another take of a clip.
The slides (`01-what`, `09-reach`) and the cards need no site.

## 5. Checks for private data

- The site is new and holds only the Blog recipe content and the three seed articles. No real name, email or number exists on
  it. The only address is `admin@example.com`, and the Base url is `https://www.example.com`.
- The user menu or an author name can show `admin`; that is a fake account.
- Do not show the browser's address bar or a sign-in form; do not type the password on camera (it is in the sign-in state).
- After assembling, OCR the video at one frame per second and at every scene change (see `SKILL.md`, step 6), and search for
  anything that is not `example.com`, `admin`, the seed titles, or the Blog recipe's text.
- Delete the folder `wwwroot/Default/sm-cache` and the site's `App_Data` copy when the recording is finished; none of it is committed.

# Recording plan: Sitemap Indexes and the Sitemaps Cache

Audience: administrators. Nothing is typed on camera except the two fields of the index form and a search word. The
Orchard Core source paths below are relative to `src/OrchardCore.Modules/` of the checkout used for the site. A selector
written as `"..."` stands for `scene.Page.Locator("...")`.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, run on `http://localhost:5010` with
  its own data folder outside both repositories (`ORCHARD_APP_DATA`), and outbound calls blocked as
  `references/recording.md` says (`HTTP_PROXY` and `HTTPS_PROXY` set to a dead address).
- **Provisioning**: Auto Setup, so no setup or sign-in form is recorded. This is not a video about the Setup screen, so
  Auto Setup is used and the sign-in is saved in `state.json` (git-ignored) by `Recorder setup`.

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>`, the value of `VIDEO_PASSWORD`, chosen outside git |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Tenant**: `Default` only. The sitemap cache of the tenant is the folder `wwwroot/Default/sm-cache` of the web
  project (`src/OrchardCore.Cms.Web/wwwroot/Default/sm-cache`), not part of the data folder
  (`Sitemaps/Cache/DefaultSitemapCacheProvider.cs:180-183`). It is called "the cache folder" below.
- **No static `robots.txt`**: `src/OrchardCore.Cms.Web/wwwroot/robots.txt` must not exist. A file there replaces the
  generated one (`OrchardCore.Seo/Services/RobotsMiddleware.cs:28-33`).
- **Admin lists**: Settings, Admin: "List layout" and "Actions layout" stay on "Use configuration default"
  (`OrchardCore.Admin/Views/AdminSettings.Edit.cshtml:86-100`), so the
  rows of the lists show the buttons **View**, **Edit**, **Delete** and an **Actions** drop-down (the default `List`
  and `Buttons` presentations, `OrchardCore.Admin/Views/AdminListActions.cshtml`).
- **External services**: none. Nothing calls a mail, SMS or payment provider.

### Features to enable before recording (Tools, Features)

Search the Features page and use each row's **Enable** button (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml`):

| Feature | Why |
|---|---|
| Sitemaps | The subject. |
| SEO | Adds the generated `robots.txt` and the Robots settings (`view-3`, `view-4`). |
| Background Tasks | Lists the cleanup task in `cleanup-2`. |

Do **not** enable **Sitemaps Cleanup**: `06-cleanup` enables it on camera. While it is enabled, a background task deletes
the unused cache files every five minutes, which would remove the file `05-cache` purges on camera.

### Settings

- Settings, General: **Base url** = `https://www.example.com`, then **Save**. Without it every sitemap page shows the
  warning "The site settings 'Base url' property is empty, but is required to generate sitemaps."
  (`OrchardCore.Sitemaps/Views/Admin/List.cshtml:9-12`) and the generated `robots.txt` has no sitemap line.
  The addresses in the XML pages then begin with `https://www.example.com`, which is a fake domain. The setting is stored
  with a trailing slash and the sitemap builders append an address that starts with a slash, so check whether the pages
  show a double slash (see `OPEN-QUESTIONS.md`).
  Never click an address inside an XML page: it points at that fake domain.
- Settings, Search, Search Engine Optimization, Robots (`/Admin/Settings/robotsSettings`): leave every value as it is, with
  the checkbox "Include all sitemaps." checked, and click **Save** once. The notice "Site settings updated successfully."
  appears. This seed step is required: the sitemap lines of the generated `robots.txt` are written only when the setting
  `SitemapsRobotsSettings` has been saved at least once, since nothing stores its default
  (`OrchardCore.Sitemaps/Services/SitemapsRobotsProvider.cs:32`, `OrchardCore.Settings/SiteSettings.cs:52-76`).
  Menu: `OrchardCore.Seo/AdminMenu.cs:25-31`; label: `OrchardCore.Sitemaps/Views/SitemapsRobotsSettings.Edit.cshtml:10`.

### Seed data (create outside the capture, in the browser, signed in as admin)

The Blog recipe provides the content: one Blog Post titled "Man must explore, and this is exploration at its greatest"
and one Article titled "About", both published. Both content types have the Autoroute part, which is why the Sitemaps
module lists them. **Every sitemap below must have at least one published item of its content type**, since a sitemap
index asks each contained sitemap for its last modified date.

1. Tools, Search Engine Optimization, Sitemaps, **Add Sitemap**. Name `Posts`, leave **Permalink** empty (the address
   becomes `posts.xml`), leave **Enabled** checked, click **Create**.
2. In the list, click **Manage Sources** on `Posts`, then **Add Source**. In the dialog "Available Sources", click **Add**
   on the card "Sitemap Content Types". On "Create Sitemap Source", leave "Index All Content Types" and "Limit Items"
   unchecked, check **Blog Post** under "Select Content Types", click **Create**. The notice "Sitemap source added
   successfully." appears.
3. Repeat 1 and 2 with the Name `Articles` (the address becomes `articles.xml`) and the content type **Article**.
4. Take **snapshot A**: stop the site, copy its data folder aside (the cache folder is still empty), start the site.
5. **Cache seed**, so the Sitemaps Cache holds a file that no sitemap uses:
   1. Open `http://localhost:5010/posts.xml` and `http://localhost:5010/articles.xml` in the browser (any visitor can).
      Two files appear in the cache folder. Note the file name of `Posts_<identifier>.xml`: it becomes the unused one.
   2. Content, Content Items: open "Man must explore, and this is exploration at its greatest", add the sentence
      "Fake edit." at the end of the body, click **Publish**. Publishing gives the Posts sitemap another identifier
      (`OrchardCore.Contents/Sitemaps/ContentTypesSitemapSourceUpdateHandler.cs:17-72`).
   3. Open `http://localhost:5010/posts.xml` again. The cache folder now holds three files: `Articles_...`, the
      `Posts_...` file of 5.1 (older, unused) and a second `Posts_...` file (current).
6. Take **snapshot B**: stop the site, copy its data folder **and** the cache folder aside, start the site. The
   identifiers in the file names live in the data folder, so the two copies always go back together.

At this point the Sitemap Indexes list is empty, and the Sitemaps Cache lists three files.

## Order of the clips, and what each one changes

Record in this order; each clip starts from the state the previous one left.

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-create` | snapshot B | Creates the index `Main`. |
| 2 | `03-view` | the end of `02-create` | Adds the cache file `Main_<identifier>.xml` (four files now). |
| 3 | | | Take **snapshot C** (data folder and cache folder) after `03-view`. |
| 4 | `05-cache` | snapshot C | Deletes cache files (one, then all). |
| 5 | `06-cleanup` | snapshot C or the end of `05-cache` | Enables Sitemaps Cleanup. |

The slide chapters `01-why` and `04-how` and the two cards need no site. Retakes: for `02-create` restore snapshot B; for
`03-view` restore snapshot B and create the index again off camera with the values of `02-create`; for `05-cache` restore
snapshot C; for `06-cleanup` make sure Sitemaps Cleanup is disabled (restore snapshot C, or disable it in Tools,
Features). To restore a snapshot: stop the site (by its process ID), replace the data folder and the cache folder with
the copies, start the site, wait for the home page. Never restore only one of the two.

## Chapter 01: `01-why` (slides)

Slides are drawn by `slides.py` with the heading "Why a sitemap index" and four cards. No site needed.

- `why-1`: cards 1 and 2 are bright ("A size limit", "Several sitemaps"), cards 3 and 4 dimmed.
- `why-2`: cards 3 and 4 are bright ("A sitemap of sitemaps", "One address"), cards 1 and 2 dimmed.

## Chapter 02: `02-create` (clip)

Starts on `/Admin/Sitemaps/List` (the Sitemaps list). Before `StartCaptureAsync`, expand the admin menu so Tools, Search
Engine Optimization and its three entries (Sitemaps, Sitemap Indexes, Sitemaps Cache) are visible. No notice is on the
screen.

- `create-1`: no action; hold on the list. When the narration ends the list shows two rows, **Posts** and **Articles**,
  each with the badge "Enabled" and the buttons **Manage Sources**, **View**, **Edit**, **Delete** and the **Actions**
  drop-down. There is no yellow Base url warning. Optionally `HighlightAsync` on the two names, then
  `ClearHighlightsAsync`. Labels: `Sitemaps/Views/Admin/List.cshtml:6,30`,
  `Sitemaps/Views/SitemapListEntry.Buttons.SummaryAdmin.cshtml:6-14`, `Sitemaps/AdminMenu.cs:19-31`.
- `create-2`: click the menu entry **Sitemap Indexes** with `ClickAndWaitForNavigationAsync`; wait for the heading
  "Sitemap Indexes" and the text "Nothing here! There are no sitemap indexes for the moment." Then move to the button
  **Add Sitemap Index** (`"a.create"`) and click it with `ClickAndWaitForNavigationAsync`, a second or so before the
  narration ends. When it ends the page "Create Sitemap Index" is open with empty fields. Labels:
  `Sitemaps/Views/SitemapIndex/List.cshtml:6,30,39`, `Sitemaps/Views/SitemapIndex/Create.cshtml:4`.
- `create-3`: `TypeAsync("#Name", "Main")`, then `TypeAsync("#Path", "sitemap.xml")`. When the narration ends the
  fields read Name "Main" and Permalink "sitemap.xml", with `https://www.example.com/` as the prefix before the Permalink
  (the Base url, which the narration names). Source of the prefix: `Sitemaps/Views/SitemapIndex/Create.cshtml:29-32`.
  Labels "Name" and "Permalink": `Sitemaps/Views/SitemapIndex/Create.cshtml:18,26`. The value must end in `.xml`
  (`Sitemaps/Services/SitemapHelperService.cs:47-50`).
- `create-4`: click the checkbox **Enabled** (`"#Enabled"`; the label reads "Enabled" with the hint "Uncheck to disable
  routing to the sitemap index."), then click the labels **Articles** and **Posts** under "Contained Sitemaps"
  (`"label.form-check-label"` with that text), in that order. When the narration ends the three boxes are checked.
  The Enabled box starts unchecked in the code (see `OPEN-QUESTIONS.md`): if it already is checked, do not click it.
  Labels: `Sitemaps/Views/SitemapIndex/Create.cshtml:44,51,62`.
- `create-5`: click **Save** (`"button.save"`) with `WaitForNavigationAsync`. When the narration ends the page "Sitemap
  Indexes" shows the notice "Sitemap index created successfully" and one row, **Main**, with the badge "Enabled" and the
  buttons **View**, **Edit**, **Delete** and the **Actions** drop-down. Wait for the row (`"text=Main"`) before the step
  ends. Labels: `Sitemaps/Controllers/SitemapIndexController.cs:195`, `Sitemaps/Views/SitemapIndexListEntry.Buttons.SummaryAdmin.cshtml:6-13`.

## Chapter 03: `03-view` (clip)

Starts on `/Admin/SitemapIndexes/List` with the row **Main**. Expand the menu as in `02-create`.

- `view-1`: click the button **View** of the row `Main` (`target="_blank"`: it opens another tab). Wrap the click in
  `Page.RunAndWaitForPopupAsync`, then `scene.SwitchToAsync(popup)`. Wait until the popup's URL path is `/sitemap.xml`.
  When the narration ends the tab shows the index as an XML document with two entries, one for
  `https://www.example.com/posts.xml` and one for `https://www.example.com/articles.xml` (see the double slash question
  in `OPEN-QUESTIONS.md`), each with a last modified date. Do not click inside the XML. Source: `Sitemaps/Builders/SitemapIndexTypeBuilder.cs:32-76`.
- `view-2`: `NavigateAsync("/posts.xml")` in the same tab. When the narration ends the page shows the Posts sitemap: one
  entry for the blog post, with its address (`https://www.example.com/blog/...`), a last modified date, "daily" as the
  change frequency and "0.5" as the priority. Source: `Contents/Sitemaps/ContentTypesSitemapSourceBuilder.cs:101-107, 133-203`.
- `view-3`: `SwitchToAsync` back to the main tab (the popup of `view-1` can stay open and unused). In the admin menu
  click **Settings**, then **Search**, then **Search Engine Optimization**, and finally **Robots** (with
  `ClickAndWaitForNavigationAsync` on **Robots**), so the menu opens on camera while the narration names the entries.
  `HighlightAsync` on the checkbox labeled "Include all sitemaps." (`"label:has-text('Include all sitemaps.')"`), then
  `ClearHighlightsAsync`. Do not click **Save** on camera: the setting was saved in the seed, and the menu clicks already
  take most of the narration. When the narration ends the Robots page is open, with the checkbox "Include all sitemaps."
  checked. Source: `OrchardCore.Seo/AdminMenu.cs:25-31`, `Sitemaps/Views/SitemapsRobotsSettings.Edit.cshtml:10-11`.
- `view-4`: `NavigateAsync("/robots.txt")` in the main tab. When the narration ends the plain text page shows the lines of
  the generated robots file and, among them, exactly one line that starts with "Sitemap:" and ends with
  `https://www.example.com/sitemap.xml`; there is no line for `posts.xml` or `articles.xml`, since the enabled index
  contains them. Source: `Sitemaps/Services/SitemapsRobotsProvider.cs:21-89`.

## Chapter 04: `04-how` (slides)

Slides are drawn by `slides.py` with the heading "How the Sitemaps Cache works" and four cards. No site needed.

- `how-1`: cards 1 and 2 are bright ("Built on request", "Stored as a file"), cards 3 and 4 dimmed.
- `how-2`: cards 3 and 4 are bright ("A changed identifier", "Old files stay"), cards 1 and 2 dimmed.

## Chapter 05: `05-cache` (clip)

Starts on `/Admin/SitemapIndexes/List` after restoring snapshot C, with the menu expanded as before. The cache folder holds
four files: `Articles_...`, `Main_...`, and two `Posts_...` (the unused one is the one noted in the cache seed, 5.1, and is
also the one with the earlier last write time in the cache folder). Write the name of the unused file down before the
capture: the recorder needs it for `cache-3`.

- `cache-1`: click the menu entry **Sitemaps Cache** with `ClickAndWaitForNavigationAsync`; wait for four rows. When the
  narration ends the page "Sitemaps Cache" lists four file names ending in `.xml`, each row with a red **Purge** button,
  and the search box and the red **Purge All** button above. Labels: `Sitemaps/AdminMenu.cs:29`,
  `Sitemaps/Views/SitemapCache/List.cshtml:6,20`, `Sitemaps/Views/SitemapCacheEntry.Buttons.SummaryAdmin.cshtml:6`.
- `cache-2`: `HighlightAsync` on the two rows whose name starts with `Posts_`, one after the other, then
  `ClearHighlightsAsync` before the step ends. No navigation.
- `cache-3`: Move to the button **Purge** inside the row of the unused Posts file
  (`".list-group-item:has-text('<unused file name>') a:has-text('Purge')"`) and click it. The dialog titled "Purge" opens with the
  message "Are you sure you want to purge this sitemap?"; wait for it (`"#modalOkButton"`), pause about one second so the
  message can be read, click **Ok** (`"#modalOkButton"`) with `WaitForNavigationAsync`. When the narration ends the
  notice "Sitemap cache item purged." is shown and the list has three rows: `Articles_...`, `Main_...` and one `Posts_...`.
  The dialog texts come from `Sitemaps/Views/SitemapCacheEntry.Buttons.SummaryAdmin.cshtml:6` and the default button
  "Ok" from the admin layout (`OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`). The notice:
  `Sitemaps/Controllers/SitemapCacheController.cs:97`.
- `cache-4`: click the red **Purge All** button (`"a:has-text('Purge All')"`). The dialog titled "Purge All" opens with
  "Are you sure you want to purge the entire sitemap cache?"; pause about one second, click **Ok** with
  `WaitForNavigationAsync`. When the narration ends the notice "Sitemap cache purged." is shown and the list reads
  "Nothing here! There are no sitemaps cached for the moment." Source: `Sitemaps/Views/SitemapCache/List.cshtml:20,33`,
  `Sitemaps/Controllers/SitemapCacheController.cs:76`.

## Chapter 06: `06-cleanup` (clip)

Starts on `/Admin/Features` with Sitemaps Cleanup disabled, and the menu expanded so Tools shows **Features** and
**Background Tasks**. Do not restore the cache; this clip does not use it.

- `cleanup-1`: `TypeAsync("#search-box", "sitemaps")`; wait until the list shows the row **Sitemaps
  Cleanup** (and, with it, the other features that match "sitemaps"). The row **Sitemaps Cleanup** shows the description "Cleanup sitemap cache files through a background task." and the badge "Disabled".
  Move to its **Enable** button (`".list-group-item:has-text('Sitemaps Cleanup') a:has-text('Enable')"`; the button id
  is `btn-enable-` followed by the feature id with dots turned into underscores) and click it a second or so before the
  narration ends, with `WaitForNavigationAsync`. When the narration ends the page "Features" is shown again with the
  notice "The feature Sitemaps Cleanup was enabled." Source: `Sitemaps/Manifest.cs:32-37`,
  `OrchardCore.Features/Views/Admin/Features.cshtml:27`, `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml`,
  `OrchardCore.Features/Controllers/AdminController.cs:317`.
- `cleanup-2`: click the menu entry **Background Tasks** (Tools) with `ClickAndWaitForNavigationAsync`. The task is only
  known to the background service after its next polling (up to about a minute), and the row first shows the badge
  "Waiting for the background service to load this task". Cut that wait out of the clip with `OffCameraAsync`: wait until
  the row of **Sitemap Cache Cleaner** contains an element `[data-next-run]`. The page refreshes the states by itself
  (`docs/reference/modules/BackgroundTasks/README.md`), but if the badge has not appeared after about 15 seconds, reload with
  `NavigateAsync("/Admin/BackgroundTasks")` and check again, every 15 seconds, for at most three minutes.
  When the narration ends the page "Background Tasks" shows the row **Sitemap Cache Cleaner**, with the description
  "Cleans up sitemap cache files.", the badges "Enabled" and "Next run" (a time left of a few minutes), and "Last run"
  "never" or a recent run. Do not open the Edit page or the Details page: they show the schedule. Source:
  `Sitemaps/Cache/SitemapCacheBackgroundTask.cs:7-10`, `OrchardCore.BackgroundTasks/AdminMenu.cs:19`,
  `OrchardCore.BackgroundTasks/Views/BackgroundTask/Index.cshtml:7`,
  `OrchardCore.BackgroundTasks/Views/BackgroundTaskEntry.Description.SummaryAdmin.cshtml`,
  `OrchardCore.BackgroundTasks/Views/BackgroundTaskEntry.NextRun.SummaryAdmin.cshtml:26`.

## Cards

The opening card (`intro`) and the closing card (`outro`) are drawn by the assembler from the storyboard; nothing to record.

## Checks for private data

- The only names on screen are the fake ones above: the user `admin`, the sitemaps `Posts`, `Articles` and `Main`, the
  site name "Orchard Core", the fake domain `www.example.com`, and the cache file names (a sitemap name and a generated
  identifier).
- No password, token or `state.json` is visible. The sign-in was saved outside the capture.
- The browser's address bar is not recorded, so `localhost:5010` does not appear, except possibly inside the popup tab
  title. Look at the contact sheet of `03-view` for any local path or machine name.
- After assembling, run the private-data scan of the skill (OCR at one frame per second and at every scene change). The
  only addresses allowed in the text are `example.com` ones.

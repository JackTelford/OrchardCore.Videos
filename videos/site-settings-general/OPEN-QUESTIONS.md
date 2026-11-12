# Open questions: site-settings-general

What was checked in the Orchard Core source, and what a person must confirm on a running site before or while
recording. Paths are under `src/` of `OrchardCMS/OrchardCore`.

## To confirm on the running site

1. **Tab names.** The three tabs are named by the placement strings `Content:1#Site;10`, `Content:1#Resources;20` and
   `Content:1#Cache;30` (`OrchardCore.Settings/Drivers/DefaultSiteSettingsDisplayDriver.cs:51-57`). `AdminTabs`
   writes the group key as the link text (`OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:59`). Confirm the
   links read `Site`, `Resources` and `Cache`, in that order, and that no other enabled feature adds a tab (no other
   file registers the group `general`).
2. **Starting values.** The narration relies on the values of a site created by Auto Setup with the Blog recipe: Page
   size 10 and a maximum page size of 100 (`OrchardCore.Settings/Services/SiteService.cs:59-60`), cache busting on
   (`SiteSettings.cs:31`), Resource Debug Mode and Cache Mode on `From environment` (the enum default), Base url and
   Site CDN base url empty. The Blog recipe's `settings` step sets only the home route and the layer zones
   (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). Confirm the table in `recording-plan.md`.
3. **Fifteen items.** The Blog recipe creates 7 content items (a menu, two taxonomies, the Blog item, a blog post,
   the About article and a footer widget). Only the types marked listable appear in Content Items: Menu
   (`OrchardCore.Menu/Migrations/menu.recipe.json:11`), Taxonomy (`OrchardCore.Taxonomies/Migrations.cs:27`), Blog and
   Article (`blog.recipe.json`). That leaves five items, plus the 10 seeded articles: 15, which is two pages at a page
   size of 10 and three at 5. Confirm the real count on the list. The narration states no total, so steps `size-1` and
   `size-3` hold for any total above 10.
4. **Top bar name.** After saving, the top bar of the admin shows the site name
   (`OrchardCore.Admin/Views/AdminBranding.cshtml:8`). Confirm the text is visible at the recording size and not
   hidden when the menu is compact.
5. **Time zone entries.** Each entry reads `(GMT{standard offset}) {id} ({wall offset})`
   (`OrchardCore/Modules/Model/TimeZone.cs:25`), where the offsets come from the zone interval of the instant at which
   the list is built (`OrchardCore/Modules/Services/Clock.cs:30`); the list is built once and kept by a singleton
   (`DefaultTimeZoneSelectListProvider.cs`, registered in `OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:158`).
   `zone-2` therefore says "a time zone's name, its standard offset from GMT, and a second offset that includes daylight
   saving time while it is in effect", which holds on any day. Confirm on screen that the entries for
   `America/Los_Angeles` and `Europe/London` show a standard offset in front of the name and a second offset after it
   (the two are equal outside daylight saving time, so the viewer may see two identical values), and that selecting by
   the value `Europe/London` works.
6. **Items per page.** With Page size 5 and the options `5, 10, 25`, the selector reads 5
   (`OrchardCore.Navigation/PageSizeSelector.cs`, `Selected = size == currentPageSize`). Choosing 10 loads the list with
   `pageSize=10`, 10 rows and 2 pages. Confirm where the selector sits (beside the pager, below the list) and whether
   a scroll is needed to see it.
7. **Message timing.** The success message lasts 5 seconds
   (`OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`). Confirm that, after the cut of `OffCameraAsync`,
   it shows during the sentence "A message reads, Site settings updated successfully" of `site-4`.
8. **Pronunciation, not confirmed with `pronounce.py`.** The storyboard's `spoken` map says `Base url` as "Base U.R.L.",
   `base url` as "base U.R.L." and `Europe/London` as "Europe London" (no slash is read aloud). "CDN", "GMT" and
   "jQuery" are left to the voice. Check the four spellings and the three left to the voice with a sample before
   narrating; the label `Site CDN (Content Delivery Network) base url` in `res-3` holds both a parenthesis and the
   `base url` spelling.
9. **Heading of the list in chapter 05.** The Content Items menu entry (`OrchardCore.Contents/AdminMenu.cs:68`) opens
   a page whose heading is the breadcrumb title `Manage Content` (`OrchardCore.Contents/Views/Admin/List.cshtml:17`), so
   `size-1` names the page "Manage Content" and the menu entry "Content Items". Confirm the `h1` reads `Manage Content`
   and is on screen during the first sentence of `size-1`.
10. **Save bar.** The Save button sits in `div.action-bar-bottom`, which the style sheet fixes to the bottom of the
    window (`TheAdmin/Assets/scss/components/_action-bar.scss:14-16`). Confirm it is visible without scrolling at the
    recording size, and that it does not cover the Items per page selector or the pager of chapter 05.

## Behaviors read in the source, beyond what the narration states

- **Cache busting.** `res-1` says the version is appended "so browsers load changed files". The setting only decides
  whether `AddFileVersionToPath` runs on a local address (`OrchardCore.ResourceManagement.Abstractions/ResourceDefinition.cs:435-439`);
  that the version changes with the file comes from the ASP.NET Core file version provider, outside the Orchard Core
  source. Confirm on the running site: the local style sheet and script addresses of the front end carry a `v=` value,
  and the value changes when the file changes.
- **The theme of the front end.** `site-2` and `site-5` name the Blog theme as the theme of the site: the Blog recipe
  sets `TheBlogTheme` as the site theme (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, step `themes`).
  Confirm that the front end of the recording shows the Blog theme.

- **Clearing Base url or Site CDN base url does not clear the saved value.** On save, the base url is stored only
  when it is a valid address; an empty value is not an error and is not stored
  (`DefaultSiteSettingsDisplayDriver.cs:92-110`). The narration only says to leave both empty, and the plan never types
  in them. Worth confirming (type an address, save, empty the field, save, reopen), and reporting if it is not
  intended.
- **Page title format.** The narration says the Blog theme builds the title of the browser tab from it
  (`TheBlogTheme/Views/Layout.liquid:6` renders the `PageTitle` shape, `OrchardCore.DisplayManagement/Shapes/PageTitleShapes.cs:20-37`),
  and that a site starts with a format that adds the site name after the page title. The default format is
  `{% page_title Site.SiteName, position: "after", separator: " - " %}` (`SiteService.cs:57`): the tag adds the site name
  as a segment at the position `after`, which sorts after the segments of the page
  (`OrchardCore.DisplayManagement.Liquid/Tags/PageTitleSegmentsTags.cs`, `OrchardCore.DisplayManagement/Title/PageTitleBuilder.cs:57`,
  `OrchardCore.DisplayManagement/Zones/FlatPositionComparer.cs:6-7`). The effect is not on camera: the capture
  is the page only, with no browser tab. Confirm outside the capture with `Recorder probe` and
  `() => document.title` on the front end before and after changing the Site name. The admin and `TheTheme` layouts
  do not use the format (`TheAdmin/Views/Layout.cshtml:21`, `TheTheme/Views/Layout.cshtml:20`).
- **Page size refusals.** A page size below 1, or above the maximum page size, is refused with "The page size must be
  greater than zero." and "The page size must be less than or equal to {0}."
  (`DefaultSiteSettingsDisplayDriver.cs:113,118`). Not shown on camera. Confirm by saving 0 and 101.
- **Time zone priority.** A time zone set on a user comes from the `User Time Zone` feature
  (`OrchardCore.Users/Manifest.cs:70-71`). Its selector has priority 100, the site's has priority 0, and the first
  non-empty zone in priority order is used (`OrchardCore.Users/TimeZone/Services/UserTimeZoneSelector.cs:28`,
  `OrchardCore/Modules/Services/LocalClock.cs:73-101`). An empty site value (`Local to server`) falls back to the
  time zone of the machine (`LocalClock.cs:83,101`). Confirm by enabling the feature on a spare site, giving a user a
  different zone, and reading a date in the admin. Not on camera.
- **Permissions.** `Manage general settings` is implied by `Manage settings`
  (`OrchardCore.Settings.Core/SettingsPermissions.cs:13`), and the Administrator role gets all three settings
  permissions (`OrchardCore.Settings/Permissions.cs:17-25`). Confirm the labels in the role editor.
- **Where the Base url is used.** The narration limits itself to background tasks: before a task runs, the scheme, host
  and path base of its request are replaced by the Base url when it is set
  (`OrchardCore/Modules/ModularBackgroundService.cs:189,490-511`). The value is also read by the Sitemaps module, which
  uses it as the host prefix of the links it builds and in `robots.txt` (`OrchardCore.Sitemaps/Controllers/SitemapController.cs:77`,
  `OrchardCore.Sitemaps/Services/SitemapsRobotsProvider.cs:25-37`), and by the Data Pipelines module
  (`OrchardCore.DataPipelines/Services/DataPipelineRunDispatcher.cs:121`). The video does not say more.
- **Which lists use the site Page size.** The narration says "lists in the admin" and "admin list" because the admin
  lists build their pager from the site's pager options (for example
  `OrchardCore.Contents/Controllers/AdminController.cs:232`), whereas the pager of a List part on the front end starts
  from the page size of the part's own settings (`OrchardCore.Lists/Models/ListPartSettings.cs:5`,
  `OrchardCore.Lists/Drivers/ListPartDisplayDriver.cs:255,266`). The Page size field hint reads "The default page size."
- **Cache Mode.** The setting is read by the Dynamic Cache module only
  (`OrchardCore.DynamicCache/CacheOptionsConfiguration.cs:23-41`), which the Blog recipe enables; the tab heading reads
  "Settings for dynamic caches" (`Settings-Cache.Edit.cshtml:5`), so the narration says "the dynamic caches".
- **Site CDN base url in debug mode.** The prefix is not added when the resource debug mode is on, and the mode "From
  environment" is on outside Production (`ResourceDefinition.cs:441-450`, `ResourceManagementOptionsConfiguration.cs:1253-1269`).
  A site run in Development therefore does not show the prefix, which is why `res-2` states the exception and the field
  stays empty.

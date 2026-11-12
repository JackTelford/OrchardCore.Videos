# Open questions: Sitemaps: Help Search Engines Find Your Pages

Things the author could not confirm from the Orchard Core source (commit `92db4a83`) and the documentation alone. Check each
one on the running site, before recording the clip it names. None of them is stated as a fact in the narration, except where
noted. If an answer differs from what is written here, stop and send the question back to the author instead of recording.

## 1. The addresses in the sitemap are expected to be written with a double slash (clips `05-view`, `06-types`, `07-path`, `08-part`)

What the code does (commit `92db4a83`), read twice and not run:

- `OrchardCore.Sitemaps/Controllers/SitemapController.cs:77` sets the prefix of every address to `siteSettings.BaseUrl`.
- `OrchardCore.Settings/SiteSettings.cs:34-37` returns `AbsoluteUri` of the saved address, so `https://www.example.com` is read
  back as `https://www.example.com/`, with a trailing slash. The Base url field shows the same value after saving (`setup-4`).
- `OrchardCore.Autoroute/Sitemaps/AutorouteContentTypeProvider.cs:36` and
  `OrchardCore.Sitemaps/Builders/CustomPathSitemapSourceBuilder.cs:45` append a path that starts with `/`
  (`UrlHelper.Action` and `UrlHelper.Content("~/...")`), without removing the trailing slash of the prefix.
- The robots provider (`OrchardCore.Sitemaps/Services/SitemapsRobotsProvider.cs:37`) does trim the trailing slash of the Base url;
  the builders of the sitemap do not.

So every `loc` of the sitemap is expected to read `https://www.example.com//about`, with two slashes, for any Base url that has
no path. No Base url avoids it, so the plan does not invent one.

What a person must do: open `http://localhost:5010/pages.xml` after clip `04-source` and read the address of each entry. If an
address reads `https://www.example.com//about`, the stop condition of this plan holds: do not record `05-view` and the clips after
it until the maintainer decides whether to record a screen that shows this, or to wait for the module to be fixed. The narration
does not describe the shape of an address, only that it starts with the Base url (`setup-3`), which stays true.

## 2. Does every entry have a last modified date? (clip `05-view`)

`ContentTypesSitemapSourceBuilder.cs:205-212` writes the date only when the content item has a modified date, so `view-2` says
"when it has one". Open `/pages.xml`: if the items of the Blog recipe have no date, the narration is right as written. If every
entry has one, it is still right. Record either way, but do not scroll so that the viewer is led to count entries without dates.

## 3. The label of the tab that holds the Sitemap part editor (clip `08-part`, step `part-3`)

`OrchardCore.Sitemaps/Drivers/SitemapPartDisplayDriver.cs:14` places the editor at `Parts#SEO:5`. The parser
(`OrchardCore/OrchardCore.DisplayManagement/Descriptors/PlacementInfo.cs:386-440`) reads the tab name between `#` and the next
delimiter, so the tab should read **SEO**, the same tab the SEO module's editors use. Confirm on the About editor that the tab is
labeled SEO and that it is not hidden when the Article type has no other tabs. If the label differs, `part-3` must be changed.

## 4. The Features page: which rows the search keeps, and the Enable button (clip `02-setup`, step `setup-1`)

The view builds the button id with `Html.GenerateIdFromName(featureId)`
(`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`). The plan writes `a#btn-enable-OrchardCore_Sitemaps`
because the dot of the id is replaced by an underscore by default, but this was not run. Confirm the id, or select the
button inside the row whose `<code>` is exactly `OrchardCore.Sitemaps`. Also confirm that the Sitemaps row is not hidden by the
"Hide on demand" or "Hide always enabled" filters (both are on by default; Sitemaps should be neither).

## 5. Do the Sitemaps list rows show buttons? (clips `03-create` to `06-types`)

`AdminListOptions` defaults to the List layout with the Buttons actions, and no recipe step of the Blog recipe changes it, so
the row of a sitemap should show the buttons **Manage Sources**, **View**, **Edit** and **Delete** and an **Actions** menu
holding **Disable**. If the site shows an ellipsis menu instead, reset Settings > Admin before recording; `create-4` and `source-1`
name the buttons.

## 6. Opening the sitemap from the View button (clips `05-view`, `06-types`, `07-path`)

The **View** button has `target="_blank"` (`OrchardCore.Sitemaps/Views/Admin/Display.cshtml:23`), which the plan handles with
`RunAndWaitForPopupAsync` and `SwitchToAsync`. Confirm that the recorder captures Chromium's XML tree for the page (the
response is `application/xml`), that the drawn cursor is allowed to be missing on that page, and that the address bar is not
part of the capture. If the popup cannot be captured, use `NavigateAsync("/pages.xml")` as `part-5` does; `view-1` says "another
tab", so the narration of `view-1` would then need the words "in the same tab" in place of "in another tab".

## 7. Does `/tags` return the Tags page? (clip `07-path`)

The Blog recipe creates a taxonomy named **Tags** with the permalink `tags` and publishes it, but the page was not requested.
Open `http://localhost:5010/tags` before recording; it must show a page and not an error. The custom path is only an address
in a sitemap, so a page that returned an error would still be listed, but the video should not point to one.

## 8. Which content types does the Select Content Types list show? (clip `06-types`)

`recording-plan.md` lists Article, Blog, Blog Post, Category, Page, Tag and Taxonomy, which are the types of the Blog recipe
and of the Taxonomies module that have the Autoroute part. Another enabled feature can add a type with that part. The
narration only asks to check Article and Blog Post, so a longer list is not a problem, but the plan's list should be corrected.

## 9. Does the Article editor require more than a title and a body? (the seed data)

The Blog recipe's Article has the fields Subtitle and Image. The plan assumes both are optional. If the form refuses to
publish without a Subtitle, type `A short note.`

## 10. Pronunciation (all clips)

Three spoken spellings were added in `storyboard.json` and not heard yet: ` .xml.` and ` .xml,` as "dot X.M.L." (steps
`what-2` and `create-2`), and `Base url` as "Base U.R.L." (steps `setup-3`, `setup-4`, `create-2`, `reach-1`). Check each with
`scripts/pronounce.py` before narrating, as `SKILL.md` asks, and change the spoken map if the voice reads them badly.
Also listen to two names that have no spoken entry: `SEO` (steps `part-3` and `reach-1`; add `"SEO": "S.E.O."` if the voice runs the letters together) and `Autoroute` (steps `source-3` and `types-2`).

## 11. Does the robots file list the sitemaps before the Robots settings were saved? (slide `reach-1` and `reach-2`)

`OrchardCore.Sitemaps/Services/SitemapsRobotsProvider.cs:32` returns nothing unless `site.TryGet<SitemapsRobotsSettings>` succeeds,
and `TryGet` is false while the site holds no stored `SitemapsRobotsSettings` (`OrchardCore/OrchardCore.Infrastructure.Abstractions/Entities/EntityExtensions.cs:98-112`).
Nothing in the source writes that setting but the Robots settings page (`OrchardCore.Sitemaps/Drivers/SitemapsRobotsSettingsDisplayDriver.cs`):
no migration, recipe or default creates it, and the `[DefaultValue(true)]` of `IncludeSitemaps` does not create it. The base driver
only handles the settings group `robotsSettings` (`OrchardCore/OrchardCore.DisplayManagement/Entities/SiteDisplayDriver.cs:32-39`) and stores the
section only when an update of that group is valid (`SectionDisplayDriverBase.cs:52-69`), so saving another settings page, such as General, does not create it. The page
shows the box checked before the first save. The documentation says the sitemaps are added by default; the code says they are
added after the page is saved with the box checked. `reach-2` therefore says "checked and saved".

What to confirm, only if the video is recorded with the SEO feature on a site that may be shown to a viewer: enable SEO, set the Base
url, create an enabled sitemap, open `http://localhost:5010/robots.txt` and note whether a `Sitemap:` line is there; then save
Settings, Search, Search Engine Optimization, Robots with **Include all sitemaps.** checked and open it again. If the first
file already holds the line, change `reach-2` ("once ... is checked and saved") and the fourth card of the slide. The
two slides need no recording, so this changes nothing in the clips.


# Open questions: Sitemap Indexes and the Sitemaps Cache

What the author checked in the Orchard Core source (commit 92db4a83) and could not check on a running site. A person
confirms each item while recording; none of them is stated as a fact in the narration.

## Behavior to confirm on the running site

1. **The "Enabled" box of "Create Sitemap Index".** The view model has no initializer for it
   (`OrchardCore.Sitemaps/ViewModels/CreateSitemapIndexViewModel.cs:13`), unlike the sitemap form, which starts checked
   (`CreateSitemapViewModel.cs:12`). So the box should start unchecked, and the **View** button only appears on an enabled
   index. The narration says "Check Enabled, so the index is served" and does not say that it starts unchecked. If the
   box starts checked, step `create-4` still reads correctly (do not click it).
2. **Does the index file refresh after publishing?** The cache file of a sitemap index is named from the index's own
   identifier, and publishing content only changes the identifier of the sitemaps whose source lists that content type
   (`OrchardCore.Sitemaps/Handlers/SitemapIndexTypeUpdateHandler.cs`, `OrchardCore.Contents/Sitemaps/ContentTypesSitemapSourceUpdateHandler.cs`).
   Enabling or disabling a contained sitemap does not change the index's identifier either. A cached index file may
   therefore keep the contained sitemaps and last modified dates it had when it was built, until the index is saved or
   its cache file is purged. The video says none of this; confirm it by publishing a post, disabling `Articles`, and
   opening `/sitemap.xml` again before and after purging `Main_...`. The documentation says the cache "is automatically
   cleared when content items are published" (see `docIssues`).
3. **An index fails when a contained sitemap has no published item.** `ContentTypesSitemapSourceModifiedDateProvider`
   reads the date of the latest published item of the sitemap's content types without checking that one exists
   (`OrchardCore.Contents/Sitemaps/ContentTypesSitemapSourceModifiedDateProvider.cs`, line 62),
   which should fail the request of the index. The seed avoids it (Blog Post and Article both have a published item).
   Confirm the symptom before telling anyone; do not record it.
4. **The XML pages in the capture.** `/sitemap.xml` and `/posts.xml` are served as `application/xml` without a style
   sheet, so Chromium draws its default tree view of the document. Confirm that it is readable at 1536x864, that the
   scene's drawn cursor and highlights work on such a page (if not, record `view-1` and `view-2` without them) and that
   the page is shown rather than downloaded. `/robots.txt` is plain text.
5. **The robots lines.** The provider writes the `Sitemap:` lines only when the site settings hold a saved
   `SitemapsRobotsSettings` (`OrchardCore.Sitemaps/Services/SitemapsRobotsProvider.cs:32`; `SiteSettings.TryGet` is false
   for a setting that was never saved, and neither a migration nor a recipe stores it). The plan therefore saves the
   Robots settings page once in the seed. Confirm on a fresh site that `/robots.txt` has no `Sitemap:` line before that
   save and one line, for the enabled index only, after it, next to the default lines of the Robots settings. Also
   confirm that no `robots.txt` file exists in `wwwroot`. The narration of `view-3` and `view-4` says what the code does
   after the save, and does not say that it works without it.
6. **The unused Posts file.** After the cache seed the cache folder must hold two `Posts_...` files. Confirm that the
   Sitemaps Cache list shows both, and that the older one (by last write time) is the one that is not referenced by the
   current identifier of `Posts`. The list order is the order of the file system and may differ between takes: find the
   row by its file name.
7. **Loading of the cleanup task.** After enabling Sitemaps Cleanup, the Background Tasks page should list "Sitemap Cache
   Cleaner" at once and show the waiting badge until the background service loads the task (one polling time, one
   minute by default, `docs/reference/modules/BackgroundTasks/README.md`). Confirm that the row gets its "Next run" badge
   by itself, so `OffCameraAsync` can wait for `[data-next-run]`.
8. **The admin list layout.** The plan assumes the defaults (`List` layout, `Buttons` actions) and the buttons **View**,
   **Edit**, **Delete** in the row. If the site shows the `Menu` layout, the buttons are inside an ellipsis menu, and
   the narration of `create-5` ("with the buttons View, Edit and Delete") no longer matches the screen.
9. **Blog Post address.** The address of the blog post in `/posts.xml` comes from its Autoroute path; confirm that it is
   a fake-looking `https://www.example.com/blog/...` address.

10. **A double slash in the addresses.** `Site.BaseUrl` returns the absolute URI of the setting, which ends with a slash
    for `https://www.example.com` (`OrchardCore.Settings/SiteSettings.cs:33-37`), and the sitemap builders append an
    address that begins with a slash (`Sitemaps/Builders/SitemapIndexTypeBuilder.cs:61`,
    `OrchardCore.Autoroute/Sitemaps/AutorouteContentTypeProvider.cs:36`). The addresses in the XML pages may therefore read
    `https://www.example.com//posts.xml`, while the robots line trims the slash
    (`Sitemaps/Services/SitemapsRobotsProvider.cs:37`). The narration never reads an address. If the double slash shows,
    decide whether `view-1` and `view-2` can be recorded as they are (the viewer sees an address that search engines would
    reject), and report it to the maintainers as a defect, not as a documentation issue.

## Pronunciation to confirm (`scripts/pronounce.py`)

The storyboard's `spoken` map has two entries a person must hear before narrating:

- `.xml` is spoken " dot X.M.L." (steps `create-3`, which also holds `sitemap.xml`);
- `robots.txt` is spoken "robots dot T.X.T." (step `view-3`).

Also listen to "Skip and Take", "Sitemaps Cleanup" and "Base url" ("base U.R.L." may sound better).

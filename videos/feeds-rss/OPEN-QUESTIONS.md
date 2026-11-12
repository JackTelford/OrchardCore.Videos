# Open questions: Feeds

The narration states only what the source shows (Orchard Core commit 92db4a83; paths are relative to
`src/OrchardCore.Modules` unless they start with `src/`). Nothing was run on a site. A person must confirm each point
below on a running site while recording, and correct the narration or the plan if the site differs.

1. **The address of the feed.** The hint under **Feed proxy Url** builds its link with
   `Url.Action("Index", "Feed", new { area = "OrchardCore.Feeds", ContentItemId = ... })`
   (`OrchardCore.Lists/Views/ListPartFeed.Edit.cshtml:18`). Two routes can produce it: `ListFeed`, with the pattern
   `Contents/Lists/{contentItemId}/rss` and `format = "rss"` as a route default (`OrchardCore.Lists/Startup.cs:102-106`),
   and the generic area route `/{area}/{controller}/{action}/{id?}` that the host maps for every area action
   (`src/OrchardCore/OrchardCore.Mvc.Core/Routing/DefaultAreaControllerRouteMapper.cs:9,17-21`). The generic route carries
   no format value unless the query adds one, and without a format no builder matches, so the controller answers Not
   Found (`OrchardCore.Feeds/Controllers/FeedController.cs:41`, `src/OrchardCore/OrchardCore.Feeds.Core/Rss/RssFeedBuilderProvider.cs:9`).
   The `ListFeed` route is added in the `Configure` of the Lists module (order 0), and the generic routes in the
   `Configure` of the MVC startup, which runs last (`ConfigureOrder` 1000: `src/OrchardCore/OrchardCore.Mvc.Core/Startup.cs:27,38-78`,
   `src/OrchardCore/OrchardCore.Abstractions/Modules/StartupBase.cs:13`). ASP.NET Core takes the endpoint with the lower
   order when it builds a link, so the link is expected to be `http://localhost:5010/Contents/Lists/<blogContentItemId>/rss`.
   That is the framework's rule applied to this code, not something the source states. Confirm that the link has this
   shape and returns the feed. If it does not, steps `find-3`, `find-4` and `find-5` cannot be recorded as planned and
   the narration must change.
2. **How the feed looks in the recorder's browser.** It answers with the type `text/xml` (`FeedController.cs:81`). A
   headless Chromium is expected to draw the tree view of an XML document with no style, but it may show raw text, or
   offer a download. Narration `find-4` and `find-5` assume a readable page with a title, a link, a body and a date for
   each entry. The description of every entry is written in a CDATA section (`OrchardCore.Contents/Feeds/CommonFeedItemBuilder.cs:48`);
   the body of a Blog Post is Markdown turned into HTML (`OrchardCore.Markdown/Handlers/MarkdownBodyPartHandler.cs:36-45`),
   so a tag such as `<p>` is expected around the sentence, while the description of the Blog is the plain text typed in
   its body editor. The date is the date of the last publication, in the form "Thu, 12 Nov 2026 10:00:00 GMT"
   (`CommonFeedItemBuilder.cs:50-59`). The recorder draws its cursor and its rings with elements appended to the root
   element of the page (`.agents/skills/orchardcore-video-producer/scripts/recorder/Scene.cs:24-61`), which may not show
   on an XML document: the plan puts no ring on the feed tab, so only the cursor could be missing there. Confirm on the
   contact sheet.
3. **What `control-4` shows after the feed is turned off.** The code answers Not Found (`OrchardCore.Lists/Feeds/ListFeedQuery.cs:38-40`,
   `FeedController.cs:59`). The Diagnostics feature, which the Blog recipe enables, answers an error status with the page
   `/Error/404` (`OrchardCore.Diagnostics/DiagnosticsStartupFilter.cs:29`), except for an address with a known file
   extension (`DiagnosticsStartupFilter.cs:37`, which does not apply to `.../rss`). The expected screen is therefore a
   page of the Blog theme whose heading reads "The page could not be found." (`OrchardCore.Diagnostics/Views/HttpError-NotFound.cshtml:1`).
   The narration only says "no longer returns a feed". Note what the screen shows, and check that it holds no private data.
4. **Where the three feed settings sit in the editor.** The feed editor shape has no placement of its own
   (`OrchardCore.Lists/Feeds/ListPartFeedDisplayDriver.cs:19-28`), so it takes the default zone and position of the part,
   `Parts.ListPart` at position 4 (`src/OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentItemDisplayCoordinator.cs:255-256`).
   The parts of the Blog are ordered Title (0), Autoroute (1), Html body (2), Blog with the Banner Image field (3) and
   Lists (4), so the settings are expected at the end of the editor, below the Banner Image field. The card with
   **List Blog Post Items** and **Create Blog Post** has its own position at the top (`ListPartDisplayDriver.cs:83`). Because
   the list part is not a named part of the Blog, no heading precedes the settings (`OrchardCore.Contents/Views/ContentPart.Edit.cshtml:26-50`).
   Confirm that all three settings are visible together after one scroll, above the Publish bar.
5. **The link in the page, claimed in `idea-4`, `control-3` and `control-4`, is never shown on screen.** The code registers
   a link of the type for RSS in the head of the page of the list (`ListPartFeed.cshtml:13-23`; the Blog theme renders
   head links in `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:20`). Outside the capture, open the home page
   `/` (the Blog is the home page of this recipe) or `/blog`, and view its source: expect one alternate link titled
   **Blog** whose address is the feed address, then the proxy address after `control-3` and `control-4` are published,
   and no link once the feed is turned off. If a cache keeps the old page, clear it.
6. **The address in the header of the feed.** The Blog is the home page of the recipe (`blog.recipe.json:113-119`), and the
   home route takes priority when an address is built for it (`OrchardCore.HomeRoute/Startup.cs:12-18`,
   `OrchardCore.HomeRoute/Routing/HomeRouteValuesAddressScheme.cs:30-48`), so the `link` of the channel is expected to be
   `http://localhost:5010/`, not `http://localhost:5010/blog`. The narration says only "the address". Confirm what the
   feed shows.
7. **The creation order.** The feed sorts by creation date, not by publication date (`ListFeedQuery.cs:105`). The recipe
   gives its post no creation date, and publishing the edit of the seed gives it one (`UpdateContentsHandler.cs:69-78`);
   an unedited post would have no date and sort last on SQLite. After the seed, confirm the order Holiday Opening Times,
   Sourdough Class Dates, Spring Menu Preview, Weekend Market Hours, Opening Week Notes, and that the draft Autumn Menu
   Ideas is absent. If the order differs, change the seed.
8. **Publish stays on the editor.** Opened without a return address, **Publish** is expected to redirect to the same editor
   with "Your Blog has been published." (`EditInternalAsync` in `OrchardCore.Contents/Controllers/AdminController.cs:841-847`).
   Confirm that the editor then shows the saved values.
9. **How long the message "The feature Feeds was enabled." stays on screen** after `enable-2`. If it fades before the
   narration ends, hold the step longer or record the take so that the message is visible at the end.
10. **Switching between the editor tab and the feed tab** in `04-control` with `SwitchToAsync`, and the popup that the hint link opens
    (`target="_blank"`). If the recorder cannot go back to a tab, use the fallback in the plan.
11. **The body editors of the seed.** The Blog uses a code editor (`OrchardCore.Html/Views/HtmlBodyPart.Edit.cshtml:14`) and
    the Blog Post a Markdown editor with a visual editor option (`OrchardCore.Markdown/Views/MarkdownBodyPart-Wysiwyg.Edit.cshtml`).
    Their labels come from the display name of the part, which was not checked. Any way that leaves the plain sentences
    of the seed is fine (click inside the editor, select all, type).
12. **Pronunciation.** The storyboard speaks "Url" as `U.R.L.` (a new entry in this video's `spoken` map) and leaves "RSS"
    as written. Listen to both with `scripts/pronounce.py` before narrating, and correct the map if the voice mispronounces them.

# Recording plan: Feeds, publish an RSS feed of your content

An admin video: browser clips of the admin and of the feed, and slides. Every label below was read in the Orchard Core
source at commit 92db4a83 (paths are relative to `src/OrchardCore.Modules` unless they start with `src/`). In this
file a selector stands for `scene.Page.Locator(...)`. Nothing here has been run on a site: what a person must confirm
is in `OPEN-QUESTIONS.md`.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout whose labels match those cited
  below, on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories
  (`ORCHARD_APP_DATA`). `HTTP_PROXY` and `HTTPS_PROXY` point to a dead address, so the site calls no outside service.
- **Auto Setup** (no Setup screen is recorded, so there is no `state.json` problem with it): default tenant, recipe
  **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`,
  password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`; never written in the repository), the default
  SQLite database. The variables are described in the `orchardcore-tester` skill of the Orchard Core repository
  (`references/autosetup.md`).
- **Sign in once** outside the capture with `Recorder setup` (`VIDEO_USER_NAME=admin`), which saves `state.json`
  (git-ignored).
- **What the Blog recipe gives**: it enables both Feeds and Lists
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step), a Blog list at `/blog`
  titled **Blog** (its type has a list part that accepts Blog Post items), and one Blog Post. The recipe also makes this
  Blog the home page of the site (its `settings` step, `HomeRoute`, `blog.recipe.json:113-119`), so `/` and `/blog` both
  show it; that decides which address the feed shows for the Blog (see `find-4`). The video enables Feeds
  on camera, so **Feeds must be disabled when the first clip starts** (see "Seed data", last step). Lists stays
  enabled. No other feature has to be turned on or off, and no setting has to be saved.
- **External services**: none. There is no mail, SMS or payment provider, and nothing to emulate.

## Seed data (create it outside the capture, in this order)

All names are invented. The feed lists the items of a list **newest first by creation date**
(`OrchardCore.Lists/Feeds/ListFeedQuery.cs:105`), so the creation order below is what the video shows.

1. **The Blog.** Content, Content Items, click the title link **Blog**. The body editor of the Blog is a code editor
   (`OrchardCore.Html/Views/HtmlBodyPart.Edit.cshtml:14`): click inside it, select all with Ctrl+A, and type
   `News and notes from Example Bakery.` (it becomes the description of the feed). Click **Publish**. The title stays
   **Blog**. The editor was opened from the Content Items list, so **Publish** returns to that list: open the Blog again
   for the next step.
2. **The Blog Post that the recipe creates.** On the Blog's editor click **List Blog Post Items**
   (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:32`; the card with this button sits at the top of the
   editor), open the post *Man must explore, and this is exploration at its greatest*, and replace its **Title** with
   `Opening Week Notes` and its body text with `We opened our doors last week and sold out of rye twice.` (the body of a
   Blog Post is a Markdown editor: click inside it, Ctrl+A, type). Click **Publish**. The recipe gives this post no
   creation date and no publication date (`ContentStep.cs:30` in `OrchardCore.Contents/Recipes` imports it as it is), and
   publishing the edit gives it both (`src/OrchardCore/OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:69-78,81-94`).
   Do this step **before** step 3, so that it is the oldest item and its entry in the feed has a date.
3. **Four posts, created one after the other** (from the Blog's editor, click **Create Blog Post**
   (`ListPartNavigationAdmin.cshtml:52`), fill the **Title** and the body text, leave the other fields empty, click
   **Publish**; none of the other fields is required, `blog.recipe.json`. **Publish** returns to the Blog's editor, where
   the next post starts. Wait a few seconds between two posts):

   | Order | Title | Body text |
   |---|---|---|
   | 1 | Weekend Market Hours | We are at the Saturday market from eight until noon. |
   | 2 | Spring Menu Preview | Rhubarb tarts and lemon scones join the counter this month. |
   | 3 | Sourdough Class Dates | Our sourdough class meets every second Sunday. |
   | 4 | Holiday Opening Times | We close at three on the last day before the holiday. |

4. **One draft that is never published**: **Create Blog Post**, Title `Autumn Menu Ideas`, body text
   `Pumpkin loaf and apple galettes are being tested.`, click **Save Draft** (not Publish; `OrchardCore.Contents/Views/Content.SaveDraftButton.cshtml:19`).
5. **Disable Feeds.** Tools, Features, type `Feeds` in the **Search** box, click **Disable** on the Feeds row, and click
   **Yes** in the dialog titled **Disable Feature** ("Are you sure you want to disable the Feeds feature? Continue?").
   The message reads "The feature Feeds was disabled." Check that the Lists row still shows **Enabled**.

Expected feed once Feeds is enabled, newest first (five entries, the draft is absent): Holiday Opening Times, Sourdough
Class Dates, Spring Menu Preview, Weekend Market Hours, Opening Week Notes. With **Feed items count** set to 3 it holds
the first three.

**Snapshot S0**: stop the site, copy its `App_Data` folder and `state.json` aside. This is the state clip `02-enable`
starts from.

**The id of the Blog** (needed by `04-control`): on Content Items, read the address of the **Blog** title link. It looks
like `/Admin/Contents/ContentItems/<blogContentItemId>/Edit?returnUrl=...`. For example:
`MSYS_NO_PATHCONV=1 Recorder probe /Admin/Contents/ContentItems "() => [...document.querySelectorAll(\"a[href*='/Edit']\")].find(a => a.textContent.trim() === 'Blog').href"`.
The feed of the Blog is expected at `/Contents/Lists/<blogContentItemId>/rss` (route in `OrchardCore.Lists/Startup.cs:102-106`).

## Order of the clips, and what each one changes

Record in the order of the chapters: `02-enable`, `03-find`, `04-control`.

| Clip | Starts from | Changes the site? | Before a new take |
|---|---|---|---|
| `02-enable` | S0 (Feeds disabled) | Yes: enables Feeds | Restore S0. |
| `03-find` | S1 (Feeds enabled) | No: it opens the editor and a feed, and saves nothing | Nothing to restore. |
| `04-control` | S1 | Yes: publishes the Blog with 3 items, a proxy address, and the feed turned off | Restore S1. |

**Snapshot S1**: after `02-enable` has been recorded, stop the site and copy `App_Data` and `state.json` aside again.
`03-find` and `04-control` both start from it. `04-control` does not depend on `03-find`.

Slides (`idea-1` to `idea-4`) and the opening and closing cards are drawn by the build; they need no recording.

## Slides and cards

- Opening card, step `intro`: the title "Publish an RSS Feed of Your Content" and its subtitle; no recording.
- `idea-1` (slide): "Following a site without visiting it", two cards: A feed, A feed reader.
- `idea-2` (slide): "Two features work together", two cards: Feeds, Lists.
- `idea-3` (slide): "What the feed of a list holds", two cards: The list, Its items.
- `idea-4` (slide): "How readers find a feed", two cards: A link in the page, Controlled by the list.
- Closing card, step `outro`: the same title, subtitle "docs.orchardcore.net"; no recording.

## Clip `02-enable` (chapter 2, "Enable the features")

Start on `/Admin` (the dashboard), signed in, from S0. `OpenAsync("/Admin")`, then `StartCaptureAsync()`.

- `enable-1`: open the **Tools** menu in the side menu, then click **Features** with `ClickAndWaitForNavigationAsync`
  (the page is `/Admin/Features`). Click the search box and type `Feeds` (`#search-box`, placeholder **Search**). The
  list narrows as you type. *On screen when the narration ends:* the Features page with `Feeds` in the search box, and
  the **Feeds** row alone under the heading **Content** (the search compares the name, the id and the description of a
  feature, `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`, and only Feeds contains "feeds"), with the
  **Disabled** badge and a blue **Enable** button. *Labels:* **Tools** and **Features** `OrchardCore.Features/AdminMenu.cs:26-27`; **Search**
  `OrchardCore.Features/Views/Admin/Features.cshtml:27`; the category heading `Features.cshtml:88` (**Content** comes
  from `OrchardCore.Feeds/Manifest.cs:9`); **Disabled** `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`.
- `enable-2`: click the **Enable** link of the Feeds row (`a[id^='btn-enable-OrchardCore_Feeds']`; the id is built from
  the feature id, confirm it with `Recorder probe`) with `ClickAndWaitForNavigationAsync`. The site restarts the tenant, so
  the page can take a few seconds: wrap the wait for the message in `OffCameraAsync`, for example
  `scene.OffCameraAsync(() => scene.Page.GetByText("The feature Feeds was enabled.").WaitForAsync())`, and
  keep the message on screen until the narration ends. *On screen when the narration ends:* the Features page with the
  success message "The feature Feeds was enabled." at the top. *Labels:* **Enable** `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`;
  the message `OrchardCore.Features/Controllers/AdminController.cs:317`.
- `enable-3`: click the search box (it is empty after the reload) and type `Lists`. *On screen when the narration ends:*
  the **Lists** row alone under the heading **Content Management**, with the **Enabled** badge and a red **Disable** button
  (do not click it). Only Lists contains "lists" in its name, id or description, so no scrolling is needed.
  *Labels:* **Content Management** `OrchardCore.Lists/Manifest.cs:15`; **Enabled** `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`;
  **Disable** `FeatureEntry.Buttons.SummaryAdmin.cshtml:53`.

After the clip, stop the site and save snapshot S1.

## Clip `03-find` (chapter 3, "The feed of a list")

Start on `/Admin` (the dashboard), signed in, from S1. `OpenAsync("/Admin")`, then `StartCaptureAsync()`.

- `find-1`: click **Content** in the side menu, click **Content Items** (`ClickAndWaitForNavigationAsync`), then click the
  title link whose text is exactly **Blog** (`a[href*='/Edit']:text-is('Blog')` in the Title column: the Type badge is also a link with the text Blog, so the address filter is needed; `ClickAndWaitForNavigationAsync`).
  *On screen when the narration ends:* the editor of the Blog (page `/Admin/Contents/ContentItems/<blogContentItemId>/Edit`),
  with the title **Blog** at the top. *Labels:* **Content** and **Content Items** `OrchardCore.Contents/AdminMenu.cs:65,68`;
  the title link `OrchardCore.Contents/Views/ContentsTitle_SummaryAdmin.cshtml:10`.
- `find-2`: scroll the editor down until the three feed settings are in view together (scroll with `ScrollAsync(0, ...)`
  by small steps). They are the last block of the editor: the parts of the Blog are ordered Title, Autoroute, Html body,
  Blog (with the Banner Image field) and Lists, at positions 0 to 4 (`blog.recipe.json:243-316`), and the feed settings
  take the position of the list part. The card with **List Blog Post Items** and **Create Blog Post** is at the top
  (`OrchardCore.Lists/Drivers/ListPartDisplayDriver.cs:83`). What fits on one screen is in `OPEN-QUESTIONS.md`. Ring the checkbox with `HighlightAsync(scene.Page.Locator("label[for$='DisableRssFeed']"), "Disable RSS feed link generation.")`.
  *On screen when the narration ends:* the checkbox **Disable RSS feed link generation.** unchecked and ringed, the field
  **Feed proxy Url** empty and the field **Feed items count** showing 20, all three visible. *Labels:*
  `OrchardCore.Lists/Views/ListPartFeed.Edit.cshtml:8,14,23`; the default of 20 `OrchardCore.Lists/Feeds/ListFeedEditViewModel.cs:11`
  and `ListPartFeedDisplayDriver.cs:25`.
- `find-3`: `ClearHighlightsAsync()`, then ring the hint under **Feed proxy Url** that reads "The current feed is available
  at" followed by a link (`span.hint:has-text('The current feed is available at') a`). Click the link; it
  opens a second tab. For example: `var feed = await scene.Page.RunAndWaitForPopupAsync(() => scene.ClickAsync(link));`
  then `await scene.SwitchToAsync(feed);` and wait for the page to load. *On screen when the narration ends:* the second
  tab with the feed loaded (the hint before the click showed an address such as
  `http://localhost:5010/Contents/Lists/<blogContentItemId>/rss`). *Label:* `OrchardCore.Lists/Views/ListPartFeed.Edit.cshtml:18`.
- `find-4`: no action; hold on the top of the feed. *On screen when the narration ends:* the start of the feed, showing
  the channel header: the title **Blog**, the address of the Blog and its description `News and notes from Example Bakery.`
  The address is expected to be `http://localhost:5010/` (the site root), not `/blog`, because the Blog is the home page
  of this recipe and the home route takes priority when an address is built for it
  (`OrchardCore.HomeRoute/Startup.cs:12-18`, `Routing/HomeRouteValuesAddressScheme.cs:30-48`,
  `src/OrchardCore/OrchardCore/Shell/Builders/ShellContainerFactory.cs:122`,
  `src/OrchardCore/OrchardCore.Abstractions/Routing/ShellRouteValuesAddressScheme.cs:29-37`). Confirm it on screen.
  *Source:* `OrchardCore.Lists/Feeds/ListFeedQuery.cs:66-80`.
- `find-5`: scroll slowly down the feed (`ScrollAsync(0, 300)` in two or three moves) so that the first entries pass by.
  Never scroll past the third entry. *On screen when the narration ends:* the first entry, Holiday Opening Times, with its
  title, its link, its body text and its publication date, and the start of the second entry, Sourdough Class Dates.
  The draft Autumn Menu Ideas appears nowhere. *Source:* `OrchardCore.Contents/Feeds/CommonFeedItemBuilder.cs:34-61`;
  published items only `ListFeedQuery.cs:104`.

Close the second tab afterwards. Nothing was saved: leave the editor with no change.

## Clip `04-control` (chapter 4, "Control the feed")

Before `StartCaptureAsync()` (not recorded), from S1: `OpenAsync("/Admin/Contents/ContentItems/<blogContentItemId>/Edit")`
(no `returnUrl`, so **Publish** stays on the editor and shows the message "Your Blog has been published.", see
`OrchardCore.Contents/Controllers/AdminController.cs:967` and `EditInternalAsync`). Keep a reference to this page
(`editorPage`). Scroll to the hint under **Feed proxy Url** and click its link to open the feed in a second tab (`feedPage`,
as in `find-3`; the feed shows five entries). Make `editorPage` the page that is recorded and scroll it so the three feed
settings are in view, then start the capture. If `SwitchToAsync` cannot return to a tab already used, close the second tab
and open it again from the hint link wherever the plan says "switch to the feed".

- `control-1`: ring **Feed items count** with `HighlightAsync(scene.Page.Locator("input[name$='FeedItemsCount']"), "Feed items count")`
  (it shows 20), then clear it with `FillAsync("")`, `TypeAsync(..., "3")` and press Tab. *On screen when the narration ends:*
  **Feed items count** showing 3, still ringed. *Labels:* `OrchardCore.Lists/Views/ListPartFeed.Edit.cshtml:23`.
- `control-2`: `ClearHighlightsAsync()`, click **Publish** (`button[name='submit.Publish'].publish`, `ClickAndWaitForNavigationAsync`); the editor
  reloads with "Your Blog has been published." Then `SwitchToAsync(feedPage)` and reload it with
  `WaitForNavigationAsync(() => feedPage.ReloadAsync())`. *On screen when the narration ends:* the feed with the channel
  header and exactly three entries: Holiday Opening Times, Sourdough Class Dates, Spring Menu Preview. *Labels:* **Publish**
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`; the feed reads the published version
  (`OrchardCore.Lists/Feeds/ListFeedQuery.cs:35,55`, default of `GetAsync` in `src/OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:102-109`).
- `control-3`: `SwitchToAsync(editorPage)`, scroll so the three settings are in view, ring **Feed proxy Url** and type
  `https://example.com/blog/feed` into it (`input[name$='FeedProxyUrl']`), then press Tab. Do not publish yet. *On screen
  when the narration ends:* **Feed proxy Url** showing `https://example.com/blog/feed`, ringed, the checkbox **Disable RSS feed
  link generation.** visible above it. *Source:* `OrchardCore.Lists/Views/ListPartFeed.cshtml:20-22` (the link in the page
  uses the proxy address when there is one).
- `control-4`: `ClearHighlightsAsync()`, click the checkbox (`input[type='checkbox'][name$='DisableRssFeed']`, not the hidden
  field of the same name) so that it shows checked, click **Publish** (`ClickAndWaitForNavigationAsync`), then
  `SwitchToAsync(feedPage)` and reload it with `WaitForNavigationAsync(() => feedPage.ReloadAsync())`. *On screen when the
  narration ends:* the feed address no longer shows a feed: the Not Found page of the site, whose heading reads "The page could not be
  found." (the Diagnostics feature, enabled by the Blog recipe, re-executes status 404 with that page:
  `OrchardCore.Diagnostics/DiagnosticsStartupFilter.cs:29`, `Views/HttpError-NotFound.cshtml:1`; confirm it on screen,
  see `OPEN-QUESTIONS.md`). *Source:* `OrchardCore.Lists/Feeds/ListFeedQuery.cs:38-40` returns no match, and
  `OrchardCore.Feeds/Controllers/FeedController.cs:59` answers Not Found; the link in the page is not registered
  (`ListPartFeed.cshtml:13`).

After the clip, stop the site and restore S1 before the next take.

## Checks for private data

- The site is fresh: the only user is `admin` with `admin@example.com`; all content is the invented bakery content
  above. The feed holds no author. The addresses shown are `localhost:5010`, `example.com` and the invented titles.
- OCR the video at one frame per second and at every scene change: the text may hold only the invented titles, `admin`,
  `localhost`, `example.com`. No real name, no real email, no token, and never the password.
- The password is never typed on camera: the clips start signed in from `state.json`.
- Look at every contact sheet of `review.py`: no error message other than the expected Not Found of `control-4`, and no
  empty list.

# Recording plan: Implement Full-Text Search

A person records this video from this plan alone. A selector in this file stands for `scene.Page.Locator(...)`. The
file:line references are in the Orchard Core source the programme works from (`src/OrchardCore.Modules/...`); check a
label there when the screen disagrees. Questions that only a running site can answer are in `OPEN-QUESTIONS.md`.

The video has recordings (chapters 02 to 08) and slides (chapters 01, 03 end, 07, 09, 10). Only the recordings are made
with the recorder. The slides are drawn by `slides.py`.

## The site

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with
  its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`), as in `references/recording.md`.
  `HTTP_PROXY` and `HTTPS_PROXY` point to a dead address, so the site never calls out.
- **Setup**: Auto Setup, with the **Blog** recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, tenant
  `Default`. Sign in as user `admin`, email `admin@example.com`, password `<password>` (passed in `VIDEO_PASSWORD`,
  chosen outside git). No setup or sign-in form is recorded. `Recorder setup` writes `state.json` (git-ignored).
- **Theme**: the Blog recipe selects `TheBlogTheme` for the site and `TheAdmin` for the admin. The search page of the
  video is drawn by the templates of `TheBlogTheme` (`Search-List.liquid`, `Search-Form.liquid`,
  `Search-Results.liquid`).
- **Features before the recording**: only those the Blog recipe enables. **Lucene (`OrchardCore.Lucene`) and Search
  (`OrchardCore.Search`) must be disabled**: clip `02-features` enables them on camera. Check it at Tools, Features:
  both rows show an Enable button.
- **Settings before the recording**: none. Site Search is empty, the Anonymous role is as Setup left it, and the
  Blog Post content type has the default Full-text options (Include display text and Include body parts selected,
  Use custom full-text cleared).
- **External services**: none, so no stand-ins.
- **Browser**: the recorder's defaults (1536x864 CSS pixels, scale 1.25, light, `en-US`).

### Seed data (created before the first clip, outside the capture)

Three Blog Posts, published, in the sample **Blog** that the recipe creates. Create each one from Content, Content
Items: on the row of **Blog**, select the button that lists its items, then **Create Blog Post**
(`OrchardCore.Lists/Views/ListPartSummaryAdmin.cshtml:10`, `ListPartNavigationAdmin.cshtml:52`). The first button is
labeled from the technical name of the contained type (`ListPartDisplayDriver.cs:98`), so it most likely reads
**List BlogPost Items**; the page it opens shows **Create Blog Post**, labeled from the display name. Fill the title,
the Subtitle field and the Markdown body (the Blog recipe gives the body the Wysiwyg editor of the Markdown body part:
type plain paragraphs), and select **Publish** (`OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`). The values
are invented. Do not add tags, a category or an image.

| # | Title | Subtitle | Body |
|---|---|---|---|
| A | Winter pruning for apple trees | Grafting basics for small gardens | Prune apple trees while they are dormant. Remove dead wood first, then thin crowded branches so light reaches the center of the tree. |
| B | Choosing a cider apple | Bittersweet or bittersharp | Cider makers blend apple varieties for sweetness, acidity and tannin. Press a small batch before committing a whole harvest. |
| C | Storing the autumn harvest | Cool cellars and slatted crates | Wrap sound pears in paper and keep them in a cool, dark cellar. Check the crates every week and remove fruit that starts to soften. |

What the search terms must give (the sample post of the recipe, "Man must explore, and this is exploration at its
greatest", contains none of these words):

| Term | Before clip `07-type` is saved and the index rebuilt | After the rebuild of clip `08-rebuild` |
|---|---|---|
| `apple` | posts A and B | posts A and B |
| `grafting` | no result: "There are no such results." | post A (the word is only in its subtitle) |

Take a snapshot **S0** here: stop the site, copy `App_Data` and `state.json` aside.

## Order of the clips and what each one changes

The clips depend on each other, and they play in the order they are recorded in.

| Order | Clip | Starts from | Changes the site | Snapshot to take afterwards |
|---|---|---|---|---|
| 1 | `02-features` | S0 | Enables Lucene and Search | S1 |
| 2 | `03-index` | S1 | Creates the index **posts** (Index Name **Posts**), filled in the background | S2 (once the index is filled, see the third note below) |
| 3 | `04-settings` | S2 | Saves Site Search | S3 |
| 4 | `05-permission` | S3 | Grants a permission to Anonymous | S4 |
| 5 | `06-try` | S4 | Nothing | none |
| 6 | `07-type` | S4 | Saves the Blog Post content type | S5 |
| 7 | `08-rebuild` | S5 | Rebuilds the index | none |

- Record `06-try` before `07-type`: its third step needs a subtitle that is not in the index. A new take of `06-try`
  can start from S4 or S5 (both have an index without the subtitle).
- To record a clip again, stop the site, restore the snapshot named in the table (the snapshot taken before that clip
  changed the site), and start it again. To record everything again, restore S0 and start at clip 1.
- After `03-index`, wait until the index has finished synchronizing before the snapshot S2 and before `06-try`:
  the search page needs the Site Search setting of clip `04-settings`, so use `/search/posts?Terms=apple` (outside the capture) until it lists posts A and B. This takes a few seconds, because the
  index is filled after the request that created it ends.
- The slide steps need no recording. Chapter cards are drawn by `assemble.py`.

## Timing of the menu paths

A step lasts about 2.5 words per second, so a step of 17 to 21 words has 7 to 8 seconds. The narration of several steps
names a menu path ("Open Settings, Search, Site Search"). Click every level in the capture when the actions fit in the
step. When they do not, open the parent entries before `StartCaptureAsync` (outside the capture) and click only the
last entry in the capture. The actions of a step must always end before its narration does.

## Clip `02-features` (chapter 02, Enable the features)

Start page: `/Admin` (the dashboard), signed in, nothing else open. Menu entries are clicked in the capture.

- `features-1`: Click **Tools** in the admin menu, then **Features** (`OrchardCore.Features/AdminMenu.cs:26-27`; use
  `ClickAndWaitForNavigationAsync` for **Features**; if the menu cannot be clicked reliably, call
  `NavigateAsync("/Admin/Features")`). Type `Lucene` in the search box `#search-box` (`TypeAsync`,
  `OrchardCore.Features/Views/Admin/Features.cshtml:27`). Wait until the row of `OrchardCore.Lucene` is shown with
  its Enable button `#btn-enable-OrchardCore_Lucene` (`FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the id is the feature id
  passed through `Html.GenerateIdFromName`, so check it on the page and fall back to the Enable link of the row).
  *On screen at the end of the narration*: the Features page, the search box holding `Lucene`, the Lucene row with a
  blue Enable button. The row shows the name **Lucene** and the code `OrchardCore.Lucene`
  (`FeatureEntry.Fields.SummaryAdmin.cshtml:11`).
- `features-2`: `ClickAndWaitForNavigationAsync` on `#btn-enable-OrchardCore_Lucene`. Wait for the notification.
  *On screen at the end*: the Features page with a green message "The feature Lucene was enabled." (or "The following
  features were enabled: ..." when a dependency is enabled with it; both are fine) (`OrchardCore.Features/Controllers/AdminController.cs:317`).
- `features-3`: Click `#search-box`, type `Search`. Wait for the row of `OrchardCore.Search`, then
  `ClickAndWaitForNavigationAsync` on `#btn-enable-OrchardCore_Search`. Wait for the notification.
  *On screen at the end*: the green message for the Search feature. Before clicking, check that the button belongs to
  the row whose code reads `OrchardCore.Search` (not `OrchardCore.Search.Lucene`, which is labeled "Lucene (Obsolete)").

## Clip `03-index` (chapter 03, Create an index)

Start page: `/Admin` (the dashboard).

- `index-1`: Click **Search** in the admin menu, then **Indexes** (`OrchardCore.Indexing/AdminMenu.cs:19,22`;
  the page is `/Admin/indexing`). Click the **Add index** button `button.create`
  (`OrchardCore.Indexing/Views/Admin/Index.cshtml:31`). The dialog **Available Sources** opens
  (`Index.cshtml:61`). Under the Lucene section, click **Add** on the card **Content in Lucene**
  (`.card` containing `h4` "Content in Lucene", then its `a` "Add"; `IndexProfile.Link.cshtml:29`,
  `OrchardCore.Lucene/Startup.cs:98`). Use `ClickAndWaitForNavigationAsync` for the last click. Keep the pauses
  short: this step has about 8 seconds.
  *On screen at the end*: the page titled **New 'Content in Lucene' index** (`Admin/Create.cshtml`) with the empty form.
- `index-2`: `TypeAsync` in the **Index Name** box (`GetByLabel("Index Name", exact)`;
  `OrchardCore.Indexing/Views/IndexProfileFields.Edit.cshtml:6`) the text `Posts`. The **Name** box
  (`GetByLabel("Name", exact)`, `IndexProfileFields.Edit.cshtml:14`) fills itself with `posts`; wait until its value
  is `posts`. Highlight the Name box with `HighlightAsync`, then `ClearHighlightsAsync`.
  *On screen at the end*: Index Name `Posts`, Name `posts`.
- `index-3`: `ScrollAsync` until **Content types** is visible. Click the check box labeled **Blog Post**
  (`label` with the exact text "Blog Post" in `.content-types`;
  `OrchardCore.Indexing/Views/ContentIndexMetadata.Edit.cshtml:17`). Leave **Culture** on **Any culture**
  (`ContentIndexMetadata.Edit.cshtml:9`).
  *On screen at the end*: only **Blog Post** is checked in the Content types list. The Content types block
  (`ContentIndexProfileDisplayDriver.cs:49`) and the Analyzer block (`LuceneIndexProfileDisplayDriver.cs:55`) have the
  same position, `Content:5`; the Content types block is expected first (see `OPEN-QUESTIONS.md`).
- `index-4`: `ScrollAsync` until **Analyzer** is visible. `HighlightAsync` the Analyzer drop-down (value
  `standardanalyzer`; `OrchardCore.Lucene/Views/LuceneIndexProfile.Edit.cshtml:4`) and the **Store Source Data**
  check box (cleared; `LuceneIndexProfile.Edit.cshtml:15`). Change nothing.
  *On screen at the end*: the Analyzer drop-down reading `standardanalyzer` and Store Source Data cleared, with the
  highlight ring on them.
- `index-5`: `ClearHighlightsAsync`, then `ScrollAsync` until **Default Query Settings** is visible
  (`OrchardCore.Lucene/Views/LuceneQuerySettings.Edit.cshtml:7`). `HighlightAsync` the **Query field names** list
  (`#DefaultQueryFields`, `LuceneQuerySettings.Edit.cshtml:37`).
  *On screen at the end*: the Query field names list with **Content.ContentItem.FullText** checked and the other
  fields cleared. On a new index the list holds three fields, **ContentItemId**, **ContentItemVersionId** and
  **Content.ContentItem.FullText**, and it does not grow when **Blog Post** is checked: the fields of the content
  types are added when the index is saved (`LuceneContentIndexProfileHandler.cs:34-41`, `LuceneIndexProfileDisplayDriver.cs:70-85`).
  Check this before the take (outside the capture, on a first pass): if another field is checked, clear it by hand
  before recording and note it in `OPEN-QUESTIONS.md`.
- `index-6`: `ClearHighlightsAsync`, `ScrollAsync` to the bottom, `ClickAndWaitForNavigationAsync` on **Save**
  (`button[type=submit]` with the text "Save"; `OrchardCore.Indexing/Views/Admin/Create.cshtml:16`). Wait for the
  notification.
  *On screen at the end*: the **Indexes** list with the green message "An index has been created successfully. The
  synchronizing process was triggered in the background." (`OrchardCore.Indexing/Controllers/AdminController.cs:257`)
  and one row, **posts**, with the badges **Lucene** and **Content**
  (`IndexProfile.DefaultTags.SummaryAdmin.cshtml`).

## Clip `04-settings` (chapter 04, Choose the search index)

Start page: `/Admin` (the dashboard).

- `settings-1`: Click **Settings** in the admin menu, then **Search**, then **Site Search**
  (`OrchardCore.Search/AdminMenu.cs:25-27`; use `ClickAndWaitForNavigationAsync` for **Site Search**). Pick `posts` in
  the **Default search index** drop-down with `SelectAsync` (`OrchardCore.Search/Views/SearchSettings.Edit.cshtml:6`,
  first option **Select a default index**, line 9).
  *On screen at the end*: the Site Search page, **Default search index** reading `posts`, the **Placeholder** box empty.
- `settings-2`: `TypeAsync` in the **Placeholder** box (`SearchSettings.Edit.cshtml:17`) the text
  `Search the orchard journal`, press Tab, then `ClickAndWaitForNavigationAsync` on **Save**
  (`OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`). Wait for the notification.
  *On screen at the end*: the green message "Site settings updated successfully."
  (`OrchardCore.Settings/Controllers/AdminController.cs:119`) with the Placeholder holding the text.

## Clip `05-permission` (chapter 05, Grant access to the index)

Start page: `/Admin` (the dashboard).

- `permission-1`: Click **Access Control** in the admin menu, then **Roles** (`OrchardCore.Roles/AdminMenu.cs:18-20`).
  On the row **Anonymous**, `ClickAndWaitForNavigationAsync` on **Edit**
  (`a.edit[href$='/Roles/Edit/Anonymous']`; `OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`).
  *On screen at the end*: the page **Edit 'Anonymous' Role** with the **Search permissions** box and the permission groups.
- `permission-2`: `TypeAsync` in `#search-box` (placeholder **Search permissions**;
  `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49`) the text `posts`. Wait until two rows are left:
  **Query 'posts' Index** and **Query Lucene Posts Index**. Click the **Allow** check box of the first
  (`[id="Checkbox.QueryIndex_posts"]`; descriptions from `OrchardCore/OrchardCore.Indexing.Core/IndexingPermissions.cs:15`
  and `OrchardCore.Lucene/LuceneIndexPermissionHelper.cs:16`; the column title is at `_RolePermissions.cshtml:69`).
  `HighlightAsync` that row. Do not check the Lucene row. The permission groups are named after the feature of their
  provider (`OrchardCore.Roles/Controllers/AdminController.cs:412-427`): **Query 'posts' Index** is expected under
  **Indexing** (`IndexingPermissionsProvider.cs`) and **Query Lucene Posts Index** under **Lucene**
  (`OrchardCore.Lucene/Permissions.cs:27`).
  *On screen at the end*: the two rows, the **Allow** box of **Query 'posts' Index** checked, the other cleared.
- `permission-3`: `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` on **Save**
  (`OrchardCore.Roles/Views/Admin/Edit.cshtml:33`). Wait for the notification.
  *On screen at the end*: the Roles list with the green message "Role updated successfully."
  (`OrchardCore.Roles/Controllers/AdminController.cs:213`).

## Clip `06-try` (chapter 06, Try the search page)

Start page: `/` (the home page of the blog). Before the take, check that `/search?Terms=apple` lists posts A and B (the index
has finished synchronizing). The session is signed in as an administrator, who can query every index; the clip does not
show what the permission changes for a visitor (see `OPEN-QUESTIONS.md`).

- `try-1`: `NavigateAsync("/search")` (`OrchardCore.Search/Controllers/SearchController.cs:64`). Wait for the form.
  *On screen at the end*: the front-end **Search** page of `TheBlogTheme`: the header "Search", an empty box with the
  placeholder `Search the orchard journal`, and the **Search** button (`TheBlogTheme/Views/Search-Form.liquid:17`,
  `Search-List.liquid:9-10`).
- `try-2`: `TypeAsync` in `input[name='Terms']` the text `apple`, then `ClickAndWaitForNavigationAsync` on the **Search**
  button (`form:has(input[name='Terms']) button[type=submit]`; `Search-Form.liquid:14-18`). *On screen at the end*: two results, **Winter pruning for apple trees** and
  **Choosing a cider apple**, each with its subtitle and the line "Posted by admin", drawn by
  `Content-BlogPost.Summary.liquid` (the Summary display type; `Search-Results.liquid`).
- `try-3`: The box holds `apple` from the previous search (`Search-Form.liquid:16`), and `TypeAsync` clicks the box
  and then types at the caret, so a selection made before it is lost. Clear the box first, off screen:
  `await scene.Page.Locator("input[name='Terms']").FillAsync("")`. Then `TypeAsync` the text `grafting` and
  `ClickAndWaitForNavigationAsync` on the **Search** button. *On screen at the end*: the yellow alert "There are no
  such results." (`OrchardCore.Search/Views/Search-Results.cshtml:43`, and the same text in `Search-Results.liquid` of the theme). The
  subtitle of post A, "Grafting basics for small gardens", was visible in the previous step.

## Clip `07-type` (chapter 07, Choose what is indexed)

Start page: `/Admin` (the dashboard), from snapshot S4.

- `type-1`: Click **Design** in the admin menu, then **Content Definition**, then **Content Types**
  (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`). On the row **Blog Post**, `ClickAndWaitForNavigationAsync` on **Edit**
  (`a[role='btn-edit-BlogPost']`; `ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`). `ScrollAsync` until the heading
  **Full-text** is visible (`OrchardCore.Contents/Views/FullTextAspectSettings.Edit.cshtml:15`).
  *On screen at the end*: the page **Edit Content Type - Blog Post** scrolled to the **Full-text** section, with
  **Include display text** and **Include body parts** checked and **Use custom full-text** cleared
  (`FullTextAspectSettings.Edit.cshtml:27,37,47`).
- `type-2`: Click the **Use custom full-text** check box (`input[id$='IncludeFullTextTemplate']`). A Liquid editor
  (CodeMirror) opens in `#fulltext-template`. Click the visible editor `#fulltext-template .CodeMirror` (CodeMirror 5 forwards the keys from the hidden text area inside it) and `TypeAsync` the text
  `{{ Model.Content.BlogPost.Subtitle.Text }}` (exactly this, no new line). Before the take, check that the editor holds
  this text and no extra braces (see `OPEN-QUESTIONS.md`).
  *On screen at the end*: the editor holding `{{ Model.Content.BlogPost.Subtitle.Text }}` under the checked box.
- `type-3`: Scroll to the bottom, `ClickAndWaitForNavigationAsync` on **Save**
  (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:215`). Wait for the notification.
  *On screen at the end*: the Content Types list with the green message "Content type updated successfully."
  (`OrchardCore.ContentTypes/Controllers/AdminController.cs:225`).

## Clip `08-rebuild` (chapter 08, Rebuild the index)

Start page: `/Admin` (the dashboard), from snapshot S5.

- `rebuild-1`: Click **Search**, then **Indexes** in the admin menu (`/Admin/indexing`). On the row **posts**, click the
  **Actions** drop-down (`button.actions` inside `.item[data-filter-value='posts']`; the default list layout shows the
  Edit and Delete buttons and an **Actions** drop-down; `OrchardCore.Admin/Views/AdminListActions.cshtml:30-32`). Click
  **Rebuild** in the menu (`OrchardCore.Indexing/Views/IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:32`).
  *On screen at the end*: the dialog **Rebuild Index** with the text "Your index will be rebuilt, which might alter some
  services during the process. Continue?" and the buttons **Yes** and **No** (same file, line 28 and following).
- `rebuild-2`: Click **Yes** (`ClickAndWaitForNavigationAsync`). *On screen at the end of the narration*: the Indexes
  list with the green message "An index has been rebuilt successfully. The synchronizing process was triggered in the
  background." (`OrchardCore.Indexing/Controllers/AdminController.cs`, `Rebuild`). Then, at the very end of the step,
  wait off camera with `scene.OffCameraAsync(...)` until the index holds the subtitle: poll
  `fetch('/search?Terms=grafting')` in the page every second (60 seconds at most) until the response text contains
  `Winter pruning for apple trees`. The condition is specific to the new state; before the rebuild it is false.
- `rebuild-3`: `NavigateAsync("/search")`, `TypeAsync` in `input[name='Terms']` the text `grafting` (the box is empty on a new
  page), then `ClickAndWaitForNavigationAsync` on the **Search** button. *On screen at the end*: one result, **Winter pruning for
  apple trees**, with its subtitle "Grafting basics for small gardens".

## Slides (no recording)

- `intro`: the opening card, drawn by `assemble.py` with the title of the video.
- `flow-1`: four cards of "From a content item to a results page"; the first two are bright (content item, index).
- `flow-2`: the same four cards; the last two are bright (settings and permission, search page).
- `analyzer-1`: the registration of the built-in analyzer in a Startup class (`OrchardCore.Lucene/Startup.cs:46-48`), with the using directives it needs and two cards "The name" and "The instance".
- `options-1`: three cards, one per option of the Full-text section: Include display text, Include body parts, Use custom full-text.
- `widgets-1`: two one-line Liquid expressions (a field of the content item, the widgets of a Flow part) and two cards,
  "Model" and "full_text".
- `templates-1`: four cards: Search-List, Search-Form, Search-Results, and the same-name rule for a theme.
- `templates-2`: two panels: `Search-Results.liquid` with the display type changed to SearchSummary (the highlights branch of the theme file is replaced by a Liquid comment), and
  `Content-BlogPost.SearchSummary.liquid`.
- `recipe-1`: the `feature` and `CreateOrUpdateIndexProfile` steps of `blog.lucene.search.recipe.json`, trimmed: two of the eleven content types, and one copy of the index settings (the file repeats them at the top level and under `Properties`).
- `recipe-2`: the `Settings`, `RebuildIndex` and `Roles` steps of the same recipe.
- `outro`: the closing card, drawn by `assemble.py`.

## Checks for private data

- Every name on screen is invented: the user is `admin`, the email is `admin@example.com` (it never appears on a
  recorded page), the posts are the three above. The only dates are the date of the recording.
- The password `<password>` is never typed on camera: the session comes from `state.json`.
- No machine path, user name or port other than `localhost:5010` appears. Check the address bar is hidden, and
  that no notification shows a file path.
- Look at the contact sheet of every clip (`review.py`), then OCR the video at one frame per second and at every scene
  change, and search the text for real names, email addresses and tokens, as `SKILL.md` asks.

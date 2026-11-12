# Recording plan: Taxonomies in Code: Query Content Items by Term

A developer video: 14 slide steps (drawn by `slides.py`, nothing to record), two cards, and **two recordings**:
`03-index` (the Lucene index settings of two fields, and a rebuild) and `04-query` (saving the Lucene query, and
running it with a parameter). In this plan a selector such as `button.save` stands for
`scene.Page.Locator("button.save")`. Every label below was read in the Orchard Core source (file:line, relative to
`src/OrchardCore.Modules/` unless it starts with `src/`). **Nothing here has been run on a site**: see
`OPEN-QUESTIONS.md` for what to confirm on the first take.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore` (the branch the video is about),
  on `http://localhost:5010`, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`), as
  `.agents/skills/orchardcore-video-producer/references/recording.md` describes. Block outbound calls with
  `HTTP_PROXY`/`HTTPS_PROXY` set to a dead address; nothing in this video calls an external service.
- **Provisioned by Auto Setup** (environment variables, so no setup or sign-in form is recorded):

  | Variable (`OrchardCore__AutoSetup__Tenants__0__...`) | Value |
  |---|---|
  | `ShellName` | `Default` |
  | `SiteName` | `Orchard Core` |
  | `SiteTimeZone` | `America/Los_Angeles` |
  | `AdminUsername` | `admin` |
  | `AdminEmail` | `admin@example.com` |
  | `AdminPassword` | `<password>` (the same value as `VIDEO_PASSWORD`, chosen outside git) |
  | `DatabaseProvider` | `Sqlite` |
  | `RecipeName` | `Blog` |

  Then sign the recorder in once (`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`), which writes
  `state.json` (git-ignored).
- **Features to enable before recording**: Lucene and the `Search` index. The `Blog` recipe enables `OrchardCore.Indexing`,
  `OrchardCore.Queries`, `OrchardCore.Queries.Sql` and `OrchardCore.Taxonomies`, but not Lucene
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-80`). Run the recipe that does:
  1. **Tools > Recipes** (`OrchardCore.Recipes/AdminMenu.cs:18-19`). Type `Lucene` in the search box.
  2. On **Blog - Lucene Search**, click **Run**, then **Yes** in the dialog
     (`OrchardCore.Recipes/Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:15-18`). The page shows
     "The recipe 'Blog - Lucene Search' has been run successfully."
  3. The recipe enables `OrchardCore.Search` and `OrchardCore.Lucene`, creates the index profile **Search** for the content
     types `Article`, `Blog`, `BlogPost` and others, with `IndexLatest` false (only published versions are indexed),
     and rebuilds it (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.lucene.search.recipe.json`).
  4. Check: **Search > Indexes** lists **Search**, and **Search > Queries** holds **All Queries** and **Run Lucene Query**
     (`OrchardCore.Indexing/AdminMenu.cs:19-22`, `OrchardCore.Queries/AdminMenu.cs:22`, `OrchardCore.Lucene/AdminMenu.cs:19-20`).
- **Settings**: none to save. Leave the **Category** and **Tags** fields of Blog Post as the recipe made them: neither has
  **Include this element in the index** checked (`03-index` checks it on camera).
- **Time zone and language**: `America/Los_Angeles`, `en-US`.

### Seed data (create it outside the capture, once, before the first snapshot)

The recipe makes the taxonomies **Categories** (term **Travel**) and **Tags** (terms **Earth**, **Exploration**, **Space**),
and one post, "Man must explore, and this is exploration at its greatest" (category Travel, the three tags).

1. **Add the term Politics.** Admin menu **Content > Taxonomy**, **Edit** on **Categories**. Click **Add Category**, type
   `Politics` in **Title**, click **Publish** (`OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml:48`,
   `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`). Back on the taxonomy, click **Publish** again:
   a term is saved in a draft of its taxonomy (`OrchardCore.Taxonomies/Controllers/AdminController.cs:110,163`), and the indexing
   reads the published taxonomy (`OrchardCore.Taxonomies/Indexing/TaxonomyFieldIndexHandler.cs:37`).
2. **Read the term ids.** On the taxonomy page, the **Edit** button of a term links to
   `/Admin/Taxonomies/Edit/<taxonomy-id>/<term-id>` (`OrchardCore.Taxonomies/Controllers/AdminController.cs:168`).
   Write down the last segment for **Politics** (`<politics-id>`) and for **Travel** (`<travel-id>`). They are random on every site, so
   they are never committed.
3. **Create three more posts.** Admin menu **Blog**, then **Create Blog Post**
   (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52`). Type the **Title**, choose the **Category**, add the **Tags**, click
   **Publish**. Publish them (a draft is not indexed):

   | Title | Category | Tags |
   |---|---|---|
   | Notes from a night train | Travel | Earth |
   | What the council voted on this week | Politics | none |
   | A town meeting, explained | Politics | none |

4. **Wait for the index** (the Lucene index is updated by a background task): open the `04-query` start address (below)
   and reload until it says **Found 4 result(s)**.

### Starting address of `04-query`, and how to build it

The query editors are CodeMirror editors that close brackets as you type, so the recorder never types the query. The Lucene
query page takes the query as Base64 in the address (`OrchardCore.Lucene/Controllers/AdminController.cs:70-88`) and runs it at
once. Save this exact text as `template.txt` **outside the repository** (it is the text of the `template-1` slide):

```
{
  "size": 10,
  "query": {
    "bool": {
      "must": [
        { "term": { "Content.ContentItem.ContentType": "BlogPost" } }
        {% if categories %},
        { "match": { "BlogPost.Category.Ids": { "query": "{{ categories }}", "operator": "or" } } }
        {% endif %}
        {% if tags %},
        { "match": { "BlogPost.Tags.Ids": { "query": "{{ tags }}", "operator": "or" } } }
        {% endif %}
      ]
    }
  }
}
```

Print the encoded text, and build the address:

```bash
python -c "import base64,sys,urllib.parse; t=open(sys.argv[1],encoding='utf-8').read(); print(urllib.parse.quote(base64.b64encode(t.encode('utf-8')).decode(), safe=''))" template.txt
# start address:  http://localhost:5010/Admin/Lucene/query/Search?query=<the printed text>
```

## Order, dependencies and snapshots

1. Provision the site, run **Blog - Lucene Search**, create the seed data, wait until the start address says **Found 4 result(s)**.
   **Stop the site, copy `App_Data` and `state.json` aside: snapshot S1.**
2. Record `03-index`. It **changes the site**: two fields get indexed and the `Search` index is rebuilt. Before another take,
   restore S1.
3. After `03-index`, wait for the rebuild to finish: open the address of the `run-2` step below until it says
   **Found 2 result(s)** (the term ids are indexed only after the rebuild). **Stop the site and copy `App_Data` aside: snapshot S2.**
4. Record `04-query`. It **depends on `03-index`** (the term ids must be in the index for `run-2`) and **changes the site**
   (it saves the query `GetBlogsByFilter`; a second take would stop at "A query with the same name already exists"). Before another take,
   restore S2.

The slide steps and the two cards need no recording.

## Clips

### `03-index` (chapter 03, "Index the terms for Lucene")

Starts on **Design > Content Definition > Content Types > Blog Post**: open `/Admin/ContentTypes/Edit/BlogPost`
(title "Edit Content Type - Blog Post", `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`) before the capture, with the
**Design** menu open on **Content Definition > Content Types** (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`); the narration of `index-1` names the menu, so the menu stays open and visible while it is read. The **Fields** list shows **Subtitle**, **Banner Image**, **Tags** and **Category** with an **Edit** button each
(`Edit.cshtml:88-100`). Start the capture here.

- `index-1`: Click **Edit** on the **Category** row
  (`li.list-group-item:has-text("Category") a:text-is("Edit")`, `ClickAndWaitForNavigationAsync`). The page is titled
  `"Category" settings` (`OrchardCore.ContentTypes/Views/Admin/EditField.cshtml:30`). Scroll down until the heading
  **Lucene Index settings** is in view (`scene.ScrollAsync(0, 700)`; clear highlights before scrolling), click the label
  **Include this element in the index** (`label:text-is("Include this element in the index")`), then click **Save**
  (`button.save`, `ClickAndWaitForNavigationAsync`). Labels: `OrchardCore.Lucene/Views/LuceneContentIndexSettings.Edit.cshtml:5,13`,
  `EditField.cshtml:120`. At the end of the narration the type editor is shown again.
  Keep the actions under the narration (about 11 seconds): do not highlight anything.
- `index-2`: Click **Edit** on the **Tags** row (`li.list-group-item:has-text("Tags") a:text-is("Edit")`), scroll to
  **Lucene Index settings**, click **Include this element in the index**, click **Save**. At the end of the narration
  the type editor is shown, after the second save.
- `index-3`: Click **Search** in the admin menu, then **Indexes** (`a:text-is("Indexes")`;
  `OrchardCore.Indexing/AdminMenu.cs:19-22`; expand the **Search** menu with a click that is part of this step, since the narration names it).
  The page "Indexes" lists the index **Search** (the address is `/Admin/indexing`). Click the **Actions** button of that row
  (`button.dropdown-toggle.actions`, `OrchardCore.Admin/Views/AdminListActions.cshtml:30-32`; the list uses the default **Buttons** layout),
  and leave the menu open: at the end of the narration the menu is open and lists **Rebuild** among its items (**Synchronize**, **Reset**,
  **Rebuild**, **Force Delete** and, for a Lucene index, **Query Index**:
  `OrchardCore.Indexing/Views/IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:5-42`, `OrchardCore.Lucene/Views/LuceneIndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:11`). Do not click anything in the menu yet.
- `index-4`: Click **Rebuild** (`a.dropdown-item:text-is("Rebuild")`, `WaitForNavigationAsync`). The link is a POST (`data-url-af="UnsafeUrl"`, same file, line 32) and the admin
  script submits it at once: it opens a confirmation dialog only for a link marked `RemoveUrl` or carrying `data-unsafe-url`, and this link has neither
  (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:74-86,118-184`), so **no dialog is expected** although the link holds `data-title` and `data-message`.
  If a dialog **Rebuild Index** does appear on the first take, click **Yes** (`button#modalOkButton`) inside the same step, and report it as described in
  `OPEN-QUESTIONS.md` (the narration of `index-4` stays true). The list is shown again with the
  notification "An index has been rebuilt successfully. The synchronizing process was triggered in the background."
  (`OrchardCore.Indexing/Controllers/AdminController.cs:480`). Hold until the end of the narration.

### `04-query` (chapter 04, "A Lucene query with parameters")

Starts on the **Lucene Query** page with the template already run: open the start address built above before the capture.
The page is titled **Lucene Query** (`OrchardCore.Lucene/Views/Admin/Query.cshtml:14`), the **Index** list shows **Search**,
the **Query** box holds the template, **Parameters** holds `{ }`, and under it: "Found 4 result(s) in N ms", a **Save Query** button, and a
table of four rows. Start the capture here.

- `query-1`: Highlight the **Query** box, then the line "Found 4 result(s) in N ms"
  (`HighlightAsync(scene.Page.Locator("textarea[name='DecodedQuery'] + .CodeMirror"))`; if that selector matches nothing, use
  `.CodeMirror` and take the first). Clear the highlights at the end. On screen at the end: the template, "Found 4 result(s)", and the table
  with the four posts (labels `Query.cshtml:39,47,54,64`).
- `query-2`: Click **Save Query** (`a.btn:text-is("Save Query")`, `ClickAndWaitForNavigationAsync`). The page "New Lucene query" opens
  (`OrchardCore.Queries/Views/Admin/Create.cshtml:4`) with the template already in **Query**
  (`OrchardCore.Lucene/Drivers/LuceneQueryDisplayDriver.cs`, `EditAsync`). Then:
  1. Type `GetBlogsByFilter` in **Name** (`div.ocat-wrapper:has(> label:text-is("Name")) input`, `TypeAsync`;
     `OrchardCore.Queries/Views/Query.Fields.Edit.cshtml:4`).
  2. Choose **Search** in **Index** (`SelectAsync(div.ocat-wrapper:has(> label:text-is("Index")) select, "Search")`;
     `OrchardCore.Lucene/Views/LuceneQuery.Edit.cshtml:9`). The list starts on "Select an index".
  3. Click the label **Return Content Items** (`label:text-is("Return Content Items")`, `LuceneQuery.Edit.cshtml:22`).

  Leave **Schema** empty. On screen at the end: Name filled, **Search** selected, **Return Content Items** checked, and the template.
- `query-3`: Click **Save** (`button.save:text-is("Save")`, `ClickAndWaitForNavigationAsync`; `Query.Fields.Buttons.cshtml:3`). The page
  **Queries** (menu item **All Queries**) shows "Query created successfully." (`OrchardCore.Queries/Controllers/AdminController.cs:162`) and a row
  **GetBlogsByFilter** with a **Lucene query** badge and a **Run** button. Hold until the end of the narration.
- `run-1`: Click **Run** on the **GetBlogsByFilter** row (`a.btn:text-is("Run")`, `ClickAndWaitForNavigationAsync`;
  `OrchardCore.Lucene/Views/LuceneQuery.Buttons.SummaryAdmin.cshtml:22`). The **Lucene Query** page opens with the template, **Search**
  selected, **Parameters** holding `{ }`, and "Found 4 result(s)". Highlight the **Parameters** box
  (`HighlightAsync(scene.Page.Locator("textarea[name='Parameters'] + .CodeMirror"), "Parameters")`) at the end of
  the narration. Note the address: it ends with `?query=<base64>`.
- `run-2`: Navigate (`NavigateAsync`, which takes the path and query of the address, `/Admin/Lucene/query/Search?query=<base64>`, without
  `http://localhost:5010`) to that address with `&parameters=` and the URL-encoded text
  `{"categories":"<politics-id>"}` added. The page runs the query as it opens (`AdminController.cs:70-88`): **Parameters** shows
  `{"categories":"<politics-id>"}`, the page says "Found 2 result(s) in N ms", and the table lists the two Politics posts, with a
  `BlogPost.Category.Ids` column holding `<politics-id>` (a stored field: `OrchardCore.Taxonomies/Indexing/TaxonomyFieldIndexHandler.cs:24,31`).
  Highlight the line "Found 2 result(s)" at the end of the narration, and scroll the table sideways first if the `BlogPost.Category.Ids`
  column is out of view.

## Slide steps (drawn by `slides.py`; nothing to record)

- `store-1`: the `TaxonomyField` class (`TaxonomyContentItemId`, `TermContentItemIds`) and two cards naming the two properties.
- `store-2`: the `TaxonomyIndex` class with its eight columns, and two cards: one row per term, and the Published and Latest flags.
- `sql-1`: the helper `QueryCategorizedContentItemsAsync`, trimmed from its source, and a one-line call in a view.
- `sql-2`: the related-items view of the documentation, with the lines that read the term ids and exclude the displayed item bright.
- `sql-3`: the same view, with the `index.Published` line bright.
- `sql-4`: two cards: only the selected terms have rows, and a parent term has none for items filed under a child.
- `sql-5`: a `TermContentService` class that runs the same query on `ISession`.
- `keys-1`: three cards: `BlogPost.Category.Ids`, `BlogPost.Category.Inherited`, and the setting **Include this element in the index**.
- `template-1`: the Liquid query template, with the content type clause and the `if` tags bright.
- `template-2`: the same template, with the `match` clauses and the `must` list bright.
- `razor-1`: the Razor code that builds the parameters and calls `ContentQueryAsync`, bright, with the loop that lists the posts below it, dimmed.
- `razor-2`: the loop that lists the `DisplayText` of each item, and the Liquid `query` filter call below it.
- `filter-1`: the trimmed `QueryAsync` of the default term list service, with the `TaxonomyIndex` line and the filter call bright, and the
  `IContentsTaxonomyListFilter` interface below.
- `filter-2`: the same two panels, with the interface bright.

The cards (`intro`, `outro`) are drawn from the storyboard.

## Checks for private data

- All names are invented: the site is **Orchard Core**, the user is `admin` (`admin@example.com`), the posts and terms are the fake ones above. No
  other email, name or token is typed or shown. The password is only in `VIDEO_PASSWORD` and `AdminPassword`; never in a file, an address
  or on screen (the sign-in is done by `state.json`).
- Look at every contact sheet (`review.py`) for: an empty table, an error such as "Invalid query", a result count other than 4 and 2, a draft
  banner, the **Parameters** box still holding `{ }` at the end of `run-2`.
- OCR the video at one frame per second and at every scene change, and search for any `@` address other than `admin@example.com`, any name that is not
  in the lists above, and the password.
- The term ids on screen are random ids of this throwaway site; they hold no personal data, but check that none of the other ids shown in
  `Found 4 result(s)` belongs to a real site.

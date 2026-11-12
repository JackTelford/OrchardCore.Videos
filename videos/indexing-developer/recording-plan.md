# Recording plan: Indexing in Code

A developer video. The code is on slides (`slides.py`, drawn from the storyboard, nothing to record). Only two chapter-4
clips are recorded in a browser, because the settings screen and the Indexes screen are part of the story: they decide
whether the part and field handlers run, and they rebuild the documents. In this plan a selector stands for
`scene.Page.Locator(...)`. Source references are `file:line` in the Orchard Core source at the commit named in
`catalog/features.json`, under `src/OrchardCore.Modules/`.

## The site it needs

- A fresh site made by Auto Setup with the **Blog** recipe: site name **Orchard Core**, time zone `America/Los_Angeles`,
  tenant `Default`, user `admin`, email `admin@example.com`, password `<password>` (passed in `VIDEO_PASSWORD`, chosen
  outside git, never written in a file). Run it on `http://localhost:5010` with its own `ORCHARD_APP_DATA`, and block
  outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. It is not a video about Setup, so Auto Setup
  and `state.json` are used as for any other video.
- The Blog recipe already enables Indexing, Contents, Content Types, Content Fields, Html and Markdown
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`), and creates the content type **Blog Post** (technical
  name `BlogPost`) with the part `BlogPost`, whose **Subtitle** is a Text field with no Lucene settings, and one sample
  blog post titled "Man must explore, and this is exploration at its greatest" whose subtitle is "Problems look mighty
  small from 150 miles up". No other content is needed.
- Features to enable before recording, outside the capture: **Lucene** (`OrchardCore.Lucene`). In **Tools, Features**
  type `Lucene` in the search box and press **Enable** on the row named exactly **Lucene** (`OrchardCore.Lucene`), not on
  **Lucene (Obsolete)**, **Lucene Worker** or **Lucene Content Picker**, which the same search lists
  (`OrchardCore.Lucene/Manifest.cs:10-45`). The button is a link, probably `#btn-enable-OrchardCore_Lucene`
  (`FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; probe the id); menu at `OrchardCore.Features/AdminMenu.cs:27`. It asks
  for no confirmation (the link has no dialog attributes): the page reloads with a notification that names the enabled
  features, "The feature Lucene was enabled." or, when dependencies are enabled with it, "The following features were
  enabled: ..." (`OrchardCore.Features/Controllers/AdminController.cs:313-317`). Do not enable the Indexing Worker,
  Elasticsearch or Azure AI Search: the Lucene settings section is the only one that must appear.
- Settings: none. External services: none, Lucene runs in the process.
- Seed data, created outside the capture. One index, through **Search, Indexes**
  (`OrchardCore.Indexing/AdminMenu.cs:19-22`):
  1. Press **Add index** (`Views/Admin/Index.cshtml:31`). In **Available Sources**, in the **Lucene** group, press **Add**
     on the **Content in Lucene** card (`OrchardCore.Lucene/Startup.cs:98`, `Views/IndexProfile.Link.cshtml:29`).
  2. On **New 'Content in Lucene' index**: **Index Name** `blogposts` (the **Name** field fills itself with the same
     value; if not, type `blogposts`), **Culture** left on **Any culture**, **Content types**: tick **Blog Post** only,
     leave **Index Latest version** unticked, leave **Analyzer** and **Store Source Data** as they are, press **Save**
     (`Views/IndexProfileFields.Edit.cshtml:6,14`, `Views/ContentIndexMetadata.Edit.cshtml:6,9,17,31`,
     `Views/Admin/Create.cshtml:16`). The notification reads "An index has been created successfully. The synchronizing
     process was triggered in the background." (`Controllers/AdminController.cs:257`).
  3. Check the starting state: from the **Actions** menu of the `blogposts` row choose **Query Index**. The table lists one
     document and has **no** column named `BlogPost.Subtitle` (the field is not included yet). If it has one, the
     Subtitle field already has Lucene settings: start again from a fresh site. If the table is empty ("Found 0
     result(s)"), the sample post has no entry in the task log: open the post from **Content, Content Items**, press
     **Publish** again without changing it (`CreateIndexingTaskContentHandler` records a task when an item is updated or
     published, and `IndexingContentHandler` indexes the post at the end of the request), then run the query again. The index must hold
     exactly one document before the snapshot is taken.
- Snapshot: sign the recorder in (`Recorder setup`), stop the site, copy `ORCHARD_APP_DATA` (it holds the Lucene index
  folder) and `state.json` aside. Restore both before every full take.

## Order of the clips

1. `04-settings` first: it saves the Lucene settings of the Subtitle field.
2. `04-reset` second: it needs the saved settings, and its Reset fills the Lucene index again.

The slide clips (`01-how`, `02-part`, `03-field`, `05-record`, `06-source`) need no site, no order, and no snapshot.

## Clips that change the site

- `04-settings` saves `LuceneContentIndexSettings` (Included, Keyword, Stored) on the Subtitle field of Blog Post.
- `04-reset` resets the `blogposts` index, which writes its documents again.

Restore the snapshot before a new take of either clip. To record `04-reset` alone, restore the snapshot, then do the
actions of `04-settings` by hand once (outside the capture).

## Clip `04-settings`

Starts on `/Admin/ContentTypes/List` (the **Content Types** page, breadcrumb "Content Types",
`OrchardCore.ContentTypes/Views/Admin/List.cshtml:6`), signed in, with the admin menu open on **Design**, **Content
Definition**, **Content Types** (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`) so the path in the narration is on screen.
The page is scrolled to the top. Capture starts when the page is still.

`settings-1`
- Highlight the **Content Types** entry of the menu (`HighlightAsync`, no caption), hold about 2 s, then
  `ClearHighlightsAsync()`.
- Click **Edit** on the **Blog Post** row: selector `a[role='btn-edit-BlogPost']`
  (`Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`), with `ClickAndWaitForNavigationAsync`. The page title is
  **Edit Content Type - Blog Post** (`Views/Admin/Edit.cshtml:63`).
- In the **Fields** list (`Views/Admin/Edit.cshtml:88`) click **Edit** on the **Subtitle** field: selector
  `a[href*='/ContentParts/BlogPost/Fields/Subtitle/Edit']` (`Views/Admin/Edit.cshtml:100`), with
  `ClickAndWaitForNavigationAsync`.
- Visible when the narration ends: the page titled **"Subtitle" settings** (`Views/Admin/EditField.cshtml:30`), at the
  top, with **Display Name** `Subtitle` and **Technical Name** `Subtitle`.

`settings-2`
- Scroll down (`ScrollAsync`) until the heading **Lucene Index settings** is on screen
  (`OrchardCore.Lucene/Views/LuceneContentIndexSettings.Edit.cshtml:5`).
- Click the checkbox labeled **Include this element in the index** (`GetByLabel("Include this element in the index")`,
  `LuceneContentIndexSettings.Edit.cshtml:13`). The **Keyword** and **Stored** boxes appear below it.
- Visible when the narration ends: the heading **Lucene Index settings**, **Include this element in the index** ticked,
  and **Keyword** and **Stored** shown below it, both unticked.

`settings-3`
- Click **Keyword** (hint "Check to index as a keyword value (indexed as a single token).",
  `LuceneContentIndexSettings.Edit.cshtml:19-20`), then **Stored** (hint "Check to be able to retrieve the original value
  from the index.", `:24-25`). Pause 700 ms after each.
- Hold until about 2 s of the narration remain, then scroll to the end of the form and click **Save**
  (`button.save`, `Views/Admin/EditField.cshtml:120`) with `ClickAndWaitForNavigationAsync`: the narration ends with
  "Save the field."
- Visible when the narration ends: the page **Edit Content Type - Blog Post** with the notification "The "Subtitle" field
  settings have been saved." (`OrchardCore.ContentTypes/Controllers/AdminController.cs:908`). The narration also says
  that Elasticsearch and Azure AI Search offer only the include option: nothing to show for it, the screen stays on the
  settings page until the click.

## Clip `04-reset`

Starts on `/Admin/indexing` (the **Indexes** page, **Search**, **Indexes**; `OrchardCore.Indexing/Controllers/AdminController.cs:60`,
breadcrumb "Indexes", `Views/Admin/Index.cshtml:12`), with the `blogposts` row listed and the page scrolled to the top.
The row shows the badges `Lucene` and `Content`.

`reset-1`
- Click the **Actions** button of the `blogposts` row. It is a button titled or labeled "Actions" in the row, whichever
  layout the admin settings give the list (`OrchardCore.Admin/Views/AdminListActions.cshtml:31`, drop-down button with the
  text "Actions"; `AdminListActions-Menu.cshtml:24`, ellipsis button with `aria-label="Actions"`): selector
  `button.actions` inside the row. The menu lists **Query Index**, **Synchronize**, **Reset**, **Rebuild** and **Force
  Delete** (`OrchardCore.Indexing/Views/IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml`,
  `OrchardCore.Lucene/Views/LuceneIndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:11`).
- Click **Reset** in the menu (`a.dropdown-item` whose text is exactly `Reset`,
  `IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:23`). A confirmation dialog titled **Reset Index** asks "This will
  restart the indexing of all content items. Continue?" (`:18-19`). Pause 1500 ms so it can be read, then click **Yes**
  (`:20`) with `WaitForNavigationAsync`.
- Visible when the narration ends: the **Indexes** page with the notification "An index has been reset successfully. The
  synchronizing process was triggered in the background." (`OrchardCore.Indexing/Controllers/AdminController.cs:420`).

`reset-2`
- First, off camera (`OffCameraAsync`): request `/Admin/Lucene/query/blogposts` with the signed-in context every 2 s until
  the HTML holds `BlogPost.Subtitle`, for 60 s at most. If it never appears, stop: the settings of `04-settings` were not
  saved, or the Reset did not run. The condition is specific to the new state, so the wait never happens on camera.
- Click **Actions** on the `blogposts` row, then **Query Index** (`LuceneIndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:11`)
  with `ClickAndWaitForNavigationAsync`. The page is **Lucene Query** (`OrchardCore.Lucene/Views/Admin/Query.cshtml:14`,
  route `Lucene/query/{indexName}`, `Controllers/AdminController.cs:70`), and it runs a match all query at once.
- Scroll down to the line "Found 1 result(s) in N ms" (`Query.cshtml:54`) and the table, then scroll the table sideways
  with `ScrollIntoViewIfNeeded()` on the header cell `th:text-is('BlogPost.Subtitle')` (`Query.cshtml:75`), and highlight
  that cell and the value under it, "Problems look mighty small from 150 miles up".
- Visible when the narration ends: the header cell **BlogPost.Subtitle** and its value on screen, highlighted, with the
  sentence "The results displayed are coming from the stored fields in Lucene." below the table (`Query.cshtml:97`).
  Clear the highlights before any scroll.

## Slide steps (nothing to record)

- `how-1`: four cards (Index profile, Task, Document, Index); the first is bright.
- `how-2`: the same cards; Task, Document and Index are bright.
- `how-3`: the same cards; Task and Document are bright.
- `part-1`: the code of `HtmlBodyPartIndexHandler`.
- `part-2`: two cards, the keys of a part (`HtmlBodyPart`, `Article.HtmlBodyPart`).
- `part-3`: the code of `LuceneContentIndexSettings.ToOptions()` above the options line of `AliasPartIndexHandler`.
- `part-4`: the registration of `HtmlBodyPartIndexHandler`, and the check of `ContentItemIndexCoordinator` that skips a part.
- `field-1`: the code of `TextFieldIndexHandler`.
- `field-2`: the code of `NumericFieldIndexHandler`, with the lines that read the field settings highlighted.
- `field-3`: the registration of `TextFieldIndexHandler`, and the check that a field needs its own Include setting.
- `record-1`: the `IDocumentIndexHandler` interface above `ContentItemIndexCoordinator`.
- `record-2`: the `ProductIndexHandler` example, and its registration.
- `source-1`: the `AddLuceneIndexingSource` call, with three cards (Provider, Type, Add index).
- `source-2`: the `AddIndexingSource` call, with a card for each of the three services.
- `source-3`: the `CreateTaskAsync` call, with the limits of the record id and the category.
- `source-4`: the `ProductIndexingService` class that derives from `NamedIndexingService`.
- `source-5`: the `ProductIndexProfileHandler` class (both overrides check the type of the index profile first) and the `ProductIndexingBackgroundTask` class.
- `source-6`: the three registrations of `ProductIndexingService`, `ProductIndexProfileHandler` and `ProductIndexingBackgroundTask`, above three cards.
- `source-7`: the lines of `LuceneContentIndexProfileHandler` that set the mappings, above the `UpdateDocument` call of `LuceneIndexManager` that reads the key field.

## Checks for private data

- The site is fresh: the only people are `admin` and `admin@example.com`. The Query page lists the owner id and the author
  `admin` of the sample post in stored columns; both are generated by the setup, not real.
- OCR the video at one frame per second and at every scene change, and search for email addresses other than
  `@example.com`, for tokens, and for any text of the machine (user names, folders, host names in the address bar).
- The password is never typed on camera: the sign-in comes from `state.json`.
- The `Product` classes and `Products` names on the slides are invented.

# Recording plan: media-indexing

Read this file from top to bottom and nothing else is needed to record the video. Paths such as
`src/OrchardCore.Modules/...` are paths in the Orchard Core source the content was written against (commit 92db4a83);
`file:line` says where a label comes from. A selector in this plan stands for `scene.Page.Locator(...)`. The selectors
were written from the views and scripts of that source, not from a running site: look at each one once on the running
site, and correct it in `Scenes.cs` rather than in the narration (OPEN-QUESTIONS.md lists the ones most likely to need it).

The video is an admin video: it shows the admin UI and the front end of the site, never code.

## The site it needs

- A local Orchard Core site (`src/OrchardCore.Cms.Web`), on `http://localhost:5010`, with its own `App_Data` folder
  outside both repositories (`ORCHARD_APP_DATA`), provisioned by Auto Setup so no setup or sign-in form is recorded.
- Auto Setup values: recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email
  `admin@example.com`, password `<password>` (chosen outside git and passed to the recorder in `VIDEO_PASSWORD`).
  This is the Default tenant; no other tenant is used.
- `HTTP_PROXY` / `HTTPS_PROXY` set to a dead address, so the site calls no outside service. This video needs no outside
  service and no local stand-in.
- Sign in once with the recorder (`Recorder setup`) and keep `state.json` out of git.

### Features to enable before recording

The Blog recipe enables the Media, Contents, Content Types, Features and Indexing features. In addition, enable these
two from Tools, Features, before the snapshot:

- **Lucene** (`OrchardCore.Lucene`), which also gives the "Content in Lucene" index source.
- **Search** (`OrchardCore.Search`), which gives the front-end search page and Settings, Search, Site Search.

Do **not** enable PDF Media Indexing, Text Media Indexing, OpenXML Media Indexing or Media Indexing before recording:
clip `02-enable` enables them.

### The fake file to upload (outside the capture)

Create a one-page PDF named `garden-planting-guide.pdf` with any word processor (Save as PDF or Export as PDF, so the
text stays real text and is not a picture of text). Set the document author to `Example Author` or leave it empty. The
file is not committed (a video folder holds no binary). The page holds exactly this text:

```
Autumn Planting Notes

Plant garlic cloves in October, pointed end up, about six inches apart.
Cover the bed with straw to keep the soil warm through the winter.
A young persimmon tree needs a sunny spot and shelter from the wind.
Water planted trees once a week until the first frost.
```

The word **persimmon** must appear in this file and nowhere else on the site (not in a title, a body, a tag or any
Blog recipe content). Open the PDF in a viewer and select a word of the text to check that it is real text.

Upload it to the root folder of the Media Library: Media, Library (`/Admin/Media`), then Upload files (the file input
is `#fileupload`; with Playwright, `SetInputFilesAsync`). No other file is uploaded.

### The index and the search settings (outside the capture)

1. Search, Indexes (`/Admin/indexing`): select Add index. In the Available Sources window, in the Lucene group, the card
   **Content in Lucene** has an Add button: select it.
2. Index Name: `Articles`. The Name field fills in by itself with `articles`: replace it with `Articles`, so that the
   index is called **Articles** in every list.
3. Content types: choose only **Article**. Leave every other setting as it is. Select Save and wait for the message
   "An index has been created successfully. The synchronizing process was triggered in the background."
4. Settle the starting ticks of the Query field names, so that nothing is decided during a take: select Edit on the
   Articles index and look at the list **Query field names** under Default Query Settings. If
   `Content.ContentItem.FullText` is not ticked, tick it. Leave every other name unticked. Select Save and wait for the
   message "An index has been updated successfully." The Documents names are not in the list yet: the field does not exist.
5. Settings, Search, Site Search (`/Admin/Settings/search`): Default search index = **Articles**; leave Placeholder empty;
   Save.
6. Check the starting state: the Article content type has no field called Documents (Design, Content Definition, Content
   Types, Article); `http://localhost:5010/search?Terms=persimmon` shows "There are no such results."

### Snapshots

After step 6, stop the site and copy its `App_Data` folder and `state.json` aside as **snapshot-0**. Every clip below
changes the site, so a snapshot is also taken after each clip (snapshot-1 after `02-enable`, and so on). Before a new
take of clip N, stop the site, restore snapshot N-1, and start the site again.

## Order and dependencies

Record in this order, which is also the order they play in:

| Clip | Starts from | What it changes |
|---|---|---|
| `02-enable` | snapshot-0 | Enables PDF, Text and OpenXML Media Indexing, and Media Indexing with them. |
| `03-field` | snapshot-1 | Adds the field Documents (Media Field) to the Article type, with "Include this element in the index" selected. |
| `04-publish` | snapshot-2 | Creates and publishes the article Autumn Garden Guide with the PDF attached. |
| `05-index` | snapshot-3 | Resets the Articles index and saves its Query field names. |
| `06-search` | snapshot-4 | Nothing: it only reads and searches. |

Each clip depends on the one before: the field names in `05-index` exist only after `02-enable` (the Media Indexing
feature registers the handler that adds them) and `03-field`; the search in `06-search` finds the article only after
`04-publish` and `05-index`. The two slide chapters (`01-how`, `07-notes`) need no site.

## Common to every recording clip

- Start page: the page named in the clip, opened with `scene.OpenAsync` before `scene.StartCaptureAsync()`.
- The admin menu is collapsed. When the narration names a menu path, click through it in the order it is spoken
  (the menu items are links with the visible text given below). If a menu item cannot be reached in the collapsed menu,
  call `scene.NavigateAsync` with the URL given for that page at the moment the narration names the path.
- After a click that loads a page, use `ClickAndWaitForNavigationAsync`. Clear highlights before scrolling.
- Wait conditions are named in each step. Cut a wait out of the video with `scene.OffCameraAsync` only where the step says so.

## Clips

### Chapter 01, slides (`01-how`)

- `why-1`: slide "What a search index holds": two cards, the text of content items (indexed) and the text of attached files (not).
- `why-2`: slide "Three features read the files": three cards, PDF Media Indexing, Text Media Indexing, OpenXML Media Indexing, each with its file extensions.
- `why-3`: slide "What has to be in place": four cards, a Media Field, the field included in the index, an index of the content type, and what is not indexed.

### Chapter 02, `02-enable` (starts on `/Admin`, any admin page)

- `enable-1`
  - Click the menu items Tools, Features (`/Admin/Features`) with `ClickAndWaitForNavigationAsync`.
  - Click the search box `#search-box` and type `media indexing`.
  - Wait until `#list-summary` reads "Showing 3 of" and the Search category group (`.feature-group:not(.d-none) h3`) is the only group left.
  - Highlight the group heading (`.feature-group:not(.d-none) h3`) with the caption `Search`.
  - On screen when the narration ends: the Search category with three rows, OpenXML Media Indexing, PDF Media Indexing and Text Media Indexing, each Disabled, each with the dependency badge Media Indexing. There is no row for Media Indexing itself: the Visibility filter "Hide on demand" is selected by default, and the feature is enabled together with the features that depend on it.
  - Labels: `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features); `.../Views/Admin/Features.cshtml:27` (search box), `:65` (Hide on demand), `:88` (category heading); feature names `src/OrchardCore.Modules/OrchardCore.Media.Indexing.Pdf/Manifest.cs:4`, `src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs:35`, `src/OrchardCore.Modules/OrchardCore.Media.Indexing.OpenXML/Manifest.cs:4`; category "Search" in the same manifests; filter text includes the name `.../Drivers/FeatureEntryDisplayDriver.cs:20`.
- `enable-2`
  - Highlight one dependency badge, `.direct-dependencies .badge:has-text("Media Indexing")` (first match), with the caption `Media Indexing`; clear it.
  - Click the group checkbox `.list-group-select-all` (its label reads "Select all 3 features grouped under Search category"). It ticks the three visible rows. Check that `input[name='featureIds'][value='OrchardCore.Media.Indexing.Pdf']`, `...Text` and `...OpenXML` are ticked.
  - Click `#bulk-action-menu-button` (Bulk Actions), then `a[data-action='Enable']` (Enable) with `ClickAndWaitForNavigationAsync`.
  - On screen when the narration ends: the Features page reloaded, with the success message "The following features were enabled: ..." at the top and an empty search box.
  - Labels: `.../Views/Admin/Features.cshtml:34` (Bulk Actions), `:38` (Enable); `.../Views/FeaturesGroupToolbar.cshtml:16-17` (select all); `.../Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:16` (dependency badges); message `.../Controllers/AdminController.cs:317`; Media Indexing is enabled on demand: `src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs:22,30`.
- `enable-3`
  - Click `#search-box` and type `media indexing` again (the reload cleared the filter).
  - Wait until `#list-summary` reads "Showing 3 of". Highlight the three status badges (`.badge:has-text("Enabled")`), then clear.
  - On screen when the narration ends: the three rows, each with the status Enabled (a green check).
  - Labels: `.../Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` (Enabled).

### Chapter 03, `03-field` (starts on `/Admin`, any admin page)

- `field-1`
  - Click the menu items Design, Content Definition, Content Types (page `/Admin/ContentTypes/List`), then click the Edit button of the Article row, `a[role='btn-edit-Article']`, with `ClickAndWaitForNavigationAsync`.
  - On screen when the narration ends: "Edit Content Type - Article" with the Fields section (Subtitle, Banner Image) and the Add Field button visible.
  - Labels: `src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25` (Design, Content Definition, Content Types); `.../Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24` (Edit); `.../Views/Admin/Edit.cshtml:63` (page title), `:137` (Add Field).
- `field-2`
  - Click Add Field (`a:has-text("Add Field")`) with `ClickAndWaitForNavigationAsync`; the page is "Add New Field To "Article"".
  - Type `Documents` in Display Name (`input[data-displayname]`) and press Tab: the Technical Name (`input[data-name]`) fills in as `Documents`. If it is still empty after Tab, type `Documents` in it.
  - Click the radio label **Media Field** in the Field Type list (`label:has-text("Media Field")`; the list is long, so let the click scroll it into view).
  - Click **Save & Configure** (`button:has-text("Save & Configure")`) with `ClickAndWaitForNavigationAsync`.
  - On screen when the narration ends: the page `"Documents" settings`, with the message `The field "Documents" has been added.`
  - Labels: `.../Views/Admin/AddFieldTo.cshtml:20` (title), `:38` (Display Name), `:46` (Technical Name), `:54` (Field Type), `:62` (the type name "MediaField" is shown as "Media Field" by CamelFriendly), `:72` (Save & Configure); `.../Controllers/AdminController.cs:768` (message); `.../Views/Admin/EditField.cshtml:30` (settings title).
- `field-3`
  - Scroll to the heading "Lucene Index settings" (`h4:has-text("Index settings")`).
  - Click the checkbox `input[id$='LuceneContentIndexSettings_Included']` (label "Include this element in the index"). Wait until `#lucene-index-settings.show` is visible: Keyword and Stored appear, both unticked.
  - Highlight the Keyword label (`label:has-text("Keyword")`) with the caption `Leave cleared`; clear it. Do not tick Keyword or Stored.
  - On screen when the narration ends: the Lucene Index settings section with "Include this element in the index" ticked, and Keyword and Stored cleared, with the hint "Check to index as a keyword value (indexed as a single token)."
  - Labels: `src/OrchardCore.Modules/OrchardCore.Lucene/Views/LuceneContentIndexSettings.Edit.cshtml:5` (heading), `:13` (Include this element in the index), `:19-20` (Keyword and its hint), `:24` (Stored). The section is shown to users who have the Manage Lucene indexes permission; the admin has it by default (`.../OrchardCore.Lucene/Permissions.cs`).
- `field-4`
  - Scroll to the checkbox **Allow media text** (`label:has-text("Allow media text")`) and highlight it with the caption `Allow media text`. Do not change it (it stays cleared, so the editor of an article shows no Media text button). Clear the highlight.
  - Scroll to the bottom and click **Save** (`button.save`) with `ClickAndWaitForNavigationAsync`.
  - On screen when the narration ends: "Edit Content Type - Article" with the message `The "Documents" field settings have been saved.` and the field Documents in the Fields list.
  - Labels: `src/OrchardCore.Modules/OrchardCore.Media/Views/MediaFieldSettings.Edit.cshtml:25` (Allow media text); the MediaText name is added only when it is on: `src/OrchardCore.Modules/OrchardCore.Media/Indexing/MediaFieldIndexHandler.cs:12,45`; Save and message `src/OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs:908`.

### Chapter 04, `04-publish` (starts on `/Admin`, any admin page)

- `publish-1`
  - Click the menu items Content, Content Items (`/Admin/Contents/ContentItems`). Click the **New** button (`#new-dropdown`), then **Article** (`a.dropdown-item:has-text("Article")`) with `ClickAndWaitForNavigationAsync`.
  - Click the Title field (`input[id$='TitlePart_Title']`) and type `Autumn Garden Guide`. Leave Subtitle, Banner Image and the body empty.
  - On screen when the narration ends: the page "New Article" with the Title filled in.
  - Labels: `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65,68` (Content, Content Items); `.../Views/ContentsAdminListCreate.cshtml:19` (New), `:25` (the type's display name); `.../Views/Admin/Create.cshtml:26` (New Article); `src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12` (Title).
- `publish-2`
  - Scroll to the Documents field (`.field-wrapper-article-documents`). Click its **Add media** button (`.field-wrapper-article-documents button:has-text("Add media")`).
  - Wait, with `scene.OffCameraAsync`, until the window titled **Select Media** (`.mf-picker-shell`) has finished loading (it first shows "Loading media browser...") and lists `garden-planting-guide.pdf`. Tick the checkbox in its row (`.file-item:has-text("garden-planting-guide.pdf") input[name='select-file']`); the footer reads "1 selected".
  - Click **OK** (`.mf-picker-shell button:has-text("OK")`). The window closes.
  - On screen when the narration ends: the Documents field shows the attached PDF (file name `garden-planting-guide.pdf`) and the Add media button.
  - Labels: `src/OrchardCore.Modules/OrchardCore.Media/Services/MediaJSLocalizer.cs:119` (Add media), `:137` (Select Media), `:126` (OK); `.../Assets/media-picker/src/components/MediaPickerModal.vue:18,39,49`; `.../Assets/media-gallery/src/components/FileItemsComponent.vue:35` (row checkbox); `.../Views/MediaField.Edit.cshtml:34` (the field's CSS class).
- `publish-3`
  - Scroll to the bottom and click **Publish** (`button.publish`) with `ClickAndWaitForNavigationAsync`.
  - Wait until the Content Items list has loaded. The article is added to the Articles index when it is published, in a task that runs at the end of the request.
  - On screen when the narration ends: the Content Items list with the message "Your Article has been published." and Autumn Garden Guide at the top.
  - Labels: `src/OrchardCore.Modules/OrchardCore.Contents/Views/Content.PublishButton.cshtml:7` (Publish); message `.../Controllers/AdminController.cs:473`.

### Chapter 05, `05-index` (starts on `/Admin`, any admin page)

- `index-1`
  - Click the menu items Search, Indexes (`/Admin/indexing`). The list shows the index Articles.
  - In the Articles row (`[data-filter-value='articles']`) click the Actions button (`button.actions`: its label is "Actions"; in the Menu layout it is the ellipsis button).
  - Highlight the **Reset** item (`a.dropdown-item:has-text("Reset")`, exact text Reset) with the caption `Reset`, then click it. A confirmation window appears.
  - On screen when the narration ends: the window titled **Reset Index** with the message "This will restart the indexing of all content items. Continue?" and the buttons Yes and No.
  - Labels: `src/OrchardCore.Modules/OrchardCore.Indexing/AdminMenu.cs:19,22` (Search, Indexes); `.../Views/Admin/Index.cshtml:12` (title); `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml:32` (Actions); `.../OrchardCore.Indexing/Views/IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:18-23` (Reset Index, message, Yes, Reset); row filter `.../OrchardCore.Indexing/Drivers/IndexProfileDisplayDriver.cs:36`. Reset also saves the index, which refreshes its field names: `.../Controllers/AdminController.cs:416-418` and `src/OrchardCore/OrchardCore.Lucene.Core/Handlers/LuceneContentIndexProfileHandler.cs:40,116`.
- `index-2`
  - Click **Yes** (`.modal.show button:has-text("Yes")`). Wait, with `scene.OffCameraAsync`, until the text "An index has been reset successfully." is on the page.
  - Click **Edit** in the Articles row (`[data-filter-value='articles'] a:has-text("Edit")`; in the Menu layout, open the row's Actions menu first) with `ClickAndWaitForNavigationAsync`.
  - On screen when the narration ends: the page "Edit 'Articles' index".
  - Labels: `.../OrchardCore.Indexing/Controllers/AdminController.cs:420` (message); `.../Views/IndexProfile.Buttons.SummaryAdmin.cshtml:8` (Edit); `.../Views/Admin/Edit.cshtml:6` (page title).
- `index-3`
  - Scroll to the heading **Default Query Settings** (`h4:has-text("Default Query Settings")`) so that the whole **Query field names** list (`#DefaultQueryFields`) is in view; scroll the page again if the list is long.
  - Highlight the two entries `Article.Documents.MediaText` and `Article.Documents.FileText` (`#DefaultQueryFields label:has-text("Article.Documents.MediaText")` and `...FileText`) with no caption; clear the highlights after the step's narration is half done.
  - On screen when the narration ends: the Query field names list with both names visible, and the hint "The fields to use for query. Leave it blank to query all fields."
  - Labels: `src/OrchardCore.Modules/OrchardCore.Lucene/Views/LuceneQuerySettings.Edit.cshtml:7` (Default Query Settings), `:37-38` (Query field names), `:61` (hint); the names are `{content type}.{field name}` plus `.MediaText` or `.FileText`: `src/OrchardCore.Modules/OrchardCore.Media/Indexing/MediaFieldIndexHandler.cs:12-13` and `src/OrchardCore/OrchardCore.Indexing.Abstractions/ContentFieldIndexHandler.cs`; the list comes from the index's field names `.../OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs:81`.
- `index-4`
  - Tick the checkboxes of `Article.Documents.MediaText` and `Article.Documents.FileText` (the `input[type='checkbox']` inside each label), unless they are already ticked. Leave every other entry as it is: the full-text name was ticked during the seed (step 4 of "The index and the search settings"); see OPEN-QUESTIONS.md, question 3. The narration says that the search covers only the selected names, or every field when none is selected: that is what the code does, and it is why the seed ticks one name.
  - Scroll to the bottom and click **Save** (`.action-bar-bottom button:has-text("Save")`) with `ClickAndWaitForNavigationAsync`.
  - On screen when the narration ends: the Indexes list with the message "An index has been updated successfully."
  - Labels: `.../OrchardCore.Indexing/Controllers/AdminController.cs:336` (message); `.../Views/Admin/Edit.cshtml:16` (Save); the ticked names become the default search fields: `.../OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs:125`; with none ticked the search covers every field of the index: `src/OrchardCore/OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:57-60`.

### Chapter 06, `06-search` (starts on `/Admin`, any admin page)

- `search-1`
  - Click the menu items Settings, Search, Site Search (`/Admin/Settings/search`).
  - Highlight the **Default search index** drop-down (`select[id$='DefaultIndexProfileName']`), which reads Articles, with the caption `Articles`. Do not select a different value and do not click Save.
  - On screen when the narration ends: the Site Search settings with Default search index = Articles and an empty Placeholder.
  - Labels: `src/OrchardCore.Modules/OrchardCore.Search/AdminMenu.cs:25-27` (Settings, Search, Site Search); `.../Views/SearchSettings.Edit.cshtml:6` (Default search index), `:17` (Placeholder).
- `search-2`
  - `scene.NavigateAsync("/search")`: the front end, with the heading Search, the box with the placeholder "Enter your search term(s)" and the Search button.
  - Type `persimmon` in the box (`input[name='Terms']`), then click **Search** (`button.btn-primary:has-text("Search")`) with `ClickAndWaitForNavigationAsync`.
  - On screen when the narration ends: the results page for the term persimmon (the address ends with `?Terms=persimmon`).
  - Labels: `src/OrchardCore.Themes/TheBlogTheme/Views/Search-Form.liquid:16-17` (box and Search button), the placeholder text on the same file at line 9; route `src/OrchardCore.Modules/OrchardCore.Search/Controllers/SearchController.cs:64`.
- `search-3`
  - Highlight the first result (`.list-group-item`), which shows Autumn Garden Guide as a heading that is a link. Do not scroll.
  - Before the take, run the same search outside the capture: if the page shows "There are no such results.", reload it until the article is listed (see OPEN-QUESTIONS.md, question 5); do not record while the list is empty.
  - On screen when the narration ends: one result, Autumn Garden Guide as a linked heading, and no "There are no such results." message. A result is drawn with the Summary display of the content item, which for an Article is the default `Content.Summary` template with the title as a link (`src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Summary.cshtml:3`, `src/OrchardCore.Modules/OrchardCore.Contents/Views/Content.Summary.cshtml`); the theme template `Content-Article.liquid` belongs to the Detail display and does not apply (`src/OrchardCore/OrchardCore.ContentManagement.Display/ContentItemDisplayManager.cs:62-73`).
  - Labels: `src/OrchardCore.Themes/TheBlogTheme/Views/Search-Results.liquid:23` (the empty message that must not show); the search covers the ticked Query field names: `src/OrchardCore/OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:59`.

### Chapter 07, slides (`07-notes`)

- `notes-1`: slide "Good to know": two cards, the first bright: other formats (the doc format and Excel workbooks) are not read.
- `notes-2`: slide "Good to know": the same two cards, the second bright: Text and Markdown files need the file types to be allowed in the Media configuration.

## Checks for private data

- The only account is `admin` / `admin@example.com`; the password is never typed on camera (the session comes from `state.json`).
- No real name, email, phone number or address appears: the only fake content is Autumn Garden Guide, `garden-planting-guide.pdf` and the Blog recipe content.
- The PDF is not opened on camera; its author is `Example Author` or empty.
- OCR the finished recording at one frame per second and at every scene change, and search for anything that is not example.com or the fake names above.
- Do not commit `state.json`, the PDF, the snapshots or any copy of `App_Data`.

# Open questions: media-indexing

Everything in the narration was read from the Orchard Core source (commit 92db4a83) and its documentation. The points
below could not be confirmed without a running site. A person who records the video confirms each one and, where the
answer differs, changes the narration or recording-plan.md. Paths are relative to the Orchard Core source.

1. **Selectors and menus.** The selectors in recording-plan.md come from the views and scripts, not from a running site.
   Check in particular: the collapsed admin menu (Design, Content Definition, Content Types; Content, Content Items;
   Search, Indexes; Settings, Search, Site Search); the Actions button and the Reset and Edit items of an index row, which
   depend on the list layout (Buttons or Menu, `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml`
   and `AdminListActions-Menu.cshtml`); the confirmation window of Reset (its Yes button); the group checkbox of the
   Features list (`src/OrchardCore.Modules/OrchardCore.Features/Views/FeaturesGroupToolbar.cshtml`); the Media Library
   window and its row checkbox (`src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/components/FileItemsComponent.vue`).

2. **Do the two field names appear after Reset?** The Query field names list is built from the field names stored in the
   index (`src/OrchardCore.Modules/OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs:81`). Those are computed only
   when an index is created or saved, from an empty item of each indexed content type
   (`src/OrchardCore/OrchardCore.Lucene.Core/Handlers/LuceneContentIndexProfileHandler.cs:40,116`). The Reset action saves the
   index after resetting it (`src/OrchardCore.Modules/OrchardCore.Indexing/Controllers/AdminController.cs:416-418`), so
   the names `Article.Documents.MediaText` and `Article.Documents.FileText` should be listed when the index is opened
   afterwards. The narration of `index-1` says that Reset saves the index. If the names are missing, save the Articles index
   once with Save, open it again, and change `index-1` and `index-2` to match.

3. **The starting ticks of Query field names.** When no name of the list is ticked, the search covers every field of the
   index; when some are ticked, it covers only those (`src/OrchardCore/OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:59`
   and the lines above it). The seed therefore ticks `Content.ContentItem.FullText` and nothing else before recording
   (step 4 of "The index and the search settings" in recording-plan.md), so that ticking the two Documents names in
   `index-4` keeps the title and body of articles searchable and "leave the other fields as they are" is true. Confirm on
   the running site that the list shows `Content.ContentItem.FullText` before the first take, and that it is still ticked
   after `05-index`. The narration of `index-4` says that the search covers only the selected names, or every field when
   none is selected; a new index starts with none selected (the default `FullText` assignment in
   `src/OrchardCore/OrchardCore.Lucene.Core/Handlers/LuceneContentIndexProfileHandler.cs:104-107` is made on an object that is
   never saved), so confirm that too: search `persimmon` once with no name ticked, outside the capture.

4. **Spoken spellings.** The storyboard proposes "Open X.M.L." for OpenXML, "File Text" for FileText and "Media Text" for
   MediaText. A person must confirm each with `scripts/pronounce.py` before narrating, and also listen to Lucene, Markdown,
   persimmon and "Save & Configure" (the voice should say "and").

5. **Is the article found at once?** Publishing adds the article to the index in a task that runs at the end of the
   request (`src/OrchardCore/OrchardCore.Indexing.Core/Handlers/IndexingContentHandler.cs:28,99`), and Reset starts a background
   synchronization (`.../OrchardCore.Indexing/Controllers/AdminController.cs:418`). Confirm that the search in `search-3`
   lists the article without a long wait; if a wait is needed, run the search before the take and record when it works.
   The Indexing Worker feature is not enabled in this plan.

6. **Reset and the existing article.** Reset sets the index to process every indexing task again
   (`src/OrchardCore/OrchardCore.Indexing.Core/Handlers/ContentIndexProfileHandler.cs:34`). Confirm that the article
   published in `04-publish` is still found after the Reset of `05-index`.

7. **The Media Indexing row.** The base Media Indexing feature is enabled only through the other three and is hidden by
   the "Hide on demand" filter (`src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs:22,30`,
   `.../OrchardCore.Features/Views/Admin/Features.cshtml:65`). Confirm that the dependency badge reads "Media Indexing" and
   that the success message after Enable names the features enabled.

8. **Text and Markdown files.** The default list of file extensions the Media Library accepts has no text or Markdown
   extension (`src/OrchardCore.Modules/OrchardCore.Media/Services/MediaOptionsConfiguration.cs:12-50`), and the Media
   documentation says that extensions in neither list are rejected (`src/docs/reference/modules/Media/README.md:465`).
   `notes-2` says that the site must allow them. Confirm on a running site that uploading a text file is refused by
   default. The video does not show a text file.

9. **Visitors.** The search page checks the "Query" permission of the index for the current user
   (`src/OrchardCore.Modules/OrchardCore.Search/Controllers/SearchController.cs:110`). The video searches as the
   signed-in administrator and says nothing about visitors. Confirm what an anonymous visitor sees if the maintainers
   want that stated.

10. **Not narrated, not verified.** Scanned PDFs that hold only pictures of text, protected or damaged files, and very
    large files were not checked (the PDF reader is `src/OrchardCore.Modules/OrchardCore.Media.Indexing.Pdf/Services/PdfMediaFileTextProvider.cs`).
    The same Include checkbox exists for Elasticsearch and Azure AI Search, but the video shows Lucene only. The Stored
    option of the Lucene settings is not needed for searching and is not mentioned.

11. **Allow media text.** The video points at the Allow media text checkbox and says that it is selected on a new field and that,
    when it is selected, the descriptions are indexed as MediaText (`src/OrchardCore.Modules/OrchardCore.Media/Indexing/MediaFieldIndexHandler.cs:45`).
    The default comes from `MediaFieldSettings.AllowMediaText = true` (`src/OrchardCore/OrchardCore.Media.Core/Settings/MediaFieldSettings.cs`)
    and from a field with no stored settings being read as a new settings object (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentDefinition.cs:16-21`).
    The recording does not change the checkbox. Confirm on the running site that it is ticked right after Save & Configure, and
    that a description typed in the Media text window of the field editor is found by a search once the MediaText name is ticked.

12. **Is the PDF text read as separate words?** The PDF reader concatenates the letters of each page
    (`src/OrchardCore.Modules/OrchardCore.Media.Indexing.Pdf/Services/PdfMediaFileTextProvider.cs`, the loop over
    `document.GetPages()`), so the words are separate only if the PDF holds its spaces as text. Confirm with the real file
    that a search for `persimmon` finds the article before recording `06-search`; if it does not, export the PDF again
    from another word processor.

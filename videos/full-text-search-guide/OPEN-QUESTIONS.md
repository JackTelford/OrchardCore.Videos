# Open questions: Implement Full-Text Search

Paths are under `src/` of the Orchard Core commit the programme works from. Everything in the narration was read from
that source. The list below holds what only a running site can confirm, with what was checked in the source.

- **Query field names on the create form (`index-5`).** Checked: on a new index the list holds three fields,
  `ContentItemId`, `ContentItemVersionId` and `Content.ContentItem.FullText`, and only the last is selected
  (`OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs:70-85`, the `Selected` expression with `context.IsNew`;
  the fields come from `OrchardCore.Lucene.Core/Handlers/LuceneContentIndexProfileHandler.cs:116-124`). The fields of
  the content types are added when the index is saved, so the list does not grow when **Blog Post** is checked. Confirm
  on screen that these three rows show and that only `Content.ContentItem.FullText` is checked. If not, fix the checks by
  hand and change the sentence.
- **Name filled from Index Name (`index-2`).** Checked: `OrchardCore.Indexing/Assets/ts/index-profile-fields.ts` calls
  `TechnicalNameGenerator.initialize` for a new index, and `Assets/js/technical-name-generator.js` writes the
  lower-case, hyphenated form of Index Name into Name on every `input` event while Name has not been edited, so `Posts`
  gives `posts`. Confirm on screen that Name reads `posts` after the typing. If not, type `posts` into Name too.
- **Order of the Content types and Analyzer blocks (`index-3`, `index-4`).** Checked: both are placed at `Content:5`
  (`OrchardCore.Indexing/Drivers/ContentIndexProfileDisplayDriver.cs:49` and `OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs:55`),
  so the order depends on the order of the drivers, which the source does not settle. Confirm that **Content types**
  comes before **Analyzer**. If the order is reversed, scroll up from `index-3` to `index-4`, or swap the two steps.
- **Both permission rows (`permission-2`).** Checked: the role editor lists "Query 'posts' Index" (from
  `OrchardCore/OrchardCore.Indexing.Core/IndexingPermissions.cs:15`, built from the Name of the index, provided by
  `OrchardCore.Indexing/IndexingPermissionsProvider.cs`) and "Query Lucene Posts Index" (from
  `OrchardCore.Lucene/LuceneIndexPermissionHelper.cs:16` and `Permissions.cs:27`, built from the Index Name).
  `SearchController.cs:110` authorizes `QuerySearchIndex` ("Query any index") against the index, and
  `IndexingAuthorizationHandler.cs:34-38` accepts the first permission for it. A search of the source finds no check of
  the second one except a data migration. Confirm in a private window (signed out): with only the first checked,
  `/search` lists posts; with it cleared, `/search` asks for a sign-in. The groups the rows sit under are named after the
  feature of their provider (`OrchardCore.Roles/Controllers/AdminController.cs:412-427`), so **Indexing** and **Lucene**
  are expected; confirm if a take shows them.
- **The effect of the permission cannot be recorded (`permission-3`).** The recorder is signed in as an administrator,
  who holds every permission, so the clip shows no before and after. If a signed-out recording is possible, a `06-try`
  variant in a context without `state.json` would show it.
- **Search results (`try-2`, `try-3`, `rebuild-3`).** Checked: the full-text value is built from the display text, the
  body and the custom template only (`OrchardCore.Contents/Handlers/FullTextAspectContentHandler.cs:49-75`), the index
  uses `standardanalyzer`, and the search page queries `Content.ContentItem.FullText` only
  (`OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:47-71`). Confirm the table of `recording-plan.md`: `apple`
  lists posts A and B, `grafting` lists nothing before the rebuild and post A after it.
- **Items keep their old value until saved or rebuilt (`type-3`).** Checked: indexing runs when a content item is
  created, updated, published, unpublished, saved as a draft or removed
  (`OrchardCore.Indexing.Core/Handlers/IndexingContentHandler.cs:28-44`), and nothing re-indexes after a content type is
  saved. Confirm: after `type-3` and before `08-rebuild`, `/search` for `grafting` still finds nothing.
- **The Liquid editor (`type-2`).** Checked: `OrchardCore.Contents/Assets/ts/full-text-aspect-settings.ts` turns the
  `FullTextTemplate` text area into a CodeMirror editor (`CodeMirror.fromTextArea` with `lineNumbers`, `styleActiveLine`
  and `matchBrackets`, no addon that closes brackets), and `FullTextAspectSettings.Edit.cshtml:46,53` expands
  `#fulltext-template` when the check box is selected. Confirm that `#fulltext-template .CodeMirror` takes the typing and
  that the editor holds exactly `{{ Model.Content.BlogPost.Subtitle.Text }}`.
- **Feature buttons (`features-1` to `features-3`).** Checked: the Enable link has the id `btn-enable-` plus
  `Html.GenerateIdFromName(<feature id>)` (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`), which
  the plan reads as `OrchardCore_Lucene` and `OrchardCore_Search`. Searching `Lucene` also lists "Lucene Worker",
  "Lucene Content Picker" and "Lucene (Obsolete)". Confirm that the click goes to the row coded `OrchardCore.Lucene`, and
  whether the message reads "The feature Lucene was enabled." or lists dependencies too.
- **Synchronizing time (`index-6`, `rebuild-2`).** Checked: the index is filled after the request ends
  (`HttpBackgroundJob.ExecuteAfterEndOfRequestAsync` in `DefaultIndexProfileManager.SynchronizeAsync`), and the handler
  processes the indexing tasks at once (`ContentIndexProfileHandler.SynchronizedAsync`), without waiting for a worker.
  Measure how long `/search/posts?Terms=apple` takes to list the posts, and adjust the off-camera wait of `rebuild-2`.
- **Row actions layout (`rebuild-1`).** Checked: the defaults are the `List` layout and the `Buttons` actions layout
  (`OrchardCore.Admin.Abstractions/AdminListOptions.cs`), so each row has Edit and Delete buttons and an **Actions**
  drop-down. If the site's admin settings choose the `Menu` layout, the actions sit behind an ellipsis button, and the
  narration "the Actions menu" still fits.
- **Seed form labels.** Checked: the Blog recipe gives Blog Post a title part, a Subtitle text field and a Markdown body
  with the Wysiwyg editor. The button that lists the items of the Blog is labeled from the technical name of the
  contained type (`OrchardCore.Lists/Views/ListPartSummaryAdmin.cshtml:10`), so it most likely reads **List BlogPost
  Items**. Not checked on screen: the labels of the Title, Subtitle and Markdown fields. The seed is created outside the
  capture.
- **Pronunciation (storyboard `spoken`).** Check each entry with `scripts/pronounce.py` before narrating:
  `Content.ContentItem.FullText` ("Content dot Content Item dot Full Text"), `/search` ("slash search"), `full_text`
  ("full text"), `standardanalyzer`, `Search-List`, `Search-Form`, `Search-Results`, `SearchSummary`,
  `CreateOrUpdateIndexProfile`, `RebuildIndex`, `TheBlogTheme` and `Blog - Lucene Search`. Also listen to "Lucene",
  "Liquid", "Startup" and "Grafting".

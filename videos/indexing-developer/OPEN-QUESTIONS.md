# Open questions: Indexing in Code

What was checked in the Orchard Core source, and what a person must confirm on a running site before this video is
recorded. None of the unconfirmed points is stated as a fact in the narration.

## Checked in the source (nothing to confirm)

- The task log, `NamedIndexingService`, `IIndexingTaskManager`, the three provider services, `AddIndexingSource` and the
  provider helpers: `OrchardCore.Indexing.Abstractions` and `OrchardCore.Indexing.Core`.
- The keys of part and field handlers, and the Included checks: `ContentPartIndexHandler.cs`, `ContentFieldIndexHandler.cs`
  and `ContentItemIndexCoordinator.cs`.
- Every label of the two clips, in `recording-plan.md` with its `file:line`.

## To confirm on a running site

1. **Selectors of the clips.** The row buttons of the Indexes and Content Types lists depend on the admin list layout
   (Buttons or Menu: `OrchardCore.Admin/Views/AdminListActions.cshtml`, `AdminListActions-Menu.cshtml`). Probe both pages
   with `Recorder probe` and fix the selectors of `recording-plan.md` (`button.actions`, `a[role='btn-edit-BlogPost']`,
   the checkbox labels) before writing `Scenes.cs`. Also confirm that the confirmation dialog of Reset has a **Yes** button.
2. **Time to index after Reset.** `reset-2` waits off camera for the column `BlogPost.Subtitle` in the Lucene Query page.
   Confirm that it appears with **Keyword** and **Stored** ticked, and how many seconds it takes. The background job starts
   after the end of the request (`DefaultIndexProfileManager.SynchronizeAsync`).
3. **Width of the Lucene Query table.** The table lists every stored field, so `BlogPost.Subtitle` may be off screen at
   1536 px. Confirm that scrolling the header cell into view, then highlighting it, gives a readable frame.
4. **A custom source on Lucene, end to end.** The slides of chapter 6 show the pieces of a source and every call checks
   against the signatures, but no custom source was run. `LuceneIndexManager.AddOrUpdateDocumentsAsync` and
   `DeleteDocumentsAsync` read `metadata.IndexMappings.KeyFieldName` of the index profile (in a `try` that logs and returns
   false, so the last task id of the index never advances), and only `LuceneContentIndexProfileHandler` sets the mappings,
   and only for the `Content` type. `CreateLuceneDocument` also writes the key field only for a `ContentItemDocumentIndex`,
   so a plain `DocumentIndex` probably has to add the key field itself. The narration (`source-7`) states only the first
   fact, as something to check. Build a sample module with the `Product` classes of the slides and confirm what a Lucene
   source needs (the mappings, the key field in the document, deletes) before the example is described as complete anywhere.
5. **Which managers win for an existing provider name.** `AddIndexingSource` registers the managers with `TryAddKeyedScoped`
   by provider name, so the first registration of a name wins. Confirm, with a module that registers after Lucene, that
   passing its own manager classes with the name "Lucene" keeps the Lucene ones (the documentation sample suggests the
   opposite). The narration separates a built-in provider (use its helper) from a provider of your own.
6. **The `Sanitize` option.** `DocumentIndexOptions.Sanitize` is set by the Html Body, Markdown, Liquid and full text
   handlers, but no provider code in the source reads it. The narration and the slides do not describe it. Confirm what
   it does, if anything.
7. **The invented examples.** `Product`, `ProductIndexHandler`, `ProductIndexingService`, `ProductIndexProfileHandler`,
   `ProductIndexingBackgroundTask` and the three registrations of `source-6` are written against the signatures read in
   the source and were not compiled. Compile them in a sample module before the slides are final (the constructor of
   `ProductIndexingService`, `_catalog`, `_indexingService` and `_serviceProvider` are left out or shown as fields; the
   record id of a `Product` must be a string of at most 26 characters).
8. **Pronunciation.** Check with `scripts/pronounce.py`: "Lucene", "Elasticsearch", "Html Body", "Keyword", "Async" in the
   method names, and "I Document Index Handler" and the other names that start with "I". Only `BlogPost.Subtitle` has an
   entry in `spoken`.
9. **Length.** The narration is 5:52 at 150 words per minute, a little over the 5.6 minutes of the tracker estimate. Cut
   `part-3` or `source-3` if a shorter video is wanted.
10. **Deleting documents of a custom source.** `NamedIndexingService.ProcessRecordsAsync` passes every task, including
    `Delete` tasks, to `GetBuildDocumentIndexAsync` and never calls `DeleteDocumentsAsync` (`ContentIndexingService`
    returns null for a task that is not an `Update`, and the request path deletes through `IndexingContentHandler`).
    The narration (`source-4`) says only that the base class never deletes documents. The only protected hook that
    receives the tasks and the document index managers is `BeforeProcessingTasksAsync(tasks, contexts)`, so a custom
    source would delete there. Confirm with a sample source. Also confirm the cursor: when a batch gives no document for an
    index, `SetLastTaskIdAsync` is not called (`NamedIndexingService.cs`, the loop over `updatedDocumentsByIndex`), so a
    batch of only `Delete` tasks is read again on the next run.
11. **Several categories in one request.** `IndexingTaskManager.FlushAsync` removes duplicate tasks of the same category
    and record, keeps the last, and runs the insert of the whole queue inside the loop over the categories. A request that
    records tasks of two categories therefore seems to insert each task once for each category. The narration (`source-3`)
    says only that tasks are saved at the end of the request. Confirm in the `RecordIndexingTask` table with a request that
    records both a content task and a `Product` task.
12. **Features page of the recording.** The plan presses **Enable** on the row **Lucene** and expects no confirmation
    dialog (the link has no dialog attributes), but the id of the link (`btn-enable-OrchardCore_Lucene`) comes from
    `IHtmlHelper.GenerateIdFromName` and was not run. Probe it, and check that the search for `Lucene` lists the four rows
    of the Lucene manifest.
13. **The sample post in the task log.** The Blog recipe enables Indexing in its first step and creates the post in a
    later one, so the post should have an `Update` task when the index is created. Confirm that the first **Query Index**
    lists one document; the plan has a fallback (publish the post again) if it lists none.
14. **The Query Index link of the Actions menu.** `LuceneIndexProfile.ActionsMenuItems.SummaryAdmin.cshtml` gives the same
    `<a>` two `asp-action` attributes (`QueryIndex`, then `Query`) and `asp-route-indexName`. `QueryIndex` (route
    `Lucene/query/{indexName}`, `Controllers/AdminController.cs:70`) runs the match all query at once, but `Query` without
    a form post (`:90`) only shows the empty form. Probe which URL the link has. If the page opens without a result line,
    press **Run Query** (`Views/Admin/Query.cshtml:47`) before the narration of `reset-2` ends, or open
    `/Admin/Lucene/query/blogposts` directly. The narration claims only the stored columns, not how the page is reached.


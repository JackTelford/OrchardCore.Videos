# Open questions: Indexes

What could not be settled from the Orchard Core source (commit 92db4a83) and the documentation alone. Each item says
what was checked, what the narration says about it, and what a person must confirm on a running site before the video is
recorded or published. Paths are relative to `src/` of `OrchardCMS/OrchardCore`.

## 1. Do Synchronize, Reset and Rebuild open a confirmation dialog?

- Checked: the three links in `OrchardCore.Modules/OrchardCore.Indexing/Views/IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml`
  carry `data-title`, `data-message`, `data-ok-text` and `data-cancel-text` (for example "Rebuild Index", "Your index will
  be rebuilt, which might alter some services during the process. Continue?") and `data-url-af="UnsafeUrl"`. The
  admin theme (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:114-186`, compiled in
  `wwwroot/js/theadmin/TheAdmin.js`) opens a dialog for such a link only when it also has `RemoveUrl` or a
  `data-unsafe-url` value; otherwise it submits the form at once. Force Delete and Delete have `RemoveUrl`, so they do
  open a dialog.
- The narration says that a message appears after selecting each action, and never mentions a dialog for Synchronize,
  Reset or Rebuild. The recording plan uses a pattern that clicks Yes if a dialog shows up.
- Confirm: select each of the three on a running site. If no dialog appears, Rebuild deletes the index with no
  warning, which the maintainers may want to know (the views suggest a dialog was intended).

## 2. Does typing in Index Name fill in Name?

- Checked: `Assets/js/technical-name-generator.js` copies a lower-case, hyphenated version of the "display" field into the
  "name" field until the "name" field is edited by hand, and `Views/IndexProfileFields.Edit.cshtml:5` passes the id of
  **Index Name** as the display field and the id of **Name** as the name field. So typing in Index Name fills Name,
  which looks like the reverse of the labels.
- Narration (`create-3`): "Type articles, and the Name field fills in from it."
- Confirm on the create form. If Name stays empty or Index Name is the one that fills in, change `create-3` and the plan.

## 3. Are content items created before the Indexing feature was enabled indexed by a new index or by Reset?

- Checked: indexing tasks are recorded only by `CreateIndexingTaskContentHandler` (`OrchardCore.Indexing/CreateIndexingTaskContentHandler.cs`)
  while the Indexing feature is enabled, and a new index, Synchronize, Reset and Rebuild all process that log
  (`OrchardCore.Indexing.Core/ContentIndexingService.cs`, `NamedIndexingService.cs`, `Handlers/ContentIndexProfileHandler.cs:34-63`).
  Nothing in the source creates tasks for content that already existed. The Reset dialog text says "all content items".
- The narration describes Reset and Rebuild in terms of "the log" (`diff-2`, `diff-3`), not "all content items". The
  seed articles are created after Lucene is enabled for that reason.
- Confirm (not needed for the recording): on a site where an Article exists from before Indexing was enabled, create an
  index and check whether the article is found. The result may be worth reporting to the maintainers.

## 4. Do searches return incomplete results while a Rebuild runs?

- Checked: `Rebuild` removes and recreates the index (`OrchardCore.Lucene.Core/Services/LuceneIndexManager.cs:91-131`),
  then resets the task position and synchronizes after the request ends, in a background job
  (`OrchardCore.Indexing.Core/DefaultIndexProfileManager.cs:205-227`). The second sentence of `diff-3` ("The refill
  runs in the background, so searches can return incomplete results until it finishes") is an inference from that
  order, and from the view text "might alter some services during the process".
- Confirm: on a site with enough content, search right after a Rebuild. If it is not observable, soften `diff-3`.

## 5. Selectors derived from markup, not tried

- The left menu ids (`#left-nav figcaption#search`, `#left-nav li#indexes a`), the Enable button id of a feature
  (`btn-enable-OrchardCore_Indexing_Worker`, from `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`
  and ASP.NET's replacement of dots by underscores in generated ids), the row locator `li[data-filter-value='articles']`,
  and the label-based locators were read from the views and the theme, not tried on a page. Probe each with
  `Recorder probe` before writing the scenes.
- The Culture select is expected to show **Any culture** on a new form (it is the first option and the model has no
  value); confirm it is the selected option (`create-4`).

## 6. Background Tasks and the Indexing Worker

- Checked: the Indexing Worker feature registers `ContentIndexingBackgroundTask` (`OrchardCore.Indexing/Startup.cs:93`),
  titled "Indexes Content Updater", scheduled `* * * * *` (every minute), described "Periodically synchronizes and updates
  all modified content items in the search indexes to ensure search results remain current."
  (`OrchardCore.Indexing/ContentIndexingBackgroundTask.cs:9-14`). The Background Tasks page takes the title and the
  description from those defaults (`OrchardCore.BackgroundTasks/Controllers/BackgroundTaskController.cs:101`).
- The Indexing Worker feature depends on Indexing and Contents, not on Background Tasks (`OrchardCore.Indexing/Manifest.cs`),
  and **Tools > Background Tasks** exists only while the Background Tasks feature is enabled
  (`OrchardCore.BackgroundTasks/AdminMenu.cs`), so `worker-2` begins "With the Background Tasks feature enabled".
- Narration `worker-2` says it is listed "with a description" and that "By default it runs every minute". The list does
  not display the schedule.
- The list is built from the background tasks registered in the tenant's services, not from the state the background
  service keeps (`BackgroundTaskController.cs:87-108`), so the row should be there as soon as the shell has restarted
  after the feature is enabled. Only the **Next run** badge needs the state of the task (`BackgroundTaskEntry.NextRun.SummaryAdmin.cshtml`).
  The list is sorted by title and shows 10 tasks to a page (`BackgroundTaskController.cs:174`).
- Confirm: that the row appears right after the feature is enabled, on the first page, and that the task runs about
  once a minute (its **Next run** badge, if shown). If the row is missing, wait a minute and reload.
- `worker-3` says that saving content updates the indexes with or without the worker. This follows from
  `IndexingContentHandler` being registered by the Indexing module's content startup, not by the worker feature
  (`OrchardCore.Indexing/Startup.cs:52-62`). Confirm by saving an Article with the worker disabled and searching for it
  (for example with the front-end search of the Search feature).

## 7. Other details

- The voice must say "Lucene", "Elasticsearch", "Azure AI Search", "Synchronize" and "Indexes" correctly; check them with
  `scripts/pronounce.py` and add fixes to the storyboard's `spoken` map.
- The Indexes page loads `Assets/ts/indexing-admin-index.ts`, which imports `bulk-select-list` from the package
  `@orchardcore/bloom`. The source of that package is not in the checkout that was read; only its bundle in
  `OrchardCore.Indexing/wwwroot/Scripts/indexing-admin-index/indexing-admin-index.js` was. By that bundle the bulk
  **Actions** menu (the wrapper `#actions`, which holds the button `#bulk-action-menu-button`,
  `OrchardCore.Admin/Views/AdminListToolbar.cshtml:65-67`) is shown only when more than one row is checked, which is why `edit-5` needs the two indexes, and its items ask for confirmation with the dialog
  **Bulk Action**. `OrchardCore.Resources/Assets/js/list-management.js` is not loaded by this page. Confirm that
  `#select-all` shows the bulk **Actions** button with two indexes (`edit-5`). The plan never chooses an item.
- The order of the editors on the Create and Edit forms. The Indexing module places the content settings (**Culture**,
  **Content types**, **Index Latest version**, `ContentIndexProfileDisplayDriver`) at `Content:5`, and Lucene places its
  own options (**Analyzer**, **Store Source Data**) at `Content:5` and its **Default Query Settings** at `Content:10`
  (`OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs`). The tie at `Content:5` was not resolved from the
  source, so the narration of `create-6` does not say where Lucene's options are. Confirm the order on the form; if the
  Lucene options come before **Culture**, adjust the scrolling of `create-4` to `create-6` and the frames they end on.
- The Create form of a Lucene index also shows the Lucene options and **Default Query Settings** (not narrated; a
  separate video covers them). On a new index the list under **Query field names** is empty, because the field
  mappings are only known once the index exists (`LuceneIndexProfileDisplayDriver.cs:70-74`), and saving with no
  field selected is accepted (`LuceneIndexProfileDisplayDriver.cs:118`). Confirm that saving with the default values
  works, as `create-7` assumes.

## 8. How long does a success message stay on screen?

- Checked: a success message is added with a dismissal of 5000 ms (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`).
  The admin theme renders it as a Bootstrap toast with `data-bs-autohide="true"` and `data-bs-delay="5000"`, in a fixed
  container whose horizontal position is set to the right (`OrchardCore.Themes/TheAdmin/Views/Message.cshtml`,
  `NotifyMessages.cshtml`), and shows it when the page script runs (`Assets/js/NotifyMessages/NotifyMessages.ts`). An error
  or warning message has no delay and stays until closed.
- The narration names the message of `create-7`, `actions-3` to `actions-5` and `edit-3`. The recording plan times each
  click so that the toast is on screen when the sentence is read (section 3 and the clips).
- Confirm on the running site: where the toast sits (the container has no explicit top or bottom, so its vertical
  position follows the layout), that it does not cover a control the clip clicks, and whether it stays open while the
  pointer rests on it (Bootstrap pauses the hiding of a toast while it is hovered). The plan does not rely on the
  pointer.

## 9. Observations from the source that the narration does not use

- Force Delete calls `await indexManager?.DeleteAsync(...)` (`AdminController.cs:364`). If no index manager is registered
  for the provider, the expression is `await null` and throws; the narration of `edit-4` therefore claims only that
  Force Delete removes the index from the site when the provider could not delete it, which follows from the ignored
  result of `DeleteAsync` (`AdminController.cs:364-365`).
- **Query Index** in the Actions menu opens a Lucene page that needs the permission **Manage Lucene Indexes**
  (`OrchardCore.Lucene/Controllers/AdminController.cs:93`, `LuceneSearchPermissions.cs:8`), not **Manage Indexes**. The
  narration of `perm-1` and its slide therefore name only the Indexes page and the actions of the Indexing module,
  each of which checks **Manage Indexes** (`AdminController.cs:67,149,184,268,294,348,404,429,452,496`).
- A deleted content item adds a delete task to the log, but processing the log skips delete tasks
  (`ContentIndexingService.cs:105-108`); a deletion reaches the index when the content item is removed
  (`IndexingContentHandler.cs:37-38,125-139,196`). `diff-2` says "every recorded update".


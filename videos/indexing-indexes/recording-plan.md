# Recording plan: Indexes

Everything a person needs to record the clips of `storyboard.json` without deciding anything. This file is for the
recorder: it may hold selectors, scene calls and file references, which the narration, slides and captions never do. A
selector written as `#left-nav` stands for `scene.Page.Locator("#left-nav")`. The seconds after a step are its
approximate narration length at 150 words per minute; the real length is in `durations.json` once `narrate.py` has run,
and every step's actions must end before it.

Read `.agents/skills/orchardcore-video-producer/references/recording.md` first for the recorder and the scene API. The
Orchard Core source paths below are relative to `src/` of an `OrchardCMS/OrchardCore` checkout (the labels were read
at commit 92db4a83; use that commit or a later `main` and compare the labels with the screen).

## 1. The site it needs

| What | Value |
|---|---|
| Site | `src/OrchardCore.Cms.Web` on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Auto Setup | Environment variables, so no Setup or sign-in screen is recorded: `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...SiteName=Orchard Core`, `...SiteTimeZone=America/Los_Angeles`, `...AdminUsername=admin`, `...AdminEmail=admin@example.com`, `...AdminPassword=<password>` (the value of `VIDEO_PASSWORD`, chosen outside git), `...DatabaseProvider=Sqlite`, `...RecipeName=Blog` |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, which saves `state.json` (git-ignored) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls a service |
| External services | None. Lucene keeps its indexes in the files of the site (`OrchardCore.Lucene.Core/LuceneIndexStore.cs:45-48`), so there is no stand-in to run |
| Admin settings | Left at their defaults: the List layout and the Buttons layout for row actions (`OrchardCore.Admin.Abstractions/AdminListOptions.cs`). Do not open Configuration > Settings > Admin |
| Features to enable before recording (outside the capture) | **Lucene** (`OrchardCore.Lucene`) and **Background Tasks** (`OrchardCore.BackgroundTasks`), in Tools > Features. The Blog recipe already enables **Indexing** (`OrchardCore.Indexing`). Do **not** enable **Indexing Worker** (`OrchardCore.Indexing.Worker`) or **Lucene Worker**: clip `06-worker` enables the Indexing Worker on camera |
| Indexes before recording | None. The list must be empty when `02-create` starts. Enabling Lucene on a new site creates no index (`OrchardCore.Lucene/DataMigrations/IndexingMigrations.cs` only converts old settings) |

### Commands

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/indexing-indexes/recorder
export VIDEO_ROOT=<repo>/videos/indexing-indexes VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
export MSYS_NO_PATHCONV=1
<bin>/Recorder setup            # signs in once and saves state.json (git-ignored)
python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/indexing-indexes
<bin>/Recorder record 02-create
<bin>/Recorder record 04-actions 05-edit 06-worker   # after the off-camera steps of section 2
python .agents/skills/orchardcore-video-producer/scripts/review.py videos/indexing-indexes <clip>
```

`<password>` is the value chosen outside git; never write a real or sample password in a file. The scene methods go in
`recorder/Scenes.cs` of this folder, one method per clip id (`02-create`, `04-actions`, `05-edit`, `06-worker`).

### Seed data (outside the capture, after Lucene is enabled)

Create three Article content items so the index has something to hold. The Indexing feature, which the Blog recipe
enables, records an indexing task for every content item that is created or published
(`OrchardCore.Indexing/CreateIndexingTaskContentHandler.cs:33-37`), whether or not Lucene is enabled, and the new index
processes those tasks when it is saved. Use Content > Content Items > New > Article, where **New** is the button of
the list (`OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`), fill in only the Title, and select Publish.
None of them appears on screen in any clip; the Blog recipe's own sample items are never framed.

| Title |
|---|
| Spring herb garden notes |
| A rainy day soup |
| Notes from a fictional coast road |

The names, emails and numbers on screen are limited to the user `admin` (email `admin@example.com`, never shown) and
the index names `articles` and `pages`.

## 2. Order, dependencies and snapshots

Record in this order, which is also the order they play in:

1. `02-create`: needs the empty list. **Changes the site** (creates the index `articles`).
2. Between the clips, **off camera**: create the second index `pages` with the same form as `02-create`: Search >
   Indexes > Add index > Add on Content in Lucene, Index Name `pages`, content type **Page** only, Save. Then copy the
   site's `App_Data` and `state.json` aside as snapshot **S1**.
3. `04-actions`: needs `articles` and `pages`. Re-indexes, leaves no visible change.
4. `05-edit`: needs `articles` and `pages`. **Changes the site** (adds Blog Post and Index Latest version to
   `articles`). Restore S1 before a new take.
5. `06-worker`: needs Background Tasks enabled. **Changes the site** (enables Indexing Worker). Restore S1 before a
   new take.

Snapshot **S0** is the site after the setup, the two features and the seed data, before `02-create`. Restore S0 before
a new take of `02-create`, and create `pages` and S1 again afterwards. Stop the site before copying or restoring
`App_Data`.

Slide steps (`why-1` to `why-3`, `diff-1` to `diff-3`, `perm-1`) are drawn by `slides.py` and need no recording.

## 3. Patterns used by several clips

**The Indexes row.** `articles` is the row `li[data-filter-value='articles']`: the Indexing module puts the lower-case
name of the index in that attribute (`OrchardCore.Indexing/Drivers/IndexProfileDisplayDriver.cs:36`). In the text
below, `row` is that locator. Its parts: the name link, `row.Locator(".tags")` (the badges), `row.Locator(".metadata")`,
and `row.Locator(".admin-list-actions")` (the Edit and Delete buttons and the Actions menu).

**Opening the row's Actions menu.** The toggle is the button labeled `Actions` inside the row,
`row.Locator(".admin-list-actions button.dropdown-toggle")` (`OrchardCore.Admin/Views/AdminListActions.cshtml:30-32`);
its items are `row.Locator(".actions-menu a.dropdown-item")`. Do not confuse it with the bulk `Actions` button
`#bulk-action-menu-button`, which sits in the wrapper `#actions` and stays hidden until more than one row is checked
(`OrchardCore.Admin/Views/AdminListToolbar.cshtml:65-67`; `OPEN-QUESTIONS.md`, item 7).

**Choosing Synchronize, Reset or Rebuild.** By the source of the admin theme these three links submit at once, with no
dialog (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:114-186`: only a link with `RemoveUrl` or a
`data-unsafe-url` value opens one). A person must confirm that on the running site (see `OPEN-QUESTIONS.md`), so use a
pattern that works with and without a dialog (its buttons are `#modalOkButton`, labeled Yes, and `#modalCancelButton`,
labeled No):

```csharp
await scene.WaitForNavigationAsync(async () =>
{
    await scene.ClickAsync(row.Locator(".actions-menu a.dropdown-item", new() { HasTextString = "Synchronize" }), pauseAfter: 300);
    try
    {
        await scene.Page.Locator("#modalOkButton").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1200 });
        await scene.ClickAsync(scene.Page.Locator("#modalOkButton"));
    }
    catch (PlaywrightException)
    {
        // No dialog: the form was already submitted.
    }
});
```

**The left menu.** The admin menu is `#left-nav` (`OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`). A
top-level group is a row, `#left-nav figcaption:has(span.title:text-is('<Group>'))` (for example `'Tools'` or `'Search'`);
clicking it expands its child list, `#left-nav ul[data-title='<Group>']` (the theme writes the group's text in that
attribute), and a child is a link in that list. Write the moves this way:

```csharp
// Expand a group only when its list is hidden (the admin remembers what was left open).
var list = scene.Page.Locator("#left-nav ul[data-title='Tools']");
if (!await list.IsVisibleAsync())
{
    await scene.ClickAsync(scene.Page.Locator("#left-nav figcaption:has(span.title:text-is('Tools'))"), pauseAfter: 600);
}
// Then the child link, which loads a page.
await scene.ClickAndWaitForNavigationAsync(list.GetByRole(AriaRole.Link, new() { Name = "Features", Exact = true }));
```

The Search group has the id `search` and Indexes the id `indexes` (`OrchardCore.Indexing/AdminMenu.cs:21,25`), so
`#left-nav li#indexes a` also matches. If a menu locator does not match, probe the page with `Recorder probe`; if the
menu still cannot be driven, use `scene.NavigateAsync` with the page's path at the same moment, which looks the same on
screen: `/Admin/indexing` (Search > Indexes), `/Admin/Features` (Tools > Features), `/Admin/BackgroundTasks` (Tools >
Background Tasks, `OrchardCore.BackgroundTasks/Controllers/BackgroundTaskController.cs:74`).

**Success messages are toasts that hide by themselves.** After the redirect, the admin theme shows each message as a
Bootstrap toast in a fixed container at the right of the page (`OrchardCore.Themes/TheAdmin/Views/NotifyMessages.cshtml`,
`Message.cshtml`, `Assets/js/NotifyMessages/NotifyMessages.ts`). A success message is added with a 5000 ms dismissal
(`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`), so the toast is on screen for 5 seconds
from the moment the page loads and is then removed. It does not stay until closed. Only an error or a warning
message has no delay and stays. Their exact texts are the `H["..."]` strings of
`OrchardCore.Indexing/Controllers/AdminController.cs` (line numbers below).

Because the contact sheet checks the frame at the end of each step, time every click that ends in a success message so
that the redirected page loads no earlier than 4.5 seconds before the narration of the step ends (step length =
words / 2.5 seconds). Each clip below gives the second of the click. Do not rely on the pointer to keep a toast open
(see `OPEN-QUESTIONS.md`, item 8).

## 4. Clips

### `02-create` (chapter 02, Add an index)

Starts on `/Admin/Features`, opened with `scene.OpenAsync` before the capture, with `Lucene` typed in the search box
(`#search-box`, placeholder `Search`, `OrchardCore.Features/Views/Admin/Features.cshtml:27`) so the list shows the
Lucene rows and the **Lucene** row says **Enabled**. The `Search` group of the left menu must be collapsed. Then
`StartCaptureAsync`.

- `create-1` (about 9 s). Expand the **Search** group (`OrchardCore.Indexing/AdminMenu.cs:19`, pattern in section 3),
  pause 600 ms, then `ClickAndWaitForNavigationAsync` on the link **Indexes** of `#left-nav ul[data-title='Search']`
  (`AdminMenu.cs:22`).
  Visible at the end: the Indexes page (`/Admin/indexing`), the title `Indexes`, the search box and the **Add index**
  button, and the message **Nothing here! There are no indexes at the moment.** (`OrchardCore.Indexing/Views/Admin/Index.cshtml:40`).
- `create-2` (about 8 s). Click `button.create` labeled `Add index` (`Index.cshtml:31`) and wait for `#modalAddIndex.show`
  (the dialog fades in); pause 2500 ms on the dialog titled **Available Sources** (`Index.cshtml:61`), which shows a
  heading **Lucene** (`OrchardCore.Lucene.Core/ServiceCollectionExtensions.cs:60`) and one card whose title is
  **Content in Lucene** (`OrchardCore.Lucene/Startup.cs:98`). Then
  `ClickAndWaitForNavigationAsync(#modalAddIndex .card with the text "Content in Lucene" > a labeled "Add")`
  (`OrchardCore.Indexing/Views/IndexProfile.Link.cshtml:29`). The click should land about 6 s into the step.
  Visible at the end: the form titled **New 'Content in Lucene' index** (`Views/Admin/Create.cshtml:6`) with the
  breadcrumb **Indexes**.
- `create-3` (about 10 s). `TypeAsync(GetByLabel("Index Name", new() { Exact = true }), "articles")`
  (`OrchardCore.Indexing/Views/IndexProfileFields.Edit.cshtml:6`). The page script fills the **Name** field with a
  lower-case, hyphenated copy of what is typed in **Index Name** (`OrchardCore.Indexing/Assets/js/technical-name-generator.js`,
  wired by `IndexProfileFields.Edit.cshtml:5,22-26`). Do not type in **Name**. If **Name** is still empty after typing
  (the script did not run), stop and see `OPEN-QUESTIONS.md`. Visible at the end: `articles` in both **Index Name** and
  **Name**.
- `create-4` (about 10 s). `HighlightAsync(GetByLabel("Name", new() { Exact = true }))` for about 3 s, then
  `ClearHighlightsAsync()`, then `HighlightAsync(GetByLabel("Culture"))`, whose select reads **Any culture**
  (`OrchardCore.Indexing/Views/ContentIndexMetadata.Edit.cshtml:6-11`). The hint under **Name** reads **A unique name for
  the index.** (`IndexProfileFields.Edit.cshtml:18`). Visible at the end: the **Culture** select showing **Any culture**.
- `create-5` (about 8 s). `ClearHighlightsAsync()`, scroll (`ScrollAsync(0, 300)`, again if needed) until **Content
  types** and its checkboxes are in view, then `ClickAsync(GetByLabel("Article", new() { Exact = true }))`. The labels
  are the display names of the content types, in alphabetical order, with a **Select all** box first
  (`OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml:10`). Visible at the end: **Article**
  checked and no other content type, under the hint **The content types to index. Choose at least one.**
  (`ContentIndexMetadata.Edit.cshtml:19`).
- `create-6` (about 11 s). Scroll to the checkbox **Index Latest version**, unchecked, with its hint **Check to index
  draft if it exists, otherwise only the published version is indexed.** (`ContentIndexMetadata.Edit.cshtml:31-32`);
  `HighlightAsync` it for about 3 s, `ClearHighlightsAsync()`, then scroll until the options Lucene adds are in view
  (**Analyzer**, **Store Source Data**, **Default Query Settings**, `OrchardCore.Lucene/Views/LuceneIndexProfile.Edit.cshtml`);
  where they sit in the form depends on the order of the editors (see `OPEN-QUESTIONS.md`). Change none of them.
  Visible at the end: the Lucene options, unchanged.
- `create-7` (about 8 s). `ClickAndWaitForNavigationAsync` on the button labeled `Save` in `.action-bar-bottom`
  (`Views/Admin/Create.cshtml:16`), clicked about 2.5 s into the step so that the Indexes page loads about 4 s in: the
  success toast then shows from 4 s to 9 s, which covers the end of the step. Visible at the end: the Indexes page, the
  row `articles`, and the toast **An index has been created successfully. The synchronizing process was triggered in the
  background.** (`AdminController.cs:257`).

### `04-actions` (chapter 04, Run the actions)

Starts on `/Admin/indexing` (`scene.OpenAsync`), with the two rows `articles` and `pages`, no message, and every menu
state as the page leaves it. The Actions menus are closed. Then `StartCaptureAsync`.

- `actions-1` (about 14 s). Follow the order of the narration. `HighlightAsync(scene.Page.Locator("li[data-filter-value='pages']"))`
  for about 5.5 s: the second index `pages`, created off camera (section 2) with the content type **Page**.
  `ClearHighlightsAsync()`, then `HighlightAsync(row.Locator(".admin-list-actions"))` for about 4.5 s: the buttons **Edit**
  and **Delete** (`IndexProfile.Buttons.SummaryAdmin.cshtml:8,13`) and the **Actions** button.
  `ClearHighlightsAsync()`, then `HighlightAsync(row.Locator(".tags"))` until the end of the step: the badges **Lucene**
  (title `Provider`) and **Content** (title `Type`) (`OrchardCore.Indexing/Views/IndexProfile.DefaultTags.SummaryAdmin.cshtml:6,10`).
  Visible at the end: the `articles` row, with its badges highlighted.
- `actions-2` (about 8 s). `ClearHighlightsAsync()`, click the row's Actions toggle (pattern in section 3), pause 3000
  ms. The menu lists, in this order, **Query Index** (`OrchardCore.Lucene/Views/LuceneIndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:11`),
  **Synchronize**, **Reset**, **Rebuild** and **Force Delete** (`IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:13,23,32,42`).
  Visible at the end: the open menu with all five items. Leave it open for the next step.
- `actions-3` (about 5 s). The menu is open from the previous step: choose **Synchronize** with the pattern of section 3,
  about 0.5 s into the step. Visible at the end: the Indexes page and the toast **The synchronizing process was
  triggered in the background.** (`AdminController.cs:443`), which shows from about 1.5 s to 6.5 s.
- `actions-4` (about 8 s). Open the row's Actions menu again and choose **Reset** about 2 s into the step, so the page
  loads about 3.5 s in. Visible at the end: the toast **An index has been reset successfully. The synchronizing process
  was triggered in the background.** (`AdminController.cs:420`).
- `actions-5` (about 8 s). Open the Actions menu again and choose **Rebuild** about 2 s into the step. Visible at the
  end: the toast **An index has been rebuilt successfully. The synchronizing process was triggered in the background.**
  (`AdminController.cs:480`). If the toast reads **An error occurred while rebuilding the index.**, the provider returned
  false: another rebuild of the same index holds the lock `LuceneRebuild-<id>` (`LuceneIndexManager.cs:97-106`), or the
  index could not be created again (`LuceneIndexManager.cs:119-128`). A running synchronization takes a different lock
  (`IndexingService-<id>`, `NamedIndexingService.cs:131`) and does not cause it. An error toast has no delay and stays
  until closed. Wait a minute, restore S1 and take the clip again.

### `05-edit` (chapter 05, Edit and delete indexes)

Starts on `/Admin/indexing` (`scene.OpenAsync`), the two rows, no message. Then `StartCaptureAsync`.

- `edit-1` (about 12 s). About 4 s into the step, as "select Edit on the articles index" ends,
  `ClickAndWaitForNavigationAsync(row.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }))`,
  then, about 5.5 s in (as "Index Name is disabled" starts), `HighlightAsync(GetByLabel("Index Name", new() { Exact = true }))` for about 3 s: the field is disabled
  (`IndexProfileFields.Edit.cshtml:8`, `asp-is-disabled="!Model.IsNew"`). Visible at the end: the form titled **Edit
  'articles' index** (`Views/Admin/Edit.cshtml:6`) with **Index Name** grayed out and the other fields enabled.
- `edit-2` (about 10 s). `ClearHighlightsAsync()`, scroll until **Content types** is in view, `ClickAsync(GetByLabel("Blog Post", new() { Exact = true }))`
  (the exact label matters: the list also holds **Blog**), then `ClickAsync(GetByLabel("Index Latest version"))`, then
  `ClickAndWaitForNavigationAsync` on the `Save` button of `.action-bar-bottom` as the last action, about 8.5 s into the
  step, so that the Indexes page loads at the end of the step or just after it. Visible at the end: the Indexes page,
  with the toast starting to show.
- `edit-3` (about 14 s). No action: hold on the page. The toast **An index has been updated successfully.**
  (`AdminController.cs:336`) shows for 5 s from the page load, which covers the first sentence of the narration; it is
  gone by the end of the step, and the narration after the first sentence names nothing on screen. Do not highlight
  the toast. Visible at the end: the Indexes page with the two rows and no dialog.
- `edit-4` (about 15 s). Click the button **Delete** of the `articles` row (`IndexProfile.Buttons.SummaryAdmin.cshtml:13`):
  a dialog titled **Delete** opens, with the text **Are you sure you want to remove this element?** and the buttons
  **Ok** and **Cancel** (`OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`). Hold 2000 ms, click `#modalCancelButton`.
  Then open the row's Actions menu and click **Force Delete**: a dialog titled **Force Delete** opens, with the buttons
  **Yes** and **No** (`IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:38-41`). Hold 2000 ms, click
  `#modalCancelButton` (labeled **No**). Never click `#modalOkButton`. Visible at the end: the Indexes page with both
  rows and no dialog.
- `edit-5` (about 10 s). Click `#select-all` (the checkbox of the toolbar above the list,
  `OrchardCore.Admin/Views/AdminListToolbar.cshtml:31`). With the two rows `articles` and `pages` both become checked, and
  the bulk **Actions** button `#bulk-action-menu-button` appears: the page script shows it only when **more than one**
  row is checked (`OrchardCore.Indexing/wwwroot/Scripts/indexing-admin-index/indexing-admin-index.js`, the bundled
  `bulk-select-list` component). Click it. The menu lists **Synchronize**, **Reset**, **Rebuild** and **Delete** (`AdminController.cs:118-121`). Hold
  3000 ms, press Escape to close the menu, click `#select-all` again to clear the selection. Never choose an item.
  Visible at the end: the list with no row selected (the menu is allowed to stay open until the narration ends if the
  frame at that moment shows it).

### `06-worker` (chapter 06, Keep indexes current)

Starts on `/Admin/indexing` (`scene.OpenAsync`). Background Tasks is enabled; Indexing Worker is not. Then
`StartCaptureAsync`.

- `worker-1` (about 11 s). Expand the **Tools** group and click the link **Features** of `#left-nav ul[data-title='Tools']`
  (pattern in section 3; `OrchardCore.Features/AdminMenu.cs:26-27`; `ClickAndWaitForNavigationAsync`). Click `#search-box` and type
  `Indexing Worker`; the list keeps the row **Indexing Worker** (its id `OrchardCore.Indexing.Worker` is printed
  beside the name) and **Disabled** is its status. Click its **Enable** button
  (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`, id `btn-enable-OrchardCore_Indexing_Worker`:
  ASP.NET replaces the dots of the feature id with underscores; check the id on the page, and otherwise use the
  **Enable** button of the only visible row) with `ClickAndWaitForNavigationAsync`, about 9.5 s into the step, after
  "Select Enable." is read. The narration names no message. Visible at the end: either the Features page after the
  redirect (the search box is empty again, because the search is done in the browser), with the toast **The feature
  Indexing Worker was enabled.** (`OrchardCore.Features/Controllers/AdminController.cs:317`), which the narration does
  not name and which is allowed; or, if the redirect has not finished when the narration ends, the filtered list with
  the row **Indexing Worker** and its **Enable** button just clicked. Both match "Select Enable.". The next clip
  step (`worker-2`) starts from whichever page is showing.
- `worker-2` (about 10 s). Expand the **Tools** group again if its list is hidden, and click the link **Background Tasks**
  of `#left-nav ul[data-title='Tools']` (`OrchardCore.BackgroundTasks/AdminMenu.cs:19`; `ClickAndWaitForNavigationAsync`),
  about 2 s into the step, while "Tools, Background Tasks" is read. About 4.5 s in, as "lists it as Indexes Content
  Updater" is read, `HighlightAsync` the row whose title is **Indexes Content Updater** (`OrchardCore.Indexing/ContentIndexingBackgroundTask.cs:11`) together with its
  description (**Periodically synchronizes and updates all modified content items in the search indexes to ensure search
  results remain current.**, line 13). The page lists the tasks sorted by title, 10 to a page
  (`OrchardCore.BackgroundTasks/Controllers/BackgroundTaskController.cs:174`, `OrchardCore.Navigation.Core/PagerOptions.cs:5`).
  With the features of this site only a few tasks exist, so the row is on the first page; if it is not, type
  `Indexes Content` in `#search-box` (placeholder `Search`, `Views/BackgroundTask/Index.cshtml:20`) and press Enter
  right after the page loads, then highlight the row. Visible at the end: the Background Tasks page, the row highlighted. If the row is
  missing, see `OPEN-QUESTIONS.md`, item 6.
- `worker-3` (about 11 s). No action: hold, keeping the highlight. Visible at the end: the same row.

### Slide steps (no recording)

- `why-1`: three cards, "Your content", "An index", "A search", under "An index is a copy built for searching".
- `why-2`: three cards, "Indexing module", "Provider", "Source", under "Who does what".
- `why-3`: three cards, "A content item changes", "The change is logged", "Indexing Worker", under "How an index stays up to date".
- `diff-1`, `diff-2`, `diff-3`: the cards "Synchronize", "Reset" and "Rebuild" under "Three ways to update an index";
  the card of the step is bright, the other two are dimmed.
- `perm-1`: three cards, "Manage Indexes", "Administrators", "Other roles", under "Who can manage indexes".

## 5. Checks for private data

- The only account on screen is `admin` (the author badge of an index row). No email is ever displayed; the address in
  the Auto Setup is `admin@example.com`.
- The Blog recipe's sample content items and the three seed articles are never framed. If a frame shows a list of
  content, record the clip again.
- No page shows a file path of the machine: the Lucene folder is not displayed anywhere in these clips.
- After assembling, OCR the video at one frame per second, at every scene change and at 5 frames per second, and search
  for real names, email addresses other than `@example.com`, and tokens (step 6 of the skill's workflow).
- No password is typed on camera: the session comes from `state.json`.

# Recording plan: Drafts and Versions

How to record the clips of this video without deciding anything. Source paths are relative to the root of
`OrchardCMS/OrchardCore`; the labels were read from the commit `92db4a83`. In this plan a selector stands for
`scene.Page.Locator(...)`.

The video has four recordings (`02-draft`, `03-publish`, `04-unpublish`, `05-find`), two slide clips (`01-versions`,
`06-limits`) and two cards (`00-intro`, `99-outro`). All recordings use the content items list of the Blog recipe and
its Article type, so no feature has to be enabled.

## 1. The site it needs

| Item | Value |
| --- | --- |
| Source | `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore`, run on `http://localhost:5010` |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Auto Setup | Tenant `Default`, recipe `Blog`, site name `Orchard Core`, time zone `America/Los_Angeles`, database `Sqlite`, user `admin`, email `admin@example.com`, password `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address |
| Features | None to enable. The Blog recipe enables Contents, Content Types, Content Fields, Autoroute, Title, Html, Content Preview and the themes (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-81`). Leave Audit Trail, Publish Later, Archive Later, Content Localization, Content Version Pruning and Workflows disabled, so the edit page and the Actions menu look as in the narration. |
| Settings | Leave every setting as the recipe makes it. The admin list must use the List layout with the Buttons actions layout (the defaults, `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`). The browser window is at least 768 px wide: below that, or with the Menu actions layout, the edit page moves Unpublish, Delete and Preview into an overflow menu (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/actionBar.ts`). |
| External services | None |

Environment for Auto Setup (Git Bash), then start the site:

```bash
export ORCHARD_APP_DATA=<empty folder outside the repositories>
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9
dotnet run --project src/OrchardCore.Cms.Web --urls http://localhost:5010
```

### Seed data (outside the capture, by hand in the admin)

Sign in as `admin`, open Content, Content Items, and create the following. Use only these values.

1. **The About article** comes from the recipe: type Article, published, Subtitle `This is what I do.`, address
   `/about` (`blog.recipe.json:1060-1082`). Do not change it. Check that `/about` shows the title About and the subtitle
   `This is what I do.` in the page header.
2. **Parking Information**, published, then with a draft:
   1. Click **New** (`src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:88`), then **Article**. Title
      `Parking Information`. Subtitle `Free parking behind the shop`. Leave Body and Banner Image empty. Click
      **Publish**. The message is `Your Article has been published.`
   2. On its row click **Edit**. Replace the Subtitle with `Free parking behind the shop on Saturdays`. Click **Save
      Draft** (the main part of the button, never its drop-down). The message is `Your Article draft has been saved.`
      The row shows the badges Published and Draft.
3. **Spring Event**, never published: click **New**, then **Article**. Title `Spring Event`. Subtitle `Join us for the
   spring harvest`. Click **Save Draft**. The row shows the badge Draft only.

Do not click **and continue**, in the drop-down of **Save Draft**, **Publish** or **Unpublish**, at any time. The choice
is kept by the browser, not by the site (in its local storage and in a cookie named after the tenant,
`Default-adminPreferences`; `src/OrchardCore.Themes/TheAdmin/Assets/js/adminPreferences.ts`,
`.../TheAdmin/actionGroup.ts`), and it would swap the buttons in the recording. The saved sign-in carries it. If it
happened in the browser that made the sign-in, delete the saved sign-in and sign in again in a fresh browser profile.

Before saving the sign-in (`Recorder setup`), check on the Content Items page that the layout selector shows the List
layout, because the choice is kept in a cookie that the saved sign-in carries.

Then stop the site and copy its data folder aside as **snapshot S0** (the state before `02-draft`). The state then is:
About published with no draft; Parking Information published with a draft; Spring Event with a draft only.

## 2. Order, dependencies and snapshots

Record in this order; each clip starts from the state the previous one leaves.

| Clip | Needs | Changes the site | Leaves |
| --- | --- | --- | --- |
| `02-draft` | S0 | Yes: gives About a draft | About published, with the draft `We grow apples, pears and plums.` |
| `03-publish` | the end of `02-draft` | Yes: publishes that draft, makes a second draft and discards it | About published with the subtitle `We grow apples, pears and plums.`, no draft |
| `04-unpublish` | the end of `03-publish` | Yes: unpublishes About and publishes it again | About published, no draft |
| `05-find` | the end of `04-unpublish` | No (a filter only) | Unchanged |

The slide clips need no site. Take a snapshot of the data folder (site stopped) after each of the first three clips:
**S1** after `02-draft`, **S2** after `03-publish`, **S3** after `04-unpublish`. To record a clip again, restore the
snapshot taken before it: S0 for `02-draft`, S1 for `03-publish`, S2 for `04-unpublish`, S3 for `05-find`. Start
the site again after every restore.

Common to every recording clip:

- The clip starts on `/Admin/Contents/ContentItems`, opened with `OpenAsync` before the capture. Wait until the row of
  About is shown. The Content section of the admin menu is expanded and **Content Items** is the active entry.
- The rows are sorted by the default order, most recently modified first.
- Selectors used below (all relative to the page):
  - A row: `ul.admin-list-list > li.list-group-item` that contains the text of the item (`About`, `Parking Information`,
    `Spring Event`).
  - In a row: the status badges `.tags .ta-badge` (Published is the one that contains `.fa-check`, Draft the one that
    contains `.fa-pencil`: `.tags .ta-badge:has(.fa-check)` and `.tags .ta-badge:has(.fa-pencil)`). Never use `.ta-badge`
    alone: the content type (`.contenttype`) and the modified time (`.metadata`) are `.ta-badge` too, and must not be
    ringed. The buttons `a.btn.edit` (Edit), `a.btn.view` (View), `a.btn.preview` (Preview Draft); the Actions button
    `button.actions`; the menu entries `.actions-menu a.dropdown-item`, found by their text.
  - The confirmation dialog `#confirmRemoveModal`, with the buttons `#modalOkButton` (Yes) and `#modalCancelButton` (No).
  - A message: `.oc-notify-toast-container .toast`. Success messages disappear after 5 seconds, so the narration of
    the step says the text of the message in its first sentences; make the click that causes it early enough for the
    message to be on screen while it is spoken. What must be visible at the end of a step never depends on a message.
  - On the edit page: the Subtitle `input[name="Article.Subtitle.Text"]`; the main Save Draft button
    `button.draft[name="submit.Save"]`; the Publish button `button.publish`.
- The edit page opened from the list has a returnUrl, so **Save Draft** and **Publish** return to the list. Only click
  the main part of the buttons.
- Do not use Preview, Clone, Delete or the bulk actions.

## 3. The clips

### `01-versions` (slides)

- `versions-1`: the heading "Published and draft", with two cards: Published and Draft.
- `versions-2`: the heading "Editing a published item", with three cards: edit and save a draft, a separate version,
  publish the draft.
- `versions-3`: the heading "Badges in the Content Items list", with three cards: Published, Draft, Published and Draft.
- `versions-4`: the heading "Two content type settings", with three cards: Draftable, Versionable, The Article type in
  this video.

### `02-draft` (recording; starts on `/Admin/Contents/ContentItems`; needs S0)

| Step | Actions | Visible when the narration ends | Labels from |
| --- | --- | --- | --- |
| `draft-1` | Highlight the **Content Items** entry of the admin menu (`#left-nav a[href$="/Contents/ContentItems"]`) for about 2 seconds, then clear it. Move to the About row and highlight its Published badge. Clear the highlight, then click **Edit** of the About row with `ClickAndWaitForNavigationAsync`, in the last 2 seconds of the step. | The Edit Article page has opened, with the title field first. | Content, Content Items: `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65,68`. Published: `.../Views/ContentsTags_SummaryAdmin.cshtml:12`. Edit: `.../Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`. |
| `draft-2` | Scroll down until the Subtitle field is visible. Triple-click the Subtitle and press Delete to empty it, then use `TypeAsync` to type `We grow apples, pears and plums.` and press Tab. Highlight the **Publish** button for about 2 seconds, clear it, then highlight the **Save Draft** button in the bottom bar and leave it. | The Subtitle shows `We grow apples, pears and plums.` and the bottom bar shows Publish and Save Draft, with Save Draft ringed. The bar also holds Unpublish (the item is published), Delete, Preview (Content Preview is enabled) and Cancel: leave them as they are. | Edit Article: `.../Views/Admin/Edit.cshtml:39`. Subtitle: `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:677-686`. Publish: `.../Views/Content.PublishButton.cshtml:7`. Save Draft: `.../Views/Content.SaveDraftButton.cshtml:7`. The other buttons: Unpublish, Delete: `.../Drivers/ContentsDriver.cs:112-120`; Preview: `src/OrchardCore.Modules/OrchardCore.ContentPreview/Views/ContentPreview.Button.cshtml:13`; Cancel: `.../Views/Content.Edit.cshtml:30`. |
| `draft-3` | Clear the highlights. Within the first second, click **Save Draft** (`button.draft[name="submit.Save"]`) with `ClickAndWaitForNavigationAsync`. When the list has loaded, highlight the two badges of the About row. | The list, with the About row at the top showing the badges Published and Draft, and the buttons Edit, View, Preview Draft and Actions. | Message: `.../Controllers/AdminController.cs:539`. Badges: `.../Views/ContentsTags_SummaryAdmin.cshtml:12,19`. |
| `draft-4` | Clear the highlights. Click **View** of the About row (`a.btn.view`) in the first second of the step. It opens a new browser tab (`target="_blank"`): wait for that page, and give it to `SwitchToAsync`. | The front end page About, in the new tab, with the unchanged subtitle `This is what I do.` in the page header. | View: `.../Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`. The page is drawn by `src/OrchardCore.Themes/TheBlogTheme/Views/Content-Article.liquid:16`. |
| `draft-5` | In the page that `draft-4` switched to, `NavigateAsync("/Admin/Contents/ContentItems")` and wait for the About row. Click **Preview Draft** of that row. It opens a new browser tab (`target="_blank"`): wait for that page, and give it to `SwitchToAsync`. | The preview tab, with the subtitle `We grow apples, pears and plums.` in the page header. | Preview Draft: `.../Views/ContentsButtonEdit_SummaryAdmin.cshtml:23`. Route and permission: `.../Startup.cs:226-232` and `.../Controllers/ItemController.cs:60`. |

### `03-publish` (recording; starts on `/Admin/Contents/ContentItems`; needs the end of `02-draft`, snapshot S1)

| Step | Actions | Visible when the narration ends | Labels from |
| --- | --- | --- | --- |
| `publish-1` | Click **Actions** in the About row. Highlight **Publish Draft** in the menu for about 3 seconds. At about the end of the first sentence, click **Publish Draft** with `ClickAndWaitForNavigationAsync` (no confirmation dialog opens). | The list, with the About row showing the badge Published only, and the buttons Edit, View and Actions (no Preview Draft). | Actions: `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml:32`. Publish Draft: `.../Contents/Views/ContentsButtonActions_SummaryAdmin.cshtml:19`. Message: `.../Contents/Controllers/AdminController.cs:716`. |
| `publish-2` | Click **View** of the About row (`a.btn.view`) in the first second of the step. It opens a new browser tab (`target="_blank"`): wait for that page, and give it to `SwitchToAsync`. | The front end page About, in the new tab, with the subtitle `We grow apples, pears and plums.` in the page header. | View: `.../Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`. `Content-Article.liquid:16`. |
| `publish-3` | `NavigateAsync("/Admin/Contents/ContentItems")`, click **Edit** in the About row, scroll to the Subtitle, triple-click it and press Delete, use `TypeAsync` to type `Closed for the winter`, press Tab, then click **Save Draft** (main button) with `ClickAndWaitForNavigationAsync`. Finish this within 11 seconds. | The list, with the About row showing the badges Published and Draft. | Save Draft: `.../Views/Content.SaveDraftButton.cshtml:7`. Message: `.../Controllers/AdminController.cs:539`. |
| `publish-4` | Click **Actions** in the About row, then **Discard Draft**. The dialog opens (title Discard Draft, text "Are you sure you want to discard this draft?"): leave it for about 2 seconds, then click `#modalOkButton` (Yes) with `WaitForNavigationAsync`. | The list, with the About row showing the badge Published only, and the buttons Edit, View and Actions. | Discard Draft and its dialog: `.../Views/ContentsButtonActions_SummaryAdmin.cshtml:30`. Message: `.../Controllers/AdminController.cs:648`. Yes: `.../ContentsButtonActions_SummaryAdmin.cshtml:30` (`data-ok-text`). |

### `04-unpublish` (recording; starts on `/Admin/Contents/ContentItems`; needs the end of `03-publish`, snapshot S2)

| Step | Actions | Visible when the narration ends | Labels from |
| --- | --- | --- | --- |
| `unpublish-1` | Click **Actions** in the About row, then **Unpublish**. The dialog opens (title Unpublish, text "Are you sure you want to unpublish this item?"): leave it for about 2 seconds, then click `#modalOkButton` (Yes) with `WaitForNavigationAsync`, at about the end of the first sentence. | The list, with the About row showing the badge Draft only. | Unpublish and its dialog: `.../Views/ContentsButtonActions_SummaryAdmin.cshtml:35`. Message: `.../Controllers/AdminController.cs:756`. |
| `unpublish-2` | Highlight the Draft badge of the About row (`.tags .ta-badge:has(.fa-pencil)`) while the second sentence starts, clear it, then highlight the row's buttons (`.admin-list-actions`: Edit, Preview Draft and Actions, with no View between them) at "the View button is gone" and leave the ring. | The About row with the Draft badge, no View button, and a ring around the row's buttons. | Draft: `.../Views/ContentsTags_SummaryAdmin.cshtml:19`. View is shown only for a published item: `.../Views/ContentsButtonEdit_SummaryAdmin.cshtml:16-19`. |
| `unpublish-3` | Clear the highlights. Click **Actions** in the About row, then click **Publish Draft** with `ClickAndWaitForNavigationAsync` (no dialog). | The About row with the badge Published only, and the buttons Edit, View and Actions. | `.../Views/ContentsButtonActions_SummaryAdmin.cshtml:19`. |

### `05-find` (recording; starts on `/Admin/Contents/ContentItems`; needs the end of `04-unpublish`, snapshot S3)

| Step | Actions | Visible when the narration ends | Labels from |
| --- | --- | --- | --- |
| `find-1` | Highlight the two badges of the Parking Information row for about 5 seconds, clear them, then highlight the badge of the Spring Event row and leave it. | The list, with the Spring Event row (badge Draft only) ringed, and the Parking Information row showing Published and Draft. | `.../Views/ContentsTags_SummaryAdmin.cshtml:12,19`. |
| `find-2` | Clear the highlight. The filter button is a funnel icon (`#filter-dropdown`) next to the search box; it has no text, only the tooltip Filters (`ContentsAdminListSearch.cshtml:17`), which a screencast does not show. Highlight it while "the funnel button next to the search box" is spoken, clear the highlight, then click it to open the menu (entries Only draft items, Only published items, Owned by me, Filter syntax). Leave the menu open for about 2 seconds. At about the end of the first sentence, click **Only draft items** with `ClickAndWaitForNavigationAsync`. | The list filtered by `status:Draft`: only the rows of Parking Information and Spring Event, and the search box showing `status:Draft`. | Filters, Only draft items: `.../Contents/Views/ContentsAdminListSearch.cshtml:17,20`. The filter: `.../Contents/Services/DefaultContentsAdminListFilterProvider.cs:29-31`. |

### `06-limits` (slides)

- `limits-1`: the heading "Who can do what", with four cards: Save Draft, Publish, Publish Draft and Unpublish, Discard
  Draft, and a user who can only edit. Permission labels: `src/OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:17-27`.
  Edit is implied by Publish (`CommonPermissions.cs:21`, third argument of `Permission`: the permissions that imply it).
  Discard Draft needs the Delete permission: `.../Contents/Controllers/AdminController.cs:637`.
- `limits-2`: the heading "Where older versions go", with three cards: stored in the database, Audit Trail (it records
  only the content types selected in its settings, none by default:
  `.../Contents/AuditTrail/Settings/ContentAuditTrailSettings.cs:5`, `.../AuditTrail/Handlers/AuditTrailContentHandler.cs:74`),
  Content Version Pruning.

### Cards

- `intro`: the opening card, "Drafts and Versions", with the subtitle "Change content without losing the live page".
- `outro`: the closing card, with the subtitle `docs.orchardcore.net`.

## 4. Checks for private data

- Only the invented values of this plan appear: About, Parking Information, Spring Event, the subtitles above, the
  user `admin` (the Author in the rows). No password, no email address other than `admin@example.com`, no token.
- The sign-in is not recorded; the password is never typed on camera.
- Look at every contact sheet: each message and dialog named in the table is on screen at the right time, no error
  page is shown, and no row other than the recipe's and the two seeded ones appears.
- OCR the video at one frame per second and at every scene change, and search for names, email addresses, phone numbers
  and tokens. Any address bar, host name or machine path must not be visible.

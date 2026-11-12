# Recording plan: Export Content From the List

Everything a person needs to record this video without deciding anything. The narration is in `storyboard.json`;
generate it first (`narrate.py`), because the recorder holds each step for the length of its narration.

How to read this plan:

- A selector written in this file stands for `scene.Page.Locator(...)`. Every selector was derived from the Orchard Core
  source named beside it; check each one once with `Recorder probe` on the running site before recording, and fix the
  selector (never the narration) if the markup differs.
- "Source" lines give the file and line where a label is written. Paths start at the module, theme or library folder
  (`OrchardCore.Contents/...` is `src/OrchardCore.Modules/OrchardCore.Contents/...`; `OrchardCore.Themes/...`,
  `OrchardCore.Cms.Web/...` and `OrchardCore/...` are under `src/` itself), in the Orchard Core checkout. Labels are
  typed exactly as the code spells them, including capitals (the row menu says "Add to Deployment Plan", the bulk menu
  says "Add to deployment plan").
- `ROW("Title")` is shorthand for `li.list-group-item:has(a:text-is("Title"))`, the row of the content items list
  that holds that title (the title is a link: `OrchardCore.Contents/Views/ContentsTitle_SummaryAdmin.cshtml:10`). The row menu is
  `ROW("Title") button.actions` (its text is "Actions"; `OrchardCore.Admin/Views/AdminListActions.cshtml:31`).
- A highlight is `scene.HighlightAsync(locator, caption)`. Clear it with `scene.ClearHighlightsAsync()` before the next
  navigation or scroll.
- "End frame" is what must be on screen when the narration of the step ends.

## 1. The site it needs

The site is a fresh local Orchard Core site with fake data only.

| Setting | Value |
|---|---|
| Host | `src/OrchardCore.Cms.Web` of an Orchard Core checkout at commit 92db4a83 (the commit every label below was read from), on `http://localhost:5010` |
| Data | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup (`OrchardCore__AutoSetup__Tenants__0__...`, the options are in `OrchardCore.AutoSetup/Options/TenantSetupOptions.cs`): `ShellName` = `Default`, `SiteName` = `Orchard Core`, `SiteTimeZone` = `America/Los_Angeles`, `AdminUsername` = `admin`, `AdminEmail` = `admin@example.com`, `AdminPassword` = `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git), `DatabaseProvider` = `Sqlite`, `RecipeName` = `Blog` |
| Sign-in | Not part of the story, so Auto Setup is used and `Recorder setup` saves `state.json` (git-ignored) |
| Outbound calls | Block them as the skill says (`HTTP_PROXY` and `HTTPS_PROXY` set to a dead address). Nothing in this video calls out: the only target is File Download |
| Admin list settings | Leave the defaults: list layout `List` and actions layout `Buttons`, which are the defaults of the code (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:34`, `:40`; the lines of `OrchardCore.Cms.Web/appsettings.json` that name them are comments). Do not set `OrchardCore:Admin:List` and do not change them in the admin settings. With the `Menu` actions layout the "Actions" button becomes an ellipsis button and every row selector below breaks |
| Remote deployment | Remote Deployment is enabled by the Blog recipe (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:58`). Create no remote instance and no remote client: "Available Targets" must list File Download only |

The Blog recipe enables the Deployment feature (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:57`) and none of the three features of this video
(`OrchardCore.Contents/Manifest.cs:47`, `:60`, `:68`). They stay disabled until clip `02-features`
enables them on camera, so check on `Tools, Features` that they are disabled before the take.

### Seed data (created outside the capture, before the first take)

Create the seed in the browser, signed in as `admin`. Do not use real names anywhere.

1. The deployment plan the add action fills. `Tools, Deployments, Plans` (page title "Deployment Plans"), select
   "Add Deployment Plan", type `Orchard Harvest Content` in "Name", select "Create"
   (`OrchardCore.Deployment/Views/DeploymentPlan/Index.cshtml:25`, `OrchardCore.Deployment/Views/DeploymentPlan/Create.cshtml:12`, `:20`). Add no step to it.
   Create no other plan: the plan the export action uses appears by itself when its feature is enabled (see
   `OPEN-QUESTIONS.md`).
2. Three published articles, created in this order so that the newest are first in the list (the list is sorted by
   "Recently modified" by default: `OrchardCore.Contents/Controllers/AdminController.cs:201`). On
   `Content, Content Items` (page title "Manage Content") use the "New" drop-down, choose "Article"
   (`OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`), type the title in "Title", select "Publish":
   1. `Cider Press Rota`
   2. `Pruning Guide`
   3. `Apple Harvest Schedule`
   Leave the other fields empty. Never choose "Save Draft" on these articles, and never edit them afterwards: an item
   with a pending draft shows other menu labels and is skipped by the bulk actions (see `OPEN-QUESTIONS.md`).
3. Check `Content, Content Items`: "Apple Harvest Schedule", "Pruning Guide" and "Cider Press Rota" are the first three
   rows, in that order, and each shows the "Published" status. The other rows (Blog recipe content) are fake and
   may stay.
4. Run `Recorder setup`, then stop the site and copy its `App_Data` folder and `state.json` aside as snapshot **S0**.

### Commands

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/deployment-content-actions/recorder
export VIDEO_ROOT=<repo>/videos/deployment-content-actions VIDEO_BASE_URL=http://localhost:5010
export VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password> MSYS_NO_PATHCONV=1
<bin>/Recorder setup
<bin>/Recorder record 02-features        # then the other clips, in the order of section 2
```

The scenes are one method per clip, with one `scene.StepAsync("<step id>", ...)` per step below, in this order. No
`PrepareAsync` is needed: the seed above is the whole preparation.

## 2. Order of the clips and the state they change

Record in the order of the storyboard. Each row says what the clip needs and what it leaves behind.

| Clip | Needs | Changes the site | Snapshot to restore before a new take |
|---|---|---|---|
| `02-features` | S0 (features disabled) | Enables the three features; the export feature creates the plan "Export content to deployment plan target" | S0. Save the result as **S2** |
| `03-download` | S2 | Nothing (downloads only) | S2 |
| `04-add` | S2 | Adds one Content Item step to "Orchard Harvest Content" | S2. Save the result as **S4a** |
| `04-many` | S4a | Adds two more steps to the same plan | S4a. Save the result as **S4b** |
| `05-settings` | S4b | Saves "Deployment Plan" in Settings, Deployment Targets | S4b. Save the result as **S5** |
| `06-export` | S5 | Nothing (downloads only) | S5 |
| `06-many` | S5 | Nothing (downloads only) | S5 |

To take a clip again: stop the site, restore the `App_Data` folder and `state.json` of the snapshot before it, start
the site, and record. To record everything again, restore S0 and start with `02-features`. `04-many` ends on the plan
page showing three steps only if `04-add` was recorded from S2 just before it. Delete the files the downloads leave in
the browser's download folder after each take.

### Downloads

Four steps start a file download: `download-3`, `download-4`, `export-3` and `exportmany-2`. A download is not a
navigation, so never use `ClickAndWaitForNavigationAsync` for them (it would wait for ever). Click with
`await scene.Page.RunAndWaitForDownloadAsync(async () => await scene.ClickAsync(<locator>));` and let the page stay
where it is. Headless Chromium draws no download bar, so the end frame is the unchanged list; the green caption
"Saved as a file" (or "Saved as a zip package") drawn with `HighlightAsync` is what shows the viewer that the
download happened.

## 3. Clips

### Chapter 00, opening card, `00-intro`

- `intro`: card. Nothing to record. The card shows the title "Export Content From the List" and the subtitle
  "Download, add to a plan, or send".

### Chapter 01, `01-actions` (slides)

Drawn by `slides.py`; nothing to record. All four slides have the heading "Three actions on the content items list"
and the same four cards, with one bright card each:

- `actions-1`: the card "View Or Download Content As JSON" is bright.
- `actions-2`: the card "Add Content To Deployment Plan" is bright.
- `actions-3`: the card "Export Content To Deployment Target" is bright.
- `actions-4`: the card "Permissions" is bright.

### Chapter 02, `02-features`

Starts on `/Admin/Contents/ContentItems` (opened before the capture), sidebar visible, no menu open, features still
disabled (S0).

- `features-1`
  - Actions: click the sidebar button `#ta-left-sidebar button:text-is("Tools")`; click the link
    `#ta-left-sidebar a:text-is("Features")` with `ClickAndWaitForNavigationAsync`; type
    `content items list` into `#search-box` (`TypeAsync`, 40 ms per character). Wait until exactly three rows are
    shown: `[data-filter-value]:not(.d-none)` has count 3.
  - End frame: the Features page (title "Features") with the search box holding `content items list` and three rows,
    each with the badge "Disabled" and a blue "Enable" button, under the group "Content Management", in this order
    (the rows of a group are sorted by name): "Add Content To Deployment Plan", "Export Content To Deployment Target" and
    "View Or Download Content As JSON".
  - Source: menu `OrchardCore.Features/AdminMenu.cs:26`, `:27`; search `OrchardCore.Features/Views/Admin/Features.cshtml:27`; the sort by
    name `OrchardCore.Features/Controllers/AdminController.cs:94`; the row
    filter is the name, the id and the description (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`); names
    `OrchardCore.Contents/Manifest.cs:48`, `:61`, `:69`; "Disabled" `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`.
- `features-2`
  - Actions: highlight the dependency badges of each of the three rows in turn, for 0.6 s each, then clear:
    `[data-filter-value*="Add Content To Deployment Plan"] .direct-dependencies`,
    `[data-filter-value*="Export Content To Deployment Target"] .direct-dependencies` and
    `[data-filter-value*="View Or Download Content As JSON"] .direct-dependencies`; click the
    three checkboxes `input[name='featureIds'][value='OrchardCore.Contents.Deployment.Download']`,
    `input[name='featureIds'][value='OrchardCore.Contents.Deployment.AddToDeploymentPlan']` and
    `input[name='featureIds'][value='OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget']` (pause 400 ms
    after each); click `#bulk-action-menu-button` (text "Bulk Actions").
  - End frame: the three checkboxes are checked and the "Bulk Actions" menu is open, showing "Disable" and "Enable".
  - Source: dependencies badges `OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12`. The badges
    of the add row and of the download row read "Contents" and "Deployment"; the export row has a third one, "Recipes Core
    Services" (`OrchardCore.Contents/Manifest.cs:52` to `:54`, `OrchardCore.Recipes/Manifest.cs:24`). The narration says only
    that each lists Deployment; the first selector uses the name in `data-filter-value`, which also holds the id and the
    description. Checkbox `OrchardCore.Features/Views/FeatureEntry.Checkbox.SummaryAdmin.cshtml:11`; menu
    `OrchardCore.Features/Views/Admin/Features.cshtml:34`, `:38`.
- `features-3`
  - Actions: click `a.dropdown-item[data-action='Enable']` (text "Enable") with `ClickAndWaitForNavigationAsync`. The
    tenant restarts, so the reload can take a few seconds. Type `content items list` into `#search-box` again, and
    wait until three rows are shown and each holds the badge "Enabled"
    (`[data-filter-value]:not(.d-none):has-text("Enabled")` has count 3); wrap that wait in `OffCameraAsync` so the
    restart is cut out of the clip.
  - End frame: the same three rows, each with the badge "Enabled" and a red "Disable" button.
  - Source: `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`; the form posts with `Force` set
    (`OrchardCore.Features/Views/Admin/Features.cshtml:49`).
  - After the take: copy the site aside as **S2**, and check `Tools, Deployments, Plans` lists "Export content to
    deployment plan target" (see `OPEN-QUESTIONS.md`, question 1).

### Chapter 03, `03-download`

Starts on `/Admin/Contents/ContentItems` (S2), no menu open. The row is `ROW("Apple Harvest Schedule")`.

- `download-1`
  - Actions: click `ROW("Apple Harvest Schedule") button.actions`; highlight (no caption)
    `ROW("Apple Harvest Schedule") a.dropdown-item:text-is("View as JSON")` and
    `ROW("Apple Harvest Schedule") a.dropdown-item:text-is("Download as JSON")`.
  - End frame: the row menu is open. Below the entries of the row (such as "Unpublish", "Delete" and "Clone") and
    after a divider come "View as JSON" and "Download as JSON", each with a green ring, then "Add to Deployment Plan" (and, if the export plan was already chosen by the
    migration, "Export to Deployment Target" after it; see `OPEN-QUESTIONS.md`, question 1).
  - Source: `OrchardCore.Contents/Views/Items/Download-Button-Actions.SummaryAdmin.cshtml:25`, `:26`; the order is set by
    the positions `ActionsMenu:10`, `:20`, `:30`, `:40` of the drivers (`OrchardCore.Contents/Deployment/Download/DownloadContentDriver.cs:30`,
    `OrchardCore.Contents/Deployment/AddToDeploymentPlan/AddToDeploymentPlanContentDriver.cs:23`,
    `OrchardCore.Contents/Deployment/ExportContentToDeploymentTarget/ExportContentToDeploymentTargetContentDriver.cs:45`).
- `download-2`
  - Actions: clear the highlights; click `ROW("Apple Harvest Schedule") a.dropdown-item:text-is("View as JSON")` with
    `ClickAndWaitForNavigationAsync`; wait for `.CodeMirror` to be shown; pause 1.5 s. Do not scroll the editor and do
    not click in it.
  - End frame: the page "View As JSON Article" (in the page title, and "Manage Content" before it): a text editor with
    line numbers shows the item from its first line, a copy icon at its top right, and under it the green "Download"
    button and the grey "Cancel" button.
  - Source: title `OrchardCore.Contents/Views/Download/Display.cshtml:30` (the type name is "Article"), "Manage
    Content" `:31`, "Download" `:48`, "Cancel" `:52`, copy icon `:40`; the page is `DownloadController.Display`
    (`OrchardCore.Contents/Deployment/Download/DownloadController.cs:29`).
  - The editor shows JSON. The narration never reads it, and the take must not dwell on it beyond this step.
- `download-3`
  - Actions: click `button.download` (text "Download") inside the `RunAndWaitForDownloadAsync` pattern of section 2;
    pause 0.9 s; highlight `button.download` with the caption `Saved as a file`; pause 1.2 s; clear; click
    `a.btn.cancel` (text "Cancel") with `ClickAndWaitForNavigationAsync`.
  - End frame: the content items list again (title "Manage Content"), no menu open, "Apple Harvest Schedule" in the
    first row.
  - Source: the download is a POST that returns a file named after the content item id
    (`OrchardCore.Contents/Deployment/Download/DownloadController.cs:83`); the Cancel link exists because the list passes `returnUrl`
    (`OrchardCore.Contents/Views/Items/Download-Button-Actions.SummaryAdmin.cshtml:25`).
- `download-4`
  - Actions: click `ROW("Apple Harvest Schedule") button.actions`; click
    `ROW("Apple Harvest Schedule") a.dropdown-item:text-is("Download as JSON")` inside the
    `RunAndWaitForDownloadAsync` pattern; pause 0.9 s; highlight `ROW("Apple Harvest Schedule")` with the caption
    `Saved as a file`.
  - End frame: the list, the row menu closed (the link closes it: `OrchardCore.Contents/Assets/ts/download-button-actions.ts:3`), the row
    ringed with the caption "Saved as a file".
  - Source: `OrchardCore.Contents/Views/Items/Download-Button-Actions.SummaryAdmin.cshtml:26`.

### Chapter 04, `04-add` (from S2)

Starts on `/Admin/Contents/ContentItems`, no menu open.

- `add-1`
  - Actions: click `ROW("Apple Harvest Schedule") button.actions`; click
    `ROW("Apple Harvest Schedule") button.dropdown-item:text-is("Add to Deployment Plan")`; wait for
    `#modalAddToDeploymentPlanActionDeploymentPlan.show`. Open this dialog once per page load: opening it, cancelling
    and opening it again stacks click handlers on "Select" (`OPEN-QUESTIONS.md`, question 6).
  - End frame: the dialog "Available Deployment Plans" with two cards, "Export content to deployment plan target" and
    "Orchard Harvest Content", each with a blue "Select" button, and a "Cancel" button in the footer.
  - Source: `OrchardCore.Contents/Views/Items/AddToDeploymentPlan-Button-Actions.SummaryAdmin.cshtml:7`;
    `OrchardCore.Contents/Views/Items/AddToDeploymentPlan-ActionDeploymentPlan.Modal.cshtml:15`, `:31`, `:39`. The entry appears only for a user who has
    both "Manage deployment plans" and "Export Data" (`OrchardCore.Deployment/DeploymentPlanService.cs:38`).
- `add-2`
  - Actions: highlight (no caption) `#modalAddToDeploymentPlanActionDeploymentPlan .card:has(h4.card-title:text-is("Orchard Harvest Content"))`.
    Do not click "Select" yet, and never select the card "Export content to deployment plan target" (it would change
    what every later export sends).
  - End frame: the dialog still open, with a green ring around the card "Orchard Harvest Content".
  - Source: same as `add-1`.
- `add-3`
  - Actions: click `#modalAddToDeploymentPlanActionDeploymentPlan .card:has(h4.card-title:text-is("Orchard Harvest Content")) button.add-to-deployment-plan`
    (text "Select") with `ClickAndWaitForNavigationAsync`; as soon as the list is back, highlight
    `.toast.message-success` (no caption). The toast disappears after five seconds, so do not pause before the highlight.
  - End frame: the content items list with the green toast "Content added successfully to the deployment plan.".
  - Source: `OrchardCore.Contents/Deployment/AddToDeploymentPlan/AddToDeploymentPlanController.cs:88`; the redirect goes back to the list
    (`:45` and the `returnUrl` of the dialog, `OrchardCore.Contents/Views/Items/AddToDeploymentPlan-ActionDeploymentPlan.Modal.cshtml:28`); five
    seconds: `OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`.
- `add-4`
  - Actions: clear the highlight; click `#ta-left-sidebar button:text-is("Tools")`; click
    `#ta-left-sidebar button:text-is("Deployments")`; click `#ta-left-sidebar a:text-is("Plans")` with
    `ClickAndWaitForNavigationAsync`; click `ROW_PLAN("Orchard Harvest Content") a:text-is("Manage Steps")` with
    `ClickAndWaitForNavigationAsync`, where `ROW_PLAN("Name")` is `li.list-group-item:has-text("Name")`. Use pauses of
    250 ms; this step runs a little longer than its narration, which is acceptable.
  - End frame: the page "Deployment Plan" with the buttons "Execute" and "Add Step", and one step on the list.
  - Source: menu `OrchardCore.Deployment/AdminMenu.cs:18`, `:19`, `:20`; "Deployment Plans" `OrchardCore.Deployment/Views/DeploymentPlan/Index.cshtml:6`;
    "Manage Steps" `OrchardCore.Deployment/Views/DeploymentPlanEntry.Buttons.SummaryAdmin.cshtml:6`; "Execute" `OrchardCore.Deployment/Views/DeploymentPlan/Display.cshtml:25`,
    "Add Step" `:31`.
- `add-5`
  - Actions: highlight `#stepOrder li:has(h5:text-is("Content Item"))` for 2 s, then clear and highlight
    `#stepOrder .badge:text-is("Apple Harvest Schedule")`.
  - End frame: the step "Content Item" with the badge "Apple Harvest Schedule" ringed in green, and the "Edit" and
    "Delete" buttons of the step on its right.
  - Source: `OrchardCore.Contents/Views/Items/ContentItemDeploymentStep.Fields.Summary.cshtml:12`; the badge shows the
    item's title, or "Nothing here!" when the item cannot be loaded (`:19`); the step stores only the item id and loads
    the published version each time the plan runs (`OrchardCore.Contents/Deployment/AddToDeploymentPlan/ContentItemDeploymentSource.cs:24`).
  - After the take: copy the site aside as **S4a**.

### Chapter 04, `04-many` (from S4a)

Starts on `/Admin/Contents/ContentItems`, no menu open.

- `addmany-1`
  - Actions: click `ROW("Pruning Guide") input[name='itemIds']`, then `ROW("Cider Press Rota") input[name='itemIds']`
    (pause 500 ms after each); wait for `#actions` to be shown (it appears when two or more rows are checked); highlight
    `#selected-items` (it reads "2 selected") for 1 s and clear; click `#bulk-action-menu-button` (text "Actions");
    click `#addToDeploymentPlanBulkActions` (text "Add to deployment plan").
  - End frame: the confirmation dialog (title "Bulk Action", message "Are you sure you want to add these items to a
    deployment plan?", buttons "Ok" and "Cancel") over the list with two checked rows. Behind it, the bulk menu listed
    "Publish Now", "Unpublish", "Delete", "Add to deployment plan" and "Export to deployment target" (the last one only
    once the export plan is chosen).
  - Source: `OrchardCore.Contents/Views/ContentsAdminList.Fields.BulkActions.cshtml:5`, `:6`; the count label `OrchardCore.Contents/Views/ContentsAdminListSummary.cshtml:8`; the menu appears only for two or more checked
    rows (`OrchardCore.Contents/Assets/ts/add-to-deployment-plan-bulk.ts:11`; the same rule is in the compiled list script
    `OrchardCore.Contents/wwwroot/Scripts/contents-admin-list/contents-admin-list.js`); `OrchardCore.Contents/Views/Items/AddToDeploymentPlan-Button-ContentsBulkActions.cshtml:1`;
    "Publish Now" `OrchardCore.Contents/Controllers/AdminController.cs:208`; dialog defaults `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`.
    Select exactly these two rows, both published with no draft.
- `addmany-2`
  - Actions: highlight `#confirmRemoveModal .modal-body` for 1.5 s, clear; click `#modalOkButton` (text "Ok"); wait for
    `#modalAddToDeploymentPlanContentsBulkActionsDeploymentPlan.show`; highlight (no caption)
    `#modalAddToDeploymentPlanContentsBulkActionsDeploymentPlan .card:has(h4.card-title:text-is("Orchard Harvest Content"))`.
  - End frame: the dialog "Available Deployment Plans" with the card "Orchard Harvest Content" ringed, and the two
    checked rows dimmed behind it.
  - Source: `OrchardCore.Contents/Views/Items/AddToDeploymentPlan-ContentsBulkActionsDeploymentPlan.Modal.cshtml:11`, `:26`.
- `addmany-3`
  - Actions: click `#modalAddToDeploymentPlanContentsBulkActionsDeploymentPlan .card:has(h4.card-title:text-is("Orchard Harvest Content")) button.add-to-deployment-plan-bulk-actions`
    (text "Select") with `ClickAndWaitForNavigationAsync`; highlight `.toast.message-success` for 0.8 s; clear;
    `NavigateAsync("/Admin/DeploymentPlan/Index")`; click `ROW_PLAN("Orchard Harvest Content") a:text-is("Manage Steps")`
    with `ClickAndWaitForNavigationAsync`.
  - End frame: the page "Deployment Plan" with three steps "Content Item", whose badges read "Apple Harvest Schedule",
    "Pruning Guide" and "Cider Press Rota" (the order may differ).
  - Source: the toast `OrchardCore.Contents/Deployment/AddToDeploymentPlan/AddToDeploymentPlanController.cs:88` (the bulk action, `:96`, adds one step per item, `:117`
    keeps the published ones).
  - After the take: copy the site aside as **S4b**.

### Chapter 05, `05-settings` (from S4b)

Starts on `/Admin/Contents/ContentItems`, no menu open.

- `settings-1`
  - Actions: click `#ta-left-sidebar button:text-is("Tools")`, `#ta-left-sidebar button:text-is("Deployments")`, then
    `#ta-left-sidebar a:text-is("Plans")` (with `ClickAndWaitForNavigationAsync`), then
    `ROW_PLAN("Export content to deployment plan target") a:text-is("Manage Steps")` (with
    `ClickAndWaitForNavigationAsync`). Use pauses of 250 ms.
  - End frame: the page "Deployment Plan" for the export plan, with one step.
  - Source: the plan name `OrchardCore.Contents/Migrations/exportcontenttodeploymenttarget.recipe.json:19`; the menu as in `add-4`.
- `settings-2`
  - Actions: highlight `#stepOrder h5:text-is("Export Content To Deployment Target")` for 2 s; clear; highlight
    `#stepOrder .hint`.
  - End frame: the step heading "Export Content To Deployment Target" and, under it, the grey hint "Allows the export
    content to deployment target feature to add content items to the plan.", ringed.
  - Source: `OrchardCore.Contents/Views/Items/ExportContentToDeploymentTargetDeploymentStep.Fields.Summary.cshtml:3`, `:5`.
- `settings-3`
  - Actions: clear; click `#ta-left-sidebar button:text-is("Settings")`; click
    `#ta-left-sidebar a:text-is("Deployment Targets")` with `ClickAndWaitForNavigationAsync`; choose
    "Export content to deployment plan target" in `select[name$='ExportContentToDeploymentTargetPlanId']` (`SelectAsync`;
    do it even when the field already shows that plan);
    click `button.save` (text "Save") with `ClickAndWaitForNavigationAsync`.
  - End frame: the page "Settings" reloaded after saving, with the toast "Site settings updated successfully." and the
    field "Deployment Plan" showing "Export content to deployment plan target".
  - Source: menu `OrchardCore.Contents/Deployment/ExportContentToDeploymentTarget/ExportContentToDeploymentTargetAdminMenu.cs:26`, `:27`
    (the item appears for users with "Manage deployment plans"); field `OrchardCore.Contents/Views/Items/ExportContentToDeploymentTargetSettings.Edit.cshtml:21`;
    when no plan has the step, the page shows the warning on `:14` instead of the list; "Save"
    `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; toast `OrchardCore.Settings/Controllers/AdminController.cs:119`.
  - After the take: copy the site aside as **S5**, and check that "Export to Deployment Target" appears in the row
    menu of the content items list. It may already have appeared after `02-features`, if enabling the export feature
    chose its plan by itself (`OPEN-QUESTIONS.md`, question 1); the narration of the earlier clips does not depend on
    its absence, so no earlier clip is recorded again for that.

### Chapter 06, `06-export` (from S5)

Starts on `/Admin/Contents/ContentItems`, no menu open.

- `export-1`
  - Actions: click `ROW("Apple Harvest Schedule") button.actions`; highlight
    `ROW("Apple Harvest Schedule") button.dropdown-item:text-is("Export to Deployment Target")` for 1 s and clear; click
    it; wait for `#modalExportContentToDeploymentTargetActionTarget.show`.
  - End frame: the dialog "Available Targets" with one card, "File Download", its description "Download a deployment
    plan locally.", a blue "Select" button, and "Cancel" in the footer. Open the dialog once per page load.
  - Source: `OrchardCore.Contents/Views/Items/ExportContentToDeploymentTarget-Button-Actions.SummaryAdmin.cshtml:21`;
    `OrchardCore.Contents/Views/Items/ExportContentToDeploymentTarget-ActionDeploymentTarget.Modal.cshtml:12`, `:27`, `:30`; the entry is shown only when
    the plan of Settings, Deployment Targets is set (`OrchardCore.Contents/Deployment/ExportContentToDeploymentTarget/ExportContentToDeploymentTargetContentDriver.cs:36`);
    "File Download" `OrchardCore.Deployment/Services/FileDownloadDeploymentTargetProvider.cs:20`, `:21`.
- `export-2`
  - Actions: highlight (no caption) `#modalExportContentToDeploymentTargetActionTarget .card:has(h4.card-title:text-is("File Download"))`.
  - End frame: the card "File Download" ringed in green, the dialog still open.
  - Source: as `export-1`. Remote instances would be further cards with the description "Sends the deployment plan to a
    remote instance." (`OrchardCore.Deployment.Remote/Services/RemoteInstanceDeploymentTargetProvider.cs:27`); none is configured.
- `export-3`
  - Actions: clear; click `#modalExportContentToDeploymentTargetActionTarget button.export-to-deployment-target` (text
    "Select") inside the `RunAndWaitForDownloadAsync` pattern; pause 0.9 s; highlight `ul.admin-list-list` with the
    caption `Saved as a zip package`.
  - End frame: the dialog closed, the list unchanged, a green ring around it with the caption "Saved as a zip package".
  - Source: the dialog posts to the plan's execute action, which returns a zip file
    (`OrchardCore.Deployment/Controllers/ExportFileController.cs:108`).

### Chapter 06, `06-many` (from S5)

Starts on `/Admin/Contents/ContentItems`, no menu open.

- `exportmany-1`
  - Actions: click `ROW("Pruning Guide") input[name='itemIds']`, then `ROW("Cider Press Rota") input[name='itemIds']`
    (pause 500 ms after each); wait for `#actions`; click `#bulk-action-menu-button`; click
    `#exportContentToDeploymentTargetBulkActions` (text "Export to deployment target").
  - End frame: the confirmation dialog (title "Bulk Action", message "Are you sure you want to export these items?",
    buttons "Ok" and "Cancel") over the list with two checked rows.
  - Source: `OrchardCore.Contents/Views/Items/ExportContentToDeploymentTarget-Button-ContentsBulkActions.cshtml:1`; two rows at least:
    `OrchardCore.Contents/Assets/ts/export-to-deployment-target-bulk.ts:11`.
- `exportmany-2`
  - Actions: click `#modalOkButton` (text "Ok"); wait for
    `#modalExportContentToDeploymentTargetContentBulkActionsTarget.show`; click
    `#modalExportContentToDeploymentTargetContentBulkActionsTarget button.export-to-deployment-target-bulk-actions`
    (text "Select") inside the `RunAndWaitForDownloadAsync` pattern; pause 0.9 s; highlight `ul.admin-list-list` with the
    caption `Saved as a zip package`.
  - End frame: the dialogs closed, the list with two checked rows, ringed with the caption "Saved as a zip package".
  - Source: `OrchardCore.Contents/Views/Items/ExportContentToDeploymentTarget-ContentsBulkActionsDeploymentTarget.Modal.cshtml:8`, `:27`.
  - After the take: open the two zip files (outside the capture) to confirm that the first holds the item "Apple Harvest
    Schedule" and the second holds both selected items.

### Chapter 99, closing card, `99-outro`

- `outro`: card. Nothing to record. The card shows the title "Export Content From the List" and the subtitle
  "docs.orchardcore.net".

## 4. Checks for private data

- The only user on the site is `admin` with the address `admin@example.com`; no clip shows the user's address, and no
  clip shows the password. The password is never typed on camera.
- Every title and name on screen is the fake data above or the Blog recipe content. The View As JSON page shows
  identifiers and the author name `admin` only.
- The takes leave downloaded files in the browser's download folder: they are not part of the video and are never
  committed.
- OCR the video at one frame per second and at every scene change, as the skill says, and search for `@`, the machine's
  user name, folder paths (`C:\`), and any name that is not in the seed above. The folder in which the site's data
  lives is never shown.

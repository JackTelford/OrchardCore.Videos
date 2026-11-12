# Recording plan: workflows-overview

Everything a person needs to record this video without deciding anything. Paths such as `src/OrchardCore.Modules/...`
are in the Orchard Core checkout (`OrchardCMS/OrchardCore`) the site is built from; `W` stands for
`src/OrchardCore.Modules/OrchardCore.Workflows`, `D` for `W/Assets/designer/src`, `C` for
`src/OrchardCore.Modules/OrchardCore.Contents`, and `Localizer.cs` for `W/Services/WorkflowsDesignerJSLocalizer.cs`
(the labels of the designer). A selector in this plan stands for `scene.Page.Locator(...)`.
The labels were read from the source at the commit recorded in `catalog/SOURCE.md`; `OPEN-QUESTIONS.md` lists what a
person must confirm on the running site.

## The site it needs

- **Build and run** `src/OrchardCore.Cms.Web` of an Orchard Core checkout at the commit of `catalog/SOURCE.md` (or later
  on the same branch, if the labels below still match), on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at
  an empty folder outside both repositories. Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead
  address; this video needs no external service, so there is nothing to emulate.
- **Auto Setup**, so no Setup screen and no sign-in form are recorded (environment variables, one tenant):

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (chosen outside git, the same as `VIDEO_PASSWORD`) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Tenant:** `Default` only. No other tenant.
- **Features before recording:** only what the Blog recipe enables (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`,
  step `feature`). Check on `/Admin/Features` that **Workflows**, **HTTP Workflows Activities**, **Timer Workflows
  Activities** and **Session Workflows Activities** are all disabled: the first clip enables **Workflows** on camera,
  and the other three stay disabled for the whole video. **Contents** and **Users** are enabled by the recipe.
- **Settings:** none to change. Leave the admin theme, the site settings and the admin list layouts at the recipe's
  defaults (list layout `List`, row actions as `Buttons`).
- **Seed data:** none to create. The Blog recipe creates the Article the video publishes, titled "About"
  (`blog.recipe.json`, step `content`, content type `Article`). The Article type is listable, so **Content Items** lists
  it (a Blog Post is not, `Listable` is not set for it in the recipe, so it would not show there). It is sample content
  shipped with Orchard Core and holds no personal data. The workflow, its name and its message are created on camera.
- **Sign in** once outside the capture: `export VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>`, then `Recorder setup`
  saves `state.json` (git-ignored). The sidebar of the admin menu is expanded in `state.json`, so it is expanded at the
  start of every clip.
- **No mask script** is needed: the site is made from scratch and shows no real data.
- **Browser and recorder:** as in `references/recording.md` (1536x864, device scale 1.25, light, `en-US`). Run
  `scripts/narrate.py videos/workflows-overview` first, so the recorder holds each step for its narration. Set
  `MSYS_NO_PATHCONV=1` in Git Bash.

## Order, dependencies and snapshots

Record the clips in the order they play. Each one starts from the state the previous one left.

| Clip | Needs | Changes the site | Snapshot to take afterwards |
|---|---|---|---|
| `03-enable` | the fresh Blog site | Yes: enables **Workflows** (the tenant restarts) | `S3` |
| `04-create` | `S3` | Yes: creates the workflow, with no activity and no draft (Orchard Core saves its version 1, an empty definition, at creation) | `S4` |
| `05-designer` | `S4` | No | none (`S4` is still current) |
| `06-build` | `S4` | Yes: fills and publishes the workflow | `S6` |
| `07-run` | `S6` | Yes: publishes the Article again (a new version) and creates an instance | `S7` |
| `08-instances` | `S7` | No | none |

A snapshot is a copy of the `App_Data` folder, made with the site stopped (`S0` is the fresh site, before `03-enable`).
To record the whole video again, stop the site, restore `S0`, and start it. To record one clip again, stop the site,
restore the snapshot of the clip before it, and start it. In particular, `06-build` must start from `S4`, where the
workflow has no activity, and `07-run` must start from `S6`, where the workflow is published.

Time budgets: a step's actions must end before its narration does (`durations.json` has the length of each step). If
a navigation takes longer than the narration of its step, shorten the pauses of that step, not the narration.

## Clip `03-enable` (chapter 03, short `enable`)

**Starts on:** `/Admin`, the admin home, signed in as `admin`. Admin menu expanded (the default). Before
`StartCaptureAsync`, nothing else.

- `enable-1`: Click the **Tools** entry of the admin menu (it expands), then **Features** (click and wait for the
  navigation to `/Admin/Features`). Click the search field (`#search-box`, placeholder "Search") and type `workflows`.
  Wait until the visible group headings (`.feature-group:not(.d-none) h3`) read exactly "Data" and then "Workflows" (the page
  sorts the groups by category name, so **Data** comes first: `.../OrchardCore.Features/Controllers/AdminController.cs:91`)
  and `#list-summary` (it reads "Showing X of N features", and X changes as the list is filtered) has stopped changing.
  Both groups fit in the 864 px viewport; if the **Workflows** group is cut at the bottom, scroll it into view. The search matches the name, the identifier
  and the description of each feature (`.../OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`, filter in
  `Assets/ts/features-index.ts`, applied on every keystroke), so two groups are expected to stay: **Workflows** (its four
  features) and **Data**, whose **Data Pipelines** row matches because its description mentions workflows
  (`src/OrchardCore.Modules/OrchardCore.DataPipelines/Manifest.cs:23`). That makes "Showing 5 of N features"; confirm it
  on the running site. *Visible when the narration ends:* the **Features** page, `workflows` in the search field, the **Data** group
  with its one row above the **Workflows** group with its four rows. *Labels:* `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-28`
  (Tools, Features); `.../OrchardCore.Features/Views/Admin/Features.cshtml:27` (search field), `:57-59` (summary).
- `enable-2`: Highlight the group of the **Workflows** heading (the `div.feature-group` whose `h3` reads "Workflows")
  with `HighlightAsync`, no caption. *Visible:* the green ring around the heading and its rows. The rows of a group are
  sorted by name (`.../OrchardCore.Features/Controllers/AdminController.cs:94`), so they read, from the top,
  **HTTP Workflows Activities**, **Session Workflows Activities**, **Timer Workflows Activities** and **Workflows**, and the
  narration names the three activity features in that order. *Labels:* `W/Manifest.cs:13-38` (the four feature names and the `Workflows` category); `Features.cshtml:87-88` (the
  group heading is the category).
- `enable-3`: `ClearHighlightsAsync`. Click **Enable** in the **Workflows** row (the last row of its group; the link `#btn-enable-OrchardCore_Workflows`;
  its id is `btn-enable-` followed by the feature id with the dots replaced by underscores: confirm with
  `Recorder probe`). Do not find the row by the text "Workflows", which is also in the three other names. Do not click the other three. The link posts and redirects: click it with `ClickAndWaitForNavigationAsync`, so the
  screencast follows the new page. The tenant restarts, which takes a few seconds: use `OffCameraAsync` for the wait
  until the page text contains "The feature Workflows was enabled." (the wait is cut from the clip; the condition is
  specific to the new page, so it is not met before the redirect). The redirect goes to the plain **Features** page, with no search
  (`.../OrchardCore.Features/Controllers/AdminController.cs:290-312`), so the whole list is shown and the **Workflows** group is far below the
  notification. Once the page text contains the notification, click the search field (`#search-box`) and type `workflows` again; wait until
  `#list-summary` has stopped changing. *Visible when the narration ends:* the **Features** page with the green success
  notification "The feature Workflows was enabled." at the top, `workflows` in the search field, the **Data** group, and the
  **Workflows** group whose **Workflows** row now shows **Disable** (the other three rows still show **Enable**).
  *Labels:* `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (**Enable**),
  `.../Controllers/AdminController.cs:226-248` and `:317` (the notification text).

## Clip `04-create` (chapter 04, short `create`)

**Starts on:** `/Admin` (open it before the capture), admin menu expanded. The **Design** group and its **Workflows**
entry exist now that the feature is enabled.

- `create-1`: Click **Design** in the admin menu (it expands), then **Workflows** (click and wait for the navigation to
  `/Admin/Workflows/Types`). *Visible when the narration ends:* the **Workflows** page, with its search field, the
  **Instances** and **Create Workflow** buttons, and the message "Nothing here! There are no workflow types for the
  moment." *Labels:* `W/AdminMenu.cs:18-23` (Design, Workflows); `W/Views/WorkflowType/Index.cshtml:24-25`
  (**Instances**, **Create Workflow**), `:34` (the empty message).
- `create-2`: Click **Create Workflow** (`a.btn.create`; click and wait for the navigation to
  `/Admin/Workflows/Types/Create`). Click the **Name** field (`input#Name`, it has the focus) and type
  `Notify the editors when an article is published`. *Visible:* the **Create Workflow** page, the name typed in the
  field, and the hint under it, "What the workflow does, such as "Notify the editors when an article is published".",
  and the **More settings** line below. *Labels:* `W/Views/WorkflowType/Create.cshtml:21` (**Name**), `:25` (hint), `:30`
  (**More settings**). The hint's wording comes from the code; the typed name is the example of the hint, as this video's fake data.
- `create-3`: Click **More settings** (`details[data-cy="create-more-settings"] summary`) so it opens. Highlight the
  **Enabled** checkbox (`input#IsEnabled`) and the **Instances at a time** drop-down (`select[data-cy="instance-limit"]`;
  leave its value, **Any number**). *Visible:* the
  opened section with the hint "The defaults suit most workflows. You can change these settings later, on the
  designer's Workflow tab.", **Enabled** checked, **Instances at a time** set to **Any number**, and the two green
  rings. *Labels:* `W/Views/WorkflowType/Create.cshtml:29-58` (**More settings**, the hint, **Enabled**, **Instances at a time**). **Enabled**
  is checked and **Instances at a time** reads **Any number** because the Create action fills the form with `IsEnabled = true`
  and leaves the model's default `InstanceLimit` (`W/Controllers/WorkflowTypeController.cs:242-256`, `W/ViewModels/WorkflowTypePropertiesViewModel.cs:12-33`).
- `create-4`: `ClearHighlightsAsync`, scroll down until **Create and open the designer** is visible, and click it (click
  and wait for the navigation to `/Admin/Workflows/Types/Edit/<id>`). Wait until `[data-cy="start-picker"]` is visible.
  *Visible when the narration ends:* the workflow designer with an empty canvas and the "Start with an event" panel,
  the workflow's name in the toolbar. *Labels:* `W/Views/WorkflowType/Create.cshtml:131` (**Create and open the
  designer**); `W/Controllers/WorkflowTypeController.cs:277` (the redirect to the designer); `D/canvas/StartPicker.vue:27`.

## Clip `05-designer` (chapter 05, short `designer`)

**Starts on:** the designer of the empty workflow. Before the capture: open `/Admin/Workflows/Types`, click the link
**Notify the editors when an article is published**, wait until `[data-cy="start-picker"]` is visible, click the
**Issues** tab of the workflow panel (`[data-cy="panel-tab-issues"]`; see "Code on screen" below), then click
`button.leftbar-compactor` (title "Collapse / expand menu", `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:79`)
so the designer has the whole width. Nothing in this clip changes the site.

- `designer-1`: `HighlightAsync` on `[data-cy="designer-toolbar"]`. Move the cursor to the **Workflows** link
  (`[data-cy="toolbar-list"]`) without clicking. *Visible:* the toolbar ringed, showing **Workflows** / the workflow's
  name, a "Version 1" badge next to the name, the history buttons, **Versions**, **Instances**, **Export**, **Run…**,
  **Discard draft** and **Publish** (the last two are greyed out when the draft has no change). The badge shows because
  Orchard Core saves the version 1 of a workflow, an empty definition, when the workflow is created
  (`W/Controllers/WorkflowTypeController.cs:271-277` calls `IWorkflowTypeStore.SaveAsync`, which calls `CreateIfChangedAsync`,
  `W/Services/WorkflowTypeStore.cs:87`, `W/Services/WorkflowTypeVersionStore.cs:90-114`; the badge is `D/App.vue:648-656`,
  fed by `W/Controllers/WorkflowDesignerController.cs:994-997`). The narration does not mention it. *Labels:*
  `D/App.vue:643` (**Workflows** link), `:755` (**Discard draft**), `:759` (**Publish**);
  `W/Services/WorkflowsDesignerJSLocalizer.cs:33,190,200,206,208,212,218,286`.
- `designer-2`: `ClearHighlightsAsync`, then `HighlightAsync` on `[data-cy="designer-toolbox"]`. Click the **Content**
  category heading (`[data-cy="toolbox-category-Content"]`) so its cards show. Then click the **Events** choice
  (`label[for="wfd-kind-events"]`) next to **All** and **Tasks**. *Visible:* the **Activities** pane ringed, the
  **Search activities** field, the **All / Events / Tasks** choice with **Events** selected, and the category headings
  with the event cards listed under them. *Labels:* `D/App.vue:811` (**Activities**); `D/toolbox/ActivityToolbox.vue:38-42`
  (**All**, **Events**, **Tasks**), `:54` (search field, placeholder from `Localizer.cs:46`), `:79-90` (categories).
- `designer-3`: `ClearHighlightsAsync`, click **All** (`label[for="wfd-kind-all"]`), then `HighlightAsync` on
  `[data-cy="designer-canvas"]`, and move the cursor onto the "Start with an event" panel (`[data-cy="start-picker"]`).
  *Visible:* the canvas ringed with the panel "Start with an event" and its hint "A workflow runs when something
  happens. Pick what starts this one, or drag any activity from the list on the left.", with the event buttons
  (for example **Content Published Event**). *Labels:* `D/canvas/StartPicker.vue:27-28`; `Localizer.cs:334-335`.
- `designer-4`: `ClearHighlightsAsync`. Highlight the tab bar of the workflow panel (the three tab names;
  `ul.wfd-panel-tabs:has([data-cy="panel-tab-workflow"])`) without selecting **Variables**, whose body shows a hint with
  code (see "Code on screen" below), and without hovering it: its `title` holds the same hint. Move the cursor to the
  middle of the panel's body, below the tab bar, then straight up onto the **Workflow** tab, so it never crosses the
  **Variables** tab; click **Workflow** (`[data-cy="panel-tab-workflow"]`), pause 1.5 seconds, then click **Issues**
  (`[data-cy="panel-tab-issues"]`).
  *Visible when the narration ends:* the workflow panel on the right with the **Issues** tab selected, showing "No
  issues found.", and the tab names **Variables**, **Workflow** and **Issues**.
  *Labels:* `D/panel/WorkflowPanel.vue:70` (tab order), `:82` (tab names); `Localizer.cs:67-68,126,142`.

## Clip `06-build` (chapter 06, short `build`)

**Starts on:** the designer of the empty workflow, as for `05-designer` (open `/Admin/Workflows/Types`, click the
workflow's name, wait for `[data-cy="start-picker"]`, click the **Issues** tab `[data-cy="panel-tab-issues"]`, click
`button.leftbar-compactor`). The site must be in the state `S4`. The activity ids are generated, so use the classes and `data-cy` values below, not ids.

- `build-1`: Click **Content Published Event** in the start panel (`[data-cy="start-picker-ContentPublishedEvent"]`).
  Wait until `.wfd-node.is-start` is visible. The activity panel opens by itself over the bottom of the canvas (it is not
  pinned by default: `D/panel/ActivityPanel.vue:75,87`), on its **Settings** tab. Highlight the green **Start** badge (`[data-cy="start-badge"]`). *Visible when the narration ends:* one card
  headed "Content Published Event" with the **Start** badge ringed, an outcome port **Done**, and the activity panel
  open with **Content Types**. *Labels:* `C/Workflows/Activities/ContentPublishedEvent.cs:22` ("Content Published
  Event"); `D/canvas/ActivityNode.vue:236` (**Start**); `W/Services/WorkflowTypeDraftManager.cs:175` (the first event
  added is the start); `D/App.vue:273-275` (the panel opens when the activity has an editor).
- `build-2`: `ClearHighlightsAsync`. In the activity panel, open the **Content Types** picker (the control that shows
  "Any content type"), type `Article` in its search field ("Search content types"), click the option **Article**, and
  close the list (press Escape or click the panel's title) so the picker loses the focus and the change is applied. Wait
  until the card's body reads "Article" under "Content Published" (`.wfd-node.is-start .wfd-node-body em`).
  The picker is a bootstrap-select control: probe its markup first (`Recorder probe`), see `OPEN-QUESTIONS.md`.
  *Visible:* the panel with **Content Types** showing **Article** as a tag, and the card showing "Article".
  *Labels:* `C/Views/Items/ContentPublishedEvent.Fields.Edit.cshtml:5,9` (**Content Types**, hint);
  `C/Views/Shared/Components/SelectContentTypes/Picker.cshtml:16,20` ("Any content type", "Search content types");
  `C/Views/Items/ContentPublishedEvent.Fields.Design.cshtml:4-6` (the card body); the type's display name "Article"
  comes from `blog.recipe.json`.
- `build-3`: Click the **Close the activity panel** button of the panel (`button[aria-label="Close the activity panel"]`),
  so the whole card shows. Move the cursor onto the **Done** port of the start card (`.wfd-node.is-start [data-outcome="Done"]`)
  and pause 800 ms: a green plus sign shows on its dot. Click it, wait until `[data-cy="quick-add"]` is visible, and
  type `Notify` in `[data-cy="quick-add-search"]`. *Visible:* the list "Add the next activity, after Done" with the
  search field containing `Notify` and one entry, **Notify Task** (category "UI"; the other activities whose
  names start with Notify, **Notify Content's Owner Task** and **Notify Specific Users Task**, need the Notifications
  feature, which this site does not enable). *Labels:* `D/canvas/OutcomePort.vue:30-34`
  (the port); `D/canvas/QuickAddMenu.vue:105-125` and `Localizer.cs:333` ("Add the next activity, after {0}");
  `W/Activities/NotifyTask.cs:34` ("Notify Task"), `:36` (category UI).
- `build-4`: Click **Notify Task** (`[data-cy="quick-add-NotifyTask"]`). Wait until a second card is visible
  (`.wfd-node.is-task`) and the activity panel shows **Notification Type** and **Message**. *Visible when the narration
  ends:* the Notify Task card to the right of the event card, joined to its **Done** port by an arrow, and the
  activity panel open over the bottom of the canvas with **Notification Type** on **Success** and an empty **Message**.
  The card is headed "Notify Task" and its body reads "Success Notification" (`W/Views/Items/NotifyTask.Fields.Design.cshtml`).
  *Labels:*
  `W/Views/Items/NotifyTask.Fields.Edit.cshtml:6,15` (**Notification Type**, **Message**); `D/App.vue:312-325` (the
  activity is added to the right and connected); the default type is the first value of `NotifyType` in
  `src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifyEntry.cs:8-14` (Success).
- `build-5`: Highlight the red issue badge on the Notify Task card (`[data-cy="issue-badge"]`) and the count on the
  **Issues** tab (`[data-cy="issues-count"]`), pause 2 seconds, then `ClearHighlightsAsync`. Click the **Message** field
  and type `An article was published.` (plain text, no Liquid), then press Tab so the field loses the focus and the
  change is applied. Wait until `[data-cy="issue-badge"]` is gone from the card (right after the pick,
  the badge can briefly count 2, the missing message and the warning "This activity can't be reached from a start
  activity, so it never runs.", until the transition is saved: wait for it to go, do not narrate it). *Visible when the narration ends:* the
  Notify Task card showing the message in quotes and no badge, and the **Issues** tab without a count. *Labels:*
  `D/canvas/ActivityNode.vue:259` (the badge), `D/panel/WorkflowPanel.vue:341-346` (the count); `W/ViewModels/NotifyTaskViewModel.cs:10`
  (**Message** is required); `W/Services/WorkflowTypeDraftManager.cs:412-421` (the error "Fix the settings of this
  activity").
- `build-6`: Wait until the toolbar reads "All changes saved" (`Localizer.cs:166`). Highlight that text for about 2
  seconds, `ClearHighlightsAsync`, then click **Publish** (`[data-cy="toolbar-publish"]`). Wait until the toast "The
  workflow was published as version 2." is visible: the workflow already had a version 1, the empty definition saved
  when it was created (see `designer-1`), so the first publish makes version 2. *Visible when the narration ends:* the
  toast, the badge "Version 2" next to the workflow's name, and **Publish** greyed out. *Labels:* `D/App.vue:759`
  (**Publish**), `:358` (toast); `Localizer.cs:166,218,221`.

## Clip `07-run` (chapter 07, short `run`)

**Starts on:** `/Admin` (open it before the capture), with the admin menu expanded (it is, in a new browser context).
The site must be in the state `S6`. Before `StartCaptureAsync`, nothing else.

- `run-1`: Click **Content** in the admin menu (it expands), then **Content Items** (click and wait for the navigation
  to `/Admin/Contents/ContentItems`). In the list, find the row whose title is "About" (type Article) and click its **Edit** button (`a.btn.edit` in that row; click and wait for the
  navigation to the editor). Highlight the **Publish** button (`button.publish`). *Visible when the narration ends:*
  the editor of the Article with a green ring around **Publish**. *Labels:* `C/AdminMenu.cs:65,68` (**Content**,
  **Content Items**); `C/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13` (**Edit**); `C/Views/Content.PublishButton.cshtml:19`
  (**Publish**).
- `run-2`: `ClearHighlightsAsync`, click **Publish** (`button.publish`; click and wait for the navigation back to the
  list). Wait until the page text contains both "An article was published." and "Your Article has been published.".
  *Visible when the narration ends:* the **Content Items** list with the two success notifications at the top, the
  one of the Notify Task (its message) and the one of Orchard Core. *Labels:* `W/Activities/NotifyTask.cs:59-64`
  (the notification); `C/Controllers/AdminController.cs:961-968` ("Your Article has been published."), `:849-860`
  (the redirect to the list); `C/Workflows/Handlers/ContentsHandler.cs:34-37` (the event, triggered when the item is published).

## Clip `08-instances` (chapter 08, short `instances`)

**Starts on:** `/Admin` (open it before the capture), admin menu expanded. The site must be in the state `S7`. This
clip changes nothing.

- `instances-1`: Click **Design**, then **Workflows** (click and wait for the navigation to `/Admin/Workflows/Types`).
  Highlight the **Instances** badge in the workflow's row (`a.badge` with the text "Instances"; not the **Instances**
  button of the toolbar). *Visible when the narration ends:* the list with the row "Notify the editors when an article
  is published", its **Instances** badge ringed, and no "No startup task" badge (the workflow is published). *Labels:*
  `W/Views/WorkflowTypeEntry.DefaultTags.SummaryAdmin.cshtml:22-23` (**Instances**), `:13-19` (the "No startup task"
  badge that must not show).
- `instances-2`: `ClearHighlightsAsync`, click the row's **Instances** badge (click and wait for the navigation to the
  instances page). Highlight the **Finished** count (`[data-cy="instance-count-Finished"]`), pause 2 seconds, then click
  the instance's link (the identifier in the row; click and wait for the navigation to its page). Wait until two cards
  have the class `is-executed` (`.wfd-node.is-executed`). If the designer is below the fold, scroll down until both
  cards show. *Visible when the narration ends:* the instance page with **Workflow ID**, **Created** and **Status**
  (**Finished**), and the read-only diagram with both activities highlighted as run; its toolbar reads "Runs on
  version 2" (`D/App.vue:678-681`, `Localizer.cs:234`). *Labels:*
  `W/Views/Workflow/Index.cshtml:39-44` (**All**, **Halted**, **Running**, **Faulted**, **Finished**, **Aborted**),
  `:66-72` (the counts); `W/Views/Workflow/Details.cshtml:34,40,46`; `W/Views/WorkflowEntry.Fields.SummaryAdmin.cshtml` (the
  link); `Localizer.cs:266` (the legend "Activities and connections the instance ran."). Do not open the **State** tab: it shows the data of the instance.

## Slides

Slides are drawn by `slides.py`; nothing is recorded.

- `why-1`: four cards, "When something happens on a site": content published, a user registers, a form submitted,
  and "Done by hand, or in code".
- `why-2`: four cards with check marks, "Workflows in Orchard Core": drawn in the admin, started by events, run by
  Orchard Core, every run is kept.
- `parts-1`: "The parts of a workflow", the **Event** card bright, the others dimmed.
- `parts-2`: the same slide, the **Task** card bright.
- `parts-3`: the same slide, the **Outcome** and **Transition** cards bright.
- `parts-4`: "From event to finish", four cards: the event happens, an instance starts, transitions are followed, it
  finishes or waits.
- `more-1`: "More events and tasks", three cards: HTTP Workflows Activities, Timer Workflows Activities, other modules.
- `more-2`: "Control and values", three cards: If Else Task, loops (For Each Task, For Loop Task, While Loop Task), variables.
- `more-3`: "Who can use workflows", two cards: Manage workflows, Execute workflows.

The opening card (`intro`) and the closing card (`outro`) are drawn from the storyboard; nothing is recorded.

## Code on screen

An admin video never shows code. In this video's screens, one place can show code: the **Variables** tab of the
workflow panel is the tab the designer opens on, and its body starts with a hint that holds a JavaScript call and a
Liquid expression (`Localizer.cs:69`, shown by `D/panel/VariablesTab.vue:223`). So `05-designer` and `06-build` open on the
**Issues** tab (before the capture), and `designer-4` never selects **Variables**. The same hint in the instance page
(`08-instances`) is a different, read-only text without code (`Localizer.cs:70`). Look at every contact sheet for
braces or calls on screen; the **Message** typed in `build-5` is plain text on purpose. The hint under the **Message**
field reads "The message to show. With Liquid support." (`W/Views/Items/NotifyTask.Fields.Edit.cshtml:19`): it is a
label of the screen, not Liquid code, and it is on screen during `build-4` and `build-5` (see `OPEN-QUESTIONS.md`). The **State** tab of the instance
page shows JSON and is never opened. The activity panel's **Available data** tab (expressions) is never opened.

## Checks for private data

Before assembling, look at every contact sheet (`review.py`), and before publishing OCR the video (see `SKILL.md`,
"Check for private data"). Nothing in this video should show more than:

- the user `admin` and `admin@example.com` (the user menu, if it shows);
- the site name "Orchard Core" and the Blog recipe's sample content;
- generated identifiers (the **Workflow ID**, the correlation id) and the workflow named in `create-2`.

Search the OCR for any other email address, a real name, a phone number, a token or a path of the machine. The
password must never be on screen: the sign-in is done outside the capture. The browser's address bar is not recorded.

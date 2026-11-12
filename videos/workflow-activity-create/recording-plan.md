# Recording plan: Write a Workflow Activity

Only one clip of this video is a recording: `06-designer`, six steps in the workflow designer. Every other chapter is made of
slides (`slides.py`), and the opening and closing cards need no recording. The clip shows the Notify Task activity that
the Workflows module already provides, so the site needs no custom module and no external service.

Notation: in this plan, a selector written in backticks stands for `scene.Page.Locator(...)` of the recorder. File and
line references are in the Orchard Core source at commit `92db4a83` (the commit this video was written from); check a
label there again if the site runs a different commit. Every file reference starts at `src/` of that checkout.

## The site it needs

- **Orchard Core**: run `src/OrchardCore.Cms.Web` on `http://localhost:5010` (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA`
  pointing at an empty folder outside both repositories.
- **Provisioning**: Auto Setup, so no setup or sign-in form is recorded (the variable names are those of the Auto Setup
  page of the documentation, `src/docs/reference/modules/AutoSetup/README.md`). One tenant, the default one:
  - `OrchardCore__AutoSetup__AutoSetupPath` = empty
  - `OrchardCore__AutoSetup__Tenants__0__ShellName` = `Default`
  - `OrchardCore__AutoSetup__Tenants__0__SiteName` = `Orchard Core`
  - `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` = `America/Los_Angeles`
  - `OrchardCore__AutoSetup__Tenants__0__AdminUsername` = `admin`
  - `OrchardCore__AutoSetup__Tenants__0__AdminEmail` = `admin@example.com`
  - `OrchardCore__AutoSetup__Tenants__0__AdminPassword` = `<password>` (chosen outside git, the same value as
    `VIDEO_PASSWORD`)
  - `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` = `Sqlite`
  - `OrchardCore__AutoSetup__Tenants__0__RecipeName` = `Blog`
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address. This video needs no mail, SMS or payment
  provider.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` saves `state.json` in the
  video's folder (git-ignored). Never write the real password in a file that is committed.
- **Features to enable before recording** (Tools > Features, outside the capture):
  1. Open `/Admin/Features`, type `Workflows` in the search box (placeholder "Search",
     `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`).
  2. On the row named **Workflows**, click **Enable** (`src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
     Its manifest makes Liquid and Scripting dependencies, which are enabled with it
     (`src/OrchardCore.Modules/OrchardCore.Workflows/Manifest.cs:12-18`).
  3. Leave the other rows (HTTP Workflows Activities, Timer Workflows Activities, Session Workflows Activities) disabled,
     and do not enable Notifications: that feature adds two more activities whose names start with "Notify", and step
     `designer-2` must find only the Notify Task.
  4. The Blog recipe already enables Contents, which provides the Content Published Event
     (`src/OrchardCore.Modules/OrchardCore.Contents/Workflows/Startup.cs:15` and `:25`). The recipe does not enable Workflows
     (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`).
- **Settings**: none to save.
- **Seed data**: one workflow named **Notify editors**, whose only activity is a Content Published Event. Create it in
  the browser, outside the capture:
  1. Open **Design**, **Workflows** (`/Admin/Workflows/Types`), click **Create Workflow**
     (`src/OrchardCore.Modules/OrchardCore.Workflows/Views/WorkflowType/Index.cshtml:25`).
  2. Type `Notify editors` in **Name** and click **Create and open the designer**
     (`src/OrchardCore.Modules/OrchardCore.Workflows/Views/WorkflowType/Create.cshtml:21` and `:131`).
  3. The designer opens empty and offers **Start with an event** (`src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/canvas/StartPicker.vue`,
     label from `src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:334`). Click **Content Published Event**.
  4. Wait until the toolbar says **All changes saved** (`src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:166`). Do not publish:
     the clip works on the draft.
  5. Reload the page and check that the canvas shows one node, **Content Published Event**, with a **Start** badge and a
     **Done** port, and that the Activities pane on the left is expanded. If the left pane is a narrow rail, click its
     expand button (**Show the activities**, `data-cy="toolbox-expand"`) and leave it expanded.
- **Snapshot**: stop the site, copy its `App_Data` folder aside (for example as `App_Data.seeded`) together with
  `state.json`, then start the site again. Restore the copy before every take of `06-designer`.

## Order of the clips, and what they change

- There is one recorded clip, `06-designer`. It depends on no other clip.
- `06-designer` changes the site: it adds a Notify Task to the draft of the workflow **Notify editors** and sets its
  settings. Stop the site and restore the snapshot before a new take.
- The slide chapters need no site.

## Clip `06-designer`

**Starts on**: `/Admin/Workflows/Types`, opened with `scene.OpenAsync` before `StartCaptureAsync`. The list shows
one row, **Notify editors**. Before the capture, open the **Design** group of the admin menu so that **Workflows** is
visible under it (labels from `src/OrchardCore.Modules/OrchardCore.Workflows/AdminMenu.cs:18-19`); if the menu is already open, leave it.

The designer keeps some preferences in the browser (pane collapsed, panel height, pinned). Use the recorder's fresh browser
context, so the Activities pane starts expanded and the activity panel starts unpinned.

### `designer-1`

- **Actions**:
  1. Click the link **Notify editors** in the list (`a` with the text `Notify editors`; it opens the designer,
     `src/OrchardCore.Modules/OrchardCore.Workflows/Views/WorkflowTypeEntry.Fields.SummaryAdmin.cshtml:7`) with `ClickAndWaitForNavigationAsync`.
  2. Wait until `[data-cy="designer-canvas"] .wfd-node` is visible and contains the text `Content Published Event`.
- **On screen when the narration ends**: the designer. The toolbar has the **Workflows** link
  (`[data-cy="toolbar-list"]`, `src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:33`), a slash, and the name **Notify editors**
  (`src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/App.vue:640-646`). The left pane is titled **Activities**
  (`src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:63`). The canvas shows one node, **Content Published Event**, with a green **Start** badge
  (`src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/canvas/ActivityNode.vue:236`, `src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:329`) and a **Done** port.
- **Labels come from**: `src/OrchardCore.Modules/OrchardCore.Workflows/AdminMenu.cs:18-19` (Design, Workflows);
  `src/OrchardCore.Modules/OrchardCore.Contents/Workflows/Activities/ContentPublishedEvent.cs:22` (Content Published Event);
  `src/OrchardCore.Modules/OrchardCore.Contents/Workflows/Activities/ContentActivity.cs:60` (the **Done** outcome).

### `designer-2`

- **Actions**:
  1. `TypeAsync` into `[data-cy="toolbox-search"]` (placeholder **Search activities**,
     `src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:46`) the text `notify`.
  2. Wait until `[data-cy="toolbox-activity-NotifyTask"]` is visible. While a search is typed, the categories that match are
     open, so no click on a category is needed.
  3. `MoveToAsync` that card and `HighlightAsync` it with the caption `Thumbnail view`. Clear the highlight at the start of
     the next step (`ClearHighlightsAsync`).
- **On screen when the narration ends**: the search box holds `notify`; one category, **UI**, is open with a count badge of
  1 (`[data-cy="toolbox-category-UI"]`, `src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/toolbox/ActivityToolbox.vue:85`); inside it one card: a bell
  icon, the bold title **Notify** and the line **Display a notification.**, with the green ring around it.
- **Labels come from**: `src/OrchardCore.Modules/OrchardCore.Workflows/Views/Items/NotifyTask.Fields.Thumbnail.cshtml:1-2` (the title and the
  description); `src/OrchardCore.Modules/OrchardCore.Workflows/Startup.cs:140` (the bell icon, `fa-solid fa-bell`);
  `src/OrchardCore.Modules/OrchardCore.Workflows/Activities/NotifyTask.cs:36` (the category **UI**).

### `designer-3`

- **Actions**:
  1. `ClearHighlightsAsync`.
  2. Click the **Done** port of the event node: `.wfd-node:has-text("Content Published Event") [data-outcome="Done"]`
     (`src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/canvas/OutcomePort.vue:29`). A click on a port that leads nowhere opens the list
     `[data-cy="quick-add"]`, titled **Add the next activity, after Done**
     (`src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:333`).
  3. `TypeAsync` into `[data-cy="quick-add-search"]` the text `notify`.
  4. Click `[data-cy="quick-add-NotifyTask"]` (the row **Notify Task**, category **UI**,
     `src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/canvas/QuickAddMenu.vue:121-127`).
  5. Wait until `.wfd-node:has-text("Notify Task")` is visible.
- **On screen when the narration ends**: the list is closed. A second node, header **Notify Task** with the bell icon, is to
  the right of the event node, joined to its **Done** port by a transition. The activity panel has opened by itself at the
  bottom of the canvas (the designer opens the settings of an activity that has an editor when it is added,
  `src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/App.vue:273-275`).
- **Labels come from**: `src/OrchardCore.Modules/OrchardCore.Workflows/Activities/NotifyTask.cs:34` (Notify Task);
  `src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/App.vue:306-325` (the activity is added to the right of the event and connected).

### `designer-4`

- **Actions**:
  1. `MoveToAsync` the body of the new node: `.wfd-node:has-text("Notify Task") .wfd-node-body`.
  2. `HighlightAsync` that body with the caption `Design view`. Then `HighlightAsync` the issue badge of the node,
     `.wfd-node:has-text("Notify Task") [data-cy="issue-badge"]`, with no caption. Pause 1.2 seconds between the two.
- **On screen when the narration ends**: the **Notify Task** node. Its header has the bell icon, the text **Notify Task**
  and a red issue badge with a number. Its body has a check-circle icon, the title **Success Notification**, and the
  message in quotes, empty (`""`). The **Done** port is in a column to the right of the body, not below it (`designer.css`, `.wfd-node-ports`).
- **Labels come from**: `src/OrchardCore.Modules/OrchardCore.Workflows/Views/Items/NotifyTask.Fields.Design.cshtml:20-22` (the title is
  `{0} Notification` with the notification type, and the message in quotes);
  `src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifyEntry.cs:8-13` (**Success** is the first value, so the default type);
  `src/OrchardCore.Modules/OrchardCore.Workflows/ViewModels/NotifyTaskViewModel.cs` (`[Required]` on Message) and
  `src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowTypeDraftManager.cs:412-420` (the issue "Fix the settings of this activity").

### `designer-5`

- **Actions**:
  1. `ClearHighlightsAsync`.
  2. Wait until `[data-cy="activity-panel"]` is visible and `[data-cy="panel-activity-form"]` contains the text
     `Notification Type`. The panel is already open from the previous steps; do not double-click the node.
  3. `HighlightAsync` in turn, with 1.2 seconds between them and no caption, these rows of the form:
     - `[data-cy="panel-activity-form"] .ocat-wrapper:has(label:has-text("Title"))`
     - `[data-cy="panel-activity-form"] .ocat-wrapper:has(label:has-text("Notification Type"))`
     - `[data-cy="panel-activity-form"] .ocat-wrapper:has(label:has-text("Message"))`
     - `[data-cy="panel-activity-form"] .ocat-wrapper:has(h5:has-text("When it fails"))`
     Call `ClearHighlightsAsync` between two rows.
- **On screen when the narration ends**: the activity panel at the bottom of the canvas, on its **Settings** tab (next to
  **Available data**; there is no Outputs tab for this activity). The form shows, from the top: **Title** (hint
  "Optionally provide a custom title for this activity."), **Notification Type** (a select showing **Success**),
  **Message** (an empty text box with a required marker, hint "The message to show. With Liquid support."), then the
  section **When it fails** with its retry settings.
- **Labels come from**: `src/OrchardCore.Modules/OrchardCore.Workflows/Views/ActivityMetadata.Edit.cshtml:5` (Title);
  `src/OrchardCore.Modules/OrchardCore.Workflows/Views/Items/NotifyTask.Fields.Edit.cshtml:6,10,15,19`;
  `src/OrchardCore.Modules/OrchardCore.Workflows/Views/ActivityRetryPolicy.Edit.cshtml:7` (When it fails);
  `src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:79` (Settings) and `:77` (Available data);
  `src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer/src/panel/ActivityPanel.vue:92-107` (which tabs a task without outputs has).

### `designer-6`

- **Actions**:
  1. `ClearHighlightsAsync`.
  2. `SelectAsync` the Notification Type select, `[data-cy="panel-activity-form"] select[name$="NotificationType"]`
     (the form has two more selects, in the retry settings, so a plain `select` would match three elements), with the label `Information`.
  3. `TypeAsync` into the Message box, `[data-cy="panel-activity-form"] input[name$="Message"]`, the text
     `A page was published.`
  4. Press Tab with `scene.Page.Keyboard.PressAsync("Tab")`: the editor applies a change when a field loses focus.
  5. Wait until `.wfd-node:has-text("Notify Task") .wfd-node-body` contains the text `Information Notification` and the
     text `A page was published.` (the node is redrawn from the saved settings).
  6. `HighlightAsync` the body of the node, no caption.
- **On screen when the narration ends**: the select shows **Information** and the Message box shows the typed text. In the
  node, the body now has an info-circle icon, the title **Information Notification**, and `"A page was published."`
  in quotes. The red issue badge is gone from the node header (if a badge remains, note it in the take and see
  `OPEN-QUESTIONS.md`).
- **Labels come from**: `src/OrchardCore.Modules/OrchardCore.Workflows/Views/Items/NotifyTask.Fields.Design.cshtml:3-18,20-22` (the icon and the
  title for each type); the options of the select are the names of `NotifyType` (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifyEntry.cs:8-13`:
  Success, Information, Warning, Error).

## The slide steps

No recording. Each line says what the slide of `slides.py` shows.

- `idea-1`: four cards (Task, Event, Outcome, Transition); the first two are bright.
- `idea-2`: the same four cards; the last two are bright.
- `idea-3`: four cards: the activity class, a display driver and a view model, three views, one registration.
- `class-1`: `NotifyTask.cs`, the declaration, `DisplayName`, `CategoryName`, `DisplayText` and `Category`.
- `class-2`: `NotifyTask.cs`, the constructor with the services it asks for.
- `class-3`: `NotifyTask.cs`, the `NotificationType` and `Message` properties.
- `class-4`: `NotifyTask.cs`, `GetPossibleOutcomes` and `ExecuteAsync`.
- `driver-1`: `NotifyTaskDisplayDriver.cs` beside `NotifyTaskViewModel.cs`.
- `shapes-1`: three cards: Thumbnail, Design and Edit, with the file name of each view.
- `views-1`: the Thumbnail view bright, the Design view dim.
- `views-2`: the Design view bright, the Thumbnail view dim.
- `views-3`: the Edit view.
- `expr-1`: `IfElseTaskViewModel.cs` and the `UpdateAsync` method of `IfElseTaskDisplayDriver.cs`.
- `expr-2`: the Edit view that builds the expression editor.
- `register-1`: the `AddActivity` call in the Startup of the Workflows module, and a note about the icon.
- `register-2`: the manifest of the Timers feature, and the Startup of the Contents module marked with `RequireFeatures`.
- `event-1`: `DataPipelineRunCompletedEvent.cs`, an event activity.
- `event-2`: the `TriggerEventAsync` call of `WorkflowDataPipelineRunHandler.cs`, and a note about what it does.

## Checks for private data

- The site is new and holds only fake data: the user `admin`, the address `admin@example.com`, the workflow **Notify editors**
  and the message `A page was published.` The Blog recipe adds sample content with invented authors only.
- OCR the video at one frame per second and at every scene change, as the skill says, and search for `@`, for any name that
  is not `admin`, and for the password. The password must never appear: the clip does not show the sign-in page.
- No other tenant, no connection string and no path of this machine may appear. The browser address bar is not recorded.

# Recording plan: workflows-timers

How to record this video without deciding anything. In this plan a selector stands for `scene.Page.Locator(...)`, and
a source reference (`file:line`) points into `OrchardCMS/OrchardCore`, in `src/OrchardCore.Modules/OrchardCore.Workflows`
unless it says otherwise. The labels are written as the code spells them.

The video has three recorded clips (`02-enable`, `03-create`, `04-run`) and two slide clips (`01-how`, `05-know`). The
slides are drawn by `slides.py`: only the three recorded clips need the recorder.

## The site it needs

- **Orchard Core:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with
  its own `ORCHARD_APP_DATA` folder outside both repositories. Outbound calls blocked with `HTTP_PROXY` and
  `HTTPS_PROXY` set to a dead address (this video calls no external service).
- **Auto Setup** (see the `orchardcore-tester` skill of the Orchard Core repository), tenant `Default`: `SiteName`
  `Orchard Core`, `SiteTimeZone` `America/Los_Angeles`, `AdminUsername` `admin`, `AdminEmail` `admin@example.com`,
  `AdminPassword` chosen outside git (written `<password>` here and passed in `VIDEO_PASSWORD`), `DatabaseProvider`
  `Sqlite`, `RecipeName` `Blog`. No setup form and no sign-in form is recorded: the session is saved once outside the
  capture (`Recorder setup`, with `VIDEO_USER_NAME=admin`), in a `state.json` that is never committed.
- **Features, before recording and outside the capture:** the Blog recipe does not enable the Workflows feature
  (`TheBlogTheme/Recipes/blog.recipe.json`), so enable **Workflows** in Tools, Features. Its dependencies (Liquid and
  Scripting) follow. Leave **Timer Workflows Activities** disabled: clip `02-enable` enables it on camera. Do not
  enable Background Tasks, Notifications or Email.
- **Settings and seed data:** none beyond the Blog recipe. No workflow exists before clip `03-create`, so the list of
  workflows is empty at its start (`Views/WorkflowType/Index.cshtml:34`).
- **External services and stand-ins:** none. The task after the timer is **Log Task**, which only writes to the log of
  the site.
- **Fake data:** the workflow is named `Scheduled check`, the log text is `The scheduled check ran.`, the only user is
  `admin` with `admin@example.com`. No real name, email, domain or token appears on any recorded screen.

## Order and dependencies

Record in this order; each clip starts from the state the previous one left.

1. `02-enable` changes the site (enables Timer Workflows Activities). Start from the snapshot **A**.
2. `03-create` changes the site (creates and publishes the workflow). Start from the snapshot **B**. From the moment
   the workflow is published, the site starts one instance, and then one more for each run, every minute.
3. `04-run` changes nothing, but it needs the state `03-create` left, **at least three minutes after Publish**: the
   first instance starts at the next check of the timers (within about a minute), and the first run comes at the next
   schedule time after that (the next minute), so the first Finished instance appears within about two minutes of
   publishing, and waiting three minutes leaves a margin. Start the clip when the list of instances of Scheduled check shows **Halted 1** and at least **Finished 1**.

**Snapshots.** Stop the site, copy its data folder aside, start it again.
- **A:** after Auto Setup, sign-in saved and **Workflows** enabled (before clip 1).
- **B:** after clip `02-enable` (before clip 2).
To record any clip again, stop the site, restore the matching snapshot, start it. To record `04-run` again, record
`03-create` again from B, then wait: the site keeps creating instances after `03-create`, so a stale take has many
Finished instances.

Stop the site (by its process ID) when the last take is done: the published workflow keeps running every minute.

## Clip `02-enable`

Starts on `/Admin` (the first page of the admin), signed in as admin from `state.json`. Open nothing else before the capture.

- `enable-1`: click **Tools** in the admin menu, then **Features** (`OrchardCore.Features/AdminMenu.cs:26-27`; use
  `ClickAndWaitForNavigationAsync` for **Features**). When the page is loaded (the breadcrumb says **Features**,
  `Views/Admin/Features.cshtml:15`), click the search box `#search-box` (`Features.cshtml:27`, placeholder **Search**)
  and type `Timer`. Wait until the row **Timer Workflows Activities** is visible (the list filters in the page).
  On screen when the narration ends: the search box with `Timer`, and the category **Workflows** (`Features.cshtml:88`)
  with the row **Timer Workflows Activities** and its **Enable** button.
  Labels: `OrchardCore.Workflows/Manifest.cs:30` (feature name), `OrchardCore.Workflows/Manifest.cs:33` (category).
- `enable-2`: highlight the category heading **Workflows** and the row, then click the **Enable** button of the row
  (`a[id^="btn-enable-"]` inside the row, `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the
  id is `btn-enable-` plus the feature id `OrchardCore.Workflows.Timers` with the dots replaced, which the search
  `Timer` leaves as the only row: its filter text is the name, id and description of each feature, `FeatureEntryDisplayDriver.cs:20`)
  with `ClickAndWaitForNavigationAsync`. The page reloads on the Features page. Wait until the success notification is
  visible: **The feature Timer Workflows Activities was enabled.**
  (`OrchardCore.Features/Controllers/AdminController.cs:317`; a list of features appears instead if a dependency was
  enabled too, which means Workflows was not enabled first: restore snapshot A and fix it).
  On screen when the narration ends: that notification. If the page reload cleared the search, it does not matter.

Changes the site: yes (snapshot **B** is taken after this clip).

## Clip `03-create`

Starts on `/Admin`, with the admin menu showing its top-level entries. Menu entries are opened on camera.

- `create-1`: click **Design** in the admin menu, then **Workflows** (`AdminMenu.cs:18-19`; `ClickAndWaitForNavigationAsync`
  for **Workflows**; the page is `/Admin/Workflows/Types`). On the list, highlight the message **Nothing here!** (**There
  are no workflow types for the moment.**, `Views/WorkflowType/Index.cshtml:34`), then click **Create Workflow**
  (`Index.cshtml:25`) with `ClickAndWaitForNavigationAsync` as the last action of the step.
  On screen when the narration ends: the page **Create Workflow** (breadcrumb **Workflows** / **Create Workflow**,
  `Views/WorkflowType/Create.cshtml:11-13`) with the field **Name** focused. If the menu is hard to drive, navigate with
  `NavigateAsync("/Admin/Workflows/Types")` after hovering **Design**, and note it in the take.
- `create-2`: type `Scheduled check` into **Name** (`Create.cshtml:21-23`; the field has `autofocus`). Leave **More settings**
  closed. Click **Create and open the designer** (`Create.cshtml:131`) with `ClickAndWaitForNavigationAsync`.
  Wait until `[data-cy="start-picker"]` is visible (the designer loaded, `Assets/designer/src/canvas/StartPicker.vue:26`).
  On screen when the narration ends: the designer with the toolbar `Workflows / Scheduled check`
  (`Assets/designer/src/App.vue:643-646`) and the empty canvas with the panel **Start with an event**.
- `create-3`: highlight `[data-cy="start-picker"]` (title **Start with an event**, hint **A workflow runs when something
  happens. Pick what starts this one, or drag any activity from the list on the left.**, `StartPicker.vue:27-28` and
  `Services/WorkflowsDesignerJSLocalizer.cs:334-335`), then click the button `[data-cy="start-picker-TimerEvent"]`
  (text **Timer Event**, `Timers/TimerEvent.cs:34`). Wait until the activity panel `[data-cy="activity-panel"]` shows the
  field **CRON Expression**.
  On screen when the narration ends: the Timer Event card on the canvas with the badge **Start**
  (`[data-cy="start-badge"]`; the first event is the start activity, `Services/WorkflowTypeDraftManager.cs:175`), and
  the activity panel open under it with the tab **Settings**.
- `create-4`: highlight the badge `[data-cy="start-badge"]`, then the field **CRON Expression** (`input[name$="CronExpression"]`,
  `Views/Items/TimerEvent.Fields.Edit.cshtml:5-7`), which holds the default `*/5 * * * *` (`Timers/TimerEvent.cs:44`).
  Triple-click the field, type `* * * * *` (five asterisks separated by single spaces), and press Tab so the change
  applies (the designer applies a field when it loses focus). Wait until the card on the canvas shows `* * * * *` under
  the header (`Views/Items/TimerEvent.Fields.Design.cshtml:7`).
  On screen when the narration ends: the field with `* * * * *`, and the card showing the same text.
- `create-5`: highlight the checkbox **Use site time zone** (`input[name$="UseLocalTime"]`, label and hint **The CRON
  expression uses UTC by default. Check this box to use the site's time zone instead.**,
  `Views/Items/TimerEvent.Fields.Edit.cshtml:16-19`), click the checkbox, and press Tab.
  On screen when the narration ends: **Use site time zone** checked, with its hint visible.
- `create-6`: click the **Done** port of the Timer Event card (`[data-cy^="port-"][data-outcome="Done"]`; its label is
  **Done**, `Timers/TimerEvent.cs:80`, `Assets/designer/src/canvas/OutcomePort.vue`). The menu `[data-cy="quick-add"]`
  opens, titled **Add the next activity, after Done** (`WorkflowsDesignerJSLocalizer.cs:333`). Type `Log Task` into
  `[data-cy="quick-add-search"]` (the list narrows to one row), and click `[data-cy="quick-add-LogTask"]`
  (**Log Task**, `Activities/LogTask.cs:25`, category **Primitives**). Wait until the Log Task card is on the canvas,
  connected to **Done**, and its settings (**Log Level** and **Text**, `Views/Items/LogTask.Fields.Edit.cshtml:5-17`)
  are in the activity panel. Highlight the Log Task card header.
  On screen when the narration ends: both cards connected by a line, the panel showing the settings of the Log Task.
- `create-7`: click the field **Text** (`input[name$="Text"]` in the activity panel, `LogTask.Fields.Edit.cshtml:14-17`),
  type `The scheduled check ran.` and press Tab.
  On screen when the narration ends: the field with the text, and the Log Task card showing `Information` and the text
  in quotes (`Views/Items/LogTask.Fields.Design.cshtml:27`).
- `create-8`: click the button `[data-cy="toolbar-publish"]` (**Publish**, `App.vue:757`). It is enabled as soon as the
  draft has a change (`canPublish`, `App.vue`: the draft has changes, nothing is publishing, no conflict), also while a
  save is running, and errors do not disable it. Clicking applies the open field, saves, then publishes (`publish()` in
  `App.vue`). If the workflow has errors, the dialog **The workflow can't be published** lists them
  (`WorkflowsDesignerJSLocalizer.cs:195-196`): the usual cause is an empty **Text** of the Log Task, so close the dialog,
  fix it and record the step again. If the workflow has warnings the dialog **Publish the workflow?** appears
  instead (`:191`); none is expected here. Wait until a toast that starts with **The workflow was published as
  version** is visible (`App.vue:358`, `WorkflowsDesignerJSLocalizer.cs:221`, the full text is
  **The workflow was published as version {0}.**). Do not wait for a fixed number: creating a workflow saves its empty
  definition as version 1 (`Services/WorkflowTypeStore.cs` `SaveAsync` calls `WorkflowTypeVersionStore.cs:90`), so the
  first publish is expected to be **version 2**. The narration does not say a number.
  On screen when the narration ends: the toast, and the toolbar showing the badge **Version 2** (`data-cy="published-version"`,
  `App.vue:652`; the number is the one the toast gives).

Changes the site: yes. The published workflow starts creating instances. Note the time of the Publish click for clip
`04-run`.

## Clip `04-run`

Before the capture, and at least three minutes after Publish: open `/Admin/Workflows/Types` and then the instances of
Scheduled check (click its **Instances** link), and check the counts in `[data-cy="instance-counts"]`
(`Views/Workflow/Index.cshtml:66`): **Halted 1** and **Finished** 1 or more. If not, wait and reload. Then open
`/Admin/Workflows/Types` again and start the capture there.

- `run-1`: the list shows the row **Scheduled check** with the link **Instances** next to its name
  (`Views/WorkflowTypeEntry.DefaultTags.SummaryAdmin.cshtml:20-23`; it is a badge link, `a.badge`, inside the row, and it
  only exists once the workflow has an instance). The toolbar of the list has a button with the same text,
  `[data-cy="all-instances"]` (`Views/WorkflowType/Index.cshtml:49`), which lists the instances of every workflow: do not
  click that one. Highlight the row, click the link **Instances** of the row with `ClickAndWaitForNavigationAsync`. On the page **Instances** (breadcrumb **Workflows** / **Scheduled check** /
  **Instances**, `Views/Workflow/Index.cshtml:59`), highlight `[data-cy="instance-count-Halted"]`.
  On screen when the narration ends: the six status buttons, each with its count: **All**, **Halted 1**, **Running 0**,
  **Faulted 0**, **Finished** and a count of 1 or more, **Aborted 0** (`Views/Workflow/Index.cshtml:39-44, 72`), with
  **Halted** highlighted, and the list below, newest first, with one instance badged **Halted**
  (`[data-cy="instance-status"]`, `Views/WorkflowEntry.DefaultTags.SummaryAdmin.cshtml:19`) above the **Finished** ones.
  Each row also has a **Version** badge (`:23`); the narration does not mention it.
- `run-2`: the page does not change by itself, so the recorder reloads it. Inside `OffCameraAsync`, first reload the page
  and read the number in `[data-cy="instance-count-Finished"]` (call it N, read at that moment, not before the capture:
  a run may have happened during `run-1`), then reload every 10 seconds until the number is N + 1 (at most 3 minutes; one
  run happens about every minute). Reload with `scene.NavigateAsync` and the current path and query (take them from
  `scene.Page.Url` before the wait), not with `Page.ReloadAsync`: a navigation made any other way freezes the screencast
  (`references/recording.md`, Pitfalls). The condition is the new count, which the page does not meet when the wait
  starts, so the wait stays off camera. Then highlight `[data-cy="instance-count-Finished"]`.
  On screen when the narration ends: **Finished** with the count N + 1 highlighted, **Halted 1** still there, and the
  list with one more **Finished** instance.
- `run-3`: click the identifier link (the link in the row, `Views/WorkflowEntry.Fields.SummaryAdmin.cshtml:13`) of
  a row whose badge `[data-cy="instance-status"]` says **Finished** (the second row, below the **Halted** one), with
  `ClickAndWaitForNavigationAsync`. The page of an instance has a breadcrumb that ends with the identifier
  (`Views/Workflow/Details.cshtml:8`). Wait until the read-only designer
  (`#workflow-designer`, `Views/Workflow/Details.cshtml:69`) shows the cards, and `.wfd-node.is-executed` is present on
  both cards. Highlight the Timer Event card, then the Log Task card.
  On screen when the narration ends: the page of the instance with the tab **Workflow**, the status **Finished**, the
  read-only designer with both cards highlighted as executed, and the legend **Activities and connections the instance
  ran.** (`WorkflowsDesignerJSLocalizer.cs:266`). From the code, the Timer Event card also carries the badge **×2**
  (`data-cy="executed-badge"`), because the journal records the timer twice: when it starts waiting and when it
  continues (`Services/WorkflowManager.cs`, `ExecuteWorkflowAsync`); the Log Task has none. The toolbar says
  **Runs on version 2** (`App.vue`, `RunsOnVersion`). The narration mentions none of this.

Changes the site: no.

## Slides

`slides.py` draws them; nothing is recorded. The cards of a step are bright, the others dimmed.

- `how-1`: "The clock as an event": cards 1 and 2 bright (something happens; the clock strikes).
- `how-2`: the same slide, card 3 bright (a wait in the middle of a workflow).
- `how-3`: "A CRON expression has five fields": four cards (minute and hour, day of the month and month, day of the week,
  a star).
- `how-4`: "Four schedules": the expressions `*/5 * * * *`, `0 * * * *`, `0 8 * * *`, `0 9 1 * *`, each with its meaning (the third says UTC by default).
- `know-1` to `know-3`: "Good to know": three cards; the card of the step is bright: checked once a minute, the site
  must run, one instance for each run.

## Checks for private data

- Only `admin`, `admin@example.com`, the site name `Orchard Core`, the workflow `Scheduled check` and the log text
  appear. The password is never typed on camera: the session comes from `state.json`.
- The identifiers of instances are random and carry no personal data; the dates are those of the take.
- After assembling, OCR the video at one frame per second and at every scene change, as the skill describes, and
  search for any name, email other than `admin@example.com`, token or path of this machine. Do not commit `state.json`,
  the snapshots, or the mask script if one is used.

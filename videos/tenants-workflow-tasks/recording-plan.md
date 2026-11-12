# Recording plan: tenants-workflow-tasks

For a person (or a session) who records this video and has to decide nothing. Read `OPEN-QUESTIONS.md` first: one
point there (the Name and Description fields of Create Tenant Task) decides how the `04-create` clip is typed.

In this file a selector such as `[data-cy='toolbar-publish']` stands for `scene.Page.Locator("[data-cy='toolbar-publish']")`.
"Source" is `file:line` in the Orchard Core checkout at the commit the content was written from (paths under
`src/OrchardCore.Modules/` unless they start with `src/`). The Orchard Core designer is a Vue application: its
`data-cy` attributes are in `OrchardCore.Workflows/Assets/designer/src/`.

## The site it needs

- Orchard Core: `src/OrchardCore.Cms.Web` of a checkout, on `http://localhost:5010`, with its own `ORCHARD_APP_DATA`
  folder outside both repositories, provisioned by Auto Setup (see `.agents/skills/orchardcore-video-producer/references/recording.md`).
  This video is not about the Setup screen, so Auto Setup is used and no setup or login form is recorded.
- Auto Setup values: site name **Orchard Core**, recipe **Blog**, time zone `America/Los_Angeles`, database provider
  **Sqlite**, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in
  `VIDEO_PASSWORD`). Block outbound calls with `HTTP_PROXY`/`HTTPS_PROXY` set to a dead address.
- The only tenant before the take is **Default**. A folder `App_Data/Sites/Bluebird` must not exist.
- Features before the take: the Blog recipe's, and nothing else. **Tenants** and **Workflows** must be disabled:
  clip `02-features` enables them on camera. Do not enable HTTP Workflows Activities, Timer Workflows Activities, or
  any other feature.
- Settings: none changed. The admin list layout, theme and language stay at their defaults.
- External services: none. The workflow only talks to the application itself, and the Blog recipe of the second tenant
  needs no network.
- Sign in once outside the capture (`Recorder setup`), which writes `state.json` (git-ignored). Nothing else is seeded:
  every record this video shows is created by the clips.
- Mask the typed password. Write a small JavaScript init script **outside the repository** and pass it as `VIDEO_INIT_SCRIPT`
  (the same mechanism the skill uses for the name mask). On `DOMContentLoaded` it appends a `<style>` element to the head with
  one rule: `input[name$='AdminPasswordExpression'] { -webkit-text-security: disc; }`. A style in the head also covers the editor, which the designer
  injects after the page has loaded. The script holds no names or other data, but is kept out of git like every mask script.
  The Admin password field of Setup Tenant Task is a plain text box, so without the rule the password is readable in the video.

### The fake values to type

| Where | Value |
|---|---|
| Workflow, Name | `Create and set up a tenant` |
| Create Tenant Task, Name | `Bluebird` |
| Create Tenant Task, Description | `Bluebird` (the same text as Name, see `OPEN-QUESTIONS.md`) |
| Create Tenant Task, Url Prefix | `bluebird` |
| Create Tenant Task, What type of database to use? | `Sqlite` |
| Create Tenant Task, Recipe | `Blog` |
| Setup Tenant Task, Tenant name | `Bluebird` |
| Setup Tenant Task, Site name | `Bluebird Books` |
| Setup Tenant Task, Admin user name | `admin` |
| Setup Tenant Task, Admin email | `owner@example.com` |
| Setup Tenant Task, Admin password | `<tenant-password>`: a throwaway password chosen outside git, different from `<password>`, that satisfies the password rules of a new site (the administrator is created in the new tenant; expect at least 6 characters, with upper and lower case letters, a digit and a symbol, and confirm on the rehearsal) |

Leave every other field of both activities empty: Host name, Table Prefix, Connection string, Schema and Feature Profile
of Create Tenant Task; the database fields and Recipe of Setup Tenant Task.

### The rehearsal (off camera, once, before the first take)

Do the whole video once without recording, then restore the first snapshot. It answers what the plan cannot:

1. The chain works: after `06-run` the tenants list shows **Bluebird** with the state **Running** and its site opens.
   If the dialog shows **Faulted**, or the state is not **Running**, stop and report; do not record.
2. Password rules: `<tenant-password>` is accepted by Setup Tenant Task (otherwise the task takes Failed, the instance
   still reads **Finished**, and Bluebird stays **Uninitialized**).
3. The two claims of the slides of chapter 8: run the published workflow a second time. Create Tenant Task takes its
   Failed outcome (Bluebird exists), no second tenant appears, and the instance reads **Finished**.
4. The swap check of `OPEN-QUESTIONS.md` (point 1), done with different values in Name and Description. Report what you
   see in the pull request; the take itself uses the same value in both fields.

### Snapshots, and the order of the clips

Every clip starts from the state the previous one left, so the clips are recorded in the order they play in:
`02-features`, `03-workflow`, `04-create`, `05-setup`, `06-run`, `07-result`. The cards (`00-intro`, `99-outro`) and the
slides (`01-activities`, `08-failed`) need no recording.

After every clip, stop the site and copy its `App_Data` aside, so a single clip can be taken again:

| Snapshot | Taken | Holds | Restore it to take again |
|---|---|---|---|
| S0 | after Auto Setup and `Recorder setup` | Default tenant only, Tenants and Workflows disabled | `02-features` |
| S2 | after `02-features` | Tenants and Workflows enabled | `03-workflow` |
| S3 | after `03-workflow` | the workflow "Create and set up a tenant", with its start activity, a draft | `04-create` |
| S4 | after `04-create` | the draft with Create Tenant Task | `05-setup` |
| S5 | after `05-setup` | the draft with both tasks | `06-run` |
| S6 | after `06-run` | the workflow published, one instance, and the tenant Bluebird (`App_Data/Sites/Bluebird`, **Running**) | `07-result` |

Restoring S6 or later keeps Bluebird; restoring S5 or earlier removes it, which a new take of `06-run` needs (Create
Tenant Task takes Failed when Bluebird exists). Never commit a snapshot: `App_Data` holds the workflow with its password.

Clips that change the site: `02-features` (enables two features, which restarts the tenant), `03-workflow` through
`05-setup` (the workflow and its draft), `06-run` (publishes, creates an instance and the tenant). `07-result` only reads.

## Chapter 1: slides

Slides are drawn by `slides.py`; nothing is recorded.

- `activities-1`: four cards, "Create Tenant Task" and "Setup Tenant Task" bright, the other two dimmed, each with its outcomes.
- `activities-2`: the same four cards, "Disable Tenant Task" and "Enable Tenant Task" bright.
- `activities-3`: the heading "Each activity checks its tenant first", with what Create Tenant Task and Setup Tenant Task need bright.
- `activities-4`: the same cards, what Disable Tenant Task and Enable Tenant Task need bright, and the line about the Default tenant.

## Chapter 2: `02-features` (clip)

- Starts on: `/Admin/Features` (opened before the capture), signed in as `admin`. The search box has the focus.
- Site state: restored from S0.

`features-1` (Tenants)
- Actions: `TypeAsync` in `#search-box` the text `Tenants`. Wait until the list shows only the matching features.
  `ClickAndWaitForNavigationAsync` on the **Enable** button of the feature named **Tenants**: `#btn-enable-OrchardCore_Tenants`
  (the id comes from the feature id `OrchardCore.Tenants` through `Html.GenerateIdFromName`; check it with `Recorder probe`).
  If the id differs, use `:is(li,tr):has-text('Provides a way to manage tenants from the admin.') a:text-is('Enable')`. Do not click
  the buttons of **Distributed Tenants**, **Tenant Feature Profiles** or **Static File Provider**.
- Visible when the narration ends: the Features page after the redirect, with the green message "The feature Tenants was enabled."
- Source: `OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features), `OrchardCore.Features/Views/Admin/Features.cshtml:27` (Search),
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable), `OrchardCore.Features/Controllers/AdminController.cs:317` (the message),
  `OrchardCore.Tenants/Manifest.cs:10-16` (the feature is named "Tenants").
- The narration says "Open Tools, Features": the page is already open when the capture starts, so move the cursor to the
  menu entries Tools, Features with `MoveToAsync` and `HighlightAsync` them, without clicking. Expand the Tools group before
  the capture.

`features-2` (Workflows)
- Actions: `TypeAsync` in `#search-box` the text `Workflows`. `ClickAndWaitForNavigationAsync` on the **Enable** button of the
  feature named **Workflows**: `#btn-enable-OrchardCore_Workflows` (fallback: the row with the text "The Workflows module provides tools and APIs to
  create custom workflows"). The message may name the dependencies enabled with it; that is expected. Then expand the
  admin menu groups **Multi-Tenancy** and **Design** (click each) so that **Tenants** and **Workflows** are listed, and `HighlightAsync` both entries.
- Visible when the narration ends: the admin menu with Multi-Tenancy, Tenants, and Design, Workflows listed.
- Wait for the page after the redirect before clicking the menu: enabling Workflows restarts the tenant.
- Source: `OrchardCore.Tenants/AdminMenu.cs:30-33` (Multi-Tenancy, Tenants), `OrchardCore.Workflows/AdminMenu.cs:18-19` (Design, Workflows),
  `OrchardCore.Workflows/Manifest.cs:12-18` (the feature is named "Workflows").

## Chapter 3: `03-workflow` (clip)

- Starts on: `/Admin` (the dashboard), opened before the capture. Expand the **Design** group of the menu before the capture.
- Site state: restored from S2.

`workflow-1` (the Workflows list and Create Workflow)
- Actions: `ClickAndWaitForNavigationAsync` on **Workflows** in the Design group (the list says "Nothing here! There are no workflow types for the moment.").
  `ClickAndWaitForNavigationAsync` on **Create Workflow**. `TypeAsync` in the **Name** box (`input[name='Name']`) the text `Create and set up a tenant`.
- Visible when the narration ends: the Create Workflow page with the Name filled in and More settings closed.
- Source: `OrchardCore.Workflows/AdminMenu.cs:18-19`, `OrchardCore.Workflows/Views/WorkflowType/Index.cshtml:25` (Create Workflow),
  `OrchardCore.Workflows/Views/WorkflowType/Create.cshtml:21` (Name).

`workflow-2` (Record activity data)
- Actions: click **More settings** (`summary` with that text) to open it. `HighlightAsync` the check box **Record activity data**
  (`input[type='checkbox'][name='RecordActivityData']`, checked by default) and click it to clear it; wait until it is unchecked. Then
  `ClickAndWaitForNavigationAsync` on the button **Create and open the designer**, near the end of the narration.
  Wait for `[data-cy='workflow-designer']` and `[data-cy='start-picker']` to be visible.
- Visible when the narration ends: the designer, with its toolbar showing the name "Create and set up a tenant" and the
  empty canvas with the heading "Start with an event".
- Source: `OrchardCore.Workflows/Views/WorkflowType/Create.cshtml:30` (More settings), `:105` (Record activity data), `:131` (Create and open the designer),
  `OrchardCore.Workflows/Controllers/WorkflowTypeController.cs:253` (checked by default),
  `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:334` (Start with an event).
- Write down the numeric id in the address after the designer opens (`/Admin/Workflows/Types/Edit/<id>`, probably 1): clips 4 to 6 start there.

`workflow-3` (Started By Workflow)
- Actions: `TypeAsync` in `[data-cy='toolbox-search']` the text `Started`. Wait until the list shows the card **Started By Workflow**
  (category **Primitives**). Click `[data-cy='toolbox-activity-StartedByWorkflowEvent']`. The card is added in the middle of the canvas.
  Do not use the "Start with an event" list: it shows at most six events, and Started By Workflow may not be among them.
- Visible when the narration ends: one activity on the canvas, with the badge **Start** (`[data-cy='start-badge']`) and one outcome port named **Done**.
  The activity panel stays closed, since this activity has no settings.
- Source: `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:46` (Search activities), `:329` (Start),
  `OrchardCore.Workflows/Views/Items/StartedByWorkflowEvent.Fields.Thumbnail.cshtml:1` (Started By Workflow),
  `OrchardCore.Workflows/Events/StartedByWorkflowEvent.cs` (outcome Done, no editor),
  `OrchardCore.Workflows/Services/WorkflowTypeDraftManager.cs:175` (the first event added becomes the start),
  `OrchardCore.Workflows/Controllers/WorkflowDesignerController.cs:1121` (Run works for a workflow that starts with it).

## Chapter 4: `04-create` (clip)

- Starts on: `/Admin/Workflows/Types/Edit/<id>` (the designer of "Create and set up a tenant"), opened before the capture.
  The canvas holds Started By Workflow. Before the capture, pin the activity panel (button "Keep the activity panel open below the
  canvas") and drag its top edge up so that about six fields of the form are visible at once; the size is remembered by the browser.
- Site state: restored from S3.
- The activity panel applies the form when a field loses the focus, 600 ms later. The button **Apply** (`[data-cy='panel-apply']`) applies it at once
  and reads **Saved!** for about two seconds. Applying does not redraw the fields, so what is typed stays on screen.
- After this clip, **never open the Create Tenant Task editor again** (see `OPEN-QUESTIONS.md`, point 1). Select other activities only.

`create-1` (add Create Tenant Task)
- Actions: click the port **Done** of the Started By Workflow activity (`.wfd-node:has-text('Started By Workflow') .wfd-port[data-outcome='Done']`), with a plain
  click that does not move the pointer: a drag from a port starts a connection, and the Enter key on a port does not open this menu (it opens the dialog "Connect an outcome to…",
  and only when the canvas holds two activities or more). If the menu does not open, click the port again; do not use the keyboard. The menu titled "Add the next activity, after Done" opens with its search box focused.
  `TypeAsync` in `[data-cy='quick-add-search']` the text `Create Tenant`. The list shows **Create Tenant Task** with the category **Tenant**. Click
  `[data-cy='quick-add-CreateTenantTask']`.
- Visible when the narration ends: a second activity, "Create Tenant Task", connected to the first by a transition from Done, with outcomes
  **Done** and **Failed**, and the activity panel open at the bottom on the **Settings** tab with the empty fields of the form.
- Source: `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:333` (Add the next activity, after {0}),
  `OrchardCore.Workflows/Assets/designer/src/canvas/DesignerCanvas.vue:201-205` (a click on an unconnected outcome opens the menu), `:207-211` and `:413` (Enter on a port opens the connect dialog, which needs two activities),
  `OrchardCore.Workflows/Assets/designer/src/App.vue:262-285` (the panel opens for an activity that has an editor),
  `OrchardCore.Tenants/Workflows/Activities/CreateTenantTask.cs:33` (Create Tenant Task), `:92` (Done, Failed),
  `OrchardCore.Tenants/TenantsLocalizationSources.cs:14` (the category Tenant), `OrchardCore.Tenants/Workflows/Startup.cs:17-20` (the four activities are registered).

`create-2` (Name, Description, Url Prefix)
- Actions, with `TypeAsync` (it clicks the field first): `input[name$='TenantNameExpression']` (label **Name**) `Bluebird`;
  `input[name$='DescriptionExpression']` (label **Description**) `Bluebird`; `input[name$='RequestUrlPrefixExpression']` (label **Url Prefix**) `bluebird`.
  Scroll the panel with `ScrollAsync` if a field is hidden. Do not read or zoom into the grey hint texts: those of Url Prefix and Host name show a sample address
  from the product (see the checks at the end).
- Watch while typing: when the focus leaves a field, the panel applies the form, and the server stores the text of **Name** as the description and the text of **Description**
  as the tenant name (`OPEN-QUESTIONS.md`, point 1). The panel is not redrawn, so what is typed stays. If a box empties itself or shows other text after the focus leaves it,
  stop and report it: the take would show the swap. The narration never says which box becomes what.
- Visible when the narration ends: Name, Description and Url Prefix filled in the panel.
- Source: `OrchardCore.Tenants/Views/Items/CreateTenantTask.Fields.Edit.cshtml:6` (Name), `:15` (Description), `:24` (Url Prefix),
  `OrchardCore.Tenants/Views/Admin/Index.cshtml` and `ShellSettingsEntry.Description.SummaryAdmin.cshtml:15` (the description is shown in the tenants list),
  `OrchardCore.Tenants/Workflows/Drivers/CreateTenantTaskDisplayDriver.cs:25-26` (the swap),
  `OrchardCore.Tenants/Services/TenantValidator.cs:84-90` (a tenant needs a host and prefix pair no other tenant has).

`create-3` (database, Recipe, apply)
- Actions: `TypeAsync` in `input[name$='DatabaseProviderExpression']` (label **What type of database to use?**) `Sqlite`; then in
  `input[name$='RecipeNameExpression']` (label **Recipe**) `Blog`. Press Tab, then click `[data-cy='panel-apply']` and wait until its text is **Saved!**.
  Leave Table Prefix, Connection string, Schema and Feature Profile empty (scroll the panel past them).
- Visible when the narration ends: the panel with Sqlite and Blog typed, the button reading **Saved!**, and, above it, the Create Tenant Task activity
  on the canvas with no issue badge.
- Source: `OrchardCore.Tenants/Views/Items/CreateTenantTask.Fields.Edit.cshtml:42` (What type of database to use?), `:75` (Recipe), `:83` (Feature Profile),
  `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:132` (Apply), `:134` (Saved!),
  `OrchardCore.Tenants/Workflows/Activities/CreateTenantTask.cs:153-208` (the settings are put on the tenant; an empty one is skipped).

## Chapter 5: `05-setup` (clip)

- Starts on: the designer of the workflow (as in chapter 4), opened before the capture. Nothing is selected and the panel is closed.
- Site state: restored from S4. Do not open the Create Tenant Task editor.

`setup-1` (add Setup Tenant Task; Tenant name, Site name)
- Actions: click the port **Done** of the Create Tenant Task activity (`.wfd-node:has-text('Create Tenant Task') .wfd-port[data-outcome='Done']`, not **Failed**), with a plain click
  that does not move the pointer (not the Enter key: that opens the dialog "Connect an outcome to…" instead of the menu).
  `TypeAsync` in `[data-cy='quick-add-search']` the text `Setup Tenant`, click `[data-cy='quick-add-SetupTenantTask']`. The panel opens on the Settings tab.
  `TypeAsync` in `input[name$='TenantNameExpression']` (label **Tenant name**) `Bluebird`, then in `input[name$='SiteNameExpression']` (label **Site name**) `Bluebird Books`.
- Visible when the narration ends: three activities connected in a row (the Done transitions), the panel showing Tenant name and Site name filled in.
- Source: `OrchardCore.Tenants/Views/Items/SetupTenantTask.Fields.Edit.cshtml:6` (Tenant name), `:15` (Site name),
  `OrchardCore.Tenants/Workflows/Activities/SetupTenantTask.cs:51` (Setup Tenant Task), `:110` (Done, Failed).

`setup-2` (the administrator)
- Actions: `TypeAsync` in `input[name$='AdminUsernameExpression']` (label **Admin user name**) `admin`; in `input[name$='AdminEmailExpression']`
  (label **Admin email**) `owner@example.com`; in `input[name$='AdminPasswordExpression']` (label **Admin password**) `<tenant-password>`.
  The init script shows the password as dots.
- Visible when the narration ends: Admin user name `admin`, Admin email `owner@example.com`, and the Admin password as dots.
- Source: `OrchardCore.Tenants/Views/Items/SetupTenantTask.Fields.Edit.cshtml:23` (Admin user name), `:31` (Admin email), `:39` (Admin password).

`setup-3` (empty fields, apply)
- Actions: `ScrollAsync` the panel down so that the empty fields **What type of database to use?**, **Table Prefix**, **Connection string**, **Schema** and
  **Recipe** are visible (`HighlightAsync` the group, then `ClearHighlightsAsync()` before scrolling again). Press Tab in the last
  field you touched, click `[data-cy='panel-apply']` and wait until it reads **Saved!**.
- Visible when the narration ends: the empty database fields and Recipe in the panel, the button reading **Saved!**.
- Source: `OrchardCore.Tenants/Views/Items/SetupTenantTask.Fields.Edit.cshtml:47-80`, `OrchardCore.Tenants/Workflows/Activities/SetupTenantTask.cs:163-186`
  (an empty database provider, connection string, table prefix, schema or recipe is read from the tenant's settings).

## Chapter 6: `06-run` (clip)

- Starts on: the designer of the workflow, opened before the capture. Nothing is selected, the panel is closed.
- Site state: restored from S5.

`run-1` (Publish)
- Actions: click `[data-cy='toolbar-publish']`. Wait for the message "The workflow was published as version 1." If a dialog titled "Publish the workflow?"
  appears, the draft has warnings: stop and report them instead of clicking "Publish anyway".
- Visible when the narration ends: the toolbar badge "Version 1" next to the workflow name, and the message.
- Source: `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:190` (Publish), `:221` (The workflow was published as version {0}.),
  `OrchardCore.Workflows/Services/WorkflowTypeDraftManager.cs:422-535` (what makes a warning or an error; an unconnected outcome is neither).

`run-2` (Run)
- Actions: click `[data-cy='toolbar-run']` (its text is "Run…"). The dialog "Run the workflow" opens with the text "The workflow has no input variables."
  Click `[data-cy='run-start']` (the button **Run**). Wrap the wait in `scene.OffCameraAsync(...)` until `[data-cy='run-status']` is visible:
  the setup recipe of the Blog site runs inside the request, and the wait is cut out of the clip. Make the condition `[data-cy='run-status']` has the text `Finished`
  (a bare "visible" would be met too early if the dialog kept an earlier result).
- Visible when the narration ends: the dialog's Result section with "Instance" and a green **Finished** badge, and the button "Open the instance".
- Source: `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:286` (Run…), `:288` (Run the workflow), `:282` (Run), `:294` (The workflow has no input variables.),
  `:310` (Instance), `:311` (Open the instance), `OrchardCore.Workflows/Assets/designer/src/draft/RunDialog.vue:263` (the status badge).

`run-3` (the instance)
- Actions: `ClickAndWaitForNavigationAsync` on `[data-cy='run-open-instance']`. Wait for `[data-cy='workflow-designer']` and for the three activities to be drawn.
  Do not open the Journal tab or the Runs tab of the page.
- Visible when the narration ends: the instance page, a read-only designer in which Started By Workflow, Create Tenant Task and Setup Tenant Task and
  the two transitions between them are highlighted, with the legend "Activities and connections the instance ran."
- Source: `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:266` (Activities and connections the instance ran.).

## Chapter 7: `07-result` (clip)

- Starts on: `/Admin/Tenants` (opened before the capture). Site state: restored from S6 (or the state `06-run` left).

`result-1` (the tenants list)
- Actions: none to type. `HighlightAsync` the row of **Bluebird** (its name link, `a.text-break:text-is('Bluebird')`, and the tags on its row), then clear the highlight.
- Visible when the narration ends: the Tenants page with two tenants, **Default** and **Bluebird**. Bluebird shows its description (`Bluebird`),
  the database tag `Sqlite`, the recipe tag `Blog`, and the state tag **Running**.
- Source: `OrchardCore.Tenants/AdminMenu.cs:30-33`, `OrchardCore.Tenants/Views/Admin/Index.cshtml:9` (Tenants), `ShellSettingsEntry.State.SummaryAdmin.cshtml:17-18` (Running),
  `ShellSettingsEntry.Database.SummaryAdmin.cshtml:8` (Database provider), `ShellSettingsEntry.Recipe.SummaryAdmin.cshtml:8` (Recipe),
  `ShellSettingsEntry.Description.SummaryAdmin.cshtml:15`.

`result-2` (open the site)
- Actions: `ClickAndWaitForNavigationAsync` on the name link `a.text-break:text-is('Bluebird')`. It opens the tenant in the same tab. The first request builds
  the tenant, which can take a few seconds: wrap the wait for the home page in `scene.OffCameraAsync(...)` with the condition that the page title contains `Bluebird Books`
  (expected from the Site name; confirm it in the rehearsal and use what the page really shows).
- Visible when the narration ends: the home page of the Blog site of the tenant, in the Blog theme, with no error page.
- Source: `OrchardCore.Tenants/Views/ShellSettingsEntry.Fields.SummaryAdmin.cshtml:9` (the name is a link to the tenant).

## Chapter 8: slides

- `wrap-1`: three cards: "Failed leads nowhere by itself", "No connection, the workflow ends" and "Connect Failed to a report".
- `wrap-2`: three cards: "Run it a second time", "Switch a tenant off and on" and "Start it from any event".

## Cards

- `intro`: the opening card, title "Workflows for Tenants", subtitle "Create and set up tenants automatically".
- `outro`: the closing card, subtitle "docs.orchardcore.net".

## Checks for private data (before assembling, and again on the video)

- Everything typed is fake: Bluebird, Bluebird Books, `bluebird`, `admin`, `owner@example.com`. No real name, address or number appears.
- `<tenant-password>` and `<password>` never appear in a frame. `<password>` is only used by `Recorder setup`. `<tenant-password>` is typed in
  `setup-2` and shown as dots; OCR every frame of `setup-2` and `setup-3` for it, and look at the contact sheet of both steps.
  The Setup Tenant Task keeps the password as plain text in the workflow: delete the site's `App_Data` after the recording, and never commit a snapshot.
- Record activity data is cleared (`workflow-2`), so the instance page holds no values. Still, do not open the Journal or Runs tabs.
- The grey hint texts of Create Tenant Task (Url Prefix, Host name) show a sample address, which is part of the product's own text.
  Keep the cursor and the highlights away from them; if they are readable in a frame, say so in the pull request.
- The tenants list, the notifications and the new tenant's home page show only the fake names above. No machine path, host name other than `localhost`,
  or port is visible (the browser's address bar is not recorded).
- Search the OCR of the video for `example.com` addresses other than `admin@example.com` and `owner@example.com`.

# Recording plan: workflows-user-events

Everything a person needs to record the video without deciding anything. A selector written here stands for
`scene.Page.Locator(...)`. File references are in the Orchard Core source (`src/OrchardCore.Modules/...`), the commit
the content was written against. `Workflows/...` means `OrchardCore.Workflows`, `Users/...` means `OrchardCore.Users`.

Read `OPEN-QUESTIONS.md` first: it lists what the author could not check without a running site, and the ten minutes
of probing it asks for before the first take.

## The site it needs

- A fresh site made from `src/OrchardCore.Cms.Web`, provisioned by Auto Setup with the **Blog** recipe, tenant
  `Default`, site name **Orchard Core**, time zone `America/Los_Angeles`, and the user `admin` with the email
  `admin@example.com`. The password is chosen outside git and passed in `VIDEO_PASSWORD`; this plan writes it as
  `<password>`.
- Outbound calls blocked (`HTTP_PROXY` and `HTTPS_PROXY` set to a dead address). The video needs no external
  service, no mail server and no SMS provider.
- The recording is not about the setup screen, so it uses Auto Setup and a saved sign-in (`state.json`, git-ignored).
- **Features to enable before recording** (**Tools**, **Features**, or `Recorder prepare`; `OrchardCore.Features/AdminMenu.cs:26-27`): **Workflows**
  (`OrchardCore.Workflows`, which brings Liquid and Scripting with it). The Blog recipe has already enabled **Users**,
  **Roles** and **Contents**. Leave every other Workflows feature off (HTTP Workflows Activities, Timer Workflows
  Activities, Session Workflows Activities) and leave **Email** and **Users Registration** off, so the Activities pane
  and the "Start with an event" list stay short. The list still holds the content events and others: it shows at most
  six events, the suggested ones first and User Created Event among them, then the rest in alphabetical order
  (`Workflows/Assets/designer/src/canvas/StartPicker.vue:13-21`), so `build-2` clicks the button by its label.
- Settings: none to change. Do not touch the Workflows or Users settings.
- Roles the video relies on: **Editor** (and the other roles the Blog recipe creates: Moderator, Author, Contributor;
  `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). The `admin` user is in **Administrator**.
- Seed data: **none**. The site starts with no workflow, and no user other than `admin`. Everything else is created on
  camera, with these fake values only:

  | Where | Value |
  |---|---|
  | Workflow names | `Account created`, `Role changed`, `Welcome back` |
  | Notification messages | `A user account was created.`, `A user account was updated.`, `Welcome back` |
  | User name | `jamie.chen` |
  | Email | `jamie.chen@example.com` |
  | Password of `jamie.chen` | Made by the **Generate password** button. Never read, never copied, never typed, and the eye icon of the field is never clicked. |
  | Sign-in | User `admin`, password `<password>` (`VIDEO_PASSWORD`), typed by the scene into a password field: only dots show. |

- Browser: the standard recorder viewport (1536x864 CSS pixels, device scale 1.25, light color scheme, `en-US`).
- The admin menu is collapsed to the entries the clip uses. Expand a menu before `StartCaptureAsync` when the clip
  starts on a page that already needs it open.

### Snapshots

Take a snapshot (stop the site, copy `App_Data` and `state.json` aside, start the site again) at each point below.

| Snapshot | Taken | State |
|---|---|---|
| S0 | After Auto Setup, `Recorder setup`, and enabling Workflows | No workflow, one user |
| S2 | After `02-build` | Workflow **Account created**, published |
| S3 | After `03-create` | S2, plus the user `jamie.chen` and one instance |
| S4 | After `04-roles` | S3, plus workflow **Role changed**, published, `jamie.chen` in **Editor**, one more instance |
| S5 | After `05-signin` | S4, plus workflow **Welcome back**, published, one more instance |

To record a clip again, restore the snapshot before it (S0 before `02-build`, S2 before `03-create`, S3 before
`04-roles`, S4 before `05-signin`, S5 before `06-instances`). `06-instances` only reads, so it can be recorded again
without restoring anything.

## The order the clips must be recorded in

`02-build`, `03-create`, `04-roles`, `05-signin`, `06-instances`: the same order the video plays them in, because
each clip needs what the one before it made.

| Clip | Needs | Changes the site |
|---|---|---|
| `02-build` | S0 | Yes: creates and publishes the workflow **Account created** |
| `03-create` | The published **Account created** (S2) | Yes: creates the user `jamie.chen`, and one instance |
| `04-roles` | The user `jamie.chen` (S3), the role **Editor** | Yes: creates and publishes **Role changed**, saves `jamie.chen` with the role **Editor**, one more instance |
| `05-signin` | S4 | Yes: creates and publishes **Welcome back**, signs `admin` out and in again, one more instance |
| `06-instances` | S5 | No |

`05-signin` ends the signed-in session of `admin` on purpose and signs in again, so it is recorded after `04-roles`.
If the browser holds no session when `06-instances` starts, run `Recorder setup` again first.

## Things that apply to every clip

- **Notifications.** A Notify Task adds its message to the notifier. The message is carried by a cookie through the
  redirect and shown on the next page, in the admin as a toast at the top right
  (`TheAdmin/Views/NotifyMessages.cshtml`, `TheAdmin/Views/Message.cshtml`). The toast of a Notify Task has no
  dismissal delay, so it stays until it is closed. The confirmation of the Users module ("User created successfully.",
  "User updated successfully.") hides itself after five seconds (`NotifierExtensions.cs:71`), so it is not part of
  the story and may be gone when a step ends.
- **The designer** (`Workflows/Views/WorkflowType/Edit.cshtml`) is a page of its own, with the Activities pane on the
  left, the canvas in the middle, and the workflow panel on the right. Adding an activity that has an editor opens the
  **activity panel** over the bottom of the canvas (`Workflows/Assets/designer/src/App.vue:274`). Close it with
  `[data-cy='activity-panel-close']` whenever it covers the outcome port or the node the next step needs, and keep it
  open for the step that shows its settings.
- **Changes apply when a field loses focus.** After typing a value in the activity panel, press Tab, and wait until
  the toolbar says all changes are saved, before selecting Publish.
- **Selectors of the designer** (`data-cy` attributes in `Workflows/Assets/designer/src`):
  `start-picker-UserCreatedEvent`, `toolbox-search`, `toolbox-activity-UserUpdatedEvent`,
  `toolbox-activity-UserLoggedInEvent`, `quick-add`, `quick-add-search`, `quick-add-NotifyTask`, `start-badge`,
  `toolbar-publish`, `activity-panel-close`. An outcome port is the button `button.wfd-port[data-outcome='Done']`.
- **Keep the actions of a step shorter than its narration.** Some steps below hold a chain of page loads: use the
  shortest `pauseAfter` that keeps each click readable.
- **Timing of the workflow's own notification.** It appears on the page that follows the action, so a step that ends
  with a redirect must leave time for that page to load before the narration of the next step starts.

## Clips

### `02-build` (chapter 02, Respond to a created account)

Starts on `/Admin/Workflows/Types` (Design, Workflows), before the capture, with the Design menu expanded. The list is
empty: it shows "Nothing here! There are no workflow types for the moment."
(`Workflows/Views/WorkflowType/Index.cshtml:34`). Signed in as `admin` from `state.json`.

- `build-1`: Highlight the menu entry **Workflows** under **Design** for about two seconds (caption "Design, Workflows"),
  clear the highlight, then `ClickAndWaitForNavigationAsync` on the link **Create Workflow**. On the page **Create
  Workflow**, `TypeAsync` into the field **Name** the text `Account created`. Leave **More settings** closed. Then
  `ClickAndWaitForNavigationAsync` on the button **Create and open the designer**. Wait until
  `[data-cy='start-picker']` is visible. At the end of the narration the designer shows the heading **Start with an
  event** with its buttons, the toolbar with the link **Workflows** and the name **Account created**, and the empty
  canvas. Labels: `Workflows/AdminMenu.cs:18-19`, `Workflows/Views/WorkflowType/Index.cshtml:25`,
  `Workflows/Views/WorkflowType/Create.cshtml:21` and `:131`, `Workflows/Services/WorkflowsDesignerJSLocalizer.cs:334`.
- `build-2`: `ClickAsync` on `[data-cy='start-picker-UserCreatedEvent']`, the button labeled **User Created Event**.
  Wait until `[data-cy='start-badge']` is visible. The activity panel opens (the event has an editor, with no field in
  it): leave it open. At the end of the narration the canvas shows the node **User Created Event** with the badge
  **Start**. Labels: `Workflows/Assets/designer/src/canvas/StartPicker.vue:13-14` and `:39`,
  `Workflows/Assets/designer/src/canvas/ActivityNode.vue:234-236`, `Users/Workflows/Activities/UserCreatedEvent.cs:17`.
  The first event added to a workflow becomes its start activity (`Workflows/Services/WorkflowTypeDraftManager.cs:175`).
- `build-3`: Close the activity panel if it covers the node. `ClickAsync` on `button.wfd-port[data-outcome='Done']` of
  the event (the port labeled **Done**). Wait for `[data-cy='quick-add']` (title "Add the next activity, after Done"),
  `TypeAsync` into `[data-cy='quick-add-search']` the text `notify`, and `ClickAsync` on
  `[data-cy='quick-add-NotifyTask']`, the entry **Notify Task** (category **UI**). Wait for the second node. At the end
  of the narration the canvas shows **User Created Event** connected to **Notify Task**, to its right. Labels:
  `Workflows/Services/WorkflowsDesignerJSLocalizer.cs:333`, `Workflows/Activities/NotifyTask.cs:34`,
  `Users/Workflows/Activities/UserActivity.cs` (the outcome **Done**).
- `build-4`: The activity panel of **Notify Task** is open, on its Settings tab (the form is inside
  `[data-cy='panel-activity-form']`; find the fields by their labels). `SelectAsync` the select
  **Notification Type** to `Information` (the select starts on `Success`, the first value of the list:
  `Workflows/Activities/NotifyTask.cs:44`). `TypeAsync` into **Message** the text `A user account was created.`, then
  press Tab. **Message** is a plain one-line text box, with no syntax picker and no code editor beside it
  (`Workflows/ViewModels/NotifyTaskViewModel.cs`, `Workflows/Views/Items/NotifyTask.Fields.Edit.cshtml:16-18`). Wait until the toolbar shows that all changes are saved. At the end of the narration the panel shows
  **Notification Type** set to **Information** and **Message** with the text, and the node shows the message in
  quotes. Labels: `Workflows/Views/Items/NotifyTask.Fields.Edit.cshtml:6` and `:15`. The hint under **Message** reads
  "The message to show. With Liquid support." (`:19`): see `OPEN-QUESTIONS.md`.
- `build-5`: `ClickAsync` on `[data-cy='toolbar-publish']` (labeled **Publish**). No dialog is expected (no warning).
  Wait for a toast that starts with "The workflow was published as version" and do not wait for a number: creating a
  workflow saves an empty first version (`Workflows/Controllers/WorkflowTypeController.cs:275`,
  `Workflows/Services/WorkflowTypeStore.cs:87`, `Workflows/Services/WorkflowTypeVersionStore.cs:95-111`), so the first
  publish is expected to say version 2 (see `OPEN-QUESTIONS.md`). Publish applies the open form and saves the draft
  first (`Workflows/Assets/designer/src/App.vue:387-391`). At the end of the narration the toast is visible and the
  toolbar shows that all changes are saved. Labels: `Workflows/Services/WorkflowsDesignerJSLocalizer.cs:190` and
  `:221`.

### `03-create` (chapter 03, Create a user)

Starts on `/Admin/Workflows/Types`, before the capture: the list holds the workflow **Account created**. Open the
**Access Control** menu entry only on camera (collapsed before the capture).

- `create-1`: `ClickAsync` the menu entry **Access Control**, then `ClickAndWaitForNavigationAsync` on **Users**, then
  `ClickAndWaitForNavigationAsync` on the button **Add User**. At the end of the narration the page **Create User**
  shows the empty form. Labels: `Users/AdminMenu.cs:27` and `:30`, `Users/Views/UsersAdminListCreate.cshtml:7`,
  `Users/Views/Admin/Create.cshtml:4`.
- `create-2`: `TypeAsync` into **User name** `jamie.chen`, into **Email** `jamie.chen@example.com`, `ClickAsync` on the
  yellow button **Generate password** (a `div` styled as a button, not a `button` element, so find it by its text; it
  fills **Password** and the password confirmation; never click the eye icons). Do not leave any role checked. Then `ClickAndWaitForNavigationAsync` on **Save**, as the last action of the
  step. At the end of the narration the page is either the form just before the redirect, or the Users list: both are
  acceptable, the next step shows the list. Labels: `Users/Views/UserName.Edit.cshtml:4`,
  `Users/Views/UserEmail.Edit.cshtml:4`, `Users/Views/UserFields.Edit.cshtml:39`,
  `Users/Views/UserSaveButtons.Edit.cshtml:2`.
- `create-3`: No navigation. `HighlightAsync` the blue toast with the text "A user account was created." (the
  toast of the notification type Information: `.toast.message-information`). At the end of the narration the Users list
  shows the rows `admin` and `jamie.chen`, with the email next to each name (`Users/Views/UserFields.cshtml:7`; an
  **Unconfirmed** badge can show beside a name whose email is not confirmed, which is expected and holds nothing
  private), and the toast of the workflow. Label:
  `Workflows/Activities/NotifyTask.cs` (the message is the one typed in `build-4`). The workflow ran when the user was
  created: `Users/Workflows/Handlers/UserEventHandler.cs:18`, `OrchardCore.Users.Core/Services/UserStore.cs:114`.

### `04-roles` (chapter 04, Respond to a role change)

Starts on `/Admin/Users` (the Users list), before the capture, with the **Design** menu expanded and no toast on the
page.

- `roles-1`: `ClickAndWaitForNavigationAsync` on **Workflows** under **Design**, then on **Create Workflow**,
  `TypeAsync` into **Name** `Role changed`, then `ClickAndWaitForNavigationAsync` on **Create and open the designer**.
  Use short pauses: this step holds three page loads. At the end of the narration the designer shows **Start with an
  event** and the name **Role changed**. Labels: as in `build-1`.
- `roles-2`: `TypeAsync` into `[data-cy='toolbox-search']` (placeholder "Search activities") the text `user updated`.
  `ClickAsync` on `[data-cy='toolbox-activity-UserUpdatedEvent']`, the card **User Updated** (text "User is
  updated."). At the end of the narration the canvas shows the node **User Updated Event** with the badge **Start**.
  The search lists only that card, because the search matches the words `user` and `updated` in the activity name and
  its category. Labels: `Users/Views/Items/UserUpdatedEvent.Fields.Thumbnail.cshtml:1-2`,
  `Users/Workflows/Activities/UserUpdatedEvent.cs:17`,
  `Workflows/Assets/designer/src/toolbox/ActivityToolbox.vue:52` and `:107`.
- `roles-3`: As `build-3` and `build-4` in one step: close the activity panel if it covers the node,
  `ClickAsync` on the **Done** port, `TypeAsync` `notify` into the quick-add search, pick **Notify Task**, `SelectAsync`
  **Notification Type** to `Information`, `TypeAsync` into **Message** `A user account was updated.`, press Tab, and
  wait until all changes are saved. At the end of the narration the canvas shows **User Updated Event** connected to
  **Notify Task**, and the panel shows the two settings. This is the longest chain of the clip: use short pauses.
  Labels: as in `build-3` and `build-4`.
- `roles-4`: `ClickAsync` on `[data-cy='toolbar-publish']`, and wait for a toast that starts with "The workflow was
  published as version" (no number, as in `build-5`). At the end of the narration the toast is visible.
- `roles-5`: `ClickAsync` the menu entry **Access Control**, `ClickAndWaitForNavigationAsync` on **Users**. In the row of
  `jamie.chen`, `ClickAndWaitForNavigationAsync` on its **Edit** link (an `a` element styled as a button, with no button role). In the list **Roles**, `ClickAsync` the check
  box labeled **Editor** (add the role; do not remove any role). `ClickAndWaitForNavigationAsync` on **Save**, as the
  last action. At the end of the narration the page is the edit form with **Editor** checked, or the Users list.
  Labels: `Users/AdminMenu.cs:27` and `:30`, `Users/Views/UserButtons.cshtml:11`,
  `Users/Views/UserRoleFields.Edit.cshtml:6`, `Users/Views/UserSaveButtons.Edit.cshtml:2`. A role is added here and not
  removed on purpose: removing a role also updates the security stamp of the user, which saves the account a second
  time (`Users/Drivers/UserRoleDisplayDriver.cs:147`) and would start the workflow twice.
- `roles-6`: No navigation. `HighlightAsync` the toast "A user account was updated." (blue, `.toast.message-information`).
  At the end of the narration the Users list shows the toast of the workflow. The edit saved the account:
  `Users/Controllers/AdminController.cs:389`, `OrchardCore.Users.Core/Services/UserStore.cs:235`,
  `Users/Workflows/Handlers/UserEventHandler.cs:38`.

### `05-signin` (chapter 05, Respond to a sign-in)

Starts on `/Admin/Users`, before the capture, with the **Design** menu expanded. The session of `admin` is the one from
`state.json`.

- `signin-1`: As `roles-1`, with the name `Welcome back`.
- `signin-2`: `TypeAsync` into `[data-cy='toolbox-search']` the text `loggedin`. `ClickAsync` on
  `[data-cy='toolbox-activity-UserLoggedInEvent']`, the card **User Logged in** (text "User Logged in."). At the end of
  the narration the canvas shows the node **User Loggedin Event** with the badge **Start**. The three labels of this
  activity differ: the card says **User Logged in**, the node header says **User Loggedin Event**, and the body of the
  node says **User LoggedIn**; the narration says what is on screen at that moment. Labels:
  `Users/Views/Items/UserLoggedInEvent.Fields.Thumbnail.cshtml:1-2`,
  `Users/Workflows/Activities/UserLoggedInEvent.cs:17`,
  `Users/Views/Items/UserLoggedInEvent.Fields.Design.cshtml:4`.
- `signin-3`: As `roles-3`, with the message `Welcome back` (no full stop). At the end of the narration the canvas
  shows **User Loggedin Event** connected to **Notify Task**, and the panel shows the two settings.
- `signin-4`: `ClickAsync` on `[data-cy='toolbar-publish']`, wait for the toast, then `ClickAsync` on the user menu of
  the top bar: in the admin the toggle `#navbarDropdown` shows only a user icon, and the name `admin` is its tooltip
  (`Users/Views/UserMenu.DetailAdmin.cshtml:7`, `Users/Views/UserMenuItems-Title.DetailAdmin.cshtml:1`). The menu
  opens with the line "Signed in as admin" (`Users/Views/UserMenuItems-SignedUser.cshtml:4`). Then
  `ClickAndWaitForNavigationAsync` on the button **Log off**. At the end of the narration the site shows the home page
  of the front end, signed out. Label: `Users/Views/UserMenuItems-SignOut.cshtml:4`, `Users/Controllers/AccountController.cs:225`.
- `signin-5`: `NavigateAsync("/Login?ReturnUrl=%2FAdmin%2FWorkflows%2FTypes")`. `TypeAsync` into **Username or email
  address** the text `admin`, and into **Password** the value of `VIDEO_PASSWORD` (the field shows dots; do not click
  the eye button). `ClickAndWaitForNavigationAsync` on **Log in**. Wait for the Workflows list. At the end of the
  narration the Workflows list shows the three workflows, and the toast "Welcome back" (blue). Type the right password
  the first time: a failed attempt followed by a good one saves the account and also starts the workflow **Role
  changed** (see `OPEN-QUESTIONS.md`). Labels: `Users/Views/LoginFormCredentials.cshtml:7` and `:13`,
  `Users/Views/LoginForm.Edit.cshtml:9`, `Users/Controllers/AccountController.cs:173`,
  `Users/Controllers/AccountBaseController.cs:26`.

### `06-instances` (chapter 06, Follow the runs)

Starts on `/Admin/Workflows/Types` (where `05-signin` ended), before the capture, signed in as `admin`, with no toast
on the page: the toast "Welcome back" has no dismissal delay and stays until it is closed, so reload the page (the
cookie that carried it is already used) before the capture. The list holds the three workflows.

- `instances-1`: `ClickAndWaitForNavigationAsync` on the button **Instances** (`[data-cy='all-instances']`). At the end
  of the narration the page **Instances** shows the buttons **All**, **Halted**, **Running**, **Faulted**, **Finished**
  and **Aborted** with their counts, and a list of rows, each with the name of its workflow, the identifier of the
  instance, and a green badge **Finished**. Labels: `Workflows/Views/WorkflowType/Index.cshtml:24`,
  `Workflows/Views/Workflow/Index.cshtml:39-44`,
  `Workflows/Views/WorkflowEntry.DefaultTags.SummaryAdmin.cshtml:19`.
- `instances-2`: In the row of the workflow **Account created** (the row that has that name as its first link),
  `ClickAndWaitForNavigationAsync` on the identifier of the instance (the second link of the row, after the workflow
  name). Wait for `#workflow-designer`. `HighlightAsync` the line **Correlation ID**
  for about two seconds. At the end of the narration the page shows **Workflow ID**, **Created**, **Status**
  (**Finished**), **Correlation ID**, and the read-only designer with the activities the instance ran highlighted.
  Labels: `Workflows/Views/Workflow/Details.cshtml:46` and `:61` (**Correlation ID**).

### Slides (`01-events`, chapter 01)

Nothing to record: `slides.py` draws them. One line per step.

- `events-1`: "Events about accounts": four cards (User Created Event, User Updated Event, User Confirmed Event, Enabled,
  Disabled and Deleted); the first two are bright.
- `events-2`: the same slide, with the last two cards bright.
- `events-3`: "Events about sign-ins": User Loggedin Event, User Loggedout Event, and what the workflow receives.
- `events-4`: "Tasks about users": Assign User Role Task, Validate User Task, Register User Task.

The events come from `Users/Workflows/Startup.cs:21-33`; Register User Task needs the Email feature
(`Users/Workflows/Startup.cs:36`).

## Checks for private data

- The only people on screen are `admin` and `jamie.chen`, the only email addresses are `admin@example.com` and
  `jamie.chen@example.com`, and no screen shows a password: the fields show dots, the eye icons are never clicked, and
  the scene types `VIDEO_PASSWORD` without echoing it.
- OCR the video at one frame per second and at every scene change, then search the text for `@` followed by anything
  other than `example.com`, for the password, and for the name of the machine or the folders (the browser address bar
  is not recorded).
- The identifiers of workflow instances and the correlation id are generated: they are not private.
- Look at every contact sheet: no error message, no empty list where the narration talks about a row, and no toast
  that belongs to another step.

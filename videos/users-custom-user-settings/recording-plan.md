# Recording plan: Custom User Settings

How to record `users-custom-user-settings` without deciding anything. Read `storyboard.json` for the narration and
`OPEN-QUESTIONS.md` for what the source cannot settle. Paths such as `OrchardCore.Modules/...` are relative to `src/`
of the OrchardCore checkout the video was written from; the commit is in `catalog/features.json` (`source.commit`).

In this plan a selector in backticks stands for `scene.Page.Locator(...)`, since the recorder's methods take locators
(Playwright selector strings: `text="..."` is an exact text match, `:text-is("...")` an exact match on an element,
`>>` chains). Clicks are labeled exactly as the Orchard Core source spells them.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of the checkout named above, on `http://localhost:5010`, with its own
  `ORCHARD_APP_DATA` folder outside both repositories, provisioned by Auto Setup (see `references/recording.md`).
- **Auto Setup**: recipe **Blog**, tenant `Default`, site name **Orchard Core**, time zone `America/Los_Angeles`, user
  `admin`, email `admin@example.com`, and a password chosen outside git and passed in `VIDEO_PASSWORD` (write
  `<password>` anywhere it is mentioned, never a real one). The Setup screen is not part of this video.
- **Features that are already on after the Blog recipe** and that the clips use: Users, Roles, Contents, Content Types,
  Content Fields (Text Field, Boolean Field), Features, Deployment, Navigation, TheAdmin as the admin theme.
- **Feature that must be OFF at the start**: Custom User Settings (`OrchardCore.Users.CustomUserSettings`). Clip
  `02-enable` turns it on, on camera. Do not enable it while seeding.
- **Settings**: none changed. The admin lists keep their default layout (List, with buttons in each row): the content
  type badge of `section-8` and the buttons of `user-1` appear in that layout. Leave the admin menu as it is after
  Setup: its default behavior is "focused", which draws every group collapsed except the one that holds the current
  page, forgets which groups were opened, and closes the other groups of a level when one opens
  (`OrchardCore.Admin.Abstractions/Models/AdminMenuBehavior.cs:6-12`, `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts:15-18,147-163`).
  Every clip starts on `/Admin`, which is in no group, so all groups are collapsed at the start; a click on an open
  group closes it. `Recorder setup` signs in before any clip is recorded.
- **External services**: none. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address so the site never calls out. No
  mail server, SMS provider or vault is needed.
- **Seed data**, created outside the capture, with the admin UI, in this order (all fake):

  | What | Where | Values |
  |---|---|---|
  | User | Access Control, Users, Add User | User name `jamie`, Email `jamie@example.com`, a password you choose and do not show |
  | User | Access Control, Users, Add User | User name `priya`, Email `priya@example.com`, a password you choose and do not show, role Editor |
  | Deployment plan | Tools, Deployments, Plans, Add Deployment Plan | Name `Team profiles` (no steps) |

  The Users list then shows `admin`, `jamie` and `priya` with their `@example.com` addresses. If an **Unconfirmed**
  badge appears beside a seeded user, choose **Confirm email** in that row's actions menu before the snapshot. The
  `Editor` role comes from the Blog recipe (it is listed under Access Control, Roles); change nothing in it while
  seeding.

## Order of the clips and the snapshots

Every clip from `02-enable` on changes the site, and each one starts from what the previous one left, so record them
in the order they play. Take a snapshot (stop the site, copy its `App_Data` folder and `state.json` aside) at the
points below, and restore the matching one, then start the site again, before a new take of a clip.

| Clip | Starts from | Changes the site | Snapshot to restore for a new take |
|---|---|---|---|
| `02-enable` | S0: seeded, Custom User Settings off | Turns the feature on | S0 |
| `03-section` | S1: feature on | Creates the content type `UserProfile` with two fields and the stereotype | S1 |
| `04-user` | S2: the section exists | Saves the nickname and the Newsletter box of `jamie` | S2 |
| `05-permissions` | S3: any state after `03-section` | Allows the permission for the `Editor` role | S3 (or S2) |
| `06-deploy` | S4: feature on, plan `Team profiles` has no step | Adds one step to the plan | S4 (or S2) |

S0 is the snapshot after seeding, S1 after `02-enable`, S2 after `03-section`, S3 after `04-user`, S4 after
`05-permissions`. `05-permissions` and `06-deploy` need the section from `03-section` (the permission and the card
only exist once the feature is on, and the permission row only once the type has the stereotype).

Narrate first (`narrate.py`), then record. Each step holds until its narration ends, so keep a step's actions shorter
than its narration: the budget in seconds is given for each step (the real lengths are in `durations.json`).

Every clip below starts on the admin dashboard, `/Admin`, opened with `scene.OpenAsync` before the capture starts, with
all menu groups collapsed, and signed in as `admin` through `state.json`.

## Clip `02-enable` (chapter 2, Enable the feature)

Starts on `/Admin`. Menu entries are in `#left-nav`.

- `enable-1` (about 10 s). Click the menu group `#left-nav >> text="Tools"`, then the entry `#left-nav >> text="Features"`
  with `ClickAndWaitForNavigation`. On the Features page, `TypeAsync` into `#search-box` the text `Custom User Settings`
  (the list filters while typing: rows that do not match only get `d-none`, they stay in the page). Then `HighlightAsync`
  on the dependencies of that one row, `.list-group-item:has(#btn-enable-OrchardCore_Users_CustomUserSettings) .direct-dependencies`,
  with the caption `Dependencies`. A bare `.direct-dependencies` matches one element for each feature that has
  dependencies, and the recorder then draws no highlight.
  *Visible when the narration ends*: the Features page, the search box holding `Custom User Settings`, one row
  **Custom User Settings** with its id `OrchardCore.Users.CustomUserSettings`, the description "The custom user settings
  feature allows content types to become custom user settings.", the badges **Users** and **Contents**, and the
  **Enable** button. *Source*: `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features);
  `OrchardCore.Features/Views/Admin/Features.cshtml:27` (Search box);
  `OrchardCore.Users/Manifest.cs:91-100` (name, description, dependencies);
  `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11` and `FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12-19`
  (name, id, dependency badges).
- `enable-2` (about 5 s). `ClearHighlightsAsync`, then `ClickAndWaitForNavigation` on
  `#btn-enable-OrchardCore_Users_CustomUserSettings` (the **Enable** button of that row; the id is the feature id with
  the dots replaced by underscores).
  *Visible*: the Features page with the green message "The feature Custom User Settings was enabled." and the row now
  offering **Disable**. *Source*: `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable);
  `OrchardCore.Features/Controllers/AdminController.cs:317` (message).

## Clip `03-section` (chapter 3, Create a section)

Starts on `/Admin` (snapshot S1). The content type is created and configured with the pages below; type the values
exactly as given.

- `section-1` (about 14 s). Click `#left-nav >> text="Design"`, `#left-nav >> text="Content Definition"`, and
  `#left-nav >> text="Content Types"` (this one with `ClickAndWaitForNavigation`; it opens the Content Types list). Then
  `ClickAndWaitForNavigation` on `#btnCreate` (**Create new type**). *Visible*: the New Content Type page, with the
  **Display Name** field focused and **Technical Name** below it. *Source*:
  `OrchardCore.ContentTypes/AdminMenu.cs:23-25` (Design, Content Definition, Content Types);
  `OrchardCore.ContentTypes/Views/Admin/List.cshtml:20` (Create new type);
  `OrchardCore.ContentTypes/Views/Admin/Create.cshtml:5,14,22` (page title, labels). The warning about existing types
  comes from the documentation page of the module (`src/docs/reference/modules/Users/CustomUserSettings/README.md`).
- `section-2` (about 6 s). `TypeAsync` into `input[data-displayname]` the text `User Profile`, press Tab (the Technical
  Name `UserProfile` fills in), `PauseAsync(700)`, then `ClickAndWaitForNavigation` on `button.create` (**Create**).
  *Visible*: the Add Parts page, with the message "The "User Profile" content type has been created." and the text
  "Choose the Parts to add to this Content Type.". *Source*: `OrchardCore.ContentTypes/Views/Admin/Create.cshtml:14,22,30`;
  `OrchardCore.ContentTypes/Controllers/AdminController.cs:162-164`;
  `OrchardCore.ContentTypes/wwwroot/Scripts/autocomplete/autocomplete.js` (Technical Name from the Display Name).
- `section-3` (about 7 s). `HighlightAsync` on `form ul.list-group` with the caption `Parts`, `PauseAsync(1500)`,
  `ClearHighlightsAsync`, then `ClickAndWaitForNavigation` on `a.cancel` (**Cancel**). *Visible*: the Edit Content Type -
  User Profile page: Description, the check boxes Creatable, Listable, Draftable, Versionable and Securable (all
  checked), Stereotype, Category, Thumbnail, the **Fields** heading with **Add Field**, and the **Parts** list. *Source*:
  `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:18,48`;
  `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63,88,137,144`;
  `OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:85-89` (a type starts with the five boxes checked).
- `section-4` (about 9 s). `ClickAndWaitForNavigation` on `a:text-is("Add Field")` (it opens "Add New Field To "User
  Profile""). `TypeAsync` into `input[data-displayname]` the text `Nickname`; click `label:text-is("Text Field")`; click
  `button.save` (**Save**) with `ClickAndWaitForNavigation`. Keep each pause short (300 ms). *Visible*: the Edit Content
  Type - User Profile page, the message "The field "Nickname" has been added.", and under **Fields** a row **Nickname**
  with the hint **Text Field**. *Source*: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:137`;
  `OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml:20,38,46,54,71`;
  `OrchardCore.ContentTypes/Controllers/AdminController.cs:690` (field types are listed in alphabetical order, so
  Boolean Field is the one selected first), `:768`; the labels "Text Field" and "Boolean Field" are the field names
  written in words (`AddFieldTo.cshtml:62`).
- `section-5` (about 9 s). Same actions: `Add Field`, `TypeAsync` the text `Newsletter` into `input[data-displayname]`,
  click `label:text-is("Boolean Field")` (already selected: the click shows the choice), `button.save`. *Visible*: the
  type page with both rows under **Fields**: **Nickname** (Text Field) and **Newsletter** (Boolean Field), and the
  message "The field "Newsletter" has been added.". *Source*: as for `section-4`.
- `section-6` (about 9 s). Click the labels `label:text-is("Creatable")`, `label:text-is("Listable")`,
  `label:text-is("Draftable")`, `label:text-is("Versionable")`, `label:text-is("Securable")`, so all five boxes are
  unchecked. Leave Stereotype empty. Scroll if the **Save** button is below the fold (clear highlights first), then
  `ClickAndWaitForNavigation` on `button.save:text-is("Save")` (the plain **Save**, never the arrow beside it: the
  choice "and continue" is stored in the admin preferences and would change the label of the button for later takes).
  *Visible*: the Content Types list with the message "Content type updated successfully." and the row **User Profile**
  (technical name `UserProfile`) with the buttons **List Items**, **Edit** and **Delete**, and no **Create New User
  Profile** button. *Source*: `OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:17,30,43,56,69`;
  `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:215`; `OrchardCore.ContentTypes/Controllers/AdminController.cs:225-230`;
  `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19-24` (the Create New button needs
  Creatable). Why it is cleared first: `OrchardCore.ContentTypes/Editors/ContentTypeSettingsDisplayDriver.cs:58-102`
  (a save that carries the stereotype ignores the settings the stereotype hides) and `OrchardCore.Users/Startup.cs:687-693`.
- `section-7` (about 16 s). `ClickAndWaitForNavigation` on
  `:is(li, tr):has-text("UserProfile") >> a:text-is("Edit")`. `TypeAsync` into `input[list="stereotypeOptions"]` the text
  `CustomUserSettings` (exactly this spelling; a headless browser draws no suggestion list, so type the whole value).
  Click `button.save:text-is("Save")` with `ClickAndWaitForNavigation`. *Visible*: the Content Types list again, with the
  message "Content type updated successfully.". *Source*: `OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:77-87`
  (the Stereotype box and its suggestions: value `CustomUserSettings`, shown as "Custom User Settings");
  `OrchardCore.Users/Services/CustomUserSettingsStereotypesProvider.cs:18`;
  `OrchardCore.Users/Drivers/CustomUserSettingsDisplayDriver.cs:121` (the value is compared exactly);
  `OrchardCore.ContentTypes/Editors/ContentTypeSettingsDisplayDriver.cs:64` (letters and digits only). The narration's
  "hides Creatable, Listable, Draftable and Versionable and leaves their values alone" comes from
  `OrchardCore.Users/Startup.cs:685-693` (the four `Show...` options are false for the stereotype) and
  `OrchardCore.ContentTypes/Editors/ContentTypeSettingsDisplayDriver.cs:76-102` (a hidden setting is not applied on save);
  Securable is not hidden and stays on the page.
- `section-8` (about 11 s). `HighlightAsync` on `.badge:has-text("CustomUserSettings")` with the caption `Stereotype`,
  then hold. *Visible*: the row **User Profile** with the badge **CustomUserSettings** (tooltip "Stereotype"), and the
  highlight. *Source*: `OrchardCore.ContentTypes/Views/ContentTypeEntry.DefaultTags.SummaryAdmin.cshtml:13-15`.

## Clip `04-user` (chapter 4, Fill in a user's section)

Starts on `/Admin` (snapshot S2).

- `user-1` (about 9 s). Click `#left-nav >> text="Access Control"`, then `#left-nav >> text="Users"` with
  `ClickAndWaitForNavigation` (the Users list). `ClickAndWaitForNavigation` on
  `:is(li, tr):has-text("jamie@example.com") >> a:text-is("Edit")`. Then `HighlightAsync` on
  `a[role="tab"]:text-is("User Profile")` with the caption `User Profile`. *Visible*: the Edit User page of `jamie` with a
  row of two tabs, the first one holding the account fields and **User Profile** as the second one, highlighted.
  *Source*: `OrchardCore.Users/AdminMenu.cs:27-30` (Access Control, Users); `OrchardCore.Users/Views/UserButtons.cshtml:11`
  (Edit); `OrchardCore.Users/Views/Admin/Edit.cshtml:4` (Edit User);
  `OrchardCore.Users/Drivers/CustomUserSettingsDisplayDriver.cs:90` (the tab is named after the type's display name);
  `OrchardCore/OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:14,62,87` and `Shapes/GroupShapes.cs:30,56,59` (the
  tab row; the first tab is the ungrouped "Content" group).
- `user-2` (about 8 s). `ClearHighlightsAsync`, click `a[role="tab"]:text-is("User Profile")`, then `HighlightAsync` on
  `.field-wrapper-user-profile-nickname` (caption `Text Field`) and on `.field-wrapper-user-profile-newsletter` (caption
  `Boolean Field`). *Visible*: the tab pane with a **Nickname** text box and a **Newsletter** check box.
  *Source*: `OrchardCore.ContentFields/Views/TextField.Edit.cshtml:19-20`;
  `OrchardCore.ContentFields/Views/BooleanField.Edit.cshtml:11,15-16` (label = the field's display name). The wrapper
  class is built from the part name (`UserProfile`) and the field name, each passed through `HtmlClassify`, which splits
  camel case first, so `UserProfile` becomes `user-profile` (`OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:14-31,77-110`).
- `user-3` (about 7 s). `ClearHighlightsAsync`; `TypeAsync` into `.field-wrapper-user-profile-nickname input.form-control` the
  text `Jamie`; click `.field-wrapper-user-profile-newsletter label.form-check-label` (the box becomes checked); click
  `button.save:text-is("Save")` with `ClickAndWaitForNavigation`. *Visible*: the Users list with the message "User updated
  successfully.". *Source*: `OrchardCore.Users/Views/UserSaveButtons.Edit.cshtml:2`;
  `OrchardCore.Users/Controllers/AdminController.cs:406,418`.
- `user-4` (about 7 s). `ClickAndWaitForNavigation` on `:is(li, tr):has-text("jamie@example.com") >> a:text-is("Edit")`, then
  click `a[role="tab"]:text-is("User Profile")`. *Visible*: the tab with `Jamie` in **Nickname** and **Newsletter**
  checked. *Source*: `OrchardCore.Users/Drivers/CustomUserSettingsDisplayDriver.cs:98-117,123-143` (the values are saved
  on the user and read back).
- `user-5` (about 9 s). Click the **Users** link of the breadcrumb (`.breadcrumb a:text-is("Users")`, with
  `ClickAndWaitForNavigation`), then `ClickAndWaitForNavigation` on `a:text-is("Add User")`. Do not save. *Visible*: the
  Create User page with the row of tabs, **User Profile** among them. *Source*:
  `OrchardCore.Users/Views/Admin/Edit.cshtml:4-6` (breadcrumb Users); `OrchardCore.Users/Views/UsersAdminListCreate.cshtml:7`
  (Add User); `OrchardCore.Users/Views/Admin/Create.cshtml:4` (Create User);
  `OrchardCore.Users/Controllers/AdminController.cs:281-293` (the same editor, for a new user).

## Clip `05-permissions` (chapter 5, Control who can edit a section)

Starts on `/Admin` (snapshot S3 or S2).

- `perm-1` (about 7 s). Click `#left-nav >> text="Access Control"`, then `#left-nav >> text="Roles"` with
  `ClickAndWaitForNavigation`; `ClickAndWaitForNavigation` on `:is(li, tr):has-text("Editor") >> a:text-is("Edit")`. Keep the
  pauses to 300 ms: this step has the least time. *Visible*: the page "Edit 'Editor' Role", with the **Search
  permissions** box. *Source*: `OrchardCore.Roles/AdminMenu.cs:18-19`; `OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`;
  `OrchardCore.Roles/Views/Admin/Edit.cshtml:6`; `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49`.
- `perm-2` (about 10 s). `TypeAsync` into `#search-box` the text `Custom User`. `HighlightAsync` on
  `tr[data-text="Manage Own Custom User Settings - User Profile"]`. *Visible*: one group, titled **Custom User Settings**
  with `(OrchardCore.Users.CustomUserSettings)` beside it, holding the row **Manage Own Custom User Settings - User
  Profile** with its **Allow** and **Effective** boxes. *Source*: `OrchardCore.Users/CustomUserSettingsPermissions.cs:10-13,30-31`
  (the label and the name `ManageOwnCustomUserSettings_UserProfile`); `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:57-61,119`
  (group title, search by the row text); `OrchardCore.Roles/Controllers/AdminController.cs:412-427` (the group takes the
  feature's name and id).
- `perm-3` (about 10 s). `ClearHighlightsAsync`; click `input[id="Checkbox.ManageOwnCustomUserSettings_UserProfile"]`
  (the **Allow** box); click `button.save:text-is("Save")` with `ClickAndWaitForNavigation`. *Visible*: the Roles list
  with the message "Role updated successfully.". *Source*: `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:131`;
  `OrchardCore.Roles/Views/Admin/Edit.cshtml:33`; `OrchardCore.Roles/Controllers/AdminController.cs:213,220`. The
  statement that the tab shows for the people in the role comes from
  `OrchardCore.Users/Drivers/CustomUserSettingsDisplayDriver.cs:90-92`, and "editing a user is a separate permission"
  from `OrchardCore.Users/Controllers/AdminController.cs:325-355`; it is not recorded (see `OPEN-QUESTIONS.md`).
- `perm-4` (about 9 s). `ClickAndWaitForNavigation` on `:is(li, tr):has-text("Administrator") >> a:text-is("Edit")`, then
  `HighlightAsync` on `.alert-info:has-text("comprehensive access")`. Do not save. *Visible*: the page "Edit 'Administrator' Role" with the blue message "This
  system role provides comprehensive access by granting all available permissions.". *Source*:
  `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:32`; `OrchardCore/OrchardCore.Users.Core/UsersPermissions.cs:14`
  (the label "Manage security settings and all users"); `OrchardCore.Users/CustomUserSettingsPermissions.cs:10-13`
  (the permission is implied by Manage Users).

## Clip `06-deploy` (chapter 6, Export the values)

Starts on `/Admin` (snapshot S4 or S2). Nothing is executed, downloaded or exported.

- `deploy-1` (about 11 s). Click `#left-nav >> text="Tools"`, `#left-nav >> text="Deployments"`, and
  `#left-nav >> text="Plans"` (with `ClickAndWaitForNavigation`); then `ClickAndWaitForNavigation` on
  `:is(li, tr):has-text("Team profiles") >> a:text-is("Manage Steps")`. Keep the pauses to 300 ms. *Visible*: the page
  "Deployment Plan" with the buttons **Execute** and **Add Step**, and the message "Nothing here! There are no steps on
  this plan for the moment.". *Source*: `OrchardCore.Deployment/AdminMenu.cs:18-20` (Tools, Deployments, Plans);
  `OrchardCore.Deployment/Views/DeploymentPlanEntry.Buttons.SummaryAdmin.cshtml:6` (Manage Steps);
  `OrchardCore.Deployment/Views/DeploymentPlan/Display.cshtml:12,25,30-31,57`.
- `deploy-2` (about 10 s). Click `button:text-is("Add Step")`; wait for `#modalSteps` ("Available Steps") to be visible;
  `TypeAsync` into `#modalSteps #search-box` (the **Filter** box) the text `Custom User`; `HighlightAsync` on
  `#modalSteps .deployment-step-item:has-text("Custom User Settings")`; hold for a second and a half; `ClearHighlightsAsync`;
  `ClickAndWaitForNavigation` on `#modalSteps .deployment-step-item:has-text("Custom User Settings") >> a:text-is("Add")`.
  *Visible*: the step page titled **Custom User Settings** (breadcrumb: Deployment Plans, Team profiles). The card
  read "Custom User Settings" and "Exports all or specified custom user settings.". *Source*:
  `OrchardCore.Deployment/Views/DeploymentPlan/Display.cshtml:31,68,75,96`; `OrchardCore.Deployment/Views/DeploymentStep.Thumbnail.cshtml:8`
  (Add); `OrchardCore.Users/Views/Items/CustomUserSettingsDeploymentStep.Fields.Thumbnail.cshtml:1-2`;
  `OrchardCore.Users/Startup.cs:682` (the step is registered by the feature).
- `deploy-3` (about 9 s). `HighlightAsync` on `label:has-text("Include all custom user settings.")` with the caption
  `Default: all`, `PauseAsync(2500)`, `ClearHighlightsAsync`, then `ClickAndWaitForNavigation` on `button.create`
  (**Create**). Do not click the check box: it is checked when the page opens, and clearing it would show the list "The
  custom user settings to add as part of the plan." with a check box `UserProfile`, but its collapse target is the page-wide
  selector `.collapse`, which the admin menu groups also carry (`NavigationItem-admin.cshtml:93` of TheAdmin), so the
  click may open or close menu groups on camera (see `OPEN-QUESTIONS.md`). The narration says what clearing it does; it
  does not need to be shown. *Visible*: the plan page, the message "Deployment plan step added successfully.". *Source*:
  `OrchardCore.Users/Views/Items/CustomUserSettingsDeploymentStep.Fields.Edit.cshtml:8-10,16-19`;
  `OrchardCore.Users/Deployment/CustomUserSettingsDeploymentStep.cs:17` (all sections are included by default);
  `OrchardCore.Deployment/Views/Step/Create.cshtml:53`; `OrchardCore.Deployment/Controllers/StepController.cs:112-113`.
- `deploy-4` (about 12 s). `HighlightAsync` on `#stepOrder .badge:text-is("All")` with the caption `All sections`, then hold.
  *Visible*: the step listed on the plan with the heading **Custom User Settings**, the badge **All**, and the buttons
  **Edit** and **Delete**. *Source*: `OrchardCore.Users/Views/Items/CustomUserSettingsDeploymentStep.Fields.Summary.cshtml:8-12`;
  `OrchardCore.Deployment/Views/DeploymentStep.Summary.cshtml:12,20` (the Edit and Delete buttons). That an import matches users by
  their user ID and skips the ones it does not find comes from `OrchardCore.Users/Recipes/CustomUserSettingsStep.cs:37-48`;
  it is not recorded (see `OPEN-QUESTIONS.md`).

## Slides

The slides are drawn by `slides.py`; nothing is recorded for them.

- `what-1`: three cards, "Built-in details", "Your own details", "Defined in the admin".
- `what-2`: three cards, "A content type", "A stereotype", "A tab".
- `what-3`: four cards, the tasks of the video, in order.

## Checks for private data

- Every name, email address and company on screen is invented: `admin`, `jamie`, `priya`, `@example.com`, the plan
  `Team profiles`. The Users list must show no other account.
- No password is typed or shown on camera, and `<password>` is never replaced in a file of the repository. The seeded
  users' passwords are chosen outside git.
- No `state.json`, `App_Data` copy, MP4 or audio file is committed. The site's name is **Orchard Core**.
- OCR the finished video at one frame per second and at every scene change, as `SKILL.md` asks, and search for any
  address that does not end in `@example.com`.

# Recording plan: Roles and Permissions

For the person who records the video. Everything to decide is decided here. In this plan a selector written in
backticks stands for `scene.Page.Locator(...)`, and "source" gives the Orchard Core file and line where the label or
the markup comes from. Paths are relative to `src/` of the Orchard Core checkout the site is built from, and the lines
are those of the source the content was written against (the commit is recorded in `catalog/SOURCE.md`).

The video is an **admin** video: the recording shows the admin and nothing else. Never open a settings file, a
console, or the browser's developer tools on camera.

## 1. The site it needs

| What | Value |
| --- | --- |
| Site | `src/OrchardCore.Cms.Web` of an Orchard Core checkout, on `http://localhost:5010`, with its own empty `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`) |
| Setup | Auto Setup, so no setup form and no sign-in form is recorded: `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` |
| Password | `<password>` is chosen outside git and passed in `VIDEO_PASSWORD` (also the recorder's sign-in). It is never written in this repository |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls no mail, SMS, or payment provider |
| Tenant | the Default tenant only |
| Features to enable | None. The Blog recipe enables Admin, Roles, Users, Contents, and Navigation (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). Before the first take, open `/Admin/Roles/Index` (the route puts the action name in the path: `OrchardCore.Modules/OrchardCore.Admin/AdminAreaControllerRouteMapper.cs:36-52`, so `/Admin/Roles` alone matches no route) and check that the list shows Administrator, Anonymous, Authenticated, Author, Contributor, Editor, and Moderator |
| Settings | Leave Settings > Admin at its defaults: List layout and Actions layout both "Use configuration default", and "Let users choose the layout of a list" cleared (source: menu `OrchardCore.Modules/OrchardCore.Admin/AdminMenu.cs:26-27`; page `OrchardCore.Modules/OrchardCore.Admin/Views/AdminSettings.Edit.cshtml:85-117`). Do not set `OrchardCore:Admin:List` anywhere. With the defaults every role and every user is a row, with its actions shown as buttons (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`) |
| External services | None |
| Browser | The recorder's own Chromium (1536x864, light, `en-US`). Sign in once with `Recorder setup` (`VIDEO_USER_NAME=admin`), which saves `state.json` (git-ignored) |

### Seed data (create it outside the capture, once, before the snapshot)

Only one record is needed besides what the Blog recipe creates:

1. Open `/Admin/Roles/Index`, click **Add Role**, enter **Name** `Guest Blogger` and **Description** `Writes posts for the blog as a guest.`, and click **Create** (not Save & Configure). The role has no permissions.

Do **not** create users, and do **not** change the roles of the `admin` user: `admin` stays in the Administrator role, and
is the only user before clip `05-assign`.

### Snapshots

After the seed data, stop the site, copy its `App_Data` folder and `state.json` aside as **snapshot A**, and start the
site again. After a successful take of `03-create`, stop the site, copy `App_Data` and `state.json` aside again as
**snapshot B** (the role Proofreader exists, with no permissions), and start the site again. To record a clip again,
restore the snapshot named in the table below, with the site stopped, and start the site again.

## 2. Order of the clips, dependencies, and what each one changes

Record in this order. The clips depend on each other as the table says.

| Order | Clip | Starts from | Changes the site | To record it again |
| --- | --- | --- | --- | --- |
| 1 | `02-roles` | Snapshot A | Nothing (it only reads, and every Cancel discards) | Snapshot A, or any state before `03-create` |
| 2 | `03-create` | Snapshot A | Creates the role Proofreader | Restore snapshot A |
| 3 | `04-permissions` | Snapshot B (the role Proofreader exists with no permissions) | Allows Access admin panel, List content items, and Edit content for others on Proofreader | Restore snapshot B |
| 4 | `05-assign` | The state after `04-permissions` | Creates the user priya.raman, who has the role Proofreader | Restore the state after `04-permissions` (take **snapshot C** then) |
| 5 | `06-delete` | The state after `05-assign` (the role Guest Blogger exists) | Deletes the role Guest Blogger | Restore snapshot C |

`02-roles` must be recorded before `03-create`: after it the list also shows Proofreader. Take snapshot C after a
successful `04-permissions`. The slide clips (`01-idea`, `07-recap`) and the cards need no site.

Before every clip: start the site, open `/Admin` once so the first request (and any compilation) is over, and check that
the Access Control group of the admin menu is closed. If it is open, click it to close it, then reload the page.

## 3. Slides and cards

| Step | What the slide shows |
| --- | --- |
| `intro` | The opening card: the logo, the title Roles and Permissions, and the subtitle "Control who can do what on your site" |
| `idea-1` | Slide "Who can do what": three cards, Permission, Role, User, with Permission bright |
| `idea-2` | The same slide with Role bright |
| `idea-3` | The same slide with User bright |
| `system-1` | Slide "The system roles, and your own": four cards, Administrator bright |
| `system-2` | The same slide with Anonymous and Authenticated bright |
| `system-3` | The same slide with "Your own roles" bright |
| `recap-1` | Slide "To sum up": four cards, all bright |
| `outro` | The closing card, with the subtitle docs.orchardcore.net |

## 4. The clips

Every step below runs inside `scene.StepAsync("<id>", ...)`. Keep the actions shorter than the narration. Navigate only
with `ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync`, or `NavigateAsync`. Clear highlights
(`ClearHighlightsAsync`) before any scroll and at the start of the next step unless the entry says to keep one.

The Roles list is the List layout. A role row is `.roles [data-filter-value="<lower-case name>"]` (for example
`.roles [data-filter-value="editor"]`), with the name link, the System badge, the description, and the buttons `a.edit`
and `a.delete` (source: `OrchardCore.Modules/OrchardCore.Roles/Drivers/RoleEntryDisplayDriver.cs:20`,
`Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12,16`). The rows are sorted by name with the culture-aware
comparer (`OrchardCore.Modules/OrchardCore.Roles/Controllers/AdminController.cs:88`), so for `en-US` they read
Administrator, Anonymous, Authenticated, Author, Contributor, Editor, Guest Blogger, Moderator, and then Proofreader once
it exists.

### Clip `02-roles`

Starts on `/Admin` (opened with `OpenAsync`, not recorded), the Access Control group closed.

- `roles-1`
  - Click the button `figcaption#accessControl button` (the label is Access Control). Wait until `ul[data-title="Access Control"]` is visible.
  - Click the link `ul[data-title="Access Control"] a:text-is("Roles")` with `ClickAndWaitForNavigationAsync`. Wait until `.roles [data-filter-value="administrator"]` is visible.
  - On screen at the end of the narration: the Roles page with the breadcrumb "Roles", the search box, the **Add Role** button, and the eight rows with their descriptions: Administrator "A system role that grants all permissions to the assigned users.", Anonymous "A system role representing all non-authenticated users.", Authenticated "A system role representing all authenticated users.", Author, Contributor, Editor (the descriptions of the Blog recipe), Guest Blogger "Writes posts for the blog as a guest.", and Moderator (Blog recipe description)
  - Source: menu `OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19`; page `Views/Admin/Index.cshtml:15`; system descriptions `OrchardCore/OrchardCore.Roles.Core/DefaultSystemRoleProvider.cs:22,32,39`; the other descriptions `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:92-107`.
- `roles-2`
  - Highlight `.roles [data-filter-value="administrator"] .badge` (the System badge), hold about 1.5 seconds, clear, then the same for `anonymous` and `authenticated`.
  - Highlight `.roles [data-filter-value="editor"] a.delete` (the Delete button), and keep it.
  - On screen at the end of the narration: a ring around the Delete button of the Editor row. The Administrator, Anonymous, and Authenticated rows have a System badge and an Edit button, and no Delete button.
  - Source: `Views/RoleEntry.DefaultTags.SummaryAdmin.cshtml:9` ("System"); `Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12,14-16` ("Edit", "Delete", shown only when the role is not a system role).
- `roles-3`
  - Clear the highlights. Click `.roles [data-filter-value="administrator"] a.edit` with `ClickAndWaitForNavigationAsync`. Wait until `.alert.alert-info` is visible.
  - Highlight `.alert.alert-info` and keep it.
  - On screen at the end of the narration: the page "Edit 'Administrator' Role" with the Description box and a blue notice "This system role provides comprehensive access by granting all available permissions." and no permission table.
  - Source: `Views/Admin/Edit.cshtml:6,17`; `Views/Admin/_RolePermissions.cshtml:30-33`.
- `roles-4`
  - Clear the highlights. Click `a.cancel` ("Cancel") with `ClickAndWaitForNavigationAsync`, then click `.roles [data-filter-value="anonymous"] a.edit` with `ClickAndWaitForNavigationAsync`. Wait until `fieldset.permissions-list` is visible.
  - On screen at the end of the narration: the page "Edit 'Anonymous' Role" with the Description box, the Allow and Effective explanation box, the Search permissions box, and the permission groups with their Allow and Effective columns. Do not tick or untick anything.
  - Source: `Views/Admin/Edit.cshtml:41`; `Views/Admin/_RolePermissions.cshtml:38-72`.
- `roles-5`
  - Click `a.cancel` ("Cancel") with `ClickAndWaitForNavigationAsync`. Wait until the Roles list is visible. Highlight `a.btn.create` (the **Add Role** button) and keep it.
  - On screen at the end of the narration: the Roles list with a ring around **Add Role**.
  - Source: `Views/Admin/Index.cshtml:28-33` (the button is shown only with Manage Roles); `OrchardCore/OrchardCore.Roles.Core/RolesPermissions.cs:9` ("View Roles").
- `roles-6`
  - Clear the highlights. Highlight `.roles [data-filter-value="editor"] .admin-list-actions` (the Edit and Delete buttons of the Editor row) and keep it.
  - On screen at the end of the narration: a ring around the Edit and Delete buttons of the Editor row.
  - Source: `OrchardCore/OrchardCore.Roles.Core/RolesPermissions.cs:8` ("Manage Roles"); `Views/RoleEntry.Buttons.SummaryAdmin.cshtml:10`.

### Clip `03-create`

Starts on `/Admin/Roles/Index` (opened with `OpenAsync`), no highlight. Restore snapshot A first.

- `create-1`
  - Click `a.btn.create` ("Add Role") with `ClickAndWaitForNavigationAsync`. Wait until `#RoleName` is visible.
  - Highlight `#RoleName`, hold about 1.5 seconds, clear, then highlight `#RoleDescription`, and keep it.
  - On screen at the end of the narration: the page "Create Role" with the empty fields Name and Description, a ring around Description, and the buttons Create, Save & Configure, and Cancel at the bottom.
  - Source: `Views/Admin/Create.cshtml:6,15,23,31-33`.
- `create-2`
  - Clear the highlights. `TypeAsync` `#RoleName`: `Proofreader`. `TypeAsync` `#RoleDescription`: `Corrects posts written by other people.`
  - On screen at the end of the narration: Name "Proofreader" and Description "Corrects posts written by other people.", and no validation message.
  - Source: `Views/Admin/Create.cshtml:17,25`; the name rules `Controllers/AdminController.cs:395-407`.
- `create-3`
  - Highlight `button.create` ("Create"), hold about 2 seconds, clear. Highlight `button.save-configure` ("Save & Configure"), hold about 2 seconds, clear.
  - Click `button.save-configure` with `ClickAndWaitForNavigationAsync`. Wait until `fieldset.permissions-list` is visible.
  - On screen at the end of the narration: the page "Edit 'Proofreader' Role" with the Description "Corrects posts written by other people." and the permission groups, every Allow box cleared. The message "Role created successfully." may still be visible at the top.
  - Source: `Views/Admin/Create.cshtml:31,32`; `Controllers/AdminController.cs:146,148-151`.

### Clip `04-permissions`

Starts on `/Admin/Roles/Edit/Proofreader` (opened with `OpenAsync`, not recorded), the page as `03-create` leaves it. Restore snapshot B first. Between steps, empty the search box with `PressAsync("Escape")` on `#search-box`, which also shows every row again (source: `Assets/ts/roles-edit.ts:11-20`, under `OrchardCore.Modules/OrchardCore.Roles`).

Before the first take only: with the cursor over the Effective box of View all content, check that its tooltip says "This permission is granted by role: Anonymous, Authenticated". If the Effective box of View all content is cleared on the fresh role, stop and report it (see `OPEN-QUESTIONS.md`), because `perms-2` describes it.

- `perms-1`
  - Highlight the legend of the first group, `fieldset.permissions-list >> nth=0 >> legend`, hold about 2 seconds, clear. Highlight its header row, `fieldset.permissions-list >> nth=0 >> thead`, and keep it.
  - On screen at the end of the narration: the top of the page "Edit 'Proofreader' Role": the Description box, the Allow and Effective box, the Search permissions box, and the first group with the ring around its header row (Permission, Allow, Effective).
  - Source: `Views/Admin/_RolePermissions.cshtml:57-72`; the group title and the feature identifier beside it are the legend, lines 57-63.
- `perms-2`
  - Clear the highlights. Highlight `div.bg-info-subtle` (the box that explains Allow and Effective), hold about 3 seconds, clear.
  - `TypeAsync` `#search-box`: `view all content`. Only the row "View all content" remains. Move the cursor onto `tr[data-text="View all content"] div.form-check[data-bs-toggle="tooltip"]` (the Effective box) with `MoveToAsync`, and wait up to 2 seconds for the tooltip; if it does not appear, carry on.
  - On screen at the end of the narration: the row "View all content" with Allow cleared and Effective ticked, and, when it appeared, the tooltip "This permission is granted by role: Anonymous, Authenticated".
  - Source: `Views/Admin/_RolePermissions.cshtml:40,43,49,103-113,136-138`; `OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:29` ("View all content"); the grants of Anonymous and Authenticated `OrchardCore.Modules/OrchardCore.Contents/Permissions.cs:86-95`.
- `perms-3`
  - `PressAsync("Escape")` on `#search-box`. `TypeAsync` `#search-box`: `access admin`. Only the row "Access admin panel" remains. Click `[id="Checkbox.AccessAdminPanel"]` (the Allow box).
  - On screen at the end of the narration: the row "Access admin panel" with Allow ticked and Effective cleared.
  - Source: `OrchardCore/OrchardCore.Admin.Abstractions/AdminPermissions.cs:8` ("Access admin panel"); the check `OrchardCore.Modules/OrchardCore.Admin/AdminFilter.cs:58`.
- `perms-4`
  - `PressAsync("Escape")` on `#search-box`. `TypeAsync` `#search-box`: `list content`. Only the row "List content items" remains. Click `[id="Checkbox.ListContent"]` (the Allow box).
  - `PressAsync("Escape")` on `#search-box`. `TypeAsync` `#search-box`: `edit content for others`. Only the row "Edit content for others" remains. Click `[id="Checkbox.EditContent"]` (the Allow box).
  - On screen at the end of the narration: the row "Edit content for others" with Allow ticked (the row "List content items" was ticked just before and is out of sight).
  - Source: `OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:21` ("Edit content for others"), `:41` ("List content items"). Why the role needs List content items: the Content Items list demands it and Edit content for others does not imply it (`OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:82`; the menu item "Content Items" under "Content", `OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65-76`).
- `perms-5`
  - `PressAsync("Escape")` on `#search-box`. `TypeAsync` `#search-box`: `manage roles`. Only the row "Manage Roles" remains. Highlight `tr[data-text="Manage Roles"] .badge` (the Security Critical badge) and keep it. Move the cursor onto the badge with `MoveToAsync`, and wait up to 2 seconds for its tooltip; if it does not appear, carry on. Do **not** tick the Allow box.
  - On screen at the end of the narration: the row "Manage Roles" with the badge "Security Critical" ringed, Allow cleared, and, when it appeared, the tooltip "This permission could allow a user to elevate their other permissions. Grant it with extreme consideration."
  - Source: `Views/Admin/_RolePermissions.cshtml:122-127`; `OrchardCore/OrchardCore.Roles.Core/RolesPermissions.cs:8`.
- `perms-6`
  - Clear the highlights. `PressAsync("Escape")` on `#search-box`. Click `button.save` ("Save") with `ClickAndWaitForNavigationAsync`. Wait until `.roles [data-filter-value="proofreader"]` is visible.
  - On screen at the end of the narration: the Roles list with the row Proofreader (description "Corrects posts written by other people.") and the message "Role updated successfully."
  - Source: `Views/Admin/Edit.cshtml:33`; `Controllers/AdminController.cs:213,220`.
- `perms-7`
  - Click `.roles [data-filter-value="proofreader"] a.edit` with `ClickAndWaitForNavigationAsync`. Wait until `#search-box` is visible. `TypeAsync` `#search-box`: `view all content`. Move the cursor onto `tr[data-text="View all content"] div.form-check[data-bs-toggle="tooltip"]` with `MoveToAsync` and wait until the tooltip is visible. This step needs the tooltip: a capture without it cannot be used (see `OPEN-QUESTIONS.md`).
  - On screen at the end of the narration: the row "View all content" with Allow cleared, Effective ticked, and the tooltip "This permission is implied by: Edit content for others".
  - Source: `Views/Admin/_RolePermissions.cshtml:91-98`; `OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:29`.

After a successful take, take snapshot C (see section 2).

### Clip `05-assign`

Starts on `/Admin` (opened with `OpenAsync`), the Access Control group closed. Start from the state after `04-permissions`.

- `assign-1`
  - Click `figcaption#accessControl button` (Access Control). Click `ul[data-title="Access Control"] a:text-is("Users")` with `ClickAndWaitForNavigationAsync`. Click `a.btn.create` ("Add User") with `ClickAndWaitForNavigationAsync`. Wait until the field "User name" is visible.
  - On screen at the end of the narration: the page "Create User" with empty fields.
  - Source: menu `OrchardCore.Modules/OrchardCore.Users/AdminMenu.cs:27,30`; button `Views/UsersAdminListCreate.cshtml:7`; page `Views/Admin/Create.cshtml:4`.
- `assign-2`
  - `TypeAsync` the field `scene.Page.GetByLabel("User name", new() { Exact = true })`: `priya.raman`. `TypeAsync` the field `scene.Page.GetByLabel("Email", new() { Exact = true })`: `priya.raman@example.com`. Click `.password-generator-button` ("Generate password").
  - Never click the eye icons next to the password fields and never click "Copy password": the generated password must not be shown.
  - On screen at the end of the narration: User name "priya.raman", Email "priya.raman@example.com", and the two password fields filled with dots.
  - Source: `Views/UserName.Edit.cshtml:4`; `Views/UserEmail.Edit.cshtml:4`; `Views/UserFields.Edit.cshtml:12,39`; the generator `Assets/ts/user-fields.ts:32-43` (under `OrchardCore.Modules/OrchardCore.Users`).
- `assign-3`
  - `ScrollAsync(0, 400)` so the Roles field is fully in view. Highlight the field `scene.Page.Locator("label.ocat-label", new() { HasTextString = "Roles" })` and keep it for about 2 seconds, clear. Click the label `scene.Page.Locator("label.form-check-label", new() { HasTextString = "Proofreader" })`.
  - On screen at the end of the narration: the Roles field with the checkboxes Administrator, Author, Contributor, Editor, Guest Blogger, Moderator, and Proofreader, in this order, with Proofreader ticked. There is no Anonymous and no Authenticated.
  - Source: `OrchardCore.Modules/OrchardCore.Users/Views/UserRoleFields.Edit.cshtml:6`; the roles offered `OrchardCore/OrchardCore.Roles.Abstractions/RoleServiceExtensions.cs:16-32`.
- `assign-4`
  - Click `button.save` ("Save") with `ClickAndWaitForNavigationAsync`. Wait until the row of priya.raman is visible in the Users list. Highlight `.badge:has-text("Proofreader")` and keep it.
  - On screen at the end of the narration: the Users list with the message "User created successfully.", the row of admin with the badge Administrator, and the row of priya.raman with the badge Proofreader ringed. An "Unconfirmed" badge beside the name may be shown: leave it.
  - Source: `OrchardCore.Modules/OrchardCore.Users/Views/UserSaveButtons.Edit.cshtml:2`; `Controllers/AdminController.cs:320`; the badge `Views/UserRolesMeta.cshtml:18`.

### Clip `06-delete`

Starts on `/Admin/Roles/Index` (opened with `OpenAsync`), no highlight. Start from the state after `05-assign`.

- `delete-1`
  - Highlight `.roles [data-filter-value="guest blogger"] a.delete` for about 1.5 seconds, clear. Click it. Wait until `#confirmRemoveModal` is visible.
  - On screen at the end of the narration: the confirmation dialog with the title "Delete role", the text "Are you sure you want to delete this role?", and the buttons Ok and Cancel.
  - Source: `Views/RoleEntry.Buttons.SummaryAdmin.cshtml:16`; the dialog `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99` (the buttons Ok and Cancel) and `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:16-31`.
- `delete-2`
  - Click `#modalOkButton` ("Ok") with `ClickAndWaitForNavigationAsync`. Wait until `.roles [data-filter-value="guest blogger"]` is no longer present.
  - On screen at the end of the narration: the Roles list without Guest Blogger and with the message "Role deleted successfully."
  - Source: `Controllers/AdminController.cs:270`; the users lose the role `OrchardCore.Modules/OrchardCore.Users/Roles/Handlers/UserRoleRemovedEventHandler.cs:15-26`, registered in `OrchardCore.Modules/OrchardCore.Users/Startup.cs:376`.

## 5. Checks for private data

Before assembling, look at every contact sheet, then OCR the video as the skill says.

- The only people on screen are **admin** (`admin@example.com`) and **priya.raman** (`priya.raman@example.com`). No other name, address, or number may appear.
- No password is visible anywhere: not typed, not in a field (only dots), not in a tooltip. The eye icons and "Copy password" stay untouched.
- The browser's address bar and sign-in are not part of the capture; `state.json` stays out of git, as does any copy of `App_Data`.
- The site name is "Orchard Core"; no company, domain, or real email other than `example.com` appears.
- Delete the snapshots and the site's `App_Data` when the video is published.

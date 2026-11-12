# Recording plan: Users: Create, Edit, Disable and Delete Accounts

This plan is for the person who records the video. It lists the site, the seed data, the order of the clips and, for
every step of a recording, what to do and what must be on screen when its narration ends. Nothing here is left to
decide. A selector in this plan stands for `scene.Page.Locator(...)`; check each one once with `Recorder probe` before
recording (see OPEN-QUESTIONS.md).

Source references are relative to the Orchard Core source at commit 92db4a83. These abbreviations are used:

- `U` = `src/OrchardCore.Modules/OrchardCore.Users`
- `A` = `src/OrchardCore.Modules/OrchardCore.Admin`
- `TA` = `src/OrchardCore.Themes/TheAdmin`

`<password>` is the password chosen outside git (the value of `VIDEO_PASSWORD`). Never write a real or sample password
anywhere in this repository, and never show one on screen.

## 1. The site it needs

| What | Value |
| --- | --- |
| Orchard Core | `src/OrchardCore.Cms.Web` of the checkout, with its own `App_Data` outside both repositories, on `http://localhost:5010` |
| Provisioning | Auto Setup (see `references/recording.md`, "The site to record"): tenant Default, recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The video needs no external service and no stand-in |
| Sign-in | `Recorder setup` once, outside the capture, saves `state.json` (never committed) |
| Browser | The recorder's defaults (1536 x 864 CSS pixels, scale 1.25, light color scheme, en-US) |

**Features.** The Blog recipe enables `OrchardCore.Users` and `OrchardCore.Roles` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the
`feature` step) and none of the other Users features, nor `OrchardCore.Email`. Enable nothing else. In particular
**leave these off**, because each one adds content to the Edit User page or to the Actions menu that the narration does
not mention: Users Registration, Users Reset Password, Users Change Email, the Two-Factor features, Users Audit Trail, User
Time Zone, User Localization, Custom User Settings, Workflows.

**Settings to check before recording (all at their defaults).**

- Settings > Security > User Login: **Allow user to change their username** and **Allow user to change their email
  address** are both unticked. Step `edit-1` shows User name and Email read-only because of this.
- Settings > Admin: the list layout and the actions layout are on **Use configuration default**, so the Users list is
  a List with Edit and View buttons and an **Actions** dropdown on each row (`A/Views/AdminListActions.cshtml`).
- Settings > General: the page size is 10 or more, so all rows are on one page.

## 2. Seed data (outside the capture)

All values are invented. All emails are `@example.com`.

**Step 1: create six users.** Sign in to the site, open Tools > Deployments > JSON Import (`TheAdmin` menu; label from
`src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30`), paste the recipe below with `<password>` replaced by
the password (no spaces, no double quotes, and it must satisfy the site's password rules), and select **Import**. The page answers
"Recipe imported successfully!". The `createUser` command (`U/Commands/UserCommands.cs:34`) marks each email as
confirmed and assigns the roles.

```json
{
  "steps": [
    {
      "name": "command",
      "Commands": [
        "createUser /UserName:alex.chen /Password:<password> /Email:alex.chen@example.com /Roles:Editor",
        "createUser /UserName:sam.okafor /Password:<password> /Email:sam.okafor@example.com /Roles:Author",
        "createUser /UserName:taylor.brooks /Password:<password> /Email:taylor.brooks@example.com /Roles:Contributor",
        "createUser /UserName:riley.nguyen /Password:<password> /Email:riley.nguyen@example.com /Roles:Moderator",
        "createUser /UserName:casey.lopez /Password:<password> /Email:casey.lopez@example.com /Roles:Contributor",
        "createUser /UserName:drew.patel /Password:<password> /Email:drew.patel@example.com /Roles:Author"
      ]
    }
  ]
}
```

**Step 2: disable one user.** Open Access Control > Users. On the row of `drew.patel`, open **Actions**, select
**Disable**, and select **Ok** in the dialog. The row now has a **Disabled** badge.

**Step 3: check the list.** Open `/Admin/Users`. The list must show exactly seven rows, in this order (the list sorts by
Name):

| User name | Email | Roles | Badges |
| --- | --- | --- | --- |
| `admin` | `admin@example.com` | Administrator | none |
| `alex.chen` | `alex.chen@example.com` | Editor | none |
| `casey.lopez` | `casey.lopez@example.com` | Contributor | none |
| `drew.patel` | `drew.patel@example.com` | Author | Disabled |
| `riley.nguyen` | `riley.nguyen@example.com` | Moderator | none |
| `sam.okafor` | `sam.okafor@example.com` | Author | none |
| `taylor.brooks` | `taylor.brooks@example.com` | Contributor | none |

No row may show **Unconfirmed**. If one does, use its **Actions** > **Confirm email**.

`morgan.ellis` does not exist yet: clip `03-create` creates it on camera. The user name `sam` appears in exactly one user
name (`sam.okafor`), which `find-1` relies on.

## 3. Order of the clips, and what changes the site

Record in the order the video plays. Every recording clip after `02-list` depends on the clips before it.

| Order | Clip | Changes the site? | Depends on |
| --- | --- | --- | --- |
| 1 | `02-list` | No | The seed data |
| 2 | `03-create` | Yes: creates `morgan.ellis` (role Author) and confirms the email | The seed data |
| 3 | `04-edit` | Yes: adds a phone number and the role Contributor to `morgan.ellis` | `03-create` |
| 4 | `05-actions` | Yes: changes the password of `morgan.ellis`, disables and enables the account, deletes `casey.lopez` | `03-create`, the seed data |
| 5 | `06-find` | Yes: disables `taylor.brooks` and `riley.nguyen` | The seed data, `03-create`, `04-edit`, `05-actions` (`find-3` expects `morgan.ellis` enabled and `casey.lopez` gone) |

Slides (`01-how`) are drawn from `slides.py` and need no site.

**Snapshots.** Stop the site and copy `App_Data` and `state.json` aside at these points:

- **S0**: after the seed data (section 2) and before `02-list`.
- **S3**: after `03-create`.
- **S4**: after `04-edit`.
- **S5**: after `05-actions`.

To record a clip again, restore the snapshot before it: `03-create` from S0, `04-edit` from S3, `05-actions` from S4,
`06-find` from S5. A full take restores S0 and records the five clips in order. `02-list` can be recorded from any
snapshot; record it from S0, where `casey.lopez` is still listed.

Narrate first (`narrate.py`), then record: each step is held for the length of its narration, and the actions of a
step must be shorter than that. Estimated narration lengths are given per step (at about 2.5 words a second).

## 4. Slides

`01-how` is made of slides drawn by `slides.py`; there is nothing to record.

- `how-1`: heading "What a user is", three cards: An account, Roles, Permissions.
- `how-2`: heading "Where accounts come from", three cards: An administrator, Visitors, External Authentication.
- `how-3`: heading "Permissions for managing users", four cards named after the permissions: List all users, Edit any
  user, Delete any user, Assign any role to users. The names come from `src/OrchardCore/OrchardCore.Users.Core/UsersPermissions.cs:14-31`.

## 5. The clips

In every clip, `OpenAsync` the start page first (it is not recorded), then `StartCaptureAsync`.

**Rows.** `ROW(name)` stands for the locator `li.list-group-item:has-text("<name>@example.com")`, the row of the user
whose user name is `<name>` (every email of this plan is the user name followed by `@example.com`). Find a row by its
email, not by its user name, for two reasons. First, the header line of the list ("7 items / 7 items in total", the three
pickers) is also an `li.list-group-item`, and the Role picker in it holds the text "Administrator", so
`has-text("admin")` matches the header line as well as the row of `admin`, and Playwright refuses a locator that matches
two elements. Second, no header line holds an email. Inside a row, **Edit** and **View** are `ROW a.btn:text-is("Edit")`
and `ROW a.btn:text-is("View")`, the dropdown button is `ROW button.actions` (its text is "Actions"), and the items of
that dropdown are `ROW .actions-menu a.dropdown-item:text-is("<label>")`. Every row holds its own copy of these items, so
a locator that does not start with `ROW(...)` matches the item of every row and fails.

**Dialogs.** A confirmation dialog is `#confirmRemoveModal`; its buttons are `#modalOkButton` (text "Ok") and
`#modalCancelButton` (text "Cancel"). Use `ClickAndWaitForNavigationAsync` or `WaitForNavigationAsync` for every click
that loads a page, including the **Ok** of a dialog. Keep pauses short.

### Clip `02-list`

Start page: `/Admin` (the dashboard), with the admin menu collapsed, so that "Access Control" is closed.

- `list-1` (about 11 s). Actions: click `#left-nav button.nav-group-label:has(span.title:text-is("Access Control"))` to open the section,
  pause 700 ms, then `ClickAndWaitForNavigationAsync` on `#left-nav a.item-label:has(span.title:text-is("Users"))`. A bare
  `a:text-is("Users")` finds nothing: the text sits in the `span.title` inside the link, and `:text-is` matches only the
  smallest element holding the text (`TA/Views/NavigationItemLink-admin.cshtml`, `U/Views/NavigationItemText-users.Id.cshtml:4`). On screen at the end: the **Users** page
  (breadcrumb "Users", the **Add User** button, the Search box, and the seven rows of the table above, with the
  "7 items / 7 items in total" count). Labels: `U/AdminMenu.cs:27` ("Access Control"), `U/AdminMenu.cs:30` ("Users"),
  `U/Views/Admin/Index.cshtml:4` (breadcrumb), `U/Views/UsersAdminListSummary.cshtml:7` (the count).
- `list-2` (about 11 s). Actions: move the cursor onto `Edit` of `ROW(alex.chen)`, then onto its `View`, then onto its
  `Actions` button; then `HighlightAsync` the **Disabled** badge of `ROW(drew.patel)` (`ROW(drew.patel) .badge:has-text("Disabled")`)
  with no caption; `ClearHighlightsAsync`; finally move the cursor onto `ROW(admin)`, between its **Edit** button and
  its **Actions** button, where the other rows have **View**. On screen at the end: the list, with the `admin` row showing
  Edit and Actions only. Labels: `U/Views/UserButtons.cshtml:11` ("Edit"), `U/Views/UserButtons.cshtml:16` ("View", not
  shown for the current user, `UserButtons.cshtml:14`), `A/Views/AdminListActions.cshtml:32` ("Actions"),
  `U/Views/UserFields.cshtml:22-23` ("Disabled").

### Clip `03-create`

Start page: `/Admin/Users`. Changes the site.

- `create-1` (about 12 s). Actions: `ClickAndWaitForNavigationAsync` on `a.btn.create` (text "Add User"); `TypeAsync`
  `#User_UserName` with `morgan.ellis`; `TypeAsync` `#User_Email` with `morgan.ellis@example.com`; then
  `HighlightAsync` `#User_PhoneNumber` with the caption "Optional". Leave the Phone Number empty. On screen at the end:
  the **Create User** page with User name and Email filled and the Phone Number field highlighted. Labels:
  `U/Views/UsersAdminListCreate.cshtml:7` ("Add User"), `U/Views/Admin/Create.cshtml:4` ("Create User"),
  `U/Views/UserName.Edit.cshtml:4` ("User name"), `U/Views/UserEmail.Edit.cshtml:4` ("Email"),
  `U/Views/UserPhoneNumber.Edit.cshtml:4` ("Phone Number"). Both User name and Email are required: the label
  has the required marker (`ocat-label-required`) when `AllowEditing` is true, which it is on a new user
  (`U/Drivers/UserInformationDisplayDriver.cs:51,59`).
- `create-2` (about 15 s). Actions: `ClearHighlightsAsync`; click `.password-generator-button` (text "Generate password");
  then move the cursor onto `.copy-password-button` (text "Copy password") and stop there; do not click it. Do not click
  the eye icons (`.password-toggle-button`, `.password-confirmation-toggle-button`): the generated password must never be
  visible. On screen at the end: the Password and Password confirmation fields both filled with masked dots, and the
  **Generate password** and **Copy password** buttons. Labels: `U/Views/UserFields.Edit.cshtml:12` ("Password"),
  `:16` (placeholder "Password"), `:27` (placeholder "Password confirmation"), `:39` ("Generate password"), `:41`
  ("Copy password"). The script `U/Assets/ts/user-fields.ts` fills both fields from the site's password rules.
- `create-3` (about 11 s). Actions: scroll down if needed so the **Roles** section is visible; click the label
  `label:text-is("Author")` of the Roles section to tick it; `ClickAndWaitForNavigationAsync` on `button.save` (text
  "Save"). On screen at the end: the Users list with the notification "User created successfully." above it and a
  `morgan.ellis` row with the role badge **Author** and the **Unconfirmed** badge. Labels:
  `U/Views/UserRoleFields.Edit.cshtml:6` ("Roles"), `U/Views/UserSaveButtons.Edit.cshtml:2` ("Save"),
  `U/Controllers/AdminController.cs:320` (the notification).
- `create-4` (about 14 s). Actions: `HighlightAsync` the badge `ROW(morgan.ellis) .badge:has-text("Unconfirmed")`
  with the caption "Unconfirmed"; hold. On screen at the end: a green ring around the **Unconfirmed** badge of the
  `morgan.ellis` row. Label: `U/Views/UserFields.cshtml:10-11`. The account is unconfirmed because the admin form never
  sets `EmailConfirmed` (`U/Controllers/AdminController.cs:299`, `new User()`).
- `create-5` (about 12 s). Actions: `ClearHighlightsAsync`; click `ROW(morgan.ellis) button.actions`; click
  `ROW(morgan.ellis) .actions-menu a.dropdown-item:text-is("Confirm email")`; let the dialog show for 1.5 s (title "Confirm email", message
  "Are you sure you want to confirm the email for this user?"); then `WaitForNavigationAsync` around a click on
  `#modalOkButton`. On screen at the end: the Users list with the notification "The email for morgan.ellis has been
  successfully confirmed." and no **Unconfirmed** badge on the `morgan.ellis` row. Labels:
  `U/Views/UserActionsMenu.cshtml:35` ("Confirm email" and the dialog texts), `TA/Views/Layout.cshtml:99` ("Ok"),
  `U/Controllers/AdminController.cs:453` (the notification). Confirm email only sets the confirmed flag
  (`AdminController.cs:652-662`); it sends no message.

### Clip `04-edit`

Start page: `/Admin/Users`, in the state after `03-create` (snapshot S3). Changes the site.

- `edit-1` (about 14 s). Actions: `ClickAndWaitForNavigationAsync` on `ROW(morgan.ellis) a.btn:text-is("Edit")`;
  `HighlightAsync` `#User_UserName` with the caption "Read-only", then `#User_Email` with the caption "Read-only", then
  (after `ClearHighlightsAsync`) `#User_PhoneNumber` with the caption "Editable". On screen at the end: the **Edit User**
  page, with the User name and Email fields greyed out and the Phone Number field highlighted; the Roles section shows
  **Author** ticked. Labels: `U/Views/Admin/Edit.cshtml:4` ("Edit User"). The fields are disabled when the matching
  Login setting is off (`U/Drivers/UserInformationDisplayDriver.cs:51,59`); the phone number is editable by default
  (`U/Drivers/UserInformationDisplayDriver.cs:68`, `src/OrchardCore/OrchardCore.Users.Core/Models/LoginSettings.cs`,
  `AllowChangingPhoneNumber` defaults to true). The setting lives under Settings > Security > User Login
  (`U/AdminMenu.cs:40-42`).
- `edit-2` (about 11 s). Actions: `ClearHighlightsAsync`; `TypeAsync` `#User_PhoneNumber` with `+12065550142` (a fictional
  555-01xx number); click the label `label:text-is("Contributor")` to tick it (leave **Author** ticked);
  `ClickAndWaitForNavigationAsync` on `button.save`. On screen at the end: the Users list with the notification "User
  updated successfully." and the `morgan.ellis` row showing the role badges **Author** and **Contributor**. Labels:
  `U/Views/UserPhoneNumber.Edit.cshtml:20` (the hint about the country code), `U/Controllers/AdminController.cs:406` (the
  notification). If the phone number is refused ("Please provide a valid phone number."), see OPEN-QUESTIONS.md.
- `edit-3` (about 11 s). Actions: `ClickAndWaitForNavigationAsync` on `ROW(morgan.ellis) a.btn:text-is("View")`. On screen at the end: the **View User**
  page with **User Info** (Username `morgan.ellis`, Email `morgan.ellis@example.com`, Phone Number `+12065550142`,
  Status **Enabled**) and **Roles** (Author, Contributor). Labels: `U/Views/User.DetailAdmin.cshtml:5` ("View User"),
  `U/Views/UserInfo.cshtml:3,6,9,14,18-19`, `U/Views/UserRoles.cshtml:10`.

### Clip `05-actions`

Start page: `/Admin/Users`, in the state after `04-edit` (snapshot S4). Changes the site.

- `actions-1` (about 12 s). Actions: click `ROW(morgan.ellis) button.actions` and leave the menu open. On screen at
  the end: the open menu with **Change password**, **Disable** and **Delete** (no **Confirm email**, because the email is
  confirmed). Labels: `U/Views/UserActionsMenu.cshtml:16` ("Change password"), `:20` ("Disable"), `:40` ("Delete").
- `actions-2` (about 14 s). Actions: `ClickAndWaitForNavigationAsync` on `ROW(morgan.ellis) .actions-menu a.dropdown-item:text-is("Change password")`;
  `TypeAsync` `#NewPassword` with `<password>`; `TypeAsync` `#PasswordConfirmation` with `<password>` (both fields
  stay masked); `ClickAndWaitForNavigationAsync` on `button.submit` (text "Submit"); on the list that loads (notification
  "The password has been changed successfully."), click `ROW(admin) button.actions`, then
  `ClickAndWaitForNavigationAsync` on `ROW(admin) .actions-menu a.dropdown-item:text-is("Change password")`. On screen at the end: the **Change Password** page
  of the `admin` account, with the **Current password** field above New password and New password confirmation. Labels:
  `U/Views/Admin/EditPassword.cshtml:4` ("Change Password"), `:15` ("Current password"), `:24` ("New password"), `:32`
  ("New password confirmation"), `:39` ("Submit"), `U/Controllers/AdminController.cs:549` (the notification). The
  Current password field is shown only for the signed-in user (`EditPassword.cshtml:12`,
  `AdminController.cs:535-543`). No password is displayed at any moment.
- `actions-3` (about 13 s). Actions: click `a.cancel` (text "Cancel") with `ClickAndWaitForNavigationAsync` to return to the list;
  click `ROW(morgan.ellis) button.actions`; click `ROW(morgan.ellis) .actions-menu a.dropdown-item:text-is("Disable")`; let the dialog
  show for 1.5 s (title "Disable user", message "Are you sure you want to disable this user?"); `WaitForNavigationAsync`
  around a click on `#modalOkButton`. On screen at the end: the list with the notification "User account was
  successfully disabled." and a **Disabled** badge on the `morgan.ellis` row (role badges Author and Contributor still
  shown). Labels: `U/Views/UserActionsMenu.cshtml:20`, `U/Controllers/AdminController.cs:605`. A disabled user cannot
  sign in: `src/OrchardCore/OrchardCore.Users.Core/Services/UserService.cs:115-120`; the only enabled administrator
  cannot be disabled: `UserService.cs:419-428`.
- `actions-4` (about 8 s). Actions: click `ROW(morgan.ellis) button.actions`; click `ROW(morgan.ellis) .actions-menu a.dropdown-item:text-is("Enable")`;
  dialog for 1.5 s (title "Enable user", message "Are you sure you want to enable this user?"); `WaitForNavigationAsync`
  around a click on `#modalOkButton`. On screen at the end: the list with the notification "User account was successfully
  enabled." and no **Disabled** badge on the `morgan.ellis` row. Labels: `U/Views/UserActionsMenu.cshtml:24`,
  `U/Controllers/AdminController.cs:580`.
- `actions-5` (about 12 s). Actions: click `ROW(casey.lopez) button.actions`; click
  `ROW(casey.lopez) .actions-menu a.dropdown-item:text-is("Delete")`; dialog for 1.5 s (title "Delete", message "Are you sure you want to remove
  this element?"); `WaitForNavigationAsync` around a click on `#modalOkButton`; then click `ROW(admin) button.actions`
  and leave the menu open. On screen at the end: the list without `casey.lopez` (six rows), the notification
  "User deleted successfully.", and the open menu of the `admin` row with **Change password** and **Disable** but no
  **Delete**. Labels: `U/Views/UserActionsMenu.cshtml:40` ("Delete"), `TA/Views/Layout.cshtml:99` (the dialog's default
  title, message and "Ok"), `U/Controllers/AdminController.cs:480`. Delete is hidden on the current user's row:
  `UserActionsMenu.cshtml:38`.

### Clip `06-find`

Start page: `/Admin/Users`, in the state after `05-actions` (snapshot S5). Changes the site.

- `find-1` (about 11 s). Actions: `TypeAsync` `.action-bar input[type="search"]` (placeholder "Search") with `sam`; then
  `ClickAndWaitForNavigationAsync` on `button[name="submit.Filter"][title="Search"]`. On screen at the end: the list with
  only the `sam.okafor` row, "1 item / 1 item in total" in the list header, and `sam` in the Search box. Labels:
  `U/Views/UsersAdminListSearch.cshtml:30` (placeholder "Search"), `:31` (the Search button). The default term matches
  the user name only (`U/Services/DefaultUsersAdminListFilterProvider.cs`, `WithDefaultTerm`). Scope the locator to
  `.action-bar` (the card that holds the Search box, `U/Views/UsersAdminList.cshtml:20`): a bare `input[type="search"]`
  matches two elements, because the Role picker is a live search list and bootstrap-select builds a second search input
  inside its menu (`src/OrchardCore.Modules/OrchardCore.Resources/wwwroot/Vendor/CrestApps/bootstrap-select/bootstrap-select.js:1037-1039`,
  `UsersAdminListFilters.cshtml:5`, `data-live-search="true"`).
- `find-2` (about 14 s). Actions: click `#filter-dropdown` (the funnel button) and hold the open menu for 3 s (items "Only
  enabled users", "Only disabled users", "Filter syntax"); click `.dropdown-menu a.dropdown-item:text-is("Filter syntax")`; hold
  while the dialog is open. On screen at the end: the **Available Filters** dialog with its cards **Name**, **Email**,
  **Status**, **Role** and **Sort**. Do not scroll the dialog and do not hover its lower cards: the narration covers
  the five cards only, and the lower cards show filter syntax examples. Labels: `U/Views/UsersAdminListSearch.cshtml:22-24` (menu items), `:40` (dialog
  title); the cards come from `U/Views/Items/UsersAdminFilters-{Name,Email,Status,Role,Sort}.Thumbnail.cshtml` (titles on
  line 7 of each) and are registered in `U/Drivers/UserOptionsDisplayDriver.cs:19-23`.
- `find-3` (about 13 s). Actions: click `.modal-footer button:text-is("Close")` to close the dialog; `NavigateAsync("/Admin/Users")` to clear
  the search; open the Role picker (`.filter button.dropdown-toggle` with `.Nth(0)`) and click the option
  `.filter a.dropdown-item:has(span.text:text-is("Author"))` (`WaitForNavigationAsync`: the list reloads); open the Show
  picker (`.Nth(1)`) and click `.filter a.dropdown-item:has(span.text:text-is("Disabled Users"))`
  (`WaitForNavigationAsync`); open the Sort picker (`.Nth(2)`), so its options "Name" and "Email" show, and leave it
  open. The three buttons are in the order Role, Show, Sort, and each button shows its title ("Role", "Show", "Sort")
  whichever option is chosen (`data-selected-text-format="static"`). The option links of every picker are in the page
  before a picker opens, but "Author" and "Disabled Users" each occur in one picker only, and nowhere else inside
  `.filter` (the role badges of the rows are outside it), so each locator matches one element; exact text keeps
  "Authenticated (no roles)" out of the first one. Each option is `li > a.dropdown-item > span.text` in
  `ul.dropdown-menu.inner`, built by bootstrap-select (`bootstrap-select.js:511-516,2282-2283`). On screen at the end: the list with only the `drew.patel` row (Author, Disabled) and the
  open Sort picker. The pickers are bootstrap-select menus, not native lists, so `SelectAsync` does not apply. A picker
  change submits the filter form (the `bulk-select-list` code compiled into `U/wwwroot/Scripts/users-admin-list/users-admin-list.js`: `changed.bs.select` clicks `submit.Filter`). Labels:
  `U/Views/UsersAdminListFilters.cshtml:5-7` (picker titles "Role", "Show", "Sort"; headers "Filter by role", "Filter by
  status", "Sort by"), `U/Controllers/AdminController.cs:125-127` ("All Users", "Enabled Users", "Disabled Users"),
  `:132-133` ("Name", "Email"), `:160` ("Any role").
- `find-4` (about 14 s). Actions: `NavigateAsync("/Admin/Users")`; click the checkbox of `ROW(taylor.brooks)`
  (`ROW(taylor.brooks) input[name="itemIds"]`) and then the one of `ROW(riley.nguyen)`; click
  `#bulk-action-menu-button` (text "Actions" in the list header); let the menu show for 2 s (items "Confirm email",
  "Enable", "Disable", "Delete"); click `.dropdown-menu a.dropdown-item[data-action="Disable"]`; the dialog shows for 1.5 s (title
  "Bulk Action", message "Are you sure you want to disable these items?"); `WaitForNavigationAsync` around a click on
  `#modalOkButton`. On screen at the end: the list with the notifications "User taylor.brooks successfully disabled." and
  "User riley.nguyen successfully disabled." and a **Disabled** badge on both rows (the notifications can come in either order: the code does not sort the checked
  users, `AdminController.cs:232`). The header **Actions** button shows only
  while two or more rows are ticked (the same compiled script: the check is `count > 1`). Labels:
  `U/Views/UsersAdminList.Fields.BulkActions.cshtml:7` ("Actions"), `U/Controllers/AdminController.cs:138-141` (the four
  items), `U/Views/UsersAdminListBulkActions.cshtml:5` ("Bulk Action" and the message), `AdminController.cs:262` (the
  notification). Bulk Enable, Disable and Delete skip the current user: `AdminController.cs:239,252,259,266`.

## 6. Checks for private data

- Every email on screen is `@example.com`; every name is one of the invented user names above.
- No password appears on screen: the Create User page is recorded with the eye icons untouched, and the Change Password
  fields stay masked. The clipboard is never pasted.
- The recording shows only the page, not the browser's address bar, so the host is not visible; the site name is "Orchard Core".
- No leftover users: the list holds only the users of this plan. Check after each clip.
- After assembling, OCR the video at one frame per second and at every scene change, as `SKILL.md` section 6 asks, and
  search for anything that is not in this plan.

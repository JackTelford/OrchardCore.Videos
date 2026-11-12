# Recording plan: Build a Members Area

Read this file alone to record the video. Labels are quoted as the Orchard Core source spells them, with the file and
line in `OrchardCMS/OrchardCore` at commit `92db4a83` (written `Module/File:line`, under `src/OrchardCore.Modules/`
unless the path starts with `src/`). Selectors are for `scene.Page.Locator(...)`. Check each one once with
`Recorder probe` on the running site before recording: the markup was read from the source, not observed.

## What the video shows

An admin video (no code on screen). Chapters and clips, in the order they play:

| Chapter | Clip | Kind | Who is signed in |
|---|---|---|---|
| 00 | `00-intro` | card | none |
| 01 | `01-idea` | 4 slides | none |
| 02 | `02-register` | recording | admin |
| 03 | `03-type` | recording | admin |
| 04 | `04-role` | recording | admin |
| 05 | `05-lock` | recording | admin |
| 06 | `06-page` | recording | admin |
| 07 | `07-signup` | recording | nobody (visitor) |
| 08 | `08-approve` | recording | admin |
| 09 | `09-member` | recording | nobody at first, then the new member |
| 10 | `10-external` | 2 slides | none |
| 99 | `99-outro` | card | none |

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore` at commit `92db4a83` or a later commit that
  keeps the labels listed below, on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside
  both repositories. Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Auto Setup** (no setup form, no state shown), with these environment variables
  (`src/docs/reference/modules/AutoSetup/README.md`):
  - `OrchardCore__AutoSetup__Tenants__0__ShellName` = `Default`
  - `OrchardCore__AutoSetup__Tenants__0__SiteName` = `Orchard Core`
  - `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` = `America/Los_Angeles`
  - `OrchardCore__AutoSetup__Tenants__0__AdminUsername` = `admin`
  - `OrchardCore__AutoSetup__Tenants__0__AdminEmail` = `admin@example.com`
  - `OrchardCore__AutoSetup__Tenants__0__AdminPassword` = the value of `VIDEO_PASSWORD` (chosen outside git)
  - `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` = `Sqlite`
  - `OrchardCore__AutoSetup__Tenants__0__RecipeName` = `Blank`
- **Why the Blank site recipe and not Blog**: the video removes "View all content" from the Anonymous and Authenticated
  roles. A content type that is not Securable has no permission of its own in the role editor, so after that change
  nobody can read it. The Blog recipe has types that are not securable (Article, Blog Post, Tag, Category), so the
  public blog would disappear. The Blank recipe has no page or article types of its own (features it enables, such as
  Menu, add their own types, which this video does not use); the seed below adds exactly two, both with the parts the
  video needs, and `Page` is securable.
- **Features**: the Blank recipe already enables Roles, Users, Contents, Content Types, Autoroute, Html, Title and
  Deployment (needed for JSON Import). Apart from the default theme that the seed enables, do not enable anything else
  before recording. **Users Registration must be off
  at the start**: `02-register` enables it on camera. Email is enabled by that dependency and is never configured.
- **External services**: none. Email verification stays off, so no mail is sent and no SMTP stand-in is needed (the
  tracker note about a local SMTP catcher does not apply to this script; see `OPEN-QUESTIONS.md`). The external sign-in
  providers (Google, GitHub, Meta) are only mentioned on slides; do not enable them.
- **Passwords**: the administrator signs in with `VIDEO_PASSWORD` (`Recorder setup` saves `state.json`). The new member
  of `07-signup` and `09-member` uses a second password chosen outside git and passed in `VIDEO_MEMBER_PASSWORD`; it
  must satisfy the site's password rules (the register form reports what is missing). Never write either value in a
  file. Do not click the eye button of the password field on the login page.
- **Visitor scenes** (`07-signup`, `09-member`) must start signed out. `state.json` signs every scene in as the
  administrator, so start these two scenes with a storage state path that does not exist
  (`Scene.StartAsync` at `.agents/skills/orchardcore-video-producer/scripts/recorder/Scene.cs:93-110` falls back to no stored state when the file is missing, for example
  `Path.Combine(Root, "no-state.json")`).
- **Fake people**: the only member is `jamie.chen`, `jamie.chen@example.com`. The administrator is `admin`,
  `admin@example.com`. No other account is created.

## Seed (outside the capture, once)

1. Start the site and let Auto Setup finish. Run `Recorder setup` to sign the administrator in and save `state.json`.
2. In the browser, open **Tools > Deployments > JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:18-30`, page `/Admin/DeploymentPlan/Import/Json`),
   click inside the JSON box (a code editor, `OrchardCore.Deployment/Views/Import/Json.cshtml`), paste the recipe
   below, and click **Import** (`Recipe imported successfully!` confirms it). It enables the default theme as the site theme,
   defines the content types `Page` (securable) and `Members Page` (not securable yet), creates the public page
   `Welcome` with a link to the members lounge, and makes `Welcome` the home page. The page `Members Lounge` that the
   link points to is created on camera by `06-page`; until then the link leads to a not-found page.

```json
{
  "steps": [
    { "name": "feature", "enable": ["TheTheme"] },
    { "name": "themes", "site": "TheTheme" },
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Page",
          "DisplayName": "Page",
          "Settings": {
            "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true, "Securable": true }
          },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            {
              "PartName": "AutoroutePart", "Name": "AutoroutePart",
              "Settings": {
                "ContentTypePartSettings": { "Position": "1" },
                "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": true }
              }
            },
            { "PartName": "HtmlBodyPart", "Name": "HtmlBodyPart", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
          ]
        },
        {
          "Name": "MembersPage",
          "DisplayName": "Members Page",
          "Settings": {
            "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true }
          },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            {
              "PartName": "AutoroutePart", "Name": "AutoroutePart",
              "Settings": {
                "ContentTypePartSettings": { "Position": "1" },
                "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": true }
              }
            },
            { "PartName": "HtmlBodyPart", "Name": "HtmlBodyPart", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
          ]
        }
      ]
    },
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "4x7k2m9q5w3e8r1t6y0u4i2o9p",
          "ContentType": "Page",
          "DisplayText": "Welcome",
          "Latest": true,
          "Published": true,
          "TitlePart": { "Title": "Welcome" },
          "AutoroutePart": { "Path": "welcome", "SetHomepage": false },
          "HtmlBodyPart": { "Html": "<p>Welcome to the site. Everyone can read this page.</p><p><a href=\"/members-lounge\">Members Lounge</a></p>" }
        }
      ]
    },
    {
      "name": "settings",
      "HomeRoute": { "Action": "Display", "Controller": "Item", "Area": "OrchardCore.Contents", "ContentItemId": "4x7k2m9q5w3e8r1t6y0u4i2o9p" }
    }
  ]
}
```

3. Check the starting state by eye, signed in as the administrator:
   - `/` shows the page **Welcome** in the default theme, with the link **Members Lounge**.
   - **Design > Content Definition > Content Types** lists **Page** and **Members Page** (and the types that enabled
     features add, such as the menu types); editing **Page** shows **Securable** ticked, editing **Members Page** shows
     it cleared.
   - **Access Control > Roles** lists Administrator, Anonymous, Authenticated, Author, Contributor, Editor, Moderator.
     Editing **Anonymous** and **Authenticated**, the row **View all content** has **Allow** ticked in both (default
     permissions of the Contents module, `OrchardCore.Contents/Permissions.cs:88-96`). If either is cleared, stop and
     report it: the video's premise does not hold on that build.
   - **Tools > Features**: **Users Registration** is disabled.
4. Stop the site and copy `App_Data` and `state.json` aside. This is snapshot **S0**.

## Order of the clips and the snapshots

Record in the order of the table above, because each clip starts from the site the previous one left:

| Clip | Changes the site | Needs |
|---|---|---|
| `02-register` | enables Users Registration, saves the Registration settings | S0 |
| `03-type` | makes Members Page securable | `02-register` |
| `04-role` | creates the role Member and grants it **View Members Page by others** | `03-type` (the permission does not exist before) |
| `05-lock` | clears **View all content** on Anonymous and Authenticated, grants **View Page by others** to Anonymous | `04-role` |
| `06-page` | creates and publishes the page Members Lounge | `03-type` |
| `07-signup` | creates the disabled user `jamie.chen` | `05-lock`, `06-page` |
| `08-approve` | enables `jamie.chen` and gives it the role Member | `07-signup` |
| `09-member` | none (a sign-in only) | `08-approve` |

The slide clips, `00-intro`, `99-outro`, and the `01-idea` and `10-external` clips need no site.

After every recorded clip, stop the site and copy `App_Data` aside as the snapshot of that clip (S02, S03, and so
on). To record a clip again, restore the snapshot of the clip before it (S0 for `02-register`), start the site, and
record. Restore S0 before a full take. `08-approve` must run before the member's first sign-in, because the role is
read when the user signs in.

## The clips

Every recording is opened with `scene.OpenAsync` on the page given under "Starts on" before `StartCaptureAsync`. The
admin menu is expanded and visible in these clips, because the menu clicks are part of what the narration says (the
specification collapses it only on builder screens). A menu click means: open the admin menu entry named, then the
child, using `ClickAndWaitForNavigationAsync` on the last one. The menu entries are in the left admin menu; locate each
by its exact text, for example `scene.Page.Locator("#adminMenu").GetByText("Tools", new() { Exact = true })` (check the
container id with `Recorder probe`).

### `01-idea` (slides, no recording)

- `idea-1`: three cards: Registration, Roles, Permissions.
- `idea-2`: four cards, Anonymous and Authenticated bright, the other two dimmed.
- `idea-3`: the same four cards, "View all content" and "Member" bright.
- `idea-4`: four cards: Register, Secure, Restrict, Try it.

### `02-register` (admin)

Starts on `/Admin` (the dashboard), signed in as the administrator, from S0.

- `register-1`: menu click **Tools** then **Features** (`OrchardCore.Features/AdminMenu.cs:26-27`; page `/Admin/Features`).
  Type `registration` into the search box (`#search-box`, placeholder "Search", `OrchardCore.Features/Views/Admin/Features.cshtml:27`).
  Highlight the row **Users Registration** (`OrchardCore.Users/Manifest.cs:47`; the row shows the name and the id
  `OrchardCore.Users.Registration`). Click its **Enable** button (`a[id^='btn-enable-'][id*='Users_Registration']`,
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`) with `ClickAndWaitForNavigationAsync`.
  Wait until the Features page has reloaded (the search box is present again). On screen at the end: the reloaded
  Features page. Keep the actions short: type with a delay of 30 ms.
- `register-2`: menu click **Settings**, **Security**, **Registration** (`OrchardCore.Users/RegistrationAdminMenu.cs:26-28`;
  page `/Admin/Settings/userRegistration`). Then highlight the label
  **Users must verify their email address** (`RegistrationSettings.Edit.cshtml:23`) and keep it highlighted until the
  narration ends. On screen at the end: the Registration page with the three options and the first one highlighted.
- `register-3`: `ClearHighlightsAsync`, highlight **Use site theme for registration page** (`RegistrationSettings.Edit.cshtml:43`),
  then clear it and highlight **Users must be approved before they can log in** (`RegistrationSettings.Edit.cshtml:33`).
  Do not click yet. On screen at the end: the approval option highlighted.
- `register-4`: `ClearHighlightsAsync`; click the checkbox `scene.Page.GetByLabel("Users must be approved before they can log in")`;
  click **Save** (`button.primaryAction.save`, `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`) with
  `ClickAndWaitForNavigationAsync`. The page reloads with the message "Site settings updated successfully."
  (`OrchardCore.Settings/Controllers/AdminController.cs:119`). On screen at the end: the Registration page with the
  approval option ticked and the message.

### `03-type` (admin)

Starts on `/Admin`, from the snapshot after `02-register`.

- `type-1`: menu click **Design**, **Content Definition**, **Content Types** (`OrchardCore.ContentTypes/AdminMenu.cs:23-25`;
  page `/Admin/ContentTypes/List`). Highlight the rows **Page** and **Members Page** in turn, then click the **Edit**
  button of Members Page (`[role='btn-edit-MembersPage']`, `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`)
  with `ClickAndWaitForNavigationAsync`. On screen at the end: the page "Edit Content Type - Members Page".
- `type-2`: scroll to the **Securable** option and highlight its label with the hint
  "Determines if this content type can have custom permissions." (`OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:69-70`).
  The box is cleared. On screen at the end: the option highlighted and cleared.
- `type-3`: `ClearHighlightsAsync`; click `scene.Page.GetByLabel("Securable")`; scroll to the bottom and click **Save**
  (`button.save[value='Save']`, `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:215`) with
  `ClickAndWaitForNavigationAsync`. On screen at the end: the page shown after the save, with the message
  "Content type updated successfully." if it is displayed (`OrchardCore.ContentTypes/Controllers/AdminController.cs:225`).

### `04-role` (admin)

Starts on `/Admin`, from the snapshot after `03-type`.

- `role-1`: menu click **Access Control**, **Roles** (`OrchardCore.Roles/AdminMenu.cs:18-19`; page `/Admin/Roles`).
  Highlight the **System** badge of the Administrator row (`RoleEntry.DefaultTags.SummaryAdmin.cshtml:9`). Click
  **Add Role** (`a.create`, `OrchardCore.Roles/Views/Admin/Index.cshtml:31`) with `ClickAndWaitForNavigationAsync`.
  On screen at the end: the page "Create Role".
- `role-2`: type `Member` into **Name** (`input[name='RoleName']`, `OrchardCore.Roles/Views/Admin/Create.cshtml:15-17`) and
  `Approved members of the site.` into **Description** (`textarea[name='RoleDescription']`, line 25). Click
  **Save & Configure** (`button.save-configure`, line 32) with `ClickAndWaitForNavigationAsync`. On screen at the end:
  the page "Edit 'Member' Role" with the permission grid.
- `role-3`: type `Members Page` into the permission search box (`#search-box`, placeholder "Search permissions",
  `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49`). The grid filters to the group
  "Members Page Content Type - MembersPage" (`OrchardCore/OrchardCore.Contents.Core/ContentTypePermissionsHelper.cs:155`).
  Highlight the column headings **Allow** and **Effective** (`_RolePermissions.cshtml:69-70`). On screen at the end:
  the filtered group with its headings highlighted.
- `role-4`: `ClearHighlightsAsync`; click the checkbox of the row **View Members Page by others**
  (`input[id='Checkbox.View_MembersPage']` in `tr[data-text='View Members Page by others']`; description from
  `ContentTypePermissionsHelper.cs:19`). Click **Save** (`button.save`, `OrchardCore.Roles/Views/Admin/Edit.cshtml:33`)
  with `ClickAndWaitForNavigationAsync`. The Roles list reloads with "Role updated successfully." On screen at the
  end: the Roles list with Member in it.

### `05-lock` (admin)

Starts on `/Admin/Roles`, from the snapshot after `04-role`.

- `lock-1`: highlight the **Anonymous** row (`.item[data-filter-value='anonymous']`), click its **Edit** button
  (`a.edit`, `OrchardCore.Roles/Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12`) with `ClickAndWaitForNavigationAsync`,
  type `view` into `#search-box`, then scroll the row **View all content** into view
  (`tr[data-text='View all content']`, text from `OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:29`).
  Its **Allow** box is ticked. On screen at the end: that row visible and ticked.
- `lock-2`: highlight the row **View all content**, then (still without clicking) the row
  `tr[data-text='View Page by others']`, which is in the group "Page Content Type - Page". On screen at the end: both
  rows visible, the second highlighted.
- `lock-3`: `ClearHighlightsAsync`; click `input[id='Checkbox.ViewContent']` to clear it; click
  `input[id='Checkbox.View_Page']` to tick it; click **Save** with `ClickAndWaitForNavigationAsync`. On screen at the
  end: the Roles list with "Role updated successfully."
- `lock-4`: click **Edit** of the **Authenticated** row (`.item[data-filter-value='authenticated'] a.edit`), type `view`
  into `#search-box`, scroll **View all content** into view, click `input[id='Checkbox.ViewContent']` to clear it, click
  **Save** with `ClickAndWaitForNavigationAsync`. On screen at the end: the Roles list with the message.

### `06-page` (admin)

Starts on `/Admin`, from the snapshot after `05-lock`.

- `page-1`: menu click **Content**, **Content Items** (`OrchardCore.Contents/AdminMenu.cs:65-68`; page
  `/Admin/Contents/ContentItems`). Click **New** (`#new-dropdown`, `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:19-20`),
  then the entry **Members Page** in the open list (`.dropdown-menu a.dropdown-item` with the text), with
  `ClickAndWaitForNavigationAsync`. On screen at the end: the empty Members Page editor.
- `page-2`: type `Members Lounge` into **Title** (`scene.Page.GetByLabel("Title", new() { Exact = true })`, `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`),
  type `members-lounge` into **Permalink** (`scene.Page.GetByLabel("Permalink", new() { Exact = true })`, `OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`),
  then click into the body editor (hint "The body of the content item.", `OrchardCore.Html/Views/HtmlBodyPart.Edit.cshtml:13`)
  with `scene.ClickAsync(scene.Page.Locator(".CodeMirror"))` and type the plain words `Welcome, members.` with
  `scene.Page.Keyboard.TypeAsync` (a delay of 55 ms), not with `TypeAsync(locator, text)`, which presses keys on the
  locator and may not reach the editor's hidden text area (`Scene.cs:443-448`). No markup. On screen at the end: the
  three fields filled.
- `page-3`: click **Publish** (`button.publish`, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`) with
  `ClickAndWaitForNavigationAsync`. The Content Items list reloads with the new item, Members Lounge, of type
  Members Page. On screen at the end: that list.

### `07-signup` (visitor, signed out)

Starts on `/` (the page Welcome), signed out (see "Visitor scenes"), from the snapshot after `06-page`.

The login, register and pending pages are drawn with the admin theme's login layout, because "Use site theme for
registration page" stays cleared and the login setting of the same name is off by default
(`OrchardCore.Users/Services/UsersThemeSelector.cs:37-86`, layout `OrchardCore.Themes/TheAdmin/Views/Layout-Login.cshtml`). Only the Welcome page and the Members Lounge page use the
site theme. That is expected; do not change either setting.

- `signup-1`: hold on the Welcome page for about three seconds, move to the link **Members Lounge**
  (`a:has-text('Members Lounge')`) and click it with `ClickAndWaitForNavigationAsync`. The site redirects to the login
  page (`ChallengeOrForbid`, `OrchardCore.Contents/Controllers/ItemController.cs:36`; cookie login path,
  `OrchardCore.Users/Startup.cs:129`). On screen at the end: the **Log in** page (`OrchardCore.Users/Views/Account/Login.cshtml:27`).
- `signup-2`: highlight the link **Register as a new user** (`OrchardCore.Users/Views/LoginFormRegisterUser.cshtml:2`),
  then click it with `ClickAndWaitForNavigationAsync`. On screen at the end: the page "Register a new account"
  (`OrchardCore.Users/Views/RegisterUserForm.Edit.cshtml:3`).
- `signup-3`: type `jamie.chen` into **Username**, `jamie.chen@example.com` into **Email address**, the value of
  `VIDEO_MEMBER_PASSWORD` into **Password** and into **Confirm password**
  (`OrchardCore.Users/Views/RegisterUserFormIdentifier.cshtml:4,10,16,22`; use `GetByLabel` with `Exact = true` for
  "Password"). Click **Register** (`OrchardCore.Users/Views/RegisterUserForm.Edit.cshtml:16`) with
  `ClickAndWaitForNavigationAsync`. The account is created disabled because approval is on
  (`OrchardCore/OrchardCore.Users.Core/Services/UserService.cs:366`), and the site redirects to the pending page
  (`OrchardCore.Users/Controllers/RegistrationController.cs:73`, `UserModerationLoginFormEvent`). On screen at the end:
  the page **User Registration Pending**.
- `signup-4`: hold on the pending page and highlight its heading and the sentence "Your user account has been created but
  has to be approved before it can be used." (`OrchardCore.Users/Views/Registration/RegistrationPending.cshtml:7-9`).

### `08-approve` (admin)

Starts on `/Admin`, signed in as the administrator, from the snapshot after `07-signup`.

- `approve-1`: menu click **Access Control**, **Users** (`OrchardCore.Users/AdminMenu.cs:27-30`; page `/Admin/Users`).
  Highlight the **Disabled** badge in the row of `jamie.chen` (`OrchardCore.Users/Views/UserFields.cshtml:22-23`). On
  screen at the end: the list with `jamie.chen` and the badge.
- `approve-2`: `ClearHighlightsAsync`; in the row of `jamie.chen` click the **Actions** menu button
  (`button.actions`, title "Actions", `OrchardCore.Admin/Views/AdminListActions.cshtml:30-32`), click **Enable**
  (`a.dropdown-item` with the text, `OrchardCore.Users/Views/UserActionsMenu.cshtml:24`), then click **Ok** in the dialog
  "Enable user" (`#modalOkButton`; text from `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`). The list reloads with
  "User account was successfully enabled." and the badge is gone. Wait for that reload.
- `approve-3`: click **Edit** in the row (`a.btn` with the text, `OrchardCore.Users/Views/UserButtons.cshtml:11`) with
  `ClickAndWaitForNavigationAsync`; in the section **Roles** (`OrchardCore.Users/Views/UserRoleFields.Edit.cshtml:6`) tick
  **Member** (`scene.Page.GetByLabel("Member", new() { Exact = true })`); click **Save** (`button.save`,
  `OrchardCore.Users/Views/UserSaveButtons.Edit.cshtml:2`) with `ClickAndWaitForNavigationAsync`. On screen at the end:
  the Users list with "User updated successfully." and the **Member** badge in the row.
- `approve-4`: open the **Actions** menu of the `jamie.chen` row again and highlight **Disable**
  (`OrchardCore.Users/Views/UserActionsMenu.cshtml:20`) without clicking it. On screen at the end: the open menu with
  **Disable** highlighted.

### `09-member` (visitor, then member)

Starts on `/`, signed out (see "Visitor scenes"), from the snapshot after `08-approve`.

- `member-1`: click the link **Members Lounge** with `ClickAndWaitForNavigationAsync`; on the login page type `jamie.chen`
  into **Username or email address** (`OrchardCore.Users/Views/LoginFormCredentials.cshtml:7`), the value of
  `VIDEO_MEMBER_PASSWORD` into **Password** (line 13), and click **Log in** (`LoginForm.Edit.cshtml:9`) with
  `ClickAndWaitForNavigationAsync`. The login returns to the page asked for
  (`OrchardCore.Users/Controllers/AccountBaseController.cs:31`). On screen at the end: the page Members Lounge.
- `member-2`: hold on the Members Lounge page and highlight its title. On screen at the end: the page with its title and
  text, highlighted.
- `member-3`: `ClearHighlightsAsync`; `NavigateAsync("/")`. On screen at the end: the Welcome page.

### `10-external` (slides, no recording)

- `external-1`: three cards: Google Authentication, GitHub Authentication, Meta Login, each with its menu path
  (`OrchardCore.Google/GoogleAuthenticationAdminMenu.cs:27-28`, `OrchardCore.GitHub/AdminMenuGitHubLogin.cs:28`,
  `OrchardCore.Facebook/AdminMenuLogin.cs:28`).
- `external-2`: three cards: the same Registration settings, an External Authentication tab
  (`OrchardCore.Users/Drivers/ExternalRegistrationSettingsDisplayDriver.cs:44`), and Disable new external registration
  (`OrchardCore.Users/Views/ExternalRegistrationSettings.Edit.cshtml:23`).

## Checks for private data

- The site is made from scratch with fake data only: no mask script is needed. Do not commit `state.json`, a copy of
  `App_Data`, or either password.
- After recording, OCR the video at one frame per second and at every scene change, as the skill asks. The only
  addresses that may appear are `admin@example.com` and `jamie.chen@example.com`; the only account names are `admin`
  and `jamie.chen`.
- Look at the contact sheet of `07-signup` and `09-member`: the password fields must show dots, not text, and the eye
  button of the login form must not have been clicked.
- No browser chrome, local path, machine name or real host name is visible; the address of every page is
  `localhost` (not shown in the headless capture).

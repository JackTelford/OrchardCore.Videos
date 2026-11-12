# Recording plan: permissions-create

A developer video. Four of its five task chapters are slides (`slides.py`, drawn by `assemble.py`; nothing to record).
**One clip is a browser recording: `04-roles`**, the roles editor. This plan is for the person who records it; the
Orchard Core paths below are relative to `src/` in the Orchard Core checkout the site runs from (the source this plan
was written from is the `main` branch at commit `92db4a83`). A path that starts with `Views/`, `Controllers/` or
`Assets/`, or a file named `_RolePermissions.cshtml` (in `Views/Admin/`), is inside `OrchardCore.Modules/OrchardCore.Roles/`.

## The site it needs

| | |
|---|---|
| Site | `src/OrchardCore.Cms.Web`, on `http://localhost:5010` (`VIDEO_BASE_URL` otherwise), with its own `ORCHARD_APP_DATA` folder outside both repositories |
| Provisioning | Auto Setup, so no setup or login form is recorded. Tenant `Default`; site name `Orchard Core`; time zone `America/Los_Angeles`; recipe `Blog`; database `Sqlite`; user `admin`; email `admin@example.com`; password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD` and in the Auto Setup `AdminPassword`). Environment variables: `OrchardCore__AutoSetup__Tenants__0__ShellName`, `...__SiteName`, `...__SiteTimeZone`, `...__AdminUsername`, `...__AdminEmail`, `...__AdminPassword`, `...__DatabaseProvider`, `...__RecipeName` (docs: `docs/reference/modules/AutoSetup/README.md`, "User Secrets and Environment Variables") |
| Outbound calls | Blocked: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The clip calls no external service |
| Features to enable | None. The Blog recipe enables `OrchardCore.Roles` (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step) and its `Roles` step creates the roles Moderator, Editor, Author and Contributor (same file, the `Roles` step) |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`; it saves `state.json`, which is git-ignored |
| Browser | The recorder's: 1536x864 CSS pixels, light color scheme, en-US (`.agents/skills/orchardcore-video-producer/scripts/recorder/Scene.cs:14-15` and `:110-111`) |
| Private data | None. The only people in the clip are the role names below. No mask script is needed |

The video shows the permissions of the Roles module itself. The Roles module declares them with the code of the
slides (`OrchardCore/OrchardCore.Roles.Core/RolesPermissions.cs:8-9`: Manage Roles is security critical, View Roles is
implied by Manage Roles), so the site needs no custom module.

### Seed data (outside the capture)

One fake role, created before recording, that holds only Manage Roles. Do it by hand as `admin`:

1. Open `/Admin/Roles/Index`. Click **Add Role** (`OrchardCore.Modules/OrchardCore.Roles/Views/Admin/Index.cshtml:31`).
2. In **Name** type `Manager`; in **Description** type `Manages the roles of the site.`
   (`Views/Admin/Create.cshtml:15` and `:23`).
3. Click **Save & Configure** (`Views/Admin/Create.cshtml:32`). The page `/Admin/Roles/Edit/Manager` opens.
4. In **Search permissions** (`Views/Admin/_RolePermissions.cshtml:49`) type `manage roles`. Tick the **Allow** box of
   the **Manage Roles** row (its id is `Checkbox.ManageRoles`, `_RolePermissions.cshtml:131`). Do not tick any other box.
5. Click **Save** (`Views/Admin/Edit.cshtml:33`). The Roles list opens and shows the role `Manager`.

Check the result: reopen `/Admin/Roles/Edit/Manager`, type `roles` in **Search permissions**. Exactly two rows must
remain, both in one group:

| Row | Allow | Effective |
|---|---|---|
| Manage Roles | ticked | ticked |
| View Roles | not ticked | ticked |

If another row remains, some other permission has a description with the word "roles" in it; write what you see in
`OPEN-QUESTIONS.md` and do not record until the narration of `roles-2` to `roles-4` matches the screen. (The source has
no such description: only `OrchardCore/OrchardCore.Roles.Core/RolesPermissions.cs:8-9` contains it. The role names of
the Users module's role permissions, such as "Edit users in Manager role", do not contain "roles" either, which is why
the seed role is named `Manager` and not something with "roles" in its name.)

Then stop the site and copy its `App_Data` folder aside as the **snapshot** for this video. The clip only reads the
site, but a snapshot makes a new take start from the same state.

## Order, and what each clip changes

There is one clip, `04-roles`. It depends on the seed data above. It changes nothing: it opens pages, types in the
search box (the box filters the page and saves nothing), and never clicks **Save** or **Delete**. A new take needs no
restore, unless someone saved a role by mistake: then restore the snapshot.

## Clip `04-roles` (chapter 04, "See them in the roles editor")

- Starts on: `/Admin/Roles/Index` (the Roles list), opened with `scene.OpenAsync("/Admin/Roles/Index")`, not recorded.
  It lists, in this order (sorted by name, `OrchardCore.Modules/OrchardCore.Roles/Controllers/AdminController.cs:88`):
  Administrator, Anonymous, Authenticated, Author, Contributor, Editor, Manager, Moderator.
- Then `scene.StartCaptureAsync()`.
- Locators (each stands for `scene.Page.Locator(...)`). `scene.HighlightAsync` and the click methods need a locator that
  matches exactly one element (Playwright is strict; `HighlightAsync` logs "nothing to highlight" and draws nothing when
  a locator matches several). Every locator below is therefore scoped to one element. The search only hides the other
  rows and groups (`display: none`), they stay in the page, so a page-wide locator such as `.badge` matches many:
  - `managerLink`: `a[href$='/Roles/Edit/Manager']`, the first match (`.First`; the name of the role is a link,
    `Views/RoleEntry.Fields.SummaryAdmin.cshtml:7`).
  - `adminLink`: `a[href$='/Roles/Edit/Administrator']`, the first match (`.First`).
  - `search`: `#search-box` (`_RolePermissions.cshtml:49`).
  - `rolesGroup`: `fieldset.permissions-list` filtered by the text `(OrchardCore.Roles)`; `rolesLegend` is its `legend`
    (`_RolePermissions.cshtml:56-63`).
  - `rolesTable`: `rolesGroup.Locator("table")`, the one table of the group, which holds its two rows.
  - `badge`: `rolesGroup.Locator(".badge")`: inside the group only the Manage Roles row has one
    (`_RolePermissions.cshtml:125`, shown when `permission.IsSecurityCritical`).
  - `allowManage`: `[id='Checkbox.ManageRoles']`; `allowView`: `[id='Checkbox.ViewRoles']`;
    `effectiveView`: `[id='Effective.ViewRoles']` (`_RolePermissions.cshtml:131` and `:137`). Each id exists once.
  - `cancel`: `a.cancel` (`Views/Admin/Edit.cshtml:41`).
  - `adminNotice`: `form .alert.alert-info` (`_RolePermissions.cshtml:32`; the partial is inside the form of
    `Views/Admin/Edit.cshtml:15`).

`roles-1`
: Narration: the roles editor lists the permissions of every enabled feature (about 8 seconds), then "Let's open the
  Manager role" (about 2 seconds), then the Roles module declares its own permissions (about 5 seconds).
  Actions: `await scene.PauseAsync(7500)` while the Roles list stays on screen, then
  `await scene.ClickAndWaitForNavigationAsync(managerLink)`.
  On screen when the narration ends: the page titled **Edit 'Manager' Role**, with the **Description** box holding
  `Manages the roles of the site.`, the box that explains **Allow** and **Effective**, and the **Search permissions** box.
  Source of the labels: `Views/Admin/Edit.cshtml:6` and `:17`, `_RolePermissions.cshtml:40`, `:43` and `:49`.

`roles-2`
: Narration: permissions are grouped by feature, with the name and the identifier (about 7 seconds); each row shows a
  description, and the **Search permissions** box filters the rows (about 5 seconds).
  Actions: `await scene.TypeAsync(search, "roles", delay: 90)`. Wait until only one group is visible (the other
  `fieldset.permissions-list` elements are hidden by the script, `OrchardCore.Modules/OrchardCore.Roles/Assets/ts/roles-edit.ts:11-46`,
  which runs on each key release):
  wait for `fieldset.permissions-list:visible` to have a count of 1. Then
  `await scene.HighlightAsync(rolesLegend, "Feature name and identifier")`, `await scene.PauseAsync(6000)`,
  `await scene.ClearHighlightsAsync()`, and `await scene.HighlightAsync(rolesTable, "Descriptions from the code")`.
  On screen when the narration ends: **Search permissions** holding `roles`; the group headed **Roles (OrchardCore.Roles)**
  with its two rows **Manage Roles** and **View Roles**, highlighted. No other group is visible.
  Source: group heading and identifier `_RolePermissions.cshtml:57-63`; the grouping `AdminController.cs:290-319`
  (`GetInstalledPermissionsAsync`) and `:412` (`GetGroupKey`); the row text `_RolePermissions.cshtml:12-27`.

`roles-3`
: Narration (about 7 seconds): Manage Roles carries the Security Critical badge; this role is allowed it.
  Actions: `await scene.ClearHighlightsAsync()`, `await scene.HighlightAsync(badge, "Security Critical")`,
  `await scene.PauseAsync(3000)`, `await scene.ClearHighlightsAsync()`, `await scene.HighlightAsync(allowManage, "Allow")`.
  The search still holds `roles`, so only the Roles group is visible and `badge` finds its one element.
  On screen when the narration ends: the **Manage Roles** row with the red **Security Critical** badge at its right,
  and its **Allow** box ticked and highlighted.
  Source: `_RolePermissions.cshtml:122-127` (badge), `:131` (box).

`roles-4`
: Narration (about 10 seconds): View Roles is not ticked under Allow, but it is Effective, because Manage Roles implies it.
  Actions: `await scene.ClearHighlightsAsync()`, `await scene.HighlightAsync(allowView, "Allow")`,
  `await scene.PauseAsync(3500)`, `await scene.ClearHighlightsAsync()`, `await scene.HighlightAsync(effectiveView, "Effective")`.
  On screen when the narration ends: the **View Roles** row with **Allow** not ticked and **Effective** ticked and
  highlighted. Both boxes of the row are visible in the same frame.
  Source: the implied permission `OrchardCore/OrchardCore.Roles.Core/RolesPermissions.cs:9`; the **Effective** column
  `_RolePermissions.cshtml:70` and `:137`; the check that fills it `Controllers/AdminController.cs:451` (`GetEffectivePermissions`).
  Do not hover the **Effective** box to open its tooltip: whether the tooltip renders in the headless browser is an
  open question (see `OPEN-QUESTIONS.md`), and the narration does not depend on it.

`roles-5`
: Narration (about 9 seconds): the Administrator role has no boxes to tick; its editor says that this system role
  provides comprehensive access by granting all available permissions.
  Actions: `await scene.ClearHighlightsAsync()`, `await scene.ClickAndWaitForNavigationAsync(cancel)` (back to the
  Roles list), `await scene.ClickAndWaitForNavigationAsync(adminLink)`, then `await scene.HighlightAsync(adminNotice)`
  (it waits until the notice is visible).
  On screen when the narration ends: the page titled **Edit 'Administrator' Role**, with the **Description** box and
  the notice **This system role provides comprehensive access by granting all available permissions.** highlighted. No
  permissions table is on the page.
  Source: `_RolePermissions.cshtml:30-33`; the Administrator role is the system administrator role
  (`OrchardCore/OrchardCore.Roles.Core/DefaultSystemRoleProvider.cs`, `GetAdminRole`).

## The slides (nothing to record)

Drawn by `slides.py`, one image per step. The chapters and what each slide shows:

- `idea-1`: three cards, **Permission**, **Role**, **User**, with one line each.
- `idea-2`: three cards, **Anonymous**, **Authenticated**, **Administrator**, and who each one applies to.
- `idea-3`: three cards, **Declare**, **Provide**, **Check**: the three jobs of a module.
- `declare-1`: the `MyPermissions` class with one permission, `ManageWidgets`; the declaration is bright.
- `declare-2`: the same code, the name line bright; cards: unique, ignoring case, duplicates.
- `declare-3`: the same code, the `using` line and the description line bright; cards: the declaring type, the PO context.
- `declare-4`: `ManageWidgets` with `isSecurityCritical: true`, and `ViewWidgets` implied by it; the first permission is bright.
- `declare-5`: the same code, `ViewWidgets` bright.
- `declare-6`: a permission with `Category = "Widgets"`.
- `provide-1`: the provider class `Permissions : IPermissionProvider`, `GetPermissionsAsync` bright.
- `provide-2`: the same class, `GetDefaultStereotypes` bright, with the `Editor` role.
- `provide-3`: the same class, the stereotype bright; cards: only adds, the Administrator role.
- `provide-4`: the `Startup` class with `services.AddPermissionProvider<Permissions>();` bright.
- `provide-5`: a `Roles` recipe step that adds `ManageWidgets` to the `Editor` role, with a `Description` (the step sets the
  description of the role from the entry, `OrchardCore.Modules/OrchardCore.Roles/Recipes/RolesStep.cs:53`, so an entry
  without one would clear it); cards: Replace, Add, Description.
- `check-1`: the `WidgetsController` with `AuthorizeAsync` and `Forbid` bright.
- `check-2`: the same code; cards: holds it, implied, Administrator.
- `check-3`: a Razor view with `AuthorizeAsync` around a link; cards: the view, the controller.
- `check-4`: an `AdminMenu` with `.Permission(MyPermissions.ManageWidgets)` bright.

The code of the slides is taken from the Orchard Core source and documentation, and trimmed. Where it comes from:
`RolesPermissions.cs:8-9` (declaration, security critical, implied), `OrchardCore.Modules/OrchardCore.Roles/Permissions.cs`
(the provider), `OrchardCore.Modules/OrchardCore.Roles/Startup.cs:50` (the registration), `Controllers/AdminController.cs:76-79`
(the check and `Forbid`), `Views/Admin/Index.cshtml:8-11` and `:28-33` (the view), `AdminMenu.cs:18-23` (the menu item),
and `docs/reference/modules/Roles/README.md` ("Declaring permissions" and the recipe example, with the `Description` line added;
the behaviors are in `Recipes/RolesStep.cs:55-80`).

## Checks for private data

- The only names on screen are the role names of the Blog recipe (Administrator, Anonymous, Authenticated, Author,
  Contributor, Editor, Moderator) and the seed role `Manager`. There is no user list in the clip.
- The browser's address bar is not recorded. The site name is `Orchard Core`, the user `admin` with the email
  `admin@example.com`; the password is never typed on camera because the session comes from `state.json`.
- After recording, run the OCR scan of the skill (one frame per second and at every scene change) and search for any
  text that is not part of the list above.

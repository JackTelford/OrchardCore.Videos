# Recording plan: Tenants: Edit, Disable, Reload and Remove

For the person who records this video. Follow it as written: nothing here needs a decision. In this file a selector such
as `a.btn.edit` stands for `scene.Page.Locator("a.btn.edit")`, since the recorder's methods take locators. File and line
references are to the source of Orchard Core at commit `92db4a83` (paths start at `src/`); the labels were read from
there. A path that starts with `Views/`, `Controllers/` or the name of a file of the module (`AdminMenu.cs`,
`Permissions.cs`, `TenantsOptions.cs`) is under `src/OrchardCore.Modules/OrchardCore.Tenants/`, and a bare
`AdminController.cs` is that module's `Controllers/AdminController.cs`. `OrchardCore.Tenants/...`,
`OrchardCore.Admin/...` and `OrchardCore.AutoSetup/...` are under `src/OrchardCore.Modules/`, and `OrchardCore/...` under `src/`.

The narration was written from the code. Everything the code does not show on its own is listed in
`OPEN-QUESTIONS.md`: read it before the first take.

## 1. The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web`, at the commit above or later on `main`, on `http://localhost:5010`,
  with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Provisioning**: Auto Setup, through environment variables, so no setup or sign-in form is recorded. This video is
  not about the Setup screen, so it uses `state.json` like the others.
- **Default tenant**: recipe `SaaS` (displayed as "Software as a Service"), site name **Orchard Core**, time zone
  `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (the value of
  `VIDEO_PASSWORD`, chosen outside git; never typed on camera and never written in a file of the repository). The `SaaS`
  recipe enables `OrchardCore.Tenants` (the **Multi-Tenancy** menu) and `OrchardCore.Features` (the **Features**
  button in the row of a running tenant).
- **Features to enable before recording**: none. Do not enable Tenant Feature Profiles: the **Feature profile** field of
  the Edit page and the Feature Profiles menu entry must not exist on this site.
- **Settings to change**: none. Leave the Admin list settings (Configuration, Settings, Admin) at their defaults: the
  tenants are listed as rows (the List layout) and the actions of a row are buttons (the Buttons layout).
  The defaults are `List` and `Buttons`: `OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`.
- **Tenant removal must be allowed** on this site, or the **Remove** button does not exist
  (`OrchardCore.Tenants/Views/ManageTenantActionButtons.cshtml:26`, `TenantsOptions.cs:5`). It is set by the environment
  variable `OrchardCore__Tenants__TenantRemovalAllowed=true`. It is for the recorder only: the narration says that
  removal is off by default and does not name the variable.
- **External services**: none. Block outbound calls with a dead proxy address (below).

### Starting the site

```bash
export ORCHARD_APP_DATA=<an empty folder outside both repositories>
export ASPNETCORE_URLS=http://localhost:5010
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9 NO_PROXY=localhost,127.0.0.1
export OrchardCore__Tenants__TenantRemovalAllowed=true

# Auto Setup: the Default tenant, then the tenants of the seed (section 2). The password is the one of VIDEO_PASSWORD.
i=0
for entry in "Default|Orchard Core||SaaS" "Acme|Acme Bakery|acme|Blog" "Harbor|Harbor Books|harbor|Blog" \
             "Juniper|Juniper Studio|juniper|Blog" "Maple|Maple Cafe|maple|Blog" "Pilot|Pilot Trial|pilot|Blog"; do
  IFS='|' read -r name site prefix recipe <<< "$entry"
  p="OrchardCore__AutoSetup__Tenants__$i"
  export "${p}__ShellName=$name" "${p}__SiteName=$site" "${p}__SiteTimeZone=America/Los_Angeles" \
         "${p}__AdminUsername=admin" "${p}__AdminEmail=admin@example.com" "${p}__AdminPassword=$VIDEO_PASSWORD" \
         "${p}__DatabaseProvider=Sqlite" "${p}__RecipeName=$recipe"
  if [ -n "$prefix" ]; then export "${p}__RequestUrlPrefix=$prefix"; fi
  i=$((i+1))
done

dotnet run --project src/OrchardCore.Cms.Web -c Release --no-launch-profile
```

Stop the site by its process ID, never by image name.

## 2. The seed (created outside the capture)

All names are invented. The tenants have no category and no description at the start: the **Edit** clip types them.

| Tenant | Site name | Url Prefix | Recipe | State when recording starts | Used by |
|---|---|---|---|---|---|
| `Default` | Orchard Core | (none) | SaaS | Running | hosts the admin |
| `Acme` | Acme Bakery | `acme` | Blog | Running | `03-edit` |
| `Harbor` | Harbor Books | `harbor` | Blog | Running | `01-list` search, `04-disable`, `05-reload` |
| `Juniper` | Juniper Studio | `juniper` | Blog | Running | `06-bulk` |
| `Maple` | Maple Cafe | `maple` | Blog | Running | `06-bulk`, then `07-remove` |
| `Pilot` | Pilot Trial | `pilot` | Blog | **Uninitialized** (never requested) | `01-list` State filter, `03-edit` |

How to reach that state, once, with the site running and nothing yet recorded:

1. Request `http://localhost:5010/` once and wait for a `200`. Auto Setup sets up the Default tenant and creates the
   settings of the other five as Uninitialized tenants (`OrchardCore.AutoSetup/AutoSetupMiddleware.cs:129-145`).
2. Request `/acme`, `/harbor`, `/juniper` and `/maple` once each, and wait for a `200` after the redirect. Each request
   sets up that tenant on demand (`AutoSetupMiddleware.cs:95-147`). For example:
   `curl --noproxy '*' -L -s -o /dev/null -w "%{http_code}\n" http://localhost:5010/acme`. **Never request `/pilot`**:
   it must stay Uninitialized.
3. Sign the recorder in (`Recorder setup`, user `admin`, `VIDEO_PASSWORD`), which saves `state.json`.
4. Open `/Admin/Tenants` in the recorder's browser and check it against the **Row layout check** below.
5. Stop the site and copy `App_Data` and `state.json` aside. **This is the snapshot.** Restore it before every full
   take, then start the site again with the same environment.

### Row layout check (before the first take)

Run `Recorder probe /Admin/Tenants "() => [...document.querySelectorAll('li.list-group-item')].map(li => li.innerText)"`
(set `MSYS_NO_PATHCONV=1` in Git Bash, or the path is turned into a Windows path). The six rows, in this order, are Acme, Default, Harbor,
Juniper, Maple, Pilot (sorted by name, `AdminController.cs:139-144`). Each row must show its buttons as links, not
behind an ellipsis menu (`OrchardCore.Admin/Views/AdminListActions.cshtml` is the Buttons layout,
`AdminListActions-Menu.cshtml` the Menu layout). If the site draws an ellipsis menu, stop and read `OPEN-QUESTIONS.md`.
Expected buttons in the rows:

- running tenants other than Default: **Edit**, **View**, **Disable**, **Reload**, **Features**;
- `Default`: **Edit**, **View**, **Reload** (no **Disable**, no **Features**);
- `Pilot` (Uninitialized): **Edit**, **Setup**, **Remove**, **Reload**.

Sources: `OrchardCore.Tenants/Views/ManageTenantActionButtons.cshtml:9-30`, `ManageFeaturesActionButton.cshtml:4-6`.

## 3. Clip order, dependencies and what changes the site

Record in this order, which is the order they play in. The slide chapters (`02-states`, `07-rules`) are not recorded.

| Order | Clip | Starts on | Changes the site? | Depends on |
|---|---|---|---|---|
| 1 | `01-list` | `/Admin` | No | the snapshot |
| 2 | `03-edit` | `/Admin/Tenants` | **Yes**: Acme gets a category and a description | the snapshot |
| 3 | `04-disable` | `/Admin/Tenants` | Yes, but ends where it started: Harbor is Disabled, then Running again | the snapshot |
| 4 | `05-reload` | `/Admin/Tenants` | No (Harbor is released and built again) | the snapshot |
| 5 | `06-bulk` | `/Admin/Tenants` | **Yes**: Juniper and Maple become Disabled | the snapshot |
| 6 | `07-remove` | `/Admin/Tenants` | **Yes**: Maple is removed for good | `06-bulk` (Maple is Disabled) |

- A full take: restore the snapshot, start the site, record the six clips in order without restoring in between.
  Wait at least 15 seconds between `06-bulk` and `07-remove`, so Maple is no longer in use when **Remove** runs (15
  seconds is a margin chosen to be safe, not a value in the source).
- One clip again: restore the snapshot, start the site, and bring it to the state the clip expects. For `01-list`,
  `05-reload`, `06-bulk`: nothing to do. For `03-edit` and `04-disable`: nothing to do. For `07-remove`: first disable
  Maple outside the capture (**Disable**, then **Ok**), then wait at least 15 seconds before recording, so the
  disabled tenant is no longer in use (`OrchardCore/OrchardCore/Shell/Removing/ShellRemovalManager.cs:45-50` refuses to remove a tenant that is still active).
- Every clip opens its first page with `scene.OpenAsync` (not recorded), and `StartCaptureAsync` follows.
- Leave every group of the admin menu closed before the capture. Only `list-1` opens one, on camera.

## 4. The clips

Common selectors, all verified in the views listed in section 5 (the row of a tenant is the list item that contains its
name, and the toolbar is also a list item, so always filter by text):

- `row(name)`: `Page.Locator("ul.admin-list li.list-group-item", new() { HasText = name })`
- buttons inside a row: `a.btn.edit`, `a.btn.view`, `a.btn.disable`, `a.btn.enable`, `a.btn.setup`, `a.btn.remove`,
  `a.btn.reload`; the actions area is `.admin-list-actions`
- the **Features** button of a running tenant also carries the classes `btn` and `enable`
  (`ManageFeaturesActionButton.cshtml:6`), so `a.btn.enable` matches it too: use `a.btn.enable` only on a row whose
  tenant is Disabled, where **Features** is not drawn (`disable-4` does), never on a running tenant;
- state badge: `a[href*="Options.Status"]`
- confirmation dialog: `#confirmRemoveModal` (title `.modal-title`, buttons `#modalOkButton` and `#modalCancelButton`)
- notification: `.toast.message-success`

### Clip `01-list` (chapter 01, "The Tenants list")

Starts on `/Admin` (opened before the capture). The admin sidebar is visible and in its normal, expanded width, with
the **Multi-Tenancy** group closed: do not hide the sidebar or switch it to icons only.

- `list-1`: click **Multi-Tenancy** in `#left-nav` (it is a button that opens its group), then click **Tenants** and
  wait for the navigation to `/Admin/Tenants` (`ClickAndWaitForNavigationAsync`). Then highlight the badges of the
  Harbor row (`row("Harbor")` badges: the `.tags` containers). Visible when the narration ends: the page titled
  **Tenants** with the six rows, and the ring around the database, recipe and state badges of Harbor (`Sqlite`, `Blog`,
  **Running**). The Default row has `Sqlite` and **Running** and no recipe badge, because Auto Setup stores a recipe
  name only for the tenants it creates besides Default (`AutoSetupService.cs:70-76`; nothing else writes
  `RecipeName` for Default): this is why the narration says "when they are set". Do not highlight the Default row here. Labels: `OrchardCore.Modules/OrchardCore.Tenants/AdminMenu.cs:30-34`; `Permissions.cs:8` ("Manage tenants");
  `Views/Admin/Index.cshtml:9`; badges `Views/ShellSettingsEntry.Database.SummaryAdmin.cshtml:8-9`,
  `ShellSettingsEntry.Recipe.SummaryAdmin.cshtml:8-9`, `ShellSettingsEntry.State.SummaryAdmin.cshtml:12-24`.
- `list-2`: clear the highlights, highlight `row("Harbor").Locator(".admin-list-actions")` (**Edit**, **View**,
  **Disable**, **Reload**, **Features**), wait about 3 seconds, clear, then highlight
  `row("Default").Locator(".admin-list-actions")` (**Edit**, **View**, **Reload**). Visible at the end: the ring around
  the buttons of the Default row, with no **Disable** among them. Labels: `ManageTenantActionButtons.cshtml:9,12,16,30`,
  `ManageFeaturesActionButton.cshtml:6`; the Default row has no **Disable** because of
  `ManageTenantActionButtons.cshtml:14`.
- `list-3`: clear the highlights. `TypeAsync(Page.Locator("#search-box"), "harbor", 90)`, then press Enter inside
  `WaitForNavigationAsync(() => Page.Keyboard.PressAsync("Enter"))` (the first submit button of the form is the
  hidden `submit.Filter`, so Enter filters: `Views/Admin/Index.cshtml:13`). Visible at the end: only the Harbor row, and
  `harbor` in the search box. Label: placeholder **Search**, `Index.cshtml:22`; the search covers the name, host and
  prefix: `AdminController.cs:118-124`.
- `list-4`: `NavigateAsync("/Admin/Tenants")` to clear the search (it keeps filtering otherwise). Click the **State**
  picker (a bootstrap-select button whose title is **State**: `Views/TenantsFilters.cshtml:13`, confirm the selector
  with `Recorder probe`), then click **Uninitialized** in the list it opens. The choice submits by itself
  (`changed.bs.select` clicks `submit.Filter`): wait for the navigation. If the picker cannot be driven, use
  `NavigateAsync("/Admin/Tenants?Options.Status=Uninitialized")` instead. Visible at the end: only the Pilot row, with
  the badges `Sqlite`, `Blog`, **Uninitialized**, and the buttons **Edit**, **Setup**, **Remove**, **Reload**. Labels:
  `AdminController.cs:204-210` (**All states**, **Running**, **Disabled**, **Uninitialized**), `TenantsFilters.cshtml:13`.

### Slide chapter `02-states` (three slides, from `slides.py`)

- `states-1`: four cards, "Uninitialized" and "Running" bright, the other two dimmed.
- `states-2`: the same cards, "Disabled" and "Initializing" bright. The state **Invalid** exists (`TenantState.cs`, and a
  badge in `ShellSettingsEntry.State.SummaryAdmin.cshtml:23-25`) but nothing in the source sets it, so the video does
  not mention it (`OPEN-QUESTIONS.md`, 12).
- `states-3`: the same cards, all bright.

### Clip `03-edit` (chapter 03, "Edit a tenant")

Starts on `/Admin/Tenants`. Changes the site.

- `edit-1`: click `row("Acme").Locator("a.btn.edit")` with `ClickAndWaitForNavigationAsync`. Highlight `#Name`
  (read-only). Visible at the end: the **Edit Tenant** page of Acme with **Name** (`Acme`, read-only), **Category**,
  **Url Prefix** (`acme`), **Description** and **Hostname**, and the **Save** and **Cancel** buttons, with no Recipe
  and no database fields. Labels: `Views/Admin/Edit.cshtml:5-6,14,16`, `Views/Shared/_TenantInfo.cshtml:4,12,21,30`.
- `edit-2`: clear the highlights. Highlight `#RequestUrlPrefix`, wait about 2 seconds, clear, then highlight
  `#RequestUrlHost`. **Do not type in either field**: a host such as `acme.example.com` would make the tenant
  unreachable at `/acme` for the rest of the take. Visible at the end: the ring on **Hostname** with its two hints
  (the example and "You may define multiple domains using the comma (,) as a separator..."). Labels:
  `_TenantInfo.cshtml:16,34-35`.
- `edit-3`: clear the highlights. `TypeAsync(Page.Locator("#Category"), "Customers", 60)` then
  `TypeAsync(Page.Locator("#Description"), "Acme Bakery storefront", 40)`. Visible at the end: both values in their
  fields. Labels: `_TenantInfo.cshtml:4,21`.
- `edit-4`: click `button.save` (**Save**) with `ClickAndWaitForNavigationAsync`. Highlight `row("Acme")`. Visible at
  the end: back on `/Admin/Tenants`, the Acme row showing the tag **Customers** next to its other badges and
  **Acme Bakery storefront** under its name. Save posts the form, which updates and reloads the tenant
  (`AdminController.cs:556-580`, `OrchardCore/OrchardCore/Shell/ShellHost.cs:152-159`). Labels: `Edit.cshtml:37`,
  `ShellSettingsEntry.Category.SummaryAdmin.cshtml:8-9`, `ShellSettingsEntry.Description.SummaryAdmin.cshtml:13-16`.
- `edit-5`: clear the highlights. Click `row("Pilot").Locator("a.btn.edit")` with `ClickAndWaitForNavigationAsync`,
  then highlight `#RecipeName`, and after about 2 seconds `#DatabaseProvider`. **Do not save.** Visible at the end:
  the Edit Tenant page of Pilot with, besides the fields of `edit-1`, **Recipe** (`Blog`), the heading
  **Database Presets** and **What type of database to use?** (`Sqlite`). Labels: `_TenantRecipes.cshtml:4`,
  `_TenantDatabaseInfo.cshtml:18,26,35`; these fields exist only while the tenant is uninitialized:
  `AdminController.cs:493-509`.

### Clip `04-disable` (chapter 04, "Disable and enable a tenant")

Starts on `/Admin/Tenants`. Ends with Harbor Running again.

- `disable-1`: click `row("Harbor").Locator("a.btn.disable")`, then wait for `#confirmRemoveModal` to be shown.
  Visible at the end: the dialog titled **Disable** with "Are you sure you want to disable this tenant?" and the
  buttons **Ok** and **Cancel**. Labels: `ManageTenantActionButtons.cshtml:16`; the dialog is drawn by
  `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-66,171-180`.
- `disable-2`: click `#modalOkButton` with `ClickAndWaitForNavigationAsync`; highlight the state badge of
  `row("Harbor")`. Visible at the end: Harbor with the badge **Disabled**, and no message (a successful disable shows
  none: `AdminController.cs:643-645`). Label: `ShellSettingsEntry.State.SummaryAdmin.cshtml:21`.
- `disable-3`: clear the highlights, highlight `row("Harbor").Locator(".admin-list-actions")`. Visible at the end:
  the ring around **Edit**, **Enable**, **Remove**, **Reload** (no **View**, no **Disable**, no **Features**). Labels:
  `ManageTenantActionButtons.cshtml:9,20,28,30`.
- `disable-4`: clear the highlights, click `row("Harbor").Locator("a.btn.enable")` with `ClickAndWaitForNavigationAsync`
  (no dialog: the link is only an `UnsafeUrl`, `TheAdmin.ts:118-185`), then highlight the state badge. Visible at the
  end: Harbor with the badge **Running** and the buttons **Edit**, **View**, **Disable**, **Reload**, **Features** again.
  Label: `ManageTenantActionButtons.cshtml:20`.

### Clip `05-reload` (chapter 05, "Reload a tenant")

Starts on `/Admin/Tenants`.

- `reload-1`: highlight `row("Harbor").Locator("a.btn.reload")`. Visible at the end: the ring around the **Reload**
  button of Harbor. Label: `ManageTenantActionButtons.cshtml:30`.
- `reload-2`: click that button with `ClickAndWaitForNavigationAsync` (no dialog: `UnsafeUrl` only). Reloading a tenant
  can take a moment; wait for the heading **Tenants** and for the Harbor row. Visible at the end: `/Admin/Tenants`
  again, the same six rows, no message and no dialog. Behavior: `AdminController.cs:677-704`.
- `reload-3`: clear the highlights; nothing to do on screen. Visible at the end: the same list, unchanged.

### Clip `06-bulk` (chapter 06, "Several tenants at once")

Starts on `/Admin/Tenants`. Changes the site.

- `bulk-1`: click `Page.Locator("#tenantNames-Juniper")`, then `Page.Locator("#tenantNames-Maple")` (the checkboxes are
  `input#tenantNames-<Name>`, `ShellSettingsEntry.Checkbox.SummaryAdmin.cshtml:7`; click the input itself, not its
  `label`, which is empty and has no size, so the recorder would wait for it forever). Visible at the end: both checkboxes ticked and the
  toolbar showing **2 selected** and the **Actions** button, with the Category, State and Sort filters hidden. If
  only one is ticked the **Actions** button stays hidden: this is the behavior of the script
  (`wwwroot/Scripts/tenants-admin-lists/tenants-admin-list.js`, the bulk select list component), so tick two.
  Labels: `Views/ShellSettingsEntry.Checkbox.SummaryAdmin.cshtml:7`; `OrchardCore.Modules/OrchardCore.Admin/Views/AdminListToolbar.cshtml:63-67`
  (**Actions**); the word "selected": `Views/Admin/Index.cshtml:12`.
- `bulk-2`: click `#bulk-action-menu-button` (**Actions**), then `a.dropdown-item[data-action="Disable"]`
  (**Disable**), wait for `#confirmRemoveModal`, then click `#modalOkButton` with `ClickAndWaitForNavigationAsync`.
  Keep the pauses short: the narration is about 9 seconds. The dialog is titled **Bulk Action** and reads "Are you
  sure you want to disable these items?". Visible at the end: Juniper and Maple with the badge **Disabled**, Acme,
  Harbor and Default still **Running**, Pilot **Uninitialized**. Labels: `AdminListToolbar.cshtml:73`,
  `AdminController.cs:218-222,283-297`.
- `bulk-3`: highlight `row("Default").Locator(".admin-list-actions")`. Visible at the end: the ring around the buttons
  of the Default row (no **Disable**). Behavior: `AdminController.cs:284-296` (warnings "You cannot disable the default
  tenant." and "The tenant 'X' is already disabled." for the cases the narration mentions; they are not shown). The
  rule is: **Disable** skips Default, then every tenant that is not Running (Uninitialized, Disabled, Initializing),
  and **Enable** skips every tenant that is not Disabled (`AdminController.cs:283-309`). Never tick Pilot in a take: it
  would be skipped with the warning "The tenant 'Pilot' is already disabled.", which is wrong for an Uninitialized
  tenant.

### Slide chapter `07-rules` (two slides)

- `rules-1`: three cards, "Off by default" bright.
- `rules-2`: the same cards, "Disabled or not set up" and "Deleted for good" bright.

### Clip `07-remove` (chapter 07, "Remove a tenant")

Starts on `/Admin/Tenants`, after `06-bulk` (Maple is Disabled). Changes the site.

- `remove-1`: click `row("Maple").Locator("a.btn.remove")`, then wait for `#confirmRemoveModal`. Visible at the end: the
  dialog titled **Remove** with "Are you sure you want to remove this tenant?" and the buttons **Ok** and **Cancel**.
  Labels: `ManageTenantActionButtons.cshtml:28`.
- `remove-2`: click `#modalOkButton` with `ClickAndWaitForNavigationAsync`. Removing a disabled tenant builds a shell
  with every installed feature and deletes its database, files and settings, so it can take several seconds: time it
  once outside the capture, and if it takes more than 3 seconds, put the wait for `.toast.message-success` inside
  `OffCameraAsync`, so the capture resumes as soon as the toast is drawn. The success toast "The tenant 'Maple' was
  removed, see the log file for more info." hides itself 5 seconds after it is drawn (`SuccessAsync` sets a dismissal of
  5000 ms: `OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`; the toast has
  `data-bs-autohide="true"`: `OrchardCore.Themes/TheAdmin/Views/Message.cshtml`), so it does not stay until closed.
  The sentence of the narration that talks about the message is the first one after "Select Ok.": keep the actions
  short and do not hold the toast open. Visible at the end of the narration: a list with Acme, Default, Harbor, Juniper
  and Pilot, no Maple, and probably no toast any more; check the contact sheet for one frame that shows the toast and
  the list together, and for none that shows a Maple row. Label: `AdminController.cs:742`; what is deleted:
  `OrchardCore/OrchardCore/Shell/Removing/` (`ShellRemovalManager.cs`, `ShellSiteFolderRemovingHandler.cs`,
  `ShellWebRootRemovingHandler.cs`, `ShellSettingsRemovingHandler.cs`) and
  `OrchardCore/OrchardCore.Data.YesSql/Removing/ShellDbTablesRemovingHandler.cs`.

## 5. Where the labels come from

`OrchardCore.Modules/OrchardCore.Tenants/`: `AdminMenu.cs`, `Permissions.cs`, `Views/Admin/Index.cshtml`,
`Views/Admin/Edit.cshtml`, `Views/Shared/_TenantInfo.cshtml`, `_TenantRecipes.cshtml`, `_TenantDatabaseInfo.cshtml`,
`Views/ManageTenantActionButtons.cshtml`, `Views/ManageFeaturesActionButton.cshtml`, `Views/TenantsFilters.cshtml`,
`Views/ShellSettingsEntry.*.SummaryAdmin.cshtml`, `Controllers/AdminController.cs`. Shared: `OrchardCore.Admin/Views/
AdminListToolbar.cshtml`, `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99` (the dialog's defaults: **Ok**,
**Cancel**), `TheAdmin/Assets/js/TheAdmin/TheAdmin.ts`.

## 6. Checks for private data

- The only account is `admin` / `admin@example.com`; no screen of this video shows an email address, a password, a
  token or a path of this machine. The sign-in is in `state.json`, which stays out of git.
- The tenants, their names, prefixes and the description typed in `edit-3` are invented.
- The hints of the **Url Prefix** and **Hostname** fields, shown in `edit-1` and `edit-2`, are text of the product
  that names its own example domains (`_TenantInfo.cshtml:16,34`). They are not data of the site: let the OCR search
  of the next check ignore them, and do not type a domain from them in a field.
- Before publishing, OCR the video at one frame per second and at every scene change, and search for the name of the
  machine, the user profile folder, `localhost` ports other than 5010, and any address that is not `example.com`.
- Look at every contact sheet: no error page, no stale toast from an earlier clip (a toast lasts 5 seconds, so only the
  one of `remove-2` should ever appear), no tenant in a state the step does
  not expect (a stale **Disabled** badge, a missing row).

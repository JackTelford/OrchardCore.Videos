# Recording plan: Deployment overview

This file is for the person who records the video. It says what the site must hold, in which order to record the
clips, and what to do in every step. Selectors below stand for `scene.Page.Locator(...)`, since the recorder's methods
take locators. File references are to the Orchard Core source at the commit the catalog was read from
(`src/OrchardCore.Modules/...`). The video shows the admin UI only: the commands and the environment variables of this
file stay off camera.

The narration of every step is in `storyboard.json`; narrate before recording (`narrate.py`), so each step is held for
the length of its narration.

## 1. The site it needs

The video moves one setting between two sites. Both sites run in one process of `src/OrchardCore.Cms.Web`, as two
tenants: the Default tenant is the **source site**, and a tenant named Staging, with the URL prefix `staging`, is the
**destination site**.

Run the site on port 5010 with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`), and block
outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, as `references/recording.md` of the skill
says. Provision both tenants with Auto Setup (docs: `src/docs/reference/modules/AutoSetup/README.md`), so no setup or
sign-in form is recorded. Use `<password>`, the same value as `VIDEO_PASSWORD`, outside git, for both administrators:

| Environment variable | Default tenant (source) | Staging tenant (destination) |
|---|---|---|
| `OrchardCore__AutoSetup__Tenants__<n>__ShellName` | `Default` (n = 0) | `Staging` (n = 1) |
| `...__SiteName` | `Orchard Core` | `Orchard Core Staging` |
| `...__SiteTimeZone` | `America/Los_Angeles` | `America/Los_Angeles` |
| `...__AdminUsername` | `admin` | `admin` |
| `...__AdminEmail` | `admin@example.com` | `admin@example.com` |
| `...__AdminPassword` | `<password>` | `<password>` |
| `...__DatabaseProvider` | `Sqlite` | `Sqlite` |
| `...__DatabaseTablePrefix` | empty | `staging` |
| `...__RecipeName` | `Blog` | `Blog` |
| `...__RequestUrlPrefix` | empty | `staging` |

The Default tenant is set up by its first request (`http://localhost:5010/`); Auto Setup then creates the settings of
the Staging tenant, and its first request (`http://localhost:5010/staging`) sets it up. Wait until both show the blog
home page.

**Features.** Nothing has to be enabled. The Blog recipe enables `OrchardCore.Deployment` and
`OrchardCore.Deployment.Remote` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:57-58`) on both
tenants. Check on Tools, Features of each site that **Deployment** is Enabled.

**Seed data** (made off camera, once, with a browser that is signed in; use the Settings page, not a script):

1. Source site, Settings, General (`/Admin/Settings/general`): **Page size** = `25`, Save.
2. Staging site, Settings, General (`/staging/Admin/Settings/general`): **Page size** = `10` (set it and save it even
   if it already reads 10, so the value is explicit).
3. Neither site has a deployment plan (the Plans page reads "Nothing here! There are no deployment plans at the moment.").
4. No other content, user or setting is changed. All the data is the fake data of the recipe, `admin` and
   `admin@example.com`.

**Sign-ins.** The sign-in cookie of each tenant is named for the tenant, so the Staging tenant needs a session of its
own. `Recorder setup` signs in at `VIDEO_BASE_URL` plus `/Login` (the fields `#LoginForm_UserName` and
`#LoginForm_Password` of `Program.cs` `SetupAsync`) and always writes `state.json`, so run it twice: first with
`VIDEO_BASE_URL=http://localhost:5010/staging`, then rename the resulting `state.json` to `state-staging.json`; then
with `VIDEO_BASE_URL=http://localhost:5010`, which leaves the `state.json` of the source site. Both files sit in
`VIDEO_ROOT` (`.gitignore` ignores `state*.json`). Clip `04-import` starts its scene (`Scene.StartAsync`) with the base URL `http://localhost:5010/staging` and
`state-staging.json`; every path in that clip is relative to that base, because `OpenAsync` and `NavigateAsync` add the
path to the base URL (write `/Admin/Settings/general`, never `/staging/Admin/Settings/general`). Clips `02-find` and
`03-plan` use the base URL `http://localhost:5010` and `state.json`.

**The package file.** Clip `03-plan` saves the downloaded plan as `StagingSetup.zip` in a folder outside the repository
(`VIDEO_PACKAGE_DIR`, for example under the system temp folder). Clip `04-import` uploads that file. It is never
committed.

**Snapshot.** With the sites seeded and signed in, stop the site, copy `App_Data` and the two state files aside, and
start the site again. Restore the copy before every full take.

## 2. Order of the clips

| Order | Clip | Site | Needs | Changes the site |
|---|---|---|---|---|
| 1 | `02-find` | source | seeded snapshot | no |
| 2 | `03-plan` | source | seeded snapshot, no plan | yes: creates the plan Staging Setup with a Site Settings step, and writes `StagingSetup.zip` |
| 3 | `04-import` | Staging | seeded snapshot, `StagingSetup.zip` from `03-plan` | yes: Page size of the Staging tenant becomes 25 |

The slide clips (`01-why`, `05-permissions`) and the two cards need no recording.

To record a clip again, stop the site, restore the snapshot, start it, and record. `04-import` can be recorded again
from a restored snapshot with the `StagingSetup.zip` of an earlier take of `03-plan`, since the file does not depend
on the state of the site; `03-plan` overwrites the file.

## 3. Clips

Every locator below that selects by text (`HasText`) must match the whole text of the element: Playwright's `HasText`
matches a part of the text, without regard to case, so use an anchored pattern such as `^Settings$`, `^General$`,
`^Tools$` or `^Add$` where the text is short, or the call fails with more than one match. The step ids are the ones of
`storyboard.json`; the narration of each step is there, and every label it names is in the lists of labels below.

### Chapter 01, `01-why` (slides)

- `why-1`: the slide "Moving work between sites by hand", three cards: Built on one site, Needed on another site, Repeated by hand.
- `why-2`: the slide "Three stages", three cards: Build a plan, Execute the plan, Import the package.
- `why-3`: the slide "What a package holds", three cards: A zip file, A recipe, Run in order.

### Chapter 02, `02-find` (recording, source site)

Starts on `/Admin/Features` (`scene.OpenAsync`, not recorded). Before `StartCaptureAsync`, close the **Deployments**
group of the left menu if it is open (click its label when `aria-expanded` is `true`), so that `find-2` shows it
opening. The menu items are `button.item-label` for a group and `a.item-label` for a page
(`src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24-86`).

- `find-1`
  - Actions: type `Deployment` into `#search-box` (`TypeAsync`, delay 90). Wait until `#btn-disable-OrchardCore_Deployment`
    is visible, then highlight the row that holds it (`.list-group-item` with `#btn-disable-OrchardCore_Deployment`).
    Hold on the result while the second sentence plays.
  - On screen at the end of the narration: the search box reads Deployment; under the heading **Deployment** the row
    **Deployment** `OrchardCore.Deployment` shows the badge **Enabled** and a **Disable** button, ringed; the row
    **Remote Deployment** is below it. Other features whose text mentions deployment may be listed under other
    headings; do not highlight them.
  - Labels: search box `OrchardCore.Features/Views/Admin/Features.cshtml:27`; category heading `:88`; name and id
    `Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`; **Enabled** `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`;
    **Enable** and **Disable** `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40,53`; the page is under Tools, Features
    (`OrchardCore.Features/AdminMenu.cs:26-27`); the Blog recipe enables the feature (`blog.recipe.json:57`).

- `find-2`
  - Actions: clear the highlights. Open the menu: if the group **Tools** (`button.item-label` with the text `Tools`)
    has `aria-expanded="false"`, click it; then click the group **Deployments** (`button.item-label` with the text
    `Deployments`) and wait until `a.item-label` with the text `Plans` is visible. Highlight, one after the other and
    in the order of the narration, with about one second on each: **Plans**, **Package Import**, **JSON Import**, then
    **Remote Instances** and **Remote Clients**. Leave the last ring on screen.
  - On screen at the end of the narration: the left menu with the group Deployments open and its five pages listed,
    the last two ringed. The narration does not claim an order for the pages; the order on screen is whatever the menu
    shows.
  - Labels: `OrchardCore.Deployment/AdminMenu.cs:18-33` (Tools, Deployments, Plans, Package Import, JSON Import);
    `OrchardCore.Deployment.Remote/AdminMenu.cs:17-30` (Remote Instances, Remote Clients).

### Chapter 03, `03-plan` (recording, source site)

Starts on `/Admin/Settings/general` (`scene.OpenAsync`, not recorded), the **Site** tab, which is the first tab. The
administrator that is signed in is `admin`, and the top bar of the admin shows the site name **Orchard Core**
(`OrchardCore.Admin/Views/AdminBranding.cshtml:8`).

- `plan-1`
  - Actions: highlight the field **Page size** (`.ocat-limited-wrapper` with a `label` whose text is `Page size`).
    Nothing else; hold.
  - On screen at the end of the narration: the General settings of the source site with the field **Page size**
    ringed and reading 25.
  - Labels: **Page size** `OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:54`; the General page is under
    Settings, General (`OrchardCore.Settings/AdminMenu.cs:36-39`); the Site tab `Drivers/DefaultSiteSettingsDisplayDriver.cs:51`.

- `plan-2`
  - Actions: clear the highlights; `scene.NavigateAsync("/Admin/DeploymentPlan/Index")`; hold about five seconds on the
    empty list while the first sentence plays; then `ClickAndWaitForNavigationAsync` on `a.create` with the text
    `Add Deployment Plan`.
  - On screen at the end of the narration: the page **Create Deployment Plan** with the empty field **Name** and the
    buttons **Create** and **Cancel**.
  - Labels: the empty list "Nothing here! There are no deployment plans at the moment." and **Add Deployment Plan**
    `OrchardCore.Deployment/Views/DeploymentPlan/Index.cshtml:25,34`; the page title `Views/DeploymentPlan/Create.cshtml:4`.

- `plan-3`
  - Actions: `TypeAsync` on `#Name` with `Staging Setup`; `ClickAndWaitForNavigationAsync` on `button.create` with the
    text `Create`.
  - On screen at the end of the narration: the page of the plan Staging Setup, with the buttons **Execute** (green) and
    **Add Step**, and the message "Nothing here! There are no steps on this plan for the moment."
  - Labels: **Name**, **Create** `Views/DeploymentPlan/Create.cshtml:12,20`; **Execute**, **Add Step**, the empty
    message `Views/DeploymentPlan/Display.cshtml:25,31,57`; the redirect to the plan page `Controllers/DeploymentPlanController.cs:268`.

- `plan-4`
  - Actions: click the button `Add Step`; wait until `#modalSteps` is shown. Highlight the list `#deployment-step-categories`
    and hold.
  - On screen at the end of the narration: the dialog **Available Steps** with the field **Filter**, the category
    list on the left (**All** active, then **Configuration**, **Content Management** and **Deployment** among
    others), and the cards of the steps, each with a description and an **Add** button.
  - Labels: **Available Steps**, **Filter**, **All** `Views/DeploymentPlan/Display.cshtml:68,75,79`; the categories are
    read from the steps (`OrchardCore.Deployment/Controllers/DeploymentPlanController.cs:191-221`): Configuration and
    Content Management (`src/OrchardCore/OrchardCore.Abstractions/Localization/CommonLocalizationSources.cs:17,23`), and
    Deployment (`OrchardCore.Deployment/DeploymentLocalizationSources.cs:17`).

- `plan-5`
  - Actions: clear the highlights; click `#deployment-step-categories .nav-link` with the text `Configuration`; wait
    until the card `.deployment-step-item` that holds `h4.card-title` with the text `Site Settings` is visible and
    highlight it; then `ClickAndWaitForNavigationAsync` on the `a` with the text `Add` inside that card.
  - On screen at the end of the narration: the page titled **Site Settings**, with the breadcrumb Deployment Plans,
    Staging Setup, Site Settings, the hint "The site settings to add as part of the plan.", and a list of check boxes.
  - Labels: the card **Site Settings** and "Exports the current site settings."
    `OrchardCore.Settings/Views/Items/SiteSettingsDeploymentStep.Fields.Thumbnail.cshtml:3-4`; **Add**
    `OrchardCore.Deployment/Views/DeploymentStep.Thumbnail.cshtml:8`; the breadcrumb `OrchardCore.Deployment/Views/Step/Create.cshtml:38-41`;
    the hint `OrchardCore.Settings/Views/Items/SiteSettingsDeploymentStep.Fields.Edit.cshtml:10`.

- `plan-6`
  - Actions: click the check box whose value is `PageSize` (`input[type=checkbox][value="PageSize"]`, with the label
    `PageSize`); check that it is checked and that no other box is. Then `ClickAndWaitForNavigationAsync` on
    `button.create` with the text `Create`. Locate the box by its value, not by its position.
  - On screen at the end of the narration: the page of the plan, with one step in the list: the heading **Site
    Settings**, the badge `PageSize`, the buttons **Edit** and **Delete**, and the Execute and Add Step buttons above it.
    The green message "Deployment plan step added successfully." may still be on screen.
  - Labels: the check boxes are the property names of the settings, so **PageSize** is written without a space
    (`SiteSettingsDeploymentStep.Fields.Edit.cshtml:34-35`); **Site Settings** and the badge `Fields.Summary.cshtml:7,13`;
    **Edit**, **Delete** `OrchardCore.Deployment/Views/DeploymentStep.Summary.cshtml:12,20`; **Create**
    `Views/Step/Create.cshtml:53`. Tick only this box: the export fails for a name it does not support.

- `plan-7`
  - Actions: click the button `Execute`; wait until `#modalTargets` is shown. Highlight the card that holds `h4.card-title`
    with the text `File Download` and hold. Near the end of the narration (during the last sentence) click the `a`
    with the text `Select` inside that card, wrapped in `scene.Page.RunAndWaitForDownloadAsync(...)`, and save the
    download to `VIDEO_PACKAGE_DIR` (`download.SaveAsAsync`). Check that the saved file is `StagingSetup.zip` and is
    not empty.
  - On screen at the end of the narration: the dialog **Available Targets** with one card, **File Download**, its
    description "Download a deployment plan locally." and its **Select** button. If a remote instance is configured, a
    second card appears; none is configured on this site.
  - Labels: **Execute** `Views/DeploymentPlan/Display.cshtml:25`; **Available Targets**, **Select** `:115,132`;
    **File Download** and its description `OrchardCore.Deployment/Services/FileDownloadDeploymentTargetProvider.cs:20-21`;
    the targets of remote instances `OrchardCore.Deployment.Remote/Services/RemoteInstanceDeploymentTargetProvider.cs:20-35`.
    The link becomes a form that posts (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:114-187`), and the
    answer is a file, so the page does not change.

- `plan-8`
  - Actions: highlight the File Download card with the caption `StagingSetup.zip` (the file name the code gives,
    `OrchardCore.Deployment/Controllers/ExportFileController.cs:67`: the plan name without spaces, plus `.zip`; use
    `download.SuggestedFilename` as the caption if it differs). Hold.
  - On screen at the end of the narration: the same dialog, with the ring and the caption `StagingSetup.zip`. A
    headless browser draws no download bar, so the download itself is never on screen; the narration only says that
    the browser downloads a zip file.
  - Labels: as `plan-7`.

### Chapter 04, `04-import` (recording, Staging site)

The scene's base URL is `http://localhost:5010/staging` and its session is `state-staging.json`; every path below is
relative to that base. It starts with `scene.OpenAsync("/Admin/Settings/general")` (not recorded), which is
`http://localhost:5010/staging/Admin/Settings/general`. The top bar shows the site name **Orchard Core Staging**.
Before `StartCaptureAsync`, close the Deployments and Tools groups of the left menu if they are open.

- `import-1`
  - Actions: highlight the field **Page size** (as in `plan-1`). Hold.
  - On screen at the end of the narration: the General settings of the Staging site, **Page size** ringed and reading
    10, and the top bar reading Orchard Core Staging.
  - Labels: as `plan-1`; the site name `OrchardCore.Admin/Views/AdminBranding.cshtml:8`; the Blog recipe enables the
    Deployment feature on this site too (`blog.recipe.json:57`).

- `import-2`
  - Actions: clear the highlights. In the left menu click **Tools**, then **Deployments**, each only if its
    `aria-expanded` is `false`, then `ClickAndWaitForNavigationAsync` on `a.item-label` with the text `Package Import`.
    On the page, set the file that `plan-7` saved in `VIDEO_PACKAGE_DIR` (`StagingSetup.zip`, unless the suggested name
    differed): `scene.Page.Locator("#file").SetInputFilesAsync(path)`. Hold.
  - On screen at the end of the narration: the page **Import Deployment Package**, with the label **Select a file**,
    the field showing the file name that `plan-7` saved (`StagingSetup.zip`), and the button **Import**.
  - Labels: `OrchardCore.Deployment/Views/Import/Index.cshtml:2,8,13`; the accepted extensions (`.zip` and `.json`)
    `:9`; the menu `OrchardCore.Deployment/AdminMenu.cs:18-28`.

- `import-3`
  - Actions: hold on the page, then click `button.import` with the text `Import` (a form post that redirects to the
    same page; use `WaitForNavigationAsync`) at the moment that is the duration of `import-3` in `durations.json` minus
    4 seconds. The message stays five seconds from when it appears
    (`OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`) and the reload adds one to three seconds, so it is
    still on screen when the narration ends.
  - On screen at the end of the narration: the page **Import Deployment Package** with the green message
    "Deployment package imported." in the top right corner.
  - Labels: `OrchardCore.Deployment/Controllers/ImportController.cs:117`. The package runs through the recipe executor
    and the tenant reloads before the redirect (`OrchardCore.Recipes/Services/RecipeDeploymentTargetHandler.cs:46`), so
    the first page after the click can take a few seconds: wait for it with `WaitForNavigationAsync`.

- `import-4`
  - Actions: in the left menu click **Settings** (only if its `aria-expanded` is `false`), then
    `ClickAndWaitForNavigationAsync` on `a.item-label` with the text `General`. Highlight the field **Page size**. Hold.
  - On screen at the end of the narration: the General settings of the Staging site, **Page size** ringed and reading
    25, and the top bar still reading Orchard Core Staging.
  - Labels: `OrchardCore.Settings/AdminMenu.cs:36-39`; **Page size** `Views/Settings-Site.Edit.cshtml:54`.

### Chapter 05, `05-permissions` (slides)

- `perm-1`: the slide "Three permissions", three cards: Manage deployment plans, Export Data, Import Data
  (`src/OrchardCore/OrchardCore.Deployment.Core/DeploymentPermissions.cs:8-12`; the three are the ones the module
  grants to administrators, `OrchardCore.Deployment/Permissions.cs:7-12`). The first card also says that the Plans
  page needs Export Data: the Plans menu item requires Export Data (`OrchardCore.Deployment/AdminMenu.cs:22`) and
  the list requires Manage deployment plans and Export Data (`Controllers/DeploymentPlanController.cs:71-79`).
- `perm-2`: the slide "Import Data is Security Critical", four cards: Security Critical badge, A recipe changes the
  site, Grant it with care, Import with care (the badge: `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:122-126`).

## 4. Checks for private data

- Only the fake data of this plan appears: `admin`, `admin@example.com`, the site names Orchard Core and Orchard Core
  Staging, the plan name Staging Setup, the file name `StagingSetup.zip`. The password is never typed on camera: both
  sessions come from `state.json` and `state-staging.json`.
- The Package Import field shows the file name only, never the folder of `VIDEO_PACKAGE_DIR`; check the frame after
  `import-2` for a path.
- No address bar is recorded, so the port and the prefix are not visible; check that no frame shows a machine name or
  a real domain (the Base url field of the General settings is empty).
- Look at every contact sheet (`review.py`) and OCR the video, at one frame per second and at every scene change,
  for names, emails and paths other than the ones above.
- Do not commit the MP4s, the audio, `state.json`, `state-staging.json`, `StagingSetup.zip`, or a copy of `App_Data`.

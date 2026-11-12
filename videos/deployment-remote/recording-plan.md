# Recording plan: Remote Deployment

How to record `deployment-remote` without having to decide anything. In this plan a selector stands for
`scene.Page.Locator(...)`. Paths of the form `Module/File:line` are in the Orchard Core source the labels were read
from (`src/OrchardCore.Modules/` unless stated otherwise); `references/recording.md` of the skill has the general rules.

The video shows **two sites**. The *source* is the Default tenant, called **Orchard Core**. The *destination* is a
second tenant of the same installation, called **Staging Site**, reached under the URL prefix `staging`
(the stand-in from the tracker: a second local tenant is the remote instance). The viewer tells them apart by the
site name at the top left of the admin bar (`OrchardCore.Admin/Views/AdminBranding.cshtml:8`).

## The site it needs

One local Orchard Core host (`src/OrchardCore.Cms.Web`), on `http://localhost:5010` (or `VIDEO_BASE_URL`), with its
own `ORCHARD_APP_DATA` folder outside both repositories. Both tenants are provisioned by Auto Setup from environment
variables (`OrchardCore.AutoSetup/Startup.cs:26`, the format is in `src/docs/reference/modules/AutoSetup/README.md`).
Auto Setup installs the Default tenant on its first request, creates the other tenants uninstalled, and installs each
of them on its first request (`OrchardCore.AutoSetup/AutoSetupMiddleware.cs:133-141`).

| Variable | Default tenant (source) | `Staging` tenant (destination) |
|---|---|---|
| `OrchardCore__AutoSetup__Tenants__N__ShellName` | `Default` (N = 0) | `Staging` (N = 1) |
| `...__SiteName` | `Orchard Core` | `Staging Site` |
| `...__SiteTimeZone` | `America/Los_Angeles` | `America/Los_Angeles` |
| `...__AdminUsername` | `admin` | `admin` |
| `...__AdminEmail` | `admin@example.com` | `admin@example.com` |
| `...__AdminPassword` | `<password>` (`VIDEO_PASSWORD`, chosen outside git) | the same `<password>` |
| `...__DatabaseProvider` | `Sqlite` | `Sqlite` |
| `...__DatabaseTablePrefix` | empty | `staging` |
| `...__RecipeName` | `Blog` | `Blog` |
| `...__RequestUrlPrefix` | empty | `staging` |

- The Blog recipe enables **Deployment** and **Remote Deployment** on both tenants
  (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). Check it on each tenant
  (Tools, Features, search `Deployment`): both rows must show a **Disable** button. If **Remote Deployment** shows
  **Enable**, enable it outside the capture. No other feature is needed.
- Settings and external services: none. No setting is saved before the recording, and no mail, SMS or payment service is
  involved. The only stand-in is the second tenant, which plays the remote destination: the request between the two
  tenants stays on the machine.
- Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, **and** set
  `NO_PROXY=localhost,127.0.0.1`: the source posts the package to the destination with its own HTTP client
  (`OrchardCore.Deployment.Remote/Controllers/ExportRemoteInstanceController.cs:109`), and that request must not go
  through the dead proxy. See OPEN-QUESTIONS.md.
- Start the host, then request `/` (installs the source) and `/staging` (installs the destination) once, and wait for
  each to redirect to its home page.
- The remote client's name and the shared key: the client name is `Main Site`. The key is `<api-key>`: a throwaway
  value chosen outside git and passed to the recorder in `VIDEO_REMOTE_API_KEY`. It is typed into masked fields, so it
  never shows on screen; use the same value in clips `02-client` and `03-instance`, and never reuse a real key.
- Do not open an **Edit** page of a remote client or a remote instance on camera: both put the key into the field's
  value (`Views/RemoteClient/Edit.cshtml:24`, `Views/RemoteInstance/Edit.cshtml:40`).

### Signing in to both tenants

`Recorder setup` signs in to `VIDEO_BASE_URL/Login`, which is the Default tenant only. Each tenant has its own
sign-in cookie (`orchauth_` followed by the tenant name, `OrchardCore.Users/Startup.cs:123`), so the destination needs
its own sign-in in the **same** `state.json`:

1. `Recorder setup` (signs in as `admin` to the source).
2. The video's `PrepareAsync` opens a browser context with `state.json`, goes to `/staging/Login`, signs in as
   `admin` with `VIDEO_PASSWORD` (`#LoginForm_UserName`, `#LoginForm_Password`, Enter), and saves the storage state
   back to `state.json` (git-ignored).
3. Check it: with `Recorder probe /staging/Admin "() => document.title"` the title must not be the login page's.

## Seed data (outside the capture)

Create all of this by hand in the admin, with fake data only, after both tenants are installed (the dates then make
the two pages newer than everything the destination's recipe created).

On the **source** (`Orchard Core`), signed in as `admin`:

1. Two **Page** content items (Content, Content Items, **New**, then the type **Page**; `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`):
   - Title `Spring Planting Workshop`, then **Publish** (the editor is opened from the list, so the page shows the **Publish** button of a button group, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`; click the main button, not the arrow).
   - Title `Seed Swap Weekend`, then **Publish**.

   Leave the flow editor empty. Both must be **published**: the Content step exports published items only
   (`OrchardCore.Contents/Deployment/ContentDeploymentSource.cs:25`). The Blog recipe creates no Page on either
   tenant, so the destination has none until the deployment (`blog.recipe.json` creates a Menu, Taxonomies, a Blog, a
   Blog Post, an Article called About and a Footer).
2. One deployment plan: Tools, Deployments, Plans, **Add Deployment Plan** (`OrchardCore.Deployment/Views/DeploymentPlan/Index.cshtml:25`),
   Name `Publish Pages`, **Create**. Then **Manage Steps** on its row, **Add Step**, in the Available Steps window find
   the card whose heading is exactly **Content** (not **All Content**, not **Content Item**; the **Filter** box helps),
   select its **Add** button, tick **Page** only, leave **Export As Setup recipe** unticked, and select **Create**
   (`OrchardCore.Deployment/Views/Step/Create.cshtml:53`). The plan page then lists one step, **Content**, with the
   badge **Page**.
3. Make sure no remote instance exists yet: Tools, Deployments, Remote Instances shows the empty message.

On the **destination** (`Staging Site`), signed in as `admin`: nothing to create. Make sure no remote client exists
(Tools, Deployments, Remote Clients shows the empty message) and that Content, Content Items lists no Page.

Then stop the host, and copy the whole `App_Data` folder and `state.json` aside as **snapshot S0**.

## Order of the clips, and what each one changes

Record in this order; each clip starts from the state the previous one left.

| # | Clip | Site | Needs | Changes |
|---|---|---|---|---|
| 1 | `02-client` | destination | S0 | Adds the remote client `Main Site` on the destination. |
| 2 | `03-instance` | source | the client's name and key (any order with 1) | Adds the remote instance `Staging` on the source. |
| 3 | `04-send` | source, then the destination receives | 1 and 2 done, the plan and the two published pages | Imports the two pages into the destination. |
| 4 | `05-check` | destination | 3 done | Nothing. |

`03-url` and `06-secure` are slides and `01-how` too: they need no site.

Snapshots: stop the host, copy the whole `App_Data` folder and `state.json` aside, and start the host again.

- **S0**: after the seed data, before any clip (made above).
- **S1**: after `02-client` (the remote client exists on the destination).
- **S2**: after `03-instance` (the client and the instance exist, no page on the destination).

To record a clip again, stop the host, restore the snapshot it needs, start the host, and record:

- `02-client`: restore S0. Then record `03-instance` again, because S1 and S2 are made after it.
- `03-instance`: restore S1.
- `04-send`: restore S2. Without it the destination already holds the pages, and the import would not be a first one.
- `05-check`: no restore, as long as the destination still holds the two pages; after a restore of S2, record `04-send` first.
- The whole video: restore S0 and record `02-client`, `03-instance`, `04-send`, `05-check` in that order.

Every clip starts signed in (the `state.json` above) and with the admin menu collapsed unless the step says otherwise.
The menu is the sidebar `#ta-left-sidebar` (`OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:76`); find its links by
their visible text.

## Clips

### `02-client` (destination, Staging Site)

Starts on `/staging/Admin`, the destination's dashboard. The sidebar is collapsed. Start the capture.

- `client-1`: highlight the site name `Staging Site` at the top left. Click **Tools** in the sidebar, then **Deployments**,
  then click **Remote Clients** (`ClickAndWaitForNavigationAsync`). Wait until the page title **Remote Clients** and the
  message **Nothing here! There are no remote clients for the moment.** are visible. End of the narration on screen:
  the empty Remote Clients page with `Staging Site` in the top bar. Labels: `OrchardCore.Deployment.Remote/AdminMenu.cs:18-25`;
  `Views/RemoteClient/Index.cshtml:6,34`.
- `client-2`: click the button **Add Remote Client** (navigation). End: the form with the labels **Client Name** and
  **Api Key** and the buttons **Create** and **Cancel**. The page has no title bar: that is the code
  (`Views/RemoteClient/Create.cshtml` has no title zone). Labels: `Views/RemoteClient/Index.cshtml:25`; `Create.cshtml:7,15,23-24`.
- `client-3`: type `Main Site` into the **Client Name** field (`#ClientName`). End: the field shows `Main Site`.
- `client-4`: type `<api-key>` (the value of `VIDEO_REMOTE_API_KEY`) into the **Api Key** field (`#ApiKey`). The field is
  a password field, so only dots show. Highlight the field. End: the filled form, key hidden.
- `client-5`: click **Create** at the start of the step (navigation back to the list). The success toast
  **Remote client created successfully.** stays on screen for 5 seconds only
  (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`), so do not wait before clicking. Wait
  until the row link `Main Site` is visible. End: the Remote Clients list with the row `Main Site` and its **Edit** and
  **Delete** buttons (the buttons layout is the default, `OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:36`).
  Labels: `Controllers/RemoteClientController.cs:151`; `Views/RemoteClient.Buttons.SummaryAdmin.cshtml:6,8`.

### `03-url` (slide)

One slide, `url-1`: see "Slides" below. Nothing to record.

### `03-instance` (source, Orchard Core)

Starts on `/Admin`, the source's dashboard. Before the capture, expand **Tools** and **Deployments** in the sidebar
(the narration of `inst-1` names the path, the viewer sees it already open). Start the capture.

- `inst-1`: highlight the site name `Orchard Core` at the top left. Click **Remote Instances** (navigation; the page
  shows **Nothing here! There are no remote instances for the moment.**), then click **Add Remote Instance**
  (navigation). End: the form under the title **Create Remote Instance** with the fields **Name**, **Url**,
  **Client Name**, **Api Key**. Labels: `Views/RemoteInstance/Index.cshtml:25,34`; `Create.cshtml:4,12,20,28,36`.
- `inst-2`: type `Staging` into **Name** (`#Name`). End: the field shows `Staging`.
- `inst-3`: type the full endpoint into **Url** (`#Url`), with a short typing delay (about 20 ms, the text is 87
  characters): `http://localhost:5010/staging/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import` (use the
  host and port of `VIDEO_BASE_URL`). End: the whole value is typed; highlight the field.
  The path comes from `src/docs/reference/modules/Deployment.Remote/README.md` and the controller routes
  (`ImportRemoteInstanceController` has no `[Admin]` route, so the default area route applies).
- `inst-4`: type `Main Site` into **Client Name** (`#ClientName`) and `<api-key>` into **Api Key** (`#ApiKey`), the same
  values as in `02-client`. Do not click **Create** yet. End: the four fields filled, the key hidden.
- `inst-5`: click **Create** at the start of the step (navigation back to the list); the toast
  **Remote instance created successfully.** stays 5 seconds. Wait until the row link `Staging` is visible. Highlight
  the line under the name that shows the Url (the element with the class `remote-instance-url`,
  `Views/RemoteInstance.Description.SummaryAdmin.cshtml:8`). End: the list with `Staging`, its Url under the name, **Edit**
  and **Delete**; no key anywhere. Label: `Controllers/RemoteInstanceController.cs:144`.

### `04-send` (source, Orchard Core)

Starts on `/Admin`. Before the capture, expand **Tools** and **Deployments** in the sidebar. Start the capture.

- `send-1`: click **Plans** (navigation; the list shows the row `Publish Pages` with **Manage Steps**, **Edit**,
  **Delete**), then click **Manage Steps** on that row (navigation). Wait until the step heading **Content** is
  visible. Highlight the step and its badge **Page**. End: the page of the plan, with the buttons **Execute** and
  **Add Step**, and one step. Labels: `OrchardCore.Deployment/AdminMenu.cs:20`; `Views/DeploymentPlanEntry.Buttons.SummaryAdmin.cshtml:6`;
  `OrchardCore.Contents/Views/Items/ContentDeploymentStep.Fields.Summary.cshtml:11`.
- `send-2`: click **Execute** (`button.btn-success` with the text `Execute`, `Views/DeploymentPlan/Display.cshtml:24-25`).
  Wait until the window **Available Targets** is open. Highlight the two cards: **File Download** with
  **Download a deployment plan locally.** and **Staging** with **Sends the deployment plan to a remote instance.**
  (`#modalTargets .card`). End: the open window with both cards. Labels: `Display.cshtml:115,128-129`;
  `OrchardCore.Deployment/Services/FileDownloadDeploymentTargetProvider.cs:20-21`;
  `OrchardCore.Deployment.Remote/Services/RemoteInstanceDeploymentTargetProvider.cs:27`.
- `send-3`: move the cursor to the **Select** button of the **Staging** card (the card whose `h4` is `Staging`:
  `#modalTargets .card` filtered by that text) and highlight it. Do not click. End: the cursor and the highlight on
  **Select** of the Staging card (`Display.cshtml:132`).
- `send-4`: click that **Select** button at the start of the step. It is a link that the page turns into a POST; it
  returns to the plan page (use `ClickAndWaitForNavigationAsync`, then wait until the text
  **Deployment executed successfully.** is visible; if the wait is longer than 2 seconds, put it inside `OffCameraAsync`).
  The toast stays 5 seconds. Highlight the toast. End: the plan page with the green toast
  **Deployment executed successfully.** (`Controllers/ExportRemoteInstanceController.cs:114`). This clip must not be
  recorded with the destination unreachable: that case ends on an error page, not a notification.

### `05-check` (destination, Staging Site)

Starts on `/staging/Admin`, the destination's dashboard. The sidebar is collapsed. Start the capture.

- `check-1`: highlight the site name `Staging Site`. Click **Content** in the sidebar, then click **Content Items**
  (navigation). Wait until the row link `Spring Planting Workshop` is visible. Highlight the two rows
  `Spring Planting Workshop` and `Seed Swap Weekend`, each showing the type badge `Page`. End: the page **Manage Content**
  with the two rows at the top of the list (the default order is **Recently modified**,
  `OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs:98`, the label is at `OrchardCore.Contents/Controllers/AdminController.cs:201`). Labels: `OrchardCore.Contents/AdminMenu.cs:65,68`;
  `Views/Admin/List.cshtml:17`; `Views/ContentsType_SummaryAdmin.cshtml`.
- `check-2`: no action; keep the highlight on the two rows. End: the same list.

## Slides

Slides are drawn by `slides.py`; one line each says what the slide shows.

- `how-1`: "Two sites, two records": cards Source site, Destination site, Shared key; the first two are bright.
- `how-2`: "What Execute does": four cards Build, Send, Check, Import.
- `how-3`: "Before you start": three check marks: Features, Network, Destination features.
- `url-1`: "The Url of a remote instance": cards Site address (`https://example.com`), Tenant prefix (`/staging`, only if the
  destination tenant has a prefix: a tenant can also be reached by its host name, `OrchardCore/OrchardCore/Shell/RunningShellTable.cs:89-115`), Import path, and the three put together.
- `sec-1`: "Keep the connection safe": four cards (Use HTTPS, One key per pairing, Rotate keys, Clean up); the first two are bright.
- `sec-2`: the same slide with the last two cards bright.
- `sec-3`: "Permissions": cards Manage remote instances, Manage remote clients, Manage deployment plans, Export Data
  (`OrchardCore/OrchardCore.Deployment.Core/DeploymentPermissions.cs:8-16`). The slide says what the code checks: the Plans list needs
  **Manage deployment plans** and **Export Data** (`OrchardCore.Deployment/Controllers/DeploymentPlanController.cs:71-79`; the **Plans**
  menu item needs **Export Data**, `OrchardCore.Deployment/AdminMenu.cs:22`), opening a plan needs **Manage deployment plans** (`DeploymentPlanController.cs:171`),
  and executing a plan needs **Export Data** (`OrchardCore.Deployment.Remote/Controllers/ExportRemoteInstanceController.cs:54`). The permission
  **Export to remote instances** is never checked, so the slide does not name it.
- `fix-1`: "If a deployment fails": cards The Url, Client name and Api Key, Reverse proxy, Destination features; the first three are bright
  (the cases where the destination answers with an error status).
- `fix-2`: the same slide with only the last card bright (the source shows success, the content is missing).

## Checks for private data

- Only `admin` and `admin@example.com` exist on both tenants; no other user, name or address is entered or shown.
- The key is typed into masked fields and no **Edit** page is opened. Look at the contact sheets of `02-client`,
  `03-instance` and `04-send`: no field may show the key in clear text.
- The only address on screen is `localhost` and `example.com` (the slide). Search the frames for the machine name,
  the Windows user name and any path of the machine (the admin pages show none; the `App_Data` folder is never opened).
- The dates of the items are the date of the recording: that is fine. No real person's name appears in the content
  (the pages are `Spring Planting Workshop` and `Seed Swap Weekend`).
- OCR the final video at one frame per second and at every scene change for the password and the key values.

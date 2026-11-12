# Recording plan: Deployment plans

How to record this video without deciding anything. The narration is in `storyboard.json`; every label below was read
from the Orchard Core source at commit 92db4a83 (paths are relative to the root of `OrchardCMS/OrchardCore`). Anything
that could not be checked without a running site is in `OPEN-QUESTIONS.md`: read it before the first take.

In this plan a selector stands for `scene.Page.Locator(...)`, and a click is `scene.ClickAsync(...)` unless it loads a
page, which is `scene.ClickAndWaitForNavigationAsync(...)`. Label sources are written `path:line`; `Deployment`, `Contents`,
`ContentTypes`, `Roles` and `Users` stand for `src/OrchardCore.Modules/OrchardCore.Deployment`, `...Contents`, `...ContentTypes`,
`...Roles` and `...Users`.

## The site it needs

| | |
|---|---|
| Checkout | `src/OrchardCore.Cms.Web` of an Orchard Core checkout, with the Auto Setup feature registered in the host (see the Auto Setup documentation page) |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Auto Setup | `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `SiteName=Orchard Core`, `SiteTimeZone=America/Los_Angeles`, `AdminUsername=admin`, `AdminEmail=admin@example.com`, `AdminPassword=<password>` (the value of `VIDEO_PASSWORD`, chosen outside git), `DatabaseProvider=Sqlite`, `RecipeName=Blog` |
| Tenant | `Default`, at `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Recipe | Blog. It enables Deployment, Remote Deployment, Contents, Content Types, Roles, Users, Settings, Media, Layers, Templates, Placements, Features and Themes, so no feature has to be enabled |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address |
| Sign-in | `Recorder setup` signs in as `admin` with `VIDEO_PASSWORD` and writes `state.json` (git-ignored). Auto Setup stays on: this video does not show the Setup screen |
| Viewport and theme | The recorder's defaults. The site theme is irrelevant: every clip is in the admin, which the Blog recipe gives the TheAdmin theme |

Content already on the site: the Blog recipe creates one published Blog Post, so the Content step has something to
export. Do not add content, do not create a remote instance (the Available Targets dialog must list File Download
only), and do not enable or disable any feature.

### Seed data (created outside the capture, in `PrepareAsync`)

Two empty plans, so the list in clips 02 and 07 has rows and the search has something to narrow. For each name, open
`/Admin/DeploymentPlan/Create` (`Deployment/Controllers/DeploymentPlanController.cs`), type the name in the field
`input[name="Name"]`, and click the button `button.create`:

1. `Archive Old Posts`
2. `Weekly Settings Backup`

No other data is needed. `Spring Launch` is created on camera by clip 02, and the labels of the fake values typed on
camera are listed in each clip.

## Order, dependencies and snapshots

Record in the order the clips play. Each clip starts from the state the previous one left, except `07-manage`, which
only needs the two seeded plans (it also lists `Spring Launch` when it is recorded after the others).

| Clip | Starts from | Changes the site |
|---|---|---|
| `02-create` | snapshot S0 (after setup and seed) | yes: adds the plan Spring Launch |
| `03-steps` | S1 | yes: adds two steps to Spring Launch |
| `04-order` | S2 | yes: moves a step, changes the Content step |
| `05-general` | S3 | yes: adds Custom File and Recipe File |
| `06-execute` | S4 | no (a file is downloaded, which is deleted afterwards) |
| `07-manage` | S4 | yes: renames Weekly Settings Backup |
| slides, cards | none | no |

Stop the site and copy its data folder aside after the seed (S0) and after each clip that changes the site (S1 after
`02-create`, S2 after `03-steps`, S3 after `04-order`, S4 after `05-general`). Before a new take of a clip, restore the
snapshot it starts from, start the site again, and run `Recorder setup` only if `state.json` is no longer valid.

## Cards and slides

- `intro` (opening card): no recording. The card shows the title "Orchard Core deployment plans" and the subtitle "Choose what a package holds".
- `plan-1` (slide): four cards under "A deployment plan"; the first two are bright: "A named list of steps" and "Most steps add data".
- `plan-2` (slide): the same four cards; the last two are bright: "Features provide the steps" and "Executing builds a package".
- `package-1` (slide): three cards under "What the package holds": "A recipe", "Your files", "A zip named after the plan".
- `access-1` (slide): three cards under "Who can use plans": "Manage deployment plans", "Export Data", "Administrators".
- `outro` (closing card): no recording. The subtitle is "docs.orchardcore.net".

## Clip 02-create

Starts on `/Admin` (the admin home page), opened before the capture, with the admin menu showing its top-level entries and
Tools closed. The cursor rests in the middle of the page.

- `create-1`: click the menu entry Tools, then Deployments, then Plans (`Deployment/AdminMenu.cs:18`, `:19`, `:20`),
  waiting for the page whose breadcrumb reads "Deployment Plans" (`Deployment/Views/DeploymentPlan/Index.cshtml:6`).
  Rows `Archive Old Posts` and `Weekly Settings Backup` are listed. About two seconds before the narration ends,
  highlight the button `a.create` ("Add Deployment Plan", `Deployment/Views/DeploymentPlan/Index.cshtml:25`), pause 600 ms, and click it with
  `ClickAndWaitForNavigationAsync`. On screen when the narration ends: the page "Create Deployment Plan"
  (`Deployment/Views/DeploymentPlan/Create.cshtml:4`) with the empty field Name focused.
- `create-2`: type `Spring Launch` in `input[name="Name"]` (label "Name", `Deployment/Views/DeploymentPlan/Create.cshtml:12`), at 70 ms per character.
  Then click the button `button.create` ("Create", `Deployment/Views/DeploymentPlan/Create.cshtml:20`) with `ClickAndWaitForNavigationAsync`. On
  screen when the narration ends: the plan page has loaded (see `create-3`).
- `create-3`: no click. Highlight the button "Execute" (`Deployment/Views/DeploymentPlan/Display.cshtml:25`), then the
  button "Add Step" (`Deployment/Views/DeploymentPlan/Display.cshtml:31`), then the alert `.alert-info` ("Nothing here! There are no steps on this
  plan for the moment.", `Deployment/Views/DeploymentPlan/Display.cshtml:57`). On screen when the narration ends: the plan page (breadcrumb
  "Deployment Plan", `Deployment/Views/DeploymentPlan/Display.cshtml:12`) with the three highlights cleared and the empty-plan message visible.

## Clip 03-steps

Starts on the page of the plan Spring Launch. Before the capture, open `/Admin/DeploymentPlan/Index`, click the link
"Spring Launch", and wait for the plan page. The plan has no steps.

- `add-1`: click the button "Add Step" (`Deployment/Views/DeploymentPlan/Display.cshtml:31`). Wait until `#modalSteps` is shown and its cards are
  visible (the dialog fades in). Highlight the Filter box (`#modalSteps input[type=search]`, placeholder "Filter",
  `Deployment/Views/DeploymentPlan/Display.cshtml:75`), then the category list `#deployment-step-categories`. On screen when the narration ends: the
  dialog "Available Steps" (`Deployment/Views/DeploymentPlan/Display.cshtml:68`) with "All" selected (`Deployment/Views/DeploymentPlan/Display.cshtml:79`) and cards in a grid.
- `add-2`: click the category link "Security" in `#deployment-step-categories` (category from
  `Roles/Deployment/AllRolesDeploymentStep.cs:15` and
  `Users/Deployment/AllUsersDeploymentStep.cs:15`). Wait until only the Security cards
  remain. Highlight the card titles "All Roles" (`Roles/Views/Items/AllRolesDeploymentStep.Thumbnail.cshtml:3`)
  and "All Users" (`Users/Views/Items/AllUsersDeploymentStep.Thumbnail.cshtml:1`) in turn. Do not click
  Add. On screen when the narration ends: the Security category selected, with the cards All Roles ("Exports every
  role.") and All Users ("Exports every user.") visible.
- `add-3`: click the category link "All". Type `content` in the Filter box (80 ms per character) and wait until the
  cards that do not contain the text are hidden. Click the link "Add" (`Deployment/Views/DeploymentStep.Thumbnail.cshtml:8`)
  of the card whose heading is exactly "Content"
  (`Contents/Views/Items/ContentDeploymentStep.Fields.Thumbnail.cshtml:3`), with
  `ClickAndWaitForNavigationAsync`: `#modalSteps .deployment-step-item` filtered by a heading with the exact name
  "Content", then its link "Add". On screen when the narration ends: the editor of the step, with the hint "The
  content types to add as part of the plan." (`Contents/Views/Items/ContentDeploymentStep.Fields.Edit.cshtml:13`) and the list of content
  types.
- `add-4`: check the box labeled exactly "Blog Post" (`GetByLabel("Blog Post", new() { Exact = true })`, label from
  `Contents/Views/Items/ContentDeploymentStep.Fields.Edit.cshtml:44`). Highlight the label "Export As Setup recipe"
  (`Contents/Views/Items/ContentDeploymentStep.Fields.Edit.cshtml:21`) and the warning under it (`:29`); leave the box unchecked. Click the
  button "Create" (`Deployment/Views/Step/Create.cshtml:53`) about two seconds before the narration ends, with
  `ClickAndWaitForNavigationAsync`. On screen when the narration ends: the plan page with one row, the heading
  "Content" (`Contents/Views/Items/ContentDeploymentStep.Fields.Summary.cshtml:11`) and the badge "Blog Post".
- `add-5`: click "Add Step". Type `update content` in the Filter box and wait until one card remains: "Update Content
  Definitions" ("Exports specified content type and part definitions for updating.",
  `ContentTypes/Views/Items/ContentDefinitionDeploymentStep.Fields.Thumbnail.cshtml:1`). Click its link
  "Add" with `ClickAndWaitForNavigationAsync`. On screen when the narration ends: the editor of the step, with the
  checkbox "Include all content types and parts definitions." unchecked
  (`ContentTypes/Views/Items/ContentDefinitionDeploymentStep.Fields.Edit.cshtml:21`) and the lists "Content Types" (`:29`) and "Content Parts" (`:86`).
- `add-6`: highlight the checkbox "Include all content types and parts definitions." and leave it unchecked. Check the
  box labeled exactly "Blog Post" (it expands the parts of the type; do not touch them). Click "Create"
  (`Deployment/Views/Step/Create.cshtml:53`) about two seconds before the narration ends. On screen when the narration ends: the plan
  page with two rows in this order: "Content" with the badge "Blog Post", then "Update Content Definitions"
  (`ContentTypes/Views/Items/ContentDefinitionDeploymentStep.Fields.Summary.cshtml:8`) with the badge "Blog Post" and the line "Content
  Definitions should be placed before any content steps." (`:46`).

## Clip 04-order

Starts on the plan page of Spring Launch with the two rows of the end of clip 03 (Content first, Update Content
Definitions second).

- `order-1`: highlight the line "Content Definitions should be placed before any content steps." under the row
  "Update Content Definitions". Clear the highlight, then drag the handle of the second row
  (`#stepOrder > li:nth-child(2) .ui-sortable-handle`, `Deployment/Views/DeploymentPlan/Display.cshtml:47`) onto the first row with
  `DragToAsync(handle, firstRow, offsetY: -10)`. On screen when the narration ends: the row "Update Content
  Definitions" is above "Content" (see `OPEN-QUESTIONS.md` if the drag does not reorder the rows).
- `order-2`: no click, until about three seconds before the narration ends: then `NavigateAsync` to the same plan page
  (reload), to show that the order was saved. On screen when the narration ends: the reloaded plan page with
  "Update Content Definitions" first and "Content" second.
- `order-3`: click the button "Edit" (`Deployment/Views/DeploymentStep.Summary.cshtml:12`) of the row "Content", with
  `ClickAndWaitForNavigationAsync`. In the editor, check the box labeled exactly "Page". Click "Save"
  (`Deployment/Views/Step/Edit.cshtml:53`) with `ClickAndWaitForNavigationAsync`. On screen when the narration ends:
  the plan page, the row "Content" showing the badges "Blog Post" and "Page", and the order unchanged.
- `order-4`: no click. Highlight the "Delete" button of the first row (`Deployment/Views/DeploymentStep.Summary.cshtml:20`), then the
  one of the second row. Never click Delete (it opens a confirmation dialog). On screen when the narration ends: the
  plan page with one Delete button highlighted.

## Clip 05-general

Starts on the plan page of Spring Launch with the two rows of the end of clip 04.

- `general-1`: click "Add Step", wait for the dialog, and click the category link "Deployment" in
  `#deployment-step-categories` (category from `Deployment/Steps/CustomFileDeploymentStep.cs:15`). On screen when the
  narration ends: the dialog with "Deployment" selected and exactly four cards: Custom File, Deployment Plans, JSON
  Recipe and Recipe File.
- `general-2`: highlight the card heading "Custom File" ("Exports a custom file and its content.",
  `Deployment/Views/Items/CustomFileDeploymentStep.Fields.Thumbnail.cshtml:3`), then "Deployment Plans" ("Exports all
  or specified deployment plans.", `Deployment/Views/Items/DeploymentPlanDeploymentStep.Fields.Thumbnail.cshtml:3`), then "JSON Recipe"
  ("Exports a JSON Recipe.", `Deployment/Views/Items/JsonRecipeDeploymentStep.Fields.Thumbnail.cshtml:3`), about two seconds each. Do not
  open the JSON Recipe step: its editor shows code. On screen when the narration ends: the same dialog, one
  highlight left on "JSON Recipe".
- `general-3`: clear the highlights. Click the link "Add" of the card "Custom File" with
  `ClickAndWaitForNavigationAsync`. Type `notes/readme.txt` in the field labeled "File Name"
  (`Deployment/Views/Items/CustomFileDeploymentStep.Fields.Edit.cshtml:11`) and `Prepared for the spring launch.` in the field labeled "File
  Content" (`:20`), at 55 ms per character. Click "Create" (`Deployment/Views/Step/Create.cshtml:53`) about two seconds before the
  narration ends. On screen when the narration ends: the plan page with three rows; the third is "Custom File"
  (`Deployment/Views/Items/CustomFileDeploymentStep.Fields.Summary.cshtml:8`) and shows `notes/readme.txt`.
- `general-4`: click "Add Step", type `recipe file` in the Filter box, and wait until one card remains: "Recipe File"
  ("Exports a recipe file.", `Deployment/Views/Items/RecipeFileDeploymentStep.Fields.Thumbnail.cshtml:3`). Click its link "Add" with
  `ClickAndWaitForNavigationAsync`. Type `spring-launch` in the field labeled "Recipe Name"
  (`Deployment/Views/Items/RecipeFileDeploymentStep.Fields.Edit.cshtml:11`) and `Spring Launch` in the field labeled "Display Name" (`:19`).
  Leave Description, Author, WebSite, Version, Categories, Tags and "Setup recipe" empty. Click "Create". On screen
  when the narration ends: the plan page with four rows; the fourth is "Recipe File"
  (`Deployment/Views/Items/RecipeFileDeploymentStep.Fields.Summary.cshtml:7`) and shows `spring-launch`.

## Clip 06-execute

Starts on the plan page of Spring Launch with the four rows of the end of clip 05. Set the browser context to accept
downloads (the Playwright default) and keep the downloaded file out of the repository.

- `run-1`: click the button "Execute" (`Deployment/Views/DeploymentPlan/Display.cshtml:25`). Wait for `#modalTargets` ("Available Targets",
  `Deployment/Views/DeploymentPlan/Display.cshtml:115`). Highlight the card whose heading is "File Download" (`Deployment/Services/FileDownloadDeploymentTargetProvider.cs:20`)
  and the line "Download a deployment plan locally." (`:21`). On screen when the narration ends: the dialog with a
  single card, File Download, and its button "Select".
- `run-2`: click the button "Select" of the card "File Download" (`Deployment/Views/DeploymentPlan/Display.cshtml:132`), wrapped in
  `scene.Page.RunAndWaitForDownloadAsync(...)`. Nothing navigates (the answer is an attachment), so do not use
  `ClickAndWaitForNavigationAsync`. Wait for the download to finish, then discard the file; its name must be
  `SpringLaunch.zip` (`Deployment/Controllers/ExportFileController.cs:67`). On screen when the narration ends: the
  plan page behind the dialog "Available Targets", which is still open, with no error message.
- `run-3`: click the button "Cancel" of the dialog (`Deployment/Views/DeploymentPlan/Display.cshtml:140`). On screen when the narration ends: the plan
  page with its four rows and no dialog.

## Clip 07-manage

Starts on `/Admin/DeploymentPlan/Index`, opened before the capture. The list shows `Archive Old Posts`, `Spring
Launch` (when clips 02 to 05 were recorded) and `Weekly Settings Backup`.

- `manage-1`: type `Weekly` in the search box (`#search-box`, placeholder "Search", `Deployment/Views/DeploymentPlan/Index.cshtml:20`), 80 ms per
  character, and press Enter inside `WaitForNavigationAsync`. Then highlight, in turn on the single row, the buttons
  "Manage Steps", "Edit" and "Delete" (`Deployment/Views/DeploymentPlanEntry.Buttons.SummaryAdmin.cshtml:6`, `:8`,
  `:10`). Never click Delete. On screen when the narration ends: the list narrowed to `Weekly Settings Backup`.
- `manage-2`: click the button "Edit" of that row with `ClickAndWaitForNavigationAsync`. On the page "Edit Deployment
  Plan" (`Deployment/Views/DeploymentPlan/Edit.cshtml:4`), clear the field Name with `FillAsync("")` (the field holds
  `Weekly Settings Backup`), type `Weekly Settings Export` with `TypeAsync`, and click "Save" (`Deployment/Views/DeploymentPlan/Edit.cshtml:21`) with
  `ClickAndWaitForNavigationAsync`. On screen when the narration ends: the list of plans with `Archive Old Posts`,
  `Spring Launch` and `Weekly Settings Export`, and the success notification "Deployment plan updated successfully."
  (`Deployment/Controllers/DeploymentPlanController.cs:335`) visible if the theme keeps it on screen for that long.

## After the takes

- Delete the downloaded `SpringLaunch.zip`. It holds only the fake data of this site, but it is not part of the video
  folder.
- Stop the site by its process ID. Remove the copied data folders.

## Checks for private data

- The only person on screen is `admin`, and the site's own sample content (the Blog recipe's text). No real names,
  email addresses or tokens appear, and nothing typed on camera is an address. The administrator's email,
  `admin@example.com`, is not on any page of this video.
- Typed values: `Spring Launch`, `content`, `update content`, `notes/readme.txt`, `Prepared for the spring launch.`,
  `recipe file`, `spring-launch`, `Weekly Settings Export`, `Weekly`. None is real.
- Never type or show the sign-in password: `state.json` carries the session, and the password lives in
  `VIDEO_PASSWORD`.
- Do not open or show the JSON Recipe step, the Site Settings step (it lists settings such as the site salt), or the
  downloaded package.
- OCR the video at one frame per second and at every scene change as the skill says, and search for the host name of
  the machine, user names of the operating system, and file paths. The page address is not shown in the frame.

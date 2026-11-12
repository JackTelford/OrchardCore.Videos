# Open questions: Deployment plans

What was checked in the source (Orchard Core commit 92db4a83; paths start at the root of the repository) and what a
person must confirm on a running site before or while recording. The narration states only what the source shows; each item below
names the sentence it protects.

## Things that need a running site

1. **The menu path (`create-1`).** `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:18-20` defines Tools, Deployments, Plans. The
   markup of the admin menu (whether Tools and Deployments expand on a click, and how a collapsed sidebar behaves) is in
   the admin theme and was not read. If clicking through the menu is unreliable, open `/Admin/DeploymentPlan/Index`
   with `NavigateAsync` after pointing at the menu entries.
2. **Heading of the Content step.** The card and the row are titled "Content"
   (`src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/ContentDeploymentStep.Fields.Thumbnail.cshtml:3`, `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/ContentDeploymentStep.Fields.Summary.cshtml:11`), but the
   step's page title comes from `ContentsLocalizationSources.ContentTypes`, which is "Content Types"
   (`src/OrchardCore.Modules/OrchardCore.Contents/ContentsLocalizationSources.cs:17`). The narration says "Content" for the card and the row and
   never names the page title. Confirm what the editor page shows in the breadcrumb.
3. **The cards behind each filter.** The Filter box matches the whole text of a card
   (`src/OrchardCore.Modules/OrchardCore.Deployment/Assets/ts/deployment-plan-display.ts`). Confirm on the site that `content` leaves a card
   whose heading is exactly "Content", that `update content` leaves only "Update Content Definitions", and that `recipe
   file` leaves only "Recipe File". Confirm that the category "Security" shows All Roles and All Users (`add-2`), and
   that "Deployment" shows exactly the four steps of the Deployment module (`general-1` says "four"): another enabled
   feature could add a card to a category.
4. **Drag and drop (`order-1`).** The order is changed with SortableJS on `.ui-sortable-handle`
   (`src/OrchardCore.Modules/OrchardCore.Deployment/Assets/js/steporder.js`) and saved with a request that shows no message. Whether
   `DragToAsync` moves the row in a headless browser was not tested. If it does not, try low-level mouse moves
   (press on the handle, move in several steps, release). The reload in `order-2` proves the order was saved; if the
   order is not saved, do not publish `order-2`.
5. **The download (`run-2`, `run-3`).** The button "Select" posts a form made by the admin theme
   (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts`, `data-url-af="UnsafeUrl"`) to
   `ExportFileController.Execute`, which answers with a zip as an attachment, so the page does not change and the
   dialog is expected to stay open. Confirm what the screen shows after the click, and that the file is named
   `SpringLaunch.zip` (`ToSafeName` removes spaces and punctuation:
   `src/OrchardCore/OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:201`). The narration says that the browser downloads a zip file
   named after the plan; it does not say that anything is visible.
6. **What is in the package (`package-1`).** Open the downloaded zip once (outside the video). Expected from the code
   (`src/OrchardCore/OrchardCore.Deployment.Abstractions/DeploymentPlanResult.cs`, `src/OrchardCore.Modules/OrchardCore.Contents/Deployment/ContentDeploymentSource.cs`,
   `src/OrchardCore.Modules/OrchardCore.ContentTypes/Deployment/ContentDefinitionDeploymentSource.cs`): one recipe whose header carries the
   recipe name `spring-launch` and the display name `Spring Launch`, a definition step first and a content step second
   (the content step exists only because the Blog recipe creates a published Blog Post), and the file
   `notes/readme.txt`. The Custom File step adds a file and no recipe step, and the Recipe File step adds no recipe step.
7. **The confirmation after Save (`manage-2`).** The message "Deployment plan updated successfully."
   (`src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/DeploymentPlanController.cs:335`) is shown by the notifier. How long the admin
   theme keeps it on screen was not read. If it disappears before the narration ends, narrate the list only.

## Statements read from the code and not run

- **A step whose feature is disabled (`plan-2`).** The plan keeps the step and shows "This deployment step is
  unavailable because the feature that provides it has been disabled or removed."
  (`src/OrchardCore.Modules/OrchardCore.Deployment/Views/Items/UnknownDeploymentStep.Fields.Summary.cshtml:5`, registered by
  `src/OrchardCore.Modules/OrchardCore.Deployment/Startup.cs:36`). To confirm: add the step All Placements, disable the Placements feature,
  open the plan, and re-enable the feature. This is not recorded.
- **The two permissions (`access-1`).** `DeploymentPlanController.Index` refuses a user without both Manage deployment
  plans and Export Data (`src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/DeploymentPlanController.cs:71-79`); the plan page, the editors and the reorder request need
  Manage deployment plans; executing needs Export Data (`src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/ExportFileController.cs:54`). Confirm with a role that holds
  only one of them. The admin menu entry needs Export Data only (`src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:22`).
- **A failing step (`run-2`).** When a step throws, `ExportFileController` shows "An error has occurred while exporting
  to file. Please try again or check the application logs." and returns to the plan page, with no download
  (`src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/ExportFileController.cs:96-105`). Not provoked on a site.
- **Targets added by other features (`run-1`).** Remote Deployment adds one card per configured remote instance
  (`src/OrchardCore.Modules/OrchardCore.Deployment.Remote/Services/RemoteInstanceDeploymentTargetProvider.cs:20-37`). The recording site has no
  instance, so only File Download is listed.
- **Only the first Recipe File step counts, and only File Download reads it (`general-4`).**
  `src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/ExportFileController.cs:74` takes the first Recipe File step of the plan. A search of the source finds no other
  reader of that step. The remote export builds an empty descriptor
  (`src/OrchardCore.Modules/OrchardCore.Deployment.Remote/Controllers/ExportRemoteInstanceController.cs:82`), so only File Download uses it.
- **Published items only (`add-4`, `run-3`).** The Content step queries the published version of each item
  (`src/OrchardCore.Modules/OrchardCore.Contents/Deployment/ContentDeploymentSource.cs:25`), when the plan is executed.
- **A Custom File step with an empty File Content.** An empty text area is expected to bind as null, and the export
  then converts that value to bytes (`src/OrchardCore.Modules/OrchardCore.Deployment/Steps/CustomFileDeploymentSource.cs:9`),
  which would make the whole export fail with the error message above. The clip types a sentence in File Content; never
  leave it empty when recording `05-general`. Not run.

## Pronunciation

The voice has to say "JSON Recipe" (`general-1`, `general-2`). Have a person listen to the narration of `general-1`
for how "JSON" is spoken. No `spoken` entry was added.

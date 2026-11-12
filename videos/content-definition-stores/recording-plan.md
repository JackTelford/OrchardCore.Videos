# Recording plan: Content Definition Stores

The video has nine slide steps (drawn by `slides.py`, nothing to record) and seven recorded steps in three clips:
`03-export`, `03-switch` and `03-import`. The three clips are one story on one site and **must be recorded in this
order**. Paths such as `src/OrchardCore.Modules/...` are in the Orchard Core checkout the site is built from.
A selector in this plan stands for `scene.Page.Locator(...)`.

## The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an Orchard Core checkout, run on `http://localhost:5010` with its own `ORCHARD_APP_DATA` folder outside both repositories |
| Setup | Auto Setup (see `references/recording.md`): recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, tenant `Default`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Outbound calls | Blocked with `HTTP_PROXY` / `HTTPS_PROXY` set to a dead address. No external service is used |
| Features | The Blog recipe enables everything the video uses: `OrchardCore.Features`, `OrchardCore.ContentTypes`, `OrchardCore.Deployment` and `OrchardCore.Contents` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). `OrchardCore.Contents.FileContentDefinition` (File Content Definition) is **not** enabled by the recipe: the seed below enables it |
| Sign-in | A `state.json` saved by `Recorder setup` at step 1 of the seed, outside the capture, and copied aside with the snapshot at step 8. Never committed |
| Downloads | A folder outside the repository, called `<downloads>` below (for example `%TEMP%/ocvideo/cds-downloads`). The scenes read it from an environment variable of your own, such as `VIDEO_DOWNLOADS`. Nothing in it is committed |
| Data | Fake only. The one record the video creates is the content type **Event**, plus the deployment plan **Content Definitions** that the first clip creates |

The viewport, the cursor and the rest of the recording follow `references/specs.md`.

## The seed (outside the capture, once)

The video starts from a site whose definitions live in the file, so the seed builds that state. Do it in a browser
signed in as `admin`, or with the recorder's `PrepareAsync`; none of it is recorded.

1. Start the site and let Auto Setup finish. Run `Recorder setup`.
2. **Export the Blog definitions from the database.** Tools, Deployments, Plans, Add Deployment Plan. Name it `Seed`,
   select Create. On the plan, Add Step, add Update Content Definitions, check Include all content types and parts
   definitions, select Create. Execute, select File Download in Available Targets. Save the download as
   `<downloads>/Seed.zip`.
3. **Delete the plan `Seed`** (Plans list, Delete, confirm), so that the Plans list is empty when `03-export` starts.
4. **Enable the file store.** Tools, Features, search `File Content`, select Enable on File Content Definition. Wait for
   the message "The feature File Content Definition was enabled."
5. **Put the Blog definitions in the file.** Tools, Deployments, Package Import, select `<downloads>/Seed.zip`, select
   Import. Wait for "Deployment package imported."
6. **Create the type Event in the file.** Design, Content Definition, Content Types, Create new type. Display Name
   `Event`, Technical Name `Event` (it fills in by itself), select Create. Wait for the message
   `The "Event" content type has been created.`
7. **Check the state.**
   - `<ORCHARD_APP_DATA>/Sites/Default/ContentDefinition.json` exists, and contains `"Name": "Event"`.
   - Design, Content Definition, Content Types lists Event and the types of the Blog recipe (Article, Blog, Blog Post,
     Page, Tag and others).
   - Typing `Event` in the search box of that page shows the Event row.
   - Tools, Deployments, Plans shows "Nothing here! There are no deployment plans at the moment."
   - Tools, Features shows File Content Definition as Enabled.
8. **Snapshot.** Stop the site (by its process id). Copy the whole `ORCHARD_APP_DATA` folder and `state.json` aside as
   the snapshot. Delete `<downloads>/ContentDefinitions.zip` if a previous take left one. Start the site again.

## Order of the clips, and what each one changes

| Clip | Needs | Changes the site |
|---|---|---|
| `03-export` | The snapshot state | Creates the deployment plan `Content Definitions` with one step. Saves `<downloads>/ContentDefinitions.zip` |
| `03-switch` | The snapshot state (the feature is enabled) | Disables File Content Definition, which restarts the tenant on the database store |
| `03-import` | The feature disabled, and `<downloads>/ContentDefinitions.zip` from `03-export` | Imports the package: the database gets the type Event |

- **A full take:** restore the snapshot (stop the site, restore `ORCHARD_APP_DATA` and `state.json`, start the site),
  then record `03-export`, `03-switch`, `03-import` in that order, without restoring in between.
- **Only `03-export` again:** restore the snapshot first, because the plan `Content Definitions` already exists
  afterwards (a plan name must be unique).
- **Only `03-switch` again:** restore the snapshot first.
- **Only `03-import` again:** restore the snapshot, then disable File Content Definition off camera (Tools, Features,
  Disable, Yes). Keep the `ContentDefinitions.zip` of the last `03-export` take: the snapshot's file has not changed,
  so the package is still right.

## Clip `03-export`

Starts on `/Admin/DeploymentPlan/Index` (Tools, Deployments, Plans), opened before the capture. The list is empty.
The clip needs a method to save a download; the scene API has none, so the scene uses Playwright directly (see
"Calls the scene API does not have").

### `export-1`

1. `PauseAsync(6000)`: the first two sentences are spoken over the empty list.
2. `ClickAndWaitForNavigationAsync` on `a.btn:has-text("Add Deployment Plan")`.
3. `TypeAsync` into `input[name="Name"]`: `Content Definitions`.
4. `ClickAndWaitForNavigationAsync` on `button.create` (text Create). The browser lands on the plan's page.

On screen when the narration ends: the page of the plan (title "Deployment Plan", breadcrumb "Deployment Plans"), with the
buttons Execute and Add Step, and the message "Nothing here! There are no steps on this plan for the moment."

Labels: Plans and Add Deployment Plan, `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:20` and
`Views/DeploymentPlan/Index.cshtml:25`; Name and Create, `Views/DeploymentPlan/Create.cshtml:12,20`; the redirect to the
plan's page, `Controllers/DeploymentPlanController.cs:268`; the empty message, `Views/DeploymentPlan/Display.cshtml`
(`There are no steps on this plan for the moment.`).

### `export-2`

1. `ClickAsync` on `button:has-text("Add Step")`. The dialog "Available Steps" opens (`#modalSteps`).
2. `TypeAsync` into `#modalSteps #search-box` (placeholder Filter): `Update Content`.
3. `ClickAndWaitForNavigationAsync` on `#modalSteps .deployment-step-item:has-text("Update Content Definitions") a:has-text("Add")`.
   The step form opens (title "Update Content Definitions").
4. `ClickAsync` on `label:has-text("Include all content types and parts definitions.")`. The checkbox is checked and the lists of types
   and parts below it collapse.
5. `ClickAndWaitForNavigationAsync` on `button.create` (text Create). The browser returns to the plan's page.

Use `pauseAfter: 300` on the clicks: the actions must finish before the narration (about 11 s) does.

On screen when the narration ends: the plan's page with one step, "Update Content Definitions", with the badge "All",
and the message "Deployment plan step added successfully."

Labels: Add Step, `Views/DeploymentPlan/Display.cshtml:31`; Available Steps, `Display.cshtml:68`; the card,
`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Items/ContentDefinitionDeploymentStep.Fields.Thumbnail.cshtml:1`; Add,
`OrchardCore.Deployment/Views/DeploymentStep.Thumbnail.cshtml:8`; the check box,
`OrchardCore.ContentTypes/Views/Items/ContentDefinitionDeploymentStep.Fields.Edit.cshtml:21`; Create,
`OrchardCore.Deployment/Views/Step/Create.cshtml:53`; the message, `OrchardCore.Deployment/Controllers/StepController.cs:112`;
the badge, `ContentDefinitionDeploymentStep.Fields.Summary.cshtml`.

### `export-3`

1. `ClickAsync` on `button:has-text("Execute")`. The dialog "Available Targets" opens (`#modalTargets`).
2. `ClickAsync` on `#modalTargets .card:has-text("File Download") a:has-text("Select")`, inside
   `scene.Page.RunAndWaitForDownloadAsync(...)`. Save the download as `<downloads>/ContentDefinitions.zip`. Select
   starts a form post and the response is a file, so the page does not navigate and the dialog stays open. The link
   carries `data-url-af="UnsafeUrl"` (`Display.cshtml:132`): `TheAdmin.ts:115-185` turns it into a POST form with the
   page's `__RequestVerificationToken`, and `ExportFileController.Execute` accepts only a POST (`:50-52`).
3. `HighlightAsync` on the card `#modalTargets .card:has-text("File Download")`, with the caption `ContentDefinitions.zip`
   (a headless browser draws no download indicator, so the caption shows what the narration says).

On screen when the narration ends: the dialog "Available Targets" over the plan's page, the File Download card ringed
in green with the caption `ContentDefinitions.zip`. Call `ClearHighlightsAsync` after the step.

Labels: Execute, `Display.cshtml:25`; Available Targets, `Display.cshtml:115`; Select, `Display.cshtml:132`; File
Download and its description "Download a deployment plan locally.", `OrchardCore.Deployment/Services/FileDownloadDeploymentTargetProvider.cs:20`;
the file name, `OrchardCore.Deployment/Controllers/ExportFileController.cs:67` (the plan's name without spaces, plus `.zip`).

## Clip `03-switch`

Starts on `/Admin/Features` (Tools, Features), opened before the capture, with the site in the state `03-export` left
(the feature is still enabled).

### `switch-1`

1. `TypeAsync` into `#search-box`: `File Content`. Only the row File Content Definition remains, under the category
   Content Management, with the badge Enabled.
2. `ClickAsync` on `#btn-disable-OrchardCore_Contents_FileContentDefinition` (text Disable). The dialog "Disable Feature"
   opens: "Are you sure you want to disable the File Content Definition feature? Continue?", with Yes and No.
3. `WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("#modalOkButton")))` (text Yes). The tenant restarts
   and the page loads again; the wait is on camera. If the restart is longer than the narration leaves (see
   OPEN-QUESTIONS.md, item 4), put the click and the wait off camera instead (not tried):
   `await scene.MoveToAsync(yes); await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => yes.ClickAsync()));`
   where `yes` is `scene.Page.Locator("#modalOkButton")`. The cursor rests on Yes, then the video cuts to the reloaded
   page (the click ripple is not shown). A wait placed after `WaitForNavigationAsync` returns at once and cuts nothing.
4. `TypeAsync` into `#search-box` again: `File Content` (the reloaded page lists every feature).

On screen when the narration ends: the message "The feature File Content Definition was disabled.", and the row File
Content Definition with the badge Disabled and the button Enable.

Labels: Tools and Features, `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26,27`; the search box,
`Views/Admin/Features.cshtml:27`; the button, the dialog title, the message, Yes and No,
`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:45,53` (the element id is built from the feature id, as in the example
`Scenes.cs` of the skill, `#btn-disable-OrchardCore_Secrets`); the badges Enabled and Disabled,
`Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml`; the message, `Controllers/AdminController.cs:321`; the dialog
buttons, `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-36` (`#modalOkButton`).

### `switch-2`

1. `NavigateAsync("/Admin/ContentTypes/List")`. The list shows the types of the Blog recipe, and not Event.
2. `TypeAsync` into `#search-box`: `Event`. The list filters as you type (the `input` event, `src/OrchardCore.Modules/OrchardCore.Resources/Assets/js/list-management.js:462`).
   **Never press Enter in this box**: the Content Types list sets `data-single-result-action-mode="first"`
   (`Views/Admin/List.cshtml:9`) and the default `noResultsActionSelector` of `list-management.js` is `#btnCreate`
   (same file, `:27`, `:372-377`), so Enter with no match leaves the page for Create with `?suggestion=Event`, and Enter with a
   match opens the first row's edit page.

On screen when the narration ends: the page Content Types, `Event` in the search box, no row, and the message "Your search
returned no results."

Labels: Design, Content Definition and Content Types, `src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23,24,25`;
the search box, `Views/Admin/List.cshtml:15`; the message, `Views/Admin/List.cshtml:39`.

## Clip `03-import`

Starts on `/Admin/ContentTypes/List`, opened before the capture, in the state `03-switch` left (the feature disabled,
Event missing).

### `import-1`

1. `NavigateAsync("/Admin/DeploymentPlan/Import/Index")`. The page "Import Deployment Package" opens.
2. `MoveToAsync` the file input `#file` (label Select a file), then
   `scene.Page.Locator("#file").SetInputFilesAsync("<downloads>/ContentDefinitions.zip")`. The input shows the file name.
3. `ClickAndWaitForNavigationAsync` on `button.import` (text Import). The page loads again with a message.

On screen when the narration ends: the page "Import Deployment Package" with the message "Deployment package imported."

Labels: Tools, Deployments and Package Import, `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:18,19,25`; the title,
`Views/Import/Index.cshtml:2`; Select a file and Import, `Views/Import/Index.cshtml:8,13`; the message,
`Controllers/ImportController.cs:117`; the redirect that shows it, `Controllers/ImportController.cs:149`.

### `import-2`

1. `PauseAsync(4500)`: the message stays on screen while the first sentence is spoken.
2. `NavigateAsync("/Admin/ContentTypes/List")`.
3. `TypeAsync` into `#search-box`: `Event`. Do not press Enter (it opens the first row; see `switch-2`).

On screen when the narration ends: the page Content Types with `Event` in the search box and one row, display name Event,
technical name Event; no "Your search returned no results." message.

Labels: as `switch-2` (`Views/Admin/List.cshtml:15`, the columns Display name and Technical name `List.cshtml:32-33`).

## The slides (nothing to record)

Each slide is drawn by `slides.py` from the code of the Orchard Core source named here (trimmed with `// ...`).

| Step | The slide shows | Source |
|---|---|---|
| `store-1` | `ContentDefinitionRecord` with its two lists, `ContentTypeDefinitionRecord` and `ContentPartDefinitionRecord` (three files of the folder, so the panel is titled with the folder; the constructors are `// ...`) | `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Records/` (`ContentDefinitionRecord.cs:5-16`, `ContentTypeDefinitionRecord.cs`, `ContentPartDefinitionRecord.cs`) |
| `store-2` | The interface `IContentDefinitionStore` with its three methods | `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/IContentDefinitionStore.cs:5-21` |
| `store-3` | `DatabaseContentDefinitionStore` | `src/OrchardCore/OrchardCore.ContentManagement/DatabaseContentDefinitionStore.cs:6-23` |
| `file-1` | The feature in the manifest, and its startup class | `src/OrchardCore.Modules/OrchardCore.Contents/Manifest.cs:38-44` and `Startup.cs:263-270` |
| `file-2` | The two registrations: `TryAddScoped` of the database store, and `AddFileContentDefinitionStore` | `src/OrchardCore/OrchardCore.ContentManagement/ServiceCollectionExtensions.cs:19,39-44` |
| `file-3` | `FileContentDefinitionStore`, the file name on the record, and how the tenant's path is built (the constructor, then `GetDocumentAsync`, marked as two places) | `FileContentDefinitionStore.cs:7-23`, `ContentDefinitionRecord.cs:5`, `src/OrchardCore/OrchardCore.Data/Documents/FileDocumentStore.cs:22-25,89` |
| `file-4` | A trimmed `ContentDefinition.json`: the type Event with the first five of its `ContentTypeSettings` (the thumbnail path, the other types, the other records and the other settings are `// ...`), and two notes | The shape of the records (`ContentTypeDefinitionRecord.cs`, `ContentTypePartDefinitionRecord.cs`), the settings and their order (`ContentTypeSettings.cs`, which has `ThumbnailPath` after `Securable`) and `ContentDefinitionService.AddTypeAsync` (`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:49-101`, whose `WithThumbnailPath` call is `:90`, default `OrchardCore.ContentTypes/Startup.cs:45`); see OPEN-QUESTIONS.md |
| `file-5` | The lines of `FileDocumentStore` that return nothing when the file does not exist, and the ones that fall back to an empty record | `FileDocumentStore.cs:39-41,90-93` |
| `profile-1` | The `FeatureProfiles` recipe step, trimmed to the one rule that excludes `OrchardCore.Contents.FileContentDefinition` | `src/docs/reference/modules/Tenants/README.md:80-114`; the rule `Exclude`, `src/OrchardCore/OrchardCore/Shell/ServiceCollectionExtensions.cs:83`; the feature Tenant Feature Profiles, on the Default tenant only, `src/OrchardCore.Modules/OrchardCore.Tenants/Manifest.cs:34-40`; profiles read through the Default tenant, `src/OrchardCore/OrchardCore/Shell/FeatureProfilesValidationProvider.cs`; an excluded feature is not available, `src/OrchardCore/OrchardCore/Shell/ShellFeaturesManager.cs:26-50`; the validator is registered for every tenant (`src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:219`, `Shell/ServiceCollectionExtensions.cs:63-67`), and both the Features page (`FeatureService.GetAvailableFeature`) and the recipe `Feature` step (`OrchardCore.Features/Recipes/Executors/FeatureStep.cs:25`) read the available features. The panel title names the recipe step, not a file |

## Calls the scene API does not have

The scene API (`references/recording.md`) has no download or upload helper. The clips use Playwright's page directly, which
the scene exposes as `scene.Page`:

```csharp
// export-3: the form post of Select answers with a file.
var download = await scene.Page.RunAndWaitForDownloadAsync(() => scene.ClickAsync(select));
await download.SaveAsAsync(Path.Combine(Downloads, "ContentDefinitions.zip"));

// import-1: a file input shows its file name once files are set.
await scene.MoveToAsync(scene.Page.Locator("#file"));
await scene.Page.Locator("#file").SetInputFilesAsync(Path.Combine(Downloads, "ContentDefinitions.zip"));
```

`Downloads` is the folder from the environment variable of your own. The recorder's browser context does not turn
downloads off (`Scene.StartAsync`), so they are accepted.

## Checks for private data

- The only name typed on camera is `Content Definitions` (a plan) and `Event` (a type); the account is `admin`, the email
  `admin@example.com`. No password appears: sign in outside the capture.
- Look at every contact sheet (`review.py`) for: the Blog recipe's sample content (it has no real names), the site name
  "Orchard Core", the user menu of the admin, and any browser address that is not `localhost`.
- OCR the final video as `SKILL.md` step 6 asks, and search for real names, emails, phone numbers and tokens.
- Never commit `Seed.zip`, `ContentDefinitions.zip`, `state.json`, the snapshot, or any copy of `ORCHARD_APP_DATA` (it
  holds the database and `ContentDefinition.json`).

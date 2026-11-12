# Recording plan: Write a Deployment Step

Only one clip of this video is a recording: `06-plan`, in the browser. Every other chapter is made of slides, which
`slides.py` draws; they need no site. The narration of every step is in `storyboard.json`.

In this plan, a selector written as `#id` or `a.create` stands for `scene.Page.Locator("#id")` and so on, since the
recorder's methods take locators. File and line references (`src/...`) are in the Orchard Core source this plan was
written from (the `main` branch of `OrchardCMS/OrchardCore`).

## The site it needs

- **Orchard Core:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout of `main`, run on
  `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories
  (`ORCHARD_APP_DATA`). Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Auto Setup:** the Blog recipe, tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, user
  `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed as `VIDEO_PASSWORD`). No setup
  or sign-in form is recorded. Sign in once with `Recorder setup`, which saves `state.json` (git-ignored).
- **Features to enable before recording:** none. The Blog recipe enables `OrchardCore.Deployment`
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:57`), and `OrchardCore.Features`, which provides the
  All Features step, is always enabled (`src/OrchardCore.Modules/OrchardCore.Features/Manifest.cs:17`). Do not enable
  the Feature Profiles feature of the Tenants module: it adds a second step in the Infrastructure category
  (`src/OrchardCore.Modules/OrchardCore.Tenants/Startup.cs:136`, `[RequireFeatures("OrchardCore.Deployment", "OrchardCore.Tenants.FeatureProfiles")]`).
- **Settings, external services, local stand-ins:** none.
- **Seed data:** none. The site must have **no deployment plan**: `/Admin/DeploymentPlan/Index` shows "Nothing here!
  There are no deployment plans at the moment." (`src/OrchardCore.Modules/OrchardCore.Deployment/Views/DeploymentPlan/Index.cshtml:34`).
  The clip creates a plan named `Release package` and a step on it, on camera.
- **Admin menu:** before the capture of `06-plan`, expand **Tools** and **Deployments** in the admin menu, so that
  **Plans** is visible (`src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:18-20`).

## Order and snapshots

1. Provision the site, sign in (`Recorder setup`), stop the site, and copy its `App_Data` folder and `state.json` aside.
   That copy is the snapshot.
2. Record `06-plan`. It is the only clip, and it **changes the site**: it adds a deployment plan with one step.
3. To record it again, stop the site, restore the snapshot, and start the site again. A second take on a site that
   holds the plan `Release package` fails at the plan name (a plan name must be unique:
   `src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/DeploymentPlanController.cs:258`).

## Clip `06-plan`

Starts on `/Admin/DeploymentPlan/Index` (Tools > Deployments > Plans), opened with `scene.OpenAsync` before
`StartCaptureAsync`, signed in as `admin`, with the plan list empty. Keep every step's actions shorter than its
narration. Navigation stops the screencast: use `ClickAndWaitForNavigationAsync` for every click that loads a page.

### `plan-1`

- Actions: `HighlightAsync` on the admin menu link whose name is exactly `Plans`
  (`scene.Page.GetByRole(AriaRole.Link, new() { Name = "Plans", Exact = true }).First`), caption `Plans`, then
  `ClearHighlightsAsync`. `ClickAndWaitForNavigationAsync` on `a.create` with the text `Add Deployment Plan`
  (the Create page, "Create Deployment Plan"). `TypeAsync` into `#Name` the text `Release package`.
  `ClickAndWaitForNavigationAsync` on `button.create` with the text `Create`. The plan opens.
- Visible when the narration ends: the plan page, with the breadcrumb title "Deployment Plan", the green **Execute**
  button, the blue **Add Step** button, and the message "Nothing here! There are no steps on this plan for the moment."
- Labels: `AdminMenu.cs:18-20` (Tools, Deployments, Plans); `Views/DeploymentPlan/Index.cshtml:25` (Add Deployment
  Plan); `Views/DeploymentPlan/Create.cshtml:12,14,20` (Name, Create); `Views/DeploymentPlan/Display.cshtml:12,25,31,57`
  (Deployment Plan, Execute, Add Step, the message). All paths are under `src/OrchardCore.Modules/OrchardCore.Deployment/`.

### `plan-2`

- Actions: `ClickAsync` on the button with the text `Add Step`. Wait until `#modalSteps` is visible, then
  `PauseAsync(500)` for the fade. `HighlightAsync` on `#deployment-step-categories .nav-link` with the text
  `Infrastructure`, caption `Category`.
- Visible when the narration ends: the **Available Steps** dialog, with the **Filter** box, the category list on its left
  (**All** selected, then the categories, among them **Infrastructure**, ringed in green), and the cards of every step.
- Labels: `Views/DeploymentPlan/Display.cshtml:31,68,75,77-79` (Add Step, Available Steps, Filter, All, the category
  list); the category text is `CommonLocalizationSources.Infrastructure`
  (`src/OrchardCore/OrchardCore.Abstractions/Localization/CommonLocalizationSources.cs:29`), set on the step at
  `src/OrchardCore.Modules/OrchardCore.Features/Deployment/AllFeaturesDeploymentStep.cs:15`.

### `plan-3`

- Actions: `ClearHighlightsAsync`. `ClickAsync` on the **Infrastructure** category link. Wait until the card whose text
  contains `All Features` (`#modalSteps .deployment-step-item` filtered by that text) is visible and the other cards are
  hidden. `HighlightAsync` on that card, caption `Thumbnail view`.
- Visible when the narration ends: **Infrastructure** is the selected category, and the only card shown is **All
  Features** with the description "Exports the state of all features." and an **Add** button, ringed in green.
  If other cards are shown, a feature that adds an Infrastructure step is enabled: disable it, and restore the snapshot.
- Labels: `src/OrchardCore.Modules/OrchardCore.Features/Views/Items/AllFeaturesDeploymentStep.Fields.Thumbnail.cshtml:3-4`
  (All Features, Exports the state of all features.); `Views/DeploymentStep.Thumbnail.cshtml:8` (Add) under the
  Deployment module.

### `plan-4`

- Actions: `ClearHighlightsAsync`. `ClickAndWaitForNavigationAsync` on the **Add** link of the All Features card
  (`#modalSteps .deployment-step-item` filtered by `All Features`, then `GetByRole(AriaRole.Link, new() { Name = "Add" })`).
  Wait until `.oc-breadcrumb-title` is visible. `HighlightAsync` on `.oc-breadcrumb-title`, caption `Title`; then
  `ClearHighlightsAsync`, and `HighlightAsync` on the checkbox (`scene.Page.GetByLabel("Ignore disabled features")`),
  caption `Edit view`.
- Visible when the narration ends: the editor page, titled **All Features**, with the breadcrumb Deployment Plans >
  Release package, the unchecked checkbox **Ignore disabled features** with its hint "Check if the disabled features
  have to be ignored.", and the **Create** and **Cancel** buttons at the bottom.
- Labels: `Views/Step/Create.cshtml:38,45-54` (the title is the title of the step, the buttons Create and Cancel);
  `src/OrchardCore.Modules/OrchardCore.Features/Views/Items/AllFeaturesDeploymentStep.Fields.Edit.cshtml:8,10`;
  `src/OrchardCore.Themes/TheAdmin/Views/Breadcrumb.cshtml:40` (the `oc-breadcrumb-title` class).

### `plan-5`

- Actions: `ClearHighlightsAsync`. `ClickAsync` on the checkbox (`GetByLabel("Ignore disabled features")`), so it is
  checked. `ClickAndWaitForNavigationAsync` on `button.create` with the text `Create`. Wait until `#stepOrder` is
  visible. `HighlightAsync` on `#stepOrder .list-group-item`, caption `Summary view`.
- Visible when the narration ends: the plan page, with the success message "Deployment plan step added successfully."
  (it fades after a few seconds: if it is gone, it is not a problem), and one row in the list: the heading **All
  Features**, its hint "Adds all features and their state to the plan. Features should be placed before other steps that
  depend on the state of the sites features.", and the **Edit** and **Delete** buttons at its right, the row ringed in
  green.
- Labels: `Controllers/StepController.cs:112` (the message); `Views/DeploymentStep.Summary.cshtml:12,20` (Edit,
  Delete); `Views/Items/AllFeaturesDeploymentStep.Fields.Summary.cshtml:3,5` of the Features module (above).

### `plan-6`

- Actions: `ClearHighlightsAsync`. `ClickAsync` on the green button with the text `Execute`. Wait until `#modalTargets`
  is visible, then `PauseAsync(500)`. `HighlightAsync` on `#modalTargets .card` filtered by `File Download`, caption
  `Target`. **Do not click Select**: it posts the plan and the browser downloads a .zip file, which the recorder does
  not capture. The narration only describes the download.
- Visible when the narration ends: the **Available Targets** dialog with one card, **File Download**, its description
  "Download a deployment plan locally.", the **Select** button, and **Cancel** in the footer; the card is ringed in
  green.
- Labels: `Views/DeploymentPlan/Display.cshtml:25,115,132,140` (Execute, Available Targets, Select, Cancel);
  `Services/FileDownloadDeploymentTargetProvider.cs:20-21` (File Download, Download a deployment plan locally.).
  The Blog recipe also enables `OrchardCore.Deployment.Remote`, whose target provider adds one card for each remote instance that is configured
  (`src/OrchardCore.Modules/OrchardCore.Deployment.Remote/Services/RemoteInstanceDeploymentTargetProvider.cs:24`). A fresh site has
  none, so only **File Download** is listed. Do not create a remote instance before recording.

## Slide steps

These steps are drawn by `slides.py` and need no recording. The code on the slides is taken from the source of Orchard
Core (paths under `src/OrchardCore.Modules/` unless stated), trimmed with `// ...`.

- `how-1`: three cards, Deployment plan, Deployment sources, Package: from a plan to a package.
- `how-2`: four cards, Step, Source, Display driver and Registration, the first three bright.
- `step-1`: `AllFeaturesDeploymentStep.cs` (`OrchardCore.Features/Deployment/`), the class, `Name` and
  `IgnoreDisabledFeatures` bright.
- `step-2`: the same file, `Category` bright.
- `step-3`: the `Title` of the step, and `FeaturesLocalizationSources.cs` (`OrchardCore.Features/`).
- `step-4`: the same file as `step-1`, the two `[JsonIgnore]` attributes and the two overrides bright.
- `step-5`: three cards on the names a plan stores: the full type name, the class name, and what happens when a class is
  renamed or moved.
- `source-1`: `AllFeaturesDeploymentSource.cs` (`OrchardCore.Features/Deployment/`), the class declaration and the
  `ProcessAsync` signature bright.
- `source-2`: the body of `ProcessAsync`, which adds the `Feature` recipe step to `result.Steps`.
- `source-3`: `CustomFileDeploymentSource.cs` (`OrchardCore.Deployment/Steps/`), with `SetFileAsync` bright, above the
  constructor of `AllFeaturesDeploymentSource`.
- `editor-1`: `AllFeaturesDeploymentStepDriver.cs` (`OrchardCore.Features/Deployment/`), `DisplayAsync` with the Summary
  and Thumbnail shapes.
- `editor-2`: the same driver, `Edit` and `UpdateAsync`.
- `editor-3`: `CustomFileDeploymentStepDriver.cs` (`OrchardCore.Deployment/Steps/`), the `AddModelError` call bright.
- `editor-4`: the Thumbnail and Summary views of All Features (`OrchardCore.Features/Views/Items/`).
- `editor-5`: the view model, and the Edit view of All Features.
- `register-1`: the `Startup.cs` of the Features module, the `AddDeployment` line bright
  (`OrchardCore.Features/Startup.cs:32`).
- `register-2`: the `DeploymentStartup` class of the Roles module, `[RequireFeatures]` and `AddDeployment` bright
  (`OrchardCore.Roles/Startup.cs:70-77`).
- `register-3`: three cards on what a plan does when the feature of a step is disabled
  (`OrchardCore.Deployment/Views/Items/UnknownDeploymentStep.Fields.Summary.cshtml`,
  `OrchardCore.Deployment/Views/DeploymentStep.Summary.cshtml:8`).
- `package-1`: a `Recipe.json` with the `Feature` step added by the All Features source, its `steps` bright
  (`DeploymentPlanResult.cs` in `src/OrchardCore/OrchardCore.Deployment.Abstractions/` builds it).
- `package-2`: `FeatureStep.cs` (`OrchardCore.Features/Recipes/Executors/`), the class and `base("Feature")` bright.
- `package-3`: `FileDownloadDeploymentTargetProvider.cs` (`OrchardCore.Deployment/Services/`).

## Checks for private data

- The site is made from scratch with Auto Setup: its only user is `admin` with `admin@example.com`, and the only plan
  is `Release package`. No real name, email address, phone number, token or password is on screen.
- The password of the user is never typed on camera: the sign-in is in `state.json`, which is git-ignored.
- OCR the video at one frame per second and at every scene change, then at 5 frames per second, and look for anything
  that is not Orchard Core's own text, `admin`, `admin@example.com`, `Release package` and the labels above. The admin
  top bar can show the user name and email: both must be the fake ones.
- Do not commit the MP4, the audio, `state.json` or a copy of `App_Data`.

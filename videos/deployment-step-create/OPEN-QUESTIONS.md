# Open questions: Write a Deployment Step

Everything in the narration was read from the Orchard Core source (the `main` branch, at the commit the catalog was
made from) and from `src/docs/reference/modules/Deployment/README.md`. Nothing was run: no site was started, no module
was compiled, and no voice was generated. What follows is what a person must confirm.

## Pronunciation (before narrating)

`storyboard.json` has a `spoken` map for the identifiers of the narration. None of the spellings was tried with
`scripts/pronounce.py`, which calls the speech service. Check each with a sample before `narrate.py` runs:

- `IDeploymentSource` as "I Deployment Source", `IDeploymentTargetProvider` as "I Deployment Target Provider".
- `DeploymentSourceBase`, `DeploymentStep`, `DisplayDriver`, `DisplayAsync`, `UpdateAsync`, `ProcessAsync`,
  `SetFileAsync`, `FileBuilder`, `FeatureStep`, `AddDeployment`, `RequireFeatures`, `JsonIgnore`,
  `CommonLocalizationSources`, `FeaturesLocalizationSources` and `IgnoreDisabledFeatures`, each spoken as its words.
- `OrchardCore.Deployment` as "Orchard Core dot Deployment", and `Recipe.json` as "Recipe dot JSON".
- The map is applied by plain text replacement, in the order of the file: a key must come before any key that it
  contains. None of the keys contains another one as it is, but keep that in mind when a key is added.

## What to confirm on a running site

1. **The clip `06-plan`.** The selectors of `recording-plan.md` were read from the Razor views, not tried in a browser:
   `#deployment-step-categories .nav-link`, `#modalSteps .deployment-step-item`, `#modalTargets .card`, `#stepOrder`,
   `.oc-breadcrumb-title`, `#Name`, and `GetByLabel("Ignore disabled features")`. Confirm that each finds exactly one
   element (or the intended one), and adjust the plan if the admin theme renders them differently.
2. **The Infrastructure category.** Confirm that on the recording site the category holds only the All Features card
   (the other Infrastructure step needs the Feature Profiles feature of the Tenants module, which the Blog recipe does not
   enable: `src/OrchardCore.Modules/OrchardCore.Tenants/Startup.cs:136`).
3. **The success message of `plan-5`.** "Deployment plan step added successfully." is shown after the redirect to the
   plan (`StepController.cs:112`). Confirm that it is on screen when the narration of `plan-5` ends, or that its absence
   does not matter.
4. **The Available Targets dialog.** The Blog recipe enables Remote Deployment, whose provider lists one target for each
   configured remote instance (`src/OrchardCore.Modules/OrchardCore.Deployment.Remote/Services/RemoteInstanceDeploymentTargetProvider.cs:24`).
   Confirm that a fresh site shows only the File Download card.
5. **The download.** `plan-6` does not click **Select**, because the response is a .zip file and `Scene.cs` has no
   download handling. If a person wants to show the download, confirm that the capture survives it, and then add the
   click to the plan; the narration already says the package is downloaded.
6. **The `Recipe.json` of `package-1`.** The slide is built from `DeploymentPlanResult` and
   `AllFeaturesDeploymentSource`, not from a downloaded package. Download the package of a plan that holds only the All
   Features step, and compare: the keys `name`, `displayName` and the others before `steps`, and a `Feature` step with
   `enable` and `disable` lists. Confirm that `OrchardCore.Admin` is in `enable` and `OrchardCore.Tenants` in `disable`
   on the recording site, or change the two ids of the slide to ids that are.
7. **A step of a class that is renamed or moved (`step-5`).** The claim that existing plans show the step as unavailable
   comes from the code path: a plan is stored with the full type name as `$type`
   (`JsonDerivedTypeInfo.cs`), an unknown `$type` becomes an `UnknownDeploymentStep`
   (`ResilientPolymorphicJsonConverter.cs`), and its summary shows "This deployment step is unavailable because the
   feature that provides it has been disabled or removed." (`UnknownDeploymentStep.Fields.Summary.cshtml`). Confirm it by
   saving a plan with a step of a feature that can be disabled (for example Media), disabling the feature, and
   opening the plan. The same check covers `register-3`: a **Delete** button, no **Edit** button
   (`DeploymentStep.Summary.cshtml:8-20`).
8. **The code of the slides.** It is copied from the source, trimmed with `// ...`, with line breaks added so that a
   line fits the panel (the `Edit` and `UpdateAsync` methods of the driver, the `ProcessAsync` of the source, the
   `FeaturesLocalizationSources` field). The hint string of the Summary view is shortened with "..." because the whole
   line does not fit. Nothing else was changed.

## Not covered on purpose

- The docs call the display driver optional, and `AddDeployment` has a form without one. The Thumbnail and Summary shapes
  of a step are built only by its driver (`DeploymentPlanController.cs:195`), so a step without one is added from a card
  without text. What the dialog and the plan show for such a step was not checked, and the video does not say.
- The recipe step that imports the data of a custom step has its own video; the video only says that its name selects
  the recipe step handler.

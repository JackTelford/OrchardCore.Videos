# Open questions: recipes-write

Everything in the narration was read from the source of Orchard Core (commit `92db4a83`) and from its documentation.
Nobody has run the recipe of this video on a site: the items below are what a person must confirm on a running site
before the clip is recorded. The narration states none of them as more than the code shows. Where an item says
"read in the code", the code path is given, and the person only confirms that the screen agrees.

## Recording setup

1. **The `Bakery.Starter` module builds and loads.** Its project file and manifest in `recording-plan.md` are modeled on
   `src/OrchardCore.Modules/OrchardCore.Recipes/OrchardCore.Recipes.csproj` and on the manifest of the module template
   (`src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Module/Manifest.cs`). Confirm that
   `src/OrchardCore.Cms.Web` builds with the extra `ProjectReference` under the build properties of the checkout, and
   that the module appears on Tools > Features as "Bakery Starter".
2. **The recipe is listed while the module is disabled.** Read in the code: the page harvests the recipes of every
   extension (`src/OrchardCore/OrchardCore.Recipes.Core/Services/RecipeHarvester.cs:29-30`, `ExtensionManager.GetExtensions()`)
   and keeps those whose folder belongs to an available feature (`Controllers/AdminController.cs:160-169`,
   `ShellFeaturesManager.GetAvailableFeaturesAsync`), without looking at what is enabled. Confirm on Tools > Recipes
   that "Bakery Pages" shows under a group headed "Bakery Starter" without enabling the module. If it does not, enable
   the feature "Bakery Starter" on Tools > Features before the snapshot, and change section 2 of the plan to say so.
3. **The group heading reads "Bakery Starter".** Read in the code: the heading is the feature name, which is the `Name`
   of the manifest for a module that has no `Feature` attribute (`FeaturesProvider.cs:87`, `ManifestInfo.cs`:
   `ModuleInfo.Name ?? ModuleInfo.Id`). Confirm the text on screen.

## Running the recipe

4. **The recipe runs without an error on the Blog site.** Read in the code: the `feature` step finds every listed
   feature already enabled and does nothing (`ShellDescriptorFeaturesManager.cs:163-168`); the `themes`, `settings`,
   `ContentDefinition` and `content` steps are traced in `RecipeExecutor.cs`, `ThemesStep.cs`, `SettingsStep.cs`,
   `ContentDefinitionStep.cs` and `ContentStep.cs`. What the code cannot show: the JavaScript engine evaluating
   `new Date().toISOString()` and `uuid()`, and the validation of the imported content item. Confirm that the
   notification says "The recipe 'Bakery Pages' has been run successfully." If a step fails, the notification lists
   its errors: copy them here.
5. **The confirmation dialog.** Read in the code: the Run link carries `data-url-af="RemoveUrl UnsafeUrl"` and the
   texts "Recipes", "Are you sure you want to run the recipe?", "Yes" and "No"
   (`Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:13-17`); the admin theme builds `#confirmRemoveModal` with
   `#modalOkButton` and `#modalCancelButton` from them (`TheAdmin.ts:14-63`, `:72-111`, `:114-187`). The dialog needs
   the antiforgery input of the page, which the form of `Views/Admin/Index.cshtml:30` provides. Confirm that the dialog
   appears and that `#modalOkButton` runs the recipe.
6. **The site settings.** Read in the code: the `settings` step stores the site name (`SettingsStep.cs:76-77`), the
   Blog theme shows `Site.SiteName` in its navigation bar (`TheBlogTheme/Views/Layout.liquid`), and the controller
   releases the tenant after a successful run (`AdminController.cs:140`). Confirm that the front end shows "Greenfield
   Bakery" and that the admin keeps working after the release.
7. **Owner and Author.** Read in the code: the Auto Setup site has a super user (`SetupEventHandler.cs:24`), so
   `parameters('AdminUserId')` and `parameters('AdminUsername')` are filled by
   `src/OrchardCore.Modules/OrchardCore.Users/Services/RecipeEnvironmentSuperUserProvider.cs`. The home page does not
   show them. Nothing to confirm beyond the successful run.

## What the home page shows

8. **The Bakery Page renders.** `run-4` says that the home page shows the title, the text from the file and the opening
   hours. Read in the code: the home route is set by `SettingsStep.cs:108-110`; the Blog theme shows the
   `DisplayText` in its page header (`TheBlogTheme/Views/Content.liquid`) and renders the content zone below it; the
   `HtmlBodyPart` is placed in the `Content` zone (`HtmlBodyPartDisplayDriver.cs`); the text field renders its text only,
   without a label (`OrchardCore.ContentFields/Views/TextField.cshtml`). Not seen: the placement of the field of the
   `BakeryPage` part inside the content zone, and the look of the page. Confirm the three on the page at `/`. If the
   field does not appear, either say so in the plan and keep the narration to the title and the text, or tell the
   author, who will change the narration.

## Narration

9. **A misspelled step name is skipped.** `recipe-3` says that a step whose name no handler recognizes is skipped. Read
   in the code: the executor calls every handler for every step (`RecipeExecutor.cs:215-218`), a named handler returns
   when the name differs (`NamedRecipeStepHandler.cs:19-22`, case-insensitive), the only handler that is not a named one
   is `OrchardCore.DataLocalization/Recipes/TranslationsStep.cs`, which tests two names, and none reports an unknown
   name. Optional check: a copy of the recipe in which a step is named `featur` should still end with the success
   notification.
10. **Spoken forms.** `storyboard.json` "spoken" maps identifiers to how the voice should say them. These spellings are
    new and a person must confirm each with `scripts/pronounce.py` before the narration is generated: "U.U.I.D."
    (uuid), "Content Item I.D." (ContentItemId), "Time Zone I.D." (TimeZoneId), "prefix J.S." (the prefix js),
    "dot recipe dot json" (.recipe.json), "H.T.M.L. Body Part" (HtmlBodyPart), "I Recipe Migrator" (IRecipeMigrator),
    "Display Name" (DisplayName), "H.T.M.L." (Html on its own) and "Add Data Migration" (AddDataMigration). "CORS" in
    `more-2` has no entry: listen to how the voice says it.
11. **The ReloadTenant step.** `more-2` repeats what the source comment of the step says
    (`src/OrchardCore.Modules/OrchardCore.Recipes/RecipeSteps/ReloadTenantStep.cs`): it requests a reload of the
    tenant, and settings that are read at startup, such as CORS policies, otherwise wait for a feature change or a
    restart. Read in the code: the step calls `RequestRelease()`, which releases the shell after the scope
    (`src/OrchardCore/OrchardCore/Shell/DefaultShellReleaseManager.cs`), and the CORS policies are read when the options
    are first built (`OrchardCore.Cors/Services/CorsOptionsConfiguration.cs`). The documentation does not describe this
    step. The behaviour was not run.
12. **A `file:` script in a variable.** `vars-5` says that a variable cannot read a file (the prefix file is for a step).
    Read in the code: a variable is evaluated with no file provider (`VariablesMethodProvider.cs`, the call passes `null`
    for the provider and the base path), and the file engine uses the provider without a check
    (`OrchardCore.Infrastructure/Scripting/Files/FilesScriptEngine.cs:29`), so the read fails. Optional check: a recipe
    with a variable `[file:text('Snippets/welcome.html')]` read by a step should end with an error notification.
13. **Where the steps after `feature` see the new features.** `steps-1` limits the claim to a recipe run from Tools,
    Recipes or during setup. Read in the code: each step runs in a new scope (`RecipeExecutor.cs:196-198`) and a change
    of the shell descriptor releases the shell (`ShellDescriptorManager.cs:133-136`). A recipe that a data migration runs
    uses the scope of the migration (`RecipeMigrator.cs:48`), where this does not hold; the narration says nothing about
    that case. The Blog recipe relies on the same behaviour; no separate check is needed.
14. **When the content step defers.** `reuse-3` limits the claim to the two cases read in the code
    (`ContentStep.cs:26-40`): the shell is not yet activated while the data migrations run
    (`OrchardCore.Data/Migration/AutomaticDataMigrations.cs`, `ShellScope.cs` activation before the scope body), and a
    recipe run from Tools, Recipes finds an activated shell. What a recipe does during setup is not claimed: each step
    of a setup recipe runs after the activation of its own scope, so the import is probably immediate there too, but this
    was not traced to the end.
15. **The `feature` step on the Blog site.** Read in the code: every listed feature is already enabled, so the step
    enables nothing (`ShellDescriptorFeaturesManager.cs:163-168`), but `UpdateFeaturesAsync` always adds the enabled
    features that are enabled only by dependency to the list to disable (`ShellDescriptorFeaturesManager.cs:50-56`) and
    re-enables them as dependencies of the listed features, so the shell descriptor may be updated and the tenant
    released during the step. The narration does not depend on it. Confirm that the recipe still ends with the success
    notification, and note whether the run takes noticeably longer than a recipe without a `feature` step.

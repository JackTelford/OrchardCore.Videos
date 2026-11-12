# Open questions

Things that were read from the Orchard Core source (`OrchardCMS/OrchardCore`, commit 92db4a83) but not seen on a
running site. The narration says nothing about them beyond what is listed under "Claimed in the narration". A person
who records the video confirms each one and writes the answer here, or removes the line.

## Not claimed in the narration

1. **A failed JSON Import may show nothing.** In `src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/ImportController.cs`
   (the `Json` action, lines 163-208) both error branches add to `ModelState` ("The recipe is written in an incorrect JSON
   format.", the step errors, "Unexpected error occurred while importing the recipe.") and then the action ends with
   `RedirectToAction(nameof(Json))`. Nothing in that module or in the MVC core carries the model state across a redirect
   (only the Forms module has such an attribute), so the validation summary and the pasted text may be lost. Check on a
   running site: paste text that is not JSON, select Import, and write down what the page shows. If it shows nothing,
   this is a defect of the code to report upstream; the video does not show or narrate an error for JSON Import.
2. **An unknown step name.** `NamedRecipeStepHandler` ignores a step whose name it does not handle, and the only other
   handler (`TranslationsStep`) is named too, so a step that no enabled feature handles seems to be skipped without an
   error. Check with a recipe that has a misspelled step name. The narration only says that a step runs when a feature
   that handles it is enabled.
3. **Importing the same package twice.** The recipe of the recording plan has a `ContentItemId` and no
   `ContentItemVersionId`, so a second import goes through `DefaultContentManager.ImportAsync` as a new version of the same
   item and removes its existing published and latest versions (`CreateContentItemVersionAsync`, `RemoveVersionsAsync`).
   Check that a second import of the package leaves one **Spring Menu**. Not shown or narrated.
4. **A file without a name.** Selecting Import with no file should show "Please add a file to import."
   (`ImportController.cs:146`); the empty file part may be dropped by model binding before that. Not shown or narrated.
5. **A file of another type**, such as a text file, should show "Only zip or json files are supported."
   (`ImportController.cs:110`). The name test is case-sensitive, so `Recipe.JSON` and `Package.ZIP` are refused. Not
   shown or narrated.

## Claimed in the narration (confirm on a running site)

6. **The two recipes of the recording plan import cleanly.** The package (`Content` step with one Article, no dates, no
   owner) and the JSON text (`Settings` step with `SiteName`) were written from `ContentStep.cs`, `DefaultContentManager.cs`
   and `SettingsStep.cs`, and the shape of the Blog recipe's items; they were not run. `package-4`, `package-5`, `json-3`
   and `json-4` depend on them. Pay attention to the `AutoroutePart` path `spring-menu` (no clash with the Blog
   recipe's paths) and to the order of Content Items.
7. **Spring Menu is at the top of Content Items** (`package-5`). Dates are left out of the recipe so the import sets the
   modification date to the time of the import (`UpdateContentsHandler.cs:34-52`), and the list sorts by it, newest
   first (`DefaultContentsAdminListFilterProvider.cs:306-307`). If the row is elsewhere, `package-5` and the narration
   need to change.
8. **"The site restarts with the next request"** (`idea-3`). `RecipeDeploymentTargetHandler.cs:46` calls
   `ReleaseShellContextAsync` after the last step, and `IShellHost` documents it as "Releases a shell so that a new one will
   be built for subsequent requests". The narration says only that. Check that the page after an import loads, how long it
   takes, and that the recorder stays signed in.
9. **"The steps before it have already run"** (`fail-1`). Each step runs in its own scope (`RecipeDescriptor.RequireNewScope`
   defaults to true) and the first failing step stops the recipe (`RecipeExecutor.cs`). The narration only claims that
   the earlier steps have run and the later ones do not. To see it on a site, import a package whose recipe has a
   `Settings` step that changes the site name, then a `deployment` step with a plan whose step `Type` does not exist, then
   a third step. That step adds an error (`DeploymentPlansRecipeStep.cs:69-76`) that stops the import, and Package
   Import shows "The import failed with the following errors: No changes have been made. The following types of
   deployment plans cannot be added: ..." (`ImportController.cs:123`). A step that throws, such as a `Content` step with an
   item that has no `ContentItemId`, shows the generic text "Unexpected error occurred while executing the 'Content'
   step." after the same prefix (`RecipeExecutor.cs:119`). Confirm that the site name changed and the third step did not run.
10. **The Security Critical badge** (`perm-1`). `_RolePermissions.cshtml:122-125` of the Roles module draws a
    "Security Critical" badge on a permission with `isSecurityCritical`, and `Import` is created with it
    (`DeploymentPermissions.cs`). Check it on Access Control, Roles, edit a role other than Administrator (the
    Administrator role shows a message instead of the list), search the permissions for "Import". Not recorded.
11. **Line numbers in the JSON editor** (`json-1`). `import-json.ts` turns the text area into a CodeMirror editor with
    `lineNumbers: true`. Check that the editor, its line numbers and the Import button show in the admin theme.
12. **A recipe file in JSON format** (`package-3`). The field accepts `.zip,.json` and the action moves a `.json` upload
    to `Recipe.json` (`ImportController.cs:103-107`). Not recorded. Check by saving the recipe text of `json-2` as a
    file and importing it.
13. **Voice.** No `spoken` entry was added. Check "JSON", "Maple Street Bakery" and "Spring Menu" with `pronounce.py`
    before narrating; add an entry only if a person confirms the new spelling.
14. **Delay after an import.** Measured in the dry run of the recording plan, from the click on Import to the success
    message: Package Import ___ seconds, JSON Import ___ seconds. Over 2 seconds, the recording plan asks for the form
    that cuts the wait out ("A slow page after an import"). Also write whether the recorder stayed signed in.
15. **The state of the admin menu** (`package-1`, `package-2`, `package-5`, `json-1`, `json-4`). Read from
    `AdminMenuBehavior.cs` and `sidebar.ts`: the default behavior is Focused, a page loads with only the sections that hold
    it open, and the stored state is ignored. Check on the fresh site that `/Admin` shows every group closed, that
    **Content** is closed on the Package Import page, and that opening a group on camera does not need a second click.

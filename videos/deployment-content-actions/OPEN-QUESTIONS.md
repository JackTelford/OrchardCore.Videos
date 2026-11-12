# Open questions: Export Content From the List

Each item says what was checked in the Orchard Core source (commit 92db4a83, paths under `src/OrchardCore.Modules/`
unless they start with `OrchardCore.Themes/` or `OrchardCore/`, which are under `src/`), what was not, and what a person must confirm on a running site before
or while recording. The narration claims only what the source shows; items 1 to 7, 13 and 14 decide whether a recorded step
matches its narration, items 8 to 12 are behaviors kept out of the narration.

1. **Does enabling "Export Content To Deployment Target" create the plan, and does it pre-select it?**
   Checked: `OrchardCore.Contents/Deployment/ExportContentToDeploymentTarget/ExportContentToDeploymentTargetMigrations.cs:29`
   runs the recipe `OrchardCore.Contents/Migrations/exportcontenttodeploymenttarget.recipe.json` (it creates the plan
   "Export content to deployment plan target", line 19, with one step "ExportContentToDeploymentTargetDeploymentStep"),
   then looks for that plan and, if it finds it, stores its id in the site settings (lines 31 to 40). The recipe runs
   in the scope and the session of the migration (`OrchardCore/OrchardCore.Recipes.Core/Services/RecipeMigrator.cs:48`
   sets `RequireNewScope` to false; `RecipeExecutor.cs:196` then uses the current shell scope), and the step handler
   saves the plan through the same `IDeploymentPlanService`
   (`OrchardCore.Deployment/Recipes/DeploymentPlansRecipeStep.cs:78`), so the lookup most likely finds it and the
   setting is most likely set as soon as the feature is enabled.
   Not verified: that the query sees the plan saved earlier in the same session (YesSql is not in the source folder),
   so whether "Export to Deployment Target" is already in the row menu before Settings, Deployment Targets is saved.
   The narration claims only what holds either way: `features-3` says that the export action uses a deployment plan,
   which the settings choose; `settings-1` says that enabling the feature creates a plan for it; `settings-3` says to
   choose the plan and save; `export-1` says "once the plan is chosen".
   Confirm after `02-features`: (a) Tools, Deployments, Plans lists "Export content to deployment plan target" with the
   step "Export Content To Deployment Target"; (b) whether "Export to Deployment Target" is already in the row menu
   (and "Export to deployment target" in the bulk menu) before Settings, Deployment Targets is saved. If (b) is true,
   the clips recorded before `05-settings` show the extra entry; nothing in their narration contradicts it. If (a)
   fails, do not record `05-settings`: the plan must be created by hand with that name and step, and the narration of
   `features-3` and `settings-1` changed.
2. **Do the four downloads leave the screencast and the page alone?** Steps `download-3`, `download-4`, `export-3` and
   `exportmany-2` start a file download by a POST that returns a file (`DownloadController.cs:60`,
   `ExportFileController.cs:108`). The plan clicks inside `RunAndWaitForDownloadAsync` and expects the page to stay
   where it is. Not verified: that a download started by a submitted form does not stop the screencast. If it does, the
   end frames freeze; record the step again with a click that does not submit a form, or report it in the pull request.
3. **File names.** The View page and the row link save `<content item id>.json` (`DownloadController.cs:83`); a plan
   saved as a file is named from the plan name by `ToSafeName`, which removes spaces and punctuation
   (`OrchardCore/OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:201`), so the export plan gives
   `Exportcontenttodeploymentplantarget.zip`. The narration says only "named after the item's identifier"
   (`download-3`) and "a zip deployment package" (`export-3`). Check both names in the download folder.
4. **What the zip files hold.** The export step reads the posted item ids and adds them to a "Content" step of the
   package (`ExportContentToDeploymentTargetDeploymentSource.cs:42`, `:53`). After `export-3` and `exportmany-2`, open
   the zip files and confirm that the first holds "Apple Harvest Schedule" and the second both selected articles.
5. **Two or more rows for the bulk actions.** The bulk "Actions" menu is shown only when more than one row is checked
   (`Assets/ts/add-to-deployment-plan-bulk.ts:11`, `Assets/ts/export-to-deployment-target-bulk.ts:11`, and the list
   script). The narration says "two or more" (`addmany-1`, `exportmany-1`). Confirm that with one row checked the menu
   stays hidden. Not known: whether this is intended; the docs do not say.
6. **Dialogs opened twice.** `wireDeploymentModalButtons` (bundled in
   `wwwroot/Scripts/deployment-modals/add-to-deployment-plan-bulk.js`, source `@orchardcore/bloom/helpers/deploymentForm`,
   not in this folder) adds a click handler with the option `once` to every "Select" button each time a dialog is
   shown, and the handler keeps the values of that opening (the item id for a row, the checked ids for the bulk
   actions). A handler is removed only when it runs. Opening a dialog, cancelling, and opening it again for another
   item leaves two handlers on each button, so one click submits two forms, one with each item id. The plan opens
   each dialog once per page load. Confirm, and report upstream if it happens.
7. **Bulk: the plan dialog after the confirmation dialog.** The plan dialog is shown from the callback of the
   confirmation dialog, while that dialog is hiding. Confirm that the dialog "Available Deployment Plans" opens
   (`addmany-2`); if not, wait 500 ms after the click on "Ok" before waiting for it.
8. **Drafts: other labels.** An item with a draft shows "View Draft as JSON", "Download Draft as JSON" and, when it has
   a published version too, "View Published as JSON" and "Download Published as JSON"
   (`Views/Items/Download-Button-Actions.SummaryAdmin.cshtml:12` to `:20`), and "Export Draft to Deployment Target" and
   "Export Published to Deployment Target" (`Views/Items/ExportContentToDeploymentTarget-Button-Actions.SummaryAdmin.cshtml:13`, `:16`).
   Not in the video: the seed has no drafts. Confirm on a site with a draft.
9. **Drafts: a quirk of the View page.** The "Download" button of the View page posts without the draft option
   (`Views/Download/Display.cshtml:35`), and `DownloadController.Download` defaults to the published version
   (`DownloadController.cs:60`). After "View Draft as JSON", "Download" would therefore save the published version, or
   return "not found" for an item that was never published. Confirm, and report upstream. The narration says "published
   version" only for an item that has no draft.
10. **Drafts and the bulk actions.** Both bulk actions load the checked rows by document id with `x.Published`
    (`AddToDeploymentPlanController.cs:117`, `ExportContentToDeploymentTargetDeploymentSource.cs:53`); the checkbox value
    of a row is the id of its latest version (`Views/ContentsCheckbox_SummaryAdmin.cshtml:4`). A row with a pending
    draft is probably skipped without a message. The seed has no drafts; confirm the behavior before anyone says more.
11. **Add to Deployment Plan with an unpublished item.** `AddContentItem` loads the item with `GetAsync(id)`, which
    returns the published version (`OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs:102`), so an item that was never published probably gives a
    "not found" page, and a step whose item cannot be loaded shows the badge "Nothing here!"
    (`Views/Items/ContentItemDeploymentStep.Fields.Summary.cshtml:19`). Not in the video.
12. **Remote instances as targets.** `RemoteInstanceDeploymentTargetProvider` lists each configured instance as a card with
    the description "Sends the deployment plan to a remote instance."
    (`OrchardCore.Deployment.Remote/Services/RemoteInstanceDeploymentTargetProvider.cs:27`). No instance is configured for
    the recording, so the card is never shown; `export-2` says only that a configured remote instance appears. Whether
    the file download and the redirect-with-notification behave as the code reads for a remote instance is covered by
    the remote deployment video, not this one.
13. **The search phrase.** Searching Tools, Features for `content items list` matches the name, id and description of a
    row (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`). On the stock source it matches exactly the
    three features (`OrchardCore.Contents/Manifest.cs:49`, `:62`, `:70`). If the recorded site has a module whose
    description contains the phrase, the step `features-1` would show a fourth row; use another phrase from the three
    descriptions and report it.
14. **"All its parts and fields".** `download-2` says the editor shows the content item with all its parts and fields.
    The page serializes the whole item (`DownloadController.cs:53`). Confirm that the part and field names of an
    Article (for example its title and body) appear in the editor.

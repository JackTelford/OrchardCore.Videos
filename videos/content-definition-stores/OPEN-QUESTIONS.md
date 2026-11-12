# Open questions: Content Definition Stores

Everything the narration says was read from the Orchard Core source or the documentation. These points were **not**
run on a site, so a person must confirm them before recording, and change the narration if the site disagrees.

## To confirm on a running site

1. **A site with the feature and no file has no definitions** (`file-5`). Source: `FileDocumentStore.GetDocumentAsync`
   returns nothing when `ContentDefinition.json` does not exist (`src/OrchardCore/OrchardCore.Data/Documents/FileDocumentStore.cs:90-93`),
   and `GetOrCreateMutableAsync` / `GetOrCreateImmutableAsync` then create an empty `ContentDefinitionRecord` (`:39-41`, `:58`).
   Nothing copies the database's definitions: the only users of `ContentDefinitionRecord` and `IContentDefinitionStore` are
   `ContentDefinitionManager`, the two stores and the two deployment sources. To check: on a Blog site, enable File Content
   Definition (Tools, Features), do not import anything, open Design, Content Definition, Content Types, and expect "Nothing
   here! There are no content types for the moment." and no `ContentDefinition.json` in `Sites/Default`.
   `file-5` also says that the way to start from the database's definitions is to export them with a deployment plan (Update
   Content Definitions) before enabling the feature, and to import the package afterwards: the import saves through
   `IContentDefinitionManager`, so the file store receives the types. The seed follows exactly this procedure, so doing the seed
   once also checks the sentence.
2. **The sample file on the `file-4` slide.** The slide shows `ContentDefinition.json` with the type Event. The property
   names are those of `ContentDefinitionRecord`, `ContentTypeDefinitionRecord` and `ContentTypePartDefinitionRecord`, written
   with `JOptions.Indented` (no camel case, nulls left out). The settings are those `ContentDefinitionService.AddTypeAsync` sets
   (Creatable, Draftable, Versionable, Listable, Securable, a thumbnail path and a category). `MergeSettings<ContentTypeSettings>`
   writes the whole object each time, in the order of the class (`ContentTypeSettings.cs`: Creatable, Listable, Draftable,
   Versionable, Stereotype, Securable, Description, Category, ThumbnailPath) and without default values
   (`ContentBuilderSettings.IgnoreDefaultValuesSerializer`), so the object should hold the five settings of the slide, then
   `ThumbnailPath` (`~/OrchardCore.ContentTypes/placeholder.png`, `OrchardCore.ContentTypes/Startup.cs:45`), and `Category` only
   if `DefaultCategory` is configured; the slide ends that object with `// ...` for this reason. The type has its own part, whose
   `Settings` is an empty object (`ContentTypePartDefinitionBuilder.cs:21`: a clone of the part's settings or a new object).
   Open the real file after the seed and compare: the order of the properties, the `Identifier` property of `Document` (not on
   the slide: System.Text.Json is expected to write the properties of the derived class first and those of `Document` last,
   which is why the slide's final `// ...` covers it), whether `ContentPartDefinitionRecords` is empty, and the keys under
   `Settings`. The slide condenses the part record `{ "PartName": ... }` to one line, whereas the file prints each property on
   its own line; the seed imports the Blog types first, so Event is the last type of the real file, which is why the slide
   puts `// ...` before it. Change the slide if the file differs.
3. **The package of `export-3`.** Open `ContentDefinitions.zip` after `03-export`: its recipe should hold a `ContentDefinition`
   step with the types of the Blog recipe and Event, read from the file store (`ContentDefinitionDeploymentSource` uses
   `IContentDefinitionStore.LoadContentDefinitionAsync`, `OrchardCore.ContentTypes/Deployment/ContentDefinitionDeploymentSource.cs`).
4. **Disabling the feature** (`switch-1`, `switch-2`). The tenant is released when the feature list changes
   (`OrchardCore.Infrastructure/Shell/ShellDescriptorManager.cs:133-136` invokes `ChangedAsync`, and `OrchardCore/Shell/ShellHost.cs:174`
   releases the shell), so the next request uses the database store. Expect
   the Blog types and no Event in Content Types. Time the restart: the actions of `switch-1` (about 8 s without it) must
   fit in the narration (about 11 s), or the wait goes in `OffCameraAsync`; the composition in `recording-plan.md`
   (`MoveToAsync`, then `OffCameraAsync` around `WaitForNavigationAsync`) was not tried.
5. **Importing** (`import-1`, `import-2`). Expect "Deployment package imported." and Event in the list afterwards, with
   the Blog types unchanged. The step is `ContentDefinitionStep` (`OrchardCore.ContentTypes/RecipeSteps/ContentDefinitionStep.cs`),
   which saves through `IContentDefinitionManager`, so the active store receives the types.
6. **The actions of the Features row.** The recorder looks for `#btn-disable-OrchardCore_Contents_FileContentDefinition`
   (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53`; the id comes from `GenerateIdFromName`, whose output was not
   run). The default list layout shows the actions as buttons (`AdminListOptions`: `List` and `Buttons`); if the site
   shows a menu, open the row's menu first. Check that the confirmation dialog has the buttons Yes and No
   (`TheAdmin.ts` `confirmDialog`).
7. **Download and upload with the recorder.** The scene API has none, so the plan uses `scene.Page.RunAndWaitForDownloadAsync`
   and `SetInputFilesAsync`. Confirm that the download works in the recorder's headless browser (Playwright accepts downloads
   unless the context turns them off, and `Scene.StartAsync` does not), that the dialog "Available Targets" stays open
   afterwards, and that the file input shows the file name when the files are set. Select is a link that `TheAdmin.ts:115-185`
   turns into a POST form, and only if the page holds a `__RequestVerificationToken` input; if the click navigates to a 405
   page instead, the token is missing from the plan's page.
8. **How the voice reads these words** (check each with `scripts/pronounce.py`, a person confirms a new spelling): the
   `spoken` entries `ContentDefinition.json` as "Content Definition dot JSON", `ContentDefinitions.zip` as "Content
   Definitions dot zip" and `App_Data` as "App Data"; and the spaced forms "I Content Definition Store", "Add File Content
   Definition Store" and "Document Manager". `narrate.py` applies `spoken` with a plain `str.replace`, in the order of the
   entries; the three keys do not overlap.
9. **The site between steps 4 and 5 of the seed.** Once File Content Definition is enabled and before the import, the file
   store has no file and so no content types (`file-5`). Tools, Deployments, Package Import does not read the definitions, so the
   import should work, but nothing was run: confirm that the Package Import page opens and the import ends with "Deployment
   package imported." The front end of the Blog site is expected to be empty or to fail until then; it is not recorded.
10. **Enter in the search boxes.** Not a question but a hazard, found in the source: in the Content Types list Enter with no
    match opens the Create page with `?suggestion=...`, and Enter with a match opens the first row (`recording-plan.md`, clip
    `03-switch`). The Features page ignores Enter. Confirm on the running site that typing alone filters the Content Types list.
11. **Length of the actions.** After narrating, check that the actions of `export-2` (about 10 s of clicks, with the dialog)
   and `switch-1` end before their narrations do.

## Left out of the narration because it could not be verified

- Whether a change made by hand to `ContentDefinition.json` is picked up while the site runs. The record is read from the
  file only when the document cache of the tenant is empty (`OrchardCore.Infrastructure/Documents/DocumentManager.cs`,
  `DocumentOptionsSetup.cs`: no expiration by default), and nothing watches the file; this was not tried.
- Any benefit of the file that the documentation does not state, such as keeping it in source control.
- Creating the Tenant Feature Profiles feature or a profile in the admin, and assigning a profile to a tenant: `profile-1` says only that the feature is on the Default tenant, that a profile excludes features, and that a tenant given the profile cannot enable them (`FeatureProfilesValidationProvider`, `ShellFeaturesManager.GetAvailableFeaturesAsync`, `FeatureService.GetAvailableFeature`); the video does not run it.
- What happens to content items whose type is missing from the definition.
- What an import does to a type that was deleted while the definitions were in the file. `ContentDefinitionStep` updates
  types and parts and never deletes them, so such a type would stay in the database; the video says only that the step saves
  each type and part through the active store.
- Running the file store on several instances of a site. The feature's description is "Stores Content Definition in a local file"
  (`OrchardCore.Contents/Manifest.cs:41`) and nothing more is said.
- The Replace Content Definitions step, which replaces instead of updating.
- `src/OrchardCore.Modules/OrchardCore.Placements/Startup.cs:48` names the startup class of the Placements file storage
  `FileContentDefinitionStartup`, the name of the class in the Contents module. It does not affect the video.

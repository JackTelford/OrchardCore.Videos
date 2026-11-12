# Open questions: Data Migrations in Orchard Core

What was checked in the Orchard Core source (commit `92db4a83`), and what a person must confirm on a running site or
in a module before the video is recorded or published. The narration states only what the source and the
documentation show; the points below are the ones the source cannot settle.

## On a running site (the clip `04-run`)

1. **Search for "Form" before and after.** Before the Forms feature is enabled, the Content Types page should show
   "Your search returned no results." after typing `Form` (the Blog recipe has no type with "form" in its name; the
   Search module's `SearchForm` type is not part of it). After the feature is enabled, the search should list the
   `Form` type. The list is filtered in the browser on the display name of a type and on the text of its row (display
   name, technical name, stereotype and description): the source shows no type of the Blog recipe or of the modules it
   enables with "form" in any of them, but only the page settles it. Confirm that nothing else matches `Form`, before
   and after.
   The page is read in its default List layout (rows with badges); the plan does not change the layout.
2. **Enable button id.** The plan uses `#btn-enable-OrchardCore_Forms`, built by `Html.GenerateIdFromName("OrchardCore.Forms")`
   in `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`. Confirm the id with `Recorder probe`.
3. **The notification.** `OrchardCore.Features/Controllers/AdminController.cs:317` produces "The feature Forms was
   enabled." when one feature is passed to it, and `OrchardCore.Features.Core/Services/FeatureService.cs:79-80` passes
   the features that were requested, so the text names Forms only, with or without the Widgets and Flows features
   enabled. Confirm the text on the page, and that the notification is still on screen when the reloaded page is
   shown (the plan relies on it).
4. **Timing of the migration.** The narration says that enabling a feature recycles the tenant and that the feature's
   migration runs as the tenant starts. The source shows the release of the tenant after the shell descriptor is
   updated (`OrchardCore/OrchardCore.Infrastructure/Shell/ShellDescriptorManager.cs`, the comment after `CommitAsync`;
   `OrchardCore/OrchardCore/Shell/ShellHost.cs:174-175`; the Features controller says the tenant is recycled in
   `GetNextUrl`) and the migrations running in
   `AutomaticDataMigrations.ActivatingAsync`. Confirm that the Content Types page lists the `Form` type on the first
   request after **Enable**, and how long the reload takes (the plan allows `OffCameraAsync` for it).

## In a module (the slides)

5. **The Products example was not compiled.** Its code is assembled from the documentation and from the Alias, Forms
   and Menu modules, with `// ...` where code is left out. Build it once in a module to confirm that it compiles,
   in particular `CreateMapIndexTableAsync<ProductIndex>` with `WithLength`, `AlterIndexTableAsync` with `CreateIndex`,
   and the `Creatable`, `Listable`, `Draftable`, `Versionable` and `Securable` calls (the YesSql package is not part of
   the Orchard Core source that was read).
6. **Two statements come from the documentation only**, since YesSql is a package and not in the source read: "give
   every string column a length, so the migration works on every database", and "a database index makes fetches by
   that column faster" (both are comments of the example in `Migrations/README.md`). The slide's index has one column,
   as in that example. A composite index such as the one of the Alias module (`DocumentId` first) is not claimed to
   speed up queries by its second column, since the source cannot show that.
7. **A failing step.** The narration says only what the code shows: the error is logged
   (`DataMigrationManager.cs:255`, "Error while running migration version ...") and the tenant still starts (the
   exception is caught). It does not say whether the failed step is run again at the next start. After the exception
   the session is cancelled (`:257`) and the migration record is saved in `finally` (`:259-263`). The version of the
   record is set only after the whole chain of steps has run (`:251`), so the steps of that chain that finished before
   the failing one are not recorded either; whether their changes are rolled back (the schema builder uses the
   session's transaction, `:195`) is runtime behaviour. Confirm the outcome with a step that throws on a test module
   before saying more. The narration says that a step that ran never runs again; that holds for a chain that finished.
8. **Recipe files in a published module.** `RecipeMigrator` reads `<module folder>/Migrations/<file>` from the content
   root file provider (`OrchardCore.Recipes.Core/Services/RecipeMigrator.cs:38-40`). The narration says "the
   Migrations folder in your module's folder". Confirm that this holds for a module added as a package and for a
   published site, and how the recipe files are copied, before anyone relies on it.
9. **The `Feature` attribute example.** `[Feature("Products.Indexing")]` needs a feature with that id declared in the
   module's manifest or project file. The slide shows only the attribute and the registration; the declaration of the
   feature is not shown. The Startup class must be public for the attribute to take effect
   (`OrchardCore/OrchardCore/Extensions/ExtensionManager.cs:204`, `:425-428`); the slide's class is.

## Left out of the video, on purpose

10. **`Uninstall` and `UninstallAsync`.** The comment of `DataMigration` (`OrchardCore.Data.YesSql.Abstractions/DataMigration.cs`)
    lists them and `IDataMigrationManager.Uninstall` exists, but no code in the source calls it (not when a feature is
    disabled, not when a tenant is removed). The video does not mention it. When a tenant is removed, the tables are
    found by replaying the `Create` and `UpdateFrom` steps against a recording schema builder
    (`OrchardCore/OrchardCore.Data.YesSql/Removing/ShellDbTablesRemovingHandler.cs:108-165`, with
    `OrchardCore.Data.YesSql.Abstractions/DataMigrationExtensions.cs`, which finds public instance methods only and
    needs a `Create` that returns `int`); the video does not cover that either.

## Pronunciation (`spoken` in `storyboard.json`)

11. Confirm each entry of `spoken` with `scripts/pronounce.py` before narrating: `UpdateFrom1Async`,
    `IContentDefinitionManager`, `AlterTypeDefinitionAsync`, `CreateMapIndexTableAsync`, `AlterIndexTableAsync`,
    `IRecipeMigrator`, `ISession`, `AddDataMigration`, `ConfigureServices`, `CreateAsync`, `ExecuteAsync`,
    `SchemaBuilder`, `DataMigration`, `ProductIndex`, `TitlePart`, `WithPart`,
    `UpdateFrom` and `Task<int>` (said as "Task of int"). The entries are applied in the order of the file, so a
    long name comes before the shorter one it contains.

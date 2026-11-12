# Recording plan: Data Migrations in Orchard Core

The video is a developer video. Eight of its nine chapters are slides drawn by `slides.py`; one chapter is a browser
recording, the clip `04-run`. Read this file from top to bottom and nothing has to be decided.

Paths below are relative to `src/` of an `OrchardCMS/OrchardCore` checkout (the video was written against commit
`92db4a83`). In a step, a selector stands for `scene.Page.Locator(...)`.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore`, run on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Auto Setup** (no setup form and no sign-in form are recorded): recipe **Blog**, site name **Orchard Core**, time zone
  `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git and passed in
  `VIDEO_PASSWORD`; never written in a file of this repository).
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, as `references/recording.md` says. The clip
  calls no external service, and needs no stand-in.
- **Features to enable before recording**: none. The Blog recipe enables `OrchardCore.Widgets` and `OrchardCore.Flows`,
  the two features that `OrchardCore.Forms` depends on (`OrchardCore.Modules/OrchardCore.Forms/Manifest.cs:11-14`).
  `OrchardCore.Forms` (the feature named **Forms**) must be **disabled** when the clip starts: the Blog recipe does not
  enable it (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step).
- **Settings and seed data**: none. The site is used as Auto Setup leaves it; there is no fake data to create. Leave the
  admin list layout at its default, **List** (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`; the
  Blog recipe sets no `AdminSettings`, and `appsettings.json` of the web project has no `List` section). The Content
  Types page then shows rows with badges, not the **Display name** and **Technical name** column headings of the Grid
  layout.
- **Sign-in**: sign in once outside the capture (`Recorder setup`), which saves `state.json` (git-ignored).

### Checks before the first take (outside the capture)

1. `/Admin/Features`: type `Forms` in the Search box. The **Forms** row shows an **Enable** button (not **Disable**).
2. `/Admin/ContentTypes/List`: type `Form` in the Search box. The page shows **Your search returned no results.** If a
   type matches, stop and report it in `OPEN-QUESTIONS.md`: the narration of `run-1` says that nothing is found. The
   search looks at the display name of a type, and at the text of its row (display name, technical name, stereotype and
   description): `OrchardCore.Modules/OrchardCore.ContentTypes/Drivers/ContentTypeEntryDisplayDriver.cs:19`,
   `OrchardCore.Modules/OrchardCore.Resources/Assets/js/list-management.js:225-241`.
3. Copy the site's `App_Data` folder and `state.json` aside (the snapshot, below).

## Order of the clips, and what changes the site

There is one clip to record, `04-run`. It **changes the site**: it enables the Forms feature, and the migration of that
feature creates content types (Form, Label, Input and others) and records its version in the tenant's database.
Disabling the feature afterwards does not remove them. Before every take, stop the site, restore the snapshot of
`App_Data` taken in the checks above, and start the site again.

## Clip `04-run`

Starts on `/Admin/ContentTypes/List` (the **Content Types** page), opened with `scene.OpenAsync` before
`StartCaptureAsync`. Leave the admin menu as the page opens it; do not click it on camera. The Search box has the focus
and is empty.

### `run-1`

- Actions: `TypeAsync(#search-box, "Form")`. Wait until `#list-alert` is visible (the page filters the list in the
  browser as the text is typed). Then `HighlightAsync(#list-alert)` without a caption.
- On screen when the narration ends: the Search box holding `Form`, no row in the list, and the highlighted message
  **Your search returned no results.**
- Labels: the menu path Design, Content Definition, Content Types is in
  `OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25`; the Search box is
  `OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/List.cshtml:15`; the message is `List.cshtml:39`.

### `run-2`

- Actions: `ClearHighlightsAsync()`. `NavigateAsync("/Admin/Features")`. Wait until `#search-box` is visible. Then
  `TypeAsync(#search-box, "Forms")`. Wait until `#btn-enable-OrchardCore_Forms` is visible (the id is the one the view
  builds from the feature id, `OrchardCore.Forms`; if it differs, use the **Enable** button of the **Forms** row, and
  correct this line). Then `HighlightAsync(#btn-enable-OrchardCore_Forms)` without a caption.
- On screen when the narration ends: the **Features** page, `Forms` in the Search box, the **Forms** row (category
  **Content**) with its highlighted **Enable** button. Other rows whose text contains `forms` may remain; the
  highlighted button is the one to click.
- Labels: Tools, Features is `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; the Search box is
  `OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`; **Enable** is
  `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.

### `run-3`

- Actions: `ClearHighlightsAsync()`. `ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_Forms)`. The click
  recycles the tenant (`OrchardCore/OrchardCore/Shell/ShellHost.cs:174-175` releases the shell when the shell
  descriptor changes) and the page is built again by the new tenant, which runs the migration first, so the load can
  take several seconds. If it takes longer than the narration of this step (about ten seconds), replace that call
  with `MoveToAsync(#btn-enable-OrchardCore_Forms)`, then `OffCameraAsync(() => scene.WaitForNavigationAsync(() =>
  scene.Page.Locator("#btn-enable-OrchardCore_Forms").ClickAsync()))`, and wait until the text **The feature Forms
  was enabled.** is visible on the page. The click is then off camera.
- On screen when the narration ends: the **Features** page, reloaded, with the notification **The feature Forms was
  enabled.** visible at the top. The Search box is empty after the reload, so the **Forms** row is not in view; do not
  type in it during this step.
- Labels: the message is `OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`. It names only
  the feature that was clicked, whatever its dependencies: `OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:79-80`
  passes the requested features to the notification. The migration runs when the new tenant is activated, before it
  serves the redirected request (`OrchardCore/OrchardCore.Data/Migration/AutomaticDataMigrations.cs:34-48`).

### `run-4`

- Actions: `NavigateAsync("/Admin/ContentTypes/List")`. Wait until `#search-box` is visible. `TypeAsync(#search-box,
  "Form")`. Wait until the row `[data-type-name='Form']` is visible and `#list-alert` is hidden (the row element
  carries `data-type-name`, `ContentTypeEntryDisplayDriver.cs:20`). Then `HighlightAsync` that row without a caption.
- On screen when the narration ends: the Search box holding `Form`, and one row in the list (List layout): the link
  **Form** (the display name, `Views/ContentTypeEntry.Fields.SummaryAdmin.cshtml:10`), a badge with the technical name
  `Form` (tooltip **Technical Name**, `Views/ContentTypeEntry.DefaultTags.SummaryAdmin.cshtml:11`) and a badge
  **Widget** (tooltip **Stereotype**, `:15`), both under `OrchardCore.Modules/OrchardCore.ContentTypes/`; no **Your
  search returned no results.** message.
- Labels: the type comes from the `CreateAsync` method of `OrchardCore.Modules/OrchardCore.Forms/Migrations.cs:17-181`,
  which returns 5 at line 180; `UpdateFrom1Async` to `UpdateFrom4Async` follow it in the file, and there is no
  `UpdateFrom5Async`, so the version stored for the tenant is 5.

## Slide steps

Nothing to record. Each slide is drawn by `slides.py`; the line says what the viewer sees.

- `why-1`: four cards, the first two bright: data comes first, every tenant differs.
- `why-2`: the same four cards, the last two bright: numbered steps, run in order.
- `class-1`: `Migrations.cs` with `CreateAsync` returning 1, and three notes (base class, first step, return value).
- `class-2`: the same class with `UpdateFrom1Async` added and bright, and three notes.
- `class-3`: three cards: the names of the steps, their return type, and what is ignored.
- `class-4`: three cards: what runs on a tenant with no stored version, one with version 1, and one with the last version.
- `class-5`: the `CreateAsync` of the Forms module returning 5, an `UpdateFrom1Async`, and three notes.
- `reg-1`: `Startup.cs` with `services.AddDataMigration<Migrations>();` bright.
- `reg-2`: a Startup class with the `Feature` attribute registering a migration, and three notes.
- `reg-3`: the constructor of the Menu module's migration, taking `IRecipeMigrator` and `ISession`.
- `cd-1`: `CreateAsync` calling `AlterTypeDefinitionAsync` for the `Product` type with `WithPart("TitlePart")`.
- `cd-2`: `UpdateFrom1Async` setting `Creatable`, `Listable`, `Draftable`, `Versionable` and `Securable`, and three notes.
- `idx-1`: the `ProductIndex` class, and `UpdateFrom2Async` creating its table with `CreateMapIndexTableAsync`.
- `idx-2`: `UpdateFrom3Async` calling `AlterIndexTableAsync` and `CreateIndex` on the `Category` column, and three notes.
- `rec-1`: `UpdateFrom4Async` calling `ExecuteAsync` of `IRecipeMigrator`.
- `rec-2`: the folder tree of the module with `Migrations/sample-products.recipe.json`, and three notes.
- `upd-1`: `UpdateFrom3Async` of the Menu module, which queries, changes and saves content items, and three notes.
- `rules-1`: three cards: keep the numbers, keep earlier steps, add a step.
- `rules-2`: three cards: the record per tenant, renaming the class, `CreateAsync` running again.
- `rules-3`: three cards: the error is logged, the tenant starts, read the log.

### Where the facts of the slides come from

For a reviewer; nothing here is recorded.

- Steps run by name, in a chain, on `public` methods (static or instance) returning `int` or `Task<int>`:
  `OrchardCore/OrchardCore.Data.YesSql/Migration/DataMigrationManager.cs:18` (the binding flags), `:233-243` (the chain),
  `:316-333` (`UpdateFrom` plus a number, with or without `Async`), `:334-354` (`Create`, `CreateAsync`).
- The version is stored per migration class full name, in one record per tenant: `DataMigrationManager.cs:81`, `:212`,
  `:251`, and `OrchardCore/OrchardCore.Data/Migration/Records/DataMigrationRecord.cs`.
- Errors are logged and the tenant starts: `DataMigrationManager.cs:149` and `:255-257`.
- Migrations run when a tenant is activated: `OrchardCore/OrchardCore.Data/Migration/AutomaticDataMigrations.cs:34-48`.
- Registration, as `Scoped<IDataMigration, T>`: `OrchardCore/OrchardCore.Data.YesSql.Abstractions/MigrationServiceCollectionExtensions.cs:11-17`.
  A migration belongs to the feature of the Startup class that registers it (`OrchardCore/OrchardCore.Data.YesSql.Abstractions/IDataMigration.cs:9`,
  `OrchardCore/OrchardCore/Shell/Builders/ShellContainerFactory.cs:115-144`, `:166-194`; a Startup class without the `Feature` attribute belongs to the feature of its module, with it to the feature it names: `OrchardCore/OrchardCore/Extensions/ExtensionManager.cs:418-423`; a migration registered by two Startup classes is refused: `OrchardCore/OrchardCore/Extensions/Features/TypeFeatureProvider.cs:51-54`); a feature Startup is
  `OrchardCore.Modules/OrchardCore.ContentFields/Startup.cs:139-144`.
- `CreateAsync` returning a higher number: `OrchardCore.Modules/OrchardCore.Forms/Migrations.cs:180` (returns 5),
  `OrchardCore.Modules/OrchardCore.Alias/Migrations.cs`, `OrchardCore.Modules/OrchardCore.Menu/Migrations.cs:29` (return 4).
- `AlterTypeDefinitionAsync` creates or changes: `OrchardCore/OrchardCore.ContentManagement.Abstractions/ContentDefinitionManagerExtensions.cs:10-24`.
  The options are `OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentTypeSettingsExtensions.cs`;
  their labels are `OrchardCore.Modules/OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml`.
- Index tables: `OrchardCore.Modules/OrchardCore.Alias/Migrations.cs:19-43` (the Alias index is a composite one; the slide uses the single-column index of the documentation's example, `src/docs/reference/modules/Migrations/README.md`); the index class: `OrchardCore.Modules/OrchardCore.Alias/Indexes/AliasPartIndex.cs:11`.
- Recipes in a migration: `OrchardCore/OrchardCore.Recipes.Core/Services/RecipeMigrator.cs:36-41` (the file is read from
  the `Migrations` folder of the module's folder).
- Updating content: `OrchardCore.Modules/OrchardCore.Menu/Migrations.cs:50-66`.
- "Many migrations change content definitions" (`cd-1`): 26 of the 49 migration files of `OrchardCore.Modules` call `AlterTypeDefinitionAsync`,
  `AlterPartDefinitionAsync` or `IRecipeMigrator`.

## Checks for private data

The site is made from scratch with fake data only, so there is nothing to mask. Still, look at every contact sheet:

- The only user visible is `admin`; no email address appears on these pages.
- The notification and the lists show feature and content type names only.
- No file path of this machine, no password and no token appears on a frame (the browser is headless, so no address bar).
- OCR the finished video as `SKILL.md` asks, at every scene change, and search for names and email addresses other
  than `admin` and `@example.com`.

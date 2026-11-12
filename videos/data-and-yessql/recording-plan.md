# Recording plan: data-and-yessql

A developer video. Almost everything is a slide, drawn by `slides.py`. There is **one** browser clip, `03-setup`, the
Setup screen of a site that is not set up. Source references below are relative to the root of the Orchard Core
repository, at the commit named in `catalog/SOURCE.md`. A selector written as `#Name` stands for
`scene.Page.Locator("#Name")`.

## The site it needs

- `src/OrchardCore.Cms.Web` of Orchard Core, on `http://localhost:5010` (`VIDEO_BASE_URL` otherwise), with
  `ORCHARD_APP_DATA` pointing at an **empty folder** outside both repositories, so the site has never been set up.
  The launch profiles of `src/OrchardCore.Cms.Web/Properties/launchSettings.json` use other ports (2918, 5000, 5001),
  so start the site without a profile and give it the address:
  `ORCHARD_APP_DATA=<empty folder> dotnet run --project src/OrchardCore.Cms.Web -c Release --no-launch-profile -- --urls http://localhost:5010`
  (`ORCHARD_APP_DATA` is `ShellOptionConstants.OrchardAppData` in `src/OrchardCore/OrchardCore.Abstractions/ShellOptionConstants.cs:5`).
- **No Auto Setup.** Set none of the `OrchardCore__AutoSetup__*` variables, and do not add an `AutoSetup` section to
  any configuration file (`src/OrchardCore.Cms.Web/appsettings.json` keeps it commented out). The screen to record is
  the Setup form itself.
- **No sign-in.** Do not run `Recorder setup` or `Recorder prepare`, do not create `state.json`, and do not set
  `VIDEO_PASSWORD`. The clip never signs in, and `Scene` starts without a storage state when the file is missing.
- **No database settings in any configuration source** (`appsettings.json`, `appsettings.*.json`, environment variables
  such as `OrchardCore__DatabaseProvider` and `OrchardCore__ConnectionString`, user secrets, a tenant's own settings
  file). When the provider is set, the Setup screen hides the list "What type of database to use?"
  (`DatabaseProviderPreset`), and when the provider and its connection string are set (for `Sqlite`, the provider
  alone), it hides the whole database block, Table Prefix and Table Schema included (`DatabaseConfigurationPreset`):
  `src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:165-173`,
  `src/OrchardCore.Modules/OrchardCore.Setup/Controllers/SetupController.cs` (`CopyShellSettingsValues`,
  `GetPresetDatabaseProvider`, `HasPresetDatabaseConfiguration`). With none set, `Sqlite` is selected
  (`IsDefault`, `src/OrchardCore/OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:53`).
- Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. The video needs no external service,
  so there is no local stand-in.
- Browser: the recorder's Chromium at 1536x864 CSS pixels, light color scheme, `en-US`.
- **Seed data:** none on the site. Three values are typed into the form **before the capture starts** (see the clip).
  They are fake, and the form is never submitted.

## Order, dependencies and snapshots

- One clip, `03-setup`, with no dependency on any other. In the finished video it plays between the slides of
  `03-providers` and the slide of `03-tenant`.
- It changes nothing on the site: the form is never submitted, so no snapshot of `App_Data` is needed. To record a new
  take, open `/` again; the form starts from its defaults.
- If a take submits the form by mistake (Finish Setup), stop the site, delete the `ORCHARD_APP_DATA` folder, and start
  again from an empty one.

## Clip `03-setup`

Starts on `/`, which shows the Setup screen. Everything before `StartCaptureAsync` is not recorded.

Before the capture (`scene.OpenAsync("/")`, then these, with no ripple and no cursor):

1. `#SiteTimeZone`: select the option with the value `America/Los_Angeles`. The field is filled from the time zone of
   the recording machine (`SetupController.cs:73`), so leave nothing else on screen that reveals where it runs.
2. `#SiteName`: type `Orchard Core`.
3. `#DatabaseProvider`: select the option labeled `Sql Server`; in `#ConnectionString` type
   `Server=db.example.com;Database=Orchard;Integrated Security=true` (the field is masked, and holds no credential);
   leave `#TablePrefix` and `#Schema` empty; then select the option labeled `Sqlite` again. The three fields hide
   again and keep their values, so the connection string is there when `Sql Server` is chosen on camera.
4. Leave the Super User fields empty (user name, email and both passwords). Never press Finish Setup.
5. Wait until the Sqlite state is on screen: `#DatabaseProvider` shows `Sqlite`, and `.connectionString` is hidden.
   Then `StartCaptureAsync()`.

Steps:

- `setup-1`: the narration is "The Setup screen asks What type of database to use? Sqlite is selected, and it needs no
  connection string, so none is asked for."
  - Actions: `MoveToAsync(#DatabaseProvider)` and `HighlightAsync(#DatabaseProvider, "Sqlite")`. Do not click: a
    headless browser draws no open list.
  - On screen when the narration ends: the Setup form with the label "What type of database to use?" above a list that
    shows `Sqlite`, and a green ring around it. No Connection String, Table Prefix or Table Schema field is visible.
  - Labels: `Index.cshtml:176` ("What type of database to use?"), `OrchardCoreBuilderExtensions.cs:53` ("Sqlite").
- `setup-2`: the narration is "Let's choose Sql Server. Three fields appear: Table Prefix, Connection String and Table
  Schema. A hint under Connection String shows a sample for that provider."
  - Actions: `ClearHighlightsAsync()`, then `SelectAsync(#DatabaseProvider, "Sql Server")`, then wait until `.connectionString`
    is visible (the script `src/OrchardCore.Modules/OrchardCore.Setup/Assets/ts/setup.ts:4`, `toggleConnectionStringAndPrefix`, shows it). Then
    `HighlightAsync(#TablePrefix, "Table Prefix")`, `HighlightAsync(#ConnectionString, "Connection String")` and
    `HighlightAsync(#Schema, "Table Schema")` while the field names are said, and finally `ClearHighlightsAsync()` and
    `HighlightAsync(#connectionStringHint)` for the last sentence.
  - On screen when the narration ends: `Sql Server` selected; Table Prefix (empty) beside it, Connection String below it
    (dots, with the eye button), the hint line under it ("The connection string to your database instance, e.g.
    Server=localhost;Database=Orchard;User Id=username;Password=password."), and Table Schema (empty) under that; a green
    ring around the hint.
  - Labels: `Index.cshtml:189` ("Table Prefix"), `:198` ("Connection String"), `:210` ("Table Schema"), the option
    label from `OrchardCoreBuilderExtensions.cs:52` ("Sql Server"). The hint is not in the HTML: it is the
    `data-connection-string-sample` attribute of the selected option (`Index.cshtml:180`, text "The connection string to
    your database instance, e.g. {0}." with the sample connection string of the provider,
    `OrchardCoreBuilderExtensions.cs:52`), copied into `#connectionStringHint` by `setup.ts:37-44` when the list
    changes. `Index.cshtml:204` writes the hint on the server only when the provider is preset, so the span is empty
    until the script runs.
- `setup-3`: the narration is "The table prefix lets several sites share one database. Leave Table Schema blank to use
  the default on the server. The connection string is masked, and the eye button shows it."
  - Actions: `ClearHighlightsAsync()`; `MoveToAsync(#TablePrefix)` for the first sentence (the help text under it
    reads "You can specify a table prefix if you intend to reuse the same database for multiple sites."); then
    `MoveToAsync(#Schema)` for the second (its help text reads "When left blank, the default value on the server will be
    used. For example, 'dbo' for SQL Server."); then `ClickAsync(#toggleConnectionString)` during the third sentence.
    Keep the actions shorter than the narration.
  - On screen when the narration ends: the Connection String field shows
    `Server=db.example.com;Database=Orchard;Integrated Security=true` in clear text, and the eye button shows the
    crossed eye. Table Prefix and Table Schema are still empty, with their help text.
  - Labels: `Index.cshtml:192` and `:213` (help texts), `:201` (the button, titled "Show/hide connection string").
    The click is handled by `setup.ts` (`togglePasswordVisibility`, bound to `#toggleConnectionString`): it sets the
    input type to `text` and swaps the Font Awesome class `fa-eye` for `fa-eye-slash`; the Setup layout loads that
    stylesheet from `OrchardCore.Resources/Vendor/fontawesome-free/css/all.min.css`
    (`src/OrchardCore.Modules/OrchardCore.Setup/Views/_Layout.cshtml`).

## The slides

Slides are drawn by `slides.py`; nothing is recorded for them. What each one shows:

- `docs-1`: three cards: no Entity Framework, a document database interface, and what YesSql stores.
- `docs-2`: one row of the Document table, with the columns Id, Type, Content and Version.
- `docs-3`: the same row, with the Content column bright and a note that the tables stay as they are.
- `index-1`: the `ContentItemIndex` class, trimmed (the first `// ...` stands for the size constants, the second for the
  three date properties), and a card saying an index is a plain C# class.
- `index-2`: the same class with two more cards: regular SQL tables, and the same transaction.
- `index-3`: two cards, map index and reduce index.
- `session-1`: three cards, with the first two bright: save and delete are buffered, and `SaveChangesAsync` flushes the
  changes and commits the transaction.
- `session-2`: the same three cards, with the third bright: the end of the shell scope commits the session.
- `session-3`: a controller that queries `ContentItemIndex` through the session, with the query lines bright.
- `session-4`: two cards, `IContentManager` and `ISession`, and what each one is for.
- `providers-1`: a table of the four providers: the Setup list name, the provider value, the connection string and the
  prefix and schema.
- `providers-2`: three cards: create the database, grant permissions, and what Orchard Core does.
- `tenant-1`: a tenant configuration section with the provider and the connection string, and three cards: the four
  tenant settings, that Setup doesn't ask for the provider and connection string when they are set, and that the
  prefix and schema are hidden along with them.
- `sqlite-1`: three cards about the database file, `DatabaseName` and the tenant.
- `sqlite-2`: the pooling setting in configuration, bright, and two cards.
- `options-1`: the YesSql configuration section, with the section lines bright, and two cards about where it is read.
- `options-2`: the same, with the page size, query gating and isolation level lines bright.
- `options-3`: the same, with the concurrent index creation line bright.
- `options-4`: the same, with the query text in telemetry line bright.
- `tables-1`: the table options section, with the document table and separator lines bright, and two cards.
- `tables-2`: the same, with the separator and identity column size lines bright.
- `sql-1`: three cards: a data migration, `IDbConnectionAccessor` and `IStore`.
- `sql-2`: a class that quotes a table name with the dialect, with the lines that build and quote the name bright.
- `sql-3`: related writes in a transaction, with the commit and the rollback.

## Checks for private data

- The frames hold the Setup form only. Nothing is typed on camera; the values entered before the capture are
  `Orchard Core`, the time zone `America/Los_Angeles`, and a connection string with the host `db.example.com` and no
  user name or password.
- The sample connection string in the hint is the placeholder the product shows
  (`OrchardCoreBuilderExtensions.cs:52`, `username` and `password` are not real credentials); leave it as it is.
- The Super User fields stay empty: no user name, email or password appears in any frame.
- OCR the contact sheet of `03-setup` and the slides' frames for the machine's name, user folder, time zone and any
  address other than `db.example.com` and `localhost`; the recorder's browser shows no address bar, so none should
  appear.
- No file is added to the video's folder except the ones the skill lists: no `state.json`, no copy of `App_Data`.

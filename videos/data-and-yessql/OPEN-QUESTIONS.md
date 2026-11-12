# Open questions: data-and-yessql

What was checked in the source of Orchard Core (the commit in `catalog/SOURCE.md`) and in the documentation, and what a
person must confirm on a running site or by ear. Source references are relative to the Orchard Core repository.

## Statements taken from the documentation, not from the source

YesSql itself is a separate package; its code is not in the Orchard Core source that was read. Where a statement about
YesSql could be checked, it was checked against the XML documentation of the `YesSql.Abstractions` 6.2.0 package, found
in a local NuGet cache (`ISession`, `IStore`, `IConfiguration`, `ISqlDialect`, `Document`). The extract of the Orchard
Core source has no `Directory.Packages.props`, so the YesSql version that the commit references is unknown, and 6.2.0 has
no `ConcurrentIndexCreationEnabled` member although Orchard Core sets it (`OrchardCoreBuilderExtensions.cs:208`): the
referenced version is newer. **Confirm** every YesSql statement below against the version in
`Directory.Packages.props` of the commit.

These statements come from `src/docs/topics/data/yessql.md` and `src/docs/reference/modules/Data/README.md`:

- `docs-2`: "YesSql serializes it to JSON and stores it as a row in a document table". The default table name `Document`
  and the columns `Id`, `Type`, `Content` and `Version` do match the source
  (`src/OrchardCore/OrchardCore.Data.Abstractions/ShellSettingsExtensions.cs` `GetDocumentTable`;
  `src/OrchardCore/OrchardCore.Data.YesSql/DbConnectionValidator.cs` `s_requiredDocumentTableColumns`; the YesSql
  `Document` class has exactly those four members). The row shown on the slide (`42`, the type name, the
  `ContentItemId` and `DisplayText` properties, version `3`) is the documentation's example, trimmed. The `Type` value is
  the full type name, a comma and the assembly name: Orchard Core writes the same shape for another document type in
  `src/OrchardCore/OrchardCore.Queries.Core/QueriesDocumentMigrationHelper.cs:33`
  (`OrchardCore.Queries.Services.QueriesDocument, OrchardCore.Queries`). **Confirm:** open the SQLite file of a set up
  site (a tenant folder under `App_Data/Sites`) with a SQLite viewer, and check that a content item row has that shape.
  Change the slide if it differs.
- `session-1`: "`SaveAsync` and `Delete` are buffered in memory. `SaveChangesAsync` flushes the changes to the database and
  commits the transaction." The documentation says "the SQL commands run in a single transaction when `SaveChangesAsync`
  is called"; the narration follows the XML documentation of YesSql instead, because that sentence is not exact: the
  summary of `ISession.SaveChangesAsync` is "Flushes any changes, commits the transaction, and disposes the transaction",
  `ISession.FlushAsync` "doesn't commit or dispose of the transaction", and `YesSqlStatistics.Flushes` counts flushes
  "whether explicit or triggered by a query", so a query in the same session can send the buffered commands before
  `SaveChangesAsync`. "Buffered in memory" is the documentation's wording only. The Orchard Core side (the session is
  committed by `IDocumentStore.CommitAsync`, which calls `SaveChangesAsync`, before the shell scope is disposed, and
  cancelled by the exception handler of the scope) is in
  `src/OrchardCore/OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs` (the scoped `ISession` registration),
  `src/OrchardCore/OrchardCore.Data.YesSql/Documents/DocumentStore.cs` and
  `src/OrchardCore/OrchardCore.Abstractions/Shell/Scope/ShellScope.cs`. **Confirm:** with the YesSql version that the
  commit references.
- `session-2`: "An exception leaving the scope cancels the changes" is read from the code: the scoped `ISession`
  registration adds an exception handler to the shell scope that calls `IDocumentStore.CancelAsync`, and
  `ShellScope.UsingAsync` runs the handlers when an exception leaves the scope (`ShellScope.cs`, `HandleExceptionAsync`),
  before it runs the commit (`BeforeDisposeAsync`). In `src/OrchardCore.Cms.Web/Program.cs` the exception handler
  middleware comes before `UseOrchardCore()`, so an unhandled exception leaves the shell scope first; an exception that
  application code catches never leaves it, and then the session is committed. `ISession.CancelAsync` is documented by
  YesSql as marking the session "canceled" so that "any following calls to `SaveChangesAsync` will be ignored".
  "A session from any other scope isn't committed for you" is the comment and the condition at
  `OrchardCoreBuilderExtensions.cs:160-164`, and YesSql documents that "Sessions are not automatically committed when
  disposed". **Confirm:** in a controller that saves a document and then throws, check that the document is not in the
  database afterwards.
- `index-2`: index rows recomputed "in the same transaction", and `index-3`: the definitions of a map index and a reduce
  index. Documentation only (the `ContentItemIndex` properties on the slide are from
  `src/OrchardCore/OrchardCore.ContentManagement/Records/ContentItemIndex.cs`; the first `// ...` replaces the
  size constants, the second `ModifiedUtc`, `PublishedUtc` and `CreatedUtc`). The narration says the index maps every content item *version*, as the comment at
  `ContentItemIndex.cs:32` does ("Each version of a content item is a document with one record here"); the
  documentation says "every content item".
- `session-3`: "YesSql runs the filter as SQL on the index table, then loads and deserializes the matching documents".
  Documentation only. The query shape `Query<ContentItem, ContentItemIndex>(predicate)` is used in
  `src/OrchardCore/OrchardCore.ContentManagement/DefaultContentManager.cs` (for example line 347).
- `providers-2`: "Orchard Core validates the connection, then creates its tables. It doesn't create the database
  itself". The validation, and the error "The provided connection string is invalid or server is unreachable.", are in
  `src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs` and `DbConnectionValidator.cs` (a connection that
  cannot be opened gives that error for SQL Server, MySQL and PostgreSQL); the creation of the tables is done by
  `IStore.InitializeAsync` (`OrchardCoreBuilderExtensions.cs`), which YesSql documents as "Initializes the database by
  creating the required tables and the default collection if necessary". **Confirm:** on a
  SQL Server, MySQL or PostgreSQL server, run Setup against a database that does not exist (it must fail with that
  message) and against an empty one (the tables must be created).
- `options-3`: which databases create an index online with `ConcurrentIndexCreationEnabled`. From the documentation and
  the XML comment of `YesSqlOptions`
  (`src/OrchardCore/OrchardCore.Data.YesSql.Abstractions/YesSqlOptions.cs`); the behavior belongs to YesSql and the
  providers. **Confirm:** on a database of each kind, if the claim is to be shown.

## The Setup screen (clip `03-setup`)

Read from `src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml` and
`src/OrchardCore.Modules/OrchardCore.Setup/Assets/ts/setup.ts`, not seen on a running site.

- **Confirm:** with no database setting in any configuration, `Sqlite` is selected, and the Connection String, Table
  Prefix and Table Schema fields are hidden. After choosing `Sql Server`, the three fields are shown, with the hint
  "The connection string to your database instance, e.g. Server=localhost;Database=Orchard;User Id=username;Password=password."
  under Connection String. Choosing `Sqlite` again hides them.
- **Confirm:** the order of the provider list is not claimed anywhere; it comes from the registration order of the
  providers. The narration names only `Sqlite` and `Sql Server` as they are written in the list.
- **Confirm:** the compiled script (`wwwroot`) behaves as `setup.ts` reads, in particular that selecting an option
  with the recorder's `SelectAsync` (Playwright's `SelectOptionAsync`, documented to fire the `input` and `change`
  events) fires the `change` event that shows and hides the fields, and that the hint is filled after the switch.
- **Confirm:** the eye button of Connection String shows the crossed eye after the click: `setup.ts` swaps
  `fa-eye` for `fa-eye-slash`, and the Setup layout loads Font Awesome from `OrchardCore.Resources`.
- **Confirm:** the whole database block fits in the 1536x864 viewport with the Table Schema row visible. If not, scroll
  the page before the capture and say so in the plan.

## The SQLite file name (`sqlite-1`)

"Setup uses OrchardCore.db when none is given" is read from `SetupService.cs` (line 178-181) and matches the
documentation. A tenant with no `DatabaseName` setting at all falls back to `yessql.db`
(`src/OrchardCore/OrchardCore.Data.YesSql/SqliteHelper.cs`); the video does not mention it. **Confirm:** on a site set
up through the Setup screen, the file in the tenant's folder is named `OrchardCore.db`.

## Table naming presets (`tables-1`, `tables-2`)

The behavior is read from `src/OrchardCore/OrchardCore.Data.Abstractions/ShellSettingsExtensions.cs`: the presets are
read from configuration only while the tenant is not set up, `ConfigureDatabaseTableOptions` copies them into the tenant's
settings during setup (`SetupService.cs:154`), and a separator other than underscores or `NULL` throws.
**Confirm:** on a site, set `DefaultTableNameSeparator` and `DefaultDocumentTable` before Setup and check the table
names in the database, if the video is ever extended to show them.

## Pronunciation

`pronounce.py` writes audio, so nothing here was heard. The `spoken` map of `storyboard.json` holds proposed spellings,
each a whole identifier (the narration replaces text literally, so no short key such as "SQL" is used). **Confirm each by
ear, and change the map if a spelling sounds wrong:**

- `YesSql` as "Yes Sequel", and `YesSqlOptions` as "Yes Sequel Options".
- `ISession` as "I Session", `IStore` as "I Store", `IContentManager` as "I Content Manager", and
  `IDbConnectionAccessor` as "I D.B. Connection Accessor".
- `OrchardCore.db` as "Orchard Core dot D.B.".
- The camel case option names (`CommandsPageSize`, `QueryGatingEnabled`, `ConcurrentIndexCreationEnabled` and the
  others) as separate words.
- Not in the map, to listen to: `SQLite`, `MySQL`, `PostgreSQL`, `Sql Server`, `MySql`, `Postgres` (the Setup list
  labels), `Int32` and `Int64`, `NULL`, and `C#`.

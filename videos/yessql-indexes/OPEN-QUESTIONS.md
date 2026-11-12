# Open questions: YesSql Indexes

What was checked in the Orchard Core source (commit `92db4a83`) and in the documentation page, and what a person has to
confirm. Paths are under `src/` of the Orchard Core checkout unless noted.

## 1. What the YesSql library does (not in the Orchard Core source)

YesSql is a NuGet package, so its code is not in the source tree used for the writing. The version that Orchard Core at
this commit references is set in a central package file that is not in the extracted source. The statements below come
from the documentation page `docs/topics/data/yessql.md` and, where it is said, from the XML documentation file of the
package `YesSql.Abstractions` 6.2.0 (found in a local NuGet cache; the DLLs of 6.2.0 have the four declarations, and
versions up to 6.1.0 do not).

Read in the XML documentation of 6.2.0, and consistent with the narration:

- `SaveChangesAsync` flushes the changes and commits the transaction, and a session is not committed when it is
  disposed (`query-2`, including "a session from any other scope is not committed for you");
- `SingleRecordPerDocument` (not grouped, saving throws when the map returns several records), `ExclusiveToDocumentType`
  (a count reads the index tables without joining the document table), `NoIndexOptimizations` and `SkipDeduplication`
  (`declare-1` to `declare-4`);
- the `Delete` delegate of a reduce index returns the updated index, or null to remove it (`reduce-2`, and the code of
  `OrchardCore.Users.Core/Indexes/UserByRoleNameIndex.cs:51-55`);
- a map index associates one document with one record, and a reduce index aggregates several documents into one record
  (`map-1`, `tables-4`).

Not found in that XML, so taken from the documentation page only:

- a map index row has a `DocumentId` column that points to its document (also declared by
  `OrchardCore.ContentManagement/Records/ContentItemIndex.cs:14`, and shown in the clip);
- the index rows are recomputed in the same transaction as the document (`why-4`; the XML says only that `SaveAsync`
  updates the corresponding indexes);
- `SaveAsync` and `Delete` are buffered in memory (`query-2`);
- a query that joins a map index groups its matches by document, and a page reads every match before sorting
  (`declare-1`);
- Map returning null leaves the document without a row (`map-4`). Orchard Core relies on it:
  `OrchardCore.Alias/Indexes/AliasPartIndex.cs:61-70` returns null "to remove index records of soft deleted items".

Check: `ExclusiveToDocumentType`, `SingleRecordPerDocument` and `NoIndexOptimizations` are called in the Orchard Core
source of this commit (`OrchardCore.ContentManagement/Records/ContentItemIndex.cs:36-37`,
`OrchardCore.ContentFields/Services/DefaultContentPickerResultProvider.cs:50`), so the version it references has them.
`SkipDeduplication` is called nowhere in the Orchard Core source: a person confirms that the referenced version has it,
and reads the YesSql source for the statements above.

## 2. Code on the slides was not compiled

- The `ProductIndex` and `ProductIndexProvider` of the slides are the documentation's example plus a `Published` column,
  because every version of a content item is its own document (`OrchardCore.ContentManagement/Records/ContentItemIndex.cs:32-34`,
  `OrchardCore.ContentManagement/DefaultContentManager.cs:419` and `:497-540`). `ProductPart` is not defined anywhere:
  the documentation does not define it either. Confirm the example compiles in a module with a `ProductPart` that has
  `Sku` and `Price`.
- The query `index.Published && index.Price < 10` follows `OrchardCore.Alias/Startup.cs:42-43`.
- The excerpts of `AliasPartIndexProvider`, `UserByRoleNameIndexProvider`, `ContentItemIndexProvider`,
  `DefaultUserPickerResultProvider` and `DefaultContentPickerResultProvider` are trimmed with `// ...`. Each provider
  excerpt carries the name of the file it comes from (`AliasPartIndex.cs`, `UserByRoleNameIndex.cs`,
  `ContentItemIndex.cs`), since the index and its provider share a file.
- The `CreateIndex("IDX_ProductIndex_DocumentId", "DocumentId", "Published", "Price")` call follows the pattern of
  `OrchardCore.Alias/Migrations.cs:32-38`; the column choice is the author's.
- The card "Type" of slide `why-2` shows `OrchardCore.ContentManagement.ContentItem, ...`, the first part of the type
  name in the documentation's table; the documentation adds the assembly `OrchardCore.ContentManagement.Abstractions`,
  where the class lives (`OrchardCore.ContentManagement.Abstractions/ContentItem.cs:9`).

## 3. Interpretation to confirm

- `setup-1` says a provider that needs scoped services implements `IScopedIndexProvider` and is registered with
  `AddScopedIndexProvider` at the session level. The code registers it as a scoped service and adds it to the session
  (`OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:145-158`; `OrchardCore.Data.YesSql.Abstractions/IScopedIndexProvider.cs`,
  `IndexServiceCollectionExtensions.cs:17-23`); "needs scoped services" is the reason for the scoped registration, not
  a sentence of the code.
- `setup-2` says only that the documentation on data migrations advises setting the length of every string column
  (`docs/reference/modules/Migrations/README.md:49`: "otherwise migration will not work for all databases"). The video
  does not say what happens without a length, because Orchard Core's own migrations create string columns without one
  and run on every provider: `RoleName` of `UserByRoleNameIndex` (`OrchardCore.Users/Migrations.cs:47-50`, shown on slide
  `reduce-2`), `LoginProvider` and `ProviderKey` (`:57-59`, and indexed at `:61-65`). A comment there says the default
  falls at 255 on SQL Server (`:19`), and `OrchardCore.Alias/Migrations.cs:143` mentions an index key of 767 characters.
  A person decides whether the reason needs a sentence once the YesSql source has been read.
- `setup-3` says Orchard Core runs the migration when the tenant starts. The documentation says "on application start"
  (`docs/reference/modules/Migrations/README.md:10`); the code runs the data migrations of every enabled feature when a
  tenant shell is activated, unless the tenant is uninitialized (`OrchardCore.Data/Migration/AutomaticDataMigrations.cs:34-45`).
  The video does not say what happens when a feature is enabled later. "Speeds up those queries" is the documentation's
  comment on `CreateIndex` (`docs/reference/modules/Migrations/README.md:55-56`). The `IndexName` of the example starts with `DocumentId`, as the one of
  `OrchardCore.Alias/Migrations.cs:113-120` does, but the XML documentation of `ExclusiveToDocumentType` notes that on
  MySQL an index that starts with `DocumentId` can make a filtered count slower. A person may choose another order.
- `query-2`: the session is committed by `IDocumentStore.CommitAsync`, which calls `SaveChangesAsync`
  (`OrchardCore.Data.YesSql/Documents/DocumentStore.cs:115-124`), registered before the shell scope is disposed, and only
  when the session was resolved from the service provider of the shell scope itself
  (`OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:160-175`). "A session from any other scope is not committed
  for you" paraphrases the comment at `:160-161`.
- `map-3` says `ProductIndex` has a row for every version, not that every index of content items does:
  `AliasPartIndex` returns null for a version that is neither published nor the latest (`AliasPartIndex.cs:61-65`).
  `ContentItemIndex` has a row for every version (comment at `ContentItemIndex.cs:32`).
- `reduce-1` says Map emits a row for each role of a user. When a user has no role, the real map emits one row with the
  role `Authenticated` as a marker (`OrchardCore.Users.Core/Indexes/UserByRoleNameIndex.cs:27-37`); the slide hides that
  branch behind `// ...`.
- `tables-4` says a reduce index row does not point to one document but aggregates a group of them (the documentation:
  "reduce indexes use a separate table to associate documents with aggregated rows"; the XML documentation of
  `ReduceIndex`). The class `UserByRoleNameIndex` and its table (`Users/Migrations.cs:47-50`) declare no `DocumentId`,
  and the plan expects none in the result: a person confirms it on screen.

## 4. On a running site

Confirm with the recorder (`Recorder probe`) on the site of `recording-plan.md`:

- the selectors of the admin menu (`button.item-label.nav-group-label`, `ul[data-title='...']`, `a.item-label`), read
  from `OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml` and `NavigationItemLink-admin.cshtml`;
- `span.hint a` (the "click here" link, whose address has the key `Query` with a capital Q),
  `span.text-muted` ("Query executed in ... ms"), `#monaco_editor` and `#monaco_editor .view-line` (Monaco draws its
  lines with this class: it is not in the Orchard Core source);
- `SELECT * FROM ContentItemIndex` returns rows with a `DocumentId` key, and the editor shows them as formatted JSON;
- after Auto Setup with the Blog recipe, `SELECT * FROM UserByRoleNameIndex` returns one row with `RoleName` and `Count`.
  The order is checked in the source: the recipe runs first and enables Roles
  (`OrchardCore.Setup.Core/Services/SetupService.cs:248`), then the shell is reloaded and the setup event handlers run,
  which create the user `admin` with the role Administrator (`:270-276`;
  `OrchardCore.Users/Services/SetupEventHandler.cs:31`), and the index provider is registered when the Roles feature is
  enabled (`OrchardCore.Users/Startup.cs:371-378`). What a person confirms is that the row shows;
- the whole page fits the viewport as described for each step of the clip, with no scroll bar on the Template editor.

## 5. Pronunciation

`storyboard.json` "spoken" maps the identifiers to words. A person confirms with `scripts/pronounce.py` (it was not run
here) how the voice says:

- "YesSql" spoken as "Yes Sequel", "ORM" as "O.R.M.", "DocumentId" as "Document I.D.", "Sku" as "S.K.U." (it could also
  be said as a word), "RoleName" as "Role Name";
- "SQL", "SQLite", "MySQL" and "PostgreSQL", left as written;
- the identifiers split into words ("Create Map Index Table Async", "I Session", and so on).

## 6. Words

The narration says "Latest" nowhere; the slides show the property `Latest` of `AliasPartIndex` because it is the
class's code (`OrchardCore.Alias/Indexes/AliasPartIndex.cs`).

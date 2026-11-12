# Open questions: Content Fields Indexing

What was checked in the Orchard Core source (commit `92db4a83`), and what a person must confirm on a running site
before the video is recorded or published. The narration only states what the source shows; each item below is a
point the source cannot settle. Paths start at `src/OrchardCore.Modules/`.

## 1. Content saved before the features were enabled

Checked: `OrchardCore.ContentFields/Startup.cs:139-157` registers the index providers and a data migration, and the
migration (`OrchardCore.ContentFields/Indexing/SQL/Migrations.cs`) creates empty tables. Nothing in the module walks
existing content items. The comment at `Migrations.cs:301` says a column added later "will not be populated until the
content item is republished". The providers map a content item when it is saved
(`.../Indexing/SQL/TextFieldIndexProvider.cs:35-85`).

The narration (`enable-4`) therefore says only that content items are indexed as they are saved.

To confirm: on a site where the Blog recipe's posts already exist, enable Content Fields Indexing (SQL), run
`SELECT * FROM TextFieldIndex` in Run SQL Query, and note whether the existing posts have rows or only the items saved
afterwards. If they have none, say so in a later revision of `enable-4`; do not add it before it is confirmed.

## 2. Run SQL Query accepts the two prefilled queries

Checked: the page only accepts `SELECT` statements and adds the table prefix
(`OrchardCore.Queries/Sql/SqlParser.cs:61-116`, `.../Views/Admin/Query.cshtml:47`). The parser is an external library, so
the source does not show whether it accepts every identifier.

To confirm, before recording `04-sql` (see `recording-plan.md`):

- `SELECT * FROM TextFieldIndex WHERE ContentType = 'Event' AND ContentField = 'Venue' AND Published = 1` returns 3 rows.
- `SELECT DocumentId, BigValue FROM MultiTextFieldIndex WHERE ContentType = 'Event' AND ContentField = 'Topics' AND Published = 1` returns 5 rows.
- `Published = 1` is understood by the database of the recording (SQLite). The narration never says which value a
  boolean column holds.
- An address with a `query` value (the seed check, and `sql-4`) opens with `Final SQL Query` and the result, without a
  click on `Query`, while the address without a value (`sql-1`, `sql-2`) shows only the form. The source says so (the
  `GET` action in `OrchardCore.Queries/Sql/Controllers/AdminController.cs:43-55` calls the action that runs the query,
  and returns the bare view when the text is blank, `:67-70`).
- In `sql-1` and `sql-2` the `Template` text is set with `CodeMirror.setValue` before the capture, and `Query` posts that
  text (the first `.CodeMirror` is a CodeMirror 5 editor that copies its text back to the textarea on submit;
  `OrchardCore.Resources/wwwroot/Scripts/codemirror/codemirror.js`). The module script that makes the editor runs after
  the page loads, so the plan waits for `.CodeMirror` first. If the result of `Query` does not show the first query's
  3 rows, stop and read this item.
- The two rows of the event with two topics are next to each other, and the first object of the first result shows
  `Id`, `DocumentId`, `ContentItemId`, `ContentItemVersionId`, `ContentType`, `ContentPart`, `ContentField`, `Published`,
  `Latest`, `Text` and `BigText` within the first screen of the result editor. The editor formats the JSON about
  300 ms after loading (`OrchardCore.Queries/Assets/ts/queries-admin-query.ts`).

If a query is rejected, or the columns come back in another order, the narration of `sql-2` to `sql-4` must be revised
before recording. Do not change the queries in the plan without revising the narration.

## 3. The Features page

Checked: the markup of `OrchardCore.Features/Views/Admin/Features.cshtml` and
`.../Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`. The button ids are built by `Html.GenerateIdFromName`, whose
code is in ASP.NET Core and not in the source tree; the plan assumes each `.` of the feature id becomes `_`.

To confirm: `Recorder probe` prints `btn-enable-OrchardCore_ContentFields_Indexing_SQL` and
`btn-enable-OrchardCore_ContentFields_Indexing_SQL_UserPicker`; searching for "Content Fields Indexing" shows exactly
two rows ("Showing 2 of ..."), which `enable-1` says ("Two features match"); the List layout is the default; and each
`Enable` click shows the notification quoted in the plan.

## 4. The seed recipe

Checked: the shapes come from the Blog recipe (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`), the
`Content` step (`OrchardCore.Contents/Recipes/ContentStep.cs`, `Data` array), the field classes (`TextField.Text`,
`NumericField.Value`, `MultiTextField.Values`) and `DefaultContentManager.cs:1014-1017` (a missing `ContentItemVersionId`
is generated when an item is imported). The recipe was not imported on a running site.

To confirm: Tools > Deployments > JSON Import accepts the recipe of `recording-plan.md`, and the three events are
published (Contents > Content Items).

## 5. Code on the slides

- `code-1` (`EventQueries`) follows `OrchardCore.ContentManagement/DefaultContentManager.cs:262` and
  `OrchardCore.DataPipelines/BackgroundTasks/DataPipelineRunsBackgroundTask.cs:55` (`Query<T, TIndex>(predicate)`,
  a comparison on a nullable column, `ListAsync()`), with the namespace and types from
  `OrchardCore.ContentFields/Indexing/SQL/`. It was not compiled. To confirm: compile it in a module that references
  `OrchardCore.ContentFields`, and run it against the seed (it must return "Orchard Core Meetup" and "Developer Workshop"
  for a limit of 100).
- `code-2` (Razor) follows the documentation, with the namespace and the `using YesSql;` line corrected. The Liquid
  half comes from the documentation (`{{ country.Text }}`): confirm that `venue.Text` prints the venue, with the saved
  query `EventVenues` = `SELECT * FROM TextFieldIndex WHERE ContentType = 'Event' AND ContentField = 'Venue' AND Published = 1`
  and Return Documents unchecked.
- `graphql-2` is the documentation's example (`product`, `PricePart`, `Amount`). The code builds the nested part
  filter and the `_gt` suffix for Numeric fields
  (`OrchardCore.ContentManagement.GraphQL/Queries/Types/DynamicContentTypeBuilder.cs`,
  `OrchardCore.ContentFields/GraphQL/Fields/ContentFieldsProvider.cs`). To confirm: run it in GraphiQL on a site with
  a Product type that has a Price part with a Numeric field named Amount.

## 6. Versions with rows

`tables-4` says only the published version and the newest version have rows. Source: the providers return no row when
a version is neither published nor the latest (`TextFieldIndexProvider.cs:37-41`), and `DefaultContentManager.cs`
clears `Latest` on the previous latest version and `Published` on the previous published version when a version is
created or published (lines 419, 521, 576). To confirm: publish "Developer Workshop", save a draft of it, and check that
`TextFieldIndex` has one row with `Published = 1, Latest = 0` and one with `Published = 0, Latest = 1`, and none for
older versions.

## 7. Pronunciation

`storyboard.json` has a `spoken` map: "Content Item Id", "Version Id", "User Id" and "Document Id" are spoken with
"I.D.", and "Url" as "U.R.L.". A person must confirm these with `scripts/pronounce.py` before narrating, together with
how the voice reads "SQL", "GraphQL", "Big Text", "Date Time", "Multi Text" and "Selected User Id".

# Open questions: SQL Queries

What the author read in the source and the documentation but could not run. A person checks each point on the
recording site before recording, and corrects `storyboard.json` or `recording-plan.md` if the site behaves
differently. The narration states only what the source shows; the points below are about screens and exact texts.

## The editors

1. **Template and Parameters** are CodeMirror editors (`Views/Admin/Query.cshtml:44,54`, class
   `codemirror-query-editor`, started by `Assets/ts/queries-admin-query.ts` from the shared component of the admin
   theme, which is not in the source tree read). Not verified: whether the editor adds a closing quote, bracket or
   brace while typing, whether it indents new lines, and whether the wrapper element exposes the editor object as
   `.CodeMirror`. `recording-plan.md` ("Putting a statement in an editor") gives three methods and the test to pick
   one; the person records which one worked.
2. After a run, the controller writes the parameters with their defaults back into the model
   (`Sql/Controllers/AdminController.cs:88-89`), but the text area is bound to the posted value, so the Parameters
   editor may show the text posted instead. The narration does not depend on it; check what `param-2` shows.

## The statements on the recording site

3. `Published = true` (used in `S2`, `S4`, `S5`, as in the documentation) is rewritten by the parser to the literal
   of the database (`Sql/SqlParser.cs`, `RenderLiteral`, which calls `GetSqlValue` of the YesSql dialect, a library
   that is not in the source tree read). Check that it returns rows on SQLite. The documentation page of the Queries
   module writes `Published = 1` in its Blog theme example; if `true` fails, use `1` in the statements, in
   `recording-plan.md`, and in the Base64 values.
4. `{% if limit > 0 %}limit @limit{% endif %}` (`S5`): check that it renders with `limit` set to `2` (two rows) and
   that the statement still runs when `limit` is not supplied (the condition is false). The documentation shows the
   same pattern. Check that the **Final SQL Query** field shows a limit clause for the first case.
5. The refused statement `S3` (`delete from ContentItemIndex where 1 = 0`): the source says the message is "Only
   SELECT statements are supported." (`Sql/SqlParser.cs:57,82`) and that it is added to the Template field
   (`Sql/Controllers/AdminController.cs:104-108`). Check that it shows in red under the Template, and that no row
   is touched. When the statement does not parse, the page shows the messages of `SqlParser.Validate` first
   (`AdminController.cs:104-106`), so a validator error could replace the sentence; the narration says only "a
   message under the Template". The text of a syntax error ("Parse error: ..., at line N, column M") comes from the SQL library; the
   video says only that a line and a column are reported, and no syntax error is recorded.
6. **Final SQL Query**: the exact text (quotes, case, trailing semicolon) depends on the database. The narration says
   names are quoted, the prefix of the site is added, and the SQL is converted to the dialect of the database
   (`Sql/SqlParser.cs`, `OrchardSqlRewriter`, `OrchardSqlDialect`). The recording site has no table prefix, so no
   prefix is visible; check that nothing on screen contradicts the sentence.
7. **Run SQL Query hint link**: `Views/Admin/Query.cshtml:46` links to the same page with the statement in Base64, and
   the `[Admin("Queries/Sql/Query")]` action passes it to the code of the **Query** button
   (`Sql/Controllers/AdminController.cs:44-54`), so the link runs the statement. The narration of `run-2` says
   "which runs at once" and `run-3` does not click **Query**. Check that, after the click, the Template holds
   `SELECT * FROM ContentItemIndex` and **Final SQL Query** and the rows are below it. The same holds for the address
   of `05-save` and for the **Run** button of a saved query (`save-6`).

## Seed data

8. The import of `recording-plan.md` (JSON Import, `Content` step): the source keeps the `CreatedUtc` of an imported
   item (`OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:37-40` sets it only when it is empty).
   Not verified: that the validators accept the five items (the `garden/` paths of `AutoroutePart`, the missing
   `ContainedPart`), that the draft stays a draft (`Published` false in `ContentItemIndex`), and that the import
   reports success. If `CreatedUtc` were rewritten to the time of the import, the posts would come back in the reverse
   order of the import, with the recipe's own post last: the narration ("newest first") would still be true, but the
   table of expected rows in the plan would change. The recipe's own post is newest only if the site is set up after
   the dates of the seed (28 October 2026).
9. The Blog recipe's "About" article is the single row expected for `contenttype` set to `Article`; check that its
   `ContentType` is `Article` and that it is published.

## Documentation-backed claims that the source does not show

10. The slide of `convert-1` lists `second`, `minute`, `hour`, `day`, `month`, `year` and `now()` (UTC) from the
    "Helper functions" table of the Queries page. The source only forwards function names to the database dialect
    (`Sql/SqlParser.cs`, `RenderFunction`), and the conversion itself is in the YesSql library, which is not in the
    source tree read. A person can try `select year(CreatedUtc) from ContentItemIndex` on the Run SQL Query page.
    The documentation's grouped example (aliases in square brackets, `now()` in a where clause) was not tried
    either, and is not shown.
11. `random()` and the `LIMIT`/`OFFSET` conversion are in the source (`Sql/SqlParser.cs`, `RandomFunctionName`,
    `GetLimit`); what each database produces is not verified.

## Permissions

12. **Role editor**: the slide of `access-1` and the narration say the role editor marks Manage SQL Queries as
    Security Critical (`OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:122-125`, from the third argument of
    `QueriesPermissions.ManageSqlQueries`), and that the default permissions of the Administrator and Editor roles
    include it (`Sql/Permissions.cs`, `GetDefaultStereotypes`). The narration says only what the provider declares.
    Not verified: whether the **Editor** role of a site made by the Blog recipe holds it. The recipe's `Roles` step
    creates Editor with an empty `Permissions` list, and `OrchardCore.Roles/Services/RoleUpdater.cs` adds the
    stereotype permissions when a role is created or a feature is installed, which depends on the order of the
    setup. Check on **Access Control**, **Roles**, **Editor**; if Editor does not hold it on the recording site, the
    sentence is still true of the default permissions, and the slide of `access-1` stays as it is.
13. Saving a SQL query needs **Manage queries** (`Controllers/AdminController.cs:119,144`), not Manage SQL Queries: the
    SQL editor driver (`Sql/Drivers/SqlQueryDisplayDriver.cs`) checks no permission. **Manage queries** also implies
    `ExecuteApiAll` and every `ExecuteApi_{name}` permission (`OrchardCore.Queries.Abstractions/QueryPermissions.cs:19-25`,
    checked at `Controllers/QueryApiController.cs:52`). The narration of `access-1` and `access-2` states exactly this:
    "Manage queries is a separate permission that creates, edits, and deletes saved queries" and "Manage queries can
    also save a SQL query and run it through the API". Not verified at run time: confirm with a role that holds
    Manage queries but not Manage SQL Queries that it can save a SQL query, that **Run** then answers with a refusal,
    and that `api/queries/PublishedBlogPosts` answers for an authenticated caller with that role. Not shown on camera.

14. **Save-time validation**: saving a SQL query without Liquid syntax runs `SqlParser.Validate` on the statement
    (`Sql/Drivers/SqlQueryDisplayDriver.cs:97-103`); the validator is in the `Cyqwel` library, which is not in the
    source tree read. Check that `S2` (`Published = true`, no Liquid) is accepted, so that **Save** in `save-5`
    creates the query. The narration does not describe this validation.
15. **Return Documents** (`save-3`): the source reads the `DocumentId` column of each row, or else the first column
    that holds a `long`, or else the first column (`Sql/SqlQuerySource.cs:100-118`), so `S2`, which also selects `DisplayText` and `CreatedUtc`,
    returns content items. Nothing is recorded that runs the saved query; check, outside the capture, that **Run**
    on the saved `PublishedBlogPosts` shows rows of values (the editor of the Run SQL Query page never loads content
    items; it shows the rows as they are).

## Voice

16. Pronunciations to confirm with `scripts/pronounce.py` before narrating: "SQL" (letters or "sequel"; the video
    uses it throughout), "JSON", "GraphQL", and the spellings of the `spoken` map: "Document I.D." for `DocumentId`,
    "Created U.T.C." for `CreatedUtc`, "Content Item Index", "Display Text", "Blog Post", "Content Type", and "SQL"
    for the value `Sql`. The narration avoids the word "ids" and says "document identifiers" instead.

## Menu

17. **Search**, **Queries**, **Run SQL Query** are the labels of the menu builders (`Sql/AdminMenu.cs:18-20`). How the
    admin menu of the theme expands them (click, hover, or all open) is not in the source tree read; `run-1` of
    `recording-plan.md` clicks each in turn. If the entry is reached another way, adjust `run-1` only; the narration
    does not describe the clicks.
18. The selectors of the saved-query form (**Name**, **Return Documents**, **Schema**, **Save**) are built from the
    labels and classes in `Views/Query.Fields.Edit.cshtml`, `Views/SqlQuery.Edit.cshtml` and
    `Views/Query.Fields.Buttons.cshtml`; the generated `id` and `name` attributes depend on the display driver prefix
    and were not read.

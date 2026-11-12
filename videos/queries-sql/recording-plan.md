# Recording plan: SQL Queries

How to record this video without having to decide anything. The narration is in `storyboard.json`; this file holds the
site, the seed data, the order of the clips, and every action. A selector written here stands for
`scene.Page.Locator(...)`. Source references are paths under `src/OrchardCore.Modules/OrchardCore.Queries/` of the
Orchard Core checkout, unless another module is named; the libraries `OrchardCore.ContentManagement`,
`OrchardCore.Queries.Abstractions`, `OrchardCore.Queries.Core` and `OrchardCore.Mvc.Core` are under `src/OrchardCore/`.

The recorder drives a browser only. The video therefore has recordings of the Run SQL Query page and of the
saved-query form, and slides for everything else (the table, the parser, the recipe step, the permission).

## The site it needs

- **Auto Setup**: the **Blog** recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, the default
  tenant, the default database provider (SQLite) and **no table prefix** (leave `DatabaseTablePrefix` empty). The
  super user is `admin` with email `admin@example.com`; the password is chosen outside git and passed in
  `VIDEO_PASSWORD` (`<password>` in every command). No setup or sign-in form is recorded.
- **Features**: nothing to enable. The Blog recipe already enables `OrchardCore.Queries`, `OrchardCore.Queries.Sql`
  (feature name **SQL Queries**) and `OrchardCore.Deployment` (for **JSON Import**), and it creates the SQL query
  `RecentBlogPosts` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `queries` step). Do not enable
  GraphQL, Lucene or Elasticsearch: the video does not use them.
- **Settings**: none. The admin list layout stays at its defaults (List layout, row actions as buttons), so the
  queries list shows **Run**, **Edit** and **Delete** as buttons.
- **External services**: none. Block outbound calls anyway with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Browser**: Chromium at the recorder's default viewport, signed in once outside the capture (`Recorder setup`).
- **Names**: the query the video saves is `PublishedBlogPosts`. It must not exist before the `05-save` clip. Do not
  reuse `RecentBlogPosts` (it exists, and a duplicate name is refused with "A query with the same name already
  exists").

### Seed data (outside the capture)

The Blog recipe creates one `BlogPost` ("Man must explore, and this is exploration at its greatest") and one
`Article` ("About"). The video needs more rows, so import the posts below once, before the snapshot:

1. Open **Tools**, **Deployments**, **JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:18-30`, page
   `Views/Import/Json.cshtml:3`).
2. Paste the JSON below into the **JSON** field and click **Import**.
3. Open **Content**, **Content Items**, filter on Blog Post, and check that the five new posts are listed (four
   published, one draft: "Spring Planting Plan").

The posts are imported oldest first, so the order is the same whether or not the import keeps the `CreatedUtc`
values (see OPEN-QUESTIONS.md).

```json
{
  "steps": [
    {
      "name": "Content",
      "Data": [
        {
          "ContentType": "BlogPost",
          "ContentItemId": "ujzde8gxd6ncf10epf91dhodzd",
          "ContentItemVersionId": "saj08xui6d39zzzzg4zdmen2kh",
          "DisplayText": "Composting for Beginners",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2026-08-28T08:00:00Z",
          "ModifiedUtc": "2026-08-28T08:00:00Z",
          "PublishedUtc": "2026-08-28T08:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Composting for Beginners"
          },
          "AutoroutePart": {
            "Path": "garden/composting-for-beginners"
          },
          "MarkdownBodyPart": {
            "Markdown": "Kitchen scraps, dry leaves and patience turn into rich soil."
          }
        },
        {
          "ContentType": "BlogPost",
          "ContentItemId": "oc9is0j8ht9lgmxg9edn581u33",
          "ContentItemVersionId": "vdgaj8gxbenyjqwx4hh5344tfj",
          "DisplayText": "Autumn Harvest Festival Recap",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2026-09-14T09:30:00Z",
          "ModifiedUtc": "2026-09-14T09:30:00Z",
          "PublishedUtc": "2026-09-14T09:30:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Autumn Harvest Festival Recap"
          },
          "AutoroutePart": {
            "Path": "garden/autumn-harvest-festival-recap"
          },
          "MarkdownBodyPart": {
            "Markdown": "Baskets of apples, pear cider and a pie contest."
          }
        },
        {
          "ContentType": "BlogPost",
          "ContentItemId": "xtplpft75v2seh60kvj50ce9uv",
          "ContentItemVersionId": "gvq4k7bn7xj8b7tfq7xkwo886v",
          "DisplayText": "Pruning Apple Trees in Winter",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2026-10-02T10:15:00Z",
          "ModifiedUtc": "2026-10-02T10:15:00Z",
          "PublishedUtc": "2026-10-02T10:15:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Pruning Apple Trees in Winter"
          },
          "AutoroutePart": {
            "Path": "garden/pruning-apple-trees-in-winter"
          },
          "MarkdownBodyPart": {
            "Markdown": "Which branches to cut, and which to leave alone."
          }
        },
        {
          "ContentType": "BlogPost",
          "ContentItemId": "w53efr4edt2sywb3wkh5dnsipz",
          "ContentItemVersionId": "ompzom75wbbr4qmw2wxfogo4mv",
          "DisplayText": "Meet the Beekeepers of Green Valley",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2026-10-21T14:45:00Z",
          "ModifiedUtc": "2026-10-21T14:45:00Z",
          "PublishedUtc": "2026-10-21T14:45:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Meet the Beekeepers of Green Valley"
          },
          "AutoroutePart": {
            "Path": "garden/meet-the-beekeepers-of-green-valley"
          },
          "MarkdownBodyPart": {
            "Markdown": "Three families, forty hives and a very busy spring."
          }
        },
        {
          "ContentType": "BlogPost",
          "ContentItemId": "z5fk2z9ri19r0wyojfljooa5lq",
          "ContentItemVersionId": "n4a4wfhym4l1vfz3zfkkibj3j4",
          "DisplayText": "Spring Planting Plan",
          "Latest": true,
          "Published": false,
          "CreatedUtc": "2026-10-28T16:20:00Z",
          "ModifiedUtc": "2026-10-28T16:20:00Z",
          "PublishedUtc": null,
          "Author": "admin",
          "TitlePart": {
            "Title": "Spring Planting Plan"
          },
          "AutoroutePart": {
            "Path": "garden/spring-planting-plan"
          },
          "MarkdownBodyPart": {
            "Markdown": "A draft of the beds, the seeds and the dates."
          }
        }
      ]
    }
  ]
}
```

Expected results after the import, used by the steps below:

| Statement | Rows |
|---|---|
| `S2` (published blog posts, newest first) | 5: the recipe's "Man must explore, and this is exploration at its greatest" first, then "Meet the Beekeepers of Green Valley", "Pruning Apple Trees in Winter", "Autumn Harvest Festival Recap", "Composting for Beginners". The draft is not listed. |
| `S4` with `contenttype` unset | the same 5 rows, with `DocumentId` and `DisplayText` only |
| `S4` with `contenttype` set to `Article` | 1 row: "About" |
| `S5` with `limit` set to `2` | 2 rows: the two newest posts |

Every statement run on camera must return at least one row, because the results panel and "Query executed in N ms"
are drawn only when there are rows.

### Snapshot

After the import: stop the site, copy its `App_Data` folder and `state.json` aside (the snapshot), start the site
again. To record again, stop the site, restore the copy, start it, and check that `PublishedBlogPosts` is absent from
**Search**, **Queries**, **All Queries**.

## The statements

Each statement is entered in the **Template** editor. `S2`, `S3`, `S4` and `S5` each have an address that opens
the Run SQL Query page with the Template already filled in (the `query` value is the statement in Base64, already
encoded for an address, `Sql/Controllers/AdminController.cs:43-55`); append it to `/Admin/Queries/Sql/Query?query=`.

**An address also runs the statement.** The action behind it hands the decoded statement to the code of the **Query**
button (`Sql/Controllers/AdminController.cs:50-54` calls the `[HttpPost]` overload), so the page opens with **Final SQL
Query**, **Create** and the results (or the message of a refused statement) already shown, with no parameters. The
link in the hint of the page and the **Run** button of a saved query
(`Views/SqlQuery.Buttons.SummaryAdmin.cshtml:12-16`) go to the same address, so they run the statement too.

| Id | Statement | Address value |
|---|---|---|
| `S1` | `SELECT * FROM ContentItemIndex` (filled in by the link, no address needed) | `U0VMRUNUICogRlJPTSBDb250ZW50SXRlbUluZGV4` |
| `S2` | `select DocumentId, DisplayText, CreatedUtc` / `from ContentItemIndex` / `where Published = true and ContentType = 'BlogPost'` / `order by CreatedUtc desc` (4 lines) | `c2VsZWN0IERvY3VtZW50SWQsIERpc3BsYXlUZXh0LCBDcmVhdGVkVXRjCmZyb20gQ29udGVudEl0ZW1JbmRleAp3aGVyZSBQdWJsaXNoZWQgPSB0cnVlIGFuZCBDb250ZW50VHlwZSA9ICdCbG9nUG9zdCcKb3JkZXIgYnkgQ3JlYXRlZFV0YyBkZXNj` |
| `S3` | `delete from ContentItemIndex where 1 = 0` (deletes nothing, even if it were run) | `ZGVsZXRlIGZyb20gQ29udGVudEl0ZW1JbmRleCB3aGVyZSAxID0gMA%3D%3D` |
| `S4` | `select DocumentId, DisplayText` / `from ContentItemIndex` / `where Published = true and ContentType = @contenttype:'BlogPost'` / `order by CreatedUtc desc` (4 lines) | `c2VsZWN0IERvY3VtZW50SWQsIERpc3BsYXlUZXh0CmZyb20gQ29udGVudEl0ZW1JbmRleAp3aGVyZSBQdWJsaXNoZWQgPSB0cnVlIGFuZCBDb250ZW50VHlwZSA9IEBjb250ZW50dHlwZTonQmxvZ1Bvc3QnCm9yZGVyIGJ5IENyZWF0ZWRVdGMgZGVzYw%3D%3D` |
| `S5` | the 4 lines of `S4`, then a fifth line: `{% if limit > 0 %}limit @limit{% endif %}` | `c2VsZWN0IERvY3VtZW50SWQsIERpc3BsYXlUZXh0CmZyb20gQ29udGVudEl0ZW1JbmRleAp3aGVyZSBQdWJsaXNoZWQgPSB0cnVlIGFuZCBDb250ZW50VHlwZSA9IEBjb250ZW50dHlwZTonQmxvZ1Bvc3QnCm9yZGVyIGJ5IENyZWF0ZWRVdGMgZGVzYwp7JSBpZiBsaW1pdCA%2BIDAgJX1saW1pdCBAbGltaXR7JSBlbmRpZiAlfQ%3D%3D` |

### Putting a statement in an editor

The **Template** and **Parameters** fields are CodeMirror editors; their text areas are hidden, so never type into
the text area. The narration never says "type", so either way is allowed. Before the first take, do a dry run and
use the first method that gives exactly the expected text:

1. **Type it**: click the editor (`.ocat-wrapper:has(label:text-is('Template')) .CodeMirror`, or the one of
   `Parameters`), select everything (`Control+A`), and type the statement with `TypeAsync` (about 20 ms per
   character, `Enter` between lines). The text in the editor must equal the statement: no closing quote, bracket or
   brace added by the editor, and no indentation on the new lines.
2. **Set the value with the page's editor object**: evaluate on the Playwright locator of the editor, for example
   `scene.Page.Locator(".ocat-wrapper:has(label:text-is('Parameters')) .CodeMirror").EvaluateAsync("(el, v) => el.CodeMirror.setValue(v)", "<text>")`,
   and the same with the `Template` label. Do not use `document.querySelector` with these locators: `:text-is()` and
   `:has()` with a label test are Playwright selectors, not CSS.

Use the same method for the whole video. In `03-params` the Parameters value is `{ "contenttype": "Article" }` and
then `{ "limit": 2 }` (an object with one property, braces and quotes as written).

Do not use an address to put a statement in the Template during a capture, because it also runs the statement
(see above) and the page would show the result before the narration says to run it. An address is for the page a clip
starts on (`05-save`) and for preparing a take outside the capture.

## Order of the clips

Record `02-run`, then `03-params`, then `05-save`. None of them needs the state another one leaves: each starts on
the page named below, with the site as it is in the snapshot.

| Clip | Changes the site? |
|---|---|
| `02-run` | No. It runs SELECT statements and `S3`, which is refused. |
| `03-params` | No. |
| `05-save` | **Yes**: it creates the query `PublishedBlogPosts`. Restore the snapshot before a new take of any clip. |

The slide clips (`01-why`, `04-convert`, `06-recipe`, `07-access`) are drawn by `slides.py`; there is nothing to
record. The cards `00-intro` and `99-outro` are drawn by the assembler.

---

## Clip `02-run`

**Starts on**: `/Admin` (the dashboard), the admin menu expanded, nothing selected. Open it before
`StartCaptureAsync`.

### `run-1`

- **Actions**: move to **Search** in the admin menu and click it (its id is `search`: `#search`, from
  `AdminMenu.cs:18-20`; if it is not found, the menu entry whose text is exactly `Search`). Click **Queries**, then
  click **Run SQL Query** with `ClickAndWaitForNavigationAsync` (the entry whose text is exactly `Run SQL Query`).
  Pause 600 ms between clicks.
- **Visible at the end of the narration**: the Run SQL Query page: breadcrumb **Queries** and **SQL Query**, the
  empty **Template** editor, the **Parameters** editor and the **Query** button. The pointer is near the menu entry.
- **Labels**: **Search**, **Queries**, **Run SQL Query**: `Sql/AdminMenu.cs:18-20`. **SQL Query** and **Queries**
  (breadcrumb): `Views/Admin/Query.cshtml:33-34`. **Tools**, **Features**: `OrchardCore.Features/AdminMenu.cs:26-27`
  (spoken only; the Features page is not shown).

### `run-2`

- **Actions**: highlight the **Template** label and editor
  (`.ocat-wrapper:has(label:text-is('Template'))`), hold 1.5 s, clear the highlight. Highlight the first hint
  (`.hint:has-text('click here')`), hold 1 s, then click the link `.hint a:text-is('click here')` with
  `ClickAndWaitForNavigationAsync`. The page reloads with `S1` in the Template, and the statement has run: the link
  goes to the address of `S1`, which runs it (see "The statements"). Clear highlights. Stay at the top of the page.
- **Visible at the end of the narration**: the **Template** holds `SELECT * FROM ContentItemIndex`. **Final SQL Query**
  and the results exist below the visible area; the next step scrolls to them.
- **Labels**: **Template**, the hint "You can click here to query all content items.":
  `Views/Admin/Query.cshtml:42,46`. The statement: `Sql/ViewModels/AdminQueryViewModel.cs:7`. The link runs the
  statement: `Sql/Controllers/AdminController.cs:44-54`.

### `run-3`

- **Actions**: do not click **Query**; the statement of `run-2` has already run. Scroll down 420 px so **Final SQL
  Query** is on screen with the start of the results, and highlight the **Final SQL Query** field
  (`.ocat-wrapper:has(label:text-is('Final SQL Query'))`).
- **Visible at the end of the narration**: the **Final SQL Query** field holds the converted statement (names quoted,
  ending with a semicolon), with its hint "The query as it will be executed by the SQL database." The **Create**
  button is under it.
- **Labels**: **Final SQL Query**, **Create**: `Views/Admin/Query.cshtml:66,69,75`. The conversion:
  `Sql/SqlParser.cs` (`OrchardSqlRewriter`, `OrchardSqlDialect`) and the hint at `Views/Admin/Query.cshtml:47`.

### `run-4`

- **Actions**: clear the highlight, scroll down 500 px so the results are on screen. Wait until the results editor
  (`#monaco_editor`) shows the text `DocumentId` (it formats the JSON about 300 ms after it loads). Highlight the line
  "Query executed in N ms" (`.text-muted:has-text('Query executed in')`).
- **Visible at the end of the narration**: "Query executed in N ms" above the results editor, which lists objects with
  `Id`, `DocumentId`, `ContentType`, `Published`, `CreatedUtc` and the other columns of the table.
- **Labels**: "Query executed in {0} ms": `Views/Admin/Query.cshtml:82`; the results editor: `:100`;
  `Sql/Controllers/AdminController.cs:95`. The columns: `OrchardCore.ContentManagement/Records/Migrations.cs:18-29`.

### `run-5`

- **Actions**: clear the highlight, scroll to the top. Put `S2` in the Template (see "Putting a statement in an
  editor"; not by an address). Click **Query** (`form.no-multisubmit button.btn-primary:text-is('Query')`) with
  `WaitForNavigationAsync`. Scroll down 420 px.
- **Visible at the end of the narration**: the Template shows the four lines of `S2`; the results editor lists five
  objects with `DocumentId`, `DisplayText` and `CreatedUtc` only, the newest first, and no draft.
- **Labels**: **Template**: as in `run-2`. **Query**: `Views/Admin/Query.cshtml:60`. The hint "The SQL query uses standard SQL92 syntax and
  is converted based on the actual database. Tables will be prefixed automatically.": `Views/Admin/Query.cshtml:47`.

### `run-6`

- **Actions**: scroll to the top. Put `S3` in the Template (not by an address, which would show the message before
  **Query** is clicked). Click **Query** with `WaitForNavigationAsync`. Leave the page at the top, so the Template and
  the message under it are on screen. An empty **Final SQL Query** field and **Create** appear below them whenever a
  statement was submitted (`Views/Admin/Query.cshtml:63`, `Elapsed` is set at `Sql/Controllers/AdminController.cs:112`
  also for a refused statement); do not scroll to them.
- **Visible at the end of the narration**: the message "Only SELECT statements are supported." in red under the
  Template (`span[data-valmsg-for='DecodedQuery']`); no results.
- **Labels**: the message: `Sql/SqlParser.cs:57,82`; shown by `Sql/Controllers/AdminController.cs:104-108` and
  `Views/Admin/Query.cshtml:45`. The line and column of a syntax error: `Sql/SqlParser.cs:44,53` (spoken only).

---

## Clip `03-params`

**Starts on**: `/Admin/Queries/Sql/Query` (empty Template and Parameters). Open it before `StartCaptureAsync`.

### `param-1`

- **Actions**: put `S4` in the Template. Highlight the parameter in the third line
  (`.CodeMirror-line:has-text('@contenttype')`), hold 2 s, clear the highlight.
- **Visible at the end of the narration**: the Template shows the four lines of `S4`, with `@contenttype:'BlogPost'`
  in the third line. Nothing has run.
- **Labels**: **Template**, **Parameters**: `Views/Admin/Query.cshtml:42,52`. The syntax `@name:default`: the
  warning text at `Views/SqlQuery.Edit.cshtml:36` and `Sql/SqlParser.cs:257-269` (`VisitParameter`).

### `param-2`

- **Actions**: click **Query** with `WaitForNavigationAsync`, scroll down 420 px (five rows with `DocumentId` and
  `DisplayText`). Scroll up. Put `{ "contenttype": "Article" }` in **Parameters**, click **Query** with
  `WaitForNavigationAsync`, scroll down 420 px.
- **Visible at the end of the narration**: **Parameters** holds `{ "contenttype": "Article" }`, and the results editor
  shows one object whose `DisplayText` is `About`.
- **Labels**: **Parameters**, the hint "An optional Json object containing the parameter values for this query.":
  `Views/Admin/Query.cshtml:52,55`. Case-sensitive names: the parameters are read into a dictionary with the default
  comparer (`Sql/Controllers/AdminController.cs:82`, `Sql/SqlParser.cs:259`).

### `param-3`

- **Actions**: scroll to the top. Put `S5` in the Template (the fifth line is the Liquid condition) and
  `{ "limit": 2 }` in **Parameters** (replacing the previous value). Highlight the fifth line
  (`.CodeMirror-line:has-text('endif')`), hold 1.5 s, clear it. Click **Query** with `WaitForNavigationAsync`, scroll
  down 420 px.
- **Visible at the end of the narration**: **Parameters** holds `{ "limit": 2 }`; **Final SQL Query** ends with a
  limit clause; the results editor lists exactly two objects.
- **Labels**: the Liquid rendering happens before the parsing, with the parameters as variables:
  `Sql/Controllers/AdminController.cs:84`.

---

## Clip `05-save`

**Starts on**: `/Admin/Queries/Sql/Query?query=<address value of S2>` (the Template holds `S2`, and the address has
already run it, so **Final SQL Query** and the five rows are below the visible area). Open it before
`StartCaptureAsync`.

### `save-1`

- **Actions**: click **Query** (`form.no-multisubmit button.btn-primary:text-is('Query')`) with
  `WaitForNavigationAsync`, which runs `S2` once more, for the narration ("run it"); scroll down 420 px so **Final SQL
  Query** and the **Create** button are on screen; move to **Create** (`a.btn-success:text-is('Create')`) and click it
  with `ClickAndWaitForNavigationAsync`.
- **Visible at the end of the narration**: the form for a SQL query: breadcrumb **Queries** and "New Sql query", the
  **Name** field empty, and further down the **Query** field filled with the four lines of `S2`.
- **Labels**: **Create**: `Views/Admin/Query.cshtml:75`. The title "New {0} query" (with the source name `Sql`):
  `Views/Admin/Create.cshtml:4`. The statement carried to the form: `Sql/Drivers/SqlQueryDisplayDriver.cs:64-70`.

### `save-2`

- **Actions**: type `PublishedBlogPosts` in **Name**
  (`.ocat-wrapper:has(label:text-is('Name')) input.form-control`, `TypeAsync`, 40 ms per character; the wrapper also
  holds a hidden input for the source, so the selector must not match every `input`). Highlight the hint under it.
- **Visible at the end of the narration**: **Name** holds `PublishedBlogPosts`, with the hint "The technical name of
  the query."
- **Labels**: **Name**, the hint: `Views/Query.Fields.Edit.cshtml:4,8`. The rules (the letters A to Z and digits, a
  letter first, at most 128 characters, unique): `Drivers/QueryDisplayDriver.cs:90-102` and
  `OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:201-228` (`ToSafeName`: `char.IsAsciiLetter`, `char.IsDigit`).

### `save-3`

- **Actions**: clear the highlight; scroll down until **Return Documents** and the **Query** field are on screen;
  click the **Return Documents** check box
  (`.form-check:has(label:text-is('Return Documents')) input[type=checkbox]`); highlight the hint "When checked, the
  result has to be a list of document ids." (`.hint:has-text('list of document ids')`).
- **Visible at the end of the narration**: **Return Documents** is checked, with its two hints, and the **Query**
  field below holds `S2`.
- **Labels**: **Return Documents** and both hints: `Views/SqlQuery.Edit.cshtml:25-29`. It is unchecked by default
  because a new query has `ReturnContentItems` false (`OrchardCore.Queries.Abstractions/Models/Query.cs:26`). What it
  does: the rows are read for their `DocumentId` column (else the first column that holds a `long`, else the first
  column), and `ISession.GetAsync<ContentItem>` loads the items: `Sql/SqlQuerySource.cs:91-140` (spoken only).

### `save-4`

- **Actions**: clear the highlight; highlight the yellow warning above the **Query** field (`.alert-warning`), hold
  until the end of the narration.
- **Visible at the end of the narration**: the warning "SQL queries are Liquid templates. Never inject user input with
  Liquid output expressions like '{{ input }}'. Use SQL parameters (for example '@name' or '@name:default') passed in
  by the caller for user-provided values." Only this warning is on screen (the second warning appears only when the
  statement contains a Liquid output expression, which `S2` does not).
- **Labels**: `Views/SqlQuery.Edit.cshtml:33-39` (the second warning: `:41-50`).

### `save-5`

- **Actions**: clear the highlight; scroll up to **Schema** (`.ocat-wrapper:has(label:text-is('Schema'))`) and
  highlight it for 2 s; clear the highlight; scroll down and click **Save** (`button.save`) with
  `ClickAndWaitForNavigationAsync`. Leave **Schema** empty.
- **Visible at the end of the narration**: the **Queries** list with the notification "Query created successfully."
  and the row `PublishedBlogPosts`.
- **Labels**: **Schema**, the hint "The schema the GraphQL API will use to return results.": `Views/Query.Fields.Edit.cshtml:14,18`.
  **Save**: `Views/Query.Fields.Buttons.cshtml:3`. The notification: `Controllers/AdminController.cs:162`. A query without a
  schema is skipped by GraphQL: `Sql/GraphQL/SqlQueryFieldTypeProvider.cs:50-55` (spoken only).

### `save-6`

- **Actions**: move the pointer to the badge **SQL query** of the row
  (`.list-group-item:has(a:text-is('PublishedBlogPosts')) .badge`), then to **Run**, then to **Edit**, then to
  **Delete** of the same row, 1.5 s on each. Do not click any of them.
- **Visible at the end of the narration**: the row `PublishedBlogPosts` with the badge **SQL query** and the buttons
  **Run**, **Edit** and **Delete**; the pointer is over **Delete**, which is not clicked. (**Run** opens Run SQL Query
  with the statement and runs it, as the address of an earlier step does; it is not clicked, to leave the clip on the
  list.)
- **Labels**: **SQL query** (badge): `Views/SqlQuery.SummaryAdmin.cshtml:4`. **Run**:
  `Views/SqlQuery.Buttons.SummaryAdmin.cshtml:16`. **Edit**, **Delete**: `Views/Query.Buttons.SummaryAdmin.cshtml:6,12`.

---

## Slides

The slides need no recording. Each line says what the slide of the step shows.

- `why-1`: the `ContentItemIndex` class with its twelve properties (the table also has the `Id` column of every index
  table, `OrchardCore.ContentManagement/Records/Migrations.cs:20-32`), and three cards: one row for each version of an
  item, the table name, the columns.
- `why-2`: three cards: filter rows, sort and page, choose columns.
- `why-3`: two cards: rows of values, and content items (Return Documents; `Sql/SqlQuerySource.cs:91-140`).
- `convert-1`: three cards: `LIMIT` and `OFFSET`, `ORDER BY random()`, the date functions (from the documentation:
  `src/docs/reference/modules/Queries/README.md`, "Helper functions").
- `recipe-1`: the `queries` recipe step of the Blog recipe (`RecentBlogPosts`) as JSON.
- `recipe-2`: three cards: the name exists (the query is updated, whatever the source), the name is free (a query is
  created; a source that no feature provides is an error), no name or no source (an error, checked first)
  (`Recipes/QueryStep.cs:38-75`).
- `access-1`: three cards: Manage SQL Queries (Security Critical), Administrator and Editor, Manage queries
  (`OrchardCore.Queries.Core/QueriesPermissions.cs:8`, `Sql/Permissions.cs:15-27`,
  `OrchardCore.Queries.Abstractions/QueryPermissions.cs:14`,
  `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:122-125`).
- `access-2`: three cards: direct table access, no content permissions, Manage queries too. The last one: saving a
  query needs only Manage queries (`Controllers/AdminController.cs:119,144`; `Sql/Drivers/SqlQueryDisplayDriver.cs`
  checks no permission), and Manage queries implies the permission to execute every query through the API
  (`OrchardCore.Queries.Abstractions/QueryPermissions.cs:19-25`, `Controllers/QueryApiController.cs:52`).

## Checks for private data

- Only fake data is on screen: the posts of the seed above, the Blog recipe's sample content, the user `admin` and the
  site name Orchard Core. No real name, email address, phone number, domain or token. The rows of `S1` also show the
  `Owner` of each item, which is the id of the user `admin` of the throwaway site, and the generated
  `ContentItemId` values.
- No password is typed or shown: the sign-in is `state.json`, outside git.
- The statements read only the content index (`ContentItemIndex`); no user table, no `Document` table.
- Before assembling, look at every contact sheet, then OCR the video at one frame per second and at every scene change,
  and search for anything that is not in the seed or the Blog recipe.

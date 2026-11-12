# Recording plan: Content Fields Indexing

Everything a person needs to record this video without deciding anything. Narration timings come from `narrate.py`
(`durations.json`): narrate first, then record. In this plan a selector such as `#search-box` stands for
`scene.Page.Locator("#search-box")`. Paths of the Orchard Core source (commit `92db4a83`) start at `src/`.

The video has two recorded clips, `02-enable` and `04-sql`. Chapters 01, 03, 05 and 06 are slides, drawn by
`slides.py`; nothing is recorded for them. No clip needs `PrepareAsync`: the site is prepared by hand, as described
below, outside the capture.

## The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, started on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data | An empty folder outside both repositories in `ORCHARD_APP_DATA` |
| Setup | Auto Setup, so no setup or sign-in form is recorded (see `references/recording.md`) |
| Recipe | `Blog` (the `RecipeName` of Auto Setup) |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Database | `Sqlite`, with an empty table prefix |
| Sign-in | User `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`, never written down) |
| Outbound calls | Blocked: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address |
| Browser | The recorder's default: 1536x864 CSS pixels, scale 1.25, light color scheme, `en-US` |

The Auto Setup variables are `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
`...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
`...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite` and `...__RecipeName=Blog`.

Features: the `Blog` recipe already enables the ones the clips use (`OrchardCore.Features`, `OrchardCore.ContentFields`,
`OrchardCore.Users`, `OrchardCore.Deployment`, `OrchardCore.Queries`, `OrchardCore.Queries.Sql`; see
`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). Enable nothing else. The two features of this video,
`OrchardCore.ContentFields.Indexing.SQL` and `OrchardCore.ContentFields.Indexing.SQL.UserPicker`, are enabled on
camera in `02-enable`, so the site must start with both disabled. The GraphQL feature stays disabled: its chapter is
a slide.

Settings: none. Keep the default admin list layout (List) and do not change the admin theme.

Sign the recorder in once, outside the capture (`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`).
In Git Bash set `MSYS_NO_PATHCONV=1`.

## Order, and what changes the site

1. Start the site. **Snapshot A:** stop it, copy `App_Data` and `state.json` aside, start it again.
2. Record `02-enable`. It **changes the site** (it enables two features): to record it again, restore snapshot A first.
3. Import the seed data below (outside the capture). It must come **after** `02-enable`: a content item writes its
   rows to the field tables when it is saved, so the items have to be saved while the features are enabled.
   **Snapshot B:** stop the site, copy `App_Data` aside, start it again.
4. Record `04-sql`. It only runs `SELECT` statements and changes nothing: record it again as often as needed, without
   restoring anything. If the site was restored from snapshot A, repeat steps 2 and 3 first.

For a full new take: restore A, record `02-enable`, import the seed data, record `04-sql`.

## The seed data (fake, created outside the capture)

After `02-enable`, open **Tools > Deployments > JSON Import** (`/Admin/DeploymentPlan/Import/Json`), paste the whole
recipe below into the field labeled `JSON`, and click `Import`. The page answers "Recipe imported successfully!".
It defines a content type `Event` with a Text field `Venue`, a Numeric field `Capacity` and a Multi Text field
`Topics` on a part named `Event`, and three published events.

Labels: `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30` (JSON Import),
`.../Views/Import/Json.cshtml:15,21` (`JSON`, `Import`), `.../Controllers/ImportController.cs:185` (the message).

```json
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Event",
          "DisplayName": "Event",
          "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            { "PartName": "Event", "Name": "Event", "Settings": { "ContentTypePartSettings": { "Position": "1" } } }
          ]
        }
      ],
      "ContentParts": [
        {
          "Name": "Event",
          "Settings": {},
          "ContentPartFieldDefinitionRecords": [
            { "FieldName": "TextField", "Name": "Venue", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Venue", "Position": "0" } } },
            { "FieldName": "NumericField", "Name": "Capacity", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Capacity", "Position": "1" } } },
            { "FieldName": "MultiTextField", "Name": "Topics", "Settings": { "ContentPartFieldSettings": { "DisplayName": "Topics", "Position": "2" } } }
          ]
        }
      ]
    },
    {
      "name": "Content",
      "Data": [
        {
          "ContentItemId": "4ujzde8gxd6ncf10epf91dhodz", "ContentType": "Event", "DisplayText": "Orchard Core Meetup",
          "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Orchard Core Meetup" },
          "Event": { "Venue": { "Text": "Maple Hall" }, "Capacity": { "Value": 80 }, "Topics": { "Values": [ "Modules", "Themes" ] } }
        },
        {
          "ContentItemId": "4doc9is0j8ht9lgmxg9edn581u", "ContentType": "Event", "DisplayText": "Developer Workshop",
          "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Developer Workshop" },
          "Event": { "Venue": { "Text": "Cedar Room" }, "Capacity": { "Value": 25 }, "Topics": { "Values": [ "Queries" ] } }
        },
        {
          "ContentItemId": "433xtplpft75v2seh60kvj50ce", "ContentType": "Event", "DisplayText": "Community Day",
          "Latest": true, "Published": true, "Author": "admin",
          "TitlePart": { "Title": "Community Day" },
          "Event": { "Venue": { "Text": "Maple Hall" }, "Capacity": { "Value": 200 }, "Topics": { "Values": [ "Recipes", "Media" ] } }
        }
      ]
    }
  ]
}
```

Check the seed before recording `04-sql`, outside the capture: open the two prefilled addresses listed under `04-sql`
(an address with a `query` value runs that query as it opens, so there is nothing to click) and confirm that the first
returns 3 rows and the second 5 rows. If a count differs, read `OPEN-QUESTIONS.md` and stop: do not change the seed or
the queries.

## Clip `02-enable`

**Page it starts on:** `scene.OpenAsync("/Admin/Features")`, before `StartCaptureAsync()`. The breadcrumb reads
"Features", the search box is empty, and the status filter shows every feature. Source: `OrchardCore.Features/AdminMenu.cs:26-27`
(Tools, Features), `.../Views/Admin/Features.cshtml:15` (breadcrumb), `:27` (`#search-box`, placeholder "Search").

Row selectors used below (the List layout draws each feature as a `.list-group-item`):

- the SQL row: `.list-group-item:has(a[id$='OrchardCore_ContentFields_Indexing_SQL'])`
- the User Picker row: `.list-group-item:has(a[id$='OrchardCore_ContentFields_Indexing_SQL_UserPicker'])`
- its buttons: `#btn-enable-OrchardCore_ContentFields_Indexing_SQL` and
  `#btn-enable-OrchardCore_ContentFields_Indexing_SQL_UserPicker` (after enabling, the ids start with `btn-disable-`).
  The id is the feature id with each `.` turned into `_`: `.../Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.
  Before recording, confirm both ids with `Recorder probe /Admin/Features "() => [...document.querySelectorAll('a[id^=btn-enable-OrchardCore_ContentFields]')].map(a => a.id)"`.

`ClickAndWaitForNavigationAsync` is used for every `Enable` click: the link loads the Features page again.

- `enable-1`
  1. `TypeAsync(#search-box, "Content Fields Indexing")`.
  2. Wait until `#list-summary` reads "Showing 2 of" (the rows filter as you type).
  3. `HighlightAsync(.feature-group:has(a[id^='btn-enable-OrchardCore_ContentFields_Indexing']))`: a ring around the
     category that holds both rows (`.../Views/Admin/Features.cshtml:87-89`, the `h3` and the list of one category).

  *On screen when the narration ends:* the search box with the text, and the ringed category "Content Management" with two rows:
  "Content Fields Indexing (SQL)" and "Content Fields Indexing (SQL) - User Picker", each with a "Disabled" badge and an
  `Enable` button. *Labels:* the names come from `OrchardCore.ContentFields/Manifest.cs:22,30`, the category from
  `:23,31`, the badge from `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`.

- `enable-2`
  1. `ClearHighlightsAsync()`, then `HighlightAsync` on the dependency badge "Content Fields" in the SQL row
     (`.list-group-item:has(a[id$='OrchardCore_ContentFields_Indexing_SQL']) .direct-dependencies .badge`).
  2. `ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_ContentFields_Indexing_SQL)`.
  3. Wait until the notification "The feature Content Fields Indexing (SQL) was enabled." is visible
     (enabling runs the data migrations, which can take a few seconds; the wait is part of the step, so keep the
     clip's frames; if it takes longer than the narration, wrap only the wait in `OffCameraAsync`).

  *On screen when the narration ends:* the Features page after the reload, with that green notification at the top.
  *Labels:* `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (`Enable`),
  `OrchardCore.Features/Controllers/AdminController.cs:317` (the message), `.../FeatureEntry.DefaultTags.SummaryAdmin.cshtml:16`
  (the badge shows the dependency's name).

- `enable-3`
  1. `TypeAsync(#search-box, "Content Fields Indexing")` (the reload emptied the box); wait until `#list-summary` reads "Showing 2 of".
  2. `HighlightAsync` on the dependency badges of the User Picker row (`.list-group-item:has(a[id$='OrchardCore_ContentFields_Indexing_SQL_UserPicker']) .direct-dependencies`),
     which read "Content Fields" and "Users".
  3. `ClearHighlightsAsync()`, then `ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_ContentFields_Indexing_SQL_UserPicker)`.
  4. Wait until the notification "The feature Content Fields Indexing (SQL) - User Picker was enabled." is visible.

  *On screen when the narration ends:* the Features page after the reload, with that notification at the top.
  *Labels:* `OrchardCore.ContentFields/Manifest.cs:30-37` (the name and the dependencies), `AdminController.cs:317`.

- `enable-4`
  1. `TypeAsync(#search-box, "Content Fields Indexing")`; wait until `#list-summary` reads "Showing 2 of".
  2. `HighlightAsync(.feature-group:has(a[id^='btn-disable-OrchardCore_ContentFields_Indexing']))`, the same ring as in `enable-1`.

  *On screen when the narration ends:* the ringed category with the two rows, each with an "Enabled" badge and a `Disable` button.
  *Labels:* `.../FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` (`Enabled`), `FeatureEntry.Buttons.SummaryAdmin.cshtml:53` (`Disable`).

## Clip `04-sql`

**Page it starts on:** the Run SQL Query page without a query, opened before the capture, with the first query put
into the `Template` field before `StartCaptureAsync()`:

```
scene.OpenAsync("/Admin/Queries/Sql/Query")
// wait until scene.Page.Locator(".CodeMirror").First is visible, then:
scene.Page.EvaluateAsync("(sql) => document.querySelector('.CodeMirror').CodeMirror.setValue(sql)",
    "SELECT * FROM TextFieldIndex WHERE ContentType = 'Event' AND ContentField = 'Venue' AND Published = 1")
```

The breadcrumb reads "Queries" then "SQL Query" (`OrchardCore.Queries/Views/Admin/Query.cshtml:33`), and the field labeled
`Template` holds the query (`:42`). Without a `query` value the page shows only the form: no `Final SQL Query`, no `Create`
button, no result (`Sql/Controllers/AdminController.cs:43-48, 67-70`; `Query.cshtml:63`). The code editor of the field is
the first `.CodeMirror` of the page (`scene.Page.Locator(".CodeMirror").First`; the `Parameters` field has one too, and
so does `Final SQL Query` once a query has run). It is a CodeMirror 5 editor made from the textarea with `fromTextArea`
(`OrchardCore.Resources/wwwroot/Scripts/codemirror/codemirror.js`, version 5.65.7), whose `CodeMirror` property on the
`.CodeMirror` element is the editor, and which copies its text back to the textarea when the form is submitted. The
`Query` button is `button.btn-primary:text-is('Query')` (`:60`).

First prefilled address, for the seed check only (Base64 of the same query; `%3D` is the `=` of the padding):

```
/Admin/Queries/Sql/Query?query=U0VMRUNUICogRlJPTSBUZXh0RmllbGRJbmRleCBXSEVSRSBDb250ZW50VHlwZSA9ICdFdmVudCcgQU5EIENvbnRlbnRGaWVsZCA9ICdWZW51ZScgQU5EIFB1Ymxpc2hlZCA9IDE%3D
```

**An address with a `query` value already runs the query.** The `GET` action hands the decoded text (`:43-52` decodes it)
to the action that handles the form (`:50`, `:58-115`), which runs it, so the page opens with `Final SQL Query`, the
`Create` button and the result below the form (`Query.cshtml:63-103`). That is why `sql-1` and `sql-2` start from the
blank form, where `Query` really runs the first query, and why `sql-4` needs no click.

Second prefilled address, opened by `sql-4` (Base64 of `SELECT DocumentId, BigValue FROM MultiTextFieldIndex WHERE ContentType = 'Event' AND ContentField = 'Topics' AND Published = 1`):

```
/Admin/Queries/Sql/Query?query=U0VMRUNUIERvY3VtZW50SWQsIEJpZ1ZhbHVlIEZST00gTXVsdGlUZXh0RmllbGRJbmRleCBXSEVSRSBDb250ZW50VHlwZSA9ICdFdmVudCcgQU5EIENvbnRlbnRGaWVsZCA9ICdUb3BpY3MnIEFORCBQdWJsaXNoZWQgPSAx
```

The result of a query is shown in a JSON editor, `#monaco_editor`, formatted about 300 ms after it loads. Wait for it
every time with: `#monaco_editor .view-lines` contains the text `DocumentId` (it is shown as `"DocumentId"`).

- `sql-1`
  1. `HighlightAsync(scene.Page.Locator(".CodeMirror").First)` on the `Template` field.
  2. `ClearHighlightsAsync()`, then `HighlightAsync` on the hint under it that starts "The SQL query uses standard SQL92 syntax"
     (`scene.Page.Locator(".hint", new() { HasTextString = "The SQL query uses standard SQL92 syntax" })`).

  *On screen when the narration ends:* the page at its top: the breadcrumb "SQL Query", the label `Template` with the
  query text, and the ringed hint "...Tables will be prefixed automatically." No result yet: below the `Query` button
  there is nothing, because nothing has run.
  *Labels:* `Query.cshtml:42` (`Template`), `:47` (the hint), `OrchardCore.Queries/Sql/AdminMenu.cs:18-20` (Search, Queries, Run SQL Query).

- `sql-2`
  1. `ClearHighlightsAsync()`.
  2. `ClickAndWaitForNavigationAsync(button.btn-primary:text-is('Query'))`.
  3. Wait for `#monaco_editor .view-lines` to contain `DocumentId`.
  4. `HighlightAsync(#monaco_editor)`: it scrolls the page so that the result editor is in view.

  *On screen when the narration ends:* the page after the form was posted, scrolled to the result: the label
  "Final SQL Query" with the statement the database runs, then "Query executed in ... ms" and the result editor with a
  JSON list that starts with `[` and an object for the first event.
  If "Final SQL Query" is cut off, scroll the page up with `scene.Page.EvaluateAsync("() => window.scrollBy({ top: -150, behavior: 'smooth' })")` and pause 600 ms; do not scroll with the mouse wheel, which
  would scroll the result editor instead.
  *Labels:* `Query.cshtml:60` (`Query`), `:66` (`Final SQL Query`), `:82` ("Query executed in {0} ms").

- `sql-3`
  1. Keep the ring on `#monaco_editor`. `MoveToAsync(scene.Page.Locator("#monaco_editor .view-line", new() { HasTextString = "\"DocumentId\"" }).First)`
     (the `"DocumentId"` line of the first object), without clicking.
  2. Pause 2 seconds, then `MoveToAsync(scene.Page.Locator("#monaco_editor .view-line", new() { HasTextString = "\"Text\"" }).First)`
     (the `"Text"` line of the same object, its tenth property; the quotes keep `"BigText"` from matching).

  *On screen when the narration ends:* the first object of the result with its keys, in this order: `Id`, `DocumentId`,
  `ContentItemId`, `ContentItemVersionId`, `ContentType` (`"Event"`), `ContentPart` (`"Event"`), `ContentField`
  (`"Venue"`), `Published` (`1`), `Latest` (`1`), `Text` (the venue of that event), `BigText` (the same venue).
  Which event comes first does not matter.
  *Source of the columns:* `OrchardCore.ContentFields/Indexing/SQL/ContentFieldIndexProvider.cs:7-16`, `TextFieldIndexProvider.cs:10-19`.

- `sql-4`
  1. `ClearHighlightsAsync()`.
  2. `NavigateAsync("/Admin/Queries/Sql/Query?query=<second value above>")`: the page runs the query as it opens, so there is no
     `Query` click in this step. Wait until the first `.CodeMirror` shows `MultiTextFieldIndex`.
  3. Wait for `#monaco_editor .view-lines` to contain `BigValue`.
  4. `HighlightAsync(#monaco_editor)`.

  *On screen when the narration ends:* the result editor with five short objects, each `{ "DocumentId": n, "BigValue": "..." }`.
  Two of them share a `DocumentId` and show `Modules` and `Themes`, and the other two events show `Queries`, and `Recipes` and `Media`.
  The rows of one event come back next to each other (they are written one after the other); if they do not, stop and read `OPEN-QUESTIONS.md`.
  *Source:* `OrchardCore.ContentFields/Indexing/SQL/MultiTextFieldIndexProvider.cs:75-88` (one row for each value).

- `sql-5`
  1. `ClearHighlightsAsync()`.
  2. `HighlightAsync(a.btn-success:text-is('Create'))`. Do not click it.

  *On screen when the narration ends:* the green `Create` button under "Final SQL Query", ringed, and the result editor
  below it. *Labels:* `Query.cshtml:75` (`Create`); `Return Documents` is on the page that `Create` opens
  (`OrchardCore.Queries/Views/SqlQuery.Edit.cshtml:26`) and is only named in the narration.

## Slides (nothing to record)

- `why-1`: the problem. Two cards (the fields are in the document; `ContentItemIndex` has no field values) over the
  `ContentItemIndex` class.
- `why-2`: the solution. Four cards: a table for each indexed field type, a row for each field value, two features, three ways to query.
- `tables-1`: the `ContentFieldIndex` class, with notes for `Content Part`, `Published` and `Latest`, and a line on `Id` and `DocumentId`.
- `tables-2`: the eleven tables with their value columns, in two columns; `UserPickerFieldIndex` is outlined in green and marked "Created by the User Picker feature."
- `tables-3`: the `TextFieldIndex` class (`MaxTextSize = 766`, `Text`, `BigText`) over the mapping of `MultiTextFieldIndexProvider` (one row for each value).
- `tables-4`: three cards (`Published`, `Latest`, `Neither`) over the check that skips soft deleted items.
- `code-1`: C# class `EventQueries` that queries `NumericFieldIndex` through `ISession`.
- `code-2`: a Razor view with the same kind of query on `TextFieldIndex`, and the Liquid that calls a saved SQL query named `EventVenues`.
- `graphql-1`: three cards: the two features, the fields with a filter, the suffixes.
- `graphql-2`: the GraphQL query `product(where: {price: {amount_gt: 10}})`, with notes for `price`, `amount` and `_gt`.

## Checks for private data

- Only fake names appear: the events "Orchard Core Meetup", "Developer Workshop" and "Community Day", the venues
  "Maple Hall" and "Cedar Room", the user `admin` and `admin@example.com`. No real person, company, address or key.
- The password is never typed on camera (the session is in `state.json`, which is not committed) and never written in a file.
- OCR the finished clips at one frame per second and at every scene change (see `SKILL.md`, "Check for private data"),
  and search for anything that is not in the list above, such as a machine name, a user name of the machine, a path of
  the machine, or a connection string. The Run SQL Query page shows only the SQL, not the connection.
- The final check of the `Final SQL Query` field: it must show table names without a prefix or a path of the machine.

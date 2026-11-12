# Recording plan: GraphQL: Filter, Sort and Page Content Items

This plan is for the person who records the video. Read only this file: it names the site, the data, the order, and
for every step of every recording what to do and what must be on screen when the narration of the step ends.

- Audience: developer. Every recording is the GraphiQL page of the admin (`/Admin/GraphQL`) in a browser. The
  video shows no other screen, except the cards and the slides.
- Slides (no recording): chapters 01, 03 and the first clip of 08. They are drawn by `slides.py`.
- Recordings: `02-filter`, `04-sort`, `05-page`, `06-status`, `07-combine` and `08-limit-error`.
- The code that the content was written against is the Orchard Core source at commit `92db4a83`. Build the checkout
  of that commit, or a later one if the checks below still pass. Paths such as `file:line` below are in that source,
  under `src/`: `src/OrchardCore.Modules/` for modules such as `OrchardCore.Apis.GraphQL`, and `src/OrchardCore/` for
  `OrchardCore.ContentManagement.GraphQL` and `OrchardCore.Apis.GraphQL.Abstractions`.

## 1. The site

A fresh site, provisioned by Auto Setup, with fake data only (see `references/recording.md` of the skill, and the
`orchardcore-tester` skill of the Orchard Core repository for running `OrchardCore.Cms.Web`).

| What | Value |
|---|---|
| Host | `OrchardCore.Cms.Web`, on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data folder | `ORCHARD_APP_DATA` = an empty folder outside both repositories |
| Setup recipe | **Headless** (`name` of `OrchardCore.Themes/TheAdmin/Recipes/headless.recipe.json:2`). It enables `OrchardCore.Apis.GraphQL` (line 44) and creates no sample content, so the results hold only the seed below. Use it instead of the Blog recipe |
| Site name, time zone | `Orchard Core`, `America/Los_Angeles` |
| Admin | user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, the same value in `OrchardCore__AutoSetup__Tenants__0__AdminPassword` and in `VIDEO_PASSWORD`) |
| Database | Sqlite (the Auto Setup default of the documentation) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, such as `http://127.0.0.1:9` |
| Validation mode | `OrchardCore__Apis__GraphQL__MaxNumberOfResultsValidationMode=Enabled`, so `08-limit-error` shows the error whatever the environment (`OrchardCore.Apis.GraphQL/Startup.cs:70-76`). The other GraphQL settings keep their defaults: first 100, maximum 1000 |

Auto Setup variables (`src/docs/reference/modules/AutoSetup/README.md`):
`OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
`...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
`...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Headless`.

The Setup screen is not part of this video. Auto Setup runs on the first request, so no setup form is recorded.

## 2. Seed the site (outside the capture)

Do this once, in this order, on the fresh site. Nothing here is recorded.

1. Start the site, open `/Admin` once so Auto Setup runs, then run `Recorder setup` so `state.json` exists.
2. **Enable Autoroute.** Open **Tools > Deployments > JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:19-30`).
   Put the first recipe below in the **JSON** field (`Views/Import/Json.cshtml:15`: a CodeMirror editor, so run
   `document.querySelector('.CodeMirror').CodeMirror.setValue(<text>)` in the page, then click **Import**
   (`Views/Import/Json.cshtml:21`)). The notice `Recipe imported successfully!` appears
   (`Controllers/ImportController.cs:185`). Autoroute adds the part filter of the video (`path`, collapsed into the
   item and into `where`: `OrchardCore.Autoroute/Startup.cs:92-98`), and the Headless recipe does not enable it. Wait
   until `/Admin` loads again, because enabling a feature restarts the tenant.

```json
{
  "steps": [
    {
      "name": "feature",
      "enable": [
        "OrchardCore.Autoroute"
      ]
    }
  ]
}
```

3. **Create the Blog Post type and its content.** Open **JSON Import** again, put the second recipe in the **JSON**
   field the same way, and click **Import**. The notice `Recipe imported successfully!` appears. The type has a title
   and an Autoroute path. Each post has a fixed id, a creation date and a path, so the results are always the same.

```json
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "BlogPost",
          "DisplayName": "Blog Post",
          "Settings": {
            "ContentTypeSettings": {
              "Creatable": true,
              "Draftable": true,
              "Versionable": true,
              "Listable": true
            }
          },
          "ContentTypePartDefinitionRecords": [
            {
              "PartName": "TitlePart",
              "Name": "TitlePart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "0"
                }
              }
            },
            {
              "PartName": "AutoroutePart",
              "Name": "AutoroutePart",
              "Settings": {
                "ContentTypePartSettings": {
                  "Position": "1"
                },
                "AutoroutePartSettings": {
                  "AllowCustomPath": true,
                  "Pattern": "blog/{{ Model.ContentItem | display_text | slugify }}",
                  "ShowHomepageOption": false
                }
              }
            }
          ]
        }
      ],
      "ContentParts": []
    },
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "4gqlfilters000000000000001",
          "ContentType": "BlogPost",
          "DisplayText": "Getting started with Orchard Core",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-01-06T09:00:00Z",
          "ModifiedUtc": "2025-01-06T09:00:00Z",
          "PublishedUtc": "2025-01-06T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Getting started with Orchard Core"
          },
          "AutoroutePart": {
            "Path": "tutorials/getting-started",
            "SetHomepage": false
          }
        },
        {
          "ContentItemId": "4gqlfilters000000000000002",
          "ContentType": "BlogPost",
          "DisplayText": "Content types explained",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-01-20T09:00:00Z",
          "ModifiedUtc": "2025-01-20T09:00:00Z",
          "PublishedUtc": "2025-01-20T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Content types explained"
          },
          "AutoroutePart": {
            "Path": "tutorials/content-types",
            "SetHomepage": false
          }
        },
        {
          "ContentItemId": "4gqlfilters000000000000003",
          "ContentType": "BlogPost",
          "DisplayText": "Liquid templates in practice",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-02-03T09:00:00Z",
          "ModifiedUtc": "2025-02-03T09:00:00Z",
          "PublishedUtc": "2025-02-03T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Liquid templates in practice"
          },
          "AutoroutePart": {
            "Path": "tutorials/liquid-templates",
            "SetHomepage": false
          }
        },
        {
          "ContentItemId": "4gqlfilters000000000000004",
          "ContentType": "BlogPost",
          "DisplayText": "Securing the admin",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-02-17T09:00:00Z",
          "ModifiedUtc": "2025-02-17T09:00:00Z",
          "PublishedUtc": "2025-02-17T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Securing the admin"
          },
          "AutoroutePart": {
            "Path": "guides/securing-the-admin",
            "SetHomepage": false
          }
        },
        {
          "ContentItemId": "4gqlfilters000000000000005",
          "ContentType": "BlogPost",
          "DisplayText": "Deploying to a container",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-03-03T09:00:00Z",
          "ModifiedUtc": "2025-03-03T09:00:00Z",
          "PublishedUtc": "2025-03-03T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Deploying to a container"
          },
          "AutoroutePart": {
            "Path": "guides/deploying-to-a-container",
            "SetHomepage": false
          }
        },
        {
          "ContentItemId": "4gqlfilters000000000000006",
          "ContentType": "BlogPost",
          "DisplayText": "Searching your content",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-03-17T09:00:00Z",
          "ModifiedUtc": "2025-03-17T09:00:00Z",
          "PublishedUtc": "2025-03-17T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Searching your content"
          },
          "AutoroutePart": {
            "Path": "guides/searching-your-content",
            "SetHomepage": false
          }
        },
        {
          "ContentItemId": "4gqlfilters000000000000007",
          "ContentType": "BlogPost",
          "DisplayText": "Headless with GraphQL",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-04-07T09:00:00Z",
          "ModifiedUtc": "2025-04-07T09:00:00Z",
          "PublishedUtc": "2025-04-07T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Headless with GraphQL"
          },
          "AutoroutePart": {
            "Path": "guides/headless-with-graphql",
            "SetHomepage": false
          }
        },
        {
          "ContentItemId": "4gqlfilters000000000000008",
          "ContentType": "BlogPost",
          "DisplayText": "Backing up a tenant",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-04-21T09:00:00Z",
          "ModifiedUtc": "2025-04-21T09:00:00Z",
          "PublishedUtc": "2025-04-21T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Backing up a tenant"
          },
          "AutoroutePart": {
            "Path": "guides/backing-up-a-tenant",
            "SetHomepage": false
          }
        },
        {
          "ContentItemId": "4gqlfilters000000000000009",
          "ContentType": "BlogPost",
          "DisplayText": "Roadmap notes",
          "Latest": true,
          "Published": false,
          "CreatedUtc": "2025-05-05T09:00:00Z",
          "ModifiedUtc": "2025-05-05T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Roadmap notes"
          },
          "AutoroutePart": {
            "Path": "guides/roadmap-notes",
            "SetHomepage": false
          }
        }
      ]
    }
  ]
}
```

The nine posts, from the oldest to the newest:

| # | Display text | Path | Created | State |
|---|---|---|---|---|
| 1 | Getting started with Orchard Core | `tutorials/getting-started` | 2025-01-06T09:00:00Z | published |
| 2 | Content types explained | `tutorials/content-types` | 2025-01-20T09:00:00Z | published |
| 3 | Liquid templates in practice | `tutorials/liquid-templates` | 2025-02-03T09:00:00Z | published |
| 4 | Securing the admin | `guides/securing-the-admin` | 2025-02-17T09:00:00Z | published |
| 5 | Deploying to a container | `guides/deploying-to-a-container` | 2025-03-03T09:00:00Z | published |
| 6 | Searching your content | `guides/searching-your-content` | 2025-03-17T09:00:00Z | published |
| 7 | Headless with GraphQL | `guides/headless-with-graphql` | 2025-04-07T09:00:00Z | published |
| 8 | Backing up a tenant | `guides/backing-up-a-tenant` | 2025-04-21T09:00:00Z | published |
| 9 | Roadmap notes | `guides/roadmap-notes` | 2025-05-05T09:00:00Z | draft (Published false, Latest true) |

4. **Check the seed.** Open `/Admin/GraphQL`, put the query below in the editor (or open
   `/Admin/GraphQL?query=` followed by the query, URL-encoded), and run it with the play button:

```
{
  blogPost(status: LATEST) {
    displayText
    published
    createdUtc
    path
  }
}
```

   Expected: nine items, in the order of the table above read from the bottom up (Roadmap notes first, with
   `"published": false`, then Backing up a tenant down to Getting started with Orchard Core, all `true`), each with its
   path, and a `createdUtc` that matches the table (the exact text can end in `Z` or carry fractions of a second).
   If the order is not this one, the import did not keep `CreatedUtc`: do not record, and report it. If the
   query fails with a message about the field `path`, open the Explorer pane, look at the fields of `blogPost` and at
   the arguments of `where`, and report it: the Autoroute part is collapsed, so its `path` is a field of the item
   and `path_starts_with` is a field of `where` (`OrchardCore.Autoroute/Startup.cs:92-98`,
   `ContentManagement.GraphQL/Queries/Types/TypedContentTypeBuilder.cs:59-95` and `:125-131`).
5. **Snapshot.** Stop the site and copy its `App_Data` folder and `state.json` aside. To record again from nothing,
   restore the copy and start the site again.

## 3. The order of the clips

No clip changes the site: a query only reads. Each clip opens its own browser context from `state.json`, so the
clips can be recorded in any order, and one clip can be recorded again alone, with no snapshot to restore. The
order of the video is the natural one: `02-filter`, `04-sort`, `05-page`, `06-status`, `07-combine`, `08-limit-error`.
Narrate before recording (`narrate.py`), because every step is held for the length of its narration.

## 4. What every clip does

Labels and selectors used by the clips:

| What | Where it comes from |
|---|---|
| Page `/Admin/GraphQL`, reached by **Tools > GraphiQL** | `OrchardCore.Apis.GraphQL/AdminMenu.cs:18-19` (labels), `Controllers/AdminController.cs:9` (the route) |
| The editor, filled from the `query` parameter of the address | `OrchardCore.Apis.GraphQL/Assets/js/App.tsx:46` (`parseQueryFromUrl`) |
| Run button, `button.execute-button`, title `Execute Query (Ctrl-Enter)` | GraphiQL, in the bundle `wwwroot/Scripts/graphiql-orchard.js` (it runs on mouse down) |
| Result pane, `.result-window`; the editor, `.query-editor .CodeMirror` | the same bundle |
| Explorer pane, opened by default; its button is titled `Toggle Explorer` and labeled `Explorer` | `Assets/js/App.tsx:118-119` |
| Menu collapse button, `button.leftbar-compactor`, title `Collapse / expand menu` | `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:79` |

GraphiQL's editor closes brackets as you type, so queries are never typed: each one is put in the address as the
`query` parameter, which loads it in the editor. Keep the Explorer pane open in every clip (it is part of the page).
No highlight rings are used in this video.

The helpers that every clip shares, in `recorder/Scenes.cs`:

```csharp
private const string Execute = "button.execute-button";

private static string Url(string query) => "/Admin/GraphQL?query=" + Uri.EscapeDataString(query);

// Waits until GraphiQL has drawn the editor and the run button.
private static async Task ReadyAsync(Scene scene)
{
    await scene.Page.Locator("#graphiql .loader").WaitForAsync(new() { State = WaitForSelectorState.Detached });
    await scene.Page.Locator(".query-editor .CodeMirror").WaitForAsync();
    await scene.Page.Locator(Execute).WaitForAsync();
}

// Before the capture: collapses the admin menu, so the editor and the result have the width.
private static async Task CollapseMenuAsync(Scene scene)
{
    var compact = await scene.Page.EvaluateAsync<bool>("document.body.classList.contains('left-sidebar-compact')");
    if (!compact)
    {
        await scene.Page.Locator("button.leftbar-compactor").ClickAsync();
        await Task.Delay(700);
    }
}

// On camera: loads the next query in the editor.
private static async Task NextQueryAsync(Scene scene, string query)
{
    await scene.NavigateAsync(Url(query));
    await ReadyAsync(scene);
    await scene.Page.EvaluateAsync("document.body.classList.add('left-sidebar-compact')");
}

// On camera: runs the query, and waits for the text that only its result has.
private static async Task RunAsync(Scene scene, string expected)
{
    await scene.ClickAsync(scene.Page.Locator(Execute), pauseAfter: 400);
    await scene.Page.WaitForFunctionAsync("text => document.querySelector('.result-window')?.innerText.includes(text)", expected);
}
```

The page is reloaded for every query, so the result pane is empty until the run: a wait on a title of the new
result cannot be met by the previous result. A clip opens with `OpenAsync(Url(first query))`, `ReadyAsync`, and (except
`02-filter`) `CollapseMenuAsync`, then `StartCaptureAsync`.

## 5. The queries

Each query is used exactly as written, with two spaces of indentation.

**Q1** (`where-1`)

```
{
  blogPost {
    displayText
  }
}
```

**Q2** (`where-2`)

```
{
  blogPost(where: { displayText_contains: "with" }) {
    displayText
  }
}
```

**Q3** (`where-3`)

```
{
  blogPost(
    where: {
      path_starts_with: "tutorials/"
    }
  ) {
    displayText
    path
  }
}
```

**Q4** (`where-4`)

```
{
  blogPost(
    where: {
      OR: [
        { displayText_contains: "Liquid" }
        { displayText_contains: "GraphQL" }
      ]
    }
  ) {
    displayText
  }
}
```

**Q5** (`sort-1`)

```
{
  blogPost(orderBy: { displayText: ASC }) {
    displayText
  }
}
```

**Q6** (`sort-2`)

```
{
  blogPost(orderBy: { createdUtc: ASC }) {
    displayText
    createdUtc
  }
}
```

**Q7, Q8, Q9** (`page-1`, `page-2`, `page-3`): the same query with `first: 3`, then `first: 3, skip: 3`, then
`first: 3, skip: 6`.

```
{
  blogPost(first: 3) {
    displayText
  }
}
```

```
{
  blogPost(first: 3, skip: 3) {
    displayText
  }
}
```

```
{
  blogPost(first: 3, skip: 6) {
    displayText
  }
}
```

**Q10, Q11** (`status-1`, `status-2`)

```
{
  blogPost(status: DRAFT) {
    displayText
    published
  }
}
```

```
{
  blogPost(status: LATEST) {
    displayText
    published
  }
}
```

**Q12** (`combine-1`)

```
{
  blogPost(
    where: {
      path_starts_with: "guides/"
    }
    orderBy: { displayText: ASC }
    first: 2
    skip: 2
  ) {
    displayText
  }
}
```

**Q13** (`limits-3`)

```
{
  blogPost(first: 1001) {
    displayText
  }
}
```

The names of the arguments and of their values come from `ContentManagement.GraphQL/Queries/ContentItemsFieldType.cs:47-75`
(`where`, `orderBy`, `first`, `skip`, `status`), `Queries/PublicationStatusGraphType.cs:12-15`
(`PUBLISHED`, `DRAFT`, `LATEST`, `ALL`) and `Queries/Types/ContentItemOrderByInput.cs:12-39` (the fields, and `ASC`
and `DESC`). The filters come from `Queries/Types/ContentItemWhereInput.cs:26-47` (`displayText` with its suffixes,
`OR`, `AND`, `NOT`) and `Apis.GraphQL.Abstractions/Queries/WhereInputObjectGraphType.cs:32-63` (the suffixes), and the
`path` filter (and the `path` field of the item) from `OrchardCore.Autoroute/GraphQL/AutorouteInputObjectType.cs:16` and
`AutorouteQueryObjectType.cs:14`: the Autoroute part is collapsed (`OrchardCore.Autoroute/Startup.cs:92-98`), so
`TypedContentTypeBuilder.cs:59-95` rolls `path` onto the item and `:125-131` adds `path`, `path_starts_with` and the
other suffixes to `where` itself, with no object named after the part. `ContentItemsFieldType.cs:299-305` maps the key
to the index alias `autoroutePart`.

## 6. The recordings

### Slides

Each line says what the slide shows (`slides.py`).

- `args-1`: "Five arguments on every content type query": the declaration of the five arguments of `ContentItemsFieldType`
  (`ContentItemsFieldType.cs:44-76`) in a code panel, and three cards: where and orderBy, first and skip, status.
- `args-2`: "What a query returns without arguments": three cards, the defaults of `status` (PUBLISHED, line 74),
  `orderBy` (createdUtc descending, line 472) and `first` (100, the `DefaultNumberOfResults` setting, lines 207-210).
- `args-3`: "One database query, with the page taken last": the lines of `ResolveAsync` that apply the version, the type, the
  order, the filters and the page (`ContentItemsFieldType.cs:109-114`), the page line bright, and a card.
- `ops-1`: "Filter operators by kind of field": four cards (every field, strings, identifiers, dates), from
  `WhereInputObjectGraphType.cs:76-108` and `ContentItemWhereInput.cs:26-33`.
- `ops-2`: "Combine conditions": three cards, AND, OR and NOT (`ContentItemsFieldType.cs:320-335`; the names in capitals
  come from `ContentItemWhereInput.cs:10-12`).
- `ops-3`: "Filter on the fields of your own parts": three cards, the feature Content Fields Indexing (SQL)
  (`OrchardCore.ContentFields/Manifest.cs`), the supported field types (`ContentFields/GraphQL/Fields/ContentFieldsProvider.cs:17-96`)
  and the suffixes of their type.
- `limits-1`: "Settings for the number of results": the GraphQL configuration section with the three settings
  (`src/docs/reference/modules/Apis.GraphQL/README.md`, "Configuration"), and three cards.
- `limits-2`: "The validation mode": three cards, Enabled, Disabled and Default (`OrchardCore.Apis.GraphQL/Startup.cs:70-76`,
  `ValidationRules/MaxNumberOfResultsValidationRule.cs:53-69`).

### `02-filter`

Starts on `/Admin/GraphQL` with Q1 in the editor, the **admin menu expanded** (do not call `CollapseMenuAsync`), so
the viewer sees **Tools** open with **GraphiQL** selected (`OrchardCore.Apis.GraphQL/AdminMenu.cs:18-19`), and the
Explorer pane open. The result pane is empty.

- `where-1`: `RunAsync(scene, "Backing up a tenant")`. When the narration ends: the menu with **Tools** and
  **GraphiQL**, Q1 in the editor, and the result with eight `displayText` values from `Backing up a tenant` down to
  `Getting started with Orchard Core`, newest first. `Roadmap notes` is not listed.
- `where-2`: click `button.leftbar-compactor` with `scene.ClickAsync` (title `Collapse / expand menu`), wait 700 ms,
  then `NextQueryAsync(scene, Q2)` and `RunAsync(scene, "Headless with GraphQL")`. When the narration ends: the menu
  collapsed, Q2 in the editor, and a result with two items, `Headless with GraphQL` then
  `Getting started with Orchard Core`.
- `where-3`: `NextQueryAsync(scene, Q3)`, `RunAsync(scene, "tutorials/liquid-templates")`. When the narration ends:
  Q3 in the editor, and a result with three items, each with its `path`: `Liquid templates in practice`
  (`tutorials/liquid-templates`), `Content types explained` (`tutorials/content-types`) and
  `Getting started with Orchard Core` (`tutorials/getting-started`).
- `where-4`: `NextQueryAsync(scene, Q4)`, `RunAsync(scene, "Liquid templates in practice")`. When the narration ends:
  Q4 in the editor, and a result with two items, `Headless with GraphQL` then `Liquid templates in practice`.

### `04-sort`

Starts with Q5 in the editor, the menu collapsed, the Explorer open, the result empty.

- `sort-1`: `RunAsync(scene, "Backing up a tenant")`. When the narration ends: Q5, and the eight titles in alphabetical
  order: `Backing up a tenant`, `Content types explained`, `Deploying to a container`, `Getting started with Orchard Core`,
  `Headless with GraphQL`, `Liquid templates in practice`, `Searching your content`, `Securing the admin`.
- `sort-2`: `NextQueryAsync(scene, Q6)`, `RunAsync(scene, "Getting started with Orchard Core")`. When the narration
  ends: Q6, and the result starts with `Getting started with Orchard Core` (`2025-01-06T09:00:00Z`), then
  `Content types explained` (`2025-01-20...`), the oldest first. The last items can be below the visible part.

### `05-page`

Starts with Q7 in the editor, the menu collapsed, the Explorer open, the result empty.

- `page-1`: `RunAsync(scene, "Searching your content")`. When the narration ends: Q7, and three items:
  `Backing up a tenant`, `Headless with GraphQL`, `Searching your content`.
- `page-2`: `NextQueryAsync(scene, Q8)`, `RunAsync(scene, "Securing the admin")`. When the narration ends: Q8, and three
  items: `Deploying to a container`, `Securing the admin`, `Liquid templates in practice`.
- `page-3`: `NextQueryAsync(scene, Q9)`, `RunAsync(scene, "Getting started with Orchard Core")`. When the narration ends:
  Q9, and two items: `Content types explained`, `Getting started with Orchard Core`.

### `06-status`

Starts with Q10 in the editor, the menu collapsed, the Explorer open, the result empty.

- `status-1`: `RunAsync(scene, "Roadmap notes")`. When the narration ends: Q10, and one item,
  `Roadmap notes` with `"published": false`.
- `status-2`: `NextQueryAsync(scene, Q11)`, `RunAsync(scene, "Roadmap notes")`. When the narration ends: Q11, and the
  result starts with `Roadmap notes` (`"published": false`), then `Backing up a tenant` (`"published": true`), and so on
  for nine items; the last ones can be below the visible part.

### `07-combine`

Starts with Q12 in the editor, the menu collapsed, the Explorer open, the result empty.

- `combine-1`: `RunAsync(scene, "Searching your content")`. When the narration ends: Q12, and two items,
  `Headless with GraphQL` then `Searching your content`.

### `08-limit-error` (the second clip of chapter 08, after the two slides)

Starts with Q13 in the editor, the menu collapsed, the Explorer open, the result empty. The site runs with
`OrchardCore__Apis__GraphQL__MaxNumberOfResultsValidationMode=Enabled`.

- `limits-3`: `RunAsync(scene, "exceeds the maximum number of results")`. When the narration ends: Q13, and in the result
  an `errors` list with the message `'1001' exceeds the maximum number of results for 'first' (1000)`
  (`ValidationRules/MaxNumberOfResultsValidationRule.cs:55-59`) and no `data`.

## 7. Checks for private data

- Every name on screen is one of the nine titles above; the admin's name and email are not shown (do not open the user
  menu). The recording needs no mask script, because the site is new and holds only the seed.
- The address bar is not part of the capture. The password is never typed on camera: `Recorder setup` signs in
  outside the capture.
- After assembling, OCR the video at one frame per second and at every scene change (see the skill) and search for
  `@`, for names other than the nine titles, and for `<password>`.

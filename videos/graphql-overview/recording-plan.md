# Recording plan: GraphQL: Query Your Content With One Request

How to record this video without deciding anything. Read it from the top. `storyboard.json` holds the narration; this
file holds everything else. File references (`file:line`) are in the Orchard Core source at commit `92db4a83`
(`src/OrchardCore.Modules/...` unless a path says otherwise). In this file a selector stands for
`scene.Page.Locator(...)`.

The video has two recordings (`02-enable`, `03-query`) and five slide clips (`01-why`, `04-schema`, `05-http`,
`06-config`, `07-extend`) that need no site. No GraphiQL query is typed on camera: the two queries are loaded from
the address, because the editor (CodeMirror) closes braces by itself and would change typed text.

## 1. The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout at or after commit `92db4a83`, on `http://localhost:5010` |
| App data | `ORCHARD_APP_DATA` pointing to an empty folder outside both repositories |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address (the site calls no external service) |
| Provisioning | Auto Setup, with the environment variables below, so no setup form and no sign-in form is recorded |
| Setup recipe | **Blog** (it does not enable GraphQL; the **Headless site** recipe does, so never use that one for this video) |
| Tenant | `Default`, site name **Orchard Core**, time zone `America/Los_Angeles` |
| Admin user | `admin`, email `admin@example.com`, password from `VIDEO_PASSWORD` (chosen outside git; never written in a file of this repository) |
| Mask script | none: the site is made from scratch with fake data only |
| External services | none |

Auto Setup environment variables (`<password>` is the value of `VIDEO_PASSWORD`):

```bash
OrchardCore__AutoSetup__Tenants__0__ShellName=Default
OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

(The names follow the Auto Setup documentation, `src/docs/reference/modules/AutoSetup/README.md`; the host must
register the Auto Setup feature, see the `orchardcore-tester` skill.)

Features: the Blog recipe enables Contents, Content Types, Content Fields, Lists, Markdown, Autoroute, Html, Title,
Media, Taxonomies and Deployment (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-81`). Enable
nothing else before recording. **GraphQL must stay disabled until the `02-enable` clip enables it on camera.**

Browser: the recorder's Chromium at 1536x864, light color scheme, `en-US`, signed in with `state.json`
(`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`).

## 2. The seed data (create it outside the capture)

The clips need one blog with exactly three published blog posts, so the sample post of the Blog recipe is replaced by
three posts with invented titles. Do this once, on the fresh site, before the first snapshot.

1. **Find the id of the Blog.** Open **Content, Content Items** (`/Admin/Contents/ContentItems`,
   `OrchardCore.Contents/AdminMenu.cs:68`), click the item named **Blog** (content type Blog). The address of its edit
   page is `/Admin/Contents/ContentItems/<blog-id>/Edit` (`OrchardCore.Contents/Controllers/AdminController.cs:502`).
   Copy `<blog-id>`.
2. **Import the three posts.** Open **Tools, Deployments, JSON Import** (`/Admin/DeploymentPlan/Import/Json`,
   `OrchardCore.Deployment/AdminMenu.cs:30`). Click into the **JSON** field, which is a code editor
   (`OrchardCore.Deployment/Views/Import/Json.cshtml:15`), select its content, replace it with the JSON below (with
   `<blog-id>` replaced by the id from step 1, in the three places), and select **Import**
   (`Json.cshtml:21`). The page shows the notification "Recipe imported successfully!"
   (`OrchardCore.Deployment/Controllers/ImportController.cs:185`).

   ```json
   {
     "steps": [
       {
         "name": "content",
         "Data": [
           {
             "ContentItemId": "dwdof7frmbuydevrr1xj5xf7ly",
             "ContentType": "BlogPost",
             "DisplayText": "Planning a Week in the Alps",
             "Latest": true,
             "Published": true,
             "CreatedUtc": "2026-03-02T09:00:00Z",
             "ModifiedUtc": "2026-03-02T09:00:00Z",
             "PublishedUtc": "2026-03-02T09:00:00Z",
             "Author": "admin",
             "TitlePart": { "Title": "Planning a Week in the Alps" },
             "AutoroutePart": { "Path": "blog/planning-a-week-in-the-alps" },
             "MarkdownBodyPart": { "Markdown": "Notes on trains, huts and maps for a week in the mountains." },
             "ContainedPart": { "ListContentItemId": "<blog-id>", "ListContentType": "Blog", "Order": 0 },
             "BlogPost": { "Subtitle": { "Text": "Trains, huts and a good map" } }
           },
           {
             "ContentItemId": "16kcqoi33qhar4cz4lqgeie807",
             "ContentType": "BlogPost",
             "DisplayText": "Packing Light for Long Walks",
             "Latest": true,
             "Published": true,
             "CreatedUtc": "2026-03-16T09:00:00Z",
             "ModifiedUtc": "2026-03-16T09:00:00Z",
             "PublishedUtc": "2026-03-16T09:00:00Z",
             "Author": "admin",
             "TitlePart": { "Title": "Packing Light for Long Walks" },
             "AutoroutePart": { "Path": "blog/packing-light-for-long-walks" },
             "MarkdownBodyPart": { "Markdown": "What to leave at home when the route is long." },
             "ContainedPart": { "ListContentItemId": "<blog-id>", "ListContentType": "Blog", "Order": 1 },
             "BlogPost": { "Subtitle": { "Text": "What stays at home" } }
           },
           {
             "ContentItemId": "zyfutu2r9h6z4q8u8fubx964y9",
             "ContentType": "BlogPost",
             "DisplayText": "A Lake Before Breakfast",
             "Latest": true,
             "Published": true,
             "CreatedUtc": "2026-03-30T09:00:00Z",
             "ModifiedUtc": "2026-03-30T09:00:00Z",
             "PublishedUtc": "2026-03-30T09:00:00Z",
             "Author": "admin",
             "TitlePart": { "Title": "A Lake Before Breakfast" },
             "AutoroutePart": { "Path": "blog/a-lake-before-breakfast" },
             "MarkdownBodyPart": { "Markdown": "A short walk to the water before the day starts." },
             "ContainedPart": { "ListContentItemId": "<blog-id>", "ListContentType": "Blog", "Order": 2 },
             "BlogPost": { "Subtitle": { "Text": "Early mornings by the water" } }
           }
         ]
       }
     ]
   }
   ```

3. **Remove the sample post.** In **Content, Content Items**, find the row **Man must explore, and this is exploration
   at its greatest** (`blog.recipe.json:1002`), open its actions menu, select **Delete**
   (`OrchardCore.Contents/Views/ContentsButtonActions_SummaryAdmin.cshtml:40`), and confirm with **Ok**.
4. **Check the list.** Content Items, filtered to the Blog Post type, holds exactly the three titles above: *Planning a
   Week in the Alps*, *Packing Light for Long Walks* and *A Lake Before Breakfast*. The Blog item is still named
   **Blog**.
5. Take **snapshot A**: stop the site, copy `ORCHARD_APP_DATA` and `state.json` aside, start the site again.

GraphQL is still disabled in snapshot A. The two queries are checked after `02-enable` (see section 3).

## 3. Order of the clips, and what changes the site

| Order | Clip | Starts from | Changes the site? |
|---|---|---|---|
| 1 | `02-enable` | snapshot A (GraphQL disabled) | **Yes**: it enables GraphQL. Before a new take, stop the site, restore snapshot A, start the site. |
| 2 | check | GraphQL enabled | no (see below) |
| 3 | `03-query` | the site left by `02-enable` | No. It can be recorded again without restoring anything. |

The slide clips (`01-why`, `04-schema`, `05-http`, `06-config`, `07-extend`) depend on nothing, and `assemble.py`
draws them from `slides.py`.

**Check before recording `03-query`** (not recorded): open **Tools, GraphiQL**, open the two addresses of `03-query`
below, run each query with the play button, and confirm:

- The Explorer on the left lists `blog` and `blogPost` among its fields (scroll it if needed, and look at whether
  `blogPost` is visible without scrolling: if it is not, write that down for the review).
- Query A returns the three titles of the seed, newest first: *A Lake Before Breakfast*, *Packing Light for Long
  Walks*, *Planning a Week in the Alps*.
- Query B returns one blog, named *Blog*, with `list.contentItems` holding the three posts in the order *Planning a
  Week in the Alps*, *Packing Light for Long Walks*, *A Lake Before Breakfast*, each with its subtitle.
- Neither response has an `errors` list.

If a field name differs from the narration (`blogPost`, `list`, `contentItems`, `subtitle`), stop: the narration of
`query-2` to `query-4` and the slides of `04-schema` name those fields. Do not change the narration while recording;
report the difference (see `OPEN-QUESTIONS.md`).

## 4. The clips

### 02-enable (chapter 02, a recording)

Starts on `/Admin` (the dashboard), opened with `scene.OpenAsync("/Admin")` before `StartCaptureAsync`. Signed in as
`admin`. GraphQL is disabled, and the **Tools** group of the admin menu is collapsed.

- `enable-1` (narration: open Tools, Features, search for GraphQL). Click **Tools** in the admin menu
  (`OrchardCore.Features/AdminMenu.cs:26`), then click **Features** (`AdminMenu.cs:27`) with
  `ClickAndWaitForNavigationAsync`; if the menu entry cannot be clicked in a take, use
  `scene.NavigateAsync("/Admin/Features")` instead. On the Features page, type `GraphQL` into the search box
  `#search-box` (`OrchardCore.Features/Views/Admin/Features.cshtml:27`) with `TypeAsync(..., delay: 90)`. Wait until
  the list shows the **GraphQL** row (its **Enable** button `#btn-enable-OrchardCore_Apis_GraphQL`,
  `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
  *Visible when the narration ends:* the Features page, **GraphQL** in the search box, the **GraphQL** row under the
  **Api** heading (`Features.cshtml:88`; the category is `Api`, `OrchardCore.Apis.GraphQL/Manifest.cs:9`; the feature
  name is `GraphQL`, `Manifest.cs:4`), with its description "Apis using the GraphQL Specification." and a blue
  **Enable** button.
- `enable-2` (narration: listed under the Api category, select Enable, a notification confirms). Move to the row
  with `HighlightAsync(<the GraphQL row>, null)` for about one second (the row is the `.list-group-item` that holds
  `#btn-enable-OrchardCore_Apis_GraphQL`, as in the `Row()` helper of `Scenes.cs`), then click **Enable** with
  `ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_Apis_GraphQL)`. The click enables the feature and restarts
  the tenant, so the next page can take several seconds: follow the click with
  `OffCameraAsync(() => <wait until #btn-disable-OrchardCore_Apis_GraphQL is visible>)`, so the wait is cut out of the
  clip. Call `ClearHighlightsAsync()` before the click.
  *Visible when the narration ends:* the Features page again, with the notification "The feature GraphQL was
  enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`) at the top of the page.
- `enable-3` (narration: Tools has a GraphiQL entry; modules add their types). Click **Tools** in the admin menu if
  its group is collapsed after the reload, so that **GraphiQL** (`OrchardCore.Apis.GraphQL/AdminMenu.cs:19`) is
  visible, and call `HighlightAsync(<the GraphiQL menu entry>, null)`. Do not click it.
  *Visible when the narration ends:* the admin menu with **Tools** open and a green ring around **GraphiQL**.

End of the clip: `ClearHighlightsAsync()`. Leave the site as it is (GraphQL enabled).

### 03-query (chapter 03, a recording)

Starts on `/Admin`, opened with `scene.OpenAsync("/Admin")` before `StartCaptureAsync`, with the **Tools** group of the
admin menu expanded (click **Tools** before the capture) so that **GraphiQL** is visible.

The two addresses (the query is the `query` parameter that the GraphiQL page reads into its editor,
`OrchardCore.Apis.GraphQL/Assets/js/App.tsx:80`):

- **Address A**: `/Admin/GraphQL?query=%7B%0A%20%20blogPost%20%7B%0A%20%20%20%20displayText%0A%20%20%7D%0A%7D`,
  which loads the query

  ```graphql
  {
    blogPost {
      displayText
    }
  }
  ```

- **Address B**:
  `/Admin/GraphQL?query=%7B%0A%20%20blog%20%7B%0A%20%20%20%20displayText%0A%20%20%20%20list%20%7B%0A%20%20%20%20%20%20contentItems%20%7B%0A%20%20%20%20%20%20%20%20...%20on%20BlogPost%20%7B%0A%20%20%20%20%20%20%20%20%20%20displayText%0A%20%20%20%20%20%20%20%20%20%20subtitle%0A%20%20%20%20%20%20%20%20%7D%0A%20%20%20%20%20%20%7D%0A%20%20%20%20%7D%0A%20%20%7D%0A%7D`,
  which loads the query

  ```graphql
  {
    blog {
      displayText
      list {
        contentItems {
          ... on BlogPost {
            displayText
            subtitle
          }
        }
      }
    }
  }
  ```

Locators used below: the play button `.execute-button` (title "Execute Query (Ctrl-Enter)"), the response pane
`.result-window`, the query editor `.query-editor`, the Explorer `.graphiql-explorer-root`. The four class names are in
the bundled script `OrchardCore.Apis.GraphQL/wwwroot/Scripts/graphiql-orchard.js` (GraphiQL 1.x, `Assets/package.json`),
and the Explorer class is also in `Assets/css/graphiql-orchard.css:5`; none of them was run, so confirm all four with
`Recorder probe` before the first take.

- `query-1` (narration: Tools, GraphiQL opens a page; Explorer on the left, editor in the middle, response on the
  right). Click **GraphiQL** in the admin menu with `ClickAndWaitForNavigationAsync`
  (`OrchardCore.Apis.GraphQL/AdminMenu.cs:19`; the page is `/Admin/GraphQL`,
  `OrchardCore.Apis.GraphQL/Controllers/AdminController.cs:9`). Wait until the Explorer (`.graphiql-explorer-root`)
  shows the text `blogPost` (the schema is loaded by an introspection request, `App.tsx:54`). Then move the cursor
  slowly from the Explorer, over the editor, to the response pane (`MoveToAsync` on `.graphiql-explorer-root`,
  `.query-editor`, `.result-window`, about one second each).
  *Visible when the narration ends:* the GraphiQL page: the Explorer pane with its list of fields on the left, an
  editor in the middle with GraphiQL's default content (a comment that starts with "# Welcome to GraphiQL", from the
  bundled script), an empty response pane on the right, and the toolbar with the play button and the **Explorer**
  button (`App.tsx:118`).
- `query-2` (narration: each content type is a field; BlogPost is blogPost; the query asks for the display text).
  Load address A with `scene.NavigateAsync(<address A>)`. Wait until `.query-editor` contains `displayText` and the
  Explorer shows `blogPost`. Call `HighlightAsync(<the blogPost row of the Explorer>, null)` (a text locator:
  `scene.Page.Locator(".graphiql-explorer-root").GetByText("blogPost", new() { Exact = true }).First`).
  *Visible when the narration ends:* the query `{ blogPost { displayText } }` in the editor, a ring around `blogPost`
  in the Explorer, and no response yet.
- `query-3` (narration: run it; JSON with a data property; blogPost is a list; newest first). Call
  `ClearHighlightsAsync()`, then `ClickAsync(.execute-button)`. Wait until `.result-window` contains `"displayText"`
  (concrete: the text `A Lake Before Breakfast` is in `.result-window`).
  *Visible when the narration ends:* the response pane shows `"data"`, `"blogPost"` and three entries, each with only
  `"displayText"`: *A Lake Before Breakfast*, *Packing Light for Long Walks*, *Planning a Week in the Alps*, in this
  order. The default order is the creation date, newest first
  (`OrchardCore.ContentManagement.GraphQL/Queries/ContentItemsFieldType.cs:472`).
- `query-4` (narration: one request follows relationships; the list part of the blog holds its posts; inline
  fragment on BlogPost; run it). Load address B with `scene.NavigateAsync(<address B>)`, wait until `.query-editor`
  contains `contentItems`, then `ClickAsync(.execute-button)`. Wait until `.result-window` contains
  `Early mornings by the water`. The `list` field is the ListPart of the Blog type (`ToFieldName` removes the suffix
  "Part", `OrchardCore.ContentManagement.GraphQL/Extensions/StringExtensions.cs:10`), and `contentItems` is
  `OrchardCore.Lists/GraphQL/ListQueryObjectType.cs:24`.
  *Visible when the narration ends:* the editor shows the nested query, and the response shows one entry of `blog`
  with `"displayText": "Blog"` and `"list"`, whose `"contentItems"` holds the three posts, each with `"displayText"`
  and `"subtitle"`: *Planning a Week in the Alps* / *Trains, huts and a good map*, *Packing Light for Long Walks* /
  *What stays at home*, *A Lake Before Breakfast* / *Early mornings by the water*. The whole response fits the pane
  without scrolling; if it does not, scroll nothing and report it.

Keep every step's actions shorter than its narration (the narration of a step lasts about 14 to 16 seconds).

### The slide clips

Each slide step is drawn by `slides.py`. One line each:

- `why-1`: three cards: a fixed response shape, fields you never use, related data needs more requests.
- `why-2`: a query (`blogPost` with `displayText` and `subtitle`) beside a response of the same shape, and a card
  "One request".
- `why-3`: four check cards: one endpoint, built from your content, no code for a content type, permission checks.
- `schema-1`: four cards: content type is a query field, a part with fields or a GraphQL type is an object, a part
  named like its type is collapsed, settings can change it.
- `schema-2`: a query with the arguments `where`, `orderBy`, `first`, `skip` and `status`, the `contentItem` query,
  and three cards naming the arguments.
- `schema-3`: three cards: change a content type, send the next request, reload GraphiQL.
- `http-1`: a POST request with a JSON body and a GET request with the query in the query string, as curl commands.
- `http-2`: a POST with the `application/graphql` content type, and a JSON body with variables.
- `http-3`: a JSON response with a `data` property, and cards for the status codes 200, 400 and 401.
- `http-4`: four cards: the Execute GraphQL permission, a session cookie, a bearer token, anonymous users.
- `http-5`: two cards on content permissions.
- `config-1`: the options of the GraphQL module as a JSON configuration, and three cards.
- `ext-1`: the Autoroute query type and the startup class that registers it.
- `ext-2`: an `ISchemaBuilder` implementation and its registration.

## 5. Checks for private data

- Every name on screen is invented: the blog, the three posts and their subtitles are in section 2. The only account
  is `admin` with `admin@example.com`. No password is typed or shown on camera (the sign-in comes from `state.json`).
- After assembling, OCR the video at one frame per second and at every scene change, and search for `@` (only
  `example.com` may appear), for the machine name, the user name of the operating system, and for any path of this
  machine. The slides only use `example.com` and a `<token>` placeholder; there is no real token anywhere.
- The browser frame has no address bar, but the pages of the admin can show the site's host name: it must be
  `localhost`.
- Make sure the sample post of the Blog recipe is gone and that no other content item (for example a draft) appears in
  the responses.

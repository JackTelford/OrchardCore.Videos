# Recording plan: Headless: Serve Content to Any Front End

Audience: developer. Three recorded clips (`02-graphql`, `03-cors`, `04-openid`) and seven slide clips. Everything is
recorded in a browser. The terminal command of `cors-6` and the recipes of `recipe-1` and `recipe-2` are slides, drawn
by `slides.py`.

Labels below come from the Orchard Core source at the commit named in `catalog/SOURCE.md`. A label is quoted as the
code spells it, with its `file:line` in `src/OrchardCore.Modules` (or `src/OrchardCore` where noted). In this file a
selector stands for `scene.Page.Locator(...)`.

## 1. The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web`, started on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own
  `App_Data` outside both repositories (`ORCHARD_APP_DATA`). Outbound calls are blocked with `HTTP_PROXY` and
  `HTTPS_PROXY` set to a dead address for the site process.
- **Auto Setup**, so no setup or sign-in form is recorded (`OrchardCore.AutoSetup` documentation, "User Secrets and
  Environment Variables"):
  - site name `Orchard Core`, tenant `Default`, time zone `America/Los_Angeles`, database provider `Sqlite`
  - recipe `Blog` (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:2`)
  - user `admin`, email `admin@example.com`, password chosen outside git and passed as `VIDEO_PASSWORD`
    (written `<password>` here; it is never typed on camera, because `Recorder setup` saves the sign-in in
    `state.json`, which is git-ignored)
- **Time zone and culture**: the browser is `en-US`, light color scheme, as the recorder sets it.
- **Features to enable before recording**: none. The Blog recipe enables neither GraphQL, CORS Configuration nor any
  OpenID Connect feature (`blog.recipe.json:31-80`). The clips enable them on camera. Before the first take, check
  in **Tools > Features** that these four are disabled: `GraphQL`, `CORS Configuration`,
  `OpenID Connect Authorization Server` and `OpenID Connect Token Validation`.
- **Settings**: none changed outside the capture. The role `Anonymous` must not have the permission
  `Execute GraphQL.` before `graphql-2`. It keeps its default permission `View all content`
  (`OrchardCore.Contents/Permissions.cs:91-95`, label in `OrchardCore.Contents.Core/CommonPermissions.cs:29`), which is
  what lets the stand-in front end read the published posts; check it in the same role editor before the first take.
- **External services**: none. The only second process is the stand-in front end below.

### The stand-in front end

The video shows "a front end on its own address". It is a one-page static site served from a different origin than
Orchard Core, started outside the capture and stopped afterwards. Create a folder outside both repositories, for
example `%TEMP%/ocvideo/frontend`, with this `index.html` (fake text only):

```html
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>Sample Front End</title>
  <style>
    body { font-family: "Open Sans", Arial, sans-serif; font-size: 24px; margin: 56px; color: #404041; }
    h1 { font-size: 40px; margin: 0 0 8px 0; }
    .origins { color: #707070; font-size: 22px; margin-bottom: 32px; }
    button { font-size: 24px; padding: 12px 28px; cursor: pointer; }
    #status { margin: 28px 0; min-height: 36px; }
    #status.error { color: #b00020; }
    li { margin: 10px 0; }
  </style>
</head>
<body>
  <h1>Sample Front End</h1>
  <div class="origins">This page: http://127.0.0.1:5020 &middot; Content from: http://localhost:5010</div>
  <button id="load">Load posts</button>
  <p id="status">Nothing loaded.</p>
  <ul id="posts"></ul>
  <script>
    const endpoint = "http://localhost:5010/api/graphql";
    const status = document.getElementById("status");
    const posts = document.getElementById("posts");

    document.getElementById("load").addEventListener("click", async () => {
      posts.innerHTML = "";
      status.className = "";
      status.textContent = "Loading...";
      try {
        const response = await fetch(endpoint, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ query: "{ blogPost { displayText } }" })
        });
        const result = await response.json();
        if (result.errors) {
          throw new Error(result.errors[0].message);
        }
        status.textContent = "Loaded " + result.data.blogPost.length + " posts.";
        for (const post of result.data.blogPost) {
          const item = document.createElement("li");
          item.textContent = post.displayText;
          posts.appendChild(item);
        }
      } catch (error) {
        status.className = "error";
        status.textContent = "The browser blocked the request: " + error.message;
      }
    });
  </script>
</body>
</html>
```

Serve it with `python -m http.server 5020 --bind 127.0.0.1 --directory <folder>`. Its origin is
`http://127.0.0.1:5020` (scheme, host and port: this exact text is what `cors-4` types). It sends no cookies and no
`Authorization` header, so Orchard Core sees an anonymous visitor.

### Seed data (create outside the capture, before the first take)

1. Start the site, run `Recorder setup` (signs in as `admin`, saves `state.json`).
2. Two extra blog posts, so the query returns three. Open **Tools > Deployments > JSON Import**
   (`OrchardCore.Deployment/AdminMenu.cs:30`, page `/Admin/DeploymentPlan/Import/Json`), paste this, select
   **Import**, and wait for the notice `Recipe imported successfully!` (`Controllers/ImportController.cs:185`):

   ```json
   {
     "steps": [
       {
         "name": "content",
         "data": [
           {
             "ContentType": "BlogPost",
             "ContentItemId": "[js: uuid()]",
             "DisplayText": "Summer Menu Preview",
             "Latest": true,
             "Published": true,
             "TitlePart": { "Title": "Summer Menu Preview" },
             "MarkdownBodyPart": { "Markdown": "Cold brew, lemonade and fruit tarts arrive next week." }
           },
           {
             "ContentType": "BlogPost",
             "ContentItemId": "[js: uuid()]",
             "DisplayText": "A Morning at the Roastery",
             "Latest": true,
             "Published": true,
             "TitlePart": { "Title": "A Morning at the Roastery" },
             "MarkdownBodyPart": { "Markdown": "The beans are roasted in small batches before the doors open." }
           }
         ]
       }
     ]
   }
   ```

   The Blog recipe's own sample post (`Man must explore, and this is exploration at its greatest`) stays: the
   result of the query has three titles. If the import fails, create the two posts from **Content > Content Items**
   with the same titles and publish them (see `OPEN-QUESTIONS.md`).
3. Do not enable any of the four features. Stop the site, copy `App_Data` and `state.json` aside as **snapshot S0**.

## 2. Order of the clips, and what each one changes

Slide clips (`01-idea`, `02-request`, `03-preflight`, `04-flow`, `05-recipe`) change nothing and need no recording.

| Order | Clip | Starts from | Changes the site | Depends on |
| --- | --- | --- | --- | --- |
| 1 | `02-graphql` | snapshot S0 | enables GraphQL; allows `Execute GraphQL.` for the role Anonymous | the seed |
| 2 | `03-cors` | snapshot S1 (after 1) | enables CORS Configuration; saves the policy `Frontend` | 1: the front end can read posts only if Anonymous may run queries |
| 3 | `04-openid` | snapshot S2 (after 2) | enables the OpenID Connect Authorization Server (with its Management UI and Core Services) and Token Validation; creates the application `Sample Front End` | none of the others, but is recorded last |

- Stop the site and copy `App_Data` aside after each clip: **S1** after `02-graphql`, **S2** after `03-cors`. To
  record one clip again, stop the site, restore the snapshot named in "Starts from", and start the site again (the
  front end server is a separate process and keeps running).
- A full take restores S0 first.
- `03-cors` needs the stand-in front end running on port 5020. `02-graphql` and `04-openid` do not use it.

## 3. Clip `02-graphql`

Starts on: `/Admin/Features`, signed in as `admin`, opened with `scene.OpenAsync` before `StartCaptureAsync`.

A helper for the rows of the features list (the same pattern as `scripts/recorder/Scenes.cs`):
`Row(id)` is `.list-group-item` filtered to the one that has `#btn-enable-<id>`. The id of an Enable button is
`btn-enable-` plus the feature id with dots replaced by underscores
(`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).

### `graphql-1`

1. `scene.TypeAsync(#search-box, "GraphQL", delay: 90)` (`OrchardCore.Features/Views/Admin/Features.cshtml:27`,
   placeholder `Search`). Wait until `Row("OrchardCore_Apis_GraphQL")` is visible.
2. `scene.HighlightAsync(Row("OrchardCore_Apis_GraphQL"), "GraphQL")`, hold briefly, then `ClearHighlightsAsync()`.
   The feature's name is `GraphQL` (`OrchardCore.Apis.GraphQL/Manifest.cs:4`).
3. `scene.ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_Apis_GraphQL)`. Wait until the text
   `The feature GraphQL was enabled.` is visible (`OrchardCore.Features/Controllers/AdminController.cs:317`).

Visible when the narration ends: the Features list with the success notice `The feature GraphQL was enabled.`
The `GraphiQL` menu entry under **Tools** (`OrchardCore.Apis.GraphQL/AdminMenu.cs:19`) is not shown on screen.

### `graphql-2`

1. `scene.NavigateAsync("/Admin/Roles/Edit/Anonymous")` (route `Roles/{action}/{id?}`,
   `OrchardCore.Roles/Controllers/AdminController.cs:22`). The page title is `Edit 'Anonymous' Role`.
2. `scene.TypeAsync(#search-box, "Execute GraphQL", delay: 60)`: the field with the placeholder
   `Search permissions` (`OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:49`). Two rows remain:
   `Execute GraphQL.` and `Execute GraphQL Mutations.`. The permission's description is `Execute GraphQL.`, with the
   period (`OrchardCore/OrchardCore.Apis.GraphQL.Abstractions/GraphQLPermissions.cs:12`).
3. Click the **Allow** checkbox of the row `Execute GraphQL.`: the selector is `[id='Checkbox.ExecuteGraphQL']`
   (the id has a dot, so use an attribute selector; `_RolePermissions.cshtml:131`). Do not touch the Mutations row.
   `scene.HighlightAsync(tr[data-text='Execute GraphQL.'])` for a moment, then `ClearHighlightsAsync()`.
4. `scene.ClickAndWaitForNavigationAsync(button.save)` (`OrchardCore.Roles/Views/Admin/Edit.cshtml:33`, text
   `Save`). Wait for the notice `Role updated successfully.` (`Controllers/AdminController.cs:213`).

Visible when the narration ends: the Roles list with the notice `Role updated successfully.`

### `graphql-3`

1. `scene.NavigateAsync("/Admin/GraphQL?query=%7B%20blogPost%20%7B%20displayText%20%7D%20%7D")`. This is the
   GraphiQL page (`[Admin("GraphQL", "GraphQL")]`, `OrchardCore.Apis.GraphQL/Controllers/AdminController.cs:9`). The
   `query` parameter fills the editor with `{ blogPost { displayText } }` (`Assets/js/App.tsx:46` and `:80`). Wait
   until the editor shows `blogPost`.
2. Click the toolbar button **Prettify** (title `Prettify Query (Shift-Ctrl-P)`), so the query is laid out on
   several lines. Fallback: skip this click if the button is not found.
3. Click the execute button, the triangle `button.execute-button` (title `Execute Query (Ctrl-Enter)`). Wait until
   the result pane contains `Summer Menu Preview`.

Visible when the narration ends: the query in the left editor, and the result pane on the right with a `data` object
holding `blogPost` and three `displayText` values: `Summer Menu Preview`, `A Morning at the Roastery` and
`Man must explore, and this is exploration at its greatest` (in any order).

## 4. Clip `03-cors`

Starts on: the stand-in front end. This clip uses two pages of the same browser context:

- **Before `StartCaptureAsync`**: `scene.Page` is the front end: `await scene.Page.GotoAsync("http://127.0.0.1:5020/",
  new() { WaitUntil = WaitUntilState.NetworkIdle })` (`OpenAsync` adds the site's base URL, so it cannot be used).
  Create the admin page: `var admin = await scene.Context.NewPageAsync()`, then `await admin.GotoAsync(scene.BaseUrl +
  "/Admin/Features", new() { WaitUntil = WaitUntilState.NetworkIdle })`. Call `StartCaptureAsync` (it records the
  front end page).
- `await scene.SwitchToAsync(page)` moves the capture from one page to the other; keep the two pages in variables
  (`app`, `admin`).

### `cors-1`

1. `scene.ClickAsync(app.GetByRole(Button, Name = "Load posts"))`. Wait until `#status` contains `blocked`.
2. `scene.HighlightAsync(#status)`, then `ClearHighlightsAsync()` at the start of the next step.

Visible when the narration ends: the page **Sample Front End**, with the line `This page: http://127.0.0.1:5020
· Content from: http://localhost:5010`, and the red status `The browser blocked the request: Failed to fetch`. No
list of posts. (The CORS Configuration feature is still disabled, so Orchard Core sends no CORS headers.)

### `cors-2`

1. `await scene.SwitchToAsync(admin)` (the Features list is already loaded).
2. `scene.TypeAsync(#search-box, "CORS", delay: 90)`; wait for `Row("OrchardCore_Cors")`. The feature is named
   `CORS Configuration` (`OrchardCore.Cors/Manifest.cs:4`).
3. `scene.ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_Cors)`. Wait for the notice
   `The feature CORS Configuration was enabled.`
4. `scene.NavigateAsync("/Admin/Cors")` (`[Admin("Cors", "CorsIndex")]`,
   `OrchardCore.Cors/Controllers/AdminController.cs:44`). The menu path is **Settings** > **Security** >
   **Cross-Origin Resource Sharing** (`OrchardCore.Cors/AdminMenu.cs:17-20`); the recording opens the page by its
   address.

Visible when the narration ends: the page titled `CORS Settings`, the yellow notice `The current tenant will be
reloaded when the CORS is executed.`, the search box, the button `Add a policy`
(`Services/CorsJSLocalizer.cs:17`), and the message `Nothing here! There are no CORS policies for the moment.`

### `cors-3`

1. `scene.ClickAsync(GetByRole(Button, Name = "Add a policy"))`. The policy editor opens. Its heading is `New policy`.
2. The name field is the first text input of the editor: `field = #corsAdmin input[type=text]` `.First`. It is
   prefilled with `New policy` (`Assets/ts/cors-admin-index.ts:283`), so replace the text: `scene.ClickAsync(field)`,
   `scene.Page.Keyboard.PressAsync("Control+A")`, then `field.PressSequentiallyAsync("Frontend", new() { Delay = 55 })`.
   The label is `Policy name`.
3. `scene.ClickAsync(#set-default-policy)` (label `Set as default policy`, `cors-admin-index.ts:110`).
4. `scene.ClickAsync(#allow-credentials)`. A new policy starts with it **checked** (`cors-admin-index.ts:290`), so this
   click turns it off (label `Allow credentials`, `cors-admin-index.ts:125`).

Visible when the narration ends: heading `Frontend`; `Policy name` = `Frontend`; `Set as default policy` checked;
`Allow credentials` unchecked.

### `cors-4`

1. The origin field is the second text input of the editor: `#corsAdmin input[type=text]` `.Nth(1)`, inside the card
   `Origins` (the list is titled `Allowed origins`). Leave `Allow any origin` unchecked.
   `scene.TypeAsync(field, "http://127.0.0.1:5020", delay: 45)`.
2. `scene.ClickAsync(GetByRole(Button, Name = "Add Origin"))`: the label is `Add` plus the option type `Origin`
   (`cors-admin-index.ts:71`, `CorsJSLocalizer.cs:21` and `:39`). The origin is listed under the field.
3. `scene.HighlightAsync(` the list item with the origin `)`, hold for a moment, `ClearHighlightsAsync()`. The cards
   `Headers` and `Methods` keep `Allow any header` and `Allow any method` checked (their defaults).
4. Save: `scene.WaitForNavigationAsync(() => scene.ClickAsync(.action-bar-bottom button with the text "Save"))`. The
   Save button submits the form (`cors-admin-index.ts`, `save()`); if it is below the fold, call
   `scene.ScrollAsync(0, 600)` first. Wait for the notice `The CORS settings have updated successfully.`
   (`Controllers/AdminController.cs:144`). The tenant reloads, so allow a few seconds.

Visible when the narration ends: the policy list with one row `Frontend`, a badge `Default Policy`, the buttons
`Edit` and `Delete`, and the green notice `The CORS settings have updated successfully.`

### `cors-5`

1. `await scene.SwitchToAsync(app)`.
2. `scene.ClickAsync(app.GetByRole(Button, Name = "Load posts"))`. Wait until `#status` starts with `Loaded` and
   `#posts li` has three items.

Visible when the narration ends: the status `Loaded 3 posts.` and the three titles in the list (`Summer Menu
Preview`, `A Morning at the Roastery`, `Man must explore, and this is exploration at its greatest`, in any order).
No red status.

## 5. Clip `04-openid`

Starts on: `/Admin/Features`, opened with `scene.OpenAsync` before `StartCaptureAsync`.

### `openid-1`

1. `scene.TypeAsync(#search-box, "OpenID", delay: 90)`. Wait for `Row("OrchardCore_OpenId_Server")`, named
   `OpenID Connect Authorization Server` (`OrchardCore.OpenId/Manifest.cs:44`).
2. `scene.ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_OpenId_Server)`. The click enables the feature with
   its dependencies (OpenID Connect Core Services and OpenID Connect Management UI). Wait for a success notice
   that names the enabled features (`AdminController.cs:317`).
3. `scene.TypeAsync(#search-box, "OpenID", delay: 90)` again (the list is shown unfiltered after the redirect). Wait
   for `Row("OrchardCore_OpenId_Validation")`, named `OpenID Connect Token Validation` (`Manifest.cs:68`).
4. `scene.ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_OpenId_Validation)`. Wait for the notice
   `The feature OpenID Connect Token Validation was enabled.`

Visible when the narration ends: the Features list with that notice. Enabling takes several seconds per feature; if
the two enables outlast the narration the step only runs longer, so record it again only if the final frame is not
the notice.

### `openid-2`

1. `scene.NavigateAsync("/Admin/OpenId/ServerConfiguration")` (`ServerConfigurationController.cs:49`; menu path
   **Settings** > **OpenID Connect** > **Authorization Server**, `ServerAdminMenu.cs:22`). The page title is
   `Configure OpenID Connect server settings`.
2. Do not change or save anything. The `Endpoints` section lists the checked boxes `Enable Token Endpoint`,
   `Enable Authorization Endpoint` and the others (`Views/OpenIdServerSettings.Edit.cshtml:25`). They are checked
   because no settings were saved, so the defaults apply (`Services/OpenIdServerService.cs:68-84`).
3. `scene.ScrollAsync(0, 450)`, so the `Flows` section is visible, then `scene.HighlightAsync(` the checkboxes
   `Allow Authorization Code Flow` and `Allow Refresh Token Flow` `)` (`OpenIdServerSettings.Edit.cshtml:113` and
   `:153`). Do not scroll to `Advanced Options` (see `OPEN-QUESTIONS.md`).

Visible when the narration ends: the `Flows` section, with `Allow Authorization Code Flow` and `Allow Refresh Token
Flow` checked and ringed; the other flows unchecked.

### `openid-3`

1. `scene.NavigateAsync("/Admin/OpenId/Application")` (`ApplicationController.cs:63`; menu path **Access Control** >
   **OpenID Connect** > **Applications**, `ManagementAdminMenu.cs:22`). The page title is `Applications`, and the list
   says `Nothing here! There are no applications at the moment.`
2. `scene.ClickAndWaitForNavigationAsync(a.create)`: the button `New application` (`Views/Application/Index.cshtml:26`).
   The page title is `Create a new application`.
3. `scene.TypeAsync(#DisplayName, "Sample Front End")` (label `Display Name`, `Views/Application/Create.cshtml:26`).
4. `scene.SelectAsync([data-openid-field='Type'], "public")`: the option `Public client` (`Create.cshtml:38`).
   The `Client Secret` field collapses.
5. `scene.TypeAsync([data-openid-field='ClientId'], "frontend")` (label `Client Id`, `Create.cshtml:45`). Do not use
   the shuffle button, which writes a random value.

Visible when the narration ends: `Display Name` = `Sample Front End`, `Type` = `Public client`, `Client Id` =
`frontend`, and no `Client Secret` field.

### `openid-4`

1. `scene.ClickAsync(#AllowAuthorizationCodeFlow)` (label `Allow Authorization Code Flow`, `Create.cshtml:79`). Wait
   until `#RedirectSection` is visible (it expands with an animation).
2. `scene.TypeAsync(#RedirectUris, "http://127.0.0.1:5020/callback.html")` (label `Redirect Uris`, `Create.cshtml:143`).
3. `scene.ClickAsync(#RequireProofKeyForCodeExchange)` (label `Require Proof Key for Code Exchange`,
   `Create.cshtml:179`). Leave `Consent type` on its first option, `Explicit consent`.
4. `scene.ScrollAsync(0, 500)` so `Allowed Scopes` is visible (`Create.cshtml:221`); click the labels `profile` and
   `roles` inside `#ScopeGroup`. These scopes exist in a new site (`Migrations/DefaultScopesMigration.cs`).
5. `scene.ClickAndWaitForNavigationAsync(button.save)` (`Save`). The list opens.

Visible when the narration ends: the `Applications` list with one row, `Sample Front End`. (The ticked boxes were
visible just before the save.)

### `openid-5`

1. `scene.NavigateAsync("/Admin/OpenId/ValidationConfiguration")` (`ValidationConfigurationController.cs:49`; menu path
   **Settings** > **OpenID Connect** > **Token Validation**, `ValidationAdminMenu.cs:20`). The page title is `Configure
   OpenID Connect validation settings`.
2. `scene.HighlightAsync(` the input under the label `Authorization server tenant` `)`
   (`Views/OpenIdValidationSettings.Edit.cshtml:15`). Its value is `Default`, the current tenant, because the settings
   were never saved and the server is enabled on this tenant (`Services/OpenIdValidationService.cs:58-65`). Do not
   save. The `Authority`, `Audience` and token type fields stay hidden while a tenant is set.

Visible when the narration ends: the field `Authorization server tenant` = `Default`, ringed.

## 6. Slide steps

| Step | What the slide shows |
| --- | --- |
| `idea-1` | Heading "Content in Orchard Core, front end anywhere": three cards, `Orchard Core`, `Your front end`, `The browser`. |
| `idea-2` | Heading "Three questions, three modules": three cards pairing each question with GraphQL, CORS and OpenID Connect. |
| `request-1` | Heading "A query over HTTP": the request line and `Content-Type` header (bright, `Authorization` dimmed) and the JSON body with the query; cards `POST or GET`, `One response`, `Content type`. |
| `request-2` | The same panels with the `Authorization: Bearer <access token>` line bright and the body dimmed; cards `Access token`, `No token`, `Permissions`. |
| `cors-6` | Heading "Test a preflight request": the `curl -i -X OPTIONS` command with the `Origin`, `Access-Control-Request-Method` and `Access-Control-Request-Headers` headers; cards `Preflight` and `Response`. |
| `cors-7` | Heading "What CORS is, and is not": cards `Enforced by browsers`, `Not authentication`, `Narrow policies`. |
| `flow-1` | Heading "Authorization code flow with PKCE": four cards (`Sign in`, `Code`, `Token`, `Call`), the first two bright. |
| `flow-2` | The same four cards with `Token` and `Call` bright. |
| `recipe-1` | Heading "Features and CORS in a recipe": the `feature` step enabling the five features, and the `settings` step with the `Frontend` policy. |
| `recipe-2` | Heading "OpenID Connect in a recipe": the `OpenIdValidationSettings` step with cards `Tenant` and `Order`, and the `OpenIdApplication` step of the public client. |

## 7. Checks for private data

- Only fake names appear: the site `Orchard Core`, the user `admin`, the posts `Summer Menu Preview` and `A Morning at
  the Roastery`, the application `Sample Front End`, the client id `frontend`, and the addresses `127.0.0.1`,
  `localhost`, `app.example.com` and `cms.example.com`. No email address is displayed in these clips; if one appears
  (for example in the user menu), it is `admin@example.com`.
- The password `<password>` is never typed or displayed: the sign-in comes from `state.json`. The OpenID application is a
  public client, so no client secret exists or appears.
- Contact sheets: look for the red status of `cors-1` and the notices of each step, and for any error message.
- After assembling, OCR the video at one frame per second and at every scene change, and search for `localhost`
  ports other than `5010` and `5020`, for the machine's user name and for folder paths.
- Stop the front end server and the site by their process IDs. Delete the stand-in folder and the copied `App_Data`
  folders; never commit them.

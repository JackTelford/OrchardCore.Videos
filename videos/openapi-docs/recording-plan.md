# Recording plan: openapi-docs

A person who reads only this file can record the video. In this file a selector such as `#search-box` stands for
`scene.Page.Locator("#search-box")`, and `file:line` points into an `OrchardCMS/OrchardCore` checkout (paths start at
`src/`). The video has four recordings (`02-enable`, `04-swagger`, `05-explorers`, `06-schema`), eleven slides, an
opening card and a closing card. The password of the recording user is never written here: it is chosen outside git
and passed in `VIDEO_PASSWORD`, written `<password>` below.

## The site it needs

| What | Value |
|---|---|
| Code | `src/OrchardCore.Cms.Web` of an Orchard Core checkout. The OpenApi module is part of it through `src/OrchardCore/OrchardCore.Application.Cms.Core.Targets/OrchardCore.Application.Cms.Core.Targets.csproj:98`. |
| Address | `http://localhost:5010` (`VIDEO_BASE_URL`). Plain HTTP is enough: the OpenID Connect server turns its transport security requirement off (`src/OrchardCore.Modules/OrchardCore.OpenId/Configuration/OpenIdServerConfiguration.cs:226`). Do not use HTTPS, and do not trust any certificate on the machine. |
| Data | `ORCHARD_APP_DATA` points at an empty folder outside both repositories. |
| Setup | Auto Setup through environment variables, so no setup form is recorded and no sign-in form is recorded: tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>`, database provider `Sqlite`, recipe `Blog`. The keys are `OrchardCore__AutoSetup__Tenants__0__ShellName`, `__SiteName`, `__SiteTimeZone`, `__AdminUsername`, `__AdminEmail`, `__AdminPassword`, `__DatabaseProvider`, `__RecipeName` (`src/docs/reference/modules/AutoSetup/README.md`). |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` point at a dead address. Nothing in this video calls an outside service. |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` saves `state.json` (never committed). |
| Mask script | None: the site is made from scratch, so it holds no real name. |
| Console | Set `MSYS_NO_PATHCONV=1` in Git Bash, or paths such as `/Admin/Features` become Windows paths. |

Features before recording: the Blog recipe enables `OrchardCore.Contents`, `OrchardCore.Media` and
`OrchardCore.Queries` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-80`), and none of the four
OpenApi features or the OpenID Connect features. Leave them that way: `enable-2` enables the OpenApi features on
camera. The setting "Allow anonymous access to the API schema" is off (the default).

External services: none.

### Seed data (outside the capture)

1. The id of the Blog content item, for `swagger-3`. The Blog recipe adds a "Blog" entry to the admin menu that links
   to the content item (`blog.recipe.json:1188-1200`, `LinkText` "Blog" at line 1197). Sign in, select **Blog** in the admin menu, and copy the id from
   the address: `/Admin/Contents/ContentItems/<blog-content-item-id>/Display`. Keep it in a note named
   `<blog-content-item-id>`; it is different on every site. It is a generated id and holds no personal data.
2. Provisioning for "Try it out", described under "Provisioning" below. It runs after `02-enable`, not before.

## Order, snapshots and what changes the site

Record in this order. The order the clips play in is the same, with the provisioning between the first and the second
recording.

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-enable` | Snapshot S0: fresh site, signed in, no OpenApi feature | Yes: enables `OrchardCore.OpenApi`, `OrchardCore.OpenApi.ReDocUI`, `OrchardCore.OpenApi.ScalarUI` and `OrchardCore.OpenApi.SwaggerUI` |
| - | Provisioning (not recorded) | The site after `02-enable` | Yes: OpenID Connect features, one OpenID application |
| 2 | `04-swagger` | Snapshot S1: after provisioning | No (Try it out only reads) |
| 3 | `05-explorers` | Snapshot S1 | No |
| 4 | `06-schema` | Snapshot S1 | Yes: saves "Allow anonymous access to the API schema" and restarts the tenant |

- Stop the site and copy its `ORCHARD_APP_DATA` folder and `state.json` aside as **S0** before `02-enable`, and as
  **S1** after the provisioning and its rehearsal.
- To record `02-enable` again, restore S0. To record `04-swagger`, `05-explorers` or `06-schema` again, restore S1. Start
  the site again after each restore.
- `04-swagger`, `05-explorers` and `06-schema` do not depend on each other, but all of them depend on S1.
- The slide steps need no recording.

## Provisioning (outside the capture, after `02-enable`)

"Try it out" needs a bearer token, which Swagger UI acquires silently from the OpenID Connect server of the same
tenant. This is the Orchard Core recipe `OpenApiPkce`, with the redirect address changed to the address of the
recording site. All labels below are in the Orchard Core source.

1. Tools, Recipes (`src/OrchardCore.Modules/OrchardCore.Recipes/AdminMenu.cs:18-19`; address `/Admin/Recipes`). Type
   `OpenAPI Documentation` in the **Search** box (`#search-box`, `Views/Admin/Index.cshtml:15`). Select **Run** on
   "OpenAPI Documentation — Bearer/PKCE" (`Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:14`), and answer **Yes** in the
   confirmation "Are you sure you want to run the recipe?". The notification reads "The recipe 'OpenAPI Documentation
   — Bearer/PKCE' has been run successfully." (`Controllers/AdminController.cs:142`). The recipe is
   `src/OrchardCore.Modules/OrchardCore.OpenApi/Recipes/openapi-pkce.recipe.json`: it enables the OpenApi and the OpenID
   Connect features, requires PKCE, validates tokens for the `Default` tenant, and registers the public client `openapi`.
2. Access Control, OpenID Connect, Applications (`OrchardCore.OpenId/ManagementAdminMenu.cs:18-22`; address
   `/Admin/OpenId/Application`). Select **Edit** on "OpenAPI Interactive UI". Set **Redirect Uris** to
   `http://localhost:5010/OrchardCore.OpenApi/openapi-oidc-silent.html` and **Post-Logout Redirect Uris** to
   `http://localhost:5010/` (`Views/Application/Edit.cshtml:162-190`), then select **Save**. The recipe registers
   `https://localhost:5001/...`, which does not match a site on port 5010; the path must stay exactly as it is.
3. Rehearse in a normal browser tab, signed in as `admin`: open `/swagger`, expand `GET /api/content/{contentItemId}`,
   select **Try it out**, type `<blog-content-item-id>`, and execute. The response status must be 200. If it is 401, check
   that the OpenID Connect Token Validation feature is enabled and that the redirect address matches the address bar
   exactly; the browser console shows `[openapi-ui]` messages (`OrchardCore.OpenApi/Assets/openapi-ui-auth/src/openapi-ui-auth.ts`).
4. Stop the site, copy the `ORCHARD_APP_DATA` folder aside as snapshot S1, and start the site again.

## Clips

Every clip is recorded with the viewport, theme and cursor of the skill's recorder. Keep each step's actions shorter
than its narration.

### `02-enable` (chapter 02, Enable the features)

Starts on `/Admin/Features` (opened with `scene.OpenAsync`, not recorded). This is Tools, Features
(`OrchardCore.Features/AdminMenu.cs:26-27`). Expand no menu.

- `enable-1`: click `#search-box` and type `OpenApi` (`scene.TypeAsync`). Wait until `#list-summary` reads "Showing 4
  of" and a number. On screen when the narration ends: the search box with `OpenApi`, one category heading **Api**
  (`Views/Admin/Features.cshtml:88`), and four rows in this order, each with its name in bold, its id in code style, its
  description and a **Disabled** badge: **OpenApi** `OrchardCore.OpenApi`, **OpenApi ReDoc UI**
  `OrchardCore.OpenApi.ReDocUI`, **OpenApi Scalar UI** `OrchardCore.OpenApi.ScalarUI`, **OpenApi Swagger UI**
  `OrchardCore.OpenApi.SwaggerUI` (`Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`, descriptions from
  `OrchardCore.OpenApi/Manifest.cs:10-48`). The search matches the name, id and description
  (`Drivers/FeatureEntryDisplayDriver.cs:20`), so only these four rows match.
- `enable-2`: click the three checkboxes, in this order: `[id='OrchardCore.OpenApi.ReDocUI']`,
  `[id='OrchardCore.OpenApi.ScalarUI']`, `[id='OrchardCore.OpenApi.SwaggerUI']`
  (`Views/FeatureEntry.Checkbox.SummaryAdmin.cshtml:11`; the checkbox of `OrchardCore.OpenApi` stays empty). Click
  `#bulk-action-menu-button` (**Bulk Actions**, `Features.cshtml:33`), then click `a.dropdown-item[data-action='Enable']`
  (**Enable**, `Features.cshtml:38`) with `scene.ClickAndWaitForNavigationAsync`. The page posts and reloads; enabling
  restarts the tenant, so the reload can take a few seconds. If the step runs longer than its narration it only adds a
  pause. Do not tick **OpenApi** itself: the three explorers depend on it and it is enabled with them
  (`Controllers/AdminController.cs:178` passes `force`; the form posts `Force=true`, `Features.cshtml:49`; with `force`, the dependencies of a feature are enabled with it, `src/OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:163-180`). On screen when the narration
  ends: the three ticked checkboxes, or the reloaded page if the reload has finished.
- `enable-3`: wait until the notification "The following features were enabled: ..." is visible
  (`scene.Page.GetByText("The following features were enabled")`, `AdminController.cs:317`; it lists the three ticked
  features, and not **OpenApi**). Type `OpenApi` into `#search-box` again, because the reload clears it. On screen when
  the narration ends: the **Api** heading and four rows, each with an **Enabled** badge
  (`Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`) and a red **Disable** button instead of **Enable**
  (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53`).

### `04-swagger` (chapter 04, Swagger UI)

Starts on `/swagger` (opened with `scene.OpenAsync`, not recorded). Before `StartCaptureAsync`, wait until the page
shows the operations and until Swagger UI's **Authorize** button is in its locked state
(`button.authorization__btn.locked`): the silent sign-in has then finished
(`openapi-ui-auth.ts:233-249`). If the button never reaches that state within 15 seconds (it is a Swagger UI behavior, see
OPEN-QUESTIONS.md), wait 5 seconds after the operations are drawn instead, and rehearse again to confirm that "Try it out" returns 200. The page title is "OrchardCore OpenAPI Documentation"
(`OrchardCore.OpenApi/Startup.cs:257`), the document title "OpenApi V1" (`Startup.cs:34`). The Swagger UI markup is from
the Swashbuckle library, so check these selectors with `Recorder probe /swagger "() => document.title"` first.

- `swagger-1`: no click. `scene.PauseAsync(1500)`, then `scene.HighlightAsync` on the first tag heading
  (`.opblock-tag`) without a caption, and `scene.ClearHighlightsAsync()` at the end. On screen when the narration ends:
  Swagger UI with "OpenApi V1" at the top and the operation list, grouped under tag headings, each operation with a
  method badge and a route. On a Blog site expect at least the tags `CreateEndpoint`, `DeleteEndpoint`, `GetEndpoint`
  (the Contents endpoints, `Startup.cs:214` of Contents), `GetIntellisenseEndpoint` (the Liquid script endpoint,
  `OrchardCore.Liquid/Startup.cs:34`; Contents depends on Liquid), `MediaApi` (`.WithTags("MediaApi")` on the Media
  endpoints) and `QueryApi` (`QueryApiController`). The names come from the classes of the generated client,
  `OrchardCore.OpenApi/Services/OpenApiClient.cs`; the document may list more, so read the real tags once with the
  rehearsal and see OPEN-QUESTIONS.md.
- `swagger-2`: click the summary bar of the `GET /api/content/{contentItemId}` operation. The locator is the operation
  block `div.opblock-get` that contains `span.opblock-summary-path[data-path='/api/content/{contentItemId}']`; click its
  `.opblock-summary-control`. Source of the route: `OrchardCore.Contents/Endpoints/Api/GetEndpoint.cs:16`. On screen
  when the narration ends: the operation expanded, with a **Parameters** section (`contentItemId`), the **Try it out**
  button, and the **Responses** section.
- `swagger-3`: click **Try it out** (`button.try-out__btn` inside that block). Click the text input of the
  `contentItemId` parameter (the first `input[type='text']` in the block's parameters table) and type
  `<blog-content-item-id>` with `scene.TypeAsync`. Click **Execute** (`button.execute`). Wait until
  `.live-responses-table` shows the status `200` (`.live-responses-table .response-col_status`). On screen when the
  narration ends: the "Server response" with code 200 and a JSON body of the "Blog" content item (it starts with a
  `ContentItemId`, `ContentType` and `DisplayText`; the handler returns `Results.Json(contentItem)`,
  `GetEndpoint.cs:49`). Use `scene.ScrollAsync(0, 300)` if the response is below the fold. Try only this content operation: the Media operations use the admin cookie by default (`OrchardCore.Media/MediaApiSettings.cs`), and the explorers send a bearer token and omit cookies on `/api/` requests (`openapi-ui-auth.ts:205`), so a Media request is not a reliable demo.

### `05-explorers` (chapter 05, ReDoc and Scalar)

Starts on `/redoc` (opened with `scene.OpenAsync`, not recorded). Wait until the left menu and the first operation are
visible.

- `redoc-1`: `scene.PauseAsync(1200)`, then `scene.ScrollAsync(0, 500)` once, slowly, in the center panel. On screen when
  the narration ends: ReDoc's three-column page with the operation list in the menu on the left, the description of an
  operation in the middle, and no "Try it out" button anywhere. The page title is "OrchardCore OpenAPI Documentation"
  (`Startup.cs:302`).
- `scalar-1`: `scene.NavigateAsync("/scalar/v1")`. Wait until the text "OpenApi V1" is visible
  (`scene.Page.GetByText("OpenApi V1").First`) and the operation list is drawn, then `scene.PauseAsync(1500)`. On screen
  when the narration ends: Scalar's page with its operation list and the API reference. Do not send a request. The route
  comes from `ScalarUIStartup` (`Startup.cs:322-338`).

### `06-schema` (chapter 06, Schema access)

Starts on `/Admin/Settings/openapi` (opened with `scene.OpenAsync`, not recorded; Settings, OpenApi,
`OrchardCore.OpenApi/AdminMenu.cs:26-31`, route `OrchardCore.Settings/Controllers/AdminController.cs:50`).

- `schema-1`: no click. `scene.HighlightAsync` on the "API Documentation UIs" heading, then clear it. On screen when the
  narration ends: the warning "The website might be restarted upon saving the settings, potentially leading to temporary
  unresponsiveness during the process." (`OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`); the heading
  **API Documentation UIs**; three rows with a green **Enabled** badge: **Swagger UI** with "Interactive API explorer at
  ~/swagger", **ReDoc UI** with "Read-only API documentation at ~/redoc", **Scalar UI** with "Modern API reference at
  ~/scalar/v1"; the hint "Enable or disable API documentation UIs via the Features page."; and, lower, the heading
  **API Schema Access** with an unchecked "Allow anonymous access to the API schema"
  (`OrchardCore.OpenApi/Views/OpenApiSettings.Edit.cshtml:4-51`).
- `schema-2`: click the checkbox labeled **Allow anonymous access to the API schema**
  (`scene.Page.GetByLabel("Allow anonymous access to the API schema")`), then `scene.HighlightAsync` on its hint, "When
  enabled, the JSON schema endpoints can be accessed without authentication...", and clear it. On screen when the
  narration ends: the checkbox ticked.
- `schema-3`: click the **Save** button (`button.save`, `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`)
  with `scene.ClickAndWaitForNavigationAsync`. Saving restarts the tenant, so wait until the notification "Site settings
  updated successfully." is visible (`OrchardCore.Settings/Controllers/AdminController.cs:119`). On screen when the
  narration ends: the notification, and the checkbox still ticked.

## Slides and cards

No recording. `slides.py` draws them; this is what each one shows.

- `intro` (opening card): the logo, "The Orchard Core OpenApi module" and the subtitle.
- `why-1`: three cards, "Your endpoints", "An OpenAPI document", "Other tools".
- `why-2`: four cards, one per feature; the three explorers are bright.
- `why-3`: three cards with the address of each explorer.
- `access-1`: three cards: not signed in, signed in without the permission, the `ViewOpenApiContent` permission.
- `access-2`: three cards: bearer tokens only, Swagger UI and Scalar, ReDoc.
- `access-3`: the trimmed recipe, with the features and the client bright.
- `access-4`: the same recipe, with the `RedirectUris` line bright.
- `access-5`: three cards: in memory only, same origin only, renewed silently.
- `own-1`: the controller of the documentation, with its three attributes bright, and three notes under it (the last one: the explorers attach the token to routes that contain `/api/`, `openapi-ui-auth.ts:146`).
- `own-2`: the same controller, with the `ProducesResponseType` line bright.
- `own-3`: `GetEndpoint.cs` and the generated C# `OpenApiClient.cs` method it produces (`OrchardCore.OpenApi.nswag:109` names it from the operation id).
- `outro` (closing card): the logo, the title and "docs.orchardcore.net".

## Checks for private data

- The only user is `admin` with `admin@example.com`; no other account exists. No real name, email, phone number or
  token appears: the content is the Blog recipe's sample content.
- The content item JSON in `swagger-3` and the admin menu show no real name. Check the response body in the contact
  sheet.
- The address bar is never recorded (the recorder captures the page only), and the address is `localhost`.
- No bearer token, client secret or password is visible. The OpenID application is public, so it has no secret.
  Swagger UI's **Authorize** dialog is never opened.
- The password is never typed on camera and never written in git.
- After assembling, OCR the video as `SKILL.md` describes and search the text for `@`, `Bearer ` and long random strings.

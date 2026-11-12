# Recording plan: Content API: Read and Write Content Items Over HTTP

Audience: developer. Narration: 769 words, 5:08. Steps marked "slide" are drawn by `slides.py` and need no
recording. Only two clips are recordings of the browser: `03-role` (the permissions of a role) and `07-admin` (the
content items the API calls left in the admin). The HTTP calls themselves are shown on slides; they are made once,
from a shell, to put the site in the state `07-admin` shows and to check that what the slides say is true.

In this plan a selector such as `#search-box` stands for `scene.Page.Locator("#search-box")`. A file reference
is `file:line` in the Orchard Core source, relative to `src/`, with the folder written out (for example
`OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:19`); `Views/...`, `Controllers/...` and similar paths in a bullet
belong to the module named in the same bullet.

## 1. The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore`, `src/OrchardCore.Cms.Web`, on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with its own `App_Data` outside both repositories (`ORCHARD_APP_DATA`). The labels and behavior
  of this plan were read from commit 92db4a83.
- **Auto Setup** (no setup or sign-in form is recorded): recipe **Blog**, tenant `Default`, site name `Orchard Core`,
  time zone `America/Los_Angeles`, user name `admin`, email `admin@example.com`, password `<password>` (chosen outside
  git, passed to the recorder as `VIDEO_PASSWORD`; `VIDEO_USER_NAME=admin`).
- `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address **on the site process only**, so the site never calls a mail,
  SMS or payment provider. The shell that makes the API calls must not use that proxy: pass `--noproxy '*'` to curl.
- **No external service** is needed. The OpenID Connect server that issues the token is the site itself (tenant
  `Default`).
- **Content type**: `Article` of the Blog recipe (parts `TitlePart`, `AutoroutePart`, `HtmlBodyPart` and the part
  `Article` with the text field `Subtitle`; draftable and versionable). Nothing is changed in it.
  Source: `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:131-186` (type), `:677-704` (part).

### Features to enable (Tools > Features)

Menu path: **Tools**, **Features** (`OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`). Search for `OpenID`, then enable:

1. **OpenID Connect Authorization Server** (`OrchardCore.Modules/OrchardCore.OpenId/Manifest.cs:44`). Its dependencies, **OpenID Connect
   Core Services** and **OpenID Connect Management UI**, are enabled with it.
2. **OpenID Connect Token Validation** (`OrchardCore.Modules/OrchardCore.OpenId/Manifest.cs:68`).

Wait until the site answers again after each change (the tenant restarts).

### Settings

1. **Settings**, **OpenID Connect**, **Authorization Server** (`OrchardCore.Modules/OrchardCore.OpenId/ServerAdminMenu.cs:18-22`), page
   "Configure OpenID Connect server settings". Under Endpoints, **Enable Token Endpoint** is checked (it enables the
   endpoint `/connect/token`; checked by default, because the settings of a site that has never saved them enable
   it: `Services/OpenIdServerService.cs:68-83`, label at `Views/OpenIdServerSettings.Edit.cshtml:25`). Under Flows, check **Allow Client
   Credentials Flow** (`:163`; it shows once the token endpoint is enabled; **Allow Authorization Code Flow** and **Allow
   Refresh Token Flow** are already checked by default: `Services/OpenIdServerService.cs:74-75`; leave them). Click **Save**. Without
   this setting the server does not offer the client credentials grant, whatever the application allows
   (`Configuration/OpenIdServerConfiguration.cs:136-139`).
2. **Settings**, **OpenID Connect**, **Token Validation** (`ValidationAdminMenu.cs:20`), page "Configure OpenID
   Connect validation settings". **Authorization server tenant** shows `Default` (the default when the authorization
   server feature is enabled on the same tenant: `Services/OpenIdValidationService.cs:51-72`). Click **Save** once, so
   the tenant restarts with the validation in place.

### Role

**Access Control**, **Roles** (`OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:19-21`), **Add Role** (`Views/Admin/Index.cshtml:31`):

- **Name**: `ApiClient`
- **Description**: `Calls the Content API`
- Click **Save & Configure** (`Views/Admin/Create.cshtml:32`). On the page "Edit 'ApiClient' Role", check **Allow**
  for exactly these three permissions (use **Search permissions** to find them), then click **Save**:
  - `Access content via the api` (`OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:45`)
  - `Publish or unpublish content for others` (`CommonPermissions.cs:17`)
  - `Delete content for others` (`CommonPermissions.cs:25`)

The role must be saved with these permissions **before** the token is requested: the token carries the permissions of
the application's roles at the time it is issued (`OrchardCore.Modules/OrchardCore.OpenId/Controllers/AccessController.cs:493-512`).

### Application

**Access Control**, **OpenID Connect**, **Applications** (`OrchardCore.Modules/OrchardCore.OpenId/ManagementAdminMenu.cs:18-22`), **New application**
(`Views/Application/Index.cshtml:26`), page "Create a new application":

- **Display Name**: `Content API client`
- **Type**: `Confidential client` (`Views/Application/Create.cshtml:37`)
- **Client Id**: `content-api-client`
- **Client Secret**: a value chosen outside git, kept in the shell variable `CLIENT_SECRET`. It never appears in a
  recorded screen and is never written in the repository.
- Under Flows, check **Allow Client Credentials Flow** (`:123`). The box is enabled once **Type** is **Confidential client**,
  and it is hidden unless the server settings above allow the flow (`Views/Application/Create.cshtml:19`).
- Under **Client Credentials Roles** (`:235`), check `ApiClient`. No scope is needed: the name of the tenant is always a
  valid audience (`AccessController.cs:676-692`).
- Click **Save**.

## 2. Seed: the API calls, made outside the capture

Run these from Git Bash, in this order, with the site running, all in the same shell. `BASE`, `CLIENT_SECRET`,
`TOKEN`, `A`, `B` and `C` are shell variables; nothing in this section is committed with a real value. A token lives for a
limited time: if a call answers `401` after a pause, run step 0 again.

```bash
export BASE=http://localhost:5010
# CLIENT_SECRET: chosen outside git, exported in this shell

# Prints the ContentItemId of the JSON that arrives on standard input.
item_id() { python -c "import json, sys; print(json.load(sys.stdin)['ContentItemId'])"; }

# 0. The access token (slide auth-3 shows this request)
export TOKEN=$(curl -s --noproxy '*' "$BASE/connect/token" \
  -d grant_type=client_credentials -d client_id=content-api-client \
  --data-urlencode "client_secret=$CLIENT_SECRET" \
  | python -c "import json, sys; print(json.load(sys.stdin)['access_token'])")

# 1. Create "Planting notes", published (slides create-1 and create-2). Its ContentItemId is A.
A=$(curl -s --noproxy '*' -X POST "$BASE/api/content" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"ContentType":"Article","TitlePart":{"Title":"Planting notes"},"HtmlBodyPart":{"Html":"<p>Seedlings go out in May.</p>"}}' | item_id)

# 2. Create "Greenhouse log", as a draft (slide create-3). Its ContentItemId is B.
B=$(curl -s --noproxy '*' -X POST "$BASE/api/content?draft=true" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"ContentType":"Article","TitlePart":{"Title":"Greenhouse log"}}' | item_id)

# 3. Read A (slide read-1)
curl -s --noproxy '*' "$BASE/api/content/$A" -H "Authorization: Bearer $TOKEN"

# 4. Update A and publish (slide update-1)
curl -s --noproxy '*' -X POST "$BASE/api/content" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d "{\"ContentItemId\":\"$A\",\"TitlePart\":{\"Title\":\"Spring planting notes\"}}"

# 5. Update A as a draft (slides update-2 and update-3)
curl -s --noproxy '*' -X POST "$BASE/api/content?draft=true" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d "{\"ContentItemId\":\"$A\",\"Article\":{\"Subtitle\":{\"Text\":\"Revised in the greenhouse\"}}}"

# 6. Create "Old notes" (published). Its ContentItemId is C. Then delete it (slide delete-1)
C=$(curl -s --noproxy '*' -X POST "$BASE/api/content" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"ContentType":"Article","TitlePart":{"Title":"Old notes"}}' | item_id)
curl -s --noproxy '*' -X DELETE "$BASE/api/content/$C" -H "Authorization: Bearer $TOKEN"
```

The identifiers on the slides (`4kd8y3m1q6x7vz2p5n9w0c4tga` and `5vq2n8t4c1m7x3w9z6k0d2hrja`) are invented and stand for
A and B. The identifiers of the site differ. No recorded screen shows an identifier.

### Checks of what the slides say

Make each call and compare. A difference is a finding: write it in `OPEN-QUESTIONS.md` before recording.

| Call | Expected | Slide or step |
| --- | --- | --- |
| Step 1 | `200`, JSON with `Published` true, `Latest` true, `DisplayText` "Planting notes" | `create-2` |
| Step 2 | `200`, JSON with `Published` false, `Latest` true | `create-3` |
| `GET` of B | `404` (a draft has no published version) | `read-1`, `delete-2` |
| `DELETE` of B | `404` | `delete-2` |
| Step 5 | `200`, JSON with `Published` false, `Latest` true, the subtitle text | `update-3` |
| `GET` of A after step 5 | `200`, the published version (title "Spring planting notes", no subtitle text) | `update-2` |
| `GET` of C after step 6 | `404` | `delete-1` |
| `GET` of A with no `Authorization` header | `401`, JSON body with `title` "Authentication required" | `status-3` |
| `GET` of A with a token whose role lacks *Access content via the api* (make this check on a copy of `App_Data`, never on the snapshot of section 3: create a role with no permission and an application for it) | `403`, JSON body with `title` "Access forbidden" | `status-3` |
| Step 1 with the property `contentType` in place of `ContentType` | `400` | `api-3` |
| `POST` with `"ContentType":"Nothing"` | `400` | `status-1` |

State after the seed, in **Content**, **Content Items**: "Spring planting notes" (`Article`, badges Published and
Draft) first, "Greenhouse log" (`Article`, badge Draft) second, then the items of the Blog recipe. "Old notes" is not
listed.

## 3. Snapshot and order of the clips

- Stop the site after the seed and copy its `App_Data` aside, with `state.json` (sign in once with `Recorder setup`).
  Restore both before a new take, and start the site again.
- **No clip changes the site**: `03-role` and `07-admin` only browse and type in search boxes (never click **Save**). They do not depend on
  each other and can be recorded in any order, both after the seed. `07-admin` needs the state above: record it
  before any other call changes the items.
- Narrate first (`narrate.py`), then record: the recorder holds each step for the length of its narration.

## 4. Clips and steps

### Chapter 00: `00-intro` (card)

- `intro`: the opening card. No recording.

### Chapter 01: `01-overview` (slides)

- `api-1`: slide "Three endpoints": a code panel with the three routes, and three cards (GET reads the published
  version, POST creates or updates, DELETE removes).
- `api-2`: slide "One item per call": two cards, the Content API and the GraphQL API.
- `api-3`: slide "A content item as JSON": the JSON of an Article, and three cards (case sensitive names, one
  property per part, set by the server).

### Chapter 02: `02-token` (slides)

- `auth-1`: slide "Before the first call": four numbered cards (enable two features, allow the flow in the server
  settings, add an application, grant permissions to the role); the first two are bright.
- `auth-2`: slide "Before the first call": the same four cards; the last two are bright.
- `auth-3`: slide "Get a token, send a token": the token request and the Authorization header.

### Chapter 03: `03-role` (recording) and `03-matrix` (slide)

Clip `03-role`. Starts on **Access Control**, **Roles**, `/Admin/Roles/Index` (the route is `Roles/{action}/{id?}`:
`OrchardCore.Modules/OrchardCore.Roles/Controllers/AdminController.cs:22`), before the capture: the list of roles with
`ApiClient` in it. Source of the labels: `OrchardCore.Modules/OrchardCore.Roles/Views/Admin/Index.cshtml`.

- `perm-1`: the actions, in order:
  1. Click and wait for navigation: `a[href$="/Roles/Edit/ApiClient"]` (use `.First`: the role name and the Edit
     button share the link target; `Views/RoleEntry.Fields.SummaryAdmin.cshtml:7`). The page is titled
     "Edit 'ApiClient' Role" (`Views/Admin/Edit.cshtml:6`).
  2. Wait until `#search-box` is visible, then type `via the api` into it (`TypeAsync`, so the search runs on each key).
  3. Wait until `tr[data-text="Access content via the api"]` is visible and `tr[data-text="Clone content"]` is hidden.
  4. Highlight `tr[data-text="Access content via the api"]` with the caption `Access content via the api`.

  When the narration ends: the page "Edit 'ApiClient' Role", the search box holding `via the api`, one row visible under
  the legend "Contents" with the hint "(OrchardCore.Contents)", the row "Access content via the api" with a checked
  box in the Allow column and in the Effective column, and the green ring on that row.
  Labels: "Search permissions" (`Views/Admin/_RolePermissions.cshtml:49`), the columns "Permission", "Allow" and
  "Effective" (`:68-70`), the group legend and its source hint (`:59-62`), the permission (`CommonPermissions.cs:45`).
- `perm-2`: the actions, in order:
  1. `ClearHighlightsAsync()`.
  2. Click `#search-box`, press `Escape` (it clears the search and shows every row again:
     `OrchardCore.Modules/OrchardCore.Roles/Assets/ts/roles-edit.ts:14-17`), then type `content for others`.
  3. Wait until `tr[data-text="Publish or unpublish content for others"]`,
     `tr[data-text="Edit content for others"]` and `tr[data-text="Delete content for others"]` are visible.
  4. Highlight the Publish row with the caption `Allow`, hold about 1.5 seconds, clear; highlight the Edit row with
     the caption `Effective`, hold, clear; highlight the Delete row with the caption `Allow`.

  When the narration ends: the search box holding `content for others`, the three rows, and the ring on "Delete content
  for others". On the contact sheet check that "Publish or unpublish content for others" is checked in both columns,
  "Edit content for others" is unchecked in Allow and checked in Effective, and "Delete content for others" is checked
  in both. If rows of other modules also match the search, they stay visible and are not highlighted.
  Labels: `OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:17`, `:21` and `:25`; why the Edit row is
  effective: its definition lists Publish as the permission that implies it (`CommonPermissions.cs:21`).

Clip `03-matrix` (slide):

- `perm-3`: slide "Permissions of each call": a banner "Every call: Access content via the api", then four rows (GET:
  View all content; POST to create: Edit content for others and Publish or unpublish content for others; POST to
  update: Edit content for others; DELETE: Delete content for others). Source: `OrchardCore.Modules/OrchardCore.Contents/Endpoints/Api/`
  `GetEndpoint.cs:32,44`, `CreateEndpoint.cs:46,68,73,111`, `DeleteEndpoint.cs:32,44`.

### Chapter 04: `04-create` (slides)

- `create-1`: slide "Create an item": the curl request that posts an Article with a title and a body.
- `create-2`: slide "The response is the stored item": the JSON answer, with `Published` true.
- `create-3`: slide "Save a draft instead": the request with `?draft=true` and the answer with `Published` false.

### Chapter 05: `05-read` (slides)

- `read-1`: slide "Read an item": the GET request, and the answer, noting 404 when there is no published version.

### Chapter 06: `06-update` (slides)

- `update-1`: slide "Update an item": the POST with an existing `ContentItemId` and a changed title.
- `update-2`: slide "Publish the changes, or keep a draft": the request with `?draft=true`, and two cards.
- `update-3`: slide "The response is the saved version": the JSON of the draft, `Latest` true and `Published` false.

### Chapter 07: `07-admin` (recording)

Clip `07-admin`. Starts on **Content**, **Content Items**, `/Admin/Contents/ContentItems`, before the capture; wait
until the row "Greenhouse log" is visible. The Content menu is expanded because the page belongs to it. Labels:
`OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65` and `:68`, the page title "Manage Content" (`Views/Admin/List.cshtml:17`).

- `admin-1`: highlight the Draft badge of the row "Greenhouse log": `scene.Page.Locator("li:has(a:text-is('Greenhouse log')), tr:has(a:text-is('Greenhouse log'))").First.Locator(".badge", new() { HasText = "Draft" })`, with the caption `Draft`.

  When the narration ends: the list with "Spring planting notes" on top and "Greenhouse log" below it, each with the
  type badge `Article`, and a green ring on the badge **Draft** of "Greenhouse log". The row shows no
  **Published** badge. Source of the badges: `Views/ContentsTags_SummaryAdmin.cshtml:12` (Published) and `:19`
  (Draft).
- `admin-2`: `ClearHighlightsAsync()`, then highlight both badges of the row "Spring planting notes" (the badge with
  the text `Published` and the badge with the text `Draft`), with the captions `Published` and `Draft`.

  When the narration ends: the row "Spring planting notes" shows the badges **Published** and **Draft**, both ringed.

### Chapter 08: `08-delete` (slides)

- `delete-1`: slide "Delete an item": the DELETE request and its answer.
- `delete-2`: slide "Which items a delete finds": three cards (published item: 200; item with only a draft: 404;
  unknown ID: 404).

### Chapter 09: `09-status` (slides)

- `status-1`: slide "Status codes": five rows (200, 400, 401, 403, 404); the first four are bright.
- `status-2`: slide "Status codes": the rows 400 and 404 are bright.
- `status-3`: slide "Problem details bodies": the 401 body with the title "Authentication required", and the 403 body
  with the title "Access forbidden". Source: `OrchardCore/OrchardCore.Infrastructure/ErrorHandling/ApiProblemDetailsMiddleware.cs:78` and `:83`.

### Chapter 99: `99-outro` (card)

- `outro`: the closing card, subtitle `docs.orchardcore.net`. No recording.

## 5. Checks for private data

- The recorded pages are the role editor and the content list of a fresh site: no email address other than
  `admin@example.com`, no client secret, no access token. Search each contact sheet (`shots/03-role.jpg`,
  `shots/07-admin.jpg`) for `@`, `eyJ` and `secret`.
- The row "Greenhouse log" and the row "Spring planting notes" may show an author name (the display name of the
  application, `Content API client`, or no name). It must be a fake name. If a real name appears, the site was not
  made from scratch: start again from a fresh `App_Data`.
- The shell used for the seed is not recorded. Do not paste a token, a secret or a password into any file of the
  repository, and do not leave the commands of section 2 with real values in a script that is committed.
- OCR the assembled video at one frame per second and at every scene change before publishing, as the skill asks.

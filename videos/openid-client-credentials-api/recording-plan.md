# Recording plan: Call the Orchard Core API With Client Credentials

A developer video. Its chapters `01-flow`, `05-token`, `06-call` and `08-fail` are slides (drawn from `slides.py`);
its chapters `02-site`, `03-role`, `04-app` and `07-result` are recordings of the admin. The calls of the client
program are never recorded: they are code on slides. One of them is run once, outside the capture, to create the
article that `07-result` shows.

Orchard Core source for the `file:line` references: commit `92db4a83`. A path that starts with `OrchardCore.Contents.Core/`,
`OrchardCore.Infrastructure/`, `OrchardCore.Abstractions/` or `OrchardCore.ContentManagement.Abstractions/` is under
`src/OrchardCore/`; every other path is under `src/OrchardCore.Modules/` (the Blog recipe is under
`src/OrchardCore.Themes/TheBlogTheme/Recipes/`).

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of a checkout at (or after) commit `92db4a83`, on
  `http://localhost:5010` (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at an empty folder outside both
  repositories.
- **Auto Setup** (no setup or sign-in form is recorded; see the `orchardcore-tester` skill, `references/autosetup.md`,
  and `src/docs/reference/modules/AutoSetup/README.md` for the option names): `ShellName` = `Default`, `SiteName` =
  `Orchard Core`, `SiteTimeZone` = `America/Los_Angeles`, `AdminUsername` = `admin`, `AdminEmail` =
  `admin@example.com`, `AdminPassword` = `<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`),
  `DatabaseProvider` = `Sqlite`, `RecipeName` = `Blog`.
- **Block outbound calls**: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address in the shell that starts the site.
  The `curl` commands below use `--noproxy "*"` so they still reach `localhost`.
- **Tenant**: only `Default`. The video is recorded on the default tenant, so the token validation settings point at
  the tenant `Default`.
- **Time zone and language**: as above; the recorder uses `en-US`.
- **Features to enable before recording**: none. The Blog recipe enables `OrchardCore.Contents` and
  `OrchardCore.Roles`, which the video needs. No `OrchardCore.OpenId.*` feature may be enabled at the start: clip
  `02-site` enables them on camera.
- **Settings to save before recording**: none.
- **External services**: none, so no local stand-in.
- **`PrepareAsync`**: the video has none (nothing is seeded outside the capture except what "Create the article"
  below describes).
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` once, outside the capture,
  which writes `state.json` (git-ignored). The session survives the tenant restarts of the clips.
- **Fake seed data** (every value below is invented; type exactly these):

  | Where | Value |
  | --- | --- |
  | Role name | `Content API Client` |
  | Application Display Name | `Content Sync Job` |
  | Application Client Id | `content-sync` |
  | Application Client Secret | `<client-secret>`: a random value of at least 24 characters, chosen outside git, passed to the recorder as `VIDEO_CLIENT_SECRET` and to the commands below as `CLIENT_SECRET` (the same value) |
  | Article created by the client | title `Hello from the sync job`, content type `Article` |

## Order of the clips, snapshots and what changes the site

Take these snapshots (a copy of the site's `App_Data` folder and of `state.json`, with the site stopped):

| Snapshot | Taken | Holds |
| --- | --- | --- |
| `S0` | After Auto Setup and `Recorder setup` | A fresh Blog site with no OpenID Connect feature enabled |
| `S2` | After recording `02-site` | OpenID Connect Authorization Server (with its Management UI and Core Services) and Token Validation enabled; the server settings saved with Allow Client Credentials Flow |
| `S3` | After recording `03-role` | S2 plus the role `Content API Client` with two permissions |
| `S4` | After recording `04-app` | S3 plus the application `content-sync` |
| `S5` | After "Create the article" below | S4 plus the article `Hello from the sync job`, published |

Record in this order, each clip starting from the snapshot before it:

1. `02-site`, from `S0`. It changes the site (features, server settings) and restarts the tenant three times.
2. `03-role`, from `S2`. It changes the site (a role).
3. `04-app`, from `S3`. It needs the server settings of `S2` (the Allow Client Credentials Flow check box is only
   listed when the server allows it) and the role of `S3`. It changes the site (an application).
4. "Create the article" (below), from `S4`, outside the capture.
5. `07-result`, from `S5`. It does not change the site (it opens the article and leaves without saving), so it can be
   recorded again at will.

To record a clip again, restore the snapshot before it and record it; the clips after it are then recorded again from
that point, because their snapshots no longer apply.

### Create the article (outside the capture, from `S4`)

Run these once the site is running with `S4`. `CLIENT_SECRET` is the `<client-secret>` typed in `04-app`.

```bash
BASE=http://localhost:5010
TOKEN=$(curl -s --noproxy "*" -X POST "$BASE/connect/token" \
  -d grant_type=client_credentials -d client_id=content-sync -d client_secret="$CLIENT_SECRET" \
  | python -c "import sys, json; print(json.load(sys.stdin)['access_token'])")

curl -s --noproxy "*" -o /dev/null -w "%{http_code}\n" -X POST "$BASE/api/content" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"ContentType":"Article","TitlePart":{"Title":"Hello from the sync job"}}'
```

The second command must print `200`. Do not print or save the token. If the first command fails, or the second prints
another code (`401`, `403`, `400`), the site is not in the state `S4` describes: stop, and see `OPEN-QUESTIONS.md`.
Then stop the site and take `S5`.

## Clip `02-site`: Prepare the site

**Starts on** `/Admin/Features` (opened with `OpenAsync` before the capture). Wait until `#search-box` is visible. The
admin menu is not opened. Features page: `OrchardCore.Features/Views/Admin/Features.cshtml`.

The Enable links are `a.btn.btn-primary` with the text `Enable` in the row of the feature; their id is
`btn-enable-` plus the feature id with dots replaced by underscores
(`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`), for example
`#btn-enable-OrchardCore_OpenId_Server`. Enabling a feature enables the features it depends on and restarts the
tenant, so the click can take several seconds: wait for the page after the click inside
`scene.OffCameraAsync`, with the condition that the row of the feature shows a `Disable` button.

- `site-1`:
  1. `TypeAsync(#search-box, "OpenID", 60)`. Wait until the row with the text `OpenID Connect Authorization Server`
     is visible (the list filters on the page, with no reload).
  2. `HighlightAsync` the name of that row, hold about 1 second, `ClearHighlightsAsync`.
  3. `ClickAndWaitForNavigationAsync` the `Enable` link of that row (`#btn-enable-OrchardCore_OpenId_Server`). Off
     camera, wait until the Features page has loaded again with the green message `The following features were
     enabled: ...` (or `The feature ... was enabled.`).
  4. `TypeAsync(#search-box, "OpenID", 60)` again, since the page reload emptied the box.
  - **On screen when the narration ends**: the Features page filtered on `OpenID`, with the row `OpenID Connect
    Authorization Server` showing a red `Disable` button, and the success message at the top.
  - **Labels**: `Tools`, `Features` (`OrchardCore.Features/AdminMenu.cs:26-27`); feature name `OpenID Connect
    Authorization Server` (`OrchardCore.OpenId/Manifest.cs`, the feature `OrchardCore.OpenId.Server`); message
    (`OrchardCore.Features/Controllers/AdminController.cs:317`).
- `site-2`:
  1. The search box still holds `OpenID`. Wait until the row `OpenID Connect Token Validation` is visible.
  2. `HighlightAsync` its name, hold about 1 second, `ClearHighlightsAsync`.
  3. `ClickAndWaitForNavigationAsync` its `Enable` link (`#btn-enable-OrchardCore_OpenId_Validation`). Off camera,
     wait until the page has loaded again with the message `The feature OpenID Connect Token Validation was
     enabled.`
  4. `TypeAsync(#search-box, "OpenID", 60)` again.
  - **On screen when the narration ends**: the filtered list with the row `OpenID Connect Token Validation` showing
    `Disable`, and the success message.
  - **Labels**: feature name `OpenID Connect Token Validation` (`OrchardCore.OpenId/Manifest.cs`, the feature
    `OrchardCore.OpenId.Validation`).
- `site-3`:
  1. `NavigateAsync("/Admin/OpenId/ServerConfiguration")` (`OrchardCore.OpenId/Controllers/ServerConfigurationController.cs:49`).
     Wait until the title `Configure OpenID Connect server settings` and the check box `Enable Token Endpoint` are
     visible.
  2. `HighlightAsync` the check box `Enable Token Endpoint` (`[data-openid-field="EnableTokenEndpoint"]`, checked by
     default), hold about 1.5 seconds, `ClearHighlightsAsync`.
  3. `ScrollAsync(0, 500)` until the heading `Flows` is on screen. Wait until `[data-openid-field="AllowClientCredentialsFlow"]`
     is visible (it is shown by the page's script a moment after the load), then
     `ClickAsync([data-openid-field="AllowClientCredentialsFlow"])`: it becomes checked.
  4. `ClickAndWaitForNavigationAsync` the `Save` button (`.action-bar-bottom button.btn-primary`). Off camera, wait
     until the page has loaded again with the message `OpenID server configuration successfully updated.`
     (the site restarts).
  5. `ScrollAsync(0, 500)` so that the `Flows` section is on screen again.
  - **On screen when the narration ends**: the `Flows` section with `Allow Client Credentials Flow` checked (next to
    `Allow Authorization Code Flow` and `Allow Refresh Token Flow`, checked by default), and, above, the success
    message if the scroll still shows it.
  - **Labels**: `Settings`, `OpenID Connect`, `Authorization Server` (`OrchardCore.OpenId/ServerAdminMenu.cs:19-22`);
    `Endpoints`, `Enable Token Endpoint` (`Views/OpenIdServerSettings.Edit.cshtml:17,25`); `Flows`, `Allow Client
    Credentials Flow` (`:105,163`); `Save` (`Views/ServerConfiguration/Index.cshtml`); message
    (`Controllers/ServerConfigurationController.cs:96`). Defaults when the settings were never saved: the token
    endpoint is on, the authorization code and refresh token flows are on, the client credentials flow is off
    (`Services/OpenIdServerService.cs:68-83`).
- `site-4`:
  1. `NavigateAsync("/Admin/OpenId/ValidationConfiguration")` (`Controllers/ValidationConfigurationController.cs:49`).
     Wait until the title `Configure OpenID Connect validation settings` is visible.
  2. `HighlightAsync` the field `Authorization server tenant` (`.openid-validation-tenant`) with the caption
     `This site`. It holds `Default`; the `Authority` and `Audience` fields are hidden. Do not save: nothing is
     changed on this page.
  - **On screen when the narration ends**: the validation settings page with the highlighted field `Authorization
    server tenant` holding `Default`.
  - **Labels**: `Token Validation` (`OrchardCore.OpenId/ValidationAdminMenu.cs:20`); `Authorization server tenant`
    (`Views/OpenIdValidationSettings.Edit.cshtml:15`). Why it holds `Default`: the validation settings, when never
    saved, use the tenant of the site if the server feature is enabled (`Services/OpenIdValidationService.cs:58-66`).

## Clip `03-role`: Create a role

**Starts on** `/Admin/Roles` (opened with `OpenAsync` before the capture): the roles `Administrator`, `Anonymous`,
`Authenticated`, `Author`, `Contributor`, `Editor` and `Moderator`.

- `role-1`:
  1. `ClickAndWaitForNavigationAsync` the button `Add Role` (`a.create`). Wait for the title `Create Role`.
  2. `TypeAsync(#RoleName, "Content API Client", 60)`. Leave `Description` empty.
  3. `ClickAndWaitForNavigationAsync` the button `Save & Configure` (`button.save-configure`). Wait for the title
     `Edit 'Content API Client' Role` and for `#search-box` to be visible.
  - **On screen when the narration ends**: the role edit page, titled `Edit 'Content API Client' Role`, with the
    `Description` box, the box `Search permissions` and the first permission tables.
  - **Labels**: `Access Control`, `Roles` (`OrchardCore.Roles/AdminMenu.cs:18-19`); `Add Role`
    (`Views/Admin/Index.cshtml:31`); `Create Role`, `Name`, `Save & Configure` (`Views/Admin/Create.cshtml:6,15,32`);
    `Edit '{0}' Role` (`Views/Admin/Edit.cshtml:6`).
- `role-2`:
  1. `TypeAsync(#search-box, "api", 80)`. Wait until the row `Access content via the api` is the visible row of its
     table.
  2. `ClickAsync(input[name="Checkbox.AccessContentApi"])`: the `Allow` check box of that row becomes checked.
  3. Select the text of the search box (`scene.Page.Keyboard.PressAsync("Control+A")` with the box focused) and
     `TypeAsync(#search-box, "publish", 80)`. Wait until the row `Publish or unpublish content for others`
     (`tr[data-text="Publish or unpublish content for others"]`) is visible.
  4. `ClickAsync(input[name="Checkbox.PublishContent"])`: its `Allow` check box becomes checked.
  - **On screen when the narration ends**: the permissions filtered on `publish`, with the `Allow` check box of
    `Publish or unpublish content for others` checked.
  - **Labels**: `Search permissions` (`Views/Admin/_RolePermissions.cshtml:49`); `Allow` (`:69`); `Access content via
    the api` (`OrchardCore.Contents.Core/CommonPermissions.cs:45`); `Publish or unpublish content for others`
    (`OrchardCore.Contents.Core/CommonPermissions.cs:17`).
- `role-3`:
  1. `ClickAndWaitForNavigationAsync` the button `Save` (`.action-bar-bottom button.save`). Wait for the roles list.
  - **On screen when the narration ends**: the roles list with the new role `Content API Client` and the green message
    `Role updated successfully.`
  - **Labels**: `Save` (`Views/Admin/Edit.cshtml`); message (`Controllers/AdminController.cs:213`).
  - The implied permissions named in the narration (`Edit content for others`, `View all content`) come from
    `CommonPermissions.cs:17,21,29` (`EditContent` is implied by `PublishContent`, `ViewContent` by `EditContent`);
    they are not shown on screen.

## Clip `04-app`: Register the application

**Starts on** `/Admin/OpenId/Application` (opened with `OpenAsync` before the capture): the message `Nothing here!
There are no applications at the moment.`

- `app-1`:
  1. `ClickAndWaitForNavigationAsync` the button `New application` (`a.create`). Wait for the title `Create a new
     application` and for `#AllowClientCredentialsFlowFieldSet` to be shown (the page's script shows the flows the
     server allows, a moment after the load).
  2. `HighlightAsync` `#DisplayName`, `#Type` and `[data-openid-field="ClientId"]` (three rings, no caption), hold about
     1.5 seconds, `ClearHighlightsAsync`.
  - **On screen when the narration ends**: the form `Create a new application` with the fields `Display Name`, `Type`
    (showing `Confidential client`) and `Client Id`.
  - **Labels**: `Access Control`, `OpenID Connect`, `Applications` (`OrchardCore.OpenId/ManagementAdminMenu.cs:18-22`);
    `New application` (`Views/Application/Index.cshtml:26`); `Create a new application`, `Display Name`, `Type`,
    `Client Id` (`Views/Application/Create.cshtml:10,26,34,45`).
- `app-2`:
  1. `TypeAsync(#DisplayName, "Content Sync Job", 60)`.
  2. `TypeAsync([data-openid-field="ClientId"], "content-sync", 60)`.
  3. `HighlightAsync(#Type)` so that `Confidential client` is readable; no change to the drop-down list.
  4. Wait until `[data-openid-field="ClientSecret"]` is visible, then `TypeAsync([data-openid-field="ClientSecret"],
     <the value of VIDEO_CLIENT_SECRET>, 30)`. The field is a password field, so the capture shows dots. Never click
     the eye button (`#toggleClientSecret`) and never the copy buttons.
  - **On screen when the narration ends**: `Display Name` = `Content Sync Job`, `Client Id` = `content-sync`, `Type` =
    `Confidential client` highlighted, `Client Secret` filled with dots (no readable value).
  - **Labels**: `Confidential client` (`Views/Application/Create.cshtml:37`); `Client Secret` (`:57`). The secret is
    required for a confidential application (`Controllers/ApplicationController.cs:181-182`).
- `app-3`:
  1. `ClearHighlightsAsync`, then `ScrollAsync(0, 450)` until the heading `Flows` and the check boxes under it are on
     screen. Only the flows the server settings allow are listed (`Allow Authorization Code Flow`, `Allow Client
     Credentials Flow`, `Allow Refresh Token Flow`).
  2. `ClickAsync(#AllowClientCredentialsFlow)`: it becomes checked, and the section `Client Credentials Roles` opens
     below. Wait for `#RoleGroup` to be shown. The same click also opens the hint `Recommended Parameters:` under the
     check box (`#AllowClientCredentialsFlowRecommendedHint`, `Views/Application/Create.cshtml:122,126`); it lists
     `grant_type`, `client_id`, `client_secret` and `scope`. It holds no secret; leave it open.
  3. `ScrollAsync(0, 300)` if needed, so that the heading `Client Credentials Roles` and the role names are on screen.
  - **On screen when the narration ends**: `Allow Client Credentials Flow` checked, and the `Client Credentials Roles`
    list under it, with `Content API Client` among the role names, unchecked.
  - **Labels**: `Flows` (`Views/Application/Create.cshtml:71`); `Allow Client Credentials Flow` (`:123`); `Client
    Credentials Roles` (`:235`). The list of flows follows the server settings, and the roles section follows the
    check box: `OrchardCore.OpenId/Assets/ts/application-create.ts` and its compiled
    `wwwroot/Scripts/openid-application-editor/application-create.js`.
- `app-4`:
  1. `ClickAsync` the label `Content API Client` inside `#RoleGroup`: its check box becomes checked.
  2. `ClickAndWaitForNavigationAsync` the button `Save` (`.action-bar-bottom button.save`). Wait for the applications
     list.
  - **On screen when the narration ends**: the applications list with `Content Sync Job` (buttons `Edit` and `Delete`).
  - **Labels**: `Save` (`Views/Application/Create.cshtml`); the list (`Views/Application/Index.cshtml`,
    `Views/OpenIdApplicationEntry.Buttons.SummaryAdmin.cshtml`).

## Clip `07-result`: See the result

**Starts on** `/Admin/Contents/ContentItems` (opened with `OpenAsync` before the capture), on site `S5`. The most
recently modified items come first, so the article is at the top.

- `result-1`:
  1. Wait until the row `Hello from the sync job` is visible at the top of the list (type `Article`, badge `Published`).
  2. `HighlightAsync` the title link of that row, hold, `ClearHighlightsAsync`. Leave the list as it is.
  - **On screen when the narration ends**: the list `Manage Content` with the first row `Hello from the sync job`,
    its type `Article` and the badge `Published`.
  - **Labels**: `Content`, `Content Items` (`OrchardCore.Contents/AdminMenu.cs:65,68`); the page title `Manage Content`
    (`Views/Admin/List.cshtml`); `Published` (`Views/ContentsTags_SummaryAdmin.cshtml:12`).
- `result-2`:
  1. `ClickAndWaitForNavigationAsync` the title link `Hello from the sync job`. Wait for the edit page of the article.
  2. `HighlightAsync(scene.Page.GetByLabel("Title", exact))`, the field `Title`, which holds `Hello from the sync job`.
     Do not edit, do not save.
  - **On screen when the narration ends**: the edit page of the article with the `Title` field highlighted and holding
    `Hello from the sync job`.
  - **Labels**: `Title` (`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`). The title comes from the `TitlePart`
    that the client sent (`OrchardCore.Title/Handlers/TitlePartHandler.cs`).

## Slides

No recording; each step is one slide, drawn by `slides.py`.

- `flow-1`: three cards: runs unattended, signs in as itself, proves it with a secret.
- `flow-2`: the four cards of the flow, the first three bright: ask for a token, receive an access token, call the API.
- `flow-3`: the same four cards, the second and the fourth bright (the token, and the roles that set the access).
- `token-1`: the C# token request, with the request lines bright.
- `token-2`: the same code, with the secret and the response lines bright.
- `call-1`: the C# code that sets the bearer header and posts a content item.
- `call-2`: the JSON body on the left; three notes on the right (`ContentType, ContentItemId` are case sensitive, an
  existing content item id updates, `?draft=true` saves a draft).
- `call-3`: the C# code that reads the item back, then sends the delete and prints its status. The role of this video
  lacks `Delete content for others`, so the delete answers `403 Forbidden` and the article stays (the next clips need it).
- `call-4`: four cards: the permissions that read, create, update and delete need.
- `fail-1`: three cards about the answers of the API; `401 Unauthorized` and `403 Forbidden` bright.
- `fail-2`: the same cards; `404 Not Found, 400 Bad Request` bright.
- `fail-3`: two cards; `Role changes do not reach issued tokens` bright.
- `fail-4`: the same cards; `Token request refused` bright.

The opening card (`intro`) and the closing card (`outro`) are drawn by the assembler from the storyboard.

## Checks for private data

Before the clips are assembled, look at every contact sheet and OCR the frames:

- The `Client Secret` field shows dots only, at every frame of `app-2`, `app-3` and `app-4`. The secret, the access
  token and the password `<password>` appear nowhere: not in a field, a URL, a notification or a tooltip.
- Every name is one of the fake values above, or comes from the Blog recipe (`admin`, `About`, the blog posts). No
  email other than `admin@example.com`; no host other than `localhost:5010` in the address bar of the recording
  (the frame does not show it).
- The `Authority` of the server settings is empty (the site generates it), and no certificate of the machine is shown:
  the `Advanced Options` of the server settings page, with its certificate lists, are never on screen in `site-3`.
- `state.json`, the `App_Data` copies and the value of `VIDEO_CLIENT_SECRET` stay outside git.

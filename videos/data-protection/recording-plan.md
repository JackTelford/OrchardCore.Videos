# Recording plan: Data Protection Keys

A developer video. One chapter is recorded in a browser (`03-enable`, the Features page of the admin); every other
chapter is a set of slides drawn by `slides.py`. Nothing else is recorded: terminal commands, files and log lines are
slides. In this plan a selector stands for `scene.Page.Locator(...)`.

| Chapter | Clip | Kind | Steps |
|---|---|---|---|
| 00 | `00-intro` | card | `intro` |
| 01 | `01-why` | slides | `why-1`, `why-2`, `why-3`, `why-4` |
| 02 | `02-places` | slides | `places-1`, `places-2` |
| 03 | `03-enable` | recording | `enable-1`, `enable-2`, `enable-3` |
| 04 | `04-azure` | slides | `azure-1` to `azure-5` |
| 05 | `05-redis` | slides | `redis-1` to `redis-4` |
| 06 | `06-check` | slides | `check-1`, `check-2` |
| 99 | `99-outro` | card | `outro` |

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore`. The CMS target package
  references both modules of the video, `OrchardCore.DataProtection.Azure` and `OrchardCore.Redis`
  (`src/OrchardCore/OrchardCore.Application.Cms.Core.Targets/OrchardCore.Application.Cms.Core.Targets.csproj:58` and `:106`),
  so the Features page lists their features.
- **Run it** on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both
  repositories, and `HTTP_PROXY` / `HTTPS_PROXY` set to a dead address so the site calls nothing.
- **Auto Setup** (no setup form and no sign-in form are recorded), through environment variables
  (names from `docs/reference/modules/AutoSetup/README.md`):

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword='<password>'   # chosen outside git, same as VIDEO_PASSWORD
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

  The Blog recipe enables `OrchardCore.Features`, which the clip needs
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:38`, in the `feature` step). One tenant, `Default`.
- **Features to enable before recording**: none. At the start of every take, `OrchardCore.DataProtection.Azure`,
  `OrchardCore.Redis` and `OrchardCore.Redis.DataProtection` are all **disabled**; the clip enables the first one.
- **Settings: none, and this matters.** The site must have **no** Azure Blob Storage connection string and **no**
  Redis configuration, in any source: `appsettings*.json`, `App_Data/appsettings*.json`, user secrets, and the
  environment variables `OrchardCore__DataProtection__Azure__ConnectionString`,
  `OrchardCore__OrchardCore_DataProtection_Azure__ConnectionString`, `OrchardCore__Redis__Configuration` and
  `OrchardCore__OrchardCore_Redis__Configuration`. Reason: with a connection string, enabling the feature moves the
  tenant to an empty key ring, the session saved in `state.json` was protected with the old keys, and the page
  after the click is expected to be the sign-in page instead of the Features page. Without settings the feature
  registers nothing (`src/OrchardCore.Modules/OrchardCore.DataProtection.Azure/Startup.cs:34` and `:69`), which is also
  what the narration of `enable-3` says.
- **External services**: none for the capture. Azurite and Redis are **not** started while recording. Azurite in
  Docker is used only for the optional check of the facts below, on a separate copy of the site.
- **Seed data**: none. The site is a fresh Blog site; no content item, user or tenant is created, and the Features page
  shows no personal data.
- **Sign in once, outside the capture**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`
  saves `state.json` (git-ignored). Then stop the site and copy `App_Data` and `state.json` aside (the snapshot).

## Order, dependencies and snapshots

There is one clip, so there is no order to follow. `03-enable` **changes the site**: it enables
`OrchardCore.DataProtection.Azure`. Before a new take, stop the site (by its process ID), restore the copy of
`App_Data` and `state.json`, and start the site again. Record on the restored site, never on the site of the previous
take.

## Clip `03-enable` (Features)

**Starts on** `/Admin/Features`, opened with `scene.OpenAsync` before `StartCaptureAsync`, signed in through
`state.json`. Check before the capture: the page title is "Features" (`Views/Admin/Features.cshtml:15`), the list is in
its default layout (List, with Enable and Disable as buttons: `OrchardCore.Admin.Abstractions/AdminListOptions.cs:30` and `:36`), the visibility
filter is on its defaults ("Hide on demand" and "Hide always enabled" selected, "Any status"), the search box is empty,
and the page is scrolled to the top. Do not click in the admin menu: the narration of `enable-1` names the path
"Tools, Features" (`OrchardCore.Features/AdminMenu.cs:26` and `:27`) while the page is already open.

Selectors used below (confirm each with `Recorder probe` on the first take):

- search box: `#search-box` (placeholder "Search", `Views/Admin/Features.cshtml:27`)
- Azure row: `[data-filter-value*="OrchardCore.DataProtection.Azure"]`
- Redis row: `[data-filter-value*="OrchardCore.Redis.DataProtection"]`
- identifier of a row: `<row> code` (`Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`)
- dependency badge of the Redis row: `<row> .direct-dependencies .badge` (`Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:13`)
- Enable button of the Azure row: `#btn-enable-OrchardCore_DataProtection_Azure` (the view builds the id with
  `Html.GenerateIdFromName`, which replaces each dot by an underscore in ASP.NET Core by default), or, if the id
  differs, `a[href*="Features/OrchardCore.DataProtection.Azure/Enable"]` (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`)

### `enable-1`

- Actions: `TypeAsync` into `#search-box` the text `data protection`. The list filters while typing.
- On screen when the narration ends: the summary "Showing 2 of N features" (`Features.cshtml:59`), the category
  heading "Security" (`Features.cshtml:88`; the category comes from `OrchardCore.DataProtection.Azure/Manifest.cs:9` and
  `OrchardCore.Redis/Manifest.cs:46`), and exactly two rows: "Distributed Data Protection (Azure Storage)"
  (`OrchardCore.DataProtection.Azure/Manifest.cs:4`) and "Distributed Data Protection (Redis)"
  (`OrchardCore.Redis/Manifest.cs:43`), each with its identifier, and both marked "Disabled".
- If a third row shows, stop: the search matches the name, the identifier and the description
  (`Drivers/FeatureEntryDisplayDriver.cs:20`), so a feature that mentions data protection in its description would show.

### `enable-2`

- Actions: `HighlightAsync` on the identifier (`code`) of the Azure row, with the caption "Identifier"; `PauseAsync` of
  about 2 seconds; `ClearHighlightsAsync`; then `HighlightAsync` on the dependency badge of the Redis row, with the
  caption "Depends on Redis". Keep the actions shorter than the narration.
- On screen when the narration ends: both rows still visible, a green ring around the "Redis" badge of the Redis row.
  The label "Redis" comes from `OrchardCore.Redis/Manifest.cs:12` (the name of the feature it depends on).

### `enable-3`

- Actions: `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` on the Enable button of the Azure row, then
  wait until the text "The feature Distributed Data Protection (Azure Storage) was enabled." is visible
  (`OrchardCore.Features/Controllers/AdminController.cs:317`). The button is labeled "Enable".
- On screen when the narration ends: the Features page again (the redirect goes back to the list, with an empty search
  box and every feature listed) and the success message at the top, reading "The feature Distributed Data Protection
  (Azure Storage) was enabled." The page must be the Features page, not the sign-in page: if it is the sign-in page,
  the site has a connection string somewhere (see "Settings: none"). Fix the site and record again.

## Slide steps

| Step | What the slide shows |
|---|---|
| `why-1` | Four cards: authentication cookies, anti-forgery tokens, saved credentials, temporary data. |
| `why-2` | The default registration of the key ring (trimmed code) and three cards: per tenant, on disk, no setup. |
| `why-3` | Four cards, the first two bright: several servers, restarts. |
| `why-4` | The same four cards, the last two bright: stored data, backups. |
| `places-1` | Three cards: local folder, Azure Blob Storage, Redis, with the names of the two features. |
| `places-2` | Three cards: no feature, a feature enabled and configured, a feature enabled and not configured. |
| `azure-1` | The Azure settings (`OrchardCore`, `DataProtection`, `Azure`) with a placeholder connection string. |
| `azure-2` | Four cards: ConnectionString, ContainerName, BlobName, CreateContainer, with their defaults. |
| `azure-3` | Three cards (container, folder, blob) and the blob paths of two tenants, Default and Marketing. |
| `azure-4` | The settings with Liquid templates for the container and blob names, and three cards. |
| `azure-5` | The trimmed startup code of the Azure feature and three cards: runs after, removes, persists. |
| `redis-1` | The Redis settings with a placeholder connection string, and three cards for the three settings. |
| `redis-2` | The pattern of the Redis key, the key of the Default tenant with the prefix MyApp, and two cards. |
| `redis-3` | Four cards: durable storage, AOF or RDB persistence, a warning in the log, AllowAdmin. |
| `redis-4` | The trimmed code of the Redis startup and of the key management setup, side by side. |
| `check-1` | Two cards with the log entries: the Azure critical entry and the Redis error entry. |
| `check-2` | Two cards with where to look: the blob path in Azure and the key in Redis. |

## Optional run: check the facts with Azurite (not recorded)

This run is not part of the capture. It confirms the questions of `OPEN-QUESTIONS.md` and needs a **copy** of the
site (never the snapshot used for recording):

```bash
docker run --rm -p 10000:10000 mcr.microsoft.com/azure-storage/azurite azurite-blob --blobHost 0.0.0.0
export OrchardCore__DataProtection__Azure__ConnectionString='UseDevelopmentStorage=true'
# start the copy of the site with OrchardCore.DataProtection.Azure enabled, sign in, then list the container:
az storage blob list --connection-string 'UseDevelopmentStorage=true' -c dataprotection -o table
```

Expected: the container `dataprotection` and the blob `Sites/Default/DataProtectionKeys.xml`. Stop the container and
delete the copy of the site afterwards.

## Checks for private data

- The capture shows the Features page only: feature names, identifiers and descriptions. The signed-in user is `admin`,
  with `admin@example.com` if the email shows anywhere. OCR the video at one frame per second and at every scene
  change, and search for any name, email address or token other than these.
- The slides hold only invented values: the tenants `Default` and `Marketing`, the prefix `MyApp`, and placeholders
  such as `<your-azure-storage-connection-string>` and `<your-redis-connection-string>`. No real account name, key or
  connection string appears anywhere.
- Do not commit `state.json`, a copy of `App_Data`, a connection string, the Azurite data folder, or any audio, image or MP4.

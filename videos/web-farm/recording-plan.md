# Recording plan: web-farm

A person records this video by following this file, without deciding anything. Labels and `file:line` references are
from `OrchardCMS/OrchardCore` at commit `92db4a83`; paths are relative to the root of that checkout. A selector written
in backticks, such as `#search-box`, stands for `scene.Page.Locator("#search-box")`. Check each selector once with
`Recorder probe` before recording, since a markup change may have moved it.

Seven things set this video apart from the others of the programme:

- It needs **two instances of the site** running at the same time, on ports 5010 (instance A) and 5011 (instance B),
  that share a database, an Azure Blob Storage container, a Redis server and the Data Protection keys.
- The site is **not** `OrchardCore.Cms.Web` as it is: it needs `AddAzureShellsConfiguration()`, so a throwaway host
  project is built (section 1.2).
- Only browser screens are recorded. Every command below runs in a terminal, off camera.
- The sign-in is the one of the Default tenant: user `admin`, email `admin@example.com`, password `<password>`, chosen
  outside git and passed in `VIDEO_PASSWORD`. The recording is made with Auto Setup, so no Setup screen is recorded.
- Nothing is typed on camera except the tenant name and URL prefix of `sync-3` and the search terms of the Features page.
- Cookies are shared by all ports of `localhost`, so one `state.json` signs the recorder in on both instances. That only
  works because both instances read the same Data Protection keys (section 1.5).
- Three clips change the site, and `03-enable` has to be recorded before instance B is started (section 2).

## 1. The site it needs

| | |
|---|---|
| Orchard Core | A checkout of `OrchardCMS/OrchardCore` at the commit above, built from source. |
| Host | `FarmHost`, a throwaway copy of `src/OrchardCore.Cms.Web` (section 1.2). Never added to the solution, never committed. |
| Tenant | `Default` only. Site name `Orchard Core`, time zone `America/Los_Angeles`, recipe `SaaS` (name "Software as a Service"), provisioned by Auto Setup. No Setup screen and no login form is recorded. |
| Database | PostgreSQL in Docker, database `farm`, shared by both instances. SQLite can't be used: its file is created under the `App_Data` folder of each instance (`src/OrchardCore/OrchardCore.Data.YesSql/SqliteHelper.cs:51`), so two instances would not share their data. The tracker note lists only Redis and Azurite; the database container is an addition (see `OPEN-QUESTIONS.md`). |
| Tenant settings | `tenants.json` in an Azure Blob Storage container on Azurite in Docker, through the Azure Shells Configuration provider. |
| Distributed cache | Redis in Docker, with append-only persistence. |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` point to a dead address, and `NO_PROXY` lists the local services, so the site never calls a real provider. |
| Fake data | The site holds none. The only record created on camera is the tenant `Northwind`, URL prefix `northwind` (`sync-3`). |
| Mask script | Not needed: the site is made from scratch and holds no real name. |

### 1.1 Containers

Use a folder `<farm>` outside both repositories for everything below. In Git Bash, set `MSYS_NO_PATHCONV=1`.

```bash
docker volume create farm-pg && docker volume create farm-azurite && docker volume create farm-redis
docker run -d --name farm-postgres -p 5432:5432 -e POSTGRES_USER=orchard -e POSTGRES_PASSWORD=<db-password> \
  -e POSTGRES_DB=farm -v farm-pg:/var/lib/postgresql/data postgres:17
docker run -d --name farm-azurite -p 10000:10000 -v farm-azurite:/data mcr.microsoft.com/azure-storage/azurite \
  azurite-blob --blobHost 0.0.0.0 --location /data --skipApiVersionCheck
docker run -d --name farm-redis -p 6379:6379 -v farm-redis:/data redis:7 redis-server --appendonly yes
az storage container create --name farm-shells --connection-string "UseDevelopmentStorage=true"
```

`<db-password>` is chosen outside git. The container `farm-shells` must exist before the first start: the provider
does not create it (`src/OrchardCore/OrchardCore.FileStorage.AzureBlob/BlobFileStore.cs:728` creates only Data Lake
directories). `UseDevelopmentStorage=true` is the public connection string of the emulator, not a credential.

### 1.2 The host project

1. Copy `src/OrchardCore.Cms.Web` to `src/FarmHost`, and rename `OrchardCore.Cms.Web.csproj` to `FarmHost.csproj`.
2. In `FarmHost.csproj`, add under the `ProjectReference` items:
   `<ProjectReference Include="..\OrchardCore\OrchardCore.Shells.Azure\OrchardCore.Shells.Azure.csproj" />`.
3. In `Program.cs`, add `.AddAzureShellsConfiguration()` after `.AddSetupFeatures("OrchardCore.AutoSetup")`:

   ```csharp
   var orchardCore = builder.Services
       .AddOrchardCms()
       .AddSetupFeatures("OrchardCore.AutoSetup")
       .AddAzureShellsConfiguration();
   ```

4. `dotnet build src/FarmHost -c Release`. Delete `src/FarmHost` when the video is finished.

### 1.3 The two instances

Both instances get the same environment, except for the port and `ORCHARD_APP_DATA`. Paste this block in each terminal,
then run the last two lines of the instance:

```bash
export ASPNETCORE_ENVIRONMENT=Development
export OrchardCore__Shells__Azure__ConnectionString="UseDevelopmentStorage=true"
export OrchardCore__Shells__Azure__ContainerName=farm-shells
export OrchardCore__Shells__Azure__UseHierarchicalNamespace=false
export OrchardCore__Redis__Configuration=localhost:6379
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="<password>"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Postgres
export OrchardCore__AutoSetup__Tenants__0__DatabaseConnectionString="Server=localhost;Port=5432;Database=farm;User Id=orchard;Password=<db-password>"
export OrchardCore__AutoSetup__Tenants__0__RecipeName=SaaS
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9 NO_PROXY=localhost,127.0.0.1
# instance A:
export ORCHARD_APP_DATA=<farm>/app-a
dotnet run --project src/FarmHost -c Release --no-launch-profile --urls http://localhost:5010
# instance B:
export ORCHARD_APP_DATA=<farm>/app-b
dotnet run --project src/FarmHost -c Release --no-launch-profile --urls http://localhost:5011
```

`<password>` is the value of `VIDEO_PASSWORD`. `UseHierarchicalNamespace=false` is set explicitly, so the
provider doesn't depend on its detection of the storage account type (`src/OrchardCore/OrchardCore.Shells.Azure/ConfigurationSchema.json`). Stop an instance by its
process ID, never by image name.

### 1.4 First start and sign-in

1. Start the containers, then **instance A only**. Open `http://localhost:5010/` once: Auto Setup runs the recipe on this
   first request and takes a few seconds. `az storage blob list --container-name farm-shells --connection-string
   "UseDevelopmentStorage=true" -o table` then lists `tenants.json`.
2. Sign in at `http://localhost:5010/Login` as `admin` with `<password>`, in a browser that is not recorded.

### 1.5 Seed (outside the capture)

On instance A, signed in, open **Tools, Features** (`/Admin/Features`) and enable, searching for each name:

1. **Background Tasks** (`OrchardCore.BackgroundTasks`): the Background Tasks page of `05-task` needs it.
2. **Distributed Data Protection (Redis)** (`OrchardCore.Redis.DataProtection`, category Security): it also enables
   **Redis**. Both instances then read their Data Protection keys from Redis, so the session of A is valid on B.

Do **not** enable Redis Cache, Distributed Tenants, Indexing Worker, Indexing, Contents or Redis Lock: the clips enable
them. Filter by **Enabled only** to check.

Enabling the second feature invalidates the session (the keys moved to Redis). Sign in again at `/Login`, and only then
create the recorder's sign-in, once:

```bash
export VIDEO_ROOT=<repo>/videos/web-farm VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password> VIDEO_BASE_URL=http://localhost:5010
<bin>/Recorder setup
```

`state.json` stays in the video folder, out of git. Check in Redis that the keys exist:
`docker exec farm-redis redis-cli keys "*DataProtection*"` lists `Default:DataProtection-Keys`.

There is no `PrepareAsync`: the seed above is all of it.

### 1.6 Snapshots

Stop the instances (by process ID), then the containers, then copy:

```bash
docker stop farm-postgres farm-azurite farm-redis
for v in farm-pg farm-azurite farm-redis; do
  docker run --rm -v $v:/v -v <farm>/snap-<name>:/s alpine tar czf /s/$v.tgz -C /v .
done
cp -r <farm>/app-a <farm>/snap-<name>/app-a     # and app-b, once it exists
```

To restore: stop everything, then for each volume
`docker run --rm -v $v:/v -v <farm>/snap-<name>:/s alpine sh -c "rm -rf /v/* /v/.[!.]* ; tar xzf /s/$v.tgz -C /v"`,
copy the `app-*` folders back (delete `app-b` for snapshot S0), and start the containers and the instances again.
Restoring puts back the Data Protection keys in Redis with the rest, so `state.json` stays valid.

## 2. Order of the clips

| Order | Clip | Instance | Changes the site | Needs |
|---|---|---|---|---|
| 0 | Take **snapshot S0** | A only | | The seed of 1.5 done, `state.json` created, B never started. |
| 1 | `03-enable` | A | Enables Redis Cache, Redis, Distributed Tenants | S0. B must **not** be running. |
| 2 | Start **instance B**, take **snapshot S1** | A and B | | B starts after `03-enable`, so it reads Distributed Tenants from the shared database. Open `http://localhost:5011/` and `/Admin/Tenants` once to build its Default tenant. |
| 3 | `04-sync` | A and B | Creates the tenant Northwind | S1. |
| 4 | `05-features` | A | Enables Indexing Worker (and Indexing, Contents), on both instances | S1. Does not need `04-sync`. |
| 5 | `05-task` | B | Nothing | `05-features` done, then the wait described in the clip. |

Why `03-enable` comes first and B second: the service that keeps instances in step only runs on an instance whose
Default tenant already has Distributed Tenants and a Redis cache (`src/OrchardCore/OrchardCore/Shell/Distributed/DistributedShellHostedService.cs`,
`GetDefaultShellDescriptorAsync`). An instance that was already running would not hear about the change (see
`OPEN-QUESTIONS.md`). To record a clip again, restore S1 (S0 for `03-enable`), do the clips before it again if it needs
them, and run the recorder with the clip's `VIDEO_BASE_URL`. Each clip is recorded with `Recorder record <clip>`.

## 3. The clips

Narration lengths are about 0.4 s per word. Keep each step's actions shorter than that.

### `03-enable`: Redis Cache and Distributed Tenants (instance A)

`VIDEO_BASE_URL=http://localhost:5010`. Starts on `/Admin/Features` (`scene.OpenAsync`), signed in as admin, search box
empty and focused, the page scrolled to the top. Redis Cache and Distributed Tenants show **Disabled**.

Enabling a feature releases the Default tenant (`src/OrchardCore/OrchardCore.Infrastructure/Shell/ShellDescriptorManager.cs:135`),
so the page after each click can take several seconds. Do a warm-up take first. If a click still takes longer than the
narration of its step, record the clip again after a warm-up, or wrap the wait for the reloaded page in `scene.OffCameraAsync`.

| Step | Actions | Visible when the narration ends | Label source |
|---|---|---|---|
| `enable-1` (about 12 s) | `scene.TypeAsync` "Redis Cache" in `#search-box`. The search matches the name, the id and the description of a feature, so two rows stay: **Redis Cache** and **Distributed Tenants**, whose description names Redis Cache (`Manifest.cs:29` of `OrchardCore.Tenants`, `Views/FeatureEntry*` `data-filter-value` set in `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`). Wait until `#btn-enable-OrchardCore_Redis_Cache` is visible. `scene.ClickAndWaitForNavigationAsync` on it. Then `scene.TypeAsync` "Redis Cache" in `#search-box` again. | The row **Redis Cache**, id `OrchardCore.Redis.Cache`, with the badge **Enabled** and a **Disable** button, under the heading **Distributed**; below it the row **Distributed Tenants**, still **Disabled**. | "Tools", "Features": `OrchardCore.Features/AdminMenu.cs:26-27`. Search box: `OrchardCore.Features/Views/Admin/Features.cshtml:27`. Name and category: `OrchardCore.Redis/Manifest.cs:19,22`. Enable button and id: `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`. Badge: `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`. |
| `enable-2` (about 12 s) | Clear `#search-box`, then `scene.TypeAsync` "Distributed Tenants". Wait until `#btn-enable-OrchardCore_Tenants_Distributed` is visible (it is the only row left: no other description names Distributed Tenants). `scene.ClickAndWaitForNavigationAsync` on it. | The page reloaded after the click (the search box empty again), the Features list on screen. | Name: `OrchardCore.Tenants/Manifest.cs:28`. Description (read in the row): `Manifest.cs:29`. |
| `enable-3` (about 11 s) | Do not search: the search matches the name, the id and the description of the features, not their categories. `ScrollIntoViewIfNeededAsync` on the heading `h3` with the exact text **Distributed**, then `scene.HighlightAsync` on the `.feature-group` that contains it, no caption. | The group **Distributed** with **Redis**, **Redis Cache** and **Distributed Tenants** showing **Enabled**, and **Redis Bus** and **Redis Lock** showing **Disabled**. | Group heading: `OrchardCore.Features/Views/Admin/Features.cshtml:88`. |

### `04-sync`: a tenant created on one instance appears on the other (instances A and B)

`VIDEO_BASE_URL=http://localhost:5010`; instance B is running (section 2, order 2); neither instance has a tenant
other than Default. The clip records two tabs of the same browser context: the Scene API prepends one base URL, so the
second tab is created by hand and switched to with `SwitchToAsync`. Before `StartCaptureAsync`:

```csharp
await scene.OpenAsync("/Admin/Tenants");                       // instance A, the first tab
var first = scene.Page;
var second = await scene.Context.NewPageAsync();               // instance B, the second tab
second.SetDefaultTimeout(20_000);
await second.GotoAsync("http://localhost:5011/Admin/Tenants", new() { WaitUntil = WaitUntilState.NetworkIdle });
await first.BringToFrontAsync();
await scene.StartCaptureAsync();
```

Both tabs show the list with the Default tenant alone and the Add Tenant button. Add Tenant is `a.create`.

| Step | Actions | Visible when the narration ends | Label source |
|---|---|---|---|
| `sync-1` (about 6 s) | `scene.HighlightAsync(first.Locator("a.create"), "First instance")`. | The Tenants list of A with only the **Default** tenant row, and the green ring with the caption **First instance** around **Add Tenant**. | "Multi-Tenancy", "Tenants": `OrchardCore.Tenants/AdminMenu.cs:30,33`. "Add Tenant": `OrchardCore.Tenants/Views/Admin/Index.cshtml:27`. |
| `sync-2` (about 11 s) | `scene.ClearHighlightsAsync()`, `scene.SwitchToAsync(second)`, `scene.HighlightAsync(second.Locator("a.create"), "Second instance")`. | The Tenants list of B with only the **Default** tenant row, and the caption **Second instance**. | Same as `sync-1`. |
| `sync-3` (about 7 s) | `scene.ClearHighlightsAsync()`, `scene.SwitchToAsync(first)`, `scene.ClickAndWaitForNavigationAsync(first.Locator("a.create"))`, then `scene.TypeAsync(first.Locator("#Name"), "Northwind")` and `scene.TypeAsync(first.Locator("#RequestUrlPrefix"), "northwind")`. | The **Create Tenant** form of A with **Name** `Northwind` and **Url Prefix** `northwind`, the other fields empty. | "Name": `Views/Admin/Create.cshtml:15`. "Url Prefix": `Views/Shared/_TenantInfo.cshtml:12`. |
| `sync-4` (about 9 s) | `scene.ClickAndWaitForNavigationAsync(first.Locator("button[name='action'][value='create']"))`, then `scene.HighlightAsync` on the badge of the **Northwind** row (the `.ta-badge` with the text Uninitialized inside the `.list-group-item` that has the text Northwind), no caption. The Default row carries more badges, the database provider and the recipe name. | The list of A with the rows **Default** and **Northwind**; Northwind has the badge **Uninitialized** inside the ring. | "Create": `Views/Admin/Create.cshtml:29`. "Uninitialized": `Views/ShellSettingsEntry.State.SummaryAdmin.cshtml:12`. |
| `sync-5` (about 13 s) | `scene.ClearHighlightsAsync()`. Off camera, `scene.OffCameraAsync(...)`: reload `second` every second, up to 20 times, until a `.list-group-item` with the text Northwind is there. Then `scene.SwitchToAsync(second)` and `scene.WaitForNavigationAsync(() => second.ReloadAsync())`, then `scene.HighlightAsync` on the `.list-group-item` of Northwind, no caption. | The list of B with **Default** and **Northwind** (**Uninitialized**), the row inside the ring. | Same as `sync-4`. |
| `sync-6` (about 9 s) | `scene.ClearHighlightsAsync()`, then `scene.HighlightAsync(<the Northwind row>.Locator("a.reload"), "Reload")`. Do not click it. | The list of B with the ring and the caption **Reload** around the **Reload** button of the Northwind row. | "Reload": `Views/ManageTenantActionButtons.cshtml:30`. |

### `05-features`: Indexing Worker (instance A)

`VIDEO_BASE_URL=http://localhost:5010`. Starts on `/Admin/Features`, search box empty. Indexing Worker, Lucene Worker and
Elasticsearch Worker show **Disabled**. As in `03-enable`, the page after the click can take several seconds because the
Default tenant is released on both instances.

| Step | Actions | Visible when the narration ends | Label source |
|---|---|---|---|
| `features-1` (about 12 s) | `scene.TypeAsync` "Worker" in `#search-box`. | Three rows: **Elasticsearch Worker** and **Lucene Worker** under the heading **Search**, and **Indexing Worker** under **Indexing**, all **Disabled**. | Names and categories: `OrchardCore.Indexing/Manifest.cs:19,22`, `OrchardCore.Lucene/Manifest.cs:33,36`, `OrchardCore.Elasticsearch/Manifest.cs:33,36`. |
| `features-2` (about 10 s) | `scene.ClickAndWaitForNavigationAsync` on `#btn-enable-OrchardCore_Indexing_Worker`. Then `scene.TypeAsync` "Indexing Worker" in `#search-box`. | The single row **Indexing Worker** with the badge **Enabled**. | Button: `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`. |

### `05-task`: the task on instance B (instance B)

`VIDEO_BASE_URL=http://localhost:5011`. Wait before recording, off camera: B must have handled a request after
`05-features` (open `/Admin/BackgroundTasks` on B), and the background service must have loaded the task, which can
take up to a minute (`PollingTime`). Reload `/Admin/BackgroundTasks` on B until the row **Indexes Content Updater** no
longer shows the badge **Waiting for the background service to load this task**, and the details page shows a **Last run**
other than **Never** (the task runs every minute). Then `scene.OpenAsync("/Admin/BackgroundTasks")`.

| Step | Actions | Visible when the narration ends | Label source |
|---|---|---|---|
| `task-1` (about 8 s) | `scene.HighlightAsync` on the link **Indexes Content Updater**. | The Background Tasks list of B with the row **Indexes Content Updater** and its description, inside the ring. | "Tools", "Background Tasks": `OrchardCore.BackgroundTasks/AdminMenu.cs:18-19`. Title: `OrchardCore.Indexing/ContentIndexingBackgroundTask.cs:11`. Row link: `OrchardCore.BackgroundTasks/Views/BackgroundTaskEntry.Fields.SummaryAdmin.cshtml`. |
| `task-2` (about 8 s) | `scene.ClearHighlightsAsync()`, `scene.ClickAndWaitForNavigationAsync` on the link **Indexes Content Updater**, then `scene.HighlightAsync` on the **Settings** card. | The details page: the card **Settings** inside the ring, with **Schedule** `* * * * *` and **Atomic** `Yes, lock timeout 1000 ms, lock expiration 300000 ms`. | Cards and rows: `OrchardCore.BackgroundTasks/Views/BackgroundTask/Details.cshtml:137,155,167`. Values: `OrchardCore.Indexing/ContentIndexingBackgroundTask.cs:11-15`. |
| `task-3` (about 10 s) | `scene.ClearHighlightsAsync()`, then `scene.HighlightAsync` on `.card-footer` of the **Execution** card, no caption. | The footer "The execution state is tracked in memory by the server handling this request, since the application started." inside the ring. | `Details.cshtml:128-129`. |

## 4. The slides

Slide steps are drawn by `slides.py`; nothing is recorded for them.

- `why-1`, `why-2`, `why-3`: four cards, "State that stays on one server": tenant settings, tenant state, Lucene indexes, background tasks. All four bright, then the first two, then the last two.
- `shared-1`: three cards for the layout of the container: `tenants.json`, the optional `appsettings.json`, and `Sites/Northwind/appsettings.json`.
- `shared-2`: the host's `Program.cs` with `AddAzureShellsConfiguration`, and the `OrchardCore:Shells:Azure` section of `appsettings.json`.
- `shared-3`: three cards: create the container first, keep it private, `MigrateFromFiles` (a setting of the `OrchardCore:Shells:Azure` section, `BlobShellStorageOptions.MigrateFromFiles`).
- `shared-4`: the `AddDatabaseShellsConfiguration` alternative in `Program.cs`.
- `redis-1`: the `OrchardCore:Redis:Configuration` setting, and two cards: the same Redis server on every instance, a valid string is required.
- `how-1`, `how-2`: four cards, "How the instances stay in step"; the first two bright, then the last two.
- `log-1`, `log-2`, `log-3`: three cards, "Indexing in a web farm": indexing tasks, last task processed, catching up; one bright at a time.
- `worker-1`, `worker-2`: the code of `ContentIndexingBackgroundTask`; the title, schedule and class are bright, then the two lock settings.
- `list-1`: four check-mark cards: shared database, shared tenant settings, distributed cache and tenants, index worker.

## 5. Checks for private data

- The password `<password>` and `<db-password>` are never typed on camera and never shown on a page. The Tenants list
  shows only the provider name of the database, not the connection string.
- The only address is `admin@example.com`; no real name, email, company or domain appears on any screen.
- OCR the recordings (one frame per second and at every scene change) for `Password`, `Server=`, `AccountKey`,
  `UseDevelopmentStorage` and any name other than `Northwind`, `Default` and `admin`.
- The slides hold only placeholders (`<Azure Storage connection string>`, `localhost:6379`).
- `state.json`, the `<farm>` folder, the `App_Data` folders and the Docker volumes are never committed.
- Stop the instances by process ID, remove the containers and volumes, and delete `src/FarmHost` afterwards.

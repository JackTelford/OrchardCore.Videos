# Open questions: web-farm

What the narration states was read from the source (`OrchardCMS/OrchardCore` at commit `92db4a83`) and its
documentation. The points below were read but not run: a person confirms them on a running farm before recording,
and corrects the narration if the site behaves differently.

## 1. An instance that is already running when Distributed Tenants is enabled

**Checked:** `DistributedShellHostedService` only reads the distributed cache of an instance whose Default tenant has
`OrchardCore.Tenants.Distributed` in its descriptor and resolves an `IDistributedCache` that is not the in-memory one
(`DistributedContext`, `GetDefaultShellDescriptorAsync`). When a feature is enabled, the instance that does it releases
its Default tenant, but its own descriptor has not got the feature yet at that moment, so it probably writes no
identifier for the others.

**Confirm:** whether a second instance that was already running before the enabling picks the change up (it is not
claimed in the narration), or needs its Default tenant to be reloaded, or a restart. The recording plan avoids the question
by starting instance B after `03-enable`.

## 2. Enabling Lucene Worker or Elasticsearch Worker

**Checked:** neither feature has a startup class or a task of its own. The only code that mentions them is
`WorkerFeatureMigrations` in `OrchardCore.Indexing`: its `Create()` runs once for a tenant and enables Indexing Worker if
one of them is enabled at that time. The task `Indexes Content Updater` is registered by Indexing Worker
(`OrchardCore.Indexing/Startup.cs`, `WorkerStartup`).

**Confirm:** on a tenant whose Indexing migration already ran, enabling Lucene Worker afterwards most likely does not enable
Indexing Worker. The narration only says the two features "register no task of their own" and enables Indexing Worker
directly. Also confirm with a maintainer whether Elasticsearch needs the worker at all: the Lucene page of the
documentation says it does not, while the manifest of Elasticsearch Worker says it keeps indices in sync with other instances.

## 3. Spoken forms (not checked with `pronounce.py`, which makes audio)

The storyboard's `spoken` map says `tenants.json` as "tenants dot Jason", `appsettings.json` as "app settings dot Jason",
`lucene.status.json` as "lucene dot status dot Jason", `OrchardCore.Shells.Azure` as "Orchard Core dot Shells dot Azure",
`App_Data` as "App Data" and `Url` as "U.R.L.". Confirm each with a sample, and also how the voice reads "Redis", "Lucene"
and "Northwind".

## 4. Azurite as the blob store of the Azure Shells Configuration provider

**Checked:** `BlobFileStore` builds a `BlobServiceClient` and a `DataLakeServiceClient` from the same connection string,
and falls back to flat-namespace operations when it can't detect the account type; the plan sets
`UseHierarchicalNamespace=false` and starts Azurite with `--skipApiVersionCheck`.

**Confirm:** that `UseDevelopmentStorage=true` is accepted by both clients, that `tenants.json` appears in the container
after Auto Setup, and that the container must exist first (nothing in `BlobFileStore` creates it, as the documentation says).

## 5. A shared database in the recording setup

The tracker note lists Redis and Azurite only. The plan adds PostgreSQL in Docker: with SQLite each instance would create its
own database file under its own `App_Data` folder (`SqliteHelper.GetDatabaseFolder`), so the instances would not share any
data. Confirm that PostgreSQL is acceptable, or choose another shared database server and change section 1.1 of the plan.

## 6. One sign-in for both instances

**Checked:** the cookie `orchauth_Default` has the same name on both instances (`OrchardCore.Users/Startup.cs:123`), cookies
are not scoped by port, and the Data Protection application name of a tenant is its name
(`ServiceCollectionExtensions.AddDataProtection`); with Distributed Data Protection (Redis) the key ring of the Default
tenant is shared.

**Confirm:** that a session created on `http://localhost:5010` is accepted on `http://localhost:5011` after the seed of
section 1.5 of the plan, and that the antiforgery cookie, whose name contains the version id of the tenant settings
(`ServiceCollectionExtensions.cs:447`), doesn't break the form posts of `03-enable` and `05-features` once Northwind exists.

## 7. Timing on the second instance

**Checked:** the sync service waits one second between two reads (`s_minIdleTime`), so "within a few seconds" holds on paper.
The Background Tasks page says a task is "Waiting for the background service to load" until the next polling of the
background service after a tenant is built, and `PollingTime` defaults to one minute in the documentation.

**Confirm:** how long B takes to list Northwind after `sync-4`, and how long B's Background Tasks page shows the task as
loaded after `05-features`. The plan waits for both off camera.

## 8. Two tabs in one clip

The plan records `04-sync` with a second tab created by hand (`scene.Context.NewPageAsync()`) and `SwitchToAsync`, since
`OpenAsync` and `NavigateAsync` use one base URL. Confirm that `SwitchToAsync` back and forth, and a `ReloadAsync` wrapped in
`WaitForNavigationAsync`, keep the screencast running on both tabs.

## 9. Selectors

The selectors of the plan (`#search-box`, `#btn-enable-OrchardCore_Redis_Cache`, `a.create`, `.list-group-item`, `.ta-badge`,
`a.reload`, `.feature-group`, `.card-footer`) were read from the Razor views, not run. `GenerateIdFromName` is expected to
replace the dots of a feature id with underscores. Check each with `Recorder probe`.

The selectors `.list-group-item`, `.ta-badge` and `a.reload` hold in the List layout, which is the default of the admin
lists (`AdminListOptions.DefaultLayout`, `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`). Confirm
that the Tenants list of the Default tenant uses it, and that the **Reload** button is shown in the row and not only in a
menu of the row actions (docs: `BackgroundTasks` mentions a Menu layout of the actions of the admin lists).

## 10. Enabling a feature from the Features page

**Checked:** the Enable link of a feature is a GET (`FeatureEntry.Buttons.SummaryAdmin.cshtml:40`) that enables the feature
with its dependencies (`Features/Controllers/AdminController.cs:247`, force `true`), so no confirmation dialog appears for
Redis Cache, Distributed Tenants or Indexing Worker.

**Confirm:** that no dialog or notification covers the rows at the end of `enable-1`, `enable-2` and `features-2`.

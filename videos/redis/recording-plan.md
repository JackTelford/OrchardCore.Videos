# Recording plan: Redis

How to record the two browser clips of this video without having to decide anything. The other chapters are slides
(`slides.py`) and the two cards, and need no recording. In this file a selector stands for `scene.Page.Locator(...)`.
Paths such as `OrchardCore.Features/...` are in `src/OrchardCore.Modules/` of the Orchard Core checkout the content was
read from (commit `92db4a83`); build and run that same checkout.

The video has nothing to seed: no content items, no settings saved by hand. The scene file has no `PrepareAsync`.

## The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of the checkout, run on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup through environment variables, so no setup form and no sign-in form is recorded (the video is not about the Setup screen) |
| Tenant | `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, recipe `Blog`, database provider `Sqlite` |
| Administrator | user `admin`, email `admin@example.com`, password `<password>`: the value of `VIDEO_PASSWORD`, chosen outside git |
| Redis | the official Redis image in Docker, with persistence on, listening on port 6379 of the machine |
| Features enabled before recording | none beyond the Blog recipe: the clips enable `OrchardCore.Redis`, `OrchardCore.Redis.Cache`, `OrchardCore.Redis.Bus`, `OrchardCore.Redis.Lock` and `OrchardCore.HealthChecks` on camera |
| Never enabled | `OrchardCore.Redis.DataProtection`: it stays disabled in this video (see `OPEN-QUESTIONS.md`) |

Environment variables of the site process (in addition to `ORCHARD_APP_DATA`):

```bash
OrchardCore__AutoSetup__Tenants__0__ShellName=Default
OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
OrchardCore__Redis__Configuration=localhost:6379
OrchardCore__Redis__InstancePrefix=MyApp:
OrchardCore__Redis__AllowAdmin=true
OrchardCore__HealthChecks__ShowDetails=true
HTTP_PROXY=http://localhost:9
HTTPS_PROXY=http://localhost:9
```

- `OrchardCore__Redis__Configuration` must be set before the site starts. Without it the Redis features can be enabled
  but stay inactive (`OrchardCore.Redis/Startup.cs:39-43`), and the health check in `04-health` reports `Unhealthy`.
- `OrchardCore__HealthChecks__ShowDetails=true` must be set before the site starts too: the endpoint is built when the
  tenant starts (`OrchardCore.HealthChecks/Startup.cs:36-55`). Enabling the feature in `04-health` starts the tenant
  again with it.
- The proxy variables keep the site from calling any outside service. Redis uses a direct connection to
  `localhost:6379`; the health check in `04-health` proves it works.
- The administrator's password is typed by nobody: Auto Setup sets it from the environment, and `Recorder setup` signs in
  with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`.

The Redis stand-in, started once before the first take and removed after the last. Start it before the site: the factory keeps the
`null` of a failed first connection in its cache for the life of the site process (`OrchardCore.Redis/Services/RedisDatabaseFactory.cs:34-51`),
so if Redis is not answering when the tenant first connects, the Redis features may stay without a connection until the site is
stopped and started again (not run, see `OPEN-QUESTIONS.md`).

```bash
docker run --name oc-video-redis -p 6379:6379 -v oc-video-redis-data:/data -d redis redis-server --appendonly yes
docker exec oc-video-redis redis-cli ping            # must answer PONG

# after the last take
docker rm -f oc-video-redis
docker volume rm oc-video-redis-data
```

## Order, snapshots and what each clip changes

Slides, cards and the two clips play in the order of the storyboard. The clips are recorded in this order, since the
second one needs the Redis features that the first one enables.

| Snapshot | How to make it |
|---|---|
| `snapshot-0` | Start the site with an empty `App_Data`, open `/` once so Auto Setup finishes, run `Recorder setup` (it saves `state.json`), stop the site, copy `App_Data` and `state.json` aside |
| `snapshot-1` | After recording `03-enable` as final: stop the site, copy `App_Data` aside |

| Clip | Starts from | Changes the site | Before a new take |
|---|---|---|---|
| `03-enable` | `snapshot-0` | enables `OrchardCore.Redis`, `OrchardCore.Redis.Cache`, `OrchardCore.Redis.Bus`, `OrchardCore.Redis.Lock` | restore `snapshot-0` |
| `04-health` | `snapshot-1` | enables `OrchardCore.HealthChecks` | restore `snapshot-1` |

Every restore of `App_Data` goes with a flush of Redis, because the cache, tag and lock keys of the earlier take stay in
the container and no longer match the restored database:

```bash
# stop the site (by its process ID), restore the snapshot, then:
docker exec oc-video-redis redis-cli FLUSHALL
# start the site again, and wait until http://localhost:5010/ answers
```

Rehearse both clips once on a copy of `snapshot-0`, then restore the snapshots and flush Redis before the take. Look
at the log of the site after the first feature is enabled: it must not hold the line "'Redis' features are not active
on tenant 'Default'".

## Clip `03-enable`

Starts on `/Admin/Features` (opened with `OpenAsync`, not recorded). Before `StartCaptureAsync`, expand **Tools** in the
admin menu (`ul#adminMenu`, the first level item whose text is `Tools`), so that **Features** is visible and selected.
The layout selector of the page stays as it is on a fresh site. The page is the `Features` view of the Features
module.

The narration of this clip is spoken over actions that reload the page after each click. Each reload empties the search
box, so each step types the search again when it needs it. `ClickAndWaitForNavigationAsync` is used for every click that
reloads the page. A success message appears at the top right after a reload and goes away after 5 seconds
(`OrchardCore.Features/Controllers/AdminController.cs:317`, `NotifierExtensions.cs:71`): start the click so that the page
has reloaded and the message is on screen when the narration of the step ends (wait `duration of the step - 6 s` first;
the durations are in `durations.json`).

`enable-1`
- Actions: `TypeAsync(#search-box, "OrchardCore.Redis")`, with the default typing delay. Wait until the summary line
  (`#list-summary`) reads `Showing 5 of ` followed by the total. Then `HighlightAsync` of the category heading
  `.feature-group:not(.d-none) h3` with text `Distributed` (caption `Distributed`), a pause of 1.5 s, `ClearHighlightsAsync`,
  `HighlightAsync` of the heading with text `Security` (caption `Security`).
- Visible at the end: five rows. Under **Distributed**: **Redis**, **Redis Bus**, **Redis Cache**, **Redis Lock**, in this
  order, each with its `Enable` button and the badge **Disabled**. Under **Security**: **Distributed Data Protection (Redis)**
  with the badge **Disabled**. The **Security** heading is ringed.
- Labels from: Tools, Features `OrchardCore.Features/AdminMenu.cs:26-27`; Search box `OrchardCore.Features/Views/Admin/Features.cshtml:27`;
  names and categories `OrchardCore.Redis/Manifest.cs:11-46`; the order (category, then name) `OrchardCore.Features/Controllers/AdminController.cs:91-94`;
  the search matches name, id and description `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`, so `OrchardCore.Redis`
  shows exactly these five features.

`enable-2`
- Actions: `ClearHighlightsAsync`. `HighlightAsync` of the dependency badge of the **Redis Cache** row,
  `[data-filter-value*="OrchardCore.Redis.Cache"] .direct-dependencies` (caption `Depends on Redis`; confirm the selector with
  `Recorder probe`), a pause of 2 s, `ClearHighlightsAsync`. Then click the `Enable` link of **Redis Cache**:
  `a[href*="OrchardCore.Redis.Cache/Enable"]` (its id is `btn-enable-OrchardCore_Redis_Cache`; confirm it with
  `Recorder probe`), with `ClickAndWaitForNavigationAsync`, timed as described above. The page reloads on `/Admin/Features`.
- Visible at the end: the success message at the top right, "The feature Redis Cache was enabled." It names only the
  feature that was clicked: **Redis**, enabled with it as its dependency, is not in the message and is not on screen
  yet (the page behind the message is the unfiltered Features list; `enable-3` shows both).
- Labels from: `Enable` `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the message
  `OrchardCore.Features/Controllers/AdminController.cs:317` ("The feature {name} was enabled." for one feature, "The following
  features were enabled: ..." for several), built from the features that were requested, not from the dependencies enabled
  with them (`OrchardCore.Features.Core/Services/FeatureService.cs:79-80`, in `src/OrchardCore/`); the click enables the
  dependencies too, because the action passes `force` as true (`AdminController.cs:247`); the dependency
  `OrchardCore.Redis/Manifest.cs:21`; the dependency badge `OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12-18`.

`enable-3`
- Actions: `TypeAsync(#search-box, "OrchardCore.Redis")`. Wait for the summary line to read `Showing 5 of `, then a pause of
  4 s: the first sentence of the narration says that **Redis** and **Redis Cache** are both enabled, and the two rows show
  the badge **Enabled** (**Redis** was enabled by the click of `enable-2`, as the dependency of **Redis Cache**). Click the
  checkbox `input[name="featureIds"][value="OrchardCore.Redis.Bus"]`, then
  `input[name="featureIds"][value="OrchardCore.Redis.Lock"]`. Click `#bulk-action-menu-button` (text **Bulk Actions**), then
  click the item `a[data-action="Enable"]` (text **Enable**) with `ClickAndWaitForNavigationAsync`. The page reloads.
- Visible at the end: the success message at the top right: "The following features were enabled: ..." followed by
  **Redis Bus** and **Redis Lock** (the order of the two names is not verified, see `OPEN-QUESTIONS.md`).
- Labels from: `OrchardCore.Features/Views/Admin/Features.cshtml:33-38` (**Bulk Actions**, **Enable**),
  `OrchardCore.Features/Views/FeatureEntry.Checkbox.SummaryAdmin.cshtml:11`; the bulk action enables with the dependencies
  `OrchardCore.Features/Views/Admin/Features.cshtml:49` (hidden field `Force`), `AdminController.cs:178`; the message
  `AdminController.cs:317`, built from the selected features only.

`enable-4`
- Actions: `TypeAsync(#search-box, "OrchardCore.Redis")`. Wait for the summary line to read `Showing 5 of `. Then
  `HighlightAsync` of the row `[data-filter-value*="OrchardCore.Redis.DataProtection"]` with the caption `Disabled`.
- Visible at the end: **Redis**, **Redis Bus**, **Redis Cache** and **Redis Lock** with the badge **Enabled**; under **Security**,
  **Distributed Data Protection (Redis)** with the badge **Disabled**, its description readable ("Enables distributed
  data protection using Redis; recommended only with a Redis server configured for persistence."), and the ring around it.
- Labels from: badges `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9,15`; the description
  `OrchardCore.Redis/Manifest.cs:44`.

## Clip `04-health`

Starts on `/Admin/Features` (opened with `OpenAsync`), from `snapshot-1`, with **Tools** expanded as in `03-enable`.
The site has `OrchardCore.Redis`, `OrchardCore.Redis.Cache`, `OrchardCore.Redis.Bus` and `OrchardCore.Redis.Lock` enabled,
and Redis is running.

`health-1`
- Actions: `TypeAsync(#search-box, "OrchardCore.HealthChecks")`. Wait until the summary line reads `Showing 1 of `. Click
  the `Enable` link `a[href*="OrchardCore.HealthChecks/Enable"]` with `ClickAndWaitForNavigationAsync`, timed so that the
  success message is on screen when the narration of the step ends.
- Visible at the end: the success message at the top right ("The feature Health Checks was enabled."); the unfiltered
  Features list behind it.
- Labels from: **Health Checks** `OrchardCore.HealthChecks/Manifest.cs:4`; the message
  `OrchardCore.Features/Controllers/AdminController.cs:317`; the check name `OrchardCore.Redis/HealthChecks/Extensions/RedisHealthCheckExtensions.cs:13`.

`health-2`
- Actions: `NavigateAsync("/health/live")`. Wait until `document.body.innerText` contains `Redis Health Check`
  (`WaitForFunctionAsync`). Do not click anything.
- Visible at the end: the JSON of the page: `Status` is `Healthy`, and the `HealthChecks` list holds an entry with `Name`
  `Redis Health Check` and `Status` `Healthy`. If the page shows only `Healthy`, the details setting was not applied:
  stop, fix `OrchardCore__HealthChecks__ShowDetails`, restore `snapshot-1`, and take it again.
- Labels from: the route `OrchardCore.HealthChecks/Startup.cs:36-55`; the response `OrchardCore.HealthChecks/Services/DefaultHealthCheckResponseWriter.cs`;
  the ping `OrchardCore.Redis/HealthChecks/RedisHealthCheck.cs`.

`health-3`
- Actions: none. Hold on the page of `health-2` (`HoldAsync` for the length of the narration).
- Visible at the end: the same JSON page.
- Labels from: the compact response and the 503 status, `src/docs/reference/modules/HealthChecks/README.md` ("Response and
  status behavior"); the missing authentication, the same page ("The endpoint does not require authentication").

## Slide steps

Rendered by `slides.py`; nothing to record.

- `why-1`: four cards, "Local to each server": the cache, the signals, the locks and the keys that each server keeps for itself.
- `why-2`: four cards, "One connection, four features": Redis Cache, Redis Bus, Redis Lock, Distributed Data Protection (Redis).
- `connect-1`: the Redis section of the configuration with `Configuration` bright, the Docker command that starts a Redis
  server, and two cards on `Configuration`.
- `connect-2`: the same section with `InstancePrefix`, `AllowAdmin` and `DisableCertificateVerification` bright, and a card for each.
- `connect-3`: three cards: one connection per connection string, prefixed keys, the key ring of the `Default` tenant as an example.
- `connect-4`: three cards: what a missing or invalid `Configuration` does (error logged, features inactive, health check Unhealthy).
- `cache-1`: the startup of Redis Cache with the distributed cache registration bright, and cards for the documents and Dynamic Cache.
- `cache-2`: the same startup with the tag cache registration bright, and cards for the tag cache.
- `bus-1`: the startup of Redis Bus and the signal that publishes to a message bus.
- `bus-2`: the publish and subscribe code of the bus, with the host name check.
- `lock-1`: the startup of Redis Lock and the call that takes the lock in Redis.
- `lock-2`: the `Program.cs` that adds Redis Lock to the setup features, the environment variables of the lock, and three cards.
- `lock-3`: an atomic background task with `LockTimeout` and `LockExpiration`, and three cards.
- `protect-1`: four cards: what the data protection keys protect, and where they are by default.
- `protect-2`: how the key ring is stored in Redis, with the key name, persistence and backup as cards.
- `protect-3`: the persistence check and the warning of the key management setup.
- `module-1`: a service that takes `IRedisService`, and the interface.
- `module-2`: the method that connects when needed and handles a failure.
- `module-3`: a startup that registers a Redis service only when Redis is configured, and `IsRedisConfigured`.

The code of the slides comes from `OrchardCore.Redis/`, `OrchardCore.Redis.Abstractions/` and
`src/docs/reference/modules/Redis/README.md`, trimmed with `// ...`; the settings come from
`OrchardCore.Redis/ConfigurationSchema.json`, and the environment variables of `lock-2` from
`src/docs/reference/modules/AutoSetup/README.md`.

## Before the take: checks for private data

- The only account on screen is `admin`, with the email `admin@example.com` (not shown by these clips). No other person,
  company, address or phone number exists on the site: it is a fresh Blog site.
- No password, connection string, token or key is on any screen: the Features page and `/health/live` show feature names,
  check names and statuses only. The password is in `VIDEO_PASSWORD` and in the Auto Setup variable, never in a file of the
  repository, a shell history that is committed, or a `state.json` in git (it is git-ignored).
- The Redis prefix is `MyApp:` and the host is `localhost`; neither names a real organization.
- Do not commit `App_Data`, `state.json`, the snapshots, the Redis data volume, a mask script or any clip.
- Look at the contact sheet of each clip (`review.py`): the frame at the end of every step matches the "Visible at the end"
  line above, and no error message, empty list or login page is on screen.

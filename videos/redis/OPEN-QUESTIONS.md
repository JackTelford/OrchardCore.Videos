# Open questions: Redis

What was checked in the source of Orchard Core (commit `92db4a83`, under `src/OrchardCore.Modules/` unless a path says
otherwise), and what a person must confirm on a running site or by listening. Nothing here is narrated as a fact beyond
what the source and the documentation show.

## To confirm on a running site

1. **The search on the Features page.** Checked: the filter text of a row is its name, id and description
   (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), so `OrchardCore.Redis` matches the ids of the five
   Redis features and nothing else; the rows are grouped by category, then ordered by name
   (`OrchardCore.Features/Controllers/AdminController.cs:91-94`), which gives **Redis**, **Redis Bus**, **Redis Cache**,
   **Redis Lock** under **Distributed** and **Distributed Data Protection (Redis)** under **Security**
   (`OrchardCore.Redis/Manifest.cs`). Confirm: the five rows and their order on screen, with the layout the admin lists use
   on a fresh site (List or Grid), and that `enable-1` ends with all of them visible without scrolling.
2. **The Enable links and the messages.** Checked: the link id is built with `GenerateIdFromName`
   (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`) and the message is "The feature ... was enabled."
   for one feature or "The following features were enabled: ..." for several (`AdminController.cs:317`), shown for 5 seconds
   (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`). The names in it are the features that were
   requested, not the dependencies enabled with them (`OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:79-80`),
   so the click on **Redis Cache** gives "The feature Redis Cache was enabled." and **Redis** is enabled without being named.
   Confirm: the exact id (expected `btn-enable-OrchardCore_Redis_Cache`), that the click reloads the page, that **Redis** shows
   the badge **Enabled** after it (`enable-3` relies on it), the order of the two names in the message of `enable-3`, that the
   message is still on screen when the narration of each step ends, and that the dependency badge of the **Redis Cache** row
   (`.direct-dependencies`, `FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12`) is drawn in the layout of a fresh site (List or Grid).
3. **The health check response.** Checked: with `ShowDetails` on, `/health/live` answers `200 OK` with a JSON object that
   has `Status`, `Duration` and `HealthChecks` (a list of `Name`, `Status`, `Description`)
   (`OrchardCore.HealthChecks/Services/DefaultHealthCheckResponseWriter.cs`), and the Redis check is registered as
   "Redis Health Check" only while both `OrchardCore.Redis` and `OrchardCore.HealthChecks` are enabled
   (`OrchardCore.Redis/HealthChecks/Startup.cs:9-13`, `RedisHealthCheckExtensions.cs:13`). Confirm: that the entry reads
   `Healthy` against the Redis container, and what the raw JSON looks like in the recorder's Chromium: whether it is
   legible at the recording scale, and whether the recorder's cursor and highlight overlays are injected into a response
   that is not an HTML page (the steps `health-2` and `health-3` need neither: they only wait and hold).
4. **Distributed Data Protection (Redis) stays disabled in the recording.** Checked: the feature replaces the key ring of
   the tenant, which is a folder of `App_Data` by default (`OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:563-566`),
   with one stored in Redis (`OrchardCore.Redis/Options/RedisKeyManagementOptionsSetup.cs`). Not verified: the effect of
   switching a signed-in site from one key ring to the other. The expectation is that the cookie of the recorder's
   `state.json` and the anti-forgery tokens of the open page can no longer be read, so the next request lands on the
   sign-in page. This is why `03-enable` leaves the feature disabled and `enable-4` says why from its description. If a
   clip that enables it is ever added, check this first.
5. **The warning about persistence.** Checked: `RedisKeyManagementOptionsSetup.cs:38` logs the warning when
   `CheckRedisPersistenceEnabled()` is false, and that method returns false without looking at the server whenever admin
   commands are not allowed (line 93) or when no save was seen since Redis started (the comment in the code says the RDB
   check "is not very reliable"). The narration (`protect-3`) says the module logs the warning unless it can confirm
   persistence, and can only check when admin commands are allowed. Confirm in the log of the site, with the container
   started with `--appendonly yes`, once with `OrchardCore__Redis__AllowAdmin=true` (no warning expected) and once without
   it (the warning is expected).
6. **Auto Setup with several instances.** The narration of `lock-2` repeats the documentation: Redis Lock makes the
   auto setup atomic when several instances share a database, and `LockTimeout` and `LockExpiration` default to sixty
   seconds. Checked: `OrchardCore.AutoSetup/Extensions/DistributedLockExtensions.cs:35-51` takes the lock `AUTOSETUP_LOCK`
   with 60,000 milliseconds when no option is set, and `OrchardCore.AutoSetup/Options/AutoSetupOptions.cs:53` rejects a
   `LockOptions` section where either value is zero or less, which is why the slide says to set them together. Not
   demonstrated: two instances setting up one database.
7. **A failed first connection.** Checked: `RedisDatabaseFactory.CreateAsync` (`OrchardCore.Redis/Services/RedisDatabaseFactory.cs:34-51`)
   returns `null` when the connection fails and keeps that `null` in its static cache for the configuration string, until the
   application stops. Not run: whether the StackExchange.Redis default (`abortConnect`) makes the first connection fail rather
   than retry in the background. The plan starts Redis before the site for that reason. Not narrated.
8. **Key names.** The slides and narration give the key ring name `{InstancePrefix}{TenantName}:DataProtection-Keys`
   (documentation and `RedisKeyManagementOptionsSetup.cs:31`). The locks, the bus channel and the tags use
   `{InstancePrefix}{TenantName}:` followed by the key (`RedisLock.cs`, `RedisBus.cs`, `RedisTagCache.cs`), but the
   distributed cache uses `{InstancePrefix}{TenantName}` without the colon (`RedisCacheOptionsSetup.cs:31`). Nothing
   narrated depends on the cache keys. List the keys with `redis-cli --scan` before any slide shows one.

## To confirm in the text of the slides

9. **The Docker command on `connect-1`** (`docker run --name redis -p 6379:6379 -d redis`) is the usual way to start the
   official Redis image. It is not in the Orchard Core documentation, which only gives connection strings such as
   `localhost:6379`. Confirm that the command and the image are what the project wants to show, and that the container
   name is not in conflict with the one of `recording-plan.md`.
10. **Invented names around real code.** `ReportBackgroundTask`, its title and its lock values (`lock-3`) are examples
   around the real `[BackgroundTask]` attribute (`OrchardCore/OrchardCore.Abstractions/BackgroundTasks/BackgroundTaskAttribute.cs`).
   The file name `RedisLock.cs` of `lock-1` shows one real line with a comment of the slide's author. `CounterService`,
   `ICounterStore`, `RedisCounterStore` and the `Startup` of `module-1` to `module-3` come from
   `src/docs/reference/modules/Redis/README.md`, trimmed (the `try` block of `module-2` keeps its `catch` as `// ...`).

## To confirm by listening

Not run: `scripts/pronounce.py` writes audio. Check how the voice says these, and fix the entries of `"spoken"` in
`storyboard.json` if needed:

- `Redis` (no entry: it should sound like "red-iss"), `SMTP`, `Docker`, `anti-forgery`.
- `OrchardCore.Redis` as "Orchard Core dot Redis", `/health/live` as "slash health slash live", `App_Data` as "App Data".
- `AOF` as "A.O.F." and `RDB` as "R.D.B.".
- `IRedisService` as "I Redis Service", `NullRedisService` as "Null Redis Service", `IsRedisConfigured` as "Is Redis Configured",
  `ConnectAsync` as "Connect Async".
- `InstancePrefix`, `AllowAdmin`, `LockTimeout`, `LockExpiration`, `DisableCertificateVerification`,
  `ShowDetails`, `DataProtection-Keys` as the words they are made of.
- The replacements are plain text substitutions in the order of the map: check that none of them changes a longer word.

## Noticed while reading, not narrated

- `OrchardCore.Redis/Options/RedisKeyManagementOptionsSetup.cs:71` builds the name of the old key it copies from, for an
  upgrade of an existing site, as `"({prefix}{tenant}:DataProtection-Keys"`, which starts with a parenthesis. It is probably
  the literal name of the old format that the comment above it mentions; it was not checked against a site made with
  that format. The video does not talk about that copy.

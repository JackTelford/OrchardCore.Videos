# Open questions: Data Protection Keys

The narration states only what the source and the documentation show. Everything below was read in the source, or is
not in the source at all; a person confirms it on a running site, as the optional run of `recording-plan.md` describes,
before recording. The source is Orchard Core at commit 92db4a83.

## Checked in the source, to confirm on a running site

1. **The sign-in survives the click on Enable (`enable-3`).** With no Azure connection string and no Redis
   configuration, the Azure feature registers nothing (`OrchardCore.DataProtection.Azure/Startup.cs:34`, `:69`), so the
   key ring does not change and `state.json` should stay valid. Confirm that the page after the click is the Features
   page with the message "The feature Distributed Data Protection (Azure Storage) was enabled." and not the sign-in page.
2. **The Features page.** Confirm the labels and the default state: the search for `data protection` leaves exactly two
   rows (the search reads the name, the identifier and the description: `Drivers/FeatureEntryDisplayDriver.cs:20`), in the
   category "Security"; the layout is List with Enable as a button (`AdminListOptions.cs:30`, `:36`); the "Redis"
   dependency badge shows on the Redis row; and the success message appears at the top after the redirect. Confirm the id
   of the Enable button, `btn-enable-OrchardCore_DataProtection_Azure`: it comes from `Html.GenerateIdFromName`
   (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`), which replaces dots by underscores unless the site changes
   `IdAttributeDotReplacement`.
3. **The log entries of `check-1`.** The texts come from `Startup.cs:69` of the Azure feature (critical level: "No
   connection string was supplied for OrchardCore.DataProtection.Azure.") and `OrchardCore.Redis/Startup.cs:41` (error
   level: "'Redis' features are not active on tenant '...': ..."). Confirm both on a site with the feature enabled and no
   settings, with the logger the recording site uses.
4. **The blob path (`azure-3`, `check-2`).** The container is `dataprotection` and the blob `Sites/Default/DataProtectionKeys.xml`
   (`BlobOptions.cs:7`, `BlobOptionsConfiguration.cs:96`, `ShellOptionConstants.DefaultSitesPath`). With Azurite
   (`UseDevelopmentStorage=true`), confirm the container and the blob, and note when the blob first appears (at the start
   of the tenant, or at the first request that protects something). The docs say the container is created "during
   `Startup`"; the code creates it when the options are first resolved (`BlobOptionsConfiguration.cs:76`).
5. **The removal of a tenant (`azure-3`).** `BlobModularTenantEvents.cs:29` deletes the blob of the tenant. Confirm that
   removing a tenant (Multi-Tenancy, Tenants) deletes `Sites/<tenant>/DataProtectionKeys.xml`, and that the container stays.
6. **Liquid in the names (`azure-4`).** Confirm that `{{ ShellSettings.Name }}-dataprotection` gives a lowercased
   container for a tenant with an upper-case name, and that the blob name is not lowercased
   (`BlobOptionsConfiguration.cs:54` lowercases the container name only).
7. **The Redis key (`redis-2`).** With a Redis container, confirm the key `MyApp:Default:DataProtection-Keys`
   (`RedisKeyManagementOptionsSetup.cs:31`).
8. **The Redis persistence warning (`redis-3`).** The feature logs the warning whenever it does not find persistence,
   including when `AllowAdmin` is false and it cannot check (`RedisKeyManagementOptionsSetup.cs:39` and `:93`). Confirm with
   Redis started with and without `--appendonly yes`, and with `AllowAdmin` true and false.
9. **Azurite details.** Confirm that `UseDevelopmentStorage=true` is accepted by the blob client of the feature, and
   whether the current Azurite image needs `--skipApiVersionCheck`.

10. **A fixed `BlobName` (`azure-4`).** The code joins nothing to a `BlobName` that is set: `BlobOptionsConfiguration.cs:108`
    uses it as given. A `BlobName` without the tenant in it gives every tenant the same blob, and
    `BlobModularTenantEvents.cs:29` deletes that blob when any one tenant is removed. Confirm on a site with two tenants;
    if it holds, one clause in `azure-4` would say that a blob name needs the tenant in it.
11. **The Redis prefix (`redis-2`).** The key is `{InstancePrefix}{TenantName}:DataProtection-Keys` with no separator
    (`RedisKeyManagementOptionsSetup.cs:31`), so the colon belongs to the prefix, as in the documentation's `MyApp:`.
    Confirm the key with the prefix `MyApp:` and with `MyApp`.

12. **The old Redis key name (not in the narration).** `RedisKeyManagementOptionsSetup.cs:71` builds the old key with a
    leading parenthesis (`"({InstancePrefix}{Tenant}:DataProtection-Keys"`), which looks like a typo, so the copy of an
    old key would never find one. The video does not mention the copy; a person may report it to the project.

## Not in the source: kept out of the narration

- **Whether the keys are stored encrypted.** The default registration sets a null XML encryptor
  (`ServiceCollectionExtensions.cs:576`), and the Azure feature removes every earlier key management setup
  (`Startup.cs:42`), so the source alone does not say whether the blob or the Redis key holds the keys in clear text.
- **Which store wins when both features are enabled and configured.** The Azure startup removes the earlier key
  management setups, and the Redis startup adds one; the order depends on the startup order of the two modules.
- **What happens to an existing site when its store changes.** Neither feature copies the keys of the local folder
  (the Redis setup only copies an old Redis key name). The consequence for the sessions and for the saved credentials
  of a site that already uses the local folder is standard Data Protection behavior, not shown in the source.
- **Two servers.** The video does not show two instances sharing a key ring. Confirm with two copies of the site on
  the same database and the same store before anyone adds that to the video.

## Pronunciation: have a person confirm each spelling

`scripts/pronounce.py` was not run. The `spoken` map of `storyboard.json` holds these spellings: `App_Data` as "App Data",
`appsettings.json` as "app settings dot JSON", `OrchardCore.DataProtection.Azure` as "Orchard Core dot Data Protection dot
Azure", `DataProtection-Keys` as "Data Protection Keys", `dataprotection` as "data protection", and `ConnectionString`,
`ContainerName`, `BlobName`, `CreateContainer`, `InstancePrefix`, `AllowAdmin` and `ShellSettings` as separate words.
Also listen to "AOF" as "A.O.F." and "RDB" as "R.D.B.", to "Redis", to "Azure Blob Storage", and to the step
`redis-2`, which reads "DataProtection-Keys" inside a sentence.

## The slides were not looked at

The slides render in memory without an error, and a script measured that every text stays inside its card or panel.
Nobody has looked at them (no image may be saved by the author). Look at each slide once: the `bash` colors of the
blob paths and of the Redis key, the `{{ ShellSettings.Name }}` lines of `azure-4`, and the two code panels of
`redis-4`.

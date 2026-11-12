# Open questions: shells-and-configuration

Everything in the narration was read from the source of `OrchardCMS/OrchardCore` (commit `92db4a83`) or from its
documentation. Nothing here was run, so a person should confirm these points on a running site, or by listening, before
the video is published. Paths are relative to `src/OrchardCore/` of that checkout unless noted.

## 1. Do environment variables win over `App_Data/appsettings.json`? (`layers-2`, the slide `layers-2`)

- **Docs** (`src/docs/reference/modules/Configuration/README.md`, "Configuration Sources Order"): the host sources,
  including environment variables and command line arguments, come first, then `App_Data/appsettings.json`, and "last
  wins". That makes `App_Data/appsettings.json` win over an environment variable.
- **Code** (`OrchardCore/Shell/ShellSettingsManager.cs`, `EnsureConfigurationAsync`, lines 245-257): the environment
  variable and command line providers of the host are added again after the `App_Data` files, so they win over them.
  The narration of `layers-2` follows the code ("applied again after them, so they still win").
- **Check on a site**: with the seed file of the recording plan, stop the site, set
  `OrchardCore__Acme__Description="From the variable"`, start the site, and open **Edit** for Acme in
  **Multi-Tenancy > Tenants**. The code predicts the Description `From the variable`; the docs predict
  `The Acme sample site.`. If the docs are right, change the second sentence of `layers-2`, the text of the second
  card in `slides.py`, and the entry about it that goes to the documentation issues. Remove the variable afterwards.

## 2. Is a shared value left out of a tenant's own files? (`shared-3`)

- **Source**: `ShellSettingsManager.SaveSettingsAsync` (lines 132-209) writes a value to `tenants.json` or to the
  tenant's `appsettings.json` only when it differs from the configuration the tenant inherits; the documentation says
  "Tenants' shell settings won't contain this information".
- **Check on a site**: start a site with `OrchardCore__DatabaseProvider=Sqlite` at the root, create a tenant from
  **Add Tenant**, set it up, and read `<app-data>/Sites/<tenant>/appsettings.json`. The code predicts that
  `DatabaseProvider` is not in the file.

## 3. The stateless providers were not run (`stores-2` to `stores-7`)

No Azure account or database server was available. The layout of the container and the two documents of the
Database provider come from `OrchardCore.Shells.Azure/Configuration/*.cs` and
`OrchardCore.Infrastructure/Shells.Database/Configuration/*.cs`. If a reviewer can, run the host with an Azure Storage
emulator and check that the container holds `tenants.json` at its root (or base path) and
`Sites/<tenant>/appsettings.json` for each tenant, as `stores-4` says.

## 4. Does a `PostConfigure` in the host win over the site settings? (`modules-3`)

- **Docs** (`src/docs/reference/modules/Configuration/README.md`, "`IOptions` Configuration"): `PostConfigure` in the
  host's `ConfigureServices` overrides the site settings of `ResourceOptions`.
- **Code**: the site settings are applied by `IPostConfigureOptions<ResourceOptions>`
  (`OrchardCore.Settings/ResourceOptionsConfiguration.cs`, registered in `OrchardCore.Settings/Startup.cs:61`), and
  post-configure actions run in the order of their registration. The host's `ConfigureServices` has the order 0
  (`OrchardCore.Abstractions/Modules/Builder/OrchardCoreBuilder.cs:32`), the same as the module startups
  (`OrchardCoreConstants.ConfigureOrder.Default`). The host's startups are copied into the tenant container before the
  module ones (`ShellContainerFactory.cs:45-61`) and the sort by `Order` is stable (`:122`), so the host's
  `PostConfigure` looks registered before the one of the Settings module, which would then run last and win.
- The narration of `modules-3` therefore says only what the code does for sure: the value is read from the tenant's
  configuration and set with `PostConfigure`, and the admin still shows the saved value
  (`DefaultSiteSettingsDisplayDriver.cs:157`). It does not say that the code wins.
- **Check on a site**: with the Resources feature enabled, in **Settings > General** (`OrchardCore.Settings/AdminMenu.cs:36-43`) tick **Use framework CDN (Content Delivery Network)**
  (`Views/Settings-Resources.Edit.cshtml:29`) and save, add the host code of
  the slide with `"Resources:UseCdn"` set to false (`OrchardCore__Resources__UseCdn=false`), and look at whether the
  resources of a page are served from a CDN. Then pass a larger `order` to `ConfigureServices` and look again. If the
  host code wins as the docs say, `modules-3` can say that it overrides the site settings.

## 5. Does `{{ ShellSettings.Name }}` render in the patterns? (`shared-2`)

- **Source**: `TenantDatabasePatternResolver.Resolve` puts the `ShellSettings` object in a Fluid `TemplateContext` built
  from `TemplateOptions.Default` (`OrchardCore.Tenants/Services/TenantDatabasePatternResolver.cs:88-89`). The docs and the
  sample `appsettings.json` use `{{ ShellSettings.Name }}`. No code registers the type for member access, and the Fluid
  library is not in the checkout, so this could not be read.
- **Check on a site**: set `RequireTablePrefix` and `TablePrefixPattern` as on the slide with a database provider that
  supports prefixes, open **Add Tenant** for the name `Beta`, and check that **Table Prefix** is read-only and generated
  (the hint says "Generated automatically from the configured tenant table prefix pattern.") and that the tenant is
  created with the prefix `Beta`. The options are read by the **Tenants** feature, so it must be enabled.

## 6. Pronunciations to confirm with `scripts/pronounce.py`

The `spoken` map of `storyboard.json` has these entries; a person should listen to each before the voice is generated:

- `appsettings.json` as "app settings dot json" and `tenants.json` as "tenants dot json" (the voice may say "json" as
  "Jason"; decide whether to spell it "J.S.O.N.").
- `App_Data` as "App Data", `ORCHARD_APP_DATA` as "ORCHARD APP DATA", and `OrchardCore_` as "Orchard Core underscore".
- `IShellConfiguration` as "I Shell Configuration".
- `OrchardCore.Shells.Azure` as "Orchard Core dot Shells dot Azure".
- `AddAzureShellsConfiguration`, `AddDatabaseShellsConfiguration`, `MigrateFromFiles`, `RequireTablePrefix`,
  `TablePrefixPattern`, `SchemaPattern`, `ConfigureServices` and `PostConfigure` as the same words with spaces.
- `DatabaseProvider` and `ConnectionString` as "Database Provider" and "Connection String".
- `UseCdn` as "Use C.D.N.", `MyService` as "My Service" and `MyProvider` as "My Provider".
- Plain words the voice might stumble on: "Uninitialized", "Acme", "Fluid", "SQL", "Url Prefix".

The validator counts each identifier as one word, but the voice says several, so check that `durations.json` stays under
8 minutes after narrating.

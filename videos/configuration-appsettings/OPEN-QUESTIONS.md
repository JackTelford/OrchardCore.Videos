# Open questions: configuration-appsettings

Everything in the narration was read in the Orchard Core source at commit `92db4a83` and in the documentation pages
`src/docs/reference/modules/Configuration/README.md` and `src/docs/topics/configuration/README.md`. No site was run to
write this video. What a person must confirm on a running site, or by listening, before recording:

## 1. The order of the sources (steps `order-3` and `order-4`)

The narration follows the code, not the documentation. `ShellSettingsManager.EnsureConfigurationAsync`
(`src/OrchardCore/OrchardCore/Shell/ShellSettingsManager.cs:245-257`) adds the files of `App_Data`
(`ShellsConfigurationSources.cs:21-22`: `appsettings.json` and `appsettings.<Environment>.json`), then adds the
environment variable and command-line providers of the application again, so they win over those files. The
documentation (`Configuration/README.md:349-362`) lists them before `App_Data`, which would make `App_Data` win.

To confirm: run the site with the same key set in `App_Data/appsettings.json` and in an environment variable, with
different values (for example `OrchardCore:Media:MaxFileSize`, with the Media feature enabled, and a file larger than
the smaller limit), and check which limit applies. Do the same with a key in `OrchardCore:Default:...` (a tenant
section) against one at the root of `OrchardCore`, and against `App_Data/Sites/Default/appsettings.json`: the code
says the tenant section beats the root, and the tenant file beats both
(`ShellSettingsManager.cs:269-281`).

## 2. What the clip `04-list` shows

The two steps describe what the views draw, read from `Views/ShellSettingsEntry.*.SummaryAdmin.cshtml`,
`Views/ManageTenantActionButtons.cshtml` and `OrchardCore.Setup/Views/Setup/Index.cshtml`. Confirm on the running site:

- the `MyTenant` row of Multi-Tenancy, Tenants shows the badges `Sqlite`, `SaaS` and `Uninitialized`, and reads
  `MyTenant` with exactly that case (the name is the key of the section, taken from the name of the environment
  variable as the process receives it, which a shell may change);
- the `Default` row shows `Running`, and probably also a `Sqlite` badge, because setup saves the database provider
  of the tenant in its own file (`src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:160` and `:303`); it has no `SaaS` badge. The narration does
  not describe the `Default` row;
- the `Setup` button is in the row (the default `Buttons` actions layout) and opens `/mytenant`;
- the setup screen of `MyTenant` has no recipe picker and no database fields, and the Auto Setup of the Default tenant
  does not interfere (`AutoSetupMiddleware` only acts on tenants listed under `OrchardCore:AutoSetup:Tenants`);
- the time zone list of the setup screen has an entry whose value is `America/Los_Angeles` (the plan selects it by
  that value, read from `DefaultTimeZoneSelectListProvider.cs:17-18`), and the admin menu group `Multi-Tenancy` can be
  closed and opened by the toggle button as the plan assumes (`TheAdmin/Views/NavigationItem-admin.cshtml`);
- the selectors in `recording-plan.md` (`#left-nav #multitenancy button`, `#left-nav a[href$="/Admin/Tenants"]`,
  `.list-group-item:has-text("MyTenant")`, `a#btn-setup-MyTenant`, `#SubmitButton`) were read from the views of the
  admin theme and the Tenants module, not from a running page: check them with `Recorder probe`.

## 3. Pronunciation (the `spoken` map of `storyboard.json`)

None of the spoken spellings was tried with `scripts/pronounce.py`. A person must confirm each one, as the skill asks:
`IShellConfiguration`, `IConfiguration`, `appsettings.json`, `appsettings.Development.json`, `tenants.json`,
`ConfigurationSchema.json`, `App_Data`, `ORCHARD_APP_DATA`, `OrchardCore__Media__MaxFileSize` (spoken in full, which is
longer than its one written word), `OrchardCore_Media_Azure`, `OrchardCore_Tenants`, `MaxFileSize`, `DatabaseProvider`,
`ConnectionString`, `TablePrefix`, `TablePrefixPattern`, `RequireTablePrefix`, `SchemaPattern`, `MyTenant`, `MyService`,
`MyProvider`, `BackgroundService`, the bare word `appsettings` (step `mod-3`) and the bare word `OrchardCore`, used as
the name of the section (the last three come last in the map: `narrate.py` replaces the keys one after the other, in
the order of the map, so a short key placed before a longer one that contains it would corrupt it). Words left to the voice, to listen to: `SaaS`, `Sqlite`, `SQLite`, `MySQL`, `PostgreSQL`,
`IntelliSense`, `ASP.NET` (already in the shared list), `Uninitialized`.

## 4. Claims that rest on the documentation alone

These are behaviors of ASP.NET Core, Azure or Visual Studio, outside the Orchard Core source:

- user secrets are part of the application configuration in the Development environment (`order-1`); ASP.NET Core
  reads them only for a project that has a `UserSecretsId`, and `OrchardCore.Cms.Web.csproj:15` has one;
- Azure App Settings reach the site as environment variables, on Windows and on Linux, and a pipeline can transform
  `appsettings.json` with values from its variables (`env-3`);
- Visual Studio uses the `ConfigurationSchema.json` file of a package, through a `JsonSchemaSegment`, for IntelliSense
  and validation in `appsettings` files, and flags deprecated names (`mod-3`). The Orchard Core side is in the
  source: `src/OrchardCore.Build/ConfigurationSchema.targets`, `OrchardCore.Commons.targets:8-12`, and the
  `deprecated` entries of the schema files (for example `OrchardCore.Media/ConfigurationSchema.json:176`).

## 5. Left out on purpose

- Overriding options from code with `PostConfigure` in the host (`Configuration/README.md`, "`IOptions` Configuration").
  The documentation says it overrides the site settings, but this was not verified: `ResourceOptionsConfiguration`
  (`OrchardCore.Settings/ResourceOptionsConfiguration.cs`) copies the site settings into `ResourceOptions` in a
  `PostConfigure`, and whether the host's `PostConfigure` always runs after it depends on the order of the startup
  classes, which was not traced.
- Live tenant options with `IOptionsMonitor`, and the Azure and Database shells configuration providers: the closing
  card names them as covered by the documentation.

## 6. Settings read for all tenants, and the table prefix patterns

Both were read in the code, not run:

- step `env-2` says a tenant section wins over the shared value, except for settings the host reads for all tenants,
  such as `BackgroundService` (`src/OrchardCore/OrchardCore/Modules/Extensions/OrchardCoreBuilderExtensions.cs:108` reads it from the
  application configuration; the schema in `src/OrchardCore/OrchardCore/ConfigurationSchema.json:95` says "Read from the application
  configuration only, not per tenant"). To confirm: set `OrchardCore__Default__BackgroundService__PollingTime` and
  `OrchardCore__BackgroundService__PollingTime` and see that only the second one is used.
- step `db-2` limits `TablePrefixPattern` and `SchemaPattern` to tenants created in the admin or through the API
  (`AdminController.cs`, `TenantApiController.cs`, `TenantValidator.cs`). The Setup module never resolves them, so a
  tenant that exists only in the configuration and is set up on its own setup screen takes its prefix from the
  configuration or the form. To confirm: with `TablePrefixPattern` set, the Add Tenant form shows the generated prefix
  instead of an input, and the setup screen of a tenant pre-configured with a provider that has table prefixes (and
  without a connection string) shows an empty `Table Prefix` input.

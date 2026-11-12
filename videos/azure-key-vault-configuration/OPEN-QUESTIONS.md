# Open questions

What the narration and the slides state was read from the Orchard Core source at commit `92db4a83`
(`src/OrchardCore/OrchardCore.Configuration.KeyVault/`, `src/OrchardCore/OrchardCore/Shell/ShellSettingsManager.cs`) and from
`src/docs/reference/modules/KeyVault.Azure/README.md`. Nothing was run: no site was started and no vault was reached. The
points below are what a person must confirm, or what was left out of the video because it could not be verified.

## 1. A stand-in for the vault (why there is no clip)

- **Checked:** `AddOrchardCoreAzureKeyVault` ends with `builder.AddAzureKeyVault(keyVaultEndpointUri, tokenCredential,
  configOptions)` (`Extensions/AzureKeyVaultConfigurationExtension.cs:88`). `configOptions` sets only `Manager` and
  `ReloadInterval` (`:76-84`). The extension takes no client options.
- **Checked:** the Secrets documentation (`src/docs/reference/modules/Secrets/README.md`, "Local development with the Azure
  Key Vault Emulator") says an emulator on `localhost` needs `DisableChallengeResourceVerification` set to `true`, and
  an identity authority that points to the emulator. That page configures the Secrets store's own `SecretClient`, not
  this provider.
- **To confirm:** whether this provider can read from an emulator at all (a `VaultURI` on `localhost`, the emulator's
  placeholder credentials in the `AZURE_*` environment variables, `DefaultAzureCredential`), or only from a real vault
  with a real Azure account, which a recording must not show. If an emulator works, a clip could show a site whose admin
  path comes from a vault secret named with two hyphens, `OrchardCore--Admin--AdminUrlPrefix` (the setting is read from
  `OrchardCore:Admin:AdminUrlPrefix`, `src/OrchardCore.Modules/OrchardCore.Admin/Startup.cs:82`). The recording plan,
  the storyboard and `slides.py` would then gain a chapter. Until then the video stays slides only.

## 2. The package

- **Checked:** the library is the project `OrchardCore.Configuration.KeyVault`. The web project `OrchardCore.Cms.Web` and
  the CMS targets projects do not reference it (only `OrchardCore.Secrets.AzureKeyVault`, a different project, is
  referenced by `OrchardCore.Application.Cms.Core.Targets.csproj`), so a host has to reference it.
- **To confirm:** that it is published to NuGet under the id `OrchardCore.Configuration.KeyVault` (the id of a project in
  `src/OrchardCore` is its project name, as for `OrchardCore.Shells.Azure`, whose appsettings comment says "Add a
  reference to the OrchardCore.Shells.Azure NuGet package"). Step `host-1` and its slide use that id. The project file, unlike its siblings such as
  `OrchardCore.Shells.Azure`, sets no `Title`, `Description` or `PackageTags` (`OrchardCore.Configuration.KeyVault.csproj`), and
  nothing in the source sets `IsPackable`; the project does carry a `ConfigurationSchema.json`, which the build packs. Check
  the package list on nuget.org before recording.

## 3. Order of the sources (step `config-4`)

- **Narrated:** the provider is added at the point of the call, after the sources registered before it, so its values
  replace the same keys of the project's `appsettings.json` (the documentation says that later sources win).
- **Left out:** Orchard Core builds its own configuration from the application configuration, adds the `App_Data`
  `appsettings.json` files, and then adds the environment variable and command line providers of the application again
  (`ShellSettingsManager.cs:245-258`). By that code, an environment variable or a command line argument still wins over a
  vault value for the settings the modules read, and a value in `App_Data/appsettings.json` wins over a vault value. The
  page `src/docs/reference/modules/Configuration/README.md` ("Configuration Sources Order") lists the `App_Data` files
  last and does not mention this.
- **To confirm on a running site:** set the same key in the vault and in an environment variable, and read it from a
  module, to see which one wins. The step points to a separate video for the combination of the other sources.

## 4. Reloading

- **Narrated:** `ReloadInterval` is a number of seconds between polls of the vault, empty to disable reloading (the
  documentation and the schema).
- **Left out:** whether a changed value reaches a running tenant, or only applies after the tenant or the application
  restarts. The shell configuration is built from the application configuration (`ShellConfiguration.cs`) and exposes the
  reload token (`ShellConfiguration.cs:146`), but the search of `src/OrchardCore` found no code that rebuilds a tenant
  when it fires. The modules were not searched.
- **To confirm:** change a secret in a vault while a site runs with `ReloadInterval` set, and see what a module reads.

## 5. Access rights

- The narration says only that the identity "must be allowed to read the secrets of the vault". Neither the documentation
  nor the source says which Azure role or access policy is required (the provider comes from the
  `Azure.Extensions.AspNetCore.Configuration.Secrets` package, whose calls are not in the Orchard Core source).
- **To confirm:** the role or the permissions to name, from the Azure documentation, and whether a disabled secret is
  skipped. If the answer is added to a video, give its source.

## 6. The credential

- **Narrated:** the code defaults to `new DefaultAzureCredential(includeInteractiveCredentials: true)` (`:86`) and the
  documentation calls `DefaultAzureCredential` appropriate for most scenarios where the application runs in Azure. The
  video repeats the argument by name and does not say what the credential tries, since that is behavior of the Azure
  Identity library, not of the Orchard Core source.
- **To confirm:** the sources the Azure Identity version used by Orchard Core tries, and whether the sign-in from
  Visual Studio Code that the documentation describes (an extension named "Azure Account Extension", without a link) is
  still the way to sign in.

## 7. Statements taken from the documentation only

- "Azure Key Vault does not allow a colon or an underscore in the name of a secret" (step `names-1`): the first
  paragraph of the documentation page. The source only does the translation.
- The tenant layout of step `names-3`: `OrchardCore--MyTenant--...` is read as `OrchardCore:MyTenant:...`, which the
  source merges as the tenant's section (`ShellSettingsManager.cs:115,276`) and the Configuration documentation shows with
  `OrchardCore:MyTenant:` and `OrchardCore:Default:`. The key `OrchardCore:Media:Azure:ConnectionString` is the documented
  setting of the Media Azure feature.
- Step `why-1` ("often kept in an appsettings.json file or in an environment variable") is a general statement, not a
  claim about Orchard Core.
- The Generic Host example on slide `host-2` is the documentation's, which needs a `Startup` class of the host.

## 8. Spoken forms to confirm with `scripts/pronounce.py`

`storyboard.json` "spoken" asks the voice to say:

| Written | Spoken |
| --- | --- |
| `OrchardCore.Configuration.KeyVault` | Orchard Core dot Configuration dot Key Vault |
| `AddOrchardCoreAzureKeyVault` | Add Orchard Core Azure Key Vault |
| `DefaultAzureCredential` | Default Azure Credential |
| `TokenCredential` | Token Credential |
| `KeyVaultName` | Key Vault Name |
| `ReloadInterval` | Reload Interval |
| `VaultURI` | Vault U.R.I. |
| `appsettings.json` | app settings dot JSON |
| `Program.cs` | Program dot C.S. |
| `KeyVault` | Key Vault |
| `OrchardCore` | Orchard Core |

The entries are ordered from the longest to the shortest, since `narrate.py` replaces them one after the other. `Vault
U.R.I.`, `app settings dot JSON` and `Orchard Core dot Configuration dot Key Vault` are not in the worked example. The
narration also contains "Generic Host" and the name of the Azure Key Vault service: check that the voice says them
naturally. No audio was generated.

## 9. Which configuration the host-builder overloads read

- **Checked:** all three overloads call the same private method with a configuration to read `OrchardCore:KeyVault:Azure`
  from. The `ConfigurationManager` overload passes the manager itself, so it sees the providers already added to
  `builder.Configuration` (`AzureKeyVaultConfigurationExtension.cs:63`). The `IHostBuilder` and `IWebHostBuilder` overloads pass
  `context.Configuration` inside `ConfigureAppConfiguration` (`:29`, `:47`), and a source comment says it holds the providers
  already added.
- **Left out:** the narration says the settings come from `appsettings.json` or environment variables only for the
  configuration of the builder. For the Generic Host, what `context.Configuration` holds at that point (the host configuration or
  the application configuration so far) is behavior of the .NET host, not of this source.
- **To confirm:** with `Host.CreateDefaultBuilder(args).AddOrchardCoreAzureKeyVault()`, put `KeyVaultName` in `appsettings.json`
  and see whether it is found; if not, say where to put it for that overload.

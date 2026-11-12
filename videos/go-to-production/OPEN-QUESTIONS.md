# Open questions: Go to Production

What was checked in the Orchard Core source (commit `92db4a83`) and the documentation, and what a person must
confirm on a running site. The narration states only what the source and the documentation show; nothing below is
narrated as a fact. File references are relative to the root of `OrchardCMS/OrchardCore`.

## 1. The HTTPS page is recorded over HTTP

- **Checked.** When `Request.IsHttps` is false, `HttpsSettingsDisplayDriver` shows `For safety, changing HTTPS-only
  settings over HTTP has been prevented.` and sets `IsHttpsRequest` to false
  (`src/OrchardCore.Modules/OrchardCore.Https/Drivers/HttpsSettingsDisplayDriver.cs:54-62`), and `UpdateAsync` saves nothing
  over HTTP (`:85-87`). The view puts `asp-is-disabled` on the two check boxes, the SSL Port input and the HSTS mode
  `select` (`Views/HttpsSettings.Edit.cshtml:6,16,30,40`), but `InputIsDisabledTagHelper` targets only `input`
  elements with `asp-for`
  (`src/OrchardCore/OrchardCore.DisplayManagement/TagHelpers/InputIsDisabledTagHelper.cs:5`), so by the source the
  three inputs are disabled and the HSTS mode list is not. The shared recorder opens its browser context without `IgnoreHTTPSErrors`
  (`scripts/recorder/Scene.cs`), and `references/recording.md` says never to trust a certificate machine-wide.
- **Decision taken.** `04-https` is a read-only tour of the page opened over HTTP, and the narration says so (chapter
  04, steps `https-1` and `https-2`). The values to set are on the recipe slide. The narration names only what the source
  shows as disabled (the two check boxes and the SSL Port field) and says the page does not save changes made over HTTP.
- **Not narrated.** That the HSTS mode list is enabled over HTTP. Saving the page over HTTP also reaches
  `IndexPost`, which calls `UpdateSiteSettingsAsync` and adds `Site settings updated successfully.` when the model is
  valid (`src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:83-119`), even though the HTTPS
  driver changed nothing.
- **To confirm.** Run the `Recorder probe /Admin/Settings/Https` command of the plan and check that the `disabled`
  flag is true for the two check boxes and the SSL Port field and false for the HSTS mode list. A person may also save
  the page over HTTP once on a scratch site and note whether the success message appears and the values stay. If a take
  that saves the page over HTTPS is wanted, someone decides how the recorder's browser
  accepts the development certificate for that process only. The narration would then describe the saved result.

## 2. HTTPS settings behind a proxy that ends TLS

- **Checked.** The HTTPS page tests `Request.IsHttps`. The Reverse Proxy feature calls `UseForwardedHeaders()` early
  (`OrchardCore.ReverseProxy/Startup.cs:23`, order `-300` in `OrchardCoreConstants.cs:56`), and the documentation says
  the `X-Forwarded-Proto` box forwards "the protocol (HTTP or HTTPS) used by the client".
- **Not narrated.** That a proxy which ends TLS makes the site see an HTTPS request once `X-Forwarded-Proto` is on
  (and the proxy is trusted) depends on how ASP.NET Core processes forwarded headers, which the source here does not show.
- **To confirm.** Put the site behind a local proxy that terminates TLS, open `/Admin/Settings/Https` through it with
  the `X-Forwarded-Proto` box off and on, and note whether the fields become enabled. Also note whether the
  `Current request value` lines of the Reverse Proxy page then show the forwarded address, protocol and host, as step
  `proxy-3` suggests when it says to open the page through your proxy after you deploy.

## 3. Which source wins: environment variables or `App_Data/appsettings.json`

- **Checked.** The documentation lists environment variables and command-line arguments before the `App_Data` files,
  "last wins" (`src/docs/reference/modules/Configuration/README.md`, "Configuration Sources Order"). The code does it
  the other way for the global files: `ShellSettingsManager.EnsureConfigurationAsync` adds the application
  configuration, then the `App_Data` sources, then adds the environment-variable and command-line providers again
  (`src/OrchardCore/OrchardCore/Shell/ShellSettingsManager.cs:245-257`), so those two win over
  `App_Data/appsettings.json`. `ShellsConfigurationSources` also reads `App_Data/appsettings.{Environment}.json`, which
  the list omits (`src/OrchardCore/OrchardCore/Shell/Configuration/ShellsConfigurationSources.cs:21-22`). The tenant's own
  `appsettings.json` is added last. This is reported as a documentation issue.
- **Not narrated.** The video does not state an order of the sources.
- **To confirm.** Set the same key in `App_Data/appsettings.json` and as an environment variable, with different
  values, and see which one the site uses. The video on configuration sources should settle the wording.

## 4. Known proxies and known networks left empty

- **Checked.** `ForwardedHeadersOptionsConfiguration` clears both lists and fills them only from the settings
  (`OrchardCore.ReverseProxy/Configuration/ForwardedHeadersOptionsConfiguration.cs:21-22`). The documentation says to
  "always configure KnownProxies and KnownNetworks" in production and warns about IP spoofing and header injection.
- **Not narrated.** What ASP.NET Core does with empty lists. The narration repeats the documentation's warning only.
- **To confirm.** With the X-Forwarded-For box on and both lists empty, send a request with a forged `X-Forwarded-For`
  header from a machine that is not a proxy, and read `Current request value` on the Reverse Proxy page.

## 5. Data protection keys on disk

- **Checked.** The default key ring is `App_Data/Sites/<tenant>/DataProtection-Keys`, with the tenant name as the
  application name, and `XmlEncryptor ??= new NullXmlEncryptor()`
  (`src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:566-576`).
- **Not narrated.** Whether the key files are readable in clear text. The narration says only to back up the folder,
  as the guide does ("Locally stored keys may be lost, invalidating existing cookies and tokens").
- **To confirm.** Open a key file in the folder and check whether the key material is in clear text.

## 6. A shared key store that is enabled but not configured

- **Checked.** Without a connection string, `OrchardCore.DataProtection.Azure` logs a critical error and registers no
  key store (`src/OrchardCore.Modules/OrchardCore.DataProtection.Azure/Startup.cs:34,67-70`). Without a valid `Redis`
  section, the Redis features stay off and the documentation says the tenant uses the default implementations
  (`OrchardCore.Redis/Startup.cs:34-44`). The Redis key setup logs its persistence warning whenever it cannot confirm
  persistence, including when `AllowAdmin` is false (`OrchardCore.Redis/Options/RedisKeyManagementOptionsSetup.cs:38,91-95`).
- **Narrated.** Steps `keys-5` and `keys-6`: the warning, the critical error, and that the site keeps the keys in `App_Data`.
- **To confirm.** Enable the Azure feature with no connection string and check the log line and that key files appear in
  `DataProtection-Keys`. Enable the Redis feature without `Configuration` and check the same.

## 7. Selectors

The selectors of `recording-plan.md` come from the views, not from a rendered page: the Settings and Security group
buttons of the admin menu, the search filter of the Features page, the `GetByLabel` texts, the `name$=` suffixes, and
the classes of the notices, and the `aria-expanded` state of the Settings and Security buttons (collapsed in a fresh
browser context according to `sidebar.ts`). The probe commands at the end of the plan check them. Whether the Features search matches
`reverse` and `https` against the feature names is also unconfirmed.

## 8. Spellings of the `spoken` map

Each spelling of the storyboard's `spoken` map must be confirmed by a person with `scripts/pronounce.py` before the
video is narrated (none has been tried): `ConfigureReverseProxySettings` as "Configure Reverse Proxy Settings"; `KnownProxies` as "Known Proxies"; `KnownNetworks` as "Known Networks"; `ReverseProxy` as "Reverse Proxy"; `RequireHttpsPermanent` as "Require HTTPS Permanent"; `RequireHttps` as "Require HTTPS"; `StrictTransportSecurityMode` as "Strict Transport Security Mode"; `FromConfiguration` as "From Configuration"; `HttpsSettings` as "HTTPS Settings"; `SslPort` as "S.S.L. Port"; `DataProtectionKeys.xml` as "Data Protection Keys dot X.M.L."; `DataProtection-Keys` as "Data Protection Keys"; `DataProtection` as "Data Protection"; `ConnectionString` as "Connection String"; `ContainerName` as "Container Name"; `BlobName` as "Blob Name"; `ShellSettings` as "Shell Settings"; `DatabaseProvider` as "Database Provider"; `AllowedHosts` as "Allowed Hosts"; `ASPNETCORE_ENVIRONMENT` as "A.S.P. .NET Core environment"; `ORCHARD_APP_DATA` as "Orchard app data"; `App_Data` as "App Data"; `appsettings.Production.json` as "app settings dot Production dot JSON"; `appsettings.json` as "app settings dot JSON"; `Program.cs` as "Program dot C.S."; `HSTS` as "H.S.T.S."; `CIDR` as "C.I.D.R."; `dataprotection` as "data protection"; `AOF` as "A.O.F."; `RDB` as "R.D.B.".

Left as written, and to listen to: `X-Forwarded-For`, `X-Forwarded-Proto`, `X-Forwarded-Host`, `SQLite`, `Lucene`,
`Redis`, `CORS`, `SMTP`, `HTTPS`, `Liquid`.

## 9. Length

The narration has 1,028 words, 6:51 at 150 words a minute, against the 7 minutes of the plan. The clips are 8 steps of
about 14 seconds each; if a take needs more room for the tenant restart in `proxy-4`, the step can grow by a sentence.

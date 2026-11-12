# Open questions: Logging

The narration states only what the Orchard Core source (the two libraries, `src/OrchardCore.Cms.Web`, the CMS project
template) and the documentation say. The points below were read but not run, or depend on NLog, Serilog or the voice,
and need a person to confirm them before the video is recorded. Nothing here is narrated as a fact beyond what the
source says.

## Pronunciation (a person confirms each spelling with `scripts/pronounce.py`)

The `spoken` map of `storyboard.json` holds these candidates, longest first (the replacement is a plain, ordered text
replacement):

- `NLog` as "N Log", also inside `OrchardCore.Logging.NLog`, `NLog.config` and `UseNLogHost`.
- `YesSql` as "Yes Sequel".
- `occms` as "O.C. C.M.S.", and `dotnet` as "dot net".
- `Program.cs` as "Program dot C.S.", `appsettings.json` as "app settings dot JSON", `NLog.config` as "N Log dot config".
- `App_Data` as "App Data", `ORCHARD_APP_DATA` as "ORCHARD APP DATA", `configDir` as "config dir".
- `UseSerilogTenantNameLogging`, `UseSerilogWeb`, `UseSerilog`, `UseNLogHost`, `UseOrchardCore`, `Enrich.FromLogContext`,
  `FromLogContext`, `TenantName` and `Microsoft.Hosting.Lifetime`, each spelled out word by word.
- `Serilog` and `orchard-tenant-name` are left as written in the spoken text (the second one is read as "orchard tenant name").

## What the narration relies on, read from the source only

1. **The rules of `NLog.config`.** The narration repeats the comments of the file ("The YesSql events go to the file
   targets", "The hosting lifetime events go to the console and file targets", "All default and above go to the file
   target"). What `finalMinLevel="Off"` does to the rules that follow is NLog's behavior, and is not narrated. Confirm on a
   running site that a YesSql event is written once, and that a lifetime event reaches both the file and the console.
2. **The file name of NLog.** The narration says "a file named orchard-log with the date" (`NLog.config:16`). Confirm what
   `${shortdate}` renders, and so the exact file name in `App_Data/logs`.
3. **The Serilog levels.** The narration reads the configuration literally (a minimum level, an override, and a
   `restrictedToMinimumLevel` for each sink). How a sink's level combines with the minimum level is Serilog's behavior.
   The advice of `serilog-6` (lower the minimum level and the level of the sink) holds whichever way the two levels
   combine, so it is narrated; the exact combination is not. Confirm on a running site, with the template's settings,
   what the console and the file receive (an Information event of `Microsoft.Hosting.Lifetime`, a Warning event of
   another logger, an Information event of another logger).
4. **The Serilog file.** `rollingInterval` is read as "a file for each day". Confirm the file name in `App_Data/logs`, and that
   replacing `App_Data` with `%ORCHARD_APP_DATA%` (the template's own comment, `appsettings.json:32`) moves the file to the
   folder of the environment variable on Windows and on Linux.
5. **`{MachineName}`.** The template's File sink output template contains `{MachineName}` (`appsettings.json:35`), but no
   machine name enricher is configured in the template's `Program.cs`, and the documentation page does not have it. The
   video shows the Console sink's template on the slide, and the File sink's output template is replaced by `// ...`.
   Confirm what a line of the file shows.
6. **The `Default` override.** The template's `MinimumLevel:Override` has a key named `Default` next to
   `Microsoft.Hosting.Lifetime` (`appsettings.json:16`). The slide trims it. Confirm whether it has an effect.
7. **`--logger` values.** The help text of the documentation lists `nlog`, `serilog` and `none` in lowercase, and
   `template.json` lists `NLog`, `Serilog` and `None`. The narration says `nlog, serilog or none`. Confirm the output of
   `dotnet new occms --help` and that the option accepts either case.
8. **The admin.** The narration says the packages are libraries with nothing to enable in the admin. Confirm that
   Configuration, Features lists no logging feature on a site made with the CMS web project.
9. **Lines outside a request.** `None` is what the NLog renderer prints when neither the request nor the current scope
   has a tenant (`TenantLayoutRenderer.cs:26`). The narration does not say which log lines are in that case (start-up
   lines, for example). Confirm on a running site before saying so in a later version of the video.
10. **A tenant other than the default.** Confirm, with a second tenant created in the admin, that its requests are logged with
    its own name by both libraries (the code reads `ShellContext.Settings.Name`).
11. **A project created from the template.** Confirm that a project created with each of the three `--logger` values builds, runs and,
    for NLog and Serilog, writes a file in `App_Data/logs`. The narration says nothing about the content of that file.
12. **OpenTelemetry and Serilog.** `compare-2` only says that a separate video covers OpenTelemetry. What was checked:
    `OrchardCore.OpenTelemetry` adds a logging provider (`OpenTelemetryOrchardCoreBuilderExtensions.cs:93-107`, an
    `OpenTelemetryLoggerProvider` filter), and its documentation says the provider "is added next to the other logging
    providers, such as NLog or Serilog". The template calls `UseSerilog` with its default arguments, and Serilog's host
    integration may not pass the events to other logging providers unless it is asked to. Confirm that a site made with
    the template's Serilog call exports its logs through OpenTelemetry, before any video says that it does.
13. **The Serilog path.** The template's File sink path is the relative `App_Data/logs/orchard-log.txt`; Serilog resolves it against the process's current directory, not the content root. `compare-2` says only "by default the files go to the logs folder of App_Data". Confirm where the file lands when the site is started from another directory.

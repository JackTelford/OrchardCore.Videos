# Recording plan: Logging

The video is made only of slides and cards: the subject is two libraries without an admin screen, and their code, configuration
files and command lines are shown on slides (a terminal is never captured). **There is no clip to record**: no site, no
browser, no recorder, no `state.json`, no `recorder/` folder. Everything is drawn by `slides.py` from the storyboard.

References below are to the Orchard Core source at the commit named in `catalog/SOURCE.md` (paths under `src/`).

## The site it needs

None. Do not start Orchard Core, do not run Auto Setup, do not sign in. There is no tenant, time zone, feature, setting,
external service or seed data to prepare.

## Order, dependencies and snapshots

No clip changes a site, so there is nothing to restore between takes and no order to respect. The steps play in the
storyboard's order: chapters 00 to 05, then 99.

## Making the video

From the repository root, with `pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`:

```bash
python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/logging
python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/logging
python tools/youtube/youtube.py chapters logging --build videos/logging/build
```

The voice reads `spoken` in `storyboard.json`. Before narrating, a person confirms each spelling with
`scripts/pronounce.py` (the candidates are listed in `OPEN-QUESTIONS.md`). Slide text is never spoken: only the
narration is.

## The steps

Each step below names the slide it plays over. The slide is drawn from the narration's step id by `slides.py`: all of
its cards and panels are on screen from the start of the step to its end.

### 00 Opening card

- `intro`: the opening card, with the title "Logging in Orchard Core" and the subtitle "Configure NLog and Serilog".

### 01 Logs and tenants (slides)

- `why-1`: heading "One application, many tenants". Three cards: "One deployment", "Shared log files", "The tenant name". Source for the claim about tenants: `src/docs/README.md` ("multi-tenant", "database, content, theme and user isolation"); one file path for every tenant: `src/OrchardCore.Cms.Web/NLog.config:16`, `appsettings.json` of the template (`Serilog:WriteTo` File `path`).
- `why-2`: heading "Two logging packages". Three cards: `OrchardCore.Logging.NLog`, `OrchardCore.Logging.Serilog`, "Libraries, not modules". Source: `src/OrchardCore/OrchardCore.Logging.NLog/OrchardCore.Logging.NLog.csproj` and `src/OrchardCore/OrchardCore.Logging.Serilog/OrchardCore.Logging.Serilog.csproj` (packages; neither folder has a manifest, so neither is a feature of the admin).

### 02 Choose a logger (slides)

- `choose-1`: heading "Pick a logger with the project template". A "Command line" panel with the three commands `dotnet new occms --logger nlog`, `--logger serilog`, `--logger none`, and three cards: "nlog" (the default), "serilog", "none". Source: `src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Web/.template.config.src/template.json:11` (`occms`), `:29-47` (the `Logger` choices and the `NLog` default), `.template.config.src/dotnetcli.host.json:8-11` (`--logger`, `-lo`), and `src/docs/getting-started/templates/README.md` (the `--logger` help text, lowercase choices).
- `choose-2`: heading "What the template adds". Three cards: NLog (package, `UseNLogHost`, `NLog.config`, Logging section), Serilog (package, `UseSerilog`, `UseSerilogTenantNameLogging`, Serilog section), None (nothing). Source: the template's `OrchardCore.Templates.Cms.Web.csproj:25-30`, `Program.cs:1-20` and `:43-47`, `appsettings.json:2-41`, and `template.json:65-75` (`NLog.config` excluded unless NLog).
- `choose-3`: heading "Add a logger to an existing project". A "Command line" panel with `dotnet add package OrchardCore.Logging.NLog` and `dotnet add package OrchardCore.Logging.Serilog`, and two cards: "Then copy the calls in Program.cs" and "And copy the configuration files". The command carries no version on purpose.

### 03 NLog (slides)

- `nlog-1`: heading "UseNLogHost in Program.cs". A `Program.cs` panel (`using OrchardCore.Logging;`, `builder.Host.UseNLogHost();`, `AddOrchardCms()`, `// ...`) and two cards: "Turns on NLog / for the host", "Registers / orchard-tenant-name". Source: `src/OrchardCore.Cms.Web/Program.cs:1-5`, `src/OrchardCore/OrchardCore.Logging.NLog/HostBuilderExtensions.cs:9-15`, `TenantLayoutRenderer.cs:16`.
- `nlog-2`: heading "The configDir variable". A panel with the `ConfigureAppConfiguration` body of `UseNLogHost` (the early return trimmed with `// ...`), and two cards: "ORCHARD_APP_DATA / The environment variable", "App_Data / The folder under the content root". Source: `HostBuilderExtensions.cs:16-32`, `src/OrchardCore/OrchardCore.Abstractions/ShellOptionConstants.cs:5-6`.
- `nlog-3`: heading "Targets in NLog.config". An `NLog.config` panel with the `targets` element (the `layout` value shown as `...`), and two cards: "File target / The logs folder under configDir", "Console target / The console". Source: `src/OrchardCore.Cms.Web/NLog.config:13-23` (identical to the template's `NLog.config`).
- `nlog-4`: heading "The layout of a line". Four cards in the order of the layout: "Time and tenant", "Request and event", "Logger and level", "Message". Source: the `layout` attribute, `NLog.config:17`.
- `nlog-5`: heading "The orchard-tenant-name renderer". A `TenantLayoutRenderer.cs` panel with `Append`, and three cards: "1. Request", "2. Scope", "3. Otherwise / None". Source: `src/OrchardCore/OrchardCore.Logging.NLog/TenantLayoutRenderer.cs:18-27`.
- `nlog-6`: heading "Three rules". An `NLog.config` panel with the `rules` element, and three cards: "YesSql / To the file", "Hosting lifetime / To the file and the console", "Default and above / To the file". Source: `NLog.config:29-38` (the card texts follow the comments of the rules).
- `nlog-7`: heading "Levels come from appsettings.json". The `defaultLevel` variable of `NLog.config` (the two other variables shown as `<!-- ... -->`), the `Logging` section of `appsettings.json`, and two cards: "Lower Default / to write more events", "Development / The Orchard Core web project uses Debug". Source: `NLog.config:25-27`, `src/OrchardCore.Cms.Web/appsettings.json:2-8`, `src/OrchardCore.Cms.Web/appsettings.Development.json:2-8` (`Default` is `Debug`).

### 04 Serilog (slides)

- `serilog-1`: heading "UseSerilog in Program.cs". A "Command line" panel with `dotnet add package OrchardCore.Logging.Serilog` above a `Program.cs` panel with the `UseSerilog` call (`ReadFrom.Configuration`, `Enrich.FromLogContext`). Source: the template's `Program.cs:4-19`, `src/docs/reference/modules/Logging.Serilog/README.md`.
- `serilog-2`: heading "UseSerilogTenantNameLogging". A `Program.cs` panel with `app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());`, and two cards: "Placed after / the tenant is resolved", "It adds / the tenant name middleware". Source: the template's `Program.cs:41-47`, `src/OrchardCore/OrchardCore/Modules/Extensions/ApplicationBuilderExtensions.cs:31-35` (the callback runs after `ModularTenantContainerMiddleware`, which sets the `ShellContextFeature` at `ModularTenantContainerMiddleware.cs:51`, and before `ModularTenantRouterMiddleware`), `src/OrchardCore/OrchardCore.Logging.Serilog/ApplicationBuilderExtensions.cs:7-10`.
- `serilog-3`: heading "The tenant name middleware". A panel with `Invoke` of the middleware, and three cards: "Request or scope / The tenant name, or None", "TenantName / Pushed as a log property", "The request / Until the request ends". Source: `src/OrchardCore/OrchardCore.Logging.Serilog/SerilogTenantNameLoggingMiddleware.cs:17-25`, and for the NLog contrast `TenantLayoutRenderer.cs:18-27`.
- `serilog-4`: heading "UseSerilogWeb". A `Program.cs` panel with `using OrchardCore.Logging;`, `builder.Host.UseSerilogWeb();` and the `UseOrchardCore` call, and two cards: "Replaces / the UseSerilog call", "Keeps / the call in UseOrchardCore". Source: `src/OrchardCore/OrchardCore.Logging.Serilog/WebHostBuilderExtensions.cs:8-15` (the same lambda as the template's `UseSerilog` call).
- `serilog-5`: heading "The Serilog section". Two panels: `MinimumLevel` with its `Default` and the `Microsoft.Hosting.Lifetime` override (the other key of `Override` shown as `// ...`), and `WriteTo` with the `Console` and `File` sinks (their `Args` shown as `// ...`). Source: the template's `appsettings.json:12-40`.
- `serilog-6`: heading "The Console sink". A panel with the Console sink: its `outputTemplate` (with `{TenantName}`) and `restrictedToMinimumLevel` lines are bright, the other lines dimmed; two cards: "{TenantName} / Pushed by the middleware", "Information / The level of this sink". Source: the template's `appsettings.json:22-27`.
- `serilog-7`: heading "The File sink". A panel with the File sink: the template's comment about `%ORCHARD_APP_DATA%`, `path`, `rollingInterval`, `restrictedToMinimumLevel`, and two cards: "rollingInterval / A file for each day", "restrictedToMinimumLevel / Warning". Source: the template's `appsettings.json:29-38` (the `outputTemplate` line of the sink is shown as `// ...`).

### 05 Comparing the two (slides)

- `compare-1`: heading "NLog and Serilog side by side". Four cards: NLog configuration, NLog tenant name, Serilog configuration, Serilog tenant name. Source: the slides above.
- `compare-2`: heading "What both have in common". Three cards: "The tenant name" (part of the output of the template), "The logs folder" (App_Data/logs by default) and "OpenTelemetry" (card text "Covered by a separate video."). Source: the template's output templates and layout (`appsettings.json:25` and `:35`, `NLog.config:17`), the logs folder as in `nlog-3` and `serilog-7`, and `src/docs/reference/modules/OpenTelemetry/README.md` as the page that other video backs. The narration does not say that OpenTelemetry receives the logs of either library (see `OPEN-QUESTIONS.md`).

### 99 Closing card

- `outro`: the closing card, with the subtitle "docs.orchardcore.net".

## Checks for private data

Nothing is captured from a site, so no real name, email, address or token can appear. The slides hold only identifiers
of the Orchard Core source and invented nothing else: no tenant name, no sample log line and no domain other than
`docs.orchardcore.net`. After assembling, read a few frames of each chapter and check that no code panel is cut at its
right edge or at the bottom.

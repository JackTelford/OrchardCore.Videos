# Recording plan: OpenTelemetry

This plan is for the person who records the video. It needs no decision: every value, order and action is here. The
words "confirm" and "OPEN-QUESTIONS" mark what comes from the Aspire dashboard, which is not part of the Orchard Core
source: read `OPEN-QUESTIONS.md` first, and confirm those points with `Recorder probe` before the first take.

In this plan a selector stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. File and
line references are in the `src` folder of the Orchard Core checkout (the commit the documentation page describes).

## What is different about this video

- **No sign-in.** Every clip opens a public page of the site or the Aspire dashboard. Do not run `Recorder setup`, and
  do not create a `state.json`: the scenes start without one.
- **The site is provisioned by Auto Setup** (the Blog recipe), so no setup form is recorded. The Setup screen is not
  part of the story.
- **Two origins.** The clips of the site use `VIDEO_BASE_URL=http://localhost:5010`; the clips of the dashboard use
  `VIDEO_BASE_URL=http://localhost:18888`. The scene methods `OpenAsync` and `NavigateAsync` prepend that base URL
  to a path, so every path in this plan is relative to the origin of its clip.
- **The seed produces part of the story.** The traces of the recipe that sets a tenant up exist only if the site is
  provisioned while the dashboard is already receiving. A full take therefore starts from an empty `App_Data` folder
  and a new dashboard container, never from a copy of `App_Data`.

## The site it needs

### Software

- A checkout of `OrchardCMS/OrchardCore` (the same commit as the documentation page), the .NET SDK its `global.json`
  asks for, and Docker.
- The application is published to a neutral folder, so that the path the application logs ("Content root path") holds
  no personal name:

  ```bash
  dotnet publish src/OrchardCore.Cms.Web -c Release -o C:/ocvideo/web
  ```

### The external service and its local stand-in

The telemetry goes to the Aspire dashboard, which runs in a container. The image, the ports and the anonymous-access
variable come from the Aspire documentation (https://aspire.dev/dashboard/standalone/), not from Orchard Core: confirm
them (OPEN-QUESTIONS 1).

```bash
docker run --rm -d -p 18888:18888 -p 4317:18889 --name aspire-dashboard \
  -e ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true mcr.microsoft.com/aspire/dashboard:latest
```

The dashboard is at `http://localhost:18888` and receives OTLP over gRPC at `http://localhost:4317`. Stop it by its name
(`docker stop aspire-dashboard`), never by image. Its data is kept in memory: stopping the container clears it.

### Environment of the application

Set these in the shell that starts the application, and nowhere else (not in a file under git). `<password>` is the
password chosen outside git for the recording (`VIDEO_PASSWORD`); never write a real one here.

| Variable | Value | Why |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `appsettings.Development.json` sets the default log level to `Debug` (line 4), so the structured logs hold records; the production file sets `Warning`. |
| `ASPNETCORE_URLS` | `http://localhost:5010` | The site of the clips. |
| `ORCHARD_APP_DATA` | `C:/ocvideo/appdata` | An empty folder outside both repositories (`OrchardCore.Abstractions/ShellOptionConstants.cs:5`). |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `http://localhost:4317` | The endpoint that makes `HasOtlpEndpoint()` true, so `OrchardCore.Cms.Web/Program.cs:14-16` calls `AddOpenTelemetry()`. |
| `OTEL_METRIC_EXPORT_INTERVAL` | `5000` | Metrics are exported every 5 seconds instead of every 60 (`OpenTelemetryOrchardCoreBuilderExtensions.cs:141`). |
| `OTEL_RESOURCE_ATTRIBUTES` | `host.name=video-host` | Replaces the machine name in the resource attributes; the environment detector is added last (`OpenTelemetryOrchardCoreBuilderExtensions.cs:56`). |
| `OrchardCore__OpenTelemetry__ServiceInstanceId` | `web-1` | The instance ID shown on the slide; otherwise it is the machine name and the process ID (`OrchardCoreResourceDetector.cs:30`). |
| `OrchardCore__OpenTelemetry__Tracing__SlowShapeThreshold` | `00:00:00` | Every shape gets a `Render shape` span, which `traces-5` shows (`OrchardCoreOpenTelemetryOptions.cs:79`). |
| `HTTP_PROXY`, `HTTPS_PROXY` | a dead address, such as `http://127.0.0.1:9` | The site's own outbound HTTP calls, if any, fail instead of reaching a real service. |
| `NO_PROXY` | `localhost,127.0.0.1` | The OTLP export to the dashboard and the requests to the site are not sent to the dead proxy. |

Auto Setup, with the two tenants (`AutoSetup/Startup.cs:26`, `AutoSetup/Options/TenantSetupOptions.cs`; the variable
names follow the Auto Setup documentation):

| Variable | Default tenant | Marketing tenant |
|---|---|---|
| `OrchardCore__AutoSetup__Tenants__{n}__ShellName` | `Default` | `Marketing` |
| `...__SiteName` | `Orchard Core` | `Marketing` |
| `...__SiteTimeZone` | `America/Los_Angeles` | `America/Los_Angeles` |
| `...__AdminUsername` | `admin` | `admin` |
| `...__AdminEmail` | `admin@example.com` | `admin@example.com` |
| `...__AdminPassword` | `<password>` | `<password>` |
| `...__DatabaseProvider` | `Sqlite` | `Sqlite` |
| `...__RecipeName` | `Blog` | `Blog` |
| `...__RequestUrlPrefix` | (not set) | `marketing` |

`{n}` is `0` for the Default tenant and `1` for the Marketing tenant. The tenant name `Marketing` is the only name that
appears in the telemetry as a tenant, together with `Default`. No other feature has to be enabled: OpenTelemetry is not
a tenant feature, and the Blog recipe is all the pages need.

### The seed (outside the capture)

Run these steps in order, from a shell that holds the variables above. The data is the sample content of the Blog
recipe and the names in the tables above; nothing else is created.

1. Stop and start the dashboard container (the command above), so that it holds no telemetry. Wait until
   `http://localhost:18888` answers.
2. Delete `C:/ocvideo/appdata` and create it empty.
3. Start the application: `cd C:/ocvideo/web && dotnet OrchardCore.Cms.Web.dll`. Note its process ID to stop it by that ID.
4. Set up the Default tenant: request `http://localhost:5010/` until it answers 200 after the redirect that follows the
   setup (`AutoSetupMiddleware.cs:145`), for example
   `curl --noproxy "*" -sL -o /dev/null -w "%{http_code}\n" http://localhost:5010/`, retried every 5 seconds for at
   most 3 minutes. This request executes the Blog recipe of the Default tenant. The other tenant is created once the
   Default tenant is set up (`AutoSetupMiddleware.cs:140`).
5. Set up the Marketing tenant the same way with `http://localhost:5010/marketing`. This request executes the Blog
   recipe of the Marketing tenant: it is the trace `traces-4` opens.
6. Send traffic to both tenants: six times in a row `curl --noproxy "*" -s -o /dev/null http://localhost:5010/` and
   `curl --noproxy "*" -s -o /dev/null http://localhost:5010/marketing`.
7. Wait 20 seconds, then open `http://localhost:18888` and check: the traces page lists traces, the metrics page lists
   the resource, and the structured logs page lists records (see "Checks before the first take").

To record again from the start, stop the application by its process ID, stop the dashboard container, and repeat the
seed from step 1.

### Checks before the first take

- The dashboard shows one resource, with the service name `OrchardCore.Cms.Web` and the instance ID `web-1`; its
  properties hold `host.name` = `video-host`, and no machine name.
- A trace with spans named `Execute recipe Blog` and `Execute recipe step ...` exists for each tenant.
- `orchardcore.tenant.name` has the values `Default` and `Marketing` in the traces, in the structured logs and on
  `http.server.request.duration`.
- The page requests of both tenants have `Render shape ...` spans.

## Order of the clips

Record the clips in this order, each after the seed and the checks, in one session:

1. `03-browse` (site, `VIDEO_BASE_URL=http://localhost:5010`). It sends two more page requests, so that the newest
   traces are the ones the viewer has just seen.
2. `04-traces` (dashboard, `VIDEO_BASE_URL=http://localhost:18888`).
3. `05-metrics` (dashboard).
4. `06-logs` (dashboard).

No clip changes the site: they only read public pages and the dashboard. `03-browse` adds telemetry to the dashboard,
which is harmless for a new take of any clip. There is no `App_Data` snapshot to restore. A clip can be recorded
again on its own until the dashboard container or the application is restarted; after a restart, repeat the seed.

Set the variables of the recorder in each run:
`VIDEO_ROOT=<repo>/videos/opentelemetry VIDEO_BASE_URL=<the origin of the clip>`, and `MSYS_NO_PATHCONV=1` in Git Bash.
The proxy variables above are set in the shell that starts the application only, never in the shell of the recorder.

## Opening a dashboard page

`OpenAsync` waits for the network to be idle, and the dashboard keeps a live connection open: if `OpenAsync` times out,
open the page with `scene.Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.DOMContentLoaded })` before
`StartCaptureAsync()`, and wait for the table: `scene.Page.Locator("[role=row]").Nth(1).WaitForAsync()`. During a
capture, navigate between dashboard pages with `scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(url))`, or by
clicking a link with `ClickAndWaitForNavigationAsync`. Dismiss any banner of the dashboard before `StartCaptureAsync()`.
The page names, the labels and the selectors of the dashboard in this plan are the ones to confirm (OPEN-QUESTIONS 1).

## Clip `03-browse`: two tenants (site)

Starts on: `/marketing` (opened with `OpenAsync` before `StartCaptureAsync()`); the page is loaded and shows the site
name `Marketing` in the navigation bar. Both steps use `a.navbar-brand`, the link that shows the site name
(`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:27`).

- `browse-1`: `NavigateAsync("/")`, then `HighlightAsync(scene.Page.Locator("a.navbar-brand"), "Default tenant")`.
  Visible at the end of the narration: the home page of the Default tenant, with the site name `Orchard Core` ringed
  and captioned `Default tenant`. Label from `Layout.liquid:27` (`Site.SiteName`, here `Orchard Core`).
- `browse-2`: `ClearHighlightsAsync()`, `NavigateAsync("/marketing")`, then
  `HighlightAsync(scene.Page.Locator("a.navbar-brand"), "Marketing tenant")`. Visible at the end: the home page of the
  Marketing tenant, with the site name `Marketing` ringed and captioned `Marketing tenant`. Then `ClearHighlightsAsync()`.

## Clip `04-traces`: the traces (dashboard)

Starts on: `/traces`, opened before `StartCaptureAsync()` and left until the table holds at least 20 rows. The table
lists traces of the single resource. The labels below belong to the dashboard (confirm each).

- `traces-1`: no click. `MoveToAsync` the first row of the table, then `HoldAsync` until the narration ends. Visible
  at the end: the traces table (columns `Timestamp`, `Name`, `Spans` and `Duration`) with rows from the resource `OrchardCore.Cms.Web`, some with one span (static files),
  some with many (pages). Source of the name `OrchardCore.Cms.Web`: the default of `service.name`
  (`OrchardCoreResourceDetector.cs:24-25`).
- `traces-2`: apply a filter on the attribute, in about 10 seconds. Click `Add filter` in the toolbar; in the dialog
  choose the parameter `orchardcore.tenant.name`, the condition `equals`, and the value `Marketing`; click the button
  that applies the filter (confirm its label, and that a custom attribute is offered as a parameter). Wait until the
  filter appears above the table and every row shows the Marketing tenant: `scene.Page.GetByText("Marketing")`
  is visible. Visible at the end: the filtered table, with the filter `orchardcore.tenant.name` = `Marketing`.
  Attribute name from `OrchardCoreTelemetryConstants.cs:26`.
- `traces-3`: open the trace of a page request. Among the first 15 rows, click the `View` link of the row with the
  most spans (a page; the single-span rows are static files) with `ClickAndWaitForNavigationAsync`. In the trace page,
  click the first span of the tree (the span of the request), then `HighlightAsync` the row of the span details whose
  name is `orchardcore.tenant.name`, with the caption `Marketing`. Visible at the end: the span of the request
  selected, its details open, and `orchardcore.tenant.name` = `Marketing` ringed. Source: the middleware sets it
  after the request (`OrchardCore.OpenTelemetry/TenantTelemetryMiddleware.cs:34`).
- `traces-4`: `ClearHighlightsAsync()`, go back to the table (`WaitForNavigationAsync(() => scene.Page.GoBackAsync())`;
  the filter stays), then open the trace with the longest duration: it is the request that set the Marketing tenant
  up (seed step 5). In its tree, expand the span `Execute recipe Blog` so that its steps (`Execute recipe step ...`)
  show, scroll the tree by about 300 pixels, click `Execute recipe Blog`, and `HighlightAsync` the detail
  `orchardcore.recipe.name` = `Blog`. Visible at the end: the tree with `Execute recipe Blog` and spans named
  `Execute recipe step ...`, and the detail `orchardcore.recipe.name` ringed. Span names from
  `OrchardCore.Recipes.Core/Services/RecipeExecutor.cs:44` and `:170`; the attribute from
  `OrchardCoreTelemetryConstants.cs:46` (`orchardcore.recipe.name`).
- `traces-5`: `ClearHighlightsAsync()`, go back to the table, open a page-request trace as in `traces-3` (the newest
  one), click the first span whose name starts with `Render shape`, and `HighlightAsync` the details
  `orchardcore.shape.type` and `orchardcore.shape.binding`. Visible at the end: many spans named `Render shape ...`
  in the tree, one selected, with its shape type and its binding (a template path) ringed. Source:
  `OrchardCore.DisplayManagement/Implementation/ShapeTelemetry.cs:32` and `DefaultHtmlDisplay.cs:85`.

## Clip `05-metrics`: the metrics (dashboard)

Starts on: `/metrics`, opened before `StartCaptureAsync()`. If the resource is not selected, choose
`OrchardCore.Cms.Web` in the resource list before the capture.

- `metrics-1`: in the instrument tree, expand the meter `Microsoft.AspNetCore.Hosting`, and click the instrument
  `http.server.request.duration`. Wait until a chart is drawn. `HighlightAsync` the attribute filter or the legend that
  lists `orchardcore.tenant.name` with the values `Default` and `Marketing`. Visible at the end: the chart of
  `http.server.request.duration`, and `orchardcore.tenant.name` with its two values ringed. Source: the tag is added by
  `TenantTelemetryMiddleware.cs:38` (`IncludeTenantName` defaults to true, `OrchardCoreOpenTelemetryOptions.cs:96`).
- `metrics-2`: `ClearHighlightsAsync()`, expand the meter `OrchardCore`, and click `orchardcore.tenants` (the chart
  shows the counts by `orchardcore.tenant.state`); `HighlightAsync` the instruments
  `orchardcore.tenant.container.duration` and `orchardcore.tenant.activation.duration` in the tree. Then expand
  `System.Runtime` and click `dotnet.process.memory.working_set`. Visible at the end: the chart of
  `dotnet.process.memory.working_set`, with the `OrchardCore` and `System.Runtime` meters in the tree. Instrument names
  from `OrchardCoreTelemetryConstants.cs:112` and `:82`, and `TenantMetrics.cs:23`; `dotnet.process.memory.working_set`
  from the documentation page (Metrics, `System.Runtime`).

## Clip `06-logs`: the structured logs (dashboard)

Starts on: `/structuredlogs`, opened before `StartCaptureAsync()`. Before the capture, apply the filter
`orchardcore.tenant.name` = `Marketing` (the filter icon button opens the advanced filter dialog; confirm its fields,
which the Aspire documentation does not name) and wait until the table holds at least
10 rows, so that the filter is already on screen when the clip starts.

- `logs-1`: click the `View` link (details) of the first row with `ClickAndWaitForNavigationAsync` if it opens a page,
  or with `ClickAsync` if it opens a panel. `HighlightAsync` the attribute `orchardcore.tenant.name` = `Marketing`
  in the details. Visible at the end: the details of a record, with `orchardcore.tenant.name` = `Marketing` ringed, and
  the filter still shown. Source: the processor adds the attribute to the records written in a tenant scope
  (`OrchardCore.OpenTelemetry/TenantLogRecordProcessor.cs:14`).
- `logs-2`: `ClearHighlightsAsync()`, close the details, and `HoldAsync` on the table. Visible at the end: the table of
  records with their levels. No click is needed: the narration talks about log levels and the Logging option.

## Slides (drawn by slides.py, nothing to record)

- `collect-1`: four cards: traces, metrics, logs, and the export over OTLP.
- `collect-2`: three cards: the tenant attribute on spans, log records and HTTP server metrics, and the resource attributes.
- `collect-3`: four cards: why OpenTelemetry is configured for the application and not per tenant.
- `enable-1`: two code panels: `AddOpenTelemetry()`, and the same call with the optional callback.
- `enable-2`: a code panel with the `HasOtlpEndpoint()` check, and two cards.
- `enable-3`: the endpoint in the configuration, the environment variable, and two cards.
- `try-1`: three cards: the receiver, the instance, the two tenants.
- `conf-1`: the options in the configuration, and three cards.
- `conf-2`: the `Sources` and `Meters` of `YesSql`, and three cards with the names of other libraries.
- `conf-3`: the `Sources` and `Meters` of `OrchardCore.DisplayManagement`, and three cards.
- `conf-4`: the environment variables of a hosted backend, and two cards.
- `ext-1`: the three `AddOrchardCoreInstrumentation()` calls, and a card on their order.
- `ext-2`: the code of a module that starts a span and records an exception.

## Checks for private data

Before assembling, look at every contact sheet, then OCR the video at one frame per second and at every scene change
(see the skill, "Check for private data") and search for:

- the Windows user name, the name of the machine (`hostname`), and any path starting with `C:\Users`: the dashboard
  logs "Content root path" and the resource properties show `host.name`; both must show the neutral folder
  `C:\ocvideo\web` and `video-host`;
- the recording password, and any email other than `admin@example.com`;
- IP addresses and ports other than `localhost`, `127.0.0.1`, `5010`, `4317` and `18888`;
- real site names: the only names are `Orchard Core` and `Marketing`;
- tokens: the dashboard runs without a token (anonymous access), and its login URL, if it ever shows, must not appear.

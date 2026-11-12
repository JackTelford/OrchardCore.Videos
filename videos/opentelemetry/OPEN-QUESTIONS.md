# Open questions: OpenTelemetry

What was not verified from the Orchard Core source or its documentation, and what a person has to confirm on a
running site before or while recording. None of it is stated as a fact in the narration beyond what the source and the
documentation say.

## 1. The Aspire dashboard

The dashboard is not part of the Orchard Core source, so its labels cannot be read from it. The narration says only
what Orchard Core emits (span names, attribute names, metric names, the `service.name` and `service.instance.id`
resource attributes) and "the traces", "the metrics" and "the structured logs", the words the documentation uses.
`recording-plan.md` uses dashboard details that come from the Aspire documentation
(https://aspire.dev/dashboard/standalone/ and https://aspire.dev/dashboard/explore/, read once each) or from memory. Confirm each with `Recorder probe`, and fix the
plan, not the narration, where the dashboard differs:

- The image `mcr.microsoft.com/aspire/dashboard:latest`, the ports (UI 18888, OTLP over gRPC 4317 mapped to 18889 in the
  container), and `ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true`, which the page names without showing a Docker flag.
- The page paths `/traces`, `/metrics` and `/structuredlogs`, and whether a banner (anonymous access, telemetry
  notice) has to be dismissed before the capture.
- A filter by attribute. The Aspire documentation describes an `Add filter` dialog on the traces page (a Parameter, a
  Condition and a Value) and a filter icon button with an advanced filter dialog on the structured logs page, but it
  does not say that a custom attribute such as `orchardcore.tenant.name` is offered as a parameter, and it does not
  name the fields of the dialog of the logs. The narration of `traces-2` only describes the action on screen. If the
  attribute is not offered, `traces-2` and the start of `06-logs` must change, and so must the narration of both.
- The columns of the traces table: the documentation names `Timestamp`, `Name`, `Spans` and `Duration`. Whether a `View`
  link opens a trace, and the order of the rows (newest first?), are not documented. The plan picks the page-request
  trace (most spans) and the setup trace (longest duration) with them.
- The metrics page: the documentation says instruments are grouped under their meter and that dimension filters list the
  values of each tag below the chart; where `orchardcore.tenant.name` appears is not documented.
- How the dashboard names a resource (`OrchardCore.Cms.Web`) and where it shows the instance ID `web-1` and
  `host.name`.

## 2. Spans that depend on timing

- `Build tenant container`, `Build tenant pipeline` and `Activate tenant` are named in the documentation and in the
  source (`ShellContextFactory.cs:137`, `ShellPipelineExtensions.cs:34`, `ShellScope.cs:336`), but whether they appear
  as root traces at the start of the application or as children of the first request of a tenant was not checked. The
  narration (`traces-4`) names them without showing them. If they are visible in the seeded data, a person may
  show them in `traces-4`; the narration still holds.
- A `Run background task` span appears only if a background task of the Blog recipe runs during the seed. The
  narration names it without showing it.
- With the slow shape threshold at zero (`recording-plan.md`), a page has a `Render shape` span for every shape.
  The narration says so ("Here the threshold is zero"). The trace of a page may then hold a long list of spans: check
  that the span details can be reached on screen.
- Whether the request that sets the Marketing tenant up (the longest trace, seed step 5) carries `orchardcore.tenant.name`
  = `Marketing` on its request span and on the recipe spans, so that it stays in the list filtered on `Marketing`
  in `traces-2` and `traces-4`. The source tags a span with the tenant of the current shell scope
  (`TenantActivityProcessor.cs`) and the request span from the shell context of the request
  (`TenantTelemetryMiddleware.cs`); what the shell scope holds during Auto Setup was not read. If the setup trace is
  not under the filter, open it before applying the filter in `traces-2`, or filter on `Default` for `traces-4`
  (the narration of `traces-4` does not name the tenant).

## 3. Logs

- `Microsoft.Hosting.Lifetime` logs the content root path at the `Information` level. The plan publishes the
  application to `C:\ocvideo\web` so that the path holds no personal name; check the structured logs by OCR anyway.
- The documentation says the OpenTelemetry logger is added next to NLog or Serilog; the narration no longer says it. `UseSerilogWeb()` calls `UseSerilog` without `writeToProviders`, and Serilog then ignores other logger providers by default (Serilog behaviour, not in this source, not checked): with Serilog the OpenTelemetry logger may receive nothing.
- That the OpenTelemetry logger provider still receives records when the NLog host of `OrchardCore.Cms.Web`
  (`UseNLogHost()`) is used was not checked in the source of NLog. The documentation says the OpenTelemetry logger is
  added next to NLog or Serilog. If no record arrives in the dashboard, `06-logs` cannot be recorded as planned.
- That records with the `orchardcore.tenant.name` attribute exist with `ASPNETCORE_ENVIRONMENT=Development` (default
  level `Debug`), for the Marketing tenant. The attribute is added by `TenantLogRecordProcessor.cs` when a record is
  written in the scope of a tenant.

## 4. Pronunciation

`pronounce.py` was not run (it creates audio files). A person confirms these spellings of the `spoken` entries of
`storyboard.json` with `scripts/pronounce.py` before the first narration: `O.T.E.L. exporter O.T.L.P. endpoint`
for `OTEL_EXPORTER_OTLP_ENDPOINT`, `O.T.L.P.` for `OTLP`, `Has O.T.L.P. Endpoint`, `I.D.` in `service.instance.id` and
`Service Instance I.D.`, `Yes S.Q.L.` for `YesSql`, `H.T.T.P. Client` for `HttpClient`, `I Logger` for `ILogger`, and
the dotted names (`orchard core dot tenant dot name`, `System dot Runtime`, `Orchard Core dot C.M.S. dot Web`),
`O.T.L.P.` for the `Otlp` section, `H.T.T.P. protobuf` for `http/protobuf`, `web one` for `web-1`. The narration also says "N plus one" for the N+1 query pattern, and writes `OpenTelemetry` as one word, which
the voice may read as one word or two.

## 5. The documentation page and this video

- The page `src/docs/reference/modules/OpenTelemetry/README.md` already embeds a YouTube video with a plain `<iframe>`
  (`r6aAa3TrLus`) and no `<!-- video:... -->` marker. Whether this video replaces it, and who edits the page, is not
  decided here.
- The page says the video shows "the traces, metrics and logs of two instances serving two tenants". This video
  records one instance serving two tenants, and explains `service.instance.id` on slides. Two instances would need two
  processes, each with its own copy of the site data, because two processes should not share one Sqlite database. If
  the sentence of the page must stay true, either change the sentence or add a second instance to the recording.

## 6. Not checked by running it

- The sources and meters of other libraries (`YesSql`, `Npgsql`, `Elastic.Transport`, `MailKit.Net.SmtpClient`,
  `Azure.*`) and the N+1 remark of `conf-2` come from the documentation page and the slide; the libraries are not in the
  Orchard Core source (`OrchardCore.OpenTelemetry` only adds the names of `Tracing:Sources` and `Metrics:Meters`
  with `AddSource` and `AddMeter`, `OpenTelemetryOrchardCoreBuilderExtensions.cs:66` and `:90`). A person confirms
  with the YesSql package version of the checkout that its activity source and meter are named `YesSql`.

- An endpoint that is not an absolute HTTP or HTTPS URI stops the application from starting (documentation, and the
  exception in `OtlpExporterOptionsFactory.cs`): the narration does not say it.
- The meters of ASP.NET Core and HttpClient that the documentation lists beyond `Microsoft.AspNetCore.Hosting` and
  `System.Runtime`: the narration does not name them.
- The Grafana Cloud values of `conf-4` are the documentation's placeholders (`{region}`, `{Base64 credentials}`); no
  real instance, token or header is shown.

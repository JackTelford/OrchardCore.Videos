# OpenTelemetry: Traces, Metrics and Logs for Each Tenant

**YouTube:** not published yet · **Length:** 5:28 · **Feature:** `OrchardCore.OpenTelemetry` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/OpenTelemetry/))

What OpenTelemetry collects in an Orchard Core application, why it is configured for the whole application instead
of per tenant, and how to enable it in the host and point it at an OTLP endpoint. The video then reads the traces,
metrics and logs of two tenants in the Aspire dashboard, and shows the options, the extra sources and meters, and the
code that adds the instrumentation to an existing setup or emits telemetry from a module. The title, description,
tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. It shows no sign-in, so it
needs no `state.json` and no `Recorder setup`. Everything to decide is in `recording-plan.md`; the points to confirm
on a running dashboard are in `OPEN-QUESTIONS.md`.

1. Publish `src/OrchardCore.Cms.Web` of OrchardCore to a neutral folder (the application logs its content root path),
   and start the Aspire dashboard in Docker as the OTLP receiver.
2. Run the application with the environment variables of `recording-plan.md`: the OTLP endpoint of the dashboard, the
   instance ID `web-1`, `ASPNETCORE_ENVIRONMENT=Development`, a slow shape threshold of zero, and Auto Setup for the
   `Default` tenant and the `Marketing` tenant (the Blog recipe, the time zone `America/Los_Angeles`, the user `admin`).
3. Seed the telemetry outside the capture: request both tenants so that Auto Setup runs their recipes while the
   dashboard receives, then send some traffic to both.
4. Narrate and record: `scripts/narrate.py videos/opentelemetry`, then `Recorder record` the clips in this order:
   `03-browse` (the site, `VIDEO_BASE_URL=http://localhost:5010`), then `04-traces`, `05-metrics` and `06-logs` (the
   dashboard, `VIDEO_BASE_URL=http://localhost:18888`). No clip changes the site. To record from the start again,
   stop the application and the dashboard, empty `App_Data`, and repeat the seed.

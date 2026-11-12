# Documentation issues: OpenTelemetry

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.OpenTelemetry logs | reference/modules/OpenTelemetry/README.md | The OpenTelemetry logger is added next to the other logging providers, such as NLog or Serilog. | WithLogging adds an OpenTelemetry logger provider. UseSerilogWeb calls UseSerilog with no writeToProviders argument, and Serilog (outside this source) ignores other providers by default. So the claim for Serilog is unverified and probably false. | OrchardCore/OrchardCore.Logging.Serilog/WebHostBuilderExtensions.cs:9; OrchardCore.OpenTelemetry/OpenTelemetryOrchardCoreBuilderExtensions.cs:93 | opentelemetry |
| OrchardCore.OpenTelemetry video link | reference/modules/OpenTelemetry/README.md | The video shows the traces, metrics and logs of two instances serving two tenants. | The planned recording uses one instance serving two tenants. This is not a code disagreement but a docs and video mismatch. | videos/opentelemetry/recording-plan.md | opentelemetry |

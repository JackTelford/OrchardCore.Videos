"""The slides of the OpenTelemetry video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"OrchardCoreTelemetry", "ActivitySource", "Exception", "WebApplication"}

SIGNALS = [
    ("1", "Traces", "Spans for requests, tenants, recipes and background tasks."),
    ("2", "Metrics", "Request durations, tenant counts and the health of the runtime."),
    ("3", "Logs", "ILogger records with their structured attributes and scopes."),
    ("check", "Exported over OTLP", "To the Aspire dashboard, Grafana, Azure Monitor and other backends."),
]

TAGS = [
    ("1", "Spans and log records", "orchardcore.tenant.name holds the name of the tenant."),
    ("2", "HTTP server metrics", "The same attribute, unless IncludeTenantName is turned off."),
    ("3", "Resource attributes", "service.name and service.instance.id identify the application and the instance."),
]

NOT_A_FEATURE = [
    ("1", "Global listeners", "The listeners of traces and metrics belong to the process, not to a tenant."),
    ("2", "Traced before the tenant", "A request is traced before its tenant is resolved."),
    ("3", "Tenants are rebuilt", "A rebuilt tenant would reset its metrics and drop what is not exported."),
    ("4", "The host decides", "Where the telemetry is exported is up to whoever hosts the application."),
]

SETUP = [
    ("1", "A receiver", "The Aspire dashboard accepts OTLP on port 4317."),
    ("2", "One instance", "OrchardCore.Cms.Web starts with that endpoint and the instance ID web-1."),
    ("3", "Two tenants", "Default, and Marketing under the marketing URL prefix."),
]

ADD = """
builder.Services
    .AddOrchardCms()
    .AddOpenTelemetry();
"""

ADD_CALLBACK = """
builder.Services
    .AddOrchardCms()
    .AddOpenTelemetry(openTelemetry => openTelemetry
        .WithTracing(tracing => tracing.AddSqlClientInstrumentation()));
"""

GUARD = """
var orchardCore = builder.Services.AddOrchardCms();

if (builder.Configuration.HasOtlpEndpoint())
{
    orchardCore.AddOpenTelemetry();
}
"""

ENDPOINT_ENV = """
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
"""

ENDPOINT_JSON = """
{
  "OrchardCore": {
    "OpenTelemetry": {
      "Otlp": {
        "Endpoint": "http://localhost:4317",
        "Protocol": "Grpc"
      }
    }
  }
}
"""

OPTIONS = """
{
  "OrchardCore": {
    "OpenTelemetry": {
      "ServiceName": "MyCms",
      "ServiceInstanceId": "web-1",
      "Tracing": {
        "SamplingRatio": 0.25,
        "SlowShapeThreshold": "00:00:00.050"
      },
      "Metrics": {
        "IncludeTenantName": true
      }
    }
  }
}
"""

LIBRARIES = """
{
  "OrchardCore": {
    "OpenTelemetry": {
      "Tracing": {
        "Sources": [ "YesSql" ]
      },
      "Metrics": {
        "Meters": [ "YesSql" ]
      }
    }
  }
}
"""

SHAPES = """
{
  "OrchardCore": {
    "OpenTelemetry": {
      "Tracing": {
        "Sources": [ "OrchardCore.DisplayManagement" ]
      },
      "Metrics": {
        "Meters": [ "OrchardCore.DisplayManagement" ]
      }
    }
  }
}
"""

HOSTED = """
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_EXPORTER_OTLP_ENDPOINT=https://otlp-gateway-{region}.grafana.net/otlp
OTEL_EXPORTER_OTLP_HEADERS=Authorization=Basic%20{Base64 credentials}
"""

EXISTING = """
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddOrchardCoreInstrumentation())
    .WithMetrics(metrics => metrics.AddOrchardCoreInstrumentation())
    .WithLogging(logging => logging.AddOrchardCoreInstrumentation());
"""

MODULE = """
using var activity = OrchardCoreTelemetry.ActivitySource.StartActivity("Import products");

try
{
    await ImportProductsAsync();
}
catch (Exception ex)
{
    OrchardCoreTelemetry.RecordException(activity, ex);

    throw;
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "collect-1":
        image = slide_base(chapter_title, label, "Three signals, one protocol")
        bullets(image, SIGNALS)
        return image

    if step_id == "collect-2":
        image = slide_base(chapter_title, label, "Tagged for each tenant and instance")
        bullets(image, TAGS)
        return image

    if step_id == "collect-3":
        image = slide_base(chapter_title, label, "Configured for the application, not per tenant")
        bullets(image, NOT_A_FEATURE)
        return image

    if step_id == "enable-1":
        image = slide_base(chapter_title, label, "1. Call AddOpenTelemetry")
        draw_code(image, (96, 240, W - 96, 520), "Program.cs", "csharp", ADD, types=TYPES, size=28)
        draw_code(image, (96, 560, W - 96, 900), "Program.cs", "csharp", ADD_CALLBACK, types=TYPES, size=26)
        return image

    if step_id == "enable-2":
        image = slide_base(chapter_title, label, "2. Export only when an endpoint is configured")
        draw_code(image, (96, 240, 1110, 680), "Program.cs", "csharp", GUARD, types=TYPES, size=26)
        note_cards(image, [
            ("Without an endpoint", "Nothing is collected."),
            ("With an endpoint", "Telemetry is exported over OTLP."),
        ], (1150, 260, W - 96, 1030))
        return image

    if step_id == "enable-3":
        image = slide_base(chapter_title, label, "3. Point it at an OTLP receiver")
        draw_code(image, (96, 240, 1000, 740), "Configuration", "json", ENDPOINT_JSON, types=TYPES, size=26)
        draw_code(image, (1040, 240, W - 96, 440), "Environment variable", "bash", ENDPOINT_ENV, types=TYPES, size=24)
        note_cards(image, [
            ("Protocol", "Grpc, or HttpProtobuf."),
            ("Precedence", "The Otlp section wins."),
        ], (1040, 480, W - 96, 1030))
        return image

    if step_id == "try-1":
        image = slide_base(chapter_title, label, "What runs in this video")
        bullets(image, SETUP)
        return image

    if step_id == "conf-1":
        image = slide_base(chapter_title, label, "The OpenTelemetry options")
        draw_code(image, (96, 240, 1110, 900), "Configuration", "json", OPTIONS, types=TYPES, size=26)
        note_cards(image, [
            ("SamplingRatio", "A ratio from 0 to 1 of traces."),
            ("SlowShapeThreshold", "50 milliseconds by default."),
            ("IncludeTenantName", "Turn off with many tenants."),
        ], (1150, 250, W - 96, 1030))
        return image

    if step_id == "conf-2":
        image = slide_base(chapter_title, label, "Collect other libraries")
        draw_code(image, (96, 240, 1000, 800), "Configuration", "json", LIBRARIES, types=TYPES, size=26)
        note_cards(image, [
            ("Databases", "YesSql, Npgsql, MySqlConnector."),
            ("Search and email", "Elastic.Transport, MailKit.Net.SmtpClient."),
            ("Azure SDK", "Azure.* matches every Azure source."),
        ], (1040, 250, W - 96, 1030))
        return image

    if step_id == "conf-3":
        image = slide_base(chapter_title, label, "Trace every shape")
        draw_code(image, (96, 240, 1010, 740), "Configuration", "json", SHAPES, types=TYPES, size=24)
        note_cards(image, [
            ("Spans", "Render shape, nested like the shapes."),
            ("Binding", "orchardcore.shape.binding names the template."),
            ("Histogram", "orchardcore.display.shape.duration."),
        ], (1050, 250, W - 96, 1030))
        return image

    if step_id == "conf-4":
        image = slide_base(chapter_title, label, "Send it to a hosted backend")
        draw_code(image, (96, 240, W - 96, 520), "Environment variables", "bash", HOSTED, types=TYPES, size=26)
        note_cards(image, [
            ("Headers", "Pass the credentials of a hosted backend."),
            ("Enabled", "Set it to false to turn all telemetry off."),
        ], (96, 580, W - 96, 1030))
        return image

    if step_id == "ext-1":
        image = slide_base(chapter_title, label, "Add it to an existing configuration")
        draw_code(image, (96, 240, W - 96, 640), "Program.cs", "csharp", EXISTING, types=TYPES, size=26)
        note_cards(image, [
            ("Order", "Add it before the exporters, so the telemetry is tagged first."),
        ], (96, 700, W - 96, 1030))
        return image

    if step_id == "ext-2":
        image = slide_base(chapter_title, label, "Emit telemetry from a module")
        draw_code(image, (96, 240, W - 96, 880), "ImportProducts.cs", "csharp", MODULE, types=TYPES, size=26)
        return image

    raise SystemExit(f"No slide for {step_id}")

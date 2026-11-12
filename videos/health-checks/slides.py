"""The slides of the Health Checks video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code is taken from the documentation of the Health Checks module, trimmed with // ... where needed.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IHealthCheck", "HealthCheckResult", "HealthCheckContext", "CancellationToken", "Task", "StartupBase",
         "IServiceCollection", "DependencyHealthCheck", "Startup"}

HOSTS = [
    ("1", "Load balancer", "Sends a probe request to the site."),
    ("2", "Container platform", "Sends a probe request to the site."),
    ("3", "Uptime monitor", "Sends a probe request to the site."),
    ("check", "Health Checks module", "Answers the probe at an endpoint of each tenant."),
]

REGISTERS = [
    ("check", "The endpoint", "Mapped in every tenant that enables the feature."),
    ("check", "A response writer", "Writes the detailed JSON response."),
    ("0", "No check of its own", "With none registered, the tenant reports Healthy."),
    ("0", "No administration pages", "And no site settings."),
]

ADDRESSES = [
    ("Tenant on a host", "https://example.com/health/live"),
    ("Tenant with a URL prefix", "https://example.com/customer-a/health/live"),
    ("Tenant with its own host name", "https://customer-a.example.com/health/live"),
]

CONTRIBUTED = [
    ("1", "Redis Health Check", "Pings the Redis server."),
    ("2", "SMS Health Check", "Validates the Twilio credentials."),
    ("check", "Both features are required", "A check is registered only when its feature and Health Checks are enabled."),
]

CHECK = """
using Microsoft.Extensions.Diagnostics.HealthChecks;

public sealed class DependencyHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Or HealthCheckResult.Degraded(...), or HealthCheckResult.Unhealthy(...).
        var result = HealthCheckResult.Healthy("The dependency is available.");

        return Task.FromResult(result);
    }
}
"""

STARTUP = """
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace MyModule;

[RequireFeatures("OrchardCore.HealthChecks")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<DependencyHealthCheck>("Dependency");
    }
}
"""

CURL = """
curl --include https://example.com/health/live
"""

COMPACT_BODY = """
Healthy
"""

DETAILED = """
{
  "Status": "Unhealthy",
  "Duration": "00:00:00.0420000",
  "HealthChecks": [
    {
      "Name": "Dependency",
      "Status": "Unhealthy",
      "Description": "The dependency did not respond."
    }
  ]
}
"""

SETTINGS = """
{
  "OrchardCore": {
    "HealthChecks": {
      "Url": "/health/live",
      "ShowDetails": false
    }
  }
}
"""

ENVIRONMENT = """
OrchardCore__HealthChecks__Url=/health/live
OrchardCore__HealthChecks__ShowDetails=false
OrchardCore__CustomerA__HealthChecks__Url=/health/ready
"""

SECURITY = [
    ("1", "No authentication", "Anyone who can reach the address can call it."),
    ("2", "Detailed descriptions", "They can disclose dependency names or operational information."),
    ("3", "Restrict access", "Use a network policy, a reverse proxy, or another perimeter control."),
]

SCOPE = [
    ("1", "Every registration runs", "The module does not use tags to filter the checks."),
    ("2", "Liveness and readiness", "Add filtered endpoints in your own code."),
    ("3", "Rate Limits", "An endpoint policy can limit repeated requests to the path."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Who asks whether the site is running")
        bullets(image, HOSTS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "What the module registers")
        bullets(image, REGISTERS)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The address of a tenant")
        note_cards(image, ADDRESSES, (96, 270, W - 96, 960))
        return image

    if step_id == "checks-1":
        image = slide_base(chapter_title, label, "Checks from Orchard Core features")
        bullets(image, CONTRIBUTED)
        return image

    if step_id == "checks-2":
        image = slide_base(chapter_title, label, "1. Implement IHealthCheck")
        draw_code(image, (96, 240, W - 96, 930), "DependencyHealthCheck.cs", "csharp", CHECK, types=TYPES, size=28)
        return image

    if step_id == "checks-3":
        image = slide_base(chapter_title, label, "2. Register it in a startup class")
        draw_code(image, (96, 240, 1180, 900), "Startup.cs", "csharp", STARTUP, types=TYPES, size=25)
        note_cards(image, [
            ("RequireFeatures", "limits it to Health Checks."),
            ("Or a dependency", "in the manifest of your feature."),
            ("AddCheck", "registers the check by name."),
        ], (1230, 270, W - 96, 960))
        return image

    if step_id == "resp-1":
        image = slide_base(chapter_title, label, "The compact response")
        draw_code(image, (96, 240, 1100, 440), "Terminal", "bash", CURL, size=27)
        draw_code(image, (96, 480, 1100, 680), "Response body", "bash", COMPACT_BODY, size=27)
        note_cards(image, [
            ("Healthy", "200 OK"),
            ("Degraded", "200 OK"),
            ("Unhealthy", "503 Service Unavailable"),
        ], (1150, 240, W - 96, 960))
        return image

    if step_id == "resp-2":
        image = slide_base(chapter_title, label, "The detailed response")
        draw_code(image, (96, 240, 1010, 800), "ShowDetails: true", "json", DETAILED, size=26)
        note_cards(image, [
            ("Every status", "returns 200 OK."),
            ("Read the JSON", "Status property."),
            ("Not included:", "exception, data, tags."),
        ], (1060, 240, W - 96, 960))
        return image

    if step_id == "resp-3":
        image = slide_base(chapter_title, label, "Configure the endpoint")
        draw_code(image, (96, 240, 1010, 660), "Configuration", "json", SETTINGS, size=26)
        draw_code(image, (96, 700, 1010, 940), "Environment variables", "bash", ENVIRONMENT, size=24)
        note_cards(image, [
            ("Url", "The route of the endpoint."),
            ("ShowDetails", "false: compact. true: JSON."),
            ("Reload or restart", "after changing a setting."),
        ], (1060, 240, W - 96, 960))
        return image

    if step_id == "sec-1":
        image = slide_base(chapter_title, label, "Keep the endpoint private")
        bullets(image, SECURITY)
        return image

    if step_id == "sec-2":
        image = slide_base(chapter_title, label, "One endpoint, every check")
        bullets(image, SCOPE)
        return image

    raise SystemExit(f"No slide for {step_id}")

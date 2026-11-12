"""The slides of the Go to Production video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"WebApplication"}

# What changes between a development machine and a live site.
CHANGES = [
    ("1", "Environment", "Development on your machine, Production on the server."),
    ("2", "Settings", "Values come from the host, not from committed files."),
    ("3", "Requests", "They arrive through a reverse proxy, and over HTTPS."),
    ("4", "Keys", "Servers that run one site share its data protection keys."),
]

# src/OrchardCore.Cms.Web/Program.cs, trimmed.
PROGRAM = """
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// ...
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseOrchardCore();

await app.RunAsync();
"""

ENVIRONMENT = """
# On the server
export ASPNETCORE_ENVIRONMENT=Production
"""

DATABASE = """
{
  "OrchardCore": {
    "DatabaseProvider": "SqlConnection",
    "ConnectionString": "<set by the host>"
  }
}
"""

DATABASE_ENVIRONMENT = """
# A double underscore separates the levels.
export OrchardCore__DatabaseProvider=SqlConnection
export OrchardCore__ConnectionString="<set by the host>"
"""

ALLOWED_HOSTS = """
{
  "AllowedHosts": "example.com;localhost",
  "OrchardCore": {
    // ...
  }
}
"""

APP_DATA_TREE = """
App_Data/
  tenants.json
  Sites/
    Default/
      appsettings.json
      DataProtection-Keys/
      Lucene/
"""

APP_DATA_VARIABLE = """
# Relative to the application, or absolute.
export ORCHARD_APP_DATA=/var/orchard/App_Data
"""

KEYS_TREE = """
App_Data/
  Sites/
    Default/
      DataProtection-Keys/
"""

PIPELINE = [
    ("1", "Reverse proxy", "Ordered before CORS, Security and authentication."),
    ("2", "CORS and Security", "Both are ordered after the proxy. CORS then uses the correct host."),
    ("3", "Authentication", "Also configured after the proxy."),
]

PROXY_HOST = """
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCms()
    .ConfigureReverseProxySettings();
// ...
"""

PROXY_SETTINGS = """
{
  "OrchardCore": {
    "ReverseProxy": {
      "ForwardedHeaders": "XForwardedFor, XForwardedHost, XForwardedProto",
      "KnownNetworks": ["192.168.1.0/24"],
      "KnownProxies": ["192.168.1.200", "192.168.1.201"]
    }
  }
}
"""

HTTPS_RECIPE = """
{
  "steps": [
    {
      "name": "settings",
      "HttpsSettings": {
        "StrictTransportSecurityMode": "FromConfiguration",
        "RequireHttps": true,
        "RequireHttpsPermanent": false,
        "SslPort": 443
      }
    }
  ]
}
"""

HSTS = [
    ("1", "Maximum age", "The header lasts 365 days."),
    ("2", "Subdomains", "The header covers the subdomains of the site."),
    ("3", "Preload", "The preload option is not set."),
]

WHY_SHARE = [
    ("1", "The keys are lost", "Existing cookies and tokens stop working."),
    ("2", "Servers differ", "Each server has its own keys, so requests fail on the others."),
    ("3", "One key ring", "Shared storage that every server reads and writes."),
]

AZURE = """
{
  "OrchardCore": {
    "DataProtection": {
      "Azure": {
        "ConnectionString": "<connection string of the storage account>",
        "ContainerName": "dataprotection",
        "CreateContainer": true
      }
    }
  }
}
"""

AZURE_TENANT = """
{
  "OrchardCore": {
    "DataProtection": {
      "Azure": {
        "ContainerName": "{{ ShellSettings.Name }}-dataprotection",
        "BlobName": "{{ ShellSettings.Name }}DataProtectionKeys.xml"
      }
    }
  }
}
"""

REDIS = """
{
  "OrchardCore": {
    "Redis": {
      "Configuration": "<your-redis-connection-string>",
      "InstancePrefix": "MyApp:",
      "AllowAdmin": true
    }
  }
}
"""

CHECK_LOG = [
    ("1", "Azure Storage", "Without a connection string, a critical error is logged."),
    ("2", "Redis", "Without a valid configuration, the Redis features stay off."),
    ("3", "Fallback", "The site keeps using the keys in App_Data."),
]

READY = [
    ("check", "Environment", "Production, with the settings supplied by the host."),
    ("check", "Reverse proxy", "Forwarded headers, known proxies and known networks."),
    ("check", "HTTPS", "Require HTTPS, and a decision about HSTS."),
    ("check", "Data protection keys", "One key ring shared by every server."),
]

SCRIPTED = [
    ("1", "The host", "Environment variables and the App_Data location."),
    ("2", "Configuration", "Data protection keys, known proxies and networks."),
    ("3", "A recipe", "The Settings step: forwarded headers and HTTPS."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("checklist-1", "checklist-2"):
        image = slide_base(chapter_title, label, "From your machine to a live site")
        bullets(image, CHANGES, active={0, 1} if step_id == "checklist-1" else {2, 3})
        return image

    if step_id == "config-1":
        image = slide_base(chapter_title, label, "Run in the Production environment")
        draw_code(image, (96, 240, 1100, 860), "Program.cs", "csharp", PROGRAM, types=TYPES, size=24)
        draw_code(image, (1140, 240, W - 96, 520), "Environment variable", "bash", ENVIRONMENT, size=24)
        note_cards(image, [("Per environment", "appsettings.Production.json")], (1140, 560, W - 96, 1030))
        return image

    if step_id == "config-2":
        image = slide_base(chapter_title, label, "Settings live under OrchardCore")
        draw_code(image, (96, 240, 940, 560), "appsettings.json", "json", DATABASE, size=24)
        draw_code(image, (980, 240, W - 96, 560), "Environment variables", "bash", DATABASE_ENVIRONMENT, size=22)
        note_cards(image, [("Root values", "They apply to each tenant that does not set its own.")],
                   (96, 620, W - 96, 1030))
        return image

    if step_id == "config-3":
        image = slide_base(chapter_title, label, "Keep credentials out of committed files")
        draw_code(image, (96, 240, 1000, 620), "appsettings.json", "json", ALLOWED_HOSTS, size=26)
        note_cards(image, [
            ("Host names", "Semicolon-delimited, no ports."),
            ("Where", "Public-facing edge servers."),
            ("Credentials", "Environment or a secret store."),
        ], (1040, 240, W - 96, 1030))
        return image

    if step_id == "config-4":
        image = slide_base(chapter_title, label, "Where the files live")
        draw_code(image, (96, 240, 900, 700), "App_Data", "bash", APP_DATA_TREE, size=26)
        draw_code(image, (940, 240, W - 96, 520), "Environment variable", "bash", APP_DATA_VARIABLE, size=22)
        note_cards(image, [("Per tenant", "Settings, keys, indexes and the SQLite file.")], (940, 560, W - 96, 1030))
        return image

    if step_id == "proxy-5":
        image = slide_base(chapter_title, label, "Where the proxy sits in the pipeline")
        bullets(image, PIPELINE)
        return image

    if step_id in ("proxy-6", "proxy-7"):
        image = slide_base(chapter_title, label, "Known proxies and networks")
        if step_id == "proxy-6":
            draw_code(image, (96, 240, W - 96, 575), "Program.cs", "csharp", PROXY_HOST, types=TYPES, size=24, highlight={6})
            draw_code(image, (96, 590, W - 96, 1030), "appsettings.json", "json", PROXY_SETTINGS, size=24, highlight={3, 5, 6})
        else:
            draw_code(image, (96, 240, W - 96, 575), "Program.cs", "csharp", PROXY_HOST, types=TYPES, size=24, highlight=set())
            draw_code(image, (96, 590, W - 96, 1030), "appsettings.json", "json", PROXY_SETTINGS, size=24, highlight={5, 6})
        return image

    if step_id == "https-5":
        image = slide_base(chapter_title, label, "Set HTTPS in a recipe")
        draw_code(image, (96, 240, 960, 780), "settings.recipe.json", "json", HTTPS_RECIPE, size=24)
        note_cards(image, [
            ("Disabled", "No HSTS header, in every environment."),
            ("Enabled", "The HSTS header, in every environment."),
            ("FromConfiguration", "Labeled From environment: Production only."),
        ], (1000, 240, W - 96, 1030))
        return image

    if step_id == "https-6":
        image = slide_base(chapter_title, label, "When HSTS is on")
        bullets(image, HSTS)
        return image

    if step_id == "keys-1":
        image = slide_base(chapter_title, label, "The default key ring")
        draw_code(image, (96, 240, 900, 620), "App_Data", "bash", KEYS_TREE, size=26, highlight={4})
        note_cards(image, [
            ("Per tenant", "Each tenant has its own key ring."),
            ("Protects", "Cookies, tokens and secrets."),
        ], (940, 240, W - 96, 1030))
        return image

    if step_id == "keys-2":
        image = slide_base(chapter_title, label, "Why the key ring must be shared")
        bullets(image, WHY_SHARE)
        return image

    if step_id == "keys-3":
        image = slide_base(chapter_title, label, "Azure Blob Storage")
        draw_code(image, (96, 240, 1250, 760), "appsettings.json", "json", AZURE, size=24)
        note_cards(image, [
            ("Default container", "dataprotection"),
            ("CreateContainer", "True by default."),
        ], (1290, 240, W - 96, 1030))
        return image

    if step_id == "keys-4":
        image = slide_base(chapter_title, label, "One container for each tenant")
        draw_code(image, (96, 240, W - 96, 700), "appsettings.json", "json", AZURE_TENANT, size=24)
        note_cards(image, [("Default blob", "Sites/<tenant>/DataProtectionKeys.xml in the container")],
                   (96, 740, W - 96, 1030))
        return image

    if step_id == "keys-5":
        image = slide_base(chapter_title, label, "Redis")
        draw_code(image, (96, 240, 1100, 680), "appsettings.json", "json", REDIS, size=24)
        note_cards(image, [
            ("Persistence", "AOF or RDB must be on."),
            ("AllowAdmin", "Lets the site check it."),
        ], (1140, 240, W - 96, 1030))
        return image

    if step_id == "keys-6":
        image = slide_base(chapter_title, label, "Check that the shared store is used")
        bullets(image, CHECK_LOG)
        return image

    if step_id == "ready-1":
        image = slide_base(chapter_title, label, "Before you go live")
        bullets(image, READY)
        return image

    if step_id == "ready-2":
        image = slide_base(chapter_title, label, "Most items can be scripted")
        bullets(image, SCRIPTED)
        return image

    raise SystemExit(f"No slide for {step_id}")

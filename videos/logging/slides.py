"""The slides of the Logging video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code is taken from the Orchard Core source (the OrchardCore.Logging.NLog and OrchardCore.Logging.Serilog
libraries, the project template of the CMS web application) and trimmed with // ...
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"StringBuilder", "LogEventInfo", "HttpContext", "ShellContextFeature", "ShellScope", "LogContext",
         "WebApplication", "ShellOptionConstants", "LogManager", "Path", "Environment", "Task"}

# One column of cards under or next to a code panel: (x0, x1) of the cards.
LEFT_PANEL = (96, 1180)
RIGHT_CARDS = (1230, W - 96)
HALF_LEFT = (96, 940)
HALF_RIGHT = (980, W - 96)

WHY = [
    ("1", "One deployment", "It hosts several tenants, each an isolated site."),
    ("2", "Shared log files", "By default, every tenant writes to the same files."),
    ("3", "The tenant name", "It tells you which tenant wrote a line."),
]

PACKAGES = [
    ("1", "OrchardCore.Logging.NLog", "NLog, the logger of the CMS web project and of the template by default."),
    ("2", "OrchardCore.Logging.Serilog", "Serilog, for structured logging."),
    ("3", "Libraries, not modules", "You reference a package. The admin has no feature to enable."),
]

CREATE = """
# NLog, the default
dotnet new occms --logger nlog

# Serilog
dotnet new occms --logger serilog

# No logger
dotnet new occms --logger none
"""

ADDS = [
    ("1", "NLog", "Package, UseNLogHost, NLog.config, Logging section"),
    ("2", "Serilog", "Package, UseSerilog, UseSerilogTenantNameLogging, Serilog section"),
    ("3", "None", "No package, no logger code, no logger configuration"),
]

ADD_PACKAGE = """
# NLog
dotnet add package OrchardCore.Logging.NLog

# Serilog
dotnet add package OrchardCore.Logging.Serilog
"""

NLOG_PROGRAM = """
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms();

// ...
"""

NLOG_CONFIG_DIR = """
.ConfigureAppConfiguration((context, _) =>
{
    // ...
    var environment = context.HostingEnvironment;

    var appData = System.Environment.GetEnvironmentVariable(ShellOptionConstants.OrchardAppData);

    var configDir = string.IsNullOrWhiteSpace(appData)
        ? Path.Combine(environment.ContentRootPath, ShellOptionConstants.DefaultAppDataPath)
        : appData;

    LogManager.Configuration.Variables["configDir"] = configDir;
});
"""

NLOG_TARGETS = """
<targets>
    <!-- file target -->
    <target xsi:type="File" name="file"
            fileName="${var:configDir}/logs/orchard-log-${shortdate}.log"
            layout="..."
    />

    <!-- console target -->
    <target xsi:type="Console" name="console" />
</targets>
"""

NLOG_LAYOUT = [
    ("1", "Time and tenant", "longdate, then orchard-tenant-name"),
    ("2", "Request and event", "aspnet-traceidentifier, then the EventId property"),
    ("3", "Logger and level", "logger, then the level in uppercase"),
    ("4", "Message", "message, then the exception with its stack trace"),
]

NLOG_RENDERER = """
protected override void Append(StringBuilder builder, LogEventInfo logEvent)
{
    // If there is no ShellContext in the Features then the log is rendered from the Host.
    var tenantName = HttpContextAccessor?.HttpContext?.Features.Get<ShellContextFeature>()
        ?.ShellContext.Settings.Name;

    tenantName ??= ShellScope.Context?.Settings.Name;

    builder.Append(tenantName ?? "None");
}
"""

NLOG_RULES = """
<rules>
    <!-- The YesSql events go to the file targets -->
    <logger name="YesSql" minlevel="${var:yessqlLevel}" writeTo="file" finalMinLevel="Off" />

    <!-- The hosting lifetime events go to the console and file targets -->
    <logger name="Microsoft.Hosting.Lifetime" minlevel="${var:hostingLifetimeLevel}" writeTo="file, console" finalMinLevel="Off" />

    <!-- All default and above go to the file target -->
    <logger name="*" minlevel="${var:defaultLevel}" writeTo="file" />
</rules>
"""

NLOG_VARIABLE = """
<variable name="defaultLevel"
    value="${configsetting:item=Logging.LogLevel.Default:whenEmpty=Warning}" />
<!-- ... -->
"""

LOGGING_SECTION = """
"Logging": {
  "LogLevel": {
    "Default": "Warning",
    "YesSql": "Information",
    "Microsoft.Hosting.Lifetime": "Information"
  }
}
"""

ADD_SERILOG = """
dotnet add package OrchardCore.Logging.Serilog
"""

SERILOG_PROGRAM = """
using OrchardCore.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, configBuilder) =>
{
    configBuilder.ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext();
});
"""

SERILOG_PIPELINE = """
// ...
app.UseStaticFiles();

app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());

app.Run();
"""

SERILOG_MIDDLEWARE = """
public async Task Invoke(HttpContext context)
{
    var tenantName = context.Features.Get<ShellContextFeature>()?.ShellContext.Settings.Name
        ?? ShellScope.Context?.Settings.Name ?? "None";

    using (LogContext.PushProperty("TenantName", tenantName))
    {
        await _next.Invoke(context);
    }
}
"""

SERILOG_WEB = """
using OrchardCore.Logging;

// ...
builder.Host.UseSerilogWeb();

// ...
app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());
"""

SERILOG_LEVELS = """
"Serilog": {
  "MinimumLevel": {
    "Default": "Warning",
    "Override": {
      // ...
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  // ...
}
"""

SERILOG_SINKS = """
"WriteTo": [
  {
    "Name": "Console",
    "Args": {
      // ...
    }
  },
  {
    "Name": "File",
    "Args": {
      // ...
    }
  }
]
"""

SERILOG_CONSOLE = """
{
  "Name": "Console",
  "Args": {
    // ...
    "outputTemplate": "{Timestamp:HH:mm:ss}|{TenantName}|{RequestId}|{SourceContext}|{Level:u3}|{Message:lj}{NewLine}{Exception}",
    "restrictedToMinimumLevel": "Information"
  }
}
"""

SERILOG_FILE = """
{
  "Name": "File",
  "Args": {
    // Replace App_Data with %ORCHARD_APP_DATA% to change log file location to use environment variable.
    "path": "App_Data/logs/orchard-log.txt",
    "rollingInterval": "Day",
    // ...
    "restrictedToMinimumLevel": "Warning"
  }
}
"""

COMPARE = [
    ("N", "NLog: configuration", "NLog.config, with the levels of the Logging section"),
    ("N", "NLog: tenant name", "The orchard-tenant-name layout renderer"),
    ("S", "Serilog: configuration", "The Serilog section of appsettings.json"),
    ("S", "Serilog: tenant name", "A middleware that pushes the TenantName property"),
]

COMMON = [
    ("1", "The tenant name", "Part of the log files of the template, with either library."),
    ("2", "The logs folder", "App_Data/logs by default."),
    ("3", "OpenTelemetry", "Covered by a separate video."),
]


def cards(image, notes, x0, x1, y):
    """Cards side by side: (title, text) each, in the band x0..x1 at y."""
    gap = 32
    width = (x1 - x0 - gap * (len(notes) - 1)) // len(notes)
    for index, note in enumerate(notes):
        left = x0 + index * (width + gap)
        note_cards(image, [note], (left, y, left + width, y + 180))


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "One application, many tenants")
        bullets(image, WHY)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Two logging packages")
        bullets(image, PACKAGES)
        return image

    if step_id == "choose-1":
        image = slide_base(chapter_title, label, "Pick a logger with the project template")
        draw_code(image, (96, 240, W - 96, 700), "Command line", "bash", CREATE, types=TYPES, size=28)
        cards(image, [("nlog", "The default"), ("serilog", "Serilog instead"), ("none", "No logger")],
              96, W - 96, 760)
        return image

    if step_id == "choose-2":
        image = slide_base(chapter_title, label, "What the template adds")
        bullets(image, ADDS)
        return image

    if step_id == "choose-3":
        image = slide_base(chapter_title, label, "Add a logger to an existing project")
        draw_code(image, (96, 240, 1180, 700), "Command line", "bash", ADD_PACKAGE, types=TYPES, size=28)
        note_cards(image, [("Then copy", "the calls in Program.cs"), ("And copy", "the configuration files")],
                   (1230, 240, W - 96, 700))
        return image

    if step_id == "nlog-1":
        image = slide_base(chapter_title, label, "UseNLogHost in Program.cs")
        draw_code(image, (96, 240, 1180, 760), "Program.cs", "csharp", NLOG_PROGRAM, types=TYPES, size=28)
        note_cards(image, [("Turns on NLog", "for the host"), ("Registers", "orchard-tenant-name")],
                   (1230, 240, W - 96, 760))
        return image

    if step_id == "nlog-2":
        image = slide_base(chapter_title, label, "The configDir variable")
        draw_code(image, (96, 240, W - 96, 790), "HostBuilderExtensions.cs", "csharp", NLOG_CONFIG_DIR,
                  types=TYPES, size=24)
        cards(image, [("ORCHARD_APP_DATA", "The environment variable"),
                      ("App_Data", "The folder under the content root")], 96, W - 96, 830)
        return image

    if step_id == "nlog-3":
        image = slide_base(chapter_title, label, "Targets in NLog.config")
        draw_code(image, (96, 240, W - 96, 730), "NLog.config", "xml", NLOG_TARGETS, types=TYPES, size=28)
        cards(image, [("File target", "The logs folder under configDir"), ("Console target", "The console")],
              96, W - 96, 780)
        return image

    if step_id == "nlog-4":
        image = slide_base(chapter_title, label, "The layout of a line")
        bullets(image, NLOG_LAYOUT, top=240)
        return image

    if step_id == "nlog-5":
        image = slide_base(chapter_title, label, "The orchard-tenant-name renderer")
        draw_code(image, (96, 240, W - 96, 730), "TenantLayoutRenderer.cs", "csharp", NLOG_RENDERER,
                  types=TYPES, size=28)
        cards(image, [("1. Request", "The tenant of the request"), ("2. Scope", "The tenant of the current scope"),
                      ("3. Otherwise", "None")], 96, W - 96, 780)
        return image

    if step_id == "nlog-6":
        image = slide_base(chapter_title, label, "Three rules")
        draw_code(image, (96, 240, W - 96, 700), "NLog.config", "xml", NLOG_RULES, types=TYPES, size=22)
        cards(image, [("YesSql", "To the file"), ("Hosting lifetime", "To the file and the console"),
                      ("Default and above", "To the file")], 96, W - 96, 760)
        return image

    if step_id == "nlog-7":
        image = slide_base(chapter_title, label, "Levels come from appsettings.json")
        draw_code(image, (96, 240, W - 96, 440), "NLog.config", "xml", NLOG_VARIABLE, types=TYPES, size=22)
        draw_code(image, (96, 470, 900, 1030), "appsettings.json", "json", LOGGING_SECTION, types=TYPES, size=24)
        note_cards(image, [("Lower Default", "to write more events"),
                           ("Development", "The Orchard Core web project uses Debug")], (940, 470, W - 96, 1030))
        return image

    if step_id == "serilog-1":
        image = slide_base(chapter_title, label, "UseSerilog in Program.cs")
        draw_code(image, (96, 240, W - 96, 390), "Command line", "bash", ADD_SERILOG, types=TYPES, size=28)
        draw_code(image, (96, 420, W - 96, 1030), "Program.cs", "csharp", SERILOG_PROGRAM, types=TYPES, size=26)
        return image

    if step_id == "serilog-2":
        image = slide_base(chapter_title, label, "UseSerilogTenantNameLogging")
        draw_code(image, (96, 240, 1180, 700), "Program.cs", "csharp", SERILOG_PIPELINE, types=TYPES, size=26)
        note_cards(image, [("Placed after", "the middleware that resolves the tenant"), ("It adds", "the tenant name middleware")],
                   (1230, 240, W - 96, 700))
        return image

    if step_id == "serilog-3":
        image = slide_base(chapter_title, label, "The tenant name middleware")
        draw_code(image, (96, 240, W - 96, 700), "SerilogTenantNameLoggingMiddleware.cs", "csharp",
                  SERILOG_MIDDLEWARE, types=TYPES, size=26)
        cards(image, [("Request or scope", "The tenant name, or None"),
                      ("TenantName", "Pushed as a log property"),
                      ("The request", "Until the request ends")], 96, W - 96, 760)
        return image

    if step_id == "serilog-4":
        image = slide_base(chapter_title, label, "UseSerilogWeb")
        draw_code(image, (96, 240, 1180, 700), "Program.cs", "csharp", SERILOG_WEB, types=TYPES, size=26)
        note_cards(image, [("Replaces", "the UseSerilog call"), ("Keeps", "the call in UseOrchardCore")],
                   (1230, 240, W - 96, 700))
        return image

    if step_id == "serilog-5":
        image = slide_base(chapter_title, label, "The Serilog section")
        draw_code(image, (96, 240, 900, 700), "appsettings.json", "json", SERILOG_LEVELS, types=TYPES, size=24)
        draw_code(image, (940, 240, W - 96, 830), "appsettings.json", "json", SERILOG_SINKS, types=TYPES, size=24)
        return image

    if step_id == "serilog-6":
        image = slide_base(chapter_title, label, "The Console sink")
        draw_code(image, (96, 240, W - 96, 600), "appsettings.json", "json", SERILOG_CONSOLE, types=TYPES,
                  highlight={5, 6}, size=22)
        cards(image, [("{TenantName}", "Pushed by the middleware"), ("Information", "The level of this sink")],
              96, W - 96, 660)
        return image

    if step_id == "serilog-7":
        image = slide_base(chapter_title, label, "The File sink")
        draw_code(image, (96, 240, W - 96, 690), "appsettings.json", "json", SERILOG_FILE, types=TYPES, size=24)
        cards(image, [("rollingInterval", "A file for each day"), ("restrictedToMinimumLevel", "Warning")],
              96, W - 96, 740)
        return image

    if step_id == "compare-1":
        image = slide_base(chapter_title, label, "NLog and Serilog side by side")
        bullets(image, COMPARE, top=240)
        return image

    if step_id == "compare-2":
        image = slide_base(chapter_title, label, "What both have in common")
        bullets(image, COMMON)
        return image

    raise SystemExit(f"No slide for {step_id}")

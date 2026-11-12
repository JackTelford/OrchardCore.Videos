"""The slides of the "Create a CMS Web Application With dotnet new" video. assemble.py calls slide() for each step of a
"slide" clip. Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.

The commands, the files and the console output are drawn here because the recorder captures a browser, not a terminal.
No panel holds a version number: the version of the templates changes, the video does not.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"WebApplication"}

INSTALL = """
dotnet new install OrchardCore.ProjectTemplates
"""

INSTALL_VERSIONS = """
# A specific release: add @ and the version number.
dotnet new install OrchardCore.ProjectTemplates@<version>

# Preview packages: add -* to the version and the preview NuGet source.
dotnet new install OrchardCore.ProjectTemplates@<version>-* --nuget-source <preview-feed-url>
"""

CREATE = """
# In an empty folder:
dotnet new occms -n MySite
"""

TREE = """
MySite/
├── MySite.csproj
├── Program.cs
├── appsettings.json
├── NLog.config
└── Properties/
    └── launchSettings.json
"""

CSPROJ = """
<Project Sdk="Microsoft.NET.Sdk.Web">

  <!-- ... -->

  <ItemGroup>
    <PackageReference Include="OrchardCore.Logging.NLog" Version="..." />
    <PackageReference Include="OrchardCore.Application.Cms.Targets" Version="..." />
  </ItemGroup>

</Project>
"""

PROGRAM = """
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // ...
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseOrchardCore();
app.Run();
"""

OPTIONS = """
dotnet new occms --help
dotnet new occms -n MySite --logger none
"""

RUN = """
dotnet run --project MySite/MySite.csproj
"""

LAUNCH_SETTINGS = """
{
  // ...
  "profiles": {
    // ...
    "MySite": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
"""

CONSOLE = """
... Now listening on: https://localhost:5001
... Now listening on: http://localhost:5000
... Application started. Press Ctrl+C to shut down.
"""

NLOG = """
<targets>
  <target xsi:type="File" name="file"
          fileName="${var:configDir}/logs/orchard-log-${shortdate}.log"
          layout="..." />
  <target xsi:type="Console" name="console" />
</targets>

<rules>
  <!-- The YesSql events go to the file targets -->
  <logger name="YesSql" minlevel="${var:yessqlLevel}"
          writeTo="file" finalMinLevel="Off" />

  <!-- The hosting lifetime events go to the console and file targets -->
  <logger name="Microsoft.Hosting.Lifetime" minlevel="${var:hostingLifetimeLevel}"
          writeTo="file, console" finalMinLevel="Off" />

  <!-- All default and above go to the file target -->
  <logger name="*" minlevel="${var:defaultLevel}" writeTo="file" />
</rules>
"""

LOG_LEVELS = """
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "YesSql": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
"""

DATA_TREE = """
App_Data/
├── Sites/
│   └── Default/
│       ├── Media/
│       └── yessql.db
└── logs/
    └── orchard-log-<date>.log
"""

NEXT = """
# A module:
dotnet new ocmodulecms -n ModuleName.OrchardCore

# A theme:
dotnet new octheme -n "ThemeName.OrchardCore"
"""

AUTO_SETUP = """
builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");
"""

BY_HAND = [
    ("1", "Create an empty web project", "An ASP.NET Core web application."),
    ("2", "Add the CMS package", "OrchardCore.Application.Cms.Targets, from NuGet."),
    ("3", "Register Orchard Core", "AddOrchardCms and UseOrchardCore in Program.cs."),
    ("4", "Remove the default pages", "The Pages and Views folders that the web project starts with."),
]

WITH_TEMPLATE = [
    ("check", "One command", "dotnet new occms creates the whole project."),
    ("check", "Logging included", "NLog by default, Serilog or none on request."),
    ("check", "Ready to run", "Launch settings with local https and http addresses."),
    ("check", "Matching versions", "By default, the packages use the version of the installed templates."),
]

INSTALL_NEEDS = [
    ("1", "You need", "The .NET SDK and a terminal."),
    ("2", "You install", "OrchardCore.ProjectTemplates, a NuGet package, once."),
]

INSTALL_VERSION_NOTES = [
    ("1", "A specific release", "Add @ and the version number after the package ID."),
    ("2", "Preview packages", "Add -* to the version and the preview NuGet source, as the documentation shows."),
]

TEMPLATES = [
    ("1", "occms", "Orchard Core Cms Web App"),
    ("2", "ocmvc", "Orchard Core Mvc Web App"),
    ("3", "ocmodulecms, ocmodulemvc", "Orchard Core Cms Module, Orchard Core Mvc Module"),
    ("4", "octheme", "Orchard Core Theme"),
]

CREATE_NOTES = [
    ("1", "-n MySite", "The name of the project: it becomes the folder and the project file."),
    ("2", "occms", "The short name of the Orchard Core Cms Web App template."),
]

TREE_NOTES = [
    ("MySite.csproj", "References the Orchard Core packages."),
    ("NLog.config", "Left out with the Serilog and None options."),
    ("launchSettings.json", "The local addresses and the environment."),
]

VERSION_NOTE = [
    ("1", "The versions", "By default, both packages use the version of the installed templates."),
]

PACKAGES = [
    ("1", "OrchardCore.Application.Cms.Targets", "Everything in the Core package, plus the starter themes and the translations."),
    ("2", "OrchardCore.Application.Cms.Core.Targets", "The modules and the admin theme, without front end themes."),
    ("3", "When to use the Core package", "Headless sites, and sites with their own theme."),
]

OPTION_CARDS = [
    ("lo", "-lo, --logger", "NLog (the default), Serilog or None."),
    ("ov", "-ov, --orchard-version", "The version of the Orchard Core packages: by default, the version of the templates."),
    ("fm", "-fm, --framework", "The target framework of the project."),
]

RUN_STEPS = [
    ("1", "Run it from the folder that holds MySite", "The path of the project file is relative to that folder."),
    ("2", "The packages come from NuGet", "The project file references them: Orchard Core is not part of your source."),
    ("3", "The launch profile is named MySite", "It defines the addresses and the environment of the site."),
]

STOP_STEPS = [
    ("1", "Press Ctrl+C", "Stops the server in the terminal."),
    ("2", "Run it again", "The data stays in App_Data, so it starts as the same site."),
]

LEVEL_NOTES = [
    ("Default", "Other events go to the file."),
    ("YesSql", "Database events go to the file."),
    ("Microsoft.Hosting.Lifetime", "File and console."),
]

LOGGER_CARDS = [
    ("1", "--logger NLog", "OrchardCore.Logging.NLog, NLog.config and UseNLogHost."),
    ("2", "--logger Serilog", "OrchardCore.Logging.Serilog, a Serilog section in appsettings.json and UseSerilog."),
    ("3", "--logger None", "No logging package and no logger code in Program.cs."),
]

DATA_NOTES = [
    ("Sqlite", "A file in the tenant folder."),
    ("Media", "Uploads go to the tenant's Media folder."),
    ("ORCHARD_APP_DATA", "Another folder for the data of the site."),
]

NEXT_CARDS = [
    ("1", "Next to the web application", "Create each module and theme in a folder of its own."),
    ("2", "Reference them", "Add a project reference from the web application."),
]

AUTO_SETUP_CARDS = [
    ("1", "On the first request", "The Auto Setup module installs the site, with no Setup screen."),
    ("2", "From the configuration", "The site name, recipe, database and super user come from the configuration."),
]


def slide(step_id, chapter_title, label):
    if step_id == "ways-1":
        image = slide_base(chapter_title, label, "Building it by hand")
        bullets(image, BY_HAND)
        return image

    if step_id == "ways-2":
        image = slide_base(chapter_title, label, "With the template")
        bullets(image, WITH_TEMPLATE)
        return image

    if step_id == "install-1":
        image = slide_base(chapter_title, label, "Install the templates")
        draw_code(image, (96, 240, W - 96, 440), "Terminal", "bash", INSTALL, size=34)
        bullets(image, INSTALL_NEEDS, top=500)
        return image

    if step_id == "install-2":
        image = slide_base(chapter_title, label, "Versions and preview packages")
        draw_code(image, (96, 240, W - 96, 580), "Terminal", "bash", INSTALL_VERSIONS, size=28)
        bullets(image, INSTALL_VERSION_NOTES, top=620)
        return image

    if step_id == "install-3":
        image = slide_base(chapter_title, label, "The templates in the package")
        bullets(image, TEMPLATES)
        return image

    if step_id == "create-1":
        image = slide_base(chapter_title, label, "Create the project")
        draw_code(image, (96, 240, W - 96, 470), "Terminal", "bash", CREATE, size=34)
        bullets(image, CREATE_NOTES, top=530)
        return image

    if step_id == "create-2":
        image = slide_base(chapter_title, label, "What the template creates")
        draw_code(image, (96, 240, 900, 700), "MySite", "bash", TREE, size=30)
        note_cards(image, TREE_NOTES, (960, 240, W - 96, 1000))
        return image

    if step_id == "create-3":
        image = slide_base(chapter_title, label, "The project file")
        draw_code(image, (96, 240, W - 96, 680), "MySite.csproj", "xml", CSPROJ, size=26)
        bullets(image, VERSION_NOTE, top=720)
        return image

    if step_id == "create-4":
        image = slide_base(chapter_title, label, "Two CMS packages")
        bullets(image, PACKAGES)
        return image

    if step_id == "create-5":
        image = slide_base(chapter_title, label, "Program.cs")
        draw_code(image, (96, 240, W - 96, 1030), "Program.cs", "csharp", PROGRAM, types=TYPES, size=24,
                  highlight={5, 7, 8, 19})
        return image

    if step_id == "create-6":
        image = slide_base(chapter_title, label, "The template options")
        bullets(image, OPTION_CARDS, top=240)
        draw_code(image, (96, 790, W - 96, 990), "Terminal", "bash", OPTIONS, size=28)
        return image

    if step_id == "run-1":
        image = slide_base(chapter_title, label, "Run the project")
        draw_code(image, (96, 240, W - 96, 440), "Terminal", "bash", RUN, size=34)
        bullets(image, RUN_STEPS, top=500)
        return image

    if step_id == "run-2":
        image = slide_base(chapter_title, label, "The launch settings")
        draw_code(image, (96, 240, W - 96, 800), "Properties/launchSettings.json", "json", LAUNCH_SETTINGS, size=24)
        draw_code(image, (96, 830, W - 96, 1040), "Console", "bash", CONSOLE, size=26)
        return image

    if step_id == "log-1":
        image = slide_base(chapter_title, label, "NLog.config")
        draw_code(image, (96, 240, W - 96, 1010), "NLog.config", "xml", NLOG, size=24)
        return image

    if step_id == "log-2":
        image = slide_base(chapter_title, label, "The log levels")
        draw_code(image, (96, 240, 1000, 700), "appsettings.json", "json", LOG_LEVELS, size=26)
        note_cards(image, LEVEL_NOTES, (1050, 240, W - 96, 1000))
        return image

    if step_id == "log-3":
        image = slide_base(chapter_title, label, "The logger option")
        bullets(image, LOGGER_CARDS)
        return image

    if step_id == "data-1":
        image = slide_base(chapter_title, label, "The App_Data folder")
        draw_code(image, (96, 240, 1000, 700), "Created when the site runs", "bash", DATA_TREE, size=28)
        note_cards(image, DATA_NOTES, (1050, 240, W - 96, 1000))
        return image

    if step_id == "data-2":
        image = slide_base(chapter_title, label, "Stop and start again")
        bullets(image, STOP_STEPS)
        return image

    if step_id == "next-1":
        image = slide_base(chapter_title, label, "Modules and themes")
        draw_code(image, (96, 240, W - 96, 540), "Terminal", "bash", NEXT, size=28)
        bullets(image, NEXT_CARDS, top=590)
        return image

    if step_id == "next-2":
        image = slide_base(chapter_title, label, "Auto Setup")
        draw_code(image, (96, 240, W - 96, 500), "Program.cs", "csharp", AUTO_SETUP, size=30)
        bullets(image, AUTO_SETUP_CARDS, top=560)
        return image

    raise SystemExit(f"No slide for {step_id}")

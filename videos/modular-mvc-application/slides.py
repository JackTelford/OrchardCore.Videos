"""The slides of the modular-mvc-application video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the projects that the Orchard Core Mvc Web App and Orchard Core Mvc Module templates create
(src/Templates/OrchardCore.ProjectTemplates in OrchardCore) and from the guide, trimmed with "..." where it is not
the subject of the step.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"Startup", "StartupBase", "IServiceCollection", "IApplicationBuilder", "IEndpointRouteBuilder",
         "IServiceProvider", "HomeController", "Controller", "ActionResult", "WebApplication", "Module"}

BUILD = [
    ("1", "The web application", "Hosts Orchard Core and supplies the layout."),
    ("2", "The module", "Holds the controller, the action and the view."),
    ("3", "A project reference", "Connects the web application to the module."),
]

PACKAGE = [
    ("1", "A modular application", "Orchard Core loads the modules the application references."),
    ("2", "ASP.NET Core MVC", "The MVC pipeline, with support for modules."),
    ("3", "No CMS modules", "The application runs only the modules it references."),
]

BEFORE = [
    ("1", "A similar sample in the Orchard Core repository",
     "src/OrchardCore.Mvc.Web and src/OrchardCore.Modules/OrchardCore.Mvc.HelloWorld"),
    ("2", "The .NET SDK", "To create, build and run both projects."),
    ("3", "A text editor and a terminal", "A terminal where you can run dotnet commands."),
]

FIND = [
    ("1", "The module builds", "Module.Targets marks the assembly as a module and embeds its files."),
    ("2", "The web application builds", "It records the names of the modules it references."),
    ("3", "The application starts", "One tenant, with every feature of those modules enabled."),
]

RESTART = [
    ("1", "The first request", "Orchard Core builds the request pipeline of the tenant."),
    ("2", "Every Startup class", "Its Configure method registers routes and middleware."),
    ("3", "After a change", "Restart the application to build the pipeline again."),
]

INSTALL = """
# The Orchard Core code generation templates.
dotnet new install OrchardCore.ProjectTemplates
"""

WEB_COMMAND = """
# In an empty folder, such as OrchardCore.Mvc
dotnet new ocmvc -n OrchardCore.Mvc.Web
"""

MODULE_COMMAND = """
# In the same folder
dotnet new ocmodulemvc -n OrchardCore.Mvc.HelloWorld
"""

REFERENCE_COMMAND = """
# The web application references the module.
dotnet add OrchardCore.Mvc.Web reference OrchardCore.Mvc.HelloWorld
"""

SOLUTION_COMMANDS = """
# Optional: a solution that references both projects.
dotnet new sln -n OrchardCore.Mvc
dotnet sln add OrchardCore.Mvc.Web/OrchardCore.Mvc.Web.csproj
dotnet sln add OrchardCore.Mvc.HelloWorld/OrchardCore.Mvc.HelloWorld.csproj
"""

PROGRAM = """
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCore()
    .AddMvc();

var app = builder.Build();

// ...

app.UseStaticFiles();

app.UseOrchardCore();

app.Run();
"""

WEB_PROJECT = """
<Project Sdk="Microsoft.NET.Sdk.Web">

  <!-- ... -->

  <ItemGroup>
    <PackageReference Include="OrchardCore.Application.Mvc.Targets" Version="..." />
  </ItemGroup>

</Project>
"""

VIEW_START = """
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}
"""

MODULE_PROJECT = """
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <!-- ... -->
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="OrchardCore.Module.Targets" Version="..." />
  </ItemGroup>

</Project>
"""

MANIFEST = """
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrchardCore.Mvc.HelloWorld",
    Author = "The Orchard Core Team",
    Website = "https://orchardcore.net",
    Version = "0.0.1",
    Description = "OrchardCore.Mvc.HelloWorld",
    Category = "OrchardCore.Mvc.HelloWorld"
)]
"""

CONTROLLER = """
// ...
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Mvc.HelloWorld.Controllers;

public sealed class HomeController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}
"""

INDEX_VIEW = """
Hello from OrchardCore.Mvc.HelloWorld
"""

STARTUP = """
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.HelloWorld;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
    }
}
"""

RUN_COMMAND = """
dotnet run --project OrchardCore.Mvc.Web/OrchardCore.Mvc.Web.csproj
"""

CONSOLE = """
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
"""

DEFAULT_ROUTE = """
# Every action of a module:
/{area}/{controller}/{action}/{id?}

# The action of this module:
/OrchardCore.Mvc.HelloWorld/Home/Index
"""

STARTUP_ROUTE = """
public sealed class Startup : StartupBase
{
    // ...

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "Home",
            areaName: "OrchardCore.Mvc.HelloWorld",
            pattern: "",
            defaults: new { controller = "Home", action = "Index" }
        );
    }
}
"""

HEADING_VIEW = """
<h1>Hello World</h1>
"""


def slide(step_id, chapter_title, label):
    if step_id == "build-1":
        image = slide_base(chapter_title, label, "Two projects, connected by a reference")
        bullets(image, BUILD)
        return image

    if step_id == "build-2":
        image = slide_base(chapter_title, label, "What the web application brings")
        bullets(image, PACKAGE)
        return image

    if step_id == "build-3":
        image = slide_base(chapter_title, label, "Before you start")
        bullets(image, BEFORE)
        return image

    if step_id == "create-1":
        image = slide_base(chapter_title, label, "1. Install the templates")
        draw_code(image, (96, 240, 1100, 480), "Terminal", "bash", INSTALL, size=24)
        note_cards(image, [
            ("Orchard Core Mvc Web App", "Creates the web application."),
            ("Orchard Core Mvc Module", "Creates the module."),
        ], (1150, 240, W - 96, 700))
        return image

    if step_id == "create-2":
        image = slide_base(chapter_title, label, "2. Create the web application")
        draw_code(image, (96, 240, 1100, 480), "Terminal", "bash", WEB_COMMAND, size=24)
        note_cards(image, [
            ("Template", "Orchard Core Mvc Web App"),
            ("Short name", "ocmvc"),
            ("Project name", "OrchardCore.Mvc.Web"),
        ], (1150, 240, W - 96, 900))
        return image

    if step_id == "create-3":
        image = slide_base(chapter_title, label, "3. Create the module")
        draw_code(image, (96, 240, 1100, 480), "Terminal", "bash", MODULE_COMMAND, size=24)
        note_cards(image, [
            ("Template", "Orchard Core Mvc Module"),
            ("Short name", "ocmodulemvc"),
            ("Project name", "OrchardCore.Mvc.HelloWorld"),
        ], (1150, 240, W - 96, 900))
        return image

    if step_id == "create-4":
        image = slide_base(chapter_title, label, "4. Reference the module")
        draw_code(image, (96, 240, 1100, 480), "Terminal", "bash", REFERENCE_COMMAND, size=24)
        note_cards(image, [
            ("Project", "OrchardCore.Mvc.Web"),
            ("References", "OrchardCore.Mvc.HelloWorld"),
        ], (1150, 240, W - 96, 700))
        return image

    if step_id == "create-5":
        image = slide_base(chapter_title, label, "5. Optional: a solution file")
        draw_code(image, (96, 240, W - 96, 560), "Terminal", "bash", SOLUTION_COMMANDS, size=24)
        return image

    if step_id == "web-1":
        image = slide_base(chapter_title, label, "Program.cs registers Orchard Core")
        draw_code(image, (96, 240, W - 96, 900), "Program.cs", "csharp", PROGRAM, types=TYPES, size=26,
                  highlight={3, 4, 5, 13})
        return image

    if step_id == "web-2":
        image = slide_base(chapter_title, label, "The package and the layout")
        draw_code(image, (96, 240, W - 96, 660), "OrchardCore.Mvc.Web.csproj", "xml", WEB_PROJECT, types=TYPES, size=24)
        draw_code(image, (96, 700, W - 96, 930), "_ViewStart.cshtml", "cshtml", VIEW_START, types=TYPES, size=24)
        return image

    if step_id == "mod-1":
        image = slide_base(chapter_title, label, "The project file of the module")
        draw_code(image, (96, 240, W - 96, 920), "OrchardCore.Mvc.HelloWorld.csproj", "xml", MODULE_PROJECT,
                  types=TYPES, size=24)
        return image

    if step_id == "mod-2":
        image = slide_base(chapter_title, label, "The manifest")
        draw_code(image, (96, 240, W - 96, 700), "Manifest.cs", "csharp", MANIFEST, types=TYPES, size=26)
        return image

    if step_id == "mod-3":
        image = slide_base(chapter_title, label, "A controller and a view")
        draw_code(image, (96, 240, W - 96, 740), "Controllers/HomeController.cs", "csharp", CONTROLLER, types=TYPES,
                  size=24)
        draw_code(image, (96, 780, W - 96, 950), "Views/Home/Index.cshtml", "cshtml", INDEX_VIEW, types=TYPES, size=26)
        return image

    if step_id == "mod-4":
        image = slide_base(chapter_title, label, "The Startup class")
        draw_code(image, (96, 240, W - 96, 960), "Startup.cs", "csharp", STARTUP, types=TYPES, size=22)
        return image

    if step_id == "find-1":
        image = slide_base(chapter_title, label, "How Orchard Core finds the module")
        bullets(image, FIND, active={0})
        return image

    if step_id == "find-2":
        image = slide_base(chapter_title, label, "How Orchard Core finds the module")
        bullets(image, FIND, active={1})
        return image

    if step_id == "find-3":
        image = slide_base(chapter_title, label, "How Orchard Core finds the module")
        bullets(image, FIND, active={2})
        return image

    if step_id == "run-1":
        image = slide_base(chapter_title, label, "Run the web application")
        draw_code(image, (96, 240, W - 96, 420), "Terminal", "bash", RUN_COMMAND, size=24)
        draw_code(image, (96, 460, W - 96, 720), "Console", "bash", CONSOLE, size=24)
        return image

    if step_id == "route-1":
        image = slide_base(chapter_title, label, "The default route of a module")
        draw_code(image, (96, 240, 1100, 620), "Route pattern", "bash", DEFAULT_ROUTE, size=24)
        note_cards(image, [
            ("Area", "The identifier of the module"),
            ("Identifier", "Its project name by default"),
            ("Controller, action", "Home and Index"),
        ], (1150, 240, W - 96, 900))
        return image

    if step_id == "route-2":
        image = slide_base(chapter_title, label, "Register a route in Startup")
        draw_code(image, (96, 240, W - 96, 760), "Startup.cs", "csharp", STARTUP_ROUTE, types=TYPES, size=22,
                  highlight={5, 6, 7, 8, 9, 10, 11, 12, 13})
        return image

    if step_id == "route-3":
        image = slide_base(chapter_title, label, "Change the Index view")
        draw_code(image, (96, 240, W - 96, 440), "Views/Home/Index.cshtml", "cshtml", HEADING_VIEW, types=TYPES,
                  size=28)
        return image

    if step_id == "route-4":
        image = slide_base(chapter_title, label, "Why a restart")
        bullets(image, RESTART)
        return image

    raise SystemExit(f"No slide for {step_id}")

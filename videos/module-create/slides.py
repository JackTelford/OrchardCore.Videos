"""The slides of the "Create a Module" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"Startup", "StartupBase", "IServiceCollection", "IApplicationBuilder", "IEndpointRouteBuilder",
         "IServiceProvider", "HomeController", "Controller", "ActionResult", "WebApplication"}

# What a module is.
WHAT = [
    ("1", "A project", "A .NET class library with services, controllers, views, and static files."),
    ("2", "A manifest", "The name, description, and category shown on the Features page."),
    ("3", "Features", "Enabled per tenant. The code of a feature runs only where it is enabled."),
]

# The module template.
COMMANDS = """
# Install the templates once.
dotnet new install OrchardCore.ProjectTemplates

# Create the module from the CMS module template.
dotnet new ocmodulecms -n Greetings.OrchardCore
"""

PARTS = [
    ("1", "Greetings.OrchardCore.csproj", "Marks the project as a module, through the Module Targets package."),
    ("2", "Manifest.cs", "Describes the module: its name, description, and category."),
    ("3", "Startup.cs", "Registers the services and the routes of the module."),
    ("4", "Controllers and Views", "A Home controller and an Index view give the module a page."),
]

PROJECT_FILE = """
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <!-- ... -->

  <ItemGroup>
    <PackageReference Include="OrchardCore.Module.Targets" Version="..." />
    <!-- ... -->
  </ItemGroup>

</Project>
"""

STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // ...
    }

    public override void Configure(
        IApplicationBuilder builder,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "Home",
            areaName: "Greetings.OrchardCore",
            pattern: "Home/Index",
            defaults: new { controller = "Home", action = "Index" }
        );
    }
}
"""

CONTROLLER = """
public sealed class HomeController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}
"""

VIEW = """
<h1>Hello from the Greetings module</h1>
"""

# The manifest.
MANIFEST = """
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Greetings",
    Author = "Jamie Chen",
    Website = "https://example.com",
    Version = "1.0.0",
    Description = "Adds a greeting page to the site.",
    Category = "Samples"
)]
"""

MANIFEST_WITH_DEPENDENCIES = """
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Greetings",
    Author = "Jamie Chen",
    Website = "https://example.com",
    Version = "1.0.0",
    Description = "Adds a greeting page to the site.",
    Category = "Samples",
    Dependencies = ["OrchardCore.Contents"]
)]
"""

# The web project.
NEW_SITE = """
# Create a web project for your site.
dotnet new occms -n Contoso.Web
"""

SITE_PACKAGE = """
<PackageReference Include="OrchardCore.Application.Cms.Targets" Version="..." />
"""

PROGRAM = """
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCms();

var app = builder.Build();

// ...

app.UseOrchardCore();

app.Run();
"""

PROJECT_REFERENCE = """
<ItemGroup>
  <ProjectReference Include="..\\Greetings.OrchardCore\\Greetings.OrchardCore.csproj" />
</ItemGroup>
"""

BUILD_AND_START = [
    ("1", "The module project", "Is marked as a module by the Module Targets package."),
    ("2", "The web project builds", "Orchard Core records the name of each referenced module in its assembly."),
    ("3", "The site starts", "Orchard Core loads the recorded modules and lists their features."),
]

RUN = """
# From the folder of the web project
dotnet run
"""

RECIPE = """
{
  "steps": [
    {
      "name": "Feature",
      "enable": [
        "Greetings.OrchardCore"
      ]
    }
  ]
}
"""

MISSING = [
    ("1", "Module Targets package", "The module project references OrchardCore.Module.Targets."),
    ("2", "Project reference", "The web project references the module project."),
    ("3", "Build and run again", "Orchard Core records the referenced modules when the web project builds."),
]

def full(top, bottom):
    return 96, top, W - 96, bottom


def pair(image, left, right, top):
    """Two (title, text) cards side by side under a panel."""
    note_cards(image, [left], (96, top, 940, top + 180))
    note_cards(image, [right], (980, top, W - 96, top + 180))


def slide(step_id, chapter_title, label):
    # Chapter 1: what a module is.
    if step_id.startswith("what-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "A module, in three parts")
        bullets(image, WHAT, active={index - 1} if index < 4 else None)
        return image

    # Chapter 2: create the module.
    if step_id == "create-1":
        image = slide_base(chapter_title, label, "Install the project templates")
        draw_code(image, full(250, 620), "Terminal", "bash", COMMANDS, highlight={1, 2}, size=34)
        pair(image, ("dotnet new install", "Installs a template package."),
             ("OrchardCore.ProjectTemplates", "The package of the Orchard Core templates."), 700)
        return image

    if step_id == "create-2":
        image = slide_base(chapter_title, label, "Create the module from the template")
        draw_code(image, full(250, 620), "Terminal", "bash", COMMANDS, highlight={4, 5}, size=34)
        pair(image, ("ocmodulecms", "The short name of the CMS module template."),
             ("-n", "Names the project and its namespace."), 700)
        return image

    if step_id == "create-3":
        image = slide_base(chapter_title, label, "The parts of the generated project")
        bullets(image, PARTS)
        return image

    if step_id == "create-4":
        image = slide_base(chapter_title, label, "The project file")
        draw_code(image, full(250, 770), "Greetings.OrchardCore.csproj", "xml", PROJECT_FILE, highlight={1, 6}, size=30)
        pair(image, ("Razor SDK", "Precompiles the views of the module."),
             ("Module Targets", "Marks the project as a module."), 820)
        return image

    if step_id == "create-5":
        image = slide_base(chapter_title, label, "The Startup class")
        draw_code(image, full(240, 1030), "Startup.cs", "csharp", STARTUP, types=TYPES,
                  highlight={1, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19}, size=25)
        return image

    if step_id == "create-6":
        image = slide_base(chapter_title, label, "A controller and a view")
        draw_code(image, full(240, 640), "Controllers/HomeController.cs", "csharp", CONTROLLER, types=TYPES, size=28)
        draw_code(image, full(690, 900), "Views/Home/Index.cshtml", "html", VIEW, size=28)
        return image

    # Chapter 3: the manifest.
    if step_id.startswith("manifest-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "Describe the module")
        box = (96, 240, 1150, 740)
        if index == 4:
            draw_code(image, box, "Manifest.cs", "csharp", MANIFEST_WITH_DEPENDENCIES, size=26, highlight={3, 10, 11})
            notes = [("Dependencies", "The ids of the features it needs.")]
        elif index == 1:
            draw_code(image, box, "Manifest.cs", "csharp", MANIFEST, size=26, highlight={3, 4, 8, 10})
            notes = [("Name", "The label on the Features page."), ("Description", "Shown under the name.")]
        elif index == 2:
            draw_code(image, box, "Manifest.cs", "csharp", MANIFEST, size=26, highlight={3, 5, 6, 7, 9, 10})
            notes = [("Category", "The group on the Features page."), ("Author, Website, Version", "Its maker and its version.")]
        elif index == 3:
            draw_code(image, box, "Manifest.cs", "csharp", MANIFEST, size=26, highlight={3, 4, 10})
            notes = [("Name", "The label: Greetings."), ("Feature id", "Greetings.OrchardCore")]
        else:
            draw_code(image, box, "Manifest.cs", "csharp", MANIFEST, size=26)
            notes = [("Project file", "Can hold the same metadata.")]
        note_cards(image, notes, (1190, 260, W - 96, 0))
        return image

    # Chapter 4: install the module.
    if step_id == "install-1":
        image = slide_base(chapter_title, label, "A web project for your site")
        draw_code(image, full(250, 440), "Terminal", "bash", NEW_SITE, size=30)
        draw_code(image, full(490, 680), "Contoso.Web.csproj", "xml", SITE_PACKAGE, size=30)
        return image

    if step_id == "install-2":
        image = slide_base(chapter_title, label, "Orchard Core in Program.cs")
        draw_code(image, full(240, 800), "Program.cs", "csharp", PROGRAM, types=TYPES, highlight={3, 4, 10}, size=28)
        pair(image, ("AddOrchardCms", "Adds the services of Orchard Core."),
             ("UseOrchardCore", "Adds Orchard Core to the request pipeline."), 850)
        return image

    if step_id == "install-3":
        image = slide_base(chapter_title, label, "Reference the module project")
        draw_code(image, full(250, 480), "Contoso.Web.csproj", "xml", PROJECT_REFERENCE, size=30)
        pair(image, ("Visual Studio", "Dependencies, Add Project Reference."),
             ("The result", "A ProjectReference item in the project file."), 540)
        return image

    if step_id == "install-4":
        image = slide_base(chapter_title, label, "Why the reference is required")
        bullets(image, BUILD_AND_START)
        return image

    if step_id == "install-5":
        image = slide_base(chapter_title, label, "Run the site")
        draw_code(image, full(250, 480), "Terminal", "bash", RUN, size=34)
        pair(image, ("dotnet run", "Builds the project, then runs it."),
             ("After a change", "Run it again to load a changed manifest."), 540)
        return image

    # Chapter 7: a recipe.
    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "Enable the feature from a recipe")
        draw_code(image, (96, 240, 1150, 780), "recipe.json", "json", RECIPE, size=28)
        note_cards(image, [("enable", "The ids of the features to turn on."),
                           ("Feature id", "The name of the module's assembly.")], (1190, 260, W - 96, 0))
        return image

    # Chapter 8: when the module is missing.
    if step_id.startswith("missing-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "If the module does not appear")
        bullets(image, MISSING, active={0, 1} if index == 1 else {2})
        return image

    raise SystemExit(f"No slide for {step_id}")

"""The slides of the Test Drive Orchard Core video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"WebApplication"}

WAYS = [
    ("1", "Try Orchard Core", "A demo site to explore, at try.orchardcore.net"),
    ("2", "DotNest", "Cloud-hosted Orchard Core sites that are not deleted periodically"),
    ("3", "Docker", "The Orchard Core image, run in a container on your machine"),
    ("4", "The source code", "Clone the repository and run it with the .NET SDK"),
]

DOCKER = """
docker run --name orchardcms -p 8080:80 orchardproject/orchardcore-cms-linux:latest
"""

SOURCE = """
# Clone OrchardCMS/OrchardCore from GitHub, then:
cd OrchardCore
git checkout main

# Start the web application.
cd src/OrchardCore.Cms.Web
dotnet run
"""

PROGRAM = """
var builder = WebApplication.CreateBuilder(args);

// ...

var orchardCore = builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

// ...

var app = builder.Build();

// ...

app.UseStaticFiles();

app.UseOrchardCore();

await app.RunAsync();
"""

LAYOUT = [
    ("1", "OrchardCore.Cms.Web", "The web application you ran, with Program.cs"),
    ("2", "OrchardCore.Modules", "A folder for each module, such as OrchardCore.Setup"),
    ("3", "OrchardCore.Themes", "The themes, such as TheBlogTheme and TheAdmin"),
    ("4", "docs", "The documentation sources"),
]

DATA = """
OrchardCore.Cms.Web/
  App_Data/
    tenants.json      # the settings of the tenants
    Sites/
      Default/
        OrchardCore.db  # the Sqlite database
"""


def slide(step_id, chapter_title, label):
    if step_id in ("ways-1", "ways-2", "ways-4"):
        image = slide_base(chapter_title, label, "Four ways to test drive Orchard Core")
        current = {"ways-1": 0, "ways-2": 1, "ways-4": 3}[step_id]
        bullets(image, WAYS, active={current})
        return image

    if step_id == "ways-3":
        image = slide_base(chapter_title, label, "Docker: one command")
        draw_code(image, (96, 240, W - 96, 420), "Terminal", "bash", DOCKER, size=28)
        note_cards(image, [
            ("--name orchardcms", "Names the container."),
            ("-p 8080:80", "Publishes port 80 of the container on port 8080 of your computer."),
        ], (96, 480, W - 96, 1000))
        return image

    if step_id == "source-1":
        image = slide_base(chapter_title, label, "Get the source code")
        draw_code(image, (96, 240, 1000, 640), "Terminal", "bash", SOURCE, highlight={1, 2, 3}, size=26)
        note_cards(image, [
            ("Install", ".NET SDK"),
            ("Clone", "OrchardCMS/OrchardCore"),
        ], (1040, 300, W - 96, 760))
        return image

    if step_id == "source-2":
        image = slide_base(chapter_title, label, "Start the web application")
        draw_code(image, (96, 240, 1000, 640), "Terminal", "bash", SOURCE, highlight={5, 6, 7}, size=26)
        note_cards(image, [
            ("Folder", "src/OrchardCore.Cms.Web"),
            ("Command", "dotnet run"),
        ], (1040, 300, W - 96, 760))
        return image

    if step_id == "source-3":
        image = slide_base(chapter_title, label, "Open the site")
        draw_code(image, (96, 240, 1000, 640), "Terminal", "bash", SOURCE, highlight={5, 6, 7}, size=26)
        note_cards(image, [
            ("Browser", "http://localhost:5000"),
            ("Visual Studio", "Startup project, then Ctrl+F5"),
        ], (1040, 300, W - 96, 760))
        return image

    if step_id == "host-1":
        image = slide_base(chapter_title, label, "The host is an ASP.NET Core application")
        draw_code(image, (96, 240, 1160, 1030), "Program.cs", "csharp", PROGRAM, types=TYPES, highlight={5, 6, 17}, size=26)
        note_cards(image, [
            ("AddOrchardCms", "Registers Orchard Core"),
            ("UseOrchardCore", "Adds it to the request pipeline"),
        ], (1200, 300, W - 96, 960))
        return image

    if step_id == "host-2":
        image = slide_base(chapter_title, label, "Auto Setup, registered in Program.cs")
        draw_code(image, (96, 240, 1160, 1030), "Program.cs", "csharp", PROGRAM, types=TYPES, highlight={5, 7}, size=26)
        note_cards(image, [
            ("AddSetupFeatures", "Makes Auto Setup available"),
            ("Auto Setup", "Installs a site from settings"),
        ], (1200, 300, W - 96, 960))
        return image

    if step_id == "host-3":
        image = slide_base(chapter_title, label, "Under src in the repository")
        bullets(image, LAYOUT)
        return image

    if step_id == "data-1":
        image = slide_base(chapter_title, label, "App_Data holds the site")
        draw_code(image, (96, 240, 1100, 640), "OrchardCore.Cms.Web", "bash", DATA, size=28)
        note_cards(image, [
            ("Default", "The name of the default tenant"),
            ("Sqlite", "The default of the Setup screen"),
        ], (1140, 300, W - 96, 760))
        return image

    if step_id == "data-2":
        image = slide_base(chapter_title, label, "Start over, or use another folder")
        note_cards(image, [
            ("To start over", "Stop the site, delete App_Data, and run it again."),
            ("To use another folder", "Set the ORCHARD_APP_DATA environment variable."),
        ], (96, 300, W - 96, 760))
        return image

    raise SystemExit(f"No slide for {step_id}")

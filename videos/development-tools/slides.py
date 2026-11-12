"""The slides of the Development Tools video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import MARGIN, MUTED, W, bullets, draw_code, font, note_cards, slide_base

PREVIEW_URL = "https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json"
RELEASE_URL = "https://api.nuget.org/v3/index.json"

EDITORS = [
    ("1", "Visual Studio", "The IDE for .NET developers on Windows"),
    ("2", "Visual Studio Code", "A free, cross-platform editor"),
    ("3", "JetBrains Rider", "A cross-platform IDE"),
]

EXTRAS = [
    ("1", "Lombiq Orchard Visual Studio Extension", "Utilities such as an error log watcher and a dependency injector"),
    ("2", "Orchard Dojo Library snippets", "Generate code for common module and theme scenarios"),
    ("3", "Orchard Dojo Library tips", "More recommended extensions, and more on using Visual Studio"),
]

DATABASE = [
    ("1", "DB Browser for SQLite", "A free, open-source tool to browse SQLite database files"),
    ("2", "App_Data, Sites, the tenant's name", "Each tenant has a folder, such as Default, with its database file"),
    ("3", "OrchardCore.db", "The name setup gives the file. A tenant with no saved name uses yessql.db"),
    ("4", "Tables, queries and JSON documents", "The Document table stores each saved object as JSON"),
]

SMTP4DEV = [
    ("1", "smtp4dev", "A small SMTP server that runs locally, installed with dotnet"),
    ("2", "Settings, Communication, Email", "The SMTP tab needs the SMTP Email Provider feature enabled"),
    ("3", "Enable SMTP Provider", "Then enter a Sender email address, a Host name and a Port number"),
    ("4", "Its web interface", "Browse the emails it processed"),
]

SOURCES = [
    ("1", "Release packages", "Published to NuGet, built from the code of a release's version tag"),
    ("2", "Preview packages", "Published to Cloudsmith by the daily build, when the main branch has changes"),
    ("3", "The Preview - CI workflow", "Can also be run manually"),
]

CARE = [
    ("1", "The development code", "Not the most stable, and can contain breaking changes"),
    ("2", "To try a fix", "For example, a fix for a bug you reported"),
    ("3", "Not in production", "Not kept forever, with no guarantee of how long one stays available"),
]

VISUAL_STUDIO = [
    ("1", "Tools, NuGet Package Manager, Package Manager Settings", "Opens the Options dialog"),
    ("2", "Package Sources", "Add a source with the plus button"),
    ("3", "Name and Source", "OrchardCorePreview, and the address of the preview feed"),
    ("4", "Update, then OK", "Confirms the source"),
]

NUGET_CONFIG = """
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
    <add key="OrchardCorePreview" value="https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json" />
  </packageSources>
  <disabledPackageSources />
</configuration>
"""

ADD_SOURCE = """
dotnet nuget add source https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json --name OrchardCorePreview
"""

INSTALL = [
    ("1", "Manage NuGet packages", "Right-click the project in Visual Studio"),
    ("2", "Include prerelease", "Preview versions carry the preview suffix, so they are prereleases"),
    ("3", "Package Source", "Select the preview source, then browse for the package and install it"),
]

LOCAL = [
    ("1", "Pack", "Build the NuGet packages from your fork or branch of the source code"),
    ("2", "Publish", "Add them to a feed: a folder, which can be on a server"),
    ("3", "Update your project", "Point its NuGet.config at the feed, and reference the version in it"),
]

PACK = """
# From the root folder of your fork or branch
dotnet pack -c Release -o c:\\OrchardCoreNuget
"""

INIT = """
# Add the packages to the feed
nuget init c:\\OrchardCoreNuget \\\\{YourServer}\\NuGetServer
"""

LOCAL_CONFIG = """
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="MyFeed" value="\\\\{YourServer}\\NuGetServer" />
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <disabledPackageSources />
</configuration>
"""

VERSION = [
    ("1", "The version of the source you packed", "Every package takes it, so they all have the same version"),
    ("2", "Set in the common build properties", "A version prefix and a version suffix, in OrchardCore.Commons.props"),
    ("3", "OrchardCore.Application.Cms.Targets", "Bundles the modules and themes as package references, at that version"),
]

REFERENCE = """
<!-- The version of the packages in your feed -->
<PackageReference Include="OrchardCore.Application.Cms.Targets" Version="YOUR_VERSION" />
"""

TEMPLATE = """
dotnet new occms -n MySite --orchard-version YOUR_VERSION
"""


def caption(image, text, y):
    """A line of text under the cards."""
    ImageDraw.Draw(image).text((MARGIN, y), text, font=font(32), fill=MUTED, anchor="lm")


def slide(step_id, chapter_title, label):
    if step_id == "tools-1":
        image = slide_base(chapter_title, label, "Use the tool you prefer")
        bullets(image, EDITORS)
        caption(image, "Any tool that edits source files and builds a .NET application works.", 830)
        return image

    if step_id == "tools-2":
        image = slide_base(chapter_title, label, "Optional extras for Visual Studio")
        bullets(image, EXTRAS)
        return image

    if step_id == "tools-3":
        image = slide_base(chapter_title, label, "Browse the database of a tenant")
        bullets(image, DATABASE)
        return image

    if step_id == "tools-4":
        image = slide_base(chapter_title, label, "Test email on your machine")
        bullets(image, SMTP4DEV)
        return image

    if step_id == "sources-1":
        image = slide_base(chapter_title, label, "Where Orchard Core packages come from")
        bullets(image, SOURCES)
        return image

    if step_id == "sources-2":
        image = slide_base(chapter_title, label, "Use preview packages with care")
        bullets(image, CARE)
        return image

    if step_id == "feed-1":
        image = slide_base(chapter_title, label, "The preview feed")
        draw_code(image, (MARGIN, 240, W - MARGIN, 400), "Preview packages: Cloudsmith", "bash", PREVIEW_URL, size=32)
        draw_code(image, (MARGIN, 430, W - MARGIN, 590), "Release packages: NuGet", "bash", RELEASE_URL, size=32)
        note_cards(image, [
            ("Hosted on Cloudsmith", "It receives the packages of each successful preview build."),
            ("No credentials", "Restoring packages from the feed needs none."),
        ], (MARGIN, 630, W - MARGIN, 1030))
        return image

    if step_id == "vs-1":
        image = slide_base(chapter_title, label, "Add the feed in Visual Studio")
        bullets(image, VISUAL_STUDIO)
        return image

    if step_id == "config-1":
        image = slide_base(chapter_title, label, "Add the feed in a NuGet.config file")
        draw_code(image, (MARGIN, 240, W - MARGIN, 700), "NuGet.config", "xml", NUGET_CONFIG, size=27, highlight={5, 6})
        bullets(image, [("2", "Two package sources", "NuGet is nuget.org, and OrchardCorePreview is the preview feed")], top=760)
        return image

    if step_id == "cli-1":
        image = slide_base(chapter_title, label, "Add the feed with the .NET CLI")
        draw_code(image, (MARGIN, 240, W - MARGIN, 400), "Terminal", "bash", ADD_SOURCE, size=26)
        note_cards(image, [
            ("The feed address", "The first argument of dotnet nuget add source."),
            ("--name", "The name you choose for the source: here OrchardCorePreview."),
        ], (MARGIN, 450, W - MARGIN, 1030))
        return image

    if step_id == "use-1":
        image = slide_base(chapter_title, label, "Install a preview package")
        bullets(image, INSTALL)
        return image

    if step_id == "local-1":
        image = slide_base(chapter_title, label, "Packages from your own source code")
        bullets(image, LOCAL)
        return image

    if step_id == "local-2":
        image = slide_base(chapter_title, label, "1. Pack the source code")
        draw_code(image, (MARGIN, 240, W - MARGIN, 430), "Terminal", "bash", PACK, size=30)
        note_cards(image, [
            ("-c Release", "The Release configuration."),
            ("-o c:\\OrchardCoreNuget", "The one output folder, of your choice, that receives every package."),
        ], (MARGIN, 480, W - MARGIN, 1030))
        return image

    if step_id == "local-3":
        image = slide_base(chapter_title, label, "2. Publish to a feed")
        draw_code(image, (MARGIN, 240, W - MARGIN, 430), "Terminal", "bash", INIT, size=30)
        note_cards(image, [
            ("c:\\OrchardCoreNuget", "First: the output folder of dotnet pack."),
            ("\\\\{YourServer}\\NuGetServer", "Second: the folder of your feed, on a server."),
        ], (MARGIN, 480, W - MARGIN, 1030))
        return image

    if step_id == "local-4":
        image = slide_base(chapter_title, label, "3. Point your project at the feed")
        draw_code(image, (MARGIN, 240, W - MARGIN, 700), "NuGet.config", "xml", LOCAL_CONFIG, size=27, highlight={5, 6})
        bullets(image, [("2", "Two package sources", "MyFeed is your local feed, and NuGet is nuget.org")], top=760)
        return image

    if step_id == "local-5":
        image = slide_base(chapter_title, label, "One version for every package")
        bullets(image, VERSION)
        return image

    if step_id == "local-6":
        image = slide_base(chapter_title, label, "Reference that version")
        draw_code(image, (MARGIN, 240, W - MARGIN, 430), "MySite.csproj", "xml", REFERENCE, size=27)
        draw_code(image, (MARGIN, 470, W - MARGIN, 640), "Terminal", "bash", TEMPLATE, size=30)
        return image

    raise SystemExit(f"No slide for {step_id}")

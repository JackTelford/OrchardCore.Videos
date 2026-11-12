"""The slides of the Create an Orchard Core theme video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

INSTALL = """
dotnet new install OrchardCore.ProjectTemplates
"""

CREATE = """
dotnet new octheme -n "MyTheme.OrchardCore"
"""

CREATE_WITHOUT_LIQUID = """
dotnet new octheme -n "MyTheme.OrchardCore" --AddLiquid false
"""

ADD = """
dotnet sln add MyTheme.OrchardCore/MyTheme.OrchardCore.csproj
dotnet add Cms.Web/Cms.Web.csproj reference MyTheme.OrchardCore/MyTheme.OrchardCore.csproj
"""

TREE = """
MyTheme.OrchardCore/
  MyTheme.OrchardCore.csproj
  Manifest.cs
  Views/
    Layout.liquid
"""

TREE_WITH_THUMBNAIL = """
MyTheme.OrchardCore/
  MyTheme.OrchardCore.csproj
  Manifest.cs
  Views/
    Layout.liquid
  wwwroot/
    Theme.png
"""

STATIC_FILES = """
wwwroot/Theme.png      ->  /MyTheme.OrchardCore/Theme.png
wwwroot/css/site.css   ->  /MyTheme.OrchardCore/css/site.css
"""

PROJECT = """
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <!-- ... -->
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <!-- ... -->
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="OrchardCore.Theme.Targets" Version="..." />
    <PackageReference Include="OrchardCore.ContentManagement" Version="..." />
    <PackageReference Include="OrchardCore.DisplayManagement" Version="..." />
    <PackageReference Include="OrchardCore.ResourceManagement" Version="..." />
  </ItemGroup>

</Project>
"""

MANIFEST_GENERATED = """
using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "MyTheme.OrchardCore",
    Author = "The Orchard Core Team",
    Website = "https://orchardcore.net",
    Version = "0.0.1",
    Description = "MyTheme.OrchardCore"
)]
"""

MANIFEST_EDITED = """
using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "MyTheme",
    Author = "Jamie Chen",
    Website = "https://example.com",
    Version = "0.0.1",
    Description = "A first theme with a plain layout.",
    Tags = ["Starter"]
)]
"""

LAYOUT = """
<!DOCTYPE html>
<html lang="{{ Culture.Name }}">
<head>
    <meta charset="utf-8">
    <title>{% page_title Site.SiteName, position: "before", separator: " - " %}</title>
    {% resources type: "Meta" %}
    <!-- INSERT REQUIRED RESOURCES HERE -->
    {% resources type: "HeadLink" %}
    {% resources type: "Stylesheet" %}
    {% resources type: "HeadScript" %}
</head>
<body dir="{{ Culture.Dir }}">
    {% render_body %}
    <footer>
    {% render_section "Footer", required: false %}
    </footer>
    {% script name:"jquery", use_cdn:"true", at:"Foot" %}
    {% resources type: "FootScript" %}
</body>
</html>
"""

PARTS = [
    ("1", "Layout", "The template that wraps every page of the site."),
    ("2", "Templates", "Views that change how content and widgets are rendered."),
    ("3", "Static files", "Stylesheets, scripts and images, served from wwwroot."),
]

TWO_THEMES = [
    ("1", "Site theme", "Renders the front end that visitors see."),
    ("2", "Admin theme", "Renders the admin. Marked with an Admin tag."),
    ("3", "Design, Themes", "Lists the themes of the application and applies them."),
]

THREE_PARTS = [
    ("1", "Project file", "Marks the project as an Orchard Core theme."),
    ("2", "Manifest", "The name, author, website, version and description."),
    ("3", "Layout", "Wraps every page of the site."),
]

TEMPLATES = [
    ("check", "occms, ocmvc", "Web applications: the CMS, and a modular MVC application."),
    ("check", "ocmodulecms, ocmodulemvc", "Modules, one for each of the two."),
    ("check", "octheme", "An Orchard Core theme."),
]

LAYOUTS = [
    ("1", "Layout.liquid", "The Liquid layout that the template generates."),
    ("2", "Layout.cshtml", "A Razor layout, in place of the Liquid one."),
    ("3", "_ViewImports.cshtml", "In Views: makes the layout inherit from the Orchard Core Razor page."),
]

VIEW_IMPORTS = """
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>
"""

TOPICS = [
    ("1", "Create a theme from an HTML template", "Turn an existing design into a theme."),
    ("2", "Declare scripts and stylesheets", "Register them as resources for the layout."),
    ("3", "Create an admin theme", "Restyle the admin with a theme tagged Admin."),
]


def lines(code, *needles):
    """The 1-based numbers of the lines of a code panel that hold one of the needles."""
    return {number for number, text in enumerate(code.strip("\n").split("\n"), 1) if any(n in text for n in needles)}


def slide(step_id, chapter_title, label):
    if step_id == "theme-1":
        image = slide_base(chapter_title, label, "What a theme holds")
        bullets(image, PARTS)
        return image

    if step_id == "theme-2":
        image = slide_base(chapter_title, label, "Two themes, and the page that applies them")
        bullets(image, TWO_THEMES)
        return image

    if step_id == "theme-3":
        image = slide_base(chapter_title, label, "What the template creates")
        bullets(image, THREE_PARTS)
        return image

    if step_id == "create-1":
        image = slide_base(chapter_title, label, "1. Install the project templates")
        draw_code(image, (96, 240, W - 96, 420), "Terminal", "bash", INSTALL, size=34)
        bullets(image, TEMPLATES, top=470)
        return image

    if step_id == "create-2":
        image = slide_base(chapter_title, label, "2. Generate the theme project")
        draw_code(image, (96, 240, W - 96, 420), "Terminal", "bash", CREATE, size=34)
        note_cards(image, [
            ("Where to run it", "In the folder of your solution, next to the web application."),
        ], (96, 480, W - 96, 1030))
        return image

    if step_id == "create-3":
        image = slide_base(chapter_title, label, "3. The generated project")
        draw_code(image, (96, 240, 1000, 640), "MyTheme.OrchardCore", "bash", TREE, size=32)
        note_cards(image, [
            ("One name", "Folder, project and identifier."),
            ("Three files", "Project file, manifest, layout."),
        ], (1060, 260, W - 96, 1030))
        return image

    if step_id == "create-4":
        image = slide_base(chapter_title, label, "Optional: leave out the Liquid layout")
        draw_code(image, (96, 240, W - 96, 420), "Terminal", "bash", CREATE_WITHOUT_LIQUID, size=34)
        note_cards(image, [
            ("AddLiquid", "True by default. False leaves out Layout.liquid."),
        ], (96, 480, W - 96, 1030))
        return image

    if step_id == "create-5":
        image = slide_base(chapter_title, label, "4. Add it to the solution")
        draw_code(image, (96, 240, W - 96, 480), "Terminal", "bash", ADD, size=30)
        note_cards(image, [
            ("Where to run them", "In the folder that holds the solution file."),
            ("The reference", "Makes the web application include the theme."),
        ], (96, 540, W - 96, 1030))
        return image

    if step_id == "files-1":
        image = slide_base(chapter_title, label, "The project file")
        draw_code(image, (96, 240, W - 96, 1030), "MyTheme.OrchardCore.csproj", "xml", PROJECT,
                  highlight=lines(PROJECT, "Theme.Targets"), size=25)
        return image

    if step_id == "files-2":
        image = slide_base(chapter_title, label, "The Razor SDK")
        draw_code(image, (96, 240, W - 96, 1030), "MyTheme.OrchardCore.csproj", "xml", PROJECT,
                  highlight=lines(PROJECT, "Sdk.Razor"), size=25)
        return image

    if step_id == "files-3":
        image = slide_base(chapter_title, label, "Manifest.cs")
        draw_code(image, (96, 240, 1000, 700), "Manifest.cs", "csharp", MANIFEST_GENERATED, size=30)
        note_cards(image, [
            ("Set by the template", "Name and Description: the name you gave."),
            ("Values of the template", "Author, Website and Version."),
        ], (1060, 260, W - 96, 1030))
        return image

    if step_id == "files-4":
        image = slide_base(chapter_title, label, "Views/Layout.liquid")
        draw_code(image, (96, 240, W - 96, 1030), "Layout.liquid", "html", LAYOUT,
                  highlight=lines(LAYOUT, "page_title", "render_body", "render_section"), size=25)
        return image

    if step_id == "files-5":
        image = slide_base(chapter_title, label, "Views/Layout.liquid")
        draw_code(image, (96, 240, W - 96, 1030), "Layout.liquid", "html", LAYOUT,
                  highlight=lines(LAYOUT, "resources type", "INSERT REQUIRED"), size=25)
        return image

    if step_id == "files-6":
        image = slide_base(chapter_title, label, "Views/Layout.liquid")
        draw_code(image, (96, 240, W - 96, 1030), "Layout.liquid", "html", LAYOUT,
                  highlight=lines(LAYOUT, "script name"), size=25)
        return image

    if step_id == "manifest-1":
        image = slide_base(chapter_title, label, "Describe the theme")
        draw_code(image, (96, 240, 1120, 800), "Manifest.cs", "csharp", MANIFEST_EDITED,
                  highlight=lines(MANIFEST_EDITED, "Name", "Author", "Website", "Description", "Tags"), size=30)
        note_cards(image, [
            ("Shown on the card", "All of these values."),
        ], (1180, 260, W - 96, 1030))
        return image

    if step_id == "manifest-2":
        image = slide_base(chapter_title, label, "Name, identifier and tags")
        draw_code(image, (96, 240, 1120, 800), "Manifest.cs", "csharp", MANIFEST_EDITED,
                  highlight=lines(MANIFEST_EDITED, "Name", "Tags"), size=30)
        note_cards(image, [
            ("Name", "The title of the card."),
            ("Admin tag", "Marks an admin theme."),
            ("hidden tag", "Keeps it off the Themes page."),
        ], (1180, 260, W - 96, 1030))
        return image

    if step_id == "manifest-3":
        image = slide_base(chapter_title, label, "A thumbnail for the card")
        draw_code(image, (96, 240, 1000, 740), "MyTheme.OrchardCore", "bash", TREE_WITH_THUMBNAIL,
                  highlight=lines(TREE_WITH_THUMBNAIL, "wwwroot", "Theme.png"), size=32)
        note_cards(image, [
            ("The file", "Theme.png, in wwwroot."),
            ("Used by", "The Themes page, as the card image."),
        ], (1060, 260, W - 96, 1030))
        return image

    if step_id == "manifest-4":
        image = slide_base(chapter_title, label, "Static files")
        draw_code(image, (96, 240, W - 96, 460), "wwwroot", "bash", STATIC_FILES, size=30)
        note_cards(image, [
            ("Served under the identifier", "Stylesheets, scripts and images work the same way."),
        ], (96, 520, W - 96, 1030))
        return image

    if step_id == "next-1":
        image = slide_base(chapter_title, label, "Liquid or Razor")
        bullets(image, LAYOUTS)
        draw_code(image, (96, 800, W - 96, 960), "Views/_ViewImports.cshtml", "cshtml", VIEW_IMPORTS, size=30)
        return image

    if step_id == "next-2":
        image = slide_base(chapter_title, label, "Where to go from here")
        bullets(image, TOPICS)
        return image

    raise SystemExit(f"No slide for {step_id}")

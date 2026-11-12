"""The slides of the "Module Metadata in Your Project File" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The project file
snippets use the invented module Acme.Greetings; the shapes of the OrchardCoreThemes and OrchardCoreAttributes items are
the ones of the documentation page src/docs/guides/leverage-csproj-meta-info/README.md.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"Module"}

MANIFEST = """
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Acme Greetings",
    Author = "Acme Software",
    Website = "https://example.com",
    Version = "1.2.0",
    Description = "Greets the visitors of a site.",
    Category = "Content Management"
)]
"""

MANIFEST_COMMENTED = """
// using OrchardCore.Modules.Manifest;
//
// [assembly: Module(
//     Name = "Acme Greetings",
//     Author = "Acme Software",
//     Website = "https://example.com",
//     Version = "1.2.0",
//     Description = "Greets the visitors of a site.",
//     Category = "Content Management"
// )]
"""

INTERNALS_VISIBLE_TO = """
<ItemGroup>
  <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
    <_Parameter1>Test.$(MSBuildProjectName)</_Parameter1>
  </AssemblyAttribute>
</ItemGroup>
"""

# Lines: 1-3 the property, 5 and 20 the item group, 6 the Include, 7-16 the metadata, 17-19 the flags.
MODULE = """
<PropertyGroup>
  <AssemblyVersion>1.2.0</AssemblyVersion>
</PropertyGroup>

<ItemGroup>
  <OrchardCoreModules Include="$(MSBuildProjectName)"
                      Name="Acme Greetings"
                      ModuleType="Module"
                      Category="Content Management"
                      Priority="0"
                      Description="Greets the visitors of a site."
                      Author="Acme Software"
                      Version="$(AssemblyVersion)"
                      Website="https://example.com"
                      Dependencies="OrchardCore.Contents"
                      Tags="greetings;sample"
                      DefaultTenant="false"
                      AlwaysEnabled="false"
                      EnabledByDependencyOnly="false" />
</ItemGroup>
"""

FEATURES = """
<ItemGroup>
  <!-- Acme.Greetings is declared the same way. -->
  <OrchardCoreFeatures Include="Acme.Greetings.Banner"
                       Name="Greetings Banner"
                       Category="Content Management"
                       Priority="0"
                       Description="Shows a greeting in a banner."
                       Dependencies="Acme.Greetings"
                       DefaultTenant="false"
                       AlwaysEnabled="false"
                       EnabledByDependencyOnly="false" />
</ItemGroup>
"""

THEMES = """
<OrchardCoreThemes Include="..."
                   Name="..."
                   Base="..."
                   Category="..."
                   Priority="..."
                   Description="..."
                   Author="..."
                   Version="..."
                   Website="..."
                   Dependencies="..."
                   Tags="..."
                   DefaultTenant="..."
                   AlwaysEnabled="..."
                   EnabledByDependencyOnly="..." />
"""

ATTRIBUTES = """
<OrchardCoreAttributes Include="..."
                       Type="..."
                       ... />
"""

COMMAND = """
dotnet build -p:OrchardCoreDebugTargets=true
"""

PARTS = [
    ("1", "No feature declared", "The module itself is the only feature."),
    ("2", "A feature declared", "Only the declared features exist."),
    ("3", "Empty category, priority, or description", "The module's values are used."),
]

TYPE_KINDS = [
    ("1", "Type feature", "Declares a feature."),
    ("2", "Type theme", "Declares a theme."),
    ("3", "Any other Type", "Declares a module."),
]

RULES = [
    ("1", "At most one module", "Otherwise the build fails with OC2002."),
    ("2", "At most one theme", "Otherwise the build fails with OC3001."),
    ("3", "Never both", "A module and a theme together fail with OC3002."),
    ("check", "Any number of features", "Features are not limited."),
]

# The module snippet sits left of its notes.
PANEL = (96, 240, 1060, 960)
NOTES = (1100, 300, W - 96, 0)


def module_slide(chapter_title, label, highlight, notes):
    image = slide_base(chapter_title, label, "The OrchardCoreModules item")
    draw_code(image, PANEL, "Acme.Greetings.csproj", "xml", MODULE, highlight=highlight, size=22)
    note_cards(image, notes, NOTES)
    return image


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "The manifest of a module")
        draw_code(image, (96, 240, 1010, 720), "Manifest.cs", "csharp", MANIFEST, types=TYPES, size=24)
        note_cards(image, [
            ("Typed by hand", "Name, author, website, version."),
            ("Read by the site", "From the assembly attributes."),
        ], (1050, 300, W - 96, 0))
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "An item list becomes an attribute")
        draw_code(image, (96, 240, W - 96, 540), "Project file", "xml", INTERNALS_VISIBLE_TO, size=24)
        note_cards(image, [
            ("Declare an item", "in the project file; the build writes the attribute."),
            ("Properties", "such as the project name can fill in its values."),
        ], (96, 590, W - 96, 0))
        return image

    if step_id == "module-1":
        return module_slide(chapter_title, label, {5, 6, 20}, [
            ("Include", "The module identifier."),
            ("Usually", "The name of the project."),
            ("Project reference", "OrchardCore.Module.Targets"),
        ])

    if step_id == "module-2":
        return module_slide(chapter_title, label, set(range(7, 17)), [
            ("Name, Category, Description", "Show on the Features page."),
            ("Dependencies, Tags", "Lists, split by ; , or a space."),
        ])

    if step_id == "module-3":
        return module_slide(chapter_title, label, {2, 6, 13}, [
            ("Properties", "Fill in the values."),
            ("Include", "$(MSBuildProjectName)"),
            ("Version", "$(AssemblyVersion)"),
        ])

    if step_id == "module-4":
        return module_slide(chapter_title, label, {17, 18, 19}, [
            ("DefaultTenant", "Default tenant only."),
            ("AlwaysEnabled", "Cannot be disabled."),
            ("EnabledByDependencyOnly", "Only as a dependency."),
        ])

    if step_id == "parts-1":
        image = slide_base(chapter_title, label, "Declare features")
        draw_code(image, (96, 240, 1060, 720), "Acme.Greetings.csproj", "xml", FEATURES, size=22)
        note_cards(image, [
            ("Per feature", "Name, category, description."),
            ("Not on a feature", "Author, website, version, tags."),
        ], NOTES)
        return image

    if step_id == "parts-2":
        image = slide_base(chapter_title, label, "When a module declares features")
        bullets(image, PARTS)
        return image

    if step_id == "parts-3":
        image = slide_base(chapter_title, label, "Declare a theme")
        draw_code(image, (96, 240, 1010, 800), "Project file", "xml", THEMES, size=22)
        note_cards(image, [
            ("Base", "Names the base theme."),
            ("Theme project", "OrchardCore.Theme.Targets"),
            ("Module project", "OrchardCore.Module.Targets"),
        ], (1050, 300, W - 96, 0))
        return image

    if step_id == "parts-4":
        image = slide_base(chapter_title, label, "One list for everything")
        draw_code(image, (96, 240, W - 96, 470), "Project file", "xml", ATTRIBUTES, size=26)
        bullets(image, TYPE_KINDS, top=520)
        return image

    if step_id == "build-1":
        image = slide_base(chapter_title, label, "Comment out or delete Manifest.cs")
        draw_code(image, (96, 240, 1010, 720), "Manifest.cs", "csharp", MANIFEST_COMMENTED, types=TYPES, size=24)
        note_cards(image, [
            ("Module attribute", "Applied once to an assembly."),
            ("The project file", "declares the module."),
        ], (1050, 300, W - 96, 0))
        return image

    if step_id == "build-2":
        image = slide_base(chapter_title, label, "Three rules, checked at build")
        bullets(image, RULES)
        return image

    if step_id == "build-3":
        image = slide_base(chapter_title, label, "See what the build embeds")
        draw_code(image, (96, 240, W - 96, 420), "Terminal", "bash", COMMAND, size=30)
        note_cards(image, [
            ("Each module and each feature", "gets a message with its values."),
            ("The values", "Most of those of the item."),
        ], (96, 480, W - 96, 0))
        return image

    if step_id == "build-4":
        image = slide_base(chapter_title, label, "Where the embedding runs")
        note_cards(image, [
            ("OrchardCoreEmbeddingAfterTargets", "The embedding runs after these. Default: AfterResolveReferences."),
            ("OrchardCoreEmbeddingBeforeTargets", "The embedding runs before these. Default: GenerateAssemblyInfo."),
        ], (96, 300, W - 96, 0))
        return image

    if step_id == "build-5":
        image = slide_base(chapter_title, label, "Where the values show in the admin")
        note_cards(image, [
            ("Tools, Features", "Name, identifier, category, description, and dependencies of a feature."),
            ("Design, Themes", "Name, description, author, website, tags, and version of a theme."),
        ], (96, 300, W - 96, 0))
        return image

    raise SystemExit(f"No slide for {step_id}")

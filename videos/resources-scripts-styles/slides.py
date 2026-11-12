"""The slides of the Resources video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"ResourceManifest", "IConfigureOptions", "ResourceManagementOptions", "ResourceManagementOptionsConfiguration",
         "StartupBase", "IServiceCollection", "IResourceManager", "MetaEntry", "HtmlString", "MyService"}

PROBLEMS = [
    ("1", "The same library twice", "Two views include the same file, and the page loads it twice."),
    ("2", "The wrong order", "A script runs before the library it depends on."),
    ("3", "Tags everywhere", "Every view decides where its own tags go."),
]

STEPS = [
    ("1", "Declare", "A name, a type, a version, and its dependencies."),
    ("2", "Include", "By name, from a Razor or Liquid view, or from code."),
    ("3", "Render", "Once, in dependency order, where the layout asks."),
]

MANIFEST = """
var manifest = new ResourceManifest();

manifest
    .DefineScript("MyTheme")
    .SetDependencies("jQuery")
    .SetUrl("~/MyTheme/js/scripts.min.js", "~/MyTheme/js/scripts.js")
    .SetVersion("1.0.0");

manifest
    .DefineStyle("MyTheme")
    .SetUrl("~/MyTheme/css/styles.min.css", "~/MyTheme/css/styles.css")
    .SetVersion("1.0.0");
"""

DEFINITION = """
manifest
    .DefineScript("MyTheme")
    .SetDependencies("jQuery")
    .SetUrl("~/MyTheme/js/scripts.min.js", "~/MyTheme/js/scripts.js")
    .SetCdn("https://cdn.example.com/my-theme/scripts.min.js",
            "https://cdn.example.com/my-theme/scripts.js")
    .SetVersion("1.0.0");
"""

CONFIGURATION = """
public sealed class ResourceManagementOptionsConfiguration
    : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;
    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();
        // ... define the resources
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
"""

STARTUP = """
public override void ConfigureServices(IServiceCollection serviceCollection)
{
    serviceCollection.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
}
"""

INCLUDE_LIQUID = """
{% script name:"bootstrap", at:"Foot" %}
{% style name:"bootstrap" %}
"""

INCLUDE_RAZOR = """
@addTagHelper *, OrchardCore.ResourceManagement

<script asp-name="bootstrap" at="Foot"></script>
<style asp-name="bootstrap"></style>
"""

VERSION_LIQUID = """
{% script name:"bootstrap", version:"5", at:"Foot" %}
"""

VERSION_RAZOR = """
<script asp-name="bootstrap" version="5" at="Foot"></script>
"""

DEPENDS_LIQUID = """
{% script name:"bootstrap", depends_on:"jQuery", at:"Foot" %}
"""

DEPENDS_RAZOR = """
<script asp-name="bootstrap" depends-on="jQuery" at="Foot"></script>
"""

AT_VALUES = [
    ("1", "Head", "Scripts go to HeadScript, and stylesheets to Stylesheet."),
    ("2", "Foot", "Scripts go to FootScript."),
    ("3", "Inline", "The resource is rendered where the tag is."),
    ("4", "Not set", "A script renders where the tag is. A stylesheet goes to the head."),
]

LAYOUT_LIQUID = """
<head>
    {% resources type: "Meta" %}
    {% resources type: "HeadLink" %}
    {% resources type: "Stylesheet" %}
    {% resources type: "HeadScript" %}
</head>
<body>
    ...
    {% resources type: "FootScript" %}
</body>
"""

LAYOUT_RAZOR = """
@addTagHelper *, OrchardCore.ResourceManagement

<head>
    <resources type="Header" />
</head>
<body>
    ...
    <resources type="FootScript" />
</body>
"""

DECLARE_LIQUID = """
{% script src:"~/MyTheme/js/gallery.min.js", at:"Foot" %}
{% script name:"menu", src:"~/MyTheme/js/menu.min.js", at:"Foot" %}
{% style name:"menu", src:"~/MyTheme/css/menu.min.css" %}
"""

DECLARE_RAZOR = """
<script asp-src="~/MyTheme/js/gallery.min.js" at="Foot"></script>
<script asp-name="menu" asp-src="~/MyTheme/js/menu.min.js" at="Foot"></script>
<style asp-name="menu" asp-src="~/MyTheme/css/menu.min.css"></style>
"""

BLOCK_LIQUID = """
{% scriptblock at: "Foot" %}
    console.log("Ready");
{% endscriptblock %}

{% styleblock at: "Head" %}
    .menu { margin: 0; }
{% endstyleblock %}
"""

BLOCK_RAZOR = """
<script at="Foot">
    console.log("Ready");
</script>

<style at="Head">
    .menu { margin: 0; }
</style>
"""

NAMED_LIQUID = """
{% scriptblock name: "menu-ready", at: "Foot", depends_on: "jQuery" %}
    $(function () { $(".menu").addClass("ready"); });
{% endscriptblock %}
"""

NAMED_RAZOR = """
<script asp-name="menu-ready" at="Foot" depends-on="jQuery">
    $(function () { $(".menu").addClass("ready"); });
</script>
"""

LINK_LIQUID = """
{% link rel:"icon", type:"image/png", sizes:"16x16", src:"~/MyTheme/favicon/favicon-16x16.png" %}
"""

LINK_RAZOR = """
<link asp-src="~/MyTheme/favicon/favicon-16x16.png" rel="icon" type="image/png" sizes="16x16" />
"""

LINK_OUTPUT = """
<link href="/MyTheme/favicon/favicon-16x16.png" rel="icon" sizes="16x16" type="image/png" />
"""

META_LIQUID = """
{% meta name:"keywords", content:"orchard" %}
{% meta name:"keywords", content:"modules" %}
"""

META_RAZOR = """
<meta asp-name="keywords" content="orchard" />
<meta asp-name="keywords" content="modules" />
"""

META_OUTPUT = """
<meta content="orchard, modules" name="keywords" />
"""

SERVICE = """
using OrchardCore.ResourceManagement;

public sealed class MyService
{
    private readonly IResourceManager _resourceManager;

    public MyService(IResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    public void RequireBootstrap()
    {
        _resourceManager
            .RegisterResource("script", "bootstrap")
            .UseVersion("5")
            .AtFoot();
    }
}
"""

CUSTOM = """
// resourceManager is the injected IResourceManager.
resourceManager.RegisterHeadScript(new HtmlString("<script>alert('Hello')</script>"));
resourceManager.RegisterFootScript(new HtmlString("<script>alert('Hello')</script>"));

resourceManager.RegisterMeta(new MetaEntry { Name = "generator", Content = "Orchard" });
resourceManager.AppendMeta(new MetaEntry { Name = "keywords", Content = "orchard" }, ",");
"""


def notes(image, lines, top, size=30, step=64):
    """Short sentences under the code panels, each with a green dot."""
    draw = ImageDraw.Draw(image)
    y = top
    for line in lines:
        draw.ellipse((100, y - 7, 114, y + 7), fill=GREEN)
        draw.text((136, y), line, font=font(size), fill=TEXT, anchor="lm")
        y += step


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without a resource manager")
        bullets(image, PROBLEMS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The Resources module")
        bullets(image, STEPS)
        return image

    if step_id == "declare-1":
        image = slide_base(chapter_title, label, "A resource: name, type, version")
        draw_code(image, (96, 240, 1140, 780), "ResourceManagementOptionsConfiguration.cs", "csharp", MANIFEST,
                  types=TYPES, size=24)
        note_cards(image, [
            ("Name", "MyTheme"),
            ("Type", "Script or stylesheet"),
            ("Version", "Optional: 1.0.0"),
        ], (1190, 260, W - 96, 900))
        return image

    if step_id == "declare-2":
        image = slide_base(chapter_title, label, "Define it with a chain of calls")
        draw_code(image, (96, 240, 1190, 640), "ResourceManagementOptionsConfiguration.cs", "csharp", DEFINITION,
                  types=TYPES, size=24)
        note_cards(image, [
            ("SetUrl", "Minified file, then debug file"),
            ("Tilde", "Stands for the site's base path"),
            ("SetCdn", "Used when the CDN is enabled"),
        ], (1240, 260, W - 96, 900))
        return image

    if step_id == "declare-3":
        image = slide_base(chapter_title, label, "Register the manifest")
        draw_code(image, (96, 240, W - 96, 790), "ResourceManagementOptionsConfiguration.cs", "csharp", CONFIGURATION,
                  types=TYPES, size=22)
        draw_code(image, (96, 810, W - 96, 1020), "Startup.cs", "csharp", STARTUP, types=TYPES, size=22)
        return image

    if step_id == "include-1":
        image = slide_base(chapter_title, label, "Include a resource by name")
        draw_code(image, (96, 240, W - 96, 450), "Any Liquid view", "html", INCLUDE_LIQUID, size=34)
        draw_code(image, (96, 490, W - 96, 800), "Any Razor view", "cshtml", INCLUDE_RAZOR, size=34)
        return image

    if step_id == "include-2":
        image = slide_base(chapter_title, label, "Pick a version")
        draw_code(image, (96, 240, 1150, 430), "Any Liquid view", "html", VERSION_LIQUID, size=26)
        draw_code(image, (96, 470, 1150, 660), "Any Razor view", "cshtml", VERSION_RAZOR, size=26)
        note_cards(image, [
            ("No version", "The highest declared version"),
            ("version: 5", "The highest 5.x version"),
            ("version: 5.3", "The highest 5.3.x version"),
        ], (1200, 260, W - 96, 900))
        return image

    if step_id == "include-3":
        image = slide_base(chapter_title, label, "Dependencies come with the resource")
        draw_code(image, (96, 240, 1150, 430), "Any Liquid view", "html", DEPENDS_LIQUID, size=26)
        draw_code(image, (96, 470, 1150, 660), "Any Razor view", "cshtml", DEPENDS_RAZOR, size=26)
        note_cards(image, [
            ("In the declaration", "Bootstrap needs Popper"),
            ("In the tag", "depends_on adds more names"),
            ("Rendered", "Dependencies, then Bootstrap"),
        ], (1200, 260, W - 96, 900))
        return image

    if step_id == "include-4":
        image = slide_base(chapter_title, label, "The at option")
        bullets(image, AT_VALUES)
        return image

    if step_id == "render-1":
        image = slide_base(chapter_title, label, "The layout renders the resources")
        draw_code(image, (96, 240, 940, 700), "Layout.liquid", "html", LAYOUT_LIQUID, size=26)
        draw_code(image, (980, 240, W - 96, 700), "Layout.cshtml", "cshtml", LAYOUT_RAZOR, size=26)
        notes(image, [
            "Header renders Meta, HeadLink, Stylesheet, and HeadScript.",
            "Footer renders FootScript.",
        ], 780)
        return image

    if step_id == "view-1":
        image = slide_base(chapter_title, label, "Declare a resource in a view")
        draw_code(image, (96, 240, W - 96, 450), "Any Liquid view", "html", DECLARE_LIQUID, size=24)
        draw_code(image, (96, 480, W - 96, 690), "Any Razor view", "cshtml", DECLARE_RAZOR, size=24)
        notes(image, [
            "A source only: the file is included.",
            "A name and a source: declared, and required because of at.",
            "A stylesheet with a name and a source: required at once.",
        ], 760)
        return image

    if step_id == "view-2":
        image = slide_base(chapter_title, label, "Scripts and styles written in the view")
        draw_code(image, (96, 240, 940, 700), "Any Liquid view", "html", BLOCK_LIQUID, size=26)
        draw_code(image, (980, 240, W - 96, 700), "Any Razor view", "cshtml", BLOCK_RAZOR, size=26)
        notes(image, [
            "By default, a script block goes to the foot.",
            "By default, a style block follows the stylesheets.",
        ], 780)
        return image

    if step_id == "view-3":
        image = slide_base(chapter_title, label, "A named block is a resource")
        draw_code(image, (96, 240, W - 96, 470), "Any Liquid view", "html", NAMED_LIQUID, size=26)
        draw_code(image, (96, 500, W - 96, 730), "Any Razor view", "cshtml", NAMED_RAZOR, size=26)
        notes(image, [
            "It renders once, however many views ask.",
            "It is written after the resources it depends on.",
        ], 810)
        return image

    if step_id == "links-1":
        image = slide_base(chapter_title, label, "The link tag")
        draw_code(image, (96, 240, W - 96, 400), "Any Liquid view", "html", LINK_LIQUID, size=24)
        draw_code(image, (96, 430, W - 96, 590), "Any Razor view", "cshtml", LINK_RAZOR, size=24)
        draw_code(image, (96, 620, W - 96, 780), "Rendered by HeadLink", "html", LINK_OUTPUT, size=24)
        return image

    if step_id == "links-2":
        image = slide_base(chapter_title, label, "The meta tag")
        draw_code(image, (96, 240, 940, 450), "Any Liquid view", "html", META_LIQUID, size=26)
        draw_code(image, (980, 240, W - 96, 450), "Any Razor view", "cshtml", META_RAZOR, size=26)
        draw_code(image, (96, 490, W - 96, 670), "Rendered by Meta", "html", META_OUTPUT, size=26)
        notes(image, [
            "Tags with the same name are combined.",
            "The separator is a comma and a space, unless the tag sets another.",
        ], 750)
        return image

    if step_id == "api-1":
        image = slide_base(chapter_title, label, "Inject IResourceManager")
        draw_code(image, (96, 240, W - 96, 1010), "MyService.cs", "csharp", SERVICE, types=TYPES, size=26)
        return image

    if step_id == "api-2":
        image = slide_base(chapter_title, label, "Custom scripts and meta tags")
        draw_code(image, (96, 240, W - 96, 560), "MyService.cs", "csharp", CUSTOM, types=TYPES, size=24)
        note_cards(image, [
            ("Package", "OrchardCore.ResourceManagement.Abstractions"),
        ], (96, 620, W - 96, 800))
        return image

    raise SystemExit(f"No slide for {step_id}")

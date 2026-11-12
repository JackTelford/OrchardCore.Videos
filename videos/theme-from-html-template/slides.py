"""The slides of the "Create a Theme From an HTML Template" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the documentation page of the video, and from the source of TheBlogTheme, trimmed to what the narration
talks about.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"Startup", "StartupBase", "IServiceCollection", "ResourceManifest", "ResourceManagementOptions",
         "ResourceManagementOptionsConfiguration", "IConfigureOptions"}

# Step 02: the files of the theme project (the documentation page).
TREE = """
MyTheme/
  wwwroot/
    css/
      styles.css
    js/
      scripts.js
    img/
      ...
    favicon.ico
  Views/
  Manifest.cs
  MyTheme.csproj
"""

# Step 03: the documentation page, and TheBlogTheme.
RESOURCES = """
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;
    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();
        s_manifest
            .DefineStyle("MyTheme")
            .SetUrl("~/MyTheme/css/styles.min.css", "~/MyTheme/css/styles.css")
            .SetVersion("1.0.0");
        s_manifest
            .DefineScript("MyTheme")
            .SetUrl("~/MyTheme/js/scripts.min.js", "~/MyTheme/js/scripts.js")
            .SetVersion("1.0.0");
    }
    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
"""

STARTUP = """
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }
}
"""

DEPENDENCY = """
// ...
s_manifest
    .DefineScript("TheBlogTheme")
    .SetDependencies("TheBlogTheme-bootstrap-bundle")
    .SetUrl("~/TheBlogTheme/js/scripts.min.js", "~/TheBlogTheme/js/scripts.js")
    .SetVersion("6.0.7");
// ...
"""

# Step 04: the template's main page (trimmed), and the layout of the documentation page.
INDEX_HTML = """
<head>
    <title>Clean Blog - Start Bootstrap Theme</title>
    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />
    <link href="css/styles.css" rel="stylesheet" />
</head>
<nav class="navbar navbar-expand-lg navbar-light" id="mainNav">
    <div class="container px-4 px-lg-5">
        <a class="navbar-brand" href="index.html">Start Bootstrap</a>
        <!-- ... -->
        <div class="collapse navbar-collapse" id="navbarResponsive">
            <ul class="navbar-nav ms-auto py-4 py-lg-0">
                <li class="nav-item">
                    <a class="nav-link px-lg-3 py-3 py-lg-4"
                       href="index.html">Home</a>
                </li>
                <!-- ... -->
            </ul>
        </div>
    </div>
</nav>
<!-- Page header and sample posts -->
<footer class="border-top"> <!-- ... --> </footer>
<script src="js/scripts.js"></script>
"""

LAYOUT_HEAD = """
<!DOCTYPE html>
<html lang="{{ Culture.Name }}">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <title>{{ "PageTitle" | shape_new | shape_stringify }}</title>
    {% resources type: "Meta" %}
    {% link type: "image/x-icon", rel: "shortcut icon", href: "~/MyTheme/favicon.ico" %}
    {% style name: "MyTheme" %}
    {% script name: "MyTheme", at: "Foot" %}
    {% resources type: "HeadScript" %}
    {% resources type: "HeadLink" %}
    {% resources type: "Stylesheet" %}
    {% render_section "HeadMeta", required: false %}
</head>
"""

LAYOUT_BODY = """
<body dir="{{ Culture.Dir }}">
    <nav class="navbar navbar-expand-lg" id="mainNav">
        <div class="container">
            <a class="navbar-brand" href="{{ '~/' | href }}">{{ Site.SiteName }}</a>
            {% shape "menu", alias: "alias:main-menu" %}
        </div>
    </nav>
    {% render_section "Header", required: false %}
    <div class="container">
        {% render_section "Messages", required: false %}
        {% render_body %}
    </div>
    <footer>
        {% render_section "Footer", required: false %}
    </footer>
    {% resources type: "FootScript" %}
</body>
</html>
"""

LAYOUT_RAZOR = """
<head>
    <!-- ... -->
    <link type="image/x-icon" rel="shortcut icon" href="~/MyTheme/favicon.ico" />
    <style asp-name="MyTheme"></style>
    <script asp-name="MyTheme" at="Foot"></script>
    <resources type="HeadScript" />
    <resources type="HeadLink" />
    <resources type="Stylesheet" />
</head>
<body dir="@Orchard.CultureDir()">
    <menu alias="alias:main-menu" />
    @await RenderSectionAsync("Header", required: false)
    <div class="container">
        @await RenderBodyAsync()
    </div>
    <resources type="FootScript" />
</body>
"""

# The menu templates of TheBlogTheme (trimmed).
MENU = """
<div class="collapse navbar-collapse" id="navbarResponsive">
    <ul class="navbar-nav ms-auto py-4 py-lg-0">
        {% for item in Model.Items %}
        {{ item | shape_render }}
        {% endfor %}
    </ul>
</div>
"""

MENU_ITEM = """
<li class="nav-item {% if Model.HasItems %}dropdown{% endif %}">
    {% shape_clear_alternates Model %}
    {% shape_type Model "MenuItemLink" %}
    {{ Model | shape_render }}
    <!-- ... -->
</li>
"""

# Step 06: the post heading of the template, and of TheBlogTheme; the template of the documentation page; placement.
POST_HEADING_HTML = """
<div class="post-heading">
    <h1>Man must explore, and this is exploration at its greatest</h1>
    <h2 class="subheading">Problems look mighty small from 150 miles up</h2>
    <!-- ... -->
</div>
"""

POST_HEADING_LIQUID = """
<div class="post-heading">
    <h1>{{ Model.ContentItem.DisplayText }}</h1>
    <h2 class="subheading">{{ Model.ContentItem.Content.BlogPost.Subtitle.Text }}</h2>
    <!-- ... -->
</div>
"""

POST_LIQUID = """
<article>
    <h1>{{ Model.ContentItem.DisplayText }}</h1>
    {{ Model.Content.HtmlBodyPart | shape_render }}
</article>
"""

PLACEMENT = """
{
  "TitlePart": [{
    "differentiator": "TitlePart",
    "place": "-"
  }]
}
"""

# Step 07: the blog recipe of TheBlogTheme (trimmed).
RECIPE = """
{
  "name": "Blog",
  "displayName": "Blog",
  "description": "Provides a functional Blog with CMS features.",
  "issetuprecipe": true,
  "categories": [ "default" ],
  "tags": [ "blog" ],
  "steps": [
    {
      "name": "feature",
      "enable": [ "OrchardCore.Layers", /* ... */ "TheBlogTheme" ]
    },
    { "name": "themes", "admin": "TheAdmin", "site": "TheBlogTheme" },
    // ...
    {
      "name": "settings",
      "LayerSettings": { "Zones": [ "Content", "Footer" ] }
    }
    // ...
  ]
}
"""

BOX_FULL = (96, 240, W - 96, 1030)


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A template is a finished design")
        bullets(image, [
            ("1", "HTML pages", "An index page and inner pages, filled with sample text."),
            ("2", "Stylesheet and scripts", "The CSS and JavaScript that give the pages their look."),
            ("3", "Images", "The backgrounds, the photos, and the site icon."),
        ])
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "TheBlogTheme started as a template")
        bullets(image, [
            ("1", "The template", "Clean Blog, from Start Bootstrap."),
            ("2", "The theme", "TheBlogTheme, a theme that ships with Orchard Core."),
            ("3", "Its source code", "A real reference for every step of this video."),
        ])
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "What you need")
        bullets(image, [
            ("1", "A CMS application", "An Orchard Core site to run the theme."),
            ("2", "An empty theme project", "Added to the solution, and referenced by the site."),
            ("3", "The HTML template", "With its CSS, JavaScript, and images."),
        ])
        return image

    if step_id == "why-4":
        image = slide_base(chapter_title, label, "Four tasks")
        bullets(image, [
            ("1", "Copy the assets", "Into the wwwroot folder of the theme."),
            ("2", "Declare the resources", "The stylesheets and scripts, by name."),
            ("3", "Build the layout", "From the main page of the template."),
            ("4", "Template the content", "One template for each content type."),
        ])
        return image

    if step_id == "assets-1":
        image = slide_base(chapter_title, label, "Copy the assets into wwwroot")
        draw_code(image, (96, 240, 900, 920), "MyTheme", "xml", TREE, size=26, highlight={2, 3, 4, 5, 6, 7, 8, 9})
        note_cards(image, [
            ("Stylesheet", "wwwroot/css"),
            ("Scripts", "wwwroot/js"),
            ("Images and icon", "wwwroot/img, wwwroot/favicon.ico"),
        ], (960, 260, W - 96, 960))
        return image

    if step_id == "assets-2":
        image = slide_base(chapter_title, label, "Served under the name of the theme")
        draw_code(image, (96, 240, 900, 920), "MyTheme", "xml", TREE, size=26, highlight={3, 4})
        note_cards(image, [
            ("In the project", "wwwroot/css/styles.css"),
            ("In the browser", "/MyTheme/css/styles.css"),
        ], (960, 260, W - 96, 960))
        return image

    if step_id == "res-1":
        image = slide_base(chapter_title, label, "Why named resources")
        bullets(image, [
            ("check", "Version hashes", "Local files get a version hash, so a changed file is loaded again."),
            ("check", "Minified and debug files", "A resource names a minified file and a readable one."),
            ("check", "CDN addresses", "A resource can name an address on a CDN as well."),
            ("check", "Dependency order", "A resource lists the resources that must load before it."),
        ])
        return image

    if step_id == "res-2":
        image = slide_base(chapter_title, label, "Declare the resources")
        draw_code(image, BOX_FULL, "ResourceManagementOptionsConfiguration.cs", "csharp", RESOURCES, types=TYPES, size=22,
                  highlight={3, 9, 10, 11, 12, 13, 14, 15, 16})
        return image

    if step_id == "res-3":
        image = slide_base(chapter_title, label, "Register them in Startup")
        draw_code(image, (96, 240, W - 96, 700), "Startup.cs", "csharp", STARTUP, types=TYPES, size=26)
        note_cards(image, [
            ("Registered once", "The theme asks for the resources by name."),
        ], (96, 760, W - 96, 940))
        return image

    if step_id == "res-4":
        image = slide_base(chapter_title, label, "Declare a dependency")
        draw_code(image, (96, 240, W - 96, 620), "TheBlogTheme/ResourceManagementOptionsConfiguration.cs", "csharp",
                  DEPENDENCY, types=TYPES, size=26, highlight={2, 3, 4, 5, 6})
        note_cards(image, [
            ("SetDependencies", "Lists the resources that load before this one."),
        ], (96, 690, W - 96, 930))
        return image

    if step_id == "layout-1":
        image = slide_base(chapter_title, label, "Start from the main page")
        draw_code(image, (96, 240, 1110, 1030), "index.html", "html", INDEX_HTML, size=22)
        note_cards(image, [
            ("Keep the markup", "The design is the point."),
            ("Replace what is hardcoded", "Tags, page content, and menu."),
            ("Add zones", "Where widgets can appear."),
        ], (1150, 250, W - 96, 1030))
        return image

    if step_id == "layout-2":
        image = slide_base(chapter_title, label, "Use the declared resources")
        draw_code(image, (96, 240, W - 96, 930), "Views/Layout.liquid", "html", LAYOUT_HEAD, size=26, highlight={9, 10})
        return image

    if step_id == "layout-3":
        image = slide_base(chapter_title, label, "Keep the resources tags")
        draw_code(image, (96, 240, W - 96, 930), "Views/Layout.liquid", "html", LAYOUT_HEAD, size=26,
                  highlight={7, 11, 12, 13})
        return image

    if step_id == "layout-4":
        image = slide_base(chapter_title, label, "Page content and menu")
        draw_code(image, (96, 240, W - 96, 1000), "Views/Layout.liquid", "html", LAYOUT_BODY, size=26, highlight={5, 11})
        return image

    if step_id == "layout-5":
        image = slide_base(chapter_title, label, "Zones for widgets")
        draw_code(image, (96, 240, W - 96, 1000), "Views/Layout.liquid", "html", LAYOUT_BODY, size=26,
                  highlight={8, 10, 14})
        return image

    if step_id == "layout-6":
        image = slide_base(chapter_title, label, "The same layout in Razor")
        draw_code(image, (96, 240, W - 96, 1000), "Views/Layout.cshtml", "cshtml", LAYOUT_RAZOR, size=26)
        return image

    if step_id == "menu-1":
        image = slide_base(chapter_title, label, "Style the menu")
        draw_code(image, (96, 240, W - 96, 600), "TheBlogTheme/Views/Menu.liquid", "html", MENU, size=26)
        draw_code(image, (96, 640, W - 96, 1000), "TheBlogTheme/Views/MenuItem.liquid", "html", MENU_ITEM, size=26)
        return image

    if step_id in ("content-1", "content-2"):
        image = slide_base(chapter_title, label, "One file per content type" if step_id == "content-1"
                           else "How the file name is read")
        bullets(image, [
            ("1", "Content-BlogPost.liquid", "A BlogPost content item, in detail view."),
            ("2", "Content-BlogPost.Summary.liquid", "A BlogPost content item, in a list."),
            ("3", "Content-Article.liquid", "An Article content item, in detail view."),
            ("4", "Widget-Paragraph.liquid", "A Paragraph widget."),
        ], active=None if step_id == "content-1" else {0, 1})
        return image

    if step_id == "content-3":
        image = slide_base(chapter_title, label, "Replace sample text with data")
        draw_code(image, (96, 240, W - 96, 520), "post.html", "html", POST_HEADING_HTML, size=26)
        draw_code(image, (96, 560, W - 96, 840), "TheBlogTheme/Views/Content-BlogPost.liquid", "html", POST_HEADING_LIQUID,
                  size=26)
        return image

    if step_id == "content-4":
        image = slide_base(chapter_title, label, "Render parts with shape_render")
        draw_code(image, (96, 240, 1110, 640), "Views/Content-Article.liquid", "html", POST_LIQUID, size=24)
        note_cards(image, [
            ("Model.Content", "The shapes of the parts and fields."),
            ("shape_render", "Writes one shape where you want it."),
        ], (1150, 250, W - 96, 700))
        return image

    if step_id == "content-5":
        image = slide_base(chapter_title, label, "Tune the output with placement")
        draw_code(image, (96, 240, 1110, 640), "placement.json", "json", PLACEMENT, size=26)
        note_cards(image, [
            ("TitlePart", "The shape the rule targets."),
            ("place: -", "A dash hides the shape."),
        ], (1150, 250, W - 96, 700))
        return image

    if step_id == "content-6":
        image = slide_base(chapter_title, label, "Where to look next")
        bullets(image, [
            ("1", "Templates reference", "Template names, and the properties each template receives."),
            ("2", "TheBlogTheme", "Its Views folder shows the templates in use."),
        ])
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "A recipe for the theme")
        draw_code(image, BOX_FULL, "TheBlogTheme/Recipes/blog.recipe.json", "json", RECIPE, size=23,
                  highlight={9, 10, 11, 12, 13, 15, 16, 17, 18})
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "Listed on the setup screen")
        draw_code(image, BOX_FULL, "TheBlogTheme/Recipes/blog.recipe.json", "json", RECIPE, size=23,
                  highlight={2, 3, 4, 5})
        return image

    if step_id == "summary-1":
        image = slide_base(chapter_title, label, "From template to theme")
        bullets(image, [
            ("1", "Assets and resources", "Copy the files to wwwroot, and declare them by name."),
            ("2", "Layout", "Convert the main page into Layout.liquid."),
            ("3", "Content templates", "Convert the inner pages, and tune them with placement."),
            ("4", "Setup recipe", "Optional: the theme works out of the box."),
        ])
        return image

    raise SystemExit(f"No slide for {step_id}")

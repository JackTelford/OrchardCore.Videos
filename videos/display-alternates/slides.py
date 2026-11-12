"""The slides of the Shape Alternates video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"ShapeTableProvider", "ShapeTableBuilder", "ValueTask", "ContentItem", "AliasPart", "ContentAliasShapeTableProvider",
         "StartupBase", "ContentAliasStartup", "RequireFeatures", "IServiceCollection"}

# The Content template of a theme, with two changes: the banner image, and the word under the title.
TEMPLATE = """
{% zone "Header" %}
    <header class="masthead" style="background-image: url('{{ "~/TheBlogTheme/assets/img/IMAGE.jpg" | href }}')">
        <div class="container position-relative px-4 px-lg-5">
            <div class="row gx-4 gx-lg-5 justify-content-center">
                <div class="col-md-10 col-lg-8 col-xl-7">
                    <div class="site-heading">
                        <h1>{{ Model.ContentItem.DisplayText }}</h1>
                        <span class="subheading">WORDS</span>
                    </div>
                </div>
            </div>
        </div>
    </header>
{% endzone %}

{{ Model.Content | shape_render }}
"""
# The lines that differ from the Content template of the theme.
TEMPLATE_CHANGES = {2, 8}

NAMING = """
# File name                                  Shape name
Content-Event.liquid                         Content__Event
Content-Event.Summary.liquid                 Content_Summary__Event
Content-Slug-events-night-market.liquid      Content__Slug__events__night__market
"""

TYPE_NAMES = """
# Shape name                  File name
Content__Event                Content-Event.liquid
Content_Summary__Event        Content-Event.Summary.liquid
"""

ITEM_NAMES = """
# Shape name                                  File name
Content__Slug__events__night__market          Content-Slug-events-night-market.liquid
Content__Alias__spring__fair                  Content-Alias-spring-fair.liquid
"""

PATH_TO_FILE = """
Path of the item     events/night-market
Shape name           Content__Slug__events__night__market
File name            Content-Slug-events-night-market.liquid
"""

COMMENTS = """
<!--shape-start type:Content bindings:content__slug__events__night__market => .../Content-Slug-events-night-market.liquid (liquid) -->
...
<!--shape-end type:Content -->
"""

LIQUID_TAG = """
{% shape "menu", alias: "alias:main-menu", alternate: "Menu__Footer" %}
"""

RAZOR_TAGS = """
<menu alias="alias:main-menu" alternate="Menu__Footer" />

<menu alias="alias:main-menu">
    <metadata>
        <clear-alternates />
        <add-alternate name="Menu__Footer" />
    </metadata>
</menu>
"""

PROVIDER = """
public sealed class ContentAliasShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Content")
            .OnDisplaying(displaying =>
            {
                var shape = displaying.Shape;
                var contentItem = shape.GetProperty<ContentItem>("ContentItem");

                if (contentItem?.TryGet<AliasPart>(out var aliasPart) == true)
                {
                    // ...
                    displaying.Shape.Metadata.Alternates.AddRange(cachedAlternates);
                }
            });

        return ValueTask.CompletedTask;
    }
}
"""

REGISTER = """
[RequireFeatures("OrchardCore.Contents")]
public sealed class ContentAliasStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddShapeTableProvider<ContentAliasShapeTableProvider>();
    }
}
"""


def dotted(image, lines, x, y, step=64, size=30):
    """Lines of text, each with a green dot."""
    draw = ImageDraw.Draw(image)
    for line in lines:
        draw.ellipse((x, y - 7, x + 14, y + 7), fill=GREEN)
        draw.text((x + 36, y), line, font=font(size), fill=TEXT, anchor="lm")
        y += step


def template(image, box, name, picture, words, highlight=None):
    code = TEMPLATE.replace("IMAGE", picture).replace("WORDS", words)
    draw_code(image, box, name, "html", code, highlight=highlight, size=24)


def slide(step_id, chapter_title, label):
    if step_id == "choose-1":
        image = slide_base(chapter_title, label, "From a shape to a template")
        bullets(image, [
            ("1", "A page is made of shapes", "The layout, content items, fields and menus are shapes."),
            ("2", "Each shape has a type", "A content item on its own page has the type Content, unless its content type has a stereotype."),
            ("3", "A template renders the type", "The template named Content shows those items."),
        ])
        return image

    if step_id == "choose-2":
        image = slide_base(chapter_title, label, "Alternates are tried before the type")
        bullets(image, [
            ("1", "The last alternate added", "It is tried first."),
            ("2", "The alternates before it", "They are tried next, in the reverse order of adding."),
            ("3", "The shape type", "It is used when no alternate has a template."),
        ])
        return image

    if step_id == "choose-3":
        image = slide_base(chapter_title, label, "From a file name to a shape name")
        draw_code(image, (96, 240, W - 96, 600), "Views", "bash", NAMING, size=28)
        dotted(image, [
            "A hyphen stands for a double underscore.",
            "A dot after the last hyphen adds the display type.",
            "Razor files, ending in .cshtml, follow the same rules.",
        ], 110, 700)
        return image

    if step_id == "choose-4":
        image = slide_base(chapter_title, label, "Where the templates live")
        bullets(image, [
            ("1", "A module", "Provides a default template, such as Content.cshtml."),
            ("2", "The active theme", "A template of the same name in its Views folder replaces it."),
            ("3", "A more specific name", "Targets one content type or one content item."),
        ])
        return image

    if step_id == "names-1":
        image = slide_base(chapter_title, label, "One content type")
        draw_code(image, (96, 240, W - 96, 520), "Views", "bash", TYPE_NAMES, size=28)
        dotted(image, [
            "Content-Event: every Event shown on its own page.",
            "Content-Event.Summary: every Event shown in the Summary display type.",
        ], 110, 620)
        return image

    if step_id == "names-2":
        image = slide_base(chapter_title, label, "One content item")
        draw_code(image, (96, 240, W - 96, 520), "Views", "bash", ITEM_NAMES, size=28)
        dotted(image, [
            "Slug: needs the Autoroute part. It uses the path of the item.",
            "Alias: needs the Alias part. It uses the alias of the item.",
        ], 110, 620)
        return image

    if step_id == "names-3":
        image = slide_base(chapter_title, label, "Which alternate is tried first")
        bullets(image, [
            ("1", "The path and the alias", "Added after the type alternate, so they are tried first."),
            ("2", "The content type", "Added first, when the item is built: Content__Event."),
            ("3", "The shape type", "Content. It is used when no alternate has a template."),
        ])
        return image

    if step_id in ("type-1", "type-2"):
        image = slide_base(chapter_title, label, "One template for every Event")
        template(image, (96, 240, W - 96, 960), "Views/Content-Event.liquid", "home-bg", "Event",
                 highlight=TEMPLATE_CHANGES if step_id == "type-2" else None)
        return image

    if step_id == "item-1":
        image = slide_base(chapter_title, label, "From the path to the file name")
        draw_code(image, (96, 240, W - 96, 520), "Views", "bash", PATH_TO_FILE, size=28)
        dotted(image, [
            "The path keeps its hyphens, and its slashes become hyphens too.",
        ], 110, 620)
        return image

    if step_id == "item-2":
        image = slide_base(chapter_title, label, "One template for one Event")
        template(image, (96, 240, W - 96, 960), "Views/Content-Slug-events-night-market.liquid", "contact-bg",
                 "Featured event", highlight=TEMPLATE_CHANGES)
        return image

    if step_id == "output-1":
        image = slide_base(chapter_title, label, "What the comments say")
        draw_code(image, (96, 240, W - 96, 520), "Page source", "html", COMMENTS, size=22)
        dotted(image, [
            "type: the shape type.",
            "bindings: the name of the template, its file, and its language.",
            "Here the name comes from the path of the item.",
        ], 110, 620)
        return image

    if step_id == "own-1":
        image = slide_base(chapter_title, label, "Add an alternate when a shape is created")
        draw_code(image, (96, 240, W - 96, 400), "Layout.liquid", "html", LIQUID_TAG, size=24)
        draw_code(image, (96, 430, W - 96, 840), "Layout.cshtml", "html", RAZOR_TAGS, size=24)
        dotted(image, ["The template for Menu__Footer is Menu-Footer.liquid."], 110, 910)
        return image

    if step_id == "own-2":
        image = slide_base(chapter_title, label, "Add the alternate before the shape renders")
        bullets(image, [
            ("1", "Create the shape", "A tag, a tag helper or code creates it."),
            ("2", "Add the alternate", "Do it before the shape is rendered."),
            ("3", "Orchard Core picks the template", "Then it runs that template, which cannot change the choice."),
        ])
        return image

    if step_id == "own-3":
        image = slide_base(chapter_title, label, "Add alternates for a shape type")
        draw_code(image, (96, 240, W - 96, 1030), "ContentAliasShapeTableProvider.cs", "csharp", PROVIDER, types=TYPES, size=22)
        return image

    if step_id == "own-4":
        image = slide_base(chapter_title, label, "Register the provider")
        draw_code(image, (96, 240, 1190, 620), "Startup.cs", "csharp", REGISTER, types=TYPES, size=22)
        note_cards(image, [
            ("Least specific first", "The last one added is tried first."),
            ("Placement files", "can add alternates too."),
            ("Debug information", "shows which one won."),
        ], (1230, 240, W - 96, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")

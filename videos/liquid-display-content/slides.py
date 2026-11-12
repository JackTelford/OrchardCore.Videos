"""The slides of the "Liquid: Display Content Items, Shapes and Zones" video. assemble.py calls slide() for each step of
a "slide" clip.

Everything visual comes from brand.py. The Liquid code is taken from The Blog Theme (src/OrchardCore.Themes/TheBlogTheme/Views
in OrchardCMS/OrchardCore) and from the Liquid documentation, and trimmed: <!-- ... --> marks what is left out, since
// ... is not Liquid. brand.py has no Liquid lexer, so the panels use the html one.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, MARGIN, MUTED, PANEL, TEXT, W, bullets, draw_code, font, mono, slide_base

BOTTOM = 1030
CARD_INNER = W - 2 * MARGIN - 140 - 40  # the width of the text in a card of bullets()

# The Blog Post template: Content-BlogPost.liquid.
POST_TEMPLATE = """
{% zone "Header" %}
    <!-- ... -->
    <h1>{{ Model.ContentItem.DisplayText }}</h1>
    <h2 class="subheading">{{ Model.ContentItem.Content.BlogPost.Subtitle.Text }}</h2>
    <!-- ... -->
{% endzone %}

{{ Model.Content.ContentsMetadata | shape_render }}
{{ Model.Content.MarkdownBodyPart | shape_render }}
{{ Model.Content.BlogPost-Category | shape_render }}
{{ Model.Content.BlogPost-Tags | shape_render }}
"""

# The Blog template: Content-Blog.liquid.
BLOG_TEMPLATE = """
{% zone "Header" %}
    <!-- ... -->
    <h1>{{ Model.ContentItem.DisplayText }}</h1>
    <span class="subheading">{{ Model.Content.HtmlBodyPart | shape_render }}</span>
    <!-- ... -->
{% endzone %}

{% shape_remove_item Model.Content "HtmlBodyPart" %}
{% shape_remove_item Model.Content "Blog-Image" %}

{{ Model.Content | shape_render }}
"""

READ_SHAPE = """
{{ Model.ContentItem.DisplayText }}
{{ Model.Content | shape_render }}
{{ Model.Content.MarkdownBodyPart | shape_render }}
"""

ZONE_TAG = """
{% zone "Header" %}
    <!-- ... -->
    <h1>{{ Model.ContentItem.DisplayText }}</h1>
    <!-- ... -->
{% endzone %}
"""

# Layout.liquid.
LAYOUT = """
{% render_section "Header", required: false %}
<div class="container px-4 px-lg-5 mb-4">
    <!-- ... -->
    {% render_section "Messages", required: false %}
    {% render_body %}
    <!-- ... -->
</div>
<footer class="border-top">
    {% render_section "Footer", required: false %}
</footer>
"""

LAYOUT_TAG = """
{% layout "CustomLayout" %}
"""

# Blog-ListPart.liquid.
LIST_PART = """
{% for item in Model.ContentItems %}
    {{ item | shape_build_display: "Summary" | shape_render }}
{% endfor %}
<!-- ... -->
{{ Model.Pager | shape_render }}
"""

LOAD_ITEM = """
{% assign post = Content["slug:blog/post-1"] %}
{{ post | shape_build_display: "Summary" | shape_render }}
"""

ITEM_TAG = """
{% contentitem handle: "slug:blog/post-1", display_type: "Summary" %}

{% assign posts = postIds | content_item_id %}
"""

# Content-BlogPost.liquid.
DATE_SHAPE = """
{% assign format = "MMMM dd, yyyy" | t %}
{% assign dateTime = "DateTime" | shape_new: utc: Model.ContentItem.CreatedUtc, format: format | shape_stringify %}
"""

# Layout.liquid.
TITLE_SHAPE = """
<title>{{ "PageTitle" | shape_new | shape_stringify }}</title>
"""

# Layout.liquid, on several lines.
MENU_SHAPE = """
{% shape "menu",
    alias: "alias:main-menu",
    cache_id: "main-menu",
    cache_fixed_duration: "00:05:00",
    cache_tag: "alias:main-menu",
    cache_context: "user.roles" %}
"""

ADD_ALTERNATE = """
{% assign post = Content["slug:blog/post-1"] %}
{% assign summary = post | shape_build_display: "Summary" %}
{% shape_add_alternates summary "Content_Summary__Featured" %}
{{ summary | shape_render }}
"""


def wrap(draw, text, face, width):
    """The lines of text that fit in width."""
    lines, line = [], ""
    for word in text.split():
        trial = f"{line} {word}".strip()
        if draw.textlength(trial, font=face) <= width:
            line = trial
        else:
            lines.append(line)
            line = word
    lines.append(line)
    return lines


def check_bullets(items):
    """bullets() doesn't wrap: every title and text must fit the card."""
    probe = ImageDraw.Draw(slide_base("", "", ""))
    for _, title, text in items:
        for value, face in ((title, font(36, "Bold")), (text, font(28))):
            if probe.textlength(value, font=face) > CARD_INNER:
                raise SystemExit(f"Too wide for a card: {value}")
    return items


def code(image, box, title, text, size, highlight=None):
    """A code panel of The Blog Theme's Liquid, checked to fit its box."""
    x0, y0, x1, y1 = box
    face = mono(size)
    lines = text.strip("\n").split("\n")
    widest = max(face.getlength(line) for line in lines)
    if widest > x1 - x0 - 60:
        raise SystemExit(f"{title}: a line is {widest:.0f} px wide in a panel of {x1 - x0 - 60} px")
    needed = 74 + len(lines) * int(size * 1.42) + 20
    if needed > y1 - y0:
        raise SystemExit(f"{title}: {len(lines)} lines need {needed} px in a panel of {y1 - y0} px")
    draw_code(image, box, title, "html", text, size=size, highlight=highlight)


def cards(image, items, top, height=210):
    """Cards of (title, text) side by side below a code panel."""
    draw = ImageDraw.Draw(image)
    gap = 24
    width = (W - 2 * MARGIN - gap * (len(items) - 1)) // len(items)
    if top + height > BOTTOM:
        raise SystemExit("The cards are below the slide")
    x = MARGIN
    for title, text in items:
        draw.rounded_rectangle((x, top, x + width, top + height), radius=16, fill=PANEL, outline=ACCENT, width=2)
        size = 30
        while draw.textlength(title, font=font(size, "Bold")) > width - 56 and size > 24:
            size -= 1
        if draw.textlength(title, font=font(size, "Bold")) > width - 56:
            raise SystemExit(f"Title too wide for a card: {title}")
        draw.text((x + 28, top + 40), title, font=font(size, "Bold"), fill=TEXT, anchor="lm")
        y = top + 92
        for line in wrap(draw, text, font(26), width - 56):
            draw.text((x + 28, y), line, font=font(26), fill=MUTED, anchor="lm")
            y += 34
        if y - 17 > top + height - 14:
            raise SystemExit(f"Text too long for a card: {text}")
        x += width + gap


def slide(step_id, chapter_title, label):
    if step_id == "shapes-1":
        image = slide_base(chapter_title, label, "A page is built from shapes")
        bullets(image, check_bullets([
            ("1", "Shape", "An object with the data and the metadata of the thing to display."),
            ("2", "Template", "Draws a shape. It comes from a module, a theme, or the admin."),
            ("3", "Model", "In a Liquid template, the shape that the template draws."),
        ]))
        return image

    if step_id == "shapes-2":
        image = slide_base(chapter_title, label, "The zones of a Content shape")
        bullets(image, check_bullets([
            ("1", "Header", "Drawn first, in the header of the item."),
            ("2", "Meta", "Drawn in the metadata block of the header."),
            ("3", "Content", "Holds the shapes of the parts and fields."),
            ("4", "Footer", "Drawn last, in the footer of the item."),
        ]))
        return image

    if step_id == "shapes-3":
        image = slide_base(chapter_title, label, "Reading a shape in Liquid")
        code(image, (MARGIN, 240, W - MARGIN, 490), "From The Blog Theme", READ_SHAPE, 30)
        cards(image, [
            ("Model.ContentItem", "The content item that the shape displays."),
            ("Model.Content", "The Content zone, kept as a property of the shape."),
            ("Model.Content.MarkdownBodyPart", "The shape of one part, found by name."),
        ], 540, 250)
        return image

    if step_id == "content-1":
        image = slide_base(chapter_title, label, "Draw the shapes of a content item")
        code(image, (MARGIN, 240, W - MARGIN, 760), "Content-BlogPost.liquid", POST_TEMPLATE, 26, highlight={1, 3, 6, 9})
        cards(image, [
            ("The zone tag", "Puts the page header in the Header zone of the layout."),
            ("Named shapes", "The part shapes are found by name and drawn with shape_render."),
            ("Only what it names", "A shape that the template does not draw does not appear."),
        ], 800)
        return image

    if step_id == "content-2":
        image = slide_base(chapter_title, label, "Move a shape, then draw the rest")
        code(image, (MARGIN, 240, W - MARGIN, 760), "Content-Blog.liquid", BLOG_TEMPLATE, 26, highlight={4, 8, 9, 11})
        cards(image, [
            ("Draw it in the header", "The body part is drawn inside the Header zone."),
            ("Remove it", "shape_remove_item removes a shape by name from a zone."),
            ("Draw the rest", "Model.Content | shape_render draws what remains."),
        ], 800)
        return image

    if step_id == "content-3":
        image = slide_base(chapter_title, label, "Where a template lives")
        bullets(image, check_bullets([
            ("1", "Content-BlogPost.liquid", "A theme file: the Detail display of a Blog Post."),
            ("2", "Content__BlogPost", "In the admin: Design, Templates. Needs the Manage templates and Manage Liquid templates permissions."),
            ("3", "Content_Summary__BlogPost", "The Summary display: Content-BlogPost.Summary.liquid in a theme."),
            ("4", "The admin comes first", "For the same name, an admin template is used before a theme file."),
        ]))
        return image

    if step_id == "zones-1":
        image = slide_base(chapter_title, label, "The zone tag")
        code(image, (MARGIN, 240, W - MARGIN, 560), "Content-Blog.liquid", ZONE_TAG, 28)
        cards(image, [
            ("Renders in place", "The block runs where the tag is, and writes nothing there."),
            ("Adds to a zone", "The result goes to the zone with the name you give."),
            ("Orders by position", "An optional position argument orders the content."),
        ], 600, 230)
        return image

    if step_id == "zones-2":
        image = slide_base(chapter_title, label, "The layout draws the zones")
        code(image, (MARGIN, 240, W - MARGIN, 745), "Layout.liquid", LAYOUT, 28)
        cards(image, [
            ("render_section", "Writes the zone with the name you give it."),
            ("required: true", "Raises an error when the zone is empty."),
            ("render_body", "Writes the Content zone, which holds the page."),
        ], 785, 220)
        return image

    if step_id == "zones-3":
        image = slide_base(chapter_title, label, "The order of rendering")
        bullets(image, check_bullets([
            ("1", "The Content zone", "Your template runs. Its zone tags fill the other zones."),
            ("2", "The other zones", "Each remaining zone of the layout is drawn."),
            ("3", "The Layout shape", "The layout template runs, and render_section writes the zones."),
        ]))
        return image

    if step_id == "zones-4":
        image = slide_base(chapter_title, label, "The layout tag")
        code(image, (MARGIN, 240, W - MARGIN, 420), "A shape template", LAYOUT_TAG, 32)
        cards(image, [
            ("Adds an alternate", "The name is added to the alternates of the Layout shape of the theme."),
            ("Chooses the layout", "A template with that name draws the page, when it exists."),
        ], 470, 230)
        return image

    if step_id == "items-1":
        image = slide_base(chapter_title, label, "Draw another content item")
        code(image, (MARGIN, 240, W - MARGIN, 560), "Blog-ListPart.liquid", LIST_PART, 28)
        cards(image, [
            ("shape_build_display", "Builds the display shape of a content item."),
            ("The display type", "An argument. Detail when it is omitted; lists use Summary."),
            ("shape_render", "Draws the shape as HTML."),
        ], 600, 230)
        return image

    if step_id == "items-2":
        image = slide_base(chapter_title, label, "Load an item by handle")
        code(image, (MARGIN, 240, W - MARGIN, 440), "A Liquid template", LOAD_ITEM, 28)
        cards(image, [
            ("alias:", "The alias of an item that has an Alias part."),
            ("slug:", "The path of an item that has an Autoroute part."),
            ("contentitemid:", "The identifier of a content item."),
        ], 490, 230)
        return image

    if step_id == "items-3":
        image = slide_base(chapter_title, label, "One item, or a list")
        code(image, (MARGIN, 240, W - MARGIN, 500), "A Liquid template", ITEM_TAG, 28)
        cards(image, [
            ("contentitem", "Loads an item by handle and draws it."),
            ("display_type", "Optional. Detail when it is omitted."),
            ("content_item_id", "Loads a list of items from their identifiers."),
        ], 540, 230)
        return image

    if step_id == "make-1":
        image = slide_base(chapter_title, label, "Create a shape")
        code(image, (MARGIN, 240, W - MARGIN, 450), "Content-BlogPost.liquid", DATE_SHAPE, 24)
        code(image, (MARGIN, 470, W - MARGIN, 630), "Layout.liquid", TITLE_SHAPE, 28)
        cards(image, [
            ("shape_new", "Creates a shape from its type. Named arguments become properties."),
            ("Pascal case", "utc becomes Utc, and format becomes Format."),
            ("shape_stringify", "Draws the shape and returns the HTML as text."),
        ], 670, 230)
        return image

    if step_id == "make-2":
        image = slide_base(chapter_title, label, "Create and draw: the shape tag")
        code(image, (MARGIN, 240, W - MARGIN, 600), "Layout.liquid", MENU_SHAPE, 28)
        cards(image, [
            ("The type", "The first argument is the type of the shape."),
            ("Properties", "Named arguments, such as alias, become properties."),
            ("Caching", "cache_id and the other cache arguments cache the result."),
        ], 640, 230)
        return image

    if step_id == "make-3":
        image = slide_base(chapter_title, label, "Tags that change a shape")
        bullets(image, check_bullets([
            ("1", "Alternates", "shape_add_alternates and shape_clear_alternates"),
            ("2", "Wrappers, classes, attributes", "shape_add_wrappers, shape_add_classes, shape_add_attributes, and a clear tag for each"),
            ("3", "Type, display type, position, tab", "shape_type, shape_display_type, shape_position, shape_tab"),
            ("4", "Properties", "shape_add_properties and shape_remove_property"),
        ]))
        return image

    if step_id == "make-4":
        image = slide_base(chapter_title, label, "Add an alternate to a shape")
        code(image, (MARGIN, 240, W - MARGIN, 530), "A Liquid template", ADD_ALTERNATE, 28)
        cards(image, [
            ("The last one wins", "The alternates added last are tried first."),
            ("No such template", "The earlier alternates are used."),
            ("Template names", "Content-Featured.Summary.liquid in a theme, or Content_Summary__Featured in the admin."),
        ], 570, 250)
        return image

    raise SystemExit(f"No slide for {step_id}")

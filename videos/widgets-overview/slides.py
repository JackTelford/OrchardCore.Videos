"""The slides of the Widgets overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WIDGET = [
    ("1", "A content item", "It has fields to fill in, such as text, an image or a quote."),
    ("2", "A content type with a stereotype", "The type carries the Widget stereotype. That is what makes it a widget type."),
    ("3", "Ready-made types", "A site set up with the Blog recipe has Blockquote, Container, Image, Paragraph and Raw Html."),
]

ZONES = [
    ("1", "Named zones", "A theme lays out each page in zones, such as Header, Content and Footer in The Blog Theme."),
    ("2", "A widget goes into a zone", "The theme shows the widgets of a zone wherever it renders that zone."),
    ("3", "Names that match the theme", "The zone names you choose must match the zones of your theme."),
]

WAYS = [
    ("1", "The WidgetsList part", "Each item of a content type chooses its own widgets, zone by zone. This video covers it."),
    ("2", "Layers", "A layer shows its widgets on every page that meets a condition. A separate video covers layers."),
    ("3", "The Flow part", "A flow builds the body of an item from widgets. A separate video covers it."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "A widget is a content item")
        bullets(image, WIDGET)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Widgets live in zones")
        bullets(image, ZONES)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "Ways to place widgets")
        bullets(image, WAYS)
        return image

    raise SystemExit("No slide for " + step_id)

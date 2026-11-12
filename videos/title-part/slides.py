"""The slides of the Title part video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

KINDS = [
    ("1", "Content type", "A kind of content, such as a product."),
    ("2", "Content item", "One entry of a content type."),
    ("3", "Display text", "The name the admin shows in its lists."),
]

TITLE_PART = [
    ("1", "Without a title", "The list shows the name of the content type."),
    ("check", "The Title part", "Gives a content type a title."),
    ("check", "Display text", "The title is stored as the display text."),
]

BEHAVIORS = [
    ("1", "Title is editable", "Editors type the title."),
    ("2", "Title is editable and required", "Editors must type a title before the item is saved."),
    ("3", "Title is generated and input is disabled", "A pattern builds the title. The field is shown, disabled."),
    ("4", "Title is generated and input is hidden", "A pattern builds the title. The field is not shown."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Display text names a content item")
        bullets(image, KINDS)
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The Title part sets the display text")
        bullets(image, TITLE_PART)
        return image
    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The Behavior option")
        bullets(image, BEHAVIORS)
        return image
    raise SystemExit("No slide for " + step_id)

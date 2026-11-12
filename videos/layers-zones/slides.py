"""The slides of the Layers video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PARTS = [
    ("1", "Zone", "A named area of the page layout, such as Content or Footer."),
    ("2", "Widget", "A block of content, such as a paragraph or an image."),
    ("3", "Layer", "Holds a rule that decides on which pages its widgets appear."),
]

FLOW = [
    ("1", "A visitor opens a page", "Layers apply to the pages of the site, not to the admin."),
    ("2", "The rules of layers with widgets are checked", "Every condition of a rule must be true for its layer to pass."),
    ("3", "Widgets appear in their zones", "Only the published widgets of the layers that pass."),
]

RULES = [
    ("1", "Every condition must be true", "A rule with several conditions needs all of them."),
    ("2", "All and Any groups", "An All group needs every condition in it, an Any group needs only one."),
    ("3", "No conditions, no widgets", "A rule without conditions is never true."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Three parts of a page")
        bullets(image, PARTS)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "What happens for each page")
        bullets(image, FLOW)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "How a rule decides")
        bullets(image, RULES)
        return image

    raise SystemExit(f"No slide for {step_id}")

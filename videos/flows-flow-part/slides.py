"""The slides of the Flow part video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

PROBLEM = [
    ("1", "Mixed content", "A landing page mixes text, quotes, images, and other blocks."),
    ("2", "One body field", "A single field gives editors one block of text, with no way to arrange blocks."),
    ("3", "Editors in control", "Editors should arrange the blocks of a page without asking a developer."),
]

FLOW_PART = [
    ("1", "A list of widgets", "The Flow part adds a list of widgets to a content item."),
    ("2", "Widgets are content items", "A widget is a content item whose type has the Widget stereotype setting."),
    ("3", "Saved with the page", "The widgets are saved inside the page itself."),
]

SIZE_AND_ALIGNMENT = [
    ("1", "Size", "25%, 33%, 50%, 66%, 75%, or 100% of the width."),
    ("2", "Alignment", "Left, Center, Right, Justify, or Inherit."),
    ("3", "The theme lays them out", "The theme decides how widgets of different sizes sit next to each other."),
]

NEEDS = [
    ("1", "The Flows feature", "Enable it under Tools, Features. It depends on the Widgets feature, which provides the Widget stereotype."),
    ("2", "The Blog recipe", "It enables both features when a site is set up."),
    ("3", "Widget types", "Paragraph, Blockquote, Image, Raw Html, and Container."),
    ("4", "The Flow part on a type", "The Page type has a Flow part, and the Container widget has one too."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "One body field is not enough")
        bullets(image, PROBLEM)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The Flow part")
        bullets(image, FLOW_PART)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Size and alignment")
        bullets(image, SIZE_AND_ALIGNMENT)
        return image

    if step_id == "need-1":
        image = slide_base(chapter_title, label, "What a site needs")
        bullets(image, NEEDS, active={0, 1})
        return image

    if step_id == "need-2":
        image = slide_base(chapter_title, label, "What a site needs")
        bullets(image, NEEDS, active={2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")

"""The slides of the Bag part video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

# What a bag is: bag-1 and bag-2 share the cards, and each step brightens the ones it talks about.
IDEA = [
    ("1", "Holds content items", "Each item is a content item with its own parts and fields."),
    ("2", "You choose the types", "The bag accepts only the content types selected in its settings."),
    ("3", "Edited in place", "Editors add, move, and remove items in the page's own editor."),
    ("4", "One document", "The items are stored inside the page's own document."),
]

PARTS = [
    ("1", "Flow part", "Holds widgets, each with an alignment and a size."),
    ("2", "Bag part", "Holds items of the content types you choose, with no layout settings."),
]

SOURCE = [
    ("1", "The Flows feature", "Provides both the Flow part and the Bag part."),
    ("2", "Recipes such as Blog", "Enable Flows when the site is set up."),
    ("3", "Any other site", "Enable Flows under Tools, then Features."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("bag-1", "bag-2"):
        image = slide_base(chapter_title, label, "A Bag holds content items")
        bullets(image, IDEA, active={0, 1, 2} if step_id == "bag-1" else {0, 3})
        return image
    if step_id == "bag-3":
        image = slide_base(chapter_title, label, "Flow part and Bag part")
        bullets(image, PARTS)
        return image
    if step_id == "bag-4":
        image = slide_base(chapter_title, label, "Where the Bag part comes from")
        bullets(image, SOURCE)
        return image
    raise SystemExit(f"No slide for {step_id}")

"""The slides of the Build a Landing Page video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PARTS = [
    ("1", "Flow", "Blocks stored inside one page, arranged in the page editor"),
    ("2", "Widgets and zones", "Content blocks, and the named places of a theme where they can go"),
    ("3", "Layers", "A name and a rule decide on which pages those widgets show"),
]

CHOICE = [
    ("1", "A Flow", "Content of one page, stored in the page and edited with it"),
    ("2", "A layer", "Widgets shared by many pages, shown wherever its rule is true"),
]


def slide(step_id, chapter_title, label):
    if step_id in ("parts-1", "parts-2", "parts-3"):
        image = slide_base(chapter_title, label, "Flows, widgets and layers")
        bullets(image, PARTS, active={int(step_id[-1]) - 1})
        return image

    if step_id in ("choose-1", "choose-2"):
        image = slide_base(chapter_title, label, "Which one to use")
        bullets(image, CHOICE, active={int(step_id[-1]) - 1})
        return image

    raise SystemExit(f"No slide for {step_id}")

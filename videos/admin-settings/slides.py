"""The slides of the Admin settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

AREAS = [
    ("1", "The admin menu", "The Filter box, the New entry, and how sections open."),
    ("2", "The top bar and page headings", "Quick navigation, theme toggler, titles, and breadcrumb."),
    ("3", "Lists and actions", "The layout of rows, the layout of actions, and each person's choice."),
]


def slide(step_id, chapter_title, label):
    if step_id == "areas-1":
        image = slide_base(chapter_title, label, "One page, three areas")
        bullets(image, AREAS)
        return image

    raise SystemExit("No slide for " + step_id)

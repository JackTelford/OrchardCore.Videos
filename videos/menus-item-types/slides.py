"""The slides of the "Menu items" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

TYPES = [
    ("1", "Link Menu Item", "An address you type: a page of your site, or another site."),
    ("2", "Content Menu Item", "A content item you pick. The address comes from the item."),
    ("3", "Html Menu Item", "An address, with HTML as the text of the link."),
]

WHO_SEES = [
    ("1", "Permissions", "Link and Html items: shown to people who have the permissions."),
    ("2", "Check content permissions", "Content items: shown to people who may view the content."),
    ("3", "Sanitize Html", "Html items: the HTML is cleaned when the menu is displayed."),
]


def slide(step_id, chapter_title, label):
    if step_id == "types-1":
        image = slide_base(chapter_title, label, "Three types of menu item")
        bullets(image, TYPES)
        return image

    if step_id in ("types-2", "types-3", "types-4"):
        image = slide_base(chapter_title, label, "Who sees a menu item")
        bullets(image, WHO_SEES, active={int(step_id[-1]) - 2})
        return image

    raise SystemExit(f"No slide for {step_id}")

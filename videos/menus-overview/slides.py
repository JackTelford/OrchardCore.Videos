"""The slides of the Menus overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

NAVIGATION = [
    ("1", "The admin menu", "For the people who manage the site."),
    ("2", "Menus", "For your visitors, created with the Menu module."),
]

PARTS = [
    ("1", "Title", "The name of the menu in the admin."),
    ("2", "Alias", "The name a theme uses to ask for the menu."),
    ("3", "Menu items", "The links of the menu, arranged in a tree."),
]

TYPES = [
    ("L", "Link Menu Item", "A link to any address, inside or outside the site."),
    ("C", "Content Menu Item", "A link to a content item, such as a page."),
    ("H", "Html Menu Item", "A link whose text is HTML that you write."),
]

FLOW = [
    ("1", "Alias", "A theme usually asks for a menu by its alias."),
    ("2", "Published version", "The theme shows the published version of the menu."),
    ("3", "Publish", "Publishing a draft makes your changes reach visitors."),
]


def slide(step_id, chapter_title, label):
    if step_id == "menus-1":
        image = slide_base(chapter_title, label, "Two kinds of navigation")
        bullets(image, NAVIGATION, active={0, 1})
        return image

    if step_id == "menus-2":
        image = slide_base(chapter_title, label, "The parts of a menu")
        bullets(image, PARTS)
        return image

    if step_id == "menus-3":
        image = slide_base(chapter_title, label, "Three types of menu items")
        bullets(image, TYPES)
        return image

    if step_id == "menus-4":
        image = slide_base(chapter_title, label, "How a theme shows a menu")
        bullets(image, FLOW)
        return image

    raise SystemExit(f"No slide for {step_id}")

"""The slides of the admin tour video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

PARTS = [
    ("1", "Top bar", "Quick navigation, the theme toggle, Visit Site, and your user menu."),
    ("2", "Admin menu", "The sections and pages in the sidebar on the left."),
    ("3", "Page", "The breadcrumb, the title, and the content you work on."),
]

RULES = [
    ("check", "Its own theme", "The Admin Theme styles the admin. The theme of your site styles the front end."),
    ("check", "One permission opens it", "Users need the Access admin panel permission to open the admin."),
]

WAYS = [
    ("1", "Admin menu", "Open a section, then a page. Collapse the menu to icons."),
    ("2", "Quick navigation", "Press Ctrl+K, type part of a name, and press Enter."),
    ("3", "Breadcrumb", "Click an earlier step of the trail to go back up."),
    ("4", "Logo and site name", "Click them to return to the start page from anywhere."),
]


def slide(step_id, chapter_title, label):
    if step_id == "glance-1":
        image = slide_base(chapter_title, label, "Every admin page has three parts")
        bullets(image, PARTS)
        return image

    if step_id == "glance-2":
        image = slide_base(chapter_title, label, "Its own theme, and one permission")
        bullets(image, RULES)
        return image

    if step_id == "recap-1":
        image = slide_base(chapter_title, label, "Four ways to find your way")
        bullets(image, WAYS)
        return image

    raise SystemExit(f"No slide for {step_id}")

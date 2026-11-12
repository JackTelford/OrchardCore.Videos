"""The slides of the Admin Dashboard video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

# Chapter 1: how the dashboard works. One card per step, the others dimmed.
HOW = [
    ("1", "A widget is a content item", "Create, edit, publish and delete it like other content."),
    ("2", "A grid sets the layout", "Position, Width and Height place and size each widget."),
    ("3", "Only published widgets show", "A draft-only widget stays in the editor until you publish it."),
]

# Chapter 5: who sees the dashboard.
PERMISSIONS = [
    ("1", "Access the Admin Dashboard", "Lets the user see the dashboard."),
    ("2", "Manage the Admin Dashboard", "Use Manage Dashboard. It includes the access permission."),
]

ROLES = [
    ("check", "Administrator", "Access the Admin Dashboard and Manage the Admin Dashboard."),
    ("check", "Editor, Moderator, Author, Contributor", "Access the Admin Dashboard."),
    ("3", "Access Control, Roles", "Change what these four roles can do."),
]

VIEW = [
    ("1", "Access the Admin Dashboard", "Lets the user see the dashboard."),
    ("2", "Permission to view the widget", "A user who cannot manage the dashboard needs it for each widget."),
    ("3", "No widget to see?", "That user gets the welcome home page instead."),
]

# Chapter 6: your own widget types.
TYPES = [
    ("1", "Create a content type", "Design, Content Definition, Content Types, Create new type."),
    ("2", "Set its Stereotype", "Enter DashboardWidget in the Stereotype field."),
    ("3", "Add the parts you need", "For example a title and a body."),
    ("check", "It joins Add Widget", "A part for the position and size is added for you."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "A start page made of widgets")
        bullets(image, HOW, active={0})
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "A start page made of widgets")
        bullets(image, HOW, active={1})
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "A start page made of widgets")
        bullets(image, HOW, active={2})
        return image

    if step_id == "who-1":
        image = slide_base(chapter_title, label, "Two permissions")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "who-2":
        image = slide_base(chapter_title, label, "Who has them by default")
        bullets(image, ROLES)
        return image

    if step_id == "who-3":
        image = slide_base(chapter_title, label, "A widget also needs view permission")
        bullets(image, VIEW, active={1, 2})
        return image

    if step_id == "types-1":
        image = slide_base(chapter_title, label, "A widget type is a content type")
        bullets(image, TYPES, active={0, 1})
        return image

    if step_id == "types-2":
        image = slide_base(chapter_title, label, "A widget type is a content type")
        bullets(image, TYPES, active={2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")

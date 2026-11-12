"""The slides of the Features video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

MODULES = [
    ("1", "Modules", "Orchard Core is divided into modules."),
    ("2", "Features", "Each module provides one or more features."),
    ("3", "Enabled features", "Your site runs only the features that are enabled."),
    ("4", "Setup recipe", "The recipe chosen at setup enables the starting features."),
]

ADDS = [
    ("check", "Admin menu entries", "Pages for using and managing the feature."),
    ("check", "Settings pages", "Options you can change for your site."),
    ("check", "Permissions", "Rules for which roles can use the feature."),
    ("0", "Disabled features", "Add none of these, so the admin stays focused."),
]

DEPENDENCIES = [
    ("1", "Enable a feature", "The features it needs are enabled with it."),
    ("2", "Disable a feature", "The features that need it are disabled with it."),
]

PERMISSION = [
    ("check", "Features page", "Only roles with Manage Features can open it."),
    ("check", "Administrator", "The Administrator role has the permission by default."),
    ("check", "Other roles", "Grant it by editing the role under Access Control, Roles."),
]


def slide(step_id, chapter_title, label):
    if step_id == "what-1":
        image = slide_base(chapter_title, label, "Modules provide features")
        bullets(image, MODULES, active={0, 1})
        return image

    if step_id == "what-2":
        image = slide_base(chapter_title, label, "Your site runs the enabled features")
        bullets(image, MODULES, active={2, 3})
        return image

    if step_id == "what-3":
        image = slide_base(chapter_title, label, "What an enabled feature adds")
        bullets(image, ADDS)
        return image

    if step_id == "what-4":
        image = slide_base(chapter_title, label, "Features can depend on one another")
        bullets(image, DEPENDENCIES)
        return image

    if step_id == "who-1":
        image = slide_base(chapter_title, label, "The Manage Features permission")
        bullets(image, PERMISSION)
        return image

    raise SystemExit(f"No slide for {step_id}")

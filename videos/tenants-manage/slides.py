"""The slides of the tenants-manage video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

STATES = [
    ("1", "Uninitialized", "Created, not set up. Offers Setup, Edit, Reload, and Remove if allowed."),
    ("2", "Running", "Serves its site. Offers Edit, View and Reload, and Disable except on Default."),
    ("3", "Disabled", "Switched off, settings and data kept. Offers Enable, Edit, Reload, and Remove if allowed."),
    ("4", "Initializing", "The tenant is being set up."),
]

REMOVAL = [
    ("1", "Off by default", "The configuration of the application has to allow removal."),
    ("2", "Disabled or not set up", "A running tenant has to be disabled first."),
    ("3", "Deleted for good", "Its settings, its files and its database content."),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("states-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "The states of a tenant")
        # The first step talks about Uninitialized and Running, the second about Disabled and Initializing, the third about all.
        active = {0, 1} if index == 1 else {2, 3} if index == 2 else None
        bullets(image, STATES, active=active)
        return image

    if step_id.startswith("rules-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "Before you remove a tenant")
        bullets(image, REMOVAL, active={0} if index == 1 else {1, 2})
        return image

    raise SystemExit(f"No slide for {step_id}")

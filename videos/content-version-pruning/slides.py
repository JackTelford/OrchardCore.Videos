"""The slides of the Content Version Pruning video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PILE_UP = [
    ("1", "Publish a change", "The version it replaces stays in the database."),
    ("2", "That copy is an archived version", "Neither the most recent version nor the published one."),
    ("3", "Versionable content types", "A type that supports versions keeps the versions it replaces."),
]

KEPT_AND_DELETED = [
    ("check", "Always kept", "The published version and the most recent version."),
    ("check", "Kept by your rules", "The newest archived versions of each item, and versions within the retention period."),
    ("×", "Deleted", "Archived versions of the chosen types, older than the retention period and beyond the ones kept."),
]

THREE_STEPS = [
    ("1", "Enable the features", "Content Version Pruning and Background Tasks, on the Features page."),
    ("2", "Choose the rules", "Retention period, versions to keep, and content types."),
    ("3", "Enable the background task", "It runs once a day by default."),
    ("!", "Pruning is permanent", "A deleted version cannot be restored."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "Old versions pile up")
        bullets(image, PILE_UP)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What pruning keeps and deletes")
        bullets(image, KEPT_AND_DELETED)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "Three steps")
        bullets(image, THREE_STEPS)
        return image

    raise SystemExit("No slide for " + step_id)

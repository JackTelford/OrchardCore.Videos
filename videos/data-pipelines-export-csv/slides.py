"""The slides of the Data Pipelines export video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

STEPS = [
    ("1", "Source", "Reads rows: content items, users or saved queries."),
    ("2", "Transform", "Optional. Filters, sorts, groups and joins the rows."),
    ("3", "File", "Writes the rows to a CSV file, a workbook, JSON or JSON Lines."),
    ("4", "Destination", "Delivers the file, for example to the media library."),
]

WHO = [
    ("1", "Publish", "The pipeline keeps the access of the user who published it."),
    ("2", "Run", "Each run reads and writes what that user may read and write."),
]

PERMISSIONS = [
    ("1", "Manage data pipelines", "Create, design, publish and delete pipelines. Security critical."),
    ("2", "Run data pipelines", "Run published pipelines, and cancel their runs."),
    ("3", "View data pipelines and their runs", "List the pipelines, open them read-only, and see their runs."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "A pipeline is a chain of steps")
        bullets(image, STEPS, active={0})
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "A pipeline is a chain of steps")
        bullets(image, STEPS, active={1})
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "A pipeline is a chain of steps")
        bullets(image, STEPS, active={2, 3})
        return image

    if step_id == "access-1":
        image = slide_base(chapter_title, label, "A pipeline runs as the user who published it")
        bullets(image, WHO)
        return image

    if step_id == "access-2":
        image = slide_base(chapter_title, label, "Three permissions")
        bullets(image, PERMISSIONS)
        return image

    raise SystemExit(f"No slide for {step_id}")

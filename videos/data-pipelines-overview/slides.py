"""The slides of the Data Pipelines overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

MOVES = [
    ("1", "Export", "Content items as a spreadsheet."),
    ("2", "Send", "A file for another system, every night."),
    ("3", "Import", "The rows of a CSV file as content items."),
]

STEPS = [
    ("1", "Sources", "Read rows from content items, users, saved queries or a file."),
    ("2", "Transforms", "Filter, calculate, sort, group, join and remove duplicates."),
    ("3", "Files", "Write the rows to a CSV file, an Excel workbook or JSON."),
    ("4", "Destinations", "Media library, link, email, FTP, SFTP, web API, content items."),
]

MORE = [
    ("1", "Transforms", "Filter, sort, group, join, calculate and remove duplicates."),
    ("2", "Destinations", "Download links, email, FTP and SFTP servers, web APIs."),
    ("3", "Workflows", "A workflow can run a pipeline, such as on a schedule."),
]

PERMISSIONS = [
    ("1", "Manage data pipelines", "Create, design, publish and delete pipelines. Security critical."),
    ("2", "Run data pipelines", "Run published pipelines, and cancel runs."),
    ("3", "View data pipelines and their runs", "List the pipelines and read their runs."),
    ("!", "Runs use the access of the publisher", "A pipeline reads and writes what its publisher may."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Data that has to move")
        bullets(image, MOVES)
        return image

    if step_id in ("why-2", "why-3"):
        image = slide_base(chapter_title, label, "A pipeline is a set of connected steps")
        bullets(image, STEPS, active={0, 1} if step_id == "why-2" else {2, 3})
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "More than a first export")
        bullets(image, MORE)
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "Who can use pipelines")
        bullets(image, PERMISSIONS)
        return image

    raise SystemExit("No slide for " + step_id)

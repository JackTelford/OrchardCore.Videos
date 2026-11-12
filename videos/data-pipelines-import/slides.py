"""The slides of the "Data Pipelines: Import Rows as Content Items" video. assemble.py calls slide() for each step of a
"slide" clip. Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

NEEDS = [
    ("1", "A file in the media library", "For example a CSV file or an Excel workbook, one row for each item."),
    ("2", "A content type", "Its parts and fields receive the values of each row."),
    ("3", "A data pipeline", "Reads the file and saves each row as a content item."),
]

STEPS = [
    ("1", "Read a media file", "Reads the rows of the file. Each column of a CSV file is text."),
    ("2", "Save content items", "Creates or updates one content item for each row."),
]

ROWS = [
    ("1", "Update the item a row matches, or create one", "Running the import again updates the items that match."),
    ("2", "Create items; skip the rows that match one", "The items that exist stay as they are."),
    ("3", "Update the items rows match; reject the others", "A row that matches no item is not saved."),
]

REJECTED = [
    ("1", "Checked first", "Each item is validated before it is saved, as the editor would validate it."),
    ("2", "The Rejected output", "A row that fails leaves through Rejected, with the reason in a field named Error."),
]

KEEP = [
    ("1", "Rejected", "Connect this output of Save content items to Create a file."),
    ("2", "Create a file", "Writes the rows to a CSV file, with an Error column for the reason."),
    ("3", "Save to the media library", "Keeps the file, so you can fix the rows and import them again."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "What an import needs")
        bullets(image, NEEDS)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "The steps of an import")
        bullets(image, STEPS)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "What happens to a row")
        bullets(image, ROWS)
        return image

    if step_id == "rej-1":
        image = slide_base(chapter_title, label, "Rows that cannot be saved")
        bullets(image, REJECTED)
        return image

    if step_id == "rej-2":
        image = slide_base(chapter_title, label, "Keep the rejected rows")
        bullets(image, KEEP)
        return image

    raise SystemExit(f"No slide for {step_id}")

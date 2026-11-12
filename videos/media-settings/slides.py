"""The slides of the Media settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

COVERS = [
    ("1", "File types", "Which extensions are accepted, and which need an extra permission."),
    ("2", "Upload limits", "The largest file, the upload chunk size and temporary files."),
    ("3", "Caching", "How long browsers keep files, and how long resized images stay valid."),
    ("4", "Storage", "Where files are kept, and what that storage supports."),
]

PAGES = [
    ("1", "Options", "Read-only. Permission: View Media Options."),
    ("2", "API", "Editable. Needs Manage Media API authentication settings and Manage settings."),
]

RULES = [
    ("check", "Allowed file extensions", "Accepted from anyone who may upload media."),
    ("!", "Restricted file extensions", "Accepted only with an extra permission."),
    ("X", "In neither list", "Refused, whoever uploads the file."),
]


def slide(step_id, chapter_title, label):
    if step_id == "pages-1":
        image = slide_base(chapter_title, label, "What the Media settings cover")
        bullets(image, COVERS)
        return image

    if step_id == "pages-2":
        image = slide_base(chapter_title, label, "Settings, Media")
        bullets(image, PAGES)
        return image

    if step_id == "rules-1":
        image = slide_base(chapter_title, label, "Every upload is checked against two lists")
        bullets(image, RULES)
        return image

    raise SystemExit("No slide for " + step_id)

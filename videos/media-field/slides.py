"""The slides of the Media Field video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

IDEA = [
    ("1", "A location, not a copy", "The field stores where each file is. The file itself is not copied into the content item."),
    ("2", "One file, many pages", "The same image can be attached to as many content items as you like."),
    ("3", "Media text", "A description of the file. For an image, the field's display uses it as the alternative text."),
    ("4", "Anchor", "A focal point on an image that a template can keep in view when it crops."),
]

EDITORS = [
    ("1", "Standard", "Pick files from the media library in a dialog."),
    ("2", "Gallery", "The same files as cards or as a list, which you can reorder."),
    ("3", "Attached", "Upload files that belong to the content item."),
]

ATTACHED = [
    ("1", "A folder for each page", "Uploaded files move into a folder that belongs to the content item."),
    ("2", "Removed with the page", "Deleting the content item deletes its attached files."),
    ("3", "A permission of its own", "Manage Attached Media Fields Folder. Administrators have it."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "What the field holds")
        bullets(image, IDEA)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Three editors")
        bullets(image, EDITORS)
        return image

    if step_id == "attached-6":
        image = slide_base(chapter_title, label, "Good to know about attached files")
        bullets(image, ATTACHED)
        return image

    raise SystemExit(f"No slide for {step_id}")

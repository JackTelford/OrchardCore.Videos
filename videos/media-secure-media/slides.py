"""The slides of the Secure Media video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WITHOUT = [
    ("1", "Open by address", "Anyone who has the address of a file can open it."),
    ("2", "No rules per folder", "No role can be allowed to open one folder only."),
    ("3", "Sensitive files are open too", "A contract in the library is as public as a blog picture."),
]

WITH = [
    ("check", "View permissions", "A role can view all folders, the root folder, or chosen folders."),
    ("check", "Not found for everyone else", "A request without the permission gets 404 Not Found."),
]

VIEW = [
    ("1", "View media content in all folders", "Includes the root folder and each folder permission."),
    ("2", "View media content in the root folder", "The files that are not inside a folder."),
    ("3", "View others media content", "The media stored in the folders of other users."),
    ("4", "View own media content", "The media stored in the folder of the signed-in user."),
]

FOLDER = [
    ("1", "One permission per folder", "View media content in folder 'press' covers press and the folders inside it."),
    ("2", "Created with the folder", "A folder created in the root folder gets its permission."),
]

RELATED = [
    ("1", "A folder permission", "Also lets a role view the files stored directly in the root folder."),
    ("2", "Manage All Media Folders", "Includes every view permission."),
    ("3", "Manage Media", "Is required to open the Media Library."),
]

KNOW = [
    ("check", "The Media Library", "A user sees only the folders that user may view."),
    ("check", "Attached editor of a Media Field", "Files follow who may view the content item, with no folder permission."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without Secure Media")
        bullets(image, WITHOUT)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "With Secure Media")
        bullets(image, WITH)
        return image

    if step_id == "perm-1":
        image = slide_base(chapter_title, label, "The view permissions")
        bullets(image, VIEW)
        return image

    if step_id == "perm-2":
        image = slide_base(chapter_title, label, "Permissions for folders")
        bullets(image, FOLDER)
        return image

    if step_id == "perm-3":
        image = slide_base(chapter_title, label, "How they relate to other permissions")
        bullets(image, RELATED)
        return image

    if step_id.startswith("know-"):
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, KNOW, active={0} if step_id == "know-1" else {1})
        return image

    raise SystemExit(f"No slide for {step_id}")

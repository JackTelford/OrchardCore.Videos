"""The slides of the Media module video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PROBLEMS = [
    ("1", "The same picture, again", "Each page that needs a picture gets its own upload."),
    ("2", "Copies drift apart", "Nobody can tell which version is the current one."),
    ("3", "Files get lost", "Without folders or search, a stored file is hard to find."),
]

SOLUTIONS = [
    ("check", "One library", "Upload a file once and keep it in a folder."),
    ("check", "Attached to content", "A content item stores the path of the file."),
]

JOURNEY = [
    ("1", "Upload", "Add the file to the library, in a folder."),
    ("2", "Attach", "Pick the file in a media field of a content item."),
    ("3", "Show", "The site displays the file, resized by a media profile when the theme asks for one."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without a shared place for files")
        bullets(image, PROBLEMS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "One library for the files of the site")
        bullets(image, SOLUTIONS)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "From upload to page")
        bullets(image, JOURNEY)
        return image

    raise SystemExit(f"No slide for {step_id}")

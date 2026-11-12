"""The slides of the Forms validation video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PARTS = [
    ("1", "A workflow checks", "It records an error for each submitted value that fails."),
    ("2", "The form displays", "It shows the errors it receives, where the visitor can read them."),
]

PLACES = [
    ("1", "Validation Summary widget", "Lists the errors together, in one place."),
    ("2", "Validation option of an input", "Shows a message under that input."),
    ("3", "Validation widget", "Shows the message of the field you name."),
]

WAY_BACK = [
    ("1", "Save Form Location", "The form stores the page it is on."),
    ("2", "HTTP Redirect To Form Location", "The workflow sends the visitor back to that page."),
    ("3", "Error messages", "The page displays them again."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Validation has two parts")
        bullets(image, PARTS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Three ways to display errors")
        bullets(image, PLACES)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The way back to the form")
        bullets(image, WAY_BACK)
        return image

    raise SystemExit(f"No slide for {step_id}")

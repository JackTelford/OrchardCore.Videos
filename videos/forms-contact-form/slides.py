"""The slides of the "Build a Contact Form From Widgets" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

BUILDING_BLOCKS = [
    ("1", "A page that holds widgets", "For example, the Page type of the Blog recipe"),
    ("2", "The Form widget", "The form itself: where it submits, and how"),
    ("3", "Field widgets", "Input, Text Area, Select, and Button, inside the Form widget"),
]

FIELD_WIDGETS = [
    ("1", "Input", "A single-line field: text, email, phone, number, date, and more"),
    ("2", "Text Area", "A multi-line field for longer text"),
    ("3", "Select", "A list of choices: a dropdown list, checkboxes, or radio buttons"),
    ("4", "Button", "Sends the form when its type is Submit"),
]

FIELD_SETTINGS = [
    ("1", "HTML Name", "The name the value is sent under. Required."),
    ("2", "Label option", "None, Standard, or Screen Readers. None is the default."),
    ("3", "Placeholder Text", "Input and Text Area: what the empty field shows"),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Three building blocks")
        bullets(image, BUILDING_BLOCKS)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "The field widgets")
        bullets(image, FIELD_WIDGETS)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "The settings of a field")
        bullets(image, FIELD_SETTINGS, active={0, 1})
        return image

    raise SystemExit("No slide for " + step_id)

"""The slides of the Field Editors and Display Modes video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

CONCEPT = [
    ("1", "Field", "Stores one kind of value: text, a number or a date."),
    ("2", "Editor", "How the value is entered on the edit screen of a content item."),
    ("3", "Display mode", "How the value is shown to visitors on the site."),
]

TEXT_AND_NUMERIC = [
    ("check", "Text Field",
     "Standard, Code Mirror, Color, Email, Icon picker, Monaco editor, Multi-line, Phone, Predefined List, Url"),
    ("check", "Numeric Field", "Number, Range, Select, Slider, Spinner"),
]

OTHER_EDITORS = [
    ("check", "Boolean Field", "Standard, Switch"),
    ("check", "Multi Text Field", "Standard, Checkbox List, Picker"),
    ("check", "Date Field", "Standard, Localized"),
    ("check", "Date Time Field and Time Field", "Standard"),
]

DISPLAY_MODES = [
    ("check", "Text Field", "Standard, Header"),
    ("check", "User Picker Field", "Standard, User Names"),
    ("check", "Every other field of the Content Fields module", "Standard"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("concept-"):
        index = int(step_id[-1]) - 1
        image = slide_base(chapter_title, label, "A field, an editor, a display mode")
        bullets(image, CONCEPT, active={index})
        return image

    if step_id == "catalog-1":
        image = slide_base(chapter_title, label, "Editors of a Text Field and a Numeric Field")
        bullets(image, TEXT_AND_NUMERIC)
        return image

    if step_id == "catalog-2":
        image = slide_base(chapter_title, label, "More fields and their editors")
        bullets(image, OTHER_EDITORS)
        return image

    if step_id == "catalog-3":
        image = slide_base(chapter_title, label, "Display modes")
        bullets(image, DISPLAY_MODES)
        return image

    raise SystemExit(f"No slide for {step_id}")

"""The slides of the Content Parts video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

TYPE_AND_PARTS = [
    ("1", "Fields", "A field holds data, such as a line of text or a date."),
    ("2", "Parts", "A part adds something to a type. The Title part adds a title."),
    ("3", "Content types", "A type is made of parts, and editors fill it in."),
]

DEFINE_ONCE = [
    ("1", "One definition", "Create the part once, on the Content Parts page."),
    ("2", "Many types", "Any content type can attach a part that is Attachable."),
    ("3", "Many times", "A part that is Reusable can be attached to one type several times."),
]

EXAMPLE = [
    ("check", "Postal Address", "One part with three fields: Street, City and Postal Code."),
    ("check", "Billing Address", "The part, attached to Customer under this name."),
    ("check", "Shipping Address", "The same part, attached to Customer under another name."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "A type is made of parts")
        bullets(image, TYPE_AND_PARTS)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Define once, attach many times")
        bullets(image, DEFINE_ONCE)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "Example: a postal address")
        bullets(image, EXAMPLE)
        return image

    raise SystemExit("No slide for " + step_id)

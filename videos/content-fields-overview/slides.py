"""The slides of the Content Fields overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

# The path from a content type to a field, in the order of the narration.
STRUCTURE = [
    ("1", "Content type", "A kind of content on your site, such as a workshop or a product."),
    ("2", "Part", "A building block of a type. The fields you add are kept in a part named after the type."),
    ("3", "Field", "A named piece of data, with a field type, settings and an editor."),
]

# The fields of the video.
FIELDS = [
    ("Aa", "Text Field", "Text, such as a name or a short description."),
    ("#", "Numeric Field", "A number, which can be limited to a range."),
    ("check", "Boolean Field", "Yes or no, shown as a checkbox."),
    ("31", "Date Field, Date Time Field and Time Field", "A day, a day with a time, and a time of day."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2"):
        image = slide_base(chapter_title, label, "From content types to fields")
        bullets(image, STRUCTURE, active={0, 1} if step_id == "why-1" else {2})
        return image

    if step_id in ("why-3", "why-4"):
        image = slide_base(chapter_title, label, "Fields for everyday data")
        bullets(image, FIELDS, active={0, 1} if step_id == "why-3" else {2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")

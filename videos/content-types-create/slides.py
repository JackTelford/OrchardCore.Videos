"""The slides of the content types video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

MODEL = [
    ("1", "Content type", "The kind each content item belongs to, such as a Page, a Blog Post or an Event."),
    ("2", "Parts", "Behaviors you attach to a type, such as Title."),
    ("3", "Fields", "Values you define, such as a venue, a number or a date."),
    ("4", "The part named like the type", "It holds the fields you add to the type."),
]

RECAP = [
    ("1", "Create the type", "A Display Name and a Technical Name, then Create."),
    ("2", "Add parts", "Attach the behaviors you need, such as Title."),
    ("3", "Add fields", "Choose a field type for each value, and configure it."),
    ("4", "Set the order", "Drag parts and fields, then Save."),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("model-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "How a content type is built")
        bullets(image, MODEL, active={0} if index == 1 else {1} if index == 2 else {2, 3})
        return image

    if step_id == "recap-1":
        image = slide_base(chapter_title, label, "Creating a content type")
        bullets(image, RECAP)
        return image

    raise SystemExit(f"No slide for {step_id}")

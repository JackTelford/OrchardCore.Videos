"""The slides of the Autoroute video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

WITHOUT = [
    ("1", "An address built from an identifier", "Orchard Core displays the item at an address that ends with its identifier."),
    ("2", "Hard to read", "A long string of letters and digits is hard to read, to say, or to remember."),
    ("3", "Hard to guess", "Nothing in the address tells a visitor what the page is about."),
]

WITH = [
    ("1", "A part of the content type", "You attach Autoroute to a type, such as Event or Page."),
    ("2", "A pattern", "The pattern builds the permalink. By default, it uses the title."),
    ("3", "A permalink for each saved item", "Readable, in lowercase words joined by dashes, and unique."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without Autoroute")
        bullets(image, WITHOUT)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "With Autoroute")
        bullets(image, WITH)
        return image

    raise SystemExit(f"No slide for {step_id}")

"""The slides of the Audit Trail Settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHERE = [
    ("1", "Tools, Features", "Enable Audit Trail and Users Audit Trail."),
    ("2", "Settings, Audit Trail", "The Events, Trimming and Content tabs."),
    ("3", "Settings, Security, User Audit Trail", "User Properties to Store."),
]

OPTIONS = [
    ("1", "Store", "Keeps the value as text."),
    ("2", "PartialAsteriskRedactor", "Keeps the first and last character, and hides the rest with asterisks."),
    ("3", "ErasingRedactor", "Stores an empty value in place of the real value."),
    ("4", "HmacRedactor", "Stores a hash of the value. Offered only when a key is configured."),
]


def slide(step_id, chapter_title, label):
    if step_id == "where-1":
        image = slide_base(chapter_title, label, "Two features, two settings pages")
        bullets(image, WHERE, active={0})
        return image

    if step_id == "where-2":
        image = slide_base(chapter_title, label, "Two features, two settings pages")
        bullets(image, WHERE, active={1, 2})
        return image

    if step_id == "options-1":
        image = slide_base(chapter_title, label, "Ways to store a user property")
        bullets(image, OPTIONS, active={0, 1})
        return image

    if step_id == "options-2":
        image = slide_base(chapter_title, label, "Ways to store a user property")
        bullets(image, OPTIONS, active={2, 3})
        return image

    raise SystemExit("No slide for " + step_id)

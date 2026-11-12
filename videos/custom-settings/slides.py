"""The slides of the Custom Settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. This is an
admin video: the slides name what the screens show, and hold no code.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

SITE_SETTINGS = [
    ("1", "Site-wide values", "They belong to the whole site, not to one page."),
    ("2", "Sections in the Settings menu", "General and Debugging are two of the sections Orchard Core provides."),
    ("3", "Sections of your own", "Custom Settings adds the sections that your site needs."),
]

SECTION = [
    ("1", "A content type", "It describes the fields of one kind of content."),
    ("2", "The CustomSettings stereotype", "A label on the type that marks it as a settings section."),
    ("3", "A section in the Settings menu", "The display name of the type is the name of the section."),
]

VALUES = [
    ("1", "Fields become the form", "The fields of the type are the fields you fill in under Settings."),
    ("2", "Saved with the site settings", "The values are stored with the site, not as content items in a list."),
    ("3", "One set of values per section", "A site holds a single copy of each section."),
]

CAUTION = [
    ("1", "A content type of its own", "Create a separate type for every section."),
    ("2", "Not a type with content items", "The documentation warns against marking such a type."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Site settings, in sections")
        bullets(image, SITE_SETTINGS)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "A section is a content type")
        bullets(image, SECTION)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "What a section holds")
        bullets(image, VALUES)
        return image

    if step_id == "idea-4":
        image = slide_base(chapter_title, label, "Before you create a section")
        bullets(image, CAUTION)
        return image

    raise SystemExit(f"No slide for {step_id}")

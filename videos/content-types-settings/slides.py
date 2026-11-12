"""The slides of the Content Type Settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

WHERE = [
    ("1", "Design, Content Definition", "Content Types lists every content type of the site."),
    ("2", "Edit", "Opens Edit Content Type, with all of the settings of the type."),
    ("check", "All five options on", "A type created in the admin starts with every option checked."),
]

OPTIONS = [
    ("check", "Creatable", "The type is offered when someone creates an item."),
    ("check", "Listable", "Its items appear in Manage Content, and the type in its filter."),
    ("check", "Draftable", "The editor of an item has a Save Draft button."),
    ("check", "Versionable", "An item can keep more than one version."),
]

MORE = [
    ("check", "Securable", "The type has its own permissions for roles."),
    ("check", "Stereotype", "Groups types for a feature, such as Widget."),
    ("check", "Category", "Groups types in a picker."),
    ("check", "Thumbnail", "An image shown with the type in a picker."),
]


def slide(step_id, chapter_title, label):
    if step_id == "options-1":
        image = slide_base(chapter_title, label, "Where the settings are")
        bullets(image, WHERE)
        return image

    if step_id == "options-2":
        image = slide_base(chapter_title, label, "What each option controls")
        bullets(image, OPTIONS)
        return image

    if step_id == "options-3":
        image = slide_base(chapter_title, label, "Permissions and grouping")
        bullets(image, MORE)
        return image

    raise SystemExit(f"No slide for {step_id}")

"""The slides of the Custom User Settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

DETAILS = [
    ("1", "Built-in details", "A user has a user name, an email address, and roles."),
    ("2", "Your own details", "A nickname, a department, a newsletter choice: whatever the site needs."),
    ("3", "Defined in the admin", "Content types and fields describe them."),
]

SECTION = [
    ("1", "A content type", "It holds the fields of the section, like any other content type."),
    ("2", "A stereotype", "The stereotype Custom User Settings marks the type as a section."),
    ("3", "A tab", "Each section is a tab on the Edit User page, named after the type."),
]

TASKS = [
    ("1", "Enable the feature", "Custom User Settings, in the Features list."),
    ("2", "Create a section", "A content type with fields and the stereotype."),
    ("3", "Fill it in for a user", "Open a user, and use the tab of the section."),
    ("4", "Share it", "Grant a permission to a role, and export the values."),
]


def slide(step_id, chapter_title, label):
    if step_id == "what-1":
        image = slide_base(chapter_title, label, "More information about each user")
        bullets(image, DETAILS)
        return image

    if step_id == "what-2":
        image = slide_base(chapter_title, label, "A section is a content type")
        bullets(image, SECTION)
        return image

    if step_id == "what-3":
        image = slide_base(chapter_title, label, "The tasks of this video")
        bullets(image, TASKS)
        return image

    raise SystemExit(f"No slide for {step_id}")

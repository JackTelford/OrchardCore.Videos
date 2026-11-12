"""The slides of the Users Change Email video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

BEFORE = [
    ("1", "Not editable by default", "On the edit page of an existing user, the Email field can't be edited."),
    ("2", "User Login setting", "Allow user to change their email address makes the field editable."),
]

FEATURE = [
    ("check", "User menu", "A Change Email item, shown while the setting is on."),
    ("check", "Change Email page", "One field, Email, and a Submit button."),
    ("check", "The signed-in user", "The page changes the address of the account that is signed in."),
    ("check", "One site setting", "It turns the item and the page on or off."),
]

STEPS = [
    ("1", "Enable the feature", "Users Change Email, under Tools, Features."),
    ("2", "Turn on the setting", "Settings, Security, Change Email."),
    ("3", "Users change their address", "Change Email in the user menu, then Submit."),
    ("4", "Deployment Plans", "Optional: the step Change Email settings exports the setting."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "The Email field of a user")
        bullets(image, BEFORE)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Users Change Email")
        bullets(image, FEATURE)
        return image

    if step_id == "summary-1":
        image = slide_base(chapter_title, label, "Change Email, start to finish")
        bullets(image, STEPS)
        return image

    raise SystemExit(f"No slide for {step_id}")

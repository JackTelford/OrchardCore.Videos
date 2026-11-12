"""The slides of the Users video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

WHAT_A_USER_IS = [
    ("1", "An account", "A user name, an email address, and a password."),
    ("2", "Roles", "A role groups permissions. A user can have several roles."),
    ("3", "Permissions", "The roles decide what a user can do on the site."),
]

WHERE_ACCOUNTS_COME_FROM = [
    ("1", "An administrator", "Creates accounts on the Users page."),
    ("2", "Visitors", "Sign themselves up when Users Registration is enabled."),
    ("3", "External Authentication", "Registers a user who signs in through an identity provider."),
]

PERMISSIONS = [
    ("check", "List all users", "Shows the Users page and its rows."),
    ("check", "Edit any user", "Creates and edits users, changes passwords, disables them."),
    ("check", "Delete any user", "Deletes accounts."),
    ("check", "Assign any role to users", "Chooses the roles of a user."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "What a user is")
        bullets(image, WHAT_A_USER_IS)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "Where accounts come from")
        bullets(image, WHERE_ACCOUNTS_COME_FROM)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "Permissions for managing users")
        bullets(image, PERMISSIONS)
        return image

    raise SystemExit(f"No slide for {step_id}")

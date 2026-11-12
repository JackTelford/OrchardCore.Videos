"""The slides of the User Login Settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PAGE = [
    ("1", "Settings, Security, User Login", "Open it from the Settings menu of the admin."),
    ("2", "One checkbox for each option", "They change the login page and which user details can be edited."),
    ("3", "More tabs with more features", "Two-factor authentication and external login add tabs of their own."),
]

GROUPS = [
    ("1", "Remember me", "Whether users can stay signed in, and the default choice."),
    ("2", "Login page theme", "Use the admin theme or the theme of the site."),
    ("3", "Local password login", "Keep or turn off the form for user names and passwords."),
    ("4", "User details", "Which fields of an existing user can be edited."),
]

LOCKOUT = [
    ("1", "Not an option of this page", "The limits are options of ASP.NET Core Identity."),
    ("2", "Set by a developer", "A developer sets the failed attempts and the length of the lock for the site."),
    ("3", "Handled in the Users list", "A Locked badge marks the account, and Unlock frees it."),
]


def slide(step_id, chapter_title, label):
    if step_id == "overview-1":
        image = slide_base(chapter_title, label, "The User Login page")
        bullets(image, PAGE)
        return image

    if step_id == "overview-2":
        image = slide_base(chapter_title, label, "Four groups of options")
        bullets(image, GROUPS)
        return image

    if step_id == "overview-3":
        image = slide_base(chapter_title, label, "Account lockout")
        bullets(image, LOCKOUT)
        return image

    raise SystemExit(f"No slide for {step_id}")

"""The slides of the Workflows user events video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

ACCOUNT_EVENTS = [
    ("1", "User Created Event", "An account is created, by an administrator or by a registration."),
    ("2", "User Updated Event", "An account is saved, for example when its roles change."),
    ("3", "User Confirmed Event", "Someone follows the link in a confirmation email."),
    ("4", "Enabled, Disabled and Deleted", "User Enabled Event, User Disabled Event and User Deleted Event."),
]

SESSION_EVENTS = [
    ("1", "User Loggedin Event", "Someone signs in, with a password or an external provider."),
    ("2", "User Loggedout Event", "Someone logs off."),
    ("i", "What the workflow receives", "The user name and the roles, and the provider of an external login."),
]

USER_TASKS = [
    ("1", "Assign User Role Task", "Adds a user to a role."),
    ("2", "Validate User Task", "Checks whether the visitor is signed in and in a role."),
    ("3", "Register User Task", "Creates an account from form fields. Needs the Email feature."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("events-1", "events-2"):
        image = slide_base(chapter_title, label, "Events about accounts")
        bullets(image, ACCOUNT_EVENTS, active={0, 1} if step_id == "events-1" else {2, 3})
        return image

    if step_id == "events-3":
        image = slide_base(chapter_title, label, "Events about sign-ins")
        bullets(image, SESSION_EVENTS)
        return image

    if step_id == "events-4":
        image = slide_base(chapter_title, label, "Tasks about users")
        bullets(image, USER_TASKS)
        return image

    raise SystemExit(f"No slide for {step_id}")

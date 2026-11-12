"""The slides of the two-factor authentication video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

TWO_STEPS = [
    ("1", "Username and password", "The user signs in as usual."),
    ("2", "A one-time code", "A second source, which only the user has, confirms who is signing in."),
]

METHODS = [
    ("1", "Authenticator app", "Codes from an app or browser extension on the user's own device."),
    ("2", "Email", "A code sent to the user's verified email address."),
    ("3", "SMS", "A text message with a code, sent to a verified phone number."),
]

ROLES = [
    ("1", "Administrator", "Turns the methods on and sets the rules for the site."),
    ("2", "Each user", "Sets up methods, and can turn two-factor authentication off unless the site requires it."),
]

TAB = [
    ("1", "Two-Factor Settings", "Remembered clients, requirements, recovery codes and theme."),
    ("2", "Email Authenticator Settings", "Email Subject and Email Body of the message with the code."),
    ("3", "Authenticator App Settings", "Show the email address or the user name in the app."),
    ("4", "SMS Authenticator Settings", "Message Body of the text message with the code."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "Two steps to sign in")
        bullets(image, TWO_STEPS)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "Three methods")
        bullets(image, METHODS)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "Who does what")
        bullets(image, ROLES)
        return image

    if step_id == "tab-1":
        image = slide_base(chapter_title, label, "The rest of the Two-Factor Authentication tab")
        bullets(image, TAB)
        return image

    raise SystemExit(f"No slide for {step_id}")

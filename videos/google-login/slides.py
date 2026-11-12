"""The slides of the Google sign-in video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

FLOW = [
    ("1", "A Google button", "The login page shows it next to the login form."),
    ("2", "Google checks the person", "The person signs in at Google and approves access."),
    ("3", "Back to your site", "Google sends the person back, and Orchard Core takes over."),
]

GOOGLE_PROVIDES = [
    ("1", "Client ID", "Defined in the Google API console for your application."),
    ("2", "Client secret", "Goes in the API secret key field."),
    ("3", "Authorized redirect URI", "Your site's address followed by the callback path."),
]

RETURN_TRIP = [
    ("1", "A linked Google Account", "The person is signed in as the linked user."),
    ("2", "The same email address", "The person enters that user's password to link the accounts."),
    ("3", "No matching user", "The person registers, unless external registration is disabled."),
]

RELATED = [
    ("1", "Permission", "Manage Google Authentication settings. The Administrator role has it."),
    ("2", "Secrets", "With the Secrets feature enabled, the API secret key can use a secret."),
    ("3", "User Login: External Login", "Can send people straight to Google when it is the only service."),
    ("4", "Registration: External Authentication", "Options for new users who register through an external service."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "Sign in with a Google Account")
        bullets(image, FLOW)
        return image
    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What Google provides")
        bullets(image, GOOGLE_PROVIDES)
        return image
    if step_id == "how-3":
        image = slide_base(chapter_title, label, "When Google sends the person back")
        bullets(image, RETURN_TRIP)
        return image
    if step_id == "related-1":
        image = slide_base(chapter_title, label, "Permission and secrets")
        bullets(image, RELATED, active={0, 1})
        return image
    if step_id == "related-2":
        image = slide_base(chapter_title, label, "Other sign-in settings")
        bullets(image, RELATED, active={2, 3})
        return image
    raise SystemExit(f"No slide for {step_id}")

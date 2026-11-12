"""The slides of the X (Twitter) video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

FEATURES = [
    ("1", "X (Twitter) Integration", "Stores the keys of your X app. Adds a workflow task that posts to X, with Workflows."),
    ("2", "Sign in with X (Twitter)", "Adds an X (Twitter) button to the login page."),
]

KEYS = [
    ("1", "API key", "Marked as required on the page."),
    ("2", "API secret key", "Stored encrypted in the settings."),
    ("3", "Access Token", "Marked as required on the page."),
    ("4", "Access Token Secret", "Stored encrypted in the settings."),
]

OUTCOMES = [
    ("1", "The X account is already linked", "The visitor is signed in, if that account may sign in."),
    ("2", "The email matches a local account", "Link your account asks for the password of that account."),
    ("3", "No account matches", "Create a new account opens, or the settings create the account without asking."),
    ("4", "New registrations are disabled", "The site reports: New registrations are disabled for this site."),
]

LATER = [
    ("1", "External Logins", "An item of the user menu of a signed-in user."),
    ("2", "Add another service to log in.", "The page lists X (Twitter) as a button to link."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Two features")
        bullets(image, FEATURES)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "From the Keys and tokens tab of your X app")
        bullets(image, KEYS)
        return image

    if step_id == "first-1":
        image = slide_base(chapter_title, label, "When X returns a visitor")
        bullets(image, OUTCOMES, active={0})
        return image

    if step_id == "first-2":
        image = slide_base(chapter_title, label, "When X returns a visitor")
        bullets(image, OUTCOMES, active={1, 2, 3})
        return image

    if step_id == "first-3":
        image = slide_base(chapter_title, label, "Add X to an existing account")
        bullets(image, LATER)
        return image

    raise SystemExit(f"No slide for {step_id}")

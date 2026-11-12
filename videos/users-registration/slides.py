"""The slides of the User Registration video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The video is
for administrators, so the slides hold labels and plain sentences only.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WITHOUT = [
    ("1", "Administrators add every account", "Each person is added in the Users list."),
    ("2", "Visitors cannot sign up", "The login page has no link to a registration form."),
]

WITH = [
    ("1", "A Register page", "Visitors enter a username, an email address and a password."),
    ("2", "A link on the login page", "Register as a new user."),
    ("3", "A settings page", "Settings, Security, Registration."),
]

PROVIDERS = [
    ("check", "Google Authentication", "Visitors register with a Google account."),
    ("check", "GitHub Authentication", "Visitors register with a GitHub account."),
    ("check", "Meta Login", "Visitors register with a Meta account."),
    ("check", "External Authentication tab", "Added to the Registration settings page."),
]

EXTERNAL_OPTIONS = [
    ("1", "Disable new external registration", "Only people who already have a local account can log in."),
    ("2", "Do not ask username", "The visitor does not choose a local username."),
    ("3", "Do not ask email address", "Used when the provider shares an email address."),
    ("4", "Do not create local password", "The account has no local password."),
]

SAME_RULES = [
    ("1", "Same options", "Email verification and approval apply to provider accounts."),
    ("2", "Own credentials", "Each provider needs an app registered in its developer portal."),
    ("3", "One page per provider", "Google, GitHub and Meta each have a page in the documentation."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Without registration")
        bullets(image, WITHOUT)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "With Users Registration")
        bullets(image, WITH)
        return image

    if step_id == "external-1":
        image = slide_base(chapter_title, label, "Registering with a login provider")
        bullets(image, PROVIDERS)
        return image

    if step_id == "external-2":
        image = slide_base(chapter_title, label, "External registration options")
        bullets(image, EXTERNAL_OPTIONS)
        return image

    if step_id == "external-3":
        image = slide_base(chapter_title, label, "Providers follow the same rules")
        bullets(image, SAME_RULES)
        return image

    raise SystemExit(f"No slide for {step_id}")

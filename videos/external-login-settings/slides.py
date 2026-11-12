"""The slides of the External Login video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

IDEA = [
    ("1", "A provider adds a button", "The login page lists each provider that is configured."),
    ("2", "The provider confirms identity", "The visitor proves who they are at the provider."),
    ("3", "The site keeps its own account", "A local user account is linked to the provider account."),
]

SHARED = [
    ("1", "Pages for a first sign-in", "Create an account, or link to an existing one."),
    ("2", "Two groups of settings", "External Login and External Authentication."),
    ("3", "Linked accounts", "A page lists the provider accounts linked to a user."),
    ("4", "Used by every provider", "Google, Microsoft, GitHub, Meta, X and OpenID Connect."),
]

FIRST = [
    ("1", "Already linked?", "The visitor is signed in to the linked local user."),
    ("2", "Same email address?", "A local user has the address the provider shares: the visitor enters that user's password."),
    ("3", "New registrations allowed?", "A local user is created. If not, the visitor is refused."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "Sign in with another service")
        bullets(image, IDEA)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What External Authentication provides")
        bullets(image, SHARED)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "Signing in with a provider")
        bullets(image, FIRST)
        return image

    raise SystemExit(f"No slide for {step_id}")

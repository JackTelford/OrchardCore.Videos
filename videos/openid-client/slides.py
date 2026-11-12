"""The slides of the OpenID Connect Client video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PARTIES = [
    ("1", "Identity provider", "A server that knows your users and signs them in."),
    ("2", "Your site", "Sends people to the provider, and trusts its answer."),
    ("3", "Local account", "Each person who signs in is linked to an account on your site."),
]

PROVIDER_GIVES = [
    ("1", "Authority", "The address of the provider."),
    ("2", "ClientId", "Identifies your site to the provider."),
    ("3", "Client Secret", "The password your site uses with the provider."),
    ("4", "CallbackPath", "The path on your site where the provider sends people back."),
]

WHAT_PEOPLE_SEE = [
    ("1", "The login page", "A button for the provider sits beside the usual form."),
    ("2", "The provider", "The person signs in there, and the provider sends them back."),
    ("3", "Your site", "The person is signed in to a local account."),
]

COMING_BACK = [
    ("1", "A linked account", "The site signs the person in."),
    ("2", "The same email address as a local account", "The person enters the local password once, to link the two."),
    ("3", "No matching account", "The site creates one, unless external registration is disabled."),
]

LOGIN_OPTIONS = [
    ("1", "Use external provider for login.", "With only one provider defined, the login page sends people straight to it."),
    ("2", "Disable local password login.", "Removes the password form. Keep an administrator with a linked account."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "Sign in with an identity provider")
        bullets(image, PARTIES, active={0, 1})
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What the provider gives you")
        bullets(image, PROVIDER_GIVES)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "What a person sees")
        bullets(image, WHAT_PEOPLE_SEE)
        return image

    if step_id == "how-4":
        image = slide_base(chapter_title, label, "When the person comes back")
        bullets(image, COMING_BACK)
        return image

    if step_id == "login-1":
        image = slide_base(chapter_title, label, "Settings, Security, User Login")
        bullets(image, LOGIN_OPTIONS)
        return image

    raise SystemExit(f"No slide for {step_id}")

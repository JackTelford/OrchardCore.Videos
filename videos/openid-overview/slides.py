"""The slides of the OpenID Connect overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PROBLEMS = [
    ("1", "Accounts everywhere", "Every application keeps its own accounts and passwords."),
    ("2", "Repeated sign-ins", "People sign in again in each application they use."),
    ("3", "Access in many places", "Changing who has access means visiting every application."),
]

PROVIDER = [
    ("1", "One place to sign in", "People sign in at the identity provider."),
    ("2", "Applications ask", "Each application sends people there to sign in."),
    ("3", "Tokens come back", "Tokens say who the person is and what is allowed."),
]

MODULE = [
    ("check", "Your Orchard Core site", "Becomes the identity provider for your applications."),
    ("check", "Your user accounts", "People sign in with their Orchard Core accounts."),
    ("check", "Standard protocols", "OpenID Connect and OAuth, so applications can use it."),
]

FLOW = [
    ("1", "The application asks", "It sends the person to Orchard Core to sign in."),
    ("2", "The person signs in", "Orchard Core shows the login page, and can ask for consent."),
    ("3", "The application gets tokens", "It receives a code, and exchanges it for tokens."),
    ("4", "The API checks the token", "The application sends the access token with its requests."),
]

ROLES = [
    ("1", "Identity provider", "Signs people in and issues tokens: Authorization Server."),
    ("2", "API", "Accepts the tokens it receives: Token Validation."),
    ("3", "Client", "Signs people in through another provider: Client Integration."),
]

PAGES = [
    ("1", "OpenID Connect Authorization Server", "Settings > OpenID Connect > Authorization Server"),
    ("2", "OpenID Connect Token Validation", "Settings > OpenID Connect > Token Validation"),
    ("3", "OpenID Connect Client Integration", "Settings > OpenID Connect > Authentication Client"),
    ("4", "OpenID Connect Management UI", "Access Control > OpenID Connect > Applications, Scopes"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without an identity provider")
        bullets(image, PROBLEMS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "With an identity provider")
        bullets(image, PROVIDER)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The OpenID Connect module")
        bullets(image, MODULE)
        return image

    if step_id.startswith("flow-") and step_id != "flow-5":
        index = int(step_id[-1]) - 1
        image = slide_base(chapter_title, label, "Signing in through the identity provider")
        bullets(image, FLOW, active={index})
        return image

    if step_id == "flow-5":
        image = slide_base(chapter_title, label, "Three roles for Orchard Core")
        bullets(image, ROLES)
        return image

    if step_id == "map-1":
        image = slide_base(chapter_title, label, "Where the pages are")
        bullets(image, PAGES)
        return image

    raise SystemExit("No slide for " + step_id)

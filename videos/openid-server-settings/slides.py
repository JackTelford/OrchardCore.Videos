"""The slides of the OpenID Connect server settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

CLIENT_AND_SERVER = [
    ("1", "A client application", "A website or a service that calls your APIs."),
    ("2", "Your Orchard Core site", "Runs the OpenID Connect Authorization Server."),
    ("3", "Tokens", "Issued to the application after it signs a user in, or on its own."),
]

THREE_GROUPS = [
    ("1", "Endpoints", "The addresses that applications call."),
    ("2", "Flows", "The ways an application can get a token. Each flow needs certain endpoints."),
    ("3", "Keys and token options", "Signing and encryption certificates, and options for the tokens."),
]

LIMITS = [
    ("1", "Server settings", "Allow the endpoints and the flows, for the whole site."),
    ("2", "Applications", "Each application can use only the flows that the server allows."),
    ("3", "Scopes", "Managed on their own page, next to the applications."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Orchard Core as an identity provider")
        bullets(image, CLIENT_AND_SERVER)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "One settings page, three groups")
        bullets(image, THREE_GROUPS)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "The server sets the limits")
        bullets(image, LIMITS)
        return image

    raise SystemExit(f"No slide for {step_id}")

"""The slides of the OpenID Connect Applications and Scopes video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHAT = [
    ("1", "Application", "A program that uses your site as its identity provider."),
    ("2", "Scope", "A named kind of access, such as Email or Roles."),
    ("3", "Allowed Scopes", "The scopes you choose for each application."),
]

WHERE = [
    ("check", "OpenID Connect Management UI", "The feature that adds the Applications and Scopes pages."),
    ("check", "OpenID Connect Authorization Server", "Depends on it, so enabling the server enables the pages."),
    ("check", "Access Control, OpenID Connect", "The menu section that holds Applications and Scopes."),
]


def slide(step_id, chapter_title, label):
    if step_id == "what-1":
        image = slide_base(chapter_title, label, "What you register")
        bullets(image, WHAT, active={0})
        return image

    if step_id == "what-2":
        image = slide_base(chapter_title, label, "What you register")
        bullets(image, WHAT, active={1, 2})
        return image

    if step_id == "what-3":
        image = slide_base(chapter_title, label, "Where they are managed")
        bullets(image, WHERE)
        return image

    raise SystemExit(f"No slide for {step_id}")

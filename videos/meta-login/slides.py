"""The slides of the Meta Login video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

FLOW = [
    ("1", "The visitor selects Facebook", "A button on your login page starts the sign-in."),
    ("2", "The visitor signs in at Meta", "The sign-in happens at Meta, outside your site."),
    ("3", "Meta sends the visitor back", "Your site finishes the sign-in."),
]

NEEDS = [
    ("1", "Application ID", "Shown under Basic Settings of your Meta app."),
    ("2", "Application secret", "Shown under Basic Settings of your Meta app."),
    ("3", "Meta Login product", "Enabled for web apps in your Meta app."),
    ("4", "Redirect URI", "Its path matches the callback path of your site: /signin-facebook by default."),
]

OUTCOMES = [
    ("1", "Already linked", "The linked user is signed in."),
    ("2", "Email matches a local user", "Link your account: the visitor enters the local password."),
    ("3", "Anyone else", "Registered as a user, unless external registrations are disabled."),
]

RELATED = [
    ("1", "Registration options", "Settings, Security, Registration"),
    ("2", "Login options", "Settings, Security, User Login"),
    ("3", "External Logins", "In the user menu of a signed-in user"),
]


def slide(step_id, chapter_title, label):
    if step_id == "need-1":
        image = slide_base(chapter_title, label, "How Meta Login works")
        bullets(image, FLOW)
        return image

    if step_id in ("need-2", "need-3"):
        image = slide_base(chapter_title, label, "What you need from Meta")
        bullets(image, NEEDS, active={0, 1} if step_id == "need-2" else {2, 3})
        return image

    if step_id in ("after-1", "after-2", "after-3"):
        image = slide_base(chapter_title, label, "What the site does next")
        bullets(image, OUTCOMES, active={int(step_id[-1]) - 1})
        return image

    if step_id == "after-4":
        image = slide_base(chapter_title, label, "Where to go from here")
        bullets(image, RELATED)
        return image

    raise SystemExit(f"No slide for {step_id}")

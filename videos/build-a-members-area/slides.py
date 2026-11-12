"""The slides of the "Build a Members Area" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

PARTS = [
    ("1", "Registration", "Visitors create their own accounts."),
    ("2", "Roles", "A role groups the people who share the same access."),
    ("3", "Permissions", "Permissions decide which content each role can open."),
]

WHO_CAN_VIEW = [
    ("1", "Anonymous", "Visitors who are not signed in. Its permissions apply to signed-in users too."),
    ("2", "Authenticated", "Every signed-in user."),
    ("3", "View all content", "Granted to both roles from the start, so every page is public."),
    ("4", "Member", "A role you create, and give only to approved people."),
]

STAGES = [
    ("1", "Register", "Enable Users Registration, and require approval."),
    ("2", "Secure", "Make Members Page securable, and create the Member role."),
    ("3", "Restrict", "Remove View all content from Anonymous and Authenticated."),
    ("4", "Try it", "Add a page, sign up, approve the account, and sign in."),
]

PROVIDERS = [
    ("1", "Google Authentication", "Settings, Security, Authentication, Google."),
    ("2", "GitHub Authentication", "Settings, Security, Authentication, GitHub."),
    ("3", "Meta Login", "Settings, Security, Authentication, Meta."),
]

EXTERNAL_REGISTRATION = [
    ("1", "The same Registration settings", "With Users Registration enabled, approval applies to provider accounts too."),
    ("2", "An External Authentication tab", "On Settings, Security, Registration, once a provider feature is enabled."),
    ("3", "Disable new external registration", "Only people who already have an account can sign in."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "The parts of a members area")
        bullets(image, PARTS)
        return image

    if step_id in ("idea-2", "idea-3"):
        image = slide_base(chapter_title, label, "Who can view content")
        bullets(image, WHO_CAN_VIEW, active={0, 1} if step_id == "idea-2" else {2, 3})
        return image

    if step_id == "idea-4":
        image = slide_base(chapter_title, label, "Four stages")
        bullets(image, STAGES)
        return image

    if step_id == "external-1":
        image = slide_base(chapter_title, label, "Sign in with another account")
        bullets(image, PROVIDERS)
        return image

    if step_id == "external-2":
        image = slide_base(chapter_title, label, "Registration settings for providers")
        bullets(image, EXTERNAL_REGISTRATION)
        return image

    raise SystemExit("No slide for " + step_id)

"""The slides of the Reset Password video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

FLOW = [
    ("1", "The user asks", "On the login page, Forgot your password? opens a short form."),
    ("2", "The site sends an email", "The email holds a link that works for a limited time."),
    ("3", "The user chooses a password", "The link opens a form, and the user can log in again."),
]

NEEDS = [
    ("1", "The Users Reset Password feature", "It depends on the Users and Email features."),
    ("2", "An email provider", "Such as SMTP Email Provider, set up under Settings, Communication, Email."),
    ("3", "A setting", "Allow the users to reset their password stays off until you select it."),
]

NOTES = [
    ("1", "One email per minute", "A user receives at most one reset email per minute."),
    ("2", "Sending failures", "If the email provider reports a failure, the user sees the confirmation page all the same."),
    ("3", "Verified email addresses", "When registration requires them, unverified users get no reset email."),
    ("4", "Rate Limits", "With the feature enabled, the two forms accept 20 submissions per minute per IP address."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "Recover a password without an administrator")
        bullets(image, FLOW)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What the site needs")
        bullets(image, NEEDS)
        return image

    if step_id == "note-1":
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, NOTES, active={0, 1})
        return image

    if step_id == "note-2":
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, NOTES, active={2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")

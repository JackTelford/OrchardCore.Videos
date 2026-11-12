"""The slides of the Azure Communication Services Email video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

# How an email reaches a user: the Email module, a provider, and the features that send email.
FIT = [
    ("1", "The Email module", "Sends the email of the site through its default provider."),
    ("2", "A provider", "The service that delivers a message, such as an SMTP server or Azure."),
    ("3", "Features that send email", "Account emails, workflow Email Tasks, and email notifications."),
]

# The two providers that the module adds.
PROVIDERS = [
    ("1", "Azure Communication Services", "Enabled and edited in the admin, in the email settings."),
    ("2", "Default Azure Communication Services", "Sender and connection string come from the server configuration."),
    ("3", "Listed only when it is set", "The admin cannot edit it, and it needs both values."),
]

# What the first provider needs.
NEEDS = [
    ("1", "An Azure resource", "An Azure Communication Services resource."),
    ("2", "A sender email address", "The address the messages come from."),
    ("3", "A connection string", "The connection string of that resource."),
]

# Good to know.
NOTES = [
    ("1", "Default provider", "Choose it on the Providers tab when more than one is enabled."),
    ("2", "Turning it off", "Clear Enable Azure Provider and save. The default setting is cleared too."),
    ("3", "When delivery fails", "A general error message, never the connection string. Details go to the log."),
    ("4", "Secrets", "With the Secrets feature enabled, Connection String can reference a secret."),
]


def slide(step_id, chapter_title, label):
    if step_id == "fit-1":
        image = slide_base(chapter_title, label, "Email goes through a provider")
        bullets(image, FIT, active={0, 1, 2})
        return image
    if step_id == "fit-2":
        image = slide_base(chapter_title, label, "Two providers")
        bullets(image, PROVIDERS, active={0, 1, 2})
        return image
    if step_id == "fit-3":
        image = slide_base(chapter_title, label, "What you need")
        bullets(image, NEEDS, active={0, 1, 2})
        return image
    if step_id == "notes-1":
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, NOTES, active={0})
        return image
    if step_id == "notes-2":
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, NOTES, active={1})
        return image
    if step_id == "notes-3":
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, NOTES, active={2})
        return image
    if step_id == "notes-4":
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, NOTES, active={3})
        return image
    raise SystemExit(f"No slide for {step_id}")

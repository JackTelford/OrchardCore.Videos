"""The slides of the SMS settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PROVIDERS = [
    ("1", "Log", "Writes each message to the application log. Available in a development environment."),
    ("2", "Twilio", "Sends messages through a Twilio account."),
    ("3", "Azure Communication Services", "Sends messages through an Azure resource. It has a feature of its own."),
]

DEFAULT_USERS = [
    ("1", "Workflow tasks", "The workflow task that sends a text message uses the default provider."),
    ("2", "Notifications", "SMS Notifications go through the default provider."),
    ("3", "Two-factor sign-in", "The Two-Factor SMS Method sends its codes through it too."),
]

PLACES = [
    ("1", "Settings, Communication, SMS", "Choose the default provider, and set up Twilio or Azure Communication Services."),
    ("2", "Tools, Testing, SMS Test", "Send a test message through any provider that is enabled."),
    ("3", "Manage SMS Settings", "Both pages need it. The settings page also needs Manage settings. Administrators have both."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "SMS providers")
        bullets(image, PROVIDERS)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "The default provider sends the site's messages")
        bullets(image, DEFAULT_USERS)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "Where to find it")
        bullets(image, PLACES)
        return image

    raise SystemExit(f"No slide for {step_id}")

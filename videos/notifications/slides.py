"""The slides of the Notifications video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

HOW = [
    ("1", "Notification center", "Every notification is stored for the user it is meant for."),
    ("2", "Notification methods", "Email and SMS send a copy, as each user chooses."),
    ("3", "Senders", "Workflows send notifications without code. A module can send them too."),
]

DELIVERY = [
    ("1", "Email Notifications", "Uses the Email service. Sends the Text Body, or the HTML Body when preferred."),
    ("2", "SMS Notifications", "Uses the SMS service. Sends the Text Body only."),
    ("3", "Notification center", "Always keeps the notification, even when email or SMS fails."),
    ("4", "Workflow outcome", "Done when an email or SMS was sent. Failed when none was. Failed: no user found."),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        index = int(step_id[-1]) - 1
        image = slide_base(chapter_title, label, "How notifications reach users")
        bullets(image, HOW, active={index})
        return image

    if step_id == "deliver-1":
        image = slide_base(chapter_title, label, "What each method sends and needs")
        bullets(image, DELIVERY, active={0})
        return image

    if step_id == "deliver-2":
        image = slide_base(chapter_title, label, "What each method sends and needs")
        bullets(image, DELIVERY, active={1})
        return image

    if step_id == "deliver-3":
        image = slide_base(chapter_title, label, "What each method sends and needs")
        bullets(image, DELIVERY, active={2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")

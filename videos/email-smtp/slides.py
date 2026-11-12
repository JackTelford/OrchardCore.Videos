"""The slides of the Email: Configure SMTP and Send a Test Email video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

SENDERS = [
    ("1", "User accounts", "Address confirmation, password reset and verification codes."),
    ("2", "Notifications", "Email Notifications deliver messages to the email address of a user."),
    ("3", "Workflows", "The Email Task activity sends a message from a workflow."),
]

FEATURES = [
    ("check", "Email", "Settings, the Default provider and the Email Test page."),
    ("check", "SMTP Email Provider", "Sends the messages through an SMTP server."),
    ("check", "Azure Communication Services Email", "Another provider, covered in a separate video."),
]

PERMISSION = [
    ("check", "Manage Email Settings", "Opens the Email settings and the Email Test page."),
    ("check", "Administrator role", "Has the permission by default."),
]

PLAN = [
    ("1", "Enable the provider", "Tools, Features: SMTP Email Provider."),
    ("2", "Configure SMTP", "Settings, Communication, Email."),
    ("3", "Send a test email", "Tools, Testing, Email Test."),
    ("4", "Check the message", "In a local test mail server, smtp4dev."),
]

FAILURES = [
    ("1", "Read the error", "The Email Test page lists why the message was not sent."),
    ("2", "Check the server", "Host name, Port number, Encryption method to use and credentials."),
    ("3", "Check the addresses", "Sender, To, Cc, Bcc and Reply To must be valid email addresses."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Where a site sends email")
        bullets(image, SENDERS)
        return image

    if step_id == "parts-1":
        image = slide_base(chapter_title, label, "A feature and its providers")
        bullets(image, FEATURES)
        return image

    if step_id == "parts-2":
        image = slide_base(chapter_title, label, "Who can use it")
        bullets(image, PERMISSION)
        return image

    if step_id == "plan-1":
        image = slide_base(chapter_title, label, "What we will do")
        bullets(image, PLAN)
        return image

    if step_id == "fail-1":
        image = slide_base(chapter_title, label, "When the test fails")
        bullets(image, FAILURES)
        return image

    raise SystemExit(f"No slide for {step_id}")

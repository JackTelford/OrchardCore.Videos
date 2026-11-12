"""The slides of the Microsoft Account sign-in video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

ROUND_TRIP = [
    ("1", "The visitor chooses Microsoft", "A button on your login page starts the sign-in."),
    ("2", "Microsoft signs the visitor in", "The visitor signs in on a Microsoft page."),
    ("3", "The visitor returns to your site", "Orchard Core receives the result at the callback path."),
]

AT_MICROSOFT = [
    ("1", "Application ID", "Identifies your app. It goes in the AppId field."),
    ("2", "Secret key", "Created for the app. It goes in the Secret Key field."),
    ("3", "Callback path", "Where visitors return. The default is /signin-microsoft."),
]

ACCOUNT_TYPES = [
    ("check", "Microsoft Account", "This video: sign in with a Microsoft account."),
    ("2", "Microsoft Entra ID", "For work or school accounts as well: a multi-tenant app."),
]

OUTCOMES = [
    ("1", "A linked local user exists", "The visitor is signed in."),
    ("2", "A local user has the same email", "Link your account: enter that user's local password."),
    ("3", "No matching local user", "Create a new account: Username, Email, Password and Confirm password."),
    ("4", "External registration settings", "They can turn new registrations off, or skip some fields."),
]

LINK_LATER = [
    ("1", "External Logins", "A link in the user menu of a signed-in user."),
    ("2", "Add another service to log in", "Choose Microsoft Account to link it."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "The sign-in round trip")
        bullets(image, ROUND_TRIP)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What you create at Microsoft")
        bullets(image, AT_MICROSOFT)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "Work or school accounts too?")
        bullets(image, ACCOUNT_TYPES)
        return image

    if step_id in ("return-1", "return-2", "return-3"):
        image = slide_base(chapter_title, label, "When Microsoft sends the visitor back")
        active = {"return-1": {0}, "return-2": {1}, "return-3": {2, 3}}[step_id]
        bullets(image, OUTCOMES, active=active)
        return image

    if step_id == "return-4":
        image = slide_base(chapter_title, label, "Link a Microsoft account later")
        bullets(image, LINK_LATER)
        return image

    raise SystemExit(f"No slide for {step_id}")

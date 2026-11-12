"""The slides of the Microsoft Entra ID sign-in video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

HOW = [
    ("1", "Select the button", "The login page shows a button named after your organization."),
    ("2", "Sign in at Microsoft", "The person enters their credentials on the Microsoft page."),
    ("3", "Return to your site", "Microsoft sends the browser back to the callback path of your site."),
    ("4", "Match the person", "Your site finds, links or registers the user for that account."),
]

REGISTER = [
    ("1", "New registration", "Name the app, choose the account types, add a redirect URI."),
    ("2", "Copy two IDs", "The Application (client) ID and the Directory (tenant) ID."),
    ("3", "Authentication", "Enable Access tokens and ID tokens for implicit and hybrid flows."),
    ("4", "Token configuration", "Add the optional email claim to the ID token."),
]

WHO = [
    ("1", "Directory (tenant) ID", "Only the user and guest accounts of your own directory."),
    ("2", "organizations", "Work or school accounts from any Microsoft Entra directory."),
    ("3", "common", "Work, school and personal Microsoft accounts."),
]

MATCH = [
    ("1", "A linked user exists", "The person is signed in as that user."),
    ("2", "A user has the same email", "The person enters that user's local password, and the accounts are linked."),
    ("3", "No user matches", "A local account is created, unless new external registrations are disabled."),
    ("4", "Link later", "Signed-in users add the login from External Logins in the user menu."),
]

OPTIONS = [
    ("1", "Use external provider for login.", "Settings, Security, User Login"),
    ("2", "Disable local password login.", "Settings, Security, User Login"),
    ("3", "Disable new external registration", "Settings, Security, Registration"),
]

ROLES = [
    ("1", "Groups in Entra ID", "A security group for each Orchard Core role."),
    ("2", "The groups claim", "Added under Token configuration in the app registration."),
    ("3", "A User Login option", "A script turns the groups into Orchard Core roles."),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Signing in with Microsoft Entra ID")
        bullets(image, HOW, active={0, 1} if step_id == "how-1" else {2, 3})
        return image

    if step_id.startswith("register-"):
        image = slide_base(chapter_title, label, "The app registration, in the Azure portal")
        bullets(image, REGISTER, active={int(step_id[-1]) - 1})
        return image

    if step_id.startswith("who-"):
        image = slide_base(chapter_title, label, "The tenant ID decides who can sign in")
        bullets(image, WHO, active={0} if step_id == "who-1" else {1, 2})
        return image

    if step_id == "match-5":
        image = slide_base(chapter_title, label, "Options for every external provider")
        bullets(image, OPTIONS)
        return image

    if step_id.startswith("match-"):
        image = slide_base(chapter_title, label, "When Microsoft sends the person back")
        bullets(image, MATCH, active={int(step_id[-1]) - 1})
        return image

    if step_id.startswith("roles-"):
        image = slide_base(chapter_title, label, "Roles from Entra ID groups")
        bullets(image, ROLES, active={0, 1} if step_id == "roles-1" else {2})
        return image

    raise SystemExit(f"No slide for {step_id}")

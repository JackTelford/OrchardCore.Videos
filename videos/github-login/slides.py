"""The slides of the GitHub sign-in video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

HOW = [
    ("1", "The visitor chooses GitHub", "A button on the login page of your site"),
    ("2", "The person signs in at GitHub", "The browser goes to GitHub, which confirms who the person is"),
    ("3", "GitHub returns to your site", "The browser comes back to the callback path of your site"),
]

NEEDS = [
    ("1", "Client ID", "The ID of your app on GitHub"),
    ("2", "Client secret", "The secret key of your app on GitHub"),
    ("3", "Callback path", "Where GitHub returns the person: /signin-github when left empty"),
]

REGISTER = [
    ("1", "Open the GitHub Developer applications", "The place where you register apps on GitHub"),
    ("2", "Create an OAuth app", "The app stands for your Orchard Core site"),
    ("3", "Set the Authorization Callback URL", "The address of your site, then /signin-github: https://example.com/signin-github"),
    ("4", "Take the Client ID and the client secret", "You enter both in Orchard Core"),
]

RETURN = [
    ("1", "A linked user is signed in", "The site finds the user linked to that GitHub account"),
    ("2", "A matching email address asks for a password", "The person links GitHub to the local user with that user's password"),
    ("3", "A person without an account registers", "A form, an automatic registration, or a notice that registrations are disabled"),
    ("4", "A signed-in user can add GitHub", "From External Logins in the user menu"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("how-"):
        if step_id == "how-1":
            image = slide_base(chapter_title, label, "Signing in with GitHub")
            bullets(image, HOW)
        else:
            image = slide_base(chapter_title, label, "What your site needs")
            bullets(image, NEEDS)
        return image

    if step_id.startswith("register-"):
        image = slide_base(chapter_title, label, "Register the app on GitHub")
        bullets(image, REGISTER, active={0, 1, 2} if step_id == "register-1" else {3})
        return image

    if step_id.startswith("return-"):
        active = {"return-1": {0, 1}, "return-2": {2}, "return-3": {3}}[step_id]
        image = slide_base(chapter_title, label, "When GitHub sends the person back")
        bullets(image, RETURN, active=active)
        return image

    raise SystemExit(f"No slide for {step_id}")

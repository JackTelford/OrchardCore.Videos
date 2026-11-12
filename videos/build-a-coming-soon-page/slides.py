"""The slides of the "Launch a Coming Soon Page While You Build Your Site" video.

assemble.py calls slide() for each step of a "slide" clip. Everything visual comes from brand.py, so the slides look
like those of every other Orchard Core video. This is an admin video: the slides name screens and labels, nothing else.
"""
from brand import bullets, slide_base

# Why a Coming Soon page.
PROBLEMS = [
    ("1", "The site has an address", "Visitors can arrive before the site is ready."),
    ("2", "Nothing useful to see", "An unfinished site gives an early visitor no reason to return."),
    ("3", "No way to follow up", "There is nowhere to leave an email address."),
]

SOLUTIONS = [
    ("check", "A title and a message", "The message gives a target launch date."),
    ("check", "A field for an email address", "Visitors leave an address to hear from you."),
    ("check", "Social icons", "Links to your pages on other sites."),
    ("check", "The Coming Soon Theme", "A background video behind the page."),
]

# What a sign-up does.
SIGN_UP = [
    ("1", "The visitor selects the button", "The form sends the email address to a workflow."),
    ("2", "The workflow checks the sign-up", "It checks that an address was entered and runs the reCaptcha check."),
    ("3", "It creates a user", "The user has the email address of the visitor."),
    ("4", "It sends a confirmation email", "Then the visitor returns to the home page."),
]

# Where to set it up.
WHERE = [
    ("1", "reCaptcha keys", "Settings, Security, reCaptcha"),
    ("2", "Email provider", "Tools, Features, then Settings, Communication, Email"),
    ("3", "Sign-ups", "Access Control, Users"),
]

# Launching the site.
LAUNCH = [
    ("1", "Open the content item", "The one that is to be your home page. It needs a permalink."),
    ("2", "Check Set as homepage", "The option is under Permalink."),
    ("3", "Publish it", "The item becomes the home page of the site."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A site under construction")
        bullets(image, PROBLEMS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The Coming Soon recipe")
        bullets(image, SOLUTIONS)
        return image

    if step_id == "signup-1":
        image = slide_base(chapter_title, label, "What a sign-up does")
        bullets(image, SIGN_UP, active={0})
        return image

    if step_id == "signup-2":
        image = slide_base(chapter_title, label, "What a sign-up does")
        bullets(image, SIGN_UP, active={1, 2, 3})
        return image

    if step_id == "signup-4":
        image = slide_base(chapter_title, label, "Where to set it up")
        bullets(image, WHERE)
        return image

    if step_id == "launch-1":
        image = slide_base(chapter_title, label, "Make another page the home page")
        bullets(image, LAUNCH)
        return image

    raise SystemExit("No slide for " + step_id)

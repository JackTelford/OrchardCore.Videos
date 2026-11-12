"""The slides of the reCAPTCHA video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

BOTS = [
    ("1", "Sign-in pages", "Programs try password after password."),
    ("2", "Password pages", "Programs request reset emails again and again."),
    ("3", "Forms", "Programs send spam through contact forms."),
]

CHECK = [
    ("1", "The visitor ticks the checkbox", "The page sends the answer with the form."),
    ("2", "Your site asks Google", "It sends the answer together with your Secret Key."),
    ("3", "Google replies", "The answer is confirmed or rejected."),
]

FEATURES = [
    ("check", "ReCaptcha", "Your keys, a widget for Forms and a task for Workflows."),
    ("check", "ReCaptcha Users", "The checkbox on the Log in, registration and password pages."),
]

FORM_FLOW = [
    ("1", "Form widget", "Its Action field holds the address of the workflow."),
    ("2", "Http Request Event", "The workflow starts when the form is submitted."),
    ("3", "Validate ReCaptcha Task", "It checks the answer of the visitor."),
]

OUTCOMES = [
    ("check", "Valid", "Connect it to the steps that handle the form."),
    ("check", "Invalid", "Connect it to an Http Redirect Task back to the page."),
    ("check", "Done", "Returned together with Valid or Invalid."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Bots send requests on their own")
        bullets(image, BOTS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "How the check works")
        bullets(image, CHECK)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Two features in Orchard Core")
        bullets(image, FEATURES)
        return image

    if step_id == "workflow-1":
        image = slide_base(chapter_title, label, "From the form to a workflow")
        bullets(image, FORM_FLOW, active={0, 1})
        return image

    if step_id == "workflow-2":
        image = slide_base(chapter_title, label, "The outcomes of Validate ReCaptcha")
        bullets(image, OUTCOMES)
        return image

    raise SystemExit(f"No slide for {step_id}")

"""The slides of the Features dependencies video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

NEEDS = [
    ("1", "Users Registration", "Lets visitors sign up and confirm their email address."),
    ("2", "Email and Users", "The features that Users Registration needs: its dependencies."),
]

RULES = [
    ("+", "Enable a feature", "Its dependencies are enabled with it."),
    ("-", "Disable a feature", "The features that depend on it are disabled with it."),
]

LIMITS = [
    ("1", "On demand", "Enabled while another feature needs it, disabled when none does."),
    ("2", "Always enabled", "Stays enabled, and its Disable button is inactive."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A feature can need other features")
        bullets(image, NEEDS)
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Both sides stay consistent")
        bullets(image, RULES)
        return image
    if step_id == "sum-1":
        image = slide_base(chapter_title, label, "Two cases to know")
        bullets(image, LIMITS)
        return image
    raise SystemExit(f"No slide for {step_id}")
